namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal140ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139 =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139,
            RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate,
            RuntimeBackedUnityPlayerLoopControlsUxFrameCount =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxFrameCount,
            RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering,
            RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear,
            RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear,
            RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified,
            RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount,
            RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount,
            RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed,
            RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority,
            RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly,
            RuntimeBackedUnityPlayerLoopControlsUxNormalCommand =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxNormalCommand,
            RuntimeBackedUnityPlayerLoopControlsUxReportPath =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxReportPath,
            RuntimeBackedUnityPlayerLoopControlsUxAccepted =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxAccepted,
            RuntimeBackedUnityPlayerLoopControlsUxQualityGatePassed =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxQualityGatePassed,
            RuntimeBackedUnityPlayerLoopControlsUxFilesDiscoveredByRelativePaths =
                qualityGate.RuntimeBackedUnityPlayerLoopControlsUxFilesDiscoveredByRelativePaths
        };
}
