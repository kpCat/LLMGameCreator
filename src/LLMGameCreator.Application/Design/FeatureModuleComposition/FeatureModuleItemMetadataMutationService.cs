using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed record FeatureModuleItemMetadataMutationResult
{
    public string PackageJson { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public IReadOnlyList<ProductLineRuntimeVariantMutationAuditEntry> Operations { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class FeatureModuleItemMetadataMutationService
{
    public const string TargetKind = "item_metadata_numeric";
    public const string ExpectedMissing = "__MISSING__";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

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
            entries.Add(Apply(root, operation));
        var diagnostics = entries.Where(item => !item.Passed)
            .Select(item => item.OperationId + ":" + item.Diagnostic).ToList();
        return new FeatureModuleItemMetadataMutationResult
        {
            PackageJson = root.ToJsonString(JsonOptions) + Environment.NewLine,
            Passed = diagnostics.Count == 0,
            Operations = entries,
            Diagnostics = diagnostics
        };
    }

    private static ProductLineRuntimeVariantMutationAuditEntry Apply(
        JsonObject root,
        ProductLineRuntimeVariantMutationOperation operation)
    {
        var actual = string.Empty;
        try
        {
            if (operation.TargetKind != TargetKind)
                throw new InvalidOperationException("Unsupported FeatureModule metadata mutation target: " + operation.TargetKind);
            var parts = operation.TargetId.Split('|');
            if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Item metadata target must be itemId|metadataKey.");
            if (!int.TryParse(operation.NewValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
                throw new InvalidOperationException("Item metadata numeric value is invalid: " + operation.NewValue);
            var items = root["game"]?["items"]?.AsArray()
                        ?? throw new InvalidOperationException("Package game.items was not found.");
            var matches = items.OfType<JsonObject>()
                .Where(item => item["id"]?.GetValue<string>() == parts[0]).ToList();
            if (matches.Count != 1)
                throw new InvalidOperationException("Expected exactly one item id=" + parts[0] + " but found " + matches.Count + ".");
            var item = matches[0];
            var metadata = item["metadata"] as JsonObject;
            var existing = metadata?[parts[1]];
            actual = existing?.GetValue<string>() ?? ExpectedMissing;
            if (!string.Equals(actual, operation.ExpectedValue, StringComparison.Ordinal))
                throw new InvalidOperationException("expected " + operation.ExpectedValue + " but found " + actual);
            if (existing is null && operation.ExpectedValue != ExpectedMissing)
                throw new InvalidOperationException("Missing metadata may be created only by an explicit expected-missing contract.");
            metadata ??= new JsonObject();
            item["metadata"] = metadata;
            metadata[parts[1]] = numericValue.ToString(CultureInfo.InvariantCulture);
            return Entry(operation, actual, true, true, string.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            return Entry(operation, actual, false, false, exception.Message);
        }
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
