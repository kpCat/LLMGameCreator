using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.StrictLlmArtifacts;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class StrictLlmArtifactsPageControl : UserControl, IEditorPage
{
    private readonly IAppSettingsRepository? _settingsRepository;
    private readonly GeneratorPlanCapabilitySelectionArtifactReader? _selectionReader;
    private readonly GeneratorPlanStrictLlmArtifactContractCatalog? _contractCatalog;
    private readonly GeneratorPlanStrictLlmArtifactGenerationService? _generationService;
    private readonly GeneratorPlanStrictLlmArtifactGenerationArtifactReader? _auditReader;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly ContentLanguagePolicyService? _contentLanguagePolicyService;
    private readonly StrictLlmArtifactsPresenter _presenter = new();

    private StrictLlmArtifactsViewState _currentViewState = new();
    private CancellationTokenSource? _currentOperationCts;
    private bool _applyingState;

    public StrictLlmArtifactsPageControl()
    {
        InitializeComponent();
        SetRuntimeUnavailable();
    }

    public StrictLlmArtifactsPageControl(
        IAppSettingsRepository settingsRepository,
        GeneratorPlanCapabilitySelectionArtifactReader selectionReader,
        GeneratorPlanStrictLlmArtifactContractCatalog contractCatalog,
        GeneratorPlanStrictLlmArtifactGenerationService generationService,
        GeneratorPlanStrictLlmArtifactGenerationArtifactReader auditReader,
        IDesignDatabaseInitializer databaseInitializer,
        ICurrentGamePackageService currentGamePackageService,
        ContentLanguagePolicyService contentLanguagePolicyService)
    {
        _settingsRepository = settingsRepository;
        _selectionReader = selectionReader;
        _contractCatalog = contractCatalog;
        _generationService = generationService;
        _auditReader = auditReader;
        _databaseInitializer = databaseInitializer;
        _currentGamePackageService = currentGamePackageService;
        _contentLanguagePolicyService = contentLanguagePolicyService;
        InitializeComponent();
        WireEvents();
        ApplyViewState(_currentViewState);
    }

    public string Id => "strict_llm_artifacts";
    public string Title => "LLM Artifacts";
    public int SortOrder => 37;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        if (_currentOperationCts != null || _settingsRepository == null || _contractCatalog == null)
        {
            return;
        }

        if (_currentViewState.Profiles.Count == 0)
        {
            _ = LoadSettingsAsync();
            return;
        }

        _ = RefreshContentLanguagePolicyAsync();
    }

    private void DisposeRuntimeResources()
    {
        _currentOperationCts?.Dispose();
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
        _contentLanguageComboBox.SelectionChangeCommitted += async (_, _) => await SaveContentLanguagePolicyAsync().ConfigureAwait(true);
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
            await LoadContentLanguagePolicyAsync(cancellationToken).ConfigureAwait(true);
        }).ConfigureAwait(true);
    }

    private async Task LoadContentLanguagePolicyAsync(CancellationToken cancellationToken)
    {
        if (_contentLanguagePolicyService == null)
        {
            return;
        }

        var result = await _contentLanguagePolicyService
            .LoadAsync(_currentGamePackageService?.CurrentFolder, cancellationToken)
            .ConfigureAwait(true);
        ApplyViewState(_presenter.FromContentLanguagePolicy(ReadControlsToState(), result));
    }

    private async Task RefreshContentLanguagePolicyAsync()
    {
        await RunBusyAsync(LoadContentLanguagePolicyAsync).ConfigureAwait(true);
    }

    private async Task SaveContentLanguagePolicyAsync()
    {
        if (_applyingState || _contentLanguagePolicyService == null)
        {
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            var state = ReadControlsToState();
            var result = await _contentLanguagePolicyService.SaveAsync(
                    _currentGamePackageService?.CurrentFolder,
                    new ContentLanguagePolicy { ContentLanguage = state.SelectedContentLanguage },
                    cancellationToken)
                .ConfigureAwait(true);
            ApplyViewState(_presenter.FromContentLanguagePolicy(state, result));
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
            SelectedContentLanguage = _contentLanguageComboBox.SelectedItem is StrictLlmContentLanguageOption language
                ? language.Code
                : ContentLanguageCodes.Russian,
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

            _contentLanguageComboBox.DisplayMember = nameof(StrictLlmContentLanguageOption.DisplayName);
            _contentLanguageComboBox.ValueMember = nameof(StrictLlmContentLanguageOption.Code);
            _contentLanguageComboBox.DataSource = state.ContentLanguages.ToList();
            _contentLanguageComboBox.SelectedValue = state.SelectedContentLanguage;

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
        _contentLanguageComboBox.Enabled = !busy;
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
        _contentLanguageComboBox.Enabled = false;
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
