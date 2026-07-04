using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

public sealed partial class OfflineGeoworldInteractiveTravelPreviewEvidenceService
{
    private static OfflineGeoworldInteractiveWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        var workspaceDirectory = Resolve(
            root,
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        var pagePath = Resolve(
            root,
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs");
        var workspaceText = Directory.Exists(workspaceDirectory)
            ? string.Join(Environment.NewLine, Directory.EnumerateFiles(workspaceDirectory, "*.cs")
                .Select(path => File.ReadAllText(path, Encoding.UTF8)))
            : string.Empty;
        var pageText = File.Exists(pagePath) ? File.ReadAllText(pagePath, Encoding.UTF8) : string.Empty;
        var group = workspaceText.Contains("offline_geoworld_interactive_travel", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldInteractiveTravelPreviewVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldInteractiveTravelMovementSampleCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldInteractiveTravelUnityScriptsReady", StringComparison.Ordinal);
        var samples = pageText.Contains("offlineGeoworldInteractiveTravelMovementSampleCount", StringComparison.Ordinal);
        var crossings = pageText.Contains("offlineGeoworldInteractiveTravelBoundaryCrossingCount", StringComparison.Ordinal);
        var active = pageText.Contains("offlineGeoworldInteractiveTravelActiveChunkCounts", StringComparison.Ordinal);
        var prefetch = pageText.Contains("offlineGeoworldInteractiveTravelBoundaryPrefetchCounts", StringComparison.Ordinal);
        var visible = pageText.Contains("offlineGeoworldInteractiveTravelExpectedVisibleObjectCounts", StringComparison.Ordinal);
        var scripts = pageText.Contains("offlineGeoworldInteractiveTravelUnityScriptsReady", StringComparison.Ordinal);
        var editor = pageText.Contains("offlineGeoworldInteractiveTravelEditorWindowReady", StringComparison.Ordinal);
        var proof = pageText.Contains("offlineGeoworldInteractiveTravelSimulatedExecutionProofPassed", StringComparison.Ordinal);
        var alpha = pageText.Contains("offlineGeoworldInteractiveTravelAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);

        AddIfFalse(group, "goal104.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal104.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal104.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(samples, "goal104.workspace.sample_count_missing", "page", diagnostics);
        AddIfFalse(crossings, "goal104.workspace.crossing_count_missing", "page", diagnostics);
        AddIfFalse(active, "goal104.workspace.active_chunks_missing", "page", diagnostics);
        AddIfFalse(prefetch, "goal104.workspace.prefetch_missing", "page", diagnostics);
        AddIfFalse(visible, "goal104.workspace.visible_counts_missing", "page", diagnostics);
        AddIfFalse(scripts, "goal104.workspace.scripts_missing", "page", diagnostics);
        AddIfFalse(editor, "goal104.workspace.editor_missing", "page", diagnostics);
        AddIfFalse(proof, "goal104.workspace.proof_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal104.workspace.alpha_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractiveWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesInteractiveTravelGroup = group,
            WorkspaceReadsGoal104EvidenceByRelativePath = relative,
            WinFormsPageDisplaysInteractiveTravelFields = winForms,
            ShowsMovementSampleCount = samples,
            ShowsBoundaryCrossingCount = crossings,
            ShowsActiveChunkCounts = active,
            ShowsBoundaryPrefetchCounts = prefetch,
            ShowsExpectedVisibleObjectCounts = visible,
            ShowsUnityScriptReadiness = scripts,
            ShowsEditorLaunchHelperReadiness = editor,
            ShowsSimulatedMovementProofStatus = proof,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldInteractiveSourceLineage BuildSourceLineage(
        string root,
        Goal104SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldInteractiveDiagnostic.Error(
                "goal104.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal103Ready, "goal104.lineage.goal103_ready", "Goal103", diagnostics);
        AddIfFalse(context.AlphaRuntimeBootstrapUnchanged, "goal104.lineage.alpha", "AlphaRuntimeBootstrap", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractiveSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal103AcceptedFalsePreserved = !context.SourceManifest.Accepted,
            Goal103PayloadConsumed = context.Goal103Ready,
            Goal103UnityScriptEvidenceConsumed = records.Any(item =>
                item.RelativePath.EndsWith("OfflineGeoworldPlayModeTravelController.cs", StringComparison.Ordinal)
                && item.Exists),
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldInteractiveQualityGateScan BuildQualityGate(
        string root,
        Goal104SourceContext context,
        Goal104Payload payload,
        OfflineGeoworldInteractiveUnityScriptInventory scripts,
        OfflineGeoworldInteractiveEditorWindowInventory editor,
        OfflineGeoworldInteractiveSimulatedExecutionProof proof,
        OfflineGeoworldInteractiveNegativeProof negative,
        OfflineGeoworldInteractiveWorkspaceBindingInventory binding,
        OfflineGeoworldInteractiveSourceLineage lineage)
    {
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(scripts.Diagnostics);
        diagnostics.AddRange(editor.Diagnostics);
        diagnostics.AddRange(proof.Diagnostics);
        diagnostics.AddRange(binding.Diagnostics);
        diagnostics.AddRange(lineage.Diagnostics);
        var sourceFiles = CandidateSourceFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .ToList();
        foreach (var file in sourceFiles.Where(item => item.Lines > 700))
        {
            diagnostics.Add(OfflineGeoworldInteractiveDiagnostic.Error(
                "goal104.source.file_over_700",
                file.RelativePath,
                "New or touched Goal104 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldInteractiveDiagnostic.Error(
                "goal104.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldInteractiveTravelPreviewVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldInteractiveTravelPreviewVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var movementPath = payload.MovementPath.MovementSampleCount >= 6
                           && payload.MovementPath.Objects.Count == 18;
        var boundaryZones = payload.BoundaryZones.BoundaryCrossingCount >= 2
                            && payload.BoundaryZones.BoundaryZones.Count >= 2;
        var prefetchPlan = payload.PrefetchPlan.PrefetchPlanCount == payload.BoundaryZones.BoundaryCrossingCount
                           && payload.PrefetchPlan.Plans.All(item => item.PrefetchChunkKeys.Count > 0);
        var boundary = payload.MovementPath.Steps.All(item => item.BoundaryPrefetchChunkKeys.Count > 0)
                       && boundaryZones
                       && payload.MovementPath.Steps
                           .Select(item => string.Join(",", item.BoundaryPrefetchChunkKeys))
                           .Distinct(StringComparer.Ordinal)
                           .Count() >= 2;
        var diffs = payload.MovementPath.Steps.Any(item => item.NewlyVisibleObjectIds.Count > 0)
                    && payload.MovementPath.Steps.Any(item => item.NewlyHiddenObjectIds.Count > 0);
        var noNetwork = proof.NoProviderOrNetworkMarkers
                        && scripts.HasNoProviderNetworkMarkers
                        && editor.HasNoProviderNetworkMarkers;
        var noRaw = proof.NoRawGeodata
                    && payload.MovementPath.Objects.All(item => !item.RawGeodataIncluded);
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia;
        var noSceneSettings = editor.HasNoScenePrefabSettingsMutationMarkers
                              && scripts.Files.All(item => item.HasNoScenePrefabSettingsMutationMarkers);

        AddIfFalse(context.Goal103Ready, "goal104.quality.goal103", "Goal103", diagnostics);
        AddIfFalse(payload.Manifest.PayloadFileCount == 5, "goal104.quality.payload", "payload", diagnostics);
        AddIfFalse(movementPath, "goal104.quality.movement_path", "movement path", diagnostics);
        AddIfFalse(boundaryZones, "goal104.quality.boundary_zones", "boundary zones", diagnostics);
        AddIfFalse(prefetchPlan, "goal104.quality.prefetch_plan", "prefetch plan", diagnostics);
        AddIfFalse(boundary, "goal104.quality.boundary", "movement path", diagnostics);
        AddIfFalse(diffs, "goal104.quality.diffs", "movement path", diagnostics);
        AddIfFalse(scripts.Passed, "goal104.quality.scripts", "Unity scripts", diagnostics);
        AddIfFalse(editor.Passed, "goal104.quality.editor", "Unity editor helper", diagnostics);
        AddIfFalse(proof.Passed, "goal104.quality.proof", "simulated execution proof", diagnostics);
        AddIfFalse(negative.Passed, "goal104.quality.negative", "negative proof", diagnostics);
        AddIfFalse(binding.Passed, "goal104.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal104.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(alphaUnchanged, "goal104.quality.alpha_bootstrap",
            OfflineGeoworldInteractiveTravelPreviewVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal104.quality.network_provider", "payload/scripts", diagnostics);
        AddIfFalse(noRaw, "goal104.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal104.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal104.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noSceneSettings, "goal104.quality.scene_settings", "Unity editor helper", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractiveQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal103Consumed = context.Goal103Ready,
            InteractivePayloadCreated = payload.Manifest.PayloadFileCount == 5,
            MovementPathBuilt = movementPath,
            BoundaryZonesBuilt = boundaryZones,
            PrefetchPlanBuilt = prefetchPlan,
            BoundaryPrefetchRepresented = boundary,
            ObjectVisibilityDiffsBuilt = diffs,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            SimulatedExecutionProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            SourceLineagePassed = lineage.Passed,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            NoNetworkOrProviderImplementation = noNetwork,
            NoRawGeodataDump = noRaw,
            NoAbsolutePaths = noAbsolute,
            NoBinaryOrRasterMedia = noBinary,
            NoScenePrefabSettingsChanges = noSceneSettings,
            StepCount = payload.Manifest.StepCount,
            MovementSampleCount = payload.Manifest.MovementSampleCount,
            BoundaryCrossingCount = payload.Manifest.BoundaryCrossingCount,
            PrefetchPlanCount = payload.Manifest.PrefetchPlanCount,
            ObjectCount = payload.Manifest.ObjectCount,
            MaxActiveChunkCount = payload.Manifest.MaxActiveChunkCount,
            MaxBoundaryPrefetchChunkCount = payload.Manifest.MaxBoundaryPrefetchChunkCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldInteractiveTravelPreview/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldInteractiveTravelPreview/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldInteractiveTravelPreviewProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104/",
                ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview/",
                "docs/agent-tasks/goal-104-offline-geoworld-interactive-travel-preview/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = ordered
        };
    }

    private static IReadOnlyDictionary<string, string> BuildEvidencePayloads(
        OfflineGeoworldInteractiveUnityScriptInventory scripts,
        OfflineGeoworldInteractiveEditorWindowInventory editor,
        OfflineGeoworldInteractiveSimulatedExecutionProof proof,
        OfflineGeoworldInteractiveNegativeProof negative,
        OfflineGeoworldInteractiveWorkspaceBindingInventory binding,
        OfflineGeoworldInteractiveSourceLineage lineage,
        OfflineGeoworldInteractiveQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName] =
                Serialize(scripts),
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventoryFileName] =
                Serialize(editor),
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName] =
                Serialize(proof),
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static OfflineGeoworldInteractiveTravelReport BuildReport(
        Goal104Payload payload,
        OfflineGeoworldInteractiveUnityScriptInventory scripts,
        OfflineGeoworldInteractiveEditorWindowInventory editor,
        OfflineGeoworldInteractiveSimulatedExecutionProof proof,
        OfflineGeoworldInteractiveNegativeProof negative,
        OfflineGeoworldInteractiveWorkspaceBindingInventory binding,
        OfflineGeoworldInteractiveQualityGateScan quality,
        IReadOnlyDictionary<string, string> evidence) =>
        new()
        {
            StepCount = payload.Manifest.StepCount,
            MovementSampleCount = payload.Manifest.MovementSampleCount,
            BoundaryCrossingCount = payload.Manifest.BoundaryCrossingCount,
            PrefetchPlanCount = payload.Manifest.PrefetchPlanCount,
            ObjectCount = payload.Manifest.ObjectCount,
            MaxActiveChunkCount = payload.Manifest.MaxActiveChunkCount,
            MaxBoundaryPrefetchChunkCount = payload.Manifest.MaxBoundaryPrefetchChunkCount,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            SimulatedExecutionProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed,
            ManifestHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName]),
            StepsHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName]),
            ChunkVisibilityHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.ChunkVisibilityFileName]),
            ObjectStateIndexHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName]),
            MovementPathHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName]),
            BoundaryZonesHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.ChunkVisibilityFileName]),
            PrefetchPlanHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName]),
            UnityScriptInventoryHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName]),
            EditorWindowInventoryHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventoryFileName]),
            SimulatedExecutionProofHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName]),
            NegativeProofHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofFileName]),
            WorkspaceBindingInventoryHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.WorkspaceBindingInventoryFileName]),
            SourceLineageHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.SourceLineageFileName]),
            QualityGateHash =
                Hash(evidence[OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName])
        };

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot
         + "/"
         + OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName,
            "Goal103 manifest"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot
         + "/"
         + OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName,
            "Goal103 travel steps"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot
         + "/"
         + OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName,
            "Goal103 chunk visibility"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot
         + "/"
         + OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName,
            "Goal103 object state index"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot
         + "/"
         + OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName,
            "Goal103 simulated execution proof"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.Goal103SourceRoot
         + "/"
         + OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName,
            "Goal103 quality gate"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath,
            "Goal103 Unity play-mode controller"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityEditorWindowScriptPath,
            "Goal103 Unity editor helper"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath,
            "Goal104 Unity interactive controller"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityEditorWindowScriptPath,
            "Goal104 Unity editor helper")
    ];

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldInteractiveTravelPreview");
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldInteractiveTravelPreview");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldInteractiveTravelPreviewProductSmokeTests.cs"));
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs"));
        paths.Add(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStateScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityChunkVisibilityScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityEditorWindowScriptPath));
        return paths;
    }

    private static void AddDirectory(ISet<string> paths, string root, string relativePath)
    {
        var directory = Resolve(root, relativePath);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            paths.Add(file);
        }
    }

    private static (string RelativePath, int Lines) ScanSourceFile(string root, string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return (Relative(root, path), CountLines(text));
    }

    private static OfflineGeoworldInteractiveSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldInteractiveSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }
}
