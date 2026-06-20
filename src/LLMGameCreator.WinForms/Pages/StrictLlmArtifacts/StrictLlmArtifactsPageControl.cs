using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;

namespace LLMGameCreator.WinForms.Pages;

public sealed class StrictLlmArtifactsPageControl : UserControl, IEditorPage
{
    private readonly IAppSettingsRepository? _settingsRepository;
    private readonly GeneratorPlanCapabilitySelectionArtifactReader? _selectionReader;
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog? _contractCatalog;
    private readonly GeneratorPlanStrictLlmArtifactGenerationService? _generationService;
    private readonly GeneratorPlanStrictLlmArtifactGenerationArtifactReader? _auditReader;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly StrictLlmArtifactsPresenter _presenter = new();

    private StrictLlmArtifactsViewState _currentViewState = new();
    private CancellationTokenSource? _currentOperationCts;
    private bool _applyingState;

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _inputLayout = new();
    private readonly ComboBox _profileComboBox = new();
    private readonly ComboBox _batchPresetComboBox = new();
    private readonly CheckedListBox _contractList = new();
    private readonly NumericUpDown _maxTokensInput = new();
    private readonly NumericUpDown _temperatureInput = new();
    private readonly CheckBox _repairCheckBox = new();
    private readonly CheckBox _stageCheckBox = new();
    private readonly TextBox _extraBriefTextBox = new();
    private readonly Button _loadSelectionButton = new();
    private readonly Button _previewPromptButton = new();
    private readonly Button _generateButton = new();
    private readonly Button _loadAuditButton = new();
    private readonly Button _copyPromptButton = new();
    private readonly Button _copyResultButton = new();
    private readonly SplitContainer _splitContainer = new();
    private readonly TableLayoutPanel _leftLayout = new();
    private readonly TextBox _sourceTextBox = new();
    private readonly TextBox _statusTextBox = new();
    private readonly DataGridView _artifactGrid = new();
    private readonly DataGridView _diagnosticsGrid = new();
    private readonly TabControl _textTabs = new();
    private readonly TextBox _promptTextBox = new();
    private readonly TextBox _resultTextBox = new();

    public StrictLlmArtifactsPageControl()
    {
        BuildLayout();
        SetRuntimeUnavailable();
    }

    public StrictLlmArtifactsPageControl(
        IAppSettingsRepository settingsRepository,
        GeneratorPlanCapabilitySelectionArtifactReader selectionReader,
        GeneratorPlanStrictLlmArtifactContractCatalog contractCatalog,
        GeneratorPlanStrictLlmArtifactGenerationService generationService,
        GeneratorPlanStrictLlmArtifactGenerationArtifactReader auditReader,
        IDesignDatabaseInitializer databaseInitializer,
        ICurrentGamePackageService currentGamePackageService)
    {
        _settingsRepository = settingsRepository;
        _selectionReader = selectionReader;
        _contractCatalog = contractCatalog;
        _generationService = generationService;
        _auditReader = auditReader;
        _databaseInitializer = databaseInitializer;
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
        WireEvents();
        ApplyViewState(_currentViewState);
    }

    public string Id => "strict_llm_artifacts";
    public string Title => "LLM Artifacts";
    public int SortOrder => 37;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        if (_currentOperationCts == null && _settingsRepository != null && _contractCatalog != null && _currentViewState.Profiles.Count == 0)
        {
            _ = LoadSettingsAsync();
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
        _rootLayout.RowCount = 2;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 164F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildInputLayout();
        BuildSplitLayout();

        _rootLayout.Controls.Add(_inputLayout, 0, 0);
        _rootLayout.Controls.Add(_splitContainer, 0, 1);
        Controls.Add(_rootLayout);

        Name = nameof(StrictLlmArtifactsPageControl);
        Size = new Size(1180, 780);
        ResumeLayout(false);
    }

    private void BuildInputLayout()
    {
        _inputLayout.ColumnCount = 8;
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
        _inputLayout.Dock = DockStyle.Fill;
        _inputLayout.RowCount = 5;
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        _profileComboBox.Dock = DockStyle.Fill;
        _profileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _batchPresetComboBox.Dock = DockStyle.Fill;
        _batchPresetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _contractList.Dock = DockStyle.Fill;
        _contractList.CheckOnClick = true;
        _contractList.DisplayMember = nameof(StrictLlmContractOption.DisplayName);
        _maxTokensInput.Dock = DockStyle.Fill;
        _maxTokensInput.Minimum = 256;
        _maxTokensInput.Maximum = 12000;
        _maxTokensInput.Value = 4000;
        _temperatureInput.Dock = DockStyle.Fill;
        _temperatureInput.DecimalPlaces = 2;
        _temperatureInput.Increment = 0.05M;
        _temperatureInput.Minimum = 0;
        _temperatureInput.Maximum = 1;
        _temperatureInput.Value = 0.2M;
        _repairCheckBox.Text = "One repair";
        _repairCheckBox.Checked = true;
        _repairCheckBox.Dock = DockStyle.Fill;
        _stageCheckBox.Text = "Stage for review";
        _stageCheckBox.Checked = true;
        _stageCheckBox.Dock = DockStyle.Fill;
        _extraBriefTextBox.Dock = DockStyle.Fill;
        _extraBriefTextBox.Multiline = true;
        _extraBriefTextBox.ScrollBars = ScrollBars.Vertical;

        AddLabel(0, 0, "LLM profile");
        _inputLayout.Controls.Add(_profileComboBox, 1, 0);
        AddLabel(0, 2, "Contracts");
        _inputLayout.Controls.Add(_contractList, 3, 0);
        _inputLayout.SetRowSpan(_contractList, 4);
        AddLabel(1, 0, "Max tokens");
        _inputLayout.Controls.Add(_maxTokensInput, 1, 1);
        AddLabel(2, 0, "Temperature");
        _inputLayout.Controls.Add(_temperatureInput, 1, 2);
        _inputLayout.Controls.Add(_repairCheckBox, 1, 3);
        _inputLayout.Controls.Add(_stageCheckBox, 1, 4);
        AddLabel(4, 2, "Extra brief");
        _inputLayout.Controls.Add(_extraBriefTextBox, 3, 4);
        _inputLayout.SetColumnSpan(_extraBriefTextBox, 5);

        ConfigureButton(_loadSelectionButton, "Load selection");
        ConfigureButton(_previewPromptButton, "Preview prompt");
        ConfigureButton(_generateButton, "Generate");
        ConfigureButton(_loadAuditButton, "Load audit");
        ConfigureButton(_copyPromptButton, "Copy prompt");
        ConfigureButton(_copyResultButton, "Copy result JSON");
        _inputLayout.Controls.Add(_loadSelectionButton, 4, 0);
        _inputLayout.Controls.Add(_previewPromptButton, 5, 0);
        _inputLayout.Controls.Add(_generateButton, 6, 0);
        _inputLayout.Controls.Add(_loadAuditButton, 7, 0);
        _inputLayout.Controls.Add(_copyPromptButton, 6, 1);
        _inputLayout.Controls.Add(_copyResultButton, 7, 1);
        AddLabel(2, 4, "Batch preset");
        _inputLayout.Controls.Add(_batchPresetComboBox, 5, 2);
        _inputLayout.SetColumnSpan(_batchPresetComboBox, 3);
    }

    private void BuildSplitLayout()
    {
        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.Orientation = Orientation.Vertical;
        _splitContainer.SplitterDistance = 560;

        _leftLayout.ColumnCount = 1;
        _leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _leftLayout.Dock = DockStyle.Fill;
        _leftLayout.RowCount = 5;
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));

        ConfigureTextBox(_sourceTextBox, true);
        ConfigureTextBox(_statusTextBox, true);
        BuildArtifactGrid();
        BuildDiagnosticsGrid();
        BuildTextTabs();

        _leftLayout.Controls.Add(_sourceTextBox, 0, 0);
        _leftLayout.Controls.Add(_statusTextBox, 0, 1);
        _leftLayout.Controls.Add(_artifactGrid, 0, 2);
        _leftLayout.Controls.Add(_diagnosticsGrid, 0, 3);

        _splitContainer.Panel1.Controls.Add(_leftLayout);
        _splitContainer.Panel2.Controls.Add(_textTabs);
    }

    private void BuildArtifactGrid()
    {
        _artifactGrid.AllowUserToAddRows = false;
        _artifactGrid.AllowUserToDeleteRows = false;
        _artifactGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _artifactGrid.Dock = DockStyle.Fill;
        _artifactGrid.ReadOnly = true;
        _artifactGrid.RowHeadersVisible = false;
        _artifactGrid.Columns.Add("ArtifactId", "ArtifactId");
        _artifactGrid.Columns.Add("Kind", "Kind");
        _artifactGrid.Columns.Add("Contract", "Contract");
        _artifactGrid.Columns.Add("Valid", "Valid");
        _artifactGrid.Columns.Add("Repaired", "Repaired");
        _artifactGrid.Columns.Add("RequiresApproval", "RequiresApproval");
    }

    private void BuildDiagnosticsGrid()
    {
        _diagnosticsGrid.AllowUserToAddRows = false;
        _diagnosticsGrid.AllowUserToDeleteRows = false;
        _diagnosticsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _diagnosticsGrid.Dock = DockStyle.Fill;
        _diagnosticsGrid.ReadOnly = true;
        _diagnosticsGrid.RowHeadersVisible = false;
        _diagnosticsGrid.Columns.Add("Severity", "Severity");
        _diagnosticsGrid.Columns.Add("Code", "Code");
        _diagnosticsGrid.Columns.Add("ContractId", "ContractId");
        _diagnosticsGrid.Columns.Add("Target", "Target");
        _diagnosticsGrid.Columns.Add("Message", "Message");
    }

    private void BuildTextTabs()
    {
        _textTabs.Dock = DockStyle.Fill;
        ConfigureTextBox(_promptTextBox, true);
        ConfigureTextBox(_resultTextBox, true);
        _promptTextBox.ScrollBars = ScrollBars.Both;
        _promptTextBox.WordWrap = false;
        _resultTextBox.ScrollBars = ScrollBars.Both;
        _resultTextBox.WordWrap = false;
        var promptTab = new TabPage("Prompt") { Padding = new Padding(3) };
        var resultTab = new TabPage("Audit JSON") { Padding = new Padding(3) };
        promptTab.Controls.Add(_promptTextBox);
        resultTab.Controls.Add(_resultTextBox);
        _textTabs.TabPages.Add(promptTab);
        _textTabs.TabPages.Add(resultTab);
    }

    private void WireEvents()
    {
        _loadSelectionButton.Click += async (_, _) => await LoadLatestSelectionAsync().ConfigureAwait(true);
        _previewPromptButton.Click += async (_, _) => await PreviewPromptAsync().ConfigureAwait(true);
        _generateButton.Click += async (_, _) => await GenerateAsync().ConfigureAwait(true);
        _loadAuditButton.Click += async (_, _) => await LoadAuditAsync().ConfigureAwait(true);
        _copyPromptButton.Click += (_, _) => TryCopyText(_promptTextBox.Text);
        _copyResultButton.Click += (_, _) => TryCopyText(_resultTextBox.Text);
        _batchPresetComboBox.SelectionChangeCommitted += (_, _) => ApplySelectedBatchPreset();
    }

    private async Task LoadSettingsAsync()
    {
        if (_settingsRepository == null || _contractCatalog == null)
        {
            SetStatusMessage("Strict LLM services are not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            var settings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromSettings(
                ReadControlsToState(),
                settings,
                _contractCatalog.ListContracts(),
                _contractCatalog.ListBatchPresets()));
        }).ConfigureAwait(true);
    }

    private void ApplySelectedBatchPreset()
    {
        if (_applyingState || _contractCatalog == null || _batchPresetComboBox.SelectedItem is not StrictLlmBatchPresetOption preset)
        {
            return;
        }

        ApplyViewState(_presenter.ApplyBatchPreset(ReadControlsToState(), preset.Id, _contractCatalog));
    }

    private async Task LoadLatestSelectionAsync()
    {
        if (_selectionReader == null)
        {
            SetStatusMessage("Capability selection reader is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var latest = await _selectionReader.ReadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLatestSelection(ReadControlsToState(), latest));
        }).ConfigureAwait(true);
    }

    private async Task PreviewPromptAsync()
    {
        if (_generationService == null)
        {
            SetStatusMessage("Strict LLM generation service is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var preview = await _generationService.PreviewPromptAsync(_presenter.BuildRequest(ReadControlsToState()), cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromPreview(ReadControlsToState(), preview));
        }).ConfigureAwait(true);
    }

    private async Task GenerateAsync()
    {
        if (_generationService == null)
        {
            SetStatusMessage("Strict LLM generation service is not available.");
            return;
        }

        var state = ReadControlsToState();
        if (!state.CanGenerate)
        {
            SetStatusMessage("Select an LLM profile, load latest capability selection, and select at least one contract.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var result = await _generationService.GenerateAsync(_presenter.BuildRequest(ReadControlsToState()), cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromGenerationResult(ReadControlsToState(), result));
        }).ConfigureAwait(true);
    }

    private async Task LoadAuditAsync()
    {
        if (_auditReader == null)
        {
            SetStatusMessage("Strict LLM audit reader is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var latest = await _auditReader.ReadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLatestAudit(ReadControlsToState(), latest));
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
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException or UnauthorizedAccessException)
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

    private StrictLlmArtifactsViewState ReadControlsToState()
    {
        if (_applyingState)
        {
            return _currentViewState;
        }

        return _currentViewState with
        {
            SelectedProfileId = _profileComboBox.SelectedItem is StrictLlmProfileOption profile ? profile.Id : _profileComboBox.SelectedValue?.ToString() ?? string.Empty,
            SelectedBatchPresetId = _batchPresetComboBox.SelectedItem is StrictLlmBatchPresetOption preset ? preset.Id : string.Empty,
            SelectedContractIds = _contractList.CheckedItems.OfType<StrictLlmContractOption>().Select(contract => contract.Id).ToList(),
            MaxTokens = (int)_maxTokensInput.Value,
            Temperature = (double)_temperatureInput.Value,
            EnableRepairAttempt = _repairCheckBox.Checked,
            StageForReview = _stageCheckBox.Checked,
            ExtraBrief = _extraBriefTextBox.Text.Trim()
        };
    }

    private void ApplyViewState(StrictLlmArtifactsViewState state)
    {
        _applyingState = true;
        try
        {
            _currentViewState = state;
            _profileComboBox.DisplayMember = nameof(StrictLlmProfileOption.DisplayName);
            _profileComboBox.ValueMember = nameof(StrictLlmProfileOption.Id);
            _profileComboBox.DataSource = state.Profiles.ToList();
            if (!string.IsNullOrWhiteSpace(state.SelectedProfileId))
            {
                _profileComboBox.SelectedValue = state.SelectedProfileId;
            }

            _batchPresetComboBox.DisplayMember = nameof(StrictLlmBatchPresetOption.DisplayName);
            _batchPresetComboBox.ValueMember = nameof(StrictLlmBatchPresetOption.Id);
            _batchPresetComboBox.DataSource = state.BatchPresets.ToList();
            _batchPresetComboBox.SelectedValue = state.SelectedBatchPresetId;

            SetContracts(state.Contracts, state.SelectedContractIds);
            _maxTokensInput.Value = Math.Clamp(state.MaxTokens, (int)_maxTokensInput.Minimum, (int)_maxTokensInput.Maximum);
            _temperatureInput.Value = Math.Clamp((decimal)state.Temperature, _temperatureInput.Minimum, _temperatureInput.Maximum);
            _repairCheckBox.Checked = state.EnableRepairAttempt;
            _stageCheckBox.Checked = state.StageForReview;
            _extraBriefTextBox.Text = state.ExtraBrief;
            _sourceTextBox.Text = state.SourceSummary;
            _statusTextBox.Text = state.Status;
            _promptTextBox.Text = state.PromptPreview;
            _resultTextBox.Text = state.ResultJson;
            SetArtifacts(state.ArtifactRows);
            SetDiagnostics(state.DiagnosticRows);
        }
        finally
        {
            _applyingState = false;
        }

        RefreshActions();
    }

    private void SetContracts(IReadOnlyList<StrictLlmContractOption> contracts, IReadOnlyList<string> selectedIds)
    {
        var selected = selectedIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _contractList.Items.Clear();
        foreach (var contract in contracts)
        {
            _contractList.Items.Add(contract, selected.Contains(contract.Id));
        }
    }

    private void SetArtifacts(IReadOnlyList<StrictLlmArtifactRow> rows)
    {
        _artifactGrid.Rows.Clear();
        foreach (var row in rows)
        {
            _artifactGrid.Rows.Add(row.ArtifactId, row.Kind, row.Contract, row.Valid, row.Repaired, row.RequiresApproval);
        }
    }

    private void SetDiagnostics(IReadOnlyList<StrictLlmDiagnosticRow> rows)
    {
        _diagnosticsGrid.Rows.Clear();
        foreach (var row in rows)
        {
            _diagnosticsGrid.Rows.Add(row.Severity, row.Code, row.ContractId, row.Target, row.Message);
        }
    }

    private void SetBusy(bool busy)
    {
        _profileComboBox.Enabled = !busy;
        _batchPresetComboBox.Enabled = !busy;
        _contractList.Enabled = !busy;
        _maxTokensInput.Enabled = !busy;
        _temperatureInput.Enabled = !busy;
        _repairCheckBox.Enabled = !busy;
        _stageCheckBox.Enabled = !busy;
        _extraBriefTextBox.Enabled = !busy;
        _loadSelectionButton.Enabled = !busy;
        _previewPromptButton.Enabled = !busy;
        _generateButton.Enabled = !busy;
        _loadAuditButton.Enabled = !busy;
        RefreshActions();
    }

    private void RefreshActions()
    {
        var busy = _currentOperationCts != null;
        _generateButton.Enabled = !busy && ReadControlsToState().CanGenerate;
        _copyPromptButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_promptTextBox.Text);
        _copyResultButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_resultTextBox.Text);
    }

    private void SetRuntimeUnavailable()
    {
        _profileComboBox.Enabled = false;
        _batchPresetComboBox.Enabled = false;
        _contractList.Enabled = false;
        _maxTokensInput.Enabled = false;
        _temperatureInput.Enabled = false;
        _repairCheckBox.Enabled = false;
        _stageCheckBox.Enabled = false;
        _extraBriefTextBox.Enabled = false;
        _loadSelectionButton.Enabled = false;
        _previewPromptButton.Enabled = false;
        _generateButton.Enabled = false;
        _loadAuditButton.Enabled = false;
        _copyPromptButton.Enabled = false;
        _copyResultButton.Enabled = false;
        SetStatusMessage("Runtime services are not available.");
    }

    private void SetStatusMessage(string message)
    {
        _statusTextBox.Text = message;
    }

    private void AddLabel(int row, int column, string text)
    {
        _inputLayout.Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, column, row);
    }

    private static void ConfigureButton(Button button, string text)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.UseVisualStyleBackColor = true;
    }

    private static void ConfigureTextBox(TextBox textBox, bool readOnly)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ReadOnly = readOnly;
        textBox.ScrollBars = ScrollBars.Vertical;
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
