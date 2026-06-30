using LLMGameCreator.Application.Design.FullGeneratorVariabilityRegressionMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullGeneratorVariabilityRegressionMatrix;

public sealed class FullGeneratorVariabilityRegressionMatrixInvalidMatrixTests
{
    [Fact]
    public void InvalidMatrixCoversRequiredFakeLeakAndBoundaryScenariosWithCausalDiagnostics()
    {
        var result = FullGeneratorVariabilityRegressionMatrixTestFactory.BuildFromRepo();

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(FullGeneratorVariabilityMatrixVocabulary.RequiredInvalidScenarioIds.Count, result.InvalidMatrix.MatchedExpectationCount);
        foreach (var scenarioId in FullGeneratorVariabilityMatrixVocabulary.RequiredInvalidScenarioIds)
        {
            var scenario = Assert.Single(result.InvalidMatrix.Scenarios, item => item.ScenarioId == scenarioId);
            Assert.False(scenario.ActualValid);
            Assert.Equal(scenario.ExpectedStatus, scenario.ActualStatus);
            Assert.NotEmpty(scenario.CausalMutation);
            Assert.NotEmpty(scenario.Diagnostics);
        }
    }
}
