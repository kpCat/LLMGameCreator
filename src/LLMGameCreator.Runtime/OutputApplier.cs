using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class OutputApplier : IOutputApplier
{
    public OutputApplicationResult Apply(GamePackageDefinition package, GameRuntimeState state, IEnumerable<OutputDefinition> outputs, string? inventoryId = null, int? seed = null)
    {
        var result = new OutputApplicationResult();
        var random = new Random(seed ?? RuntimeStateHelpers.StableSeed($"{package.Manifest.PackageId}:{state.Tick}:{inventoryId}"));
        foreach (var output in outputs)
        {
            ApplyOne(package, state, output, inventoryId, random, result, depth: 0);
        }

        return result;
    }

    private static void ApplyOne(GamePackageDefinition package, GameRuntimeState state, OutputDefinition output, string? inventoryId, Random random, OutputApplicationResult result, int depth)
    {
        if (output.Amount < 0)
        {
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("output.amount.invalid", "Output amount must be non-negative.", output.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "item"))
        {
            var inventory = RuntimeStateHelpers.FindInventory(state, output.Scope ?? inventoryId) ?? RuntimeStateHelpers.EnsurePlayerInventory(state);
            var questItem = output.Metadata.TryGetValue("questItem", out var questValue)
                && questValue.Equals("true", StringComparison.OrdinalIgnoreCase);
            RuntimeStateHelpers.AddItem(inventory, output.Id, output.Amount, questItem);
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Added item {output.Id} x{Format(output.Amount)}", output.Id));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.InventoryChanged, $"Inventory changed: {inventory.Id}", inventory.Id));
            return;
        }

        if (IsResourceOutput(output))
        {
            var definition = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, output.Id));
            if (definition == null)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("output.resource_missing", $"Output references a missing resource: {output.Id}", output.Id));
                return;
            }

            RuntimeStateHelpers.ChangeResource(state, definition, output.Amount, output.Scope);
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Changed resource {output.Id} by {Format(output.Amount)}", output.Id));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ResourceChanged, $"Resource changed: {output.Id}", output.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "status") || RuntimeStateHelpers.KindEquals(output.Kind, "add_status"))
        {
            var targetId = string.IsNullOrWhiteSpace(output.Scope) ? state.PlayerEntityId : output.Scope!;
            state.Statuses.Add(new StatusState
            {
                StatusId = output.Id,
                TargetId = targetId,
                RemainingTicks = output.Amount > 0 ? (long?)Math.Ceiling(output.Amount) : null
            });
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.StatusAdded, $"Status added: {output.Id}", output.Id));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Applied status {output.Id}", output.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "flag") || RuntimeStateHelpers.KindEquals(output.Kind, "set_flag"))
        {
            var value = output.Mode ?? output.Amount.ToString("0.####");
            if (output.Metadata.TryGetValue("value", out var metadataValue))
            {
                value = metadataValue;
            }

            RuntimeStateHelpers.SetFlag(state, output.Id, value);
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Set flag {output.Id} = {value}", output.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "log") || RuntimeStateHelpers.KindEquals(output.Kind, "log_message"))
        {
            var message = output.Metadata.TryGetValue("message", out var text) ? text : output.Id;
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LogMessageAdded, message, output.Id));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Applied log output: {message}", output.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "loot") || RuntimeStateHelpers.KindEquals(output.Kind, "loot_table"))
        {
            ApplyLootOutput(package, state, output, inventoryId, random, result, depth);
            return;
        }

        result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("output.kind.unknown", $"Unknown output kind: {output.Kind}", output.Id));
    }

    private static void ApplyLootOutput(GamePackageDefinition package, GameRuntimeState state, OutputDefinition output, string? inventoryId, Random random, OutputApplicationResult result, int depth)
    {
        if (depth > 2)
        {
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("output.loot.depth", "Nested loot output depth exceeded.", output.Id));
            return;
        }

        var table = package.Game.LootTables.FirstOrDefault(t => RuntimeStateHelpers.IdEquals(t.Id, output.Id));
        if (table == null)
        {
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("output.loot_table_missing", $"Output references a missing loot table: {output.Id}", output.Id));
            return;
        }

        var entries = table.Entries.Where(e => e.Weight.GetValueOrDefault(1) > 0).ToList();
        if (entries.Count == 0)
        {
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("output.loot_table_empty", $"Loot table has no rollable entries: {table.Id}", table.Id, "warning"));
            return;
        }

        var totalWeight = entries.Sum(e => e.Weight.GetValueOrDefault(1));
        var roll = random.NextDouble() * totalWeight;
        LootEntryDefinition selected = entries[0];
        foreach (var entry in entries)
        {
            roll -= entry.Weight.GetValueOrDefault(1);
            if (roll <= 0)
            {
                selected = entry;
                break;
            }
        }

        var selectedOutput = new OutputDefinition
        {
            Kind = selected.Output.Kind,
            Id = selected.Output.Id,
            Amount = Math.Max(1, output.Amount) * ResolveEntryAmount(selected, random, selected.Output.Amount),
            Scope = selected.Output.Scope ?? output.Scope,
            Mode = selected.Output.Mode,
            Tags = selected.Output.Tags.ToList(),
            Metadata = new Dictionary<string, string>(selected.Output.Metadata)
        };

        ApplyOne(package, state, selectedOutput, inventoryId, random, result, depth + 1);
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LootRolled, $"Loot rolled: {table.Id} -> {selected.Id}", table.Id));
    }

    private static double ResolveEntryAmount(LootEntryDefinition entry, Random random, double fallback)
    {
        if (entry.MinCount.HasValue || entry.MaxCount.HasValue)
        {
            var min = entry.MinCount ?? entry.MaxCount ?? 1;
            var max = entry.MaxCount ?? entry.MinCount ?? min;
            return random.Next(min, max + 1);
        }

        return fallback <= 0 ? 1 : fallback;
    }

    private static bool IsResourceOutput(OutputDefinition output)
    {
        return RuntimeStateHelpers.KindEquals(output.Kind, "resource")
            || RuntimeStateHelpers.KindEquals(output.Kind, "network_resource")
            || RuntimeStateHelpers.KindEquals(output.Kind, "abstract_resource")
            || RuntimeStateHelpers.KindEquals(output.Kind, "progression")
            || RuntimeStateHelpers.KindEquals(output.Kind, "faction")
            || RuntimeStateHelpers.KindEquals(output.Kind, "reputation");
    }

    private static string Format(double value)
    {
        return value.ToString("0.####");
    }
}
