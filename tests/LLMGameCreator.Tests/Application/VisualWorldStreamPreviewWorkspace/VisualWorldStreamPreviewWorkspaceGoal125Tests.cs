using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal125Tests
{
    [Fact]
    public async Task Goal125GenericGamePackageSystemsSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GenericGamePackageSystemsProjectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysGenericGamePackageSystems);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_systems_loop");
        Assert.Contains(workspace.Catalog.Groups
                .Single(group => group.GroupId == "generic_gamepackage_systems_loop")
                .Entries,
            entry => entry.ArtifactKind == "generic_gamepackage_systems_workspace_summary");
        Assert.True(workspace.QualityGateScan.GenericGamePackageSystemsGroupPresent);
        Assert.Equal("GREEN", workspace.QualityGateScan.GenericSystemsStatus);
        Assert.True(workspace.QualityGateScan.GenericSystemsRecipePreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsRecipeApplyPassed);
        Assert.True(workspace.QualityGateScan.GenericSystemsHarvestPreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsHarvestApplyPassed);
        Assert.True(workspace.QualityGateScan.GenericSystemsTransactionPreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsEncounterPreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsCombatRoundPreviewPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsInventorySummaryPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsResourceSummaryPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsEventLogPresent);
        Assert.True(workspace.QualityGateScan.GenericSystemsGoal124StillGreen);
        Assert.True(workspace.QualityGateScan.GenericSystemsSamplePackageReadOnly);
        Assert.True(workspace.QualityGateScan.GenericSystemsProjectionOnly);
        Assert.True(workspace.QualityGateScan.GenericGamePackageSystemsQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal125FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.WinFormsGenericGamePackageSystemsBindingReal);
        Assert.Contains(
            "genericSystemsStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericGamePackageSystemsQualityGatePassed: true",
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
