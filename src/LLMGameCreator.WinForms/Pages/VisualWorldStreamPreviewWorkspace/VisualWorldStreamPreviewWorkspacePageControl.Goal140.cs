using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal140RuntimeBackedControlsUxTabPage;
    private TextBox? _goal140RuntimeBackedControlsUxStatusTextBox;
    private TextBox? _goal140RuntimeBackedControlsUxCommandTextBox;
    private TextBox? _goal140RuntimeBackedControlsUxReportPathTextBox;

    private void ConfigureGoal140RuntimeBackedUnityPlayerLoopControlsUxPanel()
    {
        _goal140RuntimeBackedControlsUxTabPage = new TabPage
        {
            Name = "_goal140RuntimeBackedControlsUxTabPage",
            Text = "Goal140 UX"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 320F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal140 Controls UX"
        };
        _goal140RuntimeBackedControlsUxStatusTextBox = Goal139ReadOnlyTextBox(multiline: true);
        _goal140RuntimeBackedControlsUxCommandTextBox = Goal139ReadOnlyTextBox(multiline: false);
        _goal140RuntimeBackedControlsUxReportPathTextBox = Goal139ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal140RuntimeBackedControlsUxStatusTextBox, 0, 1);
        layout.Controls.Add(_goal140RuntimeBackedControlsUxCommandTextBox, 0, 2);
        layout.Controls.Add(_goal140RuntimeBackedControlsUxReportPathTextBox, 0, 3);
        _goal140RuntimeBackedControlsUxTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal140RuntimeBackedControlsUxTabPage);
    }

    private void BindGoal140RuntimeBackedUnityPlayerLoopControlsUx(
        VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal140RuntimeBackedControlsUxStatusTextBox is null
            || _goal140RuntimeBackedControlsUxCommandTextBox is null
            || _goal140RuntimeBackedControlsUxReportPathTextBox is null)
        {
            return;
        }

        _goal140RuntimeBackedControlsUxStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "acceptedGoal139="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139
                    .ToString().ToLowerInvariant(),
            "selectedCandidateId="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate,
            "frameCount=" + result.Report.RuntimeBackedUnityPlayerLoopControlsUxFrameCount,
            "humanReadableFrameNumbering="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering
                    .ToString().ToLowerInvariant(),
            "stepOnceSemanticsClear="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear
                    .ToString().ToLowerInvariant(),
            "playAllToEndSemanticsClear="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear
                    .ToString().ToLowerInvariant(),
            "knownUnityEditorNoiseClassified="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified
                    .ToString().ToLowerInvariant(),
            "blockingUnityErrorCount="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount,
            "unclassifiedUnityErrorCount="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount,
            "unityControlsUxSmokePassed="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed
                    .ToString().ToLowerInvariant(),
            "runtimeAuthority="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority
                    .ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly
                    .ToString().ToLowerInvariant(),
            "accepted="
                + result.Report.RuntimeBackedUnityPlayerLoopControlsUxAccepted
                    .ToString().ToLowerInvariant()
        ]);
        _goal140RuntimeBackedControlsUxCommandTextBox.Text =
            "normalCommand="
            + result.Report.RuntimeBackedUnityPlayerLoopControlsUxNormalCommand;
        _goal140RuntimeBackedControlsUxReportPathTextBox.Text =
            "reportPath="
            + result.Report.RuntimeBackedUnityPlayerLoopControlsUxReportPath;
    }
}
