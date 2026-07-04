using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal105InteractionSourceGoalId =
        "goal_105_offline_geoworld_interaction_playable_probe";
    private const string Goal105InteractionSourceRoot =
        ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe";
    private const string Goal105InteractionStreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldInteractionGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldInteractionSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                [
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_interaction_report"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName,
                        "offline_geoworld_interaction_manifest"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName,
                        "offline_geoworld_interaction_targets"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName,
                        "offline_geoworld_interaction_actions"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName,
                        "offline_geoworld_interaction_session_script"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName,
                        "offline_geoworld_interaction_state_delta_plan"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_interaction_unity_script_inventory"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.EditorWindowInventoryFileName,
                        "offline_geoworld_interaction_editor_window_inventory"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofFileName,
                        "offline_geoworld_interaction_simulated_session_proof"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.NegativeProofFileName,
                        "offline_geoworld_interaction_negative_proof"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_interaction_workspace_binding_inventory"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.SourceLineageFileName,
                        "offline_geoworld_interaction_source_lineage"),
                    (OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
                        "offline_geoworld_interaction_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldInteractionSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal105InteractionStreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldInteractionSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal105InteractionSourceGoalId + ".payload."
                         + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_interaction_streamingassets_payload",
                    SourceGoalId = Goal105InteractionSourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored Goal105 interaction payload exists" : "mirrored Goal105 interaction payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; stateDeltasSeparate=true"
                },
                summary));
        }

        foreach (var scriptPath in Goal105InteractionScriptPaths())
        {
            entries.Add(WithOfflineGeoworldInteractionSummary(
                Goal105ScriptEntry(projectRoot, scriptPath, "offline_geoworld_interaction_unity_script"),
                summary));
        }

        entries.Add(WithOfflineGeoworldInteractionSummary(
            Goal105ScriptEntry(
                projectRoot,
                OfflineGeoworldInteractionPlayableProbeVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_interaction_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldInteractionSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal105InteractionSourceGoalId + ".summary",
                RelativePath = Goal105InteractionSourceRoot + "/"
                    + OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_interaction_workspace_summary",
                SourceGoalId = Goal105InteractionSourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal105InteractionSourceRoot + "/"
                    + OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "targets=" + summary.TargetCount
                    + "; actionKinds=" + summary.ActionKindCount
                    + "; events=" + summary.ScriptedEventCount
                    + "; deltas=" + summary.StateDeltaCount,
                SafeRatingMetadataSummary = "finalStateHash=" + summary.FinalStateHash
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_interactions",
            "Goal 105 Offline Geoworld Interactions",
            Goal105InteractionSourceGoalId,
            Goal105InteractionSourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldInteractionSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldInteractionWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldInteractionTargetCount = summary.TargetCount,
            OfflineGeoworldInteractionActionKindCount = summary.ActionKindCount,
            OfflineGeoworldInteractionActionCount = summary.ActionCount,
            OfflineGeoworldInteractionScriptedEventCount = summary.ScriptedEventCount,
            OfflineGeoworldInteractionStateDeltaCount = summary.StateDeltaCount,
            OfflineGeoworldInteractionFinalStateHash = summary.FinalStateHash,
            OfflineGeoworldInteractionStateHashChainPassed = summary.StateHashChainPassed,
            OfflineGeoworldInteractionUnityScriptsReady = summary.UnityScriptsReady,
            OfflineGeoworldInteractionEditorWindowReady = summary.EditorWindowReady,
            OfflineGeoworldInteractionUnitySafetyScanPassed = summary.UnitySafetyScanPassed,
            OfflineGeoworldInteractionSimulatedSessionProofPassed = summary.SimulatedProofPassed,
            OfflineGeoworldInteractionNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldInteractionQualityGatePassed = summary.QualityGatePassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldInteractionWorkspaceSummary LoadOfflineGeoworldInteractionSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal105InteractionSourceRoot + "/"
            + OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName,
            diagnostics);
        using var scripts = TryReadJson(
            projectRoot,
            Goal105InteractionSourceRoot + "/"
            + OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName,
            diagnostics);
        using var editor = TryReadJson(
            projectRoot,
            Goal105InteractionSourceRoot + "/"
            + OfflineGeoworldInteractionPlayableProbeVocabulary.EditorWindowInventoryFileName,
            diagnostics);
        using var proof = TryReadJson(
            projectRoot,
            Goal105InteractionSourceRoot + "/"
            + OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal105InteractionSourceRoot + "/"
            + OfflineGeoworldInteractionPlayableProbeVocabulary.NegativeProofFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal105InteractionSourceRoot + "/"
            + OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
            diagnostics);

        var targetCount = manifest is null ? 0 : ReadGoal105Int(manifest.RootElement, "targetCount");
        var actionKindCount = manifest is null ? 0 : ReadGoal105Int(manifest.RootElement, "actionKindCount");
        var actionCount = manifest is null ? 0 : ReadGoal105Int(manifest.RootElement, "actionCount");
        var eventCount = manifest is null ? 0 : ReadGoal105Int(manifest.RootElement, "scriptedEventCount");
        var deltaCount = manifest is null ? 0 : ReadGoal105Int(manifest.RootElement, "stateDeltaCount");
        var finalHash = manifest is null ? string.Empty : ReadGoal105String(manifest.RootElement, "finalStateHash");
        var scriptsReady = scripts is not null && TryGetBool(scripts.RootElement, "passed");
        var editorReady = editor is not null && TryGetBool(editor.RootElement, "passed");
        var proofPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var hashChain = proof is not null && TryGetBool(proof.RootElement, "deterministicStateHashChainPassed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var safetyPassed = scripts is not null && TryGetBool(scripts.RootElement, "hasNoExternalDependencyMarkers")
                           && TryGetBool(scripts.RootElement, "hasNoProviderNetworkMarkers");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = IsSafeRelativePath(Goal105InteractionSourceRoot)
                            && IsSafeRelativePath(Goal105InteractionStreamingAssetsRoot)
                            && Goal105InteractionScriptPaths().All(IsSafeRelativePath)
                            && IsSafeRelativePath(
                                OfflineGeoworldInteractionPlayableProbeVocabulary.UnityEditorWindowScriptPath);
        var passed = targetCount >= 8
                     && actionKindCount >= 5
                     && eventCount >= 6
                     && deltaCount >= 6
                     && !string.IsNullOrWhiteSpace(finalHash)
                     && scriptsReady
                     && editorReady
                     && safetyPassed
                     && proofPassed
                     && hashChain
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(
            passed,
            "goal105.workspace.summary_failed",
            "offline_geoworld_interactions",
            diagnostics);
        return new OfflineGeoworldInteractionWorkspaceSummary(
            passed,
            targetCount,
            actionKindCount,
            actionCount,
            eventCount,
            deltaCount,
            finalHash,
            hashChain,
            scriptsReady,
            editorReady,
            safetyPassed,
            proofPassed,
            negativePassed,
            alphaUnchanged,
            qualityPassed);
    }

    private static VisualWorldPreviewArtifactEntry Goal105ScriptEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal105InteractionSourceGoalId + ".script." + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = Goal105InteractionSourceGoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Unity Goal105 interaction script exists" : "Unity Goal105 interaction script missing",
            MetadataOnly = true
        };
    }

    private static IReadOnlyList<string> Goal105InteractionScriptPaths() =>
    [
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath,
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityTargetScriptPath,
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStateDeltaLogScriptPath
    ];

    private static int ReadGoal105Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal105String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldInteractionWorkspaceSummary(
        bool Passed,
        int TargetCount,
        int ActionKindCount,
        int ActionCount,
        int ScriptedEventCount,
        int StateDeltaCount,
        string FinalStateHash,
        bool StateHashChainPassed,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool UnitySafetyScanPassed,
        bool SimulatedProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed);
}
