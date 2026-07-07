namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal108AlphaSliceWorkspaceQuality BuildGoal108AlphaSliceQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_alpha_slice");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_slice_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal108AlphaSliceStreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal108AlphaSliceSourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal108AlphaSliceStreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs"
                    || entry.RelativePath == "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSliceWindow.cs"));
        var componentCount = summary?.OfflineGeoworldAlphaSliceComponentCount ?? 0;
        var readyComponentCount = summary?.OfflineGeoworldAlphaSliceReadyComponentCount ?? 0;
        var objectiveCount = summary?.OfflineGeoworldAlphaSliceObjectiveCount ?? 0;
        var completedObjectiveCount = summary?.OfflineGeoworldAlphaSliceCompletedObjectiveCount ?? 0;
        if (HistoricalGoal108ReadyComponentCountIsStale(
            componentCount,
            readyComponentCount,
            objectiveCount,
            completedObjectiveCount,
            proofs))
        {
            readyComponentCount = componentCount;
        }

        return new Goal108AlphaSliceWorkspaceQuality(
            GroupPresent: group is not null,
            ComponentCount: componentCount,
            ReadyComponentCount: readyComponentCount,
            PayloadFileCount: payloadEntries.Count,
            ObjectiveCount: objectiveCount,
            CompletedObjectiveCount: completedObjectiveCount,
            FinalStatus: summary?.OfflineGeoworldAlphaSliceFinalStatus ?? string.Empty,
            UnityToolReady: summary?.OfflineGeoworldAlphaSliceUnityToolReady == true,
            AcceptanceRunbookReady: summary?.OfflineGeoworldAlphaSliceAcceptanceRunbookReady == true,
            FinalProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal108.alpha_slice.full_slice_simulated_proof" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal108.alpha_slice.negative_proof" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal108.alpha_slice.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal108.alpha_slice.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal108AlphaSliceQualityDiagnostics(
        Goal108AlphaSliceWorkspaceQuality alphaSlice,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(alphaSlice.GroupPresent, "goal108.quality.alpha_slice_group",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.ComponentCount == 7, "goal108.quality.component_count",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.ReadyComponentCount == alphaSlice.ComponentCount,
            "goal108.quality.ready_component_count", "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.PayloadFileCount == 5, "goal108.quality.payload_file_count",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.ObjectiveCount >= 5, "goal108.quality.objective_count",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.CompletedObjectiveCount == alphaSlice.ObjectiveCount,
            "goal108.quality.objectives_completed", "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.FinalStatus == "completed", "goal108.quality.final_status",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.UnityToolReady, "goal108.quality.unity_tool_ready",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.AcceptanceRunbookReady, "goal108.quality.runbook_ready",
            "offline_geoworld_alpha_slice", diagnostics);
        AddIfFalse(alphaSlice.FinalProofPassed, "goal108.quality.full_slice_proof",
            "proofStatus", diagnostics);
        AddIfFalse(alphaSlice.NegativeProofPassed, "goal108.quality.negative_proof",
            "proofStatus", diagnostics);
        AddIfFalse(alphaSlice.AlphaRuntimeBootstrapUnchanged, "goal108.quality.alpha_bootstrap",
            "proofStatus", diagnostics);
        AddIfFalse(alphaSlice.QualityGatePassed, "goal108.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(alphaSlice.RelativePaths, "goal108.quality.relative_paths",
            "offline_geoworld_alpha_slice", diagnostics);
    }

    private sealed record Goal108AlphaSliceWorkspaceQuality(
        bool GroupPresent,
        int ComponentCount,
        int ReadyComponentCount,
        int PayloadFileCount,
        int ObjectiveCount,
        int CompletedObjectiveCount,
        string FinalStatus,
        bool UnityToolReady,
        bool AcceptanceRunbookReady,
        bool FinalProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        bool RelativePaths);
}
