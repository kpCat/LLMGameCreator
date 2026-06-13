using LLMGameCreator.Application.Design;

namespace LLMGameCreator.Application.Design.Atlas;

public sealed record AtlasRegistryPreviewArtifactRequest
{
    public AtlasRegistryPreviewRequest PreviewRequest { get; init; } = new();
    public string ResultArtifactId { get; init; } = AtlasRegistryPreviewArtifactIds.ResultArtifactId;
    public string MarkdownArtifactId { get; init; } = AtlasRegistryPreviewArtifactIds.MarkdownArtifactId;
    public string GeneratedBy { get; init; } = AtlasRegistryPreviewArtifactIds.GeneratedBy;
}

public sealed record AtlasRegistryPreviewArtifactResult
{
    public AtlasRegistryPreviewResult PreviewResult { get; init; } = new();
    public GeneratedArtifactRecord ResultArtifact { get; init; } = AtlasRegistryPreviewArtifactService.EmptyArtifact;
    public GeneratedArtifactRecord? MarkdownArtifact { get; init; }
    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults { get; init; } = Array.Empty<GeneratedArtifactValidationResultRecord>();
}

public static class AtlasRegistryPreviewArtifactIds
{
    public const string GeneratedBy = "atlas_registry_preview_service";
    public const string ResultArtifactId = "artifact/atlas_registry_preview/latest";
    public const string MarkdownArtifactId = "artifact/atlas_registry_preview_markdown/latest";
    public const string ResultArtifactKind = "atlas.registry.preview";
    public const string MarkdownArtifactKind = "atlas.registry.markdown_report";
    public const string ResultArtifactPath = ".llmgc/atlas/atlas_registry_import_result.json";
    public const string MarkdownArtifactPath = ".llmgc/atlas/atlas_registry_import_report.md";
}
