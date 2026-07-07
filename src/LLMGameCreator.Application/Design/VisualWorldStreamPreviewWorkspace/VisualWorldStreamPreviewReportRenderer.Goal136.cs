namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal136ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Canonical Runtime Player Command Loop",
            string.Empty,
            $"- candidateId: {report.CanonicalRuntimePlayerCommandLoopCandidateId}",
            $"- playerCommandLoopPassed: {report.CanonicalRuntimePlayerCommandLoopPassed.ToString().ToLowerInvariant()}",
            $"- playerCommandCount: {report.CanonicalRuntimePlayerCommandCount}",
            $"- snapshotCount: {report.CanonicalRuntimePlayerSnapshotCount}",
            $"- runtimeEventCount: {report.CanonicalRuntimePlayerCommandLoopRuntimeEventCount}",
            $"- allRequiredCategoriesPresent: {report.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- unityPlayerConsumedCommandLoopSnapshots: {report.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.CanonicalRuntimePlayerCommandLoopProjectionOnly.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- noUnclassifiedErrorDiagnostics: {report.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.CanonicalRuntimePlayerCommandLoopNormalCommand}",
            $"- reportPath: {report.CanonicalRuntimePlayerCommandLoopReportPath}",
            $"- matrixResultPath: {report.CanonicalRuntimePlayerCommandLoopMatrixResultPath}",
            $"- manualUnityOptional: {report.CanonicalRuntimePlayerCommandLoopManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- accepted: {report.CanonicalRuntimePlayerCommandLoopAccepted.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePlayerCommandLoopQualityGatePassed: {report.CanonicalRuntimePlayerCommandLoopQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal136FilesDiscoveredByRelativePaths: {report.CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal136QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal136 Quality",
            string.Empty,
            $"- canonicalRuntimePlayerCommandLoopGroupPresent: {qualityGate.CanonicalRuntimePlayerCommandLoopGroupPresent.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.CanonicalRuntimePlayerCommandLoopCandidateId}",
            $"- playerCommandLoopPassed: {qualityGate.CanonicalRuntimePlayerCommandLoopPassed.ToString().ToLowerInvariant()}",
            $"- playerCommandCount: {qualityGate.CanonicalRuntimePlayerCommandCount}",
            $"- snapshotCount: {qualityGate.CanonicalRuntimePlayerSnapshotCount}",
            $"- runtimeEventCount: {qualityGate.CanonicalRuntimePlayerCommandLoopRuntimeEventCount}",
            $"- allRequiredCategoriesPresent: {qualityGate.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- unityPlayerConsumedCommandLoopSnapshots: {qualityGate.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.CanonicalRuntimePlayerCommandLoopProjectionOnly.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- noUnclassifiedErrorDiagnostics: {qualityGate.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.CanonicalRuntimePlayerCommandLoopAccepted.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePlayerCommandLoopWinFormsBindingReal: {qualityGate.CanonicalRuntimePlayerCommandLoopWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePlayerCommandLoopQualityGatePassed: {qualityGate.CanonicalRuntimePlayerCommandLoopQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal136Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal136ReportLines(lines, report);
        AddGoal136QualityLines(lines, qualityGate);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
