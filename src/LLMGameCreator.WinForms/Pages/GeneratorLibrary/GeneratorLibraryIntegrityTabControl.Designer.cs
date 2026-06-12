#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryIntegrityTabControl
    {
        private IContainer components;
        private Panel _rootPanel;
        private FlowLayoutPanel _toolbarPanel;
        private Button _validateButton;
        private Label _summaryLabel;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _issuesListView;
        private ColumnHeader _severityColumn;
        private ColumnHeader _codeColumn;
        private ColumnHeader _messageColumn;
        private ColumnHeader _targetColumn;
        private ColumnHeader _suggestedFixColumn;
        private TextBox _detailsTextBox;

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
            this._rootPanel = new Panel();
            this._splitContainer = new SplitContainer();
            this._issuesListView = new ListView();
            this._severityColumn = new ColumnHeader();
            this._codeColumn = new ColumnHeader();
            this._messageColumn = new ColumnHeader();
            this._targetColumn = new ColumnHeader();
            this._suggestedFixColumn = new ColumnHeader();
            this._detailsTextBox = new TextBox();
            this._statusLabel = new Label();
            this._summaryLabel = new Label();
            this._toolbarPanel = new FlowLayoutPanel();
            this._validateButton = new Button();
            this._rootPanel.SuspendLayout();
            ((ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootPanel
            // 
            this._rootPanel.Controls.Add(this._splitContainer);
            this._rootPanel.Controls.Add(this._statusLabel);
            this._rootPanel.Controls.Add(this._summaryLabel);
            this._rootPanel.Controls.Add(this._toolbarPanel);
            this._rootPanel.Dock = DockStyle.Fill;
            this._rootPanel.Location = new Point(0, 0);
            this._rootPanel.Name = "_rootPanel";
            this._rootPanel.Padding = new Padding(12);
            this._rootPanel.Size = new Size(760, 420);
            this._rootPanel.TabIndex = 0;
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(12, 94);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._issuesListView);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._detailsTextBox);
            this._splitContainer.Size = new Size(736, 314);
            this._splitContainer.SplitterDistance = 500;
            this._splitContainer.TabIndex = 3;
            // 
            // _issuesListView
            // 
            this._issuesListView.Columns.AddRange(new ColumnHeader[] { this._severityColumn, this._codeColumn, this._messageColumn, this._targetColumn, this._suggestedFixColumn });
            this._issuesListView.Dock = DockStyle.Fill;
            this._issuesListView.FullRowSelect = true;
            this._issuesListView.GridLines = true;
            this._issuesListView.Location = new Point(0, 0);
            this._issuesListView.MultiSelect = false;
            this._issuesListView.Name = "_issuesListView";
            this._issuesListView.Size = new Size(500, 314);
            this._issuesListView.TabIndex = 0;
            this._issuesListView.UseCompatibleStateImageBehavior = false;
            this._issuesListView.View = View.Details;
            // 
            // columns
            // 
            this._severityColumn.Text = "Severity";
            this._severityColumn.Width = 80;
            this._codeColumn.Text = "Code";
            this._codeColumn.Width = 160;
            this._messageColumn.Text = "Message";
            this._messageColumn.Width = 260;
            this._targetColumn.Text = "Target";
            this._targetColumn.Width = 220;
            this._suggestedFixColumn.Text = "SuggestedFix";
            this._suggestedFixColumn.Width = 260;
            // 
            // _detailsTextBox
            // 
            this._detailsTextBox.Dock = DockStyle.Fill;
            this._detailsTextBox.Location = new Point(0, 0);
            this._detailsTextBox.Multiline = true;
            this._detailsTextBox.Name = "_detailsTextBox";
            this._detailsTextBox.ReadOnly = true;
            this._detailsTextBox.ScrollBars = ScrollBars.Vertical;
            this._detailsTextBox.Size = new Size(232, 314);
            this._detailsTextBox.TabIndex = 0;
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = DockStyle.Top;
            this._statusLabel.Location = new Point(12, 70);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(736, 24);
            this._statusLabel.TabIndex = 2;
            this._statusLabel.Text = "Not validated";
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _summaryLabel
            // 
            this._summaryLabel.Dock = DockStyle.Top;
            this._summaryLabel.Location = new Point(12, 46);
            this._summaryLabel.Name = "_summaryLabel";
            this._summaryLabel.Size = new Size(736, 24);
            this._summaryLabel.TabIndex = 1;
            this._summaryLabel.Text = "No integrity report.";
            this._summaryLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.Controls.Add(this._validateButton);
            this._toolbarPanel.Dock = DockStyle.Top;
            this._toolbarPanel.Location = new Point(12, 12);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(736, 34);
            this._toolbarPanel.TabIndex = 0;
            // 
            // _validateButton
            // 
            this._validateButton.Location = new Point(3, 3);
            this._validateButton.Name = "_validateButton";
            this._validateButton.Size = new Size(190, 28);
            this._validateButton.TabIndex = 0;
            this._validateButton.Text = "Validate generator-library";
            this._validateButton.UseVisualStyleBackColor = true;
            // 
            // GeneratorLibraryIntegrityTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootPanel);
            this.Name = "GeneratorLibraryIntegrityTabControl";
            this.Size = new Size(760, 420);
            this._rootPanel.ResumeLayout(false);
            this._rootPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
