using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.WinForms;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class UnityArchiveReviewPresenterTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task PresenterInitializesWithoutProject()
    {
        var state = await new UnityArchiveReviewPresenter().RefreshAsync(null);

        Assert.Empty(state.ProjectFolder);
        Assert.Empty(state.ArchiveRoot);
        Assert.False(state.CanRefresh);
        Assert.False(state.CanOpenArchiveFolder);
        Assert.Contains("No current project", state.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterReportsMissingArchive()
    {
        using var temp = new TempDirectory();

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal("Missing", state.CurrentReviewReadiness);
        Assert.Equal("Missing", state.ComparisonReadiness);
        Assert.True(state.CanRefresh);
        Assert.False(state.CanOpenArchiveFolder);
        Assert.Contains("not found", state.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterReadsExistingReviewHistoryAndComparisonReports()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal("ReadyWithWarnings", state.CurrentReviewReadiness);
        Assert.Equal("Ready", state.ComparisonReadiness);
        Assert.Equal(2, state.HistorySnapshotCount);
        Assert.Equal("snapshot-b", state.SelectedSnapshotId);
        Assert.Contains("# Current Review", state.CurrentReviewMarkdown);
        Assert.Contains("# Comparison", state.ComparisonMarkdown);
        Assert.Contains("archive-review.json", state.HistoryIndexJson);
        Assert.True(state.CanOpenArchiveFolder);
        Assert.Equal("Archive review, comparison, and history reports loaded.", state.Status);
    }

    [Fact]
    public async Task PresenterHandlesInvalidJsonWithoutThrowing()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchiveRoot(temp.Path);
        var production = Path.Combine(archiveRoot, "production");
        await File.WriteAllTextAsync(Path.Combine(production, "archive-review.json"), "{ invalid json");
        await File.WriteAllTextAsync(Path.Combine(production, "archive-review.md"), "# Markdown survives");

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal("Invalid", state.CurrentReviewReadiness);
        Assert.Equal("{ invalid json", state.CurrentReviewJson);
        Assert.Equal("# Markdown survives", state.CurrentReviewMarkdown);
        Assert.Contains("Invalid JSON", state.Status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PresenterLoadsSelectedHistorySnapshotJson()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path, "snapshot-a");

        Assert.Equal("snapshot-a", state.SelectedSnapshotId);
        Assert.Equal(1, state.SelectedSnapshotSequence);
        Assert.Equal("review-history/snapshot-a/archive-review.json", state.SelectedSnapshotRelativePath);
        Assert.Equal("Loaded", state.SelectedSnapshotStatus);
        Assert.Contains("snapshot-a", state.SelectedSnapshotJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresenterReportsMissingSelectedHistorySnapshotWithoutThrowing()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        var archiveRoot = CreateArchiveRoot(temp.Path);
        File.Delete(Path.Combine(archiveRoot, "review-history", "snapshot-a", "archive-review.json"));

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path, "snapshot-a");

        Assert.Equal("snapshot-a", state.SelectedSnapshotId);
        Assert.Equal("Missing", state.SelectedSnapshotStatus);
        Assert.Empty(state.SelectedSnapshotJson);
        Assert.Contains("review-history/snapshot-a/archive-review.json", state.Status, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresenterDisplaysManualImportReportsWhenPresent()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        var archiveRoot = CreateArchiveRoot(temp.Path);
        File.WriteAllText(
            Path.Combine(archiveRoot, "production", "manual-provider-import-report.json"),
            "{\"schemaVersion\":\"1\",\"readiness\":\"Ready\"}");
        File.WriteAllText(
            Path.Combine(archiveRoot, "production", "manual-provider-import-report.md"),
            "# Manual Import\n\nReady.");

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Contains("\"readiness\":\"Ready\"", state.ManualImportReportJson, StringComparison.Ordinal);
        Assert.Contains("# Manual Import", state.ManualImportReportMarkdown, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresenterReportsIndividuallyMissingFiles()
    {
        using var temp = new TempDirectory();
        CreateArchiveRoot(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Contains("production/archive-review.json", state.Status);
        Assert.Contains("production/archive-review.md", state.Status);
        Assert.Contains("production/archive-review-comparison.json", state.Status);
        Assert.Contains("production/archive-review-history-index.json", state.Status);
    }

    [Fact]
    public async Task PresenterListsManualImportSlotsFromArchiveMetadata()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        CreateManualImportMetadata(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Equal(3, state.ManualImportSlots.Count);
        Assert.Contains(state.ManualImportSlots, slot => slot.Kind == UnityArchiveManualImportSlotKind.Asset);
        Assert.Contains(state.ManualImportSlots, slot => slot.Kind == UnityArchiveManualImportSlotKind.Audio);
        var available = Assert.Single(state.ManualImportSlots, slot => slot.Status == UnityArchiveFulfillmentStatus.available);
        Assert.True(available.FileExists);
        Assert.True(available.FileSizeBytes > 0);
        Assert.Equal(64, available.ContentSha256.Length);
        Assert.True(state.CanCreateManualImportTemplate);
        Assert.True(state.CanRunManualImport);
    }

    [Fact]
    public async Task PresenterReportsMissingSlotMetadataWithoutThrowing()
    {
        using var temp = new TempDirectory();
        CreateArchiveRoot(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path);

        Assert.Empty(state.ManualImportSlots);
        Assert.Contains("slot metadata", state.ManualImportWorkspaceStatus, StringComparison.OrdinalIgnoreCase);
        Assert.False(state.CanCreateManualImportTemplate);
    }

    [Fact]
    public async Task SlotFilterShowsMissingAvailableInvalid()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        CreateManualImportMetadata(temp.Path);
        var presenter = new UnityArchiveReviewPresenter();
        var state = await presenter.RefreshAsync(temp.Path);

        var missing = presenter.ApplyManualImportFilter(state, UnityArchiveManualImportSlotFilter.Missing);
        var available = presenter.ApplyManualImportFilter(state, UnityArchiveManualImportSlotFilter.Available);
        var invalid = presenter.ApplyManualImportFilter(state, UnityArchiveManualImportSlotFilter.Invalid);
        var manual = presenter.ApplyManualImportFilter(state, UnityArchiveManualImportSlotFilter.ManualImportProvider);
        var future = presenter.ApplyManualImportFilter(state, UnityArchiveManualImportSlotFilter.FutureProviders);

        Assert.All(missing.VisibleManualImportSlots, slot => Assert.Equal(UnityArchiveFulfillmentStatus.missing, slot.Status));
        Assert.All(available.VisibleManualImportSlots, slot => Assert.Equal(UnityArchiveFulfillmentStatus.available, slot.Status));
        Assert.All(invalid.VisibleManualImportSlots, slot => Assert.Equal(UnityArchiveFulfillmentStatus.invalid, slot.Status));
        Assert.Equal(2, manual.VisibleManualImportSlots.Count);
        Assert.Single(future.VisibleManualImportSlots);
    }

    [Fact]
    public async Task RunManualImportUsesExistingServiceAndRefreshesReports()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        var archiveRoot = CreateManualImportMetadata(temp.Path);
        WriteText(archiveRoot, "manual-import/put-files-here/asset-slot.missing.png", "import bytes");
        WriteJson(archiveRoot, "manual-import/import-manifest.json", new UnityArchiveManualProviderImportManifest
        {
            Entries =
            [
                new UnityArchiveManualProviderImportManifestEntry
                {
                    SlotId = "asset-slot.missing",
                    SourceRelativePath = "put-files-here/asset-slot.missing.png",
                    ExpectedOutputRelativePath = "assets/generated/icon/missing.png"
                }
            ]
        });

        var state = await new UnityArchiveReviewPresenter().RunManualImportAsync(
            temp.Path,
            "snapshot-a",
            UnityArchiveManualImportSlotFilter.All,
            "asset-slot.missing",
            overwriteExisting: false);

        Assert.True(File.Exists(Path.Combine(archiveRoot, "assets", "generated", "icon", "missing.png")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "manual-provider-import-report.json")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "fulfillment-state.json")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "archive-review.json")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "archive-review-history-index.json")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "archive-review-comparison.json")));
        Assert.Contains("Manual import finished", state.ManualImportWorkspaceStatus, StringComparison.Ordinal);
        Assert.Contains("Ready", state.ManualImportReportStatus, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunManualImportMissingManifestReportsStatus()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        CreateManualImportMetadata(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RunManualImportAsync(
            temp.Path,
            null,
            UnityArchiveManualImportSlotFilter.All,
            null,
            overwriteExisting: false);

        Assert.Contains("manifest is missing", state.ManualImportWorkspaceStatus, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MissingManifest", state.ManualImportReportStatus, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SelectedSnapshotDetailStillUpdatesAfterRefresh()
    {
        using var temp = new TempDirectory();
        CreateReports(temp.Path);
        CreateManualImportMetadata(temp.Path);

        var state = await new UnityArchiveReviewPresenter().RefreshAsync(temp.Path, "snapshot-a");
        var refreshed = await new UnityArchiveReviewPresenter().RefreshAsync(
            temp.Path,
            state.SelectedSnapshotId,
            UnityArchiveManualImportSlotFilter.Missing);

        Assert.Equal("snapshot-a", refreshed.SelectedSnapshotId);
        Assert.Equal(1, refreshed.SelectedSnapshotSequence);
        Assert.Equal("Loaded", refreshed.SelectedSnapshotStatus);
        Assert.Contains("snapshot-a", refreshed.SelectedSnapshotJson, StringComparison.Ordinal);
    }

    [Fact]
    public void UserControlCanBeConstructedWithoutRuntimeServices()
    {
        using var page = new UnityArchiveReviewPageControl();

        Assert.Equal("unity_archive_review", page.Id);
        Assert.Equal("Unity Archive Review", page.Title);
        Assert.Equal(41, page.SortOrder);
    }

    [Fact]
    public void PageExposesManualImportControls()
    {
        using var page = new UnityArchiveReviewPageControl();

        Assert.Single(page.Controls.Find("_manualImportSlotsGrid", true));
        Assert.Single(page.Controls.Find("_createManifestTemplateButton", true));
        Assert.Single(page.Controls.Find("_openManualImportFolderButton", true));
        Assert.Single(page.Controls.Find("_runManualImportButton", true));
        Assert.Single(page.Controls.Find("_allowOverwriteCheckBox", true));
    }

    [Fact]
    public void CompositionRootRegistersArchiveReviewPage()
    {
        using var compositionRoot = new CompositionRoot();

        var registry = compositionRoot.ResolveEditorPageRegistry();
        var page = Assert.Single(registry.Pages, candidate => candidate.Id == "unity_archive_review");

        Assert.IsType<UnityArchiveReviewPageControl>(page);
        Assert.Equal("Unity Archive Review", page.Title);
    }

    internal static string CreateArchiveRoot(string projectFolder)
    {
        var archiveRoot = Path.Combine(projectFolder, ".llmgc", "unity-archive");
        Directory.CreateDirectory(Path.Combine(archiveRoot, "production"));
        return archiveRoot;
    }

    internal static void CreateReports(string projectFolder)
    {
        var archiveRoot = CreateArchiveRoot(projectFolder);
        var production = Path.Combine(archiveRoot, "production");
        File.WriteAllText(Path.Combine(production, "archive-review.json"), """
        {
          "schemaVersion": "1",
          "readiness": "ReadyWithWarnings",
          "sourceFileCount": 3,
          "diagnosticCount": 1
        }
        """);
        File.WriteAllText(Path.Combine(production, "archive-review.md"), "# Current Review\n\nReady with warnings.");
        File.WriteAllText(Path.Combine(production, "archive-review-comparison.json"), """
        {
          "schemaVersion": "1",
          "readiness": "Ready",
          "currentSnapshotId": "snapshot-b",
          "previousSnapshotId": "snapshot-a"
        }
        """);
        File.WriteAllText(Path.Combine(production, "archive-review-comparison.md"), "# Comparison\n\nNo blocking changes.");
        File.WriteAllText(Path.Combine(production, "archive-review-history-index.json"), """
        {
          "schemaVersion": "1",
          "entries": [
            {
              "sequence": 1,
              "snapshotId": "snapshot-a",
              "relativePath": "review-history/snapshot-a/archive-review.json"
            },
            {
              "sequence": 2,
              "snapshotId": "snapshot-b",
              "relativePath": "review-history/snapshot-b/archive-review.json"
            }
          ]
        }
        """);

        foreach (var snapshotId in new[] { "snapshot-a", "snapshot-b" })
        {
            var snapshotFolder = Path.Combine(archiveRoot, "review-history", snapshotId);
            Directory.CreateDirectory(snapshotFolder);
            File.WriteAllText(
                Path.Combine(snapshotFolder, "archive-review.json"),
                $"{{\"schemaVersion\":\"1\",\"readiness\":\"Ready\",\"snapshotMarker\":\"{snapshotId}\"}}");
        }
    }

    internal static string CreateManualImportMetadata(string projectFolder)
    {
        var archiveRoot = CreateArchiveRoot(projectFolder);
        var assetSlots = new UnityArchiveAssetSlotIndex
        {
            Slots =
            [
                new UnityArchiveAssetSlot
                {
                    SlotId = "asset-slot.missing",
                    RequestId = "asset-request.missing",
                    AssetId = "asset/missing",
                    AssetKind = UnityArchiveAssetKind.icon,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "assets/generated/icon/missing.png"
                },
                new UnityArchiveAssetSlot
                {
                    SlotId = "asset-slot.available",
                    RequestId = "asset-request.available",
                    AssetId = "asset/available",
                    AssetKind = UnityArchiveAssetKind.icon,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "assets/generated/icon/available.png"
                }
            ]
        };
        var audioSlots = new UnityArchiveAudioSlotIndex
        {
            Slots =
            [
                new UnityArchiveAudioSlot
                {
                    SlotId = "audio-slot.invalid",
                    RequestId = "audio-request.invalid",
                    AudioId = "audio/invalid",
                    AudioKind = UnityArchiveAudioKind.music,
                    ProviderKind = UnityArchiveRequestProviderKind.suno_future,
                    ExpectedOutputRelativePath = "audio/generated/music/invalid.wav"
                }
            ]
        };
        var plan = new UnityArchiveFulfillmentPlan
        {
            Slots = assetSlots.Slots.Select(slot => new UnityArchiveFulfillmentSlot
                {
                    SlotId = slot.SlotId,
                    RequestId = slot.RequestId,
                    ProviderKind = slot.ProviderKind,
                    ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath
                })
                .Append(new UnityArchiveFulfillmentSlot
                {
                    SlotId = "audio-slot.invalid",
                    RequestId = "audio-request.invalid",
                    ProviderKind = UnityArchiveRequestProviderKind.suno_future,
                    ExpectedOutputRelativePath = "audio/generated/music/invalid.wav"
                }).ToList()
        };
        WriteJson(archiveRoot, "production/fulfillment-plan.json", plan);
        WriteJson(archiveRoot, "assets/asset-slots.json", assetSlots);
        WriteJson(archiveRoot, "audio/audio-slots.json", audioSlots);
        WriteJson(archiveRoot, "lua/module-slots.json", new UnityArchiveLuaModuleSlotIndex());
        WriteJson(archiveRoot, "production/fulfillment-state.json", new UnityArchiveFulfillmentStateReport
        {
            TotalSlotCount = 3,
            MissingCount = 1,
            AvailableCount = 1,
            InvalidCount = 1,
            Entries =
            [
                State("asset-slot.missing", UnityArchiveRequestProviderKind.manual_import, "assets/generated/icon/missing.png", UnityArchiveFulfillmentStatus.missing),
                State("asset-slot.available", UnityArchiveRequestProviderKind.manual_import, "assets/generated/icon/available.png", UnityArchiveFulfillmentStatus.available),
                State("audio-slot.invalid", UnityArchiveRequestProviderKind.suno_future, "audio/generated/music/invalid.wav", UnityArchiveFulfillmentStatus.invalid)
            ]
        });
        WriteJson(archiveRoot, "production/invalid-outputs.json", new UnityArchiveInvalidOutputsReport
        {
            InvalidOutputs =
            [
                new UnityArchiveInvalidOutputEntry
                {
                    SlotId = "audio-slot.invalid",
                    ExpectedOutputRelativePath = "audio/generated/music/invalid.wav",
                    Reason = "empty_file"
                }
            ]
        });
        WriteText(archiveRoot, "assets/generated/icon/available.png", "available bytes");
        WriteText(archiveRoot, "audio/generated/music/invalid.wav", string.Empty);
        return archiveRoot;
    }

    private static UnityArchiveFulfillmentStateEntry State(
        string slotId,
        UnityArchiveRequestProviderKind providerKind,
        string path,
        UnityArchiveFulfillmentStatus status) => new()
    {
        SlotId = slotId,
        ProviderKind = providerKind,
        ExpectedOutputRelativePath = path,
        Status = status
    };

    internal static void WriteJson<T>(string archiveRoot, string relativePath, T value) =>
        WriteText(archiveRoot, relativePath, JsonSerializer.Serialize(value, JsonOptions));

    internal static string WriteText(string archiveRoot, string relativePath, string content)
    {
        var path = Path.Combine(archiveRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
    }

    internal sealed class TempDirectory : IDisposable
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
