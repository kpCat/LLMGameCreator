using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class CostConsumer : ICostConsumer
{
    public CostConsumptionResult Consume(GamePackageDefinition package, GameRuntimeState state, IEnumerable<CostDefinition> costs, string? inventoryId = null)
    {
        var costList = costs.ToList();
        var result = Validate(package, state, costList, inventoryId);
        if (!result.Success)
        {
            return result;
        }

        foreach (var cost in costList)
        {
            ConsumeOne(package, state, cost, inventoryId, result);
        }

        return result;
    }

    private static CostConsumptionResult Validate(GamePackageDefinition package, GameRuntimeState state, IEnumerable<CostDefinition> costs, string? inventoryId)
    {
        var result = new CostConsumptionResult();
        foreach (var cost in costs)
        {
            if (cost.Amount <= 0)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("cost.amount.invalid", "Cost amount must be positive.", cost.Id));
                continue;
            }

            if (IsItemCost(cost))
            {
                var inventory = RuntimeStateHelpers.FindInventory(state, cost.Scope ?? inventoryId);
                var has = RuntimeStateHelpers.GetItemAmount(inventory, cost.Id);
                if (has < cost.Amount)
                {
                    result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("cost.item_missing", $"Missing item {cost.Id} x{Format(cost.Amount)}", cost.Id));
                }

                continue;
            }

            if (IsResourceCost(cost))
            {
                var has = RuntimeStateHelpers.GetResourceAmount(state, cost.Id, cost.Scope);
                if (has < cost.Amount)
                {
                    result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("cost.resource_too_low", $"Resource {cost.Id} requires {Format(cost.Amount)}, has {Format(has)}", cost.Id));
                }

                continue;
            }

            if (RuntimeStateHelpers.KindEquals(cost.Kind, "time") || RuntimeStateHelpers.KindEquals(cost.Kind, "tick"))
            {
                continue;
            }

            if (RuntimeStateHelpers.KindEquals(cost.Kind, "durability") || RuntimeStateHelpers.KindEquals(cost.Kind, "charge"))
            {
                var inventory = RuntimeStateHelpers.FindInventory(state, cost.Scope ?? inventoryId);
                var stack = inventory?.Stacks.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.ItemId, cost.Id));
                var value = RuntimeStateHelpers.KindEquals(cost.Kind, "durability") ? stack?.Durability : stack?.Charge;
                if (!value.HasValue || value.Value < cost.Amount)
                {
                    result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("cost.item_meter_too_low", $"{cost.Kind} {cost.Id} requires {Format(cost.Amount)}, has {Format(value ?? 0)}", cost.Id));
                }

                continue;
            }

            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("cost.kind.unknown", $"Unknown cost kind: {cost.Kind}", cost.Id));
        }

        return result;
    }

    private static void ConsumeOne(GamePackageDefinition package, GameRuntimeState state, CostDefinition cost, string? inventoryId, CostConsumptionResult result)
    {
        if (IsItemCost(cost))
        {
            var inventory = RuntimeStateHelpers.FindInventory(state, cost.Scope ?? inventoryId);
            if (inventory != null)
            {
                RuntimeStateHelpers.RemoveItem(inventory, cost.Id, cost.Amount);
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.CostConsumed, $"Consumed item {cost.Id} x{Format(cost.Amount)}", cost.Id));
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {inventory.Id}", inventory.Id));
            }

            return;
        }

        if (IsResourceCost(cost))
        {
            var definition = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, cost.Id));
            if (definition != null)
            {
                RuntimeStateHelpers.ChangeResource(state, definition, -cost.Amount, cost.Scope);
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.CostConsumed, $"Consumed resource {cost.Id} x{Format(cost.Amount)}", cost.Id));
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ResourceChanged, $"Resource changed: {cost.Id}", cost.Id));
            }

            return;
        }

        if (RuntimeStateHelpers.KindEquals(cost.Kind, "time") || RuntimeStateHelpers.KindEquals(cost.Kind, "tick"))
        {
            state.Tick += (long)Math.Ceiling(cost.Amount);
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.CostConsumed, $"Consumed time {Format(cost.Amount)} ticks", cost.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(cost.Kind, "durability") || RuntimeStateHelpers.KindEquals(cost.Kind, "charge"))
        {
            var inventory = RuntimeStateHelpers.FindInventory(state, cost.Scope ?? inventoryId);
            var stack = inventory?.Stacks.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.ItemId, cost.Id));
            if (stack != null)
            {
                if (RuntimeStateHelpers.KindEquals(cost.Kind, "durability"))
                {
                    stack.Durability -= cost.Amount;
                }
                else
                {
                    stack.Charge -= cost.Amount;
                }

                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.CostConsumed, $"Consumed {cost.Kind} {cost.Id} x{Format(cost.Amount)}", cost.Id));
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {inventory!.Id}", inventory.Id));
            }
        }
    }

    private static bool IsItemCost(CostDefinition cost)
    {
        return RuntimeStateHelpers.KindEquals(cost.Kind, "item") || RuntimeStateHelpers.KindEquals(cost.Kind, "inventory_item");
    }

    private static bool IsResourceCost(CostDefinition cost)
    {
        return RuntimeStateHelpers.KindEquals(cost.Kind, "resource")
            || RuntimeStateHelpers.KindEquals(cost.Kind, "network_resource")
            || RuntimeStateHelpers.KindEquals(cost.Kind, "abstract_resource")
            || RuntimeStateHelpers.KindEquals(cost.Kind, "faction")
            || RuntimeStateHelpers.KindEquals(cost.Kind, "reputation");
    }

    private static string Format(double value)
    {
        return value.ToString("0.####");
    }
}
