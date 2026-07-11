using System.Globalization;
using System.Text.Json;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
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
            BindTechnicalDetails(snapshot);
            if (snapshot.Diagnostics.Count > 0 && !_buildUiRunning)
                _buildResultTextBox.Text = string.Join(Environment.NewLine, snapshot.Diagnostics);
        }
        finally
        {
            _workspaceBinding = false;
        }
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
            "authoringRevision=" + snapshot.Revision,
            "catalogFingerprint=" + snapshot.CatalogFingerprint,
            "packageSha256=" + snapshot.PackageSha256,
            "finalStateHash=" + snapshot.FinalStateHash,
            "certificationExecuted=" + snapshot.LastCertificationExecutedCount,
            "certificationReused=" + snapshot.LastCertificationReusedCount,
            string.Empty,
            "Модули:"
        };
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
            _buildResultTextBox.Text = result.HumanSummary
                                       + (result.Diagnostics.Count == 0
                                           ? string.Empty
                                           : Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, result.Diagnostics));
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
