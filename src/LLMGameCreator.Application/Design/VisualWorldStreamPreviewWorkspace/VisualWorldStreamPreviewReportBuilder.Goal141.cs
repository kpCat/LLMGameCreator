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
            RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount,
            RuntimeBackedPlayerCommandRoundtripRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripRequestCount,
            RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount,
            RuntimeBackedPlayerCommandRoundtripExecutedRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount,
            RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount,
            RuntimeBackedPlayerCommandRoundtripResponseCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripResponseCount,
            RuntimeBackedPlayerCommandRoundtripSnapshotCount =
                qualityGate.RuntimeBackedPlayerCommandRoundtripSnapshotCount,
            RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent =
                qualityGate.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent,
            RuntimeBackedPlayerCommandRoundtripStateHashChainPresent =
                qualityGate.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent,
            RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed =
                qualityGate.RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed,
            RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed =
                qualityGate.RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed,
            RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed =
                qualityGate.RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed,
            RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged =
                qualityGate.RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged,
            RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged =
                qualityGate.RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged,
            RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping =
                qualityGate.RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping,
            RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed =
                qualityGate.RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed,
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
