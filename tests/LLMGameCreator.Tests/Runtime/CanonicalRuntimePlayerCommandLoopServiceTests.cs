using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class CanonicalRuntimePlayerCommandLoopServiceTests
{
    [Fact]
    public void SelectedCandidateRunsPlayerCommandLoopThroughRuntime()
    {
        var root = ProjectRoot();
        var handoffPath = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidateHandoffPath);
        var packagePath = Path.Combine(
            root,
            CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidatePackagePath);
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(packagePath);
        var candidateId =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.ReadCandidateId(handoffPath);

        var result = CanonicalRuntimePlayerCommandLoopService
            .CreateDefault()
            .Execute(package, new CanonicalRuntimePlayerCommandLoopRequest
            {
                CandidateId = candidateId,
                HandoffPath = handoffPath,
                PackagePath = packagePath,
                Goal134TranscriptPath = Path.Combine(
                    root,
                    CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134TranscriptPath),
                Goal134StateSummaryPath = Path.Combine(
                    root,
                    CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134StateSummaryPath),
                Goal135PlayerLoopPlanPath = Path.Combine(
                    root,
                    CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerLoopPlanPath),
                Goal135PlayerAdapterContractPath = Path.Combine(
                    root,
                    CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerAdapterContractPath)
            });

        Assert.Equal("minimal-map-game-balanced-baseline", result.CandidateId);
        Assert.True(result.PlayerCommandLoopPassed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(13, result.PlayerCommandCount);
        Assert.Equal(result.PlayerCommandCount, result.PlayerSnapshotCount);
        Assert.True(result.RuntimeEventCount >= 10);
        Assert.True(result.StateHashChainPresent);
        Assert.True(result.AllRequiredCategoriesPresent);
        Assert.True(result.SelectedCandidateExecutedByRuntime);
        Assert.False(result.ProjectionOnly);
        Assert.False(result.UnityGameplayTruth);
        Assert.False(result.RuntimePrimitiveMissing, string.Join(Environment.NewLine, result.MissingRuntimePrimitives));
        foreach (var category in CanonicalRuntimePlayerCommandLoopService.RequiredCategories)
        {
            Assert.Contains(result.Snapshots, snapshot => snapshot.Category == category);
        }

        Assert.Contains(result.Snapshots, snapshot =>
            snapshot.Category == "craft"
            && snapshot.RuntimeEvents.Any(runtimeEvent =>
                runtimeEvent.EventType == "RecipeCrafted"
                && runtimeEvent.TargetId == "recipe/healing_potion"));
        Assert.Contains(result.Snapshots, snapshot =>
            snapshot.Category == "combat_round"
            && snapshot.RuntimeEvents.Any(runtimeEvent =>
                runtimeEvent.EventType == "DamageApplied"));
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
