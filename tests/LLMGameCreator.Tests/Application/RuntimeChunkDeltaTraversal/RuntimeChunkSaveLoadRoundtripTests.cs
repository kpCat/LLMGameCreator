using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkSaveLoadRoundtripTests
{
    [Fact]
    public void RuntimeSerializerAndSnapshotStorePreserveChunkDeltas()
    {
        var result = RuntimeChunkDeltaTraversalTestFactory.CreateService().Build();

        Assert.True(result.SaveLoadRoundtripProof.Passed);
        Assert.Equal(4, result.SaveLoadRoundtripProof.ScenarioCount);
        Assert.All(result.SaveLoadRoundtripProof.Scenarios, scenario =>
        {
            Assert.True(scenario.RuntimeAttempted);
            Assert.True(scenario.UsedRuntimeStateSerializer);
            Assert.True(scenario.UsedRuntimeSnapshotStore);
            Assert.EndsWith(nameof(RuntimeStateSerializer), scenario.SerializerType, StringComparison.Ordinal);
            Assert.EndsWith(nameof(RuntimeSnapshotStore), scenario.SnapshotStoreType, StringComparison.Ordinal);
            Assert.True(scenario.SerializerRoundtripPassed);
            Assert.True(scenario.SnapshotRoundtripPassed);
            Assert.True(scenario.TempSnapshotCleanupSucceeded);
            Assert.Contains("metadata.runtimeChunk.runtimeDeltas", scenario.RestoredStateEvidence.Keys);
            Assert.DoesNotContain(scenario.RestoredStateEvidence.Values, value => value.Contains(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void SameSeedReplayIsDeterministic()
    {
        var result = RuntimeChunkDeltaTraversalTestFactory.CreateService().Build();

        Assert.True(result.ReplayDeterminismProof.Passed);
        Assert.Equal(4, result.ReplayDeterminismProof.ScenarioCount);
        Assert.All(result.ReplayDeterminismProof.Scenarios, scenario =>
        {
            Assert.True(scenario.SameSeedDeterministic);
            Assert.Equal(scenario.FirstRunHash, scenario.SecondRunHash);
            Assert.True(scenario.CommandCount > 0);
        });
    }
}
