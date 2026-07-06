namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal134ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            CanonicalRuntimeCandidateId = qualityGate.CanonicalRuntimeCandidateId,
            CanonicalRuntimePackageValidationPassed =
                qualityGate.CanonicalRuntimePackageValidationPassed,
            CanonicalRuntimePassed = qualityGate.CanonicalRuntimePassed,
            CanonicalRuntimeCommandCount = qualityGate.CanonicalRuntimeCommandCount,
            CanonicalRuntimeEventCount = qualityGate.CanonicalRuntimeEventCount,
            CanonicalRuntimeSaveLoadReplayPassed =
                qualityGate.CanonicalRuntimeSaveLoadReplayPassed,
            CanonicalRuntimeUnityPlayerConsumedTranscript =
                qualityGate.CanonicalRuntimeUnityPlayerConsumedTranscript,
            CanonicalRuntimeProjectionOnly = qualityGate.CanonicalRuntimeProjectionOnly,
            CanonicalRuntimeSelectedCandidateExecutedByRuntime =
                qualityGate.CanonicalRuntimeSelectedCandidateExecutedByRuntime,
            CanonicalRuntimeNormalCommand = qualityGate.CanonicalRuntimeNormalCommand,
            CanonicalRuntimeReportPath = qualityGate.CanonicalRuntimeReportPath,
            CanonicalRuntimeMatrixResultPath = qualityGate.CanonicalRuntimeMatrixResultPath,
            CanonicalRuntimeManualUnityOptional =
                qualityGate.CanonicalRuntimeManualUnityOptional,
            CanonicalRuntimeQualityGatePassed =
                qualityGate.CanonicalRuntimeQualityGatePassed,
            CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths =
                qualityGate.CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths
        };
}
