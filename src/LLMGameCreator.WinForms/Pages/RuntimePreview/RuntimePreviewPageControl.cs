using System.Text;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class RuntimePreviewPageControl : UserControl, IEditorPage
{
    private const int SplitPanel1MinSize = 420;
    private const int SplitPanel2MinSize = 320;

    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IGameRuntime? _runtime;
    private readonly GeneratedPackageRuntimePreviewService _previewService;
    private readonly GeneratedContentInteractionPreviewService _interactionService;
    private GeneratedContentInteractionCatalog _interactionCatalog = new();
    private GameState? _state;
    private bool _splitterInitialized;
    private bool _updatingGeneratedSelection;

    public RuntimePreviewPageControl()
    {
        _previewService = new GeneratedPackageRuntimePreviewService();
        _interactionService = new GeneratedContentInteractionPreviewService();
        InitializeComponent();
        ConfigureSplitSafety();
        WireGeneratedContentEvents();
        RefreshGeneratedPreview(null, null);
    }

    public RuntimePreviewPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IGameRuntime runtime,
        GeneratedPackageRuntimePreviewService previewService,
        GeneratedContentInteractionPreviewService interactionService)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtime = runtime;
        _previewService = previewService;
        _interactionService = interactionService;
        InitializeComponent();
        ConfigureSplitSafety();
        WireGeneratedContentEvents();
        _startButton.Click += (_, _) => StartRuntime();
        _canvas.CommandRequested += command => ExecuteCommand(command);
        RefreshGeneratedPreview(null, null);
    }

    public string Id => "runtime-preview";
    public string Title => "Runtime Preview";
    public int SortOrder => 50;
    Control IEditorPage. View => this;

    public void OnActivated()
    {
        _canvas.Focus();
    }

    private void StartRuntime()
    {
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null || _runtime == null)
        {
            AppendLog("Проект игры не открыт или runtime недоступен.");
            return;
        }

        var result = _runtime.Start(package);
        _state = result.State;
        ApplyResult(package, result);
        AppendGeneratedStartSummary(package, result.State);
    }

    private void ExecuteCommand(PlayerCommand command)
    {
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null || _state == null || _runtime == null)
        {
            AppendLog("Сначала запусти runtime.");
            return;
        }

        var result = _runtime.Execute(package, _state, command);
        _state = result.State;
        ApplyResult(package, result);
    }

    private void ApplyResult(GamePackageDefinition package, CommandResult result)
    {
        foreach (var runtimeEvent in result.Events)
        {
            AppendLog(runtimeEvent.Message);
        }

        RefreshGeneratedPreview(package, result.State);
        _canvas.SetState(package, result.State);
    }

    private void AppendLog(string message)
    {
        _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void ConfigureSplitSafety()
    {
        _rootSplitContainer.SizeChanged += (_, _) => ApplySafeInitialSplitterDistance();
    }

    private void WireGeneratedContentEvents()
    {
        _generatedCategoryComboBox.DisplayMember = nameof(GeneratedContentInteractionCategory.Title);
        _generatedEntriesListBox.DisplayMember = nameof(GeneratedContentInteractionEntry.Title);
        _generatedCategoryComboBox.SelectedIndexChanged += (_, _) => GeneratedCategoryChanged();
        _generatedEntriesListBox.SelectedIndexChanged += (_, _) => GeneratedEntryChanged();
        _appendGeneratedSelectionButton.Click += (_, _) => AppendGeneratedSelectionToLog();
    }

    private void ApplySafeInitialSplitterDistance()
    {
        if (_splitterInitialized)
        {
            return;
        }

        var width = _rootSplitContainer.ClientSize.Width;
        var min = SplitPanel1MinSize;
        var max = width - SplitPanel2MinSize;

        if (width <= 0 || max < min)
        {
            return;
        }

        var desired = (int)(width * 0.68);
        if (desired < min)
        {
            desired = min;
        }
        else if (desired > max)
        {
            desired = max;
        }

        _rootSplitContainer.SplitterDistance = desired;
        _rootSplitContainer.Panel1MinSize = SplitPanel1MinSize;
        _rootSplitContainer.Panel2MinSize = SplitPanel2MinSize;
        _splitterInitialized = true;
    }

    private void RefreshGeneratedPreview(GamePackageDefinition? package, GameState? state)
    {
        var previousCategoryId = (_generatedCategoryComboBox.SelectedItem as GeneratedContentInteractionCategory)?.Id;
        var previousEntryId = (_generatedEntriesListBox.SelectedItem as GeneratedContentInteractionEntry)?.EntryId;

        if (package == null)
        {
            _generatedContentTextBox.Text = "No package is running.";
            ApplyInteractionCatalog(new GeneratedContentInteractionCatalog(), null, null);
            return;
        }

        var model = _previewService.Build(package, state);
        _generatedContentTextBox.Text = FormatGeneratedPreview(model);
        ApplyInteractionCatalog(_interactionService.Build(model), previousCategoryId, previousEntryId);
    }

    private void ApplyInteractionCatalog(
        GeneratedContentInteractionCatalog catalog,
        string? preferredCategoryId,
        string? preferredEntryId)
    {
        _interactionCatalog = catalog;
        _updatingGeneratedSelection = true;
        try
        {
            _generatedCategoryComboBox.BeginUpdate();
            _generatedCategoryComboBox.Items.Clear();
            foreach (var category in catalog.Categories)
            {
                _generatedCategoryComboBox.Items.Add(category);
            }

            _generatedCategoryComboBox.EndUpdate();

            var categoryIndex = FindCategoryIndex(preferredCategoryId);
            if (categoryIndex < 0)
            {
                categoryIndex = FindFirstPopulatedCategoryIndex();
            }

            _generatedCategoryComboBox.SelectedIndex = categoryIndex;
            PopulateGeneratedEntries(_generatedCategoryComboBox.SelectedItem as GeneratedContentInteractionCategory, preferredEntryId);
        }
        finally
        {
            _updatingGeneratedSelection = false;
        }
    }

    private void GeneratedCategoryChanged()
    {
        if (_updatingGeneratedSelection)
        {
            return;
        }

        PopulateGeneratedEntries(_generatedCategoryComboBox.SelectedItem as GeneratedContentInteractionCategory, null);
    }

    private void PopulateGeneratedEntries(GeneratedContentInteractionCategory? category, string? preferredEntryId)
    {
        _updatingGeneratedSelection = true;
        try
        {
            _generatedEntriesListBox.BeginUpdate();
            _generatedEntriesListBox.Items.Clear();
            if (category != null)
            {
                foreach (var entry in category.Entries)
                {
                    _generatedEntriesListBox.Items.Add(entry);
                }
            }

            _generatedEntriesListBox.EndUpdate();
            _generatedEntriesListBox.SelectedIndex = FindEntryIndex(preferredEntryId);
            UpdateGeneratedDetails();
        }
        finally
        {
            _updatingGeneratedSelection = false;
        }
    }

    private void GeneratedEntryChanged()
    {
        if (!_updatingGeneratedSelection)
        {
            UpdateGeneratedDetails();
        }
    }

    private void UpdateGeneratedDetails()
    {
        var entry = _generatedEntriesListBox.SelectedItem as GeneratedContentInteractionEntry;
        _generatedDetailsTextBox.Text = entry?.DetailsText ?? "No generated entry selected.";
        _appendGeneratedSelectionButton.Enabled = entry != null;
    }

    private void AppendGeneratedSelectionToLog()
    {
        var category = _generatedCategoryComboBox.SelectedItem as GeneratedContentInteractionCategory;
        var entry = _generatedEntriesListBox.SelectedItem as GeneratedContentInteractionEntry;
        if (category == null || entry == null)
        {
            return;
        }

        var references = entry.ReferenceIds.Count == 0
            ? string.Empty
            : $"; refs: {string.Join(", ", entry.ReferenceIds)}";
        AppendLog($"Generated content: {category.Title} / {entry.Title}{references}");
    }

    private int FindCategoryIndex(string? categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return -1;
        }

        for (var index = 0; index < _interactionCatalog.Categories.Count; index++)
        {
            if (string.Equals(_interactionCatalog.Categories[index].Id, categoryId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private int FindFirstPopulatedCategoryIndex()
    {
        for (var index = 0; index < _interactionCatalog.Categories.Count; index++)
        {
            if (_interactionCatalog.Categories[index].Entries.Count > 0)
            {
                return index;
            }
        }

        return _interactionCatalog.Categories.Count == 0 ? -1 : 0;
    }

    private int FindEntryIndex(string? entryId)
    {
        if (!string.IsNullOrWhiteSpace(entryId))
        {
            for (var index = 0; index < _generatedEntriesListBox.Items.Count; index++)
            {
                if (_generatedEntriesListBox.Items[index] is GeneratedContentInteractionEntry entry
                    && string.Equals(entry.EntryId, entryId, StringComparison.Ordinal))
                {
                    return index;
                }
            }
        }

        return _generatedEntriesListBox.Items.Count == 0 ? -1 : 0;
    }

    private void AppendGeneratedStartSummary(GamePackageDefinition package, GameState state)
    {
        var model = _previewService.Build(package, state);
        var sceneTitle = FirstNonEmpty(model.CurrentScene?.Title, model.CurrentMapName, model.CurrentMapId);

        if (!string.IsNullOrWhiteSpace(sceneTitle))
        {
            AppendLog($"\u0421\u0446\u0435\u043d\u0430: {sceneTitle}");
        }

        if (!string.IsNullOrWhiteSpace(model.CurrentScene?.Description))
        {
            AppendLog(model.CurrentScene.Description);
        }

        AppendLog($"\u0414\u043e\u0441\u0442\u0443\u043f\u043d\u043e \u043a\u0432\u0435\u0441\u0442\u043e\u0432: {model.Quests.Count}");
        AppendLog($"\u0414\u043e\u0441\u0442\u0443\u043f\u043d\u043e \u043c\u0435\u0445\u0430\u043d\u0438\u043a: {model.Mechanics.Count}");
        AppendLog($"\u0420\u0435\u0433\u0438\u043e\u043d\u044b: {model.Regions.Count}; NPC: {model.Npcs.Count}; \u043f\u0440\u0435\u0434\u043c\u0435\u0442\u044b: {model.Items.Count}; \u0434\u0438\u0430\u043b\u043e\u0433\u0438: {model.Dialogues.Count}; \u0432\u0441\u0442\u0440\u0435\u0447\u0438: {model.Encounters.Count}");
    }

    private static string FormatGeneratedPreview(GeneratedPackageRuntimePreviewModel model)
    {
        var builder = new StringBuilder();
        AppendSection(builder, "Package");
        AppendLine(builder, "Title", FirstNonEmpty(model.PackageTitle, "(untitled)"));
        AppendLine(builder, "Description", model.PackageDescription);
        AppendLine(builder, "Map", JoinNonEmpty(model.CurrentMapId, model.CurrentMapName));

        AppendSection(builder, "Current scene");
        if (model.CurrentScene == null)
        {
            builder.AppendLine("(not mapped)");
        }
        else
        {
            AppendLine(builder, "Source", model.CurrentScene.SourceId);
            AppendLine(builder, "Title", model.CurrentScene.Title);
            AppendLine(builder, "Description", model.CurrentScene.Description);
            AppendLine(builder, "Purpose", model.CurrentScene.Purpose);
        }

        AppendSection(builder, "Profile");
        AppendLine(builder, "Title", model.Profile.Title);
        AppendLine(builder, "Description", model.Profile.Description);
        AppendLine(builder, "Genre", model.Profile.Genre);
        AppendLine(builder, "Tone", model.Profile.Tone);
        AppendLine(builder, "Core loop", string.Join(", ", model.Profile.CoreLoop));
        AppendLine(builder, "Pillars", string.Join(", ", model.Profile.Pillars));

        AppendSection(builder, $"Quests ({model.Quests.Count})");
        AppendItems(builder, model.Quests.Select(quest =>
            $"{FirstNonEmpty(quest.Title, quest.PackageQuestId, quest.SourceId)}: {quest.Description}"));

        AppendSection(builder, $"Mechanics ({model.Mechanics.Count})");
        AppendItems(builder, model.Mechanics.Select(mechanic =>
            $"{FirstNonEmpty(mechanic.Name, mechanic.PackageAbilityId, mechanic.SourceId)}: {mechanic.Description}"));

        AppendGeneratedContentItems(builder, "Regions", model.Regions);
        AppendGeneratedContentItems(builder, "NPCs", model.Npcs);
        AppendGeneratedContentItems(builder, "Items", model.Items);
        AppendGeneratedContentItems(builder, "Dialogues", model.Dialogues);
        AppendGeneratedContentItems(builder, "Encounters", model.Encounters);

        AppendSection(builder, $"Applied artifacts ({model.Provenance.Count})");
        AppendItems(builder, model.Provenance.Select(provenance =>
            $"{FirstNonEmpty(provenance.ContractId, provenance.ArtifactKind)} / {provenance.ArtifactId} / {provenance.MappingResult}"));

        if (model.Warnings.Count > 0)
        {
            AppendSection(builder, "Warnings");
            AppendItems(builder, model.Warnings);
        }

        return builder.ToString();
    }

    private static void AppendGeneratedContentItems(
        StringBuilder builder,
        string title,
        IReadOnlyList<GeneratedPackageRuntimePreviewContentItem> items)
    {
        AppendSection(builder, $"{title} ({items.Count})");
        AppendItems(builder, items.Select(item =>
        {
            var references = item.References.Count == 0 ? string.Empty : $" [{string.Join(", ", item.References)}]";
            return $"{FirstNonEmpty(item.Title, item.SourceId)}: {item.Description}{references}";
        }));
    }

    private static void AppendSection(StringBuilder builder, string title)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }

        builder.AppendLine(title + ":");
    }

    private static void AppendLine(StringBuilder builder, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.AppendLine($"{label}: {value}");
        }
    }

    private static void AppendItems(StringBuilder builder, IEnumerable<string> items)
    {
        var appended = false;
        foreach (var item in items.Where(item => !string.IsNullOrWhiteSpace(item)))
        {
            builder.AppendLine("- " + item.Trim());
            appended = true;
        }

        if (!appended)
        {
            builder.AppendLine("(none)");
        }
    }

    private static string JoinNonEmpty(params string[] values)
    {
        return string.Join(" / ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }
}
