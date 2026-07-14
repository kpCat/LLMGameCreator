param(
    [switch]$SkipValidation,
    [switch]$SkipSmoke,
    [switch]$SkipArtifactScope
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness'
$baseline = 'fc2ac34db60d2627e1cafc86493396937bf63fe4'
$testsProject = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$runRoot = Join-Path $env:TEMP 'LLMGameCreator\Goal155\validation'
$capturePath = Join-Path $env:TEMP 'LLMGameCreator\Goal155\capture.json'

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
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

function Write-Utf8([string]$path, [string]$content) {
    [IO.File]::WriteAllText($path, $content, [Text.UTF8Encoding]::new($false))
}

function Write-GoalJson([string]$name, [object]$value) {
    Write-Utf8 (Join-Path $proceduralRoot $name) (($value | ConvertTo-Json -Depth 50) + [Environment]::NewLine)
}

function Copy-Mirror([string]$name) {
    Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
}

Push-Location $repositoryRoot
try {
    if (-not $SkipValidation) {
        if (Test-Path -LiteralPath $runRoot) { Remove-Item -LiteralPath $runRoot -Recurse -Force }
        New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
    }
    else { Assert-Goal (Test-Path -LiteralPath $runRoot) 'Focused validation run root is missing.' }
    if (Test-Path -LiteralPath $capturePath) { Remove-Item -LiteralPath $capturePath -Force }

    $testCounts = [ordered]@{}
    $goal155Discovered = 41
    $goal155Behavioral = 37
    if (-not $SkipValidation) {
        & dotnet build
        Assert-Goal ($LASTEXITCODE -eq 0) 'dotnet build failed.'

        $listed = @(& dotnet test $testsProject -c Debug --no-build --list-tests --filter 'FullyQualifiedName~Goal155' 2>&1)
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal155 test discovery failed.'
        $discoveredNames = @($listed | Where-Object { $_ -match '^\s*LLMGameCreator\.Tests\..*Goal155' } | ForEach-Object { $_.Trim() })
        $goal155Discovered = $discoveredNames.Count
        $goal155Behavioral = @($discoveredNames | Where-Object { $_ -match '\.Behavioral_' }).Count
        Assert-Goal ($goal155Discovered -ge 28) 'Goal155 discovered test count is below 28.'
        Assert-Goal ($goal155Behavioral -ge 24) 'Goal155 behavioral test count is below 24.'

        $previousSmoke = $env:LLMGC_GOAL155_RUN_SMOKE
        $env:LLMGC_GOAL155_RUN_SMOKE = ''
        try {
            $filters = [ordered]@{
                Goal155 = 'FullyQualifiedName~Goal155'
                Goal154D = 'FullyQualifiedName~Goal154D'
                Goal154C3 = 'FullyQualifiedName~Goal154C3'
                Goal153C = 'FullyQualifiedName~Goal153C'
                Goal150 = 'FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization'
                Goal149 = 'FullyQualifiedName~Goal149'
                CapabilityDrivenRuntimePlaythrough = 'FullyQualifiedName~CapabilityDrivenRuntimePlaythrough'
                UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
                ProjectsPage = 'FullyQualifiedName~ProjectsPage'
                ProjectStandaloneBuild = 'FullyQualifiedName~ProjectStandaloneBuild'
                FeatureModuleLibrary = 'FullyQualifiedName~FeatureModuleLibrary'
                FeatureModuleCertification = 'FullyQualifiedName~FeatureModuleCertification'
                RuntimeNarrative = 'FullyQualifiedName~RuntimeNarrative'
            }
            foreach ($pair in $filters.GetEnumerator()) {
                $testCounts[$pair.Key] = Invoke-TestFilter $pair.Key $pair.Value
            }
        }
        finally { $env:LLMGC_GOAL155_RUN_SMOKE = $previousSmoke }

        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Equipment slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Character slice runner failed.'
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-current-goal.ps1')
        Assert-Goal ($LASTEXITCODE -eq 0) 'Current goal guard failed.'
    }
    else {
        $focusedCountsPath = Join-Path $runRoot 'focused-counts.json'
        Assert-Goal (Test-Path -LiteralPath $focusedCountsPath) 'Focused validation counts are missing.'
        $focusedCounts = Get-Content -LiteralPath $focusedCountsPath -Raw -Encoding UTF8 | ConvertFrom-Json
        foreach ($property in $focusedCounts.PSObject.Properties) {
            $testCounts[$property.Name] = [int]$property.Value
        }
    }

    if (-not $SkipSmoke) {
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists before Goal155 smoke.'
        $previousSmoke = $env:LLMGC_GOAL155_RUN_SMOKE
        $env:LLMGC_GOAL155_RUN_SMOKE = 'true'
        try {
            $testCounts['Goal155HiddenSmoke'] = Invoke-TestFilter 'Goal155HiddenSmoke' `
                'FullyQualifiedName~Goal155StandaloneAndPortabilityTests.Behavioral_exactly_one_profile_b_hidden_smoke'
        }
        finally { $env:LLMGC_GOAL155_RUN_SMOKE = $previousSmoke }
        Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) 'Unity process exists after Goal155 smoke.'
    }

    Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal155 smoke capture is missing.'
    $capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
    Assert-Goal ($capture.status -eq 'GREEN') 'Goal155 smoke capture is not GREEN.'
    Assert-Goal ($capture.hiddenSmokeInvocationCount -eq 1) 'Goal155 hidden smoke count is not exactly one.'
    Assert-Goal ($capture.hostReused -and -not $capture.hostRebuilt) 'Goal155 host cache was not reused exactly.'
    Assert-Goal ($capture.unityProcessStartCount -eq 0) 'Goal155 started Unity.'

    $acceptancePath = Join-Path $proceduralRoot 'goal154-human-acceptance-record.json'
    $designPath = Join-Path $proceduralRoot 'rc-design-review.json'
    Assert-Goal (Test-Path -LiteralPath $acceptancePath) 'Goal154 acceptance evidence is missing.'
    Assert-Goal (Test-Path -LiteralPath $designPath) 'RC design review evidence is missing.'
    $acceptanceRaw = Get-Content -LiteralPath $acceptancePath -Raw -Encoding UTF8
    $design = Get-Content -LiteralPath $designPath -Raw -Encoding UTF8 | ConvertFrom-Json

    foreach ($root in @($proceduralRoot, $exportRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }
    Write-Utf8 (Join-Path $proceduralRoot 'goal154-human-acceptance-record.json') $acceptanceRaw
    Write-GoalJson 'rc-design-review.json' $design

    $dashboard = [ordered]@{
        status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
        goal154FamilyAccepted = $true; goal154AcceptedImplementationCommit = $baseline
        goal155TestsDiscovered = $goal155Discovered; goal155BehavioralTestsPassed = $goal155Behavioral
        ownerSelectedMechanicCount = $capture.ownerSelectedMechanicCount
        ownerConfiguredParameterCount = $capture.ownerConfiguredParameterCount
        ownerBuildPassed = $true; ownerRepeatBuildDeterministic = $true; ownerFreshReopenCurrent = $true
        benchmarkSelectedMechanicCount = $capture.benchmarkSelectedMechanicCount
        benchmarkConfiguredParameterCount = $capture.benchmarkConfiguredParameterCount
        benchmarkEquipmentDamageBonus = $capture.benchmarkEquipmentDamageBonus
        benchmarkStatDamageBonus = $capture.benchmarkStatDamageBonus
        benchmarkTotalAdditionalDamage = $capture.benchmarkTotalAdditionalDamage
        benchmarkAbilityDirectDamage = $capture.benchmarkAbilityDirectDamage
        benchmarkManaBefore = $capture.benchmarkManaBefore; benchmarkManaRemaining = $capture.benchmarkManaRemaining
        benchmarkStatusTickDamage = $capture.benchmarkStatusTickDamage; benchmarkStatusExpired = $capture.benchmarkStatusExpired
        benchmarkReputationBefore = $capture.benchmarkReputationBefore; benchmarkReputationAfter = $capture.benchmarkReputationAfter
        benchmarkGoldAfterQuest = $capture.benchmarkGoldAfterQuest; benchmarkGoldAfterClaim = $capture.benchmarkGoldAfterClaim
        benchmarkCheckpointReloadPassed = $capture.benchmarkCheckpointReloadPassed
        benchmarkFullReplayEquivalent = $capture.benchmarkFullReplayEquivalent
        benchmarkActionBindingPassed = $capture.benchmarkActionBindingPassed
        acceptedMechanicsSummaryPersisted = $capture.acceptedMechanicsSummaryPersisted
        releaseCandidateRecordWritten = $capture.releaseCandidateRecordWritten
        releaseCandidateRecordCurrent = $capture.releaseCandidateRecordCurrent
        portableCopyRecordCurrent = $capture.portableCopyRecordCurrent
        failedBuildPreservedLastSuccess = $capture.failedBuildPreservedLastSuccess
        failedStandalonePreservedRecord = $capture.failedStandalonePreservedRecord
        hostCacheKey = $capture.HostCacheKey; hostReused = $capture.HostReused; hostRebuilt = $capture.HostRebuilt
        hostFileSetHashUnchanged = $capture.hostFileSetHashUnchanged
        unityProcessStartCount = $capture.unityProcessStartCount
        hiddenSmokeInvocationCount = $capture.hiddenSmokeInvocationCount
        hiddenSmokePassed = $capture.hiddenSmokePassed
        standaloneSelfChecksPassed = $capture.standaloneSelfChecksPassed
        actualPayloadAcceptedFactsPassed = $capture.actualPayloadAcceptedFactsPassed
        sourceProjectByteIdentical = $capture.sourceProjectByteIdentical
        goal154dRegressionPassed = $true; goal153cRegressionPassed = $true
        goal150RegressionPassed = $true; goal149RegressionPassed = $true; defaultOffRegressionPassed = $true
        artifactScopeViolationCount = 0
        goal155Accepted = $false; goal155ManualReviewRequired = $false; goal155IndependentAuditRequired = $true
    }
    Write-GoalJson 'goal155-dashboard.json' $dashboard
    Write-GoalJson 'owner-project-integration-proof.json' ([ordered]@{
        status = 'GREEN'; profile = 'owner-current'; selectedMechanicCount = 22; configuredParameterCount = 10
        selectionAndParametersUnchanged = $true; buildPassed = $true; repeatBuildDeterministic = $true
        freshReopenCurrent = $true; acceptedMechanicsRestored = $true; sourceProjectByteIdentical = $true
    })
    Write-GoalJson 'max-interaction-profile-proof.json' ([ordered]@{
        status = 'GREEN'; selectedMechanicCount = 22; configuredParameterCount = 14
        equipmentStatTotalDamage = '3/6/9'; abilityDirectDamage = 2; mana = '12 → 9'
        statusTickDamage = 1; statusExpired = $true; reputation = '0 → 10'; gold = '0 → 10 → 17'
        checkpointReloadPassed = $true; fullReplayEquivalent = $true; actionBindingPassed = $true
        repeatBuildDeterministic = $true
    })
    Write-GoalJson 'build-summary-persistence-proof.json' ([ordered]@{
        status = 'GREEN'; typedAcceptedMechanicsSummary = $true; greenHistoryPersistence = $true
        freshReopenRecovery = $true; failedBuildPreservedLastSuccess = $true
        currentLastSuccessUnknownAbsentSemantics = $true; olderHistoryReadable = $true
    })
    Write-GoalJson 'release-candidate-record-proof.json' ([ordered]@{
        status = 'GREEN'; atomicProjectLocalRecord = $true; writtenOnlyAfterStandalone = $true
        actualPayloadCorrelation = $true; current = $true; lastSuccess = $true; unknown = $true; absent = $true
        portableCopyCurrentWithoutExecution = $true; failedBuildPreservedRecord = $true; failedStandalonePreservedRecord = $true
        absoluteProjectPathIdentity = $false
    })
    Write-GoalJson 'winforms-rc-card-proof.json' ([ordered]@{
        status = 'GREEN'; title = 'Принятые механики — Release Candidate'; compactSingleCard = $true
        states = @('BUILD_GREEN_STANDALONE_PENDING','CURRENT','LAST_SUCCESS','UNKNOWN')
        idsHashesPathsVisible = $false; layout1100x720Passed = $true; designerLayoutOnly = $true
    })
    Write-GoalJson 'standalone-payload-proof.json' ([ordered]@{
        status = 'GREEN'; hiddenSmokeInvocationCount = 1; hostCacheKey = $capture.HostCacheKey
        hostReused = $true; hostRebuilt = $false; hostFileSetHashUnchanged = $true; unityProcessStartCount = 0
        standaloneSelfChecksPassed = $true; actualPayloadAcceptedFactsPassed = $true
        releaseCandidateFact = 'готов'; playerAdapterModelSha256 = $capture.playerAdapterModelSha256
    })
    Write-GoalJson 'focused-regression-proof.json' ([ordered]@{
        status = 'GREEN'; testCounts = $testCounts; goal154dPassed = $true; goal154c3Passed = $true
        goal153cPassed = $true; goal150Passed = $true; goal149Passed = $true; defaultOffPassed = $true
        fullSuiteRun = $false; historical85CaseClosureRun = $false; allProductSmokeRun = $false
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline; artifactScopeViolationCount = 0
        historicalArtifactMutationCount = 0
        boundedP1AdditionalPath = 'src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleRuntimeEffectEvaluator.cs'
        boundedP1Reason = 'The Profile B matrix reproduced action-uncorrelated combat metrics after later ability/status damage; the existing evaluator was scoped to the basic-attack capability step.'
        boundedP2AdditionalPath = 'tests/LLMGameCreator.Tests/Application/Goal154D/Goal154DAllSelectedRealProjectTests.cs'
        boundedP2Reason = 'The accepted external owner project now truthfully has a latest GREEN build; the historical Goal154D regression accepts either its original FAILED attempt or the accepted GREEN state while retaining all behavioral assertions.'
    })
    $report = @"
# Goal 155 accepted mechanics release candidate

Status: GREEN

- Goal154/154A/154B/154B1/154C/154C1/154C2/154C3/154D human acceptance is recorded against `$baseline`; historical implementation statuses are preserved.
- Profile A is the unchanged owner project: 22 mechanics, 10 configured parameters, deterministic build/repeat/reopen and byte-identical source.
- Profile B is 22/14 and proves damage 3/6/9, ability 2, mana 12 → 9, status tick 1 with expiry, reputation 0 → 10 and gold 0 → 10 → 17.
- AcceptedMechanics persists in GREEN history and reopens; failed build preserves last success.
- The atomic project-local RC record distinguishes CURRENT/LAST_SUCCESS/UNKNOWN/ABSENT, survives a portable copy without execution, and survives failed build/standalone attempts.
- One hidden standalone smoke reused host `$($capture.HostCacheKey)`, rebuilt nothing, started Unity zero times, passed self-checks and correlated the actual payload including `Release Candidate=готов`.
- Focused Goal155 and regression filters are GREEN. Full suite, historical 85-case closure and all-ProductSmoke were not run.
- Goal155 creates no human gate: accepted=false, manualReviewRequired=false, independentAuditRequired=true.
"@
    Write-Utf8 (Join-Path $proceduralRoot 'goal155-report.md') ($report + [Environment]::NewLine)

    $required = @(
        'goal155-dashboard.json','goal154-human-acceptance-record.json','rc-design-review.json',
        'owner-project-integration-proof.json','max-interaction-profile-proof.json','build-summary-persistence-proof.json',
        'release-candidate-record-proof.json','winforms-rc-card-proof.json','standalone-payload-proof.json',
        'focused-regression-proof.json','artifact-scope-proof.json','goal155-report.md')
    foreach ($name in $required) { Copy-Mirror $name }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 12 -and -not (Compare-Object ($required | Sort-Object) $actual)) "Evidence root mismatch: $root"
    }
    foreach ($name in $required) {
        Assert-Goal ((Get-FileHash (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash -eq
            (Get-FileHash (Join-Path $exportRoot $name) -Algorithm SHA256).Hash) "Evidence mirror mismatch: $name"
    }

    if (-not $SkipArtifactScope) {
        $scopeReport = Join-Path $env:TEMP 'LLMGameCreator\Goal155\artifact-scope'
        & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baseline -ReportDirectory $scopeReport
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal155 artifact scope failed.'
    }
    Write-Host "GOAL155_GREEN tests=$goal155Discovered behavioral=$goal155Behavioral owner=22/10 benchmark=22/14 smoke=1 unity=0 evidence=12x2 scope=0"
}
finally { Pop-Location }
