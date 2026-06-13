using LLMGameCreator.Application.Design.Atlas;
using LLMGameCreator.Infrastructure.Storage;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class AtlasRegistryPreviewArtifactServiceTests
{
    [Fact]
    public async Task CaptureAsyncSavesPreviewAndMarkdownArtifacts()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
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
        Assert.Contains("\"warningCount\": 0", storedPreview.MetadataJson);
        Assert.Contains("\"writtenFiles\": []", storedPreview.MetadataJson);

        var storedMarkdown = await database.GetGeneratedArtifactByIdAsync(AtlasRegistryPreviewArtifactIds.MarkdownArtifactId, CancellationToken.None);
        Assert.NotNull(storedMarkdown);
        Assert.Equal(AtlasRegistryPreviewArtifactIds.MarkdownArtifactKind, storedMarkdown.Kind);
        Assert.Contains("# Atlas Registry Import Report", storedMarkdown.Json);
    }

    [Fact]
    public async Task CaptureAsyncMapsAtlasDiagnosticsToValidationResults()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRootWithUnknownReference(temp.Path);
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
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateAtlasRootWithInvalidJson(temp.Path);
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
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
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

    [Fact]
    public async Task CaptureAsyncIsIdempotentForSameArtifactIds()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRootWithUnknownReference(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var request = new AtlasRegistryPreviewArtifactRequest
        {
            PreviewRequest = new AtlasRegistryPreviewRequest
            {
                RepositoryRootOrAtlasRoot = temp.Path
            }
        };

        await service.CaptureAsync(request, CancellationToken.None);
        await service.CaptureAsync(request, CancellationToken.None);

        var artifacts = await database.ListGeneratedArtifactsAsync(CancellationToken.None);
        Assert.Equal(2, artifacts.Count);
        var validationResults = await database.ListValidationResultsByArtifactAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, CancellationToken.None);
        Assert.Single(validationResults);
    }

    [Fact]
    public async Task CaptureAsyncCanUseCustomArtifactIds()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var result = await service.CaptureAsync(
            new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = new AtlasRegistryPreviewRequest
                {
                    RepositoryRootOrAtlasRoot = temp.Path
                },
                ResultArtifactId = "artifact/atlas/custom-result",
                MarkdownArtifactId = "artifact/atlas/custom-markdown",
                GeneratedBy = "test"
            },
            CancellationToken.None);

        Assert.Equal("artifact/atlas/custom-result", result.ResultArtifact.Id);
        Assert.Equal("artifact/atlas/custom-markdown", result.MarkdownArtifact?.Id);
        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync("artifact/atlas/custom-result", CancellationToken.None));
        Assert.NotNull(await database.GetGeneratedArtifactByIdAsync("artifact/atlas/custom-markdown", CancellationToken.None));
    }

    [Fact]
    public async Task CaptureAsyncDoesNotWriteReportFilesUnlessRequested()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
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

        Assert.Empty(result.PreviewResult.WrittenFiles);
        Assert.False(File.Exists(Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_report.md")));
    }

    [Fact]
    public async Task CaptureAsyncWritesReportFilesWhenRequested()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
        var database = await CreateInitializedDatabaseAsync(temp.Path);
        var service = new AtlasRegistryPreviewArtifactService(new AtlasRegistryPreviewService(), database);

        var result = await service.CaptureAsync(
            new AtlasRegistryPreviewArtifactRequest
            {
                PreviewRequest = new AtlasRegistryPreviewRequest
                {
                    RepositoryRootOrAtlasRoot = temp.Path,
                    WriteReportFiles = true
                }
            },
            CancellationToken.None);

        Assert.Equal(2, result.PreviewResult.WrittenFiles.Count);
        Assert.True(File.Exists(Path.Combine(temp.Path, ".llmgc", "atlas", "atlas_registry_import_report.md")));
        Assert.Contains("atlas_registry_import_report.md", result.ResultArtifact.MetadataJson);
    }

    [Fact]
    public async Task CaptureAsyncDoesNotMapInfoDiagnosticsToValidationResults()
    {
        using var temp = new AtlasTempDirectory();
        AtlasTestFixture.CreateCompleteAtlasRoot(temp.Path);
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

        Assert.Contains(result.PreviewResult.ImportResult.Diagnostics, diagnostic => diagnostic.Severity == AtlasDiagnosticSeverity.Info);
        Assert.Empty(result.ValidationResults);
        Assert.Empty(await database.ListValidationResultsByArtifactAsync(AtlasRegistryPreviewArtifactIds.ResultArtifactId, CancellationToken.None));
    }

    internal static async Task<SqliteDesignDatabase> CreateInitializedDatabaseAsync(string root)
    {
        var database = new SqliteDesignDatabase();
        await database.InitializeAsync(Path.Combine(root, ".llmgc", "design.db"), CancellationToken.None);
        return database;
    }
}
