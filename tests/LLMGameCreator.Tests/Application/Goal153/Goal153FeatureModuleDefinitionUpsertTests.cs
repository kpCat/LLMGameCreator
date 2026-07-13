using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153;

public sealed class Goal153FeatureModuleDefinitionUpsertTests
{
    private static readonly string[] Goal153Ids =
    [
        "feature.combat.active_ability_loadout",
        "feature.magic.mana_spellcasting",
        "feature.status.turn_effects"
    ];

    [Fact]
    public void Modules_are_default_off_dependency_driven_and_custom_values_materialize_atomically()
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var modules = library.Catalog.Modules.Where(item => Goal153Ids.Contains(item.ModuleId)).ToList();
        Assert.Equal(3, modules.Count);
        Assert.All(modules, item => Assert.False(item.DefaultSelected));
        Assert.Equal(["feature.combat.turn_based_encounter", "feature.player_adapter.runtime_summary"], modules[0].Dependencies);
        Assert.Equal(["feature.combat.active_ability_loadout"], modules[1].Dependencies);
        Assert.Equal(["feature.combat.active_ability_loadout"], modules[2].Dependencies);

        var selected = library.Catalog.Modules.Where(item => item.Required).Select(item => item.ModuleId).Concat(Goal153Ids).ToList();
        var bound = new FeatureModuleParameterBindingService().Bind(library.Catalog, selected,
        [
            Value(Goal153Ids[0], "abilityBaseDamage", 2),
            Value(Goal153Ids[1], "startingMana", 12),
            Value(Goal153Ids[1], "abilityManaCost", 3),
            Value(Goal153Ids[2], "statusDurationTurns", 2),
            Value(Goal153Ids[2], "statusTickDamage", 1)
        ]);
        Assert.True(bound.Passed, string.Join("; ", bound.Diagnostics));

        var baselinePath = Path.Combine(root, ".llmgc", "procedural", "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates", "minimal-map-game-balanced-baseline", "package.json");
        var baseline = File.ReadAllText(baselinePath);
        var forward = new FeatureModulePackageMutationService().Apply(baseline, bound.EffectiveMutationOperations);
        var reverse = new FeatureModulePackageMutationService().Apply(baseline, bound.EffectiveMutationOperations.Reverse().ToList());
        Assert.True(forward.Passed, string.Join("; ", forward.Diagnostics));
        Assert.True(reverse.Passed, string.Join("; ", reverse.Diagnostics));
        Assert.Equal(forward.PackageJson, reverse.PackageJson);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(forward.PackageJson, options)!;
        var ability = package.Game.Abilities.Single(item => item.Id == "ability/arcane_impulse");
        Assert.Equal(2, ability.Power);
        Assert.Equal(2, double.Parse(ability.Effects.Single(item => item.Type == "damage_resource").Args["amount"]));
        Assert.Equal(3, ability.Costs.Single(item => item.Id == "resource/mana").Amount);
        var player = package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel").Participants.Single(item => item.Id == "player");
        Assert.Contains("ability/arcane_impulse", player.Abilities);
        Assert.Equal(12, player.Resources.Single(item => item.Id == "resource/mana").Amount);
        Assert.Equal(1, double.Parse(package.Game.Statuses.Single(item => item.Id == "status/arcane_burn").Effects[0].Args["amount"]));
    }

    [Fact]
    public void Definition_upsert_is_idempotent_and_conflicts_are_causal()
    {
        const string package = "{\"game\":{\"abilities\":[],\"resources\":[],\"statuses\":[]}}";
        var operation = Operation("{\"id\":\"ability/example\",\"name\":\"Пример\"}");
        var service = new FeatureModulePackageMutationService();
        var first = service.Apply(package, [operation]);
        Assert.True(first.Passed);
        var second = service.Apply(first.PackageJson, [operation]);
        Assert.True(second.Passed);
        Assert.False(second.Operations.Single().Applied);

        var conflict = service.Apply(first.PackageJson, [operation with { NewValue = "{\"id\":\"ability/example\",\"name\":\"Конфликт\"}" }]);
        Assert.False(conflict.Passed);
        Assert.Contains("Conflicting definition upsert rejected", conflict.Diagnostics.Single());
    }

    [Fact]
    public void Capability_plan_executes_ability_mana_status_checkpoint_and_full_replay()
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selectedIds = library.Catalog.Modules.Where(item => item.Required).Select(item => item.ModuleId).Concat(Goal153Ids).ToList();
        var selected = library.Catalog.Modules.Where(item => selectedIds.Contains(item.ModuleId)).ToList();
        var bound = new FeatureModuleParameterBindingService().Bind(library.Catalog, selectedIds, []);
        Assert.True(bound.Passed, string.Join("; ", bound.Diagnostics));
        selected = bound.EffectiveCatalog.Modules.Where(item => selectedIds.Contains(item.ModuleId)).ToList();
        var baseline = File.ReadAllText(Path.Combine(root, ".llmgc", "procedural", "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates", "minimal-map-game-balanced-baseline", "package.json"));
        var mutation = new FeatureModulePackageMutationService().Apply(baseline, bound.EffectiveMutationOperations);
        Assert.True(mutation.Passed, string.Join("; ", mutation.Diagnostics));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(mutation.PackageJson, options)!;
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(selected, package);

        var result = new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).Qualify(package,
            new ProductLineRuntimeQualificationRequest
            {
                SessionId = "goal153-session", CandidateId = "goal153", VariantKind = "goal153",
                PackagePath = "in-memory/package.json", PackageSha256 = new string('a', 64),
                CheckpointId = "goal153-checkpoint", FinalCheckpointId = "goal153-final", CapabilityPlan = plan
            });

        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.True(result.ActionDescriptorExecutionBindingPassed);
        var events = result.Session.CanonicalSession.Snapshots.SelectMany(item => item.RuntimeEvents).ToList();
        Assert.Contains(events, item => item.EventType == "AbilityUsed" && item.TargetId == "ability/arcane_impulse");
        Assert.Contains(events, item => item.EventType == "CostConsumed" && item.Args.GetValueOrDefault("after") == "9");
        Assert.Contains(events, item => item.EventType == "StatusTicked" && item.Args.GetValueOrDefault("statusId") == "status/arcane_burn");
        Assert.Contains(events, item => item.EventType == "StatusRemoved" && item.Message.Contains("expired"));
    }

    [Fact]
    public void Normal_composition_service_qualifies_goal153_closure()
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var output = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal153-compose-" + Guid.NewGuid().ToString("N"));
        try
        {
            var qualification = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .ComposeAndQualify(root, library.Catalog, Goal153Ids, output, "goal153-normal-composition", true);
            Assert.True(qualification.Result.Passed,
                string.Join("; ", qualification.Result.Diagnostics.Concat(qualification.Result.SemanticEffects.Observations
                    .Select(item => item.EffectId + "=" + item.Passed + ":" + item.ActualValue + ":" + string.Join("|", item.Diagnostics)))));
            Assert.Equal(3, qualification.Result.SemanticEffects.SatisfiedSelectedModuleCount);
        }
        finally
        {
            if (Directory.Exists(output)) Directory.Delete(output, true);
        }
    }

    private static ProductLineRuntimeVariantMutationOperation Operation(string value) => new()
    {
        OperationId = "goal153.definition", TargetKind = FeatureModulePackageMutationTargetKinds.DefinitionUpsert,
        TargetId = "abilities|ability/example", JsonPath = "game.abilities[id=ability/example]",
        ExpectedValue = "__MISSING_OR_EQUIVALENT__", NewValue = value, RuntimeDimension = "test"
    };

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, decimal value) => new()
    {
        ModuleId = moduleId, ParameterId = parameterId, Value = JsonSerializer.SerializeToElement(value)
    };

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
