namespace LLMGameCreator.Domain.Definitions;

public sealed class QuestObjectiveDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = "custom_counter";
    public string? TargetId { get; set; }
    public double RequiredAmount { get; set; } = 1;
    public double CurrentAmountDefault { get; set; }
    public List<RequirementDefinition> Conditions { get; set; } = new List<RequirementDefinition>();
    public List<OutputDefinition> CompletionEffects { get; set; } = new List<OutputDefinition>();
    public bool Optional { get; set; }
    public bool Hidden { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class FactionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Kind { get; set; } = "faction";
    public double? DefaultReputation { get; set; }
    public double? MinReputation { get; set; }
    public double? MaxReputation { get; set; }
    public List<FactionRelationDefinition> Relations { get; set; } = new List<FactionRelationDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class FactionRelationDefinition
{
    public string FactionId { get; set; } = string.Empty;
    public string RelationKind { get; set; } = "neutral";
    public double? Value { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
