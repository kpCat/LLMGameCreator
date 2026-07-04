using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal106SessionSourceGoalId =
        "goal_106_offline_geoworld_session_persistence_replay";
    private const string Goal106SessionSourceRoot =
        ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay";
    private const string Goal106SessionStreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldSessionReplayGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldSessionSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                [
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_session_report"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName,
                        "offline_geoworld_session_manifest"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.InitialStateFileName,
                        "offline_geoworld_session_initial_state"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.DeltaLogFileName,
                        "offline_geoworld_session_delta_log"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.ReplayScriptFileName,
                        "offline_geoworld_session_replay_script"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.AcceptanceChecklistFileName,
                        "offline_geoworld_session_acceptance_checklist"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.ReadmeFileName,
                        "offline_geoworld_session_readme"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_session_unity_script_inventory"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.EditorWindowInventoryFileName,
                        "offline_geoworld_session_editor_window_inventory"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
                        "offline_geoworld_session_simulated_replay_proof"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.NegativeProofFileName,
                        "offline_geoworld_session_negative_proof"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_session_workspace_binding_inventory"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceLineageFileName,
                        "offline_geoworld_session_source_lineage"),
                    (OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
                        "offline_geoworld_session_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldSessionSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal106SessionStreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldSessionSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal106SessionSourceGoalId + ".payload."
                         + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_session_streamingassets_payload",
                    SourceGoalId = Goal106SessionSourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored Goal106 session payload exists" : "mirrored Goal106 session payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; saveLoadReplay=alphaOnly"
                },
                summary));
        }

        foreach (var scriptPath in Goal106SessionScriptPaths())
        {
            entries.Add(WithOfflineGeoworldSessionSummary(
                Goal106ScriptEntry(projectRoot, scriptPath, "offline_geoworld_session_unity_script"),
                summary));
        }

        entries.Add(WithOfflineGeoworldSessionSummary(
            Goal106ScriptEntry(
                projectRoot,
                OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_session_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldSessionSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal106SessionSourceGoalId + ".summary",
                RelativePath = Goal106SessionSourceRoot + "/"
                    + OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_session_workspace_summary",
                SourceGoalId = Goal106SessionSourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal106SessionSourceRoot + "/"
                    + OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "steps=" + summary.ReplayStepCount
                    + "; deltas=" + summary.StateDeltaCount
                    + "; checkpoint=" + summary.CheckpointStepIndex,
                SafeRatingMetadataSummary = "finalStateHash=" + summary.FinalStateHash
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_session_replay",
            "Goal 106 Offline Geoworld Session Replay",
            Goal106SessionSourceGoalId,
            Goal106SessionSourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldSessionSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldSessionWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldSessionReplayStepCount = summary.ReplayStepCount,
            OfflineGeoworldSessionStateDeltaCount = summary.StateDeltaCount,
            OfflineGeoworldSessionCheckpointStepIndex = summary.CheckpointStepIndex,
            OfflineGeoworldSessionAcceptanceChecklistStepCount = summary.AcceptanceChecklistStepCount,
            OfflineGeoworldSessionCheckpointStateHash = summary.CheckpointStateHash,
            OfflineGeoworldSessionFinalStateHash = summary.FinalStateHash,
            OfflineGeoworldSessionUnityScriptsReady = summary.UnityScriptsReady,
            OfflineGeoworldSessionEditorWindowReady = summary.EditorWindowReady,
            OfflineGeoworldSessionSimulatedReplayProofPassed = summary.SimulatedReplayProofPassed,
            OfflineGeoworldSessionNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldSessionQualityGatePassed = summary.QualityGatePassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldSessionWorkspaceSummary LoadOfflineGeoworldSessionSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName,
            diagnostics);
        using var checklist = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.AcceptanceChecklistFileName,
            diagnostics);
        using var scripts = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.UnityScriptInventoryFileName,
            diagnostics);
        using var editor = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.EditorWindowInventoryFileName,
            diagnostics);
        using var proof = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.NegativeProofFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal106SessionSourceRoot + "/"
            + OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
            diagnostics);

        var stepCount = manifest is null ? 0 : ReadGoal106Int(manifest.RootElement, "replayStepCount");
        var deltaCount = manifest is null ? 0 : ReadGoal106Int(manifest.RootElement, "stateDeltaCount");
        var checkpointStep = manifest is null ? 0 : ReadGoal106Int(manifest.RootElement, "checkpointStepIndex");
        var checkpointHash = manifest is null ? string.Empty : ReadGoal106String(manifest.RootElement, "checkpointStateHash");
        var finalHash = manifest is null ? string.Empty : ReadGoal106String(manifest.RootElement, "finalStateHash");
        var checklistStepCount = checklist is null ? 0 : ReadGoal106Int(checklist.RootElement, "stepCount");
        var scriptsReady = scripts is not null && TryGetBool(scripts.RootElement, "passed");
        var editorReady = editor is not null && TryGetBool(editor.RootElement, "passed");
        var proofPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = IsSafeRelativePath(Goal106SessionSourceRoot)
                            && IsSafeRelativePath(Goal106SessionStreamingAssetsRoot)
                            && Goal106SessionScriptPaths().All(IsSafeRelativePath)
                            && IsSafeRelativePath(
                                OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath);
        var passed = stepCount >= 6
                     && deltaCount >= 6
                     && checkpointStep >= 3
                     && checklistStepCount > 0
                     && !string.IsNullOrWhiteSpace(finalHash)
                     && scriptsReady
                     && editorReady
                     && proofPassed
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(
            passed,
            "goal106.workspace.summary_failed",
            "offline_geoworld_session_replay",
            diagnostics);
        return new OfflineGeoworldSessionWorkspaceSummary(
            passed,
            stepCount,
            deltaCount,
            checkpointStep,
            checklistStepCount,
            checkpointHash,
            finalHash,
            scriptsReady,
            editorReady,
            proofPassed,
            negativePassed,
            alphaUnchanged,
            qualityPassed);
    }

    private static VisualWorldPreviewArtifactEntry Goal106ScriptEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal106SessionSourceGoalId + ".script." + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = Goal106SessionSourceGoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Unity Goal106 session replay script exists" : "Unity Goal106 session replay script missing",
            MetadataOnly = true
        };
    }

    private static IReadOnlyList<string> Goal106SessionScriptPaths() =>
    [
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath,
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath,
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath
    ];

    private static int ReadGoal106Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal106String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldSessionWorkspaceSummary(
        bool Passed,
        int ReplayStepCount,
        int StateDeltaCount,
        int CheckpointStepIndex,
        int AcceptanceChecklistStepCount,
        string CheckpointStateHash,
        string FinalStateHash,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool SimulatedReplayProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed);
}
