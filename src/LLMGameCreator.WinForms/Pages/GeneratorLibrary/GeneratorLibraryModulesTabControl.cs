using System.Text.Json;
using LLMGameCreator.Application.Design;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratorLibraryModulesTabControl : UserControl
{
    public GeneratorLibraryModulesTabControl()
    {
        InitializeComponent();
        _modulesListView.SelectedIndexChanged += (_, _) => ShowSelectedModule();
    }

    public void SetModules(IReadOnlyList<GeneratorModuleRecord> modules)
    {
        _modulesListView.Items.Clear();
        foreach (var module in modules)
        {
            var item = new ListViewItem(module.Id);
            item.SubItems.Add(module.Category);
            item.SubItems.Add(CountJsonArray(module.CapabilitiesJson).ToString());
            item.SubItems.Add(module.BatchId);
            item.SubItems.Add(module.Path);
            item.Tag = module;
            _modulesListView.Items.Add(item);
        }

        _detailsTextBox.Text = modules.Count == 0 ? "No imported modules." : string.Empty;
    }

    private void ShowSelectedModule()
    {
        if (_modulesListView.SelectedItems.Count == 0 || _modulesListView.SelectedItems[0].Tag is not GeneratorModuleRecord module)
        {
            return;
        }

        _detailsTextBox.Text =
            $"Id: {module.Id}\r\n" +
            $"Path: {module.Path}\r\n" +
            $"Batch: {module.BatchId}\r\n" +
            $"Category: {module.Category}\r\n" +
            $"Capabilities: {module.CapabilitiesJson}\r\n" +
            $"Dependencies: {module.DependenciesJson}\r\n" +
            $"Runtime targets: {module.RuntimeTargetsJson}\r\n" +
            $"Turn modes: {module.TurnModesJson}\r\n" +
            $"Combat modes: {module.CombatModesJson}\r\n" +
            $"Source manifest: {module.SourceManifestPath}";
    }

    private static int CountJsonArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json)?.Count ?? 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }
}
