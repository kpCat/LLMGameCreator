using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using LLMGameCreator.Runtime;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private readonly ProductLineRuntimeVariantMatrixOperatorRunner _goal142OperatorRunner =
        new(new ProductLineRuntimeVariantMatrixService(
            RuntimeBackedPlayerCommandRoundtripService.CreateDefault()));
    private TabPage? _goal142RuntimeVariantMatrixTabPage;
    private TextBox? _goal142RuntimeVariantMatrixStatusTextBox;
    private TextBox? _goal142RuntimeVariantMatrixCommandTextBox;
    private TextBox? _goal142RuntimeVariantMatrixPathTextBox;
    private TextBox? _goal142RuntimeVariantMatrixOutputTailTextBox;
    private Button? _goal142RunButton;

    private void ConfigureGoal142ProductLineRuntimeVariantMatrixPanel()
    {
        _goal142RuntimeVariantMatrixTabPage = new TabPage
        {
            Name = "_goal142RuntimeVariantMatrixTabPage",
            Text = "Goal142 Variants"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal142 Runtime Variants"
        };
        _goal142RuntimeVariantMatrixStatusTextBox = Goal132ReadOnlyTextBox(multiline: true);
        _goal142RuntimeVariantMatrixCommandTextBox = Goal132ReadOnlyTextBox(multiline: false);
        _goal142RuntimeVariantMatrixPathTextBox = Goal132ReadOnlyTextBox(multiline: false);
        _goal142RuntimeVariantMatrixOutputTailTextBox = Goal132ReadOnlyTextBox(multiline: true);
        _goal142RunButton = Goal132Button("Run Runtime Variant Matrix");

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 4, 0, 0)
        };
        buttons.Controls.Add(_goal142RunButton);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal142RuntimeVariantMatrixStatusTextBox, 0, 1);
        layout.Controls.Add(_goal142RuntimeVariantMatrixCommandTextBox, 0, 2);
        layout.Controls.Add(_goal142RuntimeVariantMatrixPathTextBox, 0, 3);
        layout.Controls.Add(buttons, 0, 4);
        layout.Controls.Add(_goal142RuntimeVariantMatrixOutputTailTextBox, 0, 5);
        _goal142RuntimeVariantMatrixTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal142RuntimeVariantMatrixTabPage);
    }

    private void WireGoal142ProductLineRuntimeVariantMatrixEvents()
    {
        if (_goal142RunButton is not null)
        {
            _goal142RunButton.Click += async (_, _) => await RunGoal142RuntimeVariantMatrixAsync();
        }
    }

    private void BindGoal142ProductLineRuntimeVariantMatrix(
        VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal142RuntimeVariantMatrixStatusTextBox is null
            || _goal142RuntimeVariantMatrixCommandTextBox is null
            || _goal142RuntimeVariantMatrixPathTextBox is null
            || _goal142RuntimeVariantMatrixOutputTailTextBox is null)
        {
            return;
        }

        _goal142RuntimeVariantMatrixStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "matrixStatus=" + result.Report.ProductLineRuntimeVariantMatrixStatus,
            "candidateCount=" + result.Report.ProductLineRuntimeVariantCandidateCount,
            "passedCandidateCount=" + result.Report.ProductLineRuntimeVariantPassedCandidateCount,
            "failedCandidateCount=" + result.Report.ProductLineRuntimeVariantFailedCandidateCount,
            "runtimeSignificantCandidateCount="
                + result.Report.ProductLineRuntimeVariantRuntimeSignificantCandidateCount,
            "distinctFinalStateHashCount="
                + result.Report.ProductLineRuntimeVariantDistinctFinalStateHashCount,
            "selectedCandidateId=" + result.Report.ProductLineRuntimeVariantSelectedCandidateId,
            "selectedVariantKind=" + result.Report.ProductLineRuntimeVariantSelectedVariantKind,
            "selectedScore=" + result.Report.ProductLineRuntimeVariantSelectedScore,
            "sourceTemplateUnmodified="
                + result.Report.ProductLineRuntimeVariantSourceTemplateUnmodified
                    .ToString().ToLowerInvariant(),
            "accepted=" + result.Report.ProductLineRuntimeVariantAccepted.ToString().ToLowerInvariant()
        ]);
        _goal142RuntimeVariantMatrixCommandTextBox.Text =
            "normalCommand=" + result.Report.ProductLineRuntimeVariantNormalCommand;
        _goal142RuntimeVariantMatrixPathTextBox.Text =
            "matrixResultPath=" + result.Report.ProductLineRuntimeVariantMatrixResultPath
            + "; selectedHandoffPath=" + result.Report.ProductLineRuntimeVariantSelectedHandoffPath;
        if (string.IsNullOrWhiteSpace(_goal142RuntimeVariantMatrixOutputTailTextBox.Text))
        {
            _goal142RuntimeVariantMatrixOutputTailTextBox.Text =
                "No Goal142RuntimeVariantMatrixOutputTail captured yet.";
        }
    }

    private async Task RunGoal142RuntimeVariantMatrixAsync()
    {
        var root = FindProjectRoot();
        if (root is null)
        {
            Goal142SetStatus("Repository root was not found.");
            return;
        }

        Goal142SetRunning(true);
        var exitCode = -1;
        try
        {
            Goal142SetStatus("running inProcess=true");
            var result = await Task.Run(() => _goal142OperatorRunner.RunAsync(root));
            exitCode = string.Equals(
                result.Dashboard.MatrixStatus,
                "GREEN",
                StringComparison.Ordinal)
                ? 0
                : 1;
            Goal142SetOutputTail(
                string.Join(Environment.NewLine,
                [
                    "operatorUsesInProcessService=true",
                    "operatorStartsCompilerProcess=false",
                    "operatorStartsDotnetTestProcess=false",
                    "matrixStatus=" + result.Dashboard.MatrixStatus,
                    "candidateCount=" + result.Dashboard.CandidateCount,
                    "passedCandidateCount=" + result.Dashboard.PassedCandidateCount,
                    "distinctFinalStateHashCount=" + result.Dashboard.DistinctFinalStateHashCount,
                    "selectedCandidateId=" + result.Dashboard.SelectedCandidateId,
                    "sourceTemplateUnmodified="
                        + result.Dashboard.SourceTemplateUnmodified.ToString().ToLowerInvariant(),
                    "accepted=" + result.Dashboard.Accepted.ToString().ToLowerInvariant()
                ]),
                string.Empty);
            RefreshWorkspace();
            Goal142SetStatus("completed exitCode=" + exitCode);
        }
        catch (Exception ex)
        {
            exitCode = 1;
            Goal142SetOutputTail(
                "operatorUsesInProcessService=true",
                Goal142RuntimeVariantMatrixOutputTail(ex.Message));
            Goal142SetStatus("failed exitCode=" + exitCode + "; " + ex.Message);
        }
        finally
        {
            Goal142SetRunning(false);
        }
    }

    private void Goal142SetRunning(bool running)
    {
        if (_goal142RunButton is not null)
        {
            _goal142RunButton.Enabled = !running;
        }
    }

    private void Goal142SetStatus(string text)
    {
        if (_goal142RuntimeVariantMatrixStatusTextBox is null)
        {
            return;
        }

        if (_goal142RuntimeVariantMatrixStatusTextBox.InvokeRequired)
        {
            _goal142RuntimeVariantMatrixStatusTextBox.BeginInvoke(() => Goal142SetStatus(text));
            return;
        }

        _goal142RuntimeVariantMatrixStatusTextBox.Text = text;
    }

    private void Goal142SetOutputTail(string output, string error)
    {
        if (_goal142RuntimeVariantMatrixOutputTailTextBox is null)
        {
            return;
        }

        var tail = Goal142RuntimeVariantMatrixOutputTail(output)
                   + Environment.NewLine
                   + Goal142RuntimeVariantMatrixOutputTail(error);
        _goal142RuntimeVariantMatrixOutputTailTextBox.Text = tail.Trim();
    }

    private static string Goal142RuntimeVariantMatrixOutputTail(string text)
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

    private static IReadOnlyList<string> BuildProductLineRuntimeVariantMatrixDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "matrixStatus=" + result.Report.ProductLineRuntimeVariantMatrixStatus,
        "normalCommand=" + result.Report.ProductLineRuntimeVariantNormalCommand,
        "matrixResultPath=" + result.Report.ProductLineRuntimeVariantMatrixResultPath,
        "selectedHandoffPath=" + result.Report.ProductLineRuntimeVariantSelectedHandoffPath,
        "candidateCount=" + result.Report.ProductLineRuntimeVariantCandidateCount,
        "passedCandidateCount=" + result.Report.ProductLineRuntimeVariantPassedCandidateCount,
        "failedCandidateCount=" + result.Report.ProductLineRuntimeVariantFailedCandidateCount,
        "runtimeSignificantCandidateCount="
            + result.Report.ProductLineRuntimeVariantRuntimeSignificantCandidateCount,
        "distinctFinalStateHashCount="
            + result.Report.ProductLineRuntimeVariantDistinctFinalStateHashCount,
        "selectedCandidateId=" + result.Report.ProductLineRuntimeVariantSelectedCandidateId,
        "selectedVariantKind=" + result.Report.ProductLineRuntimeVariantSelectedVariantKind,
        "selectedScore=" + result.Report.ProductLineRuntimeVariantSelectedScore,
        "sourceTemplateUnmodified="
            + result.Report.ProductLineRuntimeVariantSourceTemplateUnmodified.ToString().ToLowerInvariant(),
        "accepted=" + result.Report.ProductLineRuntimeVariantAccepted.ToString().ToLowerInvariant()
    ];
}
