using LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyInvalidMatrixTests
{
    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakScenariosWithCausalDiagnostics()
    {
        var result = UnityAlphaMultiFamilyTestFactory.BuildFromRepo();

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(UnityAlphaMultiFamilyPlayableLoopVocabulary.RequiredInvalidScenarioIds.Count, result.InvalidMatrix.MatchedExpectationCount);
        foreach (var scenarioId in UnityAlphaMultiFamilyPlayableLoopVocabulary.RequiredInvalidScenarioIds)
        {
            var scenario = Assert.Single(result.InvalidMatrix.Scenarios, item => item.ScenarioId == scenarioId);
            Assert.False(scenario.ActualValid);
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.CausalMutation);
            Assert.NotEmpty(scenario.Diagnostics);
        }
    }
}
