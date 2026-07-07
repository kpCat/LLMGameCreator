using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class CanonicalRuntimePlayerLoopReadinessServiceTests
{
    [Fact]
    public void Goal134TranscriptBuildsRequiredPlayerLoopCategories()
    {
        var root = ProjectRoot();
        var transcriptPath = Path.Combine(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary.DefaultCanonicalRuntimeTranscriptPath);
        var stateSummaryPath = Path.Combine(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary.DefaultCanonicalRuntimeStateSummaryPath);
        var transcript =
            CanonicalRuntimePlayerLoopReadinessArtifactService.ReadTranscript(transcriptPath);
        var stateSummary =
            CanonicalRuntimePlayerLoopReadinessArtifactService.ReadStateSummary(stateSummaryPath);

        var result = new CanonicalRuntimePlayerLoopReadinessService().Build(
            transcript,
            stateSummary,
            new CanonicalRuntimePlayerLoopReadinessRequest
            {
                TranscriptPath = CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DefaultCanonicalRuntimeTranscriptPath,
                StateSummaryPath = CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DefaultCanonicalRuntimeStateSummaryPath,
                DashboardPath = CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DefaultCanonicalRuntimeDashboardPath
            },
            saveLoadReplayStillReferenced: true,
            selectedCandidateExecutedByRuntime: true);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal("minimal-map-game-balanced-baseline", result.CandidateId);
        Assert.True(result.PlayerAdapterContractPresent);
        Assert.True(result.PlayerLoopPlanPresent);
        Assert.False(result.ProjectionOnly);
        Assert.False(result.UnityGameplayTruth);
        Assert.True(result.CanonicalRuntimeSource);
        Assert.True(result.RequiredStepCategoriesPresent);
        Assert.Empty(result.MissingStepCategories);
        Assert.True(result.PlayerLoopStepCount >= 13);
        foreach (var category in CanonicalRuntimePlayerLoopReadinessService.RequiredStepCategories)
        {
            Assert.Contains(result.Steps, step => step.Category == category);
        }

        Assert.All(result.Steps, step => Assert.True(step.CanonicalRuntimeAuthority));
        Assert.Contains(result.FeatureModuleCoverageHints, hint => hint == "feature.crafting.recipes");
        Assert.Contains(result.FeatureModuleCoverageHints, hint => hint == "feature.combat.round");
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
