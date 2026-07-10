namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal141ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            RuntimeBackedPlayerCommandRoundtripGoal140Accepted =
                qualityGate.RuntimeBackedPlayerCommandRoundtripGoal140Accepted,
            RuntimeBackedPlayerCommandRoundtripCandidateId =
                qualityGate.RuntimeBackedPlayerCommandRoundtripCandidateId,
            RuntimeBackedPlayerCommandRoundtripRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripRequestCount,
            RuntimeBackedPlayerCommandRoundtripExecutedRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripSnapshotCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripSnapshotCount,
            RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent =
                qualityGate.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent,
            RuntimeBackedPlayerCommandRoundtripStateHashChainPresent =
                qualityGate.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent,
            RuntimeBackedPlayerCommandRoundtripRuntimeAuthority =
                qualityGate.RuntimeBackedPlayerCommandRoundtripRuntimeAuthority,
            RuntimeBackedPlayerCommandRoundtripProjectionOnly =
                qualityGate.RuntimeBackedPlayerCommandRoundtripProjectionOnly,
            RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth =
                qualityGate.RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth,
            RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult =
                qualityGate.RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult,
            RuntimeBackedPlayerCommandRoundtripNormalCommand =
                qualityGate.RuntimeBackedPlayerCommandRoundtripNormalCommand,
            RuntimeBackedPlayerCommandRoundtripReportPath =
                qualityGate.RuntimeBackedPlayerCommandRoundtripReportPath,
            RuntimeBackedPlayerCommandRoundtripManualUnityOptional =
                qualityGate.RuntimeBackedPlayerCommandRoundtripManualUnityOptional,
            RuntimeBackedPlayerCommandRoundtripAccepted =
                qualityGate.RuntimeBackedPlayerCommandRoundtripAccepted,
            RuntimeBackedPlayerCommandRoundtripQualityGatePassed =
                qualityGate.RuntimeBackedPlayerCommandRoundtripQualityGatePassed,
            RuntimeBackedPlayerCommandRoundtripFilesDiscoveredByRelativePaths =
                qualityGate.RuntimeBackedPlayerCommandRoundtripFilesDiscoveredByRelativePaths
        };
}
