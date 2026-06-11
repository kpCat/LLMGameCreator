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
    public IReadOnlySet<string> AssetIds { get; }
    public IReadOnlySet<string> AssetContractIds { get; }
    public IReadOnlyDictionary<string, ScriptDefinition> ScriptsById { get; }
}
