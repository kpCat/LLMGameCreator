using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveReviewSnapshotTests
{
    [Fact]
    public async Task UnityArchiveReviewSnapshotMissingArchiveReturnsDiagnostic()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, ".llmgc", "unity-archive");

        var result = await CreateReviewService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewSnapshotReadiness.MissingArchive, result.Report.Readiness);
        Assert.Contains(result.Report.Diagnostics, diagnostic =>
            diagnostic.Code == "unity.archive_review.missing_archive_directory" &&
            diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error);
        Assert.Empty(result.WrittenRelativePaths);
    }

    [Fact]
    public async Task UnityArchiveReviewSnapshotWritesDeterministicJsonAndMarkdown()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var service = CreateReviewService();

        var first = await service.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var firstJson = await File.ReadAllTextAsync(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewJsonRelativePath));
        var firstMarkdown = await File.ReadAllTextAsync(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewMarkdownRelativePath));

        var second = await service.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var secondJson = await File.ReadAllTextAsync(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewJsonRelativePath));
        var secondMarkdown = await File.ReadAllTextAsync(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewMarkdownRelativePath));

        Assert.Equal(firstJson, secondJson);
        Assert.Equal(firstMarkdown, secondMarkdown);
        Assert.Equal(first.Report.SourceFileCount, second.Report.SourceFileCount);
        Assert.DoesNotContain("lastWriteTimeUtc", firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(materialized.OutputDirectoryPath.Replace('\\', '/'), firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(UnityArchiveReviewSnapshotService.ReviewJsonRelativePath, first.WrittenRelativePaths);
        Assert.Contains(UnityArchiveReviewSnapshotService.ReviewMarkdownRelativePath, first.WrittenRelativePaths);
    }

    [Fact]
    public async Task UnityArchiveReviewSnapshotAggregatesFulfillmentAndProviderCounts()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);

        var result = await CreateReviewService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath,
            WriteReviewFiles = false
        });

        Assert.True(result.Report.Validation.ExportValidationPresent);
        Assert.True(result.Report.Providers.ReadinessReportPresent);
        Assert.True(result.Report.Fulfillment.FulfillmentStatePresent);
        Assert.True(result.Report.Fulfillment.InvalidOutputsPresent);
        Assert.True(result.Report.SourceFileCount > 0);
        Assert.Equal(result.Report.Fulfillment.TotalSlotCount,
            result.Report.Fulfillment.MissingCount + result.Report.Fulfillment.AvailableCount + result.Report.Fulfillment.InvalidCount);
    }

    [Fact]
    public async Task UnityArchiveReviewSnapshotReportsMissingRequiredArchiveFile()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        File.Delete(ArchivePath(materialized.OutputDirectoryPath, "production/fulfillment-state.json"));

        var result = await CreateReviewService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath,
            WriteReviewFiles = false
        });

        Assert.Equal(UnityArchiveReviewSnapshotReadiness.MissingArchive, result.Report.Readiness);
        Assert.Contains(result.Report.Diagnostics, diagnostic =>
            diagnostic.Code == "unity.archive_review.missing_required_file" &&
            diagnostic.TargetId == "production/fulfillment-state.json");
    }

    [Fact]
    public async Task UnityArchiveReviewSnapshotCanParseWrittenReviewJson()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);

        await CreateReviewService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(
            ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewJsonRelativePath)));

        Assert.Equal("1", document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.True(document.RootElement.TryGetProperty("readiness", out _));
        Assert.True(document.RootElement.TryGetProperty("validation", out _));
        Assert.True(document.RootElement.TryGetProperty("providers", out _));
        Assert.True(document.RootElement.TryGetProperty("fulfillment", out _));
        Assert.True(document.RootElement.TryGetProperty("sourceFiles", out _));
    }

    private static async Task<UnityArchiveMaterializationResult> MaterializeArchiveAsync(string projectRoot)
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));

        var materialization = new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));

        return await materialization.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        });
    }

    private static UnityArchiveReviewSnapshotService CreateReviewService() => new();

    private static string ArchivePath(string outputDirectory, string relativePath)
    {
        return Path.Combine(outputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
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
