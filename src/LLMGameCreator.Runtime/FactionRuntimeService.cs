using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class FactionRuntimeService : IFactionRuntimeService
{
    public GameRuntimeResult ChangeReputation(GamePackageDefinition package, GameRuntimeState state, string factionId, double amount)
    {
        var faction = package.Game.Factions.FirstOrDefault(f => RuntimeStateHelpers.IdEquals(f.Id, factionId));
        if (faction == null)
        {
            return Failure(state, "faction.missing", $"Faction not found: {factionId}", factionId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtime = RuntimeStateHelpers.EnsureFaction(working, faction);
        var before = runtime.Reputation;
        runtime.Reputation = RuntimeStateHelpers.Clamp(runtime.Reputation + amount, faction.MinReputation, faction.MaxReputation);
        RuntimeStateHelpers.CopyState(working, state);
        return Success(state, $"Faction reputation changed: {factionId} {before:0.####} -> {runtime.Reputation:0.####}", GameRuntimeEventType.FactionReputationChanged, factionId, before, runtime.Reputation, amount);
    }

    public GameRuntimeResult SetReputation(GamePackageDefinition package, GameRuntimeState state, string factionId, double value)
    {
        var faction = package.Game.Factions.FirstOrDefault(f => RuntimeStateHelpers.IdEquals(f.Id, factionId));
        if (faction == null)
        {
            return Failure(state, "faction.missing", $"Faction not found: {factionId}", factionId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtime = RuntimeStateHelpers.EnsureFaction(working, faction);
        runtime.Reputation = RuntimeStateHelpers.Clamp(value, faction.MinReputation, faction.MaxReputation);
        RuntimeStateHelpers.CopyState(working, state);
        return Success(state, $"Faction reputation set: {factionId} = {runtime.Reputation:0.####}", GameRuntimeEventType.FactionReputationChanged, factionId, runtime.Reputation, runtime.Reputation, 0);
    }

    public GameRuntimeResult SetFactionRelation(GamePackageDefinition package, GameRuntimeState state, string factionId, string relationKind)
    {
        var faction = package.Game.Factions.FirstOrDefault(f => RuntimeStateHelpers.IdEquals(f.Id, factionId));
        if (faction == null)
        {
            return Failure(state, "faction.missing", $"Faction not found: {factionId}", factionId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtime = RuntimeStateHelpers.EnsureFaction(working, faction);
        runtime.RelationKind = string.IsNullOrWhiteSpace(relationKind) ? "neutral" : relationKind.Trim();
        RuntimeStateHelpers.CopyState(working, state);
        return Success(state, $"Faction relation set: {factionId} = {runtime.RelationKind}", GameRuntimeEventType.FactionRelationChanged, factionId);
    }

    private static GameRuntimeResult Success(GameRuntimeState state, string message, GameRuntimeEventType eventType, string targetId, double? before = null, double? after = null, double requested = 0)
    {
        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = message,
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(eventType, message, targetId, before.HasValue ? new Dictionary<string, string> { ["factionId"] = targetId, ["before"] = before.Value.ToString("0.####"), ["after"] = after!.Value.ToString("0.####"), ["delta"] = (after.Value - before.Value).ToString("0.####"), ["clamped"] = (Math.Abs((before.Value + requested) - after.Value) > 0.0000001).ToString().ToLowerInvariant() } : null) }
        };
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string targetId)
    {
        return new GameRuntimeResult
        {
            Success = false,
            State = state,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) },
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, message, targetId) }
        };
    }
}
