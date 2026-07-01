using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignRowSelectorControl : UserControl
{
    public CampaignRowSelectorControl()
    {
        InitializeComponent();
    }

    public void Bind(CampaignRowSelector selector, CampaignUiBindingContract binding)
    {
        _summaryLabel.Text = "Rows=" + selector.RowCount
            + " | families=" + selector.FamilyCount
            + " | seeds=" + selector.SeedCount
            + " | binding=" + binding.RowSelector.ControlKey;
        _rowsListView.BeginUpdate();
        _rowsListView.Items.Clear();
        foreach (var row in selector.Rows)
        {
            var item = new ListViewItem(row.FamilyId);
            item.SubItems.Add(row.SeedId);
            item.SubItems.Add(row.RowId);
            item.SubItems.Add(row.StateChanging ? "state-changing" : "static");
            item.SubItems.Add(row.SaveLoadReplayPassed ? "save-load-pass" : "save-load-fail");
            item.SubItems.Add(row.PackageRelativePath);
            _rowsListView.Items.Add(item);
        }

        _rowsListView.EndUpdate();
    }
}
