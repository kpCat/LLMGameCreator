using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactApprovalArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? StagingArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public GeneratedArtifactRecord? ApprovedArtifactSetArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalWorklistItem> Worklist { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalWorklistItem>();
}

public sealed record GeneratorPlanDraftArtifactApprovalWorklistItem
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public bool RequiresHumanApproval { get; init; }
    public string RepairRequestId { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
}
