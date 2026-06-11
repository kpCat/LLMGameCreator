namespace LLMGameCreator.WinForms.Pages;

public sealed partial class GenerationPageControl
{
    private TextBox _descriptionTextBox = null!;

    private void InitializeComponent()
    {
        _descriptionTextBox = new TextBox();
        SuspendLayout();

        _descriptionTextBox.Dock = DockStyle.Fill;
        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.ReadOnly = true;
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        _descriptionTextBox.Text = "Generation v0.1\r\n\r\nЗдесь позже будут:\r\n- LLM sessions;\r\n- job-based генерация;\r\n- context packs;\r\n- draft/patch workflow;\r\n- профили локальных моделей, включая другие ПК в LAN.\r\n\r\nRuntime LLM generation запрещён архитектурно.";

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_descriptionTextBox);
        Name = "GenerationPageControl";
        Size = new Size(800, 450);

        ResumeLayout(false);
        PerformLayout();
    }
}
