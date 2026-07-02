#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignReviewPackagePlaySessionControl
    {
        private IContainer components;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _hashesListView;
        private ColumnHeader _hashNameColumn;
        private ColumnHeader _hashValueColumn;
        private Panel _rightPanel;
        private ListView _commandsListView;
        private ColumnHeader _profileColumn;
        private ColumnHeader _rowColumn;
        private ColumnHeader _commandCountColumn;
        private ColumnHeader _sampleCommandsColumn;
        private Label _proofLabel;
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
            this._commandsListView = new ListView();
            this._profileColumn = new ColumnHeader();
            this._rowColumn = new ColumnHeader();
            this._commandCountColumn = new ColumnHeader();
            this._sampleCommandsColumn = new ColumnHeader();
            this._proofLabel = new Label();
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
            this._statusLabel.Text = "Goal 078 review package playable session";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Location = new Point(0, 34);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Size = new Size(1100, 686);
            this._splitContainer.SplitterDistance = 400;
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
            this._hashNameColumn.Width = 190;
            this._hashValueColumn.Text = "value";
            this._hashValueColumn.Width = 420;
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
            // _commandsListView
            //
            this._commandsListView.Columns.AddRange(new ColumnHeader[]
            {
                this._profileColumn,
                this._rowColumn,
                this._commandCountColumn,
                this._sampleCommandsColumn
            });
            this._commandsListView.Dock = DockStyle.Fill;
            this._commandsListView.FullRowSelect = true;
            this._commandsListView.GridLines = true;
            this._commandsListView.Name = "_commandsListView";
            this._commandsListView.TabIndex = 1;
            this._commandsListView.UseCompatibleStateImageBehavior = false;
            this._commandsListView.View = View.Details;
            //
            // _proofLabel
            //
            this._proofLabel.Dock = DockStyle.Top;
            this._proofLabel.Location = new Point(0, 0);
            this._proofLabel.Name = "_proofLabel";
            this._proofLabel.Padding = new Padding(8, 6, 8, 6);
            this._proofLabel.Size = new Size(696, 34);
            this._proofLabel.TabIndex = 0;
            this._proofLabel.Text = "Playable session proof";
            //
            // _diagnosticsTextBox
            //
            this._diagnosticsTextBox.Dock = DockStyle.Bottom;
            this._diagnosticsTextBox.Location = new Point(0, 486);
            this._diagnosticsTextBox.Multiline = true;
            this._diagnosticsTextBox.Name = "_diagnosticsTextBox";
            this._diagnosticsTextBox.ReadOnly = true;
            this._diagnosticsTextBox.ScrollBars = ScrollBars.Vertical;
            this._diagnosticsTextBox.Size = new Size(696, 200);
            this._diagnosticsTextBox.TabIndex = 2;
            //
            // command columns
            //
            this._profileColumn.Text = "profile";
            this._profileColumn.Width = 180;
            this._rowColumn.Text = "row";
            this._rowColumn.Width = 280;
            this._commandCountColumn.Text = "commands";
            this._commandCountColumn.Width = 90;
            this._sampleCommandsColumn.Text = "sample";
            this._sampleCommandsColumn.Width = 520;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._hashesListView);
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            //
            // CampaignReviewPackagePlaySessionControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignReviewPackagePlaySessionControl";
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
