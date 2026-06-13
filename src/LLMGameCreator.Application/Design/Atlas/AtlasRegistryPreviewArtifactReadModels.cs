using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed record AtlasRegistryPreviewArtifactReadResult
{
    public bool Exists { get; init; }
    public GeneratedArtifactRecord? ResultArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}
