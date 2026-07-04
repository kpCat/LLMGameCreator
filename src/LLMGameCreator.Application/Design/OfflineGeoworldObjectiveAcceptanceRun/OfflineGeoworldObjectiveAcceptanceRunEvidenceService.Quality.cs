using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

namespace LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

public sealed partial class OfflineGeoworldObjectiveAcceptanceRunEvidenceService
{
    private static OfflineGeoworldObjectiveWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
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
        var group = workspaceText.Contains("offline_geoworld_objective_acceptance", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldObjectiveCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldObjectiveFinalStatus", StringComparison.Ordinal);
        var objectiveCount = pageText.Contains("offlineGeoworldObjectiveCount", StringComparison.Ordinal);
        var completedCount = pageText.Contains("offlineGeoworldObjectiveCompletedCount", StringComparison.Ordinal);
        var finalStatus = pageText.Contains("offlineGeoworldObjectiveFinalStatus", StringComparison.Ordinal);
        var replay = pageText.Contains("offlineGeoworldObjectiveReplaySaveLoadLinkage", StringComparison.Ordinal);
        var scripts = pageText.Contains("offlineGeoworldObjectiveUnityScriptsReady", StringComparison.Ordinal);
        var editor = pageText.Contains("offlineGeoworldObjectiveEditorWindowReady", StringComparison.Ordinal);
        var quality = pageText.Contains(
            "offlineGeoworldObjectiveAlphaQualityConsolidationPassed",
            StringComparison.Ordinal);
        var checklist = pageText.Contains(
            "offlineGeoworldObjectiveManualChecklistSummary",
            StringComparison.Ordinal);
        var alpha = pageText.Contains(
            "offlineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged",
            StringComparison.Ordinal);

        AddIfFalse(group, "goal107.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal107.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal107.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(objectiveCount, "goal107.workspace.objective_count_missing", "page", diagnostics);
        AddIfFalse(completedCount, "goal107.workspace.completed_count_missing", "page", diagnostics);
        AddIfFalse(finalStatus, "goal107.workspace.final_status_missing", "page", diagnostics);
        AddIfFalse(replay, "goal107.workspace.replay_linkage_missing", "page", diagnostics);
        AddIfFalse(scripts, "goal107.workspace.scripts_missing", "page", diagnostics);
        AddIfFalse(editor, "goal107.workspace.editor_missing", "page", diagnostics);
        AddIfFalse(quality, "goal107.workspace.quality_missing", "page", diagnostics);
        AddIfFalse(checklist, "goal107.workspace.checklist_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal107.workspace.alpha_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldObjectiveWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesObjectiveAcceptanceGroup = group,
            WorkspaceReadsGoal107EvidenceByRelativePath = relative,
            WinFormsPageDisplaysObjectiveFields = winForms,
            ShowsObjectiveCount = objectiveCount,
            ShowsCompletedObjectiveCount = completedCount,
            ShowsFinalStatus = finalStatus,
            ShowsReplaySaveLoadLinkage = replay,
            ShowsUnityScriptReadiness = scripts,
            ShowsEditorHelperReadiness = editor,
            ShowsAlphaQualityConsolidationStatus = quality,
            ShowsManualChecklistSummary = checklist,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldObjectiveSourceLineage BuildSourceLineage(
        string root,
        Goal107SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldObjectiveDiagnostic.Error(
                "goal107.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal106Ready, "goal107.lineage.goal106_ready", "Goal106", diagnostics);
        AddIfFalse(context.AlphaRuntimeBootstrapUnchanged, "goal107.lineage.alpha", "AlphaRuntimeBootstrap", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldObjectiveSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal106AcceptedFalsePreserved = !context.Manifest.Accepted,
            Goal106PayloadConsumed = context.Goal106Ready,
            Goal106UnityEvidenceConsumed = context.SourceUnityScripts.Passed && context.SourceEditorWindow.Passed,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldObjectiveAlphaQualityConsolidation BuildAlphaQualityConsolidation(
        string root,
        Goal107SourceContext context,
        OfflineGeoworldObjectiveUnityScriptInventory scripts,
        OfflineGeoworldObjectiveEditorWindowInventory editor)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        var unityFiles = UnityAlphaScriptInventoryPaths()
            .Select(path => SourceFile(root, path, allowManualEditorSceneObjects: path.Contains("/Editor/", StringComparison.Ordinal)))
            .ToList();
        var readable = unityFiles.All(item => item.Exists && item.NotMinified);
        var noNetwork = unityFiles.All(item => item.HasNoProviderNetworkMarkers)
                        && scripts.HasNoProviderNetworkMarkers
                        && editor.HasNoProviderNetworkMarkers;
        var noBootstrap = unityFiles.All(item => item.DoesNotReferenceAlphaRuntimeBootstrap)
                          && scripts.DoesNotReferenceAlphaRuntimeBootstrap
                          && editor.DoesNotReferenceAlphaRuntimeBootstrap;
        var noExternal = unityFiles.All(item => item.HasNoExternalDependencyMarkers)
                         && scripts.HasNoExternalDependencyMarkers;
        var noSceneSettings = unityFiles.All(item => item.HasNoScenePrefabSettingsMutationMarkers)
                              && editor.HasNoScenePrefabSettingsMutationMarkers;
        var streamingRoot = Resolve(root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.StreamingAssetsRelativeRoot);
        var noBinary = !Directory.Exists(streamingRoot)
                       || !Directory.EnumerateFiles(streamingRoot, "*", SearchOption.AllDirectories)
                           .Any(path => IsBinaryOrRasterMedia(Relative(root, path)));
        var travelPreview = unityFiles.Any(item => item.RelativePath.EndsWith(
            "OfflineGeoworldPreviewRunner.cs",
            StringComparison.Ordinal));
        var editorPreview = unityFiles.Any(item => item.RelativePath.EndsWith(
            "OfflineGeoworldPreviewWindow.cs",
            StringComparison.Ordinal));
        var playModeTravel = unityFiles.Any(item => item.RelativePath.EndsWith(
            "OfflineGeoworldPlayModeTravelController.cs",
            StringComparison.Ordinal));
        var interactiveTravel = unityFiles.Any(item => item.RelativePath.EndsWith(
            "OfflineGeoworldInteractiveTravelController.cs",
            StringComparison.Ordinal));
        var interactionProbe = unityFiles.Any(item => item.RelativePath.EndsWith(
            "OfflineGeoworldInteractionController.cs",
            StringComparison.Ordinal));
        var sessionReplay = context.SourceQualityGate.Passed
                            && context.SourceUnityScripts.Passed
                            && context.SourceEditorWindow.Passed;

        AddIfFalse(context.SourceUnityScripts.Passed, "goal107.quality.goal106_scripts", "Goal106 scripts", diagnostics);
        AddIfFalse(context.SourceEditorWindow.Passed, "goal107.quality.goal106_editor", "Goal106 editor", diagnostics);
        AddIfFalse(scripts.Passed, "goal107.quality.goal107_scripts", "Goal107 scripts", diagnostics);
        AddIfFalse(editor.Passed, "goal107.quality.goal107_editor", "Goal107 editor", diagnostics);
        AddIfFalse(readable, "goal107.quality.source_readable", "Unity C# source", diagnostics);
        AddIfFalse(noNetwork, "goal107.quality.network_provider", "Unity C# source", diagnostics);
        AddIfFalse(noBootstrap, "goal107.quality.alpha_bootstrap_dependency", "Unity C# source", diagnostics);
        AddIfFalse(noExternal, "goal107.quality.external_dependency", "Unity C# source", diagnostics);
        AddIfFalse(noSceneSettings, "goal107.quality.scene_settings", "Unity C# source", diagnostics);
        AddIfFalse(noBinary, "goal107.quality.binary_media", "StreamingAssets", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldObjectiveAlphaQualityConsolidation
        {
            Passed = ordered.All(item => item.Severity != "error"),
            TravelPreviewReady = travelPreview,
            EditorPreviewReady = editorPreview,
            PlayModeTravelReady = playModeTravel,
            InteractiveTravelReady = interactiveTravel,
            InteractionProbeReady = interactionProbe,
            SessionReplayReady = sessionReplay,
            ObjectiveAcceptanceRunReady = scripts.Passed && editor.Passed,
            ManualAcceptanceChecklistReady = true,
            SourceReadableNotMinified = readable,
            NoNetworkProviderLlmMarkers = noNetwork,
            NoAlphaRuntimeBootstrapDependency = noBootstrap,
            NoExternalPackageOrNewInputSystemMarkers = noExternal,
            NoScenePrefabSettingsBuildPackageMutation = noSceneSettings,
            NoBinaryRasterMedia = noBinary,
            ScannedUnitySourceFileCount = unityFiles.Count,
            MaxUnitySourceLineCount = unityFiles.Count == 0 ? 0 : unityFiles.Max(item => item.LineCount),
            RemainingNotFinalWarnings =
            [
                "Manual gate offline_geoworld_objective_acceptance_run_verification remains required.",
                "Unity Alpha geoworld objective acceptance is an offline proof path, not a final runtime build."
            ],
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldObjectiveQualityGateScan BuildQualityGate(
        string root,
        Goal107SourceContext context,
        Goal107Payload payload,
        OfflineGeoworldObjectiveUnityScriptInventory scripts,
        OfflineGeoworldObjectiveEditorWindowInventory editor,
        OfflineGeoworldObjectiveReplayAcceptanceProof proof,
        OfflineGeoworldObjectiveNegativeProof negative,
        OfflineGeoworldObjectiveWorkspaceBindingInventory binding,
        OfflineGeoworldObjectiveSourceLineage lineage,
        OfflineGeoworldObjectiveAlphaQualityConsolidation consolidation)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        diagnostics.AddRange(context.Diagnostics);
        diagnostics.AddRange(scripts.Diagnostics);
        diagnostics.AddRange(editor.Diagnostics);
        diagnostics.AddRange(proof.Diagnostics);
        diagnostics.AddRange(binding.Diagnostics);
        diagnostics.AddRange(lineage.Diagnostics);
        diagnostics.AddRange(consolidation.Diagnostics);
        var sourceFiles = CandidateSourceFiles(root)
            .Where(File.Exists)
            .Select(path => ScanSourceFile(root, path))
            .ToList();
        foreach (var file in sourceFiles.Where(item => item.Lines > 700))
        {
            diagnostics.Add(OfflineGeoworldObjectiveDiagnostic.Error(
                "goal107.source.file_over_700",
                file.RelativePath,
                "Goal107 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldObjectiveDiagnostic.Error(
                "goal107.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldObjectiveAcceptanceRunVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var payloadCreated = payload.Manifest.PayloadFileCount == 6
                             && payload.Manifest.ObjectiveCount >= 6
                             && payload.Manifest.SourceGoal106ReplayStepCount >= 6
                             && payload.Manifest.SourceGoal106StateDeltaCount >= 6
                             && payload.Manifest.SourceGoal106CheckpointStepIndex >= 3;
        var noNetwork = proof.NoProviderOrNetworkMarkers
                        && scripts.HasNoProviderNetworkMarkers
                        && editor.HasNoProviderNetworkMarkers
                        && consolidation.NoNetworkProviderLlmMarkers;
        var noRaw = proof.NoRawGeodata;
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia && consolidation.NoBinaryRasterMedia;
        var noSceneSettings = editor.HasNoScenePrefabSettingsMutationMarkers
                              && scripts.Files.All(item => item.HasNoScenePrefabSettingsMutationMarkers)
                              && consolidation.NoScenePrefabSettingsBuildPackageMutation;
        var noExternal = scripts.HasNoExternalDependencyMarkers
                         && scripts.Files.All(item => item.HasNoExternalDependencyMarkers)
                         && consolidation.NoExternalPackageOrNewInputSystemMarkers;

        AddIfFalse(context.Goal106Ready, "goal107.quality.goal106", "Goal106", diagnostics);
        AddIfFalse(payloadCreated, "goal107.quality.payload", "payload", diagnostics);
        AddIfFalse(proof.Passed, "goal107.quality.proof", "replay acceptance proof", diagnostics);
        AddIfFalse(negative.Passed, "goal107.quality.negative", "negative proof", diagnostics);
        AddIfFalse(scripts.Passed, "goal107.quality.scripts", "Unity scripts", diagnostics);
        AddIfFalse(editor.Passed, "goal107.quality.editor", "Unity editor helper", diagnostics);
        AddIfFalse(binding.Passed, "goal107.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal107.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(consolidation.Passed, "goal107.quality.alpha_quality", "Alpha quality", diagnostics);
        AddIfFalse(alphaUnchanged, "goal107.quality.alpha_bootstrap",
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal107.quality.network_provider", "payload/scripts", diagnostics);
        AddIfFalse(noRaw, "goal107.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal107.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal107.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noSceneSettings, "goal107.quality.scene_settings", "Unity editor helper", diagnostics);
        AddIfFalse(noExternal, "goal107.quality.external_dependency", "Unity scripts", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldObjectiveQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal106Consumed = context.Goal106Ready,
            ObjectivePayloadCreated = payloadCreated,
            ReplayAcceptanceProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            WorkspaceBindingPassed = binding.Passed,
            SourceLineagePassed = lineage.Passed,
            AlphaQualityConsolidationPassed = consolidation.Passed,
            AlphaRuntimeBootstrapUnchanged = alphaUnchanged,
            AlphaRuntimeBootstrapAfterHash = alphaHash,
            AlphaRuntimeBootstrapAfterLineCount = alphaLineCount,
            NoNetworkOrProviderImplementation = noNetwork,
            NoRawGeodataDump = noRaw,
            NoAbsolutePaths = noAbsolute,
            NoBinaryOrRasterMedia = noBinary,
            NoScenePrefabSettingsChanges = noSceneSettings,
            NoExternalDependenciesOrNewInputSystem = noExternal,
            ObjectiveCount = payload.Manifest.ObjectiveCount,
            CompletedObjectiveCount = payload.CompletionState.CompletedObjectiveCount,
            FinalStatus = payload.CompletionState.FinalStatus,
            ReplayStepCount = payload.Manifest.SourceGoal106ReplayStepCount,
            StateDeltaCount = payload.Manifest.SourceGoal106StateDeltaCount,
            CheckpointStepIndex = payload.Manifest.SourceGoal106CheckpointStepIndex,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldObjectiveAcceptanceRun/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldObjectiveAcceptanceRun/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldObjectiveAcceptanceRunProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath,
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityEditorWindowScriptPath,
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal107/",
                ".llmgc/procedural/goal-107-offline-geoworld-objective-acceptance-run/",
                "docs/agent-tasks/goal-107-offline-geoworld-objective-acceptance-run/",
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
}
