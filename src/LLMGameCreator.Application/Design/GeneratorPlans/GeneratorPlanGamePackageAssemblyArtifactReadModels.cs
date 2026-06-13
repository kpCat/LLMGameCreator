using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed record GeneratorPlanGamePackageAssemblyArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? AssemblyArtifact { get; init; }
    public GeneratedArtifactRecord? PackageDraftArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}
