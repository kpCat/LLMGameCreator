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
        if (contract.MetricKind is FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta
                or FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta
            && session.CapabilityPlan?.OrderedActions.All(action => action.ActionId != "basic_attack") == true)
        {
            return new FeatureModuleRuntimeEffectObservation
            {
                EffectId = contract.EffectId,
                ModuleId = contract.ModuleId,
                MetricKind = contract.MetricKind,
                TargetId = contract.TargetId,
                ResourceOrItemId = contract.ResourceOrItemId,
                ComparisonKind = contract.ComparisonKind,
                ExpectedValue = contract.ExpectedValue,
                BaselineValue = "not_applicable",
                ActualValue = "not_applicable",
                RuntimeDimension = contract.RuntimeDimension,
                Passed = true,
                Diagnostics = ["combat capability absent; combat delta is not applicable"]
            };
        }
        var actual = ReadMetric(contract, session, diagnostics);
        var baseline = contract.MetricKind switch
        {
            FeatureModuleRuntimeEffectMetricKinds.EquipmentSlotItemEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta => "0",
            FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta => "0",
            FeatureModuleRuntimeEffectMetricKinds.PlayerStatEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.ProgressionAmountEquals => "0",
            FeatureModuleRuntimeEffectMetricKinds.ProgressionStageEquals => string.Empty,
            _ => ReadMetric(contract, baselineSession, diagnostics)
        };
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
                InventoryQuantity(session.LatestInventorySummary, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.CombatResourceAmount =>
                CombatStartingQuantity(session, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.EquipmentSlotItemEquals =>
                EquipmentItem(session.LatestEquipmentSummary, contract.TargetId),
            FeatureModuleRuntimeEffectMetricKinds.InventoryItemAbsentOrDecreased =>
                InventoryQuantity(session.LatestInventorySummary, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta =>
                EquipmentDamageDelta(session)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.PlayerStatEquals =>
                PlayerStat(session, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta =>
                CombatDamageEventValue(session, "statDamageBonus")?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.ProgressionAmountEquals =>
                Progression(session, contract.TargetId)?.Amount.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.ProgressionStageEquals =>
                Progression(session, contract.TargetId)?.StageId,
            _ => null
        };
        if (value is null)
        {
            diagnostics.Add("unsupported or missing runtime effect metric: " + contract.MetricKind);
            return string.Empty;
        }
        return value;
    }

    private static bool Compare(FeatureModuleRuntimeEffectContract contract, string actualText, string baselineText)
    {
        if (contract.ComparisonKind == FeatureModuleRuntimeEffectComparisonKinds.Equal)
        {
            return decimal.TryParse(contract.ExpectedValue, NumberStyles.Number, CultureInfo.InvariantCulture,
                out var expected)
                ? decimal.TryParse(actualText, NumberStyles.Number, CultureInfo.InvariantCulture, out var actualEqual)
                  && actualEqual == expected
                : string.Equals(actualText, contract.ExpectedValue, StringComparison.Ordinal);
        }
        if (!decimal.TryParse(actualText, NumberStyles.Number, CultureInfo.InvariantCulture, out var actual)
            || !decimal.TryParse(baselineText, NumberStyles.Number, CultureInfo.InvariantCulture, out var baseline)) return false;
        return contract.ComparisonKind switch
        {
            FeatureModuleRuntimeEffectComparisonKinds.GreaterThanBaseline => actual > baseline,
            FeatureModuleRuntimeEffectComparisonKinds.ChangedFromBaseline => actual != baseline,
            FeatureModuleRuntimeEffectComparisonKinds.AtLeast =>
                decimal.TryParse(contract.ExpectedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var minimum)
                && actual >= minimum,
            FeatureModuleRuntimeEffectComparisonKinds.LessThanBaseline => actual < baseline,
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

    private static decimal? CombatStartingQuantity(
        RuntimeInteractiveSession session,
        string participantId,
        string resourceId)
    {
        var remaining = CombatQuantity(session.LatestCombatSummary, participantId, resourceId);
        if (!remaining.HasValue) return null;
        var damage = session.LatestSnapshot.RuntimeEvents
            .Where(item => item.EventType == "DamageApplied" && item.TargetId == participantId)
            .Select(item => item.Args.TryGetValue("damage", out var raw)
                ? raw
                : Regex.Match(item.Message, @"-(?<value>\d+(?:\.\d+)?)$").Groups["value"].Value)
            .Select(raw => decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0)
            .Sum();
        return remaining.Value + damage;
    }

    private static string? EquipmentItem(string summary, string slotId)
    {
        var entry = summary.Split(';').Select(part => part.Trim())
            .SingleOrDefault(part => part.StartsWith(slotId + ":", StringComparison.Ordinal));
        return entry is null ? null : entry[(slotId.Length + 1)..];
    }

    private static decimal? EquipmentDamageDelta(RuntimeInteractiveSession session)
    {
        var value = session.LatestSnapshot.RuntimeEvents
            .Where(item => item.EventType == "DamageApplied")
            .Select(item => item.Args.TryGetValue("equipmentDamageBonus", out var raw) ? raw : null)
            .LastOrDefault(raw => raw is not null);
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed : null;
    }

    private static double? PlayerStat(RuntimeInteractiveSession session, string statId) =>
        session.CanonicalSession.RuntimeSession.GameplayState.Stats
            .SingleOrDefault(stat => stat.StatId == statId)?.Value;

    private static ProgressionState? Progression(RuntimeInteractiveSession session, string progressionId) =>
        session.CanonicalSession.RuntimeSession.GameplayState.Progressions
            .SingleOrDefault(progression => progression.ProgressionId == progressionId);

    private static decimal? CombatDamageEventValue(RuntimeInteractiveSession session, string key)
    {
        var value = session.LatestSnapshot.RuntimeEvents
            .Where(item => item.EventType == "DamageApplied")
            .Select(item => item.Args.GetValueOrDefault(key))
            .LastOrDefault(raw => !string.IsNullOrWhiteSpace(raw));
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed : null;
    }
}
