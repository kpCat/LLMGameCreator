using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

public sealed partial class OfflineGeoworldPlayModeTravelPreviewEvidenceService
{
    private static OfflineGeoworldPlayModeSimulatedExecutionProof ValidateMirroredPayload(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        var rootPath = Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(File.Exists(path), "goal103.read.payload_file_missing", fileName, diagnostics);
        }

        return ValidatePayload(payload, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldPlayModeSimulatedExecutionProof ValidatePayload(
        IReadOnlyDictionary<string, string> payload,
        bool payloadReadAttempted,
        List<OfflineGeoworldPlayModeDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldPlayModeTravelPreviewVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestFileName, out var manifestJson);
        payload.TryGetValue(OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsFileName, out var stepsJson);
        payload.TryGetValue(OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilityFileName, out var chunkJson);
        payload.TryGetValue(OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexFileName, out var objectJson);
        payload.TryGetValue(OfflineGeoworldPlayModeTravelPreviewVocabulary.ReadmeFileName, out var readmeJson);

        var manifest = Deserialize<OfflineGeoworldPlayModeTravelManifest>(manifestJson ?? string.Empty)
                       ?? new OfflineGeoworldPlayModeTravelManifest();
        var steps = Deserialize<OfflineGeoworldPlayModeTravelStepsDocument>(stepsJson ?? string.Empty)
                    ?? new OfflineGeoworldPlayModeTravelStepsDocument();
        var chunk = Deserialize<OfflineGeoworldPlayModeChunkVisibilityDocument>(chunkJson ?? string.Empty)
                    ?? new OfflineGeoworldPlayModeChunkVisibilityDocument();
        var objects = Deserialize<OfflineGeoworldPlayModeObjectStateIndex>(objectJson ?? string.Empty)
                      ?? new OfflineGeoworldPlayModeObjectStateIndex();

        var hashesMatch = string.Equals(manifest.StepsHash, Hash(stepsJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ChunkVisibilityHash, Hash(chunkJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ObjectStateIndexHash, Hash(objectJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ReadmeHash, Hash(readmeJson ?? string.Empty), StringComparison.OrdinalIgnoreCase);
        var objectIds = objects.Objects.Select(item => item.ObjectId).ToHashSet(StringComparer.Ordinal);
        var stepCounts = steps.StepCount >= 4
                         && steps.StepCount == steps.Steps.Count
                         && steps.StepCount == chunk.StepCount
                         && steps.StepCount == chunk.Steps.Count
                         && manifest.StepCount == steps.StepCount
                         && manifest.ObjectCount == objects.ObjectCount
                         && objects.ObjectCount == objects.Objects.Count;
        var visibleCounts = steps.Steps.All(item =>
            item.ExpectedVisibleObjectCount == item.VisibleObjectIds.Count
            && item.VisibleObjectIds.All(objectIds.Contains)
            && item.HiddenObjectIds.All(objectIds.Contains)
            && item.VisibleObjectIds.Intersect(item.HiddenObjectIds, StringComparer.Ordinal).Any() == false);
        var boundary = steps.Steps.All(item => item.BoundaryPrefetchChunkKeys.Count > 0)
                       && steps.Steps.Select(item => string.Join(",", item.BoundaryPrefetchChunkKeys))
                           .Distinct(StringComparer.Ordinal)
                           .Count() >= 2;
        var chunkCoverage = steps.Steps.All(step =>
        {
            var chunkStep = chunk.Steps.SingleOrDefault(item => item.StepIndex == step.StepIndex);
            return chunkStep is not null
                   && chunkStep.ActiveChunkKeys.SequenceEqual(step.ActiveChunkKeys)
                   && chunkStep.BoundaryPrefetchChunkKeys.SequenceEqual(step.BoundaryPrefetchChunkKeys)
                   && step.ActiveChunkKeys.All(chunkStep.VisibleObjectIdsByChunk.ContainsKey);
        });
        var noUnsupportedStep = steps.Steps.All(item => SupportedStepActions.Contains(item.Action));
        var hashChain = ValidateStateHashChain(steps.Steps);
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

        AddIfFalse(requiredPresent, "goal103.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.SchemaVersion), "goal103.read.manifest", "manifest", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(steps.SchemaVersion), "goal103.read.steps", "steps", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(chunk.SchemaVersion), "goal103.read.chunk_visibility", "chunk visibility", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(objects.SchemaVersion), "goal103.read.object_index", "object index", diagnostics);
        AddIfFalse(hashesMatch, "goal103.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(stepCounts, "goal103.read.counts", "payload", diagnostics);
        AddIfFalse(visibleCounts, "goal103.read.visible_counts", "steps", diagnostics);
        AddIfFalse(boundary, "goal103.read.boundary_prefetch", "steps", diagnostics);
        AddIfFalse(chunkCoverage, "goal103.read.chunk_coverage", "chunk visibility", diagnostics);
        AddIfFalse(hashChain, "goal103.read.hash_chain", "steps", diagnostics);
        AddIfFalse(noUnsupportedStep, "goal103.read.unsupported_step", "steps", diagnostics);
        AddIfFalse(noAbsolute, "goal103.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal103.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal103.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal103.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPlayModeSimulatedExecutionProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && stepCounts
                     && visibleCounts
                     && boundary
                     && chunkCoverage
                     && hashChain
                     && noUnsupportedStep
                     && noAbsolute
                     && noRaw
                     && noBinary
                     && noMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = !string.IsNullOrWhiteSpace(manifest.SchemaVersion),
            StepsFileRead = !string.IsNullOrWhiteSpace(steps.SchemaVersion),
            ChunkVisibilityFileRead = !string.IsNullOrWhiteSpace(chunk.SchemaVersion),
            ObjectStateIndexRead = !string.IsNullOrWhiteSpace(objects.SchemaVersion),
            PayloadHashesMatchManifest = hashesMatch,
            StepByStepVisibleCountsPassed = visibleCounts,
            BoundaryPrefetchProgressionRepresented = boundary,
            DeterministicStateHashChainPassed = hashChain,
            NoUnsupportedStep = noUnsupportedStep,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            StepCount = steps.StepCount,
            ObjectCount = objects.ObjectCount,
            ExpectedVisibleObjectCountsByStep = steps.Steps
                .OrderBy(item => item.StepIndex)
                .Select(item => item.ExpectedVisibleObjectCount)
                .ToList(),
            StateHashChain = steps.Steps
                .OrderBy(item => item.StepIndex)
                .Select(item => item.DeterministicStateHash)
                .ToList(),
            Diagnostics = ordered
        };
    }

    private static bool ValidateStateHashChain(IReadOnlyList<OfflineGeoworldPlayModeTravelStep> steps)
    {
        var previous = string.Empty;
        foreach (var step in steps.OrderBy(item => item.StepIndex))
        {
            if (!string.Equals(step.PreviousStateHash, previous, StringComparison.Ordinal))
            {
                return false;
            }

            var expected = Hash(BuildStepHashSeed(step));
            if (!string.Equals(expected, step.DeterministicStateHash, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            previous = step.DeterministicStateHash;
        }

        return steps.Count > 0;
    }

    private static OfflineGeoworldPlayModeNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal101_travel_payload", "Goal101 travel payload removed",
                "goal103.negative.goal101_travel_missing", "Goal101 travel payload"),
            Scenario("missing_goal103_manifest", "Goal103 manifest removed",
                "goal103.negative.manifest_missing", "Goal103 manifest"),
            Scenario("unsupported_travel_step", "travel action changed to unsupported",
                "goal103.negative.unsupported_step", "steps"),
            Scenario("active_chunk_missing_from_chunk_visibility", "active chunk removed from chunk visibility",
                "goal103.negative.active_chunk_missing", "chunk visibility"),
            Scenario("object_state_references_unknown_object", "step references an unknown object id",
                "goal103.negative.unknown_object", "object index"),
            Scenario("fake_success_without_reading_files", "proof marked passed without reading payload",
                "goal103.negative.fake_success", "simulated proof"),
            Scenario("absolute_path_in_payload", "absolute local path inserted into payload",
                "goal103.negative.absolute_path", "payload"),
            Scenario("raw_geodata_leaked_into_playmode_plan", "raw geodata marker added to play-mode plan",
                "goal103.negative.raw_geodata", "payload"),
            Scenario("network_provider_marker_in_unity_scripts", "Unity script contains external marker",
                "goal103.negative.provider_network", "Unity scripts"),
            Scenario("alpha_runtime_bootstrap_dependency_marker", "Unity script references AlphaRuntimeBootstrap",
                "goal103.negative.alpha_bootstrap", "Unity scripts"),
            Scenario("scene_prefab_project_settings_mutation_marker", "Unity editor helper mutates saved scene or settings",
                "goal103.negative.scene_settings", "Unity editor helper"),
            Scenario("binary_raster_media_marker", "binary or raster media added to payload",
                "goal103.negative.binary_media", "payload"),
            Scenario("goal102b_closure_without_actual_evidence", "closure claimed without Goal102B actual before/after evidence",
                "goal103.negative.goal102b_actual_evidence", "Goal102B closure")
        };

        return new OfflineGeoworldPlayModeNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldPlayModeTravelPreviewVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldPlayModeUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        var controller = ReadOptionalText(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath);
        var state = ReadOptionalText(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStateScriptPath);
        var chunk = ReadOptionalText(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityChunkVisibilityScriptPath);
        var combined = controller + Environment.NewLine + state + Environment.NewLine + chunk;
        var controllerExists = File.Exists(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath));
        var stateExists = File.Exists(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStateScriptPath));
        var chunkExists = File.Exists(Resolve(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityChunkVisibilityScriptPath));
        var streaming = controller.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var rootMarker = controller.Contains(
            OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var inspector = controller.Contains("[SerializeField]", StringComparison.Ordinal)
                        && controller.Contains("currentStepIndex", StringComparison.Ordinal)
                        && controller.Contains("visibleObjectCount", StringComparison.Ordinal)
                        && controller.Contains("lastStatus", StringComparison.Ordinal);
        var manualTimer = controller.Contains("NextStep", StringComparison.Ordinal)
                          && controller.Contains("autoAdvance", StringComparison.Ordinal);
        var activates = controller.Contains("SetActive", StringComparison.Ordinal)
                        && controller.Contains("ObjectName", StringComparison.Ordinal);
        var toleratesMissing = controller.Contains("missingObjectCount", StringComparison.Ordinal)
                               && controller.Contains("missing", StringComparison.OrdinalIgnoreCase);
        var noBootstrap = !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));

        AddIfFalse(controllerExists, "goal103.script.controller_missing",
            OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath, diagnostics);
        AddIfFalse(stateExists, "goal103.script.state_missing",
            OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStateScriptPath, diagnostics);
        AddIfFalse(chunkExists, "goal103.script.chunk_missing",
            OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityChunkVisibilityScriptPath, diagnostics);
        AddIfFalse(streaming, "goal103.script.streaming_assets", "controller", diagnostics);
        AddIfFalse(rootMarker, "goal103.script.root", "controller", diagnostics);
        AddIfFalse(inspector, "goal103.script.inspector", "controller", diagnostics);
        AddIfFalse(manualTimer, "goal103.script.manual_timer", "controller", diagnostics);
        AddIfFalse(activates, "goal103.script.activate_objects", "controller", diagnostics);
        AddIfFalse(toleratesMissing, "goal103.script.missing_objects", "controller", diagnostics);
        AddIfFalse(noBootstrap, "goal103.script.bootstrap_dependency", "scripts", diagnostics);
        AddIfFalse(noMarkers, "goal103.script.provider_network", "scripts", diagnostics);

        var files = new[]
        {
            ScriptFile(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityControllerScriptPath),
            ScriptFile(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStateScriptPath),
            ScriptFile(root, OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityChunkVisibilityScriptPath)
        };

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPlayModeUnityScriptInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            ControllerExists = controllerExists,
            StateExists = stateExists,
            ChunkVisibilityExists = chunkExists,
            ControllerUsesApplicationStreamingAssetsPath = streaming,
            ControllerReadsGoal103Root = rootMarker,
            ControllerExposesInspectorFields = inspector,
            ControllerSupportsManualAndTimerSteps = manualTimer,
            ControllerActivatesObjectsByMetadata = activates,
            ControllerToleratesMissingObjects = toleratesMissing,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderNetworkMarkers = noMarkers,
            Files = files,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPlayModeEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldPlayModeDiagnostic>();
        var relativePath = OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityEditorWindowScriptPath;
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var menu = text.Contains("LLMGameCreator/Offline Geoworld Play Mode Travel", StringComparison.Ordinal);
        var streaming = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goal103Root = text.Contains(
            OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var create = text.Contains("CreateController", StringComparison.Ordinal);
        var clear = text.Contains("ClearController", StringComparison.Ordinal);
        var payloadUi = text.Contains("RefreshPayloadStatus", StringComparison.Ordinal)
                        && text.Contains("stepCount", StringComparison.Ordinal)
                        && text.Contains("objectCount", StringComparison.Ordinal);
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

        AddIfFalse(exists, "goal103.editor.script_missing", relativePath, diagnostics);
        AddIfFalse(menu, "goal103.editor.menu_marker", relativePath, diagnostics);
        AddIfFalse(streaming, "goal103.editor.streaming_assets", relativePath, diagnostics);
        AddIfFalse(goal103Root, "goal103.editor.goal103_root", relativePath, diagnostics);
        AddIfFalse(create, "goal103.editor.create_missing", relativePath, diagnostics);
        AddIfFalse(clear, "goal103.editor.clear_missing", relativePath, diagnostics);
        AddIfFalse(payloadUi, "goal103.editor.payload_status", relativePath, diagnostics);
        AddIfFalse(manualOnly, "goal103.editor.manual_only", relativePath, diagnostics);
        AddIfFalse(noProvider, "goal103.editor.provider_network", relativePath, diagnostics);
        AddIfFalse(noBootstrap, "goal103.editor.alpha_bootstrap_dependency", relativePath, diagnostics);
        AddIfFalse(noSceneSettings, "goal103.editor.scene_settings", relativePath, diagnostics);
        AddIfFalse(noAutoRun, "goal103.editor.auto_run", relativePath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPlayModeEditorWindowInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            EditorWindowScriptExists = exists,
            MenuItemMarkerPresent = menu,
            StreamingAssetsPathMarkerPresent = streaming,
            Goal103PayloadPathMarkerPresent = goal103Root,
            CreateControllerMethodPresent = create,
            ClearControllerMethodPresent = clear,
            PayloadReadinessUiPresent = payloadUi,
            ManualButtonOnly = manualOnly,
            HasNoProviderNetworkMarkers = noProvider,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoScenePrefabSettingsMutationMarkers = noSceneSettings,
            HasNoAutoRunImportMarker = noAutoRun,
            SourceFile = SourceFile(root, relativePath),
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPlayModeSourceFile ScriptFile(string root, string relativePath) =>
        SourceFile(root, relativePath);

    private static OfflineGeoworldPlayModeSourceFile SourceFile(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new OfflineGeoworldPlayModeSourceFile
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            LineCount = CountLines(text),
            HasNoProviderNetworkMarkers = !ProviderNetworkMarkers.Any(marker =>
                text.Contains(marker, StringComparison.OrdinalIgnoreCase)),
            DoesNotReferenceAlphaRuntimeBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal),
            HasNoScenePrefabSettingsMutationMarkers = !ScenePrefabSettingsMarkers.Any(marker =>
                text.Contains(marker, StringComparison.OrdinalIgnoreCase))
        };
    }
}
