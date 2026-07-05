using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaAcceptanceOperatorPackProductSmokeTests
{
    [Fact]
    public async Task Goal112OfflineGeoworldAlphaAcceptanceOperatorPackProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldAlphaAcceptanceOperatorPackService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.QualityGateScan.ImplementationStatus);
        Assert.False(result.Dashboard.AcceptedByCodex);
        Assert.True(result.Dashboard.HumanAcceptanceStillRequired);
        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun,
            result.Dashboard.OperatorStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            result.Dashboard.DecisionStatusFromGoal111);
        Assert.False(result.Dashboard.ManualResultPresent);
        Assert.True(result.QualityGateScan.Passed, string.Join(Environment.NewLine, result.QualityGateScan.Diagnostics));
        Assert.True(result.NegativeProof.Passed);
        Assert.Contains(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreferredManualResultPath,
            result.ProceduralFiles[OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName],
            StringComparison.Ordinal);

        AssertFilesExist(
            write.ProceduralOutputDirectoryPath,
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RequiredProceduralFileNames);
        AssertFilesExist(
            write.ExportPackageDirectoryPath,
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RequiredExportFileNames);
        Assert.True(File.Exists(write.DocumentationRunbookPath));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_acceptance_operator_pack");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_acceptance_operator_workspace_summary");

        Assert.Equal(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.OperatorStatusReadyPendingHumanRun,
            summary.OfflineGeoworldAlphaAcceptanceOperatorStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            summary.OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus);
        Assert.False(summary.OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaAcceptanceOperatorPackGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal112FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "offlineGeoworldAlphaAcceptanceOperatorStatus: OPERATOR_READY_PENDING_HUMAN_RUN",
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
