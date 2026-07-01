#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignProvenanceControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _provenanceListView;
        private ColumnHeader _categoryColumn;
        private ColumnHeader _goalColumn;
        private ColumnHeader _stateColumn;
        private ColumnHeader _acceptedColumn;
        private ColumnHeader _provenanceColumn;
        private ColumnHeader _evidenceColumn;

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
            this._provenanceListView = new ListView();
            this._categoryColumn = new ColumnHeader();
            this._goalColumn = new ColumnHeader();
            this._stateColumn = new ColumnHeader();
            this._acceptedColumn = new ColumnHeader();
            this._provenanceColumn = new ColumnHeader();
            this._evidenceColumn = new ColumnHeader();
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
            this._summaryLabel.Text = "Provenance";
            //
            // _provenanceListView
            //
            this._provenanceListView.Columns.AddRange(new ColumnHeader[]
            {
                this._categoryColumn,
                this._goalColumn,
                this._stateColumn,
                this._acceptedColumn,
                this._provenanceColumn,
                this._evidenceColumn
            });
            this._provenanceListView.Dock = DockStyle.Fill;
            this._provenanceListView.FullRowSelect = true;
            this._provenanceListView.GridLines = true;
            this._provenanceListView.Location = new Point(0, 34);
            this._provenanceListView.Name = "_provenanceListView";
            this._provenanceListView.Size = new Size(900, 566);
            this._provenanceListView.TabIndex = 1;
            this._provenanceListView.UseCompatibleStateImageBehavior = false;
            this._provenanceListView.View = View.Details;
            //
            // columns
            //
            this._categoryColumn.Text = "category";
            this._categoryColumn.Width = 110;
            this._goalColumn.Text = "sourceGoal";
            this._goalColumn.Width = 110;
            this._stateColumn.Text = "reviewState";
            this._stateColumn.Width = 190;
            this._acceptedColumn.Text = "accepted";
            this._acceptedColumn.Width = 110;
            this._provenanceColumn.Text = "provenance";
            this._provenanceColumn.Width = 150;
            this._evidenceColumn.Text = "evidenceRef";
            this._evidenceColumn.Width = 300;
            //
            // CampaignProvenanceControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._provenanceListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignProvenanceControl";
            this.Size = new Size(900, 600);
            this.ResumeLayout(false);
        }
    }
}
