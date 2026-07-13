using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;

public sealed class CapabilityDrivenRuntimePlaythroughValidator
{
    private readonly CapabilityDrivenRuntimePlaythroughExpansionService _expansionService;

    public CapabilityDrivenRuntimePlaythroughValidator(
        CapabilityDrivenRuntimePlaythroughExpansionService? expansionService = null)
    {
        _expansionService = expansionService ?? new CapabilityDrivenRuntimePlaythroughExpansionService();
    }

    public CapabilityDrivenRuntimePlaythroughPlanningResult Validate(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        GamePackageDefinition package)
    {
        ArgumentNullException.ThrowIfNull(selectedModules);
        ArgumentNullException.ThrowIfNull(package);
        var diagnostics = new List<string>();
        ValidateParticipantResourceDomains(package, diagnostics);
        var originalContracts = selectedModules.SelectMany(module => module.RuntimePlaythroughContracts).ToList();
        var replacements = originalContracts.SelectMany(contract => contract.ReplacesActionIds
                .Select(actionId => (Replacement: contract, ActionId: actionId)))
            .ToList();
        foreach (var replacement in replacements)
        {
            var count = originalContracts.Count(contract => contract.ActionId == replacement.ActionId);
            if (count != 1 || replacement.Replacement.ActionId == replacement.ActionId)
                diagnostics.Add("playthrough action replacement rejected: " + replacement.Replacement.ActionId
                                + "->" + replacement.ActionId + ":matches=" + count);
        }
        var replacedIds = replacements.Select(item => item.ActionId).ToHashSet(StringComparer.Ordinal);
        var replacementModules = selectedModules.Select(module => module with
        {
            RuntimePlaythroughContracts = module.RuntimePlaythroughContracts
                .Where(contract => !replacedIds.Contains(contract.ActionId)).ToList()
        }).ToList();
        var expansion = _expansionService.Expand(replacementModules, package);
        diagnostics.AddRange(expansion.Diagnostics);
        var contracts = expansion.Modules.SelectMany(module => module.RuntimePlaythroughContracts
                .Select(contract => (ModuleId: module.ModuleId, Contract: contract)))
            .ToList();

        foreach (var duplicate in contracts.GroupBy(item => item.Contract.ActionId, StringComparer.Ordinal)
                     .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
            diagnostics.Add("duplicate action ID rejected: " + duplicate.Key);
        if (diagnostics.Count > 0)
            return new CapabilityDrivenRuntimePlaythroughPlanningResult
            {
                Diagnostics = diagnostics,
                Plan = new CapabilityRuntimePlaythroughPlan { Diagnostics = diagnostics }
            };

        var actionIds = contracts.Select(item => item.Contract.ActionId).ToHashSet(StringComparer.Ordinal);
        foreach (var item in package.Game.Items.Where(item => item.Metadata.ContainsKey("combat_damage_bonus")))
        {
            var raw = item.Metadata["combat_damage_bonus"];
            if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var bonus) || bonus < 0)
                diagnostics.Add("invalid weapon damage bonus rejected: " + item.Id + ":" + raw);
        }
        ValidateStatDamageMetadata(package, diagnostics);
        foreach (var item in contracts)
        {
            var contract = item.Contract;
            if (string.IsNullOrWhiteSpace(contract.ContractId)
                || string.IsNullOrWhiteSpace(contract.CapabilityId)
                || string.IsNullOrWhiteSpace(contract.RuntimePrimitiveId))
                diagnostics.Add("playthrough contract metadata missing: " + item.ModuleId + ":" + contract.ActionId);
            if (!CapabilityRuntimePrimitiveIds.Supported.Contains(contract.RuntimePrimitiveId))
                diagnostics.Add("unknown Runtime primitive rejected: " + contract.RuntimePrimitiveId);
            foreach (var dependency in contract.DependsOnActionIds.Where(id => !actionIds.Contains(id)))
                diagnostics.Add("missing action dependency rejected: " + contract.ActionId + "->" + dependency);
        }

        if (!HasCycle(contracts.Select(item => item.Contract).ToList(), actionIds))
        {
            var resolved = new List<CapabilityRuntimePlaythroughAction>();
            foreach (var item in contracts)
            {
                var target = ResolveTarget(package, item.Contract.TargetSelector, item.Contract.Args, diagnostics,
                    item.Contract.ActionId);
                ValidateReferencedArgs(package, item.Contract, diagnostics);
                resolved.Add(new CapabilityRuntimePlaythroughAction
                {
                    ContractId = item.Contract.ContractId,
                    CapabilityId = item.Contract.CapabilityId,
                    ActionId = item.Contract.ActionId,
                    Category = item.Contract.Category,
                    Phase = item.Contract.Phase,
                    Order = item.Contract.Order,
                    RuntimePrimitiveId = item.Contract.RuntimePrimitiveId,
                    TargetSelector = item.Contract.TargetSelector,
                    ResolvedTargetId = target,
                    Args = new SortedDictionary<string, string>(item.Contract.Args.ToDictionary(pair => pair.Key,
                        pair => pair.Value, StringComparer.Ordinal), StringComparer.Ordinal),
                    DependsOnActionIds = item.Contract.DependsOnActionIds.OrderBy(id => id, StringComparer.Ordinal).ToList(),
                    CheckpointBoundaryAfter = item.Contract.CheckpointBoundaryAfter,
                    PresentationOnly = item.Contract.PresentationOnly,
                    Required = item.Contract.Required,
                    ExpectedRuntimeEffects = item.Contract.ExpectedRuntimeEffects.OrderBy(id => id, StringComparer.Ordinal).ToList()
                });
            }

            return new CapabilityDrivenRuntimePlaythroughPlanningResult
            {
                Passed = diagnostics.Count == 0,
                Plan = new CapabilityRuntimePlaythroughPlan { OrderedActions = resolved, Diagnostics = diagnostics },
                Diagnostics = diagnostics
            };
        }

        diagnostics.Add("action dependency cycle rejected");
        return new CapabilityDrivenRuntimePlaythroughPlanningResult
        {
            Diagnostics = diagnostics,
            Plan = new CapabilityRuntimePlaythroughPlan { Diagnostics = diagnostics }
        };
    }

    private static bool HasCycle(
        IReadOnlyList<FeatureModuleRuntimePlaythroughContract> contracts,
        IReadOnlySet<string> actionIds)
    {
        var indegree = contracts.ToDictionary(item => item.ActionId, _ => 0, StringComparer.Ordinal);
        var edges = contracts.ToDictionary(item => item.ActionId, _ => new List<string>(), StringComparer.Ordinal);
        foreach (var contract in contracts)
        foreach (var dependency in contract.DependsOnActionIds.Where(actionIds.Contains))
        {
            indegree[contract.ActionId]++;
            edges[dependency].Add(contract.ActionId);
        }
        var queue = new Queue<string>(indegree.Where(pair => pair.Value == 0).Select(pair => pair.Key));
        var visited = 0;
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            visited++;
            foreach (var dependent in edges[id])
                if (--indegree[dependent] == 0) queue.Enqueue(dependent);
        }
        return visited != contracts.Count;
    }

    private static string ResolveTarget(
        GamePackageDefinition package,
        string selector,
        IReadOnlyDictionary<string, string> args,
        List<string> diagnostics,
        string actionId)
    {
        var explicitId = args.GetValueOrDefault("id") ?? string.Empty;
        IReadOnlyList<string> matches = selector switch
        {
            "manifest_package" => Match(package.Manifest.PackageId, explicitId),
            "start_map" => package.Game.Maps.Where(item => item.Id == package.Manifest.StartMapId
                                                            && (explicitId.Length == 0 || item.Id == explicitId))
                .Select(item => item.Id).ToList(),
            "entity_id" => package.Game.Maps.SelectMany(map => map.Entities).Where(item => item.Id == explicitId)
                .Select(item => item.Id).ToList(),
            "interaction_id" => Matches(package.Game.Interactions.Select(item => item.Id), explicitId),
            "dialogue_id" => Matches(package.Game.Dialogues.Select(item => item.Id), explicitId),
            "quest_id" => Matches(package.Game.Quests.Select(item => item.Id), explicitId),
            "inventory_id" => Matches(package.Game.Inventories.Select(item => item.Id), explicitId),
            "container_inventory_id" => Matches(package.Game.Inventories
                .Where(item => item.OwnerKind.Equals("container", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Id), explicitId),
            "recipe_id" => Matches(package.Game.Recipes.Select(item => item.Id), explicitId),
            "resource_node_id" => Matches(package.Game.ResourceNodes.Select(item => item.Id), explicitId),
            "transaction_id" => Matches(package.Game.Transactions.Select(item => item.Id), explicitId),
            "encounter_id" => Matches(package.Game.Encounters.Select(item => item.Id), explicitId),
            "encounter_participant_id" => Matches(package.Game.Encounters.SelectMany(item => item.Participants)
                .Select(item => item.Id), explicitId),
            "ability_id" => Matches(package.Game.Abilities.Select(item => item.Id), explicitId),
            "item_id" => Matches(package.Game.Items.Select(item => item.Id), explicitId),
            "equipment_slot_id" => Matches(package.Game.EquipmentSlots.Select(item => item.Id), explicitId),
            "stat_id" => Matches(package.Game.Stats.Select(item => item.Id), explicitId),
            "progression_id" => Matches(package.Game.Progressions.Select(item => item.Id), explicitId),
            _ => []
        };
        if (matches.Count == 0)
            diagnostics.Add((KnownSelector(selector) ? "unresolved target rejected: " : "unknown target selector rejected: ")
                            + actionId + ":" + selector + ":" + explicitId);
        else if (matches.Count > 1)
            diagnostics.Add("ambiguous target rejected: " + actionId + ":" + selector + ":" + explicitId);
        return matches.Count == 1 ? matches[0] : string.Empty;
    }

    private static void ValidateReferencedArgs(
        GamePackageDefinition package,
        FeatureModuleRuntimePlaythroughContract contract,
        List<string> diagnostics)
    {
        var references = new (string Key, string Selector)[]
        {
            ("entityId", "entity_id"), ("interactionId", "interaction_id"),
            ("dialogueId", "dialogue_id"), ("questId", "quest_id"),
            ("inventoryId", "inventory_id"), ("sourceInventoryId", "container_inventory_id"),
            ("targetInventoryId", "inventory_id"), ("recipeId", "recipe_id"),
            ("resourceNodeId", "resource_node_id"), ("transactionId", "transaction_id"),
            ("encounterId", "encounter_id"), ("participantId", "encounter_participant_id"),
            ("sourceParticipantId", "encounter_participant_id"), ("targetParticipantId", "encounter_participant_id"),
            ("abilityId", "ability_id"),
            ("itemId", "item_id"), ("slotId", "equipment_slot_id"),
            ("statId", "stat_id"), ("progressionId", "progression_id")
        };
        foreach (var reference in references)
        {
            if (!contract.Args.TryGetValue(reference.Key, out var value) || string.IsNullOrWhiteSpace(value)) continue;
            ResolveTarget(package, reference.Selector,
                new Dictionary<string, string>(StringComparer.Ordinal) { ["id"] = value }, diagnostics,
                contract.ActionId + "." + reference.Key);
        }
    }

    private static IReadOnlyList<string> Match(string actual, string expected) =>
        expected.Length == 0 || actual == expected ? [actual] : [];

    private static IReadOnlyList<string> Matches(IEnumerable<string> values, string expected) =>
        values.Where(value => value == expected).ToList();

    private static bool KnownSelector(string selector) => selector is "manifest_package" or "start_map"
        or "entity_id" or "interaction_id" or "dialogue_id" or "quest_id" or "inventory_id"
        or "container_inventory_id" or "recipe_id" or "resource_node_id" or "transaction_id"
        or "encounter_id" or "encounter_participant_id" or "ability_id" or "item_id" or "equipment_slot_id"
        or "stat_id" or "progression_id";

    private static void ValidateStatDamageMetadata(
        GamePackageDefinition package,
        List<string> diagnostics)
    {
        const string statIdKey = "source_stat_damage_stat_id";
        const string baselineKey = "source_stat_damage_baseline";
        const string perPointKey = "source_stat_damage_per_point";
        foreach (var ability in package.Game.Abilities.Where(ability =>
                     ability.Metadata.ContainsKey(statIdKey)
                     || ability.Metadata.ContainsKey(baselineKey)
                     || ability.Metadata.ContainsKey(perPointKey)))
        {
            if (!ability.Metadata.TryGetValue(statIdKey, out var statId)
                || !ability.Metadata.TryGetValue(baselineKey, out var baseline)
                || !ability.Metadata.TryGetValue(perPointKey, out var perPoint)
                || string.IsNullOrWhiteSpace(statId))
            {
                diagnostics.Add("invalid stat damage metadata rejected: " + ability.Id);
                continue;
            }

            if (package.Game.Stats.Count(stat => stat.Id == statId) != 1)
                diagnostics.Add("missing or ambiguous source stat rejected: " + ability.Id + ":" + statId);
            if (!decimal.TryParse(baseline, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                diagnostics.Add("invalid stat baseline rejected: " + ability.Id + ":" + baseline);
            if (!decimal.TryParse(perPoint, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var multiplier) || multiplier < 0)
                diagnostics.Add("invalid stat multiplier rejected: " + ability.Id + ":" + perPoint);
        }
    }

    private static void ValidateParticipantResourceDomains(GamePackageDefinition package, List<string> diagnostics)
    {
        foreach (var encounter in package.Game.Encounters)
        foreach (var participant in encounter.Participants)
        foreach (var resource in participant.Resources.Where(item => item.Kind == "resource"))
        {
            var definitions = package.Game.Resources.Where(definition => definition.Id == resource.Id).ToList();
            if (definitions.Count != 1)
            {
                diagnostics.Add("participant resource definition rejected: " + encounter.Id + ":" + participant.Id + ":"
                                + resource.Id + ":matches=" + definitions.Count);
                continue;
            }

            if (!double.IsFinite(resource.Amount))
            {
                diagnostics.Add("participant resource amount must be finite: " + encounter.Id + ":" + participant.Id + ":" + resource.Id);
                continue;
            }

            var definition = definitions[0];
            if (definition.MinValue is { } minimum && resource.Amount < minimum)
                diagnostics.Add("participant resource amount below minimum rejected: " + encounter.Id + ":" + participant.Id + ":"
                                + resource.Id + ":amount=" + resource.Amount + ":minimum=" + minimum);
            if (definition.MaxValue is { } maximum && resource.Amount > maximum)
                diagnostics.Add("participant resource amount above maximum rejected: " + encounter.Id + ":" + participant.Id + ":"
                                + resource.Id + ":amount=" + resource.Amount + ":maximum=" + maximum);
        }
    }
}
