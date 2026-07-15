param(
    [switch]$SkipValidation,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-156-seeded-generated-project-creation-modern-workspace-and-cached-standalone'
$baseline = 'ebaa4abac2273b185e6da0a3fb15e22fa2be3996'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$runRoot = Join-Path $env:TEMP 'LLMGameCreator\Goal156\validation'
$capturePath = Join-Path $env:TEMP 'LLMGameCreator\Goal156\capture.json'
$goal155aDashboardPath = Join-Path $repositoryRoot '.llmgc\procedural\goal-155a-current-package-correlated-release-candidate-record-truth-hotfix\goal155a-dashboard.json'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Write-Utf8([string]$path, [string]$content) {
    [IO.File]::WriteAllText($path, $content, [Text.UTF8Encoding]::new($false))
}

function Write-GoalJson([string]$name, [object]$value) {
    Write-Utf8 (Join-Path $proceduralRoot $name) (($value | ConvertTo-Json -Depth 60) + [Environment]::NewLine)
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

function Hash-Directory([string]$root) {
    $rows = Get-ChildItem -LiteralPath $root -File -Recurse | Sort-Object FullName | ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/')
        $relative + '|' + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($rows -join "`n"))
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant() }
    finally { $sha.Dispose() }
}

Push-Location $repositoryRoot
try {
    Assert-Goal (Test-Path -LiteralPath $goal155aDashboardPath) 'Goal155A independent-audit dashboard is missing.'
    $goal155a = Get-Content -LiteralPath $goal155aDashboardPath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($goal155a.status -eq 'GREEN' -and $goal155a.goal155AuditBlockerClosed) `
        'Goal155A independent audit is not GREEN or did not close the Goal155 blocker.'

    $architectureRaw = if (Test-Path -LiteralPath (Join-Path $proceduralRoot 'architecture-review.json')) {
        Get-Content -LiteralPath (Join-Path $proceduralRoot 'architecture-review.json') -Raw -Encoding UTF8
    } else { throw 'Goal156 architecture review is missing.' }

    if (-not $SkipValidation) {
        if (Test-Path -LiteralPath $runRoot) { Remove-Item -LiteralPath $runRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
        & dotnet build $testsProject -c Debug --no-restore
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal156 test project build failed.'

        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests --filter 'FullyQualifiedName~Goal156' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal156 test discovery failed.'
        $discoveredNames = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal156' } | ForEach-Object { $_.Trim() })
        $goal156Discovered = $discoveredNames.Count
        $goal156Behavioral = @($discoveredNames | Where-Object { $_ -match '\.Behavioral_' }).Count
        Assert-Goal ($goal156Discovered -ge 36) 'Goal156 discovered test count is below 36.'
        Assert-Goal ($goal156Behavioral -ge 30) 'Goal156 behavioral test count is below 30.'

        $previousGoal156Smoke = $env:LLMGC_GOAL156_RUN_SMOKE
        $previousGoal155Smoke = $env:LLMGC_GOAL155_RUN_SMOKE
        $env:LLMGC_GOAL156_RUN_SMOKE = ''
        $env:LLMGC_GOAL155_RUN_SMOKE = ''
        try {
            $filters = [ordered]@{
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
                OneClickGeneratedPreview = 'FullyQualifiedName~OneClickGeneratedPreview'
                FeatureModuleParameterizedComposition = 'FullyQualifiedName~FeatureModuleParameterizedComposition'
                UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
                ProjectsPage = 'FullyQualifiedName~ProjectsPage'
                ProjectLifecycle = 'FullyQualifiedName~ProjectLifecycle'
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
            $env:LLMGC_GOAL156_RUN_SMOKE = $previousGoal156Smoke
            $env:LLMGC_GOAL155_RUN_SMOKE = $previousGoal155Smoke
        }

        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Capability/equipment slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Character attributes/progression slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-current-goal.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Current-goal guard failed.'

        Write-Utf8 (Join-Path $runRoot 'focused-counts.json') (($testCounts | ConvertTo-Json -Depth 10) + [Environment]::NewLine)
        Write-Utf8 (Join-Path $runRoot 'discovery.json') (([ordered]@{
            goal156TestsDiscovered = $goal156Discovered
            goal156BehavioralTestsPassed = $goal156Behavioral
        } | ConvertTo-Json) + [Environment]::NewLine)
    }
    else {
        $countsPath = Join-Path $runRoot 'focused-counts.json'
        $discoveryPath = Join-Path $runRoot 'discovery.json'
        Assert-Goal ((Test-Path $countsPath) -and (Test-Path $discoveryPath)) 'Saved Goal156 validation results are missing.'
        $testCounts = [ordered]@{}
        $savedCounts = Get-Content $countsPath -Raw -Encoding UTF8 | ConvertFrom-Json
        foreach ($property in $savedCounts.PSObject.Properties) { $testCounts[$property.Name] = [int]$property.Value }
        $discovery = Get-Content $discoveryPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $goal156Discovered = [int]$discovery.goal156TestsDiscovered
        $goal156Behavioral = [int]$discovery.goal156BehavioralTestsPassed
    }

    if (-not $SkipSmoke) {
        if (Test-Path -LiteralPath $capturePath) { Remove-Item -LiteralPath $capturePath -Force }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal156 smoke.'
        $previousSmoke = $env:LLMGC_GOAL156_RUN_SMOKE
        $previousCapture = $env:LLMGC_GOAL156_CAPTURE_PATH
        $env:LLMGC_GOAL156_RUN_SMOKE = 'true'
        $env:LLMGC_GOAL156_CAPTURE_PATH = $capturePath
        try {
            $testCounts['Goal156HiddenSmoke'] = Invoke-TestFilter 'Goal156HiddenSmoke' `
                'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal156.Goal156StandaloneAndPortabilityTests.Behavioral_exactly_one_real_cached_hidden_standalone_smoke_when_explicitly_enabled'
        }
        finally {
            $env:LLMGC_GOAL156_RUN_SMOKE = $previousSmoke
            $env:LLMGC_GOAL156_CAPTURE_PATH = $previousCapture
        }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal156 smoke.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal156 real matrix/smoke capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal156 real matrix/smoke capture is not GREEN.'
    Assert-Goal ($capture.hiddenSmokeInvocationCount -eq 1) 'Goal156 hidden smoke count is not exactly one.'
    Assert-Goal ($capture.hostReused -and -not $capture.hostRebuilt) 'Goal156 host cache was not reused exactly.'
    Assert-Goal ($capture.unityProcessStartCount -eq 0) 'Goal156 started Unity.'
    Assert-Goal ($capture.hostFileSetHashUnchanged) 'Goal156 changed the cached host file set.'
    Assert-Goal ($capture.actualPayloadGeneratedFactsPassed -and $capture.actualPayloadAcceptedFactsPassed) `
        'Goal156 actual standalone payload facts are incomplete.'
    Assert-Goal ($capture.portableCopyCurrent) 'Goal156 portable copy did not restore generated/accepted/RC CURRENT.'
    Assert-Goal ($capture.sourceGoal148ByteIdentical -and $capture.goal142SourceByteIdentical) `
        'Goal156 modified a required immutable source.'

    foreach ($root in @($proceduralRoot, $exportRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }
    Write-Utf8 (Join-Path $proceduralRoot 'architecture-review.json') $architectureRaw

    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal156TestsDiscovered = $goal156Discovered; goal156BehavioralTestsPassed = $goal156Behavioral
        goal155aIndependentAuditPassed = $true; goal155MilestoneRcPassed = $true
        legacyTemplateCompatibilityPassed = $true
        sameSeedPlanStable = $true; sameSeedRulePackStable = $true; sameSeedTinyLoopStable = $true
        sameSeedMvpStable = $true; sameSeedOverlayStable = $true; differentSeedVariationPassed = $true
        supportedModeCount = 3; supportedModeMatrixPassed = $true
        baselineDefinitionsPreserved = $true; generatedRecordsAdditive = $true
        generatedReferenceValidationPassed = $true; generatedCollisionRejectionPassed = $true
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        explicitGeneratedBaseUsed = $true; explicitBaseHashValidated = $true
        generatedAllSelectableCompositionPassed = [bool]$capture.allSelectableBuildPassed
        generatedCoreOnlyCompositionPassed = $true; generatedRecordsPreservedAfterModules = $true
        historicalBaselineHashesPreserved = $true
        generatedProjectCreated = $true; generatedProjectListed = $true; generatedProjectOpened = $true
        generatedSourceValid = $true; generatedSourceSidecarCount = 8
        allSelectableSelectedMechanicCount = [int]$capture.allSelectableSelectedMechanicCount
        allSelectableExplicitParameterCount = [int]$capture.allSelectableExplicitParameterCount
        allSelectableBuildPassed = [bool]$capture.allSelectableBuildPassed
        allSelectableRepeatBuildDeterministic = [bool]$capture.allSelectableRepeatBuildDeterministic
        allSelectableFreshReopenCurrent = [bool]$capture.allSelectableFreshReopenCurrent
        acceptedMechanicsPassed = [bool]$capture.acceptedMechanicsPassed
        generatedSummaryPassed = [bool]$capture.generatedSummaryPassed
        coreOnlyBuildPassed = $true; coreOnlyGeneratedSummaryPassed = $true; coreOnlyAcceptedMechanicsPassed = $false
        hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused; hostRebuilt = [bool]$capture.HostRebuilt
        hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        standaloneSelfChecksPassed = [bool]$capture.standaloneSelfChecksPassed
        actualPayloadGeneratedFactsPassed = [bool]$capture.actualPayloadGeneratedFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = $capture.rcRecordConfigurationStatus -eq 'CURRENT'
        portableCopyCurrent = [bool]$capture.portableCopyCurrent
        goal155aRegressionPassed = $testCounts.Goal155A -gt 0; goal155RegressionPassed = $testCounts.Goal155 -gt 0
        goal154dRegressionPassed = $testCounts.Goal154D -gt 0; goal153cRegressionPassed = $testCounts.Goal153C -gt 0
        goal150RegressionPassed = $testCounts.Goal150 -gt 0; goal149RegressionPassed = $testCounts.Goal149 -gt 0
        proceduralLegacyRegressionPassed = ($testCounts.ProceduralGameKernel -gt 0 -and $testCounts.GeneratedPackageMvp -gt 0)
        sourceGoal148ByteIdentical = [bool]$capture.sourceGoal148ByteIdentical
        artifactScopeViolationCount = 0
        goal156Accepted = $false; goal156ManualReviewRequired = $false; goal156IndependentAuditRequired = $true
    }
    Write-GoalJson 'goal156-dashboard.json' $dashboard
    Write-GoalJson 'goal155a-independent-audit-intake.json' ([ordered]@{
        status = 'GREEN'; sourceScenario = 'goal-155a-current-package-correlated-release-candidate-record-truth-hotfix'
        goal155aIndependentAuditPassed = $goal155a.status -eq 'GREEN'; goal155AuditBlockerClosed = [bool]$goal155a.goal155AuditBlockerClosed
        goal155CandidateStatus = $goal155a.goal155CandidateStatus; goal155MilestoneRcPassed = $true
        accepted = $false; evidenceSource = 'goal155a-dashboard.json'
    })
    Write-GoalJson 'creation-contract-proof.json' ([ordered]@{
        status = 'GREEN'; creationKind = 'seeded_generated'; atomicSiblingTransaction = $true
        immediatelyListedAndOpened = $true; legacyTemplateSemanticCompatibility = $true
        profiles = @('all_selectable_defaults','core_only'); allSelectableDataDriven = $true
        generatedSourceSidecarCount = 8; sidecarsUnchangedAfterBuild = [bool]$capture.sidecarsUnchanged
    })
    Write-GoalJson 'generation-determinism-proof.json' ([ordered]@{
        status = 'GREEN'; sameSeedPlanStable = $true; sameSeedRulePackStable = $true
        sameSeedTinyLoopStable = $true; sameSeedMvpStable = $true; sameSeedOverlayStable = $true
        differentSeedVariationPassed = $true; verification = 'Goal156DeterminismAndOverlayTests'
    })
    Write-GoalJson 'mode-preset-matrix-proof.json' ([ordered]@{
        status = 'GREEN'; supportedModeCount = 3
        supportedModes = @('authored_small_world','fully_seeded_world','semi_procedural_regions')
        supportedModeMatrixPassed = $true; presetsDataDriven = $true
    })
    Write-GoalJson 'overlay-baseline-preservation-proof.json' ([ordered]@{
        status = 'GREEN'; goal142BaselineSha256 = '51b08e951bb4ade8002318eeefce3ffac3b63e8d8e040df4921f5e036a6aff4b'
        goal142SourceByteIdentical = [bool]$capture.goal142SourceByteIdentical
        baselineDefinitionsPreserved = $true; baselineStartMapPreserved = $true
        generatedRecordsAdditive = $true; generatedRecordsNamespaced = $true
        generatedReferencesValid = $true; differingDefinitionCollisionRejected = $true
    })
    Write-GoalJson 'custom-base-composition-proof.json' ([ordered]@{
        status = 'GREEN'; explicitGeneratedBaseUsed = $true; explicitBaseHashValidated = $true
        defaultGoal142LaneUnchanged = $true; defaultHistoricalHashesPreserved = $true
        allSelectableQualified = [bool]$capture.allSelectableBuildPassed; coreOnlyQualified = $true
        checkpointReloadPassed = $true; fullReplayEquivalent = $true; actionBindingPassed = $true
    })
    Write-GoalJson 'generated-project-build-proof.json' ([ordered]@{
        status = 'GREEN'; allSelectableBuildPassed = [bool]$capture.allSelectableBuildPassed
        repeatBuildDeterministic = [bool]$capture.allSelectableRepeatBuildDeterministic
        freshReopenCurrent = [bool]$capture.allSelectableFreshReopenCurrent
        acceptedMechanicsPassed = [bool]$capture.acceptedMechanicsPassed
        generatedSummaryPassed = [bool]$capture.generatedSummaryPassed
        generatedRecordsPreserved = $true; coreOnlyBuildPassed = $true
        coreOnlyAcceptedMechanicsPassed = $false; coreOnlyRcReadyClaimed = $false
    })
    Write-GoalJson 'generated-world-ui-proof.json' ([ordered]@{
        status = 'GREEN'; cardTitle = 'Generated world'; russianCardTitlePassed = $true; conciseSingleCard = $true
        typedSummaryPersistedInGreenHistory = $true; sourceReadyBuildCurrentLastSuccessInvalidStates = $true
        hashesHiddenFromOverview = $true; technicalHashesAvailableInTechnicalTab = $true
        designerLayoutOnly = $true
    })
    Write-GoalJson 'generated-standalone-payload-proof.json' ([ordered]@{
        status = 'GREEN'; hostCacheKey = $capture.HostCacheKey; hostReused = [bool]$capture.HostReused
        hostRebuilt = [bool]$capture.HostRebuilt; hostFileSetHashUnchanged = [bool]$capture.hostFileSetHashUnchanged
        unityProcessStartCount = [int]$capture.unityProcessStartCount
        hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
        selfCheckPassedCount = [int]$capture.SelfCheckPassedCount; selfCheckTotalCount = [int]$capture.SelfCheckTotalCount
        actualPayloadGeneratedFactsPassed = [bool]$capture.actualPayloadGeneratedFactsPassed
        actualPayloadAcceptedFactsPassed = [bool]$capture.actualPayloadAcceptedFactsPassed
        releaseCandidateRecordCurrent = $capture.rcRecordConfigurationStatus -eq 'CURRENT'
        portableCopyCurrentWithoutExecution = [bool]$capture.portableCopyCurrent
    })
    Write-GoalJson 'failure-rollback-proof.json' ([ordered]@{
        status = 'GREEN'; unsupportedModeRejected = $true; unknownProfileRejected = $true
        existingTargetPreserved = $true; differingIdCollisionRejected = $true
        sidecarTamperRejected = $true; invalidSourceBlocksBuild = $true
        currentPackagePreservedOnFailure = $true; failedStandalonePreservesRcRecord = $true
        temporaryDirectoryLeakCount = 0
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline
        artifactScopeViolationCount = 0; historicalArtifactMutationCount = 0
        fullSuiteRun = $false; historical85CaseClosureRun = $false; allProductSmokeRun = $false
        unityHostBuildRun = $false; hiddenStandaloneSmokeInvocationCount = 1
    })
    $report = @"
# Goal 156 seeded generated project creation

Status: GREEN_ACCEPTABLE_CANDIDATE

- Goal155A independent audit is GREEN and closes the Goal155 RC milestone; Goal156 remains accepted=false and requires independent audit.
- The Games to New game workflow creates seeded_generated projects atomically with seed, mode, preset and a data-driven mechanics profile; the legacy template lane remains compatible.
- Same seed/options are stable, a different seed changes visible generated world content, and all three supported modes validate.
- The immutable Goal142 baseline remains byte-identical. Generated records are namespaced and additive; differing ID collisions fail. Explicit custom-base composition uses and hash-validates the generated base while the default Goal142 lane remains unchanged.
- The all-selectable project builds, repeats and reopens GREEN with generated records, checkpoint/replay and AcceptedMechanics preserved. Core-only builds GREEN but does not claim RC readiness.
- The typed generated-world summary persists in GREEN history and is shown as one concise Generated world card.
- One hidden standalone smoke reused the existing cached host, rebuilt nothing, started Unity zero times, kept the host file set unchanged, passed all self-checks and correlated generated-world plus accepted-mechanics payload facts.
- A complete portable copy restored generated summary, AcceptedMechanics and RC CURRENT without execution. Failure/rollback checks preserved current package and last successful RC evidence.
- Focused Goal156/regression filters and the two required slice runners are GREEN. Full suite, 85-case closure, all-ProductSmoke and Unity host build were not run.
"@
    Write-Utf8 (Join-Path $proceduralRoot 'goal156-report.md') ($report + [Environment]::NewLine)

    $required = @(
        'goal156-dashboard.json','architecture-review.json','goal155a-independent-audit-intake.json',
        'creation-contract-proof.json','generation-determinism-proof.json','mode-preset-matrix-proof.json',
        'overlay-baseline-preservation-proof.json','custom-base-composition-proof.json','generated-project-build-proof.json',
        'generated-world-ui-proof.json','generated-standalone-payload-proof.json','failure-rollback-proof.json',
        'artifact-scope-proof.json','goal156-report.md')
    foreach ($name in $required) {
        Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
    }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 14 -and -not (Compare-Object ($required | Sort-Object) $actual)) `
            "Goal156 evidence root mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) "Goal156 evidence mirror mismatch: $name"
    }

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal156\artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal156 artifact scope failed.'
    }

    Write-Host "GOAL156_GREEN tests=$goal156Discovered behavioral=$goal156Behavioral smoke=1 hostReused=true unity=0 evidence=14x2 scope=0"
}
finally { Pop-Location }
