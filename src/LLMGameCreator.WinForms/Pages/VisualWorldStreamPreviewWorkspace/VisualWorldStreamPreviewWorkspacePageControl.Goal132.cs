using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private readonly GamePackageCandidatePipelineOperatorService _goal132OperatorService = new();
    private TabPage? _goal132OperatorTabPage;
    private Label? _goal132OperatorHeaderLabel;
    private TextBox? _goal132OperatorStatusTextBox;
    private TextBox? _goal132OperatorCommandTextBox;
    private TextBox? _goal132OperatorResultPathTextBox;
    private TextBox? _goal132OperatorOutputTailTextBox;
    private Button? _goal132RefreshButton;
    private Button? _goal132CopyCommandButton;
    private Button? _goal132DryRunButton;
    private Button? _goal132RunButton;

    private void ConfigureGoal132CandidatePipelineOperatorPanel()
    {
        _goal132OperatorTabPage = new TabPage
        {
            Name = "_goal132OperatorTabPage",
            Text = "Candidate Pipeline Operator"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));

        _goal132OperatorHeaderLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal132 Candidate Pipeline Operator"
        };
        _goal132OperatorStatusTextBox = Goal132ReadOnlyTextBox(multiline: true);
        _goal132OperatorCommandTextBox = Goal132ReadOnlyTextBox(multiline: false);
        _goal132OperatorResultPathTextBox = Goal132ReadOnlyTextBox(multiline: false);
        _goal132OperatorOutputTailTextBox = Goal132ReadOnlyTextBox(multiline: true);

        var commandLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        commandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        commandLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        commandLayout.Controls.Add(_goal132OperatorCommandTextBox, 0, 0);
        commandLayout.Controls.Add(_goal132OperatorResultPathTextBox, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 4, 0, 0)
        };
        _goal132RefreshButton = Goal132Button("Refresh Candidate Pipeline Status");
        _goal132CopyCommandButton = Goal132Button("Copy Candidate Pipeline Command");
        _goal132DryRunButton = Goal132Button("Dry Run Candidate Recipe Pipeline");
        _goal132RunButton = Goal132Button("Run Candidate Recipe Pipeline");
        buttons.Controls.Add(_goal132RefreshButton);
        buttons.Controls.Add(_goal132CopyCommandButton);
        buttons.Controls.Add(_goal132DryRunButton);
        buttons.Controls.Add(_goal132RunButton);

        layout.Controls.Add(_goal132OperatorHeaderLabel, 0, 0);
        layout.Controls.Add(_goal132OperatorStatusTextBox, 0, 1);
        layout.Controls.Add(commandLayout, 0, 2);
        layout.Controls.Add(buttons, 0, 3);
        layout.Controls.Add(_goal132OperatorOutputTailTextBox, 0, 4);
        _goal132OperatorTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal132OperatorTabPage);
    }

    private void WireGoal132CandidatePipelineOperatorEvents()
    {
        if (_goal132RefreshButton is not null)
        {
            _goal132RefreshButton.Click += async (_, _) =>
                await RefreshGoal132CandidatePipelineStatusAsync();
        }

        if (_goal132CopyCommandButton is not null)
        {
            _goal132CopyCommandButton.Click += (_, _) =>
                Clipboard.SetText(GamePackageCandidatePipelineOperatorVocabulary.NormalCommand);
        }

        if (_goal132DryRunButton is not null)
        {
            _goal132DryRunButton.Click += async (_, _) =>
                await RunGoal132CandidatePipelineAsync(dryRun: true);
        }

        if (_goal132RunButton is not null)
        {
            _goal132RunButton.Click += async (_, _) =>
                await RunGoal132CandidatePipelineAsync(dryRun: false);
        }
    }

    private void BindGoal132CandidatePipelineOperator(VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal132OperatorStatusTextBox is null
            || _goal132OperatorCommandTextBox is null
            || _goal132OperatorResultPathTextBox is null
            || _goal132OperatorOutputTailTextBox is null)
        {
            return;
        }

        _goal132OperatorStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "operatorStatus=" + result.Report.CandidatePipelineOperatorStatus,
            "selectedCandidateId=" + result.Report.CandidatePipelineOperatorSelectedCandidateId,
            "selectedCandidateScore=" + result.Report.CandidatePipelineOperatorSelectedCandidateScore,
            "candidateCount=" + result.Report.CandidatePipelineOperatorCandidateCount,
            "passedCandidates=" + result.Report.CandidatePipelineOperatorPassedCandidates,
            "failedCandidates=" + result.Report.CandidatePipelineOperatorFailedCandidates,
            "matrixPassed=" + result.Report.CandidatePipelineOperatorMatrixPassed.ToString().ToLowerInvariant(),
            "lastOperatorExitCode=" + result.Report.CandidatePipelineOperatorLastExitCode,
            "lastOperatorDurationMilliseconds="
                + result.Report.CandidatePipelineOperatorLastDurationMilliseconds,
            "manualUnityOptional="
                + result.Report.CandidatePipelineOperatorManualUnityOptional.ToString().ToLowerInvariant()
        ]);
        _goal132OperatorCommandTextBox.Text =
            "normalCommand=" + result.Report.CandidatePipelineOperatorNormalCommand;
        _goal132OperatorResultPathTextBox.Text =
            "resultPath=" + result.Report.CandidatePipelineOperatorResultPath;
        _goal132OperatorOutputTailTextBox.Text =
            string.IsNullOrWhiteSpace(result.Report.CandidatePipelineOperatorOutputTail)
                ? "No Goal132 operator output tail captured yet."
                : result.Report.CandidatePipelineOperatorOutputTail;
    }

    private async Task RefreshGoal132CandidatePipelineStatusAsync()
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            Goal132SetStatus("Repository root was not found.");
            return;
        }

        try
        {
            await _goal132OperatorService.BuildAndWriteAsync(root);
            RefreshWorkspace();
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or JsonException)
        {
            Goal132SetStatus("Candidate pipeline status refresh failed: " + ex.Message);
        }
    }

    private async Task RunGoal132CandidatePipelineAsync(bool dryRun)
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            Goal132SetStatus("Repository root was not found.");
            return;
        }

        Goal132SetRunning(true);
        var command = dryRun
            ? GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand
            : GamePackageCandidatePipelineOperatorVocabulary.FullRunCommand;
        var stopwatch = Stopwatch.StartNew();
        var output = new StringBuilder();
        var error = new StringBuilder();
        var exitCode = -1;

        try
        {
            Goal132SetStatus("running command=" + command);
            using var process = new Process
            {
                StartInfo = CreateGoal132PipelineProcessStartInfo(root, dryRun),
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    lock (output)
                    {
                        output.AppendLine(args.Data);
                    }
                }
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    lock (error)
                    {
                        error.AppendLine(args.Data);
                    }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            exitCode = process.ExitCode;
            stopwatch.Stop();

            await _goal132OperatorService.WriteOperatorResultAsync(
                root,
                new GamePackageCandidatePipelineOperatorRunResultInput
                {
                    RunMode = dryRun ? "dryRun" : "fullRun",
                    Command = command,
                    ExitCode = exitCode,
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds,
                    OutputTail = BuildGoal132Tail(output.ToString()),
                    ErrorTail = BuildGoal132Tail(error.ToString())
                });
            RefreshWorkspace();
            Goal132SetStatus("completed exitCode=" + exitCode);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException)
        {
            stopwatch.Stop();
            await _goal132OperatorService.WriteOperatorResultAsync(
                root,
                new GamePackageCandidatePipelineOperatorRunResultInput
                {
                    RunMode = dryRun ? "dryRun" : "fullRun",
                    Command = command,
                    ExitCode = exitCode,
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds,
                    OutputTail = BuildGoal132Tail(output.ToString()),
                    ErrorTail = BuildGoal132Tail(error + Environment.NewLine + ex.Message)
                });
            RefreshWorkspace();
            Goal132SetStatus("failed exitCode=" + exitCode + "; " + ex.Message);
        }
        finally
        {
            Goal132SetRunning(false);
        }
    }

    private static ProcessStartInfo CreateGoal132PipelineProcessStartInfo(string root, bool dryRun)
    {
        var script = Path.Combine(
            root,
            GamePackageCandidatePipelineOperatorVocabulary.PipelineScriptPath
                .Replace('/', Path.DirectorySeparatorChar));
        var startInfo = new ProcessStartInfo("powershell")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(script);
        startInfo.ArgumentList.Add(dryRun ? "-DryRun" : "-ApplyCleanup");
        return startInfo;
    }

    private void Goal132SetRunning(bool running)
    {
        if (_goal132RefreshButton is not null)
        {
            _goal132RefreshButton.Enabled = !running;
        }
        if (_goal132CopyCommandButton is not null)
        {
            _goal132CopyCommandButton.Enabled = !running;
        }
        if (_goal132DryRunButton is not null)
        {
            _goal132DryRunButton.Enabled = !running;
        }
        if (_goal132RunButton is not null)
        {
            _goal132RunButton.Enabled = !running;
        }
    }

    private void Goal132SetStatus(string text)
    {
        if (_goal132OperatorStatusTextBox is null)
        {
            return;
        }

        if (_goal132OperatorStatusTextBox.InvokeRequired)
        {
            _goal132OperatorStatusTextBox.BeginInvoke(() => Goal132SetStatus(text));
            return;
        }

        _goal132OperatorStatusTextBox.Text = text;
    }

    private static TextBox Goal132ReadOnlyTextBox(bool multiline) =>
        new()
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10F),
            Multiline = multiline,
            ReadOnly = true,
            ScrollBars = multiline ? ScrollBars.Both : ScrollBars.Horizontal,
            WordWrap = false
        };

    private static Button Goal132Button(string text) =>
        new()
        {
            AutoSize = true,
            Text = text,
            UseVisualStyleBackColor = true
        };

    private static string BuildGoal132Tail(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            text.Replace("\r\n", "\n").Replace('\r', '\n')
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .TakeLast(80));
    }

    private static IReadOnlyList<string> BuildCandidatePipelineOperatorDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "operatorStatus=" + result.Report.CandidatePipelineOperatorStatus,
        "normalCommand=" + result.Report.CandidatePipelineOperatorNormalCommand,
        "dryRunCommand=" + result.Report.CandidatePipelineOperatorDryRunCommand,
        "resultPath=" + result.Report.CandidatePipelineOperatorResultPath,
        "selectedCandidateId=" + result.Report.CandidatePipelineOperatorSelectedCandidateId,
        "selectedCandidateScore=" + result.Report.CandidatePipelineOperatorSelectedCandidateScore,
        "candidateCount=" + result.Report.CandidatePipelineOperatorCandidateCount,
        "passedCandidates=" + result.Report.CandidatePipelineOperatorPassedCandidates,
        "failedCandidates=" + result.Report.CandidatePipelineOperatorFailedCandidates,
        "matrixPassed=" + result.Report.CandidatePipelineOperatorMatrixPassed.ToString().ToLowerInvariant(),
        "lastOperatorExitCode=" + result.Report.CandidatePipelineOperatorLastExitCode,
        "manualUnityOptional="
            + result.Report.CandidatePipelineOperatorManualUnityOptional.ToString().ToLowerInvariant(),
        "projectionOnly=" + result.Report.CandidatePipelineOperatorProjectionOnly.ToString().ToLowerInvariant(),
        "samplePackageReadOnly="
            + result.Report.CandidatePipelineOperatorSamplePackageReadOnly.ToString().ToLowerInvariant(),
        "winFormsPanelPresent="
            + result.Report.CandidatePipelineOperatorWinFormsPanelPresent.ToString().ToLowerInvariant(),
        "asyncRunPresent="
            + result.Report.CandidatePipelineOperatorAsyncRunPresent.ToString().ToLowerInvariant(),
        "candidatePipelineOperatorQualityGatePassed="
            + result.Report.CandidatePipelineOperatorQualityGatePassed.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildCandidatePipelineOperatorEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "operatorStatus: " + entry.CandidatePipelineOperatorStatus,
        "normalCommand: " + entry.CandidatePipelineOperatorNormalCommand,
        "dryRunCommand: " + entry.CandidatePipelineOperatorDryRunCommand,
        "resultPath: " + entry.CandidatePipelineOperatorResultPath,
        "selectedCandidateId: " + entry.CandidatePipelineOperatorSelectedCandidateId,
        "selectedCandidateScore: " + entry.CandidatePipelineOperatorSelectedCandidateScore,
        "candidateCount: " + entry.CandidatePipelineOperatorCandidateCount,
        "passedCandidates: " + entry.CandidatePipelineOperatorPassedCandidates,
        "failedCandidates: " + entry.CandidatePipelineOperatorFailedCandidates,
        "matrixPassed: " + entry.CandidatePipelineOperatorMatrixPassed.ToString().ToLowerInvariant(),
        "lastOperatorExitCode: " + entry.CandidatePipelineOperatorLastExitCode,
        "manualUnityOptional: "
            + entry.CandidatePipelineOperatorManualUnityOptional.ToString().ToLowerInvariant(),
        "projectionOnly: " + entry.CandidatePipelineOperatorProjectionOnly.ToString().ToLowerInvariant(),
        "samplePackageReadOnly: "
            + entry.CandidatePipelineOperatorSamplePackageReadOnly.ToString().ToLowerInvariant(),
        "winFormsPanelPresent: "
            + entry.CandidatePipelineOperatorWinFormsPanelPresent.ToString().ToLowerInvariant(),
        "asyncRunPresent: " + entry.CandidatePipelineOperatorAsyncRunPresent.ToString().ToLowerInvariant()
    ];
}
