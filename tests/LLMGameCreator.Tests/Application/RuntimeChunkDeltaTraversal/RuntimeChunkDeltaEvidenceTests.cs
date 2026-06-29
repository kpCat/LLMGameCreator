using System.Text.Json;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkDeltaEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateRequired()
    {
        var service = RuntimeChunkDeltaTraversalTestFactory.CreateService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal("GREEN", first.Report.ImplementationStatus);
        Assert.False(first.Report.Accepted);
        Assert.Equal(RuntimeChunkDeltaTraversalVocabulary.FinalGate, first.Report.ManualGate);
        Assert.True(first.Report.Goal038AcceptedByUserHandoff);
        Assert.Equal(4, first.Report.ScenarioCount);
        Assert.True(first.Report.RuntimeStateChangedAfterTraversal);
        Assert.True(first.Report.SaveLoadRoundtripPassed);
        Assert.True(first.Report.ReplayDeterminismPassed);
        Assert.True(first.Report.InvalidMatrixPassed);
        Assert.Equal(7, first.Report.MetamoduleKingdomGroupCount);
        Assert.Equal(112, first.Report.MetamoduleSpeciesArchetypeSlotRefCount);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.DoesNotContain(Environment.NewLine, first.ArtifactJsonByFileName[RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName]);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndParse()
    {
        using var temp = new TempDirectory();
        var write = await RuntimeChunkDeltaTraversalTestFactory.CreateService().BuildAndWriteAsync(temp.Path);
        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            [
                "chunk-replay-determinism-proof.json",
                "chunk-traversal-plan-caravan.json",
                "chunk-traversal-plan-frontier.json",
                "chunk-traversal-plan-gothic.json",
                "chunk-traversal-plan-metamodule.json",
                "invalid-chunk-diagnostics-matrix.json",
                "runtime-chunk-delta-state-frontier.json",
                "runtime-chunk-delta-state-metamodule.json",
                "runtime-chunk-delta-traversal-smoke-report.md",
                "runtime-save-load-roundtrip-proof.json"
            ],
            names);

        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName)));
        using var metamoduleState = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.MetamoduleStateJsonFileName)));
        using var roundtrip = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName)));
        using var replay = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("metamodule_kingdoms", metamoduleState.RootElement.GetProperty("scenarioId").GetString());
        Assert.True(metamoduleState.RootElement.GetProperty("localMutations").EnumerateObject().Any());
        Assert.True(roundtrip.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(replay.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("runtime_chunk_delta_traversal_smoke_verification required", report);
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
                Directory.Delete(Path, true);
            }
        }
    }
}
