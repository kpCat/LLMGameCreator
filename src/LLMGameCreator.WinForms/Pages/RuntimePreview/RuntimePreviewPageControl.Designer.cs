#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class RuntimePreviewPageControl
    {
        private IContainer components;
        private SplitContainer _rootSplitContainer;
        private Panel _leftPanel;
        private FlowLayoutPanel _toolbarPanel;
        private Button _startButton;
        private RuntimeMapCanvas _canvas;
        private TextBox _logTextBox;

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
            this._rootSplitContainer = new SplitContainer();
            this._leftPanel = new Panel();
            this._canvas = new RuntimeMapCanvas();
            this._toolbarPanel = new FlowLayoutPanel();
            this._startButton = new Button();
            this._logTextBox = new TextBox();
            ((ISupportInitialize)this._rootSplitContainer).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._leftPanel.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._leftPanel);
            this._rootSplitContainer.Panel2.Controls.Add(this._logTextBox);
            this._rootSplitContainer.Size = new Size(1100, 600);
            this._rootSplitContainer.SplitterDistance = 820;
            this._rootSplitContainer.TabIndex = 0;
            // 
            // _leftPanel
            // 
            this._leftPanel.Controls.Add(this._canvas);
            this._leftPanel.Controls.Add(this._toolbarPanel);
            this._leftPanel.Dock = DockStyle.Fill;
            this._leftPanel.Location = new Point(0, 0);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Padding = new Padding(12);
            this._leftPanel.Size = new Size(820, 600);
            this._leftPanel.TabIndex = 0;
            // 
            // _canvas
            // 
            this._canvas.BackColor = Color.Black;
            this._canvas.Dock = DockStyle.Fill;
            this._canvas.Location = new Point(12, 54);
            this._canvas.Name = "_canvas";
            this._canvas.Size = new Size(796, 534);
            this._canvas.TabIndex = 1;
            this._canvas.TabStop = true;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.Controls.Add(this._startButton);
            this._toolbarPanel.Dock = DockStyle.Top;
            this._toolbarPanel.Location = new Point(12, 12);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(796, 42);
            this._toolbarPanel.TabIndex = 0;
            // 
            // _startButton
            // 
            this._startButton.Location = new Point(3, 3);
            this._startButton.Name = "_startButton";
            this._startButton.Size = new Size(100, 30);
            this._startButton.TabIndex = 0;
            this._startButton.Text = "Старт";
            this._startButton.UseVisualStyleBackColor = true;
            // 
            // _logTextBox
            // 
            this._logTextBox.Dock = DockStyle.Fill;
            this._logTextBox.Location = new Point(0, 0);
            this._logTextBox.Multiline = true;
            this._logTextBox.Name = "_logTextBox";
            this._logTextBox.ReadOnly = true;
            this._logTextBox.ScrollBars = ScrollBars.Vertical;
            this._logTextBox.Size = new Size(276, 600);
            this._logTextBox.TabIndex = 0;
            // 
            // RuntimePreviewPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootSplitContainer);
            this.Name = "RuntimePreviewPageControl";
            this.Size = new Size(1100, 600);
            this._rootSplitContainer.Panel1.ResumeLayout(false);
            this._rootSplitContainer.Panel2.ResumeLayout(false);
            this._rootSplitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)this._rootSplitContainer).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._leftPanel.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
