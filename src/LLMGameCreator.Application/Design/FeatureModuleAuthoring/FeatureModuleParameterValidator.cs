using System.Globalization;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleParameterValidator
{
    public FeatureModuleParameterValidationResult Validate(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        IReadOnlyList<FeatureModuleParameterValue> suppliedValues)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(selectedModuleIds);
        ArgumentNullException.ThrowIfNull(suppliedValues);
        var diagnostics = new List<string>();
        var selected = selectedModuleIds.ToHashSet(StringComparer.Ordinal);
        var allDefinitions = catalog.Modules.SelectMany(module => module.ParameterDefinitions)
            .ToDictionary(Key, StringComparer.Ordinal);
        var selectedDefinitions = catalog.Modules.Where(module => selected.Contains(module.ModuleId))
            .SelectMany(module => module.ParameterDefinitions).OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ThenBy(item => item.ParameterId, StringComparer.Ordinal).ToList();
        var suppliedGroups = suppliedValues.GroupBy(Key, StringComparer.Ordinal).ToList();
        foreach (var duplicate in suppliedGroups.Where(group => group.Count() > 1))
            diagnostics.Add("duplicate parameter rejected: " + duplicate.Key);

        foreach (var supplied in suppliedGroups.Select(group => group.First()))
        {
            var key = Key(supplied);
            if (!allDefinitions.TryGetValue(key, out var definition))
                diagnostics.Add("unknown parameter rejected: " + key);
            else if (!selected.Contains(definition.ModuleId))
                diagnostics.Add("unselected module parameter rejected: " + key);
        }

        var suppliedByKey = suppliedGroups.Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var effective = new List<FeatureModuleResolvedParameterValue>();
        foreach (var definition in selectedDefinitions)
        {
            var key = Key(definition);
            var usedDefault = !suppliedByKey.TryGetValue(key, out var supplied);
            var value = usedDefault ? definition.DefaultValue : supplied!.Value;
            if (usedDefault && definition.Required && value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                diagnostics.Add("required parameter has no default: " + key);
                continue;
            }
            if (!ValidateValue(definition, value, diagnostics)) continue;
            effective.Add(new FeatureModuleResolvedParameterValue
            {
                ModuleId = definition.ModuleId,
                ParameterId = definition.ParameterId,
                ValueType = definition.ValueType,
                Value = value.Clone(),
                UsedDefault = usedDefault,
                BoundOperationIds = definition.Bindings.Select(binding => binding.OperationId)
                    .OrderBy(id => id, StringComparer.Ordinal).ToList(),
                AtomicGroupId = definition.AtomicGroupId
            });
        }
        return new FeatureModuleParameterValidationResult
        {
            Passed = diagnostics.Count == 0,
            EffectiveValues = effective.OrderBy(item => item.ModuleId, StringComparer.Ordinal)
                .ThenBy(item => item.ParameterId, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };
    }

    private static bool ValidateValue(
        FeatureModuleParameterDefinition definition,
        JsonElement value,
        List<string> diagnostics)
    {
        var key = Key(definition);
        decimal? numeric = null;
        var typeValid = definition.ValueType switch
        {
            FeatureModuleParameterValueTypes.Integer => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
            FeatureModuleParameterValueTypes.Number => value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out _),
            FeatureModuleParameterValueTypes.Boolean => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            FeatureModuleParameterValueTypes.Enum => value.ValueKind == JsonValueKind.String,
            _ => false
        };
        if (!typeValid)
        {
            diagnostics.Add("wrong parameter type rejected: " + key);
            return false;
        }
        if (value.ValueKind == JsonValueKind.Number) numeric = value.GetDecimal();
        if (numeric.HasValue && (definition.Minimum.HasValue && numeric < definition.Minimum
                                 || definition.Maximum.HasValue && numeric > definition.Maximum))
        {
            diagnostics.Add("parameter range violation rejected: " + key);
            return false;
        }
        if (numeric.HasValue && definition.Step is > 0)
        {
            var origin = definition.Minimum ?? 0m;
            if ((numeric.Value - origin) % definition.Step.Value != 0)
            {
                diagnostics.Add("parameter step violation rejected: " + key);
                return false;
            }
        }
        if (definition.ValueType == FeatureModuleParameterValueTypes.Enum
            && !definition.AllowedValues.Contains(value.GetString() ?? string.Empty, StringComparer.Ordinal))
        {
            diagnostics.Add("invalid enum rejected: " + key);
            return false;
        }
        return true;
    }

    public static string CanonicalValue(JsonElement value, string valueType) => valueType switch
    {
        FeatureModuleParameterValueTypes.Integer => value.GetInt64().ToString(CultureInfo.InvariantCulture),
        FeatureModuleParameterValueTypes.Number => value.GetDecimal().ToString("0.############################", CultureInfo.InvariantCulture),
        FeatureModuleParameterValueTypes.Boolean => value.GetBoolean().ToString().ToLowerInvariant(),
        FeatureModuleParameterValueTypes.Enum => value.GetString() ?? string.Empty,
        _ => throw new InvalidOperationException("Unsupported parameter value type: " + valueType)
    };

    private static string Key(FeatureModuleParameterDefinition value) => value.ModuleId + "|" + value.ParameterId;
    private static string Key(FeatureModuleParameterValue value) => value.ModuleId + "|" + value.ParameterId;
}
