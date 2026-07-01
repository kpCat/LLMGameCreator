using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.SchemaDrivenCampaignEditValidateApplyLoop;

public sealed class SchemaDrivenCampaignEditValidateApplyInvalidMatrixTests
{
    [Fact]
    public void InvalidMatrixRejectsEveryRequiredScenarioCausally()
    {
        var result = new SchemaDrivenCampaignEditEvidenceService().Build(ProjectRoot());

        Assert.True(result.InvalidMatrix.Passed);
        Assert.Equal(
            SchemaDrivenCampaignEditVocabulary.RequiredInvalidScenarioIds.Count,
            result.InvalidMatrix.ScenarioCount);
        foreach (var scenarioId in SchemaDrivenCampaignEditVocabulary.RequiredInvalidScenarioIds)
        {
            var scenario = Assert.Single(result.InvalidMatrix.Scenarios, item => item.ScenarioId == scenarioId);
            Assert.Equal("rejected", scenario.ActualStatus);
            Assert.Contains(
                scenario.Diagnostics,
                diagnostic => diagnostic.Code == "goal075.invalid." + scenarioId);
        }
    }

    [Fact]
    public void ValidCandidatesHaveNoInvalidDiagnostics()
    {
        var result = new SchemaDrivenCampaignEditEvidenceService().Build(ProjectRoot());

        Assert.True(result.ValidationMatrix.Passed);
        Assert.Equal(18, result.ValidationMatrix.ValidCandidateCount);
        Assert.Equal(0, result.ValidationMatrix.RejectedCandidateCount);
        Assert.All(result.ValidationMatrix.Records, record =>
        {
            Assert.True(record.Valid);
            Assert.Empty(record.Diagnostics);
        });
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
