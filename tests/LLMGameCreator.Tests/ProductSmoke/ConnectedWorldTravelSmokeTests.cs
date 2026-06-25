using System.Text.Json;
using LLMGameCreator.Application.Design.World;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ConnectedWorldTravelSmokeTests
{
    [Fact]
    public async Task ConnectedWorldTravelProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = new ConnectedWorldTravelAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        var json = await File.ReadAllTextAsync(write.ReportJsonPath);
        var report = JsonSerializer.Deserialize<ConnectedWorldTravelReport>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(report.Accepted);
        Assert.Equal("connected_world_travel_state_artifact_verification", report.ManualGate);
        Assert.True(report.Goal006GateRecorded);
        Assert.Equal(4, report.ValidScenarioCount);
        Assert.Equal(4, report.InvalidScenarioCount);
        Assert.True(report.DeterministicReplayPassed);
        Assert.True(report.TravelRuntimeExecutionPassed);
        Assert.True(report.SaveLoadRoundtripPassed);
        Assert.True(report.VariableMapEvidencePassed);
        Assert.True(report.ChunkDeltaPersistencePassed);
        Assert.True(report.RouteBindingEvidencePassed);
        Assert.False(report.ExternalExecution.LlmExecuted);
        Assert.False(report.ExternalExecution.RagExecuted);
        Assert.False(report.ExternalExecution.ProviderExecuted);
        Assert.False(report.ExternalExecution.LuaExecuted);
        Assert.False(report.ExternalExecution.UnityExecuted);
        Assert.False(report.ExternalExecution.MediaExecuted);

        var valid = report.Scenarios.Where(item => item.ExpectedValid).ToList();
        Assert.All(valid, scenario =>
        {
            Assert.True(scenario.ActualValid);
            Assert.NotEmpty(scenario.RuntimeEvidence.RouteSteps);
            Assert.True(scenario.Reachability.AllRequiredReachable);
            Assert.True(scenario.RuntimeEvidence.InvalidTravelRejected);
            Assert.True(scenario.RuntimeEvidence.ExactStateComparisonPassed);
            Assert.True(scenario.RuntimeEvidence.ChunkDeltasPersisted);
            Assert.True(scenario.MapSignatures
                .Select(item => item.Width + "x" + item.Height + ":" + item.LayoutSignature)
                .Distinct(StringComparer.Ordinal)
                .Count() >= 3);
        });

        var core = report.Scenarios.Single(item => item.ScenarioId == "connected_world_core_route");
        Assert.Contains(core.RuntimeEvidence.RouteSteps, step =>
            step.ConnectionId == "connection/hub-to-wildland" &&
            step.FromMapId == "map/hub-start" &&
            step.ToMapId == "map/wildland-frontier" &&
            step.Succeeded);

        var invalid = report.Scenarios.Where(item => !item.ExpectedValid).ToList();
        Assert.All(invalid, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.False(scenario.RuntimeEvidence.RuntimeAttempted);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
