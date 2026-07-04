using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaExportPackageDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaExportPackageFileCount="
            + result.Report.OfflineGeoworldAlphaExportPackageFileCount,
        "offlineGeoworldAlphaExportIndexedFileCount="
            + result.Report.OfflineGeoworldAlphaExportIndexedFileCount,
        "offlineGeoworldAlphaExportChecksumStatus="
            + result.Report.OfflineGeoworldAlphaExportChecksumStatus,
        "offlineGeoworldAlphaExportCleanImportProofPassed="
            + result.Report.OfflineGeoworldAlphaExportCleanImportProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportNegativeProofPassed="
            + result.Report.OfflineGeoworldAlphaExportNegativeProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportUnityVerifierReady="
            + result.Report.OfflineGeoworldAlphaExportUnityVerifierReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportEditorWindowReady="
            + result.Report.OfflineGeoworldAlphaExportEditorWindowReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportWorkspaceBindingPassed="
            + result.Report.OfflineGeoworldAlphaExportWorkspaceBindingPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportSourceLineagePassed="
            + result.Report.OfflineGeoworldAlphaExportSourceLineagePassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportRunbookSummary="
            + result.Report.OfflineGeoworldAlphaExportRunbookSummary,
        "offlineGeoworldAlphaExportAcceptanceGateStatus="
            + result.Report.OfflineGeoworldAlphaExportAcceptanceGateStatus,
        "offlineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged="
            + result.Report.OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaExportQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaExportPackageEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaExportPackageFileCount: "
            + entry.OfflineGeoworldAlphaExportPackageFileCount,
        "offlineGeoworldAlphaExportIndexedFileCount: "
            + entry.OfflineGeoworldAlphaExportIndexedFileCount,
        "offlineGeoworldAlphaExportChecksumStatus: "
            + entry.OfflineGeoworldAlphaExportChecksumStatus,
        "offlineGeoworldAlphaExportCleanImportProofPassed: "
            + entry.OfflineGeoworldAlphaExportCleanImportProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportNegativeProofPassed: "
            + entry.OfflineGeoworldAlphaExportNegativeProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportUnityVerifierReady: "
            + entry.OfflineGeoworldAlphaExportUnityVerifierReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportEditorWindowReady: "
            + entry.OfflineGeoworldAlphaExportEditorWindowReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportWorkspaceBindingPassed: "
            + entry.OfflineGeoworldAlphaExportWorkspaceBindingPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportSourceLineagePassed: "
            + entry.OfflineGeoworldAlphaExportSourceLineagePassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportRunbookSummary: "
            + entry.OfflineGeoworldAlphaExportRunbookSummary,
        "offlineGeoworldAlphaExportAcceptanceGateStatus: "
            + entry.OfflineGeoworldAlphaExportAcceptanceGateStatus,
        "offlineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged: "
            + entry.OfflineGeoworldAlphaExportAlphaRuntimeBootstrapUnchanged
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaExportQualityGatePassed: "
            + entry.OfflineGeoworldAlphaExportQualityGatePassed
                .ToString().ToLowerInvariant()
    ];
}
