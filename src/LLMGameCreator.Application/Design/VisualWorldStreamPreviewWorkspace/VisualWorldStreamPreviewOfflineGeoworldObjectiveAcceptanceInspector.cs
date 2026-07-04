using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal107ObjectiveSourceGoalId =
        "goal_107_offline_geoworld_objective_acceptance_run";
    private const string Goal107ObjectiveSourceRoot =
        ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run";
    private const string Goal107ObjectiveStreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldObjectiveAcceptanceGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldObjectiveSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                [
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_objective_report"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.ManifestFileName,
                        "offline_geoworld_objective_manifest"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.ObjectivesFileName,
                        "offline_geoworld_objectives"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.AcceptanceRunFileName,
                        "offline_geoworld_objective_acceptance_run"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.CompletionStateFileName,
                        "offline_geoworld_objective_completion_state"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReplayAcceptanceProofFileName,
                        "offline_geoworld_objective_replay_acceptance_proof"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReadmeFileName,
                        "offline_geoworld_objective_readme"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_objective_unity_script_inventory"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.EditorWindowInventoryFileName,
                        "offline_geoworld_objective_editor_window_inventory"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SimulatedAcceptanceProofFileName,
                        "offline_geoworld_objective_simulated_acceptance_proof"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.NegativeProofFileName,
                        "offline_geoworld_objective_negative_proof"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_objective_workspace_binding_inventory"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.SourceLineageFileName,
                        "offline_geoworld_objective_source_lineage"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaQualityConsolidationFileName,
                        "offline_geoworld_objective_alpha_quality_consolidation"),
                    (OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName,
                        "offline_geoworld_objective_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldObjectiveSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal107ObjectiveStreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldObjectiveSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal107ObjectiveSourceGoalId + ".payload."
                         + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_objective_streamingassets_payload",
                    SourceGoalId = Goal107ObjectiveSourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored Goal107 objective payload exists" : "mirrored Goal107 objective payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; objectiveAcceptance=alphaOnly"
                },
                summary));
        }

        foreach (var scriptPath in Goal107ObjectiveScriptPaths())
        {
            entries.Add(WithOfflineGeoworldObjectiveSummary(
                Goal107ScriptEntry(projectRoot, scriptPath, "offline_geoworld_objective_unity_script"),
                summary));
        }

        entries.Add(WithOfflineGeoworldObjectiveSummary(
            Goal107ScriptEntry(
                projectRoot,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_objective_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldObjectiveSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal107ObjectiveSourceGoalId + ".summary",
                RelativePath = Goal107ObjectiveSourceRoot + "/"
                    + OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_objective_workspace_summary",
                SourceGoalId = Goal107ObjectiveSourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal107ObjectiveSourceRoot + "/"
                    + OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "objectives=" + summary.ObjectiveCount
                    + "; completed=" + summary.CompletedObjectiveCount
                    + "; replaySteps=" + summary.ReplayStepCount,
                SafeRatingMetadataSummary = "finalStatus=" + summary.FinalStatus
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_objective_acceptance",
            "Goal 107 Offline Geoworld Objective Acceptance",
            Goal107ObjectiveSourceGoalId,
            Goal107ObjectiveSourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldObjectiveSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldObjectiveWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldObjectiveCount = summary.ObjectiveCount,
            OfflineGeoworldObjectiveCompletedCount = summary.CompletedObjectiveCount,
            OfflineGeoworldObjectiveReplayStepCount = summary.ReplayStepCount,
            OfflineGeoworldObjectiveStateDeltaCount = summary.StateDeltaCount,
            OfflineGeoworldObjectiveCheckpointStepIndex = summary.CheckpointStepIndex,
            OfflineGeoworldObjectiveFinalStatus = summary.FinalStatus,
            OfflineGeoworldObjectiveFinalStateHash = summary.FinalStateHash,
            OfflineGeoworldObjectiveUnityScriptsReady = summary.UnityScriptsReady,
            OfflineGeoworldObjectiveEditorWindowReady = summary.EditorWindowReady,
            OfflineGeoworldObjectiveReplayAcceptanceProofPassed = summary.ReplayAcceptanceProofPassed,
            OfflineGeoworldObjectiveNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldObjectiveAlphaQualityConsolidationPassed =
                summary.AlphaQualityConsolidationPassed,
            OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldObjectiveQualityGatePassed = summary.QualityGatePassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldObjectiveWorkspaceSummary LoadOfflineGeoworldObjectiveSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.ManifestFileName, diagnostics);
        using var completion = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.CompletionStateFileName, diagnostics);
        using var scripts = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityScriptInventoryFileName, diagnostics);
        using var editor = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.EditorWindowInventoryFileName, diagnostics);
        using var proof = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.SimulatedAcceptanceProofFileName, diagnostics);
        using var negative = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.NegativeProofFileName, diagnostics);
        using var alphaQuality = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaQualityConsolidationFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, Goal107ObjectiveSourceRoot + "/"
            + OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName, diagnostics);

        var objectiveCount = manifest is null ? 0 : ReadGoal107Int(manifest.RootElement, "objectiveCount");
        var replayStepCount = manifest is null ? 0 : ReadGoal107Int(manifest.RootElement, "sourceGoal106ReplayStepCount");
        var deltaCount = manifest is null ? 0 : ReadGoal107Int(manifest.RootElement, "sourceGoal106StateDeltaCount");
        var checkpointStep = manifest is null ? 0 : ReadGoal107Int(manifest.RootElement, "sourceGoal106CheckpointStepIndex");
        var finalHash = manifest is null ? string.Empty : ReadGoal107String(manifest.RootElement, "sourceGoal106FinalStateHash");
        var completedCount = completion is null ? 0 : ReadGoal107Int(completion.RootElement, "completedObjectiveCount");
        var finalStatus = completion is null ? string.Empty : ReadGoal107String(completion.RootElement, "finalStatus");
        var scriptsReady = scripts is not null && TryGetBool(scripts.RootElement, "passed");
        var editorReady = editor is not null && TryGetBool(editor.RootElement, "passed");
        var proofPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var alphaQualityPassed = alphaQuality is not null && TryGetBool(alphaQuality.RootElement, "passed");
        var alphaUnchanged = quality is not null && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = IsSafeRelativePath(Goal107ObjectiveSourceRoot)
                            && IsSafeRelativePath(Goal107ObjectiveStreamingAssetsRoot)
                            && Goal107ObjectiveScriptPaths().All(IsSafeRelativePath)
                            && IsSafeRelativePath(
                                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityEditorWindowScriptPath);
        var passed = objectiveCount >= 6
                     && completedCount == objectiveCount
                     && replayStepCount >= 6
                     && deltaCount >= 6
                     && checkpointStep >= 3
                     && finalStatus == "completed"
                     && !string.IsNullOrWhiteSpace(finalHash)
                     && scriptsReady
                     && editorReady
                     && proofPassed
                     && negativePassed
                     && alphaQualityPassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(passed, "goal107.workspace.summary_failed",
            "offline_geoworld_objective_acceptance", diagnostics);
        return new OfflineGeoworldObjectiveWorkspaceSummary(
            passed,
            objectiveCount,
            completedCount,
            replayStepCount,
            deltaCount,
            checkpointStep,
            finalStatus,
            finalHash,
            scriptsReady,
            editorReady,
            proofPassed,
            negativePassed,
            alphaQualityPassed,
            alphaUnchanged,
            qualityPassed);
    }

    private static VisualWorldPreviewArtifactEntry Goal107ScriptEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal107ObjectiveSourceGoalId + ".script." + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = Goal107ObjectiveSourceGoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Unity Goal107 objective script exists" : "Unity Goal107 objective script missing",
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };
    }

    private static IReadOnlyList<string> Goal107ObjectiveScriptPaths() =>
    [
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath,
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath,
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath
    ];

    private static int ReadGoal107Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal107String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record OfflineGeoworldObjectiveWorkspaceSummary(
        bool Passed,
        int ObjectiveCount,
        int CompletedObjectiveCount,
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
        bool QualityGatePassed);
}
