using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

internal static class RuntimeStateHelpers
{
    public const string DefaultPlayerInventoryId = "inventory/player";
    public const string DefaultPlayerEntityId = "player";

    public static InventoryState EnsurePlayerInventory(GameRuntimeState state)
    {
        var inventory = state.Inventories.FirstOrDefault(i => IdEquals(i.Id, DefaultPlayerInventoryId))
            ?? state.Inventories.FirstOrDefault(i => KindEquals(i.OwnerKind, "player"));

        if (inventory != null)
        {
            return inventory;
        }

        inventory = new InventoryState
        {
            Id = DefaultPlayerInventoryId,
            OwnerKind = "player",
            OwnerId = state.PlayerEntityId
        };
        state.Inventories.Add(inventory);
        return inventory;
    }

    public static InventoryState? FindInventory(GameRuntimeState state, string? inventoryId)
    {
        if (!string.IsNullOrWhiteSpace(inventoryId))
        {
            return state.Inventories.FirstOrDefault(i => IdEquals(i.Id, inventoryId));
        }

        return state.Inventories.FirstOrDefault(i => KindEquals(i.OwnerKind, "player"))
            ?? state.Inventories.FirstOrDefault();
    }

    public static double GetItemAmount(InventoryState? inventory, string itemId)
    {
        if (inventory == null)
        {
            return 0;
        }

        return inventory.Stacks.Where(s => IdEquals(s.ItemId, itemId)).Sum(s => s.Amount);
    }

    public static void AddItem(InventoryState inventory, string itemId, double amount, bool questItem = false, string? uniqueInstanceId = null)
    {
        if (amount <= 0)
        {
            return;
        }

        var stack = string.IsNullOrWhiteSpace(uniqueInstanceId)
            ? inventory.Stacks.FirstOrDefault(s => IdEquals(s.ItemId, itemId) && string.IsNullOrWhiteSpace(s.UniqueInstanceId))
            : null;

        if (stack == null)
        {
            inventory.Stacks.Add(new ItemStackState
            {
                ItemId = itemId,
                Amount = amount,
                QuestItem = questItem,
                UniqueInstanceId = uniqueInstanceId
            });
            return;
        }

        stack.Amount += amount;
        stack.QuestItem = stack.QuestItem || questItem;
    }

    public static bool RemoveItem(InventoryState inventory, string itemId, double amount)
    {
        if (GetItemAmount(inventory, itemId) < amount)
        {
            return false;
        }

        var remaining = amount;
        foreach (var stack in inventory.Stacks.Where(s => IdEquals(s.ItemId, itemId)).ToList())
        {
            if (remaining <= 0)
            {
                break;
            }

            var consumed = Math.Min(stack.Amount, remaining);
            stack.Amount -= consumed;
            remaining -= consumed;
            if (stack.Amount <= 0)
            {
                inventory.Stacks.Remove(stack);
            }
        }

        return true;
    }

    public static ResourceState EnsureResource(GameRuntimeState state, ResourceDefinition definition, string? scope = null, string? ownerId = null)
    {
        var resourceScope = NormalizeScope(scope);
        var resource = state.Resources.FirstOrDefault(r =>
            IdEquals(r.ResourceId, definition.Id)
            && IdEquals(r.Scope, resourceScope)
            && IdEquals(r.OwnerId, ownerId));

        if (resource != null)
        {
            return resource;
        }

        resource = new ResourceState
        {
            ResourceId = definition.Id,
            Scope = resourceScope,
            OwnerId = ownerId,
            Capacity = definition.MaxValue,
            Amount = Clamp(definition.DefaultValue ?? definition.MinValue ?? 0, definition.MinValue, definition.MaxValue)
        };
        state.Resources.Add(resource);
        return resource;
    }

    public static ResourceState? FindResource(GameRuntimeState state, string resourceId, string? scope = null, string? ownerId = null)
    {
        var resourceScope = NormalizeScope(scope);
        return state.Resources.FirstOrDefault(r =>
            IdEquals(r.ResourceId, resourceId)
            && IdEquals(r.Scope, resourceScope)
            && IdEquals(r.OwnerId, ownerId));
    }

    public static double GetResourceAmount(GameRuntimeState state, string resourceId, string? scope = null, string? ownerId = null)
    {
        return FindResource(state, resourceId, scope, ownerId)?.Amount ?? 0;
    }

    public static void ChangeResource(GameRuntimeState state, ResourceDefinition definition, double delta, string? scope = null, string? ownerId = null)
    {
        var resource = EnsureResource(state, definition, scope, ownerId);
        var max = resource.Capacity ?? definition.MaxValue;
        resource.Amount = Clamp(resource.Amount + delta, definition.MinValue, max);
    }

    public static string GetFlagValue(GameRuntimeState state, string flagId)
    {
        return state.Flags.FirstOrDefault(f => IdEquals(f.Id, flagId))?.Value ?? string.Empty;
    }

    public static void SetFlag(GameRuntimeState state, string flagId, string value)
    {
        var flag = state.Flags.FirstOrDefault(f => IdEquals(f.Id, flagId));
        if (flag == null)
        {
            state.Flags.Add(new RuntimeFlagState { Id = flagId, Value = value });
            return;
        }

        flag.Value = value;
    }

    public static bool HasStatus(GameRuntimeState state, string statusId, string? targetId = null)
    {
        return state.Statuses.Any(s => IdEquals(s.StatusId, statusId) && (string.IsNullOrWhiteSpace(targetId) || IdEquals(s.TargetId, targetId)));
    }

    public static bool IsUniqueLootAlreadyAcquired(GameRuntimeState state, string entryId)
    {
        return state.Metadata.TryGetValue(UniqueLootKey(entryId), out var value)
            && int.TryParse(value, out var count)
            && count > 0;
    }

    public static int GetGlobalLootCount(GameRuntimeState state, string entryId)
    {
        return state.Metadata.TryGetValue(UniqueLootKey(entryId), out var value) && int.TryParse(value, out var count) ? count : 0;
    }

    public static void IncrementGlobalLootCount(GameRuntimeState state, string entryId)
    {
        state.Metadata[UniqueLootKey(entryId)] = (GetGlobalLootCount(state, entryId) + 1).ToString();
    }

    public static RuntimeDiagnostic Diagnostic(string code, string message, string? targetId = null, string severity = "error")
    {
        return new RuntimeDiagnostic
        {
            Code = code,
            Severity = severity,
            Message = message,
            TargetId = targetId
        };
    }

    public static GameRuntimeEvent Event(GameRuntimeEventType type, string message, string? targetId = null, Dictionary<string, string>? args = null)
    {
        return new GameRuntimeEvent
        {
            Type = type,
            Message = message,
            TargetId = targetId,
            Args = args ?? new Dictionary<string, string>()
        };
    }

    public static int StableSeed(string text)
    {
        unchecked
        {
            var hash = 17;
            foreach (var ch in text)
            {
                hash = (hash * 31) + ch;
            }

            return hash;
        }
    }

    public static GameRuntimeState CloneState(GameRuntimeState state)
    {
        return new GameRuntimeState
        {
            PackageId = state.PackageId,
            CurrentMapId = state.CurrentMapId,
            PlayerEntityId = state.PlayerEntityId,
            Tick = state.Tick,
            QuestStates = new Dictionary<string, string>(state.QuestStates),
            Metadata = new Dictionary<string, string>(state.Metadata),
            Inventories = state.Inventories.Select(inventory => new InventoryState
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
                    QuestItem = stack.QuestItem,
                    Durability = stack.Durability,
                    Charge = stack.Charge,
                    Metadata = new Dictionary<string, string>(stack.Metadata)
                }).ToList()
            }).ToList(),
            Resources = state.Resources.Select(resource => new ResourceState
            {
                ResourceId = resource.ResourceId,
                Amount = resource.Amount,
                Capacity = resource.Capacity,
                Scope = resource.Scope,
                OwnerId = resource.OwnerId
            }).ToList(),
            Flags = state.Flags.Select(flag => new RuntimeFlagState
            {
                Id = flag.Id,
                Value = flag.Value
            }).ToList(),
            Statuses = state.Statuses.Select(status => new StatusState
            {
                StatusId = status.StatusId,
                TargetId = status.TargetId,
                RemainingTicks = status.RemainingTicks,
                Stacks = status.Stacks,
                Metadata = new Dictionary<string, string>(status.Metadata)
            }).ToList()
        };
    }

    public static void CopyState(GameRuntimeState source, GameRuntimeState target)
    {
        target.PackageId = source.PackageId;
        target.CurrentMapId = source.CurrentMapId;
        target.PlayerEntityId = source.PlayerEntityId;
        target.Tick = source.Tick;
        target.Inventories = source.Inventories;
        target.Resources = source.Resources;
        target.Flags = source.Flags;
        target.Statuses = source.Statuses;
        target.QuestStates = source.QuestStates;
        target.Metadata = source.Metadata;
    }

    public static bool KindEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IdEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
    }

    public static double Clamp(double value, double? min, double? max)
    {
        if (min.HasValue && value < min.Value)
        {
            return min.Value;
        }

        if (max.HasValue && value > max.Value)
        {
            return max.Value;
        }

        return value;
    }

    private static string NormalizeScope(string? scope)
    {
        return string.IsNullOrWhiteSpace(scope) ? "global" : scope.Trim();
    }

    private static string UniqueLootKey(string entryId)
    {
        return $"loot:{entryId}:count";
    }
}
