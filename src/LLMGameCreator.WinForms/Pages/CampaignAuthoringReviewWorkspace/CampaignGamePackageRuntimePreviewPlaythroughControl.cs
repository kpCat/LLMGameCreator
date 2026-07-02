using LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewPlaythrough;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignGamePackageRuntimePreviewPlaythroughControl : UserControl
{
    private EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult? _result;

    public CampaignGamePackageRuntimePreviewPlaythroughControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result)
    {
        _result = result;
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | commands=" + result.Report.CommandCount
            + " | rows=" + result.CoverageLedger.CoveredRowCount
            + " | targets=" + result.CoverageLedger.CoveredTargetCount
            + " | actions=" + result.CoverageLedger.CoveredGoal078ActionCount;
        _proofLabel.Text = "packageRead=" + result.PackageReadProof.Passed
            + " | replay=" + result.Transcript.Passed
            + " | stateHash=" + result.StateHashChain.Passed
            + " | coverage=" + result.CoverageLedger.Passed
            + " | negative=" + result.NegativeProof.Passed
            + " | quality=" + result.QualityGateScan.Passed;
        BindHashes(result);
        BindCommands(result);
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void BindHashes(EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result)
    {
        _hashesListView.BeginUpdate();
        _hashesListView.Items.Clear();
        AddHash("sourceArtifactManifestHash", result.Report.SourceArtifactManifestHash);
        AddHash("projectedPackageHash", result.Report.ProjectedPackageHash);
        AddHash("commandScriptHash", result.Report.CommandScriptHash);
        AddHash("transcriptHash", result.Report.TranscriptHash);
        AddHash("stateHashChainHash", result.Report.StateHashChainHash);
        AddHash("coverageLedgerHash", result.Report.CoverageLedgerHash);
        AddHash("negativeProofHash", result.Report.NegativeProofHash);
        AddHash("qualityGateScanHash", result.Report.QualityGateScanHash);
        _hashesListView.EndUpdate();
    }

    private void BindCommands(EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result)
    {
        _commandsListView.BeginUpdate();
        _commandsListView.Items.Clear();
        foreach (var entry in result.Transcript.Entries.OrderBy(entry => entry.CommandIndex))
        {
            var item = new ListViewItem(entry.CommandIndex.ToString("000"));
            item.SubItems.Add(entry.CommandType);
            item.SubItems.Add(entry.RowId);
            item.SubItems.Add(entry.TargetId);
            item.SubItems.Add(entry.CoveredGoal078ActionCount.ToString());
            item.SubItems.Add(entry.StateHash);
            _commandsListView.Items.Add(item);
        }

        _commandsListView.EndUpdate();
    }

    private void AddHash(string label, string value)
    {
        var item = new ListViewItem(label);
        item.SubItems.Add(value);
        _hashesListView.Items.Add(item);
    }

    private static string BuildDiagnostics(EditDrivenGamePackageRuntimePreviewPlaythroughBuildResult result)
    {
        var lines = new List<string>
        {
            "sourceManifestDiagnostics=" + result.SourceArtifactManifest.Diagnostics.Count,
            "packageReadProofPassed=" + result.PackageReadProof.Passed,
            "commandScriptPassed=" + result.CommandScript.Passed,
            "transcriptPassed=" + result.Transcript.Passed,
            "stateHashChainPassed=" + result.StateHashChain.Passed,
            "coverageLedgerPassed=" + result.CoverageLedger.Passed,
            "negativeProofPassed=" + result.NegativeProof.Passed,
            "winFormsBindingPassed=" + result.WinFormsBindingInventory.Passed,
            "qualityGatePassed=" + result.QualityGateScan.Passed,
            "finalStateHash=" + result.Transcript.FinalStateHash
        };
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
