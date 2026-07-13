namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModuleRuntimeEffectMetricKinds
{
    public const string InventoryItemQuantity = "inventory_item_quantity";
    public const string CombatResourceAmount = "combat_resource_amount";
    public const string EquipmentSlotItemEquals = "equipment_slot_item_equals";
    public const string InventoryItemAbsentOrDecreased = "inventory_item_absent_or_decreased";
    public const string CombatDamageDelta = "combat_damage_delta";
    public const string PlayerStatEquals = "player_stat_equals";
    public const string CombatStatDamageDelta = "combat_stat_damage_delta";
    public const string ProgressionAmountEquals = "progression_amount_equals";
    public const string ProgressionStageEquals = "progression_stage_equals";
    public const string AbilityDirectDamageEquals = "ability_direct_damage_equals";
    public const string ParticipantResourceEquals = "participant_resource_equals";
    public const string StatusTickDamageEquals = "status_tick_damage_equals";
    public const string StatusAbsentAfterExpiry = "status_absent_after_expiry";
    public const string StatusTerminalOutcome = "status_terminal_outcome";
    public const string FactionReputationInitialized = "faction_reputation_initialized";
    public const string QuestStateEquals = "quest_state_equals";
    public const string FactionReputationTransitionTruthful = "faction_reputation_transition_truthful";
    public const string DialogueChoiceVisibilitySequence = "dialogue_choice_visibility_sequence";
    public const string ResourceTransitionTruthful = "resource_transition_truthful";
    public const string FlagEquals = "flag_equals";
    public const string TrustedRewardSocialOutcome = "trusted_reward_social_outcome";
}

public static class FeatureModuleRuntimeEffectComparisonKinds
{
    public const string GreaterThanBaseline = "greater_than_baseline";
    public const string ChangedFromBaseline = "changed_from_baseline";
    public const string Equal = "equal";
    public const string AtLeast = "at_least";
    public const string LessThanBaseline = "less_than_baseline";
    public const string OneOf = "one_of";
}

public sealed record FeatureModuleRuntimeEffectContract
{
    public string EffectId { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string MetricKind { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ResourceOrItemId { get; init; } = string.Empty;
    public string ComparisonKind { get; init; } = FeatureModuleRuntimeEffectComparisonKinds.ChangedFromBaseline;
    public string ExpectedValue { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceOperationIds { get; init; } = [];
    public string RuntimeDimension { get; init; } = string.Empty;
}

public sealed record FeatureModuleRuntimeEffectObservation
{
    public string EffectId { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string MetricKind { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ResourceOrItemId { get; init; } = string.Empty;
    public string ComparisonKind { get; init; } = string.Empty;
    public string ExpectedValue { get; init; } = string.Empty;
    public string BaselineValue { get; init; } = string.Empty;
    public string ActualValue { get; init; } = string.Empty;
    public string RuntimeDimension { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}
