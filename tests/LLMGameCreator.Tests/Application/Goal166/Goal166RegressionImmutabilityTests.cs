using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal165;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166RegressionImmutabilityTests
{
    [Fact] public void Behavioral_goal165_both_route_still_resolves() => Assert.True(Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Both).Passed);
    [Fact] public void Behavioral_goal165_basic_route_still_resolves() => Assert.True(Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.BasicOnly).Passed);
    [Fact] public void Behavioral_goal165_ability_route_still_resolves() => Assert.True(Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.AbilityOnly).Passed);
    [Fact] public void Behavioral_goal165_neither_route_is_not_synthetic() => Assert.False(Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Neither).Passed);
    [Fact] public void Behavioral_catalog_hash_is_stable_for_same_contract() => Assert.Equal(Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Both).Contract!.QualifiedActionsSha256, Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Both).Contract!.QualifiedActionsSha256);
}
