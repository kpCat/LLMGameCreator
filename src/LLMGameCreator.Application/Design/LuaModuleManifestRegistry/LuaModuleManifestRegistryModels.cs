namespace LLMGameCreator.Application.Design.LuaModuleManifestRegistry;

public static class LuaModuleManifestVocabulary
{
    public const string SchemaVersion = "lua_module_manifest_registry_v1";

    public static readonly IReadOnlySet<string> LifecycleStatuses = new HashSet<string>(
        ["ready", "optional", "blocked", "future_required", "deprecated", "draft", "quarantined", "review_required"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> TargetDialects = new HashSet<string>(
        ["manifest_only", "lua_5_2_future", "lua_5_4_future", "lua_5_5_or_later_future"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> SourceKinds = new HashSet<string>(
        ["programmatic_registry", "manual", "imported_candidate", "llm_candidate", "goal_034_quarantined_candidate"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> PromotionStatuses = new HashSet<string>(
        ["review_required", "reviewed", "repair_required", "rejected", "quarantined", "promoted_not_materialized"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> SideEffectClasses = new HashSet<string>(
        ["none", "read_only", "planning_only", "metadata_only", "blocked"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> Scenarios = new HashSet<string>(
        ["frontier_survival", "gothic_intrigue", "caravan_trade", "metamodule_kingdoms"],
        StringComparer.Ordinal);
}

public sealed record LuaModuleFamilyDefinition
{
    public string FamilyId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> ArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> IntentFamilies { get; init; } = [];
    public string OrderingKey { get; init; } = string.Empty;
}

public sealed record LuaHostApiGroup
{
    public string GroupId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedOperationKinds { get; init; } = [];
    public IReadOnlyList<string> DeniedOperationKinds { get; init; } = [];
    public string SideEffectClass { get; init; } = "none";
    public IReadOnlyList<string> RequiredArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> RequiredSemanticScopes { get; init; } = [];
    public string DiagnosticCodePrefix { get; init; } = string.Empty;
}

public sealed record LuaModuleResourceBudget
{
    public int MaxInputRecords { get; init; }
    public int MaxOutputRecords { get; init; }
    public int MaxDependencyDepth { get; init; }
    public int MaxEstimatedMilliseconds { get; init; }
    public int MaxMemoryKb { get; init; }
}

public sealed record LuaModuleManifest
{
    public string ModuleId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public string DisplayName { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string LifecycleStatus { get; init; } = "draft";
    public string TargetDialect { get; init; } = "manifest_only";
    public string SourceKind { get; init; } = "programmatic_registry";
    public string ProvenanceId { get; init; } = string.Empty;
    public string ProvenanceDetails { get; init; } = string.Empty;
    public IReadOnlyList<string> ProfileCompatibility { get; init; } = [];
    public IReadOnlyList<string> ScenarioCompatibility { get; init; } = [];
    public IReadOnlyList<string> SemanticScopes { get; init; } = [];
    public IReadOnlyList<string> ArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> IntentFamilies { get; init; } = [];
    public IReadOnlyList<string> Dependencies { get; init; } = [];
    public IReadOnlyList<string> AllowedHostApiGroups { get; init; } = [];
    public IReadOnlyList<string> DeniedHostApiGroups { get; init; } = [];
    public IReadOnlyList<string> DeniedOperationKinds { get; init; } = [];
    public string SideEffectClass { get; init; } = "planning_only";
    public LuaModuleResourceBudget ResourceBudget { get; init; } = new();
    public string PromotionStatus { get; init; } = "review_required";
    public bool SelectableAsReady { get; init; }
    public bool ContainsFinalProse { get; init; }
    public bool DeclaresLuaSource { get; init; }
    public bool ClaimsLuaExecution { get; init; }
    public bool DeclaresProviderLlmRagAccess { get; init; }
    public bool DeclaresRuntimeUiUnityOrGamePackageMutation { get; init; }
    public string DeterministicOrderingKey { get; init; } = string.Empty;
}

public sealed record LuaModuleSelectionContext
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> AvailableArtifactContractIds { get; init; } = [];
    public IReadOnlyList<string> AvailableIntentFamilies { get; init; } = [];
    public IReadOnlyList<string> RequestedFamilyIds { get; init; } = [];
}

public sealed record LuaModuleMissingDependency
{
    public string ModuleId { get; init; } = string.Empty;
    public string MissingDependencyId { get; init; } = string.Empty;
}

public sealed record LuaModuleSelectionSummary
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public int SelectedCount { get; init; }
    public int BlockedCount { get; init; }
    public int FutureRequiredCount { get; init; }
    public int MissingDependencyCount { get; init; }
    public int DiagnosticCount { get; init; }
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record LuaModuleSelectionPlan
{
    public string SchemaVersion { get; init; } = "lua_module_selection_plan_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<LuaModuleManifest> SelectedManifests { get; init; } = [];
    public IReadOnlyList<string> DependencyOrder { get; init; } = [];
    public IReadOnlyList<LuaModuleManifest> BlockedManifests { get; init; } = [];
    public IReadOnlyList<LuaModuleManifest> FutureRequiredManifests { get; init; } = [];
    public IReadOnlyList<LuaModuleMissingDependency> MissingDependencies { get; init; } = [];
    public IReadOnlyList<LuaModuleManifestDiagnostic> DeniedApiDiagnostics { get; init; } = [];
    public IReadOnlyList<LuaModuleManifestDiagnostic> CompatibilityDiagnostics { get; init; } = [];
    public LuaModuleSelectionSummary Summary { get; init; } = new();
}

public sealed record LuaModuleDependencyPlan
{
    public string SchemaVersion { get; init; } = "lua_module_dependency_plan_v1";
    public IReadOnlyList<LuaModuleDependencyPlanRow> Rows { get; init; } = [];
    public int ScenarioCount { get; init; }
    public bool DependencyOrdersStable { get; init; }
}

public sealed record LuaModuleDependencyPlanRow
{
    public string ScenarioId { get; init; } = string.Empty;
    public IReadOnlyList<string> DependencyOrder { get; init; } = [];
    public IReadOnlyList<LuaModuleMissingDependency> MissingDependencies { get; init; } = [];
}

public sealed record LuaModuleRegistrySummary
{
    public string SchemaVersion { get; init; } = LuaModuleManifestVocabulary.SchemaVersion;
    public int FamilyCount { get; init; }
    public int ManifestCount { get; init; }
    public int ReadyManifestCount { get; init; }
    public int OptionalManifestCount { get; init; }
    public int FutureRequiredManifestCount { get; init; }
    public int QuarantinedManifestCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotManifestCount { get; init; }
    public IReadOnlyList<LuaModuleFamilyDefinition> Families { get; init; } = [];
    public IReadOnlyList<LuaModuleManifest> Manifests { get; init; } = [];
    public IReadOnlyList<LuaModuleManifestDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaHostApiSurfacePolicy
{
    public string SchemaVersion { get; init; } = "lua_host_api_surface_policy_v1";
    public int GroupCount { get; init; }
    public IReadOnlyList<string> AllowedGroupIds { get; init; } = [];
    public IReadOnlyList<string> DeniedGroupIds { get; init; } = [];
    public IReadOnlyList<LuaHostApiGroup> Groups { get; init; } = [];
    public IReadOnlyList<LuaModuleManifestDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaModuleManifestInvalidMatrix
{
    public string SchemaVersion { get; init; } = "lua_module_manifest_invalid_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<LuaModuleManifestInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record LuaModuleManifestInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<LuaModuleManifestDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaModuleManifestRegistryReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public int FamilyCount { get; init; }
    public int HostApiGroupCount { get; init; }
    public int ManifestCount { get; init; }
    public int SelectedScenarioCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotManifestCount { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool NoLuaExecutionOrParsing { get; init; } = true;
    public bool NoLuaSourceGenerated { get; init; } = true;
    public bool NoProviderLlmRagCallHappened { get; init; } = true;
    public bool NoRuntimeUiUnityGamePackageMutation { get; init; } = true;
    public string RegistrySummaryHash { get; init; } = string.Empty;
    public string HostApiPolicyHash { get; init; } = string.Empty;
    public string DependencyPlanHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectionPlanHashes { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<LuaModuleManifestDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record LuaModuleManifestEvidenceResult
{
    public LuaModuleRegistrySummary RegistrySummary { get; init; } = new();
    public LuaHostApiSurfacePolicy HostApiSurfacePolicy { get; init; } = new();
    public LuaModuleDependencyPlan DependencyPlan { get; init; } = new();
    public LuaModuleManifestInvalidMatrix InvalidMatrix { get; init; } = new();
    public LuaModuleManifestRegistryReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> SelectionJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record LuaModuleManifestEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record LuaModuleManifestDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
