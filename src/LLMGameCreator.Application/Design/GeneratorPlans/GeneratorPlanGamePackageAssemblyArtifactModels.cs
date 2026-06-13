using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanGamePackageAssemblyArtifactSaveRequest
{
    public string AssemblyArtifactId { get; init; } = GeneratorPlanGamePackageAssemblyArtifactIds.AssemblyArtifactId;
    public string PackageDraftArtifactId { get; init; } = GeneratorPlanGamePackageAssemblyArtifactIds.PackageDraftArtifactId;
    public string MarkdownArtifactId { get; init; } = GeneratorPlanGamePackageAssemblyArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = GeneratorPlanGamePackageAssemblyArtifactIds.GeneratedBy;
}

public sealed record GeneratorPlanGamePackageAssemblyArtifactResult
{
    public GeneratorPlanGamePackageAssemblyResult AssemblyResult { get; init; } = new();
    public GeneratedArtifactRecord AssemblyArtifact { get; init; } = GeneratorPlanGamePackageAssemblyArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord PackageDraftArtifact { get; init; } = GeneratorPlanGamePackageAssemblyArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class GeneratorPlanGamePackageAssemblyArtifactIds
{
    public const string GeneratedBy = "generator_plan_game_package_assembly";
    public const string AssemblyArtifactId = "artifact/generator_plan_game_package_assembly/latest";
    public const string PackageDraftArtifactId = "artifact/generator_plan_game_package_draft/latest";
    public const string MarkdownArtifactId = "artifact/generator_plan_game_package_assembly_markdown/latest";
    public const string AssemblyArtifactKind = "generator_plan.game_package_assembly";
    public const string PackageDraftArtifactKind = "game_package.draft";
    public const string MarkdownArtifactKind = "generator_plan.game_package_assembly_markdown_report";
    public const string AssemblyArtifactPath = ".llmgc/generator-plans/generator_plan_game_package_assembly_snapshot.json";
    public const string PackageDraftArtifactPath = ".llmgc/generated-artifacts/game_package_draft_package.json";
    public const string MarkdownArtifactPath = ".llmgc/generator-plans/generator_plan_game_package_assembly_report.md";
}
