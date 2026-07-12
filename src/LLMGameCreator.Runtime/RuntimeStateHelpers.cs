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

    public static void AddStack(InventoryState inventory, ItemStackState source)
    {
        if (source.Amount <= 0)
        {
            return;
        }

        var stack = string.IsNullOrWhiteSpace(source.UniqueInstanceId)
            ? inventory.Stacks.FirstOrDefault(s =>
                IdEquals(s.ItemId, source.ItemId)
                && string.IsNullOrWhiteSpace(s.UniqueInstanceId)
                && NullableEquals(s.Durability, source.Durability)
                && NullableEquals(s.Charge, source.Charge)
                && DictionaryEquals(s.Metadata, source.Metadata))
            : null;

        if (stack == null)
        {
            inventory.Stacks.Add(CloneStack(source));
            return;
        }

        stack.Amount += source.Amount;
        stack.QuestItem = stack.QuestItem || source.QuestItem;
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

    public static List<ItemStackState>? TakeItemStacks(InventoryState inventory, string itemId, double amount)
    {
        if (amount <= 0 || GetItemAmount(inventory, itemId) < amount)
        {
            return null;
        }

        var remaining = amount;
        var taken = new List<ItemStackState>();
        foreach (var stack in inventory.Stacks.Where(s => IdEquals(s.ItemId, itemId)).ToList())
        {
            if (remaining <= 0)
            {
                break;
            }

            var consumed = Math.Min(stack.Amount, remaining);
            var clone = CloneStack(stack);
            clone.Amount = consumed;
            taken.Add(clone);
            stack.Amount -= consumed;
            remaining -= consumed;
            if (stack.Amount <= 0)
            {
                inventory.Stacks.Remove(stack);
            }
        }

        return taken;
    }

    public static ItemStackState CloneStack(ItemStackState stack)
    {
        return new ItemStackState
        {
            ItemId = stack.ItemId,
            Amount = stack.Amount,
            UniqueInstanceId = stack.UniqueInstanceId,
            QuestItem = stack.QuestItem,
            Durability = stack.Durability,
            Charge = stack.Charge,
            Metadata = new Dictionary<string, string>(stack.Metadata)
        };
    }

    public static EquipmentState EnsurePlayerEquipment(GameRuntimeState state)
    {
        var equipment = state.Equipment.FirstOrDefault(e => KindEquals(e.OwnerKind, "player") && IdEquals(e.OwnerId, state.PlayerEntityId))
            ?? state.Equipment.FirstOrDefault(e => KindEquals(e.OwnerKind, "player"));

        if (equipment != null)
        {
            return equipment;
        }

        equipment = new EquipmentState
        {
            OwnerKind = "player",
            OwnerId = state.PlayerEntityId
        };
        state.Equipment.Add(equipment);
        return equipment;
    }

    public static EquipmentSlotState EnsureEquipmentSlot(EquipmentState equipment, string slotId)
    {
        var slot = equipment.Slots.FirstOrDefault(s => IdEquals(s.SlotId, slotId));
        if (slot != null)
        {
            return slot;
        }

        slot = new EquipmentSlotState { SlotId = slotId };
        equipment.Slots.Add(slot);
        return slot;
    }

    public static ItemStackState StackFromSlot(EquipmentSlotState slot)
    {
        return new ItemStackState
        {
            ItemId = slot.ItemId ?? string.Empty,
            Amount = 1,
            UniqueInstanceId = slot.UniqueInstanceId,
            QuestItem = slot.QuestItem,
            Durability = slot.Durability,
            Charge = slot.Charge,
            Metadata = new Dictionary<string, string>(slot.Metadata)
        };
    }

    public static void SetSlotFromStack(EquipmentSlotState slot, ItemStackState stack)
    {
        slot.ItemId = stack.ItemId;
        slot.UniqueInstanceId = stack.UniqueInstanceId;
        slot.QuestItem = stack.QuestItem;
        slot.Durability = stack.Durability;
        slot.Charge = stack.Charge;
        slot.Metadata = new Dictionary<string, string>(stack.Metadata);
    }

    public static void ClearSlot(EquipmentSlotState slot)
    {
        slot.ItemId = null;
        slot.UniqueInstanceId = null;
        slot.QuestItem = false;
        slot.Durability = null;
        slot.Charge = null;
        slot.Metadata = new Dictionary<string, string>();
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

    public static FactionRuntimeState EnsureFaction(GameRuntimeState state, FactionDefinition definition)
    {
        var faction = state.Factions.FirstOrDefault(f => IdEquals(f.FactionId, definition.Id));
        if (faction != null)
        {
            return faction;
        }

        faction = new FactionRuntimeState
        {
            FactionId = definition.Id,
            Reputation = Clamp(definition.DefaultReputation ?? 0, definition.MinReputation, definition.MaxReputation),
            RelationKind = "neutral",
            Metadata = new Dictionary<string, string>(definition.Metadata)
        };
        state.Factions.Add(faction);
        return faction;
    }

    public static ProgressionState EnsureProgression(GameRuntimeState state, ProgressionDefinition definition)
    {
        var progression = state.Progressions.FirstOrDefault(p => IdEquals(p.ProgressionId, definition.Id));
        if (progression != null)
        {
            return progression;
        }

        progression = new ProgressionState
        {
            ProgressionId = definition.Id,
            Amount = 0,
            StageId = ResolveProgressionStage(definition, 0)
        };
        state.Progressions.Add(progression);
        return progression;
    }

    public static string? ResolveProgressionStage(ProgressionDefinition definition, double amount)
    {
        return definition.Stages
            .Where(stage => stage.RequiredAmount <= amount)
            .OrderByDescending(stage => stage.RequiredAmount)
            .FirstOrDefault()
            ?.Id;
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
            Quests = state.Quests.Select(quest => new QuestRuntimeState
            {
                QuestId = quest.QuestId,
                State = quest.State,
                CurrentStageId = quest.CurrentStageId,
                StartedTick = quest.StartedTick,
                CompletedTick = quest.CompletedTick,
                Metadata = new Dictionary<string, string>(quest.Metadata),
                Objectives = quest.Objectives.Select(objective => new QuestObjectiveRuntimeState
                {
                    ObjectiveId = objective.ObjectiveId,
                    Kind = objective.Kind,
                    TargetId = objective.TargetId,
                    CurrentAmount = objective.CurrentAmount,
                    RequiredAmount = objective.RequiredAmount,
                    Completed = objective.Completed,
                    Metadata = new Dictionary<string, string>(objective.Metadata)
                }).ToList()
            }).ToList(),
            ActiveDialogue = state.ActiveDialogue == null
                ? null
                : new DialogueRuntimeState
                {
                    DialogueId = state.ActiveDialogue.DialogueId,
                    CurrentNodeId = state.ActiveDialogue.CurrentNodeId,
                    SpeakerId = state.ActiveDialogue.SpeakerId,
                    Open = state.ActiveDialogue.Open,
                    History = state.ActiveDialogue.History.ToList(),
                    Metadata = new Dictionary<string, string>(state.ActiveDialogue.Metadata)
                },
            Factions = state.Factions.Select(faction => new FactionRuntimeState
            {
                FactionId = faction.FactionId,
                Reputation = faction.Reputation,
                RelationKind = faction.RelationKind,
                Metadata = new Dictionary<string, string>(faction.Metadata)
            }).ToList(),
            Equipment = state.Equipment.Select(equipment => new EquipmentState
            {
                OwnerKind = equipment.OwnerKind,
                OwnerId = equipment.OwnerId,
                Metadata = new Dictionary<string, string>(equipment.Metadata),
                Slots = equipment.Slots.Select(slot => new EquipmentSlotState
                {
                    SlotId = slot.SlotId,
                    ItemId = slot.ItemId,
                    UniqueInstanceId = slot.UniqueInstanceId,
                    QuestItem = slot.QuestItem,
                    Durability = slot.Durability,
                    Charge = slot.Charge,
                    Metadata = new Dictionary<string, string>(slot.Metadata)
                }).ToList()
            }).ToList(),
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
            Stats = state.Stats.Select(stat => new StatValueState
            {
                StatId = stat.StatId,
                Value = stat.Value
            }).ToList(),
            Progressions = state.Progressions.Select(progression => new ProgressionState
            {
                ProgressionId = progression.ProgressionId,
                Amount = progression.Amount,
                StageId = progression.StageId,
                Metadata = new Dictionary<string, string>(progression.Metadata)
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
            }).ToList(),
            ActiveEncounter = CloneEncounter(state.ActiveEncounter)
        };
    }

    public static void CopyState(GameRuntimeState source, GameRuntimeState target)
    {
        target.PackageId = source.PackageId;
        target.CurrentMapId = source.CurrentMapId;
        target.PlayerEntityId = source.PlayerEntityId;
        target.Tick = source.Tick;
        target.Inventories = source.Inventories;
        target.Equipment = source.Equipment;
        target.Resources = source.Resources;
        target.Stats = source.Stats;
        target.Progressions = source.Progressions;
        target.Flags = source.Flags;
        target.Statuses = source.Statuses;
        target.ActiveEncounter = source.ActiveEncounter;
        target.QuestStates = source.QuestStates;
        target.Quests = source.Quests;
        target.ActiveDialogue = source.ActiveDialogue;
        target.Factions = source.Factions;
        target.Metadata = source.Metadata;
    }

    public static EncounterRuntimeState? CloneEncounter(EncounterRuntimeState? encounter)
    {
        if (encounter == null)
        {
            return null;
        }

        return new EncounterRuntimeState
        {
            EncounterId = encounter.EncounterId,
            Kind = encounter.Kind,
            Active = encounter.Active,
            Round = encounter.Round,
            TurnIndex = encounter.TurnIndex,
            ActionHistory = encounter.ActionHistory.ToList(),
            Metadata = new Dictionary<string, string>(encounter.Metadata),
            Participants = encounter.Participants.Select(participant => new EncounterParticipantState
            {
                Id = participant.Id,
                Name = participant.Name,
                Team = participant.Team,
                Alive = participant.Alive,
                InventoryId = participant.InventoryId,
                Metadata = new Dictionary<string, string>(participant.Metadata),
                Cooldowns = new Dictionary<string, int>(participant.Cooldowns),
                Stats = participant.Stats.Select(stat => new StatValueState
                {
                    StatId = stat.StatId,
                    Value = stat.Value
                }).ToList(),
                Resources = participant.Resources.Select(resource => new ResourceState
                {
                    ResourceId = resource.ResourceId,
                    Amount = resource.Amount,
                    Capacity = resource.Capacity,
                    Scope = resource.Scope,
                    OwnerId = resource.OwnerId
                }).ToList(),
                Statuses = participant.Statuses.Select(status => new StatusState
                {
                    StatusId = status.StatusId,
                    TargetId = status.TargetId,
                    RemainingTicks = status.RemainingTicks,
                    Stacks = status.Stacks,
                    Metadata = new Dictionary<string, string>(status.Metadata)
                }).ToList()
            }).ToList()
        };
    }

    public static bool KindEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IdEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
    }

    private static bool NullableEquals(double? left, double? right)
    {
        return (!left.HasValue && !right.HasValue) || (left.HasValue && right.HasValue && Math.Abs(left.Value - right.Value) < double.Epsilon);
    }

    private static bool DictionaryEquals(Dictionary<string, string> left, Dictionary<string, string> right)
    {
        return left.Count == right.Count
            && left.All(pair => right.TryGetValue(pair.Key, out var value) && string.Equals(value, pair.Value, StringComparison.Ordinal));
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
