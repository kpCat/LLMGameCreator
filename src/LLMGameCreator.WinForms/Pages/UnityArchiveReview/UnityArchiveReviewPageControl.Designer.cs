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
        private TextBox _currentReviewMarkdownTextBox;
        private TextBox _comparisonMarkdownTextBox;
        private TextBox _currentReviewJsonTextBox;
        private TextBox _comparisonJsonTextBox;
        private TextBox _historyIndexJsonTextBox;
        private Label _statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this._currentReviewMarkdownTextBox = new TextBox();
            this._comparisonMarkdownTextBox = new TextBox();
            this._currentReviewJsonTextBox = new TextBox();
            this._comparisonJsonTextBox = new TextBox();
            this._historyIndexJsonTextBox = new TextBox();
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
            this._reportTabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
