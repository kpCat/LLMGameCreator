using LLMGameCreator.Application.Play.GeneratedCampaign;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GeneratedCampaignPageControl : UserControl, IEditorPage
{
    private readonly GeneratedCampaignSessionService? _service;
    private GeneratedCampaignSnapshot? _snapshot;

    public GeneratedCampaignPageControl()
    {
        InitializeComponent();
    }

    public GeneratedCampaignPageControl(GeneratedCampaignSessionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        InitializeComponent();
        WireEvents();
    }

    public string Id => "generated-campaign-player";
    public string Title => "Играть";
    public int SortOrder => 54;
    Control IEditorPage.View => this;

    internal string ContextTitleText => _contextTitle.Text;
    internal IReadOnlyList<string> VisibleActionTitles => _actions.Controls.OfType<Button>()
        .Select(button => button.Text).ToList();
    internal bool TechnicalDetailsVisible => _technical.Visible;
    internal string ConsequenceText => _consequencesTab.Controls.OfType<Label>()
        .FirstOrDefault()?.Text ?? string.Empty;

    public void OnActivated() => Bind(_service?.Refresh());

    private void WireEvents()
    {
        _newGame.Click += NewGameClick;
        _save.Click += SaveClick;
        _continue.Click += ContinueClick;
        _technicalToggle.CheckedChanged += TechnicalToggleChanged;
        _map.CellClicked += MapCellClicked;
    }

    private void NewGameClick(object? sender, EventArgs eventArgs) => Bind(_service?.StartNew());

    private void SaveClick(object? sender, EventArgs eventArgs) => Bind(_service?.Save(_slot.Text));

    private void ContinueClick(object? sender, EventArgs eventArgs)
    {
        if (_service is null) return;
        var saves = _service.ListSaves();
        using var dialog = new GeneratedCampaignSavePickerDialog(saves.Entries);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedEntry is null) return;
        if (dialog.MigrateRequested)
        {
            var preview = _service.PreviewMigration(dialog.SelectedEntry.SlotName);
            Bind(_service.MigrateAndContinue(preview));
            return;
        }

        Bind(_service.Continue(dialog.SelectedEntry.SlotName));
    }

    private void TechnicalToggleChanged(object? sender, EventArgs eventArgs)
    {
        _technical.Visible = _technicalToggle.Checked;
        _rootLayout.RowStyles[2].Height = _technicalToggle.Checked ? 120F : 0F;
    }

    private void MapCellClicked(object? sender, (int X, int Y) cell)
    {
        if (_service is null || _snapshot?.Map is null
            || _snapshot.Status == GeneratedCampaignSessionStatus.DEFEATED) return;
        var player = _snapshot.Map.Cells.SingleOrDefault(item => item.PlayerPresent);
        if (player is null || Math.Abs(player.X - cell.X) + Math.Abs(player.Y - cell.Y) != 1) return;
        var kind = cell.X > player.X
            ? GeneratedCampaignActionKind.MoveRight
            : cell.X < player.X
                ? GeneratedCampaignActionKind.MoveLeft
                : cell.Y > player.Y
                    ? GeneratedCampaignActionKind.MoveDown
                    : GeneratedCampaignActionKind.MoveUp;
        ExecuteFirst(kind);
    }

    protected override bool ProcessCmdKey(ref Message message, Keys keyData)
    {
        if (!ContainsFocus) return base.ProcessCmdKey(ref message, keyData);
        var focused = FindFocusedControl(this);
        if (focused is TextBoxBase or ComboBox || focused is Button)
            return base.ProcessCmdKey(ref message, keyData);
        var kind = keyData switch
        {
            Keys.W or Keys.Up => GeneratedCampaignActionKind.MoveUp,
            Keys.S or Keys.Down => GeneratedCampaignActionKind.MoveDown,
            Keys.A or Keys.Left => GeneratedCampaignActionKind.MoveLeft,
            Keys.D or Keys.Right => GeneratedCampaignActionKind.MoveRight,
            Keys.E or Keys.Enter => GeneratedCampaignActionKind.Interact,
            _ => (GeneratedCampaignActionKind)(-1)
        };
        if (_snapshot?.Status == GeneratedCampaignSessionStatus.DEFEATED
            && kind is GeneratedCampaignActionKind.MoveUp
                or GeneratedCampaignActionKind.MoveDown
                or GeneratedCampaignActionKind.MoveLeft
                or GeneratedCampaignActionKind.MoveRight
                or GeneratedCampaignActionKind.Interact)
            return true;
        return ExecuteFirst(kind) || base.ProcessCmdKey(ref message, keyData);
    }

    private bool ExecuteFirst(GeneratedCampaignActionKind kind)
    {
        if (_service is null || _snapshot is null) return false;
        var action = _snapshot.Actions.FirstOrDefault(item => item.Kind == kind && item.Enabled);
        if (action is null) return false;
        Bind(_service.Execute(action.ActionId));
        return true;
    }

    private void Bind(GeneratedCampaignSnapshot? snapshot)
    {
        if (snapshot is null) return;
        _snapshot = snapshot;
        _status.Text = string.Join(" — ", new[]
        {
            snapshot.ProjectTitle,
            snapshot.CurrentRegionTitle,
            snapshot.StatusTitle
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        _mapTitle.Text = string.IsNullOrWhiteSpace(snapshot.CurrentMapTitle)
            ? "Карта"
            : snapshot.CurrentMapTitle;
        _map.Projection = snapshot.Map;
        _save.Enabled = snapshot.Status == GeneratedCampaignSessionStatus.ACTIVE;
        _slot.Enabled = snapshot.Status == GeneratedCampaignSessionStatus.ACTIVE;
        _newGame.Enabled = snapshot.Status is GeneratedCampaignSessionStatus.READY
            or GeneratedCampaignSessionStatus.ACTIVE
            or GeneratedCampaignSessionStatus.DEFEATED
            or GeneratedCampaignSessionStatus.STALE_PROJECT;
        _continue.Enabled = snapshot.Status is not GeneratedCampaignSessionStatus.NO_PROJECT
            and not GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED;
        BindContext(snapshot);
        BindActions(snapshot.Actions);
        WriteTab(_characterTab, snapshot.Resources.Concat(snapshot.Stats).Concat(snapshot.Progressions));
        WriteTab(_questsTab, snapshot.Quests.Select(QuestRow));
        WriteTab(_inventoryTab, snapshot.Inventory.Concat(snapshot.Equipment).Concat(snapshot.Factions));
        WriteTab(_consequencesTab, ConsequenceRows(snapshot));
        WriteTab(_eventsTab, snapshot.RecentEvents.Select(value => new GeneratedCampaignTextRow { Title = value }));
        _technical.Text = string.Join(Environment.NewLine,
            snapshot.TechnicalDetails.Select(item => item.Key + ": " + item.Value)
                .Concat(snapshot.Diagnostics));
    }

    private void BindContext(GeneratedCampaignSnapshot snapshot)
    {
        if (snapshot.Status == GeneratedCampaignSessionStatus.DEFEATED && snapshot.Recovery.Available)
        {
            _contextTitle.Text = "Поражение";
            _contextDescription.Text = string.IsNullOrWhiteSpace(snapshot.Recovery.EncounterTitle)
                ? "Выберите способ продолжить кампанию."
                : snapshot.Recovery.EncounterTitle + Environment.NewLine + Environment.NewLine
                  + "Выберите способ продолжить кампанию.";
            return;
        }

        if (snapshot.Dialogue is { Open: true } dialogue)
        {
            _contextTitle.Text = dialogue.Title;
            _contextDescription.Text = dialogue.Speaker + Environment.NewLine + Environment.NewLine + dialogue.Text;
            return;
        }

        if (snapshot.Encounter is { } encounter)
        {
            _contextTitle.Text = encounter.Title;
            var participants = encounter.Participants.Select(participant =>
                participant.Title + " — " + participant.TeamTitle
                + (participant.Alive ? string.Empty : " — побеждён")
                + (participant.CurrentTurn ? " — текущий ход" : string.Empty));
            _contextDescription.Text = "Раунд " + encounter.Round
                                       + (string.IsNullOrWhiteSpace(encounter.CurrentTurnTitle)
                                           ? string.Empty
                                           : ". Ход: " + encounter.CurrentTurnTitle)
                                       + Environment.NewLine + Environment.NewLine
                                       + string.Join(Environment.NewLine, participants);
            return;
        }

        if (snapshot.Nearby.Count > 0)
        {
            _contextTitle.Text = "Рядом";
            _contextDescription.Text = string.Join(Environment.NewLine + Environment.NewLine,
                snapshot.Nearby.Select(item => item.Title + Environment.NewLine + item.Description));
            return;
        }

        _contextTitle.Text = snapshot.Status == GeneratedCampaignSessionStatus.ACTIVE
            ? "Исследование"
            : snapshot.StatusTitle;
        _contextDescription.Text = snapshot.Status == GeneratedCampaignSessionStatus.ACTIVE
            ? "Используйте карту, кнопки действий или клавиши WASD/стрелки. Клавиша E выполняет доступное взаимодействие."
            : snapshot.StatusDescription;
    }

    private void BindActions(IEnumerable<GeneratedCampaignAction> actions)
    {
        _actions.SuspendLayout();
        _actions.Controls.Clear();
        foreach (var action in actions)
        {
            var button = new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Enabled = action.Enabled,
                Margin = new Padding(3, 3, 3, 7),
                MinimumSize = new Size(220, 36),
                Tag = action.ActionId,
                Text = action.Title,
                UseVisualStyleBackColor = true
            };
            button.Click += ActionClick;
            var tooltip = action.Tactical is null
                ? action.Description
                : string.Join(Environment.NewLine, new[]
                {
                    action.Tactical.TargetTitle,
                    action.Tactical.CostSummary,
                    action.Tactical.EffectSummary,
                    action.Tactical.AvailabilitySummary,
                    action.Tactical.ProgressesEncounter ? "Продвигает встречу" : "Поддерживающее действие"
                }.Where(value => !string.IsNullOrWhiteSpace(value)));
            if (!action.Enabled && !string.IsNullOrWhiteSpace(action.DisabledReason))
                tooltip += " — " + action.DisabledReason;
            _actionToolTip.SetToolTip(button, tooltip);
            _actions.Controls.Add(button);
        }

        _actions.ResumeLayout();
    }

    private void ActionClick(object? sender, EventArgs eventArgs)
    {
        if (_service is null || sender is not Button { Tag: string actionId }) return;
        Bind(_service.Execute(actionId));
    }

    private static GeneratedCampaignTextRow QuestRow(GeneratedCampaignQuest quest) => new()
    {
        Title = quest.Title,
        Value = quest.StateTitle + (quest.Objectives.Count == 0
            ? string.Empty
            : Environment.NewLine + string.Join(Environment.NewLine,
                quest.Objectives.Select(objective => "• " + objective.Title + ": " + objective.Progress)))
    };

    private static IEnumerable<GeneratedCampaignTextRow> ConsequenceRows(
        GeneratedCampaignSnapshot snapshot)
    {
        if (snapshot.LastActionOutcome is not null)
        {
            yield return new GeneratedCampaignTextRow
            {
                Title = snapshot.LastActionOutcome.ActionTitle,
                Value = snapshot.LastActionOutcome.Summary
            };
        }
        foreach (var consequence in snapshot.Consequences)
        {
            var transition = string.IsNullOrWhiteSpace(consequence.BeforeValue)
                             && string.IsNullOrWhiteSpace(consequence.AfterValue)
                ? string.Empty
                : consequence.BeforeValue + " → " + consequence.AfterValue;
            var value = string.Join("; ", new[] { transition, consequence.Delta, consequence.Description }
                .Where(item => !string.IsNullOrWhiteSpace(item)));
            yield return new GeneratedCampaignTextRow
            {
                Title = consequence.Title,
                Value = value
            };
        }
    }

    private static void WriteTab(TabPage tab, IEnumerable<GeneratedCampaignTextRow> rows)
    {
        tab.Controls.Clear();
        var text = string.Join(Environment.NewLine + Environment.NewLine,
            rows.Select(row => string.IsNullOrWhiteSpace(row.Value)
                ? row.Title
                : row.Title + ": " + row.Value));
        tab.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(8, 8),
            MaximumSize = new Size(330, 0),
            Text = string.IsNullOrWhiteSpace(text) ? "Пока нет данных." : text
        });
    }

    private static Control? FindFocusedControl(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (child.Focused) return child;
            if (child.ContainsFocus) return FindFocusedControl(child) ?? child;
        }

        return null;
    }
}
