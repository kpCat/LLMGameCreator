using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal103PlayModeTravelSourceGoalId =
        "goal_103_offline_geoworld_playmode_travel_preview";
    private const string Goal103PlayModeTravelSourceRoot =
        ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview";
    private const string Goal103PlayModeTravelStreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldPlayModeTravelGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldPlayModeTravelSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal103PlayModeTravelSourceRoot,
                Goal103PlayModeTravelSourceGoalId,
                [
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_playmode_travel_report"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName,
                        "offline_geoworld_playmode_travel_manifest"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName,
                        "offline_geoworld_playmode_travel_steps"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName,
                        "offline_geoworld_playmode_chunk_visibility"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName,
                        "offline_geoworld_playmode_object_state_index"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_playmode_unity_script_inventory"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.EditorWindowInventoryFileName,
                        "offline_geoworld_playmode_editor_window_inventory"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName,
                        "offline_geoworld_playmode_simulated_execution_proof"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.NegativeProofFileName,
                        "offline_geoworld_playmode_negative_proof"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_playmode_workspace_binding_inventory"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.SourceLineageFileName,
                        "offline_geoworld_playmode_source_lineage"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BClosureFileName,
                        "offline_geoworld_playmode_goal102b_closure"),
                    (OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
                        "offline_geoworld_playmode_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldPlayModeTravelSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal103PlayModeTravelStreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldPlayModeTravelSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal103PlayModeTravelSourceGoalId + ".payload."
                         + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_playmode_streamingassets_payload",
                    SourceGoalId = Goal103PlayModeTravelSourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored play-mode payload exists" : "mirrored play-mode payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; relativePath=true"
                },
                summary));
        }

        foreach (var scriptPath in Goal103PlayModeTravelScriptPaths())
        {
            entries.Add(WithOfflineGeoworldPlayModeTravelSummary(
                ScriptEntry(projectRoot, scriptPath, "offline_geoworld_playmode_unity_script"),
                summary));
        }

        entries.Add(WithOfflineGeoworldPlayModeTravelSummary(
            ScriptEntry(
                projectRoot,
                OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_playmode_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldPlayModeTravelSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal103PlayModeTravelSourceGoalId + ".summary",
                RelativePath = Goal103PlayModeTravelSourceRoot + "/"
                    + OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_playmode_travel_workspace_summary",
                SourceGoalId = Goal103PlayModeTravelSourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal103PlayModeTravelSourceRoot + "/"
                    + OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "steps=" + summary.StepCount
                    + "; objects=" + summary.ObjectCount
                    + "; activeChunks=" + summary.ActiveChunkCounts
                    + "; boundaryPrefetch=" + summary.BoundaryPrefetchCounts,
                SafeRatingMetadataSummary = "expectedVisible=" + summary.ExpectedVisibleObjectCounts
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_playmode_travel",
            "Goal 103 Offline Geoworld Play Mode Travel",
            Goal103PlayModeTravelSourceGoalId,
            Goal103PlayModeTravelSourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldPlayModeTravelSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldPlayModeTravelWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldPlayModeTravelStepCount = summary.StepCount,
            OfflineGeoworldPlayModeTravelObjectCount = summary.ObjectCount,
            OfflineGeoworldPlayModeTravelActiveChunkCounts = summary.ActiveChunkCounts,
            OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts = summary.BoundaryPrefetchCounts,
            OfflineGeoworldPlayModeTravelExpectedVisibleObjectCounts =
                summary.ExpectedVisibleObjectCounts,
            OfflineGeoworldPlayModeTravelUnityScriptsReady = summary.UnityScriptsReady,
            OfflineGeoworldPlayModeTravelEditorWindowReady = summary.EditorWindowReady,
            OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed = summary.SimulatedProofPassed,
            OfflineGeoworldPlayModeTravelNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldPlayModeTravelGoal102BClosureRecorded = summary.Goal102BClosureRecorded,
            OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldPlayModeTravelQualityGatePassed = summary.QualityGatePassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldPlayModeTravelWorkspaceSummary LoadOfflineGeoworldPlayModeTravelSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName,
            diagnostics);
        using var steps = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName,
            diagnostics);
        using var scripts = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityScriptInventoryFileName,
            diagnostics);
        using var editor = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.EditorWindowInventoryFileName,
            diagnostics);
        using var proof = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.NegativeProofFileName,
            diagnostics);
        using var closure = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BClosureFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal103PlayModeTravelSourceRoot + "/"
            + OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
            diagnostics);

        var stepCount = manifest is null ? 0 : ReadGoal103Int(manifest.RootElement, "stepCount");
        var objectCount = manifest is null ? 0 : ReadGoal103Int(manifest.RootElement, "objectCount");
        var active = steps is null ? string.Empty : ReadGoal103StepCounts(steps.RootElement, "activeChunkKeys");
        var prefetch = steps is null ? string.Empty : ReadGoal103StepCounts(steps.RootElement, "boundaryPrefetchChunkKeys");
        var visible = proof is null ? string.Empty : ReadGoal103IntArray(proof.RootElement, "expectedVisibleObjectCountsByStep");
        var scriptsReady = scripts is not null && TryGetBool(scripts.RootElement, "passed");
        var editorReady = editor is not null && TryGetBool(editor.RootElement, "passed");
        var proofPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var closurePassed = closure is not null && TryGetBool(closure.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = IsSafeRelativePath(Goal103PlayModeTravelSourceRoot)
                            && IsSafeRelativePath(Goal103PlayModeTravelStreamingAssetsRoot)
                            && Goal103PlayModeTravelScriptPaths().All(IsSafeRelativePath)
                            && IsSafeRelativePath(
                                OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityEditorWindowScriptPath);
        var passed = stepCount >= 4
                     && objectCount == 18
                     && !string.IsNullOrWhiteSpace(active)
                     && !string.IsNullOrWhiteSpace(prefetch)
                     && !string.IsNullOrWhiteSpace(visible)
                     && scriptsReady
                     && editorReady
                     && proofPassed
                     && negativePassed
                     && closurePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(
            passed,
            "goal103.workspace.summary_failed",
            "offline_geoworld_playmode_travel",
            diagnostics);
        return new OfflineGeoworldPlayModeTravelWorkspaceSummary(
            passed,
            stepCount,
            objectCount,
            active,
            prefetch,
            visible,
            scriptsReady,
            editorReady,
            proofPassed,
            negativePassed,
            closurePassed,
            alphaUnchanged,
            qualityPassed);
    }

    private static VisualWorldPreviewArtifactEntry ScriptEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal103PlayModeTravelSourceGoalId + ".script." + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = Goal103PlayModeTravelSourceGoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Unity play-mode travel script exists" : "Unity play-mode travel script missing",
            MetadataOnly = true
        };
    }

    private static IReadOnlyList<string> Goal103PlayModeTravelScriptPaths() =>
    [
        OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath,
        OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStateScriptPath,
        OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityChunkVisibilityScriptPath
    ];

    private static int ReadGoal103Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal103IntArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        return string.Join(",", property.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.Number ? item.GetInt32().ToString() : string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string ReadGoal103StepCounts(JsonElement element, string arrayProperty)
    {
        if (!element.TryGetProperty("steps", out var steps)
            || steps.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        return string.Join(
            ",",
            steps.EnumerateArray().Select(step =>
            {
                var index = ReadGoal103Int(step, "stepIndex");
                var count = step.TryGetProperty(arrayProperty, out var values)
                            && values.ValueKind == JsonValueKind.Array
                    ? values.GetArrayLength()
                    : 0;
                return index + ":" + count;
            }));
    }

    private sealed record OfflineGeoworldPlayModeTravelWorkspaceSummary(
        bool Passed,
        int StepCount,
        int ObjectCount,
        string ActiveChunkCounts,
        string BoundaryPrefetchCounts,
        string ExpectedVisibleObjectCounts,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool SimulatedProofPassed,
        bool NegativeProofPassed,
        bool Goal102BClosureRecorded,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed);
}
