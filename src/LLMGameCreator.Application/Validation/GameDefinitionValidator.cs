using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal sealed class GameDefinitionValidator : IGamePackageValidationRule
{
    private const string Category = "Game";

    public void Validate(ValidationContext context, ValidationReport report)
    {
        var game = context.Package.Game;

        ValidationIssueBuilder.CheckDuplicates(report, game.TilePrototypes.Select(x => x.Id), "tile", Category);
        ValidationIssueBuilder.CheckDuplicates(report, game.EntityPrototypes.Select(x => x.Id), "entity_prototype", Category);
        ValidationIssueBuilder.CheckDuplicates(report, game.Maps.Select(x => x.Id), "map", Category);
        ValidationIssueBuilder.CheckDuplicates(report, game.Dialogues.Select(x => x.Id), "dialogue", Category);

        ValidateMaps(context, report);
        ValidateEntities(context, report);
        ValidateAssetReferences(context, report);
    }

    private static void ValidateMaps(ValidationContext context, ValidationReport report)
    {
        foreach (var map in context.Package.Game.Maps)
        {
            if (map.Width <= 0 || map.Height <= 0)
            {
                ValidationIssueBuilder.Add(report, "map.size.invalid", ValidationSeverity.Error, "Размер карты должен быть больше нуля.", map.Id, Category);
            }

            if (!context.TileIds.Contains(map.DefaultTileId))
            {
                ValidationIssueBuilder.Add(report, "map.default_tile.missing", ValidationSeverity.Error, "DefaultTileId не найден среди tile prototypes.", map.Id, Category);
            }

            foreach (var tile in map.Tiles)
            {
                if (!context.TileIds.Contains(tile.TileId))
                {
                    ValidationIssueBuilder.Add(report, "map.tile_ref.missing", ValidationSeverity.Error, $"TileId не найден: {tile.TileId}", map.Id, Category);
                }
            }
        }
    }

    private static void ValidateEntities(ValidationContext context, ValidationReport report)
    {
        foreach (var map in context.Package.Game.Maps)
        {
            foreach (var entity in map.Entities)
            {
                if (!context.EntityPrototypeIds.Contains(entity.PrototypeId))
                {
                    ValidationIssueBuilder.Add(report, "entity.prototype.missing", ValidationSeverity.Error, $"PrototypeId не найден: {entity.PrototypeId}", entity.Id, Category);
                }
            }
        }
    }

    private static void ValidateAssetReferences(ValidationContext context, ValidationReport report)
    {
        foreach (var tile in context.Package.Game.TilePrototypes)
        {
            if (!string.IsNullOrWhiteSpace(tile.AssetId) && !context.AssetIds.Contains(tile.AssetId))
            {
                ValidationIssueBuilder.Add(report, "tile.asset.missing", ValidationSeverity.Error, "TilePrototype AssetId references a missing asset.", tile.Id, Category);
            }
        }

        foreach (var entity in context.Package.Game.EntityPrototypes)
        {
            if (!string.IsNullOrWhiteSpace(entity.AssetId) && !context.AssetIds.Contains(entity.AssetId))
            {
                ValidationIssueBuilder.Add(report, "entity.asset.missing", ValidationSeverity.Error, "EntityPrototype AssetId references a missing asset.", entity.Id, Category);
            }
        }
    }
}
