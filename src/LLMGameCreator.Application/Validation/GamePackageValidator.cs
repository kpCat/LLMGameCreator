using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Validation;

public interface IGamePackageValidator
{
    ValidationReport Validate(GamePackageDefinition package);
}

public sealed class GamePackageValidator : IGamePackageValidator
{
    public ValidationReport Validate(GamePackageDefinition package)
    {
        var report = new ValidationReport();
        ValidateManifest(package, report);
        ValidateDuplicates(package, report);
        ValidateMaps(package, report);
        ValidateEntities(package, report);
        return report;
    }

    private static void ValidateManifest(GamePackageDefinition package, ValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(package.Manifest.PackageId))
        {
            Add(report, "manifest.package_id.empty", ValidationSeverity.Error, "PackageId не заполнен.", null);
        }

        if (string.IsNullOrWhiteSpace(package.Manifest.StartMapId))
        {
            Add(report, "manifest.start_map.empty", ValidationSeverity.Error, "StartMapId не заполнен.", null);
        }
        else if (!package.Game.Maps.Any(m => m.Id == package.Manifest.StartMapId))
        {
            Add(report, "manifest.start_map.missing", ValidationSeverity.Error, "StartMapId ссылается на несуществующую карту.", package.Manifest.StartMapId);
        }
    }

    private static void ValidateDuplicates(GamePackageDefinition package, ValidationReport report)
    {
        CheckDuplicates(report, package.Game.TilePrototypes.Select(x => x.Id), "tile");
        CheckDuplicates(report, package.Game.EntityPrototypes.Select(x => x.Id), "entity_prototype");
        CheckDuplicates(report, package.Game.Maps.Select(x => x.Id), "map");
        CheckDuplicates(report, package.Game.Dialogues.Select(x => x.Id), "dialogue");
        CheckDuplicates(report, package.AssetCatalog.Assets.Select(x => x.Id), "asset");
        CheckDuplicates(report, package.ScriptCatalog.Scripts.Select(x => x.Id), "script");
    }

    private static void ValidateMaps(GamePackageDefinition package, ValidationReport report)
    {
        var tileIds = package.Game.TilePrototypes.Select(t => t.Id).ToHashSet();
        foreach (var map in package.Game.Maps)
        {
            if (map.Width <= 0 || map.Height <= 0)
            {
                Add(report, "map.size.invalid", ValidationSeverity.Error, "Размер карты должен быть больше нуля.", map.Id);
            }

            if (!tileIds.Contains(map.DefaultTileId))
            {
                Add(report, "map.default_tile.missing", ValidationSeverity.Error, "DefaultTileId не найден среди tile prototypes.", map.Id);
            }

            foreach (var tile in map.Tiles)
            {
                if (!tileIds.Contains(tile.TileId))
                {
                    Add(report, "map.tile_ref.missing", ValidationSeverity.Error, $"TileId не найден: {tile.TileId}", map.Id);
                }
            }
        }
    }

    private static void ValidateEntities(GamePackageDefinition package, ValidationReport report)
    {
        var prototypeIds = package.Game.EntityPrototypes.Select(p => p.Id).ToHashSet();
        foreach (var map in package.Game.Maps)
        {
            foreach (var entity in map.Entities)
            {
                if (!prototypeIds.Contains(entity.PrototypeId))
                {
                    Add(report, "entity.prototype.missing", ValidationSeverity.Error, $"PrototypeId не найден: {entity.PrototypeId}", entity.Id);
                }
            }
        }
    }

    private static void CheckDuplicates(ValidationReport report, IEnumerable<string> ids, string group)
    {
        foreach (var duplicate in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id).Where(g => g.Count() > 1))
        {
            Add(report, $"duplicate.{group}", ValidationSeverity.Error, $"Дублирующийся id в группе {group}: {duplicate.Key}", duplicate.Key);
        }
    }

    private static void Add(ValidationReport report, string code, ValidationSeverity severity, string message, string? targetId)
    {
        report.Issues.Add(new ValidationIssue
        {
            Code = code,
            Severity = severity,
            Message = message,
            TargetId = targetId
        });
    }
}
