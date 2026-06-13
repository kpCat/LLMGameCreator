using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryPreviewArtifactReaderTests
{
    [Fact]
    public async Task ReaderReturnsEmptyWhenPreviewArtifactDoesNotExist()
    {
        using var temp = new AtlasTempDirectory();
        var database = await AtlasRegistryPreviewArtifactServiceTests.CreateInitializedDatabaseAsync(temp.Path);

        var result = await new AtlasRegistryPreviewArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.False(result.Exists);
        Assert.Null(result.ResultArtifact);
        Assert.Null(result.MarkdownArtifact);
        Assert.Empty(result.ValidationResults);
    }

    [Fact]
    public async Task ReaderReturnsSavedPreviewMarkdownAndValidationResults()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRootWithUnknownReference(temp.Path);
        var database = await AtlasRegistryPreviewArtifactServiceTests.CreateInitializedDatabaseAsync(temp.Path);
        var capture = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);
        await capture.CaptureAsync(new AtlasRegistryPreviewArtifactRequest
        {
            PreviewRequest = new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path
            }
        }, CancellationToken.None);

        var result = await new AtlasRegistryPreviewArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Exists);
        Assert.NotNull(result.ResultArtifact);
        Assert.NotNull(result.MarkdownArtifact);
        Assert.Contains(result.ValidationResults, validation => validation.Code == AtlasDiagnosticCodes.ReferenceUnknown);
    }

    [Fact]
    public async Task ReaderWorksWhenMarkdownArtifactIsMissing()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var database = await AtlasRegistryPreviewArtifactServiceTests.CreateInitializedDatabaseAsync(temp.Path);
        var capture = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);
        await capture.CaptureAsync(new AtlasRegistryPreviewArtifactRequest
        {
            PreviewRequest = new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path,
                RenderMarkdown = false
            }
        }, CancellationToken.None);

        var result = await new AtlasRegistryPreviewArtifactReader(database).ReadLatestAsync(CancellationToken.None);

        Assert.True(result.Exists);
        Assert.NotNull(result.ResultArtifact);
        Assert.Null(result.MarkdownArtifact);
        Assert.Empty(result.ValidationResults);
    }
}
