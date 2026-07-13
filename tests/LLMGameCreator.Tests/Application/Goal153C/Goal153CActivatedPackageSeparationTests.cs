using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153C;

public sealed class Goal153CActivatedPackageSeparationTests
{
    [Fact]
    public void Activated_package_contains_only_declared_product_mutations_and_real_hostile_target()
    {
        var fixture = Goal153CFixture.Create();
        Assert.True(fixture.Library.Validation.Passed, string.Join("; ", fixture.Library.Validation.Diagnostics));
        var modules = Goal153CFixture.GoalModuleIds.Select(id => fixture.Modules.Single(module => module.ModuleId == id)).ToList();
        Assert.All(modules, module => Assert.Equal("1.1.0", module.ModuleVersion));
        Assert.DoesNotContain(modules.SelectMany(module => module.MutationOperations), operation =>
            operation.OperationId is "active.02_training_target" or "active.02a_training_target_health_capacity");
        Assert.DoesNotContain(modules.SelectMany(module => module.SourceLineage.OperationIds), operationId =>
            operationId is "active.02_training_target" or "active.02a_training_target_health_capacity");

        var package = fixture.Package;
        var health = package.Game.Resources.Single(resource => resource.Id == "resource/health");
        Assert.Equal(30, health.DefaultValue);
        Assert.Equal(0, health.MinValue);
        Assert.Equal(30, health.MaxValue);
        Assert.DoesNotContain(package.Game.Encounters.SelectMany(encounter => encounter.Participants), participant =>
            participant.Id == "goal153_target" || participant.Name == "Магическая мишень");
        Assert.Contains("ability/arcane_impulse", package.Game.Encounters.Single(encounter =>
            encounter.Id == "encounter/goblin_duel").Participants.Single(participant => participant.Id == "player").Abilities);

        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules, package);
        var ability = plan.OrderedActions.Single(action => action.ActionId == "use_arcane_impulse");
        Assert.Equal("hostile_encounter_participant", ability.TargetSelector);
        Assert.Equal("goblin", ability.ResolvedTargetId);
        Assert.Equal("goblin", ability.Args["targetParticipantId"]);

        var claims = modules.SelectMany(module => module.RequiredValidationRules
            .Where(rule => rule.StartsWith("activated_package_diff:", StringComparison.Ordinal))).ToList();
        Assert.Equal(modules.SelectMany(module => module.MutationOperations).Select(operation => operation.RuntimeDimension)
            .Distinct(StringComparer.Ordinal).Count(), claims.Count);
        Assert.DoesNotContain(claims, claim => claim.EndsWith(":forbidden_qualification_proof_fixture", StringComparison.Ordinal));
    }
}

internal sealed record Goal153CFixture(
    FeatureModuleLibrarySnapshot Library,
    GamePackageDefinition Package,
    IReadOnlyList<FeatureModuleDefinition> Modules)
{
    internal static readonly string[] GoalModuleIds =
    [
        "feature.combat.active_ability_loadout",
        "feature.magic.mana_spellcasting",
        "feature.status.turn_effects"
    ];

    internal static Goal153CFixture Create(
        int abilityDamage = 2,
        int startingMana = 12,
        int manaCost = 3,
        int duration = 5,
        int tickDamage = 1)
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selectedIds = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
            .Concat(GoalModuleIds).ToList();
        var bound = new FeatureModuleParameterBindingService().Bind(library.Catalog, selectedIds,
        [
            Value(GoalModuleIds[0], "abilityBaseDamage", abilityDamage),
            Value(GoalModuleIds[1], "startingMana", startingMana),
            Value(GoalModuleIds[1], "abilityManaCost", manaCost),
            Value(GoalModuleIds[2], "statusDurationTurns", duration),
            Value(GoalModuleIds[2], "statusTickDamage", tickDamage)
        ]);
        Assert.True(bound.Passed, string.Join("; ", bound.Diagnostics));
        var baseline = File.ReadAllText(Path.Combine(root, ".llmgc", "procedural",
            "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates",
            "minimal-map-game-balanced-baseline", "package.json"));
        var mutation = new FeatureModulePackageMutationService().Apply(baseline, bound.EffectiveMutationOperations);
        Assert.True(mutation.Passed, string.Join("; ", mutation.Diagnostics));
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(mutation.PackageJson, JsonOptions())!;
        var modules = bound.EffectiveCatalog.Modules.Where(module => selectedIds.Contains(module.ModuleId)).ToList();
        return new Goal153CFixture(library, package, modules);
    }

    internal static JsonSerializerOptions JsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    internal static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, int value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };
}
