namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal136ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            CanonicalRuntimePlayerCommandLoopCandidateId =
                qualityGate.CanonicalRuntimePlayerCommandLoopCandidateId,
            CanonicalRuntimePlayerCommandLoopPassed =
                qualityGate.CanonicalRuntimePlayerCommandLoopPassed,
            CanonicalRuntimePlayerCommandCount =
                qualityGate.CanonicalRuntimePlayerCommandCount,
            CanonicalRuntimePlayerSnapshotCount =
                qualityGate.CanonicalRuntimePlayerSnapshotCount,
            CanonicalRuntimePlayerCommandLoopRuntimeEventCount =
                qualityGate.CanonicalRuntimePlayerCommandLoopRuntimeEventCount,
            CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent =
                qualityGate.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent,
            CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots =
                qualityGate.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots,
            CanonicalRuntimePlayerCommandLoopProjectionOnly =
                qualityGate.CanonicalRuntimePlayerCommandLoopProjectionOnly,
            CanonicalRuntimePlayerCommandLoopUnityGameplayTruth =
                qualityGate.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth,
            CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors =
                qualityGate.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors,
            CanonicalRuntimePlayerCommandLoopNormalCommand =
                qualityGate.CanonicalRuntimePlayerCommandLoopNormalCommand,
            CanonicalRuntimePlayerCommandLoopReportPath =
                qualityGate.CanonicalRuntimePlayerCommandLoopReportPath,
            CanonicalRuntimePlayerCommandLoopMatrixResultPath =
                qualityGate.CanonicalRuntimePlayerCommandLoopMatrixResultPath,
            CanonicalRuntimePlayerCommandLoopManualUnityOptional =
                qualityGate.CanonicalRuntimePlayerCommandLoopManualUnityOptional,
            CanonicalRuntimePlayerCommandLoopAccepted =
                qualityGate.CanonicalRuntimePlayerCommandLoopAccepted,
            CanonicalRuntimePlayerCommandLoopQualityGatePassed =
                qualityGate.CanonicalRuntimePlayerCommandLoopQualityGatePassed,
            CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths =
                qualityGate.CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths
        };
}
