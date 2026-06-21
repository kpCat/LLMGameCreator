using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityTargetContractTests
{
    private readonly UnityTargetContractPresetProvider _presets = new();
    private readonly UnityTargetContractValidator _validator = new();

    [Fact]
    public void BuiltInUnityTargetProfileIdsAreUnique()
    {
        var profiles = _presets.ListTargetProfiles();

        Assert.Equal(
            profiles.Count,
            profiles.Select(profile => profile.TargetProfileId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void BuiltInUnityRuntimeModuleIdsAreUnique()
    {
        var modules = _presets.ListRuntimeModules();

        Assert.Equal(
            modules.Count,
            modules.Select(module => module.ModuleId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(22, modules.Count);
    }

    [Fact]
    public void CurrentGenericUnityTargetAndTopDownArchiveValidate()
    {
        Assert.True(_presets.TryGetTargetProfile(
            UnityTargetContractPresetProvider.GenericUnityPlayerTwoPointFiveD,
            out var profile));

        var profileResult = _validator.ValidateTargetProfile(profile, _presets.ListRuntimeModules());
        var archiveResult = _validator.ValidateArchive(
            _presets.CreateTopDownGeneratedRpgArchive(),
            _presets.ListTargetProfiles(),
            _presets.ListRuntimeModules());

        Assert.True(profileResult.Ok, JoinDiagnostics(profileResult));
        Assert.Empty(profileResult.Diagnostics);
        Assert.True(archiveResult.Ok, JoinDiagnostics(archiveResult));
        Assert.Empty(archiveResult.Diagnostics);
    }

    [Fact]
    public void FutureMixedViewTargetReportsWarningsWithoutThrowing()
    {
        Assert.True(_presets.TryGetTargetProfile(
            UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture,
            out var profile));

        var result = _validator.ValidateTargetProfile(profile, _presets.ListRuntimeModules());

        Assert.True(result.Ok, JoinDiagnostics(result));
        Assert.Empty(result.Errors);
        Assert.Contains(result.Warnings, diagnostic =>
            diagnostic.Code == UnityTargetContractDiagnosticCodes.FutureRuntimeModule &&
            diagnostic.RelatedId == "unity.world.imported_real_map_future");
        Assert.Contains(result.Warnings, diagnostic =>
            diagnostic.RelatedId == "unity.society.npc_schedule_future");
    }

    [Fact]
    public void UiLayoutContractSupportsPanelsWidgetsAndBindings()
    {
        var layout = Assert.Single(_presets.CreateTopDownGeneratedRpgArchive().UiLayouts);
        var panel = Assert.Single(layout.Panels);
        var widget = Assert.Single(panel.Widgets);
        var binding = Assert.Single(layout.Bindings);

        Assert.Equal("health_bar", widget.WidgetKind);
        Assert.Equal(binding.BindingId, widget.BindingId);
        Assert.Equal("player.stats.health", binding.SourcePath);
    }

    [Fact]
    public void AssetAndAudioRequestsKeepFutureProviderKindsAsMetadataOnly()
    {
        var assetRequests = new[]
        {
            new UnityAssetGenerationRequest { RequestId = "asset.manual", Source = UnityAssetRequestSource.Manual },
            new UnityAssetGenerationRequest
            {
                RequestId = "asset.comfy.future",
                Source = UnityAssetRequestSource.ComfyUiFuture,
                Metadata = new Dictionary<string, string> { ["workflow_hint"] = "portrait" }
            }
        };
        var audioRequests = new[]
        {
            new UnityAudioGenerationRequest { RequestId = "audio.manual", Source = UnityAudioRequestSource.Manual },
            new UnityAudioGenerationRequest
            {
                RequestId = "audio.suno.future",
                Source = UnityAudioRequestSource.SunoLikeFuture,
                Metadata = new Dictionary<string, string> { ["theme_hint"] = "frontier" }
            }
        };

        Assert.Contains(assetRequests, request => request.Source == UnityAssetRequestSource.ComfyUiFuture);
        Assert.Contains(audioRequests, request => request.Source == UnityAudioRequestSource.SunoLikeFuture);
        Assert.Equal("portrait", assetRequests[1].Metadata["workflow_hint"]);
        Assert.Equal("frontier", audioRequests[1].Metadata["theme_hint"]);
    }

    [Fact]
    public void LargeWorldPolicySupportsLazyGenerationAndDirtyDeltaPersistence()
    {
        var policy = _presets.CreateTopDownGeneratedRpgArchive().WorldStreamingPolicy;

        Assert.Equal(UnityWorldScale.Large, policy.WorldScale);
        Assert.True(policy.StoreSeedRulesAndTemplates);
        Assert.True(policy.MaterializeActiveChunksOnly);
        Assert.True(policy.PersistDirtyDeltas);
        Assert.True(policy.GenerateNpcsLazily);
        Assert.True(policy.GenerateQuestsLazily);
        Assert.True(policy.SeparateAuthoredAndGeneratedPopulation);
        Assert.InRange(policy.ActiveNpcBudget, 1, 128);
        Assert.Equal(UnityMaterializationPolicy.AuthoredImportantAndLazyGenerated, policy.NpcMaterializationPolicy);
        Assert.Equal(UnityMaterializationPolicy.LazyOnDemand, policy.QuestMaterializationPolicy);
    }

    [Fact]
    public void ValidatorReportsMalformedArchiveAndCatalogDiagnostics()
    {
        var modules = _presets.ListRuntimeModules()
            .Concat(
            [
                new UnityRuntimeModuleContract { ModuleId = "unity.core.archive_loader" },
                new UnityRuntimeModuleContract()
            ])
            .ToList();
        var archive = new UnityGameArchiveManifest
        {
            GameId = "../unsafe",
            TargetProfileId = "target.unknown",
            RuntimeModuleIds = ["unity.module.unknown"],
            UiLayouts =
            [
                new UnityUiLayoutContract
                {
                    LayoutId = "layout.invalid",
                    Bindings = [new UnityUiBindingContract { BindingId = "binding.invalid" }]
                }
            ],
            AssetRequests =
            [
                new UnityAssetGenerationRequest { RequestId = "request.duplicate" },
                new UnityAssetGenerationRequest { RequestId = "REQUEST.DUPLICATE" }
            ],
            AudioRequests =
            [
                new UnityAudioGenerationRequest { RequestId = "audio.duplicate" },
                new UnityAudioGenerationRequest { RequestId = "AUDIO.DUPLICATE" }
            ],
            WorldStreamingPolicy = new UnityWorldStreamingPolicy { WorldScale = UnityWorldScale.Large }
        };

        var result = _validator.ValidateArchive(archive, _presets.ListTargetProfiles(), modules);

        Assert.False(result.Ok);
        Assert.All(
            new[]
            {
                UnityTargetContractDiagnosticCodes.BlankId,
                UnityTargetContractDiagnosticCodes.DuplicateRuntimeModuleId,
                UnityTargetContractDiagnosticCodes.UnknownTargetProfile,
                UnityTargetContractDiagnosticCodes.UnknownRuntimeModule,
                UnityTargetContractDiagnosticCodes.BlankUiBindingPath,
                UnityTargetContractDiagnosticCodes.DuplicateAssetRequestId,
                UnityTargetContractDiagnosticCodes.DuplicateAudioRequestId,
                UnityTargetContractDiagnosticCodes.UnsafeArchiveId,
                UnityTargetContractDiagnosticCodes.InconsistentLargeWorldStreaming
            },
            code => Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == code));
    }

    private static string JoinDiagnostics(UnityTargetContractValidationResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
