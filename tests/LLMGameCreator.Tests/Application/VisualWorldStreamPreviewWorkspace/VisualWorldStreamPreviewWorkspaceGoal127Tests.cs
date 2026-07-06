using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal127Tests
{
    [Fact]
    public async Task Goal127UnityProjectionVerificationRunnerSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new UnityProjectionVerificationRunnerService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysUnityProjectionVerificationRunner);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "unity_projection_verification_runner");
        Assert.Contains(workspace.Catalog.Groups
                .Single(group => group.GroupId == "unity_projection_verification_runner")
                .Entries,
            entry => entry.ArtifactKind == "unity_projection_verification_runner_workspace_summary");
        Assert.True(workspace.QualityGateScan.UnityProjectionVerificationRunnerGroupPresent);
        Assert.Equal(
            UnityProjectionVerificationRunnerVocabulary.Mode,
            workspace.QualityGateScan.UnityProjectionVerificationRunnerMode);
        Assert.Equal(
            UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExecuteMethod,
            workspace.QualityGateScan.UnityProjectionVerificationRunnerExecuteMethod);
        Assert.Equal(
            UnityProjectionVerificationRunnerVocabulary.ResultRelativePath,
            workspace.QualityGateScan.UnityProjectionVerificationRunnerResultPath);
        Assert.Equal(
            UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath,
            workspace.QualityGateScan.UnityProjectionVerificationRunnerLogPath);
        Assert.Equal(
            ".devflow\\scripts\\run-unity-projection-verification.cmd",
            workspace.QualityGateScan.UnityProjectionVerificationRunnerCommand);
        Assert.False(workspace.QualityGateScan.UnityProjectionVerificationRunnerManualUnityClickingRequired);
        Assert.True(workspace.QualityGateScan.Goal127FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "runnerCommand: .devflow\\scripts\\run-unity-projection-verification.cmd",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "manualUnityClickingRequired: false",
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
