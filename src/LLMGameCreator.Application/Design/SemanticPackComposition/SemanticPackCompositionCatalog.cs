namespace LLMGameCreator.Application.Design.SemanticPackComposition;

public static class SemanticPackCompositionCatalog
{
    public static readonly IReadOnlySet<string> SupportedProfileIds = new HashSet<string>(
        ["frontier_survival", "gothic_intrigue", "caravan_trade"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> ValidFactDomains = new HashSet<string>(
        [
            "world_region",
            "route_pressure",
            "biome_hazard",
            "weather_event",
            "faction_role",
            "reputation_axis",
            "npc_archetype",
            "social_relation",
            "quest_motive",
            "quest_objective",
            "dialogue_tone",
            "localization_hint",
            "economy_chain",
            "resource_theme",
            "recipe_theme",
            "loot_theme",
            "combat_pressure",
            "progression_axis",
            "settlement_pattern",
            "landmark_theme",
            "global_event"
        ],
        StringComparer.Ordinal);

    public static IReadOnlyList<SemanticPackCompositionPack> BuildDefaultPacks() =>
    [
        Pack(
            "semantic_pack/core_blueprint_spine",
            ["frontier_survival", "gothic_intrigue", "caravan_trade"],
            ["profile", "style", "capability", "world", "topology", "route", "biome", "event", "actor", "faction", "social", "quest", "dialogue", "item", "economy", "combat", "progression", "settlement", "presentation", "tone"],
            ["core", "deterministic", "cross_artifact_spine"],
            [
                Fact("core.world.crossroads", "world_region", "shared crossroads region", ["world", "route"]),
                Fact("core.route.contested_passage", "route_pressure", "contested passage pressure", ["route", "conflict"]),
                Fact("core.faction.local_authority", "faction_role", "local authority faction", ["faction"]),
                Fact("core.npc.quest_broker", "npc_archetype", "quest broker archetype", ["npc", "quest"]),
                Fact("core.quest.recover_or_deliver", "quest_objective", "recover or deliver objective", ["quest"]),
                Fact("core.dialogue.grounded", "dialogue_tone", "grounded practical tone", ["dialogue"]),
                Fact("core.localization.key_nouns", "localization_hint", "stable string table noun hints", ["localization"]),
                Fact("core.combat.local_threat", "combat_pressure", "localized threat pressure", ["combat"]),
                Fact("core.progression.reputation_growth", "progression_axis", "reputation growth axis", ["progression"]),
                Fact("core.event.market_disruption", "global_event", "market disruption event", ["event"])
            ],
            [
                Relation("core.link.authority_broker", "core.faction.local_authority", "implies", "core.npc.quest_broker"),
                Relation("core.link.broker_objective", "core.npc.quest_broker", "implies", "core.quest.recover_or_deliver"),
                Relation("core.link.objective_dialogue", "core.quest.recover_or_deliver", "implies", "core.dialogue.grounded"),
                Relation("core.link.route_event", "core.route.contested_passage", "implies", "core.event.market_disruption"),
                Relation("core.link.combat_progression", "core.combat.local_threat", "implies", "core.progression.reputation_growth")
            ],
            [],
            [
                Intent("core.intent.world", "core.world.crossroads", "world_topology_region_route_graph_v1", "world_topology", 10),
                Intent("core.intent.actor", "core.npc.quest_broker", "entity_archetype_npc_actor_profile_v1", "actor_profile", 20),
                Intent("core.intent.quest", "core.quest.recover_or_deliver", "quest_graph_objective_reward_pattern_v1", "quest_pattern", 30),
                Intent("core.intent.dialogue", "core.dialogue.grounded", "dialogue_string_table_localization_hint_v1", "dialogue_localization", 40),
                Intent("core.intent.combat", "core.combat.local_threat", "combat_progression_ability_v1", "combat_progression", 50)
            ],
            0,
            "000-core",
            sourceNotes: "Shared deterministic semantic blueprint spine."),
        Pack(
            "semantic_pack/frontier_survival",
            ["frontier_survival"],
            ["world", "biome", "weather", "faction", "actor", "quest", "economy", "combat", "settlement"],
            ["frontier", "survival", "hazard", "resource_scarcity"],
            [
                Fact("frontier.world.river_ford", "world_region", "river ford frontier region", ["world", "frontier"]),
                Fact("frontier.biome.predator_range", "biome_hazard", "predator range hazard", ["biome", "hazard"]),
                Fact("frontier.weather.cold_snap", "weather_event", "cold snap weather event", ["weather"]),
                Fact("frontier.faction.ranger_post", "faction_role", "ranger post faction", ["faction"]),
                Fact("frontier.npc.trail_medic", "npc_archetype", "trail medic archetype", ["npc"]),
                Fact("frontier.quest.secure_medicine", "quest_motive", "secure scarce medicine", ["quest", "motive"]),
                Fact("frontier.resource.medicine_herbs", "resource_theme", "medicine herbs resource theme", ["resource"]),
                Fact("frontier.loot.survival_cache", "loot_theme", "survival cache loot", ["loot"]),
                Fact("frontier.settlement.stockade_camp", "settlement_pattern", "stockade camp settlement", ["settlement"])
            ],
            [
                Relation("frontier.link.faction_npc", "frontier.faction.ranger_post", "implies", "frontier.npc.trail_medic"),
                Relation("frontier.link.npc_motive", "frontier.npc.trail_medic", "implies", "frontier.quest.secure_medicine"),
                Relation("frontier.link.hazard_resource", "frontier.biome.predator_range", "pressures", "frontier.resource.medicine_herbs"),
                Relation("frontier.link.resource_loot", "frontier.resource.medicine_herbs", "feeds", "frontier.loot.survival_cache"),
                Relation("frontier.link.settlement_route", "frontier.settlement.stockade_camp", "anchors", "frontier.world.river_ford")
            ],
            [],
            [
                Intent("frontier.intent.biome", "frontier.biome.predator_range", "biome_weather_hazard_event_hints_v1", "biome_event_hints", 110),
                Intent("frontier.intent.faction", "frontier.faction.ranger_post", "faction_reputation_social_relation_v1", "faction_relation", 120),
                Intent("frontier.intent.item", "frontier.resource.medicine_herbs", "item_resource_recipe_loot_economy_v1", "item_economy", 130),
                Intent("frontier.intent.settlement", "frontier.settlement.stockade_camp", "settlement_building_landmark_v1", "settlement_landmark", 140, futureRequired: true)
            ],
            100,
            "100-frontier"),
        Pack(
            "semantic_pack/gothic_intrigue",
            ["gothic_intrigue"],
            ["tone", "dialogue", "quest", "actor", "faction", "social", "item", "settlement", "event"],
            ["gothic", "intrigue", "secrets", "reputation"],
            [
                Fact("gothic.world.manor_district", "world_region", "manor district region", ["world", "gothic"]),
                Fact("gothic.faction.old_house", "faction_role", "old house faction", ["faction"]),
                Fact("gothic.reputation.suspicion", "reputation_axis", "suspicion reputation axis", ["reputation"]),
                Fact("gothic.social.blackmail", "social_relation", "blackmail social relation", ["social"]),
                Fact("gothic.npc.disgraced_heir", "npc_archetype", "disgraced heir archetype", ["npc"]),
                Fact("gothic.quest.uncover_secret", "quest_motive", "uncover buried secret", ["quest"]),
                Fact("gothic.dialogue.polite_threat", "dialogue_tone", "polite threat dialogue tone", ["dialogue"]),
                Fact("gothic.landmark.locked_crypt", "landmark_theme", "locked crypt landmark", ["landmark"]),
                Fact("gothic.event.midnight_accusation", "global_event", "midnight accusation event", ["event"])
            ],
            [
                Relation("gothic.link.faction_npc", "gothic.faction.old_house", "implies", "gothic.npc.disgraced_heir"),
                Relation("gothic.link.npc_motive", "gothic.npc.disgraced_heir", "implies", "gothic.quest.uncover_secret"),
                Relation("gothic.link.motive_dialogue", "gothic.quest.uncover_secret", "implies", "gothic.dialogue.polite_threat"),
                Relation("gothic.link.social_reputation", "gothic.social.blackmail", "pressures", "gothic.reputation.suspicion"),
                Relation("gothic.link.landmark_event", "gothic.landmark.locked_crypt", "implies", "gothic.event.midnight_accusation")
            ],
            [],
            [
                Intent("gothic.intent.faction", "gothic.faction.old_house", "faction_reputation_social_relation_v1", "faction_relation", 110),
                Intent("gothic.intent.actor", "gothic.npc.disgraced_heir", "entity_archetype_npc_actor_profile_v1", "actor_profile", 120),
                Intent("gothic.intent.quest", "gothic.quest.uncover_secret", "quest_graph_objective_reward_pattern_v1", "quest_pattern", 130),
                Intent("gothic.intent.dialogue", "gothic.dialogue.polite_threat", "dialogue_string_table_localization_hint_v1", "dialogue_localization", 140)
            ],
            100,
            "100-gothic"),
        Pack(
            "semantic_pack/caravan_trade",
            ["caravan_trade"],
            ["world", "route", "item", "economy", "dialogue", "faction", "quest", "settlement", "event"],
            ["caravan", "trade", "route", "market"],
            [
                Fact("caravan.world.salt_road", "world_region", "salt road trade region", ["world", "route"]),
                Fact("caravan.route.toll_pressure", "route_pressure", "toll pressure route", ["route", "economy"]),
                Fact("caravan.faction.caravan_master", "faction_role", "caravan master faction", ["faction"]),
                Fact("caravan.reputation.credit", "reputation_axis", "credit reputation axis", ["reputation"]),
                Fact("caravan.npc.factor", "npc_archetype", "trade factor archetype", ["npc"]),
                Fact("caravan.quest.delivery_bargain", "quest_motive", "delivery bargain motive", ["quest"]),
                Fact("caravan.dialogue.bargain", "dialogue_tone", "bargaining dialogue tone", ["dialogue"]),
                Fact("caravan.economy.spice_credit", "economy_chain", "spice to credit economy chain", ["economy"]),
                Fact("caravan.settlement.trade_outpost", "settlement_pattern", "trade outpost settlement", ["settlement"])
            ],
            [
                Relation("caravan.link.faction_npc", "caravan.faction.caravan_master", "implies", "caravan.npc.factor"),
                Relation("caravan.link.npc_motive", "caravan.npc.factor", "implies", "caravan.quest.delivery_bargain"),
                Relation("caravan.link.motive_dialogue", "caravan.quest.delivery_bargain", "implies", "caravan.dialogue.bargain"),
                Relation("caravan.link.route_economy", "caravan.route.toll_pressure", "pressures", "caravan.economy.spice_credit"),
                Relation("caravan.link.settlement_route", "caravan.settlement.trade_outpost", "anchors", "caravan.route.toll_pressure")
            ],
            [],
            [
                Intent("caravan.intent.world", "caravan.world.salt_road", "world_topology_region_route_graph_v1", "world_topology", 110),
                Intent("caravan.intent.faction", "caravan.faction.caravan_master", "faction_reputation_social_relation_v1", "faction_relation", 120),
                Intent("caravan.intent.economy", "caravan.economy.spice_credit", "item_resource_recipe_loot_economy_v1", "item_economy", 130),
                Intent("caravan.intent.settlement", "caravan.settlement.trade_outpost", "settlement_building_landmark_v1", "settlement_landmark", 140, futureRequired: true)
            ],
            100,
            "100-caravan"),
        Pack(
            "semantic_pack/ruins_and_relics",
            ["frontier_survival", "gothic_intrigue"],
            ["world", "quest", "item", "settlement", "event"],
            ["ruins", "relics", "landmark"],
            [
                Fact("ruins.landmark.sunken_tower", "landmark_theme", "sunken tower landmark", ["landmark"]),
                Fact("ruins.quest.recover_relic", "quest_objective", "recover relic objective", ["quest"]),
                Fact("ruins.loot.relic_cache", "loot_theme", "relic cache loot", ["loot"]),
                Fact("ruins.event.awakened_ruin", "global_event", "awakened ruin event", ["event"])
            ],
            [
                Relation("ruins.link.landmark_route", "ruins.landmark.sunken_tower", "anchors", "core.route.contested_passage"),
                Relation("ruins.link.landmark_event", "ruins.landmark.sunken_tower", "implies", "ruins.event.awakened_ruin"),
                Relation("ruins.link.objective_loot", "ruins.quest.recover_relic", "rewards", "ruins.loot.relic_cache")
            ],
            [],
            [
                Intent("ruins.intent.quest", "ruins.quest.recover_relic", "quest_graph_objective_reward_pattern_v1", "quest_pattern", 210),
                Intent("ruins.intent.landmark", "ruins.landmark.sunken_tower", "settlement_building_landmark_v1", "settlement_landmark", 220, futureRequired: true)
            ],
            210,
            "210-ruins",
            isOptional: true),
        Pack(
            "semantic_pack/winter_hazards",
            ["frontier_survival"],
            ["biome", "weather", "resource", "combat"],
            ["winter", "hazard", "scarcity"],
            [
                Fact("winter.biome.whiteout", "biome_hazard", "whiteout hazard", ["biome"]),
                Fact("winter.weather.ice_storm", "weather_event", "ice storm event", ["weather"]),
                Fact("winter.resource.firewood", "resource_theme", "firewood scarcity resource", ["resource"]),
                Fact("winter.combat.exposure", "combat_pressure", "exposure pressure", ["combat"])
            ],
            [
                Relation("winter.link.hazard_resource", "winter.biome.whiteout", "pressures", "winter.resource.firewood"),
                Relation("winter.link.weather_combat", "winter.weather.ice_storm", "pressures", "winter.combat.exposure")
            ],
            [],
            [
                Intent("winter.intent.biome", "winter.biome.whiteout", "biome_weather_hazard_event_hints_v1", "biome_event_hints", 210),
                Intent("winter.intent.combat", "winter.combat.exposure", "combat_progression_ability_v1", "combat_progression", 220)
            ],
            220,
            "220-winter",
            isOptional: true),
        Pack(
            "semantic_pack/merchant_guilds",
            ["caravan_trade"],
            ["faction", "economy", "dialogue", "social"],
            ["merchant", "guild", "credit"],
            [
                Fact("guild.faction.merchant_council", "faction_role", "merchant council faction", ["faction"]),
                Fact("guild.social.debt_network", "social_relation", "debt network relation", ["social"]),
                Fact("guild.economy.licensed_trade", "economy_chain", "licensed trade economy chain", ["economy"]),
                Fact("guild.dialogue.contractual", "dialogue_tone", "contractual dialogue tone", ["dialogue"])
            ],
            [
                Relation("guild.link.faction_social", "guild.faction.merchant_council", "implies", "guild.social.debt_network"),
                Relation("guild.link.social_dialogue", "guild.social.debt_network", "implies", "guild.dialogue.contractual"),
                Relation("guild.link.faction_economy", "guild.faction.merchant_council", "feeds", "guild.economy.licensed_trade")
            ],
            [],
            [
                Intent("guild.intent.faction", "guild.faction.merchant_council", "faction_reputation_social_relation_v1", "faction_relation", 210),
                Intent("guild.intent.economy", "guild.economy.licensed_trade", "item_resource_recipe_loot_economy_v1", "item_economy", 220)
            ],
            230,
            "230-guilds",
            isOptional: true),
        Pack(
            "semantic_pack/border_conflict",
            ["gothic_intrigue", "caravan_trade"],
            ["route", "faction", "combat", "progression", "event"],
            ["border", "conflict", "reputation"],
            [
                Fact("border.route.checkpoint", "route_pressure", "checkpoint route pressure", ["route"]),
                Fact("border.faction.militia", "faction_role", "border militia faction", ["faction"]),
                Fact("border.combat.ambush", "combat_pressure", "ambush combat pressure", ["combat"]),
                Fact("border.progression.favor", "progression_axis", "faction favor progression", ["progression"]),
                Fact("border.event.skirmish", "global_event", "border skirmish event", ["event"])
            ],
            [
                Relation("border.link.route_event", "border.route.checkpoint", "implies", "border.event.skirmish"),
                Relation("border.link.combat_progression", "border.combat.ambush", "implies", "border.progression.favor"),
                Relation("border.link.faction_combat", "border.faction.militia", "pressures", "border.combat.ambush")
            ],
            [],
            [
                Intent("border.intent.combat", "border.combat.ambush", "combat_progression_ability_v1", "combat_progression", 210),
                Intent("border.intent.faction", "border.faction.militia", "faction_reputation_social_relation_v1", "faction_relation", 220)
            ],
            240,
            "240-border",
            isOptional: true),
        Pack(
            "semantic_pack/folk_magic",
            ["gothic_intrigue"],
            ["quest", "dialogue", "item", "progression"],
            ["folk_magic", "ritual", "secret"],
            [
                Fact("folk.quest.break_curse", "quest_motive", "break curse motive", ["quest"]),
                Fact("folk.recipe.ritual_charm", "recipe_theme", "ritual charm recipe", ["recipe"]),
                Fact("folk.progression.occult_lore", "progression_axis", "occult lore progression", ["progression"]),
                Fact("folk.dialogue.veiled_warning", "dialogue_tone", "veiled warning tone", ["dialogue"])
            ],
            [
                Relation("folk.link.motive_recipe", "folk.quest.break_curse", "requires", "folk.recipe.ritual_charm"),
                Relation("folk.link.recipe_progression", "folk.recipe.ritual_charm", "unlocks", "folk.progression.occult_lore"),
                Relation("folk.link.motive_dialogue", "folk.quest.break_curse", "implies", "folk.dialogue.veiled_warning")
            ],
            [],
            [
                Intent("folk.intent.quest", "folk.quest.break_curse", "quest_graph_objective_reward_pattern_v1", "quest_pattern", 210),
                Intent("folk.intent.recipe", "folk.recipe.ritual_charm", "item_resource_recipe_loot_economy_v1", "item_economy", 220)
            ],
            250,
            "250-folk",
            isOptional: true),
        Pack(
            "semantic_pack/scarcity_economy",
            ["frontier_survival", "caravan_trade"],
            ["economy", "resource", "recipe", "loot", "quest"],
            ["scarcity", "economy", "crafting"],
            [
                Fact("scarcity.economy.barter_loop", "economy_chain", "barter loop economy chain", ["economy"]),
                Fact("scarcity.resource.salt", "resource_theme", "salt resource theme", ["resource"]),
                Fact("scarcity.recipe.preserves", "recipe_theme", "preserves recipe theme", ["recipe"]),
                Fact("scarcity.loot.ration_bundle", "loot_theme", "ration bundle loot", ["loot"]),
                Fact("scarcity.quest.supply_debt", "quest_motive", "supply debt motive", ["quest"])
            ],
            [
                Relation("scarcity.link.resource_economy", "scarcity.resource.salt", "feeds", "scarcity.economy.barter_loop"),
                Relation("scarcity.link.economy_recipe", "scarcity.economy.barter_loop", "unlocks", "scarcity.recipe.preserves"),
                Relation("scarcity.link.recipe_loot", "scarcity.recipe.preserves", "rewards", "scarcity.loot.ration_bundle")
            ],
            [],
            [
                Intent("scarcity.intent.economy", "scarcity.economy.barter_loop", "item_resource_recipe_loot_economy_v1", "item_economy", 210),
                Intent("scarcity.intent.quest", "scarcity.quest.supply_debt", "quest_graph_objective_reward_pattern_v1", "quest_pattern", 220)
            ],
            260,
            "260-scarcity",
            isOptional: true)
    ];

    public static SemanticPackCompositionRequest FrontierRequest() =>
        Request("frontier_survival", [
            "semantic_pack/core_blueprint_spine",
            "semantic_pack/frontier_survival",
            "semantic_pack/ruins_and_relics",
            "semantic_pack/winter_hazards",
            "semantic_pack/scarcity_economy"
        ]);

    public static SemanticPackCompositionRequest GothicRequest() =>
        Request("gothic_intrigue", [
            "semantic_pack/core_blueprint_spine",
            "semantic_pack/gothic_intrigue",
            "semantic_pack/ruins_and_relics",
            "semantic_pack/border_conflict",
            "semantic_pack/folk_magic"
        ]);

    public static SemanticPackCompositionRequest CaravanRequest() =>
        Request("caravan_trade", [
            "semantic_pack/core_blueprint_spine",
            "semantic_pack/caravan_trade",
            "semantic_pack/merchant_guilds",
            "semantic_pack/border_conflict",
            "semantic_pack/scarcity_economy"
        ]);

    private static SemanticPackCompositionRequest Request(string profileId, IReadOnlyList<string> selectedPackIds) =>
        new()
        {
            ProfileId = profileId,
            SelectedPackIds = selectedPackIds,
            ComplexityHint = "standard"
        };

    private static SemanticPackCompositionPack Pack(
        string packId,
        IReadOnlyList<string> supportedProfiles,
        IReadOnlyList<string> scopes,
        IReadOnlyList<string> tags,
        IReadOnlyList<SemanticPackFact> facts,
        IReadOnlyList<SemanticPackRelationHint> relations,
        IReadOnlyList<string> exclusions,
        IReadOnlyList<SemanticPackExpansionIntent> intents,
        int priority,
        string orderingKey,
        bool isOptional = false,
        bool isFutureOnly = false,
        string sourceStatus = "ready",
        string sourceNotes = "") =>
        new()
        {
            PackId = packId,
            SupportedProfileIds = supportedProfiles,
            ProvidedSemanticScopes = scopes,
            ThemeTags = tags,
            Facts = facts,
            RelationHints = relations,
            Exclusions = exclusions,
            ExpansionIntents = intents,
            Priority = priority,
            OrderingKey = orderingKey,
            IsOptional = isOptional,
            IsFutureOnly = isFutureOnly,
            SourceStatus = sourceStatus,
            SourceNotes = sourceNotes
        };

    private static SemanticPackFact Fact(string id, string domain, string value, IReadOnlyList<string> tags) =>
        new()
        {
            FactId = id,
            Domain = domain,
            Value = value,
            Tags = tags
        };

    private static SemanticPackRelationHint Relation(string id, string source, string kind, string target) =>
        new()
        {
            RelationId = id,
            SourceFactId = source,
            RelationKind = kind,
            TargetFactId = target
        };

    private static SemanticPackExpansionIntent Intent(
        string id,
        string sourceFact,
        string contractId,
        string artifactKind,
        int priority,
        bool futureRequired = false) =>
        new()
        {
            IntentId = id,
            SourceFactId = sourceFact,
            TargetContractId = contractId,
            TargetArtifactKind = artifactKind,
            Priority = priority,
            FutureRequired = futureRequired
        };
}
