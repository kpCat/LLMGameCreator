using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal134Tests
{
    [Fact]
    public async Task Goal134CanonicalRuntimeSelectedCandidateSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        var handoffPath = Path.Combine(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .DefaultSelectedCandidateHandoffPath);
        var packagePath = Path.Combine(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                .DefaultSelectedCandidatePackagePath);
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
        await new CanonicalRuntimeSelectedCandidatePlaythroughArtifactService()
            .BuildAndWriteAsync(root, package, request, runtimeResult, PassedUnitySmoke(root));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "canonical_runtime_selected_candidate_playthrough");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "canonical_runtime_selected_candidate_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysCanonicalRuntimeSelectedCandidate);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeSelectedCandidateGroupPresent);
        Assert.Equal(candidateId, workspace.QualityGateScan.CanonicalRuntimeCandidateId);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePackageValidationPassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeCommandCount >= 6);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeEventCount >= 6);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeSaveLoadReplayPassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerConsumedTranscript);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimeProjectionOnly);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeSelectedCandidateExecutedByRuntime);
        Assert.Equal(
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.NormalCommand,
            workspace.QualityGateScan.CanonicalRuntimeNormalCommand);
        Assert.Equal(
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.CanonicalRuntimeReportPath);
        Assert.Equal(
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.MatrixResultRelativePath,
            workspace.QualityGateScan.CanonicalRuntimeMatrixResultPath);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeManualUnityOptional);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeQualityGatePassed);
        Assert.Equal(candidateId, summary.CanonicalRuntimeCandidateId);

        Assert.Contains(
            "## Canonical Runtime Selected Candidate Playthrough",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "projectionOnly: false",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "selectedCandidateExecutedByRuntime: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static CanonicalRuntimeSelectedCandidateUnitySmoke PassedUnitySmoke(string root)
    {
        var transcript = Path.Combine(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.TranscriptFileName);
        var state = Path.Combine(
            root,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateSummaryFileName);
        return new CanonicalRuntimeSelectedCandidateUnitySmoke
        {
            UnityAvailable = true,
            TranscriptPathExists = true,
            StateSummaryPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            UnityPlayerConsumedCanonicalTranscript = true,
            Passed = true,
            UnityPath = "test-unity",
            TranscriptPath = Path.GetRelativePath(root, transcript).Replace('\\', '/'),
            StateSummaryPath = Path.GetRelativePath(root, state).Replace('\\', '/'),
            Status = "GREEN"
        };
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
