using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGamePackageCandidateRecipePipelineDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "recipePipelineStatus="
            + result.Report.GamePackageCandidateRecipePipelineStatus,
        "recipeCount="
            + result.Report.GamePackageCandidateRecipePipelineRecipeCount,
        "candidateCount="
            + result.Report.GamePackageCandidateRecipePipelineCandidateCount,
        "passedCandidates="
            + result.Report.GamePackageCandidateRecipePipelinePassedCandidates,
        "failedCandidates="
            + result.Report.GamePackageCandidateRecipePipelineFailedCandidates,
        "matrixPassed="
            + result.Report.GamePackageCandidateRecipePipelineMatrixPassed.ToString().ToLowerInvariant(),
        "selectedCandidateId="
            + result.Report.GamePackageCandidateRecipePipelineSelectedCandidateId,
        "selectedCandidateScore="
            + result.Report.GamePackageCandidateRecipePipelineSelectedCandidateScore,
        "recipeCatalogPath="
            + result.Report.GamePackageCandidateRecipePipelineRecipeCatalogPath,
        "candidateIndexPath="
            + result.Report.GamePackageCandidateRecipePipelineCandidateIndexPath,
        "normalCommand="
            + result.Report.GamePackageCandidateRecipePipelineNormalCommand,
        "pipelineResultPath="
            + result.Report.GamePackageCandidateRecipePipelineResultPath,
        "scoringResultPath="
            + result.Report.GamePackageCandidateRecipePipelineScoringResultPath,
        "matrixResultPath="
            + result.Report.GamePackageCandidateRecipePipelineMatrixResultPath,
        "selectedCandidatePackagePath="
            + result.Report.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath,
        "selectedCandidateHandoffPath="
            + result.Report.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath,
        "manualUnityOptional="
            + result.Report.GamePackageCandidateRecipePipelineManualUnityOptional.ToString().ToLowerInvariant(),
        "samplePackageUnmodified="
            + result.Report.GamePackageCandidateRecipePipelineSamplePackageUnmodified.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.GamePackageCandidateRecipePipelineProjectionOnly.ToString().ToLowerInvariant(),
        "metadataOnlyRecipeMutation="
            + result.Report.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation.ToString().ToLowerInvariant(),
        "evidencePath="
            + result.Report.GamePackageCandidateRecipePipelineEvidencePath,
        "exportPath="
            + result.Report.GamePackageCandidateRecipePipelineExportPath,
        "gamePackageCandidateRecipePipelineQualityGatePassed="
            + result.Report.GamePackageCandidateRecipePipelineQualityGatePassed.ToString().ToLowerInvariant(),
        "goal131FilesDiscoveredByRelativePaths="
            + result.Report.Goal131FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGamePackageCandidateRecipePipelineEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "recipePipelineStatus: "
            + entry.GamePackageCandidateRecipePipelineStatus,
        "recipeCount: "
            + entry.GamePackageCandidateRecipePipelineRecipeCount,
        "candidateCount: "
            + entry.GamePackageCandidateRecipePipelineCandidateCount,
        "passedCandidates: "
            + entry.GamePackageCandidateRecipePipelinePassedCandidates,
        "failedCandidates: "
            + entry.GamePackageCandidateRecipePipelineFailedCandidates,
        "matrixPassed: "
            + entry.GamePackageCandidateRecipePipelineMatrixPassed.ToString().ToLowerInvariant(),
        "selectedCandidateId: "
            + entry.GamePackageCandidateRecipePipelineSelectedCandidateId,
        "selectedCandidateScore: "
            + entry.GamePackageCandidateRecipePipelineSelectedCandidateScore,
        "recipeCatalogPath: "
            + entry.GamePackageCandidateRecipePipelineRecipeCatalogPath,
        "candidateIndexPath: "
            + entry.GamePackageCandidateRecipePipelineCandidateIndexPath,
        "normalCommand: "
            + entry.GamePackageCandidateRecipePipelineNormalCommand,
        "pipelineResultPath: "
            + entry.GamePackageCandidateRecipePipelineResultPath,
        "scoringResultPath: "
            + entry.GamePackageCandidateRecipePipelineScoringResultPath,
        "matrixResultPath: "
            + entry.GamePackageCandidateRecipePipelineMatrixResultPath,
        "selectedCandidatePackagePath: "
            + entry.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath,
        "selectedCandidateHandoffPath: "
            + entry.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath,
        "manualUnityOptional: "
            + entry.GamePackageCandidateRecipePipelineManualUnityOptional.ToString().ToLowerInvariant(),
        "samplePackageUnmodified: "
            + entry.GamePackageCandidateRecipePipelineSamplePackageUnmodified.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.GamePackageCandidateRecipePipelineProjectionOnly.ToString().ToLowerInvariant(),
        "metadataOnlyRecipeMutation: "
            + entry.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation.ToString().ToLowerInvariant(),
        "evidencePath: "
            + entry.GamePackageCandidateRecipePipelineEvidencePath,
        "exportPath: "
            + entry.GamePackageCandidateRecipePipelineExportPath
    ];
}
