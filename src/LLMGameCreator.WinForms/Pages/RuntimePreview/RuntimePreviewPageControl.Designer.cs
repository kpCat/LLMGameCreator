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
        private TabControl _rightTabControl;
        private TabPage _logTabPage;
        private TabPage _generatedContentTabPage;
        private TextBox _logTextBox;
        private TextBox _generatedContentTextBox;

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
            this._rightTabControl = new TabControl();
            this._logTabPage = new TabPage();
            this._generatedContentTabPage = new TabPage();
            this._logTextBox = new TextBox();
            this._generatedContentTextBox = new TextBox();
            ((ISupportInitialize)this._rootSplitContainer).BeginInit();
            this._rootSplitContainer.Panel1.SuspendLayout();
            this._rootSplitContainer.Panel2.SuspendLayout();
            this._rootSplitContainer.SuspendLayout();
            this._leftPanel.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this._rightTabControl.SuspendLayout();
            this._logTabPage.SuspendLayout();
            this._generatedContentTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootSplitContainer
            // 
            this._rootSplitContainer.Dock = DockStyle.Fill;
            this._rootSplitContainer.Location = new Point(0, 0);
            this._rootSplitContainer.Name = "_rootSplitContainer";
            this._rootSplitContainer.Panel1.Controls.Add(this._leftPanel);
            this._rootSplitContainer.Panel2.Controls.Add(this._rightTabControl);
            this._rootSplitContainer.Size = new Size(1100, 600);
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
            // _rightTabControl
            // 
            this._rightTabControl.Controls.Add(this._logTabPage);
            this._rightTabControl.Controls.Add(this._generatedContentTabPage);
            this._rightTabControl.Dock = DockStyle.Fill;
            this._rightTabControl.Location = new Point(0, 0);
            this._rightTabControl.Name = "_rightTabControl";
            this._rightTabControl.SelectedIndex = 0;
            this._rightTabControl.Size = new Size(550, 600);
            this._rightTabControl.TabIndex = 0;
            // 
            // _logTabPage
            // 
            this._logTabPage.Controls.Add(this._logTextBox);
            this._logTabPage.Location = new Point(4, 24);
            this._logTabPage.Name = "_logTabPage";
            this._logTabPage.Padding = new Padding(3);
            this._logTabPage.Size = new Size(542, 572);
            this._logTabPage.TabIndex = 0;
            this._logTabPage.Text = "Log";
            this._logTabPage.UseVisualStyleBackColor = true;
            // 
            // _generatedContentTabPage
            // 
            this._generatedContentTabPage.Controls.Add(this._generatedContentTextBox);
            this._generatedContentTabPage.Location = new Point(4, 24);
            this._generatedContentTabPage.Name = "_generatedContentTabPage";
            this._generatedContentTabPage.Padding = new Padding(3);
            this._generatedContentTabPage.Size = new Size(542, 572);
            this._generatedContentTabPage.TabIndex = 1;
            this._generatedContentTabPage.Text = "Generated Content";
            this._generatedContentTabPage.UseVisualStyleBackColor = true;
            // 
            // _logTextBox
            // 
            this._logTextBox.Dock = DockStyle.Fill;
            this._logTextBox.Location = new Point(3, 3);
            this._logTextBox.Multiline = true;
            this._logTextBox.Name = "_logTextBox";
            this._logTextBox.ReadOnly = true;
            this._logTextBox.ScrollBars = ScrollBars.Vertical;
            this._logTextBox.Size = new Size(536, 566);
            this._logTextBox.TabIndex = 0;
            // 
            // _generatedContentTextBox
            // 
            this._generatedContentTextBox.Dock = DockStyle.Fill;
            this._generatedContentTextBox.Location = new Point(3, 3);
            this._generatedContentTextBox.Multiline = true;
            this._generatedContentTextBox.Name = "_generatedContentTextBox";
            this._generatedContentTextBox.ReadOnly = true;
            this._generatedContentTextBox.ScrollBars = ScrollBars.Vertical;
            this._generatedContentTextBox.Size = new Size(536, 566);
            this._generatedContentTextBox.TabIndex = 0;
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
            ((ISupportInitialize)this._rootSplitContainer).EndInit();
            this._rootSplitContainer.ResumeLayout(false);
            this._leftPanel.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._rightTabControl.ResumeLayout(false);
            this._logTabPage.ResumeLayout(false);
            this._logTabPage.PerformLayout();
            this._generatedContentTabPage.ResumeLayout(false);
            this._generatedContentTabPage.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
