using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class GameRuntimeStateFactory : IGameRuntimeStateFactory
{
    public GameRuntimeResult CreateInitialState(GamePackageDefinition package)
    {
        var state = new GameRuntimeState
        {
            PackageId = package.Manifest.PackageId,
            CurrentMapId = package.Manifest.StartMapId,
            PlayerEntityId = RuntimeStateHelpers.DefaultPlayerEntityId,
            Tick = 0
        };

        var map = package.Game.Maps.FirstOrDefault(m => RuntimeStateHelpers.IdEquals(m.Id, package.Manifest.StartMapId))
            ?? package.Game.Maps.FirstOrDefault();
        if (map != null)
        {
            state.CurrentMapId = map.Id;
        }

        foreach (var inventory in package.Game.Inventories)
        {
            state.Inventories.Add(new InventoryState
            {
                Id = inventory.Id,
                OwnerKind = inventory.OwnerKind,
                OwnerId = inventory.OwnerId,
                Metadata = new Dictionary<string, string>(inventory.Metadata),
                Stacks = inventory.Stacks.Select(stack => new ItemStackState
                {
                    ItemId = stack.ItemId,
                    Amount = stack.Amount,
                    UniqueInstanceId = stack.UniqueInstanceId,
                    QuestItem = stack.QuestItem ?? false,
                    Durability = stack.Durability,
                    Charge = stack.Charge,
                    Metadata = new Dictionary<string, string>(stack.Metadata)
                }).ToList()
            });
        }

        RuntimeStateHelpers.EnsurePlayerInventory(state);
        RuntimeStateHelpers.EnsurePlayerEquipment(state);

        foreach (var resource in package.Game.Resources)
        {
            RuntimeStateHelpers.EnsureResource(state, resource);
        }

        var result = new GameRuntimeResult
        {
            State = state,
            Message = "Runtime state initialized."
        };
        result.Events.Add(RuntimeStateHelpers.Event(
            GameRuntimeEventType.GameStarted,
            $"Game started: {package.Manifest.Title}",
            package.Manifest.PackageId));

        if (map == null)
        {
            result.Success = false;
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("runtime.map.missing", "Package has no map to start runtime.", package.Manifest.PackageId));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, "Package has no map to start runtime.", package.Manifest.PackageId));
        }

        return result;
    }
}
