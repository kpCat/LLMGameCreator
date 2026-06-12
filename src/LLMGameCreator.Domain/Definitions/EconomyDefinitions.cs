namespace LLMGameCreator.Domain.Definitions;

public sealed class RequirementDefinition
{
    public string Kind { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string? Operator { get; set; }
    public double? Amount { get; set; }
    public string? Value { get; set; }
    public string? Scope { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class CostDefinition
{
    public string Kind { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? Scope { get; set; }
    public string? ConsumeMode { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class OutputDefinition
{
    public string Kind { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? Scope { get; set; }
    public string? Mode { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ResourceDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "abstract";
    public string Description { get; set; } = string.Empty;
    public string? IconAssetId { get; set; }
    public double? DefaultValue { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public double? RegenPerTick { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class StatusDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Kind { get; set; } = "status";
    public string? DurationMode { get; set; }
    public List<EffectDefinition> Effects { get; set; } = new List<EffectDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class RecipeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "crafting";
    public string? StationId { get; set; }
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<CostDefinition> Inputs { get; set; } = new List<CostDefinition>();
    public List<CostDefinition> Costs { get; set; } = new List<CostDefinition>();
    public List<OutputDefinition> Outputs { get; set; } = new List<OutputDefinition>();
    public List<OutputDefinition> FailureOutputs { get; set; } = new List<OutputDefinition>();
    public double? Duration { get; set; }
    public double? Cooldown { get; set; }
    public double? SuccessChance { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class LootTableDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "common";
    public List<LootEntryDefinition> Entries { get; set; } = new List<LootEntryDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class LootEntryDefinition
{
    public string Id { get; set; } = string.Empty;
    public OutputDefinition Output { get; set; } = new OutputDefinition();
    public double? Weight { get; set; }
    public int? MinCount { get; set; }
    public int? MaxCount { get; set; }
    public string? Rarity { get; set; }
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public bool Unique { get; set; }
    public bool QuestItem { get; set; }
    public int? MaxGlobalCount { get; set; }
    public string? SetFlagOnDrop { get; set; }
    public string? RequiresFlag { get; set; }
}

public sealed class TransactionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "shop";
    public string? VendorId { get; set; }
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<CostDefinition> Costs { get; set; } = new List<CostDefinition>();
    public List<OutputDefinition> Outputs { get; set; } = new List<OutputDefinition>();
    public string? StockLootTableId { get; set; }
    public string? RestockRule { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ResourceNetworkDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Kind { get; set; } = "network_resource";
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ResourceNodeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "network_node";
    public string? NetworkId { get; set; }
    public string? EntityPrototypeId { get; set; }
    public List<OutputDefinition> Production { get; set; } = new List<OutputDefinition>();
    public List<CostDefinition> Consumption { get; set; } = new List<CostDefinition>();
    public List<OutputDefinition> Storage { get; set; } = new List<OutputDefinition>();
    public List<CostDefinition> ConversionInputs { get; set; } = new List<CostDefinition>();
    public List<OutputDefinition> ConversionOutputs { get; set; } = new List<OutputDefinition>();
    public List<RequirementDefinition> Requirements { get; set; } = new List<RequirementDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class InventoryDefinition
{
    public string Id { get; set; } = string.Empty;
    public string OwnerKind { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public int Slots { get; set; }
    public List<ItemStackDefinition> Stacks { get; set; } = new List<ItemStackDefinition>();
    public List<string> Tags { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EquipmentSlotDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> AllowedTags { get; set; } = new List<string>();
    public List<string> AllowedKinds { get; set; } = new List<string>();
    public List<RequirementDefinition> RequiredRequirements { get; set; } = new List<RequirementDefinition>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ItemStackDefinition
{
    public string ItemId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? UniqueInstanceId { get; set; }
    public bool? QuestItem { get; set; }
    public double? Durability { get; set; }
    public double? Charge { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
