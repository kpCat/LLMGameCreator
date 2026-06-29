using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using Xunit;

namespace LLMGameCreator.Tests.Application.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureResolverTests
{
    [Fact]
    public void SameInputResolvesDeterministically()
    {
        var resolver = new DynamicSemanticFeatureResolver();
        var scenario = DynamicSemanticFeatureCatalog.FrontierScenario();

        var first = resolver.ResolveScenario(scenario);
        var second = resolver.ResolveScenario(scenario);

        Assert.Equal(first.StableSummary, second.StableSummary);
        Assert.Equal(
            first.TargetStates.Select(item => item.StableSummary),
            second.TargetStates.Select(item => item.StableSummary));
        Assert.DoesNotContain(first.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public void OptionalOrInapplicableMoodAndFactionAbsenceCanBeValid()
    {
        var scenario = new DynamicSemanticScenario
        {
            ScenarioId = "quiet_npc",
            ProfileId = "frontier_survival",
            Seed = 1,
            Targets =
            [
                new DynamicSemanticTargetNode { TargetId = "world/frontier", TargetScope = "world", Tags = ["frontier"], FamilyIds = ["frontier"] },
                new DynamicSemanticTargetNode { TargetId = "npc/quiet", TargetScope = "npc", ParentTargetIds = ["world/frontier"], Tags = [], FamilyIds = [] }
            ],
            Assignments =
            [
                new DynamicSemanticFeatureAssignment
                {
                    TargetId = "world/frontier",
                    TargetScope = "world",
                    FeatureId = "world.theme",
                    Value = DynamicSemanticFeatureCatalog.Enum("frontier"),
                    SourceLayer = "world",
                    SourceId = "world/frontier"
                }
            ],
            InfluenceRules = DynamicSemanticFeatureCatalog.BuildDefaultInfluenceRules(),
            ResolveTargetIds = ["npc/quiet"]
        };

        var state = new DynamicSemanticFeatureResolver().ResolveScenario(scenario);
        var npc = Assert.Single(state.TargetStates);

        Assert.DoesNotContain(state.Diagnostics, item => item.Severity == "error");
        Assert.Contains(npc.Traces, item => item.FeatureId == "npc.faction_relation" && item.TraceKind == "inapplicable_optional");
        Assert.Contains(npc.Traces, item => item.FeatureId == "npc.mood" && item.TraceKind is "absent_optional" or "inapplicable_optional");
    }

    [Fact]
    public void InheritanceOrderOverridesAndInfluenceTracesAreRecorded()
    {
        var scenario = DynamicSemanticFeatureCatalog.FrontierScenario();
        var state = new DynamicSemanticFeatureResolver().ResolveScenario(scenario);
        var npc = Assert.Single(state.TargetStates, item => item.TargetId == "npc/trail_medic");

        Assert.Contains(npc.Features, item => item.FeatureId == "world.theme" && item.Inherited);
        Assert.Contains(npc.Features, item => item.FeatureId == "npc.hunger" && item.Value?.NumberValue == 8);
        Assert.Contains(npc.Features, item => item.FeatureId == "npc.mood" && item.Value?.EnumValue == "hungry" && item.ResolutionSource == "influence");
        Assert.Contains(npc.InfluenceEffects, item => item.RuleId == "rule/frontier_hunger_mood" && item.EffectKind == "set_feature");
    }

    [Fact]
    public void FourScenariosProduceMeaningfullyDifferentOutputsAndMetamoduleUsesSpeciesArchetypeFamilies()
    {
        var resolver = new DynamicSemanticFeatureResolver();
        var states = DynamicSemanticFeatureCatalog.BuildDefaultScenarios()
            .Select(scenario => resolver.ResolveScenario(scenario))
            .ToList();

        Assert.Equal(4, states.Select(item => item.StableSummary).Distinct(StringComparer.Ordinal).Count());
        Assert.All(states, state => Assert.DoesNotContain(state.Diagnostics, item => item.Severity == "error"));

        var metamodule = Assert.Single(states, item => item.ScenarioId == "metamodule_kingdoms");
        var species = Assert.Single(metamodule.TargetStates, item => item.TargetId == "species/metamodule_bearer");
        var archetype = Assert.Single(metamodule.TargetStates, item => item.TargetId == "archetype/module_scout");

        Assert.Contains(species.Features, item => item.FeatureId == "species.module_capacity" && item.Value?.NumberValue == 5);
        Assert.Contains(archetype.Features, item => item.FeatureId == "archetype.forbidden_affinity" && item.Value?.EnumValue == "void");
        Assert.Contains(species.AuthoringSuggestions, item => item.Contains("archetype.forbidden_affinity", StringComparison.Ordinal));
    }
}
