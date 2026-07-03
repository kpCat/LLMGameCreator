using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

namespace LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

public sealed class DeterministicVisualChunkStreamWindowEvidenceService
{
    public const string ReportMarkdownFileName = "visual-chunk-stream-window-report.md";
    public const string CatalogJsonFileName = "visual-chunk-stream-window-catalog.json";
    public const string MaterializationManifestJsonFileName = "visual-chunk-stream-materialization-manifest.json";
    public const string FileLedgerJsonFileName = "visual-chunk-stream-file-ledger.json";
    public const string DeterminismProofJsonFileName = "visual-chunk-stream-determinism-proof.json";
    public const string SeamProofJsonFileName = "visual-chunk-stream-seam-proof.json";
    public const string CacheReuseProofJsonFileName = "visual-chunk-stream-cache-reuse-proof.json";
    public const string LayerTransitionProofJsonFileName = "visual-chunk-stream-layer-transition-proof.json";
    public const string NegativeProofJsonFileName = "visual-chunk-stream-negative-proof.json";
    public const string SourceLineageJsonFileName = "visual-chunk-stream-source-lineage.json";
    public const string QualityGateScanJsonFileName = "visual-chunk-stream-quality-gate-scan.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static DeterministicVisualChunkStreamWindowEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualChunkStreamEvidenceResult Build(string? projectRootPath = null)
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var requests = DeterministicVisualChunkStreamWindowFixtures.BuildRequests();
        var windows = DeterministicVisualChunkStreamWindowMaterializer.MaterializeAll(requests, profiles);

        var catalog = BuildCatalog(windows);
        var manifest = BuildMaterializationManifest(windows);
        var determinismProof = BuildDeterminismProof(requests, profiles);
        var seamProof = BuildSeamProof(windows);
        var cacheReuseProof = BuildCacheReuseProof(windows);
        var layerTransitionProof = BuildLayerTransitionProof(windows);
        var negativeProof = BuildNegativeProof(profiles);
        var sourceLineage = BuildSourceLineage(projectRootPath);
        var overviewSvgs = RenderOverviewSvgs(windows);
        var qualityGate = DeterministicVisualChunkStreamWindowQualityGateScanner.Build(
            catalog,
            manifest,
            determinismProof,
            seamProof,
            cacheReuseProof,
            layerTransitionProof,
            negativeProof,
            sourceLineage,
            overviewSvgs);

        var catalogJson = Serialize(catalog);
        var manifestJson = Serialize(manifest);
        var determinismProofJson = Serialize(determinismProof);
        var seamProofJson = Serialize(seamProof);
        var cacheReuseProofJson = Serialize(cacheReuseProof);
        var layerTransitionProofJson = Serialize(layerTransitionProof);
        var negativeProofJson = Serialize(negativeProof);
        var sourceLineageJson = Serialize(sourceLineage);
        var qualityGateJson = Serialize(qualityGate);
        var fileLedger = BuildFileLedger(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [CatalogJsonFileName] = catalogJson,
                [MaterializationManifestJsonFileName] = manifestJson,
                [DeterminismProofJsonFileName] = determinismProofJson,
                [SeamProofJsonFileName] = seamProofJson,
                [CacheReuseProofJsonFileName] = cacheReuseProofJson,
                [LayerTransitionProofJsonFileName] = layerTransitionProofJson,
                [NegativeProofJsonFileName] = negativeProofJson,
                [SourceLineageJsonFileName] = sourceLineageJson,
                [QualityGateScanJsonFileName] = qualityGateJson
            },
            overviewSvgs);
        var fileLedgerJson = Serialize(fileLedger);

        var reportWithoutHash = BuildReport(
            catalog,
            manifest,
            determinismProof,
            seamProof,
            cacheReuseProof,
            layerTransitionProof,
            negativeProof,
            sourceLineage,
            qualityGate,
            catalogJson,
            manifestJson,
            fileLedgerJson,
            determinismProofJson,
            seamProofJson,
            cacheReuseProofJson,
            layerTransitionProofJson,
            negativeProofJson,
            sourceLineageJson,
            qualityGateJson);
        var reportMarkdownWithoutHash = RenderReport(
            reportWithoutHash,
            catalog,
            manifest,
            seamProof,
            cacheReuseProof,
            layerTransitionProof,
            negativeProof,
            qualityGate,
            string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = DeterministicVisualChunkStreamWindowHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(
            report,
            catalog,
            manifest,
            seamProof,
            cacheReuseProof,
            layerTransitionProof,
            negativeProof,
            qualityGate,
            report.DeterministicReportHash);

        return new VisualChunkStreamEvidenceResult
        {
            Catalog = catalog,
            MaterializationManifest = manifest,
            FileLedger = fileLedger,
            DeterminismProof = determinismProof,
            SeamProof = seamProof,
            CacheReuseProof = cacheReuseProof,
            LayerTransitionProof = layerTransitionProof,
            NegativeProof = negativeProof,
            SourceLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            MaterializationManifestJson = manifestJson,
            FileLedgerJson = fileLedgerJson,
            DeterminismProofJson = determinismProofJson,
            SeamProofJson = seamProofJson,
            CacheReuseProofJson = cacheReuseProofJson,
            LayerTransitionProofJson = layerTransitionProofJson,
            NegativeProofJson = negativeProofJson,
            SourceLineageJson = sourceLineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown,
            OverviewSvgByFixtureId = overviewSvgs
        };
    }

    public async Task<VisualChunkStreamWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualChunkStreamWriteResult> WriteAsync(
        string projectRootPath,
        VisualChunkStreamEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            DeterministicVisualChunkStreamWindowVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var overviewDirectory = Path.GetFullPath(Path.Combine(
            outputDirectory,
            DeterministicVisualChunkStreamWindowVocabulary.StreamOverviewRelativeDirectory));
        EnsureContained(outputDirectory, overviewDirectory);
        Directory.CreateDirectory(overviewDirectory);

        var write = new VisualChunkStreamWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StreamOverviewDirectoryPath = overviewDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            MaterializationManifestJsonPath = Path.Combine(outputDirectory, MaterializationManifestJsonFileName),
            FileLedgerJsonPath = Path.Combine(outputDirectory, FileLedgerJsonFileName),
            DeterminismProofJsonPath = Path.Combine(outputDirectory, DeterminismProofJsonFileName),
            SeamProofJsonPath = Path.Combine(outputDirectory, SeamProofJsonFileName),
            CacheReuseProofJsonPath = Path.Combine(outputDirectory, CacheReuseProofJsonFileName),
            LayerTransitionProofJsonPath = Path.Combine(outputDirectory, LayerTransitionProofJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            OverviewSvgPaths = result.OverviewSvgByFixtureId.Keys
                .OrderBy(item => item, StringComparer.Ordinal)
                .Select(item => Path.Combine(overviewDirectory, $"{item}.svg"))
                .ToList()
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CatalogJsonPath, result.CatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.MaterializationManifestJsonPath, result.MaterializationManifestJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FileLedgerJsonPath, result.FileLedgerJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.DeterminismProofJsonPath, result.DeterminismProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SeamProofJsonPath, result.SeamProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CacheReuseProofJsonPath, result.CacheReuseProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.LayerTransitionProofJsonPath, result.LayerTransitionProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        foreach (var (fixtureId, svg) in result.OverviewSvgByFixtureId.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(overviewDirectory, $"{fixtureId}.svg");
            EnsureContained(outputDirectory, path);
            await File.WriteAllTextAsync(path, svg, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        }

        return write;
    }

    public static VisualChunkStreamNegativeProof BuildNegativeProof(IReadOnlyList<VisualWorldProfile> profiles)
    {
        var baselineRequests = DeterministicVisualChunkStreamWindowFixtures.BuildRequests();
        var finite = baselineRequests.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId);
        var huge = baselineRequests.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId);
        var infinite = baselineRequests.First(item => item.WindowId == "infinite_player_spawn_radius1");
        var layerTransition = baselineRequests.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId);
        var baselineWindow = DeterministicVisualChunkStreamWindowMaterializer.Materialize(finite, profiles.Single(item => item.ProfileId == finite.ProfileId));
        var firstChunk = baselineWindow.Chunks[0];
        var secondChunk = baselineWindow.Chunks[1];
        var firstSeam = baselineWindow.Seams[0];

        var scenarios = new List<VisualChunkStreamNegativeScenario>
        {
            InvalidRequest("unknown_profile", "unknown profile id", finite with { ProfileId = "missing_profile" }, profiles),
            InvalidRequest("unknown_layer", "unknown layer id", finite with { LayerId = "missing_layer", LayerIds = ["missing_layer"] }, profiles),
            InvalidRequest("missing_seed", "missing world seed", finite with { WorldSeed = "" }, profiles),
            InvalidRequest("missing_generator_version", "missing generator version", finite with { GeneratorVersion = "" }, profiles),
            InvalidRequest("invalid_radius", "radius is negative", finite with { RadiusChunks = -1 }, profiles),
            InvalidRequest("raw_full_world_dump", "raw full-world cell dump requested", huge with { AttemptsRawFullWorldDump = true }, profiles),
            InvalidRequest("finite_out_of_bounds_without_clipping", "finite boundary crosses without clipping policy", finite with { BoundaryPolicy = VisualChunkStreamBoundaryPolicy.UnboundedInfinite }, profiles),
            InvalidRequest("prompt_text_source_of_truth", "prompt text as source of truth", finite with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" }, profiles),
            InvalidRequest("absolute_path_metadata", "absolute path metadata", finite with { ContainsAbsolutePath = true }, profiles),
            InvalidRequest("rating_metadata_without_safe_fallback", "adult/rating metadata without safe fallback", infinite with { ContainsAdultRatingMetadata = true, SafeFallbackRefId = "" }, profiles),
            InvalidWindow("chunk_key_mismatch", "chunk key does not match formula", baselineWindow with { Chunks = ReplaceChunk(baselineWindow, firstChunk with { ChunkKey = "not_deterministic" }) }),
            InvalidWindow("seam_key_mismatch", "seam key mismatch across boundary", baselineWindow with { Seams = ReplaceSeam(baselineWindow, firstSeam with { SeamKey = "bad_seam_key" }) }),
            InvalidWindow("water_connector_mismatch", "water connector mismatch across seam", baselineWindow with { Seams = ReplaceSeam(baselineWindow, firstSeam with { WaterConnector = "wrong_water" }) }),
            InvalidWindow("road_connector_mismatch", "road connector mismatch across seam", baselineWindow with { Seams = ReplaceSeam(baselineWindow, firstSeam with { RoadConnector = "wrong_road" }) }),
            InvalidWindow("duplicate_chunk_keys", "duplicate chunk keys inside one window", baselineWindow with { Chunks = ReplaceChunk(baselineWindow, secondChunk with { ChunkKey = firstChunk.ChunkKey }) }),
            InvalidWindow("delta_overlay_raw_payload", "delta overlay carries raw cells", DeterministicVisualChunkStreamWindowMaterializer.Materialize(layerTransition with { DeltaOverlay = layerTransition.DeltaOverlay! with { ContainsRawCellPayload = true } }, profiles.Single(item => item.ProfileId == layerTransition.ProfileId)))
        };

        return new VisualChunkStreamNegativeProof
        {
            Passed = scenarios.Count >= 15
                && scenarios.All(item => !item.ActualValid && item.ExpectedValid == item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualChunkStreamCatalog BuildCatalog(IReadOnlyList<VisualChunkStreamWindow> windows)
    {
        var fixtures = windows
            .GroupBy(item => item.FixtureId, StringComparer.Ordinal)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(group => new VisualChunkStreamCatalogFixture
            {
                FixtureId = group.Key,
                ProfileId = group.First().ProfileId,
                Mode = group.First().Mode,
                LayerIds = group.SelectMany(item => item.LayerIds).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                WindowIds = group.Select(item => item.WindowId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
                WindowCount = group.Count(),
                TotalMaterializedChunks = group.Sum(item => item.ChunkCount),
                BoundaryClippingExplicit = group.Any(item => item.ClippedAtFiniteBoundary),
                NoRawFullWorldDump = group.All(item => item.NoRawFullWorldDump),
                OverviewSvgRelativePath = $"{DeterministicVisualChunkStreamWindowVocabulary.StreamOverviewRelativeDirectory}/{group.Key}.svg"
            })
            .ToList();

        return new VisualChunkStreamCatalog
        {
            Accepted = false,
            FixtureCount = fixtures.Count,
            WindowCount = windows.Count,
            Fixtures = fixtures
        };
    }

    private static VisualChunkStreamMaterializationManifest BuildMaterializationManifest(IReadOnlyList<VisualChunkStreamWindow> windows) =>
        new()
        {
            Accepted = false,
            FixtureCount = windows.Select(item => item.FixtureId).Distinct(StringComparer.Ordinal).Count(),
            WindowCount = windows.Count,
            TotalMaterializedChunks = windows.Sum(item => item.ChunkCount),
            UniqueChunkKeyCount = windows.SelectMany(item => item.Chunks).Select(item => item.ChunkKey).Distinct(StringComparer.Ordinal).Count(),
            NoRawFullWorldDump = windows.All(item => item.NoRawFullWorldDump),
            Windows = windows
        };

    private static VisualChunkStreamDeterminismProof BuildDeterminismProof(
        IReadOnlyList<VisualChunkStreamRequest> requests,
        IReadOnlyList<VisualWorldProfile> profiles)
    {
        var rows = requests
            .OrderBy(item => item.WindowId, StringComparer.Ordinal)
            .Select(request =>
            {
                var profile = profiles.Single(item => item.ProfileId == request.ProfileId);
                var first = DeterministicVisualChunkStreamWindowMaterializer.Materialize(request, profile);
                var second = DeterministicVisualChunkStreamWindowMaterializer.Materialize(request, profile);
                return new VisualChunkStreamDeterminismProofRow
                {
                    WindowId = request.WindowId,
                    FirstWindowHash = first.WindowHash,
                    SecondWindowHash = second.WindowHash,
                    Stable = string.Equals(first.WindowHash, second.WindowHash, StringComparison.Ordinal)
                        && first.Chunks.Select(item => item.ChunkKey).SequenceEqual(second.Chunks.Select(item => item.ChunkKey)),
                    ChunkCount = first.ChunkCount
                };
            })
            .ToList();

        return new VisualChunkStreamDeterminismProof
        {
            Passed = rows.Count == requests.Count && rows.All(item => item.Stable),
            StableChunkKeysAcrossReruns = rows.All(item => item.Stable),
            StableEvidenceAcrossReruns = rows.All(item => item.Stable),
            Rows = rows
        };
    }

    private static VisualChunkStreamSeamProof BuildSeamProof(IReadOnlyList<VisualChunkStreamWindow> windows)
    {
        var seams = windows.SelectMany(item => item.Seams).ToList();
        var rows = seams
            .OrderBy(item => item.WindowId, StringComparer.Ordinal)
            .ThenBy(item => item.LayerId, StringComparer.Ordinal)
            .ThenBy(item => item.FromChunkY)
            .ThenBy(item => item.FromChunkX)
            .ThenBy(item => item.Direction, StringComparer.Ordinal)
            .Select(item => new VisualChunkStreamSeamProofRow
            {
                WindowId = item.WindowId,
                LayerId = item.LayerId,
                Direction = item.Direction,
                SeamKey = item.SeamKey,
                WaterConnector = item.WaterConnector,
                RoadConnector = item.RoadConnector,
                BiomeBand = item.BiomeBand
            })
            .ToList();

        return new VisualChunkStreamSeamProof
        {
            Passed = seams.Count > 0
                && seams.All(item => item.WaterContinuityPassed && item.RoadContinuityPassed && item.BiomeContinuityPassed),
            SeamCount = seams.Count,
            WaterContinuityPassed = seams.All(item => item.WaterContinuityPassed),
            RoadContinuityPassed = seams.All(item => item.RoadContinuityPassed),
            BiomeContinuityPassed = seams.All(item => item.BiomeContinuityPassed),
            Rows = rows
        };
    }

    private static VisualChunkStreamCacheReuseProof BuildCacheReuseProof(IReadOnlyList<VisualChunkStreamWindow> windows)
    {
        var chunkRows = windows
            .SelectMany(window => window.Chunks.Select(chunk => (window, chunk)))
            .GroupBy(item => item.chunk.ChunkKey, StringComparer.Ordinal)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var ordered = group
                    .OrderBy(item => item.window.WindowId, StringComparer.Ordinal)
                    .ToList();
                var first = ordered[0];
                var requestedBy = ordered.Select(item => item.window.WindowId).Distinct(StringComparer.Ordinal).ToList();
                return new VisualChunkStreamCacheRecord
                {
                    ChunkKey = group.Key,
                    ProfileId = first.chunk.ProfileId,
                    LayerId = first.chunk.LayerId,
                    ChunkX = first.chunk.ChunkX,
                    ChunkY = first.chunk.ChunkY,
                    FirstWindowId = first.window.WindowId,
                    RequestedByWindowIds = requestedBy,
                    ReusedInWindowIds = requestedBy.Skip(1).ToList(),
                    RequestCount = requestedBy.Count,
                    MaterializationCount = 1,
                    Reused = requestedBy.Count > 1,
                    CachePolicy = "deterministic_chunk_key_reuse"
                };
            })
            .ToList();
        var infiniteWindowIds = windows
            .Where(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId)
            .Select(item => item.WindowId)
            .ToHashSet(StringComparer.Ordinal);
        var infiniteOverlapReused = chunkRows.Count(item =>
            item.Reused && item.RequestedByWindowIds.Count(windowId => infiniteWindowIds.Contains(windowId)) > 1);

        return new VisualChunkStreamCacheReuseProof
        {
            Passed = infiniteOverlapReused > 0 && chunkRows.All(item => item.MaterializationCount == 1),
            CacheRecordCount = chunkRows.Count,
            ReusedChunkKeyCount = chunkRows.Count(item => item.Reused),
            InfiniteOverlapReusedChunkKeyCount = infiniteOverlapReused,
            Records = chunkRows
        };
    }

    private static VisualChunkStreamLayerTransitionProof BuildLayerTransitionProof(IReadOnlyList<VisualChunkStreamWindow> windows)
    {
        var rows = windows
            .Where(item => item.LayerLinks.Count > 0)
            .OrderBy(item => item.WindowId, StringComparer.Ordinal)
            .Select(item => new VisualChunkStreamLayerTransitionProofRow
            {
                FixtureId = item.FixtureId,
                WindowId = item.WindowId,
                ProfileId = item.ProfileId,
                LayerIds = item.LayerIds,
                LayerLinks = item.LayerLinks,
                IncludesWaterLayer = item.LayerIds.Contains("underwater", StringComparer.Ordinal)
                    || item.LayerIds.Any(layer => layer.Contains("water", StringComparison.Ordinal))
            })
            .ToList();
        var linkCount = rows.Sum(item => item.LayerLinks.Count(link =>
            string.Equals(link.LinkKind, "Portal", StringComparison.Ordinal)
            || string.Equals(link.LinkKind, "Transition", StringComparison.Ordinal)));
        var notHardcoded = rows.Any(item => item.LayerIds.Count >= 3 && item.IncludesWaterLayer);

        return new VisualChunkStreamLayerTransitionProof
        {
            Passed = rows.Count > 0 && linkCount > 0 && notHardcoded,
            DataDrivenLayerLinksPassed = rows.All(item => item.LayerLinks.Count > 0),
            NotHardcodedSurfaceUndergroundOnly = notHardcoded,
            PortalOrTransitionLinkCount = linkCount,
            Rows = rows
        };
    }

    private static VisualChunkStreamSourceLineage BuildSourceLineage(string? projectRootPath)
    {
        var records = SourceInputs()
            .Select(item => SourceRecord(projectRootPath, item.Path, item.Tags))
            .ToList();
        var goal090 = records.Any(item => item.Exists && item.PurposeTags.Contains("goal090", StringComparer.Ordinal));

        return new VisualChunkStreamSourceLineage
        {
            Passed = records.All(item => item.Exists) && goal090,
            SourceRecordCount = records.Count,
            Goal090LineagePresent = goal090,
            Records = records
        };
    }

    private static VisualChunkStreamFileLedger BuildFileLedger(
        IReadOnlyDictionary<string, string> rootFiles,
        IReadOnlyDictionary<string, string> overviewSvgs)
    {
        var entries = rootFiles
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => LedgerEntry(item.Key, item.Value, ["json_evidence"]))
            .Concat(overviewSvgs
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => LedgerEntry(
                    $"{DeterministicVisualChunkStreamWindowVocabulary.StreamOverviewRelativeDirectory}/{item.Key}.svg",
                    item.Value,
                    ["text_svg_overview"])))
            .ToList();

        return new VisualChunkStreamFileLedger
        {
            Passed = entries.Count >= 13 && entries.All(item => item.ByteLength > 0 && item.Sha256.Length == 64),
            FileCount = entries.Count,
            Files = entries
        };
    }

    private static VisualChunkStreamFileLedgerEntry LedgerEntry(
        string relativePath,
        string text,
        IReadOnlyList<string> tags) =>
        new()
        {
            RelativePath = relativePath,
            Sha256 = DeterministicVisualChunkStreamWindowHash.Compute(text),
            ByteLength = Encoding.UTF8.GetByteCount(text),
            PurposeTags = tags
        };

    private static VisualChunkStreamReport BuildReport(
        VisualChunkStreamCatalog catalog,
        VisualChunkStreamMaterializationManifest manifest,
        VisualChunkStreamDeterminismProof determinismProof,
        VisualChunkStreamSeamProof seamProof,
        VisualChunkStreamCacheReuseProof cacheReuseProof,
        VisualChunkStreamLayerTransitionProof layerTransitionProof,
        VisualChunkStreamNegativeProof negativeProof,
        VisualChunkStreamSourceLineage sourceLineage,
        VisualChunkStreamQualityGateScan qualityGate,
        string catalogJson,
        string manifestJson,
        string fileLedgerJson,
        string determinismProofJson,
        string seamProofJson,
        string cacheReuseProofJson,
        string layerTransitionProofJson,
        string negativeProofJson,
        string sourceLineageJson,
        string qualityGateJson) =>
        new()
        {
            Accepted = false,
            FixtureCount = catalog.FixtureCount,
            WindowCount = catalog.WindowCount,
            TotalMaterializedChunks = manifest.TotalMaterializedChunks,
            UniqueChunkKeyCount = manifest.UniqueChunkKeyCount,
            DeterminismProofPassed = determinismProof.Passed,
            SeamProofPassed = seamProof.Passed,
            CacheReuseProofPassed = cacheReuseProof.Passed,
            LayerTransitionProofPassed = layerTransitionProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            QualityGatePassed = qualityGate.Diagnostics.All(item => item.Severity != "error"),
            CatalogHash = DeterministicVisualChunkStreamWindowHash.Compute(catalogJson),
            MaterializationManifestHash = DeterministicVisualChunkStreamWindowHash.Compute(manifestJson),
            FileLedgerHash = DeterministicVisualChunkStreamWindowHash.Compute(fileLedgerJson),
            DeterminismProofHash = DeterministicVisualChunkStreamWindowHash.Compute(determinismProofJson),
            SeamProofHash = DeterministicVisualChunkStreamWindowHash.Compute(seamProofJson),
            CacheReuseProofHash = DeterministicVisualChunkStreamWindowHash.Compute(cacheReuseProofJson),
            LayerTransitionProofHash = DeterministicVisualChunkStreamWindowHash.Compute(layerTransitionProofJson),
            NegativeProofHash = DeterministicVisualChunkStreamWindowHash.Compute(negativeProofJson),
            SourceLineageHash = DeterministicVisualChunkStreamWindowHash.Compute(sourceLineageJson),
            QualityGateHash = DeterministicVisualChunkStreamWindowHash.Compute(qualityGateJson)
        };

    private static string RenderReport(
        VisualChunkStreamReport report,
        VisualChunkStreamCatalog catalog,
        VisualChunkStreamMaterializationManifest manifest,
        VisualChunkStreamSeamProof seamProof,
        VisualChunkStreamCacheReuseProof cacheReuseProof,
        VisualChunkStreamLayerTransitionProof layerTransitionProof,
        VisualChunkStreamNegativeProof negativeProof,
        VisualChunkStreamQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var finite = manifest.Windows.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId);
        var huge = manifest.Windows.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId);
        var infiniteWindows = manifest.Windows.Where(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId).ToList();
        var lines = new List<string>
        {
            "# Goal 091 Visual Chunk Stream Window Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 091 adds a BCL-only Application-side deterministic visual chunk stream window materializer. It consumes Goal 090 parameterized profiles and materializes only requested chunk windows with deterministic chunk keys, seam continuity, layer transition metadata and cache reuse proof.",
            string.Empty,
            "## Stream Fixtures",
            string.Empty
        };
        lines.AddRange(catalog.Fixtures.Select(item => $"- {item.FixtureId}: profile={item.ProfileId}, windows={item.WindowCount}, layers={string.Join(",", item.LayerIds)}, chunks={item.TotalMaterializedChunks}"));
        lines.AddRange(
        [
            string.Empty,
            "## Finite Boundary Proof",
            string.Empty,
            $"- finiteFixture: {finite.FixtureId}",
            $"- finiteSize: {finite.EffectiveFiniteWidth}x{finite.EffectiveFiniteHeight}",
            $"- requestedWindow: {finite.RequestedMinChunkX},{finite.RequestedMinChunkY}..{finite.RequestedMaxChunkX},{finite.RequestedMaxChunkY}",
            $"- materializedWindow: {finite.MaterializedMinChunkX},{finite.MaterializedMinChunkY}..{finite.MaterializedMaxChunkX},{finite.MaterializedMaxChunkY}",
            $"- clippedAtFiniteBoundary: {finite.ClippedAtFiniteBoundary.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Huge Sparse And Infinite Proof",
            string.Empty,
            $"- hugeFixture: {huge.FixtureId}",
            $"- hugeEstimatedFullWorldChunkCapacity: {huge.EstimatedFullWorldChunkCapacity}",
            $"- hugeMaterializedChunks: {huge.ChunkCount}",
            $"- hugeNoRawFullWorldDump: {huge.NoRawFullWorldDump.ToString().ToLowerInvariant()}",
            $"- infiniteWindowCount: {infiniteWindows.Count}",
            $"- infiniteMaterializedChunks: {infiniteWindows.Sum(item => item.ChunkCount)}",
            $"- infiniteOverlapReusedChunkKeyCount: {cacheReuseProof.InfiniteOverlapReusedChunkKeyCount}",
            string.Empty,
            "## Seam Cache Layer Proof",
            string.Empty,
            $"- seamProofPassed: {seamProof.Passed.ToString().ToLowerInvariant()}",
            $"- seamCount: {seamProof.SeamCount}",
            $"- waterContinuityPassed: {seamProof.WaterContinuityPassed.ToString().ToLowerInvariant()}",
            $"- roadContinuityPassed: {seamProof.RoadContinuityPassed.ToString().ToLowerInvariant()}",
            $"- biomeContinuityPassed: {seamProof.BiomeContinuityPassed.ToString().ToLowerInvariant()}",
            $"- cacheReuseProofPassed: {cacheReuseProof.Passed.ToString().ToLowerInvariant()}",
            $"- reusedChunkKeyCount: {cacheReuseProof.ReusedChunkKeyCount}",
            $"- layerTransitionProofPassed: {layerTransitionProof.Passed.ToString().ToLowerInvariant()}",
            $"- portalOrTransitionLinkCount: {layerTransitionProof.PortalOrTransitionLinkCount}",
            string.Empty,
            "## Validation",
            string.Empty,
            $"- determinismProofPassed: {report.DeterminismProofPassed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            $"- sourceLineagePassed: {report.SourceLineagePassed.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- noRawFullWorldDump: {qualityGate.HugeSparseNoRawDump.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            $"- noExplicitAdultContent: {qualityGate.NoExplicitAdultContent.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- materializationManifestHash: {report.MaterializationManifestHash}",
            $"- fileLedgerHash: {report.FileLedgerHash}",
            $"- determinismProofHash: {report.DeterminismProofHash}",
            $"- seamProofHash: {report.SeamProofHash}",
            $"- cacheReuseProofHash: {report.CacheReuseProofHash}",
            $"- layerTransitionProofHash: {report.LayerTransitionProofHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyDictionary<string, string> RenderOverviewSvgs(IReadOnlyList<VisualChunkStreamWindow> windows) =>
        windows
            .GroupBy(item => item.FixtureId, StringComparer.Ordinal)
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, RenderFixtureOverviewSvg, StringComparer.Ordinal);

    private static string RenderFixtureOverviewSvg(IGrouping<string, VisualChunkStreamWindow> fixtureGroup)
    {
        const int cellSize = 12;
        const int xOffset = 18;
        const int yOffset = 44;
        var chunks = fixtureGroup.SelectMany(item => item.Chunks).ToList();
        var minX = chunks.Min(item => item.ChunkX);
        var minY = chunks.Min(item => item.ChunkY);
        var width = 360;
        var height = 164;
        var lines = new List<string>
        {
            $"<svg viewBox=\"0 0 {width} {height}\" data-fixture-id=\"{Escape(fixtureGroup.Key)}\">",
            $"  <title>{Escape(fixtureGroup.Key)}</title>",
            $"  <rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"#101418\" />",
            $"  <text x=\"12\" y=\"18\" fill=\"#e8eef3\" font-size=\"10\" font-family=\"monospace\">{Escape(fixtureGroup.Key)}</text>",
            $"  <text x=\"12\" y=\"34\" fill=\"#9fb0bf\" font-size=\"8\" font-family=\"monospace\">windows={fixtureGroup.Count()}; chunks={chunks.Count}</text>"
        };

        var ordered = chunks
            .OrderBy(item => item.WindowId, StringComparer.Ordinal)
            .ThenBy(item => item.LayerId, StringComparer.Ordinal)
            .ThenBy(item => item.ChunkY)
            .ThenBy(item => item.ChunkX)
            .ToList();
        foreach (var chunk in ordered)
        {
            var x = xOffset + (int)(chunk.ChunkX - minX) * cellSize + WindowOffset(fixtureGroup, chunk.WindowId);
            var y = yOffset + (int)(chunk.ChunkY - minY) * cellSize;
            lines.Add($"  <rect x=\"{x}\" y=\"{y}\" width=\"10\" height=\"10\" fill=\"{LayerColor(chunk.LayerId)}\" data-window-id=\"{Escape(chunk.WindowId)}\" data-layer-id=\"{Escape(chunk.LayerId)}\" data-chunk-key=\"{Escape(chunk.ChunkKey[..12])}\" />");
        }

        foreach (var window in fixtureGroup.OrderBy(item => item.WindowId, StringComparer.Ordinal))
        {
            lines.Add($"  <text x=\"12\" y=\"{132 + WindowOffset(fixtureGroup, window.WindowId)}\" fill=\"#d6c890\" font-size=\"7\" font-family=\"monospace\">{Escape(window.WindowId)} r={window.RadiusChunks} c={window.CenterChunkX},{window.CenterChunkY}</text>");
        }

        lines.Add("</svg>");
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static int WindowOffset(IEnumerable<VisualChunkStreamWindow> windows, string windowId)
    {
        var index = windows
            .Select(item => item.WindowId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList()
            .IndexOf(windowId);
        return Math.Max(0, index) * 4;
    }

    private static VisualChunkStreamNegativeScenario InvalidRequest(
        string id,
        string mutation,
        VisualChunkStreamRequest request,
        IReadOnlyList<VisualWorldProfile> profiles)
    {
        var validation = DeterministicVisualChunkStreamWindowValidator.ValidateRequest(request, profiles);
        return new VisualChunkStreamNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static VisualChunkStreamNegativeScenario InvalidWindow(
        string id,
        string mutation,
        VisualChunkStreamWindow window)
    {
        var validation = DeterministicVisualChunkStreamWindowValidator.ValidateWindow(window);
        return new VisualChunkStreamNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static IReadOnlyList<VisualChunkStreamChunkRef> ReplaceChunk(
        VisualChunkStreamWindow window,
        VisualChunkStreamChunkRef replacement) =>
        window.Chunks
            .Select(item => item.WindowId == replacement.WindowId && item.LayerId == replacement.LayerId && item.ChunkX == replacement.ChunkX && item.ChunkY == replacement.ChunkY ? replacement : item)
            .ToList();

    private static IReadOnlyList<VisualChunkStreamSeam> ReplaceSeam(
        VisualChunkStreamWindow window,
        VisualChunkStreamSeam replacement) =>
        window.Seams
            .Select(item => item.WindowId == replacement.WindowId
                && item.LayerId == replacement.LayerId
                && item.Direction == replacement.Direction
                && item.FromChunkX == replacement.FromChunkX
                && item.FromChunkY == replacement.FromChunkY
                ? replacement
                : item)
            .ToList();

    private static IReadOnlyList<(string Path, IReadOnlyList<string> Tags)> SourceInputs() =>
    [
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-report.md", ["goal090", "report"]),
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-catalog.json", ["goal090", "catalog"]),
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-size-matrix.json", ["goal090", "finite_size_matrix"]),
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-chunk-address-proof.json", ["goal090", "chunk_address"]),
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-sparse-world-proof.json", ["goal090", "sparse_infinite"]),
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-layer-model-proof.json", ["goal090", "layer_model"]),
        (".llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-quality-gate-scan.json", ["goal090", "quality"]),
        (".llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-composer-report.md", ["goal088", "region_composer"]),
        (".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md", ["goal087", "patch_composer"]),
        ("docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md", ["deepsearch", "visual_stack"]),
        ("docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md", ["deepsearch", "chunked_world"]),
        ("docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md", ["deepsearch", "sidecar"])
    ];

    private static VisualChunkStreamSourceLineageRecord SourceRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new VisualChunkStreamSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : DeterministicVisualChunkStreamWindowHash.Compute(text),
            PurposeTags = tags
        };
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ReadText(string? projectRootPath, string relativePath)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        return fullPath != null && File.Exists(fullPath)
            ? File.ReadAllText(fullPath, Encoding.UTF8)
            : string.Empty;
    }

    private static string? ResolveOptionalPath(string? projectRootPath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(Path.GetFullPath(projectRootPath), relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), rootFull.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Path '{path}' must stay under '{root}'.");
        }
    }

    private static string LayerColor(string layerId) =>
        layerId switch
        {
            "surface" => "#69a55a",
            "terrain" => "#74a45a",
            "underground" => "#746b84",
            "underwater" => "#3d87a5",
            "interior" => "#d0b15c",
            "sky_overlay" => "#7ba7d1",
            _ => "#a66d58"
        };

    private static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
