#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class DashboardPageControl
    {
        private IContainer components;
        private Label _summaryLabel;

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
            this.SuspendLayout();
            // 
            // _summaryLabel
            // 
            this._summaryLabel.Dock = DockStyle.Fill;
            this._summaryLabel.Font = new Font(FontFamily.GenericSansSerif, 12F, FontStyle.Regular, GraphicsUnit.Point);
            this._summaryLabel.Location = new Point(0, 0);
            this._summaryLabel.Name = "_summaryLabel";
            this._summaryLabel.Size = new Size(800, 450);
            this._summaryLabel.TabIndex = 0;
            this._summaryLabel.Text = "LLMGameCreator v0.1 skeleton\r\n\r\nЦель: GamePackage + typed Lua + asset catalog + headless runtime + WinForms editor shell.";
            this._summaryLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DashboardPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._summaryLabel);
            this.Name = "DashboardPageControl";
            this.Size = new Size(800, 450);
            this.ResumeLayout(false);
        }
    }
}
