namespace LLMGameCreator.WinForms.Pages;

public sealed partial class DashboardPageControl
{
    private Label _summaryLabel = null!;

    private void InitializeComponent()
    {
        _summaryLabel = new Label();
        SuspendLayout();

        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.Font = new Font(FontFamily.GenericSansSerif, 12F, FontStyle.Regular, GraphicsUnit.Point);
        _summaryLabel.TextAlign = ContentAlignment.MiddleCenter;
        _summaryLabel.Text = "LLMGameCreator v0.1 skeleton\r\n\r\nЦель: GamePackage + typed Lua + asset catalog + headless runtime + WinForms editor shell.";

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_summaryLabel);
        Name = "DashboardPageControl";
        Size = new Size(800, 450);

        ResumeLayout(false);
    }
}
