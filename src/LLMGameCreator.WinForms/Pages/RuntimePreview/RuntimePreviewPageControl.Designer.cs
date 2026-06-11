namespace LLMGameCreator.WinForms.Pages;

public sealed partial class RuntimePreviewPageControl
{
    private SplitContainer _rootSplitContainer = null!;
    private Panel _leftPanel = null!;
    private FlowLayoutPanel _toolbarPanel = null!;
    private Button _startButton = null!;
    private RuntimeMapCanvas _canvas = null!;
    private TextBox _logTextBox = null!;

    private void InitializeComponent()
    {
        _rootSplitContainer = new SplitContainer();
        _leftPanel = new Panel();
        _toolbarPanel = new FlowLayoutPanel();
        _startButton = new Button();
        _canvas = new RuntimeMapCanvas();
        _logTextBox = new TextBox();
        ((System.ComponentModel.ISupportInitialize)_rootSplitContainer).BeginInit();
        _rootSplitContainer.Panel1.SuspendLayout();
        _rootSplitContainer.Panel2.SuspendLayout();
        _rootSplitContainer.SuspendLayout();
        _leftPanel.SuspendLayout();
        _toolbarPanel.SuspendLayout();
        SuspendLayout();

        _rootSplitContainer.Dock = DockStyle.Fill;
        _rootSplitContainer.Orientation = Orientation.Vertical;
        _rootSplitContainer.SplitterDistance = 820;
        _rootSplitContainer.Panel1.Controls.Add(_leftPanel);
        _rootSplitContainer.Panel2.Controls.Add(_logTextBox);

        _leftPanel.Dock = DockStyle.Fill;
        _leftPanel.Padding = new Padding(12);
        _leftPanel.Controls.Add(_canvas);
        _leftPanel.Controls.Add(_toolbarPanel);

        _toolbarPanel.Dock = DockStyle.Top;
        _toolbarPanel.Height = 42;
        _toolbarPanel.Controls.Add(_startButton);

        _startButton.Height = 30;
        _startButton.Text = "Старт";
        _startButton.UseVisualStyleBackColor = true;
        _startButton.Width = 100;

        _canvas.Dock = DockStyle.Fill;

        _logTextBox.Dock = DockStyle.Fill;
        _logTextBox.Multiline = true;
        _logTextBox.ReadOnly = true;
        _logTextBox.ScrollBars = ScrollBars.Vertical;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_rootSplitContainer);
        Name = "RuntimePreviewPageControl";
        Size = new Size(1000, 600);

        _rootSplitContainer.Panel1.ResumeLayout(false);
        _rootSplitContainer.Panel2.ResumeLayout(false);
        _rootSplitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_rootSplitContainer).EndInit();
        _rootSplitContainer.ResumeLayout(false);
        _leftPanel.ResumeLayout(false);
        _toolbarPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
