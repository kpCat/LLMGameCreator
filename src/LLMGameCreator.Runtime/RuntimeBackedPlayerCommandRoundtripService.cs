using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class RuntimeBackedPlayerCommandRoundtripService :
    IRuntimeBackedPlayerCommandRoundtripService
{
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
        var canonical = _commandLoop.Execute(package, new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = request.CandidateId,
            PackagePath = request.PackagePath,
            HandoffPath = request.HandoffPath,
            Goal134TranscriptPath = request.CommandLoopResultPath,
            Goal134StateSummaryPath = request.CommandLoopSnapshotsPath,
            Goal135PlayerLoopPlanPath = request.ControlsUxModelPath,
            Goal135PlayerAdapterContractPath = request.ControlsUxScriptPath
        });
        var diagnostics = canonical.Diagnostics.ToList();
        var plan = BuildPlan();
        var requests = new List<RuntimeBackedPlayerCommandRoundtripControlRequest>();
        var responses = new List<RuntimeBackedPlayerCommandRoundtripResponse>();

        foreach (var item in plan)
        {
            var step = canonical.Steps.FirstOrDefault(step => step.Index == item.StepIndex);
            var snapshot = canonical.Snapshots.FirstOrDefault(snapshot =>
                snapshot.StepIndex == item.StepIndex);
            if (step is null || snapshot is null)
            {
                diagnostics.Add("goal141.canonical_step_missing:" + item.StepIndex);
                continue;
            }

            var controlRequest = new RuntimeBackedPlayerCommandRoundtripControlRequest
            {
                RequestIndex = item.RequestIndex,
                ControlIntent = item.ControlIntent,
                SourceControlId = item.SourceControlId,
                RuntimeCommandCoverage = item.RuntimeCommandCoverage,
                RuntimeCommandKind = step.RuntimeCommandKind,
                TargetId = step.TargetId,
                CanonicalStepIndex = step.Index,
                CanonicalStepId = step.StepId,
                RuntimeAuthority = true,
                ProjectionOnly = false,
                UnityGameplayTruth = false
            };
            var responseSnapshot = new RuntimeBackedPlayerCommandRoundtripSnapshot
            {
                RequestIndex = item.RequestIndex,
                ControlIntent = item.ControlIntent,
                RuntimeCommandCoverage = item.RuntimeCommandCoverage,
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
                RuntimeAuthority = true,
                ProjectionOnly = false,
                UnityGameplayTruth = false
            };
            requests.Add(controlRequest);
            responses.Add(new RuntimeBackedPlayerCommandRoundtripResponse
            {
                RequestIndex = item.RequestIndex,
                ControlIntent = item.ControlIntent,
                RuntimeCommandCoverage = item.RuntimeCommandCoverage,
                RuntimeExecuted = canonical.PlayerCommandLoopPassed,
                CanonicalStepRuntimeExecuted = step.RuntimeExecuted,
                Snapshot = responseSnapshot,
                Status = canonical.PlayerCommandLoopPassed ? "executed_by_runtime" : "blocked"
            });
        }

        var presentControlIntents = requests
            .Select(item => item.ControlIntent)
            .ToHashSet(StringComparer.Ordinal);
        var presentCoverage = requests
            .Select(item => item.RuntimeCommandCoverage)
            .ToHashSet(StringComparer.Ordinal);
        var missingControls = RequiredControlIntents
            .Where(item => !presentControlIntents.Contains(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var missingCoverage = RequiredRuntimeCommandCoverage
            .Where(item => !presentCoverage.Contains(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var snapshots = responses
            .OrderBy(item => item.RequestIndex)
            .Select(item => item.Snapshot)
            .ToList();
        var stateHashChain = snapshots
            .Select(item => item.StateHashAfter)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
        var controlBridge = requests.Count >= RequiredControlIntents.Count
                            && missingControls.Count == 0
                            && missingCoverage.Count == 0
                            && responses.All(response =>
                                response.Snapshot.RequestIndex == response.RequestIndex
                                && !string.IsNullOrWhiteSpace(response.Snapshot.StateHashAfter));
        var noUnclassified = diagnostics.All(item =>
            !item.Contains("Error", StringComparison.Ordinal)
            && !item.Contains("Critical", StringComparison.Ordinal));
        var executedCount = responses.Count(item => item.RuntimeExecuted);
        var stateHashChainPresent =
            canonical.StateHashChainPresent
            && stateHashChain.Count == snapshots.Count
            && stateHashChain.Count >= RequiredControlIntents.Count;
        var passed =
            canonical.PlayerCommandLoopPassed
            && executedCount >= RequiredControlIntents.Count
            && snapshots.Count >= executedCount
            && stateHashChainPresent
            && controlBridge
            && noUnclassified;

        var session = new RuntimeBackedPlayerCommandRoundtripSession
        {
            CandidateId = request.CandidateId,
            RequestCount = requests.Count,
            ExecutedRequestCount = executedCount,
            SnapshotCount = snapshots.Count,
            StateHashChainPresent = stateHashChainPresent,
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
            RoundtripRequestCount = requests.Count,
            RuntimeExecutedRequestCount = executedCount,
            RoundtripSnapshotCount = snapshots.Count,
            StateHashChainPresent = stateHashChainPresent,
            RuntimeAuthority = true,
            ProjectionOnly = false,
            UnityGameplayTruth = false,
            ControlRequestBridgePresent = controlBridge,
            UnityConsumesRoundtripResult = true,
            NoUnclassifiedErrorDiagnostics = noUnclassified,
            RuntimeBackedPlayerCommandRoundtripPassed = passed,
            RequiredControlIntents = RequiredControlIntents,
            MissingControlIntents = missingControls,
            RequiredRuntimeCommandCoverage = RequiredRuntimeCommandCoverage,
            MissingRuntimeCommandCoverage = missingCoverage,
            StateHashChain = stateHashChain,
            Requests = requests,
            Responses = responses,
            Snapshots = snapshots,
            Session = session,
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<PlanItem> BuildPlan() =>
    [
        PlanItem.Create(0, "load_model", "load_model", "load_package_or_session", 0),
        PlanItem.Create(1, "reset_first", "first", "show_or_select_start_state", 1),
        PlanItem.Create(2, "step_once", "step_once", "advance_to_interaction", 3),
        PlanItem.Create(3, "next_frame", "next", "advance_to_dialogue_or_quest", 5),
        PlanItem.Create(4, "play_all_to_end", "play_all_to_end", "advance_to_inventory_or_crafting", 7),
        PlanItem.Create(5, "copy_frame_summary", "copy_current_frame_summary", "advance_to_combat_or_final_state", 11)
    ];

    private sealed class PlanItem
    {
        private PlanItem()
        {
        }

        public int RequestIndex { get; private set; }
        public string ControlIntent { get; private set; } = string.Empty;
        public string SourceControlId { get; private set; } = string.Empty;
        public string RuntimeCommandCoverage { get; private set; } = string.Empty;
        public int StepIndex { get; private set; }

        public static PlanItem Create(
            int requestIndex,
            string controlIntent,
            string sourceControlId,
            string runtimeCommandCoverage,
            int stepIndex) =>
            new()
            {
                RequestIndex = requestIndex,
                ControlIntent = controlIntent,
                SourceControlId = sourceControlId,
                RuntimeCommandCoverage = runtimeCommandCoverage,
                StepIndex = stepIndex
            };
    }
}
