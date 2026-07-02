#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignReviewPackageControl
    {
        private IContainer components;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _hashesListView;
        private ColumnHeader _hashNameColumn;
        private ColumnHeader _hashValueColumn;
        private Panel _rightPanel;
        private ListView _targetsListView;
        private ColumnHeader _familyColumn;
        private ColumnHeader _seedColumn;
        private ColumnHeader _rowColumn;
        private ColumnHeader _targetColumn;
        private ColumnHeader _logicalPathColumn;
        private ColumnHeader _targetHashColumn;
        private Label _proofLabel;
        private TextBox _diagnosticsTextBox;

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
            this._statusLabel = new Label();
            this._splitContainer = new SplitContainer();
            this._hashesListView = new ListView();
            this._hashNameColumn = new ColumnHeader();
            this._hashValueColumn = new ColumnHeader();
            this._rightPanel = new Panel();
            this._targetsListView = new ListView();
            this._familyColumn = new ColumnHeader();
            this._seedColumn = new ColumnHeader();
            this._rowColumn = new ColumnHeader();
            this._targetColumn = new ColumnHeader();
            this._logicalPathColumn = new ColumnHeader();
            this._targetHashColumn = new ColumnHeader();
            this._proofLabel = new Label();
            this._diagnosticsTextBox = new TextBox();
            ((ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._rightPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _statusLabel
            //
            this._statusLabel.Dock = DockStyle.Top;
            this._statusLabel.Location = new Point(0, 0);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new Padding(8, 6, 8, 6);
            this._statusLabel.Size = new Size(1100, 34);
            this._statusLabel.TabIndex = 0;
            this._statusLabel.Text = "Goal 077 review package materialization";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 34);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Size = new Size(1100, 686);
            this._splitContainer.SplitterDistance = 380;
            this._splitContainer.TabIndex = 1;
            //
            // _hashesListView
            //
            this._hashesListView.Columns.AddRange(new ColumnHeader[]
            {
                this._hashNameColumn,
                this._hashValueColumn
            });
            this._hashesListView.Dock = DockStyle.Fill;
            this._hashesListView.FullRowSelect = true;
            this._hashesListView.GridLines = true;
            this._hashesListView.Name = "_hashesListView";
            this._hashesListView.TabIndex = 0;
            this._hashesListView.UseCompatibleStateImageBehavior = false;
            this._hashesListView.View = View.Details;
            //
            // _hash columns
            //
            this._hashNameColumn.Text = "hash";
            this._hashNameColumn.Width = 190;
            this._hashValueColumn.Text = "value";
            this._hashValueColumn.Width = 420;
            //
            // _rightPanel
            //
            this._rightPanel.Controls.Add(this._targetsListView);
            this._rightPanel.Controls.Add(this._diagnosticsTextBox);
            this._rightPanel.Controls.Add(this._proofLabel);
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.TabIndex = 0;
            //
            // _targetsListView
            //
            this._targetsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._familyColumn,
                this._seedColumn,
                this._rowColumn,
                this._targetColumn,
                this._logicalPathColumn,
                this._targetHashColumn
            });
            this._targetsListView.Dock = DockStyle.Fill;
            this._targetsListView.FullRowSelect = true;
            this._targetsListView.GridLines = true;
            this._targetsListView.Name = "_targetsListView";
            this._targetsListView.TabIndex = 1;
            this._targetsListView.UseCompatibleStateImageBehavior = false;
            this._targetsListView.View = View.Details;
            //
            // _proofLabel
            //
            this._proofLabel.Dock = DockStyle.Top;
            this._proofLabel.Location = new Point(0, 0);
            this._proofLabel.Name = "_proofLabel";
            this._proofLabel.Padding = new Padding(8, 6, 8, 6);
            this._proofLabel.Size = new Size(716, 34);
            this._proofLabel.TabIndex = 0;
            this._proofLabel.Text = "Review package proof";
            //
            // _diagnosticsTextBox
            //
            this._diagnosticsTextBox.Dock = DockStyle.Bottom;
            this._diagnosticsTextBox.Location = new Point(0, 486);
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.Name = "_diagnosticsTextBox";
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Vertical;
            this._diagnosticsTextBox.Size = new Size(716, 200);
            this._diagnosticsTextBox.TabIndex = 2;
            //
            // target columns
            //
            this._familyColumn.Text = "family";
            this._familyColumn.Width = 150;
            this._seedColumn.Text = "seed";
            this._seedColumn.Width = 100;
            this._rowColumn.Text = "row";
            this._rowColumn.Width = 250;
            this._targetColumn.Text = "target";
            this._targetColumn.Width = 90;
            this._logicalPathColumn.Text = "logicalPackagePath";
            this._logicalPathColumn.Width = 360;
            this._targetHashColumn.Text = "sha256";
            this._targetHashColumn.Width = 360;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._hashesListView);
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            //
            // CampaignReviewPackageControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignReviewPackageControl";
            this.Size = new Size(1100, 720);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._rightPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
