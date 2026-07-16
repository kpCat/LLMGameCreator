using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratedGameplaySavesDialog : Form
{
    private readonly IUnifiedGameProjectWorkspaceController? _controller;
    private GeneratedGameplaySaveMigrationPreview? _preview;

    public GeneratedGameplaySavesDialog()
    {
        InitializeComponent();
        _statusLabel.Text = "Предварительный просмотр. Сервисы приложения недоступны в дизайнере.";
    }

    public GeneratedGameplaySavesDialog(IUnifiedGameProjectWorkspaceController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        InitializeComponent();
        WireEvents();
        RefreshEntries();
    }

    public bool CanPreview => _previewButton.Enabled;
    public bool CanApply => _applyButton.Enabled;
    public int EntryCount => _savesListView.Items.Count;

    private void WireEvents()
    {
        _savesListView.SelectedIndexChanged += (_, _) => RefreshSelection();
        _previewButton.Click += (_, _) => PreviewMigration();
        _applyButton.Click += (_, _) => ApplyMigration();
        _closeButton.Click += (_, _) => Close();
    }

    private void RefreshEntries()
    {
        _savesListView.Items.Clear();
        _preview = null;
        _applyButton.Enabled = false;
        var result = _controller!.ListGeneratedGameplaySaves();
        foreach (var entry in result.Entries.OrderBy(item => item.LegacyRaw)
                     .ThenBy(item => item.SlotName, StringComparer.Ordinal))
        {
            var item = new ListViewItem(entry.SlotName) { Tag = entry };
            item.SubItems.Add(StatusText(entry.Status));
            item.SubItems.Add(ShortToken(entry.CurrentRevisionSha256));
            item.SubItems.Add(entry.SavedWorldTitle);
            item.SubItems.Add(entry.CurrentWorldTitle);
            item.SubItems.Add(entry.Migration is null
                ? string.Empty
                : entry.Migration.PreservedCounts.Values.Sum() + " / "
                  + entry.Migration.DroppedCounts.Values.Sum());
            _savesListView.Items.Add(item);
        }
        if (_savesListView.Items.Count > 0) _savesListView.Items[0].Selected = true;
        _statusLabel.Text = result.Diagnostics.FirstOrDefault()
                            ?? "Выберите сохранение. Перенос выполняется только после отдельной проверки.";
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        _preview = null;
        _applyButton.Enabled = false;
        var entry = SelectedEntry();
        _previewButton.Enabled = entry?.Status is GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED
            or GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED;
        if (entry is not null) _statusLabel.Text = StatusText(entry.Status);
    }

    private void PreviewMigration()
    {
        var entry = SelectedEntry();
        if (entry is null || !_previewButton.Enabled) return;
        _preview = _controller!.PreviewGeneratedGameplaySaveMigration(entry.SlotName);
        _applyButton.Enabled = _preview.Passed;
        _statusLabel.Text = _preview.Passed
            ? "Проверка пройдена: сохранено " + _preview.PreservedCountsByKind.Values.Sum()
              + ", сброшено " + _preview.DroppedCountsByKind.Values.Sum()
              + (_preview.MapReset ? "; позиция будет сброшена на старт." : "; позиция сохранится.")
            : _preview.Diagnostics.FirstOrDefault() ?? "Проверка переноса не пройдена.";
    }

    private void ApplyMigration()
    {
        if (_preview is not { Passed: true } || !_applyButton.Enabled) return;
        var result = _controller!.ApplyGeneratedGameplaySaveMigration(_preview);
        _statusLabel.Text = result.Passed
            ? "Сохранение перенесено в текущий мир."
            : result.Diagnostics.FirstOrDefault() ?? "Перенос не выполнен.";
        if (result.Passed) RefreshEntries();
    }

    private GeneratedGameplaySaveEntry? SelectedEntry() =>
        _savesListView.SelectedItems.Count == 1
            ? _savesListView.SelectedItems[0].Tag as GeneratedGameplaySaveEntry
            : null;

    private static string ShortToken(string value) => value.Length >= 12 ? value[..12] : value;

    private static string StatusText(GeneratedGameplaySaveStatus status) => status switch
    {
        GeneratedGameplaySaveStatus.CURRENT => "Текущее",
        GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED => "Требуется обновление пакета",
        GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED => "Требуется перенос в новый мир",
        GeneratedGameplaySaveStatus.LEGACY_RAW => "Старое непроверенное сохранение",
        _ => "Повреждено"
    };
}
