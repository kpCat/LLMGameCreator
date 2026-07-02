using LLMGameCreator.Application.Design.EditDrivenSpineQualityConsolidation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignEditDrivenSpineQualityControl : UserControl
{
    private EditDrivenSpineQualityConsolidationBuildResult? _result;

    public CampaignEditDrivenSpineQualityControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenSpineQualityConsolidationBuildResult result)
    {
        _result = result;
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | blockers=" + result.Report.BlockerCount
            + " | P2/P3=" + result.Report.P2Count + "/" + result.Report.P3Count;
        BindChain(result);
        BindDebt(result);
        _proofLabel.Text = "packageRead=" + result.AcceptanceReadinessDashboard.PackageReadProofPassed
            + " | replay=" + result.AcceptanceReadinessDashboard.ReplayProofPassed
            + " | negative=" + result.NegativeProofIndex.Passed
            + " | sourceHealth=" + result.SourceHealthScan.Passed
            + " | binding=" + result.WorkspaceBindingInventory.Passed;
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void BindChain(EditDrivenSpineQualityConsolidationBuildResult result)
    {
        _chainListView.BeginUpdate();
        _chainListView.Items.Clear();
        foreach (var item in result.SpineChainManifest.ChainItems)
        {
            var row = new ListViewItem("Goal " + item.GoalNumber);
            row.SubItems.Add(item.ImplementationStatus);
            row.SubItems.Add(item.Accepted);
            row.SubItems.Add(item.ReportHash);
            _chainListView.Items.Add(row);
        }

        _chainListView.EndUpdate();
    }

    private void BindDebt(EditDrivenSpineQualityConsolidationBuildResult result)
    {
        _debtListView.BeginUpdate();
        _debtListView.Items.Clear();
        foreach (var item in result.QualityDebtClassification.Debts)
        {
            var row = new ListViewItem(item.Severity);
            row.SubItems.Add(item.FindingId);
            row.SubItems.Add(item.Area);
            row.SubItems.Add(item.Evidence);
            _debtListView.Items.Add(row);
        }

        _debtListView.EndUpdate();
    }

    private static string BuildDiagnostics(EditDrivenSpineQualityConsolidationBuildResult result)
    {
        var lines = new List<string>
        {
            "chainItemCount=" + result.SpineChainManifest.ChainItemCount,
            "negativeScenarioCount=" + result.NegativeProofIndex.ScenarioCount,
            "p0Count=" + result.QualityDebtClassification.P0Count,
            "p1Count=" + result.QualityDebtClassification.P1Count,
            "p2Count=" + result.QualityDebtClassification.P2Count,
            "p3Count=" + result.QualityDebtClassification.P3Count,
            "parentWorkspaceLineCount=" + result.SourceHealthScan.ParentWorkspaceLineCount,
            "maxCSharpLineLength=" + result.SourceHealthScan.MaxLineLength,
            "logicalMaxLineLength=" + result.SourceHealthScan.LogicalMaxLineLength,
            "zeroLfSourceFileCount=" + result.SourceHealthScan.ZeroLfSourceFileCount,
            "crOnlySourceFileCount=" + result.SourceHealthScan.CrOnlySourceFileCount,
            "rawPhysicalMaxLineLength=" + result.SourceHealthScan.RawPhysicalMaxLineLength,
            "rawPhysicalOneLineSourceFileCount=" + result.SourceHealthScan.RawPhysicalOneLineSourceFileCount,
            "alphaRuntimeBootstrapLineCount=" + result.SourceHealthScan.AlphaRuntimeBootstrapLineCount,
            "alphaRuntimeBootstrapHash=" + result.SourceHealthScan.AlphaRuntimeBootstrapHash
        };
        lines.AddRange(result.NegativeProofIndex.Scenarios.Select(item =>
            "negative:" + item.ScenarioId + "=" + item.ActualStatus
            + " diagnostics=" + item.DiagnosticCount));
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
