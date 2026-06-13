using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftExecutionArtifactRequest
{
    public GeneratorPlanPreviewRequest PreviewRequest { get; init; } = new();
    public GeneratorPlanDraftExecutionRequest DraftRequest { get; init; } = new();
    public string ResultArtifactId { get; init; } = GeneratorPlanDraftExecutionArtifactIds.ResultArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftExecutionArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftExecutionArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftExecutionArtifactResult
{
    public GeneratorPlanDraftExecutionResult DraftResult { get; init; } = new();
    public GeneratedArtifactRecord ResultArtifact { get; init; } = GeneratorPlanDraftExecutionArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanDraftExecutionArtifactIds
{
    public const string GeneratedBy = "generator_plan_draft_execution_service";
    public const string ResultArtifactId = "artifact/generator_plan_draft_execution/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_draft_execution_markdown/latest";
    public const string ResultArtifactKind = "generator_plan.draft_execution";
    public const string MarkdownArtifactKind = "generator_plan.draft_execution_markdown_report";
    public const string ResultArtifactPath = ".llmgc/generator-plans/generator_plan_draft_execution_result.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_draft_execution_report.md";
}
