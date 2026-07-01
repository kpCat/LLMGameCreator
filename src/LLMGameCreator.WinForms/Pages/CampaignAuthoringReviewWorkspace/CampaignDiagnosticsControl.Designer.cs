#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignDiagnosticsControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _diagnosticsListView;
        private ColumnHeader _severityColumn;
        private ColumnHeader _codeColumn;
        private ColumnHeader _targetColumn;
        private ColumnHeader _messageColumn;

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
            this._summaryLabel = new Label();
            this._diagnosticsListView = new ListView();
            this._severityColumn = new ColumnHeader();
            this._codeColumn = new ColumnHeader();
            this._targetColumn = new ColumnHeader();
            this._messageColumn = new ColumnHeader();
            this.SuspendLayout();
            //
            // _summaryLabel
            //
            this._summaryLabel.Dock = DockStyle.Top;
            this._summaryLabel.Location = new Point(0, 0);
            this._summaryLabel.Name = "_summaryLabel";
            this._summaryLabel.Padding = new Padding(8, 6, 8, 6);
            this._summaryLabel.Size = new Size(900, 34);
            this._summaryLabel.TabIndex = 0;
            this._summaryLabel.Text = "Diagnostics";
            //
            // _diagnosticsListView
            //
            this._diagnosticsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._severityColumn,
                this._codeColumn,
                this._targetColumn,
                this._messageColumn
            });
            this._diagnosticsListView.Dock = DockStyle.Fill;
            this._diagnosticsListView.FullRowSelect = true;
            this._diagnosticsListView.GridLines = true;
            this._diagnosticsListView.Location = new Point(0, 34);
            this._diagnosticsListView.Name = "_diagnosticsListView";
            this._diagnosticsListView.Size = new Size(900, 566);
            this._diagnosticsListView.TabIndex = 1;
            this._diagnosticsListView.UseCompatibleStateImageBehavior = false;
            this._diagnosticsListView.View = View.Details;
            //
            // columns
            //
            this._severityColumn.Text = "severity";
            this._severityColumn.Width = 90;
            this._codeColumn.Text = "code";
            this._codeColumn.Width = 260;
            this._targetColumn.Text = "target";
            this._targetColumn.Width = 280;
            this._messageColumn.Text = "message";
            this._messageColumn.Width = 360;
            //
            // CampaignDiagnosticsControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._diagnosticsListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignDiagnosticsControl";
            this.Size = new Size(900, 600);
            this.ResumeLayout(false);
        }
    }
}
