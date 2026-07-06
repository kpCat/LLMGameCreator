using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal134CanonicalRuntimeSelectedCandidateQuality
        BuildGoal134CanonicalRuntimeSelectedCandidateQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "canonical_runtime_selected_candidate_playthrough");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "canonical_runtime_selected_candidate_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal134.canonical_runtime.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal134AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && !string.IsNullOrWhiteSpace(summary.CanonicalRuntimeCandidateId)
            && summary.CanonicalRuntimePackageValidationPassed
            && summary.CanonicalRuntimePassed
            && summary.CanonicalRuntimeCommandCount >= 6
            && summary.CanonicalRuntimeEventCount >= 6
            && summary.CanonicalRuntimeSaveLoadReplayPassed
            && summary.CanonicalRuntimeUnityPlayerConsumedTranscript
            && !summary.CanonicalRuntimeProjectionOnly
            && summary.CanonicalRuntimeSelectedCandidateExecutedByRuntime
            && summary.CanonicalRuntimeManualUnityOptional
            && relativePaths
            && proofPassed;

        return new Goal134CanonicalRuntimeSelectedCandidateQuality(
            GroupPresent: group is not null,
            CandidateId: summary?.CanonicalRuntimeCandidateId ?? string.Empty,
            PackageValidationPassed: summary?.CanonicalRuntimePackageValidationPassed == true,
            CanonicalRuntimePassed: summary?.CanonicalRuntimePassed == true,
            RuntimeCommandCount: summary?.CanonicalRuntimeCommandCount ?? 0,
            RuntimeEventCount: summary?.CanonicalRuntimeEventCount ?? 0,
            SaveLoadReplayPassed: summary?.CanonicalRuntimeSaveLoadReplayPassed == true,
            UnityPlayerConsumedTranscript:
                summary?.CanonicalRuntimeUnityPlayerConsumedTranscript == true,
            ProjectionOnly: summary?.CanonicalRuntimeProjectionOnly == true,
            SelectedCandidateExecutedByRuntime:
                summary?.CanonicalRuntimeSelectedCandidateExecutedByRuntime == true,
            NormalCommand: summary?.CanonicalRuntimeNormalCommand ?? string.Empty,
            ReportPath: summary?.CanonicalRuntimeReportPath ?? string.Empty,
            MatrixResultPath: summary?.CanonicalRuntimeMatrixResultPath ?? string.Empty,
            ManualUnityOptional: summary?.CanonicalRuntimeManualUnityOptional == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal134CanonicalRuntimeSelectedCandidateQualityDiagnostics(
        Goal134CanonicalRuntimeSelectedCandidateQuality canonicalRuntime,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!canonicalRuntime.UnityPlayerConsumedTranscript)
        {
            return;
        }

        AddIfFalse(canonicalRuntime.GroupPresent,
            "goal134.quality.group_missing",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.PackageValidationPassed,
            "goal134.quality.package_validation",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.CanonicalRuntimePassed,
            "goal134.quality.runtime",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.RuntimeCommandCount >= 6,
            "goal134.quality.runtime_command_count",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.RuntimeEventCount >= 6,
            "goal134.quality.runtime_event_count",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.SaveLoadReplayPassed,
            "goal134.quality.save_load_replay",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.UnityPlayerConsumedTranscript,
            "goal134.quality.unity_transcript",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(!canonicalRuntime.ProjectionOnly,
            "goal134.quality.projection_only",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(canonicalRuntime.SelectedCandidateExecutedByRuntime,
            "goal134.quality.runtime_execution",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysCanonicalRuntimeSelectedCandidate,
            "goal134.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(canonicalRuntime.RelativePaths,
            "goal134.quality.relative_paths",
            "canonical_runtime_selected_candidate_playthrough",
            diagnostics);
    }

    private static bool Goal134AllowedPath(string path) =>
        path.StartsWith(
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal134CanonicalRuntimeSelectedCandidateQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal134CanonicalRuntimeSelectedCandidateQuality canonicalRuntime,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            CanonicalRuntimeSelectedCandidateGroupPresent = canonicalRuntime.GroupPresent,
            CanonicalRuntimeCandidateId = canonicalRuntime.CandidateId,
            CanonicalRuntimePackageValidationPassed =
                canonicalRuntime.PackageValidationPassed,
            CanonicalRuntimePassed = canonicalRuntime.CanonicalRuntimePassed,
            CanonicalRuntimeCommandCount = canonicalRuntime.RuntimeCommandCount,
            CanonicalRuntimeEventCount = canonicalRuntime.RuntimeEventCount,
            CanonicalRuntimeSaveLoadReplayPassed =
                canonicalRuntime.SaveLoadReplayPassed,
            CanonicalRuntimeUnityPlayerConsumedTranscript =
                canonicalRuntime.UnityPlayerConsumedTranscript,
            CanonicalRuntimeProjectionOnly = canonicalRuntime.ProjectionOnly,
            CanonicalRuntimeSelectedCandidateExecutedByRuntime =
                canonicalRuntime.SelectedCandidateExecutedByRuntime,
            CanonicalRuntimeNormalCommand = canonicalRuntime.NormalCommand,
            CanonicalRuntimeReportPath = canonicalRuntime.ReportPath,
            CanonicalRuntimeMatrixResultPath = canonicalRuntime.MatrixResultPath,
            CanonicalRuntimeManualUnityOptional = canonicalRuntime.ManualUnityOptional,
            CanonicalRuntimeGoal134FilesDiscoveredByRelativePaths =
                canonicalRuntime.RelativePaths,
            CanonicalRuntimeWinFormsBindingReal =
                binding.PageBindDisplaysCanonicalRuntimeSelectedCandidate,
            CanonicalRuntimeQualityGatePassed =
                canonicalRuntime.QualityGatePassed
                && binding.PageBindDisplaysCanonicalRuntimeSelectedCandidate,
            Passed = qualityGate.Passed
                     && (!canonicalRuntime.UnityPlayerConsumedTranscript
                         || canonicalRuntime.QualityGatePassed
                         && binding.PageBindDisplaysCanonicalRuntimeSelectedCandidate)
        };

    private sealed record Goal134CanonicalRuntimeSelectedCandidateQuality(
        bool GroupPresent,
        string CandidateId,
        bool PackageValidationPassed,
        bool CanonicalRuntimePassed,
        int RuntimeCommandCount,
        int RuntimeEventCount,
        bool SaveLoadReplayPassed,
        bool UnityPlayerConsumedTranscript,
        bool ProjectionOnly,
        bool SelectedCandidateExecutedByRuntime,
        string NormalCommand,
        string ReportPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool RelativePaths,
        bool QualityGatePassed);
}
