namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal135ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            CanonicalRuntimePlayerLoopCandidateId =
                qualityGate.CanonicalRuntimePlayerLoopCandidateId,
            CanonicalRuntimePlayerLoopAdapterContractPresent =
                qualityGate.CanonicalRuntimePlayerLoopAdapterContractPresent,
            CanonicalRuntimePlayerLoopStepCount =
                qualityGate.CanonicalRuntimePlayerLoopStepCount,
            CanonicalRuntimePlayerLoopRequiredCategoriesPresent =
                qualityGate.CanonicalRuntimePlayerLoopRequiredCategoriesPresent,
            CanonicalRuntimePlayerLoopUnityReadinessPassed =
                qualityGate.CanonicalRuntimePlayerLoopUnityReadinessPassed,
            CanonicalRuntimePlayerLoopSource =
                qualityGate.CanonicalRuntimePlayerLoopSource,
            CanonicalRuntimePlayerLoopUnityGameplayTruth =
                qualityGate.CanonicalRuntimePlayerLoopUnityGameplayTruth,
            CanonicalRuntimePlayerLoopProjectionOnly =
                qualityGate.CanonicalRuntimePlayerLoopProjectionOnly,
            CanonicalRuntimePlayerLoopNoUnclassifiedErrors =
                qualityGate.CanonicalRuntimePlayerLoopNoUnclassifiedErrors,
            CanonicalRuntimePlayerLoopNormalCommand =
                qualityGate.CanonicalRuntimePlayerLoopNormalCommand,
            CanonicalRuntimePlayerLoopReportPath =
                qualityGate.CanonicalRuntimePlayerLoopReportPath,
            CanonicalRuntimePlayerLoopManualUnityOptional =
                qualityGate.CanonicalRuntimePlayerLoopManualUnityOptional,
            CanonicalRuntimePlayerLoopQualityGatePassed =
                qualityGate.CanonicalRuntimePlayerLoopQualityGatePassed,
            CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths =
                qualityGate.CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths
        };
}
