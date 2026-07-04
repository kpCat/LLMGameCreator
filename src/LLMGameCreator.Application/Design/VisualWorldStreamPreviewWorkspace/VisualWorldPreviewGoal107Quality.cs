namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal107ObjectiveWorkspaceQuality BuildGoal107ObjectiveQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_objective_acceptance");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_objective_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal107ObjectiveStreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_objective_unity_script")
            .ToList();
        var editorEntry = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_objective_editor_window_script");
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal107ObjectiveSourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal107ObjectiveStreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || Goal107ObjectiveScriptPaths().Contains(entry.RelativePath)
                    || entry.RelativePath == editorEntry?.RelativePath));
        return new Goal107ObjectiveWorkspaceQuality(
            GroupPresent: group is not null,
            ObjectiveCount: summary?.OfflineGeoworldObjectiveCount ?? 0,
            CompletedObjectiveCount: summary?.OfflineGeoworldObjectiveCompletedCount ?? 0,
            PayloadFileCount: payloadEntries.Count,
            ReplayStepCount: summary?.OfflineGeoworldObjectiveReplayStepCount ?? 0,
            StateDeltaCount: summary?.OfflineGeoworldObjectiveStateDeltaCount ?? 0,
            CheckpointStepIndex: summary?.OfflineGeoworldObjectiveCheckpointStepIndex ?? 0,
            FinalStatus: summary?.OfflineGeoworldObjectiveFinalStatus ?? string.Empty,
            FinalStateHash: summary?.OfflineGeoworldObjectiveFinalStateHash ?? string.Empty,
            UnityScriptsReady:
                summary?.OfflineGeoworldObjectiveUnityScriptsReady == true
                && scriptEntries.Count == 3
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            EditorWindowReady:
                summary?.OfflineGeoworldObjectiveEditorWindowReady == true
                && editorEntry?.Status == VisualWorldPreviewArtifactStatus.Passed,
            ReplayAcceptanceProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal107.replay_acceptance" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal107.negative" && proof.Passed),
            AlphaQualityConsolidationPassed: proofs.Any(proof =>
                proof.ProofId == "goal107.alpha_quality_consolidation" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal107.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            CheckpointResumePassed: proofs.Any(proof =>
                proof.ProofId == "goal107.checkpoint_resume" && proof.Passed),
            CompletionTransitionsPassed: proofs.Any(proof =>
                proof.ProofId == "goal107.completion_transitions" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal107.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal107ObjectiveQualityDiagnostics(
        Goal107ObjectiveWorkspaceQuality objectiveAcceptance,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(objectiveAcceptance.GroupPresent, "goal107.quality.objective_group",
            "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.ObjectiveCount >= 6, "goal107.quality.objective_count",
            "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.CompletedObjectiveCount == objectiveAcceptance.ObjectiveCount,
            "goal107.quality.completed_count", "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.PayloadFileCount == 6, "goal107.quality.payload_count",
            "payload", diagnostics);
        AddIfFalse(objectiveAcceptance.ReplayStepCount >= 6, "goal107.quality.replay_step_count",
            "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.StateDeltaCount >= 6, "goal107.quality.delta_count",
            "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.CheckpointStepIndex >= 3, "goal107.quality.checkpoint",
            "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.FinalStatus == "completed", "goal107.quality.final_status",
            "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(objectiveAcceptance.FinalStateHash),
            "goal107.quality.final_hash", "offline_geoworld_objective_acceptance", diagnostics);
        AddIfFalse(objectiveAcceptance.UnityScriptsReady, "goal107.quality.scripts", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.EditorWindowReady, "goal107.quality.editor", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.ReplayAcceptanceProofPassed, "goal107.quality.proof", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.NegativeProofPassed, "goal107.quality.negative", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.AlphaQualityConsolidationPassed, "goal107.quality.alpha_quality", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.AlphaRuntimeBootstrapUnchanged, "goal107.quality.alpha", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.CheckpointResumePassed, "goal107.quality.checkpoint_resume", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.CompletionTransitionsPassed, "goal107.quality.completion", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.QualityGatePassed, "goal107.quality.quality_gate", "proofStatus", diagnostics);
        AddIfFalse(objectiveAcceptance.RelativePaths, "goal107.quality.relative_goal107_paths",
            "offline_geoworld_objective_acceptance", diagnostics);
    }

    private sealed record Goal107ObjectiveWorkspaceQuality(
        bool GroupPresent,
        int ObjectiveCount,
        int CompletedObjectiveCount,
        int PayloadFileCount,
        int ReplayStepCount,
        int StateDeltaCount,
        int CheckpointStepIndex,
        string FinalStatus,
        string FinalStateHash,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool ReplayAcceptanceProofPassed,
        bool NegativeProofPassed,
        bool AlphaQualityConsolidationPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool CheckpointResumePassed,
        bool CompletionTransitionsPassed,
        bool QualityGatePassed,
        bool RelativePaths);
}
