namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal105InteractionWorkspaceQuality BuildGoal105InteractionQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_interactions");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_interaction_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal105InteractionStreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_interaction_unity_script")
            .ToList();
        var editorEntry = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_interaction_editor_window_script");
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal105InteractionSourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal105InteractionStreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || Goal105InteractionScriptPaths().Contains(entry.RelativePath)
                    || entry.RelativePath == editorEntry?.RelativePath));
        return new Goal105InteractionWorkspaceQuality(
            GroupPresent: group is not null,
            TargetCount: summary?.OfflineGeoworldInteractionTargetCount ?? 0,
            ActionKindCount: summary?.OfflineGeoworldInteractionActionKindCount ?? 0,
            ActionCount: summary?.OfflineGeoworldInteractionActionCount ?? 0,
            ScriptedEventCount: summary?.OfflineGeoworldInteractionScriptedEventCount ?? 0,
            StateDeltaCount: summary?.OfflineGeoworldInteractionStateDeltaCount ?? 0,
            FinalStateHash: summary?.OfflineGeoworldInteractionFinalStateHash ?? string.Empty,
            PayloadFileCount: payloadEntries.Count,
            StateHashChainPassed: proofs.Any(proof =>
                proof.ProofId == "goal105.state_hash_chain" && proof.Passed),
            UnityScriptsReady:
                summary?.OfflineGeoworldInteractionUnityScriptsReady == true
                && scriptEntries.Count == 3
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            EditorWindowReady:
                summary?.OfflineGeoworldInteractionEditorWindowReady == true
                && editorEntry?.Status == VisualWorldPreviewArtifactStatus.Passed,
            UnitySafetyScanPassed: proofs.Any(proof =>
                proof.ProofId == "goal105.unity_script_inventory" && proof.Passed),
            SimulatedSessionProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal105.simulated_session" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal105.negative" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal105.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal105.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal105InteractionQualityDiagnostics(
        Goal105InteractionWorkspaceQuality interactions,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            interactions.GroupPresent,
            "goal105.quality.interaction_group",
            "offline_geoworld_interactions",
            diagnostics);
        AddIfFalse(
            interactions.TargetCount >= 8,
            "goal105.quality.target_count",
            "offline_geoworld_interactions",
            diagnostics);
        AddIfFalse(
            interactions.ActionKindCount >= 5,
            "goal105.quality.action_kind_count",
            "offline_geoworld_interactions",
            diagnostics);
        AddIfFalse(
            interactions.ScriptedEventCount >= 6,
            "goal105.quality.scripted_event_count",
            "offline_geoworld_interactions",
            diagnostics);
        AddIfFalse(
            interactions.StateDeltaCount >= 6,
            "goal105.quality.state_delta_count",
            "offline_geoworld_interactions",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(interactions.FinalStateHash),
            "goal105.quality.final_hash",
            "offline_geoworld_interactions",
            diagnostics);
        AddIfFalse(interactions.PayloadFileCount == 6, "goal105.quality.payload_count", "payload", diagnostics);
        AddIfFalse(interactions.StateHashChainPassed, "goal105.quality.hash_chain", "proofStatus", diagnostics);
        AddIfFalse(interactions.UnityScriptsReady, "goal105.quality.scripts", "proofStatus", diagnostics);
        AddIfFalse(interactions.EditorWindowReady, "goal105.quality.editor", "proofStatus", diagnostics);
        AddIfFalse(interactions.UnitySafetyScanPassed, "goal105.quality.safety", "proofStatus", diagnostics);
        AddIfFalse(interactions.SimulatedSessionProofPassed, "goal105.quality.proof", "proofStatus", diagnostics);
        AddIfFalse(interactions.NegativeProofPassed, "goal105.quality.negative", "proofStatus", diagnostics);
        AddIfFalse(interactions.AlphaRuntimeBootstrapUnchanged, "goal105.quality.alpha", "proofStatus", diagnostics);
        AddIfFalse(interactions.QualityGatePassed, "goal105.quality.quality_gate", "proofStatus", diagnostics);
        AddIfFalse(
            interactions.RelativePaths,
            "goal105.quality.relative_goal105_paths",
            "offline_geoworld_interactions",
            diagnostics);
    }

    private sealed record Goal105InteractionWorkspaceQuality(
        bool GroupPresent,
        int TargetCount,
        int ActionKindCount,
        int ActionCount,
        int ScriptedEventCount,
        int StateDeltaCount,
        string FinalStateHash,
        int PayloadFileCount,
        bool StateHashChainPassed,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool UnitySafetyScanPassed,
        bool SimulatedSessionProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        bool RelativePaths);
}
