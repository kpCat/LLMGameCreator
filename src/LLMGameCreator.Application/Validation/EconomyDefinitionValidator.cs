using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class EconomyDefinitionValidator : IGamePackageValidationRule
{
    private const string Category = "Economy";

    public void Validate(ValidationContext context, ValidationReport report)
    {
        var game = context.Package.Game;

        CheckIds(report, game.Items.Select(item => item.Id), "item");
        CheckIds(report, game.Resources.Select(resource => resource.Id), "resource");
        CheckIds(report, game.Statuses.Select(status => status.Id), "status");
        CheckIds(report, game.Recipes.Select(recipe => recipe.Id), "recipe");
        CheckIds(report, game.LootTables.Select(loot => loot.Id), "loot_table");
        CheckIds(report, game.Transactions.Select(transaction => transaction.Id), "transaction");
        CheckIds(report, game.ResourceNetworks.Select(network => network.Id), "resource_network");
        CheckIds(report, game.ResourceNodes.Select(node => node.Id), "resource_node");
        CheckIds(report, game.Inventories.Select(inventory => inventory.Id), "inventory");

        ValidateItems(context, report);
        ValidateResources(context, report);
        ValidateStatuses(context, report);
        ValidateRecipes(context, report);
        ValidateLootTables(context, report);
        ValidateTransactions(context, report);
        ValidateResourceNetworks(context, report);
        ValidateResourceNodes(context, report);
        ValidateInventories(context, report);
    }

    private static void ValidateItems(ValidationContext context, ValidationReport report)
    {
        foreach (var item in context.Package.Game.Items)
        {
            RequireText(report, item.Id, "item.id.empty", "Item id is required.", item.Id);
            RequireText(report, item.Name, "item.name.empty", "Item name is required.", item.Id);
            if (item.MaxStack.HasValue && item.MaxStack.Value <= 0)
            {
                Add(report, "item.max_stack.invalid", ValidationSeverity.Error, "Item max_stack must be positive.", item.Id);
            }

            CheckOptionalNonNegative(report, item.Value, "item.value.invalid", "Item value must be non-negative.", item.Id);
            CheckOptionalNonNegative(report, item.Weight, "item.weight.invalid", "Item weight must be non-negative.", item.Id);
            CheckOptionalNonNegative(report, item.MaxDurability, "item.max_durability.invalid", "Item max_durability must be non-negative.", item.Id);
            CheckOptionalNonNegative(report, item.MaxCharge, "item.max_charge.invalid", "Item max_charge must be non-negative.", item.Id);
            ValidateRequirements(context, report, item.Requirements, item.Id);
        }
    }

    private static void ValidateResources(ValidationContext context, ValidationReport report)
    {
        foreach (var resource in context.Package.Game.Resources)
        {
            RequireText(report, resource.Id, "resource.id.empty", "Resource id is required.", resource.Id);
            RequireText(report, resource.Name, "resource.name.empty", "Resource name is required.", resource.Id);
            if (resource.MinValue.HasValue && resource.MaxValue.HasValue && resource.MaxValue.Value < resource.MinValue.Value)
            {
                Add(report, "resource.range.invalid", ValidationSeverity.Error, "Resource max_value must be greater than or equal to min_value.", resource.Id);
            }

            CheckOptionalNonNegative(report, resource.RegenPerTick, "resource.regen_per_tick.invalid", "Resource regen_per_tick must be non-negative.", resource.Id);
            if (!string.IsNullOrWhiteSpace(resource.IconAssetId) && !context.AssetIds.Contains(resource.IconAssetId))
            {
                Add(report, "resource.icon_asset.missing", ValidationSeverity.Error, "Resource icon_asset_id references a missing asset.", resource.Id);
            }
        }
    }

    private static void ValidateStatuses(ValidationContext context, ValidationReport report)
    {
        foreach (var status in context.Package.Game.Statuses)
        {
            RequireText(report, status.Id, "status.id.empty", "Status id is required.", status.Id);
            RequireText(report, status.Name, "status.name.empty", "Status name is required.", status.Id);
            foreach (var effect in status.Effects)
            {
                ValidateEffect(context, report, effect, status.Id);
            }
        }
    }

    private static void ValidateRecipes(ValidationContext context, ValidationReport report)
    {
        foreach (var recipe in context.Package.Game.Recipes)
        {
            RequireText(report, recipe.Id, "recipe.id.empty", "Recipe id is required.", recipe.Id);
            RequireText(report, recipe.Name, "recipe.name.empty", "Recipe name is required.", recipe.Id);
            ValidateRequirements(context, report, recipe.Requirements, recipe.Id);
            ValidateCosts(context, report, recipe.Inputs, recipe.Id, "recipe.input");
            ValidateCosts(context, report, recipe.Costs, recipe.Id, "recipe.cost");
            ValidateOutputs(context, report, recipe.Outputs, recipe.Id, "recipe.output", allowProgressionWarning: true);
            ValidateOutputs(context, report, recipe.FailureOutputs, recipe.Id, "recipe.failure_output", allowProgressionWarning: true);
            CheckOptionalNonNegative(report, recipe.Duration, "recipe.duration.invalid", "Recipe duration must be non-negative.", recipe.Id);
            CheckOptionalNonNegative(report, recipe.Cooldown, "recipe.cooldown.invalid", "Recipe cooldown must be non-negative.", recipe.Id);
            if (recipe.SuccessChance.HasValue && (recipe.SuccessChance.Value < 0 || recipe.SuccessChance.Value > 1))
            {
                Add(report, "recipe.success_chance.invalid", ValidationSeverity.Error, "Recipe success_chance must be between 0 and 1.", recipe.Id);
            }

            if (!string.IsNullOrWhiteSpace(recipe.StationId)
                && !context.EntityPrototypeIds.Contains(recipe.StationId)
                && !context.ItemIds.Contains(recipe.StationId)
                && !context.Package.Game.ResourceNodes.Any(node => IdEquals(node.Id, recipe.StationId)))
            {
                Add(report, "recipe.station.missing", ValidationSeverity.Warning, "Recipe station_id does not reference a known entity prototype, item or resource node.", recipe.Id);
            }
        }
    }

    private static void ValidateLootTables(ValidationContext context, ValidationReport report)
    {
        foreach (var loot in context.Package.Game.LootTables)
        {
            RequireText(report, loot.Id, "loot_table.id.empty", "Loot table id is required.", loot.Id);
            RequireText(report, loot.Name, "loot_table.name.empty", "Loot table name is required.", loot.Id);
            CheckIds(report, loot.Entries.Select(entry => entry.Id), "loot_entry", loot.Id);
            foreach (var entry in loot.Entries)
            {
                RequireText(report, entry.Id, "loot_entry.id.empty", "Loot entry id is required.", loot.Id);
                ValidateOutput(context, report, entry.Output, loot.Id, "loot.output", allowProgressionWarning: false);
                ValidateRequirements(context, report, entry.Requirements, loot.Id);
                CheckOptionalNonNegative(report, entry.Weight, "loot.weight.invalid", "Loot entry weight must be non-negative.", loot.Id);
                CheckOptionalNonNegative(report, entry.MinCount, "loot.min_count.invalid", "Loot entry min_count must be non-negative.", loot.Id);
                CheckOptionalNonNegative(report, entry.MaxCount, "loot.max_count.invalid", "Loot entry max_count must be non-negative.", loot.Id);
                if (entry.MinCount.HasValue && entry.MaxCount.HasValue && entry.MaxCount.Value < entry.MinCount.Value)
                {
                    Add(report, "loot.count_range.invalid", ValidationSeverity.Error, "Loot entry max_count must be greater than or equal to min_count.", loot.Id);
                }

                if (entry.Unique && entry.MaxGlobalCount.HasValue && entry.MaxGlobalCount.Value <= 0)
                {
                    Add(report, "unique_loot.invalid_count", ValidationSeverity.Error, "Unique loot max_global_count must be positive.", loot.Id);
                }
            }

            var duplicateGuaranteedUniqueItems = loot.Entries
                .Where(entry => entry.Unique && entry.Output.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) && entry.MaxGlobalCount == 1)
                .GroupBy(entry => entry.Output.Id)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1);
            foreach (var duplicate in duplicateGuaranteedUniqueItems)
            {
                Add(report, "unique_loot.duplicate_item", ValidationSeverity.Warning, $"Duplicate unique loot item id: {duplicate.Key}", loot.Id);
            }
        }
    }

    private static void ValidateTransactions(ValidationContext context, ValidationReport report)
    {
        foreach (var transaction in context.Package.Game.Transactions)
        {
            RequireText(report, transaction.Id, "transaction.id.empty", "Transaction id is required.", transaction.Id);
            RequireText(report, transaction.Name, "transaction.name.empty", "Transaction name is required.", transaction.Id);
            ValidateRequirements(context, report, transaction.Requirements, transaction.Id);
            ValidateCosts(context, report, transaction.Costs, transaction.Id, "transaction.cost");
            ValidateOutputs(context, report, transaction.Outputs, transaction.Id, "transaction.output", allowProgressionWarning: true);
            if (!string.IsNullOrWhiteSpace(transaction.StockLootTableId) && !context.LootTableIds.Contains(transaction.StockLootTableId))
            {
                Add(report, "transaction.stock_loot_table.missing", ValidationSeverity.Error, "Transaction stock_loot_table_id references a missing loot table.", transaction.Id);
            }
        }
    }

    private static void ValidateResourceNetworks(ValidationContext context, ValidationReport report)
    {
        foreach (var network in context.Package.Game.ResourceNetworks)
        {
            RequireText(report, network.Id, "resource_network.id.empty", "Resource network id is required.", network.Id);
            RequireText(report, network.Name, "resource_network.name.empty", "Resource network name is required.", network.Id);
            if (string.IsNullOrWhiteSpace(network.ResourceId) || !context.ResourceIds.Contains(network.ResourceId))
            {
                Add(report, "resource_network.resource_missing", ValidationSeverity.Error, "Resource network resource_id references a missing resource.", network.Id);
            }
        }
    }

    private static void ValidateResourceNodes(ValidationContext context, ValidationReport report)
    {
        foreach (var node in context.Package.Game.ResourceNodes)
        {
            RequireText(report, node.Id, "resource_node.id.empty", "Resource node id is required.", node.Id);
            RequireText(report, node.Name, "resource_node.name.empty", "Resource node name is required.", node.Id);
            if (!string.IsNullOrWhiteSpace(node.NetworkId) && !context.ResourceNetworkIds.Contains(node.NetworkId))
            {
                Add(report, "resource_node.network_missing", ValidationSeverity.Error, "Resource node network_id references a missing resource network.", node.Id);
            }

            if (!string.IsNullOrWhiteSpace(node.EntityPrototypeId) && !context.EntityPrototypeIds.Contains(node.EntityPrototypeId))
            {
                Add(report, "resource_node.entity_prototype_missing", ValidationSeverity.Error, "Resource node entity_prototype_id references a missing entity prototype.", node.Id);
            }

            ValidateOutputs(context, report, node.Production, node.Id, "resource_node.production", allowProgressionWarning: false);
            ValidateCosts(context, report, node.Consumption, node.Id, "resource_node.consumption");
            ValidateOutputs(context, report, node.Storage, node.Id, "resource_node.storage", allowProgressionWarning: false);
            ValidateCosts(context, report, node.ConversionInputs, node.Id, "resource_node.conversion_input");
            ValidateOutputs(context, report, node.ConversionOutputs, node.Id, "resource_node.conversion_output", allowProgressionWarning: false);
            ValidateRequirements(context, report, node.Requirements, node.Id);
        }
    }

    private static void ValidateInventories(ValidationContext context, ValidationReport report)
    {
        foreach (var inventory in context.Package.Game.Inventories)
        {
            RequireText(report, inventory.Id, "inventory.id.empty", "Inventory id is required.", inventory.Id);
            RequireText(report, inventory.OwnerKind, "inventory.owner_kind.empty", "Inventory owner_kind is required.", inventory.Id);
            if (inventory.Slots < 0)
            {
                Add(report, "inventory.slots.invalid", ValidationSeverity.Error, "Inventory slots must be non-negative.", inventory.Id);
            }

            foreach (var stack in inventory.Stacks)
            {
                if (string.IsNullOrWhiteSpace(stack.ItemId) || !context.ItemIds.Contains(stack.ItemId))
                {
                    Add(report, "inventory.item_missing", ValidationSeverity.Error, $"Inventory stack references a missing item: {stack.ItemId}", inventory.Id);
                }

                if (stack.Amount <= 0)
                {
                    Add(report, "inventory.stack.amount.invalid", ValidationSeverity.Error, "Inventory stack amount must be positive.", inventory.Id);
                }

                CheckOptionalNonNegative(report, stack.Durability, "inventory.stack.durability.invalid", "Inventory stack durability must be non-negative.", inventory.Id);
                CheckOptionalNonNegative(report, stack.Charge, "inventory.stack.charge.invalid", "Inventory stack charge must be non-negative.", inventory.Id);
            }
        }
    }

    private static void ValidateRequirements(ValidationContext context, ValidationReport report, IEnumerable<RequirementDefinition> requirements, string ownerId)
    {
        foreach (var requirement in requirements)
        {
            RequireText(report, requirement.Kind, "requirement.kind.empty", "Requirement kind is required.", ownerId);
            CheckOptionalNonNegative(report, requirement.Amount, "requirement.amount.invalid", "Requirement amount must be non-negative.", ownerId);
            if (requirement.Kind.Equals("has_item", StringComparison.OrdinalIgnoreCase) && !context.ItemIds.Contains(requirement.Id))
            {
                Add(report, "requirement.item_missing", ValidationSeverity.Error, $"Requirement references a missing item: {requirement.Id}", ownerId);
            }
            else if ((requirement.Kind.Equals("resource_at_least", StringComparison.OrdinalIgnoreCase)
                || requirement.Kind.Equals("network_resource_at_least", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(requirement.Id))
            {
                Add(report, "requirement.resource_missing", ValidationSeverity.Error, $"Requirement references a missing resource: {requirement.Id}", ownerId);
            }
            else if (requirement.Kind.Equals("status_active", StringComparison.OrdinalIgnoreCase) && !context.StatusIds.Contains(requirement.Id))
            {
                Add(report, "requirement.status_missing", ValidationSeverity.Error, $"Requirement references a missing status: {requirement.Id}", ownerId);
            }
        }
    }

    private static void ValidateCosts(ValidationContext context, ValidationReport report, IEnumerable<CostDefinition> costs, string ownerId, string codePrefix)
    {
        foreach (var cost in costs)
        {
            RequireText(report, cost.Kind, $"{codePrefix}.kind.empty", "Cost kind is required.", ownerId);
            RequireText(report, cost.Id, $"{codePrefix}.id.empty", "Cost id is required.", ownerId);
            if (cost.Amount <= 0)
            {
                Add(report, $"{codePrefix}.amount.invalid", ValidationSeverity.Error, "Cost amount must be positive.", ownerId);
            }

            if (cost.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) && !context.ItemIds.Contains(cost.Id))
            {
                Add(report, $"{codePrefix}.item_missing", ValidationSeverity.Error, $"Cost references a missing item: {cost.Id}", ownerId);
            }
            else if ((cost.Kind.Equals("resource", StringComparison.OrdinalIgnoreCase)
                || cost.Kind.Equals("network_resource", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(cost.Id))
            {
                var code = codePrefix.Equals("transaction.cost", StringComparison.OrdinalIgnoreCase)
                    ? "transaction.cost.unknown_reference"
                    : $"{codePrefix}.resource_missing";
                Add(report, code, ValidationSeverity.Error, $"Cost references a missing resource: {cost.Id}", ownerId);
            }
        }
    }

    private static void ValidateOutputs(ValidationContext context, ValidationReport report, IEnumerable<OutputDefinition> outputs, string ownerId, string codePrefix, bool allowProgressionWarning)
    {
        foreach (var output in outputs)
        {
            ValidateOutput(context, report, output, ownerId, codePrefix, allowProgressionWarning);
        }
    }

    private static void ValidateOutput(ValidationContext context, ValidationReport report, OutputDefinition output, string ownerId, string codePrefix, bool allowProgressionWarning)
    {
        RequireText(report, output.Kind, $"{codePrefix}.kind.empty", "Output kind is required.", ownerId);
        RequireText(report, output.Id, $"{codePrefix}.id.empty", "Output id is required.", ownerId);
        if (output.Amount < 0)
        {
            Add(report, $"{codePrefix}.amount.invalid", ValidationSeverity.Error, "Output amount must be non-negative.", ownerId);
        }

        if (output.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) && !context.ItemIds.Contains(output.Id))
        {
            Add(report, $"{codePrefix}.item_missing", ValidationSeverity.Error, $"Output references a missing item: {output.Id}", ownerId);
        }
        else if ((output.Kind.Equals("resource", StringComparison.OrdinalIgnoreCase)
            || output.Kind.Equals("network_resource", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(output.Id))
        {
            Add(report, $"{codePrefix}.resource_missing", ValidationSeverity.Error, $"Output references a missing resource: {output.Id}", ownerId);
        }
        else if (output.Kind.Equals("status", StringComparison.OrdinalIgnoreCase) && !context.StatusIds.Contains(output.Id))
        {
            Add(report, $"{codePrefix}.status_missing", ValidationSeverity.Error, $"Output references a missing status: {output.Id}", ownerId);
        }
        else if (output.Kind.Equals("loot", StringComparison.OrdinalIgnoreCase) && !context.LootTableIds.Contains(output.Id))
        {
            Add(report, $"{codePrefix}.loot_table_missing", ValidationSeverity.Error, $"Output references a missing loot table: {output.Id}", ownerId);
        }
        else if (allowProgressionWarning && output.Kind.Equals("progression", StringComparison.OrdinalIgnoreCase))
        {
            Add(report, "economy.runtime.not_implemented", ValidationSeverity.Warning, "Progression output is data-only; no runtime handler is implemented yet.", ownerId);
        }
    }

    private static void ValidateEffect(ValidationContext context, ValidationReport report, EffectDefinition effect, string ownerId)
    {
        if (effect.Type.Equals("add_status", StringComparison.OrdinalIgnoreCase)
            && effect.Args.TryGetValue("statusId", out var statusId)
            && !context.StatusIds.Contains(statusId))
        {
            Add(report, "status.effect.status_missing", ValidationSeverity.Error, $"Status effect references a missing status: {statusId}", ownerId);
        }
    }

    private static void CheckIds(ValidationReport report, IEnumerable<string> ids, string group, string? ownerId = null)
    {
        foreach (var duplicate in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id).Where(g => g.Count() > 1))
        {
            Add(report, $"{group}.id.duplicate", ValidationSeverity.Error, $"Duplicate {group} id: {duplicate.Key}", ownerId ?? duplicate.Key);
        }
    }

    private static void RequireText(ValidationReport report, string? value, string code, string message, string? targetId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void CheckOptionalNonNegative(ValidationReport report, double? value, string code, string message, string? targetId)
    {
        if (value.HasValue && value.Value < 0)
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void CheckOptionalNonNegative(ValidationReport report, int? value, string code, string message, string? targetId)
    {
        if (value.HasValue && value.Value < 0)
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void Add(ValidationReport report, string code, ValidationSeverity severity, string message, string? targetId)
    {
        ValidationIssueBuilder.Add(report, code, severity, message, targetId, Category);
    }

    private static bool IdEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
    }
}
