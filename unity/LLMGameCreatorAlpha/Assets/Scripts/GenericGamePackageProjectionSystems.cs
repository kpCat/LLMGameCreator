using System.Collections.Generic;
using System.Linq;

namespace LLMGameCreatorAlpha
{
    public sealed class GenericGamePackageProjectionSystems
    {
        public GenericGamePackageProjectionState Run(
            GenericGamePackageProjectionModel model,
            IList<string> verificationEvents)
        {
            var state = new GenericGamePackageProjectionState();
            state.SamplePackageLoaded = !string.IsNullOrWhiteSpace(model.PackageId)
                                        && !string.IsNullOrWhiteSpace(model.MapId);
            state.GenericProjectionBuilt = model.MapWidth > 0
                                           && model.MapHeight > 0
                                           && model.Entities.Count > 0;
            state.AppendSystemsEvent("samplePackageLoaded=" + state.SamplePackageLoaded);
            state.AppendSystemsEvent("genericProjectionBuilt=" + state.GenericProjectionBuilt);

            InitializeInventory(model, state);
            InitializeResources(model, state);
            PreviewAndApplyRecipe(model, state);
            PreviewAndApplyHarvest(model, state);
            PreviewTransaction(model, state);
            PreviewEncounter(model, state);
            UpdateSummaries(state);

            state.GenericSystemsPassed =
                state.SamplePackageLoaded
                && state.GenericProjectionBuilt
                && state.InventoryInitialized
                && state.ResourcesInitialized
                && state.RecipePreviewPresent
                && state.RecipeApplyPassed
                && state.HarvestPreviewPresent
                && state.HarvestApplyPassed
                && state.TransactionPreviewPresent
                && state.EncounterPreviewPresent
                && state.CombatRoundPreviewPresent
                && state.SystemsEventLogPresent;
            state.AppendSystemsEvent("genericSystemsPassed=" + state.GenericSystemsPassed);

            if (verificationEvents != null)
            {
                foreach (var item in state.systemsEvents)
                {
                    verificationEvents.Add("goal125." + item);
                }
            }

            return state;
        }

        private static void InitializeInventory(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionState state)
        {
            var inventory = model.Inventories.FirstOrDefault(item =>
                item.InventoryId == "inventory/player_start")
                ?? model.Inventories.FirstOrDefault(item => item.OwnerKind == "player")
                ?? model.Inventories.FirstOrDefault();
            if (inventory == null)
            {
                state.AppendSystemsEvent("inventoryInitialized=false");
                return;
            }

            foreach (var stack in inventory.Stacks)
            {
                Add(state.playerInventory, stack.ItemId, stack.Amount);
                if (stack.Durability > 0)
                {
                    state.itemDurability[stack.ItemId] = stack.Durability;
                }
            }

            state.InventoryInitialized =
                Quantity(state.playerInventory, "item/red_herb") == 2
                && Quantity(state.playerInventory, "item/water_flask") == 1
                && Quantity(state.playerInventory, "item/woodcutting_axe") >= 1;
            state.AppendSystemsEvent("inventoryInitialized=" + state.InventoryInitialized);
        }

        private static void InitializeResources(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionState state)
        {
            foreach (var resource in model.Resources)
            {
                var value = resource.DefaultValue != 0 ? resource.DefaultValue : resource.MinValue;
                state.resourceLedger[resource.ResourceId] = value;
            }

            state.ResourcesInitialized =
                Quantity(state.resourceLedger, "resource/health") == 30
                && Quantity(state.resourceLedger, "resource/stamina") == 10
                && Quantity(state.resourceLedger, "resource/mana") == 10
                && state.resourceLedger.ContainsKey("resource/gold");
            state.AppendSystemsEvent("resourcesInitialized=" + state.ResourcesInitialized);
        }

        private static void PreviewAndApplyRecipe(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionState state)
        {
            var recipe = model.Recipes.FirstOrDefault(item =>
                item.RecipeId == "recipe/healing_potion");
            if (recipe == null)
            {
                return;
            }

            var requirementsMet =
                AmountsAvailable(state.playerInventory, recipe.Inputs.Where(IsItem))
                && AmountsAvailable(state.resourceLedger, recipe.Costs.Where(IsResource));
            state.recipePreview =
                "recipeId=" + recipe.RecipeId
                + "; inputs=" + AmountSummary(recipe.Inputs)
                + "; costs=" + AmountSummary(recipe.Costs)
                + "; outputs=" + AmountSummary(recipe.Outputs)
                + "; requirementsMet=" + requirementsMet;
            state.RecipePreviewPresent = true;
            state.AppendSystemsEvent("recipePreview=" + state.recipePreview);

            if (requirementsMet)
            {
                ApplyAmounts(state.playerInventory, recipe.Inputs.Where(IsItem), -1);
                ApplyAmounts(state.resourceLedger, recipe.Costs.Where(IsResource), -1);
                ApplyAmounts(state.playerInventory, recipe.Outputs.Where(IsItem), 1);
            }

            state.RecipeApplyPassed =
                requirementsMet
                && Quantity(state.playerInventory, "item/red_herb") == 0
                && Quantity(state.playerInventory, "item/water_flask") == 0
                && Quantity(state.resourceLedger, "resource/mana") == 5
                && Quantity(state.playerInventory, "item/healing_potion") >= 2;
            state.recipeApplyResult =
                "recipeId=" + recipe.RecipeId
                + "; applied=" + state.RecipeApplyPassed
                + "; redHerb=" + Quantity(state.playerInventory, "item/red_herb")
                + "; waterFlask=" + Quantity(state.playerInventory, "item/water_flask")
                + "; mana=" + Quantity(state.resourceLedger, "resource/mana")
                + "; healingPotion=" + Quantity(state.playerInventory, "item/healing_potion");
            state.AppendSystemsEvent("recipeApplyResult=" + state.recipeApplyResult);
        }

        private static void PreviewAndApplyHarvest(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionState state)
        {
            var node = model.ResourceNodes.FirstOrDefault(item =>
                item.ResourceNodeId == "node/apple_tree");
            if (node == null)
            {
                return;
            }

            var toolItemId = ResolveRequiredToolItemId(model, node);
            var hasTool = Quantity(state.playerInventory, toolItemId) > 0;
            var lootTableId = Metadata(node.Metadata, "harvest_loot_table_id");
            var loot = model.LootTables.FirstOrDefault(item => item.LootTableId == lootTableId);
            state.harvestPreview =
                "resourceNodeId=" + node.ResourceNodeId
                + "; requiredToolItemId=" + toolItemId
                + "; hasTool=" + hasTool
                + "; production=" + AmountSummary(node.Production)
                + "; lootTableId=" + lootTableId;
            state.HarvestPreviewPresent = true;
            state.AppendSystemsEvent("harvestPreview=" + state.harvestPreview);

            if (hasTool)
            {
                ApplyAmounts(state.playerInventory, node.Production.Where(IsItem), 1);
                ApplyDeterministicLoot(state, loot);
                DecrementDurability(state, toolItemId, IntMetadata(node.Metadata, "durability_cost"));
            }

            state.HarvestApplyPassed =
                hasTool
                && Quantity(state.playerInventory, "item/log") >= 1
                && Quantity(state.playerInventory, "item/apple") >= 1
                && (!state.itemDurability.ContainsKey(toolItemId)
                    || state.itemDurability[toolItemId] == 9);
            state.harvestApplyResult =
                "resourceNodeId=" + node.ResourceNodeId
                + "; applied=" + state.HarvestApplyPassed
                + "; log=" + Quantity(state.playerInventory, "item/log")
                + "; apple=" + Quantity(state.playerInventory, "item/apple")
                + "; toolDurability=" + Quantity(state.itemDurability, toolItemId);
            state.AppendSystemsEvent("harvestApplyResult=" + state.harvestApplyResult);
        }

        private static void PreviewTransaction(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionState state)
        {
            var transaction = model.Transactions.FirstOrDefault(item =>
                item.TransactionId == "transaction/buy_healing_potion");
            if (transaction == null)
            {
                return;
            }

            var affordable = AmountsAvailable(state.resourceLedger, transaction.Costs.Where(IsResource))
                             && AmountsAvailable(state.playerInventory, transaction.Costs.Where(IsItem));
            state.TransactionPreviewPresent = true;
            state.transactionPreview =
                "transactionId=" + transaction.TransactionId
                + "; costs=" + AmountSummary(transaction.Costs)
                + "; outputs=" + AmountSummary(transaction.Outputs)
                + "; affordable=" + affordable
                + "; gold=" + Quantity(state.resourceLedger, "resource/gold");
            state.AppendSystemsEvent("transactionPreview=" + state.transactionPreview);
        }

        private static void PreviewEncounter(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionState state)
        {
            var encounter = model.Encounters.FirstOrDefault(item =>
                item.EncounterId == "encounter/goblin_duel");
            if (encounter == null)
            {
                return;
            }

            var player = encounter.Participants.FirstOrDefault(item => item.ParticipantId == "player");
            var goblin = encounter.Participants.FirstOrDefault(item => item.ParticipantId == "goblin");
            var playerAttack = model.Abilities.FirstOrDefault(item => item.AbilityId == "ability/basic_attack");
            var goblinSlash = model.Abilities.FirstOrDefault(item => item.AbilityId == "ability/goblin_slash");
            var playerHealth = ParticipantResource(player, "resource/health");
            var goblinHealth = ParticipantResource(goblin, "resource/health");
            var playerDamage = playerAttack == null ? 0 : playerAttack.Power;
            var goblinDamage = goblinSlash == null ? 0 : goblinSlash.Power;
            var goblinHealthAfter = goblinHealth - playerDamage;
            var playerHealthAfter = playerHealth - goblinDamage;

            state.EncounterPreviewPresent = player != null && goblin != null;
            state.CombatRoundPreviewPresent =
                playerHealth == 30
                && goblinHealth == 12
                && playerDamage == 4
                && goblinDamage == 3
                && goblinHealthAfter == 8
                && playerHealthAfter == 27;
            state.encounterPreview =
                "encounterId=" + encounter.EncounterId
                + "; playerHealth=" + playerHealth
                + "; goblinHealth=" + goblinHealth
                + "; playerAttackDamage=" + playerDamage
                + "; goblinSlashDamage=" + goblinDamage;
            state.combatRoundPreview =
                "encounterId=" + encounter.EncounterId
                + "; goblinHealthAfter=" + goblinHealthAfter
                + "; playerHealthAfter=" + playerHealthAfter;
            state.AppendSystemsEvent("encounterPreview=" + state.encounterPreview);
            state.AppendSystemsEvent("combatRoundPreview=" + state.combatRoundPreview);
        }

        private static void UpdateSummaries(GenericGamePackageProjectionState state)
        {
            state.inventorySummary = "playerInventory="
                                     + string.Join(", ", state.playerInventory
                                         .OrderBy(item => item.Key, System.StringComparer.Ordinal)
                                         .Select(item => item.Key + "x" + item.Value)
                                         .ToArray())
                                     + "; durability="
                                     + string.Join(", ", state.itemDurability
                                         .OrderBy(item => item.Key, System.StringComparer.Ordinal)
                                         .Select(item => item.Key + "=" + item.Value)
                                         .ToArray());
            state.InventorySummaryPresent = !string.IsNullOrWhiteSpace(state.inventorySummary);
            state.resourceSummary = "resourceLedger="
                                    + string.Join(", ", state.resourceLedger
                                        .OrderBy(item => item.Key, System.StringComparer.Ordinal)
                                        .Select(item => item.Key + "=" + item.Value)
                                        .ToArray());
            state.ResourceSummaryPresent = !string.IsNullOrWhiteSpace(state.resourceSummary);
            state.AppendSystemsEvent("inventorySummary=" + state.inventorySummary);
            state.AppendSystemsEvent("resourceLedgerSummary=" + state.resourceSummary);
        }

        private static string ResolveRequiredToolItemId(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionResourceNode node)
        {
            var explicitTool = Metadata(node.Metadata, "required_tool_item_id");
            if (!string.IsNullOrWhiteSpace(explicitTool))
            {
                return explicitTool;
            }

            var tag = Metadata(node.Metadata, "required_tool_tag");
            var item = model.Items.FirstOrDefault(candidate =>
                candidate.Tags.Any(itemTag => itemTag == tag));
            return item == null ? string.Empty : item.ItemId;
        }

        private static void ApplyDeterministicLoot(
            GenericGamePackageProjectionState state,
            GenericGamePackageProjectionLootTable loot)
        {
            if (loot == null)
            {
                return;
            }

            var entry = loot.Entries.FirstOrDefault();
            if (entry == null || !IsItem(entry.Output))
            {
                return;
            }

            var count = entry.MinCount > 0 ? entry.MinCount : entry.Output.Amount;
            Add(state.playerInventory, entry.Output.Id, count);
        }

        private static void DecrementDurability(
            GenericGamePackageProjectionState state,
            string itemId,
            int cost)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || cost <= 0
                || !state.itemDurability.ContainsKey(itemId))
            {
                return;
            }

            state.itemDurability[itemId] = state.itemDurability[itemId] - cost;
        }

        private static int ParticipantResource(
            GenericGamePackageProjectionEncounterParticipant participant,
            string resourceId)
        {
            if (participant == null)
            {
                return 0;
            }

            var value = participant.Resources.FirstOrDefault(item => item.Id == resourceId);
            return value == null ? 0 : value.Amount;
        }

        private static bool AmountsAvailable(
            Dictionary<string, int> ledger,
            IEnumerable<GenericGamePackageProjectionAmount> amounts) =>
            amounts.All(amount => Quantity(ledger, amount.Id) >= amount.Amount);

        private static void ApplyAmounts(
            Dictionary<string, int> ledger,
            IEnumerable<GenericGamePackageProjectionAmount> amounts,
            int sign)
        {
            foreach (var amount in amounts)
            {
                Add(ledger, amount.Id, amount.Amount * sign);
            }
        }

        private static void Add(Dictionary<string, int> ledger, string id, int amount)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            ledger[id] = Quantity(ledger, id) + amount;
        }

        private static int Quantity(Dictionary<string, int> ledger, string id)
        {
            int value;
            return !string.IsNullOrWhiteSpace(id) && ledger.TryGetValue(id, out value) ? value : 0;
        }

        private static bool IsItem(GenericGamePackageProjectionAmount amount) =>
            amount != null && amount.Kind == "item";

        private static bool IsResource(GenericGamePackageProjectionAmount amount) =>
            amount != null && amount.Kind == "resource";

        private static string AmountSummary(IEnumerable<GenericGamePackageProjectionAmount> amounts) =>
            string.Join(", ", amounts
                .Select(amount => amount.Kind + ":" + amount.Id + "x" + amount.Amount)
                .ToArray());

        private static string Metadata(Dictionary<string, string> metadata, string key)
        {
            string value;
            return metadata != null && metadata.TryGetValue(key, out value) ? value : string.Empty;
        }

        private static int IntMetadata(Dictionary<string, string> metadata, string key)
        {
            int value;
            return int.TryParse(Metadata(metadata, key), out value) ? value : 0;
        }
    }
}
