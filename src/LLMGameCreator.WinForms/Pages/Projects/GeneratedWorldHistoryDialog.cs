using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratedWorldHistoryDialog : Form
{
    private readonly GeneratedWorldHistoryReadResult _history;

    public GeneratedWorldHistoryDialog()
        : this(new GeneratedWorldHistoryReadResult())
    {
    }

    public GeneratedWorldHistoryDialog(GeneratedWorldHistoryReadResult history)
    {
        _history = history ?? throw new ArgumentNullException(nameof(history));
        InitializeComponent();
        BindEntries();
        WireEvents();
        RefreshSelection();
    }

    public string SelectedWorldId =>
        _worldsListView.SelectedItems.Count == 1
            ? (_worldsListView.SelectedItems[0].Tag as GeneratedWorldHistoryEntry)?.WorldId ?? string.Empty
            : string.Empty;

    public bool CanRestore => _restoreButton.Enabled;
    public IReadOnlyList<GeneratedWorldHistoryEntry> Entries => _history.Entries;

    private void BindEntries()
    {
        foreach (var entry in _history.Entries.OrderByDescending(item => item.IsCurrent)
                     .ThenBy(item => item.WorldId, StringComparer.Ordinal))
        {
            if (entry.Manifest is not { } manifest) continue;
            var item = new ListViewItem(entry.IsCurrent ? "Да" : string.Empty) { Tag = entry };
            item.SubItems.Add(manifest.Seed);
            item.SubItems.Add(ModeTitle(manifest.Mode));
            item.SubItems.Add(manifest.PresetId);
            item.SubItems.Add(manifest.Counts.Regions.ToString());
            item.SubItems.Add(manifest.Counts.Factions.ToString());
            item.SubItems.Add(manifest.Counts.Actors.ToString());
            item.SubItems.Add(manifest.Counts.Encounters.ToString());
            item.SubItems.Add(manifest.Counts.QuestEvents.ToString());
            item.SubItems.Add(manifest.StartRegionTitle);
            item.SubItems.Add(manifest.TravelDestinationTitle);
            _worldsListView.Items.Add(item);
        }
        if (_worldsListView.Items.Count > 0) _worldsListView.Items[0].Selected = true;
    }

    private void WireEvents()
    {
        _worldsListView.SelectedIndexChanged += (_, _) => RefreshSelection();
        _worldsListView.DoubleClick += (_, _) => Restore();
        _restoreButton.Click += (_, _) => Restore();
        _cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
    }

    private void RefreshSelection()
    {
        var selected = _worldsListView.SelectedItems.Count == 1
            ? _worldsListView.SelectedItems[0].Tag as GeneratedWorldHistoryEntry
            : null;
        _restoreButton.Enabled = selected is { Passed: true, IsCurrent: false };
        _statusLabel.Text = selected?.IsCurrent == true
            ? "Текущий мир уже активен. Выберите сохранённый мир."
            : selected is null
                ? "Выберите сохранённый мир."
                : "Будет собран и проверен отдельный кандидат с текущими механиками проекта.";
    }

    private void Restore()
    {
        if (!_restoreButton.Enabled) return;
        DialogResult = DialogResult.OK;
    }

    private static string ModeTitle(string mode) => mode switch
    {
        ProceduralGameGenerationModes.AuthoredSmallWorld => "Авторский компактный мир",
        ProceduralGameGenerationModes.SemiProceduralRegions => "Полупроцедурные регионы",
        ProceduralGameGenerationModes.FullySeededWorld => "Полностью генерируемый мир",
        _ => mode
    };
}
