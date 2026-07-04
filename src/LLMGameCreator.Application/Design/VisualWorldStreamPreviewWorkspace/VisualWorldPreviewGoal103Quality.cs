namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal103PlayModeTravelWorkspaceQuality BuildGoal103PlayModeTravelQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_playmode_travel");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_playmode_travel_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal103PlayModeTravelStreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_playmode_unity_script")
            .ToList();
        var editorEntry = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_playmode_editor_window_script");
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal103PlayModeTravelSourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal103PlayModeTravelStreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || Goal103PlayModeTravelScriptPaths().Contains(entry.RelativePath)
                    || entry.RelativePath == editorEntry?.RelativePath));
        return new Goal103PlayModeTravelWorkspaceQuality(
            GroupPresent: group is not null,
            StepCount: summary?.OfflineGeoworldPlayModeTravelStepCount ?? 0,
            ObjectCount: summary?.OfflineGeoworldPlayModeTravelObjectCount ?? 0,
            ActiveChunkCounts: summary?.OfflineGeoworldPlayModeTravelActiveChunkCounts ?? string.Empty,
            BoundaryPrefetchCounts:
                summary?.OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts ?? string.Empty,
            ExpectedVisibleObjectCounts:
                summary?.OfflineGeoworldPlayModeTravelExpectedVisibleObjectCounts ?? string.Empty,
            PayloadFileCount: payloadEntries.Count,
            UnityScriptsReady:
                summary?.OfflineGeoworldPlayModeTravelUnityScriptsReady == true
                && scriptEntries.Count == 3
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            EditorWindowReady:
                summary?.OfflineGeoworldPlayModeTravelEditorWindowReady == true
                && editorEntry?.Status == VisualWorldPreviewArtifactStatus.Passed,
            SimulatedExecutionProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal103.simulated_execution" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal103.negative" && proof.Passed),
            Goal102BClosureRecorded: proofs.Any(proof =>
                proof.ProofId == "goal103.goal102b_closure" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal103.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            BoundaryPrefetchRepresented: proofs.Any(proof =>
                proof.ProofId == "goal103.boundary_prefetch" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal103.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal103PlayModeTravelQualityDiagnostics(
        Goal103PlayModeTravelWorkspaceQuality playModeTravel,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            playModeTravel.GroupPresent,
            "goal103.quality.playmode_group",
            "offline_geoworld_playmode_travel",
            diagnostics);
        AddIfFalse(
            playModeTravel.StepCount >= 4,
            "goal103.quality.step_count",
            "offline_geoworld_playmode_travel",
            diagnostics);
        AddIfFalse(
            playModeTravel.ObjectCount == 18,
            "goal103.quality.object_count",
            "offline_geoworld_playmode_travel",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(playModeTravel.ActiveChunkCounts),
            "goal103.quality.active_chunks",
            "offline_geoworld_playmode_travel",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(playModeTravel.BoundaryPrefetchCounts),
            "goal103.quality.boundary_prefetch_counts",
            "offline_geoworld_playmode_travel",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(playModeTravel.ExpectedVisibleObjectCounts),
            "goal103.quality.visible_counts",
            "offline_geoworld_playmode_travel",
            diagnostics);
        AddIfFalse(playModeTravel.PayloadFileCount == 5, "goal103.quality.payload_count", "payload", diagnostics);
        AddIfFalse(playModeTravel.UnityScriptsReady, "goal103.quality.scripts", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.EditorWindowReady, "goal103.quality.editor", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.SimulatedExecutionProofPassed, "goal103.quality.proof", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.NegativeProofPassed, "goal103.quality.negative", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.Goal102BClosureRecorded, "goal103.quality.goal102b", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.AlphaRuntimeBootstrapUnchanged, "goal103.quality.alpha", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.BoundaryPrefetchRepresented, "goal103.quality.boundary", "proofStatus", diagnostics);
        AddIfFalse(playModeTravel.QualityGatePassed, "goal103.quality.quality_gate", "proofStatus", diagnostics);
        AddIfFalse(
            playModeTravel.RelativePaths,
            "goal103.quality.relative_goal103_paths",
            "offline_geoworld_playmode_travel",
            diagnostics);
    }

    private sealed record Goal103PlayModeTravelWorkspaceQuality(
        bool GroupPresent,
        int StepCount,
        int ObjectCount,
        string ActiveChunkCounts,
        string BoundaryPrefetchCounts,
        string ExpectedVisibleObjectCounts,
        int PayloadFileCount,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool SimulatedExecutionProofPassed,
        bool NegativeProofPassed,
        bool Goal102BClosureRecorded,
        bool AlphaRuntimeBootstrapUnchanged,
        bool BoundaryPrefetchRepresented,
        bool QualityGatePassed,
        bool RelativePaths);
}
