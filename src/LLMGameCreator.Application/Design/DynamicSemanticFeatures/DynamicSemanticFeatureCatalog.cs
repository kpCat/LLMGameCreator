namespace LLMGameCreator.Application.Design.DynamicSemanticFeatures;

public static class DynamicSemanticFeatureCatalog
{
    public static IReadOnlyList<DynamicSemanticFeatureDefinition> BuildDefaultFeatureDefinitions() =>
    [
        Feature("world.theme", "World Theme", "world", "enum", "required", "first_allowed", "inherit", ["world"], ["frontier", "gothic", "caravan", "metamodule"], group: "world"),
        Feature("kingdom.pressure", "Kingdom Pressure", "kingdom", "number", "optional", "constant", "inherit", ["world", "kingdom"], min: 0, max: 10, defaultValue: Number(0), group: "politics"),
        Feature("region.route_pressure", "Route Pressure", "region", "enum", "optional", "constant", "inherit", ["world", "kingdom", "region"], ["none", "predators", "court_watch", "toll_risk", "siege"], Enum("none"), "world"),
        Feature("biome.hazard", "Biome Hazard", "biome", "enum", "optional", "none", "inherit", ["world", "kingdom", "region", "biome"], ["cold", "beasts", "fog", "desert", "mana_storm"], group: "world"),
        Feature("settlement.market_need", "Settlement Market Need", "settlement", "weighted_tag", "optional", "none", "inherit", ["world", "kingdom", "region", "settlement"], group: "economy"),
        Feature("faction.reputation_axis", "Faction Reputation Axis", "faction", "enum", "required", "first_allowed", "inherit", ["world", "kingdom", "faction"], ["trust", "suspicion", "credit", "oath"], group: "social"),
        Feature("species.mana_resonance", "Species Mana Resonance", "species", "number", "optional", "constant", "inherit", ["world", "species"], min: 0, max: 10, defaultValue: Number(0), tags: ["metamodule"], group: "species"),
        Feature("species.module_capacity", "Species Module Capacity", "species", "number", "optional", "constant", "inherit", ["world", "species"], min: 0, max: 12, defaultValue: Number(0), tags: ["metamodule"], group: "species"),
        Feature("archetype.forbidden_affinity", "Archetype Forbidden Affinity", "archetype", "enum", "optional", "none", "inherit", ["species", "archetype"], ["none", "void", "iron", "sun"], tags: ["metamodule"], group: "archetype"),
        Feature("npc.faction_relation", "NPC Faction Relation", "npc", "relation", "optional", "none", "inherit", ["world", "kingdom", "faction", "npc"], conditions: [Condition("target_has_tag", tag: "has_faction")], group: "npc"),
        Feature("npc.trust", "NPC Trust", "npc", "number", "optional", "constant", "inherit", ["world", "kingdom", "faction", "species", "archetype", "npc"], min: 0, max: 10, defaultValue: Number(5), group: "npc"),
        Feature("npc.mood", "NPC Mood", "npc", "enum", "optional", "none", "inherit", ["world", "kingdom", "faction", "species", "archetype", "npc"], ["calm", "hungry", "suspicious", "hopeful", "pressured"], conditions: [Condition("feature_exists", featureId: "npc.trust")], group: "npc"),
        Feature("npc.hunger", "NPC Hunger", "npc", "number", "optional", "constant", "none", [], min: 0, max: 10, defaultValue: Number(0), group: "npc"),
        Feature("quest.motive", "Quest Motive", "quest", "enum", "required", "first_allowed", "inherit", ["world", "kingdom", "region", "faction", "quest"], ["survive", "secret", "contract", "kingdom_pressure"], group: "quest"),
        Feature("dialogue.intent", "Dialogue Intent", "dialogue", "list", "optional", "none", "inherit", ["world", "kingdom", "faction", "npc", "quest", "dialogue"], group: "dialogue"),
        Feature("dialogue.text_key_hint", "Dialogue Text Key Hint", "dialogue", "text_key", "optional", "none", "inherit", ["world", "kingdom", "faction", "npc", "quest", "dialogue"], group: "dialogue"),
        Feature("event.is_volatile", "Event Is Volatile", "event", "flag", "optional", "constant", "inherit", ["world", "kingdom", "region", "event"], defaultValue: Flag(false), group: "event"),
        Feature("event.intent", "Event Intent", "event", "list", "optional", "none", "inherit", ["world", "kingdom", "region", "event"], group: "event"),
        Feature("magic.mana_pressure", "Magic Mana Pressure", "magic", "number", "optional", "constant", "inherit", ["world", "kingdom", "species", "archetype", "magic"], min: 0, max: 10, defaultValue: Number(0), group: "magic"),
        Feature("combat.threat_level", "Combat Threat Level", "combat", "number", "optional", "constant", "inherit", ["world", "kingdom", "region", "biome", "combat"], min: 0, max: 10, defaultValue: Number(0), group: "combat"),
        Feature("relationship.tension", "Relationship Tension", "relationship", "relation", "optional", "none", "inherit", ["world", "kingdom", "faction", "npc", "relationship"], group: "relationship"),
        Feature("item.resource_role", "Item Resource Role", "item", "enum", "optional", "none", "inherit", ["world", "region", "item"], ["medicine", "relic", "spice", "module_core"], group: "item"),
        Feature("resource.scarcity", "Resource Scarcity", "resource", "number", "optional", "constant", "inherit", ["world", "region", "resource"], min: 0, max: 10, defaultValue: Number(0), group: "resource")
    ];

    public static IReadOnlyList<DynamicSemanticInfluenceRule> BuildDefaultInfluenceRules() =>
    [
        Rule(
            "rule/frontier_hunger_mood",
            "npc",
            "frontier",
            [Condition("number_at_least", "npc.hunger", number: 7)],
            [Effect("set_feature", "npc.mood", Enum("hungry")), Effect("add_intent", intent: "intent/find_food")],
            10,
            "Hunger can suggest mood and survival intent through deterministic feature data."),
        Rule(
            "rule/gothic_suspicion_dialogue",
            "dialogue",
            "gothic",
            [Condition("enum_equals", "faction.reputation_axis", "suspicion")],
            [Effect("add_intent", "dialogue.intent", List("polite_threat", "hidden_accusation"))],
            20,
            "Suspicious gothic factions push dialogue toward threat/accusation intents."),
        Rule(
            "rule/caravan_market_need",
            "settlement",
            "caravan",
            [Condition("scope_is", scope: "settlement")],
            [Effect("add_weighted_tag", "settlement.market_need", Weighted(("contract_risk", 0.35)))],
            30,
            "Trade settlement needs can accumulate contract risk pressure deterministically."),
        Rule(
            "rule/metamodule_mana_capacity",
            "species",
            "metamodule",
            [Condition("number_at_least", "species.mana_resonance", number: 6), Condition("target_has_tag", tag: "metamodule_bearer")],
            [Effect("adjust_number", "species.module_capacity", numberDelta: 2), Effect("suggest_feature", "archetype.forbidden_affinity", suggestion: "Review forbidden affinity when resonance is high.")],
            40,
            "High mana resonance increases module capacity for metamodule bearer species/archetypes."),
        Rule(
            "rule/kingdom_pressure_event",
            "event",
            "metamodule",
            [Condition("number_at_least", "kingdom.pressure", number: 7)],
            [Effect("add_intent", "event.intent", List("tax_edict", "forbidden_affinity_purge"))],
            50,
            "High kingdom pressure can add deterministic event intents.")
    ];

    public static IReadOnlyList<DynamicSemanticScenario> BuildDefaultScenarios() =>
    [
        FrontierScenario(),
        GothicScenario(),
        CaravanScenario(),
        MetamoduleKingdomsScenario()
    ];

    public static DynamicSemanticScenario FrontierScenario() =>
        new()
        {
            ScenarioId = "frontier_survival",
            ProfileId = "frontier_survival",
            Seed = 3201,
            Targets =
            [
                Target("world/frontier", "world", [], ["frontier"]),
                Target("kingdom/rangers", "kingdom", ["world/frontier"], ["frontier"]),
                Target("region/river_ford", "region", ["kingdom/rangers"], ["frontier"]),
                Target("biome/predator_range", "biome", ["region/river_ford"], ["hazard"]),
                Target("settlement/stockade", "settlement", ["biome/predator_range"], ["has_market"]),
                Target("npc/trail_medic", "npc", ["settlement/stockade"], ["frontier"]),
                Target("quest/medicine_run", "quest", ["region/river_ford"], ["frontier"]),
                Target("dialogue/medic_warning", "dialogue", ["npc/trail_medic", "quest/medicine_run"], ["frontier"])
            ],
            Assignments =
            [
                Assign("world/frontier", "world", "world.theme", Enum("frontier"), "world"),
                Assign("region/river_ford", "region", "region.route_pressure", Enum("predators"), "region"),
                Assign("biome/predator_range", "biome", "biome.hazard", Enum("beasts"), "biome"),
                Assign("settlement/stockade", "settlement", "settlement.market_need", Weighted(("medicine", 0.8), ("ammo", 0.35)), "settlement"),
                Assign("npc/trail_medic", "npc", "npc.hunger", Number(8), "instance"),
                Assign("quest/medicine_run", "quest", "quest.motive", Enum("survive"), "quest")
            ],
            InfluenceRules = BuildDefaultInfluenceRules(),
            ResolveTargetIds = ["npc/trail_medic", "settlement/stockade", "quest/medicine_run", "dialogue/medic_warning"]
        };

    public static DynamicSemanticScenario GothicScenario() =>
        new()
        {
            ScenarioId = "gothic_intrigue",
            ProfileId = "gothic_intrigue",
            Seed = 3202,
            Targets =
            [
                Target("world/gothic", "world", [], ["gothic"]),
                Target("kingdom/old_court", "kingdom", ["world/gothic"], ["court"]),
                Target("faction/old_house", "faction", ["kingdom/old_court"], ["gothic"]),
                Target("npc/disgraced_heir", "npc", ["faction/old_house"], ["has_faction", "gothic"]),
                Target("quest/crypt_secret", "quest", ["faction/old_house"], ["gothic"]),
                Target("dialogue/veiled_threat", "dialogue", ["npc/disgraced_heir", "quest/crypt_secret"], ["gothic"])
            ],
            Assignments =
            [
                Assign("world/gothic", "world", "world.theme", Enum("gothic"), "world"),
                Assign("kingdom/old_court", "kingdom", "kingdom.pressure", Number(6), "kingdom"),
                Assign("faction/old_house", "faction", "faction.reputation_axis", Enum("suspicion"), "faction"),
                Assign("npc/disgraced_heir", "npc", "npc.faction_relation", Relation("member_of", "faction", "faction/old_house", 0.9), "instance"),
                Assign("quest/crypt_secret", "quest", "quest.motive", Enum("secret"), "quest")
            ],
            InfluenceRules = BuildDefaultInfluenceRules(),
            ResolveTargetIds = ["npc/disgraced_heir", "dialogue/veiled_threat", "quest/crypt_secret"]
        };

    public static DynamicSemanticScenario CaravanScenario() =>
        new()
        {
            ScenarioId = "caravan_trade",
            ProfileId = "caravan_trade",
            Seed = 3203,
            Targets =
            [
                Target("world/caravan", "world", [], ["caravan"]),
                Target("kingdom/salt_road", "kingdom", ["world/caravan"], ["trade"]),
                Target("region/toll_pass", "region", ["kingdom/salt_road"], ["route"]),
                Target("settlement/trade_outpost", "settlement", ["region/toll_pass"], ["has_market", "caravan"]),
                Target("faction/merchant_council", "faction", ["kingdom/salt_road"], ["merchant"]),
                Target("quest/delivery_bargain", "quest", ["settlement/trade_outpost"], ["contract"]),
                Target("dialogue/factor_offer", "dialogue", ["quest/delivery_bargain"], ["caravan"])
            ],
            Assignments =
            [
                Assign("world/caravan", "world", "world.theme", Enum("caravan"), "world"),
                Assign("region/toll_pass", "region", "region.route_pressure", Enum("toll_risk"), "region"),
                Assign("settlement/trade_outpost", "settlement", "settlement.market_need", Weighted(("spice", 0.7), ("credit", 0.6)), "settlement"),
                Assign("faction/merchant_council", "faction", "faction.reputation_axis", Enum("credit"), "faction"),
                Assign("quest/delivery_bargain", "quest", "quest.motive", Enum("contract"), "quest")
            ],
            InfluenceRules = BuildDefaultInfluenceRules(),
            ResolveTargetIds = ["settlement/trade_outpost", "quest/delivery_bargain", "dialogue/factor_offer"]
        };

    public static DynamicSemanticScenario MetamoduleKingdomsScenario() =>
        new()
        {
            ScenarioId = "metamodule_kingdoms",
            ProfileId = "metamodule_kingdoms",
            Seed = 3204,
            Targets =
            [
                Target("world/metamodule", "world", [], ["metamodule"]),
                Target("kingdom/auric", "kingdom", ["world/metamodule"], ["mana_court"]),
                Target("kingdom/umbra", "kingdom", ["world/metamodule"], ["pressure_front"]),
                Target("region/auric_core", "region", ["kingdom/auric"], ["mana"]),
                Target("region/umbra_border", "region", ["kingdom/umbra"], ["siege"]),
                Target("species/metamodule_bearer", "species", ["world/metamodule"], ["metamodule_bearer"]),
                Target("archetype/module_scout", "archetype", ["species/metamodule_bearer", "kingdom/auric"], ["metamodule_bearer"]),
                Target("npc/bearer_scout", "npc", ["archetype/module_scout", "kingdom/auric"], ["metamodule_bearer"]),
                Target("magic/resonance_grid", "magic", ["species/metamodule_bearer"], ["metamodule"]),
                Target("combat/border_raid", "combat", ["region/umbra_border"], ["siege"]),
                Target("event/affinity_purge", "event", ["kingdom/umbra"], ["metamodule"]),
                Target("quest/module_capacity_trial", "quest", ["archetype/module_scout"], ["metamodule"])
            ],
            Assignments =
            [
                Assign("world/metamodule", "world", "world.theme", Enum("metamodule"), "world"),
                Assign("kingdom/auric", "kingdom", "kingdom.pressure", Number(4), "kingdom"),
                Assign("kingdom/umbra", "kingdom", "kingdom.pressure", Number(8), "kingdom"),
                Assign("region/umbra_border", "region", "region.route_pressure", Enum("siege"), "region"),
                Assign("species/metamodule_bearer", "species", "species.mana_resonance", Number(7), "species"),
                Assign("species/metamodule_bearer", "species", "species.module_capacity", Number(3), "species"),
                Assign("archetype/module_scout", "archetype", "archetype.forbidden_affinity", Enum("void"), "archetype"),
                Assign("magic/resonance_grid", "magic", "magic.mana_pressure", Number(7), "magic"),
                Assign("combat/border_raid", "combat", "combat.threat_level", Number(6), "combat"),
                Assign("quest/module_capacity_trial", "quest", "quest.motive", Enum("kingdom_pressure"), "quest")
            ],
            InfluenceRules = BuildDefaultInfluenceRules(),
            ResolveTargetIds = ["species/metamodule_bearer", "archetype/module_scout", "npc/bearer_scout", "event/affinity_purge", "quest/module_capacity_trial"]
        };

    public static DynamicSemanticFeatureValue Flag(bool value) => new() { ValueKind = "flag", FlagValue = value };
    public static DynamicSemanticFeatureValue Number(double value) => new() { ValueKind = "number", NumberValue = value };
    public static DynamicSemanticFeatureValue Enum(string value) => new() { ValueKind = "enum", EnumValue = value };
    public static DynamicSemanticFeatureValue TextKey(string value) => new() { ValueKind = "text_key", TextKeyValue = value };
    public static DynamicSemanticFeatureValue List(params string[] values) => new() { ValueKind = "list", ListValues = values.Order(StringComparer.Ordinal).ToList() };
    public static DynamicSemanticFeatureValue Weighted(params (string Tag, double Weight)[] tags) => new()
    {
        ValueKind = "weighted_tag",
        WeightedTags = tags
            .Select(item => new DynamicSemanticWeightedTag { Tag = item.Tag, Weight = item.Weight })
            .OrderBy(item => item.Tag, StringComparer.Ordinal)
            .ToList()
    };

    public static DynamicSemanticFeatureValue Relation(string kind, string targetScope, string targetId, double strength) => new()
    {
        ValueKind = "relation",
        RelationValue = new DynamicSemanticRelationValue
        {
            RelationKind = kind,
            TargetScope = targetScope,
            TargetId = targetId,
            Strength = strength
        }
    };

    private static DynamicSemanticFeatureDefinition Feature(
        string id,
        string name,
        string scope,
        string kind,
        string required,
        string defaultStrategy,
        string inheritance,
        IReadOnlyList<string> inheritedScopes,
        IReadOnlyList<string>? allowed = null,
        DynamicSemanticFeatureValue? defaultValue = null,
        string group = "",
        double? min = null,
        double? max = null,
        IReadOnlyList<DynamicSemanticConditionClause>? conditions = null,
        IReadOnlyList<string>? tags = null) =>
        new()
        {
            FeatureId = id,
            DisplayName = name,
            TargetScope = scope,
            ValueKind = kind,
            RequiredMode = required,
            DefaultStrategy = defaultStrategy,
            DefaultValue = defaultValue,
            InheritanceMode = inheritance,
            InheritedSourceScopes = inheritedScopes,
            AllowedValues = allowed ?? [],
            MinValue = min,
            MaxValue = max,
            ApplicabilityConditions = conditions ?? [],
            Tags = tags ?? [],
            AuthoringGroup = string.IsNullOrWhiteSpace(group) ? scope : group,
            Provenance = "goal_032_seed_catalog",
            Notes = "BCL-only dynamic semantic feature seed."
        };

    private static DynamicSemanticConditionClause Condition(
        string op,
        string featureId = "",
        string value = "",
        double? number = null,
        string tag = "",
        string scope = "") =>
        new()
        {
            Operator = op,
            FeatureId = featureId,
            ExpectedValue = value,
            NumberValue = number,
            Tag = tag,
            Scope = scope
        };

    private static DynamicSemanticInfluenceEffect Effect(
        string kind,
        string featureId = "",
        DynamicSemanticFeatureValue? value = null,
        double numberDelta = 0,
        string intent = "",
        string suggestion = "",
        string diagnosticCode = "",
        string diagnosticMessage = "") =>
        new()
        {
            EffectKind = kind,
            FeatureId = featureId,
            Value = value,
            NumberDelta = numberDelta,
            IntentId = intent,
            Suggestion = suggestion,
            DiagnosticCode = diagnosticCode,
            DiagnosticMessage = diagnosticMessage
        };

    private static DynamicSemanticInfluenceRule Rule(
        string id,
        string scope,
        string family,
        IReadOnlyList<DynamicSemanticConditionClause> conditions,
        IReadOnlyList<DynamicSemanticInfluenceEffect> effects,
        int priority,
        string explanation) =>
        new()
        {
            RuleId = id,
            TargetScope = scope,
            TargetFamily = family,
            Conditions = conditions,
            Effects = effects,
            Priority = priority,
            TieBreaker = id,
            Provenance = "goal_032_seed_catalog",
            Explanation = explanation
        };

    private static DynamicSemanticTargetNode Target(string id, string scope, IReadOnlyList<string> parents, IReadOnlyList<string> tags) =>
        new()
        {
            TargetId = id,
            TargetScope = scope,
            ParentTargetIds = parents,
            Tags = tags,
            FamilyIds = tags
        };

    private static DynamicSemanticFeatureAssignment Assign(
        string targetId,
        string scope,
        string featureId,
        DynamicSemanticFeatureValue value,
        string layer) =>
        new()
        {
            TargetId = targetId,
            TargetScope = scope,
            FeatureId = featureId,
            Value = value,
            SourceLayer = layer,
            SourceId = targetId,
            OverrideMode = "set",
            Priority = 10,
            Provenance = "goal_032_seed_catalog"
        };
}
