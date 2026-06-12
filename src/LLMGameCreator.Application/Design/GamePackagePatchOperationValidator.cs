using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design;

public sealed record GamePackagePatchParseResult(
    GamePackagePatchDocument? Document,
    IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults);

public sealed record GamePackagePatchOperationsValidationResult(
    IReadOnlyList<GamePackagePatchOperation> Operations,
    IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidationResults);

public sealed partial class GamePackagePatchOperationValidator
{
    private const int SchemaVersion = 1;

    private static readonly HashSet<string> ForbiddenFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "lua",
        "lua_code",
        "script",
        "script_id",
        "script_path",
        "code",
        "command",
        "execute",
        "eval",
        "shell",
        "powershell",
        "cmd",
        "path",
        "json_patch",
        "patch_path",
        "asset_path",
        "asset_file",
        "file_path",
        "chunks",
        "tiles"
    };

    private static readonly HashSet<string> TileFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op",
        "id",
        "name",
        "walkable",
        "movement_cost",
        "asset_id"
    };

    private static readonly HashSet<string> MapFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op",
        "id",
        "name",
        "width",
        "height",
        "default_tile_id",
        "start_x",
        "start_y"
    };

    private static readonly HashSet<string> EntityFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op",
        "id",
        "name",
        "asset_id"
    };

    private static readonly HashSet<string> ManifestFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op",
        "title",
        "description",
        "version",
        "start_map_id"
    };

    public GamePackagePatchParseResult ParsePatchDocument(string json, string artifactId)
    {
        var results = new List<GeneratedArtifactValidationResultRecord>();
        try
        {
            var root = JsonNode.Parse(json)?.AsObject();
            if (root == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.json.root.invalid", "Patch JSON root must be an object.", artifactId, results.Count));
                return new GamePackagePatchParseResult(null, results);
            }

            var kind = ReadString(root, "kind");
            if (!kind.Equals(GamePackagePatchArtifactKinds.PatchV1, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.kind.invalid", "Patch kind must be game_package_patch_v1.", artifactId, results.Count));
            }

            var schemaVersion = ReadInt(root, "schema_version", artifactId, "schema_version", results);
            if (schemaVersion != SchemaVersion)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.schema_version.invalid", "Patch schema_version must be 1.", artifactId, results.Count));
            }

            var source = root["source"] as JsonObject;
            var planId = source == null ? string.Empty : ReadString(source, "plan_id");
            var previewArtifactId = source == null ? string.Empty : ReadString(source, "preview_artifact_id");
            if (source == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.source.missing", "Patch source is required.", artifactId, results.Count));
            }

            if (string.IsNullOrWhiteSpace(planId))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.source.plan_id.empty", "Patch source.plan_id is required.", "source.plan_id", results.Count));
            }

            if (string.IsNullOrWhiteSpace(previewArtifactId))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.source.preview_artifact_id.empty", "Patch source.preview_artifact_id is required.", "source.preview_artifact_id", results.Count));
            }

            if (root["operations"] is not JsonArray operationNodes)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operations.missing", "Patch operations array is required.", "operations", results.Count));
                return new GamePackagePatchParseResult(null, results);
            }

            var operationsResult = ParseOperations(operationNodes, artifactId, "operations", results);
            if (results.Any(IsError))
            {
                return new GamePackagePatchParseResult(null, results);
            }

            return new GamePackagePatchParseResult(new GamePackagePatchDocument(
                GamePackagePatchArtifactKinds.PatchV1,
                SchemaVersion,
                new GamePackagePatchSource(planId, previewArtifactId),
                operationsResult.Operations), results);
        }
        catch (JsonException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.json.invalid", ex.Message, artifactId, results.Count));
            return new GamePackagePatchParseResult(null, results);
        }
        catch (InvalidOperationException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.json.invalid", ex.Message, artifactId, results.Count));
            return new GamePackagePatchParseResult(null, results);
        }
    }

    public IReadOnlyList<GeneratedArtifactValidationResultRecord> ValidatePatchJson(string artifactId, string json)
    {
        var parse = ParsePatchDocument(json, artifactId);
        if (parse.ValidationResults.Any(IsError) || parse.Document == null)
        {
            return parse.ValidationResults;
        }

        return new[]
        {
            ValidationResult(artifactId, "info", "patch.schema.valid", "Patch artifact matches game_package_patch_v1 schema.", artifactId, 0)
        };
    }

    public GamePackagePatchOperationsValidationResult ValidatePackageOperationsJson(string operationsJson, string artifactId)
    {
        var results = new List<GeneratedArtifactValidationResultRecord>();
        try
        {
            if (JsonNode.Parse(operationsJson) is not JsonArray operationNodes)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operations.missing", "package_operations must be a JSON array.", "package_operations", results.Count));
                return new GamePackagePatchOperationsValidationResult(Array.Empty<GamePackagePatchOperation>(), results);
            }

            return ParseOperations(operationNodes, artifactId, "package_operations", results);
        }
        catch (JsonException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.json.invalid", ex.Message, artifactId, results.Count));
            return new GamePackagePatchOperationsValidationResult(Array.Empty<GamePackagePatchOperation>(), results);
        }
        catch (InvalidOperationException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.json.invalid", ex.Message, artifactId, results.Count));
            return new GamePackagePatchOperationsValidationResult(Array.Empty<GamePackagePatchOperation>(), results);
        }
    }

    private static GamePackagePatchOperationsValidationResult ParseOperations(
        JsonArray operationNodes,
        string artifactId,
        string targetPrefix,
        List<GeneratedArtifactValidationResultRecord> results)
    {
        if (operationNodes.Count == 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operations.empty", "Patch operations array must not be empty.", targetPrefix, results.Count));
        }

        var operations = new List<GamePackagePatchOperation>();
        var seenTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < operationNodes.Count; index++)
        {
            var target = $"{targetPrefix}[{index}]";
            if (operationNodes[index] is not JsonObject operationNode)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.invalid", "Patch operation must be an object.", target, results.Count));
                continue;
            }

            var operation = ParseOperation(operationNode, artifactId, target, results);
            if (operation == null)
            {
                continue;
            }

            var key = $"{operation.Op}:{operation.Target}";
            if (!seenTargets.Add(key))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.duplicate_target", $"Duplicate operation target: {key}", operation.Target, results.Count));
                continue;
            }

            operations.Add(operation);
        }

        return new GamePackagePatchOperationsValidationResult(results.Any(IsError) ? Array.Empty<GamePackagePatchOperation>() : operations, results);
    }

    private static GamePackagePatchOperation? ParseOperation(JsonObject operationNode, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        var op = ReadString(operationNode, "op");
        if (string.IsNullOrWhiteSpace(op))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.op.empty", "Operation op is required.", $"{target}.op", results.Count));
            return null;
        }

        if (op.Contains("delete", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.delete.unsupported", "Delete operations are not supported in this goal.", target, results.Count));
            return null;
        }

        return op switch
        {
            "upsert_tile_prototype" => ParseTileOperation(operationNode, artifactId, target, results),
            "upsert_map" => ParseMapOperation(operationNode, artifactId, target, results),
            "upsert_entity_prototype" => ParseEntityOperation(operationNode, artifactId, target, results),
            "update_manifest" => ParseManifestOperation(operationNode, artifactId, target, results),
            _ => UnknownOperation(op, artifactId, target, results)
        };
    }

    private static GamePackagePatchOperation? ParseTileOperation(JsonObject node, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        CheckFields(node, TileFields, artifactId, target, results);
        var id = RequiredId(node, "id", artifactId, target, results);
        var name = RequiredString(node, "name", artifactId, target, results);
        var walkable = RequiredBool(node, "walkable", artifactId, target, results);
        var movementCost = RequiredDouble(node, "movement_cost", artifactId, target, results);
        var assetId = OptionalString(node, "asset_id");
        if (movementCost <= 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.tile.movement_cost.invalid", "Tile movement_cost must be positive.", id ?? target, results.Count));
        }

        return id == null || name == null || walkable == null || movementCost == null || results.Any(IsError)
            ? null
            : new UpsertTilePrototypePatchOperation(id, name, walkable.Value, movementCost.Value, assetId);
    }

    private static GamePackagePatchOperation? ParseMapOperation(JsonObject node, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        CheckFields(node, MapFields, artifactId, target, results);
        var id = RequiredId(node, "id", artifactId, target, results);
        var name = RequiredString(node, "name", artifactId, target, results);
        var width = RequiredInt(node, "width", artifactId, target, results);
        var height = RequiredInt(node, "height", artifactId, target, results);
        var defaultTileId = RequiredId(node, "default_tile_id", artifactId, target, results);
        var startX = RequiredInt(node, "start_x", artifactId, target, results);
        var startY = RequiredInt(node, "start_y", artifactId, target, results);
        if (width <= 0 || height <= 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.map.size.invalid", "Map width and height must be positive.", id ?? target, results.Count));
        }

        if (width != null && height != null && startX != null && startY != null && (startX < 0 || startY < 0 || startX >= width || startY >= height))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.map.start_position.out_of_bounds", "Map start position must be inside map bounds.", id ?? target, results.Count));
        }

        return id == null || name == null || width == null || height == null || defaultTileId == null || startX == null || startY == null || results.Any(IsError)
            ? null
            : new UpsertMapPatchOperation(id, name, width.Value, height.Value, defaultTileId, startX.Value, startY.Value);
    }

    private static GamePackagePatchOperation? ParseEntityOperation(JsonObject node, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        CheckFields(node, EntityFields, artifactId, target, results);
        var id = RequiredId(node, "id", artifactId, target, results);
        var name = RequiredString(node, "name", artifactId, target, results);
        var assetId = OptionalString(node, "asset_id");
        return id == null || name == null || results.Any(IsError) ? null : new UpsertEntityPrototypePatchOperation(id, name, assetId);
    }

    private static GamePackagePatchOperation? ParseManifestOperation(JsonObject node, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        CheckFields(node, ManifestFields, artifactId, target, results);
        var title = OptionalString(node, "title");
        var description = OptionalString(node, "description");
        var version = OptionalString(node, "version");
        var startMapId = OptionalString(node, "start_map_id");
        if (startMapId != null && !SlashIdRegex().IsMatch(startMapId))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.id.invalid", "start_map_id must be a lowercase slash id.", $"{target}.start_map_id", results.Count));
        }

        if (title == null && description == null && version == null && startMapId == null)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.manifest.empty", "update_manifest must set at least one supported field.", target, results.Count));
            return null;
        }

        return results.Any(IsError) ? null : new UpdateManifestPatchOperation(title, description, version, startMapId);
    }

    private static GamePackagePatchOperation? UnknownOperation(string op, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        results.Add(ValidationResult(artifactId, "error", "patch.operation.op.unknown", $"Unsupported patch operation: {op}", $"{target}.op", results.Count));
        return null;
    }

    private static void CheckFields(JsonObject node, HashSet<string> allowedFields, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        foreach (var property in node)
        {
            if (ForbiddenFieldNames.Contains(property.Key))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.forbidden", $"Operation field is forbidden: {property.Key}", $"{target}.{property.Key}", results.Count));
                continue;
            }

            if (!allowedFields.Contains(property.Key))
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.unknown", $"Operation field is not supported: {property.Key}", $"{target}.{property.Key}", results.Count));
            }
        }
    }

    private static string? RequiredId(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        var value = RequiredString(node, propertyName, artifactId, target, results);
        if (value != null && !SlashIdRegex().IsMatch(value))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.id.invalid", $"{propertyName} must be a lowercase slash id.", $"{target}.{propertyName}", results.Count));
            return null;
        }

        return value;
    }

    private static string? RequiredString(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        var value = OptionalString(node, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"{target}.{propertyName}", results.Count));
            return null;
        }

        return value;
    }

    private static string OptionalString(JsonObject node, string propertyName)
    {
        try
        {
            return node[propertyName]?.GetValue<string>()?.Trim() ?? string.Empty;
        }
        catch (InvalidOperationException)
        {
            return string.Empty;
        }
    }

    private static string ReadString(JsonObject node, string propertyName)
    {
        return OptionalString(node, propertyName);
    }

    private static bool? RequiredBool(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            if (node[propertyName] == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"{target}.{propertyName}", results.Count));
                return null;
            }

            return node[propertyName]!.GetValue<bool>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be boolean: {propertyName}", $"{target}.{propertyName}", results.Count));
            return null;
        }
    }

    private static int? RequiredInt(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        var value = ReadInt(node, propertyName, artifactId, $"{target}.{propertyName}", results);
        return value;
    }

    private static int? ReadInt(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            if (node[propertyName] == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", target, results.Count));
                return null;
            }

            return node[propertyName]!.GetValue<int>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be integer: {propertyName}", target, results.Count));
            return null;
        }
    }

    private static double? RequiredDouble(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            if (node[propertyName] == null)
            {
                results.Add(ValidationResult(artifactId, "error", "patch.operation.field.empty", $"Operation field is required: {propertyName}", $"{target}.{propertyName}", results.Count));
                return null;
            }

            return node[propertyName]!.GetValue<double>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be numeric: {propertyName}", $"{target}.{propertyName}", results.Count));
            return null;
        }
    }

    private static GeneratedArtifactValidationResultRecord ValidationResult(string artifactId, string severity, string code, string message, string target, int index)
    {
        return new GeneratedArtifactValidationResultRecord(
            BuildValidationResultId(artifactId, code, index),
            artifactId,
            severity,
            code,
            message,
            target,
            "{}");
    }

    private static bool IsError(GeneratedArtifactValidationResultRecord result)
    {
        return result.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            || result.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildValidationResultId(string artifactId, string code, int index)
    {
        return $"{artifactId}/validation/{index.ToString("D3", CultureInfo.InvariantCulture)}/{code}";
    }

    [GeneratedRegex("^[a-z0-9]+(/[a-z0-9][a-z0-9_-]*)+$", RegexOptions.CultureInvariant)]
    private static partial Regex SlashIdRegex();
}
