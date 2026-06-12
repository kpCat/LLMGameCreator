using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class UseItemRuntimeService : IUseItemRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IOutputApplier _outputApplier;

    public UseItemRuntimeService(IRequirementEvaluator requirementEvaluator, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult UseItem(GamePackageDefinition package, GameRuntimeState state, string itemId, string? inventoryId = null, string? targetId = null)
    {
        var item = package.Game.Items.FirstOrDefault(i => RuntimeStateHelpers.IdEquals(i.Id, itemId));
        if (item == null)
        {
            return Failure(state, "item.missing", $"Item not found: {itemId}", itemId);
        }

        var inventory = RuntimeStateHelpers.FindInventory(state, inventoryId);
        if (inventory == null)
        {
            return Failure(state, "inventory.missing", "Inventory not found for item use.", inventoryId);
        }

        if (RuntimeStateHelpers.GetItemAmount(inventory, item.Id) < 1)
        {
            return Failure(state, "runtime.item_missing", $"Missing item {item.Id} x1", item.Id);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var workingInventory = RuntimeStateHelpers.FindInventory(working, inventory.Id);
        var result = new GameRuntimeResult { State = state };

        var requirements = _requirementEvaluator.Evaluate(
            package,
            working,
            item.UseConditions.Select(RuntimeEffectMapper.ToRequirement),
            workingInventory?.Id);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var outputs = item.UseEffects.Select(effect =>
        {
            var output = RuntimeEffectMapper.ToOutput(effect);
            if (!string.IsNullOrWhiteSpace(targetId) && string.IsNullOrWhiteSpace(output.Scope))
            {
                output.Scope = targetId;
            }

            return output;
        }).ToList();

        var outputResult = _outputApplier.Apply(package, working, outputs, workingInventory?.Id);
        result.Events.AddRange(outputResult.Events);
        result.Diagnostics.AddRange(outputResult.Diagnostics);
        if (!outputResult.Success)
        {
            result.Success = false;
            result.Message = $"Use item failed: {item.Id}";
            return result;
        }

        if (IsConsumable(item))
        {
            if (workingInventory == null || !RuntimeStateHelpers.RemoveItem(workingInventory, item.Id, 1))
            {
                return Failure(state, "runtime.item_missing", $"Missing item {item.Id} x1", item.Id);
            }

            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {workingInventory.Id}", workingInventory.Id));
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.Success = true;
        result.State = state;
        result.Message = $"Used item: {item.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Used item: {item.Name}", item.Id));
        return result;
    }

    private static bool IsConsumable(ItemDefinition item)
    {
        return RuntimeStateHelpers.KindEquals(item.Kind, "consumable")
            || item.Tags.Any(tag => RuntimeStateHelpers.KindEquals(tag, "consumable"))
            || (item.Metadata.TryGetValue("consumeOnUse", out var value) && value.Equals("true", StringComparison.OrdinalIgnoreCase));
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
