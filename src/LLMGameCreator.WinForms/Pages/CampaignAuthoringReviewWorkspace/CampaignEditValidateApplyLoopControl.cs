using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignEditValidateApplyLoopControl : UserControl
{
    private SchemaDrivenCampaignEditBuildResult? _result;

    public CampaignEditValidateApplyLoopControl()
    {
        InitializeComponent();
    }

    public void Bind(SchemaDrivenCampaignEditBuildResult result)
    {
        _result = result;
        _rowComboBox.BeginUpdate();
        _rowComboBox.Items.Clear();
        foreach (var row in result.DiffMatrix.Rows)
        {
            _rowComboBox.Items.Add(row.RowId);
        }

        _rowComboBox.EndUpdate();
        if (_rowComboBox.Items.Count > 0 && _rowComboBox.SelectedIndex < 0)
        {
            _rowComboBox.SelectedIndex = 0;
        }

        BindSelectedRow();
    }

    public void SelectRow(string? rowId)
    {
        if (string.IsNullOrWhiteSpace(rowId))
        {
            return;
        }

        var index = _rowComboBox.Items.IndexOf(rowId);
        if (index >= 0 && _rowComboBox.SelectedIndex != index)
        {
            _rowComboBox.SelectedIndex = index;
        }
        else
        {
            BindSelectedRow();
        }
    }

    private void RowComboBoxSelectedIndexChanged(object? sender, EventArgs e) =>
        BindSelectedRow();

    private void BindSelectedRow()
    {
        if (_result is null)
        {
            _statusLabel.Text = "Goal 075 edit loop evidence is not loaded.";
            return;
        }

        var selectedRowId = _rowComboBox.SelectedItem as string ?? string.Empty;
        _statusLabel.Text = "Gate: " + _result.Report.ManualGate
            + " required | accepted=false | status=" + _result.Report.ImplementationStatus
            + " | row=" + selectedRowId;
        _fieldSummaryControl.Bind(_result.FieldCatalog, _result.ChangeSetCatalog, selectedRowId);
        _validationControl.Bind(_result.ValidationMatrix, _result.InvalidMatrix, selectedRowId);
        _applyRollbackControl.Bind(
            _result.ApplyRollbackLedger,
            _result.DiffMatrix,
            _result.PreviewExportRefreshPayload,
            selectedRowId);
    }
}
