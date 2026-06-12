#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryCapabilitiesTabControl
    {
        private IContainer components;
        private SplitContainer _splitContainer;
        private ListView _capabilitiesListView;
        private ColumnHeader _capabilityIdColumn;
        private ColumnHeader _categoryColumn;
        private ColumnHeader _manifestColumn;
        private ColumnHeader _runtimeColumn;
        private ColumnHeader _turnColumn;
        private ColumnHeader _combatColumn;
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
            this._capabilitiesListView = new ListView();
            this._capabilityIdColumn = new ColumnHeader();
            this._categoryColumn = new ColumnHeader();
            this._manifestColumn = new ColumnHeader();
            this._runtimeColumn = new ColumnHeader();
            this._turnColumn = new ColumnHeader();
            this._combatColumn = new ColumnHeader();
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
            this._splitContainer.Panel1.Controls.Add(this._capabilitiesListView);
            this._splitContainer.Panel1.Padding = new Padding(12);
            this._splitContainer.Panel2.Controls.Add(this._detailsTextBox);
            this._splitContainer.Panel2.Padding = new Padding(12);
            this._splitContainer.Size = new Size(760, 420);
            this._splitContainer.SplitterDistance = 520;
            this._splitContainer.TabIndex = 0;
            // 
            // _capabilitiesListView
            // 
            this._capabilitiesListView.Columns.AddRange(new ColumnHeader[] { this._capabilityIdColumn, this._categoryColumn, this._manifestColumn, this._runtimeColumn, this._turnColumn, this._combatColumn });
            this._capabilitiesListView.Dock = DockStyle.Fill;
            this._capabilitiesListView.FullRowSelect = true;
            this._capabilitiesListView.GridLines = true;
            this._capabilitiesListView.Location = new Point(12, 12);
            this._capabilitiesListView.MultiSelect = false;
            this._capabilitiesListView.Name = "_capabilitiesListView";
            this._capabilitiesListView.Size = new Size(496, 396);
            this._capabilitiesListView.TabIndex = 0;
            this._capabilitiesListView.UseCompatibleStateImageBehavior = false;
            this._capabilitiesListView.View = View.Details;
            // 
            // columns
            // 
            this._capabilityIdColumn.Text = "Capability Id";
            this._capabilityIdColumn.Width = 210;
            this._categoryColumn.Text = "Category";
            this._categoryColumn.Width = 80;
            this._manifestColumn.Text = "Source manifest";
            this._manifestColumn.Width = 150;
            this._runtimeColumn.Text = "Runtime";
            this._runtimeColumn.Width = 120;
            this._turnColumn.Text = "Turn";
            this._turnColumn.Width = 120;
            this._combatColumn.Text = "Combat";
            this._combatColumn.Width = 120;
            // 
            // _detailsTextBox
            // 
            this._detailsTextBox.Dock = DockStyle.Fill;
            this._detailsTextBox.Location = new Point(12, 12);
            this._detailsTextBox.Multiline = true;
            this._detailsTextBox.Name = "_detailsTextBox";
            this._detailsTextBox.ReadOnly = true;
            this._detailsTextBox.ScrollBars = ScrollBars.Vertical;
            this._detailsTextBox.Size = new Size(212, 396);
            this._detailsTextBox.TabIndex = 0;
            // 
            // GeneratorLibraryCapabilitiesTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Name = "GeneratorLibraryCapabilitiesTabControl";
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
