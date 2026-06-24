using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveManualProviderImportTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task ManualProviderImportMissingManifestReturnsReportWithoutThrowing()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot,
            RefreshFulfillmentState = false,
            RefreshReviewHistoryComparison = false
        });

        Assert.Equal(UnityArchiveManualProviderImportReadiness.MissingManifest, result.Readiness);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "manual_import.missing_manifest");
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "manual-provider-import-report.json")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "manual-provider-import-report.md")));
    }

    [Fact]
    public async Task ManualProviderImportInvalidManifestReturnsInvalidManifestWithoutThrowing()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);
        WriteText(archiveRoot, "manual-import/import-manifest.json", "{ invalid");

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot,
            RefreshFulfillmentState = false,
            RefreshReviewHistoryComparison = false
        });

        Assert.Equal(UnityArchiveManualProviderImportReadiness.InvalidManifest, result.Readiness);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "manual_import.invalid_manifest_json");
    }

    [Fact]
    public async Task ManualProviderImportRejectsUnknownUnsafeAndExtensionMismatchEntries()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);
        WriteText(archiveRoot, "manual-import/files/wrong.jpg", "jpg bytes");
        WriteManifest(archiveRoot,
        [
            new UnityArchiveManualProviderImportManifestEntry
            {
                SlotId = "asset-slot.unknown",
                SourceRelativePath = "files/unknown.png"
            },
            new UnityArchiveManualProviderImportManifestEntry
            {
                SlotId = "asset-slot.hero",
                SourceRelativePath = "../outside.png"
            },
            new UnityArchiveManualProviderImportManifestEntry
            {
                SlotId = "asset-slot.hero-alt",
                SourceRelativePath = "files/wrong.jpg"
            }
        ]);

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot,
            RefreshFulfillmentState = false,
            RefreshReviewHistoryComparison = false
        });

        Assert.Equal(UnityArchiveManualProviderImportReadiness.BlockedByErrors, result.Readiness);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "manual_import.unknown_slot");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "manual_import.unsafe_source_path");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "manual_import.extension_mismatch");
        Assert.All(result.Entries, entry => Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Invalid, entry.Status));
        Assert.False(File.Exists(Path.Combine(archiveRoot, "assets", "generated", "icon", "hero.png")));
    }

    [Fact]
    public async Task ManualProviderImportCopiesAssetAudioAndLuaToExpectedSlots()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);
        WriteText(archiveRoot, "manual-import/files/hero.png", "png bytes");
        WriteText(archiveRoot, "manual-import/files/theme.wav", "wav bytes");
        WriteText(archiveRoot, "manual-import/files/inventory.lua", "return {}");
        WriteManifest(archiveRoot,
        [
            Entry("asset-slot.hero", "files/hero.png", "assets/generated/icon/hero.png"),
            Entry("audio-slot.theme", "files/theme.wav", "audio/generated/music/theme.wav"),
            Entry("lua-slot.inventory", "files/inventory.lua", "lua/generated/inventory.lua")
        ]);

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot,
            RefreshReviewHistoryComparison = false
        });

        Assert.Equal(UnityArchiveManualProviderImportReadiness.Ready, result.Readiness);
        Assert.Equal(3, result.ImportedCount);
        Assert.True(File.Exists(Path.Combine(archiveRoot, "assets", "generated", "icon", "hero.png")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "audio", "generated", "music", "theme.wav")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "lua", "generated", "inventory.lua")));
        Assert.All(result.Entries, entry =>
        {
            Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Imported, entry.Status);
            Assert.Equal(64, entry.ContentSha256.Length);
            Assert.True(entry.FileSizeBytes > 0);
        });
    }

    [Fact]
    public async Task ManualProviderImportIsIdempotentForSameBytesAndConflictsForDifferentBytes()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);
        WriteText(archiveRoot, "manual-import/files/hero.png", "same bytes");
        WriteManifest(archiveRoot, [Entry("asset-slot.hero", "files/hero.png")]);
        var request = new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot,
            RefreshFulfillmentState = false,
            RefreshReviewHistoryComparison = false
        };

        var first = await CreateService().ImportAsync(request);
        var second = await CreateService().ImportAsync(request);
        WriteText(archiveRoot, "manual-import/files/hero.png", "different bytes");
        var conflict = await CreateService().ImportAsync(request);

        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Imported, Assert.Single(first.Entries).Status);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.AlreadyImported, Assert.Single(second.Entries).Status);
        Assert.Equal(1, second.SkippedCount);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Conflict, Assert.Single(conflict.Entries).Status);
        Assert.Equal(1, conflict.ConflictCount);
        Assert.Equal("same bytes", File.ReadAllText(Path.Combine(archiveRoot, "assets", "generated", "icon", "hero.png")));
    }

    [Fact]
    public async Task ManualProviderImportReportsAreDeterministicTimestampFreeAndArchiveRelative()
    {
        using var firstTemp = new TempDirectory();
        using var secondTemp = new TempDirectory();
        var firstArchive = CreateArchive(firstTemp.Path);
        var secondArchive = CreateArchive(secondTemp.Path);
        foreach (var archiveRoot in new[] { firstArchive, secondArchive })
        {
            WriteText(archiveRoot, "manual-import/files/hero.png", "stable bytes");
            WriteManifest(archiveRoot, [Entry("asset-slot.hero", "files/hero.png")]);
            await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
            {
                ArchiveDirectoryPath = archiveRoot,
                RefreshFulfillmentState = false,
                RefreshReviewHistoryComparison = false
            });
        }

        var firstJson = File.ReadAllText(Path.Combine(firstArchive, "production", "manual-provider-import-report.json"));
        var secondJson = File.ReadAllText(Path.Combine(secondArchive, "production", "manual-provider-import-report.json"));
        var firstMarkdown = File.ReadAllText(Path.Combine(firstArchive, "production", "manual-provider-import-report.md"));
        var secondMarkdown = File.ReadAllText(Path.Combine(secondArchive, "production", "manual-provider-import-report.md"));

        Assert.Equal(firstJson, secondJson);
        Assert.Equal(firstMarkdown, secondMarkdown);
        Assert.DoesNotContain("timestamp", firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("lastWriteTime", firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(firstArchive, firstJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(firstArchive, firstMarkdown, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0x7B, File.ReadAllBytes(Path.Combine(firstArchive, "production", "manual-provider-import-report.json"))[0]);
    }

    [Fact]
    public async Task ManualProviderImportRefreshesFulfillmentReviewHistoryAndComparison()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path, includeReviewInputs: true);
        var reviewService = new UnityArchiveReviewSnapshotService();
        await reviewService.ReviewAsync(new UnityArchiveReviewSnapshotRequest { ArchiveDirectoryPath = archiveRoot });
        await new UnityArchiveReviewHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest { ArchiveDirectoryPath = archiveRoot });

        WriteText(archiveRoot, "manual-import/files/hero.png", "png bytes");
        WriteManifest(archiveRoot, [Entry("asset-slot.hero", "files/hero.png")]);

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot
        });

        var fulfillmentJson = File.ReadAllText(Path.Combine(archiveRoot, "production", "fulfillment-state.json"));
        var history = JsonSerializer.Deserialize<UnityArchiveReviewHistoryIndex>(
            File.ReadAllText(Path.Combine(archiveRoot, "production", "archive-review-history-index.json")),
            JsonOptions)!;
        var comparison = JsonSerializer.Deserialize<UnityArchiveReviewComparisonReport>(
            File.ReadAllText(Path.Combine(archiveRoot, "production", "archive-review-comparison.json")),
            JsonOptions)!;

        Assert.Equal(UnityArchiveManualProviderImportReadiness.Ready, result.Readiness);
        Assert.Contains("\"status\": \"available\"", fulfillmentJson, StringComparison.OrdinalIgnoreCase);
        Assert.True(history.Entries.Count >= 2);
        Assert.False(string.IsNullOrWhiteSpace(comparison.CurrentSnapshotId));
        Assert.False(string.IsNullOrWhiteSpace(comparison.PreviousSnapshotId));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "archive-review.json")));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "archive-review.md")));
    }

    [Fact]
    public async Task IdempotentAlreadyImportedDoesNotStoreNewHistorySnapshot()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path, includeReviewInputs: true);
        WriteText(archiveRoot, "manual-import/files/hero.png", "same bytes");
        WriteManifest(archiveRoot, [Entry("asset-slot.hero", "files/hero.png")]);
        var service = CreateService();

        var first = await service.ImportAsync(new UnityArchiveManualProviderImportRequest { ArchiveDirectoryPath = archiveRoot });
        var historyCountAfterFirst = ReadHistoryCount(archiveRoot);
        var second = await service.ImportAsync(new UnityArchiveManualProviderImportRequest { ArchiveDirectoryPath = archiveRoot });

        Assert.True(first.TargetOutputsChanged);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Imported, Assert.Single(first.Entries).Status);
        Assert.False(second.TargetOutputsChanged);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.AlreadyImported, Assert.Single(second.Entries).Status);
        Assert.Equal(historyCountAfterFirst, ReadHistoryCount(archiveRoot));
        Assert.True(File.Exists(Path.Combine(archiveRoot, "production", "manual-provider-import-report.json")));
    }

    [Fact]
    public async Task ConflictOnlyRunDoesNotStoreNewHistorySnapshot()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path, includeReviewInputs: true);
        await InitializeHistoryAsync(archiveRoot);
        var initialCount = ReadHistoryCount(archiveRoot);
        WriteText(archiveRoot, "assets/generated/icon/hero.png", "existing bytes");
        WriteText(archiveRoot, "manual-import/files/hero.png", "different bytes");
        WriteManifest(archiveRoot, [Entry("asset-slot.hero", "files/hero.png")]);

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot
        });

        Assert.False(result.TargetOutputsChanged);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Conflict, Assert.Single(result.Entries).Status);
        Assert.Equal(initialCount, ReadHistoryCount(archiveRoot));
    }

    [Fact]
    public async Task OverwriteChangedBytesStoresNewHistorySnapshot()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path, includeReviewInputs: true);
        await InitializeHistoryAsync(archiveRoot);
        var initialCount = ReadHistoryCount(archiveRoot);
        WriteText(archiveRoot, "assets/generated/icon/hero.png", "existing bytes");
        WriteText(archiveRoot, "manual-import/files/hero.png", "replacement bytes");
        WriteManifest(archiveRoot, [Entry("asset-slot.hero", "files/hero.png")]);

        var result = await CreateService().ImportAsync(new UnityArchiveManualProviderImportRequest
        {
            ArchiveDirectoryPath = archiveRoot,
            OverwriteExisting = true
        });

        Assert.True(result.TargetOutputsChanged);
        Assert.Equal(UnityArchiveManualProviderImportEntryStatus.Imported, Assert.Single(result.Entries).Status);
        Assert.True(ReadHistoryCount(archiveRoot) > initialCount);
        Assert.Equal("replacement bytes", File.ReadAllText(Path.Combine(archiveRoot, "assets", "generated", "icon", "hero.png")));
    }

    private static UnityArchiveManualProviderImportService CreateService() => new();

    private static async Task InitializeHistoryAsync(string archiveRoot)
    {
        await new UnityArchiveReviewSnapshotService().ReviewAsync(new UnityArchiveReviewSnapshotRequest
        {
            ArchiveDirectoryPath = archiveRoot
        });
        await new UnityArchiveReviewHistoryService().StoreAsync(new UnityArchiveReviewHistoryRequest
        {
            ArchiveDirectoryPath = archiveRoot
        });
    }

    private static int ReadHistoryCount(string archiveRoot)
    {
        var history = JsonSerializer.Deserialize<UnityArchiveReviewHistoryIndex>(
            File.ReadAllText(Path.Combine(archiveRoot, "production", "archive-review-history-index.json")),
            JsonOptions)!;
        return history.Entries.Count;
    }

    private static UnityArchiveManualProviderImportManifestEntry Entry(
        string slotId,
        string sourceRelativePath,
        string? expectedOutputRelativePath = null) =>
        new()
        {
            SlotId = slotId,
            SourceRelativePath = sourceRelativePath,
            ExpectedOutputRelativePath = expectedOutputRelativePath
        };

    private static string CreateArchive(string projectRoot, bool includeReviewInputs = false)
    {
        var archiveRoot = Path.Combine(projectRoot, ".llmgc", "unity-archive");
        Directory.CreateDirectory(archiveRoot);
        var assetSlots = new UnityArchiveAssetSlotIndex
        {
            Slots =
            [
                new UnityArchiveAssetSlot
                {
                    SlotId = "asset-slot.hero",
                    RequestId = "asset-request.hero",
                    AssetId = "asset/hero",
                    AssetKind = UnityArchiveAssetKind.icon,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "assets/generated/icon/hero.png",
                    Required = true
                },
                new UnityArchiveAssetSlot
                {
                    SlotId = "asset-slot.hero-alt",
                    RequestId = "asset-request.hero-alt",
                    AssetId = "asset/hero-alt",
                    AssetKind = UnityArchiveAssetKind.icon,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "assets/generated/icon/hero-alt.png",
                    Required = true
                }
            ]
        };
        var audioSlots = new UnityArchiveAudioSlotIndex
        {
            Slots =
            [
                new UnityArchiveAudioSlot
                {
                    SlotId = "audio-slot.theme",
                    RequestId = "audio-request.theme",
                    AudioId = "audio/theme",
                    AudioKind = UnityArchiveAudioKind.music,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "audio/generated/music/theme.wav",
                    Required = true
                }
            ]
        };
        var luaSlots = new UnityArchiveLuaModuleSlotIndex
        {
            Slots =
            [
                new UnityArchiveLuaModuleSlot
                {
                    SlotId = "lua-slot.inventory",
                    ModuleId = "inventory",
                    ModuleKind = UnityArchiveLuaModuleKind.inventory,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "lua/generated/inventory.lua",
                    Required = true
                }
            ]
        };
        var fulfillmentPlan = new UnityArchiveFulfillmentPlan
        {
            Slots = assetSlots.Slots.Select(ToFulfillmentSlot)
                .Concat(audioSlots.Slots.Select(ToFulfillmentSlot))
                .Concat(luaSlots.Slots.Select(ToFulfillmentSlot))
                .ToList()
        };

        WriteJson(archiveRoot, "production/fulfillment-plan.json", fulfillmentPlan);
        WriteJson(archiveRoot, "assets/asset-slots.json", assetSlots);
        WriteJson(archiveRoot, "audio/audio-slots.json", audioSlots);
        WriteJson(archiveRoot, "lua/module-slots.json", luaSlots);

        if (includeReviewInputs)
        {
            WriteJson(archiveRoot, "production/readiness-report.json", new UnityArchiveProviderReadinessReport
            {
                Readiness = UnityArchiveProviderPlanReadiness.Ready,
                AssetSlotCount = assetSlots.Slots.Count,
                AudioSlotCount = audioSlots.Slots.Count,
                LuaModuleSlotCount = luaSlots.Slots.Count
            });
            WriteJson(archiveRoot, "export-validation.json", new UnityArchiveMaterializationValidationReport
            {
                Readiness = UnityArchiveMaterializationReadiness.MaterializedPlayableContract,
                DryRunReadiness = UnityArchiveExportReadiness.ExportableNow
            });
            WriteJson(archiveRoot, "production/fulfillment-state.json", new UnityArchiveFulfillmentStateReport
            {
                TotalSlotCount = 4,
                MissingCount = 4
            });
            WriteJson(archiveRoot, "production/invalid-outputs.json", new UnityArchiveInvalidOutputsReport());
        }

        return archiveRoot;
    }

    private static UnityArchiveFulfillmentSlot ToFulfillmentSlot(UnityArchiveAssetSlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            RequestId = slot.RequestId,
            ProviderKind = slot.ProviderKind,
            ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
            Required = slot.Required
        };

    private static UnityArchiveFulfillmentSlot ToFulfillmentSlot(UnityArchiveAudioSlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            RequestId = slot.RequestId,
            ProviderKind = slot.ProviderKind,
            ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
            Required = slot.Required
        };

    private static UnityArchiveFulfillmentSlot ToFulfillmentSlot(UnityArchiveLuaModuleSlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            RequestId = slot.ModuleId,
            ProviderKind = slot.ProviderKind,
            ExpectedOutputRelativePath = slot.ExpectedOutputRelativePath,
            Required = slot.Required
        };

    private static void WriteManifest(
        string archiveRoot,
        IReadOnlyList<UnityArchiveManualProviderImportManifestEntry> entries)
    {
        WriteJson(archiveRoot, "manual-import/import-manifest.json", new UnityArchiveManualProviderImportManifest
        {
            Entries = entries
        });
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
