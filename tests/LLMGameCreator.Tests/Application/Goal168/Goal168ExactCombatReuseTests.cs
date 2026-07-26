using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168ExactCombatReuseTests
{
    [Fact]
    public void Behavioral_choice_and_relationships_reuse_exact_catalog()
    {
        Assert.Equal(Goal168TestKit.Combat.QualifiedActionsSha256,
            Goal168TestKit.Choices.TechnicalDetails["qualifiedActionsSha256"]);
        Assert.Equal(Goal168TestKit.Combat.QualifiedActionsSha256,
            Goal168TestKit.Relationships.QualifiedActionsSha256);
    }

    [Fact]
    public void Contract_old_raw_combat_helpers_are_removed()
    {
        var methods = typeof(GameProjectGeneratedCampaignChoiceQualificationService)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Select(item => item.Name).ToList();
        Assert.DoesNotContain("WinEncounter", methods);
        Assert.DoesNotContain("TryAbilities", methods);
    }

    [Fact]
    public void Behavioral_utility_success_is_excluded_before_damage_progress()
    {
        var fixture = Goal168TestKit.UtilityFirst();
        Assert.True(fixture.Result.Passed,
            string.Join(Environment.NewLine, fixture.Result.Diagnostics)
            + Environment.NewLine + string.Join(",", fixture.Result.Commands)
            + Environment.NewLine + string.Join(",",
                fixture.Result.UsedQualifiedActionFingerprints));
        Assert.DoesNotContain(fixture.UtilityFingerprint,
            fixture.Result.UsedQualifiedActionFingerprints);
        Assert.Contains(GameRuntimeCommandType.UseAbility, fixture.Result.Commands);
    }

    [Fact]
    public void Behavioral_ability_only_support_combat_succeeds()
    {
        var result = Goal168TestKit.AbilityOnlyRoute();
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics)
                                   + Environment.NewLine
                                   + string.Join(",", result.Commands)
                                   + Environment.NewLine
                                   + string.Join(",",
                                       result.UsedQualifiedActionFingerprints));
        Assert.DoesNotContain(GameRuntimeCommandType.BasicAttack, result.Commands);
        Assert.Contains(GameRuntimeCommandType.UseAbility, result.Commands);
    }

    [Fact]
    public void Behavioral_ability_only_challenge_combat_succeeds() =>
        Assert.True(Goal168TestKit.AbilityOnlyRoute().EncounterProgressObserved);

    [Fact]
    public void Behavioral_basic_descriptor_observed_effect_is_matched()
    {
        var result = Goal168TestKit.RealRoute();
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains(GameRuntimeCommandType.BasicAttack, result.Commands);
        Assert.True(result.EncounterProgressObserved);
    }

    [Fact]
    public void Behavioral_ability_definition_sha_change_is_rejected()
    {
        var package = Goal164TestKit.Clone(Goal168TestKit.Package);
        var ability = Goal168TestKit.Combat.QualifiedActions.First(item =>
            item.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY);
        package.Game.Abilities.Single(item => item.Id == ability.AbilityId).Name += " changed";
        var result = Goal168TestKit.Route(package, Goal168TestKit.Combat);
        Assert.False(result.Passed);
        Assert.Contains("generated_relationship.qualified_action_definition_changed",
            result.Diagnostics);
    }

    [Fact]
    public void Behavioral_no_progress_only_catalog_is_rejected()
    {
        var fixture = Goal168TestKit.UtilityOnly();
        Assert.False(fixture.Result.Passed);
        Assert.Contains("generated_relationship.qualified_action_no_progress",
            fixture.Result.Diagnostics);
    }

    [Fact]
    public void Behavioral_exact_route_keeps_package_sha_and_reference()
    {
        var result = Goal168TestKit.RealRoute();
        Assert.True(result.PackageReferenceUnchanged);
        Assert.Equal(result.PackageSha256Before, result.PackageSha256After);
        Assert.Equal(Goal168TestKit.Build.PackageSha256, result.PackageSha256After);
    }

    [Fact]
    public void Behavioral_bounded_route_failure_is_causal()
    {
        var fixture = Goal168TestKit.UtilityOnly();
        Assert.True(fixture.Result.CommandBound > 0);
        Assert.Contains("generated_relationship.arc_combat_failed",
            fixture.Result.Diagnostics);
        Assert.False(fixture.Result.EncounterProgressObserved);
    }
}

internal static class Goal168TestKit
{
    internal static Goal164BuildFixture Real => Goal164TestKit.AllSelectable;
    internal static GameProjectBuildResult Build => Real.Build;
    internal static GamePackageDefinition Package => Real.Package;
    internal static GameProjectGeneratedEncounterCombatSummary Combat =>
        Assert.IsType<GameProjectGeneratedEncounterCombatSummary>(
            Build.GeneratedEncounterCombat);
    internal static GameProjectGeneratedCampaignChoiceSummary Choices =>
        Assert.IsType<GameProjectGeneratedCampaignChoiceSummary>(
            Build.GeneratedCampaignChoices);
    internal static GameProjectGeneratedCampaignRelationshipSummary Relationships =>
        Assert.IsType<GameProjectGeneratedCampaignRelationshipSummary>(
            Build.GeneratedCampaignRelationships);
    internal static GeneratedCampaignRelationshipOverlayDocument RelationshipOverlay =>
        Assert.IsType<GeneratedCampaignRelationshipOverlayDocument>(
            Relationships.Overlay);

    internal static string EncounterId => RelationshipOverlay.Bindings
        .SelectMany(item => item.QuestArc)
        .Select(item => item.TargetEncounterId)
        .First(item => !string.IsNullOrWhiteSpace(item));

    internal static GeneratedCampaignExactCombatRouteResult RealRoute() =>
        Route(Package, Combat);

    internal static GeneratedCampaignExactCombatRouteResult AbilityOnlyRoute()
    {
        var package = AbilityOnlyPackage();
        var actions = Combat.QualifiedActions.Where(item =>
                item.ActionKind ==
                GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY)
            .ToList();
        Assert.NotEmpty(actions);
        return Route(package, SummaryFor(package, actions));
    }

    internal static UtilityFixture UtilityFirst() =>
        UtilityFixtureFor(includeDamage: true);

    internal static UtilityFixture UtilityOnly() =>
        UtilityFixtureFor(includeDamage: false);

    internal static GeneratedCampaignExactCombatRouteResult Route(
        GamePackageDefinition package,
        GameProjectGeneratedEncounterCombatSummary summary)
    {
        var started = Real.Runtime.Start(package);
        Assert.True(started.Success);
        return new GeneratedCampaignExactCombatRouteService().Execute(
            new GeneratedCampaignExactCombatRouteRequest
            {
                FinalPackage = package,
                EncounterId = EncounterId,
                CombatSummary = summary,
                Runtime = Real.Runtime,
                InitialSession = started.Session,
                Goal = GeneratedCampaignExactCombatRouteGoal.VICTORY
            });
    }

    internal static string PackageSha(GamePackageDefinition package) =>
        HashText(Serialize(package) + Environment.NewLine);

    internal static GameProjectGeneratedEncounterCombatSummary SummaryFor(
        GamePackageDefinition package,
        IReadOnlyList<GeneratedEncounterCombatQualifiedAction> actions)
    {
        var ordered = actions.OrderBy(item =>
                item.ActionKind ==
                GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK ? 0 : 1)
            .ThenBy(item => item.AbilityId, StringComparer.Ordinal)
            .ThenBy(item => item.AbilityDefinitionSha256, StringComparer.Ordinal)
            .ThenBy(item => item.ObservedEffect.Fingerprint, StringComparer.Ordinal)
            .ToList();
        return Combat with
        {
            ExactPackageSha256 = PackageSha(package),
            QualifiedActions = ordered,
            QualifiedActionCount = ordered.Count,
            QualifiedBasicAttackCount = ordered.Count(item =>
                item.ActionKind ==
                GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK),
            QualifiedPackageAbilityCount = ordered.Count(item =>
                item.ActionKind ==
                GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY),
            QualifiedActionsSha256 = Hash(ordered)
        };
    }

    private static UtilityFixture UtilityFixtureFor(bool includeDamage)
    {
        var package = AbilityOnlyPackage();
        const string utilityId = "ability/000-goal168-utility";
        const string statusId = "status/goal168-utility";
        package.Game.Statuses.Add(new StatusDefinition
        {
            Id = statusId,
            Name = "Utility marker"
        });
        var utility = new AbilityDefinition
        {
            Id = utilityId,
            Name = "Utility",
            Kind = "utility",
            Effects =
            [
                new LLMGameCreator.Domain.Definitions.EffectDefinition
                {
                    Type = "add_status",
                    Args = new Dictionary<string, string>
                    {
                        ["id"] = statusId,
                        ["amount"] = "1"
                    }
                }
            ]
        };
        package.Game.Abilities.Add(utility);
        foreach (var participant in package.Game.Encounters
                     .SelectMany(item => item.Participants)
                     .Where(item => string.Equals(item.Team, "player",
                         StringComparison.OrdinalIgnoreCase)))
            participant.Abilities.Insert(0, utilityId);
        var observed = new GeneratedEncounterCombatObservedEffect
        {
            EffectClass = "TARGET_STATUS_CHANGED",
            TargetStatusIds = ["status/goal168-utility"]
        };
        observed = observed with
        {
            Fingerprint = Hash(new
            {
                effectClass = observed.EffectClass,
                damagedResources = Array.Empty<string>(),
                changedStats = Array.Empty<string>(),
                changedStatuses = observed.TargetStatusIds
            })
        };
        var utilityDescriptor = new GeneratedEncounterCombatQualifiedAction
        {
            ActionKind =
                GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY,
            AbilityId = utilityId,
            AbilityDefinitionSha256 =
                Hash(utility),
            SourceParticipantRoleFingerprint =
                Combat.QualifiedActions[0].SourceParticipantRoleFingerprint,
            ObservedEffect = observed,
            TargetStatusIds = observed.TargetStatusIds,
            RuntimeCommandType = GameRuntimeCommandType.UseAbility,
            RuntimeQualificationPassed = true
        };
        var damage = Combat.QualifiedActions.Where(item =>
                item.ActionKind ==
                GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY)
            .ToList();
        var actions = includeDamage
            ? new[] { utilityDescriptor }.Concat(damage).ToList()
            : [utilityDescriptor];
        var summary = SummaryFor(package, actions);
        var result = Route(package, summary);
        return new UtilityFixture(result,
            Hash(utilityDescriptor));
    }

    private static GamePackageDefinition AbilityOnlyPackage()
    {
        var package = Goal164TestKit.Clone(Package);
        foreach (var encounter in package.Game.Encounters)
        {
            foreach (var participant in encounter.Participants)
            foreach (var resource in participant.Resources)
                resource.Amount = string.Equals(participant.Team, "player",
                    StringComparison.OrdinalIgnoreCase) ? 99 : 2;
        }
        return package;
    }

    private static readonly JsonSerializerOptions CanonicalOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, CanonicalOptions);

    private static string Hash<T>(T value) => HashText(Serialize(value));

    private static string HashText(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

internal sealed record UtilityFixture(
    GeneratedCampaignExactCombatRouteResult Result,
    string UtilityFingerprint);
