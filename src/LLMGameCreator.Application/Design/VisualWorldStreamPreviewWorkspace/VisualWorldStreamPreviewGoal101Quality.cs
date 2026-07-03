namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal101UnityPreviewWorkspaceQuality BuildGoal101UnityPreviewQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_unity_preview");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_unity_preview_workspace_summary");
        var payloadEntries = entries
            .Where(entry => entry.RelativePath.StartsWith(
                Goal101StreamingAssetsRoot + "/",
                StringComparison.Ordinal))
            .ToList();
        var scriptEntries = entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_unity_preview_script")
            .ToList();
        var relativePaths = entries.Count > 0
            && entries.All(entry =>
                IsSafeRelativePath(entry.RelativePath)
                && (entry.RelativePath.StartsWith(Goal101SourceRoot + "/", StringComparison.Ordinal)
                    || entry.RelativePath.StartsWith(Goal101StreamingAssetsRoot + "/", StringComparison.Ordinal)
                    || UnityScriptPaths().Contains(entry.RelativePath)));
        return new Goal101UnityPreviewWorkspaceQuality(
            GroupPresent: group is not null,
            CommandCount: summary?.OfflineGeoworldUnityPreviewCommandCount ?? 0,
            CommandKindCount: summary?.OfflineGeoworldUnityPreviewCommandKindCount ?? 0,
            TravelWindowStepCount: summary?.OfflineGeoworldUnityPreviewTravelWindowStepCount ?? 0,
            PayloadFileCount: payloadEntries.Count,
            CommandKindCoverageSummary:
                summary?.OfflineGeoworldUnityPreviewKindCoverageSummary ?? string.Empty,
            UnityScriptsReady:
                summary?.OfflineGeoworldUnityPreviewUnityScriptsReady == true
                && scriptEntries.Count == 3
                && scriptEntries.All(entry => entry.Status == VisualWorldPreviewArtifactStatus.Passed),
            SimulatedCommandProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal101.simulated_command" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal101.negative" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged: proofs.Any(proof =>
                proof.ProofId == "goal101.alpha_runtime_bootstrap_unchanged" && proof.Passed),
            AllCommandKindsMapped: proofs.Any(proof =>
                proof.ProofId == "goal101.all_command_kinds_mapped" && proof.Passed),
            TravelWindowDemoBuilt: proofs.Any(proof =>
                proof.ProofId == "goal101.travel_window_demo" && proof.Passed),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal101.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal101UnityPreviewQualityDiagnostics(
        Goal101UnityPreviewWorkspaceQuality unityPreview,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(
            unityPreview.GroupPresent,
            "goal101.quality.unity_preview_group",
            "offline_geoworld_unity_preview",
            diagnostics);
        AddIfFalse(
            unityPreview.CommandCount == 18,
            "goal101.quality.command_count",
            "offline_geoworld_unity_preview",
            diagnostics);
        AddIfFalse(
            unityPreview.CommandKindCount == 10,
            "goal101.quality.command_kind_count",
            "offline_geoworld_unity_preview",
            diagnostics);
        AddIfFalse(
            unityPreview.TravelWindowStepCount >= 4,
            "goal101.quality.travel_steps",
            "offline_geoworld_unity_preview",
            diagnostics);
        AddIfFalse(
            unityPreview.PayloadFileCount == 5,
            "goal101.quality.payload_file_count",
            "offline_geoworld_unity_preview",
            diagnostics);
        AddIfFalse(
            !string.IsNullOrWhiteSpace(unityPreview.CommandKindCoverageSummary),
            "goal101.quality.command_kind_coverage",
            "offline_geoworld_unity_preview",
            diagnostics);
        AddIfFalse(unityPreview.UnityScriptsReady, "goal101.quality.scripts", "proofStatus", diagnostics);
        AddIfFalse(
            unityPreview.SimulatedCommandProofPassed,
            "goal101.quality.simulated_command",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            unityPreview.NegativeProofPassed,
            "goal101.quality.negative_proof",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            unityPreview.AlphaRuntimeBootstrapUnchanged,
            "goal101.quality.alpha_bootstrap",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            unityPreview.AllCommandKindsMapped,
            "goal101.quality.all_command_kinds",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            unityPreview.TravelWindowDemoBuilt,
            "goal101.quality.travel_demo",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            unityPreview.QualityGatePassed,
            "goal101.quality.quality_gate",
            "proofStatus",
            diagnostics);
        AddIfFalse(
            unityPreview.RelativePaths,
            "goal101.quality.relative_goal101_paths",
            "offline_geoworld_unity_preview",
            diagnostics);
    }

    private sealed record Goal101UnityPreviewWorkspaceQuality(
        bool GroupPresent,
        int CommandCount,
        int CommandKindCount,
        int TravelWindowStepCount,
        int PayloadFileCount,
        string CommandKindCoverageSummary,
        bool UnityScriptsReady,
        bool SimulatedCommandProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool AllCommandKindsMapped,
        bool TravelWindowDemoBuilt,
        bool QualityGatePassed,
        bool RelativePaths);
}
