using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveReviewSnapshotSmokeTests
{
    [Fact]
    public async Task UnityArchiveReviewSnapshotProductSmoke()
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

        var review = await new UnityArchiveReviewSnapshotService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Contains(UnityArchiveReviewSnapshotService.ReviewJsonRelativePath, review.WrittenRelativePaths);
        Assert.Contains(UnityArchiveReviewSnapshotService.ReviewMarkdownRelativePath, review.WrittenRelativePaths);
        Assert.True(File.Exists(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewJsonRelativePath)));
        Assert.True(File.Exists(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewMarkdownRelativePath)));

        var reviewJson = await File.ReadAllTextAsync(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewJsonRelativePath));
        using var document = JsonDocument.Parse(reviewJson);

        Assert.Equal("1", document.RootElement.GetProperty("schemaVersion").GetString());
        Assert.True(document.RootElement.TryGetProperty("readiness", out _));
        Assert.True(document.RootElement.GetProperty("sourceFileCount").GetInt32() > 0);
        Assert.DoesNotContain("lastWriteTimeUtc", reviewJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", reviewJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(materialized.OutputDirectoryPath.Replace('\\', '/'), reviewJson, StringComparison.OrdinalIgnoreCase);

        var second = await new UnityArchiveReviewSnapshotService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        Assert.Equal(review.Report.SourceFileCount, second.Report.SourceFileCount);
        Assert.Equal(
            await File.ReadAllTextAsync(ArchivePath(materialized.OutputDirectoryPath, UnityArchiveReviewSnapshotService.ReviewJsonRelativePath)),
            reviewJson);
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
                PackageId = "game/archive-review-smoke",
                Title = "Archive Review Smoke"
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
