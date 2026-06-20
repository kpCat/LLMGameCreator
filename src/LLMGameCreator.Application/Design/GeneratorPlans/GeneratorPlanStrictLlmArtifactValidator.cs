using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.GeneratorPlans;

public sealed class GeneratorPlanStrictLlmArtifactValidator
{
    private static readonly HashSet<string> ForbiddenTopLevelFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "script",
        "lua",
        "csharp",
        "sql",
        "powershell",
        "command",
        "commands",
        "execute",
        "eval"
    };

    private static readonly Regex LowerSlashIdPattern = new("^[a-z0-9][a-z0-9._-]*/[a-z0-9][a-z0-9._/-]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<GeneratorPlanStrictLlmArtifactDiagnostic> Validate(
        string contentJson,
        GeneratorPlanStrictLlmArtifactContractDefinition contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        var diagnostics = new List<GeneratorPlanStrictLlmArtifactDiagnostic>();
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(contentJson) ? string.Empty : contentJson);
        }
        catch (JsonException ex)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonInvalid, ex.Message, "content_json", contract.ContractId));
            return diagnostics;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.JsonRootNotObject, "Artifact JSON root must be an object.", "content_json", contract.ContractId));
                return diagnostics;
            }

            foreach (var property in root.EnumerateObject())
            {
                if (ForbiddenTopLevelFields.Contains(property.Name))
                {
                    diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.ForbiddenField, $"Top-level forbidden field '{property.Name}' is not allowed.", property.Name, contract.ContractId));
                }
            }

            RequireString(root, "schema_version", diagnostics, contract.ContractId);
            if (!RequireString(root, "artifact_kind", diagnostics, contract.ContractId, out var artifactKind))
            {
                artifactKind = string.Empty;
            }

            if (!string.Equals(artifactKind, contract.ArtifactKind, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.WrongArtifactKind, $"artifact_kind must equal '{contract.ArtifactKind}'.", "artifact_kind", contract.ContractId));
            }

            foreach (var field in contract.RequiredTopLevelFields)
            {
                if (!root.TryGetProperty(field, out _))
                {
                    diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingField, $"Missing required top-level field '{field}'.", field, contract.ContractId));
                }
            }

            ValidateIds(root, diagnostics, contract.ContractId);
            ValidateReferenceIdTypes(root, diagnostics, contract.ContractId);
            ValidateContractPayload(root, contract, diagnostics);
        }

        return diagnostics
            .OrderBy(diagnostic => SeverityOrder(diagnostic.Severity))
            .ThenBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void ValidateContractPayload(
        JsonElement root,
        GeneratorPlanStrictLlmArtifactContractDefinition contract,
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics)
    {
        switch (contract.ContractId)
        {
            case "game_profile_v1":
                ValidateGameProfile(root, diagnostics, contract.ContractId);
                break;
            case "scene_pack_v1":
                ValidateArrayItems(root, "scenes", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "title", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                    RequireString(item, "purpose", diagnostics, contract.ContractId);
                });
                break;
            case "region_pack_v1":
                ValidateArrayItems(root, "regions", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "title", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                });
                break;
            case "npc_pack_v1":
                ValidateArrayItems(root, "npcs", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "name", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                });
                break;
            case "item_pack_v1":
                ValidateArrayItems(root, "items", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "name", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                });
                break;
            case "dialogue_pack_v1":
                ValidateArrayItems(root, "dialogues", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "title", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                    if (!HasNonEmptyArray(item, "lines"))
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.EmptyRequiredArray, "dialogues[].lines must be a non-empty array.", "dialogues.lines", contract.ContractId));
                    }
                });
                break;
            case "encounter_pack_v1":
                ValidateArrayItems(root, "encounters", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "title", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                });
                break;
            case "quest_pack_v1":
                ValidateArrayItems(root, "quests", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "title", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                    var hasSteps = HasNonEmptyArray(item, "steps");
                    var hasObjectives = HasNonEmptyArray(item, "objectives");
                    if (!hasSteps && !hasObjectives)
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidContractContent, "Each quest must have non-empty steps or objectives.", "quests", contract.ContractId));
                    }
                });
                break;
            case "mechanics_pack_v1":
                ValidateArrayItems(root, "mechanics", contract.ContractId, diagnostics, item =>
                {
                    RequireString(item, "id", diagnostics, contract.ContractId);
                    RequireString(item, "description", diagnostics, contract.ContractId);
                    if (!HasNonEmptyString(item, "title") && !HasNonEmptyString(item, "name"))
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidContractContent, "Each mechanic must have title or name.", "mechanics", contract.ContractId));
                    }

                    if (item.TryGetProperty("tags", out var tags) && tags.ValueKind != JsonValueKind.Array)
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidArray, "mechanics[].tags must be an array.", "mechanics.tags", contract.ContractId));
                    }
                });
                break;
        }
    }

    private static void ValidateGameProfile(JsonElement root, List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics, string contractId)
    {
        if (!root.TryGetProperty("game", out var game) || game.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingField, "game must be an object.", "game", contractId));
            return;
        }

        foreach (var field in new[] { "title", "description", "genre", "tone", "presentation_mode", "world_topology", "actor_model", "combat_model" })
        {
            RequireString(game, field, diagnostics, contractId);
        }

        if (!HasNonEmptyArray(game, "core_loop"))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.EmptyRequiredArray, "game.core_loop must be a non-empty array.", "game.core_loop", contractId));
        }

        if (!HasNonEmptyArray(root, "pillars"))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.EmptyRequiredArray, "pillars must be a non-empty array.", "pillars", contractId));
        }
    }

    private static void ValidateArrayItems(
        JsonElement root,
        string propertyName,
        string contractId,
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        Action<JsonElement> validateItem)
    {
        if (!root.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidArray, $"{propertyName} must be an array.", propertyName, contractId));
            return;
        }

        var items = array.EnumerateArray().ToList();
        if (items.Count == 0)
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.EmptyRequiredArray, $"{propertyName} must be non-empty.", propertyName, contractId));
            return;
        }

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidContractContent, $"{propertyName} items must be objects.", propertyName, contractId));
                continue;
            }

            validateItem(item);
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items.Where(item => item.ValueKind == JsonValueKind.Object))
        {
            if (item.TryGetProperty("id", out var idProperty)
                && idProperty.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(idProperty.GetString())
                && !seenIds.Add(idProperty.GetString()!.Trim()))
            {
                diagnostics.Add(Diagnostic(
                    GeneratorPlanPreviewDiagnosticSeverity.Error,
                    GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidId,
                    $"Duplicate id '{idProperty.GetString()}' in {propertyName}.",
                    propertyName + ".id",
                    contractId));
            }
        }
    }

    private static void ValidateReferenceIdTypes(
        JsonElement element,
        List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics,
        string contractId,
        string path = "")
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nextPath = string.IsNullOrWhiteSpace(path) ? property.Name : path + "." + property.Name;
                if (property.Name is "scene_id" or "region_id" or "npc_id")
                {
                    if (property.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.Value.GetString()))
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidContractContent, $"{property.Name} must be a non-empty string when present.", nextPath, contractId));
                    }
                }
                else if (property.Name is "scene_ids" or "region_ids" or "npc_ids")
                {
                    if (property.Value.ValueKind != JsonValueKind.Array
                        || property.Value.EnumerateArray().Any(item => item.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(item.GetString())))
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidArray, $"{property.Name} must be an array of non-empty strings when present.", nextPath, contractId));
                    }
                }

                ValidateReferenceIdTypes(property.Value, diagnostics, contractId, nextPath);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                ValidateReferenceIdTypes(item, diagnostics, contractId, $"{path}[{index}]");
                index++;
            }
        }
    }

    private static void ValidateIds(JsonElement element, List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics, string contractId, string path = "")
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var nextPath = string.IsNullOrWhiteSpace(path) ? property.Name : path + "." + property.Name;
                if (property.NameEquals("id") && property.Value.ValueKind == JsonValueKind.String)
                {
                    var id = property.Value.GetString() ?? string.Empty;
                    if (!LowerSlashIdPattern.IsMatch(id))
                    {
                        diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.InvalidId, $"Id '{id}' must be a lowercase slash id.", nextPath, contractId));
                    }
                }

                ValidateIds(property.Value, diagnostics, contractId, nextPath);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                ValidateIds(item, diagnostics, contractId, $"{path}[{index}]");
                index++;
            }
        }
    }

    private static bool RequireString(JsonElement element, string propertyName, List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics, string contractId)
    {
        return RequireString(element, propertyName, diagnostics, contractId, out _);
    }

    private static bool RequireString(JsonElement element, string propertyName, List<GeneratorPlanStrictLlmArtifactDiagnostic> diagnostics, string contractId, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
        {
            diagnostics.Add(Diagnostic(GeneratorPlanPreviewDiagnosticSeverity.Error, GeneratorPlanStrictLlmArtifactDiagnosticCodes.MissingField, $"Required string field '{propertyName}' is missing or empty.", propertyName, contractId));
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return true;
    }

    private static bool HasNonEmptyString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString());
    }

    private static bool HasNonEmptyArray(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Array
            && property.GetArrayLength() > 0;
    }

    private static GeneratorPlanStrictLlmArtifactDiagnostic Diagnostic(string severity, string code, string message, string target, string contractId)
    {
        return new GeneratorPlanStrictLlmArtifactDiagnostic
        {
            Severity = severity,
            Code = code,
            Message = message,
            Target = target,
            ContractId = contractId
        };
    }

    private static int SeverityOrder(string severity)
    {
        return severity switch
        {
            GeneratorPlanPreviewDiagnosticSeverity.Error => 0,
            GeneratorPlanPreviewDiagnosticSeverity.Warning => 1,
            GeneratorPlanPreviewDiagnosticSeverity.Info => 2,
            _ => 3
        };
    }
}
