using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal116Tests
{
    [Fact]
    public void Goal116ManualGateAcceptanceRecordSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_gate_acceptance_record");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "offline_geoworld_alpha_manual_gate_acceptance_record_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaManualGateAcceptanceGroupPresent);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaManualGateAcceptanceQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal116FilesDiscoveredByRelativePaths);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
            summary.OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus);
        Assert.True(summary.OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted);
        Assert.False(summary.OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted);
        Assert.False(summary
            .OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts);
        Assert.Equal(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256,
            summary.OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256);
        Assert.Contains(
            "offlineGeoworldAlphaManualGateAcceptanceManualGateStatus: "
            + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
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
