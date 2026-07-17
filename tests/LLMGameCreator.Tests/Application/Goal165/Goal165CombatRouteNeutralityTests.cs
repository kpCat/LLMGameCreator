using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal165;

[Collection(Goal160Collection.Name)]
public sealed class Goal165CombatRouteNeutralityTests
{
    [Fact]
    public void Behavioral_both_route_contract_infers_both()
    {
        var contract = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Both).Contract!;

        Assert.Equal(GeneratedEncounterCombatRouteMode.BOTH, contract.QualificationSummary.RouteMode);
    }

    [Fact]
    public void Behavioral_basic_only_contract_infers_basic_attack_only()
    {
        var contract = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.BasicOnly).Contract!;

        Assert.Equal(GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY, contract.QualificationSummary.RouteMode);
        Assert.True(contract.QualificationSummary.BasicAttackRequired);
        Assert.False(contract.QualificationSummary.PackageAbilityRequired);
    }

    [Fact]
    public void Behavioral_ability_only_contract_infers_package_ability_only()
    {
        var contract = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.AbilityOnly).Contract!;

        Assert.Equal(GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY, contract.QualificationSummary.RouteMode);
        Assert.False(contract.QualificationSummary.BasicAttackRequired);
        Assert.True(contract.QualificationSummary.PackageAbilityRequired);
    }

    [Fact]
    public void Behavioral_neither_player_route_is_rejected_without_fallback()
    {
        var result = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Neither);

        Assert.False(result.Passed);
        Assert.Contains("generated_combat.player_route_missing", result.Diagnostics);
        Assert.Null(result.Contract);
    }

    [Fact]
    public void Behavioral_basic_only_contract_id_is_deterministic()
    {
        var first = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.BasicOnly).Contract!;
        var second = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.BasicOnly).Contract!;

        Assert.Equal(first.ContractId, second.ContractId);
    }

    [Fact]
    public void Behavioral_ability_only_contract_id_is_deterministic()
    {
        var first = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.AbilityOnly).Contract!;
        var second = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.AbilityOnly).Contract!;

        Assert.Equal(first.ContractId, second.ContractId);
    }

    [Fact]
    public void Behavioral_basic_only_overlay_does_not_fabricate_player_abilities()
    {
        var prepared = Goal165RouteFixtures.Overlay(Goal165RouteFixtureKind.BasicOnly);

        Assert.True(prepared.Overlay.Passed, string.Join(Environment.NewLine, prepared.Overlay.Diagnostics));
        Assert.All(prepared.Overlay.CombatOverlayPackage.Game.Encounters
            .Where(encounter => prepared.Binding.Bindings.Any(binding => binding.PackageEncounterId == encounter.Id))
            .SelectMany(encounter => encounter.Participants.Where(participant => participant.Team == "player")),
            player => Assert.Empty(player.Abilities));
    }

    [Fact]
    public void Behavioral_ability_only_overlay_keeps_only_package_owned_abilities()
    {
        var prepared = Goal165RouteFixtures.Overlay(Goal165RouteFixtureKind.AbilityOnly);

        Assert.True(prepared.Overlay.Passed, string.Join(Environment.NewLine, prepared.Overlay.Diagnostics));
        Assert.All(prepared.Overlay.CombatOverlayPackage.Game.Encounters
            .Where(encounter => prepared.Binding.Bindings.Any(binding => binding.PackageEncounterId == encounter.Id))
            .SelectMany(encounter => encounter.Participants.Where(participant => participant.Team == "player")),
            player => Assert.All(player.Abilities, ability => Assert.Single(
                prepared.Overlay.CombatOverlayPackage.Game.Abilities.Where(item => item.Id == ability))));
    }

    [Fact]
    public void Behavioral_basic_only_qualification_is_campaign_current()
    {
        var summary = Goal165RouteFixtures.Qualify(Goal165RouteFixtureKind.BasicOnly);

        Assert.True(summary.Passed, string.Join(Environment.NewLine, summary.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", summary.Status);
        Assert.Equal(GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY, summary.RouteMode);
    }

    [Fact]
    public void Behavioral_ability_only_qualification_is_campaign_current()
    {
        var summary = Goal165RouteFixtures.Qualify(Goal165RouteFixtureKind.AbilityOnly);

        Assert.True(summary.Passed, string.Join(Environment.NewLine, summary.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", summary.Status);
        Assert.Equal(GeneratedEncounterCombatRouteMode.PACKAGE_ABILITY_ONLY, summary.RouteMode);
    }

    [Fact]
    public void Contract_optional_unavailable_route_is_vacuous_passed()
    {
        var summary = Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.BasicOnly).Contract!.QualificationSummary;

        Assert.True(summary.BasicAttackPassed);
        Assert.True(summary.PackageAbilityPassed);
        Assert.False(summary.PackageAbilityRequired);
    }

    [Fact]
    public void Contract_required_unavailable_route_blocks_qualification()
    {
        var summary = new GameProjectGeneratedEncounterCombatSummary
        {
            Present = true,
            Passed = true,
            Status = "CAMPAIGN_CURRENT",
            RouteMode = GeneratedEncounterCombatRouteMode.BASIC_ATTACK_ONLY,
            BasicAttackRequired = true,
            PackageAbilityRequired = false,
            BasicAttackPassed = false,
            PackageAbilityPassed = true
        };

        Assert.NotEqual(
            GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(summary),
            GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(summary with { BasicAttackPassed = true }));
    }

    [Fact]
    public void Behavioral_goal164_v4_both_route_history_remains_current()
    {
        var fixture = Goal164TestKit.AllSelectable;

        Assert.True(fixture.Build.Passed, string.Join(Environment.NewLine, fixture.Build.Diagnostics));
        Assert.Equal("CAMPAIGN_CURRENT", fixture.Snapshot.GeneratedWorld?.Status);
        Assert.Equal(GeneratedEncounterCombatRouteMode.BOTH, fixture.Build.GeneratedEncounterCombat?.RouteMode);
    }

    [Fact]
    public void Behavioral_new_basic_only_v4_summary_is_current()
    {
        var summary = Goal165RouteFixtures.Qualify(Goal165RouteFixtureKind.BasicOnly);

        Assert.Equal("CAMPAIGN_CURRENT", summary.Status);
        Assert.True(summary.BasicAttackRequired);
        Assert.False(summary.PackageAbilityRequired);
    }

    [Fact]
    public void Behavioral_new_ability_only_v4_summary_is_current()
    {
        var summary = Goal165RouteFixtures.Qualify(Goal165RouteFixtureKind.AbilityOnly);

        Assert.Equal("CAMPAIGN_CURRENT", summary.Status);
        Assert.False(summary.BasicAttackRequired);
        Assert.True(summary.PackageAbilityRequired);
    }

    [Fact]
    public void Contract_regeneration_seal_hash_covers_route_mode_and_required_flags()
    {
        var summary = Goal165RouteFixtures.Qualify(Goal165RouteFixtureKind.BasicOnly);
        var original = GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(summary);
        var modeTampered = GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(summary with
        {
            RouteMode = GeneratedEncounterCombatRouteMode.BOTH,
            PackageAbilityRequired = true
        });

        Assert.NotEqual(original, modeTampered);
    }

    [Fact]
    public void Contract_route_mode_tamper_is_not_semantically_equivalent()
    {
        var summary = Goal165RouteFixtures.Qualify(Goal165RouteFixtureKind.AbilityOnly);

        Assert.NotEqual(
            GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(summary),
            GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(summary with
            {
                BasicAttackRequired = true,
                RouteMode = GeneratedEncounterCombatRouteMode.BOTH
            }));
    }
}

internal enum Goal165RouteFixtureKind
{
    Both,
    BasicOnly,
    AbilityOnly,
    Neither
}

internal sealed record Goal165RouteOverlayFixture(
    Goal164BuildFixture Build,
    GeneratedEncounterCombatContract Contract,
    GeneratedEncounterCombatBindingResult Binding,
    GeneratedWorldEncounterCombatOverlayResult Overlay);

internal static class Goal165RouteFixtures
{
    public static GeneratedEncounterCombatContractResult Resolve(Goal165RouteFixtureKind kind)
    {
        var build = Goal164TestKit.AllSelectable;
        var package = Package(kind);
        return new GeneratedEncounterCombatContractService().Resolve(package, build.Source.Overlay!, build.Runtime);
    }

    public static Goal165RouteOverlayFixture Overlay(Goal165RouteFixtureKind kind)
    {
        var build = Goal164TestKit.AllSelectable;
        var contract = Assert.IsType<GeneratedEncounterCombatContract>(Resolve(kind).Contract);
        var binding = new GeneratedEncounterCombatBindingService().Bind(
            build.Source,
            Goal164TestKit.Clone(build.LaneAPackage),
            contract);
        var overlay = new GeneratedWorldEncounterCombatOverlayService().Build(
            Goal164TestKit.Clone(build.LaneAPackage), contract, binding);
        return new Goal165RouteOverlayFixture(build, contract, binding, overlay);
    }

    public static GameProjectGeneratedEncounterCombatSummary Qualify(Goal165RouteFixtureKind kind)
    {
        var prepared = Overlay(kind);
        return new GameProjectGeneratedEncounterCombatQualificationService().Qualify(
            prepared.Overlay.CombatOverlayPackage,
            prepared.Build.Source,
            prepared.Contract,
            prepared.Binding,
            prepared.Overlay.Document,
            prepared.Build.Runtime);
    }

    private static GamePackageDefinition Package(Goal165RouteFixtureKind kind)
    {
        var package = Goal164TestKit.Clone(Goal164TestKit.AllSelectable.LaneAPackage);
        const string nonDamagingAbilityId = "fixture/goal165/non_damage";
        const string nonDamagingStatusId = "fixture/goal165/non_damage_status";
        if (kind is Goal165RouteFixtureKind.AbilityOnly or Goal165RouteFixtureKind.Neither)
        {
            package.Game.Statuses.Add(new StatusDefinition
            {
                Id = nonDamagingStatusId,
                Name = "Fixture non-damaging status"
            });
            package.Game.Abilities.Add(new AbilityDefinition
            {
                Id = nonDamagingAbilityId,
                Name = "Fixture non-damaging attack",
                Kind = "utility",
                Effects = [new LLMGameCreator.Domain.Definitions.EffectDefinition
                {
                    Type = "add_status",
                    Args = new Dictionary<string, string>
                    {
                        ["id"] = nonDamagingStatusId,
                        ["amount"] = "1"
                    }
                }]
            });
        }

        foreach (var encounter in package.Game.Encounters)
        {
            var players = encounter.Participants.Where(item => item.Team == "player").ToList();
            if (players.Count == 0) continue;
            switch (kind)
            {
                case Goal165RouteFixtureKind.BasicOnly:
                    encounter.Metadata.Remove("default_attack_ability_id");
                    foreach (var player in players) player.Abilities.Clear();
                    foreach (var player in players)
                    foreach (var resource in player.Resources) resource.Amount = 99;
                    foreach (var opponent in encounter.Participants.Where(item => item.Team != "player"))
                    foreach (var resource in opponent.Resources) resource.Amount = 2;
                    break;
                case Goal165RouteFixtureKind.AbilityOnly:
                    encounter.Metadata["default_attack_ability_id"] = nonDamagingAbilityId;
                    break;
                case Goal165RouteFixtureKind.Neither:
                    encounter.Metadata["default_attack_ability_id"] = nonDamagingAbilityId;
                    foreach (var player in players) player.Abilities.Clear();
                    break;
            }
        }
        return package;
    }
}
