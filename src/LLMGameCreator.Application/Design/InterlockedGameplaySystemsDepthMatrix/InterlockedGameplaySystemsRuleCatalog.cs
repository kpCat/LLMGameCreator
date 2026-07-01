namespace LLMGameCreator.Application.Design.InterlockedGameplaySystemsDepthMatrix;

public static class InterlockedGameplaySystemsRuleCatalogBuilder
{
    public static InterlockedGameplayRuleCatalog Build()
    {
        var profiles = new List<InterlockedGameplayRuleProfile>
        {
            new()
            {
                FamilyId = "map_panel_rpg",
                RuleSetId = "map_panel_rpg/npc_trade_work_conflict_progression_interlock",
                RequiredDeltaCategories = RequiredCategories(),
                FamilyExpectations =
                [
                    "npc_faction_work_trade_social_consequence",
                    "conflict_combat_progression_delta",
                    "inventory_reward_status_delta"
                ],
                ForbiddenClaims = BoundaryClaims()
            },
            new()
            {
                FamilyId = "survival_sandbox",
                RuleSetId = "survival_sandbox/hazard_resource_craft_recover_interlock",
                RequiredDeltaCategories = RequiredCategories(),
                FamilyExpectations =
                [
                    "hazard_need_resource_pressure",
                    "collect_consume_craft_recover_delta",
                    "condition_status_pressure"
                ],
                ForbiddenClaims = BoundaryClaims()
            },
            new()
            {
                FamilyId = "first_person_grid_dungeon",
                RuleSetId = "first_person_grid_dungeon/orientation_encounter_loot_key_interlock",
                RequiredDeltaCategories = RequiredCategories(),
                FamilyExpectations =
                [
                    "orientation_traversal_context",
                    "encounter_combat_pressure",
                    "loot_key_progression_status_and_blocked_movement"
                ],
                ForbiddenClaims = BoundaryClaims()
            }
        };

        return new InterlockedGameplayRuleCatalog
        {
            Passed = profiles.Count == 3 && profiles.All(item => item.RequiredDeltaCategories.Count == RequiredCategories().Count),
            RuleProfileCount = profiles.Count,
            Profiles = profiles
        };
    }

    public static IReadOnlyList<string> RequiredCategories() =>
    [
        "economy",
        "crafting",
        "combat",
        "progression",
        "status",
        "inventory",
        "living_world"
    ];

    private static IReadOnlyList<string> BoundaryClaims() =>
    [
        "provider_llm_rag_media_generation",
        "runtime_runtime_abstractions_mutation",
        "winforms_ui_mutation",
        "gamepackage_schema_mutation",
        "arbitrary_lua_execution",
        "broad_unity_gameplay_system"
    ];
}
