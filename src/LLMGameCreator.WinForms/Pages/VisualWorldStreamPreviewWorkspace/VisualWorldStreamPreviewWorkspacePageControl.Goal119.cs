using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildAcceptedAlphaUnityPlayableProjectionDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "acceptedAlphaUnityPlayableProjectionStatus="
            + result.Report.AcceptedAlphaUnityPlayableProjectionStatus,
        "acceptedAlphaUnityPlayableProjectionUnityMenuPath="
            + result.Report.AcceptedAlphaUnityPlayableProjectionUnityMenuPath,
        "acceptedAlphaUnityPlayableProjectionBaselineId="
            + result.Report.AcceptedAlphaUnityPlayableProjectionBaselineId,
        "acceptedAlphaUnityPlayableProjectionAcceptedBaselineReady="
            + result.Report.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady
                .ToString().ToLowerInvariant(),
        "acceptedAlphaUnityPlayableProjectionGeneratedRootName="
            + result.Report.AcceptedAlphaUnityPlayableProjectionGeneratedRootName,
        "acceptedAlphaUnityPlayableProjectionScriptInventoryCount="
            + result.Report.AcceptedAlphaUnityPlayableProjectionScriptInventoryCount,
        "acceptedAlphaUnityPlayableProjectionSmokePlanStepCount="
            + result.Report.AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount,
        "acceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean="
            + result.Report.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean
                .ToString().ToLowerInvariant(),
        "acceptedAlphaUnityPlayableProjectionDoNotStartAutomatically="
            + result.Report.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "acceptedAlphaUnityPlayableProjectionQualityGatePassed="
            + result.Report.AcceptedAlphaUnityPlayableProjectionQualityGatePassed
                .ToString().ToLowerInvariant(),
        "goal119FilesDiscoveredByRelativePaths="
            + result.Report.Goal119FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildAcceptedAlphaUnityPlayableProjectionEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "acceptedAlphaUnityPlayableProjectionStatus: "
            + entry.AcceptedAlphaUnityPlayableProjectionStatus,
        "acceptedAlphaUnityPlayableProjectionUnityMenuPath: "
            + entry.AcceptedAlphaUnityPlayableProjectionUnityMenuPath,
        "acceptedAlphaUnityPlayableProjectionBaselineId: "
            + entry.AcceptedAlphaUnityPlayableProjectionBaselineId,
        "acceptedAlphaUnityPlayableProjectionAcceptedBaselineReady: "
            + entry.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady
                .ToString().ToLowerInvariant(),
        "acceptedAlphaUnityPlayableProjectionGeneratedRootName: "
            + entry.AcceptedAlphaUnityPlayableProjectionGeneratedRootName,
        "acceptedAlphaUnityPlayableProjectionScriptInventoryCount: "
            + entry.AcceptedAlphaUnityPlayableProjectionScriptInventoryCount,
        "acceptedAlphaUnityPlayableProjectionSmokePlanStepCount: "
            + entry.AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount,
        "acceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean: "
            + entry.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean
                .ToString().ToLowerInvariant(),
        "acceptedAlphaUnityPlayableProjectionDoNotStartAutomatically: "
            + entry.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "acceptedAlphaUnityPlayableProjectionEvidencePath: "
            + entry.AcceptedAlphaUnityPlayableProjectionEvidencePath,
        "acceptedAlphaUnityPlayableProjectionExportPath: "
            + entry.AcceptedAlphaUnityPlayableProjectionExportPath
    ];
}
