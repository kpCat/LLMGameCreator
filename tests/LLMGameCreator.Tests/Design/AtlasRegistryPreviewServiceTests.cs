using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryPreviewServiceTests
{
    [Fact]
    public async Task PreviewAsyncReturnsMarkdownByDefault()
    {
        using var temp = new AtlasTempDirectory();
        var atlasRoot = AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);

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
    }

    [Fact]
    public async Task PreviewAsyncDoesNotWriteFilesUnlessRequested()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path
            },
            CancellationToken.None);

        Assert.Empty(result.WrittenFiles);
        Assert.False(Directory.Exists(Path.Combine(temp.Path, ".llmgc", "atlas")));
    }

    [Fact]
    public async Task PreviewAsyncWritesReportFilesWhenRequested()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path,
                WriteReportFiles = true
            },
            CancellationToken.None);

        var markdownPath = Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_report.md");
        var jsonPath = Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_result.json");

        Assert.Equal(2, result.WrittenFiles.Count);
        Assert.Contains(markdownPath, result.WrittenFiles);
        Assert.Contains(jsonPath, result.WrittenFiles);
        Assert.True(File.Exists(markdownPath));
        Assert.True(File.Exists(jsonPath));
        Assert.Contains("# Atlas Registry Import Report", await File.ReadAllTextAsync(markdownPath, CancellationToken.None));
        Assert.Contains("\"ok\": true", (await File.ReadAllTextAsync(jsonPath, CancellationToken.None)).ToLowerInvariant());
    }

    [Fact]
    public async Task PreviewAsyncSupportsExplicitOutputRoot()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var outputRoot = Path.Combine(temp.Path, "custom-output");

        var result = await new AtlasRegistryPreviewService().PreviewAsync(
            new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = Path.Combine(temp.Path, "generator-library", "atlas"),
                WriteReportFiles = true,
                ReportOutputRoot = outputRoot
            },
            CancellationToken.None);

        Assert.Contains(Path.Combine(outputRoot, "atlas_registry_import_report.md"), result.WrittenFiles);
        Assert.Contains(Path.Combine(outputRoot, "atlas_registry_import_result.json"), result.WrittenFiles);
        Assert.True(File.Exists(Path.Combine(outputRoot, "atlas_registry_import_report.md")));
        Assert.True(File.Exists(Path.Combine(outputRoot, "atlas_registry_import_result.json")));
    }

    [Fact]
    public async Task PreviewAsyncCanSkipMarkdownRendering()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);

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
}
