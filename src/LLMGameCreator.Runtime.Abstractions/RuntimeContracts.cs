using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Runtime.Abstractions;

public enum PlayerCommandType
{
    Move = 0,
    Interact = 1,
    UseItem = 2,
    UseAbility = 3,
    Wait = 4,
    ChooseDialogueOption = 5
}

public enum Direction2D
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

public sealed class PlayerCommand
{
    public PlayerCommandType Type { get; set; }
    public Direction2D Direction { get; set; } = Direction2D.None;
    public string? TargetId { get; set; }
    public string? Payload { get; set; }

    public static PlayerCommand Move(Direction2D direction) => new PlayerCommand { Type = PlayerCommandType.Move, Direction = direction };
    public static PlayerCommand Interact() => new PlayerCommand { Type = PlayerCommandType.Interact };
}

public sealed class GameState
{
    public string CurrentMapId { get; set; } = string.Empty;
    public Position2D PlayerPosition { get; set; } = new Position2D();
    public string Mode { get; set; } = "map";
    public Dictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();
}

public enum RuntimeEventType
{
    Message = 0,
    PlayerMoved = 1,
    MovementBlocked = 2,
    InteractionTriggered = 3,
    DialogueRequested = 4,
    SoundRequested = 5,
    MusicRequested = 6,
    Error = 7
}

public sealed class RuntimeEvent
{
    public RuntimeEventType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}

public sealed class CommandResult
{
    public GameState State { get; set; } = new GameState();
    public List<RuntimeEvent> Events { get; set; } = new List<RuntimeEvent>();
    public bool Success { get; set; } = true;
}

public interface IGameRuntime
{
    CommandResult Start(GamePackageDefinition package);
    CommandResult Execute(GamePackageDefinition package, GameState state, PlayerCommand command);
}

public interface IChunkGenerator
{
    string Id { get; }
}
