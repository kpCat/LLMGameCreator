namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal132ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## WinForms Candidate Pipeline Operator Panel",
            string.Empty,
            $"- candidatePipelineOperatorStatus: {report.CandidatePipelineOperatorStatus}",
            $"- normalCommand: {report.CandidatePipelineOperatorNormalCommand}",
            $"- dryRunCommand: {report.CandidatePipelineOperatorDryRunCommand}",
            $"- resultPath: {report.CandidatePipelineOperatorResultPath}",
            $"- selectedCandidateId: {report.CandidatePipelineOperatorSelectedCandidateId}",
            $"- selectedCandidateScore: {report.CandidatePipelineOperatorSelectedCandidateScore}",
            $"- candidateCount: {report.CandidatePipelineOperatorCandidateCount}",
            $"- passedCandidates: {report.CandidatePipelineOperatorPassedCandidates}",
            $"- failedCandidates: {report.CandidatePipelineOperatorFailedCandidates}",
            $"- matrixPassed: {report.CandidatePipelineOperatorMatrixPassed.ToString().ToLowerInvariant()}",
            $"- lastOperatorExitCode: {report.CandidatePipelineOperatorLastExitCode}",
            $"- lastOperatorDurationMilliseconds: {report.CandidatePipelineOperatorLastDurationMilliseconds}",
            $"- manualUnityOptional: {report.CandidatePipelineOperatorManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.CandidatePipelineOperatorProjectionOnly.ToString().ToLowerInvariant()}",
            $"- samplePackageReadOnly: {report.CandidatePipelineOperatorSamplePackageReadOnly.ToString().ToLowerInvariant()}",
            $"- winFormsPanelPresent: {report.CandidatePipelineOperatorWinFormsPanelPresent.ToString().ToLowerInvariant()}",
            $"- refreshButtonPresent: {report.CandidatePipelineOperatorRefreshButtonPresent.ToString().ToLowerInvariant()}",
            $"- copyCommandButtonPresent: {report.CandidatePipelineOperatorCopyCommandButtonPresent.ToString().ToLowerInvariant()}",
            $"- dryRunButtonPresent: {report.CandidatePipelineOperatorDryRunButtonPresent.ToString().ToLowerInvariant()}",
            $"- runButtonPresent: {report.CandidatePipelineOperatorRunButtonPresent.ToString().ToLowerInvariant()}",
            $"- asyncRunPresent: {report.CandidatePipelineOperatorAsyncRunPresent.ToString().ToLowerInvariant()}",
            $"- operatorResultPresent: {report.CandidatePipelineOperatorResultPresent.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.CandidatePipelineOperatorEvidencePath}",
            $"- exportPath: {report.CandidatePipelineOperatorExportPath}",
            $"- candidatePipelineOperatorQualityGatePassed: {report.CandidatePipelineOperatorQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal132FilesDiscoveredByRelativePaths: {report.Goal132FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal132QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal132 Quality",
            string.Empty,
            $"- candidatePipelineOperatorGroupPresent: {qualityGate.CandidatePipelineOperatorGroupPresent.ToString().ToLowerInvariant()}",
            $"- candidatePipelineOperatorStatus: {qualityGate.CandidatePipelineOperatorStatus}",
            $"- normalCommand: {qualityGate.CandidatePipelineOperatorNormalCommand}",
            $"- dryRunCommand: {qualityGate.CandidatePipelineOperatorDryRunCommand}",
            $"- resultPath: {qualityGate.CandidatePipelineOperatorResultPath}",
            $"- selectedCandidateId: {qualityGate.CandidatePipelineOperatorSelectedCandidateId}",
            $"- selectedCandidateScore: {qualityGate.CandidatePipelineOperatorSelectedCandidateScore}",
            $"- candidateCount: {qualityGate.CandidatePipelineOperatorCandidateCount}",
            $"- passedCandidates: {qualityGate.CandidatePipelineOperatorPassedCandidates}",
            $"- failedCandidates: {qualityGate.CandidatePipelineOperatorFailedCandidates}",
            $"- matrixPassed: {qualityGate.CandidatePipelineOperatorMatrixPassed.ToString().ToLowerInvariant()}",
            $"- winFormsPanelPresent: {qualityGate.CandidatePipelineOperatorWinFormsPanelPresent.ToString().ToLowerInvariant()}",
            $"- refreshButtonPresent: {qualityGate.CandidatePipelineOperatorRefreshButtonPresent.ToString().ToLowerInvariant()}",
            $"- copyCommandButtonPresent: {qualityGate.CandidatePipelineOperatorCopyCommandButtonPresent.ToString().ToLowerInvariant()}",
            $"- dryRunButtonPresent: {qualityGate.CandidatePipelineOperatorDryRunButtonPresent.ToString().ToLowerInvariant()}",
            $"- runButtonPresent: {qualityGate.CandidatePipelineOperatorRunButtonPresent.ToString().ToLowerInvariant()}",
            $"- asyncRunPresent: {qualityGate.CandidatePipelineOperatorAsyncRunPresent.ToString().ToLowerInvariant()}",
            $"- candidatePipelineOperatorQualityGatePassed: {qualityGate.CandidatePipelineOperatorQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal132FilesDiscoveredByRelativePaths: {qualityGate.Goal132FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsCandidatePipelineOperatorBindingReal: {qualityGate.WinFormsCandidatePipelineOperatorBindingReal.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal132Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal131ReportLines(lines, report);
        AddGoal131QualityLines(lines, qualityGate);
        AddGoal132ReportLines(lines, report);
        AddGoal132QualityLines(lines, qualityGate);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
