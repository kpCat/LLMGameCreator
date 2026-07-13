using System.Globalization;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleParameterBindingService
{
    private readonly FeatureModuleParameterValidator _validator;
    private readonly FeatureModuleParameterConstraintEvaluator _constraints;

    public FeatureModuleParameterBindingService(
        FeatureModuleParameterValidator? validator = null,
        FeatureModuleParameterConstraintEvaluator? constraints = null)
    {
        _validator = validator ?? new FeatureModuleParameterValidator();
        _constraints = constraints ?? new FeatureModuleParameterConstraintEvaluator();
    }

    public FeatureModuleParameterBindingResult Bind(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        IReadOnlyList<FeatureModuleParameterValue> suppliedValues)
    {
        var validation = _validator.Validate(catalog, selectedModuleIds, suppliedValues);
        if (!validation.Passed) return new FeatureModuleParameterBindingResult { Diagnostics = validation.Diagnostics };
        var constraintDiagnostics = _constraints.Evaluate(catalog, selectedModuleIds, validation.EffectiveValues);
        if (constraintDiagnostics.Count > 0)
            return new FeatureModuleParameterBindingResult
            {
                EffectiveParameterValues = validation.EffectiveValues,
                Diagnostics = constraintDiagnostics
            };
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

        var declarativeBindings = selectedModules.SelectMany(module => module.EffectiveValueBindings)
            .OrderBy(binding => binding.BindingId, StringComparer.Ordinal).ToList();
        foreach (var duplicate in declarativeBindings.GroupBy(binding => binding.BindingId, StringComparer.Ordinal)
                     .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
            diagnostics.Add("duplicate effective binding ID rejected: " + duplicate.Key);
        foreach (var duplicate in declarativeBindings.GroupBy(TargetKey, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
            diagnostics.Add("duplicate binding target rejected: " + duplicate.Key);

        var effects = selectedModules.SelectMany(module => module.RuntimeEffectContracts)
            .GroupBy(effect => effect.EffectId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var actions = selectedModules.SelectMany(module => module.RuntimePlaythroughContracts)
            .GroupBy(action => action.ActionId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        foreach (var binding in declarativeBindings)
        {
            if (!FeatureModuleEffectiveValueBindingTargetKinds.Supported.Contains(binding.TargetKind))
            {
                diagnostics.Add("unsupported effective binding target kind rejected: " + binding.TargetKind);
                continue;
            }
            if (string.IsNullOrWhiteSpace(binding.ValueExpression))
                diagnostics.Add("empty effective binding expression rejected: " + binding.BindingId);
            switch (binding.TargetKind)
            {
                case FeatureModuleEffectiveValueBindingTargetKinds.MutationOperationField:
                    if (binding.TargetField != "newValue" || !byOperation.TryGetValue(binding.TargetId, out var operation)
                        || !Numeric(operation.NewValue))
                        diagnostics.Add("unknown mutation operation target or incompatible target field rejected: " + TargetKey(binding));
                    if (bindingOwners.ContainsKey(binding.TargetId + "|newValue"))
                        diagnostics.Add("duplicate binding target rejected: " + TargetKey(binding));
                    break;
                case FeatureModuleEffectiveValueBindingTargetKinds.RuntimeEffectExpectedValue:
                    if (binding.TargetField != "expectedValue" || !effects.TryGetValue(binding.TargetId, out var matchingEffects)
                        || matchingEffects.Count != 1 || !Numeric(matchingEffects[0].ExpectedValue))
                        diagnostics.Add("unknown Runtime effect target or incompatible target field rejected: " + TargetKey(binding));
                    break;
                case FeatureModuleEffectiveValueBindingTargetKinds.RuntimePlaythroughArg:
                    if (!actions.TryGetValue(binding.TargetId, out var matchingActions) || matchingActions.Count != 1
                        || !matchingActions[0].Args.TryGetValue(binding.TargetField, out var currentArg) || !Numeric(currentArg))
                        diagnostics.Add("unknown Runtime playthrough action target or incompatible target field rejected: " + TargetKey(binding));
                    break;
            }
        }
        if (diagnostics.Count > 0)
            return new FeatureModuleParameterBindingResult
            {
                EffectiveParameterValues = validation.EffectiveValues,
                Diagnostics = diagnostics
            };

        var initialOperations = operations.Select(operation => pending.TryGetValue(operation.OperationId, out var newValue)
                ? operation with { NewValue = newValue }
                : operation with { })
            .ToDictionary(operation => operation.OperationId, StringComparer.Ordinal);
        var parameterValues = validation.EffectiveValues.ToDictionary(
            value => value.ModuleId + "." + value.ParameterId,
            value => FeatureModuleParameterValidator.CanonicalValue(value.Value, value.ValueType),
            StringComparer.Ordinal);
        var mutationBindings = declarativeBindings
            .Where(binding => binding.TargetKind == FeatureModuleEffectiveValueBindingTargetKinds.MutationOperationField)
            .ToDictionary(binding => binding.TargetId, StringComparer.Ordinal);
        var resolving = new HashSet<string>(StringComparer.Ordinal);
        var resolvedOperationValues = new Dictionary<string, string>(StringComparer.Ordinal);

        string ResolveOperation(string operationId)
        {
            if (resolvedOperationValues.TryGetValue(operationId, out var resolved)) return resolved;
            if (!initialOperations.TryGetValue(operationId, out var operation))
                throw new InvalidOperationException("unknown mutation operation reference rejected: " + operationId);
            if (!mutationBindings.TryGetValue(operationId, out var binding)) return operation.NewValue;
            if (!resolving.Add(operationId))
                throw new InvalidOperationException("effective binding expression cycle rejected: " + operationId);
            try
            {
                resolved = Format(FeatureModuleEffectiveValueExpression.Evaluate(binding.ValueExpression, ResolveReference));
                resolvedOperationValues[operationId] = resolved;
                return resolved;
            }
            finally { resolving.Remove(operationId); }
        }

        decimal ResolveReference(string reference)
        {
            string raw;
            if (reference.StartsWith("parameter:", StringComparison.Ordinal))
            {
                var id = reference["parameter:".Length..];
                if (!parameterValues.TryGetValue(id, out raw!))
                    throw new InvalidOperationException("unknown or unselected parameter reference rejected: " + id);
            }
            else if (reference.StartsWith("operation:", StringComparison.Ordinal)
                     && reference.EndsWith(".newValue", StringComparison.Ordinal))
            {
                var id = reference["operation:".Length..^".newValue".Length];
                raw = ResolveOperation(id);
            }
            else throw new InvalidOperationException("unknown effective value reference rejected: " + reference);
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric))
                throw new InvalidOperationException("nonnumeric value in numeric expression rejected: " + reference);
            return numeric;
        }

        try
        {
            foreach (var binding in mutationBindings.Values) ResolveOperation(binding.TargetId);
            var resolvedBindingValues = declarativeBindings.ToDictionary(
                binding => binding.BindingId,
                binding => Format(FeatureModuleEffectiveValueExpression.Evaluate(binding.ValueExpression, ResolveReference)),
                StringComparer.Ordinal);
            var effectiveOperations = initialOperations.Values.Select(operation =>
                    operation with { NewValue = ResolveOperation(operation.OperationId) })
                .OrderBy(FeatureModuleCompositionValidator.TargetKey, StringComparer.Ordinal)
                .ThenBy(operation => operation.OperationId, StringComparer.Ordinal).ToList();
            var effectiveByOperation = effectiveOperations.ToDictionary(operation => operation.OperationId, StringComparer.Ordinal);
            var bindingByTarget = declarativeBindings.ToDictionary(TargetKey, StringComparer.Ordinal);
            var effectiveModules = catalog.Modules.Select(module => !selectedSet.Contains(module.ModuleId)
                ? module
                : module with
                {
                    MutationOperations = module.MutationOperations.Select(operation => effectiveByOperation[operation.OperationId]).ToList(),
                    RuntimeEffectContracts = module.RuntimeEffectContracts.Select(effect =>
                    {
                        var key = FeatureModuleEffectiveValueBindingTargetKinds.RuntimeEffectExpectedValue + "|"
                                  + effect.EffectId + "|expectedValue";
                        return bindingByTarget.TryGetValue(key, out var binding)
                            ? effect with { ExpectedValue = resolvedBindingValues[binding.BindingId] }
                            : effect with { };
                    }).ToList(),
                    RuntimePlaythroughContracts = module.RuntimePlaythroughContracts.Select(action =>
                    {
                        var args = new SortedDictionary<string, string>(
                            action.Args.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                            StringComparer.Ordinal);
                        foreach (var binding in declarativeBindings.Where(binding =>
                                     binding.TargetKind == FeatureModuleEffectiveValueBindingTargetKinds.RuntimePlaythroughArg
                                     && binding.TargetId == action.ActionId))
                            args[binding.TargetField] = resolvedBindingValues[binding.BindingId];
                        return action with { Args = args };
                    }).ToList()
                }).ToList();
            return new FeatureModuleParameterBindingResult
            {
                Passed = true,
                EffectiveMutationOperations = effectiveOperations,
                EffectiveParameterValues = validation.EffectiveValues,
                AppliedAtomicGroupIds = groups.OrderBy(value => value, StringComparer.Ordinal).ToList(),
                AppliedEffectiveValueBindingIds = declarativeBindings.Select(binding => binding.BindingId).ToList(),
                EffectiveCatalog = catalog with { Modules = effectiveModules }
            };
        }
        catch (InvalidOperationException exception)
        {
            return new FeatureModuleParameterBindingResult
            {
                EffectiveParameterValues = validation.EffectiveValues,
                Diagnostics = [exception.Message]
            };
        }
        catch (OverflowException)
        {
            return new FeatureModuleParameterBindingResult
            {
                EffectiveParameterValues = validation.EffectiveValues,
                Diagnostics = ["numeric expression overflow rejected"]
            };
        }
    }

    private static string TargetKey(FeatureModuleEffectiveValueBinding binding) =>
        binding.TargetKind + "|" + binding.TargetId + "|" + binding.TargetField;

    private static bool Numeric(string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);

    private static string Format(decimal value) =>
        value.ToString("0.############################", CultureInfo.InvariantCulture);

}
