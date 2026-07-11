using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;

public sealed class ProductLineRuntimeVariantMaterializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ProductLineRuntimeVariantMaterializationResult Materialize(
        string templateJson,
        ProductLineRuntimeVariantRecipe recipe,
        ProductLineRuntimeVariantMetadataContext? metadataContext = null)
    {
        var root = JsonNode.Parse(templateJson)?.AsObject()
                   ?? throw new InvalidOperationException("Template package JSON root must be an object.");
        ApplyCandidateMetadata(root, recipe, metadataContext);

        var entries = new List<ProductLineRuntimeVariantMutationAuditEntry>();
        var diagnostics = new List<string>();
        foreach (var operation in recipe.MutationOperations)
        {
            var entry = ApplyOperation(root, operation);
            entries.Add(entry);
            if (!entry.Passed)
            {
                diagnostics.Add(entry.OperationId + ":" + entry.Diagnostic);
            }
        }

        var audit = new ProductLineRuntimeVariantMutationAudit
        {
            CandidateId = recipe.CandidateId,
            RecipeId = recipe.RecipeId,
            VariantKind = recipe.VariantKind,
            RuntimeSignificant = recipe.RuntimeSignificant,
            OperationCount = entries.Count,
            Passed = entries.All(item => item.Passed),
            Operations = entries,
            Diagnostics = diagnostics
        };

        return new ProductLineRuntimeVariantMaterializationResult(
            root.ToJsonString(JsonOptions) + Environment.NewLine,
            audit);
    }

    private static void ApplyCandidateMetadata(
        JsonObject root,
        ProductLineRuntimeVariantRecipe recipe,
        ProductLineRuntimeVariantMetadataContext? context)
    {
        var manifest = Object(root, "manifest");
        var generated = Object(root, "generatedContent");
        var profile = Object(generated, "profile");
        if (context is not null)
        {
            manifest["version"] = context.VersionSuffix;
            manifest["description"] = context.ManifestDescription;
            profile["title"] = context.ProfileTitle;
            profile["description"] = context.ProfileDescription;
            profile["genre"] = context.Genre;
            profile["tone"] = context.Tone;
            profile["presentationMode"] = context.PresentationMode;
            profile["worldTopology"] = context.WorldTopology;
            profile["actorModel"] = context.ActorModel;
            profile["combatModel"] = context.CombatModel;
            profile["sourceContextJson"] = context.SourceContext;
            return;
        }

        manifest["version"] = "0.1.142-" + recipe.RecipeId.Replace('_', '-');
        manifest["description"] = recipe.DisplayName + " Goal142 runtime-significant variant.";
        profile["title"] = recipe.DisplayName;
        profile["description"] = "Goal142 product-line runtime variant candidate.";
        profile["genre"] = "runtime-variant";
        profile["tone"] = recipe.VariantKind;
        profile["presentationMode"] = "canonical-runtime";
        profile["worldTopology"] = "minimal-map-vertical-slice";
        profile["actorModel"] = "package-runtime";
        profile["combatModel"] = "turn-based-encounter";
        profile["sourceContextJson"] = JsonSerializer.Serialize(new
        {
            goalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
            recipeId = recipe.RecipeId,
            candidateId = recipe.CandidateId,
            variantKind = recipe.VariantKind,
            runtimeSignificant = recipe.RuntimeSignificant,
            requiredAnchors = recipe.RequiredAnchors
        }, JsonOptions);
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyOperation(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        try
        {
            var node = ResolveTargetValue(root, operation);
            var oldValue = NodeValue(node);
            if (!string.Equals(oldValue, operation.ExpectedValue, StringComparison.Ordinal))
            {
                return Entry(operation, oldValue, applied: false, passed: false,
                    "expected " + operation.ExpectedValue + " but found " + oldValue);
            }

            var updated = ReplaceValue(node, operation.NewValue);
            return Entry(operation, oldValue, applied: true,
                passed: string.Equals(updated, operation.NewValue, StringComparison.Ordinal),
                string.Equals(updated, operation.NewValue, StringComparison.Ordinal)
                    ? string.Empty
                    : "write verification failed; found " + updated);
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            return Entry(operation, string.Empty, applied: false, passed: false, ex.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry Entry(
        ProductLineRuntimeVariantMutationOperation operation,
        string oldValue,
        bool applied,
        bool passed,
        string diagnostic) =>
        new()
        {
            OperationId = operation.OperationId,
            TargetKind = operation.TargetKind,
            TargetId = operation.TargetId,
            JsonPath = operation.JsonPath,
            ExpectedValue = operation.ExpectedValue,
            ActualOldValue = oldValue,
            NewValue = operation.NewValue,
            RuntimeDimension = operation.RuntimeDimension,
            Applied = applied,
            Passed = passed,
            Diagnostic = diagnostic
        };

    private static JsonNode ResolveTargetValue(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var parts = operation.TargetId.Split('|');
        return operation.TargetKind switch
        {
            "inventory_stack_amount" => InventoryStack(root, parts, "amount"),
            "recipe_output_amount" => RecipeEntry(root, "outputs", parts, "amount"),
            "encounter_participant_resource_amount" => EncounterParticipantResource(root, parts, "amount"),
            "ability_power" => ObjectById(Array(root, "game", "abilities"), operation.TargetId)["power"]
                               ?? throw Missing(operation),
            "ability_effect_arg_amount" => AbilityEffectArg(root, parts, "amount"),
            "loot_entry_min_count" => LootEntry(root, parts, "minCount"),
            "loot_entry_max_count" => LootEntry(root, parts, "maxCount"),
            "resource_node_production_amount" => ResourceNodeProduction(root, parts, "amount"),
            "transaction_output_amount" => TransactionEntry(root, "outputs", parts, "amount"),
            _ => throw new InvalidOperationException("Unsupported mutation target kind: " + operation.TargetKind)
        };
    }

    private static JsonNode InventoryStack(JsonObject root, IReadOnlyList<string> parts, string property)
    {
        RequireParts(parts, 2);
        var inventory = ObjectById(Array(root, "game", "inventories"), parts[0]);
        var stack = ObjectByProperty(Array(inventory, "stacks"), "itemId", parts[1]);
        return stack[property] ?? throw Missing(property, parts[0] + "|" + parts[1]);
    }

    private static JsonNode RecipeEntry(
        JsonObject root,
        string collection,
        IReadOnlyList<string> parts,
        string property)
    {
        RequireParts(parts, 2);
        var recipe = ObjectById(Array(root, "game", "recipes"), parts[0]);
        var output = ObjectByProperty(Array(recipe, collection), "id", parts[1]);
        return output[property] ?? throw Missing(property, string.Join("|", parts));
    }

    private static JsonNode EncounterParticipantResource(
        JsonObject root,
        IReadOnlyList<string> parts,
        string property)
    {
        RequireParts(parts, 3);
        var encounter = ObjectById(Array(root, "game", "encounters"), parts[0]);
        var participant = ObjectById(Array(encounter, "participants"), parts[1]);
        var resource = ObjectByProperty(Array(participant, "resources"), "id", parts[2]);
        return resource[property] ?? throw Missing(property, string.Join("|", parts));
    }

    private static JsonNode AbilityEffectArg(JsonObject root, IReadOnlyList<string> parts, string property)
    {
        RequireParts(parts, 3);
        var ability = ObjectById(Array(root, "game", "abilities"), parts[0]);
        var effect = Array(ability, "effects").OfType<JsonObject>()
            .SingleOrDefault(item =>
                StringProperty(item, "type") == parts[1]
                && item["args"] is JsonObject args
                && StringProperty(args, "id") == parts[2])
            ?? throw Missing("effect", string.Join("|", parts));
        var effectArgs = effect["args"]?.AsObject()
                         ?? throw Missing("args", string.Join("|", parts));
        return effectArgs[property] ?? throw Missing(property, string.Join("|", parts));
    }

    private static JsonNode LootEntry(JsonObject root, IReadOnlyList<string> parts, string property)
    {
        RequireParts(parts, 2);
        var table = ObjectById(Array(root, "game", "lootTables"), parts[0]);
        var entry = ObjectById(Array(table, "entries"), parts[1]);
        return entry[property] ?? throw Missing(property, string.Join("|", parts));
    }

    private static JsonNode ResourceNodeProduction(JsonObject root, IReadOnlyList<string> parts, string property)
    {
        RequireParts(parts, 2);
        var node = ObjectById(Array(root, "game", "resourceNodes"), parts[0]);
        var production = ObjectByProperty(Array(node, "production"), "id", parts[1]);
        return production[property] ?? throw Missing(property, string.Join("|", parts));
    }

    private static JsonNode TransactionEntry(
        JsonObject root,
        string collection,
        IReadOnlyList<string> parts,
        string property)
    {
        RequireParts(parts, 2);
        var transaction = ObjectById(Array(root, "game", "transactions"), parts[0]);
        var output = ObjectByProperty(Array(transaction, collection), "id", parts[1]);
        return output[property] ?? throw Missing(property, string.Join("|", parts));
    }

    private static JsonObject Object(JsonObject parent, string property)
    {
        if (parent[property] is JsonObject existing)
        {
            return existing;
        }

        var created = new JsonObject();
        parent[property] = created;
        return created;
    }

    private static JsonArray Array(JsonObject root, string objectProperty, string arrayProperty) =>
        Array(Object(root, objectProperty), arrayProperty);

    private static JsonArray Array(JsonObject parent, string property) =>
        parent[property] as JsonArray
        ?? throw Missing(property, "object");

    private static JsonObject ObjectById(JsonArray array, string id) =>
        ObjectByProperty(array, "id", id);

    private static JsonObject ObjectByProperty(JsonArray array, string property, string value)
    {
        var matches = array.OfType<JsonObject>()
            .Where(item => string.Equals(StringProperty(item, property), value, StringComparison.Ordinal))
            .ToList();
        if (matches.Count != 1)
        {
            throw new InvalidOperationException(
                "Expected exactly one " + property + "=" + value + " but found " + matches.Count + ".");
        }

        return matches[0];
    }

    private static string StringProperty(JsonObject obj, string property) =>
        obj[property]?.GetValue<string>() ?? string.Empty;

    private static string ReplaceValue(JsonNode node, string value)
    {
        if (node.Parent is not JsonObject parent)
        {
            throw new InvalidOperationException("Mutation target must be a JSON object property.");
        }

        var key = parent.First(item => ReferenceEquals(item.Value, node)).Key;
        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<int>(out _))
            {
                parent[key] = int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                return NodeValue(parent[key]!);
            }

            if (jsonValue.TryGetValue<double>(out _))
            {
                parent[key] = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                return NodeValue(parent[key]!);
            }

            if (jsonValue.TryGetValue<bool>(out _))
            {
                parent[key] = bool.Parse(value);
                return NodeValue(parent[key]!);
            }
        }

        parent[key] = value;
        return NodeValue(parent[key]!);
    }

    private static string NodeValue(JsonNode node)
    {
        if (node is JsonValue value)
        {
            if (value.TryGetValue<int>(out var intValue))
            {
                return intValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<double>(out var doubleValue))
            {
                return doubleValue.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<bool>(out var boolValue))
            {
                return boolValue.ToString().ToLowerInvariant();
            }

            if (value.TryGetValue<string>(out var stringValue))
            {
                return stringValue;
            }
        }

        return node.ToJsonString();
    }

    private static void RequireParts(IReadOnlyCollection<string> parts, int count)
    {
        if (parts.Count != count)
        {
            throw new InvalidOperationException("Mutation target id has invalid part count.");
        }
    }

    private static InvalidOperationException Missing(ProductLineRuntimeVariantMutationOperation operation) =>
        new("Mutation target was not found: " + operation.TargetKind + " " + operation.TargetId);

    private static InvalidOperationException Missing(string property, string target) =>
        new("Mutation target property was not found: " + property + " in " + target);
}

public sealed record ProductLineRuntimeVariantMaterializationResult(
    string PackageJson,
    ProductLineRuntimeVariantMutationAudit MutationAudit);
