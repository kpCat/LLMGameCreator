namespace LLMGameCreator.Application.Design.SemanticArtifactContracts;

public sealed class SemanticArtifactCompatibilityPlanner
{
    private static readonly Dictionary<string, string> SlotFamiliesByContract = new(StringComparer.Ordinal)
    {
        ["entity_archetype_npc_actor_profile_v1"] = "npc_actor_archetype_variation",
        ["faction_reputation_social_relation_v1"] = "faction_reputation_relation",
        ["quest_graph_objective_reward_pattern_v1"] = "quest_motive_objective_pattern",
        ["dialogue_string_table_localization_hint_v1"] = "dialogue_tone_localization_string_table_hint",
        ["biome_weather_hazard_event_hints_v1"] = "biome_weather_hazard_event_hint",
        ["item_resource_recipe_loot_economy_v1"] = "item_resource_recipe_loot_hint",
        ["combat_progression_ability_v1"] = "combat_progression_ability_hint",
        ["settlement_building_landmark_v1"] = "settlement_region_route_landmark_hint"
    };

    private readonly IReadOnlyList<SemanticArtifactContractDescriptor> contracts;

    public SemanticArtifactCompatibilityPlanner(IReadOnlyList<SemanticArtifactContractDescriptor>? contracts = null)
    {
        this.contracts = contracts ?? SemanticArtifactContractRegistry.BuildDefaultContracts();
    }

    public SemanticCompatibilityPlan BuildPlan(SemanticCompatibilityRequest request)
    {
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        var registryDiagnostics = SemanticArtifactContractValidator.ValidateContracts(contracts);
        diagnostics.AddRange(registryDiagnostics);
        var byId = contracts
            .GroupBy(contract => contract.ContractId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var packs = request.SelectedSemanticPacks
            .Where(pack => IsPackCompatible(pack, request.ProfileId))
            .OrderBy(pack => pack.OrderingKey, StringComparer.Ordinal)
            .ThenBy(pack => pack.PackId, StringComparer.Ordinal)
            .ToList();
        var scopeSet = packs.SelectMany(pack => pack.SemanticScopes).ToHashSet(StringComparer.Ordinal);
        var tagSet = packs.SelectMany(pack => pack.SemanticTags).ToHashSet(StringComparer.Ordinal);
        var selectedIds = new List<string>();
        var missingDependencies = new List<SemanticMissingDependency>();
        var conflicts = new List<SemanticCompatibilityConflict>();
        var blockedItems = new List<SemanticBlockedItem>();
        var slots = new List<SemanticExpansionSlot>();
        var availableModules = request.AvailableModuleIds.Count == 0
            ? SemanticArtifactContractRegistry.DefaultAvailableModuleIds
            : request.AvailableModuleIds;

        foreach (var requestedId in request.RequestedContractIds.Order(StringComparer.Ordinal))
        {
            if (!byId.ContainsKey(requestedId))
            {
                diagnostics.Add(SemanticArtifactContractValidator.Diagnostic("error", "semantic_plan.contract.unknown", requestedId, "Planner request references an unknown contract id."));
            }
        }

        foreach (var contract in contracts.OrderBy(contract => contract.ContractId, StringComparer.Ordinal))
        {
            if (request.RequestedContractIds.Count > 0 && !request.RequestedContractIds.Contains(contract.ContractId, StringComparer.Ordinal))
            {
                continue;
            }

            var missingScopes = contract.RequiredSemanticScopes
                .Where(scope => !scopeSet.Contains(scope))
                .Order(StringComparer.Ordinal)
                .ToList();
            if (missingScopes.Count > 0)
            {
                conflicts.Add(new SemanticCompatibilityConflict
                {
                    ContractId = contract.ContractId,
                    ConflictId = "missing_semantic_scope",
                    Reason = string.Join(",", missingScopes)
                });
                diagnostics.Add(SemanticArtifactContractValidator.Diagnostic("warning", "semantic_plan.semantic_scope.missing", contract.ContractId, $"Missing semantic scopes: {string.Join(",", missingScopes)}."));
                continue;
            }

            foreach (var dependency in contract.Dependencies.Order(StringComparer.Ordinal))
            {
                if (!byId.ContainsKey(dependency))
                {
                    missingDependencies.Add(new SemanticMissingDependency { ContractId = contract.ContractId, MissingDependencyId = dependency });
                }
            }

            if (missingDependencies.Any(item => item.ContractId == contract.ContractId))
            {
                continue;
            }

            if (contract.LifecycleStatus is "blocked" or "future_required" or "deprecated")
            {
                blockedItems.Add(new SemanticBlockedItem
                {
                    ContractId = contract.ContractId,
                    Status = contract.LifecycleStatus,
                    Reason = "not_ready_for_goal_030"
                });
            }
            else if (!string.IsNullOrWhiteSpace(contract.ModuleId) && !availableModules.Contains(contract.ModuleId))
            {
                var status = contract.LifecycleStatus == "optional" ? "absent_optional" : "missing_required";
                blockedItems.Add(new SemanticBlockedItem
                {
                    ContractId = contract.ContractId,
                    Status = status,
                    Reason = contract.ModuleId
                });
                diagnostics.Add(SemanticArtifactContractValidator.Diagnostic(
                    contract.LifecycleStatus == "optional" ? "info" : "error",
                    contract.LifecycleStatus == "optional" ? "semantic_plan.module_absent.optional" : "semantic_plan.module_absent.required",
                    contract.ContractId,
                    $"Module '{contract.ModuleId}' is not available."));
                continue;
            }
            else
            {
                selectedIds.Add(contract.ContractId);
            }

            slots.AddRange(BuildSlots(request.ProfileId, contract, packs, scopeSet, tagSet, blockedItems));
        }

        var dependencyOrder = SemanticArtifactContractValidator.ResolveDependencyOrder(contracts)
            .Where(id => selectedIds.Contains(id, StringComparer.Ordinal) || blockedItems.Any(item => item.ContractId == id))
            .ToList();
        var selectedPackIds = packs.Select(pack => pack.PackId).Order(StringComparer.Ordinal).ToList();
        var sortedSelectedIds = selectedIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        var sortedSlots = slots
            .OrderBy(slot => slot.Priority)
            .ThenBy(slot => slot.SlotId, StringComparer.Ordinal)
            .ToList();

        return new SemanticCompatibilityPlan
        {
            ProfileId = request.ProfileId,
            SelectedContractIds = sortedSelectedIds,
            SelectedSemanticPackIds = selectedPackIds,
            DependencyOrder = dependencyOrder,
            MissingDependencies = missingDependencies
                .OrderBy(item => item.ContractId, StringComparer.Ordinal)
                .ThenBy(item => item.MissingDependencyId, StringComparer.Ordinal)
                .ToList(),
            Conflicts = conflicts
                .OrderBy(item => item.ContractId, StringComparer.Ordinal)
                .ThenBy(item => item.ConflictId, StringComparer.Ordinal)
                .ToList(),
            BlockedOrFutureRequiredItems = blockedItems
                .DistinctBy(item => $"{item.ContractId}|{item.Status}|{item.Reason}")
                .OrderBy(item => item.ContractId, StringComparer.Ordinal)
                .ThenBy(item => item.Status, StringComparer.Ordinal)
                .ToList(),
            SemanticExpansionSlots = sortedSlots,
            StableSummary = $"{request.ProfileId}|packs={selectedPackIds.Count}|contracts={sortedSelectedIds.Count}|slots={sortedSlots.Count}|blocked={blockedItems.Count}|missing={missingDependencies.Count}",
            Diagnostics = SemanticArtifactContractValidator.SortDiagnostics(diagnostics)
        };
    }

    private static IEnumerable<SemanticExpansionSlot> BuildSlots(
        string profileId,
        SemanticArtifactContractDescriptor contract,
        IReadOnlyList<SemanticPackDescriptor> packs,
        IReadOnlySet<string> scopeSet,
        IReadOnlySet<string> tagSet,
        IReadOnlyList<SemanticBlockedItem> blockedItems)
    {
        if (!SlotFamiliesByContract.TryGetValue(contract.ContractId, out var slotFamily))
        {
            return [];
        }

        var status = blockedItems.FirstOrDefault(item => item.ContractId == contract.ContractId)?.Status ?? contract.LifecycleStatus;
        var priorityBase = contract.LifecycleStatus switch
        {
            "ready" => 100,
            "optional" => 200,
            "future_required" => 800,
            _ => 900
        };
        var matchingPacks = packs
            .Where(pack => pack.SemanticScopes.Intersect(contract.RequiredSemanticScopes.Concat(contract.OptionalSemanticScopes), StringComparer.Ordinal).Any()
                           || pack.ExpansionHints.Any(hint => slotFamily.Contains(hint.Split('_')[0], StringComparison.Ordinal)))
            .OrderBy(pack => pack.OrderingKey, StringComparer.Ordinal)
            .ThenBy(pack => pack.PackId, StringComparer.Ordinal)
            .ToList();

        return matchingPacks.Select((pack, index) => new SemanticExpansionSlot
        {
            SlotId = $"{profileId}:{SafeTail(pack.PackId)}:{contract.ContractId}:{slotFamily}",
            SlotFamily = slotFamily,
            SourceSemanticPackId = pack.PackId,
            TargetArtifactContractId = contract.ContractId,
            TargetArtifactKind = contract.ArtifactKind,
            ProfileId = profileId,
            SemanticScopesUsed = pack.SemanticScopes.Where(scopeSet.Contains).Intersect(contract.RequiredSemanticScopes.Concat(contract.OptionalSemanticScopes), StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            SemanticTagsUsed = pack.SemanticTags.Where(tagSet.Contains).Order(StringComparer.Ordinal).Take(8).ToList(),
            Priority = priorityBase + index,
            Status = status,
            Diagnostics = status is "ready" or "optional"
                ? []
                : [SemanticArtifactContractValidator.Diagnostic("warning", "semantic_slot.status.not_ready", contract.ContractId, $"Slot remains {status}.")]
        });
    }

    private static bool IsPackCompatible(SemanticPackDescriptor pack, string profileId) =>
        pack.SupportedProfileIds.Contains(profileId, StringComparer.Ordinal)
        || pack.SupportedProfileIds.Contains("*", StringComparer.Ordinal);

    private static string SafeTail(string packId)
    {
        var index = packId.LastIndexOf('/');
        return index >= 0 ? packId[(index + 1)..] : packId;
    }
}
