using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal168;
using Xunit;
using PackageEffectDefinition =
    LLMGameCreator.Domain.Definitions.EffectDefinition;

namespace LLMGameCreator.Tests.Application.Goal169;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal169ExactEffectTests
{
    [Fact]
    public void Behavioral_health_descriptor_is_accepted()
    {
        var route = Goal168TestKit.RealRoute();
        Assert.True(route.Passed,
            string.Join(Environment.NewLine, route.Diagnostics));
        Assert.True(route.EncounterProgressObserved);
    }

    [Fact]
    public void Behavioral_stat_descriptor_is_accepted()
    {
        var route = Goal169EffectFixture.Stat.Value;
        Assert.True(route.Result.Passed,
            Goal164TestKit.Canonical(route.Result));
        Assert.Contains(route.DescriptorFingerprint,
            route.Result.UsedQualifiedActionFingerprints);
    }

    [Fact]
    public void Behavioral_status_descriptor_is_accepted()
    {
        var route = Goal169EffectFixture.Status.Value;
        Assert.True(route.Result.Passed,
            string.Join(Environment.NewLine, route.Result.Diagnostics));
        Assert.Contains(route.DescriptorFingerprint,
            route.Result.UsedQualifiedActionFingerprints);
    }

    [Fact]
    public void Behavioral_delayed_status_damage_reaches_victory()
    {
        var route = Goal169EffectFixture.Status.Value.Result;
        Assert.True(route.Passed);
        Assert.True(route.EncounterProgressObserved);
        Assert.Contains(GameRuntimeCommandType.RunCurrentTurnAi,
            route.Commands);
    }

    [Fact]
    public void Behavioral_successful_no_effect_utility_is_rejected()
    {
        var route = Goal169EffectFixture.NoOp.Value.Result;
        Assert.False(route.Passed);
        Assert.False(route.EncounterProgressObserved);
        Assert.Contains(
            "generated_relationship.qualified_action_no_progress",
            route.Diagnostics);
    }

    [Fact]
    public void Behavioral_mixed_utility_first_selects_progressing_descriptor()
    {
        var route = Goal169EffectFixture.NoOpThenDamage.Value;
        Assert.True(route.Result.Passed,
            Goal164TestKit.Canonical(route.Result));
        Assert.DoesNotContain(route.DescriptorFingerprint,
            route.Result.UsedQualifiedActionFingerprints);
    }

    [Fact]
    public void Behavioral_ability_only_route_remains_effect_neutral()
    {
        var route = Goal168TestKit.AbilityOnlyRoute();
        Assert.True(route.Passed,
            string.Join(Environment.NewLine, route.Diagnostics));
        Assert.DoesNotContain(GameRuntimeCommandType.BasicAttack,
            route.Commands);
    }

    [Fact]
    public void Behavioral_descriptor_effect_fingerprint_is_exact()
    {
        var fixture = Goal169EffectFixture.Stat.Value;
        Assert.Equal(fixture.Descriptor.ObservedEffect.Fingerprint,
            Hash(new
            {
                effectClass = "TARGET_STAT_CHANGED",
                damagedResources = Array.Empty<string>(),
                changedStats = fixture.Descriptor.TargetStatIds,
                changedStatuses = Array.Empty<string>()
            }));
    }

    [Fact]
    public void Behavioral_ability_definition_sha_is_exact()
    {
        var fixture = Goal169EffectFixture.Stat.Value;
        var definition = fixture.Package.Game.Abilities.Single(item =>
            item.Id == fixture.Descriptor.AbilityId);
        Assert.Equal(Hash(definition),
            fixture.Descriptor.AbilityDefinitionSha256);
    }

    [Fact]
    public void Behavioral_exact_effect_package_reference_is_unchanged()
    {
        Assert.True(Goal169EffectFixture.Stat.Value.Result
            .PackageReferenceUnchanged);
        Assert.Equal(Goal169EffectFixture.Stat.Value.Result
                .PackageSha256Before,
            Goal169EffectFixture.Stat.Value.Result.PackageSha256After);
    }

    [Fact]
    public void Behavioral_repeated_encounter_state_is_rejected()
    {
        var route = Goal169EffectFixture.NoOp.Value.Result;
        Assert.False(route.Passed);
        Assert.Contains(
            "generated_relationship.arc_combat_failed",
            route.Diagnostics);
    }

    [Fact]
    public void Behavioral_no_op_only_route_fails_causally()
    {
        var route = Goal169EffectFixture.NoOp.Value.Result;
        Assert.False(route.Passed);
        Assert.True(route.CommandBound > 0);
        Assert.Contains(
            "generated_relationship.qualified_action_no_progress",
            route.Diagnostics);
    }

    internal static string Hash<T>(T value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                Goal164TestKit.Canonical(value))))
            .ToLowerInvariant();
}

internal sealed record Goal169EffectRouteFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition Package,
    GeneratedEncounterCombatQualifiedAction Descriptor,
    string DescriptorFingerprint,
    GeneratedCampaignExactCombatRouteResult Result);

internal static class Goal169EffectFixture
{
    internal static readonly Lazy<Goal169EffectRouteFixture> Stat =
        new(() => Build(EffectKind.Stat));
    internal static readonly Lazy<Goal169EffectRouteFixture> Status =
        new(() => Build(EffectKind.Status));
    internal static readonly Lazy<Goal169EffectRouteFixture> NoOp =
        new(() => Build(EffectKind.NoOp, includeDamageFallback: false));
    internal static readonly Lazy<Goal169EffectRouteFixture>
        NoOpThenDamage =
        new(() => Build(EffectKind.NoOp, includeDamageFallback: true));

    private static Goal169EffectRouteFixture Build(
        EffectKind kind,
        bool includeDamageFallback = true)
    {
        var package = Goal164TestKit.Clone(Goal168TestKit.Package);
        var encounter = package.Game.Encounters.Single(item =>
            item.Id == Goal168TestKit.EncounterId);
        var player = encounter.Participants.First(item =>
            string.Equals(item.Team, "player",
                StringComparison.OrdinalIgnoreCase));
        var opponent = encounter.Participants.First(item =>
            !string.Equals(item.Team, "player",
                StringComparison.OrdinalIgnoreCase));
        const string chargeId = "resource/goal169-exact-charge";
        package.Game.Resources.Add(new ResourceDefinition
        {
            Id = chargeId,
            Name = "Exact route charge",
            Kind = "action",
            MinValue = 0,
            MaxValue = 1
        });
        player.Resources.Add(new OutputDefinition
        {
            Kind = "resource",
            Id = chargeId,
            Amount = 1
        });
        const string statId = "stat/goal169-exact";
        const string statusId = "status/goal169-delayed";
        var abilityId = kind switch
        {
            EffectKind.Stat => "ability/000-goal169-stat",
            EffectKind.Status => "ability/000-goal169-status",
            _ => "ability/000-goal169-no-op"
        };
        var targetIds = new List<string>();
        var effectClass = string.Empty;
        var effects = new List<PackageEffectDefinition>();
        if (kind == EffectKind.Stat)
        {
            package.Game.Stats.Add(new StatDefinition
            {
                Id = statId,
                Name = "Exact target stat",
                MinValue = -100,
                MaxValue = 100
            });
            opponent.Stats.Add(new OutputDefinition
            {
                Kind = "stat",
                Id = statId,
                Amount = 10
            });
            effects.Add(new PackageEffectDefinition
            {
                Type = "change_stat",
                Args = new Dictionary<string, string>
                {
                    ["id"] = statId,
                    ["amount"] = "-1"
                }
            });
            effectClass = "TARGET_STAT_CHANGED";
            targetIds.Add(statId);
        }
        else if (kind == EffectKind.Status)
        {
            package.Game.Statuses.Add(new StatusDefinition
            {
                Id = statusId,
                Name = "Delayed exact damage",
                Kind = "debuff",
                DurationMode = "turns",
                Effects =
                [
                    new PackageEffectDefinition
                    {
                        Type = "damage_resource",
                        Args = new Dictionary<string, string>
                        {
                            ["id"] = HealthId(package, opponent),
                            ["amount"] = "2"
                        }
                    }
                ]
            });
            effects.Add(new PackageEffectDefinition
            {
                Type = "add_status",
                Args = new Dictionary<string, string>
                {
                    ["id"] = statusId,
                    ["amount"] = "3"
                }
            });
            effectClass = "TARGET_STATUS_CHANGED";
            targetIds.Add(statusId);
        }

        foreach (var hostile in encounter.Participants.Where(item =>
                     !string.Equals(item.Team, "player",
                         StringComparison.OrdinalIgnoreCase)))
        foreach (var resource in hostile.Resources.Where(item =>
                     item.Id == HealthId(package, hostile)))
            resource.Amount = kind == EffectKind.Status ? 2 : 1;

        var ability = new AbilityDefinition
        {
            Id = abilityId,
            Name = "Goal169 exact effect",
            Kind = kind == EffectKind.NoOp ? "utility" : "attack",
            Targeting = "hostile_participant",
            Costs =
            [
                new CostDefinition
                {
                    Kind = "resource",
                    Id = chargeId,
                    Amount = 1
                }
            ],
            Effects = effects
        };
        package.Game.Abilities.Add(ability);
        player.Abilities.Insert(0, abilityId);
        var observed = new GeneratedEncounterCombatObservedEffect
        {
            EffectClass = kind == EffectKind.NoOp
                ? "TARGET_STATUS_CHANGED"
                : effectClass,
            TargetStatIds = kind == EffectKind.Stat ? targetIds : [],
            TargetStatusIds =
                kind is EffectKind.Status or EffectKind.NoOp
                    ? kind == EffectKind.NoOp
                        ? ["status/goal169-never-created"]
                        : targetIds
                    : []
        };
        observed = observed with
        {
            Fingerprint = Goal169ExactEffectTests.Hash(new
            {
                effectClass = observed.EffectClass,
                damagedResources = Array.Empty<string>(),
                changedStats = observed.TargetStatIds,
                changedStatuses = observed.TargetStatusIds
            })
        };
        var descriptor = new GeneratedEncounterCombatQualifiedAction
        {
            ActionKind =
                GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY,
            AbilityId = abilityId,
            AbilityDefinitionSha256 =
                Goal169ExactEffectTests.Hash(ability),
            SourceParticipantRoleFingerprint =
                Goal168TestKit.Combat.QualifiedActions[0]
                    .SourceParticipantRoleFingerprint,
            ObservedEffect = observed,
            TargetStatIds = observed.TargetStatIds,
            TargetStatusIds = observed.TargetStatusIds,
            RuntimeCommandType = GameRuntimeCommandType.UseAbility,
            RuntimeQualificationPassed = true
        };
        var actions = new List<GeneratedEncounterCombatQualifiedAction>
        {
            descriptor
        };
        if (includeDamageFallback)
            actions.AddRange(Goal168TestKit.Combat.QualifiedActions
                .Where(item => item.ActionKind ==
                    GeneratedEncounterCombatQualifiedActionKind
                        .PACKAGE_ABILITY));
        var summary = Goal168TestKit.SummaryFor(package, actions);
        var result = Goal168TestKit.Route(package, summary);
        return new Goal169EffectRouteFixture(package, descriptor,
            Goal169ExactEffectTests.Hash(descriptor), result);
    }

    private static string HealthId(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        EncounterParticipantDefinition participant) =>
        participant.Resources.Select(item => item.Id).First(id =>
            package.Game.Resources.Any(definition =>
                definition.Id == id
                && (definition.Id == "resource/health"
                    || definition.Kind == "health"
                    || definition.Tags.Contains("health"))));

    private enum EffectKind
    {
        Stat,
        Status,
        NoOp
    }
}
