using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal136CanonicalRuntimePlayerCommandLoopQuality
        BuildGoal136CanonicalRuntimePlayerCommandLoopQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "canonical_runtime_player_command_loop");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "canonical_runtime_player_command_loop_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal136.player_command_loop.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal136AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && !string.IsNullOrWhiteSpace(summary.CanonicalRuntimePlayerCommandLoopCandidateId)
            && summary.CanonicalRuntimePlayerCommandLoopPassed
            && summary.CanonicalRuntimePlayerCommandCount >= 10
            && summary.CanonicalRuntimePlayerSnapshotCount
                == summary.CanonicalRuntimePlayerCommandCount
            && summary.CanonicalRuntimePlayerCommandLoopRuntimeEventCount >= 10
            && summary.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent
            && summary.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots
            && !summary.CanonicalRuntimePlayerCommandLoopProjectionOnly
            && !summary.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth
            && summary.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors
            && summary.CanonicalRuntimePlayerCommandLoopManualUnityOptional
            && !summary.CanonicalRuntimePlayerCommandLoopAccepted
            && relativePaths
            && proofPassed;

        return new Goal136CanonicalRuntimePlayerCommandLoopQuality(
            GroupPresent: group is not null,
            CandidateId: summary?.CanonicalRuntimePlayerCommandLoopCandidateId ?? string.Empty,
            PlayerCommandLoopPassed:
                summary?.CanonicalRuntimePlayerCommandLoopPassed == true,
            PlayerCommandCount:
                summary?.CanonicalRuntimePlayerCommandCount ?? 0,
            SnapshotCount:
                summary?.CanonicalRuntimePlayerSnapshotCount ?? 0,
            RuntimeEventCount:
                summary?.CanonicalRuntimePlayerCommandLoopRuntimeEventCount ?? 0,
            AllRequiredCategoriesPresent:
                summary?.CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent == true,
            UnityConsumedSnapshots:
                summary?.CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots == true,
            ProjectionOnly:
                summary?.CanonicalRuntimePlayerCommandLoopProjectionOnly == true,
            UnityGameplayTruth:
                summary?.CanonicalRuntimePlayerCommandLoopUnityGameplayTruth == true,
            NoUnclassifiedErrorDiagnostics:
                summary?.CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors == true,
            NormalCommand: summary?.CanonicalRuntimePlayerCommandLoopNormalCommand ?? string.Empty,
            ReportPath: summary?.CanonicalRuntimePlayerCommandLoopReportPath ?? string.Empty,
            MatrixResultPath:
                summary?.CanonicalRuntimePlayerCommandLoopMatrixResultPath ?? string.Empty,
            ManualUnityOptional:
                summary?.CanonicalRuntimePlayerCommandLoopManualUnityOptional == true,
            Accepted:
                summary?.CanonicalRuntimePlayerCommandLoopAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal136CanonicalRuntimePlayerCommandLoopQualityDiagnostics(
        Goal136CanonicalRuntimePlayerCommandLoopQuality commandLoop,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!commandLoop.GroupPresent)
        {
            return;
        }

        AddIfFalse(commandLoop.PlayerCommandLoopPassed,
            "goal136.quality.player_command_loop",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(commandLoop.PlayerCommandCount >= 10,
            "goal136.quality.command_count",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(commandLoop.SnapshotCount == commandLoop.PlayerCommandCount,
            "goal136.quality.snapshot_count",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(commandLoop.RuntimeEventCount >= 10,
            "goal136.quality.runtime_event_count",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(commandLoop.AllRequiredCategoriesPresent,
            "goal136.quality.required_categories",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(commandLoop.UnityConsumedSnapshots,
            "goal136.quality.unity_consumed_snapshots",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(!commandLoop.ProjectionOnly,
            "goal136.quality.projection_only",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(!commandLoop.UnityGameplayTruth,
            "goal136.quality.unity_gameplay_truth",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(commandLoop.NoUnclassifiedErrorDiagnostics,
            "goal136.quality.no_unclassified_errors",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(!commandLoop.Accepted,
            "goal136.quality.accepted_must_stay_false",
            "canonical_runtime_player_command_loop",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysCanonicalRuntimePlayerCommandLoop,
            "goal136.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(commandLoop.RelativePaths,
            "goal136.quality.relative_paths",
            "canonical_runtime_player_command_loop",
            diagnostics);
    }

    private static bool Goal136AllowedPath(string path) =>
        path.StartsWith(
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            CanonicalRuntimePlayerCommandLoopVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal136CanonicalRuntimePlayerCommandLoopQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal136CanonicalRuntimePlayerCommandLoopQuality commandLoop,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            CanonicalRuntimePlayerCommandLoopGroupPresent = commandLoop.GroupPresent,
            CanonicalRuntimePlayerCommandLoopCandidateId = commandLoop.CandidateId,
            CanonicalRuntimePlayerCommandLoopPassed = commandLoop.PlayerCommandLoopPassed,
            CanonicalRuntimePlayerCommandCount = commandLoop.PlayerCommandCount,
            CanonicalRuntimePlayerSnapshotCount = commandLoop.SnapshotCount,
            CanonicalRuntimePlayerCommandLoopRuntimeEventCount =
                commandLoop.RuntimeEventCount,
            CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent =
                commandLoop.AllRequiredCategoriesPresent,
            CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots =
                commandLoop.UnityConsumedSnapshots,
            CanonicalRuntimePlayerCommandLoopProjectionOnly =
                commandLoop.ProjectionOnly,
            CanonicalRuntimePlayerCommandLoopUnityGameplayTruth =
                commandLoop.UnityGameplayTruth,
            CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors =
                commandLoop.NoUnclassifiedErrorDiagnostics,
            CanonicalRuntimePlayerCommandLoopNormalCommand =
                commandLoop.NormalCommand,
            CanonicalRuntimePlayerCommandLoopReportPath =
                commandLoop.ReportPath,
            CanonicalRuntimePlayerCommandLoopMatrixResultPath =
                commandLoop.MatrixResultPath,
            CanonicalRuntimePlayerCommandLoopManualUnityOptional =
                commandLoop.ManualUnityOptional,
            CanonicalRuntimePlayerCommandLoopAccepted =
                commandLoop.Accepted,
            CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths =
                commandLoop.RelativePaths,
            CanonicalRuntimePlayerCommandLoopWinFormsBindingReal =
                binding.PageBindDisplaysCanonicalRuntimePlayerCommandLoop,
            CanonicalRuntimePlayerCommandLoopQualityGatePassed =
                commandLoop.QualityGatePassed
                && binding.PageBindDisplaysCanonicalRuntimePlayerCommandLoop,
            Passed = qualityGate.Passed
                     && (!commandLoop.GroupPresent
                         || commandLoop.QualityGatePassed
                         && binding.PageBindDisplaysCanonicalRuntimePlayerCommandLoop)
        };

    private sealed record Goal136CanonicalRuntimePlayerCommandLoopQuality(
        bool GroupPresent,
        string CandidateId,
        bool PlayerCommandLoopPassed,
        int PlayerCommandCount,
        int SnapshotCount,
        int RuntimeEventCount,
        bool AllRequiredCategoriesPresent,
        bool UnityConsumedSnapshots,
        bool ProjectionOnly,
        bool UnityGameplayTruth,
        bool NoUnclassifiedErrorDiagnostics,
        string NormalCommand,
        string ReportPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
