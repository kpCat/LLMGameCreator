using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveManualImportTemplateTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task CreateManualImportTemplateWritesMissingSlotsOnly()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);
        var service = new UnityArchiveManualImportTemplateService();

        var first = await service.CreateTemplateAsync(archiveRoot);
        var firstBytes = await File.ReadAllBytesAsync(first.TemplateFullPath);
        var second = await service.CreateTemplateAsync(archiveRoot);
        var secondBytes = await File.ReadAllBytesAsync(second.TemplateFullPath);
        var manifest = JsonSerializer.Deserialize<UnityArchiveManualProviderImportManifest>(firstBytes, JsonOptions)!;

        Assert.True(first.Succeeded);
        Assert.Equal(UnityArchiveManualImportTemplateService.TemplateRelativePath, first.TemplateRelativePath);
        Assert.Equal(2, first.EntryCount);
        Assert.Equal(firstBytes, secondBytes);
        Assert.Equal(first.EntryCount, second.EntryCount);
        Assert.DoesNotContain(manifest.Entries, entry => entry.SlotId == "asset-slot.available");
        Assert.Contains(manifest.Entries, entry => entry.SlotId == "asset-slot.missing");
        Assert.Contains(manifest.Entries, entry => entry.SlotId == "audio-slot.invalid");
        Assert.All(manifest.Entries, entry =>
        {
            Assert.StartsWith("put-files-here/", entry.SourceRelativePath, StringComparison.Ordinal);
            Assert.DoesNotContain("manual-import/", entry.SourceRelativePath, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\\", entry.SourceRelativePath, StringComparison.Ordinal);
            Assert.DoesNotContain("..", entry.SourceRelativePath, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task CreateManualImportTemplateDoesNotOverwriteImportManifest()
    {
        using var temp = new TempDirectory();
        var archiveRoot = CreateArchive(temp.Path);
        var manifestPath = WriteText(archiveRoot, "manual-import/import-manifest.json", "{\"keep\":\"byte-identical\"}");
        var before = await File.ReadAllBytesAsync(manifestPath);

        await new UnityArchiveManualImportTemplateService().CreateTemplateAsync(archiveRoot);

        Assert.Equal(before, await File.ReadAllBytesAsync(manifestPath));
    }

    [Fact]
    public async Task WorkspaceLoadsPlanWhenTypedIndexesAreMissing()
    {
        using var temp = new TempDirectory();
        var archiveRoot = Path.Combine(temp.Path, ".llmgc", "unity-archive");
        WriteJson(archiveRoot, "production/fulfillment-plan.json", new UnityArchiveFulfillmentPlan
        {
            Slots =
            [
                new UnityArchiveFulfillmentSlot
                {
                    SlotId = "unknown-slot.one",
                    RequestId = "request.one",
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    ExpectedOutputRelativePath = "assets/generated/unknown/one.png"
                }
            ]
        });

        var result = await new UnityArchiveManualImportTemplateService().LoadWorkspaceAsync(archiveRoot);

        var slot = Assert.Single(result.Slots);
        Assert.Equal(UnityArchiveManualImportSlotKind.Unknown, slot.Kind);
        Assert.Equal(UnityArchiveFulfillmentStatus.missing, slot.Status);
        Assert.Equal(UnityArchiveManualImportWorkspaceReadiness.ReadyWithWarnings, result.Readiness);
    }

    private static string CreateArchive(string projectRoot)
    {
        var archiveRoot = Path.Combine(projectRoot, ".llmgc", "unity-archive");
        var assetSlots = new UnityArchiveAssetSlotIndex
        {
            Slots =
            [
                Asset("asset-slot.missing", "assets/generated/icon/missing.png"),
                Asset("asset-slot.available", "assets/generated/icon/available.png")
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
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
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
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
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
                State("asset-slot.missing", "assets/generated/icon/missing.png", UnityArchiveFulfillmentStatus.missing),
                State("asset-slot.available", "assets/generated/icon/available.png", UnityArchiveFulfillmentStatus.available),
                State("audio-slot.invalid", "audio/generated/music/invalid.wav", UnityArchiveFulfillmentStatus.invalid)
            ]
        });
        WriteText(archiveRoot, "assets/generated/icon/available.png", "available bytes");
        WriteText(archiveRoot, "audio/generated/music/invalid.wav", string.Empty);
        return archiveRoot;
    }

    private static UnityArchiveAssetSlot Asset(string slotId, string path) => new()
    {
        SlotId = slotId,
        RequestId = slotId.Replace("slot", "request", StringComparison.Ordinal),
        AssetId = slotId,
        AssetKind = UnityArchiveAssetKind.icon,
        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
        ExpectedOutputRelativePath = path
    };

    private static UnityArchiveFulfillmentStateEntry State(
        string slotId,
        string path,
        UnityArchiveFulfillmentStatus status) => new()
    {
        SlotId = slotId,
        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
        ExpectedOutputRelativePath = path,
        Status = status
    };

    private static void WriteJson<T>(string archiveRoot, string relativePath, T value) =>
        WriteText(archiveRoot, relativePath, JsonSerializer.Serialize(value, JsonOptions));

    private static string WriteText(string archiveRoot, string relativePath, string content)
    {
        var path = Path.Combine(archiveRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
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
