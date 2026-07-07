using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

[Collection("UnityAlphaProductSmoke")]
public sealed class VisualWorldStreamPreviewWorkspaceGoal138Tests
{
    [Fact]
    public async Task Goal138RuntimeBackedUnityPlayerLoopStepperSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new RuntimeBackedUnityPlayerLoopStepperArtifactService()
            .BuildAndWriteAsync(
                root,
                new RuntimeBackedUnityPlayerLoopStepperRequest(),
                unitySmoke: PassedUnitySmoke(root));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "runtime_backed_unity_player_loop_stepper");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "runtime_backed_unity_player_loop_stepper_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysRuntimeBackedUnityPlayerLoopStepper);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperGroupPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperCandidateId);
        Assert.Equal(13, workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperFrameCount);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperProjectionOnly);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperWindowPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed);
        Assert.Equal(
            RuntimeBackedUnityPlayerLoopStepperVocabulary.NormalCommand,
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperNormalCommand);
        Assert.Equal(
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperReportPath);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperManualUnityOptional);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperAccepted);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperFilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopStepperQualityGatePassed);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            summary.RuntimeBackedUnityPlayerLoopStepperCandidateId);

        Assert.Contains(
            "## Runtime-backed Unity Player Loop Stepper",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedGoal137: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "stepperBatchSmokePassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static RuntimeBackedUnityPlayerLoopStepperUnitySmoke PassedUnitySmoke(string root)
    {
        var model = Path.Combine(
            root,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName);
        return new RuntimeBackedUnityPlayerLoopStepperUnitySmoke
        {
            UnityAvailable = true,
            ModelPathExists = true,
            PassMarkerPresent = true,
            FailMarkerPresent = false,
            FrameCountPassed = true,
            RequiredFrameCategoriesPresent = true,
            RuntimeAuthorityMarkersPresent = true,
            StepperWindowPresent = true,
            StepperBatchSmokePassed = true,
            Passed = true,
            UnityPath = "test-unity",
            ModelPath = Relative(root, model),
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
