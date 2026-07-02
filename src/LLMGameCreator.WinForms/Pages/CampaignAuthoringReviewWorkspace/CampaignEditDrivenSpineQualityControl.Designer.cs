#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignEditDrivenSpineQualityControl
    {
        private IContainer components;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _chainListView;
        private ColumnHeader _goalColumn;
        private ColumnHeader _statusColumn;
        private ColumnHeader _acceptedColumn;
        private ColumnHeader _hashColumn;
        private Panel _rightPanel;
        private ListView _debtListView;
        private ColumnHeader _severityColumn;
        private ColumnHeader _findingColumn;
        private ColumnHeader _areaColumn;
        private ColumnHeader _evidenceColumn;
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
            this._chainListView = new ListView();
            this._goalColumn = new ColumnHeader();
            this._statusColumn = new ColumnHeader();
            this._acceptedColumn = new ColumnHeader();
            this._hashColumn = new ColumnHeader();
            this._rightPanel = new Panel();
            this._debtListView = new ListView();
            this._severityColumn = new ColumnHeader();
            this._findingColumn = new ColumnHeader();
            this._areaColumn = new ColumnHeader();
            this._evidenceColumn = new ColumnHeader();
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
            this._statusLabel.Text = "Goal 079 edit-driven spine quality";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 34);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Size = new Size(1100, 686);
            this._splitContainer.SplitterDistance = 470;
            this._splitContainer.TabIndex = 1;
            //
            // _chainListView
            //
            this._chainListView.Columns.AddRange(new ColumnHeader[]
            {
                this._goalColumn,
                this._statusColumn,
                this._acceptedColumn,
                this._hashColumn
            });
            this._chainListView.Dock = DockStyle.Fill;
            this._chainListView.FullRowSelect = true;
            this._chainListView.GridLines = true;
            this._chainListView.Name = "_chainListView";
            this._chainListView.TabIndex = 0;
            this._chainListView.UseCompatibleStateImageBehavior = false;
            this._chainListView.View = View.Details;
            //
            // chain columns
            //
            this._goalColumn.Text = "goal";
            this._goalColumn.Width = 80;
            this._statusColumn.Text = "status";
            this._statusColumn.Width = 90;
            this._acceptedColumn.Text = "accepted";
            this._acceptedColumn.Width = 80;
            this._hashColumn.Text = "report hash";
            this._hashColumn.Width = 420;
            //
            // _rightPanel
            //
            this._rightPanel.Controls.Add(this._debtListView);
            this._rightPanel.Controls.Add(this._diagnosticsTextBox);
            this._rightPanel.Controls.Add(this._proofLabel);
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.TabIndex = 0;
            //
            // _debtListView
            //
            this._debtListView.Columns.AddRange(new ColumnHeader[]
            {
                this._severityColumn,
                this._findingColumn,
                this._areaColumn,
                this._evidenceColumn
            });
            this._debtListView.Dock = DockStyle.Fill;
            this._debtListView.FullRowSelect = true;
            this._debtListView.GridLines = true;
            this._debtListView.Name = "_debtListView";
            this._debtListView.TabIndex = 1;
            this._debtListView.UseCompatibleStateImageBehavior = false;
            this._debtListView.View = View.Details;
            //
            // _proofLabel
            //
            this._proofLabel.Dock = DockStyle.Top;
            this._proofLabel.Location = new Point(0, 0);
            this._proofLabel.Name = "_proofLabel";
            this._proofLabel.Padding = new Padding(8, 6, 8, 6);
            this._proofLabel.Size = new Size(626, 34);
            this._proofLabel.TabIndex = 0;
            this._proofLabel.Text = "Spine proof";
            //
            // _diagnosticsTextBox
            //
            this._diagnosticsTextBox.Dock = DockStyle.Bottom;
            this._diagnosticsTextBox.Location = new Point(0, 486);
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.Name = "_diagnosticsTextBox";
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Vertical;
            this._diagnosticsTextBox.Size = new Size(626, 200);
            this._diagnosticsTextBox.TabIndex = 2;
            //
            // debt columns
            //
            this._severityColumn.Text = "sev";
            this._severityColumn.Width = 55;
            this._findingColumn.Text = "finding";
            this._findingColumn.Width = 220;
            this._areaColumn.Text = "area";
            this._areaColumn.Width = 150;
            this._evidenceColumn.Text = "evidence";
            this._evidenceColumn.Width = 520;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._chainListView);
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            //
            // CampaignEditDrivenSpineQualityControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignEditDrivenSpineQualityControl";
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
