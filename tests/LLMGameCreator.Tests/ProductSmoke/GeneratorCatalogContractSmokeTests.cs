using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeneratorCatalogContractSmokeTests
{
    [Fact]
    public void GeneratorCatalogContractProductSmoke()
    {
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var validation = new GeneratorCatalogValidator(capabilities).Validate(catalog);
        var resolver = new GeneratorPlanResolver(capabilities, catalog);
        var presets = new GameBlueprintPresetProvider();

        Assert.Empty(catalog.DuplicateIds);
        Assert.True(validation.Ok, JoinDiagnostics(validation.Diagnostics));
        Assert.All(
            new[]
            {
                "generator.strict_llm.game_profile_v1",
                "generator.strict_llm.region_pack_v1",
                "generator.strict_llm.scene_pack_v1",
                "generator.strict_llm.npc_pack_v1",
                "generator.strict_llm.quest_pack_v1",
                "generator.strict_llm.dialogue_pack_v1",
                "generator.strict_llm.mechanics_pack_v1",
                "generator.strict_llm.encounter_pack_v1",
                "generator.strict_llm.item_pack_v1",
                "generator.package.assembly_v1",
                "generator.package.activation_v1"
            },
            generatorId => Assert.True(catalog.TryGet(generatorId, out _), generatorId));

        Assert.Equal(8, catalog.Planned.Count);
        Assert.True(presets.TryGet(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview, out var baseline));
        var baselinePlan = resolver.Resolve(baseline);
        Assert.Equal(12, baselinePlan.SelectedCurrentGenerators.Count);
        Assert.Empty(baselinePlan.RelatedPlannedGenerators);
        Assert.Empty(baselinePlan.MissingGeneratorCapabilityIds);

        Assert.True(presets.TryGet(GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture, out var future));
        var futurePlan = resolver.Resolve(future);
        Assert.Equal(8, futurePlan.RelatedPlannedGenerators.Count);
        Assert.Contains("time.calendar", futurePlan.MissingGeneratorCapabilityIds);
        Assert.Contains(futurePlan.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.PlannedGeneratorRelated);
        Assert.Contains(futurePlan.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.MissingGeneratorSupport);

        Assert.All(catalog.Manifests, manifest => Assert.False(manifest.CanRunAtRuntime));
        Assert.DoesNotContain(catalog.Manifests, manifest =>
            manifest.GeneratorId.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    private static string JoinDiagnostics(IEnumerable<GeneratorCatalogDiagnostic> diagnostics)
    {
        return string.Join(Environment.NewLine, diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
