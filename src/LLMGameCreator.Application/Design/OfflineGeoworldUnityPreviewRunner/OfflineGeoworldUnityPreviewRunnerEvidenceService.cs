using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

public sealed partial class OfflineGeoworldUnityPreviewRunnerEvidenceService
{
    private const string Goal100Root =
        ".llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff";
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public OfflineGeoworldUnityPreviewBuildResult Build(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var ledger = BuildStreamingAssetsLedger(root, payload.PayloadFiles);
        var scripts = BuildUnityScriptInventory(root);
        var proof = ValidatePayload(payload.PayloadFiles, payloadReadAttempted: true);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(
            root,
            context,
            payload,
            ledger,
            scripts,
            proof,
            negative,
            binding,
            lineage);
        return BuildResult(payload, ledger, scripts, proof, negative, binding, lineage, quality);
    }

    public async Task<OfflineGeoworldUnityPreviewWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        var context = ReadSourceContext(root);
        var payload = BuildPayload(context);
        var streamingAssetsDirectory = Resolve(
            root,
            OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsRelativeRoot);
        ResetDirectory(root, streamingAssetsDirectory);

        var written = new List<string>();
        foreach (var item in payload.PayloadFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(streamingAssetsDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var mirroredPayload = ReadPayloadFiles(root);
        var mirrored = payload with { PayloadFiles = mirroredPayload };
        var ledger = BuildStreamingAssetsLedger(root, mirroredPayload);
        var scripts = BuildUnityScriptInventory(root);
        var proof = ValidateMirroredPayload(root, mirroredPayload);
        var negative = BuildNegativeProof();
        var binding = BuildWorkspaceBindingInventory(root);
        var lineage = BuildSourceLineage(root, context);
        var quality = BuildQualityGate(
            root,
            context,
            mirrored,
            ledger,
            scripts,
            proof,
            negative,
            binding,
            lineage);
        var result = BuildResult(mirrored, ledger, scripts, proof, negative, binding, lineage, quality);

        var outputDirectory = Resolve(
            root,
            OfflineGeoworldUnityPreviewRunnerVocabulary.RelativeOutputDirectory);
        ResetDirectory(root, outputDirectory);
        foreach (var item in result.PayloadJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.EvidenceJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(outputDirectory, item.Key);
            await File.WriteAllTextAsync(path, item.Value, Utf8WithoutBom, cancellationToken)
                .ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        var reportPath = Path.Combine(
            outputDirectory,
            OfflineGeoworldUnityPreviewRunnerVocabulary.ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, reportPath));

        return new OfflineGeoworldUnityPreviewWriteResult
        {
            Result = result,
            OutputDirectoryPath = outputDirectory,
            StreamingAssetsDirectoryPath = streamingAssetsDirectory,
            ReportMarkdownPath = reportPath,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldUnityPreviewBuildResult BuildResult(
        Goal101Payload payload,
        OfflineGeoworldPreviewStreamingAssetsLedger ledger,
        OfflineGeoworldPreviewUnityScriptInventory scripts,
        OfflineGeoworldPreviewSimulatedCommandProof proof,
        OfflineGeoworldPreviewNegativeProof negative,
        OfflineGeoworldPreviewWorkspaceBindingInventory binding,
        OfflineGeoworldPreviewSourceLineage lineage,
        OfflineGeoworldPreviewQualityGateScan quality)
    {
        var evidence = BuildEvidencePayloads(
            payload,
            ledger,
            scripts,
            proof,
            negative,
            binding,
            lineage,
            quality);
        var reportWithoutHash = BuildReport(payload, ledger, scripts, proof, negative, binding, quality, evidence);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, quality, proof);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Hash(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, quality, proof);
        return new OfflineGeoworldUnityPreviewBuildResult
        {
            Manifest = payload.Manifest,
            CommandCatalog = payload.CommandCatalog,
            StyleLegend = payload.StyleLegend,
            TravelWindowScript = payload.TravelWindowScript,
            Readme = payload.Readme,
            StreamingAssetsLedger = ledger,
            UnityScriptInventory = scripts,
            SimulatedCommandProof = proof,
            NegativeProof = negative,
            WorkspaceBindingInventory = binding,
            SourceLineage = lineage,
            QualityGateScan = quality,
            Report = report,
            ReportMarkdown = reportMarkdown,
            PayloadJsonByFileName = payload.PayloadFiles,
            EvidenceJsonByFileName = evidence
        };
    }

    private static Goal101Payload BuildPayload(Goal101SourceContext context)
    {
        var commands = BuildCommands(context);
        var commandCountByKind = commands
            .GroupBy(item => item.CommandKind, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var commandCatalog = new OfflineGeoworldPreviewFeatureCommandCatalog
        {
            CommandCount = commands.Count,
            CommandKindCount = commandCountByKind.Count,
            ExpectedObjectCount = commands.Sum(item => item.ExpectedObjectCount),
            CommandCountByKind = commandCountByKind,
            Commands = commands
        };
        var styleLegend = BuildStyleLegend();
        var travel = BuildTravelWindowScript(context, commands);
        var readme = new OfflineGeoworldPreviewReadme();
        var commandJson = Serialize(commandCatalog);
        var travelJson = Serialize(travel);
        var styleJson = Serialize(styleLegend);
        var readmeJson = Serialize(readme);
        var manifest = new OfflineGeoworldPreviewRunnerManifest
        {
            PayloadFileCount = OfflineGeoworldUnityPreviewRunnerVocabulary.RequiredPayloadFileNames.Count,
            CommandCount = commandCatalog.CommandCount,
            CommandKindCount = commandCatalog.CommandKindCount,
            TravelWindowStepCount = travel.StepCount,
            StyleCount = styleLegend.StyleCount,
            ExpectedObjectCount = commandCatalog.ExpectedObjectCount,
            FeatureCommandsHash = Hash(commandJson),
            TravelWindowScriptHash = Hash(travelJson),
            StyleLegendHash = Hash(styleJson),
            ReadmeHash = Hash(readmeJson)
        };

        return new Goal101Payload(
            manifest,
            commandCatalog,
            styleLegend,
            travel,
            readme,
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                [OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestFileName] = Serialize(manifest),
                [OfflineGeoworldUnityPreviewRunnerVocabulary.FeatureCommandsFileName] = commandJson,
                [OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowScriptFileName] = travelJson,
                [OfflineGeoworldUnityPreviewRunnerVocabulary.StyleLegendFileName] = styleJson,
                [OfflineGeoworldUnityPreviewRunnerVocabulary.ReadmeFileName] = readmeJson
            });
    }

    private static IReadOnlyList<OfflineGeoworldPreviewFeatureCommand> BuildCommands(
        Goal101SourceContext context)
    {
        var result = new List<OfflineGeoworldPreviewFeatureCommand>();
        var index = 0;
        foreach (var record in context.Records.OrderBy(item => item.RecordId, StringComparer.Ordinal))
        {
            var commandKind = MapCommandKind(record.FeatureKind);
            var commandHash = Hash(string.Join(
                "|",
                record.RecordId,
                commandKind,
                record.SourceFeatureId,
                record.SourceChunkKey));
            result.Add(new OfflineGeoworldPreviewFeatureCommand
            {
                CommandId = "preview_command/" + commandHash[..16],
                CommandKind = commandKind,
                SourceCacheRecordId = record.RecordId,
                SourceFeatureId = record.SourceFeatureId,
                SourceFeatureKind = record.FeatureKind,
                SourceChunkKey = record.SourceChunkKey,
                VisualChunkKey = record.VisualChunkKey,
                VisualLayerId = record.VisualLayerId,
                StyleKey = commandKind,
                GridX = (index % 6) * 3,
                GridZ = (index / 6) * 3,
                Elevation = CommandElevation(commandKind),
                ExpectedObjectCount = 1,
                MetadataOnly = true,
                RawGeodataIncluded = false,
                SafeRatingMetadataStatus = "safe_public_geoworld_fallback"
            });
            index++;
        }

        return result.OrderBy(item => item.CommandId, StringComparer.Ordinal).ToList();
    }

    private static OfflineGeoworldPreviewStyleLegend BuildStyleLegend()
    {
        var styles = new[]
        {
            Style("administrative_hint_marker", "cube", "#8a8a8a", 1.2m, 0.2m, 1.2m, 0.02m),
            Style("barrier_line", "line", "#6b4a2b", 1.0m, 0.08m, 1.0m, 0.08m),
            Style("bridge_marker", "cube", "#a87c45", 1.4m, 0.18m, 0.8m, 0.04m),
            Style("building_footprint_marker", "cube", "#65737e", 1.2m, 0.8m, 1.2m, 0.03m),
            Style("land_use_area_plane", "plane", "#9fbf7f", 1.8m, 0.04m, 1.8m, 0.03m),
            Style("poi_marker", "sphere", "#d6a329", 0.7m, 0.7m, 0.7m, 0.03m),
            Style("road_segment_line", "line", "#c9b36a", 1.0m, 0.08m, 1.0m, 0.1m),
            Style("terrain_hint_marker", "capsule", "#8d8f63", 0.9m, 0.5m, 0.9m, 0.03m),
            Style("vegetation_area_marker", "sphere", "#4f8f4f", 0.8m, 0.6m, 0.8m, 0.03m),
            Style("water_body_plane", "plane", "#4f89b8", 1.8m, 0.03m, 1.8m, 0.03m)
        };
        return new OfflineGeoworldPreviewStyleLegend
        {
            StyleCount = styles.Length,
            Styles = styles.OrderBy(item => item.CommandKind, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldPreviewStyleLegendEntry Style(
        string commandKind,
        string primitiveHint,
        string colorHex,
        decimal scaleX,
        decimal scaleY,
        decimal scaleZ,
        decimal lineWidth) =>
        new()
        {
            StyleKey = commandKind,
            CommandKind = commandKind,
            PrimitiveHint = primitiveHint,
            ColorHex = colorHex,
            ScaleX = scaleX,
            ScaleY = scaleY,
            ScaleZ = scaleZ,
            LineWidth = lineWidth
        };

    private static OfflineGeoworldPreviewTravelWindowScript BuildTravelWindowScript(
        Goal101SourceContext context,
        IReadOnlyList<OfflineGeoworldPreviewFeatureCommand> commands)
    {
        var orderedKinds = commands.Select(item => item.CommandKind)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
        var orderedIds = commands.Select(item => item.CommandId)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
        var chunks = commands.Select(item => item.SourceChunkKey)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
        var center = string.IsNullOrWhiteSpace(context.CenterChunkKey)
            ? chunks.FirstOrDefault() ?? string.Empty
            : context.CenterChunkKey;
        var steps = new[]
        {
            Step(0, "load_center_window", "load_manifest", center, orderedKinds.Take(4), orderedIds.Take(6)),
            Step(1, "spawn_feature_placeholders", "spawn_commands", center, orderedKinds, orderedIds),
            Step(2, "travel_to_boundary_window", "shift_window", chunks.LastOrDefault() ?? center,
                orderedKinds.Skip(3), orderedIds.Skip(6).Take(8)),
            Step(3, "reset_preview_window", "reset_and_replay", center, orderedKinds, orderedIds.Take(10))
        };
        return new OfflineGeoworldPreviewTravelWindowScript
        {
            StepCount = steps.Length,
            CommandCoverageCount = orderedIds.Count,
            Steps = steps
        };
    }

    private static OfflineGeoworldPreviewTravelWindowStep Step(
        int index,
        string id,
        string action,
        string centerChunk,
        IEnumerable<string> kinds,
        IEnumerable<string> ids) =>
        new()
        {
            StepIndex = index,
            StepId = id,
            Action = action,
            CenterChunkKey = centerChunk,
            VisibleCommandKinds = kinds.OrderBy(value => value, StringComparer.Ordinal).ToList(),
            VisibleCommandIds = ids.OrderBy(value => value, StringComparer.Ordinal).ToList()
        };

    private static Goal101SourceContext ReadSourceContext(string root)
    {
        var diagnostics = new List<OfflineGeoworldUnityPreviewDiagnostic>();
        using var catalog = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-visual-cache-catalog.json",
            diagnostics);
        using var ledger = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-feature-chunk-ledger.json",
            diagnostics);
        using var manifest = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-unity-handoff-manifest.json",
            diagnostics);
        using var readProof = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-unity-simulated-read-proof.json",
            diagnostics);
        using var negative = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-negative-proof.json",
            diagnostics);
        using var quality = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-quality-gate-scan.json",
            diagnostics);
        using var stream = ReadJson(
            root,
            Goal100Root + "/offline-geoworld-stream-window-index.json",
            diagnostics);

        var records = ledger is null ? [] : ReadRecords(ledger.RootElement);
        var report = ReadOptionalText(
            root,
            Goal100Root + "/offline-geoworld-visual-cache-unity-handoff-report.md");
        var acceptedFalse = report.Contains("- accepted: false", StringComparison.OrdinalIgnoreCase)
                            || (manifest is not null && !TryGetBool(manifest.RootElement, "accepted"));
        var countsOk = manifest is not null
                       && TryGetInt(manifest.RootElement, "packageCount") == 3
                       && TryGetInt(manifest.RootElement, "visualCacheRecordCount") == 18
                       && TryGetInt(manifest.RootElement, "payloadFileCount") == 5;
        var readPassed = readProof is not null && TryGetBool(readProof.RootElement, "passed");
        var negativePassed = negative is not null && TryGetBool(negative.RootElement, "passed");
        var qualityPassed = quality is not null && TryGetBool(quality.RootElement, "passed");
        var alphaUnchanged = quality is not null
            && TryGetBool(quality.RootElement, "alphaRuntimeBootstrapUnchanged");

        AddIfFalse(acceptedFalse, "goal101.source.goal100_accepted", Goal100Root, diagnostics);
        AddIfFalse(records.Count == 18, "goal101.source.record_count", Goal100Root, diagnostics);
        AddIfFalse(countsOk, "goal101.source.goal100_counts", Goal100Root, diagnostics);
        AddIfFalse(readPassed, "goal101.source.goal100_read_proof", Goal100Root, diagnostics);
        AddIfFalse(negativePassed, "goal101.source.goal100_negative", Goal100Root, diagnostics);
        AddIfFalse(qualityPassed, "goal101.source.goal100_quality", Goal100Root, diagnostics);
        AddIfFalse(alphaUnchanged, "goal101.source.goal100_alpha", Goal100Root, diagnostics);
        _ = catalog;

        return new Goal101SourceContext(
            records,
            stream is null ? string.Empty : TryGetString(stream.RootElement, "centerChunkKey"),
            acceptedFalse,
            countsOk,
            readPassed,
            negativePassed,
            qualityPassed,
            alphaUnchanged,
            SortDiagnostics(diagnostics));
    }

    private static IReadOnlyList<Goal100CacheRecord> ReadRecords(JsonElement root)
    {
        if (!root.TryGetProperty("records", out var records)
            || records.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return records.EnumerateArray()
            .Select(item => new Goal100CacheRecord(
                TryGetString(item, "recordId"),
                TryGetString(item, "sourceFeatureId"),
                TryGetString(item, "featureKind"),
                TryGetString(item, "sourceChunkKey"),
                TryGetString(item, "visualChunkKey"),
                TryGetString(item, "visualLayerId")))
            .Where(item => !string.IsNullOrWhiteSpace(item.RecordId))
            .OrderBy(item => item.RecordId, StringComparer.Ordinal)
            .ToList();
    }

    private static string MapCommandKind(string featureKind) =>
        featureKind switch
        {
            "administrativeHint" => "administrative_hint_marker",
            "barrier" => "barrier_line",
            "bridge" => "bridge_marker",
            "buildingFootprint" => "building_footprint_marker",
            "landUse" => "land_use_area_plane",
            "poi" => "poi_marker",
            "roadSegment" => "road_segment_line",
            "terrainHint" => "terrain_hint_marker",
            "vegetation" => "vegetation_area_marker",
            "waterBody" => "water_body_plane",
            _ => "unsupported"
        };

    private static int CommandElevation(string commandKind) =>
        commandKind switch
        {
            "bridge_marker" => 2,
            "building_footprint_marker" => 1,
            "poi_marker" => 2,
            "terrain_hint_marker" => 1,
            "vegetation_area_marker" => 1,
            _ => 0
        };

    private sealed record Goal101Payload(
        OfflineGeoworldPreviewRunnerManifest Manifest,
        OfflineGeoworldPreviewFeatureCommandCatalog CommandCatalog,
        OfflineGeoworldPreviewStyleLegend StyleLegend,
        OfflineGeoworldPreviewTravelWindowScript TravelWindowScript,
        OfflineGeoworldPreviewReadme Readme,
        IReadOnlyDictionary<string, string> PayloadFiles);

    private sealed record Goal101SourceContext(
        IReadOnlyList<Goal100CacheRecord> Records,
        string CenterChunkKey,
        bool Goal100AcceptedFalse,
        bool Goal100CountsProven,
        bool Goal100SimulatedReadProofPassed,
        bool Goal100NegativeProofPassed,
        bool Goal100QualityGatePassed,
        bool Goal100AlphaRuntimeBootstrapUnchanged,
        IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics);

    private sealed record Goal100CacheRecord(
        string RecordId,
        string SourceFeatureId,
        string FeatureKind,
        string SourceChunkKey,
        string VisualChunkKey,
        string VisualLayerId);
}
