using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.StrictLlmEvaluation;

namespace LLMGameCreator.WinForms.Pages;

public sealed class StrictLlmEvaluationPageControl : UserControl, IEditorPage
{
    private readonly IAppSettingsRepository? _settingsRepository;
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog? _contractCatalog;
    private readonly GeneratorPlanStrictLlmArtifactGenerationArtifactReader? _auditReader;
    private readonly GeneratorPlanStrictLlmEvaluationService? _evaluationService;
    private readonly GeneratorPlanStrictLlmEvaluationArtifactReader? _evaluationReader;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly StrictLlmEvaluationPresenter _presenter = new();

    private StrictLlmEvaluationViewState _currentViewState = new();
    private CancellationTokenSource? _currentOperationCts;
    private bool _applyingState;

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _inputLayout = new();
    private readonly RadioButton _latestAuditModeButton = new();
    private readonly RadioButton _batchModeButton = new();
    private readonly ComboBox _profileComboBox = new();
    private readonly CheckedListBox _contractList = new();
    private readonly NumericUpDown _iterationsInput = new();
    private readonly NumericUpDown _maxTokensInput = new();
    private readonly NumericUpDown _temperatureInput = new();
    private readonly NumericUpDown _maxRepairAttemptsInput = new();
    private readonly CheckBox _repairCheckBox = new();
    private readonly CheckBox _stageCheckBox = new();
    private readonly TextBox _extraBriefTextBox = new();
    private readonly Button _loadAuditButton = new();
    private readonly Button _evaluateAuditButton = new();
    private readonly Button _runBatchButton = new();
    private readonly Button _loadEvaluationButton = new();
    private readonly Button _copyReportButton = new();
    private readonly Button _copyJsonButton = new();
    private readonly TextBox _statusTextBox = new();
    private readonly SplitContainer _splitContainer = new();
    private readonly TableLayoutPanel _leftLayout = new();
    private readonly TextBox _summaryTextBox = new();
    private readonly DataGridView _contractGrid = new();
    private readonly DataGridView _diagnosticGrid = new();
    private readonly DataGridView _sampleGrid = new();
    private readonly TabControl _textTabs = new();
    private readonly TextBox _latestAuditTextBox = new();
    private readonly TextBox _reportTextBox = new();
    private readonly TextBox _jsonTextBox = new();

    public StrictLlmEvaluationPageControl()
    {
        BuildLayout();
        SetRuntimeUnavailable();
    }

    public StrictLlmEvaluationPageControl(
        IAppSettingsRepository settingsRepository,
        GeneratorPlanStrictLlmArtifactContractCatalog contractCatalog,
        GeneratorPlanStrictLlmArtifactGenerationArtifactReader auditReader,
        GeneratorPlanStrictLlmEvaluationService evaluationService,
        GeneratorPlanStrictLlmEvaluationArtifactReader evaluationReader,
        IDesignDatabaseInitializer databaseInitializer,
        ICurrentGamePackageService currentGamePackageService)
    {
        _settingsRepository = settingsRepository;
        _contractCatalog = contractCatalog;
        _auditReader = auditReader;
        _evaluationService = evaluationService;
        _evaluationReader = evaluationReader;
        _databaseInitializer = databaseInitializer;
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
        WireEvents();
        ApplyViewState(_currentViewState);
    }

    public string Id => "strict_llm_evaluation";
    public string Title => "LLM Evaluation";
    public int SortOrder => 38;
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
        _rootLayout.RowCount = 3;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 196F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildInputLayout();
        BuildSplitLayout();

        ConfigureTextBox(_statusTextBox, true);
        _rootLayout.Controls.Add(_inputLayout, 0, 0);
        _rootLayout.Controls.Add(_statusTextBox, 0, 1);
        _rootLayout.Controls.Add(_splitContainer, 0, 2);
        Controls.Add(_rootLayout);

        Name = nameof(StrictLlmEvaluationPageControl);
        Size = new Size(1180, 780);
        ResumeLayout(false);
    }

    private void BuildInputLayout()
    {
        _inputLayout.ColumnCount = 8;
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
        _inputLayout.Dock = DockStyle.Fill;
        _inputLayout.RowCount = 6;
        for (var i = 0; i < 6; i++)
        {
            _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        }

        _latestAuditModeButton.Text = "Latest audit";
        _latestAuditModeButton.Checked = true;
        _latestAuditModeButton.Dock = DockStyle.Fill;
        _batchModeButton.Text = "Batch";
        _batchModeButton.Dock = DockStyle.Fill;
        _profileComboBox.Dock = DockStyle.Fill;
        _profileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _contractList.Dock = DockStyle.Fill;
        _contractList.CheckOnClick = true;
        _contractList.DisplayMember = nameof(StrictLlmEvaluationContractOption.DisplayName);
        ConfigureNumber(_iterationsInput, 1, 10, 1, 0);
        ConfigureNumber(_maxTokensInput, 256, 12000, 4000, 0);
        ConfigureNumber(_temperatureInput, 0, 1, 0.2M, 2);
        _temperatureInput.Increment = 0.05M;
        ConfigureNumber(_maxRepairAttemptsInput, 0, 2, 1, 0);
        _repairCheckBox.Text = "Enable repair";
        _repairCheckBox.Checked = true;
        _repairCheckBox.Dock = DockStyle.Fill;
        _stageCheckBox.Text = "Stage valid";
        _stageCheckBox.Dock = DockStyle.Fill;
        _extraBriefTextBox.Dock = DockStyle.Fill;
        _extraBriefTextBox.Multiline = true;
        _extraBriefTextBox.ScrollBars = ScrollBars.Vertical;

        AddLabel(0, 0, "Mode");
        _inputLayout.Controls.Add(_latestAuditModeButton, 1, 0);
        _inputLayout.Controls.Add(_batchModeButton, 2, 0);
        AddLabel(1, 0, "LLM profile");
        _inputLayout.Controls.Add(_profileComboBox, 1, 1);
        AddLabel(2, 0, "Iterations");
        _inputLayout.Controls.Add(_iterationsInput, 1, 2);
        AddLabel(3, 0, "Max tokens");
        _inputLayout.Controls.Add(_maxTokensInput, 1, 3);
        AddLabel(4, 0, "Temperature");
        _inputLayout.Controls.Add(_temperatureInput, 1, 4);
        AddLabel(5, 0, "Repair attempts");
        _inputLayout.Controls.Add(_maxRepairAttemptsInput, 1, 5);

        AddLabel(0, 2, "Contracts");
        _inputLayout.Controls.Add(_contractList, 3, 0);
        _inputLayout.SetRowSpan(_contractList, 4);
        _inputLayout.Controls.Add(_repairCheckBox, 3, 4);
        _inputLayout.Controls.Add(_stageCheckBox, 3, 5);

        AddLabel(4, 2, "Extra brief");
        _inputLayout.Controls.Add(_extraBriefTextBox, 4, 4);
        _inputLayout.SetColumnSpan(_extraBriefTextBox, 4);
        _inputLayout.SetRowSpan(_extraBriefTextBox, 2);

        ConfigureButton(_loadAuditButton, "Load latest audit");
        ConfigureButton(_evaluateAuditButton, "Evaluate latest audit");
        ConfigureButton(_runBatchButton, "Run evaluation batch");
        ConfigureButton(_loadEvaluationButton, "Load latest evaluation");
        ConfigureButton(_copyReportButton, "Copy report");
        ConfigureButton(_copyJsonButton, "Copy evaluation JSON");
        _inputLayout.Controls.Add(_loadAuditButton, 4, 0);
        _inputLayout.Controls.Add(_evaluateAuditButton, 5, 0);
        _inputLayout.Controls.Add(_runBatchButton, 6, 0);
        _inputLayout.Controls.Add(_loadEvaluationButton, 7, 0);
        _inputLayout.Controls.Add(_copyReportButton, 6, 1);
        _inputLayout.Controls.Add(_copyJsonButton, 7, 1);
    }

    private void BuildSplitLayout()
    {
        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.Orientation = Orientation.Vertical;
        _splitContainer.SplitterDistance = 650;

        _leftLayout.ColumnCount = 1;
        _leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _leftLayout.Dock = DockStyle.Fill;
        _leftLayout.RowCount = 4;
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 148F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
        _leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));

        ConfigureTextBox(_summaryTextBox, true);
        BuildContractGrid();
        BuildDiagnosticGrid();
        BuildSampleGrid();
        BuildTextTabs();

        _leftLayout.Controls.Add(_summaryTextBox, 0, 0);
        _leftLayout.Controls.Add(_contractGrid, 0, 1);
        _leftLayout.Controls.Add(_diagnosticGrid, 0, 2);
        _leftLayout.Controls.Add(_sampleGrid, 0, 3);

        _splitContainer.Panel1.Controls.Add(_leftLayout);
        _splitContainer.Panel2.Controls.Add(_textTabs);
    }

    private void BuildContractGrid()
    {
        ConfigureGrid(_contractGrid);
        _contractGrid.Columns.Add("ContractId", "ContractId");
        _contractGrid.Columns.Add("Runs", "Runs");
        _contractGrid.Columns.Add("InitialPass", "InitialPass");
        _contractGrid.Columns.Add("RepairPass", "RepairPass");
        _contractGrid.Columns.Add("Failed", "Failed");
        _contractGrid.Columns.Add("ValidArtifacts", "ValidArtifacts");
        _contractGrid.Columns.Add("AverageAttempts", "AvgAttempts");
        _contractGrid.Columns.Add("TopDiagnosticCodes", "TopDiagnostics");
    }

    private void BuildDiagnosticGrid()
    {
        ConfigureGrid(_diagnosticGrid);
        _diagnosticGrid.Columns.Add("Severity", "Severity");
        _diagnosticGrid.Columns.Add("Code", "Code");
        _diagnosticGrid.Columns.Add("ContractId", "ContractId");
        _diagnosticGrid.Columns.Add("Target", "Target");
        _diagnosticGrid.Columns.Add("Count", "Count");
        _diagnosticGrid.Columns.Add("ExampleMessage", "Example");
    }

    private void BuildSampleGrid()
    {
        ConfigureGrid(_sampleGrid);
        _sampleGrid.Columns.Add("ContractId", "ContractId");
        _sampleGrid.Columns.Add("ArtifactId", "ArtifactId");
        _sampleGrid.Columns.Add("Valid", "Valid");
        _sampleGrid.Columns.Add("Repaired", "Repaired");
        _sampleGrid.Columns.Add("ContentExcerpt", "Content");
        _sampleGrid.Columns.Add("DiagnosticExcerpt", "Diagnostics");
    }

    private void BuildTextTabs()
    {
        _textTabs.Dock = DockStyle.Fill;
        ConfigureTextBox(_latestAuditTextBox, true);
        ConfigureTextBox(_reportTextBox, true);
        ConfigureTextBox(_jsonTextBox, true);
        _latestAuditTextBox.ScrollBars = ScrollBars.Both;
        _reportTextBox.ScrollBars = ScrollBars.Both;
        _jsonTextBox.ScrollBars = ScrollBars.Both;
        _reportTextBox.WordWrap = false;
        _jsonTextBox.WordWrap = false;

        var auditTab = new TabPage("Latest audit") { Padding = new Padding(3) };
        var reportTab = new TabPage("Report") { Padding = new Padding(3) };
        var jsonTab = new TabPage("Evaluation JSON") { Padding = new Padding(3) };
        auditTab.Controls.Add(_latestAuditTextBox);
        reportTab.Controls.Add(_reportTextBox);
        jsonTab.Controls.Add(_jsonTextBox);
        _textTabs.TabPages.Add(auditTab);
        _textTabs.TabPages.Add(reportTab);
        _textTabs.TabPages.Add(jsonTab);
    }

    private void WireEvents()
    {
        _latestAuditModeButton.CheckedChanged += (_, _) => ApplyModeFromControls();
        _batchModeButton.CheckedChanged += (_, _) => ApplyModeFromControls();
        _profileComboBox.SelectedIndexChanged += (_, _) => ApplyControlChanges();
        _contractList.ItemCheck += (_, _) => BeginInvoke(new Action(ApplyControlChanges));
        _iterationsInput.ValueChanged += (_, _) => ApplyControlChanges();
        _maxTokensInput.ValueChanged += (_, _) => ApplyControlChanges();
        _temperatureInput.ValueChanged += (_, _) => ApplyControlChanges();
        _maxRepairAttemptsInput.ValueChanged += (_, _) => ApplyControlChanges();
        _repairCheckBox.CheckedChanged += (_, _) => ApplyControlChanges();
        _stageCheckBox.CheckedChanged += (_, _) => ApplyControlChanges();
        _extraBriefTextBox.TextChanged += (_, _) => ApplyControlChanges();
        _loadAuditButton.Click += async (_, _) => await LoadLatestAuditAsync().ConfigureAwait(true);
        _evaluateAuditButton.Click += async (_, _) => await EvaluateLatestAuditAsync().ConfigureAwait(true);
        _runBatchButton.Click += async (_, _) => await RunBatchAsync().ConfigureAwait(true);
        _loadEvaluationButton.Click += async (_, _) => await LoadLatestEvaluationAsync().ConfigureAwait(true);
        _copyReportButton.Click += (_, _) => TryCopyText(_reportTextBox.Text);
        _copyJsonButton.Click += (_, _) => TryCopyText(_jsonTextBox.Text);
    }

    private async Task LoadSettingsAsync()
    {
        if (_settingsRepository == null || _contractCatalog == null)
        {
            SetStatusMessage("Strict LLM evaluation services are not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            var settings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromSettings(ReadControlsToState(), settings, _contractCatalog.ListContracts()));
        }).ConfigureAwait(true);
    }

    private async Task LoadLatestAuditAsync()
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

    private async Task EvaluateLatestAuditAsync()
    {
        if (_evaluationService == null)
        {
            SetStatusMessage("Strict LLM evaluation service is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var result = await _evaluationService.EvaluateLatestAuditAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromEvaluationResult(ReadControlsToState(), result));
        }).ConfigureAwait(true);
    }

    private async Task RunBatchAsync()
    {
        if (_evaluationService == null)
        {
            SetStatusMessage("Strict LLM evaluation service is not available.");
            return;
        }

        var state = ReadControlsToState();
        if (!state.CanRunBatch)
        {
            SetStatusMessage("Select batch mode, an LLM profile and at least one contract.");
            return;
        }

        SetStatusMessage($"Expected max LLM calls: {state.ExpectedMaxLlmCalls}");
        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var result = await _evaluationService.RunEvaluationBatchAsync(_presenter.BuildRequest(ReadControlsToState()), cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromEvaluationResult(ReadControlsToState(), result));
        }).ConfigureAwait(true);
    }

    private async Task LoadLatestEvaluationAsync()
    {
        if (_evaluationReader == null)
        {
            SetStatusMessage("Strict LLM evaluation reader is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var latest = await _evaluationReader.ReadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLatestEvaluation(ReadControlsToState(), latest));
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

    private void ApplyModeFromControls()
    {
        if (_applyingState)
        {
            return;
        }

        ApplyViewState(_presenter.SetMode(ReadControlsToState(), _latestAuditModeButton.Checked));
    }

    private void ApplyControlChanges()
    {
        if (_applyingState)
        {
            return;
        }

        var state = ReadControlsToState();
        ApplyViewState(state with
        {
            Status = state.LatestAuditOnly
                ? "Latest-audit mode selected. No LLM call will be made."
                : $"Expected max LLM calls: {state.ExpectedMaxLlmCalls}"
        });
    }

    private StrictLlmEvaluationViewState ReadControlsToState()
    {
        if (_applyingState)
        {
            return _currentViewState;
        }

        return _currentViewState with
        {
            LatestAuditOnly = _latestAuditModeButton.Checked,
            SelectedProfileId = _profileComboBox.SelectedItem is StrictLlmEvaluationProfileOption profile ? profile.Id : _profileComboBox.SelectedValue?.ToString() ?? string.Empty,
            SelectedContractIds = _contractList.CheckedItems.OfType<StrictLlmEvaluationContractOption>().Select(contract => contract.Id).Take(4).ToList(),
            IterationsPerContract = (int)_iterationsInput.Value,
            MaxTokens = (int)_maxTokensInput.Value,
            Temperature = (double)_temperatureInput.Value,
            EnableRepairAttempt = _repairCheckBox.Checked,
            MaxRepairAttempts = (int)_maxRepairAttemptsInput.Value,
            StageValidArtifactsForReview = _stageCheckBox.Checked,
            ExtraBrief = _extraBriefTextBox.Text.Trim()
        };
    }

    private void ApplyViewState(StrictLlmEvaluationViewState state)
    {
        _applyingState = true;
        try
        {
            _currentViewState = state;
            _latestAuditModeButton.Checked = state.LatestAuditOnly;
            _batchModeButton.Checked = !state.LatestAuditOnly;
            _profileComboBox.DisplayMember = nameof(StrictLlmEvaluationProfileOption.DisplayName);
            _profileComboBox.ValueMember = nameof(StrictLlmEvaluationProfileOption.Id);
            _profileComboBox.DataSource = state.Profiles.ToList();
            if (!string.IsNullOrWhiteSpace(state.SelectedProfileId))
            {
                _profileComboBox.SelectedValue = state.SelectedProfileId;
            }

            SetContracts(state.Contracts, state.SelectedContractIds);
            _iterationsInput.Value = Math.Clamp(state.IterationsPerContract, (int)_iterationsInput.Minimum, (int)_iterationsInput.Maximum);
            _maxTokensInput.Value = Math.Clamp(state.MaxTokens, (int)_maxTokensInput.Minimum, (int)_maxTokensInput.Maximum);
            _temperatureInput.Value = Math.Clamp((decimal)state.Temperature, _temperatureInput.Minimum, _temperatureInput.Maximum);
            _maxRepairAttemptsInput.Value = Math.Clamp(state.MaxRepairAttempts, (int)_maxRepairAttemptsInput.Minimum, (int)_maxRepairAttemptsInput.Maximum);
            _repairCheckBox.Checked = state.EnableRepairAttempt;
            _stageCheckBox.Checked = state.StageValidArtifactsForReview;
            _extraBriefTextBox.Text = state.ExtraBrief;
            _statusTextBox.Text = state.Status;
            _latestAuditTextBox.Text = state.LatestAuditSummary;
            _summaryTextBox.Text = state.SummaryText;
            _reportTextBox.Text = state.ReportMarkdown;
            _jsonTextBox.Text = state.EvaluationJson;
            SetContractRows(state.ContractRows);
            SetDiagnosticRows(state.DiagnosticRows);
            SetSampleRows(state.SampleRows);
        }
        finally
        {
            _applyingState = false;
        }

        RefreshActions();
    }

    private void SetContracts(IReadOnlyList<StrictLlmEvaluationContractOption> contracts, IReadOnlyList<string> selectedIds)
    {
        var selected = selectedIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _contractList.Items.Clear();
        foreach (var contract in contracts)
        {
            _contractList.Items.Add(contract, selected.Contains(contract.Id));
        }
    }

    private void SetContractRows(IReadOnlyList<StrictLlmEvaluationContractRow> rows)
    {
        _contractGrid.Rows.Clear();
        foreach (var row in rows)
        {
            _contractGrid.Rows.Add(row.ContractId, row.Runs, row.InitialPass, row.RepairPass, row.Failed, row.ValidArtifacts, row.AverageAttempts, row.TopDiagnosticCodes);
        }
    }

    private void SetDiagnosticRows(IReadOnlyList<StrictLlmEvaluationDiagnosticRow> rows)
    {
        _diagnosticGrid.Rows.Clear();
        foreach (var row in rows)
        {
            _diagnosticGrid.Rows.Add(row.Severity, row.Code, row.ContractId, row.Target, row.Count, row.ExampleMessage);
        }
    }

    private void SetSampleRows(IReadOnlyList<StrictLlmEvaluationSampleRow> rows)
    {
        _sampleGrid.Rows.Clear();
        foreach (var row in rows)
        {
            _sampleGrid.Rows.Add(row.ContractId, row.ArtifactId, row.Valid, row.Repaired, row.ContentExcerpt, row.DiagnosticExcerpt);
        }
    }

    private void SetBusy(bool busy)
    {
        _latestAuditModeButton.Enabled = !busy;
        _batchModeButton.Enabled = !busy;
        _profileComboBox.Enabled = !busy;
        _contractList.Enabled = !busy;
        _iterationsInput.Enabled = !busy;
        _maxTokensInput.Enabled = !busy;
        _temperatureInput.Enabled = !busy;
        _maxRepairAttemptsInput.Enabled = !busy;
        _repairCheckBox.Enabled = !busy;
        _stageCheckBox.Enabled = !busy;
        _extraBriefTextBox.Enabled = !busy;
        _loadAuditButton.Enabled = !busy;
        _evaluateAuditButton.Enabled = !busy;
        _runBatchButton.Enabled = !busy;
        _loadEvaluationButton.Enabled = !busy;
        RefreshActions();
    }

    private void RefreshActions()
    {
        var busy = _currentOperationCts != null;
        var state = ReadControlsToState();
        _evaluateAuditButton.Enabled = !busy;
        _runBatchButton.Enabled = !busy && state.CanRunBatch;
        _copyReportButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_reportTextBox.Text);
        _copyJsonButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_jsonTextBox.Text);
    }

    private void SetRuntimeUnavailable()
    {
        _latestAuditModeButton.Enabled = false;
        _batchModeButton.Enabled = false;
        _profileComboBox.Enabled = false;
        _contractList.Enabled = false;
        _iterationsInput.Enabled = false;
        _maxTokensInput.Enabled = false;
        _temperatureInput.Enabled = false;
        _maxRepairAttemptsInput.Enabled = false;
        _repairCheckBox.Enabled = false;
        _stageCheckBox.Enabled = false;
        _extraBriefTextBox.Enabled = false;
        _loadAuditButton.Enabled = false;
        _evaluateAuditButton.Enabled = false;
        _runBatchButton.Enabled = false;
        _loadEvaluationButton.Enabled = false;
        _copyReportButton.Enabled = false;
        _copyJsonButton.Enabled = false;
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

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
    }

    private static void ConfigureNumber(NumericUpDown input, decimal minimum, decimal maximum, decimal value, int decimalPlaces)
    {
        input.Dock = DockStyle.Fill;
        input.Minimum = minimum;
        input.Maximum = maximum;
        input.DecimalPlaces = decimalPlaces;
        input.Value = value;
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
