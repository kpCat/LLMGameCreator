namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal106SessionWorkspaceQuality BuildGoal106SessionQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_session_replay");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_session_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal106SessionStreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_session_unity_script")
            .ToList();
        var editorEntry = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_session_editor_window_script");
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal106SessionSourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal106SessionStreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || Goal106SessionScriptPaths().Contains(entry.RelativePath)
                    || entry.RelativePath == editorEntry?.RelativePath));
        return new Goal106SessionWorkspaceQuality(
            GroupPresent: group is not null,
            ReplayStepCount: summary?.OfflineGeoworldSessionReplayStepCount ?? 0,
            StateDeltaCount: summary?.OfflineGeoworldSessionStateDeltaCount ?? 0,
            CheckpointStepIndex: summary?.OfflineGeoworldSessionCheckpointStepIndex ?? 0,
            AcceptanceChecklistStepCount:
                summary?.OfflineGeoworldSessionAcceptanceChecklistStepCount ?? 0,
            FinalStateHash: summary?.OfflineGeoworldSessionFinalStateHash ?? string.Empty,
            PayloadFileCount: payloadEntries.Count,
            UnityScriptsReady:
                summary?.OfflineGeoworldSessionUnityScriptsReady == true
                && scriptEntries.Count == 3
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            EditorWindowReady:
                summary?.OfflineGeoworldSessionEditorWindowReady == true
                && editorEntry?.Status == VisualWorldPreviewArtifactStatus.Passed,
            SimulatedReplayProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal106.simulated_save_load_replay" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal106.negative" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal106.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            CheckpointResumePassed: proofs.Any(proof =>
                proof.ProofId == "goal106.checkpoint_resume" && proof.Passed),
            FinalHashPassed: proofs.Any(proof =>
                proof.ProofId == "goal106.final_hash" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal106.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal106SessionQualityDiagnostics(
        Goal106SessionWorkspaceQuality sessionReplay,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            sessionReplay.GroupPresent,
            "goal106.quality.session_group",
            "offline_geoworld_session_replay",
            diagnostics);
        AddIfFalse(
            sessionReplay.ReplayStepCount >= 6,
            "goal106.quality.replay_step_count",
            "offline_geoworld_session_replay",
            diagnostics);
        AddIfFalse(
            sessionReplay.StateDeltaCount >= 6,
            "goal106.quality.delta_count",
            "offline_geoworld_session_replay",
            diagnostics);
        AddIfFalse(
            sessionReplay.CheckpointStepIndex >= 3,
            "goal106.quality.checkpoint",
            "offline_geoworld_session_replay",
            diagnostics);
        AddIfFalse(
            sessionReplay.AcceptanceChecklistStepCount > 0,
            "goal106.quality.checklist",
            "offline_geoworld_session_replay",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(sessionReplay.FinalStateHash),
            "goal106.quality.final_hash",
            "offline_geoworld_session_replay",
            diagnostics);
        AddIfFalse(sessionReplay.PayloadFileCount == 6, "goal106.quality.payload_count", "payload", diagnostics);
        AddIfFalse(sessionReplay.UnityScriptsReady, "goal106.quality.scripts", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.EditorWindowReady, "goal106.quality.editor", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.SimulatedReplayProofPassed, "goal106.quality.proof", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.NegativeProofPassed, "goal106.quality.negative", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.AlphaRuntimeBootstrapUnchanged, "goal106.quality.alpha", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.CheckpointResumePassed, "goal106.quality.checkpoint_resume", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.FinalHashPassed, "goal106.quality.final_hash_proof", "proofStatus", diagnostics);
        AddIfFalse(sessionReplay.QualityGatePassed, "goal106.quality.quality_gate", "proofStatus", diagnostics);
        AddIfFalse(
            sessionReplay.RelativePaths,
            "goal106.quality.relative_goal106_paths",
            "offline_geoworld_session_replay",
            diagnostics);
    }

    private sealed record Goal106SessionWorkspaceQuality(
        bool GroupPresent,
        int ReplayStepCount,
        int StateDeltaCount,
        int CheckpointStepIndex,
        int AcceptanceChecklistStepCount,
        string FinalStateHash,
        int PayloadFileCount,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool SimulatedReplayProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool CheckpointResumePassed,
        bool FinalHashPassed,
        bool QualityGatePassed,
        bool RelativePaths);
}
