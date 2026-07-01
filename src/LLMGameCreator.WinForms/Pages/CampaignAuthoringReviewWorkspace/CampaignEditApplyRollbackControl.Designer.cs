#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignEditApplyRollbackControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _rowsListView;
        private ColumnHeader _familyColumn;
        private ColumnHeader _seedColumn;
        private ColumnHeader _rowColumn;
        private ColumnHeader _stateColumn;
        private ColumnHeader _rollbackColumn;
        private ColumnHeader _diffColumn;
        private ColumnHeader _hashColumn;

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
            this._rowsListView = new ListView();
            this._familyColumn = new ColumnHeader();
            this._seedColumn = new ColumnHeader();
            this._rowColumn = new ColumnHeader();
            this._stateColumn = new ColumnHeader();
            this._rollbackColumn = new ColumnHeader();
            this._diffColumn = new ColumnHeader();
            this._hashColumn = new ColumnHeader();
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
            this._summaryLabel.Text = "Apply / rollback";
            //
            // _rowsListView
            //
            this._rowsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._familyColumn,
                this._seedColumn,
                this._rowColumn,
                this._stateColumn,
                this._rollbackColumn,
                this._diffColumn,
                this._hashColumn
            });
            this._rowsListView.Dock = DockStyle.Fill;
            this._rowsListView.FullRowSelect = true;
            this._rowsListView.GridLines = true;
            this._rowsListView.Location = new Point(0, 34);
            this._rowsListView.Name = "_rowsListView";
            this._rowsListView.Size = new Size(900, 266);
            this._rowsListView.TabIndex = 1;
            this._rowsListView.UseCompatibleStateImageBehavior = false;
            this._rowsListView.View = View.Details;
            //
            // columns
            //
            this._familyColumn.Text = "family";
            this._familyColumn.Width = 170;
            this._seedColumn.Text = "seed";
            this._seedColumn.Width = 110;
            this._rowColumn.Text = "row";
            this._rowColumn.Width = 280;
            this._stateColumn.Text = "state";
            this._stateColumn.Width = 110;
            this._rollbackColumn.Text = "rollback";
            this._rollbackColumn.Width = 120;
            this._diffColumn.Text = "diffs";
            this._diffColumn.Width = 70;
            this._hashColumn.Text = "afterHash";
            this._hashColumn.Width = 360;
            //
            // CampaignEditApplyRollbackControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rowsListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignEditApplyRollbackControl";
            this.Size = new Size(900, 300);
            this.ResumeLayout(false);
        }
    }
}
