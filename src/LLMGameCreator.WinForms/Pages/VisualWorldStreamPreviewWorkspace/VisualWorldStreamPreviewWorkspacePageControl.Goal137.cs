using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private TabPage? _goal137UnityPlayerLoopPlaybackTabPage;
    private TextBox? _goal137UnityPlayerLoopPlaybackStatusTextBox;
    private TextBox? _goal137UnityPlayerLoopPlaybackCommandTextBox;
    private TextBox? _goal137UnityPlayerLoopPlaybackReportPathTextBox;
    private TextBox? _goal137UnityPlayerLoopPlaybackMatrixPathTextBox;

    private void ConfigureGoal137UnityPlayerLoopPlaybackPanel()
    {
        _goal137UnityPlayerLoopPlaybackTabPage = new TabPage
        {
            Name = "_goal137UnityPlayerLoopPlaybackTabPage",
            Text = "Playback"
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
            Text = "Goal137 Playback"
        };
        _goal137UnityPlayerLoopPlaybackStatusTextBox = Goal137ReadOnlyTextBox(multiline: true);
        _goal137UnityPlayerLoopPlaybackCommandTextBox = Goal137ReadOnlyTextBox(multiline: false);
        _goal137UnityPlayerLoopPlaybackReportPathTextBox = Goal137ReadOnlyTextBox(multiline: false);
        _goal137UnityPlayerLoopPlaybackMatrixPathTextBox = Goal137ReadOnlyTextBox(multiline: false);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_goal137UnityPlayerLoopPlaybackStatusTextBox, 0, 1);
        layout.Controls.Add(_goal137UnityPlayerLoopPlaybackCommandTextBox, 0, 2);
        layout.Controls.Add(_goal137UnityPlayerLoopPlaybackReportPathTextBox, 0, 3);
        layout.Controls.Add(_goal137UnityPlayerLoopPlaybackMatrixPathTextBox, 0, 4);
        _goal137UnityPlayerLoopPlaybackTabPage.Controls.Add(layout);
        _detailTabs.TabPages.Add(_goal137UnityPlayerLoopPlaybackTabPage);
    }

    private void BindGoal137UnityPlayerLoopPlayback(VisualWorldStreamPreviewWorkspaceResult result)
    {
        if (_goal137UnityPlayerLoopPlaybackStatusTextBox is null
            || _goal137UnityPlayerLoopPlaybackCommandTextBox is null
            || _goal137UnityPlayerLoopPlaybackReportPathTextBox is null
            || _goal137UnityPlayerLoopPlaybackMatrixPathTextBox is null)
        {
            return;
        }

        _goal137UnityPlayerLoopPlaybackStatusTextBox.Text = string.Join(Environment.NewLine,
        [
            "candidateId=" + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId,
            "playbackFrameCount="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount,
            "requiredFrameCategoriesPresent="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent
                    .ToString().ToLowerInvariant(),
            "unityPlayerLoopPlaybackPassed="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackPassed
                    .ToString().ToLowerInvariant(),
            "runtimeSnapshotSource="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource
                    .ToString().ToLowerInvariant(),
            "unityGameplayTruth="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth
                    .ToString().ToLowerInvariant(),
            "projectionOnly="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly
                    .ToString().ToLowerInvariant(),
            "selectedCandidateExecutedByRuntime="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime
                    .ToString().ToLowerInvariant(),
            "manualUnityOptional="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional
                    .ToString().ToLowerInvariant(),
            "accepted="
                + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted
                    .ToString().ToLowerInvariant()
        ]);
        _goal137UnityPlayerLoopPlaybackCommandTextBox.Text =
            "normalCommand=" + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand;
        _goal137UnityPlayerLoopPlaybackReportPathTextBox.Text =
            "reportPath=" + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackReportPath;
        _goal137UnityPlayerLoopPlaybackMatrixPathTextBox.Text =
            "matrixResultPath=" + result.Report.CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath;
    }

    private static TextBox Goal137ReadOnlyTextBox(bool multiline) =>
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
