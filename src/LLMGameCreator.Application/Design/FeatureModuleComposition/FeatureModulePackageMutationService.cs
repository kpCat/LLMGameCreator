using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public static class FeatureModulePackageMutationTargetKinds
{
    public const string ItemMetadataNumeric = "item_metadata_numeric";
    public const string StatDefaultValue = "stat_default_value";
    public const string EncounterParticipantStatAmount = "encounter_participant_stat_amount";
    public const string AbilityMetadataString = "ability_metadata_string";
    public const string AbilityMetadataNumeric = "ability_metadata_numeric";
    public const string ProgressionStageRequiredAmount = "progression_stage_required_amount";
    public const string DefinitionUpsert = "definition_upsert";
    public const string DefinitionNumericProperty = "definition_numeric_property";
    public const string EncounterParticipantAbilityReferenceUpsert = "encounter_participant_ability_reference_upsert";
    public const string EncounterParticipantResourceUpsert = "encounter_participant_resource_upsert";
    public const string EncounterParticipantUpsert = "encounter_participant_upsert";
    public const string AbilityCostUpsert = "ability_cost_upsert";
    public const string AbilityEffectUpsert = "ability_effect_upsert";
    public const string AbilityEffectAmount = "ability_effect_amount";
    public const string EncounterParticipantResourceAmount = "encounter_participant_resource_amount";
    public const string AbilityCostAmount = "ability_cost_amount";
    public const string StatusEffectAmount = "status_effect_amount";

    public static IReadOnlySet<string> Supported { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        ItemMetadataNumeric, StatDefaultValue, EncounterParticipantStatAmount,
        AbilityMetadataString, AbilityMetadataNumeric, ProgressionStageRequiredAmount,
        DefinitionUpsert, DefinitionNumericProperty, EncounterParticipantAbilityReferenceUpsert, EncounterParticipantResourceUpsert, EncounterParticipantUpsert,
        AbilityCostUpsert, AbilityEffectUpsert, AbilityEffectAmount, EncounterParticipantResourceAmount,
        AbilityCostAmount, StatusEffectAmount
    };
}

public sealed class FeatureModulePackageMutationService
{
    public const string ExpectedMissing = "__MISSING__";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private readonly IReadOnlyDictionary<string, Func<JsonObject, ProductLineRuntimeVariantMutationOperation,
        ProductLineRuntimeVariantMutationAuditEntry>> _handlers;

    public FeatureModulePackageMutationService()
    {
        _handlers = new Dictionary<string, Func<JsonObject, ProductLineRuntimeVariantMutationOperation,
            ProductLineRuntimeVariantMutationAuditEntry>>(StringComparer.Ordinal)
        {
            [FeatureModulePackageMutationTargetKinds.ItemMetadataNumeric] =
                (root, operation) => ApplyMetadata(root, operation, "items", "Item", true),
            [FeatureModulePackageMutationTargetKinds.StatDefaultValue] = ApplyStatDefaultValue,
            [FeatureModulePackageMutationTargetKinds.EncounterParticipantStatAmount] = ApplyEncounterParticipantStatAmount,
            [FeatureModulePackageMutationTargetKinds.AbilityMetadataString] =
                (root, operation) => ApplyMetadata(root, operation, "abilities", "Ability", false),
            [FeatureModulePackageMutationTargetKinds.AbilityMetadataNumeric] =
                (root, operation) => ApplyMetadata(root, operation, "abilities", "Ability", true),
            [FeatureModulePackageMutationTargetKinds.ProgressionStageRequiredAmount] = ApplyProgressionStageRequiredAmount
            , [FeatureModulePackageMutationTargetKinds.DefinitionUpsert] = ApplyDefinitionUpsert
            , [FeatureModulePackageMutationTargetKinds.DefinitionNumericProperty] = ApplyDefinitionNumericProperty
            , [FeatureModulePackageMutationTargetKinds.EncounterParticipantAbilityReferenceUpsert] = ApplyParticipantAbilityUpsert
            , [FeatureModulePackageMutationTargetKinds.EncounterParticipantResourceUpsert] = ApplyParticipantResourceUpsert
            , [FeatureModulePackageMutationTargetKinds.EncounterParticipantUpsert] = ApplyParticipantUpsert
            , [FeatureModulePackageMutationTargetKinds.AbilityCostUpsert] = ApplyAbilityCostUpsert
            , [FeatureModulePackageMutationTargetKinds.AbilityEffectUpsert] = ApplyAbilityEffectUpsert
            , [FeatureModulePackageMutationTargetKinds.AbilityEffectAmount] = ApplyAbilityEffectAmount
            , [FeatureModulePackageMutationTargetKinds.EncounterParticipantResourceAmount] = ApplyParticipantResourceAmount
            , [FeatureModulePackageMutationTargetKinds.AbilityCostAmount] = ApplyAbilityCostAmount
            , [FeatureModulePackageMutationTargetKinds.StatusEffectAmount] = ApplyStatusEffectAmount
        };
    }

    public FeatureModuleItemMetadataMutationResult Apply(
        string packageJson,
        IReadOnlyList<ProductLineRuntimeVariantMutationOperation> operations)
    {
        if (operations.Count == 0)
            return new FeatureModuleItemMetadataMutationResult { PackageJson = packageJson, Passed = true };
        var root = JsonNode.Parse(packageJson)?.AsObject()
                   ?? throw new InvalidOperationException("Package JSON root must be an object.");
        var entries = new List<ProductLineRuntimeVariantMutationAuditEntry>();
        foreach (var operation in operations.OrderBy(item => item.OperationId, StringComparer.Ordinal))
        {
            entries.Add(_handlers.TryGetValue(operation.TargetKind, out var handler)
                ? handler(root, operation)
                : Entry(operation, string.Empty, false, false,
                    "Unsupported FeatureModule mutation target: " + operation.TargetKind));
        }

        var diagnostics = entries.Where(item => !item.Passed)
            .Select(item => item.OperationId + ":" + item.Diagnostic).ToList();
        return new FeatureModuleItemMetadataMutationResult
        {
            PackageJson = diagnostics.Count == 0 ? root.ToJsonString(JsonOptions) + Environment.NewLine : packageJson,
            Passed = diagnostics.Count == 0,
            Operations = entries,
            Diagnostics = diagnostics
        };
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyMetadata(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation,
        string collectionName,
        string targetLabel,
        bool numeric)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 2, targetLabel + " metadata target must be id|metadataKey.");
            var value = numeric ? Numeric(operation.NewValue) : operation.NewValue;
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(targetLabel + " metadata value is required.");
            var target = ExactlyOne(Collection(root, collectionName), parts[0], targetLabel);
            var metadata = target["metadata"] as JsonObject;
            var existing = metadata?[parts[1]];
            actual = NodeValue(existing);
            ValidateExpected(operation, actual, existing is null);
            metadata ??= new JsonObject();
            target["metadata"] = metadata;
            metadata[parts[1]] = value;
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyStatDefaultValue(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 1, "Stat target must contain exactly one id.");
            var stat = ExactlyOne(Collection(root, "stats"), parts[0], "Stat");
            actual = SetNumericProperty(stat, "defaultValue", operation);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyEncounterParticipantStatAmount(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 3,
                "Encounter participant stat target must be encounterId|participantId|statId.");
            var encounter = ExactlyOne(Collection(root, "encounters"), parts[0], "Encounter");
            var participant = ExactlyOne(RequiredArray(encounter, "participants"), parts[1], "Encounter participant");
            var stat = ExactlyOne(RequiredArray(participant, "stats"), parts[2], "Encounter participant stat");
            actual = SetNumericProperty(stat, "amount", operation);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyProgressionStageRequiredAmount(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 2, "Progression stage target must be progressionId|stageId.");
            var progression = ExactlyOne(Collection(root, "progressions"), parts[0], "Progression");
            var stage = ExactlyOne(RequiredArray(progression, "stages"), parts[1], "Progression stage");
            actual = SetNumericProperty(stage, "requiredAmount", operation);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyDefinitionUpsert(JsonObject root, ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = ExpectedMissing;
        try
        {
            var parts = Parts(operation, 2, "Definition upsert target must be collection|id.");
            if (parts[0] is not ("abilities" or "resources" or "statuses"))
                throw new InvalidOperationException("Definition upsert collection is unsupported: " + parts[0]);
            var payload = ParseObject(operation.NewValue, "Definition upsert payload must be a JSON object.");
            if (payload["id"]?.GetValue<string>() != parts[1])
                throw new InvalidOperationException("Definition upsert payload ID must match target ID.");
            var collection = Collection(root, parts[0]);
            var matches = collection.OfType<JsonObject>().Where(item => item["id"]?.GetValue<string>() == parts[1]).ToList();
            if (matches.Count > 1) throw new InvalidOperationException("Duplicate definition IDs are not allowed: " + parts[1]);
            if (matches.Count == 1)
            {
                actual = matches[0].ToJsonString();
                if (!JsonNode.DeepEquals(matches[0], payload))
                    throw new InvalidOperationException("Conflicting definition upsert rejected: " + parts[1]);
                return Entry(operation, actual, false, true, string.Empty);
            }
            collection.Add(payload);
            SortById(collection);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or JsonException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyDefinitionNumericProperty(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 3, "Definition numeric property target must be collection|id|property.");
            if (parts[0] is not ("abilities" or "resources" or "statuses"))
                throw new InvalidOperationException("Definition numeric property collection is unsupported: " + parts[0]);
            if (string.IsNullOrWhiteSpace(parts[2]))
                throw new InvalidOperationException("Definition numeric property name is required.");
            var definition = ExactlyOne(Collection(root, parts[0]), parts[1], "Definition");
            actual = SetNumericProperty(definition, parts[2], operation);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyParticipantAbilityUpsert(JsonObject root, ProductLineRuntimeVariantMutationOperation operation)
    {
        try
        {
            var parts = Parts(operation, 2, "Participant ability target must be encounterId|participantId.");
            var participant = Participant(root, parts[0], parts[1]);
            var abilities = participant["abilities"] as JsonArray ?? new JsonArray();
            participant["abilities"] = abilities;
            var present = abilities.Any(item => item?.GetValue<string>() == operation.NewValue);
            if (!present) abilities.Add(operation.NewValue);
            SortStrings(abilities);
            return Entry(operation, present ? operation.NewValue : ExpectedMissing, !present, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException)
        {
            return Entry(operation, string.Empty, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyParticipantResourceUpsert(JsonObject root, ProductLineRuntimeVariantMutationOperation operation) =>
        ApplyChildObjectUpsert(root, operation, "participant_resource", (parts) => RequiredArray(Participant(root, parts[0], parts[1]), "resources"), 3);

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyParticipantUpsert(JsonObject root, ProductLineRuntimeVariantMutationOperation operation) =>
        ApplyChildObjectUpsert(root, operation, "encounter_participant",
            parts => RequiredArray(ExactlyOne(Collection(root, "encounters"), parts[0], "Encounter"), "participants"), 2);

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyAbilityCostUpsert(JsonObject root, ProductLineRuntimeVariantMutationOperation operation) =>
        ApplyChildObjectUpsert(root, operation, "ability_cost", parts => EnsureArray(ExactlyOne(Collection(root, "abilities"), parts[0], "Ability"), "costs"), 2);

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyAbilityEffectUpsert(JsonObject root, ProductLineRuntimeVariantMutationOperation operation) =>
        ApplyChildObjectUpsert(root, operation, "ability_effect", parts => EnsureArray(ExactlyOne(Collection(root, "abilities"), parts[0], "Ability"), "effects"), 3, effectShape: true);

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyChildObjectUpsert(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation,
        string label,
        Func<string[], JsonArray> collectionFactory,
        int partCount,
        bool effectShape = false)
    {
        var actual = ExpectedMissing;
        try
        {
            var parts = Parts(operation, partCount, label + " target is invalid.");
            var payload = ParseObject(operation.NewValue, label + " payload must be a JSON object.");
            var collection = collectionFactory(parts);
            bool Match(JsonObject item) => effectShape
                ? item["type"]?.GetValue<string>() == parts[^2] && item["args"]?["id"]?.GetValue<string>() == parts[^1]
                : item["id"]?.GetValue<string>() == parts[^1];
            var matches = collection.OfType<JsonObject>().Where(Match).ToList();
            if (matches.Count > 1) throw new InvalidOperationException("Duplicate child definitions rejected: " + operation.TargetId);
            if (matches.Count == 1)
            {
                actual = matches[0].ToJsonString();
                if (!JsonNode.DeepEquals(matches[0], payload))
                    throw new InvalidOperationException("Conflicting child upsert rejected: " + operation.TargetId);
                return Entry(operation, actual, false, true, string.Empty);
            }
            collection.Add(payload);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or JsonException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyAbilityEffectAmount(JsonObject root, ProductLineRuntimeVariantMutationOperation operation) =>
        ApplyEffectAmount(operation, EnsureArray(ExactlyOne(Collection(root, "abilities"), Parts(operation, 3, "Ability effect amount target is invalid.")[0], "Ability"), "effects"));

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyStatusEffectAmount(JsonObject root, ProductLineRuntimeVariantMutationOperation operation) =>
        ApplyEffectAmount(operation, EnsureArray(ExactlyOne(Collection(root, "statuses"), Parts(operation, 3, "Status effect amount target is invalid.")[0], "Status"), "effects"));

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyEffectAmount(ProductLineRuntimeVariantMutationOperation operation, JsonArray effects)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 3, "Effect amount target must be ownerId|effectKind|effectId.");
            var effect = effects.OfType<JsonObject>().SingleOrDefault(item => item["type"]?.GetValue<string>() == parts[1]
                && item["args"]?["id"]?.GetValue<string>() == parts[2])
                ?? throw new InvalidOperationException("Expected exactly one effect: " + operation.TargetId);
            var args = effect["args"]?.AsObject() ?? throw new InvalidOperationException("Effect args were not found.");
            var existing = args["amount"];
            actual = NodeValue(existing);
            ValidateExpected(operation, actual, existing is null);
            args["amount"] = Numeric(operation.NewValue);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyParticipantResourceAmount(JsonObject root, ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 3, "Participant resource amount target is invalid.");
            var resource = ExactlyOne(RequiredArray(Participant(root, parts[0], parts[1]), "resources"), parts[2], "Participant resource");
            actual = SetNumericProperty(resource, "amount", operation);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static ProductLineRuntimeVariantMutationAuditEntry ApplyAbilityCostAmount(JsonObject root, ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            var parts = Parts(operation, 2, "Ability cost amount target is invalid.");
            var cost = ExactlyOne(EnsureArray(ExactlyOne(Collection(root, "abilities"), parts[0], "Ability"), "costs"), parts[1], "Ability cost");
            actual = SetNumericProperty(cost, "amount", operation);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
    }

    private static JsonObject Participant(JsonObject root, string encounterId, string participantId) =>
        ExactlyOne(RequiredArray(ExactlyOne(Collection(root, "encounters"), encounterId, "Encounter"), "participants"), participantId, "Encounter participant");

    private static JsonArray EnsureArray(JsonObject parent, string name)
    {
        if (parent[name] is JsonArray array) return array;
        array = new JsonArray();
        parent[name] = array;
        return array;
    }

    private static JsonObject ParseObject(string json, string diagnostic) =>
        JsonNode.Parse(json) as JsonObject ?? throw new InvalidOperationException(diagnostic);

    private static void SortById(JsonArray array)
    {
        var ordered = array.OfType<JsonObject>().OrderBy(item => item["id"]?.GetValue<string>(), StringComparer.Ordinal).ToList();
        array.Clear();
        foreach (var item in ordered) array.Add(item);
    }

    private static void SortStrings(JsonArray array)
    {
        var ordered = array.Select(item => item?.GetValue<string>() ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToList();
        array.Clear();
        foreach (var item in ordered) array.Add(item);
    }

    private static string SetNumericProperty(
        JsonObject target,
        string propertyName,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var existing = target[propertyName];
        var actual = NodeValue(existing);
        ValidateExpected(operation, actual, existing is null);
        target[propertyName] = decimal.Parse(Numeric(operation.NewValue), NumberStyles.Number,
            CultureInfo.InvariantCulture);
        return actual;
    }

    private static JsonArray Collection(JsonObject root, string name) =>
        root["game"]?[name]?.AsArray()
        ?? throw new InvalidOperationException("Package game." + name + " was not found.");

    private static JsonArray RequiredArray(JsonObject parent, string name) =>
        parent[name]?.AsArray() ?? throw new InvalidOperationException("Required collection was not found: " + name);

    private static JsonObject ExactlyOne(JsonArray collection, string id, string label)
    {
        var matches = collection.OfType<JsonObject>()
            .Where(item => item["id"]?.GetValue<string>() == id).ToList();
        if (matches.Count != 1)
            throw new InvalidOperationException("Expected exactly one " + label.ToLowerInvariant()
                                                + " id=" + id + " but found " + matches.Count + ".");
        return matches[0];
    }

    private static string[] Parts(ProductLineRuntimeVariantMutationOperation operation, int count, string diagnostic)
    {
        var parts = operation.TargetId.Split('|');
        if (parts.Length != count || parts.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException(diagnostic);
        return parts;
    }

    private static string Numeric(string value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            throw new InvalidOperationException("FeatureModule numeric value is invalid: " + value);
        return parsed.ToString(CultureInfo.InvariantCulture);
    }

    private static string NodeValue(JsonNode? node)
    {
        if (node is null) return ExpectedMissing;
        if (node is JsonValue value && value.TryGetValue<string>(out var text)) return text;
        if (node is JsonValue numeric && numeric.TryGetValue<decimal>(out var number))
            return number.ToString(CultureInfo.InvariantCulture);
        return node.ToJsonString();
    }

    private static void ValidateExpected(
        ProductLineRuntimeVariantMutationOperation operation,
        string actual,
        bool missing)
    {
        if (!string.Equals(actual, operation.ExpectedValue, StringComparison.Ordinal))
            throw new InvalidOperationException("expected " + operation.ExpectedValue + " but found " + actual);
        if (missing && operation.ExpectedValue != ExpectedMissing)
            throw new InvalidOperationException("Missing value may be created only by an explicit expected-missing contract.");
    }

    private static ProductLineRuntimeVariantMutationAuditEntry Entry(
        ProductLineRuntimeVariantMutationOperation operation,
        string actual,
        bool applied,
        bool passed,
        string diagnostic) => new()
        {
            OperationId = operation.OperationId,
            TargetKind = operation.TargetKind,
            TargetId = operation.TargetId,
            JsonPath = operation.JsonPath,
            ExpectedValue = operation.ExpectedValue,
            ActualOldValue = actual,
            NewValue = operation.NewValue,
            RuntimeDimension = operation.RuntimeDimension,
            Applied = applied,
            Passed = passed,
            Diagnostic = diagnostic
        };
}
