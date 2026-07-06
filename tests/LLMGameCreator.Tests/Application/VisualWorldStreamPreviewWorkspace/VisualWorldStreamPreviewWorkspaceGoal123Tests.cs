using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal123Tests
{
    [Fact]
    public async Task Goal123GenericGamePackageProjectionSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GenericGamePackageProjectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService()
            .Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "generic_gamepackage_projection");
        var summary = Assert.Single(
            group.Entries,
            entry => entry.ArtifactKind == "generic_gamepackage_projection_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageProjection);
        Assert.True(workspace.QualityGateScan.GenericGamePackageProjectionGroupPresent);
        Assert.True(workspace.QualityGateScan.GenericGamePackageProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal123FilesDiscoveredByRelativePaths);
        Assert.Equal("GREEN", summary.GenericProjectionStatus);
        Assert.Equal(
            GenericGamePackageProjectionVocabulary.SamplePackagePath,
            summary.GenericProjectionSamplePackagePath);
        Assert.Equal("game/minimal-map-game", summary.GenericProjectionPackageId);
        Assert.Equal("Minimal Map Game", summary.GenericProjectionPackageTitle);
        Assert.Equal("map/village", summary.GenericProjectionMapId);
        Assert.Equal("12x8", summary.GenericProjectionMapSize);
        Assert.True(summary.GenericProjectionEntityCount >= 2);
        Assert.True(summary.GenericProjectionItemCount >= 1);
        Assert.True(summary.GenericProjectionGoal122StillGreen);
        Assert.True(summary.GenericProjectionCleanupScriptAvailable);
        Assert.True(summary.GenericProjectionDoNotStartAutomatically);
        Assert.Equal(
            GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory,
            summary.GenericProjectionEvidencePath);
        Assert.Equal(
            GenericGamePackageProjectionVocabulary.ExportPackageDirectory,
            summary.GenericProjectionExportPath);
        Assert.Contains(
            "genericProjectionStatus: GREEN",
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
