using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveManualImportWorkflowSmokeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task UnityArchiveManualImportWorkflowUiProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var targetPresets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(targetPresets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var materialized = await new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer())).MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = targetPresets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = targetPresets.ListRuntimeModules(),
            GamePackage = CreatePackage()
        });
        await new UnityArchiveReviewSnapshotService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });
        await new UnityArchiveReviewHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = materialized.OutputDirectoryPath
        });

        var presenter = new UnityArchiveReviewPresenter();
        var initial = await presenter.RefreshAsync(projectRoot);
        Assert.Contains(initial.ManualImportSlots, slot => slot.Kind == UnityArchiveManualImportSlotKind.Asset);
        Assert.Contains(initial.ManualImportSlots, slot => slot.Kind is UnityArchiveManualImportSlotKind.Audio or UnityArchiveManualImportSlotKind.Lua);

        var templated = await presenter.CreateManualImportTemplateAsync(
            projectRoot,
            initial.SelectedSnapshotId,
            UnityArchiveManualImportSlotFilter.Missing,
            null);
        var templatePath = Path.Combine(materialized.OutputDirectoryPath, "manual-import", "import-manifest.template.json");
        Assert.True(File.Exists(templatePath));
        Assert.Contains("template created", templated.ManualImportWorkspaceStatus, StringComparison.OrdinalIgnoreCase);

        var slot = Assert.Single(initial.ManualImportSlots, candidate =>
            candidate.ProviderKind == UnityArchiveRequestProviderKind.manual_import &&
            candidate.Kind == UnityArchiveManualImportSlotKind.Asset &&
            candidate.Status == UnityArchiveFulfillmentStatus.missing);
        WriteText(materialized.OutputDirectoryPath, $"manual-import/{slot.SuggestedSourceRelativePath}", "workflow import bytes");
        WriteJson(materialized.OutputDirectoryPath, "manual-import/import-manifest.json", new UnityArchiveManualProviderImportManifest
        {
            Entries =
            [
                new UnityArchiveManualProviderImportManifestEntry
                {
                    SlotId = slot.SlotId,
                    SourceRelativePath = slot.SuggestedSourceRelativePath,
                    ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath
                }
            ]
        });

        var result = await presenter.RunManualImportAsync(
            projectRoot,
            initial.SelectedSnapshotId,
            UnityArchiveManualImportSlotFilter.All,
            slot.SlotId,
            overwriteExisting: false);
        var importedPath = Path.Combine(
            materialized.OutputDirectoryPath,
            slot.ExpectedOutputRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var refreshedSlot = Assert.Single(result.ManualImportSlots, candidate => candidate.SlotId == slot.SlotId);
        using var page = new UnityArchiveReviewPageControl(presenter, new FakeCurrentGamePackageService(projectRoot));

        Assert.Equal("unity_archive_review", page.Id);
        Assert.True(File.Exists(importedPath));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "manual-provider-import-report.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "manual-provider-import-report.md")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "fulfillment-state.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-history-index.json")));
        Assert.True(File.Exists(Path.Combine(materialized.OutputDirectoryPath, "production", "archive-review-comparison.json")));
        Assert.Equal(UnityArchiveFulfillmentStatus.available, refreshedSlot.Status);
        Assert.Contains("Manual Provider Import", result.ManualImportReportMarkdown, StringComparison.Ordinal);
        Assert.Contains("\"readiness\": \"Ready\"", result.ManualImportReportJson, StringComparison.Ordinal);
        Assert.Equal(initial.SelectedSnapshotId, result.SelectedSnapshotId);
    }

    private static GamePackageDefinition CreatePackage() => new()
    {
        Manifest = new GameManifest
        {
            PackageId = "game/manual-import-workspace-smoke",
            Title = "Manual Import Workspace Smoke"
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

    private static void WriteJson<T>(string archiveRoot, string relativePath, T value) =>
        WriteText(archiveRoot, relativePath, JsonSerializer.Serialize(value, JsonOptions));

    private static void WriteText(string archiveRoot, string relativePath, string content)
    {
        var path = Path.Combine(archiveRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    private sealed class FakeCurrentGamePackageService : ICurrentGamePackageService
    {
        public FakeCurrentGamePackageService(string currentFolder) => CurrentFolder = currentFolder;
        public string? CurrentFolder { get; }
        public GamePackageDefinition? CurrentPackage { get; private set; }
        public event EventHandler? CurrentChanged;
        public Task LoadAsync(string projectFolder, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void ReplaceCurrent(GamePackageDefinition package)
        {
            CurrentPackage = package;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
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
