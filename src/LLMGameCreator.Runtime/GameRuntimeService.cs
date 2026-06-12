using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class GameRuntimeService : IGameRuntimeService
{
    private readonly IGameRuntimeStateFactory _stateFactory;
    private readonly IRecipeRuntimeService _recipeRuntimeService;
    private readonly ILootRuntimeService _lootRuntimeService;
    private readonly ITransactionRuntimeService _transactionRuntimeService;
    private readonly IResourceNetworkRuntimeService _resourceNetworkRuntimeService;
    private readonly IUseItemRuntimeService _useItemRuntimeService;
    private readonly IInteractionRuntimeService _interactionRuntimeService;

    public GameRuntimeService(
        IGameRuntimeStateFactory stateFactory,
        IRecipeRuntimeService recipeRuntimeService,
        ILootRuntimeService lootRuntimeService,
        ITransactionRuntimeService transactionRuntimeService,
        IResourceNetworkRuntimeService resourceNetworkRuntimeService,
        IUseItemRuntimeService useItemRuntimeService,
        IInteractionRuntimeService interactionRuntimeService)
    {
        _stateFactory = stateFactory;
        _recipeRuntimeService = recipeRuntimeService;
        _lootRuntimeService = lootRuntimeService;
        _transactionRuntimeService = transactionRuntimeService;
        _resourceNetworkRuntimeService = resourceNetworkRuntimeService;
        _useItemRuntimeService = useItemRuntimeService;
        _interactionRuntimeService = interactionRuntimeService;
    }

    public GameRuntimeResult CreateInitialState(GamePackageDefinition package)
    {
        return _stateFactory.CreateInitialState(package);
    }

    public GameRuntimeResult Execute(GamePackageDefinition package, GameRuntimeState state, GameRuntimeCommand command)
    {
        switch (command.Type)
        {
            case GameRuntimeCommandType.Initialize:
                return CreateInitialState(package);
            case GameRuntimeCommandType.AddItem:
                return AddItem(state, command);
            case GameRuntimeCommandType.RemoveItem:
                return RemoveItem(state, command);
            case GameRuntimeCommandType.ChangeResource:
                return ChangeResource(package, state, command);
            case GameRuntimeCommandType.CraftRecipe:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires recipe id.", null)
                    : _recipeRuntimeService.CraftRecipe(package, state, command.Id.Trim(), command.InventoryId);
            case GameRuntimeCommandType.RollLootTable:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires loot table id.", null)
                    : _lootRuntimeService.RollLootTable(package, state, command.Id.Trim(), command.InventoryId, command.Seed);
            case GameRuntimeCommandType.ExecuteTransaction:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires transaction id.", null)
                    : _transactionRuntimeService.ExecuteTransaction(package, state, command.Id.Trim(), command.InventoryId);
            case GameRuntimeCommandType.TickResourceNodes:
            case GameRuntimeCommandType.Wait:
                return _resourceNetworkRuntimeService.TickResourceNodes(package, state, Math.Max(1, command.Ticks));
            case GameRuntimeCommandType.SetFlag:
                return SetFlag(state, command);
            case GameRuntimeCommandType.UseItem:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires item id.", null)
                    : _useItemRuntimeService.UseItem(package, state, command.Id.Trim(), command.InventoryId, command.TargetId);
            case GameRuntimeCommandType.ExecuteInteraction:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires interaction id.", null)
                    : _interactionRuntimeService.ExecuteInteraction(package, state, command.Id.Trim(), command.TargetId, command.InventoryId);
            default:
                return Fail(state, "runtime.command.unknown", $"Unknown runtime command: {command.Type}", command.Type.ToString());
        }
    }

    public GameRuntimeResult ExecuteMany(GamePackageDefinition package, GameRuntimeState state, IEnumerable<GameRuntimeCommand> commands)
    {
        var aggregate = new GameRuntimeResult { State = state, Success = true };
        foreach (var command in commands)
        {
            var result = Execute(package, state, command);
            aggregate.Events.AddRange(result.Events);
            aggregate.Diagnostics.AddRange(result.Diagnostics);
            aggregate.State = result.State;
            aggregate.Success = aggregate.Success && result.Success;
            aggregate.Message = result.Message;
            if (!result.Success)
            {
                break;
            }
        }

        return aggregate;
    }

    private static GameRuntimeResult AddItem(GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires item id.", null);
        }

        var itemId = command.Id.Trim();
        var amount = command.Amount <= 0 ? 1 : command.Amount;
        var inventory = RuntimeStateHelpers.FindInventory(state, command.InventoryId) ?? RuntimeStateHelpers.EnsurePlayerInventory(state);
        RuntimeStateHelpers.AddItem(inventory, itemId, amount);
        return Success(state, $"Added item {itemId} x{Format(amount)}", GameRuntimeEventType.InventoryChanged, inventory.Id);
    }

    private static GameRuntimeResult RemoveItem(GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires item id.", null);
        }

        var itemId = command.Id.Trim();
        var amount = command.Amount <= 0 ? 1 : command.Amount;
        var inventory = RuntimeStateHelpers.FindInventory(state, command.InventoryId);
        if (inventory == null || !RuntimeStateHelpers.RemoveItem(inventory, itemId, amount))
        {
            return Fail(state, "runtime.item_missing", $"Missing item {itemId} x{Format(amount)}", itemId);
        }

        return Success(state, $"Removed item {itemId} x{Format(amount)}", GameRuntimeEventType.InventoryChanged, inventory.Id);
    }

    private static GameRuntimeResult ChangeResource(GamePackageDefinition package, GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires resource id.", null);
        }

        var resourceId = command.Id.Trim();
        var resource = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, resourceId));
        if (resource == null)
        {
            return Fail(state, "runtime.resource_missing", $"Resource not found: {resourceId}", resourceId);
        }

        RuntimeStateHelpers.ChangeResource(state, resource, command.Amount);
        return Success(state, $"Changed resource {resourceId} by {Format(command.Amount)}", GameRuntimeEventType.ResourceChanged, resourceId);
    }

    private static GameRuntimeResult SetFlag(GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires flag id.", null);
        }

        var flagId = command.Id.Trim();
        var value = command.Value ?? "true";
        RuntimeStateHelpers.SetFlag(state, flagId, value);
        return Success(state, $"Set flag {flagId} = {value}", GameRuntimeEventType.OutputApplied, flagId);
    }

    private static GameRuntimeResult Success(GameRuntimeState state, string message, GameRuntimeEventType eventType, string? targetId)
    {
        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = message,
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(eventType, message, targetId) }
        };
    }

    private static GameRuntimeResult Fail(GameRuntimeState state, string code, string message, string? targetId)
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

    private static string Format(double value)
    {
        return value.ToString("0.####");
    }
}
