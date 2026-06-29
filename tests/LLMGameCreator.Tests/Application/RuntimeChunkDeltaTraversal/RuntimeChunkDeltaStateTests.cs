using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkDeltaStateTests
{
    [Fact]
    public void TraversalMutatesRuntimeOwnedChunkDeltaState()
    {
        var plan = new RuntimeChunkTraversalPlanner()
            .BuildPlans()
            .Single(item => item.ScenarioId == "frontier_survival");

        var proof = RuntimeChunkDeltaProjector.Apply(plan);

        Assert.True(proof.RuntimeAttempted);
        Assert.True(proof.StateChangedAfterTraversal);
        Assert.Equal("GameRuntimeState", proof.RuntimeStateOwner);
        Assert.NotEqual(proof.BeforeStateHash, proof.AfterStateHash);
        Assert.Contains("metadata.runtimeChunk.runtimeDeltas", proof.ChangedStateKeys);
        Assert.Contains("metadata.runtimeChunk.localMutations", proof.ChangedStateKeys);
        Assert.NotEmpty(proof.After.VisitedRegionIds);
        Assert.NotEmpty(proof.After.DiscoveredChunkIds);
        Assert.NotEmpty(proof.After.LandmarkDiscoveryIds);
        Assert.NotEmpty(proof.After.RouteCheckpointMarkerIds);
        Assert.NotEmpty(proof.After.LocalMutations);
        Assert.All(proof.After.RuntimeDeltas, delta =>
        {
            Assert.True(delta.RuntimeSaveOnly);
            Assert.Equal(plan.ScenarioId, delta.ScenarioId);
            Assert.StartsWith("chunk/" + plan.ScenarioId + "/", delta.ChunkId, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void RuntimeChunkDeltasDoNotMutateGamePackageDefinitions()
    {
        var result = RuntimeChunkDeltaTraversalTestFactory.CreateService().Build();

        Assert.False(result.Report.GamePackageDefinitionsMutated);
        Assert.True(result.Report.NoRuntimeUiUnityProviderLlmRagLuaGeneratorLibraryLeakage);
        Assert.DoesNotContain(result.Report.Diagnostics, item => item.Code.Contains("gamepackage", StringComparison.OrdinalIgnoreCase) && item.Severity == "error");
    }
}
