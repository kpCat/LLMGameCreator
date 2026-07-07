namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal138ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137 =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137,
            RuntimeBackedUnityPlayerLoopStepperCandidateId =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperCandidateId,
            RuntimeBackedUnityPlayerLoopStepperFrameCount =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperFrameCount,
            RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent,
            RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority,
            RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopStepperProjectionOnly =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperProjectionOnly,
            RuntimeBackedUnityPlayerLoopStepperWindowPresent =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperWindowPresent,
            RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed,
            RuntimeBackedUnityPlayerLoopStepperNormalCommand =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperNormalCommand,
            RuntimeBackedUnityPlayerLoopStepperReportPath =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperReportPath,
            RuntimeBackedUnityPlayerLoopStepperManualUnityOptional =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperManualUnityOptional,
            RuntimeBackedUnityPlayerLoopStepperAccepted =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperAccepted,
            RuntimeBackedUnityPlayerLoopStepperQualityGatePassed =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperQualityGatePassed,
            RuntimeBackedUnityPlayerLoopStepperFilesDiscoveredByRelativePaths =
                qualityGate.RuntimeBackedUnityPlayerLoopStepperFilesDiscoveredByRelativePaths
        };
}
