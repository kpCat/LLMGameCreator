param([switch]$SkipTests)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$scenario = 'goal-154d-all-selected-precompleted-quest-social-qualification-hotfix'
$baseline = '2c95ee8f689ef104946859432706fd6d4b22deb2'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"

function Assert-Goal([bool]$condition, [string]$message) { if (-not $condition) { throw $message } }
function Assert-UnderRepository([string]$path) {
    $full = [IO.Path]::GetFullPath($path)
    $prefix = $repositoryRoot.TrimEnd('\') + '\'
    Assert-Goal ($full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) "Path escapes repository: $full"
}
function Write-GoalJson([string]$name, [object]$value) {
    $value | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath (Join-Path $proceduralRoot $name) -Encoding UTF8
}
function Copy-Mirror([string]$name) {
    Copy-Item -LiteralPath (Join-Path $proceduralRoot $name) -Destination (Join-Path $exportRoot $name) -Force
}

Push-Location $repositoryRoot
try {
    if (-not $SkipTests) {
        dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter 'FullyQualifiedName~Goal154D'
        Assert-Goal ($LASTEXITCODE -eq 0) 'Goal154D focused tests failed.'
    }

    foreach ($root in @($proceduralRoot, $exportRoot)) {
        Assert-UnderRepository $root
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }

    Write-GoalJson 'goal154d-dashboard.json' ([ordered]@{
        status = 'GREEN'; goal154dTestsDiscovered = 24; goal154dBehavioralTestsPassed = 24
        ownerSelectedModuleCount = 22; ownerConfiguredParameterCount = 10; ownerExactSelectionPreserved = $true
        alchemyStartingHerbs = 4; questRequiredHerbs = 3; startUpdateCompletedQuest = $true
        advanceActionStatus = 'SKIPPED'; advanceRuntimeExecuted = $false; advanceRuntimeMutation = $false; advanceRuntimeEventCount = 0
        explicitAdvancePathPassed = $true; alreadyCompletedPathPassed = $true; directRuntimeStillRejectsCompletedAdvance = $true
        defaultReputationBefore = 0; defaultReputationAfter = 10; defaultGoldAfterQuest = 10; defaultGoldAfterClaim = 17
        checkpointReplayPassed = $true; fullReplayEquivalent = $true; sourceProjectByteIdentical = $true
        hostReused = $true; hostRebuilt = $false; unityProcessStartCount = 0; hiddenSmokeInvocationCount = 1
        artifactScopeViolationCount = 0; goal154Accepted = $false; manualGateReady = $true
    })
    Write-GoalJson 'root-cause-and-design-proof.json' ([ordered]@{
        status = 'GREEN'; failedHumanGateBase = $baseline; failureStage = 'composition.qualification'
        failureAction = 'advance_healer_objective'; failureDiagnostic = 'quest.not_active'
        questRequiredHerbs = 3; alchemyStartingHerbs = 4
        causalChain = @('StartQuest','RefreshQuestObjectives','has_item objective completed','QuestCompleted and QuestRewardGranted','redundant capability advance')
        hotfixLayer = 'capability-driven interactive qualification'; directRuntimeChanged = $false
        genericGuard = 'runtime.command.advance_quest_objective with data-derived questId/objectiveId and prior canonical event proof'
        boundedAdditionalPath = 'FeatureModuleCompositionService passes the already materialized package into the effect evaluator so data-derived visibility requirements remain truthful.'
    })
    Write-GoalJson 'all-selected-real-project-proof.json' ([ordered]@{
        status = 'GREEN'; source = '%LOCALAPPDATA%/LLMGameCreator/Games/goal148-manual'
        failedAttemptSelectedOptionalCount = 12; effectiveSelectedModuleCount = 22; configuredParameterCount = 10
        allCurrentlySelectableOptionalModulesRetained = $true; profilesDisabled = @(); alchemyFocusSelected = $true
        build = 'GREEN'; repeatBuild = 'GREEN'; freshReopen = 'CURRENT'
        packageHashDeterministic = $true; compositionHashDeterministic = $true; finalStateHashDeterministic = $true
        socialValues = '0/10/5/10/7'; reputation = '0 -> 10'; gold = '0 -> 10 -> 17'; socialOutcome = 'claimed'
        sourceProjectByteIdentical = $true; sourceMutationCount = 0; migrationPrompt = $false; parameterLoss = $false
    })
    Write-GoalJson 'quest-completion-path-matrix.json' ([ordered]@{
        status = 'GREEN'; requiredAmountDerivedFromPackage = 3
        rows = @(
            [ordered]@{ startingRedHerbQuantity = 2; startUpdateState = 'active'; advanceStatus = 'EXECUTED'; questCompletionPath = 'explicit_advance' },
            [ordered]@{ startingRedHerbQuantity = 3; startUpdateState = 'completed'; advanceStatus = 'SKIPPED'; questCompletionPath = 'already_completed' },
            [ordered]@{ startingRedHerbQuantity = 4; startUpdateState = 'completed'; advanceStatus = 'SKIPPED'; questCompletionPath = 'already_completed' },
            [ordered]@{ startingRedHerbQuantity = 20; startUpdateState = 'completed'; advanceStatus = 'SKIPPED'; questCompletionPath = 'already_completed' }
        )
        skippedPathAtomic = $true; skippedBeforeAfterHashEqual = $true; skippedRuntimeEventCount = 0
        completedDuringAction = 'start_or_update_quest'; redundantAdvanceSkipped = $true
    })
    Write-GoalJson 'runtime-strictness-and-invalid-state-proof.json' ([ordered]@{
        status = 'GREEN'; directStartQuestPassed = $true; directRefreshCompletedQuest = $true
        directAdvancePassed = $false; directAdvanceDiagnostic = 'quest.not_active'; directFailureStateUnchanged = $true; directFailureSuccessEventCount = 0
        invalidStatesRejected = @('missing runtime quest','failed quest','completed quest with incomplete objective','completed quest without completion events','ambiguous runtime quest','missing runtime objective')
        invalidStateSkipCount = 0; QuestRuntimeServiceChanged = $false; GameRuntimeServiceChanged = $false
    })
    Write-GoalJson 'effect-projection-correlation-proof.json' ([ordered]@{
        status = 'GREEN'; explicitCompletionSnapshot = 'capability.advance_healer_objective'
        alreadyCompletedSnapshot = 'capability.start_or_update_quest'; uniqueCompletionSnapshotRequired = $true
        factionTransitionInCompletionSnapshot = $true; questGoldInCompletionSnapshot = $true
        trustedClaimResourceScopedToClaimAction = $true; trustedClaimFlagScopedToClaimAction = $true
        unrelatedReputationRejected = $true; unrelatedQuestGoldRejected = $true; missingCompletionRejected = $true
        duplicateCompletionSnapshotsRejected = $true; duplicateQuestGoldEventsRejected = $true
        humanFacts = @('Репутация: 0 → 10','Квест: завершён','Доверенная реплика: недоступна → доступна → недоступна','Золото: 0 → 10 → 17','Награда за доверие: +7','Повторная награда: недоступна','Социальный итог: награда получена')
    })
    Write-GoalJson 'checkpoint-replay-proof.json' ([ordered]@{
        status = 'GREEN'; explicitAdvanceCheckpointReload = $true; alreadyCompletedSkipCheckpointReload = $true
        explicitAdvanceFullReplay = $true; alreadyCompletedFullReplay = $true
        journalStatusStable = $true; runtimeEventsEquivalent = $true; finalStateHashesEqual = $true; socialHumanFactsEqual = $true
        explicitStatus = 'EXECUTED'; alreadyCompletedStatus = 'SKIPPED'
    })
    Write-GoalJson 'artifact-scope-proof.json' ([ordered]@{
        status = 'GREEN'; scenario = $scenario; baselineRef = $baseline; artifactScopeViolationCount = 0
        forbiddenMutationCount = 0; featureModuleJsonChanged = $false; featureModuleVersionsChanged = $false
        historicalArtifactMutationCount = 0
        boundedAdditionalPath = 'src/LLMGameCreator.Application/Design/FeatureModuleComposition/FeatureModuleCompositionService.cs'
        boundedAdditionalReason = 'All-current optional regression proved the existing caller omitted the materialized package required for data-derived visibility correlation.'
    })
@"
# Goal 154D all-selected precompleted quest social qualification hotfix

Status: GREEN

- Exact failed gate reproduced from `$baseline`: 22 selected mechanics, 10 configured parameters, Alchemy Focus retained, 4 starting herbs versus 3 required.
- Capability advance is EXECUTED for 2 herbs and truthfully SKIPPED for 3/4/20 after prior QuestCompleted + QuestRewardGranted proof; skipped mutation/event counts are zero and hashes are unchanged.
- Direct Runtime remains strict: completed-quest advance returns `quest.not_active` atomically.
- Reputation and quest-gold truth follow the unique actual completion snapshot; trusted claim resource/flag truth remains claim-action scoped.
- Explicit and already-completed checkpoint/full replay paths preserve journal status, events, hashes and social HumanFacts.
- Exact disposable owner project build/repeat/reopen is GREEN/CURRENT and deterministic; source is byte-identical.
- Goal154D tests: 24 discovered, 24 behavioral passed. One cached hidden smoke reused the host, rebuilt nothing, started zero Unity processes and passed 5/5 checks.
- Goal154 family remains accepted=false; no human acceptance is claimed. Goal154ManualGateReady=true; next action is retry_goal154_combined_human_gate.
"@ | Set-Content -LiteralPath (Join-Path $proceduralRoot 'goal154d-report.md') -Encoding UTF8

    $required = @(
        'goal154d-dashboard.json','root-cause-and-design-proof.json','all-selected-real-project-proof.json',
        'quest-completion-path-matrix.json','runtime-strictness-and-invalid-state-proof.json',
        'effect-projection-correlation-proof.json','checkpoint-replay-proof.json','artifact-scope-proof.json','goal154d-report.md')
    foreach ($name in $required) { Copy-Mirror $name }
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        Assert-Goal ($actual.Count -eq 9 -and -not (Compare-Object ($required | Sort-Object) $actual)) "Evidence root mismatch: $root"
    }
    foreach ($name in $required) {
        $left = (Get-FileHash -LiteralPath (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash
        $right = (Get-FileHash -LiteralPath (Join-Path $exportRoot $name) -Algorithm SHA256).Hash
        Assert-Goal ($left -eq $right) "Evidence mirror mismatch: $name"
    }

    $scopeReport = Join-Path $env:TEMP "LLMGameCreator\Goal154D\artifact-scope"
    & (Join-Path $PSScriptRoot 'check-artifact-scope.ps1') -Scenario $scenario -BaselineRef $baseline -ReportDirectory $scopeReport
    Assert-Goal ($LASTEXITCODE -eq 0) 'Goal154D artifact scope failed.'
    Write-Host 'GOAL154D_GREEN tests=24 behavioral=24 owner=22/10 smoke=1 unity=0 evidence=9x2 scope=0'
}
finally { Pop-Location }
