using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public sealed class WorldScaleRegionMapEvidenceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-038-world-scale-region-map-foundation";
    public const string RegionGraphSummaryJsonFileName = "region-graph-summary.json";
    public const string ReachabilityMatrixJsonFileName = "reachability-matrix.json";
    public const string ChunkedWorldConfigPreludeJsonFileName = "chunked-world-config-prelude.json";
    public const string TraversalItineraryMatrixJsonFileName = "traversal-itinerary-matrix.json";
    public const string InvalidMatrixJsonFileName = "invalid-world-scale-diagnostics-matrix.json";
    public const string ReportMarkdownFileName = "world-scale-region-map-foundation-report.md";
    public const string FinalGate = WorldScaleRegionMapVocabulary.FinalGate;
    public const string ProductSmokeRoute = WorldScaleRegionMapVocabulary.ProductSmokeRoute;

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public WorldScaleEvidenceResult Build()
    {
        var validator = new WorldScaleRegionMapValidator();
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var graphSummary = WorldScaleRegionMapCatalog.BuildSummary(graphs);
        var reachability = new WorldScaleReachabilityPlanner().BuildMatrix(graphs);
        var mapPacks = new FiniteMapPackBuilder().BuildMapPacksByFileName(graphs);
        var chunkConfig = new ChunkedWorldConfigPreludeBuilder().Build(graphs, mapPacks);
        var itineraryMatrix = new WorldScaleTraversalItineraryMatrix
        {
            Itineraries = reachability.Scenarios
                .SelectMany(item => item.RequiredTargetItineraries)
                .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
                .ThenBy(item => item.TargetRegionId, StringComparer.Ordinal)
                .ToList()
        };
        itineraryMatrix = itineraryMatrix with { ItineraryCount = itineraryMatrix.Itineraries.Count };
        var invalidMatrix = validator.BuildInvalidMatrix(graphs, mapPacks, chunkConfig);

        var diagnostics = WorldScaleRegionMapCatalog.SortDiagnostics(
            graphs.SelectMany(validator.ValidateGraph)
                .Concat(reachability.Scenarios.SelectMany(validator.ValidateReachability))
                .Concat(mapPacks.Values.SelectMany(mapPack =>
                    validator.ValidateMapPack(mapPack, graphs.Single(graph => graph.ScenarioId == mapPack.ScenarioId))))
                .Concat(validator.ValidateChunkConfig(chunkConfig, graphs, mapPacks)));

        var artifactJson = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [RegionGraphSummaryJsonFileName] = Serialize(graphSummary),
            [ReachabilityMatrixJsonFileName] = Serialize(reachability),
            [ChunkedWorldConfigPreludeJsonFileName] = Serialize(chunkConfig),
            [TraversalItineraryMatrixJsonFileName] = Serialize(itineraryMatrix),
            [InvalidMatrixJsonFileName] = Serialize(invalidMatrix)
        };
        foreach (var pair in mapPacks.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            artifactJson[pair.Key] = Serialize(pair.Value);
        }

        var metamodule = graphs.Single(item => item.ScenarioId == "metamodule_kingdoms");
        var reportWithoutHash = new WorldScaleRegionMapFoundationReport
        {
            Accepted = false,
            ImplementationStatus = "GREEN",
            ProductSmokeRoute = ProductSmokeRoute,
            Goal037AcceptedByUserHandoff = true,
            ScenarioCount = graphs.Count,
            RegionGraphCount = graphs.Count,
            TotalRegionCount = graphSummary.TotalRegionCount,
            TotalTravelEdgeCount = graphSummary.TotalTravelEdgeCount,
            RequiredReachabilityCount = reachability.RequiredTargetCount,
            ReachableRequiredTargetCount = reachability.ReachableRequiredTargetCount,
            RequiredReachabilityPassed = reachability.AllRequiredTargetsReachable,
            FiniteMapPackCount = mapPacks.Count,
            ChunkConfigScenarioCount = chunkConfig.ScenarioCount,
            MetamoduleKingdomGroupCount = metamodule.Kingdoms.Count,
            MetamoduleSpeciesArchetypeSlotRefCount = metamodule.Kingdoms.SelectMany(item => item.SpeciesArchetypeSlotRefs).Distinct(StringComparer.Ordinal).Count(),
            InvalidScenarioCount = invalidMatrix.ScenarioCount,
            InvalidMatrixPassed = invalidMatrix.Passed,
            RegionGraphSummaryHash = Hash(artifactJson[RegionGraphSummaryJsonFileName]),
            ReachabilityMatrixHash = Hash(artifactJson[ReachabilityMatrixJsonFileName]),
            ChunkConfigPreludeHash = Hash(artifactJson[ChunkedWorldConfigPreludeJsonFileName]),
            TraversalItineraryMatrixHash = Hash(artifactJson[TraversalItineraryMatrixJsonFileName]),
            InvalidMatrixHash = Hash(artifactJson[InvalidMatrixJsonFileName]),
            FiniteMapPackHashes = mapPacks
                .OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => Hash(artifactJson[item.Key]))
                .ToList(),
            Diagnostics = diagnostics
        };
        var routeKindsCovered = graphSummary.RouteKindsCovered.ToHashSet(StringComparer.Ordinal);
        var report = reportWithoutHash with
        {
            ContractProofPassed = diagnostics.All(item => item.Severity != "error")
                && graphs.Count == 4
                && WorldScaleRegionMapVocabulary.RequiredRouteKinds.All(routeKindsCovered.Contains)
                && reachability.AllRequiredTargetsReachable
                && mapPacks.Count == 4
                && mapPacks.Values.Any(item => item.CoordinateKind == "axial_hex")
                && chunkConfig.ScenarioCount == 4
                && metamodule.Kingdoms.Count == 7
                && reportWithoutHash.MetamoduleSpeciesArchetypeSlotRefCount >= 112
                && invalidMatrix.Passed
                && graphs.All(item => item.BoundaryClaims.AllFalse),
            DeterministicHash = Hash(Serialize(reportWithoutHash))
        };

        return new WorldScaleEvidenceResult
        {
            RegionGraphSummary = graphSummary,
            ReachabilityMatrix = reachability,
            FiniteMapPacksByFileName = mapPacks,
            ChunkConfigPrelude = chunkConfig,
            TraversalItineraryMatrix = itineraryMatrix,
            InvalidMatrix = invalidMatrix,
            Report = report,
            ArtifactJsonByFileName = artifactJson,
            ReportMarkdown = RenderReport(report, graphSummary, reachability, mapPacks, chunkConfig, invalidMatrix)
        };
    }

    public async Task<WorldScaleEvidenceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<WorldScaleEvidenceWriteResult> WriteAsync(
        string projectRootPath,
        WorldScaleEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var written = new List<string>();
        foreach (var file in result.ArtifactJsonByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, file.Key);
            await File.WriteAllTextAsync(path, file.Value, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
            written.Add(path);
        }

        var reportPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        written.Add(reportPath);

        return new WorldScaleEvidenceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            WrittenFiles = written.Order(StringComparer.Ordinal).ToList(),
            ReportMarkdownPath = reportPath
        };
    }

    private static string RenderReport(
        WorldScaleRegionMapFoundationReport report,
        WorldScaleRegionGraphSummary graphSummary,
        WorldScaleReachabilityMatrix reachability,
        IReadOnlyDictionary<string, WorldScaleFiniteMapPack> mapPacks,
        WorldScaleChunkedWorldConfigPrelude chunkConfig,
        WorldScaleInvalidMatrix invalidMatrix)
    {
        var lines = new List<string>
        {
            "# World-scale Region Map Foundation Report",
            string.Empty,
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            "- accepted=false",
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- finalStatus: {report.FinalStatus}",
            $"- manualGate: {report.ManualGate}",
            $"- required marker: {FinalGate} required",
            $"- productSmokeRoute: {report.ProductSmokeRoute}",
            $"- goal037AcceptedByUserHandoff: {report.Goal037AcceptedByUserHandoff.ToString().ToLowerInvariant()}",
            $"- contractProofPassed: {report.ContractProofPassed.ToString().ToLowerInvariant()}",
            $"- scenarioCount: {report.ScenarioCount}",
            $"- regionGraphCount: {report.RegionGraphCount}",
            $"- totalRegionCount: {report.TotalRegionCount}",
            $"- totalTravelEdgeCount: {report.TotalTravelEdgeCount}",
            $"- requiredReachabilityCount: {report.RequiredReachabilityCount}",
            $"- reachableRequiredTargetCount: {report.ReachableRequiredTargetCount}",
            $"- finiteMapPackCount: {report.FiniteMapPackCount}",
            $"- chunkConfigScenarioCount: {report.ChunkConfigScenarioCount}",
            $"- metamoduleKingdomGroupCount: {report.MetamoduleKingdomGroupCount}",
            $"- metamoduleSpeciesArchetypeSlotRefCount: {report.MetamoduleSpeciesArchetypeSlotRefCount}",
            $"- invalidScenarioCount: {report.InvalidScenarioCount}",
            $"- invalidMatrixPassed: {report.InvalidMatrixPassed.ToString().ToLowerInvariant()}",
            $"- noRuntimeUiUnityGamePackageProviderLlmRagLuaGeneratorLibraryChanges: {report.NoRuntimeUiUnityGamePackageProviderLlmRagLuaGeneratorLibraryChanges.ToString().ToLowerInvariant()}",
            $"- regionGraphSummaryHash: {report.RegionGraphSummaryHash}",
            $"- reachabilityMatrixHash: {report.ReachabilityMatrixHash}",
            $"- chunkConfigPreludeHash: {report.ChunkConfigPreludeHash}",
            $"- traversalItineraryMatrixHash: {report.TraversalItineraryMatrixHash}",
            $"- invalidMatrixHash: {report.InvalidMatrixHash}",
            $"- reportHash: {report.DeterministicHash}",
            string.Empty,
            "## What became more real",
            string.Empty,
            "Goal 037 hybrid expansion outputs now feed deterministic world-scale region graphs, reachability proof, finite map pack summaries and chunk-config prelude records that later runtime/export goals can consume.",
            string.Empty,
            "## Route kinds",
            string.Empty,
            "- " + string.Join(", ", graphSummary.RouteKindsCovered),
            string.Empty,
            "## Reachability",
            string.Empty
        };

        lines.AddRange(reachability.Scenarios.Select(item => $"- {item.ScenarioId}: start={item.StartRegionId}, required={item.RequiredTargetRegionIds.Count}, reachableRequired={item.RequiredTargetRegionIds.Count - item.UnreachableRequiredRegionIds.Count}, totalCostTargets={string.Join(",", item.RouteCostTotalsByTarget.Select(pair => pair.Key + "=" + pair.Value))}"));
        lines.Add(string.Empty);
        lines.Add("## Finite map packs");
        lines.Add(string.Empty);
        lines.AddRange(mapPacks.Values.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).Select(item => $"- {item.ScenarioId}: map={item.MapId}, coordinateKind={item.CoordinateKind}, regionBindings={item.RegionBindings.Count}, landmarks={item.LandmarkPlacements.Count}, routeSummaries={item.RouteSummaries.Count}, previewCells={item.PreviewCells.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Chunk config prelude");
        lines.Add(string.Empty);
        lines.AddRange(chunkConfig.Scenarios.Select(item => $"- {item.ScenarioId}: chunkSize={item.ChunkSize}, coverageRegions={item.RegionToChunkCoverage.Count}, chunkIds={item.FiniteMapProjection.CoveredChunkIds.Count}, futureRules={item.FutureGenerationRuleRefs.Count}"));
        lines.Add(string.Empty);
        lines.Add("## Invalid/fake/leak matrix");
        lines.Add(string.Empty);
        lines.AddRange(invalidMatrix.Scenarios.Select(item => $"- {item.ScenarioId}: expectedStatus={item.ExpectedStatus}, actualStatus={item.ActualStatus}, codes={string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}"));
        lines.Add(string.Empty);
        lines.Add("## Boundaries");
        lines.Add(string.Empty);
        lines.Add("No Runtime, UI, Unity, GamePackage schema, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change is required by this evidence.");
        lines.Add(string.Empty);
        lines.Add($"{FinalGate} required");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string Hash(string text) => WorldScaleRegionMapCatalog.ComputeHash(text);

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
