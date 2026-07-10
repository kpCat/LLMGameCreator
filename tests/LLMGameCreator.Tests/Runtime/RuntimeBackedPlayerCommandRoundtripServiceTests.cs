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
        Assert.True(result.RoundtripRequestCount >= 6);
        Assert.True(result.RuntimeExecutedRequestCount >= 6);
        Assert.True(result.RoundtripSnapshotCount >= result.RuntimeExecutedRequestCount);
        Assert.True(result.ControlRequestBridgePresent);
        Assert.True(result.StateHashChainPresent);
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
            Assert.Contains(result.Requests, request => request.RuntimeCommandCoverage == coverage);
        }

        Assert.Contains(result.Responses, response =>
            response.ControlIntent == "copy_frame_summary"
            && response.Snapshot.CombatSummary.Contains("encounter", StringComparison.Ordinal));
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
