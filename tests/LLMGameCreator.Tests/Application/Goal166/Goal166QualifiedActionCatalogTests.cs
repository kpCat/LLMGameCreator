using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal165;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166QualifiedActionCatalogTests
{
    [Fact] public void Behavioral_basic_descriptor_is_recorded() => Assert.Contains(Both().QualifiedActions, x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK);
    [Fact] public void Behavioral_package_descriptor_is_recorded() => Assert.Contains(Both().QualifiedActions, x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY);
    [Fact] public void Behavioral_package_descriptor_has_exact_ability_id() => Assert.All(Both().QualifiedActions.Where(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY), x => Assert.False(string.IsNullOrWhiteSpace(x.AbilityId)));
    [Fact] public void Behavioral_package_descriptor_has_canonical_fingerprint() => Assert.All(Both().QualifiedActions.Where(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY), x => Assert.Matches("^[a-f0-9]{64}$", x.AbilityDefinitionSha256));
    [Fact] public void Behavioral_basic_descriptor_has_no_ability_id() => Assert.All(Both().QualifiedActions.Where(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK), x => Assert.True(string.IsNullOrEmpty(x.AbilityId)));
    [Fact] public void Behavioral_descriptor_records_runtime_command_type() => Assert.All(Both().QualifiedActions, x => Assert.NotEqual(default, x.RuntimeCommandType));
    [Fact] public void Behavioral_descriptor_records_supported_observed_effect() => Assert.All(Both().QualifiedActions, x => Assert.False(string.IsNullOrWhiteSpace(x.ObservedEffect.EffectClass)));
    [Fact] public void Behavioral_catalog_is_deterministic() => Assert.Equal(Both().QualifiedActionsSha256, Both().QualifiedActionsSha256);
    [Fact] public void Behavioral_contract_id_changes_when_catalog_changes() { var contract = Both(); Assert.NotEqual(contract.ContractId, GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(contract with { QualifiedActions = [] })); }
    [Fact] public void Behavioral_basic_only_catalog_has_only_basic() => Assert.All(BasicOnly().QualifiedActions, x => Assert.Equal(GeneratedEncounterCombatQualifiedActionKind.BASIC_ATTACK, x.ActionKind));
    [Fact] public void Behavioral_ability_only_catalog_has_only_package_actions() => Assert.All(AbilityOnly().QualifiedActions, x => Assert.Equal(GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY, x.ActionKind));
    [Fact] public void Behavioral_both_catalog_derives_both_route() => Assert.Equal(GeneratedEncounterCombatRouteMode.BOTH, Both().RouteMode);
    [Fact] public void Behavioral_neither_route_is_rejected() => Assert.False(Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Neither).Passed);
    [Fact] public void Behavioral_catalog_hash_detects_ability_tamper() { var action = Both().QualifiedActions.First(x => x.ActionKind == GeneratedEncounterCombatQualifiedActionKind.PACKAGE_ABILITY); Assert.NotEqual(Both().QualifiedActionsSha256, GameProjectSeedRegenerationCandidateSealService.CanonicalSha256(Both().QualifiedActions.Select(x => x == action ? x with { AbilityId = "ability/tampered" } : x).ToList())); }

    private static GeneratedEncounterCombatContract Both() => Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Both).Contract!;
    private static GeneratedEncounterCombatContract BasicOnly() => Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.BasicOnly).Contract!;
    private static GeneratedEncounterCombatContract AbilityOnly() => Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.AbilityOnly).Contract!;
}
