using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Validation;

public interface IGamePackageValidator
{
    ValidationReport Validate(GamePackageDefinition package);
    ValidationReport Validate(GamePackageDefinition package, string? projectFolder);
}

public sealed class GamePackageValidator : IGamePackageValidator
{
    public ValidationReport Validate(GamePackageDefinition package)
    {
        return Validate(package, null);
    }

    public ValidationReport Validate(GamePackageDefinition package, string? projectFolder)
    {
        var report = new ValidationReport();
        ValidateManifest(package, report);
        ValidateDuplicates(package, report);
        ValidateAssets(package, projectFolder, report);
        ValidateAssetReferences(package, report);
        ValidateMaps(package, report);
        ValidateEntities(package, report);
        ValidateScripts(package, projectFolder, report);
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

    private static void ValidateAssets(GamePackageDefinition package, string? projectFolder, ValidationReport report)
    {
        var assetIds = package.AssetCatalog.Assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.Id))
            .Select(asset => asset.Id)
            .ToHashSet();
        var contractIds = package.AssetCatalog.Contracts
            .Where(contract => !string.IsNullOrWhiteSpace(contract.Id))
            .Select(contract => contract.Id)
            .ToHashSet();

        foreach (var asset in package.AssetCatalog.Assets)
        {
            if (string.IsNullOrWhiteSpace(asset.Id))
            {
                Add(report, "asset.id.empty", ValidationSeverity.Error, "Asset id is empty.", null);
            }

            if (string.IsNullOrWhiteSpace(asset.Type))
            {
                Add(report, "asset.type.empty", ValidationSeverity.Error, "Asset type is empty.", asset.Id);
            }

            ValidateAssetPath(asset, projectFolder, report);

            if (!string.IsNullOrWhiteSpace(asset.FallbackAssetId) && !assetIds.Contains(asset.FallbackAssetId))
            {
                Add(report, "asset.fallback.missing", ValidationSeverity.Error, "FallbackAssetId references a missing asset.", asset.Id);
            }

            if (!string.IsNullOrWhiteSpace(asset.ContractId) && !contractIds.Contains(asset.ContractId))
            {
                Add(report, "asset.contract.missing", ValidationSeverity.Error, "ContractId references a missing asset contract.", asset.Id);
            }
        }
    }

    private static void ValidateAssetPath(AssetDefinition asset, string? projectFolder, ValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(asset.Path))
        {
            return;
        }

        if (Path.IsPathRooted(asset.Path))
        {
            Add(report, "asset.path.rooted", ValidationSeverity.Error, "Asset path must be relative.", asset.Id);
            return;
        }

        if (ContainsPathTraversal(asset.Path))
        {
            Add(report, "asset.path.traversal", ValidationSeverity.Error, "Asset path must not contain path traversal.", asset.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(projectFolder))
        {
            return;
        }

        if (!TryResolveInsideProject(projectFolder, asset.Path, out var resolvedPath))
        {
            Add(report, "asset.path.outside_project", ValidationSeverity.Error, "Asset path normalizes outside projectFolder.", asset.Id);
            return;
        }

        if (!File.Exists(resolvedPath))
        {
            Add(report, "asset.path.missing", ValidationSeverity.Warning, "Asset path points to a missing file.", asset.Id);
        }
    }

    private static void ValidateAssetReferences(GamePackageDefinition package, ValidationReport report)
    {
        var assetIds = package.AssetCatalog.Assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.Id))
            .Select(asset => asset.Id)
            .ToHashSet();

        foreach (var tile in package.Game.TilePrototypes)
        {
            if (!string.IsNullOrWhiteSpace(tile.AssetId) && !assetIds.Contains(tile.AssetId))
            {
                Add(report, "tile.asset.missing", ValidationSeverity.Error, "TilePrototype AssetId references a missing asset.", tile.Id);
            }
        }

        foreach (var entity in package.Game.EntityPrototypes)
        {
            if (!string.IsNullOrWhiteSpace(entity.AssetId) && !assetIds.Contains(entity.AssetId))
            {
                Add(report, "entity.asset.missing", ValidationSeverity.Error, "EntityPrototype AssetId references a missing asset.", entity.Id);
            }
        }
    }

    private static void ValidateScripts(GamePackageDefinition package, string? projectFolder, ValidationReport report)
    {
        var scriptById = package.ScriptCatalog.Scripts
            .Where(script => !string.IsNullOrWhiteSpace(script.Id))
            .GroupBy(script => script.Id)
            .ToDictionary(group => group.Key, group => group.First());
        var duplicateGeneratorIds = package.ScriptCatalog.Generators
            .Select(generator => generator.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();

        foreach (var script in package.ScriptCatalog.Scripts)
        {
            ValidateScriptDefinition(script, projectFolder, report);
        }

        foreach (var generator in package.ScriptCatalog.Generators)
        {
            ValidateGeneratorDefinition(generator, scriptById, duplicateGeneratorIds, report);
        }
    }

    private static void ValidateScriptDefinition(ScriptDefinition script, string? projectFolder, ValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(script.Id))
        {
            Add(report, "script.id.empty", ValidationSeverity.Error, "Script id не заполнен.", null);
        }

        var resolvedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(script.Path))
        {
            Add(report, "script.path.empty", ValidationSeverity.Error, "Script path не заполнен.", script.Id);
        }
        else if (Path.IsPathRooted(script.Path))
        {
            Add(report, "script.path.rooted", ValidationSeverity.Error, "Script path должен быть относительным.", script.Id);
        }
        else if (ContainsPathTraversal(script.Path))
        {
            Add(report, "script.path.traversal", ValidationSeverity.Error, "Script path must not contain path traversal.", script.Id);
        }
        else if (!string.IsNullOrWhiteSpace(projectFolder) && !TryResolveInsideProject(projectFolder, script.Path, out resolvedPath))
        {
            Add(report, "script.path.outside_project", ValidationSeverity.Error, "Script path normalizes outside projectFolder.", script.Id);
        }
        else if (!string.IsNullOrWhiteSpace(projectFolder) && !File.Exists(resolvedPath))
        {
            Add(report, "script.path.missing", ValidationSeverity.Error, "Script path указывает на несуществующий файл.", script.Id);
        }

        if (!Enum.IsDefined(typeof(LuaScriptKind), script.Kind))
        {
            Add(report, "script.kind.invalid", ValidationSeverity.Error, "Script kind неизвестен.", script.Id);
        }

        if (!HasAnyText(script.EntryPoints))
        {
            Add(report, "script.entry_points.empty", ValidationSeverity.Error, "Script entryPoints не заполнены.", script.Id);
        }

        if (!HasAnyText(script.Capabilities))
        {
            Add(report, "script.capabilities.empty", ValidationSeverity.Error, "Script capabilities не заполнены.", script.Id);
        }
    }

    private static void ValidateGeneratorDefinition(
        GeneratorDefinition generator,
        IReadOnlyDictionary<string, ScriptDefinition> scriptById,
        IReadOnlySet<string> duplicateGeneratorIds,
        ValidationReport report)
    {
        if (string.IsNullOrWhiteSpace(generator.Id))
        {
            Add(report, "generator.id.empty", ValidationSeverity.Error, "Generator id не заполнен.", null);
        }

        if (!string.IsNullOrWhiteSpace(generator.Id) && duplicateGeneratorIds.Contains(generator.Id))
        {
            Add(report, "duplicate.generator", ValidationSeverity.Error, $"Duplicate generator id: {generator.Id}", generator.Id);
        }

        if (string.IsNullOrWhiteSpace(generator.Kind))
        {
            Add(report, "generator.kind.empty", ValidationSeverity.Error, "Generator kind is empty.", generator.Id);
        }

        if (string.IsNullOrWhiteSpace(generator.ScriptId))
        {
            Add(report, "generator.script_id.empty", ValidationSeverity.Error, "Generator scriptId не заполнен.", generator.Id);
            return;
        }

        if (!scriptById.TryGetValue(generator.ScriptId, out var script))
        {
            Add(report, "generator.script_id.missing", ValidationSeverity.Error, "Generator scriptId ссылается на несуществующий script.", generator.Id);
            return;
        }

        if (script.Kind != LuaScriptKind.Generator)
        {
            Add(report, "generator.script_kind.invalid", ValidationSeverity.Error, "Generator scriptId должен ссылаться на script с Kind == Generator.", generator.Id);
        }

        if (string.IsNullOrWhiteSpace(generator.EntryPoint))
        {
            Add(report, "generator.entry_point.empty", ValidationSeverity.Error, "Generator entryPoint не заполнен.", generator.Id);
        }
        else if (!script.EntryPoints.Any(entryPoint => string.Equals(entryPoint, generator.EntryPoint.Trim(), StringComparison.Ordinal)))
        {
            Add(report, "generator.entry_point.missing", ValidationSeverity.Error, "Generator entryPoint отсутствует в script.EntryPoints.", generator.Id);
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

    private static bool HasAnyText(IEnumerable<string> values)
    {
        return values.Any(value => !string.IsNullOrWhiteSpace(value));
    }

    private static bool ContainsPathTraversal(string path)
    {
        return path
            .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment == "..");
    }

    private static bool TryResolveInsideProject(string projectFolder, string relativePath, out string resolvedPath)
    {
        resolvedPath = string.Empty;

        try
        {
            var projectRoot = Path.GetFullPath(projectFolder);
            var projectRootWithSeparator = EnsureTrailingSeparator(projectRoot);
            var candidate = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

            if (!candidate.StartsWith(projectRootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(candidate, projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            resolvedPath = candidate;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }
}
