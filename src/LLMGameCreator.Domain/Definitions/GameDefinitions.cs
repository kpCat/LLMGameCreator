namespace LLMGameCreator.Domain.Definitions;

public sealed class GameManifest
{
    public string PackageId { get; set; } = "game/unnamed";
    public string Title { get; set; } = "Unnamed Game";
    public string Version { get; set; } = "0.1.0";
    public string FormatVersion { get; set; } = "0.1";
    public string StartMapId { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class GameDefinition
{
    public List<TilePrototypeDefinition> TilePrototypes { get; set; } = new List<TilePrototypeDefinition>();
    public List<EntityPrototypeDefinition> EntityPrototypes { get; set; } = new List<EntityPrototypeDefinition>();
    public List<MapDefinition> Maps { get; set; } = new List<MapDefinition>();
    public List<ItemDefinition> Items { get; set; } = new List<ItemDefinition>();
    public List<ResourceDefinition> Resources { get; set; } = new List<ResourceDefinition>();
    public List<StatusDefinition> Statuses { get; set; } = new List<StatusDefinition>();
    public List<RecipeDefinition> Recipes { get; set; } = new List<RecipeDefinition>();
    public List<LootTableDefinition> LootTables { get; set; } = new List<LootTableDefinition>();
    public List<TransactionDefinition> Transactions { get; set; } = new List<TransactionDefinition>();
    public List<ResourceNetworkDefinition> ResourceNetworks { get; set; } = new List<ResourceNetworkDefinition>();
    public List<ResourceNodeDefinition> ResourceNodes { get; set; } = new List<ResourceNodeDefinition>();
    public List<InventoryDefinition> Inventories { get; set; } = new List<InventoryDefinition>();
    public List<EquipmentSlotDefinition> EquipmentSlots { get; set; } = new List<EquipmentSlotDefinition>();
    public List<AbilityDefinition> Abilities { get; set; } = new List<AbilityDefinition>();
    public List<QuestDefinition> Quests { get; set; } = new List<QuestDefinition>();
    public List<DialogueDefinition> Dialogues { get; set; } = new List<DialogueDefinition>();
    public List<FormulaDefinition> Formulas { get; set; } = new List<FormulaDefinition>();
    public List<InteractionDefinition> Interactions { get; set; } = new List<InteractionDefinition>();
}

public sealed class TilePrototypeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Walkable { get; set; } = true;
    public double MovementCost { get; set; } = 1.0;
    public string? AssetId { get; set; }
}

public sealed class MapDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string DefaultTileId { get; set; } = string.Empty;
    public Position2D StartPosition { get; set; } = new Position2D();
    public List<TileOverrideDefinition> Tiles { get; set; } = new List<TileOverrideDefinition>();
    public List<EntityInstanceDefinition> Entities { get; set; } = new List<EntityInstanceDefinition>();
}

public sealed class TileOverrideDefinition
{
    public int X { get; set; }
    public int Y { get; set; }
    public string TileId { get; set; } = string.Empty;
}

public sealed class EntityPrototypeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AssetId { get; set; }
    public List<ComponentDefinition> Components { get; set; } = new List<ComponentDefinition>();
}

public sealed class EntityInstanceDefinition
{
    public string Id { get; set; } = string.Empty;
    public string PrototypeId { get; set; } = string.Empty;
    public Position2D Position { get; set; } = new Position2D();
    public List<ComponentDefinition> Components { get; set; } = new List<ComponentDefinition>();
}
