using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactProductionArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? BatchArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactRecord> ProducedArtifacts { get; init; } = Array.Empty<GeneratedArtifactRecord>();
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
    public IReadOnlyList<GeneratorPlanDraftArtifactWorklistItem> Worklist { get; init; } = Array.Empty<GeneratorPlanDraftArtifactWorklistItem>();
}

public sealed record GeneratorPlanDraftArtifactWorklistItem
{
    public string ArtifactId { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public bool RequiresHumanApproval { get; init; }
    public string RepairRequestId { get; init; } = string.Empty;
}
