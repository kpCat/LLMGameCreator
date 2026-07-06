using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildAcceptedAlphaProjectionActionLoopDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "acceptedAlphaProjectionActionLoopStatus="
            + result.Report.AcceptedAlphaProjectionActionLoopStatus,
        "acceptedAlphaProjectionActionLoopWindowPolishStatus="
            + result.Report.AcceptedAlphaProjectionActionLoopWindowPolishStatus,
        "acceptedAlphaProjectionActionLoopUnityMenuPath="
            + result.Report.AcceptedAlphaProjectionActionLoopUnityMenuPath,
        "acceptedAlphaProjectionActionLoopOneClickVerificationStillPresent="
            + result.Report.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopProjectionActionPreviewPresent="
            + result.Report.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopProjectionActionApplyPresent="
            + result.Report.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopProjectionStateResetPresent="
            + result.Report.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopWindowLayoutPolishPresent="
            + result.Report.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopUnitySmokeStatus="
            + result.Report.AcceptedAlphaProjectionActionLoopUnitySmokeStatus,
        "acceptedAlphaProjectionActionLoopCleanupScriptAvailable="
            + result.Report.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopQualityGatePassed="
            + result.Report.AcceptedAlphaProjectionActionLoopQualityGatePassed
                .ToString().ToLowerInvariant(),
        "goal122FilesDiscoveredByRelativePaths="
            + result.Report.Goal122FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildAcceptedAlphaProjectionActionLoopEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "acceptedAlphaProjectionActionLoopStatus: "
            + entry.AcceptedAlphaProjectionActionLoopStatus,
        "acceptedAlphaProjectionActionLoopWindowPolishStatus: "
            + entry.AcceptedAlphaProjectionActionLoopWindowPolishStatus,
        "acceptedAlphaProjectionActionLoopUnityMenuPath: "
            + entry.AcceptedAlphaProjectionActionLoopUnityMenuPath,
        "acceptedAlphaProjectionActionLoopOneClickVerificationStillPresent: "
            + entry.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopProjectionActionPreviewPresent: "
            + entry.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopProjectionActionApplyPresent: "
            + entry.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopProjectionStateResetPresent: "
            + entry.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopWindowLayoutPolishPresent: "
            + entry.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopUnitySmokeStatus: "
            + entry.AcceptedAlphaProjectionActionLoopUnitySmokeStatus,
        "acceptedAlphaProjectionActionLoopCleanupScriptAvailable: "
            + entry.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopDoNotStartAutomatically: "
            + entry.AcceptedAlphaProjectionActionLoopDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionActionLoopEvidencePath: "
            + entry.AcceptedAlphaProjectionActionLoopEvidencePath,
        "acceptedAlphaProjectionActionLoopExportPath: "
            + entry.AcceptedAlphaProjectionActionLoopExportPath
    ];
}
