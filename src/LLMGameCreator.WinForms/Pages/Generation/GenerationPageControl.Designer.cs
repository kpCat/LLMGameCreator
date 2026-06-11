#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GenerationPageControl
    {
        private IContainer components;
        private TableLayoutPanel _rootLayout;
        private TableLayoutPanel _topLayout;
        private TableLayoutPanel _formLayout;
        private TableLayoutPanel _buttonLayout;
        private SplitContainer _outputSplitContainer;
        private Label _profileLabel;
        private Label _profileValueLabel;
        private Label _currentPackageLabel;
        private Label _ideaLabel;
        private TextBox _ideaTextBox;
        private Label _genreLabel;
        private ComboBox _genreComboBox;
        private Label _toneLabel;
        private ComboBox _toneComboBox;
        private Label _cameraLabel;
        private ComboBox _cameraComboBox;
        private Label _settingLabel;
        private ComboBox _settingComboBox;
        private Label _conflictLabel;
        private ComboBox _conflictComboBox;
        private Label _playerRoleLabel;
        private TextBox _playerRoleTextBox;
        private Label _requiredNpcLabel;
        private TextBox _requiredNpcTextBox;
        private Label _firstLocationLabel;
        private TextBox _firstLocationTextBox;
        private Label _mapSizeLabel;
        private FlowLayoutPanel _mapSizePanel;
        private NumericUpDown _mapWidthNumeric;
        private Label _mapSizeSeparatorLabel;
        private NumericUpDown _mapHeightNumeric;
        private Button _testLmStudioButton;
        private Button _generateButton;
        private Button _applyButton;
        private Button _saveButton;
        private Button _validateButton;
        private Label _statusLabel;
        private TextBox _rawJsonTextBox;
        private TextBox _resultTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_currentGamePackageService != null)
                {
                    _currentGamePackageService.CurrentChanged -= CurrentGamePackageService_CurrentChanged;
                }

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._rootLayout = new TableLayoutPanel();
            this._topLayout = new TableLayoutPanel();
            this._profileLabel = new Label();
            this._profileValueLabel = new Label();
            this._currentPackageLabel = new Label();
            this._formLayout = new TableLayoutPanel();
            this._ideaLabel = new Label();
            this._ideaTextBox = new TextBox();
            this._genreLabel = new Label();
            this._genreComboBox = new ComboBox();
            this._toneLabel = new Label();
            this._toneComboBox = new ComboBox();
            this._cameraLabel = new Label();
            this._cameraComboBox = new ComboBox();
            this._settingLabel = new Label();
            this._settingComboBox = new ComboBox();
            this._conflictLabel = new Label();
            this._conflictComboBox = new ComboBox();
            this._playerRoleLabel = new Label();
            this._playerRoleTextBox = new TextBox();
            this._requiredNpcLabel = new Label();
            this._requiredNpcTextBox = new TextBox();
            this._firstLocationLabel = new Label();
            this._firstLocationTextBox = new TextBox();
            this._mapSizeLabel = new Label();
            this._mapSizePanel = new FlowLayoutPanel();
            this._mapWidthNumeric = new NumericUpDown();
            this._mapSizeSeparatorLabel = new Label();
            this._mapHeightNumeric = new NumericUpDown();
            this._buttonLayout = new TableLayoutPanel();
            this._testLmStudioButton = new Button();
            this._generateButton = new Button();
            this._applyButton = new Button();
            this._saveButton = new Button();
            this._validateButton = new Button();
            this._statusLabel = new Label();
            this._outputSplitContainer = new SplitContainer();
            this._rawJsonTextBox = new TextBox();
            this._resultTextBox = new TextBox();
            this._rootLayout.SuspendLayout();
            this._topLayout.SuspendLayout();
            this._formLayout.SuspendLayout();
            this._mapSizePanel.SuspendLayout();
            ((ISupportInitialize)(this._mapWidthNumeric)).BeginInit();
            ((ISupportInitialize)(this._mapHeightNumeric)).BeginInit();
            this._buttonLayout.SuspendLayout();
            ((ISupportInitialize)(this._outputSplitContainer)).BeginInit();
            this._outputSplitContainer.Panel1.SuspendLayout();
            this._outputSplitContainer.Panel2.SuspendLayout();
            this._outputSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootLayout
            // 
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._topLayout, 0, 0);
            this._rootLayout.Controls.Add(this._formLayout, 0, 1);
            this._rootLayout.Controls.Add(this._buttonLayout, 0, 2);
            this._rootLayout.Controls.Add(this._outputSplitContainer, 0, 3);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.Location = new Point(0, 0);
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.Padding = new Padding(8);
            this._rootLayout.RowCount = 4;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 178F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._rootLayout.Size = new Size(1000, 680);
            this._rootLayout.TabIndex = 0;
            // 
            // _topLayout
            // 
            this._topLayout.ColumnCount = 2;
            this._topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            this._topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._topLayout.Controls.Add(this._profileLabel, 0, 0);
            this._topLayout.Controls.Add(this._profileValueLabel, 1, 0);
            this._topLayout.Controls.Add(this._currentPackageLabel, 0, 1);
            this._topLayout.Dock = DockStyle.Fill;
            this._topLayout.Location = new Point(11, 11);
            this._topLayout.Name = "_topLayout";
            this._topLayout.RowCount = 2;
            this._topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._topLayout.Size = new Size(978, 52);
            this._topLayout.TabIndex = 0;
            // 
            // _profileLabel
            // 
            this._profileLabel.AutoSize = true;
            this._profileLabel.Dock = DockStyle.Fill;
            this._profileLabel.Location = new Point(3, 0);
            this._profileLabel.Name = "_profileLabel";
            this._profileLabel.Size = new Size(84, 28);
            this._profileLabel.TabIndex = 0;
            this._profileLabel.Text = "LLM profile:";
            this._profileLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _profileValueLabel
            // 
            this._profileValueLabel.AutoEllipsis = true;
            this._profileValueLabel.Dock = DockStyle.Fill;
            this._profileValueLabel.Location = new Point(93, 0);
            this._profileValueLabel.Name = "_profileValueLabel";
            this._profileValueLabel.Size = new Size(882, 28);
            this._profileValueLabel.TabIndex = 1;
            this._profileValueLabel.Text = "Загрузка...";
            this._profileValueLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _currentPackageLabel
            // 
            this._currentPackageLabel.AutoEllipsis = true;
            this._topLayout.SetColumnSpan(this._currentPackageLabel, 2);
            this._currentPackageLabel.Dock = DockStyle.Fill;
            this._currentPackageLabel.Location = new Point(3, 28);
            this._currentPackageLabel.Name = "_currentPackageLabel";
            this._currentPackageLabel.Size = new Size(972, 28);
            this._currentPackageLabel.TabIndex = 2;
            this._currentPackageLabel.Text = "Проект игры не открыт.";
            this._currentPackageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _formLayout
            // 
            this._formLayout.ColumnCount = 4;
            this._formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            this._formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            this._formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._formLayout.Controls.Add(this._ideaLabel, 0, 0);
            this._formLayout.Controls.Add(this._ideaTextBox, 1, 0);
            this._formLayout.Controls.Add(this._genreLabel, 0, 1);
            this._formLayout.Controls.Add(this._genreComboBox, 1, 1);
            this._formLayout.Controls.Add(this._toneLabel, 2, 1);
            this._formLayout.Controls.Add(this._toneComboBox, 3, 1);
            this._formLayout.Controls.Add(this._cameraLabel, 0, 2);
            this._formLayout.Controls.Add(this._cameraComboBox, 1, 2);
            this._formLayout.Controls.Add(this._settingLabel, 2, 2);
            this._formLayout.Controls.Add(this._settingComboBox, 3, 2);
            this._formLayout.Controls.Add(this._conflictLabel, 0, 3);
            this._formLayout.Controls.Add(this._conflictComboBox, 1, 3);
            this._formLayout.Controls.Add(this._playerRoleLabel, 2, 3);
            this._formLayout.Controls.Add(this._playerRoleTextBox, 3, 3);
            this._formLayout.Controls.Add(this._requiredNpcLabel, 0, 4);
            this._formLayout.Controls.Add(this._requiredNpcTextBox, 1, 4);
            this._formLayout.Controls.Add(this._firstLocationLabel, 2, 4);
            this._formLayout.Controls.Add(this._firstLocationTextBox, 3, 4);
            this._formLayout.Controls.Add(this._mapSizeLabel, 0, 5);
            this._formLayout.Controls.Add(this._mapSizePanel, 1, 5);
            this._formLayout.Dock = DockStyle.Fill;
            this._formLayout.Location = new Point(11, 69);
            this._formLayout.Name = "_formLayout";
            this._formLayout.RowCount = 6;
            this._formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            this._formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            this._formLayout.Size = new Size(978, 172);
            this._formLayout.TabIndex = 1;
            // 
            // _ideaLabel
            // 
            this._ideaLabel.Dock = DockStyle.Fill;
            this._ideaLabel.Location = new Point(3, 0);
            this._ideaLabel.Name = "_ideaLabel";
            this._ideaLabel.Size = new Size(134, 54);
            this._ideaLabel.TabIndex = 0;
            this._ideaLabel.Text = "Идея:";
            this._ideaLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _ideaTextBox
            // 
            this._formLayout.SetColumnSpan(this._ideaTextBox, 3);
            this._ideaTextBox.Dock = DockStyle.Fill;
            this._ideaTextBox.Location = new Point(143, 3);
            this._ideaTextBox.Multiline = true;
            this._ideaTextBox.Name = "_ideaTextBox";
            this._ideaTextBox.ScrollBars = ScrollBars.Vertical;
            this._ideaTextBox.Size = new Size(832, 48);
            this._ideaTextBox.TabIndex = 1;
            this._ideaTextBox.Text = "Небольшое приключение с понятной первой задачей и исследуемой стартовой локацией.";
            // 
            // combo labels and inputs
            // 
            this._genreLabel.Dock = DockStyle.Fill;
            this._genreLabel.Location = new Point(3, 54);
            this._genreLabel.Name = "_genreLabel";
            this._genreLabel.Size = new Size(134, 28);
            this._genreLabel.TabIndex = 2;
            this._genreLabel.Text = "Жанр:";
            this._genreLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._genreComboBox.Dock = DockStyle.Fill;
            this._genreComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._genreComboBox.Items.AddRange(new object[] { "adventure", "rpg", "mystery", "survival", "cozy exploration" });
            this._genreComboBox.Location = new Point(143, 57);
            this._genreComboBox.Name = "_genreComboBox";
            this._genreComboBox.Size = new Size(343, 23);
            this._genreComboBox.TabIndex = 3;
            this._genreComboBox.SelectedIndex = 0;
            this._toneLabel.Dock = DockStyle.Fill;
            this._toneLabel.Location = new Point(492, 54);
            this._toneLabel.Name = "_toneLabel";
            this._toneLabel.Size = new Size(134, 28);
            this._toneLabel.TabIndex = 4;
            this._toneLabel.Text = "Тон:";
            this._toneLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._toneComboBox.Dock = DockStyle.Fill;
            this._toneComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._toneComboBox.Items.AddRange(new object[] { "grounded", "hopeful", "dark fairy tale", "light humor", "tense" });
            this._toneComboBox.Location = new Point(632, 57);
            this._toneComboBox.Name = "_toneComboBox";
            this._toneComboBox.Size = new Size(343, 23);
            this._toneComboBox.TabIndex = 5;
            this._toneComboBox.SelectedIndex = 1;
            this._cameraLabel.Dock = DockStyle.Fill;
            this._cameraLabel.Location = new Point(3, 82);
            this._cameraLabel.Name = "_cameraLabel";
            this._cameraLabel.Size = new Size(134, 28);
            this._cameraLabel.TabIndex = 6;
            this._cameraLabel.Text = "Камера:";
            this._cameraLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._cameraComboBox.Dock = DockStyle.Fill;
            this._cameraComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cameraComboBox.Items.AddRange(new object[] { "top-down grid", "classic tile map", "2d exploration" });
            this._cameraComboBox.Location = new Point(143, 85);
            this._cameraComboBox.Name = "_cameraComboBox";
            this._cameraComboBox.Size = new Size(343, 23);
            this._cameraComboBox.TabIndex = 7;
            this._cameraComboBox.SelectedIndex = 0;
            this._settingLabel.Dock = DockStyle.Fill;
            this._settingLabel.Location = new Point(492, 82);
            this._settingLabel.Name = "_settingLabel";
            this._settingLabel.Size = new Size(134, 28);
            this._settingLabel.TabIndex = 8;
            this._settingLabel.Text = "Сеттинг:";
            this._settingLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._settingComboBox.Dock = DockStyle.Fill;
            this._settingComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._settingComboBox.Items.AddRange(new object[] { "border village", "abandoned station", "forest shrine", "snow town", "desert outpost" });
            this._settingComboBox.Location = new Point(632, 85);
            this._settingComboBox.Name = "_settingComboBox";
            this._settingComboBox.Size = new Size(343, 23);
            this._settingComboBox.TabIndex = 9;
            this._settingComboBox.Text = "border village";
            this._conflictLabel.Dock = DockStyle.Fill;
            this._conflictLabel.Location = new Point(3, 110);
            this._conflictLabel.Name = "_conflictLabel";
            this._conflictLabel.Size = new Size(134, 28);
            this._conflictLabel.TabIndex = 10;
            this._conflictLabel.Text = "Первый конфликт:";
            this._conflictLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._conflictComboBox.Dock = DockStyle.Fill;
            this._conflictComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._conflictComboBox.Items.AddRange(new object[] { "blocked road", "missing scout", "broken bridge", "strange signal", "closed gate" });
            this._conflictComboBox.Location = new Point(143, 113);
            this._conflictComboBox.Name = "_conflictComboBox";
            this._conflictComboBox.Size = new Size(343, 23);
            this._conflictComboBox.TabIndex = 11;
            this._conflictComboBox.Text = "blocked road";
            this._playerRoleLabel.Dock = DockStyle.Fill;
            this._playerRoleLabel.Location = new Point(492, 110);
            this._playerRoleLabel.Name = "_playerRoleLabel";
            this._playerRoleLabel.Size = new Size(134, 28);
            this._playerRoleLabel.TabIndex = 12;
            this._playerRoleLabel.Text = "Роль игрока:";
            this._playerRoleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._playerRoleTextBox.Dock = DockStyle.Fill;
            this._playerRoleTextBox.Location = new Point(632, 113);
            this._playerRoleTextBox.Name = "_playerRoleTextBox";
            this._playerRoleTextBox.Size = new Size(343, 23);
            this._playerRoleTextBox.TabIndex = 13;
            this._playerRoleTextBox.Text = "новый смотритель дороги";
            this._requiredNpcLabel.Dock = DockStyle.Fill;
            this._requiredNpcLabel.Location = new Point(3, 138);
            this._requiredNpcLabel.Name = "_requiredNpcLabel";
            this._requiredNpcLabel.Size = new Size(134, 28);
            this._requiredNpcLabel.TabIndex = 14;
            this._requiredNpcLabel.Text = "Важный NPC:";
            this._requiredNpcLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._requiredNpcTextBox.Dock = DockStyle.Fill;
            this._requiredNpcTextBox.Location = new Point(143, 141);
            this._requiredNpcTextBox.Name = "_requiredNpcTextBox";
            this._requiredNpcTextBox.Size = new Size(343, 23);
            this._requiredNpcTextBox.TabIndex = 15;
            this._requiredNpcTextBox.Text = "староста";
            this._firstLocationLabel.Dock = DockStyle.Fill;
            this._firstLocationLabel.Location = new Point(492, 138);
            this._firstLocationLabel.Name = "_firstLocationLabel";
            this._firstLocationLabel.Size = new Size(134, 28);
            this._firstLocationLabel.TabIndex = 16;
            this._firstLocationLabel.Text = "Первая локация:";
            this._firstLocationLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._firstLocationTextBox.Dock = DockStyle.Fill;
            this._firstLocationTextBox.Location = new Point(632, 141);
            this._firstLocationTextBox.Name = "_firstLocationTextBox";
            this._firstLocationTextBox.Size = new Size(343, 23);
            this._firstLocationTextBox.TabIndex = 17;
            this._firstLocationTextBox.Text = "стартовая деревня";
            this._mapSizeLabel.Dock = DockStyle.Fill;
            this._mapSizeLabel.Location = new Point(3, 166);
            this._mapSizeLabel.Name = "_mapSizeLabel";
            this._mapSizeLabel.Size = new Size(134, 28);
            this._mapSizeLabel.TabIndex = 18;
            this._mapSizeLabel.Text = "Размер карты:";
            this._mapSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._mapSizePanel.Controls.Add(this._mapWidthNumeric);
            this._mapSizePanel.Controls.Add(this._mapSizeSeparatorLabel);
            this._mapSizePanel.Controls.Add(this._mapHeightNumeric);
            this._mapSizePanel.Dock = DockStyle.Fill;
            this._mapSizePanel.Location = new Point(143, 169);
            this._mapSizePanel.Name = "_mapSizePanel";
            this._mapSizePanel.Size = new Size(343, 22);
            this._mapSizePanel.TabIndex = 19;
            this._mapWidthNumeric.Location = new Point(3, 3);
            this._mapWidthNumeric.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            this._mapWidthNumeric.Minimum = new decimal(new int[] { 12, 0, 0, 0 });
            this._mapWidthNumeric.Name = "_mapWidthNumeric";
            this._mapWidthNumeric.Size = new Size(70, 23);
            this._mapWidthNumeric.TabIndex = 0;
            this._mapWidthNumeric.Value = new decimal(new int[] { 24, 0, 0, 0 });
            this._mapSizeSeparatorLabel.AutoSize = true;
            this._mapSizeSeparatorLabel.Location = new Point(79, 0);
            this._mapSizeSeparatorLabel.Name = "_mapSizeSeparatorLabel";
            this._mapSizeSeparatorLabel.Size = new Size(12, 15);
            this._mapSizeSeparatorLabel.TabIndex = 1;
            this._mapSizeSeparatorLabel.Text = "x";
            this._mapHeightNumeric.Location = new Point(97, 3);
            this._mapHeightNumeric.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            this._mapHeightNumeric.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            this._mapHeightNumeric.Name = "_mapHeightNumeric";
            this._mapHeightNumeric.Size = new Size(70, 23);
            this._mapHeightNumeric.TabIndex = 2;
            this._mapHeightNumeric.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // _buttonLayout
            // 
            this._buttonLayout.ColumnCount = 6;
            this._buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            this._buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            this._buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            this._buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            this._buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            this._buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._buttonLayout.Controls.Add(this._testLmStudioButton, 0, 0);
            this._buttonLayout.Controls.Add(this._generateButton, 1, 0);
            this._buttonLayout.Controls.Add(this._applyButton, 2, 0);
            this._buttonLayout.Controls.Add(this._saveButton, 3, 0);
            this._buttonLayout.Controls.Add(this._validateButton, 4, 0);
            this._buttonLayout.Controls.Add(this._statusLabel, 5, 0);
            this._buttonLayout.Dock = DockStyle.Fill;
            this._buttonLayout.Location = new Point(11, 247);
            this._buttonLayout.Name = "_buttonLayout";
            this._buttonLayout.RowCount = 1;
            this._buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._buttonLayout.Size = new Size(978, 34);
            this._buttonLayout.TabIndex = 2;
            this._testLmStudioButton.Dock = DockStyle.Fill;
            this._testLmStudioButton.Location = new Point(3, 3);
            this._testLmStudioButton.Name = "_testLmStudioButton";
            this._testLmStudioButton.Size = new Size(124, 28);
            this._testLmStudioButton.TabIndex = 0;
            this._testLmStudioButton.Text = "Тест LM Studio";
            this._testLmStudioButton.UseVisualStyleBackColor = true;
            this._generateButton.Dock = DockStyle.Fill;
            this._generateButton.Location = new Point(133, 3);
            this._generateButton.Name = "_generateButton";
            this._generateButton.Size = new Size(214, 28);
            this._generateButton.TabIndex = 1;
            this._generateButton.Text = "Сгенерировать playable slice";
            this._generateButton.UseVisualStyleBackColor = true;
            this._applyButton.Dock = DockStyle.Fill;
            this._applyButton.Enabled = false;
            this._applyButton.Location = new Point(353, 3);
            this._applyButton.Name = "_applyButton";
            this._applyButton.Size = new Size(154, 28);
            this._applyButton.TabIndex = 2;
            this._applyButton.Text = "Применить к package";
            this._applyButton.UseVisualStyleBackColor = true;
            this._saveButton.Dock = DockStyle.Fill;
            this._saveButton.Location = new Point(513, 3);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new Size(144, 28);
            this._saveButton.TabIndex = 3;
            this._saveButton.Text = "Сохранить package";
            this._saveButton.UseVisualStyleBackColor = true;
            this._validateButton.Dock = DockStyle.Fill;
            this._validateButton.Location = new Point(663, 3);
            this._validateButton.Name = "_validateButton";
            this._validateButton.Size = new Size(144, 28);
            this._validateButton.TabIndex = 4;
            this._validateButton.Text = "Проверить package";
            this._validateButton.UseVisualStyleBackColor = true;
            this._statusLabel.AutoEllipsis = true;
            this._statusLabel.Dock = DockStyle.Fill;
            this._statusLabel.Location = new Point(813, 0);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new Size(162, 34);
            this._statusLabel.TabIndex = 5;
            this._statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _outputSplitContainer
            // 
            this._outputSplitContainer.Dock = DockStyle.Fill;
            this._outputSplitContainer.Location = new Point(11, 287);
            this._outputSplitContainer.Name = "_outputSplitContainer";
            this._outputSplitContainer.Panel1.Controls.Add(this._rawJsonTextBox);
            this._outputSplitContainer.Panel2.Controls.Add(this._resultTextBox);
            this._outputSplitContainer.Size = new Size(978, 382);
            this._outputSplitContainer.SplitterDistance = 489;
            this._outputSplitContainer.TabIndex = 3;
            this._rawJsonTextBox.Dock = DockStyle.Fill;
            this._rawJsonTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this._rawJsonTextBox.Location = new Point(0, 0);
            this._rawJsonTextBox.Multiline = true;
            this._rawJsonTextBox.Name = "_rawJsonTextBox";
            this._rawJsonTextBox.ReadOnly = true;
            this._rawJsonTextBox.ScrollBars = ScrollBars.Both;
            this._rawJsonTextBox.Size = new Size(489, 382);
            this._rawJsonTextBox.TabIndex = 0;
            this._rawJsonTextBox.WordWrap = false;
            this._resultTextBox.Dock = DockStyle.Fill;
            this._resultTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this._resultTextBox.Location = new Point(0, 0);
            this._resultTextBox.Multiline = true;
            this._resultTextBox.Name = "_resultTextBox";
            this._resultTextBox.ReadOnly = true;
            this._resultTextBox.ScrollBars = ScrollBars.Both;
            this._resultTextBox.Size = new Size(485, 382);
            this._resultTextBox.TabIndex = 0;
            this._resultTextBox.WordWrap = false;
            // 
            // GenerationPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "GenerationPageControl";
            this.Size = new Size(1000, 680);
            this._rootLayout.ResumeLayout(false);
            this._topLayout.ResumeLayout(false);
            this._topLayout.PerformLayout();
            this._formLayout.ResumeLayout(false);
            this._formLayout.PerformLayout();
            this._mapSizePanel.ResumeLayout(false);
            this._mapSizePanel.PerformLayout();
            ((ISupportInitialize)(this._mapWidthNumeric)).EndInit();
            ((ISupportInitialize)(this._mapHeightNumeric)).EndInit();
            this._buttonLayout.ResumeLayout(false);
            this._outputSplitContainer.Panel1.ResumeLayout(false);
            this._outputSplitContainer.Panel1.PerformLayout();
            this._outputSplitContainer.Panel2.ResumeLayout(false);
            this._outputSplitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)(this._outputSplitContainer)).EndInit();
            this._outputSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
