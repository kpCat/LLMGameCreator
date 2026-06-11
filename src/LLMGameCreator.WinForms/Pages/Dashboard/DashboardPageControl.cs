namespace LLMGameCreator.WinForms.Pages;

public sealed class DashboardPageControl : UserControl, IEditorPage
{
    public DashboardPageControl()
    {
        var label = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(FontFamily.GenericSansSerif, 12),
            Text = "LLMGameCreator v0.1 skeleton\r\n\r\nЦель: GamePackage + typed Lua + asset catalog + headless runtime + WinForms editor shell."
        };
        Controls.Add(label);
    }

    public string Id => "dashboard";
    public string Title => "Обзор";
    public int SortOrder => 0;
    public Control View => this;
    public void OnActivated() { }
}
