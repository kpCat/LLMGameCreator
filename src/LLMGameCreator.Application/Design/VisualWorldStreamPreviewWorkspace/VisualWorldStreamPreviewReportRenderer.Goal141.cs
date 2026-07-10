namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal141ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Runtime-backed Player Command Roundtrip",
            string.Empty,
            $"- goal140Accepted: {report.RuntimeBackedPlayerCommandRoundtripGoal140Accepted.ToString().ToLowerInvariant()}",
            $"- candidateId: {report.RuntimeBackedPlayerCommandRoundtripCandidateId}",
            $"- totalControlRequestCount: {report.RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount}",
            $"- roundtripRequestCount: {report.RuntimeBackedPlayerCommandRoundtripRequestCount}",
            $"- runtimeRoutedRequestCount: {report.RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount}",
            $"- presentationOnlyRequestCount: {report.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount}",
            $"- runtimeExecutedRequestCount: {report.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount}",
            $"- presentationOnlyRuntimeExecutionCount: {report.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount}",
            $"- runtimeMutatingPresentationRequestCount: {report.RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount}",
            $"- responseCount: {report.RuntimeBackedPlayerCommandRoundtripResponseCount}",
            $"- roundtripSnapshotCount: {report.RuntimeBackedPlayerCommandRoundtripSnapshotCount}",
            $"- controlRequestBridgePresent: {report.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent.ToString().ToLowerInvariant()}",
            $"- stateHashChainPresent: {report.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent.ToString().ToLowerInvariant()}",
            $"- requestResponseCorrelationPassed: {report.RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed.ToString().ToLowerInvariant()}",
            $"- sequentialCursorContinuityPassed: {report.RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed.ToString().ToLowerInvariant()}",
            $"- stateHashContinuityPassed: {report.RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed.ToString().ToLowerInvariant()}",
            $"- copySummaryStateUnchanged: {report.RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged.ToString().ToLowerInvariant()}",
            $"- loadModelStateUnchanged: {report.RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged.ToString().ToLowerInvariant()}",
            $"- noControlIntentMappedToUnrelatedGameplayCommand: {report.RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping.ToString().ToLowerInvariant()}",
            $"- roundtripSemanticCorrectnessPassed: {report.RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {report.RuntimeBackedPlayerCommandRoundtripRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.RuntimeBackedPlayerCommandRoundtripProjectionOnly.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- unityConsumesRoundtripResult: {report.RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.RuntimeBackedPlayerCommandRoundtripNormalCommand}",
            $"- reportPath: {report.RuntimeBackedPlayerCommandRoundtripReportPath}",
            $"- manualUnityOptional: {report.RuntimeBackedPlayerCommandRoundtripManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- accepted: {report.RuntimeBackedPlayerCommandRoundtripAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedPlayerCommandRoundtripQualityGatePassed: {report.RuntimeBackedPlayerCommandRoundtripQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal141FilesDiscoveredByRelativePaths: {report.RuntimeBackedPlayerCommandRoundtripFilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal141QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal141 Quality",
            string.Empty,
            $"- runtimeBackedPlayerCommandRoundtripGroupPresent: {qualityGate.RuntimeBackedPlayerCommandRoundtripGroupPresent.ToString().ToLowerInvariant()}",
            $"- goal140Accepted: {qualityGate.RuntimeBackedPlayerCommandRoundtripGoal140Accepted.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.RuntimeBackedPlayerCommandRoundtripCandidateId}",
            $"- totalControlRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount}",
            $"- roundtripRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripRequestCount}",
            $"- runtimeRoutedRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount}",
            $"- presentationOnlyRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount}",
            $"- runtimeExecutedRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount}",
            $"- presentationOnlyRuntimeExecutionCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount}",
            $"- runtimeMutatingPresentationRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount}",
            $"- responseCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripResponseCount}",
            $"- roundtripSnapshotCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripSnapshotCount}",
            $"- controlRequestBridgePresent: {qualityGate.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent.ToString().ToLowerInvariant()}",
            $"- stateHashChainPresent: {qualityGate.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent.ToString().ToLowerInvariant()}",
            $"- requestResponseCorrelationPassed: {qualityGate.RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed.ToString().ToLowerInvariant()}",
            $"- sequentialCursorContinuityPassed: {qualityGate.RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed.ToString().ToLowerInvariant()}",
            $"- stateHashContinuityPassed: {qualityGate.RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed.ToString().ToLowerInvariant()}",
            $"- copySummaryStateUnchanged: {qualityGate.RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged.ToString().ToLowerInvariant()}",
            $"- loadModelStateUnchanged: {qualityGate.RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged.ToString().ToLowerInvariant()}",
            $"- noControlIntentMappedToUnrelatedGameplayCommand: {qualityGate.RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping.ToString().ToLowerInvariant()}",
            $"- roundtripSemanticCorrectnessPassed: {qualityGate.RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {qualityGate.RuntimeBackedPlayerCommandRoundtripRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.RuntimeBackedPlayerCommandRoundtripProjectionOnly.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- unityConsumesRoundtripResult: {qualityGate.RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult.ToString().ToLowerInvariant()}",
            $"- manualUnityOptional: {qualityGate.RuntimeBackedPlayerCommandRoundtripManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.RuntimeBackedPlayerCommandRoundtripAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedPlayerCommandRoundtripWinFormsBindingReal: {qualityGate.RuntimeBackedPlayerCommandRoundtripWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- runtimeBackedPlayerCommandRoundtripQualityGatePassed: {qualityGate.RuntimeBackedPlayerCommandRoundtripQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal141Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal141ReportLines(lines, report);
        AddGoal141QualityLines(lines, qualityGate);
        return RenderWithGoal142Lines(lines, report, qualityGate);
    }
}
