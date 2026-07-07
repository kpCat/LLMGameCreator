using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimeUnityPlayerLoopPlaybackScriptProof
{
    [Fact]
    public async Task WritesGoal137UnityPlayerLoopPlaybackArtifacts()
    {
        var root = ProjectRoot();
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL137_OUTPUT_ROOT"),
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var request = new CanonicalRuntimeUnityPlayerLoopPlaybackRequest
        {
            CommandLoopSnapshotsPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL137_COMMAND_LOOP_SNAPSHOTS_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultCommandLoopSnapshotsPath))),
            CommandLoopResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL137_COMMAND_LOOP_RESULT_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultCommandLoopResultPath))),
            PlayerAdapterContractPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL137_PLAYER_ADAPTER_CONTRACT_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultPlayerAdapterContractPath))),
            StateSummaryPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL137_STATE_SUMMARY_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultStateSummaryPath)))
        };
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL137_UNITY_SMOKE_PATH");
        var unitySmoke = string.IsNullOrWhiteSpace(unitySmokePath)
            ? PassedUnitySmoke(root, outputRoot)
            : CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService.ReadUnitySmoke(unitySmokePath);

        var write = await new CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService()
            .BuildAndWriteAsync(root, request, outputRoot, exportRoot, unitySmoke);

        Assert.Equal("GREEN", write.Dashboard.Status);
        Assert.Equal("minimal-map-game-balanced-baseline", write.Dashboard.CandidateId);
        Assert.Equal(13, write.Dashboard.PlaybackFrameCount);
        Assert.True(write.Dashboard.RequiredFrameCategoriesPresent);
        Assert.True(write.Dashboard.UnityPlayerLoopPlaybackPassed);
        Assert.True(write.Dashboard.RuntimeSnapshotSource);
        Assert.True(write.Dashboard.UnityConsumesRuntimeSnapshots);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.True(write.Dashboard.SelectedCandidateExecutedByRuntime);
        Assert.True(write.Dashboard.ManualUnityOptional);
        Assert.False(write.Dashboard.Accepted);
        Assert.True(write.Dashboard.NoUnclassifiedErrorDiagnostics);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName,
                StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultFileName,
                StringComparison.Ordinal));

        var framesPath = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName);
        using var frames = JsonDocument.Parse(await File.ReadAllTextAsync(framesPath));
        var categories = frames.RootElement.EnumerateArray()
            .Select(frame => frame.GetProperty("category").GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var category in CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.RequiredFrameCategories)
        {
            Assert.Contains(category, categories);
        }
    }

    private static string EnvOrDefault(string name, string defaultPath)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultPath : value;
    }

    private static string RelativeOrDefault(string root, string? path, string defaultRelative)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return defaultRelative;
        }

        var full = Path.IsPathRooted(path) ? path : Path.Combine(root, path);
        return Relative(root, full);
    }

    private static string ToExportRoot(string outputRoot)
    {
        const string proceduralPrefix = ".llmgc/procedural/";
        if (!outputRoot.StartsWith(proceduralPrefix, StringComparison.Ordinal))
        {
            return CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ExportPackageDirectory;
        }

        return ".llmgc/exports/" + outputRoot[proceduralPrefix.Length..];
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke PassedUnitySmoke(
        string root,
        string outputRoot)
    {
        var frames = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName);
        var result = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName);
        return new CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke
        {
            UnityAvailable = true,
            FramesPathExists = true,
            ResultPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            FrameCountPassed = true,
            RequiredFrameCategoriesPresent = true,
            RuntimeAuthorityMarkersPresent = true,
            UnityPlayerLoopPlaybackPassed = true,
            Passed = true,
            UnityPath = "test-unity",
            FramesPath = Relative(root, frames),
            ResultPath = Relative(root, result),
            Status = "GREEN"
        };
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

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
