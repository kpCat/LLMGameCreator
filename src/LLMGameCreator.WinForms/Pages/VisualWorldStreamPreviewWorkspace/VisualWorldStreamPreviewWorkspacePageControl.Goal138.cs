using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal138RuntimeBackedStepperTabPage;
    private TextBox? _goal138RuntimeBackedStepperStatusTextBox;
    private TextBox? _goal138RuntimeBackedStepperCommandTextBox;
    private TextBox? _goal138RuntimeBackedStepperReportPathTextBox;

    private void ConfigureGoal138RuntimeBackedUnityPlayerLoopStepperPanel()
    {
        _goal138RuntimeBackedStepperTabPage = new TabPage
        {
            Name = "_goal138RuntimeBackedStepperTabPage",
            Text = "Stepper"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 260F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal138 Stepper"
        };
        _goal138RuntimeBackedStepperStatusTextBox = Goal138ReadOnlyTextBox(multiline: true);
        _goal138RuntimeBackedStepperCommandTextBox = Goal138ReadOnlyTextBox(multiline: false);
        _goal138RuntimeBackedStepperReportPathTextBox = Goal138ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal138RuntimeBackedStepperStatusTextBox, 0, 1);
        layout.Controls.Add(_goal138RuntimeBackedStepperCommandTextBox, 0, 2);
        layout.Controls.Add(_goal138RuntimeBackedStepperReportPathTextBox, 0, 3);
        _goal138RuntimeBackedStepperTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal138RuntimeBackedStepperTabPage);
    }

    private void BindGoal138RuntimeBackedUnityPlayerLoopStepper(
        VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal138RuntimeBackedStepperStatusTextBox is null
            || _goal138RuntimeBackedStepperCommandTextBox is null
            || _goal138RuntimeBackedStepperReportPathTextBox is null)
        {
            return;
        }

        _goal138RuntimeBackedStepperStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "acceptedGoal137="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137
                    .ToString().ToLowerInvariant(),
            "candidateId=" + result.Report.RuntimeBackedUnityPlayerLoopStepperCandidateId,
            "frameCount=" + result.Report.RuntimeBackedUnityPlayerLoopStepperFrameCount,
            "requiredFrameCategoriesPresent="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent
                    .ToString().ToLowerInvariant(),
            "runtimeAuthority="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority
                    .ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperProjectionOnly
                    .ToString().ToLowerInvariant(),
            "stepperWindowPresent="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperWindowPresent
                    .ToString().ToLowerInvariant(),
            "stepperBatchSmokePassed="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed
                    .ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperManualUnityOptional
                    .ToString().ToLowerInvariant(),
            "accepted="
                + result.Report.RuntimeBackedUnityPlayerLoopStepperAccepted
                    .ToString().ToLowerInvariant()
        ]);
        _goal138RuntimeBackedStepperCommandTextBox.Text =
            "normalCommand=" + result.Report.RuntimeBackedUnityPlayerLoopStepperNormalCommand;
        _goal138RuntimeBackedStepperReportPathTextBox.Text =
            "reportPath=" + result.Report.RuntimeBackedUnityPlayerLoopStepperReportPath;
    }

    private static TextBox Goal138ReadOnlyTextBox(bool multiline) =>
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
