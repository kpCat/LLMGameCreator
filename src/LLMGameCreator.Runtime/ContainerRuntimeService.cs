using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class ContainerRuntimeService : IContainerRuntimeService
{
    public GameRuntimeResult OpenContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId)
    {
        var container = FindContainer(package, state, containerInventoryId);
        if (container == null)
        {
            return Failure(state, "container.missing", $"Container inventory not found: {containerInventoryId}", containerInventoryId);
        }

        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = $"Container opened: {container.Id}",
            Events = new List<GameRuntimeEvent>
            {
                RuntimeStateHelpers.Event(
                    GameRuntimeEventType.ContainerOpened,
                    $"Container opened: {container.Id}",
                    container.Id,
                    new Dictionary<string, string>
                    {
                        ["itemCount"] = container.Stacks.Count.ToString(),
                        ["contents"] = string.Join(",", container.Stacks.Select(stack => $"{stack.ItemId}:{stack.Amount:0.####}"))
                    })
            }
        };
    }

    public GameRuntimeResult TakeFromContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId, string itemId, double amount, string? playerInventoryId = null)
    {
        return Transfer(package, state, containerInventoryId, playerInventoryId, itemId, amount, fromContainer: true);
    }

    public GameRuntimeResult DepositToContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId, string itemId, double amount, string? playerInventoryId = null)
    {
        return Transfer(package, state, containerInventoryId, playerInventoryId, itemId, amount, fromContainer: false);
    }

    private static GameRuntimeResult Transfer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId, string? playerInventoryId, string itemId, double amount, bool fromContainer)
    {
        if (amount <= 0)
        {
            return Failure(state, "container.amount.invalid", "Transfer amount must be positive.", itemId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var container = FindContainer(package, working, containerInventoryId);
        var player = RuntimeStateHelpers.FindInventory(working, playerInventoryId) ?? RuntimeStateHelpers.EnsurePlayerInventory(working);
        if (container == null)
        {
            return Failure(state, "container.missing", $"Container inventory not found: {containerInventoryId}", containerInventoryId);
        }

        var source = fromContainer ? container : player;
        var target = fromContainer ? player : container;
        var taken = RuntimeStateHelpers.TakeItemStacks(source, itemId, amount);
        if (taken == null)
        {
            return Failure(state, "container.item_missing", $"Missing item {itemId} x{Format(amount)}", itemId);
        }

        foreach (var stack in taken)
        {
            RuntimeStateHelpers.AddStack(target, stack);
        }

        RuntimeStateHelpers.CopyState(working, state);
        var message = fromContainer
            ? $"Took {itemId} x{Format(amount)} from {container.Id}."
            : $"Deposited {itemId} x{Format(amount)} to {container.Id}.";

        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = message,
            Events = new List<GameRuntimeEvent>
            {
                RuntimeStateHelpers.Event(GameRuntimeEventType.ItemTransferred, message, itemId, new Dictionary<string, string> { ["containerId"] = container.Id, ["amount"] = Format(amount) }),
                RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {source.Id}", source.Id),
                RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {target.Id}", target.Id)
            }
        };
    }

    private static InventoryState? FindContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId)
    {
        var inventory = RuntimeStateHelpers.FindInventory(state, containerInventoryId);
        if (inventory == null)
        {
            return null;
        }

        var definition = package.Game.Inventories.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, inventory.Id));
        var isContainer = RuntimeStateHelpers.KindEquals(inventory.OwnerKind, "container")
            || RuntimeStateHelpers.KindEquals(definition?.OwnerKind, "container")
            || inventory.Metadata.TryGetValue("container", out var runtimeFlag) && runtimeFlag.Equals("true", StringComparison.OrdinalIgnoreCase)
            || definition?.Tags.Any(tag => RuntimeStateHelpers.KindEquals(tag, "container")) == true
            || definition?.Metadata.TryGetValue("container", out var definitionFlag) == true && definitionFlag.Equals("true", StringComparison.OrdinalIgnoreCase);

        return isContainer ? inventory : null;
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string? targetId)
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
