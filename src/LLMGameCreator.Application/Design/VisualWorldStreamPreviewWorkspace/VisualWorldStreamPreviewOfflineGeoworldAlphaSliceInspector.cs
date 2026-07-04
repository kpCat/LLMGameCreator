using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal108AlphaSliceSourceGoalId =
        "goal_108_offline_geoworld_alpha_slice_orchestrator";
    private const string Goal108AlphaSliceSourceRoot =
        ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator";
    private const string Goal108AlphaSliceStreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaSliceGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaSliceSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                [
                    (OfflineGeoworldAlphaSliceVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_alpha_slice_report"),
                    (OfflineGeoworldAlphaSliceVocabulary.ManifestFileName,
                        "offline_geoworld_alpha_slice_manifest"),
                    (OfflineGeoworldAlphaSliceVocabulary.ComponentsFileName,
                        "offline_geoworld_alpha_slice_components"),
                    (OfflineGeoworldAlphaSliceVocabulary.AcceptanceRunbookFileName,
                        "offline_geoworld_alpha_slice_acceptance_runbook"),
                    (OfflineGeoworldAlphaSliceVocabulary.ReadinessMatrixFileName,
                        "offline_geoworld_alpha_slice_readiness_matrix"),
                    (OfflineGeoworldAlphaSliceVocabulary.ReadmeFileName,
                        "offline_geoworld_alpha_slice_readme"),
                    (OfflineGeoworldAlphaSliceVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_alpha_slice_unity_script_inventory"),
                    (OfflineGeoworldAlphaSliceVocabulary.EditorWindowInventoryFileName,
                        "offline_geoworld_alpha_slice_editor_window_inventory"),
                    (OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName,
                        "offline_geoworld_alpha_slice_simulated_proof"),
                    (OfflineGeoworldAlphaSliceVocabulary.NegativeProofFileName,
                        "offline_geoworld_alpha_slice_negative_proof"),
                    (OfflineGeoworldAlphaSliceVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_alpha_slice_workspace_binding_inventory"),
                    (OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName,
                        "offline_geoworld_alpha_slice_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaSliceSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaSliceVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal108AlphaSliceStreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldAlphaSliceSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal108AlphaSliceSourceGoalId + ".payload."
                         + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_alpha_slice_streamingassets_payload",
                    SourceGoalId = Goal108AlphaSliceSourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists
                        ? "mirrored Goal108 Alpha Slice payload exists"
                        : "mirrored Goal108 Alpha Slice payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; alphaSlice=toolingOnly"
                },
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaSliceSummary(
            Goal108ScriptEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceVocabulary.UnityCoordinatorScriptPath,
                "offline_geoworld_alpha_slice_coordinator_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaSliceSummary(
            Goal108ScriptEntry(
                projectRoot,
                OfflineGeoworldAlphaSliceVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_alpha_slice_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldAlphaSliceSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal108AlphaSliceSourceGoalId + ".summary",
                RelativePath = Goal108AlphaSliceSourceRoot + "/"
                    + OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_alpha_slice_workspace_summary",
                SourceGoalId = Goal108AlphaSliceSourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal108AlphaSliceSourceRoot + "/"
                    + OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "components=" + summary.ReadyComponentCount
                    + "/" + summary.ComponentCount
                    + "; objectives=" + summary.CompletedObjectiveCount
                    + "/" + summary.ObjectiveCount,
                SafeRatingMetadataSummary = "finalStatus=" + summary.FinalStatus
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_slice",
            "Goal 108 Offline Geoworld Alpha Slice",
            Goal108AlphaSliceSourceGoalId,
            Goal108AlphaSliceSourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldAlphaSliceSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldAlphaSliceWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaSliceComponentCount = summary.ComponentCount,
            OfflineGeoworldAlphaSliceReadyComponentCount = summary.ReadyComponentCount,
            OfflineGeoworldAlphaSliceObjectiveCount = summary.ObjectiveCount,
            OfflineGeoworldAlphaSliceCompletedObjectiveCount = summary.CompletedObjectiveCount,
            OfflineGeoworldAlphaSliceFinalStatus = summary.FinalStatus,
            OfflineGeoworldAlphaSliceUnityToolReady = summary.UnityToolReady,
            OfflineGeoworldAlphaSliceAcceptanceRunbookReady = summary.AcceptanceRunbookReady,
            OfflineGeoworldAlphaSliceFinalProofPassed = summary.FinalProofPassed,
            OfflineGeoworldAlphaSliceNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaSliceQualityGatePassed = summary.QualityGatePassed,
            OfflineGeoworldAlphaSliceRemainingWarnings =
                "manualGateRequired; toolingOnly; notFinalRuntime",
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaSliceWorkspaceSummary LoadOfflineGeoworldAlphaSliceSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.ManifestFileName, diagnostics);
        using var runbook = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.AcceptanceRunbookFileName, diagnostics);
        using var scripts = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.UnityScriptInventoryFileName, diagnostics);
        using var editor = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.EditorWindowInventoryFileName, diagnostics);
        using var proof = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName, diagnostics);
        using var negative = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.NegativeProofFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, Goal108AlphaSliceSourceRoot + "/"
            + OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName, diagnostics);

        var componentCount = manifest is null ? 0 : ReadGoal108Int(manifest.RootElement, "componentCount");
        var readyComponentCount = manifest is null ? 0 : ReadGoal108Int(manifest.RootElement, "readyComponentCount");
        var objectiveCount = manifest is null ? 0 : ReadGoal108Int(manifest.RootElement, "objectiveCount");
        var completedObjectiveCount = manifest is null ? 0 : ReadGoal108Int(manifest.RootElement, "completedObjectiveCount");
        var finalStatus = manifest is null ? string.Empty : ReadGoal108String(manifest.RootElement, "finalStatus");
        var alphaUnchanged = manifest is not null && TryGetBool(manifest.RootElement, "alphaRuntimeBootstrapUnchanged");
        var unityToolReady = scripts is not null && TryGetBool(scripts.RootElement, "passed")
                             && editor is not null && TryGetBool(editor.RootElement, "passed");
        var runbookReady = runbook is not null && ReadGoal108Int(runbook.RootElement, "stepCount") >= 8;
        var proofPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = IsSafeRelativePath(Goal108AlphaSliceSourceRoot)
                            && IsSafeRelativePath(Goal108AlphaSliceStreamingAssetsRoot)
                            && IsSafeRelativePath(OfflineGeoworldAlphaSliceVocabulary.UnityCoordinatorScriptPath)
                            && IsSafeRelativePath(OfflineGeoworldAlphaSliceVocabulary.UnityEditorWindowScriptPath);
        var passed = componentCount == 7
                     && readyComponentCount == componentCount
                     && objectiveCount >= 5
                     && completedObjectiveCount == objectiveCount
                     && finalStatus == "completed"
                     && unityToolReady
                     && runbookReady
                     && proofPassed
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(passed, "goal108.workspace.summary_failed",
            "offline_geoworld_alpha_slice", diagnostics);
        return new OfflineGeoworldAlphaSliceWorkspaceSummary(
            passed,
            componentCount,
            readyComponentCount,
            objectiveCount,
            completedObjectiveCount,
            finalStatus,
            unityToolReady,
            runbookReady,
            proofPassed,
            negativePassed,
            alphaUnchanged,
            qualityPassed,
            relativePaths);
    }

    private static VisualWorldPreviewArtifactEntry Goal108ScriptEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal108AlphaSliceSourceGoalId + ".script." + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = Goal108AlphaSliceSourceGoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Unity Goal108 Alpha Slice script exists" : "Unity Goal108 Alpha Slice script missing",
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };
    }

    private static int ReadGoal108Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal108String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldAlphaSliceWorkspaceSummary(
        bool Passed,
        int ComponentCount,
        int ReadyComponentCount,
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
