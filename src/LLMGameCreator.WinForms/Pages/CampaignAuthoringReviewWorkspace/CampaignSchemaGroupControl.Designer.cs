#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignSchemaGroupControl
    {
        private IContainer components;
        private Label _summaryLabel;
        private SplitContainer _splitContainer;
        private ListView _groupsListView;
        private ListView _fieldsListView;
        private ColumnHeader _orderColumn;
        private ColumnHeader _groupColumn;
        private ColumnHeader _goalColumn;
        private ColumnHeader _fieldCountColumn;
        private ColumnHeader _artifactCountColumn;
        private ColumnHeader _fieldGroupColumn;
        private ColumnHeader _fieldIdColumn;
        private ColumnHeader _fieldKindColumn;
        private ColumnHeader _fieldPathColumn;

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
            this._splitContainer = new SplitContainer();
            this._groupsListView = new ListView();
            this._fieldsListView = new ListView();
            this._orderColumn = new ColumnHeader();
            this._groupColumn = new ColumnHeader();
            this._goalColumn = new ColumnHeader();
            this._fieldCountColumn = new ColumnHeader();
            this._artifactCountColumn = new ColumnHeader();
            this._fieldGroupColumn = new ColumnHeader();
            this._fieldIdColumn = new ColumnHeader();
            this._fieldKindColumn = new ColumnHeader();
            this._fieldPathColumn = new ColumnHeader();
            ((ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
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
            this._summaryLabel.Text = "Schema";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 34);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Orientation = Orientation.Horizontal;
            this._splitContainer.Panel1.Controls.Add(this._groupsListView);
            this._splitContainer.Panel2.Controls.Add(this._fieldsListView);
            this._splitContainer.Size = new Size(900, 566);
            this._splitContainer.SplitterDistance = 258;
            this._splitContainer.TabIndex = 1;
            //
            // _groupsListView
            //
            this._groupsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._orderColumn,
                this._groupColumn,
                this._goalColumn,
                this._fieldCountColumn,
                this._artifactCountColumn
            });
            this._groupsListView.Dock = DockStyle.Fill;
            this._groupsListView.FullRowSelect = true;
            this._groupsListView.GridLines = true;
            this._groupsListView.Name = "_groupsListView";
            this._groupsListView.Size = new Size(900, 258);
            this._groupsListView.TabIndex = 0;
            this._groupsListView.UseCompatibleStateImageBehavior = false;
            this._groupsListView.View = View.Details;
            //
            // _fieldsListView
            //
            this._fieldsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._fieldGroupColumn,
                this._fieldIdColumn,
                this._fieldKindColumn,
                this._fieldPathColumn
            });
            this._fieldsListView.Dock = DockStyle.Fill;
            this._fieldsListView.FullRowSelect = true;
            this._fieldsListView.GridLines = true;
            this._fieldsListView.Name = "_fieldsListView";
            this._fieldsListView.Size = new Size(900, 304);
            this._fieldsListView.TabIndex = 0;
            this._fieldsListView.UseCompatibleStateImageBehavior = false;
            this._fieldsListView.View = View.Details;
            //
            // columns
            //
            this._orderColumn.Text = "order";
            this._orderColumn.Width = 70;
            this._groupColumn.Text = "groupId";
            this._groupColumn.Width = 360;
            this._goalColumn.Text = "source";
            this._goalColumn.Width = 140;
            this._fieldCountColumn.Text = "fields";
            this._fieldCountColumn.Width = 80;
            this._artifactCountColumn.Text = "artifacts";
            this._artifactCountColumn.Width = 80;
            this._fieldGroupColumn.Text = "groupId";
            this._fieldGroupColumn.Width = 320;
            this._fieldIdColumn.Text = "fieldId";
            this._fieldIdColumn.Width = 160;
            this._fieldKindColumn.Text = "kind";
            this._fieldKindColumn.Width = 110;
            this._fieldPathColumn.Text = "sourcePath";
            this._fieldPathColumn.Width = 300;
            //
            // CampaignSchemaGroupControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._summaryLabel);
            this.Name = "CampaignSchemaGroupControl";
            this.Size = new Size(900, 600);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
