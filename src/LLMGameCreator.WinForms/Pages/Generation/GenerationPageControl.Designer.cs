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
        private TabControl _briefTabs;
        private TabPage _coreTabPage;
        private TabPage _advancedTabPage;
        private TabPage _scopeTabPage;
        private TableLayoutPanel _coreLayout;
        private TableLayoutPanel _advancedLayout;
        private TableLayoutPanel _scopeLayout;
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
        private Label _loreLabel;
        private TextBox _loreTextBox;
        private Label _hardConstraintsLabel;
        private TextBox _hardConstraintsTextBox;
        private Label _mustIncludeLabel;
        private TextBox _mustIncludeTextBox;
        private Label _mustAvoidLabel;
        private TextBox _mustAvoidTextBox;
        private Label _playerFantasyLabel;
        private TextBox _playerFantasyTextBox;
        private Label _gameplayLogicLabel;
        private TextBox _gameplayLogicTextBox;
        private Label _maxTileOverridesLabel;
        private NumericUpDown _maxTileOverridesNumeric;
        private Label _targetNpcLabel;
        private NumericUpDown _targetNpcNumeric;
        private Label _targetEntityLabel;
        private NumericUpDown _targetEntityNumeric;
        private Label _targetQuestLabel;
        private NumericUpDown _targetQuestNumeric;
        private Label _targetDialogueLabel;
        private NumericUpDown _targetDialogueNumeric;
        private Label _detailModeLabel;
        private ComboBox _detailModeComboBox;
        private Label _logicModeLabel;
        private ComboBox _logicModeComboBox;
        private Button _testLmStudioButton;
        private Button _aiHelperButton;
        private Button _generateButton;
        private Button _applyButton;
        private Button _saveButton;
        private Button _validateButton;
        private Button _cancelButton;
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
            _rootLayout = new TableLayoutPanel();
            _topLayout = new TableLayoutPanel();
            _profileLabel = new Label();
            _profileValueLabel = new Label();
            _currentPackageLabel = new Label();
            _briefTabs = new TabControl();
            _coreTabPage = new TabPage();
            _coreLayout = new TableLayoutPanel();
            _ideaLabel = new Label();
            _ideaTextBox = new TextBox();
            _genreLabel = new Label();
            _genreComboBox = new ComboBox();
            _toneLabel = new Label();
            _toneComboBox = new ComboBox();
            _cameraLabel = new Label();
            _cameraComboBox = new ComboBox();
            _settingLabel = new Label();
            _settingComboBox = new ComboBox();
            _conflictLabel = new Label();
            _conflictComboBox = new ComboBox();
            _playerRoleLabel = new Label();
            _playerRoleTextBox = new TextBox();
            _requiredNpcLabel = new Label();
            _requiredNpcTextBox = new TextBox();
            _firstLocationLabel = new Label();
            _firstLocationTextBox = new TextBox();
            _mapSizeLabel = new Label();
            _mapSizePanel = new FlowLayoutPanel();
            _mapWidthNumeric = new NumericUpDown();
            _mapSizeSeparatorLabel = new Label();
            _mapHeightNumeric = new NumericUpDown();
            _advancedTabPage = new TabPage();
            _advancedLayout = new TableLayoutPanel();
            _loreLabel = new Label();
            _loreTextBox = new TextBox();
            _hardConstraintsLabel = new Label();
            _hardConstraintsTextBox = new TextBox();
            _mustIncludeLabel = new Label();
            _mustIncludeTextBox = new TextBox();
            _mustAvoidLabel = new Label();
            _mustAvoidTextBox = new TextBox();
            _playerFantasyLabel = new Label();
            _playerFantasyTextBox = new TextBox();
            _gameplayLogicLabel = new Label();
            _gameplayLogicTextBox = new TextBox();
            _scopeTabPage = new TabPage();
            _scopeLayout = new TableLayoutPanel();
            _maxTileOverridesLabel = new Label();
            _maxTileOverridesNumeric = new NumericUpDown();
            _targetNpcLabel = new Label();
            _targetNpcNumeric = new NumericUpDown();
            _targetEntityLabel = new Label();
            _targetEntityNumeric = new NumericUpDown();
            _targetQuestLabel = new Label();
            _targetQuestNumeric = new NumericUpDown();
            _targetDialogueLabel = new Label();
            _targetDialogueNumeric = new NumericUpDown();
            _detailModeLabel = new Label();
            _detailModeComboBox = new ComboBox();
            _logicModeLabel = new Label();
            _logicModeComboBox = new ComboBox();
            _buttonLayout = new TableLayoutPanel();
            _testLmStudioButton = new Button();
            _aiHelperButton = new Button();
            _generateButton = new Button();
            _applyButton = new Button();
            _saveButton = new Button();
            _validateButton = new Button();
            _cancelButton = new Button();
            _statusLabel = new Label();
            _outputSplitContainer = new SplitContainer();
            _rawJsonTextBox = new TextBox();
            _resultTextBox = new TextBox();
            _rootLayout.SuspendLayout();
            _topLayout.SuspendLayout();
            _briefTabs.SuspendLayout();
            _coreTabPage.SuspendLayout();
            _coreLayout.SuspendLayout();
            _mapSizePanel.SuspendLayout();
            ((ISupportInitialize)_mapWidthNumeric).BeginInit();
            ((ISupportInitialize)_mapHeightNumeric).BeginInit();
            _advancedTabPage.SuspendLayout();
            _advancedLayout.SuspendLayout();
            _scopeTabPage.SuspendLayout();
            _scopeLayout.SuspendLayout();
            ((ISupportInitialize)_maxTileOverridesNumeric).BeginInit();
            ((ISupportInitialize)_targetNpcNumeric).BeginInit();
            ((ISupportInitialize)_targetEntityNumeric).BeginInit();
            ((ISupportInitialize)_targetQuestNumeric).BeginInit();
            ((ISupportInitialize)_targetDialogueNumeric).BeginInit();
            _buttonLayout.SuspendLayout();
            ((ISupportInitialize)_outputSplitContainer).BeginInit();
            _outputSplitContainer.Panel1.SuspendLayout();
            _outputSplitContainer.Panel2.SuspendLayout();
            _outputSplitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // _rootLayout
            // 
            _rootLayout.ColumnCount = 1;
            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _rootLayout.Controls.Add(_topLayout, 0, 0);
            _rootLayout.Controls.Add(_briefTabs, 0, 1);
            _rootLayout.Controls.Add(_buttonLayout, 0, 2);
            _rootLayout.Controls.Add(_outputSplitContainer, 0, 3);
            _rootLayout.Dock = DockStyle.Fill;
            _rootLayout.Location = new Point(0, 0);
            _rootLayout.Name = "_rootLayout";
            _rootLayout.Padding = new Padding(8);
            _rootLayout.RowCount = 4;
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 268F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _rootLayout.Size = new Size(1100, 760);
            _rootLayout.TabIndex = 0;
            // 
            // _topLayout
            // 
            _topLayout.ColumnCount = 2;
            _topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            _topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _topLayout.Controls.Add(_profileLabel, 0, 0);
            _topLayout.Controls.Add(_profileValueLabel, 1, 0);
            _topLayout.Controls.Add(_currentPackageLabel, 0, 1);
            _topLayout.Dock = DockStyle.Fill;
            _topLayout.Location = new Point(11, 11);
            _topLayout.Name = "_topLayout";
            _topLayout.RowCount = 2;
            _topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            _topLayout.Size = new Size(1078, 52);
            _topLayout.TabIndex = 0;
            // 
            // _profileLabel
            // 
            _profileLabel.Dock = DockStyle.Fill;
            _profileLabel.Location = new Point(3, 0);
            _profileLabel.Name = "_profileLabel";
            _profileLabel.Size = new Size(84, 28);
            _profileLabel.TabIndex = 0;
            _profileLabel.Text = "LLM profile:";
            _profileLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _profileValueLabel
            // 
            _profileValueLabel.AutoEllipsis = true;
            _profileValueLabel.Dock = DockStyle.Fill;
            _profileValueLabel.Location = new Point(93, 0);
            _profileValueLabel.Name = "_profileValueLabel";
            _profileValueLabel.Size = new Size(982, 28);
            _profileValueLabel.TabIndex = 1;
            _profileValueLabel.Text = "Загрузка...";
            _profileValueLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _currentPackageLabel
            // 
            _currentPackageLabel.AutoEllipsis = true;
            _topLayout.SetColumnSpan(_currentPackageLabel, 2);
            _currentPackageLabel.Dock = DockStyle.Fill;
            _currentPackageLabel.Location = new Point(3, 28);
            _currentPackageLabel.Name = "_currentPackageLabel";
            _currentPackageLabel.Size = new Size(1072, 28);
            _currentPackageLabel.TabIndex = 2;
            _currentPackageLabel.Text = "Проект игры не открыт.";
            _currentPackageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _briefTabs
            // 
            _briefTabs.Controls.Add(_coreTabPage);
            _briefTabs.Controls.Add(_advancedTabPage);
            _briefTabs.Controls.Add(_scopeTabPage);
            _briefTabs.Dock = DockStyle.Fill;
            _briefTabs.Location = new Point(11, 69);
            _briefTabs.Name = "_briefTabs";
            _briefTabs.SelectedIndex = 0;
            _briefTabs.Size = new Size(1078, 262);
            _briefTabs.TabIndex = 1;
            // 
            // _coreTabPage
            // 
            _coreTabPage.Controls.Add(_coreLayout);
            _coreTabPage.Location = new Point(4, 24);
            _coreTabPage.Name = "_coreTabPage";
            _coreTabPage.Padding = new Padding(8);
            _coreTabPage.Size = new Size(1070, 234);
            _coreTabPage.TabIndex = 0;
            _coreTabPage.Text = "Основные поля";
            _coreTabPage.UseVisualStyleBackColor = true;
            // 
            // _coreLayout
            // 
            _coreLayout.ColumnCount = 4;
            _coreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
            _coreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _coreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
            _coreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _coreLayout.Controls.Add(_ideaLabel, 0, 0);
            _coreLayout.Controls.Add(_ideaTextBox, 1, 0);
            _coreLayout.Controls.Add(_genreLabel, 0, 1);
            _coreLayout.Controls.Add(_genreComboBox, 1, 1);
            _coreLayout.Controls.Add(_toneLabel, 2, 1);
            _coreLayout.Controls.Add(_toneComboBox, 3, 1);
            _coreLayout.Controls.Add(_cameraLabel, 0, 2);
            _coreLayout.Controls.Add(_cameraComboBox, 1, 2);
            _coreLayout.Controls.Add(_settingLabel, 2, 2);
            _coreLayout.Controls.Add(_settingComboBox, 3, 2);
            _coreLayout.Controls.Add(_conflictLabel, 0, 3);
            _coreLayout.Controls.Add(_conflictComboBox, 1, 3);
            _coreLayout.Controls.Add(_playerRoleLabel, 2, 3);
            _coreLayout.Controls.Add(_playerRoleTextBox, 3, 3);
            _coreLayout.Controls.Add(_requiredNpcLabel, 0, 4);
            _coreLayout.Controls.Add(_requiredNpcTextBox, 1, 4);
            _coreLayout.Controls.Add(_firstLocationLabel, 2, 4);
            _coreLayout.Controls.Add(_firstLocationTextBox, 3, 4);
            _coreLayout.Controls.Add(_mapSizeLabel, 0, 5);
            _coreLayout.Controls.Add(_mapSizePanel, 1, 5);
            _coreLayout.Dock = DockStyle.Fill;
            _coreLayout.Location = new Point(8, 8);
            _coreLayout.Name = "_coreLayout";
            _coreLayout.RowCount = 6;
            _coreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            _coreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            _coreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            _coreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            _coreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            _coreLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            _coreLayout.Size = new Size(1054, 218);
            _coreLayout.TabIndex = 0;
            // 
            // _ideaLabel
            // 
            _ideaLabel.Dock = DockStyle.Fill;
            _ideaLabel.Location = new Point(3, 0);
            _ideaLabel.Name = "_ideaLabel";
            _ideaLabel.Size = new Size(204, 62);
            _ideaLabel.TabIndex = 0;
            _ideaLabel.Text = "Идея:";
            _ideaLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _ideaTextBox
            // 
            _coreLayout.SetColumnSpan(_ideaTextBox, 3);
            _ideaTextBox.Dock = DockStyle.Fill;
            _ideaTextBox.Location = new Point(213, 3);
            _ideaTextBox.Multiline = true;
            _ideaTextBox.Name = "_ideaTextBox";
            _ideaTextBox.ScrollBars = ScrollBars.Vertical;
            _ideaTextBox.Size = new Size(838, 56);
            _ideaTextBox.TabIndex = 1;
            _ideaTextBox.Text = "Небольшое приключение с понятной первой задачей и исследуемой стартовой локацией.";
            // 
            // _genreLabel
            // 
            _genreLabel.Dock = DockStyle.Fill;
            _genreLabel.Location = new Point(3, 62);
            _genreLabel.Name = "_genreLabel";
            _genreLabel.Size = new Size(204, 32);
            _genreLabel.TabIndex = 2;
            _genreLabel.Text = "Жанр: механический жанр / gameplay expectation";
            _genreLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _genreComboBox
            // 
            _genreComboBox.Dock = DockStyle.Fill;
            _genreComboBox.Items.AddRange(new object[] { "rpg", "adventure", "survival", "visual novel", "tactical rpg", "exploration", "mystery", "horror", "sandbox" });
            _genreComboBox.Location = new Point(213, 65);
            _genreComboBox.Name = "_genreComboBox";
            _genreComboBox.Size = new Size(311, 23);
            _genreComboBox.TabIndex = 3;
            _genreComboBox.Text = "rpg";
            // 
            // _toneLabel
            // 
            _toneLabel.Dock = DockStyle.Fill;
            _toneLabel.Location = new Point(530, 62);
            _toneLabel.Name = "_toneLabel";
            _toneLabel.Size = new Size(204, 32);
            _toneLabel.TabIndex = 4;
            _toneLabel.Text = "Тон: эмоциональный стиль";
            _toneLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _toneComboBox
            // 
            _toneComboBox.Dock = DockStyle.Fill;
            _toneComboBox.Items.AddRange(new object[] { "dark fantasy", "grimdark", "dark fairy tale", "mystery", "hopeful", "tense", "cozy", "comedic", "horror", "melancholic" });
            _toneComboBox.Location = new Point(740, 65);
            _toneComboBox.Name = "_toneComboBox";
            _toneComboBox.Size = new Size(311, 23);
            _toneComboBox.TabIndex = 5;
            _toneComboBox.Text = "dark fantasy";
            // 
            // _cameraLabel
            // 
            _cameraLabel.Dock = DockStyle.Fill;
            _cameraLabel.Location = new Point(3, 94);
            _cameraLabel.Name = "_cameraLabel";
            _cameraLabel.Size = new Size(204, 32);
            _cameraLabel.TabIndex = 6;
            _cameraLabel.Text = "Камера: как смотреть на игру";
            _cameraLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cameraComboBox
            // 
            _cameraComboBox.Dock = DockStyle.Fill;
            _cameraComboBox.Items.AddRange(new object[] { "top-down grid", "side-view", "visual novel", "first person text", "tactical map", "isometric-like grid" });
            _cameraComboBox.Location = new Point(213, 97);
            _cameraComboBox.Name = "_cameraComboBox";
            _cameraComboBox.Size = new Size(311, 23);
            _cameraComboBox.TabIndex = 7;
            _cameraComboBox.Text = "top-down grid";
            // 
            // _settingLabel
            // 
            _settingLabel.Dock = DockStyle.Fill;
            _settingLabel.Location = new Point(530, 94);
            _settingLabel.Name = "_settingLabel";
            _settingLabel.Size = new Size(204, 32);
            _settingLabel.TabIndex = 8;
            _settingLabel.Text = "Сеттинг: тип мира/локации";
            _settingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _settingComboBox
            // 
            _settingComboBox.Dock = DockStyle.Fill;
            _settingComboBox.Items.AddRange(new object[] { "cursed village", "post-apocalyptic town", "fantasy city", "dungeon", "forest frontier", "cyberpunk district", "space station", "prison camp", "haunted manor" });
            _settingComboBox.Location = new Point(740, 97);
            _settingComboBox.Name = "_settingComboBox";
            _settingComboBox.Size = new Size(311, 23);
            _settingComboBox.TabIndex = 9;
            _settingComboBox.Text = "cursed village";
            // 
            // _conflictLabel
            // 
            _conflictLabel.Dock = DockStyle.Fill;
            _conflictLabel.Location = new Point(3, 126);
            _conflictLabel.Name = "_conflictLabel";
            _conflictLabel.Size = new Size(204, 32);
            _conflictLabel.TabIndex = 10;
            _conflictLabel.Text = "Первый конфликт: первая проблема playable slice";
            _conflictLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _conflictComboBox
            // 
            _conflictComboBox.Dock = DockStyle.Fill;
            _conflictComboBox.Items.AddRange(new object[] { "blocked road", "missing person", "cursed object", "monster near village", "locked gate", "poisoned well", "forbidden ruins", "stolen relic", "broken bridge", "suspicious stranger" });
            _conflictComboBox.Location = new Point(213, 129);
            _conflictComboBox.Name = "_conflictComboBox";
            _conflictComboBox.Size = new Size(311, 23);
            _conflictComboBox.TabIndex = 11;
            _conflictComboBox.Text = "blocked road";
            // 
            // _playerRoleLabel
            // 
            _playerRoleLabel.Dock = DockStyle.Fill;
            _playerRoleLabel.Location = new Point(530, 126);
            _playerRoleLabel.Name = "_playerRoleLabel";
            _playerRoleLabel.Size = new Size(204, 32);
            _playerRoleLabel.TabIndex = 12;
            _playerRoleLabel.Text = "Роль игрока:";
            _playerRoleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _playerRoleTextBox
            // 
            _playerRoleTextBox.Dock = DockStyle.Fill;
            _playerRoleTextBox.Location = new Point(740, 129);
            _playerRoleTextBox.Name = "_playerRoleTextBox";
            _playerRoleTextBox.Size = new Size(311, 23);
            _playerRoleTextBox.TabIndex = 13;
            _playerRoleTextBox.Text = "новый смотритель дороги";
            // 
            // _requiredNpcLabel
            // 
            _requiredNpcLabel.Dock = DockStyle.Fill;
            _requiredNpcLabel.Location = new Point(3, 158);
            _requiredNpcLabel.Name = "_requiredNpcLabel";
            _requiredNpcLabel.Size = new Size(204, 32);
            _requiredNpcLabel.TabIndex = 14;
            _requiredNpcLabel.Text = "NPC: обязательно добавить как prototype/entity/speaker";
            _requiredNpcLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _requiredNpcTextBox
            // 
            _requiredNpcTextBox.Dock = DockStyle.Fill;
            _requiredNpcTextBox.Location = new Point(213, 161);
            _requiredNpcTextBox.Name = "_requiredNpcTextBox";
            _requiredNpcTextBox.Size = new Size(311, 23);
            _requiredNpcTextBox.TabIndex = 15;
            _requiredNpcTextBox.Text = "староста";
            // 
            // _firstLocationLabel
            // 
            _firstLocationLabel.Dock = DockStyle.Fill;
            _firstLocationLabel.Location = new Point(530, 158);
            _firstLocationLabel.Name = "_firstLocationLabel";
            _firstLocationLabel.Size = new Size(204, 32);
            _firstLocationLabel.TabIndex = 16;
            _firstLocationLabel.Text = "Первая локация: где стартует первая карта";
            _firstLocationLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _firstLocationTextBox
            // 
            _firstLocationTextBox.Dock = DockStyle.Fill;
            _firstLocationTextBox.Location = new Point(740, 161);
            _firstLocationTextBox.Name = "_firstLocationTextBox";
            _firstLocationTextBox.Size = new Size(311, 23);
            _firstLocationTextBox.TabIndex = 17;
            _firstLocationTextBox.Text = "стартовая деревня";
            // 
            // _mapSizeLabel
            // 
            _mapSizeLabel.Dock = DockStyle.Fill;
            _mapSizeLabel.Location = new Point(3, 190);
            _mapSizeLabel.Name = "_mapSizeLabel";
            _mapSizeLabel.Size = new Size(204, 32);
            _mapSizeLabel.TabIndex = 18;
            _mapSizeLabel.Text = "Размер карты:";
            _mapSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _mapSizePanel
            // 
            _mapSizePanel.Controls.Add(_mapWidthNumeric);
            _mapSizePanel.Controls.Add(_mapSizeSeparatorLabel);
            _mapSizePanel.Controls.Add(_mapHeightNumeric);
            _mapSizePanel.Dock = DockStyle.Fill;
            _mapSizePanel.Location = new Point(213, 193);
            _mapSizePanel.Name = "_mapSizePanel";
            _mapSizePanel.Size = new Size(311, 26);
            _mapSizePanel.TabIndex = 19;
            _mapSizePanel.WrapContents = false;
            // 
            // _mapWidthNumeric
            // 
            _mapWidthNumeric.Location = new Point(3, 3);
            _mapWidthNumeric.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            _mapWidthNumeric.Minimum = new decimal(new int[] { 12, 0, 0, 0 });
            _mapWidthNumeric.Name = "_mapWidthNumeric";
            _mapWidthNumeric.Size = new Size(70, 23);
            _mapWidthNumeric.TabIndex = 0;
            _mapWidthNumeric.Value = new decimal(new int[] { 24, 0, 0, 0 });
            // 
            // _mapSizeSeparatorLabel
            // 
            _mapSizeSeparatorLabel.AutoSize = true;
            _mapSizeSeparatorLabel.Location = new Point(79, 0);
            _mapSizeSeparatorLabel.Name = "_mapSizeSeparatorLabel";
            _mapSizeSeparatorLabel.Size = new Size(13, 15);
            _mapSizeSeparatorLabel.TabIndex = 1;
            _mapSizeSeparatorLabel.Text = "x";
            _mapSizeSeparatorLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _mapHeightNumeric
            // 
            _mapHeightNumeric.Location = new Point(98, 3);
            _mapHeightNumeric.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            _mapHeightNumeric.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            _mapHeightNumeric.Name = "_mapHeightNumeric";
            _mapHeightNumeric.Size = new Size(70, 23);
            _mapHeightNumeric.TabIndex = 2;
            _mapHeightNumeric.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // _advancedTabPage
            // 
            _advancedTabPage.Controls.Add(_advancedLayout);
            _advancedTabPage.Location = new Point(4, 24);
            _advancedTabPage.Name = "_advancedTabPage";
            _advancedTabPage.Padding = new Padding(8);
            _advancedTabPage.Size = new Size(1070, 234);
            _advancedTabPage.TabIndex = 1;
            _advancedTabPage.Text = "Расширенный brief";
            _advancedTabPage.UseVisualStyleBackColor = true;
            // 
            // _advancedLayout
            // 
            _advancedLayout.ColumnCount = 4;
            _advancedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            _advancedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _advancedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            _advancedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _advancedLayout.Controls.Add(_loreLabel, 0, 0);
            _advancedLayout.Controls.Add(_loreTextBox, 1, 0);
            _advancedLayout.Controls.Add(_hardConstraintsLabel, 2, 0);
            _advancedLayout.Controls.Add(_hardConstraintsTextBox, 3, 0);
            _advancedLayout.Controls.Add(_mustIncludeLabel, 0, 1);
            _advancedLayout.Controls.Add(_mustIncludeTextBox, 1, 1);
            _advancedLayout.Controls.Add(_mustAvoidLabel, 2, 1);
            _advancedLayout.Controls.Add(_mustAvoidTextBox, 3, 1);
            _advancedLayout.Controls.Add(_playerFantasyLabel, 0, 2);
            _advancedLayout.Controls.Add(_playerFantasyTextBox, 1, 2);
            _advancedLayout.Controls.Add(_gameplayLogicLabel, 2, 2);
            _advancedLayout.Controls.Add(_gameplayLogicTextBox, 3, 2);
            _advancedLayout.Dock = DockStyle.Fill;
            _advancedLayout.Location = new Point(8, 8);
            _advancedLayout.Name = "_advancedLayout";
            _advancedLayout.RowCount = 3;
            _advancedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
            _advancedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
            _advancedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.334F));
            _advancedLayout.Size = new Size(1054, 218);
            _advancedLayout.TabIndex = 0;
            // 
            // _loreLabel
            // 
            _loreLabel.Dock = DockStyle.Fill;
            _loreLabel.Location = new Point(3, 0);
            _loreLabel.Name = "_loreLabel";
            _loreLabel.Size = new Size(164, 72);
            _loreLabel.TabIndex = 0;
            _loreLabel.Text = "Лор / уже существующие факты";
            _loreLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _loreTextBox
            // 
            _loreTextBox.Dock = DockStyle.Fill;
            _loreTextBox.Location = new Point(173, 3);
            _loreTextBox.Multiline = true;
            _loreTextBox.Name = "_loreTextBox";
            _loreTextBox.ScrollBars = ScrollBars.Vertical;
            _loreTextBox.Size = new Size(351, 66);
            _loreTextBox.TabIndex = 1;
            // 
            // _hardConstraintsLabel
            // 
            _hardConstraintsLabel.Dock = DockStyle.Fill;
            _hardConstraintsLabel.Location = new Point(530, 0);
            _hardConstraintsLabel.Name = "_hardConstraintsLabel";
            _hardConstraintsLabel.Size = new Size(164, 72);
            _hardConstraintsLabel.TabIndex = 2;
            _hardConstraintsLabel.Text = "Жёсткие ограничения";
            _hardConstraintsLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _hardConstraintsTextBox
            // 
            _hardConstraintsTextBox.Dock = DockStyle.Fill;
            _hardConstraintsTextBox.Location = new Point(700, 3);
            _hardConstraintsTextBox.Multiline = true;
            _hardConstraintsTextBox.Name = "_hardConstraintsTextBox";
            _hardConstraintsTextBox.ScrollBars = ScrollBars.Vertical;
            _hardConstraintsTextBox.Size = new Size(351, 66);
            _hardConstraintsTextBox.TabIndex = 3;
            // 
            // _mustIncludeLabel
            // 
            _mustIncludeLabel.Dock = DockStyle.Fill;
            _mustIncludeLabel.Location = new Point(3, 72);
            _mustIncludeLabel.Name = "_mustIncludeLabel";
            _mustIncludeLabel.Size = new Size(164, 72);
            _mustIncludeLabel.TabIndex = 4;
            _mustIncludeLabel.Text = "Обязательно включить";
            _mustIncludeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _mustIncludeTextBox
            // 
            _mustIncludeTextBox.Dock = DockStyle.Fill;
            _mustIncludeTextBox.Location = new Point(173, 75);
            _mustIncludeTextBox.Multiline = true;
            _mustIncludeTextBox.Name = "_mustIncludeTextBox";
            _mustIncludeTextBox.ScrollBars = ScrollBars.Vertical;
            _mustIncludeTextBox.Size = new Size(351, 66);
            _mustIncludeTextBox.TabIndex = 5;
            // 
            // _mustAvoidLabel
            // 
            _mustAvoidLabel.Dock = DockStyle.Fill;
            _mustAvoidLabel.Location = new Point(530, 72);
            _mustAvoidLabel.Name = "_mustAvoidLabel";
            _mustAvoidLabel.Size = new Size(164, 72);
            _mustAvoidLabel.TabIndex = 6;
            _mustAvoidLabel.Text = "Запрещено / избегать";
            _mustAvoidLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _mustAvoidTextBox
            // 
            _mustAvoidTextBox.Dock = DockStyle.Fill;
            _mustAvoidTextBox.Location = new Point(700, 75);
            _mustAvoidTextBox.Multiline = true;
            _mustAvoidTextBox.Name = "_mustAvoidTextBox";
            _mustAvoidTextBox.ScrollBars = ScrollBars.Vertical;
            _mustAvoidTextBox.Size = new Size(351, 66);
            _mustAvoidTextBox.TabIndex = 7;
            // 
            // _playerFantasyLabel
            // 
            _playerFantasyLabel.Dock = DockStyle.Fill;
            _playerFantasyLabel.Location = new Point(3, 144);
            _playerFantasyLabel.Name = "_playerFantasyLabel";
            _playerFantasyLabel.Size = new Size(164, 74);
            _playerFantasyLabel.TabIndex = 8;
            _playerFantasyLabel.Text = "Что должен чувствовать/делать игрок";
            _playerFantasyLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _playerFantasyTextBox
            // 
            _playerFantasyTextBox.Dock = DockStyle.Fill;
            _playerFantasyTextBox.Location = new Point(173, 147);
            _playerFantasyTextBox.Multiline = true;
            _playerFantasyTextBox.Name = "_playerFantasyTextBox";
            _playerFantasyTextBox.ScrollBars = ScrollBars.Vertical;
            _playerFantasyTextBox.Size = new Size(351, 68);
            _playerFantasyTextBox.TabIndex = 9;
            // 
            // _gameplayLogicLabel
            // 
            _gameplayLogicLabel.Dock = DockStyle.Fill;
            _gameplayLogicLabel.Location = new Point(530, 144);
            _gameplayLogicLabel.Name = "_gameplayLogicLabel";
            _gameplayLogicLabel.Size = new Size(164, 74);
            _gameplayLogicLabel.TabIndex = 10;
            _gameplayLogicLabel.Text = "Логика геймплея / будущие Lua hooks";
            _gameplayLogicLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _gameplayLogicTextBox
            // 
            _gameplayLogicTextBox.Dock = DockStyle.Fill;
            _gameplayLogicTextBox.Location = new Point(700, 147);
            _gameplayLogicTextBox.Multiline = true;
            _gameplayLogicTextBox.Name = "_gameplayLogicTextBox";
            _gameplayLogicTextBox.ScrollBars = ScrollBars.Vertical;
            _gameplayLogicTextBox.Size = new Size(351, 68);
            _gameplayLogicTextBox.TabIndex = 11;
            // 
            // _scopeTabPage
            // 
            _scopeTabPage.Controls.Add(_scopeLayout);
            _scopeTabPage.Location = new Point(4, 24);
            _scopeTabPage.Name = "_scopeTabPage";
            _scopeTabPage.Padding = new Padding(8);
            _scopeTabPage.Size = new Size(1070, 234);
            _scopeTabPage.TabIndex = 2;
            _scopeTabPage.Text = "Scope и логика";
            _scopeTabPage.UseVisualStyleBackColor = true;
            // 
            // _scopeLayout
            // 
            _scopeLayout.ColumnCount = 4;
            _scopeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            _scopeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _scopeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            _scopeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _scopeLayout.Controls.Add(_maxTileOverridesLabel, 0, 0);
            _scopeLayout.Controls.Add(_maxTileOverridesNumeric, 1, 0);
            _scopeLayout.Controls.Add(_targetNpcLabel, 2, 0);
            _scopeLayout.Controls.Add(_targetNpcNumeric, 3, 0);
            _scopeLayout.Controls.Add(_targetEntityLabel, 0, 1);
            _scopeLayout.Controls.Add(_targetEntityNumeric, 1, 1);
            _scopeLayout.Controls.Add(_targetQuestLabel, 2, 1);
            _scopeLayout.Controls.Add(_targetQuestNumeric, 3, 1);
            _scopeLayout.Controls.Add(_targetDialogueLabel, 0, 2);
            _scopeLayout.Controls.Add(_targetDialogueNumeric, 1, 2);
            _scopeLayout.Controls.Add(_detailModeLabel, 2, 2);
            _scopeLayout.Controls.Add(_detailModeComboBox, 3, 2);
            _scopeLayout.Controls.Add(_logicModeLabel, 0, 3);
            _scopeLayout.Controls.Add(_logicModeComboBox, 1, 3);
            _scopeLayout.Dock = DockStyle.Fill;
            _scopeLayout.Location = new Point(8, 8);
            _scopeLayout.Name = "_scopeLayout";
            _scopeLayout.RowCount = 5;
            _scopeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _scopeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _scopeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _scopeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            _scopeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _scopeLayout.Size = new Size(1054, 218);
            _scopeLayout.TabIndex = 0;
            // 
            // _maxTileOverridesLabel
            // 
            _maxTileOverridesLabel.Dock = DockStyle.Fill;
            _maxTileOverridesLabel.Location = new Point(3, 0);
            _maxTileOverridesLabel.Name = "_maxTileOverridesLabel";
            _maxTileOverridesLabel.Size = new Size(174, 34);
            _maxTileOverridesLabel.TabIndex = 0;
            _maxTileOverridesLabel.Text = "Max tile overrides:";
            _maxTileOverridesLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _maxTileOverridesNumeric
            // 
            _maxTileOverridesNumeric.Dock = DockStyle.Left;
            _maxTileOverridesNumeric.Location = new Point(183, 3);
            _maxTileOverridesNumeric.Maximum = new decimal(new int[] { 160, 0, 0, 0 });
            _maxTileOverridesNumeric.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            _maxTileOverridesNumeric.Name = "_maxTileOverridesNumeric";
            _maxTileOverridesNumeric.Size = new Size(120, 23);
            _maxTileOverridesNumeric.TabIndex = 1;
            _maxTileOverridesNumeric.Value = new decimal(new int[] { 40, 0, 0, 0 });
            // 
            // _targetNpcLabel
            // 
            _targetNpcLabel.Dock = DockStyle.Fill;
            _targetNpcLabel.Location = new Point(530, 0);
            _targetNpcLabel.Name = "_targetNpcLabel";
            _targetNpcLabel.Size = new Size(174, 34);
            _targetNpcLabel.TabIndex = 2;
            _targetNpcLabel.Text = "Target NPC prototypes:";
            _targetNpcLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _targetNpcNumeric
            // 
            _targetNpcNumeric.Dock = DockStyle.Left;
            _targetNpcNumeric.Location = new Point(710, 3);
            _targetNpcNumeric.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            _targetNpcNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _targetNpcNumeric.Name = "_targetNpcNumeric";
            _targetNpcNumeric.Size = new Size(120, 23);
            _targetNpcNumeric.TabIndex = 3;
            _targetNpcNumeric.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // _targetEntityLabel
            // 
            _targetEntityLabel.Dock = DockStyle.Fill;
            _targetEntityLabel.Location = new Point(3, 34);
            _targetEntityLabel.Name = "_targetEntityLabel";
            _targetEntityLabel.Size = new Size(174, 34);
            _targetEntityLabel.TabIndex = 4;
            _targetEntityLabel.Text = "Target entity instances:";
            _targetEntityLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _targetEntityNumeric
            // 
            _targetEntityNumeric.Dock = DockStyle.Left;
            _targetEntityNumeric.Location = new Point(183, 37);
            _targetEntityNumeric.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            _targetEntityNumeric.Name = "_targetEntityNumeric";
            _targetEntityNumeric.Size = new Size(120, 23);
            _targetEntityNumeric.TabIndex = 5;
            _targetEntityNumeric.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // _targetQuestLabel
            // 
            _targetQuestLabel.Dock = DockStyle.Fill;
            _targetQuestLabel.Location = new Point(530, 34);
            _targetQuestLabel.Name = "_targetQuestLabel";
            _targetQuestLabel.Size = new Size(174, 34);
            _targetQuestLabel.TabIndex = 6;
            _targetQuestLabel.Text = "Target quests:";
            _targetQuestLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _targetQuestNumeric
            // 
            _targetQuestNumeric.Dock = DockStyle.Left;
            _targetQuestNumeric.Location = new Point(710, 37);
            _targetQuestNumeric.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            _targetQuestNumeric.Name = "_targetQuestNumeric";
            _targetQuestNumeric.Size = new Size(120, 23);
            _targetQuestNumeric.TabIndex = 7;
            _targetQuestNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // _targetDialogueLabel
            // 
            _targetDialogueLabel.Dock = DockStyle.Fill;
            _targetDialogueLabel.Location = new Point(3, 68);
            _targetDialogueLabel.Name = "_targetDialogueLabel";
            _targetDialogueLabel.Size = new Size(174, 34);
            _targetDialogueLabel.TabIndex = 8;
            _targetDialogueLabel.Text = "Target dialogues:";
            _targetDialogueLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _targetDialogueNumeric
            // 
            _targetDialogueNumeric.Dock = DockStyle.Left;
            _targetDialogueNumeric.Location = new Point(183, 71);
            _targetDialogueNumeric.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            _targetDialogueNumeric.Name = "_targetDialogueNumeric";
            _targetDialogueNumeric.Size = new Size(120, 23);
            _targetDialogueNumeric.TabIndex = 9;
            _targetDialogueNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // _detailModeLabel
            // 
            _detailModeLabel.Dock = DockStyle.Fill;
            _detailModeLabel.Location = new Point(530, 68);
            _detailModeLabel.Name = "_detailModeLabel";
            _detailModeLabel.Size = new Size(174, 34);
            _detailModeLabel.TabIndex = 10;
            _detailModeLabel.Text = "Detail mode:";
            _detailModeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _detailModeComboBox
            // 
            _detailModeComboBox.Dock = DockStyle.Fill;
            _detailModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _detailModeComboBox.Items.AddRange(new object[] { "compact", "balanced", "detailed" });
            _detailModeComboBox.Location = new Point(710, 71);
            _detailModeComboBox.Name = "_detailModeComboBox";
            _detailModeComboBox.Size = new Size(341, 23);
            _detailModeComboBox.TabIndex = 11;
            // 
            // _logicModeLabel
            // 
            _logicModeLabel.Dock = DockStyle.Fill;
            _logicModeLabel.Location = new Point(3, 102);
            _logicModeLabel.Name = "_logicModeLabel";
            _logicModeLabel.Size = new Size(174, 34);
            _logicModeLabel.TabIndex = 12;
            _logicModeLabel.Text = "Logic mode:";
            _logicModeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _logicModeComboBox
            // 
            _logicModeComboBox.Dock = DockStyle.Fill;
            _logicModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _logicModeComboBox.Items.AddRange(new object[] { "data-only", "data-plus-script-plan", "no-scripts" });
            _logicModeComboBox.Location = new Point(183, 105);
            _logicModeComboBox.Name = "_logicModeComboBox";
            _logicModeComboBox.Size = new Size(341, 23);
            _logicModeComboBox.TabIndex = 13;
            // 
            // _buttonLayout
            // 
            _buttonLayout.ColumnCount = 8;
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            _buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _buttonLayout.Controls.Add(_testLmStudioButton, 0, 0);
            _buttonLayout.Controls.Add(_aiHelperButton, 1, 0);
            _buttonLayout.Controls.Add(_generateButton, 2, 0);
            _buttonLayout.Controls.Add(_applyButton, 3, 0);
            _buttonLayout.Controls.Add(_saveButton, 4, 0);
            _buttonLayout.Controls.Add(_validateButton, 5, 0);
            _buttonLayout.Controls.Add(_cancelButton, 6, 0);
            _buttonLayout.Controls.Add(_statusLabel, 7, 0);
            _buttonLayout.Dock = DockStyle.Fill;
            _buttonLayout.Location = new Point(11, 337);
            _buttonLayout.Name = "_buttonLayout";
            _buttonLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            _buttonLayout.Size = new Size(1078, 36);
            _buttonLayout.TabIndex = 2;
            // 
            // _testLmStudioButton
            // 
            _testLmStudioButton.Dock = DockStyle.Fill;
            _testLmStudioButton.Location = new Point(3, 3);
            _testLmStudioButton.Name = "_testLmStudioButton";
            _testLmStudioButton.Size = new Size(114, 30);
            _testLmStudioButton.TabIndex = 0;
            _testLmStudioButton.Text = "Тест LM Studio";
            _testLmStudioButton.UseVisualStyleBackColor = true;
            // 
            // _aiHelperButton
            // 
            _aiHelperButton.Dock = DockStyle.Fill;
            _aiHelperButton.Location = new Point(123, 3);
            _aiHelperButton.Name = "_aiHelperButton";
            _aiHelperButton.Size = new Size(184, 30);
            _aiHelperButton.TabIndex = 1;
            _aiHelperButton.Text = "ИИ: вопросы и варианты";
            _aiHelperButton.UseVisualStyleBackColor = true;
            // 
            // _generateButton
            // 
            _generateButton.Dock = DockStyle.Fill;
            _generateButton.Location = new Point(313, 3);
            _generateButton.Name = "_generateButton";
            _generateButton.Size = new Size(204, 30);
            _generateButton.TabIndex = 2;
            _generateButton.Text = "Сгенерировать playable slice";
            _generateButton.UseVisualStyleBackColor = true;
            // 
            // _applyButton
            // 
            _applyButton.Dock = DockStyle.Fill;
            _applyButton.Enabled = false;
            _applyButton.Location = new Point(523, 3);
            _applyButton.Name = "_applyButton";
            _applyButton.Size = new Size(139, 30);
            _applyButton.TabIndex = 3;
            _applyButton.Text = "Legacy direct apply";
            _applyButton.UseVisualStyleBackColor = true;
            // 
            // _saveButton
            // 
            _saveButton.Dock = DockStyle.Fill;
            _saveButton.Location = new Point(668, 3);
            _saveButton.Name = "_saveButton";
            _saveButton.Size = new Size(134, 30);
            _saveButton.TabIndex = 4;
            _saveButton.Text = "Сохранить";
            _saveButton.UseVisualStyleBackColor = true;
            // 
            // _validateButton
            // 
            _validateButton.Dock = DockStyle.Fill;
            _validateButton.Location = new Point(808, 3);
            _validateButton.Name = "_validateButton";
            _validateButton.Size = new Size(134, 30);
            _validateButton.TabIndex = 5;
            _validateButton.Text = "Проверить";
            _validateButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            _cancelButton.Dock = DockStyle.Fill;
            _cancelButton.Enabled = false;
            _cancelButton.Location = new Point(948, 3);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new Size(94, 30);
            _cancelButton.TabIndex = 6;
            _cancelButton.Text = "Отменить";
            _cancelButton.UseVisualStyleBackColor = true;
            // 
            // _statusLabel
            // 
            _statusLabel.AutoEllipsis = true;
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.Location = new Point(1048, 0);
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Size = new Size(27, 36);
            _statusLabel.TabIndex = 7;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _outputSplitContainer
            // 
            _outputSplitContainer.Dock = DockStyle.Fill;
            _outputSplitContainer.Location = new Point(11, 379);
            _outputSplitContainer.Name = "_outputSplitContainer";
            // 
            // _outputSplitContainer.Panel1
            // 
            _outputSplitContainer.Panel1.Controls.Add(_rawJsonTextBox);
            // 
            // _outputSplitContainer.Panel2
            // 
            _outputSplitContainer.Panel2.Controls.Add(_resultTextBox);
            _outputSplitContainer.Size = new Size(1078, 370);
            _outputSplitContainer.SplitterDistance = 539;
            _outputSplitContainer.TabIndex = 3;
            // 
            // _rawJsonTextBox
            // 
            _rawJsonTextBox.Dock = DockStyle.Fill;
            _rawJsonTextBox.Font = new Font("Consolas", 9F);
            _rawJsonTextBox.Location = new Point(0, 0);
            _rawJsonTextBox.Multiline = true;
            _rawJsonTextBox.Name = "_rawJsonTextBox";
            _rawJsonTextBox.ReadOnly = true;
            _rawJsonTextBox.ScrollBars = ScrollBars.Both;
            _rawJsonTextBox.Size = new Size(539, 370);
            _rawJsonTextBox.TabIndex = 0;
            _rawJsonTextBox.WordWrap = false;
            // 
            // _resultTextBox
            // 
            _resultTextBox.Dock = DockStyle.Fill;
            _resultTextBox.Font = new Font("Consolas", 9F);
            _resultTextBox.Location = new Point(0, 0);
            _resultTextBox.Multiline = true;
            _resultTextBox.Name = "_resultTextBox";
            _resultTextBox.ReadOnly = true;
            _resultTextBox.ScrollBars = ScrollBars.Both;
            _resultTextBox.Size = new Size(535, 370);
            _resultTextBox.TabIndex = 0;
            _resultTextBox.WordWrap = false;
            // 
            // GenerationPageControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_rootLayout);
            Name = "GenerationPageControl";
            Size = new Size(1100, 760);
            _rootLayout.ResumeLayout(false);
            _topLayout.ResumeLayout(false);
            _briefTabs.ResumeLayout(false);
            _coreTabPage.ResumeLayout(false);
            _coreLayout.ResumeLayout(false);
            _coreLayout.PerformLayout();
            _mapSizePanel.ResumeLayout(false);
            _mapSizePanel.PerformLayout();
            ((ISupportInitialize)_mapWidthNumeric).EndInit();
            ((ISupportInitialize)_mapHeightNumeric).EndInit();
            _advancedTabPage.ResumeLayout(false);
            _advancedLayout.ResumeLayout(false);
            _advancedLayout.PerformLayout();
            _scopeTabPage.ResumeLayout(false);
            _scopeLayout.ResumeLayout(false);
            ((ISupportInitialize)_maxTileOverridesNumeric).EndInit();
            ((ISupportInitialize)_targetNpcNumeric).EndInit();
            ((ISupportInitialize)_targetEntityNumeric).EndInit();
            ((ISupportInitialize)_targetQuestNumeric).EndInit();
            ((ISupportInitialize)_targetDialogueNumeric).EndInit();
            _buttonLayout.ResumeLayout(false);
            _outputSplitContainer.Panel1.ResumeLayout(false);
            _outputSplitContainer.Panel1.PerformLayout();
            _outputSplitContainer.Panel2.ResumeLayout(false);
            _outputSplitContainer.Panel2.PerformLayout();
            ((ISupportInitialize)_outputSplitContainer).EndInit();
            _outputSplitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
