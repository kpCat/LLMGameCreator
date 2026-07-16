#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratedWorldHistoryDialog
    {
        private IContainer components;
        private TableLayoutPanel _layoutPanel;
        private ListView _worldsListView;
        private Label _statusLabel;
        private FlowLayoutPanel _buttonsPanel;
        private Button _restoreButton;
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
            this._worldsListView = new ListView();
            this._statusLabel = new Label();
            this._buttonsPanel = new FlowLayoutPanel();
            this._restoreButton = new Button();
            this._cancelButton = new Button();
            this._layoutPanel.SuspendLayout();
            this._buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _layoutPanel
            //
            this._layoutPanel.ColumnCount = 1;
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.Controls.Add(this._worldsListView, 0, 0);
            this._layoutPanel.Controls.Add(this._statusLabel, 0, 1);
            this._layoutPanel.Controls.Add(this._buttonsPanel, 0, 2);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Padding = new Padding(12);
            this._layoutPanel.RowCount = 3;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            //
            // _worldsListView
            //
            this._worldsListView.Dock = DockStyle.Fill;
            this._worldsListView.FullRowSelect = true;
            this._worldsListView.GridLines = true;
            this._worldsListView.HideSelection = false;
            this._worldsListView.MultiSelect = false;
            this._worldsListView.View = View.Details;
            this._worldsListView.Columns.Add("Текущий", 70);
            this._worldsListView.Columns.Add("Seed", 145);
            this._worldsListView.Columns.Add("Режим", 170);
            this._worldsListView.Columns.Add("Пресет", 120);
            this._worldsListView.Columns.Add("Регионы", 75);
            this._worldsListView.Columns.Add("Фракции", 75);
            this._worldsListView.Columns.Add("Персонажи", 85);
            this._worldsListView.Columns.Add("Столкновения", 95);
            this._worldsListView.Columns.Add("Задания и события", 125);
            this._worldsListView.Columns.Add("Игровой старт", 180);
            this._worldsListView.Columns.Add("Маршрут назначения", 190);
            //
            // _statusLabel
            //
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.ForeColor = Color.FromArgb(65, 75, 90);
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _buttonsPanel
            //
            this._buttonsPanel.Controls.Add(this._restoreButton);
            this._buttonsPanel.Controls.Add(this._cancelButton);
            this._buttonsPanel.Dock = DockStyle.Fill;
            this._buttonsPanel.FlowDirection = FlowDirection.RightToLeft;
            this._restoreButton.AutoSize = true;
            this._restoreButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._restoreButton.Text = "Проверить и восстановить";
            this._restoreButton.UseVisualStyleBackColor = true;
            this._cancelButton.AutoSize = true;
            this._cancelButton.Text = "Отмена";
            this._cancelButton.UseVisualStyleBackColor = true;
            //
            // GeneratedWorldHistoryDialog
            //
            this.AcceptButton = this._restoreButton;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new Size(1200, 540);
            this.Controls.Add(this._layoutPanel);
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "История миров";
            this._layoutPanel.ResumeLayout(false);
            this._buttonsPanel.ResumeLayout(false);
            this._buttonsPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
