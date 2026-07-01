#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignEditFieldSummaryControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _fieldsListView;
        private ColumnHeader _groupColumn;
        private ColumnHeader _fieldColumn;
        private ColumnHeader _domainColumn;
        private ColumnHeader _allowedColumn;
        private ColumnHeader _candidateColumn;

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
            this._fieldsListView = new ListView();
            this._groupColumn = new ColumnHeader();
            this._fieldColumn = new ColumnHeader();
            this._domainColumn = new ColumnHeader();
            this._allowedColumn = new ColumnHeader();
            this._candidateColumn = new ColumnHeader();
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
            this._summaryLabel.Text = "Editable fields";
            //
            // _fieldsListView
            //
            this._fieldsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._groupColumn,
                this._fieldColumn,
                this._domainColumn,
                this._allowedColumn,
                this._candidateColumn
            });
            this._fieldsListView.Dock = DockStyle.Fill;
            this._fieldsListView.FullRowSelect = true;
            this._fieldsListView.GridLines = true;
            this._fieldsListView.Location = new Point(0, 34);
            this._fieldsListView.Name = "_fieldsListView";
            this._fieldsListView.Size = new Size(900, 266);
            this._fieldsListView.TabIndex = 1;
            this._fieldsListView.UseCompatibleStateImageBehavior = false;
            this._fieldsListView.View = View.Details;
            //
            // columns
            //
            this._groupColumn.Text = "group";
            this._groupColumn.Width = 230;
            this._fieldColumn.Text = "field";
            this._fieldColumn.Width = 320;
            this._domainColumn.Text = "domain";
            this._domainColumn.Width = 170;
            this._allowedColumn.Text = "allowed";
            this._allowedColumn.Width = 190;
            this._candidateColumn.Text = "candidates";
            this._candidateColumn.Width = 90;
            //
            // CampaignEditFieldSummaryControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._fieldsListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignEditFieldSummaryControl";
            this.Size = new Size(900, 300);
            this.ResumeLayout(false);
        }
    }
}
