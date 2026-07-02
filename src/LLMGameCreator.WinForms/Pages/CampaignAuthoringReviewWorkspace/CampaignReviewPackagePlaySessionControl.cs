using LLMGameCreator.Application.Design.EditDrivenReviewPackagePlayableSession;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignReviewPackagePlaySessionControl : UserControl
{
    private EditDrivenReviewPackagePlayableSessionBuildResult? _result;

    public CampaignReviewPackagePlaySessionControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenReviewPackagePlayableSessionBuildResult result)
    {
        _result = result;
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | rows=" + result.Report.RowCount
            + " | targets=" + result.Report.TargetCount
            + " | actions=" + result.Report.ActionCount;
        BindHashes(result);
        BindCommands(result);
        _proofLabel.Text = "Read=" + result.PackageReadProof.Passed
            + " | replay=" + result.ReplayProof.Passed
            + " | negative=" + result.TamperNegativeProof.Passed
            + " | initial!=final=" + result.ReplayProof.InitialDiffersFromFinal
            + " | replayHashMatch=" + result.ReplayProof.ReplayFinalHashMatchesOriginal;
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void BindHashes(EditDrivenReviewPackagePlayableSessionBuildResult result)
    {
        _hashesListView.BeginUpdate();
        _hashesListView.Items.Clear();
        AddHash("sourceGoal077ReportHash", result.Report.SourceGoal077ReportHash);
        AddHash("packageManifestHash", result.Report.PackageManifestHash);
        AddHash("packageFileLedgerHash", result.Report.PackageFileLedgerHash);
        AddHash("packageIndexHash", result.Report.PackageIndexHash);
        AddHash("playerReadableIndexHash", result.Report.PlayerReadableIndexHash);
        AddHash("initialStateHash", result.Report.InitialStateHash);
        AddHash("savedSessionHash", result.Report.SavedSessionHash);
        AddHash("finalStateHash", result.Report.FinalStateHash);
        AddHash("replayFinalStateHash", result.Report.ReplayFinalStateHash);
        _hashesListView.EndUpdate();
    }

    private void BindCommands(EditDrivenReviewPackagePlayableSessionBuildResult result)
    {
        _commandsListView.BeginUpdate();
        _commandsListView.Items.Clear();
        foreach (var group in result.PlayerCommandIndex.CommandGroups)
        {
            var item = new ListViewItem(group.ProfileId);
            item.SubItems.Add(group.RowId);
            item.SubItems.Add(group.CommandIds.Count.ToString());
            item.SubItems.Add(string.Join(", ", group.CommandIds.Take(4)));
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

    private static string BuildDiagnostics(EditDrivenReviewPackagePlayableSessionBuildResult result)
    {
        var lines = new List<string>
        {
            "packageReadProofPassed=" + result.PackageReadProof.Passed,
            "allLedgerFileHashesMatch=" + result.PackageReadProof.AllLedgerFileHashesMatch,
            "actionLogPassed=" + result.ActionLog.Passed,
            "stateChainPassed=" + result.StateChain.Passed,
            "replayProofPassed=" + result.ReplayProof.Passed,
            "tamperNegativeProofPassed=" + result.TamperNegativeProof.Passed,
            "winFormsBindingPassed=" + result.WinFormsBindingInventory.Passed,
            "qualityGatePassed=" + result.QualityGateScan.Passed
        };
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
