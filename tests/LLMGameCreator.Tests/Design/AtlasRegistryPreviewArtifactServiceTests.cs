using LLMGameCreator.Application.Design.Atlas;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryPreviewArtifactServiceTests
{
    [Fact]
    public async Task CaptureAsyncSavesPreviewAndMarkdownArtifacts()
    {
        using var temp = new TempDirectory();
        CreateMinimalAtlasRoot(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var result = await service.CaptureAsync(
            new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = new AtlasRegistryPreviewRequest
                {
                    RepositoryRootOrAtlasRoot = temp.Path
                }
            },
            CancellationToken.None);

        Assert.True(result.PreviewResult.Ok);
        Assert.Equal("valid", result.ResultArtifact.ValidationState);
        Assert.NotNull(result.MarkdownArtifact);
        Assert.Empty(result.ValidationResults);

        var artifacts = await database.ListGeneratedArtifactsAsync(CancellationToken.None);
        Assert.Contains(artifacts, artifact => artifact.Id == AtlasRegistryPreviewArtifactIds.ResultArtifactId);
        Assert.Contains(artifacts, artifact => artifact.Id == AtlasRegistryPreviewArtifactIds.MarkdownArtifactId);

        var storedPreview = await database.GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, CancellationToken.None);
        Assert.NotNull(storedPreview);
        Assert.Equal(AtlasRegistryPreviewArtifactIds.ResultArtifactKind, storedPreview.Kind);
        Assert.Contains("capability_atlas/v1", storedPreview.Json);
        Assert.Contains("\"errorCount\": 0", storedPreview.MetadataJson);

        var storedMarkdown = await database.GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.MarkdownArtifactId, CancellationToken.None);
        Assert.NotNull(storedMarkdown);
        Assert.Equal(AtlasRegistryPreviewArtifactIds.MarkdownArtifactKind, storedMarkdown.Kind);
        Assert.Contains("# Atlas Registry Import Report", storedMarkdown.Json);
    }

    [Fact]
    public async Task CaptureAsyncMapsAtlasDiagnosticsToValidationResults()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateMinimalAtlasRoot(temp.Path);
        await File.WriteAllTextAsync(Path.Combine(atlasRoot, "capability_atlas.json"), """
        {
          "schema_version": "0.1",
          "id": "capability_atlas/v1",
          "title": "Capability Atlas",
          "purpose": "References a missing bundle.",
          "required_feature_bundles": [
            "feature_bundle/missing/v1"
          ]
        }
        """, CancellationToken.None);

        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var result = await service.CaptureAsync(
            new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = new AtlasRegistryPreviewRequest
                {
                    RepositoryRootOrAtlasRoot = temp.Path
                }
            },
            CancellationToken.None);

        Assert.True(result.PreviewResult.Ok);
        Assert.Equal("warnings", result.ResultArtifact.ValidationState);
        Assert.Contains(result.ValidationResults, validation => validation.Code == AtlasDiagnosticCodes.ReferenceUnknown);

        var validationResults = await database.ListValidationResultsByArtifactAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, CancellationToken.None);
        Assert.Contains(validationResults, validation =>
            validation.Severity == AtlasDiagnosticSeverity.Warning &&
            validation.Code == AtlasDiagnosticCodes.ReferenceUnknown &&
            validation.Message.Contains("feature_bundle/missing/v1"));
    }

    [Fact]
    public async Task CaptureAsyncSavesInvalidStateWhenAtlasHasErrors()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateMinimalAtlasRoot(temp.Path);
        await File.WriteAllTextAsync(Path.Combine(atlasRoot, "capability_atlas.json"), "{ invalid json", CancellationToken.None);

        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var result = await service.CaptureAsync(
            new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = new AtlasRegistryPreviewRequest
                {
                    RepositoryRootOrAtlasRoot = temp.Path
                }
            },
            CancellationToken.None);

        Assert.False(result.PreviewResult.Ok);
        Assert.Equal("invalid", result.ResultArtifact.ValidationState);
        Assert.Contains(result.ValidationResults, validation =>
            validation.Severity == AtlasDiagnosticSeverity.Error &&
            validation.Code == AtlasDiagnosticCodes.InvalidJson);

        var storedPreview = await database.GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, CancellationToken.None);
        Assert.NotNull(storedPreview);
        Assert.Equal("invalid", storedPreview.ValidationState);
    }

    [Fact]
    public async Task CaptureAsyncSkipsMarkdownArtifactWhenMarkdownRenderingIsDisabled()
    {
        using var temp = new TempDirectory();
        CreateMinimalAtlasRoot(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var result = await service.CaptureAsync(
            new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = new AtlasRegistryPreviewRequest
                {
                    RepositoryRootOrAtlasRoot = temp.Path,
                    RenderMarkdown = false
                }
            },
            CancellationToken.None);

        Assert.True(result.PreviewResult.Ok);
        Assert.Null(result.MarkdownArtifact);

        var artifacts = await database.ListGeneratedArtifactsAsync(CancellationToken.None);
        Assert.Contains(artifacts, artifact => artifact.Id == AtlasRegistryPreviewArtifactIds.ResultArtifactId);
        Assert.DoesNotContain(artifacts, artifact => artifact.Id == AtlasRegistryPreviewArtifactIds.MarkdownArtifactId);
    }

    private static async Task<SqliteDesignDatabase> CreateInitializedDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
    }

    private static string CreateMinimalAtlasRoot(string root)
    {
        var atlasRoot = Path.Combine(root, "generator-library", "atlas");
        Directory.CreateDirectory(atlasRoot);
        Directory.CreateDirectory(Path.Combine(atlasRoot, "examples"));
        File.WriteAllText(Path.Combine(atlasRoot, "ATLAS_INDEX.md"), "# Test Atlas Index");

        File.WriteAllText(Path.Combine(atlasRoot, "capability_atlas.json"), """
        {
          "schema_version": "0.1",
          "id": "capability_atlas/v1",
          "title": "Capability Atlas",
          "purpose": "Test capability atlas."
        }
        """);

        File.WriteAllText(Path.Combine(atlasRoot, "reference_profiles.json"), """
        {
          "schema_version": "0.1",
          "id": "reference_profiles/v1",
          "title": "Reference Profiles",
          "purpose": "Test reference profiles.",
          "profiles": [
            {
              "id": "profile/test/v1",
              "title": "Test Profile",
              "purpose": "Test profile."
            }
          ]
        }
        """);

        File.WriteAllText(Path.Combine(atlasRoot, "feature_bundle_map.json"), """
        {
          "schema_version": "0.1",
          "id": "feature_bundle_map/v1",
          "title": "Feature Bundle Map",
          "purpose": "Test bundles.",
          "feature_bundles": [
            {
              "id": "feature_bundle/test/v1",
              "title": "Test Bundle",
              "purpose": "Test bundle."
            }
          ]
        }
        """);

        File.WriteAllText(Path.Combine(atlasRoot, "examples", "test.example.json"), """
        {
          "schema_version": "0.1",
          "example_id": "example/test/v1",
          "title": "Test Example",
          "purpose": "Test example.",
          "source_profile": {
            "id": "profile/test/v1"
          },
          "selected_feature_bundles": [
            "feature_bundle/test/v1"
          ],
          "target_artifacts": [
            "game_profile_v1"
          ],
          "steps": [
            {
              "id": "step/profile_summary",
              "title": "Normalize profile"
            }
          ]
        }
        """);

        return atlasRoot;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
