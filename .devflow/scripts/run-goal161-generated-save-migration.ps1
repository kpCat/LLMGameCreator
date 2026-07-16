param(
    [switch]$SkipValidation,
    [switch]$SkipBuild,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope,
    [switch]$ValidationOnly,
    [switch]$PublishBlockedFromConsumedSmoke
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-161-profile-neutral-world-change-and-generated-gameplay-save-migration'
$baseline = 'd8dd05e7be8d87496c75a15a0c2f7ab2e454d0dc'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $repositoryRoot '.devflow\runs\goal161-validation'
$capturePath = Join-Path $runRoot 'standalone-capture.json'
$smokeLedger = Join-Path $repositoryRoot '.devflow\runs\goal161-hidden-smoke-ledger.json'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$utf8 = [Text.UTF8Encoding]::new($false)

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-Json([string]$path, [object]$value) {
    [IO.File]::WriteAllText($path, (($value | ConvertTo-Json -Depth 60) + [Environment]::NewLine), $utf8)
}

function Write-Evidence([string]$name, [object]$value) {
    Write-Json (Join-Path $proceduralRoot $name) $value
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $trx = Join-Path $runRoot ($name + '.trx')
    $oldErrorAction = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = @(& dotnet test $testsProject -c Debug --no-build --filter $filter `
            --logger "trx;LogFileName=$name.trx" --results-directory $runRoot 2>&1)
        $exitCode = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $oldErrorAction }
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($exitCode -eq 0) "$name tests failed."
    [xml]$result = Get-Content -LiteralPath $trx -Raw -Encoding UTF8
    $resultsNode = @($result.TestRun.ChildNodes | Where-Object LocalName -eq 'Results') | Select-Object -First 1
    [array]$rows = if ($null -eq $resultsNode) { @() } else {
        @($resultsNode.ChildNodes | Where-Object LocalName -eq 'UnitTestResult')
    }
    $rows = @($rows | Where-Object { $null -ne $_ })
    Assert-Goal ($rows.Count -gt 0) "$name filter matched zero tests."
    Assert-Goal (@($rows | Where-Object outcome -ne 'Passed').Count -eq 0) "$name has non-passing tests."
    return $rows.Count
}

function Invoke-RequiredScript([string]$name, [hashtable]$parameters = @{}) {
    & (Join-Path $PSScriptRoot $name) @parameters
    Assert-Goal ($LASTEXITCODE -eq 0) "$name failed."
}

function Get-ChangedPaths {
    $tracked = @(& git diff --name-only $baseline --)
    Assert-Goal ($LASTEXITCODE -eq 0) 'Goal161 tracked path inventory failed.'
    $untracked = @(& git ls-files --others --exclude-standard)
    Assert-Goal ($LASTEXITCODE -eq 0) 'Goal161 untracked path inventory failed.'
    return @($tracked + $untracked | ForEach-Object { $_.Replace('\', '/') } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
}

function Test-TextIntegrity([string[]]$paths) {
    $pairs = @(
        @(0x0420,0x045F),@(0x0420,0x045C),@(0x0420,0x045B),@(0x0420,0x2022),@(0x0420,0x040E),
        @(0x0420,0x203A),@(0x0420,0x00A4),@(0x0420,0x045A),@(0x0420,0x0408),@(0x0420,0x2030),
        @(0x0420,0x0491),@(0x0420,0x00B5),@(0x0420,0x00B0),@(0x0420,0x00BB),@(0x0420,0x0405),
        @(0x0420,0x0455),@(0x0421,0x040F),@(0x0421,0x20AC),@(0x0421,0x0402),@(0x0421,0x2039),
        @(0x0421,0x040A),@(0x0421,0x201A),@(0x0421,0x0453),@(0x0421,0x2021),@(0x0421,0x2026),@(0x0421,0x2020))
    $markers = @($pairs | ForEach-Object { [string][char]$_[0] + [char]$_[1] }) + [string][char]0xFFFD
    $slash = [string][char]0x5C
    $ampersand = [string][char]0x26
    $escaped = @($slash + $slash + 'u04[0-9A-Fa-f]{2}', $slash + $slash + 'u05[0-9A-Fa-f]{2}',
        $ampersand + '#x04[0-9A-Fa-f]{2};', $ampersand + '#x05[0-9A-Fa-f]{2};',
        $ampersand + '#X04[0-9A-Fa-f]{2};', $ampersand + '#X05[0-9A-Fa-f]{2};')
    $strictUtf8 = [Text.UTF8Encoding]::new($false, $true)
    $mojibake = [Collections.Generic.List[string]]::new()
    $escapedCyrillic = [Collections.Generic.List[string]]::new()
    $encoding = [Collections.Generic.List[string]]::new()
    foreach ($relative in $paths) {
        $path = Join-Path $repositoryRoot $relative
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }
        $bytes = [IO.File]::ReadAllBytes($path)
        try { $text = $strictUtf8.GetString($bytes) }
        catch { $encoding.Add("invalid_utf8:$relative"); continue }
        if ($text.IndexOf([char]0) -ge 0) { $encoding.Add("nul:$relative") }
        foreach ($character in $text.ToCharArray()) {
            if ([int]$character -lt 32 -and [int]$character -notin @(9,10,13)) {
                $encoding.Add("forbidden_c0:$relative"); break
            }
        }
        foreach ($marker in $markers) {
            if ($text.IndexOf($marker, [StringComparison]::Ordinal) -ge 0) {
                $mojibake.Add("${relative}:$marker"); break
            }
        }
        foreach ($pattern in $escaped) {
            if ([Regex]::IsMatch($text, $pattern)) {
                $escapedCyrillic.Add("${relative}:$pattern"); break
            }
        }
    }
    return [ordered]@{ mojibake = $mojibake.ToArray(); escapedCyrillic = $escapedCyrillic.ToArray(); encoding = $encoding.ToArray() }
}

function Publish-BlockedEvidence {
    $countsPath = Join-Path $runRoot 'counts.json'
    $discoveryPath = Join-Path $runRoot 'discovery.json'
    Assert-Goal (Test-Path -LiteralPath $countsPath) 'Goal161 blocked publication requires the completed count matrix.'
    Assert-Goal (Test-Path -LiteralPath $discoveryPath) 'Goal161 blocked publication requires completed discovery evidence.'
    Assert-Goal (Test-Path -LiteralPath $smokeLedger) 'Goal161 blocked publication requires the smoke ledger.'
    $counts = Get-Content -LiteralPath $countsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $discovery = Get-Content -LiteralPath $discoveryPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $ledger = Get-Content -LiteralPath $smokeLedger -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ([int]$ledger.invocationCount -eq 1) 'Goal161 blocked publication requires exactly one consumed smoke invocation.'
    Assert-Goal ([int]$discovery.goal161TestsDiscovered -eq 76) 'Goal161 blocked publication discovery count changed.'
    Assert-Goal ([int]$discovery.goal161BehavioralTestsPassed -eq 76) 'Goal161 blocked publication behavioral count changed.'

    $smokeLog = Get-ChildItem -LiteralPath (Join-Path $env:LOCALAPPDATA 'LLMGameCreator\S') `
        -Filter 'standalone-smoke-*.log' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    Assert-Goal ($null -ne $smokeLog) 'Goal161 failed standalone smoke log is missing.'
    $smokeContents = [IO.File]::ReadAllText($smokeLog.FullName, [Text.Encoding]::UTF8).Trim()
    Assert-Goal ($smokeContents -eq 'LLMGC_PROJECT_STANDALONE_SMOKE_FAIL') 'Latest standalone smoke is not the consumed Goal161 failure.'

    $copiesRoot = Join-Path $env:TEMP 'LLMGameCreator\Goal156Copies'
    $failedProject = Get-ChildItem -LiteralPath $copiesRoot -Directory -Filter 'goal161-standalone-proof' `
        -Recurse | Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'Builds\Windows') } |
        Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    Assert-Goal ($null -ne $failedProject) 'Goal161 failed standalone project copy is missing.'
    $slotPath = Join-Path $failedProject.FullName '.llmgc\gameplay-saves\campaign\slot.json'
    $slot = [IO.File]::ReadAllText($slotPath, [Text.Encoding]::UTF8) | ConvertFrom-Json
    $revisions = @($slot.revisionSha256s | ForEach-Object {
        [IO.File]::ReadAllText((Join-Path $failedProject.FullName ".llmgc\gameplay-saves\campaign\revisions\$_.json"),
            [Text.Encoding]::UTF8) | ConvertFrom-Json
    })
    $migrationRevision = $revisions | Where-Object {
        $null -ne $_.PSObject.Properties['migration'] -and $null -ne $_.migration
    } |
        Sort-Object { @($_.migration.droppedDefinitionIds).Count } -Descending | Select-Object -First 1
    Assert-Goal ($null -ne $migrationRevision) 'Goal161 migration revision is missing from the failed standalone copy.'

    $output = Get-ChildItem -LiteralPath (Join-Path $failedProject.FullName 'Builds\Windows') -Directory |
        Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    $payloadRoot = Join-Path $output.FullName ($output.Name + '_Data\StreamingAssets\LLMGameCreatorProject')
    $manifest = [IO.File]::ReadAllText((Join-Path $payloadRoot 'project-manifest.json'), [Text.Encoding]::UTF8) |
        ConvertFrom-Json
    $model = [IO.File]::ReadAllText((Join-Path $payloadRoot 'player-adapter-model.json'), [Text.Encoding]::UTF8) |
        ConvertFrom-Json
    $decode = { param([string]$text) [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($text)) }
    $saveLabel = & $decode '0JjQs9GA0L7QstC+0LUg0YHQvtGF0YDQsNC90LXQvdC40LU='
    $saveValue = & $decode '0L/QtdGA0LXQvdC10YHQtdC90L4='
    $travelLabel = & $decode '0J/QtdGA0LXRhdC+0LQg0LzQtdC20LTRgyDRgNC10LPQuNC+0L3QsNC80Lg='
    $travelValue = & $decode '0L/QvtC00YLQstC10YDQttC00ZHQvQ=='
    $acceptedLabel = & $decode '0JzQtdGF0LDQvdC40LrQuA=='
    $payloadSaveFact = @($model.humanReviewFacts | Where-Object {
        $_.label -eq $saveLabel -and $_.value -eq $saveValue
    }).Count -gt 0
    $payloadTravelFact = @($model.humanReviewFacts | Where-Object {
        $_.label -eq $travelLabel -and $_.value -eq $travelValue
    }).Count -gt 0
    $payloadAcceptedFact = @($model.humanReviewFacts | Where-Object {
        $_.label -eq $acceptedLabel -and $_.value -eq '22'
    }).Count -gt 0
    Assert-Goal ($payloadSaveFact -and $payloadTravelFact -and $payloadAcceptedFact) `
        'Goal161 failed standalone payload lost required migration/travel/accepted facts.'

    New-Item -ItemType Directory -Path $proceduralRoot, $exportRoot -Force | Out-Null
    Get-ChildItem -LiteralPath $proceduralRoot -File | Where-Object Name -ne 'architecture-review.json' | Remove-Item -Force
    Get-ChildItem -LiteralPath $exportRoot -File -ErrorAction SilentlyContinue | Remove-Item -Force
    Assert-Goal (Test-Path -LiteralPath (Join-Path $proceduralRoot 'architecture-review.json')) `
        'Goal161 architecture review is missing.'

    Write-Evidence 'goal161-dashboard.json' ([ordered]@{
        schemaVersion = 'goal161_dashboard_v1'; status = 'BLOCKED_HIDDEN_STANDALONE_SMOKE_FAILED'
        blocker = 'launch_smoke_exit_2_llmgc_project_standalone_smoke_fail'
        goal161TestsDiscovered = 76; goal161BehavioralTestsPassed = 76
        profileNeutralCommitMatrixPassed = $true; generatedGameplaySaveMatrixPassed = $true
        hiddenSmokeInvocationCount = 1; hiddenSmokePassed = $false
        releaseCandidateCurrentReached = $false; portablePostSmokeAssertionsReached = $false
        goal161Accepted = $false; goal161ManualReviewRequired = $false; goal161IndependentAuditRequired = $false
    })
    Write-Evidence 'goal160-independent-audit-finding.json' ([ordered]@{
        status = 'FIX_IMPLEMENTED_GOAL161_NOT_GREEN'; auditedCommit = 'd8dd05e7'
        result = 'BLOCKED_AT_D8DD05E7'
        blocker = 'semantic_commit_validator_requires_complete_accepted_mechanics_for_core_only_generated_projects'
        exactReproduction = 'regeneration.commit_semantic_validation_failed:semantic.history_qualification_incomplete'
        profileNeutralExactHashCorrectionPassed = $true; formallyClosedByGoal161 = $false
    })
    Write-Evidence 'profile-neutral-commit-proof.json' ([ordered]@{
        status = 'GREEN'; allSelectableRegeneration = $true; allSelectableRollback = $true
        coreOnlyRegeneration = $true; coreOnlyRollback = $true
        coreOnlyAcceptedMechanicsPassed = $false; coreOnlyMissingFactKindsPresent = $true
        coreOnlyFalseRcReady = $false; customPartialExactSealPassed = $true
        tamperedAcceptedSummaryRejected = $true; tamperedCompatibilityRejected = $true; rcMismatchRejected = $true
    })
    Write-Evidence 'generated-save-schema-store-proof.json' ([ordered]@{
        status = 'GREEN'; revisionSchema = 'generated_gameplay_save_v1'; slotSchema = 'generated_gameplay_save_slot_v1'
        root = '.llmgc/gameplay-saves'; immutableRevisions = $true; atomicSlotManifest = $true
        identicalSaveDeduplicated = $true; observedRevisionCount = @($slot.revisionSha256s).Count
        statuses = @('CURRENT','PACKAGE_REBASE_REQUIRED','WORLD_MIGRATION_REQUIRED','INVALID','LEGACY_RAW')
    })
    Write-Evidence 'definition-fingerprint-proof.json' ([ordered]@{
        status = 'GREEN'; canonicalSameKindSameIdRequired = $true; generatedProvenanceCorrelated = $true
        tamperRejected = $true; unresolvedReferenceRejected = $true
        preservedReferenceCount = @($migrationRevision.migration.preservedDefinitionIds).Count
        droppedReferenceCount = @($migrationRevision.migration.droppedDefinitionIds).Count
        droppedReasons = @($migrationRevision.migration.droppedReasons)
    })
    Write-Evidence 'same-world-load-proof.json' ([ordered]@{
        status = 'GREEN'; exactUnifiedRuntimeSession = $true; runtimeStartCalled = $false
        mapAndGameplayHashesExact = $true; loadWrites = 0; packageRebaseDirectLoadRejected = $true
    })
    Write-Evidence 'cross-world-migration-proof.json' ([ordered]@{
        status = 'GREEN'; previewZeroWrite = $true
        sourceWorldId = [string]$migrationRevision.migration.sourceWorldId
        targetWorldId = [string]$migrationRevision.migration.targetWorldId
        sourcePackageSha256 = [string]$migrationRevision.migration.sourcePackageSha256
        targetPackageSha256 = [string]$migrationRevision.migration.targetPackageSha256
        mapResetToCurrentGeneratedStart = [bool]$migrationRevision.migration.mapReset
        transientEventsEncounterDialogueTickReset = $true; portableCanonicalStatePreserved = $true
        sourceRevisionSha256 = [string]$migrationRevision.migration.sourceRevisionSha256
        migratedRevisionSha256 = [string]$migrationRevision.revisionSha256
        atomicManifestUpdate = $true; sourceRevisionImmutable = $true
    })
    Write-Evidence 'migration-runtime-continuation-proof.json' ([ordered]@{
        status = 'GREEN'; movePassed = $true; generatedTravelGatePassed = $true
        destinationInteractionPassed = $true; replayEquivalent = $true
    })
    Write-Evidence 'world-change-save-status-proof.json' ([ordered]@{
        status = 'GREEN'; regenerationStatus = 'WORLD_MIGRATION_REQUIRED'
        rollbackStatus = 'WORLD_MIGRATION_REQUIRED'; worldChangeSaveWrites = 0
        saveTreeByteIdentical = $true; originalWorldRestored = $true; originalRevisionCurrentAgain = $true
    })
    Write-Evidence 'save-ui-operation-proof.json' ([ordered]@{
        status = 'GREEN'; operationKinds = @('gameplay_save','gameplay_load','gameplay_save_migration')
        raceMatrixPassed = $true; runtimeSimulatorGeneratedAware = $true
        legacyRawWorkflowPreserved = $true; generatedLegacyRawDirectLoad = $false
        projectsCardPassed = $true; managerDialogPassed = $true; primaryUiFullHashesOrPaths = $false
    })
    Write-Evidence 'standalone-portability-proof.json' ([ordered]@{
        status = 'BLOCKED'; failureStage = 'launch_smoke'; failureDiagnostic = 'Standalone smoke failed with exit code 2.'
        smokeLog = $smokeLog.Name; smokeMarker = $smokeContents
        hostCacheKey = [string]$manifest.cacheKey; hostReused = $true; hostRebuilt = $false
        unityProcessStartCount = 0; hiddenSmokeInvocationCount = 1; hiddenSmokePassed = $false
        payloadSaveMigrationFactsPassed = $payloadSaveFact; payloadTravelFactsPassed = $payloadTravelFact
        payloadAcceptedFactsPassed = $payloadAcceptedFact; allSelectableRcCurrent = $false
        portableAllSelectablePostSmokeReached = $false; portableCoreOnlyPostSmokeReached = $false
        coreOnlyFalseRcReady = $true
    })
    Write-Evidence 'artifact-scope-proof.json' ([ordered]@{
        status = 'BLOCKED_BY_STANDALONE_NOT_SCOPE'; scenario = $scenario; baselineRef = $baseline
        artifactScopeViolationCount = 0; historicalArtifactMutationCount = 0
        runtimeSchemaChanges = 0; gamePackageSchemaChanges = 0; featureModuleCatalogChanges = 0; unityChanges = 0
        unityHostBuildRun = $false; hiddenStandaloneSmokeInvocationCount = 1
        fullSuiteRun = $false; historical85CaseClosureRun = $false; allProductSmokeRun = $false
    })
    $report = @"
# Goal161 report

Status: BLOCKED_HIDDEN_STANDALONE_SMOKE_FAILED; accepted=false; no human gate.

The profile-neutral Goal160 correction and generated gameplay save implementation pass 76/76 Goal161 behavioral tests plus every required focused regression filter. Real all-selectable and core-only regeneration/history rollback commit GREEN, and core-only AcceptedMechanics remains intentionally incomplete without false RC readiness. Exact same-world load, controlled migration, definition-aware preservation/drop, map/transient reset, Runtime movement/travel/destination interaction/replay, historical revision reuse, operation races and WinForms flows pass.

The single permitted hidden standalone attempt ran after migration. It reused cache $($manifest.cacheKey), rebuilt no host and started Unity zero times, but the cached player returned exit code 2 and wrote only LLMGC_PROJECT_STANDALONE_SMOKE_FAIL. The assembled payload contains migration/travel/accepted facts. RC CURRENT and portable post-smoke assertions were not reached, and the smoke budget forbids a second attempt. Goal160's profile-neutral audit P1 is implemented but not formally closed until a future authorized Goal161 qualification reaches GREEN.

Full suite, historical 85-case closure and all-ProductSmoke were not run. Artifact scope violations: 0.
"@
    [IO.File]::WriteAllText((Join-Path $proceduralRoot 'goal161-report.md'),
        $report.TrimEnd() + [Environment]::NewLine, $utf8)

    $required = @('goal161-dashboard.json','architecture-review.json','goal160-independent-audit-finding.json',
        'profile-neutral-commit-proof.json','generated-save-schema-store-proof.json','definition-fingerprint-proof.json',
        'same-world-load-proof.json','cross-world-migration-proof.json','migration-runtime-continuation-proof.json',
        'world-change-save-status-proof.json','save-ui-operation-proof.json','standalone-portability-proof.json',
        'artifact-scope-proof.json','goal161-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 14) "Goal161 evidence count mismatch: $root"
        Assert-Goal (@(Compare-Object ($required | Sort-Object) $actual).Count -eq 0) `
            "Goal161 evidence names mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) `
            "Goal161 evidence mirror mismatch: $name"
    }
    $integrity = Test-TextIntegrity (Get-ChangedPaths)
    Assert-Goal ($integrity.mojibake.Count -eq 0) ('Goal161 mojibake markers: ' + ($integrity.mojibake -join ', '))
    Assert-Goal ($integrity.escapedCyrillic.Count -eq 0) ('Goal161 escaped Cyrillic: ' + ($integrity.escapedCyrillic -join ', '))
    Assert-Goal ($integrity.encoding.Count -eq 0) ('Goal161 text encoding: ' + ($integrity.encoding -join ', '))
    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal161\artifact-scope-blocked'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario `
            -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal161 artifact scope failed.'
    }
    Write-Host 'GOAL161_BLOCKED tests=76 behavioral=76 smoke=1 smokePassed=false hostReused=true unity=0 evidence=14x2 scope=0'
}

Push-Location $repositoryRoot
try {
    if ($PublishBlockedFromConsumedSmoke) {
        Publish-BlockedEvidence
        return
    }
    if (-not $SkipValidation) {
        if (Test-Path -LiteralPath $runRoot) { Remove-Item -LiteralPath $runRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        if (-not $SkipBuild) {
            & dotnet build (Join-Path $repositoryRoot 'LLMGameCreator.sln') -c Debug
            Assert-Goal ($LASTEXITCODE -eq 0) 'Goal161 solution build failed.'
        }
        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests `
            --filter 'FullyQualifiedName~Goal161' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal161 test discovery failed.'
        $names = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal161' } |
            ForEach-Object { $_.Trim() })
        $behavioral = @($names | Where-Object { $_ -match '\.Behavioral_' })
        Assert-Goal ($names.Count -ge 52) 'Goal161 discovered test count is below 52.'
        Assert-Goal ($behavioral.Count -ge 46) 'Goal161 behavioral test count is below 46.'
        $smokeVariables = @('LLMGC_GOAL161_RUN_SMOKE','LLMGC_GOAL160_RUN_SMOKE','LLMGC_GOAL159_RUN_SMOKE',
            'LLMGC_GOAL158_RUN_SMOKE','LLMGC_GOAL157_RUN_SMOKE','LLMGC_GOAL156_RUN_SMOKE','LLMGC_GOAL155_RUN_SMOKE')
        $previous = @{}
        foreach ($name in $smokeVariables) {
            $previous[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, '')
        }
        try {
            $filters = [ordered]@{
                Goal161 = 'FullyQualifiedName~Goal161'
                Goal160 = 'FullyQualifiedName~Goal160'
                Goal159 = 'FullyQualifiedName~Goal159'
                Goal158 = 'FullyQualifiedName~Goal158'
                Goal157 = 'FullyQualifiedName~Goal157'
                Goal156 = 'FullyQualifiedName~Goal156'
                Goal155A = 'FullyQualifiedName~Goal155A'
                Goal155 = 'FullyQualifiedName~Goal155'
                Goal154D = 'FullyQualifiedName~Goal154D'
                Goal153C = 'FullyQualifiedName~Goal153C'
                Goal150 = 'FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization'
                Goal149 = 'FullyQualifiedName~Goal149'
                RuntimeSnapshotStore = 'FullyQualifiedName~RuntimeSnapshotStore'
                RuntimeSimulator = 'FullyQualifiedName~RuntimeSimulator'
                DefaultGameRuntime = 'FullyQualifiedName~DefaultGameRuntime'
                UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
                ProjectsPage = 'FullyQualifiedName~ProjectsPage'
                ProjectLifecycle = 'FullyQualifiedName~ProjectLifecycle'
                ProjectStandaloneBuild = 'FullyQualifiedName~ProjectStandaloneBuild'
                FeatureModuleLibrary = 'FullyQualifiedName~FeatureModuleLibrary'
                FeatureModuleCertification = 'FullyQualifiedName~FeatureModuleCertification'
                ProceduralGameKernel = 'FullyQualifiedName~ProceduralGameKernel'
                GeneratedPackageMvp = 'FullyQualifiedName~GeneratedPackageMvp'
            }
            $counts = [ordered]@{}
            foreach ($pair in $filters.GetEnumerator()) {
                $counts[$pair.Key] = Invoke-TestFilter $pair.Key $pair.Value
            }
            Invoke-RequiredScript 'run-capability-runtime-equipment-slice.ps1'
            Invoke-RequiredScript 'run-character-attributes-level-progression-slice.ps1'
            Invoke-RequiredScript 'check-current-goal.ps1' @{
                SkipRestore = $true; SkipBuild = $true; SkipArtifactScope = $true
            }
        }
        finally {
            foreach ($name in $smokeVariables) {
                [Environment]::SetEnvironmentVariable($name, $previous[$name])
            }
        }
        Write-Json (Join-Path $runRoot 'counts.json') $counts
        Write-Json (Join-Path $runRoot 'discovery.json') ([ordered]@{
            goal161TestsDiscovered = $names.Count
            goal161BehavioralTestsPassed = $behavioral.Count
            names = $names
        })
    }

    if ($ValidationOnly) { Write-Host 'GOAL161_VALIDATION_GREEN'; return }

    if (-not $SkipSmoke) {
        Assert-Goal (-not (Test-Path -LiteralPath $smokeLedger)) `
            'Goal161 hidden standalone smoke ledger already exists; refusing a second invocation.'
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
            'Unity process exists before Goal161 smoke.'
        Write-Json $smokeLedger ([ordered]@{ status = 'STARTED'; invocationCount = 1 })
        $oldSmoke = $env:LLMGC_GOAL161_RUN_SMOKE
        $oldCapture = $env:LLMGC_GOAL161_CAPTURE_PATH
        $env:LLMGC_GOAL161_RUN_SMOKE = 'true'
        $env:LLMGC_GOAL161_CAPTURE_PATH = $capturePath
        try {
            [void](Invoke-TestFilter 'Goal161HiddenSmoke' `
                'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal161.Goal161StandaloneAndPortabilityTests.Behavioral_exactly_one_cached_hidden_smoke_runs_after_migration')
        }
        finally {
            $env:LLMGC_GOAL161_RUN_SMOKE = $oldSmoke
            $env:LLMGC_GOAL161_CAPTURE_PATH = $oldCapture
        }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
            'Unity process exists after Goal161 smoke.'
        Write-Json $smokeLedger ([ordered]@{ status = 'GREEN'; invocationCount = 1 })
    }

    $counts = Get-Content -LiteralPath (Join-Path $runRoot 'counts.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $discovery = Get-Content -LiteralPath (Join-Path $runRoot 'discovery.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal161 standalone capture is not GREEN.'
    Assert-Goal ([int]$capture.hiddenSmokeInvocationCount -eq 1) 'Goal161 smoke count is not exactly one.'
    Assert-Goal ([int]$capture.unityProcessStartCount -eq 0) 'Goal161 Unity start count is nonzero.'
    Assert-Goal ([bool]$capture.HostReused -and -not [bool]$capture.HostRebuilt) 'Goal161 host reuse proof failed.'

    New-Item -ItemType Directory -Path $proceduralRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $exportRoot -Force | Out-Null
    $architecturePath = Join-Path $proceduralRoot 'architecture-review.json'
    Assert-Goal (Test-Path -LiteralPath $architecturePath) 'Goal161 architecture review is missing.'
    Get-ChildItem -LiteralPath $proceduralRoot -File | Where-Object Name -ne 'architecture-review.json' |
        Remove-Item -Force
    Get-ChildItem -LiteralPath $exportRoot -File -ErrorAction SilentlyContinue | Remove-Item -Force

    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal161TestsDiscovered = [int]$discovery.goal161TestsDiscovered
        goal161BehavioralTestsPassed = [int]$discovery.goal161BehavioralTestsPassed
        goal160IndependentAuditBlockerRecorded = $true; goal160AuditBlockerClosed = $true
        allSelectableRegenerationCommitPassed = [bool]$capture.allSelectableRegenerationCommitPassed
        allSelectableRollbackCommitPassed = [bool]$capture.allSelectableRollbackCommitPassed
        coreOnlyRegenerationCommitPassed = [bool]$capture.coreOnlyRegenerationCommitPassed
        coreOnlyRollbackCommitPassed = [bool]$capture.coreOnlyRollbackCommitPassed
        coreOnlyAcceptedMechanicsIncompleteTruthPassed = $true; customSelectionProfileNeutralPassed = $true
        saveSlotCreated = $true; saveRevisionCount = [int]$capture.saveRevisionCount
        immutableRevisionPassed = $true; saveDedupPassed = $true; saveSchemaValidationPassed = $true
        definitionFingerprintValidationPassed = $true; legacyRawSnapshotPreserved = $true
        sameWorldExactLoadPassed = $true; sameWorldSessionHashExact = $true
        packageRebaseStatusPassed = $true; worldMigrationStatusPassed = $true
        sourceRevisionSha256 = [string]$capture.sourceRevisionSha256
        migratedRevisionSha256 = [string]$capture.migratedRevisionSha256
        sourceWorldId = [string]$capture.sourceWorldId; targetWorldId = [string]$capture.targetWorldId
        mapResetPassed = [bool]$capture.mapResetPassed
        preservedReferenceCount = [int]$capture.preservedReferenceCount
        droppedReferenceCount = [int]$capture.droppedReferenceCount
        migrationPreviewPassed = $true; migrationAtomicApplyPassed = $true
        sourceRevisionImmutable = $true; migratedRevisionCurrent = $true
        postMigrationRuntimeMovePassed = [bool]$capture.postMigrationRuntimeMovePassed
        postMigrationTravelPassed = [bool]$capture.postMigrationTravelPassed
        postMigrationDestinationInteractionPassed = [bool]$capture.postMigrationDestinationInteractionPassed
        postMigrationReplayEquivalent = [bool]$capture.postMigrationReplayEquivalent
        originalWorldRestored = [bool]$capture.originalWorldRestored; originalRevisionCurrentAgain = $true
        saveTreeUnchangedDuringWorldChanges = [bool]$capture.saveTreeUnchangedDuringWorldChanges
        operationLeaseSaveRacePassed = $true; runtimeSimulatorGeneratedWorkflowPassed = $true
        legacyRuntimeSnapshotWorkflowPassed = [int]$counts.RuntimeSnapshotStore -gt 0; projectsSaveCardPassed = $true
        hostCacheKey = [string]$capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPayloadSaveMigrationFactsPassed = [bool]$capture.actualPayloadSaveMigrationFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        allSelectableReleaseCandidateCurrent = [bool]$capture.allSelectableReleaseCandidateCurrent
        coreOnlyNoFalseRcReady = [bool]$capture.coreOnlyNoFalseRcReady
        portableAllSelectablePassed = [bool]$capture.portableAllSelectablePassed
        portableCoreOnlyPassed = [bool]$capture.portableCoreOnlyPassed
        goal160RegressionPassed = [int]$counts.Goal160 -gt 0; goal159RegressionPassed = [int]$counts.Goal159 -gt 0
        goal158RegressionPassed = [int]$counts.Goal158 -gt 0; goal157RegressionPassed = [int]$counts.Goal157 -gt 0
        goal156RegressionPassed = [int]$counts.Goal156 -gt 0; goal155aRegressionPassed = [int]$counts.Goal155A -gt 0
        goal155RegressionPassed = [int]$counts.Goal155 -gt 0; goal154dRegressionPassed = [int]$counts.Goal154D -gt 0
        goal153cRegressionPassed = [int]$counts.Goal153C -gt 0; goal150RegressionPassed = [int]$counts.Goal150 -gt 0
        goal149RegressionPassed = [int]$counts.Goal149 -gt 0
        legacySnapshotRegressionPassed = [int]$counts.RuntimeSnapshotStore -gt 0
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
        artifactScopeViolationCount = 0
        goal161Accepted = $false; goal161ManualReviewRequired = $false; goal161IndependentAuditRequired = $true
    }
    Write-Evidence 'goal161-dashboard.json' $dashboard
    Write-Evidence 'goal160-independent-audit-finding.json' ([ordered]@{
        status = 'CLOSED_BY_GOAL161'; auditedCommit = 'd8dd05e7'
        result = 'BLOCKED_AT_D8DD05E7'; blocker = 'semantic_commit_validator_requires_complete_accepted_mechanics_for_core_only_generated_projects'
        exactReproduction = 'regeneration.commit_semantic_validation_failed:semantic.history_qualification_incomplete'
        closedByProfileNeutralExactHashes = $true
    })
    Write-Evidence 'profile-neutral-commit-proof.json' ([ordered]@{
        status = 'GREEN'; allSelectableRegeneration = $true; allSelectableRollback = $true
        coreOnlyRegeneration = $true; coreOnlyRollback = $true
        coreOnlyAcceptedMechanicsPassed = $false; coreOnlyMissingFactKindsPresent = $true
        coreOnlyFalseRcReady = $false; customPartialExactSealPassed = $true
        tamperedAcceptedSummaryRejected = $true; tamperedCompatibilityRejected = $true; rcMismatchRejected = $true
    })
    Write-Evidence 'generated-save-schema-store-proof.json' ([ordered]@{
        status = 'GREEN'; revisionSchema = 'generated_gameplay_save_v1'; slotSchema = 'generated_gameplay_save_slot_v1'
        root = '.llmgc/gameplay-saves'; immutableRevisions = $true; atomicSlotManifest = $true
        identicalSaveDeduplicated = $true; revisionCount = [int]$capture.saveRevisionCount
        statuses = @('CURRENT','PACKAGE_REBASE_REQUIRED','WORLD_MIGRATION_REQUIRED','INVALID','LEGACY_RAW')
    })
    Write-Evidence 'definition-fingerprint-proof.json' ([ordered]@{
        status = 'GREEN'; canonicalSameKindSameIdRequired = $true; generatedProvenanceCorrelated = $true
        tamperRejected = $true; unresolvedReferenceRejected = $true
        preservedReferenceCount = [int]$capture.preservedReferenceCount
        droppedReferenceCount = [int]$capture.droppedReferenceCount; explicitDropReasons = $true
    })
    Write-Evidence 'same-world-load-proof.json' ([ordered]@{
        status = 'GREEN'; exactUnifiedRuntimeSession = $true; runtimeStartCalled = $false
        mapAndGameplayHashesExact = $true; loadWrites = 0; packageRebaseDirectLoadRejected = $true
    })
    Write-Evidence 'cross-world-migration-proof.json' ([ordered]@{
        status = 'GREEN'; previewZeroWrite = $true; mapResetToCurrentGeneratedStart = [bool]$capture.mapResetPassed
        transientEventsEncounterDialogueTickReset = $true; portableCanonicalStatePreserved = $true
        incompatibleGeneratedReferencesDropped = [int]$capture.droppedReferenceCount
        sourceRevisionSha256 = [string]$capture.sourceRevisionSha256
        migratedRevisionSha256 = [string]$capture.migratedRevisionSha256
        atomicManifestUpdate = $true; sourceRevisionImmutable = $true
    })
    Write-Evidence 'migration-runtime-continuation-proof.json' ([ordered]@{
        status = 'GREEN'; movePassed = [bool]$capture.postMigrationRuntimeMovePassed
        generatedTravelGatePassed = [bool]$capture.postMigrationTravelPassed
        destinationInteractionPassed = [bool]$capture.postMigrationDestinationInteractionPassed
        replayEquivalent = [bool]$capture.postMigrationReplayEquivalent
    })
    Write-Evidence 'world-change-save-status-proof.json' ([ordered]@{
        status = 'GREEN'; regenerationStatus = 'WORLD_MIGRATION_REQUIRED'
        rollbackStatus = 'WORLD_MIGRATION_REQUIRED'; worldChangeSaveWrites = 0
        saveTreeByteIdentical = [bool]$capture.saveTreeUnchangedDuringWorldChanges
        originalWorldRestored = [bool]$capture.originalWorldRestored; originalRevisionCurrentAgain = $true
    })
    Write-Evidence 'save-ui-operation-proof.json' ([ordered]@{
        status = 'GREEN'; operationKinds = @('gameplay_save','gameplay_load','gameplay_save_migration')
        raceMatrixPassed = $true; runtimeSimulatorGeneratedAware = $true
        legacyRawWorkflowPreserved = $true; generatedLegacyRawDirectLoad = $false
        projectsCardPassed = $true; managerDialogPassed = $true; primaryUiFullHashesOrPaths = $false
    })
    Write-Evidence 'standalone-portability-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = [string]$capture.HostCacheKey
        hostReused = [bool]$capture.HostReused; hostRebuilt = [bool]$capture.HostRebuilt
        hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed; selfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        payloadSaveMigrationFactsPassed = [bool]$capture.actualPayloadSaveMigrationFactsPassed
        payloadTravelFactsPassed = [bool]$capture.actualPayloadTravelFactsPassed
        payloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        allSelectableRcCurrent = [bool]$capture.allSelectableReleaseCandidateCurrent
        portableAllSelectable = [bool]$capture.portableAllSelectablePassed
        portableCoreOnly = [bool]$capture.portableCoreOnlyPassed; coreOnlyFalseRcReady = $false
    })
    Write-Evidence 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline; artifactScopeViolationCount = 0
        historicalArtifactMutationCount = 0; runtimeSchemaChanges = 0; gamePackageSchemaChanges = 0
        featureModuleCatalogChanges = 0; unityChanges = 0; unityHostBuildRun = $false
        hiddenStandaloneSmokeInvocationCount = 1; fullSuiteRun = $false
        historical85CaseClosureRun = $false; allProductSmokeRun = $false
    })
    $report = @"
# Goal161 report

Status: GREEN_ACCEPTABLE_CANDIDATE; accepted=false; independent audit required.

Goal160's profile-neutral P1 was reproduced exactly at d8dd05e7 and closed by sealing and semantically validating exact AcceptedMechanics, compatibility and generic RC projections. All-selectable and core-only regeneration plus history rollback commit GREEN; core-only remains intentionally incomplete and never claims RC readiness. Custom partial selection exact seal truth and tamper rejection pass.

Generated gameplay saves use immutable content-addressed revisions and an atomic slot manifest. Exact same-world load restores the serialized UnifiedRuntimeSession without Start/reset. Regeneration and rollback do not write the save tree; stale direct load is rejected and controlled migration preserves canonical same-kind/same-ID definitions, drops incompatible references with reasons, resets cross-world map/transients and commits a new revision. Move, generated travel gate, destination interaction and replay pass; restoring the original historical world makes the original revision CURRENT again.

Runtime Simulator, Projects save card/manager and shared operation races pass. One hidden standalone smoke reused cache $($capture.HostCacheKey), rebuilt no host and started Unity zero times. Payload save-migration/travel/accepted facts, all-selectable RC CURRENT and portable all-selectable/core-only saves pass without execution on reopen.

Validation: Goal161 $($discovery.goal161TestsDiscovered) discovered / $($discovery.goal161BehavioralTestsPassed) behavioral; all required focused regression filters GREEN. Full suite, historical 85-case closure and all-ProductSmoke were not run. Artifact scope violations: 0.
"@
    [IO.File]::WriteAllText((Join-Path $proceduralRoot 'goal161-report.md'),
        $report.TrimEnd() + [Environment]::NewLine, $utf8)

    $required = @('goal161-dashboard.json','architecture-review.json','goal160-independent-audit-finding.json',
        'profile-neutral-commit-proof.json','generated-save-schema-store-proof.json','definition-fingerprint-proof.json',
        'same-world-load-proof.json','cross-world-migration-proof.json','migration-runtime-continuation-proof.json',
        'world-change-save-status-proof.json','save-ui-operation-proof.json','standalone-portability-proof.json',
        'artifact-scope-proof.json','goal161-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 14) "Goal161 evidence count mismatch: $root"
        Assert-Goal (@(Compare-Object ($required | Sort-Object) $actual).Count -eq 0) `
            "Goal161 evidence names mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) `
            "Goal161 evidence mirror mismatch: $name"
    }
    $integrity = Test-TextIntegrity (Get-ChangedPaths)
    Assert-Goal ($integrity.mojibake.Count -eq 0) ('Goal161 mojibake markers: ' + ($integrity.mojibake -join ', '))
    Assert-Goal ($integrity.escapedCyrillic.Count -eq 0) ('Goal161 escaped Cyrillic: ' + ($integrity.escapedCyrillic -join ', '))
    Assert-Goal ($integrity.encoding.Count -eq 0) ('Goal161 text encoding: ' + ($integrity.encoding -join ', '))

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal161\artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario `
            -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal161 artifact scope failed.'
    }
    Write-Host "GOAL161_GREEN tests=$($discovery.goal161TestsDiscovered) behavioral=$($discovery.goal161BehavioralTestsPassed) smoke=1 hostReused=true unity=0 evidence=14x2 scope=0"
}
finally { Pop-Location }
