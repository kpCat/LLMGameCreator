namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal142ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Product-Line Runtime Variant Matrix",
            string.Empty,
            $"- matrixStatus: {report.ProductLineRuntimeVariantMatrixStatus}",
            $"- candidateCount: {report.ProductLineRuntimeVariantCandidateCount}",
            $"- passedCandidateCount: {report.ProductLineRuntimeVariantPassedCandidateCount}",
            $"- failedCandidateCount: {report.ProductLineRuntimeVariantFailedCandidateCount}",
            $"- runtimeSignificantCandidateCount: {report.ProductLineRuntimeVariantRuntimeSignificantCandidateCount}",
            $"- distinctFinalStateHashCount: {report.ProductLineRuntimeVariantDistinctFinalStateHashCount}",
            $"- selectedCandidateId: {report.ProductLineRuntimeVariantSelectedCandidateId}",
            $"- selectedVariantKind: {report.ProductLineRuntimeVariantSelectedVariantKind}",
            $"- selectedScore: {report.ProductLineRuntimeVariantSelectedScore}",
            $"- sourceTemplateUnmodified: {report.ProductLineRuntimeVariantSourceTemplateUnmodified.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.ProductLineRuntimeVariantNormalCommand}",
            $"- matrixResultPath: {report.ProductLineRuntimeVariantMatrixResultPath}",
            $"- selectedHandoffPath: {report.ProductLineRuntimeVariantSelectedHandoffPath}",
            $"- accepted: {report.ProductLineRuntimeVariantAccepted.ToString().ToLowerInvariant()}",
            $"- productLineRuntimeVariantQualityGatePassed: {report.ProductLineRuntimeVariantQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal142FilesDiscoveredByRelativePaths: {report.ProductLineRuntimeVariantFilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal142QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal142 Quality",
            string.Empty,
            $"- productLineRuntimeVariantMatrixGroupPresent: {qualityGate.ProductLineRuntimeVariantMatrixGroupPresent.ToString().ToLowerInvariant()}",
            $"- matrixStatus: {qualityGate.ProductLineRuntimeVariantMatrixStatus}",
            $"- candidateCount: {qualityGate.ProductLineRuntimeVariantCandidateCount}",
            $"- passedCandidateCount: {qualityGate.ProductLineRuntimeVariantPassedCandidateCount}",
            $"- failedCandidateCount: {qualityGate.ProductLineRuntimeVariantFailedCandidateCount}",
            $"- runtimeSignificantCandidateCount: {qualityGate.ProductLineRuntimeVariantRuntimeSignificantCandidateCount}",
            $"- distinctFinalStateHashCount: {qualityGate.ProductLineRuntimeVariantDistinctFinalStateHashCount}",
            $"- selectedCandidateId: {qualityGate.ProductLineRuntimeVariantSelectedCandidateId}",
            $"- selectedVariantKind: {qualityGate.ProductLineRuntimeVariantSelectedVariantKind}",
            $"- selectedScore: {qualityGate.ProductLineRuntimeVariantSelectedScore}",
            $"- sourceTemplateUnmodified: {qualityGate.ProductLineRuntimeVariantSourceTemplateUnmodified.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.ProductLineRuntimeVariantAccepted.ToString().ToLowerInvariant()}",
            $"- productLineRuntimeVariantWinFormsBindingReal: {qualityGate.ProductLineRuntimeVariantWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- productLineRuntimeVariantQualityGatePassed: {qualityGate.ProductLineRuntimeVariantQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal142Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal142ReportLines(lines, report);
        AddGoal142QualityLines(lines, qualityGate);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
