using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal112Tests
{
    [Fact]
    public async Task Goal112OperatorPackGroupSurfacesPendingHumanRun()
    {
        var root = ProjectRoot();
        await new OfflineGeoworldAlphaAcceptanceOperatorPackService().BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
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
        Assert.False(summary.OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent);
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
