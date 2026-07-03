using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal102SourceGoalId =
        "goal_102_offline_geoworld_unity_editor_preview_tool";
    private const string Goal102SourceRoot =
        ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldUnityEditorPreviewGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldUnityEditorPreviewSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal102SourceRoot,
                Goal102SourceGoalId,
                [
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_unity_editor_preview_report"),
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName,
                        "offline_geoworld_unity_editor_preview_tool_inventory"),
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName,
                        "offline_geoworld_unity_editor_preview_simulated_action_proof"),
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.NegativeProofFileName,
                        "offline_geoworld_unity_editor_preview_negative_proof"),
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_unity_editor_preview_workspace_binding_inventory"),
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.SourceLineageFileName,
                        "offline_geoworld_unity_editor_preview_source_lineage"),
                    (OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
                        "offline_geoworld_unity_editor_preview_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldUnityEditorPreviewSummary(entry, summary))
            .ToList();

        var editorScript = OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath;
        var exists = File.Exists(Resolve(projectRoot, editorScript));
        entries.Add(WithOfflineGeoworldUnityEditorPreviewSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal102SourceGoalId + ".script.OfflineGeoworldPreviewWindow",
                RelativePath = editorScript,
                ArtifactKind = "offline_geoworld_unity_editor_preview_script",
                SourceGoalId = Goal102SourceGoalId,
                Sha256 = exists
                    ? HashFor(projectRoot, editorScript, new Dictionary<string, string>(StringComparer.Ordinal))
                    : string.Empty,
                Status = exists
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = exists
                    ? "Unity Editor preview window script exists"
                    : "Unity Editor preview window script missing",
                MetadataOnly = true
            },
            summary));

        entries.Add(WithOfflineGeoworldUnityEditorPreviewSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal102SourceGoalId + ".summary",
                RelativePath = Goal102SourceRoot + "/"
                    + OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_unity_editor_preview_workspace_summary",
                SourceGoalId = Goal102SourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal102SourceRoot + "/"
                    + OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "commands=" + summary.CommandCount
                    + "; kinds=" + summary.CommandKindCount
                    + "; travelSteps=" + summary.TravelWindowStepCount
                    + "; expectedObjects=" + summary.ExpectedObjectCount,
                SafeRatingMetadataSummary = "payloadPath=" + summary.PayloadPath
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_unity_editor_preview",
            "Goal 102 Offline Geoworld Unity Editor Preview",
            Goal102SourceGoalId,
            Goal102SourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldUnityEditorPreviewSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldUnityEditorPreviewWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldUnityEditorPreviewCommandCount = summary.CommandCount,
            OfflineGeoworldUnityEditorPreviewCommandKindCount = summary.CommandKindCount,
            OfflineGeoworldUnityEditorPreviewTravelWindowStepCount = summary.TravelWindowStepCount,
            OfflineGeoworldUnityEditorPreviewExpectedObjectCount = summary.ExpectedObjectCount,
            OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath = summary.EditorWindowScriptPath,
            OfflineGeoworldUnityEditorPreviewMenuItemMarker = summary.MenuItemMarker,
            OfflineGeoworldUnityEditorPreviewPayloadPath = summary.PayloadPath,
            OfflineGeoworldUnityEditorPreviewManualInstructions = summary.ManualInstructions,
            OfflineGeoworldUnityEditorPreviewToolInventoryPassed = summary.ToolInventoryPassed,
            OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady =
                summary.EditorWindowScriptReady,
            OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed =
                summary.SimulatedActionProofPassed,
            OfflineGeoworldUnityEditorPreviewClearOperationProofPassed =
                summary.ClearOperationProofPassed,
            OfflineGeoworldUnityEditorPreviewNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldUnityEditorPreviewQualityGatePassed = summary.QualityGatePassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldUnityEditorPreviewWorkspaceSummary LoadOfflineGeoworldUnityEditorPreviewSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var inventory = TryReadJson(
            projectRoot,
            Goal102SourceRoot + "/" + OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName,
            diagnostics);
        using var proof = TryReadJson(
            projectRoot,
            Goal102SourceRoot + "/" + OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal102SourceRoot + "/" + OfflineGeoworldUnityEditorPreviewToolVocabulary.NegativeProofFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal102SourceRoot + "/" + OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
            diagnostics);

        var commandCount = proof is null ? 0 : ReadGoal102Int(proof.RootElement, "commandCount");
        var kindCount = proof is null ? 0 : ReadGoal102Int(proof.RootElement, "commandKindCount");
        var travelSteps = proof is null ? 0 : ReadGoal102Int(proof.RootElement, "travelWindowStepCount");
        var expectedObjects = proof is null ? 0 : ReadGoal102Int(proof.RootElement, "expectedObjectCount");
        var inventoryPassed = inventory is not null && TryGetBool(inventory.RootElement, "passed");
        var editorReady = quality is not null && TryGetBool(quality.RootElement, "editorWindowScriptReady");
        var simulatedPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var clearPassed = proof is not null && TryGetBool(proof.RootElement, "clearOperationModelPassed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var scriptPath = quality is null
            ? string.Empty
            : ReadGoal102String(quality.RootElement, "editorWindowScriptPath");
        var menu = quality is null
            ? string.Empty
            : ReadGoal102String(quality.RootElement, "menuItemMarker");
        var payload = quality is null
            ? string.Empty
            : ReadGoal102String(quality.RootElement, "goal101PayloadPath");
        var manual = quality is null
            ? string.Empty
            : ReadGoal102String(quality.RootElement, "manualInstructions");
        var relativePaths = IsSafeRelativePath(Goal102SourceRoot)
                            && IsSafeRelativePath(scriptPath);
        var passed = commandCount == 18
                     && kindCount == 10
                     && travelSteps >= 4
                     && expectedObjects == 18
                     && inventoryPassed
                     && editorReady
                     && simulatedPassed
                     && clearPassed
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && !string.IsNullOrWhiteSpace(menu)
                     && !string.IsNullOrWhiteSpace(payload)
                     && !string.IsNullOrWhiteSpace(manual)
                     && relativePaths;
        AddIfFalse(
            passed,
            "goal102.workspace.summary_failed",
            "offline_geoworld_unity_editor_preview",
            diagnostics);
        return new OfflineGeoworldUnityEditorPreviewWorkspaceSummary(
            Passed: passed,
            CommandCount: commandCount,
            CommandKindCount: kindCount,
            TravelWindowStepCount: travelSteps,
            ExpectedObjectCount: expectedObjects,
            EditorWindowScriptPath: scriptPath,
            MenuItemMarker: menu,
            PayloadPath: payload,
            ManualInstructions: manual,
            ToolInventoryPassed: inventoryPassed,
            EditorWindowScriptReady: editorReady,
            SimulatedActionProofPassed: simulatedPassed,
            ClearOperationProofPassed: clearPassed,
            NegativeProofPassed: negativePassed,
            AlphaRuntimeBootstrapUnchanged: alphaUnchanged,
            QualityGatePassed: qualityPassed);
    }

    private static int ReadGoal102Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal102String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldUnityEditorPreviewWorkspaceSummary(
        bool Passed,
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
        bool QualityGatePassed);
}
