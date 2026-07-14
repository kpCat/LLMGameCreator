using System.Globalization;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ProjectsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IAppSettingsRepository? _settingsRepository;
    private readonly IGameProjectService? _gameProjectService;
    private readonly IGamePackageValidator? _validator;
    private readonly IUnifiedGameProjectWorkspaceController? _workspaceController;
    private readonly ToolTip _workspaceToolTip = new();
    private AppSettings? _settings;
    private bool _workspaceBinding;
    private bool _buildUiRunning;

    public ProjectsPageControl()
    {
        InitializeComponent();
        _infoTextBox.Text = "Предварительный просмотр. Сервисы приложения недоступны в дизайнере.";
    }

    public ProjectsPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IAppSettingsRepository settingsRepository,
        IGameProjectService gameProjectService,
        IGamePackageValidator validator)
        : this(currentGamePackageService, settingsRepository, gameProjectService, validator, null)
    {
    }

    public ProjectsPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IAppSettingsRepository settingsRepository,
        IGameProjectService gameProjectService,
        IGamePackageValidator validator,
        IUnifiedGameProjectWorkspaceController? workspaceController)
    {
        _currentGamePackageService = currentGamePackageService;
        _settingsRepository = settingsRepository;
        _gameProjectService = gameProjectService;
        _validator = validator;
        _workspaceController = workspaceController;
        InitializeComponent();
        WireEvents();
    }

    public string Id => "projects";
    public string Title => "Игры";
    public int SortOrder => 10;
    Control IEditorPage.View => this;

    public async void OnActivated()
    {
        await LoadSettingsAndRefreshAsync();
    }

    private void WireEvents()
    {
        _browseGamesRootButton.Click += (_, _) => BrowseGamesRoot();
        _saveGamesRootButton.Click += async (_, _) => await SaveGamesRootAsync();
        _refreshButton.Click += async (_, _) => await RefreshProjectsListAsync();
        _newGameButton.Click += async (_, _) => await CreateNewGameAsync();
        _openSelectedButton.Click += async (_, _) => await OpenSelectedProjectAsync();
        _openFolderButton.Click += async (_, _) => await OpenArbitraryFolderAsync();
        _saveCurrentButton.Click += async (_, _) => await SaveCurrentGameAsync();
        _backToGamesButton.Click += (_, _) => ShowProjectStart();
        _buildAndQualifyButton.Click += async (_, _) => await BuildAndQualifyAsync();
        _buildWindowsStandaloneButton.Click += async (_, _) => await BuildWindowsStandaloneAsync();
        _cancelWindowsStandaloneButton.Click += (_, _) => _workspaceController?.CancelWindowsStandaloneBuild();
        _launchWindowsStandaloneButton.Click += (_, _) => LaunchWindowsStandalone();
        _openWindowsStandaloneFolderButton.Click += (_, _) => OpenWindowsStandaloneFolder();
        _findUnityEditorButton.Click += (_, _) => FindUnityEditor();
        _chooseUnityEditorButton.Click += (_, _) => ChooseUnityEditor();
        _projectsListView.DoubleClick += async (_, _) => await OpenSelectedProjectAsync();
    }

    private async Task LoadSettingsAndRefreshAsync()
    {
        if (_settingsRepository == null) return;
        _settings = await _settingsRepository.LoadAsync(CancellationToken.None).ConfigureAwait(true);
        _gamesRootTextBox.Text = _settings.GamesRootPath;
        await RefreshProjectsListAsync();
        RefreshInfo();
    }

    private void BrowseGamesRoot()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Выбери корневую папку, внутри которой будут лежать папки отдельных игр.",
            SelectedPath = Directory.Exists(_gamesRootTextBox.Text) ? _gamesRootTextBox.Text : string.Empty
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) _gamesRootTextBox.Text = dialog.SelectedPath;
    }

    private async Task SaveGamesRootAsync()
    {
        if (_settingsRepository == null) return;
        _settings ??= await _settingsRepository.LoadAsync(CancellationToken.None).ConfigureAwait(true);
        _settings.GamesRootPath = _gamesRootTextBox.Text.Trim();
        await _settingsRepository.SaveAsync(_settings, CancellationToken.None).ConfigureAwait(true);
        await RefreshProjectsListAsync();
        RefreshInfo();
    }

    private async Task RefreshProjectsListAsync()
    {
        _projectsListView.Items.Clear();
        var root = _gamesRootTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(root))
        {
            _infoTextBox.Text = "Корневая папка игр не указана.";
            return;
        }
        if (_gameProjectService == null)
        {
            _infoTextBox.Text = "Сервис игровых проектов недоступен.";
            return;
        }

        try
        {
            var summaries = await _gameProjectService.ListAsync(root, CancellationToken.None).ConfigureAwait(true);
            foreach (var summary in summaries)
            {
                var item = new ListViewItem(string.IsNullOrWhiteSpace(summary.Title) ? summary.FolderName : summary.Title);
                item.SubItems.Add(summary.PackageId ?? string.Empty);
                item.SubItems.Add(summary.Version ?? string.Empty);
                item.SubItems.Add(GetStatusText(summary));
                item.SubItems.Add(summary.FolderPath);
                item.Tag = summary;
                _projectsListView.Items.Add(item);
            }
        }
        catch (Exception exception)
        {
            _infoTextBox.Text = "Не удалось обновить список игр:" + Environment.NewLine + exception.Message;
            return;
        }
        RefreshInfo();
    }

    private static string GetStatusText(GameProjectSummary summary)
    {
        if (!summary.HasPackageFile) return "Нет package.json";
        if (!summary.IsValidPackage) return "Есть ошибки";
        if (summary.WarningCount > 0) return "Готово, предупреждений: " + summary.WarningCount;
        return "Готово";
    }

    private async Task OpenSelectedProjectAsync()
    {
        if (_projectsListView.SelectedItems.Count == 0)
        {
            MessageBox.Show(this, "Выбери игру в списке.", "Игра не выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_projectsListView.SelectedItems[0].Tag is GameProjectSummary summary)
            await LoadProjectFolderAsync(summary.FolderPath);
    }

    private async Task OpenArbitraryFolderAsync()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Выбери папку конкретной игры, где лежит package.json.",
            SelectedPath = Directory.Exists(_gamesRootTextBox.Text) ? _gamesRootTextBox.Text : string.Empty
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) await LoadProjectFolderAsync(dialog.SelectedPath);
    }

    private async Task CreateNewGameAsync()
    {
        if (_gameProjectService == null) return;
        var root = _gamesRootTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(root))
        {
            MessageBox.Show(this, "Сначала укажи корневую папку игр.", "Новая игра", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new CreateGameDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var summary = await _gameProjectService.CreateAsync(dialog.CreateRequest(root), CancellationToken.None).ConfigureAwait(true);
            await RefreshProjectsListAsync();
            await LoadProjectFolderAsync(summary.FolderPath);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Не удалось создать игру", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveCurrentGameAsync()
    {
        if (_currentGamePackageService == null) return;
        try
        {
            if (_workspaceController?.HasOpenProject == true) _workspaceController.SaveAuthoring();
            await _currentGamePackageService.SaveAsync(CancellationToken.None).ConfigureAwait(true);
            if (_workspaceController?.HasOpenProject == true) BindWorkspace(_workspaceController.Snapshot());
            MessageBox.Show(this, "Проект сохранён.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadProjectFolderAsync(string folder)
    {
        if (_currentGamePackageService == null) return;
        try
        {
            await _currentGamePackageService.LoadAsync(folder, CancellationToken.None).ConfigureAwait(true);
            if (_workspaceController == null)
                throw new InvalidOperationException("Рабочая область проекта недоступна.");
            BindWorkspace(_workspaceController.OpenProject(folder));
            ShowWorkspace();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowProjectStart()
    {
        _projectStartPanel.Visible = true;
        _projectStartPanel.BringToFront();
        _workspacePanel.Visible = false;
        _pageTitleLabel.Text = "Мои игры";
    }

    private void ShowWorkspace()
    {
        _projectStartPanel.Visible = false;
        _workspacePanel.Visible = true;
        _workspacePanel.BringToFront();
        _pageTitleLabel.Text = "Игры";
        _workspaceTabs.SelectedTab = _overviewTab;
    }

    private void BindWorkspace(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        _workspaceBinding = true;
        try
        {
            _workspaceTitleLabel.Text = snapshot.ProjectTitle;
            _overviewProjectLabel.Text = "Проект: " + snapshot.ProjectTitle;
            _overviewFolderLabel.Text = "Папка: " + snapshot.ProjectFolder;
            _overviewPackageStatusLabel.Text = "Пакет игры: " + snapshot.PackageStatus;
            _overviewAuthoringStatusLabel.Text = "Настройки: " + snapshot.AuthoringStatus;
            _overviewMechanicsCountLabel.Text = "Выбрано механик: " + snapshot.SelectedMechanicCount;
            _overviewLastBuildLabel.Text = "Последняя успешная сборка: " + snapshot.LastSuccessfulBuild;
            _overviewRuntimeLabel.Text = "Последняя Runtime-проверка: " + snapshot.LastRuntimeQualification;
            BindMechanics(snapshot);
            BindParameters(snapshot);
            BindSocialCard(snapshot);
            BindReleaseCandidateCard(snapshot);
            BindTechnicalDetails(snapshot);
            BindStandalone(snapshot);
            if (snapshot.Diagnostics.Count > 0 && !_buildUiRunning)
                _buildResultTextBox.Text = string.Join(Environment.NewLine, snapshot.Diagnostics);
        }
        finally
        {
            _workspaceBinding = false;
        }
    }

    private void BindSocialCard(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        var social = snapshot.Social;
        var visible = social is { Present: true, Passed: true } && social.HumanFacts.Count > 0;
        _socialCardPanel.Visible = visible;
        _socialCardLabel.Text = !visible
            ? string.Empty
            : "Социальные последствия" + (snapshot.SocialConfigurationStatus is "LAST_SUCCESS" or "UNKNOWN"
                ? " — последняя успешная проверка" : string.Empty) + Environment.NewLine + Environment.NewLine
              + string.Join(Environment.NewLine, social!.HumanFacts.Select(fact => fact.Label + "    " + fact.Value));
    }

    private void BindReleaseCandidateCard(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        var visible = snapshot.AcceptedMechanics is { Passed: true };
        _releaseCandidateCardPanel.Visible = visible;
        _releaseCandidateCardLabel.Text = visible ? BuildReleaseCandidateCardText(snapshot) : string.Empty;
        if (visible) _socialCardPanel.Visible = false;
    }

    private static string BuildReleaseCandidateCardText(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        var summary = snapshot.AcceptedMechanics
                      ?? throw new InvalidOperationException("Accepted mechanics summary is required for the RC card.");
        var status = snapshot.ReleaseCandidateConfigurationStatus switch
        {
            "CURRENT" => "Статус: RC готов",
            "LAST_SUCCESS" => "Статус: последняя успешная RC-проверка",
            "UNKNOWN" => "Статус: последняя RC-проверка; соответствие текущим настройкам не подтверждено",
            _ => "Статус: сборка пройдена; Windows RC ещё не подтверждён"
        };
        var social = summary.Social;
        var reputation = social is null ? "не подтверждено" : Number(social.ReputationBefore) + " → " + Number(social.ReputationAfter);
        var gold = social is null ? "не подтверждено" : Number(social.GoldBefore) + " → "
            + Number(social.GoldAfterQuest) + " → " + Number(social.GoldAfterClaim);
        var standalone = snapshot.ReleaseCandidateConfigurationStatus switch
        {
            "CURRENT" => "cache reused; проверки пройдены",
            "LAST_SUCCESS" => "последняя успешная проверка",
            "UNKNOWN" => "последняя проверка; текущие настройки не подтверждены",
            _ => "ещё не подтверждён"
        };
        return string.Join(Environment.NewLine, new[]
        {
            "Принятые механики — Release Candidate",
            status,
            "Механики    " + summary.SelectedMechanicCount,
            "Настроенные параметры    " + summary.ConfiguredParameterCount,
            "Снаряжение и характеристики    " + Signed(summary.EquipmentDamageBonus) + " / "
                + Signed(summary.StatDamageBonus) + " / " + Signed(summary.TotalAdditionalDamage),
            "Прогрессия    пройдена",
            "Способность и мана    урон " + Number(summary.AbilityDirectDamage) + "; "
                + Number(summary.ManaBefore) + " → " + Number(summary.ManaRemaining),
            "Эффект по ходам    " + Number(summary.StatusTickDamage) + " за ход; "
                + (summary.StatusExpired ? "завершён" : "не завершён"),
            "Репутация    " + reputation,
            "Золото    " + gold,
            "Сохранение и повтор    " + (summary.CheckpointReloadPassed && summary.FullReplayEquivalent
                && summary.ActionBindingPassed ? "пройдено" : "не пройдено"),
            "Windows standalone    " + standalone
        });
    }

    private static string Signed(decimal value) => (value >= 0 ? "+" : string.Empty) + Number(value);
    private static string Number(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private void BindStandalone(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        _unityEditorPathTextBox.Text = string.IsNullOrWhiteSpace(snapshot.StandaloneUnityEditorPath)
            ? "Не выбран — будет найден через UNITY_EDITOR_PATH или Unity Hub"
            : snapshot.StandaloneUnityEditorPath;
        var result = snapshot.LastStandaloneBuild;
        _buildWindowsStandaloneButton.Enabled = !_buildUiRunning && !snapshot.Dirty;
        _cancelWindowsStandaloneButton.Enabled = _buildUiRunning;
        _launchWindowsStandaloneButton.Enabled = result is { Status: "GREEN" } && File.Exists(result.ExecutablePath);
        _openWindowsStandaloneFolderButton.Enabled = result is { Status: "GREEN" } && Directory.Exists(result.OutputFolder);
        if (result is null) return;
        if (result.Status == "GREEN")
        {
            _standaloneStatusTextBox.Text = string.Join(Environment.NewLine, new[]
            {
                "Автоматическая проверка: ПРОЙДЕНА",
                "Payload integrity: GREEN",
                "Runtime authority: GREEN",
                "Navigation self-check: GREEN",
                "Frames: " + result.FrameCount,
                "Host cache: " + (result.HostRebuilt ? "rebuilt" : "reused"),
                "",
                "Для ручной проверки:",
                "1. Запустите игру.",
                "2. Нажмите Далее, Назад, В конец и Сбросить.",
                "3. Убедитесь, что текст обновляется без наложения."
            });
            return;
        }
        _standaloneStatusTextBox.Text = string.Join(Environment.NewLine, new[]
        {
            "Текущий этап: " + result.Stage,
            "Общий статус: " + result.Status,
            "First causal diagnostic: " + (result.Diagnostics.FirstOrDefault() ?? string.Empty),
            "Технические пути и хэши доступны на вкладке «Технические сведения»."
        });
    }

    private void FindUnityEditor()
    {
        if (_workspaceController?.HasOpenProject != true) return;
        _workspaceController.SaveStandaloneBuildSettings(new ProjectStandaloneBuildSettings());
        BindWorkspace(_workspaceController.Snapshot());
    }

    private void ChooseUnityEditor()
    {
        if (_workspaceController?.HasOpenProject != true) return;
        using var dialog = new OpenFileDialog { Filter = "Unity Editor (Unity.exe)|Unity.exe|Executable (*.exe)|*.exe", Title = "Выберите Unity Editor" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _workspaceController.SaveStandaloneBuildSettings(new ProjectStandaloneBuildSettings { UnityEditorPath = dialog.FileName });
        BindWorkspace(_workspaceController.Snapshot());
    }

    private async Task BuildWindowsStandaloneAsync()
    {
        if (_workspaceController?.HasOpenProject != true || _buildUiRunning) return;
        _buildUiRunning = true;
        SetWorkspaceBusy(true);
        _standaloneStatusTextBox.Text = "Текущий этап: validate_current_project" + Environment.NewLine + "Общий статус: RUNNING";
        try
        {
            var result = await Task.Run(() => _workspaceController.BuildWindowsStandalone()).ConfigureAwait(true);
            BindWorkspace(_workspaceController.Snapshot());
            if (result.Status != "GREEN") _buildStatusLabel.Text = "Standalone: есть ошибки";
        }
        catch (Exception exception)
        {
            _standaloneStatusTextBox.Text = "Текущий этап: unexpected" + Environment.NewLine + "First causal diagnostic: " + exception.Message;
        }
        finally
        {
            _buildUiRunning = false;
            SetWorkspaceBusy(false);
            if (_workspaceController.HasOpenProject) BindWorkspace(_workspaceController.Snapshot());
        }
    }

    private void LaunchWindowsStandalone()
    {
        try { _workspaceController?.LaunchWindowsStandalone(); }
        catch (Exception exception) { _standaloneStatusTextBox.Text = "First causal diagnostic: " + exception.Message; }
    }

    private void OpenWindowsStandaloneFolder()
    {
        try { _workspaceController?.OpenWindowsStandaloneFolder(); }
        catch (Exception exception) { _standaloneStatusTextBox.Text = "First causal diagnostic: " + exception.Message; }
    }

    private void BindMechanics(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        _mechanicsFlow.SuspendLayout();
        _mechanicsFlow.Controls.Clear();
        foreach (var mechanic in snapshot.Mechanics)
        {
            var row = new Panel { Width = 930, Height = 82, Margin = new Padding(3, 3, 3, 8) };
            var check = new CheckBox
            {
                AutoSize = true,
                Checked = mechanic.Selected,
                Enabled = !mechanic.Required,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(6, 5),
                Text = mechanic.Title,
                Tag = mechanic
            };
            var description = new Label
            {
                AutoEllipsis = true,
                Location = new Point(28, 31),
                Size = new Size(880, 42),
                Text = mechanic.Description + "  Категория: " + mechanic.Category
            };
            var technical = "ID: " + mechanic.ModuleId;
            if (mechanic.DependencyTitles.Count > 0) technical += Environment.NewLine + "Зависимости: " + string.Join(", ", mechanic.DependencyTitles);
            if (mechanic.ConflictTitles.Count > 0) technical += Environment.NewLine + "Конфликты: " + string.Join(", ", mechanic.ConflictTitles);
            _workspaceToolTip.SetToolTip(check, technical);
            check.CheckedChanged += MechanicCheckedChanged;
            row.Controls.Add(check);
            row.Controls.Add(description);
            _mechanicsFlow.Controls.Add(row);
        }
        _mechanicsFlow.ResumeLayout();
    }

    private void MechanicCheckedChanged(object? sender, EventArgs eventArgs)
    {
        if (_workspaceBinding || sender is not CheckBox check || check.Tag is not GameProjectMechanicPresentation mechanic
            || _workspaceController == null) return;
        try
        {
            BindWorkspace(_workspaceController.SetModuleSelected(mechanic.ModuleId, check.Checked));
        }
        catch (Exception exception)
        {
            _buildResultTextBox.Text = exception.Message;
            BindWorkspace(_workspaceController.Snapshot());
        }
    }

    private void BindParameters(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        _settingsFlow.SuspendLayout();
        _settingsFlow.Controls.Clear();
        if (snapshot.Parameters.Count == 0)
        {
            _settingsFlow.Controls.Add(new Label { AutoSize = true, Text = "Для выбранных механик дополнительных настроек нет." });
        }
        foreach (var parameter in snapshot.Parameters)
        {
            var row = new Panel { Width = 930, Height = 112, Margin = new Padding(3, 3, 3, 10) };
            var title = new Label
            {
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(6, 4),
                Text = parameter.ModuleTitle + " — " + parameter.Title
            };
            var description = new Label
            {
                AutoEllipsis = true,
                Location = new Point(6, 29),
                Size = new Size(700, 38),
                Text = parameter.Description + FormatRange(parameter)
            };
            var editor = CreateParameterEditor(parameter);
            editor.Location = new Point(720, 26);
            editor.Width = 185;
            editor.Tag = parameter;
            var error = new Label
            {
                ForeColor = Color.Firebrick,
                Location = new Point(6, 75),
                Size = new Size(900, 30),
                Text = parameter.ValidationError
            };
            row.Controls.Add(title);
            row.Controls.Add(description);
            row.Controls.Add(editor);
            row.Controls.Add(error);
            _settingsFlow.Controls.Add(row);
        }
        _settingsFlow.ResumeLayout();
    }

    private Control CreateParameterEditor(GameProjectParameterPresentation parameter)
    {
        if (parameter.ValueType is FeatureModuleParameterValueTypes.Integer or FeatureModuleParameterValueTypes.Number)
        {
            var editor = new NumericUpDown
            {
                DecimalPlaces = parameter.ValueType == FeatureModuleParameterValueTypes.Integer ? 0 : 2,
                Minimum = parameter.Minimum ?? -1000000m,
                Maximum = parameter.Maximum ?? 1000000m,
                Increment = parameter.Step ?? 1m,
                Value = parameter.Value.GetDecimal()
            };
            editor.ValueChanged += ParameterEditorChanged;
            return editor;
        }
        if (parameter.ValueType == FeatureModuleParameterValueTypes.Boolean)
        {
            var editor = new CheckBox { AutoSize = true, Checked = parameter.Value.GetBoolean(), Text = "Включено" };
            editor.CheckedChanged += ParameterEditorChanged;
            return editor;
        }
        if (parameter.ValueType == FeatureModuleParameterValueTypes.Enum)
        {
            var editor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            editor.Items.AddRange(parameter.AllowedValues.Cast<object>().ToArray());
            editor.SelectedItem = parameter.Value.GetString();
            editor.SelectedIndexChanged += ParameterEditorChanged;
            return editor;
        }
        throw new InvalidOperationException("Неподдерживаемый тип параметра: " + parameter.ValueType);
    }

    private void ParameterEditorChanged(object? sender, EventArgs eventArgs)
    {
        if (_workspaceBinding || sender is not Control control || control.Tag is not GameProjectParameterPresentation parameter
            || _workspaceController == null) return;
        try
        {
            JsonElement value = control switch
            {
                NumericUpDown numeric when parameter.ValueType == FeatureModuleParameterValueTypes.Integer =>
                    JsonSerializer.SerializeToElement(decimal.ToInt64(numeric.Value)),
                NumericUpDown numeric => JsonSerializer.SerializeToElement(numeric.Value),
                CheckBox check => JsonSerializer.SerializeToElement(check.Checked),
                ComboBox combo => JsonSerializer.SerializeToElement(combo.SelectedItem?.ToString() ?? string.Empty),
                _ => throw new InvalidOperationException("Редактор параметра не поддерживается.")
            };
            BindWorkspace(_workspaceController.SetParameterValue(parameter.ModuleId, parameter.ParameterId, value));
        }
        catch (Exception exception)
        {
            _buildResultTextBox.Text = exception.Message;
            BindWorkspace(_workspaceController.Snapshot());
        }
    }

    private static string FormatRange(GameProjectParameterPresentation parameter)
    {
        var parts = new List<string>();
        if (parameter.Minimum.HasValue || parameter.Maximum.HasValue)
            parts.Add("диапазон " + (parameter.Minimum?.ToString(CultureInfo.InvariantCulture) ?? "…")
                      + "–" + (parameter.Maximum?.ToString(CultureInfo.InvariantCulture) ?? "…"));
        if (!string.IsNullOrWhiteSpace(parameter.Unit)) parts.Add(parameter.Unit);
        return parts.Count == 0 ? string.Empty : " (" + string.Join(", ", parts) + ")";
    }

    private void BindTechnicalDetails(UnifiedGameProjectWorkspaceSnapshot snapshot)
    {
        var lines = new List<string>
        {
            "projectFolder=" + snapshot.ProjectFolder,
            "Project package ID: " + snapshot.ProjectPackageId,
            "Project title: " + snapshot.ProjectTitle,
            "Project version: " + snapshot.ProjectVersion,
            "Project format version: " + snapshot.ProjectFormatVersion,
            "Project-scoped composition ID: " + snapshot.ProjectScopedCompositionId,
            "Identity source/recovery status: " + snapshot.IdentitySource,
            "authoringRevision=" + snapshot.Revision,
            "catalogFingerprint=" + snapshot.CatalogFingerprint,
            string.Empty,
            "Последняя успешная сборка",
            "Composition package SHA-256: " + snapshot.CompositionPackageSha256,
            "Activated project package SHA-256: " + snapshot.ActivatedProjectPackageSha256,
            "Final Runtime state hash: " + snapshot.FinalStateHash,
            string.Empty,
            "Последняя попытка сборки",
            "Attempt ID: " + snapshot.LastBuildAttemptId,
            "Attempt status: " + snapshot.LastBuildAttemptStatus,
            "Этап сбоя: " + snapshot.LastBuildFailureStage,
            "Attempted composition package SHA-256: " + snapshot.LastBuildAttemptedCompositionPackageSha256,
            "Attempted final Runtime state hash: " + snapshot.LastBuildAttemptedFinalStateHash,
            "Attempted configured parameter count: " + snapshot.LastBuildAttemptedConfiguredParameterCount,
            "Attempted capability count: " + snapshot.LastBuildAttemptedCapabilityCount,
            "Attempted planned action count: " + snapshot.LastBuildAttemptedPlannedActionCount,
            "Attempted checkpoint action count: " + snapshot.LastBuildAttemptedCheckpointActionCount,
            "Attempted final replay action count: " + snapshot.LastBuildAttemptedFinalReplayActionCount,
            "Runtime playthrough plan ID: " + snapshot.RuntimePlaythroughPlanId,
            "Capability count: " + snapshot.CapabilityCount,
            "Planned action count: " + snapshot.PlannedActionCount,
            "Checkpoint action count: " + snapshot.CheckpointActionCount,
            "Final replay action count: " + snapshot.FinalReplayActionCount,
            "Playthrough signature: " + snapshot.PlaythroughSignature,
            "Equipment slot summary: " + snapshot.EquipmentSlotSummary,
            "Attributes summary: " + snapshot.AttributesSummary,
            "Progression summary: " + snapshot.ProgressionSummary,
            "Stat damage bonus: " + snapshot.StatDamageBonus.ToString(CultureInfo.InvariantCulture),
            "Equipment damage bonus: " + snapshot.EquipmentDamageBonus.ToString(CultureInfo.InvariantCulture),
            "Total additional damage: " + snapshot.TotalAdditionalDamage.ToString(CultureInfo.InvariantCulture),
            "certificationExecuted=" + snapshot.LastCertificationExecutedCount,
            "certificationReused=" + snapshot.LastCertificationReusedCount,
            string.Empty,
            "Текущая сохранённая конфигурация",
            "Selected module count: " + snapshot.SelectedMechanicCount,
            string.Empty,
            "Executable provenance",
            "Executable path: " + snapshot.ExecutablePath,
            "Executable SHA-256: " + snapshot.ExecutableSha256,
            "File version: " + snapshot.ExecutableFileVersion,
            "Informational version: " + snapshot.ExecutableInformationalVersion,
            string.Empty,
            "Identity recovery diagnostics:"
        };
        lines.AddRange(snapshot.IdentityRecoveryDiagnostics);
        lines.AddRange(new[]
        {
            string.Empty,
            "Модули:"
        });
        lines.AddRange(snapshot.Mechanics.Select(item => item.ModuleId + " | selected=" + item.Selected.ToString().ToLowerInvariant()));
        _technicalDetailsTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private async Task BuildAndQualifyAsync()
    {
        if (_workspaceController == null || !_workspaceController.HasOpenProject) return;
        if (_buildUiRunning || _workspaceController.BuildRunning)
        {
            _buildResultTextBox.Text = "Сборка уже выполняется. Дождитесь её завершения.";
            return;
        }
        _buildUiRunning = true;
        SetWorkspaceBusy(true);
        _buildStatusLabel.Text = "Идёт сборка и проверка...";
        _buildResultTextBox.Text = "Проверяем механики, сохранение и повтор действий.";
        try
        {
            var result = await Task.Run(() => _workspaceController.BuildAndQualify()).ConfigureAwait(true);
            _buildStatusLabel.Text = result.Passed ? "Готово" : "Есть ошибки";
            if (result.Passed)
            {
                _buildResultTextBox.Text = result.HumanSummary;
            }
            else
            {
                var firstDiagnostic = result.Diagnostics.FirstOrDefault() ?? "build.unknown_failure";
                _buildResultTextBox.Text = result.HumanSummary
                                           + Environment.NewLine + Environment.NewLine
                                           + "Этап сбоя: " + result.FailureStage
                                           + Environment.NewLine + "Причина: " + firstDiagnostic
                                           + Environment.NewLine + Environment.NewLine
                                           + "Диагностика:" + Environment.NewLine
                                           + string.Join(Environment.NewLine, result.Diagnostics);
            }
            BindWorkspace(_workspaceController.Snapshot());
        }
        catch (Exception exception)
        {
            _buildStatusLabel.Text = "Есть ошибки";
            _buildResultTextBox.Text = exception.Message;
        }
        finally
        {
            SetWorkspaceBusy(false);
            _buildUiRunning = false;
        }
    }

    private void SetWorkspaceBusy(bool busy)
    {
        _backToGamesButton.Enabled = !busy;
        _saveCurrentButton.Enabled = !busy;
        _workspaceTabs.Enabled = !busy;
        _buildAndQualifyButton.Enabled = !busy;
        _buildWindowsStandaloneButton.Enabled = !busy;
        _cancelWindowsStandaloneButton.Enabled = busy;
        _launchWindowsStandaloneButton.Enabled = !busy && _workspaceController?.Snapshot().LastStandaloneBuild is { Status: "GREEN" } result && File.Exists(result.ExecutablePath);
        _openWindowsStandaloneFolderButton.Enabled = !busy && _workspaceController?.Snapshot().LastStandaloneBuild is { Status: "GREEN" } folderResult && Directory.Exists(folderResult.OutputFolder);
    }

    private void RefreshInfo()
    {
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null)
        {
            _infoTextBox.Text = "Найдено игр: " + _projectsListView.Items.Count + Environment.NewLine
                                + "Игра не открыта. Выбери игру из списка или открой папку.";
            return;
        }
        _infoTextBox.Text = "Открыт проект: " + package.Manifest.Title + Environment.NewLine
                            + "Папка: " + _currentGamePackageService?.CurrentFolder;
    }
}
