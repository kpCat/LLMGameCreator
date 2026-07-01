using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignEditApplyRollbackControl : UserControl
{
    public CampaignEditApplyRollbackControl()
    {
        InitializeComponent();
    }

    public void Bind(
        ApplyRollbackLedger ledger,
        RowBeforeAfterDiffMatrix diffMatrix,
        PreviewExportRefreshPayload refreshPayload,
        string selectedRowId)
    {
        _summaryLabel.Text = "Apply=" + ledger.Passed
            + " | rows=" + ledger.RowCount
            + " | changes=" + ledger.AppliedChangeCount
            + " | refreshRows=" + refreshPayload.ChangedRowCount;
        _rowsListView.BeginUpdate();
        _rowsListView.Items.Clear();
        foreach (var row in ledger.Rows.Where(row =>
                     string.IsNullOrWhiteSpace(selectedRowId) || row.RowId == selectedRowId))
        {
            var diff = diffMatrix.Rows.First(item => item.RowId == row.RowId);
            var item = new ListViewItem(row.FamilyId);
            item.SubItems.Add(row.SeedId);
            item.SubItems.Add(row.RowId);
            item.SubItems.Add(row.StateChanged ? "changed" : "unchanged");
            item.SubItems.Add(row.RollbackRestored ? "rollback-ok" : "rollback-failed");
            item.SubItems.Add(diff.ChangedFields.Count.ToString());
            item.SubItems.Add(row.AfterHash);
            _rowsListView.Items.Add(item);
        }

        _rowsListView.EndUpdate();
    }
}
