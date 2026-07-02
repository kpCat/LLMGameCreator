#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignPlayableRefreshControl
    {
        private IContainer components;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _hashesListView;
        private ColumnHeader _hashNameColumn;
        private ColumnHeader _hashValueColumn;
        private Panel _rightPanel;
        private ListView _rowsListView;
        private ColumnHeader _familyColumn;
        private ColumnHeader _seedColumn;
        private ColumnHeader _rowColumn;
        private ColumnHeader _stateColumn;
        private ColumnHeader _rollbackColumn;
        private ColumnHeader _replayColumn;
        private ColumnHeader _targetsColumn;
        private ColumnHeader _afterHashColumn;
        private Label _handoffLabel;
        private TextBox _diagnosticsTextBox;

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
            this._statusLabel = new Label();
            this._splitContainer = new SplitContainer();
            this._hashesListView = new ListView();
            this._hashNameColumn = new ColumnHeader();
            this._hashValueColumn = new ColumnHeader();
            this._rightPanel = new Panel();
            this._rowsListView = new ListView();
            this._familyColumn = new ColumnHeader();
            this._seedColumn = new ColumnHeader();
            this._rowColumn = new ColumnHeader();
            this._stateColumn = new ColumnHeader();
            this._rollbackColumn = new ColumnHeader();
            this._replayColumn = new ColumnHeader();
            this._targetsColumn = new ColumnHeader();
            this._afterHashColumn = new ColumnHeader();
            this._handoffLabel = new Label();
            this._diagnosticsTextBox = new TextBox();
            ((ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._rightPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _statusLabel
            //
            this._statusLabel.Dock = DockStyle.Top;
            this._statusLabel.Location = new Point(0, 0);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new Padding(8, 6, 8, 6);
            this._statusLabel.Size = new Size(1100, 34);
            this._statusLabel.TabIndex = 0;
            this._statusLabel.Text = "Goal 076 playable preview refresh";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 34);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Size = new Size(1100, 686);
            this._splitContainer.SplitterDistance = 380;
            this._splitContainer.TabIndex = 1;
            //
            // _hashesListView
            //
            this._hashesListView.Columns.AddRange(new ColumnHeader[]
            {
                this._hashNameColumn,
                this._hashValueColumn
            });
            this._hashesListView.Dock = DockStyle.Fill;
            this._hashesListView.FullRowSelect = true;
            this._hashesListView.GridLines = true;
            this._hashesListView.Name = "_hashesListView";
            this._hashesListView.TabIndex = 0;
            this._hashesListView.UseCompatibleStateImageBehavior = false;
            this._hashesListView.View = View.Details;
            //
            // _hash columns
            //
            this._hashNameColumn.Text = "hash";
            this._hashNameColumn.Width = 180;
            this._hashValueColumn.Text = "value";
            this._hashValueColumn.Width = 420;
            //
            // _rightPanel
            //
            this._rightPanel.Controls.Add(this._rowsListView);
            this._rightPanel.Controls.Add(this._diagnosticsTextBox);
            this._rightPanel.Controls.Add(this._handoffLabel);
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.TabIndex = 0;
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
                this._replayColumn,
                this._targetsColumn,
                this._afterHashColumn
            });
            this._rowsListView.Dock = DockStyle.Fill;
            this._rowsListView.FullRowSelect = true;
            this._rowsListView.GridLines = true;
            this._rowsListView.Name = "_rowsListView";
            this._rowsListView.TabIndex = 1;
            this._rowsListView.UseCompatibleStateImageBehavior = false;
            this._rowsListView.View = View.Details;
            //
            // _handoffLabel
            //
            this._handoffLabel.Dock = DockStyle.Top;
            this._handoffLabel.Location = new Point(0, 0);
            this._handoffLabel.Name = "_handoffLabel";
            this._handoffLabel.Padding = new Padding(8, 6, 8, 6);
            this._handoffLabel.Size = new Size(716, 34);
            this._handoffLabel.TabIndex = 0;
            this._handoffLabel.Text = "Staged handoff";
            //
            // _diagnosticsTextBox
            //
            this._diagnosticsTextBox.Dock = DockStyle.Bottom;
            this._diagnosticsTextBox.Location = new Point(0, 486);
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.Name = "_diagnosticsTextBox";
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Vertical;
            this._diagnosticsTextBox.Size = new Size(716, 200);
            this._diagnosticsTextBox.TabIndex = 2;
            //
            // row columns
            //
            this._familyColumn.Text = "family";
            this._familyColumn.Width = 160;
            this._seedColumn.Text = "seed";
            this._seedColumn.Width = 110;
            this._rowColumn.Text = "row";
            this._rowColumn.Width = 260;
            this._stateColumn.Text = "state";
            this._stateColumn.Width = 95;
            this._rollbackColumn.Text = "rollback";
            this._rollbackColumn.Width = 110;
            this._replayColumn.Text = "replay";
            this._replayColumn.Width = 95;
            this._targetsColumn.Text = "targets";
            this._targetsColumn.Width = 70;
            this._afterHashColumn.Text = "afterHash";
            this._afterHashColumn.Width = 360;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._hashesListView);
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            //
            // CampaignPlayableRefreshControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignPlayableRefreshControl";
            this.Size = new Size(1100, 720);
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._rightPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
