#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignEditValidateApplyLoopControl
    {
        private IContainer components;
        private Label _statusLabel;
        private Panel _rowPanel;
        private Label _rowLabel;
        private ComboBox _rowComboBox;
        private TableLayoutPanel _layoutPanel;
        private CampaignEditFieldSummaryControl _fieldSummaryControl;
        private CampaignEditValidationControl _validationControl;
        private CampaignEditApplyRollbackControl _applyRollbackControl;

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
            this._statusLabel = new Label();
            this._rowPanel = new Panel();
            this._rowLabel = new Label();
            this._rowComboBox = new ComboBox();
            this._layoutPanel = new TableLayoutPanel();
            this._fieldSummaryControl = new CampaignEditFieldSummaryControl();
            this._validationControl = new CampaignEditValidationControl();
            this._applyRollbackControl = new CampaignEditApplyRollbackControl();
            this._rowPanel.SuspendLayout();
            this._layoutPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _statusLabel
            //
            this._statusLabel.Dock = DockStyle.Top;
            this._statusLabel.Location = new Point(0, 0);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new Padding(8, 6, 8, 6);
            this._statusLabel.Size = new Size(1100, 34);
            this._statusLabel.TabIndex = 0;
            this._statusLabel.Text = "Goal 075 edit loop";
            //
            // _rowPanel
            //
            this._rowPanel.Controls.Add(this._rowComboBox);
            this._rowPanel.Controls.Add(this._rowLabel);
            this._rowPanel.Dock = DockStyle.Top;
            this._rowPanel.Location = new Point(0, 34);
            this._rowPanel.Name = "_rowPanel";
            this._rowPanel.Padding = new Padding(8, 6, 8, 6);
            this._rowPanel.Size = new Size(1100, 42);
            this._rowPanel.TabIndex = 1;
            //
            // _rowLabel
            //
            this._rowLabel.Dock = DockStyle.Left;
            this._rowLabel.Location = new Point(8, 6);
            this._rowLabel.Name = "_rowLabel";
            this._rowLabel.Size = new Size(90, 30);
            this._rowLabel.TabIndex = 0;
            this._rowLabel.Text = "Selected row";
            this._rowLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _rowComboBox
            //
            this._rowComboBox.Dock = DockStyle.Fill;
            this._rowComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._rowComboBox.FormattingEnabled = true;
            this._rowComboBox.Location = new Point(98, 6);
            this._rowComboBox.Name = "_rowComboBox";
            this._rowComboBox.Size = new Size(994, 23);
            this._rowComboBox.TabIndex = 1;
            this._rowComboBox.SelectedIndexChanged += this.RowComboBoxSelectedIndexChanged;
            //
            // _layoutPanel
            //
            this._layoutPanel.ColumnCount = 1;
            this._layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._layoutPanel.Controls.Add(this._fieldSummaryControl, 0, 0);
            this._layoutPanel.Controls.Add(this._validationControl, 0, 1);
            this._layoutPanel.Controls.Add(this._applyRollbackControl, 0, 2);
            this._layoutPanel.Dock = DockStyle.Fill;
            this._layoutPanel.Location = new Point(0, 76);
            this._layoutPanel.Name = "_layoutPanel";
            this._layoutPanel.RowCount = 3;
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            this._layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            this._layoutPanel.Size = new Size(1100, 644);
            this._layoutPanel.TabIndex = 2;
            //
            // child controls
            //
            this._fieldSummaryControl.Dock = DockStyle.Fill;
            this._validationControl.Dock = DockStyle.Fill;
            this._applyRollbackControl.Dock = DockStyle.Fill;
            //
            // CampaignEditValidateApplyLoopControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._layoutPanel);
            this.Controls.Add(this._rowPanel);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignEditValidateApplyLoopControl";
            this.Size = new Size(1100, 720);
            this._rowPanel.ResumeLayout(false);
            this._layoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
