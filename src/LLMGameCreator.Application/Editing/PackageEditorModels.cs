namespace LLMGameCreator.Application.Editing;

public sealed class PackageEditorSnapshot
{
    public bool HasCurrentPackage { get; set; }
    public string? CurrentFolder { get; set; }
    public ManifestEditModel Manifest { get; set; } = new ManifestEditModel();
    public IReadOnlyList<MapEditModel> Maps { get; set; } = Array.Empty<MapEditModel>();
    public IReadOnlyList<TilePrototypeEditModel> TilePrototypes { get; set; } = Array.Empty<TilePrototypeEditModel>();
    public IReadOnlyList<EntityPrototypeEditModel> EntityPrototypes { get; set; } = Array.Empty<EntityPrototypeEditModel>();
    public IReadOnlyList<AssetSummaryModel> Assets { get; set; } = Array.Empty<AssetSummaryModel>();
    public IReadOnlyList<ScriptSummaryModel> Scripts { get; set; } = Array.Empty<ScriptSummaryModel>();
}

public sealed class ManifestEditModel
{
    public string PackageId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FormatVersion { get; set; } = string.Empty;
    public string StartMapId { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class MapEditModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string DefaultTileId { get; set; } = string.Empty;
    public int StartX { get; set; }
    public int StartY { get; set; }
}

public sealed class TilePrototypeEditModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Walkable { get; set; } = true;
    public double MovementCost { get; set; } = 1.0;
    public string? AssetId { get; set; }
}

public sealed class EntityPrototypeEditModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AssetId { get; set; }
    public int ComponentsCount { get; set; }
}

public sealed class AssetSummaryModel
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? ContractId { get; set; }
}

public sealed class ScriptSummaryModel
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public IReadOnlyList<string> EntryPoints { get; set; } = Array.Empty<string>();
}
