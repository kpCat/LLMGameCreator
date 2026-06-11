using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class AssetsPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;

    public AssetsPageControl()
    {
        InitializeComponent();
    }

    public AssetsPageControl(ICurrentGamePackageService currentGamePackageService)
    {
        _currentGamePackageService = currentGamePackageService;
        InitializeComponent();
    }

    public string Id => "assets";
    public string Title => "Ассеты";
    public int SortOrder => 60;
    public Control View => this;

    public void OnActivated()
    {
        RefreshAssets();
    }

    private void RefreshAssets()
    {
        _listView.Items.Clear();
        var package = _currentGamePackageService?.CurrentPackage;
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
