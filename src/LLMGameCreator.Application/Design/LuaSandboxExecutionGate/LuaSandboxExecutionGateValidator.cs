using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

namespace LLMGameCreator.Application.Design.LuaSandboxExecutionGate;

public static partial class LuaSandboxExecutionGateValidator
{
    private static readonly IReadOnlySet<string> RepairableCodes = new HashSet<string>(
        [
            "lua_sandbox.budget.missing",
            "lua_sandbox.budget.over_limit",
            "lua_sandbox.host_api.denied"
        ],
        StringComparer.Ordinal);

    public static IReadOnlyList<LuaSandboxDiagnostic> ValidatePolicy(
        LuaSandboxExecutionPolicy policy,
        LuaSandboxHostBindingMatrix matrix)
    {
        var diagnostics = new List<LuaSandboxDiagnostic>();
        if (policy.RealLuaExecutionAllowed)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.policy.lua_execution.allowed", policy.PolicyId, "Goal 036 policy must not allow real Lua execution."));
        }

        if (policy.LuaParserAllowed)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.policy.lua_parser.allowed", policy.PolicyId, "Goal 036 policy must not allow Lua parsing."));
        }

        if (policy.LuaSourceGenerationAllowed)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.policy.lua_source_generation.allowed", policy.PolicyId, "Goal 036 policy must not allow Lua source generation."));
        }

        foreach (var groupId in policy.DeniedBoundaryGroups.Order(StringComparer.Ordinal))
        {
            if (!matrix.DeniedGroupIds.Contains(groupId, StringComparer.Ordinal)
                && !matrix.BoundaryBlockedGroupIds.Contains(groupId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.policy.denied_boundary.missing", groupId, "Required denied boundary group is missing from the host binding matrix."));
            }
        }

        foreach (var stepId in policy.RequiredProbeStepIds.Order(StringComparer.Ordinal))
        {
            if (!LuaSandboxExecutionGateVocabulary.ProbeStepIds.Contains(stepId))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.policy.probe_step.unknown", stepId, "Policy references an unknown probe step."));
            }
        }

        return SortDiagnostics(diagnostics);
    }

    public static LuaSandboxExecutionDecision Decide(
        LuaSandboxExecutionRequest request,
        IReadOnlyList<LuaModuleManifest> availableManifests,
        LuaSandboxExecutionPolicy? policy = null,
        LuaSandboxHostBindingMatrix? bindingMatrix = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(availableManifests);

        var sandboxPolicy = policy ?? LuaSandboxExecutionGateCatalog.BuildPolicy();
        var matrix = bindingMatrix ?? LuaSandboxExecutionGateCatalog.BuildHostBindingMatrix();
        var bindingsById = matrix.Bindings.ToDictionary(item => item.HostApiGroupId, StringComparer.Ordinal);
        var manifestsById = availableManifests
            .GroupBy(item => item.ModuleId, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
        var diagnostics = new List<LuaSandboxDiagnostic>();

        ValidateRequestShape(request, sandboxPolicy, matrix, bindingsById, manifestsById, diagnostics);

        var selectedManifests = request.SelectedManifestIds
            .Where(manifestsById.ContainsKey)
            .Select(item => manifestsById[item])
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();

        ValidateSelectedManifests(request, selectedManifests, matrix, bindingsById, diagnostics);
        ValidateDependencyOrder(request, selectedManifests, diagnostics);
        ValidateBudget(request, sandboxPolicy, diagnostics);

        var bindingUses = request.RequestedHostApiGroups
            .Where(bindingsById.ContainsKey)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(groupId =>
            {
                var binding = bindingsById[groupId];
                return new LuaSandboxBindingUse
                {
                    HostApiGroupId = groupId,
                    BindingDecision = binding.BindingDecision,
                    ReasonCode = binding.ReasonCode
                };
            })
            .ToList();

        var sortedDiagnostics = SortDiagnostics(diagnostics);
        var status = DetermineStatus(request, sortedDiagnostics);
        if (status == "blocked_no_executor")
        {
            sortedDiagnostics = SortDiagnostics(sortedDiagnostics.Concat(
            [
                Diagnostic(
                    "warning",
                    "lua_sandbox.executor_adapter.missing",
                    request.RequestId,
                    "The request is valid for policy review but remains blocked because no future executor adapter is available in Goal 036.")
            ]));
        }

        return new LuaSandboxExecutionDecision
        {
            RequestId = request.RequestId,
            ScenarioId = request.ScenarioId,
            DecisionStatus = status,
            SelectedManifestIds = selectedManifests.Select(item => item.ModuleId).ToList(),
            SelectedManifestCount = selectedManifests.Count,
            MetamoduleSpeciesArchetypeSlotManifestCount = selectedManifests.Count(item => item.ModuleId.StartsWith("lua-module/metamodule/species-archetype-slot/", StringComparison.Ordinal)),
            DependencyOrder = request.DependencyOrder,
            BindingDecisions = bindingUses,
            Diagnostics = sortedDiagnostics,
            LuaExecuted = false,
            StableSummary = $"{request.ScenarioId}|status={status}|selected={selectedManifests.Count}|bindings={bindingUses.Count}|diagnostics={sortedDiagnostics.Count}|luaExecuted=false"
        };
    }

    public static LuaSandboxInvalidMatrix BuildInvalidMatrix()
    {
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var matrix = LuaSandboxExecutionGateCatalog.BuildHostBindingMatrix();
        var policy = LuaSandboxExecutionGateCatalog.BuildPolicy();
        var cases = BuildInvalidRequestCases()
            .Select(item => Invalid(item.ScenarioId, item.MutatedEvidenceKind, item.ExpectedStatus, item.Request, manifests, policy, matrix))
            .ToList();

        return new LuaSandboxInvalidMatrix
        {
            ScenarioCount = cases.Count,
            MatchedExpectationCount = cases.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = cases.Count(item => item.ActualStatus == "rejected"),
            NeedsRepairCount = cases.Count(item => item.ActualStatus == "needs_repair"),
            Passed = cases.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            Scenarios = cases.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    public static IReadOnlyList<(string ScenarioId, string MutatedEvidenceKind, string ExpectedStatus, LuaSandboxExecutionRequest Request)> BuildInvalidRequestCases()
    {
        var policy = LuaSandboxExecutionGateCatalog.BuildPolicy();
        var valid = LuaSandboxExecutionGateCatalog.BuildDefaultRequests().First(item => item.ScenarioId == "frontier_survival");
        var overBudget = valid.Budget! with { InstructionLimit = policy.MaxInstructionLimit + 1 };
        var cases = new List<(string ScenarioId, string MutatedEvidenceKind, string ExpectedStatus, LuaSandboxExecutionRequest Request)>
        {
            ("fake_manifest_id", "fake manifest id", "rejected", valid with { SelectedManifestIds = valid.SelectedManifestIds.Take(2).Concat(["lua-module/fake/missing"]).ToList() }),
            ("unknown_host_api_group", "unknown host API group", "rejected", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["unknown.host"]).Order(StringComparer.Ordinal).ToList() }),
            ("denied_host_api_group", "denied host API group", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["arbitrary_code_generation"]).Order(StringComparer.Ordinal).ToList() }),
            ("missing_budget", "missing budget", "needs_repair", valid with { Budget = null }),
            ("over_budget", "over budget", "needs_repair", valid with { Budget = overBudget }),
            ("unstable_dependency_order", "unstable dependency order", "rejected", valid with { DependencyOrder = valid.DependencyOrder.Reverse().ToList() }),
            ("source_text_included", "source text included", "rejected", valid with { ContainsSourceText = true }),
            ("parser_claim_included", "parser claim included", "rejected", valid with { ClaimsParserUsed = true }),
            ("lua_execution_claim_included", "lua execution claim included", "rejected", valid with { ClaimsLuaExecution = true, LuaExecuted = true }),
            ("final_prose_included", "final prose included", "rejected", valid with { ContainsFinalProse = true }),
            ("self_promotion", "self promotion", "rejected", valid with { SelfPromoted = true }),
            ("missing_goal034_promotion_trace", "missing Goal 034 promotion trace", "rejected", valid with { ProvenanceKind = "promoted_from_goal034", PromotionTraceId = string.Empty }),
            ("provider_llm_rag_leak", "provider/LLM/RAG leak", "rejected", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["provider_llm", "rag"]).Order(StringComparer.Ordinal).ToList() }),
            ("runtime_ui_unity_gamepackage_schema_mutation_leak", "Runtime/UI/Unity/GamePackage schema mutation leak", "rejected", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["runtime_mutation", "ui", "unity", "gamepackage_schema_mutation"]).Order(StringComparer.Ordinal).ToList() }),
            ("filesystem_leak", "filesystem leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["file_system"]).Order(StringComparer.Ordinal).ToList() }),
            ("network_leak", "network leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["network"]).Order(StringComparer.Ordinal).ToList() }),
            ("process_leak", "process leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["process"]).Order(StringComparer.Ordinal).ToList() }),
            ("reflection_leak", "reflection leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["reflection"]).Order(StringComparer.Ordinal).ToList() }),
            ("threading_leak", "threading leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["threading"]).Order(StringComparer.Ordinal).ToList() }),
            ("time_leak", "time leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["time"]).Order(StringComparer.Ordinal).ToList() }),
            ("random_leak", "random leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["random"]).Order(StringComparer.Ordinal).ToList() }),
            ("native_interop_leak", "native interop leak", "needs_repair", valid with { RequestedHostApiGroups = valid.RequestedHostApiGroups.Concat(["native_interop"]).Order(StringComparer.Ordinal).ToList() }),
            ("immutable_repair_mutation", "immutable repair mutation", "rejected", valid with { MutatesAcceptedManifests = true }),
            ("nondeterministic_ordering", "nondeterministic ordering", "rejected", valid with { SelectedManifestIds = valid.SelectedManifestIds.Reverse().ToList() })
        };

        return cases
            .Select(item => (item.ScenarioId, item.MutatedEvidenceKind, item.ExpectedStatus, Request: item.Request with { RequestId = $"lua-sandbox-invalid/{item.ScenarioId}" }))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();
    }

    public static bool IsRepairableDiagnostic(string code) => RepairableCodes.Contains(code);

    public static LuaSandboxDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    public static IReadOnlyList<LuaSandboxDiagnostic> SortDiagnostics(IEnumerable<LuaSandboxDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static void ValidateRequestShape(
        LuaSandboxExecutionRequest request,
        LuaSandboxExecutionPolicy policy,
        LuaSandboxHostBindingMatrix matrix,
        IReadOnlyDictionary<string, LuaSandboxHostBinding> bindingsById,
        IReadOnlyDictionary<string, LuaModuleManifest> manifestsById,
        ICollection<LuaSandboxDiagnostic> diagnostics)
    {
        if (!StableIdPattern().IsMatch(request.RequestId))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.request_id.invalid", request.RequestId, "Execution request id must be stable."));
        }

        if (!LuaSandboxExecutionGateVocabulary.Scenarios.Contains(request.ScenarioId))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.scenario.unknown", request.ScenarioId, "Execution request references an unknown scenario."));
        }

        if (!LuaSandboxExecutionGateVocabulary.ProvenanceKinds.Contains(request.ProvenanceKind))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.provenance.unknown", request.RequestId, "Execution request provenance is unknown."));
        }

        if (request.ProvenanceKind == "promoted_from_goal034" && string.IsNullOrWhiteSpace(request.PromotionTraceId))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.promotion_trace.missing", request.RequestId, "Goal 034 promotion trace is required before a promoted request can be accepted."));
        }

        if (request.SelfPromoted)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.promotion.self_forbidden", request.RequestId, "Execution request cannot promote itself."));
        }

        if (request.SelectedManifestIds.Count == 0)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_selection.empty", request.RequestId, "Execution request must select at least one Goal 035 manifest."));
        }

        foreach (var duplicate in request.SelectedManifestIds.GroupBy(item => item, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_id.duplicate", duplicate.Key, "Selected manifest ids must be unique."));
        }

        foreach (var manifestId in request.SelectedManifestIds.Order(StringComparer.Ordinal))
        {
            if (!manifestsById.ContainsKey(manifestId))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_id.fake", manifestId, "Selected manifest id does not exist in Goal 035 registry."));
            }
        }

        var knownSelected = request.SelectedManifestIds.Where(manifestsById.ContainsKey).Select(item => manifestsById[item]).OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).Select(item => item.ModuleId).ToList();
        if (knownSelected.Count == request.SelectedManifestIds.Count
            && !request.SelectedManifestIds.SequenceEqual(knownSelected, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_order.nondeterministic", request.RequestId, "Selected manifest ids must follow deterministic Goal 035 ordering."));
        }

        foreach (var duplicate in request.RequestedHostApiGroups.GroupBy(item => item, StringComparer.Ordinal).Where(item => item.Count() > 1).OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.host_api.duplicate", duplicate.Key, "Requested host API groups must be unique."));
        }

        if (!request.RequestedHostApiGroups.SequenceEqual(request.RequestedHostApiGroups.Order(StringComparer.Ordinal), StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.host_api.order.nondeterministic", request.RequestId, "Requested host API groups must use stable ordering."));
        }

        foreach (var groupId in request.RequestedHostApiGroups.Order(StringComparer.Ordinal))
        {
            if (!bindingsById.TryGetValue(groupId, out var binding))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.host_api.unknown", groupId, "Requested host API group is not known to the sandbox binding matrix."));
                continue;
            }

            if (binding.BindingDecision == "denied")
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.host_api.denied", groupId, "Requested host API group is denied by sandbox policy."));
            }

            if (binding.BindingDecision == "blocked_by_boundary")
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.host_api.boundary_blocked", groupId, "Requested host API group crosses a forbidden Runtime/UI/Unity/GamePackage/provider boundary."));
            }
        }

        foreach (var stepId in request.DryRunProbeStepIds.Order(StringComparer.Ordinal))
        {
            if (!policy.RequiredProbeStepIds.Contains(stepId, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.probe_step.unknown", stepId, "Dry-run probe step is not allowed by policy."));
            }
        }

        foreach (var family in request.ExpectedTraceEventFamilies.Order(StringComparer.Ordinal))
        {
            if (!LuaSandboxExecutionGateVocabulary.ExpectedTraceEventFamilies.Contains(family))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.trace_event_family.unknown", family, "Expected trace event family is unknown."));
            }
        }

        if (request.ContainsSourceText)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.source_text.forbidden", request.RequestId, "Execution requests must not include Lua source text."));
        }

        if (request.ClaimsParserUsed)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.parser_claim.forbidden", request.RequestId, "Goal 036 must not claim Lua parsing happened."));
        }

        if (request.ClaimsLuaExecution || request.LuaExecuted)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.lua_execution_claim.forbidden", request.RequestId, "Goal 036 must not claim Lua execution happened."));
        }

        if (request.ContainsFinalProse)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.final_prose.forbidden", request.RequestId, "Execution requests must not include final prose."));
        }

        if (request.MutatesAcceptedManifests)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.repair.immutable_manifest_mutation", request.RequestId, "Repair must not mutate accepted Goal 035 manifests."));
        }

        if (!request.Determinism.NoTime || !request.Determinism.NoRandom || !request.Determinism.NoNetwork || !request.Determinism.NoFilesystem || !request.Determinism.NoReflection || !request.Determinism.NoThreads)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.determinism.disabled", request.RequestId, "Sandbox determinism flags must deny time, random, network, filesystem, reflection and threads."));
        }

        foreach (var deniedGroup in request.DeniedHostApiGroups.Order(StringComparer.Ordinal))
        {
            if (!matrix.DeniedGroupIds.Contains(deniedGroup, StringComparer.Ordinal)
                && !matrix.BoundaryBlockedGroupIds.Contains(deniedGroup, StringComparer.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.denied_host_api.unknown", deniedGroup, "Denied host API declaration is not in the sandbox denied/boundary matrix."));
            }
        }
    }

    private static void ValidateSelectedManifests(
        LuaSandboxExecutionRequest request,
        IReadOnlyList<LuaModuleManifest> selectedManifests,
        LuaSandboxHostBindingMatrix matrix,
        IReadOnlyDictionary<string, LuaSandboxHostBinding> bindingsById,
        ICollection<LuaSandboxDiagnostic> diagnostics)
    {
        foreach (var manifest in selectedManifests.OrderBy(item => item.ModuleId, StringComparer.Ordinal))
        {
            if (manifest.ClaimsLuaExecution || manifest.DeclaresLuaSource)
            {
                diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest.lua_source_or_execution", manifest.ModuleId, "Selected Goal 035 manifest claims Lua source or execution."));
            }

            foreach (var groupId in manifest.AllowedHostApiGroups.Order(StringComparer.Ordinal))
            {
                if (!bindingsById.TryGetValue(groupId, out var binding))
                {
                    diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_host_api.unknown", $"{manifest.ModuleId}:{groupId}", "Selected manifest references an unknown host API group."));
                    continue;
                }

                if (binding.BindingDecision == "denied")
                {
                    diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_host_api.denied", $"{manifest.ModuleId}:{groupId}", "Selected manifest requires a denied host API group."));
                }

                if (binding.BindingDecision == "blocked_by_boundary")
                {
                    diagnostics.Add(Diagnostic("error", "lua_sandbox.manifest_host_api.boundary_blocked", $"{manifest.ModuleId}:{groupId}", "Selected manifest crosses a forbidden boundary."));
                }
            }

            foreach (var deniedGroup in manifest.DeniedHostApiGroups.Order(StringComparer.Ordinal))
            {
                if (request.RequestedHostApiGroups.Contains(deniedGroup, StringComparer.Ordinal)
                    && matrix.DeniedGroupIds.Contains(deniedGroup, StringComparer.Ordinal))
                {
                    diagnostics.Add(Diagnostic("error", "lua_sandbox.host_api.denied", $"{manifest.ModuleId}:{deniedGroup}", "Request asks for a group explicitly denied by the selected manifest."));
                }
            }
        }
    }

    private static void ValidateDependencyOrder(
        LuaSandboxExecutionRequest request,
        IReadOnlyList<LuaModuleManifest> selectedManifests,
        ICollection<LuaSandboxDiagnostic> diagnostics)
    {
        if (selectedManifests.Count == 0)
        {
            return;
        }

        var expectedOrder = BuildDependencyOrder(selectedManifests);
        if (!request.DependencyOrder.SequenceEqual(expectedOrder, StringComparer.Ordinal))
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.dependency_order.unstable", request.RequestId, "Dependency order must match deterministic Goal 035 dependency order."));
        }
    }

    private static void ValidateBudget(
        LuaSandboxExecutionRequest request,
        LuaSandboxExecutionPolicy policy,
        ICollection<LuaSandboxDiagnostic> diagnostics)
    {
        if (request.Budget == null)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.budget.missing", request.RequestId, "Sandbox request must include instruction, memory, output/event and deterministic step budgets."));
            return;
        }

        if (request.Budget.InstructionLimit <= 0
            || request.Budget.MemoryLimitKb <= 0
            || request.Budget.OutputEventLimit <= 0
            || request.Budget.DeterministicStepLimit <= 0)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.budget.missing", request.RequestId, "Sandbox request budget values must be positive."));
        }

        if (request.Budget.InstructionLimit > policy.MaxInstructionLimit
            || request.Budget.MemoryLimitKb > policy.MaxMemoryLimitKb
            || request.Budget.OutputEventLimit > policy.MaxOutputEventLimit
            || request.Budget.DeterministicStepLimit > policy.MaxDeterministicStepLimit)
        {
            diagnostics.Add(Diagnostic("error", "lua_sandbox.budget.over_limit", request.RequestId, "Sandbox request budget exceeds Goal 036 policy limits."));
        }
    }

    private static string DetermineStatus(LuaSandboxExecutionRequest request, IReadOnlyList<LuaSandboxDiagnostic> diagnostics)
    {
        var errorCodes = diagnostics.Where(item => item.Severity == "error").Select(item => item.Code).Distinct(StringComparer.Ordinal).ToList();
        if (errorCodes.Count > 0 && errorCodes.All(RepairableCodes.Contains))
        {
            return "needs_repair";
        }

        if (errorCodes.Count > 0)
        {
            return "rejected";
        }

        if (request.RequiresFutureExecutorAdapter && !request.FutureExecutorAdapterAvailable)
        {
            return "blocked_no_executor";
        }

        return request.AllowFutureExecutorReadiness
            ? "ready_for_future_executor"
            : "dry_run_only";
    }

    private static LuaSandboxInvalidScenario Invalid(
        string scenarioId,
        string kind,
        string expectedStatus,
        LuaSandboxExecutionRequest request,
        IReadOnlyList<LuaModuleManifest> manifests,
        LuaSandboxExecutionPolicy policy,
        LuaSandboxHostBindingMatrix matrix)
    {
        var decision = Decide(request with { RequestId = $"lua-sandbox-invalid/{scenarioId}" }, manifests, policy, matrix);
        return new LuaSandboxInvalidScenario
        {
            ScenarioId = scenarioId,
            MutatedEvidenceKind = kind,
            ExpectedStatus = expectedStatus,
            ActualStatus = decision.DecisionStatus,
            ExpectedValid = false,
            ActualValid = decision.DecisionStatus is "ready_for_future_executor" or "dry_run_only" or "blocked_no_executor",
            Diagnostics = decision.Diagnostics
        };
    }

    private static IReadOnlyList<string> BuildDependencyOrder(IReadOnlyList<LuaModuleManifest> selected)
    {
        var byId = selected.ToDictionary(item => item.ModuleId, StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var order = new List<string>();

        foreach (var manifest in selected.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal))
        {
            Visit(manifest.ModuleId);
        }

        return order;

        void Visit(string moduleId)
        {
            if (!visited.Add(moduleId) || !byId.TryGetValue(moduleId, out var manifest))
            {
                return;
            }

            foreach (var dependency in manifest.Dependencies.Order(StringComparer.Ordinal))
            {
                Visit(dependency);
            }

            order.Add(moduleId);
        }
    }

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    [GeneratedRegex("^[a-z0-9][a-z0-9_./:-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();
}
