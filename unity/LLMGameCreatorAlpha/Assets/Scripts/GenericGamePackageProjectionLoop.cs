using System.Collections.Generic;
using System.Linq;

namespace LLMGameCreatorAlpha
{
    public sealed class GenericGamePackageProjectionLoop
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
            state.AppendEvent("samplePackageLoaded=" + state.SamplePackageLoaded);
            state.AppendEvent("genericProjectionBuilt=" + state.GenericProjectionBuilt);

            var sign = model.Entities.FirstOrDefault(entity =>
                entity.EntityId == "entity/village/sign")
                ?? model.Entities.FirstOrDefault(entity => !string.IsNullOrWhiteSpace(entity.InteractionId))
                ?? model.Entities.FirstOrDefault(entity => entity.Interactable);
            if (sign != null)
            {
                state.selectedEntityId = sign.EntityId;
                state.selectedInteractionId = string.IsNullOrWhiteSpace(sign.InteractionId)
                    ? "interaction/sign_inspect"
                    : sign.InteractionId;
                state.AppendEvent("selectedEntityId=" + state.selectedEntityId);
                state.AppendEvent("selectedInteractionId=" + state.selectedInteractionId);
            }

            var interaction = model.Interactions.FirstOrDefault(item =>
                item.InteractionId == state.selectedInteractionId)
                ?? model.Interactions.FirstOrDefault(item => item.Kind == "inspect")
                ?? model.Interactions.FirstOrDefault();
            PreviewAndApplyInteraction(state, interaction);

            var guard = model.Entities.FirstOrDefault(entity =>
                entity.EntityId == "entity/village/old_guard")
                ?? model.Entities.FirstOrDefault(entity => !string.IsNullOrWhiteSpace(entity.DialogueId));
            if (guard != null)
            {
                state.selectedEntityId = guard.EntityId;
                state.selectedDialogueId = string.IsNullOrWhiteSpace(guard.DialogueId)
                    ? "dialogue/old_guard_intro"
                    : guard.DialogueId;
                state.AppendEvent("selectedDialogueEntityId=" + state.selectedEntityId);
                state.AppendEvent("selectedDialogueId=" + state.selectedDialogueId);
            }

            var dialogue = model.Dialogues.FirstOrDefault(item =>
                item.DialogueId == state.selectedDialogueId)
                ?? model.Dialogues.FirstOrDefault(item => item.DialogueId == "dialogue/old_guard_intro")
                ?? model.Dialogues.FirstOrDefault();
            state.DialogueSummaryPresent = dialogue != null;
            if (dialogue != null)
            {
                state.AppendEvent("dialogueSummary=" + DialogueSummary(dialogue));
            }

            var quest = model.Quests.FirstOrDefault(item => item.QuestId == "quest/help_healer")
                ?? model.Quests.FirstOrDefault();
            if (quest != null)
            {
                state.selectedQuestId = quest.QuestId;
                state.startedQuestCount = 1;
                state.questObjectiveSummary = BuildQuestObjectiveSummary(model, quest);
                state.QuestObjectiveSummaryPresent = !string.IsNullOrWhiteSpace(state.questObjectiveSummary);
                state.AppendEvent("selectedQuestId=" + state.selectedQuestId);
                state.AppendEvent("questObjectiveSummary=" + state.questObjectiveSummary);
            }

            state.inventorySummary = BuildInventorySummary(model);
            state.InventorySummaryPresent = !string.IsNullOrWhiteSpace(state.inventorySummary);
            state.AppendEvent("inventorySummary=" + state.inventorySummary);

            state.resourceSummary = BuildResourceSummary(model);
            state.ResourceSummaryPresent = !string.IsNullOrWhiteSpace(state.resourceSummary);
            state.AppendEvent("resourceSummary=" + state.resourceSummary);

            state.GenericProjectionBuilt = state.GenericProjectionBuilt
                                           && state.InteractionPreviewPresent
                                           && state.InteractionApplyPassed
                                           && state.DialogueSummaryPresent
                                           && state.QuestObjectiveSummaryPresent
                                           && state.InventorySummaryPresent
                                           && state.ResourceSummaryPresent;
            state.AppendEvent("genericLoopPassed=" + state.GenericProjectionBuilt);

            if (verificationEvents != null)
            {
                foreach (var item in state.events)
                {
                    verificationEvents.Add("goal124." + item);
                }
            }

            return state;
        }

        private static void PreviewAndApplyInteraction(
            GenericGamePackageProjectionState state,
            GenericGamePackageProjectionInteraction interaction)
        {
            if (interaction == null)
            {
                state.interactionEffectPreview = string.Empty;
                return;
            }

            state.selectedInteractionId = interaction.InteractionId;
            state.interactionEffectPreview =
                "interactionId=" + interaction.InteractionId
                + "; kind=" + interaction.Kind
                + "; effects=" + string.Join(", ", interaction.Effects.Select(EffectSummary).ToArray());
            state.InteractionPreviewPresent = interaction.Effects.Count > 0;
            state.AppendEvent("interactionPreview=" + state.interactionEffectPreview);

            foreach (var effect in interaction.Effects)
            {
                if (effect.Type == "set_flag")
                {
                    state.projectionFlags[effect.Id] = string.IsNullOrWhiteSpace(effect.Value)
                        ? "true"
                        : effect.Value;
                    state.AppendEvent("projectionFlag." + effect.Id + "=" + state.projectionFlags[effect.Id]);
                }

                if (effect.Type == "log" && !string.IsNullOrWhiteSpace(effect.Message))
                {
                    state.AppendEvent("interactionLog=" + effect.Message);
                }
            }

            state.appliedInteractionCount++;
            state.InteractionApplyPassed = state.projectionFlags.ContainsKey("flag/sign_inspected")
                                           && state.appliedInteractionCount == 1;
            state.AppendEvent("appliedInteractionCount=" + state.appliedInteractionCount);
            state.AppendEvent("interactionApplyPassed=" + state.InteractionApplyPassed);
        }

        private static string BuildQuestObjectiveSummary(
            GenericGamePackageProjectionModel model,
            GenericGamePackageProjectionQuest quest)
        {
            var objective = quest.Objectives.FirstOrDefault(item => item.TargetId == "item/red_herb")
                ?? quest.Objectives.FirstOrDefault();
            if (objective == null)
            {
                return string.Empty;
            }

            var inventory = model.Inventories.FirstOrDefault(item => item.InventoryId == "inventory/player_start")
                ?? model.Inventories.FirstOrDefault(item => item.OwnerKind == "player")
                ?? model.Inventories.FirstOrDefault();
            var have = inventory == null
                ? 0
                : inventory.Stacks
                    .Where(stack => stack.ItemId == objective.TargetId)
                    .Sum(stack => stack.Amount);
            var complete = have >= objective.RequiredAmount;
            return "questId=" + quest.QuestId
                   + "; objectiveId=" + objective.ObjectiveId
                   + "; targetId=" + objective.TargetId
                   + "; required=" + objective.RequiredAmount
                   + "; inventoryHas=" + have
                   + "; status=" + (complete ? "complete" : "incomplete");
        }

        private static string BuildInventorySummary(GenericGamePackageProjectionModel model)
        {
            var inventory = model.Inventories.FirstOrDefault(item => item.InventoryId == "inventory/player_start")
                ?? model.Inventories.FirstOrDefault(item => item.OwnerKind == "player")
                ?? model.Inventories.FirstOrDefault();
            if (inventory == null)
            {
                return string.Empty;
            }

            return "inventoryId=" + inventory.InventoryId
                   + "; slots=" + inventory.Slots
                   + "; stacks="
                   + string.Join(", ", inventory.Stacks
                       .Select(stack => stack.ItemId + "x" + stack.Amount)
                       .ToArray());
        }

        private static string BuildResourceSummary(GenericGamePackageProjectionModel model)
        {
            if (model.Resources.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", model.Resources
                .Select(resource =>
                    resource.ResourceId
                    + "("
                    + resource.Kind
                    + ") default="
                    + resource.DefaultValue
                    + " range="
                    + resource.MinValue
                    + "-"
                    + resource.MaxValue)
                .ToArray());
        }

        private static string DialogueSummary(GenericGamePackageProjectionDialogue dialogue)
        {
            return "dialogueId=" + dialogue.DialogueId
                   + "; title=" + dialogue.Title
                   + "; startNodeId=" + dialogue.StartNodeId
                   + "; speakerId=" + dialogue.StartSpeakerId
                   + "; text=" + dialogue.StartText;
        }

        private static string EffectSummary(GenericGamePackageProjectionEffect effect)
        {
            return effect.Type
                   + ":"
                   + effect.Id
                   + "="
                   + (string.IsNullOrWhiteSpace(effect.Value) ? effect.Message : effect.Value);
        }
    }
}
