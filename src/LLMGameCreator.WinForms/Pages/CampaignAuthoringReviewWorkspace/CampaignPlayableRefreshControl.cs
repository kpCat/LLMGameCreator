using LLMGameCreator.Application.Design.EditDrivenPlayablePreviewRefresh;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignPlayableRefreshControl : UserControl
{
    private EditDrivenPlayablePreviewRefreshBuildResult? _result;

    public CampaignPlayableRefreshControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenPlayablePreviewRefreshBuildResult result)
    {
        _result = result;
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | rows=" + result.Report.ChangedRowCount
            + " | targets=" + result.Report.PackageTargetCount;
        _hashesListView.BeginUpdate();
        _hashesListView.Items.Clear();
        AddHash("sourceGoal075ReportHash", result.Report.SourceGoal075ReportHash);
        AddHash("beforeStateHash", result.Report.BeforeStateHash);
        AddHash("afterStateHash", result.Report.AfterStateHash);
        AddHash("rollbackStateHash", result.Report.RollbackStateHash);
        AddHash("replayStateHash", result.Report.ReplayStateHash);
        AddHash("previewRefreshHash", result.Report.PreviewRefreshHash);
        AddHash("handoffManifestHash", result.Report.HandoffManifestHash);
        _hashesListView.EndUpdate();

        _rowsListView.BeginUpdate();
        _rowsListView.Items.Clear();
        foreach (var row in result.StateTransitionProof.Rows)
        {
            var item = new ListViewItem(row.FamilyId);
            item.SubItems.Add(row.SeedId);
            item.SubItems.Add(row.RowId);
            item.SubItems.Add(row.StateChanged ? "changed" : "unchanged");
            item.SubItems.Add(row.RollbackRestored ? "rollback-ok" : "rollback-failed");
            item.SubItems.Add(row.ReplayRestoredAfter ? "replay-ok" : "replay-failed");
            item.SubItems.Add(row.PackageLogicalTargets.Count.ToString());
            item.SubItems.Add(row.AfterHash);
            _rowsListView.Items.Add(item);
        }

        _rowsListView.EndUpdate();
        _handoffLabel.Text = "Handoff: " + result.UnityPlayerHandoffManifest.ManifestRelativePath
            + " | hash=" + result.Report.HandoffManifestHash
            + " | staged=" + result.StagedHandoffProof.Passed
            + " | negative=" + result.TamperNegativeProof.Passed;
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void AddHash(string label, string value)
    {
        var item = new ListViewItem(label);
        item.SubItems.Add(value);
        _hashesListView.Items.Add(item);
    }

    private static string BuildDiagnostics(EditDrivenPlayablePreviewRefreshBuildResult result)
    {
        var lines = new List<string>
        {
            "qualityGatePassed=" + result.QualityGateScan.Passed,
            "winFormsBindingPassed=" + result.WinFormsBindingInventory.Passed,
            "tamperNegativeProofPassed=" + result.TamperNegativeProof.Passed,
            "fullMaterializationDisposition=" + result.GamePackageRefreshPlan.FullMaterializationDisposition
        };
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
