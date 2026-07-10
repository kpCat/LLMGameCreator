using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal141RuntimeBackedCommandRoundtripTabPage;
    private TextBox? _goal141RuntimeBackedCommandRoundtripStatusTextBox;
    private TextBox? _goal141RuntimeBackedCommandRoundtripCommandTextBox;
    private TextBox? _goal141RuntimeBackedCommandRoundtripReportPathTextBox;

    private void ConfigureGoal141RuntimeBackedPlayerCommandRoundtripPanel()
    {
        _goal141RuntimeBackedCommandRoundtripTabPage = new TabPage
        {
            Name = "_goal141RuntimeBackedCommandRoundtripTabPage",
            Text = "Goal141 Roundtrip"
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
            Text = "Goal141 Command Roundtrip"
        };
        _goal141RuntimeBackedCommandRoundtripStatusTextBox = Goal139ReadOnlyTextBox(multiline: true);
        _goal141RuntimeBackedCommandRoundtripCommandTextBox = Goal139ReadOnlyTextBox(multiline: false);
        _goal141RuntimeBackedCommandRoundtripReportPathTextBox = Goal139ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal141RuntimeBackedCommandRoundtripStatusTextBox, 0, 1);
        layout.Controls.Add(_goal141RuntimeBackedCommandRoundtripCommandTextBox, 0, 2);
        layout.Controls.Add(_goal141RuntimeBackedCommandRoundtripReportPathTextBox, 0, 3);
        _goal141RuntimeBackedCommandRoundtripTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal141RuntimeBackedCommandRoundtripTabPage);
    }

    private void BindGoal141RuntimeBackedPlayerCommandRoundtrip(
        VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal141RuntimeBackedCommandRoundtripStatusTextBox is null
            || _goal141RuntimeBackedCommandRoundtripCommandTextBox is null
            || _goal141RuntimeBackedCommandRoundtripReportPathTextBox is null)
        {
            return;
        }

        _goal141RuntimeBackedCommandRoundtripStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "goal140Accepted="
                + result.Report.RuntimeBackedPlayerCommandRoundtripGoal140Accepted
                    .ToString().ToLowerInvariant(),
            "candidateId=" + result.Report.RuntimeBackedPlayerCommandRoundtripCandidateId,
            "totalControlRequestCount="
                + result.Report.RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount,
            "roundtripRequestCount=" + result.Report.RuntimeBackedPlayerCommandRoundtripRequestCount,
            "runtimeRoutedRequestCount="
                + result.Report.RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount,
            "presentationOnlyRequestCount="
                + result.Report.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount,
            "runtimeExecutedRequestCount="
                + result.Report.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount,
            "presentationOnlyRuntimeExecutionCount="
                + result.Report.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount,
            "runtimeMutatingPresentationRequestCount="
                + result.Report.RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount,
            "responseCount=" + result.Report.RuntimeBackedPlayerCommandRoundtripResponseCount,
            "roundtripSnapshotCount=" + result.Report.RuntimeBackedPlayerCommandRoundtripSnapshotCount,
            "controlRequestBridgePresent="
                + result.Report.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent
                    .ToString().ToLowerInvariant(),
            "stateHashChainPresent="
                + result.Report.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent
                    .ToString().ToLowerInvariant(),
            "requestResponseCorrelationPassed="
                + result.Report.RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed
                    .ToString().ToLowerInvariant(),
            "sequentialCursorContinuityPassed="
                + result.Report.RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed
                    .ToString().ToLowerInvariant(),
            "stateHashContinuityPassed="
                + result.Report.RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed
                    .ToString().ToLowerInvariant(),
            "copySummaryStateUnchanged="
                + result.Report.RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged
                    .ToString().ToLowerInvariant(),
            "loadModelStateUnchanged="
                + result.Report.RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged
                    .ToString().ToLowerInvariant(),
            "noControlIntentMappedToUnrelatedGameplayCommand="
                + result.Report.RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping
                    .ToString().ToLowerInvariant(),
            "roundtripSemanticCorrectnessPassed="
                + result.Report.RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed
                    .ToString().ToLowerInvariant(),
            "runtimeAuthority="
                + result.Report.RuntimeBackedPlayerCommandRoundtripRuntimeAuthority
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.RuntimeBackedPlayerCommandRoundtripProjectionOnly
                    .ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "unityConsumesRoundtripResult="
                + result.Report.RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult
                    .ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.RuntimeBackedPlayerCommandRoundtripManualUnityOptional
                    .ToString().ToLowerInvariant(),
            "accepted="
                + result.Report.RuntimeBackedPlayerCommandRoundtripAccepted
                    .ToString().ToLowerInvariant()
        ]);
        _goal141RuntimeBackedCommandRoundtripCommandTextBox.Text =
            "normalCommand=" + result.Report.RuntimeBackedPlayerCommandRoundtripNormalCommand;
        _goal141RuntimeBackedCommandRoundtripReportPathTextBox.Text =
            "reportPath=" + result.Report.RuntimeBackedPlayerCommandRoundtripReportPath;
    }
}
