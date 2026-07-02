#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignUnityAlphaStreamingAssetsHandoffControl
    {
        private IContainer components;
        private Label _statusLabel;
        private SplitContainer _splitContainer;
        private ListView _hashesListView;
        private ColumnHeader _hashNameColumn;
        private ColumnHeader _hashValueColumn;
        private Panel _rightPanel;
        private Label _proofLabel;
        private ListView _payloadListView;
        private ColumnHeader _payloadPathColumn;
        private ColumnHeader _payloadRoleColumn;
        private ColumnHeader _payloadBytesColumn;
        private ColumnHeader _payloadHashColumn;
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
            this._payloadListView = new ListView();
            this._payloadPathColumn = new ColumnHeader();
            this._payloadRoleColumn = new ColumnHeader();
            this._payloadBytesColumn = new ColumnHeader();
            this._payloadHashColumn = new ColumnHeader();
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
            this._statusLabel.Text = "Goal 082 Unity Alpha StreamingAssets handoff";
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
            this._rightPanel.Controls.Add(this._payloadListView);
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
            this._proofLabel.Text = "StreamingAssets payload proof";
            //
            // _payloadListView
            //
            this._payloadListView.Columns.AddRange(new ColumnHeader[]
            {
                this._payloadPathColumn,
                this._payloadRoleColumn,
                this._payloadBytesColumn,
                this._payloadHashColumn
            });
            this._payloadListView.Dock = DockStyle.Fill;
            this._payloadListView.FullRowSelect = true;
            this._payloadListView.GridLines = true;
            this._payloadListView.Name = "_payloadListView";
            this._payloadListView.TabIndex = 1;
            this._payloadListView.UseCompatibleStateImageBehavior = false;
            this._payloadListView.View = View.Details;
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
            // payload columns
            //
            this._payloadPathColumn.Text = "payload file";
            this._payloadPathColumn.Width = 220;
            this._payloadRoleColumn.Text = "role";
            this._payloadRoleColumn.Width = 160;
            this._payloadBytesColumn.Text = "bytes";
            this._payloadBytesColumn.Width = 90;
            this._payloadHashColumn.Text = "sha256";
            this._payloadHashColumn.Width = 460;
            //
            // split panels
            //
            this._splitContainer.Panel1.Controls.Add(this._hashesListView);
            this._splitContainer.Panel2.Controls.Add(this._rightPanel);
            //
            // CampaignUnityAlphaStreamingAssetsHandoffControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignUnityAlphaStreamingAssetsHandoffControl";
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
