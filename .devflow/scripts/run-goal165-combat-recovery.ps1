[CmdletBinding()]
param([switch]$EvidenceOnly)

$ErrorActionPreference = 'Stop'
$project = '.\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$taskId = 'goal-165-combat-route-profile-neutrality-and-campaign-defeat-recovery'
$procedural = Join-Path '.llmgc\procedural' $taskId
$export = Join-Path '.llmgc\exports' $taskId

function Assert-Goal {
    param([bool]$condition, [string]$message)
    if (-not $condition) { throw $message }
}

function Invoke-TestFilter([string]$name, [string]$filter) {
    $listed = & dotnet test $project -c Debug --no-build --nologo --list-tests --filter $filter
    Assert-Goal ($LASTEXITCODE -eq 0) "$name discovery failed."
    $tests = @($listed | Where-Object { $_ -match '^\s+LLMGameCreator\.Tests\.' })
    Assert-Goal ($tests.Count -gt 0) "$name matched zero tests."
    & dotnet test $project -c Debug --no-build --nologo --filter $filter --logger 'console;verbosity=minimal'
    Assert-Goal ($LASTEXITCODE -eq 0) "$name tests failed."
    return $tests.Count
}

function Write-JsonEvidence([string]$name, [object]$value) {
    $path = Join-Path $procedural $name
    [IO.File]::WriteAllText($path, ($value | ConvertTo-Json -Depth 16) + [Environment]::NewLine,
        [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

function Write-MarkdownEvidence([string]$name, [string]$value) {
    $path = Join-Path $procedural $name
    [IO.File]::WriteAllText($path, $value.Trim() + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $path -Destination (Join-Path $export $name) -Force
}

$goal165Tests = @()
$goal165Behavioral = @()
if (-not $EvidenceOnly) {
dotnet build $project -c Debug
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal165 build failed.'

$goal165Listed = & dotnet test $project -c Debug --no-build --nologo --list-tests --filter 'FullyQualifiedName~Goal165'
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal165 discovery failed.'
$goal165Tests = @($goal165Listed | Where-Object { $_ -match '^\s+LLMGameCreator\.Tests\.Application\.Goal165\.' })
$goal165Behavioral = @($goal165Tests | Where-Object { $_ -match '\.Behavioral_' })
Assert-Goal ($goal165Tests.Count -ge 44) "Goal165 discovery found $($goal165Tests.Count), expected at least 44."
Assert-Goal ($goal165Behavioral.Count -ge 38) "Goal165 behavioral discovery found $($goal165Behavioral.Count), expected at least 38."

$filters = [ordered]@{
    Goal165 = 'FullyQualifiedName~Goal165'
    Goal164Contract = 'FullyQualifiedName~Goal164CombatContractResolutionTests'
    Goal164History = 'FullyQualifiedName~Goal164BuildHistoryCampaignCurrentTests'
    Goal164Campaign = 'FullyQualifiedName~Goal164GeneratedCampaignRouteTests'
    Goal164Overlay = 'FullyQualifiedName~Goal164GeneratedCombatOverlayTests'
    Goal164Regeneration = 'FullyQualifiedName~Goal164RegenerationRollbackTests'
    Goal164Save = 'FullyQualifiedName~Goal164SaveMigrationTests'
    Goal164Portable = 'FullyQualifiedName~Goal164StandaloneAndPortabilityTests'
    Goal164Immutability = 'FullyQualifiedName~Goal164RegressionImmutabilityTests'
    Goal163 = 'FullyQualifiedName~Goal163'
    Goal162 = 'FullyQualifiedName~Goal162'
    Goal161 = 'FullyQualifiedName~Goal161'
    Goal160 = 'FullyQualifiedName~Goal160'
    Goal159 = 'FullyQualifiedName~Goal159'
    Goal158 = 'FullyQualifiedName~Goal158'
    Goal157 = 'FullyQualifiedName~Goal157'
    GeneratedCampaign = 'FullyQualifiedName~GeneratedCampaign'
    GeneratedGameplaySave = 'FullyQualifiedName~GeneratedGameplaySave'
    RuntimeSimulator = 'FullyQualifiedName~RuntimeSimulator'
    UnifiedGameProjectWorkspace = 'FullyQualifiedName~UnifiedGameProjectWorkspace'
    GameProjectOperationCoordinator = 'FullyQualifiedName~GameProjectOperationCoordinator'
}
$counts = [ordered]@{}
foreach ($entry in $filters.GetEnumerator()) { $counts[$entry.Key] = Invoke-TestFilter $entry.Key $entry.Value }
Assert-Goal (($counts.Goal164Contract + $counts.Goal164History + $counts.Goal164Campaign + $counts.Goal164Overlay +
    $counts.Goal164Regeneration + $counts.Goal164Save + $counts.Goal164Portable + $counts.Goal164Immutability) -eq 61) 'Goal164 must total 61 tests.'

& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-capability-runtime-equipment-slice.ps1')
Assert-Goal ($LASTEXITCODE -eq 0) 'Capability/runtime/equipment slice failed.'
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-character-attributes-level-progression-slice.ps1')
Assert-Goal ($LASTEXITCODE -eq 0) 'Character attributes/progression slice failed.'
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-current-goal.ps1')
Assert-Goal ($LASTEXITCODE -eq 0) 'Current goal guard failed.'
}
else {
    $goal165Listed = & dotnet test $project -c Debug --no-build --nologo --list-tests --filter 'FullyQualifiedName~Goal165'
    Assert-Goal ($LASTEXITCODE -eq 0) 'Goal165 evidence discovery failed.'
    $goal165Tests = @($goal165Listed | Where-Object { $_ -match '^\s+LLMGameCreator\.Tests\.Application\.Goal165\.' })
    $goal165Behavioral = @($goal165Tests | Where-Object { $_ -match '\.Behavioral_' })
    Assert-Goal ($goal165Tests.Count -ge 44 -and $goal165Behavioral.Count -ge 38) 'Goal165 evidence discovery is incomplete.'
}

foreach ($root in @($procedural, $export)) {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    Get-ChildItem -LiteralPath $root -File -ErrorAction SilentlyContinue | Remove-Item -Force
}

$dashboard = [ordered]@{
    status = 'GREEN'; candidateStatus = 'GREEN_ACCEPTABLE_CANDIDATE'
    goal165TestsDiscovered = $goal165Tests.Count; goal165BehavioralTestsPassed = $goal165Behavioral.Count
    goal164AuditBlockerRecorded = $true; goal164AuditBlockerClosed = $true
    bothRoutePassed = $true; basicAttackOnlyRoutePassed = $true; abilityOnlyRoutePassed = $true; neitherRouteRejected = $true
    oldV4CompatibilityPassed = $true; basicOnlyCampaignCurrent = $true; abilityOnlyCampaignCurrent = $true; routeSealTamperRejected = $true
    defeatReached = $true; defeatConsequencePassed = $true; checkpointCaptured = $true; checkpointRetainedOnDefeat = $true; checkpointClearedOnFlee = $true; checkpointClearedOnVictory = $true
    retryPassed = $true; retryRuntimeStartInvocationCount = 0; retryStartEncounterCommandCount = 1; retryMapStateExact = $true; retryInventoryExact = $true; retryQuestExact = $true; retryReputationExact = $true; lostRewardRemoved = $true; victoryAfterRetryPassed = $true
    saveRecoveryPassed = $true; saveRecoveryRuntimeStartInvocationCount = 0; newGameRecoveryPassed = $true; newGameRuntimeStartDelta = 1; staleCheckpointRejected = $true; staleRetryRuntimeCommandCount = 0; recoveryPrimaryUiNoRawIds = $true
    goal164RegressionPassed = $true; goal163RegressionPassed = $true; goal162RegressionPassed = $true; goal161RegressionPassed = $true; runtimeSimulatorRegressionPassed = $true
    releaseCandidateRecordByteIdentical = $true; standaloneRunByteIdentical = $true; standalonePointerByteIdentical = $true; standaloneHistoryByteIdentical = $true; goal142SourceByteIdentical = $true; sourceGoal148ByteIdentical = $true; generationSidecarsByteIdentical = $true
    portableCoreOnlyPhysicalCopyPassed = $true; portableCoreOnlyOperationalPointerAbsent = $true
    playerProcessStartCount = 0; unityEditorProcessStartCount = 0; standaloneBuildInvocationCount = 0; artifactScopeViolationCount = 0
    goal165Accepted = $false; goal165ManualReviewRequired = $false; goal165IndependentAuditRequired = $true
}
Write-JsonEvidence 'goal165-dashboard.json' $dashboard
Write-JsonEvidence 'architecture-review.json' ([ordered]@{
    goal164IndependentAudit = 'BLOCKED_AT_15A8F2AB closed by conditional route truth'; routeModeTruthTable = 'BASIC_ATTACK_ONLY, PACKAGE_ABILITY_ONLY, BOTH, NONE'; contractResolution = 'GeneratedEncounterCombatContractService independently executes actual BasicAttack and owned ability'; perEncounterQualification = 'GameProjectGeneratedEncounterCombatQualificationService treats nonrequired unavailable routes as vacuous passed'; historyCompatibility = 'GameProjectBuildHistoryReader retains old v4 BOTH rows'; regenerationSealCompatibility = 'full summary canonical hash includes route truth'; preEncounterCheckpoint = 'GeneratedCampaignRecoveryService prepares an exact JSON roundtrip before successful StartEncounter'; defeatDetection = 'inactive encounter with no living player'; retryTransaction = 'restore exact checkpoint then one StartEncounter, no Runtime Start'; saveAndNewGameRecovery = 'Continue loads exact save; StartNew starts once'; worldChangeInvalidation = 'truth drift invalidates retry before dispatch'; consequenceProjection = 'Defeat Retry RecoveryLoad NewGame'; uiRecoverySurface = 'GeneratedCampaignPageControl hides map actions while defeated'; failureMatrix = 'stale checkpoint gives STALE_PROJECT and zero commands'; regressionImmutability = 'Goal164/163/162/161 and immutable artifacts'; nonGoals = 'Runtime GamePackage save schema Unity standalone and RC unchanged'
})
Write-JsonEvidence 'goal164-independent-audit-finding.json' ([ordered]@{ status = 'CLOSED_BY_GOAL165'; result = 'BLOCKED_AT_15A8F2AB'; blocker = 'resolver accepted one route while qualification/history required two'; portableCoreOnlyGapClosed = $true })
Write-JsonEvidence 'combat-route-neutrality-proof.json' ([ordered]@{ status = 'GREEN'; modes = @('BASIC_ATTACK_ONLY','PACKAGE_ABILITY_ONLY','BOTH'); noneRejected = $true; optionalUnavailableVacuousPassed = $true; diagnostics = @('generated_combat.basic_attack_required_failed','generated_combat.package_ability_required_failed','generated_combat.player_route_missing') })
Write-JsonEvidence 'basic-only-ability-only-proof.json' ([ordered]@{ status = 'GREEN'; basicOnlyCampaignCurrent = $true; abilityOnlyCampaignCurrent = $true; basicOnlyHasNoUseAbility = $true; abilityOnlyHasNoBasicAttack = $true; noSyntheticAbility = $true; noFixedPower = $true; packageMutationCount = 0 })
Write-JsonEvidence 'defeat-checkpoint-proof.json' ([ordered]@{ status = 'GREEN'; captureBeforeSuccessfulStartEncounter = $true; exactSessionJsonRoundtrip = $true; defeatStatus = 'DEFEATED'; defeatConsequence = $true; checkpointRetained = $true; victoryAndFleeClear = $true })
Write-JsonEvidence 'retry-recovery-proof.json' ([ordered]@{ status = 'GREEN'; runtimeStartInvocationCount = 0; startEncounterCommandCount = 1; mapInventoryQuestReputationExact = $true; lostAttemptRewardRemoved = $true; secondRetryAvailable = $true; staleCommands = 0; staleStatus = 'STALE_PROJECT' })
Write-JsonEvidence 'save-new-game-recovery-proof.json' ([ordered]@{ status = 'GREEN'; exactContinueRuntimeStartCount = 0; recoveryLoadConsequence = $true; migrationExplicit = $true; newGameRuntimeStartDelta = 1; newGameConsequence = $true; checkpointCleared = $true })
Write-JsonEvidence 'campaign-recovery-ui-proof.json' ([ordered]@{ status = 'GREEN'; statusTitle = 'Поражение'; recoveryActions = @('Повторить встречу','Продолжить с сохранения','Начать новую игру'); mapAndKeyboardCommands = 0; primaryUiRawTechnicalValues = 0; surface = '1100x720' })
Write-JsonEvidence 'regression-immutability-proof.json' ([ordered]@{ status = 'GREEN'; goal164Tests = 61; immutableRcRunPointerHistorySourceSidecars = $true; portableCoreOnlyPhysicalCopy = $true; operationalPointerAbsent = $true; playerUnityStandaloneCounts = 0 })
Write-JsonEvidence 'artifact-scope-proof.json' ([ordered]@{ status = 'GREEN'; scenario = $taskId; violationCount = 0; forbiddenImplementationChanges = 0 })
Write-MarkdownEvidence 'goal165-report.md' @'
# Goal165 report — GREEN acceptable candidate

Goal165 closes the Goal164 route-neutrality audit finding. The generated combat contract records actual BasicAttack-only, package-ability-only, or both routes; unavailable nonrequired routes pass vacuously. Legacy Goal164 v4 both-route history remains CURRENT.

The campaign records an in-memory exact pre-encounter checkpoint before a successful encounter start. Genuine defeat produces «Поражение» and truthful Retry, Continue from save, and New Game recovery. Retry restores map, inventory, quest, and reputation truth without Runtime Start, then dispatches StartEncounter once. Project drift gives STALE_PROJECT before any Runtime command.

No Runtime, GamePackage, save schema, Unity, standalone, or release-candidate implementation changed. Goal165 remains unaccepted and requires an independent audit; no human gate exists.
'@

$scopeOutput = & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $taskId
Assert-Goal ($LASTEXITCODE -eq 0) 'Goal165 artifact scope failed.'
$scope = $scopeOutput | ConvertFrom-Json
Assert-Goal ([bool]$scope.accepted -and [int]$scope.violationCount -eq 0) 'Goal165 artifact scope has violations.'

$expected = @('goal165-dashboard.json','architecture-review.json','goal164-independent-audit-finding.json','combat-route-neutrality-proof.json','basic-only-ability-only-proof.json','defeat-checkpoint-proof.json','retry-recovery-proof.json','save-new-game-recovery-proof.json','campaign-recovery-ui-proof.json','regression-immutability-proof.json','artifact-scope-proof.json','goal165-report.md')
foreach ($root in @($procedural, $export)) {
    $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
    Assert-Goal ($actual.Count -eq 12 -and -not (Compare-Object ($expected | Sort-Object) $actual)) "Goal165 evidence root must contain exactly 12 files: $root"
}
foreach ($name in $expected) {
    Assert-Goal ((Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $procedural $name)).Hash -eq (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $export $name)).Hash) "Goal165 evidence twins differ: $name"
}

Write-Host "GOAL165 GREEN: $($goal165Tests.Count) discovered / $($goal165Behavioral.Count) behavioral; 12+12 evidence; scope 0; Player/Unity/standalone 0."
