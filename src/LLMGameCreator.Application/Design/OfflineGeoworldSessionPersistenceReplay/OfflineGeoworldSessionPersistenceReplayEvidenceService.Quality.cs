using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

namespace LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

public sealed partial class OfflineGeoworldSessionPersistenceReplayEvidenceService
{
    private static OfflineGeoworldSessionWorkspaceBindingInventory BuildWorkspaceBindingInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
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
        var group = workspaceText.Contains("offline_geoworld_session_replay", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldSessionPersistenceReplayVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldSessionReplayStepCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldSessionCheckpointStepIndex", StringComparison.Ordinal);
        var deltas = pageText.Contains("offlineGeoworldSessionStateDeltaCount", StringComparison.Ordinal);
        var steps = pageText.Contains("offlineGeoworldSessionReplayStepCount", StringComparison.Ordinal);
        var checkpoint = pageText.Contains("offlineGeoworldSessionCheckpointStepIndex", StringComparison.Ordinal);
        var finalHash = pageText.Contains("offlineGeoworldSessionFinalStateHash", StringComparison.Ordinal);
        var scripts = pageText.Contains("offlineGeoworldSessionUnityScriptsReady", StringComparison.Ordinal);
        var editor = pageText.Contains("offlineGeoworldSessionEditorWindowReady", StringComparison.Ordinal);
        var proof = pageText.Contains("offlineGeoworldSessionSimulatedReplayProofPassed", StringComparison.Ordinal);
        var checklist = pageText.Contains("offlineGeoworldSessionAcceptanceChecklistStepCount", StringComparison.Ordinal);
        var alpha = pageText.Contains("offlineGeoworldSessionAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);

        AddIfFalse(group, "goal106.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal106.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal106.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(deltas, "goal106.workspace.delta_count_missing", "page", diagnostics);
        AddIfFalse(steps, "goal106.workspace.replay_step_missing", "page", diagnostics);
        AddIfFalse(checkpoint, "goal106.workspace.checkpoint_missing", "page", diagnostics);
        AddIfFalse(finalHash, "goal106.workspace.final_hash_missing", "page", diagnostics);
        AddIfFalse(scripts, "goal106.workspace.scripts_missing", "page", diagnostics);
        AddIfFalse(editor, "goal106.workspace.editor_missing", "page", diagnostics);
        AddIfFalse(proof, "goal106.workspace.proof_missing", "page", diagnostics);
        AddIfFalse(checklist, "goal106.workspace.checklist_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal106.workspace.alpha_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSessionWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesSessionReplayGroup = group,
            WorkspaceReadsGoal106EvidenceByRelativePath = relative,
            WinFormsPageDisplaysSessionReplayFields = winForms,
            ShowsDeltaCount = deltas,
            ShowsReplayStepCount = steps,
            ShowsCheckpointStep = checkpoint,
            ShowsFinalHash = finalHash,
            ShowsUnityScriptReadiness = scripts,
            ShowsEditorHelperReadiness = editor,
            ShowsSimulatedReplayProofStatus = proof,
            ShowsAcceptanceChecklistSummary = checklist,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldSessionSourceLineage BuildSourceLineage(
        string root,
        Goal106SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldSessionDiagnostic.Error(
                "goal106.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal105Ready, "goal106.lineage.goal105_ready", "Goal105", diagnostics);
        AddIfFalse(context.AlphaRuntimeBootstrapUnchanged, "goal106.lineage.alpha", "AlphaRuntimeBootstrap", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSessionSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal105AcceptedFalsePreserved = !context.Manifest.Accepted,
            Goal105PayloadConsumed = context.Goal105Ready,
            Goal105UnityEvidenceConsumed = context.UnityScripts.Passed,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldSessionQualityGateScan BuildQualityGate(
        string root,
        Goal106SourceContext context,
        Goal106Payload payload,
        OfflineGeoworldSessionUnityScriptInventory scripts,
        OfflineGeoworldSessionEditorWindowInventory editor,
        OfflineGeoworldSessionSimulatedReplayProof proof,
        OfflineGeoworldSessionNegativeProof negative,
        OfflineGeoworldSessionWorkspaceBindingInventory binding,
        OfflineGeoworldSessionSourceLineage lineage)
    {
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
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
            diagnostics.Add(OfflineGeoworldSessionDiagnostic.Error(
                "goal106.source.file_over_700",
                file.RelativePath,
                "Goal106 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldSessionDiagnostic.Error(
                "goal106.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldSessionPersistenceReplayVocabulary
                                     .AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldSessionPersistenceReplayVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var payloadCreated = payload.Manifest.PayloadFileCount == 6
                             && payload.Manifest.ReplayStepCount >= 6
                             && payload.Manifest.StateDeltaCount >= 6
                             && payload.Manifest.CheckpointStepIndex >= 3;
        var noNetwork = proof.NoProviderOrNetworkMarkers
                        && scripts.HasNoProviderNetworkMarkers
                        && editor.HasNoProviderNetworkMarkers;
        var noRaw = proof.NoRawGeodata
                    && payload.InitialState.Targets.All(item => !item.RawGeodataIncluded);
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia;
        var noSceneSettings = editor.HasNoScenePrefabSettingsMutationMarkers
                              && scripts.Files.All(item => item.HasNoScenePrefabSettingsMutationMarkers);
        var noExternal = scripts.HasNoExternalDependencyMarkers
                         && scripts.Files.All(item => item.HasNoExternalDependencyMarkers);

        AddIfFalse(context.Goal105Ready, "goal106.quality.goal105", "Goal105", diagnostics);
        AddIfFalse(payloadCreated, "goal106.quality.payload", "payload", diagnostics);
        AddIfFalse(proof.Passed, "goal106.quality.proof", "simulated save/load/replay proof", diagnostics);
        AddIfFalse(negative.Passed, "goal106.quality.negative", "negative proof", diagnostics);
        AddIfFalse(scripts.Passed, "goal106.quality.scripts", "Unity scripts", diagnostics);
        AddIfFalse(editor.Passed, "goal106.quality.editor", "Unity editor helper", diagnostics);
        AddIfFalse(binding.Passed, "goal106.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal106.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(alphaUnchanged, "goal106.quality.alpha_bootstrap",
            OfflineGeoworldSessionPersistenceReplayVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal106.quality.network_provider", "payload/scripts", diagnostics);
        AddIfFalse(noRaw, "goal106.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal106.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal106.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noSceneSettings, "goal106.quality.scene_settings", "Unity editor helper", diagnostics);
        AddIfFalse(noExternal, "goal106.quality.external_dependency", "Unity scripts", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSessionQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal105Consumed = context.Goal105Ready,
            SessionPayloadCreated = payloadCreated,
            SaveLoadReplayProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
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
            NoExternalDependenciesOrNewInputSystem = noExternal,
            ReplayStepCount = payload.Manifest.ReplayStepCount,
            StateDeltaCount = payload.Manifest.StateDeltaCount,
            CheckpointStepIndex = payload.Manifest.CheckpointStepIndex,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldSessionPersistenceReplay/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldSessionPersistenceReplay/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldSessionPersistenceReplayProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath,
                OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath,
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106/",
                ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay/",
                "docs/agent-tasks/goal-106-offline-geoworld-session-persistence-replay/",
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
        OfflineGeoworldSessionUnityScriptInventory scripts,
        OfflineGeoworldSessionEditorWindowInventory editor,
        OfflineGeoworldSessionSimulatedReplayProof proof,
        OfflineGeoworldSessionNegativeProof negative,
        OfflineGeoworldSessionWorkspaceBindingInventory binding,
        OfflineGeoworldSessionSourceLineage lineage,
        OfflineGeoworldSessionQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldSessionPersistenceReplayVocabulary.UnityScriptInventoryFileName] =
                Serialize(scripts),
            [OfflineGeoworldSessionPersistenceReplayVocabulary.EditorWindowInventoryFileName] =
                Serialize(editor),
            [OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName] =
                Serialize(proof),
            [OfflineGeoworldSessionPersistenceReplayVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldSessionPersistenceReplayVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldSessionPersistenceReplayVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static OfflineGeoworldSessionReport BuildReport(
        Goal106Payload payload,
        OfflineGeoworldSessionUnityScriptInventory scripts,
        OfflineGeoworldSessionEditorWindowInventory editor,
        OfflineGeoworldSessionSimulatedReplayProof proof,
        OfflineGeoworldSessionNegativeProof negative,
        OfflineGeoworldSessionWorkspaceBindingInventory binding,
        OfflineGeoworldSessionQualityGateScan quality) =>
        new()
        {
            ReplayStepCount = payload.Manifest.ReplayStepCount,
            StateDeltaCount = payload.Manifest.StateDeltaCount,
            CheckpointStepIndex = payload.Manifest.CheckpointStepIndex,
            CheckpointStateHash = payload.Manifest.CheckpointStateHash,
            FinalStateHash = payload.Manifest.FinalStateHash,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            SimulatedSaveLoadReplayProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed
        };

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root
         + "/"
         + OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName,
            "Goal105 interaction manifest"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root
         + "/"
         + OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName,
            "Goal105 targets"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root
         + "/"
         + OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName,
            "Goal105 actions"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root
         + "/"
         + OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName,
            "Goal105 session script"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root
         + "/"
         + OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName,
            "Goal105 state delta plan"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.SourceGoal105Root
         + "/"
         + OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName,
            "Goal105 Unity script inventory"),
        ("unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs",
            "Goal105 Unity interaction controller"),
        ("unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs",
            "Goal105 state delta log"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath,
            "Goal106 Unity save-load controller"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath,
            "Goal106 Unity replay controller"),
        (OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath,
            "Goal106 Unity editor helper")
    ];

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldSessionPersistenceReplay");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceService.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewEvidenceWriter.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportBuilder.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportRenderer.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWinFormsBindingScanner.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal106.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldSessionReplayInspector.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal106Quality.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal106.cs");
        AddPath(paths, root, "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldSessionPersistenceReplay");
        AddPath(paths, root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldSessionPersistenceReplayProductSmokeTests.cs");
        AddPath(paths, root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath);
        AddPath(paths, root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath);
        AddPath(paths, root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath);
        AddPath(paths, root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath);
        return paths;
    }

    private static void AddPath(ISet<string> paths, string root, string relativePath) =>
        paths.Add(Resolve(root, relativePath));

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

    private static OfflineGeoworldSessionSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldSessionSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }
}
