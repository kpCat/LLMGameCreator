namespace LLMGameCreator.WinForms.Pages
{
    partial class CompositionWorkbenchPageControl
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel _rootLayout;
        private FlowLayoutPanel _toolbarPanel;
        private Label _presetLabel;
        private ComboBox _presetComboBox;
        private Button _buildPreviewButton;
        private Button _exportReportButton;
        private Button _refreshReportsButton;
        private SplitContainer _splitContainer;
        private TableLayoutPanel _leftLayout;
        private Label _readinessLabel;
        private Label _readinessValueLabel;
        private Label _summaryLabel;
        private TextBox _summaryTextBox;
        private Label _savedReportsLabel;
        private ListBox _savedReportsList;
        private TextBox _markdownTextBox;
        private Label _statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeRuntime();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._rootLayout = new TableLayoutPanel();
            this._toolbarPanel = new FlowLayoutPanel();
            this._presetLabel = new Label();
            this._presetComboBox = new ComboBox();
            this._buildPreviewButton = new Button();
            this._exportReportButton = new Button();
            this._refreshReportsButton = new Button();
            this._splitContainer = new SplitContainer();
            this._leftLayout = new TableLayoutPanel();
            this._readinessLabel = new Label();
            this._readinessValueLabel = new Label();
            this._summaryLabel = new Label();
            this._summaryTextBox = new TextBox();
            this._savedReportsLabel = new Label();
            this._savedReportsList = new ListBox();
            this._markdownTextBox = new TextBox();
            this._statusLabel = new Label();
            this._rootLayout.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._splitContainer).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._leftLayout.SuspendLayout();
            this.SuspendLayout();
            //
            // _rootLayout
            //
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._toolbarPanel, 0, 0);
            this._rootLayout.Controls.Add(this._splitContainer, 0, 1);
            this._rootLayout.Controls.Add(this._statusLabel, 0, 2);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.RowCount = 3;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            //
            // _toolbarPanel
            //
            this._toolbarPanel.Controls.Add(this._presetLabel);
            this._toolbarPanel.Controls.Add(this._presetComboBox);
            this._toolbarPanel.Controls.Add(this._buildPreviewButton);
            this._toolbarPanel.Controls.Add(this._exportReportButton);
            this._toolbarPanel.Controls.Add(this._refreshReportsButton);
            this._toolbarPanel.Dock = DockStyle.Fill;
            this._toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
            this._toolbarPanel.Padding = new Padding(6, 7, 6, 4);
            this._toolbarPanel.WrapContents = false;
            //
            // toolbar controls
            //
            this._presetLabel.AutoSize = true;
            this._presetLabel.Margin = new Padding(3, 6, 3, 0);
            this._presetLabel.Text = "Blueprint preset:";
            this._presetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._presetComboBox.Margin = new Padding(3, 3, 8, 3);
            this._presetComboBox.Size = new Size(360, 23);
            this._buildPreviewButton.AutoSize = true;
            this._buildPreviewButton.Text = "Build preview report";
            this._exportReportButton.AutoSize = true;
            this._exportReportButton.Text = "Export report";
            this._refreshReportsButton.AutoSize = true;
            this._refreshReportsButton.Text = "Refresh reports";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.FixedPanel = FixedPanel.Panel1;
            this._splitContainer.Panel1.Controls.Add(this._leftLayout);
            this._splitContainer.Panel2.Controls.Add(this._markdownTextBox);
            this._splitContainer.SplitterDistance = 380;
            //
            // _leftLayout
            //
            this._leftLayout.ColumnCount = 1;
            this._leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._leftLayout.Controls.Add(this._readinessLabel, 0, 0);
            this._leftLayout.Controls.Add(this._readinessValueLabel, 0, 1);
            this._leftLayout.Controls.Add(this._summaryLabel, 0, 2);
            this._leftLayout.Controls.Add(this._summaryTextBox, 0, 3);
            this._leftLayout.Controls.Add(this._savedReportsLabel, 0, 4);
            this._leftLayout.Controls.Add(this._savedReportsList, 0, 5);
            this._leftLayout.Dock = DockStyle.Fill;
            this._leftLayout.Padding = new Padding(8);
            this._leftLayout.RowCount = 6;
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            //
            // left controls
            //
            this._readinessLabel.AutoSize = true;
            this._readinessLabel.Text = "Readiness";
            this._readinessValueLabel.AutoSize = true;
            this._readinessValueLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._readinessValueLabel.Text = "Not built";
            this._summaryLabel.AutoSize = true;
            this._summaryLabel.Text = "Diagnostics and recommended actions";
            this._summaryTextBox.Dock = DockStyle.Fill;
            this._summaryTextBox.Multiline = true;
            this._summaryTextBox.ReadOnly = true;
            this._summaryTextBox.ScrollBars = ScrollBars.Both;
            this._summaryTextBox.WordWrap = false;
            this._savedReportsLabel.AutoSize = true;
            this._savedReportsLabel.Margin = new Padding(3, 8, 3, 0);
            this._savedReportsLabel.Text = "Saved reports";
            this._savedReportsList.Dock = DockStyle.Fill;
            this._savedReportsList.IntegralHeight = false;
            //
            // _markdownTextBox
            //
            this._markdownTextBox.Dock = DockStyle.Fill;
            this._markdownTextBox.Font = new Font("Consolas", 10F);
            this._markdownTextBox.Multiline = true;
            this._markdownTextBox.ReadOnly = true;
            this._markdownTextBox.ScrollBars = ScrollBars.Both;
            this._markdownTextBox.WordWrap = false;
            //
            // _statusLabel
            //
            this._statusLabel.AutoEllipsis = true;
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Padding = new Padding(8, 6, 8, 0);
            this._statusLabel.Text = "Not loaded.";
            //
            // CompositionWorkbenchPageControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "CompositionWorkbenchPageControl";
            this.Size = new Size(1180, 780);
            this._rootLayout.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._toolbarPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this._splitContainer).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._leftLayout.ResumeLayout(false);
            this._leftLayout.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
