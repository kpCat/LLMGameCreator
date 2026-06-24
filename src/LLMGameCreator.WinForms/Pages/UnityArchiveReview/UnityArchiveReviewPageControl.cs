using System.ComponentModel;
using System.Diagnostics;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class UnityArchiveReviewPageControl : UserControl, IEditorPage
{
    private const int SplitPanel1MinSize = 320;
    private const int SplitPanel2MinSize = 420;
    private readonly UnityArchiveReviewPresenter? _presenter;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private UnityArchiveReviewViewState _state = new();
    private bool _loaded;
    private bool _applyingState;
    private bool _splitterInitialized;

    public UnityArchiveReviewPageControl()
    {
        InitializeComponent();
        SetRuntimeUnavailable();
    }

    public UnityArchiveReviewPageControl(
        UnityArchiveReviewPresenter presenter,
        ICurrentGamePackageService currentGamePackageService)
    {
        _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        _currentGamePackageService = currentGamePackageService ?? throw new ArgumentNullException(nameof(currentGamePackageService));
        InitializeComponent();
        ConfigureControls();
        WireEvents();
        ApplyViewState(_presenter.Initialize(_currentGamePackageService.CurrentFolder));
    }

    public string Id => "unity_archive_review";
    public string Title => "Unity Archive Review";
    public int SortOrder => 41;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        if (_presenter is null || _currentGamePackageService is null)
        {
            return;
        }

        _ = RefreshAsync();
    }

    private void ConfigureControls()
    {
        _historySnapshotsList.DisplayMember = nameof(UnityArchiveReviewSnapshotOption.DisplayName);
        _historySnapshotsList.ValueMember = nameof(UnityArchiveReviewSnapshotOption.SnapshotId);
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

        _splitContainer.SplitterDistance = Math.Clamp((int)(width * 0.32), SplitPanel1MinSize, maximum);
        _splitContainer.Panel1MinSize = SplitPanel1MinSize;
        _splitContainer.Panel2MinSize = SplitPanel2MinSize;
        _splitterInitialized = true;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await EnsureLoadedAsync();
        _refreshButton.Click += async (_, _) => await RefreshAsync();
        _openArchiveFolderButton.Click += (_, _) => OpenArchiveFolder();
        _historySnapshotsList.SelectedIndexChanged += async (_, _) => await SelectedSnapshotChangedAsync();
        _currentGamePackageService!.CurrentChanged += async (_, _) => await CurrentProjectChangedAsync();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_presenter is null || _currentGamePackageService is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var refreshed = await _presenter.RefreshAsync(
                _currentGamePackageService.CurrentFolder,
                _state.SelectedSnapshotId);
            ApplyViewState(refreshed);
        });
    }

    private async Task CurrentProjectChangedAsync()
    {
        if (_presenter is null || _currentGamePackageService is null)
        {
            return;
        }

        ApplyViewState(_presenter.Initialize(_currentGamePackageService.CurrentFolder));
        await RefreshAsync();
    }

    private async Task SelectedSnapshotChangedAsync()
    {
        if (_applyingState || _presenter is null || _currentGamePackageService is null ||
            _historySnapshotsList.SelectedItem is not UnityArchiveReviewSnapshotOption selected)
        {
            return;
        }

        _state = _state with { SelectedSnapshotId = selected.SnapshotId };
        await RunBusyAsync(async () =>
        {
            var refreshed = await _presenter.RefreshAsync(
                _currentGamePackageService.CurrentFolder,
                selected.SnapshotId);
            ApplyViewState(refreshed);
        });
    }

    private void OpenArchiveFolder()
    {
        if (!_state.CanOpenArchiveFolder || string.IsNullOrWhiteSpace(_state.ArchiveRoot))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _state.ArchiveRoot,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or Win32Exception)
        {
            _statusLabel.Text = $"Archive folder could not be opened: {ex.Message}";
        }
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

    private void ApplyViewState(UnityArchiveReviewViewState state)
    {
        _state = state;
        _applyingState = true;
        try
        {
            _projectFolderValueLabel.Text = DisplayValue(state.ProjectFolder);
            _archiveRootValueLabel.Text = DisplayValue(state.ArchiveRoot);
            _currentReviewReadinessValueLabel.Text = state.CurrentReviewReadiness;
            _comparisonReadinessValueLabel.Text = state.ComparisonReadiness;
            _historyCountValueLabel.Text = state.HistorySnapshotCount.ToString();
            _historySnapshotsList.DataSource = state.HistorySnapshots.ToList();
            if (!string.IsNullOrWhiteSpace(state.SelectedSnapshotId))
            {
                _historySnapshotsList.SelectedValue = state.SelectedSnapshotId;
            }

            _currentReviewMarkdownTextBox.Text = state.CurrentReviewMarkdown;
            _comparisonMarkdownTextBox.Text = state.ComparisonMarkdown;
            _currentReviewJsonTextBox.Text = state.CurrentReviewJson;
            _comparisonJsonTextBox.Text = state.ComparisonJson;
            _historyIndexJsonTextBox.Text = state.HistoryIndexJson;
            _selectedSnapshotJsonTextBox.Text = state.SelectedSnapshotJson;
            _selectedSnapshotInfoLabel.Text =
                $"Status: {state.SelectedSnapshotStatus} | Sequence: {state.SelectedSnapshotSequence} | Path: {DisplayValue(state.SelectedSnapshotRelativePath)}";
            _manualImportMarkdownTextBox.Text = state.ManualImportReportMarkdown;
            _manualImportJsonTextBox.Text = state.ManualImportReportJson;
            _statusSummaryTextBox.Text = state.Status;
            _statusLabel.Text = state.Status;
        }
        finally
        {
            _applyingState = false;
        }

        _refreshButton.Enabled = state.CanRefresh;
        _openArchiveFolderButton.Enabled = state.CanOpenArchiveFolder;
    }

    private void SetBusy(bool busy)
    {
        _refreshButton.Enabled = !busy && _state.CanRefresh;
        _openArchiveFolderButton.Enabled = !busy && _state.CanOpenArchiveFolder;
        _historySnapshotsList.Enabled = !busy;
    }

    private void SetRuntimeUnavailable()
    {
        _refreshButton.Enabled = false;
        _openArchiveFolderButton.Enabled = false;
        _historySnapshotsList.Enabled = false;
        _statusSummaryTextBox.Text = "Runtime services are not available in design mode.";
        _statusLabel.Text = "Runtime services are not available in design mode.";
    }

    private static string DisplayValue(string value) => string.IsNullOrWhiteSpace(value) ? "Not available" : value;
}
