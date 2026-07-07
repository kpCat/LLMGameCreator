using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal135PlayerLoopTabPage;
    private TextBox? _goal135PlayerLoopStatusTextBox;
    private TextBox? _goal135PlayerLoopCommandTextBox;
    private TextBox? _goal135PlayerLoopReportPathTextBox;

    private void ConfigureGoal135PlayerLoopPanel()
    {
        _goal135PlayerLoopTabPage = new TabPage
        {
            Name = "_goal135PlayerLoopTabPage",
            Text = "Player Loop"
        };
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

        var header = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Goal135 Player Loop"
        };
        _goal135PlayerLoopStatusTextBox = Goal135ReadOnlyTextBox(multiline: true);
        _goal135PlayerLoopCommandTextBox = Goal135ReadOnlyTextBox(multiline: false);
        _goal135PlayerLoopReportPathTextBox = Goal135ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal135PlayerLoopStatusTextBox, 0, 1);
        layout.Controls.Add(_goal135PlayerLoopCommandTextBox, 0, 2);
        layout.Controls.Add(_goal135PlayerLoopReportPathTextBox, 0, 3);
        _goal135PlayerLoopTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal135PlayerLoopTabPage);
    }

    private void BindGoal135PlayerLoop(VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal135PlayerLoopStatusTextBox is null
            || _goal135PlayerLoopCommandTextBox is null
            || _goal135PlayerLoopReportPathTextBox is null)
        {
            return;
        }

        _goal135PlayerLoopStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "candidateId=" + result.Report.CanonicalRuntimePlayerLoopCandidateId,
            "playerAdapterContractPresent="
                + result.Report.CanonicalRuntimePlayerLoopAdapterContractPresent
                    .ToString().ToLowerInvariant(),
            "playerLoopStepCount="
                + result.Report.CanonicalRuntimePlayerLoopStepCount,
            "requiredStepCategoriesPresent="
                + result.Report.CanonicalRuntimePlayerLoopRequiredCategoriesPresent
                    .ToString().ToLowerInvariant(),
            "unityPlayerLoopReadinessPassed="
                + result.Report.CanonicalRuntimePlayerLoopUnityReadinessPassed
                    .ToString().ToLowerInvariant(),
            "canonicalRuntimeSource="
                + result.Report.CanonicalRuntimePlayerLoopSource.ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.CanonicalRuntimePlayerLoopUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.CanonicalRuntimePlayerLoopProjectionOnly.ToString().ToLowerInvariant(),
            "noUnclassifiedErrorDiagnostics="
                + result.Report.CanonicalRuntimePlayerLoopNoUnclassifiedErrors
                    .ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.CanonicalRuntimePlayerLoopManualUnityOptional
                    .ToString().ToLowerInvariant()
        ]);
        _goal135PlayerLoopCommandTextBox.Text =
            "normalCommand=" + result.Report.CanonicalRuntimePlayerLoopNormalCommand;
        _goal135PlayerLoopReportPathTextBox.Text =
            "reportPath=" + result.Report.CanonicalRuntimePlayerLoopReportPath;
    }

    private static TextBox Goal135ReadOnlyTextBox(bool multiline) =>
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
