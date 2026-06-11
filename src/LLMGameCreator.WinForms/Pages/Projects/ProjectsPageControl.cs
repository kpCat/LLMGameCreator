using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.WinForms.Pages;

public sealed class ProjectsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly TextBox _folderTextBox = new TextBox();
    private readonly TextBox _infoTextBox = new TextBox();

    public ProjectsPageControl(ICurrentGamePackageService currentGamePackageService)
    {
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
    }

    public string Id => "projects";
    public string Title => "Проекты";
    public int SortOrder => 10;
    public Control View => this;

    public void OnActivated()
    {
        RefreshInfo();
    }

    private void BuildLayout()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(12)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var folderLabel = new Label { Text = "Папка игры:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        _folderTextBox.Dock = DockStyle.Fill;
        var browseButton = new Button { Text = "Выбрать...", Dock = DockStyle.Fill };
        var loadButton = new Button { Text = "Загрузить", Dock = DockStyle.Left, Width = 140 };
        _infoTextBox.Dock = DockStyle.Fill;
        _infoTextBox.Multiline = true;
        _infoTextBox.ReadOnly = true;
        _infoTextBox.ScrollBars = ScrollBars.Vertical;

        browseButton.Click += (_, _) => BrowseFolder();
        loadButton.Click += async (_, _) => await LoadSelectedFolderAsync();

        panel.Controls.Add(folderLabel, 0, 0);
        panel.Controls.Add(_folderTextBox, 1, 0);
        panel.Controls.Add(browseButton, 2, 0);
        panel.Controls.Add(loadButton, 1, 1);
        panel.Controls.Add(_infoTextBox, 0, 2);
        panel.SetColumnSpan(_infoTextBox, 3);

        Controls.Add(panel);
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
