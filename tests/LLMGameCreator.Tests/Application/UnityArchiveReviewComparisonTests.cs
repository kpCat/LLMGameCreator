using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveReviewComparisonTests
{
    [Fact]
    public async Task UnityArchiveReviewComparisonMissingArchiveReturnsMissingReview()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, ".llmgc", "unity-archive");

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewComparisonReadiness.MissingReview, result.Report.Readiness);
        Assert.Equal(string.Empty, result.Report.CurrentSnapshotId);
        Assert.Equal(string.Empty, result.Report.PreviousSnapshotId);
        Assert.Contains("production/archive-review-comparison.json", result.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonMissingReviewJsonReturnsMissingReview()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, ".llmgc", "unity-archive");
        Directory.CreateDirectory(archivePath);

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewComparisonReadiness.MissingReview, result.Report.Readiness);
        Assert.Contains("production/archive-review-comparison.json", result.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonInvalidReviewJsonReturnsInvalid()
    {
        using var temp = new TempDirectory();
        var archivePath = Path.Combine(temp.Path, "unity-archive");
        Directory.CreateDirectory(Path.Combine(archivePath, "production"));
        await File.WriteAllTextAsync(Path.Combine(archivePath, "production", "archive-review.json"), "not valid json");

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = archivePath
        });

        Assert.Equal(UnityArchiveReviewComparisonReadiness.Invalid, result.Report.Readiness);
        Assert.Contains("production/archive-review-comparison.json", result.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonOneSnapshotReturnsNoPreviousSnapshot()
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

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Equal(UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot, result.Report.Readiness);
        Assert.False(string.IsNullOrWhiteSpace(result.Report.CurrentSnapshotId));
        Assert.Equal(string.Empty, result.Report.PreviousSnapshotId);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonTwoSnapshotsReportsDeltas()
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

        CreateFirstFulfilledAsset(materialized.OutputDirectoryPath);

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.True(result.Report.Readiness is UnityArchiveReviewComparisonReadiness.Ready or UnityArchiveReviewComparisonReadiness.ReadyWithWarnings);
        Assert.False(string.IsNullOrWhiteSpace(result.Report.CurrentSnapshotId));
        Assert.False(string.IsNullOrWhiteSpace(result.Report.PreviousSnapshotId));
        Assert.True(result.Report.Deltas.Count > 0 || result.Report.SourceFileChanges.Count > 0 || result.Report.DiagnosticChanges.Count > 0);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonReportsDiagnosticChanges()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath,
            WriteReviewFiles = true
        });

        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        File.Delete(Path.Combine(materialized.OutputDirectoryPath, "production", "fulfillment-state.json"));

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath,
            WriteReviewFiles = true
        });

        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Contains(result.Report.DiagnosticChanges, dc => dc.Change == "added");
        Assert.Contains(result.Report.DiagnosticChanges, dc => dc.Code.Contains("missing_required_file"));
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonOutputsAreDeterministic()
    {
        using var temp = new TempDirectory();
        var materialized = await MaterializeArchiveAsync(temp.Path);
        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();
        var comparisonService = CreateComparisonService();

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var first = await comparisonService.CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var second = await comparisonService.CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Equal(first.Report.CurrentSnapshotId, second.Report.CurrentSnapshotId);
        Assert.Equal(first.Report.PreviousSnapshotId, second.Report.PreviousSnapshotId);

        var comparisonPath = Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json");
        var firstComparison = await File.ReadAllTextAsync(comparisonPath);
        var secondComparison = await File.ReadAllTextAsync(comparisonPath);
        Assert.Equal(firstComparison, secondComparison);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonContainsNoTimestamps()
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

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var comparisonJson = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json"));

        Assert.DoesNotContain("lastWriteTimeUtc", comparisonJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", comparisonJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DateTime", comparisonJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(materialized.OutputDirectoryPath.Replace('\\', '/'), comparisonJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnityArchiveReviewComparisonCanParseComparisonJson()
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

        await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json")));

        Assert.Equal("1", document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.True(document.RootElement.TryGetProperty("readiness", out _));
        Assert.True(document.RootElement.TryGetProperty("currentSnapshotId", out _));
        Assert.True(document.RootElement.TryGetProperty("previousSnapshotId", out _));
        Assert.True(document.RootElement.TryGetProperty("summary", out _));
    }

    [Fact]
    public async Task ComparisonUsesPreviousSequenceNotHashOrder()
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
        var snapshotIdA = first.Report.SnapshotId;

        CreateFirstFulfilledAsset(materialized.OutputDirectoryPath);

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        var second = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        var snapshotIdB = second.Report.SnapshotId;

        CreateSecondFulfilledAsset(materialized.OutputDirectoryPath);

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        var third = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        var snapshotIdC = third.Report.SnapshotId;

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.True(result.Report.Readiness is UnityArchiveReviewComparisonReadiness.Ready or UnityArchiveReviewComparisonReadiness.ReadyWithWarnings);
        Assert.Equal(snapshotIdC, result.Report.CurrentSnapshotId);
        Assert.Equal(snapshotIdB, result.Report.PreviousSnapshotId);
    }

    [Fact]
    public async Task ComparisonReportsCurrentSnapshotNotIndexed()
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

        CreateFirstFulfilledAsset(materialized.OutputDirectoryPath);

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var indexPath = Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json");
        var indexJson = await File.ReadAllTextAsync(indexPath);
        var corruptedJson = indexJson.Replace("\"snapshotId\":", "\"corruptedId\":");
        await File.WriteAllTextAsync(indexPath, corruptedJson);

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Equal(UnityArchiveReviewComparisonReadiness.NoPreviousSnapshot, result.Report.Readiness);
        Assert.Equal(string.Empty, result.Report.PreviousSnapshotId);
    }

    [Fact]
    public async Task ComparisonMissingSnapshotFileIncludesDiagnostic()
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

        CreateFirstFulfilledAsset(materialized.OutputDirectoryPath);

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        var secondStore = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        CreateSecondFulfilledAsset(materialized.OutputDirectoryPath);

        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        var thirdStore = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var snapshotDir = Path.Combine(materialized.OutputDirectoryPath, "review-history", secondStore.Report.SnapshotId);
        if (Directory.Exists(snapshotDir))
        {
            Directory.Delete(snapshotDir, true);
        }

        var result = await CreateComparisonService().CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Equal(UnityArchiveReviewComparisonReadiness.Blocked, result.Report.Readiness);
        Assert.False(string.IsNullOrWhiteSpace(result.Report.CurrentSnapshotId));
        Assert.Equal(thirdStore.Report.SnapshotId, result.Report.CurrentSnapshotId);
        Assert.True(result.Report.DiagnosticChanges.Any(dc => dc.Code.Contains("missing_snapshot_file")));
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

    private static UnityArchiveReviewComparisonService CreateComparisonService() => new();

    private static void CreateFirstFulfilledAsset(string archiveRoot)
    {
        var productionDir = Path.Combine(archiveRoot, "production");
        Directory.CreateDirectory(productionDir);

        var fulfilledAssetsPath = Path.Combine(productionDir, "fulfilled-assets-index.json");
        var content = """
        {
          "schemaVersion": "1",
          "assets": [
            {
              "slotId": "asset/001",
              "assetId": "asset/test",
              "assetKind": "sprite",
              "expectedOutputRelativePath": "assets/asset-test.png",
              "fileSizeBytes": 100
            }
          ]
        }
        """;
        File.WriteAllText(fulfilledAssetsPath, content);

        var expectedOutputDir = Path.Combine(archiveRoot, "assets");
        Directory.CreateDirectory(expectedOutputDir);
        File.WriteAllText(Path.Combine(expectedOutputDir, "asset-test.png"), "x");
    }

    private static void CreateSecondFulfilledAsset(string archiveRoot)
    {
        var productionDir = Path.Combine(archiveRoot, "production");
        Directory.CreateDirectory(productionDir);

        var fulfilledAssetsPath = Path.Combine(productionDir, "fulfilled-assets-index.json");
        var content = """
        {
          "schemaVersion": "1",
          "assets": [
            {
              "slotId": "asset/001",
              "assetId": "asset/test",
              "assetKind": "sprite",
              "expectedOutputRelativePath": "assets/asset-test.png",
              "fileSizeBytes": 100
            },
            {
              "slotId": "asset/002",
              "assetId": "asset/test2",
              "assetKind": "sprite",
              "expectedOutputRelativePath": "assets/asset-test2.png",
              "fileSizeBytes": 100
            }
          ]
        }
        """;
        File.WriteAllText(fulfilledAssetsPath, content);

        var expectedOutputDir = Path.Combine(archiveRoot, "assets");
        Directory.CreateDirectory(expectedOutputDir);
        File.WriteAllText(Path.Combine(expectedOutputDir, "asset-test2.png"), "x");
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