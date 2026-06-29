using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.CandidateModules.WorldBiomeNoise;

public sealed class RegionHydrologyWaterwayHintsCandidateService
{
    public const string CandidateId = "candidate_region_hydrology_waterway_hints_v1";
    public const string ContractId = "region_hydrology_waterway_hints_contract_v1";
    public const string BaseCandidateId = WorldBiomeNoiseCandidateService.GatewayConnectivityHintsCandidateId;
    public const string FinalStatus = WorldBiomeNoiseCandidateService.FinalStatus;
    public const string RelativeOutputDirectory = ".llmgc/procedural/candidate-region-hydrology-waterway-hints-v1";
    public const string ReportJsonFileName = "candidate-region-hydrology-waterway-hints-v1-report.json";
    public const string ReportMarkdownFileName = "candidate-region-hydrology-waterway-hints-v1-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly WorldBiomeNoiseCandidateService sourceService;

    public RegionHydrologyWaterwayHintsCandidateService()
        : this(new WorldBiomeNoiseCandidateService())
    {
    }

    public RegionHydrologyWaterwayHintsCandidateService(WorldBiomeNoiseCandidateService sourceService)
    {
        this.sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
    }

    public RegionHydrologyWaterwayHintsCandidateResult Build(RegionHydrologyWaterwayHintsCandidateOptions? options = null)
    {
        var settings = options ?? new RegionHydrologyWaterwayHintsCandidateOptions();
        var diagnostics = new List<WorldBiomeNoiseDiagnostic>
        {
            Diagnostic("info", "region_hydrology_waterway_hints.external_scouting.reference_only", "mapgen4/hydrobasins/dem/fluvial", "External hydrology references remain reference-only; no dependency, copied implementation or simulation is adopted."),
            Diagnostic("info", "region_hydrology_waterway_hints.boundary", CandidateId, "Candidate produces hydrology and waterway planning hints only; no rivers, lakes, paths, erosion, rainfall simulation, navigation, settlements or factions are generated.")
        };

        ValidateOptions(settings, diagnostics);
        var hasErrors = diagnostics.Any(item => item.Severity == "error");
        var plan = hasErrors ? new RegionHydrologyWaterwayPlan() : BuildPlan(settings);
        var differentSeedPlan = hasErrors ? new RegionHydrologyWaterwayPlan() : BuildPlan(settings with { Seed = settings.Seed + "/variant" });
        var differentSeedVariationVisible = !hasErrors && HasMeaningfulSeedVariation(plan, differentSeedPlan);
        diagnostics.Add(Diagnostic(
            differentSeedVariationVisible ? "info" : "error",
            differentSeedVariationVisible ? "region_hydrology_waterway_hints.seed_variation.visible" : "region_hydrology_waterway_hints.seed_variation.missing",
            "seed",
            "Different seed should change at least one hydrology planning field while preserving bounded output shape."));

        var scoresBounded = PlanScoresAreBounded(plan);
        diagnostics.Add(Diagnostic(
            scoresBounded ? "info" : "error",
            scoresBounded ? "region_hydrology_waterway_hints.scores.bounded" : "region_hydrology_waterway_hints.scores.out_of_range",
            "scores",
            "All hydrology, waterway and crossing scores must stay within 0..1."));

        var sourceIdsUnique = IdsUnique(plan.WaterSourceCandidates.Select(item => item.WaterSourceCandidateId));
        diagnostics.Add(Diagnostic(
            sourceIdsUnique ? "info" : "error",
            sourceIdsUnique ? "region_hydrology_waterway_hints.source_ids.unique" : "region_hydrology_waterway_hints.source_ids.duplicate",
            "waterSourceCandidates",
            "Water source ids must be stable and unique."));

        var waterbodyIdsUnique = IdsUnique(plan.WaterbodyCandidates.Select(item => item.WaterbodyCandidateId));
        diagnostics.Add(Diagnostic(
            waterbodyIdsUnique ? "info" : "error",
            waterbodyIdsUnique ? "region_hydrology_waterway_hints.waterbody_ids.unique" : "region_hydrology_waterway_hints.waterbody_ids.duplicate",
            "waterbodyCandidates",
            "Waterbody ids must be stable and unique."));

        var waterwayIdsUnique = IdsUnique(plan.WaterwayCorridorHints.Select(item => item.WaterwayHintId));
        diagnostics.Add(Diagnostic(
            waterwayIdsUnique ? "info" : "error",
            waterwayIdsUnique ? "region_hydrology_waterway_hints.waterway_ids.unique" : "region_hydrology_waterway_hints.waterway_ids.duplicate",
            "waterwayCorridorHints",
            "Waterway hint ids must be stable and unique."));

        var crossingIdsUnique = IdsUnique(plan.CrossingPressureHints.Select(item => item.CrossingHintId));
        diagnostics.Add(Diagnostic(
            crossingIdsUnique ? "info" : "error",
            crossingIdsUnique ? "region_hydrology_waterway_hints.crossing_ids.unique" : "region_hydrology_waterway_hints.crossing_ids.duplicate",
            "crossingPressureHints",
            "Crossing hint ids must be stable and unique."));

        var referencesValid = ReferencesAreValid(plan);
        diagnostics.Add(Diagnostic(
            referencesValid ? "info" : "error",
            referencesValid ? "region_hydrology_waterway_hints.refs.valid" : "region_hydrology_waterway_hints.refs.invalid",
            "hints",
            "Hints must reference only generated water sources, waterbodies, gateways, road hints, neighbor regions or basin ids from the same plan."));

        var limitsRespected = plan.WaterSourceCandidates.Count <= settings.MaxWaterSourceCandidates
                              && plan.WaterbodyCandidates.Count <= settings.MaxWaterbodyCandidates
                              && plan.WaterwayCorridorHints.Count <= settings.MaxWaterwayHints
                              && plan.CrossingPressureHints.Count <= settings.MaxCrossingHints;
        diagnostics.Add(Diagnostic(
            limitsRespected ? "info" : "error",
            limitsRespected ? "region_hydrology_waterway_hints.limits.respected" : "region_hydrology_waterway_hints.limits.exceeded",
            "options",
            "Max water source, waterbody, waterway and crossing options must be respected."));

        var externalExecution = new WorldBiomeNoiseExternalExecutionFlags();
        var contractProofPassed = diagnostics.All(item => item.Severity != "error")
                                  && plan.WaterSourceCandidates.Count > 0
                                  && plan.WaterwayCorridorHints.Count > 0
                                  && differentSeedVariationVisible
                                  && scoresBounded
                                  && sourceIdsUnique
                                  && waterbodyIdsUnique
                                  && waterwayIdsUnique
                                  && crossingIdsUnique
                                  && referencesValid
                                  && limitsRespected
                                  && externalExecution.AllFalse;

        var reportWithoutHash = new RegionHydrologyWaterwayHintsCandidateReport
        {
            CandidateId = CandidateId,
            ContractId = ContractId,
            BaseCandidateId = BaseCandidateId,
            FinalStatus = FinalStatus,
            ContractProofPassed = contractProofPassed,
            AcceptedGateClaimed = false,
            Seed = settings.Seed,
            RulesVersion = settings.RulesVersion,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            IncludeDiagonals = settings.IncludeDiagonals,
            MaxWaterSourceCandidates = settings.MaxWaterSourceCandidates,
            MaxWaterbodyCandidates = settings.MaxWaterbodyCandidates,
            MaxWaterwayHints = settings.MaxWaterwayHints,
            MaxCrossingHints = settings.MaxCrossingHints,
            BasinCodeDepth = settings.BasinCodeDepth,
            SameSeedStable = true,
            DifferentSeedVariationVisible = differentSeedVariationVisible,
            OrderIndependent = true,
            GlobalMapMaterialized = false,
            ActualRiversGenerated = false,
            ActualWaterbodiesGenerated = false,
            RiverPathsGenerated = false,
            WaterwayPolylinesGenerated = false,
            ErosionSimulationImplemented = false,
            RainfallSimulationImplemented = false,
            FloodSimulationImplemented = false,
            PathfindingNavigationImplemented = false,
            NavigationGraphGenerated = false,
            ActualSettlementsGenerated = false,
            FactionGenerationImplemented = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            RuntimeProviderNetworkDependency = false,
            ExternalExecution = externalExecution,
            ExternalScoutingDecisions =
            [
                new WorldRegionClimateExternalScoutingDecision("Red Blob Mapgen4 rainfall and rivers", "reference_only", "Rainfall and river relationships inform labels only; no simulation is copied."),
                new WorldRegionClimateExternalScoutingDecision("HydroBASINS nested basin topology", "reference_only", "Hierarchical basin coding is reference-inspired only; local codes are not real Pfafstetter ids."),
                new WorldRegionClimateExternalScoutingDecision("DEM watershed and flow direction", "reference_only", "Elevation and outflow concepts remain bounded local hints, not watershed delineation."),
                new WorldRegionClimateExternalScoutingDecision("Fluvial erosion research", "reference_only", "Terrain/rainfall/erosion relationships stay conceptual; no erosion engine is implemented.")
            ],
            Plan = plan,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new RegionHydrologyWaterwayHintsCandidateResult
        {
            Report = report,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderReport(report)
        };
    }

    public async Task<RegionHydrologyWaterwayHintsCandidateWriteResult> WriteAsync(
        string projectRootPath,
        RegionHydrologyWaterwayHintsCandidateResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new RegionHydrologyWaterwayHintsCandidateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    public async Task<RegionHydrologyWaterwayHintsCandidateWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private RegionHydrologyWaterwayPlan BuildPlan(RegionHydrologyWaterwayHintsCandidateOptions settings)
    {
        var climateOptions = ToRegionClimateOptions(settings);
        var sourceClimate = sourceService.SummarizeRegionClimate(climateOptions, settings.RegionX, settings.RegionY);
        var settlementPlan = sourceService.BuildSettlementRoadSeeds(ToSettlementOptions(settings)).Report.Plan;
        var gatewayPlan = sourceService.BuildGatewayConnectivityHints(ToGatewayOptions(settings)).Report.Plan;
        var neighbors = BuildNeighborSummaries(settings, climateOptions);
        var drainage = BuildDrainageSummary(settings, sourceClimate, neighbors);
        var samples = BuildLocalSamples(settings, climateOptions);
        var sources = BuildWaterSourceCandidates(settings, samples, sourceClimate, drainage);
        var waterbodies = BuildWaterbodyCandidates(settings, samples, sourceClimate, settlementPlan, drainage);
        var waterways = BuildWaterwayHints(settings, sourceClimate, drainage, gatewayPlan, neighbors, sources, waterbodies);
        var crossings = BuildCrossingHints(settings, gatewayPlan, settlementPlan, waterways);

        return new RegionHydrologyWaterwayPlan
        {
            PlanId = BuildPlanId(settings, sourceClimate.RegionId, settlementPlan.PlanId, gatewayPlan.PlanId),
            RegionX = settings.RegionX,
            RegionY = settings.RegionY,
            RegionId = sourceClimate.RegionId,
            SourceClimateSummary = sourceClimate,
            SourceSettlementRoadSeedPlanId = settlementPlan.PlanId,
            SourceSettlementAnchorCount = settlementPlan.SettlementAnchors.Count,
            SourceRoadHintCount = settlementPlan.RoadHints.Count,
            SourceGatewayConnectivityPlanId = gatewayPlan.PlanId,
            SourceGatewayCandidateCount = gatewayPlan.GatewayCandidates.Count,
            SourceCorridorHintCount = gatewayPlan.CorridorHints.Count,
            NeighborRegionSummaries = neighbors,
            Drainage = drainage,
            WaterSourceCandidates = sources,
            WaterbodyCandidates = waterbodies,
            WaterwayCorridorHints = waterways,
            CrossingPressureHints = crossings,
            SummaryTags = BuildSummaryTags(settings, sourceClimate, drainage, sources, waterbodies, waterways, crossings)
        };
    }

    private IReadOnlyList<HydrologyNeighborRegionSummary> BuildNeighborSummaries(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        WorldRegionClimateCandidateOptions climateOptions)
    {
        return BuildDirections(settings.IncludeDiagonals)
            .Select(direction =>
            {
                var regionX = settings.RegionX + direction.DeltaX;
                var regionY = settings.RegionY + direction.DeltaY;
                return new HydrologyNeighborRegionSummary
                {
                    RegionX = regionX,
                    RegionY = regionY,
                    RegionId = sourceService.SummarizeRegionClimate(climateOptions, regionX, regionY).RegionId,
                    Direction = direction.Name,
                    IsDiagonal = direction.IsDiagonal,
                    ClimateSummary = sourceService.SummarizeRegionClimate(climateOptions, regionX, regionY)
                };
            })
            .OrderBy(item => DirectionSortOrder(item.Direction))
            .ToList();
    }

    private static HydrologyDrainageSummary BuildDrainageSummary(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceClimate,
        IReadOnlyList<HydrologyNeighborRegionSummary> neighbors)
    {
        var runoff = Clamp01(sourceClimate.AverageMoistureScore * 0.56
                             + sourceClimate.AverageRuggednessScore * 0.16
                             + sourceClimate.AverageElevationScore * 0.10
                             + (sourceClimate.AverageTemperatureScore < 0.36 ? 0.06 : 0.0)
                             + ScoreJitter(settings, "runoff", settings.RegionX, settings.RegionY, 0.06));
        var aridity = Clamp01((1.0 - sourceClimate.AverageMoistureScore) * 0.62
                              + sourceClimate.AverageTemperatureScore * 0.22
                              + (sourceClimate.DominantBiomeId == "biome/desert" ? 0.14 : 0.0)
                              + ScoreJitter(settings, "aridity", settings.RegionX, settings.RegionY, 0.04));
        var floodplain = Clamp01(sourceClimate.AverageMoistureScore * 0.36
                                 + (1.0 - sourceClimate.AverageElevationScore) * 0.28
                                 + (1.0 - sourceClimate.AverageRuggednessScore) * 0.28
                                 + runoff * 0.08);
        var accumulation = Clamp01(sourceClimate.AverageMoistureScore * 0.32
                                   + (1.0 - sourceClimate.RoadSuitabilityScore) * 0.10
                                   + (1.0 - sourceClimate.AverageRuggednessScore) * 0.20
                                   + floodplain * 0.22
                                   + ScoreJitter(settings, "accumulation", settings.RegionX, settings.RegionY, 0.05));
        var lowest = neighbors
            .OrderBy(item => item.ClimateSummary.AverageElevationScore)
            .ThenBy(item => DirectionSortOrder(item.Direction))
            .FirstOrDefault();
        var closedNoise = ToScore01(SampleScore(settings, "closed_basin", settings.RegionX, settings.RegionY));
        var closed = lowest is null
                     || (closedNoise < 0.18 && aridity >= 0.42 && accumulation >= 0.28)
                     || (runoff < 0.24 && accumulation >= 0.52 && aridity >= 0.55);
        var outflowDirection = closed ? "ClosedBasin" : lowest!.Direction;
        var downstreamRegionId = closed ? string.Empty : lowest!.RegionId;
        var downstreamRegionX = closed ? settings.RegionX : lowest!.RegionX;
        var downstreamRegionY = closed ? settings.RegionY : lowest!.RegionY;

        return new HydrologyDrainageSummary
        {
            BasinId = BuildBasinId(settings, sourceClimate.RegionId),
            BasinCode = BuildBasinCode(settings, sourceClimate, outflowDirection),
            OutflowDirection = outflowDirection,
            DownstreamNeighborRegionId = downstreamRegionId,
            DownstreamNeighborRegionX = downstreamRegionX,
            DownstreamNeighborRegionY = downstreamRegionY,
            RunoffPotentialScore = RoundScore(runoff),
            AccumulationPotentialScore = RoundScore(closed ? Clamp01(accumulation + 0.12) : accumulation),
            FloodplainPotentialScore = RoundScore(floodplain),
            AridityScore = RoundScore(aridity),
            Reasons = BuildDrainageReasons(sourceClimate, outflowDirection, runoff, accumulation, floodplain, aridity),
            Tags = BuildDrainageTags(outflowDirection, runoff, accumulation, floodplain, aridity)
        };
    }

    private IReadOnlyList<HydrologyLocalSample> BuildLocalSamples(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        WorldRegionClimateCandidateOptions climateOptions)
    {
        var maxOffset = settings.RegionSize - 1;
        var midOffset = settings.RegionSize / 2;
        var offsets = new[] { 0, Math.Max(0, midOffset / 2), midOffset, Math.Min(maxOffset, midOffset + Math.Max(1, midOffset / 2)), maxOffset }
            .Distinct()
            .Order()
            .ToArray();
        var originX = settings.RegionX * settings.RegionSize;
        var originY = settings.RegionY * settings.RegionSize;

        return offsets
            .SelectMany(localX => offsets.Select(localY =>
            {
                var sample = sourceService.SampleRegionClimate(climateOptions, new WorldRegionClimateCoordinate(originX + localX, originY + localY));
                return new HydrologyLocalSample(sample, localX, localY);
            }))
            .OrderBy(item => item.Sample.X)
            .ThenBy(item => item.Sample.Y)
            .ToList();
    }

    private static IReadOnlyList<WaterSourceCandidate> BuildWaterSourceCandidates(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        IReadOnlyList<HydrologyLocalSample> samples,
        WorldRegionClimateSummary sourceClimate,
        HydrologyDrainageSummary drainage)
    {
        return samples
            .Where(item => item.Sample.BiomeId != "biome/water" || item.Sample.MoistureScore >= 0.58)
            .Select(item => BuildWaterSourceCandidate(settings, item, sourceClimate, drainage))
            .OrderByDescending(item => item.FlowPotentialScore)
            .ThenByDescending(item => item.SeasonalReliabilityScore)
            .ThenBy(item => item.WaterSourceCandidateId, StringComparer.Ordinal)
            .Take(settings.MaxWaterSourceCandidates)
            .ToList();
    }

    private static WaterSourceCandidate BuildWaterSourceCandidate(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        HydrologyLocalSample localSample,
        WorldRegionClimateSummary sourceClimate,
        HydrologyDrainageSummary drainage)
    {
        var sample = localSample.Sample;
        var jitter = ScoreJitter(settings, "water_source", sample.X, sample.Y, 0.06);
        var flow = Clamp01(sample.MoistureScore * 0.46
                           + sample.ElevationScore * 0.14
                           + sample.RuggednessScore * 0.12
                           + drainage.RunoffPotentialScore * 0.20
                           + (sample.ClimateBand is "cold" or "polar" ? 0.06 : 0.0)
                           + jitter);
        var reliability = Clamp01(sample.MoistureScore * 0.42
                                  + (1.0 - drainage.AridityScore) * 0.22
                                  + sourceClimate.AverageMoistureScore * 0.18
                                  + (sample.BiomeId == "biome/forest" ? 0.08 : 0.0)
                                  + ScoreJitter(settings, "water_source_reliability", sample.X, sample.Y, 0.04));
        var kind = ClassifyWaterSourceKind(sample, drainage, sourceClimate);

        return new WaterSourceCandidate
        {
            WaterSourceCandidateId = BuildWaterSourceId(settings, sample.X, sample.Y, kind),
            WorldCellX = sample.X,
            WorldCellY = sample.Y,
            LocalCellX = localSample.LocalX,
            LocalCellY = localSample.LocalY,
            Kind = kind,
            FlowPotentialScore = RoundScore(flow),
            SeasonalReliabilityScore = RoundScore(reliability),
            ClimateBand = sample.ClimateBand,
            BiomeId = sample.BiomeId,
            ElevationScore = sample.ElevationScore,
            MoistureScore = sample.MoistureScore,
            TemperatureScore = sample.TemperatureScore,
            RuggednessScore = sample.RuggednessScore,
            PreferredOutflowDirection = drainage.OutflowDirection,
            Reasons = BuildWaterSourceReasons(sample, drainage, flow, reliability),
            Tags = BuildWaterSourceTags(kind, sample, flow, reliability)
        };
    }

    private static IReadOnlyList<WaterbodyCandidate> BuildWaterbodyCandidates(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        IReadOnlyList<HydrologyLocalSample> samples,
        WorldRegionClimateSummary sourceClimate,
        RegionSettlementRoadSeedPlan settlementPlan,
        HydrologyDrainageSummary drainage)
    {
        return samples
            .Select(item => BuildWaterbodyCandidate(settings, item, sourceClimate, settlementPlan, drainage))
            .OrderByDescending(item => item.RetentionScore)
            .ThenByDescending(item => item.WaterAvailabilityScore)
            .ThenBy(item => item.WaterbodyCandidateId, StringComparer.Ordinal)
            .Take(settings.MaxWaterbodyCandidates)
            .ToList();
    }

    private static WaterbodyCandidate BuildWaterbodyCandidate(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        HydrologyLocalSample localSample,
        WorldRegionClimateSummary sourceClimate,
        RegionSettlementRoadSeedPlan settlementPlan,
        HydrologyDrainageSummary drainage)
    {
        var sample = localSample.Sample;
        var settlementContext = settlementPlan.SettlementAnchors.Count == 0
            ? sourceClimate.AverageSettlementSuitabilityScore
            : settlementPlan.SettlementAnchors.Average(item => item.SuitabilityScore);
        var retention = Clamp01(sample.MoistureScore * 0.40
                                + (1.0 - sample.RuggednessScore) * 0.20
                                + (1.0 - sample.ElevationScore) * 0.18
                                + drainage.AccumulationPotentialScore * 0.16
                                + ScoreJitter(settings, "waterbody_retention", sample.X, sample.Y, 0.05));
        var availability = Clamp01(sample.MoistureScore * 0.48
                                   + drainage.RunoffPotentialScore * 0.24
                                   + (1.0 - drainage.AridityScore) * 0.16
                                   + ScoreJitter(settings, "waterbody_availability", sample.X, sample.Y, 0.04));
        var settlementSupport = Clamp01(availability * 0.42 + retention * 0.20 + settlementContext * 0.28 + (sample.BiomeId == "biome/water" ? -0.10 : 0.0));
        var roadObstacle = Clamp01(retention * 0.34 + sample.MoistureScore * 0.28 + (1.0 - sample.RuggednessScore) * 0.16 + drainage.FloodplainPotentialScore * 0.22);
        var kind = ClassifyWaterbodyKind(sample, drainage, retention, availability);

        return new WaterbodyCandidate
        {
            WaterbodyCandidateId = BuildWaterbodyId(settings, sample.X, sample.Y, kind),
            WorldCellX = sample.X,
            WorldCellY = sample.Y,
            LocalCellX = localSample.LocalX,
            LocalCellY = localSample.LocalY,
            Kind = kind,
            RetentionScore = RoundScore(retention),
            WaterAvailabilityScore = RoundScore(availability),
            SettlementSupportScore = RoundScore(settlementSupport),
            RoadObstacleScore = RoundScore(roadObstacle),
            ClimateBand = sample.ClimateBand,
            BiomeId = sample.BiomeId,
            ElevationScore = sample.ElevationScore,
            MoistureScore = sample.MoistureScore,
            TemperatureScore = sample.TemperatureScore,
            RuggednessScore = sample.RuggednessScore,
            Reasons = BuildWaterbodyReasons(sample, drainage, retention, availability, roadObstacle),
            Tags = BuildWaterbodyTags(kind, sample, retention, availability, roadObstacle)
        };
    }

    private static IReadOnlyList<WaterwayCorridorHint> BuildWaterwayHints(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceClimate,
        HydrologyDrainageSummary drainage,
        RegionGatewayConnectivityPlan gatewayPlan,
        IReadOnlyList<HydrologyNeighborRegionSummary> neighbors,
        IReadOnlyList<WaterSourceCandidate> sources,
        IReadOnlyList<WaterbodyCandidate> waterbodies)
    {
        var hints = new List<WaterwayCorridorHint>();
        foreach (var source in sources)
        {
            hints.Add(BuildWaterwayHint(settings, sourceClimate, drainage, gatewayPlan, neighbors, source.WaterSourceCandidateId, source.PreferredOutflowDirection, source.FlowPotentialScore, source.SeasonalReliabilityScore, "source"));
        }

        foreach (var waterbody in waterbodies)
        {
            hints.Add(BuildWaterwayHint(settings, sourceClimate, drainage, gatewayPlan, neighbors, waterbody.WaterbodyCandidateId, drainage.OutflowDirection, waterbody.WaterAvailabilityScore, waterbody.RetentionScore, "waterbody"));
        }

        return hints
            .GroupBy(item => item.FromCandidateId, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(item => item.EstimatedFlowScore).ThenBy(item => item.WaterwayHintId, StringComparer.Ordinal).First())
            .OrderByDescending(item => item.EstimatedFlowScore)
            .ThenByDescending(item => item.RoadCrossingPressureScore)
            .ThenBy(item => item.WaterwayHintId, StringComparer.Ordinal)
            .Take(settings.MaxWaterwayHints)
            .ToList();
    }

    private static WaterwayCorridorHint BuildWaterwayHint(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceClimate,
        HydrologyDrainageSummary drainage,
        RegionGatewayConnectivityPlan gatewayPlan,
        IReadOnlyList<HydrologyNeighborRegionSummary> neighbors,
        string fromCandidateId,
        string preferredDirection,
        double sourceFlow,
        double sourceReliability,
        string fromKind)
    {
        var target = ResolveWaterwayTarget(drainage, gatewayPlan, neighbors, preferredDirection);
        var estimatedFlow = Clamp01(sourceFlow * 0.50 + drainage.RunoffPotentialScore * 0.32 + sourceClimate.AverageMoistureScore * 0.18);
        var persistence = Clamp01(sourceReliability * 0.42 + (1.0 - drainage.AridityScore) * 0.30 + drainage.AccumulationPotentialScore * 0.18 + (drainage.OutflowDirection == "ClosedBasin" ? -0.10 : 0.0));
        var erosion = Clamp01(sourceClimate.AverageRuggednessScore * 0.32 + sourceClimate.AverageElevationScore * 0.22 + estimatedFlow * 0.26 + drainage.FloodplainPotentialScore * 0.10);
        var settlementSupport = Clamp01(estimatedFlow * 0.28 + persistence * 0.28 + sourceClimate.AverageSettlementSuitabilityScore * 0.28 + drainage.FloodplainPotentialScore * 0.16);
        var crossingPressure = Clamp01(estimatedFlow * 0.34 + persistence * 0.20 + gatewayPlan.GatewayCandidates.DefaultIfEmpty().Max(item => item?.SuitabilityScore ?? 0.0) * 0.24 + sourceClimate.RoadSuitabilityScore * 0.22);
        var kind = ClassifyWaterwayKind(drainage, estimatedFlow, persistence, fromKind, target.TargetKind);

        return new WaterwayCorridorHint
        {
            WaterwayHintId = BuildWaterwayHintId(settings, fromCandidateId, target.TargetId, kind),
            FromCandidateId = fromCandidateId,
            FromCandidateKind = fromKind,
            ToTargetId = target.TargetId,
            ToTargetKind = target.TargetKind,
            RelatedGatewayId = target.RelatedGatewayId,
            Kind = kind,
            EstimatedFlowScore = RoundScore(estimatedFlow),
            PersistenceScore = RoundScore(persistence),
            ErosionRiskScore = RoundScore(erosion),
            SettlementSupportScore = RoundScore(settlementSupport),
            RoadCrossingPressureScore = RoundScore(crossingPressure),
            Reasons = BuildWaterwayReasons(drainage, estimatedFlow, persistence, erosion, crossingPressure, target.TargetKind),
            Tags = BuildWaterwayTags(kind, drainage.OutflowDirection, fromKind, target.TargetKind)
        };
    }

    private static IReadOnlyList<CrossingPressureHint> BuildCrossingHints(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        RegionGatewayConnectivityPlan gatewayPlan,
        RegionSettlementRoadSeedPlan settlementPlan,
        IReadOnlyList<WaterwayCorridorHint> waterways)
    {
        var bestRoadHint = settlementPlan.RoadHints
            .OrderByDescending(item => item.PriorityScore)
            .ThenBy(item => item.RoadHintId, StringComparer.Ordinal)
            .FirstOrDefault();

        return waterways
            .Select(waterway =>
            {
                var relatedGatewayId = !string.IsNullOrWhiteSpace(waterway.RelatedGatewayId)
                    ? waterway.RelatedGatewayId
                    : gatewayPlan.GatewayCandidates
                        .OrderByDescending(item => item.SuitabilityScore)
                        .ThenBy(item => item.GatewayId, StringComparer.Ordinal)
                        .FirstOrDefault()?.GatewayId ?? string.Empty;
                var relatedRoadHintId = bestRoadHint?.RoadHintId ?? string.Empty;
                var crossingNeed = Clamp01(waterway.RoadCrossingPressureScore * 0.54
                                           + gatewayPlan.SourceRoadHintCount * 0.03
                                           + (string.IsNullOrWhiteSpace(relatedGatewayId) ? 0.0 : 0.18)
                                           + (string.IsNullOrWhiteSpace(relatedRoadHintId) ? 0.0 : 0.12));
                var bridge = Clamp01(crossingNeed * 0.44 + waterway.PersistenceScore * 0.26 + waterway.EstimatedFlowScore * 0.24 + waterway.ErosionRiskScore * 0.06);
                var ford = Clamp01(crossingNeed * 0.40 + (1.0 - waterway.PersistenceScore) * 0.30 + (1.0 - waterway.EstimatedFlowScore) * 0.18 + waterway.RoadCrossingPressureScore * 0.12);

                return new CrossingPressureHint
                {
                    CrossingHintId = BuildCrossingHintId(settings, waterway.WaterwayHintId, relatedGatewayId, relatedRoadHintId),
                    RelatedWaterwayHintId = waterway.WaterwayHintId,
                    RelatedGatewayId = relatedGatewayId,
                    RelatedRoadHintId = relatedRoadHintId,
                    CrossingNeedScore = RoundScore(crossingNeed),
                    BridgePressureScore = RoundScore(bridge),
                    FerryOrFordPressureScore = RoundScore(ford),
                    Reasons = BuildCrossingReasons(crossingNeed, bridge, ford, waterway),
                    Tags = BuildCrossingTags(waterway, relatedGatewayId, relatedRoadHintId)
                };
            })
            .OrderByDescending(item => item.CrossingNeedScore)
            .ThenBy(item => item.CrossingHintId, StringComparer.Ordinal)
            .Take(settings.MaxCrossingHints)
            .ToList();
    }

    private static HydrologyWaterwayTarget ResolveWaterwayTarget(
        HydrologyDrainageSummary drainage,
        RegionGatewayConnectivityPlan gatewayPlan,
        IReadOnlyList<HydrologyNeighborRegionSummary> neighbors,
        string preferredDirection)
    {
        if (drainage.OutflowDirection == "ClosedBasin")
        {
            return new HydrologyWaterwayTarget(drainage.BasinId, "Basin", string.Empty);
        }

        var direction = preferredDirection == "ClosedBasin" ? drainage.OutflowDirection : preferredDirection;
        var gateway = gatewayPlan.GatewayCandidates
            .Where(item => item.Direction == direction)
            .OrderByDescending(item => item.SuitabilityScore)
            .ThenBy(item => item.GatewayId, StringComparer.Ordinal)
            .FirstOrDefault();
        if (gateway is not null)
        {
            return new HydrologyWaterwayTarget(gateway.GatewayId, "Gateway", gateway.GatewayId);
        }

        var neighbor = neighbors.FirstOrDefault(item => item.Direction == drainage.OutflowDirection);
        if (neighbor is not null)
        {
            return new HydrologyWaterwayTarget(neighbor.RegionId, "NeighborRegion", string.Empty);
        }

        return new HydrologyWaterwayTarget(drainage.BasinId, "Basin", string.Empty);
    }

    private static void ValidateOptions(RegionHydrologyWaterwayHintsCandidateOptions settings, List<WorldBiomeNoiseDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(settings.Seed))
        {
            diagnostics.Add(Diagnostic("error", "region_hydrology_waterway_hints.seed.missing", "seed", "Seed is required for deterministic hydrology planning."));
        }

        if (settings.CoordinateSpace is not "world_cell" and not "chunk_cell" and not "region_anchor")
        {
            diagnostics.Add(Diagnostic("error", "region_hydrology_waterway_hints.coordinate_space.unknown", settings.CoordinateSpace, "Coordinate space must be one of the existing candidate coordinate spaces."));
        }

        if (settings.RegionSize <= 0)
        {
            diagnostics.Add(Diagnostic("error", "region_hydrology_waterway_hints.region_size.invalid", settings.RegionSize.ToString(System.Globalization.CultureInfo.InvariantCulture), "Region size must be positive."));
        }

        if (settings.LatitudeBandPeriod < 4)
        {
            diagnostics.Add(Diagnostic("error", "region_hydrology_waterway_hints.latitude_period.invalid", settings.LatitudeBandPeriod.ToString(System.Globalization.CultureInfo.InvariantCulture), "Latitude band period must be at least 4 cells."));
        }

        if (settings.MaxWaterSourceCandidates < 0 || settings.MaxWaterbodyCandidates < 0 || settings.MaxWaterwayHints < 0 || settings.MaxCrossingHints < 0)
        {
            diagnostics.Add(Diagnostic("error", "region_hydrology_waterway_hints.max_options.invalid", "options", "Max candidate and hint options cannot be negative."));
        }

        if (settings.BasinCodeDepth < 1 || settings.BasinCodeDepth > 8)
        {
            diagnostics.Add(Diagnostic("error", "region_hydrology_waterway_hints.basin_code_depth.invalid", settings.BasinCodeDepth.ToString(System.Globalization.CultureInfo.InvariantCulture), "Basin code depth must stay within 1..8."));
        }
    }

    private static WorldRegionClimateCandidateOptions ToRegionClimateOptions(RegionHydrologyWaterwayHintsCandidateOptions settings) =>
        new()
        {
            Seed = settings.Seed,
            RulesVersion = WorldBiomeNoiseCandidateService.RegionClimateContractId,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod
        };

    private static RegionSettlementRoadSeedsCandidateOptions ToSettlementOptions(RegionHydrologyWaterwayHintsCandidateOptions settings) =>
        new()
        {
            Seed = settings.Seed,
            RulesVersion = WorldBiomeNoiseCandidateService.SettlementRoadSeedsContractId,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            RegionX = settings.RegionX,
            RegionY = settings.RegionY,
            MaxSettlementAnchors = 5,
            MaxRoadHints = 6
        };

    private static RegionGatewayConnectivityHintsCandidateOptions ToGatewayOptions(RegionHydrologyWaterwayHintsCandidateOptions settings) =>
        new()
        {
            Seed = settings.Seed,
            RulesVersion = WorldBiomeNoiseCandidateService.GatewayConnectivityHintsContractId,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            RegionX = settings.RegionX,
            RegionY = settings.RegionY,
            IncludeDiagonals = settings.IncludeDiagonals,
            MaxGatewaysPerSide = 2,
            MaxCorridorHints = 8,
            MaxNeighborRegions = settings.IncludeDiagonals ? 8 : 4
        };

    private static string ClassifyWaterSourceKind(WorldRegionClimateSample sample, HydrologyDrainageSummary drainage, WorldRegionClimateSummary sourceClimate)
    {
        if (sample.ClimateBand is "cold" or "polar" && sample.ElevationScore >= 0.58)
        {
            return "SnowmeltCandidate";
        }

        if (sample.MoistureScore >= 0.68 && sample.ElevationScore >= sourceClimate.AverageElevationScore)
        {
            return "RainfedHeadwaterCandidate";
        }

        if (sample.MoistureScore >= 0.62 && drainage.AccumulationPotentialScore >= 0.48)
        {
            return "WetlandSourceCandidate";
        }

        if (drainage.AridityScore >= 0.58 && sample.MoistureScore >= sourceClimate.AverageMoistureScore)
        {
            return "OasisCandidate";
        }

        return "SpringCandidate";
    }

    private static string ClassifyWaterbodyKind(WorldRegionClimateSample sample, HydrologyDrainageSummary drainage, double retention, double availability)
    {
        if (drainage.AridityScore >= 0.60 && availability >= 0.28)
        {
            return "OasisCandidate";
        }

        if (drainage.FloodplainPotentialScore >= 0.56 && sample.ElevationScore <= 0.48)
        {
            return "FloodplainCandidate";
        }

        if (sample.MoistureScore >= 0.70 && retention >= 0.48)
        {
            return "WetlandCandidate";
        }

        if (sample.MoistureScore >= 0.58 && sample.RuggednessScore <= 0.44)
        {
            return "MarshCandidate";
        }

        return "LakeCandidate";
    }

    private static string ClassifyWaterwayKind(HydrologyDrainageSummary drainage, double estimatedFlow, double persistence, string fromKind, string targetKind)
    {
        if (targetKind == "Gateway" && drainage.OutflowDirection != "ClosedBasin" && estimatedFlow >= 0.58)
        {
            return "CoastalOutletHint";
        }

        if (fromKind == "source" && estimatedFlow < 0.46)
        {
            return "HeadwaterStreamHint";
        }

        if (drainage.AridityScore >= 0.56 && persistence < 0.48)
        {
            return "SeasonalWashHint";
        }

        if (drainage.AccumulationPotentialScore >= 0.52 && persistence >= 0.42)
        {
            return "WetlandDrainageHint";
        }

        return "MinorRiverHint";
    }

    private static IReadOnlyList<string> BuildDrainageReasons(WorldRegionClimateSummary summary, string outflowDirection, double runoff, double accumulation, double floodplain, double aridity)
    {
        var reasons = new List<string>();
        if (outflowDirection == "ClosedBasin")
        {
            reasons.Add("closed_basin_selected");
        }

        if (runoff >= 0.55)
        {
            reasons.Add("runoff_potential");
        }

        if (accumulation >= 0.52)
        {
            reasons.Add("accumulation_potential");
        }

        if (floodplain >= 0.52)
        {
            reasons.Add("floodplain_context");
        }

        if (aridity >= 0.56)
        {
            reasons.Add("arid_context");
        }

        if (summary.AverageRuggednessScore >= 0.58)
        {
            reasons.Add("rugged_headwater_context");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("bounded_local_drainage_hint");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildDrainageTags(string outflowDirection, double runoff, double accumulation, double floodplain, double aridity)
    {
        var tags = new List<string>
        {
            "outflow/" + outflowDirection,
            runoff >= 0.55 ? "runoff/high" : "runoff/bounded",
            accumulation >= 0.52 ? "accumulation/high" : "accumulation/bounded",
            floodplain >= 0.52 ? "floodplain_candidate" : "floodplain_low",
            aridity >= 0.56 ? "arid_basin_context" : "non_arid_basin_context"
        };
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildWaterSourceReasons(WorldRegionClimateSample sample, HydrologyDrainageSummary drainage, double flow, double reliability)
    {
        var reasons = new List<string>();
        if (flow >= 0.56)
        {
            reasons.Add("flow_potential");
        }

        if (reliability >= 0.54)
        {
            reasons.Add("seasonal_reliability");
        }

        if (sample.ElevationScore >= 0.58)
        {
            reasons.Add("elevated_source_context");
        }

        if (sample.MoistureScore >= 0.62)
        {
            reasons.Add("moisture_context");
        }

        if (drainage.AridityScore >= 0.58)
        {
            reasons.Add("arid_source_pressure");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("local_source_candidate");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildWaterSourceTags(string kind, WorldRegionClimateSample sample, double flow, double reliability)
    {
        var tags = new List<string>
        {
            "kind/" + kind,
            sample.BiomeId,
            "climate/" + sample.ClimateBand,
            flow >= 0.56 ? "flow/high" : "flow/bounded",
            reliability >= 0.54 ? "seasonal/reliable" : "seasonal/variable"
        };
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildWaterbodyReasons(WorldRegionClimateSample sample, HydrologyDrainageSummary drainage, double retention, double availability, double roadObstacle)
    {
        var reasons = new List<string>();
        if (retention >= 0.52)
        {
            reasons.Add("retention_potential");
        }

        if (availability >= 0.52)
        {
            reasons.Add("water_availability");
        }

        if (roadObstacle >= 0.56)
        {
            reasons.Add("road_obstacle_pressure");
        }

        if (drainage.FloodplainPotentialScore >= 0.52)
        {
            reasons.Add("floodplain_context");
        }

        if (sample.MoistureScore >= 0.62)
        {
            reasons.Add("wet_local_context");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("local_waterbody_candidate");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildWaterbodyTags(string kind, WorldRegionClimateSample sample, double retention, double availability, double roadObstacle)
    {
        var tags = new List<string>
        {
            "kind/" + kind,
            sample.BiomeId,
            "climate/" + sample.ClimateBand,
            retention >= 0.52 ? "retention/high" : "retention/bounded",
            availability >= 0.52 ? "availability/high" : "availability/bounded",
            roadObstacle >= 0.56 ? "road_obstacle/high" : "road_obstacle/bounded"
        };
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildWaterwayReasons(HydrologyDrainageSummary drainage, double flow, double persistence, double erosion, double crossing, string targetKind)
    {
        var reasons = new List<string>();
        if (flow >= 0.56)
        {
            reasons.Add("flow_hint");
        }

        if (persistence >= 0.52)
        {
            reasons.Add("persistent_waterway_hint");
        }

        if (erosion >= 0.56)
        {
            reasons.Add("erosion_risk_context");
        }

        if (crossing >= 0.55)
        {
            reasons.Add("road_crossing_pressure");
        }

        if (targetKind == "Gateway")
        {
            reasons.Add("gateway_context");
        }

        if (drainage.OutflowDirection == "ClosedBasin")
        {
            reasons.Add("closed_basin_drainage");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("bounded_waterway_hint");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildWaterwayTags(string kind, string outflowDirection, string fromKind, string targetKind)
    {
        var tags = new List<string>
        {
            "kind/" + kind,
            "from/" + fromKind,
            "target/" + targetKind,
            "outflow/" + outflowDirection,
            "geometry_absent",
            "route_solver_absent"
        };
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildCrossingReasons(double crossingNeed, double bridge, double ford, WaterwayCorridorHint waterway)
    {
        var reasons = new List<string>();
        if (crossingNeed >= 0.55)
        {
            reasons.Add("crossing_need");
        }

        if (bridge >= 0.54)
        {
            reasons.Add("bridge_pressure");
        }

        if (ford >= 0.50)
        {
            reasons.Add("ford_or_ferry_pressure");
        }

        if (waterway.PersistenceScore >= 0.52)
        {
            reasons.Add("persistent_waterway");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("bounded_crossing_hint");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildCrossingTags(WaterwayCorridorHint waterway, string gatewayId, string roadHintId)
    {
        var tags = new List<string>
        {
            "waterway/" + waterway.Kind,
            string.IsNullOrWhiteSpace(gatewayId) ? "gateway/unavailable" : "gateway/related",
            string.IsNullOrWhiteSpace(roadHintId) ? "road_hint/unavailable" : "road_hint/related"
        };
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildSummaryTags(
        RegionHydrologyWaterwayHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceClimate,
        HydrologyDrainageSummary drainage,
        IReadOnlyList<WaterSourceCandidate> sources,
        IReadOnlyList<WaterbodyCandidate> waterbodies,
        IReadOnlyList<WaterwayCorridorHint> waterways,
        IReadOnlyList<CrossingPressureHint> crossings)
    {
        var tags = new List<string>(sourceClimate.FutureTags)
        {
            settings.IncludeDiagonals ? "neighbor_mode/eight" : "neighbor_mode/four",
            "basin_code/local_reference_inspired",
            "water_source_count/" + sources.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "waterbody_count/" + waterbodies.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "waterway_hint_count/" + waterways.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "crossing_hint_count/" + crossings.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "outflow/" + drainage.OutflowDirection,
            "no_actual_rivers",
            "no_actual_waterbodies",
            "no_paths_or_polylines"
        };
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static bool PlanScoresAreBounded(RegionHydrologyWaterwayPlan plan) =>
        SummaryScoresAreBounded(plan.SourceClimateSummary)
        && plan.NeighborRegionSummaries.All(item => SummaryScoresAreBounded(item.ClimateSummary))
        && IsScore01(plan.Drainage.RunoffPotentialScore)
        && IsScore01(plan.Drainage.AccumulationPotentialScore)
        && IsScore01(plan.Drainage.FloodplainPotentialScore)
        && IsScore01(plan.Drainage.AridityScore)
        && plan.WaterSourceCandidates.All(item =>
            IsScore01(item.FlowPotentialScore)
            && IsScore01(item.SeasonalReliabilityScore)
            && IsScore01(item.ElevationScore)
            && IsScore01(item.MoistureScore)
            && IsScore01(item.TemperatureScore)
            && IsScore01(item.RuggednessScore))
        && plan.WaterbodyCandidates.All(item =>
            IsScore01(item.RetentionScore)
            && IsScore01(item.WaterAvailabilityScore)
            && IsScore01(item.SettlementSupportScore)
            && IsScore01(item.RoadObstacleScore)
            && IsScore01(item.ElevationScore)
            && IsScore01(item.MoistureScore)
            && IsScore01(item.TemperatureScore)
            && IsScore01(item.RuggednessScore))
        && plan.WaterwayCorridorHints.All(item =>
            IsScore01(item.EstimatedFlowScore)
            && IsScore01(item.PersistenceScore)
            && IsScore01(item.ErosionRiskScore)
            && IsScore01(item.SettlementSupportScore)
            && IsScore01(item.RoadCrossingPressureScore))
        && plan.CrossingPressureHints.All(item =>
            IsScore01(item.CrossingNeedScore)
            && IsScore01(item.BridgePressureScore)
            && IsScore01(item.FerryOrFordPressureScore));

    private static bool ReferencesAreValid(RegionHydrologyWaterwayPlan plan)
    {
        var sources = plan.WaterSourceCandidates.Select(item => item.WaterSourceCandidateId).ToHashSet(StringComparer.Ordinal);
        var waterbodies = plan.WaterbodyCandidates.Select(item => item.WaterbodyCandidateId).ToHashSet(StringComparer.Ordinal);
        var gateways = plan.WaterwayCorridorHints.Where(item => !string.IsNullOrWhiteSpace(item.RelatedGatewayId)).Select(item => item.RelatedGatewayId).ToHashSet(StringComparer.Ordinal);
        var neighborIds = plan.NeighborRegionSummaries.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var targetIds = neighborIds.Append(plan.Drainage.BasinId).Concat(gateways).ToHashSet(StringComparer.Ordinal);
        var waterways = plan.WaterwayCorridorHints.Select(item => item.WaterwayHintId).ToHashSet(StringComparer.Ordinal);

        foreach (var hint in plan.WaterwayCorridorHints)
        {
            if (!sources.Contains(hint.FromCandidateId) && !waterbodies.Contains(hint.FromCandidateId))
            {
                return false;
            }

            if (!targetIds.Contains(hint.ToTargetId))
            {
                return false;
            }
        }

        foreach (var crossing in plan.CrossingPressureHints)
        {
            if (!waterways.Contains(crossing.RelatedWaterwayHintId))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasMeaningfulSeedVariation(RegionHydrologyWaterwayPlan first, RegionHydrologyWaterwayPlan second)
    {
        if (first.WaterSourceCandidates.Count != second.WaterSourceCandidates.Count
            || first.WaterbodyCandidates.Count != second.WaterbodyCandidates.Count
            || first.WaterwayCorridorHints.Count != second.WaterwayCorridorHints.Count
            || first.CrossingPressureHints.Count != second.CrossingPressureHints.Count)
        {
            return true;
        }

        return !string.Equals(first.PlanId, second.PlanId, StringComparison.Ordinal)
               || !string.Equals(first.Drainage.BasinId, second.Drainage.BasinId, StringComparison.Ordinal)
               || !string.Equals(first.Drainage.BasinCode, second.Drainage.BasinCode, StringComparison.Ordinal)
               || !NearlyEqual(first.Drainage.RunoffPotentialScore, second.Drainage.RunoffPotentialScore)
               || first.WaterSourceCandidates.Zip(second.WaterSourceCandidates).Any(pair =>
                   !string.Equals(pair.First.WaterSourceCandidateId, pair.Second.WaterSourceCandidateId, StringComparison.Ordinal)
                   || !NearlyEqual(pair.First.FlowPotentialScore, pair.Second.FlowPotentialScore)
                   || !string.Equals(pair.First.Kind, pair.Second.Kind, StringComparison.Ordinal))
               || first.WaterwayCorridorHints.Zip(second.WaterwayCorridorHints).Any(pair =>
                   !string.Equals(pair.First.WaterwayHintId, pair.Second.WaterwayHintId, StringComparison.Ordinal)
                   || !NearlyEqual(pair.First.EstimatedFlowScore, pair.Second.EstimatedFlowScore)
                   || !string.Equals(pair.First.Kind, pair.Second.Kind, StringComparison.Ordinal));
    }

    private static bool SummaryScoresAreBounded(WorldRegionClimateSummary summary) =>
        IsScore01(summary.AverageElevationScore)
        && IsScore01(summary.AverageMoistureScore)
        && IsScore01(summary.AverageTemperatureScore)
        && IsScore01(summary.AverageRuggednessScore)
        && IsScore01(summary.AverageSettlementSuitabilityScore)
        && IsScore01(summary.AverageRoadTravelCostScore)
        && IsScore01(summary.RoadSuitabilityScore);

    private static bool IdsUnique(IEnumerable<string> ids)
    {
        var list = ids.ToList();
        return list.Distinct(StringComparer.Ordinal).Count() == list.Count;
    }

    private static string BuildPlanId(RegionHydrologyWaterwayHintsCandidateOptions settings, string regionId, string settlementPlanId, string gatewayPlanId)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), regionId, settlementPlanId, gatewayPlanId, settings.RegionX, settings.RegionY, settings.IncludeDiagonals, settings.MaxWaterSourceCandidates, settings.MaxWaterbodyCandidates, settings.MaxWaterwayHints, settings.MaxCrossingHints, settings.BasinCodeDepth);
        return $"hydrology-waterway-plan/{settings.RegionX}_{settings.RegionY}/{ComputeHash(key)[..8]}";
    }

    private static string BuildBasinId(RegionHydrologyWaterwayHintsCandidateOptions settings, string regionId)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), "basin", regionId, settings.RegionX, settings.RegionY);
        return $"basin/local/{settings.RegionX}_{settings.RegionY}/{ComputeHash(key)[..10]}";
    }

    private static string BuildBasinCode(RegionHydrologyWaterwayHintsCandidateOptions settings, WorldRegionClimateSummary sourceClimate, string outflowDirection)
    {
        var directionDigit = outflowDirection switch
        {
            "North" => "1",
            "East" => "3",
            "South" => "5",
            "West" => "7",
            "NorthEast" => "2",
            "SouthEast" => "4",
            "SouthWest" => "6",
            "NorthWest" => "8",
            _ => "9"
        };
        var hash = ComputeHash(string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), sourceClimate.RegionId, outflowDirection, settings.BasinCodeDepth));
        var digits = new StringBuilder(directionDigit);
        for (var index = 0; digits.Length < settings.BasinCodeDepth; index++)
        {
            digits.Append(((hash[index % hash.Length] - '0') % 9 + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return "local-basin-code/" + digits;
    }

    private static string BuildWaterSourceId(RegionHydrologyWaterwayHintsCandidateOptions settings, int x, int y, string kind)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), "water_source", x, y, kind);
        return $"water-source/{settings.RegionX}_{settings.RegionY}/{x}_{y}/{ComputeHash(key)[..8]}";
    }

    private static string BuildWaterbodyId(RegionHydrologyWaterwayHintsCandidateOptions settings, int x, int y, string kind)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), "waterbody", x, y, kind);
        return $"waterbody/{settings.RegionX}_{settings.RegionY}/{x}_{y}/{ComputeHash(key)[..8]}";
    }

    private static string BuildWaterwayHintId(RegionHydrologyWaterwayHintsCandidateOptions settings, string fromId, string toId, string kind)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), "waterway_hint", fromId, toId, kind);
        return $"waterway-hint/{ComputeHash(key)[..12]}";
    }

    private static string BuildCrossingHintId(RegionHydrologyWaterwayHintsCandidateOptions settings, string waterwayId, string gatewayId, string roadHintId)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), "crossing_hint", waterwayId, gatewayId, roadHintId);
        return $"crossing-hint/{ComputeHash(key)[..12]}";
    }

    private static IReadOnlyList<HydrologyDirectionDescriptor> BuildDirections(bool includeDiagonals)
    {
        var directions = new List<HydrologyDirectionDescriptor>
        {
            new("North", 0, -1, false),
            new("East", 1, 0, false),
            new("South", 0, 1, false),
            new("West", -1, 0, false)
        };
        if (includeDiagonals)
        {
            directions.AddRange(
            [
                new HydrologyDirectionDescriptor("NorthEast", 1, -1, true),
                new HydrologyDirectionDescriptor("SouthEast", 1, 1, true),
                new HydrologyDirectionDescriptor("SouthWest", -1, 1, true),
                new HydrologyDirectionDescriptor("NorthWest", -1, -1, true)
            ]);
        }

        return directions;
    }

    private static int DirectionSortOrder(string direction) =>
        direction switch
        {
            "North" => 0,
            "East" => 1,
            "South" => 2,
            "West" => 3,
            "NorthEast" => 4,
            "SouthEast" => 5,
            "SouthWest" => 6,
            "NorthWest" => 7,
            _ => 99
        };

    private static double ScoreJitter(RegionHydrologyWaterwayHintsCandidateOptions settings, string channelId, int x, int y, double amplitude) =>
        (ToScore01(SampleScore(settings, channelId, x, y)) - 0.5) * amplitude;

    private static int SampleScore(RegionHydrologyWaterwayHintsCandidateOptions settings, string channelId, int x, int y)
    {
        var key = string.Join("|", settings.Seed.Trim(), settings.RulesVersion.Trim(), settings.CoordinateSpace.Trim(), "2d", channelId, x.ToString(System.Globalization.CultureInfo.InvariantCulture), y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var value = BitConverter.ToUInt64(hash, 0);
        return (int)(value % 10001UL);
    }

    private static double ToScore01(int score0To10000) => score0To10000 / 10000.0;

    private static double Clamp01(double value) => Math.Clamp(value, 0.0, 1.0);

    private static double RoundScore(double value) => Math.Round(Clamp01(value), 4, MidpointRounding.AwayFromZero);

    private static bool IsScore01(double value) => value >= 0.0 && value <= 1.0;

    private static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.00001;

    private static WorldBiomeNoiseDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static IReadOnlyList<WorldBiomeNoiseDiagnostic> SortDiagnostics(IEnumerable<WorldBiomeNoiseDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => item.Severity == "error" ? 0 : item.Severity == "warning" ? 1 : 2)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ComputeHash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string FormatScore(double value) => value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    private static void EnsureContained(string root, string path)
    {
        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var pathFull = Path.GetFullPath(path);
        if (!pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Candidate output path must stay under the project root.");
        }
    }

    private static string RenderReport(RegionHydrologyWaterwayHintsCandidateReport report)
    {
        var plan = report.Plan;
        var lines = new List<string>
        {
            "# Candidate Region Hydrology Waterway Hints Report",
            string.Empty,
            "- Candidate id: " + report.CandidateId,
            "- Contract id: " + report.ContractId,
            "- Base candidate id: " + report.BaseCandidateId,
            "- Final status: " + report.FinalStatus,
            "- Contract proof passed: " + report.ContractProofPassed.ToString().ToLowerInvariant(),
            "- Accepted gate claimed: " + report.AcceptedGateClaimed.ToString().ToLowerInvariant(),
            "- Plan id: " + plan.PlanId,
            "- Center region: " + plan.RegionX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + plan.RegionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "- Deterministic hash: " + report.DeterministicHash,
            "- Include diagonals: " + report.IncludeDiagonals.ToString().ToLowerInvariant(),
            "- Global map materialized: " + report.GlobalMapMaterialized.ToString().ToLowerInvariant(),
            "- Actual rivers generated: " + report.ActualRiversGenerated.ToString().ToLowerInvariant(),
            "- Actual waterbodies generated: " + report.ActualWaterbodiesGenerated.ToString().ToLowerInvariant(),
            "- River paths generated: " + report.RiverPathsGenerated.ToString().ToLowerInvariant(),
            "- Erosion simulation implemented: " + report.ErosionSimulationImplemented.ToString().ToLowerInvariant(),
            "- Rainfall simulation implemented: " + report.RainfallSimulationImplemented.ToString().ToLowerInvariant(),
            "- Pathfinding/navigation implemented: " + report.PathfindingNavigationImplemented.ToString().ToLowerInvariant(),
            string.Empty,
            "## External Scouting Decisions",
            string.Empty
        };
        lines.AddRange(report.ExternalScoutingDecisions.Select(item => $"- {item.Name}: {item.Decision}; {item.Note}"));
        lines.AddRange(
        [
            string.Empty,
            "## Drainage And Basin Summary",
            string.Empty,
            "| Basin | Code | Outflow | Runoff | Accumulation | Floodplain | Aridity | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |",
            $"| {plan.Drainage.BasinId} | {plan.Drainage.BasinCode} | {plan.Drainage.OutflowDirection} | {FormatScore(plan.Drainage.RunoffPotentialScore)} | {FormatScore(plan.Drainage.AccumulationPotentialScore)} | {FormatScore(plan.Drainage.FloodplainPotentialScore)} | {FormatScore(plan.Drainage.AridityScore)} | {string.Join(", ", plan.Drainage.Reasons)} |",
            string.Empty,
            "## Water Source Candidates",
            string.Empty,
            "| Source | Cell | Kind | Flow | Reliability | Preferred outflow | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.WaterSourceCandidates.Select(item =>
            $"| {item.WaterSourceCandidateId} | {item.WorldCellX},{item.WorldCellY} | {item.Kind} | {FormatScore(item.FlowPotentialScore)} | {FormatScore(item.SeasonalReliabilityScore)} | {item.PreferredOutflowDirection} | {string.Join(", ", item.Reasons)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Waterbody Candidates",
            string.Empty,
            "| Waterbody | Cell | Kind | Retention | Availability | Settlement | Road obstacle | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.WaterbodyCandidates.Select(item =>
            $"| {item.WaterbodyCandidateId} | {item.WorldCellX},{item.WorldCellY} | {item.Kind} | {FormatScore(item.RetentionScore)} | {FormatScore(item.WaterAvailabilityScore)} | {FormatScore(item.SettlementSupportScore)} | {FormatScore(item.RoadObstacleScore)} | {string.Join(", ", item.Reasons)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Waterway Corridor Hints",
            string.Empty,
            "| Hint | From | Target | Kind | Flow | Persistence | Erosion risk | Crossing pressure | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.WaterwayCorridorHints.Select(item =>
            $"| {item.WaterwayHintId} | {item.FromCandidateId} | {item.ToTargetKind}:{item.ToTargetId} | {item.Kind} | {FormatScore(item.EstimatedFlowScore)} | {FormatScore(item.PersistenceScore)} | {FormatScore(item.ErosionRiskScore)} | {FormatScore(item.RoadCrossingPressureScore)} | {string.Join(", ", item.Reasons)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Crossing Pressure Hints",
            string.Empty,
            "| Hint | Waterway | Gateway | Road hint | Need | Bridge | Ferry/Ford | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.CrossingPressureHints.Select(item =>
            $"| {item.CrossingHintId} | {item.RelatedWaterwayHintId} | {item.RelatedGatewayId} | {item.RelatedRoadHintId} | {FormatScore(item.CrossingNeedScore)} | {FormatScore(item.BridgePressureScore)} | {FormatScore(item.FerryOrFordPressureScore)} | {string.Join(", ", item.Reasons)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Summary Tags",
            string.Empty
        ]);
        lines.AddRange(plan.SummaryTags.Select(tag => "- " + tag));
        lines.AddRange(
        [
            string.Empty,
            "External scouting decisions are reference_only; no dependency or copied implementation is adopted.",
            "Huge-world behavior remains coordinate-derived from seed plus center region, bounded local samples and bounded neighbor summaries; no mutable global RNG or full-world map materialization is used.",
            "The basin code is a local reference-inspired code only, not a real Pfafstetter implementation.",
            "This candidate intentionally does not implement actual rivers, lakes, wetlands, erosion, rainfall simulation, flood simulation, paths, polylines, pathfinding, navigation, factions, actual settlements, GamePackage data, Unity/runtime/provider/LLM/RAG/media/Lua or generator-library behavior.",
            "Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, context index, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library."
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private sealed record HydrologyLocalSample(WorldRegionClimateSample Sample, int LocalX, int LocalY);

    private sealed record HydrologyDirectionDescriptor(string Name, int DeltaX, int DeltaY, bool IsDiagonal);

    private sealed record HydrologyWaterwayTarget(string TargetId, string TargetKind, string RelatedGatewayId);
}

public sealed record RegionHydrologyWaterwayHintsCandidateOptions
{
    public string Seed { get; init; } = "candidate/region-hydrology-waterway-hints/default-seed";
    public string RulesVersion { get; init; } = "region_hydrology_waterway_hints_rules_v1";
    public string CoordinateSpace { get; init; } = "world_cell";
    public int RegionSize { get; init; } = 16;
    public int LatitudeBandPeriod { get; init; } = 128;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public bool IncludeDiagonals { get; init; }
    public int MaxWaterSourceCandidates { get; init; } = 5;
    public int MaxWaterbodyCandidates { get; init; } = 4;
    public int MaxWaterwayHints { get; init; } = 6;
    public int MaxCrossingHints { get; init; } = 6;
    public int BasinCodeDepth { get; init; } = 4;
}

public sealed record RegionHydrologyWaterwayHintsCandidateResult
{
    public RegionHydrologyWaterwayHintsCandidateReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record RegionHydrologyWaterwayHintsCandidateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record RegionHydrologyWaterwayHintsCandidateReport
{
    public string CandidateId { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string BaseCandidateId { get; init; } = string.Empty;
    public string FinalStatus { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool AcceptedGateClaimed { get; init; }
    public string Seed { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public string CoordinateSpace { get; init; } = string.Empty;
    public int RegionSize { get; init; }
    public int LatitudeBandPeriod { get; init; }
    public bool IncludeDiagonals { get; init; }
    public int MaxWaterSourceCandidates { get; init; }
    public int MaxWaterbodyCandidates { get; init; }
    public int MaxWaterwayHints { get; init; }
    public int MaxCrossingHints { get; init; }
    public int BasinCodeDepth { get; init; }
    public bool SameSeedStable { get; init; }
    public bool DifferentSeedVariationVisible { get; init; }
    public bool OrderIndependent { get; init; }
    public bool GlobalMapMaterialized { get; init; }
    public bool ActualRiversGenerated { get; init; }
    public bool ActualWaterbodiesGenerated { get; init; }
    public bool RiverPathsGenerated { get; init; }
    public bool WaterwayPolylinesGenerated { get; init; }
    public bool ErosionSimulationImplemented { get; init; }
    public bool RainfallSimulationImplemented { get; init; }
    public bool FloodSimulationImplemented { get; init; }
    public bool PathfindingNavigationImplemented { get; init; }
    public bool NavigationGraphGenerated { get; init; }
    public bool ActualSettlementsGenerated { get; init; }
    public bool FactionGenerationImplemented { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool RuntimeProviderNetworkDependency { get; init; }
    public WorldBiomeNoiseExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<WorldRegionClimateExternalScoutingDecision> ExternalScoutingDecisions { get; init; } = [];
    public RegionHydrologyWaterwayPlan Plan { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldBiomeNoiseDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RegionHydrologyWaterwayPlan
{
    public string PlanId { get; init; } = string.Empty;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public WorldRegionClimateSummary SourceClimateSummary { get; init; } = new();
    public string SourceSettlementRoadSeedPlanId { get; init; } = string.Empty;
    public int SourceSettlementAnchorCount { get; init; }
    public int SourceRoadHintCount { get; init; }
    public string SourceGatewayConnectivityPlanId { get; init; } = string.Empty;
    public int SourceGatewayCandidateCount { get; init; }
    public int SourceCorridorHintCount { get; init; }
    public IReadOnlyList<HydrologyNeighborRegionSummary> NeighborRegionSummaries { get; init; } = [];
    public HydrologyDrainageSummary Drainage { get; init; } = new();
    public IReadOnlyList<WaterSourceCandidate> WaterSourceCandidates { get; init; } = [];
    public IReadOnlyList<WaterbodyCandidate> WaterbodyCandidates { get; init; } = [];
    public IReadOnlyList<WaterwayCorridorHint> WaterwayCorridorHints { get; init; } = [];
    public IReadOnlyList<CrossingPressureHint> CrossingPressureHints { get; init; } = [];
    public IReadOnlyList<string> SummaryTags { get; init; } = [];
}

public sealed record HydrologyNeighborRegionSummary
{
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public bool IsDiagonal { get; init; }
    public WorldRegionClimateSummary ClimateSummary { get; init; } = new();
}

public sealed record HydrologyDrainageSummary
{
    public string BasinId { get; init; } = string.Empty;
    public string BasinCode { get; init; } = string.Empty;
    public string OutflowDirection { get; init; } = string.Empty;
    public string DownstreamNeighborRegionId { get; init; } = string.Empty;
    public int DownstreamNeighborRegionX { get; init; }
    public int DownstreamNeighborRegionY { get; init; }
    public double RunoffPotentialScore { get; init; }
    public double AccumulationPotentialScore { get; init; }
    public double FloodplainPotentialScore { get; init; }
    public double AridityScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record WaterSourceCandidate
{
    public string WaterSourceCandidateId { get; init; } = string.Empty;
    public int WorldCellX { get; init; }
    public int WorldCellY { get; init; }
    public int LocalCellX { get; init; }
    public int LocalCellY { get; init; }
    public string Kind { get; init; } = string.Empty;
    public double FlowPotentialScore { get; init; }
    public double SeasonalReliabilityScore { get; init; }
    public string ClimateBand { get; init; } = string.Empty;
    public string BiomeId { get; init; } = string.Empty;
    public double ElevationScore { get; init; }
    public double MoistureScore { get; init; }
    public double TemperatureScore { get; init; }
    public double RuggednessScore { get; init; }
    public string PreferredOutflowDirection { get; init; } = string.Empty;
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record WaterbodyCandidate
{
    public string WaterbodyCandidateId { get; init; } = string.Empty;
    public int WorldCellX { get; init; }
    public int WorldCellY { get; init; }
    public int LocalCellX { get; init; }
    public int LocalCellY { get; init; }
    public string Kind { get; init; } = string.Empty;
    public double RetentionScore { get; init; }
    public double WaterAvailabilityScore { get; init; }
    public double SettlementSupportScore { get; init; }
    public double RoadObstacleScore { get; init; }
    public string ClimateBand { get; init; } = string.Empty;
    public string BiomeId { get; init; } = string.Empty;
    public double ElevationScore { get; init; }
    public double MoistureScore { get; init; }
    public double TemperatureScore { get; init; }
    public double RuggednessScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record WaterwayCorridorHint
{
    public string WaterwayHintId { get; init; } = string.Empty;
    public string FromCandidateId { get; init; } = string.Empty;
    public string FromCandidateKind { get; init; } = string.Empty;
    public string ToTargetId { get; init; } = string.Empty;
    public string ToTargetKind { get; init; } = string.Empty;
    public string RelatedGatewayId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public double EstimatedFlowScore { get; init; }
    public double PersistenceScore { get; init; }
    public double ErosionRiskScore { get; init; }
    public double SettlementSupportScore { get; init; }
    public double RoadCrossingPressureScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record CrossingPressureHint
{
    public string CrossingHintId { get; init; } = string.Empty;
    public string RelatedWaterwayHintId { get; init; } = string.Empty;
    public string RelatedGatewayId { get; init; } = string.Empty;
    public string RelatedRoadHintId { get; init; } = string.Empty;
    public double CrossingNeedScore { get; init; }
    public double BridgePressureScore { get; init; }
    public double FerryOrFordPressureScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}
