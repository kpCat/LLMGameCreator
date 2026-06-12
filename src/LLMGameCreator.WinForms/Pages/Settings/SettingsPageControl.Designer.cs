#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class SettingsPageControl
    {
        private IContainer components;
        private TableLayoutPanel _rootLayout;
        private GroupBox _rootSettingsGroupBox;
        private TableLayoutPanel _rootSettingsLayout;
        private Label _gamesRootLabel;
        private TextBox _gamesRootTextBox;
        private Label _logsPathLabel;
        private TextBox _logsPathTextBox;
        private Label _defaultLlmProfileLabel;
        private ComboBox _defaultLlmProfileComboBox;
        private Label _defaultAssetProviderLabel;
        private TextBox _defaultAssetProviderTextBox;
        private SplitContainer _profilesSplitContainer;
        private GroupBox _profilesGroupBox;
        private ListView _profilesListView;
        private ColumnHeader _profileIdColumnHeader;
        private ColumnHeader _profileTitleColumnHeader;
        private ColumnHeader _profileEndpointColumnHeader;
        private ColumnHeader _profileModelColumnHeader;
        private ColumnHeader _profileContextColumnHeader;
        private ColumnHeader _profileRoleColumnHeader;
        private GroupBox _profileEditorGroupBox;
        private TableLayoutPanel _profileEditorLayout;
        private Label _profileIdLabel;
        private TextBox _profileIdTextBox;
        private Label _profileTitleLabel;
        private TextBox _profileTitleTextBox;
        private Label _profileEndpointLabel;
        private TextBox _profileEndpointTextBox;
        private Label _profileModelLabel;
        private TextBox _profileModelTextBox;
        private Label _profileContextLabel;
        private NumericUpDown _profileContextNumeric;
        private Label _profileRoleLabel;
        private ComboBox _profileRoleComboBox;
        private FlowLayoutPanel _buttonPanel;
        private Button _reloadButton;
        private Button _applyProfileButton;
        private Button _addProfileButton;
        private Button _removeProfileButton;
        private Button _saveButton;
        private Label _statusLabel;

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
            this._rootLayout = new TableLayoutPanel();
            this._rootSettingsGroupBox = new GroupBox();
            this._rootSettingsLayout = new TableLayoutPanel();
            this._gamesRootLabel = new Label();
            this._gamesRootTextBox = new TextBox();
            this._logsPathLabel = new Label();
            this._logsPathTextBox = new TextBox();
            this._defaultLlmProfileLabel = new Label();
            this._defaultLlmProfileComboBox = new ComboBox();
            this._defaultAssetProviderLabel = new Label();
            this._defaultAssetProviderTextBox = new TextBox();
            this._profilesSplitContainer = new SplitContainer();
            this._profilesGroupBox = new GroupBox();
            this._profilesListView = new ListView();
            this._profileIdColumnHeader = new ColumnHeader();
            this._profileTitleColumnHeader = new ColumnHeader();
            this._profileEndpointColumnHeader = new ColumnHeader();
            this._profileModelColumnHeader = new ColumnHeader();
            this._profileContextColumnHeader = new ColumnHeader();
            this._profileRoleColumnHeader = new ColumnHeader();
            this._profileEditorGroupBox = new GroupBox();
            this._profileEditorLayout = new TableLayoutPanel();
            this._profileIdLabel = new Label();
            this._profileIdTextBox = new TextBox();
            this._profileTitleLabel = new Label();
            this._profileTitleTextBox = new TextBox();
            this._profileEndpointLabel = new Label();
            this._profileEndpointTextBox = new TextBox();
            this._profileModelLabel = new Label();
            this._profileModelTextBox = new TextBox();
            this._profileContextLabel = new Label();
            this._profileContextNumeric = new NumericUpDown();
            this._profileRoleLabel = new Label();
            this._profileRoleComboBox = new ComboBox();
            this._buttonPanel = new FlowLayoutPanel();
            this._reloadButton = new Button();
            this._applyProfileButton = new Button();
            this._addProfileButton = new Button();
            this._removeProfileButton = new Button();
            this._saveButton = new Button();
            this._statusLabel = new Label();
            this._rootLayout.SuspendLayout();
            this._rootSettingsGroupBox.SuspendLayout();
            this._rootSettingsLayout.SuspendLayout();
            ((ISupportInitialize)(this._profilesSplitContainer)).BeginInit();
            this._profilesSplitContainer.Panel1.SuspendLayout();
            this._profilesSplitContainer.Panel2.SuspendLayout();
            this._profilesSplitContainer.SuspendLayout();
            this._profilesGroupBox.SuspendLayout();
            this._profileEditorGroupBox.SuspendLayout();
            this._profileEditorLayout.SuspendLayout();
            ((ISupportInitialize)(this._profileContextNumeric)).BeginInit();
            this._buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootLayout
            // 
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._rootSettingsGroupBox, 0, 0);
            this._rootLayout.Controls.Add(this._profilesSplitContainer, 0, 1);
            this._rootLayout.Controls.Add(this._buttonPanel, 0, 2);
            this._rootLayout.Controls.Add(this._statusLabel, 0, 3);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.Location = new Point(0, 0);
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.Padding = new Padding(12);
            this._rootLayout.RowCount = 4;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 128F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._rootLayout.Size = new Size(1000, 650);
            this._rootLayout.TabIndex = 0;
            // 
            // _rootSettingsGroupBox
            // 
            this._rootSettingsGroupBox.Controls.Add(this._rootSettingsLayout);
            this._rootSettingsGroupBox.Dock = DockStyle.Fill;
            this._rootSettingsGroupBox.Location = new Point(15, 15);
            this._rootSettingsGroupBox.Name = "_rootSettingsGroupBox";
            this._rootSettingsGroupBox.Padding = new Padding(8);
            this._rootSettingsGroupBox.Size = new Size(970, 122);
            this._rootSettingsGroupBox.TabIndex = 0;
            this._rootSettingsGroupBox.TabStop = false;
            this._rootSettingsGroupBox.Text = "Root settings";
            // 
            // _rootSettingsLayout
            // 
            this._rootSettingsLayout.ColumnCount = 4;
            this._rootSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            this._rootSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._rootSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            this._rootSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._rootSettingsLayout.Controls.Add(this._gamesRootLabel, 0, 0);
            this._rootSettingsLayout.Controls.Add(this._gamesRootTextBox, 1, 0);
            this._rootSettingsLayout.Controls.Add(this._logsPathLabel, 2, 0);
            this._rootSettingsLayout.Controls.Add(this._logsPathTextBox, 3, 0);
            this._rootSettingsLayout.Controls.Add(this._defaultLlmProfileLabel, 0, 1);
            this._rootSettingsLayout.Controls.Add(this._defaultLlmProfileComboBox, 1, 1);
            this._rootSettingsLayout.Controls.Add(this._defaultAssetProviderLabel, 2, 1);
            this._rootSettingsLayout.Controls.Add(this._defaultAssetProviderTextBox, 3, 1);
            this._rootSettingsLayout.Dock = DockStyle.Fill;
            this._rootSettingsLayout.RowCount = 2;
            this._rootSettingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._rootSettingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._rootSettingsLayout.TabIndex = 0;
            this._gamesRootLabel.Dock = DockStyle.Fill;
            this._gamesRootLabel.Text = "GamesRootPath:";
            this._gamesRootLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._gamesRootTextBox.Dock = DockStyle.Fill;
            this._logsPathLabel.Dock = DockStyle.Fill;
            this._logsPathLabel.Text = "LogsPath:";
            this._logsPathLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._logsPathTextBox.Dock = DockStyle.Fill;
            this._defaultLlmProfileLabel.Dock = DockStyle.Fill;
            this._defaultLlmProfileLabel.Text = "DefaultLlmProfileId:";
            this._defaultLlmProfileLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._defaultLlmProfileComboBox.Dock = DockStyle.Fill;
            this._defaultLlmProfileComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._defaultAssetProviderLabel.Dock = DockStyle.Fill;
            this._defaultAssetProviderLabel.Text = "DefaultAssetProviderId:";
            this._defaultAssetProviderLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._defaultAssetProviderTextBox.Dock = DockStyle.Fill;
            // 
            // _profilesSplitContainer
            // 
            this._profilesSplitContainer.Dock = DockStyle.Fill;
            this._profilesSplitContainer.Location = new Point(15, 143);
            this._profilesSplitContainer.Name = "_profilesSplitContainer";
            this._profilesSplitContainer.Panel1.Controls.Add(this._profilesGroupBox);
            this._profilesSplitContainer.Panel2.Controls.Add(this._profileEditorGroupBox);
            this._profilesSplitContainer.Size = new Size(970, 418);
            this._profilesSplitContainer.SplitterDistance = 610;
            this._profilesSplitContainer.TabIndex = 1;
            // 
            // _profilesGroupBox
            // 
            this._profilesGroupBox.Controls.Add(this._profilesListView);
            this._profilesGroupBox.Dock = DockStyle.Fill;
            this._profilesGroupBox.Location = new Point(0, 0);
            this._profilesGroupBox.Name = "_profilesGroupBox";
            this._profilesGroupBox.Padding = new Padding(8);
            this._profilesGroupBox.Size = new Size(610, 418);
            this._profilesGroupBox.TabIndex = 0;
            this._profilesGroupBox.TabStop = false;
            this._profilesGroupBox.Text = "LLM profiles";
            this._profilesListView.Columns.AddRange(new ColumnHeader[] { this._profileIdColumnHeader, this._profileTitleColumnHeader, this._profileEndpointColumnHeader, this._profileModelColumnHeader, this._profileContextColumnHeader, this._profileRoleColumnHeader });
            this._profilesListView.Dock = DockStyle.Fill;
            this._profilesListView.FullRowSelect = true;
            this._profilesListView.HideSelection = false;
            this._profilesListView.MultiSelect = false;
            this._profilesListView.UseCompatibleStateImageBehavior = false;
            this._profilesListView.View = View.Details;
            this._profileIdColumnHeader.Text = "Id";
            this._profileIdColumnHeader.Width = 110;
            this._profileTitleColumnHeader.Text = "Title";
            this._profileTitleColumnHeader.Width = 130;
            this._profileEndpointColumnHeader.Text = "Endpoint";
            this._profileEndpointColumnHeader.Width = 170;
            this._profileModelColumnHeader.Text = "Model";
            this._profileModelColumnHeader.Width = 110;
            this._profileContextColumnHeader.Text = "Context";
            this._profileContextColumnHeader.Width = 70;
            this._profileRoleColumnHeader.Text = "Role";
            this._profileRoleColumnHeader.Width = 80;
            // 
            // _profileEditorGroupBox
            // 
            this._profileEditorGroupBox.Controls.Add(this._profileEditorLayout);
            this._profileEditorGroupBox.Dock = DockStyle.Fill;
            this._profileEditorGroupBox.Location = new Point(0, 0);
            this._profileEditorGroupBox.Name = "_profileEditorGroupBox";
            this._profileEditorGroupBox.Padding = new Padding(8);
            this._profileEditorGroupBox.Size = new Size(356, 418);
            this._profileEditorGroupBox.TabIndex = 0;
            this._profileEditorGroupBox.TabStop = false;
            this._profileEditorGroupBox.Text = "Selected profile editor";
            this._profileEditorLayout.ColumnCount = 2;
            this._profileEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            this._profileEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._profileEditorLayout.Controls.Add(this._profileIdLabel, 0, 0);
            this._profileEditorLayout.Controls.Add(this._profileIdTextBox, 1, 0);
            this._profileEditorLayout.Controls.Add(this._profileTitleLabel, 0, 1);
            this._profileEditorLayout.Controls.Add(this._profileTitleTextBox, 1, 1);
            this._profileEditorLayout.Controls.Add(this._profileEndpointLabel, 0, 2);
            this._profileEditorLayout.Controls.Add(this._profileEndpointTextBox, 1, 2);
            this._profileEditorLayout.Controls.Add(this._profileModelLabel, 0, 3);
            this._profileEditorLayout.Controls.Add(this._profileModelTextBox, 1, 3);
            this._profileEditorLayout.Controls.Add(this._profileContextLabel, 0, 4);
            this._profileEditorLayout.Controls.Add(this._profileContextNumeric, 1, 4);
            this._profileEditorLayout.Controls.Add(this._profileRoleLabel, 0, 5);
            this._profileEditorLayout.Controls.Add(this._profileRoleComboBox, 1, 5);
            this._profileEditorLayout.Dock = DockStyle.Fill;
            this._profileEditorLayout.RowCount = 7;
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            this._profileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._profileIdLabel.Dock = DockStyle.Fill;
            this._profileIdLabel.Text = "Id:";
            this._profileIdLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._profileIdTextBox.Dock = DockStyle.Fill;
            this._profileTitleLabel.Dock = DockStyle.Fill;
            this._profileTitleLabel.Text = "Title:";
            this._profileTitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._profileTitleTextBox.Dock = DockStyle.Fill;
            this._profileEndpointLabel.Dock = DockStyle.Fill;
            this._profileEndpointLabel.Text = "Endpoint:";
            this._profileEndpointLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._profileEndpointTextBox.Dock = DockStyle.Fill;
            this._profileModelLabel.Dock = DockStyle.Fill;
            this._profileModelLabel.Text = "Model:";
            this._profileModelLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._profileModelTextBox.Dock = DockStyle.Fill;
            this._profileContextLabel.Dock = DockStyle.Fill;
            this._profileContextLabel.Text = "ContextWindowTokens:";
            this._profileContextLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._profileContextNumeric.Dock = DockStyle.Left;
            this._profileContextNumeric.Maximum = new decimal(new int[] { 2000000, 0, 0, 0 });
            this._profileContextNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._profileContextNumeric.Size = new Size(120, 23);
            this._profileContextNumeric.Value = new decimal(new int[] { 32768, 0, 0, 0 });
            this._profileRoleLabel.Dock = DockStyle.Fill;
            this._profileRoleLabel.Text = "Role:";
            this._profileRoleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._profileRoleComboBox.Dock = DockStyle.Fill;
            this._profileRoleComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._profileRoleComboBox.Items.AddRange(new object[] { "general", "writer", "planner", "coder", "critic" });
            // 
            // _buttonPanel
            // 
            this._buttonPanel.Controls.Add(this._reloadButton);
            this._buttonPanel.Controls.Add(this._applyProfileButton);
            this._buttonPanel.Controls.Add(this._addProfileButton);
            this._buttonPanel.Controls.Add(this._removeProfileButton);
            this._buttonPanel.Controls.Add(this._saveButton);
            this._buttonPanel.Dock = DockStyle.Fill;
            this._buttonPanel.Location = new Point(15, 567);
            this._buttonPanel.Name = "_buttonPanel";
            this._buttonPanel.Size = new Size(970, 38);
            this._buttonPanel.TabIndex = 2;
            this._reloadButton.Size = new Size(130, 30);
            this._reloadButton.Text = "Reload settings";
            this._reloadButton.UseVisualStyleBackColor = true;
            this._applyProfileButton.Size = new Size(190, 30);
            this._applyProfileButton.Text = "Apply selected profile changes";
            this._applyProfileButton.UseVisualStyleBackColor = true;
            this._addProfileButton.Size = new Size(110, 30);
            this._addProfileButton.Text = "Add profile";
            this._addProfileButton.UseVisualStyleBackColor = true;
            this._removeProfileButton.Size = new Size(120, 30);
            this._removeProfileButton.Text = "Remove profile";
            this._removeProfileButton.UseVisualStyleBackColor = true;
            this._saveButton.Size = new Size(120, 30);
            this._saveButton.Text = "Save settings";
            this._saveButton.UseVisualStyleBackColor = true;
            this._statusLabel.AutoEllipsis = true;
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SettingsPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "SettingsPageControl";
            this.Size = new Size(1000, 650);
            this._rootLayout.ResumeLayout(false);
            this._rootSettingsGroupBox.ResumeLayout(false);
            this._rootSettingsLayout.ResumeLayout(false);
            this._rootSettingsLayout.PerformLayout();
            this._profilesSplitContainer.Panel1.ResumeLayout(false);
            this._profilesSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._profilesSplitContainer)).EndInit();
            this._profilesSplitContainer.ResumeLayout(false);
            this._profilesGroupBox.ResumeLayout(false);
            this._profileEditorGroupBox.ResumeLayout(false);
            this._profileEditorLayout.ResumeLayout(false);
            this._profileEditorLayout.PerformLayout();
            ((ISupportInitialize)(this._profileContextNumeric)).EndInit();
            this._buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
