using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaSliceDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaSliceComponentCount=" + result.Report.OfflineGeoworldAlphaSliceComponentCount,
        "offlineGeoworldAlphaSliceReadyComponentCount=" + result.Report.OfflineGeoworldAlphaSliceReadyComponentCount,
        "offlineGeoworldAlphaSliceObjectiveCount=" + result.Report.OfflineGeoworldAlphaSliceObjectiveCount,
        "offlineGeoworldAlphaSliceCompletedObjectiveCount=" + result.Report.OfflineGeoworldAlphaSliceCompletedObjectiveCount,
        "offlineGeoworldAlphaSliceFinalStatus=" + result.Report.OfflineGeoworldAlphaSliceFinalStatus,
        "offlineGeoworldAlphaSliceUnityToolReady=" + result.Report.OfflineGeoworldAlphaSliceUnityToolReady.ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaSliceAcceptanceRunbookReady=" + result.Report.OfflineGeoworldAlphaSliceAcceptanceRunbookReady.ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaSliceFinalProofPassed=" + result.Report.OfflineGeoworldAlphaSliceFinalProofPassed.ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged=" + result.Report.OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaSliceEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaSliceComponentCount: " + entry.OfflineGeoworldAlphaSliceComponentCount,
        "offlineGeoworldAlphaSliceReadyComponentCount: " + entry.OfflineGeoworldAlphaSliceReadyComponentCount,
        "offlineGeoworldAlphaSliceObjectiveCount: " + entry.OfflineGeoworldAlphaSliceObjectiveCount,
        "offlineGeoworldAlphaSliceCompletedObjectiveCount: " + entry.OfflineGeoworldAlphaSliceCompletedObjectiveCount,
        "offlineGeoworldAlphaSliceFinalStatus: " + entry.OfflineGeoworldAlphaSliceFinalStatus,
        "offlineGeoworldAlphaSliceUnityToolReady: " + entry.OfflineGeoworldAlphaSliceUnityToolReady.ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaSliceAcceptanceRunbookReady: " + entry.OfflineGeoworldAlphaSliceAcceptanceRunbookReady.ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaSliceFinalProofPassed: " + entry.OfflineGeoworldAlphaSliceFinalProofPassed.ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged: " + entry.OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged.ToString().ToLowerInvariant()
    ];
}
