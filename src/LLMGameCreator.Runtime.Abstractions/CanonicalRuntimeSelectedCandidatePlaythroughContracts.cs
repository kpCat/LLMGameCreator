namespace LLMGameCreator.Runtime.Abstractions;

public sealed class CanonicalRuntimeSelectedCandidatePlaythroughRequest
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
}

public sealed class CanonicalRuntimeSelectedCandidateCommand
{
    public int Index { get; set; }
    public string StepId { get; set; } = string.Empty;
    public string CommandKind { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string InventoryId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public int? Seed { get; set; }
    public bool RuntimeExecuted { get; set; }
    public bool RequiredForGreen { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}

public sealed class CanonicalRuntimeSelectedCandidateEvent
{
    public int EventIndex { get; set; }
    public int CommandIndex { get; set; }
    public string StepId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
}

public sealed class CanonicalRuntimeSelectedCandidateStateSummary
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string PackageTitle { get; set; } = string.Empty;
    public string CurrentMapId { get; set; } = string.Empty;
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public long Tick { get; set; }
    public string InventorySummary { get; set; } = string.Empty;
    public string ResourceSummary { get; set; } = string.Empty;
    public string QuestSummary { get; set; } = string.Empty;
    public string ActiveDialogueSummary { get; set; } = string.Empty;
    public string ActiveEncounterSummary { get; set; } = string.Empty;
    public string FinalStateHash { get; set; } = string.Empty;
    public IReadOnlyList<string> StateHashChain { get; set; } = new List<string>();
}

public sealed class CanonicalRuntimeSelectedCandidateSaveLoadReplayResult
{
    public string SaveStateHash { get; set; } = string.Empty;
    public string LoadStateHash { get; set; } = string.Empty;
    public string ReplayStateHash { get; set; } = string.Empty;
    public bool SaveLoadHashMatch { get; set; }
    public bool ReplayHashMatch { get; set; }
    public bool EventHashChainMatch { get; set; }
    public bool Passed { get; set; }
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public sealed class CanonicalRuntimeSelectedCandidatePlaythroughResult
{
    public string CandidateId { get; set; } = string.Empty;
    public string PackagePath { get; set; } = string.Empty;
    public string HandoffPath { get; set; } = string.Empty;
    public bool CanonicalRuntimeStarted { get; set; }
    public bool SelectedCandidateExecutedByRuntime { get; set; }
    public bool ProjectionOnly { get; set; }
    public bool RuntimePrimitiveMissing { get; set; }
    public IReadOnlyList<string> MissingRuntimePrimitives { get; set; } = new List<string>();
    public int RuntimeCommandCount { get; set; }
    public int RuntimeEventCount { get; set; }
    public bool StateHashChainPresent { get; set; }
    public bool Passed { get; set; }
    public IReadOnlyList<CanonicalRuntimeSelectedCandidateCommand> PlaythroughScript { get; set; } =
        new List<CanonicalRuntimeSelectedCandidateCommand>();
    public IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent> Transcript { get; set; } =
        new List<CanonicalRuntimeSelectedCandidateEvent>();
    public CanonicalRuntimeSelectedCandidateStateSummary StateSummary { get; set; } = new();
    public UnifiedRuntimeSession StateBeforeSave { get; set; } = new();
    public UnifiedRuntimeSession StateAfterLoad { get; set; } = new();
    public IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent> ReplayTranscript { get; set; } =
        new List<CanonicalRuntimeSelectedCandidateEvent>();
    public CanonicalRuntimeSelectedCandidateSaveLoadReplayResult SaveLoadReplay { get; set; } = new();
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public interface ICanonicalRuntimeSelectedCandidatePlaythroughService
{
    CanonicalRuntimeSelectedCandidatePlaythroughResult Execute(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        CanonicalRuntimeSelectedCandidatePlaythroughRequest request);
}
