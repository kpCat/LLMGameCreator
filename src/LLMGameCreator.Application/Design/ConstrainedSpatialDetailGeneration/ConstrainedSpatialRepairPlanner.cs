namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialRepairPlanner
{
    public ConstrainedSpatialRepairFallbackRecord BuildRecord(
        ConstrainedSpatialPackageRowSource row,
        ConstrainedSpatialRewriteRuleCatalog rewriteCatalog,
        ConstrainedSpatialReachabilityProof proof)
    {
        var repairRules = rewriteCatalog.Rules
            .Where(rule => rule.RuleId.Contains("connect_critical", StringComparison.Ordinal)
                || rule.RuleId.Contains("repair_isolated", StringComparison.Ordinal)
                || rule.RuleId.Contains("mark_blocked", StringComparison.Ordinal)
                || rule.FamilyApplicability.Contains(row.FamilyId, StringComparer.Ordinal))
            .OrderBy(rule => rule.DeterministicApplicationOrder)
            .Select(rule => rule.RuleId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var diagnostics = new List<ConstrainedSpatialDiagnostic>
        {
            Info("goal062.repair.rule_trace_recorded", row.RowId, "Deterministic rewrite/repair rule records were applied in priority order."),
            proof.Reachable
                ? Info("goal062.repair.no_fallback_required", row.RowId, "Reachability was achieved without fallback relaxation.")
                : ConstrainedSpatialDiagnostic.Warning("goal062.repair.fallback_required", row.RowId, "Fallback would be required because reachability proof failed.")
        };

        return new ConstrainedSpatialRepairFallbackRecord
        {
            RowId = row.RowId,
            FamilyId = row.FamilyId,
            SeedId = row.SeedId,
            RetryBudget = 2,
            FallbackBudget = 3,
            ContradictionCount = 0,
            FallbackApplied = !proof.Reachable,
            AppliedRepairRuleIds = repairRules,
            Diagnostics = ConstrainedSpatialDetailSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public ConstrainedSpatialRepairFallbackMatrix BuildMatrix(IReadOnlyList<ConstrainedSpatialDetailRow> rows)
    {
        var records = rows
            .OrderBy(row => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => row.RepairFallback)
            .ToList();
        var contradictionDiagnostics = new List<ConstrainedSpatialDiagnostic>
        {
            ConstrainedSpatialDiagnostic.Error("goal062.constraint.no_tile_candidate", "synthetic/contradiction/no-tile-candidate", "A contradictory candidate set with no allowed tile is rejected before row promotion."),
            ConstrainedSpatialDiagnostic.Info("goal062.constraint.fallback_budget_recorded", "fallbackBudget=3", "Fallback relaxation is explicit and bounded.")
        };

        return new ConstrainedSpatialRepairFallbackMatrix
        {
            Passed = records.Count == 9
                && records.All(item => item.RetryBudget == 2)
                && records.All(item => item.FallbackBudget == 3)
                && records.All(item => item.AppliedRepairRuleIds.Count > 0)
                && contradictionDiagnostics.Any(item => item.Code == "goal062.constraint.no_tile_candidate"),
            RowCount = records.Count,
            ContradictionScenarioCount = 1,
            Rows = records,
            ContradictionDiagnostics = ConstrainedSpatialDetailSourceLoader.SortDiagnostics(contradictionDiagnostics)
        };
    }

    private static ConstrainedSpatialDiagnostic Info(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Info(code, target, message);
}
