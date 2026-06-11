namespace LLMGameCreator.WinForms.Pages;

public sealed class GenerationPageControl : UserControl, IEditorPage
{
    public GenerationPageControl()
    {
        var text = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = "Generation v0.1\r\n\r\nЗдесь позже будут:\r\n- LLM sessions;\r\n- job-based генерация;\r\n- context packs;\r\n- draft/patch workflow;\r\n- профили локальных моделей, включая другие ПК в LAN.\r\n\r\nRuntime LLM generation запрещён архитектурно."
        };
        Controls.Add(text);
    }

    public string Id => "generation";
    public string Title => "Генерация";
    public int SortOrder => 30;
    public Control View => this;
    public void OnActivated() { }
}
