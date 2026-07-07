using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

[Collection("UnityAlphaProductSmoke")]
public sealed class VisualWorldStreamPreviewWorkspaceGoal140Tests
{
    [Fact]
    public async Task Goal140RuntimeBackedUnityPlayerLoopControlsUxSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new RuntimeBackedUnityPlayerLoopInteractiveControlsArtifactService()
            .BuildAndWriteAsync(
                root,
                new RuntimeBackedUnityPlayerLoopInteractiveControlsRequest(),
                unitySmoke: PassedGoal139UnitySmoke(root));

        await new RuntimeBackedUnityPlayerLoopControlsUxPolishArtifactService()
            .BuildAndWriteAsync(
                root,
                new RuntimeBackedUnityPlayerLoopControlsUxPolishRequest(),
                unitySmoke: PassedGoal140UnitySmoke(root),
                unityNoise: PassedGoal140Noise());

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "runtime_backed_unity_player_loop_controls_ux");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "runtime_backed_unity_player_loop_controls_ux_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysRuntimeBackedUnityPlayerLoopControlsUx);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxGroupPresent);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate);
        Assert.Equal(13, workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxFrameCount);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified);
        Assert.Equal(0, workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount);
        Assert.Equal(0, workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly);
        Assert.False(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxAccepted);
        Assert.Equal(
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NormalCommand,
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxNormalCommand);
        Assert.Equal(
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportMarkdownRelativePath,
            workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxReportPath);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxFilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.RuntimeBackedUnityPlayerLoopControlsUxQualityGatePassed);
        Assert.Equal(
            "minimal-map-game-balanced-baseline",
            summary.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate);

        Assert.Contains(
            "## Runtime-backed Unity Player Loop Controls UX",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedGoal139: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "unityControlsUxSmokePassed: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "knownUnityEditorNoiseClassified: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke
        PassedGoal139UnitySmoke(string root)
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

    private static RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke
        PassedGoal140UnitySmoke(string root)
    {
        var model = Path.Combine(
            root,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName);
        var script = Path.Combine(
            root,
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory,
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

    private static RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification PassedGoal140Noise() =>
        new()
        {
            KnownUnityEditorBuildProfileNoiseClassified = true,
            KnownUnityEditorNoiseCount = 0,
            BlockingUnityErrorCount = 0,
            UnclassifiedUnityErrorCount = 0,
            FixtureKnownUnityEditorBuildProfileNoiseClassified = true,
            SourceLogPath = "test-unity.log",
            KnownMarkers = ["BuildProfileContext", "CreateOrLoad", "NullReferenceException"],
            BlockingMarkers = [],
            Diagnostics = ["known fixture classified; no blocking Unity errors"],
            Passed = true
        };

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
