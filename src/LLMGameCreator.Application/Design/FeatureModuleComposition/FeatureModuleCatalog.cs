using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModuleCatalog
{
    public static FeatureModuleCatalogDocument LoadFromGoal142(string repositoryRoot, string goal142Root)
    {
        var catalogPath = Path.Combine(
            repositoryRoot,
            goal142Root.Replace('/', Path.DirectorySeparatorChar),
            ProductLineRuntimeVariantMatrixVocabulary.CatalogFileName);
        var source = JsonSerializer.Deserialize<ProductLineRuntimeVariantCatalogDocument>(
                         File.ReadAllText(catalogPath),
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                     ?? throw new InvalidOperationException("Goal142 runtime-variant catalog could not be read.");
        var modules = RequiredCoreModules().Concat(OptionalProfileModules(source, goal142Root))
            .OrderBy(module => module.ModuleId, StringComparer.Ordinal)
            .ToList();
        return new FeatureModuleCatalogDocument
        {
            RequiredCoreModuleCount = modules.Count(module => module.Required),
            OptionalProfileModuleCount = modules.Count(module => !module.Required && module.Selectable),
            Modules = modules
        };
    }

    public static IReadOnlyList<FeatureModuleDefinition> RequiredCoreModules() =>
    [
        Core("feature.world.grid_navigation", "Grid Navigation", "world", [],
            ["game.maps", "game.tilePrototypes"], ["runtime.command.move"], ["map_references"], ["map_position_state"], ["map_summary"]),
        Core("feature.interaction.basic", "Basic Interaction", "interaction", ["feature.world.grid_navigation"],
            ["game.interactions"], ["runtime.command.interact"], ["interaction_targets"], ["interaction_event_state"], ["interaction_actions"]),
        Core("feature.dialogue.basic", "Basic Dialogue", "dialogue", ["feature.interaction.basic"],
            ["game.dialogues"], ["runtime.command.open_dialogue"], ["dialogue_references"], ["dialogue_state"], ["dialogue_summary"]),
        Core("feature.quest.objective_chain", "Quest Objective Chain", "quest", ["feature.dialogue.basic"],
            ["game.quests"], ["runtime.command.advance_objective"], ["quest_references"], ["quest_progress_state"], ["quest_summary"]),
        Core("feature.inventory.basic", "Basic Inventory", "inventory", [],
            ["game.inventories", "game.items"], ["runtime.command.add_item"], ["inventory_item_references"], ["inventory_stack_state"], ["inventory_summary"]),
        Core("feature.crafting.recipes", "Recipe Crafting", "crafting", ["feature.inventory.basic"],
            ["game.recipes"], ["runtime.command.craft_recipe"], ["recipe_item_references"], ["inventory_stack_state"], ["craft_actions"]),
        Core("feature.resources.harvest", "Resource Harvest", "resources", ["feature.world.grid_navigation", "feature.inventory.basic"],
            ["game.resourceNodes", "game.lootTables"], ["runtime.command.harvest"], ["resource_loot_references"], ["resource_node_state"], ["harvest_actions"]),
        Core("feature.economy.transaction", "Transactions", "economy", ["feature.inventory.basic"],
            ["game.transactions"], ["runtime.command.execute_transaction"], ["transaction_item_references"], ["inventory_stack_state"], ["transaction_actions"]),
        Core("feature.combat.turn_based_encounter", "Turn-Based Encounter", "combat", ["feature.interaction.basic", "feature.inventory.basic"],
            ["game.encounters", "game.abilities"], ["runtime.command.use_ability"], ["encounter_ability_references"], ["encounter_resource_state"], ["combat_summary"]),
        Core("feature.player_adapter.runtime_summary", "Runtime Summary Player Adapter", "player_adapter", ["feature.world.grid_navigation"],
            ["generatedContent.profile"], ["runtime.command.show_final_state"], ["runtime_summary_contract"], ["runtime_state_hash"], ["runtime_summary"])
    ];

    private static IEnumerable<FeatureModuleDefinition> OptionalProfileModules(
        ProductLineRuntimeVariantCatalogDocument source,
        string goal142Root)
    {
        foreach (var recipe in source.Variants.Where(recipe => recipe.RecipeId != "balanced_baseline"))
        {
            var moduleId = "feature.profile." + recipe.RecipeId;
            var dependencies = DependenciesFromOperations(recipe.MutationOperations);
            yield return new FeatureModuleDefinition
            {
                ModuleId = moduleId,
                Title = recipe.DisplayName,
                Category = "profile",
                ModuleKind = "goal142_runtime_profile",
                Selectable = true,
                Dependencies = dependencies,
                RequiredSchemaSections = ["generatedContent.profile"],
                RequiredRuntimePrimitives = ["runtime.qualification.canonical_action_plan"],
                RequiredValidationRules = ["structured_mutation_expected_old_value", "package_cross_reference_validation"],
                RequiredSaveLoadPolicy = ["journal_checkpoint_reload", "full_replay_equivalence"],
                RequiredPlayerAdapterSurface = ["runtime_summary", "semantic_effect_summary"],
                GeneratorInputs = [moduleId],
                AuthoringControls = ["checked_optional_module"],
                GoldenPackages = [recipe.CandidateId],
                SmokePlaythroughs = ["goal145_canonical_13_action_plan"],
                KnownLimitations = ["Goal146 imports fixed Goal142 numeric mutations; parameter authoring is deferred."],
                FutureExpansionNotes = ["Goal147 may add bounded authoring parameters and persistence."],
                MutationOperations = recipe.MutationOperations
                    .OrderBy(operation => operation.OperationId, StringComparer.Ordinal)
                    .ToList(),
                RuntimeEffectContracts = RuntimeEffectContractsFromOperations(moduleId, recipe.MutationOperations),
                SourceLineage = new FeatureModuleSourceLineage
                {
                    GoalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
                    CatalogPath = goal142Root.Replace('\\', '/') + "/" + ProductLineRuntimeVariantMatrixVocabulary.CatalogFileName,
                    RecipeId = recipe.RecipeId,
                    VariantKind = recipe.VariantKind,
                    CandidateId = recipe.CandidateId,
                    OperationIds = recipe.MutationOperations.Select(operation => operation.OperationId)
                        .OrderBy(id => id, StringComparer.Ordinal)
                        .ToList()
                }
            };
        }
    }

    private static IReadOnlyList<string> DependenciesFromOperations(
        IReadOnlyList<ProductLineRuntimeVariantMutationOperation> operations) =>
        operations.SelectMany(operation => operation.TargetKind switch
            {
                "inventory_stack_amount" => new[] { "feature.inventory.basic" },
                "recipe_output_amount" => new[] { "feature.crafting.recipes", "feature.inventory.basic" },
                "encounter_participant_resource_amount" or "ability_power" or "ability_effect_arg_amount" =>
                    new[] { "feature.combat.turn_based_encounter" },
                "loot_entry_min_count" or "loot_entry_max_count" or "resource_node_production_amount" =>
                    new[] { "feature.resources.harvest", "feature.inventory.basic" },
                "transaction_output_amount" => new[] { "feature.economy.transaction", "feature.inventory.basic" },
                _ => Array.Empty<string>()
            })
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<FeatureModuleRuntimeEffectContract> RuntimeEffectContractsFromOperations(
        string moduleId,
        IReadOnlyList<ProductLineRuntimeVariantMutationOperation> operations)
    {
        foreach (var operation in operations)
        {
            var contract = RuntimeEffectContract(moduleId, operation);
            if (contract is not null) return [contract];
        }
        return [];
    }

    private static FeatureModuleRuntimeEffectContract? RuntimeEffectContract(
        string moduleId,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var parts = operation.TargetId.Split('|');
        var metricKind = string.Empty;
        var targetId = string.Empty;
        var resourceOrItemId = string.Empty;
        switch (operation.TargetKind)
        {
            case "inventory_stack_amount" when parts.Length == 2:
                metricKind = FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity;
                targetId = parts[0];
                resourceOrItemId = parts[1];
                break;
            case "recipe_output_amount" or "transaction_output_amount" or "resource_node_production_amount" when parts.Length == 2:
                metricKind = FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity;
                targetId = "inventory/player_start";
                resourceOrItemId = parts[1];
                break;
            case "loot_entry_min_count" or "loot_entry_max_count" when parts.Length == 2:
                metricKind = FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity;
                targetId = "inventory/player_start";
                resourceOrItemId = "item/" + parts[1].Split('/')[^1];
                break;
            case "encounter_participant_resource_amount" when parts.Length == 3:
                metricKind = FeatureModuleRuntimeEffectMetricKinds.CombatResourceAmount;
                targetId = parts[1];
                resourceOrItemId = parts[2];
                break;
            default:
                return null;
        }
        return new FeatureModuleRuntimeEffectContract
        {
            EffectId = "runtime_effect." + moduleId[(moduleId.LastIndexOf('.') + 1)..],
            ModuleId = moduleId,
            MetricKind = metricKind,
            TargetId = targetId,
            ResourceOrItemId = resourceOrItemId,
            ComparisonKind = operation.TargetKind == "encounter_participant_resource_amount"
                ? FeatureModuleRuntimeEffectComparisonKinds.ChangedFromBaseline
                : FeatureModuleRuntimeEffectComparisonKinds.GreaterThanBaseline,
            ExpectedValue = operation.NewValue,
            SourceOperationIds = [operation.OperationId],
            RuntimeDimension = operation.RuntimeDimension
        };
    }

    private static FeatureModuleDefinition Core(
        string id,
        string title,
        string category,
        IReadOnlyList<string> dependencies,
        IReadOnlyList<string> schema,
        IReadOnlyList<string> runtime,
        IReadOnlyList<string> validation,
        IReadOnlyList<string> saveLoad,
        IReadOnlyList<string> player) => new()
    {
        ModuleId = id,
        Title = title,
        Category = category,
        ModuleKind = "required_core",
        Required = true,
        Selectable = false,
        Dependencies = dependencies,
        RequiredSchemaSections = schema,
        RequiredRuntimePrimitives = runtime,
        RequiredValidationRules = validation,
        RequiredSaveLoadPolicy = saveLoad,
        RequiredPlayerAdapterSurface = player,
        GeneratorInputs = [id],
        AuthoringControls = ["locked_core_module"],
        GoldenPackages = [FeatureModuleCompositionVocabulary.BaselineCandidateId],
        SmokePlaythroughs = ["goal145_canonical_13_action_plan"],
        KnownLimitations = ["Goal146 composes the proven minimal-map vertical slice only."],
        FutureExpansionNotes = ["Additional module families attach through the same application-layer contract."],
        SourceLineage = new FeatureModuleSourceLineage
        {
            GoalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
            CandidateId = FeatureModuleCompositionVocabulary.BaselineCandidateId
        }
    };
}
