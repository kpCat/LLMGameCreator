namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal135ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Canonical Runtime Player Loop Readiness",
            string.Empty,
            $"- candidateId: {report.CanonicalRuntimePlayerLoopCandidateId}",
            $"- playerAdapterContractPresent: {report.CanonicalRuntimePlayerLoopAdapterContractPresent.ToString().ToLowerInvariant()}",
            $"- playerLoopStepCount: {report.CanonicalRuntimePlayerLoopStepCount}",
            $"- requiredStepCategoriesPresent: {report.CanonicalRuntimePlayerLoopRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- unityPlayerLoopReadinessPassed: {report.CanonicalRuntimePlayerLoopUnityReadinessPassed.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeSource: {report.CanonicalRuntimePlayerLoopSource.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.CanonicalRuntimePlayerLoopUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.CanonicalRuntimePlayerLoopProjectionOnly.ToString().ToLowerInvariant()}",
            $"- noUnclassifiedErrorDiagnostics: {report.CanonicalRuntimePlayerLoopNoUnclassifiedErrors.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.CanonicalRuntimePlayerLoopNormalCommand}",
            $"- reportPath: {report.CanonicalRuntimePlayerLoopReportPath}",
            $"- manualUnityOptional: {report.CanonicalRuntimePlayerLoopManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePlayerLoopQualityGatePassed: {report.CanonicalRuntimePlayerLoopQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal135FilesDiscoveredByRelativePaths: {report.CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal135QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal135 Quality",
            string.Empty,
            $"- canonicalRuntimePlayerLoopGroupPresent: {qualityGate.CanonicalRuntimePlayerLoopGroupPresent.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.CanonicalRuntimePlayerLoopCandidateId}",
            $"- playerAdapterContractPresent: {qualityGate.CanonicalRuntimePlayerLoopAdapterContractPresent.ToString().ToLowerInvariant()}",
            $"- playerLoopStepCount: {qualityGate.CanonicalRuntimePlayerLoopStepCount}",
            $"- requiredStepCategoriesPresent: {qualityGate.CanonicalRuntimePlayerLoopRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- unityPlayerLoopReadinessPassed: {qualityGate.CanonicalRuntimePlayerLoopUnityReadinessPassed.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeSource: {qualityGate.CanonicalRuntimePlayerLoopSource.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.CanonicalRuntimePlayerLoopUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.CanonicalRuntimePlayerLoopProjectionOnly.ToString().ToLowerInvariant()}",
            $"- noUnclassifiedErrorDiagnostics: {qualityGate.CanonicalRuntimePlayerLoopNoUnclassifiedErrors.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePlayerLoopWinFormsBindingReal: {qualityGate.CanonicalRuntimePlayerLoopWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimePlayerLoopQualityGatePassed: {qualityGate.CanonicalRuntimePlayerLoopQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal135Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal135ReportLines(lines, report);
        AddGoal135QualityLines(lines, qualityGate);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
