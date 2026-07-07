namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal137ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Canonical Runtime Unity Player Loop Playback",
            string.Empty,
            $"- candidateId: {report.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId}",
            $"- playbackFrameCount: {report.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount}",
            $"- requiredFrameCategoriesPresent: {report.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- unityPlayerLoopPlaybackPassed: {report.CanonicalRuntimeUnityPlayerLoopPlaybackPassed.ToString().ToLowerInvariant()}",
            $"- runtimeSnapshotSource: {report.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {report.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly.ToString().ToLowerInvariant()}",
            $"- selectedCandidateExecutedByRuntime: {report.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime.ToString().ToLowerInvariant()}",
            $"- normalCommand: {report.CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand}",
            $"- reportPath: {report.CanonicalRuntimeUnityPlayerLoopPlaybackReportPath}",
            $"- matrixResultPath: {report.CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath}",
            $"- manualUnityOptional: {report.CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- accepted: {report.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed: {report.CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal137FilesDiscoveredByRelativePaths: {report.CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal137QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal137 Quality",
            string.Empty,
            $"- canonicalRuntimeUnityPlayerLoopPlaybackGroupPresent: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackGroupPresent.ToString().ToLowerInvariant()}",
            $"- candidateId: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId}",
            $"- playbackFrameCount: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount}",
            $"- requiredFrameCategoriesPresent: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent.ToString().ToLowerInvariant()}",
            $"- unityPlayerLoopPlaybackPassed: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackPassed.ToString().ToLowerInvariant()}",
            $"- runtimeSnapshotSource: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource.ToString().ToLowerInvariant()}",
            $"- unityGameplayTruth: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly.ToString().ToLowerInvariant()}",
            $"- selectedCandidateExecutedByRuntime: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime.ToString().ToLowerInvariant()}",
            $"- accepted: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeUnityPlayerLoopPlaybackWinFormsBindingReal: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackWinFormsBindingReal.ToString().ToLowerInvariant()}",
            $"- canonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed: {qualityGate.CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed.ToString().ToLowerInvariant()}"
        ]);

    private static string RenderWithGoal137Lines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate)
    {
        AddGoal137ReportLines(lines, report);
        AddGoal137QualityLines(lines, qualityGate);
        return RenderWithGoal138Lines(lines, report, qualityGate);
    }
}
