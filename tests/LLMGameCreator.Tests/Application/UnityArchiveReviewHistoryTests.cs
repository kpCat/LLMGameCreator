using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveReviewHistoryTests
{
    [Fact]
    public async Task UnityArchiveReviewHistoryMissingArchiveReturnsMissingReview()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, "unity-archive");

        var result = await CreateHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewHistoryReadiness.MissingReview, result.Report.Readiness);
        Assert.Equal(string.Empty, result.Report.SnapshotId);
        Assert.Empty(result.WrittenRelativePaths);
    }

    [Fact]
    public async Task UnityArchiveReviewHistoryMissingReviewJsonReturnsMissingReview()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, "unity-archive");
        Directory.CreateDirectory(archivePath);

        var result = await CreateHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewHistoryReadiness.MissingReview, result.Report.Readiness);
        Assert.Equal(string.Empty, result.Report.SnapshotId);
        Assert.Empty(result.WrittenRelativePaths);
    }

    [Fact]
    public async Task UnityArchiveReviewHistoryInvalidReviewJsonReturnsInvalid()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, "unity-archive");
        Directory.CreateDirectory(Path.Combine(archivePath, "production"));
        await File.WriteAllTextAsync(Path.Combine(archivePath, "production", "archive-review.json"), "not valid json");

        var result = await CreateHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewHistoryReadiness.Invalid, result.Report.Readiness);
        Assert.Equal(string.Empty, result.Report.SnapshotId);
        Assert.Empty(result.WrittenRelativePaths);
    }

    [Fact]
    public async Task UnityArchiveReviewHistoryStoresSnapshotWithContentHash()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var result = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.True(result.WrittenRelativePaths.Count > 0);
        Assert.True(result.WrittenRelativePaths.Any(p => p.Contains("review-history/", StringComparison.OrdinalIgnoreCase)));
        Assert.True(result.WrittenRelativePaths.Any(p => p.Contains("archive-review.json", StringComparison.OrdinalIgnoreCase)));
        Assert.False(string.IsNullOrWhiteSpace(result.Report.SnapshotId));
        Assert.Equal(64, result.Report.SnapshotId.Length);
    }

    [Fact]
    public async Task UnityArchiveReviewHistoryStoresHistoryIndex()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var result = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Contains("production/archive-review-history-index.json", result.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnityArchiveReviewHistorySameContentDoesNotDuplicateIndex()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var first = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var second = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var indexPath = Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json");
        var indexJson = await File.ReadAllTextAsync(indexPath);

        Assert.Equal(first.Report.SnapshotId, second.Report.SnapshotId);
        Assert.Single(first.Report.HistoryEntries);
        Assert.Single(second.Report.HistoryEntries);
        Assert.DoesNotContain("lastWriteTimeUtc", indexJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", indexJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnityArchiveReviewHistoryHistoryIndexIsDeterministic()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var firstIndex = await File.ReadAllTextAsync(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json"));

        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var secondIndex = await File.ReadAllTextAsync(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json"));

        Assert.Equal(firstIndex, secondIndex);
    }

    [Fact]
    public async Task UnityArchiveReviewHistoryOutputsAreByteIdentical()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var firstResult = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var firstSnapshot = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "review-history", $"{firstResult.Report.SnapshotId}", "archive-review.json"));
        var firstIndex = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json"));

        var secondResult = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var secondSnapshot = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "review-history", $"{secondResult.Report.SnapshotId}", "archive-review.json"));
        var secondIndex = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json"));

        Assert.Equal(firstSnapshot, secondSnapshot);
        Assert.Equal(firstIndex, secondIndex);
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

    private static UnityArchiveReviewHistoryService CreateHistoryService() => new();

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