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

