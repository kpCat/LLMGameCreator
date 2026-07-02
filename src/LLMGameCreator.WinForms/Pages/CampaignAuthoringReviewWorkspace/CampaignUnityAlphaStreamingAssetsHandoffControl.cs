using LLMGameCreator.Application.Design.EditDrivenUnityAlphaStreamingAssetsHandoff;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignUnityAlphaStreamingAssetsHandoffControl : UserControl
{
    private EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult? _result;

    public CampaignUnityAlphaStreamingAssetsHandoffControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result)
    {
        _result = result;
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | files=" + result.FileLedger.FileCount
            + " | rows=" + result.Report.RowCount
            + " | targets=" + result.Report.TargetCount
            + " | commands=" + result.Report.CommandCount;
        _proofLabel.Text = "root=" + result.Report.StreamingAssetsRelativeRoot
            + " | probeRead=" + result.ProbeReadProof.Passed
            + " | negative=" + result.NegativeProof.Passed
            + " | commandTranscript=" + result.CommandTranscriptProof.Passed
            + " | quality=" + result.QualityGateScan.Passed;
        BindHashes(result);
        BindPayloadFiles(result);
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void BindHashes(EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result)
    {
        _hashesListView.BeginUpdate();
        _hashesListView.Items.Clear();
        AddHash("projectedPackageHash", result.Report.ProjectedPackageHash);
        AddHash("commandScriptHash", result.Report.CommandScriptHash);
        AddHash("transcriptHash", result.Report.TranscriptHash);
        AddHash("stateHashChainHash", result.Report.StateHashChainHash);
        AddHash("finalCoverageStateHash", result.Report.FinalCoverageStateHash);
        AddHash("replayFinalStateHash", result.Report.ReplayFinalStateHash);
        AddHash("handoffManifestHash", result.Report.HandoffManifestHash);
        AddHash("fileLedgerHash", result.Report.FileLedgerHash);
        AddHash("probeReadProofHash", result.Report.ProbeReadProofHash);
        AddHash("qualityGateScanHash", result.Report.QualityGateScanHash);
        _hashesListView.EndUpdate();
    }

    private void BindPayloadFiles(EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result)
    {
        _payloadListView.BeginUpdate();
        _payloadListView.Items.Clear();
        foreach (var file in result.FileLedger.Files.OrderBy(file => file.RelativePath, StringComparer.Ordinal))
        {
            var item = new ListViewItem(file.RelativePath);
            item.SubItems.Add(file.Role);
            item.SubItems.Add(file.ByteCount.ToString());
            item.SubItems.Add(file.Sha256);
            _payloadListView.Items.Add(item);
        }

        _payloadListView.EndUpdate();
    }

    private void AddHash(string label, string value)
    {
        var item = new ListViewItem(label);
        item.SubItems.Add(value);
        _hashesListView.Items.Add(item);
    }

    private static string BuildDiagnostics(EditDrivenUnityAlphaStreamingAssetsHandoffBuildResult result)
    {
        var lines = new List<string>
        {
            "sourceManifestDiagnostics=" + result.SourceArtifactManifest.Diagnostics.Count,
            "goal081AcceptedByHandoff=" + result.SourceArtifactManifest.Goal081AcceptedByHandoff,
            "payloadFileCount=" + result.FileLedger.FileCount,
            "probeReadPassed=" + result.ProbeReadProof.Passed,
            "negativeProofPassed=" + result.NegativeProof.Passed,
            "commandTranscriptProofPassed=" + result.CommandTranscriptProof.Passed,
            "winFormsBindingPassed=" + result.WinFormsBindingInventory.Passed,
            "qualityGatePassed=" + result.QualityGateScan.Passed,
            "alphaRuntimeBootstrapUnchanged=" + result.QualityGateScan.AlphaRuntimeBootstrapUnchanged,
            "unityProbeLineCount=" + result.QualityGateScan.UnityProbeLineCount
        };
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
