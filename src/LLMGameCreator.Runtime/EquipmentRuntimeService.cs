using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class EquipmentRuntimeService : IEquipmentRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;

    public EquipmentRuntimeService(IRequirementEvaluator requirementEvaluator)
    {
        _requirementEvaluator = requirementEvaluator;
    }

    public GameRuntimeResult EquipItem(GamePackageDefinition package, GameRuntimeState state, string itemId, string slotId, string? inventoryId = null)
    {
        var item = package.Game.Items.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, itemId));
        if (item == null)
        {
            return Failure(state, "equipment.item_missing", $"Item not found: {itemId}", itemId);
        }

        var slotDefinition = package.Game.EquipmentSlots.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.Id, slotId));
        if (!ItemMatchesSlot(item, slotId, slotDefinition))
        {
            return Failure(state, "equipment.slot_mismatch", $"Item {item.Id} cannot be equipped in slot {slotId}.", slotId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var inventory = RuntimeStateHelpers.FindInventory(working, inventoryId);
        if (inventory == null)
        {
            return Failure(state, "inventory.missing", "Inventory not found for equip.", inventoryId);
        }

        var requirements = slotDefinition == null
            ? new RequirementEvaluationResult()
            : _requirementEvaluator.Evaluate(package, working, slotDefinition.RequiredRequirements, inventory.Id);
        if (!requirements.Success)
        {
            var result = new GameRuntimeResult { Success = false, State = state, Message = $"Equip failed: {item.Id}" };
            RecipeRuntimeService.AddRequirementFailures(result, requirements);
            return result;
        }

        var taken = RuntimeStateHelpers.TakeItemStacks(inventory, item.Id, 1);
        if (taken == null || taken.Count == 0)
        {
            return Failure(state, "equipment.item_not_in_inventory", $"Item {item.Id} is not in inventory.", item.Id);
        }

        var equipment = RuntimeStateHelpers.EnsurePlayerEquipment(working);
        var slot = RuntimeStateHelpers.EnsureEquipmentSlot(equipment, slotId);
        if (!string.IsNullOrWhiteSpace(slot.ItemId))
        {
            RuntimeStateHelpers.AddStack(inventory, RuntimeStateHelpers.StackFromSlot(slot));
        }

        RuntimeStateHelpers.SetSlotFromStack(slot, taken[0]);
        RuntimeStateHelpers.CopyState(working, state);

        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = $"Equipped {item.Id} in {slotId}.",
            Events = new List<GameRuntimeEvent>
            {
                RuntimeStateHelpers.Event(GameRuntimeEventType.EquipmentChanged, $"Equipped {item.Id} in {slotId}.", slotId),
                RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {inventory.Id}", inventory.Id)
            }
        };
    }

    public GameRuntimeResult UnequipItem(GamePackageDefinition package, GameRuntimeState state, string slotId, string? inventoryId = null)
    {
        var working = RuntimeStateHelpers.CloneState(state);
        var inventory = RuntimeStateHelpers.FindInventory(working, inventoryId) ?? RuntimeStateHelpers.EnsurePlayerInventory(working);
        var equipment = RuntimeStateHelpers.EnsurePlayerEquipment(working);
        var slot = equipment.Slots.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.SlotId, slotId));
        if (slot == null || string.IsNullOrWhiteSpace(slot.ItemId))
        {
            return Failure(state, "equipment.slot_empty", $"Equipment slot is empty: {slotId}", slotId);
        }

        var itemId = slot.ItemId!;
        RuntimeStateHelpers.AddStack(inventory, RuntimeStateHelpers.StackFromSlot(slot));
        RuntimeStateHelpers.ClearSlot(slot);
        RuntimeStateHelpers.CopyState(working, state);

        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = $"Unequipped {itemId} from {slotId}.",
            Events = new List<GameRuntimeEvent>
            {
                RuntimeStateHelpers.Event(GameRuntimeEventType.EquipmentChanged, $"Unequipped {itemId} from {slotId}.", slotId),
                RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {inventory.Id}", inventory.Id)
            }
        };
    }

    private static bool ItemMatchesSlot(ItemDefinition item, string slotId, EquipmentSlotDefinition? slotDefinition)
    {
        if (slotDefinition != null)
        {
            var kindAllowed = slotDefinition.AllowedKinds.Count == 0
                || slotDefinition.AllowedKinds.Any(kind => RuntimeStateHelpers.KindEquals(kind, item.Kind));
            var tagAllowed = slotDefinition.AllowedTags.Count == 0
                || slotDefinition.AllowedTags.Any(tag => item.Tags.Any(itemTag => RuntimeStateHelpers.KindEquals(tag, itemTag)));
            return kindAllowed && tagAllowed;
        }

        if (item.Metadata.TryGetValue("equip_slot", out var declaredSlot))
        {
            return RuntimeStateHelpers.IdEquals(declaredSlot, slotId) || RuntimeStateHelpers.KindEquals(declaredSlot, slotId);
        }

        return item.Tags.Any(tag => RuntimeStateHelpers.KindEquals(tag, slotId))
            || RuntimeStateHelpers.KindEquals(item.Kind, slotId);
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
}
