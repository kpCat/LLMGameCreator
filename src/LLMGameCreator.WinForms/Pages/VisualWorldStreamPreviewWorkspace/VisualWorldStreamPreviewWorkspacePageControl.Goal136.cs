using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal136PlayerCommandLoopTabPage;
    private TextBox? _goal136PlayerCommandLoopStatusTextBox;
    private TextBox? _goal136PlayerCommandLoopCommandTextBox;
    private TextBox? _goal136PlayerCommandLoopReportPathTextBox;
    private TextBox? _goal136PlayerCommandLoopMatrixPathTextBox;

    private void ConfigureGoal136PlayerCommandLoopPanel()
    {
        _goal136PlayerCommandLoopTabPage = new TabPage
        {
            Name = "_goal136PlayerCommandLoopTabPage",
            Text = "Command Loop"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 230F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal136 Command Loop"
        };
        _goal136PlayerCommandLoopStatusTextBox = Goal136ReadOnlyTextBox(multiline: true);
        _goal136PlayerCommandLoopCommandTextBox = Goal136ReadOnlyTextBox(multiline: false);
        _goal136PlayerCommandLoopReportPathTextBox = Goal136ReadOnlyTextBox(multiline: false);
        _goal136PlayerCommandLoopMatrixPathTextBox = Goal136ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal136PlayerCommandLoopStatusTextBox, 0, 1);
        layout.Controls.Add(_goal136PlayerCommandLoopCommandTextBox, 0, 2);
        layout.Controls.Add(_goal136PlayerCommandLoopReportPathTextBox, 0, 3);
        layout.Controls.Add(_goal136PlayerCommandLoopMatrixPathTextBox, 0, 4);
        _goal136PlayerCommandLoopTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal136PlayerCommandLoopTabPage);
    }

    private void BindGoal136PlayerCommandLoop(VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal136PlayerCommandLoopStatusTextBox is null
            || _goal136PlayerCommandLoopCommandTextBox is null
            || _goal136PlayerCommandLoopReportPathTextBox is null
            || _goal136PlayerCommandLoopMatrixPathTextBox is null)
        {
            return;
        }

        _goal136PlayerCommandLoopStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "candidateId=" + result.Report.CanonicalRuntimePlayerCommandLoopCandidateId,
            "playerCommandLoopPassed="
                + result.Report.CanonicalRuntimePlayerCommandLoopPassed
                    .ToString().ToLowerInvariant(),
            "playerCommandCount="
                + result.Report.CanonicalRuntimePlayerCommandCount,
            "snapshotCount="
                + result.Report.CanonicalRuntimePlayerSnapshotCount,
            "runtimeEventCount="
                + result.Report.CanonicalRuntimePlayerCommandLoopRuntimeEventCount,
            "allRequiredCategoriesPresent="
                + result.Report.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent
                    .ToString().ToLowerInvariant(),
            "unityPlayerConsumedCommandLoopSnapshots="
                + result.Report.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.CanonicalRuntimePlayerCommandLoopProjectionOnly
                    .ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "noUnclassifiedErrorDiagnostics="
                + result.Report.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors
                    .ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.CanonicalRuntimePlayerCommandLoopManualUnityOptional
                    .ToString().ToLowerInvariant(),
            "accepted="
                + result.Report.CanonicalRuntimePlayerCommandLoopAccepted
                    .ToString().ToLowerInvariant()
        ]);
        _goal136PlayerCommandLoopCommandTextBox.Text =
            "normalCommand=" + result.Report.CanonicalRuntimePlayerCommandLoopNormalCommand;
        _goal136PlayerCommandLoopReportPathTextBox.Text =
            "reportPath=" + result.Report.CanonicalRuntimePlayerCommandLoopReportPath;
        _goal136PlayerCommandLoopMatrixPathTextBox.Text =
            "matrixResultPath=" + result.Report.CanonicalRuntimePlayerCommandLoopMatrixResultPath;
    }

    private static TextBox Goal136ReadOnlyTextBox(bool multiline) =>
        new()
        {
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Multiline = multiline,
            ReadOnly = true,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
            WordWrap = multiline
        };
}
