using System.Globalization;
using System.Text.RegularExpressions;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleRuntimeEffectEvaluator
{
    public IReadOnlyList<FeatureModuleRuntimeEffectObservation> Evaluate(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        RuntimeInteractiveSession session,
        RuntimeInteractiveSession baselineSession)
    {
        ArgumentNullException.ThrowIfNull(selectedModules);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(baselineSession);
        return selectedModules.SelectMany(module => module.RuntimeEffectContracts)
            .OrderBy(contract => contract.EffectId, StringComparer.Ordinal)
            .Select(contract => Observe(contract, session, baselineSession))
            .ToList();
    }

    private static FeatureModuleRuntimeEffectObservation Observe(
        FeatureModuleRuntimeEffectContract contract,
        RuntimeInteractiveSession session,
        RuntimeInteractiveSession baselineSession)
    {
        var diagnostics = new List<string>();
        var actual = ReadMetric(contract, session, diagnostics);
        var baseline = ReadMetric(contract, baselineSession, diagnostics);
        var passed = diagnostics.Count == 0 && Compare(contract, actual, baseline);
        if (!passed && diagnostics.Count == 0)
            diagnostics.Add("runtime effect comparison failed");
        return new FeatureModuleRuntimeEffectObservation
        {
            EffectId = contract.EffectId,
            ModuleId = contract.ModuleId,
            MetricKind = contract.MetricKind,
            TargetId = contract.TargetId,
            ResourceOrItemId = contract.ResourceOrItemId,
            ComparisonKind = contract.ComparisonKind,
            ExpectedValue = contract.ExpectedValue,
            BaselineValue = baseline,
            ActualValue = actual,
            RuntimeDimension = contract.RuntimeDimension,
            Passed = passed,
            Diagnostics = diagnostics
        };
    }

    private static string ReadMetric(
        FeatureModuleRuntimeEffectContract contract,
        RuntimeInteractiveSession session,
        List<string> diagnostics)
    {
        var value = contract.MetricKind switch
        {
            FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity =>
                InventoryQuantity(session.LatestInventorySummary, contract.TargetId, contract.ResourceOrItemId),
            FeatureModuleRuntimeEffectMetricKinds.CombatResourceAmount =>
                CombatQuantity(session.LatestCombatSummary, contract.TargetId, contract.ResourceOrItemId),
            _ => null
        };
        if (value is null)
        {
            diagnostics.Add("unsupported or missing runtime effect metric: " + contract.MetricKind);
            return string.Empty;
        }
        return value.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static bool Compare(FeatureModuleRuntimeEffectContract contract, string actualText, string baselineText)
    {
        if (!int.TryParse(actualText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var actual)
            || !int.TryParse(baselineText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var baseline))
            return false;
        return contract.ComparisonKind switch
        {
            FeatureModuleRuntimeEffectComparisonKinds.GreaterThanBaseline => actual > baseline,
            FeatureModuleRuntimeEffectComparisonKinds.ChangedFromBaseline => actual != baseline,
            FeatureModuleRuntimeEffectComparisonKinds.Equal =>
                int.TryParse(contract.ExpectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected)
                && actual == expected,
            FeatureModuleRuntimeEffectComparisonKinds.AtLeast =>
                int.TryParse(contract.ExpectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minimum)
                && actual >= minimum,
            _ => false
        };
    }

    private static int? InventoryQuantity(string summary, string inventoryId, string itemId)
    {
        var inventory = summary.Split(';').FirstOrDefault(part =>
            part.TrimStart().StartsWith(inventoryId + "=", StringComparison.Ordinal));
        if (inventory is null) return null;
        var match = Regex.Match(inventory, Regex.Escape(itemId) + @":(?<value>\d+)");
        return match.Success
            ? int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture)
            : 0;
    }

    private static int? CombatQuantity(string summary, string participantId, string resourceId)
    {
        var match = Regex.Match(summary,
            Regex.Escape(participantId) + @"\[[^\]]*" + Regex.Escape(resourceId) + @"=(?<value>\d+)");
        return match.Success
            ? int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture)
            : null;
    }
}
