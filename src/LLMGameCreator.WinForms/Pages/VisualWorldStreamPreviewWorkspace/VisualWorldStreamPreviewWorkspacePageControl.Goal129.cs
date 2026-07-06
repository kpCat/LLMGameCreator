using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGamePackageCandidateMatrixDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "gamePackageCandidateMatrixStatus="
            + result.Report.GamePackageCandidateMatrixStatus,
        "candidateCount="
            + result.Report.GamePackageCandidateMatrixCandidateCount,
        "passedCandidateCount="
            + result.Report.GamePackageCandidateMatrixPassedCandidateCount,
        "failedCandidateCount="
            + result.Report.GamePackageCandidateMatrixFailedCandidateCount,
        "candidateIndexPath="
            + result.Report.GamePackageCandidateMatrixCandidateIndexPath,
        "matrixResultPath="
            + result.Report.GamePackageCandidateMatrixResultPath,
        "normalCommand="
            + result.Report.GamePackageCandidateMatrixNormalCommand,
        "exampleCommand="
            + result.Report.GamePackageCandidateMatrixExampleCommand,
        "baselineCandidatePackagePath="
            + result.Report.GamePackageCandidateMatrixBaselineCandidatePackagePath,
        "variantCandidatePackagePath="
            + result.Report.GamePackageCandidateMatrixVariantCandidatePackagePath,
        "manualUnityOptional="
            + result.Report.GamePackageCandidateMatrixManualUnityOptional.ToString().ToLowerInvariant(),
        "cleanupApplied="
            + result.Report.GamePackageCandidateMatrixCleanupApplied.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.GamePackageCandidateMatrixProjectionOnly.ToString().ToLowerInvariant(),
        "gamePackageCandidateMatrixQualityGatePassed="
            + result.Report.GamePackageCandidateMatrixQualityGatePassed.ToString().ToLowerInvariant(),
        "goal129FilesDiscoveredByRelativePaths="
            + result.Report.Goal129FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGamePackageCandidateMatrixEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "gamePackageCandidateMatrixStatus: "
            + entry.GamePackageCandidateMatrixStatus,
        "candidateCount: "
            + entry.GamePackageCandidateMatrixCandidateCount,
        "passedCandidateCount: "
            + entry.GamePackageCandidateMatrixPassedCandidateCount,
        "failedCandidateCount: "
            + entry.GamePackageCandidateMatrixFailedCandidateCount,
        "candidateIndexPath: "
            + entry.GamePackageCandidateMatrixCandidateIndexPath,
        "matrixResultPath: "
            + entry.GamePackageCandidateMatrixResultPath,
        "normalCommand: "
            + entry.GamePackageCandidateMatrixNormalCommand,
        "exampleCommand: "
            + entry.GamePackageCandidateMatrixExampleCommand,
        "baselineCandidatePackagePath: "
            + entry.GamePackageCandidateMatrixBaselineCandidatePackagePath,
        "variantCandidatePackagePath: "
            + entry.GamePackageCandidateMatrixVariantCandidatePackagePath,
        "manualUnityOptional: "
            + entry.GamePackageCandidateMatrixManualUnityOptional.ToString().ToLowerInvariant(),
        "cleanupApplied: "
            + entry.GamePackageCandidateMatrixCleanupApplied.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.GamePackageCandidateMatrixProjectionOnly.ToString().ToLowerInvariant(),
        "scriptScanPassed: "
            + entry.GamePackageCandidateMatrixScriptScanPassed.ToString().ToLowerInvariant(),
        "matrixResultPassed: "
            + entry.GamePackageCandidateMatrixResultPassed.ToString().ToLowerInvariant(),
        "logScanPassed: "
            + entry.GamePackageCandidateMatrixLogScanPassed.ToString().ToLowerInvariant()
    ];
}
