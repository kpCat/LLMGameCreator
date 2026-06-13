using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactProductionArtifactRequest
{
    public GeneratorPlanPreviewRequest PreviewRequest { get; init; } = new();
    public GeneratorPlanDraftArtifactProductionRequest ProductionRequest { get; init; } = new();
    public string BatchArtifactId { get; init; } = GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftArtifactProductionArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftArtifactProductionArtifactSaveRequest
{
    public string BatchArtifactId { get; init; } = GeneratorPlanDraftArtifactProductionArtifactIds.BatchArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftArtifactProductionArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftArtifactProductionArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftArtifactProductionArtifactResult
{
    public GeneratorPlanDraftArtifactProductionResult ProductionResult { get; init; } = new();
    public GeneratedArtifactRecord BatchArtifact { get; init; } = GeneratorPlanDraftArtifactProductionArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactRecord> ProducedArtifacts { get; init; } = Array.Empty<GeneratedArtifactRecord>();
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanDraftArtifactProductionArtifactIds
{
    public const string GeneratedBy = "generator_plan_draft_artifact_producer";
    public const string BatchArtifactId = "artifact/generator_plan_draft_artifact_production/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_draft_artifact_production_markdown/latest";
    public const string BatchArtifactKind = "generator_plan.draft_artifact_production";
    public const string MarkdownArtifactKind = "generator_plan.draft_artifact_production_markdown_report";
    public const string BatchArtifactPath = ".llmgc/generator-plans/generator_plan_draft_artifact_production_result.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_draft_artifact_production_report.md";
}
