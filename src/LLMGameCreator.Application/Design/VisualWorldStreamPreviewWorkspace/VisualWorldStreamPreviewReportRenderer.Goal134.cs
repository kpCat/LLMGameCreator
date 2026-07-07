namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal134ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Canonical Runtime Selected Candidate Playthrough",
            string.Empty,
            $"- candidateId: {report.CanonicalRuntimeCandidateId}",
            $"- packageValidationPassed: {report.CanonicalRuntimePackageValidationPassed.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePassed: {report.CanonicalRuntimePassed.ToString().ToLowerInvariant()}",
            $"- runtimeCommandCount: {report.CanonicalRuntimeCommandCount}",
            $"- runtimeEventCount: {report.CanonicalRuntimeEventCount}",
            $"- saveLoadReplayPassed: {report.CanonicalRuntimeSaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"- unityPlayerConsumedCanonicalTranscript: {report.CanonicalRuntimeUnityPlayerConsumedTranscript.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.CanonicalRuntimeProjectionOnly.ToString().ToLowerInvariant()}",
            $"- selectedCandidateExecutedByRuntime: {report.CanonicalRuntimeSelectedCandidateExecutedByRuntime.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.CanonicalRuntimeNormalCommand}",
            $"- reportPath: {report.CanonicalRuntimeReportPath}",
            $"- matrixResultPath: {report.CanonicalRuntimeMatrixResultPath}",
            $"- manualUnityOptional: {report.CanonicalRuntimeManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeQualityGatePassed: {report.CanonicalRuntimeQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal134FilesDiscoveredByRelativePaths: {report.CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal134QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal134 Quality",
            string.Empty,
            $"- canonicalRuntimeSelectedCandidateGroupPresent: {qualityGate.CanonicalRuntimeSelectedCandidateGroupPresent.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.CanonicalRuntimeCandidateId}",
            $"- packageValidationPassed: {qualityGate.CanonicalRuntimePackageValidationPassed.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePassed: {qualityGate.CanonicalRuntimePassed.ToString().ToLowerInvariant()}",
            $"- runtimeCommandCount: {qualityGate.CanonicalRuntimeCommandCount}",
            $"- runtimeEventCount: {qualityGate.CanonicalRuntimeEventCount}",
            $"- saveLoadReplayPassed: {qualityGate.CanonicalRuntimeSaveLoadReplayPassed.ToString().ToLowerInvariant()}",
            $"- unityPlayerConsumedCanonicalTranscript: {qualityGate.CanonicalRuntimeUnityPlayerConsumedTranscript.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.CanonicalRuntimeProjectionOnly.ToString().ToLowerInvariant()}",
            $"- selectedCandidateExecutedByRuntime: {qualityGate.CanonicalRuntimeSelectedCandidateExecutedByRuntime.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeWinFormsBindingReal: {qualityGate.CanonicalRuntimeWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeQualityGatePassed: {qualityGate.CanonicalRuntimeQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal134Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal131ReportLines(lines, report);
        AddGoal131QualityLines(lines, qualityGate);
        AddGoal132ReportLines(lines, report);
        AddGoal132QualityLines(lines, qualityGate);
        AddGoal134ReportLines(lines, report);
        AddGoal134QualityLines(lines, qualityGate);
        return RenderWithGoal135Lines(lines, report, qualityGate);
    }
}
