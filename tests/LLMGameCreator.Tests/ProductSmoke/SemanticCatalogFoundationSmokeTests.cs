using System.Text.Json;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Design.Semantics;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SemanticCatalogFoundationSmokeTests
{
    [Fact]
    public async Task SemanticCatalogFoundationProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var artifactSet = new GeneratorPlanApprovedArtifactSet
        {
            SchemaVersion = "1",
            SnapshotId = "snapshot/product-smoke/semantic-catalog",
            ApprovedArtifacts =
            [
                new GeneratorPlanApprovedArtifact
                {
                    ArtifactId = "artifact/product-smoke/semantic-pack",
                    ArtifactKind = "semantic_pack_v1",
                    ExpectedArtifactContract = "semantic_pack_v1",
                    ContentJson = """
                    {
                      "terms": [
                        { "id": "location/sky_lantern_outpost", "kind": "unknown", "label": "Sky Lantern Outpost" }
                      ],
                      "themes": ["survival", "glass frontier"],
                      "tones": ["mysterious"],
                      "dialogueIntents": ["warn", "bargain"],
                      "relations": [
                        { "source": "location/sky_lantern_outpost", "kind": "has_theme", "target": "theme/survival" }
                      ]
                    }
                    """
                }
            ]
        };

        var catalogService = new SemanticCatalogService();
        var catalog = catalogService.Build(artifactSet);
        var catalogWrite = await catalogService.WriteAsync(projectRoot, catalog);
        var previewService = new SemanticGenerationContextPreviewService();
        var preview = previewService.Build(catalog);
        var previewWrite = await previewService.WriteAsync(projectRoot, preview);

        Assert.True(File.Exists(catalogWrite.CatalogJsonPath));
        Assert.True(File.Exists(catalogWrite.CatalogMarkdownPath));
        Assert.True(File.Exists(previewWrite.PreviewJsonPath));
        Assert.True(File.Exists(previewWrite.PreviewMarkdownPath));
        Assert.Contains(catalog.Terms, term =>
            term.TermId == "theme/glass_frontier" && term.Status == SemanticTermStatuses.Candidate);
        Assert.Contains(catalog.Terms, term =>
            term.TermId == "location/sky_lantern_outpost" && term.Status == SemanticTermStatuses.Candidate);
        Assert.Contains(preview.CandidateTerms, termId => termId == "theme/glass_frontier");
        Assert.Contains("semantic catalog merge", preview.LlmPolicy.DeterministicSteps);

        using var catalogJson = JsonDocument.Parse(await File.ReadAllTextAsync(catalogWrite.CatalogJsonPath));
        using var previewJson = JsonDocument.Parse(await File.ReadAllTextAsync(previewWrite.PreviewJsonPath));
        Assert.Equal("1", catalogJson.RootElement.GetProperty("schemaVersion").GetString());
        Assert.Equal("project-semantic-catalog", catalogJson.RootElement.GetProperty("catalogId").GetString());
        Assert.Equal("semantic-generation-context-preview", previewJson.RootElement.GetProperty("contextId").GetString());
        Assert.DoesNotContain("providerExecution", await File.ReadAllTextAsync(previewWrite.PreviewJsonPath), StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
