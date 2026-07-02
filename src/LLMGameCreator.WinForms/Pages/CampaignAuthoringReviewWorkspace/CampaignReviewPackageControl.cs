using LLMGameCreator.Application.Design.EditDrivenPlayableReviewPackageMaterialization;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignReviewPackageControl : UserControl
{
    private EditDrivenPlayableReviewPackageMaterializationBuildResult? _result;

    public CampaignReviewPackageControl()
    {
        InitializeComponent();
    }

    public void Bind(EditDrivenPlayableReviewPackageMaterializationBuildResult result)
    {
        _result = result;
        _statusLabel.Text = "Gate: " + result.Report.ManualGate
            + " required | accepted=false | status=" + result.Report.ImplementationStatus
            + " | rows=" + result.Report.RowCount
            + " | targets=" + result.Report.TargetCount
            + " | files=" + result.Report.ReviewPackageFileCount;
        BindHashes(result);
        BindTargets(result);
        _proofLabel.Text = "Package hash: " + result.ReviewPackageManifest.PackageHash
            + " | stagedRead=" + result.StagedPackageReadProof.Passed
            + " | negative=" + result.TamperNegativeProof.Passed
            + " | ledger=" + result.PackageFileLedger.Passed;
        _diagnosticsTextBox.Text = BuildDiagnostics(result);
    }

    private void BindHashes(EditDrivenPlayableReviewPackageMaterializationBuildResult result)
    {
        _hashesListView.BeginUpdate();
        _hashesListView.Items.Clear();
        AddHash("sourceGoal076ReportHash", result.Report.SourceGoal076ReportHash);
        AddHash("sourceGoal076ManifestHash", result.Report.SourceGoal076ManifestHash);
        AddHash("reviewPackageManifestHash", result.Report.ReviewPackageManifestHash);
        AddHash("packageFileLedgerHash", result.Report.PackageFileLedgerHash);
        AddHash("playerReadablePackageIndexHash", result.Report.PlayerReadablePackageIndexHash);
        AddHash("stateLineageProofHash", result.Report.StateLineageProofHash);
        AddHash("tamperNegativeProofHash", result.Report.TamperNegativeProofHash);
        _hashesListView.EndUpdate();
    }

    private void BindTargets(EditDrivenPlayableReviewPackageMaterializationBuildResult result)
    {
        _targetsListView.BeginUpdate();
        _targetsListView.Items.Clear();
        foreach (var row in result.PackageIndex.Rows)
        {
            foreach (var target in row.Targets)
            {
                var item = new ListViewItem(row.FamilyId);
                item.SubItems.Add(row.SeedId);
                item.SubItems.Add(row.RowId);
                item.SubItems.Add(target.TargetId);
                item.SubItems.Add(target.LogicalPackagePath);
                item.SubItems.Add(target.Sha256);
                _targetsListView.Items.Add(item);
            }
        }

        _targetsListView.EndUpdate();
    }

    private void AddHash(string label, string value)
    {
        var item = new ListViewItem(label);
        item.SubItems.Add(value);
        _hashesListView.Items.Add(item);
    }

    private static string BuildDiagnostics(EditDrivenPlayableReviewPackageMaterializationBuildResult result)
    {
        var lines = new List<string>
        {
            "qualityGatePassed=" + result.QualityGateScan.Passed,
            "winFormsBindingPassed=" + result.WinFormsBindingInventory.Passed,
            "stagedPackageReadProofPassed=" + result.StagedPackageReadProof.Passed,
            "tamperNegativeProofPassed=" + result.TamperNegativeProof.Passed,
            "allFileHashesMatch=" + result.StagedPackageReadProof.AllFileHashesMatch,
            "allExpectedTargetsPresent=" + result.StagedPackageReadProof.AllExpectedTargetsPresent
        };
        lines.AddRange(result.Report.Diagnostics.Select(diagnostic =>
            diagnostic.Severity + ": " + diagnostic.Code + " [" + diagnostic.Target + "]"));
        return string.Join(Environment.NewLine, lines);
    }
}
