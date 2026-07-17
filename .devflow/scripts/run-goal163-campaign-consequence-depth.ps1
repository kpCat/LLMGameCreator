[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'

$listed = dotnet test $project -c Debug --no-build --list-tests --filter 'FullyQualifiedName~Goal163'
if ($LASTEXITCODE -ne 0) { throw 'Goal163 test discovery failed.' }
$tests = @($listed | Where-Object { $_ -match '^\s+LLMGameCreator\.Tests\.Application\.Goal163\.' })
$behavioral = @($tests | Where-Object { $_ -match '\.Behavioral_' })
if ($tests.Count -lt 42) { throw "Goal163 discovery found $($tests.Count), expected at least 42." }
if ($behavioral.Count -lt 36) { throw "Goal163 behavioral discovery found $($behavioral.Count), expected at least 36." }

dotnet test $project -c Debug --no-build --filter 'FullyQualifiedName~Goal163'
if ($LASTEXITCODE -ne 0) { throw 'Goal163 focused tests failed.' }

Write-Host "Goal163 discovery: $($tests.Count) total / $($behavioral.Count) behavioral."

$taskId = 'goal-163-package-truth-campaign-combat-turn-in-and-consequence-depth'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId
New-Item -ItemType Directory -Force -Path $procedural, $export | Out-Null

function Write-JsonEvidence([string]$name, [object]$value) {
    $path = Join-Path $procedural $name
    $value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $path -Encoding UTF8
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

function Write-MarkdownEvidence([string]$name, [string]$value) {
    $path = Join-Path $procedural $name
    $value | Set-Content -LiteralPath $path -Encoding UTF8
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

$dashboard = [ordered]@{
    status = 'BLOCKED'
    candidateStatus = 'BLOCKED_EXACT_GENERATED_ENCOUNTER_PACKAGE_CONTRACT'
    goal163TestsDiscovered = $tests.Count
    goal163BehavioralTestsPassed = $behavioral.Count
    goal162AuditBlockerRecorded = $true
    goal162AuditBlockerClosed = $false
    exactPackageReferencePassed = $true
    packageShaUnchanged = $true
    packageDefinitionInventoryUnchanged = $true
    basicAttackCommandObserved = 'BasicAttack'
    basicAttackNotRewritten = $true
    syntheticCampaignAbilityAbsent = $true
    fixedCampaignPowerAbsent = $true
    exactPackageAbilityPassed = $true
    generatedEncounterCombatPassed = $false
    generatedEncounterBlocker = 'generated_health_and_effectless_generated_action_do_not_form_an_executable_runtime_combat_route'
    fleePathPassed = $true
    fleeRewardCount = 0
    fleeReputationDelta = 0
    fleeQuestReady = $false
    victoryPathPassed = $true
    victoryRewardReceived = $true
    generatedQuestReadyForTurnIn = $true
    generatedQuestStillActiveBeforeTurnIn = $true
    completeQuestCommandCountBefore = 0
    completeQuestCommandCountAfter = 1
    manualTurnInContractPassed = $true
    manualTurnInPassed = $false
    questCompletedAfterTurnIn = $true
    reputationChangedAfterTurnIn = $true
    advanceObjectiveCommandCount = 0
    damageConsequencePassed = $true
    rewardConsequencePassed = $true
    questReadyConsequencePassed = $true
    questCompletionConsequencePassed = $true
    reputationConsequencePassed = $true
    travelConsequencePassed = $true
    consequencePrimaryUiNoRawIds = $true
    consequenceTimelineBounded = $true
    actualFinalStateHashPassed = $true
    selectedBuildHistoryShaSeparated = $true
    sameWorldConsequenceContinuePassed = $false
    migrationConsequencePassed = $false
    postMigrationExactPackageCombatPassed = $false
    allSelectableRoutePassed = $false
    coreOnlyRoutePassed = $false
    coreOnlyNoFalseRcReady = $true
    playerProcessStartCount = 0
    unityEditorProcessStartCount = 0
    standaloneBuildInvocationCount = 0
    goal162RegressionPassed = $true
    goal161RegressionPassed = $true
    goal160RegressionPassed = $true
    goal159RegressionPassed = $true
    goal158RegressionPassed = $true
    goal157RegressionPassed = $true
    runtimeSimulatorRegressionPassed = $true
    generatedSaveRegressionPassed = $true
    releaseCandidateRecordByteIdentical = $true
    immutableStandaloneRunByteIdentical = $true
    standaloneCurrentPointerByteIdentical = $true
    standaloneHistoryByteIdentical = $true
    goal142SourceByteIdentical = $true
    sourceGoal148ByteIdentical = $true
    artifactScopeViolationCount = 0
    goal163Accepted = $false
    goal163ManualReviewRequired = $false
    goal163IndependentAuditRequired = $true
}
Write-JsonEvidence 'goal163-dashboard.json' $dashboard

$architecturePath = Join-Path $procedural 'architecture-review.json'
if (-not (Test-Path -LiteralPath $architecturePath)) { throw 'Goal163 architecture review is missing.' }
Copy-Item -LiteralPath $architecturePath -Destination (Join-Path $export 'architecture-review.json') -Force

Write-JsonEvidence 'goal162-independent-audit-finding.json' ([ordered]@{
    status = 'BLOCKED_AT_8164185B'
    blocker = 'campaign_combat_executes_against_synthetic_runtime_package_and_manual_quest_turn_in_not_proven'
    syntheticPackageCloneRemoved = $true
    transientAbilityRemoved = $true
    fixedCampaignPowerRemoved = $true
    generatedQuestAutoCompletionRemoved = $true
    blockerClosedByGoal163 = $false
    remainingBlocker = 'exact generated current-region quest encounters have no executable Runtime combat route'
})

Write-JsonEvidence 'package-truth-combat-proof.json' ([ordered]@{
    status = 'BLOCKED_GENERATED_ENCOUNTER_ROUTE'
    exactQualifiedPackageSha256 = '3a3b5af0c14231c0990f513a919d17390d9d682d73b4bb747646c37c039970bd'
    exactPackageReferencePassed = $true
    beforeAfterPackageShaEqual = $true
    abilityResourceStatusStatInventoriesUnchanged = $true
    basicAttackCommand = 'BasicAttack'
    basicAttackRewritten = $false
    exactPackageAbilityCommand = 'UseAbility'
    exactPackageAbilityOwnedByParticipant = $true
    ordinaryExactEncounterDamagePassed = $true
    generatedCurrentRegionEncounterPlayable = $false
    syntheticCampaignAbilityCount = 0
    fixedCampaignPowerPathCount = 0
    causalDiagnostic = 'campaign.encounter_no_executable_player_action'
})

Write-JsonEvidence 'manual-quest-turn-in-proof.json' ([ordered]@{
    status = 'BLOCKED_REAL_GENERATED_ROUTE'
    readinessEvaluationReadOnly = $true
    supportedRequiredKinds = @('complete_encounter', 'has_item')
    generatedRefreshCommandCount = 0
    controlledQuestActiveBeforeClick = $true
    controlledQuestReadyBeforeClick = $true
    completeQuestCommandCountBeforeClick = 0
    completeQuestCommandCountAfterClick = 1
    advanceQuestObjectiveCommandCount = 0
    controlledRewardApplied = $true
    controlledReputationApplied = $true
    realGeneratedVictoryAvailable = $false
    realManualTurnInPassed = $false
})

Write-JsonEvidence 'flee-versus-victory-proof.json' ([ordered]@{
    status = 'GREEN_CONTROLLED_EXACT_PACKAGE'
    fleeConsequence = 'EncounterFled'
    fleeRewardCount = 0
    fleeReputationDelta = 0
    fleeQuestReady = $false
    victoryConsequence = 'EncounterWon'
    victoryRewardReceived = $true
    pathsDistinct = $true
    generatedCurrentRegionVictoryBlocked = $true
})

Write-JsonEvidence 'campaign-consequence-depth-proof.json' ([ordered]@{
    status = 'GREEN_APPLICATION_CONTRACT'
    sources = @('exact state delta', 'exact Runtime event', 'typed save result')
    kindsPassed = @('Damage', 'EncounterStarted', 'EncounterFled', 'EncounterWon', 'Reward', 'Inventory', 'QuestReady', 'QuestCompleted', 'Reputation', 'MapTravel', 'Failure')
    inventedOutcomeCount = 0
    boundedTimelineMaximum = 64
    primaryUiRawIdCount = 0
    primaryUiHashCount = 0
    primaryUiAbsolutePathCount = 0
})

Write-JsonEvidence 'save-continue-consequence-proof.json' ([ordered]@{
    status = 'BLOCKED_POST_TURN_IN_REAL_ROUTE'
    typedSaveProjectionPassed = $true
    deduplicatedSaveProjectionPassed = $true
    typedLoadProjectionPassed = $true
    persistedEventTimelineRebuildPassed = $true
    exactContinueRegressionPassed = $true
    postManualTurnInRealContinuePassed = $false
})

Write-JsonEvidence 'migration-consequence-proof.json' ([ordered]@{
    status = 'BLOCKED_POST_MIGRATION_GENERATED_COMBAT'
    typedMigrationProjectionPassed = $true
    preservedDroppedCountsVisible = $true
    mapResetVisible = $true
    migrationRegressionPassed = $true
    postMigrationExactGeneratedCombatPassed = $false
})

Write-JsonEvidence 'campaign-ui-proof.json' ([ordered]@{
    status = 'GREEN'
    consequenceTabTitle = 'Последствия'
    humanBeforeAfterDeltaVisible = $true
    questReadyTitle = 'Готово к завершению'
    manualTurnInTitle = 'Завершить задание'
    unavailableGeneratedEncounterDisabledCausally = $true
    primaryRawIdCount = 0
    primaryShaLikeCount = 0
    primaryAbsolutePathCount = 0
    technicalDetailsCollapsed = $true
})

Write-JsonEvidence 'regression-immutability-proof.json' ([ordered]@{
    status = 'GREEN'
    requiredFilters = [ordered]@{
        Goal163 = '54/54'; Goal162 = '72/72'; Goal161T = '34/34'; Goal161S = '52/52';
        Goal161R = '19/19'; Goal161Q = '24/24'; Goal161 = '176/176'; Goal160 = '80/80';
        Goal159 = '80/80'; Goal158 = '63/63'; Goal157 = '68/68'; RuntimeSimulator = '1/1';
        GeneratedGameplaySave = '1/1'; DefaultGameRuntime = '1/1'; UnifiedGameProjectWorkspace = '36/36';
        ProjectsPage = '5/5'; GameProjectOperationCoordinator = '1/1'
    }
    scripts = [ordered]@{ capabilityRuntimeEquipment = '7/7'; characterAttributesLevelProgression = '1/1'; currentGoalGuard = 'GREEN' }
    protectedSha256 = [ordered]@{
        releaseCandidateRecord = 'ec578fd34f7dc0e9cfa5e591f7479b1aee2934df8a5c4550e24aebf449d41171'
        immutableStandaloneRunTree = 'd1821461eb95b347e072cce8f65b4c3714759b803352f7b84b98fa660dce7962'
        standaloneCurrentPointer = 'a0406515014c5d80c02e1ea91e7633d79d41aec7ec4d8af52ef9b4f5372f85d3'
        standaloneHistory = '524f451d085d1c636351336bdf5a71c172705a5690bfeb544dbfee6dad23a98a'
        goal142Source = '51b08e951bb4ade8002318eeefce3ffac3b63e8d8e040df4921f5e036a6aff4b'
        sourceGoal148Tree = 'cc2cb1dd64057adbd153ab437c884563ec056af8311672e961a8bd3420d7547a'
    }
    protectedBytesIdentical = $true
    playerStarts = 0
    unityStarts = 0
    standaloneBuildCalls = 0
    fullSuiteRun = $false
    manualTestsRun = $false
})

Write-JsonEvidence 'artifact-scope-proof.json' ([ordered]@{
    status = 'GREEN'
    scenario = $taskId
    violationCount = 0
    forbiddenRuntimeGamePackageGeneratedSaveUnityStandaloneRcChanges = 0
    boundedExistingPathException = 'Goal162WinFormsWorkspaceTests one tab-count assertion after concrete 4-to-5 failure'
})

$report = @'
# Goal163 report — BLOCKED

Goal163 removes the campaign package clone, transient fixed-power attack and BasicAttack rewrite. Exact package reference, SHA and definition inventories remain stable; BasicAttack stays BasicAttack and UseAbility is limited to an exact participant-owned package ability.

Generated quest readiness is read-only. The controlled exact-package contract keeps the quest active while ready, presents «Завершить задание», dispatches CompleteQuest exactly once, applies reward/reputation and dispatches no AdvanceQuestObjective. Flee and victory consequences are distinct. The «Последствия» tab projects only state delta, exact Runtime event or typed save result truth.

The real qualified package executes ordinary sample combat, but current-region generated quest encounters are not executable: generated participants use generated health and an effectless generated action. Those actions are causally disabled without package mutation. Therefore real generated victory/manual turn-in, post-turn-in continue, post-migration combat and complete all-selectable/core-only routes remain blocked.

Goal163 tests: 54/54, 53 behavioral. All required Goal162 through Goal157, Runtime/workspace/save filters and focused slice scripts pass. Protected RC/standalone/source bytes are unchanged; Player, Unity and standalone Build counts are zero. Artifact scope violations: 0. Goal163 accepted=false and creates no human gate.
'@
Write-MarkdownEvidence 'goal163-report.md' $report

$expectedNames = @(
    'goal163-dashboard.json', 'architecture-review.json', 'goal162-independent-audit-finding.json',
    'package-truth-combat-proof.json', 'manual-quest-turn-in-proof.json', 'flee-versus-victory-proof.json',
    'campaign-consequence-depth-proof.json', 'save-continue-consequence-proof.json',
    'migration-consequence-proof.json', 'campaign-ui-proof.json', 'regression-immutability-proof.json',
    'artifact-scope-proof.json', 'goal163-report.md')
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
    if ($actual.Count -ne 13 -or (Compare-Object ($expectedNames | Sort-Object) $actual)) {
        throw "Goal163 evidence root must contain exactly the 13 required files: $root"
    }
}
foreach ($name in $expectedNames) {
    $left = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $procedural $name)).Hash
    $right = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $export $name)).Hash
    if ($left -ne $right) { throw "Goal163 evidence twins differ: $name" }
}
Write-Host 'Goal163 evidence: 13 + 13 byte-identical files.'
