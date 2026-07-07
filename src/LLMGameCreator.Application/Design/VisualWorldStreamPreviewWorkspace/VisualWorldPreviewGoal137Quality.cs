using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality
        BuildGoal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "canonical_runtime_unity_player_loop_playback");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "canonical_runtime_unity_player_loop_playback_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal137.unity_player_loop_playback.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal137AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && !string.IsNullOrWhiteSpace(summary.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId)
            && summary.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount >= 13
            && summary.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent
            && summary.CanonicalRuntimeUnityPlayerLoopPlaybackPassed
            && summary.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource
            && !summary.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth
            && !summary.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly
            && summary.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime
            && summary.CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional
            && !summary.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted
            && relativePaths
            && proofPassed;

        return new Goal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality(
            GroupPresent: group is not null,
            CandidateId: summary?.CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId ?? string.Empty,
            PlaybackFrameCount:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount ?? 0,
            RequiredFrameCategoriesPresent:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent == true,
            UnityPlayerLoopPlaybackPassed:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackPassed == true,
            RuntimeSnapshotSource:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource == true,
            UnityGameplayTruth:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth == true,
            ProjectionOnly:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly == true,
            SelectedCandidateExecutedByRuntime:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime == true,
            NormalCommand:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand ?? string.Empty,
            ReportPath:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackReportPath ?? string.Empty,
            MatrixResultPath:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath ?? string.Empty,
            ManualUnityOptional:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional == true,
            Accepted:
                summary?.CanonicalRuntimeUnityPlayerLoopPlaybackAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal137CanonicalRuntimeUnityPlayerLoopPlaybackQualityDiagnostics(
        Goal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality playback,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!playback.GroupPresent)
        {
            return;
        }

        AddIfFalse(playback.PlaybackFrameCount >= 13,
            "goal137.quality.playback_frame_count",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(playback.RequiredFrameCategoriesPresent,
            "goal137.quality.required_frame_categories",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(playback.UnityPlayerLoopPlaybackPassed,
            "goal137.quality.unity_player_loop_playback",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(playback.RuntimeSnapshotSource,
            "goal137.quality.runtime_snapshot_source",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(!playback.UnityGameplayTruth,
            "goal137.quality.unity_gameplay_truth",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(!playback.ProjectionOnly,
            "goal137.quality.projection_only",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(playback.SelectedCandidateExecutedByRuntime,
            "goal137.quality.selected_candidate_executed_by_runtime",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(!playback.Accepted,
            "goal137.quality.accepted_must_stay_false",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback,
            "goal137.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(playback.RelativePaths,
            "goal137.quality.relative_paths",
            "canonical_runtime_unity_player_loop_playback",
            diagnostics);
    }

    private static bool Goal137AllowedPath(string path) =>
        path.StartsWith(
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality playback,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            CanonicalRuntimeUnityPlayerLoopPlaybackGroupPresent = playback.GroupPresent,
            CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId = playback.CandidateId,
            CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount =
                playback.PlaybackFrameCount,
            CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent =
                playback.RequiredFrameCategoriesPresent,
            CanonicalRuntimeUnityPlayerLoopPlaybackPassed =
                playback.UnityPlayerLoopPlaybackPassed,
            CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource =
                playback.RuntimeSnapshotSource,
            CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth =
                playback.UnityGameplayTruth,
            CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly =
                playback.ProjectionOnly,
            CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime =
                playback.SelectedCandidateExecutedByRuntime,
            CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand =
                playback.NormalCommand,
            CanonicalRuntimeUnityPlayerLoopPlaybackReportPath =
                playback.ReportPath,
            CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath =
                playback.MatrixResultPath,
            CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional =
                playback.ManualUnityOptional,
            CanonicalRuntimeUnityPlayerLoopPlaybackAccepted =
                playback.Accepted,
            CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths =
                playback.RelativePaths,
            CanonicalRuntimeUnityPlayerLoopPlaybackWinFormsBindingReal =
                binding.PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback,
            CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed =
                playback.QualityGatePassed
                && binding.PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback,
            Passed = qualityGate.Passed
                     && (!playback.GroupPresent
                         || playback.QualityGatePassed
                         && binding.PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback)
        };

    private sealed record Goal137CanonicalRuntimeUnityPlayerLoopPlaybackQuality(
        bool GroupPresent,
        string CandidateId,
        int PlaybackFrameCount,
        bool RequiredFrameCategoriesPresent,
        bool UnityPlayerLoopPlaybackPassed,
        bool RuntimeSnapshotSource,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        bool SelectedCandidateExecutedByRuntime,
        string NormalCommand,
        string ReportPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
