using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class EncounterDefinitionValidator : IGamePackageValidationRule
{
    private const string Category = "Encounter";

    private static readonly HashSet<string> KnownConditionKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        "always",
        "team_defeated",
        "all_enemies_defeated",
        "all_players_defeated",
        "turn_limit",
        "flag_equals",
        "status_present",
        "status_active",
        "resource_at_least"
    };

    public void Validate(ValidationContext context, ValidationReport report)
    {
        var game = context.Package.Game;
        CheckIds(report, game.Stats.Select(stat => stat.Id), "stat");
        CheckIds(report, game.Progressions.Select(progression => progression.Id), "progression");
        CheckIds(report, game.Encounters.Select(encounter => encounter.Id), "encounter");
        CheckIds(report, game.Abilities.Select(ability => ability.Id), "ability");

        ValidateStats(context, report);
        ValidateProgressions(context, report);
        ValidateAbilities(context, report);
        ValidateEncounters(context, report);
    }

    private static void ValidateStats(ValidationContext context, ValidationReport report)
    {
        foreach (var stat in context.Package.Game.Stats)
        {
            RequireText(report, stat.Id, "stat.id.empty", "Stat id is required.", stat.Id);
            RequireText(report, stat.Name, "stat.name.empty", "Stat name is required.", stat.Id);
            if (stat.MinValue.HasValue && stat.MaxValue.HasValue && stat.MaxValue.Value < stat.MinValue.Value)
            {
                Add(report, "stat.range.invalid", ValidationSeverity.Error, "Stat max_value must be greater than or equal to min_value.", stat.Id);
            }

            if (!string.IsNullOrWhiteSpace(stat.IconAssetId) && !context.AssetIds.Contains(stat.IconAssetId))
            {
                Add(report, "stat.icon_asset.missing", ValidationSeverity.Error, "Stat icon_asset_id references a missing asset.", stat.Id);
            }
        }
    }

    private static void ValidateProgressions(ValidationContext context, ValidationReport report)
    {
        foreach (var progression in context.Package.Game.Progressions)
        {
            RequireText(report, progression.Id, "progression.id.empty", "Progression id is required.", progression.Id);
            RequireText(report, progression.Name, "progression.name.empty", "Progression name is required.", progression.Id);
            CheckIds(report, progression.Stages.Select(stage => stage.Id), "progression.stage", progression.Id);
            foreach (var stage in progression.Stages)
            {
                RequireText(report, stage.Id, "progression.stage.id.empty", "Progression stage id is required.", progression.Id);
                RequireText(report, stage.Name, "progression.stage.name.empty", "Progression stage name is required.", progression.Id);
                if (stage.RequiredAmount < 0)
                {
                    Add(report, "progression.stage.required_amount.invalid", ValidationSeverity.Error, "Progression stage required_amount must be non-negative.", progression.Id);
                }

                ValidateRequirements(context, report, stage.Requirements, progression.Id);
                ValidateOutputs(context, report, stage.Outputs, progression.Id, "progression.stage.output");
            }
        }
    }

    private static void ValidateAbilities(ValidationContext context, ValidationReport report)
    {
        foreach (var ability in context.Package.Game.Abilities)
        {
            RequireText(report, ability.Id, "ability.id.empty", "Ability id is required.", ability.Id);
            RequireText(report, ability.Name, "ability.name.empty", "Ability name is required.", ability.Id);
            CheckOptionalNonNegative(report, ability.Cooldown, "ability.cooldown.invalid", "Ability cooldown must be non-negative.", ability.Id);
            CheckOptionalNonNegative(report, ability.Range, "ability.range.invalid", "Ability range must be non-negative.", ability.Id);
            CheckOptionalNonNegative(report, ability.Power, "ability.power.invalid", "Ability power must be non-negative.", ability.Id);
            if (!string.IsNullOrWhiteSpace(ability.ResourceId) && !context.ResourceIds.Contains(ability.ResourceId))
            {
                Add(report, "ability.resource_missing", ValidationSeverity.Error, "Ability resource_id references a missing resource.", ability.Id);
            }

            ValidateRequirements(context, report, ability.Requirements, ability.Id);
            ValidateCosts(context, report, ability.Costs, ability.Id, "ability.cost");
            foreach (var effect in ability.Effects)
            {
                ValidateEffect(context, report, effect, ability.Id);
            }
        }
    }

    private static void ValidateEncounters(ValidationContext context, ValidationReport report)
    {
        foreach (var encounter in context.Package.Game.Encounters)
        {
            RequireText(report, encounter.Id, "encounter.id.empty", "Encounter id is required.", encounter.Id);
            RequireText(report, encounter.Name, "encounter.name.empty", "Encounter name is required.", encounter.Id);
            CheckIds(report, encounter.Participants.Select(participant => participant.Id), "encounter.participant", encounter.Id);
            ValidateRequirements(context, report, encounter.StartRequirements, encounter.Id);
            ValidateConditions(report, encounter.WinConditions, encounter.Id, "encounter.win_condition");
            ValidateConditions(report, encounter.LoseConditions, encounter.Id, "encounter.lose_condition");
            ValidateOutputs(context, report, encounter.Rewards, encounter.Id, "encounter.reward");
            ValidateOutputs(context, report, encounter.Consequences, encounter.Id, "encounter.consequence");
            if (!string.IsNullOrWhiteSpace(encounter.LootTableId) && !context.LootTableIds.Contains(encounter.LootTableId))
            {
                Add(report, "encounter.loot_table_missing", ValidationSeverity.Error, "Encounter loot_table_id references a missing loot table.", encounter.Id);
            }

            foreach (var participant in encounter.Participants)
            {
                ValidateParticipant(context, report, encounter, participant);
            }

            foreach (var action in encounter.Actions)
            {
                ValidateAction(context, report, encounter, action);
            }
        }
    }

    private static void ValidateParticipant(ValidationContext context, ValidationReport report, EncounterDefinition encounter, EncounterParticipantDefinition participant)
    {
        RequireText(report, participant.Id, "encounter.participant.id.empty", "Encounter participant id is required.", encounter.Id);
        RequireText(report, participant.Name, "encounter.participant.name.empty", "Encounter participant name is required.", encounter.Id);
        if (!string.IsNullOrWhiteSpace(participant.EntityPrototypeId) && !context.EntityPrototypeIds.Contains(participant.EntityPrototypeId))
        {
            Add(report, "encounter.participant.entity_missing", ValidationSeverity.Error, "Encounter participant entity_prototype_id references a missing entity prototype.", encounter.Id);
        }

        foreach (var abilityId in participant.Abilities.Where(id => !context.AbilityIds.Contains(id)))
        {
            Add(report, "encounter.ability_missing", ValidationSeverity.Error, $"Encounter participant ability references a missing ability: {abilityId}", encounter.Id);
        }

        foreach (var resource in participant.Resources.Where(resource => !context.ResourceIds.Contains(resource.Id)))
        {
            Add(report, "encounter.participant.resource_missing", ValidationSeverity.Error, $"Encounter participant resource references a missing resource: {resource.Id}", encounter.Id);
        }

        foreach (var stat in participant.Stats.Where(stat => !context.StatIds.Contains(stat.Id)))
        {
            Add(report, "encounter.participant.stat_missing", ValidationSeverity.Error, $"Encounter participant stat references a missing stat: {stat.Id}", encounter.Id);
        }
    }

    private static void ValidateAction(ValidationContext context, ValidationReport report, EncounterDefinition encounter, EncounterActionDefinition action)
    {
        RequireText(report, action.Id, "encounter.action.id.empty", "Encounter action id is required.", encounter.Id);
        RequireText(report, action.Name, "encounter.action.name.empty", "Encounter action name is required.", encounter.Id);
        if (!string.IsNullOrWhiteSpace(action.AbilityId) && !context.AbilityIds.Contains(action.AbilityId))
        {
            Add(report, "encounter.ability_missing", ValidationSeverity.Error, "Encounter action ability_id references a missing ability.", encounter.Id);
        }

        CheckOptionalNonNegative(report, action.Cooldown, "encounter.action.cooldown.invalid", "Encounter action cooldown must be non-negative.", encounter.Id);
        ValidateRequirements(context, report, action.Requirements, encounter.Id);
        ValidateCosts(context, report, action.Costs, encounter.Id, "encounter.action.cost");
        ValidateOutputs(context, report, action.Outputs, encounter.Id, "encounter.action.output");
    }

    private static void ValidateRequirements(ValidationContext context, ValidationReport report, IEnumerable<RequirementDefinition> requirements, string ownerId)
    {
        foreach (var requirement in requirements)
        {
            RequireText(report, requirement.Kind, "requirement.kind.empty", "Requirement kind is required.", ownerId);
            CheckOptionalNonNegative(report, requirement.Amount, "requirement.amount.invalid", "Requirement amount must be non-negative.", ownerId);
            if ((requirement.Kind.Equals("resource_at_least", StringComparison.OrdinalIgnoreCase)
                || requirement.Kind.Equals("network_resource_at_least", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(requirement.Id))
            {
                Add(report, "requirement.resource_missing", ValidationSeverity.Error, $"Requirement references a missing resource: {requirement.Id}", ownerId);
            }
            else if ((requirement.Kind.Equals("status_present", StringComparison.OrdinalIgnoreCase)
                || requirement.Kind.Equals("status_active", StringComparison.OrdinalIgnoreCase)) && !context.StatusIds.Contains(requirement.Id))
            {
                Add(report, "requirement.status_missing", ValidationSeverity.Error, $"Requirement references a missing status: {requirement.Id}", ownerId);
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

            if (cost.Kind.Equals("resource", StringComparison.OrdinalIgnoreCase) && !context.ResourceIds.Contains(cost.Id))
            {
                Add(report, "ability.cost.resource_missing", ValidationSeverity.Error, $"Ability cost references a missing resource: {cost.Id}", ownerId);
            }
            else if (cost.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) && !context.ItemIds.Contains(cost.Id))
            {
                Add(report, $"{codePrefix}.item_missing", ValidationSeverity.Error, $"Cost references a missing item: {cost.Id}", ownerId);
            }
        }
    }

    private static void ValidateOutputs(ValidationContext context, ValidationReport report, IEnumerable<OutputDefinition> outputs, string ownerId, string codePrefix)
    {
        foreach (var output in outputs)
        {
            RequireText(report, output.Kind, $"{codePrefix}.kind.empty", "Output kind is required.", ownerId);
            RequireText(report, output.Id, $"{codePrefix}.id.empty", "Output id is required.", ownerId);
            if (output.Amount < 0 && !output.Kind.Equals("change_resource", StringComparison.OrdinalIgnoreCase) && !output.Kind.Equals("change_stat", StringComparison.OrdinalIgnoreCase))
            {
                Add(report, $"{codePrefix}.amount.invalid", ValidationSeverity.Error, "Output amount must be non-negative.", ownerId);
            }

            if ((output.Kind.Equals("resource", StringComparison.OrdinalIgnoreCase)
                || output.Kind.Equals("change_resource", StringComparison.OrdinalIgnoreCase)
                || output.Kind.Equals("damage_resource", StringComparison.OrdinalIgnoreCase)
                || output.Kind.Equals("heal_resource", StringComparison.OrdinalIgnoreCase)) && !context.ResourceIds.Contains(output.Id))
            {
                Add(report, "ability.effect.resource_missing", ValidationSeverity.Error, $"Output references a missing resource: {output.Id}", ownerId);
            }
            else if ((output.Kind.Equals("status", StringComparison.OrdinalIgnoreCase)
                || output.Kind.Equals("add_status", StringComparison.OrdinalIgnoreCase)
                || output.Kind.Equals("remove_status", StringComparison.OrdinalIgnoreCase)) && !context.StatusIds.Contains(output.Id))
            {
                Add(report, "ability.effect.status_missing", ValidationSeverity.Error, $"Output references a missing status: {output.Id}", ownerId);
            }
            else if ((output.Kind.Equals("progression", StringComparison.OrdinalIgnoreCase)
                || output.Kind.Equals("change_progression", StringComparison.OrdinalIgnoreCase)) && !context.ProgressionIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.progression_missing", ValidationSeverity.Error, $"Output references a missing progression: {output.Id}", ownerId);
            }
            else if (output.Kind.Equals("item", StringComparison.OrdinalIgnoreCase) && !context.ItemIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.item_missing", ValidationSeverity.Error, $"Output references a missing item: {output.Id}", ownerId);
            }
            else if (output.Kind.Equals("loot", StringComparison.OrdinalIgnoreCase) && !context.LootTableIds.Contains(output.Id))
            {
                Add(report, $"{codePrefix}.loot_table_missing", ValidationSeverity.Error, $"Output references a missing loot table: {output.Id}", ownerId);
            }
        }
    }

    private static void ValidateEffect(ValidationContext context, ValidationReport report, EffectDefinition effect, string ownerId)
    {
        var output = RuntimeLikeOutput(effect);
        ValidateOutputs(context, report, new[] { output }, ownerId, "ability.effect");
    }

    private static OutputDefinition RuntimeLikeOutput(EffectDefinition effect)
    {
        effect.Args.TryGetValue("id", out var id);
        effect.Args.TryGetValue("resourceId", out var resourceId);
        effect.Args.TryGetValue("statusId", out var statusId);
        effect.Args.TryGetValue("itemId", out var itemId);
        effect.Args.TryGetValue("amount", out var amountText);
        double.TryParse(amountText, out var amount);
        return new OutputDefinition
        {
            Kind = effect.Type,
            Id = id ?? resourceId ?? statusId ?? itemId ?? string.Empty,
            Amount = string.IsNullOrWhiteSpace(amountText) ? 1 : amount
        };
    }

    private static void ValidateConditions(ValidationReport report, IEnumerable<RequirementDefinition> conditions, string ownerId, string codePrefix)
    {
        foreach (var condition in conditions)
        {
            if (!string.IsNullOrWhiteSpace(condition.Kind) && !KnownConditionKinds.Contains(condition.Kind))
            {
                Add(report, $"{codePrefix}.kind.future", ValidationSeverity.Warning, $"Condition kind is not handled by runtime v1 yet: {condition.Kind}", ownerId);
            }
        }
    }

    private static void CheckIds(ValidationReport report, IEnumerable<string> ids, string group, string? ownerId = null)
    {
        foreach (var duplicate in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id).Where(g => g.Count() > 1))
        {
            var code = group.Equals("encounter.participant", StringComparison.OrdinalIgnoreCase)
                ? "encounter.participant.duplicate"
                : $"{group}.id.duplicate";
            Add(report, code, ValidationSeverity.Error, $"Duplicate {group} id: {duplicate.Key}", ownerId ?? duplicate.Key);
        }
    }

    private static void RequireText(ValidationReport report, string? value, string code, string message, string? targetId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void CheckOptionalNonNegative(ValidationReport report, double? value, string code, string message, string? targetId)
    {
        if (value.HasValue && value.Value < 0)
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void CheckOptionalNonNegative(ValidationReport report, int? value, string code, string message, string? targetId)
    {
        if (value.HasValue && value.Value < 0)
        {
            Add(report, code, ValidationSeverity.Error, message, targetId);
        }
    }

    private static void Add(ValidationReport report, string code, ValidationSeverity severity, string message, string? targetId)
    {
        ValidationIssueBuilder.Add(report, code, severity, message, targetId, Category);
    }
}
