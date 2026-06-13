using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using LLMGameCreator.Domain.Definitions;

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

    private static readonly JsonSerializerOptions OperationJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

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

    private static readonly HashSet<string> ItemFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "description", "icon_asset_id", "kind", "rarity", "max_stack", "value", "weight",
        "quest_item", "unique", "max_durability", "max_charge", "ammo_type", "fuel_type", "cannot_sell",
        "cannot_drop", "requirements", "tags", "metadata", "use_conditions", "use_effects"
    };

    private static readonly HashSet<string> ResourceFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "description", "icon_asset_id", "default_value", "min_value", "max_value",
        "regen_per_tick", "tags", "metadata"
    };

    private static readonly HashSet<string> StatusFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "description", "kind", "duration_mode", "effects", "tags", "metadata"
    };

    private static readonly HashSet<string> RecipeFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "category", "station_id", "requirements", "inputs", "costs", "outputs",
        "failure_outputs", "duration", "cooldown", "success_chance", "tags", "metadata"
    };

    private static readonly HashSet<string> LootTableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "entries", "tags", "metadata"
    };

    private static readonly HashSet<string> TransactionFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "vendor_id", "requirements", "costs", "outputs", "stock_loot_table_id",
        "restock_rule", "tags", "metadata"
    };

    private static readonly HashSet<string> ResourceNetworkFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "resource_id", "kind", "tags", "metadata"
    };

    private static readonly HashSet<string> ResourceNodeFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "network_id", "entity_prototype_id", "production", "consumption", "storage",
        "conversion_inputs", "conversion_outputs", "requirements", "tags", "metadata"
    };

    private static readonly HashSet<string> InventoryFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "owner_kind", "owner_id", "slots", "stacks", "tags", "metadata"
    };

    private static readonly HashSet<string> EquipmentSlotFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "allowed_tags", "allowed_kinds", "required_requirements", "metadata"
    };

    private static readonly HashSet<string> StatFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "description", "default_value", "min_value", "max_value", "icon_asset_id", "tags", "metadata"
    };

    private static readonly HashSet<string> ProgressionFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "description", "stages", "tags", "metadata"
    };

    private static readonly HashSet<string> EncounterFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "participants", "actions", "start_requirements", "win_conditions",
        "lose_conditions", "rewards", "consequences", "loot_table_id", "default_seed", "tags", "metadata"
    };

    private static readonly HashSet<string> AbilityFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "kind", "requirements", "costs", "cooldown", "targeting", "range", "power",
        "resource_id", "tags", "stages", "learn_conditions", "effects", "metadata"
    };

    private static readonly HashSet<string> QuestFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "title", "description", "kind", "start_conditions", "start_effects", "objectives", "rewards",
        "failure_conditions", "failure_effects", "repeatable", "auto_start", "tags", "metadata", "stages"
    };

    private static readonly HashSet<string> DialogueFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "title", "start_node_id", "background_asset_id", "conditions", "enter_effects", "exit_effects",
        "tags", "metadata", "nodes"
    };

    private static readonly HashSet<string> FactionFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "op", "id", "name", "description", "kind", "default_reputation", "min_reputation", "max_reputation",
        "relations", "tags", "metadata"
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
            "upsert_item_prototype" => ParseEconomyOperation(operationNode, ItemFields, node => new UpsertItemPrototypePatchOperation(ReadDefinition<ItemDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_resource" => ParseEconomyOperation(operationNode, ResourceFields, node => new UpsertResourcePatchOperation(ReadDefinition<ResourceDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_status" => ParseEconomyOperation(operationNode, StatusFields, node => new UpsertStatusPatchOperation(ReadDefinition<StatusDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_recipe" => ParseEconomyOperation(operationNode, RecipeFields, node => new UpsertRecipePatchOperation(ReadDefinition<RecipeDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_loot_table" => ParseEconomyOperation(operationNode, LootTableFields, node => new UpsertLootTablePatchOperation(ReadDefinition<LootTableDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_transaction" => ParseEconomyOperation(operationNode, TransactionFields, node => new UpsertTransactionPatchOperation(ReadDefinition<TransactionDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_resource_network" => ParseEconomyOperation(operationNode, ResourceNetworkFields, node => new UpsertResourceNetworkPatchOperation(ReadDefinition<ResourceNetworkDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_resource_node" => ParseEconomyOperation(operationNode, ResourceNodeFields, node => new UpsertResourceNodePatchOperation(ReadDefinition<ResourceNodeDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_inventory" => ParseEconomyOperation(operationNode, InventoryFields, node => new UpsertInventoryPatchOperation(ReadDefinition<InventoryDefinition>(node)), artifactId, target, results, requireName: false),
            "upsert_equipment_slot" => ParseEconomyOperation(operationNode, EquipmentSlotFields, node => new UpsertEquipmentSlotPatchOperation(ReadDefinition<EquipmentSlotDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_stat" => ParseEconomyOperation(operationNode, StatFields, node => new UpsertStatPatchOperation(ReadDefinition<StatDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_progression" => ParseEconomyOperation(operationNode, ProgressionFields, node => new UpsertProgressionPatchOperation(ReadDefinition<ProgressionDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_encounter" => ParseEconomyOperation(operationNode, EncounterFields, node => new UpsertEncounterPatchOperation(ReadDefinition<EncounterDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_ability" => ParseEconomyOperation(operationNode, AbilityFields, node => new UpsertAbilityPatchOperation(ReadDefinition<AbilityDefinition>(node)), artifactId, target, results, requireName: true),
            "upsert_quest" => ParseEconomyOperation(operationNode, QuestFields, node => new UpsertQuestPatchOperation(ReadDefinition<QuestDefinition>(node)), artifactId, target, results, requireName: false),
            "upsert_dialogue" => ParseEconomyOperation(operationNode, DialogueFields, node => new UpsertDialoguePatchOperation(ReadDefinition<DialogueDefinition>(node)), artifactId, target, results, requireName: false),
            "upsert_faction" => ParseEconomyOperation(operationNode, FactionFields, node => new UpsertFactionPatchOperation(ReadDefinition<FactionDefinition>(node)), artifactId, target, results, requireName: true),
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

    private static GamePackagePatchOperation? ParseEconomyOperation(
        JsonObject node,
        HashSet<string> fields,
        Func<JsonObject, GamePackagePatchOperation> create,
        string artifactId,
        string target,
        List<GeneratedArtifactValidationResultRecord> results,
        bool requireName)
    {
        CheckFields(node, fields, artifactId, target, results);
        CheckForbiddenFieldsRecursive(node, artifactId, target, results);
        var id = RequiredId(node, "id", artifactId, target, results);
        if (requireName)
        {
            RequiredString(node, "name", artifactId, target, results);
        }

        ValidateEconomyNumbers(node, artifactId, target, results);
        if (id == null || results.Any(IsError))
        {
            return null;
        }

        try
        {
            return create(node);
        }
        catch (JsonException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.json.invalid", ex.Message, target, results.Count));
            return null;
        }
        catch (InvalidOperationException ex)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.json.invalid", ex.Message, target, results.Count));
            return null;
        }
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

    private static void CheckForbiddenFieldsRecursive(JsonNode? node, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj)
            {
                var childTarget = $"{target}.{property.Key}";
                if (ForbiddenFieldNames.Contains(property.Key))
                {
                    results.Add(ValidationResult(artifactId, "error", "patch.operation.field.forbidden", $"Operation field is forbidden: {property.Key}", childTarget, results.Count));
                }

                CheckForbiddenFieldsRecursive(property.Value, artifactId, childTarget, results);
            }
        }
        else if (node is JsonArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                CheckForbiddenFieldsRecursive(array[index], artifactId, $"{target}[{index}]", results);
            }
        }
    }

    private static void ValidateEconomyNumbers(JsonObject node, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        CheckOptionalPositive(node, "amount", artifactId, target, results);
        CheckOptionalPositive(node, "max_stack", artifactId, target, results);
        CheckOptionalNonNegative(node, "value", artifactId, target, results);
        CheckOptionalNonNegative(node, "weight", artifactId, target, results);
        CheckOptionalNonNegative(node, "max_durability", artifactId, target, results);
        CheckOptionalNonNegative(node, "max_charge", artifactId, target, results);
        CheckOptionalNonNegative(node, "default_value", artifactId, target, results);
        CheckOptionalNonNegative(node, "min_value", artifactId, target, results);
        CheckOptionalNonNegative(node, "max_value", artifactId, target, results);
        CheckOptionalNonNegative(node, "regen_per_tick", artifactId, target, results);
        CheckOptionalNonNegative(node, "duration", artifactId, target, results);
        CheckOptionalNonNegative(node, "cooldown", artifactId, target, results);
        CheckOptionalNonNegative(node, "slots", artifactId, target, results);

        var successChance = OptionalDouble(node, "success_chance", artifactId, target, results);
        if (successChance.HasValue && (successChance.Value < 0 || successChance.Value > 1))
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.number.invalid", "success_chance must be between 0 and 1.", $"{target}.success_chance", results.Count));
        }

        var minValue = OptionalDouble(node, "min_value", artifactId, target, results);
        var maxValue = OptionalDouble(node, "max_value", artifactId, target, results);
        if (minValue.HasValue && maxValue.HasValue && maxValue.Value < minValue.Value)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.number.invalid", "max_value must be greater than or equal to min_value.", $"{target}.max_value", results.Count));
        }
    }

    private static void CheckOptionalPositive(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        var value = OptionalDouble(node, propertyName, artifactId, target, results);
        if (value.HasValue && value.Value <= 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.number.invalid", $"{propertyName} must be positive.", $"{target}.{propertyName}", results.Count));
        }
    }

    private static void CheckOptionalNonNegative(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        var value = OptionalDouble(node, propertyName, artifactId, target, results);
        if (value.HasValue && value.Value < 0)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.number.invalid", $"{propertyName} must be non-negative.", $"{target}.{propertyName}", results.Count));
        }
    }

    private static double? OptionalDouble(JsonObject node, string propertyName, string artifactId, string target, List<GeneratedArtifactValidationResultRecord> results)
    {
        try
        {
            return node[propertyName]?.GetValue<double>();
        }
        catch (InvalidOperationException)
        {
            results.Add(ValidationResult(artifactId, "error", "patch.operation.field.type", $"Operation field must be numeric: {propertyName}", $"{target}.{propertyName}", results.Count));
            return null;
        }
    }

    private static T ReadDefinition<T>(JsonObject node)
    {
        return node.Deserialize<T>(OperationJsonOptions)
            ?? throw new InvalidOperationException($"Failed to parse {typeof(T).Name}.");
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
