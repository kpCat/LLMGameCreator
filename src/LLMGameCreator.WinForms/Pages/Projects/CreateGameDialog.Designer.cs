#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CreateGameDialog
    {
        private IContainer components;
        private TableLayoutPanel _layoutPanel;
        private Label _folderNameLabel;
        private TextBox _folderNameTextBox;
        private Label _titleLabel;
        private TextBox _titleTextBox;
        private Label _packageIdLabel;
        private TextBox _packageIdTextBox;
        private Label _versionLabel;
        private TextBox _versionTextBox;
        private FlowLayoutPanel _buttonsPanel;
        private Button _createButton;
        private Button _cancelButton;

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
            this._layoutPanel = new TableLayoutPanel();
            this._folderNameLabel = new Label();
            this._folderNameTextBox = new TextBox();
            this._titleLabel = new Label();
            this._titleTextBox = new TextBox();
            this._packageIdLabel = new Label();
            this._packageIdTextBox = new TextBox();
            this._versionLabel = new Label();
            this._versionTextBox = new TextBox();
            this._buttonsPanel = new FlowLayoutPanel();
            this._createButton = new Button();
            this._cancelButton = new Button();
            this._layoutPanel.SuspendLayout();
            this._buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layoutPanel
            // 
            this._layoutPanel.ColumnCount = 2;
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.Controls.Add(this._folderNameLabel, 0, 0);
            this._layoutPanel.Controls.Add(this._folderNameTextBox, 1, 0);
            this._layoutPanel.Controls.Add(this._titleLabel, 0, 1);
            this._layoutPanel.Controls.Add(this._titleTextBox, 1, 1);
            this._layoutPanel.Controls.Add(this._packageIdLabel, 0, 2);
            this._layoutPanel.Controls.Add(this._packageIdTextBox, 1, 2);
            this._layoutPanel.Controls.Add(this._versionLabel, 0, 3);
            this._layoutPanel.Controls.Add(this._versionTextBox, 1, 3);
            this._layoutPanel.Controls.Add(this._buttonsPanel, 0, 4);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Location = new Point(0, 0);
            this._layoutPanel.Name = "_layoutPanel";
            this._layoutPanel.Padding = new Padding(12);
            this._layoutPanel.RowCount = 5;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._layoutPanel.Size = new Size(420, 196);
            this._layoutPanel.TabIndex = 0;
            // 
            // _folderNameLabel
            // 
            this._folderNameLabel.Dock = DockStyle.Fill;
            this._folderNameLabel.Location = new Point(15, 12);
            this._folderNameLabel.Name = "_folderNameLabel";
            this._folderNameLabel.Size = new Size(104, 34);
            this._folderNameLabel.TabIndex = 0;
            this._folderNameLabel.Text = "Folder name:";
            this._folderNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _folderNameTextBox
            // 
            this._folderNameTextBox.Dock = DockStyle.Fill;
            this._folderNameTextBox.Location = new Point(125, 15);
            this._folderNameTextBox.Name = "_folderNameTextBox";
            this._folderNameTextBox.Size = new Size(280, 23);
            this._folderNameTextBox.TabIndex = 1;
            // 
            // _titleLabel
            // 
            this._titleLabel.Dock = DockStyle.Fill;
            this._titleLabel.Location = new Point(15, 46);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new Size(104, 34);
            this._titleLabel.TabIndex = 2;
            this._titleLabel.Text = "Title:";
            this._titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _titleTextBox
            // 
            this._titleTextBox.Dock = DockStyle.Fill;
            this._titleTextBox.Location = new Point(125, 49);
            this._titleTextBox.Name = "_titleTextBox";
            this._titleTextBox.Size = new Size(280, 23);
            this._titleTextBox.TabIndex = 3;
            // 
            // _packageIdLabel
            // 
            this._packageIdLabel.Dock = DockStyle.Fill;
            this._packageIdLabel.Location = new Point(15, 80);
            this._packageIdLabel.Name = "_packageIdLabel";
            this._packageIdLabel.Size = new Size(104, 34);
            this._packageIdLabel.TabIndex = 4;
            this._packageIdLabel.Text = "PackageId:";
            this._packageIdLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _packageIdTextBox
            // 
            this._packageIdTextBox.Dock = DockStyle.Fill;
            this._packageIdTextBox.Location = new Point(125, 83);
            this._packageIdTextBox.Name = "_packageIdTextBox";
            this._packageIdTextBox.Size = new Size(280, 23);
            this._packageIdTextBox.TabIndex = 5;
            // 
            // _versionLabel
            // 
            this._versionLabel.Dock = DockStyle.Fill;
            this._versionLabel.Location = new Point(15, 114);
            this._versionLabel.Name = "_versionLabel";
            this._versionLabel.Size = new Size(104, 34);
            this._versionLabel.TabIndex = 6;
            this._versionLabel.Text = "Version:";
            this._versionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _versionTextBox
            // 
            this._versionTextBox.Dock = DockStyle.Fill;
            this._versionTextBox.Location = new Point(125, 117);
            this._versionTextBox.Name = "_versionTextBox";
            this._versionTextBox.Size = new Size(280, 23);
            this._versionTextBox.TabIndex = 7;
            // 
            // _buttonsPanel
            // 
            this._layoutPanel.SetColumnSpan(this._buttonsPanel, 2);
            this._buttonsPanel.Controls.Add(this._createButton);
            this._buttonsPanel.Controls.Add(this._cancelButton);
            this._buttonsPanel.Dock = DockStyle.Fill;
            this._buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
            this._buttonsPanel.Location = new Point(15, 151);
            this._buttonsPanel.Name = "_buttonsPanel";
            this._buttonsPanel.Size = new Size(390, 38);
            this._buttonsPanel.TabIndex = 8;
            // 
            // _createButton
            // 
            this._createButton.Location = new Point(312, 3);
            this._createButton.Name = "_createButton";
            this._createButton.Size = new Size(75, 30);
            this._createButton.TabIndex = 0;
            this._createButton.Text = "Create";
            this._createButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Location = new Point(231, 3);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new Size(75, 30);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // CreateGameDialog
            // 
            this.AcceptButton = this._createButton;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new Size(420, 196);
            this.Controls.Add(this._layoutPanel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateGameDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "New game";
            this._layoutPanel.ResumeLayout(false);
            this._layoutPanel.PerformLayout();
            this._buttonsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
