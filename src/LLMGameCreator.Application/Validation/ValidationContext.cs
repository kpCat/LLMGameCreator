using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Validation;

public sealed class ValidationContext
{
    public ValidationContext(GamePackageDefinition package, string? projectFolder)
    {
        Package = package;
        ProjectFolder = projectFolder;
        TileIds = package.Game.TilePrototypes
            .Where(tile => !string.IsNullOrWhiteSpace(tile.Id))
            .Select(tile => tile.Id)
            .ToHashSet();
        EntityPrototypeIds = package.Game.EntityPrototypes
            .Where(prototype => !string.IsNullOrWhiteSpace(prototype.Id))
            .Select(prototype => prototype.Id)
            .ToHashSet();
        ItemIds = package.Game.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.Id))
            .Select(item => item.Id)
            .ToHashSet();
        ResourceIds = package.Game.Resources
            .Where(resource => !string.IsNullOrWhiteSpace(resource.Id))
            .Select(resource => resource.Id)
            .ToHashSet();
        StatusIds = package.Game.Statuses
            .Where(status => !string.IsNullOrWhiteSpace(status.Id))
            .Select(status => status.Id)
            .ToHashSet();
        LootTableIds = package.Game.LootTables
            .Where(loot => !string.IsNullOrWhiteSpace(loot.Id))
            .Select(loot => loot.Id)
            .ToHashSet();
        ResourceNetworkIds = package.Game.ResourceNetworks
            .Where(network => !string.IsNullOrWhiteSpace(network.Id))
            .Select(network => network.Id)
            .ToHashSet();
        AssetIds = package.AssetCatalog.Assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.Id))
            .Select(asset => asset.Id)
            .ToHashSet();
        AssetContractIds = package.AssetCatalog.Contracts
            .Where(contract => !string.IsNullOrWhiteSpace(contract.Id))
            .Select(contract => contract.Id)
            .ToHashSet();
        ScriptsById = package.ScriptCatalog.Scripts
            .Where(script => !string.IsNullOrWhiteSpace(script.Id))
            .GroupBy(script => script.Id)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public GamePackageDefinition Package { get; }
    public string? ProjectFolder { get; }
    public IReadOnlySet<string> TileIds { get; }
    public IReadOnlySet<string> EntityPrototypeIds { get; }
    public IReadOnlySet<string> ItemIds { get; }
    public IReadOnlySet<string> ResourceIds { get; }
    public IReadOnlySet<string> StatusIds { get; }
    public IReadOnlySet<string> LootTableIds { get; }
    public IReadOnlySet<string> ResourceNetworkIds { get; }
    public IReadOnlySet<string> AssetIds { get; }
    public IReadOnlySet<string> AssetContractIds { get; }
    public IReadOnlyDictionary<string, ScriptDefinition> ScriptsById { get; }
}
