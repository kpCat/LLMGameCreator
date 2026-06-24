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
    private readonly OneClickGeneratedPreviewWorkflowService? _generatePreviewWorkflowService;
    private readonly GeneratedPackageRuntimePreviewService _previewService;
    private readonly GeneratedContentInteractionPreviewService _interactionService;
    private readonly GeneratedQuestDialoguePreviewService _questDialoguePreviewService;
    private readonly GeneratedMicrogameGoalPreviewService _microgameGoalPreviewService;
    private readonly GeneratedMicrogameChallengePreviewService _microgameChallengePreviewService;
    private readonly GeneratedMapPlacementPreviewService _mapPlacementPreviewService;
    private GeneratedContentInteractionCatalog _interactionCatalog = new();
    private GeneratedMicrogameChallengePreviewModel _microgameChallengeModel = new();
    private GeneratedMapPlacementPreviewModel _mapPlacementModel = new();
    private GameState? _state;
    private bool _splitterInitialized;
    private bool _updatingGeneratedSelection;
    private bool _generatePreviewRunning;

    public RuntimePreviewPageControl()
    {
        _previewService = new GeneratedPackageRuntimePreviewService();
        _generatePreviewWorkflowService = new OneClickGeneratedPreviewWorkflowService();
        _interactionService = new GeneratedContentInteractionPreviewService();
        _questDialoguePreviewService = new GeneratedQuestDialoguePreviewService();
        _microgameGoalPreviewService = new GeneratedMicrogameGoalPreviewService();
        _microgameChallengePreviewService = new GeneratedMicrogameChallengePreviewService();
        _mapPlacementPreviewService = new GeneratedMapPlacementPreviewService();
        InitializeComponent();
        ConfigureSplitSafety();
        WireGeneratedContentEvents();
        RefreshGeneratedPreview(null, null);
    }

    public RuntimePreviewPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IGameRuntime runtime,
        OneClickGeneratedPreviewWorkflowService generatePreviewWorkflowService,
        GeneratedPackageRuntimePreviewService previewService,
        GeneratedContentInteractionPreviewService interactionService,
        GeneratedQuestDialoguePreviewService questDialoguePreviewService,
        GeneratedMicrogameGoalPreviewService microgameGoalPreviewService,
        GeneratedMicrogameChallengePreviewService microgameChallengePreviewService,
        GeneratedMapPlacementPreviewService mapPlacementPreviewService)
    {
        _currentGamePackageService = currentGamePackageService;
        _runtime = runtime;
        _generatePreviewWorkflowService = generatePreviewWorkflowService;
        _previewService = previewService;
        _interactionService = interactionService;
        _questDialoguePreviewService = questDialoguePreviewService;
        _microgameGoalPreviewService = microgameGoalPreviewService;
        _microgameChallengePreviewService = microgameChallengePreviewService;
        _mapPlacementPreviewService = mapPlacementPreviewService;
        InitializeComponent();
        ConfigureSplitSafety();
        WireGeneratedContentEvents();
        _generatePreviewButton.Click += async (_, _) => await GeneratePreviewAsync();
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
        _questDialoguePreviewService.StartSession(package);
        _microgameChallengeModel = new GeneratedMicrogameChallengePreviewModel();
        ApplyResult(package, result);
        AppendGeneratedStartSummary(package, result.State);
    }

    private async Task GeneratePreviewAsync()
    {
        if (_generatePreviewWorkflowService == null || _generatePreviewRunning)
        {
            return;
        }

        SetGeneratePreviewRunning(true);
        AppendLog("Generate Preview: running deterministic S029-S033 workflow...");
        try
        {
            var projectRootPath = _currentGamePackageService?.CurrentFolder ?? string.Empty;
            var result = await _generatePreviewWorkflowService.ExecuteAsync(new OneClickGeneratedPreviewWorkflowRequest
            {
                ProjectRootPath = projectRootPath,
                ReplaceCurrentPackage = false
            });

            AppendWorkflowDiagnostics(result.Diagnostics);
            if (!result.Ok)
            {
                AppendLog($"Generate Preview failed: {result.Status}");
                return;
            }

            var currentPackageReplaced = false;
            RunOnUiThread(() =>
            {
                _currentGamePackageService?.ReplaceCurrent(result.GeneratedPackage);
                currentPackageReplaced = _currentGamePackageService != null;
                _state = null;
                _questDialoguePreviewService.StartSession(result.GeneratedPackage);
                RefreshGeneratedPreview(result.GeneratedPackage, null);
                _rightTabControl.SelectedTab = _generatedContentTabPage;
                _generatedContentInnerTabControl.SelectedTab = _generatedSummaryTabPage;
            });

            AppendLog($"Generate Preview ready: {result.PackageTitle} / {result.PackageId}");
            AppendLog($"Output folder: {result.Paths.VisiblePreviewOutputDirectoryPath}");
            AppendLog($"Generated package: {result.Paths.GeneratedPackageJsonPath}");
            AppendLog($"Snapshot: {result.Paths.VisiblePreviewSnapshotJsonPath}");
            AppendLog(currentPackageReplaced
                ? "Generated package loaded as current package. Press Start to run Runtime Preview."
                : "Generated package was not loaded into current package state; inspect the output folder.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            AppendLog($"Generate Preview failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            SetGeneratePreviewRunning(false);
        }
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
        if (command.Type == PlayerCommandType.Interact && result.Events.Any(item => item.Type == RuntimeEventType.InteractionTriggered))
        {
            var goal = AdvanceGeneratedGoalProgress(package, result);
            var preview = _previewService.Build(package, result.State);
            var selectedChallenge = _microgameChallengeModel.ChallengeSelected
                ? _microgameChallengeModel
                : _microgameChallengePreviewService.SelectChallenge(package, preview, goal);
            _microgameChallengeModel = _microgameChallengePreviewService.ResolveAfterInteraction(package, preview, goal, selectedChallenge);
            if (_microgameChallengeModel.Resolved)
            {
                AppendLog($"Challenge resolved: {_microgameChallengeModel.EncounterTitle} / reward: {_microgameChallengeModel.RewardTitle} / completion: {_microgameChallengeModel.CompletionStatus}");
            }
        }

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

    private void AppendWorkflowDiagnostics(IReadOnlyList<OneClickGeneratedPreviewWorkflowDiagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            AppendLog($"{diagnostic.Severity}: {diagnostic.Code} / {diagnostic.Target} / {diagnostic.Message}");
        }
    }

    private GeneratedMicrogameGoalPreviewModel AdvanceGeneratedGoalProgress(GamePackageDefinition package, CommandResult result)
    {
        var preview = _previewService.Build(package, result.State);
        var goal = _microgameGoalPreviewService.AdvanceAfterInteraction(package, preview, _questDialoguePreviewService, result);
        if (goal.ProgressAdvancedByInteraction)
        {
            AppendLog($"Active goal progress: {goal.ActiveQuestTitle} / {goal.CompletedStepCount} of {goal.StepCount} / {goal.ProgressStatus}");
        }

        return goal;
    }

    private void SetGeneratePreviewRunning(bool running)
    {
        _generatePreviewRunning = running;
        _generatePreviewButton.Enabled = !running;
        _startButton.Enabled = !running;
    }

    private void RunOnUiThread(Action action)
    {
        if (InvokeRequired)
        {
            Invoke(action);
            return;
        }

        action();
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
        _previewDialogueButton.Click += (_, _) => PreviewSelectedDialogue();
        _startQuestPreviewButton.Click += (_, _) => StartSelectedQuestPreview();
        _markNextQuestStepButton.Click += (_, _) => MarkNextSelectedQuestStep();
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
            _mapPlacementModel = new GeneratedMapPlacementPreviewModel();
            _canvas.SetGeneratedMarkers(_mapPlacementModel.Markers);
            _generatedContentTextBox.Text = "No package is running.";
            _questJournalTextBox.Text = "No package is running.";
            ApplyInteractionCatalog(new GeneratedContentInteractionCatalog(), null, null);
            return;
        }

        var model = _previewService.Build(package, state);
        var goal = state == null
            ? new GeneratedMicrogameGoalPreviewModel()
            : _microgameGoalPreviewService.EnsureActiveGoal(package, model, _questDialoguePreviewService);
        var challenge = state == null
            ? new GeneratedMicrogameChallengePreviewModel()
            : _microgameChallengeModel.ChallengeSelected
                ? _microgameChallengeModel
                : _microgameChallengePreviewService.SelectChallenge(package, model, goal);
        if (state != null)
        {
            _mapPlacementModel = _mapPlacementPreviewService.Build(package, state, model);
            _canvas.SetGeneratedMarkers(_mapPlacementModel.Markers);
        }
        _generatedContentTextBox.Text = FormatGeneratedPreview(model, goal, challenge);
        RefreshQuestJournal(goal, challenge);
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
        var marker = FindMarker(entry);
        _generatedDetailsTextBox.Text = entry == null
            ? "No generated entry selected."
            : marker == null
                ? entry.DetailsText
                : entry.DetailsText + Environment.NewLine + Environment.NewLine + "Map marker:" + Environment.NewLine + marker.DetailsText;
        _appendGeneratedSelectionButton.Enabled = entry != null;
        _previewDialogueButton.Enabled = entry?.CategoryId == "dialogues";
        _startQuestPreviewButton.Enabled = entry?.CategoryId == "quests";
        _markNextQuestStepButton.Enabled = entry?.CategoryId == "quests";
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
        var marker = FindMarker(entry);
        if (marker != null)
        {
            AppendLog($"Map marker: {marker.Type} / {marker.MapId} / {marker.Position.X}, {marker.Position.Y}");
            AppendLog(marker.DetailsText.Replace(Environment.NewLine, "; "));
        }
    }

    private GeneratedRuntimeMapMarker? FindMarker(GeneratedContentInteractionEntry? entry)
    {
        if (entry == null || entry.CategoryId is not ("npcs" or "encounters"))
        {
            return null;
        }

        return _mapPlacementModel.Markers.FirstOrDefault(marker =>
            string.Equals(marker.SourceId, entry.EntryId, StringComparison.OrdinalIgnoreCase));
    }

    private void PreviewSelectedDialogue()
    {
        if (_generatedEntriesListBox.SelectedItem is not GeneratedContentInteractionEntry { CategoryId: "dialogues" } entry)
        {
            return;
        }

        var result = _questDialoguePreviewService.PreviewDialogue(entry.EntryId);
        if (!result.Ok)
        {
            AppendLog($"Dialogue preview failed: {result.Status} / {entry.EntryId}");
            return;
        }

        AppendLog($"Dialogue preview: {FirstNonEmpty(result.Title, result.DialogueId)}");
        foreach (var line in result.Lines)
        {
            AppendLog(line);
        }
    }

    private void StartSelectedQuestPreview()
    {
        if (_generatedEntriesListBox.SelectedItem is not GeneratedContentInteractionEntry { CategoryId: "quests" } entry)
        {
            return;
        }

        var result = _questDialoguePreviewService.StartQuest(entry.EntryId);
        AppendLog(result.Ok
            ? $"Quest preview started: {entry.Title}"
            : $"Quest preview failed: {result.Status} / {entry.EntryId}");
        RefreshQuestJournal();
    }

    private void MarkNextSelectedQuestStep()
    {
        if (_generatedEntriesListBox.SelectedItem is not GeneratedContentInteractionEntry { CategoryId: "quests" } entry)
        {
            return;
        }

        var result = _questDialoguePreviewService.MarkNextStep(entry.EntryId);
        AppendLog(result.Ok
            ? $"Quest preview step: {entry.Title} / {result.CompletedStepCount} of {result.StepCount} / {result.QuestStatus}"
            : $"Quest preview step failed: {result.Status} / {entry.EntryId}");
        RefreshQuestJournal();
    }

    private void RefreshQuestJournal(
        GeneratedMicrogameGoalPreviewModel? goal = null,
        GeneratedMicrogameChallengePreviewModel? challenge = null)
    {
        if (_state == null)
        {
            _questJournalTextBox.Text = "No package is running.";
            return;
        }

        var journal = _questDialoguePreviewService.BuildJournal();
        var builder = new StringBuilder();
        if (goal?.ActiveGoalSelected == true)
        {
            builder.AppendLine($"Active goal: {goal.ActiveQuestTitle}");
            builder.AppendLine($"Current objective: {goal.CurrentObjectiveText}");
            builder.AppendLine($"Progress: {goal.CompletedStepCount}/{goal.StepCount} [{goal.ProgressStatus}]");
            if (!string.IsNullOrWhiteSpace(goal.Related.NpcTitle) || !string.IsNullOrWhiteSpace(goal.Related.ItemTitle) || !string.IsNullOrWhiteSpace(goal.Related.EncounterTitle))
            {
                builder.AppendLine($"Related: {JoinNonEmpty(goal.Related.NpcTitle, goal.Related.ItemTitle, goal.Related.EncounterTitle)}");
            }

            if (!string.IsNullOrWhiteSpace(goal.LastProgressAction))
            {
                builder.AppendLine($"Last progress action: {goal.LastProgressAction}");
            }

            if (challenge?.ChallengeSelected == true)
            {
                builder.AppendLine($"Challenge: {challenge.EncounterTitle}");
                builder.AppendLine($"Reward: {challenge.RewardTitle}");
                builder.AppendLine($"Resolved/completion: {challenge.Resolved} / {challenge.CompletionVisible} [{challenge.CompletionStatus}]");
            }

            builder.AppendLine();
        }

        builder.AppendLine($"Available quests: {journal.AvailableCount}");
        builder.AppendLine($"Active preview quests: {journal.ActiveCount}");
        builder.AppendLine($"Completed preview quests: {journal.CompletedCount}");
        foreach (var quest in journal.Entries)
        {
            builder.AppendLine();
            builder.AppendLine($"{FirstNonEmpty(quest.Title, quest.QuestId)} [{quest.Status}]");
            builder.AppendLine($"Steps: {quest.CompletedStepCount}/{quest.StepCount}");
            if (!string.IsNullOrWhiteSpace(quest.CurrentStep))
            {
                builder.AppendLine($"Current/next step: {quest.CurrentStep}");
            }
        }

        _questJournalTextBox.Text = builder.ToString().TrimEnd();
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

    private static string FormatGeneratedPreview(
        GeneratedPackageRuntimePreviewModel model,
        GeneratedMicrogameGoalPreviewModel? goal = null,
        GeneratedMicrogameChallengePreviewModel? challenge = null)
    {
        var builder = new StringBuilder();
        AppendSection(builder, "Package");
        AppendLine(builder, "Title", FirstNonEmpty(model.PackageTitle, "(untitled)"));
        AppendLine(builder, "Description", model.PackageDescription);
        AppendLine(builder, "Map", JoinNonEmpty(model.CurrentMapId, model.CurrentMapName));

        AppendSection(builder, "Generated counts");
        AppendLine(builder, "Regions", model.Regions.Count.ToString());
        AppendLine(builder, "NPCs", model.Npcs.Count.ToString());
        AppendLine(builder, "Items", model.Items.Count.ToString());
        AppendLine(builder, "Dialogues", model.Dialogues.Count.ToString());
        AppendLine(builder, "Encounters", model.Encounters.Count.ToString());
        AppendLine(builder, "Quests", model.Quests.Count.ToString());
        AppendLine(builder, "Active goals", goal?.ActiveGoalSelected == true ? "1" : "0");
        AppendLine(builder, "Goal progress", goal?.ActiveGoalSelected == true ? $"{goal.CompletedStepCount}/{goal.StepCount}" : "0/0");
        AppendLine(builder, "Resolved challenges", challenge?.Resolved == true ? "1" : "0");
        AppendLine(builder, "Visible rewards", challenge?.RewardVisible == true ? "1" : "0");
        AppendLine(builder, "Visible completions", challenge?.CompletionVisible == true ? "1" : "0");
        AppendLine(builder, "Mechanics", model.Mechanics.Count.ToString());
        AppendLine(builder, "Applied artifacts", model.Provenance.Count.ToString());

        AppendSection(builder, "Active goal");
        if (goal?.ActiveGoalSelected == true)
        {
            AppendLine(builder, "Quest", FirstNonEmpty(goal.ActiveQuestTitle, goal.ActiveQuestId));
            AppendLine(builder, "Objective", goal.CurrentObjectiveText);
            AppendLine(builder, "Progress", $"{goal.CompletedStepCount}/{goal.StepCount} [{goal.ProgressStatus}]");
            AppendLine(builder, "Related NPC", FirstNonEmpty(goal.Related.NpcTitle, goal.Related.NpcId));
            AppendLine(builder, "Related item", FirstNonEmpty(goal.Related.ItemTitle, goal.Related.ItemId));
            AppendLine(builder, "Related encounter", FirstNonEmpty(goal.Related.EncounterTitle, goal.Related.EncounterId));
            AppendLine(builder, "Last action", goal.LastProgressAction);
        }
        else
        {
            builder.AppendLine("(none)");
        }

        AppendSection(builder, "Challenge");
        if (challenge?.ChallengeSelected == true)
        {
            AppendLine(builder, "Encounter", FirstNonEmpty(challenge.EncounterTitle, challenge.EncounterId));
            AppendLine(builder, "Related quest", FirstNonEmpty(challenge.QuestTitle, challenge.QuestId));
            AppendLine(builder, "Reward", FirstNonEmpty(challenge.RewardTitle, challenge.RewardItemId));
            AppendLine(builder, "Resolved", challenge.Resolved.ToString());
            AppendLine(builder, "Completion", $"{challenge.CompletionVisible} [{challenge.CompletionStatus}]");
            AppendLine(builder, "Resolve action", challenge.ResolveAction);
        }
        else
        {
            builder.AppendLine("(none)");
        }

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

        AppendSection(builder, "Representative ids");
        AppendRepresentativeIds(builder, "Regions", model.Regions.Select(item => item.SourceId));
        AppendRepresentativeIds(builder, "NPCs", model.Npcs.Select(item => item.SourceId));
        AppendRepresentativeIds(builder, "Items", model.Items.Select(item => item.SourceId));
        AppendRepresentativeIds(builder, "Encounters", model.Encounters.Select(item => item.SourceId));
        AppendRepresentativeIds(builder, "Quests", model.Quests.Select(item => item.SourceId));
        AppendRepresentativeIds(builder, "Mechanics", model.Mechanics.Select(item => item.SourceId));

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

    private static void AppendRepresentativeIds(StringBuilder builder, string label, IEnumerable<string> ids)
    {
        var values = ids
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .OrderBy(value => value, StringComparer.Ordinal)
            .Take(5)
            .ToArray();
        AppendLine(builder, label, values.Length == 0 ? "(none)" : string.Join(", ", values));
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
