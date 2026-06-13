using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryPreviewServiceTests
{
    [Fact]
    public async Task PreviewAsyncReturnsImportResultAndMarkdownWithoutWritingByDefault()
    {
        using var temp = new TempDirectory();
        var atlasRoot = CreateMinimalAtlasRoot(temp.Path);

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path
            },
            CancellationToken.None);

        Assert.True(result.Ok);
        Assert.NotEqual(default, result.GeneratedAtUtc);
        Assert.Equal(atlasRoot, result.ImportResult.AtlasRoot);
        Assert.Contains("# Atlas Registry Import Report", result.MarkdownReport);
        Assert.Contains("capability_atlas/v1", result.MarkdownReport);
        Assert.Empty(result.WrittenFiles);
        Assert.False(Directory.Exists(Path.Combine(temp.Path, ".llmgc", "atlas")));
    }

    [Fact]
    public async Task PreviewAsyncCanWriteMarkdownAndJsonSnapshot()
    {
        using var temp = new TempDirectory();
        CreateMinimalAtlasRoot(temp.Path);

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path,
                WriteReportFiles = true
            },
            CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Equal(2, result.WrittenFiles.Count);

        var markdownPath = Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_report.md");
        var jsonPath = Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_result.json");

        Assert.Contains(markdownPath, result.WrittenFiles);
        Assert.Contains(jsonPath, result.WrittenFiles);
        Assert.True(File.Exists(markdownPath));
        Assert.True(File.Exists(jsonPath));
        Assert.Contains("# Atlas Registry Import Report", await File.ReadAllTextAsync(markdownPath, CancellationToken.None));
        Assert.Contains("\"ok\": true", (await File.ReadAllTextAsync(jsonPath, CancellationToken.None)).ToLowerInvariant());
        Assert.Contains("capability_atlas/v1", await File.ReadAllTextAsync(jsonPath, CancellationToken.None));
    }

    [Fact]
    public async Task PreviewAsyncCanWriteToExplicitOutputRoot()
    {
        using var temp = new TempDirectory();
        CreateMinimalAtlasRoot(temp.Path);
        var outputRoot = Path.Combine(temp.Path, "custom-output");

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = Path.Combine(temp.Path, "generator-library", "atlas"),
                WriteReportFiles = true,
                ReportOutputRoot = outputRoot
            },
            CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Contains(Path.Combine(outputRoot, "atlas_registry_import_report.md"), result.WrittenFiles);
        Assert.Contains(Path.Combine(outputRoot, "atlas_registry_import_result.json"), result.WrittenFiles);
        Assert.True(File.Exists(Path.Combine(outputRoot, "atlas_registry_import_report.md")));
        Assert.True(File.Exists(Path.Combine(outputRoot, "atlas_registry_import_result.json")));
    }

    [Fact]
    public async Task PreviewAsyncCanSkipMarkdownRendering()
    {
        using var temp = new TempDirectory();
        CreateMinimalAtlasRoot(temp.Path);

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path,
                RenderMarkdown = false
            },
            CancellationToken.None);

        Assert.True(result.Ok);
        Assert.Empty(result.MarkdownReport);
        Assert.Empty(result.WrittenFiles);
    }

    [Fact]
    public async Task PreviewAsyncRequiresRootInput()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest(),
            CancellationToken.None));
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
