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
            $"- roundtripRequestCount: {report.RuntimeBackedPlayerCommandRoundtripRequestCount}",
            $"- runtimeExecutedRequestCount: {report.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount}",
            $"- roundtripSnapshotCount: {report.RuntimeBackedPlayerCommandRoundtripSnapshotCount}",
            $"- controlRequestBridgePresent: {report.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent.ToString().ToLowerInvariant()}",
            $"- stateHashChainPresent: {report.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent.ToString().ToLowerInvariant()}",
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
            $"- roundtripRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripRequestCount}",
            $"- runtimeExecutedRequestCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount}",
            $"- roundtripSnapshotCount: {qualityGate.RuntimeBackedPlayerCommandRoundtripSnapshotCount}",
            $"- controlRequestBridgePresent: {qualityGate.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent.ToString().ToLowerInvariant()}",
            $"- stateHashChainPresent: {qualityGate.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent.ToString().ToLowerInvariant()}",
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
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
