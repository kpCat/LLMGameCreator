using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleParameterBindingService
{
    private readonly FeatureModuleParameterValidator _validator;

    public FeatureModuleParameterBindingService(FeatureModuleParameterValidator? validator = null)
    {
        _validator = validator ?? new FeatureModuleParameterValidator();
    }

    public FeatureModuleParameterBindingResult Bind(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        IReadOnlyList<FeatureModuleParameterValue> suppliedValues)
    {
        var validation = _validator.Validate(catalog, selectedModuleIds, suppliedValues);
        if (!validation.Passed) return new FeatureModuleParameterBindingResult { Diagnostics = validation.Diagnostics };
        var selectedSet = selectedModuleIds.ToHashSet(StringComparer.Ordinal);
        var selectedModules = catalog.Modules.Where(module => selectedSet.Contains(module.ModuleId)).ToList();
        var operations = selectedModules.SelectMany(module => module.MutationOperations)
            .Select(operation => operation with { }).ToList();
        var operationGroups = operations.GroupBy(operation => operation.OperationId, StringComparer.Ordinal).ToList();
        var diagnostics = new List<string>();
        foreach (var group in operationGroups.Where(group => group.Count() != 1))
            diagnostics.Add("bound operation ID must exist exactly once: " + group.Key);
        var byOperation = operationGroups.Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        var definitions = selectedModules.SelectMany(module => module.ParameterDefinitions)
            .ToDictionary(definition => definition.ModuleId + "|" + definition.ParameterId, StringComparer.Ordinal);
        var pending = new Dictionary<string, string>(StringComparer.Ordinal);
        var bindingOwners = new Dictionary<string, string>(StringComparer.Ordinal);
        var groups = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in validation.EffectiveValues)
        {
            var key = value.ModuleId + "|" + value.ParameterId;
            var definition = definitions[key];
            var canonical = FeatureModuleParameterValidator.CanonicalValue(value.Value, value.ValueType);
            foreach (var binding in definition.Bindings)
            {
                var target = binding.OperationId + "|" + binding.OperationField;
                if (!byOperation.ContainsKey(binding.OperationId))
                    diagnostics.Add("operation reference mismatch rejected: " + target);
                else if (binding.OperationField != "newValue" || binding.TransformKind != "identity")
                    diagnostics.Add("unsupported parameter binding rejected: " + target);
                else if (bindingOwners.TryGetValue(target, out var owner) && owner != key)
                    diagnostics.Add("conflicting parameter binding rejected: " + owner + " and " + key + " -> " + target);
                else
                {
                    bindingOwners[target] = key;
                    pending[binding.OperationId] = canonical;
                    if (!string.IsNullOrWhiteSpace(binding.AtomicGroupId)) groups.Add(binding.AtomicGroupId);
                }
            }
        }
        if (diagnostics.Count > 0)
            return new FeatureModuleParameterBindingResult
            {
                EffectiveParameterValues = validation.EffectiveValues,
                Diagnostics = diagnostics
            };

        var effective = operations.Select(operation => pending.TryGetValue(operation.OperationId, out var newValue)
                ? operation with { NewValue = newValue }
                : operation with { })
            .OrderBy(FeatureModuleCompositionValidator.TargetKey, StringComparer.Ordinal)
            .ThenBy(operation => operation.OperationId, StringComparer.Ordinal).ToList();
        return new FeatureModuleParameterBindingResult
        {
            Passed = true,
            EffectiveMutationOperations = effective,
            EffectiveParameterValues = validation.EffectiveValues,
            AppliedAtomicGroupIds = groups.OrderBy(value => value, StringComparer.Ordinal).ToList()
        };
    }
}
