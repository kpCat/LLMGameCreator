namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal140ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Runtime-backed Unity Player Loop Controls UX",
            string.Empty,
            $"- acceptedGoal139: {report.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139.ToString().ToLowerInvariant()}",
            $"- selectedCandidateId: {report.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate}",
            $"- frameCount: {report.RuntimeBackedUnityPlayerLoopControlsUxFrameCount}",
            $"- humanReadableFrameNumbering: {report.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering.ToString().ToLowerInvariant()}",
            $"- stepOnceSemanticsClear: {report.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear.ToString().ToLowerInvariant()}",
            $"- playAllToEndSemanticsClear: {report.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear.ToString().ToLowerInvariant()}",
            $"- knownUnityEditorNoiseClassified: {report.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified.ToString().ToLowerInvariant()}",
            $"- blockingUnityErrorCount: {report.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount}",
            $"- unclassifiedUnityErrorCount: {report.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount}",
            $"- unityControlsUxSmokePassed: {report.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {report.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.RuntimeBackedUnityPlayerLoopControlsUxNormalCommand}",
            $"- reportPath: {report.RuntimeBackedUnityPlayerLoopControlsUxReportPath}",
            $"- accepted: {report.RuntimeBackedUnityPlayerLoopControlsUxAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopControlsUxQualityGatePassed: {report.RuntimeBackedUnityPlayerLoopControlsUxQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal140FilesDiscoveredByRelativePaths: {report.RuntimeBackedUnityPlayerLoopControlsUxFilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal140QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal140 Quality",
            string.Empty,
            $"- runtimeBackedUnityPlayerLoopControlsUxGroupPresent: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedGoal139: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139.ToString().ToLowerInvariant()}",
            $"- selectedCandidateId: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate}",
            $"- frameCount: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxFrameCount}",
            $"- humanReadableFrameNumbering: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering.ToString().ToLowerInvariant()}",
            $"- stepOnceSemanticsClear: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear.ToString().ToLowerInvariant()}",
            $"- playAllToEndSemanticsClear: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear.ToString().ToLowerInvariant()}",
            $"- knownUnityEditorNoiseClassified: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified.ToString().ToLowerInvariant()}",
            $"- blockingUnityErrorCount: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount}",
            $"- unclassifiedUnityErrorCount: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount}",
            $"- unityControlsUxSmokePassed: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed.ToString().ToLowerInvariant()}",
            $"- runtimeAuthority: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxAccepted.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopControlsUxWinFormsBindingReal: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- runtimeBackedUnityPlayerLoopControlsUxQualityGatePassed: {qualityGate.RuntimeBackedUnityPlayerLoopControlsUxQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal140Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal140ReportLines(lines, report);
        AddGoal140QualityLines(lines, qualityGate);
        return RenderWithGoal141Lines(lines, report, qualityGate);
    }
}
