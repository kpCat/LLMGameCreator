using System.Text.Json;
using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldScaleRegionMapFoundation;

public sealed class WorldScaleRegionMapEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateRequired()
    {
        var service = new WorldScaleRegionMapEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => item.Severity + ":" + item.Code)));
        Assert.False(first.Report.Accepted);
        Assert.Equal(WorldScaleRegionMapVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Equal(4, first.Report.ScenarioCount);
        Assert.Equal(4, first.Report.FiniteMapPackCount);
        Assert.True(first.Report.RequiredReachabilityPassed);
        Assert.Equal(7, first.Report.MetamoduleKingdomGroupCount);
        Assert.True(first.Report.MetamoduleSpeciesArchetypeSlotRefCount >= 112);
        Assert.True(first.Report.InvalidMatrixPassed);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(
            first.ArtifactJsonByFileName[WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName],
            second.ArtifactJsonByFileName[WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName]);
        Assert.DoesNotContain(Environment.NewLine, first.ArtifactJsonByFileName[WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName]);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndParse()
    {
        using var temp = new TempDirectory();
        var write = await new WorldScaleRegionMapEvidenceService().BuildAndWriteAsync(temp.Path);
        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            [
                "chunked-world-config-prelude.json",
                "finite-map-pack-caravan.json",
                "finite-map-pack-frontier.json",
                "finite-map-pack-gothic.json",
                "finite-map-pack-metamodule-kingdoms.json",
                "invalid-world-scale-diagnostics-matrix.json",
                "reachability-matrix.json",
                "region-graph-summary.json",
                "traversal-itinerary-matrix.json",
                "world-scale-region-map-foundation-report.md"
            ],
            names);

        using var graph = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName)));
        using var reachability = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.ReachabilityMatrixJsonFileName)));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("metamodule_kingdoms"))));
        using var chunks = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.ChunkedWorldConfigPreludeJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(4, graph.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.True(reachability.RootElement.GetProperty("allRequiredTargetsReachable").GetBoolean());
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("axial_hex", metamodule.RootElement.GetProperty("coordinateKind").GetString());
        Assert.Equal(4, chunks.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("world_scale_region_map_foundation_verification required", report);
        Assert.Contains("accepted=false", report);
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
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
