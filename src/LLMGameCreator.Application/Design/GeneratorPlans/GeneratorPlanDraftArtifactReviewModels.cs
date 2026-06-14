using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactReviewLoadResult
{
    public bool Exists { get; init; }
    public string Message { get; init; } = string.Empty;
    public GeneratedArtifactRecord? StagingArtifact { get; init; }
    public GeneratedArtifactRecord? ApprovedArtifactSetArtifact { get; init; }
    public GeneratorPlanDraftArtifactStagingSnapshot Snapshot { get; init; } = new();
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public sealed record GeneratorPlanDraftArtifactReviewDecisionRequest
{
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDecision> Decisions { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDecision>();
    public bool RenderMarkdown { get; init; } = true;
    public string GeneratedBy { get; init; } = "artifact_review_ui";
}

public sealed record GeneratorPlanDraftArtifactReviewDecisionResult
{
    public bool Ok { get; init; }
    public string Status { get; init; } = string.Empty;
    public GeneratorPlanDraftArtifactStagingSnapshot Snapshot { get; init; } = new();
    public GeneratedArtifactRecord StagingArtifact { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord ApprovedArtifactSetArtifact { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratorPlanDraftArtifactApprovalDiagnostic> Diagnostics { get; init; } = Array.Empty<GeneratorPlanDraftArtifactApprovalDiagnostic>();
}
