#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignActionPlanControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private ListView _itemsListView;
        private ColumnHeader _orderColumn;
        private ColumnHeader _actionColumn;
        private ColumnHeader _categoryColumn;
        private ColumnHeader _groupColumn;
        private ColumnHeader _policyColumn;

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
            this._itemsListView = new ListView();
            this._orderColumn = new ColumnHeader();
            this._actionColumn = new ColumnHeader();
            this._categoryColumn = new ColumnHeader();
            this._groupColumn = new ColumnHeader();
            this._policyColumn = new ColumnHeader();
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
            this._summaryLabel.Text = "Action plan";
            //
            // _itemsListView
            //
            this._itemsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._orderColumn,
                this._actionColumn,
                this._categoryColumn,
                this._groupColumn,
                this._policyColumn
            });
            this._itemsListView.Dock = DockStyle.Fill;
            this._itemsListView.FullRowSelect = true;
            this._itemsListView.GridLines = true;
            this._itemsListView.Location = new Point(0, 34);
            this._itemsListView.Name = "_itemsListView";
            this._itemsListView.Size = new Size(900, 566);
            this._itemsListView.TabIndex = 1;
            this._itemsListView.UseCompatibleStateImageBehavior = false;
            this._itemsListView.View = View.Details;
            //
            // columns
            //
            this._orderColumn.Text = "order";
            this._orderColumn.Width = 70;
            this._actionColumn.Text = "actionId";
            this._actionColumn.Width = 300;
            this._categoryColumn.Text = "category";
            this._categoryColumn.Width = 110;
            this._groupColumn.Text = "schemaGroupId";
            this._groupColumn.Width = 260;
            this._policyColumn.Text = "reviewPolicy";
            this._policyColumn.Width = 260;
            //
            // CampaignActionPlanControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._itemsListView);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignActionPlanControl";
            this.Size = new Size(900, 600);
            this.ResumeLayout(false);
        }
    }
}
