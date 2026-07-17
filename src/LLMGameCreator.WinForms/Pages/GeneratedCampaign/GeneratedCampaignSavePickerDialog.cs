using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratedCampaignSavePickerDialog : Form
{
    public GeneratedCampaignSavePickerDialog(IEnumerable<GeneratedGameplaySaveEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        InitializeComponent();
        WireEvents();
        BindEntries(GeneratedCampaignProjectionService.ProjectSaves(entries));
    }

    public GeneratedGameplaySaveEntry? SelectedEntry =>
        _list.SelectedItems.Count == 1
            ? (_list.SelectedItems[0].Tag as GeneratedCampaignSaveEntryProjection)?.Entry
            : null;

    public bool MigrateRequested { get; private set; }

    internal string SelectedStatusText => _details.Text;
    internal bool ContinueEnabled => _continue.Enabled;
    internal bool MigrateEnabled => _migrate.Enabled;

    private void WireEvents()
    {
        _list.SelectedIndexChanged += SelectionChanged;
        _list.DoubleClick += ContinueClick;
        _continue.Click += ContinueClick;
        _migrate.Click += MigrateClick;
    }

    private void BindEntries(IEnumerable<GeneratedCampaignSaveEntryProjection> entries)
    {
        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var entry in entries)
        {
            var item = new ListViewItem(entry.Slot) { Tag = entry };
            item.SubItems.Add(entry.StatusTitle);
            item.SubItems.Add(entry.RevisionCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            _list.Items.Add(item);
        }

        _list.EndUpdate();
        if (_list.Items.Count > 0) _list.Items[0].Selected = true;
    }

    private void SelectionChanged(object? sender, EventArgs eventArgs)
    {
        var selected = _list.SelectedItems.Count == 1
            ? _list.SelectedItems[0].Tag as GeneratedCampaignSaveEntryProjection
            : null;
        _continue.Enabled = selected?.CanContinue == true;
        _migrate.Enabled = selected?.CanMigrate == true;
        _details.Text = selected is null
            ? "Выберите сохранение."
            : "Сохранённый мир: " + selected.SavedWorldTitle
              + Environment.NewLine + "Текущий мир: " + selected.CurrentWorldTitle
              + (string.IsNullOrWhiteSpace(selected.MigrationSummary)
                  ? string.Empty
                  : Environment.NewLine + selected.MigrationSummary);
    }

    private void ContinueClick(object? sender, EventArgs eventArgs)
    {
        if (SelectedEntry?.Status != GeneratedGameplaySaveStatus.CURRENT) return;
        MigrateRequested = false;
        DialogResult = DialogResult.OK;
    }

    private void MigrateClick(object? sender, EventArgs eventArgs)
    {
        if (SelectedEntry?.Status is not (GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED
            or GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED)) return;
        MigrateRequested = true;
        DialogResult = DialogResult.OK;
    }
}
