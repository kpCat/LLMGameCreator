using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal167;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168RelationshipBindingOverlayTests
{
    [Fact]
    public void Behavioral_relationship_identity_is_exact_dialogue_id()
    {
        Assert.All(Goal168RelationshipFixture.Binding.Bindings, item =>
        {
            Assert.Equal(item.DialogueId, item.RelationshipId);
            Assert.Equal(item.DialogueId, item.DecisionFlagId);
        });
    }

    [Fact]
    public void Behavioral_each_quest_is_assigned_at_most_once()
    {
        var ids = Goal168RelationshipFixture.Binding.Bindings
            .SelectMany(item => item.QuestArc).Select(item => item.QuestId)
            .ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Behavioral_actor_specific_encounter_assignment_is_preferred()
    {
        foreach (var relationship in Goal168RelationshipFixture.Binding.Bindings)
        foreach (var step in relationship.QuestArc)
        {
            var seed = Goal168RelationshipFixture.Source.RegeneratedPlan!.EncounterSeeds
                .Single(item => Goal167TestKit.SourceIdMatches(
                    item.EncounterSeedId, step.TargetEncounterSourceId));
            var eligibleActorExists = seed.ActorSeedIds.Any(actorId =>
                Goal168RelationshipFixture.Binding.Bindings.Any(item =>
                    Goal167TestKit.SourceIdMatches(actorId, item.ActorSeedId)
                    && item.FactionId == relationship.FactionId));
            if (eligibleActorExists)
                Assert.Contains(seed.ActorSeedIds, actorId =>
                    Goal167TestKit.SourceIdMatches(actorId,
                        relationship.ActorSeedId));
        }
    }

    [Fact]
    public void Behavioral_faction_region_fallback_is_deterministic()
    {
        var rebuilt = Goal168RelationshipFixture.Rebind();
        Assert.Equal(Goal164TestKit.Canonical(
                Goal168RelationshipFixture.Binding.Bindings),
            Goal164TestKit.Canonical(rebuilt.Bindings));
    }

    [Fact]
    public void Behavioral_arbitrary_quest_count_is_supported()
    {
        var multi = Goal168RelationshipFixture.MultiArc();
        Assert.True(multi.Result.Passed,
            string.Join(Environment.NewLine, multi.Result.Diagnostics));
        Assert.Equal(2, multi.Result.Document.Bindings.Single().QuestArc.Count);
    }

    [Fact]
    public void Behavioral_arc_order_uses_distance_encounter_and_source()
    {
        Assert.All(Goal168RelationshipFixture.Binding.Bindings, relationship =>
        {
            var ordered = relationship.QuestArc.OrderBy(item => item.RegionDistance)
                .ThenBy(item => item.TargetEncounterSourceId, StringComparer.Ordinal)
                .ThenBy(item => item.QuestSourceId, StringComparer.Ordinal)
                .Select(item => item.QuestId);
            Assert.Equal(ordered, relationship.QuestArc.Select(item => item.QuestId));
        });
    }

    [Fact]
    public void Behavioral_no_valid_quest_means_no_support_arc()
    {
        var binding = Goal168RelationshipFixture.Binding with
        {
            Bindings = Goal168RelationshipFixture.Binding.Bindings.Select(item =>
                item with { QuestArc = [] }).ToList()
        };
        var overlay = new GeneratedCampaignRelationshipOverlayService().Build(
            Goal168RelationshipFixture.ChoicePackage, binding);
        Assert.True(overlay.Passed,
            string.Join(Environment.NewLine, overlay.Diagnostics));
        Assert.Equal(0, overlay.Document.RelationshipCount);
        Assert.Equal(0, overlay.Document.ArcQuestCount);
    }

    [Fact]
    public void Behavioral_challenge_and_refuse_remain_independent()
    {
        Assert.All(Goal168RelationshipFixture.Binding.Bindings.Where(item =>
            item.QuestArc.Count > 0), item =>
        {
            Assert.Contains(GeneratedCampaignRelationshipBranch.REFUSE,
                item.Branches);
            if (!string.IsNullOrWhiteSpace(item.ChallengeEncounterId))
                Assert.Contains(GeneratedCampaignRelationshipBranch.CHALLENGE,
                    item.Branches);
        });
    }

    [Fact]
    public void Behavioral_assigned_quests_do_not_auto_start()
    {
        var assigned = Goal168RelationshipFixture.Overlay.Document.Bindings
            .SelectMany(item => item.QuestArc).Select(item => item.QuestId)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(Goal168RelationshipFixture.Overlay.RelationshipOverlayPackage
            .Game.Quests.Where(item => assigned.Contains(item.Id)),
            item => Assert.False(item.AutoStart));
    }

    [Fact]
    public void Behavioral_unassigned_quest_autostart_is_preserved()
    {
        var assigned = Goal168RelationshipFixture.Overlay.Document.Bindings
            .SelectMany(item => item.QuestArc).Select(item => item.QuestId)
            .ToHashSet(StringComparer.Ordinal);
        var before = Goal168RelationshipFixture.ChoicePackage.Game.Quests
            .Where(item => !assigned.Contains(item.Id))
            .ToDictionary(item => item.Id, item => item.AutoStart,
                StringComparer.Ordinal);
        Assert.All(Goal168RelationshipFixture.Overlay.RelationshipOverlayPackage
            .Game.Quests.Where(item => before.ContainsKey(item.Id)),
            item => Assert.Equal(before[item.Id], item.AutoStart));
    }

    [Fact]
    public void Behavioral_support_starts_first_arc_quest()
    {
        foreach (var relationship in Goal168RelationshipFixture.Overlay.Document
                     .Bindings)
        {
            var dialogue = Goal168RelationshipFixture.Overlay
                .RelationshipOverlayPackage.Game.Dialogues.Single(item =>
                    item.Id == relationship.DialogueId);
            var support = dialogue.Nodes.SelectMany(item => item.Choices)
                .Single(item =>
                    item.Metadata.GetValueOrDefault("generatedChoiceKind")
                    == "SUPPORT"
                    && item.Metadata.GetValueOrDefault("generatedChoicePhase")
                    == "initial");
            Assert.Equal(relationship.QuestArc[0].QuestId,
                support.StartQuestId);
        }
    }

    [Fact]
    public void Behavioral_next_followup_starts_next_arc_quest()
    {
        var multi = Goal168RelationshipFixture.MultiArc();
        var relationship = multi.Result.Document.Bindings.Single();
        var dialogue = multi.Result.RelationshipOverlayPackage.Game.Dialogues
            .Single(item => item.Id == relationship.DialogueId);
        Assert.Contains(dialogue.Nodes.SelectMany(item => item.Choices), item =>
            item.StartQuestId == relationship.QuestArc[1].QuestId
            && item.Metadata.GetValueOrDefault(
                "generatedRelationshipArcOrder") == "1");
    }

    [Fact]
    public void Behavioral_final_followup_starts_no_quest()
    {
        var multi = Goal168RelationshipFixture.MultiArc();
        var dialogue = multi.Result.RelationshipOverlayPackage.Game.Dialogues
            .Single(item => item.Id ==
                multi.Result.Document.Bindings.Single().DialogueId);
        var final = dialogue.Nodes.SelectMany(item => item.Choices).Single(item =>
            item.Metadata.GetValueOrDefault("generatedRelationshipPhase")
            == "followup/completed");
        Assert.Null(final.StartQuestId);
    }

    [Fact]
    public void Behavioral_controlled_delta_is_limited_to_declared_fields() =>
        Assert.True(Goal168RelationshipFixture.Overlay.Document
            .ControlledDeltaPassed);

    [Fact]
    public void Behavioral_quest_source_objectives_and_rewards_are_preserved()
    {
        var assigned = Goal168RelationshipFixture.Overlay.Document.Bindings
            .SelectMany(item => item.QuestArc).Select(item => item.QuestId);
        foreach (var id in assigned)
        {
            var before = Goal168RelationshipFixture.ChoicePackage.Game.Quests
                .Single(item => item.Id == id);
            var after = Goal168RelationshipFixture.Overlay
                .RelationshipOverlayPackage.Game.Quests.Single(item =>
                    item.Id == id);
            Assert.Equal(Goal164TestKit.Canonical(before.Objectives),
                Goal164TestKit.Canonical(after.Objectives));
            Assert.Equal(Goal164TestKit.Canonical(before.Rewards),
                Goal164TestKit.Canonical(after.Rewards));
            Assert.Equal(before.Metadata["sourceQuestEventSeedId"],
                after.Metadata["sourceQuestEventSeedId"]);
        }
    }

    [Fact]
    public void Behavioral_independent_overlay_build_is_deterministic()
    {
        var rebuilt = new GeneratedCampaignRelationshipOverlayService().Build(
            Goal164TestKit.Clone(Goal168RelationshipFixture.ChoicePackage),
            Goal168RelationshipFixture.Rebind());
        Assert.Equal(Goal168RelationshipFixture.Overlay.Document.OutputPackageSha256,
            rebuilt.Document.OutputPackageSha256);
        Assert.Equal(Goal168RelationshipFixture.Overlay.Document.InventorySha256,
            rebuilt.Document.InventorySha256);
    }

    [Fact]
    public void Behavioral_reordered_binding_input_is_deterministic()
    {
        var reversed = Goal168RelationshipFixture.Binding with
        {
            Bindings = Goal168RelationshipFixture.Binding.Bindings.Reverse()
                .ToList()
        };
        var rebuilt = new GeneratedCampaignRelationshipOverlayService().Build(
            Goal164TestKit.Clone(Goal168RelationshipFixture.ChoicePackage),
            reversed);
        Assert.Equal(Goal168RelationshipFixture.Overlay
                .RelationshipOverlayPackageJson,
            rebuilt.RelationshipOverlayPackageJson);
    }

    [Fact]
    public void Behavioral_forbidden_delta_is_rejected()
    {
        var tampered = Goal164TestKit.Clone(Goal168RelationshipFixture.Overlay
            .RelationshipOverlayPackage);
        tampered.Game.Items[0].Name += " changed";
        var validation = new GeneratedCampaignRelationshipOverlayService()
            .ValidateOverlayPackage(Goal168RelationshipFixture.ChoicePackage,
                tampered, Goal168RelationshipFixture.Overlay.Document);
        Assert.False(validation.Passed);
        Assert.Contains(
            "generated_relationship.delta_non_dialogue_quest_changed",
            validation.Diagnostics);
    }
}

internal static class Goal168RelationshipFixture
{
    private static readonly Lazy<GeneratedCampaignRelationshipBindingResult>
        BindingLazy = new(Rebind);
    private static readonly Lazy<GeneratedCampaignRelationshipOverlayResult>
        OverlayLazy = new(() =>
            new GeneratedCampaignRelationshipOverlayService().Build(
                Goal164TestKit.Clone(ChoicePackage), Binding));

    internal static SeededGeneratedProjectSourceValidationResult Source =>
        Goal167TestKit.Source.Source;
    internal static LLMGameCreator.GamePackage.GamePackageDefinition
        ChoicePackage => Goal167TestKit.Source.Overlay.ChoiceOverlayPackage;
    internal static GeneratedCampaignRelationshipBindingResult Binding =>
        BindingLazy.Value;
    internal static GeneratedCampaignRelationshipOverlayResult Overlay =>
        OverlayLazy.Value;

    internal static GeneratedCampaignRelationshipBindingResult Rebind() =>
        new GeneratedCampaignRelationshipBindingService().Bind(
            Source, Goal164TestKit.Clone(ChoicePackage),
            new GeneratedCampaignChoiceBindingService().Bind(
                Source, Goal164TestKit.Clone(ChoicePackage)));

    internal static MultiArcOverlayFixture MultiArc()
    {
        var package = Goal164TestKit.Clone(ChoicePackage);
        var relationship = Binding.Bindings.First(item =>
            item.QuestArc.Count > 0);
        var first = relationship.QuestArc[0];
        var sourceQuest = package.Game.Quests.Single(item =>
            item.Id == first.QuestId);
        var copy = Goal164TestKit.Clone(sourceQuest);
        copy.Id += "/goal168-second";
        copy.Title += " II";
        copy.Metadata["sourceQuestEventSeedId"] += "/goal168-second";
        package.Game.Quests.Add(copy);
        var second = first with
        {
            Order = 1,
            QuestId = copy.Id,
            QuestSourceId =
                copy.Metadata["sourceQuestEventSeedId"]
        };
        var binding = new GeneratedCampaignRelationshipBindingResult
        {
            Passed = true,
            Bindings =
            [
                relationship with
                {
                    QuestArc = [first with { Order = 0 }, second]
                }
            ]
        };
        var result = new GeneratedCampaignRelationshipOverlayService().Build(
            package, binding);
        return new MultiArcOverlayFixture(package, result);
    }
}

internal sealed record MultiArcOverlayFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition SourcePackage,
    GeneratedCampaignRelationshipOverlayResult Result);
