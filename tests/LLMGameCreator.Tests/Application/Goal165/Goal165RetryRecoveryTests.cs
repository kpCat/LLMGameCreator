using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal165;

public sealed class Goal165RetryRecoveryTests
{
    [Fact]
    public void Behavioral_retry_restores_without_runtime_start()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package);

        Assert.True(restored.Passed);
        Assert.NotNull(restored.Session);
    }

    [Fact]
    public void Behavioral_retry_preserves_exact_encounter_identity_for_one_start_encounter_dispatch()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();

        Assert.Equal("encounter/fixture", harness.Recovery.Checkpoint!.EncounterId);
    }

    [Fact]
    public void Behavioral_retry_restores_map_and_position_exactly()
    {
        var harness = StatefulHarness();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package).Session!;

        Assert.Equal("map/before", restored.MapState.CurrentMapId);
        Assert.Equal(3, restored.MapState.PlayerPosition.X);
        Assert.Equal(5, restored.MapState.PlayerPosition.Y);
    }

    [Fact]
    public void Behavioral_retry_restores_inventory_exactly()
    {
        var harness = StatefulHarness();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package).Session!;

        var stack = Assert.Single(Assert.Single(restored.GameplayState.Inventories).Stacks);
        Assert.Equal("item/reward", stack.ItemId);
        Assert.Equal(2, stack.Amount);
    }

    [Fact]
    public void Behavioral_retry_restores_quest_readiness_exactly()
    {
        var harness = StatefulHarness();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package).Session!;

        Assert.Equal("active", Assert.Single(restored.GameplayState.Quests).State);
    }

    [Fact]
    public void Behavioral_retry_restores_reputation_exactly()
    {
        var harness = StatefulHarness();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package).Session!;

        Assert.Equal(4, Assert.Single(restored.GameplayState.Factions).Reputation);
    }

    [Fact]
    public void Behavioral_lost_attempt_reward_does_not_survive_restore()
    {
        var harness = StatefulHarness();
        harness.PreEncounter.GameplayState.Inventories[0].Stacks[0].Amount = 99;
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package).Session!;

        Assert.Equal(2, restored.GameplayState.Inventories[0].Stacks[0].Amount);
    }

    [Fact]
    public void Behavioral_retry_can_return_to_same_active_encounter_checkpoint()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package);

        Assert.True(restored.Passed);
        Assert.Equal(harness.Recovery.Checkpoint!.EncounterId, "encounter/fixture");
    }

    [Fact]
    public void Behavioral_second_defeat_keeps_same_checkpoint_retryable()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var first = harness.Recovery.Restore(harness.Truth, harness.Package);
        var second = harness.Recovery.Restore(harness.Truth, harness.Package);

        Assert.True(first.Passed);
        Assert.True(second.Passed);
    }

    [Fact]
    public void Behavioral_victory_after_retry_clears_checkpoint()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        harness.Recovery.Clear();

        Assert.Null(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Behavioral_stale_retry_restores_nothing()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var stale = harness.Recovery.Restore(harness.Truth with { PackageSha256 = "drift" }, harness.Package);

        Assert.True(stale.Stale);
        Assert.Null(stale.Session);
    }

    private static Goal165RecoveryHarness StatefulHarness()
    {
        var harness = Goal165RecoveryHarness.Create();
        harness.PreEncounter.MapState.CurrentMapId = "map/before";
        harness.PreEncounter.MapState.PlayerPosition.X = 3;
        harness.PreEncounter.MapState.PlayerPosition.Y = 5;
        harness.PreEncounter.GameplayState.Inventories =
        [
            new InventoryState
            {
                Id = "inventory/player",
                OwnerKind = "player",
                Stacks = [new ItemStackState { ItemId = "item/reward", Amount = 2 }]
            }
        ];
        harness.PreEncounter.GameplayState.Quests = [new QuestRuntimeState { QuestId = "quest/active", State = "active" }];
        harness.PreEncounter.GameplayState.Factions = [new FactionRuntimeState { FactionId = "faction/home", Reputation = 4 }];
        harness.Recovery.Commit(harness.Recovery.Prepare(harness.Truth, harness.Package,
            harness.PreEncounter, "encounter/fixture", "Проверочная встреча"));
        return harness;
    }
}
