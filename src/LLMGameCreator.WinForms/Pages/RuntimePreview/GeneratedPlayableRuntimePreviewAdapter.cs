using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.WinForms.Pages;

public sealed class GeneratedPlayableRuntimePreviewAdapter : IVisibleGeneratedPlayableRuntimeAdapter
{
    private readonly IGameRuntime _runtime;

    public GeneratedPlayableRuntimePreviewAdapter(IGameRuntime runtime)
    {
        _runtime = runtime;
    }

    public VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package)
    {
        var start = _runtime.Start(package);
        var startPosition = new VisibleGeneratedPlayablePosition
        {
            X = start.State.PlayerPosition.X,
            Y = start.State.PlayerPosition.Y
        };
        var eventTypes = new SortedSet<string>(start.Events.Select(item => item.Type.ToString()), StringComparer.Ordinal);
        var commandAttempts = new List<VisibleGeneratedPlayableRuntimeCommandAttempt>();
        var currentState = start.State;

        if (start.Success)
        {
            var move = _runtime.Execute(package, currentState, PlayerCommand.Move(Direction2D.Right));
            currentState = move.State;
            commandAttempts.Add(ToAttempt("01_move_right", "move/right", move));
            foreach (var eventType in move.Events.Select(item => item.Type.ToString()))
            {
                eventTypes.Add(eventType);
            }

            var interact = _runtime.Execute(package, currentState, PlayerCommand.Interact());
            currentState = interact.State;
            commandAttempts.Add(ToAttempt("02_interact", "interact", interact));
            foreach (var eventType in interact.Events.Select(item => item.Type.ToString()))
            {
                eventTypes.Add(eventType);
            }
        }

        return new VisibleGeneratedPlayableRuntimeAttempt
        {
            RuntimeStartAttempted = true,
            RuntimeStartSucceeded = start.Success,
            StartMapId = package.Manifest.StartMapId,
            CurrentMapId = currentState.CurrentMapId,
            PlayerStartPosition = startPosition,
            PlayerCurrentPosition = new VisibleGeneratedPlayablePosition
            {
                X = currentState.PlayerPosition.X,
                Y = currentState.PlayerPosition.Y
            },
            CommandAttempts = commandAttempts,
            EventTypes = eventTypes.ToList()
        };
    }

    private static VisibleGeneratedPlayableRuntimeCommandAttempt ToAttempt(
        string commandId,
        string commandType,
        CommandResult result) => new()
    {
        CommandId = commandId,
        CommandType = commandType,
        Succeeded = result.Success,
        CurrentMapId = result.State.CurrentMapId,
        PlayerPosition = new VisibleGeneratedPlayablePosition
        {
            X = result.State.PlayerPosition.X,
            Y = result.State.PlayerPosition.Y
        },
        EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList()
    };
}
