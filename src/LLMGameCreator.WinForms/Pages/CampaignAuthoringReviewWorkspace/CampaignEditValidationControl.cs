using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignEditValidationControl : UserControl
{
    public CampaignEditValidationControl()
    {
        InitializeComponent();
    }

    public void Bind(ValidationDiagnosticsMatrix matrix, InvalidEditDiagnosticsMatrix invalidMatrix, string selectedRowId)
    {
        _summaryLabel.Text = "Validation=" + matrix.Passed
            + " | valid=" + matrix.ValidCandidateCount
            + " | rejected=" + invalidMatrix.ScenarioCount
            + " | selected=" + selectedRowId;
        _diagnosticsListView.BeginUpdate();
        _diagnosticsListView.Items.Clear();
        foreach (var record in matrix.Records.Where(record =>
                     string.IsNullOrWhiteSpace(selectedRowId) || record.RowId == selectedRowId))
        {
            var item = new ListViewItem(record.Status);
            item.SubItems.Add(record.CandidateId);
            item.SubItems.Add(record.RowId);
            item.SubItems.Add(record.FieldId);
            item.SubItems.Add(record.Diagnostics.Count.ToString());
            _diagnosticsListView.Items.Add(item);
        }

        foreach (var scenario in invalidMatrix.Scenarios)
        {
            var item = new ListViewItem(scenario.ActualStatus);
            item.SubItems.Add(scenario.CandidateId);
            item.SubItems.Add(scenario.ScenarioId);
            item.SubItems.Add("invalid-matrix");
            item.SubItems.Add(scenario.Diagnostics.Count.ToString());
            _diagnosticsListView.Items.Add(item);
        }

        _diagnosticsListView.EndUpdate();
    }
}
