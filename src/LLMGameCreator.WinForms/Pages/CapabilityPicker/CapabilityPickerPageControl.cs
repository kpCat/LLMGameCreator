using System.Text.Json;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.CapabilityPicker;

namespace LLMGameCreator.WinForms.Pages;

public sealed class CapabilityPickerPageControl : UserControl, IEditorPage
{
    private readonly GeneratorPlanCapabilitySelectionService? _selectionService;
    private readonly GeneratorPlanCapabilitySelectionArtifactService? _artifactService;
    private readonly GeneratorPlanCapabilitySelectionArtifactReader? _artifactReader;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly CapabilityPickerPresenter _presenter = new();

    private CapabilityPickerViewState _currentViewState = new() { Status = "Atlas not loaded." };
    private CancellationTokenSource? _currentOperationCts;
    private bool _applyingState;
    private bool _atlasLoaded;

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _inputLayout = new();
    private readonly TextBox _atlasRootTextBox = new();
    private readonly Button _browseAtlasButton = new();
    private readonly Button _loadAtlasButton = new();
    private readonly TextBox _titleTextBox = new();
    private readonly TextBox _purposeTextBox = new();
    private readonly TableLayoutPanel _variantLayout = new();
    private readonly ComboBox _presentationComboBox = new();
    private readonly ComboBox _worldComboBox = new();
    private readonly ComboBox _actorComboBox = new();
    private readonly ComboBox _inventoryComboBox = new();
    private readonly ComboBox _combatComboBox = new();
    private readonly ComboBox _progressionComboBox = new();
    private readonly ComboBox _pathfindingComboBox = new();
    private readonly ComboBox _npcComboBox = new();
    private readonly ComboBox _runtimeTargetComboBox = new();
    private readonly SplitContainer _splitContainer = new();
    private readonly TableLayoutPanel _featurePanel = new();
    private readonly Label _featureBundleLabel = new();
    private readonly Label _helpLabel = new();
    private readonly CheckedListBox _featureBundleList = new();
    private readonly TextBox _helpTextBox = new();
    private readonly TableLayoutPanel _resultLayout = new();
    private readonly FlowLayoutPanel _actionPanel = new();
    private readonly Button _buildButton = new();
    private readonly Button _saveButton = new();
    private readonly Button _loadLatestButton = new();
    private readonly Button _copyJsonButton = new();
    private readonly TextBox _statusTextBox = new();
    private readonly TextBox _summaryTextBox = new();
    private readonly TextBox _artifactContractsTextBox = new();
    private readonly TextBox _validatorsTextBox = new();
    private readonly TextBox _runtimeTargetsTextBox = new();
    private readonly TextBox _promptContextsTextBox = new();
    private readonly TextBox _gapsTextBox = new();
    private readonly DataGridView _diagnosticsGrid = new();

    public CapabilityPickerPageControl()
    {
        BuildLayout();
        SetRuntimeUnavailable();
    }

    public CapabilityPickerPageControl(
        GeneratorPlanCapabilitySelectionService selectionService,
        GeneratorPlanCapabilitySelectionArtifactService artifactService,
        GeneratorPlanCapabilitySelectionArtifactReader artifactReader,
        IDesignDatabaseInitializer databaseInitializer,
        ICurrentGamePackageService currentGamePackageService)
    {
        _selectionService = selectionService;
        _artifactService = artifactService;
        _artifactReader = artifactReader;
        _databaseInitializer = databaseInitializer;
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
        WireEvents();
        _atlasRootTextBox.Text = _selectionService.DiscoverAtlasRoot();
        ApplyViewState(_currentViewState with { AtlasRootPath = _atlasRootTextBox.Text });
    }

    public string Id => "capability_picker";
    public string Title => "Capability Picker";
    public int SortOrder => 36;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        if (!_atlasLoaded && _selectionService != null && _currentOperationCts == null)
        {
            _ = LoadAtlasAsync();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentOperationCts?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        SuspendLayout();

        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Padding = new Padding(8);
        _rootLayout.RowCount = 3;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 156F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildInputLayout();
        BuildVariantLayout();
        BuildSplitLayout();

        _rootLayout.Controls.Add(_inputLayout, 0, 0);
        _rootLayout.Controls.Add(_variantLayout, 0, 1);
        _rootLayout.Controls.Add(_splitContainer, 0, 2);

        Controls.Add(_rootLayout);
        Name = nameof(CapabilityPickerPageControl);
        Size = new Size(1180, 780);
        ResumeLayout(false);
    }

    private void BuildInputLayout()
    {
        _inputLayout.ColumnCount = 4;
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
        _inputLayout.Dock = DockStyle.Fill;
        _inputLayout.RowCount = 3;
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        AddInputRow(0, "Atlas root:", _atlasRootTextBox);
        ConfigureButton(_browseAtlasButton, "Browse...", 90);
        ConfigureButton(_loadAtlasButton, "Load atlas", 96);
        _inputLayout.Controls.Add(_browseAtlasButton, 2, 0);
        _inputLayout.Controls.Add(_loadAtlasButton, 3, 0);

        AddInputRow(1, "Game title:", _titleTextBox);
        _inputLayout.SetColumnSpan(_titleTextBox, 3);

        AddInputRow(2, "Purpose:", _purposeTextBox);
        _purposeTextBox.Multiline = true;
        _inputLayout.SetColumnSpan(_purposeTextBox, 3);
    }

    private void BuildVariantLayout()
    {
        _variantLayout.ColumnCount = 6;
        for (var i = 0; i < 6; i++)
        {
            _variantLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
        }

        _variantLayout.Dock = DockStyle.Fill;
        _variantLayout.RowCount = 6;
        for (var i = 0; i < 6; i++)
        {
            _variantLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        }

        AddCombo(0, 0, "\u0424\u043e\u0440\u043c\u0430 \u0438\u0433\u0440\u044b", _presentationComboBox);
        AddCombo(0, 2, "\u0421\u0442\u0440\u0443\u043a\u0442\u0443\u0440\u0430 \u043c\u0438\u0440\u0430", _worldComboBox);
        AddCombo(0, 4, "\u041c\u043e\u0434\u0435\u043b\u044c \u043f\u0435\u0440\u0441\u043e\u043d\u0430\u0436\u0435\u0439", _actorComboBox);
        AddCombo(2, 0, "\u0418\u043d\u0432\u0435\u043d\u0442\u0430\u0440\u044c", _inventoryComboBox);
        AddCombo(2, 2, "\u0411\u043e\u0439", _combatComboBox);
        AddCombo(2, 4, "\u041f\u0440\u043e\u0433\u0440\u0435\u0441\u0441\u0438\u044f", _progressionComboBox);
        AddCombo(4, 0, "\u041d\u0430\u0432\u0438\u0433\u0430\u0446\u0438\u044f", _pathfindingComboBox);
        AddCombo(4, 2, "\u041f\u043e\u0432\u0435\u0434\u0435\u043d\u0438\u0435 NPC", _npcComboBox);
        AddCombo(4, 4, "\u0426\u0435\u043b\u044c \u0440\u0430\u043d\u0442\u0430\u0439\u043c\u0430", _runtimeTargetComboBox);
    }

    private void BuildSplitLayout()
    {
        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.Orientation = Orientation.Vertical;
        _splitContainer.Panel1MinSize = 420;
        _splitContainer.Panel2MinSize = 520;
        _splitContainer.SplitterDistance = 500;

        _featureBundleList.CheckOnClick = true;
        _featureBundleList.DisplayMember = nameof(CapabilityPickerFeatureBundleViewModel.DisplayName);
        _featureBundleList.Dock = DockStyle.Fill;
        _featureBundleList.HorizontalScrollbar = true;
        ConfigureReadOnly(_helpTextBox, true);
        ConfigureSectionLabel(_featureBundleLabel, "\u0424\u0438\u0447\u0438 / \u043c\u043e\u0434\u0443\u043b\u0438 / \u0431\u0443\u0434\u0443\u0449\u0438\u0435 \u0438\u0434\u0435\u0438");
        ConfigureSectionLabel(_helpLabel, "\u0421\u043f\u0440\u0430\u0432\u043a\u0430 / \u0434\u0435\u0442\u0430\u043b\u0438");

        _featurePanel.ColumnCount = 1;
        _featurePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _featurePanel.Dock = DockStyle.Fill;
        _featurePanel.RowCount = 4;
        _featurePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _featurePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
        _featurePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _featurePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
        _featurePanel.Controls.Add(_featureBundleLabel, 0, 0);
        _featurePanel.Controls.Add(_featureBundleList, 0, 1);
        _featurePanel.Controls.Add(_helpLabel, 0, 2);
        _featurePanel.Controls.Add(_helpTextBox, 0, 3);

        BuildResultLayout();
        _splitContainer.Panel1.Controls.Add(_featurePanel);
        _splitContainer.Panel2.Controls.Add(_resultLayout);
    }

    private void BuildResultLayout()
    {
        _resultLayout.ColumnCount = 1;
        _resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _resultLayout.Dock = DockStyle.Fill;
        _resultLayout.RowCount = 9;
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildActionPanel();
        ConfigureReadOnly(_statusTextBox, false);
        ConfigureReadOnly(_summaryTextBox, true);
        ConfigureReadOnly(_artifactContractsTextBox, true);
        ConfigureReadOnly(_validatorsTextBox, true);
        ConfigureReadOnly(_runtimeTargetsTextBox, true);
        ConfigureReadOnly(_promptContextsTextBox, true);
        ConfigureReadOnly(_gapsTextBox, true);
        BuildDiagnosticsGrid();

        _resultLayout.Controls.Add(_actionPanel, 0, 0);
        _resultLayout.Controls.Add(_statusTextBox, 0, 1);
        _resultLayout.Controls.Add(_summaryTextBox, 0, 2);
        _resultLayout.Controls.Add(_artifactContractsTextBox, 0, 3);
        _resultLayout.Controls.Add(_validatorsTextBox, 0, 4);
        _resultLayout.Controls.Add(_runtimeTargetsTextBox, 0, 5);
        _resultLayout.Controls.Add(_promptContextsTextBox, 0, 6);
        _resultLayout.Controls.Add(_gapsTextBox, 0, 7);
        _resultLayout.Controls.Add(_diagnosticsGrid, 0, 8);
    }

    private void BuildActionPanel()
    {
        _actionPanel.Dock = DockStyle.Fill;
        _actionPanel.FlowDirection = FlowDirection.LeftToRight;
        _actionPanel.WrapContents = true;
        ConfigureButton(_buildButton, "\u0421\u043e\u0431\u0440\u0430\u0442\u044c \u0432\u044b\u0431\u043e\u0440", 132);
        ConfigureButton(_saveButton, "\u0421\u043e\u0445\u0440\u0430\u043d\u0438\u0442\u044c \u0432\u044b\u0431\u043e\u0440", 146);
        ConfigureButton(_loadLatestButton, "\u0417\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044c \u0432\u044b\u0431\u043e\u0440", 146);
        ConfigureButton(_copyJsonButton, "\u041a\u043e\u043f\u0438\u0440\u043e\u0432\u0430\u0442\u044c JSON", 142);
        _actionPanel.Controls.Add(_buildButton);
        _actionPanel.Controls.Add(_saveButton);
        _actionPanel.Controls.Add(_loadLatestButton);
        _actionPanel.Controls.Add(_copyJsonButton);
    }

    private void BuildDiagnosticsGrid()
    {
        _diagnosticsGrid.AllowUserToAddRows = false;
        _diagnosticsGrid.AllowUserToDeleteRows = false;
        _diagnosticsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _diagnosticsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _diagnosticsGrid.Dock = DockStyle.Fill;
        _diagnosticsGrid.MultiSelect = false;
        _diagnosticsGrid.ReadOnly = true;
        _diagnosticsGrid.RowHeadersVisible = false;
        _diagnosticsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _diagnosticsGrid.Columns.Add("Severity", "Severity");
        _diagnosticsGrid.Columns.Add("Category", "Category");
        _diagnosticsGrid.Columns.Add("Code", "Code");
        _diagnosticsGrid.Columns.Add("Target", "Target");
        _diagnosticsGrid.Columns.Add("Message", "Message");
    }

    private void WireEvents()
    {
        _browseAtlasButton.Click += (_, _) => BrowseAtlasRoot();
        _loadAtlasButton.Click += async (_, _) => await LoadAtlasAsync().ConfigureAwait(true);
        _buildButton.Click += async (_, _) => await BuildSelectionAsync().ConfigureAwait(true);
        _saveButton.Click += async (_, _) => await SaveLatestSelectionAsync().ConfigureAwait(true);
        _loadLatestButton.Click += async (_, _) => await LoadLatestSelectionAsync().ConfigureAwait(true);
        _copyJsonButton.Click += (_, _) => TryCopyText(_currentViewState.SelectionJson);
        _featureBundleList.SelectedIndexChanged += (_, _) => UpdateHelpFromSelection();
        _featureBundleList.ItemCheck += FeatureBundleListItemCheck;
        _diagnosticsGrid.SelectionChanged += (_, _) => UpdateHelpFromDiagnosticSelection();
        foreach (var comboBox in new[] { _presentationComboBox, _worldComboBox, _actorComboBox, _inventoryComboBox, _combatComboBox, _progressionComboBox, _pathfindingComboBox, _npcComboBox, _runtimeTargetComboBox })
        {
            comboBox.SelectedIndexChanged += (_, _) => UpdateHelpFromSelection();
        }
    }

    private void BrowseAtlasRoot()
    {
        using var dialog = new FolderBrowserDialog
        {
            SelectedPath = Directory.Exists(_atlasRootTextBox.Text) ? _atlasRootTextBox.Text : string.Empty
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _atlasRootTextBox.Text = dialog.SelectedPath;
        }
    }

    private async Task LoadAtlasAsync()
    {
        if (_selectionService == null)
        {
            SetStatusMessage("Capability selection service is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            var state = ReadControlsToState();
            var atlas = await _selectionService.LoadAtlasAsync(state.AtlasRootPath, cancellationToken).ConfigureAwait(true);
            _atlasLoaded = !atlas.Diagnostics.Any(diagnostic => diagnostic.Severity == GeneratorPlanPreviewDiagnosticSeverity.Error);
            ApplyViewState(_presenter.FromAtlas(state, atlas));
        }).ConfigureAwait(true);
    }

    private async Task BuildSelectionAsync()
    {
        if (_selectionService == null)
        {
            SetStatusMessage("Capability selection service is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            var state = ReadControlsToState();
            var result = await _selectionService.BuildSelectionAsync(_presenter.BuildRequest(state), cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromSelectionResult(state, result));
        }).ConfigureAwait(true);
    }

    private async Task SaveLatestSelectionAsync()
    {
        if (_artifactService == null)
        {
            SetStatusMessage("Capability selection artifact service is not available.");
            return;
        }

        if (_currentViewState.CurrentResult == null)
        {
            SetStatusMessage("Build a selection before saving.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            await _artifactService.SaveAsync(_currentViewState.CurrentResult, cancellationToken).ConfigureAwait(true);
            SetStatusMessage("Latest capability selection saved.");
        }).ConfigureAwait(true);
    }

    private async Task LoadLatestSelectionAsync()
    {
        if (_artifactReader == null)
        {
            SetStatusMessage("Capability selection artifact reader is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var latest = await _artifactReader.ReadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLatestSelection(ReadControlsToState(), latest));
        }).ConfigureAwait(true);
    }

    private async Task RunBusyAsync(Func<CancellationToken, Task> action)
    {
        if (_currentOperationCts != null)
        {
            return;
        }

        _currentOperationCts = new CancellationTokenSource();
        try
        {
            SetBusy(true);
            await action(_currentOperationCts.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            SetStatusMessage("Operation canceled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or JsonException)
        {
            SetStatusMessage(ex.Message);
        }
        finally
        {
            _currentOperationCts.Dispose();
            _currentOperationCts = null;
            SetBusy(false);
        }
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_databaseInitializer == null)
        {
            return;
        }

        await _databaseInitializer.InitializeAsync(ResolveDatabasePath(), cancellationToken).ConfigureAwait(true);
    }

    private string ResolveDatabasePath()
    {
        if (!string.IsNullOrWhiteSpace(_currentGamePackageService?.CurrentFolder))
        {
            return Path.Combine(_currentGamePackageService.CurrentFolder, ".llmgc", "design.db");
        }

        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator");
        return Path.Combine(appData, "design.db");
    }

    private CapabilityPickerViewState ReadControlsToState()
    {
        if (_applyingState)
        {
            return _currentViewState;
        }

        return _currentViewState with
        {
            AtlasRootPath = _atlasRootTextBox.Text.Trim(),
            Title = _titleTextBox.Text.Trim(),
            Purpose = _purposeTextBox.Text.Trim(),
            PresentationModeId = SelectedId(_presentationComboBox),
            WorldTopologyId = SelectedId(_worldComboBox),
            ActorModelId = SelectedId(_actorComboBox),
            InventoryModelId = SelectedId(_inventoryComboBox),
            CombatModelId = SelectedId(_combatComboBox),
            ProgressionModelId = SelectedId(_progressionComboBox),
            PathfindingProfileId = SelectedId(_pathfindingComboBox),
            NpcBehaviorModelId = SelectedId(_npcComboBox),
            RuntimeTargetId = SelectedId(_runtimeTargetComboBox),
            SelectedFeatureBundleIds = _featureBundleList.CheckedItems
                .OfType<CapabilityPickerFeatureBundleViewModel>()
                .Select(bundle => bundle.Id)
                .ToList()
        };
    }

    private void ApplyViewState(CapabilityPickerViewState state)
    {
        _applyingState = true;
        try
        {
            _currentViewState = state;
            _atlasRootTextBox.Text = state.AtlasRootPath;
            _titleTextBox.Text = state.Title;
            _purposeTextBox.Text = state.Purpose;
            SetCombo(_presentationComboBox, state.PresentationModes, state.PresentationModeId);
            SetCombo(_worldComboBox, state.WorldTopologies, state.WorldTopologyId);
            SetCombo(_actorComboBox, state.ActorModels, state.ActorModelId);
            SetCombo(_inventoryComboBox, state.InventoryModels, state.InventoryModelId);
            SetCombo(_combatComboBox, state.CombatModels, state.CombatModelId);
            SetCombo(_progressionComboBox, state.ProgressionModels, state.ProgressionModelId);
            SetCombo(_pathfindingComboBox, state.PathfindingProfiles, state.PathfindingProfileId);
            SetCombo(_npcComboBox, state.NpcBehaviorModels, state.NpcBehaviorModelId);
            SetCombo(_runtimeTargetComboBox, state.RuntimeTargets, state.RuntimeTargetId);
            SetFeatureBundles(state.FeatureBundles, state.SelectedFeatureBundleIds);
            _statusTextBox.Text = state.Status;
            _summaryTextBox.Text = state.Summary;
            _artifactContractsTextBox.Text = FormatList("Artifact contracts", state.ResolvedArtifactContracts);
            _validatorsTextBox.Text = FormatList("Validators", state.ResolvedValidators);
            _runtimeTargetsTextBox.Text = FormatList("Runtime targets", state.ResolvedRuntimeTargets);
            _promptContextsTextBox.Text = FormatList("Prompt context templates", state.ResolvedPromptContextTemplates);
            _gapsTextBox.Text = FormatList("Capability gaps / future modules", state.CapabilityGaps);
            SetDiagnostics(state.Diagnostics);
            _helpTextBox.Text = BuildCurrentHelpText();
        }
        finally
        {
            _applyingState = false;
        }

        RefreshActions();
    }

    private void SetDiagnostics(IReadOnlyList<CapabilityPickerDiagnosticRow> diagnostics)
    {
        _diagnosticsGrid.Rows.Clear();
        foreach (var diagnostic in diagnostics)
        {
            _diagnosticsGrid.Rows.Add(diagnostic.Severity, $"{diagnostic.CategoryDisplayName} ({diagnostic.Category})", diagnostic.Code, diagnostic.Target, diagnostic.Message);
        }
    }

    private void SetFeatureBundles(IReadOnlyList<CapabilityPickerFeatureBundleViewModel> bundles, IReadOnlyList<string> selectedIds)
    {
        var selected = selectedIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _featureBundleList.Items.Clear();
        foreach (var bundle in bundles)
        {
            _featureBundleList.Items.Add(bundle, bundle.IsRequiredTechnicalBase || selected.Contains(bundle.Id));
        }
    }

    private static void SetCombo(ComboBox comboBox, IReadOnlyList<CapabilityPickerOptionViewModel> options, string selectedId)
    {
        comboBox.DisplayMember = nameof(CapabilityPickerOptionViewModel.DisplayName);
        comboBox.ValueMember = nameof(CapabilityPickerOptionViewModel.Id);
        comboBox.DataSource = options.ToList();
        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            comboBox.SelectedValue = selectedId;
        }
    }

    private static string SelectedId(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is CapabilityPickerOptionViewModel option)
        {
            return option.Id;
        }

        return comboBox.SelectedValue?.ToString() ?? string.Empty;
    }

    private void SetBusy(bool busy)
    {
        _browseAtlasButton.Enabled = !busy;
        _loadAtlasButton.Enabled = !busy;
        _buildButton.Enabled = !busy;
        _loadLatestButton.Enabled = !busy;
        _saveButton.Enabled = !busy && _currentViewState.CanSave;
        _copyJsonButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_currentViewState.SelectionJson);
        _featureBundleList.Enabled = !busy;
        _helpTextBox.Enabled = !busy;
        _presentationComboBox.Enabled = !busy;
        _worldComboBox.Enabled = !busy;
        _actorComboBox.Enabled = !busy;
        _inventoryComboBox.Enabled = !busy;
        _combatComboBox.Enabled = !busy;
        _progressionComboBox.Enabled = !busy;
        _pathfindingComboBox.Enabled = !busy;
        _npcComboBox.Enabled = !busy;
        _runtimeTargetComboBox.Enabled = !busy;
    }

    private void RefreshActions()
    {
        var busy = _currentOperationCts != null;
        _saveButton.Enabled = !busy && _currentViewState.CanSave;
        _copyJsonButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_currentViewState.SelectionJson);
    }

    private void SetRuntimeUnavailable()
    {
        _atlasRootTextBox.Enabled = false;
        _browseAtlasButton.Enabled = false;
        _loadAtlasButton.Enabled = false;
        _buildButton.Enabled = false;
        _saveButton.Enabled = false;
        _loadLatestButton.Enabled = false;
        _copyJsonButton.Enabled = false;
        _featureBundleList.Enabled = false;
        _helpTextBox.Enabled = false;
        SetStatusMessage("Runtime services are not available.");
    }

    private void SetStatusMessage(string message)
    {
        _statusTextBox.Text = message;
        _summaryTextBox.Text = message;
    }

    private void AddInputRow(int row, string labelText, TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        _inputLayout.Controls.Add(BuildLabel(labelText), 0, row);
        _inputLayout.Controls.Add(textBox, 1, row);
    }

    private void AddCombo(int row, int column, string labelText, ComboBox comboBox)
    {
        var label = BuildLabel(labelText);
        comboBox.Dock = DockStyle.Fill;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _variantLayout.Controls.Add(label, column, row);
        _variantLayout.Controls.Add(comboBox, column, row + 1);
        _variantLayout.SetColumnSpan(label, 2);
        _variantLayout.SetColumnSpan(comboBox, 2);
    }

    private static Label BuildLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static void ConfigureButton(Button button, string text, int width)
    {
        button.Text = text;
        button.Width = width;
        button.Height = 30;
        button.Margin = new Padding(3);
        button.UseVisualStyleBackColor = true;
    }

    private static void ConfigureSectionLabel(Label label, string text)
    {
        label.Dock = DockStyle.Fill;
        label.Font = new Font(label.Font, FontStyle.Bold);
        label.Text = text;
        label.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static void ConfigureReadOnly(TextBox textBox, bool multiline)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.ReadOnly = true;
        textBox.Multiline = multiline;
        textBox.ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None;
        textBox.WordWrap = true;
    }

    private static string FormatList(string title, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return title + ": none";
        }

        return title + ":" + Environment.NewLine + string.Join(Environment.NewLine, values);
    }

    private void UpdateHelpFromSelection()
    {
        if (_applyingState)
        {
            return;
        }

        _helpTextBox.Text = BuildCurrentHelpText();
    }

    private void FeatureBundleListItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (!_applyingState &&
            e.Index >= 0 &&
            _featureBundleList.Items[e.Index] is CapabilityPickerFeatureBundleViewModel { IsRequiredTechnicalBase: true } &&
            e.NewValue == CheckState.Unchecked)
        {
            e.NewValue = CheckState.Checked;
            SetStatusMessage("\u041e\u0431\u044f\u0437\u0430\u0442\u0435\u043b\u044c\u043d\u0430\u044f \u0442\u0435\u0445\u043d\u0438\u0447\u0435\u0441\u043a\u0430\u044f \u0431\u0430\u0437\u0430 \u0433\u0435\u043d\u0435\u0440\u0430\u0446\u0438\u0438 \u0443\u0436\u0435 \u0432\u043a\u043b\u044e\u0447\u0435\u043d\u0430.");
        }

        BeginInvoke(UpdateHelpFromSelection);
    }

    private void UpdateHelpFromDiagnosticSelection()
    {
        if (_applyingState || _diagnosticsGrid.CurrentRow == null)
        {
            return;
        }

        var category = _diagnosticsGrid.CurrentRow.Cells["Category"].Value?.ToString() ?? string.Empty;
        var code = _diagnosticsGrid.CurrentRow.Cells["Code"].Value?.ToString() ?? string.Empty;
        var target = _diagnosticsGrid.CurrentRow.Cells["Target"].Value?.ToString() ?? string.Empty;
        var message = _diagnosticsGrid.CurrentRow.Cells["Message"].Value?.ToString() ?? string.Empty;
        _helpTextBox.Text = string.Join(Environment.NewLine, new[]
        {
            "\u0414\u0438\u0430\u0433\u043d\u043e\u0441\u0442\u0438\u043a\u0430",
            "\u041a\u0430\u0442\u0435\u0433\u043e\u0440\u0438\u044f: " + category,
            "\u041a\u043e\u0434: " + code,
            "\u0426\u0435\u043b\u044c: " + target,
            message,
            string.Empty,
            FormatHelp(FindHelp(target))
        });
    }

    private GeneratorPlanCapabilityHelpMetadata FindHelp(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return GeneratorPlanCapabilityHelpCatalog.Get(id);
        }

        foreach (var option in _currentViewState.PresentationModes
                     .Concat(_currentViewState.WorldTopologies)
                     .Concat(_currentViewState.ActorModels)
                     .Concat(_currentViewState.InventoryModels)
                     .Concat(_currentViewState.CombatModels)
                     .Concat(_currentViewState.ProgressionModels)
                     .Concat(_currentViewState.PathfindingProfiles)
                     .Concat(_currentViewState.NpcBehaviorModels)
                     .Concat(_currentViewState.RuntimeTargets))
        {
            if (string.Equals(option.Id, id, StringComparison.OrdinalIgnoreCase))
            {
                return option.Help;
            }
        }

        foreach (var bundle in _currentViewState.FeatureBundles)
        {
            if (string.Equals(bundle.Id, id, StringComparison.OrdinalIgnoreCase))
            {
                return bundle.Help;
            }
        }

        return GeneratorPlanCapabilityHelpCatalog.Get(id);
    }

    private string BuildCurrentHelpText()
    {
        if (_featureBundleList.SelectedItem is CapabilityPickerFeatureBundleViewModel bundle)
        {
            return FormatHelp(bundle.Help);
        }

        if (ActiveControl is ComboBox comboBox && comboBox.SelectedItem is CapabilityPickerOptionViewModel option)
        {
            return FormatHelp(option.Help);
        }

        foreach (var candidateComboBox in new[] { _presentationComboBox, _worldComboBox, _actorComboBox, _inventoryComboBox, _combatComboBox, _progressionComboBox, _pathfindingComboBox, _npcComboBox, _runtimeTargetComboBox })
        {
            if (candidateComboBox.Focused && candidateComboBox.SelectedItem is CapabilityPickerOptionViewModel focusedOption)
            {
                return FormatHelp(focusedOption.Help);
            }
        }

        return "\u0412\u044b\u0431\u0435\u0440\u0438 \u0432\u0430\u0440\u0438\u0430\u043d\u0442, feature bundle \u0438\u043b\u0438 \u0441\u0442\u0440\u043e\u043a\u0443 \u0434\u0438\u0430\u0433\u043d\u043e\u0441\u0442\u0438\u043a\u0438, \u0447\u0442\u043e\u0431\u044b \u0443\u0432\u0438\u0434\u0435\u0442\u044c \u0434\u0435\u0442\u0430\u043b\u0438.";
    }

    private static string FormatHelp(GeneratorPlanCapabilityHelpMetadata help)
    {
        return string.Join(Environment.NewLine, new[]
        {
            help.DisplayNameRu,
            "id: " + help.Id,
            "status: " + help.ImplementationStatus,
            string.Empty,
            help.ShortDescriptionRu,
            string.Empty,
            "\u041a\u043e\u0433\u0434\u0430 \u0432\u044b\u0431\u0438\u0440\u0430\u0442\u044c: " + EmptyAsDash(help.DetailsRu),
            "\u0427\u0442\u043e \u0434\u043e\u0431\u0430\u0432\u043b\u044f\u0435\u0442 \u0432 \u0438\u0433\u0440\u0443: " + EmptyAsDash(help.ExamplesRu),
            "\u0421 \u0447\u0435\u043c \u0445\u043e\u0440\u043e\u0448\u043e \u0441\u043e\u0447\u0435\u0442\u0430\u0435\u0442\u0441\u044f: " + EmptyAsDash(help.BestForRu),
            "\u0427\u0442\u043e \u043f\u043e\u043a\u0430 \u043d\u0435 \u0440\u0435\u0430\u043b\u0438\u0437\u0443\u0435\u0442 / \u0447\u0435\u043c \u043e\u043f\u0430\u0441\u043d\u043e: " + EmptyAsDash(help.WarningsRu)
        });
    }

    private static string EmptyAsDash(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private void TryCopyText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.ExternalException)
        {
            MessageBox.Show(this, ex.Message, "Copy failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
