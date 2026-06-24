using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveManualProviderImportSmokeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task UnityArchiveManualProviderImportProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var targetPresets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(targetPresets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));

        var materialization = new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));
        var materialized = await materialization.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = targetPresets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = targetPresets.ListRuntimeModules(),
            GamePackage = CreatePackage()
        });

        var reviewService = new UnityArchiveReviewSnapshotService();
        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        await new UnityArchiveReviewHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var assetSlots = JsonSerializer.Deserialize<UnityArchiveAssetSlotIndex>(
            await File.ReadAllTextAsync(Path.Combine(materialized.OutputDirectoryPath, "assets", "asset-slots.json")),
            JsonOptions)!;
        var slot = Assert.Single(assetSlots.Slots, candidate =>
            candidate.ProviderKind == UnityArchiveRequestProviderKind.manual_import &&
            candidate.AssetKind == UnityArchiveAssetKind.portrait);
        var extension = Path.GetExtension(slot.ExpectedOutputRelativePath);
        var sourceRelativePath = $"files/imported-portrait{extension}";
        WriteText(materialized.OutputDirectoryPath, $"manual-import/{sourceRelativePath}", "manual portrait bytes");
        WriteJson(materialized.OutputDirectoryPath, "manual-import/import-manifest.json", new UnityArchiveManualProviderImportManifest
        {
            Entries =
            [
                new UnityArchiveManualProviderImportManifestEntry
                {
                    SlotId = slot.SlotId,
                    SourceRelativePath = sourceRelativePath,
                    ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath
                }
            ]
        });

        var result = await new UnityArchiveManualProviderImportService().ImportAsync(
            new UnityArchiveManualProviderImportRequest
            {
                ArchiveDirectoryPath = materialized.OutputDirectoryPath
            });
        var presenterState = await new UnityArchiveReviewPresenter().RefreshAsync(projectRoot);

        Assert.Equal(UnityArchiveManualProviderImportReadiness.Ready, result.Readiness);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Imported, Assert.Single(result.Entries).Status);
        Assert.True(File.Exists(Path.Combine(
            materialized.OutputDirectoryPath,
            slot.ExpectedOutputRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "fulfillment-state.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json")));
        Assert.Contains("Manual Provider Import", presenterState.ManualImportReportMarkdown, StringComparison.Ordinal);
        Assert.Contains("\"readiness\": \"Ready\"", presenterState.ManualImportReportJson, StringComparison.Ordinal);
        Assert.Equal("Loaded", presenterState.SelectedSnapshotStatus);
        Assert.Contains("\"schemaVersion\": \"1\"", presenterState.SelectedSnapshotJson, StringComparison.Ordinal);

        var providerJobs = await File.ReadAllTextAsync(
            Path.Combine(materialized.OutputDirectoryPath, "providers", "manual-import", "jobs.json"));
        Assert.Contains("\"executionEnabled\": false", providerJobs, StringComparison.Ordinal);
        Assert.All(result.Diagnostics, diagnostic =>
            Assert.DoesNotContain("llm", diagnostic.Code, StringComparison.OrdinalIgnoreCase));
    }

    private static GamePackageDefinition CreatePackage() =>
        new()
        {
            Manifest = new GameManifest
            {
                PackageId = "game/manual-provider-import-smoke",
                Title = "Manual Provider Import Smoke"
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Npcs =
                [
                    new GeneratedNpcDefinition
                    {
                        SourceId = "npc/hero",
                        Name = "Hero",
                        SceneId = "scene/start"
                    }
                ]
            }
        };

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static void WriteJson<T>(string archiveRoot, string relativePath, T value)
    {
        WriteText(archiveRoot, relativePath, JsonSerializer.Serialize(value, JsonOptions));
    }

    private static void WriteText(string archiveRoot, string relativePath, string content)
    {
        var path = Path.Combine(archiveRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
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
