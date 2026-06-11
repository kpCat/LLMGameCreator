#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class ProjectsPageControl
    {
        private IContainer components;
        private TableLayoutPanel _layoutPanel;
        private Label _gamesRootLabel;
        private TextBox _gamesRootTextBox;
        private Button _browseGamesRootButton;
        private Button _saveGamesRootButton;
        private Button _refreshButton;
        private ListView _projectsListView;
        private ColumnHeader _gameNameColumnHeader;
        private ColumnHeader _packageIdColumnHeader;
        private ColumnHeader _versionColumnHeader;
        private ColumnHeader _statusColumnHeader;
        private ColumnHeader _gameFolderColumnHeader;
        private FlowLayoutPanel _actionsPanel;
        private Button _newGameButton;
        private Button _openSelectedButton;
        private Button _openFolderButton;
        private Button _saveCurrentButton;
        private TextBox _infoTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _layoutPanel = new TableLayoutPanel();
            _gamesRootLabel = new Label();
            _gamesRootTextBox = new TextBox();
            _browseGamesRootButton = new Button();
            _saveGamesRootButton = new Button();
            _refreshButton = new Button();
            _projectsListView = new ListView();
            _gameNameColumnHeader = new ColumnHeader();
            _packageIdColumnHeader = new ColumnHeader();
            _versionColumnHeader = new ColumnHeader();
            _statusColumnHeader = new ColumnHeader();
            _gameFolderColumnHeader = new ColumnHeader();
            _actionsPanel = new FlowLayoutPanel();
            _newGameButton = new Button();
            _openSelectedButton = new Button();
            _openFolderButton = new Button();
            _saveCurrentButton = new Button();
            _infoTextBox = new TextBox();
            _layoutPanel.SuspendLayout();
            _actionsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _layoutPanel
            // 
            _layoutPanel.ColumnCount = 4;
            _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            _layoutPanel.Controls.Add(_gamesRootLabel, 0, 0);
            _layoutPanel.Controls.Add(_gamesRootTextBox, 1, 0);
            _layoutPanel.Controls.Add(_browseGamesRootButton, 2, 0);
            _layoutPanel.Controls.Add(_saveGamesRootButton, 3, 0);
            _layoutPanel.Controls.Add(_refreshButton, 3, 1);
            _layoutPanel.Controls.Add(_projectsListView, 0, 2);
            _layoutPanel.Controls.Add(_actionsPanel, 0, 3);
            _layoutPanel.Controls.Add(_infoTextBox, 0, 4);
            _layoutPanel.Dock = DockStyle.Fill;
            _layoutPanel.Location = new Point(0, 0);
            _layoutPanel.Name = "_layoutPanel";
            _layoutPanel.Padding = new Padding(12);
            _layoutPanel.RowCount = 5;
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            _layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            _layoutPanel.Size = new Size(900, 560);
            _layoutPanel.TabIndex = 0;
            // 
            // _gamesRootLabel
            // 
            _gamesRootLabel.Dock = DockStyle.Fill;
            _gamesRootLabel.Location = new Point(15, 12);
            _gamesRootLabel.Name = "_gamesRootLabel";
            _gamesRootLabel.Size = new Size(134, 34);
            _gamesRootLabel.TabIndex = 0;
            _gamesRootLabel.Text = "Папка с играми:";
            _gamesRootLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _gamesRootTextBox
            // 
            _gamesRootTextBox.Dock = DockStyle.Fill;
            _gamesRootTextBox.Location = new Point(155, 15);
            _gamesRootTextBox.Name = "_gamesRootTextBox";
            _gamesRootTextBox.Size = new Size(470, 23);
            _gamesRootTextBox.TabIndex = 1;
            // 
            // _browseGamesRootButton
            // 
            _browseGamesRootButton.Dock = DockStyle.Fill;
            _browseGamesRootButton.Location = new Point(631, 15);
            _browseGamesRootButton.Name = "_browseGamesRootButton";
            _browseGamesRootButton.Size = new Size(124, 28);
            _browseGamesRootButton.TabIndex = 2;
            _browseGamesRootButton.Text = "Выбрать...";
            _browseGamesRootButton.UseVisualStyleBackColor = true;
            // 
            // _saveGamesRootButton
            // 
            _saveGamesRootButton.Dock = DockStyle.Fill;
            _saveGamesRootButton.Location = new Point(761, 15);
            _saveGamesRootButton.Name = "_saveGamesRootButton";
            _saveGamesRootButton.Size = new Size(124, 28);
            _saveGamesRootButton.TabIndex = 3;
            _saveGamesRootButton.Text = "Сохранить";
            _saveGamesRootButton.UseVisualStyleBackColor = true;
            // 
            // _refreshButton
            // 
            _refreshButton.Dock = DockStyle.Fill;
            _refreshButton.Location = new Point(761, 49);
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Size = new Size(124, 28);
            _refreshButton.TabIndex = 4;
            _refreshButton.Text = "Обновить";
            _refreshButton.UseVisualStyleBackColor = true;
            // 
            // _projectsListView
            // 
            _projectsListView.Columns.AddRange(new ColumnHeader[] { _gameNameColumnHeader, _packageIdColumnHeader, _versionColumnHeader, _statusColumnHeader, _gameFolderColumnHeader });
            _layoutPanel.SetColumnSpan(_projectsListView, 4);
            _projectsListView.Dock = DockStyle.Fill;
            _projectsListView.FullRowSelect = true;
            _projectsListView.Location = new Point(15, 83);
            _projectsListView.MultiSelect = false;
            _projectsListView.Name = "_projectsListView";
            _projectsListView.Size = new Size(870, 249);
            _projectsListView.TabIndex = 5;
            _projectsListView.UseCompatibleStateImageBehavior = false;
            _projectsListView.View = System.Windows.Forms.View.Details;
            // 
            // _gameNameColumnHeader
            // 
            _gameNameColumnHeader.Text = "Игра";
            _gameNameColumnHeader.Width = 180;
            // 
            // _packageIdColumnHeader
            // 
            _packageIdColumnHeader.Text = "PackageId";
            _packageIdColumnHeader.Width = 160;
            // 
            // _versionColumnHeader
            // 
            _versionColumnHeader.Text = "Version";
            _versionColumnHeader.Width = 80;
            // 
            // _statusColumnHeader
            // 
            _statusColumnHeader.Text = "Status";
            _statusColumnHeader.Width = 180;
            // 
            // _gameFolderColumnHeader
            // 
            _gameFolderColumnHeader.Text = "Папка";
            _gameFolderColumnHeader.Width = 300;
            // 
            // _actionsPanel
            // 
            _layoutPanel.SetColumnSpan(_actionsPanel, 4);
            _actionsPanel.Controls.Add(_newGameButton);
            _actionsPanel.Controls.Add(_openSelectedButton);
            _actionsPanel.Controls.Add(_openFolderButton);
            _actionsPanel.Controls.Add(_saveCurrentButton);
            _actionsPanel.Dock = DockStyle.Fill;
            _actionsPanel.Location = new Point(15, 338);
            _actionsPanel.Name = "_actionsPanel";
            _actionsPanel.Size = new Size(870, 36);
            _actionsPanel.TabIndex = 6;
            // 
            // _newGameButton
            // 
            _newGameButton.Location = new Point(3, 3);
            _newGameButton.Name = "_newGameButton";
            _newGameButton.Size = new Size(110, 30);
            _newGameButton.TabIndex = 0;
            _newGameButton.Text = "Новая игра";
            _newGameButton.UseVisualStyleBackColor = true;
            // 
            // _openSelectedButton
            // 
            _openSelectedButton.Location = new Point(119, 3);
            _openSelectedButton.Name = "_openSelectedButton";
            _openSelectedButton.Size = new Size(170, 30);
            _openSelectedButton.TabIndex = 1;
            _openSelectedButton.Text = "Открыть выбранную";
            _openSelectedButton.UseVisualStyleBackColor = true;
            // 
            // _openFolderButton
            // 
            _openFolderButton.Location = new Point(295, 3);
            _openFolderButton.Name = "_openFolderButton";
            _openFolderButton.Size = new Size(190, 30);
            _openFolderButton.TabIndex = 2;
            _openFolderButton.Text = "Открыть папку вручную";
            _openFolderButton.UseVisualStyleBackColor = true;
            // 
            // _saveCurrentButton
            // 
            _saveCurrentButton.Location = new Point(491, 3);
            _saveCurrentButton.Name = "_saveCurrentButton";
            _saveCurrentButton.Size = new Size(170, 30);
            _saveCurrentButton.TabIndex = 3;
            _saveCurrentButton.Text = "Сохранить текущую";
            _saveCurrentButton.UseVisualStyleBackColor = true;
            // 
            // _infoTextBox
            // 
            _layoutPanel.SetColumnSpan(_infoTextBox, 4);
            _infoTextBox.Dock = DockStyle.Fill;
            _infoTextBox.Location = new Point(15, 380);
            _infoTextBox.Multiline = true;
            _infoTextBox.Name = "_infoTextBox";
            _infoTextBox.ReadOnly = true;
            _infoTextBox.ScrollBars = ScrollBars.Vertical;
            _infoTextBox.Size = new Size(870, 165);
            _infoTextBox.TabIndex = 7;
            // 
            // ProjectsPageControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_layoutPanel);
            Name = "ProjectsPageControl";
            Size = new Size(900, 560);
            _layoutPanel.ResumeLayout(false);
            _layoutPanel.PerformLayout();
            _actionsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
