namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal129ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## GamePackage Candidate Matrix Projection Runner",
            string.Empty,
            $"- gamePackageCandidateMatrixStatus: {report.GamePackageCandidateMatrixStatus}",
            $"- candidateCount: {report.GamePackageCandidateMatrixCandidateCount}",
            $"- passedCandidateCount: {report.GamePackageCandidateMatrixPassedCandidateCount}",
            $"- failedCandidateCount: {report.GamePackageCandidateMatrixFailedCandidateCount}",
            $"- candidateIndexPath: {report.GamePackageCandidateMatrixCandidateIndexPath}",
            $"- matrixResultPath: {report.GamePackageCandidateMatrixResultPath}",
            $"- normalCommand: {report.GamePackageCandidateMatrixNormalCommand}",
            $"- exampleCommand: {report.GamePackageCandidateMatrixExampleCommand}",
            $"- baselineCandidatePackagePath: {report.GamePackageCandidateMatrixBaselineCandidatePackagePath}",
            $"- variantCandidatePackagePath: {report.GamePackageCandidateMatrixVariantCandidatePackagePath}",
            $"- manualUnityOptional: {report.GamePackageCandidateMatrixManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- cleanupApplied: {report.GamePackageCandidateMatrixCleanupApplied.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.GamePackageCandidateMatrixProjectionOnly.ToString().ToLowerInvariant()}",
            $"- scriptScanPassed: {report.GamePackageCandidateMatrixScriptScanPassed.ToString().ToLowerInvariant()}",
            $"- matrixResultPassed: {report.GamePackageCandidateMatrixResultPassed.ToString().ToLowerInvariant()}",
            $"- logScanPassed: {report.GamePackageCandidateMatrixLogScanPassed.ToString().ToLowerInvariant()}",
            $"- gamePackageCandidateMatrixQualityGatePassed: {report.GamePackageCandidateMatrixQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal129FilesDiscoveredByRelativePaths: {report.Goal129FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal129QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal129 Quality",
            string.Empty,
            $"- gamePackageCandidateMatrixGroupPresent: {qualityGate.GamePackageCandidateMatrixGroupPresent.ToString().ToLowerInvariant()}",
            $"- gamePackageCandidateMatrixStatus: {qualityGate.GamePackageCandidateMatrixStatus}",
            $"- candidateCount: {qualityGate.GamePackageCandidateMatrixCandidateCount}",
            $"- passedCandidateCount: {qualityGate.GamePackageCandidateMatrixPassedCandidateCount}",
            $"- failedCandidateCount: {qualityGate.GamePackageCandidateMatrixFailedCandidateCount}",
            $"- candidateIndexPath: {qualityGate.GamePackageCandidateMatrixCandidateIndexPath}",
            $"- matrixResultPath: {qualityGate.GamePackageCandidateMatrixResultPath}",
            $"- normalCommand: {qualityGate.GamePackageCandidateMatrixNormalCommand}",
            $"- cleanupApplied: {qualityGate.GamePackageCandidateMatrixCleanupApplied.ToString().ToLowerInvariant()}",
            $"- manualUnityOptional: {qualityGate.GamePackageCandidateMatrixManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.GamePackageCandidateMatrixProjectionOnly.ToString().ToLowerInvariant()}",
            $"- scriptScanPassed: {qualityGate.GamePackageCandidateMatrixScriptScanPassed.ToString().ToLowerInvariant()}",
            $"- matrixResultPassed: {qualityGate.GamePackageCandidateMatrixResultPassed.ToString().ToLowerInvariant()}",
            $"- logScanPassed: {qualityGate.GamePackageCandidateMatrixLogScanPassed.ToString().ToLowerInvariant()}",
            $"- gamePackageCandidateMatrixQualityGatePassed: {qualityGate.GamePackageCandidateMatrixQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal129FilesDiscoveredByRelativePaths: {qualityGate.Goal129FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGamePackageCandidateMatrixBindingReal: {qualityGate.WinFormsGamePackageCandidateMatrixBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
