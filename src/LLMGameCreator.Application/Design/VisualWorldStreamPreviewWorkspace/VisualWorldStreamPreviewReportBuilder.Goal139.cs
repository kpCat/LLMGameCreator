namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal139ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138 =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138,
            RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId,
            RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount,
            RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority,
            RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly,
            RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand,
            RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath,
            RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional,
            RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted,
            RuntimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsFilesDiscoveredByRelativePaths =
                qualityGate.RuntimeBackedUnityPlayerLoopInteractiveControlsFilesDiscoveredByRelativePaths
        };
}
