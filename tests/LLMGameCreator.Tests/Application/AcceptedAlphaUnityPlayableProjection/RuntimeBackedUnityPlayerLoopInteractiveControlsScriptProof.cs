using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

[Collection("UnityAlphaProductSmoke")]
public sealed class RuntimeBackedUnityPlayerLoopInteractiveControlsScriptProof
{
    [Fact]
    public async Task WritesGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsArtifacts()
    {
        var root = ProjectRoot();
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL139_OUTPUT_ROOT"),
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var request = new RuntimeBackedUnityPlayerLoopInteractiveControlsRequest
        {
            StepperModelPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL139_STEPPER_MODEL_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultStepperModelPath))),
            StepperResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL139_STEPPER_RESULT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultStepperResultPath))),
            PlaybackFramesPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL139_PLAYBACK_FRAMES_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultPlaybackFramesPath))),
            CommandLoopSnapshotsPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL139_COMMAND_LOOP_SNAPSHOTS_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultCommandLoopSnapshotsPath))),
            PlayerAdapterContractPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL139_PLAYER_ADAPTER_CONTRACT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultPlayerAdapterContractPath)))
        };
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL139_UNITY_SMOKE_PATH");
        var unitySmoke = string.IsNullOrWhiteSpace(unitySmokePath)
            ? PassedUnitySmoke(root, outputRoot)
            : RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService.ReadUnitySmoke(unitySmokePath);

        var write = await new RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService()
            .BuildAndWriteAsync(root, request, outputRoot, exportRoot, unitySmoke);

        Assert.Equal("GREEN", write.Dashboard.Status);
        Assert.True(write.Dashboard.AcceptedGoal138);
        Assert.False(write.Dashboard.Accepted);
        Assert.Equal("minimal-map-game-balanced-baseline", write.Dashboard.CandidateId);
        Assert.Equal(13, write.Dashboard.FrameCount);
        Assert.True(write.Dashboard.RequiredControlsPresent);
        Assert.True(write.Dashboard.ControlScriptPassed);
        Assert.True(write.Dashboard.InteractiveControlsWindowPresent);
        Assert.True(write.Dashboard.UnityInteractiveControlsSmokePassed);
        Assert.True(write.Dashboard.RuntimeAuthority);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.True(write.Dashboard.ManualUnityOptional);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName,
                StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName,
                StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.Goal138AcceptanceFileName,
                StringComparison.Ordinal));

        var modelPath = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName);
        using var model = JsonDocument.Parse(await File.ReadAllTextAsync(modelPath));
        Assert.Equal(0, model.RootElement.GetProperty("currentFrameIndex").GetInt32());
        Assert.True(model.RootElement.GetProperty("runtimeAuthority").GetBoolean());
        Assert.False(model.RootElement.GetProperty("projectionOnly").GetBoolean());
        Assert.False(model.RootElement.GetProperty("unityGameplayTruth").GetBoolean());
        var controls = model.RootElement.GetProperty("controls").EnumerateArray()
            .Select(control => control.GetProperty("id").GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var control in RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.RequiredControls)
        {
            Assert.Contains(control, controls);
        }

        var sessionPath = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.SessionFileName);
        using var session = JsonDocument.Parse(await File.ReadAllTextAsync(sessionPath));
        Assert.True(session.RootElement.GetProperty("controlScriptPassed").GetBoolean());
        Assert.True(session.RootElement.GetProperty("finalFrameReachable").GetBoolean());
        Assert.Equal(12, session.RootElement.GetProperty("finalFrameIndex").GetInt32());

        var acceptanceText = await File.ReadAllTextAsync(
            Path.Combine(
                root,
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.Goal138AcceptanceDocumentationPath));
        Assert.Contains("accepted=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("acceptedByHuman=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("acceptedByCodex=false", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("selectedCandidate=minimal-map-game-balanced-baseline", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("stepperFrames=13", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("stepperBatchSmoke=GREEN", acceptanceText, StringComparison.Ordinal);
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
            return RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ExportPackageDirectory;
        }

        return ".llmgc/exports/" + outputRoot[proceduralPrefix.Length..];
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke PassedUnitySmoke(
        string root,
        string outputRoot)
    {
        var model = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName);
        var script = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName);
        return new RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke
        {
            UnityAvailable = true,
            InteractiveModelPathExists = true,
            ControlScriptPathExists = true,
            FrameCountPassed = true,
            RequiredControlsPresent = true,
            ControlScriptPassed = true,
            RuntimeAuthorityMarkersPresent = true,
            InteractiveControlsWindowPresent = true,
            UnityGameplayTruth = false,
            Passed = true,
            UnityPath = "test-unity",
            InteractiveModelPath = Relative(root, model),
            ControlScriptPath = Relative(root, script),
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
