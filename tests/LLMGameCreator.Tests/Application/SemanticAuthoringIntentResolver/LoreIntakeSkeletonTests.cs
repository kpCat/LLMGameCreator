using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticAuthoringIntentResolver;

public sealed class LoreIntakeSkeletonTests
{
    [Fact]
    public void MetamoduleSkeletonHasHighComplexitySlotsAndQuarantinedCandidates()
    {
        var skeleton = SemanticAuthoringIntentCatalog.BuildMetamoduleKingdomsLoreSkeleton();

        Assert.Equal("metamodule_kingdoms", skeleton.ScenarioId);
        Assert.InRange(skeleton.KingdomSlots.Count, 6, 7);
        Assert.True(skeleton.SpeciesArchetypeSlots.Count >= 100);
        Assert.True(skeleton.EvidenceSummary.LlmCandidatesQuarantined);
        Assert.All(skeleton.LlmCandidateSlots, slot =>
        {
            Assert.Equal("llm_candidate", slot.Provenance);
            Assert.Equal("review_required", slot.ReviewStatus);
        });

        foreach (var family in new[] { "module_carriers", "mana_resonance", "forbidden_affinities", "kingdom_pressure", "faction_relation", "dialogue_intent", "quest_motive", "event_intent", "economy_pressure", "combat_pressure" })
        {
            Assert.Contains(family, skeleton.FeatureFamilies);
        }
    }
}
