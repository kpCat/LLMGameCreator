using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class GeneratorCatalogTests
{
    private static readonly string[] StrictGeneratorIds =
    [
        "generator.strict_llm.game_profile_v1",
        "generator.strict_llm.region_pack_v1",
        "generator.strict_llm.scene_pack_v1",
        "generator.strict_llm.npc_pack_v1",
        "generator.strict_llm.quest_pack_v1",
        "generator.strict_llm.dialogue_pack_v1",
        "generator.strict_llm.mechanics_pack_v1",
        "generator.strict_llm.encounter_pack_v1",
        "generator.strict_llm.item_pack_v1"
    ];

    private static readonly string[] PlannedGeneratorIds =
    [
        "generator.semantic.world_model_seed_v1",
        "generator.procedural.quest_templates_v1",
        "generator.procedural.dialogue_realizer_v1",
        "generator.world.lazy_region_cache_v1",
        "generator.events.offscreen_scheduler_v1",
        "generator.imported_map.osm_like_classifier_v1",
        "generator.population.households_v1",
        "generator.schedule.daily_life_v1"
    ];

    [Fact]
    public void BuiltInGeneratorIdsAreUniqueAndCatalogHasNoErrors()
    {
        var catalog = BuiltInGeneratorCatalog.Create();
        var result = CreateValidator().Validate(catalog);

        Assert.Empty(catalog.DuplicateIds);
        Assert.Equal(
            catalog.Manifests.Count,
            catalog.Manifests.Select(manifest => manifest.GeneratorId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.True(result.Ok, JoinDiagnostics(result.Diagnostics));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CurrentStrictLlmGeneratorManifestsExistWithExpectedExecutionProfile()
    {
        var catalog = BuiltInGeneratorCatalog.Create();

        Assert.All(StrictGeneratorIds, generatorId =>
        {
            Assert.True(catalog.TryGet(generatorId, out var manifest));
            Assert.Equal(GeneratorMaturity.Current, manifest.Maturity);
            Assert.True(manifest.UsesLlm);
            Assert.False(manifest.Deterministic);
            Assert.False(manifest.CanRunAtRuntime);
            Assert.Single(manifest.OutputContracts);
        });
    }

    [Fact]
    public void PackageAssemblyAndActivationManifestsExistAndAreOfflineDeterministic()
    {
        var catalog = BuiltInGeneratorCatalog.Create();

        foreach (var generatorId in new[] { "generator.package.assembly_v1", "generator.package.activation_v1" })
        {
            Assert.True(catalog.TryGet(generatorId, out var manifest));
            Assert.Equal(GeneratorMaturity.Current, manifest.Maturity);
            Assert.False(manifest.UsesLlm);
            Assert.True(manifest.Deterministic);
            Assert.True(manifest.CanRunOffline);
            Assert.False(manifest.CanRunAtRuntime);
        }
    }

    [Fact]
    public void PlannedFutureGeneratorManifestsExist()
    {
        var catalog = BuiltInGeneratorCatalog.Create();

        Assert.All(PlannedGeneratorIds, generatorId =>
        {
            Assert.True(catalog.TryGet(generatorId, out var manifest));
            Assert.Equal(GeneratorMaturity.Planned, manifest.Maturity);
            Assert.False(manifest.CanRunAtRuntime);
        });
    }

    [Fact]
    public void CatalogValidatorCatchesDuplicateIds()
    {
        var catalog = new GeneratorCatalog(
        [
            Manifest("generator.test.duplicate"),
            Manifest("GENERATOR.TEST.DUPLICATE")
        ]);

        var result = CreateValidator().Validate(catalog);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == GeneratorCatalogDiagnosticCodes.DuplicateGeneratorId &&
            diagnostic.Severity == GeneratorDiagnosticSeverity.Error);
    }

    [Fact]
    public void CatalogValidatorCatchesUnknownCapabilityReferences()
    {
        var catalog = new GeneratorCatalog(
        [
            Manifest("generator.test.unknown-capability") with
            {
                RequiresCapabilities = ["capability.missing.required"],
                OptionalCapabilities = ["capability.missing.optional"],
                ProvidesCapabilities = ["capability.missing.provided"]
            }
        ]);

        var result = CreateValidator().Validate(catalog);

        Assert.False(result.Ok);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.UnknownRequiredCapability);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.UnknownOptionalCapability);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.UnknownProvidedCapability);
    }

    [Fact]
    public void BaselineGeneratedRpgBlueprintResolvesCurrentGeneratorPlan()
    {
        var result = CreateResolver().Resolve(GetPreset(GameBlueprintPresetProvider.BaselineGeneratedRpgPreview));
        var selectedIds = result.SelectedCurrentGenerators.Select(manifest => manifest.GeneratorId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(StrictGeneratorIds, generatorId => Assert.Contains(generatorId, selectedIds));
        Assert.Contains("generator.package.assembly_v1", selectedIds);
        Assert.Contains("generator.package.activation_v1", selectedIds);
        Assert.Contains("generator.runtime_preview.generated_map_markers_v1", selectedIds);
        Assert.Empty(result.RelatedPlannedGenerators);
        Assert.Empty(result.MissingGeneratorCapabilityIds);
    }

    [Fact]
    public void FutureImportedMapBlueprintReportsPlannedAndMissingSupportWithoutThrowing()
    {
        var result = CreateResolver().Resolve(GetPreset(GameBlueprintPresetProvider.RealisticCitySurvivalImportedMapFuture));
        var plannedIds = result.RelatedPlannedGenerators.Select(manifest => manifest.GeneratorId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(PlannedGeneratorIds, generatorId => Assert.Contains(generatorId, plannedIds));
        Assert.Contains("time.calendar", result.MissingGeneratorCapabilityIds);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.PlannedGeneratorRelated);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == GeneratorCatalogDiagnosticCodes.MissingGeneratorSupport);
    }

    private static GeneratorCatalogValidator CreateValidator()
    {
        return new GeneratorCatalogValidator(BuiltInCapabilityRegistry.Create());
    }

    private static GeneratorPlanResolver CreateResolver()
    {
        return new GeneratorPlanResolver(BuiltInCapabilityRegistry.Create(), BuiltInGeneratorCatalog.Create());
    }

    private static GameBlueprint GetPreset(string presetId)
    {
        Assert.True(new GameBlueprintPresetProvider().TryGet(presetId, out var blueprint));
        return blueprint;
    }

    private static GeneratorModuleManifest Manifest(string id)
    {
        return new GeneratorModuleManifest
        {
            GeneratorId = id,
            Title = id,
            Maturity = GeneratorMaturity.Current,
            Deterministic = true,
            CanRunOffline = true
        };
    }

    private static string JoinDiagnostics(IEnumerable<GeneratorCatalogDiagnostic> diagnostics)
    {
        return string.Join(Environment.NewLine, diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
