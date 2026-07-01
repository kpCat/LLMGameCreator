#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignQualityGateControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _filesListView;
        private ColumnHeader _pathColumn;
        private ColumnHeader _lineCountColumn;
        private ColumnHeader _maxLineColumn;
        private ColumnHeader _over500Column;
        private ColumnHeader _statusColumn;

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
            this._filesListView = new ListView();
            this._pathColumn = new ColumnHeader();
            this._lineCountColumn = new ColumnHeader();
            this._maxLineColumn = new ColumnHeader();
            this._over500Column = new ColumnHeader();
            this._statusColumn = new ColumnHeader();
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
            this._summaryLabel.Text = "Quality";
            //
            // _filesListView
            //
            this._filesListView.Columns.AddRange(new ColumnHeader[]
            {
                this._pathColumn,
                this._lineCountColumn,
                this._maxLineColumn,
                this._over500Column,
                this._statusColumn
            });
            this._filesListView.Dock = DockStyle.Fill;
            this._filesListView.FullRowSelect = true;
            this._filesListView.GridLines = true;
            this._filesListView.Location = new Point(0, 34);
            this._filesListView.Name = "_filesListView";
            this._filesListView.Size = new Size(900, 566);
            this._filesListView.TabIndex = 1;
            this._filesListView.UseCompatibleStateImageBehavior = false;
            this._filesListView.View = View.Details;
            //
            // columns
            //
            this._pathColumn.Text = "path";
            this._pathColumn.Width = 500;
            this._lineCountColumn.Text = "lines";
            this._lineCountColumn.Width = 80;
            this._maxLineColumn.Text = "maxLine";
            this._maxLineColumn.Width = 90;
            this._over500Column.Text = "over500";
            this._over500Column.Width = 80;
            this._statusColumn.Text = "status";
            this._statusColumn.Width = 220;
            //
            // CampaignQualityGateControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._filesListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignQualityGateControl";
            this.Size = new Size(900, 600);
            this.ResumeLayout(false);
        }
    }
}
