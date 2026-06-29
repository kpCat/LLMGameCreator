using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;

namespace LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

public static class SemanticAuthoringIntentVocabulary
{
    public static readonly IReadOnlyList<string> DomainGroups =
    [
        "world",
        "kingdom",
        "region",
        "species",
        "archetype",
        "faction",
        "npc",
        "quest",
        "dialogue",
        "economy",
        "combat",
        "settlement",
        "event"
    ];

    public static readonly IReadOnlySet<string> ProvenanceKinds = new HashSet<string>(
        ["user", "programmatic", "inherited", "semantic_pack", "llm_candidate", "imported_candidate", "unset", "blocked"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> IntentFamilies = new HashSet<string>(
        ["npc_role", "relationship_pressure", "faction_reaction", "quest_motive", "dialogue_act", "event_intent", "economy_pressure", "combat_pressure", "settlement_need", "lore_gap"],
        StringComparer.Ordinal);
}

public sealed record SemanticAuthoringWorkspace
{
    public string SchemaVersion { get; init; } = "semantic_authoring_workspace_v1";
    public string WorkspaceId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticAuthoringDomainGroup> DomainGroups { get; init; } = [];
    public IReadOnlyList<SemanticAuthoringDiagnostic> Diagnostics { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record SemanticAuthoringDomainGroup
{
    public string DomainId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticAuthoringSection> Sections { get; init; } = [];
}

public sealed record SemanticAuthoringSection
{
    public string SectionId { get; init; } = string.Empty;
    public string SourceTargetId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticAuthoringField> Fields { get; init; } = [];
    public string CompletionStatus { get; init; } = "partial";
}

public sealed record SemanticAuthoringField
{
    public string FieldId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public string ValueKind { get; init; } = string.Empty;
    public string RequirementStatus { get; init; } = "optional";
    public bool Repeatable { get; init; }
    public string ApplicabilityHint { get; init; } = string.Empty;
    public string InheritanceHint { get; init; } = string.Empty;
    public string ControlHint { get; init; } = string.Empty;
    public string CompletionStatus { get; init; } = "partial";
    public string Provenance { get; init; } = "unset";
    public string ResolvedValueSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record SemanticAuthoringWorkspaceSchemaSummary
{
    public string SchemaVersion { get; init; } = "semantic_authoring_workspace_schema_summary_v1";
    public int WorkspaceCount { get; init; }
    public int DomainGroupCount { get; init; }
    public int FieldCount { get; init; }
    public IReadOnlyDictionary<string, int> FieldsByDomain { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> ProvenanceKinds { get; init; } = [];
    public IReadOnlyList<string> ValueKinds { get; init; } = [];
    public IReadOnlyList<SemanticAuthoringWorkspace> SampleWorkspaces { get; init; } = [];
    public UpstreamSemanticSeamSummary UpstreamSeams { get; init; } = new();
    public IReadOnlyList<SemanticAuthoringDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UpstreamSemanticSeamSummary
{
    public int Goal030ContractCount { get; init; }
    public int Goal031PackCount { get; init; }
    public int Goal032FeatureCount { get; init; }
    public int Goal032InfluenceRuleCount { get; init; }
    public IReadOnlyList<string> Goal030ReadyContractIds { get; init; } = [];
    public IReadOnlyList<string> Goal031PackIds { get; init; } = [];
    public IReadOnlyList<string> Goal032FeatureIds { get; init; } = [];
}

public sealed record LoreIntakeSkeleton
{
    public string SchemaVersion { get; init; } = "semantic_authoring_lore_intake_skeleton_v1";
    public string LoreBriefId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string StyleProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> WorldThemes { get; init; } = [];
    public IReadOnlyList<LoreKingdomSlot> KingdomSlots { get; init; } = [];
    public IReadOnlyList<LoreNamedSlot> RegionSlots { get; init; } = [];
    public IReadOnlyList<LoreSpeciesArchetypeSlot> SpeciesArchetypeSlots { get; init; } = [];
    public IReadOnlyList<string> MagicSystemAxes { get; init; } = [];
    public IReadOnlyList<string> ConflictAxes { get; init; } = [];
    public IReadOnlyList<LoreAuthoringSlot> ManualFillSlots { get; init; } = [];
    public IReadOnlyList<LoreAuthoringSlot> ProgrammaticallyInferableSlots { get; init; } = [];
    public IReadOnlyList<LoreAuthoringSlot> LlmCandidateSlots { get; init; } = [];
    public IReadOnlyList<string> FeatureFamilies { get; init; } = [];
    public LoreSkeletonEvidenceSummary EvidenceSummary { get; init; } = new();
}

public sealed record LoreKingdomSlot
{
    public string KingdomId { get; init; } = string.Empty;
    public string RegionFamily { get; init; } = string.Empty;
    public string PressureAxis { get; init; } = string.Empty;
}

public sealed record LoreNamedSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string Family { get; init; } = string.Empty;
}

public sealed record LoreSpeciesArchetypeSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string KingdomId { get; init; } = string.Empty;
    public string SpeciesFamily { get; init; } = string.Empty;
    public string ArchetypeFamily { get; init; } = string.Empty;
    public int Ordinal { get; init; }
}

public sealed record LoreAuthoringSlot
{
    public string SlotId { get; init; } = string.Empty;
    public string DomainId { get; init; } = string.Empty;
    public string FillMode { get; init; } = string.Empty;
    public string Provenance { get; init; } = string.Empty;
    public string ReviewStatus { get; init; } = string.Empty;
}

public sealed record LoreSkeletonEvidenceSummary
{
    public int KingdomCount { get; init; }
    public int RegionSlotCount { get; init; }
    public int SpeciesArchetypeSlotCount { get; init; }
    public IReadOnlyList<string> RepresentativeSpeciesArchetypeSlots { get; init; } = [];
    public bool LlmCandidatesQuarantined { get; init; }
}

public sealed record SemanticAuthoringIntentResolution
{
    public string SchemaVersion { get; init; } = "semantic_authoring_intent_resolution_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<SemanticContentIntentRecord> Intents { get; init; } = [];
    public IReadOnlyList<SemanticAuthoringDiagnostic> Diagnostics { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record SemanticContentIntentRecord
{
    public string IntentId { get; init; } = string.Empty;
    public string IntentFamily { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string TargetDomain { get; init; } = string.Empty;
    public IReadOnlyList<string> SourceFeatureIds { get; init; } = [];
    public string ResolvedFeatureValueSummary { get; init; } = string.Empty;
    public int Priority { get; init; }
    public double Weight { get; init; }
    public string TemplateHint { get; init; } = string.Empty;
    public string LocalizationKeyHint { get; init; } = string.Empty;
    public IReadOnlyList<string> BlockersOrGaps { get; init; } = [];
    public string ProvenanceSummary { get; init; } = string.Empty;
    public string TraceSummary { get; init; } = string.Empty;
}

public sealed record ManualVsAutoAuthoringMatrix
{
    public string SchemaVersion { get; init; } = "manual_vs_auto_authoring_matrix_v1";
    public IReadOnlyList<ManualVsAutoAuthoringMatrixRow> Rows { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record ManualVsAutoAuthoringMatrixRow
{
    public string CaseId { get; init; } = string.Empty;
    public string Provenance { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = string.Empty;
    public bool AcceptedAutomatically { get; init; }
    public string DiagnosticCode { get; init; } = string.Empty;
}

public sealed record SemanticAuthoringIntentInvalidMatrix
{
    public string SchemaVersion { get; init; } = "semantic_authoring_intent_invalid_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<SemanticAuthoringIntentInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record SemanticAuthoringIntentInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<SemanticAuthoringDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticAuthoringIntentResolverReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousProducedGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool ContractProofPassed { get; init; }
    public bool WorkspaceImplemented { get; init; }
    public bool LoreSkeletonImplemented { get; init; }
    public bool ProvenanceMatrixImplemented { get; init; }
    public bool IntentResolverImplemented { get; init; }
    public bool EvidenceArtifactsWritten { get; init; }
    public int WorkspaceFieldCount { get; init; }
    public int IntentCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotCount { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool UiChanged { get; init; }
    public bool RuntimeBehaviorChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool FinalDialogueProseGenerated { get; init; }
    public bool FinalGamePackageMaterialized { get; init; }
    public string WorkspaceSchemaSummaryHash { get; init; } = string.Empty;
    public string LoreSkeletonHash { get; init; } = string.Empty;
    public string ManualMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> IntentResolutionHashes { get; init; } = [];
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<SemanticAuthoringDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticAuthoringDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record SemanticAuthoringIntentEvidenceResult
{
    public SemanticAuthoringWorkspaceSchemaSummary WorkspaceSchemaSummary { get; init; } = new();
    public LoreIntakeSkeleton MetamoduleLoreSkeleton { get; init; } = new();
    public ManualVsAutoAuthoringMatrix ManualVsAutoAuthoringMatrix { get; init; } = new();
    public SemanticAuthoringIntentResolution FrontierResolution { get; init; } = new();
    public SemanticAuthoringIntentResolution GothicResolution { get; init; } = new();
    public SemanticAuthoringIntentResolution CaravanResolution { get; init; } = new();
    public SemanticAuthoringIntentResolution MetamoduleKingdomsResolution { get; init; } = new();
    public SemanticAuthoringIntentInvalidMatrix InvalidMatrix { get; init; } = new();
    public SemanticAuthoringIntentResolverReport Report { get; init; } = new();
    public string WorkspaceSchemaSummaryJson { get; init; } = string.Empty;
    public string MetamoduleLoreSkeletonJson { get; init; } = string.Empty;
    public string ManualVsAutoAuthoringMatrixJson { get; init; } = string.Empty;
    public string FrontierResolutionJson { get; init; } = string.Empty;
    public string GothicResolutionJson { get; init; } = string.Empty;
    public string CaravanResolutionJson { get; init; } = string.Empty;
    public string MetamoduleKingdomsResolutionJson { get; init; } = string.Empty;
    public string InvalidMatrixJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record SemanticAuthoringIntentEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string WorkspaceSchemaSummaryJsonPath { get; init; } = string.Empty;
    public string MetamoduleLoreSkeletonJsonPath { get; init; } = string.Empty;
    public string ManualVsAutoAuthoringMatrixJsonPath { get; init; } = string.Empty;
    public string FrontierResolutionJsonPath { get; init; } = string.Empty;
    public string GothicResolutionJsonPath { get; init; } = string.Empty;
    public string CaravanResolutionJsonPath { get; init; } = string.Empty;
    public string MetamoduleKingdomsResolutionJsonPath { get; init; } = string.Empty;
    public string InvalidMatrixJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
}
