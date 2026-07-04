using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

public sealed partial class OfflineGeoworldInteractionPlayableProbeEvidenceService
{
    private static OfflineGeoworldInteractionWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
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
        var group = workspaceText.Contains("offline_geoworld_interactions", StringComparison.Ordinal);
        var relative = workspaceText.Contains(
            OfflineGeoworldInteractionPlayableProbeVocabulary.RelativeOutputDirectory,
            StringComparison.Ordinal);
        var winForms = pageText.Contains("offlineGeoworldInteractionTargetCount", StringComparison.Ordinal)
                       && pageText.Contains("offlineGeoworldInteractionStateDeltaCount", StringComparison.Ordinal);
        var targets = pageText.Contains("offlineGeoworldInteractionTargetCount", StringComparison.Ordinal);
        var kinds = pageText.Contains("offlineGeoworldInteractionActionKindCount", StringComparison.Ordinal);
        var events = pageText.Contains("offlineGeoworldInteractionScriptedEventCount", StringComparison.Ordinal);
        var deltas = pageText.Contains("offlineGeoworldInteractionStateDeltaCount", StringComparison.Ordinal);
        var hash = pageText.Contains("offlineGeoworldInteractionStateHashChainPassed", StringComparison.Ordinal);
        var scripts = pageText.Contains("offlineGeoworldInteractionUnityScriptsReady", StringComparison.Ordinal);
        var editor = pageText.Contains("offlineGeoworldInteractionEditorWindowReady", StringComparison.Ordinal);
        var safety = pageText.Contains("offlineGeoworldInteractionUnitySafetyScanPassed", StringComparison.Ordinal);
        var proof = pageText.Contains("offlineGeoworldInteractionSimulatedSessionProofPassed", StringComparison.Ordinal);
        var alpha = pageText.Contains("offlineGeoworldInteractionAlphaRuntimeBootstrapUnchanged", StringComparison.Ordinal);

        AddIfFalse(group, "goal105.workspace.group_missing", "workspace", diagnostics);
        AddIfFalse(relative, "goal105.workspace.relative_path_missing", "workspace", diagnostics);
        AddIfFalse(winForms, "goal105.workspace.winforms_fields_missing", "page", diagnostics);
        AddIfFalse(targets, "goal105.workspace.target_count_missing", "page", diagnostics);
        AddIfFalse(kinds, "goal105.workspace.action_kind_missing", "page", diagnostics);
        AddIfFalse(events, "goal105.workspace.event_count_missing", "page", diagnostics);
        AddIfFalse(deltas, "goal105.workspace.delta_count_missing", "page", diagnostics);
        AddIfFalse(hash, "goal105.workspace.hash_chain_missing", "page", diagnostics);
        AddIfFalse(scripts, "goal105.workspace.scripts_missing", "page", diagnostics);
        AddIfFalse(editor, "goal105.workspace.editor_missing", "page", diagnostics);
        AddIfFalse(safety, "goal105.workspace.safety_missing", "page", diagnostics);
        AddIfFalse(proof, "goal105.workspace.proof_missing", "page", diagnostics);
        AddIfFalse(alpha, "goal105.workspace.alpha_missing", "page", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractionWorkspaceBindingInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            WorkspaceCatalogIncludesInteractionGroup = group,
            WorkspaceReadsGoal105EvidenceByRelativePath = relative,
            WinFormsPageDisplaysInteractionFields = winForms,
            ShowsTargetCount = targets,
            ShowsActionKindCount = kinds,
            ShowsScriptedEventCount = events,
            ShowsStateDeltaCount = deltas,
            ShowsDeterministicHashChainStatus = hash,
            ShowsUnityScriptReadiness = scripts,
            ShowsEditorHelperReadiness = editor,
            ShowsUnitySafetyScanStatus = safety,
            ShowsSimulatedSessionProofStatus = proof,
            ShowsAlphaRuntimeBootstrapUnchangedStatus = alpha,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldInteractionSourceLineage BuildSourceLineage(
        string root,
        Goal105SourceContext context)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(root, item.Path, item.Purpose))
            .ToList();
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
        foreach (var missing in records.Where(item => !item.Exists))
        {
            diagnostics.Add(OfflineGeoworldInteractionDiagnostic.Error(
                "goal105.lineage.missing",
                missing.RelativePath,
                "Required source lineage artifact is missing."));
        }

        AddIfFalse(context.Goal104Ready, "goal105.lineage.goal104_ready", "Goal104", diagnostics);
        AddIfFalse(context.AlphaRuntimeBootstrapUnchanged, "goal105.lineage.alpha", "AlphaRuntimeBootstrap", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractionSourceLineage
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal104AcceptedFalsePreserved = !context.SourceManifest.Accepted,
            Goal104PayloadConsumed = context.Goal104Ready,
            Goal104UnityScriptEvidenceConsumed = context.SourceUnityScripts.Passed,
            Records = records,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldInteractionQualityGateScan BuildQualityGate(
        string root,
        Goal105SourceContext context,
        Goal105Payload payload,
        OfflineGeoworldInteractionUnityScriptInventory scripts,
        OfflineGeoworldInteractionEditorWindowInventory editor,
        OfflineGeoworldInteractionSimulatedSessionProof proof,
        OfflineGeoworldInteractionNegativeProof negative,
        OfflineGeoworldInteractionWorkspaceBindingInventory binding,
        OfflineGeoworldInteractionSourceLineage lineage)
    {
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
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
            diagnostics.Add(OfflineGeoworldInteractionDiagnostic.Error(
                "goal105.source.file_over_700",
                file.RelativePath,
                "New or touched Goal105 C# files must remain below 700 logical lines."));
        }

        foreach (var file in sourceFiles.Where(item => item.Lines > 1000))
        {
            diagnostics.Add(OfflineGeoworldInteractionDiagnostic.Error(
                "goal105.source.file_over_1000",
                file.RelativePath,
                "Changed C# files must remain below 1000 logical lines."));
        }

        var alphaPath = Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.AlphaRuntimeBootstrapPath);
        var alphaText = File.Exists(alphaPath) ? File.ReadAllText(alphaPath, Encoding.UTF8) : string.Empty;
        var alphaHash = File.Exists(alphaPath) ? HashFile(alphaPath) : string.Empty;
        var alphaLineCount = CountLines(alphaText);
        var alphaUnchanged = string.Equals(
                                 alphaHash,
                                 OfflineGeoworldInteractionPlayableProbeVocabulary.AlphaRuntimeBootstrapExpectedHash,
                                 StringComparison.OrdinalIgnoreCase)
                             && alphaLineCount == OfflineGeoworldInteractionPlayableProbeVocabulary
                                 .AlphaRuntimeBootstrapExpectedLineCount;
        var targets = payload.Targets.TargetCount >= 8
                      && payload.Targets.Targets.All(item => item.RawGeodataIncluded == false);
        var actionKinds = payload.Actions.ActionKindCount >= 5
                          && OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredActionKinds
                              .All(kind => payload.Actions.ActionKinds.Contains(kind, StringComparer.Ordinal));
        var session = payload.SessionScript.EventCount >= 6
                      && payload.SessionScript.Events.All(item => item.AvailableByDistance);
        var deltas = payload.StateDeltaPlan.StateDeltaCount == payload.SessionScript.EventCount
                     && payload.StateDeltaPlan.MutatesBaseDataDirectly == false;
        var noNetwork = proof.NoProviderOrNetworkMarkers
                        && scripts.HasNoProviderNetworkMarkers
                        && editor.HasNoProviderNetworkMarkers;
        var noRaw = proof.NoRawGeodata
                    && payload.Targets.Targets.All(item => !item.RawGeodataIncluded);
        var noAbsolute = proof.NoAbsolutePaths;
        var noBinary = proof.NoBinaryOrRasterMedia;
        var noSceneSettings = editor.HasNoScenePrefabSettingsMutationMarkers
                              && scripts.Files.All(item => item.HasNoScenePrefabSettingsMutationMarkers);
        var noExternal = scripts.HasNoExternalDependencyMarkers
                         && scripts.Files.All(item => item.HasNoExternalDependencyMarkers);

        AddIfFalse(context.Goal104Ready, "goal105.quality.goal104", "Goal104", diagnostics);
        AddIfFalse(payload.Manifest.PayloadFileCount == 6, "goal105.quality.payload", "payload", diagnostics);
        AddIfFalse(targets, "goal105.quality.targets", "targets", diagnostics);
        AddIfFalse(actionKinds, "goal105.quality.action_kinds", "actions", diagnostics);
        AddIfFalse(session, "goal105.quality.session", "session", diagnostics);
        AddIfFalse(deltas, "goal105.quality.deltas", "state deltas", diagnostics);
        AddIfFalse(proof.DeterministicStateHashChainPassed, "goal105.quality.hash_chain", "state deltas", diagnostics);
        AddIfFalse(scripts.Passed, "goal105.quality.scripts", "Unity scripts", diagnostics);
        AddIfFalse(editor.Passed, "goal105.quality.editor", "Unity editor helper", diagnostics);
        AddIfFalse(proof.Passed, "goal105.quality.proof", "simulated session proof", diagnostics);
        AddIfFalse(negative.Passed, "goal105.quality.negative", "negative proof", diagnostics);
        AddIfFalse(binding.Passed, "goal105.quality.workspace", "workspace", diagnostics);
        AddIfFalse(lineage.Passed, "goal105.quality.lineage", "source lineage", diagnostics);
        AddIfFalse(alphaUnchanged, "goal105.quality.alpha_bootstrap",
            OfflineGeoworldInteractionPlayableProbeVocabulary.AlphaRuntimeBootstrapPath, diagnostics);
        AddIfFalse(noNetwork, "goal105.quality.network_provider", "payload/scripts", diagnostics);
        AddIfFalse(noRaw, "goal105.quality.raw_geodata", "payload", diagnostics);
        AddIfFalse(noAbsolute, "goal105.quality.absolute_paths", "payload", diagnostics);
        AddIfFalse(noBinary, "goal105.quality.binary_media", "payload", diagnostics);
        AddIfFalse(noSceneSettings, "goal105.quality.scene_settings", "Unity editor helper", diagnostics);
        AddIfFalse(noExternal, "goal105.quality.external_dependency", "Unity scripts", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractionQualityGateScan
        {
            Passed = ordered.All(item => item.Severity != "error"),
            Goal104Consumed = context.Goal104Ready,
            InteractionPayloadCreated = payload.Manifest.PayloadFileCount == 6,
            TargetGraphBuilt = targets,
            ActionGraphBuilt = actionKinds,
            SessionScriptBuilt = session,
            StateDeltaPlanBuilt = deltas,
            StateHashChainPassed = proof.DeterministicStateHashChainPassed,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            UnityScriptInventorySafetyPassed = scripts.Passed,
            SimulatedSessionProofPassed = proof.Passed,
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
            NoExternalDependenciesOrNewInputSystem = noExternal,
            TargetCount = payload.Manifest.TargetCount,
            ActionKindCount = payload.Manifest.ActionKindCount,
            ActionCount = payload.Manifest.ActionCount,
            ScriptedEventCount = payload.Manifest.ScriptedEventCount,
            StateDeltaCount = payload.Manifest.StateDeltaCount,
            ScannedCSharpFileCount = sourceFiles.Count,
            MaxLogicalLineCount = sourceFiles.Count == 0 ? 0 : sourceFiles.Max(item => item.Lines),
            FilesOver700LogicalLinesCount = sourceFiles.Count(item => item.Lines > 700),
            FilesOver1000LogicalLinesCount = sourceFiles.Count(item => item.Lines > 1000),
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldInteractionPlayableProbe/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldInteractionPlayableProbe/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldInteractionPlayableProbeProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs",
                "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs",
                "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs",
                "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105/",
                ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe/",
                "docs/agent-tasks/goal-105-offline-geoworld-interaction-playable-probe/",
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
        OfflineGeoworldInteractionUnityScriptInventory scripts,
        OfflineGeoworldInteractionEditorWindowInventory editor,
        OfflineGeoworldInteractionSimulatedSessionProof proof,
        OfflineGeoworldInteractionNegativeProof negative,
        OfflineGeoworldInteractionWorkspaceBindingInventory binding,
        OfflineGeoworldInteractionSourceLineage lineage,
        OfflineGeoworldInteractionQualityGateScan quality) =>
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName] =
                Serialize(scripts),
            [OfflineGeoworldInteractionPlayableProbeVocabulary.EditorWindowInventoryFileName] =
                Serialize(editor),
            [OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofFileName] =
                Serialize(proof),
            [OfflineGeoworldInteractionPlayableProbeVocabulary.NegativeProofFileName] =
                Serialize(negative),
            [OfflineGeoworldInteractionPlayableProbeVocabulary.WorkspaceBindingInventoryFileName] =
                Serialize(binding),
            [OfflineGeoworldInteractionPlayableProbeVocabulary.SourceLineageFileName] =
                Serialize(lineage),
            [OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName] =
                Serialize(quality)
        };

    private static OfflineGeoworldInteractionReport BuildReport(
        Goal105Payload payload,
        OfflineGeoworldInteractionUnityScriptInventory scripts,
        OfflineGeoworldInteractionEditorWindowInventory editor,
        OfflineGeoworldInteractionSimulatedSessionProof proof,
        OfflineGeoworldInteractionNegativeProof negative,
        OfflineGeoworldInteractionWorkspaceBindingInventory binding,
        OfflineGeoworldInteractionQualityGateScan quality,
        IReadOnlyDictionary<string, string> evidence) =>
        new()
        {
            TargetCount = payload.Manifest.TargetCount,
            ActionKindCount = payload.Manifest.ActionKindCount,
            ActionCount = payload.Manifest.ActionCount,
            ScriptedEventCount = payload.Manifest.ScriptedEventCount,
            StateDeltaCount = payload.Manifest.StateDeltaCount,
            DeterministicStateHashChainPassed = proof.DeterministicStateHashChainPassed,
            UnityScriptsReady = scripts.Passed,
            EditorWindowReady = editor.Passed,
            UnityScriptInventorySafetyPassed = scripts.Passed,
            SimulatedSessionProofPassed = proof.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            AlphaRuntimeBootstrapUnchanged = quality.AlphaRuntimeBootstrapUnchanged,
            QualityGatePassed = quality.Passed,
            FinalStateHash = payload.Manifest.FinalStateHash,
            ManifestHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName]),
            TargetsHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName]),
            ActionsHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName]),
            SessionScriptHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName]),
            StateDeltaPlanHash =
                Hash(payload.PayloadFiles[OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName]),
            UnityScriptInventoryHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName]),
            EditorWindowInventoryHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.EditorWindowInventoryFileName]),
            SimulatedSessionProofHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofFileName]),
            NegativeProofHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.NegativeProofFileName]),
            WorkspaceBindingInventoryHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.WorkspaceBindingInventoryFileName]),
            SourceLineageHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.SourceLineageFileName]),
            QualityGateHash =
                Hash(evidence[OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName])
        };

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        (OfflineGeoworldInteractionPlayableProbeVocabulary.Goal104SourceRoot
         + "/"
         + OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName,
            "Goal104 manifest"),
        (OfflineGeoworldInteractionPlayableProbeVocabulary.Goal104SourceRoot
         + "/"
         + OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName,
            "Goal104 movement path"),
        (OfflineGeoworldInteractionPlayableProbeVocabulary.Goal104SourceRoot
         + "/"
         + OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName,
            "Goal104 visible object index"),
        (OfflineGeoworldInteractionPlayableProbeVocabulary.Goal104SourceRoot
         + "/"
         + OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName,
            "Goal104 simulated execution proof"),
        (OfflineGeoworldInteractionPlayableProbeVocabulary.Goal104SourceRoot
         + "/"
         + OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName,
            "Goal104 Unity script inventory"),
        (OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath,
            "Goal104 Unity interactive controller"),
        (OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath,
            "Goal105 Unity interaction controller"),
        (OfflineGeoworldInteractionPlayableProbeVocabulary.UnityEditorWindowScriptPath,
            "Goal105 Unity editor helper")
    ];

    private static IEnumerable<string> CandidateSourceFiles(string root)
    {
        var paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectory(paths, root, "src/LLMGameCreator.Application/Design/OfflineGeoworldInteractionPlayableProbe");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceService.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewEvidenceWriter.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportBuilder.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewReportRenderer.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWinFormsBindingScanner.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceModels.Goal105.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewOfflineGeoworldInteractionInspector.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldPreviewGoal105Quality.cs");
        AddPath(paths, root, "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewProofStatusLoader.Goal105.cs");
        AddPath(paths, root, "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs");
        AddDirectory(paths, root, "tests/LLMGameCreator.Tests/Application/OfflineGeoworldInteractionPlayableProbe");
        paths.Add(Resolve(root, "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldInteractionPlayableProbeProductSmokeTests.cs"));
        paths.Add(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityTargetScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStateDeltaLogScriptPath));
        paths.Add(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityEditorWindowScriptPath));
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

    private static OfflineGeoworldInteractionSourceLineageRecord SourceLineageRecord(
        string root,
        string relativePath,
        string purpose)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        return new OfflineGeoworldInteractionSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            Purpose = purpose
        };
    }
}
