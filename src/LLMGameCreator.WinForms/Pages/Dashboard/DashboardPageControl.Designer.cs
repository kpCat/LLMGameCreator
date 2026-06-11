#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class DashboardPageControl
    {
        private IContainer components;
        private TableLayoutPanel _rootLayout;
        private Panel _headerPanel;
        private Label _currentPackageLabel;
        private Label _currentFolderLabel;
        private Label _validationSummaryLabel;
        private FlowLayoutPanel _actionsPanel;
        private Button _savePackageButton;
        private Button _validatePackageButton;
        private TabControl _tabs;
        private TabPage _manifestTabPage;
        private TabPage _mapsTabPage;
        private TabPage _tilesTabPage;
        private TabPage _entitiesTabPage;
        private TabPage _assetsScriptsTabPage;
        private TabPage _validationTabPage;
        private TableLayoutPanel _manifestLayout;
        private Label _packageIdLabel;
        private TextBox _packageIdTextBox;
        private Label _titleLabel;
        private TextBox _titleTextBox;
        private Label _versionLabel;
        private TextBox _versionTextBox;
        private Label _formatVersionLabel;
        private TextBox _formatVersionTextBox;
        private Label _startMapLabel;
        private ComboBox _startMapComboBox;
        private Label _descriptionLabel;
        private TextBox _descriptionTextBox;
        private Button _applyManifestButton;
        private SplitContainer _mapsSplitContainer;
        private ListView _mapsListView;
        private ColumnHeader _mapIdColumnHeader;
        private ColumnHeader _mapNameColumnHeader;
        private ColumnHeader _mapSizeColumnHeader;
        private ColumnHeader _mapDefaultTileColumnHeader;
        private ColumnHeader _mapStartColumnHeader;
        private TableLayoutPanel _mapEditorLayout;
        private Label _mapIdLabel;
        private TextBox _mapIdTextBox;
        private Label _mapNameLabel;
        private TextBox _mapNameTextBox;
        private Label _mapWidthLabel;
        private NumericUpDown _mapWidthNumeric;
        private Label _mapHeightLabel;
        private NumericUpDown _mapHeightNumeric;
        private Label _mapDefaultTileLabel;
        private ComboBox _mapDefaultTileComboBox;
        private Label _mapStartXLabel;
        private NumericUpDown _mapStartXNumeric;
        private Label _mapStartYLabel;
        private NumericUpDown _mapStartYNumeric;
        private FlowLayoutPanel _mapButtonsPanel;
        private Button _addMapButton;
        private Button _updateMapButton;
        private Button _removeMapButton;
        private SplitContainer _tilesSplitContainer;
        private ListView _tilesListView;
        private ColumnHeader _tileIdColumnHeader;
        private ColumnHeader _tileNameColumnHeader;
        private ColumnHeader _tileWalkableColumnHeader;
        private ColumnHeader _tileMovementCostColumnHeader;
        private ColumnHeader _tileAssetColumnHeader;
        private TableLayoutPanel _tileEditorLayout;
        private Label _tileIdLabel;
        private TextBox _tileIdTextBox;
        private Label _tileNameLabel;
        private TextBox _tileNameTextBox;
        private CheckBox _tileWalkableCheckBox;
        private Label _tileMovementCostLabel;
        private NumericUpDown _tileMovementCostNumeric;
        private Label _tileAssetIdLabel;
        private TextBox _tileAssetIdTextBox;
        private FlowLayoutPanel _tileButtonsPanel;
        private Button _addTileButton;
        private Button _updateTileButton;
        private Button _removeTileButton;
        private SplitContainer _entitiesSplitContainer;
        private ListView _entitiesListView;
        private ColumnHeader _entityIdColumnHeader;
        private ColumnHeader _entityNameColumnHeader;
        private ColumnHeader _entityAssetColumnHeader;
        private ColumnHeader _entityComponentsColumnHeader;
        private TableLayoutPanel _entityEditorLayout;
        private Label _entityIdLabel;
        private TextBox _entityIdTextBox;
        private Label _entityNameLabel;
        private TextBox _entityNameTextBox;
        private Label _entityAssetIdLabel;
        private TextBox _entityAssetIdTextBox;
        private Label _entityComponentsCountLabel;
        private FlowLayoutPanel _entityButtonsPanel;
        private Button _addEntityButton;
        private Button _updateEntityButton;
        private Button _removeEntityButton;
        private SplitContainer _assetsScriptsSplitContainer;
        private ListView _assetsListView;
        private ColumnHeader _assetIdColumnHeader;
        private ColumnHeader _assetTypeColumnHeader;
        private ColumnHeader _assetRoleColumnHeader;
        private ColumnHeader _assetPathColumnHeader;
        private ColumnHeader _assetContractColumnHeader;
        private ListView _scriptsListView;
        private ColumnHeader _scriptIdColumnHeader;
        private ColumnHeader _scriptKindColumnHeader;
        private ColumnHeader _scriptPathColumnHeader;
        private ColumnHeader _scriptEntryPointsColumnHeader;
        private TextBox _validationOutputTextBox;

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
            this.components = new Container();
            this._rootLayout = new TableLayoutPanel();
            this._headerPanel = new Panel();
            this._currentPackageLabel = new Label();
            this._currentFolderLabel = new Label();
            this._validationSummaryLabel = new Label();
            this._actionsPanel = new FlowLayoutPanel();
            this._savePackageButton = new Button();
            this._validatePackageButton = new Button();
            this._tabs = new TabControl();
            this._manifestTabPage = new TabPage();
            this._mapsTabPage = new TabPage();
            this._tilesTabPage = new TabPage();
            this._entitiesTabPage = new TabPage();
            this._assetsScriptsTabPage = new TabPage();
            this._validationTabPage = new TabPage();
            this._manifestLayout = new TableLayoutPanel();
            this._packageIdLabel = new Label();
            this._packageIdTextBox = new TextBox();
            this._titleLabel = new Label();
            this._titleTextBox = new TextBox();
            this._versionLabel = new Label();
            this._versionTextBox = new TextBox();
            this._formatVersionLabel = new Label();
            this._formatVersionTextBox = new TextBox();
            this._startMapLabel = new Label();
            this._startMapComboBox = new ComboBox();
            this._descriptionLabel = new Label();
            this._descriptionTextBox = new TextBox();
            this._applyManifestButton = new Button();
            this._mapsSplitContainer = new SplitContainer();
            this._mapsListView = new ListView();
            this._mapIdColumnHeader = new ColumnHeader();
            this._mapNameColumnHeader = new ColumnHeader();
            this._mapSizeColumnHeader = new ColumnHeader();
            this._mapDefaultTileColumnHeader = new ColumnHeader();
            this._mapStartColumnHeader = new ColumnHeader();
            this._mapEditorLayout = new TableLayoutPanel();
            this._mapIdLabel = new Label();
            this._mapIdTextBox = new TextBox();
            this._mapNameLabel = new Label();
            this._mapNameTextBox = new TextBox();
            this._mapWidthLabel = new Label();
            this._mapWidthNumeric = new NumericUpDown();
            this._mapHeightLabel = new Label();
            this._mapHeightNumeric = new NumericUpDown();
            this._mapDefaultTileLabel = new Label();
            this._mapDefaultTileComboBox = new ComboBox();
            this._mapStartXLabel = new Label();
            this._mapStartXNumeric = new NumericUpDown();
            this._mapStartYLabel = new Label();
            this._mapStartYNumeric = new NumericUpDown();
            this._mapButtonsPanel = new FlowLayoutPanel();
            this._addMapButton = new Button();
            this._updateMapButton = new Button();
            this._removeMapButton = new Button();
            this._tilesSplitContainer = new SplitContainer();
            this._tilesListView = new ListView();
            this._tileIdColumnHeader = new ColumnHeader();
            this._tileNameColumnHeader = new ColumnHeader();
            this._tileWalkableColumnHeader = new ColumnHeader();
            this._tileMovementCostColumnHeader = new ColumnHeader();
            this._tileAssetColumnHeader = new ColumnHeader();
            this._tileEditorLayout = new TableLayoutPanel();
            this._tileIdLabel = new Label();
            this._tileIdTextBox = new TextBox();
            this._tileNameLabel = new Label();
            this._tileNameTextBox = new TextBox();
            this._tileWalkableCheckBox = new CheckBox();
            this._tileMovementCostLabel = new Label();
            this._tileMovementCostNumeric = new NumericUpDown();
            this._tileAssetIdLabel = new Label();
            this._tileAssetIdTextBox = new TextBox();
            this._tileButtonsPanel = new FlowLayoutPanel();
            this._addTileButton = new Button();
            this._updateTileButton = new Button();
            this._removeTileButton = new Button();
            this._entitiesSplitContainer = new SplitContainer();
            this._entitiesListView = new ListView();
            this._entityIdColumnHeader = new ColumnHeader();
            this._entityNameColumnHeader = new ColumnHeader();
            this._entityAssetColumnHeader = new ColumnHeader();
            this._entityComponentsColumnHeader = new ColumnHeader();
            this._entityEditorLayout = new TableLayoutPanel();
            this._entityIdLabel = new Label();
            this._entityIdTextBox = new TextBox();
            this._entityNameLabel = new Label();
            this._entityNameTextBox = new TextBox();
            this._entityAssetIdLabel = new Label();
            this._entityAssetIdTextBox = new TextBox();
            this._entityComponentsCountLabel = new Label();
            this._entityButtonsPanel = new FlowLayoutPanel();
            this._addEntityButton = new Button();
            this._updateEntityButton = new Button();
            this._removeEntityButton = new Button();
            this._assetsScriptsSplitContainer = new SplitContainer();
            this._assetsListView = new ListView();
            this._assetIdColumnHeader = new ColumnHeader();
            this._assetTypeColumnHeader = new ColumnHeader();
            this._assetRoleColumnHeader = new ColumnHeader();
            this._assetPathColumnHeader = new ColumnHeader();
            this._assetContractColumnHeader = new ColumnHeader();
            this._scriptsListView = new ListView();
            this._scriptIdColumnHeader = new ColumnHeader();
            this._scriptKindColumnHeader = new ColumnHeader();
            this._scriptPathColumnHeader = new ColumnHeader();
            this._scriptEntryPointsColumnHeader = new ColumnHeader();
            this._validationOutputTextBox = new TextBox();
            this._rootLayout.SuspendLayout();
            this._headerPanel.SuspendLayout();
            this._actionsPanel.SuspendLayout();
            this._tabs.SuspendLayout();
            this._manifestTabPage.SuspendLayout();
            this._mapsTabPage.SuspendLayout();
            this._tilesTabPage.SuspendLayout();
            this._entitiesTabPage.SuspendLayout();
            this._assetsScriptsTabPage.SuspendLayout();
            this._validationTabPage.SuspendLayout();
            this._manifestLayout.SuspendLayout();
            ((ISupportInitialize)(this._mapsSplitContainer)).BeginInit();
            this._mapsSplitContainer.Panel1.SuspendLayout();
            this._mapsSplitContainer.Panel2.SuspendLayout();
            this._mapsSplitContainer.SuspendLayout();
            this._mapEditorLayout.SuspendLayout();
            ((ISupportInitialize)(this._mapWidthNumeric)).BeginInit();
            ((ISupportInitialize)(this._mapHeightNumeric)).BeginInit();
            ((ISupportInitialize)(this._mapStartXNumeric)).BeginInit();
            ((ISupportInitialize)(this._mapStartYNumeric)).BeginInit();
            this._mapButtonsPanel.SuspendLayout();
            ((ISupportInitialize)(this._tilesSplitContainer)).BeginInit();
            this._tilesSplitContainer.Panel1.SuspendLayout();
            this._tilesSplitContainer.Panel2.SuspendLayout();
            this._tilesSplitContainer.SuspendLayout();
            this._tileEditorLayout.SuspendLayout();
            ((ISupportInitialize)(this._tileMovementCostNumeric)).BeginInit();
            this._tileButtonsPanel.SuspendLayout();
            ((ISupportInitialize)(this._entitiesSplitContainer)).BeginInit();
            this._entitiesSplitContainer.Panel1.SuspendLayout();
            this._entitiesSplitContainer.Panel2.SuspendLayout();
            this._entitiesSplitContainer.SuspendLayout();
            this._entityEditorLayout.SuspendLayout();
            this._entityButtonsPanel.SuspendLayout();
            ((ISupportInitialize)(this._assetsScriptsSplitContainer)).BeginInit();
            this._assetsScriptsSplitContainer.Panel1.SuspendLayout();
            this._assetsScriptsSplitContainer.Panel2.SuspendLayout();
            this._assetsScriptsSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rootLayout
            // 
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._headerPanel, 0, 0);
            this._rootLayout.Controls.Add(this._actionsPanel, 0, 1);
            this._rootLayout.Controls.Add(this._tabs, 0, 2);
            this._rootLayout.Dock = DockStyle.Fill;
            this._rootLayout.Location = new Point(0, 0);
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.Padding = new Padding(10);
            this._rootLayout.RowCount = 3;
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._rootLayout.Size = new Size(1000, 650);
            this._rootLayout.TabIndex = 0;
            // 
            // _headerPanel
            // 
            this._headerPanel.Controls.Add(this._currentPackageLabel);
            this._headerPanel.Controls.Add(this._currentFolderLabel);
            this._headerPanel.Controls.Add(this._validationSummaryLabel);
            this._headerPanel.Dock = DockStyle.Fill;
            this._headerPanel.Location = new Point(13, 13);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Size = new Size(974, 52);
            this._headerPanel.TabIndex = 0;
            // 
            // _currentPackageLabel
            // 
            this._currentPackageLabel.AutoSize = true;
            this._currentPackageLabel.Font = new Font(FontFamily.GenericSansSerif, 11F, FontStyle.Bold, GraphicsUnit.Point);
            this._currentPackageLabel.Location = new Point(0, 0);
            this._currentPackageLabel.Name = "_currentPackageLabel";
            this._currentPackageLabel.Size = new Size(132, 18);
            this._currentPackageLabel.TabIndex = 0;
            this._currentPackageLabel.Text = "Проект не открыт";
            // 
            // _currentFolderLabel
            // 
            this._currentFolderLabel.AutoSize = true;
            this._currentFolderLabel.Location = new Point(0, 26);
            this._currentFolderLabel.Name = "_currentFolderLabel";
            this._currentFolderLabel.Size = new Size(43, 15);
            this._currentFolderLabel.TabIndex = 1;
            this._currentFolderLabel.Text = "Папка:";
            // 
            // _validationSummaryLabel
            // 
            this._validationSummaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._validationSummaryLabel.Location = new Point(660, 0);
            this._validationSummaryLabel.Name = "_validationSummaryLabel";
            this._validationSummaryLabel.Size = new Size(314, 42);
            this._validationSummaryLabel.TabIndex = 2;
            this._validationSummaryLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // _actionsPanel
            // 
            this._actionsPanel.Controls.Add(this._savePackageButton);
            this._actionsPanel.Controls.Add(this._validatePackageButton);
            this._actionsPanel.Dock = DockStyle.Fill;
            this._actionsPanel.Location = new Point(13, 71);
            this._actionsPanel.Name = "_actionsPanel";
            this._actionsPanel.Size = new Size(974, 36);
            this._actionsPanel.TabIndex = 1;
            // 
            // _savePackageButton
            // 
            this._savePackageButton.Location = new Point(3, 3);
            this._savePackageButton.Name = "_savePackageButton";
            this._savePackageButton.Size = new Size(150, 30);
            this._savePackageButton.TabIndex = 0;
            this._savePackageButton.Text = "Сохранить package";
            this._savePackageButton.UseVisualStyleBackColor = true;
            // 
            // _validatePackageButton
            // 
            this._validatePackageButton.Location = new Point(159, 3);
            this._validatePackageButton.Name = "_validatePackageButton";
            this._validatePackageButton.Size = new Size(150, 30);
            this._validatePackageButton.TabIndex = 1;
            this._validatePackageButton.Text = "Проверить package";
            this._validatePackageButton.UseVisualStyleBackColor = true;
            // 
            // _tabs
            // 
            this._tabs.Controls.Add(this._manifestTabPage);
            this._tabs.Controls.Add(this._mapsTabPage);
            this._tabs.Controls.Add(this._tilesTabPage);
            this._tabs.Controls.Add(this._entitiesTabPage);
            this._tabs.Controls.Add(this._assetsScriptsTabPage);
            this._tabs.Controls.Add(this._validationTabPage);
            this._tabs.Dock = DockStyle.Fill;
            this._tabs.Location = new Point(13, 113);
            this._tabs.Name = "_tabs";
            this._tabs.SelectedIndex = 0;
            this._tabs.Size = new Size(974, 524);
            this._tabs.TabIndex = 2;
            // 
            // _manifestTabPage
            // 
            this._manifestTabPage.Controls.Add(this._manifestLayout);
            this._manifestTabPage.Location = new Point(4, 24);
            this._manifestTabPage.Name = "_manifestTabPage";
            this._manifestTabPage.Padding = new Padding(8);
            this._manifestTabPage.Size = new Size(966, 496);
            this._manifestTabPage.TabIndex = 0;
            this._manifestTabPage.Text = "Manifest";
            this._manifestTabPage.UseVisualStyleBackColor = true;
            // 
            // _manifestLayout
            // 
            this._manifestLayout.ColumnCount = 2;
            this._manifestLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            this._manifestLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._manifestLayout.Controls.Add(this._packageIdLabel, 0, 0);
            this._manifestLayout.Controls.Add(this._packageIdTextBox, 1, 0);
            this._manifestLayout.Controls.Add(this._titleLabel, 0, 1);
            this._manifestLayout.Controls.Add(this._titleTextBox, 1, 1);
            this._manifestLayout.Controls.Add(this._versionLabel, 0, 2);
            this._manifestLayout.Controls.Add(this._versionTextBox, 1, 2);
            this._manifestLayout.Controls.Add(this._formatVersionLabel, 0, 3);
            this._manifestLayout.Controls.Add(this._formatVersionTextBox, 1, 3);
            this._manifestLayout.Controls.Add(this._startMapLabel, 0, 4);
            this._manifestLayout.Controls.Add(this._startMapComboBox, 1, 4);
            this._manifestLayout.Controls.Add(this._descriptionLabel, 0, 5);
            this._manifestLayout.Controls.Add(this._descriptionTextBox, 1, 5);
            this._manifestLayout.Controls.Add(this._applyManifestButton, 1, 6);
            this._manifestLayout.Dock = DockStyle.Fill;
            this._manifestLayout.Location = new Point(8, 8);
            this._manifestLayout.Name = "_manifestLayout";
            this._manifestLayout.RowCount = 7;
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._manifestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            this._manifestLayout.Size = new Size(950, 480);
            this._manifestLayout.TabIndex = 0;
            // 
            // Manifest controls
            // 
            this._packageIdLabel.Dock = DockStyle.Fill;
            this._packageIdLabel.Text = "PackageId";
            this._packageIdLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._packageIdTextBox.Dock = DockStyle.Fill;
            this._titleLabel.Dock = DockStyle.Fill;
            this._titleLabel.Text = "Название";
            this._titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._titleTextBox.Dock = DockStyle.Fill;
            this._versionLabel.Dock = DockStyle.Fill;
            this._versionLabel.Text = "Version";
            this._versionLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._versionTextBox.Dock = DockStyle.Fill;
            this._formatVersionLabel.Dock = DockStyle.Fill;
            this._formatVersionLabel.Text = "FormatVersion";
            this._formatVersionLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._formatVersionTextBox.Dock = DockStyle.Fill;
            this._startMapLabel.Dock = DockStyle.Fill;
            this._startMapLabel.Text = "StartMapId";
            this._startMapLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._startMapComboBox.Dock = DockStyle.Fill;
            this._startMapComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._descriptionLabel.Dock = DockStyle.Fill;
            this._descriptionLabel.Text = "Описание";
            this._descriptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._descriptionTextBox.Dock = DockStyle.Fill;
            this._descriptionTextBox.Multiline = true;
            this._descriptionTextBox.ScrollBars = ScrollBars.Vertical;
            this._applyManifestButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this._applyManifestButton.Size = new Size(170, 30);
            this._applyManifestButton.Text = "Применить manifest";
            this._applyManifestButton.UseVisualStyleBackColor = true;
            // 
            // Maps tab
            // 
            this._mapsTabPage.Controls.Add(this._mapsSplitContainer);
            this._mapsTabPage.Location = new Point(4, 24);
            this._mapsTabPage.Name = "_mapsTabPage";
            this._mapsTabPage.Padding = new Padding(8);
            this._mapsTabPage.Size = new Size(966, 496);
            this._mapsTabPage.TabIndex = 1;
            this._mapsTabPage.Text = "Maps";
            this._mapsTabPage.UseVisualStyleBackColor = true;
            this._mapsSplitContainer.Dock = DockStyle.Fill;
            this._mapsSplitContainer.Location = new Point(8, 8);
            this._mapsSplitContainer.Name = "_mapsSplitContainer";
            this._mapsSplitContainer.Size = new Size(950, 480);
            this._mapsSplitContainer.SplitterDistance = 610;
            this._mapsSplitContainer.Panel1.Controls.Add(this._mapsListView);
            this._mapsSplitContainer.Panel2.Controls.Add(this._mapEditorLayout);
            this._mapsListView.Columns.AddRange(new ColumnHeader[] { this._mapIdColumnHeader, this._mapNameColumnHeader, this._mapSizeColumnHeader, this._mapDefaultTileColumnHeader, this._mapStartColumnHeader });
            this._mapsListView.Dock = DockStyle.Fill;
            this._mapsListView.FullRowSelect = true;
            this._mapsListView.MultiSelect = false;
            this._mapsListView.UseCompatibleStateImageBehavior = false;
            this._mapsListView.View = System.Windows.Forms.View.Details;
            this._mapIdColumnHeader.Text = "Id";
            this._mapIdColumnHeader.Width = 180;
            this._mapNameColumnHeader.Text = "Name";
            this._mapNameColumnHeader.Width = 150;
            this._mapSizeColumnHeader.Text = "Size";
            this._mapSizeColumnHeader.Width = 80;
            this._mapDefaultTileColumnHeader.Text = "DefaultTileId";
            this._mapDefaultTileColumnHeader.Width = 130;
            this._mapStartColumnHeader.Text = "Start";
            this._mapStartColumnHeader.Width = 80;
            ConfigureMapEditor();
            // 
            // Tiles tab
            // 
            this._tilesTabPage.Controls.Add(this._tilesSplitContainer);
            this._tilesTabPage.Location = new Point(4, 24);
            this._tilesTabPage.Name = "_tilesTabPage";
            this._tilesTabPage.Padding = new Padding(8);
            this._tilesTabPage.Size = new Size(966, 496);
            this._tilesTabPage.TabIndex = 2;
            this._tilesTabPage.Text = "Tiles";
            this._tilesTabPage.UseVisualStyleBackColor = true;
            this._tilesSplitContainer.Dock = DockStyle.Fill;
            this._tilesSplitContainer.Name = "_tilesSplitContainer";
            this._tilesSplitContainer.Size = new Size(950, 480);
            this._tilesSplitContainer.SplitterDistance = 610;
            this._tilesSplitContainer.Panel1.Controls.Add(this._tilesListView);
            this._tilesSplitContainer.Panel2.Controls.Add(this._tileEditorLayout);
            this._tilesListView.Columns.AddRange(new ColumnHeader[] { this._tileIdColumnHeader, this._tileNameColumnHeader, this._tileWalkableColumnHeader, this._tileMovementCostColumnHeader, this._tileAssetColumnHeader });
            this._tilesListView.Dock = DockStyle.Fill;
            this._tilesListView.FullRowSelect = true;
            this._tilesListView.MultiSelect = false;
            this._tilesListView.UseCompatibleStateImageBehavior = false;
            this._tilesListView.View = System.Windows.Forms.View.Details;
            this._tileIdColumnHeader.Text = "Id";
            this._tileIdColumnHeader.Width = 170;
            this._tileNameColumnHeader.Text = "Name";
            this._tileNameColumnHeader.Width = 130;
            this._tileWalkableColumnHeader.Text = "Walkable";
            this._tileWalkableColumnHeader.Width = 80;
            this._tileMovementCostColumnHeader.Text = "MovementCost";
            this._tileMovementCostColumnHeader.Width = 110;
            this._tileAssetColumnHeader.Text = "AssetId";
            this._tileAssetColumnHeader.Width = 180;
            ConfigureTileEditor();
            // 
            // Entities tab
            // 
            this._entitiesTabPage.Controls.Add(this._entitiesSplitContainer);
            this._entitiesTabPage.Location = new Point(4, 24);
            this._entitiesTabPage.Name = "_entitiesTabPage";
            this._entitiesTabPage.Padding = new Padding(8);
            this._entitiesTabPage.Size = new Size(966, 496);
            this._entitiesTabPage.TabIndex = 3;
            this._entitiesTabPage.Text = "Entity Prototypes";
            this._entitiesTabPage.UseVisualStyleBackColor = true;
            this._entitiesSplitContainer.Dock = DockStyle.Fill;
            this._entitiesSplitContainer.Name = "_entitiesSplitContainer";
            this._entitiesSplitContainer.Size = new Size(950, 480);
            this._entitiesSplitContainer.SplitterDistance = 610;
            this._entitiesSplitContainer.Panel1.Controls.Add(this._entitiesListView);
            this._entitiesSplitContainer.Panel2.Controls.Add(this._entityEditorLayout);
            this._entitiesListView.Columns.AddRange(new ColumnHeader[] { this._entityIdColumnHeader, this._entityNameColumnHeader, this._entityAssetColumnHeader, this._entityComponentsColumnHeader });
            this._entitiesListView.Dock = DockStyle.Fill;
            this._entitiesListView.FullRowSelect = true;
            this._entitiesListView.MultiSelect = false;
            this._entitiesListView.UseCompatibleStateImageBehavior = false;
            this._entitiesListView.View = System.Windows.Forms.View.Details;
            this._entityIdColumnHeader.Text = "Id";
            this._entityIdColumnHeader.Width = 220;
            this._entityNameColumnHeader.Text = "Name";
            this._entityNameColumnHeader.Width = 160;
            this._entityAssetColumnHeader.Text = "AssetId";
            this._entityAssetColumnHeader.Width = 190;
            this._entityComponentsColumnHeader.Text = "Components";
            this._entityComponentsColumnHeader.Width = 90;
            ConfigureEntityEditor();
            // 
            // Assets/scripts tab
            // 
            this._assetsScriptsTabPage.Controls.Add(this._assetsScriptsSplitContainer);
            this._assetsScriptsTabPage.Location = new Point(4, 24);
            this._assetsScriptsTabPage.Name = "_assetsScriptsTabPage";
            this._assetsScriptsTabPage.Padding = new Padding(8);
            this._assetsScriptsTabPage.Size = new Size(966, 496);
            this._assetsScriptsTabPage.TabIndex = 4;
            this._assetsScriptsTabPage.Text = "Assets/Scripts";
            this._assetsScriptsTabPage.UseVisualStyleBackColor = true;
            this._assetsScriptsSplitContainer.Dock = DockStyle.Fill;
            this._assetsScriptsSplitContainer.Orientation = Orientation.Horizontal;
            this._assetsScriptsSplitContainer.SplitterDistance = 240;
            this._assetsScriptsSplitContainer.Panel1.Controls.Add(this._assetsListView);
            this._assetsScriptsSplitContainer.Panel2.Controls.Add(this._scriptsListView);
            this._assetsListView.Columns.AddRange(new ColumnHeader[] { this._assetIdColumnHeader, this._assetTypeColumnHeader, this._assetRoleColumnHeader, this._assetPathColumnHeader, this._assetContractColumnHeader });
            this._assetsListView.Dock = DockStyle.Fill;
            this._assetsListView.FullRowSelect = true;
            this._assetsListView.UseCompatibleStateImageBehavior = false;
            this._assetsListView.View = System.Windows.Forms.View.Details;
            this._assetIdColumnHeader.Text = "Asset Id";
            this._assetIdColumnHeader.Width = 220;
            this._assetTypeColumnHeader.Text = "Type";
            this._assetTypeColumnHeader.Width = 130;
            this._assetRoleColumnHeader.Text = "Role";
            this._assetRoleColumnHeader.Width = 120;
            this._assetPathColumnHeader.Text = "Path";
            this._assetPathColumnHeader.Width = 260;
            this._assetContractColumnHeader.Text = "Contract";
            this._assetContractColumnHeader.Width = 160;
            this._scriptsListView.Columns.AddRange(new ColumnHeader[] { this._scriptIdColumnHeader, this._scriptKindColumnHeader, this._scriptPathColumnHeader, this._scriptEntryPointsColumnHeader });
            this._scriptsListView.Dock = DockStyle.Fill;
            this._scriptsListView.FullRowSelect = true;
            this._scriptsListView.UseCompatibleStateImageBehavior = false;
            this._scriptsListView.View = System.Windows.Forms.View.Details;
            this._scriptIdColumnHeader.Text = "Script Id";
            this._scriptIdColumnHeader.Width = 220;
            this._scriptKindColumnHeader.Text = "Kind";
            this._scriptKindColumnHeader.Width = 110;
            this._scriptPathColumnHeader.Text = "Path";
            this._scriptPathColumnHeader.Width = 280;
            this._scriptEntryPointsColumnHeader.Text = "EntryPoints";
            this._scriptEntryPointsColumnHeader.Width = 240;
            // 
            // Validation tab
            // 
            this._validationTabPage.Controls.Add(this._validationOutputTextBox);
            this._validationTabPage.Location = new Point(4, 24);
            this._validationTabPage.Name = "_validationTabPage";
            this._validationTabPage.Padding = new Padding(8);
            this._validationTabPage.Size = new Size(966, 496);
            this._validationTabPage.TabIndex = 5;
            this._validationTabPage.Text = "Validation";
            this._validationTabPage.UseVisualStyleBackColor = true;
            this._validationOutputTextBox.Dock = DockStyle.Fill;
            this._validationOutputTextBox.Multiline = true;
            this._validationOutputTextBox.ReadOnly = true;
            this._validationOutputTextBox.ScrollBars = ScrollBars.Both;
            this._validationOutputTextBox.WordWrap = false;
            // 
            // DashboardPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._rootLayout);
            this.Name = "DashboardPageControl";
            this.Size = new Size(1000, 650);
            this._rootLayout.ResumeLayout(false);
            this._headerPanel.ResumeLayout(false);
            this._headerPanel.PerformLayout();
            this._actionsPanel.ResumeLayout(false);
            this._tabs.ResumeLayout(false);
            this._manifestTabPage.ResumeLayout(false);
            this._mapsTabPage.ResumeLayout(false);
            this._tilesTabPage.ResumeLayout(false);
            this._entitiesTabPage.ResumeLayout(false);
            this._assetsScriptsTabPage.ResumeLayout(false);
            this._validationTabPage.ResumeLayout(false);
            this._validationTabPage.PerformLayout();
            this._manifestLayout.ResumeLayout(false);
            this._manifestLayout.PerformLayout();
            this._mapsSplitContainer.Panel1.ResumeLayout(false);
            this._mapsSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._mapsSplitContainer)).EndInit();
            this._mapsSplitContainer.ResumeLayout(false);
            this._mapEditorLayout.ResumeLayout(false);
            this._mapEditorLayout.PerformLayout();
            ((ISupportInitialize)(this._mapWidthNumeric)).EndInit();
            ((ISupportInitialize)(this._mapHeightNumeric)).EndInit();
            ((ISupportInitialize)(this._mapStartXNumeric)).EndInit();
            ((ISupportInitialize)(this._mapStartYNumeric)).EndInit();
            this._mapButtonsPanel.ResumeLayout(false);
            this._tilesSplitContainer.Panel1.ResumeLayout(false);
            this._tilesSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._tilesSplitContainer)).EndInit();
            this._tilesSplitContainer.ResumeLayout(false);
            this._tileEditorLayout.ResumeLayout(false);
            this._tileEditorLayout.PerformLayout();
            ((ISupportInitialize)(this._tileMovementCostNumeric)).EndInit();
            this._tileButtonsPanel.ResumeLayout(false);
            this._entitiesSplitContainer.Panel1.ResumeLayout(false);
            this._entitiesSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._entitiesSplitContainer)).EndInit();
            this._entitiesSplitContainer.ResumeLayout(false);
            this._entityEditorLayout.ResumeLayout(false);
            this._entityEditorLayout.PerformLayout();
            this._entityButtonsPanel.ResumeLayout(false);
            this._assetsScriptsSplitContainer.Panel1.ResumeLayout(false);
            this._assetsScriptsSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this._assetsScriptsSplitContainer)).EndInit();
            this._assetsScriptsSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void ConfigureMapEditor()
        {
            this._mapEditorLayout.ColumnCount = 2;
            this._mapEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
            this._mapEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._mapEditorLayout.Dock = DockStyle.Fill;
            this._mapEditorLayout.Padding = new Padding(8);
            this._mapEditorLayout.RowCount = 8;
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._mapEditorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._mapEditorLayout.Controls.Add(this._mapIdLabel, 0, 0);
            this._mapEditorLayout.Controls.Add(this._mapIdTextBox, 1, 0);
            this._mapEditorLayout.Controls.Add(this._mapNameLabel, 0, 1);
            this._mapEditorLayout.Controls.Add(this._mapNameTextBox, 1, 1);
            this._mapEditorLayout.Controls.Add(this._mapWidthLabel, 0, 2);
            this._mapEditorLayout.Controls.Add(this._mapWidthNumeric, 1, 2);
            this._mapEditorLayout.Controls.Add(this._mapHeightLabel, 0, 3);
            this._mapEditorLayout.Controls.Add(this._mapHeightNumeric, 1, 3);
            this._mapEditorLayout.Controls.Add(this._mapDefaultTileLabel, 0, 4);
            this._mapEditorLayout.Controls.Add(this._mapDefaultTileComboBox, 1, 4);
            this._mapEditorLayout.Controls.Add(this._mapStartXLabel, 0, 5);
            this._mapEditorLayout.Controls.Add(this._mapStartXNumeric, 1, 5);
            this._mapEditorLayout.Controls.Add(this._mapStartYLabel, 0, 6);
            this._mapEditorLayout.Controls.Add(this._mapStartYNumeric, 1, 6);
            this._mapEditorLayout.Controls.Add(this._mapButtonsPanel, 0, 7);
            this._mapEditorLayout.SetColumnSpan(this._mapButtonsPanel, 2);
            this._mapIdLabel.Text = "Id";
            this._mapNameLabel.Text = "Name";
            this._mapWidthLabel.Text = "Width";
            this._mapHeightLabel.Text = "Height";
            this._mapDefaultTileLabel.Text = "Default tile";
            this._mapStartXLabel.Text = "Start X";
            this._mapStartYLabel.Text = "Start Y";
            ConfigureEditorLabel(this._mapIdLabel);
            ConfigureEditorLabel(this._mapNameLabel);
            ConfigureEditorLabel(this._mapWidthLabel);
            ConfigureEditorLabel(this._mapHeightLabel);
            ConfigureEditorLabel(this._mapDefaultTileLabel);
            ConfigureEditorLabel(this._mapStartXLabel);
            ConfigureEditorLabel(this._mapStartYLabel);
            this._mapIdTextBox.Dock = DockStyle.Fill;
            this._mapNameTextBox.Dock = DockStyle.Fill;
            this._mapDefaultTileComboBox.Dock = DockStyle.Fill;
            this._mapWidthNumeric.Minimum = 1;
            this._mapWidthNumeric.Maximum = 10000;
            this._mapHeightNumeric.Minimum = 1;
            this._mapHeightNumeric.Maximum = 10000;
            this._mapStartXNumeric.Maximum = 10000;
            this._mapStartYNumeric.Maximum = 10000;
            this._mapButtonsPanel.Controls.Add(this._addMapButton);
            this._mapButtonsPanel.Controls.Add(this._updateMapButton);
            this._mapButtonsPanel.Controls.Add(this._removeMapButton);
            this._mapButtonsPanel.Dock = DockStyle.Fill;
            this._addMapButton.Text = "Добавить карту";
            this._updateMapButton.Text = "Обновить карту";
            this._removeMapButton.Text = "Удалить карту";
            ConfigureActionButton(this._addMapButton);
            ConfigureActionButton(this._updateMapButton);
            ConfigureActionButton(this._removeMapButton);
        }

        private void ConfigureTileEditor()
        {
            this._tileEditorLayout.ColumnCount = 2;
            this._tileEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            this._tileEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._tileEditorLayout.Dock = DockStyle.Fill;
            this._tileEditorLayout.Padding = new Padding(8);
            this._tileEditorLayout.RowCount = 6;
            this._tileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._tileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._tileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._tileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._tileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._tileEditorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._tileEditorLayout.Controls.Add(this._tileIdLabel, 0, 0);
            this._tileEditorLayout.Controls.Add(this._tileIdTextBox, 1, 0);
            this._tileEditorLayout.Controls.Add(this._tileNameLabel, 0, 1);
            this._tileEditorLayout.Controls.Add(this._tileNameTextBox, 1, 1);
            this._tileEditorLayout.Controls.Add(this._tileWalkableCheckBox, 1, 2);
            this._tileEditorLayout.Controls.Add(this._tileMovementCostLabel, 0, 3);
            this._tileEditorLayout.Controls.Add(this._tileMovementCostNumeric, 1, 3);
            this._tileEditorLayout.Controls.Add(this._tileAssetIdLabel, 0, 4);
            this._tileEditorLayout.Controls.Add(this._tileAssetIdTextBox, 1, 4);
            this._tileEditorLayout.Controls.Add(this._tileButtonsPanel, 0, 5);
            this._tileEditorLayout.SetColumnSpan(this._tileButtonsPanel, 2);
            this._tileIdLabel.Text = "Id";
            this._tileNameLabel.Text = "Name";
            this._tileMovementCostLabel.Text = "Movement cost";
            this._tileAssetIdLabel.Text = "AssetId";
            ConfigureEditorLabel(this._tileIdLabel);
            ConfigureEditorLabel(this._tileNameLabel);
            ConfigureEditorLabel(this._tileMovementCostLabel);
            ConfigureEditorLabel(this._tileAssetIdLabel);
            this._tileIdTextBox.Dock = DockStyle.Fill;
            this._tileNameTextBox.Dock = DockStyle.Fill;
            this._tileAssetIdTextBox.Dock = DockStyle.Fill;
            this._tileWalkableCheckBox.Text = "Walkable";
            this._tileWalkableCheckBox.Dock = DockStyle.Fill;
            this._tileMovementCostNumeric.DecimalPlaces = 2;
            this._tileMovementCostNumeric.Minimum = 0;
            this._tileMovementCostNumeric.Maximum = 10000;
            this._tileMovementCostNumeric.Increment = 0.1M;
            this._tileButtonsPanel.Controls.Add(this._addTileButton);
            this._tileButtonsPanel.Controls.Add(this._updateTileButton);
            this._tileButtonsPanel.Controls.Add(this._removeTileButton);
            this._tileButtonsPanel.Dock = DockStyle.Fill;
            this._addTileButton.Text = "Добавить тайл";
            this._updateTileButton.Text = "Обновить тайл";
            this._removeTileButton.Text = "Удалить тайл";
            ConfigureActionButton(this._addTileButton);
            ConfigureActionButton(this._updateTileButton);
            ConfigureActionButton(this._removeTileButton);
        }

        private void ConfigureEntityEditor()
        {
            this._entityEditorLayout.ColumnCount = 2;
            this._entityEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            this._entityEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._entityEditorLayout.Dock = DockStyle.Fill;
            this._entityEditorLayout.Padding = new Padding(8);
            this._entityEditorLayout.RowCount = 5;
            this._entityEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._entityEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._entityEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._entityEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            this._entityEditorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._entityEditorLayout.Controls.Add(this._entityIdLabel, 0, 0);
            this._entityEditorLayout.Controls.Add(this._entityIdTextBox, 1, 0);
            this._entityEditorLayout.Controls.Add(this._entityNameLabel, 0, 1);
            this._entityEditorLayout.Controls.Add(this._entityNameTextBox, 1, 1);
            this._entityEditorLayout.Controls.Add(this._entityAssetIdLabel, 0, 2);
            this._entityEditorLayout.Controls.Add(this._entityAssetIdTextBox, 1, 2);
            this._entityEditorLayout.Controls.Add(this._entityComponentsCountLabel, 1, 3);
            this._entityEditorLayout.Controls.Add(this._entityButtonsPanel, 0, 4);
            this._entityEditorLayout.SetColumnSpan(this._entityButtonsPanel, 2);
            this._entityIdLabel.Text = "Id";
            this._entityNameLabel.Text = "Name";
            this._entityAssetIdLabel.Text = "AssetId";
            ConfigureEditorLabel(this._entityIdLabel);
            ConfigureEditorLabel(this._entityNameLabel);
            ConfigureEditorLabel(this._entityAssetIdLabel);
            this._entityIdTextBox.Dock = DockStyle.Fill;
            this._entityNameTextBox.Dock = DockStyle.Fill;
            this._entityAssetIdTextBox.Dock = DockStyle.Fill;
            this._entityComponentsCountLabel.Dock = DockStyle.Fill;
            this._entityComponentsCountLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._entityComponentsCountLabel.Text = "Components: 0";
            this._entityButtonsPanel.Controls.Add(this._addEntityButton);
            this._entityButtonsPanel.Controls.Add(this._updateEntityButton);
            this._entityButtonsPanel.Controls.Add(this._removeEntityButton);
            this._entityButtonsPanel.Dock = DockStyle.Fill;
            this._addEntityButton.Text = "Добавить entity prototype";
            this._updateEntityButton.Text = "Обновить entity prototype";
            this._removeEntityButton.Text = "Удалить entity prototype";
            this._addEntityButton.Size = new Size(190, 30);
            this._updateEntityButton.Size = new Size(200, 30);
            this._removeEntityButton.Size = new Size(190, 30);
        }

        private static void ConfigureEditorLabel(Label label)
        {
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static void ConfigureActionButton(Button button)
        {
            button.Size = new Size(130, 30);
            button.UseVisualStyleBackColor = true;
        }
    }
}
