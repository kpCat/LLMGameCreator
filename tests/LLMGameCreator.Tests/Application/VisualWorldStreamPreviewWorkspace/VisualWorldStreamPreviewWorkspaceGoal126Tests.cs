using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal126Tests
{
    [Fact]
    public async Task Goal126GenericGamePackageFullPlaythroughSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GenericGamePackageFullPlaythroughProjectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysGenericGamePackageFullPlaythrough);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_full_playthrough");
        Assert.Contains(workspace.Catalog.Groups
                .Single(group => group.GroupId == "generic_gamepackage_full_playthrough")
                .Entries,
            entry => entry.ArtifactKind == "generic_gamepackage_full_playthrough_workspace_summary");
        Assert.True(workspace.QualityGateScan.GenericGamePackageFullPlaythroughGroupPresent);
        Assert.Equal("GREEN", workspace.QualityGateScan.GenericFullPlaythroughStatus);
        Assert.Equal("game/minimal-map-game", workspace.QualityGateScan.GenericFullPlaythroughPackageId);
        Assert.Equal("map/village", workspace.QualityGateScan.GenericFullPlaythroughMapId);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughMapPathPreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughSignInteractionApplied);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughDialogueSummaryPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughQuestObjectiveStatusPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughInventorySummaryPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughResourceSummaryPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughSystemsSummaryPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughCombatRoundPreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughEventTranscriptPresent);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughGoal125StillGreen);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughSamplePackageReadOnly);
        Assert.True(workspace.QualityGateScan.GenericFullPlaythroughProjectionOnly);
        Assert.True(workspace.QualityGateScan.GenericGamePackageFullPlaythroughQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal126FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.WinFormsGenericGamePackageFullPlaythroughBindingReal);
        Assert.Contains(
            "fullPlaythroughStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericGamePackageFullPlaythroughQualityGatePassed: true",
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
