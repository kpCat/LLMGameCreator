using System.Globalization;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GameProjectGeneratedEncounterCombatQualificationService
{
    private readonly GeneratedCampaignQuestReadinessService _questReadiness;
    private readonly GeneratedCampaignConsequenceProjector _consequences;

    public GameProjectGeneratedEncounterCombatQualificationService(
        GeneratedCampaignQuestReadinessService? questReadiness = null,
        GeneratedCampaignConsequenceProjector? consequences = null)
    {
        _questReadiness = questReadiness ?? new GeneratedCampaignQuestReadinessService();
        _consequences = consequences ?? new GeneratedCampaignConsequenceProjector();
    }

    public GameProjectGeneratedEncounterCombatSummary Qualify(
        GamePackageDefinition package,
        SeededGeneratedProjectSourceValidationResult strictSource,
        GeneratedEncounterCombatContract contract,
        GeneratedEncounterCombatBindingResult bindings,
        GeneratedWorldEncounterCombatOverlayDocument overlay,
        IUnifiedGameRuntimeService runtime)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(strictSource);
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(bindings);
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(runtime);
        if (!bindings.Passed)
            return Invalid(contract, overlay, bindings.Diagnostics);
        if (contract.QualifiedActions.Count == 0)
            return Invalid(contract, overlay, ["generated_combat.qualified_action_missing"]);
        if (contract.QualifiedActions.Any(item => !DescriptorDefinitionCurrent(package, item)))
            return Invalid(contract, overlay, ["generated_combat.qualified_action_definition_changed"]);
        if (bindings.Bindings.Count == 0)
            return new GameProjectGeneratedEncounterCombatSummary
            {
                Passed = true,
                Status = "ABSENT",
                Overlay = overlay
            };

        var packageBefore = GeneratedEncounterCombatCanonical.Hash(package);
        var ordered = bindings.Bindings.OrderBy(item => item.EncounterSeedId, StringComparer.Ordinal).ToList();
        var routeResults = ordered.Select(binding => ExecuteEncounterRoute(
            package, binding.PackageEncounterId, runtime, contract,
            completeQuest: false)).ToList();
        var representative = SelectRepresentative(package, strictSource, ordered);
        if (representative is null)
            return Invalid(contract, overlay, ["generated_combat.representative_campaign_missing"]);
        var campaign = ExecuteEncounterRoute(package, representative.Binding.PackageEncounterId, runtime,
            contract, completeQuest: true, representative.PreparationEncounterIds,
            representative.QuestId);
        var replay = ExecuteEncounterRoute(package, representative.Binding.PackageEncounterId, runtime,
            contract, completeQuest: true, representative.PreparationEncounterIds,
            representative.QuestId);
        var replayPassed = campaign.Passed && replay.Passed
                           && campaign.ActionKinds.SequenceEqual(replay.ActionKinds, StringComparer.Ordinal)
                           && string.Equals(campaign.FinalStateHash, replay.FinalStateHash, StringComparison.Ordinal)
                           && campaign.RewardPassed == replay.RewardPassed
                           && campaign.ManualTurnInPassed == replay.ManualTurnInPassed;
        var packageAfter = GeneratedEncounterCombatCanonical.Hash(package);
        var allBasic = routeResults.All(item => item.BasicAttackPassed);
        var allAbilities = routeResults.All(item => item.PackageAbilityPassed);
        var allPlayerRoutes = routeResults.All(item => item.PlayerRoutePassed);
        var allAi = routeResults.All(item => item.OpponentAiPassed);
        var allFlee = routeResults.All(item => item.FleePassed);
        var allVictory = routeResults.All(item => item.VictoryPassed);
        var exact = routeResults.All(item => item.ExactPackageReferencePassed)
                    && campaign.ExactPackageReferencePassed && replay.ExactPackageReferencePassed;
        var unchanged = string.Equals(packageBefore, packageAfter, StringComparison.Ordinal);
        var passed = routeResults.All(item => item.Passed)
                     && campaign.Passed
                     && campaign.RewardPassed
                     && campaign.GeneratedQuestReadyPassed
                     && campaign.ManualTurnInPassed
                     && campaign.CompleteQuestCommandCount == 1
                     && campaign.AdvanceObjectiveCommandCount == 0
                     && campaign.ConsequencePassed
                     && replayPassed
                      && allPlayerRoutes && allBasic && allAbilities && allAi && allFlee && allVictory
                     && exact && unchanged;
        var diagnostics = routeResults.SelectMany(item => item.Diagnostics)
            .Concat(campaign.Diagnostics).Concat(replay.Diagnostics)
            .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        if (!campaign.RewardPassed) diagnostics.Add("generated_combat.reward_failed");
        if (!campaign.GeneratedQuestReadyPassed) diagnostics.Add("generated_combat.generated_quest_not_ready");
        if (!campaign.ManualTurnInPassed) diagnostics.Add("generated_combat.manual_turn_in_failed");
        if (campaign.CompleteQuestCommandCount != 1) diagnostics.Add("generated_combat.complete_quest_count_invalid");
        if (campaign.AdvanceObjectiveCommandCount != 0) diagnostics.Add("generated_combat.advance_objective_dispatched");
        if (!campaign.ConsequencePassed) diagnostics.Add("generated_combat.consequence_failed");
        if (!replayPassed) diagnostics.Add("generated_combat.replay_failed");
        if (!exact) diagnostics.Add("generated_combat.package_reference_failed");
        if (!unchanged) diagnostics.Add("generated_combat.package_mutated_during_runtime");
        if (!passed && diagnostics.Count == 0) diagnostics.Add("generated_combat.qualification_failed");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        return new GameProjectGeneratedEncounterCombatSummary
        {
            Present = true,
            Passed = passed,
            Status = passed ? "CAMPAIGN_CURRENT" : "INVALID",
            ContractId = contract.ContractId,
            ContractSourcePackageSha256 = contract.SourcePackageSha256,
            GeneratedEncounterCount = ordered.Count,
            QualifiedEncounterCount = routeResults.Count(item => item.Passed),
            ExactPackageSha256 = overlay.OutputPackageSha256,
            ExactPackageReferencePassed = exact,
            PackageShaUnchangedDuringRuntime = unchanged,
            RouteMode = contract.QualificationSummary.RouteMode,
            BasicAttackAvailable = contract.QualificationSummary.BasicAttackAvailable,
            BasicAttackRequired = contract.QualificationSummary.BasicAttackRequired,
            PackageAbilityAvailable = contract.QualificationSummary.PackageAbilityAvailable,
            PackageAbilityRequired = contract.QualificationSummary.PackageAbilityRequired,
            BasicAttackPassed = allBasic,
            PackageAbilityPassed = allAbilities,
            PlayerRoutePassed = allPlayerRoutes,
            QualifiedActionCount = contract.QualifiedActionCount,
            QualifiedBasicAttackCount = contract.QualifiedBasicAttackCount,
            QualifiedPackageAbilityCount = contract.QualifiedPackageAbilityCount,
            QualifiedActionsSha256 = contract.QualifiedActionsSha256,
            QualifiedActions = contract.QualifiedActions,
            OpponentAiPassed = allAi,
            VictoryPassed = allVictory,
            FleePassed = allFlee,
            RewardPassed = campaign.RewardPassed,
            GeneratedQuestReadyPassed = campaign.GeneratedQuestReadyPassed,
            ManualTurnInPassed = campaign.ManualTurnInPassed,
            CompleteQuestCommandCount = campaign.CompleteQuestCommandCount,
            AdvanceObjectiveCommandCount = campaign.AdvanceObjectiveCommandCount,
            ConsequencePassed = campaign.ConsequencePassed,
            ReplayPassed = replayPassed,
            FinalStateHash = campaign.FinalStateHash,
            RuntimeFrames = campaign.Frames,
            HumanReviewFacts = HumanFacts(passed, campaign),
            TechnicalDetails = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["representativeEncounterSource"] = representative.Binding.EncounterSeedId,
                ["representativeQuestId"] = representative.QuestId,
                ["campaignPreparationEncounterIds"] = string.Join(",", representative.PreparationEncounterIds),
                ["orderedActionKinds"] = string.Join(">", campaign.ActionKinds),
                ["qualifiedEncounterCount"] = routeResults.Count(item => item.Passed).ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            },
            Overlay = overlay,
            Diagnostics = diagnostics
        };
    }

    private EncounterRouteResult ExecuteEncounterRoute(
        GamePackageDefinition package,
        string encounterId,
        IUnifiedGameRuntimeService runtime,
        GeneratedEncounterCombatContract contract,
        bool completeQuest,
        IReadOnlyList<string>? preparationEncounterIds = null,
        string? questId = null)
    {
        var packageBefore = GeneratedEncounterCombatCanonical.Hash(package);
        var diagnostics = new List<string>();
        var actionKinds = new List<string>();
        var frames = new List<GeneratedEncounterCombatRuntimeFrame>();
        var routeMode = contract.RouteMode;
        var basicObserved = TrySinglePlayerAction(package, encounterId, runtime, contract,
            GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK, out var basicAi);
        var abilityObserved = TrySinglePlayerAction(package, encounterId, runtime, contract,
            GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY, out var abilityAi);
        var basicRequired = routeMode is GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY
            or GeneratedEncounterCombatRouteMode.BOTH;
        var abilityRequired = routeMode is GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY
            or GeneratedEncounterCombatRouteMode.BOTH;
        var basic = !basicRequired || basicObserved;
        var ability = !abilityRequired || abilityObserved;
        var ai = basicRequired ? basicAi : abilityRequired ? abilityAi : basicAi || abilityAi;
        var flee = ExecuteFlee(package, encounterId, runtime);
        var victory = ExecuteVictory(package, encounterId, runtime, completeQuest,
            preparationEncounterIds ?? [], contract, questId);
        actionKinds.AddRange(victory.ActionKinds);
        frames.AddRange(victory.Frames);
        if (!basic) diagnostics.Add("generated_combat.basic_attack_required_failed");
        if (!ability) diagnostics.Add("generated_combat.package_ability_required_failed");
        if (basicRequired && !basicObserved || abilityRequired && !abilityObserved)
            diagnostics.Add("generated_combat.qualified_action_no_progress");
        if (!basicObserved && !abilityObserved) diagnostics.Add("generated_combat.player_route_missing");
        if (!ai) diagnostics.Add("generated_combat.opponent_ai_failed");
        if (!flee) diagnostics.Add("generated_combat.flee_failed");
        if (!victory.VictoryPassed) diagnostics.Add("generated_combat.victory_no_progress");
        diagnostics.AddRange(victory.Diagnostics);
        var packageAfter = GeneratedEncounterCombatCanonical.Hash(package);
        var exact = string.Equals(packageBefore, packageAfter, StringComparison.Ordinal);
        if (!exact) diagnostics.Add("generated_combat.package_mutated_during_runtime");
        var passed = basic && ability && ai && flee && victory.VictoryPassed && exact
                     && (!completeQuest || victory.ManualTurnInPassed && victory.ConsequencePassed);
        return new EncounterRouteResult
        {
            Passed = passed,
            BasicAttackAvailable = basicObserved,
            BasicAttackPassed = basic,
            PackageAbilityAvailable = abilityObserved,
            PackageAbilityPassed = ability,
            PlayerRoutePassed = basicObserved || abilityObserved,
            OpponentAiPassed = ai,
            FleePassed = flee,
            VictoryPassed = victory.VictoryPassed,
            RewardPassed = victory.RewardPassed,
            GeneratedQuestReadyPassed = victory.GeneratedQuestReadyPassed,
            ManualTurnInPassed = victory.ManualTurnInPassed,
            CompleteQuestCommandCount = victory.CompleteQuestCommandCount,
            AdvanceObjectiveCommandCount = 0,
            ConsequencePassed = victory.ConsequencePassed,
            ExactPackageReferencePassed = exact,
            FinalStateHash = victory.FinalStateHash,
            ActionKinds = actionKinds,
            Frames = frames,
            Diagnostics = diagnostics
        };
    }

    private static bool TrySinglePlayerAction(
        GamePackageDefinition package,
        string encounterId,
        IUnifiedGameRuntimeService runtime,
        GeneratedEncounterCombatContract contract,
        GeneratedEncounterCombatQualifiedActionKind actionKind,
        out bool opponentAiPassed)
    {
        opponentAiPassed = false;
        var session = Start(package, runtime, encounterId);
        if (session is null) return false;
        session = AdvanceToPlayer(package, runtime, session, out _);
        if (session is null) return false;
        var encounter = session.GameplayState.ActiveEncounter!;
        var player = Current(encounter);
        var target = encounter.Participants.FirstOrDefault(item => item.Alive && !IsPlayer(item.Team));
        if (target is null) return false;
        var descriptors = contract.QualifiedActions.Where(item => item.ActionKind == actionKind)
            .OrderBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256, StringComparer.Ordinal).ToList();
        foreach (var descriptor in descriptors)
        {
            var attempt = GeneratedEncounterCombatCanonical.Clone(session);
            var before = GeneratedEncounterCombatCanonical.Clone(attempt);
            if (!DescriptorDefinitionCurrent(package, descriptor)) continue;
            var command = actionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                ? GameRuntimeCommand.BasicAttack(player.Id, target.Id)
                : GameRuntimeCommand.UseAbility(descriptor.AbilityId, player.Id, target.Id);
            var result = runtime.ExecuteGameplayCommand(package, attempt, command);
            if (!result.Success || !GeneratedEncounterCombatContractService.TryObserveSupportedEffect(
                    package, before, result.Session, target.Id,
                    actionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY, out var observed)
                || !GeneratedEncounterCombatContractService.MatchesObservedEffect(descriptor, observed)) continue;
            opponentAiPassed = RunOpponentAi(package, runtime, result.Session);
            return true;
        }
        return false;
    }

    private static bool ExecuteFlee(
        GamePackageDefinition package,
        string encounterId,
        IUnifiedGameRuntimeService runtime)
    {
        var session = Start(package, runtime, encounterId);
        if (session is null) return false;
        var before = GeneratedEncounterCombatCanonical.Hash(session.GameplayState.Inventories);
        var flee = runtime.ExecuteGameplayCommand(package, session,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });
        return flee.Success
               && flee.Session.GameplayState.ActiveEncounter is { Active: false }
               && !flee.GameplayEvents.Any(item => item.Type is GameRuntimeEventType.EncounterWon
                   or GameRuntimeEventType.RewardGranted or GameRuntimeEventType.QuestRewardGranted)
               && string.Equals(before,
                   GeneratedEncounterCombatCanonical.Hash(flee.Session.GameplayState.Inventories),
                   StringComparison.Ordinal);
    }

    private VictoryRouteResult ExecuteVictory(
        GamePackageDefinition package,
        string encounterId,
        IUnifiedGameRuntimeService runtime,
        bool completeQuest,
        IReadOnlyList<string> preparationEncounterIds,
        GeneratedEncounterCombatContract contract,
        string? questId)
    {
        var session = Start(package, runtime);
        if (session is null) return VictoryRouteResult.Failed("generated_combat.start_failed");
        var actionKinds = new List<string>();
        var frames = new List<GeneratedEncounterCombatRuntimeFrame>();
        if (completeQuest && !string.IsNullOrWhiteSpace(questId)
                          && !QuestActive(session, questId))
        {
            var activated = ActivateAssignedQuestThroughDialogue(
                package, runtime, session, questId);
            if (activated is null)
                return VictoryRouteResult.Failed(
                    "generated_combat.generated_quest_start_failed");
            session = activated.Value.Session;
            actionKinds.Add(nameof(GameRuntimeCommandType.OpenDialogue));
            AddFrame(nameof(GameRuntimeCommandType.OpenDialogue),
                activated.Value.OpenedSession);
            actionKinds.Add(
                nameof(GameRuntimeCommandType.ChooseDialogueOption));
            AddFrame(nameof(GameRuntimeCommandType.ChooseDialogueOption),
                session);
        }
        var rewardsBefore = RewardState(session);
        var rewardEvent = false;
        EncounterRuntimeState? encounter = null;
        foreach (var routeEncounterId in preparationEncounterIds.Append(encounterId))
        {
            var started = runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.StartEncounter(routeEncounterId));
            if (!started.Success) return VictoryRouteResult.Failed("generated_combat.start_failed");
            session = started.Session;
            actionKinds.Add(nameof(GameRuntimeCommandType.StartEncounter));
            AddFrame(nameof(GameRuntimeCommandType.StartEncounter), session);
            encounter = session.GameplayState.ActiveEncounter!;
            var bound = DynamicCommandBound(package, routeEncounterId, encounter);
            for (var index = 0; index < bound
                 && session.GameplayState.ActiveEncounter is { Active: true }; index++)
            {
                encounter = session.GameplayState.ActiveEncounter!;
                var participant = Current(encounter);
                UnifiedRuntimeResult result;
                if (IsPlayer(participant.Team))
                {
                    var target = encounter.Participants.FirstOrDefault(item => item.Alive && !IsPlayer(item.Team));
                    if (target is null) break;
                    var playerAction = ExecuteVictoryPlayerAction(package, runtime, session,
                        routeEncounterId, participant.Id, target.Id, contract);
                    if (playerAction is null)
                        return VictoryRouteResult.Failed("generated_combat.victory_command_failed");
                    result = playerAction.Value.Result;
                    actionKinds.Add(playerAction.Value.ActionKind);
                    AddFrame(playerAction.Value.ActionKind, result.Session);
                }
                else
                {
                    result = runtime.ExecuteGameplayCommand(package, session,
                        new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
                    actionKinds.Add(nameof(GameRuntimeCommandType.RunCurrentTurnAi));
                    AddFrame(nameof(GameRuntimeCommandType.RunCurrentTurnAi), result.Session);
                }
                if (!result.Success) return VictoryRouteResult.Failed("generated_combat.victory_command_failed");
                rewardEvent |= result.GameplayEvents.Any(item => item.Type is GameRuntimeEventType.RewardGranted
                    or GameRuntimeEventType.QuestRewardGranted);
                session = result.Session;
            }
            encounter = session.GameplayState.ActiveEncounter!;
            var routeVictory = !encounter.Active
                               && encounter.Participants.Any(item => item.Alive && IsPlayer(item.Team))
                               && encounter.Participants.Where(item => !IsPlayer(item.Team)).All(item => !item.Alive)
                               && !encounter.ActionHistory.Any(item => string.Equals(
                                   item, "flee", StringComparison.OrdinalIgnoreCase));
            if (!routeVictory) return VictoryRouteResult.Failed("generated_combat.victory_failed");
        }
        encounter = session.GameplayState.ActiveEncounter!;
        var victory = !encounter.Active
                      && encounter.Participants.Any(item => item.Alive && IsPlayer(item.Team))
                      && encounter.Participants.Where(item => !IsPlayer(item.Team)).All(item => !item.Alive)
                      && !encounter.ActionHistory.Any(item => string.Equals(item, "flee", StringComparison.OrdinalIgnoreCase));
        var reward = rewardEvent || !string.Equals(rewardsBefore, RewardState(session), StringComparison.Ordinal);
        var readinessCandidates = _questReadiness.EvaluateAll(package, session)
            .Where(item => item.Generated && item.Objectives.Any(objective =>
                string.Equals(objective.Kind, "complete_encounter", StringComparison.OrdinalIgnoreCase)
                && string.Equals(objective.TargetId, encounterId, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(item => item.QuestId, StringComparer.Ordinal).ToList();
        var readiness = readinessCandidates.FirstOrDefault();
        var victoryDiagnostics = new List<string>();
        var ready = readiness is { Ready: true, Active: true, Completed: false };
        if (completeQuest && readiness is null)
            victoryDiagnostics.Add("generated_combat.generated_quest_mapping_missing");
        else if (completeQuest && !ready)
        {
            victoryDiagnostics.Add("generated_combat.generated_quest_state_not_ready");
            victoryDiagnostics.AddRange(readiness!.Objectives.Where(item => !item.Satisfied && !item.Optional)
                .Select(item => "generated_combat.objective_not_ready:"
                                + item.Kind + ":" + item.TargetId + ":"
                                + item.CurrentAmount.ToString(CultureInfo.InvariantCulture) + "/"
                                + item.RequiredAmount.ToString(CultureInfo.InvariantCulture)));
            victoryDiagnostics.AddRange(readiness.Diagnostics);
        }
        var turnIn = false;
        var consequence = false;
        var completeCount = 0;
        if (completeQuest && ready && readiness is not null)
        {
            var before = GeneratedEncounterCombatCanonical.Clone(session);
            var readinessBefore = _questReadiness.EvaluateAll(package, before);
            var completed = runtime.ExecuteGameplayCommand(package, session,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.CompleteQuest, Id = readiness.QuestId });
            completeCount++;
            actionKinds.Add(nameof(GameRuntimeCommandType.CompleteQuest));
            AddFrame(nameof(GameRuntimeCommandType.CompleteQuest), completed.Session);
            if (completed.Success)
            {
                session = completed.Session;
                var readinessAfter = _questReadiness.EvaluateAll(package, session);
                var outcome = _consequences.ProjectAction(package, before, session,
                    completed.MapEvents, completed.GameplayEvents,
                    new GeneratedCampaignAction
                    {
                        Kind = GeneratedCampaignActionKind.CompleteQuest,
                        Title = "Завершить задание"
                    }, readinessBefore, readinessAfter, true, []);
                turnIn = session.GameplayState.Quests.Any(item =>
                    string.Equals(item.QuestId, readiness.QuestId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(item.State, "completed", StringComparison.OrdinalIgnoreCase));
                consequence = outcome.Consequences.Any(item => item.Kind is
                    GeneratedCampaignConsequenceKind.QuestCompleted or GeneratedCampaignConsequenceKind.Reputation);
            }
        }
        return new VictoryRouteResult
        {
            VictoryPassed = victory,
            RewardPassed = reward,
            GeneratedQuestReadyPassed = ready,
            ManualTurnInPassed = completeQuest ? turnIn : ready,
            CompleteQuestCommandCount = completeCount,
            ConsequencePassed = completeQuest ? consequence : true,
            FinalStateHash = GeneratedCampaignConsequenceProjector.HashSession(session),
            ActionKinds = actionKinds,
            Frames = frames,
            Diagnostics = victoryDiagnostics
        };

        void AddFrame(string kind, UnifiedRuntimeSession value) => frames.Add(
            new GeneratedEncounterCombatRuntimeFrame
            {
                Index = frames.Count,
                ActionKind = kind,
                StateHash = GeneratedCampaignConsequenceProjector.HashSession(value)
            });
    }

    private static (UnifiedRuntimeSession OpenedSession,
        UnifiedRuntimeSession Session)?
        ActivateAssignedQuestThroughDialogue(
            GamePackageDefinition package,
            IUnifiedGameRuntimeService runtime,
            UnifiedRuntimeSession session,
            string questId)
    {
        var candidates = package.Game.Dialogues.SelectMany(dialogue =>
                dialogue.Nodes.SelectMany(node => node.Choices)
                    .Where(choice => string.Equals(choice.StartQuestId,
                        questId, StringComparison.Ordinal)
                                     && choice.Metadata.GetValueOrDefault(
                                         "generatedChoiceKind") == "SUPPORT"
                                     && choice.Metadata.GetValueOrDefault(
                                         "generatedChoicePhase") == "initial")
                    .Select(choice => new
                    {
                        DialogueId = dialogue.Id,
                        ChoiceId = choice.Id
                    }))
            .OrderBy(item => item.DialogueId, StringComparer.Ordinal)
            .ThenBy(item => item.ChoiceId, StringComparer.Ordinal)
            .ToList();
        foreach (var candidate in candidates)
        {
            var opened = runtime.ExecuteGameplayCommand(package, session,
                GameRuntimeCommand.OpenDialogue(candidate.DialogueId));
            if (!opened.Success) continue;
            var chosen = runtime.ExecuteGameplayCommand(package,
                opened.Session,
                GameRuntimeCommand.ChooseDialogueOption(
                    candidate.ChoiceId));
            if (chosen.Success && QuestActive(chosen.Session, questId))
                return (opened.Session, chosen.Session);
        }
        return null;
    }

    private static bool QuestActive(UnifiedRuntimeSession session,
        string questId) =>
        session.GameplayState.Quests.Any(item =>
            string.Equals(item.QuestId, questId,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(item.State, "active",
                StringComparison.OrdinalIgnoreCase))
        || session.GameplayState.QuestStates.TryGetValue(questId,
            out var state)
        && string.Equals(state, "active",
            StringComparison.OrdinalIgnoreCase);

    private static (UnifiedRuntimeResult Result, string ActionKind)? ExecuteVictoryPlayerAction(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session,
        string encounterId,
        string participantId,
        string targetId,
        GeneratedEncounterCombatContract contract)
    {
        var ordered = contract.QualifiedActions
            .OrderBy(item => item.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK ? 0 : 1)
            .ThenBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256, StringComparer.Ordinal)
            .ToList();
        if (contract.RouteMode == GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY)
            ordered = ordered.Where(item => item.ActionKind
                == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY).ToList();
        foreach (var descriptor in ordered)
        {
            if (!DescriptorDefinitionCurrent(package, descriptor)) continue;
            var before = GeneratedEncounterCombatCanonical.Clone(session);
            var command = descriptor.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                ? GameRuntimeCommand.BasicAttack(participantId, targetId)
                : GameRuntimeCommand.UseAbility(descriptor.AbilityId, participantId, targetId);
            var result = runtime.ExecuteGameplayCommand(package, session, command);
            if (!result.Success || !GeneratedEncounterCombatContractService.TryObserveSupportedEffect(
                    package, before, result.Session, targetId,
                    descriptor.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY,
                    out var observed)
                || !GeneratedEncounterCombatContractService.MatchesObservedEffect(descriptor, observed)) continue;
            return (result, descriptor.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
                ? nameof(GameRuntimeCommandType.BasicAttack) : nameof(GameRuntimeCommandType.UseAbility));
        }
        return null;
    }

    private static bool DescriptorDefinitionCurrent(
        GamePackageDefinition package,
        GeneratedEncounterCombatQualifiedAction descriptor) =>
        descriptor.RuntimeQualificationPassed
        && (descriptor.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK
            || (package.Game.Abilities.SingleOrDefault(item => string.Equals(item.Id, descriptor.AbilityId,
                    StringComparison.Ordinal)) is { } ability
                && string.Equals(GeneratedEncounterCombatCanonical.Hash(ability), descriptor.AbilityDefinitionSha256,
                    StringComparison.Ordinal)));

    private static UnifiedRuntimeSession? Start(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        string? encounterId = null)
    {
        var started = runtime.Start(package);
        if (!started.Success) return null;
        var session = started.Session;
        foreach (var quest in package.Game.Quests.Where(item => item.AutoStart)
                     .OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            if (session.GameplayState.Quests.Any(item =>
                    string.Equals(item.QuestId, quest.Id, StringComparison.OrdinalIgnoreCase))) continue;
            var questStart = runtime.ExecuteGameplayCommand(package, session, GameRuntimeCommand.StartQuest(quest.Id));
            if (!questStart.Success) return null;
            session = questStart.Session;
        }
        if (string.IsNullOrWhiteSpace(encounterId)) return session;
        var encounter = runtime.ExecuteGameplayCommand(package, session,
            GameRuntimeCommand.StartEncounter(encounterId));
        return encounter.Success ? encounter.Session : null;
    }

    private static UnifiedRuntimeSession? AdvanceToPlayer(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session,
        out bool aiPassed)
    {
        aiPassed = false;
        var current = session;
        var encounter = current.GameplayState.ActiveEncounter!;
        var limit = Math.Max(1, encounter.Participants.Count * 4);
        for (var index = 0; index < limit; index++)
        {
            encounter = current.GameplayState.ActiveEncounter!;
            if (!encounter.Active) return null;
            var participant = Current(encounter);
            if (IsPlayer(participant.Team)) return current;
            var result = runtime.ExecuteGameplayCommand(package, current,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            if (!result.Success) return null;
            aiPassed = true;
            current = result.Session;
        }
        return null;
    }

    private static bool RunOpponentAi(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session)
    {
        var current = session;
        var before = PlayerState(current);
        var succeeded = false;
        var encounter = current.GameplayState.ActiveEncounter!;
        var limit = Math.Max(1, encounter.Participants.Count * 4);
        for (var index = 0; index < limit && current.GameplayState.ActiveEncounter is { Active: true }; index++)
        {
            encounter = current.GameplayState.ActiveEncounter!;
            var participant = Current(encounter);
            if (IsPlayer(participant.Team)) break;
            var result = runtime.ExecuteGameplayCommand(package, current,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            if (!result.Success) return false;
            succeeded = true;
            current = result.Session;
        }
        var returned = current.GameplayState.ActiveEncounter is not { Active: true }
                       || IsPlayer(Current(current.GameplayState.ActiveEncounter!).Team);
        return succeeded && returned && !string.Equals(before, PlayerState(current), StringComparison.Ordinal);
    }

    private static int DynamicCommandBound(
        GamePackageDefinition package,
        string encounterId,
        EncounterRuntimeState encounter)
    {
        var definition = package.Game.Encounters.Single(item => item.Id == encounterId);
        var health = encounter.Participants.SelectMany(item => item.Resources)
            .Where(item => package.Game.Resources.Any(resource => resource.Id == item.ResourceId
                                                                  && GeneratedEncounterCombatContractService.IsRuntimeHealth(resource)))
            .Sum(item => Math.Max(0, item.Amount));
        var damage = definition.Participants.SelectMany(item => item.Abilities)
            .Select(id => package.Game.Abilities.Single(definition => definition.Id == id))
            .SelectMany(ability => ability.Effects.Count == 0
                ? new double[] { ability.Power.GetValueOrDefault(1) }
                : ability.Effects.Where(effect => effect.Type.Contains("damage", StringComparison.OrdinalIgnoreCase))
                    .Select(EffectDamage))
            .Where(value => value > 0).DefaultIfEmpty(1).Min();
        return Math.Max(encounter.Participants.Count,
            (int)Math.Ceiling(health / damage + encounter.Participants.Count) * encounter.Participants.Count);
    }

    private static double EffectDamage(LLMGameCreator.Domain.Definitions.EffectDefinition effect) =>
        effect.Args.TryGetValue("amount", out var raw)
        && double.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? Math.Abs(amount)
            : 0;

    private static RepresentativeCampaign? SelectRepresentative(
        GamePackageDefinition package,
        SeededGeneratedProjectSourceValidationResult source,
        IReadOnlyList<GeneratedEncounterCombatBinding> bindings)
    {
        if (source.RegeneratedPlan is null || source.GeneratedMvpPackage is null)
            return null;
        var plan = source.RegeneratedPlan;
        var startRegion = plan.World.Regions.FirstOrDefault(region =>
            source.GeneratedMvpPackage.GeneratedContent.Regions.Any(row =>
                row.SceneIds.Contains(source.Source?.GeneratedStartMapId ?? string.Empty, StringComparer.Ordinal)
                && string.Equals(row.SourceId, CanonicalGeneratedSourceId(region.RegionId), StringComparison.Ordinal)))?.RegionId;
        var distances = RegionDistances(plan, startRegion);
        var generatedEncounterIds = bindings.Select(item => item.PackageEncounterId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var candidates = bindings.SelectMany(binding => package.Game.Quests
                .Where(quest => GeneratedQuest(package, quest.Id)
                                && quest.Objectives.Any(objective =>
                                    string.Equals(objective.Kind, "complete_encounter",
                                        StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(objective.TargetId, binding.PackageEncounterId,
                                        StringComparison.OrdinalIgnoreCase)))
                .Select(quest => new
                {
                    Binding = binding,
                    Quest = quest,
                    Preparation = PreparationEncounterIds(
                        package, quest, binding.PackageEncounterId, generatedEncounterIds),
                    QuestSourceId = package.GeneratedContent.Quests.Single(item =>
                        string.Equals(item.PackageQuestId, quest.Id, StringComparison.OrdinalIgnoreCase)).SourceId
                }).Where(candidate => candidate.Preparation is not null))
            .Where(candidate => CampaignRouteExists(
                plan,
                startRegion,
                candidate.Preparation!
                    .OrderBy(id => distances.GetValueOrDefault(RegionForEncounter(plan, bindings, id), int.MaxValue))
                    .ThenBy(id => id, StringComparer.Ordinal)
                    .Select(id => RegionForEncounter(plan, bindings, id))
                    .Append(RegionForEncounter(plan, bindings, candidate.Binding.PackageEncounterId))))
            .OrderByDescending(candidate => candidate.Quest.AutoStart)
            .ThenBy(candidate => distances.GetValueOrDefault(plan.EncounterSeeds.Single(seed =>
                seed.EncounterSeedId == candidate.Binding.EncounterSeedId).RegionId, int.MaxValue))
            .ThenBy(candidate => candidate.QuestSourceId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Binding.GeneratedContentSourceId, StringComparer.Ordinal)
            .ToList();
        var selected = candidates.FirstOrDefault();
        return selected is null ? null : new RepresentativeCampaign(
            selected.Binding, selected.Quest.Id, selected.Preparation!
                .OrderBy(id => distances.GetValueOrDefault(RegionForEncounter(plan, bindings, id), int.MaxValue))
                .ThenBy(id => id, StringComparer.Ordinal).ToList());
    }

    private static bool GeneratedQuest(GamePackageDefinition package, string questId) =>
        package.GeneratedContent.Quests.Count(item =>
            string.Equals(item.PackageQuestId, questId, StringComparison.OrdinalIgnoreCase)) == 1;

    private static IReadOnlyList<string>? PreparationEncounterIds(
        GamePackageDefinition package,
        QuestDefinition quest,
        string encounterId,
        IReadOnlySet<string> generatedEncounterIds)
    {
        var encounter = package.Game.Encounters.Single(item => item.Id == encounterId);
        var preparation = new List<string>();
        foreach (var objective in quest.Objectives.Where(item => !item.Optional))
        {
            var required = objective.RequiredAmount <= 0 ? 1 : objective.RequiredAmount;
            if (string.Equals(objective.Kind, "complete_encounter", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(objective.TargetId, encounterId, StringComparison.OrdinalIgnoreCase))
                    return null;
                continue;
            }
            if (!string.Equals(objective.Kind, "has_item", StringComparison.OrdinalIgnoreCase)) return null;
            var initial = package.Game.Inventories.Where(item =>
                    string.Equals(item.OwnerKind, "player", StringComparison.OrdinalIgnoreCase))
                .SelectMany(item => item.Stacks)
                .Where(item => string.Equals(item.ItemId, objective.TargetId, StringComparison.OrdinalIgnoreCase))
                .Sum(item => item.Amount);
            var available = initial + encounter.Rewards.Where(item =>
                    item.Kind is "item" or "add_item"
                    && string.Equals(item.Id, objective.TargetId, StringComparison.OrdinalIgnoreCase))
                .Sum(item => item.Amount);
            foreach (var provider in package.Game.Encounters
                         .Where(item => generatedEncounterIds.Contains(item.Id)
                                        && !string.Equals(item.Id, encounterId,
                                            StringComparison.OrdinalIgnoreCase))
                         .Select(item => new
                         {
                             Encounter = item,
                             Amount = item.Rewards.Where(reward => reward.Kind is "item" or "add_item"
                                                                  && string.Equals(reward.Id,
                                                                      objective.TargetId,
                                                                      StringComparison.OrdinalIgnoreCase))
                                 .Sum(reward => reward.Amount)
                         })
                         .Where(item => item.Amount > 0)
                         .OrderBy(item => item.Encounter.Id, StringComparer.Ordinal))
            {
                if (available >= required) break;
                if (!preparation.Contains(provider.Encounter.Id, StringComparer.OrdinalIgnoreCase))
                    preparation.Add(provider.Encounter.Id);
                available += provider.Amount;
            }
            if (available < required) return null;
        }
        return preparation;
    }

    private static IReadOnlyDictionary<string, int> RegionDistances(
        ProceduralGeneratedGamePlan plan,
        string? startRegion)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(startRegion)) return result;
        var queue = new Queue<string>();
        result[startRegion] = 0;
        queue.Enqueue(startRegion);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in plan.World.Connections.Where(item => item.FromRegionId == current)
                         .Select(item => item.ToRegionId).OrderBy(value => value, StringComparer.Ordinal))
            {
                if (!result.TryAdd(next, result[current] + 1)) continue;
                queue.Enqueue(next);
            }
        }
        return result;
    }

    private static string RegionForEncounter(
        ProceduralGeneratedGamePlan plan,
        IReadOnlyList<GeneratedEncounterCombatBinding> bindings,
        string encounterId) => plan.EncounterSeeds.Single(seed => seed.EncounterSeedId == bindings.Single(binding =>
            string.Equals(binding.PackageEncounterId, encounterId, StringComparison.OrdinalIgnoreCase)).EncounterSeedId)
        .RegionId;

    private static bool CampaignRouteExists(
        ProceduralGeneratedGamePlan plan,
        string? startRegion,
        IEnumerable<string> orderedRegions)
    {
        if (string.IsNullOrWhiteSpace(startRegion)) return false;
        var current = startRegion;
        foreach (var target in orderedRegions)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal) { current };
            var queue = new Queue<string>();
            queue.Enqueue(current);
            while (queue.Count > 0 && !visited.Contains(target))
            {
                var region = queue.Dequeue();
                foreach (var next in plan.World.Connections.Where(item => item.FromRegionId == region)
                             .Select(item => item.ToRegionId).OrderBy(value => value, StringComparer.Ordinal))
                    if (visited.Add(next)) queue.Enqueue(next);
            }
            if (!visited.Contains(target)) return false;
            current = target;
        }
        return true;
    }

    private static string CanonicalGeneratedSourceId(string sourceId) =>
        sourceId.StartsWith("generated/", StringComparison.Ordinal)
        || sourceId.StartsWith("seeded_generated_project/", StringComparison.Ordinal)
            ? sourceId
            : "generated/" + sourceId;

    private static EncounterParticipantState Current(EncounterRuntimeState encounter) =>
        encounter.Participants[Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1)];

    private static string RewardState(UnifiedRuntimeSession session) => GeneratedEncounterCombatCanonical.Serialize(new
    {
        Inventories = session.GameplayState.Inventories,
        Resources = session.GameplayState.Resources,
        Progressions = session.GameplayState.Progressions
    });

    private static string PlayerState(UnifiedRuntimeSession session) => GeneratedEncounterCombatCanonical.Serialize(
        session.GameplayState.ActiveEncounter?.Participants.Where(item => IsPlayer(item.Team))
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList() ?? []);

    private static IReadOnlyList<GeneratedEncounterCombatHumanFact> HumanFacts(
        bool passed,
        EncounterRouteResult route) =>
    [
        new() { Label = "Боевая готовность", Value = passed ? "подтверждена" : "не подтверждена" },
        new() { Label = "Действия игрока", Value = PlayerRouteFact(route) },
        new() { Label = "Противники", Value = route.OpponentAiPassed ? "ходы выполнены игровым Runtime" : "ходы не подтверждены" },
        new() { Label = "Победа и награда", Value = route.VictoryPassed && route.RewardPassed ? "получены" : "не подтверждены" },
        new() { Label = "Завершение задания", Value = route.ManualTurnInPassed ? "выполнено вручную" : "не подтверждено" }
    ];

    private static string PlayerRouteFact(EncounterRouteResult route) =>
        (route.BasicAttackAvailable, route.PackageAbilityAvailable) switch
        {
            (true, false) => "Обычная атака",
            (false, true) => "Способность",
            (true, true) => "Обычная атака и способность",
            _ => "не подтверждены"
        };

    private static GameProjectGeneratedEncounterCombatSummary Invalid(
        GeneratedEncounterCombatContract contract,
        GeneratedWorldEncounterCombatOverlayDocument overlay,
        IReadOnlyList<string> diagnostics) => new()
    {
        Present = true,
        Status = "INVALID",
        ContractId = contract.ContractId,
        ContractSourcePackageSha256 = contract.SourcePackageSha256,
        ExactPackageSha256 = overlay.OutputPackageSha256,
        Overlay = overlay,
        Diagnostics = diagnostics
    };

    private static bool IsPlayer(string? team) =>
        string.Equals(team, "player", StringComparison.OrdinalIgnoreCase);

    private sealed record EncounterRouteResult
    {
        public bool Passed { get; init; }
        public bool BasicAttackAvailable { get; init; }
        public bool BasicAttackPassed { get; init; }
        public bool PackageAbilityAvailable { get; init; }
        public bool PackageAbilityPassed { get; init; }
        public bool PlayerRoutePassed { get; init; }
        public bool OpponentAiPassed { get; init; }
        public bool FleePassed { get; init; }
        public bool VictoryPassed { get; init; }
        public bool RewardPassed { get; init; }
        public bool GeneratedQuestReadyPassed { get; init; }
        public bool ManualTurnInPassed { get; init; }
        public int CompleteQuestCommandCount { get; init; }
        public int AdvanceObjectiveCommandCount { get; init; }
        public bool ConsequencePassed { get; init; }
        public bool ExactPackageReferencePassed { get; init; }
        public string FinalStateHash { get; init; } = string.Empty;
        public IReadOnlyList<string> ActionKinds { get; init; } = [];
        public IReadOnlyList<GeneratedEncounterCombatRuntimeFrame> Frames { get; init; } = [];
        public IReadOnlyList<string> Diagnostics { get; init; } = [];
    }

    private sealed record RepresentativeCampaign(
        GeneratedEncounterCombatBinding Binding,
        string QuestId,
        IReadOnlyList<string> PreparationEncounterIds);

    private sealed record VictoryRouteResult
    {
        public bool VictoryPassed { get; init; }
        public bool RewardPassed { get; init; }
        public bool GeneratedQuestReadyPassed { get; init; }
        public bool ManualTurnInPassed { get; init; }
        public int CompleteQuestCommandCount { get; init; }
        public bool ConsequencePassed { get; init; }
        public string FinalStateHash { get; init; } = string.Empty;
        public IReadOnlyList<string> ActionKinds { get; init; } = [];
        public IReadOnlyList<GeneratedEncounterCombatRuntimeFrame> Frames { get; init; } = [];
        public IReadOnlyList<string> Diagnostics { get; init; } = [];

        public static VictoryRouteResult Failed(string diagnostic) => new() { Diagnostics = [diagnostic] };
    }
}
