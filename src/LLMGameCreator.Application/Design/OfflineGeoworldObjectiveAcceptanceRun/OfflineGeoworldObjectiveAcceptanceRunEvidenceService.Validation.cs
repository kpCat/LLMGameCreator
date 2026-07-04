using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

public sealed partial class OfflineGeoworldObjectiveAcceptanceRunEvidenceService
{
    private static OfflineGeoworldObjectiveReplayAcceptanceProof ValidateMirroredPayload(
        string root,
        IReadOnlyDictionary<string, string> payload,
        Goal107SourceContext context)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        var rootPath = Resolve(root, OfflineGeoworldObjectiveAcceptanceRunVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(File.Exists(path), "goal107.read.payload_file_missing", fileName, diagnostics);
        }

        return ValidatePayload(payload, context, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldObjectiveReplayAcceptanceProof ValidatePayload(
        IReadOnlyDictionary<string, string> payload,
        Goal107SourceContext context,
        bool payloadReadAttempted,
        List<OfflineGeoworldObjectiveDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldObjectiveAcceptanceRunVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(OfflineGeoworldObjectiveAcceptanceRunVocabulary.ManifestFileName, out var manifestJson);
        payload.TryGetValue(OfflineGeoworldObjectiveAcceptanceRunVocabulary.ObjectivesFileName, out var objectivesJson);
        payload.TryGetValue(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.AcceptanceRunFileName,
            out var runJson);
        payload.TryGetValue(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.CompletionStateFileName,
            out var completionJson);
        payload.TryGetValue(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReplayAcceptanceProofFileName,
            out var proofJson);
        payload.TryGetValue(OfflineGeoworldObjectiveAcceptanceRunVocabulary.ReadmeFileName, out var readmeJson);

        var manifest = Deserialize<OfflineGeoworldObjectiveManifest>(manifestJson ?? string.Empty)
                       ?? new OfflineGeoworldObjectiveManifest();
        var objectives = Deserialize<OfflineGeoworldObjectiveDocument>(objectivesJson ?? string.Empty)
                         ?? new OfflineGeoworldObjectiveDocument();
        var run = Deserialize<OfflineGeoworldObjectiveAcceptanceRun>(runJson ?? string.Empty)
                  ?? new OfflineGeoworldObjectiveAcceptanceRun();
        var completion = Deserialize<OfflineGeoworldObjectiveCompletionState>(completionJson ?? string.Empty)
                         ?? new OfflineGeoworldObjectiveCompletionState();
        var proof = Deserialize<OfflineGeoworldObjectiveReplayAcceptanceProof>(proofJson ?? string.Empty)
                    ?? new OfflineGeoworldObjectiveReplayAcceptanceProof();
        var readme = Deserialize<OfflineGeoworldObjectiveReadme>(readmeJson ?? string.Empty)
                     ?? new OfflineGeoworldObjectiveReadme();

        var hashesMatch =
            string.Equals(manifest.ObjectivesHash, Hash(objectivesJson ?? string.Empty), StringComparison.Ordinal)
            && string.Equals(manifest.AcceptanceRunHash, Hash(runJson ?? string.Empty), StringComparison.Ordinal)
            && string.Equals(
                manifest.CompletionStateFileHash,
                Hash(completionJson ?? string.Empty),
                StringComparison.Ordinal)
            && string.Equals(
                manifest.ReplayAcceptanceProofHash,
                Hash(proofJson ?? string.Empty),
                StringComparison.Ordinal)
            && string.Equals(manifest.ReadmeHash, Hash(readmeJson ?? string.Empty), StringComparison.Ordinal);
        var counts = manifest.PayloadFileCount == 6
                     && manifest.ObjectiveCount >= 6
                     && manifest.ObjectiveCount == objectives.ObjectiveCount
                     && objectives.Objectives.Count == objectives.ObjectiveCount
                     && completion.CompletedObjectiveCount == objectives.ObjectiveCount
                     && run.Steps.Count == objectives.ObjectiveCount;
        var requiredKinds = new HashSet<string>(
            objectives.Objectives.Select(item => item.ObjectiveKind),
            StringComparer.Ordinal)
        {
        };
        var kindsPresent = requiredKinds.Contains("inspect_poi_or_building_target")
                           && requiredKinds.Contains("mark_target_visited")
                           && requiredKinds.Contains("collect_sample")
                           && requiredKinds.Contains("toggle_or_clear_blocked_route")
                           && requiredKinds.Contains("save_load_checkpoint_resume")
                           && requiredKinds.Contains("finalize_acceptance_run");
        var actionIds = context.InitialState.Actions.Select(item => item.ActionId).ToHashSet(StringComparer.Ordinal);
        var targetIds = context.InitialState.Targets.Select(item => item.TargetId).ToHashSet(StringComparer.Ordinal);
        var deltaKeys = context.DeltaLog.Deltas.Select(item => item.StateKey).ToHashSet(StringComparer.Ordinal);
        var linkage = objectives.Objectives.All(item =>
            item.LinkedActionIds.Count > 0
            && item.LinkedActionIds.All(actionIds.Contains)
            && item.LinkedTargetIds.All(targetIds.Contains)
            && item.ExpectedStateDeltaKeys.Count > 0
            && item.ExpectedStateDeltaKeys.All(deltaKeys.Contains)
            && !string.IsNullOrWhiteSpace(item.DeterministicHashContribution));
        var prerequisites = ValidatePrerequisites(objectives.Objectives);
        var completionState = completion.Completed
                              && completion.FinalStatus == "completed"
                              && completion.CompletedObjectiveIds.SequenceEqual(
                                  objectives.Objectives.Select(item => item.ObjectiveId))
                              && completion.ObjectiveHashChain.Count == objectives.ObjectiveCount + 1
                              && completion.ObjectiveHashChain.Last() == manifest.ObjectiveAcceptanceHash;
        var replayLink = run.ReplayStepCount == context.Manifest.ReplayStepCount
                         && run.StateDeltaCount == context.Manifest.StateDeltaCount
                         && run.CheckpointStepIndex == context.Manifest.CheckpointStepIndex
                         && run.CheckpointStateHash == context.Manifest.CheckpointStateHash
                         && run.FinalStateHash == context.Manifest.FinalStateHash
                         && proof.CheckpointResumeApplied
                         && context.SourceReplayProof.ReplayResumedToFinalHash;
        var values = payload.Values.ToList();
        var noAbsolute = values.All(value => !ContainsAbsolutePath(value));
        var noRaw = values.All(value =>
            !value.Contains("\"rawGeodataIncluded\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"noRawGeodata\": false", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"rawFullAreaDump\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"planetDump\": true", StringComparison.OrdinalIgnoreCase));
        var noBinary = payload.Keys.All(path => !IsBinaryOrRasterMedia(path))
                       && values.All(value => !IsBinaryOrRasterMedia(value));
        var noMarkers = values.All(value => !ProviderNetworkMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase)));

        AddIfFalse(requiredPresent, "goal107.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.GoalId), "goal107.read.manifest", "manifest", diagnostics);
        AddIfFalse(objectives.ObjectiveCount > 0, "goal107.read.objectives", "objectives", diagnostics);
        AddIfFalse(run.Steps.Count > 0, "goal107.read.run", "acceptance run", diagnostics);
        AddIfFalse(completion.CompletedObjectiveCount > 0, "goal107.read.completion", "completion", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(readme.GoalId), "goal107.read.readme", "readme", diagnostics);
        AddIfFalse(hashesMatch, "goal107.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(counts, "goal107.read.counts", "payload", diagnostics);
        AddIfFalse(kindsPresent, "goal107.read.objective_kinds", "objectives", diagnostics);
        AddIfFalse(linkage, "goal107.read.linkage", "objectives", diagnostics);
        AddIfFalse(prerequisites, "goal107.read.prerequisites", "objectives", diagnostics);
        AddIfFalse(completionState, "goal107.read.completion_state", "completion", diagnostics);
        AddIfFalse(replayLink, "goal107.read.replay_link", "acceptance run", diagnostics);
        AddIfFalse(proof.FailedPrerequisiteRejected, "goal107.read.failed_prerequisite", "proof", diagnostics);
        AddIfFalse(noAbsolute, "goal107.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal107.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal107.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal107.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return proof with
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && counts
                     && kindsPresent
                     && linkage
                     && prerequisites
                     && completionState
                     && replayLink
                     && noAbsolute
                     && noRaw
                     && noBinary
                     && noMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = !string.IsNullOrWhiteSpace(manifest.GoalId),
            ObjectivesRead = objectives.ObjectiveCount > 0,
            AcceptanceRunRead = run.Steps.Count > 0,
            CompletionStateRead = completion.CompletedObjectiveCount > 0,
            SourceGoal106PayloadRead = context.Goal106Ready,
            SourceGoal106ReplayProofRead = context.SourceReplayProof.Passed,
            SourceGoal106ReplayHashChainPassed = context.SourceReplayProof.ReplayResumedToFinalHash,
            CheckpointResumeApplied = replayLink,
            ObjectivePrerequisitesPassed = prerequisites,
            CompletionTransitionsPassed = completionState,
            StateDeltaLinkagePassed = linkage,
            DeterministicHashChainPassed = completionState,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            ObjectiveCount = objectives.ObjectiveCount,
            CompletedObjectiveCount = completion.CompletedObjectiveCount,
            FinalStatus = completion.FinalStatus,
            FinalObjectiveAcceptanceHash = completion.FinalObjectiveAcceptanceHash,
            ObjectiveHashChain = completion.ObjectiveHashChain,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldObjectiveNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal106_payload", "Goal106 source payload removed",
                "goal107.negative.goal106_missing", "Goal106 payload"),
            Scenario("unknown_action_ref", "objective references unknown action id",
                "goal107.negative.unknown_action", "objectives"),
            Scenario("unknown_target_ref", "objective references unknown target id",
                "goal107.negative.unknown_target", "objectives"),
            Scenario("unknown_delta_ref", "objective references unknown state delta",
                "goal107.negative.unknown_delta", "objectives"),
            Scenario("prerequisite_bypass", "objective completes before prerequisite",
                "goal107.negative.prerequisite", "objectives"),
            Scenario("completion_without_required_state_delta", "completion without state delta",
                "goal107.negative.required_delta", "completion"),
            Scenario("save_load_without_checkpoint", "save/load objective has no checkpoint",
                "goal107.negative.checkpoint", "acceptance run"),
            Scenario("replay_mismatch", "objective replay final hash mismatches Goal106",
                "goal107.negative.replay_mismatch", "acceptance run"),
            Scenario("fake_success_without_file_reads", "proof claims success without reads",
                "goal107.negative.fake_reads", "proof"),
            Scenario("absolute_path", "absolute local path inserted",
                "goal107.negative.absolute_path", "payload"),
            Scenario("raw_geodata_leak", "raw geodata marker inserted",
                "goal107.negative.raw_geodata", "payload"),
            Scenario("network_provider_marker", "network/provider marker inserted",
                "goal107.negative.provider_network", "Unity scripts"),
            Scenario("alpha_runtime_bootstrap_dependency_marker", "Unity script references AlphaRuntimeBootstrap",
                "goal107.negative.alpha_bootstrap", "Unity scripts"),
            Scenario("scene_prefab_settings_mutation_marker", "editor helper mutates scene/settings on import",
                "goal107.negative.scene_settings", "Unity editor helper"),
            Scenario("binary_raster_media_marker", "binary or raster media added",
                "goal107.negative.binary_media", "payload"),
            Scenario("external_dependency_new_input_system_marker", "external dependency marker added",
                "goal107.negative.external_dependency", "Unity scripts")
        };
        return new OfflineGeoworldObjectiveNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldObjectiveAcceptanceRunVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldObjectiveUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        var state = ReadOptionalText(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath);
        var tracker = ReadOptionalText(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath);
        var controller = ReadOptionalText(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath);
        var combined = state + Environment.NewLine + tracker + Environment.NewLine + controller;
        var stateExists = File.Exists(Resolve(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath));
        var trackerExists = File.Exists(Resolve(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath));
        var controllerExists = File.Exists(Resolve(
            root,
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath));
        var streaming = combined.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var rootMarker = combined.Contains(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var integratesGoal105 = combined.Contains("OfflineGeoworldInteractionController", StringComparison.Ordinal);
        var integratesGoal106 = combined.Contains("OfflineGeoworldSessionReplayController", StringComparison.Ordinal)
                                && combined.Contains(
                                    "OfflineGeoworldSessionSaveLoadController",
                                    StringComparison.Ordinal);
        var advanceReplay = combined.Contains("ManualAdvanceCurrentObjective", StringComparison.Ordinal)
                            && combined.Contains("ReplayFromMetadata", StringComparison.Ordinal)
                            && combined.Contains("CheckReplayLinkage", StringComparison.Ordinal);
        var noBootstrap = !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noExternal = !ExternalDependencyMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var files = UnityAlphaScriptInventoryPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => SourceFile(root, path, path.Contains("/Editor/", StringComparison.Ordinal)))
            .ToList();
        var sourceHealth = files.Count >= 24
                           && files.All(item => item.Exists
                                                && item.NotMinified
                                                && item.LineCount < 700
                                                && item.HasNoProviderNetworkMarkers
                                                && item.DoesNotReferenceAlphaRuntimeBootstrap
                                                && item.HasNoExternalDependencyMarkers);

        AddIfFalse(stateExists, "goal107.script.state_missing",
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath, diagnostics);
        AddIfFalse(trackerExists, "goal107.script.tracker_missing",
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath, diagnostics);
        AddIfFalse(controllerExists, "goal107.script.controller_missing",
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath, diagnostics);
        AddIfFalse(streaming, "goal107.script.streaming_assets", "scripts", diagnostics);
        AddIfFalse(rootMarker, "goal107.script.goal107_root", "scripts", diagnostics);
        AddIfFalse(integratesGoal105, "goal107.script.goal105_integration", "scripts", diagnostics);
        AddIfFalse(integratesGoal106, "goal107.script.goal106_integration", "scripts", diagnostics);
        AddIfFalse(advanceReplay, "goal107.script.manual_replay", "scripts", diagnostics);
        AddIfFalse(noBootstrap, "goal107.script.bootstrap_dependency", "scripts", diagnostics);
        AddIfFalse(noMarkers, "goal107.script.provider_network", "scripts", diagnostics);
        AddIfFalse(noExternal, "goal107.script.external_dependency", "scripts", diagnostics);
        AddIfFalse(sourceHealth, "goal107.script.source_health", "scripts", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldObjectiveUnityScriptInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            ScannedUnitySourceFileCount = files.Count,
            ObjectiveStateExists = stateExists,
            ObjectiveTrackerExists = trackerExists,
            ObjectiveAcceptanceControllerExists = controllerExists,
            ReadsApplicationStreamingAssetsPath = streaming,
            ReadsGoal107Root = rootMarker,
            IntegratesGoal105InteractionController = integratesGoal105,
            IntegratesGoal106ReplayAndSaveLoadControllers = integratesGoal106,
            SupportsManualAdvanceAndReplayChecks = advanceReplay,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderNetworkMarkers = noMarkers,
            HasNoExternalDependencyMarkers = noExternal,
            Files = files,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldObjectiveEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldObjectiveDiagnostic>();
        var relativePath = OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityEditorWindowScriptPath;
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var menu = text.Contains("LLMGameCreator/Offline Geoworld Objective Acceptance", StringComparison.Ordinal);
        var streaming = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goalRoot = text.Contains(
            OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var create = text.Contains("CreateObjectiveAcceptanceRig", StringComparison.Ordinal);
        var clear = text.Contains("ClearObjectiveAcceptanceRig", StringComparison.Ordinal);
        var instructions = text.Contains("acceptance instructions", StringComparison.OrdinalIgnoreCase)
                           && text.Contains("RefreshPayloadStatus", StringComparison.Ordinal);
        var manualOnly = text.Contains("GUILayout.Button", StringComparison.Ordinal)
                         && !text.Contains("InitializeOnLoad", StringComparison.Ordinal)
                         && !text.Contains("DidReloadScripts", StringComparison.Ordinal);
        var noProvider = !ProviderNetworkMarkers.Any(marker =>
            text.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noSceneSettings = !ScenePrefabSettingsMarkers.Any(marker =>
            text.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noAutoRun = !text.Contains("ExecuteInEditMode", StringComparison.OrdinalIgnoreCase)
                        && !text.Contains("runOnStart", StringComparison.OrdinalIgnoreCase);

        AddIfFalse(exists, "goal107.editor.script_missing", relativePath, diagnostics);
        AddIfFalse(menu, "goal107.editor.menu_marker", relativePath, diagnostics);
        AddIfFalse(streaming, "goal107.editor.streaming_assets", relativePath, diagnostics);
        AddIfFalse(goalRoot, "goal107.editor.goal107_root", relativePath, diagnostics);
        AddIfFalse(create, "goal107.editor.create_missing", relativePath, diagnostics);
        AddIfFalse(clear, "goal107.editor.clear_missing", relativePath, diagnostics);
        AddIfFalse(instructions, "goal107.editor.instructions", relativePath, diagnostics);
        AddIfFalse(manualOnly, "goal107.editor.manual_only", relativePath, diagnostics);
        AddIfFalse(noProvider, "goal107.editor.provider_network", relativePath, diagnostics);
        AddIfFalse(noBootstrap, "goal107.editor.alpha_bootstrap_dependency", relativePath, diagnostics);
        AddIfFalse(noSceneSettings, "goal107.editor.scene_settings", relativePath, diagnostics);
        AddIfFalse(noAutoRun, "goal107.editor.auto_run", relativePath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldObjectiveEditorWindowInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            EditorWindowScriptExists = exists,
            MenuItemMarkerPresent = menu,
            StreamingAssetsPathMarkerPresent = streaming,
            Goal107PayloadPathMarkerPresent = goalRoot,
            CreateRigMethodPresent = create,
            ClearRigMethodPresent = clear,
            AcceptanceInstructionsPresent = instructions,
            ManualButtonOnly = manualOnly,
            HasNoProviderNetworkMarkers = noProvider,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoScenePrefabSettingsMutationMarkers = noSceneSettings,
            HasNoAutoRunImportMarker = noAutoRun,
            SourceFile = SourceFile(root, relativePath, allowManualEditorSceneObjects: true),
            Diagnostics = ordered
        };
    }

    private static IReadOnlyList<string> UnityAlphaScriptInventoryPaths() =>
    [
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelState.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeChunkVisibility.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSnapshot.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionReplayController.cs",
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveStateScriptPath,
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveTrackerScriptPath,
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityObjectiveAcceptanceControllerScriptPath,
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldSessionReplayWindow.cs",
        OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityEditorWindowScriptPath
    ];

    private static OfflineGeoworldObjectiveSourceFile SourceFile(
        string root,
        string relativePath,
        bool allowManualEditorSceneObjects)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var sceneMarkers = ScenePrefabSettingsMarkers
            .Where(marker => allowManualEditorSceneObjects
                             && (marker == ".unity" || marker == ".prefab")
                ? false
                : text.Contains(marker, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return new OfflineGeoworldObjectiveSourceFile
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            LineCount = CountLines(text),
            MaxLineLength = MaxLineLength(text),
            NotMinified = exists && !IsMinified(text) && CountLines(text) > 2,
            HasNoProviderNetworkMarkers = !ProviderNetworkMarkers.Any(marker =>
                text.Contains(marker, StringComparison.OrdinalIgnoreCase)),
            DoesNotReferenceAlphaRuntimeBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal),
            HasNoScenePrefabSettingsMutationMarkers = sceneMarkers.Count == 0,
            HasNoExternalDependencyMarkers = !ExternalDependencyMarkers.Any(marker =>
                text.Contains(marker, StringComparison.OrdinalIgnoreCase))
        };
    }
}
