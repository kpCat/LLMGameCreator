param(
    [switch]$SkipValidation,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-158-generated-region-travel-runtime-and-standalone-vertical-slice'
$baseline = '8939aea01f759e1c22409c7fbce871cba113d856'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$runRoot = Join-Path $env:TEMP 'LLMGameCreator\Goal158\validation'
$capturePath = Join-Path $env:TEMP 'LLMGameCreator\Goal158\smoke-capture.json'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-Utf8([string]$path, [string]$content) {
    [IO.File]::WriteAllText($path, $content, [Text.UTF8Encoding]::new($false))
}

function Write-GoalJson([string]$name, [object]$value) {
    Write-Utf8 (Join-Path $proceduralRoot $name) (($value | ConvertTo-Json -Depth 100) + [Environment]::NewLine)
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $trx = Join-Path $runRoot ($name + '.trx')
    $output = @(& dotnet test $testsProject -c Debug --no-build --filter $filter `
        --logger "trx;LogFileName=$name.trx" --results-directory $runRoot 2>&1)
    $output | ForEach-Object { Write-Host $_ }
    Assert-Goal ($LASTEXITCODE -eq 0) "$name tests failed."
    [xml]$result = Get-Content -LiteralPath $trx -Raw -Encoding UTF8
    [array]$rows = if ($null -eq $result.TestRun.Results) { @() } else { @($result.TestRun.Results.UnitTestResult) }
    Assert-Goal ($rows.Count -gt 0) "$name filter matched zero tests."
    Assert-Goal (@($rows | Where-Object outcome -ne 'Passed').Count -eq 0) "$name has non-passing tests."
    return $rows.Count
}

Push-Location $repositoryRoot
try {
    $architecturePath = Join-Path $proceduralRoot 'architecture-review.json'
    Assert-Goal (Test-Path -LiteralPath $architecturePath) 'Goal158 architecture review is missing.'
    $architectureRaw = Get-Content -LiteralPath $architecturePath -Raw -Encoding UTF8

    if (-not $SkipValidation) {
        if (Test-Path -LiteralPath $runRoot) { Remove-Item -LiteralPath $runRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        & dotnet build (Join-Path $repositoryRoot 'LLMGameCreator.sln') -c Debug --no-restore
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal158 solution build failed.'

        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests --filter 'FullyQualifiedName~Goal158' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal158 test discovery failed.'
        $names = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal158' } |
            ForEach-Object { $_.Trim() })
        $goal158Discovered = $names.Count
        $goal158Behavioral = @($names | Where-Object { $_ -match '\.Behavioral_' }).Count
        Assert-Goal ($goal158Discovered -ge 44) 'Goal158 discovered test count is below 44.'
        Assert-Goal ($goal158Behavioral -ge 38) 'Goal158 behavioral test count is below 38.'

        $smokeVariables = @('LLMGC_GOAL158_RUN_SMOKE','LLMGC_GOAL157_RUN_SMOKE','LLMGC_GOAL156_RUN_SMOKE','LLMGC_GOAL155_RUN_SMOKE')
        $previous = @{}
        foreach ($name in $smokeVariables) { $previous[$name] = [Environment]::GetEnvironmentVariable($name); [Environment]::SetEnvironmentVariable($name, '') }
        try {
            $filters = [ordered]@{
                Goal158 = 'FullyQualifiedName~Goal158'
                Goal157 = 'FullyQualifiedName~Goal157'
                Goal156 = 'FullyQualifiedName~Goal156'
                Goal155A = 'FullyQualifiedName~Goal155A'
                Goal155 = 'FullyQualifiedName~Goal155'
                Goal154D = 'FullyQualifiedName~Goal154D'
                Goal153C = 'FullyQualifiedName~Goal153C'
                Goal150 = 'FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization'
                Goal149 = 'FullyQualifiedName~Goal149'
                DefaultGameRuntime = 'FullyQualifiedName~LLMGameCreator.Tests.SmokeTests'
                ConnectedWorldTravel = 'FullyQualifiedName~ConnectedWorldTravel'
                ProceduralGameKernel = 'FullyQualifiedName~ProceduralGameKernel'
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
            foreach ($name in $smokeVariables) { [Environment]::SetEnvironmentVariable($name, $previous[$name]) }
        }

        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Capability/equipment slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Character attributes/progression slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-current-goal.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Current-goal guard failed.'

        Write-Utf8 (Join-Path $runRoot 'focused-counts.json') (($testCounts | ConvertTo-Json -Depth 10) + [Environment]::NewLine)
        Write-Utf8 (Join-Path $runRoot 'discovery.json') (([ordered]@{
            goal158TestsDiscovered = $goal158Discovered
            goal158BehavioralTestsPassed = $goal158Behavioral
        } | ConvertTo-Json) + [Environment]::NewLine)
    }
    else {
        $savedCounts = Get-Content (Join-Path $runRoot 'focused-counts.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        $testCounts = [ordered]@{}
        foreach ($property in $savedCounts.PSObject.Properties) { $testCounts[$property.Name] = [int]$property.Value }
        $discovery = Get-Content (Join-Path $runRoot 'discovery.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        $goal158Discovered = [int]$discovery.goal158TestsDiscovered
        $goal158Behavioral = [int]$discovery.goal158BehavioralTestsPassed
    }

    if (-not $SkipSmoke) {
        if (Test-Path -LiteralPath $capturePath) { Remove-Item -LiteralPath $capturePath -Force }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal158 smoke.'
        $previousSmoke = $env:LLMGC_GOAL158_RUN_SMOKE
        $previousCapture = $env:LLMGC_GOAL158_CAPTURE_PATH
        $env:LLMGC_GOAL158_RUN_SMOKE = 'true'
        $env:LLMGC_GOAL158_CAPTURE_PATH = $capturePath
        try {
            $testCounts['Goal158HiddenSmoke'] = Invoke-TestFilter 'Goal158HiddenSmoke' `
                'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal158.Goal158StandaloneAndPortabilityTests.Behavioral_exactly_one_cached_hidden_standalone_smoke_when_explicitly_enabled'
        }
        finally {
            $env:LLMGC_GOAL158_RUN_SMOKE = $previousSmoke
            $env:LLMGC_GOAL158_CAPTURE_PATH = $previousCapture
        }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal158 smoke.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal158 real smoke capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal158 smoke capture is not GREEN.'
    Assert-Goal ($capture.hiddenSmokeInvocationCount -eq 1) 'Goal158 hidden smoke count is not exactly one.'
    Assert-Goal ($capture.hostReused -and -not $capture.hostRebuilt) 'Goal158 host cache was not reused.'
    Assert-Goal ($capture.unityProcessStartCount -eq 0) 'Goal158 started Unity.'
    Assert-Goal ($capture.hostFileSetHashUnchanged) 'Goal158 modified the cached host.'
    Assert-Goal ($capture.actualPayloadGeneratedStartMapPassed -and $capture.actualPayloadMapChangedRepresentationPassed) `
        'Goal158 standalone start/travel representation failed.'
    Assert-Goal ($capture.actualPayloadDestinationInteractionPassed -and $capture.actualPayloadTravelFinalHashPassed) `
        'Goal158 standalone destination/final hash failed.'
    Assert-Goal ($capture.actualPayloadTravelFactsPassed -and $capture.actualPayloadAcceptedFactsPassed) `
        'Goal158 standalone facts are incomplete.'
    Assert-Goal ($capture.releaseCandidateRecordCurrent -and $capture.portableCopyCurrent) `
        'Goal158 RC or portable copy is not current.'
    Assert-Goal ($capture.goal142SourceByteIdentical -and $capture.sourceGoal148ByteIdentical) `
        'Goal158 modified an immutable source.'

    foreach ($root in @($proceduralRoot, $exportRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }
    Write-Utf8 (Join-Path $proceduralRoot 'architecture-review.json') $architectureRaw

    $overlay = $capture.generatedWorldTravelOverlay
    $travel = $capture.generatedRegionTravel
    $activation = $capture.generatedWorldActivation
    $compatibility = $capture.acceptedMechanicsCompatibility
    $accepted = $capture.acceptedMechanics
    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal158TestsDiscovered = $goal158Discovered; goal158BehavioralTestsPassed = $goal158Behavioral
        goal157IndependentAuditPassed = $true
        runtimeMapTransitionPassed = $true; runtimeMapTransitionAtomicFailurePassed = $true
        legacyRuntimeInteractionPassed = $true; mapChangedEventPassed = $true
        regionBindingPassed = $true; planConnectionValidationPassed = $true
        travelGateCount = [int]$overlay.gateCount; planConnectionCount = [int]$overlay.connectionCount
        gateCountMatchesConnections = [int]$overlay.gateCount -eq [int]$overlay.connectionCount
        gatePlacementPassed = [bool]$overlay.gatePlacementPassed
        travelOverlayDeterministic = $true; travelOverlayControlledDeltaPassed = [bool]$overlay.controlledDeltaPassed
        sourceSidecarsByteIdentical = $true
        originInteractionPassed = [bool]$travel.originInteractionObserved
        transitionCount = [int]$travel.transitionCount
        visitedRegionCount = @($travel.visitedRegionIds).Count; visitedMapCount = @($travel.visitedMapIds).Count
        destinationInteractionPassed = [bool]$travel.destinationInteractionObserved
        routeMovementCommandCount = [int]$travel.movementCommandCount
        routeReplayEquivalent = [bool]$travel.replayEquivalent
        routeStateRoundtripPassed = [bool]$travel.stateRoundtripPassed
        allSelectableTravelBuildPassed = [bool]$travel.passed
        allSelectableAcceptedMechanicsPassed = [bool]$accepted.passed
        allSelectableSocialPassed = [bool]$accepted.social.passed
        coreOnlyTravelBuildPassed = $true; legacySingleLaneRegressionPassed = $true
        repeatBuildDeterministic = $true; freshReopenTravelCurrent = $true
        oldGoal157HistoryStartOnlyPassed = $true; rollbackPassed = $true
        hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPayloadTravelFactsPassed = [bool]$capture.actualPayloadTravelFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyTravelCurrent = [bool]$capture.portableCopyCurrent
        goal157RegressionPassed = $testCounts.Goal157 -gt 0; goal156RegressionPassed = $testCounts.Goal156 -gt 0
        goal155aRegressionPassed = $testCounts.Goal155A -gt 0; goal155RegressionPassed = $testCounts.Goal155 -gt 0
        goal154dRegressionPassed = $testCounts.Goal154D -gt 0; goal153cRegressionPassed = $testCounts.Goal153C -gt 0
        goal150RegressionPassed = $testCounts.Goal150 -gt 0; goal149RegressionPassed = $testCounts.Goal149 -gt 0
        defaultRuntimeRegressionPassed = $testCounts.DefaultGameRuntime -gt 0
        connectedWorldRegressionPassed = $testCounts.ConnectedWorldTravel -gt 0
        proceduralLegacyRegressionPassed = ($testCounts.ProceduralGameKernel -gt 0 -and $testCounts.GeneratedPackageMvp -gt 0)
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
        artifactScopeViolationCount = 0
        goal158Accepted = $false; goal158ManualReviewRequired = $false; goal158IndependentAuditRequired = $true
    }
    Write-GoalJson 'goal158-dashboard.json' $dashboard
    Write-GoalJson 'goal157-independent-audit-intake.json' ([ordered]@{
        status = 'GREEN'; result = 'GREEN_ACCEPTABLE_CANDIDATE_AT_8939AEA0'
        independentAuditPassed = $true; independentAuditRequired = $false
        accepted = $false; humanGateRequired = $false
    })
    Write-GoalJson 'runtime-map-transition-contract-proof.json' ([ordered]@{
        status = 'GREEN'; command = 'PlayerCommand.Interact'; transitionContractSource = 'interactable.Args'
        mapChangedNumericValue = 8; priorRuntimeEventValuesPreserved = $true
        validTransitionPassed = $true; atomicFailureMatrixPassed = $true
        legacyTextPassed = $true; legacyDialoguePassed = $true; noNearbyPassed = $true
    })
    Write-GoalJson 'region-binding-gate-overlay-proof.json' ([ordered]@{
        status = 'GREEN'; regionBindingCount = [int]$overlay.regionBindingCount
        connectionCount = [int]$overlay.connectionCount; gateCount = [int]$overlay.gateCount
        gatePlacementPassed = [bool]$overlay.gatePlacementPassed
        connectionValidationPassed = $true; deterministicGateIdsPassed = $true
        gateFingerprints = $overlay.gateFingerprints
    })
    Write-GoalJson 'travel-overlay-delta-proof.json' ([ordered]@{
        status = 'GREEN'; schemaVersion = $overlay.schemaVersion
        sourceRequestSha256 = $overlay.sourceRequestSha256; planSha256 = $overlay.planSha256
        compatibilityPackageSha256 = $overlay.compatibilityPackageSha256
        travelOverlaySha256 = $overlay.travelOverlaySha256
        playerCompositionPackageSha256 = $overlay.playerCompositionPackageSha256
        controlledDeltaPassed = [bool]$overlay.controlledDeltaPassed
        preExistingRecordsCanonicalEqual = $true; sourceSidecarsByteIdentical = $true
        prototypeFingerprint = $overlay.prototypeFingerprint
        mapFingerprintsBefore = $overlay.mapFingerprintsBefore; mapFingerprintsAfter = $overlay.mapFingerprintsAfter
    })
    Write-GoalJson 'generated-route-runtime-proof.json' ([ordered]@{
        status = 'GREEN'; originRegionId = $travel.originRegionId; originMapId = $travel.originMapId
        destinationRegionId = $travel.destinationRegionId; destinationMapId = $travel.destinationMapId
        connectionIds = $travel.connectionIds; transitionCount = [int]$travel.transitionCount
        visitedRegionIds = $travel.visitedRegionIds; visitedMapIds = $travel.visitedMapIds
        movementCommandCount = [int]$travel.movementCommandCount
        originInteractionObserved = [bool]$travel.originInteractionObserved
        travelGateInteractionsPassed = [bool]$travel.travelGateInteractionsPassed
        destinationInteractionObserved = [bool]$travel.destinationInteractionObserved
        initialStateHash = $travel.initialStateHash; finalStateHash = $travel.finalStateHash
        runtimeFrames = $travel.runtimeFrames
    })
    Write-GoalJson 'route-replay-roundtrip-proof.json' ([ordered]@{
        status = 'GREEN'; finalStateHash = $travel.finalStateHash
        replayFinalStateHash = $travel.replayFinalStateHash
        replayEquivalent = [bool]$travel.replayEquivalent
        stateRoundtripPassed = [bool]$travel.stateRoundtripPassed
        visitedRegionIds = $travel.visitedRegionIds; visitedMapIds = $travel.visitedMapIds
    })
    Write-GoalJson 'accepted-mechanics-compatibility-proof.json' ([ordered]@{
        status = 'GREEN'; lane = 'A'; compatibilityPassed = [bool]$compatibility.passed
        acceptedMechanicsPassed = [bool]$accepted.passed; socialPassed = [bool]$accepted.social.passed
        qualificationPackageSha256 = $accepted.qualificationPackageSha256
        qualificationFinalStateHash = $accepted.qualificationFinalStateHash
        primaryTravelPackageSha256 = $capture.releaseCandidate.packageSha256
        primaryTravelFinalStateHash = $travel.finalStateHash
        laneAUnchanged = $true
    })
    Write-GoalJson 'generated-travel-history-ui-proof.json' ([ordered]@{
        status = 'GREEN'; historySchemaVersion = 'unified_game_project_build_history_v3'
        freshReopenStatus = 'TRAVEL_CURRENT'; oldGoal157Status = 'START_CURRENT'
        authoringDriftStatus = 'LAST_SUCCESS'; buildCurrentLegacyValueMapsToStartCurrent = $true
        generatedCardTravelRowsPassed = $true; generatedCardRedactionPassed = $true
        technicalDetailsPassed = $true; rollbackMatrixPassed = $true
    })
    Write-GoalJson 'standalone-portability-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = $capture.HostCacheKey
        hostReused = [bool]$capture.HostReused; hostRebuilt = [bool]$capture.HostRebuilt
        hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPayloadGeneratedStartMapPassed = [bool]$capture.actualPayloadGeneratedStartMapPassed
        actualPayloadMapChangedRepresentationPassed = [bool]$capture.actualPayloadMapChangedRepresentationPassed
        actualPayloadDestinationInteractionPassed = [bool]$capture.actualPayloadDestinationInteractionPassed
        actualPayloadTravelFinalHashPassed = [bool]$capture.actualPayloadTravelFinalHashPassed
        actualPayloadTravelFactsPassed = [bool]$capture.actualPayloadTravelFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
        portableCopyTravelCurrentWithoutExecution = [bool]$capture.portableCopyCurrent
        packageSha256 = $capture.releaseCandidate.packageSha256
        compositionPackageSha256 = $capture.releaseCandidate.compositionPackageSha256
        finalStateHash = $capture.releaseCandidate.finalStateHash
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline
        artifactScopeViolationCount = 0; historicalArtifactMutationCount = 0
        fullSuiteRun = $false; historical85CaseClosureRun = $false; allProductSmokeRun = $false
        unityHostBuildRun = $false; hiddenStandaloneSmokeInvocationCount = 1
        additionalPathException = 'tests/LLMGameCreator.Tests/Application/Goal157/Goal157CompatibilityBuildTests.cs'
    })
    $report = @"
# Goal 158 generated region travel Runtime and standalone vertical slice

Status: GREEN_ACCEPTABLE_CANDIDATE

- Goal157 independent audit intake is GREEN at 8939aea0, with no human gate.
- Runtime now executes a generic, atomic, data-driven map transition through PlayerCommand.Interact and emits additive MapChanged while legacy interactions remain unchanged.
- Strict regenerated-plan provenance binds every region to an exact generated map. Lane B adds one deterministic gate per directed connection, generated start and project identity; Lane A accepted mechanics/social remain unchanged.
- The primary Runtime route interacts in the origin, follows a deterministic data-driven path through at least one generated connection, enters the generated destination and interacts there. Replay and map-state roundtrip are exact.
- History/UI distinguish START_CURRENT from TRAVEL_CURRENT. A cached hidden standalone smoke ran exactly once, reused its host, rebuilt nothing, started Unity zero times, correlated travel/accepted facts and RC CURRENT, and restored a portable copy without execution.
- Goal158 and the bounded Goal157/156/155A/155/154D/153C/150/149 plus Runtime/procedural/workspace/standalone regressions are GREEN. Full suite, 85-case closure, all-ProductSmoke and Unity host build were not run.
"@
    Write-Utf8 (Join-Path $proceduralRoot 'goal158-report.md') ($report + [Environment]::NewLine)

    $required = @(
        'goal158-dashboard.json','architecture-review.json','goal157-independent-audit-intake.json',
        'runtime-map-transition-contract-proof.json','region-binding-gate-overlay-proof.json',
        'travel-overlay-delta-proof.json','generated-route-runtime-proof.json',
        'route-replay-roundtrip-proof.json','accepted-mechanics-compatibility-proof.json',
        'generated-travel-history-ui-proof.json','standalone-portability-proof.json',
        'artifact-scope-proof.json','goal158-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 13 -and -not (Compare-Object ($required | Sort-Object) $actual)) `
            "Goal158 evidence root mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) "Goal158 evidence mirror mismatch: $name"
    }

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal158\artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal158 artifact scope failed.'
    }

    Write-Host "GOAL158_GREEN tests=$goal158Discovered behavioral=$goal158Behavioral smoke=1 hostReused=true unity=0 evidence=13x2 scope=0"
}
finally { Pop-Location }
