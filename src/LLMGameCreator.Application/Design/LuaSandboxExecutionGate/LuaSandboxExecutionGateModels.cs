using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

namespace LLMGameCreator.Application.Design.LuaSandboxExecutionGate;

public static class LuaSandboxExecutionGateVocabulary
{
    public const string SchemaVersion = "lua_sandbox_execution_gate_v1";

    public static readonly IReadOnlySet<string> DecisionStatuses = new HashSet<string>(
        ["ready_for_future_executor", "dry_run_only", "needs_repair", "blocked_no_executor", "rejected"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> BindingDecisions = new HashSet<string>(
        ["allowed_in_dry_run", "allowed_only_for_future_executor", "denied", "needs_explicit_adapter", "blocked_by_boundary"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> ProvenanceKinds = new HashSet<string>(
        ["manual", "import", "llm_draft", "promoted_from_goal034"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> ProbeStepIds = new HashSet<string>(
        ["validate_manifest_selection", "validate_host_bindings", "validate_budget", "validate_dependency_order", "validate_expected_outputs"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> ExpectedTraceEventFamilies = new HashSet<string>(
        ["manifest_selection", "host_binding", "budget_validation", "dependency_order", "expected_output", "repair_plan", "executor_boundary"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> Scenarios = new HashSet<string>(
        ["frontier_survival", "gothic_intrigue", "caravan_trade", "metamodule_kingdoms"],
        StringComparer.Ordinal);
}

public sealed record LuaSandboxBudget
{
    public int InstructionLimit { get; init; }
    public int MemoryLimitKb { get; init; }
    public int OutputEventLimit { get; init; }
    public int DeterministicStepLimit { get; init; }
}

public sealed record LuaSandboxDeterminismFlags
{
    public bool NoTime { get; init; } = true;
    public bool NoRandom { get; init; } = true;
    public bool NoNetwork { get; init; } = true;
    public bool NoFilesystem { get; init; } = true;
    public bool NoReflection { get; init; } = true;
    public bool NoThreads { get; init; } = true;
}

public sealed record LuaSandboxExecutionPolicy
{
    public string PolicyId { get; init; } = "lua_sandbox_execution_policy_v1";
    public int MaxInstructionLimit { get; init; }
    public int MaxMemoryLimitKb { get; init; }
    public int MaxOutputEventLimit { get; init; }
    public int MaxDeterministicStepLimit { get; init; }
    public LuaSandboxDeterminismFlags RequiredDeterminism { get; init; } = new();
    public IReadOnlyList<string> RequiredProbeStepIds { get; init; } = [];
    public IReadOnlyList<string> DeniedBoundaryGroups { get; init; } = [];
    public bool RealLuaExecutionAllowed { get; init; }
    public bool LuaParserAllowed { get; init; }
    public bool LuaSourceGenerationAllowed { get; init; }
}

public sealed record LuaSandboxHostBinding
{
    public string HostApiGroupId { get; init; } = string.Empty;
    public string Goal035HostApiGroupId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string BindingDecision { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public bool LuaExecutable { get; init; }
}

public sealed record LuaSandboxHostBindingMatrix
{
    public string SchemaVersion { get; init; } = "lua_sandbox_host_binding_matrix_v1";
    public int BindingCount { get; init; }
    public IReadOnlyList<string> DryRunAllowedGroupIds { get; init; } = [];
    public IReadOnlyList<string> FutureExecutorOnlyGroupIds { get; init; } = [];
    public IReadOnlyList<string> DeniedGroupIds { get; init; } = [];
    public IReadOnlyList<string> ExplicitAdapterRequiredGroupIds { get; init; } = [];
    public IReadOnlyList<string> BoundaryBlockedGroupIds { get; init; } = [];
    public IReadOnlyList<LuaSandboxHostBinding> Bindings { get; init; } = [];
    public bool LuaExecutable { get; init; }
}

public sealed record LuaSandboxPolicySummary
{
    public string SchemaVersion { get; init; } = LuaSandboxExecutionGateVocabulary.SchemaVersion;
    public LuaSandboxExecutionPolicy Policy { get; init; } = new();
    public int Goal035HostApiGroupCount { get; init; }
    public int DeniedBoundaryGroupCount { get; init; }
    public bool NoLuaExecution { get; init; } = true;
    public bool NoLuaParser { get; init; } = true;
    public bool NoLuaSourceGeneration { get; init; } = true;
    public IReadOnlyList<LuaSandboxDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaSandboxExecutionRequest
{
    public string RequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedManifestIds { get; init; } = [];
    public IReadOnlyList<string> RequestedHostApiGroups { get; init; } = [];
    public IReadOnlyList<string> DeniedHostApiGroups { get; init; } = [];
    public LuaSandboxBudget? Budget { get; init; }
    public LuaSandboxDeterminismFlags Determinism { get; init; } = new();
    public string ProvenanceKind { get; init; } = "manual";
    public string PromotionTraceId { get; init; } = string.Empty;
    public IReadOnlyList<string> DryRunProbeStepIds { get; init; } = [];
    public IReadOnlyList<string> ExpectedTraceEventFamilies { get; init; } = [];
    public IReadOnlyList<string> DependencyOrder { get; init; } = [];
    public bool AllowFutureExecutorReadiness { get; init; }
    public bool RequiresFutureExecutorAdapter { get; init; }
    public bool FutureExecutorAdapterAvailable { get; init; }
    public bool ContainsSourceText { get; init; }
    public bool ClaimsParserUsed { get; init; }
    public bool ClaimsLuaExecution { get; init; }
    public bool ContainsFinalProse { get; init; }
    public bool SelfPromoted { get; init; }
    public bool MutatesAcceptedManifests { get; init; }
    public bool LuaExecuted { get; init; }
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record LuaSandboxExecutionRequestMatrix
{
    public string SchemaVersion { get; init; } = "lua_sandbox_execution_request_matrix_v1";
    public int RequestCount { get; init; }
    public int SelectedManifestCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotManifestCount { get; init; }
    public IReadOnlyList<LuaSandboxExecutionRequest> Requests { get; init; } = [];
}

public sealed record LuaSandboxBindingUse
{
    public string HostApiGroupId { get; init; } = string.Empty;
    public string BindingDecision { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
}

public sealed record LuaSandboxExecutionDecision
{
    public string SchemaVersion { get; init; } = "lua_sandbox_execution_decision_v1";
    public string RequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string DecisionStatus { get; init; } = "rejected";
    public IReadOnlyList<string> SelectedManifestIds { get; init; } = [];
    public int SelectedManifestCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotManifestCount { get; init; }
    public IReadOnlyList<string> DependencyOrder { get; init; } = [];
    public IReadOnlyList<LuaSandboxBindingUse> BindingDecisions { get; init; } = [];
    public IReadOnlyList<LuaSandboxDiagnostic> Diagnostics { get; init; } = [];
    public bool LuaExecuted { get; init; }
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record LuaSandboxProbeStep
{
    public string StepId { get; init; } = string.Empty;
    public string Status { get; init; } = "not_run";
    public bool LuaExecuted { get; init; }
    public IReadOnlyList<string> TraceEventFamilies { get; init; } = [];
    public IReadOnlyList<LuaSandboxDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaSandboxTrace
{
    public string TraceId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string DecisionStatus { get; init; } = string.Empty;
    public bool LuaExecuted { get; init; }
    public IReadOnlyList<LuaSandboxProbeStep> ProbeSteps { get; init; } = [];
}

public sealed record LuaSandboxDryRunTraceMatrix
{
    public string SchemaVersion { get; init; } = "lua_sandbox_dry_run_trace_matrix_v1";
    public int TraceCount { get; init; }
    public bool LuaExecuted { get; init; }
    public IReadOnlyList<LuaSandboxTrace> Traces { get; init; } = [];
}

public sealed record LuaSandboxRepairAction
{
    public string ActionId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public bool MutatesAcceptedManifest { get; init; }
}

public sealed record LuaSandboxRepairPlan
{
    public string RepairPlanId { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public string DecisionStatus { get; init; } = string.Empty;
    public string Status { get; init; } = "planned";
    public IReadOnlyList<string> BlockingDiagnosticCodes { get; init; } = [];
    public IReadOnlyList<LuaSandboxRepairAction> Actions { get; init; } = [];
    public IReadOnlyList<string> ImmutableAcceptedManifestIds { get; init; } = [];
    public bool MutatesAcceptedManifests { get; init; }
}

public sealed record LuaSandboxRepairPlanMatrix
{
    public string SchemaVersion { get; init; } = "lua_sandbox_repair_plan_matrix_v1";
    public int RepairPlanCount { get; init; }
    public int RepairActionCount { get; init; }
    public bool MutatesAcceptedManifests { get; init; }
    public IReadOnlyList<LuaSandboxRepairPlan> RepairPlans { get; init; } = [];
}

public sealed record LuaSandboxInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<LuaSandboxDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaSandboxInvalidMatrix
{
    public string SchemaVersion { get; init; } = "invalid_lua_sandbox_diagnostics_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int NeedsRepairCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<LuaSandboxInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record LuaSandboxExecutionGateReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public int RequestCount { get; init; }
    public int DecisionCount { get; init; }
    public int TraceCount { get; init; }
    public int RepairPlanCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotManifestCount { get; init; }
    public bool LuaExecuted { get; init; }
    public bool LuaParserUsed { get; init; }
    public bool LuaSourceGenerated { get; init; }
    public bool ExternalDependencyAdded { get; init; }
    public bool RuntimeUiUnityGamePackageProviderLlmRagTouched { get; init; }
    public string PolicySummaryHash { get; init; } = string.Empty;
    public string HostBindingMatrixHash { get; init; } = string.Empty;
    public string RequestMatrixHash { get; init; } = string.Empty;
    public string DryRunTraceMatrixHash { get; init; } = string.Empty;
    public string RepairPlanMatrixHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> DecisionHashes { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<LuaSandboxDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaSandboxExecutionGateEvidenceResult
{
    public LuaSandboxPolicySummary PolicySummary { get; init; } = new();
    public LuaSandboxHostBindingMatrix HostBindingMatrix { get; init; } = new();
    public LuaSandboxExecutionRequestMatrix RequestMatrix { get; init; } = new();
    public IReadOnlyDictionary<string, LuaSandboxExecutionDecision> DecisionsByFileName { get; init; } = new Dictionary<string, LuaSandboxExecutionDecision>(StringComparer.Ordinal);
    public LuaSandboxDryRunTraceMatrix DryRunTraceMatrix { get; init; } = new();
    public LuaSandboxRepairPlanMatrix RepairPlanMatrix { get; init; } = new();
    public LuaSandboxInvalidMatrix InvalidMatrix { get; init; } = new();
    public LuaSandboxExecutionGateReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> DecisionJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record LuaSandboxExecutionGateEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record LuaSandboxDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
