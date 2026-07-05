using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal115Tests
{
    [Fact]
    public async Task Goal115HumanResultRevalidationGroupSurfacesManualGateStatus()
    {
        var root = ProjectRoot();
        var manualResultExists = File.Exists(Path.Combine(
            root,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ManualResultRelativePath
                .Replace('/', Path.DirectorySeparatorChar)));
        await new OfflineGeoworldAlphaHumanResultRevalidationService().BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_human_result_revalidation");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "offline_geoworld_alpha_human_result_revalidation_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAlphaHumanResultRevalidation);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaHumanResultRevalidationGroupPresent);
        Assert.False(summary.OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired);
        Assert.True(summary.OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision);
        Assert.True(summary.OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted);

        if (manualResultExists)
        {
            Assert.Equal(
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusGreenCandidate,
                summary.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus);
            Assert.Equal(
                OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusGreenCandidate,
                summary.OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus);
            Assert.True(summary.OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate);
            Assert.True(workspace.QualityGateScan
                .OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed);
            Assert.True(workspace.QualityGateScan.Goal115FilesDiscoveredByRelativePaths);
            Assert.Contains(
                "offlineGeoworldAlphaHumanResultRevalidationDecisionStatus: "
                + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusGreenCandidate,
                workspace.ReportMarkdown,
                StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionStatusPending,
                summary.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus);
            Assert.False(summary.OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate);
            Assert.False(workspace.QualityGateScan
                .OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed);
        }
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
