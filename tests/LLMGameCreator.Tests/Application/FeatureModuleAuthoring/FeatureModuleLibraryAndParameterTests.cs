using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleAuthoring;

public sealed class FeatureModuleLibraryAndParameterTests
{
    [Fact]
    public void Tracked_library_loads_deterministically_with_seeded_contracts()
    {
        var root = FindRoot();
        var loader = new FeatureModuleLibraryLoader();
        var first = loader.Load(Path.Combine(root, "catalogs", "feature-modules"));
        var second = loader.Load(Path.Combine(root, "catalogs", "feature-modules"));

        Assert.True(first.Validation.Passed);
        Assert.Equal(10, first.Index.RequiredCoreModuleCount);
        Assert.Equal(12, first.Index.OptionalModuleCount);
        Assert.Equal(22, first.Index.ParameterDefinitionCount);
        Assert.Equal(22, first.Index.Modules.Count);
        Assert.Equal(first.CatalogFingerprint, second.CatalogFingerprint);
        Assert.Equal(first.ModuleFingerprints, second.ModuleFingerprints);
        Assert.All(first.Catalog.Modules, module => Assert.Equal("featuremodule_definition_v1", module.SchemaVersion));
        Assert.Equal("2", Operation(first, "alchemy.healing_potion_output_quantity").NewValue);
        Assert.Equal("6", Operation(first, "combat.basic_attack_power").NewValue);
        Assert.Equal("3", Operation(first, "exploration.apple_tree_loot_minimum").NewValue);
    }

    [Fact]
    public void Parameter_binding_resolves_defaults_and_applies_multi_target_groups_atomically()
    {
        var library = Load();
        var selected = OptionalIds(library);
        var service = new FeatureModuleParameterBindingService();
        var defaults = service.Bind(library.Catalog, selected, []);
        var custom = service.Bind(library.Catalog, selected,
        [
            Value("feature.profile.combat_focus", "basicAttackDamage", 9),
            Value("feature.profile.exploration_resource_focus", "appleYield", 5)
        ]);

        Assert.True(defaults.Passed);
        Assert.Equal(22, defaults.EffectiveParameterValues.Count);
        Assert.Equal("6", defaults.EffectiveMutationOperations.Single(item => item.OperationId == "combat.basic_attack_power").NewValue);
        Assert.True(custom.Passed);
        Assert.Equal("9", custom.EffectiveMutationOperations.Single(item => item.OperationId == "combat.basic_attack_power").NewValue);
        Assert.Equal("9", custom.EffectiveMutationOperations.Single(item => item.OperationId == "combat.basic_attack_damage_effect").NewValue);
        Assert.Equal("5", custom.EffectiveMutationOperations.Single(item => item.OperationId == "exploration.apple_tree_loot_minimum").NewValue);
        Assert.Equal("5", custom.EffectiveMutationOperations.Single(item => item.OperationId == "exploration.apple_tree_loot_maximum").NewValue);
        Assert.Contains("combat.basic_attack_damage", custom.AppliedAtomicGroupIds);
        Assert.Contains("exploration.apple_yield", custom.AppliedAtomicGroupIds);
    }

    [Fact]
    public void Parameter_validation_rejects_unknown_unselected_type_range_step_enum_and_duplicates()
    {
        var library = Load();
        var validator = new FeatureModuleParameterValidator();
        var alchemyOnly = new[] { "feature.profile.alchemy_focus" };
        Assert.Contains("unknown parameter", string.Join(";", validator.Validate(library.Catalog, alchemyOnly,
            [Value("feature.profile.alchemy_focus", "unknown", 1)]).Diagnostics));
        Assert.Contains("unselected module parameter", string.Join(";", validator.Validate(library.Catalog, alchemyOnly,
            [Value("feature.profile.combat_focus", "basicAttackDamage", 6)]).Diagnostics));
        Assert.Contains("wrong parameter type", string.Join(";", validator.Validate(library.Catalog, alchemyOnly,
            [Value("feature.profile.alchemy_focus", "healingPotionOutput", "two")]).Diagnostics));
        Assert.Contains("range violation", string.Join(";", validator.Validate(library.Catalog, alchemyOnly,
            [Value("feature.profile.alchemy_focus", "healingPotionOutput", 99)]).Diagnostics));
        var stepped = WithSyntheticParameter(library.Catalog, "number", 0m, 10m, 2m, []);
        Assert.Contains("step violation", string.Join(";", validator.Validate(stepped, alchemyOnly,
            [Value("feature.profile.alchemy_focus", "synthetic", 3)]).Diagnostics));
        var enumerated = WithSyntheticParameter(library.Catalog, "enum", null, null, null, ["low", "high"]);
        Assert.Contains("invalid enum", string.Join(";", validator.Validate(enumerated, alchemyOnly,
            [Value("feature.profile.alchemy_focus", "synthetic", "middle")]).Diagnostics));
        var duplicate = Value("feature.profile.alchemy_focus", "healingPotionOutput", 3);
        Assert.Contains("duplicate parameter", string.Join(";", validator.Validate(library.Catalog, alchemyOnly,
            [duplicate, duplicate]).Diagnostics));
    }

    [Fact]
    public void Parameter_input_order_produces_identical_effective_plan_and_never_mutates_source_modules()
    {
        var library = Load();
        var selected = OptionalIds(library);
        var before = FeatureModuleLibraryLoader.SerializeCanonical(library.Catalog);
        var binder = new FeatureModuleParameterBindingService();
        var values = new[]
        {
            Value("feature.profile.alchemy_focus", "healingPotionOutput", 3),
            Value("feature.profile.combat_focus", "basicAttackDamage", 7),
            Value("feature.profile.exploration_resource_focus", "logYield", 4)
        };
        var first = binder.Bind(library.Catalog, selected, values);
        var second = binder.Bind(library.Catalog, selected, values.AsEnumerable().Reverse().ToList());
        Assert.Equal(JsonSerializer.Serialize(first.EffectiveMutationOperations),
            JsonSerializer.Serialize(second.EffectiveMutationOperations));
        Assert.Equal(before, FeatureModuleLibraryLoader.SerializeCanonical(library.Catalog));
    }

    private static FeatureModuleCatalogDocument WithSyntheticParameter(
        FeatureModuleCatalogDocument catalog, string type, decimal? min, decimal? max, decimal? step,
        IReadOnlyList<string> allowed)
    {
        return catalog with
        {
            Modules = catalog.Modules.Select(module => module.ModuleId == "feature.profile.alchemy_focus"
                ? module with
                {
                    ParameterDefinitions = module.ParameterDefinitions.Append(new FeatureModuleParameterDefinition
                    {
                        ParameterId = "synthetic",
                        ModuleId = module.ModuleId,
                        Title = "Synthetic",
                        ValueType = type,
                        Required = true,
                        DefaultValue = type == "enum" ? JsonSerializer.SerializeToElement("low") : JsonSerializer.SerializeToElement(0),
                        Minimum = min,
                        Maximum = max,
                        Step = step,
                        AllowedValues = allowed,
                        AuthoringControl = type == "enum" ? "combo_box" : "numeric_up_down"
                    }).ToList()
                }
                : module).ToList()
        };
    }

    internal static FeatureModuleParameterValue Value<T>(string moduleId, string parameterId, T value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    internal static FeatureModuleLibrarySnapshot Load()
    {
        var root = FindRoot();
        return new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
    }

    internal static IReadOnlyList<string> OptionalIds(FeatureModuleLibrarySnapshot library) =>
        library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
            .Select(module => module.ModuleId).OrderBy(id => id, StringComparer.Ordinal).ToList();

    private static LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection.ProductLineRuntimeVariantMutationOperation Operation(
        FeatureModuleLibrarySnapshot library, string operationId) => library.Catalog.Modules
        .SelectMany(module => module.MutationOperations).Single(item => item.OperationId == operationId);

    internal static string FindRoot()
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
