namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal104InteractiveTravelWorkspaceQuality BuildGoal104InteractiveTravelQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_interactive_travel");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_interactive_travel_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal104InteractiveTravelStreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_interactive_unity_script")
            .ToList();
        var editorEntry = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_interactive_editor_window_script");
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal104InteractiveTravelSourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal104InteractiveTravelStreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || Goal104InteractiveTravelScriptPaths().Contains(entry.RelativePath)
                    || entry.RelativePath == editorEntry?.RelativePath));
        return new Goal104InteractiveTravelWorkspaceQuality(
            GroupPresent: group is not null,
            MovementSampleCount: summary?.OfflineGeoworldInteractiveTravelMovementSampleCount ?? 0,
            BoundaryCrossingCount: summary?.OfflineGeoworldInteractiveTravelBoundaryCrossingCount ?? 0,
            ObjectCount: summary?.OfflineGeoworldInteractiveTravelObjectCount ?? 0,
            ActiveChunkCounts: summary?.OfflineGeoworldInteractiveTravelActiveChunkCounts ?? string.Empty,
            BoundaryPrefetchCounts:
                summary?.OfflineGeoworldInteractiveTravelBoundaryPrefetchCounts ?? string.Empty,
            ExpectedVisibleObjectCounts:
                summary?.OfflineGeoworldInteractiveTravelExpectedVisibleObjectCounts ?? string.Empty,
            PayloadFileCount: payloadEntries.Count,
            UnityScriptsReady:
                summary?.OfflineGeoworldInteractiveTravelUnityScriptsReady == true
                && scriptEntries.Count == 3
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            EditorWindowReady:
                summary?.OfflineGeoworldInteractiveTravelEditorWindowReady == true
                && editorEntry?.Status == VisualWorldPreviewArtifactStatus.Passed,
            SimulatedExecutionProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal104.simulated_execution" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal104.negative" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal104.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            BoundaryPrefetchRepresented: proofs.Any(proof =>
                proof.ProofId == "goal104.boundary_crossings" && proof.Passed),
            PrefetchPlanPassed: proofs.Any(proof =>
                proof.ProofId == "goal104.prefetch_plan" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal104.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal104InteractiveTravelQualityDiagnostics(
        Goal104InteractiveTravelWorkspaceQuality interactiveTravel,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            interactiveTravel.GroupPresent,
            "goal104.quality.interactive_group",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(
            interactiveTravel.MovementSampleCount >= 6,
            "goal104.quality.sample_count",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(
            interactiveTravel.BoundaryCrossingCount >= 2,
            "goal104.quality.crossing_count",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(
            interactiveTravel.ObjectCount == 18,
            "goal104.quality.object_count",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(interactiveTravel.ActiveChunkCounts),
            "goal104.quality.active_chunks",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(interactiveTravel.BoundaryPrefetchCounts),
            "goal104.quality.boundary_prefetch_counts",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(interactiveTravel.ExpectedVisibleObjectCounts),
            "goal104.quality.visible_counts",
            "offline_geoworld_interactive_travel",
            diagnostics);
        AddIfFalse(interactiveTravel.PayloadFileCount == 5, "goal104.quality.payload_count", "payload", diagnostics);
        AddIfFalse(interactiveTravel.UnityScriptsReady, "goal104.quality.scripts", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.EditorWindowReady, "goal104.quality.editor", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.SimulatedExecutionProofPassed, "goal104.quality.proof", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.NegativeProofPassed, "goal104.quality.negative", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.AlphaRuntimeBootstrapUnchanged, "goal104.quality.alpha", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.BoundaryPrefetchRepresented, "goal104.quality.boundary", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.PrefetchPlanPassed, "goal104.quality.prefetch", "proofStatus", diagnostics);
        AddIfFalse(interactiveTravel.QualityGatePassed, "goal104.quality.quality_gate", "proofStatus", diagnostics);
        AddIfFalse(
            interactiveTravel.RelativePaths,
            "goal104.quality.relative_goal104_paths",
            "offline_geoworld_interactive_travel",
            diagnostics);
    }

    private sealed record Goal104InteractiveTravelWorkspaceQuality(
        bool GroupPresent,
        int MovementSampleCount,
        int BoundaryCrossingCount,
        int ObjectCount,
        string ActiveChunkCounts,
        string BoundaryPrefetchCounts,
        string ExpectedVisibleObjectCounts,
        int PayloadFileCount,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool SimulatedExecutionProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool BoundaryPrefetchRepresented,
        bool PrefetchPlanPassed,
        bool QualityGatePassed,
        bool RelativePaths);
}
