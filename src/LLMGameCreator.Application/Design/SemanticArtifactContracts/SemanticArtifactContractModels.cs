namespace LLMGameCreator.Application.Design.SemanticArtifactContracts;

public sealed record SemanticArtifactContractDescriptor
{
    public string ContractId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public IReadOnlyList<string> ProducedArtifactTypes { get; init; } = [];
    public IReadOnlyList<string> ConsumedArtifactTypes { get; init; } = [];
    public IReadOnlyList<string> RequiredSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> OptionalSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> CapabilityTags { get; init; } = [];
    public IReadOnlyList<string> CompatibilityTags { get; init; } = [];
    public IReadOnlyList<string> Dependencies { get; init; } = [];
    public string ModuleOwner { get; init; } = string.Empty;
    public string ModuleId { get; init; } = string.Empty;
    public string LifecycleStatus { get; init; } = string.Empty;
    public string DiagnosticCodePrefix { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

public sealed record SemanticPackDescriptor
{
    public string PackId { get; init; } = string.Empty;
    public IReadOnlyList<string> SupportedProfileIds { get; init; } = [];
    public IReadOnlyList<string> SemanticScopes { get; init; } = [];
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
    public IReadOnlyList<string> RelationHints { get; init; } = [];
    public IReadOnlyList<string> ExpansionHints { get; init; } = [];
    public IReadOnlyList<string> BlockedCapabilityHints { get; init; } = [];
    public IReadOnlyList<string> FutureCapabilityHints { get; init; } = [];
    public string OrderingKey { get; init; } = string.Empty;
}

public sealed record SemanticCompatibilityRequest
{
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticPackDescriptor> SelectedSemanticPacks { get; init; } = [];
    public IReadOnlyList<string> RequestedContractIds { get; init; } = [];
    public IReadOnlySet<string> AvailableModuleIds { get; init; } = new HashSet<string>(StringComparer.Ordinal);
}

public sealed record SemanticCompatibilityPlan
{
    public string SchemaVersion { get; init; } = "semantic_artifact_compatibility_plan_v1";
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedContractIds { get; init; } = [];
    public IReadOnlyList<string> SelectedSemanticPackIds { get; init; } = [];
    public IReadOnlyList<string> DependencyOrder { get; init; } = [];
    public IReadOnlyList<SemanticMissingDependency> MissingDependencies { get; init; } = [];
    public IReadOnlyList<SemanticCompatibilityConflict> Conflicts { get; init; } = [];
    public IReadOnlyList<SemanticBlockedItem> BlockedOrFutureRequiredItems { get; init; } = [];
    public IReadOnlyList<SemanticExpansionSlot> SemanticExpansionSlots { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticExpansionSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string SlotFamily { get; init; } = string.Empty;
    public string SourceSemanticPackId { get; init; } = string.Empty;
    public string TargetArtifactContractId { get; init; } = string.Empty;
    public string TargetArtifactKind { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SemanticScopesUsed { get; init; } = [];
    public IReadOnlyList<string> SemanticTagsUsed { get; init; } = [];
    public int Priority { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticMissingDependency
{
    public string ContractId { get; init; } = string.Empty;
    public string MissingDependencyId { get; init; } = string.Empty;
}

public sealed record SemanticCompatibilityConflict
{
    public string ContractId { get; init; } = string.Empty;
    public string ConflictId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record SemanticBlockedItem
{
    public string ContractId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record SemanticArtifactRegistrySummary
{
    public string SchemaVersion { get; init; } = "semantic_artifact_contract_registry_summary_v1";
    public int ContractCount { get; init; }
    public IReadOnlyList<string> ContractIds { get; init; } = [];
    public IReadOnlyList<string> ReadyContractIds { get; init; } = [];
    public IReadOnlyList<string> OptionalContractIds { get; init; } = [];
    public IReadOnlyList<string> FutureRequiredContractIds { get; init; } = [];
    public IReadOnlyList<string> BlockedContractIds { get; init; } = [];
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticCompatibilityMatrix
{
    public string SchemaVersion { get; init; } = "semantic_artifact_compatibility_matrix_v1";
    public IReadOnlyList<SemanticCompatibilityMatrixRow> Rows { get; init; } = [];
    public bool PlannerSharedByAllScenarios { get; init; }
    public bool ScenariosAreMeaningfullyDifferent { get; init; }
}

public sealed record SemanticCompatibilityMatrixRow
{
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> PackIds { get; init; } = [];
    public int SelectedContractCount { get; init; }
    public int ExpansionSlotCount { get; init; }
    public IReadOnlyList<string> SlotFamilies { get; init; } = [];
    public IReadOnlyList<string> BlockedOrFutureRequiredContractIds { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record SemanticArtifactContractRegistryReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public int ContractCount { get; init; }
    public int ScenarioCount { get; init; }
    public bool RegistryValidated { get; init; }
    public bool CompatibilityPlannerShared { get; init; }
    public bool SemanticExpansionSlotsWritten { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool RuntimeBehaviorChanged { get; init; }
    public string RegistrySummaryHash { get; init; } = string.Empty;
    public string CompatibilityMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PlanHashes { get; init; } = [];
    public SemanticArtifactInvalidMatrix InvalidMatrix { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticArtifactInvalidMatrix
{
    public string SchemaVersion { get; init; } = "semantic_artifact_invalid_matrix_v1";
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<SemanticArtifactInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record SemanticArtifactInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticArtifactDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
