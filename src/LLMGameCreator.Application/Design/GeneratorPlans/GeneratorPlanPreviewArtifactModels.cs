using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanPreviewArtifactRequest
{
    public GeneratorPlanPreviewRequest PreviewRequest { get; init; } = new();
    public string ResultArtifactId { get; init; } = GeneratorPlanPreviewArtifactIds.ResultArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanPreviewArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanPreviewArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanPreviewArtifactResult
{
    public GeneratorPlanPreviewResult PreviewResult { get; init; } = new();
    public GeneratedArtifactRecord ResultArtifact { get; init; } = GeneratorPlanPreviewArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanPreviewArtifactIds
{
    public const string GeneratedBy = "generator_plan_preview_service";
    public const string ResultArtifactId = "artifact/generator_plan_preview/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_preview_markdown/latest";
    public const string ResultArtifactKind = "generator_plan.preview";
    public const string MarkdownArtifactKind = "generator_plan.markdown_report";
    public const string ResultArtifactPath = ".llmgc/generator-plans/generator_plan_preview_result.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_preview_report.md";
}
