using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class CanonicalRuntimeSelectedCandidatePlaythroughServiceTests
{
    [Fact]
    public void SelectedCandidateRunsThroughCanonicalRuntimeAndReplays()
    {
        var root = ProjectRoot();
        var handoffPath = Path.Combine(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .DefaultSelectedCandidateHandoffPath);
        var packagePath = Path.Combine(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .DefaultSelectedCandidatePackagePath);
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(packagePath);
        var candidateId =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.ReadCandidateId(handoffPath);
        var request = new CanonicalRuntimeSelectedCandidatePlaythroughRequest
        {
            CandidateId = candidateId,
            HandoffPath = handoffPath,
            PackagePath = packagePath
        };

        var result = CanonicalRuntimeSelectedCandidatePlaythroughService
            .CreateDefault()
            .Execute(package, request);

        Assert.Equal("minimal-map-game-balanced-baseline", result.CandidateId);
        Assert.True(result.CanonicalRuntimeStarted);
        Assert.True(result.SelectedCandidateExecutedByRuntime);
        Assert.False(result.ProjectionOnly);
        Assert.False(result.RuntimePrimitiveMissing, string.Join(Environment.NewLine, result.MissingRuntimePrimitives));
        Assert.True(result.RuntimeCommandCount >= 6);
        Assert.True(result.RuntimeEventCount >= 6);
        Assert.True(result.StateHashChainPresent);
        Assert.True(result.SaveLoadReplay.Passed, string.Join(Environment.NewLine, result.SaveLoadReplay.Diagnostics));
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains(result.Transcript, item =>
            item.Source == "gameplay-runtime"
            && item.EventType == "RecipeCrafted"
            && item.TargetId == "recipe/healing_potion");
        Assert.Contains(result.Transcript, item =>
            item.Source == "gameplay-runtime"
            && item.EventType == "EncounterStarted"
            && item.TargetId == "encounter/goblin_duel");
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
