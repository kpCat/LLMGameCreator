namespace LLMGameCreator.Domain.Definitions;

public sealed class ItemDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconAssetId { get; set; }
    public string Kind { get; set; } = "generic";
    public string? Rarity { get; set; }
    public int? MaxStack { get; set; }
    public double? Value { get; set; }
    public double? Weight { get; set; }
    public bool? QuestItem { get; set; }
    public bool? Unique { get; set; }
    public double? MaxDurability { get; set; }
    public double? MaxCharge { get; set; }
    public string? AmmoType { get; set; }
    public string? FuelType { get; set; }
    public bool? CannotSell { get; set; }
    public bool? CannotDrop { get; set; }
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public List<ConditionDefinition> UseConditions { get; set; } = new List<ConditionDefinition>();
    public List<EffectDefinition> UseEffects { get; set; } = new List<EffectDefinition>();
}

public sealed class AbilityDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "active";
    public List<string> Tags { get; set; } = new List<string>();
    public List<AbilityStageDefinition> Stages { get; set; } = new List<AbilityStageDefinition>();
    public List<ConditionDefinition> LearnConditions { get; set; } = new List<ConditionDefinition>();
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
}

public sealed class AbilityStageDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ConditionDefinition> UnlockConditions { get; set; } = new List<ConditionDefinition>();
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
}

public sealed class QuestDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<QuestStageDefinition> Stages { get; set; } = new List<QuestStageDefinition>();
}

public sealed class QuestStageDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<ConditionDefinition> CompleteConditions { get; set; } = new List<ConditionDefinition>();
}

public sealed class InteractionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = "inspect";
    public List<ConditionDefinition> Conditions { get; set; } = new List<ConditionDefinition>();
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
