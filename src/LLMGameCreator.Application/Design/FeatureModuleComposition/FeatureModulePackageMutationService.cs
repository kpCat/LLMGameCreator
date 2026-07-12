using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    public static IReadOnlySet<string> Supported { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        ItemMetadataNumeric, StatDefaultValue, EncounterParticipantStatAmount,
        AbilityMetadataString, AbilityMetadataNumeric, ProgressionStageRequiredAmount
    };
}

public sealed class FeatureModulePackageMutationService
{
    public const string ExpectedMissing = "__MISSING__";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
