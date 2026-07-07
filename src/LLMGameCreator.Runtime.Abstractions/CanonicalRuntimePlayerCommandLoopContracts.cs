namespace LLMGameCreator.Runtime.Abstractions;

public sealed class CanonicalRuntimePlayerCommandLoopRequest
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
    public string Goal134TranscriptPath { get; set; } = string.Empty;
    public string Goal134StateSummaryPath { get; set; } = string.Empty;
    public string Goal135PlayerLoopPlanPath { get; set; } = string.Empty;
    public string Goal135PlayerAdapterContractPath { get; set; } = string.Empty;
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
}

public sealed class CanonicalRuntimePlayerCommandLoopSnapshot
{
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
    public string DiagnosticSummary { get; set; } = string.Empty;
    public bool ProjectionOnly { get; set; }
    public bool UnityGameplayTruth { get; set; }
    public IReadOnlyList<CanonicalRuntimePlayerCommandLoopRuntimeEvent> RuntimeEvents { get; set; } =
        new List<CanonicalRuntimePlayerCommandLoopRuntimeEvent>();
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
    CanonicalRuntimePlayerCommandLoopResult Execute(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        CanonicalRuntimePlayerCommandLoopRequest request);
}
