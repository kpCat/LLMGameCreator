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
        private ColumnHeader _gameFolderColumnHeader;
        private FlowLayoutPanel _actionsPanel;
        private Button _openSelectedButton;
        private Button _openFolderButton;
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
            this.components = new Container();
            this._layoutPanel = new TableLayoutPanel();
            this._gamesRootLabel = new Label();
            this._gamesRootTextBox = new TextBox();
            this._browseGamesRootButton = new Button();
            this._saveGamesRootButton = new Button();
            this._refreshButton = new Button();
            this._projectsListView = new ListView();
            this._gameNameColumnHeader = new ColumnHeader();
            this._gameFolderColumnHeader = new ColumnHeader();
            this._actionsPanel = new FlowLayoutPanel();
            this._openSelectedButton = new Button();
            this._openFolderButton = new Button();
            this._infoTextBox = new TextBox();
            this._layoutPanel.SuspendLayout();
            this._actionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layoutPanel
            // 
            this._layoutPanel.ColumnCount = 4;
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            this._layoutPanel.Controls.Add(this._gamesRootLabel, 0, 0);
            this._layoutPanel.Controls.Add(this._gamesRootTextBox, 1, 0);
            this._layoutPanel.Controls.Add(this._browseGamesRootButton, 2, 0);
            this._layoutPanel.Controls.Add(this._saveGamesRootButton, 3, 0);
            this._layoutPanel.Controls.Add(this._refreshButton, 3, 1);
            this._layoutPanel.Controls.Add(this._projectsListView, 0, 2);
            this._layoutPanel.Controls.Add(this._actionsPanel, 0, 3);
            this._layoutPanel.Controls.Add(this._infoTextBox, 0, 4);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Location = new Point(0, 0);
            this._layoutPanel.Name = "_layoutPanel";
            this._layoutPanel.Padding = new Padding(12);
            this._layoutPanel.RowCount = 5;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            this._layoutPanel.Size = new Size(900, 560);
            this._layoutPanel.TabIndex = 0;
            // 
            // _gamesRootLabel
            // 
            this._gamesRootLabel.Dock = DockStyle.Fill;
            this._gamesRootLabel.Location = new Point(15, 12);
            this._gamesRootLabel.Name = "_gamesRootLabel";
            this._gamesRootLabel.Size = new Size(134, 34);
            this._gamesRootLabel.TabIndex = 0;
            this._gamesRootLabel.Text = "Папка с играми:";
            this._gamesRootLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _gamesRootTextBox
            // 
            this._gamesRootTextBox.Dock = DockStyle.Fill;
            this._gamesRootTextBox.Location = new Point(155, 15);
            this._gamesRootTextBox.Name = "_gamesRootTextBox";
            this._gamesRootTextBox.Size = new Size(480, 23);
            this._gamesRootTextBox.TabIndex = 1;
            // 
            // _browseGamesRootButton
            // 
            this._browseGamesRootButton.Dock = DockStyle.Fill;
            this._browseGamesRootButton.Location = new Point(641, 15);
            this._browseGamesRootButton.Name = "_browseGamesRootButton";
            this._browseGamesRootButton.Size = new Size(124, 28);
            this._browseGamesRootButton.TabIndex = 2;
            this._browseGamesRootButton.Text = "Выбрать...";
            this._browseGamesRootButton.UseVisualStyleBackColor = true;
            // 
            // _saveGamesRootButton
            // 
            this._saveGamesRootButton.Dock = DockStyle.Fill;
            this._saveGamesRootButton.Location = new Point(771, 15);
            this._saveGamesRootButton.Name = "_saveGamesRootButton";
            this._saveGamesRootButton.Size = new Size(114, 28);
            this._saveGamesRootButton.TabIndex = 3;
            this._saveGamesRootButton.Text = "Сохранить";
            this._saveGamesRootButton.UseVisualStyleBackColor = true;
            // 
            // _refreshButton
            // 
            this._refreshButton.Dock = DockStyle.Fill;
            this._refreshButton.Location = new Point(771, 49);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new Size(114, 28);
            this._refreshButton.TabIndex = 4;
            this._refreshButton.Text = "Обновить";
            this._refreshButton.UseVisualStyleBackColor = true;
            // 
            // _projectsListView
            // 
            this._projectsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._gameNameColumnHeader,
                this._gameFolderColumnHeader
            });
            this._layoutPanel.SetColumnSpan(this._projectsListView, 4);
            this._projectsListView.Dock = DockStyle.Fill;
            this._projectsListView.FullRowSelect = true;
            this._projectsListView.Location = new Point(15, 83);
            this._projectsListView.MultiSelect = false;
            this._projectsListView.Name = "_projectsListView";
            this._projectsListView.Size = new Size(870, 255);
            this._projectsListView.TabIndex = 5;
            this._projectsListView.UseCompatibleStateImageBehavior = false;
            this._projectsListView.View = System.Windows.Forms.View.Details;
            // 
            // _gameNameColumnHeader
            // 
            this._gameNameColumnHeader.Text = "Игра";
            this._gameNameColumnHeader.Width = 220;
            // 
            // _gameFolderColumnHeader
            // 
            this._gameFolderColumnHeader.Text = "Папка";
            this._gameFolderColumnHeader.Width = 620;
            // 
            // _actionsPanel
            // 
            this._layoutPanel.SetColumnSpan(this._actionsPanel, 4);
            this._actionsPanel.Controls.Add(this._openSelectedButton);
            this._actionsPanel.Controls.Add(this._openFolderButton);
            this._actionsPanel.Dock = DockStyle.Fill;
            this._actionsPanel.Location = new Point(15, 344);
            this._actionsPanel.Name = "_actionsPanel";
            this._actionsPanel.Size = new Size(870, 36);
            this._actionsPanel.TabIndex = 6;
            // 
            // _openSelectedButton
            // 
            this._openSelectedButton.Location = new Point(3, 3);
            this._openSelectedButton.Name = "_openSelectedButton";
            this._openSelectedButton.Size = new Size(150, 30);
            this._openSelectedButton.TabIndex = 0;
            this._openSelectedButton.Text = "Открыть выбранную";
            this._openSelectedButton.UseVisualStyleBackColor = true;
            // 
            // _openFolderButton
            // 
            this._openFolderButton.Location = new Point(159, 3);
            this._openFolderButton.Name = "_openFolderButton";
            this._openFolderButton.Size = new Size(180, 30);
            this._openFolderButton.TabIndex = 1;
            this._openFolderButton.Text = "Открыть папку вручную";
            this._openFolderButton.UseVisualStyleBackColor = true;
            // 
            // _infoTextBox
            // 
            this._layoutPanel.SetColumnSpan(this._infoTextBox, 4);
            this._infoTextBox.Dock = DockStyle.Fill;
            this._infoTextBox.Location = new Point(15, 386);
            this._infoTextBox.Multiline = true;
            this._infoTextBox.Name = "_infoTextBox";
            this._infoTextBox.ReadOnly = true;
            this._infoTextBox.ScrollBars = ScrollBars.Vertical;
            this._infoTextBox.Size = new Size(870, 159);
            this._infoTextBox.TabIndex = 7;
            // 
            // ProjectsPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._layoutPanel);
            this.Name = "ProjectsPageControl";
            this.Size = new Size(900, 560);
            this._layoutPanel.ResumeLayout(false);
            this._layoutPanel.PerformLayout();
            this._actionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
