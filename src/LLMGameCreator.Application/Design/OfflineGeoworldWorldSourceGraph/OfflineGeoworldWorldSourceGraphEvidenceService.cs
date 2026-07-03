using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public sealed class OfflineGeoworldWorldSourceGraphEvidenceService
{
    public const string ReportMarkdownFileName = "offline-geoworld-worldsourcegraph-report.md";
    public const string BundleCatalogJsonFileName = "offline-geoworld-bundle-catalog.json";
    public const string NormalizedFeaturesJsonFileName = "offline-geoworld-normalized-features.json";
    public const string WorldSourceGraphJsonFileName = "offline-geoworld-worldsourcegraph.json";
    public const string StreamWindowPlanJsonFileName = "offline-geoworld-stream-window-plan.json";
    public const string BoundaryPrefetchProofJsonFileName = "offline-geoworld-boundary-prefetch-proof.json";
    public const string VisualProjectionSummaryJsonFileName = "offline-geoworld-visual-projection-summary.json";
    public const string NegativeProofJsonFileName = "offline-geoworld-negative-proof.json";
    public const string WorkspaceBindingInventoryJsonFileName = "offline-geoworld-workspace-binding-inventory.json";
    public const string SourceLineageJsonFileName = "offline-geoworld-source-lineage.json";
    public const string QualityGateScanJsonFileName = "offline-geoworld-quality-gate-scan.json";
    public const string OverviewSvgRelativePath = "overviews/synthetic_city_radius_stream_window.svg";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static OfflineGeoworldWorldSourceGraphEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public OfflineGeoworldBuildResult Build(string? projectRootPath = null)
    {
        var bundle = OfflineGeoworldBundleFixtures.BuildSyntheticCityRadiusBundle();
        var catalog = BuildCatalog(bundle);
        var normalized = OfflineGeoworldNormalizer.Normalize(bundle);
        var graph = OfflineGeoworldWorldSourceGraphBuilder.Build(bundle, normalized);
        var streamWindow = OfflineGeoworldStreamWindowScheduler.BuildPlan(graph);
        var boundaryPrefetchProof =
            OfflineGeoworldStreamWindowScheduler.BuildBoundaryPrefetchProof(streamWindow);
        var projection = OfflineGeoworldVisualProjectionBuilder.BuildProjection(graph, normalized, streamWindow);
        var overviewSvg = OfflineGeoworldVisualProjectionBuilder.RenderSvg(projection, streamWindow);
        var negativeProof = BuildNegativeProof(bundle, normalized, graph, streamWindow, projection);
        var sourceLineage = BuildSourceLineage(projectRootPath);
        var workspaceBinding = BuildWorkspaceBindingInventory(projectRootPath);
        var qualityGate = BuildQualityGate(
            bundle,
            normalized,
            graph,
            streamWindow,
            boundaryPrefetchProof,
            projection,
            negativeProof,
            sourceLineage,
            workspaceBinding);

        var catalogJson = Serialize(catalog);
        var normalizedJson = Serialize(normalized);
        var graphJson = Serialize(graph);
        var streamJson = Serialize(streamWindow);
        var boundaryJson = Serialize(boundaryPrefetchProof);
        var projectionJson = Serialize(projection);
        var negativeJson = Serialize(negativeProof);
        var bindingJson = Serialize(workspaceBinding);
        var lineageJson = Serialize(sourceLineage);
        var qualityJson = Serialize(qualityGate);
        var reportWithoutHash = BuildReport(
            catalog,
            normalized,
            graph,
            streamWindow,
            boundaryPrefetchProof,
            negativeProof,
            workspaceBinding,
            qualityGate,
            catalogJson,
            normalizedJson,
            graphJson,
            streamJson,
            boundaryJson,
            projectionJson,
            negativeJson,
            bindingJson,
            lineageJson,
            qualityJson);
        var markdownWithoutHash = RenderReport(reportWithoutHash, qualityGate, string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = OfflineGeoworldWorldSourceGraphHash.Compute(markdownWithoutHash)
        };
        var markdown = RenderReport(report, qualityGate, report.DeterministicReportHash);

        return new OfflineGeoworldBuildResult
        {
            BundleCatalog = catalog,
            NormalizedFeatures = normalized,
            WorldSourceGraph = graph,
            StreamWindowPlan = streamWindow,
            BoundaryPrefetchProof = boundaryPrefetchProof,
            VisualProjectionSummary = projection,
            NegativeProof = negativeProof,
            WorkspaceBindingInventory = workspaceBinding,
            SourceLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            OverviewSvgText = overviewSvg,
            BundleCatalogJson = catalogJson,
            NormalizedFeaturesJson = normalizedJson,
            WorldSourceGraphJson = graphJson,
            StreamWindowPlanJson = streamJson,
            BoundaryPrefetchProofJson = boundaryJson,
            VisualProjectionSummaryJson = projectionJson,
            NegativeProofJson = negativeJson,
            WorkspaceBindingInventoryJson = bindingJson,
            SourceLineageJson = lineageJson,
            QualityGateScanJson = qualityJson,
            ReportMarkdown = markdown
        };
    }

    public async Task<OfflineGeoworldWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OfflineGeoworldWriteResult> BuildAndWriteAsync(
        string sourceRootPath,
        string outputRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(sourceRootPath);
        return await WriteAsync(outputRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OfflineGeoworldWriteResult> WriteAsync(
        string projectRootPath,
        OfflineGeoworldBuildResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            OfflineGeoworldWorldSourceGraphVocabulary.RelativeOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var overviewDirectory = Path.Combine(outputDirectory, "overviews");
        EnsureContained(projectRoot, overviewDirectory);
        Directory.CreateDirectory(overviewDirectory);

        var write = new OfflineGeoworldWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            BundleCatalogJsonPath = Path.Combine(outputDirectory, BundleCatalogJsonFileName),
            NormalizedFeaturesJsonPath = Path.Combine(outputDirectory, NormalizedFeaturesJsonFileName),
            WorldSourceGraphJsonPath = Path.Combine(outputDirectory, WorldSourceGraphJsonFileName),
            StreamWindowPlanJsonPath = Path.Combine(outputDirectory, StreamWindowPlanJsonFileName),
            BoundaryPrefetchProofJsonPath = Path.Combine(outputDirectory, BoundaryPrefetchProofJsonFileName),
            VisualProjectionSummaryJsonPath = Path.Combine(outputDirectory, VisualProjectionSummaryJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            WorkspaceBindingInventoryJsonPath = Path.Combine(outputDirectory, WorkspaceBindingInventoryJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            OverviewSvgPath = Path.Combine(outputDirectory, OverviewSvgRelativePath.Replace('/', Path.DirectorySeparatorChar)),
            Result = result
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.BundleCatalogJsonPath, result.BundleCatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NormalizedFeaturesJsonPath, result.NormalizedFeaturesJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.WorldSourceGraphJsonPath, result.WorldSourceGraphJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.StreamWindowPlanJsonPath, result.StreamWindowPlanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.BoundaryPrefetchProofJsonPath, result.BoundaryPrefetchProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.VisualProjectionSummaryJsonPath, result.VisualProjectionSummaryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.WorkspaceBindingInventoryJsonPath, result.WorkspaceBindingInventoryJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.OverviewSvgPath, result.OverviewSvgText, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        return write;
    }

    public static OfflineGeoworldNegativeProof BuildNegativeProof(
        OfflineGeoworldBundle? baselineBundle = null,
        OfflineGeoworldNormalizedFeatureSet? baselineFeatures = null,
        OfflineGeoworldWorldSourceGraph? baselineGraph = null,
        OfflineGeoworldStreamWindowPlan? baselineStreamWindow = null,
        OfflineGeoworldVisualProjectionSummary? baselineProjection = null)
    {
        var bundle = baselineBundle ?? OfflineGeoworldBundleFixtures.BuildSyntheticCityRadiusBundle();
        var features = baselineFeatures ?? OfflineGeoworldNormalizer.Normalize(bundle);
        var graph = baselineGraph ?? OfflineGeoworldWorldSourceGraphBuilder.Build(bundle, features);
        var stream = baselineStreamWindow ?? OfflineGeoworldStreamWindowScheduler.BuildPlan(graph);
        var projection = baselineProjection ?? OfflineGeoworldVisualProjectionBuilder.BuildProjection(graph, features, stream);
        var firstRaw = bundle.RawDescriptors[0];

        var scenarios = new List<OfflineGeoworldNegativeScenario>
        {
            Invalid(
                "raw_osm_tags_direct_to_gameplay",
                "raw OSM-like tags consumed directly by gameplay",
                bundle with { RawDescriptors = ReplaceRaw(bundle, firstRaw with { ConsumedDirectlyByGameplay = true }) },
                streamOverride: stream),
            Invalid(
                "missing_license_provenance",
                "license and provenance missing",
                bundle with { SourceLineage = "", LicenseProvenanceSummary = "" },
                streamOverride: stream),
            Invalid(
                "runtime_online_fetch_attempted",
                "runtime online fetch attempted",
                bundle,
                streamOverride: stream with { NetworkFetchAttempted = true }),
            Invalid(
                "public_tile_scraping",
                "public tile scraping attempted",
                bundle with { PublicTileScrapingAttempted = true },
                streamOverride: stream),
            Invalid(
                "full_area_raw_dump",
                "full area raw dump present",
                bundle with { ContainsRawFullAreaDump = true },
                graphOverride: graph with { NoRawFullAreaDump = false },
                streamOverride: stream),
            Invalid(
                "absolute_paths",
                "absolute path in source lineage",
                bundle with { SourceLineage = "C:/unsafe/raw.osm" },
                streamOverride: stream),
            Invalid(
                "lfz_copied_code_marker",
                "LFZ copied-code marker present",
                bundle with { ContainsLfzCopiedCodeMarker = true },
                streamOverride: stream),
            Invalid(
                "unknown_feature_kind",
                "unknown feature kind",
                bundle with { RawDescriptors = ReplaceRaw(bundle, firstRaw with { NormalizedKind = OfflineGeoFeatureKind.Unknown }) },
                streamOverride: stream),
            Invalid(
                "boundary_crossing_without_reference",
                "road/water crossing chunk boundary without cross-chunk reference",
                bundle,
                graphOverride: graph with { CrossChunkReferences = [] },
                streamOverride: stream),
            Invalid(
                "boundary_prefetch_disabled_runtime_travel",
                "boundary prefetch disabled while runtime travel mode requested",
                bundle,
                streamOverride: stream with
                {
                    Request = stream.Request with { BoundaryPrefetchEnabled = false, BoundaryPrefetchBandChunks = 0 },
                    BoundaryPrefetchChunkKeys = []
                }),
            Invalid(
                "prompt_text_source_of_truth",
                "prompt text as source of truth",
                bundle with { PromptTextIsSourceOfTruth = true },
                streamOverride: stream),
            Invalid(
                "real_geodata_dump_marker",
                "real geodata dump marker present",
                bundle with { RealGeodataDumpMarkerPresent = true, ContainsRealMapData = true },
                streamOverride: stream),
            Invalid(
                "rating_metadata_without_safe_fallback",
                "adult/rating metadata without safe fallback",
                bundle with { ContainsAdultOrRatingMetadata = true, SafeFallbackPolicyId = "" },
                streamOverride: stream),
            Invalid(
                "raster_or_unity_projection_output",
                "projection attempts raster or Unity output",
                bundle,
                streamOverride: stream,
                projectionOverride: projection with { NoRasterImages = false, NoUnityOutput = false })
        };

        return new OfflineGeoworldNegativeProof
        {
            Passed = scenarios.Count >= 13
                && scenarios.All(item => !item.ActualValid && item.ExpectedValid == item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static OfflineGeoworldNegativeScenario Invalid(
        string scenarioId,
        string mutation,
        OfflineGeoworldBundle bundle,
        OfflineGeoworldWorldSourceGraph? graphOverride = null,
        OfflineGeoworldStreamWindowPlan? streamOverride = null,
        OfflineGeoworldVisualProjectionSummary? projectionOverride = null)
    {
        var features = OfflineGeoworldNormalizer.Normalize(bundle);
        var graph = graphOverride ?? OfflineGeoworldWorldSourceGraphBuilder.Build(bundle, features);
        var stream = streamOverride ?? OfflineGeoworldStreamWindowScheduler.BuildPlan(graph);
        var projection = projectionOverride ?? OfflineGeoworldVisualProjectionBuilder.BuildProjection(graph, features, stream);
        var validation = OfflineGeoworldWorldSourceGraphValidator.Validate(bundle, features, graph, stream, projection);
        return new OfflineGeoworldNegativeScenario
        {
            ScenarioId = scenarioId,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static OfflineGeoworldBundleCatalog BuildCatalog(OfflineGeoworldBundle bundle) =>
        new()
        {
            Accepted = false,
            BundleCount = 1,
            BundleIds = [bundle.BundleId],
            Bundles = [bundle]
        };

    private static OfflineGeoworldWorkspaceBindingInventory BuildWorkspaceBindingInventory(
        string? projectRootPath)
    {
        var diagnostics = new List<OfflineGeoworldDiagnostic>();
        var projectRoot = Path.GetFullPath(projectRootPath ?? Directory.GetCurrentDirectory());
        var workspaceServicePath =
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspaceService.cs";
        var workspaceGeoworldPath =
            "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewGeoworldInspector.cs";
        var pagePath =
            "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/"
            + "VisualWorldStreamPreviewWorkspacePageControl.cs";
        var serviceText = ReadOptionalText(projectRoot, workspaceServicePath);
        var geoworldText = ReadOptionalText(projectRoot, workspaceGeoworldPath);
        var pageText = ReadOptionalText(projectRoot, pagePath);

        var readsGoal099 = serviceText.Contains("BuildGeoworldGroup", StringComparison.Ordinal)
            && geoworldText.Contains(OfflineGeoworldWorldSourceGraphVocabulary.RelativeOutputDirectory, StringComparison.Ordinal)
            && geoworldText.Contains(nameof(BundleCatalogJsonFileName), StringComparison.Ordinal)
            && geoworldText.Contains(nameof(VisualProjectionSummaryJsonFileName), StringComparison.Ordinal);
        var catalogIncludesGroup = geoworldText.Contains("\"geoworld\"", StringComparison.Ordinal)
            && geoworldText.Contains("Goal 099 Offline Geoworld", StringComparison.Ordinal);
        var pageDisplaysFields = pageText.Contains("OfflineBundleId", StringComparison.Ordinal)
            && pageText.Contains("GeoworldNormalizedFeatureCount", StringComparison.Ordinal)
            && pageText.Contains("BoundaryPrefetchStatus", StringComparison.Ordinal);
        var relativePaths = OfflineGeoworldWorldSourceGraphValidator.IsSafeRelativePath(
            OfflineGeoworldWorldSourceGraphVocabulary.RelativeOutputDirectory);

        AddIfFalse(readsGoal099, "goal099.workspace.goal099_reader_missing", workspaceGeoworldPath, diagnostics);
        AddIfFalse(catalogIncludesGroup, "goal099.workspace.group_missing", workspaceGeoworldPath, diagnostics);
        AddIfFalse(pageDisplaysFields, "goal099.workspace.winforms_fields_missing", pagePath, diagnostics);
        AddIfFalse(relativePaths, "goal099.workspace.relative_paths", "workspace", diagnostics);

        return new OfflineGeoworldWorkspaceBindingInventory
        {
            Passed = diagnostics.Count == 0,
            WorkspaceServiceReadsGoal099Evidence = readsGoal099,
            WorkspaceCatalogIncludesGeoworldGroup = catalogIncludesGroup,
            WinFormsPageDisplaysGeoworldFields = pageDisplaysFields,
            UsesRepositoryRelativeGoal099Paths = relativePaths,
            Diagnostics = diagnostics
        };
    }

    private static OfflineGeoworldSourceLineage BuildSourceLineage(string? projectRootPath)
    {
        var records = SourceLineageInputs()
            .Select(item => SourceLineageRecord(projectRootPath, item.Path, item.Purpose))
            .ToList();
        var report = ReadJson(projectRootPath, ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-source-adapter-catalog.json");
        var quality = ReadJson(projectRootPath, ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-quality-gate-scan.json");
        var taxonomy = ReadJson(projectRootPath, ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-normalized-feature-taxonomy.json");
        var acceptedFalse = report is not null && !TryGetBool(report.RootElement, "accepted");
        var noLfz = quality is not null && TryGetBool(quality.RootElement, "noLfzCodeCopied");
        var noNetwork = quality is not null && TryGetBool(quality.RootElement, "noNetworkOrProviderImplementation");
        var taxonomyExists = taxonomy is not null
            && taxonomy.RootElement.TryGetProperty("rows", out var rows)
            && rows.ValueKind == JsonValueKind.Array
            && rows.GetArrayLength() >= 8;

        return new OfflineGeoworldSourceLineage
        {
            Passed = records.All(item => item.Exists)
                && acceptedFalse
                && noLfz
                && noNetwork
                && taxonomyExists,
            Goal098AcceptedFalsePreserved = acceptedFalse,
            Goal098NoLfzCodeCopiedProven = noLfz,
            Goal098NoNetworkImplementationProven = noNetwork,
            Goal098TaxonomyExists = taxonomyExists,
            Records = records
        };
    }

    private static OfflineGeoworldQualityGateScan BuildQualityGate(
        OfflineGeoworldBundle bundle,
        OfflineGeoworldNormalizedFeatureSet normalized,
        OfflineGeoworldWorldSourceGraph graph,
        OfflineGeoworldStreamWindowPlan streamWindow,
        OfflineGeoworldBoundaryPrefetchProof boundaryPrefetchProof,
        OfflineGeoworldVisualProjectionSummary projection,
        OfflineGeoworldNegativeProof negativeProof,
        OfflineGeoworldSourceLineage sourceLineage,
        OfflineGeoworldWorkspaceBindingInventory workspaceBinding)
    {
        var validation = OfflineGeoworldWorldSourceGraphValidator.Validate(
            bundle,
            normalized,
            graph,
            streamWindow,
            projection);
        var diagnostics = new List<OfflineGeoworldDiagnostic>();
        diagnostics.AddRange(validation.Diagnostics);
        AddIfFalse(negativeProof.Passed, "goal099.negative_proof.failed", "negativeProof", diagnostics);
        AddIfFalse(sourceLineage.Passed, "goal099.source_lineage.failed", "sourceLineage", diagnostics);
        AddIfFalse(workspaceBinding.Passed, "goal099.workspace_binding.failed", "workspaceBinding", diagnostics);

        var noNetwork = !bundle.RuntimeOnlineFetchAttempted && !streamWindow.NetworkFetchAttempted;
        var noDump = !bundle.ContainsRawOsmDump
            && !bundle.ContainsRawFullAreaDump
            && !bundle.RealGeodataDumpMarkerPresent
            && graph.NoRawFullAreaDump;
        var passed = validation.Passed
            && negativeProof.Passed
            && sourceLineage.Passed
            && workspaceBinding.Passed
            && boundaryPrefetchProof.Passed
            && projection.Passed
            && noNetwork
            && noDump;

        return new OfflineGeoworldQualityGateScan
        {
            Accepted = false,
            Passed = passed,
            OfflineSyntheticBundleOnly = bundle.MetadataOnly && bundle.SyntheticOnly && !bundle.ContainsRealMapData,
            ValidBundleNormalizes = validation.Passed && normalized.GameplaySafeOnlyAfterNormalization,
            WorldSourceGraphBuilds = graph.Chunks.Count > 0 && graph.CrossChunkReferences.Count > 0,
            StreamWindowAndBoundaryPrefetchPass = boundaryPrefetchProof.Passed,
            VisualProjectionPasses = projection.Passed,
            WorkspaceBindingInventoryPasses = workspaceBinding.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            NoNetworkOrProviderImplementation = noNetwork,
            NoLfzCodeCopied = !bundle.ContainsLfzCopiedCodeMarker && sourceLineage.Goal098NoLfzCodeCopiedProven,
            NoRawGeodataDump = noDump,
            NormalizedFeatureCount = normalized.FeatureCount,
            WorldSourceGraphChunkCount = graph.Chunks.Count,
            StreamWindowChunkCount = streamWindow.RequiredChunkKeys.Count,
            ExpectedChangedPathPrefixes =
            [
                "src/LLMGameCreator.Application/Design/OfflineGeoworldWorldSourceGraph/",
                "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
                "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/Application/OfflineGeoworldWorldSourceGraph/",
                "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
                "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldWorldSourceGraphProductSmokeTests.cs",
                "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
                OfflineGeoworldWorldSourceGraphVocabulary.RelativeOutputDirectory + "/",
                "docs/agent-tasks/goal-099-offline-geoworld-worldsourcegraph-streaming/",
                "docs/CURRENT_GENERATOR_STATE.md",
                "docs/CURRENT_GENERATOR_STATE.json",
                "docs/CONTEXT_INDEX.md",
                "docs/FULL_GENERATOR_GOAL_QUEUE.md",
                "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
                ".devflow/artifact-scope/artifact-scope-policy.json"
            ],
            Diagnostics = OfflineGeoworldWorldSourceGraphValidator.SortDiagnostics(diagnostics)
        };
    }

    private static OfflineGeoworldReport BuildReport(
        OfflineGeoworldBundleCatalog catalog,
        OfflineGeoworldNormalizedFeatureSet normalized,
        OfflineGeoworldWorldSourceGraph graph,
        OfflineGeoworldStreamWindowPlan streamWindow,
        OfflineGeoworldBoundaryPrefetchProof boundary,
        OfflineGeoworldNegativeProof negative,
        OfflineGeoworldWorkspaceBindingInventory binding,
        OfflineGeoworldQualityGateScan quality,
        string catalogJson,
        string normalizedJson,
        string graphJson,
        string streamJson,
        string boundaryJson,
        string projectionJson,
        string negativeJson,
        string bindingJson,
        string lineageJson,
        string qualityJson) =>
        new()
        {
            Accepted = false,
            BundleCount = catalog.BundleCount,
            OfflineBundleId = OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId,
            RawDescriptorCount = catalog.Bundles.Single().RawDescriptors.Count,
            NormalizedFeatureCount = normalized.FeatureCount,
            WorldSourceGraphChunkCount = graph.Chunks.Count,
            StreamWindowChunkCount = streamWindow.RequiredChunkKeys.Count,
            BoundaryPrefetchChunkCount = streamWindow.BoundaryPrefetchChunkKeys.Count,
            BoundaryPrefetchPassed = boundary.Passed,
            NegativeProofPassed = negative.Passed,
            WorkspaceBindingPassed = binding.Passed,
            QualityGatePassed = quality.Passed,
            BundleCatalogHash = OfflineGeoworldWorldSourceGraphHash.Compute(catalogJson),
            NormalizedFeaturesHash = OfflineGeoworldWorldSourceGraphHash.Compute(normalizedJson),
            WorldSourceGraphHash = OfflineGeoworldWorldSourceGraphHash.Compute(graphJson),
            StreamWindowPlanHash = OfflineGeoworldWorldSourceGraphHash.Compute(streamJson),
            BoundaryPrefetchProofHash = OfflineGeoworldWorldSourceGraphHash.Compute(boundaryJson),
            VisualProjectionSummaryHash = OfflineGeoworldWorldSourceGraphHash.Compute(projectionJson),
            NegativeProofHash = OfflineGeoworldWorldSourceGraphHash.Compute(negativeJson),
            WorkspaceBindingInventoryHash = OfflineGeoworldWorldSourceGraphHash.Compute(bindingJson),
            SourceLineageHash = OfflineGeoworldWorldSourceGraphHash.Compute(lineageJson),
            QualityGateHash = OfflineGeoworldWorldSourceGraphHash.Compute(qualityJson)
        };

    private static string RenderReport(
        OfflineGeoworldReport report,
        OfflineGeoworldQualityGateScan quality,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 099 Offline Geoworld WorldSourceGraph Streaming Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 099 builds a deterministic synthetic offline geoworld bundle pipeline from metadata-only raw descriptors through normalized geofeatures, WorldSourceGraph chunks, stream-window boundary prefetch, compact visual projection and existing workspace binding. It performs no network fetch, copies no LFZ source, writes no real geodata dump and produces no raster or Unity output.",
            string.Empty,
            "## Pipeline",
            string.Empty,
            $"- offlineBundleId: {report.OfflineBundleId}",
            $"- rawDescriptorCount: {report.RawDescriptorCount}",
            $"- normalizedFeatureCount: {report.NormalizedFeatureCount}",
            $"- worldSourceGraphChunkCount: {report.WorldSourceGraphChunkCount}",
            $"- streamWindowChunkCount: {report.StreamWindowChunkCount}",
            $"- boundaryPrefetchChunkCount: {report.BoundaryPrefetchChunkCount}",
            $"- boundaryPrefetchPassed: {report.BoundaryPrefetchPassed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {report.NegativeProofPassed.ToString().ToLowerInvariant()}",
            $"- workspaceBindingPassed: {report.WorkspaceBindingPassed.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Quality Gate",
            string.Empty,
            $"- qualityGatePassed: {quality.Passed.ToString().ToLowerInvariant()}",
            $"- offlineSyntheticBundleOnly: {quality.OfflineSyntheticBundleOnly.ToString().ToLowerInvariant()}",
            $"- validBundleNormalizes: {quality.ValidBundleNormalizes.ToString().ToLowerInvariant()}",
            $"- worldSourceGraphBuilds: {quality.WorldSourceGraphBuilds.ToString().ToLowerInvariant()}",
            $"- streamWindowAndBoundaryPrefetchPass: {quality.StreamWindowAndBoundaryPrefetchPass.ToString().ToLowerInvariant()}",
            $"- visualProjectionPasses: {quality.VisualProjectionPasses.ToString().ToLowerInvariant()}",
            $"- workspaceBindingInventoryPasses: {quality.WorkspaceBindingInventoryPasses.ToString().ToLowerInvariant()}",
            $"- noNetworkOrProviderImplementation: {quality.NoNetworkOrProviderImplementation.ToString().ToLowerInvariant()}",
            $"- noLfzCodeCopied: {quality.NoLfzCodeCopied.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnitySchemaChanges: {quality.NoRuntimeUnitySchemaChanges.ToString().ToLowerInvariant()}",
            $"- noRawGeodataDump: {quality.NoRawGeodataDump.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMedia: {quality.NoBinaryOrRasterMedia.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- bundleCatalogHash: {report.BundleCatalogHash}",
            $"- normalizedFeaturesHash: {report.NormalizedFeaturesHash}",
            $"- worldSourceGraphHash: {report.WorldSourceGraphHash}",
            $"- streamWindowPlanHash: {report.StreamWindowPlanHash}",
            $"- boundaryPrefetchProofHash: {report.BoundaryPrefetchProofHash}",
            $"- visualProjectionSummaryHash: {report.VisualProjectionSummaryHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- workspaceBindingInventoryHash: {report.WorkspaceBindingInventoryHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<RawGeoFeatureDescriptor> ReplaceRaw(
        OfflineGeoworldBundle bundle,
        RawGeoFeatureDescriptor replacement) =>
        bundle.RawDescriptors
            .Select(item => item.RawDescriptorId == replacement.RawDescriptorId ? replacement : item)
            .ToList();

    private static IReadOnlyList<(string Path, string Purpose)> SourceLineageInputs() =>
    [
        ("docs/context/LFZ_ARCHIVE_ANALYSIS_MANIFEST.md", "LFZ pattern no-code-copy boundary"),
        ("docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md", "source to normalized feature pipeline pattern"),
        ("docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md", "stream window and boundary prefetch policy"),
        ("docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md", "cache provenance license adapter vocabulary"),
        ("docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md", "future staged geoworld sequence"),
        (".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-source-adapter-streaming-contract-report.md", "Goal098 accepted=false report"),
        (".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-source-adapter-catalog.json", "Goal098 adapter fixtures"),
        (".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-normalized-feature-taxonomy.json", "Goal098 taxonomy"),
        (".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-streaming-policy-matrix.json", "Goal098 stream policy"),
        (".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-negative-proof.json", "Goal098 negative proof")
    ];

    private static OfflineGeoworldSourceLineageRecord SourceLineageRecord(
        string? projectRootPath,
        string relativePath,
        string purpose)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        var exists = fullPath is not null && File.Exists(fullPath);
        var text = exists ? File.ReadAllText(fullPath!, Encoding.UTF8) : string.Empty;
        return new OfflineGeoworldSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = exists,
            Sha256 = exists ? OfflineGeoworldWorldSourceGraphHash.Compute(text) : string.Empty,
            Purpose = purpose
        };
    }

    private static JsonDocument? ReadJson(string? projectRootPath, string relativePath)
    {
        var fullPath = ResolveOptionalPath(projectRootPath, relativePath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return null;
        }

        return JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
    }

    private static bool TryGetBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(property.GetString(), out var value) && value,
            _ => false
        };
    }

    private static string ReadOptionalText(string projectRoot, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

    private static string? ResolveOptionalPath(string? projectRootPath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(
            Path.GetFullPath(projectRootPath),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void AddIfFalse(
        bool condition,
        string code,
        string target,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        if (!condition)
        {
            diagnostics.Add(OfflineGeoworldDiagnostic.Error(
                code,
                target,
                "Offline geoworld quality gate did not pass."));
        }
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path must stay under the project root.");
        }
    }
}
