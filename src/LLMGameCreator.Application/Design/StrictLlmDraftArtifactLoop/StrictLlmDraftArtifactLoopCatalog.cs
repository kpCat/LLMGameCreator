using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

namespace LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;

public static class StrictLlmDraftArtifactLoopCatalog
{
    public static IReadOnlyList<StrictLlmDraftFamily> BuildDraftFamilies() =>
    [
        Family(
            "lore_rule_draft",
            "Lore rule draft",
            ["rule_key", "scope_id", "constraint_tag"],
            ["final_lore_prose", "quest_text", "dialogue_line"],
            ["world", "kingdom", "region", "species", "archetype"],
            ["lore_gap"],
            ["semantic_pack_v1"]),
        Family(
            "species_archetype_feature_draft",
            "Species/archetype feature draft",
            ["slot_id", "feature_id", "value_key"],
            ["species_biography", "final_description"],
            ["species", "archetype", "kingdom"],
            ["lore_gap", "combat_pressure", "settlement_need"],
            ["entity_archetype_npc_actor_profile_v1", "semantic_pack_v1"]),
        Family(
            "faction_relation_draft",
            "Faction relation draft",
            ["faction_id", "relation_axis", "pressure_tag"],
            ["final_reputation_text", "dialogue_line"],
            ["faction", "relationship", "kingdom"],
            ["faction_reaction", "relationship_pressure"],
            ["faction_reputation_social_relation_v1"]),
        Family(
            "npc_role_personality_draft",
            "NPC role/personality draft",
            ["npc_slot_id", "role_tag", "personality_axis"],
            ["npc_backstory_prose", "dialogue_line"],
            ["npc", "archetype", "faction"],
            ["npc_role", "relationship_pressure", "dialogue_act"],
            ["entity_archetype_npc_actor_profile_v1"]),
        Family(
            "quest_motive_objective_draft",
            "Quest motive/objective draft",
            ["quest_slot_id", "motive_tag", "objective_pattern"],
            ["quest_text", "final_lore_prose", "reward_definition"],
            ["quest", "event", "faction"],
            ["quest_motive", "event_intent"],
            ["quest_graph_objective_reward_pattern_v1"]),
        Family(
            "dialogue_act_template_slot_draft",
            "Dialogue act/template-slot draft",
            ["dialogue_act", "tone_tag", "template_slot_id", "state_condition", "localization_key_hint"],
            ["dialogue_line", "final_dialogue_prose", "speaker_text", "localized_text"],
            ["dialogue", "tone", "npc", "faction"],
            ["dialogue_act", "relationship_pressure", "faction_reaction"],
            ["dialogue_string_table_localization_hint_v1"]),
        Family(
            "economy_item_resource_hint_draft",
            "Economy/item/resource hint draft",
            ["resource_id", "economy_pressure", "availability_tag"],
            ["item_definition_json", "final_item_description"],
            ["item", "resource", "economy", "settlement"],
            ["economy_pressure", "settlement_need"],
            ["item_resource_recipe_loot_economy_v1"]),
        Family(
            "combat_ability_progression_hint_draft",
            "Combat/ability/progression hint draft",
            ["ability_slot_id", "combat_pressure", "progression_tag"],
            ["ability_code", "runtime_effect", "combat_formula"],
            ["combat", "progression", "npc", "archetype"],
            ["combat_pressure", "npc_role"],
            ["combat_progression_ability_v1"]),
        Family(
            "settlement_region_event_hint_draft",
            "Settlement/region/event hint draft",
            ["region_slot_id", "settlement_pressure", "event_hint"],
            ["event_script", "map_definition", "gamepackage_region"],
            ["settlement", "region", "event", "world"],
            ["settlement_need", "event_intent", "quest_motive"],
            ["settlement_building_landmark_v1", "biome_weather_hazard_event_hints_v1", "world_topology_region_route_graph_v1"])
    ];

    public static IReadOnlyList<StrictLlmDraftRequestSet> BuildDefaultRequestSets()
    {
        var families = BuildDraftFamilies();
        var resolver = new FeatureDrivenIntentResolver();
        var resolutions = DynamicSemanticFeatureCatalog.BuildDefaultScenarios()
            .Select(resolver.ResolveScenario)
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        return resolutions
            .Select(resolution => BuildRequestSet(resolution, families))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<StrictLlmDraftCandidateEnvelope> BuildProgrammaticFixtureCandidates(
        IReadOnlyList<StrictLlmDraftRequestSet> requestSets)
    {
        var candidates = new List<StrictLlmDraftCandidateEnvelope>();
        foreach (var request in requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal))
        {
            var ordinal = candidates.Count + 1;
            candidates.Add(new StrictLlmDraftCandidateEnvelope
            {
                CandidateId = $"candidate/{request.ScenarioId}/{ordinal:0000}",
                RequestId = request.RequestId,
                ScenarioId = request.ScenarioId,
                ProfileId = request.ProfileId,
                SourceKind = "programmatic_fixture",
                ProvenanceId = $"fixture/{request.RequestId}",
                ProvenanceDetails = "deterministic fixture candidate; external calls absent",
                DraftFamilyId = request.TargetDraftFamily,
                PayloadFields = BuildPayloadFields(request),
                LinkedIntentIds = request.SourceIntentIds,
                LinkedFeatureIds = BuildFeatureLinks(request),
                LinkedContractIds = request.AllowedArtifactContractIds.Take(1).ToList(),
                LinkedSemanticScopes = request.AllowedSemanticScopes.Take(2).ToList(),
                DeclaredConstraints =
                [
                    $"scenario:{request.ScenarioId}",
                    "no_final_prose:true",
                    "no_authority:true"
                ],
                Status = "quarantined"
            });
        }

        return candidates.OrderBy(item => item.CandidateId, StringComparer.Ordinal).ToList();
    }

    private static StrictLlmDraftRequestSet BuildRequestSet(
        SemanticAuthoringIntentResolution resolution,
        IReadOnlyList<StrictLlmDraftFamily> families)
    {
        var requests = new List<StrictLlmDraftRequest>();
        var scenarioId = resolution.ScenarioId;
        var profileId = resolution.ProfileId;
        var intents = resolution.Intents
            .GroupBy(item => item.IntentFamily, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.OrderBy(intent => intent.IntentId, StringComparer.Ordinal).ToList(), StringComparer.Ordinal);

        foreach (var family in families.OrderBy(item => item.OrderingKey, StringComparer.Ordinal))
        {
            var sourceIntentIds = family.AllowedIntentFamilies
                .SelectMany(intentFamily => intents.GetValueOrDefault(intentFamily) ?? [])
                .Select(item => item.IntentId)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .Take(3)
                .ToList();

            if (sourceIntentIds.Count == 0 && family.FamilyId != "species_archetype_feature_draft")
            {
                continue;
            }

            requests.Add(Request(scenarioId, profileId, family, sourceIntentIds, requests.Count + 1));
        }

        var speciesSlotCount = 0;
        if (scenarioId == "metamodule_kingdoms")
        {
            var skeleton = SemanticAuthoringIntentCatalog.BuildMetamoduleKingdomsLoreSkeleton();
            foreach (var slot in skeleton.SpeciesArchetypeSlots.OrderBy(item => item.SlotId, StringComparer.Ordinal))
            {
                var family = families.Single(item => item.FamilyId == "species_archetype_feature_draft");
                var sourceIntentIds = resolution.Intents
                    .Where(item => item.TargetDomain is "world" or "combat" or "settlement")
                    .OrderBy(item => item.IntentId, StringComparer.Ordinal)
                    .Select(item => item.IntentId)
                    .Take(2)
                    .ToList();
                requests.Add(Request(scenarioId, profileId, family, sourceIntentIds, requests.Count + 1) with
                {
                    RequestId = $"draft-request/{scenarioId}/species-archetype-slot/{slot.Ordinal:000}",
                    DeterministicOrderingKey = $"{scenarioId}|species_archetype_slot|{slot.Ordinal:000}|{slot.SlotId}",
                    RequiredFields = ["slot_id", "feature_id", "value_key"],
                    AllowedSemanticScopes = ["species", "archetype", "kingdom"]
                });
                speciesSlotCount++;
            }
        }

        requests = requests
            .GroupBy(item => item.RequestId, StringComparer.Ordinal)
            .Select(item => item.First())
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        return new StrictLlmDraftRequestSet
        {
            ScenarioId = scenarioId,
            ProfileId = profileId,
            Requests = requests,
            SpeciesArchetypeSlotRequestCount = speciesSlotCount,
            StableSummary = $"{scenarioId}|requests={requests.Count}|families={requests.Select(item => item.TargetDraftFamily).Distinct(StringComparer.Ordinal).Count()}|speciesSlots={speciesSlotCount}"
        };
    }

    private static StrictLlmDraftRequest Request(
        string scenarioId,
        string profileId,
        StrictLlmDraftFamily family,
        IReadOnlyList<string> sourceIntentIds,
        int ordinal) =>
        new()
        {
            RequestId = $"draft-request/{scenarioId}/{family.FamilyId}/{ordinal:000}",
            ScenarioId = scenarioId,
            ProfileId = profileId,
            TargetDraftFamily = family.FamilyId,
            SourceIntentIds = sourceIntentIds,
            AllowedArtifactContractIds = family.AllowedArtifactContractIds,
            AllowedSemanticScopes = family.AllowedSemanticScopes,
            RequiredFields = family.RequiredFields,
            ForbiddenFields = family.ForbiddenFields,
            MaximumCandidates = 3,
            ExpectedSourceKinds = ["manual", "llm", "imported", "programmatic_fixture"],
            NoFinalProse = true,
            NoRuntimeAuthority = true,
            RepairPolicyId = "strict_draft_repair_policy_v1",
            DeterministicOrderingKey = $"{scenarioId}|{ordinal:000}|{family.FamilyId}"
        };

    private static StrictLlmDraftFamily Family(
        string id,
        string displayName,
        IReadOnlyList<string> required,
        IReadOnlyList<string> forbidden,
        IReadOnlyList<string> scopes,
        IReadOnlyList<string> intentFamilies,
        IReadOnlyList<string> contractIds) =>
        new()
        {
            FamilyId = id,
            DisplayName = displayName,
            RequiredFields = required.Order(StringComparer.Ordinal).ToList(),
            ForbiddenFields = forbidden.Order(StringComparer.Ordinal).ToList(),
            AllowedSemanticScopes = scopes.Order(StringComparer.Ordinal).ToList(),
            AllowedIntentFamilies = intentFamilies.Order(StringComparer.Ordinal).ToList(),
            AllowedArtifactContractIds = contractIds.Order(StringComparer.Ordinal).ToList(),
            OrderingKey = id
        };

    private static IReadOnlyList<StrictLlmDraftPayloadField> BuildPayloadFields(StrictLlmDraftRequest request) =>
        request.RequiredFields
            .Order(StringComparer.Ordinal)
            .Select(field => new StrictLlmDraftPayloadField
            {
                Name = field,
                ValueKind = field.EndsWith("_id", StringComparison.Ordinal) || field.EndsWith("_key", StringComparison.Ordinal) ? "stable_id" : "tag",
                Value = $"draft/{request.TargetDraftFamily}/{field}/{StableSuffix(request.RequestId)}",
                FinalProse = false
            })
            .ToList();

    private static IReadOnlyList<string> BuildFeatureLinks(StrictLlmDraftRequest request)
    {
        var features = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions()
            .Where(item => request.AllowedSemanticScopes.Contains(NormalizeFeatureScope(item.TargetScope), StringComparer.Ordinal))
            .Select(item => item.FeatureId)
            .Order(StringComparer.Ordinal)
            .Take(3)
            .ToList();
        return features.Count == 0 ? ["feature.trace.unavailable_optional"] : features;
    }

    private static string NormalizeFeatureScope(string scope) =>
        scope switch
        {
            "resource" => "item",
            "biome" => "region",
            _ => scope
        };

    private static string StableSuffix(string value)
    {
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "draft" : parts[^1].Replace('_', '-');
    }
}
