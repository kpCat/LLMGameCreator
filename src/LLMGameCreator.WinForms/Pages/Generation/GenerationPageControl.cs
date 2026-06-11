namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GenerationPageControl : UserControl, IEditorPage
{
    public GenerationPageControl()
    {
        InitializeComponent();
    }

    public string Id => "generation";
    public string Title => "Генерация";
    public int SortOrder => 30;
    public Control View => this;
    public void OnActivated() { }
}
