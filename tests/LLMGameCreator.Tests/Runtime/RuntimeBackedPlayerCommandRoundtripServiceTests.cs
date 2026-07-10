using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class RuntimeBackedPlayerCommandRoundtripServiceTests
{
    [Fact]
    public void ExecutesGoal140ControlsThroughRuntimeBackedRoundtripBridge()
    {
        var root = ProjectRoot();
        var packagePath = Path.Combine(
            root,
            RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultSelectedCandidatePackagePath);
        var handoffPath = Path.Combine(
            root,
            RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultSelectedCandidateHandoffPath);
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(packagePath);
        var candidateId =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.ReadCandidateId(handoffPath);

        var result = RuntimeBackedPlayerCommandRoundtripService
            .CreateDefault()
            .Execute(package, Request(root, candidateId, packagePath, handoffPath));

        Assert.Equal("minimal-map-game-balanced-baseline", result.CandidateId);
        Assert.True(result.RuntimeBackedPlayerCommandRoundtripPassed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.RoundtripSemanticCorrectnessPassed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(6, result.TotalControlRequestCount);
        Assert.Equal(6, result.RoundtripRequestCount);
        Assert.Equal(4, result.RuntimeRoutedRequestCount);
        Assert.Equal(2, result.PresentationOnlyRequestCount);
        Assert.Equal(4, result.RuntimeExecutedRequestCount);
        Assert.Equal(0, result.PresentationOnlyRuntimeExecutionCount);
        Assert.Equal(0, result.RuntimeMutatingPresentationRequestCount);
        Assert.Equal(6, result.ResponseCount);
        Assert.True(result.RoundtripSnapshotCount >= result.RuntimeExecutedRequestCount);
        Assert.True(result.ControlRequestBridgePresent);
        Assert.True(result.StateHashChainPresent);
        Assert.True(result.RequestResponseCorrelationPassed);
        Assert.True(result.SequentialCursorContinuityPassed);
        Assert.True(result.StateHashContinuityPassed);
        Assert.True(result.CopySummaryStateUnchanged);
        Assert.True(result.LoadModelStateUnchanged);
        Assert.True(result.PlayAllExecutedRemainingCommands);
        Assert.True(result.NoControlIntentMappedToUnrelatedGameplayCommand);
        Assert.True(result.RuntimeAuthority);
        Assert.False(result.ProjectionOnly);
        Assert.False(result.UnityGameplayTruth);
        Assert.True(result.UnityConsumesRoundtripResult);
        Assert.True(result.NoUnclassifiedErrorDiagnostics);
        foreach (var intent in RuntimeBackedPlayerCommandRoundtripVocabulary.RequiredControlIntents)
        {
            Assert.Contains(result.Requests, request => request.ControlIntent == intent);
        }

        foreach (var coverage in RuntimeBackedPlayerCommandRoundtripVocabulary.RequiredRuntimeCommandCoverage)
        {
            Assert.Contains(result.Snapshots, snapshot => snapshot.RuntimeCommandCoverage == coverage);
        }

        var loadModel = Assert.Single(result.Responses, response =>
            response.ControlIntent == "load_model");
        Assert.Equal("presentation_only", loadModel.Route);
        Assert.False(loadModel.RuntimeExecuted);
        Assert.False(loadModel.CanonicalStepRuntimeExecuted);
        Assert.False(loadModel.RuntimeMutation);
        Assert.Equal(0, loadModel.ExecutedCommandCount);
        Assert.Equal(0, loadModel.EventCount);
        Assert.Equal(loadModel.StateHashBefore, loadModel.StateHashAfter);

        var copySummary = Assert.Single(result.Responses, response =>
            response.ControlIntent == "copy_frame_summary");
        Assert.Equal("presentation_only", copySummary.Route);
        Assert.False(copySummary.RuntimeExecuted);
        Assert.False(copySummary.CanonicalStepRuntimeExecuted);
        Assert.False(copySummary.RuntimeMutation);
        Assert.Equal(0, copySummary.ExecutedCommandCount);
        Assert.Equal(0, copySummary.EventCount);
        Assert.Equal(copySummary.StateHashBefore, copySummary.StateHashAfter);
        Assert.Equal(-1, copySummary.Snapshot.CanonicalStepIndex);
        Assert.NotEqual("combat_round", copySummary.Snapshot.CanonicalStepId);

        Assert.All(result.Responses.Where(response => response.RuntimeExecuted), response =>
            Assert.True(response.ExecutedCommandCount > 0, response.ControlIntent));
    }

    internal static RuntimeBackedPlayerCommandRoundtripRequest Request(
        string root,
        string candidateId,
        string packagePath,
        string handoffPath) =>
        new()
        {
            CandidateId = candidateId,
            PackagePath = Relative(root, packagePath),
            HandoffPath = Relative(root, handoffPath),
            ControlsUxModelPath =
                RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultControlsUxModelPath,
            ControlsUxResultPath =
                RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultControlsUxResultPath,
            ControlsUxScriptPath =
                RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultControlsUxScriptPath,
            CommandLoopSnapshotsPath =
                RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultCommandLoopSnapshotsPath,
            CommandLoopResultPath =
                RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultCommandLoopResultPath
        };

    internal static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    internal static string ProjectRoot()
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
