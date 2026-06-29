namespace LLMGameCreator.Application.Design.LuaSandboxExecutionGate;

public sealed class LuaSandboxDryRunTraceBuilder
{
    public LuaSandboxTrace BuildTrace(LuaSandboxExecutionRequest request, LuaSandboxExecutionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        var steps = request.DryRunProbeStepIds
            .Order(StringComparer.Ordinal)
            .Select(stepId => BuildStep(stepId, request, decision))
            .ToList();

        return new LuaSandboxTrace
        {
            TraceId = $"lua-sandbox-dry-run-trace/{request.ScenarioId}",
            RequestId = request.RequestId,
            ScenarioId = request.ScenarioId,
            DecisionStatus = decision.DecisionStatus,
            LuaExecuted = false,
            ProbeSteps = steps
        };
    }

    public LuaSandboxDryRunTraceMatrix BuildMatrix(
        IReadOnlyList<LuaSandboxExecutionRequest> requests,
        IReadOnlyList<LuaSandboxExecutionDecision> decisions)
    {
        var requestById = requests.ToDictionary(item => item.RequestId, StringComparer.Ordinal);
        var traces = decisions
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Where(item => requestById.ContainsKey(item.RequestId))
            .Select(item => BuildTrace(requestById[item.RequestId], item))
            .ToList();

        return new LuaSandboxDryRunTraceMatrix
        {
            TraceCount = traces.Count,
            LuaExecuted = traces.Any(item => item.LuaExecuted || item.ProbeSteps.Any(step => step.LuaExecuted)),
            Traces = traces
        };
    }

    private static LuaSandboxProbeStep BuildStep(
        string stepId,
        LuaSandboxExecutionRequest request,
        LuaSandboxExecutionDecision decision)
    {
        var diagnostics = DiagnosticsForStep(stepId, decision.Diagnostics);
        return new LuaSandboxProbeStep
        {
            StepId = stepId,
            Status = diagnostics.Any(item => item.Severity == "error") ? "failed" : "passed",
            LuaExecuted = false,
            TraceEventFamilies = TraceFamiliesForStep(stepId, request.ExpectedTraceEventFamilies),
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<LuaSandboxDiagnostic> DiagnosticsForStep(
        string stepId,
        IReadOnlyList<LuaSandboxDiagnostic> diagnostics) =>
        stepId switch
        {
            "validate_manifest_selection" => diagnostics.Where(item => item.Code.Contains("manifest", StringComparison.Ordinal)).ToList(),
            "validate_host_bindings" => diagnostics.Where(item => item.Code.Contains("host_api", StringComparison.Ordinal)).ToList(),
            "validate_budget" => diagnostics.Where(item => item.Code.Contains("budget", StringComparison.Ordinal)).ToList(),
            "validate_dependency_order" => diagnostics.Where(item => item.Code.Contains("dependency_order", StringComparison.Ordinal)).ToList(),
            "validate_expected_outputs" => diagnostics.Where(item => item.Code.Contains("source_text", StringComparison.Ordinal)
                || item.Code.Contains("parser_claim", StringComparison.Ordinal)
                || item.Code.Contains("lua_execution", StringComparison.Ordinal)
                || item.Code.Contains("final_prose", StringComparison.Ordinal)
                || item.Code.Contains("trace_event", StringComparison.Ordinal)).ToList(),
            _ => diagnostics
        };

    private static IReadOnlyList<string> TraceFamiliesForStep(string stepId, IReadOnlyList<string> requested) =>
        stepId switch
        {
            "validate_manifest_selection" => Include(requested, "manifest_selection"),
            "validate_host_bindings" => Include(requested, "host_binding", "executor_boundary"),
            "validate_budget" => Include(requested, "budget_validation"),
            "validate_dependency_order" => Include(requested, "dependency_order"),
            "validate_expected_outputs" => Include(requested, "expected_output"),
            _ => requested.Order(StringComparer.Ordinal).ToList()
        };

    private static IReadOnlyList<string> Include(IReadOnlyList<string> requested, params string[] preferred)
    {
        var values = requested
            .Where(item => preferred.Contains(item, StringComparer.Ordinal))
            .Concat(preferred.Where(item => LuaSandboxExecutionGateVocabulary.ExpectedTraceEventFamilies.Contains(item)))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        return values;
    }
}
