using LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignInvalidMatrixTests
{
    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakScenariosWithCausalDiagnostics()
    {
        var result = FullMediaBoundGeneratorCampaignTestFactory.BuildFromRepo();

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(FullMediaBoundGeneratorCampaignVocabulary.RequiredInvalidScenarioIds.Count, result.InvalidMatrix.MatchedExpectationCount);
        foreach (var scenarioId in FullMediaBoundGeneratorCampaignVocabulary.RequiredInvalidScenarioIds)
        {
            var scenario = Assert.Single(result.InvalidMatrix.Scenarios, item => item.ScenarioId == scenarioId);
            Assert.False(scenario.ActualValid);
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.CausalMutation);
            Assert.NotEmpty(scenario.Diagnostics);
        }
    }
}
