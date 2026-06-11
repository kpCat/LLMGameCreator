using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ProjectsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IAppSettingsRepository? _settingsRepository;
    private AppSettings? _settings;

    public ProjectsPageControl()
    {
        InitializeComponent();
        _infoTextBox.Text = "Design-time preview. Runtime services are not available in Visual Studio Designer.";
    }

    public ProjectsPageControl(ICurrentGamePackageService currentGamePackageService, IAppSettingsRepository settingsRepository)
    {
        _currentGamePackageService = currentGamePackageService;
        _settingsRepository = settingsRepository;
        InitializeComponent();
        WireEvents();
    }

    public string Id => "projects";
    public string Title => "Игры";
    public int SortOrder => 10;
    public Control View => this;

    public async void OnActivated()
    {
        await LoadSettingsAndRefreshAsync();
    }

    private void WireEvents()
    {
        _browseGamesRootButton.Click += (_, _) => BrowseGamesRoot();
        _saveGamesRootButton.Click += async (_, _) => await SaveGamesRootAsync();
        _refreshButton.Click += (_, _) => RefreshProjectsList();
        _openSelectedButton.Click += async (_, _) => await OpenSelectedProjectAsync();
        _openFolderButton.Click += async (_, _) => await OpenArbitraryFolderAsync();
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
        RefreshProjectsList();
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
        RefreshProjectsList();
        RefreshInfo();
    }

    private void RefreshProjectsList()
    {
        _projectsListView.Items.Clear();
        var root = _gamesRootTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(root))
        {
            _infoTextBox.Text = "Корневая папка игр не указана.";
            return;
        }

        if (!Directory.Exists(root))
        {
            _infoTextBox.Text = $"Корневая папка игр не существует:\r\n{root}";
            return;
        }

        foreach (var folder in EnumerateGameFolders(root))
        {
            var item = new ListViewItem(Path.GetFileName(folder));
            item.SubItems.Add(folder);
            item.Tag = folder;
            _projectsListView.Items.Add(item);
        }

        RefreshInfo();
    }

    private static IEnumerable<string> EnumerateGameFolders(string root)
    {
        if (File.Exists(Path.Combine(root, "package.json")))
        {
            yield return root;
            yield break;
        }

        foreach (var directory in Directory.EnumerateDirectories(root).OrderBy(Path.GetFileName))
        {
            if (File.Exists(Path.Combine(directory, "package.json")))
            {
                yield return directory;
            }
        }
    }

    private async Task OpenSelectedProjectAsync()
    {
        if (_projectsListView.SelectedItems.Count == 0)
        {
            MessageBox.Show(this, "Выбери игру в списке.", "Игра не выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_projectsListView.SelectedItems[0].Tag is string folder)
        {
            await LoadProjectFolderAsync(folder);
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

        _infoTextBox.Text =
            $"Открыт проект: {package.Manifest.Title}\r\n" +
            $"PackageId: {package.Manifest.PackageId}\r\n" +
            $"Папка: {_currentGamePackageService?.CurrentFolder}\r\n\r\n" +
            $"Maps: {package.Game.Maps.Count}\r\n" +
            $"Tiles: {package.Game.TilePrototypes.Count}\r\n" +
            $"Entities: {package.Game.EntityPrototypes.Count}\r\n" +
            $"Assets: {package.AssetCatalog.Assets.Count}\r\n" +
            $"Scripts: {package.ScriptCatalog.Scripts.Count}";
    }
}
