using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class NarrativeDefinitionValidator : IGamePackageValidationRule
{
    private const string Category = "Narrative";

    public void Validate(ValidationContext context, ValidationReport report)
    {
        CheckIds(report, context.Package.Game.Quests.Select(quest => quest.Id), "quest");
        CheckIds(report, context.Package.Game.Dialogues.Select(dialogue => dialogue.Id), "dialogue");
        CheckIds(report, context.Package.Game.Factions.Select(faction => faction.Id), "faction");
        ValidateQuests(context, report);
        ValidateDialogues(context, report);
        ValidateFactions(context, report);
    }

    private static void ValidateQuests(ValidationContext context, ValidationReport report)
    {
        foreach (var quest in context.Package.Game.Quests)
        {
            RequireText(report, quest.Id, "quest.id.empty", "Quest id is required.", quest.Id);
            RequireText(report, quest.Title, "quest.title.empty", "Quest title is required.", quest.Id);
            CheckIds(report, quest.Stages.Select(stage => stage.Id), "quest.stage", quest.Id);
            CheckIds(report, quest.Objectives.Select(objective => objective.Id).Concat(quest.Stages.SelectMany(stage => stage.Objectives.Select(objective => objective.Id))), "quest.objective", quest.Id);
            ValidateRequirements(context, report, quest.StartConditions, quest.Id);
            ValidateOutputs(context, report, quest.StartEffects, quest.Id, "quest.start_effect");
            ValidateRequirements(context, report, quest.FailureConditions, quest.Id);
            ValidateOutputs(context, report, quest.FailureEffects, quest.Id, "quest.failure_effect");
            ValidateOutputs(context, report, quest.Rewards, quest.Id, "quest.reward");

            foreach (var objective in quest.Objectives)
            {
                ValidateObjective(context, report, objective, quest.Id);
            }

            foreach (var stage in quest.Stages)
            {
                RequireText(report, stage.Id, "quest.stage.id.empty", "Quest stage id is required.", quest.Id);
                if (!string.IsNullOrWhiteSpace(stage.NextStageId) && !quest.Stages.Any(candidate => IdEquals(candidate.Id, stage.NextStageId)))
                {
                    Add(report, "quest.stage.next_missing", ValidationSeverity.Error, "Quest stage next_stage_id references a missing stage.", quest.Id);
                }

                ValidateOutputs(context, report, stage.EnterEffects, quest.Id, "quest.stage.enter_effect");
                ValidateOutputs(context, report, stage.CompleteEffects, quest.Id, "quest.stage.complete_effect");
                ValidateOutputs(context, report, stage.Rewards, quest.Id, "quest.stage.reward");
                foreach (var objective in stage.Objectives)
                {
                    ValidateObjective(context, report, objective, quest.Id);
                }
            }
        }
    }

    private static void ValidateDialogues(ValidationContext context, ValidationReport report)
    {
        foreach (var dialogue in context.Package.Game.Dialogues)
        {
            RequireText(report, dialogue.Id, "dialogue.id.empty", "Dialogue id is required.", dialogue.Id);
            CheckIds(report, dialogue.Nodes.Select(node => node.Id), "dialogue.node", dialogue.Id);
            if (string.IsNullOrWhiteSpace(dialogue.StartNodeId) || !dialogue.Nodes.Any(node => IdEquals(node.Id, dialogue.StartNodeId)))
            {
                Add(report, "dialogue.start_node_missing", ValidationSeverity.Error, "Dialogue start_node_id references a missing node.", dialogue.Id);
            }

            ValidateRequirements(context, report, dialogue.Conditions, dialogue.Id);
            ValidateOutputs(context, report, dialogue.EnterEffects, dialogue.Id, "dialogue.enter_effect");
            ValidateOutputs(context, report, dialogue.ExitEffects, dialogue.Id, "dialogue.exit_effect");

            foreach (var node in dialogue.Nodes)
            {
                RequireText(report, node.Id, "dialogue.node.id.empty", "Dialogue node id is required.", dialogue.Id);
                ValidateRequirements(context, report, node.Conditions, dialogue.Id);
                ValidateOutputs(context, report, node.EnterEffects, dialogue.Id, "dialogue.node.enter_effect");
                ValidateOutputs(context, report, node.ExitEffects, dialogue.Id, "dialogue.node.exit_effect");
                CheckIds(report, node.Choices.Select(choice => choice.Id), "dialogue.choice", dialogue.Id);
                foreach (var choice in node.Choices)
                {
                    ValidateChoice(context, report, dialogue, choice);
                }
            }
        }
    }

    private static void ValidateChoice(ValidationContext context, ValidationReport report, DialogueDefinition dialogue, DialogueChoiceDefinition choice)
    {
        RequireText(report, choice.Id, "dialogue.choice.id.empty", "Dialogue choice id is required.", dialogue.Id);
        if (!choice.CloseDialogue && !string.IsNullOrWhiteSpace(choice.TargetNodeId) && !dialogue.Nodes.Any(node => IdEquals(node.Id, choice.TargetNodeId)))
        {
            Add(report, "dialogue.choice.target_missing", ValidationSeverity.Error, "Dialogue choice target_node_id references a missing node.", dialogue.Id);
        }

        if (!string.IsNullOrWhiteSpace(choice.StartQuestId) && !context.QuestIds.Contains(choice.StartQuestId))
        {
            Add(report, "dialogue.choice.quest_missing", ValidationSeverity.Error, "Dialogue choice start_quest_id references a missing quest.", dialogue.Id);
        }

        if (!string.IsNullOrWhiteSpace(choice.AdvanceQuestId) && !context.QuestIds.Contains(choice.AdvanceQuestId))
        {
            Add(report, "dialogue.choice.quest_missing", ValidationSeverity.Error, "Dialogue choice advance_quest_id references a missing quest.", dialogue.Id);
        }

        if (!string.IsNullOrWhiteSpace(choice.OpenTransactionId) && !context.TransactionIds.Contains(choice.OpenTransactionId))
        {
            Add(report, "dialogue.choice.transaction_missing", ValidationSeverity.Error, "Dialogue choice open_transaction_id references a missing transaction.", dialogue.Id);
        }

        if (!string.IsNullOrWhiteSpace(choice.StartEncounterId) && !context.Package.Game.Encounters.Any(encounter => IdEquals(encounter.Id, choice.StartEncounterId)))
        {
            Add(report, "dialogue.choice.encounter_missing", ValidationSeverity.Error, "Dialogue choice start_encounter_id references a missing encounter.", dialogue.Id);
        }

        ValidateRequirements(context, report, choice.Requirements, dialogue.Id);
        ValidateRequirements(context, report, choice.Conditions.Select(RuntimeLikeRequirement), dialogue.Id);
        ValidateCosts(context, report, choice.Costs, dialogue.Id, "dialogue.choice.cost");
        ValidateOutputs(context, report, choice.Effects.Select(RuntimeLikeOutput).Concat(choice.Rewards), dialogue.Id, "dialogue.choice.output");
    }

    private static void ValidateFactions(ValidationContext context, ValidationReport report)
    {
        foreach (var faction in context.Package.Game.Factions)
        {
            RequireText(report, faction.Id, "faction.id.empty", "Faction id is required.", faction.Id);
            RequireText(report, faction.Name, "faction.name.empty", "Faction name is required.", faction.Id);
            if (faction.MinReputation.HasValue && faction.MaxReputation.HasValue && faction.MinReputation.Value > faction.MaxReputation.Value)
            {
                Add(report, "faction.reputation.range_invalid", ValidationSeverity.Error, "Faction min_reputation must be less than or equal to max_reputation.", faction.Id);
            }

            if (faction.DefaultReputation.HasValue)
            {
                if (faction.MinReputation.HasValue && faction.DefaultReputation.Value < faction.MinReputation.Value
                    || faction.MaxReputation.HasValue && faction.DefaultReputation.Value > faction.MaxReputation.Value)
                {
                    Add(report, "faction.reputation.default_invalid", ValidationSeverity.Error, "Faction default_reputation must be inside min/max bounds.", faction.Id);
                }
            }

            foreach (var duplicate in faction.Relations.Where(relation => !string.IsNullOrWhiteSpace(relation.FactionId)).GroupBy(relation => relation.FactionId).Where(group => group.Count() > 1))
            {
                Add(report, "faction.relation.duplicate", ValidationSeverity.Warning, $"Duplicate faction relation target: {duplicate.Key}", faction.Id);
            }

            foreach (var relation in faction.Relations)
            {
                RequireText(report, relation.FactionId, "faction.relation.target_empty", "Faction relation target is required.", faction.Id);
                if (!string.IsNullOrWhiteSpace(relation.FactionId) && !context.FactionIds.Contains(relation.FactionId))
                {
                    Add(report, "faction.relation.target_missing", ValidationSeverity.Error, "Faction relation references a missing faction.", faction.Id);
                }
            }
        }
    }

    private static void ValidateObjective(ValidationContext context, ValidationReport report, QuestObjectiveDefinition objective, string ownerId)
    {
        RequireText(report, objective.Id, "quest.objective.id.empty", "Quest objective id is required.", ownerId);
        RequireText(report, objective.Kind, "quest.objective.kind.empty", "Quest objective kind is required.", ownerId);
        if (objective.RequiredAmount <= 0)
        {
            Add(report, "quest.objective.required_amount.invalid", ValidationSeverity.Error, "Quest objective required_amount must be positive.", ownerId);
        }

        if ((objective.Kind.Equals("collect_item", StringComparison.OrdinalIgnoreCase) || objective.Kind.Equals("has_item", StringComparison.OrdinalIgnoreCase))
            && !string.IsNullOrWhiteSpace(objective.TargetId) && !context.ItemIds.Contains(objective.TargetId))
        {
            Add(report, "quest.objective.item_missing", ValidationSeverity.Error, "Quest objective references a missing item.", ownerId);
        }
        else if (objective.Kind.Equals("complete_encounter", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(objective.TargetId) && !context.Package.Game.Encounters.Any(encounter => IdEquals(encounter.Id, objective.TargetId)))
        {
            Add(report, "quest.objective.encounter_missing", ValidationSeverity.Error, "Quest objective references a missing encounter.", ownerId);
        }
        else if ((objective.Kind.Equals("talk_to", StringComparison.OrdinalIgnoreCase) || objective.Kind.Equals("choose_dialogue", StringComparison.OrdinalIgnoreCase))
            && !string.IsNullOrWhiteSpace(objective.TargetId) && !context.DialogueIds.Contains(objective.TargetId) && !objective.Metadata.ContainsKey("dialogue_id"))
        {
            Add(report, "quest.objective.dialogue_missing", ValidationSeverity.Error, "Quest objective references a missing dialogue.", ownerId);
        }
        else if (objective.Kind.Equals("craft_recipe", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(objective.TargetId) && !context.RecipeIds.Contains(objective.TargetId))
        {
            Add(report, "quest.objective.recipe_missing", ValidationSeverity.Error, "Quest objective references a missing recipe.", ownerId);
        }
        else if (objective.Kind.Equals("execute_transaction", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(objective.TargetId) && !context.TransactionIds.Contains(objective.TargetId))
        {
            Add(report, "quest.objective.transaction_missing", ValidationSeverity.Error, "Quest objective references a missing transaction.", ownerId);
        }
        else if (objective.Kind.Equals("harvest_resource", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(objective.TargetId) && !context.ResourceNodeIds.Contains(objective.TargetId))
        {
            Add(report, "quest.objective.resource_node_missing", ValidationSeverity.Error, "Quest objective references a missing resource node.", ownerId);
        }

        ValidateRequirements(context, report, objective.Conditions, ownerId);
        ValidateOutputs(context, report, objective.CompletionEffects, ownerId, "quest.objective.completion_effect");
    }

    private static void ValidateRequirements(ValidationContext context, ValidationReport report, IEnumerable<RequirementDefinition> requirements, string ownerId)
    {
        foreach (var requirement in requirements)
        {
            RequireText(report, requirement.Kind, "requirement.kind.empty", "Requirement kind is required.", ownerId);
            if ((requirement.Kind.Equals("has_item", StringComparison.OrdinalIgnoreCase) || requirement.Kind.Equals("inventory_has", StringComparison.OrdinalIgnoreCase)) && !context.ItemIds.Contains(requirement.Id))
            {
                Add(report, "requirement.item_missing", ValidationSeverity.Error, $"Requirement references a missing item: {requirement.Id}", ownerId);
            }
            else if ((requirement.Kind.Equals("resource_at_least", StringComparison.OrdinalIgnoreCase) || requirement.Kind.Equals("network_resource_at_least", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(requirement.Id))
            {
                Add(report, "requirement.resource_missing", ValidationSeverity.Error, $"Requirement references a missing resource: {requirement.Id}", ownerId);
            }
            else if (requirement.Kind.Equals("quest_state", StringComparison.OrdinalIgnoreCase) && !context.QuestIds.Contains(requirement.Id))
            {
                Add(report, "requirement.quest_missing", ValidationSeverity.Error, $"Requirement references a missing quest: {requirement.Id}", ownerId);
            }
            else if ((requirement.Kind.Equals("reputation_at_least", StringComparison.OrdinalIgnoreCase) || requirement.Kind.Equals("faction_reputation_at_least", StringComparison.OrdinalIgnoreCase) || requirement.Kind.Equals("faction_relation_is", StringComparison.OrdinalIgnoreCase)) && !context.FactionIds.Contains(requirement.Id))
            {
                Add(report, "requirement.faction_missing", ValidationSeverity.Error, $"Requirement references a missing faction: {requirement.Id}", ownerId);
            }
        }
    }

    private static void ValidateCosts(ValidationContext context, ValidationReport report, IEnumerable<CostDefinition> costs, string ownerId, string codePrefix)
    {
        foreach (var cost in costs)
        {
            RequireText(report, cost.Kind, $"{codePrefix}.kind.empty", "Cost kind is required.", ownerId);
            RequireText(report, cost.Id, $"{codePrefix}.id.empty", "Cost id is required.", ownerId);
            if (cost.Amount <= 0)
            {
                Add(report, $"{codePrefix}.amount.invalid", ValidationSeverity.Error, "Cost amount must be positive.", ownerId);
            }
            else if (cost.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) && !context.ItemIds.Contains(cost.Id))
            {
                Add(report, $"{codePrefix}.item_missing", ValidationSeverity.Error, $"Cost references a missing item: {cost.Id}", ownerId);
            }
            else if (cost.Kind.Equals("resource", StringComparison.OrdinalIgnoreCase) && !context.ResourceIds.Contains(cost.Id))
            {
                Add(report, $"{codePrefix}.resource_missing", ValidationSeverity.Error, $"Cost references a missing resource: {cost.Id}", ownerId);
            }
        }
    }

    private static void ValidateOutputs(ValidationContext context, ValidationReport report, IEnumerable<OutputDefinition> outputs, string ownerId, string codePrefix)
    {
        foreach (var output in outputs)
        {
            RequireText(report, output.Kind, $"{codePrefix}.kind.empty", "Output kind is required.", ownerId);
            RequireText(report, output.Id, $"{codePrefix}.id.empty", "Output id is required.", ownerId);
            if ((output.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) || output.Kind.Equals("add_item", StringComparison.OrdinalIgnoreCase)) && !context.ItemIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.item_missing", ValidationSeverity.Error, $"Output references a missing item: {output.Id}", ownerId);
            }
            else if ((output.Kind.Equals("resource", StringComparison.OrdinalIgnoreCase) || output.Kind.Equals("change_resource", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.resource_missing", ValidationSeverity.Error, $"Output references a missing resource: {output.Id}", ownerId);
            }
            else if ((output.Kind.Equals("reputation", StringComparison.OrdinalIgnoreCase) || output.Kind.Equals("faction_reputation", StringComparison.OrdinalIgnoreCase) || output.Kind.Equals("change_reputation", StringComparison.OrdinalIgnoreCase)) && !context.FactionIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.faction_missing", ValidationSeverity.Error, $"Output references a missing faction: {output.Id}", ownerId);
            }
            else if ((output.Kind.Equals("progression", StringComparison.OrdinalIgnoreCase) || output.Kind.Equals("change_progression", StringComparison.OrdinalIgnoreCase)) && !context.ProgressionIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.progression_missing", ValidationSeverity.Error, $"Output references a missing progression: {output.Id}", ownerId);
            }
            else if (output.Kind.Equals("loot", StringComparison.OrdinalIgnoreCase) && !context.LootTableIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.loot_table_missing", ValidationSeverity.Error, $"Output references a missing loot table: {output.Id}", ownerId);
            }
        }
    }

    private static RequirementDefinition RuntimeLikeRequirement(ConditionDefinition condition)
    {
        condition.Args.TryGetValue("id", out var id);
        condition.Args.TryGetValue("itemId", out var itemId);
        condition.Args.TryGetValue("resourceId", out var resourceId);
        condition.Args.TryGetValue("flagId", out var flagId);
        condition.Args.TryGetValue("amount", out var amountText);
        double.TryParse(amountText, out var amount);
        return new RequirementDefinition
        {
            Kind = condition.Type,
            Id = id ?? itemId ?? resourceId ?? flagId ?? string.Empty,
            Amount = string.IsNullOrWhiteSpace(amountText) ? null : amount,
            Metadata = new Dictionary<string, string>(condition.Args)
        };
    }

    private static OutputDefinition RuntimeLikeOutput(EffectDefinition effect)
    {
        effect.Args.TryGetValue("id", out var id);
        effect.Args.TryGetValue("resourceId", out var resourceId);
        effect.Args.TryGetValue("itemId", out var itemId);
        effect.Args.TryGetValue("flagId", out var flagId);
        effect.Args.TryGetValue("amount", out var amountText);
        double.TryParse(amountText, out var amount);
        return new OutputDefinition
        {
            Kind = effect.Type,
            Id = id ?? itemId ?? resourceId ?? flagId ?? string.Empty,
            Amount = string.IsNullOrWhiteSpace(amountText) ? 1 : amount,
            Metadata = new Dictionary<string, string>(effect.Args)
        };
    }

    private static void CheckIds(ValidationReport report, IEnumerable<string> ids, string group, string? ownerId = null)
    {
        foreach (var duplicate in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id).Where(g => g.Count() > 1))
        {
            Add(report, $"{group}.id.duplicate", ValidationSeverity.Error, $"Duplicate {group} id: {duplicate.Key}", ownerId ?? duplicate.Key);
        }
    }

    private static void RequireText(ValidationReport report, string? value, string code, string message, string? targetId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void Add(ValidationReport report, string code, ValidationSeverity severity, string message, string? targetId)
    {
        ValidationIssueBuilder.Add(report, code, severity, message, targetId, Category);
    }

    private static bool IdEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
    }
}
