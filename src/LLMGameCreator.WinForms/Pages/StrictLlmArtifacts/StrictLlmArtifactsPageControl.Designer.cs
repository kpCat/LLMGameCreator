namespace LLMGameCreator.WinForms.Pages
{
    partial class StrictLlmArtifactsPageControl
    {
        private TableLayoutPanel _rootLayout;
        private TableLayoutPanel _inputLayout;
        private ComboBox _profileComboBox;
        private ComboBox _batchPresetComboBox;
        private ComboBox _contentLanguageComboBox;
        private CheckedListBox _contractList;
        private NumericUpDown _maxTokensInput;
        private NumericUpDown _temperatureInput;
        private CheckBox _repairCheckBox;
        private CheckBox _stageCheckBox;
        private TextBox _extraBriefTextBox;
        private Button _loadSelectionButton;
        private Button _previewPromptButton;
        private Button _generateButton;
        private Button _loadAuditButton;
        private Button _copyPromptButton;
        private Button _copyResultButton;
        private SplitContainer _splitContainer;
        private TableLayoutPanel _leftLayout;
        private TextBox _sourceTextBox;
        private TextBox _statusTextBox;
        private DataGridView _artifactGrid;
        private DataGridView _diagnosticsGrid;
        private TabControl _textTabs;
        private TextBox _promptTextBox;
        private TextBox _resultTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeRuntimeResources();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._rootLayout = new TableLayoutPanel();
            this._inputLayout = new TableLayoutPanel();
            this._profileComboBox = new ComboBox();
            this._batchPresetComboBox = new ComboBox();
            this._contentLanguageComboBox = new ComboBox();
            this._contractList = new CheckedListBox();
            this._maxTokensInput = new NumericUpDown();
            this._temperatureInput = new NumericUpDown();
            this._repairCheckBox = new CheckBox();
            this._stageCheckBox = new CheckBox();
            this._extraBriefTextBox = new TextBox();
            this._loadSelectionButton = new Button();
            this._previewPromptButton = new Button();
            this._generateButton = new Button();
            this._loadAuditButton = new Button();
            this._copyPromptButton = new Button();
            this._copyResultButton = new Button();
            this._splitContainer = new SplitContainer();
            this._leftLayout = new TableLayoutPanel();
            this._sourceTextBox = new TextBox();
            this._statusTextBox = new TextBox();
            this._artifactGrid = new DataGridView();
            this._diagnosticsGrid = new DataGridView();
            this._textTabs = new TabControl();
            this._promptTextBox = new TextBox();
            this._resultTextBox = new TextBox();
            Label profileLabel = new Label();
            Label contractsLabel = new Label();
            Label maxTokensLabel = new Label();
            Label temperatureLabel = new Label();
            Label extraBriefLabel = new Label();
            Label batchPresetLabel = new Label();
            Label contentLanguageLabel = new Label();
            TabPage promptTab = new TabPage();
            TabPage resultTab = new TabPage();
            ((System.ComponentModel.ISupportInitialize)this._maxTokensInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this._temperatureInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this._splitContainer).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._artifactGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this._diagnosticsGrid).BeginInit();
            this._rootLayout.SuspendLayout();
            this._inputLayout.SuspendLayout();
            this._leftLayout.SuspendLayout();
            this._textTabs.SuspendLayout();
            this.SuspendLayout();
            //
            // _rootLayout
            //
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._inputLayout, 0, 0);
            this._rootLayout.Controls.Add(this._splitContainer, 0, 1);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.Padding = new Padding(8);
            this._rootLayout.RowCount = 2;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 164F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            //
            // _inputLayout
            //
            this._inputLayout.ColumnCount = 8;
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
            this._inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
            this._inputLayout.Dock = DockStyle.Fill;
            this._inputLayout.RowCount = 5;
            this._inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            //
            // labels
            //
            profileLabel.Dock = DockStyle.Fill;
            profileLabel.Text = "LLM profile";
            profileLabel.TextAlign = ContentAlignment.MiddleLeft;
            contractsLabel.Dock = DockStyle.Fill;
            contractsLabel.Text = "Contracts";
            contractsLabel.TextAlign = ContentAlignment.MiddleLeft;
            maxTokensLabel.Dock = DockStyle.Fill;
            maxTokensLabel.Text = "Max tokens";
            maxTokensLabel.TextAlign = ContentAlignment.MiddleLeft;
            temperatureLabel.Dock = DockStyle.Fill;
            temperatureLabel.Text = "Temperature";
            temperatureLabel.TextAlign = ContentAlignment.MiddleLeft;
            extraBriefLabel.Dock = DockStyle.Fill;
            extraBriefLabel.Text = "Extra brief";
            extraBriefLabel.TextAlign = ContentAlignment.MiddleLeft;
            batchPresetLabel.Dock = DockStyle.Fill;
            batchPresetLabel.Text = "Batch preset";
            batchPresetLabel.TextAlign = ContentAlignment.MiddleLeft;
            contentLanguageLabel.Dock = DockStyle.Fill;
            contentLanguageLabel.Text = "Content language";
            contentLanguageLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._inputLayout.Controls.Add(profileLabel, 0, 0);
            this._inputLayout.Controls.Add(contractsLabel, 2, 0);
            this._inputLayout.Controls.Add(maxTokensLabel, 0, 1);
            this._inputLayout.Controls.Add(temperatureLabel, 0, 2);
            this._inputLayout.Controls.Add(batchPresetLabel, 4, 2);
            this._inputLayout.Controls.Add(contentLanguageLabel, 4, 3);
            this._inputLayout.Controls.Add(extraBriefLabel, 2, 4);
            //
            // inputs
            //
            this._profileComboBox.Dock = DockStyle.Fill;
            this._profileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._batchPresetComboBox.Dock = DockStyle.Fill;
            this._batchPresetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._contentLanguageComboBox.Dock = DockStyle.Fill;
            this._contentLanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._contractList.CheckOnClick = true;
            this._contractList.DisplayMember = "DisplayName";
            this._contractList.Dock = DockStyle.Fill;
            this._maxTokensInput.Dock = DockStyle.Fill;
            this._maxTokensInput.Maximum = new decimal(new int[] { 12000, 0, 0, 0 });
            this._maxTokensInput.Minimum = new decimal(new int[] { 256, 0, 0, 0 });
            this._maxTokensInput.Value = new decimal(new int[] { 4000, 0, 0, 0 });
            this._temperatureInput.DecimalPlaces = 2;
            this._temperatureInput.Dock = DockStyle.Fill;
            this._temperatureInput.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            this._temperatureInput.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            this._temperatureInput.Value = new decimal(new int[] { 2, 0, 0, 65536 });
            this._repairCheckBox.Checked = true;
            this._repairCheckBox.CheckState = CheckState.Checked;
            this._repairCheckBox.Dock = DockStyle.Fill;
            this._repairCheckBox.Text = "One repair";
            this._stageCheckBox.Checked = true;
            this._stageCheckBox.CheckState = CheckState.Checked;
            this._stageCheckBox.Dock = DockStyle.Fill;
            this._stageCheckBox.Text = "Stage for review";
            this._extraBriefTextBox.Dock = DockStyle.Fill;
            this._extraBriefTextBox.Multiline = true;
            this._extraBriefTextBox.ScrollBars = ScrollBars.Vertical;
            this._inputLayout.Controls.Add(this._profileComboBox, 1, 0);
            this._inputLayout.Controls.Add(this._contractList, 3, 0);
            this._inputLayout.SetRowSpan(this._contractList, 4);
            this._inputLayout.Controls.Add(this._maxTokensInput, 1, 1);
            this._inputLayout.Controls.Add(this._temperatureInput, 1, 2);
            this._inputLayout.Controls.Add(this._repairCheckBox, 1, 3);
            this._inputLayout.Controls.Add(this._stageCheckBox, 1, 4);
            this._inputLayout.Controls.Add(this._batchPresetComboBox, 5, 2);
            this._inputLayout.SetColumnSpan(this._batchPresetComboBox, 3);
            this._inputLayout.Controls.Add(this._contentLanguageComboBox, 5, 3);
            this._inputLayout.SetColumnSpan(this._contentLanguageComboBox, 3);
            this._inputLayout.Controls.Add(this._extraBriefTextBox, 3, 4);
            this._inputLayout.SetColumnSpan(this._extraBriefTextBox, 5);
            //
            // buttons
            //
            this._loadSelectionButton.Dock = DockStyle.Fill;
            this._loadSelectionButton.Text = "Load selection";
            this._loadSelectionButton.UseVisualStyleBackColor = true;
            this._previewPromptButton.Dock = DockStyle.Fill;
            this._previewPromptButton.Text = "Preview prompt";
            this._previewPromptButton.UseVisualStyleBackColor = true;
            this._generateButton.Dock = DockStyle.Fill;
            this._generateButton.Text = "Generate";
            this._generateButton.UseVisualStyleBackColor = true;
            this._loadAuditButton.Dock = DockStyle.Fill;
            this._loadAuditButton.Text = "Load audit";
            this._loadAuditButton.UseVisualStyleBackColor = true;
            this._copyPromptButton.Dock = DockStyle.Fill;
            this._copyPromptButton.Text = "Copy prompt";
            this._copyPromptButton.UseVisualStyleBackColor = true;
            this._copyResultButton.Dock = DockStyle.Fill;
            this._copyResultButton.Text = "Copy result JSON";
            this._copyResultButton.UseVisualStyleBackColor = true;
            this._inputLayout.Controls.Add(this._loadSelectionButton, 4, 0);
            this._inputLayout.Controls.Add(this._previewPromptButton, 5, 0);
            this._inputLayout.Controls.Add(this._generateButton, 6, 0);
            this._inputLayout.Controls.Add(this._loadAuditButton, 7, 0);
            this._inputLayout.Controls.Add(this._copyPromptButton, 6, 1);
            this._inputLayout.Controls.Add(this._copyResultButton, 7, 1);
            //
            // _splitContainer
            //
            this._splitContainer.Dock = DockStyle.Fill;
            this._splitContainer.Orientation = Orientation.Vertical;
            this._splitContainer.SplitterDistance = 560;
            this._splitContainer.Panel1.Controls.Add(this._leftLayout);
            this._splitContainer.Panel2.Controls.Add(this._textTabs);
            //
            // _leftLayout
            //
            this._leftLayout.ColumnCount = 1;
            this._leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._leftLayout.Dock = DockStyle.Fill;
            this._leftLayout.RowCount = 5;
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            this._leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            //
            // text boxes
            //
            this._sourceTextBox.Dock = DockStyle.Fill;
            this._sourceTextBox.Multiline = true;
            this._sourceTextBox.ReadOnly = true;
            this._sourceTextBox.ScrollBars = ScrollBars.Vertical;
            this._statusTextBox.Dock = DockStyle.Fill;
            this._statusTextBox.Multiline = true;
            this._statusTextBox.ReadOnly = true;
            this._statusTextBox.ScrollBars = ScrollBars.Vertical;
            this._promptTextBox.Dock = DockStyle.Fill;
            this._promptTextBox.Multiline = true;
            this._promptTextBox.ReadOnly = true;
            this._promptTextBox.ScrollBars = ScrollBars.Both;
            this._promptTextBox.WordWrap = false;
            this._resultTextBox.Dock = DockStyle.Fill;
            this._resultTextBox.Multiline = true;
            this._resultTextBox.ReadOnly = true;
            this._resultTextBox.ScrollBars = ScrollBars.Both;
            this._resultTextBox.WordWrap = false;
            //
            // grids
            //
            this._artifactGrid.AllowUserToAddRows = false;
            this._artifactGrid.AllowUserToDeleteRows = false;
            this._artifactGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._artifactGrid.Dock = DockStyle.Fill;
            this._artifactGrid.ReadOnly = true;
            this._artifactGrid.RowHeadersVisible = false;
            this._artifactGrid.Columns.Add("ArtifactId", "ArtifactId");
            this._artifactGrid.Columns.Add("Kind", "Kind");
            this._artifactGrid.Columns.Add("Contract", "Contract");
            this._artifactGrid.Columns.Add("Valid", "Valid");
            this._artifactGrid.Columns.Add("Repaired", "Repaired");
            this._artifactGrid.Columns.Add("RequiresApproval", "RequiresApproval");
            this._diagnosticsGrid.AllowUserToAddRows = false;
            this._diagnosticsGrid.AllowUserToDeleteRows = false;
            this._diagnosticsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._diagnosticsGrid.Dock = DockStyle.Fill;
            this._diagnosticsGrid.ReadOnly = true;
            this._diagnosticsGrid.RowHeadersVisible = false;
            this._diagnosticsGrid.Columns.Add("Severity", "Severity");
            this._diagnosticsGrid.Columns.Add("Code", "Code");
            this._diagnosticsGrid.Columns.Add("ContractId", "ContractId");
            this._diagnosticsGrid.Columns.Add("Target", "Target");
            this._diagnosticsGrid.Columns.Add("Message", "Message");
            this._leftLayout.Controls.Add(this._sourceTextBox, 0, 0);
            this._leftLayout.Controls.Add(this._statusTextBox, 0, 1);
            this._leftLayout.Controls.Add(this._artifactGrid, 0, 2);
            this._leftLayout.Controls.Add(this._diagnosticsGrid, 0, 3);
            //
            // tabs
            //
            promptTab.Padding = new Padding(3);
            promptTab.Text = "Prompt";
            promptTab.Controls.Add(this._promptTextBox);
            resultTab.Padding = new Padding(3);
            resultTab.Text = "Audit JSON";
            resultTab.Controls.Add(this._resultTextBox);
            this._textTabs.Dock = DockStyle.Fill;
            this._textTabs.TabPages.Add(promptTab);
            this._textTabs.TabPages.Add(resultTab);
            //
            // StrictLlmArtifactsPageControl
            //
            this.Controls.Add(this._rootLayout);
            this.Name = "StrictLlmArtifactsPageControl";
            this.Size = new Size(1180, 780);
            ((System.ComponentModel.ISupportInitialize)this._maxTokensInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)this._temperatureInput).EndInit();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._splitContainer).EndInit();
            this._splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._artifactGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)this._diagnosticsGrid).EndInit();
            this._rootLayout.ResumeLayout(false);
            this._inputLayout.ResumeLayout(false);
            this._inputLayout.PerformLayout();
            this._leftLayout.ResumeLayout(false);
            this._leftLayout.PerformLayout();
            this._textTabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
