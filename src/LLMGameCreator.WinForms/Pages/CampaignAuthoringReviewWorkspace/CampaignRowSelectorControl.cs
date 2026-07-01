using LLMGameCreator.Application.Design.SchemaDrivenCampaignAuthoringReviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CampaignRowSelectorControl : UserControl
{
    public CampaignRowSelectorControl()
    {
        InitializeComponent();
        _rowsListView.ItemSelectionChanged += RowsListViewItemSelectionChanged;
    }

    public event EventHandler? SelectedRowIdChanged;

    public string? SelectedRowId =>
        _rowsListView.SelectedItems.Count == 0 ? null : _rowsListView.SelectedItems[0].Tag as string;

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
            item.Tag = row.RowId;
            item.SubItems.Add(row.SeedId);
            item.SubItems.Add(row.RowId);
            item.SubItems.Add(row.StateChanging ? "state-changing" : "static");
            item.SubItems.Add(row.SaveLoadReplayPassed ? "save-load-pass" : "save-load-fail");
            item.SubItems.Add(row.PackageRelativePath);
            _rowsListView.Items.Add(item);
        }

        _rowsListView.EndUpdate();
        if (_rowsListView.Items.Count > 0)
        {
            _rowsListView.Items[0].Selected = true;
        }
    }

    private void RowsListViewItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e)
    {
        if (e.IsSelected)
        {
            SelectedRowIdChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
