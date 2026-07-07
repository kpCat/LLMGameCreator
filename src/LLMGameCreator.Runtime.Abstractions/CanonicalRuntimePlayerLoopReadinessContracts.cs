namespace LLMGameCreator.Runtime.Abstractions;

public sealed class CanonicalRuntimePlayerLoopReadinessRequest
{
    public string TranscriptPath { get; set; } = string.Empty;
    public string StateSummaryPath { get; set; } = string.Empty;
    public string DashboardPath { get; set; } = string.Empty;
}

public sealed class CanonicalRuntimePlayerLoopStep
{
    public int Index { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int? SourceEventIndex { get; set; }
    public int? SourceCommandIndex { get; set; }
    public string SourceStepId { get; set; } = string.Empty;
    public string SourceEventType { get; set; } = string.Empty;
    public string SourceTargetId { get; set; } = string.Empty;
    public string FeatureModuleHint { get; set; } = string.Empty;
    public string RuntimePrimitiveHint { get; set; } = string.Empty;
    public string StateHashBefore { get; set; } = string.Empty;
    public string StateHashAfter { get; set; } = string.Empty;
    public bool CanonicalRuntimeAuthority { get; set; } = true;
}

public sealed class CanonicalRuntimePlayerAdapterContract
{
    public string ContractId { get; set; } = "canonical_runtime_player_adapter_contract_v1";
    public string PlayerAdapterId { get; set; } = "player_adapter.canonical_runtime_step_plan";
    public string CandidateId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string PackageTitle { get; set; } = string.Empty;
    public string TranscriptPath { get; set; } = string.Empty;
    public string StateSummaryPath { get; set; } = string.Empty;
    public bool CanonicalRuntimeSource { get; set; } = true;
    public bool UnityGameplayTruth { get; set; }
    public bool ProjectionOnly { get; set; }
    public bool GameplayExecutedByPlayerAdapter { get; set; }
    public IReadOnlyList<string> RequiredStepCategories { get; set; } = new List<string>();
    public IReadOnlyList<string> FeatureModuleCoverageHints { get; set; } = new List<string>();
}

public sealed class CanonicalRuntimePlayerLoopReadinessResult
{
    public string GoalId { get; set; } =
        "goal_135_canonical_runtime_playable_player_loop_readiness";
    public string CandidateId { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public bool CanonicalRuntimeSource { get; set; } = true;
    public bool UnityGameplayTruth { get; set; }
    public bool ProjectionOnly { get; set; }
    public bool PlayerAdapterContractPresent { get; set; }
    public bool PlayerLoopPlanPresent { get; set; }
    public bool RequiredStepCategoriesPresent { get; set; }
    public bool SaveLoadReplayStillReferenced { get; set; }
    public bool SelectedCandidateExecutedByRuntime { get; set; }
    public int PlayerLoopStepCount { get; set; }
    public IReadOnlyList<string> RequiredStepCategories { get; set; } = new List<string>();
    public IReadOnlyList<string> MissingStepCategories { get; set; } = new List<string>();
    public IReadOnlyList<CanonicalRuntimePlayerLoopStep> Steps { get; set; } =
        new List<CanonicalRuntimePlayerLoopStep>();
    public CanonicalRuntimePlayerAdapterContract PlayerAdapterContract { get; set; } = new();
    public IReadOnlyList<string> FeatureModuleCoverageHints { get; set; } = new List<string>();
    public IReadOnlyList<string> Diagnostics { get; set; } = new List<string>();
}

public interface ICanonicalRuntimePlayerLoopReadinessService
{
    CanonicalRuntimePlayerLoopReadinessResult Build(
        IReadOnlyList<CanonicalRuntimeSelectedCandidateEvent> transcript,
        CanonicalRuntimeSelectedCandidateStateSummary stateSummary,
        CanonicalRuntimePlayerLoopReadinessRequest request,
        bool saveLoadReplayStillReferenced,
        bool selectedCandidateExecutedByRuntime);
}
