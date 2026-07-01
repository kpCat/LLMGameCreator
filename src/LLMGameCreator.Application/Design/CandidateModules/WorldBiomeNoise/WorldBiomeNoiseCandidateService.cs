using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.CandidateModules.WorldBiomeNoise;

public sealed class WorldBiomeNoiseCandidateService
{
    public const string CandidateId = "candidate_world_biome_noise_v1";
    public const string ContractId = "world_biome_noise_contract_v1";
    public const string FinalStatus = "candidate_ready_for_serial_adoption";
    public const string RelativeOutputDirectory = ".llmgc/procedural/candidate-world-biome-noise-v1";
    public const string ReportJsonFileName = "candidate-world-biome-noise-v1-report.json";
    public const string ReportMarkdownFileName = "candidate-world-biome-noise-v1-report.md";
    public const string RegionClimateCandidateId = "candidate_world_region_climate_v1";
    public const string RegionClimateContractId = "world_region_climate_contract_v1";
    public const string RegionClimateRelativeOutputDirectory = ".llmgc/procedural/candidate-world-region-climate-v1";
    public const string RegionClimateReportJsonFileName = "candidate-world-region-climate-v1-report.json";
    public const string RegionClimateReportMarkdownFileName = "candidate-world-region-climate-v1-report.md";
    public const string SettlementRoadSeedsCandidateId = "candidate_region_settlement_road_seeds_v1";
    public const string SettlementRoadSeedsContractId = "region_settlement_road_seeds_contract_v1";
    public const string SettlementRoadSeedsRelativeOutputDirectory = ".llmgc/procedural/candidate-region-settlement-road-seeds-v1";
    public const string SettlementRoadSeedsReportJsonFileName = "candidate-region-settlement-road-seeds-v1-report.json";
    public const string SettlementRoadSeedsReportMarkdownFileName = "candidate-region-settlement-road-seeds-v1-report.md";
    public const string GatewayConnectivityHintsCandidateId = "candidate_region_gateway_connectivity_hints_v1";
    public const string GatewayConnectivityHintsContractId = "region_gateway_connectivity_hints_contract_v1";
    public const string GatewayConnectivityHintsRelativeOutputDirectory = ".llmgc/procedural/candidate-region-gateway-connectivity-hints-v1";
    public const string GatewayConnectivityHintsReportJsonFileName = "candidate-region-gateway-connectivity-hints-v1-report.json";
    public const string GatewayConnectivityHintsReportMarkdownFileName = "candidate-region-gateway-connectivity-hints-v1-report.md";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public WorldBiomeNoiseCandidateResult Build(WorldBiomeNoiseCandidateOptions? options = null)
    {
        var settings = options ?? new WorldBiomeNoiseCandidateOptions();
        var diagnostics = new List<WorldBiomeNoiseDiagnostic>
        {
            Diagnostic("info", "world_biome_noise.external_adapter.absent_optional", "FastNoiseLite", "FastNoise Lite was scouted but not adopted as a candidate dependency."),
            Diagnostic("info", "world_biome_noise.boundary", CandidateId, "Candidate remains adapter/contract proof only; no production integration or accepted gate is claimed.")
        };

        if (string.IsNullOrWhiteSpace(settings.Seed))
        {
            diagnostics.Add(Diagnostic("error", "world_biome_noise.seed.missing", "seed", "Seed is required for deterministic biome noise."));
        }

        if (!KnownCoordinateSpaces.Contains(settings.CoordinateSpace))
        {
            diagnostics.Add(Diagnostic("error", "world_biome_noise.coordinate_space.unknown", settings.CoordinateSpace, "Coordinate space must be one of the contract-defined values."));
        }

        var samples = diagnostics.Any(item => item.Severity == "error")
            ? []
            : BuildSamples(settings);

        var classifierBoundaryProof = BuildBoundaryProof();
        var differentSeedSamples = diagnostics.Any(item => item.Severity == "error")
            ? []
            : BuildSamples(settings with { Seed = settings.Seed + "/variant" });
        var variationVisible = samples.Count == differentSeedSamples.Count
            && samples.Zip(differentSeedSamples).Any(pair =>
                pair.First.ElevationScore0To10000 != pair.Second.ElevationScore0To10000
                || pair.First.MoistureScore0To10000 != pair.Second.MoistureScore0To10000
                || !string.Equals(pair.First.BiomeId, pair.Second.BiomeId, StringComparison.Ordinal));

        diagnostics.Add(Diagnostic(
            variationVisible ? "info" : "error",
            variationVisible ? "world_biome_noise.seed_variation.visible" : "world_biome_noise.seed_variation.missing",
            "seed",
            "Different seed should change at least one score or biome while preserving sample shape."));

        var externalExecution = new WorldBiomeNoiseExternalExecutionFlags();
        var contractProofPassed = diagnostics.All(item => item.Severity != "error")
                                  && classifierBoundaryProof.Passed
                                  && variationVisible
                                  && externalExecution.AllFalse;
        var reportWithoutHash = new WorldBiomeNoiseCandidateReport
        {
            CandidateId = CandidateId,
            ContractId = ContractId,
            FinalStatus = FinalStatus,
            ContractProofPassed = contractProofPassed,
            AcceptedGateClaimed = false,
            FastNoiseLiteDependencyAdopted = false,
            FastNoiseLiteDecision = "reference_only",
            FallbackDecision = "adapt_behind_adapter",
            AdapterRecommendation = "ISeededNoiseSampler",
            Seed = settings.Seed,
            RulesVersion = settings.RulesVersion,
            CoordinateSpace = settings.CoordinateSpace,
            NormalizationVersion = "hash_score_0_10000_v1",
            SampleCount = samples.Count,
            Samples = samples,
            ClassifierBoundaryProof = classifierBoundaryProof,
            SameSeedStable = true,
            DifferentSeedVariationVisible = variationVisible,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            RuntimeProviderNetworkDependency = false,
            ExternalExecution = externalExecution,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new WorldBiomeNoiseCandidateResult
        {
            Report = report,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderReport(report)
        };
    }

    public async Task<WorldBiomeNoiseCandidateWriteResult> WriteAsync(
        string projectRootPath,
        WorldBiomeNoiseCandidateResult result,
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

        return new WorldBiomeNoiseCandidateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    public async Task<WorldBiomeNoiseCandidateWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build();
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public WorldRegionClimateCandidateResult BuildRegionClimate(WorldRegionClimateCandidateOptions? options = null)
    {
        var settings = options ?? new WorldRegionClimateCandidateOptions();
        var diagnostics = new List<WorldBiomeNoiseDiagnostic>
        {
            Diagnostic("info", "world_region_climate.external_scouting.reference_only", "H3/S2/AutoBiomes/PCG", "External scouting remains reference-only; no dependency or external execution is adopted."),
            Diagnostic("info", "world_region_climate.boundary", RegionClimateCandidateId, "Candidate remains internal module proof only; no production integration or accepted gate is claimed.")
        };

        if (string.IsNullOrWhiteSpace(settings.Seed))
        {
            diagnostics.Add(Diagnostic("error", "world_region_climate.seed.missing", "seed", "Seed is required for deterministic climate sampling."));
        }

        if (!KnownCoordinateSpaces.Contains(settings.CoordinateSpace))
        {
            diagnostics.Add(Diagnostic("error", "world_region_climate.coordinate_space.unknown", settings.CoordinateSpace, "Coordinate space must be one of the contract-defined values."));
        }

        if (settings.RegionSize <= 0)
        {
            diagnostics.Add(Diagnostic("error", "world_region_climate.region_size.invalid", settings.RegionSize.ToString(System.Globalization.CultureInfo.InvariantCulture), "Region size must be positive."));
        }

        if (settings.LatitudeBandPeriod < 4)
        {
            diagnostics.Add(Diagnostic("error", "world_region_climate.latitude_period.invalid", settings.LatitudeBandPeriod.ToString(System.Globalization.CultureInfo.InvariantCulture), "Latitude band period must be at least 4 cells."));
        }

        var hasErrors = diagnostics.Any(item => item.Severity == "error");
        var samples = hasErrors ? [] : BuildRegionClimateSamples(settings);
        var summaries = hasErrors ? [] : BuildRegionSummaries(settings, samples);
        var differentSeedSamples = hasErrors ? [] : BuildRegionClimateSamples(settings with { Seed = settings.Seed + "/variant" });
        var differentSeedVariationVisible = samples.Count == differentSeedSamples.Count
                                            && samples.Zip(differentSeedSamples).Any(pair =>
                                                !NearlyEqual(pair.First.ElevationScore, pair.Second.ElevationScore)
                                                || !NearlyEqual(pair.First.MoistureScore, pair.Second.MoistureScore)
                                                || !NearlyEqual(pair.First.TemperatureScore, pair.Second.TemperatureScore)
                                                || !NearlyEqual(pair.First.RuggednessScore, pair.Second.RuggednessScore)
                                                || !string.Equals(pair.First.BiomeId, pair.Second.BiomeId, StringComparison.Ordinal)
                                                || !string.Equals(pair.First.ClimateBand, pair.Second.ClimateBand, StringComparison.Ordinal));

        diagnostics.Add(Diagnostic(
            differentSeedVariationVisible ? "info" : "error",
            differentSeedVariationVisible ? "world_region_climate.seed_variation.visible" : "world_region_climate.seed_variation.missing",
            "seed",
            "Different seed should change at least one climate score or classification while preserving sample shape."));

        var scoresBounded = samples.All(SampleScoresAreBounded)
                            && summaries.All(SummaryScoresAreBounded);
        diagnostics.Add(Diagnostic(
            scoresBounded ? "info" : "error",
            scoresBounded ? "world_region_climate.scores.bounded" : "world_region_climate.scores.out_of_range",
            "scores",
            "All climate, settlement and road scores must stay within 0..1."));

        var externalExecution = new WorldBiomeNoiseExternalExecutionFlags();
        var contractProofPassed = diagnostics.All(item => item.Severity != "error")
                                  && samples.Count > 0
                                  && summaries.Count > 0
                                  && differentSeedVariationVisible
                                  && scoresBounded
                                  && externalExecution.AllFalse;

        var reportWithoutHash = new WorldRegionClimateCandidateReport
        {
            CandidateId = RegionClimateCandidateId,
            ContractId = RegionClimateContractId,
            FinalStatus = FinalStatus,
            ContractProofPassed = contractProofPassed,
            AcceptedGateClaimed = false,
            Seed = settings.Seed,
            RulesVersion = settings.RulesVersion,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            ScoreSampler = "sha256_score_0_10000_v1_reused",
            BiomeClassifier = "world_biome_noise_contract_v1_classifier_reused",
            ClimateLogic = "latitude_like_coordinate_plus_sha256_variation_plus_elevation_cooling",
            SampleCount = samples.Count,
            Samples = samples,
            RegionSummaryCount = summaries.Count,
            RegionSummaries = summaries,
            SameSeedStable = true,
            DifferentSeedVariationVisible = differentSeedVariationVisible,
            OrderIndependent = true,
            GlobalMapMaterialized = false,
            ExternalScoutingDecisions =
            [
                new WorldRegionClimateExternalScoutingDecision("H3", "reference_only", "No dependency; useful future reference for hex indexing."),
                new WorldRegionClimateExternalScoutingDecision("S2", "reference_only", "No dependency; useful future reference for spherical cell partitioning."),
                new WorldRegionClimateExternalScoutingDecision("AutoBiomes", "reference_only", "No dependency; useful future reference for authoring biome rules."),
                new WorldRegionClimateExternalScoutingDecision("PCG papers", "reference_only", "No dependency; used only as conceptual reference.")
            ],
            SettlementGenerationImplemented = false,
            RoadGenerationImplemented = false,
            FactionGenerationImplemented = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            RuntimeProviderNetworkDependency = false,
            ExternalExecution = externalExecution,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new WorldRegionClimateCandidateResult
        {
            Report = report,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderRegionClimateReport(report)
        };
    }

    public async Task<WorldRegionClimateCandidateWriteResult> WriteRegionClimateAsync(
        string projectRootPath,
        WorldRegionClimateCandidateResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RegionClimateRelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.Combine(outputDirectory, RegionClimateReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, RegionClimateReportMarkdownFileName);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new WorldRegionClimateCandidateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    public async Task<WorldRegionClimateCandidateWriteResult> BuildAndWriteRegionClimateAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = BuildRegionClimate();
        return await WriteRegionClimateAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public WorldRegionClimateSample SampleRegionClimate(
        WorldRegionClimateCandidateOptions? options,
        WorldRegionClimateCoordinate coordinate)
    {
        var settings = options ?? new WorldRegionClimateCandidateOptions();
        return BuildRegionClimateSample(settings, coordinate);
    }

    public WorldRegionClimateSummary SummarizeRegionClimate(
        WorldRegionClimateCandidateOptions? options,
        int regionX,
        int regionY)
    {
        var settings = options ?? new WorldRegionClimateCandidateOptions();
        return BuildRegionSummary(settings, regionX, regionY);
    }

    public RegionSettlementRoadSeedsCandidateResult BuildSettlementRoadSeeds(
        RegionSettlementRoadSeedsCandidateOptions? options = null)
    {
        var settings = options ?? new RegionSettlementRoadSeedsCandidateOptions();
        var diagnostics = new List<WorldBiomeNoiseDiagnostic>
        {
            Diagnostic("info", "region_settlement_road_seeds.external_scouting.reference_only", "village/road/GDMC", "External scouting remains reference-only; no dependency or copied implementation is adopted."),
            Diagnostic("info", "region_settlement_road_seeds.boundary", SettlementRoadSeedsCandidateId, "Candidate produces planning seeds only; no settlements, paths, factions or navigation are generated.")
        };

        if (string.IsNullOrWhiteSpace(settings.Seed))
        {
            diagnostics.Add(Diagnostic("error", "region_settlement_road_seeds.seed.missing", "seed", "Seed is required for deterministic settlement and road seed planning."));
        }

        if (!KnownCoordinateSpaces.Contains(settings.CoordinateSpace))
        {
            diagnostics.Add(Diagnostic("error", "region_settlement_road_seeds.coordinate_space.unknown", settings.CoordinateSpace, "Coordinate space must be one of the contract-defined values."));
        }

        if (settings.RegionSize <= 0)
        {
            diagnostics.Add(Diagnostic("error", "region_settlement_road_seeds.region_size.invalid", settings.RegionSize.ToString(System.Globalization.CultureInfo.InvariantCulture), "Region size must be positive."));
        }

        if (settings.LatitudeBandPeriod < 4)
        {
            diagnostics.Add(Diagnostic("error", "region_settlement_road_seeds.latitude_period.invalid", settings.LatitudeBandPeriod.ToString(System.Globalization.CultureInfo.InvariantCulture), "Latitude band period must be at least 4 cells."));
        }

        if (settings.MaxSettlementAnchors < 0)
        {
            diagnostics.Add(Diagnostic("error", "region_settlement_road_seeds.max_anchors.invalid", settings.MaxSettlementAnchors.ToString(System.Globalization.CultureInfo.InvariantCulture), "Max settlement anchors cannot be negative."));
        }

        if (settings.MaxRoadHints < 0)
        {
            diagnostics.Add(Diagnostic("error", "region_settlement_road_seeds.max_road_hints.invalid", settings.MaxRoadHints.ToString(System.Globalization.CultureInfo.InvariantCulture), "Max road hints cannot be negative."));
        }

        var hasErrors = diagnostics.Any(item => item.Severity == "error");
        var plan = hasErrors ? new RegionSettlementRoadSeedPlan() : BuildSettlementRoadSeedPlan(settings);
        var differentSeedPlan = hasErrors ? new RegionSettlementRoadSeedPlan() : BuildSettlementRoadSeedPlan(settings with { Seed = settings.Seed + "/variant" });
        var differentSeedVariationVisible = !hasErrors && PlanHasMeaningfulSeedVariation(plan, differentSeedPlan);

        diagnostics.Add(Diagnostic(
            differentSeedVariationVisible ? "info" : "error",
            differentSeedVariationVisible ? "region_settlement_road_seeds.seed_variation.visible" : "region_settlement_road_seeds.seed_variation.missing",
            "seed",
            "Different seed should change at least one anchor or road-planning field while preserving bounded output shape."));

        var scoresBounded = PlanScoresAreBounded(plan);
        diagnostics.Add(Diagnostic(
            scoresBounded ? "info" : "error",
            scoresBounded ? "region_settlement_road_seeds.scores.bounded" : "region_settlement_road_seeds.scores.out_of_range",
            "scores",
            "All anchor and road scores must stay within 0..1."));

        var anchorIdsUnique = plan.SettlementAnchors.Select(item => item.AnchorId).Distinct(StringComparer.Ordinal).Count() == plan.SettlementAnchors.Count;
        diagnostics.Add(Diagnostic(
            anchorIdsUnique ? "info" : "error",
            anchorIdsUnique ? "region_settlement_road_seeds.anchor_ids.unique" : "region_settlement_road_seeds.anchor_ids.duplicate",
            "settlementAnchors",
            "Settlement anchor ids must be stable and unique."));

        var roadIdsUnique = plan.RoadHints.Select(item => item.RoadHintId).Distinct(StringComparer.Ordinal).Count() == plan.RoadHints.Count;
        diagnostics.Add(Diagnostic(
            roadIdsUnique ? "info" : "error",
            roadIdsUnique ? "region_settlement_road_seeds.road_hint_ids.unique" : "region_settlement_road_seeds.road_hint_ids.duplicate",
            "roadHints",
            "Road hint ids must be stable and unique."));

        var roadRefsValid = RoadHintsReferenceOnlyKnownAnchors(plan);
        diagnostics.Add(Diagnostic(
            roadRefsValid ? "info" : "error",
            roadRefsValid ? "region_settlement_road_seeds.road_refs.valid" : "region_settlement_road_seeds.road_refs.invalid",
            "roadHints",
            "Road hints must reference only existing anchors and must not self-link or duplicate unordered pairs."));

        var limitsRespected = plan.SettlementAnchors.Count <= settings.MaxSettlementAnchors && plan.RoadHints.Count <= settings.MaxRoadHints;
        diagnostics.Add(Diagnostic(
            limitsRespected ? "info" : "error",
            limitsRespected ? "region_settlement_road_seeds.limits.respected" : "region_settlement_road_seeds.limits.exceeded",
            "options",
            "Max settlement anchor and road hint options must be respected."));

        var externalExecution = new WorldBiomeNoiseExternalExecutionFlags();
        var contractProofPassed = diagnostics.All(item => item.Severity != "error")
                                  && plan.SettlementAnchors.Count > 0
                                  && differentSeedVariationVisible
                                  && scoresBounded
                                  && anchorIdsUnique
                                  && roadIdsUnique
                                  && roadRefsValid
                                  && limitsRespected
                                  && externalExecution.AllFalse;

        var reportWithoutHash = new RegionSettlementRoadSeedsCandidateReport
        {
            CandidateId = SettlementRoadSeedsCandidateId,
            ContractId = SettlementRoadSeedsContractId,
            BaseCandidateId = RegionClimateCandidateId,
            FinalStatus = FinalStatus,
            ContractProofPassed = contractProofPassed,
            AcceptedGateClaimed = false,
            Seed = settings.Seed,
            RulesVersion = settings.RulesVersion,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            MaxSettlementAnchors = settings.MaxSettlementAnchors,
            MaxRoadHints = settings.MaxRoadHints,
            SameSeedStable = true,
            DifferentSeedVariationVisible = differentSeedVariationVisible,
            OrderIndependent = true,
            GlobalMapMaterialized = false,
            ActualSettlementsGenerated = false,
            RoadPathsGenerated = false,
            NavigationPathfindingImplemented = false,
            FactionGenerationImplemented = false,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            RuntimeProviderNetworkDependency = false,
            ExternalExecution = externalExecution,
            ExternalScoutingDecisions =
            [
                new WorldRegionClimateExternalScoutingDecision("Procedural village generation", "reference_only", "Interest maps, settlement seeds and road skeletons inform the candidate shape only."),
                new WorldRegionClimateExternalScoutingDecision("Road network research", "reference_only", "Settlement nuclei, waterways/terrain and neighbourhood context stay conceptual references only."),
                new WorldRegionClimateExternalScoutingDecision("GDMC settlement generation", "reference_only", "Terrain-adaptive settlement ideas remain reference-only; no implementation is copied.")
            ],
            Plan = plan,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new RegionSettlementRoadSeedsCandidateResult
        {
            Report = report,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderSettlementRoadSeedsReport(report)
        };
    }

    public async Task<RegionSettlementRoadSeedsCandidateWriteResult> WriteSettlementRoadSeedsAsync(
        string projectRootPath,
        RegionSettlementRoadSeedsCandidateResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, SettlementRoadSeedsRelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.Combine(outputDirectory, SettlementRoadSeedsReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, SettlementRoadSeedsReportMarkdownFileName);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new RegionSettlementRoadSeedsCandidateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    public async Task<RegionSettlementRoadSeedsCandidateWriteResult> BuildAndWriteSettlementRoadSeedsAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = BuildSettlementRoadSeeds();
        return await WriteSettlementRoadSeedsAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public RegionGatewayConnectivityHintsCandidateResult BuildGatewayConnectivityHints(
        RegionGatewayConnectivityHintsCandidateOptions? options = null)
    {
        var settings = options ?? new RegionGatewayConnectivityHintsCandidateOptions();
        var diagnostics = new List<WorldBiomeNoiseDiagnostic>
        {
            Diagnostic("info", "region_gateway_connectivity_hints.external_scouting.reference_only", "road-network/neighbourhood/terrain", "External scouting remains reference-only; no dependency or copied implementation is adopted."),
            Diagnostic("info", "region_gateway_connectivity_hints.boundary", GatewayConnectivityHintsCandidateId, "Candidate produces gateway and corridor planning hints only; no roads, paths, navigation, settlements or factions are generated.")
        };

        if (string.IsNullOrWhiteSpace(settings.Seed))
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.seed.missing", "seed", "Seed is required for deterministic gateway connectivity planning."));
        }

        if (!KnownCoordinateSpaces.Contains(settings.CoordinateSpace))
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.coordinate_space.unknown", settings.CoordinateSpace, "Coordinate space must be one of the contract-defined values."));
        }

        if (settings.RegionSize <= 0)
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.region_size.invalid", settings.RegionSize.ToString(System.Globalization.CultureInfo.InvariantCulture), "Region size must be positive."));
        }

        if (settings.LatitudeBandPeriod < 4)
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.latitude_period.invalid", settings.LatitudeBandPeriod.ToString(System.Globalization.CultureInfo.InvariantCulture), "Latitude band period must be at least 4 cells."));
        }

        if (settings.MaxGatewaysPerSide < 0)
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.max_gateways.invalid", settings.MaxGatewaysPerSide.ToString(System.Globalization.CultureInfo.InvariantCulture), "Max gateways per side cannot be negative."));
        }

        if (settings.MaxCorridorHints < 0)
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.max_corridors.invalid", settings.MaxCorridorHints.ToString(System.Globalization.CultureInfo.InvariantCulture), "Max corridor hints cannot be negative."));
        }

        if (settings.MaxNeighborRegions < 0)
        {
            diagnostics.Add(Diagnostic("error", "region_gateway_connectivity_hints.max_neighbors.invalid", settings.MaxNeighborRegions.ToString(System.Globalization.CultureInfo.InvariantCulture), "Max neighbor regions cannot be negative."));
        }

        var hasErrors = diagnostics.Any(item => item.Severity == "error");
        var plan = hasErrors ? new RegionGatewayConnectivityPlan() : BuildGatewayConnectivityPlan(settings);
        var differentSeedPlan = hasErrors ? new RegionGatewayConnectivityPlan() : BuildGatewayConnectivityPlan(settings with { Seed = settings.Seed + "/variant" });
        var differentSeedVariationVisible = !hasErrors && GatewayConnectivityPlanHasMeaningfulSeedVariation(plan, differentSeedPlan);

        diagnostics.Add(Diagnostic(
            differentSeedVariationVisible ? "info" : "error",
            differentSeedVariationVisible ? "region_gateway_connectivity_hints.seed_variation.visible" : "region_gateway_connectivity_hints.seed_variation.missing",
            "seed",
            "Different seed should change at least one gateway or corridor planning field while preserving bounded output shape."));

        var scoresBounded = GatewayConnectivityPlanScoresAreBounded(plan);
        diagnostics.Add(Diagnostic(
            scoresBounded ? "info" : "error",
            scoresBounded ? "region_gateway_connectivity_hints.scores.bounded" : "region_gateway_connectivity_hints.scores.out_of_range",
            "scores",
            "All gateway and corridor scores must stay within 0..1."));

        var gatewayIdsUnique = plan.GatewayCandidates.Select(item => item.GatewayId).Distinct(StringComparer.Ordinal).Count() == plan.GatewayCandidates.Count;
        diagnostics.Add(Diagnostic(
            gatewayIdsUnique ? "info" : "error",
            gatewayIdsUnique ? "region_gateway_connectivity_hints.gateway_ids.unique" : "region_gateway_connectivity_hints.gateway_ids.duplicate",
            "gatewayCandidates",
            "Gateway ids must be stable and unique."));

        var corridorIdsUnique = plan.CorridorHints.Select(item => item.CorridorHintId).Distinct(StringComparer.Ordinal).Count() == plan.CorridorHints.Count;
        diagnostics.Add(Diagnostic(
            corridorIdsUnique ? "info" : "error",
            corridorIdsUnique ? "region_gateway_connectivity_hints.corridor_ids.unique" : "region_gateway_connectivity_hints.corridor_ids.duplicate",
            "corridorHints",
            "Corridor hint ids must be stable and unique."));

        var corridorRefsValid = CorridorHintsReferenceOnlyKnownGatewaysAndNeighbors(plan);
        diagnostics.Add(Diagnostic(
            corridorRefsValid ? "info" : "error",
            corridorRefsValid ? "region_gateway_connectivity_hints.corridor_refs.valid" : "region_gateway_connectivity_hints.corridor_refs.invalid",
            "corridorHints",
            "Corridor hints must reference existing gateways and neighbor regions without duplicate canonical region-pair links."));

        var limitsRespected = plan.GatewayCandidates
                                  .GroupBy(item => item.Direction, StringComparer.Ordinal)
                                  .All(group => group.Count() <= settings.MaxGatewaysPerSide)
                              && plan.CorridorHints.Count <= settings.MaxCorridorHints
                              && plan.NeighborRegionSummaries.Count <= ResolveMaxNeighborRegions(settings);
        diagnostics.Add(Diagnostic(
            limitsRespected ? "info" : "error",
            limitsRespected ? "region_gateway_connectivity_hints.limits.respected" : "region_gateway_connectivity_hints.limits.exceeded",
            "options",
            "Max gateway, corridor and neighbor options must be respected."));

        var externalExecution = new WorldBiomeNoiseExternalExecutionFlags();
        var contractProofPassed = diagnostics.All(item => item.Severity != "error")
                                  && plan.GatewayCandidates.Count > 0
                                  && plan.CorridorHints.Count > 0
                                  && differentSeedVariationVisible
                                  && scoresBounded
                                  && gatewayIdsUnique
                                  && corridorIdsUnique
                                  && corridorRefsValid
                                  && limitsRespected
                                  && externalExecution.AllFalse;

        var reportWithoutHash = new RegionGatewayConnectivityHintsCandidateReport
        {
            CandidateId = GatewayConnectivityHintsCandidateId,
            ContractId = GatewayConnectivityHintsContractId,
            BaseCandidateId = SettlementRoadSeedsCandidateId,
            FinalStatus = FinalStatus,
            ContractProofPassed = contractProofPassed,
            AcceptedGateClaimed = false,
            Seed = settings.Seed,
            RulesVersion = settings.RulesVersion,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            IncludeDiagonals = settings.IncludeDiagonals,
            MaxGatewaysPerSide = settings.MaxGatewaysPerSide,
            MaxCorridorHints = settings.MaxCorridorHints,
            MaxNeighborRegions = settings.MaxNeighborRegions,
            SameSeedStable = true,
            DifferentSeedVariationVisible = differentSeedVariationVisible,
            OrderIndependent = true,
            GlobalMapMaterialized = false,
            ActualRoadsGenerated = false,
            RoadPathsGenerated = false,
            NavigationPathfindingImplemented = false,
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
                new WorldRegionClimateExternalScoutingDecision("Road network neighbourhood context", "reference_only", "Neighbour and region-pair context informs candidate shape only."),
                new WorldRegionClimateExternalScoutingDecision("Settlement nuclei and terrain corridors", "reference_only", "Existing candidate settlement/road seeds and climate summaries are reused; no road implementation is copied."),
                new WorldRegionClimateExternalScoutingDecision("Patch/semantic road approaches", "reference_only", "Kept as future reference only; this candidate remains bounded deterministic hints.")
            ],
            Plan = plan,
            Diagnostics = SortDiagnostics(diagnostics)
        };

        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(Serialize(reportWithoutHash))
        };

        return new RegionGatewayConnectivityHintsCandidateResult
        {
            Report = report,
            ReportJson = Serialize(report),
            ReportMarkdown = RenderGatewayConnectivityHintsReport(report)
        };
    }

    public async Task<RegionGatewayConnectivityHintsCandidateWriteResult> WriteGatewayConnectivityHintsAsync(
        string projectRootPath,
        RegionGatewayConnectivityHintsCandidateResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, GatewayConnectivityHintsRelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var reportJsonPath = Path.Combine(outputDirectory, GatewayConnectivityHintsReportJsonFileName);
        var reportMarkdownPath = Path.Combine(outputDirectory, GatewayConnectivityHintsReportMarkdownFileName);
        await File.WriteAllTextAsync(reportJsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(reportMarkdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new RegionGatewayConnectivityHintsCandidateWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportJsonPath = reportJsonPath,
            ReportMarkdownPath = reportMarkdownPath
        };
    }

    public async Task<RegionGatewayConnectivityHintsCandidateWriteResult> BuildAndWriteGatewayConnectivityHintsAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = BuildGatewayConnectivityHints();
        return await WriteGatewayConnectivityHintsAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<WorldBiomeSample> BuildSamples(WorldBiomeNoiseCandidateOptions settings)
    {
        var coordinates = new[]
        {
            new WorldBiomeCoordinate(0, 0),
            new WorldBiomeCoordinate(7, 3),
            new WorldBiomeCoordinate(16, -4),
            new WorldBiomeCoordinate(-9, 11),
            new WorldBiomeCoordinate(32, 32)
        };

        return coordinates
            .Select(coordinate =>
            {
                var elevation = SampleScore(settings, "elevation", coordinate);
                var moisture = SampleScore(settings, "moisture", coordinate);
                var temperature = SampleScore(settings, "temperature", coordinate);
                return new WorldBiomeSample
                {
                    X = coordinate.X,
                    Y = coordinate.Y,
                    ElevationScore0To10000 = elevation,
                    MoistureScore0To10000 = moisture,
                    TemperatureScore0To10000 = temperature,
                    BiomeId = ClassifyBiome(elevation, moisture)
                };
            })
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .ToList();
    }

    private static WorldBiomeClassifierBoundaryProof BuildBoundaryProof()
    {
        var cases = new[]
        {
            new WorldBiomeClassifierCase("water_low_boundary", 2499, 9000, "biome/water"),
            new WorldBiomeClassifierCase("alpine_high_boundary", 7500, 1000, "biome/alpine"),
            new WorldBiomeClassifierCase("desert_dry_midland", 5000, 2999, "biome/desert"),
            new WorldBiomeClassifierCase("forest_wet_midland", 5000, 6500, "biome/forest"),
            new WorldBiomeClassifierCase("plains_midland", 5000, 3000, "biome/plains")
        };

        var evaluated = cases
            .Select(item => item with { ActualBiomeId = ClassifyBiome(item.ElevationScore0To10000, item.MoistureScore0To10000) })
            .ToList();

        return new WorldBiomeClassifierBoundaryProof
        {
            Cases = evaluated,
            Passed = evaluated.All(item => string.Equals(item.ExpectedBiomeId, item.ActualBiomeId, StringComparison.Ordinal))
        };
    }

    private static int SampleScore(WorldBiomeNoiseCandidateOptions settings, string channelId, WorldBiomeCoordinate coordinate)
    {
        return SampleScore(
            settings.Seed,
            settings.RulesVersion,
            settings.CoordinateSpace,
            channelId,
            coordinate.X,
            coordinate.Y);
    }

    private static int SampleScore(WorldRegionClimateCandidateOptions settings, string channelId, WorldRegionClimateCoordinate coordinate)
    {
        return SampleScore(
            settings.Seed,
            settings.RulesVersion,
            settings.CoordinateSpace,
            channelId,
            coordinate.X,
            coordinate.Y);
    }

    private static int SampleScore(string seed, string rulesVersion, string coordinateSpace, string channelId, int x, int y)
    {
        var key = string.Join(
            "|",
            seed.Trim(),
            rulesVersion.Trim(),
            coordinateSpace.Trim(),
            "2d",
            channelId,
            x.ToString(System.Globalization.CultureInfo.InvariantCulture),
            y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var value = BitConverter.ToUInt64(hash, 0);
        return (int)(value % 10001UL);
    }

    public static string ClassifyBiome(int elevationScore0To10000, int moistureScore0To10000)
    {
        if (elevationScore0To10000 < 2500)
        {
            return "biome/water";
        }

        if (elevationScore0To10000 >= 7500)
        {
            return "biome/alpine";
        }

        if (moistureScore0To10000 < 3000)
        {
            return "biome/desert";
        }

        if (moistureScore0To10000 >= 6500)
        {
            return "biome/forest";
        }

        return "biome/plains";
    }

    private static IReadOnlyList<WorldRegionClimateSample> BuildRegionClimateSamples(WorldRegionClimateCandidateOptions settings)
    {
        var coordinates = settings.SampleCoordinates.Count == 0
            ? DefaultRegionClimateCoordinates
            : settings.SampleCoordinates;

        return coordinates
            .Select(coordinate => BuildRegionClimateSample(settings, coordinate))
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .ToList();
    }

    private static WorldRegionClimateSample BuildRegionClimateSample(
        WorldRegionClimateCandidateOptions settings,
        WorldRegionClimateCoordinate coordinate)
    {
        var elevation = ToScore01(SampleScore(settings, "elevation", coordinate));
        var ruggedness = ToScore01(SampleScore(settings, "ruggedness", coordinate));
        var temperatureVariation = ToScore01(SampleScore(settings, "temperature", coordinate));
        var moistureVariation = ToScore01(SampleScore(settings, "moisture", coordinate));
        var latitudeHeat = LatitudeHeatScore(coordinate.Y, settings.LatitudeBandPeriod);
        var temperature = Clamp01(latitudeHeat * 0.82 + (temperatureVariation - 0.5) * 0.22 + 0.12 - elevation * 0.32);
        var moisture = Clamp01(moistureVariation * 0.72 + (1.0 - Math.Abs(temperature - 0.55)) * 0.18 - elevation * 0.10 - ruggedness * 0.06);
        var biomeId = ClassifyBiome(ToScore10000(elevation), ToScore10000(moisture));
        var climateBand = ClassifyClimateBand(temperature);
        var regionX = FloorDiv(coordinate.X, settings.RegionSize);
        var regionY = FloorDiv(coordinate.Y, settings.RegionSize);
        var settlementSuitability = CalculateSettlementSuitability(elevation, moisture, temperature, ruggedness, biomeId);
        var roadTravelCost = CalculateRoadTravelCost(elevation, moisture, temperature, ruggedness, biomeId);

        return new WorldRegionClimateSample
        {
            X = coordinate.X,
            Y = coordinate.Y,
            ElevationScore = RoundScore(elevation),
            MoistureScore = RoundScore(moisture),
            TemperatureScore = RoundScore(temperature),
            RuggednessScore = RoundScore(ruggedness),
            ClimateBand = climateBand,
            BiomeId = biomeId,
            RegionX = regionX,
            RegionY = regionY,
            RegionId = BuildRegionId(settings, regionX, regionY),
            SettlementSuitabilityScore = RoundScore(settlementSuitability),
            RoadTravelCostScore = RoundScore(roadTravelCost),
            Tags = BuildSampleTags(climateBand, biomeId, settlementSuitability, roadTravelCost)
        };
    }

    private static IReadOnlyList<WorldRegionClimateSummary> BuildRegionSummaries(
        WorldRegionClimateCandidateOptions settings,
        IReadOnlyList<WorldRegionClimateSample> samples)
    {
        return samples
            .Select(sample => (sample.RegionX, sample.RegionY))
            .Distinct()
            .OrderBy(item => item.RegionX)
            .ThenBy(item => item.RegionY)
            .Select(item => BuildRegionSummary(settings, item.RegionX, item.RegionY))
            .ToList();
    }

    private static WorldRegionClimateSummary BuildRegionSummary(
        WorldRegionClimateCandidateOptions settings,
        int regionX,
        int regionY)
    {
        var samples = BuildRegionSummarySamples(settings, regionX, regionY);
        var dominantBiome = samples
            .GroupBy(item => item.BiomeId, StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .First()
            .Key;
        var averageElevation = Average(samples, item => item.ElevationScore);
        var averageMoisture = Average(samples, item => item.MoistureScore);
        var averageTemperature = Average(samples, item => item.TemperatureScore);
        var averageRuggedness = Average(samples, item => item.RuggednessScore);
        var averageSettlement = Average(samples, item => item.SettlementSuitabilityScore);
        var averageRoadCost = Average(samples, item => item.RoadTravelCostScore);
        var roadSuitability = Clamp01(1.0 - averageRoadCost);

        return new WorldRegionClimateSummary
        {
            RegionX = regionX,
            RegionY = regionY,
            RegionId = BuildRegionId(settings, regionX, regionY),
            DominantBiomeId = dominantBiome,
            AverageElevationScore = RoundScore(averageElevation),
            AverageMoistureScore = RoundScore(averageMoisture),
            AverageTemperatureScore = RoundScore(averageTemperature),
            AverageRuggednessScore = RoundScore(averageRuggedness),
            AverageSettlementSuitabilityScore = RoundScore(averageSettlement),
            AverageRoadTravelCostScore = RoundScore(averageRoadCost),
            RoadSuitabilityScore = RoundScore(roadSuitability),
            SampleCount = samples.Count,
            UsedGlobalMapMaterialization = false,
            FutureTags = BuildRegionTags(dominantBiome, averageSettlement, roadSuitability, averageRuggedness)
        };
    }

    private static IReadOnlyList<WorldRegionClimateSample> BuildRegionSummarySamples(
        WorldRegionClimateCandidateOptions settings,
        int regionX,
        int regionY)
    {
        var minX = regionX * settings.RegionSize;
        var minY = regionY * settings.RegionSize;
        var maxOffset = settings.RegionSize - 1;
        var midOffset = settings.RegionSize / 2;
        var offsets = new[] { 0, midOffset, maxOffset }
            .Distinct()
            .Order()
            .ToArray();

        return offsets
            .SelectMany(xOffset => offsets.Select(yOffset => new WorldRegionClimateCoordinate(minX + xOffset, minY + yOffset)))
            .Select(coordinate => BuildRegionClimateSample(settings, coordinate))
            .OrderBy(item => item.X)
            .ThenBy(item => item.Y)
            .ToList();
    }

    private static IReadOnlyList<string> BuildSampleTags(
        string climateBand,
        string biomeId,
        double settlementSuitability,
        double roadTravelCost)
    {
        var tags = new List<string>
        {
            "climate/" + climateBand,
            biomeId
        };

        if (settlementSuitability >= 0.62)
        {
            tags.Add("future_settlement_candidate");
        }

        if (roadTravelCost <= 0.42)
        {
            tags.Add("future_road_easy");
        }
        else if (roadTravelCost >= 0.68)
        {
            tags.Add("future_road_high_cost");
        }

        return tags;
    }

    private static IReadOnlyList<string> BuildRegionTags(
        string dominantBiome,
        double averageSettlementSuitability,
        double roadSuitability,
        double averageRuggedness)
    {
        var tags = new List<string>
        {
            "dominant_" + dominantBiome.Replace('/', '_')
        };

        if (averageSettlementSuitability >= 0.55)
        {
            tags.Add("future_settlement_cluster_candidate");
        }
        else
        {
            tags.Add("future_sparse_settlement_candidate");
        }

        tags.Add(roadSuitability >= 0.55 ? "future_road_corridor_candidate" : "future_road_detour_candidate");

        if (averageRuggedness >= 0.60)
        {
            tags.Add("future_faction_border_candidate");
        }

        return tags;
    }

    private static RegionSettlementRoadSeedPlan BuildSettlementRoadSeedPlan(RegionSettlementRoadSeedsCandidateOptions settings)
    {
        var climateOptions = ToRegionClimateOptions(settings);
        var sourceSummary = BuildRegionSummary(climateOptions, settings.RegionX, settings.RegionY);
        var climateSamples = BuildRegionSummarySamples(climateOptions, settings.RegionX, settings.RegionY);
        var anchors = climateSamples
            .Where(sample => !string.Equals(sample.BiomeId, "biome/water", StringComparison.Ordinal))
            .Select((sample, index) => BuildSettlementAnchor(settings, sample, index, sourceSummary))
            .OrderByDescending(item => item.SuitabilityScore)
            .ThenBy(item => item.AnchorId, StringComparer.Ordinal)
            .Take(settings.MaxSettlementAnchors)
            .Select((anchor, index) => anchor with { Kind = ClassifySettlementAnchorKind(anchor, index) })
            .Select(anchor => anchor with { AnchorId = BuildSettlementAnchorId(settings, anchor.RegionX, anchor.RegionY, anchor.CellX, anchor.CellY, anchor.Kind) })
            .OrderByDescending(item => item.SuitabilityScore)
            .ThenBy(item => item.AnchorId, StringComparer.Ordinal)
            .ToList();
        var roadHints = BuildRoadHints(settings, sourceSummary, anchors);
        var planId = BuildSettlementRoadPlanId(settings, sourceSummary.RegionId);

        return new RegionSettlementRoadSeedPlan
        {
            PlanId = planId,
            RegionX = settings.RegionX,
            RegionY = settings.RegionY,
            RegionId = sourceSummary.RegionId,
            SourceClimateSummary = sourceSummary,
            SettlementAnchors = anchors,
            RoadHints = roadHints,
            SummaryTags = BuildSettlementRoadSummaryTags(sourceSummary, anchors, roadHints)
        };
    }

    private static WorldRegionClimateCandidateOptions ToRegionClimateOptions(RegionSettlementRoadSeedsCandidateOptions settings) =>
        new()
        {
            Seed = settings.Seed,
            RulesVersion = RegionClimateContractId,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod
        };

    private static RegionSettlementAnchorCandidate BuildSettlementAnchor(
        RegionSettlementRoadSeedsCandidateOptions settings,
        WorldRegionClimateSample sample,
        int sampleIndex,
        WorldRegionClimateSummary sourceSummary)
    {
        var scoreJitter = (ToScore01(SampleScore(settings.Seed, settings.RulesVersion, settings.CoordinateSpace, "settlement_anchor_jitter", sample.X, sample.Y)) - 0.5) * 0.06;
        var roadFriendlyBonus = Clamp01(1.0 - sample.RoadTravelCostScore) * 0.10;
        var regionalBonus = sourceSummary.AverageSettlementSuitabilityScore * 0.10;
        var suitability = Clamp01(sample.SettlementSuitabilityScore * 0.76 + roadFriendlyBonus + regionalBonus + scoreJitter);
        var kind = "VillageCandidate";

        return new RegionSettlementAnchorCandidate
        {
            AnchorId = BuildSettlementAnchorId(settings, sample.RegionX, sample.RegionY, sample.X, sample.Y, kind),
            RegionX = sample.RegionX,
            RegionY = sample.RegionY,
            CellX = sample.X,
            CellY = sample.Y,
            Kind = kind,
            SuitabilityScore = RoundScore(suitability),
            ElevationScore = sample.ElevationScore,
            MoistureScore = sample.MoistureScore,
            TemperatureScore = sample.TemperatureScore,
            RuggednessScore = sample.RuggednessScore,
            ClimateBand = sample.ClimateBand,
            BiomeId = sample.BiomeId,
            SourceClimateRegionId = sample.RegionId,
            SourceClimateSampleIndex = sampleIndex,
            Reasons = BuildSettlementAnchorReasons(sample, suitability),
            Tags = BuildSettlementAnchorTags(sample, suitability)
        };
    }

    private static string ClassifySettlementAnchorKind(RegionSettlementAnchorCandidate anchor, int sortedIndex)
    {
        if (sortedIndex == 0 && anchor.SuitabilityScore >= 0.58)
        {
            return "CapitalCandidate";
        }

        if (anchor.SuitabilityScore >= 0.54)
        {
            return "TownCandidate";
        }

        if (anchor.SuitabilityScore >= 0.34)
        {
            return "VillageCandidate";
        }

        return "OutpostCandidate";
    }

    private static IReadOnlyList<RegionRoadConnectionHint> BuildRoadHints(
        RegionSettlementRoadSeedsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        IReadOnlyList<RegionSettlementAnchorCandidate> anchors)
    {
        var candidates = new List<RegionRoadConnectionHint>();
        for (var left = 0; left < anchors.Count; left++)
        {
            for (var right = left + 1; right < anchors.Count; right++)
            {
                candidates.Add(BuildRoadHint(settings, sourceSummary, anchors[left], anchors[right]));
            }
        }

        return candidates
            .OrderByDescending(item => item.PriorityScore)
            .ThenBy(item => item.RoadHintId, StringComparer.Ordinal)
            .Take(settings.MaxRoadHints)
            .ToList();
    }

    private static RegionRoadConnectionHint BuildRoadHint(
        RegionSettlementRoadSeedsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        RegionSettlementAnchorCandidate fromAnchor,
        RegionSettlementAnchorCandidate toAnchor)
    {
        var ordered = string.CompareOrdinal(fromAnchor.AnchorId, toAnchor.AnchorId) <= 0
            ? (From: fromAnchor, To: toAnchor)
            : (From: toAnchor, To: fromAnchor);
        var dx = ordered.From.CellX - ordered.To.CellX;
        var dy = ordered.From.CellY - ordered.To.CellY;
        var distanceScore = Clamp01(Math.Sqrt(dx * dx + dy * dy) / Math.Max(1.0, settings.RegionSize * 1.42));
        var ruggedness = (ordered.From.RuggednessScore + ordered.To.RuggednessScore + sourceSummary.AverageRuggednessScore) / 3.0;
        var elevationDelta = Math.Abs(ordered.From.ElevationScore - ordered.To.ElevationScore);
        var estimatedCost = Clamp01(distanceScore * 0.38 + ruggedness * 0.28 + elevationDelta * 0.18 + sourceSummary.AverageRoadTravelCostScore * 0.16);
        var suitability = (ordered.From.SuitabilityScore + ordered.To.SuitabilityScore) / 2.0;
        var priority = Clamp01(suitability * 0.58 + (1.0 - estimatedCost) * 0.34 + sourceSummary.RoadSuitabilityScore * 0.08);
        var kind = ordered.From.Kind == "CapitalCandidate" || ordered.To.Kind == "CapitalCandidate"
            ? "TradeRouteHint"
            : "InternalRegionLink";

        return new RegionRoadConnectionHint
        {
            RoadHintId = BuildRoadHintId(settings, ordered.From.AnchorId, ordered.To.AnchorId, kind),
            FromId = ordered.From.AnchorId,
            ToId = ordered.To.AnchorId,
            ConnectionKind = kind,
            EstimatedCostScore = RoundScore(estimatedCost),
            PriorityScore = RoundScore(priority),
            Reasons = BuildRoadHintReasons(estimatedCost, priority, sourceSummary),
            Tags = BuildRoadHintTags(ordered.From, ordered.To, sourceSummary)
        };
    }

    private static IReadOnlyList<string> BuildSettlementAnchorReasons(WorldRegionClimateSample sample, double suitability)
    {
        var reasons = new List<string>();
        if (sample.MoistureScore >= 0.42 && sample.MoistureScore <= 0.72 && sample.TemperatureScore >= 0.32 && sample.TemperatureScore <= 0.72)
        {
            reasons.Add("fertile");
        }

        if (sample.MoistureScore < 0.30)
        {
            reasons.Add("dry");
        }

        if (sample.ClimateBand is "polar" or "cold")
        {
            reasons.Add("cold");
        }

        if (sample.RuggednessScore >= 0.58)
        {
            reasons.Add("rugged");
        }

        if (sample.ElevationScore <= 0.34 && sample.MoistureScore >= 0.58)
        {
            reasons.Add("coastal_like");
        }

        if (sample.RuggednessScore >= 0.48 && sample.RoadTravelCostScore <= 0.52)
        {
            reasons.Add("pass_candidate");
        }

        if (sample.RoadTravelCostScore <= 0.42 || suitability >= 0.50)
        {
            reasons.Add("road_friendly");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("frontier_candidate");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildSettlementAnchorTags(WorldRegionClimateSample sample, double suitability)
    {
        var tags = new List<string>
        {
            sample.BiomeId,
            "climate/" + sample.ClimateBand,
            suitability >= 0.50 ? "settlement_anchor_primary" : "settlement_anchor_secondary"
        };
        tags.AddRange(BuildSettlementAnchorReasons(sample, suitability).Select(item => "reason/" + item));
        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildRoadHintReasons(
        double estimatedCost,
        double priority,
        WorldRegionClimateSummary sourceSummary)
    {
        var reasons = new List<string>();
        if (estimatedCost <= 0.42)
        {
            reasons.Add("road_friendly");
        }
        else if (estimatedCost >= 0.68)
        {
            reasons.Add("rugged");
        }

        if (sourceSummary.RoadSuitabilityScore >= 0.56)
        {
            reasons.Add("regional_corridor");
        }

        if (priority >= 0.56)
        {
            reasons.Add("trade_route_candidate");
        }

        if (sourceSummary.AverageMoistureScore < 0.30)
        {
            reasons.Add("dry");
        }

        if (sourceSummary.AverageTemperatureScore < 0.38)
        {
            reasons.Add("cold");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("internal_link_candidate");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildRoadHintTags(
        RegionSettlementAnchorCandidate fromAnchor,
        RegionSettlementAnchorCandidate toAnchor,
        WorldRegionClimateSummary sourceSummary)
    {
        return new[]
            {
                "from/" + fromAnchor.Kind,
                "to/" + toAnchor.Kind,
                "dominant_" + sourceSummary.DominantBiomeId.Replace('/', '_'),
                sourceSummary.RoadSuitabilityScore >= 0.55 ? "road_corridor_candidate" : "road_detour_candidate"
            }
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> BuildSettlementRoadSummaryTags(
        WorldRegionClimateSummary summary,
        IReadOnlyList<RegionSettlementAnchorCandidate> anchors,
        IReadOnlyList<RegionRoadConnectionHint> roadHints)
    {
        var tags = new List<string>(summary.FutureTags)
        {
            "anchor_count/" + anchors.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "road_hint_count/" + roadHints.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        if (anchors.Any(item => item.Kind == "CapitalCandidate"))
        {
            tags.Add("capital_candidate_present");
        }

        if (roadHints.Any(item => item.ConnectionKind == "TradeRouteHint"))
        {
            tags.Add("trade_route_hint_present");
        }

        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static string BuildSettlementRoadPlanId(RegionSettlementRoadSeedsCandidateOptions settings, string regionId)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            regionId,
            settings.RegionX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.RegionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.MaxSettlementAnchors.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.MaxRoadHints.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return $"settlement-road-plan/{settings.RegionX}_{settings.RegionY}/{ComputeHash(key)[..8]}";
    }

    private static string BuildSettlementAnchorId(
        RegionSettlementRoadSeedsCandidateOptions settings,
        int regionX,
        int regionY,
        int cellX,
        int cellY,
        string kind)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            "settlement_anchor",
            regionX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            regionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            cellX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            cellY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            kind);
        return $"settlement-anchor/{regionX}_{regionY}/{cellX}_{cellY}/{ComputeHash(key)[..8]}";
    }

    private static string BuildRoadHintId(
        RegionSettlementRoadSeedsCandidateOptions settings,
        string fromId,
        string toId,
        string kind)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            "road_hint",
            kind,
            fromId,
            toId);
        return $"road-hint/{ComputeHash(key)[..12]}";
    }

    private static bool PlanHasMeaningfulSeedVariation(
        RegionSettlementRoadSeedPlan first,
        RegionSettlementRoadSeedPlan second)
    {
        if (first.SettlementAnchors.Count != second.SettlementAnchors.Count || first.RoadHints.Count != second.RoadHints.Count)
        {
            return true;
        }

        return !string.Equals(first.PlanId, second.PlanId, StringComparison.Ordinal)
               || first.SettlementAnchors.Zip(second.SettlementAnchors).Any(pair =>
                   !string.Equals(pair.First.AnchorId, pair.Second.AnchorId, StringComparison.Ordinal)
                   || !NearlyEqual(pair.First.SuitabilityScore, pair.Second.SuitabilityScore)
                   || pair.First.CellX != pair.Second.CellX
                   || pair.First.CellY != pair.Second.CellY
                   || !string.Equals(pair.First.Kind, pair.Second.Kind, StringComparison.Ordinal))
               || first.RoadHints.Zip(second.RoadHints).Any(pair =>
                   !string.Equals(pair.First.RoadHintId, pair.Second.RoadHintId, StringComparison.Ordinal)
                   || !NearlyEqual(pair.First.EstimatedCostScore, pair.Second.EstimatedCostScore)
                   || !NearlyEqual(pair.First.PriorityScore, pair.Second.PriorityScore));
    }

    private static bool PlanScoresAreBounded(RegionSettlementRoadSeedPlan plan) =>
        IsScore01(plan.SourceClimateSummary.AverageElevationScore)
        && IsScore01(plan.SourceClimateSummary.AverageMoistureScore)
        && IsScore01(plan.SourceClimateSummary.AverageTemperatureScore)
        && IsScore01(plan.SourceClimateSummary.AverageRuggednessScore)
        && IsScore01(plan.SourceClimateSummary.AverageSettlementSuitabilityScore)
        && IsScore01(plan.SourceClimateSummary.AverageRoadTravelCostScore)
        && IsScore01(plan.SourceClimateSummary.RoadSuitabilityScore)
        && plan.SettlementAnchors.All(anchor =>
            IsScore01(anchor.SuitabilityScore)
            && IsScore01(anchor.ElevationScore)
            && IsScore01(anchor.MoistureScore)
            && IsScore01(anchor.TemperatureScore)
            && IsScore01(anchor.RuggednessScore))
        && plan.RoadHints.All(hint => IsScore01(hint.EstimatedCostScore) && IsScore01(hint.PriorityScore));

    private static bool RoadHintsReferenceOnlyKnownAnchors(RegionSettlementRoadSeedPlan plan)
    {
        var anchors = plan.SettlementAnchors.Select(item => item.AnchorId).ToHashSet(StringComparer.Ordinal);
        var pairs = new HashSet<string>(StringComparer.Ordinal);
        foreach (var hint in plan.RoadHints)
        {
            if (string.Equals(hint.FromId, hint.ToId, StringComparison.Ordinal)
                || !anchors.Contains(hint.FromId)
                || !anchors.Contains(hint.ToId))
            {
                return false;
            }

            var key = string.CompareOrdinal(hint.FromId, hint.ToId) <= 0
                ? hint.FromId + "|" + hint.ToId
                : hint.ToId + "|" + hint.FromId;
            if (!pairs.Add(key))
            {
                return false;
            }
        }

        return true;
    }

    private static RegionGatewayConnectivityPlan BuildGatewayConnectivityPlan(RegionGatewayConnectivityHintsCandidateOptions settings)
    {
        var climateOptions = ToRegionClimateOptions(settings);
        var settlementOptions = ToSettlementRoadSeedsOptions(settings);
        var sourceSummary = BuildRegionSummary(climateOptions, settings.RegionX, settings.RegionY);
        var settlementPlan = BuildSettlementRoadSeedPlan(settlementOptions);
        var neighbors = BuildGatewayNeighborSummaries(settings, climateOptions);
        var gateways = BuildGatewayCandidates(settings, sourceSummary, settlementPlan, neighbors);
        var corridors = BuildCorridorHints(settings, sourceSummary, settlementPlan, neighbors, gateways);
        var planId = BuildGatewayConnectivityPlanId(settings, sourceSummary.RegionId, settlementPlan.PlanId);

        return new RegionGatewayConnectivityPlan
        {
            PlanId = planId,
            RegionX = settings.RegionX,
            RegionY = settings.RegionY,
            RegionId = sourceSummary.RegionId,
            SourceClimateSummary = sourceSummary,
            SourceSettlementRoadSeedPlanId = settlementPlan.PlanId,
            SourceSettlementAnchorCount = settlementPlan.SettlementAnchors.Count,
            SourceRoadHintCount = settlementPlan.RoadHints.Count,
            NeighborRegionSummaries = neighbors,
            GatewayCandidates = gateways,
            CorridorHints = corridors,
            SummaryTags = BuildGatewayConnectivitySummaryTags(settings, sourceSummary, settlementPlan, gateways, corridors)
        };
    }

    private static WorldRegionClimateCandidateOptions ToRegionClimateOptions(RegionGatewayConnectivityHintsCandidateOptions settings) =>
        new()
        {
            Seed = settings.Seed,
            RulesVersion = RegionClimateContractId,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod
        };

    private static RegionSettlementRoadSeedsCandidateOptions ToSettlementRoadSeedsOptions(RegionGatewayConnectivityHintsCandidateOptions settings) =>
        new()
        {
            Seed = settings.Seed,
            RulesVersion = SettlementRoadSeedsContractId,
            CoordinateSpace = settings.CoordinateSpace,
            RegionSize = settings.RegionSize,
            LatitudeBandPeriod = settings.LatitudeBandPeriod,
            RegionX = settings.RegionX,
            RegionY = settings.RegionY,
            MaxSettlementAnchors = 5,
            MaxRoadHints = 6
        };

    private static IReadOnlyList<GatewayNeighborRegionSummary> BuildGatewayNeighborSummaries(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        WorldRegionClimateCandidateOptions climateOptions)
    {
        return BuildGatewayDirections(settings)
            .Take(ResolveMaxNeighborRegions(settings))
            .Select(direction =>
            {
                var regionX = settings.RegionX + direction.DeltaX;
                var regionY = settings.RegionY + direction.DeltaY;
                return new GatewayNeighborRegionSummary
                {
                    RegionX = regionX,
                    RegionY = regionY,
                    RegionId = BuildRegionId(climateOptions, regionX, regionY),
                    Direction = direction.Direction,
                    Side = direction.Side,
                    IsDiagonal = direction.IsDiagonal,
                    ClimateSummary = BuildRegionSummary(climateOptions, regionX, regionY)
                };
            })
            .OrderBy(item => DirectionSortOrder(item.Direction))
            .ToList();
    }

    private static IReadOnlyList<RegionGatewayCandidate> BuildGatewayCandidates(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        RegionSettlementRoadSeedPlan settlementPlan,
        IReadOnlyList<GatewayNeighborRegionSummary> neighbors)
    {
        var candidates = new List<RegionGatewayCandidate>();
        foreach (var neighbor in neighbors)
        {
            var direction = GetGatewayDirection(neighbor.Direction);
            for (var index = 0; index < settings.MaxGatewaysPerSide; index++)
            {
                candidates.Add(BuildGatewayCandidate(settings, sourceSummary, settlementPlan, neighbor, direction, index));
            }
        }

        return candidates
            .OrderBy(item => DirectionSortOrder(item.Direction))
            .ThenByDescending(item => item.SuitabilityScore)
            .ThenBy(item => item.GatewayId, StringComparer.Ordinal)
            .ToList();
    }

    private static RegionGatewayCandidate BuildGatewayCandidate(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        RegionSettlementRoadSeedPlan settlementPlan,
        GatewayNeighborRegionSummary neighbor,
        RegionDirectionDescriptor direction,
        int candidateIndex)
    {
        var local = BuildGatewayLocalCell(settings, direction, candidateIndex);
        var worldX = settings.RegionX * settings.RegionSize + local.X;
        var worldY = settings.RegionY * settings.RegionSize + local.Y;
        var sample = BuildRegionClimateSample(ToRegionClimateOptions(settings), new WorldRegionClimateCoordinate(worldX, worldY));
        var neighborSummary = neighbor.ClimateSummary;
        var climateCompatibility = 1.0 - (
            Math.Abs(sourceSummary.AverageTemperatureScore - neighborSummary.AverageTemperatureScore) * 0.30
            + Math.Abs(sourceSummary.AverageMoistureScore - neighborSummary.AverageMoistureScore) * 0.22
            + Math.Abs(sourceSummary.AverageElevationScore - neighborSummary.AverageElevationScore) * 0.24
            + Math.Abs(sourceSummary.AverageRuggednessScore - neighborSummary.AverageRuggednessScore) * 0.24);
        var settlementContext = settlementPlan.SettlementAnchors.Count == 0
            ? 0.0
            : settlementPlan.SettlementAnchors.Max(item => item.SuitabilityScore);
        var jitter = (ToScore01(SampleScore(settings.Seed, settings.RulesVersion, settings.CoordinateSpace, "gateway_jitter", worldX, worldY)) - 0.5) * 0.05;
        var crossingCost = Clamp01(sample.RoadTravelCostScore * 0.48
                                   + sourceSummary.AverageRoadTravelCostScore * 0.22
                                   + neighborSummary.AverageRoadTravelCostScore * 0.20
                                   + Math.Abs(sourceSummary.AverageElevationScore - neighborSummary.AverageElevationScore) * 0.10);
        var suitability = Clamp01((1.0 - crossingCost) * 0.44
                                  + Clamp01(climateCompatibility) * 0.24
                                  + settlementContext * 0.14
                                  + sourceSummary.RoadSuitabilityScore * 0.10
                                  + neighborSummary.RoadSuitabilityScore * 0.08
                                  + jitter);
        var kind = ClassifyGatewayKind(sample, sourceSummary, neighborSummary, suitability, crossingCost);

        return new RegionGatewayCandidate
        {
            GatewayId = BuildGatewayId(settings, settings.RegionX, settings.RegionY, neighbor.RegionX, neighbor.RegionY, direction.Direction, local.X, local.Y, kind),
            CenterRegionX = settings.RegionX,
            CenterRegionY = settings.RegionY,
            CenterRegionId = sourceSummary.RegionId,
            NeighborRegionX = neighbor.RegionX,
            NeighborRegionY = neighbor.RegionY,
            NeighborRegionId = neighbor.RegionId,
            Side = direction.Side,
            Direction = direction.Direction,
            IsDiagonal = direction.IsDiagonal,
            BoundaryCellX = worldX,
            BoundaryCellY = worldY,
            LocalCellX = local.X,
            LocalCellY = local.Y,
            GatewayKind = kind,
            SuitabilityScore = RoundScore(suitability),
            EstimatedCrossingCostScore = RoundScore(crossingCost),
            ClimateBand = sample.ClimateBand,
            BiomeId = sample.BiomeId,
            ElevationScore = sample.ElevationScore,
            MoistureScore = sample.MoistureScore,
            TemperatureScore = sample.TemperatureScore,
            RuggednessScore = sample.RuggednessScore,
            Reasons = BuildGatewayReasons(sample, sourceSummary, neighborSummary, suitability, crossingCost),
            Tags = BuildGatewayTags(direction, kind, sample, suitability, crossingCost)
        };
    }

    private static IReadOnlyList<RegionCorridorHint> BuildCorridorHints(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        RegionSettlementRoadSeedPlan settlementPlan,
        IReadOnlyList<GatewayNeighborRegionSummary> neighbors,
        IReadOnlyList<RegionGatewayCandidate> gateways)
    {
        return neighbors
            .Select(neighbor => gateways
                .Where(gateway => string.Equals(gateway.NeighborRegionId, neighbor.RegionId, StringComparison.Ordinal))
                .OrderByDescending(gateway => gateway.SuitabilityScore)
                .ThenBy(gateway => gateway.GatewayId, StringComparer.Ordinal)
                .FirstOrDefault())
            .Where(gateway => gateway is not null)
            .Select(gateway => BuildCorridorHint(settings, sourceSummary, settlementPlan, gateway!))
            .OrderByDescending(item => item.PriorityScore)
            .ThenBy(item => item.CorridorHintId, StringComparer.Ordinal)
            .Take(settings.MaxCorridorHints)
            .ToList();
    }

    private static RegionCorridorHint BuildCorridorHint(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        RegionSettlementRoadSeedPlan settlementPlan,
        RegionGatewayCandidate gateway)
    {
        var canonicalPairId = BuildCanonicalRegionPairId(settings, settings.RegionX, settings.RegionY, gateway.NeighborRegionX, gateway.NeighborRegionY);
        var settlementBonus = settlementPlan.SettlementAnchors.Count == 0
            ? 0.0
            : settlementPlan.SettlementAnchors.Average(item => item.SuitabilityScore);
        var estimatedCost = Clamp01(gateway.EstimatedCrossingCostScore * 0.62 + sourceSummary.AverageRoadTravelCostScore * 0.24 + (1.0 - sourceSummary.RoadSuitabilityScore) * 0.14);
        var priority = Clamp01(gateway.SuitabilityScore * 0.54 + (1.0 - estimatedCost) * 0.28 + settlementBonus * 0.18);
        var kind = ClassifyCorridorKind(gateway, priority, settlementBonus);

        return new RegionCorridorHint
        {
            CorridorHintId = BuildCorridorHintId(settings, canonicalPairId, kind),
            CanonicalRegionPairId = canonicalPairId,
            FromGatewayId = gateway.GatewayId,
            ToNeighborRegionId = gateway.NeighborRegionId,
            ToGatewayId = string.Empty,
            CorridorKind = kind,
            EstimatedCostScore = RoundScore(estimatedCost),
            PriorityScore = RoundScore(priority),
            Reasons = BuildCorridorReasons(gateway, estimatedCost, priority, settlementBonus),
            Tags = BuildCorridorTags(gateway, kind)
        };
    }

    private static (int X, int Y) BuildGatewayLocalCell(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        RegionDirectionDescriptor direction,
        int candidateIndex)
    {
        var max = Math.Max(0, settings.RegionSize - 1);
        var jitter = SampleScore(settings.Seed, settings.RulesVersion, settings.CoordinateSpace, "gateway_local_cell", settings.RegionX + direction.DeltaX * 31, settings.RegionY + direction.DeltaY * 31 + candidateIndex);
        var along = settings.MaxGatewaysPerSide <= 1
            ? settings.RegionSize / 2
            : (int)Math.Round((candidateIndex + 1) * (settings.RegionSize - 1) / (double)(settings.MaxGatewaysPerSide + 1), MidpointRounding.AwayFromZero);
        along = Math.Clamp(along + (jitter % 3) - 1, 0, max);
        var diagonalOffset = Math.Clamp(candidateIndex, 0, max);

        return direction.Direction switch
        {
            "North" => (along, 0),
            "East" => (max, along),
            "South" => (along, max),
            "West" => (0, along),
            "NorthEast" => (max - diagonalOffset, diagonalOffset),
            "SouthEast" => (max - diagonalOffset, max - diagonalOffset),
            "SouthWest" => (diagonalOffset, max - diagonalOffset),
            "NorthWest" => (diagonalOffset, diagonalOffset),
            _ => (along, along)
        };
    }

    private static string ClassifyGatewayKind(
        WorldRegionClimateSample sample,
        WorldRegionClimateSummary sourceSummary,
        WorldRegionClimateSummary neighborSummary,
        double suitability,
        double crossingCost)
    {
        if (string.Equals(sample.BiomeId, "biome/water", StringComparison.Ordinal) || sourceSummary.AverageMoistureScore >= 0.72 || neighborSummary.AverageMoistureScore >= 0.72)
        {
            return "CoastalCrossingCandidate";
        }

        if (sample.RuggednessScore >= 0.62 || sourceSummary.AverageRuggednessScore >= 0.62 || neighborSummary.AverageRuggednessScore >= 0.62)
        {
            return crossingCost <= 0.58 ? "MountainPassCandidate" : "WildernessTrailCandidate";
        }

        if (sample.ElevationScore <= 0.38 && sample.MoistureScore >= 0.46)
        {
            return "ValleyPassCandidate";
        }

        return suitability >= 0.58 ? "TradePassCandidate" : "WildernessTrailCandidate";
    }

    private static string ClassifyCorridorKind(RegionGatewayCandidate gateway, double priority, double settlementBonus)
    {
        if (priority >= 0.58 || gateway.GatewayKind == "TradePassCandidate")
        {
            return "RegionalTradeHint";
        }

        if (settlementBonus >= 0.42 || gateway.GatewayKind == "ValleyPassCandidate")
        {
            return "SettlementConnectorHint";
        }

        return "WildernessConnectorHint";
    }

    private static IReadOnlyList<string> BuildGatewayReasons(
        WorldRegionClimateSample sample,
        WorldRegionClimateSummary sourceSummary,
        WorldRegionClimateSummary neighborSummary,
        double suitability,
        double crossingCost)
    {
        var reasons = new List<string>();
        if (crossingCost <= 0.42)
        {
            reasons.Add("low_crossing_cost");
        }

        if (suitability >= 0.58)
        {
            reasons.Add("high_connectivity_suitability");
        }

        if (sample.RuggednessScore >= 0.58)
        {
            reasons.Add("rugged_pass_context");
        }

        if (sample.MoistureScore >= 0.66)
        {
            reasons.Add("wet_or_coastal_context");
        }

        if (sourceSummary.RoadSuitabilityScore >= 0.55 || neighborSummary.RoadSuitabilityScore >= 0.55)
        {
            reasons.Add("road_suitability_context");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("frontier_gateway_candidate");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildGatewayTags(
        RegionDirectionDescriptor direction,
        string kind,
        WorldRegionClimateSample sample,
        double suitability,
        double crossingCost)
    {
        var tags = new List<string>
        {
            "direction/" + direction.Direction,
            "side/" + direction.Side,
            "kind/" + kind,
            sample.BiomeId,
            "climate/" + sample.ClimateBand,
            suitability >= 0.58 ? "gateway_primary" : "gateway_secondary",
            crossingCost <= 0.42 ? "crossing_cost_low" : "crossing_cost_bounded"
        };

        if (direction.IsDiagonal)
        {
            tags.Add("diagonal_neighbor");
        }

        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildCorridorReasons(
        RegionGatewayCandidate gateway,
        double estimatedCost,
        double priority,
        double settlementBonus)
    {
        var reasons = new List<string>();
        if (estimatedCost <= 0.42)
        {
            reasons.Add("low_estimated_cost");
        }

        if (priority >= 0.58)
        {
            reasons.Add("high_priority");
        }

        if (settlementBonus >= 0.42)
        {
            reasons.Add("settlement_seed_context");
        }

        if (gateway.GatewayKind is "TradePassCandidate" or "ValleyPassCandidate")
        {
            reasons.Add("gateway_kind_supports_connector");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("bounded_neighbor_connector");
        }

        return reasons.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildCorridorTags(RegionGatewayCandidate gateway, string kind)
    {
        var tags = new List<string>
        {
            "kind/" + kind,
            "direction/" + gateway.Direction,
            "gateway_kind/" + gateway.GatewayKind,
            gateway.IsDiagonal ? "diagonal_neighbor" : "cardinal_neighbor"
        };

        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> BuildGatewayConnectivitySummaryTags(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        WorldRegionClimateSummary sourceSummary,
        RegionSettlementRoadSeedPlan settlementPlan,
        IReadOnlyList<RegionGatewayCandidate> gateways,
        IReadOnlyList<RegionCorridorHint> corridors)
    {
        var tags = new List<string>(sourceSummary.FutureTags)
        {
            settings.IncludeDiagonals ? "neighbor_mode/eight" : "neighbor_mode/four",
            "gateway_count/" + gateways.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "corridor_hint_count/" + corridors.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "source_anchor_count/" + settlementPlan.SettlementAnchors.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "canonical_region_pair_ids"
        };

        if (gateways.Any(item => item.GatewayKind == "TradePassCandidate"))
        {
            tags.Add("trade_gateway_candidate_present");
        }

        if (corridors.Any(item => item.CorridorKind == "RegionalTradeHint"))
        {
            tags.Add("regional_trade_hint_present");
        }

        return tags.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static string BuildGatewayConnectivityPlanId(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        string regionId,
        string settlementPlanId)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            regionId,
            settlementPlanId,
            settings.RegionX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.RegionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.IncludeDiagonals.ToString(),
            settings.MaxGatewaysPerSide.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.MaxCorridorHints.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settings.MaxNeighborRegions.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return $"gateway-connectivity-plan/{settings.RegionX}_{settings.RegionY}/{ComputeHash(key)[..8]}";
    }

    private static string BuildGatewayId(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        int centerRegionX,
        int centerRegionY,
        int neighborRegionX,
        int neighborRegionY,
        string direction,
        int localX,
        int localY,
        string kind)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            "gateway",
            centerRegionX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            centerRegionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            neighborRegionX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            neighborRegionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            direction,
            localX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            localY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            kind);
        return $"gateway/{centerRegionX}_{centerRegionY}/{direction}/{localX}_{localY}/{ComputeHash(key)[..8]}";
    }

    private static string BuildCanonicalRegionPairId(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        int firstRegionX,
        int firstRegionY,
        int secondRegionX,
        int secondRegionY)
    {
        var firstBeforeSecond = firstRegionX < secondRegionX || (firstRegionX == secondRegionX && firstRegionY <= secondRegionY);
        var aX = firstBeforeSecond ? firstRegionX : secondRegionX;
        var aY = firstBeforeSecond ? firstRegionY : secondRegionY;
        var bX = firstBeforeSecond ? secondRegionX : firstRegionX;
        var bY = firstBeforeSecond ? secondRegionY : firstRegionY;
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            settings.RegionSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "canonical_region_pair",
            aX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            aY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            bX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            bY.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return $"region-pair/{aX}_{aY}__{bX}_{bY}/{ComputeHash(key)[..10]}";
    }

    private static string BuildCorridorHintId(
        RegionGatewayConnectivityHintsCandidateOptions settings,
        string canonicalPairId,
        string kind)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            "corridor_hint",
            canonicalPairId,
            kind);
        return $"corridor-hint/{ComputeHash(key)[..12]}";
    }

    private static bool GatewayConnectivityPlanHasMeaningfulSeedVariation(
        RegionGatewayConnectivityPlan first,
        RegionGatewayConnectivityPlan second)
    {
        if (first.GatewayCandidates.Count != second.GatewayCandidates.Count || first.CorridorHints.Count != second.CorridorHints.Count)
        {
            return true;
        }

        return !string.Equals(first.PlanId, second.PlanId, StringComparison.Ordinal)
               || first.GatewayCandidates.Zip(second.GatewayCandidates).Any(pair =>
                   !string.Equals(pair.First.GatewayId, pair.Second.GatewayId, StringComparison.Ordinal)
                   || !NearlyEqual(pair.First.SuitabilityScore, pair.Second.SuitabilityScore)
                   || !NearlyEqual(pair.First.EstimatedCrossingCostScore, pair.Second.EstimatedCrossingCostScore)
                   || !string.Equals(pair.First.GatewayKind, pair.Second.GatewayKind, StringComparison.Ordinal))
               || first.CorridorHints.Zip(second.CorridorHints).Any(pair =>
                   !string.Equals(pair.First.CorridorHintId, pair.Second.CorridorHintId, StringComparison.Ordinal)
                   || !NearlyEqual(pair.First.EstimatedCostScore, pair.Second.EstimatedCostScore)
                   || !NearlyEqual(pair.First.PriorityScore, pair.Second.PriorityScore)
                   || !string.Equals(pair.First.CorridorKind, pair.Second.CorridorKind, StringComparison.Ordinal));
    }

    private static bool GatewayConnectivityPlanScoresAreBounded(RegionGatewayConnectivityPlan plan) =>
        SummaryScoresAreBounded(plan.SourceClimateSummary)
        && plan.NeighborRegionSummaries.All(item => SummaryScoresAreBounded(item.ClimateSummary))
        && plan.GatewayCandidates.All(gateway =>
            IsScore01(gateway.SuitabilityScore)
            && IsScore01(gateway.EstimatedCrossingCostScore)
            && IsScore01(gateway.ElevationScore)
            && IsScore01(gateway.MoistureScore)
            && IsScore01(gateway.TemperatureScore)
            && IsScore01(gateway.RuggednessScore))
        && plan.CorridorHints.All(hint => IsScore01(hint.EstimatedCostScore) && IsScore01(hint.PriorityScore));

    private static bool CorridorHintsReferenceOnlyKnownGatewaysAndNeighbors(RegionGatewayConnectivityPlan plan)
    {
        var gateways = plan.GatewayCandidates.Select(item => item.GatewayId).ToHashSet(StringComparer.Ordinal);
        var neighbors = plan.NeighborRegionSummaries.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var pairs = new HashSet<string>(StringComparer.Ordinal);
        foreach (var hint in plan.CorridorHints)
        {
            if (!gateways.Contains(hint.FromGatewayId)
                || !neighbors.Contains(hint.ToNeighborRegionId)
                || string.IsNullOrWhiteSpace(hint.CanonicalRegionPairId)
                || !pairs.Add(hint.CanonicalRegionPairId))
            {
                return false;
            }
        }

        return true;
    }

    private static IReadOnlyList<RegionDirectionDescriptor> BuildGatewayDirections(RegionGatewayConnectivityHintsCandidateOptions settings)
    {
        var directions = new List<RegionDirectionDescriptor>
        {
            new("North", "North", 0, -1, false),
            new("East", "East", 1, 0, false),
            new("South", "South", 0, 1, false),
            new("West", "West", -1, 0, false)
        };

        if (settings.IncludeDiagonals)
        {
            directions.AddRange(
            [
                new RegionDirectionDescriptor("NorthEast", "NorthEast", 1, -1, true),
                new RegionDirectionDescriptor("SouthEast", "SouthEast", 1, 1, true),
                new RegionDirectionDescriptor("SouthWest", "SouthWest", -1, 1, true),
                new RegionDirectionDescriptor("NorthWest", "NorthWest", -1, -1, true)
            ]);
        }

        return directions;
    }

    private static RegionDirectionDescriptor GetGatewayDirection(string direction) =>
        BuildGatewayDirections(new RegionGatewayConnectivityHintsCandidateOptions { IncludeDiagonals = true })
            .First(item => string.Equals(item.Direction, direction, StringComparison.Ordinal));

    private static int ResolveMaxNeighborRegions(RegionGatewayConnectivityHintsCandidateOptions settings)
    {
        var defaultCount = settings.IncludeDiagonals ? 8 : 4;
        return settings.MaxNeighborRegions <= 0
            ? defaultCount
            : Math.Min(settings.MaxNeighborRegions, defaultCount);
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

    private static double CalculateSettlementSuitability(
        double elevation,
        double moisture,
        double temperature,
        double ruggedness,
        string biomeId)
    {
        var biomePenalty = biomeId switch
        {
            "biome/water" => 0.72,
            "biome/alpine" => 0.46,
            "biome/desert" => 0.25,
            _ => 0.0
        };
        var score = 0.72
                    - Math.Abs(elevation - 0.45) * 0.55
                    - Math.Abs(moisture - 0.55) * 0.22
                    - Math.Abs(temperature - 0.55) * 0.24
                    - ruggedness * 0.34
                    - biomePenalty;
        return Clamp01(score);
    }

    private static double CalculateRoadTravelCost(
        double elevation,
        double moisture,
        double temperature,
        double ruggedness,
        string biomeId)
    {
        var biomeCost = biomeId switch
        {
            "biome/water" => 0.42,
            "biome/alpine" => 0.24,
            "biome/forest" => 0.08,
            _ => 0.0
        };
        var score = ruggedness * 0.44
                    + Math.Abs(elevation - 0.45) * 0.25
                    + Math.Abs(temperature - 0.52) * 0.16
                    + Math.Abs(moisture - 0.50) * 0.08
                    + biomeCost;
        return Clamp01(score);
    }

    private static string ClassifyClimateBand(double temperatureScore)
    {
        if (temperatureScore < 0.20)
        {
            return "polar";
        }

        if (temperatureScore < 0.38)
        {
            return "cold";
        }

        if (temperatureScore < 0.62)
        {
            return "temperate";
        }

        if (temperatureScore < 0.82)
        {
            return "warm";
        }

        return "hot";
    }

    private static double LatitudeHeatScore(int y, int latitudeBandPeriod)
    {
        var period = Math.Max(4, latitudeBandPeriod);
        var phase = PositiveModulo(y, period) / (double)(period - 1);
        return Clamp01(1.0 - Math.Abs(phase * 2.0 - 1.0));
    }

    private static string BuildRegionId(WorldRegionClimateCandidateOptions settings, int regionX, int regionY)
    {
        var key = string.Join(
            "|",
            settings.Seed.Trim(),
            settings.RulesVersion.Trim(),
            settings.CoordinateSpace.Trim(),
            "region",
            settings.RegionSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            regionX.ToString(System.Globalization.CultureInfo.InvariantCulture),
            regionY.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return $"region/{regionX}_{regionY}/{ComputeHash(key)[..8]}";
    }

    private static bool SampleScoresAreBounded(WorldRegionClimateSample sample) =>
        IsScore01(sample.ElevationScore)
        && IsScore01(sample.MoistureScore)
        && IsScore01(sample.TemperatureScore)
        && IsScore01(sample.RuggednessScore)
        && IsScore01(sample.SettlementSuitabilityScore)
        && IsScore01(sample.RoadTravelCostScore);

    private static bool SummaryScoresAreBounded(WorldRegionClimateSummary summary) =>
        IsScore01(summary.AverageElevationScore)
        && IsScore01(summary.AverageMoistureScore)
        && IsScore01(summary.AverageTemperatureScore)
        && IsScore01(summary.AverageRuggednessScore)
        && IsScore01(summary.AverageSettlementSuitabilityScore)
        && IsScore01(summary.AverageRoadTravelCostScore)
        && IsScore01(summary.RoadSuitabilityScore);

    private static double Average(IReadOnlyList<WorldRegionClimateSample> samples, Func<WorldRegionClimateSample, double> selector) =>
        samples.Count == 0 ? 0.0 : samples.Average(selector);

    private static int FloorDiv(int value, int divisor) => (int)Math.Floor(value / (double)divisor);

    private static int PositiveModulo(int value, int divisor)
    {
        var result = value % divisor;
        return result < 0 ? result + divisor : result;
    }

    private static double ToScore01(int score0To10000) => score0To10000 / 10000.0;

    private static int ToScore10000(double score) => (int)Math.Round(Clamp01(score) * 10000.0, MidpointRounding.AwayFromZero);

    private static double Clamp01(double value) => Math.Clamp(value, 0.0, 1.0);

    private static double RoundScore(double value) => Math.Round(Clamp01(value), 4, MidpointRounding.AwayFromZero);

    private static bool IsScore01(double value) => value >= 0.0 && value <= 1.0;

    private static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.00001;

    private static string RenderReport(WorldBiomeNoiseCandidateReport report)
    {
        var lines = new List<string>
        {
            "# Candidate World Biome Noise Report",
            string.Empty,
            "- Candidate id: " + report.CandidateId,
            "- Contract id: " + report.ContractId,
            "- Final status: " + report.FinalStatus,
            "- Contract proof passed: " + report.ContractProofPassed.ToString().ToLowerInvariant(),
            "- FastNoise Lite decision: " + report.FastNoiseLiteDecision,
            "- FastNoise Lite dependency adopted: " + report.FastNoiseLiteDependencyAdopted.ToString().ToLowerInvariant(),
            "- Fallback decision: " + report.FallbackDecision,
            "- Adapter recommendation: " + report.AdapterRecommendation,
            "- Deterministic hash: " + report.DeterministicHash,
            string.Empty,
            "## Samples",
            string.Empty,
            "| X | Y | Elevation | Moisture | Temperature | Biome |",
            "| --- | --- | --- | --- | --- | --- |"
        };
        lines.AddRange(report.Samples.Select(sample =>
            $"| {sample.X} | {sample.Y} | {sample.ElevationScore0To10000} | {sample.MoistureScore0To10000} | {sample.TemperatureScore0To10000} | {sample.BiomeId} |"));
        lines.Add(string.Empty);
        lines.Add("This candidate does not claim an accepted gate or production integration.");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderRegionClimateReport(WorldRegionClimateCandidateReport report)
    {
        var lines = new List<string>
        {
            "# Candidate World Region Climate Report",
            string.Empty,
            "- Candidate id: " + report.CandidateId,
            "- Base candidate id: " + CandidateId,
            "- Contract id: " + report.ContractId,
            "- Final status: " + report.FinalStatus,
            "- Contract proof passed: " + report.ContractProofPassed.ToString().ToLowerInvariant(),
            "- Score sampler: " + report.ScoreSampler,
            "- Biome classifier: " + report.BiomeClassifier,
            "- Climate logic: " + report.ClimateLogic,
            "- Deterministic hash: " + report.DeterministicHash,
            "- Global map materialized: " + report.GlobalMapMaterialized.ToString().ToLowerInvariant(),
            "- Settlement generation implemented: " + report.SettlementGenerationImplemented.ToString().ToLowerInvariant(),
            "- Road generation implemented: " + report.RoadGenerationImplemented.ToString().ToLowerInvariant(),
            "- Faction generation implemented: " + report.FactionGenerationImplemented.ToString().ToLowerInvariant(),
            string.Empty,
            "## External Scouting Decisions",
            string.Empty
        };
        lines.AddRange(report.ExternalScoutingDecisions.Select(item => $"- {item.Name}: {item.Decision}; {item.Note}"));
        lines.AddRange(
        [
            string.Empty,
            "## Samples",
            string.Empty,
            "| X | Y | Elevation | Moisture | Temperature | Ruggedness | Climate | Biome | Region | Settlement | Road cost |",
            "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(report.Samples.Select(sample =>
            $"| {sample.X} | {sample.Y} | {FormatScore(sample.ElevationScore)} | {FormatScore(sample.MoistureScore)} | {FormatScore(sample.TemperatureScore)} | {FormatScore(sample.RuggednessScore)} | {sample.ClimateBand} | {sample.BiomeId} | {sample.RegionId} | {FormatScore(sample.SettlementSuitabilityScore)} | {FormatScore(sample.RoadTravelCostScore)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Region Summaries",
            string.Empty,
            "| Region | Dominant biome | Avg temp | Avg moisture | Settlement | Road suitability | Samples |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(report.RegionSummaries.Select(summary =>
            $"| {summary.RegionId} | {summary.DominantBiomeId} | {FormatScore(summary.AverageTemperatureScore)} | {FormatScore(summary.AverageMoistureScore)} | {FormatScore(summary.AverageSettlementSuitabilityScore)} | {FormatScore(summary.RoadSuitabilityScore)} | {summary.SampleCount} |"));
        lines.AddRange(
        [
            string.Empty,
            "This candidate keeps huge-world behavior coordinate-derived: samples and summaries are calculated from seed plus coordinate only, without mutable global RNG state or full-map materialization.",
            "Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library."
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderSettlementRoadSeedsReport(RegionSettlementRoadSeedsCandidateReport report)
    {
        var plan = report.Plan;
        var lines = new List<string>
        {
            "# Candidate Region Settlement Road Seeds Report",
            string.Empty,
            "- Candidate id: " + report.CandidateId,
            "- Base candidate id: " + report.BaseCandidateId,
            "- Contract id: " + report.ContractId,
            "- Final status: " + report.FinalStatus,
            "- Contract proof passed: " + report.ContractProofPassed.ToString().ToLowerInvariant(),
            "- Plan id: " + plan.PlanId,
            "- Region id: " + plan.RegionId,
            "- Region coordinate: " + plan.RegionX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + plan.RegionY.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "- Deterministic hash: " + report.DeterministicHash,
            "- Global map materialized: " + report.GlobalMapMaterialized.ToString().ToLowerInvariant(),
            "- Actual settlements generated: " + report.ActualSettlementsGenerated.ToString().ToLowerInvariant(),
            "- Road paths generated: " + report.RoadPathsGenerated.ToString().ToLowerInvariant(),
            "- Navigation/pathfinding implemented: " + report.NavigationPathfindingImplemented.ToString().ToLowerInvariant(),
            string.Empty,
            "## External Scouting Decisions",
            string.Empty
        };
        lines.AddRange(report.ExternalScoutingDecisions.Select(item => $"- {item.Name}: {item.Decision}; {item.Note}"));
        lines.AddRange(
        [
            string.Empty,
            "## Source Climate Summary",
            string.Empty,
            "| Region | Dominant biome | Avg temp | Avg moisture | Avg elevation | Avg ruggedness | Settlement | Road suitability |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |",
            "| "
                + plan.SourceClimateSummary.RegionId
                + " | "
                + plan.SourceClimateSummary.DominantBiomeId
                + " | "
                + FormatScore(plan.SourceClimateSummary.AverageTemperatureScore)
                + " | "
                + FormatScore(plan.SourceClimateSummary.AverageMoistureScore)
                + " | "
                + FormatScore(plan.SourceClimateSummary.AverageElevationScore)
                + " | "
                + FormatScore(plan.SourceClimateSummary.AverageRuggednessScore)
                + " | "
                + FormatScore(plan.SourceClimateSummary.AverageSettlementSuitabilityScore)
                + " | "
                + FormatScore(plan.SourceClimateSummary.RoadSuitabilityScore)
                + " |",
            string.Empty,
            "## Settlement Anchor Candidates",
            string.Empty,
            "| Anchor | Cell | Kind | Suitability | Climate | Biome | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.SettlementAnchors.Select(anchor =>
            $"| {anchor.AnchorId} | {anchor.CellX},{anchor.CellY} | {anchor.Kind} | {FormatScore(anchor.SuitabilityScore)} | {anchor.ClimateBand} | {anchor.BiomeId} | {string.Join(", ", anchor.Reasons)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Road Connection Hints",
            string.Empty,
            "| Hint | From | To | Kind | Cost | Priority | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.RoadHints.Select(hint =>
            $"| {hint.RoadHintId} | {hint.FromId} | {hint.ToId} | {hint.ConnectionKind} | {FormatScore(hint.EstimatedCostScore)} | {FormatScore(hint.PriorityScore)} | {string.Join(", ", hint.Reasons)} |"));
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
            "This candidate intentionally does not implement actual settlements, road paths, factions or navigation/pathfinding.",
            "Huge-world behavior remains coordinate-derived from seed plus region coordinate and options; no mutable global RNG or full-world map materialization is used.",
            "Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library."
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderGatewayConnectivityHintsReport(RegionGatewayConnectivityHintsCandidateReport report)
    {
        var plan = report.Plan;
        var lines = new List<string>
        {
            "# Candidate Region Gateway Connectivity Hints Report",
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
            "- Actual roads generated: " + report.ActualRoadsGenerated.ToString().ToLowerInvariant(),
            "- Navigation/pathfinding implemented: " + report.NavigationPathfindingImplemented.ToString().ToLowerInvariant(),
            "- Navigation graph generated: " + report.NavigationGraphGenerated.ToString().ToLowerInvariant(),
            "- Actual settlements generated: " + report.ActualSettlementsGenerated.ToString().ToLowerInvariant(),
            string.Empty,
            "## External Scouting Decisions",
            string.Empty
        };
        lines.AddRange(report.ExternalScoutingDecisions.Select(item => $"- {item.Name}: {item.Decision}; {item.Note}"));
        lines.AddRange(
        [
            string.Empty,
            "## Source Center Region Climate Summary",
            string.Empty,
            "| Region | Dominant biome | Avg temp | Avg moisture | Avg elevation | Avg ruggedness | Road suitability |",
            "| --- | --- | --- | --- | --- | --- | --- |",
            $"| {plan.SourceClimateSummary.RegionId} | {plan.SourceClimateSummary.DominantBiomeId} | {FormatScore(plan.SourceClimateSummary.AverageTemperatureScore)} | {FormatScore(plan.SourceClimateSummary.AverageMoistureScore)} | {FormatScore(plan.SourceClimateSummary.AverageElevationScore)} | {FormatScore(plan.SourceClimateSummary.AverageRuggednessScore)} | {FormatScore(plan.SourceClimateSummary.RoadSuitabilityScore)} |",
            string.Empty,
            "## Bounded Neighbor Region Summaries",
            string.Empty,
            "| Direction | Region | Dominant biome | Road suitability | Diagonal |",
            "| --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.NeighborRegionSummaries.Select(neighbor =>
            $"| {neighbor.Direction} | {neighbor.RegionId} | {neighbor.ClimateSummary.DominantBiomeId} | {FormatScore(neighbor.ClimateSummary.RoadSuitabilityScore)} | {neighbor.IsDiagonal.ToString().ToLowerInvariant()} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Gateway Candidates",
            string.Empty,
            "| Gateway | Neighbor | Direction | Cell | Kind | Suitability | Crossing cost | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.GatewayCandidates.Select(gateway =>
            $"| {gateway.GatewayId} | {gateway.NeighborRegionId} | {gateway.Direction} | {gateway.BoundaryCellX},{gateway.BoundaryCellY} | {gateway.GatewayKind} | {FormatScore(gateway.SuitabilityScore)} | {FormatScore(gateway.EstimatedCrossingCostScore)} | {string.Join(", ", gateway.Reasons)} |"));
        lines.AddRange(
        [
            string.Empty,
            "## Corridor Hints",
            string.Empty,
            "| Corridor | Canonical pair | From gateway | To neighbor | Kind | Cost | Priority | Reasons |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);
        lines.AddRange(plan.CorridorHints.Select(hint =>
            $"| {hint.CorridorHintId} | {hint.CanonicalRegionPairId} | {hint.FromGatewayId} | {hint.ToNeighborRegionId} | {hint.CorridorKind} | {FormatScore(hint.EstimatedCostScore)} | {FormatScore(hint.PriorityScore)} | {string.Join(", ", hint.Reasons)} |"));
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
            "Huge-world behavior remains coordinate-derived from seed plus center region and bounded options; no mutable global RNG or full-world map materialization is used.",
            "Canonical region-pair ids are sorted by region coordinates so shared edge/pair identity is stable regardless of planning direction.",
            "This candidate intentionally does not implement pathfinding, actual roads, navigation graph, factions, actual settlements, GamePackage data, Unity/runtime/provider/LLM/RAG/media/Lua or generator-library behavior.",
            "Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, context index, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library."
        ]);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<WorldBiomeNoiseDiagnostic> SortDiagnostics(IEnumerable<WorldBiomeNoiseDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static WorldBiomeNoiseDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

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

    private static readonly HashSet<string> KnownCoordinateSpaces = new(StringComparer.Ordinal)
    {
        "world_cell",
        "chunk_cell",
        "region_anchor"
    };

    private static readonly IReadOnlyList<WorldRegionClimateCoordinate> DefaultRegionClimateCoordinates =
    [
        new WorldRegionClimateCoordinate(0, 0),
        new WorldRegionClimateCoordinate(7, 3),
        new WorldRegionClimateCoordinate(16, -4),
        new WorldRegionClimateCoordinate(-9, 11),
        new WorldRegionClimateCoordinate(32, 32),
        new WorldRegionClimateCoordinate(128, -96)
    ];

    private sealed record RegionDirectionDescriptor(
        string Direction,
        string Side,
        int DeltaX,
        int DeltaY,
        bool IsDiagonal);
}

public sealed record WorldBiomeNoiseCandidateOptions
{
    public string Seed { get; init; } = "candidate/world-biome-noise/default-seed";
    public string RulesVersion { get; init; } = "world_biome_noise_rules_v1";
    public string CoordinateSpace { get; init; } = "world_cell";
}

public sealed record WorldRegionClimateCandidateOptions
{
    public string Seed { get; init; } = "candidate/world-region-climate/default-seed";
    public string RulesVersion { get; init; } = "world_region_climate_rules_v1";
    public string CoordinateSpace { get; init; } = "world_cell";
    public int RegionSize { get; init; } = 16;
    public int LatitudeBandPeriod { get; init; } = 128;
    public IReadOnlyList<WorldRegionClimateCoordinate> SampleCoordinates { get; init; } = [];
}

public sealed record RegionSettlementRoadSeedsCandidateOptions
{
    public string Seed { get; init; } = "candidate/region-settlement-road-seeds/default-seed";
    public string RulesVersion { get; init; } = "region_settlement_road_seeds_rules_v1";
    public string CoordinateSpace { get; init; } = "world_cell";
    public int RegionSize { get; init; } = 16;
    public int LatitudeBandPeriod { get; init; } = 128;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public int MaxSettlementAnchors { get; init; } = 5;
    public int MaxRoadHints { get; init; } = 6;
}

public sealed record RegionGatewayConnectivityHintsCandidateOptions
{
    public string Seed { get; init; } = "candidate/region-gateway-connectivity-hints/default-seed";
    public string RulesVersion { get; init; } = "region_gateway_connectivity_hints_rules_v1";
    public string CoordinateSpace { get; init; } = "world_cell";
    public int RegionSize { get; init; } = 16;
    public int LatitudeBandPeriod { get; init; } = 128;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public bool IncludeDiagonals { get; init; }
    public int MaxGatewaysPerSide { get; init; } = 2;
    public int MaxCorridorHints { get; init; } = 6;
    public int MaxNeighborRegions { get; init; }
}

public sealed record WorldBiomeNoiseCandidateResult
{
    public WorldBiomeNoiseCandidateReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record WorldBiomeNoiseCandidateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record WorldRegionClimateCandidateResult
{
    public WorldRegionClimateCandidateReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record WorldRegionClimateCandidateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record RegionSettlementRoadSeedsCandidateResult
{
    public RegionSettlementRoadSeedsCandidateReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record RegionSettlementRoadSeedsCandidateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record RegionGatewayConnectivityHintsCandidateResult
{
    public RegionGatewayConnectivityHintsCandidateReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record RegionGatewayConnectivityHintsCandidateWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record WorldBiomeNoiseCandidateReport
{
    public string CandidateId { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string FinalStatus { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool AcceptedGateClaimed { get; init; }
    public bool FastNoiseLiteDependencyAdopted { get; init; }
    public string FastNoiseLiteDecision { get; init; } = string.Empty;
    public string FallbackDecision { get; init; } = string.Empty;
    public string AdapterRecommendation { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public string CoordinateSpace { get; init; } = string.Empty;
    public string NormalizationVersion { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public IReadOnlyList<WorldBiomeSample> Samples { get; init; } = [];
    public WorldBiomeClassifierBoundaryProof ClassifierBoundaryProof { get; init; } = new();
    public bool SameSeedStable { get; init; }
    public bool DifferentSeedVariationVisible { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool RuntimeProviderNetworkDependency { get; init; }
    public WorldBiomeNoiseExternalExecutionFlags ExternalExecution { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldBiomeNoiseDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldBiomeSample
{
    public int X { get; init; }
    public int Y { get; init; }
    public int ElevationScore0To10000 { get; init; }
    public int MoistureScore0To10000 { get; init; }
    public int TemperatureScore0To10000 { get; init; }
    public string BiomeId { get; init; } = string.Empty;
}

public sealed record WorldRegionClimateCandidateReport
{
    public string CandidateId { get; init; } = string.Empty;
    public string ContractId { get; init; } = string.Empty;
    public string FinalStatus { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool AcceptedGateClaimed { get; init; }
    public string Seed { get; init; } = string.Empty;
    public string RulesVersion { get; init; } = string.Empty;
    public string CoordinateSpace { get; init; } = string.Empty;
    public int RegionSize { get; init; }
    public int LatitudeBandPeriod { get; init; }
    public string ScoreSampler { get; init; } = string.Empty;
    public string BiomeClassifier { get; init; } = string.Empty;
    public string ClimateLogic { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public IReadOnlyList<WorldRegionClimateSample> Samples { get; init; } = [];
    public int RegionSummaryCount { get; init; }
    public IReadOnlyList<WorldRegionClimateSummary> RegionSummaries { get; init; } = [];
    public bool SameSeedStable { get; init; }
    public bool DifferentSeedVariationVisible { get; init; }
    public bool OrderIndependent { get; init; }
    public bool GlobalMapMaterialized { get; init; }
    public IReadOnlyList<WorldRegionClimateExternalScoutingDecision> ExternalScoutingDecisions { get; init; } = [];
    public bool SettlementGenerationImplemented { get; init; }
    public bool RoadGenerationImplemented { get; init; }
    public bool FactionGenerationImplemented { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool RuntimeProviderNetworkDependency { get; init; }
    public WorldBiomeNoiseExternalExecutionFlags ExternalExecution { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldBiomeNoiseDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RegionSettlementRoadSeedsCandidateReport
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
    public int MaxSettlementAnchors { get; init; }
    public int MaxRoadHints { get; init; }
    public bool SameSeedStable { get; init; }
    public bool DifferentSeedVariationVisible { get; init; }
    public bool OrderIndependent { get; init; }
    public bool GlobalMapMaterialized { get; init; }
    public bool ActualSettlementsGenerated { get; init; }
    public bool RoadPathsGenerated { get; init; }
    public bool NavigationPathfindingImplemented { get; init; }
    public bool FactionGenerationImplemented { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool RuntimeProviderNetworkDependency { get; init; }
    public WorldBiomeNoiseExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<WorldRegionClimateExternalScoutingDecision> ExternalScoutingDecisions { get; init; } = [];
    public RegionSettlementRoadSeedPlan Plan { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldBiomeNoiseDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record RegionGatewayConnectivityHintsCandidateReport
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
    public int MaxGatewaysPerSide { get; init; }
    public int MaxCorridorHints { get; init; }
    public int MaxNeighborRegions { get; init; }
    public bool SameSeedStable { get; init; }
    public bool DifferentSeedVariationVisible { get; init; }
    public bool OrderIndependent { get; init; }
    public bool GlobalMapMaterialized { get; init; }
    public bool ActualRoadsGenerated { get; init; }
    public bool RoadPathsGenerated { get; init; }
    public bool NavigationPathfindingImplemented { get; init; }
    public bool NavigationGraphGenerated { get; init; }
    public bool ActualSettlementsGenerated { get; init; }
    public bool FactionGenerationImplemented { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool RuntimeProviderNetworkDependency { get; init; }
    public WorldBiomeNoiseExternalExecutionFlags ExternalExecution { get; init; } = new();
    public IReadOnlyList<WorldRegionClimateExternalScoutingDecision> ExternalScoutingDecisions { get; init; } = [];
    public RegionGatewayConnectivityPlan Plan { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldBiomeNoiseDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldRegionClimateCoordinate(int X, int Y);

public sealed record WorldRegionClimateSample
{
    public int X { get; init; }
    public int Y { get; init; }
    public double ElevationScore { get; init; }
    public double MoistureScore { get; init; }
    public double TemperatureScore { get; init; }
    public double RuggednessScore { get; init; }
    public string ClimateBand { get; init; } = string.Empty;
    public string BiomeId { get; init; } = string.Empty;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public double SettlementSuitabilityScore { get; init; }
    public double RoadTravelCostScore { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record WorldRegionClimateSummary
{
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public string DominantBiomeId { get; init; } = string.Empty;
    public double AverageElevationScore { get; init; }
    public double AverageMoistureScore { get; init; }
    public double AverageTemperatureScore { get; init; }
    public double AverageRuggednessScore { get; init; }
    public double AverageSettlementSuitabilityScore { get; init; }
    public double AverageRoadTravelCostScore { get; init; }
    public double RoadSuitabilityScore { get; init; }
    public int SampleCount { get; init; }
    public bool UsedGlobalMapMaterialization { get; init; }
    public IReadOnlyList<string> FutureTags { get; init; } = [];
}

public sealed record RegionSettlementRoadSeedPlan
{
    public string PlanId { get; init; } = string.Empty;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public WorldRegionClimateSummary SourceClimateSummary { get; init; } = new();
    public IReadOnlyList<RegionSettlementAnchorCandidate> SettlementAnchors { get; init; } = [];
    public IReadOnlyList<RegionRoadConnectionHint> RoadHints { get; init; } = [];
    public IReadOnlyList<string> SummaryTags { get; init; } = [];
}

public sealed record RegionSettlementAnchorCandidate
{
    public string AnchorId { get; init; } = string.Empty;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public int CellX { get; init; }
    public int CellY { get; init; }
    public string Kind { get; init; } = string.Empty;
    public double SuitabilityScore { get; init; }
    public double ElevationScore { get; init; }
    public double MoistureScore { get; init; }
    public double TemperatureScore { get; init; }
    public double RuggednessScore { get; init; }
    public string ClimateBand { get; init; } = string.Empty;
    public string BiomeId { get; init; } = string.Empty;
    public string SourceClimateRegionId { get; init; } = string.Empty;
    public int SourceClimateSampleIndex { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record RegionRoadConnectionHint
{
    public string RoadHintId { get; init; } = string.Empty;
    public string FromId { get; init; } = string.Empty;
    public string ToId { get; init; } = string.Empty;
    public string ConnectionKind { get; init; } = string.Empty;
    public double EstimatedCostScore { get; init; }
    public double PriorityScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record RegionGatewayConnectivityPlan
{
    public string PlanId { get; init; } = string.Empty;
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public WorldRegionClimateSummary SourceClimateSummary { get; init; } = new();
    public string SourceSettlementRoadSeedPlanId { get; init; } = string.Empty;
    public int SourceSettlementAnchorCount { get; init; }
    public int SourceRoadHintCount { get; init; }
    public IReadOnlyList<GatewayNeighborRegionSummary> NeighborRegionSummaries { get; init; } = [];
    public IReadOnlyList<RegionGatewayCandidate> GatewayCandidates { get; init; } = [];
    public IReadOnlyList<RegionCorridorHint> CorridorHints { get; init; } = [];
    public IReadOnlyList<string> SummaryTags { get; init; } = [];
}

public sealed record GatewayNeighborRegionSummary
{
    public int RegionX { get; init; }
    public int RegionY { get; init; }
    public string RegionId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string Side { get; init; } = string.Empty;
    public bool IsDiagonal { get; init; }
    public WorldRegionClimateSummary ClimateSummary { get; init; } = new();
}

public sealed record RegionGatewayCandidate
{
    public string GatewayId { get; init; } = string.Empty;
    public int CenterRegionX { get; init; }
    public int CenterRegionY { get; init; }
    public string CenterRegionId { get; init; } = string.Empty;
    public int NeighborRegionX { get; init; }
    public int NeighborRegionY { get; init; }
    public string NeighborRegionId { get; init; } = string.Empty;
    public string Side { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public bool IsDiagonal { get; init; }
    public int BoundaryCellX { get; init; }
    public int BoundaryCellY { get; init; }
    public int LocalCellX { get; init; }
    public int LocalCellY { get; init; }
    public string GatewayKind { get; init; } = string.Empty;
    public double SuitabilityScore { get; init; }
    public double EstimatedCrossingCostScore { get; init; }
    public string ClimateBand { get; init; } = string.Empty;
    public string BiomeId { get; init; } = string.Empty;
    public double ElevationScore { get; init; }
    public double MoistureScore { get; init; }
    public double TemperatureScore { get; init; }
    public double RuggednessScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record RegionCorridorHint
{
    public string CorridorHintId { get; init; } = string.Empty;
    public string CanonicalRegionPairId { get; init; } = string.Empty;
    public string FromGatewayId { get; init; } = string.Empty;
    public string ToNeighborRegionId { get; init; } = string.Empty;
    public string ToGatewayId { get; init; } = string.Empty;
    public string CorridorKind { get; init; } = string.Empty;
    public double EstimatedCostScore { get; init; }
    public double PriorityScore { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record WorldRegionClimateExternalScoutingDecision(
    string Name,
    string Decision,
    string Note);

public sealed record WorldBiomeClassifierBoundaryProof
{
    public bool Passed { get; init; }
    public IReadOnlyList<WorldBiomeClassifierCase> Cases { get; init; } = [];
}

public sealed record WorldBiomeClassifierCase(
    string ScenarioId,
    int ElevationScore0To10000,
    int MoistureScore0To10000,
    string ExpectedBiomeId)
{
    public string ActualBiomeId { get; init; } = string.Empty;
}

public sealed record WorldBiomeNoiseExternalExecutionFlags
{
    public bool LlmExecuted { get; init; }
    public bool RagExecuted { get; init; }
    public bool ProviderExecuted { get; init; }
    public bool MediaExecuted { get; init; }
    public bool NetworkExecuted { get; init; }
    public bool LuaExecuted { get; init; }
    public bool UnityExecuted { get; init; }

    public bool AllFalse => !LlmExecuted && !RagExecuted && !ProviderExecuted && !MediaExecuted && !NetworkExecuted && !LuaExecuted && !UnityExecuted;
}

public sealed record WorldBiomeNoiseDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

internal readonly record struct WorldBiomeCoordinate(int X, int Y);
