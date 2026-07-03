namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal102UnityEditorPreviewWorkspaceQuality BuildGoal102UnityEditorPreviewQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_unity_editor_preview");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_unity_editor_preview_workspace_summary");
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_unity_editor_preview_script")
            .ToList();
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal102SourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath == summary?.OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath));
        return new Goal102UnityEditorPreviewWorkspaceQuality(
            GroupPresent: group is not null,
            CommandCount: summary?.OfflineGeoworldUnityEditorPreviewCommandCount ?? 0,
            CommandKindCount: summary?.OfflineGeoworldUnityEditorPreviewCommandKindCount ?? 0,
            TravelWindowStepCount: summary?.OfflineGeoworldUnityEditorPreviewTravelWindowStepCount ?? 0,
            ExpectedObjectCount: summary?.OfflineGeoworldUnityEditorPreviewExpectedObjectCount ?? 0,
            EditorWindowScriptPath:
                summary?.OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath ?? string.Empty,
            MenuItemMarker: summary?.OfflineGeoworldUnityEditorPreviewMenuItemMarker ?? string.Empty,
            PayloadPath: summary?.OfflineGeoworldUnityEditorPreviewPayloadPath ?? string.Empty,
            ManualInstructions:
                summary?.OfflineGeoworldUnityEditorPreviewManualInstructions ?? string.Empty,
            ToolInventoryPassed: proofs.Any(proof =>
                proof.ProofId == "goal102.tool_inventory" && proof.Passed),
            EditorWindowScriptReady:
                summary?.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady == true
                && scriptEntries.Count == 1
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            SimulatedActionProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal102.simulated_action" && proof.Passed),
            ClearOperationProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal102.clear_operation" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal102.negative" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal102.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal102.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal102UnityEditorPreviewQualityDiagnostics(
        Goal102UnityEditorPreviewWorkspaceQuality editorPreview,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            editorPreview.GroupPresent,
            "goal102.quality.editor_preview_group",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            editorPreview.CommandCount == 18,
            "goal102.quality.command_count",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            editorPreview.CommandKindCount == 10,
            "goal102.quality.command_kind_count",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            editorPreview.TravelWindowStepCount >= 4,
            "goal102.quality.travel_steps",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            editorPreview.ExpectedObjectCount == 18,
            "goal102.quality.expected_objects",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(editorPreview.EditorWindowScriptPath),
            "goal102.quality.script_path",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(editorPreview.MenuItemMarker),
            "goal102.quality.menu",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(editorPreview.PayloadPath),
            "goal102.quality.payload_path",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(editorPreview.ManualInstructions),
            "goal102.quality.manual",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        AddIfFalse(editorPreview.ToolInventoryPassed, "goal102.quality.inventory", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.EditorWindowScriptReady, "goal102.quality.script", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.SimulatedActionProofPassed, "goal102.quality.simulated", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.ClearOperationProofPassed, "goal102.quality.clear", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.NegativeProofPassed, "goal102.quality.negative", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.AlphaRuntimeBootstrapUnchanged, "goal102.quality.alpha", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.QualityGatePassed, "goal102.quality.quality_gate", "proofStatus", diagnostics);
        AddIfFalse(editorPreview.RelativePaths, "goal102.quality.relative_goal102_paths", "offline_geoworld_unity_editor_preview", diagnostics);
    }

    private sealed record Goal102UnityEditorPreviewWorkspaceQuality(
        bool GroupPresent,
        int CommandCount,
        int CommandKindCount,
        int TravelWindowStepCount,
        int ExpectedObjectCount,
        string EditorWindowScriptPath,
        string MenuItemMarker,
        string PayloadPath,
        string ManualInstructions,
        bool ToolInventoryPassed,
        bool EditorWindowScriptReady,
        bool SimulatedActionProofPassed,
        bool ClearOperationProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        bool RelativePaths);
}
