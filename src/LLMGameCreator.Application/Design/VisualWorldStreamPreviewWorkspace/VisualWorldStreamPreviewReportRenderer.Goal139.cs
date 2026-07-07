namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal139ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Runtime-backed Unity Player Loop Interactive Controls",
            string.Empty,
            $"- acceptedGoal138: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138.ToString().ToLowerInvariant()}",
            $"- candidateId: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId}",
            $"- frameCount: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount}",
            $"- requiredControlsPresent: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent.ToString().ToLowerInvariant()}",
            $"- controlScriptPassed: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed.ToString().ToLowerInvariant()}",
            $"- interactiveControlsWindowPresent: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent.ToString().ToLowerInvariant()}",
            $"- unityInteractiveControlsSmokePassed: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand}",
            $"- reportPath: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath}",
            $"- manualUnityOptional: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- accepted: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal139FilesDiscoveredByRelativePaths: {report.RuntimeBackedUnityPlayerLoopInteractiveControlsFilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal139QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal139 Quality",
            string.Empty,
            $"- runtimeBackedUnityPlayerLoopInteractiveControlsGroupPresent: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedGoal138: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId}",
            $"- frameCount: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount}",
            $"- requiredControlsPresent: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent.ToString().ToLowerInvariant()}",
            $"- controlScriptPassed: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed.ToString().ToLowerInvariant()}",
            $"- interactiveControlsWindowPresent: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent.ToString().ToLowerInvariant()}",
            $"- unityInteractiveControlsSmokePassed: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopInteractiveControlsWinFormsBindingReal: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed: {qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal139Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal139ReportLines(lines, report);
        AddGoal139QualityLines(lines, qualityGate);
        return RenderWithGoal140Lines(lines, report, qualityGate);
    }
}
