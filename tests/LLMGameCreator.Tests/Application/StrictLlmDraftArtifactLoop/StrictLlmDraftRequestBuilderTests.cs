using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftRequestBuilderTests
{
    [Fact]
    public void RequestBuilderProducesDistinctScenarioRequestSets()
    {
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();

        Assert.Equal(["caravan_trade", "frontier_survival", "gothic_intrigue", "metamodule_kingdoms"], requestSets.Select(item => item.ScenarioId).ToArray());
        Assert.Equal(4, requestSets.Select(item => item.StableSummary).Distinct(StringComparer.Ordinal).Count());
        Assert.All(requestSets, set =>
        {
            Assert.NotEmpty(set.Requests);
            Assert.Equal(set.Requests.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).Select(item => item.RequestId), set.Requests.Select(item => item.RequestId));
            Assert.DoesNotContain(StrictLlmDraftArtifactLoopValidator.ValidateRequests(set.Requests, StrictLlmDraftArtifactLoopCatalog.BuildDraftFamilies()), item => item.Severity == "error");
        });
    }

    [Fact]
    public void MetamoduleScenarioRepresentsManySpeciesArchetypeSlotsWithoutFinalProse()
    {
        var metamodule = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets().Single(item => item.ScenarioId == "metamodule_kingdoms");

        Assert.True(metamodule.SpeciesArchetypeSlotRequestCount >= 100);
        Assert.True(metamodule.Requests.Count(item => item.TargetDraftFamily == "species_archetype_feature_draft") >= 100);
        Assert.All(metamodule.Requests, request =>
        {
            Assert.True(request.NoFinalProse);
            Assert.True(request.NoRuntimeAuthority);
            Assert.DoesNotContain("dialogue_line", request.RequiredFields);
            Assert.NotEmpty(request.AllowedArtifactContractIds);
            Assert.NotEmpty(request.AllowedSemanticScopes);
        });
    }
}
