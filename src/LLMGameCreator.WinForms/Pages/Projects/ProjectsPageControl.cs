using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ProjectsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService _currentGamePackageService;

    public ProjectsPageControl(ICurrentGamePackageService currentGamePackageService)
    {
        _currentGamePackageService = currentGamePackageService;
        InitializeComponent();

        _browseButton.Click += (_, _) => BrowseFolder();
        _loadButton.Click += async (_, _) => await LoadSelectedFolderAsync();
    }

    public string Id => "projects";
    public string Title => "Проекты";
    public int SortOrder => 10;
    public Control View => this;

    public void OnActivated()
    {
        RefreshInfo();
    }

    private void BrowseFolder()
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _folderTextBox.Text = dialog.SelectedPath;
        }
    }

    private async Task LoadSelectedFolderAsync()
    {
        try
        {
            await _currentGamePackageService.LoadAsync(_folderTextBox.Text, CancellationToken.None);
            RefreshInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshInfo()
    {
        var package = _currentGamePackageService.CurrentPackage;
        _infoTextBox.Text = package == null
            ? "Проект игры не открыт. Выбери папку, где лежит package.json. Например: samples\\minimal-map-game."
            : $"Открыт: {package.Manifest.Title}\r\nPackageId: {package.Manifest.PackageId}\r\nMaps: {package.Game.Maps.Count}\r\nTiles: {package.Game.TilePrototypes.Count}\r\nEntities: {package.Game.EntityPrototypes.Count}\r\nAssets: {package.AssetCatalog.Assets.Count}\r\nScripts: {package.ScriptCatalog.Scripts.Count}";
    }
}
