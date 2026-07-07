using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal137Tests
{
    [Fact]
    public async Task Goal137UnityPlayerLoopPlaybackSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new CanonicalRuntimeUnityPlayerLoopPlaybackArtifactService()
            .BuildAndWriteAsync(
                root,
                new CanonicalRuntimeUnityPlayerLoopPlaybackRequest(),
                unitySmoke: PassedUnitySmoke(root));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "canonical_runtime_unity_player_loop_playback");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "canonical_runtime_unity_player_loop_playback_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackGroupPresent);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId);
        Assert.Equal(13, workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackPassed);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime);
        Assert.Equal(
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.NormalCommand,
            workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand);
        Assert.Equal(
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackReportPath);
        Assert.Equal(
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultRelativePath,
            workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional);
        Assert.False(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            summary.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId);

        Assert.Contains(
            "## Canonical Runtime Unity Player Loop Playback",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "playbackFrameCount: 13",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "unityPlayerLoopPlaybackPassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke PassedUnitySmoke(string root)
    {
        var frames = Path.Combine(
            root,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName);
        var result = Path.Combine(
            root,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
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
