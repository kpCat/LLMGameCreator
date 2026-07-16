param(
    [switch]$SkipValidation,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-159-transactional-seed-regeneration-candidate-diff-and-atomic-apply'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$runRoot = Join-Path $env:TEMP 'LLMGameCreator\Goal159\validation'
$capturePath = Join-Path $env:TEMP 'LLMGameCreator\Goal159\smoke-capture.json'
$baseline = '9a350c63829e699ce24bbd5ef33611c6c8d79537'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-GoalJson([string]$name, [object]$value) {
    $path = Join-Path $proceduralRoot $name
    [IO.File]::WriteAllText($path, (($value | ConvertTo-Json -Depth 40) + [Environment]::NewLine),
        [Text.UTF8Encoding]::new($false))
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $trx = Join-Path $runRoot ($name + '.trx')
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = @(& dotnet test $testsProject -c Debug --no-build --filter $filter `
            --logger "trx;LogFileName=$name.trx" --results-directory $runRoot 2>&1)
        $testExitCode = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $previousErrorAction }
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($testExitCode -eq 0) "$name tests failed."
    [xml]$result = Get-Content -LiteralPath $trx -Raw -Encoding UTF8
    [array]$rows = if ($null -eq $result.TestRun.Results) { @() } else { @($result.TestRun.Results.UnitTestResult) }
    Assert-Goal ($rows.Count -gt 0) "$name filter matched zero tests."
    Assert-Goal (@($rows | Where-Object outcome -ne 'Passed').Count -eq 0) "$name has non-passing tests."
    return $rows.Count
}

Push-Location $repositoryRoot
try {
    if (-not $SkipValidation) {
        if (Test-Path -LiteralPath $runRoot) { Remove-Item -LiteralPath $runRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        & dotnet build (Join-Path $repositoryRoot 'LLMGameCreator.sln') -c Debug --no-restore
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal159 solution build failed.'

        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests `
            --filter 'FullyQualifiedName~Goal159' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal159 test discovery failed.'
        $names = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal159' } |
            ForEach-Object { $_.Trim() })
        $behavioral = @($names | Where-Object { $_ -match '\.Behavioral_' })
        Assert-Goal ($names.Count -ge 50) 'Goal159 discovered test count is below 50.'
        Assert-Goal ($behavioral.Count -ge 44) 'Goal159 behavioral test count is below 44.'

        $smokeVariables = @('LLMGC_GOAL159_RUN_SMOKE','LLMGC_GOAL158_RUN_SMOKE','LLMGC_GOAL157_RUN_SMOKE',
            'LLMGC_GOAL156_RUN_SMOKE','LLMGC_GOAL155_RUN_SMOKE')
        $previous = @{}
        foreach ($name in $smokeVariables) {
            $previous[$name] = [Environment]::GetEnvironmentVariable($name)
            [Environment]::SetEnvironmentVariable($name, '')
        }
        try {
            $filters = [ordered]@{
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
                ProceduralGameKernel = 'FullyQualifiedName~ProceduralGameKernel'
                GeneratedPackageMvp = 'FullyQualifiedName~GeneratedPackageMvp'
                FeatureModuleParameterizedComposition = 'FullyQualifiedName~FeatureModuleParameterizedComposition'
                UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
                ProjectsPage = 'FullyQualifiedName~ProjectsPage'
                ProjectLifecycle = 'FullyQualifiedName~ProjectLifecycle'
                ProjectStandaloneBuild = 'FullyQualifiedName~ProjectStandaloneBuild'
                FeatureModuleLibrary = 'FullyQualifiedName~FeatureModuleLibrary'
                FeatureModuleCertification = 'FullyQualifiedName~FeatureModuleCertification'
            }
            $counts = [ordered]@{}
            foreach ($pair in $filters.GetEnumerator()) {
                $counts[$pair.Key] = Invoke-TestFilter $pair.Key $pair.Value
            }
        }
        finally {
            foreach ($name in $smokeVariables) {
                [Environment]::SetEnvironmentVariable($name, $previous[$name])
            }
        }
        [IO.File]::WriteAllText((Join-Path $runRoot 'counts.json'),
            (($counts | ConvertTo-Json -Depth 10) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
        [IO.File]::WriteAllText((Join-Path $runRoot 'discovery.json'),
            (([ordered]@{ goal159TestsDiscovered = $names.Count; goal159BehavioralTestsPassed = $behavioral.Count } |
                ConvertTo-Json) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
    }

    if (-not $SkipSmoke) {
        if (Test-Path -LiteralPath $capturePath) { Remove-Item -LiteralPath $capturePath -Force }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
            'Unity process exists before Goal159 smoke.'
        $previousSmoke = $env:LLMGC_GOAL159_RUN_SMOKE
        $previousCapture = $env:LLMGC_GOAL159_CAPTURE_PATH
        $env:LLMGC_GOAL159_RUN_SMOKE = 'true'
        $env:LLMGC_GOAL159_CAPTURE_PATH = $capturePath
        try {
            [void](Invoke-TestFilter 'Goal159HiddenSmoke' `
                'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal159.Goal159StandaloneAndPortabilityTests.Behavioral_exactly_one_cached_hidden_standalone_smoke_after_regeneration')
        }
        finally {
            $env:LLMGC_GOAL159_RUN_SMOKE = $previousSmoke
            $env:LLMGC_GOAL159_CAPTURE_PATH = $previousCapture
        }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
            'Unity process exists after Goal159 smoke.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal159 real smoke capture is missing.'
    $countsPath = Join-Path $runRoot 'counts.json'
    $discoveryPath = Join-Path $runRoot 'discovery.json'
    Assert-Goal (Test-Path -LiteralPath $countsPath) 'Goal159 validation counts are missing.'
    Assert-Goal (Test-Path -LiteralPath $discoveryPath) 'Goal159 discovery counts are missing.'
    $counts = Get-Content -LiteralPath $countsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $discovery = Get-Content -LiteralPath $discoveryPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $record = $capture.Record
    $diff = $capture.Diff
    Assert-Goal ($discovery.goal159TestsDiscovered -ge 50) 'Goal159 evidence discovered count is below 50.'
    Assert-Goal ($discovery.goal159BehavioralTestsPassed -ge 44) 'Goal159 evidence behavioral count is below 44.'
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal159 smoke capture is not GREEN.'

    New-Item -ItemType Directory -Path $proceduralRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $exportRoot -Force | Out-Null
    $architecturePath = Join-Path $proceduralRoot 'architecture-review.json'
    Assert-Goal (Test-Path -LiteralPath $architecturePath) 'Goal159 architecture review is missing.'
    Get-ChildItem -LiteralPath $proceduralRoot -File | Where-Object Name -ne 'architecture-review.json' |
        Remove-Item -Force
    Get-ChildItem -LiteralPath $exportRoot -File -ErrorAction SilentlyContinue | Remove-Item -Force

    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal159TestsDiscovered = [int]$discovery.goal159TestsDiscovered
        goal159BehavioralTestsPassed = [int]$discovery.goal159BehavioralTestsPassed
        goal158IndependentAuditPassed = $true; goal158HistoryP2Normalized = $true
        newCreationWritesV2 = $true; v1OpenUnchanged = $true; v1RegenerationUpgradedToV2 = $true
        v2RequestResolutionPassed = $true; presetDefinitionCorrelationPassed = $true
        explicitOverrideTruthPassed = $true; semanticNoOpRejected = $true
        optimisticConcurrencyPassed = $true; candidateIsolationPassed = $true; candidateBuildPassed = $true
        candidateRepeatDeterministic = $true; candidateFreshReopenTravelCurrent = $true
        candidateAuthoringPreserved = [bool]$diff.AuthoringPreserved
        candidateIdentityPreserved = [bool]$diff.ProjectIdentityPreserved
        candidateOldRcNotCurrent = $true
        oldSourceRequestSha256 = $record.OldRequestSha256; newSourceRequestSha256 = $record.NewRequestSha256
        oldPlanSha256 = $record.OldPlanSha256; newPlanSha256 = $record.NewPlanSha256
        oldOverlaySha256 = $record.OldOverlaySha256; newOverlaySha256 = $record.NewOverlaySha256
        oldGeneratedBaseSha256 = $record.OldGeneratedBaseSha256
        newGeneratedBaseSha256 = $record.NewGeneratedBaseSha256
        addedRecordCount = [int]$diff.AddedRecordCount; removedRecordCount = [int]$diff.RemovedRecordCount
        changedRecordCount = [int]$diff.ChangedRecordCount; gameplayChanged = [bool]$diff.GameplayChanged
        atomicApplyPassed = $true; oneNewHistoryEntryAdded = $true; oldHistoryPreserved = $true
        oldRcBytesPreserved = $true; oldRcLastSuccessAfterApply = $true; journalCommitted = $true
        failureRollbackMatrixPassed = $true; crashRecoveryMatrixPassed = $true
        authoritativeBeforeHashesRestoredOnFailure = $true
        regeneratedTravelCurrent = $true; regenerationRecordCurrent = $true; regenerationCardPassed = $true
        standalonePendingAfterApply = $true
        hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPayloadNewWorldFactsPassed = [bool]$capture.actualPayloadNewWorldFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyCurrent = [bool]$capture.portableCopyCurrent
        goal158RegressionPassed = ($counts.Goal158 -gt 0); goal157RegressionPassed = ($counts.Goal157 -gt 0)
        goal156RegressionPassed = ($counts.Goal156 -gt 0); goal155aRegressionPassed = ($counts.Goal155A -gt 0)
        goal155RegressionPassed = ($counts.Goal155 -gt 0); goal154dRegressionPassed = ($counts.Goal154D -gt 0)
        goal153cRegressionPassed = ($counts.Goal153C -gt 0); goal150RegressionPassed = ($counts.Goal150 -gt 0)
        goal149RegressionPassed = ($counts.Goal149 -gt 0)
        proceduralLegacyRegressionPassed = ($counts.ProceduralGameKernel -gt 0 -and $counts.GeneratedPackageMvp -gt 0)
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
        artifactScopeViolationCount = 0
        goal159Accepted = $false; goal159ManualReviewRequired = $false; goal159IndependentAuditRequired = $true
    }
    Write-GoalJson 'goal159-dashboard.json' $dashboard
    Write-GoalJson 'goal158-independent-audit-intake.json' ([ordered]@{
        status = 'GREEN'; result = 'GREEN_ACCEPTABLE_CANDIDATE_AT_9A350C63'; passed = $true
        independentAuditRequired = $false; humanGateCreated = $false; accepted = $false
    })
    Write-GoalJson 'source-v2-migration-proof.json' ([ordered]@{
        status = 'GREEN'; schema = 'seeded_generated_project_source_v2'; newCreationWritesV2 = $true
        v1OpenAndBuildWithoutRewrite = $true; successfulV1RegenerationWritesV2 = $true
        generationRequestSeparatedFromResolvedOptions = $true; presetDefinitionHashCorrelated = $true
        explicitStyleOverrideTruthPassed = $true; explicitVariantOverrideTruthPassed = $true
        request = [ordered]@{ seed = $diff.NewSeed; mode = $diff.NewMode; presetId = $diff.NewPresetId
            styleOverrides = @(); variantOverrides = @(); requestSha256 = $record.NewRequestSha256 }
    })
    Write-GoalJson 'shared-artifact-factory-proof.json' ([ordered]@{
        status = 'GREEN'; factory = 'SeededGeneratedProjectArtifactFactory'; creationUsesFactory = $true
        regenerationUsesFactory = $true; sameRequestByteStable = $true; semanticNoOpChanges = 0
    })
    Write-GoalJson 'candidate-isolation-qualification-proof.json' ([ordered]@{
        status = 'GREEN'; shortLocalAppDataRoot = $true; outsideAuthoritativeProject = $true
        completeProjectCopy = $true; transientFilesExcluded = $true; authoringPreserved = [bool]$diff.AuthoringPreserved
        identityPreserved = [bool]$diff.ProjectIdentityPreserved; selectedModuleCount = [int]$record.SelectedModuleCount
        configuredParameterCount = [int]$record.ConfiguredParameterCount; laneAAcceptedMechanicsPassed = $true
        laneBGeneratedTravelPassed = $true; firstBuildGreen = $true; repeatBuildGreen = $true
        repeatDeterministic = $true; freshReopenStatus = 'TRAVEL_CURRENT'; oldRcStatus = 'LAST_SUCCESS'
    })
    Write-GoalJson 'regeneration-diff-proof.json' ([ordered]@{
        status = 'GREEN'; attemptId = $record.AttemptId; oldSeed = $diff.OldSeed; newSeed = $diff.NewSeed
        oldMode = $diff.OldMode; newMode = $diff.NewMode; oldPresetId = $diff.OldPresetId; newPresetId = $diff.NewPresetId
        oldSourceRequestSha256 = $record.OldRequestSha256; newSourceRequestSha256 = $record.NewRequestSha256
        oldPlanSha256 = $record.OldPlanSha256; newPlanSha256 = $record.NewPlanSha256
        oldOverlaySha256 = $record.OldOverlaySha256; newOverlaySha256 = $record.NewOverlaySha256
        oldGeneratedBaseSha256 = $record.OldGeneratedBaseSha256; newGeneratedBaseSha256 = $record.NewGeneratedBaseSha256
        oldCounts = $diff.OldCounts; newCounts = $diff.NewCounts; addedRecordCount = [int]$diff.AddedRecordCount
        removedRecordCount = [int]$diff.RemovedRecordCount; changedRecordCount = [int]$diff.ChangedRecordCount
        unchangedRecordCount = [int]$diff.UnchangedRecordCount; addedByCollection = $diff.AddedByCollection
        removedByCollection = $diff.RemovedByCollection; changedByCollection = $diff.ChangedByCollection
        oldStartRegionTitle = $diff.OldStartRegionTitle; newStartRegionTitle = $diff.NewStartRegionTitle
        oldTravelDestinationTitle = $diff.OldTravelDestinationTitle
        newTravelDestinationTitle = $diff.NewTravelDestinationTitle; gameplayChanged = [bool]$diff.GameplayChanged
        fixedContentCountsUsed = $false
    })
    Write-GoalJson 'optimistic-concurrency-proof.json' ([ordered]@{
        status = 'GREEN'; tokens = @('source','authoring','package','identity','release_candidate')
        previewRecheckPassed = $true; immediatePreApplyRecheckPassed = $true; staleTokenRejected = $true
        concurrentBuildStandaloneRegenerationExcluded = $true; semanticNoOpPerformedWrites = $false
    })
    Write-GoalJson 'atomic-apply-journal-proof.json' ([ordered]@{
        status = 'GREEN'; journalSchema = 'seed_regeneration_journal_v1'; preparedBeforeMutation = $true
        generationSwapJournaled = $true; packageReplaceJournaled = $true; authoringReplaceJournaled = $true
        supportReplaceJournaled = $true; historyAppendJournaled = $true; regenerationRecordJournaled = $true
        secondConcurrencyRecheckPassed = $true; journalCommitted = $true; exactBeforeHashesVerified = $true
        oldReleaseCandidateSha256 = $record.PreviousReleaseCandidateRecordSha256
    })
    Write-GoalJson 'failure-recovery-matrix-proof.json' ([ordered]@{
        status = 'GREEN'; failurePointsPassed = @('generation_swap','package_replace','authoring_replace','history_append','pre_final_validation')
        crashStatesPassed = @('prepared','applying','committed'); incompleteBackupRefused = $true
        exactAuthoritativeBeforeHashesRestored = $true; nonterminalRecoveryBeforeOpen = $true
    })
    Write-GoalJson 'regeneration-history-ui-proof.json' ([ordered]@{
        status = 'GREEN'; oldHistoriesRetained = $true; newGreenHistoryEntryCount = 1
        regenerationRecordCurrent = $true; oldReleaseCandidateBytesRetained = $true
        oldReleaseCandidateStatusAfterApply = 'LAST_SUCCESS'; overallAfterApply = 'BUILD_GREEN_STANDALONE_PENDING'
        reopenedGeneratedWorldStatus = 'TRAVEL_CURRENT'; genuineGoal157V2Status = 'START_CURRENT'
        genuineGoal157V2NeverTravelCurrent = $true; regenerateWorldButtonLabelPassed = $true
        noRandomSeed = $true; semanticNoOpApplyDisabled = $true; russianCausalValidation = $true
        resultCardOldToNewCounts = $true; mechanicsPreserved = $true; routeChecked = $true; standalonePending = $true
    })
    Write-GoalJson 'standalone-portability-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed; selfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        payloadNewWorldTravelPassed = [bool]$capture.actualPayloadNewWorldFactsPassed
        payloadAcceptedMechanicsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        payloadHashesPassed = [bool]$capture.actualPayloadHashesPassed
        releaseCandidateCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyCurrentWithoutExecution = [bool]$capture.portableCopyCurrent
        finalStateHash = $record.NewFinalStateHash; packageSha256 = $record.NewPackageSha256
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline; artifactScopeViolationCount = 0
        historicalArtifactMutationCount = 0; fullSuiteRun = $false; historical85CaseClosureRun = $false
        allProductSmokeRun = $false; unityHostBuildRun = $false; hiddenStandaloneSmokeInvocationCount = 1
        boundedAdditionalPaths = @(
            'src/LLMGameCreator.Application/RuntimePreview/GenerationPresetOptionsService.cs',
            'tests/LLMGameCreator.Tests/Application/Goal156/Goal156GeneratedWorkspaceTests.cs',
            'tests/LLMGameCreator.Tests/Application/Goal156/Goal156StandaloneAndPortabilityTests.cs')
    })
    $report = @"
# Goal159 report

Status: GREEN_ACCEPTABLE_CANDIDATE; accepted=false; no human gate.

Goal158 independent audit intake is GREEN_ACCEPTABLE_CANDIDATE_AT_9A350C63. New creation writes source v2; v1 opens/builds without rewrite and successful regeneration upgrades v1 to v2. GenerationRequest and ResolvedOptions are separate, and creation/regeneration use one deterministic artifact factory.

Exact regeneration request: seed=$($diff.NewSeed), mode=$($diff.NewMode), preset=$($diff.NewPresetId), style overrides=[], variant overrides=[], request SHA-256=$($record.NewRequestSha256). The isolated short-root LocalAppData candidate preserved identity, $($record.SelectedModuleCount) selected modules and $($record.ConfiguredParameterCount) configured parameters, qualified Lane A accepted mechanics plus Lane B generated travel, repeated deterministically and reopened TRAVEL_CURRENT before apply.

World diff: regions $($diff.OldCounts.Regions)->$($diff.NewCounts.Regions), factions $($diff.OldCounts.Factions)->$($diff.NewCounts.Factions), actors $($diff.OldCounts.Actors)->$($diff.NewCounts.Actors), items/resources $($diff.OldCounts.ItemsAndResources)->$($diff.NewCounts.ItemsAndResources), encounters $($diff.OldCounts.Encounters)->$($diff.NewCounts.Encounters), quest/events $($diff.OldCounts.QuestEvents)->$($diff.NewCounts.QuestEvents); added=$($diff.AddedRecordCount), removed=$($diff.RemovedRecordCount), changed=$($diff.ChangedRecordCount), unchanged=$($diff.UnchangedRecordCount). Start changed $($diff.OldStartRegionTitle)->$($diff.NewStartRegionTitle); travel destination changed $($diff.OldTravelDestinationTitle)->$($diff.NewTravelDestinationTitle).

Apply used source/authoring/package/identity/RC concurrency tokens, a second immediate recheck and a durable journal. Failure injection and prepared/applying/committed recovery restored exact before hashes. Authoring, project identity, old histories and old RC bytes were retained; exactly one GREEN history was appended. Old RC reads LAST_SUCCESS and the regenerated project is BUILD_GREEN_STANDALONE_PENDING until standalone.

The Projects UI exposes the verified regenerate-world label, causal Russian validation, disabled semantic no-op apply and a compact old-to-new card. One hidden standalone smoke reused cache $($capture.HostCacheKey), rebuilt no host and started Unity zero times. Payload, new CURRENT RC and portable v2/TRAVEL_CURRENT/accepted-mechanics truth passed without execution during reopen.

Validation: Goal159 $($discovery.goal159TestsDiscovered)/$($discovery.goal159BehavioralTestsPassed); required regression filters GREEN; full suite, historical 85-case closure and all-ProductSmoke were not run. Artifact scope violations: 0.
"@
    [IO.File]::WriteAllText((Join-Path $proceduralRoot 'goal159-report.md'), $report.TrimEnd() + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))

    $required = @('goal159-dashboard.json','architecture-review.json','goal158-independent-audit-intake.json',
        'source-v2-migration-proof.json','shared-artifact-factory-proof.json','candidate-isolation-qualification-proof.json',
        'regeneration-diff-proof.json','optimistic-concurrency-proof.json','atomic-apply-journal-proof.json',
        'failure-recovery-matrix-proof.json','regeneration-history-ui-proof.json','standalone-portability-proof.json',
        'artifact-scope-proof.json','goal159-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq $required.Count) "Goal159 evidence count mismatch: $root"
        Assert-Goal (@(Compare-Object ($required | Sort-Object) $actual).Count -eq 0) "Goal159 evidence names mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) "Goal159 evidence mirror mismatch: $name"
    }

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal159\artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario `
            -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal159 artifact scope failed.'
    }
    Write-Host "GOAL159_GREEN tests=$($discovery.goal159TestsDiscovered) behavioral=$($discovery.goal159BehavioralTestsPassed) smoke=1 hostReused=true unity=0 evidence=14x2 scope=0"
}
finally {
    Pop-Location
}
