#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryPlansTabControl
    {
        private IContainer components;
        private SplitContainer _rootSplitContainer;
        private TableLayoutPanel _inputTable;
        private Label _titleLabel;
        private Label _goalLabel;
        private Label _runtimeTargetLabel;
        private Label _turnModeLabel;
        private Label _combatModeLabel;
        private Label _briefLabel;
        private TextBox _titleTextBox;
        private TextBox _goalTextBox;
        private TextBox _runtimeTargetTextBox;
        private TextBox _turnModeTextBox;
        private TextBox _combatModeTextBox;
        private TextBox _briefTextBox;
        private FlowLayoutPanel _toolbarPanel;
        private Button _createButton;
        private Button _refreshButton;
        private Button _revalidateButton;
        private Button _createPreviewButton;
        private Button _approveButton;
        private Button _rejectButton;
        private Button _archiveButton;
        private Label _statusLabel;
        private TabControl _detailsTabs;
        private TabPage _plansPage;
        private TabPage _stepsPage;
        private TabPage _issuesPage;
        private TabPage _previewArtifactPage;
        private TabPage _rawPage;
        private ListView _plansListView;
        private ColumnHeader _planTitleColumn;
        private ColumnHeader _planStatusColumn;
        private ColumnHeader _planGoalColumn;
        private ColumnHeader _planUpdatedColumn;
        private ListView _stepsListView;
        private ColumnHeader _stepOrderColumn;
        private ColumnHeader _stepModuleColumn;
        private ColumnHeader _stepStatusColumn;
        private ColumnHeader _stepDependsOnColumn;
        private TextBox _issuesTextBox;
        private TextBox _previewArtifactTextBox;
        private TextBox _rawResponseTextBox;

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
            this._rootSplitContainer = new SplitContainer();
            this._inputTable = new TableLayoutPanel();
            this._titleLabel = new Label();
            this._goalLabel = new Label();
            this._runtimeTargetLabel = new Label();
            this._turnModeLabel = new Label();
            this._combatModeLabel = new Label();
            this._briefLabel = new Label();
            this._titleTextBox = new TextBox();
            this._goalTextBox = new TextBox();
            this._runtimeTargetTextBox = new TextBox();
            this._turnModeTextBox = new TextBox();
            this._combatModeTextBox = new TextBox();
            this._briefTextBox = new TextBox();
            this._toolbarPanel = new FlowLayoutPanel();
            this._createButton = new Button();
            this._refreshButton = new Button();
            this._revalidateButton = new Button();
            this._createPreviewButton = new Button();
            this._approveButton = new Button();
            this._rejectButton = new Button();
            this._archiveButton = new Button();
            this._statusLabel = new Label();
            this._detailsTabs = new TabControl();
            this._plansPage = new TabPage();
            this._stepsPage = new TabPage();
            this._issuesPage = new TabPage();
            this._previewArtifactPage = new TabPage();
            this._rawPage = new TabPage();
            this._plansListView = new ListView();
            this._planTitleColumn = new ColumnHeader();
            this._planStatusColumn = new ColumnHeader();
            this._planGoalColumn = new ColumnHeader();
            this._planUpdatedColumn = new ColumnHeader();
            this._stepsListView = new ListView();
            this._stepOrderColumn = new ColumnHeader();
            this._stepModuleColumn = new ColumnHeader();
            this._stepStatusColumn = new ColumnHeader();
            this._stepDependsOnColumn = new ColumnHeader();
            this._issuesTextBox = new TextBox();
            this._previewArtifactTextBox = new TextBox();
            this._rawResponseTextBox = new TextBox();
            ((ISupportInitialize)(this._rootSplitContainer)).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._inputTable.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this._detailsTabs.SuspendLayout();
            this._plansPage.SuspendLayout();
            this._stepsPage.SuspendLayout();
            this._issuesPage.SuspendLayout();
            this._previewArtifactPage.SuspendLayout();
            this._rawPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._inputTable);
            this._rootSplitContainer.Panel1.Padding = new Padding(12);
            this._rootSplitContainer.Panel2.Controls.Add(this._detailsTabs);
            this._rootSplitContainer.Panel2.Padding = new Padding(12);
            this._rootSplitContainer.Size = new Size(760, 420);
            this._rootSplitContainer.SplitterDistance = 330;
            this._rootSplitContainer.TabIndex = 0;
            // 
            // _inputTable
            // 
            this._inputTable.ColumnCount = 2;
            this._inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            this._inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._inputTable.Controls.Add(this._titleLabel, 0, 0);
            this._inputTable.Controls.Add(this._titleTextBox, 1, 0);
            this._inputTable.Controls.Add(this._goalLabel, 0, 1);
            this._inputTable.Controls.Add(this._goalTextBox, 1, 1);
            this._inputTable.Controls.Add(this._runtimeTargetLabel, 0, 2);
            this._inputTable.Controls.Add(this._runtimeTargetTextBox, 1, 2);
            this._inputTable.Controls.Add(this._turnModeLabel, 0, 3);
            this._inputTable.Controls.Add(this._turnModeTextBox, 1, 3);
            this._inputTable.Controls.Add(this._combatModeLabel, 0, 4);
            this._inputTable.Controls.Add(this._combatModeTextBox, 1, 4);
            this._inputTable.Controls.Add(this._briefLabel, 0, 5);
            this._inputTable.Controls.Add(this._briefTextBox, 0, 6);
            this._inputTable.Controls.Add(this._toolbarPanel, 0, 7);
            this._inputTable.Controls.Add(this._statusLabel, 0, 8);
            this._inputTable.Dock = DockStyle.Fill;
            this._inputTable.Location = new Point(12, 12);
            this._inputTable.Name = "_inputTable";
            this._inputTable.RowCount = 9;
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            this._inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._inputTable.Size = new Size(306, 396);
            this._inputTable.TabIndex = 0;
            // 
            // labels and inputs
            // 
            this._titleLabel.Text = "Title";
            this._titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._titleTextBox.Dock = DockStyle.Fill;
            this._goalLabel.Text = "Goal";
            this._goalLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._goalTextBox.Dock = DockStyle.Fill;
            this._runtimeTargetLabel.Text = "Runtime";
            this._runtimeTargetLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._runtimeTargetTextBox.Dock = DockStyle.Fill;
            this._turnModeLabel.Text = "Turn mode";
            this._turnModeLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._turnModeTextBox.Dock = DockStyle.Fill;
            this._combatModeLabel.Text = "Combat";
            this._combatModeLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._combatModeTextBox.Dock = DockStyle.Fill;
            this._briefLabel.Text = "Design brief";
            this._briefLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._briefTextBox.Dock = DockStyle.Fill;
            this._briefTextBox.Multiline = true;
            this._briefTextBox.ScrollBars = ScrollBars.Vertical;
            this._inputTable.SetColumnSpan(this._briefLabel, 2);
            this._inputTable.SetColumnSpan(this._briefTextBox, 2);
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.AutoScroll = true;
            this._toolbarPanel.Controls.Add(this._createButton);
            this._toolbarPanel.Controls.Add(this._refreshButton);
            this._toolbarPanel.Controls.Add(this._revalidateButton);
            this._toolbarPanel.Controls.Add(this._createPreviewButton);
            this._toolbarPanel.Controls.Add(this._approveButton);
            this._toolbarPanel.Controls.Add(this._rejectButton);
            this._toolbarPanel.Controls.Add(this._archiveButton);
            this._toolbarPanel.Dock = DockStyle.Fill;
            this._toolbarPanel.Location = new Point(3, 303);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(300, 62);
            this._toolbarPanel.TabIndex = 12;
            this._inputTable.SetColumnSpan(this._toolbarPanel, 2);
            this._createButton.Location = new Point(3, 3);
            this._createButton.Name = "_createButton";
            this._createButton.Size = new Size(130, 26);
            this._createButton.TabIndex = 0;
            this._createButton.Text = "Create Draft Plan";
            this._createButton.UseVisualStyleBackColor = true;
            this._refreshButton.Location = new Point(139, 3);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new Size(100, 26);
            this._refreshButton.TabIndex = 1;
            this._refreshButton.Text = "Refresh Plans";
            this._refreshButton.UseVisualStyleBackColor = true;
            this._revalidateButton.Location = new Point(245, 3);
            this._revalidateButton.Name = "_revalidateButton";
            this._revalidateButton.Size = new Size(120, 26);
            this._revalidateButton.TabIndex = 2;
            this._revalidateButton.Text = "Revalidate Selected";
            this._revalidateButton.UseVisualStyleBackColor = true;
            this._createPreviewButton.Location = new Point(371, 3);
            this._createPreviewButton.Name = "_createPreviewButton";
            this._createPreviewButton.Size = new Size(145, 26);
            this._createPreviewButton.TabIndex = 3;
            this._createPreviewButton.Text = "Create Preview Artifact";
            this._createPreviewButton.UseVisualStyleBackColor = true;
            this._approveButton.Location = new Point(522, 3);
            this._approveButton.Name = "_approveButton";
            this._approveButton.Size = new Size(110, 26);
            this._approveButton.TabIndex = 4;
            this._approveButton.Text = "Approve Selected";
            this._approveButton.UseVisualStyleBackColor = true;
            this._rejectButton.Location = new Point(638, 3);
            this._rejectButton.Name = "_rejectButton";
            this._rejectButton.Size = new Size(100, 26);
            this._rejectButton.TabIndex = 5;
            this._rejectButton.Text = "Reject Selected";
            this._rejectButton.UseVisualStyleBackColor = true;
            this._archiveButton.Location = new Point(744, 3);
            this._archiveButton.Name = "_archiveButton";
            this._archiveButton.Size = new Size(110, 26);
            this._archiveButton.TabIndex = 6;
            this._archiveButton.Text = "Archive Selected";
            this._archiveButton.UseVisualStyleBackColor = true;
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Location = new Point(3, 368);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(300, 28);
            this._statusLabel.TabIndex = 13;
            this._statusLabel.Text = "Not initialized";
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._inputTable.SetColumnSpan(this._statusLabel, 2);
            // 
            // _detailsTabs
            // 
            this._detailsTabs.Controls.Add(this._plansPage);
            this._detailsTabs.Controls.Add(this._stepsPage);
            this._detailsTabs.Controls.Add(this._issuesPage);
            this._detailsTabs.Controls.Add(this._previewArtifactPage);
            this._detailsTabs.Controls.Add(this._rawPage);
            this._detailsTabs.Dock = DockStyle.Fill;
            this._detailsTabs.Location = new Point(12, 12);
            this._detailsTabs.Name = "_detailsTabs";
            this._detailsTabs.SelectedIndex = 0;
            this._detailsTabs.Size = new Size(398, 396);
            this._detailsTabs.TabIndex = 0;
            // 
            // pages
            // 
            this._plansPage.Controls.Add(this._plansListView);
            this._plansPage.Location = new Point(4, 24);
            this._plansPage.Name = "_plansPage";
            this._plansPage.Padding = new Padding(3);
            this._plansPage.Size = new Size(390, 368);
            this._plansPage.TabIndex = 0;
            this._plansPage.Text = "Plans";
            this._plansPage.UseVisualStyleBackColor = true;
            this._stepsPage.Controls.Add(this._stepsListView);
            this._stepsPage.Location = new Point(4, 24);
            this._stepsPage.Name = "_stepsPage";
            this._stepsPage.Padding = new Padding(3);
            this._stepsPage.Size = new Size(390, 368);
            this._stepsPage.TabIndex = 1;
            this._stepsPage.Text = "Steps";
            this._stepsPage.UseVisualStyleBackColor = true;
            this._issuesPage.Controls.Add(this._issuesTextBox);
            this._issuesPage.Location = new Point(4, 24);
            this._issuesPage.Name = "_issuesPage";
            this._issuesPage.Padding = new Padding(3);
            this._issuesPage.Size = new Size(390, 368);
            this._issuesPage.TabIndex = 2;
            this._issuesPage.Text = "Issues";
            this._issuesPage.UseVisualStyleBackColor = true;
            this._previewArtifactPage.Controls.Add(this._previewArtifactTextBox);
            this._previewArtifactPage.Location = new Point(4, 24);
            this._previewArtifactPage.Name = "_previewArtifactPage";
            this._previewArtifactPage.Padding = new Padding(3);
            this._previewArtifactPage.Size = new Size(390, 368);
            this._previewArtifactPage.TabIndex = 3;
            this._previewArtifactPage.Text = "Preview Artifact";
            this._previewArtifactPage.UseVisualStyleBackColor = true;
            this._rawPage.Controls.Add(this._rawResponseTextBox);
            this._rawPage.Location = new Point(4, 24);
            this._rawPage.Name = "_rawPage";
            this._rawPage.Padding = new Padding(3);
            this._rawPage.Size = new Size(390, 368);
            this._rawPage.TabIndex = 4;
            this._rawPage.Text = "Raw JSON";
            this._rawPage.UseVisualStyleBackColor = true;
            // 
            // list views
            // 
            this._plansListView.Columns.AddRange(new ColumnHeader[] { this._planTitleColumn, this._planStatusColumn, this._planGoalColumn, this._planUpdatedColumn });
            this._plansListView.Dock = DockStyle.Fill;
            this._plansListView.FullRowSelect = true;
            this._plansListView.GridLines = true;
            this._plansListView.MultiSelect = false;
            this._plansListView.Name = "_plansListView";
            this._plansListView.UseCompatibleStateImageBehavior = false;
            this._plansListView.View = View.Details;
            this._planTitleColumn.Text = "Title";
            this._planTitleColumn.Width = 150;
            this._planStatusColumn.Text = "Status";
            this._planStatusColumn.Width = 70;
            this._planGoalColumn.Text = "Goal";
            this._planGoalColumn.Width = 220;
            this._planUpdatedColumn.Text = "Updated";
            this._planUpdatedColumn.Width = 120;
            this._stepsListView.Columns.AddRange(new ColumnHeader[] { this._stepOrderColumn, this._stepModuleColumn, this._stepStatusColumn, this._stepDependsOnColumn });
            this._stepsListView.Dock = DockStyle.Fill;
            this._stepsListView.FullRowSelect = true;
            this._stepsListView.GridLines = true;
            this._stepsListView.Name = "_stepsListView";
            this._stepsListView.UseCompatibleStateImageBehavior = false;
            this._stepsListView.View = View.Details;
            this._stepOrderColumn.Text = "Order";
            this._stepOrderColumn.Width = 60;
            this._stepModuleColumn.Text = "Module";
            this._stepModuleColumn.Width = 180;
            this._stepStatusColumn.Text = "Status";
            this._stepStatusColumn.Width = 80;
            this._stepDependsOnColumn.Text = "Depends on";
            this._stepDependsOnColumn.Width = 220;
            // 
            // text boxes
            // 
            this._issuesTextBox.Dock = DockStyle.Fill;
            this._issuesTextBox.Multiline = true;
            this._issuesTextBox.ReadOnly = true;
            this._issuesTextBox.ScrollBars = ScrollBars.Vertical;
            this._previewArtifactTextBox.Dock = DockStyle.Fill;
            this._previewArtifactTextBox.Multiline = true;
            this._previewArtifactTextBox.ReadOnly = true;
            this._previewArtifactTextBox.ScrollBars = ScrollBars.Both;
            this._previewArtifactTextBox.WordWrap = false;
            this._rawResponseTextBox.Dock = DockStyle.Fill;
            this._rawResponseTextBox.Multiline = true;
            this._rawResponseTextBox.ReadOnly = true;
            this._rawResponseTextBox.ScrollBars = ScrollBars.Both;
            this._rawResponseTextBox.WordWrap = false;
            // 
            // GeneratorLibraryPlansTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootSplitContainer);
            this.Name = "GeneratorLibraryPlansTabControl";
            this.Size = new Size(760, 420);
            this._rootSplitContainer.Panel1.ResumeLayout(false);
            this._rootSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._rootSplitContainer)).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._inputTable.ResumeLayout(false);
            this._inputTable.PerformLayout();
            this._toolbarPanel.ResumeLayout(false);
            this._detailsTabs.ResumeLayout(false);
            this._plansPage.ResumeLayout(false);
            this._stepsPage.ResumeLayout(false);
            this._issuesPage.ResumeLayout(false);
            this._issuesPage.PerformLayout();
            this._previewArtifactPage.ResumeLayout(false);
            this._previewArtifactPage.PerformLayout();
            this._rawPage.ResumeLayout(false);
            this._rawPage.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
