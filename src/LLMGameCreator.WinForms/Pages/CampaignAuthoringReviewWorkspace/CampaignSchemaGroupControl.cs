using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignSchemaGroupControl : UserControl
{
    public CampaignSchemaGroupControl()
    {
        InitializeComponent();
    }

    public void Bind(CampaignAuthoringSchema schema, CampaignUiBindingContract binding)
    {
        _groupsListView.BeginUpdate();
        _fieldsListView.BeginUpdate();
        _groupsListView.Items.Clear();
        _fieldsListView.Items.Clear();
        foreach (var group in schema.Groups.OrderBy(item => item.Order))
        {
            var item = new ListViewItem(group.Order.ToString("000"));
            item.SubItems.Add(group.GroupId);
            item.SubItems.Add(group.SourceGoalRange);
            item.SubItems.Add(group.Fields.Count.ToString());
            item.SubItems.Add(group.SourceArtifactRefs.Count.ToString());
            _groupsListView.Items.Add(item);

            foreach (var field in group.Fields)
            {
                var fieldItem = new ListViewItem(group.GroupId);
                fieldItem.SubItems.Add(field.FieldId);
                fieldItem.SubItems.Add(field.ValueKind);
                fieldItem.SubItems.Add(field.SourcePath);
                _fieldsListView.Items.Add(fieldItem);
            }
        }

        _summaryLabel.Text = "Groups=" + schema.Groups.Count
            + " | bindings=" + binding.GroupBindings.Count
            + " | passed=" + schema.Passed;
        _fieldsListView.EndUpdate();
        _groupsListView.EndUpdate();
    }
}
