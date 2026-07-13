using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.FeatureModuleComposition;

namespace LLMGameCreator.Application.Design.FeatureModuleAuthoring;

public sealed class FeatureModuleParameterConstraintEvaluator
{
    private static readonly Regex ParameterReference = new(@"\$\{parameter:([^}]+)\}", RegexOptions.CultureInvariant);

    public static bool ValidateDefinitions(
        FeatureModuleDefinition module,
        IReadOnlyDictionary<string, FeatureModuleDefinition> catalog,
        List<string> diagnostics)
    {
        var valid = true;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var semanticTargets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var constraint in module.ParameterConstraints)
        {
            var referencesValid = true;
            if (string.IsNullOrWhiteSpace(constraint.ConstraintId) || !ids.Add(constraint.ConstraintId))
            {
                diagnostics.Add("duplicate parameter constraint ID rejected: " + module.ModuleId + ":" + constraint.ConstraintId);
                valid = false;
            }
            if (!FeatureModuleParameterConstraintKinds.Supported.Contains(constraint.Kind))
            {
                diagnostics.Add("unsupported parameter constraint kind rejected: " + module.ModuleId + ":" + constraint.Kind);
                valid = false;
            }
            if (!FeatureModuleParameterConstraintOperators.Supported.Contains(constraint.Operator))
            {
                diagnostics.Add("unsupported parameter constraint operator rejected: " + module.ModuleId + ":" + constraint.Operator);
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(constraint.LeftExpression) || string.IsNullOrWhiteSpace(constraint.RightExpression)
                || string.IsNullOrWhiteSpace(constraint.DiagnosticCode) || string.IsNullOrWhiteSpace(constraint.Message))
            {
                diagnostics.Add("parameter constraint metadata missing: " + module.ModuleId + ":" + constraint.ConstraintId);
                valid = false;
            }
            if (!semanticTargets.Add(constraint.Kind + "|" + constraint.LeftExpression + "|" + constraint.Operator + "|" + constraint.RightExpression))
            {
                diagnostics.Add("duplicate parameter constraint target rejected: " + module.ModuleId + ":" + constraint.ConstraintId);
                valid = false;
            }
            foreach (var reference in References(module, constraint))
            {
                if (!catalog.TryGetValue(reference.ModuleId, out var referencedModule)
                    || !referencedModule.ParameterDefinitions.Any(parameter => parameter.ParameterId == reference.ParameterId))
                {
                    diagnostics.Add("unknown parameter constraint reference rejected: " + module.ModuleId + ":" + reference.DisplayId);
                    valid = false;
                    referencesValid = false;
                }
                else if (reference.ModuleId != module.ModuleId && !module.Dependencies.Contains(reference.ModuleId, StringComparer.Ordinal))
                {
                    diagnostics.Add("cross-module parameter constraint dependency rejected: " + module.ModuleId + "->" + reference.ModuleId);
                    valid = false;
                    referencesValid = false;
                }
            }

            if (!referencesValid) continue;
            try
            {
                var left = FeatureModuleEffectiveValueExpression.Evaluate(constraint.LeftExpression,
                    reference => ResolveDefaultParameterValue(module, catalog, reference));
                var right = FeatureModuleEffectiveValueExpression.Evaluate(constraint.RightExpression,
                    reference => ResolveDefaultParameterValue(module, catalog, reference));
                if (!Compare(left, constraint.Operator, right))
                {
                    diagnostics.Add("default parameter constraint rejected: " + module.ModuleId + ":"
                                    + constraint.ConstraintId + "; left=" + Format(left) + "; right=" + Format(right)
                                    + "; parameters=" + string.Join(",", References(module, constraint)
                                        .Select(reference => reference.DisplayId + "=" + Format(ResolveDefaultParameterValue(
                                            module, catalog, "parameter:" + reference.DisplayId)))
                                        .Distinct(StringComparer.Ordinal)));
                    valid = false;
                }
            }
            catch (InvalidOperationException exception)
            {
                diagnostics.Add("invalid parameter constraint expression rejected: " + module.ModuleId + ":"
                                + constraint.ConstraintId + ":" + exception.Message);
                valid = false;
            }
            catch (OverflowException)
            {
                diagnostics.Add("parameter constraint numeric overflow rejected: " + module.ModuleId + ":" + constraint.ConstraintId);
                valid = false;
            }
        }
        return valid;
    }

    public IReadOnlyList<string> Evaluate(
        FeatureModuleCatalogDocument catalog,
        IReadOnlyList<string> selectedModuleIds,
        IReadOnlyList<FeatureModuleResolvedParameterValue> values)
    {
        var selected = selectedModuleIds.ToHashSet(StringComparer.Ordinal);
        var diagnostics = new List<string>();
        var resolved = values.ToDictionary(value => value.ModuleId + "." + value.ParameterId, StringComparer.Ordinal);
        foreach (var module in catalog.Modules.Where(module => selected.Contains(module.ModuleId)))
        foreach (var constraint in module.ParameterConstraints.OrderBy(item => item.ConstraintId, StringComparer.Ordinal))
        {
            try
            {
                var references = References(module, constraint).ToList();
                decimal Resolve(string reference)
                {
                    if (!reference.StartsWith("parameter:", StringComparison.Ordinal))
                        throw new InvalidOperationException("unknown parameter constraint reference rejected: " + reference);
                    var parsed = ParseReference(module, reference["parameter:".Length..]);
                    if (!selected.Contains(parsed.ModuleId))
                        throw new InvalidOperationException("unselected parameter constraint reference rejected: " + parsed.DisplayId);
                    if (!resolved.TryGetValue(parsed.DisplayId, out var value))
                        throw new InvalidOperationException("unknown parameter constraint reference rejected: " + parsed.DisplayId);
                    if (value.ValueType is not FeatureModuleParameterValueTypes.Integer and not FeatureModuleParameterValueTypes.Number
                        || !decimal.TryParse(value.Value.GetRawText(), NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric))
                        throw new InvalidOperationException("nonnumeric parameter constraint expression rejected: " + parsed.DisplayId);
                    return numeric;
                }

                var left = FeatureModuleEffectiveValueExpression.Evaluate(constraint.LeftExpression, Resolve);
                var right = FeatureModuleEffectiveValueExpression.Evaluate(constraint.RightExpression, Resolve);
                if (Compare(left, constraint.Operator, right)) continue;
                diagnostics.Add(constraint.DiagnosticCode + ": " + constraint.Message + "; left="
                                + Format(left) + "; right=" + Format(right) + "; parameters="
                                + string.Join(",", references.Select(item => item.DisplayId).Distinct(StringComparer.Ordinal)
                                    .Select(id => id + "=" + Format(Resolve("parameter:" + id)))));
            }
            catch (InvalidOperationException exception) { diagnostics.Add(exception.Message); }
            catch (OverflowException) { diagnostics.Add("parameter constraint numeric overflow rejected: " + constraint.ConstraintId); }
        }
        return diagnostics;
    }

    private static bool Compare(decimal left, string op, decimal right) => op switch
    {
        "<" => left < right, "<=" => left <= right, "==" => left == right,
        "!=" => left != right, ">=" => left >= right, ">" => left > right,
        _ => false
    };

    private static IEnumerable<ParameterReferenceValue> References(FeatureModuleDefinition module, FeatureModuleParameterConstraint constraint) =>
        ParameterReference.Matches(constraint.LeftExpression + "\n" + constraint.RightExpression)
            .Select(match => ParseReference(module, match.Groups[1].Value));

    private static ParameterReferenceValue ParseReference(FeatureModuleDefinition owner, string reference)
    {
        var separator = reference.LastIndexOf(".", StringComparison.Ordinal);
        return separator < 0
            ? new ParameterReferenceValue(owner.ModuleId, reference)
            : new ParameterReferenceValue(reference[..separator], reference[(separator + 1)..]);
    }

    private static decimal ResolveDefaultParameterValue(
        FeatureModuleDefinition owner,
        IReadOnlyDictionary<string, FeatureModuleDefinition> catalog,
        string reference)
    {
        if (!reference.StartsWith("parameter:", StringComparison.Ordinal))
            throw new InvalidOperationException("unknown parameter constraint reference rejected: " + reference);
        var parsed = ParseReference(owner, reference["parameter:".Length..]);
        var parameter = catalog[parsed.ModuleId].ParameterDefinitions.Single(item => item.ParameterId == parsed.ParameterId);
        if (parameter.ValueType is not FeatureModuleParameterValueTypes.Integer and not FeatureModuleParameterValueTypes.Number
            || parameter.DefaultValue.ValueKind is not JsonValueKind.Number
            || !decimal.TryParse(parameter.DefaultValue.GetRawText(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            throw new InvalidOperationException("nonnumeric parameter constraint expression rejected: " + parsed.DisplayId);
        return value;
    }

    private static string Format(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);

    private sealed record ParameterReferenceValue(string ModuleId, string ParameterId)
    {
        public string DisplayId => ModuleId + "." + ParameterId;
    }
}
