using LLMGameCreator.Application.Editing;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.WinForms;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class DashboardPageControl : UserControl, IEditorPage
{
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly IPackageEditorService? _packageEditorService;
    private readonly ValidationReportFormatter _validationFormatter = new ValidationReportFormatter();
    private bool _isRefreshing;

    public DashboardPageControl()
    {
        InitializeComponent();
        SetNoPackageState("Design-time preview. Runtime services are not available in Visual Studio Designer.");
    }

    public DashboardPageControl(ICurrentGamePackageService currentGamePackageService, IPackageEditorService packageEditorService)
    {
        _currentGamePackageService = currentGamePackageService;
        _packageEditorService = packageEditorService;
        InitializeComponent();
        WireEvents();
        _currentGamePackageService.CurrentChanged += CurrentGamePackageService_CurrentChanged;
        RefreshSnapshot();
    }

    public string Id => "dashboard";
    public string Title => "Обзор";
    public int SortOrder => 0;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        RefreshSnapshot();
    }

    private void WireEvents()
    {
        _applyManifestButton.Click += (_, _) => ExecuteEditorAction(ApplyManifest);
        _addMapButton.Click += (_, _) => ExecuteEditorAction(AddMap);
        _updateMapButton.Click += (_, _) => ExecuteEditorAction(UpdateMap);
        _removeMapButton.Click += (_, _) => ExecuteEditorAction(RemoveMap);
        _addTileButton.Click += (_, _) => ExecuteEditorAction(AddTile);
        _updateTileButton.Click += (_, _) => ExecuteEditorAction(UpdateTile);
        _removeTileButton.Click += (_, _) => ExecuteEditorAction(RemoveTile);
        _addEntityButton.Click += (_, _) => ExecuteEditorAction(AddEntity);
        _updateEntityButton.Click += (_, _) => ExecuteEditorAction(UpdateEntity);
        _removeEntityButton.Click += (_, _) => ExecuteEditorAction(RemoveEntity);
        _savePackageButton.Click += async (_, _) => await SavePackageAsync();
        _validatePackageButton.Click += (_, _) => ValidatePackage();
        _mapsListView.SelectedIndexChanged += (_, _) => FillMapEditorFromSelection();
        _tilesListView.SelectedIndexChanged += (_, _) => FillTileEditorFromSelection();
        _entitiesListView.SelectedIndexChanged += (_, _) => FillEntityEditorFromSelection();
    }

    private void CurrentGamePackageService_CurrentChanged(object? sender, EventArgs e)
    {
        WinFormsUiThreadDispatcher.Post(this, RefreshSnapshot);
    }

    private void DisposeRuntime()
    {
        if (_currentGamePackageService != null)
        {
            _currentGamePackageService.CurrentChanged -= CurrentGamePackageService_CurrentChanged;
        }
    }

    private void RefreshSnapshot()
    {
        if (_packageEditorService == null)
        {
            return;
        }

        var snapshot = _packageEditorService.GetSnapshot();
        if (!snapshot.HasCurrentPackage)
        {
            SetNoPackageState("Проект игры не открыт.");
            return;
        }

        _isRefreshing = true;
        try
        {
            SetEditorEnabled(true);
            _currentFolderLabel.Text = $"Папка: {snapshot.CurrentFolder ?? string.Empty}";
            _currentPackageLabel.Text = $"{snapshot.Manifest.Title} ({snapshot.Manifest.PackageId})";

            _packageIdTextBox.Text = snapshot.Manifest.PackageId;
            _titleTextBox.Text = snapshot.Manifest.Title;
            _versionTextBox.Text = snapshot.Manifest.Version;
            _formatVersionTextBox.Text = snapshot.Manifest.FormatVersion;
            _startMapComboBox.Items.Clear();
            foreach (var map in snapshot.Maps)
            {
                _startMapComboBox.Items.Add(map.Id);
            }

            _startMapComboBox.Text = snapshot.Manifest.StartMapId;
            _descriptionTextBox.Text = snapshot.Manifest.Description ?? string.Empty;

            FillMapsList(snapshot.Maps);
            FillTilesList(snapshot.TilePrototypes);
            FillEntitiesList(snapshot.EntityPrototypes);
            FillAssetsList(snapshot.Assets);
            FillScriptsList(snapshot.Scripts);
            RefreshMapTileChoices(snapshot.TilePrototypes);
            RefreshValidationSummary();
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void SetNoPackageState(string message)
    {
        _currentFolderLabel.Text = string.Empty;
        _currentPackageLabel.Text = message;
        _validationSummaryLabel.Text = string.Empty;
        _validationOutputTextBox.Text = message;
        ClearAllLists();
        SetEditorEnabled(false);
    }

    private void SetEditorEnabled(bool enabled)
    {
        _tabs.Enabled = enabled;
        _applyManifestButton.Enabled = enabled;
        _savePackageButton.Enabled = enabled;
        _validatePackageButton.Enabled = enabled;
    }

    private void ClearAllLists()
    {
        _mapsListView.Items.Clear();
        _tilesListView.Items.Clear();
        _entitiesListView.Items.Clear();
        _assetsListView.Items.Clear();
        _scriptsListView.Items.Clear();
        _startMapComboBox.Items.Clear();
        _mapDefaultTileComboBox.Items.Clear();
    }

    private void FillMapsList(IReadOnlyList<MapEditModel> maps)
    {
        _mapsListView.Items.Clear();
        foreach (var map in maps)
        {
            var item = new ListViewItem(map.Id);
            item.SubItems.Add(map.Name);
            item.SubItems.Add($"{map.Width}x{map.Height}");
            item.SubItems.Add(map.DefaultTileId);
            item.SubItems.Add($"{map.StartX},{map.StartY}");
            item.Tag = map;
            _mapsListView.Items.Add(item);
        }
    }

    private void FillTilesList(IReadOnlyList<TilePrototypeEditModel> tiles)
    {
        _tilesListView.Items.Clear();
        foreach (var tile in tiles)
        {
            var item = new ListViewItem(tile.Id);
            item.SubItems.Add(tile.Name);
            item.SubItems.Add(tile.Walkable ? "Да" : "Нет");
            item.SubItems.Add(tile.MovementCost.ToString("0.###"));
            item.SubItems.Add(tile.AssetId ?? string.Empty);
            item.Tag = tile;
            _tilesListView.Items.Add(item);
        }
    }

    private void FillEntitiesList(IReadOnlyList<EntityPrototypeEditModel> entities)
    {
        _entitiesListView.Items.Clear();
        foreach (var entity in entities)
        {
            var item = new ListViewItem(entity.Id);
            item.SubItems.Add(entity.Name);
            item.SubItems.Add(entity.AssetId ?? string.Empty);
            item.SubItems.Add(entity.ComponentsCount.ToString());
            item.Tag = entity;
            _entitiesListView.Items.Add(item);
        }
    }

    private void FillAssetsList(IReadOnlyList<AssetSummaryModel> assets)
    {
        _assetsListView.Items.Clear();
        foreach (var asset in assets)
        {
            var item = new ListViewItem(asset.Id);
            item.SubItems.Add(asset.Type);
            item.SubItems.Add(asset.Role);
            item.SubItems.Add(asset.Path ?? string.Empty);
            item.SubItems.Add(asset.ContractId ?? string.Empty);
            _assetsListView.Items.Add(item);
        }
    }

    private void FillScriptsList(IReadOnlyList<ScriptSummaryModel> scripts)
    {
        _scriptsListView.Items.Clear();
        foreach (var script in scripts)
        {
            var item = new ListViewItem(script.Id);
            item.SubItems.Add(script.Kind);
            item.SubItems.Add(script.Path);
            item.SubItems.Add(string.Join(", ", script.EntryPoints));
            _scriptsListView.Items.Add(item);
        }
    }

    private void RefreshMapTileChoices(IReadOnlyList<TilePrototypeEditModel> tiles)
    {
        var previous = _mapDefaultTileComboBox.Text;
        _mapDefaultTileComboBox.Items.Clear();
        foreach (var tile in tiles)
        {
            _mapDefaultTileComboBox.Items.Add(tile.Id);
        }

        _mapDefaultTileComboBox.Text = previous;
    }

    private void FillMapEditorFromSelection()
    {
        if (_isRefreshing || _mapsListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_mapsListView.SelectedItems[0].Tag is not MapEditModel map)
        {
            return;
        }

        _mapIdTextBox.Text = map.Id;
        _mapNameTextBox.Text = map.Name;
        _mapWidthNumeric.Value = ClampToNumericRange(_mapWidthNumeric, map.Width);
        _mapHeightNumeric.Value = ClampToNumericRange(_mapHeightNumeric, map.Height);
        _mapDefaultTileComboBox.Text = map.DefaultTileId;
        _mapStartXNumeric.Value = ClampToNumericRange(_mapStartXNumeric, map.StartX);
        _mapStartYNumeric.Value = ClampToNumericRange(_mapStartYNumeric, map.StartY);
    }

    private void FillTileEditorFromSelection()
    {
        if (_isRefreshing || _tilesListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_tilesListView.SelectedItems[0].Tag is not TilePrototypeEditModel tile)
        {
            return;
        }

        _tileIdTextBox.Text = tile.Id;
        _tileNameTextBox.Text = tile.Name;
        _tileWalkableCheckBox.Checked = tile.Walkable;
        _tileMovementCostNumeric.Value = ClampToNumericRange(_tileMovementCostNumeric, (decimal)tile.MovementCost);
        _tileAssetIdTextBox.Text = tile.AssetId ?? string.Empty;
    }

    private void FillEntityEditorFromSelection()
    {
        if (_isRefreshing || _entitiesListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_entitiesListView.SelectedItems[0].Tag is not EntityPrototypeEditModel entity)
        {
            return;
        }

        _entityIdTextBox.Text = entity.Id;
        _entityNameTextBox.Text = entity.Name;
        _entityAssetIdTextBox.Text = entity.AssetId ?? string.Empty;
        _entityComponentsCountLabel.Text = $"Components: {entity.ComponentsCount}";
    }

    private void ApplyManifest()
    {
        RequireEditorService().UpdateManifest(new ManifestEditModel
        {
            PackageId = _packageIdTextBox.Text,
            Title = _titleTextBox.Text,
            Version = _versionTextBox.Text,
            FormatVersion = _formatVersionTextBox.Text,
            StartMapId = _startMapComboBox.Text,
            Description = _descriptionTextBox.Text
        });
    }

    private void AddMap()
    {
        RequireEditorService().AddMap(ReadMapEditor());
    }

    private void UpdateMap()
    {
        RequireEditorService().UpdateMap(ReadMapEditor());
    }

    private void RemoveMap()
    {
        RequireEditorService().RemoveMap(_mapIdTextBox.Text);
    }

    private void AddTile()
    {
        RequireEditorService().AddTilePrototype(ReadTileEditor());
    }

    private void UpdateTile()
    {
        RequireEditorService().UpdateTilePrototype(ReadTileEditor());
    }

    private void RemoveTile()
    {
        RequireEditorService().RemoveTilePrototype(_tileIdTextBox.Text);
    }

    private void AddEntity()
    {
        RequireEditorService().AddEntityPrototype(ReadEntityEditor());
    }

    private void UpdateEntity()
    {
        RequireEditorService().UpdateEntityPrototype(ReadEntityEditor());
    }

    private void RemoveEntity()
    {
        RequireEditorService().RemoveEntityPrototype(_entityIdTextBox.Text);
    }

    private MapEditModel ReadMapEditor()
    {
        return new MapEditModel
        {
            Id = _mapIdTextBox.Text,
            Name = _mapNameTextBox.Text,
            Width = (int)_mapWidthNumeric.Value,
            Height = (int)_mapHeightNumeric.Value,
            DefaultTileId = _mapDefaultTileComboBox.Text,
            StartX = (int)_mapStartXNumeric.Value,
            StartY = (int)_mapStartYNumeric.Value
        };
    }

    private TilePrototypeEditModel ReadTileEditor()
    {
        return new TilePrototypeEditModel
        {
            Id = _tileIdTextBox.Text,
            Name = _tileNameTextBox.Text,
            Walkable = _tileWalkableCheckBox.Checked,
            MovementCost = (double)_tileMovementCostNumeric.Value,
            AssetId = _tileAssetIdTextBox.Text
        };
    }

    private EntityPrototypeEditModel ReadEntityEditor()
    {
        return new EntityPrototypeEditModel
        {
            Id = _entityIdTextBox.Text,
            Name = _entityNameTextBox.Text,
            AssetId = _entityAssetIdTextBox.Text
        };
    }

    private async Task SavePackageAsync()
    {
        try
        {
            await RequireEditorService().SaveAsync(CancellationToken.None);
            RefreshSnapshot();
            MessageBox.Show(this, "Package сохранён.", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ValidatePackage()
    {
        try
        {
            var report = RequireEditorService().Validate();
            _validationOutputTextBox.Text = _validationFormatter.Format(report);
            _validationSummaryLabel.Text = GetValidationSummary(report);
            _tabs.SelectedTab = _validationTabPage;
        }
        catch (Exception ex)
        {
            _validationOutputTextBox.Text = ex.Message;
            _validationSummaryLabel.Text = "Validation недоступна";
        }
    }

    private void RefreshValidationSummary()
    {
        try
        {
            _validationSummaryLabel.Text = GetValidationSummary(RequireEditorService().Validate());
        }
        catch (Exception ex)
        {
            _validationSummaryLabel.Text = ex.Message;
        }
    }

    private void ExecuteEditorAction(Action action)
    {
        try
        {
            action();
            RefreshSnapshot();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Package editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private IPackageEditorService RequireEditorService()
    {
        return _packageEditorService ?? throw new InvalidOperationException("Package editor service is not available.");
    }

    private static string GetValidationSummary(ValidationReport report)
    {
        var errors = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Error || issue.Severity == ValidationSeverity.Critical);
        var warnings = report.Issues.Count(issue => issue.Severity == ValidationSeverity.Warning);
        return report.IsValid ? $"Validation: valid, {warnings} warnings" : $"Validation: invalid, {errors} errors, {warnings} warnings";
    }

    private static decimal ClampToNumericRange(NumericUpDown control, decimal value)
    {
        if (value < control.Minimum)
        {
            return control.Minimum;
        }

        if (value > control.Maximum)
        {
            return control.Maximum;
        }

        return value;
    }
}
