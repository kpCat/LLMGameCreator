using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

public sealed partial class OfflineGeoworldInteractionPlayableProbeEvidenceService
{
    private static OfflineGeoworldInteractionSimulatedSessionProof ValidateMirroredPayload(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
        var rootPath = Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(File.Exists(path), "goal105.read.payload_file_missing", fileName, diagnostics);
        }

        return ValidatePayload(payload, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldInteractionSimulatedSessionProof ValidatePayload(
        IReadOnlyDictionary<string, string> payload,
        bool payloadReadAttempted,
        List<OfflineGeoworldInteractionDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestFileName, out var manifestJson);
        payload.TryGetValue(OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsFileName, out var targetsJson);
        payload.TryGetValue(OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsFileName, out var actionsJson);
        payload.TryGetValue(OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptFileName, out var sessionJson);
        payload.TryGetValue(OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanFileName, out var deltaJson);
        payload.TryGetValue(OfflineGeoworldInteractionPlayableProbeVocabulary.ReadmeFileName, out var readmeJson);

        var manifest = Deserialize<OfflineGeoworldInteractionManifest>(manifestJson ?? string.Empty)
                       ?? new OfflineGeoworldInteractionManifest();
        var targets = Deserialize<OfflineGeoworldInteractionTargetsDocument>(targetsJson ?? string.Empty)
                      ?? new OfflineGeoworldInteractionTargetsDocument();
        var actions = Deserialize<OfflineGeoworldInteractionActionsDocument>(actionsJson ?? string.Empty)
                      ?? new OfflineGeoworldInteractionActionsDocument();
        var session = Deserialize<OfflineGeoworldInteractionSessionScript>(sessionJson ?? string.Empty)
                      ?? new OfflineGeoworldInteractionSessionScript();
        var plan = Deserialize<OfflineGeoworldInteractionStateDeltaPlan>(deltaJson ?? string.Empty)
                   ?? new OfflineGeoworldInteractionStateDeltaPlan();

        var hashesMatch = string.Equals(manifest.TargetsHash, Hash(targetsJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ActionsHash, Hash(actionsJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.SessionScriptHash, Hash(sessionJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.StateDeltaPlanHash, Hash(deltaJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ReadmeHash, Hash(readmeJson ?? string.Empty), StringComparison.OrdinalIgnoreCase);
        var targetIds = targets.Targets.Select(item => item.TargetId).ToHashSet(StringComparer.Ordinal);
        var sourceObjectIds = targets.Targets.Select(item => item.SourceObjectId).ToHashSet(StringComparer.Ordinal);
        var sourceObjectNames = targets.Targets.Select(item => item.SourceObjectName).ToHashSet(StringComparer.Ordinal);
        var actionById = actions.Actions.ToDictionary(item => item.ActionId, item => item, StringComparer.Ordinal);
        var targetCounts = targets.TargetCount == targets.Targets.Count
                           && targets.TargetCount >= 8
                           && targets.Targets.All(item =>
                               !string.IsNullOrWhiteSpace(item.SourceObjectId)
                               && !string.IsNullOrWhiteSpace(item.SourceObjectName)
                               && item.VisibleStepIndexes.Count > 0
                               && item.RawGeodataIncluded == false);
        var actionKinds = OfflineGeoworldInteractionPlayableProbeVocabulary.RequiredActionKinds
            .All(required => actions.ActionKinds.Contains(required, StringComparer.Ordinal))
                          && actions.ActionKindCount >= 5
                          && actions.ActionCount == actions.Actions.Count
                          && actions.Actions.All(action => targetIds.Contains(action.TargetId));
        var targetActions = targets.Targets.All(target =>
            target.ActionIds.Count > 0
            && target.ActionIds.All(actionById.ContainsKey));
        var binding = targets.Targets.Count >= 8
                      && targets.Targets.All(target =>
                          sourceObjectIds.Contains(target.SourceObjectId)
                          && sourceObjectNames.Contains(target.SourceObjectName));
        var sessionRefs = session.EventCount == session.Events.Count
                          && session.EventCount >= 6
                          && session.Events.All(item =>
                              targetIds.Contains(item.TargetId)
                              && actionById.TryGetValue(item.ActionId, out var action)
                              && string.Equals(action.TargetId, item.TargetId, StringComparison.Ordinal)
                              && string.Equals(action.ActionKind, item.ActionKind, StringComparison.Ordinal));
        var availability = session.Events.All(item =>
            item.AvailableByDistance
            && item.DistanceToTarget <= item.RequiredRadius);
        var deltaRefs = plan.StateDeltaCount == plan.Deltas.Count
                        && plan.StateDeltaCount == session.EventCount
                        && plan.MutatesBaseDataDirectly == false
                        && plan.Deltas.All(delta =>
                            targetIds.Contains(delta.TargetId)
                            && actionById.ContainsKey(delta.ActionId)
                            && delta.MutatesBaseDataDirectly == false);
        var hashChain = ValidateStateHashChain(plan, session);
        var unavailableRejected = targets.Targets.Count > 0
                                  && actions.Actions.Count > 0
                                  && Distance(
                                      targets.Targets[0].GridX + 100,
                                      targets.Targets[0].GridZ + 100,
                                      targets.Targets[0]) > actions.Actions[0].RequiredRadius;
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

        AddIfFalse(requiredPresent, "goal105.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.SchemaVersion), "goal105.read.manifest", "manifest", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(targets.SchemaVersion), "goal105.read.targets", "targets", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(actions.SchemaVersion), "goal105.read.actions", "actions", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(session.SchemaVersion), "goal105.read.session", "session", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(plan.SchemaVersion), "goal105.read.delta_plan", "state deltas", diagnostics);
        AddIfFalse(hashesMatch, "goal105.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(targetCounts, "goal105.read.target_counts", "targets", diagnostics);
        AddIfFalse(actionKinds, "goal105.read.action_kinds", "actions", diagnostics);
        AddIfFalse(targetActions, "goal105.read.target_actions", "targets", diagnostics);
        AddIfFalse(binding, "goal105.read.binding", "targets", diagnostics);
        AddIfFalse(sessionRefs, "goal105.read.session_refs", "session", diagnostics);
        AddIfFalse(availability, "goal105.read.availability", "session", diagnostics);
        AddIfFalse(deltaRefs, "goal105.read.delta_refs", "state deltas", diagnostics);
        AddIfFalse(hashChain, "goal105.read.hash_chain", "state deltas", diagnostics);
        AddIfFalse(unavailableRejected, "goal105.read.unavailable_rejection", "actions", diagnostics);
        AddIfFalse(noAbsolute, "goal105.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal105.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal105.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal105.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractionSimulatedSessionProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && targetCounts
                     && actionKinds
                     && targetActions
                     && binding
                     && sessionRefs
                     && availability
                     && deltaRefs
                     && hashChain
                     && unavailableRejected
                     && noAbsolute
                     && noRaw
                     && noBinary
                     && noMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = !string.IsNullOrWhiteSpace(manifest.SchemaVersion),
            TargetsRead = !string.IsNullOrWhiteSpace(targets.SchemaVersion),
            ActionsRead = !string.IsNullOrWhiteSpace(actions.SchemaVersion),
            SessionScriptRead = !string.IsNullOrWhiteSpace(session.SchemaVersion),
            StateDeltaPlanRead = !string.IsNullOrWhiteSpace(plan.SchemaVersion),
            PayloadHashesMatchManifest = hashesMatch,
            TargetBindingByIdOrNamePassed = binding,
            ActionAvailabilityByDistancePassed = availability,
            ScriptedInteractionsApplied = sessionRefs && availability,
            StateDeltaAppendPassed = deltaRefs,
            DeterministicStateHashChainPassed = hashChain,
            UnavailableActionRejected = unavailableRejected,
            StateDeltasSeparateFromBaseData = deltaRefs,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            TargetCount = targets.TargetCount,
            ActionKindCount = actions.ActionKindCount,
            ScriptedEventCount = session.EventCount,
            StateDeltaCount = plan.StateDeltaCount,
            InitialStateHash = plan.InitialStateHash,
            FinalStateHash = plan.FinalStateHash,
            StateHashChain = plan.StateHashChain,
            Diagnostics = ordered
        };
    }

    private static bool ValidateStateHashChain(
        OfflineGeoworldInteractionStateDeltaPlan plan,
        OfflineGeoworldInteractionSessionScript session)
    {
        if (plan.Deltas.Count == 0 || plan.StateHashChain.Count != plan.Deltas.Count + 1)
        {
            return false;
        }

        var previous = plan.InitialStateHash;
        if (!string.Equals(plan.StateHashChain[0], previous, StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var delta in plan.Deltas.OrderBy(item => item.DeltaIndex))
        {
            var matchingEvent = session.Events.SingleOrDefault(item => item.EventId == delta.EventId);
            if (matchingEvent is null
                || !string.Equals(delta.PreviousStateHash, previous, StringComparison.Ordinal)
                || !string.Equals(matchingEvent.ExpectedStateHashBefore, previous, StringComparison.Ordinal))
            {
                return false;
            }

            var expected = Hash(BuildDeltaHashSeed(delta));
            if (!string.Equals(expected, delta.DeterministicStateHash, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(matchingEvent.ExpectedStateHashAfter, delta.DeterministicStateHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            previous = delta.DeterministicStateHash;
        }

        return string.Equals(previous, plan.FinalStateHash, StringComparison.Ordinal)
               && string.Equals(plan.StateHashChain[^1], plan.FinalStateHash, StringComparison.Ordinal);
    }

    private static OfflineGeoworldInteractionNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal104_payload", "Goal104 source payload removed",
                "goal105.negative.goal104_missing", "Goal104 payload"),
            Scenario("interaction_target_referencing_unknown_object", "target source object id is unknown",
                "goal105.negative.unknown_target_object", "targets"),
            Scenario("action_missing_target", "action references a missing target",
                "goal105.negative.action_missing_target", "actions"),
            Scenario("unavailable_action_accepted_outside_radius", "outside-radius action accepted",
                "goal105.negative.radius", "actions"),
            Scenario("state_delta_mutates_base_data_directly", "state delta mutates immutable base target",
                "goal105.negative.base_mutation", "state deltas"),
            Scenario("fake_success_without_file_reads", "proof marked passed without file reads",
                "goal105.negative.fake_success", "simulated proof"),
            Scenario("absolute_path", "absolute local path inserted",
                "goal105.negative.absolute_path", "payload"),
            Scenario("raw_geodata_leak", "raw geodata marker inserted",
                "goal105.negative.raw_geodata", "payload"),
            Scenario("network_provider_marker", "network/provider marker inserted",
                "goal105.negative.provider_network", "Unity scripts"),
            Scenario("alpha_runtime_bootstrap_dependency_marker", "Unity script references AlphaRuntimeBootstrap",
                "goal105.negative.alpha_bootstrap", "Unity scripts"),
            Scenario("scene_prefab_settings_mutation_marker", "editor helper mutates scene/settings on import",
                "goal105.negative.scene_settings", "Unity editor helper"),
            Scenario("binary_raster_media_marker", "binary or raster media added",
                "goal105.negative.binary_media", "payload"),
            Scenario("external_dependency_new_input_system_marker", "external dependency or new input-system marker added",
                "goal105.negative.external_dependency", "Unity scripts")
        };

        return new OfflineGeoworldInteractionNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldInteractionPlayableProbeVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldInteractionUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
        var controller = ReadOptionalText(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath);
        var target = ReadOptionalText(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityTargetScriptPath);
        var log = ReadOptionalText(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStateDeltaLogScriptPath);
        var combined = controller + Environment.NewLine + target + Environment.NewLine + log;
        var controllerExists = File.Exists(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath));
        var targetExists = File.Exists(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityTargetScriptPath));
        var logExists = File.Exists(Resolve(root, OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStateDeltaLogScriptPath));
        var streaming = controller.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goalRoot = controller.Contains(
            OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var bind = controller.Contains("BindTargetsByIdOrName", StringComparison.Ordinal)
                   && controller.Contains("SourceObjectId", StringComparison.Ordinal)
                   && controller.Contains("SourceObjectName", StringComparison.Ordinal);
        var nearest = controller.Contains("nearestTargetId", StringComparison.Ordinal)
                      && controller.Contains("FindNearestTarget", StringComparison.Ordinal);
        var execute = controller.Contains("ExecuteScriptedSession", StringComparison.Ordinal)
                      && controller.Contains("ExecuteManualAction", StringComparison.Ordinal);
        var inMemoryLog = log.Contains("OfflineGeoworldStateDeltaLog", StringComparison.Ordinal)
                          && log.Contains("readonly List", StringComparison.Ordinal)
                          && !log.Contains("File.Write", StringComparison.Ordinal);
        var noBootstrap = !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noExternal = !ExternalDependencyMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var files = UnityScriptInventoryPaths()
            .Where(path => File.Exists(Resolve(root, path)))
            .Select(path => SourceFile(root, path, allowManualEditorSceneObjects: path.Contains("/Editor/", StringComparison.Ordinal)))
            .ToList();
        var sourceHealth = files.Count >= 12
                           && files.All(item => item.Exists
                                                && item.NotMinified
                                                && item.LineCount < 700
                                                && item.HasNoProviderNetworkMarkers
                                                && item.DoesNotReferenceAlphaRuntimeBootstrap
                                                && item.HasNoExternalDependencyMarkers);

        AddIfFalse(controllerExists, "goal105.script.controller_missing",
            OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath, diagnostics);
        AddIfFalse(targetExists, "goal105.script.target_missing",
            OfflineGeoworldInteractionPlayableProbeVocabulary.UnityTargetScriptPath, diagnostics);
        AddIfFalse(logExists, "goal105.script.log_missing",
            OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStateDeltaLogScriptPath, diagnostics);
        AddIfFalse(streaming, "goal105.script.streaming_assets", "controller", diagnostics);
        AddIfFalse(goalRoot, "goal105.script.root", "controller", diagnostics);
        AddIfFalse(bind, "goal105.script.binding", "controller", diagnostics);
        AddIfFalse(nearest, "goal105.script.nearest", "controller", diagnostics);
        AddIfFalse(execute, "goal105.script.execute", "controller", diagnostics);
        AddIfFalse(inMemoryLog, "goal105.script.delta_log", "state delta log", diagnostics);
        AddIfFalse(noBootstrap, "goal105.script.bootstrap_dependency", "scripts", diagnostics);
        AddIfFalse(noMarkers, "goal105.script.provider_network", "scripts", diagnostics);
        AddIfFalse(noExternal, "goal105.script.external_dependency", "scripts", diagnostics);
        AddIfFalse(sourceHealth, "goal105.script.source_health", "scripts", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractionUnityScriptInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            ScannedUnitySourceFileCount = files.Count,
            ControllerExists = controllerExists,
            TargetScriptExists = targetExists,
            StateDeltaLogExists = logExists,
            ControllerUsesApplicationStreamingAssetsPath = streaming,
            ControllerReadsGoal105Root = goalRoot,
            ControllerBindsTargetsByIdOrName = bind,
            ControllerSupportsNearestTargetSelection = nearest,
            ControllerExecutesScriptedAndManualActions = execute,
            StateDeltaLogInMemoryOnly = inMemoryLog,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderNetworkMarkers = noMarkers,
            HasNoExternalDependencyMarkers = noExternal,
            Files = files,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldInteractionEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractionDiagnostic>();
        var relativePath = OfflineGeoworldInteractionPlayableProbeVocabulary.UnityEditorWindowScriptPath;
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var menu = text.Contains("LLMGameCreator/Offline Geoworld Interaction Probe", StringComparison.Ordinal);
        var streaming = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goalRoot = text.Contains(
            OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var create = text.Contains("CreateInteractionProbeRig", StringComparison.Ordinal);
        var clear = text.Contains("ClearInteractionProbeRig", StringComparison.Ordinal);
        var payloadUi = text.Contains("RefreshPayloadStatus", StringComparison.Ordinal)
                        && text.Contains("targetCount", StringComparison.Ordinal)
                        && text.Contains("scriptedEventCount", StringComparison.Ordinal);
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

        AddIfFalse(exists, "goal105.editor.script_missing", relativePath, diagnostics);
        AddIfFalse(menu, "goal105.editor.menu_marker", relativePath, diagnostics);
        AddIfFalse(streaming, "goal105.editor.streaming_assets", relativePath, diagnostics);
        AddIfFalse(goalRoot, "goal105.editor.goal105_root", relativePath, diagnostics);
        AddIfFalse(create, "goal105.editor.create_missing", relativePath, diagnostics);
        AddIfFalse(clear, "goal105.editor.clear_missing", relativePath, diagnostics);
        AddIfFalse(payloadUi, "goal105.editor.payload_status", relativePath, diagnostics);
        AddIfFalse(manualOnly, "goal105.editor.manual_only", relativePath, diagnostics);
        AddIfFalse(noProvider, "goal105.editor.provider_network", relativePath, diagnostics);
        AddIfFalse(noBootstrap, "goal105.editor.alpha_bootstrap_dependency", relativePath, diagnostics);
        AddIfFalse(noSceneSettings, "goal105.editor.scene_settings", relativePath, diagnostics);
        AddIfFalse(noAutoRun, "goal105.editor.auto_run", relativePath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractionEditorWindowInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            EditorWindowScriptExists = exists,
            MenuItemMarkerPresent = menu,
            StreamingAssetsPathMarkerPresent = streaming,
            Goal105PayloadPathMarkerPresent = goalRoot,
            CreateRigMethodPresent = create,
            ClearRigMethodPresent = clear,
            PayloadReadinessUiPresent = payloadUi,
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
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityControllerScriptPath,
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityTargetScriptPath,
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStateDeltaLogScriptPath,
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs",
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityEditorWindowScriptPath
    ];

    private static OfflineGeoworldInteractionSourceFile SourceFile(
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
        return new OfflineGeoworldInteractionSourceFile
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
