using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class DefaultGameRuntime : IGameRuntime
{
    public CommandResult Start(GamePackageDefinition package)
    {
        var map = package.Game.Maps.FirstOrDefault(m => m.Id == package.Manifest.StartMapId)
                  ?? package.Game.Maps.FirstOrDefault();

        if (map == null)
        {
            return Error(new GameState(), "В пакете игры нет карт.");
        }

        var state = new GameState
        {
            CurrentMapId = map.Id,
            PlayerPosition = new Position2D(map.StartPosition.X, map.StartPosition.Y),
            Mode = "map"
        };

        return new CommandResult
        {
            State = state,
            Events = new List<RuntimeEvent>
            {
                new RuntimeEvent { Type = RuntimeEventType.Message, Message = $"Игра запущена: {package.Manifest.Title}" }
            }
        };
    }

    public CommandResult Execute(GamePackageDefinition package, GameState state, PlayerCommand command)
    {
        if (command.Type == PlayerCommandType.Move)
        {
            return Move(package, state, command.Direction);
        }

        if (command.Type == PlayerCommandType.Interact)
        {
            return Interact(package, state);
        }

        return new CommandResult
        {
            State = state,
            Success = false,
            Events = new List<RuntimeEvent>
            {
                new RuntimeEvent { Type = RuntimeEventType.Error, Message = $"Команда пока не реализована: {command.Type}" }
            }
        };
    }

    private static CommandResult Move(GamePackageDefinition package, GameState state, Direction2D direction)
    {
        var map = FindCurrentMap(package, state);
        if (map == null)
        {
            return Error(state, "Текущая карта не найдена.");
        }

        var dx = direction == Direction2D.Left ? -1 : direction == Direction2D.Right ? 1 : 0;
        var dy = direction == Direction2D.Up ? -1 : direction == Direction2D.Down ? 1 : 0;
        var next = new Position2D(state.PlayerPosition.X + dx, state.PlayerPosition.Y + dy);

        if (next.X < 0 || next.Y < 0 || next.X >= map.Width || next.Y >= map.Height)
        {
            return Blocked(state, "Край карты.");
        }

        var tileId = GetTileId(map, next.X, next.Y);
        var tile = package.Game.TilePrototypes.FirstOrDefault(t => t.Id == tileId);
        if (tile == null || !tile.Walkable)
        {
            return Blocked(state, "Туда нельзя пройти.");
        }

        var blockingEntity = map.Entities.FirstOrDefault(e => e.Position.X == next.X && e.Position.Y == next.Y && HasComponent(package, e, "collidable"));
        if (blockingEntity != null)
        {
            return Blocked(state, $"Путь блокирует: {blockingEntity.Id}");
        }

        state.PlayerPosition = next;
        return new CommandResult
        {
            State = state,
            Events = new List<RuntimeEvent>
            {
                new RuntimeEvent
                {
                    Type = RuntimeEventType.PlayerMoved,
                    Message = $"Игрок переместился: {next.X}, {next.Y}",
                    Args = new Dictionary<string, string> { ["x"] = next.X.ToString(), ["y"] = next.Y.ToString() }
                }
            }
        };
    }

    private static CommandResult Interact(GamePackageDefinition package, GameState state)
    {
        var map = FindCurrentMap(package, state);
        if (map == null)
        {
            return Error(state, "Текущая карта не найдена.");
        }

        var nearby = map.Entities.FirstOrDefault(e => IsAdjacent(state.PlayerPosition, e.Position) && HasComponent(package, e, "interactable"));
        if (nearby == null)
        {
            return new CommandResult
            {
                State = state,
                Events = new List<RuntimeEvent> { new RuntimeEvent { Type = RuntimeEventType.Message, Message = "Рядом нет объекта для взаимодействия." } }
            };
        }

        var interactionComponent = GetComponent(package, nearby, "interactable");
        var events = new List<RuntimeEvent>
        {
            new RuntimeEvent { Type = RuntimeEventType.InteractionTriggered, TargetId = nearby.Id, Message = $"Взаимодействие: {nearby.Id}" }
        };

        if (interactionComponent != null && interactionComponent.Args.TryGetValue("dialogueId", out var dialogueId))
        {
            events.Add(new RuntimeEvent
            {
                Type = RuntimeEventType.DialogueRequested,
                TargetId = dialogueId,
                Message = $"Открыть диалог: {dialogueId}"
            });
        }
        else if (interactionComponent != null && interactionComponent.Args.TryGetValue("text", out var text))
        {
            events.Add(new RuntimeEvent { Type = RuntimeEventType.Message, Message = text });
        }

        return new CommandResult { State = state, Events = events };
    }

    private static MapDefinition? FindCurrentMap(GamePackageDefinition package, GameState state)
    {
        return package.Game.Maps.FirstOrDefault(m => m.Id == state.CurrentMapId);
    }

    private static string GetTileId(MapDefinition map, int x, int y)
    {
        return map.Tiles.FirstOrDefault(t => t.X == x && t.Y == y)?.TileId ?? map.DefaultTileId;
    }

    private static bool HasComponent(GamePackageDefinition package, EntityInstanceDefinition entity, string componentType)
    {
        return GetComponent(package, entity, componentType) != null;
    }

    private static ComponentDefinition? GetComponent(GamePackageDefinition package, EntityInstanceDefinition entity, string componentType)
    {
        var local = entity.Components.FirstOrDefault(c => c.Type == componentType);
        if (local != null)
        {
            return local;
        }

        var prototype = package.Game.EntityPrototypes.FirstOrDefault(p => p.Id == entity.PrototypeId);
        return prototype?.Components.FirstOrDefault(c => c.Type == componentType);
    }

    private static bool IsAdjacent(Position2D a, Position2D b)
    {
        var distance = Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        return distance == 1;
    }

    private static CommandResult Blocked(GameState state, string message)
    {
        return new CommandResult
        {
            State = state,
            Success = false,
            Events = new List<RuntimeEvent> { new RuntimeEvent { Type = RuntimeEventType.MovementBlocked, Message = message } }
        };
    }

    private static CommandResult Error(GameState state, string message)
    {
        return new CommandResult
        {
            State = state,
            Success = false,
            Events = new List<RuntimeEvent> { new RuntimeEvent { Type = RuntimeEventType.Error, Message = message } }
        };
    }
}
