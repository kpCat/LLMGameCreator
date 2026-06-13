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
        StatIds = package.Game.Stats
            .Where(stat => !string.IsNullOrWhiteSpace(stat.Id))
            .Select(stat => stat.Id)
            .ToHashSet();
        ProgressionIds = package.Game.Progressions
            .Where(progression => !string.IsNullOrWhiteSpace(progression.Id))
            .Select(progression => progression.Id)
            .ToHashSet();
        AbilityIds = package.Game.Abilities
            .Where(ability => !string.IsNullOrWhiteSpace(ability.Id))
            .Select(ability => ability.Id)
            .ToHashSet();
        StatusIds = package.Game.Statuses
            .Where(status => !string.IsNullOrWhiteSpace(status.Id))
            .Select(status => status.Id)
            .ToHashSet();
        LootTableIds = package.Game.LootTables
            .Where(loot => !string.IsNullOrWhiteSpace(loot.Id))
            .Select(loot => loot.Id)
            .ToHashSet();
        TransactionIds = package.Game.Transactions
            .Where(transaction => !string.IsNullOrWhiteSpace(transaction.Id))
            .Select(transaction => transaction.Id)
            .ToHashSet();
        RecipeIds = package.Game.Recipes
            .Where(recipe => !string.IsNullOrWhiteSpace(recipe.Id))
            .Select(recipe => recipe.Id)
            .ToHashSet();
        DialogueIds = package.Game.Dialogues
            .Where(dialogue => !string.IsNullOrWhiteSpace(dialogue.Id))
            .Select(dialogue => dialogue.Id)
            .ToHashSet();
        QuestIds = package.Game.Quests
            .Where(quest => !string.IsNullOrWhiteSpace(quest.Id))
            .Select(quest => quest.Id)
            .ToHashSet();
        FactionIds = package.Game.Factions
            .Where(faction => !string.IsNullOrWhiteSpace(faction.Id))
            .Select(faction => faction.Id)
            .ToHashSet();
        ResourceNodeIds = package.Game.ResourceNodes
            .Where(node => !string.IsNullOrWhiteSpace(node.Id))
            .Select(node => node.Id)
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
    public IReadOnlySet<string> StatIds { get; }
    public IReadOnlySet<string> ProgressionIds { get; }
    public IReadOnlySet<string> AbilityIds { get; }
    public IReadOnlySet<string> StatusIds { get; }
    public IReadOnlySet<string> LootTableIds { get; }
    public IReadOnlySet<string> TransactionIds { get; }
    public IReadOnlySet<string> RecipeIds { get; }
    public IReadOnlySet<string> DialogueIds { get; }
    public IReadOnlySet<string> QuestIds { get; }
    public IReadOnlySet<string> FactionIds { get; }
    public IReadOnlySet<string> ResourceNodeIds { get; }
    public IReadOnlySet<string> ResourceNetworkIds { get; }
    public IReadOnlySet<string> AssetIds { get; }
    public IReadOnlySet<string> AssetContractIds { get; }
    public IReadOnlyDictionary<string, ScriptDefinition> ScriptsById { get; }
}
