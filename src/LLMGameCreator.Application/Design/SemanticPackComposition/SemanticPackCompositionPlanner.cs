using LLMGameCreator.Application.Design.SemanticArtifactContracts;

namespace LLMGameCreator.Application.Design.SemanticPackComposition;

public sealed class SemanticPackCompositionPlanner
{
    private static readonly IReadOnlyList<SectionSpec> SectionSpecs =
    [
        new("world_route_pressure", "World regions / route pressure", ["world_region", "route_pressure"]),
        new("biome_weather_hazard_event_pressure", "Biome/weather/hazard/event pressure", ["biome_hazard", "weather_event", "global_event"]),
        new("factions_reputation_social_relations", "Factions and reputation/social relation anchors", ["faction_role", "reputation_axis", "social_relation"]),
        new("npc_archetype_variation", "NPC archetype variation anchors", ["npc_archetype"]),
        new("quest_motive_objective_reward_patterns", "Quest motive/objective/reward pattern anchors", ["quest_motive", "quest_objective", "loot_theme"]),
        new("dialogue_localization_hints", "Dialogue tone/localization/string-table hints", ["dialogue_tone", "localization_hint"]),
        new("economy_resource_recipe_loot_chains", "Economy/resource/recipe/loot chains", ["economy_chain", "resource_theme", "recipe_theme", "loot_theme"]),
        new("combat_progression_ability_pressure", "Combat/progression/ability pressures", ["combat_pressure", "progression_axis"]),
        new("settlement_landmark_anchors", "Settlement/building/landmark anchors", ["settlement_pattern", "landmark_theme"]),
        new("global_events", "Global events", ["global_event"]),
        new("coverage_gaps_future_required", "Coverage gaps and future-required items", [])
    ];

    private readonly IReadOnlyList<SemanticPackCompositionPack> packs;
    private readonly IReadOnlyList<SemanticArtifactContractDescriptor> contracts;
    private readonly SemanticArtifactCompatibilityPlanner goal030Planner;

    public SemanticPackCompositionPlanner(
        IReadOnlyList<SemanticPackCompositionPack>? packs = null,
        IReadOnlyList<SemanticArtifactContractDescriptor>? contracts = null,
        SemanticArtifactCompatibilityPlanner? goal030Planner = null)
    {
        this.packs = packs ?? SemanticPackCompositionCatalog.BuildDefaultPacks();
        this.contracts = contracts ?? SemanticArtifactContractRegistry.BuildDefaultContracts();
        this.goal030Planner = goal030Planner ?? new SemanticArtifactCompatibilityPlanner(this.contracts);
    }

    public SemanticBlueprintPlan BuildBlueprint(SemanticPackCompositionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var diagnostics = new List<SemanticArtifactDiagnostic>();
        diagnostics.AddRange(SemanticPackCompositionValidator.ValidateCatalog(packs, contracts));
        diagnostics.AddRange(SemanticPackCompositionValidator.ValidateRequest(request, packs));
        var byPackId = packs
            .GroupBy(pack => pack.PackId, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var selected = new List<SemanticPackCompositionPack>();
        var rejected = new List<SemanticRejectedPack>();

        foreach (var packId in request.SelectedPackIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (!byPackId.TryGetValue(packId, out var pack))
            {
                rejected.Add(Rejected(packId, "semantic_pack.request.pack_id.unknown", "Selected semantic pack id is not in the seed catalog."));
                continue;
            }

            if (pack.IsFutureOnly)
            {
                rejected.Add(Rejected(packId, "semantic_pack.request.pack.future_only", "Future-only semantic pack cannot be selected as ready."));
                continue;
            }

            if (!SemanticPackCompositionValidator.IsPackCompatible(pack, request.ProfileId))
            {
                rejected.Add(Rejected(packId, "semantic_pack.request.profile.unsupported", "Selected semantic pack does not support the requested profile/family id."));
                continue;
            }

            selected.Add(pack);
        }

        rejected.AddRange(RejectExcludedPacks(selected));
        var rejectedIds = rejected.Select(item => item.PackId).ToHashSet(StringComparer.Ordinal);
        selected = selected
            .Where(pack => !rejectedIds.Contains(pack.PackId))
            .OrderBy(pack => pack.Priority)
            .ThenBy(pack => pack.OrderingKey, StringComparer.Ordinal)
            .ThenBy(pack => pack.PackId, StringComparer.Ordinal)
            .ToList();
        var selectedFacts = selected
            .SelectMany(pack => pack.Facts.Select(fact => new SemanticPackMergedFact
            {
                FactId = fact.FactId,
                Domain = fact.Domain,
                Value = fact.Value,
                SourcePackId = pack.PackId,
                Tags = fact.Tags.Order(StringComparer.Ordinal).ToList()
            }))
            .GroupBy(fact => fact.FactId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Domain, StringComparer.Ordinal)
            .ThenBy(fact => fact.FactId, StringComparer.Ordinal)
            .ToList();
        var selectedFactIds = selectedFacts.Select(fact => fact.FactId).ToHashSet(StringComparer.Ordinal);
        var relations = selected
            .SelectMany(pack => pack.RelationHints)
            .Where(relation => selectedFactIds.Contains(relation.SourceFactId) && selectedFactIds.Contains(relation.TargetFactId))
            .GroupBy(relation => relation.RelationId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.RelationId, StringComparer.Ordinal)
            .ToList();
        var compatibilityPlan = goal030Planner.BuildPlan(new SemanticCompatibilityRequest
        {
            ProfileId = request.ProfileId,
            SelectedSemanticPacks = selected.Select(ToGoal030Pack).ToList()
        });
        diagnostics.AddRange(compatibilityPlan.Diagnostics);
        var resolvedIntents = ResolveExpansionIntents(selected, selectedFactIds, compatibilityPlan).ToList();
        var links = BuildCrossArtifactLinks(selectedFacts, compatibilityPlan.SelectedContractIds).ToList();
        var gaps = BuildGaps(compatibilityPlan, resolvedIntents).ToList();
        var sections = BuildSections(selectedFacts, relations, resolvedIntents, gaps).ToList();
        var selectedPackIds = selected.Select(pack => pack.PackId).Order(StringComparer.Ordinal).ToList();

        return new SemanticBlueprintPlan
        {
            ProfileId = request.ProfileId,
            ComplexityHint = string.IsNullOrWhiteSpace(request.ComplexityHint) ? "standard" : request.ComplexityHint,
            SelectedPackIds = selectedPackIds,
            RejectedPacks = rejected
                .DistinctBy(item => $"{item.PackId}|{item.ReasonCode}")
                .OrderBy(item => item.PackId, StringComparer.Ordinal)
                .ThenBy(item => item.ReasonCode, StringComparer.Ordinal)
                .ToList(),
            MergedSemanticFacts = selectedFacts,
            RelationGraph = relations,
            ResolvedExpansionIntents = resolvedIntents
                .OrderBy(intent => intent.Priority)
                .ThenBy(intent => intent.IntentId, StringComparer.Ordinal)
                .ToList(),
            Goal030CoverageContractIds = compatibilityPlan.SelectedContractIds,
            CrossArtifactLinks = links,
            Sections = sections,
            CoverageGapsAndFutureRequiredItems = gaps,
            StableSummary = $"{request.ProfileId}|packs={selectedPackIds.Count}|facts={selectedFacts.Count}|relations={relations.Count}|links={links.Count}|contracts={compatibilityPlan.SelectedContractIds.Count}|gaps={gaps.Count}|rejected={rejected.Count}",
            Diagnostics = SemanticPackCompositionValidator.SortDiagnostics(diagnostics)
        };
    }

    private static IEnumerable<SemanticRejectedPack> RejectExcludedPacks(IReadOnlyList<SemanticPackCompositionPack> selected)
    {
        var selectedIds = selected.Select(pack => pack.PackId).ToHashSet(StringComparer.Ordinal);
        foreach (var pack in selected.OrderBy(pack => pack.Priority).ThenBy(pack => pack.PackId, StringComparer.Ordinal))
        {
            foreach (var excluded in pack.Exclusions.Order(StringComparer.Ordinal))
            {
                if (selectedIds.Contains(excluded))
                {
                    yield return Rejected(excluded, "semantic_pack.selection.exclusion.incompatible", $"Excluded by {pack.PackId}.");
                }
            }
        }
    }

    private static IEnumerable<SemanticResolvedExpansionIntent> ResolveExpansionIntents(
        IReadOnlyList<SemanticPackCompositionPack> selected,
        IReadOnlySet<string> selectedFactIds,
        SemanticCompatibilityPlan compatibilityPlan)
    {
        var selectedContracts = compatibilityPlan.SelectedContractIds.ToHashSet(StringComparer.Ordinal);
        var blockedByContract = compatibilityPlan.BlockedOrFutureRequiredItems
            .GroupBy(item => item.ContractId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        foreach (var pack in selected)
        {
            foreach (var intent in pack.ExpansionIntents.OrderBy(intent => intent.Priority).ThenBy(intent => intent.IntentId, StringComparer.Ordinal))
            {
                if (!selectedFactIds.Contains(intent.SourceFactId))
                {
                    yield return ResolvedIntent(pack.PackId, intent, "blocked", "semantic_pack.expansion_intent.fact.unknown", "Intent source fact was not selected.");
                    continue;
                }

                if (selectedContracts.Contains(intent.TargetContractId))
                {
                    yield return ResolvedIntent(pack.PackId, intent, "ready");
                    continue;
                }

                if (blockedByContract.TryGetValue(intent.TargetContractId, out var blocked))
                {
                    yield return ResolvedIntent(pack.PackId, intent, blocked.Status, "semantic_pack.expansion_intent.contract.not_ready", blocked.Reason);
                    continue;
                }

                yield return ResolvedIntent(pack.PackId, intent, intent.FutureRequired ? "future_required" : "missing_contract_coverage", "semantic_pack.expansion_intent.contract.not_selected", "Goal 030 planner did not select this contract for the current semantic pack set.");
            }
        }
    }

    private static SemanticResolvedExpansionIntent ResolvedIntent(
        string packId,
        SemanticPackExpansionIntent intent,
        string status,
        string diagnosticCode = "",
        string diagnosticMessage = "") =>
        new()
        {
            IntentId = intent.IntentId,
            SourcePackId = packId,
            SourceFactId = intent.SourceFactId,
            TargetContractId = intent.TargetContractId,
            TargetArtifactKind = intent.TargetArtifactKind,
            Status = status,
            Priority = intent.Priority,
            Diagnostics = string.IsNullOrWhiteSpace(diagnosticCode)
                ? []
                : [SemanticPackCompositionValidator.Diagnostic(status == "blocked" ? "error" : "warning", diagnosticCode, intent.IntentId, diagnosticMessage)]
        };

    private static IEnumerable<SemanticBlueprintGap> BuildGaps(
        SemanticCompatibilityPlan compatibilityPlan,
        IReadOnlyList<SemanticResolvedExpansionIntent> intents)
    {
        foreach (var blocked in compatibilityPlan.BlockedOrFutureRequiredItems.OrderBy(item => item.ContractId, StringComparer.Ordinal).ThenBy(item => item.Status, StringComparer.Ordinal))
        {
            yield return new SemanticBlueprintGap
            {
                GapId = blocked.ContractId,
                Status = blocked.Status,
                Source = "goal_030_compatibility_plan",
                Reason = blocked.Reason
            };
        }

        foreach (var conflict in compatibilityPlan.Conflicts.OrderBy(item => item.ContractId, StringComparer.Ordinal).ThenBy(item => item.ConflictId, StringComparer.Ordinal))
        {
            yield return new SemanticBlueprintGap
            {
                GapId = $"{conflict.ContractId}:{conflict.ConflictId}",
                Status = "missing_or_degraded",
                Source = "goal_030_compatibility_plan",
                Reason = conflict.Reason
            };
        }

        foreach (var intent in intents.Where(item => item.Status is "future_required" or "missing_contract_coverage").OrderBy(item => item.IntentId, StringComparer.Ordinal))
        {
            yield return new SemanticBlueprintGap
            {
                GapId = intent.IntentId,
                Status = intent.Status,
                Source = "semantic_pack_expansion_intent",
                Reason = intent.TargetContractId
            };
        }
    }

    private static IEnumerable<SemanticBlueprintSection> BuildSections(
        IReadOnlyList<SemanticPackMergedFact> facts,
        IReadOnlyList<SemanticPackRelationHint> relations,
        IReadOnlyList<SemanticResolvedExpansionIntent> intents,
        IReadOnlyList<SemanticBlueprintGap> gaps)
    {
        foreach (var spec in SectionSpecs)
        {
            if (spec.Id == "coverage_gaps_future_required")
            {
                yield return new SemanticBlueprintSection
                {
                    SectionId = spec.Id,
                    Title = spec.Title,
                    FactIds = [],
                    RelationIds = [],
                    ExpansionIntentIds = gaps.Select(gap => gap.GapId).Order(StringComparer.Ordinal).ToList(),
                    Summary = $"gaps={gaps.Count}"
                };
                continue;
            }

            var domainSet = spec.Domains.ToHashSet(StringComparer.Ordinal);
            var factIds = facts.Where(fact => domainSet.Contains(fact.Domain)).Select(fact => fact.FactId).Order(StringComparer.Ordinal).ToList();
            var factIdSet = factIds.ToHashSet(StringComparer.Ordinal);
            var relationIds = relations
                .Where(relation => factIdSet.Contains(relation.SourceFactId) || factIdSet.Contains(relation.TargetFactId))
                .Select(relation => relation.RelationId)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();
            var intentIds = intents
                .Where(intent => factIdSet.Contains(intent.SourceFactId))
                .Select(intent => intent.IntentId)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();

            yield return new SemanticBlueprintSection
            {
                SectionId = spec.Id,
                Title = spec.Title,
                FactIds = factIds,
                RelationIds = relationIds,
                ExpansionIntentIds = intentIds,
                Summary = $"facts={factIds.Count}|relations={relationIds.Count}|intents={intentIds.Count}"
            };
        }
    }

    private static IEnumerable<SemanticCrossArtifactLink> BuildCrossArtifactLinks(
        IReadOnlyList<SemanticPackMergedFact> facts,
        IReadOnlyList<string> coverageContractIds)
    {
        var byDomain = facts
            .GroupBy(fact => fact.Domain, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(fact => fact.FactId, StringComparer.Ordinal).ToList(), StringComparer.Ordinal);

        yield return Link(
            "faction_npc_quest_dialogue",
            First(byDomain, "faction_role"),
            First(byDomain, "npc_archetype"),
            First(byDomain, "quest_motive") ?? First(byDomain, "quest_objective"),
            First(byDomain, "dialogue_tone"),
            coverageContractIds,
            ["faction_reputation_social_relation_v1", "entity_archetype_npc_actor_profile_v1", "quest_graph_objective_reward_pattern_v1", "dialogue_string_table_localization_hint_v1"]);

        yield return Link(
            "biome_resource_economy_loot",
            First(byDomain, "biome_hazard") ?? First(byDomain, "weather_event"),
            First(byDomain, "resource_theme"),
            First(byDomain, "economy_chain"),
            First(byDomain, "loot_theme"),
            coverageContractIds,
            ["biome_weather_hazard_event_hints_v1", "item_resource_recipe_loot_economy_v1"]);

        yield return Link(
            "settlement_landmark_route_event",
            First(byDomain, "settlement_pattern"),
            First(byDomain, "landmark_theme") ?? First(byDomain, "world_region"),
            First(byDomain, "route_pressure"),
            First(byDomain, "global_event"),
            coverageContractIds,
            ["world_topology_region_route_graph_v1", "settlement_building_landmark_v1", "biome_weather_hazard_event_hints_v1"]);

        yield return Link(
            "combat_progression_reward_pattern",
            First(byDomain, "combat_pressure"),
            First(byDomain, "progression_axis"),
            First(byDomain, "quest_objective") ?? First(byDomain, "quest_motive"),
            First(byDomain, "loot_theme"),
            coverageContractIds,
            ["combat_progression_ability_v1", "quest_graph_objective_reward_pattern_v1", "item_resource_recipe_loot_economy_v1"]);
    }

    private static SemanticCrossArtifactLink Link(
        string id,
        SemanticPackMergedFact? a,
        SemanticPackMergedFact? b,
        SemanticPackMergedFact? c,
        SemanticPackMergedFact? d,
        IReadOnlyList<string> selectedContracts,
        IReadOnlyList<string> requiredContracts)
    {
        var path = new[] { a, b, c, d }
            .Where(fact => fact != null)
            .Select(fact => fact!.FactId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        var contracts = requiredContracts
            .Where(contract => selectedContracts.Contains(contract, StringComparer.Ordinal) || contract == "settlement_building_landmark_v1")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new SemanticCrossArtifactLink
        {
            LinkId = id,
            FactPath = path,
            ContractIds = contracts,
            Summary = $"{id}|facts={path.Count}|contracts={contracts.Count}"
        };
    }

    private static SemanticPackMergedFact? First(
        IReadOnlyDictionary<string, List<SemanticPackMergedFact>> byDomain,
        string domain) =>
        byDomain.TryGetValue(domain, out var facts) ? facts.FirstOrDefault() : null;

    private static SemanticPackDescriptor ToGoal030Pack(SemanticPackCompositionPack pack) =>
        new()
        {
            PackId = pack.PackId,
            SupportedProfileIds = pack.SupportedProfileIds,
            SemanticScopes = pack.ProvidedSemanticScopes,
            SemanticTags = pack.ThemeTags,
            RelationHints = pack.RelationHints.Select(relation => relation.RelationKind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            ExpansionHints = pack.ExpansionIntents.Select(intent => intent.TargetArtifactKind).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            BlockedCapabilityHints = pack.Exclusions,
            FutureCapabilityHints = pack.ExpansionIntents.Where(intent => intent.FutureRequired).Select(intent => intent.TargetContractId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            OrderingKey = pack.OrderingKey
        };

    private static SemanticRejectedPack Rejected(string id, string code, string reason) =>
        new()
        {
            PackId = id,
            ReasonCode = code,
            Reason = reason
        };

    private sealed record SectionSpec(string Id, string Title, IReadOnlyList<string> Domains);
}
