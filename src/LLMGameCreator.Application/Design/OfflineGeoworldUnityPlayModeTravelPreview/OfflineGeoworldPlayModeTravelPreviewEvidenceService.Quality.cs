using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

public sealed partial class OfflineGeoworldPlayModeTravelPreviewEvidenceService
{
    private static OfflineGeoworldPlayModeWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
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
        var group = workspaceText.Contains("offline_geoworld_playmode_travel", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldPlayModeTravelPreviewVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldPlayModeTravelStepCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldPlayModeTravelUnityScriptsReady", StringComparison.Ordinal);
        var stepCount = pageText.Contains("offlineGeoworldPlayModeTravelStepCount", StringComparison.Ordinal);
        var active = pageText.Contains("offlineGeoworldPlayModeTravelActiveChunkCounts", StringComparison.Ordinal);
        var prefetch = pageText.Contains("offlineGeoworldPlayModeTravelBoundaryPrefetchCounts", StringComparison.Ordinal);
        var visible = pageText.Contains("offlineGeoworldPlayModeTravelExpectedVisibleObjectCounts", StringComparison.Ordinal);
        var scripts = pageText.Contains("offlineGeoworldPlayModeTravelUnityScriptsReady", StringComparison.Ordinal);
        var editor = pageText.Contains("offlineGeoworldPlayModeTravelEditorWindowReady", StringComparison.Ordinal);
        var proof = pageText.Contains("offlineGeoworldPlayModeTravelSimulatedExecutionProofPassed", StringComparison.Ordinal);
        var closure = pageText.Contains("offlineGeoworldPlayModeTravelGoal102BClosureRecorded", StringComparison.Ordinal);
        var alpha = pageText.Contains("offlineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);

        AddIfFalse(group, "goal103.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal103.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal103.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(stepCount, "goal103.workspace.step_count_missing", "page", diagnostics);
        AddIfFalse(active, "goal103.workspace.active_chunks_missing", "page", diagnostics);
        AddIfFalse(prefetch, "goal103.workspace.prefetch_missing", "page", diagnostics);
        AddIfFalse(visible, "goal103.workspace.visible_counts_missing", "page", diagnostics);
        AddIfFalse(scripts, "goal103.workspace.scripts_missing", "page", diagnostics);
        AddIfFalse(editor, "goal103.workspace.editor_missing", "page", diagnostics);
        AddIfFalse(proof, "goal103.workspace.proof_missing", "page", diagnostics);
        AddIfFalse(closure, "goal103.workspace.closure_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal103.workspace.alpha_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPlayModeWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesPlayModeTravelGroup = group,
            WorkspaceReadsGoal103EvidenceByRelativePath = relative,
            WinFormsPageDisplaysPlayModeTravelFields = winForms,
            ShowsTravelStepCount = stepCount,
            ShowsActiveChunkCounts = active,
            ShowsBoundaryPrefetchCounts = prefetch,
            ShowsExpectedVisibleObjectCounts = visible,
            ShowsUnityScriptReadiness = scripts,
            ShowsEditorLaunchHelperReadiness = editor,
            ShowsSimulatedPlayModeProofStatus = proof,
            ShowsGoal102BFalsePositiveClosureStatus = closure,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPlayModeSourceLineage BuildSourceLineage(
        string root,
        Goal103SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldPlayModeDiagnostic.Error(
                "goal103.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal101AcceptedFalse, "goal103.lineage.goal101_accepted", "Goal101", diagnostics);
        AddIfFalse(context.Goal101CountsProven, "goal103.lineage.goal101_counts", "Goal101", diagnostics);
        AddIfFalse(context.Goal101QualityGatePassed, "goal103.lineage.goal101_quality", "Goal101", diagnostics);
        AddIfFalse(context.Goal102QualityGatePassed, "goal103.lineage.goal102_quality", "Goal102", diagnostics);
        AddIfFalse(context.Goal102BClosureInputsPresent, "goal103.lineage.goal102b_inputs", "Goal102B", diagnostics);
        AddIfFalse(context.Goal102BActualHeadBeforeBlobRead, "goal103.lineage.goal102b_actual", "Goal102B", diagnostics);
        AddIfFalse(!context.Goal102BActualHeadBeforeMalformedDetected, "goal103.lineage.goal102b_false_positive", "Goal102B", diagnostics);
        AddIfFalse(context.Goal102BTrustDefectRecorded, "goal103.lineage.goal102b_trust", "Goal102B", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPlayModeSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal101AcceptedFalsePreserved = context.Goal101AcceptedFalse,
            Goal101PayloadConsumed = context.Goal101CountsProven && context.Goal101QualityGatePassed,
            Goal102EvidenceConsumed = context.Goal102QualityGatePassed,
            Goal102BBlockedStatusPreserved = context.Goal102BClosureInputsPresent
                                             && !context.Goal102BActualHeadBeforeMalformedDetected,
            Goal102BActualEvidenceConsumed = context.Goal102BActualHeadBeforeBlobRead
                                             && context.Goal102BTrustDefectRecorded,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static Goal102BFalsePositiveClosure BuildGoal102BClosure(Goal103SourceContext context)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        var productBlockerClosed = context.Goal102BClosureInputsPresent
                                   && context.Goal102BActualHeadBeforeBlobRead
                                   && !context.Goal102BActualHeadBeforeMalformedDetected
                                   && context.Goal102BWorkingTreeSourceReadable
                                   && context.Goal102BTrustDefectRecorded;
        AddIfFalse(context.Goal102BClosureInputsPresent, "goal103.closure.inputs_missing", "Goal102B", diagnostics);
        AddIfFalse(context.Goal102BActualHeadBeforeBlobRead, "goal103.closure.actual_read", "Goal102B", diagnostics);
        AddIfFalse(!context.Goal102BActualHeadBeforeMalformedDetected, "goal103.closure.false_positive", "Goal102B", diagnostics);
        AddIfFalse(context.Goal102BWorkingTreeSourceReadable, "goal103.closure.working_tree", "Goal102B", diagnostics);
        AddIfFalse(context.Goal102BTrustDefectRecorded, "goal103.closure.trust_defect", "Goal102B", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new Goal102BFalsePositiveClosure
        {
            Passed = ordered.All(item => item.Severity != "error") && productBlockerClosed,
            Goal102BRemainsBlocked = true,
            ProductSourceBlockerClosed = productBlockerClosed,
            Goal102ANotMarkedGreenByThisGoal = true,
            Goal102BNotMarkedGreen = true,
            ActualHeadBeforeEvidenceRead = context.Goal102BActualHeadBeforeBlobRead,
            ActualHeadBeforeMalformedDetected = context.Goal102BActualHeadBeforeMalformedDetected,
            WorkingTreeSourceReadable = context.Goal102BWorkingTreeSourceReadable,
            ActualHeadRawPhysicalLineCount = context.Goal102BActualHeadRawPhysicalLineCount,
            ActualHeadMaxPhysicalLineLength = context.Goal102BActualHeadMaxPhysicalLineLength,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPlayModeQualityGateScan BuildQualityGate(
        string root,
        Goal103SourceContext context,
        Goal103Payload payload,
        OfflineGeoworldPlayModeUnityScriptInventory scripts,
        OfflineGeoworldPlayModeEditorWindowInventory editor,
        OfflineGeoworldPlayModeSimulatedExecutionProof proof,
        OfflineGeoworldPlayModeNegativeProof negative,
        OfflineGeoworldPlayModeWorkspaceBindingInventory binding,
        OfflineGeoworldPlayModeSourceLineage lineage,
        Goal102BFalsePositiveClosure closure)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(scripts.Diagnostics);
        diagnostics.AddRange(editor.Diagnostics);
        diagnostics.AddRange(proof.Diagnostics);
        diagnostics.AddRange(binding.Diagnostics);
        diagnostics.AddRange(lineage.Diagnostics);
        diagnostics.AddRange(closure.Diagnostics);
        var sourceFiles = CandidateSourceFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .ToList();
        foreach (var file in sourceFiles.Where(item => item.Lines > 700))
        {
            diagnostics.Add(OfflineGeoworldPlayModeDiagnostic.Error(
                "goal103.source.file_over_700",
                file.RelativePath,
                "New or touched Goal103 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldPlayModeDiagnostic.Error(
                "goal103.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldPlayModeTravelPreviewVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldPlayModeTravelPreviewVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var stepPlan = payload.Steps.StepCount >= 4
                       && payload.Steps.Steps.All(item => item.ExpectedVisibleObjectCount == item.VisibleObjectIds.Count);
        var boundary = payload.Steps.Steps.All(item => item.BoundaryPrefetchChunkKeys.Count > 0)
                       && payload.Steps.Steps
                           .Select(item => string.Join(",", item.BoundaryPrefetchChunkKeys))
                           .Distinct(StringComparer.Ordinal)
                           .Count() >= 2;
        var diffs = payload.Steps.Steps.Any(item => item.NewlyVisibleObjectIds.Count > 0)
                    && payload.Steps.Steps.Any(item => item.NewlyHiddenObjectIds.Count > 0);
        var noNetwork = proof.NoProviderOrNetworkMarkers
                        && scripts.HasNoProviderNetworkMarkers
                        && editor.HasNoProviderNetworkMarkers;
        var noRaw = proof.NoRawGeodata
                    && payload.ObjectStateIndex.Objects.All(item => !item.RawGeodataIncluded);
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia;
        var noSceneSettings = editor.HasNoScenePrefabSettingsMutationMarkers
                              && scripts.Files.All(item => item.HasNoScenePrefabSettingsMutationMarkers);

        AddIfFalse(context.Goal101CountsProven, "goal103.quality.goal101", "Goal101", diagnostics);
        AddIfFalse(closure.Passed, "goal103.quality.goal102b_closure", "Goal102B closure", diagnostics);
        AddIfFalse(payload.Manifest.PayloadFileCount == 5, "goal103.quality.payload", "payload", diagnostics);
        AddIfFalse(stepPlan, "goal103.quality.steps", "steps", diagnostics);
        AddIfFalse(boundary, "goal103.quality.boundary", "steps", diagnostics);
        AddIfFalse(diffs, "goal103.quality.diffs", "steps", diagnostics);
        AddIfFalse(scripts.Passed, "goal103.quality.scripts", "Unity scripts", diagnostics);
        AddIfFalse(editor.Passed, "goal103.quality.editor", "Unity editor helper", diagnostics);
        AddIfFalse(proof.Passed, "goal103.quality.proof", "simulated execution proof", diagnostics);
        AddIfFalse(negative.Passed, "goal103.quality.negative", "negative proof", diagnostics);
        AddIfFalse(binding.Passed, "goal103.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal103.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(alphaUnchanged, "goal103.quality.alpha_bootstrap",
            OfflineGeoworldPlayModeTravelPreviewVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal103.quality.network_provider", "payload/scripts", diagnostics);
        AddIfFalse(noRaw, "goal103.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal103.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal103.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noSceneSettings, "goal103.quality.scene_settings", "Unity editor helper", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPlayModeQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal101Consumed = context.Goal101CountsProven,
            Goal102BClosureRecorded = closure.Passed,
            PlayModePayloadCreated = payload.Manifest.PayloadFileCount == 5,
            TravelStepPlanBuilt = stepPlan,
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
            ObjectCount = payload.Manifest.ObjectCount,
            MaxActiveChunkCount = payload.Manifest.MaxActiveChunkCount,
            MaxBoundaryPrefetchChunkCount = payload.Manifest.MaxBoundaryPrefetchChunkCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPlayModeTravelPreview/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPlayModeTravelPreview/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPlayModeTravelPreviewProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelState.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeChunkVisibility.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103/",
                ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview/",
                "docs/agent-tasks/goal-103-offline-geoworld-playmode-travel-preview/",
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
        OfflineGeoworldPlayModeUnityScriptInventory scripts,
        OfflineGeoworldPlayModeEditorWindowInventory editor,
        OfflineGeoworldPlayModeSimulatedExecutionProof proof,
        OfflineGeoworldPlayModeNegativeProof negative,
        OfflineGeoworldPlayModeWorkspaceBindingInventory binding,
        OfflineGeoworldPlayModeSourceLineage lineage,
        Goal102BFalsePositiveClosure closure,
        OfflineGeoworldPlayModeQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityScriptInventoryFileName] =
                Serialize(scripts),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.EditorWindowInventoryFileName] =
                Serialize(editor),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName] =
                Serialize(proof),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BClosureFileName] =
                Serialize(closure),
            [OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static OfflineGeoworldPlayModeTravelReport BuildReport(
        Goal103Payload payload,
        OfflineGeoworldPlayModeUnityScriptInventory scripts,
        OfflineGeoworldPlayModeEditorWindowInventory editor,
        OfflineGeoworldPlayModeSimulatedExecutionProof proof,
        OfflineGeoworldPlayModeNegativeProof negative,
        OfflineGeoworldPlayModeWorkspaceBindingInventory binding,
        Goal102BFalsePositiveClosure closure,
        OfflineGeoworldPlayModeQualityGateScan quality,
        IReadOnlyDictionary<string, string> evidence) =>
        new()
        {
            StepCount = payload.Manifest.StepCount,
            ObjectCount = payload.Manifest.ObjectCount,
            MaxActiveChunkCount = payload.Manifest.MaxActiveChunkCount,
            MaxBoundaryPrefetchChunkCount = payload.Manifest.MaxBoundaryPrefetchChunkCount,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            SimulatedExecutionProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            Goal102BClosureRecorded = closure.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed,
            ManifestHash =
                Hash(payload.PayloadFiles[OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName]),
            StepsHash =
                Hash(payload.PayloadFiles[OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName]),
            ChunkVisibilityHash =
                Hash(payload.PayloadFiles[OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName]),
            ObjectStateIndexHash =
                Hash(payload.PayloadFiles[OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName]),
            UnityScriptInventoryHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityScriptInventoryFileName]),
            EditorWindowInventoryHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.EditorWindowInventoryFileName]),
            SimulatedExecutionProofHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofFileName]),
            NegativeProofHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.NegativeProofFileName]),
            WorkspaceBindingInventoryHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.WorkspaceBindingInventoryFileName]),
            SourceLineageHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.SourceLineageFileName]),
            QualityGateHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateScanFileName]),
            Goal102BClosureHash =
                Hash(evidence[OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BClosureFileName])
        };

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.CommandCatalogFileName,
            "Goal101 command catalog"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName,
            "Goal101 travel window script"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal101SourceRoot
         + "/"
         + OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateScanFileName,
            "Goal101 quality gate"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102SourceRoot
         + "/"
         + OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName,
            "Goal102 editor preview quality gate"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BSourceRoot
         + "/"
         + OfflineGeoworldActualUnityEditorSourceReformatVocabulary.BeforeAfterFileName,
            "Goal102B actual before/after evidence"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BSourceRoot
         + "/"
         + OfflineGeoworldActualUnityEditorSourceReformatVocabulary.TrustAuditFileName,
            "Goal102B trust audit"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath,
            "Goal103 Unity play-mode controller"),
        (OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityEditorWindowScriptPath,
            "Goal103 Unity editor helper")
    ];

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldUnityPlayModeTravelPreview");
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldUnityPlayModeTravelPreview");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldUnityPlayModeTravelPreviewProductSmokeTests.cs"));
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs"));
        paths.Add(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStateScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityChunkVisibilityScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityEditorWindowScriptPath));
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

    private static OfflineGeoworldPlayModeSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldPlayModeSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }
}
