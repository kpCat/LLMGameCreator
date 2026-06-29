using LLMGameCreator.Application.Design.SemanticArtifactContracts;

namespace LLMGameCreator.Application.Design.SemanticPackComposition;

public sealed record SemanticPackCompositionPack
{
    public string PackId { get; init; } = string.Empty;
    public IReadOnlyList<string> SupportedProfileIds { get; init; } = [];
    public IReadOnlyList<string> ProvidedSemanticScopes { get; init; } = [];
    public IReadOnlyList<string> ThemeTags { get; init; } = [];
    public IReadOnlyList<SemanticPackFact> Facts { get; init; } = [];
    public IReadOnlyList<SemanticPackRelationHint> RelationHints { get; init; } = [];
    public IReadOnlyList<string> Exclusions { get; init; } = [];
    public IReadOnlyList<SemanticPackExpansionIntent> ExpansionIntents { get; init; } = [];
    public int Priority { get; init; }
    public string OrderingKey { get; init; } = string.Empty;
    public bool IsOptional { get; init; }
    public bool IsFutureOnly { get; init; }
    public string SourceStatus { get; init; } = "ready";
    public string SourceNotes { get; init; } = string.Empty;
}

public sealed record SemanticPackFact
{
    public string FactId { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = [];
    public string SourceNote { get; init; } = string.Empty;
}

public sealed record SemanticPackRelationHint
{
    public string RelationId { get; init; } = string.Empty;
    public string SourceFactId { get; init; } = string.Empty;
    public string RelationKind { get; init; } = string.Empty;
    public string TargetFactId { get; init; } = string.Empty;
    public bool Directed { get; init; } = true;
    public string SourceNote { get; init; } = string.Empty;
}

public sealed record SemanticPackExpansionIntent
{
    public string IntentId { get; init; } = string.Empty;
    public string SourceFactId { get; init; } = string.Empty;
    public string TargetContractId { get; init; } = string.Empty;
    public string TargetArtifactKind { get; init; } = string.Empty;
    public int Priority { get; init; }
    public bool FutureRequired { get; init; }
}

public sealed record SemanticPackCompositionRequest
{
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedPackIds { get; init; } = [];
    public string ComplexityHint { get; init; } = "standard";
}

public sealed record SemanticBlueprintPlan
{
    public string SchemaVersion { get; init; } = "semantic_blueprint_plan_v1";
    public string ProfileId { get; init; } = string.Empty;
    public string ComplexityHint { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedPackIds { get; init; } = [];
    public IReadOnlyList<SemanticRejectedPack> RejectedPacks { get; init; } = [];
    public IReadOnlyList<SemanticPackMergedFact> MergedSemanticFacts { get; init; } = [];
    public IReadOnlyList<SemanticPackRelationHint> RelationGraph { get; init; } = [];
    public IReadOnlyList<SemanticResolvedExpansionIntent> ResolvedExpansionIntents { get; init; } = [];
    public IReadOnlyList<string> Goal030CoverageContractIds { get; init; } = [];
    public IReadOnlyList<SemanticCrossArtifactLink> CrossArtifactLinks { get; init; } = [];
    public IReadOnlyList<SemanticBlueprintSection> Sections { get; init; } = [];
    public IReadOnlyList<SemanticBlueprintGap> CoverageGapsAndFutureRequiredItems { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticPackMergedFact
{
    public string FactId { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string SourcePackId { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record SemanticRejectedPack
{
    public string PackId { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record SemanticResolvedExpansionIntent
{
    public string IntentId { get; init; } = string.Empty;
    public string SourcePackId { get; init; } = string.Empty;
    public string SourceFactId { get; init; } = string.Empty;
    public string TargetContractId { get; init; } = string.Empty;
    public string TargetArtifactKind { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int Priority { get; init; }
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticCrossArtifactLink
{
    public string LinkId { get; init; } = string.Empty;
    public IReadOnlyList<string> FactPath { get; init; } = [];
    public IReadOnlyList<string> ContractIds { get; init; } = [];
    public string Summary { get; init; } = string.Empty;
}

public sealed record SemanticBlueprintSection
{
    public string SectionId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<string> FactIds { get; init; } = [];
    public IReadOnlyList<string> RelationIds { get; init; } = [];
    public IReadOnlyList<string> ExpansionIntentIds { get; init; } = [];
    public string Summary { get; init; } = string.Empty;
}

public sealed record SemanticBlueprintGap
{
    public string GapId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record SemanticPackCatalogSummary
{
    public string SchemaVersion { get; init; } = "semantic_pack_catalog_summary_v1";
    public int PackCount { get; init; }
    public int FactCount { get; init; }
    public IReadOnlyList<string> PackIds { get; init; } = [];
    public IReadOnlyDictionary<string, int> FactDomainCounts { get; init; } = new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<string> ProfileIds { get; init; } = [];
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticPackCompositionMatrix
{
    public string SchemaVersion { get; init; } = "semantic_pack_composition_matrix_v1";
    public IReadOnlyList<SemanticPackCompositionMatrixRow> Rows { get; init; } = [];
    public bool ComposerSharedByAllScenarios { get; init; }
    public bool Goal030PlannerUsedByAllScenarios { get; init; }
    public bool ScenariosAreMeaningfullyDifferent { get; init; }
}

public sealed record SemanticPackCompositionMatrixRow
{
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> SelectedPackIds { get; init; } = [];
    public int FactCount { get; init; }
    public int RelationCount { get; init; }
    public int LinkCount { get; init; }
    public int SectionCount { get; init; }
    public IReadOnlyList<string> CoverageContractIds { get; init; } = [];
    public string StableSummary { get; init; } = string.Empty;
}

public sealed record SemanticPackLinkageReport
{
    public string SchemaVersion { get; init; } = "semantic_pack_cross_artifact_linkage_report_v1";
    public int ScenarioCount { get; init; }
    public int LinkCount { get; init; }
    public IReadOnlyList<SemanticCrossArtifactLink> Links { get; init; } = [];
}

public sealed record SemanticPackCompositionBlueprintReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public bool BlueprintProofPassed { get; init; }
    public int PackCount { get; init; }
    public int ScenarioCount { get; init; }
    public bool CatalogValidated { get; init; }
    public bool ComposerShared { get; init; }
    public bool Goal030PlannerIntegrated { get; init; }
    public bool CrossArtifactLinksWritten { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public bool UnityBuildExecuted { get; init; }
    public bool LlmRagProviderMediaLuaExecuted { get; init; }
    public bool RuntimeBehaviorChanged { get; init; }
    public string CatalogSummaryHash { get; init; } = string.Empty;
    public string CompositionMatrixHash { get; init; } = string.Empty;
    public string CrossArtifactLinkageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> PlanHashes { get; init; } = [];
    public SemanticPackCompositionInvalidMatrix InvalidMatrix { get; init; } = new();
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record SemanticPackCompositionInvalidMatrix
{
    public string SchemaVersion { get; init; } = "semantic_pack_composition_invalid_matrix_v1";
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<SemanticPackCompositionInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record SemanticPackCompositionInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public IReadOnlyList<SemanticArtifactDiagnostic> Diagnostics { get; init; } = [];
}
