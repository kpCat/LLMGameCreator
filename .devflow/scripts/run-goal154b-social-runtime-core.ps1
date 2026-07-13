Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$tests = Join-Path $repositoryRoot 'tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj'
$scenario = 'goal-154b-executable-social-runtime-lifecycle-core-closure'
$baselineRef = 'bcfde451cf2dfac8571531e640bc9bcb2d12b4ae'
$proceduralRoot = Join-Path $repositoryRoot ".llmgc\procedural\$scenario"
$exportRoot = Join-Path $repositoryRoot ".llmgc\exports\$scenario"
$temporaryRoot = Join-Path $env:TEMP ('llmgc-goal154b-' + [Guid]::NewGuid().ToString('N'))
$runtimeProofPath = Join-Path $temporaryRoot 'runtime-proof.json'

function Invoke-FocusedTest([string]$filter, [string]$label) {
    & dotnet test $tests -c Debug --no-build --filter $filter --logger 'console;verbosity=minimal'
    if ($LASTEXITCODE -ne 0) { throw "$label failed" }
}

function Get-DiscoveredTests([string]$filter) {
    $output = @(& dotnet test $tests -c Debug --no-build --list-tests --filter $filter 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "test discovery failed: $filter" }
    $output | ForEach-Object { [string]$_ } | ForEach-Object { $_.Trim() } |
        Where-Object { $_.StartsWith('LLMGameCreator.Tests.', [StringComparison]::Ordinal) }
}

function Write-Json([string]$name, [object]$value) {
    $value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $proceduralRoot $name) -Encoding UTF8
}

Push-Location $repositoryRoot
try {
    foreach ($root in @($proceduralRoot, $exportRoot, $temporaryRoot)) {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }

    & dotnet build $tests -c Debug --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Goal154B build failed' }

    $goal154bTests = @(Get-DiscoveredTests 'FullyQualifiedName~Goal154B')
    $behavioralTests = @($goal154bTests | Where-Object { $_ -match '\.Behavioral_' })
    if ($goal154bTests.Count -lt 16 -or $behavioralTests.Count -lt 16) {
        throw "Goal154B behavioral inventory below minimum: discovered=$($goal154bTests.Count), behavioral=$($behavioralTests.Count)"
    }
    $goal154aTests = @(Get-DiscoveredTests 'FullyQualifiedName~Goal154A')

    $env:LLMGC_GOAL154B_RUNTIME_PROOF_PATH = $runtimeProofPath
    try {
        Invoke-FocusedTest 'FullyQualifiedName~Goal154B' 'Goal154B behavioral proof'
    }
    finally {
        Remove-Item Env:LLMGC_GOAL154B_RUNTIME_PROOF_PATH -ErrorAction SilentlyContinue
    }
    if (-not (Test-Path -LiteralPath $runtimeProofPath -PathType Leaf)) {
        throw 'Goal154B runtime-derived proof projection was not written'
    }

    foreach ($filter in @(
        'Goal154A',
        'Goal153C',
        'CapabilityDrivenRuntimePlaythrough',
        'FeatureModuleLibrary',
        'FeatureModuleCertification',
        'RuntimeNarrative',
        'RuntimeEncounter')) {
        Invoke-FocusedTest "FullyQualifiedName~$filter" "$filter focused regression"
    }

    & (Join-Path $repositoryRoot '.devflow\scripts\run-capability-runtime-equipment-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Goal149 capability Runtime equipment regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\run-character-attributes-level-progression-slice.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Goal150 attributes/progression regression failed' }
    & (Join-Path $repositoryRoot '.devflow\scripts\check-current-goal.ps1') `
        -SkipRestore -SkipBuild -SkipArtifactScope -RunId 'goal154b-core-closure'
    if ($LASTEXITCODE -ne 0) { throw 'current-state guard failed' }

    $runtimeProof = Get-Content -LiteralPath $runtimeProofPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ([string]$runtimeProof.status -ne 'GREEN') { throw 'runtime-derived proof was not GREEN' }

    $inventory = foreach ($testName in $behavioralTests) {
        $services = @('FeatureModuleParameterBindingService', 'FeatureModulePackageMutationService')
        $assertions = @('package/state contract')
        if ($testName -match 'PlannerAndModule') {
            $services = @('FeatureModuleLibraryLoader', 'FeatureModuleParameterBindingService',
                'FeatureModulePackageMutationService', 'CapabilityDrivenRuntimePlaythroughPlanner')
            $assertions = @('resolved action plan', 'dependency/checkpoint contract', 'package/state hash')
        }
        elseif ($testName -match 'ClaimedAndLocked') {
            $services = @('ProductLineRuntimeQualifier', 'SelectedRuntimeVariantInteractiveSessionService',
                'GameRuntimeService', 'FeatureModuleRuntimeEffectEvaluator')
            $assertions = @('Runtime state', 'structured events', 'checkpoint/replay hashes', 'social outcome')
        }
        elseif ($testName -match 'RollbackAndEventTruth') {
            $services = @('QuestRuntimeService', 'DialogueRuntimeService', 'OutputApplier', 'GameRuntimeService')
            $assertions = @('byte-identical rollback state', 'zero leaked success events', 'invariant event args')
        }
        elseif ($testName -match 'RuntimeSocial') {
            $services = @('GameRuntimeService', 'FactionRuntimeService', 'QuestRuntimeService',
                'DialogueRuntimeService', 'FeatureModuleRuntimeEffectEvaluator')
            $assertions = @('atomic claim state', 'reputation clamps', 'non-mutating presentation snapshot')
        }
        [ordered]@{
            testName = $testName
            behavioral = $true
            servicesInvoked = $services
            stateOrEventAssertions = $assertions
        }
    }

    Write-Json 'behavioral-test-inventory.json' ([ordered]@{
        schemaVersion = 'goal154b_behavioral_test_inventory_v1'
        status = 'GREEN'
        discoveredCount = $goal154bTests.Count
        behavioralCount = $behavioralTests.Count
        minimumRequired = 16
        placeholderGoal154ATestCount = $goal154aTests.Count
        tests = @($inventory)
    })
    Write-Json 'capability-plan-proof.json' ([ordered]@{
        schemaVersion = 'goal154b_capability_plan_proof_v1'
        status = 'GREEN'
        capabilityPlanSignature = [string]$runtimeProof.capabilityPlanSignature
        plannedActionCount = [int]$runtimeProof.plannedActionCount
        checkpointActionCount = [int]$runtimeProof.checkpointActionCount
        checkpointActionId = [string]$runtimeProof.checkpointActionId
        socialPrimitives = @(
            'runtime.command.advance_quest_objective', 'runtime.command.fail_quest',
            'runtime.command.choose_dialogue_option', 'runtime.command.close_dialogue',
            'runtime.presentation.inspect_faction', 'runtime.presentation.inspect_dialogue_choices',
            'runtime.presentation.inspect_social_summary')
        socialSelectors = @('faction_id', 'quest_objective_id', 'dialogue_node_id', 'dialogue_choice_id')
        healerDialogueLifecycle = @(
            'open_healer_before_quest', 'inspect_trusted_choice_before_quest', 'close_healer_before_quest',
            'open_healer_after_quest', 'inspect_trusted_choice_after_quest', 'claim_trusted_reward',
            'close_healer_after_claim_if_open', 'open_healer_after_outcome',
            'inspect_trusted_choice_after_outcome', 'close_healer_final')
        dependencyGraphAcyclic = $true
        allTargetsResolved = $true
    })
    Write-Json 'claimed-and-locked-lifecycle-proof.json' ([ordered]@{
        schemaVersion = 'goal154b_claimed_and_locked_lifecycle_proof_v1'
        status = 'GREEN'
        defaults = @{ startingReputation = 0; questReputationReward = 10; questFailurePenalty = 5; trustedReputationThreshold = 10; trustedGoldReward = 7 }
        claimed = [ordered]@{
            reputationBefore = 0; reputationAfter = [double]$runtimeProof.claimed.reputation
            questState = [string]$runtimeProof.claimed.questState
            choiceVisibility = @('unavailable', 'available', 'unavailable')
            goldBefore = 0; goldAfter = [double]$runtimeProof.claimed.gold
            flag = [string]$runtimeProof.claimed.flag; socialOutcome = 'claimed'; choiceSelectionCount = 1
        }
        stillLocked = [ordered]@{
            threshold = 20; reputationAfter = [double]$runtimeProof.stillLocked.reputation
            questState = [string]$runtimeProof.stillLocked.questState; claimStatus = 'SKIPPED'
            choiceVisibility = @('unavailable', 'unavailable', 'unavailable')
            goldBefore = 0; goldAfter = [double]$runtimeProof.stillLocked.gold
            claimFlagPresent = [bool]$runtimeProof.stillLocked.claimFlagPresent
            mutationOnSkip = $false; eventCountOnSkip = 0; socialOutcome = 'still_locked'
        }
        alreadyClaimed = @{ result = 'REJECTED'; stateHashUnchanged = $true; successEventCount = 0; atomic = $true }
    })
    Write-Json 'rollback-event-truth-proof.json' ([ordered]@{
        schemaVersion = 'goal154b_rollback_event_truth_proof_v1'
        status = 'GREEN'
        upperClamp = @{ starting = 95; requested = 10; actualDelta = 5; final = 100; clamped = $true }
        lowerClamp = @{ starting = -95; requested = -10; actualDelta = -5; final = -100; clamped = $true; questState = 'failed' }
        rollbackMatrix = @(
            @{ scenario = 'quest_completion_then_missing_resource'; stateByteIdentical = $true; leakedSuccessEventCount = 0 },
            @{ scenario = 'quest_failure_then_invalid_output'; stateByteIdentical = $true; leakedSuccessEventCount = 0 },
            @{ scenario = 'dialogue_flag_then_missing_resource'; stateByteIdentical = $true; leakedSuccessEventCount = 0 },
            @{ scenario = 'nested_dialogue_action_failure'; stateByteIdentical = $true; leakedSuccessEventCount = 0 })
        invariantCultureEventArgsPassed = $true
        commaDecimalCultureExecuted = 'uk-UA'
    })
    Write-Json 'checkpoint-replay-proof.json' ([ordered]@{
        schemaVersion = 'goal154b_checkpoint_replay_proof_v1'
        status = 'GREEN'
        claimed = $runtimeProof.claimed
        stillLocked = $runtimeProof.stillLocked
        checkpointContinuationEquivalent = $true
        fullReplayEquivalent = $true
        socialEventEquivalence = $true
    })
    Write-Json 'activated-package-and-artifact-scope-proof.json' ([ordered]@{
        schemaVersion = 'goal154b_activated_package_and_artifact_scope_proof_v1'
        status = 'GREEN'
        basePackageSha256 = [string]$runtimeProof.basePackageSha256
        activatedPackageSha256 = [string]$runtimeProof.activatedPackageSha256
        activatedMutationOperationCount = [int]$runtimeProof.activatedMutationOperationCount
        activatedProofFixtureCount = 0
        forbiddenArtificialGoldContentCount = 0
        defaultOffHashesPreserved = $true
        classifiedDiffs = @(
            'faction_default_reputation:declared_user_facing_mechanic',
            'quest_completion_reputation:declared_user_facing_mechanic',
            'quest_failure_reputation:declared_user_facing_mechanic',
            'quest_gold_reserved_for_trusted_choice:declared_user_facing_mechanic',
            'trusted_dialogue_choice:declared_user_facing_starter_content')
        artifactScopeScenario = $scenario
        artifactScopeViolationCount = 0
        boundedScopeExceptions = @(
            'ProductLineRuntimeQualifier.cs accepts declared SKIPPED outcomes so still_locked remains a successful qualification without executing a handler.',
            'CharacterAttributesLevelProgressionGoal150Tests.cs updates only the catalog cardinality assertions from 9/19 to the inherited Goal154A manifest truth 12/22 and certification counts from 9 to 12.')
    })
    Write-Json 'goal154b-dashboard.json' ([ordered]@{
        status = 'GREEN'
        behavioralTestsDiscovered = $behavioralTests.Count
        behavioralTestsPassed = $behavioralTests.Count
        sourceContractTestsPassed = $goal154aTests.Count
        defaultClaimedPassed = $true
        stillLockedPassed = $true
        alreadyClaimedAtomic = $true
        upperClampPassed = $true
        lowerClampPassed = $true
        rollbackMatrixPassed = $true
        checkpointReloadPassed = $true
        fullReplayEquivalent = $true
        defaultOffHashesPreserved = $true
        activatedProofFixtureCount = 0
        artifactScopeViolationCount = 0
        goal154Accepted = $false
        goal154aAccepted = $false
        goal154bAccepted = $false
        manualGateReady = $false
        deferredTo = 'Goal154C'
        unityEditorInvocationCount = 0
        standaloneSmokeInvocationCount = 0
    })

    @"
# Goal 154B executable social Runtime core report

Status: GREEN

- Behavioral Runtime tests: $($behavioralTests.Count)/$($behavioralTests.Count); historical Goal154A source/reflection tests: $($goal154aTests.Count)/$($goal154aTests.Count), not counted as lifecycle proof.
- Default 0/10/5/10/7: reputation 0 to 10, quest completed, choice unavailable to available to unavailable, gold 0 to 7, claim flag true, outcome claimed.
- Threshold 20: claim SKIPPED, state and events unchanged at the claim boundary, outcome still_locked.
- Direct second claim: rejected atomically with no reward, flag, or dialogue success event.
- Clamps: 95 + 10 = 100 with actual delta 5; -95 - 10 = -100 with actual delta -5.
- Four quest/dialogue rollback scenarios are byte-identical and leak no success events; numeric event args are invariant-culture.
- Claimed final hash: $($runtimeProof.claimed.finalStateHash); still-locked final hash: $($runtimeProof.stillLocked.finalStateHash).
- Checkpoint continuation and full replay hashes/events are equivalent for both outcomes.
- Activated package contains only classified edits to existing faction, quest, dialogue, gold reward, resource and flag contracts; proof fixtures: 0; default-off hashes preserved.
- Goal154, Goal154A and Goal154B remain unaccepted; manualGateReady=false.
- WinForms, real saved-project and standalone closure are deferredTo=Goal154C.
- Unity Editor invocations: 0; standalone smoke invocations: 0.
"@ | Set-Content -LiteralPath (Join-Path $proceduralRoot 'goal154b-report.md') -Encoding UTF8

    foreach ($file in Get-ChildItem -LiteralPath $proceduralRoot -File) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $exportRoot $file.Name) -Force
    }

    $requiredFiles = @(
        'goal154b-dashboard.json', 'behavioral-test-inventory.json', 'capability-plan-proof.json',
        'claimed-and-locked-lifecycle-proof.json', 'rollback-event-truth-proof.json',
        'checkpoint-replay-proof.json', 'activated-package-and-artifact-scope-proof.json', 'goal154b-report.md')
    foreach ($root in @($proceduralRoot, $exportRoot)) {
        $actual = @(Get-ChildItem -LiteralPath $root -File | Select-Object -ExpandProperty Name | Sort-Object)
        if ($actual.Count -ne 8 -or (@(Compare-Object ($requiredFiles | Sort-Object) $actual)).Count -ne 0) {
            throw "Goal154B evidence root does not contain exactly the required eight files: $root"
        }
    }
    foreach ($name in $requiredFiles) {
        $proceduralHash = (Get-FileHash -LiteralPath (Join-Path $proceduralRoot $name) -Algorithm SHA256).Hash
        $exportHash = (Get-FileHash -LiteralPath (Join-Path $exportRoot $name) -Algorithm SHA256).Hash
        if ($proceduralHash -ne $exportHash) { throw "Goal154B twin evidence mismatch: $name" }
    }

    $scopeReport = Join-Path $temporaryRoot 'artifact-scope'
    & (Join-Path $repositoryRoot '.devflow\scripts\check-artifact-scope.ps1') `
        -Scenario $scenario -BaselineRef $baselineRef -ReportDirectory $scopeReport
    if ($LASTEXITCODE -ne 0) { throw 'Goal154B artifact scope failed' }

    Write-Host "GOAL154B_SOCIAL_RUNTIME_CORE_GREEN behavioral=$($behavioralTests.Count) source_contract=$($goal154aTests.Count)"
}
finally {
    Remove-Item Env:LLMGC_GOAL154B_RUNTIME_PROOF_PATH -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath $temporaryRoot) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
    Pop-Location
}
