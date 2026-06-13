using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryMarkdownReportRendererTests
{
    [Fact]
    public void RenderIncludesSummaryDocumentsExamplesAndDiagnostics()
    {
        var result = new AtlasRegistryImportResult
        {
            Ok = false,
            AtlasRoot = "/repo/generator-library/atlas",
            Documents =
            [
                new AtlasDocumentSummary
                {
                    Path = "capability_atlas.json",
                    Id = "capability_atlas/v1",
                    Title = "Capability Atlas",
                    Loaded = true,
                    TopLevelIds = ["capability_atlas/v1", "capability/test/v1"],
                    ReferencedIds = ["profile/test/v1"]
                }
            ],
            Examples =
            [
                new AtlasExampleSummary
                {
                    Path = "examples/test.example.json",
                    ExampleId = "example/test/v1",
                    Title = "Test Example",
                    SourceProfileId = "profile/test/v1",
                    SelectedFeatureBundles = ["feature_bundle/test/v1"],
                    TargetArtifacts = ["game_profile_v1", "semantic_pack_v1"],
                    StepCount = 3
                }
            ],
            Diagnostics =
            [
                new AtlasDiagnostic
                {
                    Severity = AtlasDiagnosticSeverity.Warning,
                    Code = AtlasDiagnosticCodes.ReferenceUnknown,
                    Path = "capability_atlas.json",
                    Id = "profile/test/v1",
                    Message = "Unknown reference."
                }
            ],
            Summary = new AtlasRegistrySummary
            {
                DocumentCount = 1,
                LoadedDocumentCount = 1,
                ExampleCount = 1,
                UniqueIdCount = 3,
                ErrorCount = 1,
                WarningCount = 1
            }
        };

        var markdown = new AtlasRegistryMarkdownReportRenderer().Render(result);

        Assert.Contains("# Atlas Registry Import Report", markdown);
        Assert.Contains("Status: **FAILED**", markdown);
        Assert.Contains("## Summary", markdown);
        Assert.Contains("| Documents | 1 |", markdown);
        Assert.Contains("## Documents", markdown);
        Assert.Contains("capability_atlas/v1", markdown);
        Assert.Contains("## Examples", markdown);
        Assert.Contains("example/test/v1", markdown);
        Assert.Contains("| examples/test.example.json | example/test/v1 | Test Example | profile/test/v1 | 1 | 2 | 3 |", markdown);
        Assert.Contains("## Diagnostics", markdown);
        Assert.Contains(AtlasDiagnosticCodes.ReferenceUnknown, markdown);
    }

    [Fact]
    public void RenderHandlesEmptySections()
    {
        var result = new AtlasRegistryImportResult
        {
            Ok = true,
            AtlasRoot = "",
            Summary = new AtlasRegistrySummary()
        };

        var markdown = new AtlasRegistryMarkdownReportRenderer().Render(result);

        Assert.Contains("Status: **OK**", markdown);
        Assert.Contains("_No atlas documents were reported._", markdown);
        Assert.Contains("_No atlas examples were reported._", markdown);
        Assert.Contains("_No diagnostics were reported._", markdown);
    }

    [Fact]
    public void RenderEscapesMarkdownTablePipesAndNewlines()
    {
        var result = new AtlasRegistryImportResult
        {
            Ok = false,
            AtlasRoot = "/repo`root",
            Documents =
            [
                new AtlasDocumentSummary
                {
                    Path = "a|b.json",
                    Id = "doc|id",
                    Title = "Title\nLine",
                    Loaded = true
                }
            ],
            Diagnostics =
            [
                new AtlasDiagnostic
                {
                    Severity = AtlasDiagnosticSeverity.Error,
                    Code = "atlas.test",
                    Path = "a|b.json",
                    Id = "id|x",
                    Message = "Line 1\nLine 2"
                }
            ],
            Summary = new AtlasRegistrySummary
            {
                DocumentCount = 1,
                LoadedDocumentCount = 1,
                ErrorCount = 1
            }
        };

        var markdown = new AtlasRegistryMarkdownReportRenderer().Render(result);

        Assert.Contains("`/repo\\`root`", markdown);
        Assert.Contains("a\\|b.json", markdown);
        Assert.Contains("doc\\|id", markdown);
        Assert.Contains("Title<br>Line", markdown);
        Assert.Contains("Line 1<br>Line 2", markdown);
    }
}
