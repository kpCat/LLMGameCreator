using System.Text.Json;
using LLMGameCreator.Application.Design.CandidateModules.WorldBiomeNoise;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class CandidateWorldBiomeNoiseSmokeTests
{
    [Fact]
    public async Task CandidateWorldBiomeNoiseProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new WorldBiomeNoiseCandidateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        var report = JsonSerializer.Deserialize<WorldBiomeNoiseCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(WorldBiomeNoiseCandidateService.CandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.FastNoiseLiteDependencyAdopted);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.RuntimeProviderNetworkDependency);
        Assert.True(report.ExternalExecution.AllFalse);
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public async Task CandidateWorldRegionClimateProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new WorldBiomeNoiseCandidateService();

        var write = await service.BuildAndWriteRegionClimateAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        var report = JsonSerializer.Deserialize<WorldRegionClimateCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(WorldBiomeNoiseCandidateService.RegionClimateCandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.GlobalMapMaterialized);
        Assert.False(report.SettlementGenerationImplemented);
        Assert.False(report.RoadGenerationImplemented);
        Assert.False(report.FactionGenerationImplemented);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.RuntimeProviderNetworkDependency);
        Assert.True(report.ExternalExecution.AllFalse);
        Assert.NotEmpty(report.Samples);
        Assert.NotEmpty(report.RegionSummaries);
        Assert.All(report.Samples, sample =>
        {
            Assert.InRange(sample.ElevationScore, 0.0, 1.0);
            Assert.InRange(sample.MoistureScore, 0.0, 1.0);
            Assert.InRange(sample.TemperatureScore, 0.0, 1.0);
            Assert.InRange(sample.RuggednessScore, 0.0, 1.0);
            Assert.InRange(sample.SettlementSuitabilityScore, 0.0, 1.0);
            Assert.InRange(sample.RoadTravelCostScore, 0.0, 1.0);
        });
        Assert.All(report.RegionSummaries, summary => Assert.False(summary.UsedGlobalMapMaterialization));
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public async Task CandidateRegionSettlementRoadSeedsProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new WorldBiomeNoiseCandidateService();

        var write = await service.BuildAndWriteSettlementRoadSeedsAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        var report = JsonSerializer.Deserialize<RegionSettlementRoadSeedsCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(WorldBiomeNoiseCandidateService.SettlementRoadSeedsCandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.RegionClimateCandidateId, report.BaseCandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.GlobalMapMaterialized);
        Assert.False(report.ActualSettlementsGenerated);
        Assert.False(report.RoadPathsGenerated);
        Assert.False(report.NavigationPathfindingImplemented);
        Assert.False(report.FactionGenerationImplemented);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.RuntimeProviderNetworkDependency);
        Assert.True(report.ExternalExecution.AllFalse);
        Assert.NotEmpty(report.Plan.SettlementAnchors);
        Assert.NotEmpty(report.Plan.SourceClimateSummary.RegionId);
        Assert.All(report.Plan.SettlementAnchors, anchor => Assert.InRange(anchor.SuitabilityScore, 0.0, 1.0));
        Assert.All(report.Plan.RoadHints, hint =>
        {
            Assert.InRange(hint.EstimatedCostScore, 0.0, 1.0);
            Assert.InRange(hint.PriorityScore, 0.0, 1.0);
        });
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public async Task CandidateRegionGatewayConnectivityHintsProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new WorldBiomeNoiseCandidateService();

        var write = await service.BuildAndWriteGatewayConnectivityHintsAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        var report = JsonSerializer.Deserialize<RegionGatewayConnectivityHintsCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(WorldBiomeNoiseCandidateService.GatewayConnectivityHintsCandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.SettlementRoadSeedsCandidateId, report.BaseCandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.GlobalMapMaterialized);
        Assert.False(report.ActualRoadsGenerated);
        Assert.False(report.RoadPathsGenerated);
        Assert.False(report.NavigationPathfindingImplemented);
        Assert.False(report.NavigationGraphGenerated);
        Assert.False(report.ActualSettlementsGenerated);
        Assert.False(report.FactionGenerationImplemented);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.RuntimeProviderNetworkDependency);
        Assert.True(report.ExternalExecution.AllFalse);
        Assert.NotEmpty(report.Plan.NeighborRegionSummaries);
        Assert.NotEmpty(report.Plan.GatewayCandidates);
        Assert.NotEmpty(report.Plan.CorridorHints);
        Assert.All(report.Plan.GatewayCandidates, gateway =>
        {
            Assert.InRange(gateway.SuitabilityScore, 0.0, 1.0);
            Assert.InRange(gateway.EstimatedCrossingCostScore, 0.0, 1.0);
        });
        Assert.All(report.Plan.CorridorHints, hint =>
        {
            Assert.InRange(hint.EstimatedCostScore, 0.0, 1.0);
            Assert.InRange(hint.PriorityScore, 0.0, 1.0);
            Assert.StartsWith("region-pair/", hint.CanonicalRegionPairId, StringComparison.Ordinal);
        });
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public async Task CandidateRegionHydrologyWaterwayHintsProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new RegionHydrologyWaterwayHintsCandidateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        var report = JsonSerializer.Deserialize<RegionHydrologyWaterwayHintsCandidateReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.Equal(RegionHydrologyWaterwayHintsCandidateService.CandidateId, report.CandidateId);
        Assert.Equal(WorldBiomeNoiseCandidateService.GatewayConnectivityHintsCandidateId, report.BaseCandidateId);
        Assert.Equal(RegionHydrologyWaterwayHintsCandidateService.FinalStatus, report.FinalStatus);
        Assert.True(report.ContractProofPassed, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(report.AcceptedGateClaimed);
        Assert.False(report.GlobalMapMaterialized);
        Assert.False(report.ActualRiversGenerated);
        Assert.False(report.ActualWaterbodiesGenerated);
        Assert.False(report.RiverPathsGenerated);
        Assert.False(report.WaterwayPolylinesGenerated);
        Assert.False(report.ErosionSimulationImplemented);
        Assert.False(report.RainfallSimulationImplemented);
        Assert.False(report.FloodSimulationImplemented);
        Assert.False(report.PathfindingNavigationImplemented);
        Assert.False(report.NavigationGraphGenerated);
        Assert.False(report.ActualSettlementsGenerated);
        Assert.False(report.FactionGenerationImplemented);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.RuntimeProviderNetworkDependency);
        Assert.True(report.ExternalExecution.AllFalse);
        Assert.NotEmpty(report.Plan.NeighborRegionSummaries);
        Assert.NotEmpty(report.Plan.WaterSourceCandidates);
        Assert.NotEmpty(report.Plan.WaterbodyCandidates);
        Assert.NotEmpty(report.Plan.WaterwayCorridorHints);
        Assert.NotEmpty(report.Plan.CrossingPressureHints);
        Assert.StartsWith("basin/local/", report.Plan.Drainage.BasinId, StringComparison.Ordinal);
        Assert.StartsWith("local-basin-code/", report.Plan.Drainage.BasinCode, StringComparison.Ordinal);
        Assert.All(report.Plan.WaterSourceCandidates, source =>
        {
            Assert.InRange(source.FlowPotentialScore, 0.0, 1.0);
            Assert.InRange(source.SeasonalReliabilityScore, 0.0, 1.0);
        });
        Assert.All(report.Plan.WaterwayCorridorHints, hint =>
        {
            Assert.InRange(hint.EstimatedFlowScore, 0.0, 1.0);
            Assert.InRange(hint.PersistenceScore, 0.0, 1.0);
            Assert.InRange(hint.RoadCrossingPressureScore, 0.0, 1.0);
        });
        Assert.DoesNotContain(report.Diagnostics, item => item.Severity == "error");
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
