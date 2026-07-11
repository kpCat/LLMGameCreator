using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeQualification;

public sealed record ProductLineRuntimeQualificationRequest
{
    public string SessionId { get; init; } = string.Empty;
    public string CandidateId { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CheckpointId { get; init; } = string.Empty;
    public string FinalCheckpointId { get; init; } = string.Empty;
    public string CreatedAtUtc { get; init; } = "2026-07-11T00:00:00Z";
}

public sealed record ProductLineRuntimeQualificationReplayEvidence
{
    public string ReplayKind { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public bool PackageHashValidated { get; init; }
    public bool CandidateValidated { get; init; }
    public bool JournalCorrelationPassed { get; init; }
    public bool StateHashContinuityPassed { get; init; }
    public bool ExpectedStateHashMatched { get; init; }
    public string ExpectedStateHash { get; init; } = string.Empty;
    public string ActualStateHash { get; init; } = string.Empty;
    public int ReplayedActionCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ProductLineRuntimeQualificationResult
{
    public SelectedRuntimeVariantInteractiveSessionStartRequest StartRequest { get; init; } = new();
    public RuntimeInteractiveSession Session { get; init; } = new();
    public IReadOnlyList<SelectedRuntimeVariantActionDescriptor> ActionCatalog { get; init; } = [];
    public SelectedRuntimeVariantInteractiveCheckpoint Checkpoint { get; init; } = new();
    public ProductLineRuntimeQualificationReplayEvidence CheckpointReplay { get; init; } = new();
    public ProductLineRuntimeQualificationReplayEvidence FinalReplay { get; init; } = new();
    public bool InvalidActionStateUnchanged { get; init; }
    public bool ActionDescriptorExecutionBindingPassed { get; init; }
    public string CanonicalActionPlanSignature { get; init; } = string.Empty;
}
