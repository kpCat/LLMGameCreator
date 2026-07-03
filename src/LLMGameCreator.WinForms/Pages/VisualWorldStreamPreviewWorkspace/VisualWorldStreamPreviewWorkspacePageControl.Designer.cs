#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class VisualWorldStreamPreviewWorkspacePageControl
    {
        private IContainer components;
        private TableLayoutPanel _rootLayout;
        private FlowLayoutPanel _toolbarPanel;
        private Button _refreshButton;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private TableLayoutPanel _leftLayout;
        private Label _groupsLabel;
        private ListBox _groupsListBox;
        private Label _entriesLabel;
        private ListView _entriesListView;
        private ColumnHeader _entryIdColumn;
        private ColumnHeader _entryKindColumn;
        private ColumnHeader _entryStatusColumn;
        private ColumnHeader _entryPathColumn;
        private Label _proofsLabel;
        private ListView _proofsListView;
        private ColumnHeader _proofIdColumn;
        private ColumnHeader _proofStatusColumn;
        private ColumnHeader _proofPathColumn;
        private ColumnHeader _proofSummaryColumn;
        private TabControl _detailTabs;
        private TabPage _detailsTabPage;
        private TabPage _svgPreviewTabPage;
        private TabPage _diagnosticsTabPage;
        private TextBox _detailsTextBox;
        private TextBox _svgPreviewTextBox;
        private TextBox _diagnosticsTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._rootLayout = new TableLayoutPanel();
            this._toolbarPanel = new FlowLayoutPanel();
            this._refreshButton = new Button();
            this._statusLabel = new Label();
            this._splitContainer = new SplitContainer();
            this._leftLayout = new TableLayoutPanel();
            this._groupsLabel = new Label();
            this._groupsListBox = new ListBox();
            this._entriesLabel = new Label();
            this._entriesListView = new ListView();
            this._entryIdColumn = new ColumnHeader();
            this._entryKindColumn = new ColumnHeader();
            this._entryStatusColumn = new ColumnHeader();
            this._entryPathColumn = new ColumnHeader();
            this._proofsLabel = new Label();
            this._proofsListView = new ListView();
            this._proofIdColumn = new ColumnHeader();
            this._proofStatusColumn = new ColumnHeader();
            this._proofPathColumn = new ColumnHeader();
            this._proofSummaryColumn = new ColumnHeader();
            this._detailTabs = new TabControl();
            this._detailsTabPage = new TabPage();
            this._svgPreviewTabPage = new TabPage();
            this._diagnosticsTabPage = new TabPage();
            this._detailsTextBox = new TextBox();
            this._svgPreviewTextBox = new TextBox();
            this._diagnosticsTextBox = new TextBox();
            this._rootLayout.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            ((ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._leftLayout.SuspendLayout();
            this._detailTabs.SuspendLayout();
            this._detailsTabPage.SuspendLayout();
            this._svgPreviewTabPage.SuspendLayout();
            this._diagnosticsTabPage.SuspendLayout();
            this.SuspendLayout();
            //
            // _rootLayout
            //
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._toolbarPanel, 0, 0);
            this._rootLayout.Controls.Add(this._statusLabel, 0, 1);
            this._rootLayout.Controls.Add(this._splitContainer, 0, 2);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.RowCount = 3;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            //
            // _toolbarPanel
            //
            this._toolbarPanel.Controls.Add(this._refreshButton);
            this._toolbarPanel.Dock = DockStyle.Fill;
            this._toolbarPanel.Padding = new Padding(8, 7, 8, 4);
            this._toolbarPanel.Name = "_toolbarPanel";
            //
            // _refreshButton
            //
            this._refreshButton.AutoSize = true;
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Text = "Refresh";
            this._refreshButton.UseVisualStyleBackColor = true;
            //
            // _statusLabel
            //
            this._statusLabel.AutoEllipsis = true;
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new Padding(8, 7, 8, 0);
            this._statusLabel.Text = "Visual world stream preview workspace not loaded.";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(3, 79);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Size = new Size(1214, 718);
            this._splitContainer.SplitterDistance = 520;
            this._splitContainer.TabIndex = 2;
            //
            // _leftLayout
            //
            this._leftLayout.ColumnCount = 1;
            this._leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._leftLayout.Controls.Add(this._groupsLabel, 0, 0);
            this._leftLayout.Controls.Add(this._groupsListBox, 0, 1);
            this._leftLayout.Controls.Add(this._entriesLabel, 0, 2);
            this._leftLayout.Controls.Add(this._entriesListView, 0, 3);
            this._leftLayout.Controls.Add(this._proofsLabel, 0, 4);
            this._leftLayout.Controls.Add(this._proofsListView, 0, 5);
            this._leftLayout.Dock = DockStyle.Fill;
            this._leftLayout.RowCount = 6;
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 43F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            //
            // labels
            //
            this._groupsLabel.AutoSize = true;
            this._groupsLabel.Dock = DockStyle.Fill;
            this._groupsLabel.Padding = new Padding(6, 7, 6, 0);
            this._groupsLabel.Text = "Artifact groups";
            this._entriesLabel.AutoSize = true;
            this._entriesLabel.Dock = DockStyle.Fill;
            this._entriesLabel.Padding = new Padding(6, 7, 6, 0);
            this._entriesLabel.Text = "Entries";
            this._proofsLabel.AutoSize = true;
            this._proofsLabel.Dock = DockStyle.Fill;
            this._proofsLabel.Padding = new Padding(6, 7, 6, 0);
            this._proofsLabel.Text = "Proof status";
            //
            // _groupsListBox
            //
            this._groupsListBox.Dock = DockStyle.Fill;
            this._groupsListBox.IntegralHeight = false;
            this._groupsListBox.Name = "_groupsListBox";
            //
            // _entriesListView
            //
            this._entriesListView.Columns.AddRange(new ColumnHeader[]
            {
                this._entryIdColumn,
                this._entryKindColumn,
                this._entryStatusColumn,
                this._entryPathColumn
            });
            this._entriesListView.Dock = DockStyle.Fill;
            this._entriesListView.FullRowSelect = true;
            this._entriesListView.GridLines = true;
            this._entriesListView.HideSelection = false;
            this._entriesListView.MultiSelect = false;
            this._entriesListView.Name = "_entriesListView";
            this._entriesListView.UseCompatibleStateImageBehavior = false;
            this._entriesListView.View = View.Details;
            this._entryIdColumn.Text = "id";
            this._entryIdColumn.Width = 210;
            this._entryKindColumn.Text = "kind";
            this._entryKindColumn.Width = 180;
            this._entryStatusColumn.Text = "status";
            this._entryStatusColumn.Width = 80;
            this._entryPathColumn.Text = "path";
            this._entryPathColumn.Width = 360;
            //
            // _proofsListView
            //
            this._proofsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._proofIdColumn,
                this._proofStatusColumn,
                this._proofPathColumn,
                this._proofSummaryColumn
            });
            this._proofsListView.Dock = DockStyle.Fill;
            this._proofsListView.FullRowSelect = true;
            this._proofsListView.GridLines = true;
            this._proofsListView.Name = "_proofsListView";
            this._proofsListView.UseCompatibleStateImageBehavior = false;
            this._proofsListView.View = View.Details;
            this._proofIdColumn.Text = "proof";
            this._proofIdColumn.Width = 170;
            this._proofStatusColumn.Text = "status";
            this._proofStatusColumn.Width = 70;
            this._proofPathColumn.Text = "path";
            this._proofPathColumn.Width = 230;
            this._proofSummaryColumn.Text = "summary";
            this._proofSummaryColumn.Width = 260;
            //
            // _detailTabs
            //
            this._detailTabs.Controls.Add(this._detailsTabPage);
            this._detailTabs.Controls.Add(this._svgPreviewTabPage);
            this._detailTabs.Controls.Add(this._diagnosticsTabPage);
            this._detailTabs.Dock = DockStyle.Fill;
            this._detailTabs.Name = "_detailTabs";
            this._detailsTabPage.Controls.Add(this._detailsTextBox);
            this._detailsTabPage.Text = "Details";
            this._svgPreviewTabPage.Controls.Add(this._svgPreviewTextBox);
            this._svgPreviewTabPage.Text = "SVG Text";
            this._diagnosticsTabPage.Controls.Add(this._diagnosticsTextBox);
            this._diagnosticsTabPage.Text = "Diagnostics";
            //
            // text boxes
            //
            this._detailsTextBox.Dock = DockStyle.Fill;
            this._detailsTextBox.Font = new Font("Consolas", 10F);
            this._detailsTextBox.Multiline = true;
            this._detailsTextBox.Name = "_detailsTextBox";
            this._detailsTextBox.ReadOnly = true;
            this._detailsTextBox.ScrollBars = ScrollBars.Both;
            this._detailsTextBox.WordWrap = false;
            this._svgPreviewTextBox.Dock = DockStyle.Fill;
            this._svgPreviewTextBox.Font = new Font("Consolas", 10F);
            this._svgPreviewTextBox.Multiline = true;
            this._svgPreviewTextBox.Name = "_svgPreviewTextBox";
            this._svgPreviewTextBox.ReadOnly = true;
            this._svgPreviewTextBox.ScrollBars = ScrollBars.Both;
            this._svgPreviewTextBox.WordWrap = false;
            this._diagnosticsTextBox.Dock = DockStyle.Fill;
            this._diagnosticsTextBox.Font = new Font("Consolas", 10F);
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.Name = "_diagnosticsTextBox";
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Both;
            this._diagnosticsTextBox.WordWrap = false;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._leftLayout);
            this._splitContainer.Panel2.Controls.Add(this._detailTabs);
            //
            // VisualWorldStreamPreviewWorkspacePageControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "VisualWorldStreamPreviewWorkspacePageControl";
            this.Size = new Size(1220, 800);
            this._rootLayout.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._toolbarPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._leftLayout.ResumeLayout(false);
            this._leftLayout.PerformLayout();
            this._detailsTabPage.ResumeLayout(false);
            this._detailsTabPage.PerformLayout();
            this._svgPreviewTabPage.ResumeLayout(false);
            this._svgPreviewTabPage.PerformLayout();
            this._diagnosticsTabPage.ResumeLayout(false);
            this._diagnosticsTabPage.PerformLayout();
            this._detailTabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
