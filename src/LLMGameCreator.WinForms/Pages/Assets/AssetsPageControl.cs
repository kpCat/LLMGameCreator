using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.WinForms.Pages;

public sealed class AssetsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly ListView _listView = new ListView();

    public AssetsPageControl(ICurrentGamePackageService currentGamePackageService)
    {
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
    }

    public string Id => "assets";
    public string Title => "Ассеты";
    public int SortOrder => 60;
    public Control View => this;

    public void OnActivated()
    {
        RefreshAssets();
    }

    private void BuildLayout()
    {
        _listView.Dock = DockStyle.Fill;
        _listView.View = System.Windows.Forms.View.Details;
        _listView.FullRowSelect = true;
        _listView.Columns.Add("Id", 280);
        _listView.Columns.Add("Type", 160);
        _listView.Columns.Add("Path", 380);
        _listView.Columns.Add("Contract", 180);
        Controls.Add(_listView);
    }

    private void RefreshAssets()
    {
        _listView.Items.Clear();
        var package = _currentGamePackageService.CurrentPackage;
        if (package == null)
        {
            return;
        }

        foreach (var asset in package.AssetCatalog.Assets)
        {
            var item = new ListViewItem(asset.Id);
            item.SubItems.Add(asset.Type);
            item.SubItems.Add(asset.Path ?? string.Empty);
            item.SubItems.Add(asset.ContractId ?? string.Empty);
            _listView.Items.Add(item);
        }
    }
}
