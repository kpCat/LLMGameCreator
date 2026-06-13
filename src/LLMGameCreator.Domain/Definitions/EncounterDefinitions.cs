namespace LLMGameCreator.Domain.Definitions;

public sealed class StatDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "attribute";
    public string Description { get; set; } = string.Empty;
    public double? DefaultValue { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? IconAssetId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ProgressionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "xp_level";
    public string Description { get; set; } = string.Empty;
    public List<ProgressionStageDefinition> Stages { get; set; } = new List<ProgressionStageDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ProgressionStageDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double RequiredAmount { get; set; }
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<OutputDefinition> Outputs { get; set; } = new List<OutputDefinition>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EncounterDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "combat";
    public List<EncounterParticipantDefinition> Participants { get; set; } = new List<EncounterParticipantDefinition>();
    public List<EncounterActionDefinition> Actions { get; set; } = new List<EncounterActionDefinition>();
    public List<RequirementDefinition> StartRequirements { get; set; } = new List<RequirementDefinition>();
    public List<RequirementDefinition> WinConditions { get; set; } = new List<RequirementDefinition>();
    public List<RequirementDefinition> LoseConditions { get; set; } = new List<RequirementDefinition>();
    public List<OutputDefinition> Rewards { get; set; } = new List<OutputDefinition>();
    public List<OutputDefinition> Consequences { get; set; } = new List<OutputDefinition>();
    public string? LootTableId { get; set; }
    public int? DefaultSeed { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EncounterParticipantDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "enemy";
    public string? EntityPrototypeId { get; set; }
    public string? FactionId { get; set; }
    public string Team { get; set; } = "neutral";
    public List<OutputDefinition> Stats { get; set; } = new List<OutputDefinition>();
    public List<OutputDefinition> Resources { get; set; } = new List<OutputDefinition>();
    public List<string> Abilities { get; set; } = new List<string>();
    public string? InventoryId { get; set; }
    public string? EquipmentOwnerId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EncounterActionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "ability";
    public string? AbilityId { get; set; }
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<CostDefinition> Costs { get; set; } = new List<CostDefinition>();
    public List<OutputDefinition> Outputs { get; set; } = new List<OutputDefinition>();
    public string? Targeting { get; set; }
    public int? Cooldown { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
