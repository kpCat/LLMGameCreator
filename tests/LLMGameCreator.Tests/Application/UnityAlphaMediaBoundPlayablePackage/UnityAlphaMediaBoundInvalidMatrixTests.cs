using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundInvalidMatrixTests
{
    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakScenariosWithCausalDiagnostics()
    {
        var result = UnityAlphaMediaBoundTestFactory.BuildFromRepo();

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(UnityAlphaMediaBoundPlayablePackageVocabulary.RequiredInvalidScenarioIds.Count, result.InvalidMatrix.MatchedExpectationCount);
        foreach (var scenarioId in UnityAlphaMediaBoundPlayablePackageVocabulary.RequiredInvalidScenarioIds)
        {
            var scenario = Assert.Single(result.InvalidMatrix.Scenarios, item => item.ScenarioId == scenarioId);
            Assert.False(scenario.ActualValid);
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.CausalMutation);
            Assert.NotEmpty(scenario.Diagnostics);
        }
    }
}
