#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryIssuesTabControl
    {
        private IContainer components;
        private ListView _issuesListView;
        private ColumnHeader _scopeColumn;
        private ColumnHeader _importIdColumn;
        private ColumnHeader _severityColumn;
        private ColumnHeader _codeColumn;
        private ColumnHeader _messageColumn;
        private ColumnHeader _targetColumn;

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
            this._issuesListView = new ListView();
            this._scopeColumn = new ColumnHeader();
            this._importIdColumn = new ColumnHeader();
            this._severityColumn = new ColumnHeader();
            this._codeColumn = new ColumnHeader();
            this._messageColumn = new ColumnHeader();
            this._targetColumn = new ColumnHeader();
            this.SuspendLayout();
            // 
            // _issuesListView
            // 
            this._issuesListView.Columns.AddRange(new ColumnHeader[] { this._scopeColumn, this._importIdColumn, this._severityColumn, this._codeColumn, this._messageColumn, this._targetColumn });
            this._issuesListView.Dock = DockStyle.Fill;
            this._issuesListView.FullRowSelect = true;
            this._issuesListView.GridLines = true;
            this._issuesListView.Location = new Point(12, 12);
            this._issuesListView.Name = "_issuesListView";
            this._issuesListView.Size = new Size(736, 396);
            this._issuesListView.TabIndex = 0;
            this._issuesListView.UseCompatibleStateImageBehavior = false;
            this._issuesListView.View = View.Details;
            // 
            // columns
            // 
            this._scopeColumn.Text = "Scope";
            this._scopeColumn.Width = 80;
            this._importIdColumn.Text = "Import";
            this._importIdColumn.Width = 160;
            this._severityColumn.Text = "Severity";
            this._severityColumn.Width = 80;
            this._codeColumn.Text = "Code";
            this._codeColumn.Width = 180;
            this._messageColumn.Text = "Message";
            this._messageColumn.Width = 320;
            this._targetColumn.Text = "Target";
            this._targetColumn.Width = 240;
            // 
            // GeneratorLibraryIssuesTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._issuesListView);
            this.Name = "GeneratorLibraryIssuesTabControl";
            this.Padding = new Padding(12);
            this.Size = new Size(760, 420);
            this.ResumeLayout(false);
        }
    }
}
