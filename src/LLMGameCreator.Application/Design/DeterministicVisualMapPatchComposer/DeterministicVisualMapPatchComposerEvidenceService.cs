using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;

public sealed class DeterministicVisualMapPatchComposerEvidenceService
{
    public const string ReportMarkdownFileName = "visual-map-patch-composer-report.md";
    public const string CatalogJsonFileName = "visual-map-patch-catalog.json";
    public const string MaterializationManifestJsonFileName = "visual-map-patch-materialization-manifest.json";
    public const string FileLedgerJsonFileName = "visual-map-patch-file-ledger.json";
    public const string WaterFlowProofJsonFileName = "visual-map-patch-water-flow-proof.json";
    public const string ReachabilityProofJsonFileName = "visual-map-patch-reachability-proof.json";
    public const string LayeringProofJsonFileName = "visual-map-patch-layering-proof.json";
    public const string NegativeProofJsonFileName = "visual-map-patch-negative-proof.json";
    public const string SourceLineageJsonFileName = "visual-map-patch-source-lineage.json";
    public const string QualityGateScanJsonFileName = "visual-map-patch-quality-gate-scan.json";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static DeterministicVisualMapPatchComposerEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualMapPatchEvidenceResult Build(string? projectRootPath = null)
    {
        var request = DeterministicVisualMapPatchComposerFixtures.BuildDefaultRequest();
        var svgByPatchId = request.Patches
            .OrderBy(item => item.PatchId, StringComparer.Ordinal)
            .ToDictionary(item => item.PatchId, RenderSvg, StringComparer.Ordinal);
        var validation = DeterministicVisualMapPatchComposerValidator.Validate(
            request,
            DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds,
            svgByPatchId);
        var catalog = BuildCatalog(request);
        var manifest = BuildMaterializationManifest(request, svgByPatchId);
        var waterFlowProof = BuildWaterFlowProof(request);
        var reachabilityProof = BuildReachabilityProof(request);
        var layeringProof = BuildLayeringProof(request);
        var negativeProof = BuildNegativeProof(request);
        var sourceLineage = BuildSourceLineage(projectRootPath);
        var qualityGate = DeterministicVisualMapPatchComposerQualityGateScanner.Build(
            request,
            validation,
            waterFlowProof,
            reachabilityProof,
            layeringProof,
            negativeProof,
            sourceLineage,
            svgByPatchId);

        var catalogJson = Serialize(catalog);
        var manifestJson = Serialize(manifest);
        var waterFlowProofJson = Serialize(waterFlowProof);
        var reachabilityProofJson = Serialize(reachabilityProof);
        var layeringProofJson = Serialize(layeringProof);
        var negativeProofJson = Serialize(negativeProof);
        var sourceLineageJson = Serialize(sourceLineage);
        var qualityGateJson = Serialize(qualityGate);
        var fileLedger = BuildFileLedger(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [CatalogJsonFileName] = catalogJson,
                [MaterializationManifestJsonFileName] = manifestJson,
                [WaterFlowProofJsonFileName] = waterFlowProofJson,
                [ReachabilityProofJsonFileName] = reachabilityProofJson,
                [LayeringProofJsonFileName] = layeringProofJson,
                [NegativeProofJsonFileName] = negativeProofJson,
                [SourceLineageJsonFileName] = sourceLineageJson,
                [QualityGateScanJsonFileName] = qualityGateJson
            },
            request,
            svgByPatchId);
        var fileLedgerJson = Serialize(fileLedger);

        var reportWithoutHash = BuildReport(
            catalog,
            validation,
            waterFlowProof,
            reachabilityProof,
            layeringProof,
            negativeProof,
            sourceLineage,
            qualityGate,
            catalogJson,
            manifestJson,
            fileLedgerJson,
            waterFlowProofJson,
            reachabilityProofJson,
            layeringProofJson,
            negativeProofJson,
            sourceLineageJson,
            qualityGateJson);
        var reportMarkdownWithoutHash = RenderReport(reportWithoutHash, catalog, waterFlowProof, reachabilityProof, negativeProof, qualityGate, string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = DeterministicVisualMapPatchComposerHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(report, catalog, waterFlowProof, reachabilityProof, negativeProof, qualityGate, report.DeterministicReportHash);

        return new VisualMapPatchEvidenceResult
        {
            Catalog = catalog,
            MaterializationManifest = manifest,
            FileLedger = fileLedger,
            WaterFlowProof = waterFlowProof,
            ReachabilityProof = reachabilityProof,
            LayeringProof = layeringProof,
            NegativeProof = negativeProof,
            SourceLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            CatalogJson = catalogJson,
            MaterializationManifestJson = manifestJson,
            FileLedgerJson = fileLedgerJson,
            WaterFlowProofJson = waterFlowProofJson,
            ReachabilityProofJson = reachabilityProofJson,
            LayeringProofJson = layeringProofJson,
            NegativeProofJson = negativeProofJson,
            SourceLineageJson = sourceLineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown,
            SvgByPatchId = svgByPatchId
        };
    }

    public async Task<VisualMapPatchWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualMapPatchWriteResult> WriteAsync(
        string projectRootPath,
        VisualMapPatchEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            DeterministicVisualMapPatchComposerVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var patchDirectory = Path.GetFullPath(Path.Combine(outputDirectory, DeterministicVisualMapPatchComposerVocabulary.PatchRelativeDirectory));
        EnsureContained(outputDirectory, patchDirectory);
        Directory.CreateDirectory(patchDirectory);

        var write = new VisualMapPatchWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            PatchDirectoryPath = patchDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            MaterializationManifestJsonPath = Path.Combine(outputDirectory, MaterializationManifestJsonFileName),
            FileLedgerJsonPath = Path.Combine(outputDirectory, FileLedgerJsonFileName),
            WaterFlowProofJsonPath = Path.Combine(outputDirectory, WaterFlowProofJsonFileName),
            ReachabilityProofJsonPath = Path.Combine(outputDirectory, ReachabilityProofJsonFileName),
            LayeringProofJsonPath = Path.Combine(outputDirectory, LayeringProofJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            PatchSvgPaths = result.Catalog.Patches
                .OrderBy(item => item.PatchId, StringComparer.Ordinal)
                .Select(item => Path.Combine(outputDirectory, item.PatchSvgRelativePath.Replace('/', Path.DirectorySeparatorChar)))
                .ToList()
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.CatalogJsonPath, result.CatalogJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.MaterializationManifestJsonPath, result.MaterializationManifestJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.FileLedgerJsonPath, result.FileLedgerJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.WaterFlowProofJsonPath, result.WaterFlowProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ReachabilityProofJsonPath, result.ReachabilityProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.LayeringProofJsonPath, result.LayeringProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        foreach (var patch in result.Catalog.Patches.OrderBy(item => item.PatchId, StringComparer.Ordinal))
        {
            var patchPath = Path.Combine(outputDirectory, patch.PatchSvgRelativePath.Replace('/', Path.DirectorySeparatorChar));
            EnsureContained(outputDirectory, patchPath);
            await File.WriteAllTextAsync(patchPath, result.SvgByPatchId[patch.PatchId], Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        }

        return write;
    }

    public static VisualMapPatchNegativeProof BuildNegativeProof(VisualMapPatchComposerRequest baseline)
    {
        var firstPatch = baseline.Patches[0];
        var waterPatch = baseline.Patches.Single(item => item.PatchId == "water_coast_river_lake_marsh_24x16");
        var mixedPatch = baseline.Patches.Single(item => item.PatchId == "mixed_biome_settlement_creature_24x16");
        var firstCell = firstPatch.Cells[0];
        var firstAnchor = waterPatch.ObjectAnchors[0];
        var firstCreature = mixedPatch.CreatureMarkers[0];
        var adultOverlay = mixedPatch.Overlays.Single(item => item.AdultMetadataOnly);
        var baselineSvgs = baseline.Patches.ToDictionary(item => item.PatchId, RenderSvg, StringComparer.Ordinal);
        var unsafeSvgs = new Dictionary<string, string>(baselineSvgs, StringComparer.Ordinal)
        {
            [firstPatch.PatchId] = "<svg viewBox=\"0 0 288 192\"><script></script><image href=\"https://example.invalid/a.png\" /></svg>"
        };

        var scenarios = new List<VisualMapPatchNegativeScenario>
        {
            Invalid("absolute_output_path", "absolute output path", baseline with { OutputRelativeDirectory = "C:/unsafe/out" }),
            Invalid("absolute_patch_svg_path", "absolute patch SVG path", ReplacePatch(baseline, firstPatch with { PatchSvgRelativePath = "C:/unsafe/patch.svg" })),
            Invalid("prompt_text_as_source_of_truth", "prompt text as source of truth", baseline with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" }),
            Invalid("unknown_microtile_preview_ref", "unknown Goal086 microtile preview id", ReplacePatch(baseline, ReplaceCell(firstPatch, firstCell with { SourceMicrotilePreviewId = "fake_microtile_ref" }))),
            Invalid("coast_without_water_land_adjacency", "coast without water and land adjacency", ReplacePatch(baseline, ReplaceCell(waterPatch, waterPatch.Cells.Single(item => item.X == 10 && item.Y == 10) with { WaterKind = VisualMapPatchWaterKind.Coast, SourceMicrotilePreviewId = "water_coast_transition" }))),
            Invalid("river_without_flow_connectors", "river cells without flow connectors", ReplacePatch(baseline, waterPatch with { WaterFlows = [] })),
            Invalid("bridge_without_water_adjacency", "bridge without valid water adjacency", ReplacePatch(baseline, ReplaceObject(waterPatch, firstAnchor with { X = 10, Y = 10 }))),
            Invalid("road_connector_gap", "road/path connector gap", ReplacePatch(baseline, ReplaceRoad(firstPatch, firstPatch.RoadPaths[0] with { Nodes = [Node(0, 0), Node(2, 0)] }))),
            Invalid("settlement_on_water_without_path", "settlement on water without valid path", ReplacePatch(baseline, ReplaceSettlement(waterPatch, waterPatch.SettlementAnchors[0] with { X = 0, Y = 0, NearPathId = "missing_path" }))),
            Invalid("creature_unsafe_missing_bodyplan_equipment", "creature marker missing safe metadata", ReplacePatch(baseline, ReplaceCreature(mixedPatch, firstCreature with { BodyPlanId = "", EquipmentProfileId = "", RatingSafe = false }))),
            Invalid("adult_metadata_without_safe_fallback", "adult metadata route without safe fallback", ReplacePatch(baseline, ReplaceOverlay(mixedPatch, adultOverlay with { SafeFallbackMicrotilePreviewId = "" }))),
            Invalid("provider_candidate_treated_as_approved", "provider candidate treated as approved", ReplacePatch(baseline, ReplaceOverlay(mixedPatch, adultOverlay with { TreatProviderCandidateAsApprovedOutput = true }))),
            Invalid("duplicate_patch_id", "duplicate patch id", baseline with { Patches = [.. baseline.Patches, firstPatch] }),
            Invalid("missing_source_lineage", "missing source lineage", ReplacePatch(baseline, firstPatch with { SourceReferences = [] })),
            Invalid("svg_with_script_external_resource_base64", "unsafe SVG content", baseline, unsafeSvgs)
        };

        return new VisualMapPatchNegativeProof
        {
            Passed = scenarios.Count >= 12 && scenarios.All(item => item.ExpectedValid == item.ActualValid && !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualMapPatchCatalog BuildCatalog(VisualMapPatchComposerRequest request)
    {
        var entries = request.Patches
            .OrderBy(item => item.PatchId, StringComparer.Ordinal)
            .Select(item => new VisualMapPatchCatalogEntry
            {
                PatchId = item.PatchId,
                Width = item.Width,
                Height = item.Height,
                Seed = item.Seed,
                PatchSvgRelativePath = item.PatchSvgRelativePath,
                CellCount = item.Cells.Count,
                ObjectAnchorCount = item.ObjectAnchors.Count,
                RoadPathCount = item.RoadPaths.Count,
                WaterFlowCount = item.WaterFlows.Count,
                SettlementAnchorCount = item.SettlementAnchors.Count,
                CreatureMarkerCount = item.CreatureMarkers.Count,
                OverlayCount = item.Overlays.Count,
                ReferencedMicrotilePreviewIds = DeterministicVisualMapPatchComposerFixtures.ReferencedMicrotilePreviewIds(item),
                Cells = item.Cells
                    .OrderBy(cell => cell.Y)
                    .ThenBy(cell => cell.X)
                    .ToList()
            })
            .ToList();

        return new VisualMapPatchCatalog
        {
            Accepted = false,
            PatchCount = entries.Count,
            TotalCellCount = entries.Sum(item => item.CellCount),
            Patches = entries
        };
    }

    private static VisualMapPatchMaterializationManifest BuildMaterializationManifest(
        VisualMapPatchComposerRequest request,
        IReadOnlyDictionary<string, string> svgByPatchId) =>
        new()
        {
            GeneratorVersion = request.GeneratorVersion,
            OutputRelativeDirectory = request.OutputRelativeDirectory,
            PatchCount = request.Patches.Count,
            Patches = request.Patches
                .OrderBy(item => item.PatchId, StringComparer.Ordinal)
                .Select(item =>
                {
                    var svg = svgByPatchId[item.PatchId];
                    return new VisualMapPatchMaterializedPatch
                    {
                        PatchId = item.PatchId,
                        PatchSvgRelativePath = item.PatchSvgRelativePath,
                        SvgSha256 = DeterministicVisualMapPatchComposerHash.Compute(svg),
                        ByteLength = Encoding.UTF8.GetByteCount(svg),
                        ContainsViewBox = svg.Contains("viewBox=", StringComparison.Ordinal),
                        ExternalResourceFree = DeterministicVisualMapPatchComposerValidator.IsSvgSafe(svg),
                        ScriptFree = !svg.Contains("<script", StringComparison.OrdinalIgnoreCase),
                        RectCount = DeterministicVisualMapPatchComposerValidator.CountSvgRects(svg)
                    };
                })
                .ToList()
        };

    private static VisualMapPatchWaterFlowProof BuildWaterFlowProof(VisualMapPatchComposerRequest request)
    {
        var rows = request.Patches
            .OrderBy(item => item.PatchId, StringComparer.Ordinal)
            .Select(item => new VisualMapPatchWaterFlowProofRow
            {
                PatchId = item.PatchId,
                WaterCellCount = item.Cells.Count(cell => cell.WaterKind != VisualMapPatchWaterKind.None),
                CoastCellCount = item.Cells.Count(cell => cell.WaterKind == VisualMapPatchWaterKind.Coast),
                RiverNodeCount = item.WaterFlows.Where(flow => flow.WaterKind == VisualMapPatchWaterKind.River).Sum(flow => flow.Nodes.Count),
                LakeCellCount = item.Cells.Count(cell => cell.WaterKind == VisualMapPatchWaterKind.Lake),
                MarshCellCount = item.Cells.Count(cell => cell.WaterKind == VisualMapPatchWaterKind.Marsh)
            })
            .ToList();
        var allCells = request.Patches.SelectMany(item => item.Cells).ToList();
        var anchors = request.Patches.SelectMany(item => item.ObjectAnchors).ToList();
        var flowConnectorCount = request.Patches
            .SelectMany(item => item.WaterFlows)
            .SelectMany(item => item.Nodes)
            .Sum(item => item.Connectors.Count);
        var proof = new VisualMapPatchWaterFlowProof
        {
            SeaCovered = allCells.Any(item => item.WaterKind == VisualMapPatchWaterKind.Sea),
            CoastCovered = allCells.Any(item => item.WaterKind == VisualMapPatchWaterKind.Coast),
            RiverCovered = allCells.Any(item => item.WaterKind == VisualMapPatchWaterKind.River),
            LakeCovered = allCells.Any(item => item.WaterKind == VisualMapPatchWaterKind.Lake),
            MarshCovered = allCells.Any(item => item.WaterKind == VisualMapPatchWaterKind.Marsh),
            BridgeCovered = anchors.Any(item => item.ObjectKind == "bridge"),
            DockCovered = anchors.Any(item => item.ObjectKind == "dock"),
            FlowConnectorCount = flowConnectorCount,
            Rows = rows
        };

        return proof with
        {
            Passed = proof.SeaCovered
                && proof.CoastCovered
                && proof.RiverCovered
                && proof.LakeCovered
                && proof.MarshCovered
                && proof.BridgeCovered
                && proof.DockCovered
                && proof.FlowConnectorCount >= 2
        };
    }

    private static VisualMapPatchReachabilityProof BuildReachabilityProof(VisualMapPatchComposerRequest request)
    {
        var rows = request.Patches
            .OrderBy(item => item.PatchId, StringComparer.Ordinal)
            .Select(item => new VisualMapPatchReachabilityProofRow
            {
                PatchId = item.PatchId,
                RoadPathCount = item.RoadPaths.Count,
                RoadNodeCount = item.RoadPaths.Sum(path => path.Nodes.Count),
                SettlementAnchorCount = item.SettlementAnchors.Count,
                ObjectAnchorCount = item.ObjectAnchors.Count
            })
            .ToList();
        var roadNodes = request.Patches.SelectMany(item => item.RoadPaths).SelectMany(item => item.Nodes).ToList();
        var roadNodeCount = roadNodes.Count;
        var settlementCount = request.Patches.Sum(item => item.SettlementAnchors.Count);
        var objectCount = request.Patches.Sum(item => item.ObjectAnchors.Count);
        var roadsConnected = request.Patches.All(patch => patch.RoadPaths.All(path => PathIsAdjacent(path.Nodes)));
        var settlementsReachable = request.Patches.All(patch =>
            patch.SettlementAnchors.All(settlement =>
                patch.RoadPaths.Any(path => path.PathId == settlement.NearPathId
                    && path.Nodes.Any(node => Distance(node.X, node.Y, settlement.X, settlement.Y) <= 2))));
        var objectsReachable = request.Patches.All(patch =>
            patch.ObjectAnchors.Where(anchor => anchor.RequiresRoadAdjacency)
                .All(anchor => patch.RoadPaths.SelectMany(path => path.Nodes).Any(node => Distance(node.X, node.Y, anchor.X, anchor.Y) <= 1)));

        return new VisualMapPatchReachabilityProof
        {
            Passed = roadsConnected && settlementsReachable && objectsReachable && roadNodeCount > 0 && settlementCount > 0 && objectCount > 0,
            RoadsConnected = roadsConnected,
            SettlementsReachable = settlementsReachable,
            ObjectsReachable = objectsReachable,
            RoadNodeCount = roadNodeCount,
            SettlementAnchorCount = settlementCount,
            ObjectAnchorCount = objectCount,
            Rows = rows
        };
    }

    private static VisualMapPatchLayeringProof BuildLayeringProof(VisualMapPatchComposerRequest request)
    {
        var rows = request.Patches
            .OrderBy(item => item.PatchId, StringComparer.Ordinal)
            .Select(item => new VisualMapPatchLayeringProofRow
            {
                PatchId = item.PatchId,
                LayerKinds = item.Layers.OrderBy(layer => layer.Order).Select(layer => layer.Kind).ToList(),
                OverlayCount = item.Overlays.Count,
                AdultMetadataOnlyOverlayCount = item.Overlays.Count(overlay => overlay.AdultMetadataOnly)
            })
            .ToList();
        var layerOrderingStable = request.Patches.All(item =>
            item.Layers.Count > 0
            && item.Layers.Select(layer => layer.Order).Distinct().Count() == item.Layers.Count);
        var requiredKinds = new[]
        {
            VisualMapPatchLayerKind.Terrain,
            VisualMapPatchLayerKind.Water,
            VisualMapPatchLayerKind.Road,
            VisualMapPatchLayerKind.Object,
            VisualMapPatchLayerKind.Settlement,
            VisualMapPatchLayerKind.Creature,
            VisualMapPatchLayerKind.Overlay,
            VisualMapPatchLayerKind.RatingFallback
        };
        var allKindsCovered = requiredKinds.All(kind => request.Patches.Any(patch => patch.Layers.Any(layer => layer.Kind == kind)));
        var adultFallbackBound = request.Patches
            .SelectMany(item => item.Overlays)
            .Where(item => item.AdultMetadataOnly)
            .All(item => !string.IsNullOrWhiteSpace(item.SafeFallbackMicrotilePreviewId)
                && item.ProviderState == VisualMapPatchProviderState.CandidateQuarantine
                && !item.TreatProviderCandidateAsApprovedOutput);

        return new VisualMapPatchLayeringProof
        {
            Passed = layerOrderingStable && allKindsCovered && adultFallbackBound && rows.Any(item => item.AdultMetadataOnlyOverlayCount > 0),
            LayerOrderingStable = layerOrderingStable,
            TerrainWaterRoadObjectSettlementCreatureOverlayLayersCovered = allKindsCovered,
            AdultMetadataFallbackBound = adultFallbackBound,
            Rows = rows
        };
    }

    private static VisualMapPatchSourceLineage BuildSourceLineage(string? projectRootPath)
    {
        var records = SourceInputs()
            .Select(item => BuildSourceRecord(projectRootPath, item.Path, item.Tags))
            .ToList();
        var goal084Report = ReadText(projectRootPath, ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md");
        var goal085Report = ReadText(projectRootPath, ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md");
        var goal086Report = ReadText(projectRootPath, ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md");
        var goal086Catalog = ReadText(projectRootPath, ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-preview-catalog.json");
        var proof = new VisualMapPatchSourceLineage
        {
            Goal084ArtifactsGreen = goal084Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal084AcceptedFalse = goal084Report.Contains("accepted: false", StringComparison.Ordinal),
            Goal085ArtifactsGreen = goal085Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal085AcceptedFalse = goal085Report.Contains("accepted: false", StringComparison.Ordinal),
            Goal086ArtifactsGreen = goal086Report.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal086AcceptedFalse = goal086Report.Contains("accepted: false", StringComparison.Ordinal),
            Goal086CatalogRead = goal086Catalog.Contains("\"previewCount\": 24", StringComparison.Ordinal),
            SourceRecordCount = records.Count,
            Records = records
        };

        return proof with
        {
            Passed = proof.Goal084ArtifactsGreen
                && proof.Goal084AcceptedFalse
                && proof.Goal085ArtifactsGreen
                && proof.Goal085AcceptedFalse
                && proof.Goal086ArtifactsGreen
                && proof.Goal086AcceptedFalse
                && proof.Goal086CatalogRead
                && records.All(item => item.Exists)
        };
    }

    private static VisualMapPatchFileLedger BuildFileLedger(
        IReadOnlyDictionary<string, string> jsonFiles,
        VisualMapPatchComposerRequest request,
        IReadOnlyDictionary<string, string> svgByPatchId)
    {
        var entries = new List<VisualMapPatchFileLedgerEntry>();
        foreach (var file in jsonFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            entries.Add(LedgerEntry(file.Key, "json_manifest", file.Value));
        }

        foreach (var patch in request.Patches.OrderBy(item => item.PatchId, StringComparer.Ordinal))
        {
            entries.Add(LedgerEntry(patch.PatchSvgRelativePath, "patch_svg", svgByPatchId[patch.PatchId]));
        }

        return new VisualMapPatchFileLedger
        {
            Passed = entries.Count == jsonFiles.Count + request.Patches.Count
                && entries.All(item => item.Sha256.Length == 64)
                && entries.All(item => item.ByteLength > 0),
            FileCount = entries.Count,
            Files = entries
        };
    }

    private static VisualMapPatchFileLedgerEntry LedgerEntry(string relativePathUnderOutput, string role, string text)
    {
        var repoRelativePath = DeterministicVisualMapPatchComposerVocabulary.RelativeOutputDirectory + "/" + relativePathUnderOutput;
        return new VisualMapPatchFileLedgerEntry
        {
            RelativePath = repoRelativePath,
            Role = role,
            Sha256 = DeterministicVisualMapPatchComposerHash.Compute(text),
            ByteLength = Encoding.UTF8.GetByteCount(text)
        };
    }

    private static VisualMapPatchReport BuildReport(
        VisualMapPatchCatalog catalog,
        VisualMapPatchValidationResult validation,
        VisualMapPatchWaterFlowProof waterFlowProof,
        VisualMapPatchReachabilityProof reachabilityProof,
        VisualMapPatchLayeringProof layeringProof,
        VisualMapPatchNegativeProof negativeProof,
        VisualMapPatchSourceLineage sourceLineage,
        VisualMapPatchQualityGateScan qualityGate,
        string catalogJson,
        string manifestJson,
        string fileLedgerJson,
        string waterFlowProofJson,
        string reachabilityProofJson,
        string layeringProofJson,
        string negativeProofJson,
        string sourceLineageJson,
        string qualityGateJson) =>
        new()
        {
            Accepted = false,
            PatchCount = catalog.PatchCount,
            TotalCellCount = catalog.TotalCellCount,
            ValidationPassed = validation.Passed,
            WaterFlowProofPassed = waterFlowProof.Passed,
            ReachabilityProofPassed = reachabilityProof.Passed,
            LayeringProofPassed = layeringProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            SourceLineagePassed = sourceLineage.Passed,
            QualityGatePassed = qualityGate.Diagnostics.All(item => item.Severity != "error"),
            CatalogHash = DeterministicVisualMapPatchComposerHash.Compute(catalogJson),
            MaterializationManifestHash = DeterministicVisualMapPatchComposerHash.Compute(manifestJson),
            FileLedgerHash = DeterministicVisualMapPatchComposerHash.Compute(fileLedgerJson),
            WaterFlowProofHash = DeterministicVisualMapPatchComposerHash.Compute(waterFlowProofJson),
            ReachabilityProofHash = DeterministicVisualMapPatchComposerHash.Compute(reachabilityProofJson),
            LayeringProofHash = DeterministicVisualMapPatchComposerHash.Compute(layeringProofJson),
            NegativeProofHash = DeterministicVisualMapPatchComposerHash.Compute(negativeProofJson),
            SourceLineageHash = DeterministicVisualMapPatchComposerHash.Compute(sourceLineageJson),
            QualityGateHash = DeterministicVisualMapPatchComposerHash.Compute(qualityGateJson)
        };

    private static string RenderReport(
        VisualMapPatchReport report,
        VisualMapPatchCatalog catalog,
        VisualMapPatchWaterFlowProof waterFlowProof,
        VisualMapPatchReachabilityProof reachabilityProof,
        VisualMapPatchNegativeProof negativeProof,
        VisualMapPatchQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 087 Visual Map Patch Composer Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 087 adds a BCL-only Application-side deterministic visual map patch composer. It consumes Goal 084 visual asset metadata, Goal 085 part-pack rule-stack metadata and Goal 086 text SVG microtile previews, then writes compact 24x16 text SVG patch previews plus JSON evidence. It does not add dependencies, provider calls, Runtime behavior, Unity behavior, public GamePackage schema changes, binary or raster media, real adult content or prompt dumps.",
            string.Empty,
            "## Patch Fixtures",
            string.Empty,
            $"- patchCount: {report.PatchCount}",
            $"- totalCellCount: {report.TotalCellCount}",
            string.Empty
        };
        lines.AddRange(catalog.Patches.Select(item => $"- {item.PatchId}: {item.Width}x{item.Height}, cells={item.CellCount}, svg={item.PatchSvgRelativePath}"));
        lines.AddRange(
        [
            string.Empty,
            "## Water Biome Path Proof",
            string.Empty,
            $"- waterFlowProofPassed: {waterFlowProof.Passed.ToString().ToLowerInvariant()}",
            $"- seaCovered: {waterFlowProof.SeaCovered.ToString().ToLowerInvariant()}",
            $"- coastCovered: {waterFlowProof.CoastCovered.ToString().ToLowerInvariant()}",
            $"- riverCovered: {waterFlowProof.RiverCovered.ToString().ToLowerInvariant()}",
            $"- lakeCovered: {waterFlowProof.LakeCovered.ToString().ToLowerInvariant()}",
            $"- marshCovered: {waterFlowProof.MarshCovered.ToString().ToLowerInvariant()}",
            $"- bridgeCovered: {waterFlowProof.BridgeCovered.ToString().ToLowerInvariant()}",
            $"- dockCovered: {waterFlowProof.DockCovered.ToString().ToLowerInvariant()}",
            $"- flowConnectorCount: {waterFlowProof.FlowConnectorCount}",
            string.Empty,
            "## Reachability Proof",
            string.Empty,
            $"- reachabilityProofPassed: {reachabilityProof.Passed.ToString().ToLowerInvariant()}",
            $"- roadsConnected: {reachabilityProof.RoadsConnected.ToString().ToLowerInvariant()}",
            $"- settlementsReachable: {reachabilityProof.SettlementsReachable.ToString().ToLowerInvariant()}",
            $"- objectsReachable: {reachabilityProof.ObjectsReachable.ToString().ToLowerInvariant()}",
            $"- roadNodeCount: {reachabilityProof.RoadNodeCount}",
            string.Empty,
            "## Validation",
            string.Empty,
            $"- validationPassed: {report.ValidationPassed.ToString().ToLowerInvariant()}",
            $"- negativeProofPassed: {negativeProof.Passed.ToString().ToLowerInvariant()}",
            $"- negativeScenarioCount: {negativeProof.ScenarioCount}",
            $"- rejectedNegativeScenarioCount: {negativeProof.RejectedCount}",
            string.Empty,
            "## Boundaries",
            string.Empty,
            $"- svgTextOnlyPreviews: {qualityGate.SvgTextOnlyPreviews.ToString().ToLowerInvariant()}",
            $"- allReferencesKnownGoal086Microtiles: {qualityGate.AllReferencesKnownGoal086Microtiles.ToString().ToLowerInvariant()}",
            $"- noExternalDependenciesAdded: {qualityGate.NoExternalDependenciesAdded.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noProviderCalls: {qualityGate.NoProviderCalls.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            $"- noExplicitAdultContent: {qualityGate.NoExplicitAdultContent.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- catalogHash: {report.CatalogHash}",
            $"- materializationManifestHash: {report.MaterializationManifestHash}",
            $"- fileLedgerHash: {report.FileLedgerHash}",
            $"- waterFlowProofHash: {report.WaterFlowProofHash}",
            $"- reachabilityProofHash: {report.ReachabilityProofHash}",
            $"- layeringProofHash: {report.LayeringProofHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}"
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderSvg(VisualMapPatchDefinition patch)
    {
        const int cellSize = 12;
        var width = patch.Width * cellSize;
        var height = patch.Height * cellSize;
        var lines = new List<string>
        {
            $"<svg viewBox=\"0 0 {width} {height}\" data-patch-id=\"{Escape(patch.PatchId)}\" data-seed=\"{patch.Seed}\">",
            "  <rect x=\"0\" y=\"0\" width=\"288\" height=\"192\" fill=\"#101418\" />"
        };

        foreach (var cell in patch.Cells.OrderBy(item => item.Y).ThenBy(item => item.X))
        {
            lines.Add($"  <rect x=\"{cell.X * cellSize}\" y=\"{cell.Y * cellSize}\" width=\"12\" height=\"12\" fill=\"{ColorFor(cell)}\" data-cell-id=\"{cell.CellId}\" data-microtile=\"{Escape(cell.SourceMicrotilePreviewId)}\" />");
        }

        foreach (var road in patch.RoadPaths.OrderBy(item => item.PathId, StringComparer.Ordinal))
        {
            lines.Add(RenderPolyline(road.Nodes, cellSize, "#6b5436", "4", road.PathId));
        }

        foreach (var flow in patch.WaterFlows.OrderBy(item => item.FlowId, StringComparer.Ordinal))
        {
            lines.Add(RenderPolyline(flow.Nodes, cellSize, "#bce7ee", "3", flow.FlowId));
        }

        foreach (var anchor in patch.ObjectAnchors.OrderBy(item => item.AnchorId, StringComparer.Ordinal))
        {
            lines.Add($"  <rect x=\"{anchor.X * cellSize + 3}\" y=\"{anchor.Y * cellSize + 3}\" width=\"6\" height=\"6\" fill=\"#f2d16b\" data-anchor-id=\"{Escape(anchor.AnchorId)}\" />");
        }

        foreach (var settlement in patch.SettlementAnchors.OrderBy(item => item.SettlementId, StringComparer.Ordinal))
        {
            lines.Add($"  <rect x=\"{settlement.X * cellSize + 2}\" y=\"{settlement.Y * cellSize + 2}\" width=\"8\" height=\"8\" fill=\"#e4c298\" data-settlement-id=\"{Escape(settlement.SettlementId)}\" />");
        }

        foreach (var creature in patch.CreatureMarkers.OrderBy(item => item.MarkerId, StringComparer.Ordinal))
        {
            lines.Add($"  <circle cx=\"{creature.X * cellSize + 6}\" cy=\"{creature.Y * cellSize + 6}\" r=\"4\" fill=\"#263040\" stroke=\"#f2f0d0\" stroke-width=\"1\" data-creature-id=\"{Escape(creature.MarkerId)}\" />");
        }

        foreach (var overlay in patch.Overlays.OrderBy(item => item.OverlayId, StringComparer.Ordinal))
        {
            var fill = overlay.AdultMetadataOnly ? "none" : "#ffffff";
            var stroke = overlay.AdultMetadataOnly ? "#d8edf0" : "#dfe8ff";
            lines.Add($"  <rect x=\"{overlay.X * cellSize}\" y=\"{overlay.Y * cellSize}\" width=\"{overlay.Width * cellSize}\" height=\"{overlay.Height * cellSize}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"1\" opacity=\"0.45\" data-overlay-id=\"{Escape(overlay.OverlayId)}\" />");
        }

        lines.Add("</svg>");
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderPolyline(IReadOnlyList<VisualMapPatchPathNode> nodes, int cellSize, string color, string width, string id)
    {
        var points = string.Join(
            " ",
            nodes.Select(item => $"{item.X * cellSize + cellSize / 2},{item.Y * cellSize + cellSize / 2}"));
        return $"  <polyline points=\"{points}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"{width}\" stroke-linecap=\"round\" stroke-linejoin=\"round\" data-path-id=\"{Escape(id)}\" />";
    }

    private static string ColorFor(VisualMapPatchCell cell) =>
        cell.WaterKind switch
        {
            VisualMapPatchWaterKind.Sea => "#195478",
            VisualMapPatchWaterKind.Coast => "#c5a765",
            VisualMapPatchWaterKind.River => "#3aa2ba",
            VisualMapPatchWaterKind.Lake => "#438ba0",
            VisualMapPatchWaterKind.Marsh => "#3f6f52",
            _ => cell.TerrainBiome switch
            {
                VisualMapPatchTerrainBiome.Forest => "#346d3d",
                VisualMapPatchTerrainBiome.Mountain => "#747a7c",
                VisualMapPatchTerrainBiome.Snow => "#dfeef2",
                VisualMapPatchTerrainBiome.Desert => "#d7aa62",
                VisualMapPatchTerrainBiome.LavaAsh => "#6b3530",
                _ => "#5f9f47"
            }
        };

    private static VisualMapPatchNegativeScenario Invalid(
        string id,
        string mutation,
        VisualMapPatchComposerRequest request,
        IReadOnlyDictionary<string, string>? svgByPatchId = null)
    {
        var validation = DeterministicVisualMapPatchComposerValidator.Validate(
            request,
            DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds,
            svgByPatchId);
        return new VisualMapPatchNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static VisualMapPatchComposerRequest ReplacePatch(
        VisualMapPatchComposerRequest request,
        VisualMapPatchDefinition replacement) =>
        request with
        {
            Patches = request.Patches.Select(item => item.PatchId == replacement.PatchId ? replacement : item).ToList()
        };

    private static VisualMapPatchDefinition ReplaceCell(VisualMapPatchDefinition patch, VisualMapPatchCell replacement) =>
        patch with { Cells = patch.Cells.Select(item => item.CellId == replacement.CellId ? replacement : item).ToList() };

    private static VisualMapPatchDefinition ReplaceObject(VisualMapPatchDefinition patch, VisualMapPatchObjectAnchor replacement) =>
        patch with { ObjectAnchors = patch.ObjectAnchors.Select(item => item.AnchorId == replacement.AnchorId ? replacement : item).ToList() };

    private static VisualMapPatchDefinition ReplaceRoad(VisualMapPatchDefinition patch, VisualMapPatchRoadPath replacement) =>
        patch with { RoadPaths = patch.RoadPaths.Select(item => item.PathId == replacement.PathId ? replacement : item).ToList() };

    private static VisualMapPatchDefinition ReplaceSettlement(VisualMapPatchDefinition patch, VisualMapPatchSettlementAnchor replacement) =>
        patch with { SettlementAnchors = patch.SettlementAnchors.Select(item => item.SettlementId == replacement.SettlementId ? replacement : item).ToList() };

    private static VisualMapPatchDefinition ReplaceCreature(VisualMapPatchDefinition patch, VisualMapPatchCreatureMarker replacement) =>
        patch with { CreatureMarkers = patch.CreatureMarkers.Select(item => item.MarkerId == replacement.MarkerId ? replacement : item).ToList() };

    private static VisualMapPatchDefinition ReplaceOverlay(VisualMapPatchDefinition patch, VisualMapPatchOverlay replacement) =>
        patch with { Overlays = patch.Overlays.Select(item => item.OverlayId == replacement.OverlayId ? replacement : item).ToList() };

    private static VisualMapPatchPathNode Node(int x, int y) =>
        new() { X = x, Y = y, Connectors = [VisualMapPatchConnector.East] };

    private static IReadOnlyList<(string Path, IReadOnlyList<string> Tags)> SourceInputs() =>
    [
        (".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md", ["goal084", "report"]),
        (".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-catalog.json", ["goal084", "catalog"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md", ["goal085", "report"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-catalog.json", ["goal085", "catalog"]),
        (".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/water-biome-coverage-matrix.json", ["goal085", "water"]),
        (".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md", ["goal086", "report"]),
        (".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-preview-catalog.json", ["goal086", "catalog"]),
        (".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materialization-manifest.json", ["goal086", "manifest"]),
        (".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-water-biome-proof.json", ["goal086", "water"]),
        (".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-negative-proof.json", ["goal086", "negative"]),
        (".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-quality-gate-scan.json", ["goal086", "quality"]),
        ("docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md", ["deepsearch", "synthesis"]),
        ("docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md", ["deepsearch", "water_biome"]),
        ("docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md", ["deepsearch", "pseudo3d"]),
        ("docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md", ["deepsearch", "settlement"])
    ];

    private static VisualMapPatchSourceLineageRecord BuildSourceRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new VisualMapPatchSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : DeterministicVisualMapPatchComposerHash.Compute(text),
            PurposeTags = tags
        };
    }

    private static bool PathIsAdjacent(IReadOnlyList<VisualMapPatchPathNode> nodes)
    {
        for (var index = 1; index < nodes.Count; index++)
        {
            if (Distance(nodes[index - 1].X, nodes[index - 1].Y, nodes[index].X, nodes[index].Y) != 1)
            {
                return false;
            }
        }

        return nodes.Count >= 2;
    }

    private static int Distance(int ax, int ay, int bx, int by) =>
        Math.Abs(ax - bx) + Math.Abs(ay - by);

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

    private static string Escape(string text) =>
        text.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
