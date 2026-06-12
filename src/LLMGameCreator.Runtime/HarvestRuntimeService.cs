using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class HarvestRuntimeService : IHarvestRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly ICostConsumer _costConsumer;
    private readonly IOutputApplier _outputApplier;

    public HarvestRuntimeService(IRequirementEvaluator requirementEvaluator, ICostConsumer costConsumer, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _costConsumer = costConsumer;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult HarvestResourceNode(GamePackageDefinition package, GameRuntimeState state, string nodeId, string? inventoryId = null, string? toolItemId = null, int? seed = null)
    {
        var node = package.Game.ResourceNodes.FirstOrDefault(n => RuntimeStateHelpers.IdEquals(n.Id, nodeId));
        if (node == null)
        {
            return Failure(state, "resource_node.missing", $"Resource node not found: {nodeId}", nodeId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var requirements = _requirementEvaluator.Evaluate(package, working, node.Requirements, inventoryId);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var toolFailure = ValidateTool(package, working, node, inventoryId, toolItemId);
        if (toolFailure != null)
        {
            return Failure(state, toolFailure.Code, toolFailure.Message, toolFailure.TargetId);
        }

        var costs = node.Consumption.Concat(node.ConversionInputs).Concat(BuildToolCosts(node, toolItemId)).ToList();
        var costResult = _costConsumer.Consume(package, working, costs, inventoryId);
        result.Events.AddRange(costResult.Events);
        result.Diagnostics.AddRange(costResult.Diagnostics);
        if (!costResult.Success)
        {
            result.Success = false;
            result.Message = $"Harvest failed: {node.Id}";
            return result;
        }

        var outputs = node.Production.Concat(node.ConversionOutputs).ToList();
        if (TryMetadata(node.Metadata, "loot_table_id", out var lootTableId) || TryMetadata(node.Metadata, "harvest_loot_table_id", out lootTableId))
        {
            outputs.Add(new OutputDefinition { Kind = "loot", Id = lootTableId, Amount = 1 });
        }

        var outputResult = _outputApplier.Apply(package, working, outputs, inventoryId, seed);
        result.Events.AddRange(outputResult.Events);
        result.Diagnostics.AddRange(outputResult.Diagnostics);
        if (!outputResult.Success)
        {
            result.Success = false;
            result.Message = $"Harvest output failed: {node.Id}";
            return result;
        }

        if (TryMetadata(node.Metadata, "deplete_on_harvest", out var deplete) && deplete.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            working.Metadata[$"resource_node:{node.Id}:depleted"] = "true";
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.Success = true;
        result.State = state;
        result.Message = $"Resource harvested: {node.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ResourceHarvested, $"Resource harvested: {node.Name}", node.Id));
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LogMessageAdded, $"Harvested {node.Name}.", node.Id));
        return result;
    }

    private static RuntimeDiagnostic? ValidateTool(GamePackageDefinition package, GameRuntimeState state, ResourceNodeDefinition node, string? inventoryId, string? toolItemId)
    {
        var inventory = RuntimeStateHelpers.FindInventory(state, inventoryId);
        if (TryMetadata(node.Metadata, "required_tool_item_id", out var requiredItemId))
        {
            var actualItemId = string.IsNullOrWhiteSpace(toolItemId) ? requiredItemId : toolItemId!;
            if (!RuntimeStateHelpers.IdEquals(actualItemId, requiredItemId) || !HasItemOrEquipped(state, inventory, requiredItemId, null))
            {
                return RuntimeStateHelpers.Diagnostic("harvest.tool_item_missing", $"Harvest requires tool item: {requiredItemId}", requiredItemId);
            }
        }

        if (TryMetadata(node.Metadata, "required_tool_tag", out var requiredTag))
        {
            if (string.IsNullOrWhiteSpace(requiredTag))
            {
                return RuntimeStateHelpers.Diagnostic("harvest.tool_tag_empty", "Harvest required_tool_tag is empty.", node.Id);
            }

            if (!HasItemOrEquipped(state, inventory, toolItemId, requiredTag, package))
            {
                return RuntimeStateHelpers.Diagnostic("harvest.tool_tag_missing", $"Harvest requires tool tag: {requiredTag}", node.Id);
            }
        }

        return null;
    }

    private static bool HasItemOrEquipped(GameRuntimeState state, InventoryState? inventory, string? itemId, string? requiredTag, GamePackageDefinition? package = null)
    {
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            if (RuntimeStateHelpers.GetItemAmount(inventory, itemId) > 0)
            {
                return true;
            }

            return state.Equipment.SelectMany(e => e.Slots).Any(s => RuntimeStateHelpers.IdEquals(s.ItemId, itemId));
        }

        if (string.IsNullOrWhiteSpace(requiredTag) || package == null)
        {
            return false;
        }

        var matchingItemIds = package.Game.Items
            .Where(item => item.Tags.Any(tag => RuntimeStateHelpers.KindEquals(tag, requiredTag)))
            .Select(item => item.Id)
            .ToList();
        return matchingItemIds.Any(id => RuntimeStateHelpers.GetItemAmount(inventory, id) > 0)
            || state.Equipment.SelectMany(e => e.Slots).Any(s => matchingItemIds.Any(id => RuntimeStateHelpers.IdEquals(s.ItemId, id)));
    }

    private static IEnumerable<CostDefinition> BuildToolCosts(ResourceNodeDefinition node, string? toolItemId)
    {
        var target = toolItemId;
        if (string.IsNullOrWhiteSpace(target) && TryMetadata(node.Metadata, "required_tool_item_id", out var requiredTool))
        {
            target = requiredTool;
        }

        if (string.IsNullOrWhiteSpace(target) && TryMetadata(node.Metadata, "tool_slot_id", out var slotId))
        {
            target = slotId;
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            target = "slot/tool";
        }

        if (TryMetadata(node.Metadata, "durability_cost", out var durability) && double.TryParse(durability, out var durabilityAmount) && durabilityAmount > 0)
        {
            yield return new CostDefinition { Kind = "durability", Id = target!, Amount = durabilityAmount };
        }

        if (TryMetadata(node.Metadata, "charge_cost", out var charge) && double.TryParse(charge, out var chargeAmount) && chargeAmount > 0)
        {
            yield return new CostDefinition { Kind = "charge", Id = target!, Amount = chargeAmount };
        }
    }

    private static bool TryMetadata(Dictionary<string, string> metadata, string key, out string value)
    {
        return metadata.TryGetValue(key, out value!) && !string.IsNullOrWhiteSpace(value);
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
