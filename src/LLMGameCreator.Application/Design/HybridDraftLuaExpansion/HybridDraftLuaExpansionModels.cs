namespace LLMGameCreator.Application.Design.HybridDraftLuaExpansion;

public static class HybridDraftLuaExpansionVocabulary
{
    public const string SchemaVersion = "hybrid_draft_lua_expansion_v1";
    public const string FinalGate = "hybrid_llm_draft_lua_deterministic_expansion_verification";
    public const string ProductSmokeRoute = "goal-037-hybrid-llm-draft-lua-deterministic-expansion";

    public static readonly IReadOnlySet<string> Scenarios = new HashSet<string>(
        ["frontier_survival", "gothic_intrigue", "caravan_trade", "metamodule_kingdoms"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> ArtifactFamilies = new HashSet<string>(
        [
            "npc_species_archetype_expansion_hints",
            "region_faction_kingdom_expansion_hints",
            "quest_event_intent_expansion_hints",
            "economy_combat_settlement_expansion_hints",
            "metamodule_species_archetype_slot_expansion"
        ],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> PromotionStatuses = new HashSet<string>(
        ["accepted", "rejected", "repair_required", "blocked"],
        StringComparer.Ordinal);
}

public sealed record HybridExecutorCapabilityFlags
{
    public bool RealLuaExecution { get; init; }
    public bool RepoOwnedFixtureOnly { get; init; } = true;
    public bool ArbitraryUserLuaAllowed { get; init; }
    public bool StandardLibrariesOpened { get; init; }
    public bool FilesystemExposed { get; init; }
    public bool NetworkExposed { get; init; }
    public bool ProcessExposed { get; init; }
    public bool ReflectionExposed { get; init; }
    public bool ThreadingExposed { get; init; }
    public bool WallClockTimeExposed { get; init; }
    public bool RandomExposed { get; init; }
    public bool NativeInteropExposed { get; init; }
    public bool RuntimeUiUnityGamePackageProviderLlmRagExposed { get; init; }
    public bool CancellationTokenSupported { get; init; }
    public bool InstructionCountHookSupported { get; init; }
    public bool DeclarativeFixtureRestrictionRequired { get; init; }
}

public sealed record HybridExecutorAdapterSelection
{
    public string SchemaVersion { get; init; } = "hybrid_executor_adapter_selection_v1";
    public string AdapterId { get; init; } = "hybrid-lua-executor/luacsharp-0.5.5";
    public string PackageId { get; init; } = "LuaCSharp";
    public string PackageVersion { get; init; } = "0.5.5";
    public string LicenseExpression { get; init; } = "MIT";
    public string NuGetSource { get; init; } = "nuget.org";
    public string Status { get; init; } = "selected_real_bounded_executor";
    public bool LocalRestoreProbeSucceeded { get; init; }
    public bool TransitiveSourceGeneratorPackageObserved { get; init; }
    public bool SourceGeneratorAnalyzersExcludedByPackageMetadata { get; init; }
    public bool SafeApiIsolationProven { get; init; }
    public bool DependencyUnavailableOrUnsafe { get; init; }
    public string BlockerReason { get; init; } = string.Empty;
    public HybridExecutorCapabilityFlags CapabilityFlags { get; init; } = new();
    public IReadOnlyList<string> RiskNotes { get; init; } = [];
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record HybridPipelineStep
{
    public int Ordinal { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string SourceGoal { get; init; } = string.Empty;
    public string Responsibility { get; init; } = string.Empty;
}

public sealed record HybridPipelineSummary
{
    public string SchemaVersion { get; init; } = "hybrid_pipeline_summary_v1";
    public string GoalId { get; init; } = "goal_037_hybrid_llm_draft_plus_lua_deterministic_expansion";
    public string FinalGate { get; init; } = HybridDraftLuaExpansionVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string ProductSmokeRoute { get; init; } = HybridDraftLuaExpansionVocabulary.ProductSmokeRoute;
    public int ScenarioCount { get; init; }
    public int ExpansionRequestCount { get; init; }
    public int ExecutedRequestCount { get; init; }
    public int OutputCount { get; init; }
    public int MetamoduleSlotCount { get; init; }
    public bool RealBoundedExecutorPathProven { get; init; }
    public bool NoLiveLlmProviderRagCall { get; init; } = true;
    public bool NoRuntimeUiUnityGamePackageMutation { get; init; } = true;
    public bool NoFinalProse { get; init; } = true;
    public IReadOnlyList<HybridPipelineStep> Steps { get; init; } = [];
    public IReadOnlyList<string> RequiredFamiliesCovered { get; init; } = [];
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record HybridDraftLuaExpansionRequest
{
    public string ExecutionRequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string SourceDraftRequestId { get; init; } = string.Empty;
    public string SourceManifestId { get; init; } = string.Empty;
    public string SandboxDecisionId { get; init; } = string.Empty;
    public string Goal036DecisionStatus { get; init; } = string.Empty;
    public string SourceCategory { get; init; } = "repo_owned_fixture";
    public string ProducedArtifactFamily { get; init; } = string.Empty;
    public int OutputBudget { get; init; }
    public string FixtureId { get; init; } = string.Empty;
    public bool SandboxApprovedForGoal037Executor { get; init; }
    public bool ExecutorAttempted { get; init; }
    public bool SelfPromoted { get; init; }
    public bool ClaimsGamePackageMutation { get; init; }
    public IReadOnlyList<string> RequestedBoundaryGroups { get; init; } = [];
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record HybridDraftToLuaRequestMapRow
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExecutionRequestId { get; init; } = string.Empty;
    public string SourceDraftRequestId { get; init; } = string.Empty;
    public string SourceManifestId { get; init; } = string.Empty;
    public string SandboxDecisionId { get; init; } = string.Empty;
    public string ProducedArtifactFamily { get; init; } = string.Empty;
    public string FixtureId { get; init; } = string.Empty;
    public int OutputBudget { get; init; }
    public bool SandboxApprovedForGoal037Executor { get; init; }
}

public sealed record HybridDraftToLuaRequestMap
{
    public string SchemaVersion { get; init; } = "hybrid_draft_to_lua_request_map_v1";
    public int RequestCount { get; init; }
    public IReadOnlyList<HybridDraftToLuaRequestMapRow> Rows { get; init; } = [];
}

public sealed record HybridSandboxApprovalRow
{
    public string ScenarioId { get; init; } = string.Empty;
    public string SandboxDecisionId { get; init; } = string.Empty;
    public string Goal036DecisionStatus { get; init; } = string.Empty;
    public bool Goal036RejectedOrRepairRequired { get; init; }
    public bool Goal037AdapterAvailable { get; init; }
    public bool ApprovedForRepoOwnedFixtureExecution { get; init; }
    public string ApprovalReason { get; init; } = string.Empty;
}

public sealed record HybridSandboxApprovedExpansionMatrix
{
    public string SchemaVersion { get; init; } = "hybrid_sandbox_approved_expansion_matrix_v1";
    public int RowCount { get; init; }
    public int ApprovedCount { get; init; }
    public IReadOnlyList<HybridSandboxApprovalRow> Rows { get; init; } = [];
}

public sealed record HybridLuaFixture
{
    public string FixtureId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProducedArtifactFamily { get; init; } = string.Empty;
    public string SourceCategory { get; init; } = "repo_owned_fixture";
    public string ScriptText { get; init; } = string.Empty;
    public string ScriptHash { get; init; } = string.Empty;
    public bool DeclarativeOnly { get; init; } = true;
}

public sealed record HybridExpansionSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string SlotKind { get; init; } = string.Empty;
    public int Weight { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<string> RelationIds { get; init; } = [];
}

public sealed record HybridWeightedTag
{
    public string TagId { get; init; } = string.Empty;
    public int Weight { get; init; }
}

public sealed record HybridExpansionRelation
{
    public string RelationId { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
}

public sealed record HybridExpansionOutput
{
    public string SchemaVersion { get; init; } = "hybrid_lua_expansion_output_v1";
    public string StableId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string SourceDraftRequestId { get; init; } = string.Empty;
    public string SourceManifestId { get; init; } = string.Empty;
    public string SandboxDecisionId { get; init; } = string.Empty;
    public string ProducedArtifactFamily { get; init; } = string.Empty;
    public IReadOnlyList<HybridExpansionSlot> Slots { get; init; } = [];
    public IReadOnlyList<HybridWeightedTag> Tags { get; init; } = [];
    public IReadOnlyList<HybridExpansionRelation> Relations { get; init; } = [];
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
    public string PromotionStatus { get; init; } = "blocked";
    public string TraceHash { get; init; } = string.Empty;
    public string StructuralTraceSummary { get; init; } = string.Empty;
    public bool LuaExecuted { get; init; }
}

public sealed record HybridScenarioExpansionOutput
{
    public string SchemaVersion { get; init; } = "hybrid_lua_expansion_scenario_output_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public int OutputCount { get; init; }
    public int SlotCount { get; init; }
    public IReadOnlyList<HybridExpansionOutput> Outputs { get; init; } = [];
}

public sealed record HybridExecutorAdapterResult
{
    public string ExecutionRequestId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string FixtureId { get; init; } = string.Empty;
    public string Status { get; init; } = "blocked";
    public bool LuaExecuted { get; init; }
    public HybridExpansionOutput? Output { get; init; }
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record HybridPromotionDecision
{
    public string DecisionId { get; init; } = string.Empty;
    public string ExecutionRequestId { get; init; } = string.Empty;
    public string StableOutputId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string PromotionStatus { get; init; } = "blocked";
    public bool Promoted { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record HybridPromotionDecisionMatrix
{
    public string SchemaVersion { get; init; } = "hybrid_promotion_decision_matrix_v1";
    public int DecisionCount { get; init; }
    public int AcceptedCount { get; init; }
    public int RejectedCount { get; init; }
    public int RepairRequiredCount { get; init; }
    public int BlockedCount { get; init; }
    public IReadOnlyList<HybridPromotionDecision> Decisions { get; init; } = [];
}

public sealed record HybridInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record HybridInvalidMatrix
{
    public string SchemaVersion { get; init; } = "invalid_hybrid_expansion_diagnostics_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int RepairRequiredCount { get; init; }
    public int BlockedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<HybridInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record HybridDraftLuaExpansionReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = HybridDraftLuaExpansionVocabulary.FinalGate;
    public string ManualGate { get; init; } = HybridDraftLuaExpansionVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public string ProductSmokeRoute { get; init; } = HybridDraftLuaExpansionVocabulary.ProductSmokeRoute;
    public bool ContractProofPassed { get; init; }
    public bool RealBoundedExecutorPathProven { get; init; }
    public string AdapterId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string PackageVersion { get; init; } = string.Empty;
    public int ScenarioCount { get; init; }
    public int ExpansionRequestCount { get; init; }
    public int ExecutedRequestCount { get; init; }
    public int OutputCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool NoLiveLlmProviderRagCall { get; init; } = true;
    public bool NoFinalProse { get; init; } = true;
    public bool NoRuntimeUiUnityGamePackageMutation { get; init; } = true;
    public bool NoFilesystemNetworkProcessReflectionThreadTimeRandomNativeInterop { get; init; } = true;
    public string AdapterSelectionHash { get; init; } = string.Empty;
    public string PipelineSummaryHash { get; init; } = string.Empty;
    public string DraftRequestMapHash { get; init; } = string.Empty;
    public string SandboxMatrixHash { get; init; } = string.Empty;
    public string PromotionMatrixHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ScenarioOutputHashes { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<HybridDraftLuaDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record HybridDraftLuaExpansionEvidenceResult
{
    public HybridExecutorAdapterSelection AdapterSelection { get; init; } = new();
    public HybridPipelineSummary PipelineSummary { get; init; } = new();
    public HybridDraftToLuaRequestMap DraftToLuaRequestMap { get; init; } = new();
    public HybridSandboxApprovedExpansionMatrix SandboxApprovedExpansionMatrix { get; init; } = new();
    public IReadOnlyDictionary<string, HybridScenarioExpansionOutput> ScenarioOutputsByFileName { get; init; } = new Dictionary<string, HybridScenarioExpansionOutput>(StringComparer.Ordinal);
    public HybridPromotionDecisionMatrix PromotionDecisionMatrix { get; init; } = new();
    public HybridInvalidMatrix InvalidMatrix { get; init; } = new();
    public HybridDraftLuaExpansionReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ScenarioOutputJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record HybridDraftLuaExpansionEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record HybridDraftLuaDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
