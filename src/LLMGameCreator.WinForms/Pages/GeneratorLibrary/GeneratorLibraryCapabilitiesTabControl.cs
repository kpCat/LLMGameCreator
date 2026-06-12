using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryCapabilitiesTabControl : UserControl
{
    public GeneratorLibraryCapabilitiesTabControl()
    {
        InitializeComponent();
        _capabilitiesListView.SelectedIndexChanged += (_, _) => ShowSelectedCapability();
    }

    public void SetCapabilities(IReadOnlyList<CapabilityModuleRecord> capabilities)
    {
        _capabilitiesListView.Items.Clear();
        foreach (var capability in capabilities)
        {
            var item = new ListViewItem(capability.Id);
            item.SubItems.Add(capability.Category);
            item.SubItems.Add(capability.SourceManifestPath);
            item.SubItems.Add(capability.RuntimeTargetsJson);
            item.SubItems.Add(capability.TurnModesJson);
            item.SubItems.Add(capability.CombatModesJson);
            item.Tag = capability;
            _capabilitiesListView.Items.Add(item);
        }

        _detailsTextBox.Text = capabilities.Count == 0 ? "No imported capabilities." : string.Empty;
    }

    private void ShowSelectedCapability()
    {
        if (_capabilitiesListView.SelectedItems.Count == 0 || _capabilitiesListView.SelectedItems[0].Tag is not CapabilityModuleRecord capability)
        {
            return;
        }

        _detailsTextBox.Text =
            $"Id: {capability.Id}\r\n" +
            $"Category: {capability.Category}\r\n" +
            $"Source manifest: {capability.SourceManifestPath}\r\n" +
            $"Runtime targets: {capability.RuntimeTargetsJson}\r\n" +
            $"Turn modes: {capability.TurnModesJson}\r\n" +
            $"Combat modes: {capability.CombatModesJson}\r\n" +
            $"UI modes: {capability.UiModesJson}\r\n" +
            $"World scales: {capability.WorldScalesJson}\r\n" +
            $"Metadata: {capability.MetadataJson}";
    }
}
