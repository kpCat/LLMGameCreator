using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class RuntimeBackedPlayerCommandRoundtripService :
    IRuntimeBackedPlayerCommandRoundtripService
{
    private const string RuntimeSessionRoute = "runtime_session";
    private const string RuntimeCommandRoute = "runtime_command";
    private const string RuntimeCommandBatchRoute = "runtime_command_batch";
    private const string PresentationOnlyRoute = "presentation_only";

    private static readonly IReadOnlyList<string> RequiredControlIntents =
    [
        "load_model",
        "reset_first",
        "step_once",
        "next_frame",
        "play_all_to_end",
        "copy_frame_summary"
    ];

    private static readonly IReadOnlyList<string> RequiredRuntimeCommandCoverage =
    [
        "load_package_or_session",
        "show_or_select_start_state",
        "advance_to_interaction",
        "advance_to_dialogue_or_quest",
        "advance_to_inventory_or_crafting",
        "advance_to_combat_or_final_state"
    ];

    private readonly ICanonicalRuntimePlayerCommandLoopService _commandLoop;

    public RuntimeBackedPlayerCommandRoundtripService(
        ICanonicalRuntimePlayerCommandLoopService commandLoop)
    {
        _commandLoop = commandLoop;
    }

    public static RuntimeBackedPlayerCommandRoundtripService CreateDefault() =>
        new(CanonicalRuntimePlayerCommandLoopService.CreateDefault());

    public RuntimeBackedPlayerCommandRoundtripResult Execute(
        GamePackageDefinition package,
        RuntimeBackedPlayerCommandRoundtripRequest request)
    {
        var canonicalRequest = new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath,
            HandoffPath = request.HandoffPath,
            Goal134TranscriptPath = request.CommandLoopResultPath,
            Goal134StateSummaryPath = request.CommandLoopSnapshotsPath,
            Goal135PlayerLoopPlanPath = request.ControlsUxModelPath,
            Goal135PlayerAdapterContractPath = request.ControlsUxScriptPath
        };
        var session = _commandLoop.BeginSession(package, canonicalRequest);
        var diagnostics = new List<string>();
        var requests = new List<RuntimeBackedPlayerCommandRoundtripControlRequest>();
        var responses = new List<RuntimeBackedPlayerCommandRoundtripResponse>();
        var snapshots = new List<RuntimeBackedPlayerCommandRoundtripSnapshot>();

        foreach (var item in BuildPlan())
        {
            var range = ResolveRange(item, session);
            var controlRequest = BuildControlRequest(item, session, range);
            requests.Add(controlRequest);

            var response = item.Route == PresentationOnlyRoute
                ? BuildPresentationResponse(package, session, controlRequest)
                : BuildRuntimeResponse(
                    controlRequest,
                    _commandLoop.ExecuteRange(
                        package,
                        session,
                        new CanonicalRuntimePlayerCommandLoopExecutionRequest
                        {
                            RequestedOperation = item.RequestedOperation,
                            RuntimeCommandStartIndex = range.StartIndex,
                            RuntimeCommandEndIndex = range.EndIndex
                        }));

            diagnostics.AddRange(response.ProducedSnapshots
                .Where(snapshot => !snapshot.CorrelationPassed)
                .Select(snapshot => "goal141.correlation_failed:" + snapshot.RequestId));
            diagnostics.AddRange(response.Status == "blocked"
                ? ["goal141.runtime_request_blocked:" + response.RequestId]
                : []);
            responses.Add(response);
            snapshots.AddRange(response.ProducedSnapshots);
        }

        diagnostics.AddRange(session.Diagnostics);

        var presentControlIntents = requests
            .Select(item => item.ControlIntent)
            .ToHashSet(StringComparer.Ordinal);
        var presentCoverage = snapshots
            .Select(item => item.RuntimeCommandCoverage)
            .Where(item => !string.IsNullOrWhiteSpace(item)
                           && item != PresentationOnlyRoute)
            .ToHashSet(StringComparer.Ordinal);
        var missingControls = RequiredControlIntents
            .Where(item => !presentControlIntents.Contains(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var missingCoverage = RequiredRuntimeCommandCoverage
            .Where(item => !presentCoverage.Contains(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var runtimeRoutedCount = requests.Count(item => item.Route != PresentationOnlyRoute);
        var presentationOnlyCount = requests.Count(item => item.Route == PresentationOnlyRoute);
        var executedCount = responses.Count(item => item.RuntimeExecuted);
        var presentationExecutionCount = responses.Count(item =>
            item.Route == PresentationOnlyRoute && item.RuntimeExecuted);
        var mutatingPresentationCount = responses.Count(item =>
            item.Route == PresentationOnlyRoute && item.RuntimeMutation);
        var requestResponseCorrelation = responses.Count == requests.Count
                                         && responses.All(response =>
                                             response.CorrelationPassed
                                             && requests.Any(requestItem =>
                                                 requestItem.RequestId == response.RequestId
                                                 && requestItem.RequestIndex == response.RequestIndex
                                                 && requestItem.ControlIntent == response.ControlIntent
                                                 && requestItem.Route == response.Route));
        var sequentialCursor = SequentialCursorContinuityPassed(responses);
        var stateHashContinuity = StateHashContinuityPassed(responses);
        var copyUnchanged = PresentationRequestUnchanged(responses, "copy_frame_summary");
        var loadUnchanged = PresentationRequestUnchanged(responses, "load_model");
        var playAll = responses.FirstOrDefault(item => item.ControlIntent == "play_all_to_end");
        var playAllExecutedRemaining = playAll is not null
                                       && playAll.Route == RuntimeCommandBatchRoute
                                       && playAll.RuntimeExecuted
                                       && playAll.ExecutedCommandCount > 0
                                       && playAll.RuntimeCommandEndIndex == session.Steps.Count - 1;
        var noUnrelatedGameplayMapping = NoPresentationRequestMappedToGameplay(requests, responses);
        var stateHashChain = snapshots
            .Select(item => item.StateHashAfter)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
        var stateHashChainPresent =
            snapshots.Count > 0
            && stateHashChain.Count == snapshots.Count
            && snapshots.All(snapshot =>
                !string.IsNullOrWhiteSpace(snapshot.StateHashBefore)
                && !string.IsNullOrWhiteSpace(snapshot.StateHashAfter))
            && snapshots.Skip(1).Select((snapshot, index) => new
                {
                    Previous = snapshots[index],
                    Current = snapshot
                })
                .All(item => string.Equals(
                    item.Previous.StateHashAfter,
                    item.Current.StateHashBefore,
                    StringComparison.Ordinal));
        var controlBridge = requests.Count == RequiredControlIntents.Count
                            && missingControls.Count == 0
                            && missingCoverage.Count == 0
                            && requestResponseCorrelation;
        var noUnclassified = diagnostics.All(item =>
            !item.Contains("Error", StringComparison.Ordinal)
            && !item.Contains("Critical", StringComparison.Ordinal));
        var semanticPassed =
            requests.Count == 6
            && runtimeRoutedCount == 4
            && presentationOnlyCount == 2
            && executedCount == 4
            && presentationExecutionCount == 0
            && mutatingPresentationCount == 0
            && responses.Count == 6
            && requestResponseCorrelation
            && sequentialCursor
            && stateHashContinuity
            && copyUnchanged
            && loadUnchanged
            && playAllExecutedRemaining
            && noUnrelatedGameplayMapping;
        var passed =
            semanticPassed
            && stateHashChainPresent
            && controlBridge
            && noUnclassified;

        var runtimeSession = new RuntimeBackedPlayerCommandRoundtripSession
        {
            CandidateId = request.CandidateId,
            RequestCount = requests.Count,
            ExecutedRequestCount = executedCount,
            SnapshotCount = snapshots.Count,
            RuntimeRoutedRequestCount = runtimeRoutedCount,
            PresentationOnlyRequestCount = presentationOnlyCount,
            PresentationOnlyRuntimeExecutionCount = presentationExecutionCount,
            RuntimeMutatingPresentationRequestCount = mutatingPresentationCount,
            ResponseCount = responses.Count,
            StateHashChainPresent = stateHashChainPresent,
            RequestResponseCorrelationPassed = requestResponseCorrelation,
            SequentialCursorContinuityPassed = sequentialCursor,
            StateHashContinuityPassed = stateHashContinuity,
            CopySummaryStateUnchanged = copyUnchanged,
            LoadModelStateUnchanged = loadUnchanged,
            PlayAllExecutedRemainingCommands = playAllExecutedRemaining,
            NoControlIntentMappedToUnrelatedGameplayCommand = noUnrelatedGameplayMapping,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            ControlRequestBridgePresent = controlBridge,
            UnityConsumesRoundtripResult = true,
            StateHashChain = stateHashChain,
            Requests = requests,
            Responses = responses
        };

        return new RuntimeBackedPlayerCommandRoundtripResult
        {
            CandidateId = request.CandidateId,
            Inputs = new RuntimeBackedPlayerCommandRoundtripInput
            {
                CandidateId = request.CandidateId,
                PackagePath = request.PackagePath,
                HandoffPath = request.HandoffPath,
                ControlsUxModelPath = request.ControlsUxModelPath,
                ControlsUxResultPath = request.ControlsUxResultPath,
                ControlsUxScriptPath = request.ControlsUxScriptPath,
                CommandLoopSnapshotsPath = request.CommandLoopSnapshotsPath,
                CommandLoopResultPath = request.CommandLoopResultPath
            },
            TotalControlRequestCount = requests.Count,
            RoundtripRequestCount = requests.Count,
            RuntimeRoutedRequestCount = runtimeRoutedCount,
            PresentationOnlyRequestCount = presentationOnlyCount,
            RuntimeExecutedRequestCount = executedCount,
            PresentationOnlyRuntimeExecutionCount = presentationExecutionCount,
            RuntimeMutatingPresentationRequestCount = mutatingPresentationCount,
            ResponseCount = responses.Count,
            RoundtripSnapshotCount = snapshots.Count,
            StateHashChainPresent = stateHashChainPresent,
            RequestResponseCorrelationPassed = requestResponseCorrelation,
            SequentialCursorContinuityPassed = sequentialCursor,
            StateHashContinuityPassed = stateHashContinuity,
            CopySummaryStateUnchanged = copyUnchanged,
            LoadModelStateUnchanged = loadUnchanged,
            PlayAllExecutedRemainingCommands = playAllExecutedRemaining,
            NoControlIntentMappedToUnrelatedGameplayCommand = noUnrelatedGameplayMapping,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            ControlRequestBridgePresent = controlBridge,
            UnityConsumesRoundtripResult = true,
            NoUnclassifiedErrorDiagnostics = noUnclassified,
            RoundtripSemanticCorrectnessPassed = semanticPassed,
            RuntimeBackedPlayerCommandRoundtripPassed = passed,
            RequiredControlIntents = RequiredControlIntents,
            MissingControlIntents = missingControls,
            RequiredRuntimeCommandCoverage = RequiredRuntimeCommandCoverage,
            MissingRuntimeCommandCoverage = missingCoverage,
            StateHashChain = stateHashChain,
            Requests = requests,
            Responses = responses,
            Snapshots = snapshots,
            Session = runtimeSession,
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<PlanItem> BuildPlan() =>
    [
        PlanItem.Create(0, "load_model", "load_model", PresentationOnlyRoute, "preserve_loaded_model_state"),
        PlanItem.Create(1, "reset_first", "first", RuntimeSessionRoute, "reset_or_initialize_session"),
        PlanItem.Create(2, "step_once", "step_once", RuntimeCommandRoute, "execute_next_runtime_command"),
        PlanItem.Create(3, "next_frame", "next", RuntimeCommandRoute, "execute_next_runtime_command"),
        PlanItem.Create(4, "play_all_to_end", "play_all_to_end", RuntimeCommandBatchRoute, "execute_remaining_runtime_commands"),
        PlanItem.Create(5, "copy_frame_summary", "copy_current_frame_summary", PresentationOnlyRoute, "copy_current_frame_summary")
    ];

    private static RuntimeRange ResolveRange(
        PlanItem item,
        CanonicalRuntimePlayerCommandLoopSession session)
    {
        if (item.Route == PresentationOnlyRoute)
        {
            return new RuntimeRange(-1, -1);
        }

        var start = session.CurrentCommandIndex;
        var end = item.ControlIntent switch
        {
            "reset_first" => Math.Min(1, session.Steps.Count - 1),
            "play_all_to_end" => session.Steps.Count - 1,
            _ => start
        };
        return new RuntimeRange(start, end);
    }

    private static RuntimeBackedPlayerCommandRoundtripControlRequest BuildControlRequest(
        PlanItem item,
        CanonicalRuntimePlayerCommandLoopSession session,
        RuntimeRange range)
    {
        var primaryStep = item.Route == PresentationOnlyRoute
            ? null
            : session.Steps
                .Skip(range.StartIndex)
                .Take(range.EndIndex - range.StartIndex + 1)
                .FirstOrDefault(step => step.RuntimeExecuted)
              ?? session.Steps.ElementAtOrDefault(range.StartIndex);
        return new RuntimeBackedPlayerCommandRoundtripControlRequest
        {
            RequestId = RequestId(item),
            RequestIndex = item.RequestIndex,
            ControlIntent = item.ControlIntent,
            Route = item.Route,
            RequestedOperation = item.RequestedOperation,
            SourceControlId = item.SourceControlId,
            RuntimeCommandCoverage = primaryStep is null
                ? PresentationOnlyRoute
                : CoverageForCategory(primaryStep.Category),
            RuntimeCommandKind = primaryStep?.RuntimeCommandKind ?? "none",
            TargetId = primaryStep?.TargetId ?? string.Empty,
            RuntimeCommandStartIndex = range.StartIndex,
            RuntimeCommandEndIndex = range.EndIndex,
            CanonicalStepIndex = primaryStep?.Index ?? -1,
            CanonicalStepId = primaryStep?.StepId ?? string.Empty,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripResponse BuildRuntimeResponse(
        RuntimeBackedPlayerCommandRoundtripControlRequest request,
        CanonicalRuntimePlayerCommandLoopExecutionResult execution)
    {
        var produced = execution.Snapshots
            .Select(snapshot => FromCanonicalSnapshot(request, execution, snapshot))
            .ToList();
        var responseSnapshot = produced.LastOrDefault()
                               ?? EmptySnapshot(request, execution.StateHashBefore);
        var correlation = produced.All(snapshot =>
                              snapshot.RequestId == request.RequestId
                              && snapshot.RequestIndex == request.RequestIndex
                              && snapshot.ControlIntent == request.ControlIntent)
                          && execution.RuntimeCommandStartIndex == request.RuntimeCommandStartIndex
                          && execution.RuntimeCommandEndIndex == request.RuntimeCommandEndIndex;
        return new RuntimeBackedPlayerCommandRoundtripResponse
        {
            RequestId = request.RequestId,
            RequestIndex = request.RequestIndex,
            ControlIntent = request.ControlIntent,
            Route = request.Route,
            RequestedOperation = request.RequestedOperation,
            RuntimeCommandCoverage = request.RuntimeCommandCoverage,
            RuntimeCommandStartIndex = request.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = request.RuntimeCommandEndIndex,
            RuntimeExecuted = execution.RuntimeExecuted,
            CanonicalStepRuntimeExecuted = execution.Steps.Any(step => step.RuntimeExecuted),
            RuntimeMutation = execution.RuntimeMutation,
            ExecutedCommandCount = execution.ExecutedCommandCount,
            ProducedSnapshotCount = produced.Count,
            StateHashBefore = execution.StateHashBefore,
            StateHashAfter = execution.StateHashAfter,
            EventCount = execution.EventCount,
            CorrelationPassed = correlation,
            Snapshot = responseSnapshot,
            ProducedSnapshots = produced,
            Status = execution.RuntimeExecuted && execution.Success
                ? "executed_by_runtime"
                : "blocked"
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripResponse BuildPresentationResponse(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopSession session,
        RuntimeBackedPlayerCommandRoundtripControlRequest request)
    {
        var snapshot = PresentationSnapshot(package, session, request);
        return new RuntimeBackedPlayerCommandRoundtripResponse
        {
            RequestId = request.RequestId,
            RequestIndex = request.RequestIndex,
            ControlIntent = request.ControlIntent,
            Route = request.Route,
            RequestedOperation = request.RequestedOperation,
            RuntimeCommandCoverage = PresentationOnlyRoute,
            RuntimeCommandStartIndex = -1,
            RuntimeCommandEndIndex = -1,
            RuntimeExecuted = false,
            CanonicalStepRuntimeExecuted = false,
            RuntimeMutation = false,
            ExecutedCommandCount = 0,
            ProducedSnapshotCount = 1,
            StateHashBefore = session.CurrentStateHash,
            StateHashAfter = session.CurrentStateHash,
            EventCount = 0,
            CorrelationPassed = true,
            Snapshot = snapshot,
            ProducedSnapshots = [snapshot],
            Status = "presentation_only_preserved_state"
        };
    }

    private static RuntimeBackedPlayerCommandRoundtripSnapshot FromCanonicalSnapshot(
        RuntimeBackedPlayerCommandRoundtripControlRequest request,
        CanonicalRuntimePlayerCommandLoopExecutionResult execution,
        CanonicalRuntimePlayerCommandLoopSnapshot snapshot) =>
        new()
        {
            RequestId = request.RequestId,
            RequestIndex = request.RequestIndex,
            ControlIntent = request.ControlIntent,
            Route = request.Route,
            RequestedOperation = request.RequestedOperation,
            RuntimeCommandCoverage = CoverageForCategory(snapshot.Category),
            RuntimeCommandStartIndex = request.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = request.RuntimeCommandEndIndex,
            CanonicalStepIndex = snapshot.StepIndex,
            CanonicalStepId = snapshot.StepId,
            StateHashBefore = snapshot.StateHashBefore,
            StateHashAfter = snapshot.StateHashAfter,
            MapSummary = snapshot.MapSummary,
            VisibleInteractionSummary = snapshot.VisibleInteractionSummary,
            DialogueSummary = snapshot.DialogueSummary,
            QuestSummary = snapshot.QuestSummary,
            InventorySummary = snapshot.InventorySummary,
            CombatSummary = snapshot.CombatSummary,
            RuntimeEventCount = snapshot.RuntimeEvents.Count,
            RuntimeExecuted = execution.RuntimeExecuted,
            RuntimeMutation = !string.Equals(snapshot.StateHashBefore, snapshot.StateHashAfter, StringComparison.Ordinal),
            ExecutedCommandCount = execution.ExecutedCommandCount,
            ProducedSnapshotCount = execution.ProducedSnapshotCount,
            EventCount = snapshot.RuntimeEvents.Count,
            CorrelationPassed = true,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false
        };

    private static RuntimeBackedPlayerCommandRoundtripSnapshot PresentationSnapshot(
        GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopSession session,
        RuntimeBackedPlayerCommandRoundtripControlRequest request) =>
        new()
        {
            RequestId = request.RequestId,
            RequestIndex = request.RequestIndex,
            ControlIntent = request.ControlIntent,
            Route = request.Route,
            RequestedOperation = request.RequestedOperation,
            RuntimeCommandCoverage = PresentationOnlyRoute,
            RuntimeCommandStartIndex = -1,
            RuntimeCommandEndIndex = -1,
            CanonicalStepIndex = -1,
            CanonicalStepId = string.Empty,
            StateHashBefore = session.CurrentStateHash,
            StateHashAfter = session.CurrentStateHash,
            MapSummary = MapSummary(package, session.RuntimeSession),
            VisibleInteractionSummary = string.Empty,
            DialogueSummary = DialogueSummary(session.RuntimeSession),
            QuestSummary = QuestSummary(session.RuntimeSession),
            InventorySummary = InventorySummary(session.RuntimeSession),
            CombatSummary = CombatSummary(session.RuntimeSession),
            RuntimeEventCount = 0,
            RuntimeExecuted = false,
            RuntimeMutation = false,
            ExecutedCommandCount = 0,
            ProducedSnapshotCount = 1,
            EventCount = 0,
            CorrelationPassed = true,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false
        };

    private static RuntimeBackedPlayerCommandRoundtripSnapshot EmptySnapshot(
        RuntimeBackedPlayerCommandRoundtripControlRequest request,
        string stateHash) =>
        new()
        {
            RequestId = request.RequestId,
            RequestIndex = request.RequestIndex,
            ControlIntent = request.ControlIntent,
            Route = request.Route,
            RequestedOperation = request.RequestedOperation,
            RuntimeCommandCoverage = request.RuntimeCommandCoverage,
            RuntimeCommandStartIndex = request.RuntimeCommandStartIndex,
            RuntimeCommandEndIndex = request.RuntimeCommandEndIndex,
            StateHashBefore = stateHash,
            StateHashAfter = stateHash,
            CorrelationPassed = false,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false
        };

    private static bool SequentialCursorContinuityPassed(
        IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> responses)
    {
        var cursor = 0;
        foreach (var response in responses.OrderBy(item => item.RequestIndex))
        {
            if (response.Route == PresentationOnlyRoute)
            {
                if (response.RuntimeCommandStartIndex != -1 || response.RuntimeCommandEndIndex != -1)
                {
                    return false;
                }

                continue;
            }

            if (response.RuntimeCommandStartIndex != cursor)
            {
                return false;
            }

            cursor = response.RuntimeCommandEndIndex + 1;
        }

        return true;
    }

    private static bool StateHashContinuityPassed(
        IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> responses) =>
        responses.Count > 0
        && responses.All(response =>
            !string.IsNullOrWhiteSpace(response.StateHashBefore)
            && !string.IsNullOrWhiteSpace(response.StateHashAfter))
        && responses
            .OrderBy(response => response.RequestIndex)
            .Skip(1)
            .Select((response, index) => new
            {
                Previous = responses.OrderBy(item => item.RequestIndex).ElementAt(index),
                Current = response
            })
            .All(item => string.Equals(
                item.Previous.StateHashAfter,
                item.Current.StateHashBefore,
                StringComparison.Ordinal));

    private static bool PresentationRequestUnchanged(
        IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> responses,
        string controlIntent)
    {
        var response = responses.FirstOrDefault(item => item.ControlIntent == controlIntent);
        return response is not null
               && response.Route == PresentationOnlyRoute
               && !response.RuntimeExecuted
               && !response.RuntimeMutation
               && response.ExecutedCommandCount == 0
               && response.EventCount == 0
               && string.Equals(response.StateHashBefore, response.StateHashAfter, StringComparison.Ordinal);
    }

    private static bool NoPresentationRequestMappedToGameplay(
        IReadOnlyList<RuntimeBackedPlayerCommandRoundtripControlRequest> requests,
        IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> responses) =>
        requests.Where(item => item.Route == PresentationOnlyRoute)
            .All(request =>
            {
                var response = responses.FirstOrDefault(item => item.RequestId == request.RequestId);
                return response is not null
                       && request.RuntimeCommandKind == "none"
                       && request.CanonicalStepIndex == -1
                       && string.IsNullOrWhiteSpace(request.CanonicalStepId)
                       && !response.RuntimeExecuted
                       && !response.CanonicalStepRuntimeExecuted
                       && response.ExecutedCommandCount == 0
                       && response.EventCount == 0
                       && response.Snapshot.CanonicalStepIndex == -1
                       && response.Snapshot.CanonicalStepId.Length == 0;
            });

    private static string RequestId(PlanItem item) =>
        "goal141-request-" + item.RequestIndex.ToString("000", System.Globalization.CultureInfo.InvariantCulture)
        + "-" + item.ControlIntent;

    private static string CoverageForCategory(string category) =>
        category switch
        {
            "load_package" => "load_package_or_session",
            "start_runtime" => "show_or_select_start_state",
            "move" or "interact" => "advance_to_interaction",
            "show_dialogue" or "start_or_update_quest" => "advance_to_dialogue_or_quest",
            "show_inventory" or "craft" or "harvest" or "transaction" => "advance_to_inventory_or_crafting",
            "encounter" or "combat_round" or "final_state" => "advance_to_combat_or_final_state",
            _ => category
        };

    private static string MapSummary(GamePackageDefinition package, UnifiedRuntimeSession session) =>
        string.IsNullOrWhiteSpace(session.MapState.CurrentMapId)
            ? package.Manifest.StartMapId + " @ not_started"
            : session.MapState.CurrentMapId
              + " @ "
              + session.MapState.PlayerPosition.X
              + ","
              + session.MapState.PlayerPosition.Y;

    private static string DialogueSummary(UnifiedRuntimeSession session) =>
        session.GameplayState.ActiveDialogue == null
            ? string.Empty
            : session.GameplayState.ActiveDialogue.DialogueId
              + ":"
              + session.GameplayState.ActiveDialogue.CurrentNodeId
              + ":"
              + session.GameplayState.ActiveDialogue.Open;

    private static string QuestSummary(UnifiedRuntimeSession session) =>
        string.Join("; ", session.GameplayState.Quests
            .OrderBy(quest => quest.QuestId, StringComparer.Ordinal)
            .Select(quest => quest.QuestId + ":" + quest.State + ":" + (quest.CurrentStageId ?? string.Empty)));

    private static string InventorySummary(UnifiedRuntimeSession session) =>
        string.Join("; ", session.GameplayState.Inventories
            .OrderBy(inventory => inventory.Id, StringComparer.Ordinal)
            .Select(inventory => inventory.Id + "=" + string.Join(",", inventory.Stacks
                .OrderBy(stack => stack.ItemId, StringComparer.Ordinal)
                .Select(stack => stack.ItemId + ":" + Format(stack.Amount)))));

    private static string CombatSummary(UnifiedRuntimeSession session)
    {
        if (session.GameplayState.ActiveEncounter == null)
        {
            return string.Empty;
        }

        var encounter = session.GameplayState.ActiveEncounter;
        var participantSummary = string.Join(",", encounter.Participants
            .OrderBy(participant => participant.Id, StringComparer.Ordinal)
            .Select(participant =>
                participant.Id
                + "[alive="
                + participant.Alive
                + ";"
                + string.Join("|", participant.Resources
                    .OrderBy(resource => resource.ResourceId, StringComparer.Ordinal)
                    .Select(resource => resource.ResourceId + "=" + Format(resource.Amount)))
                + "]"));
        return encounter.EncounterId
               + ":round="
               + encounter.Round
               + ":turn="
               + encounter.TurnIndex
               + ":active="
               + encounter.Active
               + ":participants="
               + participantSummary;
    }

    private static string Format(double value) =>
        value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    private sealed class RuntimeRange
    {
        public RuntimeRange(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public int StartIndex { get; }
        public int EndIndex { get; }
    }

    private sealed class PlanItem
    {
        private PlanItem()
        {
        }

        public int RequestIndex { get; private set; }
        public string ControlIntent { get; private set; } = string.Empty;
        public string SourceControlId { get; private set; } = string.Empty;
        public string Route { get; private set; } = string.Empty;
        public string RequestedOperation { get; private set; } = string.Empty;

        public static PlanItem Create(
            int requestIndex,
            string controlIntent,
            string sourceControlId,
            string route,
            string requestedOperation) =>
            new()
            {
                RequestIndex = requestIndex,
                ControlIntent = controlIntent,
                SourceControlId = sourceControlId,
                Route = route,
                RequestedOperation = requestedOperation
            };
    }
}
