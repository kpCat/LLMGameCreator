#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryPrototypeLuaTabControl
    {
        private IContainer components;
        private SplitContainer _rootSplitContainer;
        private TableLayoutPanel _leftTable;
        private FlowLayoutPanel _toolbarPanel;
        private TextBox _scriptIdTextBox;
        private Button _runButton;
        private Button _createArtifactButton;
        private Button _dryRunCreatedButton;
        private Label _statusLabel;
        private TextBox _sourceTextBox;
        private TableLayoutPanel _rightTable;
        private Label _artifactIdLabel;
        private TextBox _artifactIdTextBox;
        private TabControl _resultTabs;
        private TabPage _diagnosticsPage;
        private TabPage _declarationsPage;
        private TextBox _diagnosticsTextBox;
        private TextBox _declarationsTextBox;

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
            this._scriptIdTextBox = new TextBox();
            this._runButton = new Button();
            this._createArtifactButton = new Button();
            this._dryRunCreatedButton = new Button();
            this._statusLabel = new Label();
            this._sourceTextBox = new TextBox();
            this._rightTable = new TableLayoutPanel();
            this._artifactIdLabel = new Label();
            this._artifactIdTextBox = new TextBox();
            this._resultTabs = new TabControl();
            this._diagnosticsPage = new TabPage();
            this._declarationsPage = new TabPage();
            this._diagnosticsTextBox = new TextBox();
            this._declarationsTextBox = new TextBox();
            ((ISupportInitialize)(this._rootSplitContainer)).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._leftTable.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this._rightTable.SuspendLayout();
            this._resultTabs.SuspendLayout();
            this._diagnosticsPage.SuspendLayout();
            this._declarationsPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._leftTable);
            this._rootSplitContainer.Panel1.Padding = new Padding(12);
            this._rootSplitContainer.Panel2.Controls.Add(this._rightTable);
            this._rootSplitContainer.Panel2.Padding = new Padding(12);
            this._rootSplitContainer.Size = new Size(760, 420);
            this._rootSplitContainer.SplitterDistance = 400;
            this._rootSplitContainer.TabIndex = 0;
            // 
            // _leftTable
            // 
            this._leftTable.ColumnCount = 1;
            this._leftTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._leftTable.Controls.Add(this._toolbarPanel, 0, 0);
            this._leftTable.Controls.Add(this._sourceTextBox, 0, 1);
            this._leftTable.Controls.Add(this._statusLabel, 0, 2);
            this._leftTable.Dock = DockStyle.Fill;
            this._leftTable.Location = new Point(12, 12);
            this._leftTable.Name = "_leftTable";
            this._leftTable.RowCount = 3;
            this._leftTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            this._leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._leftTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftTable.Size = new Size(376, 396);
            this._leftTable.TabIndex = 0;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.AutoScroll = true;
            this._toolbarPanel.Controls.Add(this._scriptIdTextBox);
            this._toolbarPanel.Controls.Add(this._runButton);
            this._toolbarPanel.Controls.Add(this._createArtifactButton);
            this._toolbarPanel.Controls.Add(this._dryRunCreatedButton);
            this._toolbarPanel.Dock = DockStyle.Fill;
            this._toolbarPanel.Location = new Point(3, 3);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(370, 62);
            this._toolbarPanel.TabIndex = 0;
            this._scriptIdTextBox.Location = new Point(3, 3);
            this._scriptIdTextBox.Name = "_scriptIdTextBox";
            this._scriptIdTextBox.PlaceholderText = "script/prototype/inline";
            this._scriptIdTextBox.Size = new Size(170, 23);
            this._scriptIdTextBox.TabIndex = 0;
            this._runButton.Location = new Point(179, 3);
            this._runButton.Name = "_runButton";
            this._runButton.Size = new Size(120, 26);
            this._runButton.TabIndex = 1;
            this._runButton.Text = "Run Prototype Lua";
            this._runButton.UseVisualStyleBackColor = true;
            this._createArtifactButton.Location = new Point(3, 32);
            this._createArtifactButton.Name = "_createArtifactButton";
            this._createArtifactButton.Size = new Size(145, 26);
            this._createArtifactButton.TabIndex = 2;
            this._createArtifactButton.Text = "Create Patch Artifact";
            this._createArtifactButton.UseVisualStyleBackColor = true;
            this._dryRunCreatedButton.Location = new Point(154, 32);
            this._dryRunCreatedButton.Name = "_dryRunCreatedButton";
            this._dryRunCreatedButton.Size = new Size(145, 26);
            this._dryRunCreatedButton.TabIndex = 3;
            this._dryRunCreatedButton.Text = "Dry Run Created Patch";
            this._dryRunCreatedButton.UseVisualStyleBackColor = true;
            // 
            // _sourceTextBox
            // 
            this._sourceTextBox.AcceptsReturn = true;
            this._sourceTextBox.AcceptsTab = true;
            this._sourceTextBox.Dock = DockStyle.Fill;
            this._sourceTextBox.Font = new Font("Consolas", 9F);
            this._sourceTextBox.Multiline = true;
            this._sourceTextBox.Name = "_sourceTextBox";
            this._sourceTextBox.ScrollBars = ScrollBars.Both;
            this._sourceTextBox.TabIndex = 1;
            this._sourceTextBox.WordWrap = false;
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Location = new Point(3, 368);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(370, 28);
            this._statusLabel.TabIndex = 2;
            this._statusLabel.Text = "Not initialized";
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _rightTable
            // 
            this._rightTable.ColumnCount = 2;
            this._rightTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            this._rightTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rightTable.Controls.Add(this._artifactIdLabel, 0, 0);
            this._rightTable.Controls.Add(this._artifactIdTextBox, 1, 0);
            this._rightTable.Controls.Add(this._resultTabs, 0, 1);
            this._rightTable.Dock = DockStyle.Fill;
            this._rightTable.Location = new Point(12, 12);
            this._rightTable.Name = "_rightTable";
            this._rightTable.RowCount = 2;
            this._rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._rightTable.Size = new Size(332, 396);
            this._rightTable.TabIndex = 0;
            this._artifactIdLabel.Dock = DockStyle.Fill;
            this._artifactIdLabel.Location = new Point(3, 0);
            this._artifactIdLabel.Name = "_artifactIdLabel";
            this._artifactIdLabel.Size = new Size(64, 28);
            this._artifactIdLabel.TabIndex = 0;
            this._artifactIdLabel.Text = "Artifact";
            this._artifactIdLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._artifactIdTextBox.Dock = DockStyle.Fill;
            this._artifactIdTextBox.Location = new Point(73, 3);
            this._artifactIdTextBox.Name = "_artifactIdTextBox";
            this._artifactIdTextBox.ReadOnly = true;
            this._artifactIdTextBox.Size = new Size(256, 23);
            this._artifactIdTextBox.TabIndex = 1;
            this._rightTable.SetColumnSpan(this._resultTabs, 2);
            // 
            // _resultTabs
            // 
            this._resultTabs.Controls.Add(this._diagnosticsPage);
            this._resultTabs.Controls.Add(this._declarationsPage);
            this._resultTabs.Dock = DockStyle.Fill;
            this._resultTabs.Location = new Point(3, 31);
            this._resultTabs.Name = "_resultTabs";
            this._resultTabs.SelectedIndex = 0;
            this._resultTabs.Size = new Size(326, 362);
            this._resultTabs.TabIndex = 2;
            this._diagnosticsPage.Controls.Add(this._diagnosticsTextBox);
            this._diagnosticsPage.Location = new Point(4, 24);
            this._diagnosticsPage.Name = "_diagnosticsPage";
            this._diagnosticsPage.Padding = new Padding(3);
            this._diagnosticsPage.Size = new Size(318, 334);
            this._diagnosticsPage.TabIndex = 0;
            this._diagnosticsPage.Text = "Diagnostics";
            this._diagnosticsPage.UseVisualStyleBackColor = true;
            this._declarationsPage.Controls.Add(this._declarationsTextBox);
            this._declarationsPage.Location = new Point(4, 24);
            this._declarationsPage.Name = "_declarationsPage";
            this._declarationsPage.Padding = new Padding(3);
            this._declarationsPage.Size = new Size(318, 334);
            this._declarationsPage.TabIndex = 1;
            this._declarationsPage.Text = "Declarations / Patch";
            this._declarationsPage.UseVisualStyleBackColor = true;
            this._diagnosticsTextBox.Dock = DockStyle.Fill;
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Both;
            this._diagnosticsTextBox.WordWrap = false;
            this._declarationsTextBox.Dock = DockStyle.Fill;
            this._declarationsTextBox.Multiline = true;
            this._declarationsTextBox.ReadOnly = true;
            this._declarationsTextBox.ScrollBars = ScrollBars.Both;
            this._declarationsTextBox.WordWrap = false;
            // 
            // GeneratorLibraryPrototypeLuaTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootSplitContainer);
            this.Name = "GeneratorLibraryPrototypeLuaTabControl";
            this.Size = new Size(760, 420);
            this._rootSplitContainer.Panel1.ResumeLayout(false);
            this._rootSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._rootSplitContainer)).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._leftTable.ResumeLayout(false);
            this._leftTable.PerformLayout();
            this._toolbarPanel.ResumeLayout(false);
            this._toolbarPanel.PerformLayout();
            this._rightTable.ResumeLayout(false);
            this._rightTable.PerformLayout();
            this._resultTabs.ResumeLayout(false);
            this._diagnosticsPage.ResumeLayout(false);
            this._diagnosticsPage.PerformLayout();
            this._declarationsPage.ResumeLayout(false);
            this._declarationsPage.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

