using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Editing;

public sealed class PackageEditorService : IPackageEditorService
{
    private readonly ICurrentGamePackageService _currentGamePackageService;
    private readonly IGamePackageValidator _validator;

    public PackageEditorService(ICurrentGamePackageService currentGamePackageService, IGamePackageValidator validator)
    {
        _currentGamePackageService = currentGamePackageService;
        _validator = validator;
    }

    public PackageEditorSnapshot GetSnapshot()
    {
        var package = _currentGamePackageService.CurrentPackage;
        if (package == null)
        {
            return new PackageEditorSnapshot
            {
                HasCurrentPackage = false,
                CurrentFolder = _currentGamePackageService.CurrentFolder
            };
        }

        return new PackageEditorSnapshot
        {
            HasCurrentPackage = true,
            CurrentFolder = _currentGamePackageService.CurrentFolder,
            Manifest = ToManifestModel(package.Manifest),
            Maps = package.Game.Maps.Select(ToMapModel).ToList(),
            TilePrototypes = package.Game.TilePrototypes.Select(ToTileModel).ToList(),
            EntityPrototypes = package.Game.EntityPrototypes.Select(ToEntityModel).ToList(),
            Assets = package.AssetCatalog.Assets.Select(asset => new AssetSummaryModel
            {
                Id = asset.Id,
                Type = asset.Type,
                Role = asset.Role,
                Path = asset.Path,
                ContractId = asset.ContractId
            }).ToList(),
            Scripts = package.ScriptCatalog.Scripts.Select(script => new ScriptSummaryModel
            {
                Id = script.Id,
                Kind = script.Kind.ToString(),
                Path = script.Path,
                EntryPoints = script.EntryPoints.ToList()
            }).ToList(),
            EconomySystems = new EconomySystemsSummaryModel
            {
                Items = package.Game.Items.Count,
                Resources = package.Game.Resources.Count,
                Statuses = package.Game.Statuses.Count,
                Recipes = package.Game.Recipes.Count,
                LootTables = package.Game.LootTables.Count,
                Transactions = package.Game.Transactions.Count,
                ResourceNetworks = package.Game.ResourceNetworks.Count,
                ResourceNodes = package.Game.ResourceNodes.Count,
                Inventories = package.Game.Inventories.Count
            }
        };
    }

    public void UpdateManifest(ManifestEditModel model)
    {
        var package = RequirePackage();
        var startMapId = RequireId(model.StartMapId, nameof(model.StartMapId));
        if (!package.Game.Maps.Any(map => IdEquals(map.Id, startMapId)))
        {
            throw new InvalidOperationException($"Start map '{startMapId}' does not exist.");
        }

        package.Manifest.PackageId = RequireId(model.PackageId, nameof(model.PackageId));
        package.Manifest.Title = RequireText(model.Title, nameof(model.Title));
        package.Manifest.Version = RequireText(model.Version, nameof(model.Version));
        package.Manifest.FormatVersion = RequireText(model.FormatVersion, nameof(model.FormatVersion));
        package.Manifest.StartMapId = startMapId;
        package.Manifest.Description = NormalizeNullable(model.Description);
    }

    public void AddTilePrototype(TilePrototypeEditModel model)
    {
        var package = RequirePackage();
        var id = RequireId(model.Id, nameof(model.Id));
        EnsureUnique(package.Game.TilePrototypes.Select(tile => tile.Id), id, "tile prototype");
        package.Game.TilePrototypes.Add(ToTileDefinition(model, id));
    }

    public void UpdateTilePrototype(TilePrototypeEditModel model)
    {
        var package = RequirePackage();
        var id = RequireId(model.Id, nameof(model.Id));
        var tile = package.Game.TilePrototypes.FirstOrDefault(item => IdEquals(item.Id, id))
            ?? throw new InvalidOperationException($"Tile prototype '{id}' does not exist.");

        tile.Name = model.Name.Trim();
        tile.Walkable = model.Walkable;
        tile.MovementCost = model.MovementCost;
        tile.AssetId = NormalizeNullable(model.AssetId);
    }

    public void RemoveTilePrototype(string id)
    {
        var package = RequirePackage();
        var normalizedId = RequireId(id, nameof(id));
        if (package.Game.Maps.Any(map => IdEquals(map.DefaultTileId, normalizedId)))
        {
            throw new InvalidOperationException($"Tile prototype '{normalizedId}' is used as a map default tile.");
        }

        if (package.Game.Maps.Any(map => map.Tiles.Any(tile => IdEquals(tile.TileId, normalizedId))))
        {
            throw new InvalidOperationException($"Tile prototype '{normalizedId}' is used by map tile overrides.");
        }

        var tile = package.Game.TilePrototypes.FirstOrDefault(item => IdEquals(item.Id, normalizedId))
            ?? throw new InvalidOperationException($"Tile prototype '{normalizedId}' does not exist.");
        package.Game.TilePrototypes.Remove(tile);
    }

    public void AddMap(MapEditModel model)
    {
        var package = RequirePackage();
        var id = RequireId(model.Id, nameof(model.Id));
        EnsureUnique(package.Game.Maps.Select(map => map.Id), id, "map");
        EnsureTileExists(package, model.DefaultTileId);
        package.Game.Maps.Add(ToMapDefinition(model, id));
    }

    public void UpdateMap(MapEditModel model)
    {
        var package = RequirePackage();
        var id = RequireId(model.Id, nameof(model.Id));
        EnsureTileExists(package, model.DefaultTileId);
        var map = package.Game.Maps.FirstOrDefault(item => IdEquals(item.Id, id))
            ?? throw new InvalidOperationException($"Map '{id}' does not exist.");

        map.Name = model.Name.Trim();
        map.Width = model.Width;
        map.Height = model.Height;
        map.DefaultTileId = RequireId(model.DefaultTileId, nameof(model.DefaultTileId));
        map.StartPosition.X = model.StartX;
        map.StartPosition.Y = model.StartY;
    }

    public void RemoveMap(string id)
    {
        var package = RequirePackage();
        var normalizedId = RequireId(id, nameof(id));
        if (IdEquals(package.Manifest.StartMapId, normalizedId))
        {
            throw new InvalidOperationException($"Map '{normalizedId}' is the current start map.");
        }

        var map = package.Game.Maps.FirstOrDefault(item => IdEquals(item.Id, normalizedId))
            ?? throw new InvalidOperationException($"Map '{normalizedId}' does not exist.");
        package.Game.Maps.Remove(map);
    }

    public void AddEntityPrototype(EntityPrototypeEditModel model)
    {
        var package = RequirePackage();
        var id = RequireId(model.Id, nameof(model.Id));
        EnsureUnique(package.Game.EntityPrototypes.Select(entity => entity.Id), id, "entity prototype");
        package.Game.EntityPrototypes.Add(ToEntityDefinition(model, id));
    }

    public void UpdateEntityPrototype(EntityPrototypeEditModel model)
    {
        var package = RequirePackage();
        var id = RequireId(model.Id, nameof(model.Id));
        var entity = package.Game.EntityPrototypes.FirstOrDefault(item => IdEquals(item.Id, id))
            ?? throw new InvalidOperationException($"Entity prototype '{id}' does not exist.");

        entity.Name = model.Name.Trim();
        entity.AssetId = NormalizeNullable(model.AssetId);
    }

    public void RemoveEntityPrototype(string id)
    {
        var package = RequirePackage();
        var normalizedId = RequireId(id, nameof(id));
        if (package.Game.Maps.Any(map => map.Entities.Any(entity => IdEquals(entity.PrototypeId, normalizedId))))
        {
            throw new InvalidOperationException($"Entity prototype '{normalizedId}' is used by map entities.");
        }

        var entity = package.Game.EntityPrototypes.FirstOrDefault(item => IdEquals(item.Id, normalizedId))
            ?? throw new InvalidOperationException($"Entity prototype '{normalizedId}' does not exist.");
        package.Game.EntityPrototypes.Remove(entity);
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        RequirePackage();
        return _currentGamePackageService.SaveAsync(cancellationToken);
    }

    public ValidationReport Validate()
    {
        var package = RequirePackage();
        return _validator.Validate(package, _currentGamePackageService.CurrentFolder);
    }

    private GamePackageDefinition RequirePackage()
    {
        return _currentGamePackageService.CurrentPackage
            ?? throw new InvalidOperationException("No current game package is loaded.");
    }

    private static ManifestEditModel ToManifestModel(GameManifest manifest)
    {
        return new ManifestEditModel
        {
            PackageId = manifest.PackageId,
            Title = manifest.Title,
            Version = manifest.Version,
            FormatVersion = manifest.FormatVersion,
            StartMapId = manifest.StartMapId,
            Description = manifest.Description
        };
    }

    private static MapEditModel ToMapModel(MapDefinition map)
    {
        return new MapEditModel
        {
            Id = map.Id,
            Name = map.Name,
            Width = map.Width,
            Height = map.Height,
            DefaultTileId = map.DefaultTileId,
            StartX = map.StartPosition.X,
            StartY = map.StartPosition.Y
        };
    }

    private static TilePrototypeEditModel ToTileModel(TilePrototypeDefinition tile)
    {
        return new TilePrototypeEditModel
        {
            Id = tile.Id,
            Name = tile.Name,
            Walkable = tile.Walkable,
            MovementCost = tile.MovementCost,
            AssetId = tile.AssetId
        };
    }

    private static EntityPrototypeEditModel ToEntityModel(EntityPrototypeDefinition entity)
    {
        return new EntityPrototypeEditModel
        {
            Id = entity.Id,
            Name = entity.Name,
            AssetId = entity.AssetId,
            ComponentsCount = entity.Components.Count
        };
    }

    private static TilePrototypeDefinition ToTileDefinition(TilePrototypeEditModel model, string id)
    {
        return new TilePrototypeDefinition
        {
            Id = id,
            Name = model.Name.Trim(),
            Walkable = model.Walkable,
            MovementCost = model.MovementCost,
            AssetId = NormalizeNullable(model.AssetId)
        };
    }

    private static MapDefinition ToMapDefinition(MapEditModel model, string id)
    {
        return new MapDefinition
        {
            Id = id,
            Name = model.Name.Trim(),
            Width = model.Width,
            Height = model.Height,
            DefaultTileId = RequireId(model.DefaultTileId, nameof(model.DefaultTileId)),
            StartPosition = new Position2D(model.StartX, model.StartY)
        };
    }

    private static EntityPrototypeDefinition ToEntityDefinition(EntityPrototypeEditModel model, string id)
    {
        return new EntityPrototypeDefinition
        {
            Id = id,
            Name = model.Name.Trim(),
            AssetId = NormalizeNullable(model.AssetId)
        };
    }

    private static void EnsureTileExists(GamePackageDefinition package, string id)
    {
        var normalizedId = RequireId(id, nameof(id));
        if (!package.Game.TilePrototypes.Any(tile => IdEquals(tile.Id, normalizedId)))
        {
            throw new InvalidOperationException($"Tile prototype '{normalizedId}' does not exist.");
        }
    }

    private static void EnsureUnique(IEnumerable<string> existingIds, string id, string kind)
    {
        if (existingIds.Any(existing => IdEquals(existing, id)))
        {
            throw new InvalidOperationException($"A {kind} with id '{id}' already exists.");
        }
    }

    private static string RequireId(string? value, string name)
    {
        var id = value?.Trim();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id must not be empty.", name);
        }

        return id;
    }

    private static string RequireText(string? value, string name)
    {
        var text = value?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Value must not be empty.", name);
        }

        return text;
    }

    private static string? NormalizeNullable(string? value)
    {
        var text = value?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool IdEquals(string? left, string? right)
    {
        return string.Equals(left?.Trim(), right?.Trim(), StringComparison.Ordinal);
    }
}
