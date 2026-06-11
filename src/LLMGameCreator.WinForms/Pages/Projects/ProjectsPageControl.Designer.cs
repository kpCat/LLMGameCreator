namespace LLMGameCreator.WinForms.Pages;

public sealed partial class ProjectsPageControl
{
    private TableLayoutPanel _layoutPanel = null!;
    private Label _folderLabel = null!;
    private TextBox _folderTextBox = null!;
    private Button _browseButton = null!;
    private Button _loadButton = null!;
    private TextBox _infoTextBox = null!;

    private void InitializeComponent()
    {
        _layoutPanel = new TableLayoutPanel();
        _folderLabel = new Label();
        _folderTextBox = new TextBox();
        _browseButton = new Button();
        _loadButton = new Button();
        _infoTextBox = new TextBox();
        _layoutPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _layoutPanel
        // 
        _layoutPanel.ColumnCount = 3;
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        _layoutPanel.Controls.Add(_folderLabel, 0, 0);
        _layoutPanel.Controls.Add(_folderTextBox, 1, 0);
        _layoutPanel.Controls.Add(_browseButton, 2, 0);
        _layoutPanel.Controls.Add(_loadButton, 1, 1);
        _layoutPanel.Controls.Add(_infoTextBox, 0, 2);
        _layoutPanel.Dock = DockStyle.Fill;
        _layoutPanel.Location = new Point(0, 0);
        _layoutPanel.Name = "_layoutPanel";
        _layoutPanel.Padding = new Padding(12);
        _layoutPanel.RowCount = 3;
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layoutPanel.Size = new Size(800, 450);
        _layoutPanel.TabIndex = 0;
        // 
        // _folderLabel
        // 
        _folderLabel.Dock = DockStyle.Fill;
        _folderLabel.Location = new Point(15, 12);
        _folderLabel.Name = "_folderLabel";
        _folderLabel.Size = new Size(114, 32);
        _folderLabel.TabIndex = 0;
        _folderLabel.Text = "Папка игры:";
        _folderLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _folderTextBox
        // 
        _folderTextBox.Dock = DockStyle.Fill;
        _folderTextBox.Location = new Point(135, 15);
        _folderTextBox.Name = "_folderTextBox";
        _folderTextBox.Size = new Size(490, 23);
        _folderTextBox.TabIndex = 1;
        // 
        // _browseButton
        // 
        _browseButton.Dock = DockStyle.Fill;
        _browseButton.Location = new Point(631, 15);
        _browseButton.Name = "_browseButton";
        _browseButton.Size = new Size(154, 26);
        _browseButton.TabIndex = 2;
        _browseButton.Text = "Выбрать...";
        _browseButton.UseVisualStyleBackColor = true;
        // 
        // _loadButton
        // 
        _loadButton.Dock = DockStyle.Left;
        _loadButton.Location = new Point(135, 47);
        _loadButton.Name = "_loadButton";
        _loadButton.Size = new Size(140, 34);
        _loadButton.TabIndex = 3;
        _loadButton.Text = "Загрузить";
        _loadButton.UseVisualStyleBackColor = true;
        // 
        // _infoTextBox
        // 
        _layoutPanel.SetColumnSpan(_infoTextBox, 3);
        _infoTextBox.Dock = DockStyle.Fill;
        _infoTextBox.Location = new Point(15, 87);
        _infoTextBox.Multiline = true;
        _infoTextBox.Name = "_infoTextBox";
        _infoTextBox.ReadOnly = true;
        _infoTextBox.ScrollBars = ScrollBars.Vertical;
        _infoTextBox.Size = new Size(770, 348);
        _infoTextBox.TabIndex = 4;
        // 
        // ProjectsPageControl
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_layoutPanel);
        Name = "ProjectsPageControl";
        Size = new Size(800, 450);
        _layoutPanel.ResumeLayout(false);
        _layoutPanel.PerformLayout();
        ResumeLayout(false);
    }
}
