#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class SettingsPageControl
    {
        private IContainer components;
        private TextBox _settingsTextBox;

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
            this._settingsTextBox = new TextBox();
            this.SuspendLayout();
            // 
            // _settingsTextBox
            // 
            this._settingsTextBox.Dock = DockStyle.Fill;
            this._settingsTextBox.Location = new Point(0, 0);
            this._settingsTextBox.Multiline = true;
            this._settingsTextBox.Name = "_settingsTextBox";
            this._settingsTextBox.ReadOnly = true;
            this._settingsTextBox.ScrollBars = ScrollBars.Vertical;
            this._settingsTextBox.Size = new Size(800, 450);
            this._settingsTextBox.TabIndex = 0;
            // 
            // SettingsPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._settingsTextBox);
            this.Name = "SettingsPageControl";
            this.Size = new Size(800, 450);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
