namespace LLMGameCreator.Application.Design;

public sealed record GeneratorPlanPipelineResult(
    GeneratorPlanRecord? Plan,
    GeneratedArtifactRecord? PreviewArtifact,
    GeneratedArtifactRecord? PatchArtifact,
    GamePackagePatchDryRunResult? DryRunResult,
    IReadOnlyList<GeneratorPlanValidationIssue> ValidationIssues,
    bool CanApply,
    string Message);
