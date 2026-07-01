using LLMGameCreator.Application.Design.SchemaDrivenCampaignEditValidateApplyLoop;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignEditFieldSummaryControl : UserControl
{
    public CampaignEditFieldSummaryControl()
    {
        InitializeComponent();
    }

    public void Bind(
        EditableSchemaFieldCatalog catalog,
        ChangeSetCatalog changeSetCatalog,
        string selectedRowId)
    {
        _summaryLabel.Text = "Fields=" + catalog.FieldCount
            + " | candidates=" + changeSetCatalog.CandidateCount
            + " | selected=" + selectedRowId;
        _fieldsListView.BeginUpdate();
        _fieldsListView.Items.Clear();
        foreach (var field in catalog.Fields.OrderBy(item => item.FieldId, StringComparer.Ordinal))
        {
            var candidateCount = changeSetCatalog.Candidates.Count(candidate =>
                candidate.FieldId == field.FieldId
                && (string.IsNullOrWhiteSpace(selectedRowId) || candidate.RowId == selectedRowId));
            var item = new ListViewItem(field.SchemaGroupId);
            item.SubItems.Add(field.FieldId);
            item.SubItems.Add(field.DomainId);
            item.SubItems.Add(string.Join(",", field.AllowedValues));
            item.SubItems.Add(candidateCount.ToString());
            _fieldsListView.Items.Add(item);
        }

        _fieldsListView.EndUpdate();
    }
}
