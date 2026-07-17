using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164GeneratedCombatOverlayTests
{
    [Fact]
    public void Behavioral_every_plan_encounter_binds_through_exact_provenance()
    {
        var fixture = Overlay();

        Assert.True(fixture.Binding.Passed, string.Join(",", fixture.Binding.Diagnostics));
        Assert.Equal(fixture.Build.Source.RegeneratedPlan!.EncounterSeeds.Count,
            fixture.Binding.Bindings.Count);
        Assert.All(fixture.Binding.Bindings, binding =>
            Assert.StartsWith("generated/", binding.GeneratedContentSourceId, StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_overlay_changes_only_bound_generated_encounters()
    {
        var fixture = Overlay();
        var generated = fixture.Binding.Bindings.Select(item => item.PackageEncounterId)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(fixture.Result.Passed, string.Join(",", fixture.Result.Diagnostics));
        foreach (var before in fixture.Build.LaneAPackage.Game.Encounters.Where(item => !generated.Contains(item.Id)))
            Assert.Equal(Goal164TestKit.Canonical(before), Goal164TestKit.Canonical(
                fixture.Result.CombatOverlayPackage.Game.Encounters.Single(item => item.Id == before.Id)));
    }

    [Fact]
    public void Behavioral_overlay_preserves_encounter_identity_reward_and_provenance()
    {
        var fixture = Overlay();
        foreach (var binding in fixture.Binding.Bindings)
        {
            var before = fixture.Build.LaneAPackage.Game.Encounters.Single(item => item.Id == binding.PackageEncounterId);
            var after = fixture.Result.CombatOverlayPackage.Game.Encounters.Single(item => item.Id == binding.PackageEncounterId);
            Assert.Equal(before.Id, after.Id);
            Assert.Equal(before.Name, after.Name);
            Assert.Equal(before.Kind, after.Kind);
            Assert.Equal(Goal164TestKit.Canonical(before.Rewards), Goal164TestKit.Canonical(after.Rewards));
            Assert.Equal(Goal164TestKit.Canonical(before.Metadata), Goal164TestKit.Canonical(after.Metadata));
            Assert.Equal(Goal164TestKit.Canonical(before.Tags), Goal164TestKit.Canonical(after.Tags));
        }
    }

    [Fact]
    public void Behavioral_overlay_preserves_participant_identity_fields()
    {
        var fixture = Overlay();
        foreach (var binding in fixture.Binding.Bindings)
        {
            var before = fixture.Build.LaneAPackage.Game.Encounters.Single(item => item.Id == binding.PackageEncounterId);
            var after = fixture.Result.CombatOverlayPackage.Game.Encounters.Single(item => item.Id == binding.PackageEncounterId);
            foreach (var participant in before.Participants)
            {
                var actual = after.Participants.Single(item => item.Id == participant.Id);
                Assert.Equal(participant.Name, actual.Name);
                Assert.Equal(participant.Kind, actual.Kind);
                Assert.Equal(participant.Team, actual.Team);
                Assert.Equal(participant.FactionId, actual.FactionId);
                Assert.Equal(participant.EntityPrototypeId, actual.EntityPrototypeId);
            }
        }
    }

    [Fact]
    public void Behavioral_overlay_assigns_lane_a_role_to_every_participant()
    {
        var fixture = Overlay();
        var contract = fixture.Build.Contract.Contract!;
        foreach (var encounter in fixture.Result.CombatOverlayPackage.Game.Encounters.Where(item =>
                     fixture.Binding.Bindings.Any(binding => binding.PackageEncounterId == item.Id)))
        foreach (var participant in encounter.Participants)
        {
            var role = participant.Team.Equals("player", StringComparison.OrdinalIgnoreCase)
                ? contract.PlayerRole : contract.OpponentRole;
            Assert.Equal(role.Abilities, participant.Abilities);
            Assert.Equal(Goal164TestKit.Canonical(role.Resources), Goal164TestKit.Canonical(participant.Resources));
        }
    }

    [Fact]
    public void Behavioral_overlay_adds_or_removes_no_definitions()
    {
        var fixture = Overlay();

        Assert.Equal(fixture.Result.Document.DefinitionCollectionCountsBefore,
            fixture.Result.Document.DefinitionCollectionCountsAfter);
        Assert.Equal(fixture.Build.LaneAPackage.Game.Abilities.Count,
            fixture.Result.CombatOverlayPackage.Game.Abilities.Count);
        Assert.Equal(fixture.Build.LaneAPackage.Game.Resources.Count,
            fixture.Result.CombatOverlayPackage.Game.Resources.Count);
    }

    [Fact]
    public void Behavioral_generated_data_only_definitions_remain_but_are_not_combat_roles()
    {
        var fixture = Overlay();
        var generatedAbilities = GeneratedIds(fixture.Build.Source.Overlay!, "game.abilities");
        var generatedResources = GeneratedIds(fixture.Build.Source.Overlay!, "game.resources");
        var participants = fixture.Result.CombatOverlayPackage.Game.Encounters.Where(item =>
                fixture.Binding.Bindings.Any(binding => binding.PackageEncounterId == item.Id))
            .SelectMany(item => item.Participants).ToList();

        Assert.All(generatedAbilities, id => Assert.Contains(fixture.Result.CombatOverlayPackage.Game.Abilities,
            item => item.Id == id));
        Assert.All(generatedResources, id => Assert.Contains(fixture.Result.CombatOverlayPackage.Game.Resources,
            item => item.Id == id));
        Assert.DoesNotContain(participants.SelectMany(item => item.Abilities), generatedAbilities.Contains);
        Assert.DoesNotContain(participants.SelectMany(item => item.Resources).Select(item => item.Id),
            generatedResources.Contains);
    }

    [Fact]
    public void Behavioral_overlay_is_deterministic_under_generated_array_reordering()
    {
        var fixture = Overlay();
        var reordered = Goal164TestKit.Clone(fixture.Build.LaneAPackage);
        var ids = fixture.Binding.Bindings.Select(item => item.PackageEncounterId).ToHashSet(StringComparer.Ordinal);
        var reversed = new Queue<EncounterDefinition>(reordered.Game.Encounters.Where(item => ids.Contains(item.Id)).Reverse());
        reordered.Game.Encounters = reordered.Game.Encounters.Select(item => ids.Contains(item.Id)
            ? reversed.Dequeue() : item).ToList();
        foreach (var encounter in reordered.Game.Encounters.Where(item => ids.Contains(item.Id)))
            encounter.Participants.Reverse();
        var binding = new GeneratedEncounterCombatBindingService().Bind(
            fixture.Build.Source, reordered, fixture.Build.Contract.Contract!);
        var repeated = new GeneratedWorldEncounterCombatOverlayService().Build(
            reordered, fixture.Build.Contract.Contract!, binding);

        Assert.True(repeated.Passed, string.Join(",", repeated.Diagnostics));
        Assert.Equal(fixture.Result.Document.OutputPackageSha256, repeated.Document.OutputPackageSha256);
        Assert.Equal(Goal164TestKit.Canonical(fixture.Result.Document), Goal164TestKit.Canonical(repeated.Document));
    }

    private static IReadOnlySet<string> GeneratedIds(GeneratedProjectOverlayDocument overlay, string collection) =>
        overlay.GeneratedRecords.Where(item => item.CollectionPath == collection).Select(item => item.RecordId)
            .ToHashSet(StringComparer.Ordinal);

    private static Goal164OverlayFixture Overlay()
    {
        var build = Goal164TestKit.AllSelectable;
        var contract = Assert.IsType<GeneratedEncounterCombatContract>(build.Contract.Contract);
        var binding = new GeneratedEncounterCombatBindingService().Bind(build.Source, build.LaneAPackage, contract);
        var result = new GeneratedWorldEncounterCombatOverlayService().Build(build.LaneAPackage, contract, binding);
        return new Goal164OverlayFixture(build, binding, result);
    }
}

internal sealed record Goal164OverlayFixture(
    Goal164BuildFixture Build,
    GeneratedEncounterCombatBindingResult Binding,
    GeneratedWorldEncounterCombatOverlayResult Result);
