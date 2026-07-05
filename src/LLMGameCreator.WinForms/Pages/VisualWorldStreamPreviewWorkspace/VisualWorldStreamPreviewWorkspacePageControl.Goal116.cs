using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualGateAcceptanceDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaManualGateAcceptanceManualGate="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceManualGate,
        "offlineGeoworldAlphaManualGateAcceptanceManualGateStatus="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus,
        "offlineGeoworldAlphaManualGateAcceptanceHumanAccepted="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement,
        "offlineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus,
        "offlineGeoworldAlphaManualGateAcceptanceManualResultSha256="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256,
        "offlineGeoworldAlphaManualGateAcceptanceAcceptedByCodex="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision,
        "offlineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceRequiredStepCount="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount,
        "offlineGeoworldAlphaManualGateAcceptancePassedStepCount="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptancePassedStepCount,
        "offlineGeoworldAlphaManualGateAcceptanceQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaManualGateAcceptanceQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualGateAcceptanceEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaManualGateAcceptanceManualGate: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceManualGate,
        "offlineGeoworldAlphaManualGateAcceptanceManualGateStatus: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus,
        "offlineGeoworldAlphaManualGateAcceptanceHumanAccepted: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement,
        "offlineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus,
        "offlineGeoworldAlphaManualGateAcceptanceManualResultSha256: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256,
        "offlineGeoworldAlphaManualGateAcceptanceAcceptedByCodex: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision,
        "offlineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualGateAcceptanceRequiredStepCount: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount,
        "offlineGeoworldAlphaManualGateAcceptancePassedStepCount: "
            + entry.OfflineGeoworldAlphaManualGateAcceptancePassedStepCount,
        "offlineGeoworldAlphaManualGateAcceptanceProceduralPath: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceProceduralPath,
        "offlineGeoworldAlphaManualGateAcceptanceExportPath: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceExportPath,
        "offlineGeoworldAlphaManualGateAcceptanceErrors: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceErrors,
        "offlineGeoworldAlphaManualGateAcceptanceWarnings: "
            + entry.OfflineGeoworldAlphaManualGateAcceptanceWarnings
    ];
}
