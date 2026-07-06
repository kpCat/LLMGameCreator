using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGamePackageCandidateFactoryDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "candidateFactoryStatus="
            + result.Report.GamePackageCandidateFactoryStatus,
        "candidateCount="
            + result.Report.GamePackageCandidateFactoryCandidateCount,
        "passedCandidates="
            + result.Report.GamePackageCandidateFactoryPassedCandidates,
        "failedCandidates="
            + result.Report.GamePackageCandidateFactoryFailedCandidates,
        "matrixPassed="
            + result.Report.GamePackageCandidateFactoryMatrixPassed.ToString().ToLowerInvariant(),
        "candidateIndexPath="
            + result.Report.GamePackageCandidateFactoryCandidateIndexPath,
        "normalCommand="
            + result.Report.GamePackageCandidateFactoryNormalCommand,
        "factoryResultPath="
            + result.Report.GamePackageCandidateFactoryResultPath,
        "matrixResultPath="
            + result.Report.GamePackageCandidateFactoryMatrixResultPath,
        "manualUnityOptional="
            + result.Report.GamePackageCandidateFactoryManualUnityOptional.ToString().ToLowerInvariant(),
        "samplePackageUnmodified="
            + result.Report.GamePackageCandidateFactorySamplePackageUnmodified.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.GamePackageCandidateFactoryProjectionOnly.ToString().ToLowerInvariant(),
        "evidencePath="
            + result.Report.GamePackageCandidateFactoryEvidencePath,
        "exportPath="
            + result.Report.GamePackageCandidateFactoryExportPath,
        "gamePackageCandidateFactoryQualityGatePassed="
            + result.Report.GamePackageCandidateFactoryQualityGatePassed.ToString().ToLowerInvariant(),
        "goal130FilesDiscoveredByRelativePaths="
            + result.Report.Goal130FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGamePackageCandidateFactoryEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "candidateFactoryStatus: "
            + entry.GamePackageCandidateFactoryStatus,
        "candidateCount: "
            + entry.GamePackageCandidateFactoryCandidateCount,
        "passedCandidates: "
            + entry.GamePackageCandidateFactoryPassedCandidates,
        "failedCandidates: "
            + entry.GamePackageCandidateFactoryFailedCandidates,
        "matrixPassed: "
            + entry.GamePackageCandidateFactoryMatrixPassed.ToString().ToLowerInvariant(),
        "candidateIndexPath: "
            + entry.GamePackageCandidateFactoryCandidateIndexPath,
        "normalCommand: "
            + entry.GamePackageCandidateFactoryNormalCommand,
        "factoryResultPath: "
            + entry.GamePackageCandidateFactoryResultPath,
        "matrixResultPath: "
            + entry.GamePackageCandidateFactoryMatrixResultPath,
        "manualUnityOptional: "
            + entry.GamePackageCandidateFactoryManualUnityOptional.ToString().ToLowerInvariant(),
        "samplePackageUnmodified: "
            + entry.GamePackageCandidateFactorySamplePackageUnmodified.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.GamePackageCandidateFactoryProjectionOnly.ToString().ToLowerInvariant(),
        "evidencePath: "
            + entry.GamePackageCandidateFactoryEvidencePath,
        "exportPath: "
            + entry.GamePackageCandidateFactoryExportPath
    ];
}
