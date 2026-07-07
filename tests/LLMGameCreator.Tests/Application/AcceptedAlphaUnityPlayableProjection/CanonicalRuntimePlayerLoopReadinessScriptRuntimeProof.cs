using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimePlayerLoopReadinessScriptRuntimeProof
{
    [Fact]
    public async Task WritesGoal135PlayerLoopReadinessArtifacts()
    {
        var root = ProjectRoot();
        var transcriptPath = EnvOrDefault(
            "LLMGC_GOAL135_CANONICAL_RUNTIME_TRANSCRIPT_PATH",
            Path.Combine(
                root,
                CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DefaultCanonicalRuntimeTranscriptPath));
        var stateSummaryPath = EnvOrDefault(
            "LLMGC_GOAL135_CANONICAL_RUNTIME_STATE_SUMMARY_PATH",
            Path.Combine(
                root,
                CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DefaultCanonicalRuntimeStateSummaryPath));
        var dashboardPath = EnvOrDefault(
            "LLMGC_GOAL135_CANONICAL_RUNTIME_DASHBOARD_PATH",
            Path.Combine(
                root,
                CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DefaultCanonicalRuntimeDashboardPath));
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL135_OUTPUT_ROOT"),
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL135_UNITY_SMOKE_PATH");
        var unitySmoke = string.IsNullOrWhiteSpace(unitySmokePath)
            ? null
            : CanonicalRuntimePlayerLoopReadinessArtifactService.ReadUnitySmoke(unitySmokePath);
        var request = new CanonicalRuntimePlayerLoopReadinessRequest
        {
            TranscriptPath = Relative(root, transcriptPath),
            StateSummaryPath = Relative(root, stateSummaryPath),
            DashboardPath = Relative(root, dashboardPath)
        };
        var runtimeResult = BuildRuntimeResult(root, request);

        var write = await new CanonicalRuntimePlayerLoopReadinessArtifactService()
            .BuildAndWriteAsync(
                root,
                request,
                runtimeResult,
                outputRoot,
                exportRoot,
                unitySmoke);

        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            write.Dashboard.CandidateId);
        Assert.True(write.Dashboard.PlayerAdapterContractPresent);
        Assert.True(write.Dashboard.PlayerLoopPlanPresent);
        Assert.True(write.Dashboard.PlayerLoopStepCount >= 13);
        Assert.True(write.Dashboard.RequiredStepCategoriesPresent);
        Assert.True(write.Dashboard.CanonicalRuntimeSource);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.True(write.Dashboard.SaveLoadReplayStillReferenced);
        Assert.True(write.Dashboard.SelectedCandidateExecutedByRuntime);
        Assert.True(write.Dashboard.NoUnclassifiedErrorDiagnostics);
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName,
                StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                CanonicalRuntimePlayerLoopReadinessVocabulary
                    .DiagnosticClassificationFileName,
                StringComparison.Ordinal));

        var classificationPath = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimePlayerLoopReadinessVocabulary
                .DiagnosticClassificationFileName);
        using var classification = JsonDocument.Parse(await File.ReadAllTextAsync(classificationPath));
        Assert.True(Bool(classification.RootElement, "noUnclassifiedErrorDiagnostics"));
        Assert.True(Bool(classification.RootElement, "passAllowsNonBlockingDiagnostics"));

        if (unitySmoke is not null)
        {
            Assert.Equal("GREEN", write.Dashboard.Status);
            Assert.True(write.Dashboard.UnityPlayerLoopReadinessPassed);
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
            return CanonicalRuntimePlayerLoopReadinessVocabulary.ExportPackageDirectory;
        }

        return ".llmgc/exports/" + outputRoot[proceduralPrefix.Length..];
    }

    private static bool Bool(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static CanonicalRuntimePlayerLoopReadinessResult BuildRuntimeResult(
        string root,
        CanonicalRuntimePlayerLoopReadinessRequest request)
    {
        var transcriptPath = Path.Combine(root, request.TranscriptPath);
        var stateSummaryPath = Path.Combine(root, request.StateSummaryPath);
        var dashboardPath = Path.Combine(root, request.DashboardPath);
        var transcript =
            CanonicalRuntimePlayerLoopReadinessArtifactService.ReadTranscript(transcriptPath);
        var stateSummary =
            CanonicalRuntimePlayerLoopReadinessArtifactService.ReadStateSummary(stateSummaryPath);
        using var dashboard = JsonDocument.Parse(File.ReadAllText(dashboardPath));
        return new CanonicalRuntimePlayerLoopReadinessService().Build(
            transcript,
            stateSummary,
            request,
            saveLoadReplayStillReferenced: Bool(dashboard.RootElement, "saveLoadReplayPassed")
                                           && Bool(dashboard.RootElement, "stateHashChainPresent"),
            selectedCandidateExecutedByRuntime:
            Bool(dashboard.RootElement, "selectedCandidateExecutedByRuntime"));
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
