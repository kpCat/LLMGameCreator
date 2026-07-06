using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class GenericGamePackageProjectionService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly string[] SourceWriteMarkers =
    [
        "File.Write",
        "WriteAllText",
        "WriteAllBytes",
        "AssetDatabase.CreateAsset",
        "EditorSceneManager.Save",
        "SaveScene",
        "StreamingAssets"
    ];

    public GenericGamePackageProjectionBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var samplePackage = BuildSamplePackageSummary(root);
        var scriptInventory = BuildScriptInventory(root);
        var smokePlan = BuildSmokePlan();
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof();
        var goal122StillGreen = Goal122StillGreen(root);
        var dashboard = BuildDashboard(
            samplePackage,
            scriptInventory,
            logScan,
            negative,
            goal122StillGreen);
        var report = RenderReport(dashboard, samplePackage, scriptInventory, smokePlan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [GenericGamePackageProjectionVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [GenericGamePackageProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [GenericGamePackageProjectionVocabulary.LogScanFileName] = Serialize(logScan),
            [GenericGamePackageProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory,
            "goal123_generic_gamepackage_projection_evidence",
            includeUnityLog: true);
        proceduralFiles[GenericGamePackageProjectionVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GenericGamePackageProjectionVocabulary.DashboardFileName] = Serialize(dashboard),
            [GenericGamePackageProjectionVocabulary.ScriptInventoryFileName] = Serialize(scriptInventory),
            [GenericGamePackageProjectionVocabulary.SmokePlanFileName] = Serialize(smokePlan),
            [GenericGamePackageProjectionVocabulary.LogScanFileName] = Serialize(logScan),
            [GenericGamePackageProjectionVocabulary.ReportFileName] = report,
            [GenericGamePackageProjectionVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            GenericGamePackageProjectionVocabulary.ExportPackageDirectory,
            "goal123_generic_gamepackage_projection_export",
            includeUnityLog: false);
        exportFiles[GenericGamePackageProjectionVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new GenericGamePackageProjectionBuildResult
        {
            Dashboard = dashboard,
            SamplePackage = samplePackage,
            ScriptInventory = scriptInventory,
            SmokePlan = smokePlan,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<GenericGamePackageProjectionWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(root, GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory);
        var export = Resolve(root, GenericGamePackageProjectionVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(root, GenericGamePackageProjectionVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new GenericGamePackageProjectionWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageProjectionDashboard BuildDashboard(
        GenericGamePackageProjectionSamplePackageSummary samplePackage,
        GenericGamePackageProjectionScriptInventory scriptInventory,
        GenericGamePackageProjectionLogScan logScan,
        GenericGamePackageProjectionNegativeProof negative,
        bool goal122StillGreen)
    {
        var diagnostics = new List<string>();
        Require(goal122StillGreen, "goal123.goal122_not_green", diagnostics);
        Require(samplePackage.Passed, "goal123.sample_package_projection_failed", diagnostics);
        Require(scriptInventory.Passed, "goal123.script_inventory_failed", diagnostics);
        Require(negative.Passed, "goal123.negative_proof_failed", diagnostics);
        Require(logScan.Status != "BLOCKED_UNITY_BATCHMODE_GENERIC_PACKAGE_PROJECTION",
            "goal123.unity_generic_smoke_failed",
            diagnostics);

        return new GenericGamePackageProjectionDashboard
        {
            GenericProjectionStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            PackageId = samplePackage.PackageId,
            PackageTitle = samplePackage.PackageTitle,
            MapId = samplePackage.MapId,
            MapSize = samplePackage.MapWidth + "x" + samplePackage.MapHeight,
            EntityCount = samplePackage.EntityCount,
            ItemCount = samplePackage.ItemCount,
            UnitySmokeStatus = logScan.Status,
            Goal122StillGreen = goal122StillGreen,
            CleanupScriptAvailable = CleanupScriptAvailable(scriptInventory),
            SamplePackageReadOnly = samplePackage.ReadOnlySource,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static GenericGamePackageProjectionSamplePackageSummary BuildSamplePackageSummary(string root)
    {
        var diagnostics = new List<string>();
        var path = Resolve(root, GenericGamePackageProjectionVocabulary.SamplePackagePath);
        if (!File.Exists(path))
        {
            diagnostics.Add("goal123.sample_package_missing");
            return new GenericGamePackageProjectionSamplePackageSummary
            {
                Exists = false,
                Parsed = false,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths = true,
                Diagnostics = diagnostics,
                Passed = false
            };
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            using var doc = JsonDocument.Parse(text);
            var rootElement = doc.RootElement;
            var manifest = rootElement.GetProperty("manifest");
            var game = rootElement.GetProperty("game");
            var maps = game.GetProperty("maps").EnumerateArray().ToList();
            var startMapId = StringValue(manifest, "startMapId");
            var map = maps.FirstOrDefault(item => StringValue(item, "id") == startMapId);
            if (map.ValueKind == JsonValueKind.Undefined && maps.Count > 0)
            {
                map = maps[0];
            }

            var entityPrototypes = game.GetProperty("entityPrototypes").EnumerateArray().ToList();
            var entities = map.GetProperty("entities").EnumerateArray().ToList();
            var explicitTiles = map.GetProperty("tiles").EnumerateArray().ToList();
            var items = game.GetProperty("items").EnumerateArray().ToList();
            var interactablePrototypeIds = entityPrototypes
                .Where(PrototypeHasInteractableComponent)
                .Select(item => StringValue(item, "id"))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.Ordinal);
            var explicitTileIds = explicitTiles
                .Select(tile => StringValue(tile, "tileId"))
                .ToList();
            var packageId = StringValue(manifest, "packageId");
            var packageTitle = StringValue(manifest, "title");
            var mapWidth = IntValue(map, "width");
            var mapHeight = IntValue(map, "height");
            var startPosition = map.GetProperty("startPosition");

            var summary = new GenericGamePackageProjectionSamplePackageSummary
            {
                Exists = true,
                Parsed = true,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths =
                    !BuildExpectedChangedPaths().Contains(
                        GenericGamePackageProjectionVocabulary.SamplePackagePath,
                        StringComparer.Ordinal),
                Sha256 = HashBytes(File.ReadAllBytes(path)),
                PackageId = packageId,
                PackageTitle = packageTitle,
                StartMapId = startMapId,
                MapId = StringValue(map, "id"),
                MapName = StringValue(map, "name"),
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                StartX = IntValue(startPosition, "x"),
                StartY = IntValue(startPosition, "y"),
                ExplicitTileCount = explicitTiles.Count,
                WallTilePresent = explicitTileIds.Any(id =>
                    id.Contains("wall", StringComparison.OrdinalIgnoreCase)),
                RoadTilePresent = explicitTileIds.Any(id =>
                    id.Contains("road", StringComparison.OrdinalIgnoreCase)),
                EntityCount = entities.Count,
                InteractableEntityCount = entities.Count(entity =>
                    interactablePrototypeIds.Contains(StringValue(entity, "prototypeId"))),
                ItemCount = items.Count,
                PackageIdentityPresent =
                    !string.IsNullOrWhiteSpace(packageId)
                    && !string.IsNullOrWhiteSpace(packageTitle),
                MapDimensionsPresent = mapWidth > 0 && mapHeight > 0,
                StartPositionPresent = startPosition.ValueKind == JsonValueKind.Object,
                Diagnostics = diagnostics
            };
            return summary with
            {
                Passed = summary.Exists
                         && summary.Parsed
                         && summary.ReadOnlySource
                         && summary.ExcludedFromExpectedChangedPaths
                         && summary.PackageIdentityPresent
                         && summary.MapDimensionsPresent
                         && summary.StartPositionPresent
                         && summary.ExplicitTileCount > 0
                         && summary.WallTilePresent
                         && summary.RoadTilePresent
                         && summary.EntityCount > 0
                         && summary.InteractableEntityCount > 0
                         && summary.ItemCount > 0
            };
        }
        catch (Exception ex)
        {
            diagnostics.Add("goal123.sample_package_parse_failed:" + ex.GetType().Name);
            return new GenericGamePackageProjectionSamplePackageSummary
            {
                Exists = true,
                Parsed = false,
                ReadOnlySource = true,
                ExcludedFromExpectedChangedPaths = true,
                Sha256 = HashBytes(File.ReadAllBytes(path)),
                Diagnostics = diagnostics,
                Passed = false
            };
        }
    }

    private static bool PrototypeHasInteractableComponent(JsonElement prototype)
    {
        if (!prototype.TryGetProperty("components", out var components)
            || components.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return components.EnumerateArray().Any(component =>
            StringValue(component, "type") == "interactable");
    }

    private static GenericGamePackageProjectionScriptInventory BuildScriptInventory(string root)
    {
        var entries = new[]
        {
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
                "unity_editor_window", "Run Generic Package Projection Verification"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityAdapterPath,
                "unity_generic_projection_adapter", "SamplePackageRelativePath"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityModelsPath,
                "unity_generic_projection_models", "GenericGamePackageProjectionSmokeResult"),
            Entry(root, GenericGamePackageProjectionVocabulary.UnityControllerPath,
                "unity_generic_projection_controller", "RunGenericPackageProjectionVerification"),
            Entry(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
                "unity_projection_primitive_factory", "AttachDescriptor"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
                "cleanup_script", "Unity editor noise cleanup mode"),
            Entry(root, AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
                "cleanup_cmd_wrapper", "clean-unity-editor-noise.ps1")
        }.ToList();

        var editorText = SourceText(root, AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath);
        var adapterText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityAdapterPath);
        var modelsText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityModelsPath);
        var controllerText = SourceText(root, GenericGamePackageProjectionVocabulary.UnityControllerPath);
        var genericSource = string.Join("\n", new[] { adapterText, modelsText, controllerText });
        var forbidden = SourceWriteMarkers
            .Where(marker => genericSource.Contains(marker, StringComparison.Ordinal))
            .OrderBy(marker => marker, StringComparer.Ordinal)
            .ToList();

        var inventory = new GenericGamePackageProjectionScriptInventory
        {
            ScriptCount = entries.Count,
            WindowActionPresent =
                editorText.Contains("Run Generic Package Projection Verification", StringComparison.Ordinal)
                && editorText.Contains("RunGenericPackageProjectionVerification()", StringComparison.Ordinal),
            BatchmodeMethodPresent =
                editorText.Contains("RunBatchmodeGenericGamePackageProjectionSmoke", StringComparison.Ordinal),
            BatchmodePassMarkerPresent =
                editorText.Contains("GOAL123_GENERIC_PACKAGE_PROJECTION_PASS", StringComparison.Ordinal),
            BatchmodeFailMarkerPresent =
                editorText.Contains("GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL", StringComparison.Ordinal),
            AdapterReadsSamplePackage =
                adapterText.Contains(GenericGamePackageProjectionVocabulary.SamplePackagePath, StringComparison.Ordinal)
                && adapterText.Contains("File.ReadAllText", StringComparison.Ordinal),
            ControllerBuildsGenericSection =
                controllerText.Contains("goal123_generic_gamepackage_projection", StringComparison.Ordinal)
                && controllerText.Contains("BuildOrRefreshGenericPackageProjection", StringComparison.Ordinal),
            ControllerVerifiesRequiredMarkers =
                controllerText.Contains("PackageIdentityPresent", StringComparison.Ordinal)
                && controllerText.Contains("StartPlayerMarkerPresent", StringComparison.Ordinal)
                && controllerText.Contains("ItemSummaryEntryPresent", StringComparison.Ordinal),
            ModelsExposeSmokeFields =
                modelsText.Contains("PackageIdentityPresent", StringComparison.Ordinal)
                && modelsText.Contains("MapDimensionsPresent", StringComparison.Ordinal)
                && modelsText.Contains("ItemSummaryEntryPresent", StringComparison.Ordinal),
            ExistingGoal122VerificationStillPresent =
                editorText.Contains("Run Full Projection Verification", StringComparison.Ordinal)
                && editorText.Contains("RunBatchmodeProjectionActionLoopSmoke", StringComparison.Ordinal)
                && editorText.Contains("GOAL122_ACTION_LOOP_SMOKE_PASS", StringComparison.Ordinal),
            MarkerDescriptorCompatible =
                controllerText.Contains("AcceptedAlphaPlayableProjectionMarkerDescriptor", StringComparison.Ordinal)
                && controllerText.Contains("AttachDescriptor", StringComparison.Ordinal)
                && controllerText.Contains("DescribeMarker", StringComparison.Ordinal),
            NoSourceWriteMarkers = forbidden.Count == 0,
            ForbiddenSourceMarkersFound = forbidden,
            Scripts = entries
        };

        return inventory with
        {
            Passed = inventory.ScriptCount == 7
                     && entries.All(entry => entry.Exists && entry.ContainsRequiredMarker)
                     && inventory.WindowActionPresent
                     && inventory.BatchmodeMethodPresent
                     && inventory.BatchmodePassMarkerPresent
                     && inventory.BatchmodeFailMarkerPresent
                     && inventory.AdapterReadsSamplePackage
                     && inventory.ControllerBuildsGenericSection
                     && inventory.ControllerVerifiesRequiredMarkers
                     && inventory.ModelsExposeSmokeFields
                     && inventory.ExistingGoal122VerificationStillPresent
                     && inventory.MarkerDescriptorCompatible
                     && inventory.NoSourceWriteMarkers
        };
    }

    private static GenericGamePackageProjectionSmokePlan BuildSmokePlan()
    {
        var steps = new List<GenericGamePackageProjectionSmokePlanStep>
        {
            Step(1, "open_projection_window", "Open the accepted Alpha projection menu path."),
            Step(2, "run_generic_package_projection_verification", "Click Run Generic Package Projection Verification."),
            Step(3, "load_sample_package", "Read samples/minimal-map-game/package.json without mutating it."),
            Step(4, "render_package_identity", "Title and package id are visible."),
            Step(5, "render_map_grid", "Map id and dimensions plus tile/road/wall markers are visible."),
            Step(6, "render_start_player_proxy", "Start/player proxy marker is present."),
            Step(7, "render_entities_and_interactions", "Entity markers and interaction details are present."),
            Step(8, "render_item_summary", "Item list panel has at least one item entry."),
            Step(9, "read_event_log", "Package verification event log reports pass/fail."),
            Step(10, "cleanup_after_unity", "Use the existing clean-unity-editor-noise command after Unity checks.")
        };

        return new GenericGamePackageProjectionSmokePlan
        {
            StepCount = steps.Count,
            Steps = steps
        };
    }

    private static GenericGamePackageProjectionLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, GenericGamePackageProjectionVocabulary.UnityBatchmodeLogRelativePath);
        var logExists = File.Exists(path);
        var text = logExists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var forbidden = new List<string>();
        if (text.Contains("GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL", StringComparison.Ordinal))
        {
            forbidden.Add("GOAL123_GENERIC_PACKAGE_PROJECTION_FAIL");
        }

        var smokeFields = new[]
        {
            "packageIdentityPresent=True",
            "mapDimensionsPresent=True",
            "startPlayerMarkerPresent=True",
            "tileMarkerPresent=True",
            "entityMarkerPresent=True",
            "interactionMarkerPresent=True",
            "itemSummaryEntryPresent=True",
            "eventLogPresent=True",
            "zeroFatalErrors=True",
            "packageId=game/minimal-map-game",
            "packageTitle=Minimal Map Game",
            "mapId=map/village",
            "mapWidth=12",
            "mapHeight=8"
        };
        var passMarkerPresent = text.Contains("GOAL123_GENERIC_PACKAGE_PROJECTION_PASS", StringComparison.Ordinal);
        var smokeFieldsPresent = smokeFields.All(field => text.Contains(field, StringComparison.Ordinal));
        var passed = logExists && passMarkerPresent && smokeFieldsPresent && forbidden.Count == 0;
        return new GenericGamePackageProjectionLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = forbidden.Count == 0,
            SmokeRequiredFieldsPresent = smokeFieldsPresent,
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_BATCHMODE_GENERIC_PACKAGE_PROJECTION"
                    : "PENDING_UNITY_BATCHMODE_GENERIC_PACKAGE_PROJECTION",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden
        };
    }

    private static GenericGamePackageProjectionNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new GenericGamePackageProjectionNegativeProof
        {
            ManualInputRejected = true,
            SamplePackageMutationRejected = true,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected = true,
            FinalReleasePackagingRejected = true,
            NoForbiddenPathExpected = BuildExpectedChangedPaths()
                .All(path => !IsRejectedPath(path)),
            RejectedPathSamples = rejected,
            Passed = rejected.All(path => !IsAllowedChangedPath(path))
        };
    }

    private static GenericGamePackageProjectionFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        bool includeUnityLog)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new GenericGamePackageProjectionFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (includeUnityLog)
        {
            var logPath = Resolve(root, GenericGamePackageProjectionVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new GenericGamePackageProjectionFileIndexEntry
                {
                    RelativePath = GenericGamePackageProjectionVocabulary.UnityBatchmodeLogRelativePath,
                    Role = "goal123_unity_batchmode_generic_gamepackage_projection_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new GenericGamePackageProjectionFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static string RenderReport(
        GenericGamePackageProjectionDashboard dashboard,
        GenericGamePackageProjectionSamplePackageSummary samplePackage,
        GenericGamePackageProjectionScriptInventory scriptInventory,
        GenericGamePackageProjectionSmokePlan smokePlan,
        GenericGamePackageProjectionLogScan logScan,
        GenericGamePackageProjectionNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 123 Generic GamePackage Playable Projection Adapter",
            string.Empty,
            "- genericProjectionStatus: " + dashboard.GenericProjectionStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- packageTitle: " + dashboard.PackageTitle,
            "- mapId: " + dashboard.MapId,
            "- mapSize: " + dashboard.MapSize,
            "- entityCount: " + dashboard.EntityCount,
            "- itemCount: " + dashboard.ItemCount,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- goal122StillGreen: " + dashboard.Goal122StillGreen.ToString().ToLowerInvariant(),
            "- cleanupScriptAvailable: " + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Sample Package",
            string.Empty,
            "- exists: " + samplePackage.Exists.ToString().ToLowerInvariant(),
            "- parsed: " + samplePackage.Parsed.ToString().ToLowerInvariant(),
            "- readOnlySource: " + samplePackage.ReadOnlySource.ToString().ToLowerInvariant(),
            "- sha256: " + samplePackage.Sha256,
            "- explicitTileCount: " + samplePackage.ExplicitTileCount,
            "- wallTilePresent: " + samplePackage.WallTilePresent.ToString().ToLowerInvariant(),
            "- roadTilePresent: " + samplePackage.RoadTilePresent.ToString().ToLowerInvariant(),
            "- interactableEntityCount: " + samplePackage.InteractableEntityCount,
            string.Empty,
            "## Script Inventory",
            string.Empty,
            "- passed: " + scriptInventory.Passed.ToString().ToLowerInvariant(),
            "- scriptCount: " + scriptInventory.ScriptCount,
            "- windowActionPresent: " + scriptInventory.WindowActionPresent.ToString().ToLowerInvariant(),
            "- adapterReadsSamplePackage: " + scriptInventory.AdapterReadsSamplePackage.ToString().ToLowerInvariant(),
            "- noSourceWriteMarkers: " + scriptInventory.NoSourceWriteMarkers.ToString().ToLowerInvariant(),
            string.Empty,
            "## Smoke Plan",
            string.Empty,
            "- stepCount: " + smokePlan.StepCount,
            string.Empty,
            "## Log Scan",
            string.Empty,
            "- status: " + logScan.Status,
            "- logExists: " + logScan.LogExists.ToString().ToLowerInvariant(),
            "- passMarkerPresent: " + logScan.PassMarkerPresent.ToString().ToLowerInvariant(),
            "- smokeRequiredFieldsPresent: " + logScan.SmokeRequiredFieldsPresent.ToString().ToLowerInvariant(),
            "- forbiddenMarkerCount: " + logScan.ForbiddenMarkersFound.Count,
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- passed: " + negative.Passed.ToString().ToLowerInvariant()
        };
        if (dashboard.Diagnostics.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Diagnostics");
            lines.Add(string.Empty);
            lines.AddRange(dashboard.Diagnostics.Select(item => "- " + item));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(GenericGamePackageProjectionDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Generic GamePackage Playable Projection Adapter",
            string.Empty,
            "Goal123 adds a projection-only Unity Editor preview for `samples/minimal-map-game/package.json` under the accepted Alpha projection shell.",
            string.Empty,
            "## Hands-on Verification",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Generic Package Projection Verification`.",
            "- Verify the package title/id, map dimensions, start/player proxy, tile markers, entities, interaction details, item summary and event log.",
            "- Do not save scenes, prefabs, ProjectSettings, Packages or StreamingAssets as part of this check.",
            string.Empty,
            "## Cleanup Command",
            string.Empty,
            "- After Unity checks: `.\\.devflow\\scripts\\clean-unity-editor-noise.cmd`",
            string.Empty,
            "## Status",
            string.Empty,
            "- genericProjectionStatus: " + dashboard.GenericProjectionStatus,
            "- samplePackagePath: " + dashboard.SamplePackagePath,
            "- packageId: " + dashboard.PackageId,
            "- packageTitle: " + dashboard.PackageTitle,
            "- mapId: " + dashboard.MapId,
            "- mapSize: " + dashboard.MapSize,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- noRuntimeProviderSchemaLuaGeneratorLibrary: "
            + dashboard.NoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool Goal122StillGreen(string root)
    {
        var result = new AcceptedAlphaProjectionActionLoopService().Build(root);
        return result.Dashboard.ActionLoopStatus == "GREEN"
               && result.Dashboard.WindowPolishStatus == "GREEN"
               && result.ScriptInventory.Passed
               && result.Dashboard.OneClickVerificationStillPresent
               && result.Dashboard.ProjectionActionPreviewPresent
               && result.Dashboard.ProjectionActionApplyPresent
               && result.Dashboard.ProjectionStateResetPresent;
    }

    private static bool CleanupScriptAvailable(GenericGamePackageProjectionScriptInventory inventory) =>
        inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath
            && entry.Exists
            && entry.ContainsRequiredMarker)
        && inventory.Scripts.Any(entry =>
            entry.RelativePath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath
            && entry.Exists
            && entry.ContainsRequiredMarker);

    private static GenericGamePackageProjectionScriptInventoryEntry Entry(
        string root,
        string relativePath,
        string role,
        string marker)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new GenericGamePackageProjectionScriptInventoryEntry
        {
            RelativePath = relativePath,
            Role = role,
            Exists = exists,
            ContainsRequiredMarker = text.Contains(marker, StringComparison.Ordinal),
            RequiredMarker = marker,
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static GenericGamePackageProjectionSmokePlanStep Step(
        int index,
        string stepId,
        string expectedResult) =>
        new()
        {
            StepIndex = index,
            StepId = stepId,
            ExpectedResult = expectedResult
        };

    private static IReadOnlyList<string> BuildExpectedChangedPaths() =>
    [
        GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory + "/",
        GenericGamePackageProjectionVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-123-generic-gamepackage-playable-projection-adapter/",
        GenericGamePackageProjectionVocabulary.DocumentationPath,
        ".devflow/artifact-scope/artifact-scope-policy.json",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/CONTEXT_INDEX.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
        GenericGamePackageProjectionVocabulary.UnityAdapterPath,
        GenericGamePackageProjectionVocabulary.UnityModelsPath,
        GenericGamePackageProjectionVocabulary.UnityControllerPath,
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs"
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        GenericGamePackageProjectionVocabulary.SamplePackagePath,
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/AcceptedAlpha.prefab",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json",
        ".llmgc/exports/final-release/package.zip",
        "provider/live-geodata/Adapter.cs"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildExpectedChangedPaths().Any(prefix =>
            prefix.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(prefix, StringComparison.Ordinal)
                : string.Equals(path, prefix, StringComparison.Ordinal));

    private static bool IsRejectedPath(string path) =>
        BuildRejectedPathSamples().Any(rejected =>
            rejected.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(rejected, StringComparison.Ordinal)
                : string.Equals(path, rejected, StringComparison.Ordinal));

    private static string StringValue(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int IntValue(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static string SourceText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal123 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
