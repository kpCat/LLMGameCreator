using System.Diagnostics;
using LLMGameCreator.Application.Design;
using LLMGameCreator.Application.Design.GeneratorPlans;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.WinForms.Pages.PackageExport;

namespace LLMGameCreator.WinForms.Pages;

public sealed class PackageExportPageControl : UserControl, IEditorPage
{
    private const string ExampleFilter = "Generator plan example (*.example.json)|*.example.json|JSON files (*.json)|*.json|All files (*.*)|*.*";

    private readonly GeneratorPlanPackageExportRunService? _runService;
    private readonly GeneratorPlanPackageExportRunArtifactReader? _runReader;
    private readonly IDesignDatabaseInitializer? _databaseInitializer;
    private readonly ICurrentGamePackageService? _currentGamePackageService;
    private readonly PackageExportRunPresenter _presenter = new();

    private CancellationTokenSource? _currentOperationCts;
    private PackageExportRunViewState _currentViewState = new() { Exists = false, Summary = "No run loaded." };

    private readonly TableLayoutPanel _rootLayout = new();
    private readonly TableLayoutPanel _inputLayout = new();
    private readonly TextBox _sourceExampleTextBox = new();
    private readonly TextBox _exportFolderTextBox = new();
    private readonly Button _browseExampleButton = new();
    private readonly Button _browseExportFolderButton = new();
    private readonly Button _generateButton = new();
    private readonly Button _cancelButton = new();
    private readonly Button _loadLatestButton = new();
    private readonly TableLayoutPanel _resultLayout = new();
    private readonly TextBox _statusTextBox = new();
    private readonly TextBox _packageJsonTextBox = new();
    private readonly TextBox _resultExportFolderTextBox = new();
    private readonly TextBox _approvalStatusTextBox = new();
    private readonly TextBox _assemblyStatusTextBox = new();
    private readonly TextBox _errorCountTextBox = new();
    private readonly TextBox _warningCountTextBox = new();
    private readonly TextBox _summaryTextBox = new();
    private readonly DataGridView _diagnosticsGrid = new();
    private readonly FlowLayoutPanel _actionsPanel = new();
    private readonly Button _openExportFolderButton = new();
    private readonly Button _openPackageJsonButton = new();
    private readonly Button _copyPackagePathButton = new();
    private readonly Button _copyMarkdownReportButton = new();
    private readonly TextBox _reportTextBox = new();

    public PackageExportPageControl()
    {
        BuildLayout();
        SetRuntimeUnavailable();
    }

    public PackageExportPageControl(
        GeneratorPlanPackageExportRunService runService,
        GeneratorPlanPackageExportRunArtifactReader runReader,
        IDesignDatabaseInitializer databaseInitializer,
        ICurrentGamePackageService currentGamePackageService)
    {
        _runService = runService;
        _runReader = runReader;
        _databaseInitializer = databaseInitializer;
        _currentGamePackageService = currentGamePackageService;
        BuildLayout();
        WireEvents();
        ApplyViewState(_currentViewState);
    }

    public string Id => "package_export";
    public string Title => "Package Export";
    public int SortOrder => 34;
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
        _rootLayout.RowCount = 5;
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 176F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 54F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));

        BuildInputLayout();
        BuildResultLayout();
        BuildDiagnosticsGrid();
        BuildActionsPanel();
        BuildReportBox();

        _rootLayout.Controls.Add(_inputLayout, 0, 0);
        _rootLayout.Controls.Add(_resultLayout, 0, 1);
        _rootLayout.Controls.Add(_diagnosticsGrid, 0, 2);
        _rootLayout.Controls.Add(_actionsPanel, 0, 3);
        _rootLayout.Controls.Add(_reportTextBox, 0, 4);

        Controls.Add(_rootLayout);
        Name = nameof(PackageExportPageControl);
        Size = new Size(1100, 760);
        ResumeLayout(false);
    }

    private void BuildInputLayout()
    {
        _inputLayout.ColumnCount = 3;
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        _inputLayout.Dock = DockStyle.Fill;
        _inputLayout.RowCount = 3;
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        _inputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

        AddInputRow(0, "Source example path:", _sourceExampleTextBox, _browseExampleButton, "Browse...");
        AddInputRow(1, "Export folder:", _exportFolderTextBox, _browseExportFolderButton, "Browse...");

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        ConfigureButton(_generateButton, "Generate package", 150);
        ConfigureButton(_cancelButton, "Cancel", 90);
        ConfigureButton(_loadLatestButton, "Load latest run", 140);
        buttonsPanel.Controls.Add(_generateButton);
        buttonsPanel.Controls.Add(_cancelButton);
        buttonsPanel.Controls.Add(_loadLatestButton);
        _inputLayout.SetColumnSpan(buttonsPanel, 3);
        _inputLayout.Controls.Add(buttonsPanel, 0, 2);
    }

    private void AddInputRow(int row, string labelText, TextBox textBox, Button button, string buttonText)
    {
        var label = new Label
        {
            Dock = DockStyle.Fill,
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        textBox.Dock = DockStyle.Fill;
        button.Text = buttonText;
        button.Dock = DockStyle.Fill;
        _inputLayout.Controls.Add(label, 0, row);
        _inputLayout.Controls.Add(textBox, 1, row);
        _inputLayout.Controls.Add(button, 2, row);
    }

    private void BuildResultLayout()
    {
        _resultLayout.ColumnCount = 4;
        _resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        _resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        _resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _resultLayout.Dock = DockStyle.Fill;
        _resultLayout.RowCount = 4;
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        _resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));

        AddResultRow(0, "Status:", _statusTextBox, "Package JSON:", _packageJsonTextBox);
        AddResultRow(1, "Export folder:", _resultExportFolderTextBox, "Approval status:", _approvalStatusTextBox);
        AddResultRow(2, "Assembly status:", _assemblyStatusTextBox, "Errors:", _errorCountTextBox);
        AddResultRow(3, "Warnings:", _warningCountTextBox, "Run summary:", null);
    }

    private void AddResultRow(int row, string leftLabel, TextBox leftBox, string rightLabel, TextBox? rightBox)
    {
        _resultLayout.Controls.Add(BuildLabel(leftLabel), 0, row);
        ConfigureReadOnlyBox(leftBox);
        _resultLayout.Controls.Add(leftBox, 1, row);
        _resultLayout.Controls.Add(BuildLabel(rightLabel), 2, row);

        if (rightBox != null)
        {
            ConfigureReadOnlyBox(rightBox);
            _resultLayout.Controls.Add(rightBox, 3, row);
            return;
        }

        _summaryTextBox.Multiline = true;
        _summaryTextBox.ReadOnly = true;
        _summaryTextBox.ScrollBars = ScrollBars.Vertical;
        _summaryTextBox.WordWrap = false;
        _summaryTextBox.Font = new Font("Consolas", 9F);
        _summaryTextBox.Dock = DockStyle.Fill;
        _resultLayout.Controls.Add(_summaryTextBox, 3, row);
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

    private static void ConfigureReadOnlyBox(TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.ReadOnly = true;
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
        _diagnosticsGrid.Columns.Add("Code", "Code");
        _diagnosticsGrid.Columns.Add("Target", "Target");
        _diagnosticsGrid.Columns.Add("Message", "Message");
    }

    private void BuildActionsPanel()
    {
        _actionsPanel.Dock = DockStyle.Fill;
        _actionsPanel.FlowDirection = FlowDirection.LeftToRight;
        _actionsPanel.WrapContents = false;
        ConfigureButton(_openExportFolderButton, "Open export folder", 150);
        ConfigureButton(_openPackageJsonButton, "Open package.json", 145);
        ConfigureButton(_copyPackagePathButton, "Copy package path", 145);
        ConfigureButton(_copyMarkdownReportButton, "Copy markdown report", 165);
        _actionsPanel.Controls.Add(_openExportFolderButton);
        _actionsPanel.Controls.Add(_openPackageJsonButton);
        _actionsPanel.Controls.Add(_copyPackagePathButton);
        _actionsPanel.Controls.Add(_copyMarkdownReportButton);
    }

    private void BuildReportBox()
    {
        _reportTextBox.ReadOnly = true;
        _reportTextBox.Multiline = true;
        _reportTextBox.ScrollBars = ScrollBars.Both;
        _reportTextBox.WordWrap = false;
        _reportTextBox.Font = new Font("Consolas", 9F);
        _reportTextBox.Dock = DockStyle.Fill;
    }

    private static void ConfigureButton(Button button, string text, int width)
    {
        button.Text = text;
        button.Width = width;
        button.Height = 32;
        button.Margin = new Padding(3);
        button.UseVisualStyleBackColor = true;
    }

    private void WireEvents()
    {
        _browseExampleButton.Click += (_, _) => BrowseExample();
        _browseExportFolderButton.Click += (_, _) => BrowseExportFolder();
        _generateButton.Click += async (_, _) => await GeneratePackageAsync().ConfigureAwait(true);
        _cancelButton.Click += (_, _) => CancelCurrentOperation();
        _loadLatestButton.Click += async (_, _) => await LoadLatestRunAsync().ConfigureAwait(true);
        _openExportFolderButton.Click += (_, _) => TryOpenPath(_currentViewState.ExportFolderPath);
        _openPackageJsonButton.Click += (_, _) => TryOpenPath(_currentViewState.PackageJsonPath);
        _copyPackagePathButton.Click += (_, _) => TryCopyText(_currentViewState.PackageJsonPath);
        _copyMarkdownReportButton.Click += (_, _) => TryCopyText(_currentViewState.MarkdownReport);
    }

    private void BrowseExample()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = ExampleFilter,
            FileName = _sourceExampleTextBox.Text.Trim()
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceExampleTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseExportFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            SelectedPath = Directory.Exists(_exportFolderTextBox.Text) ? _exportFolderTextBox.Text : string.Empty
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _exportFolderTextBox.Text = dialog.SelectedPath;
        }
    }

    private async Task GeneratePackageAsync()
    {
        if (_runService == null)
        {
            SetStatusMessage("Package export service is not available.");
            return;
        }

        var sourcePath = _sourceExampleTextBox.Text.Trim();
        var exportFolder = _exportFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            SetStatusMessage("Source example path is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(exportFolder))
        {
            SetStatusMessage("Export folder path is required.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            ClearDiagnostics();
            var result = await _runService.RunAsync(new GeneratorPlanPackageExportRunRequest
            {
                SourceExamplePath = sourcePath,
                ExportFolderPath = exportFolder,
                AutoApproveValidArtifacts = true,
                RenderMarkdown = true,
                SaveArtifacts = true
            }, cancellationToken).ConfigureAwait(true);

            ApplyViewState(_presenter.FromRunResult(result));
        }).ConfigureAwait(true);
    }

    private async Task LoadLatestRunAsync()
    {
        if (_runReader == null)
        {
            SetStatusMessage("Package export run reader is not available.");
            return;
        }

        await RunBusyAsync(async cancellationToken =>
        {
            await InitializeDatabaseAsync(cancellationToken).ConfigureAwait(true);
            var result = await _runReader.ReadLatestAsync(cancellationToken).ConfigureAwait(true);
            ApplyViewState(_presenter.FromLatestRun(result));
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
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
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

    private void CancelCurrentOperation()
    {
        try
        {
            _currentOperationCts?.Cancel();
            _cancelButton.Enabled = false;
            SetStatusMessage("Cancel requested...");
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void ApplyViewState(PackageExportRunViewState state)
    {
        _currentViewState = state;
        _statusTextBox.Text = state.Status;
        _packageJsonTextBox.Text = state.PackageJsonPath;
        _resultExportFolderTextBox.Text = state.ExportFolderPath;
        _approvalStatusTextBox.Text = state.ApprovalStatus;
        _assemblyStatusTextBox.Text = state.AssemblyStatus;
        _errorCountTextBox.Text = state.ErrorCount.ToString();
        _warningCountTextBox.Text = state.WarningCount.ToString();
        _summaryTextBox.Text = state.Summary;
        _reportTextBox.Text = string.IsNullOrWhiteSpace(state.MarkdownReport)
            ? string.Empty
            : state.MarkdownReport;
        SetDiagnostics(state.Diagnostics);
        RefreshActions();
    }

    private void SetDiagnostics(IReadOnlyList<PackageExportDiagnosticRow> diagnostics)
    {
        _diagnosticsGrid.Rows.Clear();
        foreach (var diagnostic in diagnostics)
        {
            _diagnosticsGrid.Rows.Add(diagnostic.Severity, diagnostic.Code, diagnostic.Target, diagnostic.Message);
        }
    }

    private void ClearDiagnostics()
    {
        _diagnosticsGrid.Rows.Clear();
    }

    private void SetBusy(bool busy)
    {
        _generateButton.Enabled = !busy;
        _cancelButton.Enabled = busy;
        _loadLatestButton.Enabled = !busy;
        _browseExampleButton.Enabled = !busy;
        _browseExportFolderButton.Enabled = !busy;
        RefreshActions();
    }

    private void RefreshActions()
    {
        var busy = _currentOperationCts != null;
        _openExportFolderButton.Enabled = !busy && _currentViewState.CanOpenExportFolder;
        _openPackageJsonButton.Enabled = !busy && _currentViewState.CanOpenPackageJson;
        _copyPackagePathButton.Enabled = !busy && _currentViewState.CanCopyPackagePath;
        _copyMarkdownReportButton.Enabled = !busy && _currentViewState.CanCopyMarkdownReport;
    }

    private void SetRuntimeUnavailable()
    {
        _sourceExampleTextBox.Enabled = false;
        _exportFolderTextBox.Enabled = false;
        _browseExampleButton.Enabled = false;
        _browseExportFolderButton.Enabled = false;
        _generateButton.Enabled = false;
        _cancelButton.Enabled = false;
        _loadLatestButton.Enabled = false;
        SetStatusMessage("Runtime services are not available.");
    }

    private void SetStatusMessage(string message)
    {
        _statusTextBox.Text = message;
        _summaryTextBox.Text = message;
        _reportTextBox.Text = message;
    }

    private void TryOpenPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            MessageBox.Show(this, ex.Message, "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
