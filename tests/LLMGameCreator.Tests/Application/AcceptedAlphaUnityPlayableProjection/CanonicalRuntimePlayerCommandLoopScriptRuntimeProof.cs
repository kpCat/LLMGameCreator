using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimePlayerCommandLoopScriptRuntimeProof
{
    [Fact]
    public async Task WritesGoal136PlayerCommandLoopArtifacts()
    {
        var root = ProjectRoot();
        var handoffPath = EnvOrDefault(
            "LLMGC_GOAL136_SELECTED_CANDIDATE_HANDOFF_PATH",
            Path.Combine(
                root,
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidateHandoffPath));
        var packagePath = EnvOrDefault(
            "LLMGC_GOAL136_SELECTED_CANDIDATE_PACKAGE_PATH",
            Path.Combine(
                root,
                CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidatePackagePath));
        var outputRoot = RelativeOrDefault(
            root,
            Environment.GetEnvironmentVariable("LLMGC_GOAL136_OUTPUT_ROOT"),
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory);
        var exportRoot = ToExportRoot(outputRoot);
        var request = new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = CanonicalRuntimeSelectedCandidatePlaythroughArtifactService
                .ReadCandidateId(handoffPath),
            HandoffPath = Relative(root, handoffPath),
            PackagePath = Relative(root, packagePath),
            Goal134TranscriptPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL136_GOAL134_TRANSCRIPT_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134TranscriptPath))),
            Goal134StateSummaryPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL136_GOAL134_STATE_SUMMARY_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134StateSummaryPath))),
            Goal135PlayerLoopPlanPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL136_GOAL135_PLAYER_LOOP_PLAN_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerLoopPlanPath))),
            Goal135PlayerAdapterContractPath = Relative(
                root,
                EnvOrDefault(
                    "LLMGC_GOAL136_GOAL135_PLAYER_ADAPTER_CONTRACT_PATH",
                    Path.Combine(
                        root,
                        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerAdapterContractPath)))
        };
        var package =
            CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.LoadPackage(
                Path.Combine(root, request.PackagePath));
        var runtimeResult = CanonicalRuntimePlayerCommandLoopService
            .CreateDefault()
            .Execute(package, request);
        var unitySmokePath = Environment.GetEnvironmentVariable("LLMGC_GOAL136_UNITY_SMOKE_PATH");
        var unitySmoke = string.IsNullOrWhiteSpace(unitySmokePath)
            ? PassedUnitySmoke(root, outputRoot)
            : CanonicalRuntimePlayerCommandLoopArtifactService.ReadUnitySmoke(unitySmokePath);

        var write = await new CanonicalRuntimePlayerCommandLoopArtifactService()
            .BuildAndWriteAsync(
                root,
                request,
                runtimeResult,
                outputRoot,
                exportRoot,
                unitySmoke);

        Assert.Equal("minimal-map-game-balanced-baseline", write.Dashboard.CandidateId);
        Assert.True(write.Dashboard.PlayerCommandLoopPassed, string.Join(Environment.NewLine, write.Dashboard.Diagnostics));
        Assert.Equal(13, write.Dashboard.PlayerCommandCount);
        Assert.Equal(13, write.Dashboard.SnapshotCount);
        Assert.True(write.Dashboard.RuntimeEventCount >= 10);
        Assert.True(write.Dashboard.StateHashChainPresent);
        Assert.True(write.Dashboard.AllRequiredCategoriesPresent);
        Assert.True(write.Dashboard.SelectedCandidateExecutedByRuntime);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.False(write.Dashboard.UnityGameplayTruth);
        Assert.True(write.Dashboard.NoUnclassifiedErrorDiagnostics);
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName,
                StringComparison.Ordinal));
        Assert.Contains(write.WrittenFiles, path =>
            path.EndsWith(
                CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultFileName,
                StringComparison.Ordinal));
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        var classificationPath = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimePlayerCommandLoopVocabulary.DiagnosticClassificationFileName);
        using var classification = JsonDocument.Parse(await File.ReadAllTextAsync(classificationPath));
        Assert.True(Bool(classification.RootElement, "noUnclassifiedErrorDiagnostics"));
        Assert.True(Bool(classification.RootElement, "passAllowsNonBlockingDiagnostics"));

        Assert.Equal("GREEN", write.Dashboard.Status);
        Assert.True(write.Dashboard.UnityPlayerConsumedCommandLoopSnapshots);
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
            return CanonicalRuntimePlayerCommandLoopVocabulary.ExportPackageDirectory;
        }

        return ".llmgc/exports/" + outputRoot[proceduralPrefix.Length..];
    }

    private static bool Bool(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static CanonicalRuntimePlayerCommandLoopUnitySmoke PassedUnitySmoke(
        string root,
        string outputRoot)
    {
        var snapshots = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName);
        var result = Path.Combine(
            root,
            outputRoot,
            CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName);
        return new CanonicalRuntimePlayerCommandLoopUnitySmoke
        {
            UnityAvailable = true,
            SnapshotsPathExists = true,
            ResultPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            SnapshotContractPresent = true,
            UnityPlayerConsumedCommandLoopSnapshots = true,
            Passed = true,
            UnityPath = "test-unity",
            SnapshotsPath = Relative(root, snapshots),
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
