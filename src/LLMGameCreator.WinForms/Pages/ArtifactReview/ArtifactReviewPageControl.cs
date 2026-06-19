using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.ArtifactReview;

namespace LLMGameCreator.WinForms.Pages;

public sealed class ArtifactReviewPageControl : UserControl, IEditorPage
{
    private const string ExampleFilter = "Generator plan example (*.example.json)|*.example.json|JSON files (*.json)|*.json|All files (*.*)|*.*";

    private readonly GeneratorPlanDraftArtifactReviewService? _reviewService;
    private readonly GeneratorPlanGamePackageAssemblyService? _assemblyService;
    private readonly GeneratorPlanGamePackageAssemblyArtifactService? _assemblyArtifactService;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly ArtifactReviewPresenter _presenter = new();

    private CancellationTokenSource? _currentOperationCts;
    private ArtifactReviewViewState _currentViewState = new() { Exists = false, Message = "No review snapshot loaded." };
    private bool _updatingGrid;

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _actionsLayout = new();
    private readonly TextBox _sourceExampleTextBox = new();
    private readonly Button _browseExampleButton = new();
    private readonly Button _captureButton = new();
    private readonly Button _loadLatestButton = new();
    private readonly Button _applyDecisionsButton = new();
    private readonly Button _approveAllButton = new();
    private readonly Button _rejectSelectedButton = new();
    private readonly Button _repairSelectedButton = new();
    private readonly Button _approveSelectedButton = new();
    private readonly TextBox _assemblyExportFolderTextBox = new();
    private readonly Button _browseAssemblyExportFolderButton = new();
    private readonly Button _applyApprovedToPackageButton = new();
    private readonly ComboBox _filterComboBox = new();
    private readonly TableLayoutPanel _summaryLayout = new();
    private readonly TextBox _snapshotIdTextBox = new();
    private readonly TextBox _sourceExampleIdTextBox = new();
    private readonly TextBox _sourcePathTextBox = new();
    private readonly TextBox _statusTextBox = new();
    private readonly TextBox _countsTextBox = new();
    private readonly SplitContainer _splitContainer = new();
    private readonly DataGridView _artifactGrid = new();
    private readonly TableLayoutPanel _detailLayout = new();
    private readonly TextBox _artifactJsonTextBox = new();
    private readonly TextBox _validationIssuesTextBox = new();
    private readonly TextBox _reasonCodeTextBox = new();
    private readonly TextBox _commentTextBox = new();
    private readonly Button _copyJsonButton = new();

    public ArtifactReviewPageControl()
    {
        BuildLayout();
        SetRuntimeUnavailable();
    }

    public ArtifactReviewPageControl(
        GeneratorPlanDraftArtifactReviewService reviewService,
        GeneratorPlanGamePackageAssemblyService assemblyService,
        GeneratorPlanGamePackageAssemblyArtifactService assemblyArtifactService,
        IDesignDatabaseInitializer databaseInitializer,
        ICurrentGamePackageService currentGamePackageService)
    {
        _reviewService = reviewService;
        _assemblyService = assemblyService;
        _assemblyArtifactService = assemblyArtifactService;
        _databaseInitializer = databaseInitializer;
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
        WireEvents();
        ApplyViewState(_currentViewState);
    }

    public string Id => "artifact_review";
    public string Title => "Artifact Review";
    public int SortOrder => 38;
    Control IEditorPage.View => this;

    public void OnActivated()
    {
        if (_currentOperationCts == null)
        {
            SetBusy(false);
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
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 98F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildActionsLayout();
        BuildSummaryLayout();
        BuildSplitLayout();

        _rootLayout.Controls.Add(_actionsLayout, 0, 0);
        _rootLayout.Controls.Add(_summaryLayout, 0, 1);
        _rootLayout.Controls.Add(_splitContainer, 0, 2);

        Controls.Add(_rootLayout);
        Name = nameof(ArtifactReviewPageControl);
        Size = new Size(1180, 780);
        ResumeLayout(false);
    }

    private void BuildActionsLayout()
    {
        _actionsLayout.ColumnCount = 6;
        _actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        _actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        _actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
        _actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 136F));
        _actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        _actionsLayout.Dock = DockStyle.Fill;
        _actionsLayout.RowCount = 3;
        _actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        _actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        _actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var sourceLabel = new Label
        {
            Text = "Source example path:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        ConfigureTextBox(_sourceExampleTextBox, false);
        ConfigureButton(_browseExampleButton, "Browse...");
        ConfigureButton(_captureButton, "Capture for review");
        ConfigureButton(_loadLatestButton, "Load latest staging");
        ConfigureButton(_applyDecisionsButton, "Apply selected decisions");
        ConfigureButton(_approveAllButton, "Approve all valid pending");
        ConfigureButton(_approveSelectedButton, "Approve selected");
        ConfigureButton(_rejectSelectedButton, "Reject selected");
        ConfigureButton(_repairSelectedButton, "Request repair selected");
        ConfigureButton(_browseAssemblyExportFolderButton, "Browse...");
        ConfigureButton(_applyApprovedToPackageButton, "Apply approved to package");
        ConfigureTextBox(_assemblyExportFolderTextBox, false);

        _filterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _filterComboBox.Dock = DockStyle.Fill;
        _filterComboBox.Items.AddRange(new object[]
        {
            ArtifactReviewFilter.All,
            ArtifactReviewFilter.Pending,
            ArtifactReviewFilter.Approved,
            ArtifactReviewFilter.Rejected,
            ArtifactReviewFilter.RepairRequested,
            ArtifactReviewFilter.Blocked
        });
        _filterComboBox.SelectedIndex = 0;

        _actionsLayout.Controls.Add(sourceLabel, 0, 0);
        _actionsLayout.Controls.Add(_sourceExampleTextBox, 1, 0);
        _actionsLayout.Controls.Add(_browseExampleButton, 2, 0);
        _actionsLayout.Controls.Add(_captureButton, 3, 0);
        _actionsLayout.Controls.Add(_loadLatestButton, 4, 0);
        _actionsLayout.Controls.Add(_applyDecisionsButton, 5, 0);
        _actionsLayout.Controls.Add(_approveAllButton, 1, 1);
        _actionsLayout.Controls.Add(_approveSelectedButton, 2, 1);
        _actionsLayout.Controls.Add(_rejectSelectedButton, 3, 1);
        _actionsLayout.Controls.Add(_repairSelectedButton, 4, 1);
        _actionsLayout.Controls.Add(_filterComboBox, 5, 1);

        var assemblyExportLabel = new Label
        {
            Text = "Assembly export folder:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _actionsLayout.Controls.Add(assemblyExportLabel, 0, 2);
        _actionsLayout.Controls.Add(_assemblyExportFolderTextBox, 1, 2);
        _actionsLayout.SetColumnSpan(_assemblyExportFolderTextBox, 3);
        _actionsLayout.Controls.Add(_browseAssemblyExportFolderButton, 4, 2);
        _actionsLayout.Controls.Add(_applyApprovedToPackageButton, 5, 2);
    }

    private void BuildSummaryLayout()
    {
        _summaryLayout.ColumnCount = 2;
        _summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        _summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _summaryLayout.Dock = DockStyle.Fill;
        _summaryLayout.RowCount = 5;
        for (var i = 0; i < 5; i++)
        {
            _summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        }

        AddSummaryRow(0, "Snapshot id", _snapshotIdTextBox);
        AddSummaryRow(1, "Source example id", _sourceExampleIdTextBox);
        AddSummaryRow(2, "Source path", _sourcePathTextBox);
        AddSummaryRow(3, "Status", _statusTextBox);
        AddSummaryRow(4, "Counts", _countsTextBox);
    }

    private void BuildSplitLayout()
    {
        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.Orientation = Orientation.Vertical;
        _splitContainer.SplitterDistance = 690;

        BuildArtifactGrid();
        BuildDetailLayout();

        _splitContainer.Panel1.Controls.Add(_artifactGrid);
        _splitContainer.Panel2.Controls.Add(_detailLayout);
    }

    private void BuildArtifactGrid()
    {
        _artifactGrid.AllowUserToAddRows = false;
        _artifactGrid.AllowUserToDeleteRows = false;
        _artifactGrid.AllowUserToResizeRows = false;
        _artifactGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _artifactGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _artifactGrid.Dock = DockStyle.Fill;
        _artifactGrid.MultiSelect = false;
        _artifactGrid.ReadOnly = true;
        _artifactGrid.RowHeadersVisible = false;
        _artifactGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _artifactGrid.Columns.Add("ArtifactId", "ArtifactId");
        _artifactGrid.Columns.Add("Kind", "Kind");
        _artifactGrid.Columns.Add("Contract", "Contract");
        _artifactGrid.Columns.Add("State", "State");
        _artifactGrid.Columns.Add("RequiresApproval", "RequiresApproval");
        _artifactGrid.Columns.Add("Issues", "Issues");
        _artifactGrid.Columns.Add("Reason", "Reason");
        _artifactGrid.Columns.Add("Comment", "Comment");
    }

    private void BuildDetailLayout()
    {
        _detailLayout.ColumnCount = 1;
        _detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _detailLayout.Dock = DockStyle.Fill;
        _detailLayout.RowCount = 7;
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        _detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

        _artifactJsonTextBox.Dock = DockStyle.Fill;
        _artifactJsonTextBox.Multiline = true;
        _artifactJsonTextBox.ReadOnly = true;
        _artifactJsonTextBox.ScrollBars = ScrollBars.Both;
        _artifactJsonTextBox.WordWrap = false;

        _validationIssuesTextBox.Dock = DockStyle.Fill;
        _validationIssuesTextBox.Multiline = true;
        _validationIssuesTextBox.ReadOnly = true;
        _validationIssuesTextBox.ScrollBars = ScrollBars.Vertical;

        var reasonLabel = new Label { Text = "Decision reason code", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        var commentLabel = new Label { Text = "Decision comment", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        ConfigureTextBox(_reasonCodeTextBox, false);
        _commentTextBox.Dock = DockStyle.Fill;
        _commentTextBox.Multiline = true;
        _commentTextBox.ScrollBars = ScrollBars.Vertical;
        ConfigureButton(_copyJsonButton, "Copy artifact JSON");

        _detailLayout.Controls.Add(_artifactJsonTextBox, 0, 0);
        _detailLayout.Controls.Add(_validationIssuesTextBox, 0, 1);
        _detailLayout.Controls.Add(reasonLabel, 0, 2);
        _detailLayout.Controls.Add(_reasonCodeTextBox, 0, 3);
        _detailLayout.Controls.Add(commentLabel, 0, 4);
        _detailLayout.Controls.Add(_commentTextBox, 0, 5);
        _detailLayout.Controls.Add(_copyJsonButton, 0, 6);
    }

    private void AddSummaryRow(int row, string labelText, TextBox textBox)
    {
        _summaryLayout.Controls.Add(new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, row);
        ConfigureTextBox(textBox, true);
        _summaryLayout.Controls.Add(textBox, 1, row);
    }

    private static void ConfigureTextBox(TextBox textBox, bool readOnly)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.ReadOnly = readOnly;
    }

    private static void ConfigureButton(Button button, string text)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
    }

    private void WireEvents()
    {
        _browseExampleButton.Click += (_, _) => BrowseExample();
        _captureButton.Click += async (_, _) => await CaptureForReviewAsync().ConfigureAwait(true);
        _loadLatestButton.Click += async (_, _) => await LoadLatestAsync().ConfigureAwait(true);
        _applyDecisionsButton.Click += async (_, _) => await ApplyDecisionsAsync().ConfigureAwait(true);
        _approveAllButton.Click += (_, _) => ApplyApproveAllValidPending();
        _approveSelectedButton.Click += (_, _) => ApplyDecisionToSelected(GeneratorPlanDraftArtifactApprovalDecisionKind.Approved);
        _rejectSelectedButton.Click += (_, _) => ApplyDecisionToSelected(GeneratorPlanDraftArtifactApprovalDecisionKind.Rejected);
        _repairSelectedButton.Click += (_, _) => ApplyDecisionToSelected(GeneratorPlanDraftArtifactApprovalDecisionKind.RepairRequested);
        _browseAssemblyExportFolderButton.Click += (_, _) => BrowseAssemblyExportFolder();
        _applyApprovedToPackageButton.Click += async (_, _) => await ApplyApprovedToPackageAsync().ConfigureAwait(true);
        _copyJsonButton.Click += (_, _) => TryCopyText(_artifactJsonTextBox.Text);
        _filterComboBox.SelectedIndexChanged += (_, _) => ApplyFilter();
        _artifactGrid.SelectionChanged += (_, _) => SelectCurrentGridRow();
    }

    private void BrowseExample()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = ExampleFilter,
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceExampleTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseAssemblyExportFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            SelectedPath = Directory.Exists(_assemblyExportFolderTextBox.Text) ? _assemblyExportFolderTextBox.Text : ResolveAssemblyExportFolder()
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _assemblyExportFolderTextBox.Text = dialog.SelectedPath;
        }
    }

    private async Task CaptureForReviewAsync()
    {
        if (_reviewService == null)
        {
            SetStatusMessage("Artifact review service is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            await _reviewService.CaptureReviewFromExampleAsync(_sourceExampleTextBox.Text, true, cancellationToken).ConfigureAwait(true);
            var latest = await _reviewService.LoadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLoadResult(latest, CurrentFilter()));
        }).ConfigureAwait(true);
    }

    private async Task LoadLatestAsync()
    {
        if (_reviewService == null)
        {
            SetStatusMessage("Artifact review service is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var latest = await _reviewService.LoadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLoadResult(latest, CurrentFilter()));
        }).ConfigureAwait(true);
    }

    private async Task ApplyDecisionsAsync()
    {
        if (_reviewService == null)
        {
            SetStatusMessage("Artifact review service is not available.");
            return;
        }

        UpdateSelectedDecisionText();
        var request = _presenter.BuildDecisionRequest(_currentViewState);
        if (request.Decisions.Count == 0)
        {
            SetStatusMessage("No changed decisions to apply.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var result = await _reviewService.ApplyDecisionsToLatestAsync(request, cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromDecisionResult(result, CurrentFilter()));
        }).ConfigureAwait(true);
    }

    private async Task ApplyApprovedToPackageAsync()
    {
        if (_assemblyService == null || _assemblyArtifactService == null)
        {
            SetStatusMessage("Package assembly service is not available.");
            return;
        }

        var exportFolder = string.IsNullOrWhiteSpace(_assemblyExportFolderTextBox.Text)
            ? ResolveAssemblyExportFolder()
            : _assemblyExportFolderTextBox.Text.Trim();
        _assemblyExportFolderTextBox.Text = exportFolder;

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var result = await _assemblyService.AssembleFromLatestApprovedArtifactSetAsync(new GeneratorPlanGamePackageAssemblyRequest
            {
                ExportPackageJson = true,
                ExportFolderPath = exportFolder,
                SerializePackageJson = true,
                RenderMarkdown = true
            }, cancellationToken).ConfigureAwait(true);

            await _assemblyArtifactService.SaveAsync(result, new GeneratorPlanGamePackageAssemblyArtifactSaveRequest
            {
                GeneratedBy = "artifact_review_ui"
            }, cancellationToken).ConfigureAwait(true);

            _currentGamePackageService?.ReplaceCurrent(result.Package);
            var packageJsonPath = Path.Combine(exportFolder, "package.json");
            var reportText = string.Join(Environment.NewLine, new[]
            {
                $"Package assembly status: {result.Status}",
                $"Approved artifacts: {result.Summary.ApprovedArtifactCount}",
                $"Applied artifacts: {result.Summary.AppliedArtifactCount}",
                $"Skipped artifacts: {result.Summary.SkippedArtifactCount}",
                $"Export folder: {exportFolder}",
                $"Package JSON: {packageJsonPath}",
                $"Diagnostics: {result.Diagnostics.Count}"
            });
            _statusTextBox.Text = result.Ok ? "Approved artifacts applied to draft package." : "Package assembly completed with errors.";
            _countsTextBox.Text = reportText;
            _validationIssuesTextBox.Text = string.IsNullOrWhiteSpace(result.MarkdownReport)
                ? reportText
                : result.MarkdownReport;
        }).ConfigureAwait(true);
    }

    private void ApplyApproveAllValidPending()
    {
        _currentViewState = _presenter.ApproveAllValidPending(_currentViewState, _reasonCodeTextBox.Text, _commentTextBox.Text);
        ApplyViewState(_currentViewState);
    }

    private void ApplyDecisionToSelected(string decision)
    {
        if (string.IsNullOrWhiteSpace(_currentViewState.SelectedArtifactId))
        {
            return;
        }

        _currentViewState = _presenter.SetDecision(
            _currentViewState,
            _currentViewState.SelectedArtifactId,
            decision,
            _reasonCodeTextBox.Text,
            _commentTextBox.Text);
        ApplyViewState(_currentViewState);
    }

    private void ApplyFilter()
    {
        _currentViewState = _presenter.ApplyFilter(_currentViewState, CurrentFilter());
        ApplyViewState(_currentViewState);
    }

    private void SelectCurrentGridRow()
    {
        if (_updatingGrid)
        {
            return;
        }

        if (_artifactGrid.SelectedRows.Count == 0 || _artifactGrid.SelectedRows[0].Tag is not ArtifactReviewRowViewModel row)
        {
            return;
        }

        _currentViewState = _presenter.SelectArtifact(_currentViewState, row.ArtifactId);
        ApplyDetail(_currentViewState.Detail);
    }

    private void UpdateSelectedDecisionText()
    {
        if (string.IsNullOrWhiteSpace(_currentViewState.SelectedArtifactId))
        {
            return;
        }

        var selected = _currentViewState.Rows.FirstOrDefault(row => string.Equals(row.ArtifactId, _currentViewState.SelectedArtifactId, StringComparison.OrdinalIgnoreCase));
        if (selected == null || !selected.IsChanged)
        {
            return;
        }

        _currentViewState = _presenter.SetDecision(
            _currentViewState,
            selected.ArtifactId,
            selected.Decision,
            _reasonCodeTextBox.Text,
            _commentTextBox.Text);
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
        catch (Exception ex)
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

    private string ResolveAssemblyExportFolder()
    {
        if (!string.IsNullOrWhiteSpace(_currentGamePackageService?.CurrentFolder))
        {
            return Path.Combine(_currentGamePackageService.CurrentFolder, ".llmgc", "package-assembly");
        }

        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator");
        return Path.Combine(appData, "package-assembly");
    }

    private void ApplyViewState(ArtifactReviewViewState state)
    {
        _currentViewState = state;
        _snapshotIdTextBox.Text = state.SnapshotId;
        _sourceExampleIdTextBox.Text = state.SourceExampleId;
        _sourcePathTextBox.Text = state.SourcePath;
        _statusTextBox.Text = string.IsNullOrWhiteSpace(state.Status) ? state.Message : state.Status;
        _countsTextBox.Text = $"Items {state.ItemCount}; Pending {state.PendingCount}; Approved {state.ApprovedCount}; Rejected {state.RejectedCount}; Repair {state.RepairRequestedCount}; Blocked {state.BlockedCount}; Errors {state.ErrorCount}; Warnings {state.WarningCount}";
        SetGridRows(state.FilteredRows);
        ApplyDetail(state.Detail);
        RefreshActions();
    }

    private void SetGridRows(IReadOnlyList<ArtifactReviewRowViewModel> rows)
    {
        _updatingGrid = true;
        _artifactGrid.Rows.Clear();
        foreach (var row in rows)
        {
            var index = _artifactGrid.Rows.Add(row.ArtifactId, row.Kind, row.Contract, row.Decision, row.RequiresApproval, row.Issues, row.ReasonCode, row.Comment);
            _artifactGrid.Rows[index].Tag = row;
            if (row.IsChanged)
            {
                _artifactGrid.Rows[index].DefaultCellStyle.BackColor = Color.LightYellow;
            }

            if (string.Equals(row.ArtifactId, _currentViewState.SelectedArtifactId, StringComparison.OrdinalIgnoreCase))
            {
                _artifactGrid.Rows[index].Selected = true;
            }
        }
        _updatingGrid = false;
    }

    private void ApplyDetail(ArtifactReviewDetailViewModel detail)
    {
        _artifactJsonTextBox.Text = detail.ContentJson;
        _validationIssuesTextBox.Text = detail.ValidationIssues;
        _reasonCodeTextBox.Text = detail.ReasonCode;
        _commentTextBox.Text = detail.Comment;
    }

    private void SetBusy(bool busy)
    {
        _browseExampleButton.Enabled = !busy;
        _captureButton.Enabled = !busy;
        _loadLatestButton.Enabled = !busy;
        _applyDecisionsButton.Enabled = !busy;
        _approveAllButton.Enabled = !busy;
        _approveSelectedButton.Enabled = !busy;
        _rejectSelectedButton.Enabled = !busy;
        _repairSelectedButton.Enabled = !busy;
        _browseAssemblyExportFolderButton.Enabled = !busy;
        _applyApprovedToPackageButton.Enabled = !busy;
        _filterComboBox.Enabled = !busy;
        RefreshActions();
    }

    private void RefreshActions()
    {
        var busy = _currentOperationCts != null;
        var hasRows = _currentViewState.Rows.Count > 0;
        var hasSelection = !string.IsNullOrWhiteSpace(_currentViewState.SelectedArtifactId);
        _applyDecisionsButton.Enabled = !busy && _currentViewState.Rows.Any(row => row.IsChanged);
        _approveAllButton.Enabled = !busy && _currentViewState.Rows.Any(row => row.CanApprove);
        _approveSelectedButton.Enabled = !busy && hasSelection && hasRows;
        _rejectSelectedButton.Enabled = !busy && hasSelection && hasRows;
        _repairSelectedButton.Enabled = !busy && hasSelection && hasRows;
        _browseAssemblyExportFolderButton.Enabled = !busy;
        _applyApprovedToPackageButton.Enabled = !busy && _currentViewState.ApprovedCount > 0;
        _copyJsonButton.Enabled = !busy && !string.IsNullOrWhiteSpace(_artifactJsonTextBox.Text);
    }

    private void SetRuntimeUnavailable()
    {
        _sourceExampleTextBox.Enabled = false;
        _browseExampleButton.Enabled = false;
        _captureButton.Enabled = false;
        _loadLatestButton.Enabled = false;
        _applyDecisionsButton.Enabled = false;
        _approveAllButton.Enabled = false;
        _approveSelectedButton.Enabled = false;
        _rejectSelectedButton.Enabled = false;
        _repairSelectedButton.Enabled = false;
        _assemblyExportFolderTextBox.Enabled = false;
        _browseAssemblyExportFolderButton.Enabled = false;
        _applyApprovedToPackageButton.Enabled = false;
        _filterComboBox.Enabled = false;
        _copyJsonButton.Enabled = false;
        SetStatusMessage("Runtime services are not available.");
    }

    private string CurrentFilter()
    {
        return _filterComboBox.SelectedItem?.ToString() ?? ArtifactReviewFilter.All;
    }

    private void SetStatusMessage(string message)
    {
        _statusTextBox.Text = message;
        _countsTextBox.Text = message;
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
