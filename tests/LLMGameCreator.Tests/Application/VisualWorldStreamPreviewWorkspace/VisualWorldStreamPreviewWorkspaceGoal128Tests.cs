using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal128Tests
{
    [Fact]
    public async Task Goal128ParameterizedGamePackageRunnerSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new ParameterizedGamePackageProjectionRunnerService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysParameterizedGamePackageRunner);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "parameterized_gamepackage_projection_runner");
        Assert.Contains(workspace.Catalog.Groups
                .Single(group => group.GroupId == "parameterized_gamepackage_projection_runner")
                .Entries,
            entry => entry.ArtifactKind == "parameterized_gamepackage_runner_workspace_summary");
        Assert.True(workspace.QualityGateScan.ParameterizedGamePackageRunnerGroupPresent);
        Assert.Equal(
            ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath,
            workspace.QualityGateScan.ParameterizedGamePackageRunnerPackagePathRelative);
        Assert.Equal(
            ParameterizedGamePackageProjectionRunnerVocabulary.NormalCommand,
            workspace.QualityGateScan.ParameterizedGamePackageRunnerNormalCommand);
        Assert.Equal(
            ParameterizedGamePackageProjectionRunnerVocabulary.ExampleCommandWithPackagePath,
            workspace.QualityGateScan.ParameterizedGamePackageRunnerExampleCommandWithPackagePath);
        Assert.Equal(
            ParameterizedGamePackageProjectionRunnerVocabulary.ResultRelativePath,
            workspace.QualityGateScan.ParameterizedGamePackageRunnerResultPath);
        Assert.Equal(
            ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath,
            workspace.QualityGateScan.ParameterizedGamePackageRunnerLogPath);
        Assert.True(workspace.QualityGateScan.ParameterizedGamePackageRunnerManualUnityOptional);
        Assert.True(workspace.QualityGateScan.ParameterizedGamePackageRunnerProjectionOnly);
        Assert.True(workspace.QualityGateScan.Goal128FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "normalCommand: .devflow\\scripts\\run-unity-projection-verification.cmd",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "packagePathRelative: samples/minimal-map-game/package.json",
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
