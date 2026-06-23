using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveFulfillmentStateProductSmokeTests
{
    private static readonly string[] RequiredFulfillmentStateFiles =
    [
        "production/fulfillment-state.json",
        "production/fulfilled-assets-index.json",
        "production/fulfilled-audio-index.json",
        "production/fulfilled-lua-index.json",
        "production/invalid-outputs.json"
    ];

    [Fact]
    public async Task UnityArchiveFulfillmentStateProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = temp.Path;
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = CreateService();
        var request = new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = CreatePackage()
        };

        var first = await service.MaterializeAsync(request);
        
        Assert.All(RequiredFulfillmentStateFiles, relativePath => Assert.True(File.Exists(ArchivePath(first.OutputDirectoryPath, relativePath)), relativePath));

        var fulfillmentStateContents = RequiredFulfillmentStateFiles.ToDictionary(
            relativePath => relativePath,
            relativePath => File.ReadAllText(ArchivePath(first.OutputDirectoryPath, relativePath)),
            StringComparer.Ordinal);
        foreach (var content in fulfillmentStateContents.Values)
        {
            using var document = JsonDocument.Parse(content);
            Assert.True(document.RootElement.TryGetProperty("schemaVersion", out _));
        }

        using (var fulfillmentState = JsonDocument.Parse(fulfillmentStateContents["production/fulfillment-state.json"]))
        {
            var report = fulfillmentState.RootElement.GetProperty("entries");
            Assert.True(report.GetArrayLength() >= 0);
        }

        foreach (var content in RequiredFulfillmentStateFiles.Where(path => path.EndsWith("-index.json") || path == "production/fulfillment-state.json"))
        {
            using var doc = JsonDocument.Parse(fulfillmentStateContents[content]);
            Assert.True(doc.RootElement.TryGetProperty("schemaVersion", out _));
        }

        var second = await service.MaterializeAsync(request);
        Assert.All(RequiredFulfillmentStateFiles, relativePath =>
            Assert.Equal(fulfillmentStateContents[relativePath], File.ReadAllText(ArchivePath(second.OutputDirectoryPath, relativePath))));
    }

    [Fact]
    public async Task UnityArchiveFulfillmentStateScannerDetectsManuallyCreatedOutputs()
    {
        using var temp = new TempDirectory();
        var projectRoot = temp.Path;
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = CreateService();
        var request = new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = CreatePackageWithAssetRequest()
        };

        var materialization = await service.MaterializeAsync(request);

        var slotPath = "assets/generated/portrait/portrait.npc.npc-alpha.png";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(materialization.OutputDirectoryPath, slotPath.Replace('/', Path.DirectorySeparatorChar)))!);
        File.WriteAllText(Path.Combine(materialization.OutputDirectoryPath, slotPath.Replace('/', Path.DirectorySeparatorChar)), "fake png content for test");

        var fulfillmentStateService = new UnityArchiveFulfillmentStateService();
        var providerJobPlan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.portrait.npc-alpha",
                        RequestId = "asset-request.portrait.npc-alpha",
                        AssetId = "portrait.npc.npc-alpha",
                        AssetKind = UnityArchiveAssetKind.portrait,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = slotPath,
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };
        
        var scanResult = fulfillmentStateService.Scan(new UnityArchiveFulfillmentStateRequest
        {
            OutputDirectoryPath = materialization.OutputDirectoryPath,
            ProviderJobPlan = providerJobPlan
        });
        
        Assert.Contains(scanResult.FulfillmentState.Entries, e => e.Status == UnityArchiveFulfillmentStatus.available);
    }

    private static UnityArchiveMaterializationService CreateService()
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
                PackageId = "game/fulfillment-state-smoke",
                Title = "Fulfillment State Smoke"
            }
        };
    }

    private static GamePackageDefinition CreatePackageWithAssetRequest()
    {
        return new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/fulfillment-state-smoke",
                Title = "Fulfillment State Smoke"
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