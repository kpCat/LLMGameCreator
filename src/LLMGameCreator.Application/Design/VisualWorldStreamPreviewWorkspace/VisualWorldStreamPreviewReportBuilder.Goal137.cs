namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal137ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId,
            CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount,
            CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent,
            CanonicalRuntimeUnityPlayerLoopPlaybackPassed =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackPassed,
            CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource,
            CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth,
            CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly,
            CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime,
            CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand,
            CanonicalRuntimeUnityPlayerLoopPlaybackReportPath =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackReportPath,
            CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath,
            CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional,
            CanonicalRuntimeUnityPlayerLoopPlaybackAccepted =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted,
            CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed,
            CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths =
                qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths
        };
}
