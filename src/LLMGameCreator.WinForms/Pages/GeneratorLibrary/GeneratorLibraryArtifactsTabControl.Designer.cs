#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryArtifactsTabControl
    {
        private IContainer components;
        private SplitContainer _rootSplitContainer;
        private TableLayoutPanel _leftTable;
        private FlowLayoutPanel _toolbarPanel;
        private Button _refreshButton;
        private Button _createPatchButton;
        private Button _dryRunButton;
        private Button _applyButton;
        private Label _statusLabel;
        private ListView _artifactsListView;
        private ColumnHeader _kindColumn;
        private ColumnHeader _stateColumn;
        private ColumnHeader _generatedByColumn;
        private ColumnHeader _idColumn;
        private TableLayoutPanel _detailsTable;
        private Label _kindLabel;
        private Label _pathLabel;
        private Label _generatedByLabel;
        private Label _validationStateLabel;
        private Label _kindValueLabel;
        private Label _pathValueLabel;
        private Label _generatedByValueLabel;
        private Label _validationStateValueLabel;
        private TabControl _detailsTabs;
        private TabPage _jsonPage;
        private TabPage _metadataPage;
        private TabPage _validationPage;
        private TabPage _resultPage;
        private TextBox _jsonTextBox;
        private TextBox _metadataTextBox;
        private TextBox _validationTextBox;
        private TextBox _resultTextBox;

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
            this._leftTable = new TableLayoutPanel();
            this._toolbarPanel = new FlowLayoutPanel();
            this._refreshButton = new Button();
            this._createPatchButton = new Button();
            this._dryRunButton = new Button();
            this._applyButton = new Button();
            this._statusLabel = new Label();
            this._artifactsListView = new ListView();
            this._kindColumn = new ColumnHeader();
            this._stateColumn = new ColumnHeader();
            this._generatedByColumn = new ColumnHeader();
            this._idColumn = new ColumnHeader();
            this._detailsTable = new TableLayoutPanel();
            this._kindLabel = new Label();
            this._pathLabel = new Label();
            this._generatedByLabel = new Label();
            this._validationStateLabel = new Label();
            this._kindValueLabel = new Label();
            this._pathValueLabel = new Label();
            this._generatedByValueLabel = new Label();
            this._validationStateValueLabel = new Label();
            this._detailsTabs = new TabControl();
            this._jsonPage = new TabPage();
            this._metadataPage = new TabPage();
            this._validationPage = new TabPage();
            this._resultPage = new TabPage();
            this._jsonTextBox = new TextBox();
            this._metadataTextBox = new TextBox();
            this._validationTextBox = new TextBox();
            this._resultTextBox = new TextBox();
            ((ISupportInitialize)(this._rootSplitContainer)).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._leftTable.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this._detailsTable.SuspendLayout();
            this._detailsTabs.SuspendLayout();
            this._jsonPage.SuspendLayout();
            this._metadataPage.SuspendLayout();
            this._validationPage.SuspendLayout();
            this._resultPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._leftTable);
            this._rootSplitContainer.Panel1.Padding = new Padding(12);
            this._rootSplitContainer.Panel2.Controls.Add(this._detailsTable);
            this._rootSplitContainer.Panel2.Padding = new Padding(12);
            this._rootSplitContainer.Size = new Size(760, 420);
            this._rootSplitContainer.SplitterDistance = 360;
            this._rootSplitContainer.TabIndex = 0;
            // 
            // _leftTable
            // 
            this._leftTable.ColumnCount = 1;
            this._leftTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._leftTable.Controls.Add(this._toolbarPanel, 0, 0);
            this._leftTable.Controls.Add(this._artifactsListView, 0, 1);
            this._leftTable.Controls.Add(this._statusLabel, 0, 2);
            this._leftTable.Dock = DockStyle.Fill;
            this._leftTable.Location = new Point(12, 12);
            this._leftTable.Name = "_leftTable";
            this._leftTable.RowCount = 3;
            this._leftTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            this._leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._leftTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftTable.Size = new Size(336, 396);
            this._leftTable.TabIndex = 0;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.AutoScroll = true;
            this._toolbarPanel.Controls.Add(this._refreshButton);
            this._toolbarPanel.Controls.Add(this._createPatchButton);
            this._toolbarPanel.Controls.Add(this._dryRunButton);
            this._toolbarPanel.Controls.Add(this._applyButton);
            this._toolbarPanel.Dock = DockStyle.Fill;
            this._toolbarPanel.Location = new Point(3, 3);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(330, 62);
            this._toolbarPanel.TabIndex = 0;
            this._refreshButton.Location = new Point(3, 3);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new Size(115, 26);
            this._refreshButton.TabIndex = 0;
            this._refreshButton.Text = "Refresh Artifacts";
            this._refreshButton.UseVisualStyleBackColor = true;
            this._createPatchButton.Location = new Point(124, 3);
            this._createPatchButton.Name = "_createPatchButton";
            this._createPatchButton.Size = new Size(155, 26);
            this._createPatchButton.TabIndex = 1;
            this._createPatchButton.Text = "Create Patch From Preview";
            this._createPatchButton.UseVisualStyleBackColor = true;
            this._dryRunButton.Location = new Point(3, 35);
            this._dryRunButton.Name = "_dryRunButton";
            this._dryRunButton.Size = new Size(105, 26);
            this._dryRunButton.TabIndex = 2;
            this._dryRunButton.Text = "Dry Run Patch";
            this._dryRunButton.UseVisualStyleBackColor = true;
            this._applyButton.Location = new Point(114, 35);
            this._applyButton.Name = "_applyButton";
            this._applyButton.Size = new Size(95, 26);
            this._applyButton.TabIndex = 3;
            this._applyButton.Text = "Apply Patch";
            this._applyButton.UseVisualStyleBackColor = true;
            // 
            // _artifactsListView
            // 
            this._artifactsListView.Columns.AddRange(new ColumnHeader[] { this._kindColumn, this._stateColumn, this._generatedByColumn, this._idColumn });
            this._artifactsListView.Dock = DockStyle.Fill;
            this._artifactsListView.FullRowSelect = true;
            this._artifactsListView.GridLines = true;
            this._artifactsListView.MultiSelect = false;
            this._artifactsListView.Name = "_artifactsListView";
            this._artifactsListView.UseCompatibleStateImageBehavior = false;
            this._artifactsListView.View = View.Details;
            this._kindColumn.Text = "Kind";
            this._kindColumn.Width = 150;
            this._stateColumn.Text = "State";
            this._stateColumn.Width = 70;
            this._generatedByColumn.Text = "Generated by";
            this._generatedByColumn.Width = 150;
            this._idColumn.Text = "Id";
            this._idColumn.Width = 260;
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Location = new Point(3, 368);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(330, 28);
            this._statusLabel.TabIndex = 2;
            this._statusLabel.Text = "Not initialized";
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _detailsTable
            // 
            this._detailsTable.ColumnCount = 2;
            this._detailsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            this._detailsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._detailsTable.Controls.Add(this._kindLabel, 0, 0);
            this._detailsTable.Controls.Add(this._kindValueLabel, 1, 0);
            this._detailsTable.Controls.Add(this._pathLabel, 0, 1);
            this._detailsTable.Controls.Add(this._pathValueLabel, 1, 1);
            this._detailsTable.Controls.Add(this._generatedByLabel, 0, 2);
            this._detailsTable.Controls.Add(this._generatedByValueLabel, 1, 2);
            this._detailsTable.Controls.Add(this._validationStateLabel, 0, 3);
            this._detailsTable.Controls.Add(this._validationStateValueLabel, 1, 3);
            this._detailsTable.Controls.Add(this._detailsTabs, 0, 4);
            this._detailsTable.Dock = DockStyle.Fill;
            this._detailsTable.Location = new Point(12, 12);
            this._detailsTable.Name = "_detailsTable";
            this._detailsTable.RowCount = 5;
            this._detailsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._detailsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._detailsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._detailsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._detailsTable.Size = new Size(372, 396);
            this._detailsTable.TabIndex = 0;
            this._kindLabel.Text = "Kind";
            this._pathLabel.Text = "Path";
            this._generatedByLabel.Text = "Generated by";
            this._validationStateLabel.Text = "Validation";
            this._kindValueLabel.Text = "-";
            this._pathValueLabel.Text = "-";
            this._generatedByValueLabel.Text = "-";
            this._validationStateValueLabel.Text = "-";
            this._kindLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._pathLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._generatedByLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._validationStateLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._kindValueLabel.Dock = DockStyle.Fill;
            this._pathValueLabel.Dock = DockStyle.Fill;
            this._generatedByValueLabel.Dock = DockStyle.Fill;
            this._validationStateValueLabel.Dock = DockStyle.Fill;
            this._detailsTable.SetColumnSpan(this._detailsTabs, 2);
            // 
            // _detailsTabs
            // 
            this._detailsTabs.Controls.Add(this._jsonPage);
            this._detailsTabs.Controls.Add(this._metadataPage);
            this._detailsTabs.Controls.Add(this._validationPage);
            this._detailsTabs.Controls.Add(this._resultPage);
            this._detailsTabs.Dock = DockStyle.Fill;
            this._detailsTabs.Location = new Point(3, 99);
            this._detailsTabs.Name = "_detailsTabs";
            this._detailsTabs.SelectedIndex = 0;
            this._detailsTabs.Size = new Size(366, 294);
            this._detailsTabs.TabIndex = 8;
            // 
            // pages
            // 
            this._jsonPage.Controls.Add(this._jsonTextBox);
            this._jsonPage.Location = new Point(4, 24);
            this._jsonPage.Name = "_jsonPage";
            this._jsonPage.Padding = new Padding(3);
            this._jsonPage.Size = new Size(358, 266);
            this._jsonPage.TabIndex = 0;
            this._jsonPage.Text = "JSON";
            this._jsonPage.UseVisualStyleBackColor = true;
            this._metadataPage.Controls.Add(this._metadataTextBox);
            this._metadataPage.Location = new Point(4, 24);
            this._metadataPage.Name = "_metadataPage";
            this._metadataPage.Padding = new Padding(3);
            this._metadataPage.Size = new Size(358, 266);
            this._metadataPage.TabIndex = 1;
            this._metadataPage.Text = "Metadata";
            this._metadataPage.UseVisualStyleBackColor = true;
            this._validationPage.Controls.Add(this._validationTextBox);
            this._validationPage.Location = new Point(4, 24);
            this._validationPage.Name = "_validationPage";
            this._validationPage.Padding = new Padding(3);
            this._validationPage.Size = new Size(358, 266);
            this._validationPage.TabIndex = 2;
            this._validationPage.Text = "Validation";
            this._validationPage.UseVisualStyleBackColor = true;
            this._resultPage.Controls.Add(this._resultTextBox);
            this._resultPage.Location = new Point(4, 24);
            this._resultPage.Name = "_resultPage";
            this._resultPage.Padding = new Padding(3);
            this._resultPage.Size = new Size(358, 266);
            this._resultPage.TabIndex = 3;
            this._resultPage.Text = "Result";
            this._resultPage.UseVisualStyleBackColor = true;
            // 
            // text boxes
            // 
            this._jsonTextBox.Dock = DockStyle.Fill;
            this._jsonTextBox.Multiline = true;
            this._jsonTextBox.ReadOnly = true;
            this._jsonTextBox.ScrollBars = ScrollBars.Both;
            this._jsonTextBox.WordWrap = false;
            this._metadataTextBox.Dock = DockStyle.Fill;
            this._metadataTextBox.Multiline = true;
            this._metadataTextBox.ReadOnly = true;
            this._metadataTextBox.ScrollBars = ScrollBars.Both;
            this._metadataTextBox.WordWrap = false;
            this._validationTextBox.Dock = DockStyle.Fill;
            this._validationTextBox.Multiline = true;
            this._validationTextBox.ReadOnly = true;
            this._validationTextBox.ScrollBars = ScrollBars.Vertical;
            this._resultTextBox.Dock = DockStyle.Fill;
            this._resultTextBox.Multiline = true;
            this._resultTextBox.ReadOnly = true;
            this._resultTextBox.ScrollBars = ScrollBars.Both;
            this._resultTextBox.WordWrap = false;
            // 
            // GeneratorLibraryArtifactsTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootSplitContainer);
            this.Name = "GeneratorLibraryArtifactsTabControl";
            this.Size = new Size(760, 420);
            this._rootSplitContainer.Panel1.ResumeLayout(false);
            this._rootSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._rootSplitContainer)).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._leftTable.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._detailsTable.ResumeLayout(false);
            this._detailsTabs.ResumeLayout(false);
            this._jsonPage.ResumeLayout(false);
            this._jsonPage.PerformLayout();
            this._metadataPage.ResumeLayout(false);
            this._metadataPage.PerformLayout();
            this._validationPage.ResumeLayout(false);
            this._validationPage.PerformLayout();
            this._resultPage.ResumeLayout(false);
            this._resultPage.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

