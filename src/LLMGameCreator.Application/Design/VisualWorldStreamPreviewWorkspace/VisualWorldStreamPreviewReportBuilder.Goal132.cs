namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal132ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            CandidatePipelineOperatorStatus = qualityGate.CandidatePipelineOperatorStatus,
            CandidatePipelineOperatorNormalCommand = qualityGate.CandidatePipelineOperatorNormalCommand,
            CandidatePipelineOperatorDryRunCommand = qualityGate.CandidatePipelineOperatorDryRunCommand,
            CandidatePipelineOperatorResultPath = qualityGate.CandidatePipelineOperatorResultPath,
            CandidatePipelineOperatorSelectedCandidateId =
                qualityGate.CandidatePipelineOperatorSelectedCandidateId,
            CandidatePipelineOperatorSelectedCandidateScore =
                qualityGate.CandidatePipelineOperatorSelectedCandidateScore,
            CandidatePipelineOperatorCandidateCount =
                qualityGate.CandidatePipelineOperatorCandidateCount,
            CandidatePipelineOperatorPassedCandidates =
                qualityGate.CandidatePipelineOperatorPassedCandidates,
            CandidatePipelineOperatorFailedCandidates =
                qualityGate.CandidatePipelineOperatorFailedCandidates,
            CandidatePipelineOperatorMatrixPassed =
                qualityGate.CandidatePipelineOperatorMatrixPassed,
            CandidatePipelineOperatorLastExitCode =
                qualityGate.CandidatePipelineOperatorLastExitCode,
            CandidatePipelineOperatorLastDurationMilliseconds =
                qualityGate.CandidatePipelineOperatorLastDurationMilliseconds,
            CandidatePipelineOperatorOutputTail =
                qualityGate.CandidatePipelineOperatorOutputTail,
            CandidatePipelineOperatorManualUnityOptional =
                qualityGate.CandidatePipelineOperatorManualUnityOptional,
            CandidatePipelineOperatorProjectionOnly =
                qualityGate.CandidatePipelineOperatorProjectionOnly,
            CandidatePipelineOperatorSamplePackageReadOnly =
                qualityGate.CandidatePipelineOperatorSamplePackageReadOnly,
            CandidatePipelineOperatorWinFormsPanelPresent =
                qualityGate.CandidatePipelineOperatorWinFormsPanelPresent,
            CandidatePipelineOperatorRefreshButtonPresent =
                qualityGate.CandidatePipelineOperatorRefreshButtonPresent,
            CandidatePipelineOperatorCopyCommandButtonPresent =
                qualityGate.CandidatePipelineOperatorCopyCommandButtonPresent,
            CandidatePipelineOperatorDryRunButtonPresent =
                qualityGate.CandidatePipelineOperatorDryRunButtonPresent,
            CandidatePipelineOperatorRunButtonPresent =
                qualityGate.CandidatePipelineOperatorRunButtonPresent,
            CandidatePipelineOperatorAsyncRunPresent =
                qualityGate.CandidatePipelineOperatorAsyncRunPresent,
            CandidatePipelineOperatorResultPresent =
                qualityGate.CandidatePipelineOperatorResultPresent,
            CandidatePipelineOperatorEvidencePath =
                qualityGate.CandidatePipelineOperatorEvidencePath,
            CandidatePipelineOperatorExportPath =
                qualityGate.CandidatePipelineOperatorExportPath,
            CandidatePipelineOperatorQualityGatePassed =
                qualityGate.CandidatePipelineOperatorQualityGatePassed,
            Goal132FilesDiscoveredByRelativePaths =
                qualityGate.Goal132FilesDiscoveredByRelativePaths
        };
}
