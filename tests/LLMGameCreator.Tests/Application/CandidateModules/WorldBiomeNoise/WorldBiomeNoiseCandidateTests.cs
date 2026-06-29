using System.Text.Json;
using LLMGameCreator.Application.Design.CandidateModules.WorldBiomeNoise;
using Xunit;

namespace LLMGameCreator.Tests.Application.CandidateModules.WorldBiomeNoise;

public sealed class WorldBiomeNoiseCandidateTests
{
    [Fact]
    public async Task BuildsDeterministicCandidateReportWithoutExternalDependency()
    {
        using var temp = new TempDirectory();
        var service = new WorldBiomeNoiseCandidateService();

        var first = service.Build();
        var second = service.Build();
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(WorldBiomeNoiseCandidateService.CandidateId, first.Report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, first.Report.FinalStatus);
        Assert.False(first.Report.AcceptedGateClaimed);
        Assert.False(first.Report.FastNoiseLiteDependencyAdopted);
        Assert.Equal("reference_only", first.Report.FastNoiseLiteDecision);
        Assert.Equal("adapt_behind_adapter", first.Report.FallbackDecision);
        Assert.True(first.Report.ExternalExecution.AllFalse);
        Assert.Equal(5, first.Report.SampleCount);
        Assert.True(first.Report.DifferentSeedVariationVisible);
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
    }

    [Fact]
    public void ClassifierBoundariesAreStable()
    {
        Assert.Equal("biome/water", WorldBiomeNoiseCandidateService.ClassifyBiome(2499, 10000));
        Assert.Equal("biome/plains", WorldBiomeNoiseCandidateService.ClassifyBiome(2500, 3000));
        Assert.Equal("biome/desert", WorldBiomeNoiseCandidateService.ClassifyBiome(5000, 2999));
        Assert.Equal("biome/forest", WorldBiomeNoiseCandidateService.ClassifyBiome(5000, 6500));
        Assert.Equal("biome/alpine", WorldBiomeNoiseCandidateService.ClassifyBiome(7500, 0));
    }

    [Fact]
    public void RejectsMissingSeedAndUnknownCoordinateSpace()
    {
        var result = new WorldBiomeNoiseCandidateService().Build(new WorldBiomeNoiseCandidateOptions
        {
            Seed = " ",
            CoordinateSpace = "unknown_space"
        });

        Assert.False(result.Report.ContractProofPassed);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "world_biome_noise.seed.missing");
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "world_biome_noise.coordinate_space.unknown");
    }

    [Fact]
    public async Task WrittenReportRoundTripsCandidateStatus()
    {
        using var temp = new TempDirectory();
        var service = new WorldBiomeNoiseCandidateService();
        var write = await service.BuildAndWriteAsync(temp.Path);

        var report = JsonSerializer.Deserialize<WorldBiomeNoiseCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(WorldBiomeNoiseCandidateService.CandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed);
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public void RegionClimateSamplesAreDeterministicBoundedAndOrderIndependent()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var options = new WorldRegionClimateCandidateOptions
        {
            Seed = "candidate-region-climate-test",
            SampleCoordinates =
            [
                new WorldRegionClimateCoordinate(12, -5),
                new WorldRegionClimateCoordinate(-3, 20),
                new WorldRegionClimateCoordinate(64, 64)
            ]
        };
        var coordinates = options.SampleCoordinates;

        var first = service.SampleRegionClimate(options, coordinates[0]);
        var second = service.SampleRegionClimate(options, coordinates[0]);
        var forward = coordinates.Select(coordinate => service.SampleRegionClimate(options, coordinate)).ToList();
        var reverse = coordinates.Reverse().Select(coordinate => service.SampleRegionClimate(options, coordinate)).OrderBy(item => item.X).ThenBy(item => item.Y).ToList();
        var sortedForward = forward.OrderBy(item => item.X).ThenBy(item => item.Y).ToList();

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.Equal(JsonSerializer.Serialize(sortedForward), JsonSerializer.Serialize(reverse));
        Assert.All(forward, sample =>
        {
            Assert.InRange(sample.ElevationScore, 0.0, 1.0);
            Assert.InRange(sample.MoistureScore, 0.0, 1.0);
            Assert.InRange(sample.TemperatureScore, 0.0, 1.0);
            Assert.InRange(sample.RuggednessScore, 0.0, 1.0);
            Assert.InRange(sample.SettlementSuitabilityScore, 0.0, 1.0);
            Assert.InRange(sample.RoadTravelCostScore, 0.0, 1.0);
            Assert.StartsWith("biome/", sample.BiomeId, StringComparison.Ordinal);
            Assert.NotEmpty(sample.ClimateBand);
            Assert.False(string.IsNullOrWhiteSpace(sample.RegionId));
        });
    }

    [Fact]
    public void RegionClimateDifferentSeedChangesMeaningfulFieldAndKeepsNoGenerationClaims()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var coordinate = new WorldRegionClimateCoordinate(9, 14);
        var first = service.SampleRegionClimate(new WorldRegionClimateCandidateOptions { Seed = "region-climate-a" }, coordinate);
        var second = service.SampleRegionClimate(new WorldRegionClimateCandidateOptions { Seed = "region-climate-b" }, coordinate);
        var report = service.BuildRegionClimate();

        Assert.True(
            first.ElevationScore != second.ElevationScore
            || first.MoistureScore != second.MoistureScore
            || first.TemperatureScore != second.TemperatureScore
            || first.RuggednessScore != second.RuggednessScore
            || !string.Equals(first.BiomeId, second.BiomeId, StringComparison.Ordinal)
            || !string.Equals(first.ClimateBand, second.ClimateBand, StringComparison.Ordinal));
        Assert.Equal(WorldBiomeNoiseCandidateService.RegionClimateCandidateId, report.Report.CandidateId);
        Assert.True(report.Report.ContractProofPassed, string.Join(Environment.NewLine, report.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.Report.SettlementGenerationImplemented);
        Assert.False(report.Report.RoadGenerationImplemented);
        Assert.False(report.Report.FactionGenerationImplemented);
        Assert.False(report.Report.PublicGamePackageSchemaChanged);
        Assert.All(report.Report.ExternalScoutingDecisions, item => Assert.Equal("reference_only", item.Decision));
    }

    [Fact]
    public void RegionClimateSummaryIsDeterministicBoundedAndDoesNotMaterializeWorldMap()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var options = new WorldRegionClimateCandidateOptions { Seed = "region-summary-test", RegionSize = 16 };

        var first = service.SummarizeRegionClimate(options, 2, -1);
        var second = service.SummarizeRegionClimate(options, 2, -1);

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.False(first.UsedGlobalMapMaterialization);
        Assert.Equal(9, first.SampleCount);
        Assert.StartsWith("region/2_-1/", first.RegionId, StringComparison.Ordinal);
        Assert.StartsWith("biome/", first.DominantBiomeId, StringComparison.Ordinal);
        Assert.InRange(first.AverageElevationScore, 0.0, 1.0);
        Assert.InRange(first.AverageMoistureScore, 0.0, 1.0);
        Assert.InRange(first.AverageTemperatureScore, 0.0, 1.0);
        Assert.InRange(first.AverageRuggednessScore, 0.0, 1.0);
        Assert.InRange(first.AverageSettlementSuitabilityScore, 0.0, 1.0);
        Assert.InRange(first.AverageRoadTravelCostScore, 0.0, 1.0);
        Assert.InRange(first.RoadSuitabilityScore, 0.0, 1.0);
        Assert.Contains(first.FutureTags, item => item.Contains("settlement", StringComparison.Ordinal));
        Assert.Contains(first.FutureTags, item => item.StartsWith("future_road_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WrittenRegionClimateReportRoundTripsCandidateStatus()
    {
        using var temp = new TempDirectory();
        var service = new WorldBiomeNoiseCandidateService();
        var first = service.BuildRegionClimate();
        var second = service.BuildRegionClimate();
        var write = await service.WriteRegionClimateAsync(temp.Path, first);

        var report = JsonSerializer.Deserialize<WorldRegionClimateCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(WorldBiomeNoiseCandidateService.RegionClimateCandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed);
        Assert.False(report.GlobalMapMaterialized);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.Contains("H3", await File.ReadAllTextAsync(write.ReportMarkdownPath), StringComparison.Ordinal);
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public void SettlementRoadSeedsAreDeterministicBoundedSortedAndOrderIndependent()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var options = new RegionSettlementRoadSeedsCandidateOptions
        {
            Seed = "settlement-road-seeds-test",
            RegionX = -3,
            RegionY = 5,
            MaxSettlementAnchors = 5,
            MaxRoadHints = 6
        };
        var otherRegion = options with { RegionX = 8, RegionY = -2 };

        var first = service.BuildSettlementRoadSeeds(options);
        _ = service.BuildSettlementRoadSeeds(otherRegion);
        var otherFirst = service.BuildSettlementRoadSeeds(otherRegion);
        var second = service.BuildSettlementRoadSeeds(options);

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(JsonSerializer.Serialize(first.Report.Plan), JsonSerializer.Serialize(second.Report.Plan));
        Assert.Equal(JsonSerializer.Serialize(otherFirst.Report.Plan), JsonSerializer.Serialize(service.BuildSettlementRoadSeeds(otherRegion).Report.Plan));
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(WorldBiomeNoiseCandidateService.SettlementRoadSeedsCandidateId, first.Report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.RegionClimateCandidateId, first.Report.BaseCandidateId);
        Assert.False(first.Report.GlobalMapMaterialized);
        Assert.False(first.Report.ActualSettlementsGenerated);
        Assert.False(first.Report.RoadPathsGenerated);
        Assert.False(first.Report.NavigationPathfindingImplemented);
        Assert.True(first.Report.ExternalExecution.AllFalse);

        var anchors = first.Report.Plan.SettlementAnchors;
        var sortedAnchorIds = anchors
            .OrderByDescending(item => item.SuitabilityScore)
            .ThenBy(item => item.AnchorId, StringComparer.Ordinal)
            .Select(item => item.AnchorId)
            .ToList();
        Assert.Equal(sortedAnchorIds, anchors.Select(item => item.AnchorId).ToList());
        Assert.All(anchors, anchor =>
        {
            Assert.InRange(anchor.SuitabilityScore, 0.0, 1.0);
            Assert.InRange(anchor.ElevationScore, 0.0, 1.0);
            Assert.InRange(anchor.MoistureScore, 0.0, 1.0);
            Assert.InRange(anchor.TemperatureScore, 0.0, 1.0);
            Assert.InRange(anchor.RuggednessScore, 0.0, 1.0);
            Assert.StartsWith("settlement-anchor/", anchor.AnchorId, StringComparison.Ordinal);
            Assert.Contains(anchor.Kind, new[] { "CapitalCandidate", "TownCandidate", "VillageCandidate", "OutpostCandidate" });
            Assert.NotEmpty(anchor.Reasons);
            Assert.NotEmpty(anchor.Tags);
        });
        Assert.All(first.Report.Plan.RoadHints, hint =>
        {
            Assert.InRange(hint.EstimatedCostScore, 0.0, 1.0);
            Assert.InRange(hint.PriorityScore, 0.0, 1.0);
            Assert.StartsWith("road-hint/", hint.RoadHintId, StringComparison.Ordinal);
            Assert.Contains(hint.ConnectionKind, new[] { "InternalRegionLink", "RegionalGatewayLink", "TradeRouteHint" });
            Assert.DoesNotContain("polyline", JsonSerializer.Serialize(hint), StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void SettlementRoadSeedsDifferentSeedChangesPlanAndSupportsLargeCoordinates()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var options = new RegionSettlementRoadSeedsCandidateOptions
        {
            Seed = "region-settlement-seed-a",
            RegionX = -1234567,
            RegionY = 987654,
            MaxSettlementAnchors = 4,
            MaxRoadHints = 3
        };

        var first = service.BuildSettlementRoadSeeds(options);
        var second = service.BuildSettlementRoadSeeds(options with { Seed = "region-settlement-seed-b" });

        Assert.NotEqual(first.Report.Plan.PlanId, second.Report.Plan.PlanId);
        Assert.NotEqual(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(options.RegionX, first.Report.Plan.RegionX);
        Assert.Equal(options.RegionY, first.Report.Plan.RegionY);
        Assert.StartsWith($"region/{options.RegionX}_{options.RegionY}/", first.Report.Plan.RegionId, StringComparison.Ordinal);
        Assert.True(first.Report.DifferentSeedVariationVisible);
        Assert.DoesNotContain(first.Report.Diagnostics, item => item.Severity == "error");
        Assert.All(first.Report.ExternalScoutingDecisions, item => Assert.Equal("reference_only", item.Decision));
    }

    [Fact]
    public void SettlementRoadSeedsRespectLimitsAndReferenceOnlyKnownAnchors()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var result = service.BuildSettlementRoadSeeds(new RegionSettlementRoadSeedsCandidateOptions
        {
            Seed = "settlement-road-limit-test",
            RegionX = 2,
            RegionY = -4,
            MaxSettlementAnchors = 3,
            MaxRoadHints = 2
        });
        var anchors = result.Report.Plan.SettlementAnchors.Select(item => item.AnchorId).ToHashSet(StringComparer.Ordinal);
        var pairs = new HashSet<string>(StringComparer.Ordinal);

        Assert.True(result.Report.ContractProofPassed, string.Join(Environment.NewLine, result.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.InRange(result.Report.Plan.SettlementAnchors.Count, 1, 3);
        Assert.InRange(result.Report.Plan.RoadHints.Count, 0, 2);
        Assert.Equal(result.Report.Plan.SettlementAnchors.Count, anchors.Count);
        Assert.Equal(result.Report.Plan.RoadHints.Count, result.Report.Plan.RoadHints.Select(item => item.RoadHintId).Distinct(StringComparer.Ordinal).Count());
        foreach (var hint in result.Report.Plan.RoadHints)
        {
            Assert.Contains(hint.FromId, anchors);
            Assert.Contains(hint.ToId, anchors);
            Assert.NotEqual(hint.FromId, hint.ToId);
            var key = string.CompareOrdinal(hint.FromId, hint.ToId) <= 0
                ? hint.FromId + "|" + hint.ToId
                : hint.ToId + "|" + hint.FromId;
            Assert.True(pairs.Add(key));
        }
    }

    [Fact]
    public async Task WrittenSettlementRoadSeedsReportRoundTripsCandidateStatus()
    {
        using var temp = new TempDirectory();
        var service = new WorldBiomeNoiseCandidateService();
        var first = service.BuildSettlementRoadSeeds();
        var second = service.BuildSettlementRoadSeeds();
        var write = await service.WriteSettlementRoadSeedsAsync(temp.Path, first);

        var report = JsonSerializer.Deserialize<RegionSettlementRoadSeedsCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(WorldBiomeNoiseCandidateService.SettlementRoadSeedsCandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.Contains("reference_only", await File.ReadAllTextAsync(write.ReportMarkdownPath), StringComparison.Ordinal);
        Assert.Contains("does not implement actual settlements", await File.ReadAllTextAsync(write.ReportMarkdownPath), StringComparison.Ordinal);
    }

    [Fact]
    public void GatewayConnectivityHintsAreDeterministicBoundedUniqueAndOrderIndependent()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var options = new RegionGatewayConnectivityHintsCandidateOptions
        {
            Seed = "gateway-connectivity-test",
            RegionX = -3,
            RegionY = 5,
            IncludeDiagonals = true,
            MaxGatewaysPerSide = 2,
            MaxCorridorHints = 5
        };
        var otherRegion = options with { RegionX = 8, RegionY = -2 };

        var first = service.BuildGatewayConnectivityHints(options);
        _ = service.BuildGatewayConnectivityHints(otherRegion);
        var otherFirst = service.BuildGatewayConnectivityHints(otherRegion);
        var second = service.BuildGatewayConnectivityHints(options);

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(JsonSerializer.Serialize(first.Report.Plan), JsonSerializer.Serialize(second.Report.Plan));
        Assert.Equal(JsonSerializer.Serialize(otherFirst.Report.Plan), JsonSerializer.Serialize(service.BuildGatewayConnectivityHints(otherRegion).Report.Plan));
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(WorldBiomeNoiseCandidateService.GatewayConnectivityHintsCandidateId, first.Report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.SettlementRoadSeedsCandidateId, first.Report.BaseCandidateId);
        Assert.False(first.Report.GlobalMapMaterialized);
        Assert.False(first.Report.ActualRoadsGenerated);
        Assert.False(first.Report.RoadPathsGenerated);
        Assert.False(first.Report.NavigationPathfindingImplemented);
        Assert.False(first.Report.NavigationGraphGenerated);
        Assert.False(first.Report.ActualSettlementsGenerated);
        Assert.False(first.Report.FactionGenerationImplemented);
        Assert.True(first.Report.ExternalExecution.AllFalse);

        var gateways = first.Report.Plan.GatewayCandidates;
        var gatewayIds = gateways.Select(item => item.GatewayId).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(gateways.Count, gatewayIds.Count);
        Assert.All(gateways, gateway =>
        {
            Assert.InRange(gateway.SuitabilityScore, 0.0, 1.0);
            Assert.InRange(gateway.EstimatedCrossingCostScore, 0.0, 1.0);
            Assert.InRange(gateway.ElevationScore, 0.0, 1.0);
            Assert.InRange(gateway.MoistureScore, 0.0, 1.0);
            Assert.InRange(gateway.TemperatureScore, 0.0, 1.0);
            Assert.InRange(gateway.RuggednessScore, 0.0, 1.0);
            Assert.StartsWith("gateway/", gateway.GatewayId, StringComparison.Ordinal);
            Assert.Contains(gateway.Direction, new[] { "North", "East", "South", "West", "NorthEast", "SouthEast", "SouthWest", "NorthWest" });
            Assert.Contains(gateway.GatewayKind, new[] { "TradePassCandidate", "ValleyPassCandidate", "CoastalCrossingCandidate", "WildernessTrailCandidate", "MountainPassCandidate" });
            Assert.NotEmpty(gateway.Reasons);
            Assert.NotEmpty(gateway.Tags);
        });

        var neighborIds = first.Report.Plan.NeighborRegionSummaries.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var corridorPairs = new HashSet<string>(StringComparer.Ordinal);
        Assert.Equal(first.Report.Plan.CorridorHints.Count, first.Report.Plan.CorridorHints.Select(item => item.CorridorHintId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(first.Report.Plan.CorridorHints, hint =>
        {
            Assert.InRange(hint.EstimatedCostScore, 0.0, 1.0);
            Assert.InRange(hint.PriorityScore, 0.0, 1.0);
            Assert.StartsWith("corridor-hint/", hint.CorridorHintId, StringComparison.Ordinal);
            Assert.StartsWith("region-pair/", hint.CanonicalRegionPairId, StringComparison.Ordinal);
            Assert.Contains(hint.FromGatewayId, gatewayIds);
            Assert.Contains(hint.ToNeighborRegionId, neighborIds);
            Assert.Contains(hint.CorridorKind, new[] { "RegionalTradeHint", "SettlementConnectorHint", "WildernessConnectorHint" });
            Assert.True(corridorPairs.Add(hint.CanonicalRegionPairId));
            Assert.DoesNotContain("polyline", JsonSerializer.Serialize(hint), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("pathfinding", JsonSerializer.Serialize(hint), StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void GatewayConnectivityHintsRespectLimitsNeighborModeAndLargeCoordinates()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var cardinal = service.BuildGatewayConnectivityHints(new RegionGatewayConnectivityHintsCandidateOptions
        {
            Seed = "gateway-limit-cardinal",
            RegionX = -1234567,
            RegionY = 987654,
            IncludeDiagonals = false,
            MaxGatewaysPerSide = 1,
            MaxCorridorHints = 3
        });
        var diagonal = service.BuildGatewayConnectivityHints(new RegionGatewayConnectivityHintsCandidateOptions
        {
            Seed = "gateway-limit-diagonal",
            RegionX = 2,
            RegionY = -4,
            IncludeDiagonals = true,
            MaxGatewaysPerSide = 1,
            MaxCorridorHints = 8,
            MaxNeighborRegions = 8
        });

        Assert.True(cardinal.Report.ContractProofPassed, string.Join(Environment.NewLine, cardinal.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(-1234567, cardinal.Report.Plan.RegionX);
        Assert.Equal(987654, cardinal.Report.Plan.RegionY);
        Assert.Equal(4, cardinal.Report.Plan.NeighborRegionSummaries.Count);
        Assert.DoesNotContain(cardinal.Report.Plan.NeighborRegionSummaries, item => item.IsDiagonal);
        Assert.All(cardinal.Report.Plan.GatewayCandidates.GroupBy(item => item.Direction), group => Assert.True(group.Count() <= 1));
        Assert.True(cardinal.Report.Plan.CorridorHints.Count <= 3);
        Assert.StartsWith("region/-1234567_987654/", cardinal.Report.Plan.RegionId, StringComparison.Ordinal);

        Assert.True(diagonal.Report.ContractProofPassed, string.Join(Environment.NewLine, diagonal.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(8, diagonal.Report.Plan.NeighborRegionSummaries.Count);
        Assert.Contains(diagonal.Report.Plan.NeighborRegionSummaries, item => item.IsDiagonal);
        Assert.Contains(diagonal.Report.Plan.GatewayCandidates, item => item.IsDiagonal);
        Assert.True(diagonal.Report.Plan.CorridorHints.Count <= 8);
    }

    [Fact]
    public void GatewayConnectivityHintsDifferentSeedChangesPlan()
    {
        var service = new WorldBiomeNoiseCandidateService();
        var options = new RegionGatewayConnectivityHintsCandidateOptions
        {
            Seed = "gateway-seed-a",
            RegionX = 4,
            RegionY = -7,
            IncludeDiagonals = true,
            MaxGatewaysPerSide = 2,
            MaxCorridorHints = 6
        };

        var first = service.BuildGatewayConnectivityHints(options);
        var second = service.BuildGatewayConnectivityHints(options with { Seed = "gateway-seed-b" });

        Assert.NotEqual(first.Report.Plan.PlanId, second.Report.Plan.PlanId);
        Assert.NotEqual(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.DifferentSeedVariationVisible);
        Assert.DoesNotContain(first.Report.Diagnostics, item => item.Severity == "error");
        Assert.All(first.Report.ExternalScoutingDecisions, item => Assert.Equal("reference_only", item.Decision));
    }

    [Fact]
    public async Task GatewayConnectivityHintsCanonicalPairIdAndWrittenReportAreStable()
    {
        using var temp = new TempDirectory();
        var service = new WorldBiomeNoiseCandidateService();
        var aToB = service.BuildGatewayConnectivityHints(new RegionGatewayConnectivityHintsCandidateOptions
        {
            Seed = "gateway-canonical-pair",
            RegionX = 0,
            RegionY = 0,
            IncludeDiagonals = false,
            MaxGatewaysPerSide = 1,
            MaxCorridorHints = 4
        });
        var bToA = service.BuildGatewayConnectivityHints(new RegionGatewayConnectivityHintsCandidateOptions
        {
            Seed = "gateway-canonical-pair",
            RegionX = 1,
            RegionY = 0,
            IncludeDiagonals = false,
            MaxGatewaysPerSide = 1,
            MaxCorridorHints = 4
        });
        var write = await service.WriteGatewayConnectivityHintsAsync(temp.Path, aToB);

        var eastPair = aToB.Report.Plan.CorridorHints.Single(item => item.ToNeighborRegionId.Contains("region/1_0/", StringComparison.Ordinal)).CanonicalRegionPairId;
        var westPair = bToA.Report.Plan.CorridorHints.Single(item => item.ToNeighborRegionId.Contains("region/0_0/", StringComparison.Ordinal)).CanonicalRegionPairId;
        var report = JsonSerializer.Deserialize<RegionGatewayConnectivityHintsCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(eastPair, westPair);
        Assert.Equal(WorldBiomeNoiseCandidateService.GatewayConnectivityHintsCandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.Contains("reference_only", await File.ReadAllTextAsync(write.ReportMarkdownPath), StringComparison.Ordinal);
        Assert.Contains("does not implement pathfinding", await File.ReadAllTextAsync(write.ReportMarkdownPath), StringComparison.Ordinal);
    }

    [Fact]
    public void HydrologyWaterwayHintsAreDeterministicBoundedUniqueAndOrderIndependent()
    {
        var service = new RegionHydrologyWaterwayHintsCandidateService();
        var options = new RegionHydrologyWaterwayHintsCandidateOptions
        {
            Seed = "hydrology-waterway-test",
            RegionX = -3,
            RegionY = 5,
            IncludeDiagonals = true,
            MaxWaterSourceCandidates = 5,
            MaxWaterbodyCandidates = 4,
            MaxWaterwayHints = 6,
            MaxCrossingHints = 6
        };
        var otherRegion = options with { RegionX = 8, RegionY = -2 };

        var first = service.Build(options);
        _ = service.Build(otherRegion);
        var otherFirst = service.Build(otherRegion);
        var second = service.Build(options);

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(JsonSerializer.Serialize(first.Report.Plan), JsonSerializer.Serialize(second.Report.Plan));
        Assert.Equal(JsonSerializer.Serialize(otherFirst.Report.Plan), JsonSerializer.Serialize(service.Build(otherRegion).Report.Plan));
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(RegionHydrologyWaterwayHintsCandidateService.CandidateId, first.Report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.GatewayConnectivityHintsCandidateId, first.Report.BaseCandidateId);
        Assert.False(first.Report.GlobalMapMaterialized);
        Assert.False(first.Report.ActualRiversGenerated);
        Assert.False(first.Report.ActualWaterbodiesGenerated);
        Assert.False(first.Report.RiverPathsGenerated);
        Assert.False(first.Report.WaterwayPolylinesGenerated);
        Assert.False(first.Report.ErosionSimulationImplemented);
        Assert.False(first.Report.RainfallSimulationImplemented);
        Assert.False(first.Report.FloodSimulationImplemented);
        Assert.False(first.Report.PathfindingNavigationImplemented);
        Assert.False(first.Report.NavigationGraphGenerated);
        Assert.False(first.Report.ActualSettlementsGenerated);
        Assert.False(first.Report.FactionGenerationImplemented);
        Assert.True(first.Report.ExternalExecution.AllFalse);

        Assert.StartsWith("basin/local/", first.Report.Plan.Drainage.BasinId, StringComparison.Ordinal);
        Assert.StartsWith("local-basin-code/", first.Report.Plan.Drainage.BasinCode, StringComparison.Ordinal);
        Assert.InRange(first.Report.Plan.Drainage.RunoffPotentialScore, 0.0, 1.0);
        Assert.InRange(first.Report.Plan.Drainage.AccumulationPotentialScore, 0.0, 1.0);
        Assert.InRange(first.Report.Plan.Drainage.FloodplainPotentialScore, 0.0, 1.0);
        Assert.InRange(first.Report.Plan.Drainage.AridityScore, 0.0, 1.0);

        var sourceIds = first.Report.Plan.WaterSourceCandidates.Select(item => item.WaterSourceCandidateId).ToHashSet(StringComparer.Ordinal);
        var waterbodyIds = first.Report.Plan.WaterbodyCandidates.Select(item => item.WaterbodyCandidateId).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(first.Report.Plan.WaterSourceCandidates.Count, sourceIds.Count);
        Assert.Equal(first.Report.Plan.WaterbodyCandidates.Count, waterbodyIds.Count);
        Assert.All(first.Report.Plan.WaterSourceCandidates, source =>
        {
            Assert.StartsWith("water-source/", source.WaterSourceCandidateId, StringComparison.Ordinal);
            Assert.Contains(source.Kind, new[] { "SpringCandidate", "RainfedHeadwaterCandidate", "SnowmeltCandidate", "WetlandSourceCandidate", "OasisCandidate" });
            Assert.InRange(source.FlowPotentialScore, 0.0, 1.0);
            Assert.InRange(source.SeasonalReliabilityScore, 0.0, 1.0);
            Assert.NotEmpty(source.Reasons);
            Assert.NotEmpty(source.Tags);
        });
        Assert.All(first.Report.Plan.WaterbodyCandidates, waterbody =>
        {
            Assert.StartsWith("waterbody/", waterbody.WaterbodyCandidateId, StringComparison.Ordinal);
            Assert.Contains(waterbody.Kind, new[] { "LakeCandidate", "WetlandCandidate", "MarshCandidate", "OasisCandidate", "FloodplainCandidate" });
            Assert.InRange(waterbody.RetentionScore, 0.0, 1.0);
            Assert.InRange(waterbody.WaterAvailabilityScore, 0.0, 1.0);
            Assert.InRange(waterbody.SettlementSupportScore, 0.0, 1.0);
            Assert.InRange(waterbody.RoadObstacleScore, 0.0, 1.0);
        });

        var neighborIds = first.Report.Plan.NeighborRegionSummaries.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var waterwayIds = first.Report.Plan.WaterwayCorridorHints.Select(item => item.WaterwayHintId).ToHashSet(StringComparer.Ordinal);
        Assert.Equal(first.Report.Plan.WaterwayCorridorHints.Count, waterwayIds.Count);
        Assert.All(first.Report.Plan.WaterwayCorridorHints, hint =>
        {
            Assert.StartsWith("waterway-hint/", hint.WaterwayHintId, StringComparison.Ordinal);
            Assert.True(sourceIds.Contains(hint.FromCandidateId) || waterbodyIds.Contains(hint.FromCandidateId));
            Assert.True(hint.ToTargetId == first.Report.Plan.Drainage.BasinId || neighborIds.Contains(hint.ToTargetId) || hint.ToTargetKind == "Gateway");
            Assert.Contains(hint.Kind, new[] { "HeadwaterStreamHint", "MinorRiverHint", "SeasonalWashHint", "WetlandDrainageHint", "CoastalOutletHint" });
            Assert.InRange(hint.EstimatedFlowScore, 0.0, 1.0);
            Assert.InRange(hint.PersistenceScore, 0.0, 1.0);
            Assert.InRange(hint.ErosionRiskScore, 0.0, 1.0);
            Assert.InRange(hint.SettlementSupportScore, 0.0, 1.0);
            Assert.InRange(hint.RoadCrossingPressureScore, 0.0, 1.0);
            Assert.DoesNotContain("polyline", JsonSerializer.Serialize(hint), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("pathfinding", JsonSerializer.Serialize(hint), StringComparison.OrdinalIgnoreCase);
        });

        Assert.Equal(first.Report.Plan.CrossingPressureHints.Count, first.Report.Plan.CrossingPressureHints.Select(item => item.CrossingHintId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(first.Report.Plan.CrossingPressureHints, hint =>
        {
            Assert.StartsWith("crossing-hint/", hint.CrossingHintId, StringComparison.Ordinal);
            Assert.Contains(hint.RelatedWaterwayHintId, waterwayIds);
            Assert.InRange(hint.CrossingNeedScore, 0.0, 1.0);
            Assert.InRange(hint.BridgePressureScore, 0.0, 1.0);
            Assert.InRange(hint.FerryOrFordPressureScore, 0.0, 1.0);
        });
    }

    [Fact]
    public void HydrologyWaterwayHintsRespectLimitsNeighborModesAndLargeCoordinates()
    {
        var service = new RegionHydrologyWaterwayHintsCandidateService();
        var cardinal = service.Build(new RegionHydrologyWaterwayHintsCandidateOptions
        {
            Seed = "hydrology-limit-cardinal",
            RegionX = -1234567,
            RegionY = 987654,
            IncludeDiagonals = false,
            MaxWaterSourceCandidates = 3,
            MaxWaterbodyCandidates = 2,
            MaxWaterwayHints = 3,
            MaxCrossingHints = 2,
            BasinCodeDepth = 5
        });
        var diagonal = service.Build(new RegionHydrologyWaterwayHintsCandidateOptions
        {
            Seed = "hydrology-limit-diagonal",
            RegionX = 2,
            RegionY = -4,
            IncludeDiagonals = true,
            MaxWaterSourceCandidates = 4,
            MaxWaterbodyCandidates = 3,
            MaxWaterwayHints = 5,
            MaxCrossingHints = 4,
            BasinCodeDepth = 6
        });
        var closed = Enumerable.Range(0, 200)
            .Select(index => service.Build(new RegionHydrologyWaterwayHintsCandidateOptions
            {
                Seed = "hydrology-closed-basin-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                RegionX = index - 75,
                RegionY = 40 - index,
                IncludeDiagonals = true
            }))
            .FirstOrDefault(result => result.Report.Plan.Drainage.OutflowDirection == "ClosedBasin");

        Assert.True(cardinal.Report.ContractProofPassed, string.Join(Environment.NewLine, cardinal.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(-1234567, cardinal.Report.Plan.RegionX);
        Assert.Equal(987654, cardinal.Report.Plan.RegionY);
        Assert.StartsWith("region/-1234567_987654/", cardinal.Report.Plan.RegionId, StringComparison.Ordinal);
        Assert.Equal(4, cardinal.Report.Plan.NeighborRegionSummaries.Count);
        Assert.DoesNotContain(cardinal.Report.Plan.NeighborRegionSummaries, item => item.IsDiagonal);
        Assert.Contains(cardinal.Report.Plan.Drainage.OutflowDirection, new[] { "North", "East", "South", "West", "ClosedBasin" });
        Assert.True(cardinal.Report.Plan.WaterSourceCandidates.Count <= 3);
        Assert.True(cardinal.Report.Plan.WaterbodyCandidates.Count <= 2);
        Assert.True(cardinal.Report.Plan.WaterwayCorridorHints.Count <= 3);
        Assert.True(cardinal.Report.Plan.CrossingPressureHints.Count <= 2);
        Assert.Equal("local-basin-code/".Length + 5, cardinal.Report.Plan.Drainage.BasinCode.Length);

        Assert.True(diagonal.Report.ContractProofPassed, string.Join(Environment.NewLine, diagonal.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.Equal(8, diagonal.Report.Plan.NeighborRegionSummaries.Count);
        Assert.Contains(diagonal.Report.Plan.NeighborRegionSummaries, item => item.IsDiagonal);
        Assert.Contains(diagonal.Report.Plan.Drainage.OutflowDirection, new[] { "North", "East", "South", "West", "NorthEast", "SouthEast", "SouthWest", "NorthWest", "ClosedBasin" });
        Assert.Equal("local-basin-code/".Length + 6, diagonal.Report.Plan.Drainage.BasinCode.Length);

        Assert.NotNull(closed);
        Assert.Equal("ClosedBasin", closed!.Report.Plan.Drainage.OutflowDirection);
        Assert.Empty(closed.Report.Plan.Drainage.DownstreamNeighborRegionId);
        Assert.Equal(closed.Report.Plan.RegionX, closed.Report.Plan.Drainage.DownstreamNeighborRegionX);
        Assert.Equal(closed.Report.Plan.RegionY, closed.Report.Plan.Drainage.DownstreamNeighborRegionY);
    }

    [Fact]
    public async Task HydrologyWaterwayHintsDifferentSeedAndWrittenReportAreStable()
    {
        using var temp = new TempDirectory();
        var service = new RegionHydrologyWaterwayHintsCandidateService();
        var options = new RegionHydrologyWaterwayHintsCandidateOptions
        {
            Seed = "hydrology-seed-a",
            RegionX = 4,
            RegionY = -7,
            IncludeDiagonals = true
        };

        var first = service.Build(options);
        var second = service.Build(options with { Seed = "hydrology-seed-b" });
        var write = await service.WriteAsync(temp.Path, first);
        var report = JsonSerializer.Deserialize<RegionHydrologyWaterwayHintsCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.NotEqual(first.Report.Plan.PlanId, second.Report.Plan.PlanId);
        Assert.NotEqual(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.DifferentSeedVariationVisible);
        Assert.Equal(RegionHydrologyWaterwayHintsCandidateService.CandidateId, report.CandidateId);
        Assert.Equal(RegionHydrologyWaterwayHintsCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.All(report.ExternalScoutingDecisions, item => Assert.Equal("reference_only", item.Decision));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        var markdown = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("reference_only", markdown, StringComparison.Ordinal);
        Assert.Contains("does not implement actual rivers", markdown, StringComparison.Ordinal);
        Assert.Contains("not a real Pfafstetter implementation", markdown, StringComparison.Ordinal);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
