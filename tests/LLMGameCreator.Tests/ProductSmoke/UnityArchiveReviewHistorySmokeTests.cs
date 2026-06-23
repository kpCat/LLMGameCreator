using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveReviewHistorySmokeTests
{
    [Fact]
    public async Task UnityArchiveReviewHistoryProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));

        var materialization = CreateMaterializationService();
        var materialized = await materialization.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = CreatePackage()
        });

        var reviewService = new UnityArchiveReviewSnapshotService();
        var historyService = new UnityArchiveReviewHistoryService();
        var comparisonService = new UnityArchiveReviewComparisonService();

        var review = await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Contains(UnityArchiveReviewSnapshotService.ReviewJsonRelativePath, review.WrittenRelativePaths);

        var firstHistory = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.True(firstHistory.WrittenRelativePaths.Any(p => p.Contains("review-history/", StringComparison.OrdinalIgnoreCase)));
        Assert.Contains("production/archive-review-history-index.json", firstHistory.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);

        Assert.True(Directory.Exists(Path.Combine(materialized.OutputDirectoryPath, "review-history", firstHistory.Report.SnapshotId)));

        CreateFirstFulfilledAsset(materialized.OutputDirectoryPath);

        var secondReview = await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var secondHistory = await historyService.StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var comparison = await comparisonService.CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Contains("production/archive-review-comparison.json", comparison.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("production/archive-review-comparison.md", comparison.WrittenRelativePaths, StringComparer.OrdinalIgnoreCase);

        var comparisonJson = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json"));

        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.md")));

        Assert.False(string.IsNullOrWhiteSpace(comparison.Report.CurrentSnapshotId));
        Assert.False(string.IsNullOrWhiteSpace(comparison.Report.PreviousSnapshotId));
        Assert.NotEqual(comparison.Report.CurrentSnapshotId, comparison.Report.PreviousSnapshotId);

        Assert.True(comparison.Report.Deltas.Count > 0 || comparison.Report.SourceFileChanges.Count > 0 || comparison.Report.DiagnosticChanges.Count > 0);

        Assert.DoesNotContain("lastWriteTimeUtc", comparisonJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", comparisonJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(materialized.OutputDirectoryPath.Replace('\\', '/'), comparisonJson, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(comparisonJson);
        Assert.Equal("1", document.RootElement.GetProperty("schemaVersion").GetString());

        var secondComparison = await comparisonService.CompareAsync(new UnityArchiveReviewComparisonRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var secondComparisonJson = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json"));

        Assert.Equal(comparisonJson, secondComparisonJson);
    }

    private static UnityArchiveMaterializationService CreateMaterializationService()
    {
        return new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/archive-review-history-smoke",
                Title = "Archive Review History Smoke"
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Npcs =
                [
                    new GeneratedNpcDefinition
                    {
                        SourceId = "npc/alpha",
                        Name = "Alpha",
                        SceneId = "scene/start"
                    }
                ]
            }
        };
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

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