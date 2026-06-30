using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using Xunit;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public sealed class InfiniteChunkedWorldSmokeTests
{
    [Fact]
    public async Task BoundedInfiniteChunkWindowProofIsDeterministic()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();
        var service = ChunkedRuntimePreviewExportTestFactory.CreateService();

        var first = service.Build(temp.Path).InfiniteSmokeProof;
        var second = service.Build(temp.Path).InfiniteSmokeProof;

        Assert.True(first.Deterministic);
        Assert.False(first.RealInfiniteStreamingImplemented);
        Assert.Equal(first.RepeatableHash, first.ReplayedHash);
        Assert.Equal(first.RepeatableHash, second.RepeatableHash);
        Assert.Equal(3, first.Window.Width);
        Assert.Equal(3, first.Window.Height);
        Assert.Equal(9, first.DerivedChunks.Count);
        Assert.All(first.DerivedChunks, chunk => Assert.StartsWith("chunk/infinite/goal040/", chunk.ChunkId, StringComparison.Ordinal));
        Assert.NotEmpty(first.BoundaryHandoffPlaceholders);
    }
}
