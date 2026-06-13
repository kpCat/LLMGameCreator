using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed record AtlasRegistryPipelineRequest
{
    public AtlasRegistryPreviewRequest PreviewRequest { get; init; } = new();
    public bool PersistArtifacts { get; init; }
    public string ResultArtifactId { get; init; } = AtlasRegistryPreviewArtifactIds.ResultArtifactId;
    public string MarkdownArtifactId { get; init; } = AtlasRegistryPreviewArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = AtlasRegistryPreviewArtifactIds.GeneratedBy;
}

public sealed record AtlasRegistryPipelineRunResult
{
    public AtlasRegistryPreviewResult PreviewResult { get; init; } = new();
    public AtlasRegistryImportResult ImportResult => PreviewResult.ImportResult;
    public string MarkdownReport => PreviewResult.MarkdownReport;
    public IReadOnlyList<string> WrittenFiles => PreviewResult.WrittenFiles;
    public GeneratedArtifactRecord? ResultArtifact { get; init; }
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
    public bool PersistedArtifacts { get; init; }
}
