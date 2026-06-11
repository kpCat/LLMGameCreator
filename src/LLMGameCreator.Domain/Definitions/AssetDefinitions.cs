namespace LLMGameCreator.Domain.Definitions;

public sealed class AssetCatalog
{
    public List<AssetDefinition> Assets { get; set; } = new List<AssetDefinition>();
    public List<AssetContractDefinition> Contracts { get; set; } = new List<AssetContractDefinition>();
    public List<AssetGenerationRequestDefinition> GenerationRequests { get; set; } = new List<AssetGenerationRequestDefinition>();
}

public sealed class AssetDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? ContractId { get; set; }
    public string? FallbackAssetId { get; set; }
    public List<string> LinkedEntityIds { get; set; } = new List<string>();
    public Dictionary<string, AssetVariantDefinition> Variants { get; set; } = new Dictionary<string, AssetVariantDefinition>();
}

public sealed class AssetVariantDefinition
{
    public string Path { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
}

public sealed class AssetContractDefinition
{
    public string Id { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? TileWidth { get; set; }
    public int? TileHeight { get; set; }
    public int? FrameWidth { get; set; }
    public int? FrameHeight { get; set; }
    public int? FramesPerDirection { get; set; }
    public List<string> Directions { get; set; } = new List<string>();
    public List<string> RequiredVariants { get; set; } = new List<string>();
    public List<string> OptionalVariants { get; set; } = new List<string>();
}

public sealed class AssetGenerationRequestDefinition
{
    public string Id { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public string WorkflowProfileId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string NegativePrompt { get; set; } = string.Empty;
    public List<string> RequiredVariants { get; set; } = new List<string>();
    public string Status { get; set; } = "draft";
}
