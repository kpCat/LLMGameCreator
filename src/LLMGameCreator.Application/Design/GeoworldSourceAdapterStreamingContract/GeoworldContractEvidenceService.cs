using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;

public sealed class GeoworldContractEvidenceService
{
    public const string ReportMarkdownFileName = "geoworld-source-adapter-streaming-contract-report.md";
    public const string CatalogJsonFileName = "geoworld-source-adapter-catalog.json";
    public const string TaxonomyJsonFileName = "geoworld-normalized-feature-taxonomy.json";
    public const string StreamingPolicyMatrixJsonFileName = "geoworld-streaming-policy-matrix.json";
    public const string NegativeProofJsonFileName = "geoworld-negative-proof.json";
    public const string LfzPatternLineageJsonFileName = "geoworld-lfz-pattern-lineage.json";
    public const string QualityGateScanJsonFileName = "geoworld-quality-gate-scan.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static GeoworldContractEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public GeoworldContractEvidenceResult Build(string? projectRootPath = null)
    {
        var fixtures = GeoworldContractFixtures.BuildFixtures();
        var catalog = BuildCatalog(fixtures);
        var taxonomy = GeoworldContractFixtures.BuildTaxonomy();
        var streamingPolicyMatrix = BuildStreamingPolicyMatrix(fixtures);
        var negativeProof = BuildNegativeProof(fixtures);
        var lineage = BuildLfzPatternLineage(projectRootPath);
        var qualityGate = BuildQualityGate(fixtures, taxonomy, streamingPolicyMatrix, negativeProof, lineage);

        var catalogJson = Serialize(catalog);
        var taxonomyJson = Serialize(taxonomy);
        var streamingPolicyJson = Serialize(streamingPolicyMatrix);
        var negativeProofJson = Serialize(negativeProof);
        var lineageJson = Serialize(lineage);
        var qualityGateJson = Serialize(qualityGate);

        var reportWithoutHash = BuildReport(
            catalog,
            taxonomy,
            streamingPolicyMatrix,
            negativeProof,
            lineage,
            qualityGate,
            catalogJson,
            taxonomyJson,
            streamingPolicyJson,
            negativeProofJson,
            lineageJson,
            qualityGateJson);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, catalog, taxonomy, streamingPolicyMatrix, negativeProof, lineage, qualityGate, string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = GeoworldContractHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, catalog, taxonomy, streamingPolicyMatrix, negativeProof, lineage, qualityGate, report.DeterministicReportHash);

        return new GeoworldContractEvidenceResult
        {
            Catalog = catalog,
            Taxonomy = taxonomy,
            StreamingPolicyMatrix = streamingPolicyMatrix,
            NegativeProof = negativeProof,
            LfzPatternLineage = lineage,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            TaxonomyJson = taxonomyJson,
            StreamingPolicyMatrixJson = streamingPolicyJson,
            NegativeProofJson = negativeProofJson,
            LfzPatternLineageJson = lineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown
        };
    }

    public async Task<GeoworldContractWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeoworldContractWriteResult> BuildAndWriteAsync(
        string lineageRootPath,
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(lineageRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeoworldContractWriteResult> WriteAsync(
        string projectRootPath,
        GeoworldContractEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            GeoworldSourceAdapterStreamingContractVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new GeoworldContractWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            TaxonomyJsonPath = Path.Combine(outputDirectory, TaxonomyJsonFileName),
            StreamingPolicyMatrixJsonPath = Path.Combine(outputDirectory, StreamingPolicyMatrixJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            LfzPatternLineageJsonPath = Path.Combine(outputDirectory, LfzPatternLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName)
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CatalogJsonPath, result.CatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.TaxonomyJsonPath, result.TaxonomyJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.StreamingPolicyMatrixJsonPath, result.StreamingPolicyMatrixJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.LfzPatternLineageJsonPath, result.LfzPatternLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public static GeoworldNegativeProof BuildNegativeProof(IReadOnlyList<GeoSourceAdapterSpec>? baselineFixtures = null)
    {
        var fixtures = baselineFixtures ?? GeoworldContractFixtures.BuildFixtures();
        var baseline = fixtures.Single(item => item.SpecId == "offline_osm_extract_city_radius");
        var ocr = fixtures.Single(item => item.SpecId == "ocr_georeference_fallback_future_only");
        var raw = baseline.RawDescriptors[0];

        var scenarios = new List<GeoworldNegativeScenario>
        {
            Invalid("public_tile_scraping", "adapter attempts public tile server scraping", baseline with { AdapterKind = GeoSourceAdapterKind.PublicTileServerScrape, FetchPlan = baseline.FetchPlan with { PublicTileServerScrapeAttempted = true } }),
            Invalid("bulk_public_tile_archive", "bulk public tile archive/preseed mode", baseline with { AdapterKind = GeoSourceAdapterKind.BulkPublicTileArchive, CachePolicy = baseline.CachePolicy! with { PublicTileBulkArchiveMode = true, NoRawPublicTilePreseed = false }, FetchPlan = baseline.FetchPlan with { BulkPublicTileArchiveMode = true } }),
            Invalid("runtime_online_without_explicit_policy", "runtime online mode enabled without explicit policy", baseline with { FetchPlan = baseline.FetchPlan with { RuntimeOnlineModeEnabled = true, RuntimeOnlinePolicyExplicitlyEnabled = false }, LicensePolicy = baseline.LicensePolicy! with { RuntimeOnlineExplicitPolicyAllowed = false } }),
            Invalid("missing_license_policy", "license policy missing", baseline with { LicensePolicy = null }),
            Invalid("missing_attribution", "attribution missing", baseline with { LicensePolicy = baseline.LicensePolicy! with { AttributionText = "" } }),
            Invalid("missing_provenance", "provenance missing", baseline with { Provenance = null }),
            Invalid("raw_osm_tags_direct_to_gameplay", "raw OSM tags consumed directly by gameplay", baseline with { RawDescriptors = ReplaceRaw(baseline, raw with { ConsumedDirectlyByGameplay = true }) }),
            Invalid("absolute_source_path", "absolute source/cache path", baseline with { Provenance = baseline.Provenance! with { SourceReference = "C:/unsafe/raw.osm" } }),
            Invalid("missing_cache_policy", "cache policy missing", baseline with { CachePolicy = null }),
            Invalid("missing_stream_radius_boundary_prefetch", "stream radius and boundary prefetch missing", baseline with { StreamingPolicy = baseline.StreamingPolicy! with { StreamWindowRequest = baseline.StreamingPolicy.StreamWindowRequest with { BoundaryPrefetchEnabled = false, GridRequest = baseline.StreamingPolicy.StreamWindowRequest.GridRequest with { RadiusTiles = 0, BoundaryPrefetchTiles = 0 } } } }),
            Invalid("full_planet_raw_dump", "full planet raw dump requested", baseline with { FetchPlan = baseline.FetchPlan with { FullPlanetRawDumpRequested = true }, WorldSourceGraph = baseline.WorldSourceGraph with { NoFullPlanetRawDump = false } }),
            Invalid("hardcoded_provider_api_in_core", "provider/API hardcoded into core", baseline with { FetchPlan = baseline.FetchPlan with { ProviderOrApiHardcodedIntoCore = true } }),
            Invalid("prompt_text_source_of_truth", "prompt text used as source of truth", baseline with { Provenance = baseline.Provenance! with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" } }),
            Invalid("lfz_copied_code_marker", "LFZ copied source marker present", baseline with { Provenance = baseline.Provenance! with { ContainsLfzCopiedCodeMarker = true } }),
            Invalid("ocr_fallback_primary_path", "OCR fallback treated as primary path", ocr with { OcrFallbackFutureOnly = false, FetchPlan = ocr.FetchPlan with { OcrFallbackIsPrimaryPath = true } }),
            Invalid("rating_metadata_without_safe_fallback", "adult/rating metadata without safe fallback", baseline with { LicensePolicy = baseline.LicensePolicy! with { ContainsAdultOrRatingMetadata = true, SafeFallbackPolicyId = "" } })
        };

        return new GeoworldNegativeProof
        {
            Passed = scenarios.Count >= 16
                && scenarios.All(item => !item.ActualValid && item.ExpectedValid == item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static GeoworldSourceAdapterCatalog BuildCatalog(IReadOnlyList<GeoSourceAdapterSpec> fixtures) =>
        new()
        {
            Accepted = false,
            FixtureCount = fixtures.Count,
            FixtureIds = fixtures.Select(item => item.SpecId).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Fixtures = fixtures.OrderBy(item => item.SpecId, StringComparer.Ordinal).ToList()
        };

    private static GeoworldStreamingPolicyMatrix BuildStreamingPolicyMatrix(IReadOnlyList<GeoSourceAdapterSpec> fixtures)
    {
        var rows = fixtures
            .OrderBy(item => item.SpecId, StringComparer.Ordinal)
            .Select(item =>
            {
                var request = item.StreamingPolicy!.StreamWindowRequest;
                return new GeoworldStreamingPolicyRow
                {
                    FixtureId = item.SpecId,
                    RadiusTiles = request.GridRequest.RadiusTiles,
                    BoundaryPrefetchTiles = request.GridRequest.BoundaryPrefetchTiles,
                    BoundaryPrefetchEnabled = request.BoundaryPrefetchEnabled,
                    MaterializesOnlyRequestedWindow = request.MaterializesOnlyRequestedWindow,
                    RuntimeOnlineBlockedByDefault = item.StreamingPolicy.RuntimeOnlineBlockedByDefault,
                    FutureRuntimeStreamingContractOnly = item.StreamingPolicy.FutureRuntimeStreamingContractOnly
                };
            })
            .ToList();

        return new GeoworldStreamingPolicyMatrix
        {
            Passed = rows.Count == fixtures.Count
                && rows.All(item => item.RadiusTiles > 0 && item.BoundaryPrefetchEnabled && item.BoundaryPrefetchTiles > 0 && item.MaterializesOnlyRequestedWindow),
            Rows = rows
        };
    }

    private static GeoworldLfzPatternLineage BuildLfzPatternLineage(string? projectRootPath)
    {
        var records = LineageInputs()
            .Select(item => LineageRecord(projectRootPath, item.Path, item.Tags))
            .ToList();

        var lfzDocs = records
            .Where(item => item.PurposeTags.Contains("lfz_pattern", StringComparer.Ordinal))
            .ToList();
        var lfzDocsConsumed = lfzDocs.Count >= 2 && lfzDocs.All(item => item.Exists);
        return new GeoworldLfzPatternLineage
        {
            Passed = records.All(item => item.Exists) && lfzDocsConsumed,
            LfzDocsConsumedAsLineage = lfzDocsConsumed,
            LfzArchiveNotRequired = true,
            LfzSourceCodeNotCopied = true,
            Records = records
        };
    }

    private static GeoworldQualityGateScan BuildQualityGate(
        IReadOnlyList<GeoSourceAdapterSpec> fixtures,
        GeoworldNormalizedFeatureTaxonomy taxonomy,
        GeoworldStreamingPolicyMatrix streamingPolicyMatrix,
        GeoworldNegativeProof negativeProof,
        GeoworldLfzPatternLineage lineage)
    {
        var diagnostics = new List<GeoworldContractDiagnostic>();
        var validations = fixtures.Select(GeoworldContractValidator.Validate).ToList();
        var validFixturesPassed = validations.All(item => item.Passed);
        var requiredKinds = new[]
        {
            GeoFeatureKind.Building,
            GeoFeatureKind.Road,
            GeoFeatureKind.Water,
            GeoFeatureKind.LandUse,
            GeoFeatureKind.Poi,
            GeoFeatureKind.Barrier,
            GeoFeatureKind.Bridge,
            GeoFeatureKind.Vegetation
        };
        var taxonomyPassed = requiredKinds.All(kind => taxonomy.Rows.Any(row => row.Kind == kind && row.GameplayConsumesNormalizedFeatureOnly));
        var runtimeBoundaryPrefetchContract = fixtures.Any(item =>
            item.SpecId == "earth_radius_stream_window_boundary_prefetch"
            && item.StreamingPolicy?.StreamWindowRequest.GridRequest.BoundaryPrefetchTiles >= 2
            && item.StreamingPolicy.StreamWindowRequest.BoundaryPrefetchEnabled);
        var noNetwork = fixtures.All(item =>
            !item.FetchPlan.PerformsNetworkIo
            && item.FetchPlan.NetworkIoMode != GeoNetworkIoMode.LiveNetworkFetch
            && !item.FetchPlan.ProviderOrApiHardcodedIntoCore
            && !item.FetchResult.NetworkIoPerformed);
        var noRawDumps = fixtures.All(item =>
            item.MetadataOnly
            && item.FetchResult.MetadataOnly
            && !item.FetchResult.RawGeodataDumpPresent
            && item.FetchResult.BinaryMediaFileCount == 0);
        var streamingContractsOnly = fixtures.All(item => item.StreamingPolicy?.FutureRuntimeStreamingContractOnly == true && item.WorldSourceGraph.ContractOnly);
        var noLfzCodeCopied = fixtures.All(item => item.Provenance?.ContainsLfzCopiedCodeMarker == false)
            && negativeProof.Scenarios.Any(item => item.ScenarioId == "lfz_copied_code_marker" && !item.ActualValid);

        if (!validFixturesPassed)
        {
            diagnostics.AddRange(validations.SelectMany(item => item.Diagnostics));
        }

        AddQualityDiagnostic(!negativeProof.Passed, "geoworld.negative_proof.failed", "negative_proof", "Negative matrix must reject all unsafe cases.", diagnostics);
        AddQualityDiagnostic(!taxonomyPassed, "geoworld.taxonomy.required_kinds_missing", "taxonomy", "Normalized taxonomy must include required geoworld kinds.", diagnostics);
        AddQualityDiagnostic(!runtimeBoundaryPrefetchContract, "geoworld.boundary_prefetch_contract.missing", "streaming_policy", "Runtime boundary-prefetch contract must be present.", diagnostics);
        AddQualityDiagnostic(!lineage.Passed, "geoworld.lfz_lineage.failed", "lfz_lineage", "LFZ/geoworld docs must be consumed as lineage.", diagnostics);
        AddQualityDiagnostic(!noNetwork, "geoworld.network_or_provider_implementation", "fixtures", "Goal098 fixtures must not perform network I/O or hardcode providers.", diagnostics);
        AddQualityDiagnostic(!noRawDumps, "geoworld.raw_geodata_dump", "fixtures", "Goal098 evidence must not contain raw geodata dumps or binary media.", diagnostics);
        AddQualityDiagnostic(!streamingContractsOnly, "geoworld.streaming_contract_only.failed", "fixtures", "Future runtime streaming must remain contracts only.", diagnostics);

        return new GeoworldQualityGateScan
        {
            Accepted = false,
            ValidFixturesPassed = validFixturesPassed,
            NegativeProofPassed = negativeProof.Passed,
            NormalizedTaxonomyPassed = taxonomyPassed,
            RuntimeBoundaryPrefetchContractPresent = runtimeBoundaryPrefetchContract,
            LfzDocsConsumedAsLineage = lineage.LfzDocsConsumedAsLineage,
            NoLfzCodeCopied = noLfzCodeCopied,
            NoNetworkOrProviderImplementation = noNetwork,
            FutureRuntimeStreamingContractsOnly = streamingContractsOnly,
            NoRawGeodataDumps = noRawDumps,
            ArtifactScopeReady = true,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/GeoworldSourceAdapterStreamingContract/",
                "tests/LLMGameCreator.Tests/Application/GeoworldSourceAdapterStreamingContract/",
                "tests/LLMGameCreator.Tests/ProductSmoke/GeoworldSourceAdapterStreamingContractProductSmokeTests.cs",
                ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/",
                "docs/agent-tasks/goal-098-geoworld-source-adapter-streaming-contract/"
            ],
            Diagnostics = GeoworldContractValidator.SortDiagnostics(diagnostics)
        };
    }

    private static GeoworldContractReport BuildReport(
        GeoworldSourceAdapterCatalog catalog,
        GeoworldNormalizedFeatureTaxonomy taxonomy,
        GeoworldStreamingPolicyMatrix streamingPolicyMatrix,
        GeoworldNegativeProof negativeProof,
        GeoworldLfzPatternLineage lineage,
        GeoworldQualityGateScan qualityGate,
        string catalogJson,
        string taxonomyJson,
        string streamingPolicyJson,
        string negativeProofJson,
        string lineageJson,
        string qualityGateJson) =>
        new()
        {
            Accepted = false,
            FixtureCount = catalog.FixtureCount,
            TaxonomyKindCount = taxonomy.Rows.Count,
            NegativeScenarioCount = negativeProof.ScenarioCount,
            ValidFixturesPassed = qualityGate.ValidFixturesPassed,
            NegativeProofPassed = negativeProof.Passed,
            LfzLineagePassed = lineage.Passed,
            QualityGatePassed = qualityGate.Diagnostics.All(item => item.Severity != "error"),
            CatalogHash = GeoworldContractHash.Compute(catalogJson),
            TaxonomyHash = GeoworldContractHash.Compute(taxonomyJson),
            StreamingPolicyMatrixHash = GeoworldContractHash.Compute(streamingPolicyJson),
            NegativeProofHash = GeoworldContractHash.Compute(negativeProofJson),
            LfzPatternLineageHash = GeoworldContractHash.Compute(lineageJson),
            QualityGateHash = GeoworldContractHash.Compute(qualityGateJson)
        };

    private static string RenderReport(
        GeoworldContractReport report,
        GeoworldSourceAdapterCatalog catalog,
        GeoworldNormalizedFeatureTaxonomy taxonomy,
        GeoworldStreamingPolicyMatrix streamingPolicyMatrix,
        GeoworldNegativeProof negativeProof,
        GeoworldLfzPatternLineage lineage,
        GeoworldQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 098 Geoworld Source Adapter Streaming Contract Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 098 adds the first LLMGameCreator-native geoworld source adapter and runtime streaming contract foundation. It is a BCL-only Application-side metadata, validation and evidence seam: no LFZ archive read, no LFZ source copy, no live network fetching, no public tile scraping and no raw geodata dumps.",
            string.Empty,
            "## Fixture Coverage",
            string.Empty
        };
        lines.AddRange(catalog.FixtureIds.Select(id => $"- {id}"));
        lines.AddRange(
        [
            string.Empty,
            "## Normalized Feature Taxonomy",
            string.Empty
        ]);
        lines.AddRange(taxonomy.Rows.Select(row => $"- {row.Kind}: {row.NeutralFeatureContract}"));
        lines.AddRange(
        [
            string.Empty,
            "## Streaming Policy",
            string.Empty,
            $"- streamingPolicyMatrixPassed: {streamingPolicyMatrix.Passed.ToString().ToLowerInvariant()}",
            $"- boundaryPrefetchRows: {streamingPolicyMatrix.Rows.Count(item => item.BoundaryPrefetchEnabled && item.BoundaryPrefetchTiles > 0)}",
            $"- runtimeBoundaryPrefetchContractPresent: {qualityGate.RuntimeBoundaryPrefetchContractPresent.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Validation",
            string.Empty,
            $"- validFixturesPassed: {report.ValidFixturesPassed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            string.Empty,
            "## LFZ Pattern Lineage",
            string.Empty,
            $"- lfzLineagePassed: {lineage.Passed.ToString().ToLowerInvariant()}",
            $"- lfzDocsConsumedAsLineage: {lineage.LfzDocsConsumedAsLineage.ToString().ToLowerInvariant()}",
            $"- lfzArchiveNotRequired: {lineage.LfzArchiveNotRequired.ToString().ToLowerInvariant()}",
            $"- lfzSourceCodeNotCopied: {lineage.LfzSourceCodeNotCopied.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- noLfzCodeCopied: {qualityGate.NoLfzCodeCopied.ToString().ToLowerInvariant()}",
            $"- noNetworkOrProviderImplementation: {qualityGate.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnitySchemaChanges: {qualityGate.NoRuntimeUnitySchemaChanges.ToString().ToLowerInvariant()}",
            $"- futureRuntimeStreamingContractsOnly: {qualityGate.FutureRuntimeStreamingContractsOnly.ToString().ToLowerInvariant()}",
            $"- noRawGeodataDumps: {qualityGate.NoRawGeodataDumps.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMedia: {qualityGate.NoBinaryOrRasterMedia.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- taxonomyHash: {report.TaxonomyHash}",
            $"- streamingPolicyMatrixHash: {report.StreamingPolicyMatrixHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- lfzPatternLineageHash: {report.LfzPatternLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static GeoworldNegativeScenario Invalid(string id, string mutation, GeoSourceAdapterSpec spec)
    {
        var validation = GeoworldContractValidator.Validate(spec);
        return new GeoworldNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static IReadOnlyList<GeoFeatureRawDescriptor> ReplaceRaw(
        GeoSourceAdapterSpec spec,
        GeoFeatureRawDescriptor replacement) =>
        spec.RawDescriptors
            .Select(item => item.RawDescriptorId == replacement.RawDescriptorId ? replacement : item)
            .ToList();

    private static IReadOnlyList<(string Path, IReadOnlyList<string> Tags)> LineageInputs() =>
    [
        ("docs/context/LFZ_ARCHIVE_ANALYSIS_MANIFEST.md", ["lfz_pattern", "archive_manifest", "no_archive_to_codex"]),
        ("docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md", ["lfz_pattern", "geoworld_pipeline", "source_adapter"]),
        ("docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md", ["geoworld_streaming", "boundary_prefetch"]),
        ("docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md", ["geoworld_source_adapter", "cache_provenance_license"]),
        ("docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md", ["future_goal_sequence", "no_network_now"]),
        ("docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md", ["dream_scope", "geospatial_policy"]),
        ("docs/ROADMAP_FINAL_REBASELINE.md", ["goal097", "productivity_boundary"]),
        ("docs/RELEASE_RISK_REGISTER.md", ["release_risk", "geospatial_tos"])
    ];

    private static GeoworldLineageRecord LineageRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new GeoworldLineageRecord
        {
            RelativePath = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : GeoworldContractHash.Compute(text),
            PurposeTags = tags
        };
    }

    private static void AddQualityDiagnostic(
        bool condition,
        string code,
        string target,
        string message,
        List<GeoworldContractDiagnostic> diagnostics)
    {
        if (condition)
        {
            diagnostics.Add(GeoworldContractDiagnostic.Error(code, target, message));
        }
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
}
