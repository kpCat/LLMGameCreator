using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

namespace LLMGameCreator.Application.Design.LuaSandboxExecutionGate;

public static class LuaSandboxExecutionGateCatalog
{
    private static readonly IReadOnlyList<string> RequiredDeniedBoundaryGroups =
    [
        "file_system",
        "network",
        "process",
        "reflection",
        "threading",
        "time",
        "random",
        "ui",
        "unity",
        "runtime_mutation",
        "gamepackage_schema_mutation",
        "provider_llm",
        "rag",
        "media_generation",
        "native_interop"
    ];

    public static LuaSandboxExecutionPolicy BuildPolicy() =>
        new()
        {
            MaxInstructionLimit = 25_000,
            MaxMemoryLimitKb = 4096,
            MaxOutputEventLimit = 256,
            MaxDeterministicStepLimit = 512,
            RequiredProbeStepIds = LuaSandboxExecutionGateVocabulary.ProbeStepIds.Order(StringComparer.Ordinal).ToList(),
            DeniedBoundaryGroups = RequiredDeniedBoundaryGroups.Order(StringComparer.Ordinal).ToList(),
            RealLuaExecutionAllowed = false,
            LuaParserAllowed = false,
            LuaSourceGenerationAllowed = false
        };

    public static LuaSandboxPolicySummary BuildPolicySummary()
    {
        var goal035Policy = LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy();
        var sandboxPolicy = BuildPolicy();
        var diagnostics = LuaSandboxExecutionGateValidator.ValidatePolicy(sandboxPolicy, BuildHostBindingMatrix(goal035Policy.Groups));
        return new LuaSandboxPolicySummary
        {
            Policy = sandboxPolicy,
            Goal035HostApiGroupCount = goal035Policy.GroupCount,
            DeniedBoundaryGroupCount = sandboxPolicy.DeniedBoundaryGroups.Count,
            Diagnostics = diagnostics
        };
    }

    public static LuaSandboxHostBindingMatrix BuildHostBindingMatrix(IReadOnlyList<LuaHostApiGroup>? goal035HostApiGroups = null)
    {
        var hostApiGroups = (goal035HostApiGroups ?? LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy().Groups)
            .OrderBy(item => item.GroupId, StringComparer.Ordinal)
            .ToList();
        var bindings = new Dictionary<string, LuaSandboxHostBinding>(StringComparer.Ordinal);

        foreach (var group in hostApiGroups)
        {
            var decision = BindingDecisionForGoal035Group(group.GroupId);
            AddBinding(bindings, group.GroupId, group.GroupId, group.DisplayName, decision, ReasonCodeFor(group.GroupId, decision));
        }

        AddBoundaryAlias(bindings, "file_system", "filesystem", "Filesystem boundary");
        AddBoundaryAlias(bindings, "process", "os_process", "Process boundary");
        AddBoundaryAlias(bindings, "ui", "ui_winforms", "UI boundary");
        AddBoundaryAlias(bindings, "unity", "unity_direct_call", "Unity boundary");
        AddBoundaryAlias(bindings, "runtime_mutation", "runtime_direct_mutation", "Runtime mutation boundary");
        AddBoundaryAlias(bindings, "provider_llm", "provider_llm_rag", "Provider/LLM boundary");
        AddBoundaryAlias(bindings, "rag", "provider_llm_rag", "RAG boundary");

        AddBinding(bindings, "threading", string.Empty, "Threading", "denied", "lua_sandbox.host_api.threading.denied");
        AddBinding(bindings, "time", string.Empty, "Time", "denied", "lua_sandbox.host_api.time.denied");
        AddBinding(bindings, "random", string.Empty, "Random", "denied", "lua_sandbox.host_api.random.denied");
        AddBinding(bindings, "media_generation", string.Empty, "Media generation", "blocked_by_boundary", "lua_sandbox.host_api.media_generation.boundary");
        AddBinding(bindings, "native_interop", string.Empty, "Native interop", "denied", "lua_sandbox.host_api.native_interop.denied");

        var orderedBindings = bindings.Values
            .OrderBy(item => item.HostApiGroupId, StringComparer.Ordinal)
            .ToList();

        return new LuaSandboxHostBindingMatrix
        {
            BindingCount = orderedBindings.Count,
            DryRunAllowedGroupIds = GroupIdsByDecision(orderedBindings, "allowed_in_dry_run"),
            FutureExecutorOnlyGroupIds = GroupIdsByDecision(orderedBindings, "allowed_only_for_future_executor"),
            DeniedGroupIds = orderedBindings
                .Where(item => item.BindingDecision is "denied" or "blocked_by_boundary")
                .Select(item => item.HostApiGroupId)
                .Order(StringComparer.Ordinal)
                .ToList(),
            ExplicitAdapterRequiredGroupIds = GroupIdsByDecision(orderedBindings, "needs_explicit_adapter"),
            BoundaryBlockedGroupIds = GroupIdsByDecision(orderedBindings, "blocked_by_boundary"),
            Bindings = orderedBindings,
            LuaExecutable = false
        };
    }

    public static IReadOnlyList<LuaSandboxExecutionRequest> BuildDefaultRequests()
    {
        var plans = new LuaModuleManifestPlanner().PlanDefaultScenarios()
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var matrix = BuildHostBindingMatrix();
        var deniedGroups = matrix.DeniedGroupIds.Order(StringComparer.Ordinal).ToList();
        return
        [
            Request(
                plans["frontier_survival"],
                "lua-sandbox-request/frontier-survival",
                "manual",
                string.Empty,
                new LuaSandboxBudget { InstructionLimit = 12_000, MemoryLimitKb = 1024, OutputEventLimit = 48, DeterministicStepLimit = 128 },
                deniedGroups,
                allowFutureExecutorReadiness: false,
                requiresFutureExecutorAdapter: false,
                futureExecutorAdapterAvailable: false),
            Request(
                plans["gothic_intrigue"],
                "lua-sandbox-request/gothic-intrigue",
                "promoted_from_goal034",
                "goal034-promotion/strict_llm_draft_artifact_loop/promoted/gothic-intrigue",
                new LuaSandboxBudget { InstructionLimit = 14_000, MemoryLimitKb = 1536, OutputEventLimit = 64, DeterministicStepLimit = 160 },
                deniedGroups,
                allowFutureExecutorReadiness: true,
                requiresFutureExecutorAdapter: false,
                futureExecutorAdapterAvailable: false),
            Request(
                plans["caravan_trade"],
                "lua-sandbox-request/caravan-trade",
                "import",
                string.Empty,
                new LuaSandboxBudget { InstructionLimit = 13_000, MemoryLimitKb = 1280, OutputEventLimit = 56, DeterministicStepLimit = 144 },
                deniedGroups,
                allowFutureExecutorReadiness: false,
                requiresFutureExecutorAdapter: false,
                futureExecutorAdapterAvailable: false),
            Request(
                plans["metamodule_kingdoms"],
                "lua-sandbox-request/metamodule-kingdoms",
                "llm_draft",
                string.Empty,
                new LuaSandboxBudget { InstructionLimit = 24_000, MemoryLimitKb = 4096, OutputEventLimit = 224, DeterministicStepLimit = 480 },
                deniedGroups,
                allowFutureExecutorReadiness: true,
                requiresFutureExecutorAdapter: true,
                futureExecutorAdapterAvailable: false)
        ];
    }

    private static LuaSandboxExecutionRequest Request(
        LuaModuleSelectionPlan plan,
        string requestId,
        string provenanceKind,
        string promotionTraceId,
        LuaSandboxBudget budget,
        IReadOnlyList<string> deniedGroups,
        bool allowFutureExecutorReadiness,
        bool requiresFutureExecutorAdapter,
        bool futureExecutorAdapterAvailable)
    {
        var selectedManifests = plan.SelectedManifests
            .OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal)
            .ToList();
        var requestedHostApiGroups = selectedManifests
            .SelectMany(item => item.AllowedHostApiGroups)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new LuaSandboxExecutionRequest
        {
            RequestId = requestId,
            ScenarioId = plan.ScenarioId,
            SelectedManifestIds = selectedManifests.Select(item => item.ModuleId).ToList(),
            RequestedHostApiGroups = requestedHostApiGroups,
            DeniedHostApiGroups = deniedGroups,
            Budget = budget,
            Determinism = new LuaSandboxDeterminismFlags(),
            ProvenanceKind = provenanceKind,
            PromotionTraceId = promotionTraceId,
            DryRunProbeStepIds = LuaSandboxExecutionGateVocabulary.ProbeStepIds.Order(StringComparer.Ordinal).ToList(),
            ExpectedTraceEventFamilies =
            [
                "manifest_selection",
                "host_binding",
                "budget_validation",
                "dependency_order",
                "expected_output"
            ],
            DependencyOrder = plan.DependencyOrder,
            AllowFutureExecutorReadiness = allowFutureExecutorReadiness,
            RequiresFutureExecutorAdapter = requiresFutureExecutorAdapter,
            FutureExecutorAdapterAvailable = futureExecutorAdapterAvailable,
            DeterministicOrderingKey = $"{plan.ScenarioId}|{requestId}"
        };
    }

    private static string BindingDecisionForGoal035Group(string groupId) =>
        groupId switch
        {
            "semantic.read" or "feature.read" or "intent.read" => "allowed_in_dry_run",
            "quest.plan" or "dialogue.intent" or "economy.plan" or "combat.plan" or "world.plan" or "event.plan" => "allowed_only_for_future_executor",
            "metamodule.expand" => "needs_explicit_adapter",
            "provider_llm_rag" or "ui_winforms" or "runtime_direct_mutation" or "unity_direct_call" or "gamepackage_schema_mutation" => "blocked_by_boundary",
            "filesystem" or "network" or "os_process" or "reflection" or "arbitrary_code_generation" or "implicit_lua_execution" => "denied",
            _ => "denied"
        };

    private static string ReasonCodeFor(string groupId, string decision) =>
        decision switch
        {
            "allowed_in_dry_run" => $"lua_sandbox.host_api.{groupId}.dry_run_allowed",
            "allowed_only_for_future_executor" => $"lua_sandbox.host_api.{groupId}.future_executor_only",
            "needs_explicit_adapter" => $"lua_sandbox.host_api.{groupId}.adapter_required",
            "blocked_by_boundary" => $"lua_sandbox.host_api.{groupId}.boundary",
            _ => $"lua_sandbox.host_api.{groupId}.denied"
        };

    private static void AddBoundaryAlias(
        IDictionary<string, LuaSandboxHostBinding> bindings,
        string alias,
        string goal035GroupId,
        string displayName)
    {
        var decision = alias is "ui" or "unity" or "runtime_mutation" or "provider_llm" or "rag"
            ? "blocked_by_boundary"
            : "denied";
        AddBinding(bindings, alias, goal035GroupId, displayName, decision, ReasonCodeFor(alias, decision));
    }

    private static void AddBinding(
        IDictionary<string, LuaSandboxHostBinding> bindings,
        string groupId,
        string goal035GroupId,
        string displayName,
        string decision,
        string reasonCode)
    {
        bindings[groupId] = new LuaSandboxHostBinding
        {
            HostApiGroupId = groupId,
            Goal035HostApiGroupId = goal035GroupId,
            DisplayName = displayName,
            BindingDecision = decision,
            ReasonCode = reasonCode,
            LuaExecutable = false
        };
    }

    private static IReadOnlyList<string> GroupIdsByDecision(IReadOnlyList<LuaSandboxHostBinding> bindings, string decision) =>
        bindings
            .Where(item => item.BindingDecision == decision)
            .Select(item => item.HostApiGroupId)
            .Order(StringComparer.Ordinal)
            .ToList();
}
