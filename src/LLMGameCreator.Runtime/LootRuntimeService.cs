using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class LootRuntimeService : ILootRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IOutputApplier _outputApplier;

    public LootRuntimeService(IRequirementEvaluator requirementEvaluator, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult RollLootTable(GamePackageDefinition package, GameRuntimeState state, string lootTableId, string? targetInventoryId = null, int? seed = null)
    {
        var table = package.Game.LootTables.FirstOrDefault(t => RuntimeStateHelpers.IdEquals(t.Id, lootTableId));
        if (table == null)
        {
            return Failure(state, "loot_table.missing", $"Loot table not found: {lootTableId}", lootTableId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var entries = GetEligibleEntries(package, working, table, targetInventoryId, result);
        if (entries.Count == 0)
        {
            result.Success = false;
            result.Message = $"Loot table has no eligible entries: {table.Id}";
            result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("loot_table.no_entries", result.Message, table.Id));
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, result.Message, table.Id));
            return result;
        }

        var random = new Random(seed ?? RuntimeStateHelpers.StableSeed($"{package.Manifest.PackageId}:{table.Id}:{working.Tick}"));
        var selected = SelectWeighted(entries, random);
        var output = CopyOutput(selected.Output);
        output.Amount = ResolveEntryAmount(selected, random, output.Amount);
        if (selected.QuestItem)
        {
            output.Metadata["questItem"] = "true";
        }

        var outputResult = _outputApplier.Apply(package, working, new[] { output }, targetInventoryId, seed);
        result.Events.AddRange(outputResult.Events);
        result.Diagnostics.AddRange(outputResult.Diagnostics);
        if (!outputResult.Success)
        {
            result.Success = false;
            result.Message = $"Loot roll failed: {table.Id}";
            return result;
        }

        if (selected.Unique || selected.MaxGlobalCount.HasValue)
        {
            RuntimeStateHelpers.IncrementGlobalLootCount(working, selected.Id);
        }

        if (!string.IsNullOrWhiteSpace(selected.SetFlagOnDrop))
        {
            RuntimeStateHelpers.SetFlag(working, selected.SetFlagOnDrop!, "true");
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Success = true;
        result.Message = $"Loot rolled: {table.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LootRolled, $"Loot rolled: {table.Name} -> {selected.Id}", table.Id));
        return result;
    }

    private List<LootEntryDefinition> GetEligibleEntries(GamePackageDefinition package, GameRuntimeState state, LootTableDefinition table, string? targetInventoryId, GameRuntimeResult result)
    {
        var entries = new List<LootEntryDefinition>();
        foreach (var entry in table.Entries)
        {
            if (entry.Weight.GetValueOrDefault(1) <= 0)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(entry.RequiresFlag)
                && !string.Equals(RuntimeStateHelpers.GetFlagValue(state, entry.RequiresFlag!), "true", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (entry.Unique && RuntimeStateHelpers.IsUniqueLootAlreadyAcquired(state, entry.Id))
            {
                continue;
            }

            if (entry.MaxGlobalCount.HasValue && RuntimeStateHelpers.GetGlobalLootCount(state, entry.Id) >= entry.MaxGlobalCount.Value)
            {
                continue;
            }

            var requirements = _requirementEvaluator.Evaluate(package, state, entry.Requirements, targetInventoryId);
            if (!requirements.Success)
            {
                foreach (var failure in requirements.Failures)
                {
                    result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(failure.Code, failure.Message, failure.TargetId, "warning"));
                }

                continue;
            }

            entries.Add(entry);
        }

        return entries;
    }

    private static LootEntryDefinition SelectWeighted(IReadOnlyList<LootEntryDefinition> entries, Random random)
    {
        var totalWeight = entries.Sum(e => e.Weight.GetValueOrDefault(1));
        var roll = random.NextDouble() * totalWeight;
        foreach (var entry in entries)
        {
            roll -= entry.Weight.GetValueOrDefault(1);
            if (roll <= 0)
            {
                return entry;
            }
        }

        return entries[entries.Count - 1];
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

    private static OutputDefinition CopyOutput(OutputDefinition source)
    {
        return new OutputDefinition
        {
            Kind = source.Kind,
            Id = source.Id,
            Amount = source.Amount,
            Scope = source.Scope,
            Mode = source.Mode,
            Tags = source.Tags.ToList(),
            Metadata = new Dictionary<string, string>(source.Metadata)
        };
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string targetId)
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
