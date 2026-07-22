using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Tests.Application.Goal156;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167ChoiceBindingOverlayTests
{
    [Fact]
    public void Behavioral_binding_uses_exact_generated_dialogue_provenance()
    {
        var fixture = Goal167TestKit.Source;
        Assert.True(fixture.Binding.Passed, string.Join(Environment.NewLine, fixture.Binding.Diagnostics));
        Assert.All(fixture.Binding.Bindings, binding =>
        {
            Assert.Contains(fixture.Source.Overlay!.GeneratedRecords, row => row.CollectionPath == "game.dialogues"
                && row.RecordId == binding.DialogueId);
            Assert.Single(fixture.Package.Game.Dialogues, dialogue => dialogue.Id == binding.DialogueId
                && dialogue.Metadata.GetValueOrDefault("sourceActorSeedId") == binding.ActorSeedId);
        });
    }

    [Fact]
    public void Behavioral_binding_never_double_prefixes_namespaced_actor_ids() =>
        Assert.DoesNotContain(Goal167TestKit.Source.Binding.Bindings,
            binding => binding.ActorSeedId.Contains("generated/generated/", StringComparison.Ordinal));

    [Fact]
    public void Behavioral_missing_generated_dialogue_is_rejected()
    {
        var fixture = Goal167TestKit.Source;
        var package = Goal164TestKit.Clone(fixture.Package);
        package.Game.Dialogues.RemoveAll(item => item.Id == fixture.Binding.Bindings[0].DialogueId);
        var result = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source, package);
        Assert.False(result.Passed);
        Assert.Contains("generated_choice.dialogue_mapping_missing", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_duplicate_generated_dialogue_is_rejected()
    {
        var fixture = Goal167TestKit.Source;
        var package = Goal164TestKit.Clone(fixture.Package);
        package.Game.Dialogues.Add(Goal164TestKit.Clone(package.Game.Dialogues.Single(item =>
            item.Id == fixture.Binding.Bindings[0].DialogueId)));
        var result = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source, package);
        Assert.False(result.Passed);
        Assert.Contains("generated_choice.dialogue_mapping_duplicate", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_actor_entity_is_exactly_bound_through_dialogue_component()
    {
        var fixture = Goal167TestKit.Source;
        Assert.All(fixture.Binding.Bindings, binding => Assert.Single(
            fixture.Package.Game.Maps.SelectMany(map => map.Entities), entity => entity.Id == binding.ActorEntityId
            && entity.Components.Any(component => component.Args.GetValueOrDefault("dialogueId") == binding.DialogueId)));
    }

    [Fact]
    public void Behavioral_missing_actor_entity_is_rejected()
    {
        var fixture = Goal167TestKit.Source;
        var package = Goal164TestKit.Clone(fixture.Package);
        foreach (var map in package.Game.Maps) map.Entities.RemoveAll(item => item.Id == fixture.Binding.Bindings[0].ActorEntityId);
        var result = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source, package);
        Assert.False(result.Passed);
        Assert.Contains("generated_choice.actor_entity_missing", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_interaction_is_exactly_bound()
    {
        var fixture = Goal167TestKit.Source;
        Assert.All(fixture.Binding.Bindings, binding => Assert.Single(fixture.Package.Game.Interactions,
            interaction => interaction.Id == binding.InteractionId));
    }

    [Fact]
    public void Behavioral_missing_interaction_is_rejected()
    {
        var fixture = Goal167TestKit.Source;
        var package = Goal164TestKit.Clone(fixture.Package);
        package.Game.Interactions.RemoveAll(item => item.Id == fixture.Binding.Bindings[0].InteractionId);
        var result = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source, package);
        Assert.False(result.Passed);
        Assert.Contains("generated_choice.interaction_missing", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_support_relationship_is_data_derived()
    {
        var fixture = Goal167TestKit.Source;
        var branches = fixture.Binding.Bindings.SelectMany(item => item.Branches)
            .Where(item => item.Kind == GeneratedCampaignBranchKind.SUPPORT).ToList();
        Assert.NotEmpty(branches);
        Assert.All(branches, branch =>
        {
            var quest = Assert.Single(fixture.Package.Game.Quests, item => item.Id == branch.QuestId);
            var reward = Assert.Single(quest.Rewards, item => item.Id == branch.FactionId
                && item.Kind.Contains("reputation", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(Math.Abs(reward.Amount), branch.ReputationAmount);
        });
    }

    [Fact]
    public void Behavioral_challenge_prefers_actor_specific_encounter_when_available()
    {
        var fixture = Goal167TestKit.Source;
        foreach (var binding in fixture.Binding.Bindings.Where(item => item.Branches.Any(branch =>
                     branch.Kind == GeneratedCampaignBranchKind.CHALLENGE)))
        {
            var actorSeed = Goal167TestKit.PlanActorId(fixture, binding.ActorSeedId);
            var challenge = binding.Branches.Single(branch => branch.Kind == GeneratedCampaignBranchKind.CHALLENGE);
            var sourceId = fixture.Package.Game.Encounters.Single(item => item.Id == challenge.EncounterId)
                .Metadata["sourceEncounterSeedId"];
            var selected = fixture.Source.RegeneratedPlan!.EncounterSeeds.Single(item =>
                Goal167TestKit.SourceIdMatches(item.EncounterSeedId, sourceId));
            var actorSpecific = fixture.Source.RegeneratedPlan.EncounterSeeds.Where(item =>
                item.RegionId == selected.RegionId && item.ActorSeedIds.Contains(actorSeed, StringComparer.Ordinal)).ToList();
            if (actorSpecific.Count > 0) Assert.Contains(actorSeed, selected.ActorSeedIds);
        }
    }

    [Fact]
    public void Behavioral_refuse_relationship_is_exact_negative_support_magnitude()
    {
        foreach (var binding in Goal167TestKit.Source.Binding.Bindings)
        {
            var support = binding.Branches.SingleOrDefault(item => item.Kind == GeneratedCampaignBranchKind.SUPPORT);
            var refuse = binding.Branches.SingleOrDefault(item => item.Kind == GeneratedCampaignBranchKind.REFUSE);
            if (support is null) continue;
            Assert.NotNull(refuse);
            Assert.Equal(-support.ReputationAmount, refuse!.ReputationAmount);
            Assert.Equal(support.QuestId, refuse.QuestId);
        }
    }

    [Fact]
    public void Behavioral_actor_without_relationships_remains_minimal()
    {
        var fixture = Goal167TestKit.Source;
        var package = Goal164TestKit.Clone(fixture.Package);
        package.Game.Quests = [];
        package.Game.Encounters = [];
        package.GeneratedContent.Quests = [];
        package.GeneratedContent.Encounters = [];
        var result = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source, package);
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.All(result.Bindings, item =>
        {
            Assert.Empty(item.Branches);
            Assert.Equal("NO_BRANCH_RELATIONSHIP", item.Status);
        });
    }

    [Fact]
    public void Behavioral_overlay_flag_id_is_exact_dialogue_id()
    {
        var fixture = Goal167TestKit.Source;
        foreach (var binding in fixture.Binding.Bindings.Where(item => item.Branches.Count > 0))
        {
            var dialogue = fixture.Overlay.ChoiceOverlayPackage.Game.Dialogues.Single(item => item.Id == binding.DialogueId);
            var choices = dialogue.Nodes.Single(item => item.Id == dialogue.StartNodeId).Choices
                .Where(item => item.Metadata.GetValueOrDefault("generatedChoicePhase") == "initial");
            Assert.All(choices, choice => Assert.Contains(choice.Effects, effect => effect.Type == "set_flag"
                && effect.Args.GetValueOrDefault("id") == dialogue.Id));
        }
    }

    [Fact]
    public void Behavioral_initial_choices_require_empty_branch_flag()
    {
        var fixture = Goal167TestKit.Source;
        Assert.All(Goal167TestKit.InitialChoices(fixture.Overlay.ChoiceOverlayPackage), choice =>
            Assert.Contains(choice.Requirements, requirement => requirement.Kind == "flag_equals"
                && requirement.Value == string.Empty));
    }

    [Fact]
    public void Behavioral_followups_require_exact_branch_value()
    {
        var fixture = Goal167TestKit.Source;
        Assert.All(Goal167TestKit.FollowUpChoices(fixture.Overlay.ChoiceOverlayPackage), choice =>
            Assert.Contains(choice.Requirements, requirement => requirement.Kind == "flag_equals"
                && requirement.Value == choice.Metadata["generatedChoiceKind"]));
    }

    [Fact]
    public void Behavioral_original_generated_placeholder_is_replaced()
    {
        var fixture = Goal167TestKit.Source;
        Assert.DoesNotContain(fixture.Overlay.ChoiceOverlayPackage.Game.Dialogues
            .Where(dialogue => fixture.Binding.Bindings.Any(binding => binding.DialogueId == dialogue.Id
                && binding.Branches.Count > 0)).SelectMany(dialogue => dialogue.Nodes).SelectMany(node => node.Choices),
            choice => choice.Id == "close");
    }

    [Fact]
    public void Behavioral_non_choice_dialogue_content_is_preserved()
    {
        var fixture = Goal167TestKit.Source;
        var binding = fixture.Binding.Bindings.First(item => item.Branches.Count > 0);
        var before = fixture.Package.Game.Dialogues.Single(item => item.Id == binding.DialogueId);
        var after = fixture.Overlay.ChoiceOverlayPackage.Game.Dialogues.Single(item => item.Id == binding.DialogueId);
        Assert.Equal(before.Title, after.Title);
        Assert.Equal(before.Nodes.Single(item => item.Id == before.StartNodeId).Text,
            after.Nodes.Single(item => item.Id == after.StartNodeId).Text);
    }

    [Fact]
    public void Behavioral_non_generated_dialogues_are_byte_identical()
    {
        var fixture = Goal167TestKit.Source;
        var bound = fixture.Binding.Bindings.Select(item => item.DialogueId).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(Goal164TestKit.Canonical(fixture.Package.Game.Dialogues.Where(item => !bound.Contains(item.Id))
                .OrderBy(item => item.Id, StringComparer.Ordinal).ToList()),
            Goal164TestKit.Canonical(fixture.Overlay.ChoiceOverlayPackage.Game.Dialogues.Where(item => !bound.Contains(item.Id))
                .OrderBy(item => item.Id, StringComparer.Ordinal).ToList()));
    }

    [Fact]
    public void Behavioral_non_dialogue_collections_are_byte_identical()
    {
        var fixture = Goal167TestKit.Source;
        var before = Goal164TestKit.Clone(fixture.Package);
        var after = Goal164TestKit.Clone(fixture.Overlay.ChoiceOverlayPackage);
        before.Game.Dialogues = [];
        after.Game.Dialogues = [];
        Assert.Equal(Goal164TestKit.Canonical(before), Goal164TestKit.Canonical(after));
    }

    [Fact]
    public void Behavioral_definition_counts_are_unchanged() =>
        Assert.Equal(Goal167TestKit.Source.Overlay.Document.DefinitionCollectionCountsBefore,
            Goal167TestKit.Source.Overlay.Document.DefinitionCollectionCountsAfter);

    [Fact]
    public void Behavioral_independent_overlay_rebuild_is_deterministic()
    {
        var fixture = Goal167TestKit.Source;
        var rebuiltBinding = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source,
            Goal164TestKit.Clone(fixture.Package));
        var rebuilt = new GeneratedCampaignChoiceOverlayService().Build(
            Goal164TestKit.Clone(fixture.Package), rebuiltBinding);
        Assert.Equal(fixture.Overlay.ChoiceOverlayPackageJson, rebuilt.ChoiceOverlayPackageJson);
        Assert.Equal(fixture.Overlay.Document.OutputPackageSha256, rebuilt.Document.OutputPackageSha256);
    }

    [Fact]
    public void Behavioral_reordered_dialogue_input_has_same_canonical_output()
    {
        var fixture = Goal167TestKit.Source;
        var package = Goal164TestKit.Clone(fixture.Package);
        package.Game.Dialogues.Reverse();
        var binding = new GeneratedCampaignChoiceBindingService().Bind(fixture.Source, package);
        var overlay = new GeneratedCampaignChoiceOverlayService().Build(package, binding);
        Assert.Equal(fixture.Overlay.Document.OutputPackageSha256, overlay.Document.OutputPackageSha256);
    }

    [Fact]
    public void Behavioral_forbidden_non_dialogue_delta_is_rejected()
    {
        var fixture = Goal167TestKit.Source;
        var tampered = Goal164TestKit.Clone(fixture.Overlay.ChoiceOverlayPackage);
        tampered.Game.Items[0].Name += " tampered";
        var validation = new GeneratedCampaignChoiceOverlayService().ValidateFinalPackage(
            fixture.Package, tampered, fixture.Overlay.Document);
        Assert.False(validation.Passed);
        Assert.Contains("generated_choice.delta_non_dialogue_changed", validation.Diagnostics);
    }

    [Fact]
    public void Behavioral_overlay_flag_inventory_matches_bound_branch_kinds()
    {
        var fixture = Goal167TestKit.Source;
        Assert.Equal(fixture.Binding.Bindings.Count(item => item.Branches.Count > 0),
            fixture.Overlay.Document.FlagInventory.Count);
        Assert.All(fixture.Overlay.Document.FlagInventory, row =>
            Assert.Equal(fixture.Binding.Bindings.Single(item => item.DialogueId == row.DialogueId).Branches
                    .Select(item => item.Kind).Distinct().OrderBy(item => item).ToList(),
                row.SupportedBranchKinds));
    }
}

internal static class Goal167TestKit
{
    private static readonly Lazy<Goal167ChoiceSourceFixture> SourceLazy = new(Goal167ChoiceSourceFixture.Create);
    public static Goal167ChoiceSourceFixture Source => SourceLazy.Value;

    public static IReadOnlyList<LLMGameCreator.Domain.Definitions.DialogueChoiceDefinition> InitialChoices(
        GamePackageDefinition package) => package.Game.Dialogues.SelectMany(item => item.Nodes)
        .SelectMany(item => item.Choices).Where(item =>
            item.Metadata.GetValueOrDefault("generatedChoicePhase") == "initial").ToList();

    public static IReadOnlyList<LLMGameCreator.Domain.Definitions.DialogueChoiceDefinition> FollowUpChoices(
        GamePackageDefinition package) => package.Game.Dialogues.SelectMany(item => item.Nodes)
        .SelectMany(item => item.Choices).Where(item =>
            (item.Metadata.GetValueOrDefault("generatedChoicePhase") ?? string.Empty)
            .StartsWith("followup/", StringComparison.Ordinal)).ToList();

    public static bool SourceIdMatches(string sourceId, string mappedId) => string.Equals(sourceId, mappedId,
        StringComparison.Ordinal) || (!sourceId.StartsWith("generated/", StringComparison.Ordinal)
        && !sourceId.StartsWith("seeded_generated_project/", StringComparison.Ordinal)
        && string.Equals("generated/" + sourceId, mappedId, StringComparison.Ordinal));

    public static string PlanActorId(Goal167ChoiceSourceFixture fixture, string mappedId) =>
        fixture.Source.RegeneratedPlan!.ActorSeeds.Single(item => SourceIdMatches(item.ActorSeedId, mappedId)).ActorSeedId;
}

internal sealed record Goal167ChoiceSourceFixture(
    SeededGeneratedProjectSourceValidationResult Source,
    GamePackageDefinition Package,
    GeneratedCampaignChoiceBindingResult Binding,
    GeneratedCampaignChoiceOverlayResult Overlay)
{
    public static Goal167ChoiceSourceFixture Create()
    {
        var source = Goal156TestKit.SourceService.Validate(Goal156TestKit.AllSelectable.Path);
        var package = Goal164TestKit.Clone(source.GeneratedBasePackage
            ?? throw new InvalidOperationException("goal167.generated_base_missing"));
        var binding = new GeneratedCampaignChoiceBindingService().Bind(source, package);
        var overlay = new GeneratedCampaignChoiceOverlayService().Build(package, binding);
        return new Goal167ChoiceSourceFixture(source, package, binding, overlay);
    }
}
