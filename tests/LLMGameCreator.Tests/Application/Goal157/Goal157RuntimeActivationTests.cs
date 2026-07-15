using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal157;

[Collection(Goal156Collection.Name)]
public sealed class Goal157RuntimeActivationTests
{
    [Fact]
    public void Behavioral_runtime_starts_on_generated_map()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-start");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Summary.StartSucceeded);
        Assert.Equal(Goal157TestKit.GeneratedStartMapId(copy), result.Summary.GeneratedStartMapId);
    }

    [Fact]
    public void Behavioral_move_right_succeeds_on_generated_map()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-move");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Summary.MoveSucceeded);
        Assert.Contains(result.Summary.RuntimeFrames, frame => frame.ActionId == "01_move_right");
    }

    [Fact]
    public void Behavioral_interact_succeeds_after_move_right()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-interact");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Summary.InteractSucceeded);
        Assert.Contains(result.Summary.RuntimeFrames, frame => frame.ActionId == "02_interact");
    }

    [Fact]
    public void Behavioral_interaction_is_correlated_to_generated_target_provenance()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-correlation");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Summary.GeneratedInteractionObserved);
        Assert.Contains(result.Summary.HumanFacts,
            fact => fact.Label == "Сгенерированное содержимое" && fact.Value == "подтверждено");
    }

    [Fact]
    public void Behavioral_runtime_activation_changes_state()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-state-change");

        var result = Goal157TestKit.Activate(copy);

        Assert.NotEqual(result.Summary.InitialStateHash, result.Summary.FinalStateHash);
    }

    [Fact]
    public void Behavioral_runtime_replay_is_deterministically_equivalent()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-replay");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Summary.ReplayEquivalent);
        Assert.Equal(result.Summary.FinalStateHash, result.Summary.ReplayFinalStateHash);
    }

    [Fact]
    public void Behavioral_runtime_state_roundtrip_is_exact()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-roundtrip");

        var result = Goal157TestKit.Activate(copy);

        Assert.True(result.Summary.StateRoundtripPassed);
    }

    [Fact]
    public void Behavioral_failed_move_rejects_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-move-failure");

        var result = Goal157TestKit.Activate(copy, new FaultInjectingRuntime(failMove: true));

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.move_failed", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_failed_interact_rejects_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-interact-failure");

        var result = Goal157TestKit.Activate(copy, new FaultInjectingRuntime(failInteract: true));

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.interact_failed", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_baseline_only_interaction_cannot_satisfy_generated_correlation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-baseline-target");

        var result = Goal157TestKit.Activate(copy, new FaultInjectingRuntime(replaceInteractionTarget: true));

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.generated_interaction_not_observed", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_replay_divergence_rejects_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-replay-failure");

        var result = Goal157TestKit.Activate(copy, new FaultInjectingRuntime(divergeSecondRun: true));

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.replay_mismatch", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_corrupt_roundtrip_rejects_activation()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-roundtrip-failure");

        var result = Goal157TestKit.Activate(copy, serializer: new CorruptingRuntimeStateSerializer());

        Assert.False(result.Passed);
        Assert.Contains("generated_activation.state_roundtrip_mismatch", result.Diagnostics);
    }

    [Fact]
    public void Contract_activation_lane_exposes_exact_start_move_interact_frames()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "runtime-frames");

        var result = Goal157TestKit.Activate(copy);

        Assert.Equal(["00_start_generated_world", "01_move_right", "02_interact"],
            result.Summary.RuntimeFrames.Select(frame => frame.ActionId));
        Assert.Equal([0, 1, 2], result.Summary.RuntimeFrames.Select(frame => frame.Index));
    }
}

internal sealed class FaultInjectingRuntime : IGameRuntime
{
    private readonly DefaultGameRuntime _inner = new();
    private readonly bool _failMove;
    private readonly bool _failInteract;
    private readonly bool _replaceInteractionTarget;
    private readonly bool _divergeSecondRun;
    private int _run;

    public FaultInjectingRuntime(
        bool failMove = false,
        bool failInteract = false,
        bool replaceInteractionTarget = false,
        bool divergeSecondRun = false)
    {
        _failMove = failMove;
        _failInteract = failInteract;
        _replaceInteractionTarget = replaceInteractionTarget;
        _divergeSecondRun = divergeSecondRun;
    }

    public CommandResult Start(GamePackageDefinition package)
    {
        _run++;
        return _inner.Start(package);
    }

    public CommandResult Execute(GamePackageDefinition package, GameState state, PlayerCommand command)
    {
        if (_failMove && command.Type == PlayerCommandType.Move)
            return new CommandResult { State = state, Success = false };
        if (_failInteract && command.Type == PlayerCommandType.Interact)
            return new CommandResult { State = state, Success = false };

        var result = _inner.Execute(package, state, command);
        if (_replaceInteractionTarget && command.Type == PlayerCommandType.Interact)
            foreach (var runtimeEvent in result.Events.Where(item => item.Type == RuntimeEventType.InteractionTriggered))
                runtimeEvent.TargetId = "baseline-only-target";
        if (_divergeSecondRun && _run >= 2 && command.Type == PlayerCommandType.Interact)
            result.State.Flags["goal157.replay.divergence"] = "true";
        return result;
    }
}

internal sealed class CorruptingRuntimeStateSerializer : IRuntimeStateSerializer
{
    private readonly RuntimeStateSerializer _inner = new();

    public string Serialize(GameRuntimeState state) => _inner.Serialize(state);

    public GameRuntimeState DeserializeGameRuntimeState(string json) => _inner.DeserializeGameRuntimeState(json);

    public string Serialize(UnifiedRuntimeSession session) => _inner.Serialize(session);

    public UnifiedRuntimeSession DeserializeUnifiedSession(string json)
    {
        var session = _inner.DeserializeUnifiedSession(json);
        session.MapState.CurrentMapId = "goal157-corrupt-roundtrip";
        return session;
    }
}
