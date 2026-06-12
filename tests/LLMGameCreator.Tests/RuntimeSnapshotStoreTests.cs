using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeSnapshotStoreTests
{
    [Fact]
    public void SaveListAndLoadSnapshotRoundtripRuntimeSessionOnly()
    {
        var folder = Path.Combine(Path.GetTempPath(), "llmgc-snapshot-tests", Guid.NewGuid().ToString("N"));
        var store = new RuntimeSnapshotStore(new RuntimeStateSerializer());
        var session = new UnifiedRuntimeSession
        {
            GameplayState = new GameRuntimeState { PackageId = "game/snapshot", PlayerEntityId = "player" }
        };

        var save = store.SaveSnapshot(folder, "slot1", session);
        var list = store.ListSnapshots(folder);
        var load = store.LoadSnapshot(folder, "slot1");
        var unsafeSlot = store.SaveSnapshot(folder, "..\\escape", session);

        Assert.True(save.Success);
        Assert.True(list.Success);
        Assert.Contains("slot1", list.SlotNames);
        Assert.True(load.Success);
        Assert.Equal("game/snapshot", load.Session!.GameplayState.PackageId);
        Assert.False(unsafeSlot.Success);
        Assert.DoesNotContain("tilePrototypes", File.ReadAllText(save.Path!));
    }
}
