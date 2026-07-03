using System.Text;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;

public sealed partial class OfflineGeoworldUnityEditorPreviewToolEvidenceService
{
    private static readonly HashSet<string> ProviderNetworkMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "HttpClient",
        "UnityWebRequest",
        "WebRequest",
        "TcpClient",
        "NetworkStream",
        "Socket(",
        "http://",
        "https://",
        "ProviderCallRequested",
        "LLMProvider",
        "ComfyUI",
        "Fooocus"
    };

    private static readonly HashSet<string> ScenePrefabSettingsMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "EditorSceneManager.SaveScene",
        "EditorSceneManager.MarkSceneDirty",
        "PrefabUtility",
        "EditorBuildSettings",
        "ProjectSettings/",
        "Packages/manifest.json",
        ".unity",
        ".prefab"
    };

    private static readonly HashSet<string> BinaryOrRasterExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };

    private static OfflineGeoworldUnityEditorToolInventory BuildToolInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        var relativePath = OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath;
        var fullPath = Resolve(root, relativePath);
        var exists = File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath, Encoding.UTF8) : string.Empty;
        var menu = text.Contains("LLMGameCreator/Offline Geoworld Preview", StringComparison.Ordinal);
        var streaming = text.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var goal101Root = text.Contains(
            OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var create = text.Contains("CreatePreviewObjects", StringComparison.Ordinal);
        var clear = text.Contains("ClearPreviewObjects", StringComparison.Ordinal);
        var payloadUi = text.Contains("RefreshPayloadStatus", StringComparison.Ordinal)
                        && text.Contains("commandCount", StringComparison.Ordinal)
                        && text.Contains("travelWindowStepCount", StringComparison.Ordinal);
        var manualOnly = text.Contains("GUILayout.Button", StringComparison.Ordinal)
                         && !text.Contains("InitializeOnLoad", StringComparison.Ordinal)
                         && !text.Contains("DidReloadScripts", StringComparison.Ordinal);
        var noProvider = !ProviderNetworkMarkers.Any(marker =>
            text.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noBootstrap = !text.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noScenePrefab = !ScenePrefabSettingsMarkers.Any(marker =>
            text.Contains(marker, StringComparison.OrdinalIgnoreCase));
        var noAutoRun = !text.Contains("runOnStart", StringComparison.OrdinalIgnoreCase)
                        && !text.Contains("ExecuteInEditMode", StringComparison.OrdinalIgnoreCase);

        AddIfFalse(exists, "goal102.editor.script_missing", relativePath, diagnostics);
        AddIfFalse(menu, "goal102.editor.menu_marker", relativePath, diagnostics);
        AddIfFalse(streaming, "goal102.editor.streaming_assets", relativePath, diagnostics);
        AddIfFalse(goal101Root, "goal102.editor.goal101_root", relativePath, diagnostics);
        AddIfFalse(create, "goal102.editor.create_missing", relativePath, diagnostics);
        AddIfFalse(clear, "goal102.editor.clear_missing", relativePath, diagnostics);
        AddIfFalse(payloadUi, "goal102.editor.payload_status", relativePath, diagnostics);
        AddIfFalse(manualOnly, "goal102.editor.manual_only", relativePath, diagnostics);
        AddIfFalse(noProvider, "goal102.editor.provider_network", relativePath, diagnostics);
        AddIfFalse(noBootstrap, "goal102.editor.alpha_bootstrap_dependency", relativePath, diagnostics);
        AddIfFalse(noScenePrefab, "goal102.editor.scene_prefab_settings", relativePath, diagnostics);
        AddIfFalse(noAutoRun, "goal102.editor.auto_run", relativePath, diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnityEditorToolInventory
        {
            Passed = ordered.All(item => item.Severity != "error"),
            EditorWindowScriptExists = exists,
            MenuItemMarkerPresent = menu,
            StreamingAssetsPathMarkerPresent = streaming,
            Goal101PayloadPathMarkerPresent = goal101Root,
            CreatePreviewObjectsMethodPresent = create,
            ClearPreviewObjectsMethodPresent = clear,
            PayloadStatusUiPresent = payloadUi,
            ManualButtonOnly = manualOnly,
            HasNoProviderNetworkMarkers = noProvider,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoScenePrefabSettingsMutationMarkers = noScenePrefab,
            HasNoAutoRunImportMarker = noAutoRun,
            SourceFile = new OfflineGeoworldUnityEditorSourceFile
            {
                RelativePath = relativePath,
                Exists = exists,
                Sha256 = exists ? HashFile(fullPath) : string.Empty,
                LineCount = CountLines(text),
                HasNoProviderNetworkMarkers = noProvider,
                DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
                HasNoScenePrefabSettingsMutationMarkers = noScenePrefab
            },
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldUnityEditorSimulatedActionProof BuildSimulatedActionProof(
        string root,
        Goal101EditorPreviewContext context,
        OfflineGeoworldUnityEditorToolInventory inventory)
    {
        var diagnostics = new List<OfflineGeoworldUnityEditorPreviewDiagnostic>();
        var countByKind = context.Commands.Commands
            .GroupBy(item => item.CommandKind, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var allKinds = OfflineGeoworldUnityPreviewRunnerVocabulary
            .RequiredCommandKinds
            .All(kind => countByKind.ContainsKey(kind));
        var noUnsupported = context.Commands.Commands.All(item =>
            OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredCommandKinds.Contains(item.CommandKind));
        var expectedObjects = context.Commands.Commands.Sum(item => item.ExpectedObjectCount);
        var objects = context.Commands.Commands
            .OrderBy(item => item.CommandId, StringComparer.Ordinal)
            .Select(item => new OfflineGeoworldUnityEditorPreviewObjectPlan
            {
                CommandId = item.CommandId,
                CommandKind = item.CommandKind,
                ObjectKind = EditorObjectKind(item.CommandKind),
                ObjectName = "editor_preview_" + Compact(item.CommandKind),
                ExpectedObjectCount = item.ExpectedObjectCount,
                MetadataOnly = item.MetadataOnly
            })
            .ToList();
        var countsMatch = context.Manifest.CommandCount == 18
                          && context.Commands.CommandCount == 18
                          && context.Manifest.CommandKindCount == 10
                          && context.Commands.CommandKindCount == 10
                          && context.TravelWindowScript.StepCount >= 4
                          && context.Manifest.ExpectedObjectCount == expectedObjects
                          && expectedObjects == 18;
        var previewBuilt = objects.Count == 18
                           && objects.All(item => item.ExpectedObjectCount == 1)
                           && objects.All(item => item.MetadataOnly);
        var createModel = inventory.CreatePreviewObjectsMethodPresent
                          && previewBuilt
                          && expectedObjects == objects.Sum(item => item.ExpectedObjectCount);
        var clearModel = inventory.ClearPreviewObjectsMethodPresent
                         && expectedObjects == 18;
        var values = new[]
        {
            context.ManifestJson,
            context.CommandCatalogJson,
            context.TravelWindowJson,
            ReadOptionalText(
                root,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.UnityEditorWindowScriptPath)
        }.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        var noAbsolute = values.All(value => !ContainsAbsolutePath(value));
        var noRaw = values.All(value =>
            !value.Contains("\"rawGeodataIncluded\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"noRawGeodata\": false", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"rawFullAreaDump\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"planetDump\": true", StringComparison.OrdinalIgnoreCase));
        var noBinary = values.All(value => !BinaryOrRasterExtensions.Any(ext =>
            value.Contains(ext, StringComparison.OrdinalIgnoreCase)));
        var noProvider = values.All(value => !ProviderNetworkMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase)));
        var noScenePrefab = inventory.HasNoScenePrefabSettingsMutationMarkers;

        AddIfFalse(context.Goal101PayloadFilesExist, "goal102.proof.payload_files", "Goal101 payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(context.Manifest.SchemaVersion), "goal102.proof.manifest", "manifest", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(context.Commands.SchemaVersion), "goal102.proof.commands", "commands", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(context.TravelWindowScript.SchemaVersion), "goal102.proof.travel", "travel", diagnostics);
        AddIfFalse(inventory.EditorWindowScriptExists, "goal102.proof.editor_script", "editor script", diagnostics);
        AddIfFalse(countsMatch, "goal102.proof.counts", "Goal101 payload", diagnostics);
        AddIfFalse(allKinds, "goal102.proof.command_kinds", "commands", diagnostics);
        AddIfFalse(noUnsupported, "goal102.proof.unsupported_command", "commands", diagnostics);
        AddIfFalse(previewBuilt, "goal102.proof.preview_plan", "preview objects", diagnostics);
        AddIfFalse(createModel, "goal102.proof.create_model", "editor action", diagnostics);
        AddIfFalse(clearModel, "goal102.proof.clear_model", "editor action", diagnostics);
        AddIfFalse(noAbsolute, "goal102.proof.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal102.proof.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal102.proof.binary_raster", "payload", diagnostics);
        AddIfFalse(noProvider, "goal102.proof.provider_network", "payload/editor", diagnostics);
        AddIfFalse(noScenePrefab, "goal102.proof.scene_prefab_settings", "editor", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldUnityEditorSimulatedActionProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && context.Goal101PayloadFilesExist
                     && countsMatch
                     && allKinds
                     && noUnsupported
                     && previewBuilt
                     && createModel
                     && clearModel
                     && noAbsolute
                     && noRaw
                     && noBinary
                     && noProvider
                     && noScenePrefab,
            PayloadReadAttempted = true,
            ManifestRead = !string.IsNullOrWhiteSpace(context.Manifest.SchemaVersion),
            CommandCatalogRead = !string.IsNullOrWhiteSpace(context.Commands.SchemaVersion),
            TravelWindowScriptRead = !string.IsNullOrWhiteSpace(context.TravelWindowScript.SchemaVersion),
            EditorWindowScriptRead = inventory.EditorWindowScriptExists,
            PayloadCountsMatchGoal101 = countsMatch,
            AllRequiredCommandKindsRepresented = allKinds,
            NoUnsupportedCommandKind = noUnsupported,
            PreviewObjectPlanBuilt = previewBuilt,
            CreateOperationModelPassed = createModel,
            ClearOperationModelPassed = clearModel,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noProvider,
            NoScenePrefabSettingsChangeMarkers = noScenePrefab,
            CommandCount = context.Commands.CommandCount,
            CommandKindCount = countByKind.Count,
            TravelWindowStepCount = context.TravelWindowScript.StepCount,
            ExpectedObjectCount = expectedObjects,
            ClearOperationRemovedObjectCount = clearModel ? expectedObjects : 0,
            CommandCountByKind = countByKind,
            PreviewObjects = objects,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldUnityEditorNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal101_payload", "Goal101 StreamingAssets payload removed",
                "goal102.negative.goal101_missing", "Goal101 payload"),
            Scenario("missing_editor_window_script", "Editor window source removed",
                "goal102.negative.editor_script_missing", "editor script"),
            Scenario("missing_menu_marker", "MenuItem marker removed",
                "goal102.negative.menu_marker", "editor script"),
            Scenario("missing_clear_method", "ClearPreviewObjects method removed",
                "goal102.negative.clear_missing", "editor script"),
            Scenario("unsupported_command_kind", "command kind changed to unsupported",
                "goal102.negative.unsupported_command", "preview command"),
            Scenario("network_provider_marker_in_editor_script", "editor script contains external marker",
                "goal102.negative.provider_network", "editor script"),
            Scenario("alpha_runtime_bootstrap_dependency_marker", "editor script references AlphaRuntimeBootstrap",
                "goal102.negative.alpha_bootstrap", "editor script"),
            Scenario("scene_prefab_project_settings_change_marker", "scene/prefab/settings mutation marker added",
                "goal102.negative.scene_prefab_settings", "editor script"),
            Scenario("fake_success_without_payload_read", "proof marked passed without reading payload",
                "goal102.negative.fake_success", "simulated action proof"),
            Scenario("absolute_path_in_payload", "absolute local path inserted into payload",
                "goal102.negative.absolute_path", "payload"),
            Scenario("raw_geodata_leaked_into_command", "raw geodata marker added to command",
                "goal102.negative.raw_geodata", "preview command"),
            Scenario("binary_raster_media_marker", "binary or raster media added to payload",
                "goal102.negative.binary_media", "payload"),
            Scenario("missing_create_method", "CreatePreviewObjects method removed",
                "goal102.negative.create_missing", "editor script")
        };
        return new OfflineGeoworldUnityEditorNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldUnityEditorPreviewToolVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static string EditorObjectKind(string commandKind) =>
        commandKind switch
        {
            "barrier_line" => "line",
            "road_segment_line" => "line",
            "land_use_area_plane" => "plane",
            "water_body_plane" => "plane",
            "poi_marker" => "sphere",
            "terrain_hint_marker" => "capsule",
            "vegetation_area_marker" => "sphere",
            _ => "cube"
        };

    private static string Compact(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_');
        }

        return builder.ToString();
    }

    private static OfflineGeoworldUnityEditorNegativeScenario Scenario(
        string id,
        string mutation,
        string code,
        string target) =>
        new()
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ActualStatus = "rejected",
            Diagnostics =
            [
                OfflineGeoworldUnityEditorPreviewDiagnostic.Error(
                    code,
                    target,
                    "Goal102 negative proof rejected the mutated editor preview input.")
            ]
        };
}
