using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualAcceptanceDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaManualAcceptanceChecklistStepCount="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount,
        "offlineGeoworldAlphaManualAcceptancePayloadFileCount="
            + result.Report.OfflineGeoworldAlphaManualAcceptancePayloadFileCount,
        "offlineGeoworldAlphaManualAcceptanceExportFileCount="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceExportFileCount,
        "offlineGeoworldAlphaManualAcceptanceAutomatedGatePassed="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceManualPending="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceManualPending
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceUnityRunnerReady="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceSimulatedProofPassed="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceNegativeProofPassed="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceQualityGatePassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceResultTemplatePath="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceResultTemplatePath,
        "offlineGeoworldAlphaManualAcceptanceReleaseRiskLinks="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks,
        "offlineGeoworldAlphaManualAcceptanceMilestoneGateLinks="
            + result.Report.OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualAcceptanceEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaManualAcceptanceChecklistStepCount: "
            + entry.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount,
        "offlineGeoworldAlphaManualAcceptancePayloadFileCount: "
            + entry.OfflineGeoworldAlphaManualAcceptancePayloadFileCount,
        "offlineGeoworldAlphaManualAcceptanceExportFileCount: "
            + entry.OfflineGeoworldAlphaManualAcceptanceExportFileCount,
        "offlineGeoworldAlphaManualAcceptanceAutomatedGatePassed: "
            + entry.OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceManualPending: "
            + entry.OfflineGeoworldAlphaManualAcceptanceManualPending
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceUnityRunnerReady: "
            + entry.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceSimulatedProofPassed: "
            + entry.OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceNegativeProofPassed: "
            + entry.OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed: "
            + entry.OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged: "
            + entry.OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceQualityGatePassed: "
            + entry.OfflineGeoworldAlphaManualAcceptanceQualityGatePassed
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualAcceptanceResultTemplatePath: "
            + entry.OfflineGeoworldAlphaManualAcceptanceResultTemplatePath,
        "offlineGeoworldAlphaManualAcceptanceReleaseRiskLinks: "
            + entry.OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks,
        "offlineGeoworldAlphaManualAcceptanceMilestoneGateLinks: "
            + entry.OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks
    ];
}
