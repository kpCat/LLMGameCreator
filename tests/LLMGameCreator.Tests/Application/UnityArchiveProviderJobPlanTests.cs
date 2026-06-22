using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveProviderJobPlanTests
{
    [Fact]
    public void UnityArchiveProviderJobPlanCreatesEmptyValidManifests()
    {
        var result = CreateService().BuildPlan(CreateRequest(new UnityArchiveRequestPipelineResult()));

        Assert.Equal(UnityArchiveProviderPlanReadiness.Ready, result.Readiness);
        Assert.Empty(result.FulfillmentPlan.Slots);
        Assert.Empty(result.AssetSlots.Slots);
        Assert.Empty(result.AudioSlots.Slots);
        Assert.Empty(result.LuaModuleSlots.Slots);
        Assert.Equal(5, result.ProviderJobs.Batches.Count);
        Assert.All(result.ProviderJobs.Batches, batch =>
        {
            Assert.Equal("1", batch.SchemaVersion);
            Assert.Empty(batch.Jobs);
            Assert.False(batch.ExecutionEnabled);
        });
        Assert.Equal("1", result.FulfillmentPlan.SchemaVersion);
        Assert.Equal("1", result.ReadinessReport.SchemaVersion);
    }

    [Fact]
    public void UnityArchiveProviderJobPlanCreatesTypedSlotsAndProviderJobs()
    {
        var pipeline = new UnityArchiveRequestPipelineResult
        {
            AssetRequests =
            [
                new UnityArchiveAssetRequest
                {
                    RequestId = "asset-request.portrait.npc-alpha",
                    AssetId = "portrait.npc.npc-alpha",
                    AssetKind = UnityArchiveAssetKind.portrait,
                    ProviderKind = UnityArchiveRequestProviderKind.manual_import,
                    PromptOrInstruction = "Portrait",
                    SourceRef = new UnityArchiveRequestSourceRef { SourceId = "npc/alpha", SourceKind = "generated_npc" },
                    StyleTags = ["painted"]
                }
            ],
            AudioRequests =
            [
                new UnityArchiveAudioRequest
                {
                    RequestId = "audio-request.ui_sfx.ui-click",
                    AudioId = "sfx.ui.click",
                    AudioKind = UnityArchiveAudioKind.ui_sfx,
                    ProviderKind = UnityArchiveRequestProviderKind.local_audio_future,
                    PromptOrInstruction = "Click",
                    SourceRef = new UnityArchiveRequestSourceRef { SourceId = "ui.click", SourceKind = "ui_layout" }
                },
                new UnityArchiveAudioRequest
                {
                    RequestId = "audio-request.music.short-sfx",
                    AudioId = "music.theme.short_sfx",
                    AudioKind = UnityArchiveAudioKind.music,
                    ProviderKind = UnityArchiveRequestProviderKind.suno_future,
                    PromptOrInstruction = "Theme",
                    SourceRef = new UnityArchiveRequestSourceRef { SourceId = "short_sfx", SourceKind = "design_brief_audio_wish" }
                }
            ],
            LuaModuleRequests =
            [
                new UnityArchiveLuaModuleRequest
                {
                    ModuleId = "lua-request.inventory",
                    ModuleKind = UnityArchiveLuaModuleKind.inventory,
                    ProviderKind = UnityArchiveRequestProviderKind.none,
                    PromptOrInstruction = "Inventory",
                    SourceRef = new UnityArchiveRequestSourceRef { SourceId = "brief", SourceKind = "design_brief" }
                }
            ]
        };

        var result = CreateService().BuildPlan(CreateRequest(pipeline));

        Assert.Single(result.AssetSlots.Slots);
        Assert.Equal("assets/generated/portrait/portrait.npc.npc-alpha.png", result.AssetSlots.Slots[0].ExpectedOutputRelativePath);
        Assert.Equal(2, result.AudioSlots.Slots.Count);
        Assert.Contains(result.AudioSlots.Slots, slot => slot.ExpectedOutputRelativePath == "audio/generated/ui_sfx/sfx.ui.click.wav");
        Assert.Contains(result.AudioSlots.Slots, slot => slot.ExpectedOutputRelativePath == "audio/generated/music/music.theme.short_sfx.wav");
        Assert.Single(result.LuaModuleSlots.Slots);
        Assert.Equal("lua/generated/lua-request.inventory.lua", result.LuaModuleSlots.Slots[0].ExpectedOutputRelativePath);
        Assert.Equal(4, result.FulfillmentPlan.Slots.Count);
        Assert.Equal(3, result.ProviderJobs.Batches.Sum(batch => batch.Jobs.Count));
        Assert.DoesNotContain(result.ProviderJobs.Batches.SelectMany(batch => batch.Jobs), job => job.ProviderKind == UnityArchiveRequestProviderKind.none);
        Assert.All(result.ProviderJobs.Batches.SelectMany(batch => batch.Jobs), job =>
        {
            Assert.Equal(UnityArchiveProviderJobReadiness.planned_not_executed, job.Readiness);
            Assert.False(job.ExecutionEnabled);
        });
    }

    [Fact]
    public void UnityArchiveProviderJobPlanNormalizesSafeDeterministicPathsAndOutput()
    {
        var pipeline = new UnityArchiveRequestPipelineResult
        {
            AssetRequests =
            [
                new UnityArchiveAssetRequest
                {
                    RequestId = "asset/request:portrait",
                    AssetId = "../NPC Portrait:Alpha",
                    AssetKind = UnityArchiveAssetKind.portrait,
                    ProviderKind = UnityArchiveRequestProviderKind.comfyui_future
                }
            ]
        };
        var request = CreateRequest(pipeline);

        var first = CreateService().BuildPlan(request);
        var second = CreateService().BuildPlan(request);

        Assert.All(first.FulfillmentPlan.Slots, slot => Assert.True(UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(slot.ExpectedOutputRelativePath)));
        Assert.Equal("assets/generated/portrait/npc-portrait-alpha.png", first.AssetSlots.Slots[0].ExpectedOutputRelativePath);
        Assert.Equal(JsonSerializer.Serialize(first, JsonOptions), JsonSerializer.Serialize(second, JsonOptions));
    }

    [Fact]
    public void UnityArchiveProviderJobPlanReportsDuplicateIdsAndUnknownProvider()
    {
        var duplicate = new UnityArchiveAssetRequest
        {
            RequestId = "asset-request.icon.same",
            AssetId = "icon.item.same",
            AssetKind = UnityArchiveAssetKind.icon,
            ProviderKind = UnityArchiveRequestProviderKind.manual_import
        };
        var pipeline = new UnityArchiveRequestPipelineResult
        {
            AssetRequests = [duplicate, duplicate],
            AudioRequests =
            [
                new UnityArchiveAudioRequest
                {
                    RequestId = "audio-request.ui_sfx.unknown",
                    AudioId = "sfx.ui.unknown",
                    AudioKind = UnityArchiveAudioKind.ui_sfx,
                    ProviderKind = (UnityArchiveRequestProviderKind)999
                }
            ]
        };

        var result = CreateService().BuildPlan(CreateRequest(pipeline));

        Assert.Equal(UnityArchiveProviderPlanReadiness.BlockedByErrors, result.Readiness);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "provider_plan.duplicate_slot_id");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "provider_plan.duplicate_job_id");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "provider_plan.unknown_provider_kind");
    }

    private static UnityArchiveProviderJobPlanService CreateService() => new();

    private static UnityArchiveProviderJobPlanRequest CreateRequest(UnityArchiveRequestPipelineResult pipeline)
    {
        return new UnityArchiveProviderJobPlanRequest
        {
            ProjectRootPath = ".",
            RequestPipeline = pipeline,
            ArchiveManifest = new UnityGameArchiveManifest { GameId = "game" },
            DesignBrief = new GameDesignBrief { BriefId = "brief" },
            TargetProfile = new UnityTargetProfile { TargetProfileId = "target" }
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
