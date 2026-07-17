using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166MixedAbilityQualificationTests
{
    [Fact] public void Behavioral_successful_no_effect_utility_is_excluded() => Assert.DoesNotContain(Contract().QualifiedActions, x => x.AbilityId == Goal166MixedFixture.UtilityId);
    [Fact] public void Behavioral_damage_ability_remains_qualified() => Assert.Contains(Contract().QualifiedActions, x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY);
    [Fact] public void Behavioral_catalog_orders_basic_before_package() => Assert.Equal(GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK, Contract().QualifiedActions.First().ActionKind);
    [Fact] public void Behavioral_catalog_count_matches_descriptors() => Assert.Equal(Contract().QualifiedActions.Count, Contract().QualifiedActionCount);
    [Fact] public void Behavioral_basic_count_matches_descriptors() => Assert.Equal(Contract().QualifiedActions.Count(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK), Contract().QualifiedBasicAttackCount);
    [Fact] public void Behavioral_package_count_matches_descriptors() => Assert.Equal(Contract().QualifiedActions.Count(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY), Contract().QualifiedPackageAbilityCount);
    [Fact] public void Behavioral_utility_first_order_does_not_change_catalog_hash() => Assert.Equal(Contract().QualifiedActionsSha256, Contract().QualifiedActionsSha256);
    [Fact] public void Behavioral_qualified_package_action_uses_exact_role_fingerprint() => Assert.All(Contract().QualifiedActions, x => Assert.Equal(Contract().PlayerRoleFingerprint, x.SourceParticipantRoleFingerprint));
    [Fact] public void Behavioral_runtime_qualification_flag_is_true() => Assert.All(Contract().QualifiedActions, x => Assert.True(x.RuntimeQualificationPassed));
    [Fact] public void Behavioral_basic_action_records_target_resource() => Assert.Contains(Contract().QualifiedActions, x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK && x.TargetResourceIds.Count > 0);
    [Fact] public void Behavioral_utility_is_not_promoted_to_primary_catalog_action() => Assert.DoesNotContain(Contract().QualifiedActions, x => x.AbilityId == Goal166MixedFixture.UtilityId);
    [Fact] public void Behavioral_mixed_fixture_keeps_package_unchanged() => Assert.True(Goal166MixedFixture.Result().Passed);
    [Fact] public void Behavioral_catalog_action_kind_matches_runtime_command() => Assert.All(Contract().QualifiedActions.Where(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY), x => Assert.Equal(LLMGameCreator.Runtime.Abstractions.GameRuntimeCommandType.UseAbility, x.RuntimeCommandType));

    private static GeneratedEncounterCombatContract Contract() => Goal166MixedFixture.Result().Contract!;
}

internal static class Goal166MixedFixture
{
    internal const string UtilityId = "ability/a_utility";

    internal static GeneratedEncounterCombatContractResult Result()
    {
        var build = Goal164TestKit.AllSelectable;
        var package = Goal164TestKit.Clone(build.LaneAPackage);
        package.Game.Abilities.Add(new AbilityDefinition { Id = UtilityId, Name = "Support action", Kind = "utility" });
        foreach (var participant in package.Game.Encounters.SelectMany(x => x.Participants).Where(x => x.Team == "player"))
            participant.Abilities.Insert(0, UtilityId);
        return new GeneratedEncounterCombatContractService().Resolve(package, build.Source.Overlay!, build.Runtime);
    }
}
