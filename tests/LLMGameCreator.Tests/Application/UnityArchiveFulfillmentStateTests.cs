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
        Assert.All(result.FulfillmentState.Entries, entry =>
        {
            Assert.Equal(UnityArchiveFulfillmentStatus.available, entry.Status);
            Assert.True(entry.FileSizeBytes > 0);
        });
        Assert.Single(result.FulfilledAssets.Assets);
        Assert.Single(result.FulfilledAudio.Audio);
        Assert.Single(result.FulfilledLua.Lua);
        Assert.True(result.FulfilledAssets.Assets[0].FileSizeBytes > 0);
        Assert.True(result.FulfilledAudio.Audio[0].FileSizeBytes > 0);
        Assert.True(result.FulfilledLua.Lua[0].FileSizeBytes > 0);

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        Assert.DoesNotContain("lastWriteTimeUtc", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("timestamp", json, StringComparison.OrdinalIgnoreCase);
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
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "fulfillment_state.invalid_existing_output" &&
            diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error);
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
        Assert.Single(result.InvalidOutputs.InvalidOutputs);
        Assert.Equal("wrong_extension", result.InvalidOutputs.InvalidOutputs[0].Reason);
        Assert.Contains(result.Diagnostics, d =>
            d.Code == "fulfillment_state.wrong_extension" &&
            d.Severity == UnityArchiveExportDiagnosticSeverity.Error);
    }

    [Fact]
    public void UnityArchiveFulfillmentStateDirectoryAtExpectedPathMarkedInvalid()
    {
        using var temp = new TempDirectory();
        Directory.CreateDirectory(Path.Combine(temp.Path, "assets", "generated", "icon", "asset.test.png"));
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
            }
        };

        var result = CreateService().Scan(CreateRequest(plan, temp.Path));

        Assert.Equal(UnityArchiveFulfillmentStatus.invalid, Assert.Single(result.FulfillmentState.Entries).Status);
        Assert.Equal("is_directory", Assert.Single(result.InvalidOutputs.InvalidOutputs).Reason);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == "fulfillment_state.invalid_existing_output" &&
            diagnostic.Severity == UnityArchiveExportDiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData("../../escaped.png")]
    [InlineData("C:/escaped.png")]
    [InlineData("assets\\escaped.png")]
    [InlineData("assets/generated/icon:escaped.png")]
    public void UnityArchiveFulfillmentStateUnsafePathIsDiagnosticError(string unsafePath)
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
                        ExpectedOutputRelativePath = unsafePath,
                        Required = true
                    }
                ]
            },
            AudioSlots = new UnityArchiveAudioSlotIndex(),
            LuaModuleSlots = new UnityArchiveLuaModuleSlotIndex()
        };

        var result = CreateService().Scan(CreateRequest(plan));

        Assert.Equal(UnityArchiveFulfillmentStatus.invalid, Assert.Single(result.FulfillmentState.Entries).Status);
        Assert.Equal(string.Empty, result.FulfillmentState.Entries[0].ExpectedOutputRelativePath);
        Assert.Equal("unsafe_path", Assert.Single(result.InvalidOutputs.InvalidOutputs).Reason);
        Assert.Contains(result.Diagnostics, d => d.Code == "fulfillment_state.unsafe_expected_output_path");
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        Assert.DoesNotContain(unsafePath, json, StringComparison.Ordinal);
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
        Assert.Contains(result.FulfillmentState.Diagnostics, d => d.Code == "fulfillment_state.duplicate_expected_output_path");
        var json = JsonSerializer.Serialize(result.FulfillmentState, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        Assert.Contains("fulfillment_state.duplicate_expected_output_path", json, StringComparison.Ordinal);
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
