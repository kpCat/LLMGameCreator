using LLMGameCreator.Application.Design.Atlas;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryPipelineServiceTests
{
    [Fact]
    public async Task PipelineCanRunPreviewOnly()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var service = new AtlasRegistryPipelineService(new AtlasRegistryPreviewService());

        var result = await service.RunPreviewAsync(new AtlasRegistryPipelineRequest
        {
            PreviewRequest = new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path
            }
        }, CancellationToken.None);

        Assert.True(result.PreviewResult.Ok);
        Assert.False(result.PersistedArtifacts);
        Assert.Null(result.ResultArtifact);
        Assert.Empty(result.ValidationResults);
    }

    [Fact]
    public async Task PipelineCanRunPreviewAndPersistArtifacts()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var database = await AtlasRegistryPreviewArtifactServiceTests.CreateInitializedDatabaseAsync(temp.Path);
        var preview = new AtlasRegistryPreviewService();
        var artifact = new AtlasRegistryPreviewArtifactService(preview, database);
        var service = new AtlasRegistryPipelineService(preview, artifact);

        var result = await service.RunPreviewAsync(new AtlasRegistryPipelineRequest
        {
            PersistArtifacts = true,
            PreviewRequest = new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path
            }
        }, CancellationToken.None);

        Assert.True(result.PersistedArtifacts);
        Assert.NotNull(result.ResultArtifact);
        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, CancellationToken.None));
    }

    [Fact]
    public async Task PipelineCanRunPreviewAndWriteReportFiles()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var service = new AtlasRegistryPipelineService(new AtlasRegistryPreviewService());

        var result = await service.RunPreviewAsync(new AtlasRegistryPipelineRequest
        {
            PreviewRequest = new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path,
                WriteReportFiles = true
            }
        }, CancellationToken.None);

        Assert.Equal(2, result.WrittenFiles.Count);
        Assert.True(File.Exists(Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_report.md")));
        Assert.True(File.Exists(Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_result.json")));
    }
}
