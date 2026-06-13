using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactApprovalArtifactRequest
{
    public GeneratorPlanPreviewRequest PreviewRequest { get; init; } = new();
    public GeneratorPlanDraftArtifactApprovalRequest ApprovalRequest { get; init; } = new();
    public string StagingArtifactId { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.StagingArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.MarkdownArtifactId;
    public string ApprovedArtifactSetArtifactId { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftArtifactApprovalArtifactSaveRequest
{
    public string StagingArtifactId { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.StagingArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.MarkdownArtifactId;
    public string ApprovedArtifactSetArtifactId { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.ApprovedArtifactSetArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftArtifactApprovalArtifactResult
{
    public GeneratorPlanDraftArtifactApprovalResult ApprovalResult { get; init; } = new();
    public GeneratedArtifactRecord StagingArtifact { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public GeneratedArtifactRecord ApprovedArtifactSetArtifact { get; init; } = GeneratorPlanDraftArtifactApprovalArtifactService.EmptyArtifact;
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanDraftArtifactApprovalArtifactIds
{
    public const string GeneratedBy = "generator_plan_draft_artifact_approval";
    public const string StagingArtifactId = "artifact/generator_plan_draft_artifact_staging/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_draft_artifact_staging_markdown/latest";
    public const string ApprovedArtifactSetArtifactId = "artifact/generator_plan_approved_artifact_set/latest";
    public const string StagingArtifactKind = "generator_plan.draft_artifact_staging";
    public const string MarkdownArtifactKind = "generator_plan.draft_artifact_staging_markdown_report";
    public const string ApprovedArtifactSetArtifactKind = "generator_plan.approved_artifact_set";
    public const string StagingArtifactPath = ".llmgc/generator-plans/generator_plan_draft_artifact_staging_snapshot.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_draft_artifact_staging_report.md";
    public const string ApprovedArtifactSetArtifactPath = ".llmgc/generator-plans/generator_plan_approved_artifact_set.json";
}
