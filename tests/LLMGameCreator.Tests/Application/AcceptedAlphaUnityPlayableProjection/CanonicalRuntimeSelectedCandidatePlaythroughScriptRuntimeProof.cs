using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.AcceptedAlphaUnityPlayableProjection;

public sealed class CanonicalRuntimeSelectedCandidatePlaythroughScriptRuntimeProof
{
    [Fact]
    public async Task Goal134WritesCanonicalRuntimeSelectedCandidateArtifacts()
    {
        var root = ProjectRoot();
        var packagePath = ReadPathEnv(
            root,
            "LLMGC_GOAL134_SELECTED_CANDIDATE_PACKAGE_PATH",
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .DefaultSelectedCandidatePackagePath);
        var handoffPath = ReadPathEnv(
            root,
            "LLMGC_GOAL134_SELECTED_CANDIDATE_HANDOFF_PATH",
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .DefaultSelectedCandidateHandoffPath);
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
        var runtimeResult = CanonicalRuntimeSelectedCandidatePlaythroughService
            .CreateDefault()
            .Execute(package, request);
        var unitySmoke = ReadUnitySmokeOverride();

        var write = await new CanonicalRuntimeSelectedCandidatePlaythroughArtifactService()
            .BuildAndWriteAsync(root, package, request, runtimeResult, unitySmoke);

        Assert.True(runtimeResult.Passed, string.Join(Environment.NewLine, runtimeResult.Diagnostics));
        Assert.True(runtimeResult.SaveLoadReplay.Passed);
        Assert.True(write.Dashboard.PackageValidationPassed);
        Assert.True(write.Dashboard.CanonicalRuntimePassed);
        Assert.True(write.Dashboard.RuntimeCommandCount >= 6);
        Assert.True(write.Dashboard.RuntimeEventCount >= 6);
        Assert.True(write.Dashboard.StateHashChainPresent);
        Assert.True(write.Dashboard.SaveLoadReplayPassed);
        Assert.False(write.Dashboard.ProjectionOnly);
        Assert.True(write.Dashboard.SelectedCandidateExecutedByRuntime);
        Assert.Contains(write.WrittenFiles, path =>
            path == CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .ExportPackageDirectory
            + "/"
            + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportMarkdownFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.UnitySmokeFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DocumentationPath);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        if (unitySmoke != null)
        {
            Assert.True(write.Dashboard.UnityConsumedCanonicalTranscript);
            Assert.Equal("GREEN", write.Dashboard.Status);
        }
    }

    private static CanonicalRuntimeSelectedCandidateUnitySmoke? ReadUnitySmokeOverride()
    {
        var path = Environment.GetEnvironmentVariable("LLMGC_GOAL134_UNITY_SMOKE_PATH");
        return string.IsNullOrWhiteSpace(path) || !File.Exists(path)
            ? null
            : CanonicalRuntimeSelectedCandidatePlaythroughArtifactService.ReadUnitySmoke(path);
    }

    private static string ReadPathEnv(string root, string name, string fallbackRelative)
    {
        var value = Environment.GetEnvironmentVariable(name);
        var path = string.IsNullOrWhiteSpace(value) ? fallbackRelative : value;
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
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
