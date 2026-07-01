#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignEditValidationControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _diagnosticsListView;
        private ColumnHeader _statusColumn;
        private ColumnHeader _candidateColumn;
        private ColumnHeader _rowColumn;
        private ColumnHeader _fieldColumn;
        private ColumnHeader _diagnosticsColumn;

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
            this._statusColumn = new ColumnHeader();
            this._candidateColumn = new ColumnHeader();
            this._rowColumn = new ColumnHeader();
            this._fieldColumn = new ColumnHeader();
            this._diagnosticsColumn = new ColumnHeader();
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
            this._summaryLabel.Text = "Validation";
            //
            // _diagnosticsListView
            //
            this._diagnosticsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._statusColumn,
                this._candidateColumn,
                this._rowColumn,
                this._fieldColumn,
                this._diagnosticsColumn
            });
            this._diagnosticsListView.Dock = DockStyle.Fill;
            this._diagnosticsListView.FullRowSelect = true;
            this._diagnosticsListView.GridLines = true;
            this._diagnosticsListView.Location = new Point(0, 34);
            this._diagnosticsListView.Name = "_diagnosticsListView";
            this._diagnosticsListView.Size = new Size(900, 266);
            this._diagnosticsListView.TabIndex = 1;
            this._diagnosticsListView.UseCompatibleStateImageBehavior = false;
            this._diagnosticsListView.View = View.Details;
            //
            // columns
            //
            this._statusColumn.Text = "status";
            this._statusColumn.Width = 120;
            this._candidateColumn.Text = "candidate";
            this._candidateColumn.Width = 300;
            this._rowColumn.Text = "row";
            this._rowColumn.Width = 280;
            this._fieldColumn.Text = "field";
            this._fieldColumn.Width = 300;
            this._diagnosticsColumn.Text = "diagnostics";
            this._diagnosticsColumn.Width = 90;
            //
            // CampaignEditValidationControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._diagnosticsListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignEditValidationControl";
            this.Size = new Size(900, 300);
            this.ResumeLayout(false);
        }
    }
}
