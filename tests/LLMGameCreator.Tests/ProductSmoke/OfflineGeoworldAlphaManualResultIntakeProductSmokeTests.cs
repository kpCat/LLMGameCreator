using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaManualResultIntakeProductSmokeTests
{
    [Fact]
    public async Task Goal111OfflineGeoworldAlphaManualResultIntakeProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldAlphaManualResultIntakeService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            result.Decision.DecisionStatus);
        Assert.False(result.Decision.AcceptableCandidate);
        Assert.False(result.Decision.AcceptedByCodex);
        Assert.True(result.Decision.HumanAcceptanceStillRequired);
        Assert.True(result.Decision.InputPackageLineage.Goal110ExportPackagePresent);
        Assert.True(result.Decision.InputPackageLineage.Goal110ProceduralEvidencePresent);
        Assert.True(result.Decision.InputPackageLineage.Goal110StreamingAssetsPresent);
        Assert.True(result.Decision.InputPackageLineage.ChecklistRead);
        Assert.True(result.Decision.InputPackageLineage.ResultTemplateRead);
        Assert.True(result.Decision.InputPackageLineage.ManifestRead);
        Assert.Equal(12, result.Decision.StepSummary.RequiredStepCount);
        Assert.True(result.QualityGateScan.Passed, string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.MissingResultProof.Passed);
        Assert.True(result.InvalidResultProof.Passed);

        AssertFilesExist(write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaManualResultIntakeVocabulary.RequiredProceduralFileNames);
        AssertFilesExist(write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaManualResultIntakeVocabulary.RequiredExportFileNames);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_result_intake");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_manual_result_intake_workspace_summary");
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            summary.OfflineGeoworldAlphaManualResultIntakeDecisionStatus);
        Assert.True(summary.OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent);
        Assert.False(summary.OfflineGeoworldAlphaManualResultIntakeResultFilePresent);
        Assert.False(summary.OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultIntakeGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultIntakeQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal111FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "offlineGeoworldAlphaManualResultIntakeDecisionStatus: BLOCKED_PENDING_MANUAL_RESULT",
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
