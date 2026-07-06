using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal134CanonicalRuntimeTabPage;
    private TextBox? _goal134CanonicalRuntimeStatusTextBox;
    private TextBox? _goal134CanonicalRuntimeCommandTextBox;
    private TextBox? _goal134CanonicalRuntimeReportPathTextBox;
    private TextBox? _goal134CanonicalRuntimeMatrixPathTextBox;

    private void ConfigureGoal134CanonicalRuntimePanel()
    {
        _goal134CanonicalRuntimeTabPage = new TabPage
        {
            Name = "_goal134CanonicalRuntimeTabPage",
            Text = "Canonical Runtime"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal134 Canonical Runtime"
        };
        _goal134CanonicalRuntimeStatusTextBox = Goal134ReadOnlyTextBox(multiline: true);
        _goal134CanonicalRuntimeCommandTextBox = Goal134ReadOnlyTextBox(multiline: false);
        _goal134CanonicalRuntimeReportPathTextBox = Goal134ReadOnlyTextBox(multiline: false);
        _goal134CanonicalRuntimeMatrixPathTextBox = Goal134ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal134CanonicalRuntimeStatusTextBox, 0, 1);
        layout.Controls.Add(_goal134CanonicalRuntimeCommandTextBox, 0, 2);
        layout.Controls.Add(_goal134CanonicalRuntimeReportPathTextBox, 0, 3);
        layout.Controls.Add(_goal134CanonicalRuntimeMatrixPathTextBox, 0, 4);
        _goal134CanonicalRuntimeTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal134CanonicalRuntimeTabPage);
    }

    private void BindGoal134CanonicalRuntime(VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal134CanonicalRuntimeStatusTextBox is null
            || _goal134CanonicalRuntimeCommandTextBox is null
            || _goal134CanonicalRuntimeReportPathTextBox is null
            || _goal134CanonicalRuntimeMatrixPathTextBox is null)
        {
            return;
        }

        _goal134CanonicalRuntimeStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "candidateId=" + result.Report.CanonicalRuntimeCandidateId,
            "packageValidationPassed="
                + result.Report.CanonicalRuntimePackageValidationPassed.ToString().ToLowerInvariant(),
            "canonicalRuntimePassed="
                + result.Report.CanonicalRuntimePassed.ToString().ToLowerInvariant(),
            "runtimeCommandCount=" + result.Report.CanonicalRuntimeCommandCount,
            "runtimeEventCount=" + result.Report.CanonicalRuntimeEventCount,
            "saveLoadReplayPassed="
                + result.Report.CanonicalRuntimeSaveLoadReplayPassed.ToString().ToLowerInvariant(),
            "unityPlayerConsumedCanonicalTranscript="
                + result.Report.CanonicalRuntimeUnityPlayerConsumedTranscript.ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.CanonicalRuntimeProjectionOnly.ToString().ToLowerInvariant(),
            "selectedCandidateExecutedByRuntime="
                + result.Report.CanonicalRuntimeSelectedCandidateExecutedByRuntime.ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.CanonicalRuntimeManualUnityOptional.ToString().ToLowerInvariant()
        ]);
        _goal134CanonicalRuntimeCommandTextBox.Text =
            "normalCommand=" + result.Report.CanonicalRuntimeNormalCommand;
        _goal134CanonicalRuntimeReportPathTextBox.Text =
            "reportPath=" + result.Report.CanonicalRuntimeReportPath;
        _goal134CanonicalRuntimeMatrixPathTextBox.Text =
            "matrixResultPath=" + result.Report.CanonicalRuntimeMatrixResultPath;
    }

    private static TextBox Goal134ReadOnlyTextBox(bool multiline) =>
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
