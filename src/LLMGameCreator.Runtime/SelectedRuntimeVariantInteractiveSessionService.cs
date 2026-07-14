using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class SelectedRuntimeVariantInteractiveSessionService :
    ISelectedRuntimeVariantInteractiveSessionService
{
    private const string RuntimeRoute = "runtime_session";
    private const string PresentationRoute = "presentation_only";
    private readonly ICanonicalRuntimePlayerCommandLoopService _commandLoop;
    private readonly IRequirementEvaluator _requirementEvaluator;

    public SelectedRuntimeVariantInteractiveSessionService(
        ICanonicalRuntimePlayerCommandLoopService commandLoop,
        IRequirementEvaluator? requirementEvaluator = null)
    {
        _commandLoop = commandLoop ?? throw new ArgumentNullException(nameof(commandLoop));
        _requirementEvaluator = requirementEvaluator ?? new RequirementEvaluator();
    }

    public static SelectedRuntimeVariantInteractiveSessionService CreateDefault() =>
        new(CanonicalRuntimePlayerCommandLoopService.CreateDefault());

    public SelectedRuntimeVariantInteractiveSession StartSession(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.SessionId)
            || string.IsNullOrWhiteSpace(request.CandidateId)
            || string.IsNullOrWhiteSpace(request.VariantKind)
            || string.IsNullOrWhiteSpace(request.PackageSha256))
        {
            throw new InvalidOperationException("Goal144 session identity and package hash are required.");
        }

        var canonical = _commandLoop.BeginSession(package, new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath,
            CapabilityPlan = request.CapabilityPlan
        });
        var session = new SelectedRuntimeVariantInteractiveSession
        {
            SessionId = request.SessionId,
            CandidateId = request.CandidateId,
            VariantKind = request.VariantKind,
            PackagePath = request.PackagePath,
            PackageSha256 = request.PackageSha256,
            CapabilityPlan = request.CapabilityPlan,
            CanonicalSession = canonical,
            CurrentStateHash = canonical.CurrentStateHash
        };
        Refresh(session, package);
        return session;
    }

    public SelectedRuntimeVariantInteractiveActionResult ExecuteAction(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        SelectedRuntimeVariantInteractiveActionRequest request)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));
        if (session is null) throw new ArgumentNullException(nameof(session));
        if (request is null) throw new ArgumentNullException(nameof(request));
        var before = session.CurrentStateHash;
        if (request.SessionId != session.SessionId
            || request.ActionIndex != session.CurrentActionIndex
            || string.IsNullOrWhiteSpace(request.ActionRequestId))
        {
            return Rejected(request, before, "goal144.action_correlation_rejected");
        }

        var descriptor = session.AvailableActions.FirstOrDefault(action =>
            action.ActionId == request.ActionId);
        if (descriptor is null)
        {
            return Rejected(request, before, "goal144.action_unknown");
        }

        if (!descriptor.Available)
        {
            return Rejected(
                request,
                before,
                "goal144.action_unavailable:" + descriptor.UnavailableReason,
                descriptor);
        }

        var plannedAction = session.CapabilityPlan?.OrderedActions.SingleOrDefault(action =>
            action.ActionId == descriptor.ActionId);
        var conditional = plannedAction is null
            ? ConditionalActionDecision.Execute()
            : EvaluateConditionalAction(package, session, plannedAction);
        if (conditional.Kind == ConditionalActionDecisionKind.Failure)
            return Rejected(request, before, "conditional_action_failed:" + conditional.Reason,
                descriptor, conditional.Diagnostics);
        if (conditional.Kind == ConditionalActionDecisionKind.Skip)
            return SkipConditionalAction(package, session, request, descriptor, before,
                conditional.Reason, conditional.Diagnostics);

        var runtimeExecuted = false;
        var runtimeMutation = false;
        var eventCount = 0;
        var diagnostics = new List<string>(conditional.Diagnostics);
        if (descriptor.Route == RuntimeRoute)
        {
            if (!ValidateExecutionBinding(package, session, descriptor))
            {
                return Rejected(
                    request,
                    before,
                    "goal144.action_execution_binding_rejected",
                    descriptor);
            }

            var execution = _commandLoop.ExecuteRange(
                package,
                session.CanonicalSession,
                new CanonicalRuntimePlayerCommandLoopExecutionRequest
                {
                    RequestedOperation = descriptor.ActionId,
                    RuntimeCommandStartIndex = descriptor.RuntimeCommandStartIndex,
                    RuntimeCommandEndIndex = descriptor.RuntimeCommandEndIndex
                });
            diagnostics.AddRange(execution.Diagnostics);
            if (!execution.Success)
            {
                return Rejected(
                    request,
                    before,
                    "goal144.runtime_execution_failed",
                    descriptor,
                    diagnostics);
            }

            runtimeExecuted = execution.RuntimeExecuted;
            runtimeMutation = execution.RuntimeMutation;
            eventCount = execution.EventCount;
            var primaryStep = execution.Steps.SingleOrDefault(step =>
                step.Index == descriptor.CanonicalStepIndex);
            var executionBindingValidated = primaryStep is not null
                                            && primaryStep.StepId == descriptor.CanonicalStepId
                                            && primaryStep.RuntimeCommandKind == descriptor.CommandKind
                                            && primaryStep.TargetId == descriptor.TargetId
                                            && execution.RuntimeCommandStartIndex ==
                                            descriptor.RuntimeCommandStartIndex
                                            && execution.RuntimeCommandEndIndex ==
                                            descriptor.RuntimeCommandEndIndex;
            if (!executionBindingValidated)
            {
                throw new InvalidOperationException(
                    "Goal144 canonical execution result violated its validated action binding.");
            }

            session.RuntimeCommandExecutionCount += execution.ExecutedCommandCount;
            session.RuntimeStarted = session.CanonicalSession.RuntimeStarted;
            if (execution.Snapshots.Count > 0)
            {
                session.LatestSnapshot = execution.Snapshots[^1];
            }
        }
        else
        {
            session.PresentationOnlyActionCount++;
            if (plannedAction?.RuntimePrimitiveId is "runtime.presentation.inspect_faction"
                or "runtime.presentation.inspect_dialogue_choices"
                or "runtime.presentation.inspect_social_summary")
            {
                var snapshot = BuildPresentationSnapshot(package, session, descriptor, plannedAction, before);
                session.CanonicalSession.Snapshots.Add(snapshot);
                session.CanonicalSession.StateHashChain.Add(before);
                session.LatestSnapshot = snapshot;
            }
            if (session.CapabilityPlan is null && descriptor.ActionId == "show_final_state")
            {
                session.CanonicalSession.CurrentCommandIndex++;
                session.Completed = true;
            }
        }

        UpdateSummaries(session);
        session.CurrentStateHash = session.CanonicalSession.CurrentStateHash;
        var result = new SelectedRuntimeVariantInteractiveActionResult
        {
            ActionRequestId = request.ActionRequestId,
            SessionId = request.SessionId,
            ActionIndex = request.ActionIndex,
            ActionId = descriptor.ActionId,
            Category = descriptor.Category,
            Route = descriptor.Route,
            CommandKind = descriptor.CommandKind,
            TargetId = descriptor.TargetId,
            CanonicalStepId = descriptor.CanonicalStepId,
            CanonicalStepIndex = descriptor.CanonicalStepIndex,
            RuntimeCommandStartIndex = descriptor.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = descriptor.RuntimeCommandEndIndex,
            ExecutionTargetId = descriptor.ExecutionTargetId,
            ExecutionBindingValidated = descriptor.ExecutionBindingValidated,
            StateHashBefore = before,
            StateHashAfter = session.CurrentStateHash,
            RuntimeExecuted = runtimeExecuted,
            RuntimeMutation = runtimeMutation,
            RuntimeEventCount = eventCount,
            CorrelationPassed = true,
            Status = "EXECUTED",
            Diagnostics = diagnostics
        };
        session.ActionJournal.Add(ToJournal(result));
        session.CurrentActionIndex++;
        Refresh(session, package);
        return result;
    }

    private ConditionalActionDecision EvaluateConditionalAction(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        CapabilityRuntimePlaythroughAction action)
    {
        if (action.RuntimePrimitiveId == "runtime.command.advance_quest_objective")
        {
            var questId = action.Args.GetValueOrDefault("questId") ?? string.Empty;
            var objectiveId = action.Args.GetValueOrDefault("objectiveId");
            if (string.IsNullOrWhiteSpace(objectiveId)) objectiveId = action.ResolvedTargetId;
            var questDefinitions = package.Game.Quests.Where(item => item.Id == questId).ToList();
            var runtimeQuests = session.CanonicalSession.RuntimeSession.GameplayState.Quests
                .Where(item => item.QuestId == questId).ToList();
            var observedState = runtimeQuests.Count == 1 ? runtimeQuests[0].State : "matches=" + runtimeQuests.Count;
            var priorEvents = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
            var completionEventCount = priorEvents.Count(item =>
                item.EventType == "QuestCompleted" && item.TargetId == questId);
            var rewardEventCount = priorEvents.Count(item =>
                item.EventType == "QuestRewardGranted" && item.TargetId == questId);
            var context = new List<string>
            {
                "actionId=" + action.ActionId,
                "questId=" + questId,
                "objectiveId=" + objectiveId,
                "observedQuestState=" + observedState,
                "priorCompletionEventCount=" + completionEventCount,
                "priorRewardEventCount=" + rewardEventCount
            };
            if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(objectiveId)
                || questDefinitions.Count != 1 || runtimeQuests.Count != 1)
                return ConditionalActionDecision.Failure("quest_advance_state_invalid", context);
            var objectiveDefinitions = questDefinitions[0].Objectives
                .Where(item => item.Id == objectiveId).ToList();
            var runtimeObjectives = runtimeQuests[0].Objectives
                .Where(item => item.ObjectiveId == objectiveId).ToList();
            if (objectiveDefinitions.Count != 1 || runtimeObjectives.Count != 1)
                return ConditionalActionDecision.Failure("quest_advance_state_invalid", context);

            var runtimeQuest = runtimeQuests[0];
            var runtimeObjective = runtimeObjectives[0];
            if (string.Equals(runtimeQuest.State, "active", StringComparison.Ordinal)
                && !runtimeObjective.Completed)
                return ConditionalActionDecision.Execute(["questCompletionPath=explicit_advance"]);

            if (string.Equals(runtimeQuest.State, "completed", StringComparison.Ordinal)
                && runtimeObjective.Completed
                && completionEventCount == 1
                && rewardEventCount == 1)
            {
                var completionSnapshots = session.CanonicalSession.Snapshots.Where(snapshot =>
                    snapshot.RuntimeEvents.Any(item => item.EventType == "QuestCompleted" && item.TargetId == questId)
                    && snapshot.RuntimeEvents.Any(item => item.EventType == "QuestRewardGranted" && item.TargetId == questId))
                    .ToList();
                var completedDuringAction = completionSnapshots.Count == 1
                    ? session.CapabilityPlan?.OrderedActions.SingleOrDefault(item =>
                        "capability." + item.ActionId == completionSnapshots[0].StepId)?.ActionId
                    : null;
                if (!string.IsNullOrWhiteSpace(completedDuringAction))
                    return ConditionalActionDecision.Skip(
                        "quest_already_completed",
                        [
                            "questCompletionPath=already_completed",
                            "questAlreadyCompletedBeforeAdvance=true",
                            "completedDuringAction=" + completedDuringAction,
                            "redundantAdvanceSkipped=true",
                            .. context
                        ]);
            }

            return ConditionalActionDecision.Failure("quest_advance_state_invalid", context);
        }

        if (!action.Args.TryGetValue("executionPredicates", out var raw) || string.IsNullOrWhiteSpace(raw))
            return ConditionalActionDecision.Execute();
        var predicates = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim()).ToList();
        if (predicates.Contains("dialogue_choice_available", StringComparer.Ordinal))
        {
            var dialogueId = action.Args.GetValueOrDefault("dialogueId") ?? string.Empty;
            var nodeId = action.Args.GetValueOrDefault("nodeId") ?? string.Empty;
            var choiceId = action.Args.GetValueOrDefault("choiceId") ?? string.Empty;
            var dialogues = package.Game.Dialogues.Where(item => item.Id == dialogueId).ToList();
            if (dialogues.Count != 1)
                return ConditionalActionDecision.Failure("dialogue_choice.dialogue_matches=" + dialogues.Count);
            var nodes = dialogues[0].Nodes.Where(item => item.Id == nodeId).ToList();
            if (nodes.Count != 1)
                return ConditionalActionDecision.Failure("dialogue_choice.node_matches=" + nodes.Count);
            var choices = nodes[0].Choices.Where(item => item.Id == choiceId).ToList();
            if (choices.Count != 1)
                return ConditionalActionDecision.Failure("dialogue_choice.choice_matches=" + choices.Count);
            if (action.Args.GetValueOrDefault("unavailableOutcome") != "still_locked")
                return ConditionalActionDecision.Failure("dialogue_choice.unavailable_outcome_invalid");
            var choice = choices[0];
            var requirements = _requirementEvaluator.Evaluate(package,
                session.CanonicalSession.RuntimeSession.GameplayState,
                choice.Conditions.Select(RuntimeEffectMapper.ToRequirement).Concat(choice.Requirements),
                action.Args.GetValueOrDefault("inventoryId"));
            if (requirements.Diagnostics.Any(diagnostic =>
                    diagnostic.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
                return ConditionalActionDecision.Failure("dialogue_choice.requirement_evaluation_failed",
                    requirements.Diagnostics.Select(item => item.Code + ":" + item.Message).ToList());
            if (!requirements.Success)
            {
                var codes = requirements.Failures.Select(item => item.Code).Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal).ToList();
                return ConditionalActionDecision.Skip(
                    "socialOutcome=still_locked;failedRequirements=" + string.Join(",", codes),
                    codes.Select(code => "failed_requirement:" + code).Append("socialOutcome=still_locked").ToList());
            }
        }
        if (predicates.Contains("dialogue_open", StringComparer.Ordinal))
        {
            var active = session.CanonicalSession.RuntimeSession.GameplayState.ActiveDialogue;
            if (active is null || !active.Open)
                return ConditionalActionDecision.Skip("dialogue_open=false", ["dialogue_open=false"]);
            var expected = action.Args.GetValueOrDefault("dialogueId");
            if (!string.IsNullOrWhiteSpace(expected) && active.DialogueId != expected)
                return ConditionalActionDecision.Failure("dialogue_open.mismatched=" + active.DialogueId + ";expected=" + expected);
        }
        var state = session.CanonicalSession.RuntimeSession.GameplayState;
        var encounter = state.ActiveEncounter;
        var failed = new List<string>();
        foreach (var predicate in predicates.Where(predicate => predicate is "encounter_active" or "participant_alive" or "status_present"))
        {
            var passed = predicate switch
            {
                "encounter_active" => encounter is not null && encounter.Active
                                      && encounter.EncounterId == action.Args.GetValueOrDefault("encounterId"),
                "participant_alive" => encounter?.Participants.SingleOrDefault(item =>
                    item.Id == action.Args.GetValueOrDefault("predicateParticipantId"))?.Alive == true,
                "status_present" => encounter?.Participants.SingleOrDefault(item =>
                    item.Id == action.Args.GetValueOrDefault("statusTargetParticipantId"))?.Statuses.Any(status =>
                    status.StatusId == action.Args.GetValueOrDefault("statusId")) == true,
                _ => false
            };
            if (!passed) failed.Add(predicate);
        }
        if (failed.Count == 0) return ConditionalActionDecision.Execute();

        var events = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
        var statusTarget = action.Args.GetValueOrDefault("statusTargetParticipantId");
        var terminalOutcome = events.Any(item => item.EventType == "ParticipantDefeated" && item.TargetId == statusTarget)
            ? "target_defeated"
            : events.Any(item => item.EventType == "EncounterWon") ? "encounter_won"
            : events.Any(item => item.EventType == "EncounterLost") ? "encounter_lost"
            : events.Any(item => item.EventType == "EncounterEnded") ? "encounter_ended"
            : events.Any(item => item.EventType == "StatusRemoved"
                                      && item.TargetId == statusTarget
                                      && item.Message.Contains("expired", StringComparison.OrdinalIgnoreCase))
                ? "expired"
                : string.Empty;
        return terminalOutcome.Length == 0
            ? ConditionalActionDecision.Execute()
            : ConditionalActionDecision.Skip(
                "terminal_outcome=" + terminalOutcome + ";predicates=" + string.Join(",", failed));
    }

    private static SelectedRuntimeVariantInteractiveActionResult SkipConditionalAction(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        SelectedRuntimeVariantInteractiveActionRequest request,
        SelectedRuntimeVariantActionDescriptor descriptor,
        string before,
        string reason,
        IReadOnlyList<string>? decisionDiagnostics = null)
    {
        session.CanonicalSession.CurrentCommandIndex = descriptor.RuntimeCommandEndIndex + 1;
        var source = session.LatestSnapshot;
        var snapshot = new CanonicalRuntimePlayerCommandLoopSnapshot
        {
            Status = "SKIPPED",
            StepIndex = descriptor.CanonicalStepIndex,
            StepId = descriptor.CanonicalStepId,
            Category = descriptor.Category,
            CommandLabel = descriptor.ActionId,
            StateHashBefore = before,
            StateHashAfter = before,
            MapSummary = source.MapSummary,
            PlayerX = source.PlayerX,
            PlayerY = source.PlayerY,
            VisibleInteractionSummary = source.VisibleInteractionSummary,
            DialogueSummary = source.DialogueSummary,
            QuestSummary = source.QuestSummary,
            InventorySummary = source.InventorySummary,
            CombatSummary = source.CombatSummary,
            EquipmentSummary = source.EquipmentSummary,
            AttributesSummary = source.AttributesSummary,
            ProgressionSummary = source.ProgressionSummary,
            FactionSummary = source.FactionSummary,
            DialogueChoicesSummary = source.DialogueChoicesSummary,
            ResourceSummary = source.ResourceSummary,
            FlagSummary = source.FlagSummary,
            SocialSummary = source.SocialSummary,
            DiagnosticSummary = "conditional action skipped: " + reason,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            RuntimeEvents = []
        };
        session.CanonicalSession.Snapshots.Add(snapshot);
        session.CanonicalSession.StateHashChain.Add(before);
        session.LatestSnapshot = snapshot;
        UpdateSummaries(session);
        var result = new SelectedRuntimeVariantInteractiveActionResult
        {
            ActionRequestId = request.ActionRequestId,
            SessionId = request.SessionId,
            ActionIndex = request.ActionIndex,
            ActionId = descriptor.ActionId,
            Category = descriptor.Category,
            Route = "conditional_skip",
            CommandKind = reason,
            TargetId = descriptor.TargetId,
            CanonicalStepId = descriptor.CanonicalStepId,
            CanonicalStepIndex = descriptor.CanonicalStepIndex,
            RuntimeCommandStartIndex = descriptor.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = descriptor.RuntimeCommandEndIndex,
            ExecutionTargetId = descriptor.ExecutionTargetId,
            ExecutionBindingValidated = descriptor.ExecutionBindingValidated,
            StateHashBefore = before,
            StateHashAfter = before,
            RuntimeExecuted = false,
            RuntimeMutation = false,
            RuntimeEventCount = 0,
            CorrelationPassed = true,
            Status = "SKIPPED",
            Diagnostics = decisionDiagnostics is null
                ? ["conditional_action_skipped:" + reason]
                : ["conditional_action_skipped:" + reason, .. decisionDiagnostics]
        };
        session.ActionJournal.Add(ToJournal(result));
        session.CurrentActionIndex++;
        Refresh(session, package);
        return result;
    }

    private CanonicalRuntimePlayerCommandLoopSnapshot BuildPresentationSnapshot(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        SelectedRuntimeVariantActionDescriptor descriptor,
        CapabilityRuntimePlaythroughAction? action,
        string stateHash)
    {
        var source = session.LatestSnapshot;
        var state = session.CanonicalSession.RuntimeSession.GameplayState;
        var factionSummary = string.Join("; ", state.Factions.OrderBy(item => item.FactionId, StringComparer.Ordinal)
            .Select(item => item.FactionId + "=" + Format(item.Reputation) + ":" + item.RelationKind));
        var resourceSummary = string.Join("; ", state.Resources.OrderBy(item => item.ResourceId, StringComparer.Ordinal)
            .ThenBy(item => item.Scope, StringComparer.Ordinal)
            .Select(item => item.ResourceId + "@" + item.Scope + "=" + Format(item.Amount)));
        var flagSummary = string.Join("; ", state.Flags.OrderBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => item.Id + "=" + item.Value));
        var choiceSummary = source.DialogueChoicesSummary;
        var socialSummary = source.SocialSummary;
        var diagnostic = "presentation snapshot read from Runtime state";
        if (action?.RuntimePrimitiveId == "runtime.presentation.inspect_dialogue_choices")
        {
            var dialogueId = action.Args.GetValueOrDefault("dialogueId") ?? string.Empty;
            var nodeId = action.Args.GetValueOrDefault("nodeId") ?? string.Empty;
            var dialogue = package.Game.Dialogues.Single(item => item.Id == dialogueId);
            var node = dialogue.Nodes.Single(item => item.Id == nodeId);
            choiceSummary = string.Join("; ", node.Choices.OrderBy(choice => choice.Id, StringComparer.Ordinal).Select(choice =>
            {
                var evaluated = _requirementEvaluator.Evaluate(package, state,
                    choice.Conditions.Select(RuntimeEffectMapper.ToRequirement).Concat(choice.Requirements),
                    action.Args.GetValueOrDefault("inventoryId"));
                var codes = evaluated.Failures.Select(failure => failure.Code)
                    .Concat(evaluated.Diagnostics.Select(item => item.Code)).Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal).ToList();
                return choice.Id + "=" + (evaluated.Success ? "available" : "unavailable")
                       + (codes.Count == 0 ? string.Empty : "[" + string.Join(",", codes) + "]");
            }));
            diagnostic = dialogueId + ":" + nodeId + ":" + choiceSummary;
        }
        else if (action?.RuntimePrimitiveId == "runtime.presentation.inspect_faction")
        {
            var factionId = action.Args.GetValueOrDefault("factionId") ?? action.ResolvedTargetId;
            diagnostic = factionSummary.Split(';').Select(item => item.Trim())
                .Single(item => item.StartsWith(factionId + "=", StringComparison.Ordinal));
        }
        else if (action?.RuntimePrimitiveId == "runtime.presentation.inspect_social_summary")
        {
            var choiceId = action.Args.GetValueOrDefault("choiceId") ?? string.Empty;
            var flagId = action.Args.GetValueOrDefault("flagId") ?? string.Empty;
            var selected = session.CanonicalSession.Snapshots.SelectMany(item => item.RuntimeEvents)
                .Any(item => item.EventType == "DialogueChoiceSelected" && item.TargetId == choiceId);
            var locked = session.ActionJournal.Any(entry => entry.Status == "SKIPPED"
                && entry.Diagnostics.Any(item => item.Contains("socialOutcome=still_locked", StringComparison.Ordinal)));
            var flag = state.Flags.SingleOrDefault(item => item.Id == flagId)?.Value;
            var outcome = selected ? "claimed" : locked ? "still_locked"
                : string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase) ? "already_claimed" : "unknown";
            socialSummary = "socialOutcome=" + outcome + ";" + factionSummary + ";" + resourceSummary + ";" + flagSummary;
            diagnostic = socialSummary;
        }
        return new CanonicalRuntimePlayerCommandLoopSnapshot
        {
            Status = "EXECUTED",
            StepIndex = session.CurrentActionIndex,
            StepId = "presentation." + descriptor.ActionId,
            Category = descriptor.Category,
            CommandLabel = descriptor.ActionId,
            StateHashBefore = stateHash,
            StateHashAfter = stateHash,
            MapSummary = source.MapSummary,
            PlayerX = source.PlayerX,
            PlayerY = source.PlayerY,
            VisibleInteractionSummary = source.VisibleInteractionSummary,
            DialogueSummary = state.ActiveDialogue is null
                ? string.Empty
                : state.ActiveDialogue.DialogueId + ":" + state.ActiveDialogue.CurrentNodeId + ":" + state.ActiveDialogue.Open,
            QuestSummary = string.Join("; ", state.Quests.OrderBy(item => item.QuestId, StringComparer.Ordinal)
                .Select(item => item.QuestId + ":" + item.State + ":" + (item.CurrentStageId ?? string.Empty))),
            InventorySummary = source.InventorySummary,
            CombatSummary = source.CombatSummary,
            EquipmentSummary = source.EquipmentSummary,
            AttributesSummary = source.AttributesSummary,
            ProgressionSummary = source.ProgressionSummary,
            FactionSummary = factionSummary,
            DialogueChoicesSummary = choiceSummary,
            ResourceSummary = resourceSummary,
            FlagSummary = flagSummary,
            SocialSummary = socialSummary,
            DiagnosticSummary = diagnostic,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            RuntimeEvents = []
        };
    }

    public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint(
        SelectedRuntimeVariantInteractiveSession session,
        string checkpointId,
        string createdAtUtc)
    {
        if (session is null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrWhiteSpace(checkpointId) || string.IsNullOrWhiteSpace(createdAtUtc))
        {
            throw new InvalidOperationException("Goal144 checkpoint id and UTC marker are required.");
        }

        return new SelectedRuntimeVariantInteractiveCheckpoint
        {
            CheckpointId = checkpointId,
            SessionId = session.SessionId,
            CandidateId = session.CandidateId,
            VariantKind = session.VariantKind,
            PackageSha256 = session.PackageSha256,
            CapabilityPlanId = session.CapabilityPlan?.PlanId ?? string.Empty,
            CapabilityPlanSignature = session.CapabilityPlan?.ActionPlanSignature ?? string.Empty,
            ActionJournal = session.ActionJournal.Select(Clone).ToList(),
            RuntimeCommandExecutionCount = session.RuntimeCommandExecutionCount,
            ExpectedStateHash = session.CurrentStateHash,
            ExpectedActionIndex = session.CurrentActionIndex,
            MapSummary = session.LatestMapSummary,
            InventorySummary = session.LatestInventorySummary,
            QuestSummary = session.LatestQuestSummary,
            CombatSummary = session.LatestCombatSummary,
            EquipmentSummary = session.LatestEquipmentSummary,
            AttributesSummary = session.LatestAttributesSummary,
            ProgressionSummary = session.LatestProgressionSummary,
            CreatedAtUtc = createdAtUtc
        };
    }

    public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (checkpoint is null) throw new ArgumentNullException(nameof(checkpoint));
        var diagnostics = new List<string>();
        var packageHashValid = request.PackageSha256 == checkpoint.PackageSha256;
        var planValid = string.Equals(request.CapabilityPlan?.PlanId ?? string.Empty,
                            checkpoint.CapabilityPlanId, StringComparison.Ordinal)
                        && string.Equals(request.CapabilityPlan?.ActionPlanSignature ?? string.Empty,
                            checkpoint.CapabilityPlanSignature, StringComparison.Ordinal);
        var candidateValid = request.CandidateId == checkpoint.CandidateId
                             && request.VariantKind == checkpoint.VariantKind
                             && request.SessionId == checkpoint.SessionId
                             && planValid;
        if (!packageHashValid || !candidateValid)
        {
            diagnostics.Add(packageHashValid
                ? planValid ? "goal144.checkpoint_candidate_mismatch" : "goal149.checkpoint_plan_identity_mismatch"
                : "goal144.checkpoint_package_hash_mismatch");
            return FailedReplay(packageHashValid, candidateValid, checkpoint, diagnostics);
        }

        var fresh = StartSession(package, request);
        var correlation = true;
        var continuity = true;
        foreach (var entry in checkpoint.ActionJournal)
        {
            if (entry.ActionIndex != fresh.CurrentActionIndex
                || entry.SessionId != fresh.SessionId
                || string.IsNullOrWhiteSpace(entry.ActionRequestId))
            {
                correlation = false;
                diagnostics.Add("goal144.checkpoint_journal_index_or_identity_tamper");
                break;
            }

            var replay = ExecuteAction(package, fresh, new SelectedRuntimeVariantInteractiveActionRequest
            {
                ActionRequestId = entry.ActionRequestId,
                SessionId = entry.SessionId,
                ActionIndex = entry.ActionIndex,
                ActionId = entry.ActionId
            });
            correlation &= replay.CorrelationPassed
                           && replay.Status == entry.Status
                           && replay.Category == entry.Category
                           && replay.Route == entry.Route
                           && replay.CommandKind == entry.CommandKind
                           && replay.TargetId == entry.TargetId
                           && replay.CanonicalStepId == entry.CanonicalStepId
                           && replay.CanonicalStepIndex == entry.CanonicalStepIndex
                           && replay.RuntimeCommandStartIndex == entry.RuntimeCommandStartIndex
                           && replay.RuntimeCommandEndIndex == entry.RuntimeCommandEndIndex
                           && replay.ExecutionTargetId == entry.ExecutionTargetId
                           && replay.ExecutionBindingValidated
                           && entry.ExecutionBindingValidated;
            continuity &= replay.StateHashBefore == entry.StateHashBefore
                          && replay.StateHashAfter == entry.StateHashAfter;
            if (!correlation || !continuity)
            {
                diagnostics.Add("goal144.checkpoint_journal_tamper_or_hash_discontinuity");
                break;
            }
        }

        var expectedHashMatched = fresh.CurrentStateHash == checkpoint.ExpectedStateHash
                                  && fresh.CurrentActionIndex == checkpoint.ExpectedActionIndex
                                  && fresh.RuntimeCommandExecutionCount ==
                                  checkpoint.RuntimeCommandExecutionCount;
        if (!expectedHashMatched)
        {
            diagnostics.Add("goal144.checkpoint_expected_hash_or_count_mismatch");
        }

        return new SelectedRuntimeVariantInteractiveReplayResult
        {
            Passed = packageHashValid && candidateValid && correlation && continuity
                     && expectedHashMatched,
            PackageHashValidated = packageHashValid,
            CandidateValidated = candidateValid,
            JournalCorrelationPassed = correlation,
            StateHashContinuityPassed = continuity,
            ExpectedStateHashMatched = expectedHashMatched,
            ExpectedStateHash = checkpoint.ExpectedStateHash,
            ActualStateHash = fresh.CurrentStateHash,
            ReplayedActionCount = fresh.ActionJournal.Count,
            Session = fresh,
            Diagnostics = diagnostics
        };
    }

    private static SelectedRuntimeVariantInteractiveReplayResult FailedReplay(
        bool packageHashValid,
        bool candidateValid,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint,
        IReadOnlyList<string> diagnostics) =>
        new()
        {
            PackageHashValidated = packageHashValid,
            CandidateValidated = candidateValid,
            ExpectedStateHash = checkpoint.ExpectedStateHash,
            ReplayedActionCount = 0,
            Diagnostics = diagnostics
        };

    private static SelectedRuntimeVariantInteractiveActionResult Rejected(
        SelectedRuntimeVariantInteractiveActionRequest request,
        string stateHash,
        string diagnostic,
        SelectedRuntimeVariantActionDescriptor? descriptor = null,
        IReadOnlyList<string>? diagnostics = null)
    {
        var items = new List<string> { diagnostic };
        if (diagnostics is not null)
        {
            items.AddRange(diagnostics);
        }

        return new SelectedRuntimeVariantInteractiveActionResult
        {
            ActionRequestId = request.ActionRequestId,
            SessionId = request.SessionId,
            ActionIndex = request.ActionIndex,
            ActionId = request.ActionId,
            Category = descriptor?.Category ?? string.Empty,
            Route = descriptor?.Route ?? string.Empty,
            CommandKind = descriptor?.CommandKind ?? string.Empty,
            TargetId = descriptor?.TargetId ?? string.Empty,
            CanonicalStepId = descriptor?.CanonicalStepId ?? string.Empty,
            CanonicalStepIndex = descriptor?.CanonicalStepIndex ?? -1,
            RuntimeCommandStartIndex = descriptor?.RuntimeCommandStartIndex ?? -1,
            RuntimeCommandEndIndex = descriptor?.RuntimeCommandEndIndex ?? -1,
            ExecutionTargetId = descriptor?.ExecutionTargetId ?? string.Empty,
            ExecutionBindingValidated = false,
            StateHashBefore = stateHash,
            StateHashAfter = stateHash,
            CorrelationPassed = request.SessionId.Length > 0,
            Status = "REJECTED",
            Diagnostics = items
        };
    }

    private static void Refresh(
        SelectedRuntimeVariantInteractiveSession session,
        GamePackageDefinition package)
    {
        session.CurrentStateHash = session.CanonicalSession.CurrentStateHash;
        session.RuntimeStarted = session.CanonicalSession.RuntimeStarted;
        session.Completed = session.CapabilityPlan is null
            ? session.CanonicalSession.CurrentCommandIndex >= 13
            : session.CurrentActionIndex >= session.CapabilityPlan.OrderedActions.Count;
        session.AvailableActions = session.CapabilityPlan is null
            ? BuildCatalog(package, session)
            : BuildCapabilityCatalog(package, session, session.CapabilityPlan);
        UpdateSummaries(session);
    }

    private static IReadOnlyList<SelectedRuntimeVariantActionDescriptor> BuildCatalog(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session)
    {
        var cursor = session.CanonicalSession.CurrentCommandIndex;
        var actions = new List<SelectedRuntimeVariantActionDescriptor>
        {
            Runtime(package, session, "start_runtime", "start_runtime", "start_canonical_runtime", 0, 1),
            Runtime(package, session, "move", "move", "move_to_sign", 2, 2),
            Runtime(package, session, "interact", "interact", "interact_with_sign", 3, 3),
            Runtime(package, session, "open_dialogue", "open_dialogue", "show_old_guard_dialogue", 4, 4),
            Runtime(package, session, "start_or_update_quest", "start_or_update_quest", "start_or_update_help_healer_quest", 5, 5),
            Runtime(package, session, "show_inventory", "show_inventory", "show_inventory_state", 6, 6),
            Runtime(package, session, "craft", "craft", "craft_healing_potion", 7, 7),
            Runtime(package, session, "harvest", "harvest", "harvest_apple_tree", 8, 8),
            Runtime(package, session, "transaction", "transaction", "execute_transaction", 9, 9),
            Runtime(package, session, "begin_encounter", "begin_encounter", "start_encounter", 10, 10),
            Runtime(package, session, "basic_attack", "basic_attack", "combat_round", 11, 11),
            Presentation("show_final_state", "show_final_state", package.Manifest.PackageId, cursor == 12,
                cursor < 12 ? "complete Runtime actions first" : cursor > 12 ? "session already completed" : string.Empty),
            Presentation("inspect_inventory", "show_inventory", "inventory/player_start", session.RuntimeStarted && !session.Completed,
                session.RuntimeStarted ? "session completed" : "runtime not started"),
            Presentation("inspect_status", "show_status", package.Manifest.PackageId, session.RuntimeStarted,
                session.RuntimeStarted ? string.Empty : "runtime not started")
        };
        return actions;
    }

    private static IReadOnlyList<SelectedRuntimeVariantActionDescriptor> BuildCapabilityCatalog(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        CapabilityRuntimePlaythroughPlan plan)
    {
        var actions = new List<SelectedRuntimeVariantActionDescriptor>();
        for (var actionIndex = 0; actionIndex < plan.OrderedActions.Count; actionIndex++)
        {
            var action = plan.OrderedActions[actionIndex];
            var available = actionIndex == session.CurrentActionIndex && !session.Completed;
            if (action.PresentationOnly)
            {
                actions.Add(new SelectedRuntimeVariantActionDescriptor
                {
                    ActionId = action.ActionId,
                    Category = action.Category,
                    Route = PresentationRoute,
                    CommandKind = "read_state_summary",
                    TargetId = action.ResolvedTargetId,
                    ExecutionTargetId = action.ResolvedTargetId,
                    ExecutionBindingValidated = !string.IsNullOrWhiteSpace(action.ResolvedTargetId),
                    Prerequisites = action.DependsOnActionIds,
                    MayMutateState = false,
                    Available = available,
                    UnavailableReason = available ? string.Empty : actionIndex < session.CurrentActionIndex
                        ? "capability action already completed" : "previous capability action required"
                });
                continue;
            }

            var step = session.CanonicalSession.Steps.SingleOrDefault(item => item.ActionId == action.ActionId);
            var startIndex = action.RuntimePrimitiveId == "runtime.command.start" ? 0 : step?.Index ?? -1;
            var endIndex = step?.Index ?? -1;
            var binding = step is not null && step.RuntimePrimitiveHint == action.RuntimePrimitiveId
                          && step.TargetId == action.ResolvedTargetId && TargetExists(package, step);
            actions.Add(new SelectedRuntimeVariantActionDescriptor
            {
                ActionId = action.ActionId,
                Category = action.Category,
                Route = RuntimeRoute,
                CommandKind = step?.RuntimeCommandKind ?? string.Empty,
                TargetId = action.ResolvedTargetId,
                CanonicalStepId = step?.StepId ?? string.Empty,
                CanonicalStepIndex = step?.Index ?? -1,
                RuntimeCommandStartIndex = startIndex,
                RuntimeCommandEndIndex = endIndex,
                ExecutionTargetId = action.ResolvedTargetId,
                ExecutionBindingValidated = binding,
                Prerequisites = action.DependsOnActionIds,
                MayMutateState = true,
                Available = available && binding && session.CanonicalSession.CurrentCommandIndex == startIndex,
                UnavailableReason = binding ? available ? "canonical cursor mismatch" : actionIndex < session.CurrentActionIndex
                    ? "capability action already completed" : "previous capability action required"
                    : "capability execution binding or target is invalid"
            });
        }
        return actions;
    }

    private static SelectedRuntimeVariantActionDescriptor Runtime(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        string actionId,
        string category,
        string primaryStepId,
        int runtimeCommandStartIndex,
        int runtimeCommandEndIndex)
    {
        var steps = session.CanonicalSession.Steps;
        var primaryStep = steps.SingleOrDefault(step => step.StepId == primaryStepId);
        var rangeValid = primaryStep is not null
                         && runtimeCommandStartIndex >= 0
                         && runtimeCommandEndIndex < steps.Count
                         && runtimeCommandEndIndex >= runtimeCommandStartIndex
                         && primaryStep.Index >= runtimeCommandStartIndex
                         && primaryStep.Index <= runtimeCommandEndIndex;
        var targetsExist = rangeValid
                           && steps.Skip(runtimeCommandStartIndex)
                               .Take(runtimeCommandEndIndex - runtimeCommandStartIndex + 1)
                               .All(step => TargetExists(package, step));
        var bindingValidated = rangeValid && targetsExist;
        var available = bindingValidated
                        && session.CanonicalSession.CurrentCommandIndex == runtimeCommandStartIndex;
        return new SelectedRuntimeVariantActionDescriptor
        {
            ActionId = actionId,
            Category = category,
            Route = RuntimeRoute,
            CommandKind = primaryStep?.RuntimeCommandKind ?? string.Empty,
            TargetId = primaryStep?.TargetId ?? string.Empty,
            CanonicalStepId = primaryStep?.StepId ?? primaryStepId,
            CanonicalStepIndex = primaryStep?.Index ?? -1,
            RuntimeCommandStartIndex = runtimeCommandStartIndex,
            RuntimeCommandEndIndex = runtimeCommandEndIndex,
            ExecutionTargetId = primaryStep?.TargetId ?? string.Empty,
            ExecutionBindingValidated = bindingValidated,
            Prerequisites = runtimeCommandStartIndex == 0
                ? new List<string> { "selected package hash validated" }
                : new List<string> { "previous canonical action completed" },
            MayMutateState = true,
            Available = available,
            UnavailableReason = !bindingValidated
                ? "canonical execution binding or target is invalid"
                : session.CanonicalSession.CurrentCommandIndex < runtimeCommandStartIndex
                    ? "previous canonical action required"
                    : "canonical action already completed"
        };
    }

    private static SelectedRuntimeVariantActionDescriptor Presentation(
        string actionId,
        string category,
        string targetId,
        bool available,
        string unavailableReason) =>
        new()
        {
            ActionId = actionId,
            Category = category,
            Route = PresentationRoute,
            CommandKind = "read_state_summary",
            TargetId = targetId,
            ExecutionTargetId = targetId,
            ExecutionBindingValidated = true,
            Prerequisites = new List<string> { "runtime state exists" },
            MayMutateState = false,
            Available = available,
            UnavailableReason = available ? string.Empty : unavailableReason
        };

    private static bool ValidateExecutionBinding(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSession session,
        SelectedRuntimeVariantActionDescriptor descriptor)
    {
        var steps = session.CanonicalSession.Steps;
        if (!descriptor.ExecutionBindingValidated
            || descriptor.RuntimeCommandStartIndex < 0
            || descriptor.RuntimeCommandEndIndex >= steps.Count
            || descriptor.RuntimeCommandEndIndex < descriptor.RuntimeCommandStartIndex
            || descriptor.RuntimeCommandStartIndex != session.CanonicalSession.CurrentCommandIndex)
        {
            return false;
        }

        var primaryStep = steps.SingleOrDefault(step =>
            step.Index == descriptor.CanonicalStepIndex);
        return primaryStep is not null
               && primaryStep.StepId == descriptor.CanonicalStepId
               && primaryStep.RuntimeCommandKind == descriptor.CommandKind
               && primaryStep.TargetId == descriptor.TargetId
               && descriptor.ExecutionTargetId == primaryStep.TargetId
               && primaryStep.Index >= descriptor.RuntimeCommandStartIndex
               && primaryStep.Index <= descriptor.RuntimeCommandEndIndex
               && steps.Skip(descriptor.RuntimeCommandStartIndex)
                   .Take(descriptor.RuntimeCommandEndIndex - descriptor.RuntimeCommandStartIndex + 1)
                   .All(step => TargetExists(package, step));
    }

    private static bool TargetExists(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopStep step)
    {
        if (!string.IsNullOrWhiteSpace(step.ActionId))
        {
            return step.RuntimePrimitiveHint switch
            {
                "runtime.command.start" => package.Game.Maps.Any(item => item.Id == step.TargetId),
                "runtime.command.move" => package.Game.Maps.SelectMany(item => item.Entities).Any(item => item.Id == step.TargetId),
                "runtime.command.interact" => package.Game.Interactions.Any(item => item.Id == step.TargetId),
                "runtime.command.open_dialogue" => package.Game.Dialogues.Any(item => item.Id == step.TargetId),
                "runtime.command.start_or_update_quest" => package.Game.Quests.Any(item => item.Id == step.TargetId),
                "runtime.command.show_inventory" => package.Game.Inventories.Any(item => item.Id == step.TargetId),
                "runtime.command.craft_recipe" => package.Game.Recipes.Any(item => item.Id == step.TargetId),
                "runtime.command.harvest_resource" => package.Game.ResourceNodes.Any(item => item.Id == step.TargetId),
                "runtime.command.execute_transaction" => package.Game.Transactions.Any(item => item.Id == step.TargetId),
                "runtime.command.start_encounter" => package.Game.Encounters.Any(item => item.Id == step.TargetId),
                "runtime.command.basic_attack" => package.Game.Encounters.SelectMany(item => item.Participants).Any(item => item.Id == step.TargetId),
                "runtime.command.use_ability" => package.Game.Abilities.Any(item => item.Id == step.Args.GetValueOrDefault("abilityId", step.TargetId))
                                                   && package.Game.Encounters.SelectMany(item => item.Participants)
                                                       .Any(item => item.Id == step.Args.GetValueOrDefault("targetParticipantId", step.TargetId)),
                "runtime.command.end_turn" => package.Game.Encounters.SelectMany(item => item.Participants).Any(item => item.Id == step.TargetId),
                "runtime.command.open_container" => package.Game.Inventories.Any(item => item.Id == step.TargetId),
                "runtime.command.take_from_container" => package.Game.Items.Any(item => item.Id == step.TargetId),
                "runtime.command.equip_item" => package.Game.EquipmentSlots.Any(item => item.Id == step.TargetId),
                "runtime.command.change_progression" => package.Game.Progressions.Any(item => item.Id == step.TargetId),
                "runtime.command.advance_quest_objective" => package.Game.Quests.Any(quest =>
                    quest.Id == step.Args.GetValueOrDefault("questId")
                    && quest.Objectives.Concat(quest.Stages.SelectMany(stage => stage.Objectives))
                        .Count(objective => objective.Id == step.Args.GetValueOrDefault("objectiveId", step.TargetId)) == 1),
                "runtime.command.fail_quest" => package.Game.Quests.Any(item => item.Id == step.Args.GetValueOrDefault("questId", step.TargetId)),
                "runtime.command.choose_dialogue_option" => package.Game.Dialogues.Any(dialogue =>
                    dialogue.Id == step.Args.GetValueOrDefault("dialogueId")
                    && dialogue.Nodes.Any(node => node.Id == step.Args.GetValueOrDefault("nodeId")
                        && node.Choices.Count(choice => choice.Id == step.Args.GetValueOrDefault("choiceId", step.TargetId)) == 1)),
                "runtime.command.close_dialogue" => package.Game.Dialogues.Any(item => item.Id == step.TargetId),
                _ => false
            };
        }
        return step.StepId switch
    {
        "load_selected_package" => package.Manifest.PackageId == step.TargetId,
        "start_canonical_runtime" => package.Manifest.StartMapId == step.TargetId
                                     && package.Game.Maps.Any(map => map.Id == step.TargetId),
        "move_to_sign" => package.Game.Maps.SelectMany(map => map.Entities)
            .Any(entity => entity.Id == step.TargetId),
        "interact_with_sign" => package.Game.Interactions.Any(item => item.Id == step.TargetId),
        "show_old_guard_dialogue" => package.Game.Dialogues.Any(item => item.Id == step.TargetId),
        "start_or_update_help_healer_quest" => package.Game.Quests.Any(item => item.Id == step.TargetId),
        "show_inventory_state" => package.Game.Inventories.Any(item => item.Id == step.TargetId),
        "craft_healing_potion" => package.Game.Recipes.Any(item => item.Id == step.TargetId),
        "harvest_apple_tree" => package.Game.ResourceNodes.Any(item => item.Id == step.TargetId),
        "execute_transaction" => package.Game.Transactions.Any(item => item.Id == step.TargetId),
        "start_encounter" => package.Game.Encounters.Any(item => item.Id == step.TargetId),
        "combat_round" => package.Game.Encounters.SelectMany(item => item.Participants)
            .Any(item => item.Id == step.TargetId),
        "final_state" => package.Manifest.PackageId == step.TargetId,
        _ => false
    };
    }

    private static void UpdateSummaries(SelectedRuntimeVariantInteractiveSession session)
    {
        var snapshot = session.LatestSnapshot;
        session.LatestMapSummary = snapshot.MapSummary;
        session.LatestInventorySummary = snapshot.InventorySummary;
        session.LatestQuestSummary = snapshot.QuestSummary;
        session.LatestCombatSummary = snapshot.CombatSummary;
        session.LatestEquipmentSummary = snapshot.EquipmentSummary;
        session.LatestAttributesSummary = snapshot.AttributesSummary;
        session.LatestProgressionSummary = snapshot.ProgressionSummary;
    }

    private static SelectedRuntimeVariantInteractiveJournalEntry ToJournal(
        SelectedRuntimeVariantInteractiveActionResult result) =>
        new()
        {
            Status = result.Status,
            ActionRequestId = result.ActionRequestId,
            SessionId = result.SessionId,
            ActionIndex = result.ActionIndex,
            ActionId = result.ActionId,
            Category = result.Category,
            Route = result.Route,
            CommandKind = result.CommandKind,
            TargetId = result.TargetId,
            CanonicalStepId = result.CanonicalStepId,
            CanonicalStepIndex = result.CanonicalStepIndex,
            RuntimeCommandStartIndex = result.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = result.RuntimeCommandEndIndex,
            ExecutionTargetId = result.ExecutionTargetId,
            ExecutionBindingValidated = result.ExecutionBindingValidated,
            StateHashBefore = result.StateHashBefore,
            StateHashAfter = result.StateHashAfter,
            RuntimeExecuted = result.RuntimeExecuted,
            RuntimeMutation = result.RuntimeMutation,
            RuntimeEventCount = result.RuntimeEventCount,
            Diagnostics = result.Diagnostics.ToList()
        };

    private static SelectedRuntimeVariantInteractiveJournalEntry Clone(
        SelectedRuntimeVariantInteractiveJournalEntry entry) =>
        new()
        {
            Status = entry.Status,
            ActionRequestId = entry.ActionRequestId,
            SessionId = entry.SessionId,
            ActionIndex = entry.ActionIndex,
            ActionId = entry.ActionId,
            Category = entry.Category,
            Route = entry.Route,
            CommandKind = entry.CommandKind,
            TargetId = entry.TargetId,
            CanonicalStepId = entry.CanonicalStepId,
            CanonicalStepIndex = entry.CanonicalStepIndex,
            RuntimeCommandStartIndex = entry.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = entry.RuntimeCommandEndIndex,
            ExecutionTargetId = entry.ExecutionTargetId,
            ExecutionBindingValidated = entry.ExecutionBindingValidated,
            StateHashBefore = entry.StateHashBefore,
            StateHashAfter = entry.StateHashAfter,
            RuntimeExecuted = entry.RuntimeExecuted,
            RuntimeMutation = entry.RuntimeMutation,
            RuntimeEventCount = entry.RuntimeEventCount,
            Diagnostics = entry.Diagnostics.ToList()
        };

    private static string Format(double value) =>
        value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    private enum ConditionalActionDecisionKind
    {
        Execute,
        Skip,
        Failure
    }

    private sealed class ConditionalActionDecision
    {
        private ConditionalActionDecision(
            ConditionalActionDecisionKind kind,
            string reason,
            IReadOnlyList<string> diagnostics)
        {
            Kind = kind;
            Reason = reason;
            Diagnostics = diagnostics;
        }

        public ConditionalActionDecisionKind Kind { get; }
        public string Reason { get; }
        public IReadOnlyList<string> Diagnostics { get; }

        public static ConditionalActionDecision Execute(IReadOnlyList<string>? diagnostics = null) =>
            new(ConditionalActionDecisionKind.Execute, string.Empty, diagnostics ?? []);
        public static ConditionalActionDecision Skip(string reason, IReadOnlyList<string>? diagnostics = null) =>
            new(ConditionalActionDecisionKind.Skip, reason, diagnostics ?? []);
        public static ConditionalActionDecision Failure(string reason, IReadOnlyList<string>? diagnostics = null) =>
            new(ConditionalActionDecisionKind.Failure, reason, diagnostics ?? []);
    }
}
