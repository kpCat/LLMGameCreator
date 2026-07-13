using System.Text.Json;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154B;

public sealed class Goal154BParameterAndMutationTests
{
    [Fact]
    public void Behavioral_all_five_parameters_bind_min_default_max_and_interior_values()
    {
        var (library, selected) = Catalog();
        var definitions = library.Catalog.Modules.Where(module => Goal154BFixture.SocialModuleIds.Contains(module.ModuleId))
            .SelectMany(module => module.ParameterDefinitions).ToList();
        Assert.Equal(5, definitions.Count);

        foreach (var definition in definitions)
        foreach (var value in new[]
                 {
                     definition.Minimum!.Value,
                     definition.DefaultValue.GetDecimal(),
                     definition.Maximum!.Value,
                     definition.Minimum.Value + ((definition.Maximum.Value - definition.Minimum.Value) / 2m)
                 }.Distinct())
        {
            var result = new FeatureModuleParameterBindingService().Bind(library.Catalog, selected,
                [Goal154BFixture.Value(definition.ModuleId, definition.ParameterId, decimal.Truncate(value))]);
            Assert.True(result.Passed, definition.ParameterId + "=" + value + ":" + string.Join("; ", result.Diagnostics));
        }
    }

    [Fact]
    public void Behavioral_parameter_binding_rejects_below_above_fractional_unknown_and_unselected_values()
    {
        var (library, selected) = Catalog();
        var definitions = library.Catalog.Modules.Where(module => Goal154BFixture.SocialModuleIds.Contains(module.ModuleId))
            .SelectMany(module => module.ParameterDefinitions).ToList();
        foreach (var definition in definitions)
        {
            Assert.False(Bind(library, selected, Goal154BFixture.Value(definition.ModuleId, definition.ParameterId,
                definition.Minimum!.Value - 1)).Passed);
            Assert.False(Bind(library, selected, Goal154BFixture.Value(definition.ModuleId, definition.ParameterId,
                definition.Maximum!.Value + 1)).Passed);
            Assert.False(Bind(library, selected, Goal154BFixture.Value(definition.ModuleId, definition.ParameterId,
                definition.DefaultValue.GetDecimal() + 0.5m)).Passed);
        }

        Assert.False(Bind(library, selected,
            Goal154BFixture.Value(Goal154BFixture.FactionModuleId, "unknownParameter", 1)).Passed);
        var requiredOnly = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        Assert.False(Bind(library, requiredOnly,
            Goal154BFixture.Value(Goal154BFixture.FactionModuleId, "startingReputation", 1)).Passed);
    }

    [Fact]
    public void Behavioral_threshold_above_start_plus_reward_is_a_valid_still_locked_product_outcome()
    {
        var fixture = Goal154BFixture.Create(startingReputation: 0, questReputationReward: 10,
            trustedReputationThreshold: 20);
        var result = fixture.Qualify("parameter-still-locked");

        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.Equal("SKIPPED", result.Session.ActionJournal.Single(item => item.ActionId == "claim_trusted_reward").Status);
        Assert.Contains("socialOutcome=still_locked", result.Session.LatestSnapshot.SocialSummary, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_nested_social_mutations_are_additive_idempotent_and_order_independent()
    {
        var fixture = Goal154BFixture.Create();
        var service = new FeatureModulePackageMutationService();
        var first = service.Apply(fixture.BasePackageJson, fixture.Binding.EffectiveMutationOperations);
        var second = service.Apply(first.PackageJson, fixture.Binding.EffectiveMutationOperations);
        var reverse = service.Apply(fixture.BasePackageJson, fixture.Binding.EffectiveMutationOperations.Reverse().ToList());

        Assert.True(first.Passed, string.Join("; ", first.Diagnostics));
        Assert.True(second.Passed, string.Join("; ", second.Diagnostics));
        Assert.True(reverse.Passed, string.Join("; ", reverse.Diagnostics));
        Assert.Equal(first.PackageJson, second.PackageJson);
        Assert.Equal(first.PackageJson, reverse.PackageJson);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.QuestOutputAmount);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.QuestFailureOutputUpsert);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.QuestFailureOutputAmount);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.DialogueChoiceUpsert);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.DialogueChoiceRequirementAmount);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.DialogueChoiceRewardAmount);
        Assert.Contains(first.Operations, item => item.TargetKind == FeatureModulePackageMutationTargetKinds.DefinitionNumericProperty);
    }

    [Fact]
    public void Behavioral_mutation_conflicts_wrong_owners_and_duplicate_targets_preserve_input_bytes()
    {
        var fixture = Goal154BFixture.Create();
        var service = new FeatureModulePackageMutationService();
        foreach (var operation in fixture.Binding.EffectiveMutationOperations.Where(operation =>
                     operation.OperationId.StartsWith("quest.", StringComparison.Ordinal)
                     || operation.OperationId.StartsWith("dialogue.", StringComparison.Ordinal)
                     || operation.OperationId.StartsWith("faction.", StringComparison.Ordinal)))
        {
            var wrong = operation with { TargetId = WrongOwner(operation.TargetId) };
            var rejected = service.Apply(fixture.BasePackageJson, [wrong]);
            Assert.False(rejected.Passed, operation.OperationId);
            Assert.Equal(fixture.BasePackageJson, rejected.PackageJson);
        }

        var numeric = fixture.Binding.EffectiveMutationOperations.Single(item => item.OperationId == "quest.00_completion_reputation");
        var conflict = service.Apply(fixture.PackageJson, [numeric with { NewValue = "11" }]);
        Assert.False(conflict.Passed);
        Assert.Equal(fixture.PackageJson, conflict.PackageJson);

        foreach (var duplicateKind in new[] { "faction", "quest", "dialogue" })
        {
            var package = Goal154BFixture.ClonePackage(fixture.Package);
            var operation = duplicateKind switch
            {
                "faction" => fixture.Binding.EffectiveMutationOperations.Single(item => item.OperationId == "faction.10_starting_reputation"),
                "quest" => fixture.Binding.EffectiveMutationOperations.Single(item => item.OperationId == "quest.00_completion_reputation"),
                _ => fixture.Binding.EffectiveMutationOperations.Single(item => item.OperationId == "dialogue.00_trusted_choice")
            };
            if (duplicateKind == "faction") package.Game.Factions.Add(Goal154BFixture.ClonePackage(fixture.Package).Game.Factions.Single(item => item.Id == "faction/village"));
            if (duplicateKind == "quest") package.Game.Quests.Add(Goal154BFixture.ClonePackage(fixture.Package).Game.Quests.Single(item => item.Id == "quest/help_healer"));
            if (duplicateKind == "dialogue") package.Game.Dialogues.Add(Goal154BFixture.ClonePackage(fixture.Package).Game.Dialogues.Single(item => item.Id == "dialogue/healer"));
            var json = Goal154BFixture.Serialize(package);
            var rejected = service.Apply(json, [operation]);
            Assert.False(rejected.Passed, duplicateKind);
            Assert.Equal(json, rejected.PackageJson);
        }
    }

    [Fact]
    public void Behavioral_activated_package_diff_contains_only_declared_existing_social_content()
    {
        var fixture = Goal154BFixture.Create();
        var baseline = Goal154BFixture.Deserialize(fixture.BasePackageJson);
        var package = fixture.Package;

        Assert.Equal(baseline.Game.Factions.Count, package.Game.Factions.Count);
        Assert.Equal(baseline.Game.Quests.Count, package.Game.Quests.Count);
        Assert.Equal(baseline.Game.Dialogues.Count, package.Game.Dialogues.Count);
        Assert.Equal(baseline.Game.Inventories.Count, package.Game.Inventories.Count);
        Assert.Equal(baseline.Game.Maps.SelectMany(map => map.Entities).Count(), package.Game.Maps.SelectMany(map => map.Entities).Count());
        Assert.Equal(10, package.Game.Quests.Single(item => item.Id == "quest/help_healer").Rewards
            .Single(item => item.Kind == "resource" && item.Id == "resource/gold").Amount);
        Assert.Single(package.Game.Dialogues.Single(item => item.Id == "dialogue/healer").Nodes
            .Single(item => item.Id == "start").Choices, item => item.Id == "trusted_village_reward");
        var text = fixture.PackageJson.ToLowerInvariant();
        Assert.DoesNotContain("dummy faction", text);
        Assert.DoesNotContain("dummy quest", text);
        Assert.DoesNotContain("proof npc", text);
        Assert.DoesNotContain("qualification inventory", text);
        Assert.DoesNotContain("goal154b", text);
    }

    [Fact]
    public void Behavioral_generic_package_validation_rejects_ambiguous_and_broken_social_references()
    {
        var fixture = Goal154BFixture.Create();
        var invalidPackages = new List<LLMGameCreator.GamePackage.GamePackageDefinition>();

        var duplicateChoice = Goal154BFixture.ClonePackage(fixture.Package);
        var node = duplicateChoice.Game.Dialogues.Single(item => item.Id == "dialogue/healer").Nodes.Single(item => item.Id == "start");
        node.Choices.Add(Goal154BFixture.ClonePackage(fixture.Package).Game.Dialogues.Single(item => item.Id == "dialogue/healer")
            .Nodes.Single(item => item.Id == "start").Choices.Single(item => item.Id == "trusted_village_reward"));
        invalidPackages.Add(duplicateChoice);

        var missingTarget = Goal154BFixture.ClonePackage(fixture.Package);
        missingTarget.Game.Dialogues.Single(item => item.Id == "dialogue/healer").Nodes.Single(item => item.Id == "start")
            .Choices.Single(item => item.Id == "accept").TargetNodeId = "missing";
        invalidPackages.Add(missingTarget);

        var missingFactionOutput = Goal154BFixture.ClonePackage(fixture.Package);
        missingFactionOutput.Game.Quests.Single(item => item.Id == "quest/help_healer").Rewards
            .Single(item => item.Kind == "reputation").Id = "faction/missing";
        invalidPackages.Add(missingFactionOutput);

        var missingFactionRequirement = Goal154BFixture.ClonePackage(fixture.Package);
        missingFactionRequirement.Game.Dialogues.Single(item => item.Id == "dialogue/healer").Nodes.Single(item => item.Id == "start")
            .Choices.Single(item => item.Id == "trusted_village_reward").Requirements
            .Single(item => item.Kind == "reputation_at_least").Id = "faction/missing";
        invalidPackages.Add(missingFactionRequirement);

        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();
        Assert.All(invalidPackages, package => Assert.False(planner.TryPlan(fixture.Modules, package).Passed));
    }

    private static FeatureModuleParameterBindingResult Bind(
        FeatureModuleLibrarySnapshot library,
        IReadOnlyList<string> selected,
        FeatureModuleParameterValue value) =>
        new FeatureModuleParameterBindingService().Bind(library.Catalog, selected, [value]);

    private static (FeatureModuleLibrarySnapshot Library, IReadOnlyList<string> Selected) Catalog()
    {
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(Goal154BFixture.FindRoot(), "catalogs", "feature-modules"));
        var selected = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId)
            .Concat(Goal154BFixture.SocialModuleIds).ToList();
        return (library, selected);
    }

    private static string WrongOwner(string targetId)
    {
        var parts = targetId.Split('|');
        if (parts[0] == "factions") parts[1] = "faction/missing";
        else parts[0] = parts[0].StartsWith("quest/", StringComparison.Ordinal) ? "quest/missing" : "dialogue/missing";
        return string.Join("|", parts);
    }
}
