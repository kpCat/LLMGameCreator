using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

namespace LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

public static class LuaModuleManifestRegistryCatalog
{
    public static IReadOnlyList<LuaModuleFamilyDefinition> BuildFamilies() =>
    [
        Family("world_generation_hints", "World generation hints", ["world"], ["world_topology_region_route_graph_v1"], ["lore_gap", "event_intent"]),
        Family("region_biome_weather_hazard_rules", "Region/biome/weather/hazard rules", ["world", "region", "biome", "event"], ["biome_weather_hazard_event_hints_v1"], ["event_intent", "quest_motive"]),
        Family("npc_species_archetype_rules", "NPC/species/archetype rules", ["npc", "species", "archetype"], ["entity_archetype_npc_actor_profile_v1"], ["npc_role", "combat_pressure"]),
        Family("faction_reputation_social_relation_rules", "Faction/reputation/social relation rules", ["faction", "relationship"], ["faction_reputation_social_relation_v1"], ["faction_reaction", "relationship_pressure"]),
        Family("quest_objective_reward_rules", "Quest/objective/reward rules", ["quest"], ["quest_graph_objective_reward_pattern_v1"], ["quest_motive", "event_intent"]),
        Family("dialogue_act_tone_localization_hint_rules", "Dialogue act/tone/localization hint rules", ["dialogue", "npc", "faction"], ["dialogue_string_table_localization_hint_v1"], ["dialogue_act", "relationship_pressure"]),
        Family("item_resource_recipe_loot_economy_rules", "Item/resource/recipe/loot/economy rules", ["item"], ["item_resource_recipe_loot_economy_v1"], ["economy_pressure", "settlement_need"]),
        Family("combat_stat_ability_status_rules", "Combat/stat/ability/status rules", ["combat"], ["combat_progression_ability_v1"], ["combat_pressure", "npc_role"]),
        Family("settlement_building_landmark_rules", "Settlement/building/landmark rules", ["settlement", "world"], ["settlement_building_landmark_v1"], ["settlement_need", "event_intent"]),
        Family("event_global_pressure_rules", "Event/global pressure rules", ["event", "world", "faction"], ["biome_weather_hazard_event_hints_v1", "quest_graph_objective_reward_pattern_v1"], ["event_intent", "quest_motive"]),
        Family("metamodule_species_archetype_expansion_rules", "Metamodule species/archetype expansion rules", ["kingdom", "species", "archetype"], ["entity_archetype_npc_actor_profile_v1", "semantic_pack_v1"], ["lore_gap", "combat_pressure", "settlement_need"])
    ];

    public static LuaHostApiSurfacePolicy BuildHostApiSurfacePolicy()
    {
        var groups = new List<LuaHostApiGroup>
        {
            Host("semantic.read", "Semantic read", "ready", ["read_semantic_pack", "read_semantic_scope"], [], "read_only", ["semantic_pack_v1"], ["world", "quest"], "lua_host.semantic"),
            Host("feature.read", "Feature read", "ready", ["read_feature_definition", "read_resolved_feature"], [], "read_only", ["semantic_pack_v1"], ["world", "npc"], "lua_host.feature"),
            Host("intent.read", "Intent read", "ready", ["read_intent_record", "read_intent_trace"], [], "read_only", ["semantic_pack_v1"], ["quest", "dialogue"], "lua_host.intent"),
            Host("quest.plan", "Quest planning", "ready", ["plan_quest_pattern", "plan_reward_hint"], ["materialize_gamepackage_quest"], "planning_only", ["quest_graph_objective_reward_pattern_v1"], ["quest"], "lua_host.quest"),
            Host("dialogue.intent", "Dialogue intent planning", "ready", ["plan_dialogue_act", "plan_localization_hint"], ["write_dialogue_line"], "planning_only", ["dialogue_string_table_localization_hint_v1"], ["dialogue"], "lua_host.dialogue"),
            Host("economy.plan", "Economy planning", "ready", ["plan_resource_hint", "plan_loot_hint"], ["mutate_inventory_state"], "planning_only", ["item_resource_recipe_loot_economy_v1"], ["item"], "lua_host.economy"),
            Host("combat.plan", "Combat planning", "ready", ["plan_ability_hint", "plan_status_hint"], ["execute_runtime_effect"], "planning_only", ["combat_progression_ability_v1"], ["combat"], "lua_host.combat"),
            Host("world.plan", "World planning", "ready", ["plan_region_hint", "plan_route_hint"], ["write_map_file"], "planning_only", ["world_topology_region_route_graph_v1"], ["world"], "lua_host.world"),
            Host("event.plan", "Event planning", "ready", ["plan_event_pressure", "plan_hazard_hint"], ["dispatch_runtime_event"], "planning_only", ["biome_weather_hazard_event_hints_v1"], ["event"], "lua_host.event"),
            Host("metamodule.expand", "Metamodule expansion planning", "ready", ["expand_species_slot_manifest", "expand_archetype_slot_manifest"], ["generate_lua_source"], "metadata_only", ["entity_archetype_npc_actor_profile_v1", "semantic_pack_v1"], ["kingdom", "species", "archetype"], "lua_host.metamodule"),
            Host("filesystem", "Filesystem", "blocked", [], ["read_file", "write_file", "enumerate_directory"], "blocked", [], [], "lua_host.denied.filesystem"),
            Host("network", "Network", "blocked", [], ["http_request", "socket_connect"], "blocked", [], [], "lua_host.denied.network"),
            Host("os_process", "OS process", "blocked", [], ["start_process", "shell_execute"], "blocked", [], [], "lua_host.denied.os_process"),
            Host("reflection", "Reflection", "blocked", [], ["reflect_type", "invoke_member"], "blocked", [], [], "lua_host.denied.reflection"),
            Host("provider_llm_rag", "Provider/LLM/RAG", "blocked", [], ["call_provider", "call_llm", "query_rag"], "blocked", [], [], "lua_host.denied.provider"),
            Host("ui_winforms", "WinForms UI", "blocked", [], ["show_form", "mutate_control"], "blocked", [], [], "lua_host.denied.ui"),
            Host("runtime_direct_mutation", "Runtime direct mutation", "blocked", [], ["mutate_runtime_state", "execute_command"], "blocked", [], [], "lua_host.denied.runtime"),
            Host("unity_direct_call", "Unity direct call", "blocked", [], ["call_unity_api", "build_player"], "blocked", [], [], "lua_host.denied.unity"),
            Host("gamepackage_schema_mutation", "GamePackage schema mutation", "blocked", [], ["change_schema", "write_package_definition"], "blocked", [], [], "lua_host.denied.gamepackage"),
            Host("arbitrary_code_generation", "Arbitrary code generation", "blocked", [], ["generate_csharp", "generate_lua_source"], "blocked", [], [], "lua_host.denied.codegen"),
            Host("implicit_lua_execution", "Implicit Lua execution", "blocked", [], ["execute_lua", "eval_lua"], "blocked", [], [], "lua_host.denied.execution")
        };

        var diagnostics = LuaModuleManifestRegistryValidator.ValidateHostApiSurface(groups);
        return new LuaHostApiSurfacePolicy
        {
            GroupCount = groups.Count,
            AllowedGroupIds = groups.Where(item => item.Status is "ready" or "optional").Select(item => item.GroupId).Order(StringComparer.Ordinal).ToList(),
            DeniedGroupIds = groups.Where(item => item.Status is "blocked" or "future_required").Select(item => item.GroupId).Order(StringComparer.Ordinal).ToList(),
            Groups = groups.OrderBy(item => item.GroupId, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };
    }

    public static IReadOnlyList<LuaModuleManifest> BuildDefaultManifests()
    {
        var manifests = new List<LuaModuleManifest>
        {
            Module("lua-module/frontier/world-generation-hints", "world_generation_hints", "Frontier world generation hints", "Grounded frontier region and route planning hints.", "ready", "frontier_survival", ["world"], ["world.plan", "semantic.read", "feature.read"], [], ["world_topology_region_route_graph_v1"], ["lore_gap"], []),
            Module("lua-module/frontier/biome-weather-hazard-rules", "region_biome_weather_hazard_rules", "Frontier biome hazard rules", "Cold, scarcity and wilderness hazard planning hints.", "ready", "frontier_survival", ["world", "region", "biome", "event"], ["world.plan", "event.plan", "semantic.read"], ["lua-module/frontier/world-generation-hints"], ["biome_weather_hazard_event_hints_v1"], ["event_intent"], []),
            Module("lua-module/frontier/npc-species-archetype-rules", "npc_species_archetype_rules", "Frontier actor archetype rules", "Survivor, trader and healer actor planning hints.", "ready", "frontier_survival", ["npc", "species", "archetype"], ["intent.read", "feature.read"], ["lua-module/frontier/world-generation-hints"], ["entity_archetype_npc_actor_profile_v1"], ["npc_role"], []),
            Module("lua-module/frontier/quest-objective-reward-rules", "quest_objective_reward_rules", "Frontier quest objective rules", "Survival objective and reward pattern planning hints.", "ready", "frontier_survival", ["quest", "event", "item"], ["quest.plan", "intent.read"], ["lua-module/frontier/biome-weather-hazard-rules", "lua-module/frontier/item-resource-economy-rules"], ["quest_graph_objective_reward_pattern_v1"], ["quest_motive"], []),
            Module("lua-module/frontier/item-resource-economy-rules", "item_resource_recipe_loot_economy_rules", "Frontier resource economy rules", "Scarce resource, recipe and loot planning hints.", "ready", "frontier_survival", ["item", "resource"], ["economy.plan", "semantic.read"], ["lua-module/frontier/world-generation-hints"], ["item_resource_recipe_loot_economy_v1"], ["economy_pressure"], []),
            Module("lua-module/frontier/combat-stat-ability-status-rules", "combat_stat_ability_status_rules", "Frontier combat pressure rules", "Wildlife and exposure combat/status planning hints.", "optional", "frontier_survival", ["combat", "npc"], ["combat.plan", "intent.read"], ["lua-module/frontier/npc-species-archetype-rules"], ["combat_progression_ability_v1"], ["combat_pressure"], []),

            Module("lua-module/gothic/world-generation-hints", "world_generation_hints", "Gothic estate world hints", "Manor, village and forbidden-route planning hints.", "ready", "gothic_intrigue", ["world"], ["world.plan", "semantic.read"], [], ["world_topology_region_route_graph_v1"], ["lore_gap"], []),
            Module("lua-module/gothic/faction-social-rules", "faction_reputation_social_relation_rules", "Gothic faction relation rules", "House rivalry and reputation planning hints.", "ready", "gothic_intrigue", ["faction", "relationship"], ["intent.read", "semantic.read"], ["lua-module/gothic/world-generation-hints"], ["faction_reputation_social_relation_v1"], ["faction_reaction", "relationship_pressure"], []),
            Module("lua-module/gothic/dialogue-tone-localization-rules", "dialogue_act_tone_localization_hint_rules", "Gothic dialogue tone rules", "Suspicion, etiquette and clue-intent localization template hints without authored lines.", "ready", "gothic_intrigue", ["dialogue", "npc", "faction"], ["dialogue.intent", "intent.read"], ["lua-module/gothic/faction-social-rules"], ["dialogue_string_table_localization_hint_v1"], ["dialogue_act", "relationship_pressure"], []),
            Module("lua-module/gothic/quest-objective-reward-rules", "quest_objective_reward_rules", "Gothic quest motive rules", "Secret, clue and favor objective planning hints.", "ready", "gothic_intrigue", ["quest", "dialogue"], ["quest.plan", "dialogue.intent"], ["lua-module/gothic/dialogue-tone-localization-rules"], ["quest_graph_objective_reward_pattern_v1"], ["quest_motive"], []),
            Module("lua-module/gothic/event-global-pressure-rules", "event_global_pressure_rules", "Gothic global pressure rules", "Curfew, rumor and omen event planning hints.", "optional", "gothic_intrigue", ["event", "world", "faction"], ["event.plan", "intent.read"], ["lua-module/gothic/faction-social-rules"], ["biome_weather_hazard_event_hints_v1"], ["event_intent"], []),

            Module("lua-module/caravan/world-generation-hints", "world_generation_hints", "Caravan route world hints", "Route, stopover and travel-risk planning hints.", "ready", "caravan_trade", ["world"], ["world.plan", "semantic.read"], [], ["world_topology_region_route_graph_v1"], ["lore_gap"], []),
            Module("lua-module/caravan/item-resource-economy-rules", "item_resource_recipe_loot_economy_rules", "Caravan economy rules", "Goods, recipes, demand and loot planning hints.", "ready", "caravan_trade", ["item", "resource"], ["economy.plan", "semantic.read"], ["lua-module/caravan/world-generation-hints"], ["item_resource_recipe_loot_economy_v1"], ["economy_pressure"], []),
            Module("lua-module/caravan/faction-social-rules", "faction_reputation_social_relation_rules", "Caravan faction relation rules", "Guild, road guard and settlement relation planning hints.", "ready", "caravan_trade", ["faction", "relationship"], ["intent.read", "semantic.read"], ["lua-module/caravan/world-generation-hints"], ["faction_reputation_social_relation_v1"], ["faction_reaction"], []),
            Module("lua-module/caravan/settlement-landmark-rules", "settlement_building_landmark_rules", "Caravan settlement landmark rules", "Market, camp and pass landmark planning hints.", "future_required", "caravan_trade", ["settlement", "world"], ["world.plan"], ["lua-module/caravan/world-generation-hints"], ["settlement_building_landmark_v1"], ["settlement_need"], [], selectableAsReady: false),
            Module("lua-module/caravan/quest-objective-reward-rules", "quest_objective_reward_rules", "Caravan delivery quest rules", "Contract, delivery and trade reward pattern hints.", "ready", "caravan_trade", ["quest", "item"], ["quest.plan", "economy.plan"], ["lua-module/caravan/item-resource-economy-rules"], ["quest_graph_objective_reward_pattern_v1"], ["quest_motive"], []),

            Module("lua-module/metamodule/world-generation-hints", "world_generation_hints", "Metamodule kingdom world hints", "Kingdom-scale world pressure planning hints.", "ready", "metamodule_kingdoms", ["world", "kingdom"], ["world.plan", "semantic.read"], [], ["world_topology_region_route_graph_v1", "semantic_pack_v1"], ["lore_gap"], []),
            Module(
                "lua-module/metamodule/species-archetype-expansion-base",
                "metamodule_species_archetype_expansion_rules",
                "Metamodule species/archetype expansion base",
                "Manifest-only base for species/archetype slot expansion planning.",
                "ready",
                "metamodule_kingdoms",
                ["kingdom", "species", "archetype"],
                ["metamodule.expand", "semantic.read", "feature.read", "intent.read"],
                ["lua-module/metamodule/world-generation-hints"],
                ["entity_archetype_npc_actor_profile_v1", "semantic_pack_v1"],
                ["lore_gap", "combat_pressure", "settlement_need"],
                []),
            Module("lua-module/metamodule/faction-social-rules", "faction_reputation_social_relation_rules", "Metamodule faction relation rules", "Kingdom pressure and faction relation planning hints.", "ready", "metamodule_kingdoms", ["faction", "relationship", "kingdom"], ["intent.read", "semantic.read"], ["lua-module/metamodule/world-generation-hints"], ["faction_reputation_social_relation_v1"], ["faction_reaction", "relationship_pressure"], []),
            Module("lua-module/metamodule/combat-stat-ability-status-rules", "combat_stat_ability_status_rules", "Metamodule combat pressure rules", "Mana resonance and forbidden-affinity combat pressure hints.", "optional", "metamodule_kingdoms", ["combat", "archetype", "species"], ["combat.plan", "feature.read"], ["lua-module/metamodule/species-archetype-expansion-base"], ["combat_progression_ability_v1"], ["combat_pressure"], []),
            Module(
                "lua-module/draft/goal034-quarantined-lua-manifest-candidate",
                "metamodule_species_archetype_expansion_rules",
                "Goal 034 quarantined Lua manifest candidate",
                "Compatibility record for future_lua_module_manifest_request; repair diagnostics required before review.",
                "quarantined",
                "metamodule_kingdoms",
                ["kingdom", "species", "archetype"],
                ["semantic.read"],
                [],
                ["entity_archetype_npc_actor_profile_v1", "semantic_pack_v1"],
                ["lore_gap"],
                [],
                "goal_034_quarantined_candidate",
                "quarantine/goal034/future_lua_module_manifest_request",
                "quarantined",
                selectableAsReady: false)
        };

        manifests.AddRange(BuildMetamoduleSlotManifests());
        return manifests.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<LuaModuleSelectionContext> BuildDefaultSelectionContexts() =>
    [
        Context("frontier_survival", ["world", "region", "biome", "event", "npc", "item", "resource", "quest", "combat"], ["world_generation_hints", "region_biome_weather_hazard_rules", "npc_species_archetype_rules", "item_resource_recipe_loot_economy_rules", "quest_objective_reward_rules", "combat_stat_ability_status_rules"]),
        Context("gothic_intrigue", ["world", "faction", "relationship", "dialogue", "quest", "event"], ["world_generation_hints", "faction_reputation_social_relation_rules", "dialogue_act_tone_localization_hint_rules", "quest_objective_reward_rules", "event_global_pressure_rules"]),
        Context("caravan_trade", ["world", "item", "resource", "faction", "relationship", "settlement", "quest"], ["world_generation_hints", "item_resource_recipe_loot_economy_rules", "faction_reputation_social_relation_rules", "settlement_building_landmark_rules", "quest_objective_reward_rules"]),
        Context("metamodule_kingdoms", ["world", "kingdom", "species", "archetype", "faction", "relationship", "combat"], ["world_generation_hints", "metamodule_species_archetype_expansion_rules", "faction_reputation_social_relation_rules", "combat_stat_ability_status_rules"])
    ];

    private static IReadOnlyList<LuaModuleManifest> BuildMetamoduleSlotManifests()
    {
        var skeleton = SemanticAuthoringIntentCatalog.BuildMetamoduleKingdomsLoreSkeleton();
        return skeleton.SpeciesArchetypeSlots
            .OrderBy(item => item.Ordinal)
            .Select(slot => Module(
                $"lua-module/metamodule/species-archetype-slot/{slot.Ordinal:000}",
                "metamodule_species_archetype_expansion_rules",
                $"Metamodule species/archetype slot {slot.Ordinal:000}",
                $"Manifest declaration for {slot.KingdomId} {slot.SpeciesFamily}/{slot.ArchetypeFamily} slot.",
                "ready",
                "metamodule_kingdoms",
                ["kingdom", "species", "archetype"],
                ["metamodule.expand", "semantic.read", "feature.read", "intent.read"],
                ["lua-module/metamodule/species-archetype-expansion-base"],
                ["entity_archetype_npc_actor_profile_v1", "semantic_pack_v1"],
                ["lore_gap", "combat_pressure", "settlement_need"],
                [$"slot:{slot.SlotId}"],
                deterministicSuffix: $"slot|{slot.Ordinal:000}|{slot.SlotId}"))
            .ToList();
    }

    private static LuaModuleSelectionContext Context(
        string scenarioId,
        IReadOnlyList<string> scopes,
        IReadOnlyList<string> families)
    {
        var allContracts = BuildFamilies()
            .Where(family => families.Contains(family.FamilyId, StringComparer.Ordinal))
            .SelectMany(item => item.ArtifactContractIds)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        var allIntents = BuildFamilies()
            .Where(family => families.Contains(family.FamilyId, StringComparer.Ordinal))
            .SelectMany(item => item.IntentFamilies)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new LuaModuleSelectionContext
        {
            ScenarioId = scenarioId,
            ProfileId = scenarioId,
            RequiredSemanticScopes = scopes.Order(StringComparer.Ordinal).ToList(),
            RequestedFamilyIds = families.Order(StringComparer.Ordinal).ToList(),
            AvailableArtifactContractIds = allContracts,
            AvailableIntentFamilies = allIntents
        };
    }

    private static LuaModuleFamilyDefinition Family(
        string id,
        string displayName,
        IReadOnlyList<string> scopes,
        IReadOnlyList<string> contracts,
        IReadOnlyList<string> intents) =>
        new()
        {
            FamilyId = id,
            DisplayName = displayName,
            RequiredSemanticScopes = scopes.Order(StringComparer.Ordinal).ToList(),
            ArtifactContractIds = contracts.Order(StringComparer.Ordinal).ToList(),
            IntentFamilies = intents.Order(StringComparer.Ordinal).ToList(),
            OrderingKey = id
        };

    private static LuaHostApiGroup Host(
        string id,
        string displayName,
        string status,
        IReadOnlyList<string> allowed,
        IReadOnlyList<string> denied,
        string sideEffectClass,
        IReadOnlyList<string> contracts,
        IReadOnlyList<string> scopes,
        string diagnosticPrefix) =>
        new()
        {
            GroupId = id,
            DisplayName = displayName,
            Status = status,
            AllowedOperationKinds = allowed.Order(StringComparer.Ordinal).ToList(),
            DeniedOperationKinds = denied.Order(StringComparer.Ordinal).ToList(),
            SideEffectClass = sideEffectClass,
            RequiredArtifactContractIds = contracts.Order(StringComparer.Ordinal).ToList(),
            RequiredSemanticScopes = scopes.Order(StringComparer.Ordinal).ToList(),
            DiagnosticCodePrefix = diagnosticPrefix
        };

    private static LuaModuleManifest Module(
        string moduleId,
        string familyId,
        string displayName,
        string summary,
        string status,
        string scenarioId,
        IReadOnlyList<string> semanticScopes,
        IReadOnlyList<string> allowedHostApiGroups,
        IReadOnlyList<string> dependencies,
        IReadOnlyList<string> artifactContractIds,
        IReadOnlyList<string> intentFamilies,
        IReadOnlyList<string> deniedOperationKinds,
        string sourceKind = "programmatic_registry",
        string provenanceId = "goal_035_seed_registry",
        string promotionStatus = "reviewed",
        bool selectableAsReady = true,
        string deterministicSuffix = "") =>
        new()
        {
            ModuleId = moduleId,
            FamilyId = familyId,
            Version = "1.0.0",
            DisplayName = displayName,
            Summary = summary,
            LifecycleStatus = status,
            TargetDialect = status is "future_required" ? "lua_5_4_future" : "manifest_only",
            SourceKind = sourceKind,
            ProvenanceId = provenanceId,
            ProvenanceDetails = "deterministic manifest-only registry seed; no Lua source or execution",
            ProfileCompatibility = [scenarioId],
            ScenarioCompatibility = [scenarioId],
            SemanticScopes = semanticScopes.Order(StringComparer.Ordinal).ToList(),
            ArtifactContractIds = artifactContractIds.Order(StringComparer.Ordinal).ToList(),
            IntentFamilies = intentFamilies.Order(StringComparer.Ordinal).ToList(),
            Dependencies = dependencies.Order(StringComparer.Ordinal).ToList(),
            AllowedHostApiGroups = allowedHostApiGroups.Order(StringComparer.Ordinal).ToList(),
            DeniedHostApiGroups = LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy().DeniedGroupIds,
            DeniedOperationKinds = deniedOperationKinds
                .Concat(["execute_lua", "eval_lua", "call_provider", "call_llm", "query_rag", "mutate_runtime_state", "call_unity_api", "show_form", "change_schema", "generate_lua_source"])
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList(),
            SideEffectClass = allowedHostApiGroups.Contains("metamodule.expand", StringComparer.Ordinal) ? "metadata_only" : "planning_only",
            ResourceBudget = new LuaModuleResourceBudget
            {
                MaxInputRecords = 256,
                MaxOutputRecords = allowedHostApiGroups.Contains("metamodule.expand", StringComparer.Ordinal) ? 64 : 32,
                MaxDependencyDepth = 4,
                MaxEstimatedMilliseconds = 250,
                MaxMemoryKb = 1024
            },
            PromotionStatus = promotionStatus,
            SelectableAsReady = selectableAsReady && (status is "ready" or "optional"),
            ContainsFinalProse = false,
            DeclaresLuaSource = false,
            ClaimsLuaExecution = false,
            DeclaresProviderLlmRagAccess = false,
            DeclaresRuntimeUiUnityOrGamePackageMutation = false,
            DeterministicOrderingKey = string.IsNullOrWhiteSpace(deterministicSuffix)
                ? $"{scenarioId}|{familyId}|{moduleId}"
                : $"{scenarioId}|{familyId}|{deterministicSuffix}"
        };
}
