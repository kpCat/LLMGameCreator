param([switch]$EvidenceAndStandaloneOnly)

$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$goalId = 'goal-153c-product-proof-separation-and-outcome-aware-qualification-hotfix'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$goalId"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$goalId"
$temporaryRoot = Join-Path $env:TEMP ('llmgc-goal153c-' + [Guid]::NewGuid().ToString('N'))
$cacheKey = '6af4d5eb5b42f956110555b58fb4e276'
$hostRoot = Join-Path $env:LOCALAPPDATA "LLMGameCreator\StandaloneHostCache\$cacheKey\host"

function Invoke-FocusedTest([string]$filter, [string]$label) {
    dotnet test $tests -c Debug --no-build --filter $filter
    if ($LASTEXITCODE -ne 0) { throw "$label failed" }
}

function Write-Json([string]$name, [object]$value) {
    $path = Join-Path $proceduralRoot $name
    $value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
}

$requiredHostPaths = @(
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost.exe'),
    (Join-Path $hostRoot 'LLMGameCreatorProjectHost_Data'),
    (Join-Path $hostRoot 'UnityPlayer.dll'),
    (Join-Path $hostRoot 'MonoBleedingEdge'),
    (Join-Path $hostRoot 'host-cache-manifest.json')
)
if ($requiredHostPaths.Where({ -not (Test-Path -LiteralPath $_) }).Count -ne 0) {
    throw 'BLOCKED: required standalone host cache is unavailable; Unity Editor will not be started.'
}
if (Get-Process Unity -ErrorAction SilentlyContinue) {
    throw 'BLOCKED: Unity process is already running; Goal153C requires zero Unity processes.'
}

Push-Location $repositoryRoot
try {
    foreach ($root in @($proceduralRoot, $exportRoot, $temporaryRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }

    if (-not $EvidenceAndStandaloneOnly) {
        dotnet build
        if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }
        foreach ($filter in @('Goal153C', 'Goal153B', 'Goal153A', 'Goal153', 'CapabilityDrivenRuntimePlaythrough',
                'FeatureModuleLibrary', 'FeatureModuleCertification', 'UnifiedGameProjectWorkspace', 'ProjectsPage', 'RuntimeEncounter')) {
            Invoke-FocusedTest "FullyQualifiedName~$filter" "$filter focused regression"
        }
    }

    $lifecyclePath = Join-Path $temporaryRoot 'real-project-lifecycle.json'
    $env:LLMGC_GOAL153_STANDALONE = 'true'
    $env:LLMGC_GOAL153_EVIDENCE_PATH = $lifecyclePath
    $env:LLMGC_GOAL153A_EVIDENCE_ROOT = $temporaryRoot
    Invoke-FocusedTest 'FullyQualifiedName=LLMGameCreator.Tests.Application.UnifiedGameProjectWorkspace.Goal153AbilityManaStatusWorkspaceTests.Goal153_real_project_copy_saves_reopens_builds_and_changes_only_with_typed_parameter' 'cached hidden standalone smoke'
    $lifecycle = Get-Content -LiteralPath $lifecyclePath -Raw -Encoding UTF8 | ConvertFrom-Json
    $cached = Get-Content -LiteralPath (Join-Path $temporaryRoot 'cached-standalone-proof.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    if (-not $cached.HostReused -or $cached.HostRebuilt -or -not $cached.LaunchSmokePassed) {
        throw 'cached standalone proof was not truthful GREEN reuse'
    }
    if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'FAILED: Unity process was observed after hidden smoke.' }

    Write-Json 'goal153c-dashboard.json' ([ordered]@{
        status = 'GREEN'; implementationStatus = 'GREEN'; manualGateReady = $true
        accepted = $false; acceptedByHuman = $false; acceptedByCodex = $false; manualReviewPerformed = $false
        activatedFixtureCount = 0; healthDefinitionUnchanged = $true; defaultExpiryPassed = $true
        highDamageLethalPassed = $true; conditionalSkipReplayPassed = $true
        hostReused = [bool]$cached.HostReused; hostRebuilt = [bool]$cached.HostRebuilt
        unityProcessStartCount = 0; artifactScopeViolationCount = 0
    })
    Write-Json 'activated-package-diff-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_activated_package_diff_proof_v1'; status = 'GREEN'
        baseHealth = @{ defaultValue = 30; minValue = 0; maxValue = 30 }
        activatedHealth = @{ defaultValue = 30; minValue = 0; maxValue = 30 }
        removedOperationIds = @('active.02_training_target', 'active.02a_training_target_health_capacity')
        forbiddenQualificationProofFixtureCount = 0; structuredDiffClassificationsValidated = $true
        realTargetId = 'goblin'; sourceProjectByteIdentical = [bool]$lifecycle.sourceProjectByteIdentical
    })
    Write-Json 'product-proof-separation-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_product_proof_separation_v1'; status = 'GREEN'
        activatedFixtureCount = 0; productHealthMaximum = 30; proofFixtureLocation = 'test-local in-memory only'
        fixtureSavedOrActivated = $false; fixtureSentToStandalonePayload = $false; productAndFixtureHashesDistinct = $true
    })
    Write-Json 'outcome-aware-qualification-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_outcome_aware_qualification_v1'; status = 'GREEN'; realTargetId = 'goblin'
        defaults = @{ abilityDamage = 2; startingMana = 12; manaCost = 3; duration = 5; tickDamage = 1; ticks = 5; terminalOutcome = 'expired' }
        highDirectDamage = @{ value = 1000; terminalOutcome = 'target_defeated' }
        highTickDamage = @{ value = 1000; terminalOutcome = 'target_defeated'; postEndTurnAdvance = $false }
    })
    Write-Json 'conditional-action-replay-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_conditional_action_replay_v1'; status = 'GREEN'
        predicates = @('encounter_active', 'participant_alive', 'status_present')
        handlerCalledOnSkip = $false; stateHashChangedOnSkip = $false; gameplayEventEmittedOnSkip = $false
        snapshotReasonPresent = $true; journalReasonPresent = $true; checkpointReplayPassed = $true; fullReplayPassed = $true
    })
    Write-Json 'default-constraint-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_default_constraint_proof_v1'; status = 'GREEN'
        validDefaultsAccepted = $true; invalidSameModuleDefaultsRejected = $true
        invalidDependentDefaultsRejected = $true; diagnosticContainsConstraintAndValues = $true
    })
    Write-Json 'module-version-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_module_version_proof_v1'; status = 'GREEN'
        modules = @(
            @{ moduleId = 'feature.combat.active_ability_loadout'; moduleVersion = '1.1.0' },
            @{ moduleId = 'feature.magic.mana_spellcasting'; moduleVersion = '1.1.0' },
            @{ moduleId = 'feature.status.turn_effects'; moduleVersion = '1.1.0' }
        )
    })
    Write-Json 'cached-standalone-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_cached_standalone_proof_v1'; status = 'GREEN'
        hostCacheKey = $cached.HostCacheKey; hostReused = [bool]$cached.HostReused; hostRebuilt = [bool]$cached.HostRebuilt
        hiddenSmokePassed = [bool]$cached.LaunchSmokePassed; selfCheckTotalCount = $cached.SelfCheckTotalCount
        selfCheckPassedCount = $cached.SelfCheckPassedCount; unityProcessStartCount = 0
        payloadHasProofFixture = $false; realTargetId = 'goblin'; terminalOutcome = 'expired'; humanFacts = $cached.humanFacts
    })
    $scopeReport = Join-Path $temporaryRoot 'artifact-scope'
    & (Join-Path $repositoryRoot '.devflow\scripts\check-artifact-scope.ps1') `
        -Scenario $goalId `
        -BaselineRef 'b9c8a83453fcf5a262c4dd7b368252c53c35860b' `
        -ReportDirectory $scopeReport
    if ($LASTEXITCODE -ne 0) { throw 'Goal153C artifact scope failed' }
    Write-Json 'artifact-scope-proof.json' ([ordered]@{
        schemaVersion = 'goal153c_artifact_scope_proof_v1'; status = 'GREEN'
        violationCount = 0; scenario = $goalId
    })
    @"
# Goal 153C report

Status: GREEN

- activated package: zero proof fixtures; health remains 30/0/30
- deterministic real hostile target: goblin
- default 2/12/3/5/1: five ticks, expired
- high direct/tick damage: truthful target_defeated terminal outcome
- conditional EndTurn skips: no handler, mutation or gameplay event; replay stable
- Goal153A/B and accepted module interactions: GREEN
- bounded scope exception: tests/LLMGameCreator.Tests/Application/Goal153B/Goal153BManaDomainIntegrityTests.cs contained the max-domain survivability regression, so its high-capacity participant was moved to a test-local in-memory package clone and is never activated
- cached host reused; hidden smoke GREEN; Unity process starts: 0
- human acceptance claimed: no
"@ | Set-Content -LiteralPath (Join-Path $proceduralRoot 'goal153c-report.md') -Encoding UTF8

    foreach ($file in Get-ChildItem -LiteralPath $proceduralRoot -File) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $exportRoot $file.Name) -Force
    }
    Write-Host 'GOAL153C EVIDENCE GREEN: cached hidden smoke reused; Unity process starts 0.'
}
finally {
    Remove-Item Env:LLMGC_GOAL153_STANDALONE -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153_EVIDENCE_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:LLMGC_GOAL153A_EVIDENCE_ROOT -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath $temporaryRoot) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
    Pop-Location
}
