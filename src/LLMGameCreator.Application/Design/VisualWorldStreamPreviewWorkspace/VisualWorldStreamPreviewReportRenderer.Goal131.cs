namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal131ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## GamePackage Candidate Recipe Catalog Scoring and Promotion",
            string.Empty,
            $"- recipePipelineStatus: {report.GamePackageCandidateRecipePipelineStatus}",
            $"- recipeCount: {report.GamePackageCandidateRecipePipelineRecipeCount}",
            $"- candidateCount: {report.GamePackageCandidateRecipePipelineCandidateCount}",
            $"- passedCandidates: {report.GamePackageCandidateRecipePipelinePassedCandidates}",
            $"- failedCandidates: {report.GamePackageCandidateRecipePipelineFailedCandidates}",
            $"- matrixPassed: {report.GamePackageCandidateRecipePipelineMatrixPassed.ToString().ToLowerInvariant()}",
            $"- selectedCandidateId: {report.GamePackageCandidateRecipePipelineSelectedCandidateId}",
            $"- selectedCandidateScore: {report.GamePackageCandidateRecipePipelineSelectedCandidateScore}",
            $"- recipeCatalogPath: {report.GamePackageCandidateRecipePipelineRecipeCatalogPath}",
            $"- candidateIndexPath: {report.GamePackageCandidateRecipePipelineCandidateIndexPath}",
            $"- normalCommand: {report.GamePackageCandidateRecipePipelineNormalCommand}",
            $"- pipelineResultPath: {report.GamePackageCandidateRecipePipelineResultPath}",
            $"- scoringResultPath: {report.GamePackageCandidateRecipePipelineScoringResultPath}",
            $"- matrixResultPath: {report.GamePackageCandidateRecipePipelineMatrixResultPath}",
            $"- selectedCandidatePackagePath: {report.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath}",
            $"- selectedCandidateHandoffPath: {report.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath}",
            $"- manualUnityOptional: {report.GamePackageCandidateRecipePipelineManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- samplePackageUnmodified: {report.GamePackageCandidateRecipePipelineSamplePackageUnmodified.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.GamePackageCandidateRecipePipelineProjectionOnly.ToString().ToLowerInvariant()}",
            $"- metadataOnlyRecipeMutation: {report.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.GamePackageCandidateRecipePipelineEvidencePath}",
            $"- exportPath: {report.GamePackageCandidateRecipePipelineExportPath}",
            $"- gamePackageCandidateRecipePipelineQualityGatePassed: {report.GamePackageCandidateRecipePipelineQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal131FilesDiscoveredByRelativePaths: {report.Goal131FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal131QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal131 Quality",
            string.Empty,
            $"- gamePackageCandidateRecipePipelineGroupPresent: {qualityGate.GamePackageCandidateRecipePipelineGroupPresent.ToString().ToLowerInvariant()}",
            $"- recipePipelineStatus: {qualityGate.GamePackageCandidateRecipePipelineStatus}",
            $"- recipeCount: {qualityGate.GamePackageCandidateRecipePipelineRecipeCount}",
            $"- candidateCount: {qualityGate.GamePackageCandidateRecipePipelineCandidateCount}",
            $"- passedCandidates: {qualityGate.GamePackageCandidateRecipePipelinePassedCandidates}",
            $"- failedCandidates: {qualityGate.GamePackageCandidateRecipePipelineFailedCandidates}",
            $"- matrixPassed: {qualityGate.GamePackageCandidateRecipePipelineMatrixPassed.ToString().ToLowerInvariant()}",
            $"- selectedCandidateId: {qualityGate.GamePackageCandidateRecipePipelineSelectedCandidateId}",
            $"- selectedCandidateScore: {qualityGate.GamePackageCandidateRecipePipelineSelectedCandidateScore}",
            $"- recipeCatalogPath: {qualityGate.GamePackageCandidateRecipePipelineRecipeCatalogPath}",
            $"- normalCommand: {qualityGate.GamePackageCandidateRecipePipelineNormalCommand}",
            $"- pipelineResultPath: {qualityGate.GamePackageCandidateRecipePipelineResultPath}",
            $"- scoringResultPath: {qualityGate.GamePackageCandidateRecipePipelineScoringResultPath}",
            $"- selectedCandidatePackagePath: {qualityGate.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath}",
            $"- selectedCandidateHandoffPath: {qualityGate.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath}",
            $"- metadataOnlyRecipeMutation: {qualityGate.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation.ToString().ToLowerInvariant()}",
            $"- gamePackageCandidateRecipePipelineQualityGatePassed: {qualityGate.GamePackageCandidateRecipePipelineQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal131FilesDiscoveredByRelativePaths: {qualityGate.Goal131FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGamePackageCandidateRecipePipelineBindingReal: {qualityGate.WinFormsGamePackageCandidateRecipePipelineBindingReal.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal131Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal131ReportLines(lines, report);
        AddGoal131QualityLines(lines, qualityGate);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
