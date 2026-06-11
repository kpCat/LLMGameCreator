#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class RuntimeMapCanvas
    {
        private IContainer components;

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
            this.SuspendLayout();
            // 
            // RuntimeMapCanvas
            // 
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
            this.Name = "RuntimeMapCanvas";
            this.Size = new Size(640, 480);
            this.TabStop = true;
            this.ResumeLayout(false);
        }
    }
}
