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
                var meter = FindMeterTarget(state, cost, inventoryId);
                var value = meter == null
                    ? null
                    : RuntimeStateHelpers.KindEquals(cost.Kind, "durability")
                        ? meter.Durability
                        : meter.Charge;
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
            var meter = FindMeterTarget(state, cost, inventoryId);
            if (meter != null)
            {
                if (RuntimeStateHelpers.KindEquals(cost.Kind, "durability"))
                {
                    meter.Durability -= cost.Amount;
                }
                else
                {
                    meter.Charge -= cost.Amount;
                }

                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.CostConsumed, $"Consumed {cost.Kind} {cost.Id} x{Format(cost.Amount)}", cost.Id));
                if (meter.Inventory != null)
                {
                    if (ShouldBreak(package, meter.Stack, meter.Durability, meter.Charge))
                    {
                        meter.Inventory.Stacks.Remove(meter.Stack!);
                        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LogMessageAdded, $"Item broke: {meter.Stack!.ItemId}", meter.Stack.ItemId));
                    }

                    result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {meter.Inventory.Id}", meter.Inventory.Id));
                }
                else if (meter.Slot != null)
                {
                    if (ShouldBreak(package, meter.Slot, meter.Durability, meter.Charge))
                    {
                        var brokenItemId = meter.Slot.ItemId;
                        RuntimeStateHelpers.ClearSlot(meter.Slot);
                        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LogMessageAdded, $"Equipped item broke: {brokenItemId}", brokenItemId));
                    }

                    result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.EquipmentChanged, $"Equipment changed: {meter.Slot.SlotId}", meter.Slot.SlotId));
                }
            }
        }
    }

    private static MeterTarget? FindMeterTarget(GameRuntimeState state, CostDefinition cost, string? inventoryId)
    {
        var inventory = RuntimeStateHelpers.FindInventory(state, cost.Scope ?? inventoryId);
        var stack = inventory?.Stacks.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.ItemId, cost.Id));
        if (stack != null)
        {
            return MeterTarget.FromStack(inventory!, stack);
        }

        foreach (var equipment in state.Equipment)
        {
            var slot = equipment.Slots.FirstOrDefault(s =>
                RuntimeStateHelpers.IdEquals(s.SlotId, cost.Id)
                || RuntimeStateHelpers.IdEquals(s.ItemId, cost.Id));
            if (slot != null)
            {
                return MeterTarget.FromSlot(slot);
            }
        }

        return null;
    }

    private static bool ShouldBreak(GamePackageDefinition package, ItemStackState? stack, double? durability, double? charge)
    {
        if (stack == null)
        {
            return false;
        }

        return IsBreakOnZero(package, stack.ItemId, stack.Metadata)
            && (durability.HasValue && durability.Value <= 0 || charge.HasValue && charge.Value <= 0);
    }

    private static bool ShouldBreak(GamePackageDefinition package, EquipmentSlotState slot, double? durability, double? charge)
    {
        return !string.IsNullOrWhiteSpace(slot.ItemId)
            && IsBreakOnZero(package, slot.ItemId!, slot.Metadata)
            && (durability.HasValue && durability.Value <= 0 || charge.HasValue && charge.Value <= 0);
    }

    private static bool IsBreakOnZero(GamePackageDefinition package, string itemId, Dictionary<string, string> stackMetadata)
    {
        if (stackMetadata.TryGetValue("break_on_zero", out var stackValue) && stackValue.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var item = package.Game.Items.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, itemId));
        return item?.Metadata.TryGetValue("break_on_zero", out var itemValue) == true && itemValue.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class MeterTarget
    {
        private MeterTarget()
        {
        }

        public InventoryState? Inventory { get; private set; }
        public ItemStackState? Stack { get; private set; }
        public EquipmentSlotState? Slot { get; private set; }
        public double? Durability
        {
            get => Stack?.Durability ?? Slot?.Durability;
            set
            {
                if (Stack != null)
                {
                    Stack.Durability = value;
                }
                else if (Slot != null)
                {
                    Slot.Durability = value;
                }
            }
        }

        public double? Charge
        {
            get => Stack?.Charge ?? Slot?.Charge;
            set
            {
                if (Stack != null)
                {
                    Stack.Charge = value;
                }
                else if (Slot != null)
                {
                    Slot.Charge = value;
                }
            }
        }

        public static MeterTarget FromStack(InventoryState inventory, ItemStackState stack)
        {
            return new MeterTarget { Inventory = inventory, Stack = stack };
        }

        public static MeterTarget FromSlot(EquipmentSlotState slot)
        {
            return new MeterTarget { Slot = slot };
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
