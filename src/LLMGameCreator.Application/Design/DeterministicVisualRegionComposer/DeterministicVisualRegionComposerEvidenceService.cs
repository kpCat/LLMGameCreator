using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;

public sealed class DeterministicVisualRegionComposerEvidenceService
{
    public const string ReportMarkdownFileName = "visual-region-composer-report.md";
    public const string DefinitionJsonFileName = "visual-region-definition.json";
    public const string PatchPlacementIndexJsonFileName = "visual-region-patch-placement-index.json";
    public const string ChunkIndexJsonFileName = "visual-region-chunk-index.json";
    public const string BiomeDistributionProofJsonFileName = "visual-region-biome-distribution-proof.json";
    public const string WaterNetworkProofJsonFileName = "visual-region-water-network-proof.json";
    public const string RoadReachabilityProofJsonFileName = "visual-region-road-reachability-proof.json";
    public const string LayerTransitionProofJsonFileName = "visual-region-layer-transition-proof.json";
    public const string ObjectPlacementProofJsonFileName = "visual-region-object-placement-proof.json";
    public const string NegativeProofJsonFileName = "visual-region-negative-proof.json";
    public const string SourceLineageJsonFileName = "visual-region-source-lineage.json";
    public const string QualityGateScanJsonFileName = "visual-region-quality-gate-scan.json";
    public const string SurfaceOverviewSvgFileName = "region-overview-surface.svg";
    public const string UndergroundOverviewSvgFileName = "region-overview-underground.svg";
    public const string CombinedOverviewSvgFileName = "region-overview-combined.svg";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static DeterministicVisualRegionComposerEvidenceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualRegionEvidenceResult Build(string? projectRootPath = null)
    {
        var definition = DeterministicVisualRegionComposerFixtures.BuildDefaultDefinition();
        var overviewSvgs = RenderOverviewSvgs(definition);
        var validation = DeterministicVisualRegionComposerValidator.Validate(
            definition,
            DeterministicVisualRegionComposerFixtures.KnownGoal087PatchIds,
            overviewSvgs);
        var patchPlacementIndex = BuildPatchPlacementIndex(definition);
        var chunkIndex = BuildChunkIndex(definition);
        var biomeDistributionProof = BuildBiomeDistributionProof(definition);
        var waterNetworkProof = BuildWaterNetworkProof(definition);
        var roadReachabilityProof = BuildRoadReachabilityProof(definition);
        var layerTransitionProof = BuildLayerTransitionProof(definition);
        var objectPlacementProof = BuildObjectPlacementProof(definition);
        var negativeProof = BuildNegativeProof(definition);
        var sourceLineage = BuildSourceLineage(definition, projectRootPath);
        var qualityGate = DeterministicVisualRegionComposerQualityGateScanner.Build(
            definition,
            validation,
            patchPlacementIndex,
            chunkIndex,
            waterNetworkProof,
            roadReachabilityProof,
            layerTransitionProof,
            objectPlacementProof,
            negativeProof,
            sourceLineage,
            overviewSvgs);

        var definitionJson = Serialize(definition);
        var patchPlacementIndexJson = Serialize(patchPlacementIndex);
        var chunkIndexJson = Serialize(chunkIndex);
        var biomeDistributionProofJson = Serialize(biomeDistributionProof);
        var waterNetworkProofJson = Serialize(waterNetworkProof);
        var roadReachabilityProofJson = Serialize(roadReachabilityProof);
        var layerTransitionProofJson = Serialize(layerTransitionProof);
        var objectPlacementProofJson = Serialize(objectPlacementProof);
        var negativeProofJson = Serialize(negativeProof);
        var sourceLineageJson = Serialize(sourceLineage);
        var qualityGateJson = Serialize(qualityGate);

        var reportWithoutHash = BuildReport(
            definition,
            validation,
            patchPlacementIndex,
            waterNetworkProof,
            roadReachabilityProof,
            layerTransitionProof,
            objectPlacementProof,
            negativeProof,
            qualityGate,
            definitionJson,
            patchPlacementIndexJson,
            chunkIndexJson,
            biomeDistributionProofJson,
            waterNetworkProofJson,
            roadReachabilityProofJson,
            layerTransitionProofJson,
            objectPlacementProofJson,
            negativeProofJson,
            sourceLineageJson,
            qualityGateJson,
            overviewSvgs);
        var reportMarkdownWithoutHash = RenderReport(
            reportWithoutHash,
            biomeDistributionProof,
            waterNetworkProof,
            roadReachabilityProof,
            layerTransitionProof,
            objectPlacementProof,
            negativeProof,
            qualityGate,
            string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = DeterministicVisualRegionComposerHash.Compute(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(
            report,
            biomeDistributionProof,
            waterNetworkProof,
            roadReachabilityProof,
            layerTransitionProof,
            objectPlacementProof,
            negativeProof,
            qualityGate,
            report.DeterministicReportHash);

        return new VisualRegionEvidenceResult
        {
            Definition = definition,
            PatchPlacementIndex = patchPlacementIndex,
            ChunkIndex = chunkIndex,
            BiomeDistributionProof = biomeDistributionProof,
            WaterNetworkProof = waterNetworkProof,
            RoadReachabilityProof = roadReachabilityProof,
            LayerTransitionProof = layerTransitionProof,
            ObjectPlacementProof = objectPlacementProof,
            NegativeProof = negativeProof,
            SourceLineage = sourceLineage,
            QualityGateScan = qualityGate,
            Report = report,
            DefinitionJson = definitionJson,
            PatchPlacementIndexJson = patchPlacementIndexJson,
            ChunkIndexJson = chunkIndexJson,
            BiomeDistributionProofJson = biomeDistributionProofJson,
            WaterNetworkProofJson = waterNetworkProofJson,
            RoadReachabilityProofJson = roadReachabilityProofJson,
            LayerTransitionProofJson = layerTransitionProofJson,
            ObjectPlacementProofJson = objectPlacementProofJson,
            NegativeProofJson = negativeProofJson,
            SourceLineageJson = sourceLineageJson,
            QualityGateScanJson = qualityGateJson,
            ReportMarkdown = reportMarkdown,
            OverviewSvgByFileName = overviewSvgs
        };
    }

    public async Task<VisualRegionWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualRegionWriteResult> WriteAsync(
        string projectRootPath,
        VisualRegionEvidenceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            DeterministicVisualRegionComposerVocabulary.RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualRegionWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            DefinitionJsonPath = Path.Combine(outputDirectory, DefinitionJsonFileName),
            PatchPlacementIndexJsonPath = Path.Combine(outputDirectory, PatchPlacementIndexJsonFileName),
            ChunkIndexJsonPath = Path.Combine(outputDirectory, ChunkIndexJsonFileName),
            BiomeDistributionProofJsonPath = Path.Combine(outputDirectory, BiomeDistributionProofJsonFileName),
            WaterNetworkProofJsonPath = Path.Combine(outputDirectory, WaterNetworkProofJsonFileName),
            RoadReachabilityProofJsonPath = Path.Combine(outputDirectory, RoadReachabilityProofJsonFileName),
            LayerTransitionProofJsonPath = Path.Combine(outputDirectory, LayerTransitionProofJsonFileName),
            ObjectPlacementProofJsonPath = Path.Combine(outputDirectory, ObjectPlacementProofJsonFileName),
            NegativeProofJsonPath = Path.Combine(outputDirectory, NegativeProofJsonFileName),
            SourceLineageJsonPath = Path.Combine(outputDirectory, SourceLineageJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            OverviewSvgPaths = result.OverviewSvgByFileName.Keys
                .OrderBy(item => item, StringComparer.Ordinal)
                .Select(item => Path.Combine(outputDirectory, item))
                .ToList()
        };

        await File.WriteAllTextAsync(write.ReportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.DefinitionJsonPath, result.DefinitionJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.PatchPlacementIndexJsonPath, result.PatchPlacementIndexJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ChunkIndexJsonPath, result.ChunkIndexJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.BiomeDistributionProofJsonPath, result.BiomeDistributionProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.WaterNetworkProofJsonPath, result.WaterNetworkProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.RoadReachabilityProofJsonPath, result.RoadReachabilityProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.LayerTransitionProofJsonPath, result.LayerTransitionProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.ObjectPlacementProofJsonPath, result.ObjectPlacementProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.NegativeProofJsonPath, result.NegativeProofJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.SourceLineageJsonPath, result.SourceLineageJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(write.QualityGateScanJsonPath, result.QualityGateScanJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        foreach (var (fileName, svg) in result.OverviewSvgByFileName.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var path = Path.Combine(outputDirectory, fileName);
            EnsureContained(outputDirectory, path);
            await File.WriteAllTextAsync(path, svg, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        }

        return write;
    }

    public static VisualRegionNegativeProof BuildNegativeProof(VisualRegionDefinition baseline)
    {
        var surface = baseline.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.SurfaceLayerId);
        var underground = baseline.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.UndergroundLayerId);
        var firstSurfacePlacement = surface.PatchPlacements[0];
        var riverPlacement = surface.PatchPlacements.Single(item => item.PlacementId == "surface_p01_04");
        var firstSettlement = baseline.Settlements[0];
        var firstCreature = baseline.CreaturePlacements[0];
        var adultOverlay = baseline.Overlays.Single(item => item.AdultMetadataOnly);
        var safeSvgs = RenderOverviewSvgs(baseline);
        var unsafeSvgs = new Dictionary<string, string>(safeSvgs, StringComparer.Ordinal)
        {
            [SurfaceOverviewSvgFileName] = "<svg viewBox=\"0 0 216 216\"><script></script><image href=\"https://example.invalid/a.png\" /></svg>"
        };

        var scenarios = new List<VisualRegionNegativeScenario>
        {
            Invalid("wrong_dimensions", "wrong 144x144x2 dimensions", baseline with { Width = 143 }),
            Invalid("wrong_layer_count", "missing underground layer", baseline with { LayerCount = 1, Layers = [surface] }),
            Invalid("wrong_patch_grid", "wrong patch grid", baseline with { PatchGridColumns = 5 }),
            Invalid("unknown_goal087_patch_id", "unknown Goal087 patch id", ReplacePlacement(baseline, firstSurfacePlacement with { SourceGoal087PatchId = "fake_goal087_patch" })),
            Invalid("placement_outside_bounds", "patch placement outside bounds", ReplacePlacement(baseline, firstSurfacePlacement with { GridX = 6, X = 144 })),
            Invalid("duplicate_patch_coordinate", "duplicate patch coordinate in same layer", ReplaceLayer(baseline, surface with { PatchPlacements = [.. surface.PatchPlacements, firstSurfacePlacement with { PlacementId = "surface_duplicate_00_00" }] })),
            Invalid("missing_water_network", "missing water network when water is declared", baseline with { WaterNetwork = new VisualRegionWaterNetwork { DeclaresWater = false, Segments = [] } }),
            Invalid("connector_mismatch", "river/coast connector mismatch", ReplacePlacement(baseline, riverPlacement with { WaterConnectors = riverPlacement.WaterConnectors with { West = "coast" } })),
            Invalid("road_not_connected", "road network disconnected from required anchors", baseline with { RoadNetwork = baseline.RoadNetwork with { Edges = baseline.RoadNetwork.Edges.Where(item => item.EdgeId != "surface_garrison_to_caravan" && item.EdgeId != "surface_caravan_to_artifact" && item.EdgeId != "surface_caravan_to_south_gate").ToList() } }),
            Invalid("transition_without_pair", "surface-underground transition without paired gate", ReplaceGate(baseline, baseline.GateTransitions[0] with { Paired = false, UndergroundGateId = "" })),
            Invalid("settlement_on_invalid_water", "settlement/castle on invalid water terrain", ReplaceSettlement(baseline, firstSettlement with { TerrainKind = "water" })),
            Invalid("creature_missing_bodyplan_equipment", "creature missing bodyplan and equipment metadata", ReplaceCreature(baseline, firstCreature with { BodyPlanId = "", EquipmentProfileId = "", RatingSafe = false })),
            Invalid("adult_rating_without_safe_fallback", "adult/rating metadata without safe fallback", ReplaceOverlay(baseline, adultOverlay with { SafeFallbackRefId = "" })),
            Invalid("prompt_text_as_source_of_truth", "prompt text as source of truth", baseline with { PromptTextIsSourceOfTruth = true, SourceOfTruthKind = "provider_prompt_text" }),
            Invalid("provider_candidate_treated_as_approved", "provider candidate treated as approved", ReplaceOverlay(baseline, adultOverlay with { TreatProviderCandidateAsApprovedOutput = true })),
            Invalid("absolute_source_path", "absolute source path", baseline with { SourceReferences = [baseline.SourceReferences[0] with { RelativePath = "C:/unsafe/source.json" }, .. baseline.SourceReferences.Skip(1)] }),
            Invalid("unsafe_svg_script_external_base64", "unsafe SVG overview", baseline, unsafeSvgs),
            Invalid("heavy_raw_cell_dump", "explicit 41,472 raw cell dump mode", baseline with { HeavyRawCellMode = true, ExplicitRawCellRecordCount = DeterministicVisualRegionComposerVocabulary.DerivedLogicalCellCount })
        };

        return new VisualRegionNegativeProof
        {
            Passed = scenarios.Count >= 18
                && scenarios.All(item => item.ExpectedValid == item.ActualValid && !item.ActualValid && item.Diagnostics.Any(diagnostic => diagnostic.Severity == "error")),
            ScenarioCount = scenarios.Count,
            RejectedCount = scenarios.Count(item => !item.ActualValid),
            MatchedExpectationCount = scenarios.Count(item => item.ExpectedValid == item.ActualValid),
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualRegionPatchPlacementIndex BuildPatchPlacementIndex(VisualRegionDefinition definition)
    {
        var placements = definition.Layers.SelectMany(item => item.PatchPlacements).ToList();
        return new VisualRegionPatchPlacementIndex
        {
            Passed = placements.Count == DeterministicVisualRegionComposerVocabulary.TotalPatchPlacements
                && definition.Layers.All(layer => layer.PatchPlacements.Count == DeterministicVisualRegionComposerVocabulary.PatchPlacementsPerLayer)
                && placements.All(item => DeterministicVisualRegionComposerFixtures.KnownGoal087PatchIds.Contains(item.SourceGoal087PatchId)),
            PatchPlacementCount = placements.Count,
            SurfacePatchPlacementCount = definition.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.SurfaceLayerId).PatchPlacements.Count,
            UndergroundPatchPlacementCount = definition.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.UndergroundLayerId).PatchPlacements.Count,
            DerivedLogicalCellCount = definition.DerivedLogicalCellCount,
            AllPatchIdsKnownGoal087 = placements.All(item => DeterministicVisualRegionComposerFixtures.KnownGoal087PatchIds.Contains(item.SourceGoal087PatchId)),
            Placements = placements
                .OrderBy(item => item.LayerId, StringComparer.Ordinal)
                .ThenBy(item => item.GridY)
                .ThenBy(item => item.GridX)
                .Select(item => new VisualRegionPatchPlacementIndexRow
                {
                    PlacementId = item.PlacementId,
                    LayerId = item.LayerId,
                    SourceGoal087PatchId = item.SourceGoal087PatchId,
                    GridX = item.GridX,
                    GridY = item.GridY,
                    TransformSummary = $"rot={item.Transform.RotationDegrees};mirrorX={item.Transform.MirrorX.ToString().ToLowerInvariant()};mirrorY={item.Transform.MirrorY.ToString().ToLowerInvariant()};palette={item.Transform.RepaletteProfileId}"
                })
                .ToList()
        };
    }

    private static VisualRegionChunkIndex BuildChunkIndex(VisualRegionDefinition definition)
    {
        var chunks = definition.Layers.SelectMany(item => item.Chunks)
            .OrderBy(item => item.LayerId, StringComparer.Ordinal)
            .ThenBy(item => item.GridY)
            .ThenBy(item => item.GridX)
            .ToList();
        return new VisualRegionChunkIndex
        {
            Passed = chunks.Count == DeterministicVisualRegionComposerVocabulary.TotalPatchPlacements
                && chunks.All(item => item.CompactRleRows.Count > 0),
            ChunkCount = chunks.Count,
            Chunks = chunks
        };
    }

    private static VisualRegionBiomeDistributionProof BuildBiomeDistributionProof(VisualRegionDefinition definition)
    {
        var surfaceRequired = new[] { "grass", "forest", "mountain", "snow", "desert", "lava_ash" };
        var undergroundRequired = new[] { "cave", "rock", "lava", "underground_water", "mushroom", "ruin" };
        var bands = definition.BiomeBands.ToList();
        var bandIds = bands.Where(item => item.EstimatedCellCount > 0).Select(item => (item.LayerId, item.BiomeId)).ToHashSet();
        var surfacePassed = surfaceRequired.All(item => bandIds.Contains((DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, item)));
        var undergroundPassed = undergroundRequired.All(item => bandIds.Contains((DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, item)));

        return new VisualRegionBiomeDistributionProof
        {
            Passed = surfacePassed && undergroundPassed,
            SurfaceCoveragePassed = surfacePassed,
            UndergroundCoveragePassed = undergroundPassed,
            Bands = bands.OrderBy(item => item.LayerId, StringComparer.Ordinal).ThenBy(item => item.BiomeId, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualRegionWaterNetworkProof BuildWaterNetworkProof(VisualRegionDefinition definition)
    {
        var kinds = definition.WaterNetwork.Segments.Select(item => item.WaterKind).ToHashSet(StringComparer.Ordinal);
        var crossingIds = definition.WaterNetwork.Segments.SelectMany(item => item.CrossingObjectIds).ToHashSet(StringComparer.Ordinal);
        var connectorRejected = BuildNegativeProof(definition).Scenarios
            .Any(item => item.ScenarioId == "connector_mismatch" && !item.ActualValid);
        var proof = new VisualRegionWaterNetworkProof
        {
            SeaCovered = kinds.Contains("sea"),
            LakeCovered = kinds.Contains("lake"),
            CoastCovered = kinds.Contains("sea"),
            RiverCovered = kinds.Contains("river"),
            MarshCovered = kinds.Contains("marsh"),
            BridgeCovered = crossingIds.Contains("surface_bridge_river"),
            DockCovered = crossingIds.Contains("surface_dock_west"),
            UndergroundWaterCovered = kinds.Contains("underground_water"),
            LavaBoundaryMetadataCovered = kinds.Contains("lava_boundary") && definition.WaterNetwork.DeclaresLavaBoundaryMetadata,
            ConnectorMismatchesRejectedByValidator = connectorRejected,
            SegmentCount = definition.WaterNetwork.Segments.Count
        };
        return proof with
        {
            Passed = proof.SeaCovered
                && proof.LakeCovered
                && proof.CoastCovered
                && proof.RiverCovered
                && proof.MarshCovered
                && proof.BridgeCovered
                && proof.DockCovered
                && proof.UndergroundWaterCovered
                && proof.LavaBoundaryMetadataCovered
                && proof.ConnectorMismatchesRejectedByValidator
        };
    }

    private static VisualRegionRoadReachabilityProof BuildRoadReachabilityProof(VisualRegionDefinition definition)
    {
        var reachable = ReachableRoadNodes(definition.RoadNetwork);
        var required = definition.RoadNetwork.Nodes.Where(item => item.RequiredAnchor).Select(item => item.NodeId).ToHashSet(StringComparer.Ordinal);
        var objectNodes = definition.ObjectPlacements.Where(item => item.RequiresRoadConnection).Select(item => item.RoadNodeId).ToHashSet(StringComparer.Ordinal);
        var settlementNodes = definition.Settlements.Select(item => item.RoadNodeId).ToHashSet(StringComparer.Ordinal);
        var requiredReachable = required.All(reachable.Contains);
        return new VisualRegionRoadReachabilityProof
        {
            Passed = requiredReachable && objectNodes.All(reachable.Contains) && settlementNodes.All(reachable.Contains),
            RoadsConnected = requiredReachable,
            SettlementCastleGarrisonCaravanAnchorsReachable = settlementNodes.All(reachable.Contains),
            ObjectAnchorsReachable = objectNodes.All(reachable.Contains),
            RoadNodeCount = definition.RoadNetwork.Nodes.Count,
            RoadEdgeCount = definition.RoadNetwork.Edges.Count,
            ReachableRequiredAnchorIds = required.Where(reachable.Contains).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static VisualRegionLayerTransitionProof BuildLayerTransitionProof(VisualRegionDefinition definition) =>
        new()
        {
            Passed = definition.GateTransitions.Count >= 2
                && definition.GateTransitions.All(item => item.Paired && !string.IsNullOrWhiteSpace(item.SurfaceGateId) && !string.IsNullOrWhiteSpace(item.UndergroundGateId)),
            GatePairCount = definition.GateTransitions.Count,
            GateTransitions = definition.GateTransitions.OrderBy(item => item.TransitionId, StringComparer.Ordinal).ToList()
        };

    private static VisualRegionObjectPlacementProof BuildObjectPlacementProof(VisualRegionDefinition definition)
    {
        var roles = definition.Settlements.Select(item => item.Role).ToHashSet(StringComparer.Ordinal);
        var objectKinds = definition.ObjectPlacements.Select(item => item.ObjectKind).ToHashSet(StringComparer.Ordinal);
        var coveredRoles = new[] { "castle", "settlement", "garrison", "caravan" }.All(roles.Contains);
        var coveredObjects = new[] { "mine", "bridge", "dock", "object" }.All(objectKinds.Contains)
            && definition.CreaturePlacements.Count >= 4;
        return new VisualRegionObjectPlacementProof
        {
            Passed = coveredRoles && coveredObjects,
            SettlementCount = definition.Settlements.Count,
            ObjectCount = definition.ObjectPlacements.Count,
            CreatureCount = definition.CreaturePlacements.Count,
            CastleSettlementGarrisonCaravanCovered = coveredRoles,
            MineBridgeDockObjectCreatureCovered = coveredObjects
        };
    }

    private static VisualRegionSourceLineage BuildSourceLineage(VisualRegionDefinition definition, string? projectRootPath)
    {
        var records = definition.SourceReferences
            .Select(item => BuildSourceRecord(projectRootPath, item.RelativePath, [item.SourceKind, item.SourceId]))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        var report084 = ReadText(projectRootPath, ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md");
        var report085 = ReadText(projectRootPath, ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md");
        var report086 = ReadText(projectRootPath, ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md");
        var report087 = ReadText(projectRootPath, ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md");
        var catalog087 = ReadText(projectRootPath, ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-catalog.json");

        var lineage = new VisualRegionSourceLineage
        {
            Goal084ArtifactsGreen = report084.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal085ArtifactsGreen = report085.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal086ArtifactsGreen = report086.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal087ArtifactsGreen = report087.Contains("implementationStatus: GREEN", StringComparison.Ordinal),
            Goal087AcceptedFalseArtifactPreserved = report087.Contains("accepted: false", StringComparison.Ordinal),
            Goal087CatalogRead = catalog087.Contains("\"patchCount\": 3", StringComparison.Ordinal)
                && DeterministicVisualRegionComposerFixtures.KnownGoal087PatchIds.All(id => catalog087.Contains(id, StringComparison.Ordinal)),
            SourceRecordCount = records.Count,
            Records = records
        };
        return lineage with
        {
            Passed = lineage.Goal084ArtifactsGreen
                && lineage.Goal085ArtifactsGreen
                && lineage.Goal086ArtifactsGreen
                && lineage.Goal087ArtifactsGreen
                && lineage.Goal087AcceptedFalseArtifactPreserved
                && lineage.Goal087CatalogRead
                && records.All(item => item.Exists)
        };
    }

    private static VisualRegionReport BuildReport(
        VisualRegionDefinition definition,
        VisualRegionValidationResult validation,
        VisualRegionPatchPlacementIndex patchPlacementIndex,
        VisualRegionWaterNetworkProof waterNetworkProof,
        VisualRegionRoadReachabilityProof roadReachabilityProof,
        VisualRegionLayerTransitionProof layerTransitionProof,
        VisualRegionObjectPlacementProof objectPlacementProof,
        VisualRegionNegativeProof negativeProof,
        VisualRegionQualityGateScan qualityGate,
        string definitionJson,
        string patchPlacementIndexJson,
        string chunkIndexJson,
        string biomeDistributionProofJson,
        string waterNetworkProofJson,
        string roadReachabilityProofJson,
        string layerTransitionProofJson,
        string objectPlacementProofJson,
        string negativeProofJson,
        string sourceLineageJson,
        string qualityGateJson,
        IReadOnlyDictionary<string, string> overviewSvgs)
    {
        var surface = definition.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.SurfaceLayerId);
        var underground = definition.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.UndergroundLayerId);
        return new VisualRegionReport
        {
            Accepted = false,
            SurfaceWidth = surface.Width,
            SurfaceHeight = surface.Height,
            UndergroundWidth = underground.Width,
            UndergroundHeight = underground.Height,
            PatchPlacementCount = patchPlacementIndex.PatchPlacementCount,
            DerivedLogicalCellCount = patchPlacementIndex.DerivedLogicalCellCount,
            ValidationPassed = validation.Passed,
            CompactArtifactsPassed = qualityGate.CompactArtifactsPassed,
            WaterNetworkProofPassed = waterNetworkProof.Passed,
            RoadReachabilityProofPassed = roadReachabilityProof.Passed,
            LayerTransitionProofPassed = layerTransitionProof.Passed,
            ObjectPlacementProofPassed = objectPlacementProof.Passed,
            NegativeProofPassed = negativeProof.Passed,
            QualityGatePassed = qualityGate.Diagnostics.All(item => item.Severity != "error"),
            DefinitionHash = DeterministicVisualRegionComposerHash.Compute(definitionJson),
            PatchPlacementIndexHash = DeterministicVisualRegionComposerHash.Compute(patchPlacementIndexJson),
            ChunkIndexHash = DeterministicVisualRegionComposerHash.Compute(chunkIndexJson),
            BiomeDistributionProofHash = DeterministicVisualRegionComposerHash.Compute(biomeDistributionProofJson),
            WaterNetworkProofHash = DeterministicVisualRegionComposerHash.Compute(waterNetworkProofJson),
            RoadReachabilityProofHash = DeterministicVisualRegionComposerHash.Compute(roadReachabilityProofJson),
            LayerTransitionProofHash = DeterministicVisualRegionComposerHash.Compute(layerTransitionProofJson),
            ObjectPlacementProofHash = DeterministicVisualRegionComposerHash.Compute(objectPlacementProofJson),
            NegativeProofHash = DeterministicVisualRegionComposerHash.Compute(negativeProofJson),
            SourceLineageHash = DeterministicVisualRegionComposerHash.Compute(sourceLineageJson),
            QualityGateHash = DeterministicVisualRegionComposerHash.Compute(qualityGateJson),
            SurfaceOverviewHash = DeterministicVisualRegionComposerHash.Compute(overviewSvgs[SurfaceOverviewSvgFileName]),
            UndergroundOverviewHash = DeterministicVisualRegionComposerHash.Compute(overviewSvgs[UndergroundOverviewSvgFileName]),
            CombinedOverviewHash = DeterministicVisualRegionComposerHash.Compute(overviewSvgs[CombinedOverviewSvgFileName])
        };
    }

    private static string RenderReport(
        VisualRegionReport report,
        VisualRegionBiomeDistributionProof biomeDistributionProof,
        VisualRegionWaterNetworkProof waterNetworkProof,
        VisualRegionRoadReachabilityProof roadReachabilityProof,
        VisualRegionLayerTransitionProof layerTransitionProof,
        VisualRegionObjectPlacementProof objectPlacementProof,
        VisualRegionNegativeProof negativeProof,
        VisualRegionQualityGateScan qualityGate,
        string deterministicReportHash)
    {
        var lines = new List<string>
        {
            "# Goal 088 Visual Region Composer Report",
            string.Empty,
            $"- implementationStatus: {report.ImplementationStatus}",
            $"- accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- manualGate: {report.ManualGate} required",
            $"- deterministicReportHash: {deterministicReportHash}",
            string.Empty,
            "## Summary",
            string.Empty,
            "Goal 088 adds a BCL-only Application-side deterministic visual region composer. It consumes Goal 084 visual asset metadata, Goal 085 visual part-pack rule-stack metadata, Goal 086 microtile metadata and Goal 087 24x16 map patches, then assembles a compact Heroes-scale logical region: 144x144 surface plus 144x144 underground. Evidence is patch placements, chunk indexes, compact RLE summaries, proof manifests and safe text SVG overviews. It does not generate raster images, call providers, mutate Runtime, mutate Unity, change public GamePackage schema, change Lua/generator-library, add dependencies or dump prompt/provider output.",
            string.Empty,
            "## Region Fixture",
            string.Empty,
            $"- regionId: {report.RegionId}",
            $"- surfaceDimensions: {report.SurfaceWidth}x{report.SurfaceHeight}",
            $"- undergroundDimensions: {report.UndergroundWidth}x{report.UndergroundHeight}",
            $"- patchGridPerLayer: {DeterministicVisualRegionComposerVocabulary.PatchGridColumns}x{DeterministicVisualRegionComposerVocabulary.PatchGridRows}",
            $"- patchPlacementCount: {report.PatchPlacementCount}",
            $"- derivedLogicalCellCount: {report.DerivedLogicalCellCount}",
            $"- compactArtifactsPassed: {report.CompactArtifactsPassed.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Biome Distribution",
            string.Empty,
            $"- biomeDistributionProofPassed: {biomeDistributionProof.Passed.ToString().ToLowerInvariant()}",
            $"- surfaceCoveragePassed: {biomeDistributionProof.SurfaceCoveragePassed.ToString().ToLowerInvariant()}",
            $"- undergroundCoveragePassed: {biomeDistributionProof.UndergroundCoveragePassed.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Water Network Proof",
            string.Empty,
            $"- waterNetworkProofPassed: {waterNetworkProof.Passed.ToString().ToLowerInvariant()}",
            $"- seaCovered: {waterNetworkProof.SeaCovered.ToString().ToLowerInvariant()}",
            $"- coastCovered: {waterNetworkProof.CoastCovered.ToString().ToLowerInvariant()}",
            $"- riverCovered: {waterNetworkProof.RiverCovered.ToString().ToLowerInvariant()}",
            $"- lakeCovered: {waterNetworkProof.LakeCovered.ToString().ToLowerInvariant()}",
            $"- marshCovered: {waterNetworkProof.MarshCovered.ToString().ToLowerInvariant()}",
            $"- bridgeCovered: {waterNetworkProof.BridgeCovered.ToString().ToLowerInvariant()}",
            $"- dockCovered: {waterNetworkProof.DockCovered.ToString().ToLowerInvariant()}",
            $"- undergroundWaterCovered: {waterNetworkProof.UndergroundWaterCovered.ToString().ToLowerInvariant()}",
            $"- lavaBoundaryMetadataCovered: {waterNetworkProof.LavaBoundaryMetadataCovered.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Road Reachability Proof",
            string.Empty,
            $"- roadReachabilityProofPassed: {roadReachabilityProof.Passed.ToString().ToLowerInvariant()}",
            $"- roadsConnected: {roadReachabilityProof.RoadsConnected.ToString().ToLowerInvariant()}",
            $"- settlementCastleGarrisonCaravanAnchorsReachable: {roadReachabilityProof.SettlementCastleGarrisonCaravanAnchorsReachable.ToString().ToLowerInvariant()}",
            $"- objectAnchorsReachable: {roadReachabilityProof.ObjectAnchorsReachable.ToString().ToLowerInvariant()}",
            $"- roadNodeCount: {roadReachabilityProof.RoadNodeCount}",
            string.Empty,
            "## Layer Transition And Placement Proof",
            string.Empty,
            $"- layerTransitionProofPassed: {layerTransitionProof.Passed.ToString().ToLowerInvariant()}",
            $"- gatePairCount: {layerTransitionProof.GatePairCount}",
            $"- objectPlacementProofPassed: {objectPlacementProof.Passed.ToString().ToLowerInvariant()}",
            $"- settlementCount: {objectPlacementProof.SettlementCount}",
            $"- objectCount: {objectPlacementProof.ObjectCount}",
            $"- creatureCount: {objectPlacementProof.CreatureCount}",
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
            $"- safeSvgOverviewsPassed: {qualityGate.SafeSvgOverviewsPassed.ToString().ToLowerInvariant()}",
            $"- noRuntimeUnityProviderSchemaProjectDependencyChanges: {qualityGate.NoRuntimeUnityProviderSchemaProjectDependencyChanges.ToString().ToLowerInvariant()}",
            $"- noBinaryOrRasterMediaAdded: {qualityGate.NoBinaryOrRasterMediaAdded.ToString().ToLowerInvariant()}",
            $"- noPromptDumps: {qualityGate.NoPromptDumps.ToString().ToLowerInvariant()}",
            $"- noExplicitAdultContent: {qualityGate.NoExplicitAdultContent.ToString().ToLowerInvariant()}",
            string.Empty,
            "## Artifact Hashes",
            string.Empty,
            $"- definitionHash: {report.DefinitionHash}",
            $"- patchPlacementIndexHash: {report.PatchPlacementIndexHash}",
            $"- chunkIndexHash: {report.ChunkIndexHash}",
            $"- biomeDistributionProofHash: {report.BiomeDistributionProofHash}",
            $"- waterNetworkProofHash: {report.WaterNetworkProofHash}",
            $"- roadReachabilityProofHash: {report.RoadReachabilityProofHash}",
            $"- layerTransitionProofHash: {report.LayerTransitionProofHash}",
            $"- objectPlacementProofHash: {report.ObjectPlacementProofHash}",
            $"- negativeProofHash: {report.NegativeProofHash}",
            $"- sourceLineageHash: {report.SourceLineageHash}",
            $"- qualityGateHash: {report.QualityGateHash}",
            $"- surfaceOverviewHash: {report.SurfaceOverviewHash}",
            $"- undergroundOverviewHash: {report.UndergroundOverviewHash}",
            $"- combinedOverviewHash: {report.CombinedOverviewHash}"
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyDictionary<string, string> RenderOverviewSvgs(VisualRegionDefinition definition)
    {
        var surface = definition.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.SurfaceLayerId);
        var underground = definition.Layers.Single(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.UndergroundLayerId);
        var surfaceSvg = RenderLayerOverview(surface, "Goal 088 surface 144x144");
        var undergroundSvg = RenderLayerOverview(underground, "Goal 088 underground 144x144");
        var combinedSvg = RenderCombinedOverview(surface, underground);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SurfaceOverviewSvgFileName] = surfaceSvg,
            [UndergroundOverviewSvgFileName] = undergroundSvg,
            [CombinedOverviewSvgFileName] = combinedSvg
        };
    }

    private static string RenderLayerOverview(VisualRegionLayer layer, string title)
    {
        const int blockWidth = 36;
        const int blockHeight = 24;
        var width = DeterministicVisualRegionComposerVocabulary.PatchGridColumns * blockWidth;
        var height = DeterministicVisualRegionComposerVocabulary.PatchGridRows * blockHeight;
        var lines = new List<string>
        {
            $"<svg viewBox=\"0 0 {width} {height}\" data-region-id=\"{DeterministicVisualRegionComposerVocabulary.RegionId}\" data-layer-id=\"{Escape(layer.LayerId)}\">",
            $"  <title>{Escape(title)}</title>",
            $"  <rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"#14181d\" />"
        };

        foreach (var placement in layer.PatchPlacements.OrderBy(item => item.GridY).ThenBy(item => item.GridX))
        {
            var x = placement.GridX * blockWidth;
            var y = placement.GridY * blockHeight;
            var fill = ColorFor(placement);
            lines.Add($"  <rect x=\"{x}\" y=\"{y}\" width=\"{blockWidth}\" height=\"{blockHeight}\" fill=\"{fill}\" stroke=\"#2b3138\" stroke-width=\"1\" data-placement-id=\"{Escape(placement.PlacementId)}\" data-source-goal087-patch-id=\"{Escape(placement.SourceGoal087PatchId)}\" />");
            lines.Add($"  <text x=\"{x + 3}\" y=\"{y + 10}\" fill=\"#f0f4f8\" font-size=\"6\" font-family=\"monospace\">{placement.GridX},{placement.GridY}</text>");
            lines.Add($"  <text x=\"{x + 3}\" y=\"{y + 19}\" fill=\"#101418\" font-size=\"5\" font-family=\"monospace\">{Escape(ShortPatchId(placement.SourceGoal087PatchId))}</text>");
        }

        lines.Add("</svg>");
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string RenderCombinedOverview(VisualRegionLayer surface, VisualRegionLayer underground)
    {
        const int blockWidth = 36;
        const int blockHeight = 24;
        const int gap = 16;
        var layerWidth = DeterministicVisualRegionComposerVocabulary.PatchGridColumns * blockWidth;
        var height = DeterministicVisualRegionComposerVocabulary.PatchGridRows * blockHeight;
        var width = layerWidth * 2 + gap;
        var lines = new List<string>
        {
            $"<svg viewBox=\"0 0 {width} {height}\" data-region-id=\"{DeterministicVisualRegionComposerVocabulary.RegionId}\" data-layer-id=\"combined\">",
            "  <title>Goal 088 combined surface and underground overview</title>",
            $"  <rect x=\"0\" y=\"0\" width=\"{width}\" height=\"{height}\" fill=\"#11161a\" />"
        };
        AppendLayerBlocks(lines, surface, 0, blockWidth, blockHeight);
        AppendLayerBlocks(lines, underground, layerWidth + gap, blockWidth, blockHeight);
        lines.Add("</svg>");
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AppendLayerBlocks(
        List<string> lines,
        VisualRegionLayer layer,
        int offsetX,
        int blockWidth,
        int blockHeight)
    {
        foreach (var placement in layer.PatchPlacements.OrderBy(item => item.GridY).ThenBy(item => item.GridX))
        {
            var x = offsetX + placement.GridX * blockWidth;
            var y = placement.GridY * blockHeight;
            lines.Add($"  <rect x=\"{x}\" y=\"{y}\" width=\"{blockWidth}\" height=\"{blockHeight}\" fill=\"{ColorFor(placement)}\" stroke=\"#2b3138\" stroke-width=\"1\" data-placement-id=\"{Escape(placement.PlacementId)}\" data-source-goal087-patch-id=\"{Escape(placement.SourceGoal087PatchId)}\" />");
        }
    }

    private static string ColorFor(VisualRegionPatchPlacement placement)
    {
        var water = placement.DeclaredWaterKinds.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(water))
        {
            return water switch
            {
                "sea" => "#1f5d79",
                "river" => "#3aa2ba",
                "lake" => "#4c93a8",
                "marsh" => "#4c6f4a",
                "underground_water" => "#355b70",
                "lava_boundary" => "#8b3f32",
                _ => "#53798b"
            };
        }

        return (placement.DeclaredBiomes.FirstOrDefault() ?? string.Empty) switch
        {
            "forest" => "#346d3d",
            "mountain" => "#747a7c",
            "snow" => "#dfeef2",
            "desert" => "#d7aa62",
            "lava_ash" => "#6b3530",
            "cave" => "#4f4a45",
            "rock" => "#6d6760",
            "lava" => "#9c4938",
            "mushroom" => "#72548a",
            "ruin" => "#6d6170",
            _ => "#5f9f47"
        };
    }

    private static string ShortPatchId(string patchId) =>
        patchId switch
        {
            "heroes_like_overworld_24x16" => "hero",
            "mixed_biome_settlement_creature_24x16" => "mix",
            "water_coast_river_lake_marsh_24x16" => "water",
            _ => "patch"
        };

    private static VisualRegionNegativeScenario Invalid(
        string id,
        string mutation,
        VisualRegionDefinition definition,
        IReadOnlyDictionary<string, string>? overviewSvgs = null)
    {
        var svgs = overviewSvgs
            ?? (definition.Layers.Any(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.SurfaceLayerId)
                && definition.Layers.Any(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.UndergroundLayerId)
                    ? RenderOverviewSvgs(definition)
                    : MinimalSafeOverviewSvgs());
        var validation = DeterministicVisualRegionComposerValidator.Validate(
            definition,
            DeterministicVisualRegionComposerFixtures.KnownGoal087PatchIds,
            svgs);
        return new VisualRegionNegativeScenario
        {
            ScenarioId = id,
            CausalMutation = mutation,
            ExpectedValid = false,
            ActualValid = validation.Passed,
            Diagnostics = validation.Diagnostics
        };
    }

    private static IReadOnlyDictionary<string, string> MinimalSafeOverviewSvgs() =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SurfaceOverviewSvgFileName] = MinimalSafeOverviewSvg("surface", 54),
            [UndergroundOverviewSvgFileName] = MinimalSafeOverviewSvg("underground", 54),
            [CombinedOverviewSvgFileName] = MinimalSafeOverviewSvg("combined", 108)
        };

    private static string MinimalSafeOverviewSvg(string layerId, int rectCount)
    {
        var lines = new List<string>
        {
            $"<svg viewBox=\"0 0 216 216\" data-layer-id=\"{layerId}\">",
            "  <rect x=\"0\" y=\"0\" width=\"216\" height=\"216\" fill=\"#14181d\" />"
        };
        for (var index = 0; index < rectCount; index++)
        {
            var x = (index % 12) * 18;
            var y = (index / 12) * 18;
            lines.Add($"  <rect x=\"{x}\" y=\"{y}\" width=\"18\" height=\"18\" fill=\"#5f9f47\" />");
        }

        lines.Add("</svg>");
        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static VisualRegionDefinition ReplaceLayer(VisualRegionDefinition definition, VisualRegionLayer replacement) =>
        definition with { Layers = definition.Layers.Select(item => item.LayerId == replacement.LayerId ? replacement : item).ToList() };

    private static VisualRegionDefinition ReplacePlacement(VisualRegionDefinition definition, VisualRegionPatchPlacement replacement) =>
        definition with
        {
            Layers = definition.Layers
                .Select(layer => layer.LayerId == replacement.LayerId
                    ? layer with { PatchPlacements = layer.PatchPlacements.Select(item => item.PlacementId == replacement.PlacementId ? replacement : item).ToList() }
                    : layer)
                .ToList()
        };

    private static VisualRegionDefinition ReplaceSettlement(VisualRegionDefinition definition, VisualRegionSettlementPlacement replacement) =>
        definition with { Settlements = definition.Settlements.Select(item => item.SettlementId == replacement.SettlementId ? replacement : item).ToList() };

    private static VisualRegionDefinition ReplaceGate(VisualRegionDefinition definition, VisualRegionGateTransition replacement) =>
        definition with { GateTransitions = definition.GateTransitions.Select(item => item.TransitionId == replacement.TransitionId ? replacement : item).ToList() };

    private static VisualRegionDefinition ReplaceCreature(VisualRegionDefinition definition, VisualRegionCreaturePlacement replacement) =>
        definition with { CreaturePlacements = definition.CreaturePlacements.Select(item => item.CreatureId == replacement.CreatureId ? replacement : item).ToList() };

    private static VisualRegionDefinition ReplaceOverlay(VisualRegionDefinition definition, VisualRegionOverlay replacement) =>
        definition with { Overlays = definition.Overlays.Select(item => item.OverlayId == replacement.OverlayId ? replacement : item).ToList() };

    private static HashSet<string> ReachableRoadNodes(VisualRegionRoadNetwork roadNetwork)
    {
        var requiredStart = roadNetwork.Nodes.FirstOrDefault(item => item.RequiredAnchor)?.NodeId
            ?? roadNetwork.Nodes.FirstOrDefault()?.NodeId
            ?? string.Empty;
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(requiredStart))
        {
            return reachable;
        }

        var adjacency = roadNetwork.Nodes.ToDictionary(item => item.NodeId, _ => new List<string>(), StringComparer.Ordinal);
        foreach (var edge in roadNetwork.Edges)
        {
            if (!adjacency.ContainsKey(edge.FromNodeId) || !adjacency.ContainsKey(edge.ToNodeId))
            {
                continue;
            }

            adjacency[edge.FromNodeId].Add(edge.ToNodeId);
            adjacency[edge.ToNodeId].Add(edge.FromNodeId);
        }

        var queue = new Queue<string>();
        queue.Enqueue(requiredStart);
        reachable.Add(requiredStart);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in adjacency[current].OrderBy(item => item, StringComparer.Ordinal))
            {
                if (reachable.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return reachable;
    }

    private static VisualRegionSourceLineageRecord BuildSourceRecord(
        string? projectRootPath,
        string relativePath,
        IReadOnlyList<string> tags)
    {
        var text = ReadText(projectRootPath, relativePath);
        return new VisualRegionSourceLineageRecord
        {
            RelativePath = relativePath,
            Exists = !string.IsNullOrWhiteSpace(text),
            Sha256 = string.IsNullOrWhiteSpace(text) ? string.Empty : DeterministicVisualRegionComposerHash.Compute(text),
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
            && !string.Equals(
                pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                rootFull.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
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
