using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class UnifiedGameRuntimeService : IUnifiedGameRuntimeService
{
    private readonly IGameRuntime _mapRuntime;
    private readonly IGameRuntimeService _gameplayRuntime;

    public UnifiedGameRuntimeService(IGameRuntime mapRuntime, IGameRuntimeService gameplayRuntime)
    {
        _mapRuntime = mapRuntime;
        _gameplayRuntime = gameplayRuntime;
    }

    public UnifiedRuntimeResult Start(GamePackageDefinition package)
    {
        var map = _mapRuntime.Start(package);
        var gameplay = _gameplayRuntime.CreateInitialState(package);
        var session = new UnifiedRuntimeSession
        {
            MapState = map.State,
            GameplayState = gameplay.State,
            MapEvents = map.Events.ToList(),
            GameplayEvents = gameplay.Events.ToList()
        };
        session.Metadata["runtimeBridge"] = "unified-v1";

        return new UnifiedRuntimeResult
        {
            Success = map.Success && gameplay.Success,
            Session = session,
            MapEvents = map.Events.ToList(),
            GameplayEvents = gameplay.Events.ToList(),
            Diagnostics = gameplay.Diagnostics.ToList(),
            Message = map.Success && gameplay.Success ? "Unified runtime session started." : "Unified runtime session start failed."
        };
    }

    public UnifiedRuntimeResult ExecutePlayerCommand(GamePackageDefinition package, UnifiedRuntimeSession session, PlayerCommand command)
    {
        if (command.Type == PlayerCommandType.Wait)
        {
            return ExecuteGameplayCommand(package, session, GameRuntimeCommand.TickResourceNodes(1));
        }

        if (command.Type == PlayerCommandType.UseItem && !string.IsNullOrWhiteSpace(command.Payload))
        {
            return ExecuteGameplayCommand(package, session, GameRuntimeCommand.UseItem(command.Payload.Trim(), targetId: command.TargetId));
        }

        var mapResult = _mapRuntime.Execute(package, session.MapState, command);
        session.MapState = mapResult.State;
        session.MapEvents.AddRange(mapResult.Events);

        var gameplayEvents = new List<GameRuntimeEvent>();
        var diagnostics = new List<RuntimeDiagnostic>();
        var success = mapResult.Success;

        if (command.Type == PlayerCommandType.Interact)
        {
            var interactionId = ResolveInteractionId(package, session.MapState, command, mapResult.Events);
            if (!string.IsNullOrWhiteSpace(interactionId))
            {
                var gameplayResult = _gameplayRuntime.Execute(
                    package,
                    session.GameplayState,
                    GameRuntimeCommand.ExecuteInteraction(interactionId, command.TargetId));
                session.GameplayState = gameplayResult.State;
                session.GameplayEvents.AddRange(gameplayResult.Events);
                gameplayEvents.AddRange(gameplayResult.Events);
                diagnostics.AddRange(gameplayResult.Diagnostics);
                success = success && gameplayResult.Success;
            }
            else if (mapResult.Events.Any(e => e.Type == RuntimeEventType.InteractionTriggered))
            {
                var message = "Map interaction has no InteractionDefinition route.";
                var runtimeEvent = RuntimeStateHelpers.Event(GameRuntimeEventType.LogMessageAdded, message, command.TargetId);
                session.GameplayEvents.Add(runtimeEvent);
                gameplayEvents.Add(runtimeEvent);
            }
        }

        return new UnifiedRuntimeResult
        {
            Success = success,
            Session = session,
            MapEvents = mapResult.Events.ToList(),
            GameplayEvents = gameplayEvents,
            Diagnostics = diagnostics,
            Message = mapResult.Success ? "Player command executed." : "Player command failed."
        };
    }

    public UnifiedRuntimeResult ExecuteGameplayCommand(GamePackageDefinition package, UnifiedRuntimeSession session, GameRuntimeCommand command)
    {
        var result = _gameplayRuntime.Execute(package, session.GameplayState, command);
        session.GameplayState = result.State;
        session.GameplayEvents.AddRange(result.Events);

        return new UnifiedRuntimeResult
        {
            Success = result.Success,
            Session = session,
            GameplayEvents = result.Events.ToList(),
            Diagnostics = result.Diagnostics.ToList(),
            Message = result.Message
        };
    }

    public UnifiedRuntimeResult ExecuteMany(GamePackageDefinition package, UnifiedRuntimeSession session, IEnumerable<GameRuntimeCommand> commands)
    {
        var aggregate = new UnifiedRuntimeResult { Session = session, Success = true };
        foreach (var command in commands)
        {
            var result = ExecuteGameplayCommand(package, session, command);
            aggregate.GameplayEvents.AddRange(result.GameplayEvents);
            aggregate.Diagnostics.AddRange(result.Diagnostics);
            aggregate.Success = aggregate.Success && result.Success;
            aggregate.Message = result.Message;
            if (!result.Success)
            {
                break;
            }
        }

        return aggregate;
    }

    private static string? ResolveInteractionId(GamePackageDefinition package, GameState state, PlayerCommand command, IEnumerable<RuntimeEvent> mapEvents)
    {
        if (!string.IsNullOrWhiteSpace(command.Payload))
        {
            return command.Payload.Trim();
        }

        foreach (var runtimeEvent in mapEvents.Where(e => e.Type == RuntimeEventType.InteractionTriggered))
        {
            if (!string.IsNullOrWhiteSpace(runtimeEvent.TargetId))
            {
                var interactionId = FindInteractionIdForEntity(package, state.CurrentMapId, runtimeEvent.TargetId);
                if (!string.IsNullOrWhiteSpace(interactionId))
                {
                    return interactionId;
                }
            }
        }

        return null;
    }

    private static string? FindInteractionIdForEntity(GamePackageDefinition package, string mapId, string entityId)
    {
        var map = package.Game.Maps.FirstOrDefault(m => RuntimeStateHelpers.IdEquals(m.Id, mapId));
        var entity = map?.Entities.FirstOrDefault(e => RuntimeStateHelpers.IdEquals(e.Id, entityId));
        if (entity == null)
        {
            return null;
        }

        var component = GetComponent(package, entity, "interactable");
        if (component != null && component.Args.TryGetValue("interactionId", out var interactionId))
        {
            return interactionId;
        }

        return package.Game.Interactions.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, entityId))?.Id;
    }

    private static ComponentDefinition? GetComponent(GamePackageDefinition package, EntityInstanceDefinition entity, string componentType)
    {
        var local = entity.Components.FirstOrDefault(c => RuntimeStateHelpers.KindEquals(c.Type, componentType));
        if (local != null)
        {
            return local;
        }

        var prototype = package.Game.EntityPrototypes.FirstOrDefault(p => RuntimeStateHelpers.IdEquals(p.Id, entity.PrototypeId));
        return prototype?.Components.FirstOrDefault(c => RuntimeStateHelpers.KindEquals(c.Type, componentType));
    }
}
