using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153B;

public sealed class Goal153BManaDomainIntegrityTests
{
    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(1, 1, true)]
    [InlineData(12, 3, true)]
    [InlineData(1000, 1000, true)]
    [InlineData(2, 3, false)]
    public void Mana_parameter_domain_and_declarative_boundary_are_validated(int starting, int cost, bool expected)
    {
        var library = Load();
        var module = library.Catalog.Modules.Single(item => item.ModuleId == "feature.magic.mana_spellcasting");
        var result = new FeatureModuleParameterBindingService().Bind(library.Catalog, Selected(library),
            [Value(module.ModuleId, "startingMana", starting), Value(module.ModuleId, "abilityManaCost", cost)]);
        Assert.Equal(expected, result.Passed);
        if (!expected)
        {
            Assert.Contains(result.Diagnostics, item => item.StartsWith("parameter_binding.mana_cost_not_above_starting:", StringComparison.Ordinal)
                                                    && item.Contains("left=" + cost, StringComparison.Ordinal)
                                                    && item.Contains("right=" + starting, StringComparison.Ordinal));
            return;
        }

        var package = Materialize(result);
        var definition = package.Game.Resources.Single(item => item.Id == "resource/mana");
        var amount = package.Game.Encounters.Single().Participants.Single(item => item.Id == "player").Resources
            .Single(item => item.Id == "resource/mana").Amount;
        Assert.InRange(amount, definition.MinValue!.Value, definition.MaxValue!.Value);
        Assert.Equal(starting, amount);
        Assert.True(definition.MaxValue >= 1000);
    }

    [Fact]
    public void Participant_resource_domains_are_rejected_before_runtime_and_unrelated_resources_remain_valid()
    {
        var package = Materialize(new FeatureModuleParameterBindingService().Bind(Load().Catalog, Selected(Load()),
            [Value("feature.magic.mana_spellcasting", "startingMana", 12), Value("feature.magic.mana_spellcasting", "abilityManaCost", 3)]));
        var validator = new CapabilityDrivenRuntimePlaythroughValidator();
        Assert.DoesNotContain(validator.Validate([], package).Diagnostics,
            item => item.Contains("participant resource", StringComparison.Ordinal));

        var above = Copy(package);
        above.Game.Encounters.Single().Participants.Single(item => item.Id == "player").Resources.Single(item => item.Id == "resource/mana").Amount = 1001;
        Assert.Contains(validator.Validate([], above).Diagnostics, item => item.Contains("above maximum", StringComparison.Ordinal));

        var below = Copy(package);
        below.Game.Encounters.Single().Participants.Single(item => item.Id == "player").Resources.Single(item => item.Id == "resource/mana").Amount = -1;
        Assert.Contains(validator.Validate([], below).Diagnostics, item => item.Contains("below minimum", StringComparison.Ordinal));

        var missing = Copy(package);
        missing.Game.Encounters.Single().Participants.Single(item => item.Id == "player").Resources.Single(item => item.Id == "resource/mana").Id = "resource/missing";
        Assert.Contains(validator.Validate([], missing).Diagnostics, item => item.Contains("definition rejected", StringComparison.Ordinal));
    }

    [Fact]
    public void Qualification_domain_is_derived_from_declarations_and_rejects_fixture_that_is_too_small()
    {
        var library = Load();
        decimal Maximum(string moduleId, string parameterId) => library.Catalog.Modules.Single(module => module.ModuleId == moduleId)
            .ParameterDefinitions.Single(parameter => parameter.ParameterId == parameterId).Maximum!.Value;
        var ability = Maximum("feature.combat.active_ability_loadout", "abilityBaseDamage");
        var tick = Maximum("feature.status.turn_effects", "statusTickDamage");
        var duration = Maximum("feature.status.turn_effects", "statusDurationTurns");
        var package = Materialize(new FeatureModuleParameterBindingService().Bind(library.Catalog, Selected(library), []));
        var target = (decimal)package.Game.Encounters.Single().Participants.Single(item => item.Id == "goal153_target").Resources
            .Single(item => item.Id == "resource/health").Amount;
        var required = checked(ability + checked(tick * duration));
        Assert.True(target > required);
        Assert.False(target > checked((target + 1) + checked(tick * duration)));
    }

    private static GamePackageDefinition Materialize(FeatureModuleParameterBindingResult result)
    {
        Assert.True(result.Passed, string.Join("; ", result.Diagnostics));
        var root = FindRoot();
        var baseline = File.ReadAllText(Path.Combine(root, ".llmgc", "procedural",
            "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates",
            "minimal-map-game-balanced-baseline", "package.json"));
        var mutated = new FeatureModulePackageMutationService().Apply(baseline, result.EffectiveMutationOperations);
        Assert.True(mutated.Passed, string.Join("; ", mutated.Diagnostics));
        return JsonSerializer.Deserialize<GamePackageDefinition>(mutated.PackageJson, JsonOptions())!;
    }

    private static GamePackageDefinition Copy(GamePackageDefinition source) => JsonSerializer.Deserialize<GamePackageDefinition>(
        JsonSerializer.Serialize(source, JsonOptions()), JsonOptions())!;

    private static FeatureModuleLibrarySnapshot Load() => new FeatureModuleLibraryLoader().Load(Path.Combine(
        FindRoot(), "catalogs", "feature-modules"));

    private static IReadOnlyList<string> Selected(FeatureModuleLibrarySnapshot library) => library.Catalog.Modules
        .Where(module => module.Required || module.ModuleId is "feature.combat.active_ability_loadout"
            or "feature.magic.mana_spellcasting" or "feature.status.turn_effects")
        .Select(module => module.ModuleId).ToList();

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    private static JsonSerializerOptions JsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
