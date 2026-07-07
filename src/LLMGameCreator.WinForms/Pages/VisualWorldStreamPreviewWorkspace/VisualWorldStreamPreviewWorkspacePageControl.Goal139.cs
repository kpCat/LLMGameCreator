using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal139RuntimeBackedControlsTabPage;
    private TextBox? _goal139RuntimeBackedControlsStatusTextBox;
    private TextBox? _goal139RuntimeBackedControlsCommandTextBox;
    private TextBox? _goal139RuntimeBackedControlsReportPathTextBox;

    private void ConfigureGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsPanel()
    {
        _goal139RuntimeBackedControlsTabPage = new TabPage
        {
            Name = "_goal139RuntimeBackedControlsTabPage",
            Text = "Controls"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 280F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal139 Controls"
        };
        _goal139RuntimeBackedControlsStatusTextBox = Goal139ReadOnlyTextBox(multiline: true);
        _goal139RuntimeBackedControlsCommandTextBox = Goal139ReadOnlyTextBox(multiline: false);
        _goal139RuntimeBackedControlsReportPathTextBox = Goal139ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal139RuntimeBackedControlsStatusTextBox, 0, 1);
        layout.Controls.Add(_goal139RuntimeBackedControlsCommandTextBox, 0, 2);
        layout.Controls.Add(_goal139RuntimeBackedControlsReportPathTextBox, 0, 3);
        _goal139RuntimeBackedControlsTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal139RuntimeBackedControlsTabPage);
    }

    private void BindGoal139RuntimeBackedUnityPlayerLoopInteractiveControls(
        VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal139RuntimeBackedControlsStatusTextBox is null
            || _goal139RuntimeBackedControlsCommandTextBox is null
            || _goal139RuntimeBackedControlsReportPathTextBox is null)
        {
            return;
        }

        _goal139RuntimeBackedControlsStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "acceptedGoal138="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138
                    .ToString().ToLowerInvariant(),
            "candidateId=" + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId,
            "frameCount=" + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount,
            "requiredControlsPresent="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent
                    .ToString().ToLowerInvariant(),
            "controlScriptPassed="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed
                    .ToString().ToLowerInvariant(),
            "interactiveControlsWindowPresent="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent
                    .ToString().ToLowerInvariant(),
            "unityInteractiveControlsSmokePassed="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed
                    .ToString().ToLowerInvariant(),
            "runtimeAuthority="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority
                    .ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly
                    .ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional
                    .ToString().ToLowerInvariant(),
            "accepted="
                + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted
                    .ToString().ToLowerInvariant()
        ]);
        _goal139RuntimeBackedControlsCommandTextBox.Text =
            "normalCommand="
            + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand;
        _goal139RuntimeBackedControlsReportPathTextBox.Text =
            "reportPath="
            + result.Report.RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath;
    }

    private static TextBox Goal139ReadOnlyTextBox(bool multiline) =>
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
