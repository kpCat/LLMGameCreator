using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.CompositionWorkbench;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class CompositionWorkbenchPageControl : UserControl, IEditorPage
{
    private const int SplitPanel1MinSize = 300;
    private const int SplitPanel2MinSize = 320;
    private readonly CompositionWorkbenchPresenter? _presenter;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private CompositionWorkbenchViewState _state = new();
    private bool _loaded;
    private bool _applyingState;
    private bool _splitterInitialized;

    public CompositionWorkbenchPageControl()
    {
        InitializeComponent();
        SetRuntimeUnavailable();
    }

    public CompositionWorkbenchPageControl(
        CompositionWorkbenchPresenter presenter,
        ICurrentGamePackageService currentGamePackageService)
    {
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        _currentGamePackageService = currentGamePackageService ?? throw new ArgumentNullException(nameof(currentGamePackageService));
        InitializeComponent();
        ConfigureControls();
        WireEvents();
        ApplyViewState(_presenter.Initialize(_currentGamePackageService.CurrentFolder));
    }

    public string Id => "composition_workbench";
    public string Title => "Composition Workbench";
    public int SortOrder => 39;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        if (_presenter is null || _currentGamePackageService is null)
        {
            return;
        }

        _ = EnsureLoadedAsync();
    }

    private void ConfigureControls()
    {
        _presetComboBox.DisplayMember = nameof(CompositionWorkbenchPresetOption.DisplayName);
        _presetComboBox.ValueMember = nameof(CompositionWorkbenchPresetOption.Id);
        _savedReportsList.DisplayMember = nameof(CompositionWorkbenchSavedReportOption.DisplayName);
        _savedReportsList.ValueMember = nameof(CompositionWorkbenchSavedReportOption.ReportFileName);
        _splitContainer.SizeChanged += (_, _) => ApplySafeInitialSplitterDistance();
        ApplySafeInitialSplitterDistance();
    }

    private void ApplySafeInitialSplitterDistance()
    {
        if (_splitterInitialized)
        {
            return;
        }

        var width = _splitContainer.ClientSize.Width;
        var maximum = width - SplitPanel2MinSize;
        if (width <= 0 || maximum < SplitPanel1MinSize)
        {
            return;
        }

        _splitContainer.SplitterDistance = Math.Clamp((int)(width * 0.34), SplitPanel1MinSize, maximum);
        _splitContainer.Panel1MinSize = SplitPanel1MinSize;
        _splitContainer.Panel2MinSize = SplitPanel2MinSize;
        _splitterInitialized = true;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await EnsureLoadedAsync();
        _buildPreviewButton.Click += (_, _) => BuildPreview();
        _refreshReportsButton.Click += async (_, _) => await RefreshReportsAsync();
        _exportReportButton.Click += async (_, _) => await ExportReportAsync();
        _savedReportsList.SelectedIndexChanged += async (_, _) => await LoadSelectedSavedReportAsync();
        _currentGamePackageService!.CurrentChanged += async (_, _) => await CurrentProjectChangedAsync();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded || _presenter is null)
        {
            return;
        }

        _loaded = true;
        BuildPreview();
        await RefreshReportsAsync();
    }

    private void BuildPreview()
    {
        if (_presenter is null)
        {
            return;
        }

        var presetId = (_presetComboBox.SelectedItem as CompositionWorkbenchPresetOption)?.Id;
        ApplyViewState(_presenter.BuildPreview(_state, presetId));
    }

    private async Task RefreshReportsAsync()
    {
        if (_presenter is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            ApplyViewState(await _presenter.RefreshSavedReportsAsync(_state));
        });
    }

    private async Task ExportReportAsync()
    {
        if (_presenter is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var presetId = (_presetComboBox.SelectedItem as CompositionWorkbenchPresetOption)?.Id;
            _state = _presenter.BuildPreview(_state, presetId);
            ApplyViewState(await _presenter.ExportAsync(_state));
        });
    }

    private async Task LoadSelectedSavedReportAsync()
    {
        if (_applyingState || _presenter is null || _savedReportsList.SelectedItem is not CompositionWorkbenchSavedReportOption selected)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            ApplyViewState(await _presenter.LoadSavedReportAsync(_state, selected.ReportFileName));
        });
    }

    private async Task CurrentProjectChangedAsync()
    {
        if (_presenter is null || _currentGamePackageService is null)
        {
            return;
        }

        ApplyViewState(_presenter.Initialize(_currentGamePackageService.CurrentFolder));
        BuildPreview();
        await RefreshReportsAsync();
    }

    private async Task RunBusyAsync(Func<Task> operation)
    {
        SetBusy(true);
        try
        {
            await operation();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            _statusLabel.Text = ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyViewState(CompositionWorkbenchViewState state)
    {
        _state = state;
        _applyingState = true;
        try
        {
            SetPresets(state.Presets, state.SelectedPresetId);
            SetSavedReports(state.SavedReports, state.SelectedReportFileName);
            _readinessValueLabel.Text = state.Readiness;
            _summaryTextBox.Text = state.Summary;
            _markdownTextBox.Text = state.Markdown;
            _statusLabel.Text = state.Status;
        }
        finally
        {
            _applyingState = false;
        }

        _exportReportButton.Enabled = state.CanExport;
    }

    private void SetPresets(IReadOnlyList<CompositionWorkbenchPresetOption> presets, string selectedId)
    {
        if (_presetComboBox.Items.Count != presets.Count || !_presetComboBox.Items.Cast<CompositionWorkbenchPresetOption>()
                .Select(item => item.Id).SequenceEqual(presets.Select(item => item.Id), StringComparer.OrdinalIgnoreCase))
        {
            _presetComboBox.DataSource = presets.ToList();
        }

        var selected = presets.FirstOrDefault(item => string.Equals(item.Id, selectedId, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
        {
            _presetComboBox.SelectedValue = selected.Id;
        }
    }

    private void SetSavedReports(IReadOnlyList<CompositionWorkbenchSavedReportOption> reports, string selectedFileName)
    {
        _savedReportsList.DataSource = reports.ToList();
        if (!string.IsNullOrWhiteSpace(selectedFileName))
        {
            _savedReportsList.SelectedValue = selectedFileName;
        }
    }

    private void SetBusy(bool busy)
    {
        _presetComboBox.Enabled = !busy;
        _savedReportsList.Enabled = !busy;
        _refreshReportsButton.Enabled = !busy;
        _buildPreviewButton.Enabled = !busy;
        _exportReportButton.Enabled = !busy && _state.CanExport;
    }

    private void SetRuntimeUnavailable()
    {
        _presetComboBox.Enabled = false;
        _savedReportsList.Enabled = false;
        _refreshReportsButton.Enabled = false;
        _buildPreviewButton.Enabled = false;
        _exportReportButton.Enabled = false;
        _statusLabel.Text = "Runtime services are not available in design mode.";
    }
}
