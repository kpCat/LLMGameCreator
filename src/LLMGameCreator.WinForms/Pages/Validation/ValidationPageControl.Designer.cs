namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ValidationPageControl
{
    private Panel _rootPanel = null!;
    private Button _validateButton = null!;
    private ListBox _issuesListBox = null!;

    private void InitializeComponent()
    {
        _rootPanel = new Panel();
        _validateButton = new Button();
        _issuesListBox = new ListBox();
        _rootPanel.SuspendLayout();
        SuspendLayout();

        _rootPanel.Dock = DockStyle.Fill;
        _rootPanel.Padding = new Padding(12);
        _rootPanel.Controls.Add(_issuesListBox);
        _rootPanel.Controls.Add(_validateButton);

        _validateButton.Dock = DockStyle.Top;
        _validateButton.Height = 36;
        _validateButton.Text = "Проверить текущий GamePackage";
        _validateButton.UseVisualStyleBackColor = true;

        _issuesListBox.Dock = DockStyle.Fill;
        _issuesListBox.FormattingEnabled = true;
        _issuesListBox.ItemHeight = 15;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_rootPanel);
        Name = "ValidationPageControl";
        Size = new Size(800, 450);

        _rootPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
