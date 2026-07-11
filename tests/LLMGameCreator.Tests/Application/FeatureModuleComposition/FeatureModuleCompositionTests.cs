using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleComposition;

public sealed class FeatureModuleCompositionTests
{
    [Fact]
    public void Catalog_has_locked_core_and_imported_goal142_optional_module_lineage()
    {
        var root = FindRoot();
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, FeatureModuleCompositionVocabulary.Goal142Root);

        Assert.Equal(10, catalog.RequiredCoreModuleCount);
        Assert.Equal(3, catalog.OptionalProfileModuleCount);
        Assert.All(catalog.Modules.Where(module => module.Required), module => Assert.False(module.Selectable));
        Assert.All(catalog.Modules.Where(module => module.Selectable), module =>
        {
            Assert.StartsWith("feature.profile.", module.ModuleId, StringComparison.Ordinal);
            Assert.NotEmpty(module.MutationOperations);
            Assert.NotEmpty(module.SourceLineage.RecipeId);
            Assert.Equal(module.MutationOperations.Count, module.SourceLineage.OperationIds.Count);
        });
    }

    [Fact]
    public void Validator_rejects_unknown_duplicate_missing_required_dependency_conflict_and_override()
    {
        var root = FindRoot();
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, FeatureModuleCompositionVocabulary.Goal142Root);
        var validator = new FeatureModuleCompositionValidator();
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        var optional = catalog.Modules.Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();
        var alchemy = optional[0].ModuleId;
        var combat = optional[1].ModuleId;

        Assert.False(validator.Validate(catalog, required.Append("unknown").ToList()).AllModuleIdsExist);
        Assert.False(validator.Validate(catalog, required.Concat([alchemy, alchemy]).ToList()).ModuleIdsUnique);
        Assert.False(validator.Validate(catalog, required.Skip(1).ToList()).RequiredModulesSelected);
        Assert.False(validator.Validate(catalog,
            required.Where(id => id != "feature.crafting.recipes").Append(alchemy).ToList()).DependenciesSatisfied);
        var conflictCatalog = catalog with
        {
            Modules = catalog.Modules.Select(module => module.ModuleId == alchemy
                ? module with { Conflicts = [combat] }
                : module).ToList()
        };
        Assert.False(validator.Validate(conflictCatalog, required.Concat([alchemy, combat]).ToList()).ConflictsAbsent);
        Assert.False(validator.Validate(catalog, required,
            new Dictionary<string, string> { ["unsupported"] = "1" }).ParameterOverridesSupported);
    }

    [Fact]
    public async Task Service_materializes_and_qualifies_all_eight_novel_compositions()
    {
        var root = FindRoot();
        var service = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        var result = await service.RunAndWriteAsync(root);

        Assert.Equal(8, result.Matrix.CompositionCount);
        Assert.Equal(8, result.Matrix.PassedCompositionCount);
        Assert.Equal(8, result.Matrix.DistinctPackageSha256Count);
        Assert.Equal(8, result.Matrix.DistinctFinalStateHashCount);
        Assert.Equal(4, result.Matrix.MultiModuleCompositionCount);
        Assert.Equal(FeatureModuleCompositionCoverageModes.ExhaustiveSmallCatalog, result.Matrix.CoveragePlan.CoverageMode);
        Assert.True(result.Matrix.CoveragePlan.FullPowersetEnumerated);
        Assert.True(result.Matrix.AllOrderIndependenceProofsPassed);
        Assert.True(result.Matrix.AllCheckpointReloadsPassed);
        Assert.True(result.Matrix.AllFullReplaysEquivalent);
        Assert.True(result.Matrix.AllActionBindingsPassed);
        Assert.True(result.Matrix.SameRuntimeQualifierUsedForGoal145AndGoal146);
        Assert.Equal(3, result.Selection.SelectedOptionalModuleIds.Count);
        Assert.Equal(3, result.Selection.SemanticEffects.Count);
        Assert.False(result.Dashboard.Accepted);
        Assert.True(result.Dashboard.ManualReviewDeferred);
        var expected = new Dictionary<string, (string Package, string Final)>(StringComparer.Ordinal)
        {
            ["minimal-map-game-composed-alchemy"] = ("faa35f6b608042b8a8a9b52ca0bd282af4504dfb53d8775b7e261a26402082f6", "652bdaaee90703ff36a361de7a5553d76403d549a8f9cdae63585d3fa2bacd72"),
            ["minimal-map-game-composed-alchemy-combat"] = ("ba18c0aa4e792c4ab05784ea6f9a5235cffe76ab19852f2d077d5a3080110142", "8f5fb25e2f2063aafc4702b71d193b35502931dd4975732610480c7bbee4a112"),
            ["minimal-map-game-composed-alchemy-combat-exploration"] = ("9a83d47e8e2ae541e7789b804c32f489acb8e7525c0a9dc32a7cc8be8822d65a", "d5ad29ee7c350918681c2859b80f5d2944834a6414918a16d8b4e1c0746753b9"),
            ["minimal-map-game-composed-alchemy-exploration"] = ("dfc3b0c2e48f2e3425156257d84f454cc3e69ccf0fb9f103cb3da24f69301a36", "2661c01856324dedf7a8f0652672fa18f79ce1d65f513a1a0bc4658c833dbab3"),
            ["minimal-map-game-composed-baseline"] = ("5170d610379d818b2ff55535e1fac0e5ee98f26d8e039e9bc1054bfdea87fa49", "29c99098d25aa934b72a06063d82b5bf44b6454cb7195a178ef959a6224b95c2"),
            ["minimal-map-game-composed-combat"] = ("e156f9f356013dc5f1c515a6ce5f1b1610e2656604e71e793835a10076f9364d", "adf6785cf7f9984587c3ed007392d26dcd4fef1ca041053fdc1b7e613dcf2fc7"),
            ["minimal-map-game-composed-combat-exploration"] = ("655e47603a203b49d1e4318a514f9c1bb0714be5a490a7fb2a5cab62dff0037c", "b9326775d8925dc2857327e2611a9b7df3f7c922eb78ea2052656a4f6c6e257c"),
            ["minimal-map-game-composed-exploration"] = ("5a59c2b552ea56f53a660550c8de2f55cf105c0914b2a2296f8ad507f9e34aa7", "d7c04179cb76ca48ba9694905e491bead014c0f56f446f66331becd5e3211e54")
        };
        Assert.All(result.Matrix.Compositions, row =>
        {
            Assert.Equal(expected[row.CompositionId].Package, row.PackageSha256);
            Assert.Equal(expected[row.CompositionId].Final, row.FinalStateHash);
        });
    }

    [Fact]
    public void Coverage_planner_is_bounded_for_four_and_twelve_optional_modules()
    {
        var catalog = FeatureModuleCatalog.LoadFromGoal142(FindRoot(), FeatureModuleCompositionVocabulary.Goal142Root);
        var fourth = SyntheticFuelModule();
        var fourCatalog = AppendOptional(catalog, fourth);
        var selected = new[] { Optional(fourCatalog)[0].ModuleId, fourth.ModuleId };
        var planner = new FeatureModuleCompositionCoveragePlanner();
        var four = planner.Plan(fourCatalog, selected);

        Assert.Equal(FeatureModuleCompositionCoverageModes.BoundedInteractionCoverage, four.CoverageMode);
        Assert.False(four.FullPowersetEnumerated);
        Assert.True(four.GeneratedCompositionCount < 16);
        Assert.True(four.SelectedCompositionIncluded);

        var twelveModules = Enumerable.Range(0, 12).Select(index => new FeatureModuleDefinition
        {
            ModuleId = "feature.synthetic.module_" + index.ToString("00"),
            Title = "Synthetic Module " + index.ToString("00"),
            Category = "synthetic",
            ModuleKind = "test",
            Selectable = true
        }).ToList();
        var twelveCatalog = catalog with
        {
            OptionalProfileModuleCount = twelveModules.Count,
            Modules = catalog.Modules.Where(module => module.Required).Concat(twelveModules).ToList()
        };
        var policy = new FeatureModuleCompositionCoveragePolicy();
        var twelveSelected = new[] { twelveModules[0].ModuleId, twelveModules[^1].ModuleId };
        var first = planner.Plan(twelveCatalog, twelveSelected, policy);
        var second = planner.Plan(twelveCatalog, twelveSelected, policy);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

        Assert.False(first.FullPowersetEnumerated);
        Assert.True(first.GeneratedCompositionCount <= policy.MaxTotalRows);
        Assert.True(first.GeneratedCompositionCount < 4096 / 10);
        Assert.Equal(JsonSerializer.Serialize(first, options), JsonSerializer.Serialize(second, options));
        var constrained = planner.Plan(
            twelveCatalog,
            twelveSelected,
            policy with { MaxTotalRows = 10 });
        Assert.True(constrained.GeneratedCompositionCount <= 10);
        Assert.True(constrained.SelectedCompositionIncluded);
    }

    [Fact]
    public void Synthetic_fourth_module_materializes_qualifies_replays_and_observes_declared_effect()
    {
        var root = FindRoot();
        var sourcePath = Path.Combine(root, "src", "LLMGameCreator.Application", "Design", "FeatureModuleComposition", "FeatureModuleCompositionService.cs");
        var sourceHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath)));
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, FeatureModuleCompositionVocabulary.Goal142Root);
        var synthetic = SyntheticFuelModule();
        catalog = AppendOptional(catalog, synthetic);
        var selected = new[] { Optional(catalog)[0].ModuleId, synthetic.ModuleId };
        var output = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal146a-synthetic-" + Guid.NewGuid().ToString("N"));
        try
        {
            var qualification = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .ComposeAndQualify(root, catalog, selected, output);
            var syntheticObservation = Assert.Single(qualification.Artifacts.SemanticEffects.Observations,
                observation => observation.ModuleId == synthetic.ModuleId);

            Assert.Contains("synthetic-fuel-reserve", qualification.Result.CompositionId, StringComparison.Ordinal);
            Assert.True(qualification.Result.PackageValidationPassed);
            Assert.True(qualification.Result.CheckpointReloadPassed);
            Assert.True(qualification.Result.FullReplayEquivalent);
            Assert.True(qualification.Result.ActionBindingsPassed);
            Assert.True(syntheticObservation.Passed);
            Assert.Equal("2", syntheticObservation.ActualValue);
            Assert.Equal(sourceHash, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath))));
        }
        finally
        {
            if (Directory.Exists(output)) Directory.Delete(output, true);
        }
    }

    internal static FeatureModuleDefinition SyntheticFuelModule() => new()
    {
        ModuleId = "feature.profile.synthetic_fuel_reserve",
        Title = "Synthetic Fuel Reserve",
        Category = "profile",
        ModuleKind = "synthetic_test",
        Selectable = true,
        Dependencies = ["feature.inventory.basic"],
        MutationOperations =
        [
            new ProductLineRuntimeVariantMutationOperation
            {
                OperationId = "synthetic.fuel_reserve",
                TargetKind = "inventory_stack_amount",
                TargetId = "inventory/player_start|item/fuel_can",
                JsonPath = "$.game.inventories[id=inventory/player_start].stacks[itemId=item/fuel_can].amount",
                ExpectedValue = "1",
                NewValue = "2",
                RuntimeDimension = "synthetic_fuel_reserve"
            }
        ],
        RuntimeEffectContracts =
        [
            new FeatureModuleRuntimeEffectContract
            {
                EffectId = "runtime_effect.synthetic_fuel_reserve",
                ModuleId = "feature.profile.synthetic_fuel_reserve",
                MetricKind = FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity,
                TargetId = "inventory/player_start",
                ResourceOrItemId = "item/fuel_can",
                ComparisonKind = FeatureModuleRuntimeEffectComparisonKinds.GreaterThanBaseline,
                ExpectedValue = "2",
                SourceOperationIds = ["synthetic.fuel_reserve"],
                RuntimeDimension = "synthetic_fuel_reserve"
            }
        ]
    };

    internal static FeatureModuleCatalogDocument AppendOptional(
        FeatureModuleCatalogDocument catalog,
        FeatureModuleDefinition module) => catalog with
    {
        OptionalProfileModuleCount = catalog.OptionalProfileModuleCount + 1,
        Modules = catalog.Modules.Append(module).OrderBy(item => item.ModuleId, StringComparer.Ordinal).ToList()
    };

    internal static IReadOnlyList<FeatureModuleDefinition> Optional(FeatureModuleCatalogDocument catalog) =>
        catalog.Modules.Where(module => module.Selectable && !module.Required)
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal).ToList();

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
