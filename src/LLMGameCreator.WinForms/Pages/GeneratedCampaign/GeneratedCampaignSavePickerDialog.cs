using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratedCampaignSavePickerDialog : Form
{
    public GeneratedGameplaySaveEntry? SelectedEntry => _list.SelectedItem as GeneratedGameplaySaveEntry;
    public bool MigrateRequested { get; private set; }
    public GeneratedCampaignSavePickerDialog(IEnumerable<GeneratedGameplaySaveEntry> entries) { InitializeComponent(); foreach(var entry in entries) _list.Items.Add(entry); _list.DisplayMember=nameof(GeneratedGameplaySaveEntry.SlotName); }
    private void ContinueClick(object? sender, EventArgs e) { if(SelectedEntry?.Status==GeneratedGameplaySaveStatus.CURRENT) DialogResult=DialogResult.OK; }
    private void MigrateClick(object? sender, EventArgs e) { if(SelectedEntry?.Status is GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED or GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED){MigrateRequested=true;DialogResult=DialogResult.OK;} }
}
