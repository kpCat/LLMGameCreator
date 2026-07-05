using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaManualResultWorkbenchProductSmokeTests
{
    [Fact]
    public async Task Goal113OfflineGeoworldAlphaManualResultWorkbenchProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var preferredManualResultPath = Path.Combine(
            repoRoot,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.PreferredManualResultPath
                .Replace('/', Path.DirectorySeparatorChar));
        var existedBefore = File.Exists(preferredManualResultPath);

        var write = await new OfflineGeoworldAlphaManualResultWorkbenchService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult,
            result.Dashboard.WorkbenchStatus);
        Assert.False(result.Dashboard.ManualResultPresent);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.True(result.Dashboard.HumanAcceptanceStillRequired);
        Assert.True(result.Dashboard.DoesNotWritePreferredManualResultPath);
        Assert.True(result.Dashboard.DraftTemplateOnly);
        Assert.True(result.Dashboard.NoUnityFileChangesRequired);
        Assert.True(result.Dashboard.NoRuntimeProviderOrNetworkChanges);
        Assert.Equal(12, result.Dashboard.ChecklistStepCount);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            result.Dashboard.Goal111DecisionStatus);
        Assert.True(result.QualityGateScan.Passed, string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.NegativeNoResultProof.Passed);
        Assert.True(result.NegativeInvalidResultProof.Passed);

        AssertFilesExist(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RequiredProceduralFileNames);
        AssertFilesExist(
            write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RequiredExportFileNames);
        Assert.True(File.Exists(write.DocumentationPath));
        Assert.All(write.WrittenFiles, path =>
            Assert.False(path.StartsWith(".llmgc/manual/", StringComparison.Ordinal), path));
        Assert.DoesNotContain(
            write.WrittenFiles,
            path => path.StartsWith("unity/", StringComparison.Ordinal));
        Assert.Equal(existedBefore, File.Exists(preferredManualResultPath));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_result_workbench");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_manual_result_workbench_workspace_summary");

        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult,
            summary.OfflineGeoworldAlphaManualResultWorkbenchStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            summary.OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus);
        Assert.False(summary.OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent);
        Assert.False(summary.OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired);
        Assert.True(summary.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultWorkbenchGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal113FilesDiscoveredByRelativePaths);
        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysOfflineGeoworldAlphaManualResultWorkbench);
        Assert.Contains(
            "offlineGeoworldAlphaManualResultWorkbenchStatus: WORKBENCH_READY_PENDING_HUMAN_RESULT",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static void AssertFilesExist(string directory, IReadOnlyList<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
