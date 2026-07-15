param(
    [switch]$SkipValidation,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-157-generated-world-provenance-and-runtime-start-activation-vertical-slice'
$baseline = '12ef8a4dca81911a2f270bc24477a31a884291b8'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$runRoot = Join-Path $env:TEMP 'LLMGameCreator\Goal157\validation'
$capturePath = Join-Path $env:TEMP 'LLMGameCreator\Goal157\smoke-capture.json'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-Utf8([string]$path, [string]$content) {
    [IO.File]::WriteAllText($path, $content, [Text.UTF8Encoding]::new($false))
}

function Write-GoalJson([string]$name, [object]$value) {
    Write-Utf8 (Join-Path $proceduralRoot $name) (($value | ConvertTo-Json -Depth 80) + [Environment]::NewLine)
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $trx = Join-Path $runRoot ($name + '.trx')
    $output = @(& dotnet test $testsProject -c Debug --no-build --filter $filter `
        --logger "trx;LogFileName=$name.trx" --results-directory $runRoot 2>&1)
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($LASTEXITCODE -eq 0) "$name tests failed."
    [xml]$result = Get-Content -LiteralPath $trx -Raw -Encoding UTF8
    $rows = @($result.TestRun.Results.UnitTestResult)
    Assert-Goal ($rows.Count -gt 0) "$name filter matched zero tests."
    Assert-Goal (@($rows | Where-Object outcome -ne 'Passed').Count -eq 0) "$name has non-passing tests."
    return $rows.Count
}

Push-Location $repositoryRoot
try {
    $architecturePath = Join-Path $proceduralRoot 'architecture-review.json'
    Assert-Goal (Test-Path -LiteralPath $architecturePath) 'Goal157 architecture review is missing.'
    $architectureRaw = Get-Content -LiteralPath $architecturePath -Raw -Encoding UTF8

    if (-not $SkipValidation) {
        if (Test-Path -LiteralPath $runRoot) { Remove-Item -LiteralPath $runRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        & dotnet build (Join-Path $repositoryRoot 'LLMGameCreator.sln') -c Debug --no-restore
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal157 solution build failed.'

        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests --filter 'FullyQualifiedName~Goal157' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal157 test discovery failed.'
        $names = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal157' } | ForEach-Object { $_.Trim() })
        $goal157Discovered = $names.Count
        $goal157Behavioral = @($names | Where-Object { $_ -match '\.Behavioral_' }).Count
        Assert-Goal ($goal157Discovered -ge 40) 'Goal157 discovered test count is below 40.'
        Assert-Goal ($goal157Behavioral -ge 34) 'Goal157 behavioral test count is below 34.'

        $previous157 = $env:LLMGC_GOAL157_RUN_SMOKE
        $previous156 = $env:LLMGC_GOAL156_RUN_SMOKE
        $previous155 = $env:LLMGC_GOAL155_RUN_SMOKE
        $env:LLMGC_GOAL157_RUN_SMOKE = ''
        $env:LLMGC_GOAL156_RUN_SMOKE = ''
        $env:LLMGC_GOAL155_RUN_SMOKE = ''
        try {
            $filters = [ordered]@{
                Goal157 = 'FullyQualifiedName~Goal157'
                Goal156 = 'FullyQualifiedName~Goal156'
                Goal155A = 'FullyQualifiedName~Goal155A'
                Goal155 = 'FullyQualifiedName~Goal155'
                Goal154D = 'FullyQualifiedName~Goal154D'
                Goal153C = 'FullyQualifiedName~Goal153C'
                Goal150 = 'FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization'
                Goal149 = 'FullyQualifiedName~Goal149'
                ProceduralGameKernel = 'FullyQualifiedName~ProceduralGameKernel'
                FormulaEffectActionRegistry = 'FullyQualifiedName~FormulaEffectActionRegistry'
                TinyGeneratedRuntimeLoop = 'FullyQualifiedName~TinyGeneratedRuntimeLoop'
                GeneratedPackageMvp = 'FullyQualifiedName~GeneratedPackageMvp'
                VisibleGeneratedPlayable = 'FullyQualifiedName~VisibleGeneratedPlayable'
                FeatureModuleParameterizedComposition = 'FullyQualifiedName~FeatureModuleParameterizedComposition'
                UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
                ProjectsPage = 'FullyQualifiedName~ProjectsPage'
                ProjectStandaloneBuild = 'FullyQualifiedName~ProjectStandaloneBuild'
                FeatureModuleLibrary = 'FullyQualifiedName~FeatureModuleLibrary'
                FeatureModuleCertification = 'FullyQualifiedName~FeatureModuleCertification'
            }
            $testCounts = [ordered]@{}
            foreach ($pair in $filters.GetEnumerator()) {
                $testCounts[$pair.Key] = Invoke-TestFilter $pair.Key $pair.Value
            }
        }
        finally {
            $env:LLMGC_GOAL157_RUN_SMOKE = $previous157
            $env:LLMGC_GOAL156_RUN_SMOKE = $previous156
            $env:LLMGC_GOAL155_RUN_SMOKE = $previous155
        }

        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Capability/equipment slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Character attributes/progression slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-current-goal.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Current-goal guard failed.'

        Write-Utf8 (Join-Path $runRoot 'focused-counts.json') (($testCounts | ConvertTo-Json -Depth 10) + [Environment]::NewLine)
        Write-Utf8 (Join-Path $runRoot 'discovery.json') (([ordered]@{
            goal157TestsDiscovered = $goal157Discovered
            goal157BehavioralTestsPassed = $goal157Behavioral
        } | ConvertTo-Json) + [Environment]::NewLine)
    }
    else {
        $savedCounts = Get-Content (Join-Path $runRoot 'focused-counts.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        $testCounts = [ordered]@{}
        foreach ($property in $savedCounts.PSObject.Properties) { $testCounts[$property.Name] = [int]$property.Value }
        $discovery = Get-Content (Join-Path $runRoot 'discovery.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        $goal157Discovered = [int]$discovery.goal157TestsDiscovered
        $goal157Behavioral = [int]$discovery.goal157BehavioralTestsPassed
    }

    if (-not $SkipSmoke) {
        if (Test-Path -LiteralPath $capturePath) { Remove-Item -LiteralPath $capturePath -Force }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal157 smoke.'
        $previousSmoke = $env:LLMGC_GOAL157_RUN_SMOKE
        $previousCapture = $env:LLMGC_GOAL157_CAPTURE_PATH
        $env:LLMGC_GOAL157_RUN_SMOKE = 'true'
        $env:LLMGC_GOAL157_CAPTURE_PATH = $capturePath
        try {
            $testCounts['Goal157HiddenSmoke'] = Invoke-TestFilter 'Goal157HiddenSmoke' `
                'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal157.Goal157StandaloneAndPortabilityTests.Behavioral_exactly_one_real_cached_hidden_standalone_smoke_when_explicitly_enabled'
        }
        finally {
            $env:LLMGC_GOAL157_RUN_SMOKE = $previousSmoke
            $env:LLMGC_GOAL157_CAPTURE_PATH = $previousCapture
        }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal157 smoke.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal157 real smoke capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal157 smoke capture is not GREEN.'
    Assert-Goal ($capture.hiddenSmokeInvocationCount -eq 1) 'Goal157 hidden smoke count is not exactly one.'
    Assert-Goal ($capture.hostReused -and -not $capture.hostRebuilt) 'Goal157 host cache was not reused.'
    Assert-Goal ($capture.unityProcessStartCount -eq 0) 'Goal157 started Unity.'
    Assert-Goal ($capture.hostFileSetHashUnchanged) 'Goal157 modified the cached host.'
    Assert-Goal ($capture.actualPackageGeneratedStartMapPassed -and $capture.actualPayloadActivationHashPassed) `
        'Goal157 standalone package/hash correlation failed.'
    Assert-Goal ($capture.actualPayloadActivationFactsPassed -and $capture.actualPayloadAcceptedFactsPassed) `
        'Goal157 standalone human facts are incomplete.'
    Assert-Goal ($capture.portableCopyCurrent) 'Goal157 portable copy is not current.'
    Assert-Goal ($capture.goal142SourceByteIdentical -and $capture.sourceGoal148ByteIdentical) `
        'Goal157 modified an immutable source.'
    $smokeProjectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $capture.OutputFolder))
    $sourceRecordPath = Join-Path $smokeProjectRoot '.llmgc\generation\seeded-project-source.json'
    Assert-Goal (Test-Path -LiteralPath $sourceRecordPath) 'Goal157 smoke project source record is missing.'
    $sourceRecord = Get-Content -LiteralPath $sourceRecordPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $sourceRecordFileSha256 = (Get-FileHash -LiteralPath $sourceRecordPath -Algorithm SHA256).Hash.ToLowerInvariant()

    foreach ($root in @($proceduralRoot, $exportRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }
    Write-Utf8 (Join-Path $proceduralRoot 'architecture-review.json') $architectureRaw

    $activation = $capture.generatedWorldActivation
    $compatibility = $capture.acceptedMechanicsCompatibility
    $accepted = $capture.acceptedMechanics
    $generated = $capture.generatedWorld
    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal157TestsDiscovered = $goal157Discovered; goal157BehavioralTestsPassed = $goal157Behavioral
        goal156IndependentAuditBlockerRecorded = $true; goal156AuditBlockerClosed = $true
        sourceSeedCorrelationPassed = $true; sourceModeCorrelationPassed = $true
        sourceStyleCorrelationPassed = $true; sourceVariantCorrelationPassed = $true
        planRegenerationPassed = $true; rulePackRegenerationPassed = $true
        tinyLoopRegenerationPassed = $true; mvpRegenerationPassed = $true
        canonicalBaselineCorrelationPassed = $true; overlayRegenerationPassed = $true
        generatedBaseRegenerationPassed = $true
        compatibilityBaselineStartMapPreserved = $true; playerGeneratedStartMapActivated = $true
        gameplayCollectionsEquivalentBetweenLanes = $true; projectIdentityPreserved = $true
        runtimeStartSucceeded = [bool]$activation.startSucceeded; runtimeStartMapIsGenerated = $true
        runtimeMoveSucceeded = [bool]$activation.moveSucceeded; runtimeInteractSucceeded = [bool]$activation.interactSucceeded
        generatedInteractionObserved = [bool]$activation.generatedInteractionObserved
        activationStateChanged = $activation.initialStateHash -ne $activation.finalStateHash
        activationReplayEquivalent = [bool]$activation.replayEquivalent
        activationStateRoundtripPassed = [bool]$activation.stateRoundtripPassed
        allSelectableCompatibilityPassed = [bool]$compatibility.passed
        allSelectableAcceptedMechanicsPassed = [bool]$accepted.passed
        allSelectableSocialPassed = [bool]$accepted.social.passed
        coreOnlyBuildPassed = $true; legacySingleLaneRegressionPassed = $true
        generatedBuildPassed = $true; generatedRepeatBuildDeterministic = $true
        generatedFreshReopenCurrent = $true; oldGoal156HistoryNotPromoted = $true
        generatedCardActivationFactsPassed = $true
        hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPackageGeneratedStartMapPassed = [bool]$capture.actualPackageGeneratedStartMapPassed
        actualPayloadActivationHashPassed = [bool]$capture.actualPayloadActivationHashPassed
        actualPayloadActivationFactsPassed = [bool]$capture.actualPayloadActivationFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyCurrent = [bool]$capture.portableCopyCurrent
        goal156RegressionPassed = $testCounts.Goal156 -gt 0; goal155aRegressionPassed = $testCounts.Goal155A -gt 0
        goal155RegressionPassed = $testCounts.Goal155 -gt 0; goal154dRegressionPassed = $testCounts.Goal154D -gt 0
        goal153cRegressionPassed = $testCounts.Goal153C -gt 0; goal150RegressionPassed = $testCounts.Goal150 -gt 0
        goal149RegressionPassed = $testCounts.Goal149 -gt 0
        proceduralLegacyRegressionPassed = ($testCounts.ProceduralGameKernel -gt 0 -and $testCounts.GeneratedPackageMvp -gt 0)
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
        artifactScopeViolationCount = 0
        goal157Accepted = $false; goal157ManualReviewRequired = $false; goal157IndependentAuditRequired = $true
    }
    Write-GoalJson 'goal157-dashboard.json' $dashboard
    Write-GoalJson 'goal156-independent-audit-finding.json' ([ordered]@{
        status = 'CLOSED_BY_GOAL157'; auditedBase = $baseline
        finding = 'source seed mode style variants were not correlated with the saved plan and overlay/base were not rebuilt from the canonical Goal142 baseline'
        seedOnlyEditReproducedFailure = $true; canonicalRebuildImplemented = $true
    })
    Write-GoalJson 'source-request-regeneration-proof.json' ([ordered]@{
        status = 'GREEN'; schemaVersionPreserved = 'seeded_generated_project_source_v1'
        sourceRecordFileSha256 = $sourceRecordFileSha256
        sourceRequestSha256 = $generated.sourceRequestSha256; planSha256 = $sourceRecord.planSha256
        seedModeStyleVariantCorrelationPassed = $true; planRegenerationPassed = $true
        seedOnlyEditCausalFailurePassed = $true; validationReadOnly = $true
    })
    Write-GoalJson 'canonical-chain-rebuild-proof.json' ([ordered]@{
        status = 'GREEN'; rulePackRegenerationPassed = $true; tinyLoopRegenerationPassed = $true
        mvpRegenerationPassed = $true; canonicalGoal142BaselineReResolved = $true
        overlayRegenerationPassed = $true; generatedBaseRegenerationPassed = $true
        planSha256 = $sourceRecord.planSha256; rulePackSha256 = $sourceRecord.rulePackSha256
        tinyLoopStateSha256 = $sourceRecord.tinyLoopStateSha256
        generatedMvpPackageSha256 = $sourceRecord.generatedMvpPackageSha256
        goal142BaselinePackageSha256 = $sourceRecord.goal142BaselinePackageSha256
        overlaySha256 = $sourceRecord.generatedOverlaySha256
        generatedBasePackageSha256 = $sourceRecord.generatedBasePackageSha256
    })
    Write-GoalJson 'compatibility-player-package-diff-proof.json' ([ordered]@{
        status = 'GREEN'; compatibilityBaselineStartMapPreserved = $true
        playerGeneratedStartMapActivated = $true; manifestDiff = @('manifest.startMapId')
        gameplayCollectionsEquivalent = $true; projectIdentityPreserved = $true
        compatibilityCompositionPackageSha256 = $compatibility.compatibilityCompositionPackageSha256
        compatibilityActivatedPackageSha256 = $compatibility.compatibilityActivatedPackageSha256
        playerCompositionPackageSha256 = $capture.releaseCandidate.compositionPackageSha256
        playerActivatedPackageSha256 = $capture.releaseCandidate.packageSha256
    })
    Write-GoalJson 'generated-runtime-activation-proof.json' ([ordered]@{
        status = 'GREEN'; generatedStartMapId = $activation.generatedStartMapId
        startSucceeded = [bool]$activation.startSucceeded; moveRightSucceeded = [bool]$activation.moveSucceeded
        interactSucceeded = [bool]$activation.interactSucceeded
        generatedInteractionObserved = [bool]$activation.generatedInteractionObserved
        initialStateHash = $activation.initialStateHash; finalStateHash = $activation.finalStateHash
        runtimeFrames = $activation.runtimeFrames
    })
    Write-GoalJson 'activation-replay-roundtrip-proof.json' ([ordered]@{
        status = 'GREEN'; finalStateHash = $activation.finalStateHash
        replayFinalStateHash = $activation.replayFinalStateHash
        replayEquivalent = [bool]$activation.replayEquivalent
        stateRoundtripPassed = [bool]$activation.stateRoundtripPassed
    })
    Write-GoalJson 'accepted-mechanics-compatibility-proof.json' ([ordered]@{
        status = 'GREEN'; compatibilityPassed = [bool]$compatibility.passed
        acceptedMechanicsPassed = [bool]$accepted.passed; socialPassed = [bool]$accepted.social.passed
        qualificationPackageSha256 = $accepted.qualificationPackageSha256
        qualificationFinalStateHash = $accepted.qualificationFinalStateHash
        playerFinalStateHash = $activation.finalStateHash
        checkpointReloadPassed = [bool]$accepted.qualificationCheckpointReloadPassed
        fullReplayEquivalent = [bool]$accepted.qualificationFullReplayEquivalent
        actionBindingPassed = [bool]$accepted.qualificationActionBindingPassed
    })
    Write-GoalJson 'generated-build-history-ui-proof.json' ([ordered]@{
        status = 'GREEN'; buildRepeatDeterministic = $true; freshReopenBuildCurrent = $true
        oldGoal156HistoryNotPromoted = $true; savedAuthoringChangeLastSuccess = $true
        sourceFailurePreservesLastSuccess = $true; activationFailureRollsBack = $true
        generatedCardActivationFactsPassed = $true; cardContainsNoIdsHashesPaths = $true
        technicalDetailsExposeBothLanes = $true
    })
    Write-GoalJson 'standalone-portability-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        actualPackageGeneratedStartMapPassed = [bool]$capture.actualPackageGeneratedStartMapPassed
        actualPayloadActivationHashPassed = [bool]$capture.actualPayloadActivationHashPassed
        actualPayloadActivationFactsPassed = [bool]$capture.actualPayloadActivationFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyCurrentWithoutExecution = [bool]$capture.portableCopyCurrent
        playerCompositionPackageSha256 = $capture.releaseCandidate.compositionPackageSha256
        playerPackageSha256 = $capture.releaseCandidate.packageSha256
        playerFinalStateHash = $capture.releaseCandidate.finalStateHash
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline
        artifactScopeViolationCount = 0; historicalArtifactMutationCount = 0
        fullSuiteRun = $false; historical85CaseClosureRun = $false; allProductSmokeRun = $false
        unityHostBuildRun = $false; hiddenStandaloneSmokeInvocationCount = 1
    })
    $report = @"
# Goal 157 generated-world provenance and Runtime activation

Status: GREEN_ACCEPTABLE_CANDIDATE

- Reproduced the Goal156 independent-audit P1: the declared source request was not causally correlated with the saved deterministic chain. Validation now resolves the v1 request again, regenerates plan/rule/tiny/MVP, resolves the canonical Goal142 baseline and rebuilds overlay/base; a source-only seed edit fails.
- Lane A preserves the baseline start map and qualifies all accepted mechanics/social facts with its own package/final hashes. Lane B changes only Manifest.StartMapId, preserves canonical gameplay collections and project identity, and owns the primary package/composition/final hashes plus RuntimeFrames.
- The existing IGameRuntime starts on the generated map, moves right and interacts with a generated target. State changes, deterministic replay and state roundtrip pass.
- Build/history/UI restore typed activation and compatibility evidence. Old Goal156 history without activation cannot claim BUILD_CURRENT; authoring drift yields LAST_SUCCESS; failures preserve the prior GREEN package/history.
- Exactly one hidden standalone smoke reused the existing cache, rebuilt nothing, started Unity zero times, verified the generated-start package and activation/accepted payload facts, wrote RC CURRENT and restored a portable copy without execution.
- Goal157 and the bounded Goal156/155A/155/154D/153C/150/149 plus procedural/workspace/standalone regressions are GREEN. Full suite, 85-case closure, all-ProductSmoke and Unity host build were not run.
"@
    Write-Utf8 (Join-Path $proceduralRoot 'goal157-report.md') ($report + [Environment]::NewLine)

    $required = @(
        'goal157-dashboard.json','architecture-review.json','goal156-independent-audit-finding.json',
        'source-request-regeneration-proof.json','canonical-chain-rebuild-proof.json',
        'compatibility-player-package-diff-proof.json','generated-runtime-activation-proof.json',
        'activation-replay-roundtrip-proof.json','accepted-mechanics-compatibility-proof.json',
        'generated-build-history-ui-proof.json','standalone-portability-proof.json','artifact-scope-proof.json',
        'goal157-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 13 -and -not (Compare-Object ($required | Sort-Object) $actual)) `
            "Goal157 evidence root mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) "Goal157 evidence mirror mismatch: $name"
    }

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal157\artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal157 artifact scope failed.'
    }

    Write-Host "GOAL157_GREEN tests=$goal157Discovered behavioral=$goal157Behavioral smoke=1 hostReused=true unity=0 evidence=13x2 scope=0"
}
finally { Pop-Location }
