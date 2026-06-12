namespace LLMGameCreator.Application.Design;

public sealed record GeneratorPlanPreviewRequest(
    string PlanId,
    string? Title = null,
    bool IncludeWarnings = true);

public sealed record GeneratorPlanPreviewResult(
    GeneratorPlanRecord? Plan,
    IReadOnlyList<GeneratorPlanStepRecord> Steps,
    GeneratedArtifactRecord? Artifact,
    IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults,
    IReadOnlyList<GeneratorPlanValidationIssue> ValidationIssues,
    bool Saved,
    string Message);
