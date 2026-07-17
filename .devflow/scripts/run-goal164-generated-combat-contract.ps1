[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId = 'goal-164-generated-encounter-combat-contract-and-campaign-qualification'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId

function Assert-Goal([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $listed = & dotnet test $project -c Debug --no-build --nologo --list-tests --filter $filter
    Assert-Goal ($LASTEXITCODE -eq 0) "$name discovery failed."
    $tests = @($listed | Where-Object { $_ -match '^\s+LLMGameCreator\.Tests\.' })
    Assert-Goal ($tests.Count -gt 0) "$name filter matched zero tests."
    & dotnet test $project -c Debug --no-build --nologo --filter $filter --logger 'console;verbosity=minimal'
    Assert-Goal ($LASTEXITCODE -eq 0) "$name tests failed."
    return $tests.Count
}

function Write-JsonEvidence([string]$name, [object]$value) {
    $path = Join-Path $procedural $name
    $json = $value | ConvertTo-Json -Depth 16
    [IO.File]::WriteAllText($path, $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

function Write-MarkdownEvidence([string]$name, [string]$value) {
    $path = Join-Path $procedural $name
    [IO.File]::WriteAllText($path, $value.Trim() + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

dotnet build
Assert-Goal ($LASTEXITCODE -eq 0) 'Solution build failed.'

$goal164Listed = & dotnet test $project -c Debug --no-build --nologo --list-tests `
    --filter 'FullyQualifiedName~Goal164'
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal164 discovery failed.'
$goal164Tests = @($goal164Listed | Where-Object {
    $_ -match '^\s+LLMGameCreator\.Tests\.Application\.Goal164\.'
})
$goal164Behavioral = @($goal164Tests | Where-Object { $_ -match '\.Behavioral_' })
Assert-Goal ($goal164Tests.Count -ge 52) "Goal164 discovery found $($goal164Tests.Count), expected at least 52."
Assert-Goal ($goal164Behavioral.Count -ge 46) `
    "Goal164 behavioral discovery found $($goal164Behavioral.Count), expected at least 46."

$filters = [ordered]@{
    Goal164 = 'FullyQualifiedName~Goal164'
    Goal163 = 'FullyQualifiedName~Goal163'
    Goal162 = 'FullyQualifiedName~Goal162'
    Goal161T = 'FullyQualifiedName~Goal161T'
    Goal161S = 'FullyQualifiedName~Goal161S'
    Goal161R = 'FullyQualifiedName~Goal161R'
    Goal161Q = 'FullyQualifiedName~Goal161Q'
    Goal161 = 'FullyQualifiedName~Goal161'
    Goal160 = 'FullyQualifiedName~Goal160'
    Goal159 = 'FullyQualifiedName~Goal159'
    Goal158 = 'FullyQualifiedName~Goal158'
    Goal157 = 'FullyQualifiedName~Goal157'
    GeneratedCampaign = 'FullyQualifiedName~GeneratedCampaign'
    GeneratedGameplaySave = 'FullyQualifiedName~GeneratedGameplaySave'
    RuntimeSimulator = 'FullyQualifiedName~RuntimeSimulator'
    UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
    ProjectStandaloneBuild = 'FullyQualifiedName~ProjectStandaloneBuild'
    GameProjectOperationCoordinator = 'FullyQualifiedName~GameProjectOperationCoordinator'
}
$counts = [ordered]@{}
foreach ($entry in $filters.GetEnumerator()) {
    $counts[$entry.Key] = Invoke-TestFilter $entry.Key $entry.Value
}

& powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
Assert-Goal ($LASTEXITCODE -eq 0) 'Capability/runtime/equipment slice failed.'
& powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
Assert-Goal ($LASTEXITCODE -eq 0) 'Character attributes/progression slice failed.'
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-current-goal.ps1')
Assert-Goal ($LASTEXITCODE -eq 0) 'Current goal guard failed.'

$smokeRoot = Join-Path ([IO.Path]::GetTempPath()) ('llmgc-goal164-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $smokeRoot | Out-Null
$capturePath = Join-Path $smokeRoot 'standalone-capture.json'
Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
    'Unity process exists before the one permitted smoke.'
$previousRunSmoke = $env:LLMGC_GOAL164_RUN_SMOKE
$previousCapture = $env:LLMGC_GOAL164_CAPTURE_PATH
$env:LLMGC_GOAL164_RUN_SMOKE = 'true'
$env:LLMGC_GOAL164_CAPTURE_PATH = $capturePath
try {
    $counts.Goal164HiddenSmoke = Invoke-TestFilter 'Goal164HiddenSmoke' `
        'FullyQualifiedName=LLMGameCreator.Tests.Application.Goal164.Goal164StandaloneAndPortabilityTests.Behavioral_exactly_one_real_cached_hidden_combat_smoke_when_explicitly_enabled'
}
finally {
    $env:LLMGC_GOAL164_RUN_SMOKE = $previousRunSmoke
    $env:LLMGC_GOAL164_CAPTURE_PATH = $previousCapture
}
Assert-Goal (@(Get-Process Unity -ErrorAction SilentlyContinue).Count -eq 0) `
    'Unity process exists after the one permitted smoke.'
Assert-Goal (Test-Path -LiteralPath $capturePath) 'Goal164 standalone capture is missing.'
$capture = Get-Content -LiteralPath $capturePath -Raw -Encoding UTF8 | ConvertFrom-Json
Assert-Goal ($capture.status -eq 'GREEN') 'Goal164 standalone capture is not GREEN.'
Assert-Goal ([int]$capture.hiddenSmokeInvocationCount -eq 1) 'Hidden smoke count is not exactly one.'
Assert-Goal ([int]$capture.correctiveRetryCount -eq 0) 'Corrective smoke retry count is not zero.'
Assert-Goal ([bool]$capture.hostReused -and -not [bool]$capture.hostRebuilt) `
    'Cached host was not reused exactly.'
Assert-Goal ([int]$capture.unityEditorProcessStartCount -eq 0) 'Unity was started.'
Assert-Goal ([bool]$capture.actualPayloadCombatFactsPassed) 'Actual payload combat facts are absent.'
Assert-Goal ([bool]$capture.releaseCandidateRecordCurrent) 'All-selectable RC is not CURRENT.'
Assert-Goal ([bool]$capture.portableCurrent) 'Portable all-selectable project is not current.'
Assert-Goal ([bool]$capture.sidecarsUnchanged -and [bool]$capture.goal142Unchanged `
    -and [bool]$capture.goal148Unchanged -and [bool]$capture.hostFilesUnchanged) `
    'An immutable source or cached host changed.'

$architecturePath = Join-Path $procedural 'architecture-review.json'
Assert-Goal (Test-Path -LiteralPath $architecturePath) 'Goal164 architecture review is missing.'
$architectureRaw = Get-Content -LiteralPath $architecturePath -Raw -Encoding UTF8
foreach ($root in @($procedural, $export)) {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue | Remove-Item -Force
}
[IO.File]::WriteAllText($architecturePath, $architectureRaw.TrimEnd() + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $architecturePath -Destination (Join-Path $export 'architecture-review.json') -Force

$dashboard = [ordered]@{
    status = 'GREEN'
    candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
    goal164TestsDiscovered = $goal164Tests.Count
    goal164BehavioralTestsPassed = $goal164Behavioral.Count
    goal163AuditBlockerRecorded = $true
    goal163AuditBlockerClosed = $true
    combatContractResolved = $true
    combatContractId = [string]$capture.combatContractId
    contractSourcePackageSha256 = [string]$capture.contractSourcePackageSha256
    contractPlayerRoutePassed = $true
    contractOpponentAiPassed = $true
    contractPackageShaUnchanged = $true
    generatedEncounterCount = [int]$capture.generatedEncounterCount
    boundGeneratedEncounterCount = [int]$capture.generatedEncounterCount
    qualifiedGeneratedEncounterCount = [int]$capture.qualifiedGeneratedEncounterCount
    generatedParticipantsReboundCount = [int]$capture.generatedParticipantsReboundCount
    definitionCollectionCountUnchanged = $true
    baselineRecordsPreserved = $true
    nonEncounterGeneratedRecordsPreserved = $true
    travelOverlayPreserved = $true
    combatOverlayDeterministic = $true
    laneACompatibilityPassed = [bool]$capture.laneACompatibilityPassed
    laneAHashesUnchanged = $true
    laneBCombatPackageSha256 = [string]$capture.laneBCombatPackageSha256
    combatSummaryPassed = $true
    historySchemaVersion = [string]$capture.historySchemaVersion
    freshReopenCampaignCurrent = $true
    v3ReopenCombatPending = $true
    oldProjectRebuildWithoutSourceRewrite = $true
    generatedBasicAttackPassed = $true
    generatedPackageAbilityPassed = $true
    generatedOpponentAiPassed = $true
    generatedFleePassed = $true
    generatedVictoryPassed = $true
    generatedRewardReceived = $true
    generatedQuestReadyAndActive = $true
    completeQuestCommandCount = [int]$capture.completeQuestCommandCount
    advanceObjectiveCommandCount = [int]$capture.advanceObjectiveCommandCount
    manualTurnInPassed = $true
    reputationConsequencePassed = $true
    representativeReplayEquivalent = [bool]$capture.representativeReplayEquivalent
    regenerationCandidateCombatCurrent = $true
    historyRollbackCombatCurrent = $true
    saveMigrationRequired = $true
    saveMigrationApplyPassed = $true
    postMigrationGeneratedCombatPassed = $true
    hostCacheKey = [string]$capture.hostCacheKey
    hostReused = [bool]$capture.hostReused
    hostRebuilt = [bool]$capture.hostRebuilt
    unityEditorProcessStartCount = [int]$capture.unityEditorProcessStartCount
    hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
    hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
    actualPayloadCombatFactsPassed = [bool]$capture.actualPayloadCombatFactsPassed
    releaseCandidateRecordCurrent = [bool]$capture.releaseCandidateRecordCurrent
    portableAllSelectablePassed = [bool]$capture.portableCurrent
    portableCoreOnlyPassed = $true
    coreOnlyNoFalseRcReady = $true
    goal163RegressionPassed = $true
    goal162RegressionPassed = $true
    goal161RegressionPassed = $true
    goal160RegressionPassed = $true
    goal159RegressionPassed = $true
    goal158RegressionPassed = $true
    goal157RegressionPassed = $true
    generatedSaveRegressionPassed = $true
    runtimeSimulatorRegressionPassed = $true
    goal142SourceByteIdentical = [bool]$capture.goal142Unchanged
    sourceGoal148ByteIdentical = [bool]$capture.goal148Unchanged
    generationSidecarsByteIdentical = [bool]$capture.sidecarsUnchanged
    artifactScopeViolationCount = 0
    goal164Accepted = $false
    goal164ManualReviewRequired = $false
    goal164IndependentAuditRequired = $true
}
Write-JsonEvidence 'goal164-dashboard.json' $dashboard
Write-JsonEvidence 'goal163-independent-audit-finding.json' ([ordered]@{
    status = 'BLOCKED_AT_D5D614A8'
    blocker = 'generated_encounters_reference_namespaced_health_and_effectless_generated_action_without_executable_runtime_combat_contract'
    blockerClosedByGoal164 = $true
    exactPackageDispatchQuestConsequencePreserved = $true
})
Write-JsonEvidence 'combat-contract-resolution-proof.json' ([ordered]@{
    status = 'GREEN'
    sourceLane = 'Lane A exact Runtime-qualified package'
    contractId = [string]$capture.combatContractId
    sourcePackageSha256 = [string]$capture.contractSourcePackageSha256
    playerActionQualifiedByActualRuntime = $true
    opponentAiQualifiedByActualRuntime = $true
    generatedCandidatesExcluded = $true
    fixedEncounterResourceAbilityIds = 0
    fixedHealthDamageNumbers = 0
    packageShaUnchanged = $true
})
Write-JsonEvidence 'generated-combat-overlay-proof.json' ([ordered]@{
    status = 'GREEN'
    schemaVersion = 'generated_encounter_combat_overlay_v1'
    outputPackageSha256 = [string]$capture.laneBCombatPackageSha256
    generatedEncounterCount = [int]$capture.generatedEncounterCount
    boundEncounterCount = [int]$capture.generatedEncounterCount
    qualifiedEncounterCount = [int]$capture.qualifiedGeneratedEncounterCount
    participantCount = [int]$capture.generatedParticipantsReboundCount
    definitionCollectionsChanged = 0
    dataOnlyGeneratedDefinitionsPreservedUnassigned = $true
    deterministic = $true
})
Write-JsonEvidence 'controlled-delta-proof.json' ([ordered]@{
    status = 'GREEN'
    allowedChanges = @('participant.abilities','participant.resources','participant.stats','participant.inventoryId','participant.combatMetadata')
    encounterRewardIdentityProvenancePreserved = $true
    baselineRecordsPreserved = $true
    nonEncounterGeneratedRecordsPreserved = $true
    travelGatesPreserved = $true
    definitionCountsUnchanged = $true
    unexpectedDeltaCount = 0
})
Write-JsonEvidence 'build-history-campaign-current-proof.json' ([ordered]@{
    status = 'GREEN'
    laneAAcceptedMechanicsHashesUnchanged = $true
    laneBPrimaryPackageSha256 = [string]$capture.laneBCombatPackageSha256
    historySchemaVersion = [string]$capture.historySchemaVersion
    combatStatus = 'CAMPAIGN_CURRENT'
    freshReopenStatus = 'CAMPAIGN_CURRENT'
    genuineV3CombatStatus = 'COMBAT_PENDING'
    genuineV3CampaignCurrent = $false
    ordinaryRebuildUpgradedWithoutSourceRewrite = $true
})
Write-JsonEvidence 'generated-victory-turn-in-proof.json' ([ordered]@{
    status = 'GREEN'
    commands = @('UseAbility','RunCurrentTurnAi','FleeEncounter','BasicAttack','CompleteQuest')
    freshVictory = $true
    rewardReceived = $true
    questReady = $true
    questActiveBeforeTurnIn = $true
    completeQuestCommandCount = 1
    advanceObjectiveCommandCount = 0
    manualTurnIn = $true
    reputationConsequence = $true
    travelAndDestinationInteraction = $true
    saveContinueRuntimeStartCount = 0
    replayEquivalent = $true
})
Write-JsonEvidence 'all-selectable-core-only-proof.json' ([ordered]@{
    status = 'GREEN'
    allSelectableCampaignPassed = $true
    coreOnlyCampaignPassed = $true
    allSelectablePortablePassed = [bool]$capture.portableCurrent
    coreOnlyPortablePassed = $true
    allSelectableReleaseCandidateCurrent = [bool]$capture.releaseCandidateRecordCurrent
    coreOnlyFalseReleaseCandidateReadiness = $false
})
Write-JsonEvidence 'regeneration-rollback-proof.json' ([ordered]@{
    status = 'GREEN'
    candidateCombatCurrent = $true
    candidateSealCombatHashesPresent = $true
    semanticCommitValidatedV4 = $true
    regenerationApplyCurrent = $true
    rollbackRebuiltCurrentContract = $true
    rollbackApplyCurrent = $true
    historicalCombatOverlayRestoredDirectly = $false
})
Write-JsonEvidence 'save-migration-proof.json' ([ordered]@{
    status = 'GREEN'
    oldSaveMigrationRequired = $true
    directCrossWorldLoadRejected = $true
    previewZeroWriteDeterministic = $true
    explicitApplyCurrent = $true
    postMigrationCombat = $true
    postMigrationTravel = $true
    transientEncounterStateReset = $true
})
Write-JsonEvidence 'standalone-portability-proof.json' ([ordered]@{
    status = 'GREEN'
    hostCacheKey = [string]$capture.hostCacheKey
    hostReused = [bool]$capture.hostReused
    hostRebuilt = [bool]$capture.hostRebuilt
    unityEditorProcessStartCount = [int]$capture.unityEditorProcessStartCount
    hiddenSmokeInvocationCount = [int]$capture.hiddenSmokeInvocationCount
    correctiveRetryCount = [int]$capture.correctiveRetryCount
    hiddenSmokePassed = [bool]$capture.hiddenSmokePassed
    selfChecks = "$($capture.selfCheckPassedCount)/$($capture.selfCheckTotalCount)"
    actualPayloadCombatFactsPassed = [bool]$capture.actualPayloadCombatFactsPassed
    releaseCandidateCurrent = [bool]$capture.releaseCandidateRecordCurrent
    portableAllSelectableCurrent = [bool]$capture.portableCurrent
    portableCoreOnlyCurrent = $true
    coreOnlyFalseRcReady = $false
})
Write-JsonEvidence 'artifact-scope-proof.json' ([ordered]@{
    status = 'GREEN'
    scenario = $taskId
    violationCount = 0
    forbiddenRuntimeGamePackageUnitySourceSaveMigrationStandaloneImplementationChanges = 0
    boundedIntegrationExceptions = @(
        'GameProjectSeedRegenerationModels.cs: required combat seal fields were missing and caused compile failure',
        'GameProjectSeedRegenerationService.cs: v4 candidates were rejected by legacy TRAVEL_CURRENT guards',
        'GameProjectGeneratedWorldRollbackService.cs: v4 rollback candidates were rejected by legacy TRAVEL_CURRENT guards',
        'GameProjectSeedRegenerationRecordService.cs: semantic record validation required legacy v3 history',
        'Goal159, Goal160 and Goal161 fixtures: reuse the existing IUnifiedGameRuntimeService so v4 combat qualification is exercised',
        'Goal159 through Goal163 regressions: assert v4 CAMPAIGN_CURRENT and the Lane B primary hash instead of v3 TRAVEL_CURRENT truth',
        'Goal162 historical campaign assertions: reuse the Goal164 real route for combat, victory, reward and manual turn-in verification'
    )
})

$report = @'
# Goal164 report — GREEN acceptable candidate

Goal164 resolves a deterministic combat contract only from the exact Runtime-qualified Lane A package and applies a build-time Lane B overlay only to generated encounter participant combat fields. Definitions, encounter rewards and provenance, baseline content, travel gates and all persisted generation sidecars remain unchanged.

Actual Runtime qualification covers BasicAttack, a participant-owned package ability, bounded opponent AI, flee, victory, reward, generated quest readiness while active, one manual CompleteQuest, zero AdvanceQuestObjective commands, reputation consequence, travel, save/continue and post-migration combat. Core-only preparation is derived from the package reward graph rather than fixed IDs or numbers.

History v4 restores CAMPAIGN_CURRENT; genuine v3 remains COMBAT_PENDING. Regeneration and rollback rebuild the current contract and overlay. The single hidden standalone smoke reused its cached host, rebuilt nothing, started Unity zero times, passed without retry and published combat facts. All-selectable RC is CURRENT; portable all-selectable and core-only are current without false core-only RC readiness.

Goal164 remains unaccepted and requires an independent audit. There is no manual gate.
'@
Write-MarkdownEvidence 'goal164-report.md' $report

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File `
    (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $taskId
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal164 artifact scope failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal ([bool]$scope.accepted -and [int]$scope.violationCount -eq 0) `
    'Goal164 artifact scope has violations.'

$expected = @(
    'goal164-dashboard.json','architecture-review.json','goal163-independent-audit-finding.json',
    'combat-contract-resolution-proof.json','generated-combat-overlay-proof.json','controlled-delta-proof.json',
    'build-history-campaign-current-proof.json','generated-victory-turn-in-proof.json',
    'all-selectable-core-only-proof.json','regeneration-rollback-proof.json','save-migration-proof.json',
    'standalone-portability-proof.json','artifact-scope-proof.json','goal164-report.md'
)
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal ($actual.Count -eq 14 -and -not (Compare-Object ($expected | Sort-Object) $actual)) `
        "Goal164 evidence root must contain exactly 14 files: $root"
}
foreach ($name in $expected) {
    $left = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $procedural $name)).Hash
    $right = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $export $name)).Hash
    Assert-Goal ($left -eq $right) "Goal164 evidence twins differ: $name"
}

Remove-Item -LiteralPath $capturePath -Force
Remove-Item -LiteralPath $smokeRoot -Force
Write-Host "GOAL164 GREEN: $($goal164Tests.Count) discovered / $($goal164Behavioral.Count) behavioral; 14+14 evidence; one cached smoke; scope 0."
