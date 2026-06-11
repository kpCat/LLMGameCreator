#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class ValidationPageControl
    {
        private IContainer components;
        private Panel _rootPanel;
        private Button _validateButton;
        private ListBox _issuesListBox;

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
            this._rootPanel = new Panel();
            this._issuesListBox = new ListBox();
            this._validateButton = new Button();
            this._rootPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootPanel
            // 
            this._rootPanel.Controls.Add(this._issuesListBox);
            this._rootPanel.Controls.Add(this._validateButton);
            this._rootPanel.Dock = DockStyle.Fill;
            this._rootPanel.Location = new Point(0, 0);
            this._rootPanel.Name = "_rootPanel";
            this._rootPanel.Padding = new Padding(12);
            this._rootPanel.Size = new Size(800, 450);
            this._rootPanel.TabIndex = 0;
            // 
            // _issuesListBox
            // 
            this._issuesListBox.Dock = DockStyle.Fill;
            this._issuesListBox.FormattingEnabled = true;
            this._issuesListBox.ItemHeight = 15;
            this._issuesListBox.Location = new Point(12, 48);
            this._issuesListBox.Name = "_issuesListBox";
            this._issuesListBox.Size = new Size(776, 390);
            this._issuesListBox.TabIndex = 1;
            // 
            // _validateButton
            // 
            this._validateButton.Dock = DockStyle.Top;
            this._validateButton.Location = new Point(12, 12);
            this._validateButton.Name = "_validateButton";
            this._validateButton.Size = new Size(776, 36);
            this._validateButton.TabIndex = 0;
            this._validateButton.Text = "Проверить текущий GamePackage";
            this._validateButton.UseVisualStyleBackColor = true;
            // 
            // ValidationPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootPanel);
            this.Name = "ValidationPageControl";
            this.Size = new Size(800, 450);
            this._rootPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
