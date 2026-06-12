#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryImportTabControl
    {
        private IContainer components;
        private Panel _rootPanel;
        private FlowLayoutPanel _toolbarPanel;
        private Button _importButton;
        private Button _refreshButton;
        private Label _databasePathLabel;
        private Label _statusLabel;
        private TextBox _summaryTextBox;

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
            this._summaryTextBox = new TextBox();
            this._statusLabel = new Label();
            this._databasePathLabel = new Label();
            this._toolbarPanel = new FlowLayoutPanel();
            this._importButton = new Button();
            this._refreshButton = new Button();
            this._rootPanel.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootPanel
            // 
            this._rootPanel.Controls.Add(this._summaryTextBox);
            this._rootPanel.Controls.Add(this._statusLabel);
            this._rootPanel.Controls.Add(this._databasePathLabel);
            this._rootPanel.Controls.Add(this._toolbarPanel);
            this._rootPanel.Dock = DockStyle.Fill;
            this._rootPanel.Location = new Point(0, 0);
            this._rootPanel.Name = "_rootPanel";
            this._rootPanel.Padding = new Padding(12);
            this._rootPanel.Size = new Size(760, 420);
            this._rootPanel.TabIndex = 0;
            // 
            // _summaryTextBox
            // 
            this._summaryTextBox.Dock = DockStyle.Fill;
            this._summaryTextBox.Location = new Point(12, 94);
            this._summaryTextBox.Multiline = true;
            this._summaryTextBox.Name = "_summaryTextBox";
            this._summaryTextBox.ReadOnly = true;
            this._summaryTextBox.ScrollBars = ScrollBars.Vertical;
            this._summaryTextBox.Size = new Size(736, 314);
            this._summaryTextBox.TabIndex = 3;
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = DockStyle.Top;
            this._statusLabel.Location = new Point(12, 70);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(736, 24);
            this._statusLabel.TabIndex = 2;
            this._statusLabel.Text = "Not initialized";
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _databasePathLabel
            // 
            this._databasePathLabel.Dock = DockStyle.Top;
            this._databasePathLabel.Location = new Point(12, 46);
            this._databasePathLabel.Name = "_databasePathLabel";
            this._databasePathLabel.Size = new Size(736, 24);
            this._databasePathLabel.TabIndex = 1;
            this._databasePathLabel.Text = "DB:";
            this._databasePathLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.Controls.Add(this._importButton);
            this._toolbarPanel.Controls.Add(this._refreshButton);
            this._toolbarPanel.Dock = DockStyle.Top;
            this._toolbarPanel.Location = new Point(12, 12);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Size = new Size(736, 34);
            this._toolbarPanel.TabIndex = 0;
            // 
            // _importButton
            // 
            this._importButton.Location = new Point(3, 3);
            this._importButton.Name = "_importButton";
            this._importButton.Size = new Size(180, 28);
            this._importButton.TabIndex = 0;
            this._importButton.Text = "Import generator-library";
            this._importButton.UseVisualStyleBackColor = true;
            // 
            // _refreshButton
            // 
            this._refreshButton.Location = new Point(189, 3);
            this._refreshButton.Name = "_refreshButton";
            this._refreshButton.Size = new Size(100, 28);
            this._refreshButton.TabIndex = 1;
            this._refreshButton.Text = "Refresh";
            this._refreshButton.UseVisualStyleBackColor = true;
            // 
            // GeneratorLibraryImportTabControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootPanel);
            this.Name = "GeneratorLibraryImportTabControl";
            this.Size = new Size(760, 420);
            this._rootPanel.ResumeLayout(false);
            this._rootPanel.PerformLayout();
            this._toolbarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
