using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignGamePackageRuntimePreviewBridgeControl : UserControl
{
    public CampaignGamePackageRuntimePreviewBridgeControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenGamePackageRuntimePreviewBridgeBuildResult result)
    {
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | rows=" + result.Report.RowCount
            + " | targets=" + result.Report.TargetCount
            + " | actions=" + result.Report.ActionCount;
        BindHashes(result);
        BindFiles(result);
        _proofLabel.Text = "packageRead=" + result.RuntimePreviewBridgeProof.ProjectedPackagePayloadRead
            + " | validation=" + result.RuntimePreviewBridgeProof.GamePackageValidationPassed
            + " | preview=" + result.RuntimePreviewBridgeProof.RuntimePreviewProjectionPassed
            + " | interactions=" + result.RuntimePreviewBridgeProof.InteractionCatalogProjectionPassed
            + " | negative=" + result.RuntimePreviewNegativeProof.Passed;
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void BindHashes(EditDrivenGamePackageRuntimePreviewBridgeBuildResult result)
    {
        _hashesListView.BeginUpdate();
        _hashesListView.Items.Clear();
        AddHash("sourceGoal077ReportHash", result.Report.SourceGoal077ReportHash);
        AddHash("sourceGoal078ReportHash", result.Report.SourceGoal078ReportHash);
        AddHash("sourceGoal079ReportHash", result.Report.SourceGoal079ReportHash);
        AddHash("sourceGoal079AReportHash", result.Report.SourceGoal079AReportHash);
        AddHash("projectedPackageHash", result.Report.ProjectedPackageHash);
        AddHash("projectedPackageFileLedgerHash", result.Report.ProjectedPackageFileLedgerHash);
        AddHash("runtimePreviewBridgeProofHash", result.Report.RuntimePreviewBridgeProofHash);
        _hashesListView.EndUpdate();
    }

    private void BindFiles(EditDrivenGamePackageRuntimePreviewBridgeBuildResult result)
    {
        _filesListView.BeginUpdate();
        _filesListView.Items.Clear();
        foreach (var file in result.ProjectedPackageFileLedger.Files)
        {
            var item = new ListViewItem(file.Role);
            item.SubItems.Add(file.RelativePath);
            item.SubItems.Add(file.Sha256);
            _filesListView.Items.Add(item);
        }

        _filesListView.EndUpdate();
    }

    private void AddHash(string label, string value)
    {
        var item = new ListViewItem(label);
        item.SubItems.Add(value);
        _hashesListView.Items.Add(item);
    }

    private static string BuildDiagnostics(EditDrivenGamePackageRuntimePreviewBridgeBuildResult result)
    {
        var lines = new List<string>
        {
            "sourceArtifactManifestAccepted=false",
            "projectedFileLedgerPassed=" + result.ProjectedPackageFileLedger.Passed,
            "runtimePreviewBridgeProofPassed=" + result.RuntimePreviewBridgeProof.Passed,
            "runtimePreviewNegativeProofPassed=" + result.RuntimePreviewNegativeProof.Passed,
            "winFormsBindingPassed=" + result.WinFormsBindingInventory.Passed,
            "qualityGatePassed=" + result.QualityGateScan.Passed,
            "interactionEntryCount=" + result.RuntimePreviewBridgeProof.InteractionEntryCount
        };
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
