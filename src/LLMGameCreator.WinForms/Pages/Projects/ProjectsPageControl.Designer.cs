#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class ProjectsPageControl
    {
        private IContainer components;
        private TableLayoutPanel _rootLayout;
        private Label _pageTitleLabel;
        private TableLayoutPanel _gamesRootPanel;
        private Label _gamesRootLabel;
        private TextBox _gamesRootTextBox;
        private Button _browseGamesRootButton;
        private Button _saveGamesRootButton;
        private Button _refreshButton;
        private Panel _contentPanel;
        private TableLayoutPanel _projectStartPanel;
        private Label _myGamesLabel;
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
        private TextBox _infoTextBox;
        private TableLayoutPanel _workspacePanel;
        private FlowLayoutPanel _workspaceToolbar;
        private Button _backToGamesButton;
        private Button _saveCurrentButton;
        private Button _regenerateGeneratedWorldButton;
        private Button _generatedWorldHistoryButton;
        private Label _workspaceTitleLabel;
        private TabControl _workspaceTabs;
        private TabPage _overviewTab;
        private TabPage _mechanicsTab;
        private TabPage _settingsTab;
        private TabPage _buildTab;
        private TabPage _technicalTab;
        private TableLayoutPanel _overviewLayout;
        private Label _overviewProjectLabel;
        private Label _overviewFolderLabel;
        private Label _overviewPackageStatusLabel;
        private Label _overviewAuthoringStatusLabel;
        private Label _overviewMechanicsCountLabel;
        private Label _overviewLastBuildLabel;
        private Label _overviewRuntimeLabel;
        private Panel _generatedWorldCardPanel;
        private Label _generatedWorldCardLabel;
        private Panel _generatedGameplaySavesCardPanel;
        private Label _generatedGameplaySavesCardLabel;
        private Button _manageGeneratedGameplaySavesButton;
        private FlowLayoutPanel _mechanicsFlow;
        private FlowLayoutPanel _settingsFlow;
        private TableLayoutPanel _buildLayout;
        private Button _buildAndQualifyButton;
        private Label _buildStatusLabel;
        private TextBox _buildResultTextBox;
        private Panel _socialCardPanel;
        private Label _socialCardLabel;
        private Panel _releaseCandidateCardPanel;
        private Label _releaseCandidateCardLabel;
        private TableLayoutPanel _standaloneLayout;
        private Label _unityEditorLabel;
        private TextBox _unityEditorPathTextBox;
        private Button _findUnityEditorButton;
        private Button _chooseUnityEditorButton;
        private FlowLayoutPanel _standaloneActionsPanel;
        private Button _buildWindowsStandaloneButton;
        private Button _cancelWindowsStandaloneButton;
        private Button _launchWindowsStandaloneButton;
        private Button _openWindowsStandaloneFolderButton;
        private TextBox _standaloneStatusTextBox;
        private TextBox _technicalDetailsTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._workspaceToolTip.Dispose();
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._rootLayout = new TableLayoutPanel();
            this._pageTitleLabel = new Label();
            this._gamesRootPanel = new TableLayoutPanel();
            this._gamesRootLabel = new Label();
            this._gamesRootTextBox = new TextBox();
            this._browseGamesRootButton = new Button();
            this._saveGamesRootButton = new Button();
            this._refreshButton = new Button();
            this._contentPanel = new Panel();
            this._projectStartPanel = new TableLayoutPanel();
            this._myGamesLabel = new Label();
            this._projectsListView = new ListView();
            this._gameNameColumnHeader = new ColumnHeader();
            this._packageIdColumnHeader = new ColumnHeader();
            this._versionColumnHeader = new ColumnHeader();
            this._statusColumnHeader = new ColumnHeader();
            this._gameFolderColumnHeader = new ColumnHeader();
            this._actionsPanel = new FlowLayoutPanel();
            this._newGameButton = new Button();
            this._openSelectedButton = new Button();
            this._openFolderButton = new Button();
            this._infoTextBox = new TextBox();
            this._workspacePanel = new TableLayoutPanel();
            this._workspaceToolbar = new FlowLayoutPanel();
            this._backToGamesButton = new Button();
            this._saveCurrentButton = new Button();
            this._regenerateGeneratedWorldButton = new Button();
            this._generatedWorldHistoryButton = new Button();
            this._workspaceTitleLabel = new Label();
            this._workspaceTabs = new TabControl();
            this._overviewTab = new TabPage();
            this._mechanicsTab = new TabPage();
            this._settingsTab = new TabPage();
            this._buildTab = new TabPage();
            this._technicalTab = new TabPage();
            this._overviewLayout = new TableLayoutPanel();
            this._overviewProjectLabel = new Label();
            this._overviewFolderLabel = new Label();
            this._overviewPackageStatusLabel = new Label();
            this._overviewAuthoringStatusLabel = new Label();
            this._overviewMechanicsCountLabel = new Label();
            this._overviewLastBuildLabel = new Label();
            this._overviewRuntimeLabel = new Label();
            this._generatedWorldCardPanel = new Panel();
            this._generatedWorldCardLabel = new Label();
            this._generatedGameplaySavesCardPanel = new Panel();
            this._generatedGameplaySavesCardLabel = new Label();
            this._manageGeneratedGameplaySavesButton = new Button();
            this._mechanicsFlow = new FlowLayoutPanel();
            this._settingsFlow = new FlowLayoutPanel();
            this._buildLayout = new TableLayoutPanel();
            this._buildAndQualifyButton = new Button();
            this._buildStatusLabel = new Label();
            this._buildResultTextBox = new TextBox();
            this._socialCardPanel = new Panel();
            this._socialCardLabel = new Label();
            this._releaseCandidateCardPanel = new Panel();
            this._releaseCandidateCardLabel = new Label();
            this._standaloneLayout = new TableLayoutPanel();
            this._unityEditorLabel = new Label();
            this._unityEditorPathTextBox = new TextBox();
            this._findUnityEditorButton = new Button();
            this._chooseUnityEditorButton = new Button();
            this._standaloneActionsPanel = new FlowLayoutPanel();
            this._buildWindowsStandaloneButton = new Button();
            this._cancelWindowsStandaloneButton = new Button();
            this._launchWindowsStandaloneButton = new Button();
            this._openWindowsStandaloneFolderButton = new Button();
            this._standaloneStatusTextBox = new TextBox();
            this._technicalDetailsTextBox = new TextBox();
            this._rootLayout.SuspendLayout();
            this._gamesRootPanel.SuspendLayout();
            this._contentPanel.SuspendLayout();
            this._projectStartPanel.SuspendLayout();
            this._actionsPanel.SuspendLayout();
            this._workspacePanel.SuspendLayout();
            this._workspaceToolbar.SuspendLayout();
            this._workspaceTabs.SuspendLayout();
            this._overviewTab.SuspendLayout();
            this._mechanicsTab.SuspendLayout();
            this._settingsTab.SuspendLayout();
            this._buildTab.SuspendLayout();
            this._technicalTab.SuspendLayout();
            this._overviewLayout.SuspendLayout();
            this._buildLayout.SuspendLayout();
            this._standaloneLayout.SuspendLayout();
            this._standaloneActionsPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _rootLayout
            //
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._pageTitleLabel, 0, 0);
            this._rootLayout.Controls.Add(this._gamesRootPanel, 0, 1);
            this._rootLayout.Controls.Add(this._contentPanel, 0, 2);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.Padding = new Padding(12);
            this._rootLayout.RowCount = 3;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            //
            // _pageTitleLabel
            //
            this._pageTitleLabel.Dock = DockStyle.Fill;
            this._pageTitleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this._pageTitleLabel.Text = "Мои игры";
            this._pageTitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _gamesRootPanel
            //
            this._gamesRootPanel.ColumnCount = 5;
            this._gamesRootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            this._gamesRootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._gamesRootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            this._gamesRootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            this._gamesRootPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            this._gamesRootPanel.Controls.Add(this._gamesRootLabel, 0, 0);
            this._gamesRootPanel.Controls.Add(this._gamesRootTextBox, 1, 0);
            this._gamesRootPanel.Controls.Add(this._browseGamesRootButton, 2, 0);
            this._gamesRootPanel.Controls.Add(this._saveGamesRootButton, 3, 0);
            this._gamesRootPanel.Controls.Add(this._refreshButton, 4, 0);
            this._gamesRootPanel.Dock = DockStyle.Fill;
            this._gamesRootLabel.Dock = DockStyle.Fill;
            this._gamesRootLabel.Text = "Папка с играми:";
            this._gamesRootLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._gamesRootTextBox.Dock = DockStyle.Fill;
            this._browseGamesRootButton.Dock = DockStyle.Fill;
            this._browseGamesRootButton.Text = "Выбрать...";
            this._browseGamesRootButton.UseVisualStyleBackColor = true;
            this._saveGamesRootButton.Dock = DockStyle.Fill;
            this._saveGamesRootButton.Text = "Сохранить";
            this._saveGamesRootButton.UseVisualStyleBackColor = true;
            this._refreshButton.Dock = DockStyle.Fill;
            this._refreshButton.Text = "Обновить";
            this._refreshButton.UseVisualStyleBackColor = true;
            //
            // _contentPanel
            //
            this._contentPanel.Controls.Add(this._workspacePanel);
            this._contentPanel.Controls.Add(this._projectStartPanel);
            this._contentPanel.Dock = DockStyle.Fill;
            //
            // _projectStartPanel
            //
            this._projectStartPanel.ColumnCount = 1;
            this._projectStartPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._projectStartPanel.Controls.Add(this._myGamesLabel, 0, 0);
            this._projectStartPanel.Controls.Add(this._projectsListView, 0, 1);
            this._projectStartPanel.Controls.Add(this._actionsPanel, 0, 2);
            this._projectStartPanel.Controls.Add(this._infoTextBox, 0, 3);
            this._projectStartPanel.Dock = DockStyle.Fill;
            this._projectStartPanel.RowCount = 4;
            this._projectStartPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._projectStartPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            this._projectStartPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._projectStartPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            this._myGamesLabel.Dock = DockStyle.Fill;
            this._myGamesLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._myGamesLabel.Text = "Мои игры";
            this._myGamesLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _projectsListView
            //
            this._projectsListView.Columns.AddRange(new ColumnHeader[] { this._gameNameColumnHeader, this._packageIdColumnHeader, this._versionColumnHeader, this._statusColumnHeader, this._gameFolderColumnHeader });
            this._projectsListView.Dock = DockStyle.Fill;
            this._projectsListView.FullRowSelect = true;
            this._projectsListView.MultiSelect = false;
            this._projectsListView.UseCompatibleStateImageBehavior = false;
            this._projectsListView.View = View.Details;
            this._gameNameColumnHeader.Text = "Игра";
            this._gameNameColumnHeader.Width = 190;
            this._packageIdColumnHeader.Text = "Идентификатор";
            this._packageIdColumnHeader.Width = 160;
            this._versionColumnHeader.Text = "Версия";
            this._versionColumnHeader.Width = 80;
            this._statusColumnHeader.Text = "Состояние";
            this._statusColumnHeader.Width = 190;
            this._gameFolderColumnHeader.Text = "Папка";
            this._gameFolderColumnHeader.Width = 320;
            //
            // _actionsPanel
            //
            this._actionsPanel.Controls.Add(this._newGameButton);
            this._actionsPanel.Controls.Add(this._openSelectedButton);
            this._actionsPanel.Controls.Add(this._openFolderButton);
            this._actionsPanel.Dock = DockStyle.Fill;
            this._newGameButton.AutoSize = true;
            this._newGameButton.Text = "Новая игра";
            this._newGameButton.UseVisualStyleBackColor = true;
            this._openSelectedButton.AutoSize = true;
            this._openSelectedButton.Text = "Открыть выбранную";
            this._openSelectedButton.UseVisualStyleBackColor = true;
            this._openFolderButton.AutoSize = true;
            this._openFolderButton.Text = "Открыть папку";
            this._openFolderButton.UseVisualStyleBackColor = true;
            this._infoTextBox.Dock = DockStyle.Fill;
            this._infoTextBox.Multiline = true;
            this._infoTextBox.ReadOnly = true;
            this._infoTextBox.ScrollBars = ScrollBars.Vertical;
            //
            // _workspacePanel
            //
            this._workspacePanel.ColumnCount = 1;
            this._workspacePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._workspacePanel.Controls.Add(this._workspaceToolbar, 0, 0);
            this._workspacePanel.Controls.Add(this._workspaceTabs, 0, 1);
            this._workspacePanel.Dock = DockStyle.Fill;
            this._workspacePanel.RowCount = 2;
            this._workspacePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._workspacePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._workspacePanel.Visible = false;
            //
            // _workspaceToolbar
            //
            this._workspaceToolbar.Controls.Add(this._backToGamesButton);
            this._workspaceToolbar.Controls.Add(this._saveCurrentButton);
            this._workspaceToolbar.Controls.Add(this._regenerateGeneratedWorldButton);
            this._workspaceToolbar.Controls.Add(this._generatedWorldHistoryButton);
            this._workspaceToolbar.Controls.Add(this._workspaceTitleLabel);
            this._workspaceToolbar.Dock = DockStyle.Fill;
            this._backToGamesButton.AutoSize = true;
            this._backToGamesButton.Text = "К списку игр";
            this._backToGamesButton.UseVisualStyleBackColor = true;
            this._saveCurrentButton.AutoSize = true;
            this._saveCurrentButton.Text = "Сохранить проект";
            this._saveCurrentButton.UseVisualStyleBackColor = true;
            this._regenerateGeneratedWorldButton.AutoSize = true;
            this._regenerateGeneratedWorldButton.Text = "Перегенерировать мир";
            this._regenerateGeneratedWorldButton.UseVisualStyleBackColor = true;
            this._generatedWorldHistoryButton.AutoSize = true;
            this._generatedWorldHistoryButton.Text = "История миров";
            this._generatedWorldHistoryButton.UseVisualStyleBackColor = true;
            this._workspaceTitleLabel.AutoSize = true;
            this._workspaceTitleLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._workspaceTitleLabel.Margin = new Padding(18, 7, 3, 3);
            //
            // _workspaceTabs
            //
            this._workspaceTabs.Controls.Add(this._overviewTab);
            this._workspaceTabs.Controls.Add(this._mechanicsTab);
            this._workspaceTabs.Controls.Add(this._settingsTab);
            this._workspaceTabs.Controls.Add(this._buildTab);
            this._workspaceTabs.Controls.Add(this._technicalTab);
            this._workspaceTabs.Dock = DockStyle.Fill;
            this._overviewTab.Text = "Обзор";
            this._mechanicsTab.Text = "Механики";
            this._settingsTab.Text = "Настройки";
            this._buildTab.Text = "Сборка и проверка";
            this._technicalTab.Text = "Технические детали";
            //
            // _overviewTab
            //
            this._overviewTab.Controls.Add(this._overviewLayout);
            this._overviewLayout.ColumnCount = 1;
            this._overviewLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._overviewLayout.Controls.Add(this._overviewProjectLabel, 0, 0);
            this._overviewLayout.Controls.Add(this._overviewFolderLabel, 0, 1);
            this._overviewLayout.Controls.Add(this._overviewPackageStatusLabel, 0, 2);
            this._overviewLayout.Controls.Add(this._overviewAuthoringStatusLabel, 0, 3);
            this._overviewLayout.Controls.Add(this._overviewMechanicsCountLabel, 0, 4);
            this._overviewLayout.Controls.Add(this._overviewLastBuildLabel, 0, 5);
            this._overviewLayout.Controls.Add(this._overviewRuntimeLabel, 0, 6);
            this._overviewLayout.Controls.Add(this._generatedWorldCardPanel, 0, 7);
            this._overviewLayout.Controls.Add(this._generatedGameplaySavesCardPanel, 0, 8);
            this._overviewLayout.Dock = DockStyle.Fill;
            this._overviewLayout.Padding = new Padding(18);
            this._overviewLayout.RowCount = 9;
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            this._overviewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            this._overviewProjectLabel.Dock = DockStyle.Fill;
            this._overviewProjectLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this._overviewFolderLabel.Dock = DockStyle.Fill;
            this._overviewPackageStatusLabel.Dock = DockStyle.Fill;
            this._overviewAuthoringStatusLabel.Dock = DockStyle.Fill;
            this._overviewMechanicsCountLabel.Dock = DockStyle.Fill;
            this._overviewLastBuildLabel.Dock = DockStyle.Fill;
            this._overviewRuntimeLabel.Dock = DockStyle.Fill;
            this._generatedWorldCardPanel.AutoScroll = true;
            this._generatedWorldCardPanel.BackColor = Color.FromArgb(245, 249, 255);
            this._generatedWorldCardPanel.BorderStyle = BorderStyle.FixedSingle;
            this._generatedWorldCardPanel.Controls.Add(this._generatedWorldCardLabel);
            this._generatedWorldCardPanel.Dock = DockStyle.Fill;
            this._generatedWorldCardPanel.Margin = new Padding(0, 8, 0, 0);
            this._generatedWorldCardPanel.Padding = new Padding(12);
            this._generatedWorldCardPanel.Visible = false;
            this._generatedWorldCardLabel.AutoSize = false;
            this._generatedWorldCardLabel.Dock = DockStyle.Fill;
            this._generatedWorldCardLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this._generatedWorldCardLabel.TextAlign = ContentAlignment.TopLeft;
            this._generatedWorldCardLabel.UseMnemonic = false;
            //
            // _generatedGameplaySavesCardPanel
            //
            this._generatedGameplaySavesCardPanel.AutoScroll = true;
            this._generatedGameplaySavesCardPanel.BackColor = Color.FromArgb(248, 250, 246);
            this._generatedGameplaySavesCardPanel.BorderStyle = BorderStyle.FixedSingle;
            this._generatedGameplaySavesCardPanel.Controls.Add(this._generatedGameplaySavesCardLabel);
            this._generatedGameplaySavesCardPanel.Controls.Add(this._manageGeneratedGameplaySavesButton);
            this._generatedGameplaySavesCardPanel.Dock = DockStyle.Fill;
            this._generatedGameplaySavesCardPanel.Margin = new Padding(0, 8, 0, 0);
            this._generatedGameplaySavesCardPanel.Padding = new Padding(12);
            this._generatedGameplaySavesCardPanel.Visible = false;
            this._generatedGameplaySavesCardLabel.AutoSize = false;
            this._generatedGameplaySavesCardLabel.Dock = DockStyle.Fill;
            this._generatedGameplaySavesCardLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this._generatedGameplaySavesCardLabel.TextAlign = ContentAlignment.TopLeft;
            this._generatedGameplaySavesCardLabel.UseMnemonic = false;
            this._manageGeneratedGameplaySavesButton.AutoSize = true;
            this._manageGeneratedGameplaySavesButton.Dock = DockStyle.Bottom;
            this._manageGeneratedGameplaySavesButton.Text = "Управление сохранениями";
            this._manageGeneratedGameplaySavesButton.UseVisualStyleBackColor = true;
            //
            // dynamic sections
            //
            this._mechanicsTab.Controls.Add(this._mechanicsFlow);
            this._mechanicsFlow.AutoScroll = true;
            this._mechanicsFlow.Dock = DockStyle.Fill;
            this._mechanicsFlow.FlowDirection = FlowDirection.TopDown;
            this._mechanicsFlow.Padding = new Padding(12);
            this._mechanicsFlow.WrapContents = false;
            this._settingsTab.Controls.Add(this._settingsFlow);
            this._settingsFlow.AutoScroll = true;
            this._settingsFlow.Dock = DockStyle.Fill;
            this._settingsFlow.FlowDirection = FlowDirection.TopDown;
            this._settingsFlow.Padding = new Padding(12);
            this._settingsFlow.WrapContents = false;
            //
            // _buildTab
            //
            this._buildTab.Controls.Add(this._buildLayout);
            this._buildLayout.ColumnCount = 1;
            this._buildLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._buildLayout.Controls.Add(this._buildAndQualifyButton, 0, 0);
            this._buildLayout.Controls.Add(this._buildStatusLabel, 0, 1);
            this._buildLayout.Controls.Add(this._buildResultTextBox, 0, 2);
            this._buildLayout.Controls.Add(this._socialCardPanel, 0, 3);
            this._buildLayout.Controls.Add(this._releaseCandidateCardPanel, 0, 3);
            this._buildLayout.Controls.Add(this._standaloneLayout, 0, 4);
            this._buildLayout.Dock = DockStyle.Fill;
            this._buildLayout.Padding = new Padding(18);
            this._buildLayout.RowCount = 5;
            this._buildLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            this._buildLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._buildLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            this._buildLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 235F));
            this._buildLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            this._buildAndQualifyButton.Dock = DockStyle.Left;
            this._buildAndQualifyButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._buildAndQualifyButton.Size = new Size(270, 42);
            this._buildAndQualifyButton.Text = "Собрать и проверить игру";
            this._buildAndQualifyButton.UseVisualStyleBackColor = true;
            this._buildStatusLabel.Dock = DockStyle.Fill;
            this._buildStatusLabel.Text = "Проверка ещё не запускалась";
            this._buildStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._buildResultTextBox.Dock = DockStyle.Fill;
            this._buildResultTextBox.Multiline = true;
            this._buildResultTextBox.ReadOnly = true;
            this._buildResultTextBox.ScrollBars = ScrollBars.Vertical;
            //
            // _socialCardPanel
            //
            this._socialCardPanel.BorderStyle = BorderStyle.FixedSingle;
            this._socialCardPanel.Controls.Add(this._socialCardLabel);
            this._socialCardPanel.Dock = DockStyle.Fill;
            this._socialCardPanel.Margin = new Padding(0, 8, 0, 8);
            this._socialCardPanel.Padding = new Padding(12);
            this._socialCardPanel.Visible = false;
            this._socialCardLabel.AutoSize = false;
            this._socialCardLabel.Dock = DockStyle.Fill;
            this._socialCardLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this._socialCardLabel.TextAlign = ContentAlignment.TopLeft;
            //
            // _releaseCandidateCardPanel
            //
            this._releaseCandidateCardPanel.BorderStyle = BorderStyle.FixedSingle;
            this._releaseCandidateCardPanel.Controls.Add(this._releaseCandidateCardLabel);
            this._releaseCandidateCardPanel.Dock = DockStyle.Fill;
            this._releaseCandidateCardPanel.Margin = new Padding(0, 8, 0, 8);
            this._releaseCandidateCardPanel.Padding = new Padding(12);
            this._releaseCandidateCardPanel.Visible = false;
            this._releaseCandidateCardLabel.AutoSize = false;
            this._releaseCandidateCardLabel.Dock = DockStyle.Fill;
            this._releaseCandidateCardLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this._releaseCandidateCardLabel.TextAlign = ContentAlignment.TopLeft;
            //
            // _standaloneLayout
            //
            this._standaloneLayout.ColumnCount = 4;
            this._standaloneLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            this._standaloneLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._standaloneLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            this._standaloneLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            this._standaloneLayout.Controls.Add(this._unityEditorLabel, 0, 0);
            this._standaloneLayout.Controls.Add(this._unityEditorPathTextBox, 1, 0);
            this._standaloneLayout.Controls.Add(this._findUnityEditorButton, 2, 0);
            this._standaloneLayout.Controls.Add(this._chooseUnityEditorButton, 3, 0);
            this._standaloneLayout.Controls.Add(this._standaloneActionsPanel, 0, 1);
            this._standaloneLayout.Controls.Add(this._standaloneStatusTextBox, 0, 2);
            this._standaloneLayout.SetColumnSpan(this._standaloneActionsPanel, 4);
            this._standaloneLayout.SetColumnSpan(this._standaloneStatusTextBox, 4);
            this._standaloneLayout.Dock = DockStyle.Fill;
            this._standaloneLayout.Margin = new Padding(0, 8, 0, 0);
            this._standaloneLayout.RowCount = 3;
            this._standaloneLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._standaloneLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._standaloneLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._unityEditorLabel.Dock = DockStyle.Fill;
            this._unityEditorLabel.Text = "Unity Editor:";
            this._unityEditorLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._unityEditorPathTextBox.Dock = DockStyle.Fill;
            this._unityEditorPathTextBox.ReadOnly = true;
            this._findUnityEditorButton.Dock = DockStyle.Fill;
            this._findUnityEditorButton.Text = "Найти автоматически";
            this._findUnityEditorButton.UseVisualStyleBackColor = true;
            this._chooseUnityEditorButton.Dock = DockStyle.Fill;
            this._chooseUnityEditorButton.Text = "Выбрать...";
            this._chooseUnityEditorButton.UseVisualStyleBackColor = true;
            this._standaloneActionsPanel.Controls.Add(this._buildWindowsStandaloneButton);
            this._standaloneActionsPanel.Controls.Add(this._cancelWindowsStandaloneButton);
            this._standaloneActionsPanel.Controls.Add(this._launchWindowsStandaloneButton);
            this._standaloneActionsPanel.Controls.Add(this._openWindowsStandaloneFolderButton);
            this._standaloneActionsPanel.Dock = DockStyle.Fill;
            this._buildWindowsStandaloneButton.AutoSize = true;
            this._buildWindowsStandaloneButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this._buildWindowsStandaloneButton.Text = "Собрать Windows-игру (Alpha)";
            this._buildWindowsStandaloneButton.UseVisualStyleBackColor = true;
            this._cancelWindowsStandaloneButton.AutoSize = true;
            this._cancelWindowsStandaloneButton.Text = "Отменить";
            this._cancelWindowsStandaloneButton.UseVisualStyleBackColor = true;
            this._launchWindowsStandaloneButton.AutoSize = true;
            this._launchWindowsStandaloneButton.Text = "Запустить игру";
            this._launchWindowsStandaloneButton.UseVisualStyleBackColor = true;
            this._openWindowsStandaloneFolderButton.AutoSize = true;
            this._openWindowsStandaloneFolderButton.Text = "Открыть папку сборки";
            this._openWindowsStandaloneFolderButton.UseVisualStyleBackColor = true;
            this._standaloneStatusTextBox.Dock = DockStyle.Fill;
            this._standaloneStatusTextBox.Multiline = true;
            this._standaloneStatusTextBox.ReadOnly = true;
            this._standaloneStatusTextBox.ScrollBars = ScrollBars.Vertical;
            //
            // _technicalTab
            //
            this._technicalTab.Controls.Add(this._technicalDetailsTextBox);
            this._technicalDetailsTextBox.Dock = DockStyle.Fill;
            this._technicalDetailsTextBox.Font = new Font("Consolas", 9F);
            this._technicalDetailsTextBox.Multiline = true;
            this._technicalDetailsTextBox.ReadOnly = true;
            this._technicalDetailsTextBox.ScrollBars = ScrollBars.Both;
            this._technicalDetailsTextBox.WordWrap = false;
            //
            // ProjectsPageControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "ProjectsPageControl";
            this.Size = new Size(1100, 720);
            this._rootLayout.ResumeLayout(false);
            this._gamesRootPanel.ResumeLayout(false);
            this._gamesRootPanel.PerformLayout();
            this._contentPanel.ResumeLayout(false);
            this._projectStartPanel.ResumeLayout(false);
            this._projectStartPanel.PerformLayout();
            this._actionsPanel.ResumeLayout(false);
            this._actionsPanel.PerformLayout();
            this._workspacePanel.ResumeLayout(false);
            this._workspaceToolbar.ResumeLayout(false);
            this._workspaceToolbar.PerformLayout();
            this._workspaceTabs.ResumeLayout(false);
            this._overviewTab.ResumeLayout(false);
            this._mechanicsTab.ResumeLayout(false);
            this._settingsTab.ResumeLayout(false);
            this._buildTab.ResumeLayout(false);
            this._technicalTab.ResumeLayout(false);
            this._overviewLayout.ResumeLayout(false);
            this._buildLayout.ResumeLayout(false);
            this._buildLayout.PerformLayout();
            this._standaloneLayout.ResumeLayout(false);
            this._standaloneLayout.PerformLayout();
            this._standaloneActionsPanel.ResumeLayout(false);
            this._standaloneActionsPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
