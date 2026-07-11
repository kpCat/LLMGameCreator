namespace LLMGameCreator.WinForms.Pages
{
    partial class UnityArchiveReviewPageControl
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel _rootLayout;
        private FlowLayoutPanel _toolbarPanel;
        private Button _refreshButton;
        private Button _openArchiveFolderButton;
        private SplitContainer _splitContainer;
        private TableLayoutPanel _leftLayout;
        private Label _projectFolderLabel;
        private Label _projectFolderValueLabel;
        private Label _archiveRootLabel;
        private Label _archiveRootValueLabel;
        private Label _currentReviewReadinessLabel;
        private Label _currentReviewReadinessValueLabel;
        private Label _comparisonReadinessLabel;
        private Label _comparisonReadinessValueLabel;
        private Label _historyCountLabel;
        private Label _historyCountValueLabel;
        private Label _historySnapshotsLabel;
        private ListBox _historySnapshotsList;
        private Label _statusSummaryLabel;
        private TextBox _statusSummaryTextBox;
        private TabControl _reportTabs;
        private TabPage _currentReviewMarkdownTab;
        private TabPage _comparisonMarkdownTab;
        private TabPage _currentReviewJsonTab;
        private TabPage _comparisonJsonTab;
        private TabPage _historyIndexJsonTab;
        private TabPage _selectedSnapshotJsonTab;
        private TabPage _manualImportWorkspaceTab;
        private TabPage _manualImportMarkdownTab;
        private TabPage _manualImportJsonTab;
        private TextBox _currentReviewMarkdownTextBox;
        private TextBox _comparisonMarkdownTextBox;
        private TextBox _currentReviewJsonTextBox;
        private TextBox _comparisonJsonTextBox;
        private TextBox _historyIndexJsonTextBox;
        private TableLayoutPanel _selectedSnapshotLayout;
        private Label _selectedSnapshotInfoLabel;
        private TextBox _selectedSnapshotJsonTextBox;
        private TextBox _manualImportMarkdownTextBox;
        private TextBox _manualImportJsonTextBox;
        private TableLayoutPanel _manualImportWorkspaceLayout;
        private FlowLayoutPanel _manualImportToolbarPanel;
        private Label _manualImportFilterLabel;
        private ComboBox _manualImportFilterComboBox;
        private Button _createManifestTemplateButton;
        private Button _openManualImportFolderButton;
        private Button _runManualImportButton;
        private CheckBox _allowOverwriteCheckBox;
        private Button _copySlotIdButton;
        private Button _copyExpectedPathButton;
        private DataGridView _manualImportSlotsGrid;
        private DataGridViewTextBoxColumn _slotIdColumn;
        private DataGridViewTextBoxColumn _slotKindColumn;
        private DataGridViewTextBoxColumn _slotProviderColumn;
        private DataGridViewTextBoxColumn _slotExpectedPathColumn;
        private DataGridViewTextBoxColumn _slotStatusColumn;
        private DataGridViewCheckBoxColumn _slotFileExistsColumn;
        private DataGridViewTextBoxColumn _slotFileSizeColumn;
        private DataGridViewTextBoxColumn _slotHashColumn;
        private DataGridViewTextBoxColumn _slotSourceColumn;
        private TextBox _manualImportSlotDetailTextBox;
        private Label _manualImportWorkspaceStatusLabel;
        private Label _manualImportReportStatusLabel;
        private Label _statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeRuntime();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._rootLayout = new TableLayoutPanel();
            this._toolbarPanel = new FlowLayoutPanel();
            this._refreshButton = new Button();
            this._openArchiveFolderButton = new Button();
            this._splitContainer = new SplitContainer();
            this._leftLayout = new TableLayoutPanel();
            this._projectFolderLabel = new Label();
            this._projectFolderValueLabel = new Label();
            this._archiveRootLabel = new Label();
            this._archiveRootValueLabel = new Label();
            this._currentReviewReadinessLabel = new Label();
            this._currentReviewReadinessValueLabel = new Label();
            this._comparisonReadinessLabel = new Label();
            this._comparisonReadinessValueLabel = new Label();
            this._historyCountLabel = new Label();
            this._historyCountValueLabel = new Label();
            this._historySnapshotsLabel = new Label();
            this._historySnapshotsList = new ListBox();
            this._statusSummaryLabel = new Label();
            this._statusSummaryTextBox = new TextBox();
            this._reportTabs = new TabControl();
            this._currentReviewMarkdownTab = new TabPage();
            this._comparisonMarkdownTab = new TabPage();
            this._currentReviewJsonTab = new TabPage();
            this._comparisonJsonTab = new TabPage();
            this._historyIndexJsonTab = new TabPage();
            this._selectedSnapshotJsonTab = new TabPage();
            this._manualImportWorkspaceTab = new TabPage();
            this._manualImportMarkdownTab = new TabPage();
            this._manualImportJsonTab = new TabPage();
            this._currentReviewMarkdownTextBox = new TextBox();
            this._comparisonMarkdownTextBox = new TextBox();
            this._currentReviewJsonTextBox = new TextBox();
            this._comparisonJsonTextBox = new TextBox();
            this._historyIndexJsonTextBox = new TextBox();
            this._selectedSnapshotLayout = new TableLayoutPanel();
            this._selectedSnapshotInfoLabel = new Label();
            this._selectedSnapshotJsonTextBox = new TextBox();
            this._manualImportMarkdownTextBox = new TextBox();
            this._manualImportJsonTextBox = new TextBox();
            this._manualImportWorkspaceLayout = new TableLayoutPanel();
            this._manualImportToolbarPanel = new FlowLayoutPanel();
            this._manualImportFilterLabel = new Label();
            this._manualImportFilterComboBox = new ComboBox();
            this._createManifestTemplateButton = new Button();
            this._openManualImportFolderButton = new Button();
            this._runManualImportButton = new Button();
            this._allowOverwriteCheckBox = new CheckBox();
            this._copySlotIdButton = new Button();
            this._copyExpectedPathButton = new Button();
            this._manualImportSlotsGrid = new DataGridView();
            this._slotIdColumn = new DataGridViewTextBoxColumn();
            this._slotKindColumn = new DataGridViewTextBoxColumn();
            this._slotProviderColumn = new DataGridViewTextBoxColumn();
            this._slotExpectedPathColumn = new DataGridViewTextBoxColumn();
            this._slotStatusColumn = new DataGridViewTextBoxColumn();
            this._slotFileExistsColumn = new DataGridViewCheckBoxColumn();
            this._slotFileSizeColumn = new DataGridViewTextBoxColumn();
            this._slotHashColumn = new DataGridViewTextBoxColumn();
            this._slotSourceColumn = new DataGridViewTextBoxColumn();
            this._manualImportSlotDetailTextBox = new TextBox();
            this._manualImportWorkspaceStatusLabel = new Label();
            this._manualImportReportStatusLabel = new Label();
            this._statusLabel = new Label();
            this._rootLayout.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._splitContainer).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._leftLayout.SuspendLayout();
            this._reportTabs.SuspendLayout();
            this._currentReviewMarkdownTab.SuspendLayout();
            this._comparisonMarkdownTab.SuspendLayout();
            this._currentReviewJsonTab.SuspendLayout();
            this._comparisonJsonTab.SuspendLayout();
            this._historyIndexJsonTab.SuspendLayout();
            this._selectedSnapshotJsonTab.SuspendLayout();
            this._manualImportWorkspaceTab.SuspendLayout();
            this._manualImportMarkdownTab.SuspendLayout();
            this._manualImportJsonTab.SuspendLayout();
            this._selectedSnapshotLayout.SuspendLayout();
            this._manualImportWorkspaceLayout.SuspendLayout();
            this._manualImportToolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._manualImportSlotsGrid).BeginInit();
            this.SuspendLayout();
            //
            // _rootLayout
            //
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._toolbarPanel, 0, 0);
            this._rootLayout.Controls.Add(this._splitContainer, 0, 1);
            this._rootLayout.Controls.Add(this._statusLabel, 0, 2);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.RowCount = 3;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            //
            // _toolbarPanel
            //
            this._toolbarPanel.Controls.Add(this._refreshButton);
            this._toolbarPanel.Controls.Add(this._openArchiveFolderButton);
            this._toolbarPanel.Dock = DockStyle.Fill;
            this._toolbarPanel.Padding = new Padding(6, 7, 6, 4);
            this._toolbarPanel.WrapContents = false;
            this._refreshButton.AutoSize = true;
            this._refreshButton.Text = "Refresh";
            this._openArchiveFolderButton.AutoSize = true;
            this._openArchiveFolderButton.Text = "Open archive folder";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.FixedPanel = FixedPanel.Panel1;
            this._splitContainer.Panel1.Controls.Add(this._leftLayout);
            this._splitContainer.Panel2.Controls.Add(this._reportTabs);
            this._splitContainer.SplitterDistance = 380;
            //
            // _leftLayout
            //
            this._leftLayout.ColumnCount = 2;
            this._leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155F));
            this._leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._leftLayout.Controls.Add(this._projectFolderLabel, 0, 0);
            this._leftLayout.Controls.Add(this._projectFolderValueLabel, 0, 1);
            this._leftLayout.SetColumnSpan(this._projectFolderValueLabel, 2);
            this._leftLayout.Controls.Add(this._archiveRootLabel, 0, 2);
            this._leftLayout.Controls.Add(this._archiveRootValueLabel, 0, 3);
            this._leftLayout.SetColumnSpan(this._archiveRootValueLabel, 2);
            this._leftLayout.Controls.Add(this._currentReviewReadinessLabel, 0, 4);
            this._leftLayout.Controls.Add(this._currentReviewReadinessValueLabel, 1, 4);
            this._leftLayout.Controls.Add(this._comparisonReadinessLabel, 0, 5);
            this._leftLayout.Controls.Add(this._comparisonReadinessValueLabel, 1, 5);
            this._leftLayout.Controls.Add(this._historyCountLabel, 0, 6);
            this._leftLayout.Controls.Add(this._historyCountValueLabel, 1, 6);
            this._leftLayout.Controls.Add(this._historySnapshotsLabel, 0, 7);
            this._leftLayout.SetColumnSpan(this._historySnapshotsLabel, 2);
            this._leftLayout.Controls.Add(this._historySnapshotsList, 0, 8);
            this._leftLayout.SetColumnSpan(this._historySnapshotsList, 2);
            this._leftLayout.Controls.Add(this._statusSummaryLabel, 0, 9);
            this._leftLayout.SetColumnSpan(this._statusSummaryLabel, 2);
            this._leftLayout.Controls.Add(this._statusSummaryTextBox, 0, 10);
            this._leftLayout.SetColumnSpan(this._statusSummaryTextBox, 2);
            this._leftLayout.Dock = DockStyle.Fill;
            this._leftLayout.Padding = new Padding(8);
            this._leftLayout.RowCount = 11;
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            //
            // left controls
            //
            this._projectFolderLabel.AutoSize = true;
            this._projectFolderLabel.Text = "Project folder";
            this._projectFolderValueLabel.AutoEllipsis = true;
            this._projectFolderValueLabel.Dock = DockStyle.Fill;
            this._projectFolderValueLabel.Text = "Not available";
            this._archiveRootLabel.AutoSize = true;
            this._archiveRootLabel.Text = "Archive root";
            this._archiveRootValueLabel.AutoEllipsis = true;
            this._archiveRootValueLabel.Dock = DockStyle.Fill;
            this._archiveRootValueLabel.Text = "Not available";
            this._currentReviewReadinessLabel.AutoSize = true;
            this._currentReviewReadinessLabel.Text = "Current review";
            this._currentReviewReadinessValueLabel.AutoSize = true;
            this._currentReviewReadinessValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._currentReviewReadinessValueLabel.Text = "Unavailable";
            this._comparisonReadinessLabel.AutoSize = true;
            this._comparisonReadinessLabel.Text = "Comparison";
            this._comparisonReadinessValueLabel.AutoSize = true;
            this._comparisonReadinessValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._comparisonReadinessValueLabel.Text = "Unavailable";
            this._historyCountLabel.AutoSize = true;
            this._historyCountLabel.Text = "History snapshots";
            this._historyCountValueLabel.AutoSize = true;
            this._historyCountValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._historyCountValueLabel.Text = "0";
            this._historySnapshotsLabel.AutoSize = true;
            this._historySnapshotsLabel.Margin = new Padding(3, 7, 3, 0);
            this._historySnapshotsLabel.Text = "Snapshot list";
            this._historySnapshotsList.Dock = DockStyle.Fill;
            this._historySnapshotsList.IntegralHeight = false;
            this._statusSummaryLabel.AutoSize = true;
            this._statusSummaryLabel.Margin = new Padding(3, 7, 3, 0);
            this._statusSummaryLabel.Text = "Missing files / status";
            this._statusSummaryTextBox.Dock = DockStyle.Fill;
            this._statusSummaryTextBox.Multiline = true;
            this._statusSummaryTextBox.ReadOnly = true;
            this._statusSummaryTextBox.ScrollBars = ScrollBars.Vertical;
            //
            // _reportTabs
            //
            this._reportTabs.Controls.Add(this._currentReviewMarkdownTab);
            this._reportTabs.Controls.Add(this._comparisonMarkdownTab);
            this._reportTabs.Controls.Add(this._currentReviewJsonTab);
            this._reportTabs.Controls.Add(this._comparisonJsonTab);
            this._reportTabs.Controls.Add(this._historyIndexJsonTab);
            this._reportTabs.Controls.Add(this._selectedSnapshotJsonTab);
            this._reportTabs.Controls.Add(this._manualImportWorkspaceTab);
            this._reportTabs.Controls.Add(this._manualImportMarkdownTab);
            this._reportTabs.Controls.Add(this._manualImportJsonTab);
            this._reportTabs.Dock = DockStyle.Fill;
            this._currentReviewMarkdownTab.Controls.Add(this._currentReviewMarkdownTextBox);
            this._currentReviewMarkdownTab.Text = "Current Review";
            this._comparisonMarkdownTab.Controls.Add(this._comparisonMarkdownTextBox);
            this._comparisonMarkdownTab.Text = "Comparison";
            this._currentReviewJsonTab.Controls.Add(this._currentReviewJsonTextBox);
            this._currentReviewJsonTab.Text = "Current Review JSON";
            this._comparisonJsonTab.Controls.Add(this._comparisonJsonTextBox);
            this._comparisonJsonTab.Text = "Comparison JSON";
            this._historyIndexJsonTab.Controls.Add(this._historyIndexJsonTextBox);
            this._historyIndexJsonTab.Text = "History Index JSON";
            this._selectedSnapshotJsonTab.Controls.Add(this._selectedSnapshotLayout);
            this._selectedSnapshotJsonTab.Text = "Selected Snapshot JSON";
            this._manualImportWorkspaceTab.Controls.Add(this._manualImportWorkspaceLayout);
            this._manualImportWorkspaceTab.Text = "Manual Import Workspace";
            this._manualImportMarkdownTab.Controls.Add(this._manualImportMarkdownTextBox);
            this._manualImportMarkdownTab.Text = "Manual Import";
            this._manualImportJsonTab.Controls.Add(this._manualImportJsonTextBox);
            this._manualImportJsonTab.Text = "Manual Import JSON";
            //
            // report text boxes
            //
            this._currentReviewMarkdownTextBox.Dock = DockStyle.Fill;
            this._currentReviewMarkdownTextBox.Font = new Font("Consolas", 10F);
            this._currentReviewMarkdownTextBox.Multiline = true;
            this._currentReviewMarkdownTextBox.ReadOnly = true;
            this._currentReviewMarkdownTextBox.ScrollBars = ScrollBars.Both;
            this._currentReviewMarkdownTextBox.WordWrap = false;
            this._comparisonMarkdownTextBox.Dock = DockStyle.Fill;
            this._comparisonMarkdownTextBox.Font = new Font("Consolas", 10F);
            this._comparisonMarkdownTextBox.Multiline = true;
            this._comparisonMarkdownTextBox.ReadOnly = true;
            this._comparisonMarkdownTextBox.ScrollBars = ScrollBars.Both;
            this._comparisonMarkdownTextBox.WordWrap = false;
            this._currentReviewJsonTextBox.Dock = DockStyle.Fill;
            this._currentReviewJsonTextBox.Font = new Font("Consolas", 10F);
            this._currentReviewJsonTextBox.Multiline = true;
            this._currentReviewJsonTextBox.ReadOnly = true;
            this._currentReviewJsonTextBox.ScrollBars = ScrollBars.Both;
            this._currentReviewJsonTextBox.WordWrap = false;
            this._comparisonJsonTextBox.Dock = DockStyle.Fill;
            this._comparisonJsonTextBox.Font = new Font("Consolas", 10F);
            this._comparisonJsonTextBox.Multiline = true;
            this._comparisonJsonTextBox.ReadOnly = true;
            this._comparisonJsonTextBox.ScrollBars = ScrollBars.Both;
            this._comparisonJsonTextBox.WordWrap = false;
            this._historyIndexJsonTextBox.Dock = DockStyle.Fill;
            this._historyIndexJsonTextBox.Font = new Font("Consolas", 10F);
            this._historyIndexJsonTextBox.Multiline = true;
            this._historyIndexJsonTextBox.ReadOnly = true;
            this._historyIndexJsonTextBox.ScrollBars = ScrollBars.Both;
            this._historyIndexJsonTextBox.WordWrap = false;
            this._selectedSnapshotLayout.ColumnCount = 1;
            this._selectedSnapshotLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._selectedSnapshotLayout.Controls.Add(this._selectedSnapshotInfoLabel, 0, 0);
            this._selectedSnapshotLayout.Controls.Add(this._selectedSnapshotJsonTextBox, 0, 1);
            this._selectedSnapshotLayout.Dock = DockStyle.Fill;
            this._selectedSnapshotLayout.RowCount = 2;
            this._selectedSnapshotLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._selectedSnapshotLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._selectedSnapshotInfoLabel.AutoEllipsis = true;
            this._selectedSnapshotInfoLabel.Dock = DockStyle.Fill;
            this._selectedSnapshotInfoLabel.Padding = new Padding(6, 8, 6, 0);
            this._selectedSnapshotInfoLabel.Text = "Status: Unavailable";
            this._selectedSnapshotJsonTextBox.Dock = DockStyle.Fill;
            this._selectedSnapshotJsonTextBox.Font = new Font("Consolas", 10F);
            this._selectedSnapshotJsonTextBox.Multiline = true;
            this._selectedSnapshotJsonTextBox.ReadOnly = true;
            this._selectedSnapshotJsonTextBox.ScrollBars = ScrollBars.Both;
            this._selectedSnapshotJsonTextBox.WordWrap = false;
            this._manualImportMarkdownTextBox.Dock = DockStyle.Fill;
            this._manualImportMarkdownTextBox.Font = new Font("Consolas", 10F);
            this._manualImportMarkdownTextBox.Multiline = true;
            this._manualImportMarkdownTextBox.ReadOnly = true;
            this._manualImportMarkdownTextBox.ScrollBars = ScrollBars.Both;
            this._manualImportMarkdownTextBox.WordWrap = false;
            this._manualImportJsonTextBox.Dock = DockStyle.Fill;
            this._manualImportJsonTextBox.Font = new Font("Consolas", 10F);
            this._manualImportJsonTextBox.Multiline = true;
            this._manualImportJsonTextBox.ReadOnly = true;
            this._manualImportJsonTextBox.ScrollBars = ScrollBars.Both;
            this._manualImportJsonTextBox.WordWrap = false;
            //
            // manual import workspace
            //
            this._manualImportWorkspaceLayout.ColumnCount = 1;
            this._manualImportWorkspaceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._manualImportWorkspaceLayout.Controls.Add(this._manualImportToolbarPanel, 0, 0);
            this._manualImportWorkspaceLayout.Controls.Add(this._manualImportSlotsGrid, 0, 1);
            this._manualImportWorkspaceLayout.Controls.Add(this._manualImportSlotDetailTextBox, 0, 2);
            this._manualImportWorkspaceLayout.Controls.Add(this._manualImportWorkspaceStatusLabel, 0, 3);
            this._manualImportWorkspaceLayout.Controls.Add(this._manualImportReportStatusLabel, 0, 4);
            this._manualImportWorkspaceLayout.Dock = DockStyle.Fill;
            this._manualImportWorkspaceLayout.RowCount = 5;
            this._manualImportWorkspaceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            this._manualImportWorkspaceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            this._manualImportWorkspaceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            this._manualImportWorkspaceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._manualImportWorkspaceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._manualImportToolbarPanel.AutoScroll = true;
            this._manualImportToolbarPanel.Controls.Add(this._manualImportFilterLabel);
            this._manualImportToolbarPanel.Controls.Add(this._manualImportFilterComboBox);
            this._manualImportToolbarPanel.Controls.Add(this._createManifestTemplateButton);
            this._manualImportToolbarPanel.Controls.Add(this._openManualImportFolderButton);
            this._manualImportToolbarPanel.Controls.Add(this._runManualImportButton);
            this._manualImportToolbarPanel.Controls.Add(this._allowOverwriteCheckBox);
            this._manualImportToolbarPanel.Controls.Add(this._copySlotIdButton);
            this._manualImportToolbarPanel.Controls.Add(this._copyExpectedPathButton);
            this._manualImportToolbarPanel.Dock = DockStyle.Fill;
            this._manualImportToolbarPanel.Padding = new Padding(6, 7, 6, 4);
            this._manualImportFilterLabel.AutoSize = true;
            this._manualImportFilterLabel.Margin = new Padding(3, 7, 3, 0);
            this._manualImportFilterLabel.Text = "Filter";
            this._manualImportFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._manualImportFilterComboBox.Name = "_manualImportFilterComboBox";
            this._manualImportFilterComboBox.Width = 150;
            this._createManifestTemplateButton.AutoSize = true;
            this._createManifestTemplateButton.Name = "_createManifestTemplateButton";
            this._createManifestTemplateButton.Text = "Create manifest template";
            this._openManualImportFolderButton.AutoSize = true;
            this._openManualImportFolderButton.Name = "_openManualImportFolderButton";
            this._openManualImportFolderButton.Text = "Create/Open manual-import folder";
            this._runManualImportButton.AutoSize = true;
            this._runManualImportButton.Name = "_runManualImportButton";
            this._runManualImportButton.Text = "Run manual import";
            this._allowOverwriteCheckBox.AutoSize = true;
            this._allowOverwriteCheckBox.Margin = new Padding(8, 7, 3, 0);
            this._allowOverwriteCheckBox.Name = "_allowOverwriteCheckBox";
            this._allowOverwriteCheckBox.Text = "Allow overwrite existing different files (risky)";
            this._copySlotIdButton.AutoSize = true;
            this._copySlotIdButton.Name = "_copySlotIdButton";
            this._copySlotIdButton.Text = "Copy slotId";
            this._copyExpectedPathButton.AutoSize = true;
            this._copyExpectedPathButton.Name = "_copyExpectedPathButton";
            this._copyExpectedPathButton.Text = "Copy expected path";
            this._manualImportSlotsGrid.AllowUserToAddRows = false;
            this._manualImportSlotsGrid.AllowUserToDeleteRows = false;
            this._manualImportSlotsGrid.AllowUserToResizeRows = false;
            this._manualImportSlotsGrid.AutoGenerateColumns = false;
            this._manualImportSlotsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this._manualImportSlotsGrid.Columns.AddRange(new DataGridViewColumn[] {
                this._slotIdColumn,
                this._slotKindColumn,
                this._slotProviderColumn,
                this._slotExpectedPathColumn,
                this._slotStatusColumn,
                this._slotFileExistsColumn,
                this._slotFileSizeColumn,
                this._slotHashColumn,
                this._slotSourceColumn });
            this._manualImportSlotsGrid.Dock = DockStyle.Fill;
            this._manualImportSlotsGrid.MultiSelect = false;
            this._manualImportSlotsGrid.Name = "_manualImportSlotsGrid";
            this._manualImportSlotsGrid.ReadOnly = true;
            this._manualImportSlotsGrid.RowHeadersVisible = false;
            this._manualImportSlotsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._slotIdColumn.DataPropertyName = "SlotId";
            this._slotIdColumn.HeaderText = "SlotId";
            this._slotIdColumn.Name = "_slotIdColumn";
            this._slotKindColumn.DataPropertyName = "Kind";
            this._slotKindColumn.HeaderText = "Kind";
            this._slotKindColumn.Name = "_slotKindColumn";
            this._slotProviderColumn.DataPropertyName = "ProviderKind";
            this._slotProviderColumn.HeaderText = "ProviderKind";
            this._slotProviderColumn.Name = "_slotProviderColumn";
            this._slotExpectedPathColumn.DataPropertyName = "ExpectedOutputRelativePath";
            this._slotExpectedPathColumn.HeaderText = "ExpectedOutputRelativePath";
            this._slotExpectedPathColumn.Name = "_slotExpectedPathColumn";
            this._slotExpectedPathColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this._slotStatusColumn.DataPropertyName = "Status";
            this._slotStatusColumn.HeaderText = "Status";
            this._slotStatusColumn.Name = "_slotStatusColumn";
            this._slotFileExistsColumn.DataPropertyName = "FileExists";
            this._slotFileExistsColumn.HeaderText = "FileExists";
            this._slotFileExistsColumn.Name = "_slotFileExistsColumn";
            this._slotFileSizeColumn.DataPropertyName = "FileSizeBytes";
            this._slotFileSizeColumn.HeaderText = "FileSizeBytes";
            this._slotFileSizeColumn.Name = "_slotFileSizeColumn";
            this._slotHashColumn.DataPropertyName = "ContentSha256";
            this._slotHashColumn.HeaderText = "ContentSha256";
            this._slotHashColumn.Name = "_slotHashColumn";
            this._slotSourceColumn.DataPropertyName = "RequestId";
            this._slotSourceColumn.HeaderText = "RequestId";
            this._slotSourceColumn.Name = "_slotSourceColumn";
            this._manualImportSlotDetailTextBox.Dock = DockStyle.Fill;
            this._manualImportSlotDetailTextBox.Font = new Font("Consolas", 10F);
            this._manualImportSlotDetailTextBox.Multiline = true;
            this._manualImportSlotDetailTextBox.Name = "_manualImportSlotDetailTextBox";
            this._manualImportSlotDetailTextBox.ReadOnly = true;
            this._manualImportSlotDetailTextBox.ScrollBars = ScrollBars.Both;
            this._manualImportSlotDetailTextBox.WordWrap = false;
            this._manualImportWorkspaceStatusLabel.AutoEllipsis = true;
            this._manualImportWorkspaceStatusLabel.Dock = DockStyle.Fill;
            this._manualImportWorkspaceStatusLabel.Name = "_manualImportWorkspaceStatusLabel";
            this._manualImportWorkspaceStatusLabel.Padding = new Padding(6, 6, 6, 0);
            this._manualImportWorkspaceStatusLabel.Text = "No manual import workspace loaded.";
            this._manualImportReportStatusLabel.AutoEllipsis = true;
            this._manualImportReportStatusLabel.Dock = DockStyle.Fill;
            this._manualImportReportStatusLabel.Name = "_manualImportReportStatusLabel";
            this._manualImportReportStatusLabel.Padding = new Padding(6, 6, 6, 0);
            this._manualImportReportStatusLabel.Text = "No manual import report yet.";
            //
            // _statusLabel
            //
            this._statusLabel.AutoEllipsis = true;
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Padding = new Padding(8, 7, 8, 0);
            this._statusLabel.Text = "Not loaded.";
            //
            // UnityArchiveReviewPageControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "UnityArchiveReviewPageControl";
            this.Size = new Size(1240, 800);
            this._rootLayout.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._toolbarPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._splitContainer).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._leftLayout.ResumeLayout(false);
            this._leftLayout.PerformLayout();
            this._currentReviewMarkdownTab.ResumeLayout(false);
            this._currentReviewMarkdownTab.PerformLayout();
            this._comparisonMarkdownTab.ResumeLayout(false);
            this._comparisonMarkdownTab.PerformLayout();
            this._currentReviewJsonTab.ResumeLayout(false);
            this._currentReviewJsonTab.PerformLayout();
            this._comparisonJsonTab.ResumeLayout(false);
            this._comparisonJsonTab.PerformLayout();
            this._historyIndexJsonTab.ResumeLayout(false);
            this._historyIndexJsonTab.PerformLayout();
            this._selectedSnapshotLayout.ResumeLayout(false);
            this._selectedSnapshotJsonTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._manualImportSlotsGrid).EndInit();
            this._manualImportToolbarPanel.ResumeLayout(false);
            this._manualImportToolbarPanel.PerformLayout();
            this._manualImportWorkspaceLayout.ResumeLayout(false);
            this._manualImportWorkspaceLayout.PerformLayout();
            this._manualImportWorkspaceTab.ResumeLayout(false);
            this._manualImportMarkdownTab.ResumeLayout(false);
            this._manualImportMarkdownTab.PerformLayout();
            this._manualImportJsonTab.ResumeLayout(false);
            this._manualImportJsonTab.PerformLayout();
            this._reportTabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
