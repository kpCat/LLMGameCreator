using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal104InteractiveTravelSourceGoalId =
        "goal_104_offline_geoworld_interactive_travel_preview";
    private const string Goal104InteractiveTravelSourceRoot =
        ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview";
    private const string Goal104InteractiveTravelStreamingAssetsRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104";

    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldInteractiveTravelGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldInteractiveTravelSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                [
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.ReportMarkdownFileName,
                        "offline_geoworld_interactive_travel_report"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName,
                        "offline_geoworld_interactive_travel_manifest"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName,
                        "offline_geoworld_interactive_movement_path"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.ChunkVisibilityFileName,
                        "offline_geoworld_interactive_boundary_zones"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName,
                        "offline_geoworld_interactive_prefetch_plan"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName,
                        "offline_geoworld_interactive_unity_script_inventory"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventoryFileName,
                        "offline_geoworld_interactive_editor_window_inventory"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName,
                        "offline_geoworld_interactive_simulated_execution_proof"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofFileName,
                        "offline_geoworld_interactive_negative_proof"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.WorkspaceBindingInventoryFileName,
                        "offline_geoworld_interactive_workspace_binding_inventory"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.SourceLineageFileName,
                        "offline_geoworld_interactive_source_lineage"),
                    (OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                        "offline_geoworld_interactive_quality_gate")
                ],
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldInteractiveTravelSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            var relativePath = Goal104InteractiveTravelStreamingAssetsRoot + "/" + fileName;
            var exists = File.Exists(Resolve(projectRoot, relativePath));
            entries.Add(WithOfflineGeoworldInteractiveTravelSummary(
                new VisualWorldPreviewArtifactEntry
                {
                    Id = Goal104InteractiveTravelSourceGoalId + ".payload."
                         + Path.GetFileNameWithoutExtension(fileName),
                    RelativePath = relativePath,
                    ArtifactKind = "offline_geoworld_interactive_streamingassets_payload",
                    SourceGoalId = Goal104InteractiveTravelSourceGoalId,
                    Sha256 = exists
                        ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                        : string.Empty,
                    Status = exists
                        ? VisualWorldPreviewArtifactStatus.Passed
                        : VisualWorldPreviewArtifactStatus.Failed,
                    DiagnosticSummary = exists ? "mirrored interactive payload exists" : "mirrored interactive payload missing",
                    SafeRatingMetadataSummary = "metadataOnly=true; relativePath=true"
                },
                summary));
        }

        foreach (var scriptPath in Goal104InteractiveTravelScriptPaths())
        {
            entries.Add(WithOfflineGeoworldInteractiveTravelSummary(
                Goal104ScriptEntry(projectRoot, scriptPath, "offline_geoworld_interactive_unity_script"),
                summary));
        }

        entries.Add(WithOfflineGeoworldInteractiveTravelSummary(
            Goal104ScriptEntry(
                projectRoot,
                OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityEditorWindowScriptPath,
                "offline_geoworld_interactive_editor_window_script"),
            summary));
        entries.Add(WithOfflineGeoworldInteractiveTravelSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = Goal104InteractiveTravelSourceGoalId + ".summary",
                RelativePath = Goal104InteractiveTravelSourceRoot + "/"
                    + OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                ArtifactKind = "offline_geoworld_interactive_travel_workspace_summary",
                SourceGoalId = Goal104InteractiveTravelSourceGoalId,
                Sha256 = HashFor(
                    projectRoot,
                    Goal104InteractiveTravelSourceRoot + "/"
                    + OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.Passed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "samples=" + summary.MovementSampleCount
                    + "; crossings=" + summary.BoundaryCrossingCount
                    + "; objects=" + summary.ObjectCount
                    + "; activeChunks=" + summary.ActiveChunkCounts
                    + "; boundaryPrefetch=" + summary.BoundaryPrefetchCounts,
                SafeRatingMetadataSummary = "expectedVisible=" + summary.ExpectedVisibleObjectCounts
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_interactive_travel",
            "Goal 104 Offline Geoworld Interactive Travel",
            Goal104InteractiveTravelSourceGoalId,
            Goal104InteractiveTravelSourceRoot,
            entries,
            groupDiagnostics);
    }

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldInteractiveTravelSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldInteractiveTravelWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldInteractiveTravelMovementSampleCount = summary.MovementSampleCount,
            OfflineGeoworldInteractiveTravelBoundaryCrossingCount = summary.BoundaryCrossingCount,
            OfflineGeoworldInteractiveTravelObjectCount = summary.ObjectCount,
            OfflineGeoworldInteractiveTravelActiveChunkCounts = summary.ActiveChunkCounts,
            OfflineGeoworldInteractiveTravelBoundaryPrefetchCounts = summary.BoundaryPrefetchCounts,
            OfflineGeoworldInteractiveTravelExpectedVisibleObjectCounts =
                summary.ExpectedVisibleObjectCounts,
            OfflineGeoworldInteractiveTravelUnityScriptsReady = summary.UnityScriptsReady,
            OfflineGeoworldInteractiveTravelEditorWindowReady = summary.EditorWindowReady,
            OfflineGeoworldInteractiveTravelSimulatedExecutionProofPassed = summary.SimulatedProofPassed,
            OfflineGeoworldInteractiveTravelNegativeProofPassed = summary.NegativeProofPassed,
            OfflineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged =
                summary.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldInteractiveTravelQualityGatePassed = summary.QualityGatePassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldInteractiveTravelWorkspaceSummary LoadOfflineGeoworldInteractiveTravelSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var manifest = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName,
            diagnostics);
        using var steps = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName,
            diagnostics);
        using var scripts = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName,
            diagnostics);
        using var editor = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventoryFileName,
            diagnostics);
        using var proof = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName,
            diagnostics);
        using var negative = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            Goal104InteractiveTravelSourceRoot + "/"
            + OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
            diagnostics);

        var stepCount = manifest is null ? 0 : ReadGoal104Int(manifest.RootElement, "movementSampleCount");
        var boundaryCrossingCount = manifest is null ? 0 : ReadGoal104Int(manifest.RootElement, "boundaryCrossingCount");
        var objectCount = manifest is null ? 0 : ReadGoal104Int(manifest.RootElement, "objectCount");
        var active = steps is null ? string.Empty : ReadGoal104StepCounts(steps.RootElement, "activeChunkKeys");
        var prefetch = steps is null ? string.Empty : ReadGoal104StepCounts(steps.RootElement, "boundaryPrefetchChunkKeys");
        var visible = proof is null ? string.Empty : ReadGoal104IntArray(proof.RootElement, "expectedVisibleObjectCountsByStep");
        var scriptsReady = scripts is not null && TryGetBool(scripts.RootElement, "passed");
        var editorReady = editor is not null && TryGetBool(editor.RootElement, "passed");
        var proofPassed = proof is not null && TryGetBool(proof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var relativePaths = IsSafeRelativePath(Goal104InteractiveTravelSourceRoot)
                            && IsSafeRelativePath(Goal104InteractiveTravelStreamingAssetsRoot)
                            && Goal104InteractiveTravelScriptPaths().All(IsSafeRelativePath)
                            && IsSafeRelativePath(
                                OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityEditorWindowScriptPath);
        var passed = stepCount >= 6
                     && boundaryCrossingCount >= 2
                     && objectCount == 18
                     && !string.IsNullOrWhiteSpace(active)
                     && !string.IsNullOrWhiteSpace(prefetch)
                     && !string.IsNullOrWhiteSpace(visible)
                     && scriptsReady
                     && editorReady
                     && proofPassed
                     && negativePassed
                     && alphaUnchanged
                     && qualityPassed
                     && relativePaths;
        AddIfFalse(
            passed,
            "goal104.workspace.summary_failed",
            "offline_geoworld_interactive_travel",
            diagnostics);
        return new OfflineGeoworldInteractiveTravelWorkspaceSummary(
            passed,
            stepCount,
            boundaryCrossingCount,
            objectCount,
            active,
            prefetch,
            visible,
            scriptsReady,
            editorReady,
            proofPassed,
            negativePassed,
            alphaUnchanged,
            qualityPassed);
    }

    private static VisualWorldPreviewArtifactEntry Goal104ScriptEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = Goal104InteractiveTravelSourceGoalId + ".script." + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = Goal104InteractiveTravelSourceGoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Unity interactive travel script exists" : "Unity interactive travel script missing",
            MetadataOnly = true
        };
    }

    private static IReadOnlyList<string> Goal104InteractiveTravelScriptPaths() =>
    [
        OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath,
        OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStateScriptPath,
        OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityChunkVisibilityScriptPath
    ];

    private static int ReadGoal104Int(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out var value) ? value : 0;

    private static string ReadGoal104IntArray(JsonElement element, string propertyName)
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

    private static string ReadGoal104StepCounts(JsonElement element, string arrayProperty)
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
                var index = ReadGoal104Int(step, "stepIndex");
                var count = step.TryGetProperty(arrayProperty, out var values)
                            && values.ValueKind == JsonValueKind.Array
                    ? values.GetArrayLength()
                    : 0;
                return index + ":" + count;
            }));
    }

    private sealed record OfflineGeoworldInteractiveTravelWorkspaceSummary(
        bool Passed,
        int MovementSampleCount,
        int BoundaryCrossingCount,
        int ObjectCount,
        string ActiveChunkCounts,
        string BoundaryPrefetchCounts,
        string ExpectedVisibleObjectCounts,
        bool UnityScriptsReady,
        bool EditorWindowReady,
        bool SimulatedProofPassed,
        bool NegativeProofPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed);
}
