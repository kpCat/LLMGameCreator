using System.Collections.Generic;
using System.Linq;

namespace LLMGameCreatorAlpha
{
    public sealed class GenericGamePackageProjectionPlaythrough
    {
        private const string SignEntityId = "entity/village/sign";
        private const string GuardEntityId = "entity/village/old_guard";
        private const string SignInteractionId = "interaction/sign_inspect";
        private const string DialogueId = "dialogue/old_guard_intro";
        private const string QuestId = "quest/help_healer";
        private const string RecipeId = "recipe/healing_potion";
        private const string HarvestNodeId = "node/apple_tree";
        private const string TransactionId = "transaction/buy_healing_potion";
        private const string EncounterId = "encounter/goblin_duel";

        public GenericGamePackageProjectionState Run(
            GenericGamePackageProjectionModel model,
            IList<string> verificationEvents)
        {
            var events = new List<string>();
            var state = new GenericGamePackageProjectionState();
            state.SamplePackageLoaded = !string.IsNullOrWhiteSpace(model.PackageId)
                                        && !string.IsNullOrWhiteSpace(model.MapId);
            state.GenericProjectionBuilt = model.MapWidth > 0
                                           && model.MapHeight > 0
                                           && model.Entities.Count > 0;
            state.AppendEvent("packageId=" + model.PackageId);
            state.AppendEvent("packageTitle=" + model.PackageTitle);
            state.AppendEvent("startMapId=" + model.StartMapId);
            state.AppendEvent("mapId=" + model.MapId);
            events.Add("samplePackageLoaded=" + state.SamplePackageLoaded);
            events.Add("genericProjectionBuilt=" + state.GenericProjectionBuilt);
            events.Add("sequenceTargets="
                       + SignEntityId
                       + "|"
                       + SignInteractionId
                       + "|"
                       + DialogueId
                       + "|"
                       + QuestId
                       + "|"
                       + RecipeId
                       + "|"
                       + HarvestNodeId
                       + "|"
                       + TransactionId
                       + "|"
                       + EncounterId);

            state.movementPathSummary = BuildMovementPathSummary(model, out var pathPreviewPresent);
            state.MapPathPreviewPresent = pathPreviewPresent;
            state.AppendEvent("movementPathSummary=" + state.movementPathSummary);

            var loopEvents = new List<string>();
            var loop = new GenericGamePackageProjectionLoop().Run(model, loopEvents);
            CopyLoopState(loop, state);
            state.signInteractionResult =
                "entityId=" + SignEntityId
                + "; interactionId=" + EmptyAsNone(state.selectedInteractionId)
                + "; applied=" + state.InteractionApplyPassed
                + "; flag/sign_inspected="
                + FlagValue(state, "flag/sign_inspected");
            state.SignInteractionApplied = state.InteractionApplyPassed
                                           && state.appliedInteractionCount == 1;
            state.dialogueSummary = BuildDialogueSummary(model, state.selectedDialogueId);
            state.questObjectiveStatus = state.questObjectiveSummary;
            state.QuestObjectiveStatusPresent =
                !string.IsNullOrWhiteSpace(state.questObjectiveStatus);

            var systemsEvents = new List<string>();
            var systems = new GenericGamePackageProjectionSystems().Run(model, systemsEvents);
            CopySystemsState(systems, state);
            state.inventoryResourceFinalSummary =
                "inventory=" + EmptyAsNone(state.inventorySummary)
                + "; resources=" + EmptyAsNone(state.resourceSummary);
            state.systemsSummary =
                "recipe=" + EmptyAsNone(state.recipeApplyResult)
                + "; harvest=" + EmptyAsNone(state.harvestApplyResult)
                + "; transaction=" + EmptyAsNone(state.transactionPreview);
            state.combatSummary =
                EmptyAsNone(state.encounterPreview)
                + "; "
                + EmptyAsNone(state.combatRoundPreview);
            state.SystemsSummaryPresent =
                state.RecipeApplyPassed
                && state.HarvestApplyPassed
                && state.TransactionPreviewPresent;

            foreach (var item in loopEvents)
            {
                events.Add(item);
            }

            foreach (var item in systemsEvents)
            {
                events.Add(item);
            }

            events.Add("mapPathPreviewPresent=" + state.MapPathPreviewPresent);
            events.Add("signInteractionApplied=" + state.SignInteractionApplied);
            events.Add("dialogueSummaryPresent=" + state.DialogueSummaryPresent);
            events.Add("questObjectiveStatusPresent=" + state.QuestObjectiveStatusPresent);
            events.Add("inventorySummaryPresent=" + state.InventorySummaryPresent);
            events.Add("resourceSummaryPresent=" + state.ResourceSummaryPresent);
            events.Add("systemsSummaryPresent=" + state.SystemsSummaryPresent);
            events.Add("combatRoundPreviewPresent=" + state.CombatRoundPreviewPresent);

            state.eventTranscriptSummary =
                "eventCount=" + events.Count
                + "; first=" + (events.Count == 0 ? "none" : events[0])
                + "; last=" + (events.Count == 0 ? "none" : events[events.Count - 1]);
            state.EventTranscriptPresent = events.Count > 0
                                           && !string.IsNullOrWhiteSpace(state.eventTranscriptSummary);
            state.AppendEvent("eventTranscriptSummary=" + state.eventTranscriptSummary);
            state.finalStateSummary =
                "questStatus=" + EmptyAsNone(state.questObjectiveStatus)
                + "; inventoryResourceFinalSummary="
                + EmptyAsNone(state.inventoryResourceFinalSummary)
                + "; combat=" + EmptyAsNone(state.combatRoundPreview);

            state.FullPlaythroughPassed =
                state.SamplePackageLoaded
                && state.GenericProjectionBuilt
                && model.PackageId == "game/minimal-map-game"
                && model.PackageTitle == "Minimal Map Game"
                && model.StartMapId == "map/village"
                && state.MapPathPreviewPresent
                && state.SignInteractionApplied
                && state.DialogueSummaryPresent
                && state.QuestObjectiveStatusPresent
                && state.InventorySummaryPresent
                && state.ResourceSummaryPresent
                && state.RecipeApplyPassed
                && state.HarvestApplyPassed
                && state.TransactionPreviewPresent
                && state.CombatRoundPreviewPresent
                && state.EventTranscriptPresent;
            state.fullPlaythroughStatus =
                state.FullPlaythroughPassed ? "passed" : "not passed";
            state.AppendEvent("fullPlaythroughPassed=" + state.FullPlaythroughPassed);

            if (verificationEvents != null)
            {
                foreach (var item in state.events)
                {
                    verificationEvents.Add("goal126." + item);
                }

                foreach (var item in events)
                {
                    verificationEvents.Add("goal126.transcript." + item);
                }
            }

            return state;
        }

        private static string BuildMovementPathSummary(
            GenericGamePackageProjectionModel model,
            out bool pathPreviewPresent)
        {
            var target = model.Entities.FirstOrDefault(entity =>
                entity.EntityId == SignEntityId)
                ?? model.Entities.FirstOrDefault(entity =>
                    entity.EntityId == GuardEntityId)
                ?? model.Entities.FirstOrDefault();
            if (target == null)
            {
                pathPreviewPresent = false;
                return string.Empty;
            }

            var points = new List<GenericGamePackagePosition>();
            var x = model.StartX;
            var y = model.StartY;
            points.Add(new GenericGamePackagePosition { x = x, y = y });
            while (x != target.X)
            {
                x += target.X > x ? 1 : -1;
                points.Add(new GenericGamePackagePosition { x = x, y = y });
            }

            while (y != target.Y)
            {
                y += target.Y > y ? 1 : -1;
                points.Add(new GenericGamePackagePosition { x = x, y = y });
            }

            var walkable = 0;
            var blocked = 0;
            var tileIds = new List<string>();
            foreach (var point in points)
            {
                var tile = model.Tiles.FirstOrDefault(item =>
                    item.X == point.x && item.Y == point.y);
                if (tile == null || tile.Walkable)
                {
                    walkable++;
                }
                else
                {
                    blocked++;
                }

                tileIds.Add(tile == null ? "unknown" : tile.TileId);
            }

            pathPreviewPresent = points.Count >= 2 && blocked == 0;
            return "start=(" + model.StartX + "," + model.StartY + ")"
                   + "; targetEntityId=" + target.EntityId
                   + "; target=(" + target.X + "," + target.Y + ")"
                   + "; path="
                   + string.Join("->", points
                       .Select(point => "(" + point.x + "," + point.y + ")")
                       .ToArray())
                   + "; walkableSteps=" + walkable
                   + "; blockedSteps=" + blocked
                   + "; tileIds=" + string.Join(",", tileIds.ToArray());
        }

        private static void CopyLoopState(
            GenericGamePackageProjectionState source,
            GenericGamePackageProjectionState target)
        {
            target.selectedEntityId = source.selectedEntityId;
            target.selectedInteractionId = source.selectedInteractionId;
            target.selectedDialogueId = source.selectedDialogueId;
            target.selectedQuestId = source.selectedQuestId;
            target.interactionEffectPreview = source.interactionEffectPreview;
            target.questObjectiveSummary = source.questObjectiveSummary;
            target.appliedInteractionCount = source.appliedInteractionCount;
            target.startedQuestCount = source.startedQuestCount;
            target.InteractionPreviewPresent = source.InteractionPreviewPresent;
            target.InteractionApplyPassed = source.InteractionApplyPassed;
            target.DialogueSummaryPresent = source.DialogueSummaryPresent;
            target.QuestObjectiveSummaryPresent = source.QuestObjectiveSummaryPresent;
            foreach (var item in source.projectionFlags)
            {
                target.projectionFlags[item.Key] = item.Value;
            }
        }

        private static void CopySystemsState(
            GenericGamePackageProjectionState source,
            GenericGamePackageProjectionState target)
        {
            target.inventorySummary = source.inventorySummary;
            target.resourceSummary = source.resourceSummary;
            target.recipePreview = source.recipePreview;
            target.recipeApplyResult = source.recipeApplyResult;
            target.harvestPreview = source.harvestPreview;
            target.harvestApplyResult = source.harvestApplyResult;
            target.transactionPreview = source.transactionPreview;
            target.encounterPreview = source.encounterPreview;
            target.combatRoundPreview = source.combatRoundPreview;
            target.systemsEventLog = source.systemsEventLog;
            target.InventoryInitialized = source.InventoryInitialized;
            target.ResourcesInitialized = source.ResourcesInitialized;
            target.InventorySummaryPresent = source.InventorySummaryPresent;
            target.ResourceSummaryPresent = source.ResourceSummaryPresent;
            target.RecipePreviewPresent = source.RecipePreviewPresent;
            target.RecipeApplyPassed = source.RecipeApplyPassed;
            target.HarvestPreviewPresent = source.HarvestPreviewPresent;
            target.HarvestApplyPassed = source.HarvestApplyPassed;
            target.TransactionPreviewPresent = source.TransactionPreviewPresent;
            target.EncounterPreviewPresent = source.EncounterPreviewPresent;
            target.CombatRoundPreviewPresent = source.CombatRoundPreviewPresent;
            target.SystemsEventLogPresent = source.SystemsEventLogPresent;
            foreach (var item in source.playerInventory)
            {
                target.playerInventory[item.Key] = item.Value;
            }

            foreach (var item in source.resourceLedger)
            {
                target.resourceLedger[item.Key] = item.Value;
            }

            foreach (var item in source.itemDurability)
            {
                target.itemDurability[item.Key] = item.Value;
            }
        }

        private static string BuildDialogueSummary(
            GenericGamePackageProjectionModel model,
            string dialogueId)
        {
            var dialogue = model.Dialogues.FirstOrDefault(item =>
                item.DialogueId == dialogueId)
                ?? model.Dialogues.FirstOrDefault(item =>
                    item.DialogueId == DialogueId)
                ?? model.Dialogues.FirstOrDefault();
            if (dialogue == null)
            {
                return string.Empty;
            }

            return "dialogueId=" + dialogue.DialogueId
                   + "; startNodeId=" + dialogue.StartNodeId
                   + "; speakerId=" + dialogue.StartSpeakerId
                   + "; text=" + dialogue.StartText;
        }

        private static string FlagValue(
            GenericGamePackageProjectionState state,
            string flagId)
        {
            string value;
            return state.projectionFlags.TryGetValue(flagId, out value) ? value : "none";
        }

        private static string EmptyAsNone(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "none" : value;
        }
    }
}
