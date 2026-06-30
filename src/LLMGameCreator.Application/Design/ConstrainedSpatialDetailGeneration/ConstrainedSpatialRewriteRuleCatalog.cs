namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialRewriteRuleCatalogBuilder
{
    public ConstrainedSpatialRewriteRuleCatalog Build()
    {
        var rules = new List<ConstrainedSpatialRewriteRule>
        {
            Rule("rule/spatial/ensure_entry_exists", ConstrainedSpatialDetailVocabulary.FamilyIds, 10, "row has no entry anchor", "place exactly one deterministic entry anchor", "goal062.rule.ensure_entry"),
            Rule("rule/spatial/ensure_exit_exists", ConstrainedSpatialDetailVocabulary.FamilyIds, 20, "row has no exit anchor", "place exactly one deterministic exit anchor opposite the entry", "goal062.rule.ensure_exit"),
            Rule("rule/spatial/ensure_objective_anchor_exists", ConstrainedSpatialDetailVocabulary.FamilyIds, 30, "row has no objective-like semantic anchor", "place one family-specific objective anchor", "goal062.rule.ensure_objective"),
            Rule("rule/spatial/connect_critical_anchors", ConstrainedSpatialDetailVocabulary.FamilyIds, 40, "entry/objective/exit are disconnected", "carve deterministic passable route segments between critical anchors", "goal062.rule.connect_anchors"),
            Rule("rule/spatial/repair_isolated_passable_pockets", ConstrainedSpatialDetailVocabulary.FamilyIds, 50, "passable pocket is not connected to the main route", "convert or connect the pocket using the nearest deterministic route cell", "goal062.rule.repair_pockets"),
            Rule("rule/spatial/mark_blocked_or_unsafe_cells", ConstrainedSpatialDetailVocabulary.FamilyIds, 60, "blocked/unsafe cells are unclassified", "tag blocked cells and forbid unsafe route traversal", "goal062.rule.mark_blocked_unsafe"),
            Rule("rule/spatial/map_panel_rpg_insert_landmark", ["map_panel_rpg"], 70, "map_panel_rpg lacks RPG landmark/resource/quest affordance", "insert settlement, NPC, quest marker and item marker off the route spine", "goal062.rule.map_landmarks"),
            Rule("rule/spatial/survival_sandbox_insert_resource_loop", ["survival_sandbox"], 70, "survival_sandbox lacks survival resource loop", "insert shelter, water, resource, weather marker and off-route hazard", "goal062.rule.survival_resources"),
            Rule("rule/spatial/first_person_grid_dungeon_insert_encounter_chain", ["first_person_grid_dungeon"], 70, "first_person_grid_dungeon lacks dungeon sequence", "insert door, corridor, encounter, objective and exit sequence", "goal062.rule.dungeon_chain")
        };

        return new ConstrainedSpatialRewriteRuleCatalog
        {
            Passed = rules.Count >= 9
                && RequiredRuleFragments().All(fragment => rules.Any(rule => rule.RuleId.Contains(fragment, StringComparison.Ordinal)))
                && rules.Select(rule => rule.DeterministicApplicationOrder).SequenceEqual(rules.Select(rule => rule.DeterministicApplicationOrder).Order()),
            RuleCount = rules.Count,
            Rules = rules.OrderBy(rule => rule.DeterministicApplicationOrder).ToList()
        };
    }

    private static ConstrainedSpatialRewriteRule Rule(
        string id,
        IReadOnlyList<string> families,
        int priority,
        string matchDescription,
        string effectDescription,
        string diagnosticCode) =>
        new()
        {
            RuleId = id,
            FamilyApplicability = families,
            Priority = priority,
            MatchDescription = matchDescription,
            EffectDescription = effectDescription,
            DeterministicApplicationOrder = priority,
            Diagnostics =
            [
                ConstrainedSpatialDiagnostic.Info(diagnosticCode, id, "Rule is a deterministic in-house record; no external rewrite interpreter is used.")
            ]
        };

    private static IReadOnlyList<string> RequiredRuleFragments() =>
    [
        "ensure_entry",
        "ensure_exit",
        "ensure_objective",
        "connect_critical",
        "repair_isolated",
        "map_panel_rpg_insert",
        "survival_sandbox_insert",
        "first_person_grid_dungeon_insert",
        "mark_blocked"
    ];
}
