#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms
{
    partial class MainForm
    {
        private IContainer components;
        private ListBox _navigation;
        private Panel _workspace;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;

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
            this._navigation = new ListBox();
            this._workspace = new Panel();
            this._statusStrip = new StatusStrip();
            this._statusLabel = new ToolStripStatusLabel();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _navigation
            // 
            this._navigation.DisplayMember = "Title";
            this._navigation.Dock = DockStyle.Left;
            this._navigation.FormattingEnabled = true;
            this._navigation.ItemHeight = 15;
            this._navigation.Location = new Point(0, 0);
            this._navigation.Name = "_navigation";
            this._navigation.Size = new Size(220, 778);
            this._navigation.TabIndex = 0;
            // 
            // _workspace
            // 
            this._workspace.BackColor = SystemColors.Control;
            this._workspace.Dock = DockStyle.Fill;
            this._workspace.Location = new Point(220, 0);
            this._workspace.Name = "_workspace";
            this._workspace.Size = new Size(1060, 778);
            this._workspace.TabIndex = 1;
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new ToolStripItem[]
            {
                this._statusLabel
            });
            this._statusStrip.Location = new Point(0, 778);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new Size(1280, 22);
            this._statusStrip.TabIndex = 2;
            // 
            // _statusLabel
            // 
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(126, 17);
            this._statusLabel.Text = "Проект игры не открыт";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1280, 800);
            this.Controls.Add(this._workspace);
            this.Controls.Add(this._navigation);
            this.Controls.Add(this._statusStrip);
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "LLMGameCreator";
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
