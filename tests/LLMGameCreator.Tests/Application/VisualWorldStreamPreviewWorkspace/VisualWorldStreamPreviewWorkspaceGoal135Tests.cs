using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal135Tests
{
    [Fact]
    public async Task Goal135CanonicalRuntimePlayerLoopSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        var request = new CanonicalRuntimePlayerLoopReadinessRequest
        {
            TranscriptPath = CanonicalRuntimePlayerLoopReadinessVocabulary
                .DefaultCanonicalRuntimeTranscriptPath,
            StateSummaryPath = CanonicalRuntimePlayerLoopReadinessVocabulary
                .DefaultCanonicalRuntimeStateSummaryPath,
            DashboardPath = CanonicalRuntimePlayerLoopReadinessVocabulary
                .DefaultCanonicalRuntimeDashboardPath
        };
        await new CanonicalRuntimePlayerLoopReadinessArtifactService()
            .BuildAndWriteAsync(
                root,
                request,
                BuildRuntimeResult(root, request),
                unitySmoke: PassedUnitySmoke(root));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "canonical_runtime_player_loop_readiness");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "canonical_runtime_player_loop_readiness_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysCanonicalRuntimePlayerLoopReadiness);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopGroupPresent);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            workspace.QualityGateScan.CanonicalRuntimePlayerLoopCandidateId);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopAdapterContractPresent);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopStepCount >= 13);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopRequiredCategoriesPresent);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopUnityReadinessPassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopSource);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimePlayerLoopUnityGameplayTruth);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimePlayerLoopProjectionOnly);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopNoUnclassifiedErrors);
        Assert.Equal(
            CanonicalRuntimePlayerLoopReadinessVocabulary.NormalCommand,
            workspace.QualityGateScan.CanonicalRuntimePlayerLoopNormalCommand);
        Assert.Equal(
            CanonicalRuntimePlayerLoopReadinessVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.CanonicalRuntimePlayerLoopReportPath);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopManualUnityOptional);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimePlayerLoopQualityGatePassed);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            summary.CanonicalRuntimePlayerLoopCandidateId);

        Assert.Contains(
            "## Canonical Runtime Player Loop Readiness",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "canonicalRuntimeSource: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "unityGameplayTruth: false",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "noUnclassifiedErrorDiagnostics: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static CanonicalRuntimePlayerLoopUnitySmoke PassedUnitySmoke(string root)
    {
        var plan = Path.Combine(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName);
        var state = Path.Combine(
            root,
            CanonicalRuntimePlayerLoopReadinessVocabulary
                .DefaultCanonicalRuntimeStateSummaryPath);
        return new CanonicalRuntimePlayerLoopUnitySmoke
        {
            UnityAvailable = true,
            PlanPathExists = true,
            StateSummaryPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            RequiredStepCategoriesPresent = true,
            CanonicalAuthorityMarkersPresent = true,
            UnityPlayerLoopReadinessPassed = true,
            Passed = true,
            UnityPath = "test-unity",
            PlanPath = Path.GetRelativePath(root, plan).Replace('\\', '/'),
            StateSummaryPath = Path.GetRelativePath(root, state).Replace('\\', '/'),
            Status = "GREEN"
        };
    }

    private static CanonicalRuntimePlayerLoopReadinessResult BuildRuntimeResult(
        string root,
        CanonicalRuntimePlayerLoopReadinessRequest request)
    {
        var transcript =
            CanonicalRuntimePlayerLoopReadinessArtifactService.ReadTranscript(
                Path.Combine(root, request.TranscriptPath));
        var stateSummary =
            CanonicalRuntimePlayerLoopReadinessArtifactService.ReadStateSummary(
                Path.Combine(root, request.StateSummaryPath));
        return new CanonicalRuntimePlayerLoopReadinessService().Build(
            transcript,
            stateSummary,
            request,
            saveLoadReplayStillReferenced: true,
            selectedCandidateExecutedByRuntime: true);
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
