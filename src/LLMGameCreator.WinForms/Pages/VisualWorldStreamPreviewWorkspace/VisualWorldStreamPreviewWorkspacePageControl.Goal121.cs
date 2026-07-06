using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildAcceptedAlphaInteractionDrilldownDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "acceptedAlphaInteractionDrilldownFullVerificationStatus="
            + result.Report.AcceptedAlphaInteractionDrilldownFullVerificationStatus,
        "acceptedAlphaInteractionDrilldownUnityMenuPath="
            + result.Report.AcceptedAlphaInteractionDrilldownUnityMenuPath,
        "acceptedAlphaInteractionDrilldownOneClickButtonPresent="
            + result.Report.AcceptedAlphaInteractionDrilldownOneClickButtonPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownDrilldownFieldsPresent="
            + result.Report.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownInteractionPreviewPresent="
            + result.Report.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent="
            + result.Report.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker="
            + result.Report.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker,
        "acceptedAlphaInteractionDrilldownCleanupScriptAvailable="
            + result.Report.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownMaterialWarningGuardPresent="
            + result.Report.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton="
            + result.Report.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus="
            + result.Report.AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus,
        "acceptedAlphaInteractionDrilldownQualityGatePassed="
            + result.Report.AcceptedAlphaInteractionDrilldownQualityGatePassed
                .ToString().ToLowerInvariant(),
        "goal121FilesDiscoveredByRelativePaths="
            + result.Report.Goal121FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildAcceptedAlphaInteractionDrilldownEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "acceptedAlphaInteractionDrilldownFullVerificationStatus: "
            + entry.AcceptedAlphaInteractionDrilldownFullVerificationStatus,
        "acceptedAlphaInteractionDrilldownUnityMenuPath: "
            + entry.AcceptedAlphaInteractionDrilldownUnityMenuPath,
        "acceptedAlphaInteractionDrilldownOneClickButtonPresent: "
            + entry.AcceptedAlphaInteractionDrilldownOneClickButtonPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownDrilldownFieldsPresent: "
            + entry.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownInteractionPreviewPresent: "
            + entry.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent: "
            + entry.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker: "
            + entry.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker,
        "acceptedAlphaInteractionDrilldownCleanupScriptAvailable: "
            + entry.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownMaterialWarningGuardPresent: "
            + entry.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton: "
            + entry.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton
                .ToString().ToLowerInvariant(),
        "acceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus: "
            + entry.AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus,
        "acceptedAlphaInteractionDrilldownEvidencePath: "
            + entry.AcceptedAlphaInteractionDrilldownEvidencePath,
        "acceptedAlphaInteractionDrilldownExportPath: "
            + entry.AcceptedAlphaInteractionDrilldownExportPath
    ];
}
