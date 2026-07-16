#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class RegenerateGeneratedWorldDialog
    {
        private IContainer components;
        private TableLayoutPanel _layoutPanel;
        private Label _seedLabel;
        private TextBox _seedTextBox;
        private Label _modeLabel;
        private ComboBox _modeComboBox;
        private Label _presetLabel;
        private ComboBox _presetComboBox;
        private Label _advancedLabel;
        private Label _styleOverridesLabel;
        private TextBox _styleOverridesTextBox;
        private Label _variantOverridesLabel;
        private TextBox _variantOverridesTextBox;
        private Label _validationLabel;
        private FlowLayoutPanel _buttonsPanel;
        private Button _applyButton;
        private Button _cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._layoutPanel = new TableLayoutPanel();
            this._seedLabel = new Label();
            this._seedTextBox = new TextBox();
            this._modeLabel = new Label();
            this._modeComboBox = new ComboBox();
            this._presetLabel = new Label();
            this._presetComboBox = new ComboBox();
            this._advancedLabel = new Label();
            this._styleOverridesLabel = new Label();
            this._styleOverridesTextBox = new TextBox();
            this._variantOverridesLabel = new Label();
            this._variantOverridesTextBox = new TextBox();
            this._validationLabel = new Label();
            this._buttonsPanel = new FlowLayoutPanel();
            this._applyButton = new Button();
            this._cancelButton = new Button();
            this._layoutPanel.SuspendLayout();
            this._buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _layoutPanel
            //
            this._layoutPanel.ColumnCount = 2;
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175F));
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.Controls.Add(this._seedLabel, 0, 0);
            this._layoutPanel.Controls.Add(this._seedTextBox, 1, 0);
            this._layoutPanel.Controls.Add(this._modeLabel, 0, 1);
            this._layoutPanel.Controls.Add(this._modeComboBox, 1, 1);
            this._layoutPanel.Controls.Add(this._presetLabel, 0, 2);
            this._layoutPanel.Controls.Add(this._presetComboBox, 1, 2);
            this._layoutPanel.Controls.Add(this._advancedLabel, 0, 3);
            this._layoutPanel.SetColumnSpan(this._advancedLabel, 2);
            this._layoutPanel.Controls.Add(this._styleOverridesLabel, 0, 4);
            this._layoutPanel.Controls.Add(this._styleOverridesTextBox, 1, 4);
            this._layoutPanel.Controls.Add(this._variantOverridesLabel, 0, 5);
            this._layoutPanel.Controls.Add(this._variantOverridesTextBox, 1, 5);
            this._layoutPanel.Controls.Add(this._validationLabel, 0, 6);
            this._layoutPanel.SetColumnSpan(this._validationLabel, 2);
            this._layoutPanel.Controls.Add(this._buttonsPanel, 0, 7);
            this._layoutPanel.SetColumnSpan(this._buttonsPanel, 2);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Padding = new Padding(12);
            this._layoutPanel.RowCount = 8;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            //
            // labels and editors
            //
            this._seedLabel.Dock = DockStyle.Fill;
            this._seedLabel.Text = "Seed:";
            this._seedLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._seedTextBox.Dock = DockStyle.Fill;
            this._modeLabel.Dock = DockStyle.Fill;
            this._modeLabel.Text = "Режим:";
            this._modeLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._modeComboBox.Dock = DockStyle.Fill;
            this._modeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._presetLabel.Dock = DockStyle.Fill;
            this._presetLabel.Text = "Пресет:";
            this._presetLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._presetComboBox.Dock = DockStyle.Fill;
            this._presetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._advancedLabel.Dock = DockStyle.Fill;
            this._advancedLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._advancedLabel.Text = "Расширенные переопределения (по одному ID в строке)";
            this._advancedLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._styleOverridesLabel.Dock = DockStyle.Fill;
            this._styleOverridesLabel.Text = "Style hint IDs:";
            this._styleOverridesLabel.TextAlign = ContentAlignment.TopLeft;
            this._styleOverridesTextBox.Dock = DockStyle.Fill;
            this._styleOverridesTextBox.Multiline = true;
            this._styleOverridesTextBox.ScrollBars = ScrollBars.Vertical;
            this._variantOverridesLabel.Dock = DockStyle.Fill;
            this._variantOverridesLabel.Text = "Variant IDs:";
            this._variantOverridesLabel.TextAlign = ContentAlignment.TopLeft;
            this._variantOverridesTextBox.Dock = DockStyle.Fill;
            this._variantOverridesTextBox.Multiline = true;
            this._variantOverridesTextBox.ScrollBars = ScrollBars.Vertical;
            this._validationLabel.Dock = DockStyle.Fill;
            this._validationLabel.ForeColor = Color.FromArgb(130, 55, 20);
            this._validationLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _buttonsPanel
            //
            this._buttonsPanel.Controls.Add(this._applyButton);
            this._buttonsPanel.Controls.Add(this._cancelButton);
            this._buttonsPanel.Dock = DockStyle.Fill;
            this._buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
            this._applyButton.AutoSize = true;
            this._applyButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._applyButton.Text = "Перегенерировать";
            this._applyButton.UseVisualStyleBackColor = true;
            this._cancelButton.AutoSize = true;
            this._cancelButton.Text = "Отмена";
            this._cancelButton.UseVisualStyleBackColor = true;
            //
            // RegenerateGeneratedWorldDialog
            //
            this.AcceptButton = this._applyButton;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new Size(650, 500);
            this.Controls.Add(this._layoutPanel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Перегенерация мира";
            this._layoutPanel.ResumeLayout(false);
            this._layoutPanel.PerformLayout();
            this._buttonsPanel.ResumeLayout(false);
            this._buttonsPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
