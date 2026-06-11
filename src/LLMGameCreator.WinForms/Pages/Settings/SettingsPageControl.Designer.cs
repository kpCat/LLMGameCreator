namespace LLMGameCreator.WinForms.Pages;

public sealed partial class SettingsPageControl
{
    private TextBox _settingsTextBox = null!;

    private void InitializeComponent()
    {
        _settingsTextBox = new TextBox();
        SuspendLayout();

        _settingsTextBox.Dock = DockStyle.Fill;
        _settingsTextBox.Multiline = true;
        _settingsTextBox.ReadOnly = true;
        _settingsTextBox.ScrollBars = ScrollBars.Vertical;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_settingsTextBox);
        Name = "SettingsPageControl";
        Size = new Size(800, 450);

        ResumeLayout(false);
        PerformLayout();
    }
}
