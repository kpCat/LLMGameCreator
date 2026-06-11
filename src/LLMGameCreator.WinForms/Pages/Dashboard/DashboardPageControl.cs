namespace LLMGameCreator.WinForms.Pages;

public sealed partial class DashboardPageControl : UserControl, IEditorPage
{
    public DashboardPageControl()
    {
        InitializeComponent();
    }

    public string Id => "dashboard";
    public string Title => "Обзор";
    public int SortOrder => 0;
    Control IEditorPage.View => this;
    public void OnActivated() { }
}
