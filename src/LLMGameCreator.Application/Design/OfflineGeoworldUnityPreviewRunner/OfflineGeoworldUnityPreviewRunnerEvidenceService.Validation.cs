using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

public sealed partial class OfflineGeoworldUnityPreviewRunnerEvidenceService
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

    private static OfflineGeoworldPreviewSimulatedCommandProof ValidateMirroredPayload(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        var rootPath = Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsRelativeRoot);
        foreach (var fileName in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames)
        {
            var path = Path.Combine(rootPath, fileName);
            AddIfFalse(File.Exists(path), "goal101.read.payload_file_missing", fileName, diagnostics);
        }

        return ValidatePayload(payload, payloadReadAttempted: true, diagnostics);
    }

    private static OfflineGeoworldPreviewSimulatedCommandProof ValidatePayload(
        IReadOnlyDictionary<string, string> payload,
        bool payloadReadAttempted,
        List<OfflineGeoworldUnityPreviewDiagnostic>? seedDiagnostics = null)
    {
        var diagnostics = seedDiagnostics ?? [];
        var requiredPresent = OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames
            .All(payload.ContainsKey);
        payload.TryGetValue(OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestFileName, out var manifestJson);
        payload.TryGetValue(
            OfflineGeoworldUnityPreviewRunnerVocabulary.FeatureCommandsFileName,
            out var commandsJson);
        payload.TryGetValue(
            OfflineGeoworldUnityPreviewRunnerVocabulary.StyleLegendFileName,
            out var styleJson);
        payload.TryGetValue(
            OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName,
            out var travelJson);
        payload.TryGetValue(OfflineGeoworldUnityPreviewRunnerVocabulary.ReadmeFileName, out var readmeJson);

        var manifest = OfflineGeoworldUnityPreviewJson
            .Deserialize<OfflineGeoworldPreviewRunnerManifest>(manifestJson ?? string.Empty)
            ?? new OfflineGeoworldPreviewRunnerManifest();
        var commands = OfflineGeoworldUnityPreviewJson
            .Deserialize<OfflineGeoworldPreviewFeatureCommandCatalog>(commandsJson ?? string.Empty)
            ?? new OfflineGeoworldPreviewFeatureCommandCatalog();
        var style = OfflineGeoworldUnityPreviewJson
            .Deserialize<OfflineGeoworldPreviewStyleLegend>(styleJson ?? string.Empty)
            ?? new OfflineGeoworldPreviewStyleLegend();
        var travel = OfflineGeoworldUnityPreviewJson
            .Deserialize<OfflineGeoworldPreviewTravelWindowScript>(travelJson ?? string.Empty)
            ?? new OfflineGeoworldPreviewTravelWindowScript();

        var hashesMatch = string.Equals(
                              manifest.FeatureCommandsHash,
                              Hash(commandsJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              manifest.TravelWindowScriptHash,
                              Hash(travelJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              manifest.StyleLegendHash,
                              Hash(styleJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              manifest.ReadmeHash,
                              Hash(readmeJson ?? string.Empty),
                              StringComparison.OrdinalIgnoreCase);
        var countByKind = commands.Commands
            .GroupBy(item => item.CommandKind, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var allKinds = OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredCommandKinds
            .All(kind => countByKind.ContainsKey(kind));
        var noUnsupported = commands.Commands.All(item =>
            OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredCommandKinds.Contains(item.CommandKind));
        var stylesCoverCommands = style.Styles.Select(item => item.CommandKind)
            .OrderBy(value => value, StringComparer.Ordinal)
            .SequenceEqual(
                OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredCommandKinds
                    .OrderBy(value => value, StringComparer.Ordinal),
                StringComparer.Ordinal);
        var travelCoversCommands = travel.CommandCoverageCount == commands.CommandCount
                                   && travel.StepCount >= 4
                                   && travel.Steps.Any(item => item.VisibleCommandIds.Count == commands.CommandCount);
        var expectedObjects = commands.Commands.Sum(item => item.ExpectedObjectCount);
        var countsMatch = manifest.CommandCount == 18
                          && manifest.CommandCount == commands.CommandCount
                          && manifest.CommandKindCount == 10
                          && manifest.CommandKindCount == commands.CommandKindCount
                          && manifest.StyleCount == style.StyleCount
                          && manifest.TravelWindowStepCount == travel.StepCount
                          && manifest.ExpectedObjectCount == expectedObjects;
        var values = payload.Values.ToList();
        var noAbsolute = values.All(value => !ContainsAbsolutePath(value));
        var noRaw = values.All(value =>
            !value.Contains("\"rawGeodataIncluded\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"noRawGeodata\": false", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"rawFullAreaDump\": true", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("\"planetDump\": true", StringComparison.OrdinalIgnoreCase));
        var noBinary = payload.Keys.All(path => !IsBinaryOrRasterMedia(path))
                       && values.All(value => !BinaryOrRasterExtensions.Any(ext =>
                           value.Contains(ext, StringComparison.OrdinalIgnoreCase)));
        var noMarkers = values.All(value => !ProviderNetworkMarkers.Any(marker =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase)));

        AddIfFalse(requiredPresent, "goal101.read.required_files", "payload", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manifest.SchemaVersion), "goal101.read.manifest", "manifest", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(commands.SchemaVersion), "goal101.read.commands", "commands", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(style.SchemaVersion), "goal101.read.style", "style", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(travel.SchemaVersion), "goal101.read.travel", "travel", diagnostics);
        AddIfFalse(hashesMatch, "goal101.read.hash_mismatch", "manifest", diagnostics);
        AddIfFalse(countsMatch, "goal101.read.counts", "payload", diagnostics);
        AddIfFalse(allKinds, "goal101.read.command_kind_coverage", "commands", diagnostics);
        AddIfFalse(noUnsupported, "goal101.read.unsupported_command", "commands", diagnostics);
        AddIfFalse(stylesCoverCommands, "goal101.read.style_coverage", "style", diagnostics);
        AddIfFalse(travelCoversCommands, "goal101.read.travel_coverage", "travel", diagnostics);
        AddIfFalse(noAbsolute, "goal101.read.absolute_path", "payload", diagnostics);
        AddIfFalse(noRaw, "goal101.read.raw_geodata", "payload", diagnostics);
        AddIfFalse(noBinary, "goal101.read.binary_raster", "payload", diagnostics);
        AddIfFalse(noMarkers, "goal101.read.provider_network", "payload", diagnostics);

        var ordered = SortDiagnostics(diagnostics);
        return new OfflineGeoworldPreviewSimulatedCommandProof
        {
            Passed = ordered.All(item => item.Severity != "error")
                     && payloadReadAttempted
                     && requiredPresent
                     && hashesMatch
                     && countsMatch
                     && allKinds
                     && noUnsupported
                     && stylesCoverCommands
                     && travelCoversCommands
                     && noAbsolute
                     && noRaw
                     && noBinary
                     && noMarkers,
            PayloadReadAttempted = payloadReadAttempted,
            ManifestRead = !string.IsNullOrWhiteSpace(manifest.SchemaVersion),
            CommandFileRead = !string.IsNullOrWhiteSpace(commands.SchemaVersion),
            StyleLegendRead = !string.IsNullOrWhiteSpace(style.SchemaVersion),
            TravelWindowScriptRead = !string.IsNullOrWhiteSpace(travel.SchemaVersion),
            PayloadHashesMatchManifest = hashesMatch,
            AllRequiredCommandKindsRepresented = allKinds,
            NoUnsupportedCommandKind = noUnsupported,
            NoAbsolutePaths = noAbsolute,
            NoRawGeodata = noRaw,
            NoBinaryOrRasterMedia = noBinary,
            NoProviderOrNetworkMarkers = noMarkers,
            CommandCount = commands.CommandCount,
            CommandKindCount = countByKind.Count,
            TravelWindowStepCount = travel.StepCount,
            ExpectedObjectCount = expectedObjects,
            CommandCountByKind = countByKind,
            Diagnostics = ordered
        };
    }

    private static OfflineGeoworldPreviewNegativeProof BuildNegativeProof()
    {
        var scenarios = new[]
        {
            Scenario("missing_goal100_payload", "Goal100 command source removed",
                "goal101.negative.goal100_missing", Goal100Root),
            Scenario("unsupported_feature_command_kind", "command kind changed to unsupported",
                "goal101.negative.unsupported_command", "preview command"),
            Scenario("raw_geodata_leaked_into_command", "raw geodata marker added to command",
                "goal101.negative.raw_geodata", "preview command"),
            Scenario("missing_style_legend", "style legend payload removed",
                "goal101.negative.missing_style", "style legend"),
            Scenario("missing_travel_window_script", "travel window script payload removed",
                "goal101.negative.missing_travel", "travel window"),
            Scenario("absolute_path_in_payload", "absolute local path inserted into payload",
                "goal101.negative.absolute_path", "payload"),
            Scenario("network_provider_marker_in_unity_script", "Unity script contains external marker",
                "goal101.negative.script_marker", "Unity preview script"),
            Scenario("fake_success_without_file_read", "proof marked passed without reading payload",
                "goal101.negative.fake_success", "simulated command proof"),
            Scenario("alpha_runtime_bootstrap_changed_marker", "bootstrap hash changed",
                "goal101.negative.alpha_bootstrap", "AlphaRuntimeBootstrap.cs"),
            Scenario("binary_raster_media_marker", "binary or raster media added to payload",
                "goal101.negative.binary_media", "StreamingAssets"),
            Scenario("rating_metadata_missing_safe_fallback", "rating metadata lacks safe fallback",
                "goal101.negative.rating_safe_fallback", "preview command")
        };
        return new OfflineGeoworldPreviewNegativeProof
        {
            Passed = scenarios.Length == OfflineGeoworldUnityPreviewRunnerVocabulary
                .RequiredNegativeScenarioIds.Count
                     && scenarios.All(item => item.ActualStatus == "rejected"),
            ScenarioCount = scenarios.Length,
            RejectedCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            MatchedExpectationCount = scenarios.Count(item => item.ActualStatus == "rejected"),
            Scenarios = scenarios
        };
    }

    private static OfflineGeoworldPreviewStreamingAssetsLedger BuildStreamingAssetsLedger(
        string root,
        IReadOnlyDictionary<string, string> payload)
    {
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        var files = new List<OfflineGeoworldPreviewPayloadFile>();
        foreach (var fileName in OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames)
        {
            payload.TryGetValue(fileName, out var text);
            var fullPath = Resolve(
                root,
                OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsRelativeRoot + "/" + fileName);
            var exists = File.Exists(fullPath) || text is not null;
            AddIfFalse(exists, "goal101.streamingassets.file_missing", fileName, diagnostics);
            files.Add(new OfflineGeoworldPreviewPayloadFile
            {
                RelativePath = fileName,
                RepositoryRelativePath =
                    OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsRelativeRoot + "/" + fileName,
                Role = PayloadRole(fileName),
                Sha256 = text is null ? string.Empty : Hash(text),
                ByteCount = text is null ? 0 : Encoding.UTF8.GetByteCount(text),
                Exists = exists
            });
        }

        return new OfflineGeoworldPreviewStreamingAssetsLedger
        {
            Passed = diagnostics.Count == 0 && files.Count == 5,
            PayloadFileCount = files.Count(item => item.Exists),
            Files = files.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static OfflineGeoworldPreviewUnityScriptInventory BuildUnityScriptInventory(string root)
    {
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        var runnerPath = Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPreviewRunnerScriptPath);
        var factoryPath = Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPrimitiveFactoryScriptPath);
        var travelPath = Resolve(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityTravelWindowScriptPath);
        var runner = ReadOptionalText(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPreviewRunnerScriptPath);
        var factory = ReadOptionalText(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPrimitiveFactoryScriptPath);
        var travel = ReadOptionalText(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityTravelWindowScriptPath);
        var combined = runner + Environment.NewLine + factory + Environment.NewLine + travel;
        var runnerExists = File.Exists(runnerPath);
        var factoryExists = File.Exists(factoryPath);
        var travelExists = File.Exists(travelPath);
        var usesStreamingAssets = runner.Contains("Application.streamingAssetsPath", StringComparison.Ordinal);
        var usesRoot = runner.Contains(
            OfflineGeoworldUnityPreviewRunnerVocabulary.UnityStreamingAssetsProbeRoot,
            StringComparison.Ordinal);
        var exposesInspector = runner.Contains("[SerializeField]", StringComparison.Ordinal)
                               && runner.Contains("LastStatus", StringComparison.Ordinal)
                               && runner.Contains("LastResult", StringComparison.Ordinal);
        var factoryCreates = factory.Contains("GameObject.CreatePrimitive", StringComparison.Ordinal)
                             && factory.Contains("LineRenderer", StringComparison.Ordinal);
        var travelSupports = travel.Contains("ApplyStep", StringComparison.Ordinal)
                             && travel.Contains("StepCount", StringComparison.Ordinal);
        var noBootstrap = !combined.Contains("AlphaRuntimeBootstrap", StringComparison.Ordinal);
        var noMarkers = !ProviderNetworkMarkers.Any(marker =>
            combined.Contains(marker, StringComparison.OrdinalIgnoreCase));

        AddIfFalse(runnerExists, "goal101.script.runner_missing",
            OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPreviewRunnerScriptPath, diagnostics);
        AddIfFalse(factoryExists, "goal101.script.factory_missing",
            OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPrimitiveFactoryScriptPath, diagnostics);
        AddIfFalse(travelExists, "goal101.script.travel_missing",
            OfflineGeoworldUnityPreviewRunnerVocabulary.UnityTravelWindowScriptPath, diagnostics);
        AddIfFalse(usesStreamingAssets, "goal101.script.streamingassets", "runner", diagnostics);
        AddIfFalse(usesRoot, "goal101.script.root", "runner", diagnostics);
        AddIfFalse(exposesInspector, "goal101.script.inspector", "runner", diagnostics);
        AddIfFalse(factoryCreates, "goal101.script.factory_primitives", "factory", diagnostics);
        AddIfFalse(travelSupports, "goal101.script.travel_steps", "travel", diagnostics);
        AddIfFalse(noBootstrap, "goal101.script.bootstrap_dependency", "scripts", diagnostics);
        AddIfFalse(noMarkers, "goal101.script.provider_network", "scripts", diagnostics);

        var files = new[]
        {
            ScriptFile(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPreviewRunnerScriptPath),
            ScriptFile(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityPrimitiveFactoryScriptPath),
            ScriptFile(root, OfflineGeoworldUnityPreviewRunnerVocabulary.UnityTravelWindowScriptPath)
        };
        return new OfflineGeoworldPreviewUnityScriptInventory
        {
            Passed = diagnostics.Count == 0,
            RunnerExists = runnerExists,
            FactoryExists = factoryExists,
            TravelWindowExists = travelExists,
            RunnerUsesApplicationStreamingAssetsPath = usesStreamingAssets,
            RunnerReadsGoal101Root = usesRoot,
            RunnerExposesInspectorFields = exposesInspector,
            FactoryCreatesPrimitivePlaceholders = factoryCreates,
            TravelWindowSupportsDemoSteps = travelSupports,
            DoesNotReferenceAlphaRuntimeBootstrap = noBootstrap,
            HasNoProviderLlmNetworkMarkers = noMarkers,
            Files = files,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static OfflineGeoworldPreviewUnityScriptFile ScriptFile(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new OfflineGeoworldPreviewUnityScriptFile
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? HashFile(path) : string.Empty,
            LineCount = CountLines(text),
            HasNoProviderLlmNetworkMarkers = !ProviderNetworkMarkers.Any(marker =>
                text.Contains(marker, StringComparison.OrdinalIgnoreCase))
        };
    }
}
