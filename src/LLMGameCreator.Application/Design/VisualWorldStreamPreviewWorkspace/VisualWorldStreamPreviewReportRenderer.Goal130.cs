namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal130ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## GamePackage Candidate Factory and Matrix Pipeline",
            string.Empty,
            $"- candidateFactoryStatus: {report.GamePackageCandidateFactoryStatus}",
            $"- candidateCount: {report.GamePackageCandidateFactoryCandidateCount}",
            $"- passedCandidates: {report.GamePackageCandidateFactoryPassedCandidates}",
            $"- failedCandidates: {report.GamePackageCandidateFactoryFailedCandidates}",
            $"- matrixPassed: {report.GamePackageCandidateFactoryMatrixPassed.ToString().ToLowerInvariant()}",
            $"- candidateIndexPath: {report.GamePackageCandidateFactoryCandidateIndexPath}",
            $"- normalCommand: {report.GamePackageCandidateFactoryNormalCommand}",
            $"- factoryResultPath: {report.GamePackageCandidateFactoryResultPath}",
            $"- matrixResultPath: {report.GamePackageCandidateFactoryMatrixResultPath}",
            $"- manualUnityOptional: {report.GamePackageCandidateFactoryManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- samplePackageUnmodified: {report.GamePackageCandidateFactorySamplePackageUnmodified.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.GamePackageCandidateFactoryProjectionOnly.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.GamePackageCandidateFactoryEvidencePath}",
            $"- exportPath: {report.GamePackageCandidateFactoryExportPath}",
            $"- gamePackageCandidateFactoryQualityGatePassed: {report.GamePackageCandidateFactoryQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal130FilesDiscoveredByRelativePaths: {report.Goal130FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal130QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal130 Quality",
            string.Empty,
            $"- gamePackageCandidateFactoryGroupPresent: {qualityGate.GamePackageCandidateFactoryGroupPresent.ToString().ToLowerInvariant()}",
            $"- candidateFactoryStatus: {qualityGate.GamePackageCandidateFactoryStatus}",
            $"- candidateCount: {qualityGate.GamePackageCandidateFactoryCandidateCount}",
            $"- passedCandidates: {qualityGate.GamePackageCandidateFactoryPassedCandidates}",
            $"- failedCandidates: {qualityGate.GamePackageCandidateFactoryFailedCandidates}",
            $"- matrixPassed: {qualityGate.GamePackageCandidateFactoryMatrixPassed.ToString().ToLowerInvariant()}",
            $"- candidateIndexPath: {qualityGate.GamePackageCandidateFactoryCandidateIndexPath}",
            $"- normalCommand: {qualityGate.GamePackageCandidateFactoryNormalCommand}",
            $"- factoryResultPath: {qualityGate.GamePackageCandidateFactoryResultPath}",
            $"- matrixResultPath: {qualityGate.GamePackageCandidateFactoryMatrixResultPath}",
            $"- manualUnityOptional: {qualityGate.GamePackageCandidateFactoryManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- samplePackageUnmodified: {qualityGate.GamePackageCandidateFactorySamplePackageUnmodified.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.GamePackageCandidateFactoryProjectionOnly.ToString().ToLowerInvariant()}",
            $"- gamePackageCandidateFactoryQualityGatePassed: {qualityGate.GamePackageCandidateFactoryQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal130FilesDiscoveredByRelativePaths: {qualityGate.Goal130FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGamePackageCandidateFactoryBindingReal: {qualityGate.WinFormsGamePackageCandidateFactoryBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
