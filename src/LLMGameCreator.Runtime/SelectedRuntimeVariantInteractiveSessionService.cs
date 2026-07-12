using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class SelectedRuntimeVariantInteractiveSessionService :
    ISelectedRuntimeVariantInteractiveSessionService
{
    private const string RuntimeRoute = "runtime_session";
    private const string PresentationRoute = "presentation_only";
    private readonly ICanonicalRuntimePlayerCommandLoopService _commandLoop;

    public SelectedRuntimeVariantInteractiveSessionService(
        ICanonicalRuntimePlayerCommandLoopService commandLoop)
    {
        _commandLoop = commandLoop ?? throw new ArgumentNullException(nameof(commandLoop));
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

        var runtimeExecuted = false;
        var runtimeMutation = false;
        var eventCount = 0;
        var diagnostics = new List<string>();
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
                           && replay.Status == "EXECUTED"
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
                "runtime.command.open_container" => package.Game.Inventories.Any(item => item.Id == step.TargetId),
                "runtime.command.take_from_container" => package.Game.Items.Any(item => item.Id == step.TargetId),
                "runtime.command.equip_item" => package.Game.EquipmentSlots.Any(item => item.Id == step.TargetId),
                "runtime.command.change_progression" => package.Game.Progressions.Any(item => item.Id == step.TargetId),
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
            RuntimeEventCount = result.RuntimeEventCount
        };

    private static SelectedRuntimeVariantInteractiveJournalEntry Clone(
        SelectedRuntimeVariantInteractiveJournalEntry entry) =>
        new()
        {
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
            RuntimeEventCount = entry.RuntimeEventCount
        };
}
