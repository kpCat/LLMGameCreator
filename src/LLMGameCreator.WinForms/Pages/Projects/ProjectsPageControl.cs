using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ProjectsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IAppSettingsRepository? _settingsRepository;
    private readonly IGameProjectService? _gameProjectService;
    private readonly IGamePackageValidator? _validator;
    private AppSettings? _settings;

    public ProjectsPageControl()
    {
        InitializeComponent();
        _infoTextBox.Text = "Design-time preview. Runtime services are not available in Visual Studio Designer.";
    }

    public ProjectsPageControl(
        ICurrentGamePackageService currentGamePackageService,
        IAppSettingsRepository settingsRepository,
        IGameProjectService gameProjectService,
        IGamePackageValidator validator)
    {
        _currentGamePackageService = currentGamePackageService;
        _settingsRepository = settingsRepository;
        _gameProjectService = gameProjectService;
        _validator = validator;
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
        _projectsListView.DoubleClick += async (_, _) => await OpenSelectedProjectAsync();
    }

    private async Task LoadSettingsAndRefreshAsync()
    {
        if (_settingsRepository == null)
        {
            return;
        }

        _settings = await _settingsRepository.LoadAsync(CancellationToken.None).ConfigureAwait(true);
        _gamesRootTextBox.Text = _settings.GamesRootPath;
        await RefreshProjectsListAsync();
        RefreshInfo();
    }

    private void BrowseGamesRoot()
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "Выбери корневую папку, внутри которой будут лежать папки отдельных игр.";
        dialog.SelectedPath = Directory.Exists(_gamesRootTextBox.Text) ? _gamesRootTextBox.Text : string.Empty;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _gamesRootTextBox.Text = dialog.SelectedPath;
        }
    }

    private async Task SaveGamesRootAsync()
    {
        if (_settingsRepository == null)
        {
            return;
        }

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
            _infoTextBox.Text = "Project service is not available.";
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
        catch (Exception ex)
        {
            _infoTextBox.Text = $"Не удалось обновить список игр:\r\n{ex.Message}";
            return;
        }

        RefreshInfo();
    }

    private static string GetStatusText(GameProjectSummary summary)
    {
        if (!summary.HasPackageFile)
        {
            return "No package.json";
        }

        if (!summary.IsValidPackage)
        {
            return string.IsNullOrWhiteSpace(summary.ErrorMessage)
                ? $"Invalid ({summary.ErrorCount} errors)"
                : $"Invalid: {summary.ErrorMessage}";
        }

        if (summary.WarningCount > 0)
        {
            return $"Valid, {summary.WarningCount} warnings";
        }

        return "Valid";
    }

    private async Task OpenSelectedProjectAsync()
    {
        if (_projectsListView.SelectedItems.Count == 0)
        {
            MessageBox.Show(this, "Выбери игру в списке.", "Игра не выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_projectsListView.SelectedItems[0].Tag is GameProjectSummary summary)
        {
            await LoadProjectFolderAsync(summary.FolderPath);
        }
    }

    private async Task OpenArbitraryFolderAsync()
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "Выбери папку конкретной игры, где лежит package.json.";
        dialog.SelectedPath = Directory.Exists(_gamesRootTextBox.Text) ? _gamesRootTextBox.Text : string.Empty;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await LoadProjectFolderAsync(dialog.SelectedPath);
        }
    }

    private async Task CreateNewGameAsync()
    {
        if (_gameProjectService == null)
        {
            return;
        }

        var root = _gamesRootTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(root))
        {
            MessageBox.Show(this, "Сначала укажи корневую папку игр.", "Новая игра", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new CreateGameDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var summary = await _gameProjectService.CreateAsync(dialog.CreateRequest(root), CancellationToken.None).ConfigureAwait(true);
            await RefreshProjectsListAsync();
            await LoadProjectFolderAsync(summary.FolderPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Не удалось создать игру", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveCurrentGameAsync()
    {
        if (_currentGamePackageService == null)
        {
            return;
        }

        try
        {
            await _currentGamePackageService.SaveAsync(CancellationToken.None).ConfigureAwait(true);
            RefreshInfo();
            MessageBox.Show(this, "Текущая игра сохранена.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadProjectFolderAsync(string folder)
    {
        if (_currentGamePackageService == null)
        {
            return;
        }

        try
        {
            await _currentGamePackageService.LoadAsync(folder, CancellationToken.None).ConfigureAwait(true);
            RefreshInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshInfo()
    {
        var package = _currentGamePackageService?.CurrentPackage;
        if (package == null)
        {
            _infoTextBox.Text =
                $"Корневая папка игр: {_gamesRootTextBox.Text}\r\n" +
                $"Найдено игр: {_projectsListView.Items.Count}\r\n\r\n" +
                "Игра не открыта. Выбери игру из списка или открой папку вручную.";
            return;
        }

        var validationSummary = GetCurrentValidationSummary(package);

        _infoTextBox.Text =
            $"Открыт проект: {package.Manifest.Title}\r\n" +
            $"PackageId: {package.Manifest.PackageId}\r\n" +
            $"Папка: {_currentGamePackageService?.CurrentFolder}\r\n\r\n" +
            $"Validation: {validationSummary}\r\n" +
            $"Maps: {package.Game.Maps.Count}\r\n" +
            $"Tiles: {package.Game.TilePrototypes.Count}\r\n" +
            $"Entities: {package.Game.EntityPrototypes.Count}\r\n" +
            $"Assets: {package.AssetCatalog.Assets.Count}\r\n" +
            $"Scripts: {package.ScriptCatalog.Scripts.Count}";
    }

    private string GetCurrentValidationSummary(LLMGameCreator.GamePackage.GamePackageDefinition package)
    {
        if (_validator == null)
        {
            return "validator is not available";
        }

        var report = _validator.Validate(package, _currentGamePackageService?.CurrentFolder);
        var errors = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Error || issue.Severity == ValidationSeverity.Critical);
        var warnings = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        return report.IsValid ? $"valid, {warnings} warnings" : $"invalid, {errors} errors, {warnings} warnings";
    }
}
