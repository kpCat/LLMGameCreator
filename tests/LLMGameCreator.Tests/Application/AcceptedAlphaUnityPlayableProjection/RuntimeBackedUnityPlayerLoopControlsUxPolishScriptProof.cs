using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

[Collection("UnityAlphaProductSmoke")]
public sealed class RuntimeBackedUnityPlayerLoopControlsUxPolishScriptProof
{
    [Fact]
    public async Task WritesGoal140RuntimeBackedUnityPlayerLoopControlsUxArtifacts()
    {
        var root = ProjectRoot();
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL140_OUTPUT_ROOT"),
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var request = new RuntimeBackedUnityPlayerLoopControlsUxPolishRequest
        {
            InteractiveControlsModelPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL140_INTERACTIVE_CONTROLS_MODEL_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary
                            .DefaultInteractiveControlsModelPath))),
            InteractiveControlsResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL140_INTERACTIVE_CONTROLS_RESULT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary
                            .DefaultInteractiveControlsResultPath))),
            InteractiveControlsScriptPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL140_INTERACTIVE_CONTROLS_SCRIPT_PATH",
                    Path.Combine(
                        root,
                        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary
                            .DefaultInteractiveControlsScriptPath)))
        };
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL140_UNITY_SMOKE_PATH");
        var unitySmoke = string.IsNullOrWhiteSpace(unitySmokePath)
            ? PassedUnitySmoke(root, outputRoot)
            : RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService.ReadUnitySmoke(unitySmokePath);
        var noisePath = Environment.GetEnvironmentVariable(
            "LLMGC_GOAL140_UNITY_NOISE_CLASSIFICATION_PATH");
        var noise = string.IsNullOrWhiteSpace(noisePath)
            ? RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService
                .ClassifyUnityEditorNoise(string.Empty, string.Empty)
            : RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService
                .ReadUnityNoiseClassification(noisePath);

        var write = await new RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService()
            .BuildAndWriteAsync(root, request, outputRoot, exportRoot, unitySmoke, noise);

        Assert.Equal("GREEN", write.Dashboard.Status);
        Assert.True(write.Dashboard.AcceptedGoal139);
        Assert.False(write.Dashboard.Accepted);
        Assert.Equal("minimal-map-game-balanced-baseline", write.Dashboard.SelectedCandidate);
        Assert.Equal(13, write.Dashboard.FrameCount);
        Assert.True(write.Dashboard.HumanReadableFrameNumbering);
        Assert.True(write.Dashboard.StepOnceSemanticsClear);
        Assert.True(write.Dashboard.PlayAllToEndSemanticsClear);
        Assert.True(write.Dashboard.CopyFrameSummaryStatusPresent);
        Assert.True(write.Dashboard.RequiredControlsPresent);
        Assert.True(write.Dashboard.ControlsUxPolished);
        Assert.True(write.Dashboard.UnityControlsUxSmokePassed);
        Assert.True(write.Dashboard.RuntimeAuthority);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.True(write.Dashboard.KnownUnityEditorNoiseClassified);
        Assert.Equal(0, write.Dashboard.BlockingUnityErrorCount);
        Assert.Equal(0, write.Dashboard.UnclassifiedUnityErrorCount);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        foreach (var fileName in RequiredGoal140Files())
        {
            Assert.Contains(write.WrittenFiles, path =>
                path == outputRoot + "/" + fileName);
            Assert.Contains(write.WrittenFiles, path =>
                path == exportRoot + "/" + fileName);
        }

        var modelPath = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName);
        using var model = JsonDocument.Parse(await File.ReadAllTextAsync(modelPath));
        Assert.True(model.RootElement.GetProperty("humanReadableFrameNumbering").GetBoolean());
        Assert.Equal("Current Frame: 1/13", model.RootElement.GetProperty("currentFrameLabel").GetString());
        Assert.Equal("Frame Index: 0", model.RootElement.GetProperty("frameIndexLabel").GetString());
        var controls = model.RootElement.GetProperty("controls").EnumerateArray().ToList();
        Assert.Contains(controls, control =>
            control.GetProperty("id").GetString() == "step_once"
            && control.GetProperty("label").GetString() == "Step Once"
            && control.GetProperty("lastControlAction").GetString() == "step_once");
        Assert.Contains(controls, control =>
            control.GetProperty("id").GetString() == "play_all_to_end"
            && control.GetProperty("label").GetString() == "Play All To End"
            && control.GetProperty("lastControlAction").GetString() == "play_all_to_end");
        Assert.Contains(controls, control =>
            control.GetProperty("id").GetString() == "copy_current_frame_summary"
            && control.GetProperty("statusAfterAction").GetString() == "copied_frame_summary");

        var known = RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService
            .ClassifyUnityEditorNoise(
                "BuildProfileContext asset exists but could not be loaded\n"
                + "NullReferenceException: Object reference not set to an instance of an object\n"
                + "UnityEditor.Build.Profile.BuildProfileContext.CreateOrLoad",
                "fixture.log");
        Assert.True(known.KnownUnityEditorBuildProfileNoiseClassified);
        Assert.Equal(1, known.KnownUnityEditorNoiseCount);
        Assert.Equal(0, known.BlockingUnityErrorCount);
        Assert.Equal(0, known.UnclassifiedUnityErrorCount);

        var unpaired = RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService
            .ClassifyUnityEditorNoise(
                "NullReferenceException: Object reference not set to an instance of an object",
                "fixture.log");
        Assert.True(unpaired.KnownUnityEditorBuildProfileNoiseClassified);
        Assert.Equal(0, unpaired.KnownUnityEditorNoiseCount);
        Assert.Equal(1, unpaired.BlockingUnityErrorCount);
        Assert.Equal(1, unpaired.UnclassifiedUnityErrorCount);

        var acceptanceText = await File.ReadAllTextAsync(
            Path.Combine(
                root,
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.Goal139DocumentationPath));
        Assert.Contains("accepted=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("acceptedByHuman=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("acceptedByCodex=false", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("selectedCandidate=minimal-map-game-balanced-baseline", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("frames=13", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("interactiveControlsSmoke=GREEN", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("autoStepAutoPlayAllUxAcceptedWithFollowUpDebt=true", acceptanceText, StringComparison.Ordinal);
        Assert.Contains("rawManualInputNotCommitted=true", acceptanceText, StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> RequiredGoal140Files() =>
    [
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.Goal139AcceptanceFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ResultFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnitySmokeFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnityNoiseClassificationFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportJsonFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportMarkdownFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NegativeProofFileName,
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.FileIndexFileName
    ];

    private static RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke PassedUnitySmoke(
        string root,
        string outputRoot)
    {
        var model = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName);
        var script = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName);
        return new RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = true,
            FrameCountPassed = true,
            RequiredControlsPresent = true,
            HumanReadableFrameNumberingPresent = true,
            StepOnceSemanticsClear = true,
            PlayAllToEndSemanticsClear = true,
            CopyFrameSummaryStatusPresent = true,
            RuntimeAuthorityMarkersPresent = true,
            UnityGameplayTruth = false,
            Passed = true,
            UnityPath = "test-unity",
            ModelPath = Relative(root, model),
            ScriptPath = Relative(root, script),
            Status = "GREEN"
        };
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
            return RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ExportPackageDirectory;
        }

        return ".llmgc/exports/" + outputRoot[proceduralPrefix.Length..];
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
