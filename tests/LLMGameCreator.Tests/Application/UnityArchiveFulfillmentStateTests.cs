using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveFulfillmentStateTests
{
    [Fact]
    public void UnityArchiveFulfillmentStateCreatesEmptyValidManifests()
    {
        var result = CreateService().Scan(CreateRequest(new UnityArchiveProviderJobPlanResult()));

        Assert.Equal(0, result.FulfillmentState.TotalSlotCount);
        Assert.Equal(0, result.FulfillmentState.MissingCount);
        Assert.Equal(0, result.FulfillmentState.AvailableCount);
        Assert.Equal(0, result.FulfillmentState.InvalidCount);
        Assert.Empty(result.FulfillmentState.Entries);
        Assert.Empty(result.FulfilledAssets.Assets);
        Assert.Empty(result.FulfilledAudio.Audio);
        Assert.Empty(result.FulfilledLua.Lua);
        Assert.Empty(result.InvalidOutputs.InvalidOutputs);
    }

    [Fact]
    public void UnityArchiveFulfillmentStateMissingExpectedOutputsMarkedMissing()
    {
        using var temp = new TempDirectory();
        var plan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.test",
                        RequestId = "request.test",
                        AssetId = "asset/test",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "assets/generated/icon/asset.test.png",
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };

        var result = CreateService().Scan(CreateRequest(plan, temp.Path));

        Assert.Single(result.FulfillmentState.Entries);
        Assert.Equal(UnityArchiveFulfillmentStatus.missing, result.FulfillmentState.Entries[0].Status);
        Assert.Equal(1, result.FulfillmentState.MissingCount);
    }

    [Fact]
    public void UnityArchiveFulfillmentStateManuallyCreatedFilesMarkedAvailable()
    {
        using var temp = new TempDirectory();
        
        Directory.CreateDirectory(Path.Combine(temp.Path, "assets", "generated", "icon"));
        File.WriteAllText(Path.Combine(temp.Path, "assets", "generated", "icon", "asset.test.png"), "fake png content");
        
        Directory.CreateDirectory(Path.Combine(temp.Path, "audio", "generated", "ui_sfx"));
        File.WriteAllText(Path.Combine(temp.Path, "audio", "generated", "ui_sfx", "sfx.ui.click.wav"), "fake wav content");
        
        Directory.CreateDirectory(Path.Combine(temp.Path, "lua", "generated"));
        File.WriteAllText(Path.Combine(temp.Path, "lua", "generated", "lua-request.inventory.lua"), "fake lua content");

        var plan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.test",
                        RequestId = "request.test",
                        AssetId = "asset/test",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "assets/generated/icon/asset.test.png",
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAudioSlot
                    {
                        SlotId = "audio-slot.test",
                        RequestId = "request.audio",
                        AudioId = "audio/ui/click",
                        AudioKind = UnityArchiveAudioKind.ui_sfx,
                        ProviderKind = UnityArchiveRequestProviderKind.local_audio_future,
                        ExpectedOutputRelativePath = "audio/generated/ui_sfx/sfx.ui.click.wav",
                        Required = true
                    }
                ]
            },
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex
            {
                Slots =
                [
                    new UnityArchiveLuaModuleSlot
                    {
                        SlotId = "lua-slot.test",
                        ModuleId = "lua-request.inventory",
                        ModuleKind = UnityArchiveLuaModuleKind.inventory,
                        ProviderKind = UnityArchiveRequestProviderKind.none,
                        ExpectedOutputRelativePath = "lua/generated/lua-request.inventory.lua",
                        Required = true
                    }
                ]
            }
        };

        var result = CreateService().Scan(CreateRequest(plan, temp.Path));

        Assert.Equal(3, result.FulfillmentState.Entries.Count);
        Assert.Equal(UnityArchiveFulfillmentStatus.available, result.FulfillmentState.Entries[0].Status);
        Assert.Single(result.FulfilledAssets.Assets);
        Assert.Single(result.FulfilledAudio.Audio);
        Assert.Single(result.FulfilledLua.Lua);
    }

    [Fact]
    public void UnityArchiveFulfillmentStateEmptyFileMarkedInvalid()
    {
        using var temp = new TempDirectory();
        
        Directory.CreateDirectory(Path.Combine(temp.Path, "assets", "generated", "icon"));
        File.WriteAllText(Path.Combine(temp.Path, "assets", "generated", "icon", "asset.test.png"), "");

        var plan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.test",
                        RequestId = "request.test",
                        AssetId = "asset/test",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "assets/generated/icon/asset.test.png",
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };

        var result = CreateService().Scan(CreateRequest(plan, temp.Path));

        Assert.Single(result.FulfillmentState.Entries);
        Assert.Equal(UnityArchiveFulfillmentStatus.invalid, result.FulfillmentState.Entries[0].Status);
        Assert.Single(result.InvalidOutputs.InvalidOutputs);
        Assert.Equal("empty_file", result.InvalidOutputs.InvalidOutputs[0].Reason);
    }

    [Fact]
    public void UnityArchiveFulfillmentStateWrongExtensionMarkedInvalid()
    {
        var plan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.test",
                        RequestId = "request.test",
                        AssetId = "asset/test",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "assets/generated/icon/asset.test.jpg",
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };

        var result = CreateService().Scan(CreateRequest(plan));

        Assert.Single(result.FulfillmentState.Entries);
        Assert.Equal(UnityArchiveFulfillmentStatus.invalid, result.FulfillmentState.Entries[0].Status);
        Assert.Single(result.Diagnostics);
        Assert.Contains(result.Diagnostics, d => d.Code == "fulfillment_state.wrong_extension");
    }

    [Fact]
    public void UnityArchiveFulfillmentStateUnsafePathIsDiagnosticError()
    {
        var plan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.test",
                        RequestId = "request.test",
                        AssetId = "asset/test",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "../../escaped.png",
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };

        var result = CreateService().Scan(CreateRequest(plan));

        Assert.Contains(result.Diagnostics, d => d.Code == "fulfillment_state.unsafe_expected_output_path");
    }

    [Fact]
    public void UnityArchiveFulfillmentStateDuplicateExpectedOutputPathIsDiagnosticError()
    {
        var plan = new UnityArchiveProviderJobPlanResult
        {
            AssetSlots = new UnityArchiveAssetSlotIndex
            {
                Slots =
                [
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.1",
                        RequestId = "request.1",
                        AssetId = "asset/1",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "assets/duplicate.png",
                        Required = true
                    },
                    new UnityArchiveAssetSlot
                    {
                        SlotId = "asset-slot.2",
                        RequestId = "request.2",
                        AssetId = "asset/2",
                        AssetKind = UnityArchiveAssetKind.icon,
                        ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                        ExpectedOutputRelativePath = "assets/duplicate.png",
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };

        var result = CreateService().Scan(CreateRequest(plan));

        Assert.Contains(result.Diagnostics, d => d.Code == "fulfillment_state.duplicate_expected_output_path");
    }

    private static UnityArchiveFulfillmentStateService CreateService() => new();

    private static UnityArchiveFulfillmentStateRequest CreateRequest(UnityArchiveProviderJobPlanResult plan, string outputDirectory = ".")
    {
        return new UnityArchiveFulfillmentStateRequest
        {
            OutputDirectoryPath = outputDirectory,
            ProviderJobPlan = plan
        };
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