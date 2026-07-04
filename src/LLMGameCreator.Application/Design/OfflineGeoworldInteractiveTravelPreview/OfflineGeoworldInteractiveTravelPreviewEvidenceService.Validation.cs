using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

public sealed partial class OfflineGeoworldInteractiveTravelPreviewEvidenceService
{
    private static OfflineGeoworldInteractiveSimulatedExecutionProof ValidateMirroredPayload(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        var rootPath = Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(File.Exists(path), "goal104.read.payload_file_missing", fileName, diagnostics);
        }

        return ValidatePayload(payload, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldInteractiveSimulatedExecutionProof ValidatePayload(
        IReadOnlyDictionary<string, string> payload,
        bool payloadReadAttempted,
        List<OfflineGeoworldInteractiveDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldInteractiveTravelPreviewVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestFileName, out var manifestJson);
        payload.TryGetValue(OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsFileName, out var stepsJson);
        payload.TryGetValue(OfflineGeoworldInteractiveTravelPreviewVocabulary.ChunkVisibilityFileName, out var chunkJson);
        payload.TryGetValue(OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexFileName, out var objectJson);
        payload.TryGetValue(OfflineGeoworldInteractiveTravelPreviewVocabulary.ReadmeFileName, out var readmeJson);

        var manifest = Deserialize<OfflineGeoworldInteractiveTravelManifest>(manifestJson ?? string.Empty)
                       ?? new OfflineGeoworldInteractiveTravelManifest();
        var steps = Deserialize<OfflineGeoworldInteractiveTravelStepsDocument>(stepsJson ?? string.Empty)
                    ?? new OfflineGeoworldInteractiveTravelStepsDocument();
        var chunk = Deserialize<OfflineGeoworldInteractiveChunkVisibilityDocument>(chunkJson ?? string.Empty)
                    ?? new OfflineGeoworldInteractiveChunkVisibilityDocument();
        var objects = Deserialize<OfflineGeoworldInteractiveObjectStateIndex>(objectJson ?? string.Empty)
                      ?? new OfflineGeoworldInteractiveObjectStateIndex();

        var hashesMatch = string.Equals(manifest.StepsHash, Hash(stepsJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ChunkVisibilityHash, Hash(chunkJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ObjectStateIndexHash, Hash(objectJson ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                          && string.Equals(manifest.ReadmeHash, Hash(readmeJson ?? string.Empty), StringComparison.OrdinalIgnoreCase);
        var objectIds = objects.Objects.Select(item => item.ObjectId).ToHashSet(StringComparer.Ordinal);
        var samples = steps.MovementSamples.Count > 0 ? steps.MovementSamples : steps.Steps;
        var stepCounts = samples.Count >= 6
                         && steps.StepCount == samples.Count
                         && steps.MovementSampleCount == samples.Count
                         && chunk.StepCount == samples.Count
                         && chunk.Steps.Count == samples.Count
                         && manifest.MovementSampleCount == samples.Count
                         && manifest.ObjectCount == objects.ObjectCount
                         && objects.ObjectCount == objects.Objects.Count;
        var visibleCounts = steps.Steps.All(item =>
            item.ExpectedVisibleObjectCount == item.VisibleObjectIds.Count
            && item.VisibleObjectIds.All(objectIds.Contains)
            && item.HiddenObjectIds.All(objectIds.Contains)
            && item.VisibleObjectIds.Intersect(item.HiddenObjectIds, StringComparer.Ordinal).Any() == false);
        var boundary = samples.All(item => item.BoundaryPrefetchChunkKeys.Count > 0)
                       && chunk.BoundaryCrossingCount >= 2
                       && chunk.BoundaryZones.Count >= 2
                       && samples.Select(item => string.Join(",", item.BoundaryPrefetchChunkKeys))
                           .Distinct(StringComparer.Ordinal)
                           .Count() >= 2;
        var prefetch = objects.PrefetchPlanCount == chunk.BoundaryCrossingCount
                       && objects.Plans.Count == chunk.BoundaryCrossingCount
                       && objects.Plans.All(item => item.PrefetchChunkKeys.Count > 0);
        var diffs = samples.Any(item => item.NewlyVisibleObjectIds.Count > 0)
                    && samples.Any(item => item.NewlyHiddenObjectIds.Count > 0)
                    && chunk.BoundaryZones.All(item =>
                        item.NewlyVisibleObjectIds.All(objectIds.Contains)
                        && item.NewlyHiddenObjectIds.All(objectIds.Contains));
        var chunkCoverage = steps.Steps.All(step =>
        {
            var chunkStep = chunk.Steps.SingleOrDefault(item => item.StepIndex == step.StepIndex);
            return chunkStep is not null
                   && chunkStep.ActiveChunkKeys.SequenceEqual(step.ActiveChunkKeys)
                   && chunkStep.BoundaryPrefetchChunkKeys.SequenceEqual(step.BoundaryPrefetchChunkKeys)
                   && step.ActiveChunkKeys.All(chunkStep.VisibleObjectIdsByChunk.ContainsKey);
        });
        var noUnsupportedStep = samples.All(item => SupportedStepActions.Contains(item.Action));
        var hashChain = ValidateStateHashChain(samples);
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

        AddIfFalse(requiredPresent, "goal104.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.SchemaVersion), "goal104.read.manifest", "manifest", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(steps.SchemaVersion), "goal104.read.steps", "steps", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(chunk.SchemaVersion), "goal104.read.chunk_visibility", "chunk visibility", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(objects.SchemaVersion), "goal104.read.object_index", "object index", diagnostics);
        AddIfFalse(hashesMatch, "goal104.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(stepCounts, "goal104.read.counts", "payload", diagnostics);
        AddIfFalse(visibleCounts, "goal104.read.visible_counts", "steps", diagnostics);
        AddIfFalse(boundary, "goal104.read.boundary_prefetch", "steps", diagnostics);
        AddIfFalse(prefetch, "goal104.read.prefetch_plan", "prefetch plan", diagnostics);
        AddIfFalse(diffs, "goal104.read.visibility_diffs", "movement path", diagnostics);
        AddIfFalse(chunkCoverage, "goal104.read.chunk_coverage", "chunk visibility", diagnostics);
        AddIfFalse(hashChain, "goal104.read.hash_chain", "steps", diagnostics);
        AddIfFalse(noUnsupportedStep, "goal104.read.unsupported_step", "steps", diagnostics);
        AddIfFalse(noAbsolute, "goal104.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal104.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal104.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal104.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractiveSimulatedExecutionProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && stepCounts
                     && visibleCounts
                     && boundary
                     && prefetch
                     && diffs
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
            MovementSampleCountPassed = stepCounts,
            BoundaryCrossingCountPassed = boundary,
            PrefetchPlanCoveragePassed = prefetch,
            ObjectVisibilityDiffsPassed = diffs,
            BoundaryPrefetchProgressionRepresented = boundary,
            DeterministicStateHashChainPassed = hashChain,
            NoUnsupportedStep = noUnsupportedStep,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            StepCount = steps.StepCount,
            MovementSampleCount = samples.Count,
            BoundaryCrossingCount = chunk.BoundaryCrossingCount,
            PrefetchPlanCount = objects.PrefetchPlanCount,
            ObjectCount = objects.ObjectCount,
            ExpectedVisibleObjectCountsByStep = samples
                .OrderBy(item => item.StepIndex)
                .Select(item => item.ExpectedVisibleObjectCount)
                .ToList(),
            StateHashChain = samples
                .OrderBy(item => item.StepIndex)
                .Select(item => item.DeterministicStateHash)
                .ToList(),
            Diagnostics = ordered
        };
    }

    private static bool ValidateStateHashChain(IReadOnlyList<OfflineGeoworldInteractiveTravelStep> steps)
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

    private static OfflineGeoworldInteractiveNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal103_payload", "Goal103 interactive source payload removed",
                "goal104.negative.goal103_missing", "Goal103 payload"),
            Scenario("missing_goal104_manifest", "Goal104 manifest removed",
                "goal104.negative.manifest_missing", "Goal104 manifest"),
            Scenario("movement_path_without_boundary_crossings", "movement path has no crossing samples",
                "goal104.negative.boundary_crossings_missing", "movement path"),
            Scenario("boundary_crossing_without_prefetch_plan", "boundary crossing has no prefetch plan",
                "goal104.negative.prefetch_missing", "prefetch plan"),
            Scenario("object_visibility_diff_references_unknown_object", "visibility diff references an unknown object id",
                "goal104.negative.unknown_object", "visibility diff"),
            Scenario("fake_success_without_reading_files", "proof marked passed without reading payload",
                "goal104.negative.fake_success", "simulated proof"),
            Scenario("absolute_path_in_payload", "absolute local path inserted into payload",
                "goal104.negative.absolute_path", "payload"),
            Scenario("raw_geodata_leak", "raw geodata marker added to interactive plan",
                "goal104.negative.raw_geodata", "payload"),
            Scenario("network_provider_marker_in_unity_scripts", "Unity script contains external marker",
                "goal104.negative.provider_network", "Unity scripts"),
            Scenario("alpha_runtime_bootstrap_dependency_marker", "Unity script references AlphaRuntimeBootstrap",
                "goal104.negative.alpha_bootstrap", "Unity scripts"),
            Scenario("scene_prefab_project_settings_mutation_marker", "Unity editor helper mutates saved scene or settings",
                "goal104.negative.scene_settings", "Unity editor helper"),
            Scenario("binary_raster_media_marker", "binary or raster media added to payload",
                "goal104.negative.binary_media", "payload"),
            Scenario("new_input_system_or_external_dependency_marker", "new input system or external dependency marker added",
                "goal104.negative.external_dependency", "Unity scripts")
        };

        return new OfflineGeoworldInteractiveNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldInteractiveTravelPreviewVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldInteractiveUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        var controller = ReadOptionalText(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath);
        var state = ReadOptionalText(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStateScriptPath);
        var chunk = ReadOptionalText(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityChunkVisibilityScriptPath);
        var combined = controller + Environment.NewLine + state + Environment.NewLine + chunk;
        var controllerExists = File.Exists(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath));
        var stateExists = File.Exists(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStateScriptPath));
        var chunkExists = File.Exists(Resolve(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityChunkVisibilityScriptPath));
        var streaming = controller.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var rootMarker = controller.Contains(
            OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var inspector = controller.Contains("[SerializeField]", StringComparison.Ordinal)
                        && controller.Contains("currentSampleIndex", StringComparison.Ordinal)
                        && controller.Contains("visibleObjectCount", StringComparison.Ordinal)
                        && controller.Contains("lastStatus", StringComparison.Ordinal);
        var manualTimer = controller.Contains("ApplyManualMovement", StringComparison.Ordinal)
                          && controller.Contains("autoAdvance", StringComparison.Ordinal);
        var activates = controller.Contains("SetActive", StringComparison.Ordinal)
                        && controller.Contains("ObjectName", StringComparison.Ordinal);
        var toleratesMissing = controller.Contains("missingObjectCount", StringComparison.Ordinal)
                               && controller.Contains("missing", StringComparison.OrdinalIgnoreCase);
        var motor = state.Contains("OfflineGeoworldPreviewPlayerMotor", StringComparison.Ordinal)
                    && state.Contains("Input.GetKey", StringComparison.Ordinal);
        var prefetchState = chunk.Contains("OfflineGeoworldBoundaryPrefetchState", StringComparison.Ordinal)
                            && chunk.Contains("CurrentBoundaryCrossingId", StringComparison.Ordinal);
        var noBootstrap = !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));

        AddIfFalse(controllerExists, "goal104.script.controller_missing",
            OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath, diagnostics);
        AddIfFalse(stateExists, "goal104.script.state_missing",
            OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStateScriptPath, diagnostics);
        AddIfFalse(chunkExists, "goal104.script.chunk_missing",
            OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityChunkVisibilityScriptPath, diagnostics);
        AddIfFalse(streaming, "goal104.script.streaming_assets", "controller", diagnostics);
        AddIfFalse(rootMarker, "goal104.script.root", "controller", diagnostics);
        AddIfFalse(inspector, "goal104.script.inspector", "controller", diagnostics);
        AddIfFalse(manualTimer, "goal104.script.manual_timer", "controller", diagnostics);
        AddIfFalse(activates, "goal104.script.activate_objects", "controller", diagnostics);
        AddIfFalse(toleratesMissing, "goal104.script.missing_objects", "controller", diagnostics);
        AddIfFalse(motor, "goal104.script.player_motor", "player motor", diagnostics);
        AddIfFalse(prefetchState, "goal104.script.prefetch_state", "prefetch state", diagnostics);
        AddIfFalse(noBootstrap, "goal104.script.bootstrap_dependency", "scripts", diagnostics);
        AddIfFalse(noMarkers, "goal104.script.provider_network", "scripts", diagnostics);

        var files = new[]
        {
            ScriptFile(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityControllerScriptPath),
            ScriptFile(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStateScriptPath),
            ScriptFile(root, OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityChunkVisibilityScriptPath)
        };

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractiveUnityScriptInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            ControllerExists = controllerExists,
            StateExists = stateExists,
            ChunkVisibilityExists = chunkExists,
            ControllerUsesApplicationStreamingAssetsPath = streaming,
            ControllerReadsGoal104Root = rootMarker,
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

    private static OfflineGeoworldInteractiveEditorWindowInventory BuildEditorWindowInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldInteractiveDiagnostic>();
        var relativePath = OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityEditorWindowScriptPath;
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var menu = text.Contains("LLMGameCreator/Offline Geoworld Interactive Travel", StringComparison.Ordinal);
        var streaming = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goal104Root = text.Contains(
            OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var create = text.Contains("CreatePreviewRig", StringComparison.Ordinal);
        var clear = text.Contains("ClearPreviewRig", StringComparison.Ordinal);
        var payloadUi = text.Contains("RefreshPayloadStatus", StringComparison.Ordinal)
                        && text.Contains("movementSampleCount", StringComparison.Ordinal)
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

        AddIfFalse(exists, "goal104.editor.script_missing", relativePath, diagnostics);
        AddIfFalse(menu, "goal104.editor.menu_marker", relativePath, diagnostics);
        AddIfFalse(streaming, "goal104.editor.streaming_assets", relativePath, diagnostics);
        AddIfFalse(goal104Root, "goal104.editor.goal104_root", relativePath, diagnostics);
        AddIfFalse(create, "goal104.editor.create_missing", relativePath, diagnostics);
        AddIfFalse(clear, "goal104.editor.clear_missing", relativePath, diagnostics);
        AddIfFalse(payloadUi, "goal104.editor.payload_status", relativePath, diagnostics);
        AddIfFalse(manualOnly, "goal104.editor.manual_only", relativePath, diagnostics);
        AddIfFalse(noProvider, "goal104.editor.provider_network", relativePath, diagnostics);
        AddIfFalse(noBootstrap, "goal104.editor.alpha_bootstrap_dependency", relativePath, diagnostics);
        AddIfFalse(noSceneSettings, "goal104.editor.scene_settings", relativePath, diagnostics);
        AddIfFalse(noAutoRun, "goal104.editor.auto_run", relativePath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldInteractiveEditorWindowInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            EditorWindowScriptExists = exists,
            MenuItemMarkerPresent = menu,
            StreamingAssetsPathMarkerPresent = streaming,
            Goal104PayloadPathMarkerPresent = goal104Root,
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

    private static OfflineGeoworldInteractiveSourceFile ScriptFile(string root, string relativePath) =>
        SourceFile(root, relativePath);

    private static OfflineGeoworldInteractiveSourceFile SourceFile(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new OfflineGeoworldInteractiveSourceFile
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
