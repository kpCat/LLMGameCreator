#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GenerationPageControl
    {
        private IContainer components;
        private TextBox _descriptionTextBox;

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
            this._descriptionTextBox = new TextBox();
            this.SuspendLayout();
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Dock = DockStyle.Fill;
            this._descriptionTextBox.Location = new Point(0, 0);
            this._descriptionTextBox.Multiline = true;
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.ReadOnly = true;
            this._descriptionTextBox.ScrollBars = ScrollBars.Vertical;
            this._descriptionTextBox.Size = new Size(800, 450);
            this._descriptionTextBox.TabIndex = 0;
            this._descriptionTextBox.Text = "Generation v0.1\r\n\r\nЗдесь позже будут:\r\n- LLM sessions;\r\n- job-based генерация;\r\n- context packs;\r\n- draft/patch workflow;\r\n- профили локальных моделей, включая другие ПК в LAN.\r\n\r\nRuntime LLM generation запрещён архитектурно.";
            // 
            // GenerationPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._descriptionTextBox);
            this.Name = "GenerationPageControl";
            this.Size = new Size(800, 450);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
