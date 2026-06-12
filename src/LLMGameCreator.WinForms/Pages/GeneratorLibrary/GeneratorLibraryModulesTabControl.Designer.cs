#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryModulesTabControl
    {
        private IContainer components;
        private SplitContainer _splitContainer;
        private ListView _modulesListView;
        private ColumnHeader _moduleIdColumn;
        private ColumnHeader _categoryColumn;
        private ColumnHeader _capabilitiesColumn;
        private ColumnHeader _batchColumn;
        private ColumnHeader _pathColumn;
        private TextBox _detailsTextBox;

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
            this._splitContainer = new SplitContainer();
            this._modulesListView = new ListView();
            this._moduleIdColumn = new ColumnHeader();
            this._categoryColumn = new ColumnHeader();
            this._capabilitiesColumn = new ColumnHeader();
            this._batchColumn = new ColumnHeader();
            this._pathColumn = new ColumnHeader();
            this._detailsTextBox = new TextBox();
            ((ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._modulesListView);
            this._splitContainer.Panel1.Padding = new Padding(12);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._detailsTextBox);
            this._splitContainer.Panel2.Padding = new Padding(12);
            this._splitContainer.Size = new Size(760, 420);
            this._splitContainer.SplitterDistance = 500;
            this._splitContainer.TabIndex = 0;
            // 
            // _modulesListView
            // 
            this._modulesListView.Columns.AddRange(new ColumnHeader[] { this._moduleIdColumn, this._categoryColumn, this._capabilitiesColumn, this._batchColumn, this._pathColumn });
            this._modulesListView.Dock = DockStyle.Fill;
            this._modulesListView.FullRowSelect = true;
            this._modulesListView.GridLines = true;
            this._modulesListView.Location = new Point(12, 12);
            this._modulesListView.MultiSelect = false;
            this._modulesListView.Name = "_modulesListView";
            this._modulesListView.Size = new Size(476, 396);
            this._modulesListView.TabIndex = 0;
            this._modulesListView.UseCompatibleStateImageBehavior = false;
            this._modulesListView.View = View.Details;
            // 
            // columns
            // 
            this._moduleIdColumn.Text = "Module Id";
            this._moduleIdColumn.Width = 190;
            this._categoryColumn.Text = "Category";
            this._categoryColumn.Width = 80;
            this._capabilitiesColumn.Text = "Caps";
            this._capabilitiesColumn.Width = 50;
            this._batchColumn.Text = "Batch";
            this._batchColumn.Width = 60;
            this._pathColumn.Text = "Path";
            this._pathColumn.Width = 180;
            // 
            // _detailsTextBox
            // 
            this._detailsTextBox.Dock = DockStyle.Fill;
            this._detailsTextBox.Location = new Point(12, 12);
            this._detailsTextBox.Multiline = true;
            this._detailsTextBox.Name = "_detailsTextBox";
            this._detailsTextBox.ReadOnly = true;
            this._detailsTextBox.ScrollBars = ScrollBars.Vertical;
            this._detailsTextBox.Size = new Size(232, 396);
            this._detailsTextBox.TabIndex = 0;
            // 
            // GeneratorLibraryModulesTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Name = "GeneratorLibraryModulesTabControl";
            this.Size = new Size(760, 420);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
