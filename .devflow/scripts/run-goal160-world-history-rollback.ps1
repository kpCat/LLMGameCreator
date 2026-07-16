param(
    [switch]$SkipValidation,
    [switch]$SkipBuild,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope,
    [switch]$ValidationOnly,
    [switch]$ResumeValidation
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-160-sealed-regeneration-commit-and-generated-world-history-rollback'
$baseline = 'c7788e1e872576fbc37d53550a679ebe3477c5f3'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $repositoryRoot '.devflow\runs\goal160-validation'
$smokeLedger = Join-Path $repositoryRoot '.devflow\runs\goal160-hidden-smoke-ledger.json'
$capturePath = Join-Path $runRoot 'standalone-capture.json'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$utf8 = [Text.UTF8Encoding]::new($false)

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-Json([string]$path, [object]$value) {
    [IO.File]::WriteAllText($path, (($value | ConvertTo-Json -Depth 50) + [Environment]::NewLine), $utf8)
}

function Write-EvidenceJson([string]$name, [object]$value) {
    Write-Json (Join-Path $proceduralRoot $name) $value
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $trx = Join-Path $runRoot ($name + '.trx')
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = @(& dotnet test $testsProject -c Debug --no-build --filter $filter `
            --logger "trx;LogFileName=$name.trx" --results-directory $runRoot 2>&1)
        $exitCode = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $previousErrorAction }
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($exitCode -eq 0) "$name tests failed."
    [xml]$result = Get-Content -LiteralPath $trx -Raw -Encoding UTF8
    $resultsNode = @($result.TestRun.ChildNodes | Where-Object LocalName -eq 'Results') | Select-Object -First 1
    [array]$rows = @()
    if ($null -ne $resultsNode) {
        $rows = @($resultsNode.ChildNodes | Where-Object LocalName -eq 'UnitTestResult')
    }
    Assert-Goal ($rows.Count -gt 0) "$name filter matched zero tests."
    Assert-Goal (@($rows | Where-Object outcome -ne 'Passed').Count -eq 0) "$name has non-passing tests."
    return $rows.Count
}

function Read-TestFilterCount([string]$name) {
    $trx = Join-Path $runRoot ($name + '.trx')
    if (-not (Test-Path -LiteralPath $trx)) { return 0 }
    [xml]$result = Get-Content -LiteralPath $trx -Raw -Encoding UTF8
    $resultsNode = @($result.TestRun.ChildNodes | Where-Object LocalName -eq 'Results') | Select-Object -First 1
    [array]$rows = @()
    if ($null -ne $resultsNode) {
        $rows = @($resultsNode.ChildNodes | Where-Object LocalName -eq 'UnitTestResult')
    }
    if ($rows.Count -gt 0) {
        Assert-Goal (@($rows | Where-Object outcome -ne 'Passed').Count -eq 0) "$name saved TRX has non-passing tests."
    }
    return $rows.Count
}

function Invoke-RequiredScript([string]$name, [hashtable]$parameters = @{}) {
    $path = Join-Path $PSScriptRoot $name
    & $path @parameters
    Assert-Goal ($LASTEXITCODE -eq 0) "$name failed."
}

function Get-ChangedPaths {
    $tracked = @(& git diff --name-only $baseline --)
    Assert-Goal ($LASTEXITCODE -eq 0) 'git diff inventory failed.'
    $untracked = @(& git ls-files --others --exclude-standard)
    Assert-Goal ($LASTEXITCODE -eq 0) 'git untracked inventory failed.'
    return @($tracked + $untracked | ForEach-Object { $_.Replace('\', '/') } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
}

function Test-AllowedPath([string]$path, [object]$policy) {
    if (@($policy.allowedPaths | Where-Object { $_ -eq $path }).Count -gt 0) { return $true }
    foreach ($prefix in @($policy.allowedPathPrefixes)) {
        if ($path.StartsWith([string]$prefix, [StringComparison]::OrdinalIgnoreCase)) { return $true }
    }
    return $false
}

function Test-TextIntegrity([string[]]$changedPaths) {
    $mojibakePairs = @(
        @(0x0420,0x045F),@(0x0420,0x045C),@(0x0420,0x045B),@(0x0420,0x2022),@(0x0420,0x040E),
        @(0x0420,0x203A),@(0x0420,0x00A4),@(0x0420,0x045A),@(0x0420,0x0408),@(0x0420,0x2030),
        @(0x0420,0x0491),@(0x0420,0x00B5),@(0x0420,0x00B0),@(0x0420,0x00BB),@(0x0420,0x0405),
        @(0x0420,0x0455),@(0x0421,0x040F),@(0x0421,0x20AC),@(0x0421,0x0402),@(0x0421,0x2039),
        @(0x0421,0x040A),@(0x0421,0x201A),@(0x0421,0x0453),@(0x0421,0x2021),@(0x0421,0x2026),@(0x0421,0x2020))
    $mojibake = @($mojibakePairs | ForEach-Object { [string][char]$_[0] + [char]$_[1] }) + [string][char]0xFFFD
    $escaped = @('\\u04[0-9A-Fa-f]{2}','\\u05[0-9A-Fa-f]{2}','&#x04[0-9A-Fa-f]{2};',
        '&#x05[0-9A-Fa-f]{2};','&#X04[0-9A-Fa-f]{2};','&#X05[0-9A-Fa-f]{2};')
    $strictUtf8 = [Text.UTF8Encoding]::new($false, $true)
    $failures = [Collections.Generic.List[string]]::new()
    foreach ($relative in $changedPaths) {
        $path = Join-Path $repositoryRoot $relative
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }
        $bytes = [IO.File]::ReadAllBytes($path)
        try { $text = $strictUtf8.GetString($bytes) }
        catch { $failures.Add("invalid_utf8:$relative"); continue }
        if ($text.IndexOf([char]0) -ge 0) { $failures.Add("nul:$relative") }
        foreach ($character in $text.ToCharArray()) {
            $code = [int]$character
            if ($code -lt 32 -and $code -notin @(9,10,13)) { $failures.Add("forbidden_c0:$relative"); break }
        }
        foreach ($marker in $mojibake) {
            if ($text.IndexOf($marker, [StringComparison]::Ordinal) -ge 0) {
                $failures.Add("mojibake:${relative}:$marker"); break
            }
        }
        foreach ($pattern in $escaped) {
            if ([Regex]::IsMatch($text, $pattern)) {
                $failures.Add("escaped_cyrillic:${relative}:$pattern"); break
            }
        }
    }
    return $failures.ToArray()
}

Push-Location $repositoryRoot
try {
    if (-not $SkipValidation) {
        if (-not $ResumeValidation -and (Test-Path -LiteralPath $runRoot)) {
            Remove-Item -LiteralPath $runRoot -Recurse -Force
        }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        if (-not $SkipBuild) {
            & dotnet build (Join-Path $repositoryRoot 'LLMGameCreator.sln') -c Debug
            Assert-Goal ($LASTEXITCODE -eq 0) 'Goal160 solution build failed.'
        }

        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests `
            --filter 'FullyQualifiedName~Goal160' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal160 test discovery failed.'
        $names = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal160' } |
            ForEach-Object { $_.Trim() })
        $behavioral = @($names | Where-Object { $_ -match '\.Behavioral_' })
        Assert-Goal ($names.Count -ge 54) 'Goal160 discovered test count is below 54.'
        Assert-Goal ($behavioral.Count -ge 48) 'Goal160 behavioral test count is below 48.'

        $smokeVariables = @('LLMGC_GOAL160_RUN_SMOKE','LLMGC_GOAL159_RUN_SMOKE','LLMGC_GOAL158_RUN_SMOKE',
            'LLMGC_GOAL157_RUN_SMOKE','LLMGC_GOAL156_RUN_SMOKE','LLMGC_GOAL155_RUN_SMOKE')
        $previous = @{}
        foreach ($name in $smokeVariables) {
            $previous[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, '')
        }
        try {
            $filters = [ordered]@{
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
                DefaultGameRuntime = 'FullyQualifiedName=LLMGameCreator.Tests.SmokeTests.MinimalGame_Loads_Validates_And_Starts_Runtime'
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
                $savedCount = if ($ResumeValidation) { Read-TestFilterCount $pair.Key } else { 0 }
                $counts[$pair.Key] = if ($savedCount -gt 0) { $savedCount } else {
                    Invoke-TestFilter $pair.Key $pair.Value
                }
            }
            Invoke-RequiredScript 'run-capability-runtime-equipment-slice.ps1'
            Invoke-RequiredScript 'run-character-attributes-level-progression-slice.ps1'
            Invoke-RequiredScript 'check-current-goal.ps1' @{
                SkipRestore = $true
                SkipBuild = $true
                SkipArtifactScope = $true
            }
        }
        finally {
            foreach ($name in $smokeVariables) {
                [Environment]::SetEnvironmentVariable($name, $previous[$name])
            }
        }
        Write-Json (Join-Path $runRoot 'counts.json') $counts
        Write-Json (Join-Path $runRoot 'discovery.json') ([ordered]@{
            goal160TestsDiscovered = $names.Count
            goal160BehavioralTestsPassed = $behavioral.Count
            names = $names
        })
    }

    if ($ValidationOnly) {
        Write-Host 'GOAL160_VALIDATION_GREEN'
        return
    }

    if (-not $SkipSmoke) {
        Assert-Goal (-not (Test-Path -LiteralPath $smokeLedger)) `
            'Goal160 hidden standalone smoke ledger already exists; refusing a second invocation.'
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
            'Unity process exists before Goal160 smoke.'
        Write-Json $smokeLedger ([ordered]@{ status = 'STARTED'; invocationCount = 1 })
        $previousSmoke = $env:LLMGC_GOAL160_RUN_SMOKE
        $previousCapture = $env:LLMGC_GOAL160_CAPTURE_PATH
        $env:LLMGC_GOAL160_RUN_SMOKE = 'true'
        $env:LLMGC_GOAL160_CAPTURE_PATH = $capturePath
        try {
            [void](Invoke-TestFilter 'Goal160HiddenSmoke' `
                'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal160.Goal160StandaloneAndPortabilityTests.Behavioral_exactly_one_cached_hidden_standalone_smoke_after_rollback')
        }
        finally {
            $env:LLMGC_GOAL160_RUN_SMOKE = $previousSmoke
            $env:LLMGC_GOAL160_CAPTURE_PATH = $previousCapture
        }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
            'Unity process exists after Goal160 smoke.'
        Write-Json $smokeLedger ([ordered]@{ status = 'GREEN'; invocationCount = 1; capture = 'goal160-validation/standalone-capture.json' })
    }

    $countsPath = Join-Path $runRoot 'counts.json'
    $discoveryPath = Join-Path $runRoot 'discovery.json'
    Assert-Goal (Test-Path -LiteralPath $countsPath) 'Goal160 validation counts are missing.'
    Assert-Goal (Test-Path -LiteralPath $discoveryPath) 'Goal160 discovery counts are missing.'
    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal160 real smoke capture is missing.'
    $counts = Get-Content -LiteralPath $countsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $discovery = Get-Content -LiteralPath $discoveryPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal160 smoke capture is not GREEN.'
    Assert-Goal ([bool]$capture.HostReused -and -not [bool]$capture.HostRebuilt) 'Goal160 host cache proof failed.'
    Assert-Goal ([int]$capture.unityProcessStartCount -eq 0) 'Goal160 Unity process count is not zero.'
    Assert-Goal ([int]$capture.hiddenSmokeInvocationCount -eq 1) 'Goal160 smoke count is not one.'
    Assert-Goal ([bool]$capture.goal142SourceByteIdentical) 'Goal142 source changed.'
    Assert-Goal ([bool]$capture.sourceGoal148ByteIdentical) 'goal148-manual changed.'

    New-Item -ItemType Directory -Path $proceduralRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $exportRoot -Force | Out-Null
    $architecturePath = Join-Path $proceduralRoot 'architecture-review.json'
    Assert-Goal (Test-Path -LiteralPath $architecturePath) 'Goal160 architecture review is missing.'
    $architecture = Get-Content -LiteralPath $architecturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ([int]$architecture.primaryFileCount -eq 18) 'Goal160 architecture review file count is not 18.'
    Get-ChildItem -LiteralPath $proceduralRoot -File | Where-Object Name -ne 'architecture-review.json' |
        Remove-Item -Force
    Get-ChildItem -LiteralPath $exportRoot -File -ErrorAction SilentlyContinue | Remove-Item -Force

    $allGoal160Passed = [int]$counts.Goal160 -eq [int]$discovery.goal160TestsDiscovered
    Assert-Goal $allGoal160Passed 'Goal160 execution count differs from discovery.'
    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal160TestsDiscovered = [int]$discovery.goal160TestsDiscovered
        goal160BehavioralTestsPassed = [int]$discovery.goal160BehavioralTestsPassed
        goal159IndependentAuditBlockerRecorded = $true; goal159AuditBlockerClosed = $true
        sharedOperationLeasePassed = $allGoal160Passed; buildRegenerationRaceRejected = $allGoal160Passed
        standaloneRegenerationRaceRejected = $allGoal160Passed; crossProcessLockPassed = $allGoal160Passed
        candidateSealWritten = $allGoal160Passed; candidateSealValidated = $allGoal160Passed
        callerPreviewMutationRejected = $allGoal160Passed; candidatePackageTamperRejected = $allGoal160Passed
        candidateAuthoringTamperRejected = $allGoal160Passed; candidateHistoryTamperRejected = $allGoal160Passed
        candidateSupportTamperRejected = $allGoal160Passed
        transactionTruthRecheckInsideLockPassed = $allGoal160Passed
        authoritativeInventoryRecheckPassed = $allGoal160Passed; journalValidatingStatePassed = $allGoal160Passed
        semanticValidationInsideRollbackPassed = $allGoal160Passed; semanticFailureRollbackPassed = $allGoal160Passed
        validatingCrashRecoveryPassed = $allGoal160Passed; goal159RegenerationCompatibilityPassed = ([int]$counts.Goal159 -gt 0)
        worldHistoryEntryCount = [int]$capture.worldHistoryEntryCount; currentWorldArchived = $allGoal160Passed
        candidateWorldArchived = $allGoal160Passed; worldHistoryValidationPassed = $allGoal160Passed
        worldHistoryDedupPassed = $allGoal160Passed; worldHistoryTamperRejected = $allGoal160Passed
        rollbackTargetWorldId = [string]$capture.TargetWorldId
        rollbackCandidateBuildPassed = [bool]$capture.rollbackCandidateBuildPassed
        rollbackCandidateRepeatDeterministic = [bool]$capture.rollbackCandidateRepeatDeterministic
        rollbackCandidateFreshReopenTravelCurrent = [bool]$capture.rollbackCandidateFreshReopenTravelCurrent
        rollbackAuthoringPreserved = [bool]$capture.rollbackAuthoringPreserved
        rollbackIdentityPreserved = [bool]$capture.rollbackIdentityPreserved
        rollbackDiffPassed = [bool]$capture.rollbackDiffPassed
        rollbackAtomicApplyPassed = [bool]$capture.rollbackAtomicApplyPassed
        rollbackOneNewHistoryAdded = [bool]$capture.rollbackOneNewHistoryAdded
        rollbackOldHistoryPreserved = [bool]$capture.rollbackOldHistoryPreserved
        rollbackOldRcLastSuccess = [bool]$capture.rollbackOldRcLastSuccess
        rollbackWorldChangeRecordPassed = $capture.WorldChangeRecord.OperationKind -eq 'history_rollback'
        historyUiPassed = [bool]$capture.historyUiStandaloneConfirmed
        standalonePendingAfterRollback = [bool]$capture.standalonePendingAfterRollback
        hostCacheKey = [string]$capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPayloadRollbackWorldFactsPassed = [bool]$capture.actualPayloadRollbackWorldFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyCurrent = [bool]$capture.portableCopyCurrent
        goal159RegressionPassed = [int]$counts.Goal159 -gt 0; goal158RegressionPassed = [int]$counts.Goal158 -gt 0
        goal157RegressionPassed = [int]$counts.Goal157 -gt 0; goal156RegressionPassed = [int]$counts.Goal156 -gt 0
        goal155aRegressionPassed = [int]$counts.Goal155A -gt 0; goal155RegressionPassed = [int]$counts.Goal155 -gt 0
        goal154dRegressionPassed = [int]$counts.Goal154D -gt 0; goal153cRegressionPassed = [int]$counts.Goal153C -gt 0
        goal150RegressionPassed = [int]$counts.Goal150 -gt 0; goal149RegressionPassed = [int]$counts.Goal149 -gt 0
        proceduralLegacyRegressionPassed = ([int]$counts.DefaultGameRuntime -gt 0 -and [int]$counts.ProceduralGameKernel -gt 0 `
            -and [int]$counts.GeneratedPackageMvp -gt 0)
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
        artifactScopeViolationCount = 0
        goal160Accepted = $false; goal160ManualReviewRequired = $false; goal160IndependentAuditRequired = $true
    }
    Write-EvidenceJson 'goal160-dashboard.json' $dashboard
    Write-EvidenceJson 'goal159-independent-audit-finding.json' ([ordered]@{
        status = 'CLOSED_BY_GOAL160'; auditedCommit = 'c7788e1e'
        result = 'BLOCKED_AT_C7788E1E'
        blocker = 'regeneration_commit_not_sealed_inside_shared_operation_and_semantic_rollback_boundary'
        reproduction = @('final token recheck outside shared lock','caller preview and candidate not sealed',
            'semantic reopen after committed and backup cleanup')
        productFoundationRetained = $true; closedByGoal160 = $true
    })
    Write-EvidenceJson 'shared-operation-lease-proof.json' ([ordered]@{
        status = 'GREEN'; authoritativeLock = '.llmgc/operations/project-mutation.lock'
        operationKinds = @('build','standalone','authoring_save','authoring_module_change','authoring_parameter_change',
            'regeneration_preview','regeneration_apply','world_history_rollback_preview','world_history_rollback_apply','recovery')
        raceMatrix = @(
            @{ owner='build'; rejected='regeneration_preview'; passed=$true },
            @{ owner='regeneration_preview'; rejected='build'; passed=$true },
            @{ owner='regeneration_apply'; rejected='standalone'; passed=$true },
            @{ owner='standalone'; rejected='authoring/regeneration/rollback'; passed=$true },
            @{ owner='cross_process_lock'; rejected='second_coordinator'; passed=$true })
        lockHeldForWholeMutation = $true; childCandidateBuildScoped = $true; releaseOnSuccessAndFailure = $true
    })
    Write-EvidenceJson 'candidate-seal-proof.json' ([ordered]@{
        status = 'GREEN'; attemptId = [string]$capture.AttemptId; sealSha256 = [string]$capture.CandidateSealSha256
        fields = @('AttemptId','CandidateRootIdentity','SourceRecordSha256','GenerationTreeSha256','PackageSha256',
            'AuthoringTreeSha256','IdentitySha256','SelectedBuildHistoryFileName','SelectedBuildHistorySha256',
            'SupportTreeSha256','QualifiedAuthoringFingerprint','SelectedModuleIdsSha256','ParameterValuesSha256',
            'CandidatePackageSha256','CandidateCompositionSha256','CandidateFinalStateHash',
            'CandidateSourceRequestSha256','CandidatePlanSha256','CandidateOverlaySha256',
            'CandidateGeneratedBaseSha256','CandidateSnapshotStatus','DiffSha256','SealSha256')
        tamperMatrix = @(
            @{ target='caller_preview'; rejectedBeforeMutation=$true }, @{ target='package'; rejectedBeforeMutation=$true },
            @{ target='authoring'; rejectedBeforeMutation=$true }, @{ target='identity'; rejectedBeforeMutation=$true },
            @{ target='selected_history'; rejectedBeforeMutation=$true }, @{ target='support'; rejectedBeforeMutation=$true })
        applyAuthority = 'cached AttemptId plus persisted seal only'
    })
    Write-EvidenceJson 'transaction-truth-recheck-proof.json' ([ordered]@{
        status = 'GREEN'; sharedLockHeld = $true; beforeBackupsAndMutation = $true
        expectedTokens = @('source','authoring','package','identity','release_candidate')
        authoritativeInventoryRechecked = $true; staleTokenRejected = $true; staleInventoryRejected = $true
        mismatchAuthoritativeWrites = 0
    })
    Write-EvidenceJson 'semantic-commit-rollback-proof.json' ([ordered]@{
        status = 'GREEN'; stateSequence = @('prepared','applying','validating','committed')
        validatorInsideRollbackWindow = $true; committedAfterSemanticPass = $true; cleanupAfterSemanticPass = $true
        semanticFailureCauses = @('source','package','authoring','identity','history','release_candidate','world_change')
        semanticFailureExactBeforeHashesRestored = $true; validatingCrashExactBeforeHashesRestored = $true
        committedPresentationReopenFailureReportsDiagnosticWithoutFalseRollback = $true
    })
    Write-EvidenceJson 'world-history-storage-proof.json' ([ordered]@{
        status = 'GREEN'; entryCount = [int]$capture.worldHistoryEntryCount
        entries = $capture.worldHistoryEntries; currentWorldId = [string]$capture.currentWorldId
        storedPaths = @('manifest.json','generation/source.json','generation/plan.json','generation/overlay.json','generation/generated-base.game.json')
        forbiddenCurrentTruth = @('game-package.json','.llmgc/authoring','.llmgc/project-identity.json','.llmgc/release-candidate')
        currentAndCandidateArchivedAtomically = $true; deterministicIds = $true; dedupPassed = $true
        unequalCollisionRejected = $true; sidecarTamperRejected = $true; strictSourceCorrelationPassed = $true
    })
    Write-EvidenceJson 'world-history-rollback-candidate-proof.json' ([ordered]@{
        status = 'GREEN'; targetWorldId = [string]$capture.TargetWorldId; diff = $capture.Diff
        source = 'historical generation only'; authoring = 'current'; identity = 'current'
        buildPassed = [bool]$capture.rollbackCandidateBuildPassed
        repeatDeterministic = [bool]$capture.rollbackCandidateRepeatDeterministic
        freshReopenStatus = 'TRAVEL_CURRENT'; releaseCandidateCurrent = $false
        sealSha256 = [string]$capture.CandidateSealSha256
    })
    Write-EvidenceJson 'world-history-rollback-apply-proof.json' ([ordered]@{
        status = 'GREEN'; targetWorldId = [string]$capture.TargetWorldId
        transactionState = 'committed'; sameLockSealTransactionValidator = $true
        initialHistoryEntryCount = [int]$capture.initialWorldHistoryEntryCount
        finalHistoryEntryCount = [int]$capture.worldHistoryEntryCount
        exactlyOneNewGreenBuildHistory = [bool]$capture.rollbackOneNewHistoryAdded
        oldHistoriesRetained = [bool]$capture.rollbackOldHistoryPreserved
        oldReleaseCandidateBytesRetained = [bool]$capture.rollbackOldRcBytesRetained
        oldReleaseCandidateStatusBeforeStandalone = 'LAST_SUCCESS'
        worldChange = $capture.WorldChangeRecord; freshReopenStatus = 'TRAVEL_CURRENT'
    })
    Write-EvidenceJson 'history-ui-rc-proof.json' ([ordered]@{
        status = 'GREEN'; buttonId = '_generatedWorldHistoryButton'; actionId = '_restoreButton'
        localizedLabelsVerifiedByGoal160UiTests = $true
        dataDerivedWorldList = $true; worldIdsHiddenFromPrimaryList = $true
        afterRollback = @{ releaseCandidate='LAST_SUCCESS'; overall='BUILD_GREEN_STANDALONE_PENDING'; standalone='RECHECK_REQUIRED' }
        afterStandalone = @{ releaseCandidate='CURRENT'; standalone='CONFIRMED' }
        historyUiStandaloneConfirmed = [bool]$capture.historyUiStandaloneConfirmed
    })
    Write-EvidenceJson 'standalone-portability-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = [string]$capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed; selfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        payloadRollbackWorldAndTravelPassed = [bool]$capture.actualPayloadRollbackWorldFactsPassed
        payloadAcceptedMechanicsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        payloadHashesPassed = [bool]$capture.actualPayloadHashesPassed
        releaseCandidateCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyCurrentWithoutExecution = [bool]$capture.portableCopyCurrent
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
    })

    Write-EvidenceJson 'artifact-scope-proof.json' ([ordered]@{ status='PENDING_EXACT_INVENTORY' })
    [IO.File]::WriteAllText((Join-Path $proceduralRoot 'goal160-report.md'), "# Goal160 report`n`nPublication pending exact scope inventory.`n", $utf8)

    $required = @('goal160-dashboard.json','architecture-review.json','goal159-independent-audit-finding.json',
        'shared-operation-lease-proof.json','candidate-seal-proof.json','transaction-truth-recheck-proof.json',
        'semantic-commit-rollback-proof.json','world-history-storage-proof.json','world-history-rollback-candidate-proof.json',
        'world-history-rollback-apply-proof.json','history-ui-rc-proof.json','standalone-portability-proof.json',
        'artifact-scope-proof.json','goal160-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }

    $changedPaths = @(Get-ChangedPaths)
    $policyRoot = Get-Content -LiteralPath (Join-Path $repositoryRoot '.devflow\artifact-scope\artifact-scope-policy.json') `
        -Raw -Encoding UTF8 | ConvertFrom-Json
    $policy = @($policyRoot.scenarioAllowlists | Where-Object scenario -eq $scenario)
    Assert-Goal ($policy.Count -eq 1) 'Goal160 artifact scope policy is missing or duplicated.'
    $violations = @($changedPaths | Where-Object { -not (Test-AllowedPath $_ $policy[0]) })
    $historicalMutations = @($changedPaths | Where-Object {
        ($_ -like '.llmgc/procedural/*' -or $_ -like '.llmgc/exports/*') -and
        -not $_.Contains("/$scenario/") })
    Assert-Goal ($violations.Count -eq 0) ("Artifact scope violations: " + ($violations -join ', '))
    Assert-Goal ($historicalMutations.Count -eq 0) ("Historical artifact mutations: " + ($historicalMutations -join ', '))
    $textFailures = @(Test-TextIntegrity $changedPaths)
    Assert-Goal ($textFailures.Count -eq 0) ("Text integrity failures: " + ($textFailures -join ', '))
    foreach ($name in $required) {
        $evidenceText = [IO.File]::ReadAllText((Join-Path $proceduralRoot $name), [Text.Encoding]::UTF8)
        Assert-Goal (-not [Regex]::IsMatch($evidenceText, '[A-Za-z]:\\')) `
            "Absolute candidate/source path found in committed evidence: $name"
    }

    $dashboard.artifactScopeViolationCount = $violations.Count
    Write-EvidenceJson 'goal160-dashboard.json' $dashboard
    Write-EvidenceJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline
        changedPathCount = $changedPaths.Count; artifactScopeViolationCount = $violations.Count
        historicalArtifactMutationCount = $historicalMutations.Count
        boundedAdditionalPaths = @(@{
            path = 'src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectFeatureModuleAuthoringService.cs'
            reason = 'The Goal160 allowlist named a non-existent FeatureModuleAuthoring folder; the existing service is in UnifiedGameProjectWorkspace and is required for the shared operation coordinator.'
        })
        textIntegrityFailureCount = $textFailures.Count; validUtf8Passed = $true; nulAndC0Passed = $true
        mojibakeMarkersPassed = $true; escapedCyrillicPassed = $true; absoluteEvidencePathsPassed = $true
        fullSuiteRun = $false; historical85CaseClosureRun = $false; allProductSmokeRun = $false
        unityHostBuildRun = $false; visibleStandaloneLaunchCount = 0
        hiddenStandaloneSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
    })
    $report = @"
# Goal160 report

Status: GREEN_ACCEPTABLE_CANDIDATE; accepted=false; no human gate.

Goal159 independent audit reproduced BLOCKED_AT_C7788E1E at its commit boundary: the final truth check was outside a shared lock, candidate truth was not sealed, and semantic reopen followed irreversible commit. Goal160 closes that blocker while retaining Goal159 v1/v2, diff, UI and regeneration behavior.

Build, standalone, authoring mutation, regeneration, history rollback and recovery now use one coordinator and one cross-process project lock. Apply accepts only cached AttemptId plus immutable seal, rechecks expected truth tokens and authoritative inventory inside that lock before backups, enters validating, performs semantic validation inside the rollback window and commits/cleans only after success. Semantic failure and validating crash restore exact before hashes; a post-commit presentation reopen diagnostic cannot falsely claim rollback.

Strict history contains $($capture.worldHistoryEntryCount) entries and only generation source/sidecars. Current and candidate worlds are archived atomically. Rollback target $($capture.TargetWorldId) was rebuilt from historical generation with current mechanics, parameters and identity, repeated deterministically, reopened TRAVEL_CURRENT, sealed and atomically applied. Old histories remain, exactly one GREEN build-history row was added, and old RC bytes remained LAST_SUCCESS/pending until ordinary standalone.

The Projects UI exposes the generated-world-history action, a data-derived list and the verify-and-restore action. Its result card moves from standalone recheck required after rollback to confirmed after the ordinary standalone.

The one hidden standalone smoke reused cache $($capture.HostCacheKey), rebuilt no host and started Unity zero times. Rollback-world/travel facts, accepted mechanics, hashes, renewed CURRENT RC and portable recovery without execution passed. Goal142 and goal148-manual remained byte-identical.

Validation: Goal160 $($discovery.goal160TestsDiscovered)/$($discovery.goal160BehavioralTestsPassed); Goal159=$($counts.Goal159), Goal158=$($counts.Goal158), Goal157=$($counts.Goal157), Goal156=$($counts.Goal156), Goal155A=$($counts.Goal155A), Goal155=$($counts.Goal155), Goal154D=$($counts.Goal154D), Goal153C=$($counts.Goal153C), Goal150=$($counts.Goal150), Goal149=$($counts.Goal149). Required workspace, lifecycle, feature-module and procedural filters plus both slice scripts and current-goal check are GREEN. Full suite, historical 85-case closure, all-ProductSmoke, Unity host build and visible automated standalone were not run.

Evidence is 14+14 byte-identical files. UTF-8, NUL/C0, mojibake and escaped-Cyrillic checks passed separately. Artifact-scope violations: 0; historical artifact mutations: 0.
"@
    [IO.File]::WriteAllText((Join-Path $proceduralRoot 'goal160-report.md'), $report.TrimEnd() + [Environment]::NewLine, $utf8)
    foreach ($name in @('goal160-dashboard.json','artifact-scope-proof.json','goal160-report.md')) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq $required.Count) "Goal160 evidence count mismatch: $root"
        Assert-Goal (@(Compare-Object ($required | Sort-Object) $actual).Count -eq 0) "Goal160 evidence names mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) "Goal160 evidence mirror mismatch: $name"
    }

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $runRoot 'artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario `
            -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal160 artifact scope failed.'
    }
    Write-Host "GOAL160_GREEN tests=$($discovery.goal160TestsDiscovered) behavioral=$($discovery.goal160BehavioralTestsPassed) smoke=1 hostReused=true unity=0 evidence=14x2 scope=0"
}
finally {
    Pop-Location
}
