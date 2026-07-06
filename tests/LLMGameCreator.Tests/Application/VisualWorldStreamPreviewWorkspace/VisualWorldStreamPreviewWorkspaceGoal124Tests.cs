using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal124Tests
{
    [Fact]
    public async Task Goal124GenericGamePackageLoopSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GenericGamePackageLoopProjectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService()
            .Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "generic_gamepackage_loop");
        var summary = Assert.Single(
            group.Entries,
            entry => entry.ArtifactKind == "generic_gamepackage_loop_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageLoop);
        Assert.True(workspace.QualityGateScan.GenericGamePackageLoopGroupPresent);
        Assert.True(workspace.QualityGateScan.GenericGamePackageLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal124FilesDiscoveredByRelativePaths);
        Assert.Equal("GREEN", summary.GenericLoopStatus);
        Assert.Equal(
            GenericGamePackageLoopProjectionVocabulary.SamplePackagePath,
            summary.GenericLoopSamplePackagePath);
        Assert.Equal("game/minimal-map-game", summary.GenericLoopPackageId);
        Assert.Equal("map/village", summary.GenericLoopMapId);
        Assert.True(summary.GenericLoopInteractionPreviewPresent);
        Assert.True(summary.GenericLoopInteractionApplyPassed);
        Assert.True(summary.GenericLoopDialogueSummaryPresent);
        Assert.True(summary.GenericLoopQuestObjectiveSummaryPresent);
        Assert.True(summary.GenericLoopInventorySummaryPresent);
        Assert.True(summary.GenericLoopResourceSummaryPresent);
        Assert.True(summary.GenericLoopGoal123StillGreen);
        Assert.True(summary.GenericLoopCleanupScriptAvailable);
        Assert.True(summary.GenericLoopProjectionOnly);
        Assert.Equal(1, summary.GenericLoopAppliedInteractionCount);
        Assert.Equal(1, summary.GenericLoopStartedQuestCount);
        Assert.Equal(
            GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory,
            summary.GenericLoopEvidencePath);
        Assert.Equal(
            GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory,
            summary.GenericLoopExportPath);
        Assert.Contains(
            "genericLoopStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "questObjectiveSummaryPresent: true",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
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
