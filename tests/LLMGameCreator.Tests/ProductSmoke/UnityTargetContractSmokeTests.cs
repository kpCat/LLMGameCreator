using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityTargetContractSmokeTests
{
    [Fact]
    public void UnityTargetContractProductSmoke()
    {
        var presets = new UnityTargetContractPresetProvider();
        var validator = new UnityTargetContractValidator();
        var profiles = presets.ListTargetProfiles();
        var modules = presets.ListRuntimeModules();

        Assert.Equal(
            profiles.Count,
            profiles.Select(profile => profile.TargetProfileId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(
            modules.Count,
            modules.Select(module => module.ModuleId).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.True(presets.TryGetTargetProfile(
            UnityTargetContractPresetProvider.GenericUnityPlayerTwoPointFiveD,
            out var currentProfile));
        var currentResult = validator.ValidateTargetProfile(currentProfile, modules);
        Assert.True(currentResult.Ok, JoinDiagnostics(currentResult));
        Assert.Empty(currentResult.Diagnostics);

        var archive = presets.CreateTopDownGeneratedRpgArchive();
        var archiveResult = validator.ValidateArchive(archive, profiles, modules);
        Assert.True(archiveResult.Ok, JoinDiagnostics(archiveResult));
        Assert.Empty(archiveResult.Diagnostics);

        Assert.True(presets.TryGetTargetProfile(
            UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture,
            out var futureProfile));
        var futureResult = validator.ValidateTargetProfile(futureProfile, modules);
        Assert.True(futureResult.Ok, JoinDiagnostics(futureResult));
        Assert.Empty(futureResult.Errors);
        Assert.Contains(futureResult.Warnings, diagnostic =>
            diagnostic.Code == UnityTargetContractDiagnosticCodes.FutureRuntimeModule);

        var world = archive.WorldStreamingPolicy;
        Assert.True(world.StoreSeedRulesAndTemplates);
        Assert.True(world.MaterializeActiveChunksOnly);
        Assert.True(world.PersistDirtyDeltas);
        Assert.True(world.GenerateNpcsLazily);
        Assert.True(world.GenerateQuestsLazily);
        Assert.True(world.SeparateAuthoredAndGeneratedPopulation);
        Assert.InRange(world.ActiveNpcBudget, 1, 128);
        Assert.Equal(UnityMaterializationPolicy.AuthoredImportantAndLazyGenerated, world.NpcMaterializationPolicy);

        Assert.All(modules, module => Assert.DoesNotContain("provider", module.ModuleId, StringComparison.OrdinalIgnoreCase));
        Assert.All(modules, module => Assert.DoesNotContain("generator", module.ModuleId, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(modules, module => module.ModuleId.StartsWith("runtime.", StringComparison.OrdinalIgnoreCase));
    }

    private static string JoinDiagnostics(UnityTargetContractValidationResult result)
    {
        return string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
