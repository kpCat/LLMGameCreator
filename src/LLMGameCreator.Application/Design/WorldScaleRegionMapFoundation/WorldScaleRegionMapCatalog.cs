using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public static class WorldScaleRegionMapCatalog
{
    private static readonly IReadOnlyDictionary<string, string[]> Goal037EvidenceIds = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["frontier_survival"] =
        [
            "hybrid-expansion/frontier_survival/npc_species_archetype_expansion_hints",
            "hybrid-expansion/frontier_survival/quest_event_intent_expansion_hints"
        ],
        ["gothic_intrigue"] =
        [
            "hybrid-expansion/gothic_intrigue/quest_event_intent_expansion_hints",
            "hybrid-expansion/gothic_intrigue/region_faction_kingdom_expansion_hints"
        ],
        ["caravan_trade"] =
        [
            "hybrid-expansion/caravan_trade/economy_combat_settlement_expansion_hints",
            "hybrid-expansion/caravan_trade/quest_event_intent_expansion_hints"
        ],
        ["metamodule_kingdoms"] =
        [
            "hybrid-expansion/metamodule_kingdoms/metamodule_species_archetype_slot_expansion",
            "hybrid-expansion/metamodule_kingdoms/region_faction_kingdom_expansion_hints"
        ]
    };

    public static IReadOnlySet<string> AcceptedGoal037EvidenceIds { get; } = Goal037EvidenceIds
        .SelectMany(item => item.Value)
        .ToHashSet(StringComparer.Ordinal);

    public static IReadOnlyList<WorldScaleRegionGraph> BuildDefaultGraphs() =>
    [
        SortGraph(BuildFrontierGraph()),
        SortGraph(BuildGothicGraph()),
        SortGraph(BuildCaravanGraph()),
        SortGraph(BuildMetamoduleGraph())
    ];

    public static WorldScaleRegionGraphSummary BuildSummary(IReadOnlyList<WorldScaleRegionGraph> graphs)
    {
        var routeKinds = graphs
            .SelectMany(item => item.TravelEdges)
            .Select(item => item.RouteKind)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new WorldScaleRegionGraphSummary
        {
            Accepted = false,
            ScenarioCount = graphs.Count,
            TotalKingdomCount = graphs.Sum(item => item.Kingdoms.Count),
            TotalRegionCount = graphs.Sum(item => item.Regions.Count),
            TotalTravelEdgeCount = graphs.Sum(item => item.TravelEdges.Count),
            RouteKindsCovered = routeKinds,
            Graphs = graphs.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static string ComputeHash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    public static WorldScaleRegionMapDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<WorldScaleRegionMapDiagnostic> SortDiagnostics(IEnumerable<WorldScaleRegionMapDiagnostic> diagnostics) =>
        diagnostics
            .GroupBy(item => (item.Severity, item.Code, item.Target, item.Message))
            .Select(group => group.First())
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static IReadOnlyList<WorldScaleSourceEvidenceRef> Goal037Refs(string scenarioId) =>
        Goal037EvidenceIds[scenarioId]
            .Select(id => new WorldScaleSourceEvidenceRef
            {
                SourceGoal = "Goal037",
                EvidenceId = id,
                ArtifactFamily = id.Split('/')[^1]
            })
            .OrderBy(item => item.EvidenceId, StringComparer.Ordinal)
            .ToList();

    private static WorldScaleRegionGraph BuildFrontierGraph()
    {
        const string scenario = "frontier_survival";
        var refs = Goal037Refs(scenario);
        return new WorldScaleRegionGraph
        {
            ScenarioId = scenario,
            ProfileId = scenario,
            WorldGraphId = "world-graph/frontier_survival/settlement-river-pass",
            DeterministicSeed = "goal038-frontier-survival-world-scale-seed",
            StartRegionId = "region/frontier/homestead",
            RequiredTargetRegionIds =
            [
                "region/frontier/pine-barrens",
                "region/frontier/river-ford",
                "region/frontier/mountain-pass"
            ],
            OptionalTargetRegionIds = ["region/frontier/coast-watch", "region/frontier/old-mine"],
            SourceEvidenceRefs = refs,
            Kingdoms =
            [
                Kingdom("kingdom/frontier/freehold", "region-group/frontier/freehold",
                    ["region/frontier/homestead", "region/frontier/pine-barrens", "region/frontier/river-ford", "region/frontier/mountain-pass", "region/frontier/coast-watch", "region/frontier/old-mine"],
                    [], ["frontier", "survival", "homestead"])
            ],
            Regions =
            [
                Region("region/frontier/homestead", "kingdom/frontier/freehold", ["temperate_forest"], ["clearing", "farmland"], [], ["clear_morning"], ["settlement_start"], ["settlement/frontier/homestead"], ["landmark/frontier/well"], false, false, refs),
                Region("region/frontier/pine-barrens", "kingdom/frontier/freehold", ["pine_forest"], ["woods", "scrub"], ["wolves"], ["wind"], ["forage"], [], ["landmark/frontier/signal-pine"], true, false, refs),
                Region("region/frontier/river-ford", "kingdom/frontier/freehold", ["riparian"], ["riverbank", "mud"], ["flood"], ["rain"], ["crossing"], [], ["landmark/frontier/ford-stones"], true, false, refs),
                Region("region/frontier/mountain-pass", "kingdom/frontier/freehold", ["alpine"], ["cliff", "snowline"], ["rockslide"], ["thin_air"], ["rescue"], [], ["landmark/frontier/pass-shrine"], true, false, refs),
                Region("region/frontier/coast-watch", "kingdom/frontier/freehold", ["coastal"], ["shingle", "grass"], ["storm"], ["fog"], ["signal_fire"], ["settlement/frontier/watch"], ["landmark/frontier/beacon"], false, true, refs),
                Region("region/frontier/old-mine", "kingdom/frontier/freehold", ["hills"], ["tunnel", "ore"], ["collapse"], ["cold"], ["salvage"], [], ["landmark/frontier/ore-gate"], false, true, refs)
            ],
            TravelEdges =
            [
                Edge("edge/frontier/homestead-pine", "region/frontier/homestead", "region/frontier/pine-barrens", "trail", 2, true, ["survival_route"], refs),
                Edge("edge/frontier/homestead-coast", "region/frontier/homestead", "region/frontier/coast-watch", "road", 4, true, ["supply_route"], refs),
                Edge("edge/frontier/pine-river", "region/frontier/pine-barrens", "region/frontier/river-ford", "river", 3, true, ["water_crossing"], refs),
                Edge("edge/frontier/river-pass", "region/frontier/river-ford", "region/frontier/mountain-pass", "mountain_pass", 5, true, ["highland"], refs),
                Edge("edge/frontier/pine-mine", "region/frontier/pine-barrens", "region/frontier/old-mine", "trail", 3, true, ["salvage"], refs),
                Edge("edge/frontier/coast-pass-future", "region/frontier/coast-watch", "region/frontier/mountain-pass", "sea_lane", 8, false, ["future_boat"], refs, futureRequired: true)
            ]
        };
    }

    private static WorldScaleRegionGraph BuildGothicGraph()
    {
        const string scenario = "gothic_intrigue";
        var refs = Goal037Refs(scenario);
        return new WorldScaleRegionGraph
        {
            ScenarioId = scenario,
            ProfileId = scenario,
            WorldGraphId = "world-graph/gothic_intrigue/manor-crypt-gate",
            DeterministicSeed = "goal038-gothic-intrigue-world-scale-seed",
            StartRegionId = "region/gothic/manor",
            RequiredTargetRegionIds =
            [
                "region/gothic/market-square",
                "region/gothic/abbey",
                "region/gothic/crypt",
                "region/gothic/observatory"
            ],
            OptionalTargetRegionIds = ["region/gothic/forbidden-wing"],
            SourceEvidenceRefs = refs,
            Kingdoms =
            [
                Kingdom("kingdom/gothic/vale", "region-group/gothic/vale",
                    ["region/gothic/manor", "region/gothic/market-square", "region/gothic/abbey", "region/gothic/crypt", "region/gothic/observatory", "region/gothic/forbidden-wing"],
                    [], ["gothic", "intrigue", "social"])
            ],
            Regions =
            [
                Region("region/gothic/abbey", "kingdom/gothic/vale", ["mist_vale"], ["chapel", "graveyard"], ["curse"], ["fog"], ["confession"], ["settlement/gothic/abbey"], ["landmark/gothic/bell-tower"], true, false, refs),
                Region("region/gothic/crypt", "kingdom/gothic/vale", ["undercrypt"], ["catacomb", "stone"], ["undead"], ["stale_air"], ["reveal"], [], ["landmark/gothic/sarcophagus"], true, false, refs),
                Region("region/gothic/forbidden-wing", "kingdom/gothic/vale", ["manor"], ["locked_hall"], ["ward"], ["stillness"], ["future_key"], [], ["landmark/gothic/sealed-door"], false, true, refs),
                Region("region/gothic/manor", "kingdom/gothic/vale", ["mist_vale"], ["manor", "garden"], ["blackmail"], ["moonlight"], ["investigation_start"], ["settlement/gothic/manor"], ["landmark/gothic/family-crest"], false, false, refs),
                Region("region/gothic/market-square", "kingdom/gothic/vale", ["town"], ["cobblestone", "lantern"], ["rumor"], ["drizzle"], ["witness"], ["settlement/gothic/market"], ["landmark/gothic/statue"], true, false, refs),
                Region("region/gothic/observatory", "kingdom/gothic/vale", ["hill"], ["tower", "glass"], ["astral_omen"], ["starlight"], ["omen"], [], ["landmark/gothic/astrolabe"], true, false, refs)
            ],
            TravelEdges =
            [
                Edge("edge/gothic/abbey-crypt", "region/gothic/abbey", "region/gothic/crypt", "dungeon_descent", 4, true, ["descent"], refs),
                Edge("edge/gothic/abbey-wing-future", "region/gothic/abbey", "region/gothic/forbidden-wing", "road", 6, false, ["locked"], refs, isConditional: true),
                Edge("edge/gothic/crypt-observatory", "region/gothic/crypt", "region/gothic/observatory", "magical_gate", 7, false, ["moon_key"], refs),
                Edge("edge/gothic/manor-market", "region/gothic/manor", "region/gothic/market-square", "road", 2, true, ["social"], refs),
                Edge("edge/gothic/market-abbey", "region/gothic/market-square", "region/gothic/abbey", "trail", 3, true, ["pilgrim"], refs)
            ]
        };
    }

    private static WorldScaleRegionGraph BuildCaravanGraph()
    {
        const string scenario = "caravan_trade";
        var refs = Goal037Refs(scenario);
        return new WorldScaleRegionGraph
        {
            ScenarioId = scenario,
            ProfileId = scenario,
            WorldGraphId = "world-graph/caravan_trade/oasis-harbor-pass",
            DeterministicSeed = "goal038-caravan-trade-world-scale-seed",
            StartRegionId = "region/caravan/oasis-camp",
            RequiredTargetRegionIds =
            [
                "region/caravan/spice-market",
                "region/caravan/salt-pass",
                "region/caravan/harbor",
                "region/caravan/glass-dunes"
            ],
            OptionalTargetRegionIds = ["region/caravan/bandit-ruins"],
            SourceEvidenceRefs = refs,
            Kingdoms =
            [
                Kingdom("kingdom/caravan/route", "region-group/caravan/route",
                    ["region/caravan/oasis-camp", "region/caravan/spice-market", "region/caravan/salt-pass", "region/caravan/harbor", "region/caravan/glass-dunes", "region/caravan/bandit-ruins"],
                    [], ["caravan", "trade", "economy"])
            ],
            Regions =
            [
                Region("region/caravan/bandit-ruins", "kingdom/caravan/route", ["badlands"], ["ruin", "canyon"], ["ambush"], ["dust"], ["raid"], [], ["landmark/caravan/broken-arch"], false, true, refs),
                Region("region/caravan/glass-dunes", "kingdom/caravan/route", ["desert"], ["glass", "dune"], ["heat"], ["mirage"], ["resource"], [], ["landmark/caravan/glass-field"], true, false, refs),
                Region("region/caravan/harbor", "kingdom/caravan/route", ["coast"], ["dock", "warehouse"], ["tariff"], ["sea_wind"], ["contract"], ["settlement/caravan/harbor"], ["landmark/caravan/tide-gate"], true, false, refs),
                Region("region/caravan/oasis-camp", "kingdom/caravan/route", ["oasis"], ["water", "palm"], ["scarcity"], ["clear"], ["trade_start"], ["settlement/caravan/oasis"], ["landmark/caravan/well"], false, false, refs),
                Region("region/caravan/salt-pass", "kingdom/caravan/route", ["salt_flat"], ["pass", "white_stone"], ["bandits"], ["glare"], ["escort"], [], ["landmark/caravan/salt-marker"], true, false, refs),
                Region("region/caravan/spice-market", "kingdom/caravan/route", ["city"], ["bazaar", "alley"], ["debt"], ["hot"], ["bargain"], ["settlement/caravan/market"], ["landmark/caravan/auction-bell"], true, false, refs)
            ],
            TravelEdges =
            [
                Edge("edge/caravan/harbor-glass", "region/caravan/harbor", "region/caravan/glass-dunes", "sea_lane", 6, true, ["cargo"], refs),
                Edge("edge/caravan/oasis-market", "region/caravan/oasis-camp", "region/caravan/spice-market", "caravan_route", 3, true, ["trade"], refs),
                Edge("edge/caravan/ruins-harbor-future", "region/caravan/bandit-ruins", "region/caravan/harbor", "trail", 9, false, ["future_safe_passage"], refs, isBlocked: true),
                Edge("edge/caravan/salt-ruins", "region/caravan/salt-pass", "region/caravan/bandit-ruins", "trail", 4, true, ["risk"], refs),
                Edge("edge/caravan/salt-harbor", "region/caravan/salt-pass", "region/caravan/harbor", "road", 5, true, ["customs"], refs),
                Edge("edge/caravan/market-salt", "region/caravan/spice-market", "region/caravan/salt-pass", "caravan_route", 4, true, ["merchant"], refs)
            ]
        };
    }

    private static WorldScaleRegionGraph BuildMetamoduleGraph()
    {
        const string scenario = "metamodule_kingdoms";
        var refs = Goal037Refs(scenario);
        var kingdoms = BuildMetamoduleKingdoms();
        var regions = kingdoms
            .SelectMany(group => group.RegionIds.Select((regionId, index) =>
                Region(
                    regionId,
                    group.KingdomId,
                    [$"biome:{group.KingdomId.Split('/')[^1]}", "metamodule"],
                    index == 0 ? ["capital", "district"] : ["wildland", "threshold"],
                    [$"hazard:{group.KingdomId.Split('/')[^1]}"],
                    index == 0 ? ["court_weather"] : ["border_weather"],
                    index == 0 ? ["kingdom_pressure"] : ["frontier_event"],
                    index == 0 ? [$"settlement/{group.KingdomId.Split('/')[^1]}/capital"] : [],
                    [$"landmark/{group.KingdomId.Split('/')[^1]}/{(index == 0 ? "crown" : "gate")}"],
                    index == 0,
                    index != 0,
                    refs)))
            .ToList();

        return new WorldScaleRegionGraph
        {
            ScenarioId = scenario,
            ProfileId = scenario,
            WorldGraphId = "world-graph/metamodule_kingdoms/seven-kingdom-weave",
            DeterministicSeed = "goal038-metamodule-kingdoms-world-scale-seed",
            StartRegionId = "region/metamodule/aurelian-capital",
            RequiredTargetRegionIds = kingdoms.Select(group => group.RegionIds[0]).Order(StringComparer.Ordinal).ToList(),
            OptionalTargetRegionIds = kingdoms.Select(group => group.RegionIds[1]).Order(StringComparer.Ordinal).ToList(),
            SourceEvidenceRefs = refs,
            Kingdoms = kingdoms,
            Regions = regions,
            TravelEdges =
            [
                Edge("edge/metamodule/aurelian-brindle", "region/metamodule/aurelian-capital", "region/metamodule/brindle-capital", "road", 3, true, ["kingdom_ring"], refs),
                Edge("edge/metamodule/aurelian-cindervale", "region/metamodule/aurelian-wilds", "region/metamodule/cindervale-wilds", "mountain_pass", 6, true, ["wild_crossing"], refs),
                Edge("edge/metamodule/brindle-cindervale", "region/metamodule/brindle-capital", "region/metamodule/cindervale-capital", "river", 4, true, ["river_court"], refs),
                Edge("edge/metamodule/brindle-wilds", "region/metamodule/brindle-capital", "region/metamodule/brindle-wilds", "trail", 2, true, ["internal"], refs),
                Edge("edge/metamodule/cindervale-duskmire", "region/metamodule/cindervale-capital", "region/metamodule/duskmire-capital", "caravan_route", 5, true, ["merchant_court"], refs),
                Edge("edge/metamodule/cindervale-wilds", "region/metamodule/cindervale-capital", "region/metamodule/cindervale-wilds", "trail", 2, true, ["internal"], refs),
                Edge("edge/metamodule/duskmire-elderglass", "region/metamodule/duskmire-capital", "region/metamodule/elderglass-capital", "sea_lane", 7, true, ["mist_sea"], refs),
                Edge("edge/metamodule/duskmire-wilds", "region/metamodule/duskmire-capital", "region/metamodule/duskmire-wilds", "dungeon_descent", 4, false, ["underway"], refs),
                Edge("edge/metamodule/elderglass-frostmere", "region/metamodule/elderglass-capital", "region/metamodule/frostmere-capital", "magical_gate", 8, false, ["resonance_gate"], refs),
                Edge("edge/metamodule/elderglass-wilds", "region/metamodule/elderglass-capital", "region/metamodule/elderglass-wilds", "trail", 2, true, ["internal"], refs),
                Edge("edge/metamodule/frostmere-goldwake", "region/metamodule/frostmere-capital", "region/metamodule/goldwake-capital", "mountain_pass", 6, true, ["ice_pass"], refs),
                Edge("edge/metamodule/frostmere-wilds", "region/metamodule/frostmere-capital", "region/metamodule/frostmere-wilds", "trail", 2, true, ["internal"], refs),
                Edge("edge/metamodule/goldwake-aurelian", "region/metamodule/goldwake-capital", "region/metamodule/aurelian-capital", "sea_lane", 5, true, ["sun_sea"], refs),
                Edge("edge/metamodule/goldwake-wilds", "region/metamodule/goldwake-capital", "region/metamodule/goldwake-wilds", "trail", 2, true, ["internal"], refs),
                Edge("edge/metamodule/wild-future-gate", "region/metamodule/goldwake-wilds", "region/metamodule/aurelian-wilds", "magical_gate", 9, false, ["goal041_future_delta"], refs, futureRequired: true)
            ]
        };
    }

    private static IReadOnlyList<WorldScaleKingdomGroup> BuildMetamoduleKingdoms()
    {
        var names = new[]
        {
            "aurelian",
            "brindle",
            "cindervale",
            "duskmire",
            "elderglass",
            "frostmere",
            "goldwake"
        };

        return names
            .Select((name, index) => Kingdom(
                $"kingdom/metamodule/{name}",
                $"region-group/metamodule/{name}",
                [$"region/metamodule/{name}-capital", $"region/metamodule/{name}-wilds"],
                Enumerable.Range(index * 16 + 1, 16).Select(slot => $"slot/metamodule/species-archetype/{slot:000}").ToList(),
                ["metamodule", "kingdom_group", $"kingdom:{name}"]))
            .OrderBy(item => item.KingdomId, StringComparer.Ordinal)
            .ToList();
    }

    private static WorldScaleRegionGraph SortGraph(WorldScaleRegionGraph graph) =>
        graph with
        {
            RequiredTargetRegionIds = graph.RequiredTargetRegionIds.Order(StringComparer.Ordinal).ToList(),
            OptionalTargetRegionIds = graph.OptionalTargetRegionIds.Order(StringComparer.Ordinal).ToList(),
            Kingdoms = graph.Kingdoms.OrderBy(item => item.KingdomId, StringComparer.Ordinal).ToList(),
            Regions = graph.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal).ToList(),
            TravelEdges = graph.TravelEdges.OrderBy(item => item.EdgeId, StringComparer.Ordinal).ToList(),
            SourceEvidenceRefs = graph.SourceEvidenceRefs.OrderBy(item => item.EvidenceId, StringComparer.Ordinal).ToList()
        };

    private static WorldScaleKingdomGroup Kingdom(
        string kingdomId,
        string regionGroupId,
        IReadOnlyList<string> regionIds,
        IReadOnlyList<string> slotRefs,
        IReadOnlyList<string> tags) =>
        new()
        {
            KingdomId = kingdomId,
            RegionGroupId = regionGroupId,
            RegionIds = regionIds.Order(StringComparer.Ordinal).ToList(),
            SpeciesArchetypeSlotRefs = slotRefs.Order(StringComparer.Ordinal).ToList(),
            SemanticTags = tags.Order(StringComparer.Ordinal).ToList()
        };

    private static WorldScaleRegionNode Region(
        string regionId,
        string kingdomId,
        IReadOnlyList<string> biomeTags,
        IReadOnlyList<string> terrainTags,
        IReadOnlyList<string> hazardTags,
        IReadOnlyList<string> weatherTags,
        IReadOnlyList<string> eventTags,
        IReadOnlyList<string> settlementIds,
        IReadOnlyList<string> landmarkIds,
        bool required,
        bool optional,
        IReadOnlyList<WorldScaleSourceEvidenceRef> refs) =>
        new()
        {
            RegionId = regionId,
            KingdomId = kingdomId,
            BiomeTags = biomeTags.Order(StringComparer.Ordinal).ToList(),
            TerrainTags = terrainTags.Order(StringComparer.Ordinal).ToList(),
            HazardTags = hazardTags.Order(StringComparer.Ordinal).ToList(),
            WeatherTags = weatherTags.Order(StringComparer.Ordinal).ToList(),
            EventTags = eventTags.Order(StringComparer.Ordinal).ToList(),
            SettlementIds = settlementIds.Order(StringComparer.Ordinal).ToList(),
            LandmarkIds = landmarkIds.Order(StringComparer.Ordinal).ToList(),
            RequiredGameplayTarget = required,
            OptionalTarget = optional,
            SourceEvidenceRefs = refs
        };

    private static WorldScaleTravelEdge Edge(
        string edgeId,
        string from,
        string to,
        string routeKind,
        int cost,
        bool bidirectional,
        IReadOnlyList<string> tags,
        IReadOnlyList<WorldScaleSourceEvidenceRef> refs,
        bool isBlocked = false,
        bool isConditional = false,
        bool futureRequired = false) =>
        new()
        {
            EdgeId = edgeId,
            FromRegionId = from,
            ToRegionId = to,
            RouteKind = routeKind,
            Cost = cost,
            Bidirectional = bidirectional,
            IsBlocked = isBlocked,
            IsConditional = isConditional,
            FutureRequired = futureRequired,
            Constraints = (isBlocked || isConditional || futureRequired) ? ["future_or_conditional"] : [],
            SemanticTags = tags.Order(StringComparer.Ordinal).ToList(),
            SourceEvidenceRefs = refs
        };

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };
}
