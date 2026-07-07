using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

[Collection("UnityAlphaProductSmoke")]
public sealed class RuntimeBackedUnityPlayerLoopStepperScriptProof
{
    [Fact]
    public async Task WritesGoal138RuntimeBackedUnityPlayerLoopStepperArtifacts()
    {
        var root = ProjectRoot();
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL138_OUTPUT_ROOT"),
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var request = new RuntimeBackedUnityPlayerLoopStepperRequest
        {
            PlaybackFramesPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL138_PLAYBACK_FRAMES_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultPlaybackFramesPath))),
            PlaybackResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL138_PLAYBACK_RESULT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultPlaybackResultPath))),
            CommandLoopSnapshotsPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL138_COMMAND_LOOP_SNAPSHOTS_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultCommandLoopSnapshotsPath))),
            CommandLoopResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL138_COMMAND_LOOP_RESULT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultCommandLoopResultPath))),
            PlayerAdapterContractPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL138_PLAYER_ADAPTER_CONTRACT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultPlayerAdapterContractPath)))
        };
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL138_UNITY_SMOKE_PATH");
        var unitySmoke = string.IsNullOrWhiteSpace(unitySmokePath)
            ? PassedUnitySmoke(root, outputRoot)
            : RuntimeBackedUnityPlayerLoopStepperArtifactService.ReadUnitySmoke(unitySmokePath);

        var write = await new RuntimeBackedUnityPlayerLoopStepperArtifactService()
            .BuildAndWriteAsync(root, request, outputRoot, exportRoot, unitySmoke);

        Assert.Equal("GREEN", write.Dashboard.Status);
        Assert.True(write.Dashboard.AcceptedGoal137);
        Assert.False(write.Dashboard.Accepted);
        Assert.Equal("minimal-map-game-balanced-baseline", write.Dashboard.CandidateId);
        Assert.Equal(13, write.Dashboard.FrameCount);
        Assert.True(write.Dashboard.RequiredFrameCategoriesPresent);
        Assert.True(write.Dashboard.RuntimeAuthority);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.True(write.Dashboard.StepperWindowPresent);
        Assert.True(write.Dashboard.StepperBatchSmokePassed);
        Assert.True(write.Dashboard.ManualUnityOptional);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName,
                StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                RuntimeBackedUnityPlayerLoopStepperVocabulary.Goal137AcceptanceFileName,
                StringComparison.Ordinal));

        var modelPath = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName);
        using var model = JsonDocument.Parse(await File.ReadAllTextAsync(modelPath));
        Assert.Equal(0, model.RootElement.GetProperty("currentFrameIndex").GetInt32());
        var categories = model.RootElement.GetProperty("frames").EnumerateArray()
            .Select(frame => frame.GetProperty("frameCategory").GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var category in RuntimeBackedUnityPlayerLoopStepperVocabulary.RequiredFrameCategories)
        {
            Assert.Contains(category, categories);
        }

        var acceptanceText = await File.ReadAllTextAsync(
            Path.Combine(root, RuntimeBackedUnityPlayerLoopStepperVocabulary.Goal137AcceptanceDocumentationPath));
        Assert.Contains("accepted=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("acceptedByHuman=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("acceptedByCodex=false", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("rawManualInputNotCommitted=true", acceptanceText, StringComparison.Ordinal);
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
            return RuntimeBackedUnityPlayerLoopStepperVocabulary.ExportPackageDirectory;
        }

        return ".llmgc/exports/" + outputRoot[proceduralPrefix.Length..];
    }

    private static RuntimeBackedUnityPlayerLoopStepperUnitySmoke PassedUnitySmoke(
        string root,
        string outputRoot)
    {
        var model = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName);
        return new RuntimeBackedUnityPlayerLoopStepperUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            FrameCountPassed = true,
            RequiredFrameCategoriesPresent = true,
            RuntimeAuthorityMarkersPresent = true,
            StepperWindowPresent = true,
            StepperBatchSmokePassed = true,
            Passed = true,
            UnityPath = "test-unity",
            ModelPath = Relative(root, model),
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
