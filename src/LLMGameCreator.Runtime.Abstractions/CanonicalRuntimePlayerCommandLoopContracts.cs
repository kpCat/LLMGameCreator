namespace LLMGameCreator.Runtime.Abstractions;

public sealed class CapabilityRuntimePlaythroughAction
{
    public string ContractId { get; set; } = string.Empty;
    public string CapabilityId { get; set; } = string.Empty;
    public string ActionId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public int Order { get; set; }
    public string RuntimePrimitiveId { get; set; } = string.Empty;
    public string TargetSelector { get; set; } = string.Empty;
    public string ResolvedTargetId { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Args { get; set; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyList<string> DependsOnActionIds { get; set; } = new List<string>();
    public bool CheckpointBoundaryAfter { get; set; }
    public bool PresentationOnly { get; set; }
    public bool Required { get; set; } = true;
    public IReadOnlyList<string> ExpectedRuntimeEffects { get; set; } = new List<string>();
}

public sealed class CapabilityRuntimePlaythroughPlan
{
    public string PlanId { get; set; } = string.Empty;
    public IReadOnlyList<string> SelectedModuleIds { get; set; } = new List<string>();
    public IReadOnlyList<string> CapabilityIds { get; set; } = new List<string>();
    public IReadOnlyList<CapabilityRuntimePlaythroughAction> OrderedActions { get; set; } =
        new List<CapabilityRuntimePlaythroughAction>();
    public string CheckpointBoundaryActionId { get; set; } = string.Empty;
    public IReadOnlyList<string> RuntimePrimitiveIds { get; set; } = new List<string>();
    public string ActionPlanSignature { get; set; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public sealed class CanonicalRuntimePlayerCommandLoopRequest
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
    public string Goal134TranscriptPath { get; set; } = string.Empty;
    public string Goal134StateSummaryPath { get; set; } = string.Empty;
    public string Goal135PlayerLoopPlanPath { get; set; } = string.Empty;
    public string Goal135PlayerAdapterContractPath { get; set; } = string.Empty;
    public CapabilityRuntimePlaythroughPlan? CapabilityPlan { get; set; }
}

public sealed class CanonicalRuntimePlayerCommandLoopInput
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
    public string Goal134TranscriptPath { get; set; } = string.Empty;
    public string Goal134StateSummaryPath { get; set; } = string.Empty;
    public string Goal135PlayerLoopPlanPath { get; set; } = string.Empty;
    public string Goal135PlayerAdapterContractPath { get; set; } = string.Empty;
    public bool PackagePathExists { get; set; }
    public bool HandoffPathExists { get; set; }
    public bool Goal134TranscriptPathExists { get; set; }
    public bool Goal134StateSummaryPathExists { get; set; }
    public bool Goal135PlayerLoopPlanPathExists { get; set; }
    public bool Goal135PlayerAdapterContractPathExists { get; set; }
}

public sealed class CanonicalRuntimePlayerCommandLoopStep
{
    public int Index { get; set; }
    public string StepId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CommandLabel { get; set; } = string.Empty;
    public string RuntimeCommandKind { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string RuntimePrimitiveHint { get; set; } = string.Empty;
    public string ActionId { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Args { get; set; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
    public bool RuntimeExecuted { get; set; } = true;
    public bool RequiredForGreen { get; set; } = true;
}

public sealed class CanonicalRuntimePlayerCommandLoopRuntimeEvent
{
    public int EventIndex { get; set; }
    public int StepIndex { get; set; }
    public string StepId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Args { get; set; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed class CanonicalRuntimePlayerCommandLoopSnapshot
{
    public string Status { get; set; } = "EXECUTED";
    public int StepIndex { get; set; }
    public string StepId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CommandLabel { get; set; } = string.Empty;
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
    public string MapSummary { get; set; } = string.Empty;
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public string VisibleInteractionSummary { get; set; } = string.Empty;
    public string DialogueSummary { get; set; } = string.Empty;
    public string QuestSummary { get; set; } = string.Empty;
    public string InventorySummary { get; set; } = string.Empty;
    public string CombatSummary { get; set; } = string.Empty;
    public string EquipmentSummary { get; set; } = string.Empty;
    public string AttributesSummary { get; set; } = string.Empty;
    public string ProgressionSummary { get; set; } = string.Empty;
    public string FactionSummary { get; set; } = string.Empty;
    public string DialogueChoicesSummary { get; set; } = string.Empty;
    public string ResourceSummary { get; set; } = string.Empty;
    public string FlagSummary { get; set; } = string.Empty;
    public string SocialSummary { get; set; } = string.Empty;
    public string DiagnosticSummary { get; set; } = string.Empty;
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopRuntimeEvent> RuntimeEvents { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopRuntimeEvent>();
}

public sealed class CanonicalRuntimePlayerCommandLoopSession
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public int CurrentCommandIndex { get; set; }
    public string CurrentStateHash { get; set; } = "not_loaded";
    public bool RuntimeStarted { get; set; }
    public bool RuntimeExecutionSucceeded { get; set; } = true;
    public int EventIndex { get; set; }
    public UnifiedRuntimeSession RuntimeSession { get; set; } = new();
    public CapabilityRuntimePlaythroughPlan? CapabilityPlan { get; set; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopStep> Steps { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopStep>();
    public List<CanonicalRuntimePlayerCommandLoopSnapshot> Snapshots { get; set; } = new();
    public List<string> StateHashChain { get; set; } = new();
    public List<string> MissingRuntimePrimitives { get; set; } = new();
    public List<string> Diagnostics { get; set; } = new();
}

public sealed class CanonicalRuntimePlayerCommandLoopExecutionRequest
{
    public string RequestedOperation { get; set; } = string.Empty;
    public int RuntimeCommandStartIndex { get; set; }
    public int RuntimeCommandEndIndex { get; set; }
}

public sealed class CanonicalRuntimePlayerCommandLoopExecutionResult
{
    public string RequestedOperation { get; set; } = string.Empty;
    public int RuntimeCommandStartIndex { get; set; }
    public int RuntimeCommandEndIndex { get; set; }
    public int CursorBefore { get; set; }
    public int CursorAfter { get; set; }
    public bool RuntimeExecuted { get; set; }
    public bool RuntimeMutation { get; set; }
    public int ExecutedCommandCount { get; set; }
    public int ProducedSnapshotCount { get; set; }
    public int EventCount { get; set; }
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
    public bool Success { get; set; }
    public CanonicalRuntimePlayerCommandLoopSession Session { get; set; } = new();
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopStep> Steps { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopStep>();
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot> Snapshots { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopSnapshot>();
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public sealed class CanonicalRuntimePlayerCommandLoopResult
{
    public string GoalId { get; set; } =
        "goal_136_canonical_runtime_player_command_loop_execution_matrix";
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public bool PlayerCommandLoopPassed { get; set; }
    public int PlayerCommandCount { get; set; }
    public int PlayerSnapshotCount { get; set; }
    public int RuntimeEventCount { get; set; }
    public bool StateHashChainPresent { get; set; }
    public bool AllRequiredCategoriesPresent { get; set; }
    public bool SelectedCandidateExecutedByRuntime { get; set; }
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
    public bool RuntimePrimitiveMissing { get; set; }
    public IReadOnlyList<string> MissingRuntimePrimitives { get; set; } = new List<string>();
    public IReadOnlyList<string> RequiredCategories { get; set; } = new List<string>();
    public IReadOnlyList<string> MissingCategories { get; set; } = new List<string>();
    public CanonicalRuntimePlayerCommandLoopInput Inputs { get; set; } = new();
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopStep> Steps { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopStep>();
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopSnapshot> Snapshots { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopSnapshot>();
    public IReadOnlyList<string> StateHashChain { get; set; } = new List<string>();
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public interface ICanonicalRuntimePlayerCommandLoopService
{
    CanonicalRuntimePlayerCommandLoopSession BeginSession(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopRequest request);

    CanonicalRuntimePlayerCommandLoopExecutionResult ExecuteRange(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopSession session,
        CanonicalRuntimePlayerCommandLoopExecutionRequest request);

    CanonicalRuntimePlayerCommandLoopResult Execute(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopRequest request);
}
