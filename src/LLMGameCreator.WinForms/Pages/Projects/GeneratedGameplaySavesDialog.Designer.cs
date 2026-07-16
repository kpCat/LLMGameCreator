#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratedGameplaySavesDialog
    {
        private IContainer components;
        private TableLayoutPanel _layoutPanel;
        private ListView _savesListView;
        private Label _statusLabel;
        private FlowLayoutPanel _buttonsPanel;
        private Button _previewButton;
        private Button _applyButton;
        private Button _closeButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._layoutPanel = new TableLayoutPanel();
            this._savesListView = new ListView();
            this._statusLabel = new Label();
            this._buttonsPanel = new FlowLayoutPanel();
            this._previewButton = new Button();
            this._applyButton = new Button();
            this._closeButton = new Button();
            this._layoutPanel.SuspendLayout();
            this._buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _layoutPanel
            //
            this._layoutPanel.ColumnCount = 1;
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.Controls.Add(this._savesListView, 0, 0);
            this._layoutPanel.Controls.Add(this._statusLabel, 0, 1);
            this._layoutPanel.Controls.Add(this._buttonsPanel, 0, 2);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Padding = new Padding(12);
            this._layoutPanel.RowCount = 3;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            //
            // _savesListView
            //
            this._savesListView.Dock = DockStyle.Fill;
            this._savesListView.FullRowSelect = true;
            this._savesListView.GridLines = true;
            this._savesListView.HideSelection = false;
            this._savesListView.MultiSelect = false;
            this._savesListView.View = View.Details;
            this._savesListView.Columns.Add("Слот", 150);
            this._savesListView.Columns.Add("Статус", 220);
            this._savesListView.Columns.Add("Ревизия", 110);
            this._savesListView.Columns.Add("Мир сохранения", 190);
            this._savesListView.Columns.Add("Текущий мир", 190);
            this._savesListView.Columns.Add("Сохранено / сброшено", 170);
            //
            // _statusLabel
            //
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.ForeColor = Color.FromArgb(65, 75, 90);
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _buttonsPanel
            //
            this._buttonsPanel.Controls.Add(this._closeButton);
            this._buttonsPanel.Controls.Add(this._applyButton);
            this._buttonsPanel.Controls.Add(this._previewButton);
            this._buttonsPanel.Dock = DockStyle.Fill;
            this._buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
            this._previewButton.AutoSize = true;
            this._previewButton.Text = "Проверить перенос";
            this._previewButton.UseVisualStyleBackColor = true;
            this._applyButton.AutoSize = true;
            this._applyButton.Enabled = false;
            this._applyButton.Text = "Перенести в текущий мир";
            this._applyButton.UseVisualStyleBackColor = true;
            this._closeButton.AutoSize = true;
            this._closeButton.Text = "Закрыть";
            this._closeButton.UseVisualStyleBackColor = true;
            //
            // GeneratedGameplaySavesDialog
            //
            this.CancelButton = this._closeButton;
            this.ClientSize = new Size(1080, 500);
            this.Controls.Add(this._layoutPanel);
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Игровые сохранения";
            this._layoutPanel.ResumeLayout(false);
            this._buttonsPanel.ResumeLayout(false);
            this._buttonsPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
