namespace LLMGameCreator.Domain.Definitions;

public sealed class Position2D
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position2D()
    {
    }

    public Position2D(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public sealed class AssetReference
{
    public string AssetId { get; set; } = string.Empty;
    public string? Variant { get; set; }
}

public sealed class ComponentDefinition
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}

public sealed class ConditionDefinition
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}

public sealed class EffectDefinition
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}

public sealed class FormulaDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string ResultType { get; set; } = "number";
    public string Description { get; set; } = string.Empty;
}
