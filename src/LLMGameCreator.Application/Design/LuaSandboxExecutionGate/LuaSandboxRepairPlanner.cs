namespace LLMGameCreator.Application.Design.LuaSandboxExecutionGate;

public sealed class LuaSandboxRepairPlanner
{
    public LuaSandboxRepairPlan Plan(LuaSandboxExecutionRequest request, LuaSandboxExecutionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        var blockingCodes = decision.Diagnostics
            .Where(item => item.Severity == "error" || item.Code == "lua_sandbox.executor_adapter.missing")
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        var actions = new List<LuaSandboxRepairAction>();

        foreach (var diagnostic in decision.Diagnostics.OrderBy(item => item.Code, StringComparer.Ordinal).ThenBy(item => item.Target, StringComparer.Ordinal))
        {
            actions.AddRange(ActionsFor(diagnostic));
        }

        var distinctActions = actions
            .GroupBy(item => item.ActionId, StringComparer.Ordinal)
            .Select(item => item.First())
            .OrderBy(item => item.ActionId, StringComparer.Ordinal)
            .ToList();

        var nonRepairable = blockingCodes.Any(code => !IsPotentiallyRepairable(code));
        return new LuaSandboxRepairPlan
        {
            RepairPlanId = $"lua-sandbox-repair-plan/{request.RequestId.Replace("lua-sandbox-request/", string.Empty, StringComparison.Ordinal).Replace("lua-sandbox-invalid/", "invalid-", StringComparison.Ordinal)}",
            RequestId = request.RequestId,
            DecisionStatus = decision.DecisionStatus,
            Status = distinctActions.Count == 0 ? "not_required" : nonRepairable ? "blocked" : "planned",
            BlockingDiagnosticCodes = blockingCodes,
            Actions = distinctActions,
            ImmutableAcceptedManifestIds = request.SelectedManifestIds.Order(StringComparer.Ordinal).ToList(),
            MutatesAcceptedManifests = distinctActions.Any(item => item.MutatesAcceptedManifest)
        };
    }

    public LuaSandboxRepairPlanMatrix BuildRepairPlanMatrix(
        IReadOnlyList<LuaSandboxExecutionRequest> requests,
        IReadOnlyList<LuaSandboxExecutionDecision> decisions)
    {
        var requestById = requests.ToDictionary(item => item.RequestId, StringComparer.Ordinal);
        var plans = new List<LuaSandboxRepairPlan>();
        foreach (var decision in decisions.OrderBy(item => item.RequestId, StringComparer.Ordinal))
        {
            if (!requestById.TryGetValue(decision.RequestId, out var request))
            {
                continue;
            }

            if (decision.DecisionStatus is "needs_repair" or "rejected" or "blocked_no_executor")
            {
                plans.Add(Plan(request, decision));
            }
        }

        return new LuaSandboxRepairPlanMatrix
        {
            RepairPlanCount = plans.Count,
            RepairActionCount = plans.SelectMany(item => item.Actions).Count(),
            MutatesAcceptedManifests = plans.Any(item => item.MutatesAcceptedManifests),
            RepairPlans = plans.OrderBy(item => item.RepairPlanId, StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<LuaSandboxRepairAction> ActionsFor(LuaSandboxDiagnostic diagnostic)
    {
        var target = diagnostic.Target;
        return diagnostic.Code switch
        {
            "lua_sandbox.host_api.denied" => [Action("remove-denied-host-api-group", target, diagnostic.Code)],
            "lua_sandbox.budget.missing" => [Action("add-missing-budget", target, diagnostic.Code)],
            "lua_sandbox.budget.over_limit" => [Action("reduce-budget", target, diagnostic.Code), Action("split-overlarge-request", target, diagnostic.Code)],
            "lua_sandbox.promotion_trace.missing" => [Action("add-goal034-promotion-trace", target, diagnostic.Code)],
            "lua_sandbox.manifest_id.fake" => [Action("replace-fake-manifest-id", target, diagnostic.Code)],
            "lua_sandbox.executor_adapter.missing" => [Action("mark-future-executor-adapter-required", target, diagnostic.Code)],
            "lua_sandbox.host_api.unknown" => [Action("remove-unknown-host-api-group", target, diagnostic.Code)],
            "lua_sandbox.dependency_order.unstable" or "lua_sandbox.manifest_order.nondeterministic" => [Action("restore-deterministic-ordering", target, diagnostic.Code)],
            _ => []
        };
    }

    private static LuaSandboxRepairAction Action(string kind, string target, string reasonCode) =>
        new()
        {
            ActionId = $"lua-sandbox-repair/{kind}/{NormalizeTarget(target)}",
            ActionKind = kind,
            Target = target,
            ReasonCode = reasonCode,
            MutatesAcceptedManifest = false
        };

    private static bool IsPotentiallyRepairable(string code) =>
        LuaSandboxExecutionGateValidator.IsRepairableDiagnostic(code)
        || code is "lua_sandbox.promotion_trace.missing"
            or "lua_sandbox.manifest_id.fake"
            or "lua_sandbox.executor_adapter.missing"
            or "lua_sandbox.host_api.unknown"
            or "lua_sandbox.dependency_order.unstable"
            or "lua_sandbox.manifest_order.nondeterministic";

    private static string NormalizeTarget(string target)
    {
        var chars = target
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-')
            .ToArray();
        return new string(chars).Trim('-');
    }
}
