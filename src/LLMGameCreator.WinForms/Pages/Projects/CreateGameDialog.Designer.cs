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
        private Label _creationKindLabel;
        private ComboBox _creationKindComboBox;
        private Label _folderNameLabel;
        private TextBox _folderNameTextBox;
        private Label _titleLabel;
        private TextBox _titleTextBox;
        private Label _packageIdLabel;
        private TextBox _packageIdTextBox;
        private Label _versionLabel;
        private TextBox _versionTextBox;
        private Label _seedLabel;
        private TextBox _seedTextBox;
        private Label _generationModeLabel;
        private ComboBox _generationModeComboBox;
        private Label _generationPresetLabel;
        private ComboBox _generationPresetComboBox;
        private Label _mechanicsProfileLabel;
        private ComboBox _mechanicsProfileComboBox;
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
            this._creationKindLabel = new Label();
            this._creationKindComboBox = new ComboBox();
            this._folderNameLabel = new Label();
            this._folderNameTextBox = new TextBox();
            this._titleLabel = new Label();
            this._titleTextBox = new TextBox();
            this._packageIdLabel = new Label();
            this._packageIdTextBox = new TextBox();
            this._versionLabel = new Label();
            this._versionTextBox = new TextBox();
            this._seedLabel = new Label();
            this._seedTextBox = new TextBox();
            this._generationModeLabel = new Label();
            this._generationModeComboBox = new ComboBox();
            this._generationPresetLabel = new Label();
            this._generationPresetComboBox = new ComboBox();
            this._mechanicsProfileLabel = new Label();
            this._mechanicsProfileComboBox = new ComboBox();
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
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.Controls.Add(this._creationKindLabel, 0, 0);
            this._layoutPanel.Controls.Add(this._creationKindComboBox, 1, 0);
            this._layoutPanel.Controls.Add(this._folderNameLabel, 0, 1);
            this._layoutPanel.Controls.Add(this._folderNameTextBox, 1, 1);
            this._layoutPanel.Controls.Add(this._titleLabel, 0, 2);
            this._layoutPanel.Controls.Add(this._titleTextBox, 1, 2);
            this._layoutPanel.Controls.Add(this._packageIdLabel, 0, 3);
            this._layoutPanel.Controls.Add(this._packageIdTextBox, 1, 3);
            this._layoutPanel.Controls.Add(this._versionLabel, 0, 4);
            this._layoutPanel.Controls.Add(this._versionTextBox, 1, 4);
            this._layoutPanel.Controls.Add(this._seedLabel, 0, 5);
            this._layoutPanel.Controls.Add(this._seedTextBox, 1, 5);
            this._layoutPanel.Controls.Add(this._generationModeLabel, 0, 6);
            this._layoutPanel.Controls.Add(this._generationModeComboBox, 1, 6);
            this._layoutPanel.Controls.Add(this._generationPresetLabel, 0, 7);
            this._layoutPanel.Controls.Add(this._generationPresetComboBox, 1, 7);
            this._layoutPanel.Controls.Add(this._mechanicsProfileLabel, 0, 8);
            this._layoutPanel.Controls.Add(this._mechanicsProfileComboBox, 1, 8);
            this._layoutPanel.Controls.Add(this._buttonsPanel, 0, 9);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Location = new Point(0, 0);
            this._layoutPanel.Name = "_layoutPanel";
            this._layoutPanel.Padding = new Padding(12);
            this._layoutPanel.RowCount = 10;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._layoutPanel.Size = new Size(520, 374);
            this._layoutPanel.TabIndex = 0;
            //
            // _creationKindLabel
            //
            this._creationKindLabel.Dock = DockStyle.Fill;
            this._creationKindLabel.Location = new Point(15, 12);
            this._creationKindLabel.Name = "_creationKindLabel";
            this._creationKindLabel.Size = new Size(144, 34);
            this._creationKindLabel.TabIndex = 0;
            this._creationKindLabel.Text = "Тип проекта:";
            this._creationKindLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _creationKindComboBox
            //
            this._creationKindComboBox.Dock = DockStyle.Fill;
            this._creationKindComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._creationKindComboBox.FormattingEnabled = true;
            this._creationKindComboBox.Location = new Point(165, 15);
            this._creationKindComboBox.Name = "_creationKindComboBox";
            this._creationKindComboBox.Size = new Size(340, 23);
            this._creationKindComboBox.TabIndex = 1;
            //
            // _folderNameLabel
            //
            this._folderNameLabel.Dock = DockStyle.Fill;
            this._folderNameLabel.Location = new Point(15, 46);
            this._folderNameLabel.Name = "_folderNameLabel";
            this._folderNameLabel.Size = new Size(144, 34);
            this._folderNameLabel.TabIndex = 2;
            this._folderNameLabel.Text = "Папка:";
            this._folderNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _folderNameTextBox
            // 
            this._folderNameTextBox.Dock = DockStyle.Fill;
            this._folderNameTextBox.Location = new Point(165, 49);
            this._folderNameTextBox.Name = "_folderNameTextBox";
            this._folderNameTextBox.Size = new Size(340, 23);
            this._folderNameTextBox.TabIndex = 3;
            // 
            // _titleLabel
            // 
            this._titleLabel.Dock = DockStyle.Fill;
            this._titleLabel.Location = new Point(15, 80);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new Size(144, 34);
            this._titleLabel.TabIndex = 4;
            this._titleLabel.Text = "Название:";
            this._titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _titleTextBox
            // 
            this._titleTextBox.Dock = DockStyle.Fill;
            this._titleTextBox.Location = new Point(165, 83);
            this._titleTextBox.Name = "_titleTextBox";
            this._titleTextBox.Size = new Size(340, 23);
            this._titleTextBox.TabIndex = 5;
            // 
            // _packageIdLabel
            // 
            this._packageIdLabel.Dock = DockStyle.Fill;
            this._packageIdLabel.Location = new Point(15, 114);
            this._packageIdLabel.Name = "_packageIdLabel";
            this._packageIdLabel.Size = new Size(144, 34);
            this._packageIdLabel.TabIndex = 6;
            this._packageIdLabel.Text = "Идентификатор:";
            this._packageIdLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _packageIdTextBox
            // 
            this._packageIdTextBox.Dock = DockStyle.Fill;
            this._packageIdTextBox.Location = new Point(165, 117);
            this._packageIdTextBox.Name = "_packageIdTextBox";
            this._packageIdTextBox.Size = new Size(340, 23);
            this._packageIdTextBox.TabIndex = 7;
            // 
            // _versionLabel
            // 
            this._versionLabel.Dock = DockStyle.Fill;
            this._versionLabel.Location = new Point(15, 148);
            this._versionLabel.Name = "_versionLabel";
            this._versionLabel.Size = new Size(144, 34);
            this._versionLabel.TabIndex = 8;
            this._versionLabel.Text = "Версия:";
            this._versionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _versionTextBox
            // 
            this._versionTextBox.Dock = DockStyle.Fill;
            this._versionTextBox.Location = new Point(165, 151);
            this._versionTextBox.Name = "_versionTextBox";
            this._versionTextBox.Size = new Size(340, 23);
            this._versionTextBox.TabIndex = 9;
            //
            // _seedLabel
            //
            this._seedLabel.Dock = DockStyle.Fill;
            this._seedLabel.Location = new Point(15, 182);
            this._seedLabel.Name = "_seedLabel";
            this._seedLabel.Size = new Size(144, 34);
            this._seedLabel.TabIndex = 10;
            this._seedLabel.Text = "Seed:";
            this._seedLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _seedTextBox
            //
            this._seedTextBox.Dock = DockStyle.Fill;
            this._seedTextBox.Location = new Point(165, 185);
            this._seedTextBox.Name = "_seedTextBox";
            this._seedTextBox.Size = new Size(340, 23);
            this._seedTextBox.TabIndex = 11;
            //
            // _generationModeLabel
            //
            this._generationModeLabel.Dock = DockStyle.Fill;
            this._generationModeLabel.Location = new Point(15, 216);
            this._generationModeLabel.Name = "_generationModeLabel";
            this._generationModeLabel.Size = new Size(144, 34);
            this._generationModeLabel.TabIndex = 12;
            this._generationModeLabel.Text = "Режим:";
            this._generationModeLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _generationModeComboBox
            //
            this._generationModeComboBox.Dock = DockStyle.Fill;
            this._generationModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._generationModeComboBox.FormattingEnabled = true;
            this._generationModeComboBox.Location = new Point(165, 219);
            this._generationModeComboBox.Name = "_generationModeComboBox";
            this._generationModeComboBox.Size = new Size(340, 23);
            this._generationModeComboBox.TabIndex = 13;
            //
            // _generationPresetLabel
            //
            this._generationPresetLabel.Dock = DockStyle.Fill;
            this._generationPresetLabel.Location = new Point(15, 250);
            this._generationPresetLabel.Name = "_generationPresetLabel";
            this._generationPresetLabel.Size = new Size(144, 34);
            this._generationPresetLabel.TabIndex = 14;
            this._generationPresetLabel.Text = "Пресет:";
            this._generationPresetLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _generationPresetComboBox
            //
            this._generationPresetComboBox.Dock = DockStyle.Fill;
            this._generationPresetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._generationPresetComboBox.FormattingEnabled = true;
            this._generationPresetComboBox.Location = new Point(165, 253);
            this._generationPresetComboBox.Name = "_generationPresetComboBox";
            this._generationPresetComboBox.Size = new Size(340, 23);
            this._generationPresetComboBox.TabIndex = 15;
            //
            // _mechanicsProfileLabel
            //
            this._mechanicsProfileLabel.Dock = DockStyle.Fill;
            this._mechanicsProfileLabel.Location = new Point(15, 284);
            this._mechanicsProfileLabel.Name = "_mechanicsProfileLabel";
            this._mechanicsProfileLabel.Size = new Size(144, 34);
            this._mechanicsProfileLabel.TabIndex = 16;
            this._mechanicsProfileLabel.Text = "Профиль механик:";
            this._mechanicsProfileLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _mechanicsProfileComboBox
            //
            this._mechanicsProfileComboBox.Dock = DockStyle.Fill;
            this._mechanicsProfileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._mechanicsProfileComboBox.FormattingEnabled = true;
            this._mechanicsProfileComboBox.Location = new Point(165, 287);
            this._mechanicsProfileComboBox.Name = "_mechanicsProfileComboBox";
            this._mechanicsProfileComboBox.Size = new Size(340, 23);
            this._mechanicsProfileComboBox.TabIndex = 17;
            // 
            // _buttonsPanel
            // 
            this._layoutPanel.SetColumnSpan(this._buttonsPanel, 2);
            this._buttonsPanel.Controls.Add(this._createButton);
            this._buttonsPanel.Controls.Add(this._cancelButton);
            this._buttonsPanel.Dock = DockStyle.Fill;
            this._buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
            this._buttonsPanel.Location = new Point(15, 321);
            this._buttonsPanel.Name = "_buttonsPanel";
            this._buttonsPanel.Size = new Size(490, 38);
            this._buttonsPanel.TabIndex = 18;
            // 
            // _createButton
            // 
            this._createButton.Location = new Point(402, 3);
            this._createButton.Name = "_createButton";
            this._createButton.Size = new Size(75, 30);
            this._createButton.TabIndex = 0;
            this._createButton.Text = "Создать";
            this._createButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Location = new Point(321, 3);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new Size(75, 30);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Отмена";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // CreateGameDialog
            // 
            this.AcceptButton = this._createButton;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new Size(520, 374);
            this.Controls.Add(this._layoutPanel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateGameDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Новая игра";
            this._layoutPanel.ResumeLayout(false);
            this._layoutPanel.PerformLayout();
            this._buttonsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
