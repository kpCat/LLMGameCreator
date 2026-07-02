#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignGamePackageRuntimePreviewPlaythroughControl
    {
        private IContainer components;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _hashesListView;
        private ColumnHeader _hashNameColumn;
        private ColumnHeader _hashValueColumn;
        private Panel _rightPanel;
        private Label _proofLabel;
        private ListView _commandsListView;
        private ColumnHeader _sequenceColumn;
        private ColumnHeader _commandTypeColumn;
        private ColumnHeader _scenarioColumn;
        private ColumnHeader _targetColumn;
        private ColumnHeader _statusColumn;
        private ColumnHeader _stateHashColumn;
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
            this._proofLabel = new Label();
            this._commandsListView = new ListView();
            this._sequenceColumn = new ColumnHeader();
            this._commandTypeColumn = new ColumnHeader();
            this._scenarioColumn = new ColumnHeader();
            this._targetColumn = new ColumnHeader();
            this._statusColumn = new ColumnHeader();
            this._stateHashColumn = new ColumnHeader();
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
            this._statusLabel.Text = "Goal 081 GamePackage runtime preview playthrough";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 34);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Size = new Size(1100, 686);
            this._splitContainer.SplitterDistance = 420;
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
            // hash columns
            //
            this._hashNameColumn.Text = "hash";
            this._hashNameColumn.Width = 220;
            this._hashValueColumn.Text = "value";
            this._hashValueColumn.Width = 460;
            //
            // _rightPanel
            //
            this._rightPanel.Controls.Add(this._commandsListView);
            this._rightPanel.Controls.Add(this._diagnosticsTextBox);
            this._rightPanel.Controls.Add(this._proofLabel);
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.TabIndex = 0;
            //
            // _proofLabel
            //
            this._proofLabel.Dock = DockStyle.Top;
            this._proofLabel.Location = new Point(0, 0);
            this._proofLabel.Name = "_proofLabel";
            this._proofLabel.Padding = new Padding(8, 6, 8, 6);
            this._proofLabel.Size = new Size(676, 34);
            this._proofLabel.TabIndex = 0;
            this._proofLabel.Text = "Runtime preview playthrough proof";
            //
            // _commandsListView
            //
            this._commandsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._sequenceColumn,
                this._commandTypeColumn,
                this._scenarioColumn,
                this._targetColumn,
                this._statusColumn,
                this._stateHashColumn
            });
            this._commandsListView.Dock = DockStyle.Fill;
            this._commandsListView.FullRowSelect = true;
            this._commandsListView.GridLines = true;
            this._commandsListView.Name = "_commandsListView";
            this._commandsListView.TabIndex = 1;
            this._commandsListView.UseCompatibleStateImageBehavior = false;
            this._commandsListView.View = View.Details;
            //
            // _diagnosticsTextBox
            //
            this._diagnosticsTextBox.Dock = DockStyle.Bottom;
            this._diagnosticsTextBox.Location = new Point(0, 486);
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.Name = "_diagnosticsTextBox";
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Vertical;
            this._diagnosticsTextBox.Size = new Size(676, 200);
            this._diagnosticsTextBox.TabIndex = 2;
            //
            // command columns
            //
            this._sequenceColumn.Text = "#";
            this._sequenceColumn.Width = 52;
            this._commandTypeColumn.Text = "command";
            this._commandTypeColumn.Width = 160;
            this._scenarioColumn.Text = "scenario";
            this._scenarioColumn.Width = 180;
            this._targetColumn.Text = "target";
            this._targetColumn.Width = 180;
            this._statusColumn.Text = "actions";
            this._statusColumn.Width = 90;
            this._stateHashColumn.Text = "state hash";
            this._stateHashColumn.Width = 460;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._hashesListView);
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            //
            // CampaignGamePackageRuntimePreviewPlaythroughControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignGamePackageRuntimePreviewPlaythroughControl";
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
