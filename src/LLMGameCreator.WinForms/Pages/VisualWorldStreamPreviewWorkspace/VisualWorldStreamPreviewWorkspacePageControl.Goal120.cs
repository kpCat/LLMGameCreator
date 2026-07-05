using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildAcceptedAlphaProjectionUsabilityDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "acceptedAlphaProjectionUsabilityStatus="
            + result.Report.AcceptedAlphaProjectionUsabilityStatus,
        "acceptedAlphaProjectionUsabilityUnityMenuPath="
            + result.Report.AcceptedAlphaProjectionUsabilityUnityMenuPath,
        "acceptedAlphaProjectionUsabilityCleanupScriptPath="
            + result.Report.AcceptedAlphaProjectionUsabilityCleanupScriptPath,
        "acceptedAlphaProjectionUsabilityCleanupScriptCmdPath="
            + result.Report.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath,
        "acceptedAlphaProjectionUsabilityLegendPresent="
            + result.Report.AcceptedAlphaProjectionUsabilityLegendPresent.ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityMarkerDescriptorPresent="
            + result.Report.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilitySelectionControlsPresent="
            + result.Report.AcceptedAlphaProjectionUsabilitySelectionControlsPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityFocusCameraControlPresent="
            + result.Report.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityMaterialWarningGuardPresent="
            + result.Report.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityUnitySmokeStatus="
            + result.Report.AcceptedAlphaProjectionUsabilityUnitySmokeStatus,
        "acceptedAlphaProjectionUsabilityDoNotStartAutomatically="
            + result.Report.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityQualityGatePassed="
            + result.Report.AcceptedAlphaProjectionUsabilityQualityGatePassed
                .ToString().ToLowerInvariant(),
        "goal120FilesDiscoveredByRelativePaths="
            + result.Report.Goal120FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildAcceptedAlphaProjectionUsabilityEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "acceptedAlphaProjectionUsabilityStatus: "
            + entry.AcceptedAlphaProjectionUsabilityStatus,
        "acceptedAlphaProjectionUsabilityUnityMenuPath: "
            + entry.AcceptedAlphaProjectionUsabilityUnityMenuPath,
        "acceptedAlphaProjectionUsabilityCleanupScriptPath: "
            + entry.AcceptedAlphaProjectionUsabilityCleanupScriptPath,
        "acceptedAlphaProjectionUsabilityCleanupScriptCmdPath: "
            + entry.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath,
        "acceptedAlphaProjectionUsabilityLegendPresent: "
            + entry.AcceptedAlphaProjectionUsabilityLegendPresent.ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityMarkerDescriptorPresent: "
            + entry.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilitySelectionControlsPresent: "
            + entry.AcceptedAlphaProjectionUsabilitySelectionControlsPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityFocusCameraControlPresent: "
            + entry.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityMaterialWarningGuardPresent: "
            + entry.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityUnitySmokeStatus: "
            + entry.AcceptedAlphaProjectionUsabilityUnitySmokeStatus,
        "acceptedAlphaProjectionUsabilityDoNotStartAutomatically: "
            + entry.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "acceptedAlphaProjectionUsabilityEvidencePath: "
            + entry.AcceptedAlphaProjectionUsabilityEvidencePath,
        "acceptedAlphaProjectionUsabilityExportPath: "
            + entry.AcceptedAlphaProjectionUsabilityExportPath
    ];
}
