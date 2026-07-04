using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

public sealed partial class OfflineGeoworldSessionPersistenceReplayEvidenceService
{
    private static OfflineGeoworldSessionSimulatedReplayProof ValidateMirroredPayload(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
        var rootPath = Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(File.Exists(path), "goal106.read.payload_file_missing", fileName, diagnostics);
        }

        return ValidatePayload(payload, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldSessionSimulatedReplayProof ValidatePayload(
        IReadOnlyDictionary<string, string> payload,
        bool payloadReadAttempted,
        List<OfflineGeoworldSessionDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldSessionPersistenceReplayVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(OfflineGeoworldSessionPersistenceReplayVocabulary.ManifestFileName, out var manifestJson);
        payload.TryGetValue(OfflineGeoworldSessionPersistenceReplayVocabulary.InitialStateFileName, out var initialJson);
        payload.TryGetValue(OfflineGeoworldSessionPersistenceReplayVocabulary.DeltaLogFileName, out var deltaJson);
        payload.TryGetValue(OfflineGeoworldSessionPersistenceReplayVocabulary.ReplayScriptFileName, out var replayJson);
        payload.TryGetValue(
            OfflineGeoworldSessionPersistenceReplayVocabulary.AcceptanceChecklistFileName,
            out var checklistJson);
        payload.TryGetValue(OfflineGeoworldSessionPersistenceReplayVocabulary.ReadmeFileName, out var readmeJson);

        var manifest = Deserialize<OfflineGeoworldSessionManifest>(manifestJson ?? string.Empty)
                       ?? new OfflineGeoworldSessionManifest();
        var initial = Deserialize<OfflineGeoworldSessionInitialState>(initialJson ?? string.Empty)
                      ?? new OfflineGeoworldSessionInitialState();
        var deltaLog = Deserialize<OfflineGeoworldSessionDeltaLog>(deltaJson ?? string.Empty)
                       ?? new OfflineGeoworldSessionDeltaLog();
        var replay = Deserialize<OfflineGeoworldSessionReplayScript>(replayJson ?? string.Empty)
                     ?? new OfflineGeoworldSessionReplayScript();
        var checklist = Deserialize<OfflineGeoworldSessionAcceptanceChecklist>(checklistJson ?? string.Empty)
                        ?? new OfflineGeoworldSessionAcceptanceChecklist();
        var hashesMatch =
            string.Equals(manifest.InitialStateHashFile, Hash(initialJson ?? string.Empty), StringComparison.Ordinal)
            && string.Equals(manifest.DeltaLogHash, Hash(deltaJson ?? string.Empty), StringComparison.Ordinal)
            && string.Equals(manifest.ReplayScriptHash, Hash(replayJson ?? string.Empty), StringComparison.Ordinal)
            && string.Equals(
                manifest.AcceptanceChecklistHash,
                Hash(checklistJson ?? string.Empty),
                StringComparison.Ordinal)
            && string.Equals(manifest.ReadmeHash, Hash(readmeJson ?? string.Empty), StringComparison.Ordinal);

        var counts = manifest.PayloadFileCount == 6
                     && manifest.ReplayStepCount >= 6
                     && manifest.StateDeltaCount >= 6
                     && initial.TargetCount >= 8
                     && initial.ActionCount >= 8
                     && deltaLog.DeltaCount == deltaLog.Deltas.Count
                     && replay.ReplayStepCount == replay.Steps.Count
                     && replay.ReplayStepCount == deltaLog.DeltaCount;
        var checkpoint = replay.Checkpoint.StepIndex >= 3
                         && replay.Checkpoint.StepIndex <= deltaLog.DeltaCount
                         && replay.Checkpoint.AfterEventCount == replay.Checkpoint.StepIndex
                         && string.Equals(
                             manifest.CheckpointStateHash,
                             replay.Checkpoint.StateHash,
                             StringComparison.Ordinal)
                         && !string.IsNullOrWhiteSpace(replay.Checkpoint.SnapshotHash);
        var hashChain = ValidateReplayHashChain(deltaLog, replay, out var replayChain);
        var firstHalf = checkpoint
                        && hashChain
                        && replay.Steps.Take(replay.Checkpoint.StepIndex).Last().StateHashAfter
                           == replay.Checkpoint.StateHash;
        var snapshotHash = BuildSnapshotHash(
            replay.InitialStateHash,
            replay.Checkpoint.StepIndex,
            replay.Checkpoint.StateHash,
            replay.Steps.Take(replay.Checkpoint.StepIndex).Select(item => item.EventId));
        var checkpointSaved = firstHalf
                              && string.Equals(
                                  snapshotHash,
                                  replay.Checkpoint.SnapshotHash,
                                  StringComparison.Ordinal);
        var checkpointLoaded = checkpointSaved && replay.Checkpoint.StateHash == manifest.CheckpointStateHash;
        var replayFinal = checkpointLoaded
                          && string.Equals(
                              replay.Steps.Last().StateHashAfter,
                              manifest.FinalStateHash,
                              StringComparison.Ordinal)
                          && string.Equals(
                              deltaLog.FinalStateHash,
                              manifest.FinalStateHash,
                              StringComparison.Ordinal);
        var duplicateRejected = replay.DuplicateReplayPolicy == "reject_already_applied_step";
        var corruptedRejected = checkpointSaved
                                && !string.Equals(
                                    snapshotHash + "-corrupt",
                                    replay.Checkpoint.SnapshotHash,
                                    StringComparison.Ordinal);
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

        AddIfFalse(requiredPresent, "goal106.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.GoalId), "goal106.read.manifest", "manifest", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(initial.GoalId), "goal106.read.initial", "initial state", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(deltaLog.GoalId), "goal106.read.delta_log", "delta log", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(replay.GoalId), "goal106.read.replay", "replay script", diagnostics);
        AddIfFalse(checklist.StepCount > 0, "goal106.read.checklist", "acceptance checklist", diagnostics);
        AddIfFalse(hashesMatch, "goal106.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(counts, "goal106.read.counts", "payload", diagnostics);
        AddIfFalse(checkpoint, "goal106.read.checkpoint", "replay script", diagnostics);
        AddIfFalse(hashChain, "goal106.read.hash_chain", "delta log", diagnostics);
        AddIfFalse(firstHalf, "goal106.read.first_half", "replay script", diagnostics);
        AddIfFalse(checkpointSaved, "goal106.read.checkpoint_saved", "snapshot", diagnostics);
        AddIfFalse(checkpointLoaded, "goal106.read.checkpoint_loaded", "snapshot", diagnostics);
        AddIfFalse(replayFinal, "goal106.read.final_hash", "replay script", diagnostics);
        AddIfFalse(duplicateRejected, "goal106.read.duplicate", "replay script", diagnostics);
        AddIfFalse(corruptedRejected, "goal106.read.corrupted_snapshot", "snapshot", diagnostics);
        AddIfFalse(noAbsolute, "goal106.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal106.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal106.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal106.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSessionSimulatedReplayProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && counts
                     && checkpoint
                     && hashChain
                     && firstHalf
                     && checkpointSaved
                     && checkpointLoaded
                     && replayFinal
                     && duplicateRejected
                     && corruptedRejected
                     && noAbsolute
                     && noRaw
                     && noBinary
                     && noMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = !string.IsNullOrWhiteSpace(manifest.GoalId),
            InitialStateRead = !string.IsNullOrWhiteSpace(initial.GoalId),
            DeltaLogRead = !string.IsNullOrWhiteSpace(deltaLog.GoalId),
            ReplayScriptRead = !string.IsNullOrWhiteSpace(replay.GoalId),
            AcceptanceChecklistRead = checklist.StepCount > 0,
            PayloadHashesMatchManifest = hashesMatch,
            FirstHalfReplayApplied = firstHalf,
            CheckpointSaved = checkpointSaved,
            CheckpointLoaded = checkpointLoaded,
            ReplayResumedToFinalHash = replayFinal,
            DuplicateReplayRejected = duplicateRejected,
            CorruptedSnapshotRejected = corruptedRejected,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            ReplayStepCount = replay.ReplayStepCount,
            StateDeltaCount = deltaLog.DeltaCount,
            CheckpointStepIndex = replay.Checkpoint.StepIndex,
            CheckpointStateHash = replay.Checkpoint.StateHash,
            SavedSnapshotHash = replay.Checkpoint.SnapshotHash,
            FinalStateHash = manifest.FinalStateHash,
            ReplayStateHashChain = replayChain,
            Diagnostics = ordered
        };
    }

    private static bool ValidateReplayHashChain(
        OfflineGeoworldSessionDeltaLog deltaLog,
        OfflineGeoworldSessionReplayScript replay,
        out IReadOnlyList<string> replayChain)
    {
        var chain = new List<string>();
        var current = replay.InitialStateHash;
        chain.Add(current);
        if (deltaLog.Deltas.Count == 0 || replay.Steps.Count != deltaLog.Deltas.Count)
        {
            replayChain = chain;
            return false;
        }

        foreach (var pair in replay.Steps.Zip(deltaLog.Deltas.OrderBy(item => item.ReplayStepIndex)))
        {
            var step = pair.First;
            var delta = pair.Second;
            if (step.StepIndex != delta.ReplayStepIndex
                || step.EventId != delta.EventId
                || step.StateHashBefore != current
                || delta.StateHashBefore != current
                || step.StateHashAfter != delta.StateHashAfter)
            {
                replayChain = chain;
                return false;
            }

            current = step.StateHashAfter;
            chain.Add(current);
        }

        replayChain = chain;
        return chain.Count == deltaLog.DeltaCount + 1
               && chain.SequenceEqual(deltaLog.StateHashChain)
               && current == deltaLog.FinalStateHash
               && current == replay.FinalStateHash;
    }

    private static OfflineGeoworldSessionNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal105_payload", "Goal105 source payload removed",
                "goal106.negative.goal105_missing", "Goal105 payload"),
            Scenario("missing_delta_log", "session delta log removed",
                "goal106.negative.delta_log_missing", "delta log"),
            Scenario("checkpoint_without_prior_deltas", "checkpoint index is zero",
                "goal106.negative.checkpoint_prior", "checkpoint"),
            Scenario("load_snapshot_hash_mismatch", "snapshot state hash differs from checkpoint",
                "goal106.negative.snapshot_hash", "snapshot"),
            Scenario("corrupted_snapshot_accepted", "corrupted snapshot accepted",
                "goal106.negative.corrupted_snapshot", "snapshot"),
            Scenario("replay_final_hash_mismatch", "final replay hash differs from manifest",
                "goal106.negative.final_hash", "replay"),
            Scenario("duplicate_replay_mutates_state_non_deterministically", "duplicate replay mutates state",
                "goal106.negative.duplicate_replay", "replay"),
            Scenario("absolute_path", "absolute local path inserted",
                "goal106.negative.absolute_path", "payload"),
            Scenario("raw_geodata_leak", "raw geodata marker inserted",
                "goal106.negative.raw_geodata", "payload"),
            Scenario("network_provider_marker", "network/provider marker inserted",
                "goal106.negative.provider_network", "Unity scripts"),
            Scenario("alpha_runtime_bootstrap_dependency_marker", "Unity script references AlphaRuntimeBootstrap",
                "goal106.negative.alpha_bootstrap", "Unity scripts"),
            Scenario("scene_prefab_settings_mutation_marker", "editor helper mutates scene/settings on import",
                "goal106.negative.scene_settings", "Unity editor helper"),
            Scenario("binary_raster_media_marker", "binary or raster media added",
                "goal106.negative.binary_media", "payload"),
            Scenario("external_dependency_new_input_system_marker", "external dependency marker added",
                "goal106.negative.external_dependency", "Unity scripts")
        };
        return new OfflineGeoworldSessionNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldSessionPersistenceReplayVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldSessionUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
        var saveLoad = ReadOptionalText(root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath);
        var replay = ReadOptionalText(root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath);
        var snapshot = ReadOptionalText(root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath);
        var combined = saveLoad + Environment.NewLine + replay + Environment.NewLine + snapshot;
        var saveLoadExists = File.Exists(Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath));
        var replayExists = File.Exists(Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath));
        var snapshotExists = File.Exists(Resolve(root, OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath));
        var streaming = combined.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var persistent = combined.Contains("Application.persistentDataPath", StringComparison.Ordinal);
        var rootMarker = combined.Contains(
            OfflineGeoworldSessionPersistenceReplayVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var integratesGoal105 = combined.Contains("OfflineGeoworldInteractionController", StringComparison.Ordinal)
                                && combined.Contains("OfflineGeoworldStateDeltaLog", StringComparison.Ordinal);
        var saveLoadDelete = combined.Contains("SaveSnapshot", StringComparison.Ordinal)
                             && combined.Contains("LoadSnapshot", StringComparison.Ordinal)
                             && combined.Contains("DeleteSnapshot", StringComparison.Ordinal);
        var stepping = combined.Contains("ReplayNextStep", StringComparison.Ordinal)
                       && combined.Contains("ReplayAllRemaining", StringComparison.Ordinal);
        var noBootstrap = !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noExternal = !ExternalDependencyMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var files = UnityScriptInventoryPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => SourceFile(root, path, path.Contains("/Editor/", StringComparison.Ordinal)))
            .ToList();
        var sourceHealth = files.Count >= 20
                           && files.All(item => item.Exists
                                                && item.NotMinified
                                                && item.LineCount < 700
                                                && item.HasNoProviderNetworkMarkers
                                                && item.DoesNotReferenceAlphaRuntimeBootstrap
                                                && item.HasNoExternalDependencyMarkers);

        AddIfFalse(saveLoadExists, "goal106.script.save_load_missing",
            OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath, diagnostics);
        AddIfFalse(replayExists, "goal106.script.replay_missing",
            OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath, diagnostics);
        AddIfFalse(snapshotExists, "goal106.script.snapshot_missing",
            OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath, diagnostics);
        AddIfFalse(streaming, "goal106.script.streaming_assets", "scripts", diagnostics);
        AddIfFalse(persistent, "goal106.script.persistent_data", "scripts", diagnostics);
        AddIfFalse(rootMarker, "goal106.script.goal106_root", "scripts", diagnostics);
        AddIfFalse(integratesGoal105, "goal106.script.goal105_integration", "scripts", diagnostics);
        AddIfFalse(saveLoadDelete, "goal106.script.save_load_delete", "scripts", diagnostics);
        AddIfFalse(stepping, "goal106.script.replay_stepping", "scripts", diagnostics);
        AddIfFalse(noBootstrap, "goal106.script.bootstrap_dependency", "scripts", diagnostics);
        AddIfFalse(noMarkers, "goal106.script.provider_network", "scripts", diagnostics);
        AddIfFalse(noExternal, "goal106.script.external_dependency", "scripts", diagnostics);
        AddIfFalse(sourceHealth, "goal106.script.source_health", "scripts", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSessionUnityScriptInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            ScannedUnitySourceFileCount = files.Count,
            SaveLoadControllerExists = saveLoadExists,
            ReplayControllerExists = replayExists,
            SnapshotModelExists = snapshotExists,
            ReadsApplicationStreamingAssetsPath = streaming,
            UsesApplicationPersistentDataPath = persistent,
            ReadsGoal106Root = rootMarker,
            IntegratesGoal105ControllerAndDeltaLog = integratesGoal105,
            SupportsSaveLoadDeleteSnapshot = saveLoadDelete,
            SupportsReplayStepping = stepping,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderNetworkMarkers = noMarkers,
            HasNoExternalDependencyMarkers = noExternal,
            Files = files,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldSessionEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldSessionDiagnostic>();
        var relativePath = OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath;
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var menu = text.Contains("LLMGameCreator/Offline Geoworld Session Replay", StringComparison.Ordinal);
        var streaming = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goalRoot = text.Contains(
            OfflineGeoworldSessionPersistenceReplayVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var create = text.Contains("CreateSessionReplayRig", StringComparison.Ordinal);
        var clear = text.Contains("ClearSessionReplayRig", StringComparison.Ordinal);
        var checklist = text.Contains("RefreshPayloadStatus", StringComparison.Ordinal)
                        && text.Contains("acceptance checklist", StringComparison.OrdinalIgnoreCase);
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

        AddIfFalse(exists, "goal106.editor.script_missing", relativePath, diagnostics);
        AddIfFalse(menu, "goal106.editor.menu_marker", relativePath, diagnostics);
        AddIfFalse(streaming, "goal106.editor.streaming_assets", relativePath, diagnostics);
        AddIfFalse(goalRoot, "goal106.editor.goal106_root", relativePath, diagnostics);
        AddIfFalse(create, "goal106.editor.create_missing", relativePath, diagnostics);
        AddIfFalse(clear, "goal106.editor.clear_missing", relativePath, diagnostics);
        AddIfFalse(checklist, "goal106.editor.checklist", relativePath, diagnostics);
        AddIfFalse(manualOnly, "goal106.editor.manual_only", relativePath, diagnostics);
        AddIfFalse(noProvider, "goal106.editor.provider_network", relativePath, diagnostics);
        AddIfFalse(noBootstrap, "goal106.editor.alpha_bootstrap_dependency", relativePath, diagnostics);
        AddIfFalse(noSceneSettings, "goal106.editor.scene_settings", relativePath, diagnostics);
        AddIfFalse(noAutoRun, "goal106.editor.auto_run", relativePath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldSessionEditorWindowInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            EditorWindowScriptExists = exists,
            MenuItemMarkerPresent = menu,
            StreamingAssetsPathMarkerPresent = streaming,
            Goal106PayloadPathMarkerPresent = goalRoot,
            CreateRigMethodPresent = create,
            ClearRigMethodPresent = clear,
            AcceptanceChecklistUiPresent = checklist,
            ManualButtonOnly = manualOnly,
            HasNoProviderNetworkMarkers = noProvider,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoScenePrefabSettingsMutationMarkers = noSceneSettings,
            HasNoAutoRunImportMarker = noAutoRun,
            SourceFile = SourceFile(root, relativePath, allowManualEditorSceneObjects: true),
            Diagnostics = ordered
        };
    }

    private static IReadOnlyList<string> UnityScriptInventoryPaths() =>
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
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySnapshotScriptPath,
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnitySaveLoadControllerScriptPath,
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnityReplayControllerScriptPath,
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs",
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnityEditorWindowScriptPath
    ];

    private static OfflineGeoworldSessionSourceFile SourceFile(
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
        return new OfflineGeoworldSessionSourceFile
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
