using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanDraftArtifactQueueArtifactRequest
{
    public GeneratorPlanPreviewRequest PreviewRequest { get; init; } = new();
    public GeneratorPlanDraftArtifactQueueRequest QueueRequest { get; init; } = new();
    public string ResultArtifactId { get; init; } = GeneratorPlanDraftArtifactQueueArtifactIds.ResultArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftArtifactQueueArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftArtifactQueueArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftArtifactQueueArtifactSaveRequest
{
    public string ResultArtifactId { get; init; } = GeneratorPlanDraftArtifactQueueArtifactIds.ResultArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanDraftArtifactQueueArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanDraftArtifactQueueArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanDraftArtifactQueueArtifactResult
{
    public GeneratorPlanDraftArtifactQueueResult QueueResult { get; init; } = new();
    public GeneratedArtifactRecord ResultArtifact { get; init; } = GeneratorPlanDraftArtifactQueueArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanDraftArtifactQueueArtifactIds
{
    public const string GeneratedBy = "generator_plan_draft_artifact_queue_service";
    public const string ResultArtifactId = "artifact/generator_plan_draft_artifact_queue/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_draft_artifact_queue_markdown/latest";
    public const string ResultArtifactKind = "generator_plan.draft_artifact_queue";
    public const string MarkdownArtifactKind = "generator_plan.draft_artifact_queue_markdown_report";
    public const string ResultArtifactPath = ".llmgc/generator-plans/generator_plan_draft_artifact_queue_result.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_draft_artifact_queue_report.md";
}
