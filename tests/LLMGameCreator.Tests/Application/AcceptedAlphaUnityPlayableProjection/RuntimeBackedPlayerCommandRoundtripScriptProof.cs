using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

[Collection("UnityAlphaProductSmoke")]
public sealed class RuntimeBackedPlayerCommandRoundtripScriptProof
{
    [Fact]
    public async Task WritesGoal141RuntimeBackedPlayerCommandRoundtripArtifacts()
    {
        var root = ProjectRoot();
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL141_OUTPUT_ROOT"),
            RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var packagePath = EnvOrDefault(
            "LLMGC_GOAL141_SELECTED_CANDIDATE_PACKAGE_PATH",
            Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultSelectedCandidatePackagePath));
        var handoffPath = EnvOrDefault(
            "LLMGC_GOAL141_SELECTED_CANDIDATE_HANDOFF_PATH",
            Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultSelectedCandidateHandoffPath));
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(packagePath);
        var candidateId =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.ReadCandidateId(handoffPath);
        var request = new RuntimeBackedPlayerCommandRoundtripRequest
        {
            CandidateId = candidateId,
            PackagePath = Relative(root, packagePath),
            HandoffPath = Relative(root, handoffPath),
            ControlsUxModelPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL141_CONTROLS_UX_MODEL_PATH",
                    Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultControlsUxModelPath))),
            ControlsUxResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL141_CONTROLS_UX_RESULT_PATH",
                    Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultControlsUxResultPath))),
            ControlsUxScriptPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL141_CONTROLS_UX_SCRIPT_PATH",
                    Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultControlsUxScriptPath))),
            CommandLoopSnapshotsPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL141_COMMAND_LOOP_SNAPSHOTS_PATH",
                    Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultCommandLoopSnapshotsPath))),
            CommandLoopResultPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL141_COMMAND_LOOP_RESULT_PATH",
                    Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DefaultCommandLoopResultPath)))
        };
        var runtimeResult = RuntimeBackedPlayerCommandRoundtripService
            .CreateDefault()
            .Execute(package, request);
        var smokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL141_UNITY_SMOKE_PATH");
        var smoke = string.IsNullOrWhiteSpace(smokePath)
            ? PassedUnitySmoke(root, outputRoot)
            : RuntimeBackedPlayerCommandRoundtripArtifactService.ReadUnitySmoke(smokePath);

        var write = await new RuntimeBackedPlayerCommandRoundtripArtifactService()
            .BuildAndWriteAsync(root, request, runtimeResult, outputRoot, exportRoot, smoke);

        Assert.Equal("GREEN", write.Dashboard.Status);
        Assert.True(write.Dashboard.Goal140Accepted);
        Assert.False(write.Dashboard.Accepted);
        Assert.Equal("minimal-map-game-balanced-baseline", write.Dashboard.CandidateId);
        Assert.True(write.Dashboard.RoundtripSemanticCorrectnessPassed);
        Assert.Equal(6, write.Dashboard.TotalControlRequestCount);
        Assert.Equal(6, write.Dashboard.RoundtripRequestCount);
        Assert.Equal(4, write.Dashboard.RuntimeRoutedRequestCount);
        Assert.Equal(2, write.Dashboard.PresentationOnlyRequestCount);
        Assert.Equal(4, write.Dashboard.RuntimeExecutedRequestCount);
        Assert.Equal(0, write.Dashboard.PresentationOnlyRuntimeExecutionCount);
        Assert.Equal(0, write.Dashboard.RuntimeMutatingPresentationRequestCount);
        Assert.Equal(6, write.Dashboard.ResponseCount);
        Assert.True(write.Dashboard.RoundtripSnapshotCount >= write.Dashboard.RuntimeExecutedRequestCount);
        Assert.True(write.Dashboard.ControlRequestBridgePresent);
        Assert.True(write.Dashboard.StateHashChainPresent);
        Assert.True(write.Dashboard.RequestResponseCorrelationPassed);
        Assert.True(write.Dashboard.SequentialCursorContinuityPassed);
        Assert.True(write.Dashboard.StateHashContinuityPassed);
        Assert.True(write.Dashboard.CopySummaryStateUnchanged);
        Assert.True(write.Dashboard.LoadModelStateUnchanged);
        Assert.True(write.Dashboard.PlayAllExecutedRemainingCommands);
        Assert.True(write.Dashboard.NoControlIntentMappedToUnrelatedGameplayCommand);
        Assert.True(write.Dashboard.RuntimeAuthority);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.True(write.Dashboard.UnityConsumesRoundtripResult);
        Assert.True(write.Dashboard.UnitySmokePassed);
        Assert.True(write.Dashboard.NoUnclassifiedErrorDiagnostics);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        foreach (var fileName in RequiredGoal141Files())
        {
            Assert.Contains(write.WrittenFiles, path => path == outputRoot + "/" + fileName);
            Assert.Contains(write.WrittenFiles, path => path == exportRoot + "/" + fileName);
        }

        foreach (var fileName in RequiredGoal141AFiles())
        {
            Assert.Contains(write.WrittenFiles, path =>
                path == RuntimeBackedPlayerCommandRoundtripVocabulary
                    .SemanticCorrectnessProceduralOutputDirectory
                + "/"
                + fileName);
            Assert.Contains(write.WrittenFiles, path =>
                path == RuntimeBackedPlayerCommandRoundtripVocabulary
                    .SemanticCorrectnessExportPackageDirectory
                + "/"
                + fileName);
        }

        var acceptancePath = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140AcceptanceFileName);
        using var acceptance = JsonDocument.Parse(await File.ReadAllTextAsync(acceptancePath));
        Assert.True(acceptance.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("acceptedByHuman").GetBoolean());
        Assert.False(acceptance.RootElement.GetProperty("acceptedByCodex").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("rawManualInputNotCommitted").GetBoolean());
        Assert.Equal("minimal-map-game-balanced-baseline",
            acceptance.RootElement.GetProperty("selectedCandidate").GetString());
        Assert.Equal(13, acceptance.RootElement.GetProperty("frames").GetInt32());
        Assert.True(acceptance.RootElement.GetProperty("humanReadableFrameNumbering").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("stepOnceSemanticsClear").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("playAllToEndSemanticsClear").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("copyFrameSummaryStatusPresent").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("knownUnityEditorNoiseClassified").GetBoolean());
        Assert.Equal(0, acceptance.RootElement.GetProperty("blockingUnityErrorCount").GetInt32());
        Assert.False(acceptance.RootElement.GetProperty("projectionOnly").GetBoolean());
        Assert.True(acceptance.RootElement.GetProperty("runtimeAuthority").GetBoolean());
        Assert.False(acceptance.RootElement.GetProperty("unityGameplayTruth").GetBoolean());

        var docsText = await File.ReadAllTextAsync(
            Path.Combine(root, RuntimeBackedPlayerCommandRoundtripVocabulary.DocumentationPath));
        Assert.Contains("accepted=false", docsText, StringComparison.Ordinal);
        Assert.Contains("goal140Accepted=true", docsText, StringComparison.Ordinal);
        Assert.Contains("runtimeExecutedRequestCount=4", docsText, StringComparison.Ordinal);
        Assert.Contains("presentationOnlyRuntimeExecutionCount=0", docsText, StringComparison.Ordinal);
        Assert.Contains("roundtripSemanticCorrectnessPassed=true", docsText, StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> RequiredGoal141Files() =>
    [
        RuntimeBackedPlayerCommandRoundtripVocabulary.Goal140AcceptanceFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.RequestFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.ResultFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.SessionFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.SnapshotsFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.ModelFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.DashboardFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.NegativeProofFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.FileIndexFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.UnitySmokeFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.ReportJsonFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.ReportMarkdownFileName
    ];

    private static IReadOnlyList<string> RequiredGoal141AFiles() =>
    [
        RuntimeBackedPlayerCommandRoundtripVocabulary.SemanticCorrectnessDashboardFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.SemanticCorrectnessRegressionProofFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.SemanticCorrectnessReportFileName,
        RuntimeBackedPlayerCommandRoundtripVocabulary.SemanticCorrectnessFileIndexFileName
    ];

    private static RuntimeBackedPlayerCommandRoundtripUnitySmoke PassedUnitySmoke(
        string root,
        string outputRoot)
    {
        var model = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedPlayerCommandRoundtripVocabulary.ModelFileName);
        var result = Path.Combine(
            root,
            outputRoot,
            RuntimeBackedPlayerCommandRoundtripVocabulary.ResultFileName);
        return new RuntimeBackedPlayerCommandRoundtripUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = true,
            RoundtripRequestCountPassed = true,
            PresentationOnlyRequestCountPassed = true,
            PresentationOnlyRuntimeExecutionCountPassed = true,
            RequestResponseCorrelationPassed = true,
            SequentialCursorContinuityPassed = true,
            CopySummaryStateUnchanged = true,
            LoadModelStateUnchanged = true,
            NoControlIntentMappedToUnrelatedGameplayCommand = true,
            RuntimeSnapshotResponsePresent = true,
            RuntimeAuthorityMarkersPresent = true,
            UnityConsumesRoundtripResult = true,
            UnityGameplayTruth = false,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            Passed = true,
            UnityPath = "test-unity",
            ModelPath = Relative(root, model),
            ResultPath = Relative(root, result),
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
            return RuntimeBackedPlayerCommandRoundtripVocabulary.ExportPackageDirectory;
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
