using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

[Collection("UnityAlphaProductSmoke")]
public sealed class VisualWorldStreamPreviewWorkspaceGoal139Tests
{
    [Fact]
    public async Task Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService()
            .BuildAndWriteAsync(
                root,
                new RuntimeBackedUnityPlayerLoopInteractiveControlsRequest(),
                unitySmoke: PassedUnitySmoke(root));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "runtime_backed_unity_player_loop_interactive_controls");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind
            == "runtime_backed_unity_player_loop_interactive_controls_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysRuntimeBackedUnityPlayerLoopInteractiveControls);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsGroupPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId);
        Assert.Equal(13, workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly);
        Assert.Equal(
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.NormalCommand,
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand);
        Assert.Equal(
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsFilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            summary.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId);

        Assert.Contains(
            "## Runtime-backed Unity Player Loop Interactive Controls",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedGoal138: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "unityInteractiveControlsSmokePassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke PassedUnitySmoke(
        string root)
    {
        var model = Path.Combine(
            root,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName);
        var script = Path.Combine(
            root,
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory,
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
