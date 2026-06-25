using LLMGameCreator.Application.Design.World;
using Xunit;

namespace LLMGameCreator.Tests.Application.World;

public sealed class ConnectedWorldTravelAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedConnectedWorldTravelArtifacts()
    {
        using var temp = new TempDirectory();
        var service = new ConnectedWorldTravelAcceptanceService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("connected_world_travel_state_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal006GateRecorded);
        Assert.Equal(4, first.Report.ValidScenarioCount);
        Assert.Equal(4, first.Report.InvalidScenarioCount);
        Assert.True(first.Report.ValidScenariosAccepted);
        Assert.True(first.Report.InvalidScenariosRejected);
        Assert.True(first.Report.DeterministicReplayPassed);
        Assert.True(first.Report.TravelRuntimeExecutionPassed);
        Assert.True(first.Report.SaveLoadRoundtripPassed);
        Assert.True(first.Report.RouteBindingEvidencePassed);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
    }

    [Fact]
    public void ValidRouteStepsReferenceConnectionsAndDestinationMapBindings()
    {
        var report = new ConnectedWorldTravelAcceptanceService().Build().Report;

        foreach (var scenario in report.Scenarios.Where(item => item.ExpectedValid))
        {
            var connectionIds = scenario.RegionGraph.Connections.Select(item => item.ConnectionId).ToHashSet(StringComparer.Ordinal);
            var bindingByRegion = scenario.MapBindings.ToDictionary(item => item.RegionId, item => item.MapId, StringComparer.Ordinal);

            Assert.NotEmpty(scenario.RuntimeEvidence.RouteSteps);
            Assert.All(scenario.RuntimeEvidence.RouteSteps, step =>
            {
                Assert.True(step.Succeeded);
                Assert.Contains(step.ConnectionId, connectionIds);
                Assert.True(bindingByRegion.TryGetValue(step.ToRegionId, out var expectedMapId));
                Assert.Equal(expectedMapId, step.ToMapId);
            });
        }
    }

    [Fact]
    public void GraphReachabilityCoversAllRequiredRegions()
    {
        var report = new ConnectedWorldTravelAcceptanceService().Build().Report;

        Assert.True(report.GraphReachabilityPassed);
        Assert.All(report.Scenarios.Where(item => item.ExpectedValid), scenario =>
        {
            Assert.True(scenario.Reachability.AllRequiredReachable);
            Assert.Empty(scenario.Reachability.MissingRequiredRegionIds);
            Assert.Subset(
                scenario.Reachability.RequiredRegionIds.ToHashSet(StringComparer.Ordinal),
                scenario.Reachability.ReachableRegionIds.ToHashSet(StringComparer.Ordinal));
        });
    }

    [Fact]
    public void VariableMapsHaveDistinctExactDimensionsOrSignatures()
    {
        var report = new ConnectedWorldTravelAcceptanceService().Build().Report;

        Assert.True(report.VariableMapEvidencePassed);
        var variableMaps = report.Scenarios.Single(item => item.ScenarioId == "connected_world_variable_maps");
        Assert.Equal(4, variableMaps.MapBindings.Count);
        Assert.All(variableMaps.MapBindings, binding =>
            Assert.Contains(variableMaps.MapSignatures, signature =>
                signature.RegionId == binding.RegionId &&
                signature.MapId == binding.MapId &&
                signature.Width > 0 &&
                signature.Height > 0 &&
                !string.IsNullOrWhiteSpace(signature.LayoutSignature)));
        Assert.True(variableMaps.MapSignatures
            .Select(item => item.Width + "x" + item.Height + ":" + item.LayoutSignature)
            .Distinct(StringComparer.Ordinal)
            .Count() >= 3);
    }

    [Fact]
    public void TravelRuntimeChangesCurrentRegionAndMapThroughRuntimeOwnedState()
    {
        var scenario = new ConnectedWorldTravelAcceptanceService()
            .Build()
            .Report
            .Scenarios
            .Single(item => item.ScenarioId == "connected_world_core_route");

        Assert.True(scenario.RuntimeEvidence.RuntimeAttempted);
        Assert.Equal("GameRuntimeState", scenario.RuntimeEvidence.RuntimeStateOwner);
        Assert.Equal("region/hub", scenario.RuntimeEvidence.StartRegionId);
        Assert.Equal("map/hub-start", scenario.RuntimeEvidence.StartMapId);
        Assert.Contains(scenario.RuntimeEvidence.RouteSteps, step =>
            step.FromRegionId == "region/hub" &&
            step.FromMapId == "map/hub-start" &&
            step.ToRegionId == "region/wildland-frontier" &&
            step.ToMapId == "map/wildland-frontier");
        Assert.Equal("region/hub", scenario.RuntimeEvidence.FinalRegionId);
        Assert.Equal("map/hub-start", scenario.RuntimeEvidence.FinalMapId);
        Assert.Contains("region/wildland-frontier", scenario.RuntimeEvidence.StateEvidence["visitedRegionIds"]);
    }

    [Fact]
    public void InvalidTravelIsRejectedWithoutWeakeningValidScenarioAcceptance()
    {
        var scenario = new ConnectedWorldTravelAcceptanceService()
            .Build()
            .Report
            .Scenarios
            .Single(item => item.ScenarioId == "connected_world_core_route");

        Assert.True(scenario.ActualValid);
        Assert.True(scenario.RuntimeEvidence.InvalidTravelRejected);
        Assert.Contains(scenario.RuntimeEvidence.Diagnostics, item => item.Code == "connected_world.travel.connection_not_available");
    }

    [Fact]
    public void SaveLoadRestoresExactTravelWorldAndChunkEvidence()
    {
        var report = new ConnectedWorldTravelAcceptanceService().Build().Report;

        Assert.True(report.SaveLoadRoundtripPassed);
        Assert.All(report.Scenarios.Where(item => item.ExpectedValid), scenario =>
        {
            Assert.True(scenario.RuntimeEvidence.SaveLoadRoundtripPassed);
            Assert.True(scenario.RuntimeEvidence.ExactStateComparisonPassed);
            Assert.Equal(scenario.RuntimeEvidence.RuntimeStateHash, scenario.RuntimeEvidence.RestoredRuntimeStateHash);
            Assert.Equal(scenario.RuntimeEvidence.StateEvidence, scenario.RuntimeEvidence.RestoredStateEvidence);
            Assert.Contains("worldProfileId", scenario.RuntimeEvidence.StateEvidence.Keys);
            Assert.Contains("travelLog", scenario.RuntimeEvidence.StateEvidence.Keys);
            Assert.Contains("runtimeChunkDeltas", scenario.RuntimeEvidence.StateEvidence.Keys);
        });
    }

    [Fact]
    public void BoundedChunkIdsAreDeterministicAndRuntimeDeltasPersist()
    {
        var first = new ConnectedWorldTravelAcceptanceService().Build().Report;
        var second = new ConnectedWorldTravelAcceptanceService().Build().Report;

        Assert.Equal(first.ReportJsonHash(), second.ReportJsonHash());
        Assert.True(first.ChunkDeltaPersistencePassed);
        Assert.All(first.Scenarios.Where(item => item.ExpectedValid), scenario =>
        {
            Assert.Equal(16, scenario.ChunkEvidence.ChunkSize);
            Assert.Equal("chunk_rules/goal007_v1", scenario.ChunkEvidence.RulesVersion);
            Assert.All(scenario.ChunkEvidence.Chunks, chunk =>
            {
                Assert.StartsWith("chunk/", chunk.ChunkId, StringComparison.Ordinal);
                Assert.False(string.IsNullOrWhiteSpace(chunk.Hash));
            });
            Assert.NotEmpty(scenario.ChunkEvidence.RuntimeDeltas);
            Assert.All(scenario.ChunkEvidence.RuntimeDeltas, delta => Assert.True(delta.RuntimeSaveOnly));
            Assert.True(scenario.RuntimeEvidence.ChunkDeltasPersisted);
        });
    }

    [Fact]
    public void InvalidScenariosAreRejectedByStableDiagnosticsAndDoNotExecuteRuntime()
    {
        var report = new ConnectedWorldTravelAcceptanceService().Build().Report;
        var invalid = report.Scenarios.Where(item => !item.ExpectedValid).ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.Equal(4, invalid.Count);
        Assert.Contains(invalid["invalid_disconnected_region_graph"].Diagnostics, item => item.Code == "connected_world.disconnected_required_region");
        Assert.Contains(invalid["invalid_missing_region_or_map_ref"].Diagnostics, item => item.Code == "connected_world.missing_map_ref");
        Assert.Contains(invalid["invalid_missing_region_or_map_ref"].Diagnostics, item => item.Code == "connected_world.missing_connection_region_ref");
        Assert.Contains(invalid["invalid_chunk_boundary_or_rules"].Diagnostics, item => item.Code == "connected_world.chunk_seed_missing");
        Assert.Contains(invalid["invalid_chunk_boundary_or_rules"].Diagnostics, item => item.Code == "connected_world.chunk_boundary_incompatible");
        Assert.Contains(invalid["invalid_runtime_delta_as_source"].Diagnostics, item => item.Code == "connected_world.runtime_delta_in_source_content");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.False(scenario.RuntimeEvidence.RuntimeAttempted);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public void RuntimeDeltasAreNotAcceptedAsSourcePackageContent()
    {
        var report = new ConnectedWorldTravelAcceptanceService().Build().Report;

        Assert.All(report.Scenarios.Where(item => item.ExpectedValid), scenario => Assert.Empty(scenario.ChunkEvidence.SourceRuntimeDeltaIds));
        var invalid = report.Scenarios.Single(item => item.ScenarioId == "invalid_runtime_delta_as_source");
        Assert.NotEmpty(invalid.ChunkEvidence.SourceRuntimeDeltaIds);
        Assert.False(invalid.ActualValid);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "connected_world.runtime_delta_in_source_content");
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

internal static class ConnectedWorldTravelReportTestExtensions
{
    public static string ReportJsonHash(this ConnectedWorldTravelReport report) => report.DeterministicHash;
}
