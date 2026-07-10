using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;

public static class ProductLineRuntimeVariantCatalog
{
    public static ProductLineRuntimeVariantCatalogDocument CreateDefault() =>
        new()
        {
            Variants =
            [
                new ProductLineRuntimeVariantRecipe
                {
                    RecipeId = "balanced_baseline",
                    CandidateId = "minimal-map-game-balanced-baseline",
                    DisplayName = "Balanced Baseline",
                    VariantKind = "balanced_baseline",
                    RuntimeSignificant = true,
                    ExpectedRuntimeEffects =
                    [
                        "baseline runtime comparison row exists",
                        "corrected Goal141A roundtrip passes over the baseline candidate"
                    ],
                    SelectionWeights = new ProductLineRuntimeVariantSelectionWeights
                    {
                        TieBreakPriority = 0
                    },
                    MutationOperations = []
                },
                new ProductLineRuntimeVariantRecipe
                {
                    RecipeId = "alchemy_focus",
                    CandidateId = "minimal-map-game-alchemy-focus",
                    DisplayName = "Alchemy Focus",
                    VariantKind = "alchemy_focus",
                    RuntimeSignificant = true,
                    ExpectedRuntimeEffects =
                    [
                        "final inventory summary differs from baseline",
                        "final runtime state hash differs from baseline",
                        "craft request still passes"
                    ],
                    SelectionWeights = new ProductLineRuntimeVariantSelectionWeights
                    {
                        TieBreakPriority = 1
                    },
                    MutationOperations =
                    [
                        Operation(
                            "alchemy.starting_red_herb_supply",
                            "inventory_stack_amount",
                            "inventory/player_start|item/red_herb",
                            "game.inventories[id=inventory/player_start].stacks[itemId=item/red_herb].amount",
                            "2",
                            "4",
                            "alchemy_inventory_resource"),
                        Operation(
                            "alchemy.starting_water_supply",
                            "inventory_stack_amount",
                            "inventory/player_start|item/water_flask",
                            "game.inventories[id=inventory/player_start].stacks[itemId=item/water_flask].amount",
                            "1",
                            "2",
                            "alchemy_inventory_resource"),
                        Operation(
                            "alchemy.healing_potion_output_quantity",
                            "recipe_output_amount",
                            "recipe/healing_potion|item/healing_potion",
                            "game.recipes[id=recipe/healing_potion].outputs[id=item/healing_potion].amount",
                            "1",
                            "2",
                            "alchemy_recipe_output")
                    ]
                },
                new ProductLineRuntimeVariantRecipe
                {
                    RecipeId = "combat_focus",
                    CandidateId = "minimal-map-game-combat-focus",
                    DisplayName = "Combat Focus",
                    VariantKind = "combat_focus",
                    RuntimeSignificant = true,
                    ExpectedRuntimeEffects =
                    [
                        "combat summary or post-combat participant resource state differs from baseline",
                        "final runtime state hash differs from baseline",
                        "combat request still passes"
                    ],
                    SelectionWeights = new ProductLineRuntimeVariantSelectionWeights
                    {
                        TieBreakPriority = 2
                    },
                    MutationOperations =
                    [
                        Operation(
                            "combat.goblin_health_pool",
                            "encounter_participant_resource_amount",
                            "encounter/goblin_duel|goblin|resource/health",
                            "game.encounters[id=encounter/goblin_duel].participants[id=goblin].resources[id=resource/health].amount",
                            "12",
                            "16",
                            "combat_participant_resource"),
                        Operation(
                            "combat.basic_attack_power",
                            "ability_power",
                            "ability/basic_attack",
                            "game.abilities[id=ability/basic_attack].power",
                            "4",
                            "6",
                            "combat_ability_damage"),
                        Operation(
                            "combat.basic_attack_damage_effect",
                            "ability_effect_arg_amount",
                            "ability/basic_attack|damage_resource|resource/health",
                            "game.abilities[id=ability/basic_attack].effects[type=damage_resource,args.id=resource/health].args.amount",
                            "4",
                            "6",
                            "combat_ability_damage")
                    ]
                },
                new ProductLineRuntimeVariantRecipe
                {
                    RecipeId = "exploration_resource_focus",
                    CandidateId = "minimal-map-game-exploration-resource-focus",
                    DisplayName = "Exploration Resource Focus",
                    VariantKind = "exploration_resource_focus",
                    RuntimeSignificant = true,
                    ExpectedRuntimeEffects =
                    [
                        "harvest, transaction or inventory summary differs from baseline",
                        "final runtime state hash differs from baseline",
                        "harvest and transaction requests still pass"
                    ],
                    SelectionWeights = new ProductLineRuntimeVariantSelectionWeights
                    {
                        TieBreakPriority = 3
                    },
                    MutationOperations =
                    [
                        Operation(
                            "exploration.apple_tree_loot_minimum",
                            "loot_entry_min_count",
                            "loot/apple_tree|entry/apple",
                            "game.lootTables[id=loot/apple_tree].entries[id=entry/apple].minCount",
                            "1",
                            "3",
                            "harvest_loot_quantity"),
                        Operation(
                            "exploration.apple_tree_loot_maximum",
                            "loot_entry_max_count",
                            "loot/apple_tree|entry/apple",
                            "game.lootTables[id=loot/apple_tree].entries[id=entry/apple].maxCount",
                            "2",
                            "3",
                            "harvest_loot_quantity"),
                        Operation(
                            "exploration.apple_tree_log_yield",
                            "resource_node_production_amount",
                            "node/apple_tree|item/log",
                            "game.resourceNodes[id=node/apple_tree].production[id=item/log].amount",
                            "1",
                            "2",
                            "harvest_resource_node_output"),
                        Operation(
                            "exploration.transaction_potion_output",
                            "transaction_output_amount",
                            "transaction/buy_healing_potion|item/healing_potion",
                            "game.transactions[id=transaction/buy_healing_potion].outputs[id=item/healing_potion].amount",
                            "1",
                            "2",
                            "transaction_output")
                    ]
                }
            ]
        };

    private static ProductLineRuntimeVariantMutationOperation Operation(
        string operationId,
        string targetKind,
        string targetId,
        string jsonPath,
        string expectedValue,
        string newValue,
        string runtimeDimension) =>
        new()
        {
            OperationId = operationId,
            TargetKind = targetKind,
            TargetId = targetId,
            JsonPath = jsonPath,
            ExpectedValue = expectedValue,
            NewValue = newValue,
            RuntimeDimension = runtimeDimension
        };
}
