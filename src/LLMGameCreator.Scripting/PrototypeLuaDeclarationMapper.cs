using System.Text.Json;
using System.Text.Json.Nodes;

namespace LLMGameCreator.Scripting;

public sealed class PrototypeLuaDeclarationMapper
{
    public PrototypeLuaMappingResult MapToPackageOperations(IReadOnlyList<PrototypeLuaDeclaration> declarations)
    {
        var diagnostics = new List<PrototypeLuaDiagnostic>();
        var operations = new JsonArray();
        for (var index = 0; index < declarations.Count; index++)
        {
            var declaration = declarations[index];
            var operation = MapDeclaration(declaration, index, diagnostics);
            if (operation != null)
            {
                operations.Add(operation);
            }
        }

        return new PrototypeLuaMappingResult
        {
            Success = !diagnostics.Any(IsError),
            OperationsJson = operations.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            Diagnostics = diagnostics
        };
    }

    private static JsonObject? MapDeclaration(PrototypeLuaDeclaration declaration, int index, List<PrototypeLuaDiagnostic> diagnostics)
    {
        return declaration.Type switch
        {
            "tile" => CopyFields(declaration, index, diagnostics, "upsert_tile_prototype", "id", "name", "walkable", "movement_cost", "asset_id"),
            "map" => CopyFields(declaration, index, diagnostics, "upsert_map", "id", "name", "width", "height", "default_tile_id", "start_x", "start_y"),
            "entity_prototype" => CopyFields(declaration, index, diagnostics, "upsert_entity_prototype", "id", "name", "asset_id"),
            "manifest_update" => CopyFields(declaration, index, diagnostics, "update_manifest", "title", "description", "version", "start_map_id"),
            _ => UnknownType(declaration, index, diagnostics)
        };
    }

    private static JsonObject? CopyFields(PrototypeLuaDeclaration declaration, int index, List<PrototypeLuaDiagnostic> diagnostics, string op, params string[] fields)
    {
        var operation = new JsonObject
        {
            ["op"] = op
        };

        var allowed = fields.Concat(new[] { "type" }).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var property in declaration.Json)
        {
            if (!allowed.Contains(property.Key))
            {
                diagnostics.Add(Error("lua.prototype.declaration.field.unknown", $"Unsupported field for {declaration.Type}: {property.Key}", $"declarations[{index}].{property.Key}"));
                continue;
            }

            if (property.Key.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            operation[property.Key] = property.Value?.DeepClone();
        }

        return operation;
    }

    private static JsonObject? UnknownType(PrototypeLuaDeclaration declaration, int index, List<PrototypeLuaDiagnostic> diagnostics)
    {
        diagnostics.Add(Error("lua.prototype.declaration.type.unknown", $"Unsupported Prototype Lua declaration type: {declaration.Type}", $"declarations[{index}].type"));
        return null;
    }

    private static PrototypeLuaDiagnostic Error(string code, string message, string target)
    {
        return new PrototypeLuaDiagnostic
        {
            Severity = "error",
            Code = code,
            Message = message,
            Target = target
        };
    }

    private static bool IsError(PrototypeLuaDiagnostic diagnostic)
    {
        return diagnostic.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            || diagnostic.Severity.Equals("critical", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class PrototypeLuaMappingResult
{
    public bool Success { get; set; }
    public string OperationsJson { get; set; } = "[]";
    public IReadOnlyList<PrototypeLuaDiagnostic> Diagnostics { get; set; } = Array.Empty<PrototypeLuaDiagnostic>();
}

