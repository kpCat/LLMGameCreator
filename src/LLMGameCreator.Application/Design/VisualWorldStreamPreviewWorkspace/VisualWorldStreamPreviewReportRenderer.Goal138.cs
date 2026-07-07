namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal138ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Runtime-backed Unity Player Loop Stepper",
            string.Empty,
            $"- acceptedGoal137: {report.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137.ToString().ToLowerInvariant()}",
            $"- candidateId: {report.RuntimeBackedUnityPlayerLoopStepperCandidateId}",
            $"- frameCount: {report.RuntimeBackedUnityPlayerLoopStepperFrameCount}",
            $"- requiredFrameCategoriesPresent: {report.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {report.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.RuntimeBackedUnityPlayerLoopStepperProjectionOnly.ToString().ToLowerInvariant()}",
            $"- stepperWindowPresent: {report.RuntimeBackedUnityPlayerLoopStepperWindowPresent.ToString().ToLowerInvariant()}",
            $"- stepperBatchSmokePassed: {report.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.RuntimeBackedUnityPlayerLoopStepperNormalCommand}",
            $"- reportPath: {report.RuntimeBackedUnityPlayerLoopStepperReportPath}",
            $"- manualUnityOptional: {report.RuntimeBackedUnityPlayerLoopStepperManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- accepted: {report.RuntimeBackedUnityPlayerLoopStepperAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopStepperQualityGatePassed: {report.RuntimeBackedUnityPlayerLoopStepperQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal138FilesDiscoveredByRelativePaths: {report.RuntimeBackedUnityPlayerLoopStepperFilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal138QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal138 Quality",
            string.Empty,
            $"- runtimeBackedUnityPlayerLoopStepperGroupPresent: {qualityGate.RuntimeBackedUnityPlayerLoopStepperGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedGoal137: {qualityGate.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.RuntimeBackedUnityPlayerLoopStepperCandidateId}",
            $"- frameCount: {qualityGate.RuntimeBackedUnityPlayerLoopStepperFrameCount}",
            $"- requiredFrameCategoriesPresent: {qualityGate.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {qualityGate.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.RuntimeBackedUnityPlayerLoopStepperProjectionOnly.ToString().ToLowerInvariant()}",
            $"- stepperWindowPresent: {qualityGate.RuntimeBackedUnityPlayerLoopStepperWindowPresent.ToString().ToLowerInvariant()}",
            $"- stepperBatchSmokePassed: {qualityGate.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.RuntimeBackedUnityPlayerLoopStepperAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopStepperWinFormsBindingReal: {qualityGate.RuntimeBackedUnityPlayerLoopStepperWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopStepperQualityGatePassed: {qualityGate.RuntimeBackedUnityPlayerLoopStepperQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal138Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal138ReportLines(lines, report);
        AddGoal138QualityLines(lines, qualityGate);
        return RenderWithGoal139Lines(lines, report, qualityGate);
    }
}
