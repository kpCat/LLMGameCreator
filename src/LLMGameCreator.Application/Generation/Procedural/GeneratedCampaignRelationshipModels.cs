using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public enum GeneratedCampaignRelationshipBranch
{
    SUPPORT,
    CHALLENGE,
    REFUSE
}

public enum GeneratedCampaignRelationshipStatus
{
    UNDECIDED,
    SUPPORTED,
    QUEST_ACTIVE,
    QUEST_READY,
    COMPLETED,
    CHALLENGED,
    CHALLENGE_RESOLVED,
    REFUSED
}

public sealed record GeneratedCampaignQuestArcStep
{
    public int Order { get; init; }
    public int RegionDistance { get; init; }
    public string QuestId { get; init; } = string.Empty;
    public string QuestSourceId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string TargetEncounterId { get; init; } = string.Empty;
    public string TargetEncounterSourceId { get; init; } = string.Empty;
    public string TargetItemId { get; init; } = string.Empty;
    public string TargetItemSourceId { get; init; } = string.Empty;
    public double ReputationReward { get; init; }
}

public sealed record GeneratedCampaignRelationshipBinding
{
    public string RelationshipId { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string DecisionFlagId { get; init; } = string.Empty;
    public string ActorSeedId { get; init; } = string.Empty;
    public string ActorEntityId { get; init; } = string.Empty;
    public string InteractionId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string ChallengeEncounterId { get; init; } = string.Empty;
    public double SupportReputationAmount { get; init; }
    public double RefuseReputationAmount { get; init; }
    public IReadOnlyList<GeneratedCampaignRelationshipBranch> Branches { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignQuestArcStep> QuestArc { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipBindingResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedCampaignRelationshipBinding> Bindings { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipDefinitionFingerprint
{
    public string CollectionPath { get; init; } = string.Empty;
    public string DefinitionId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignRelationshipInventoryRow
{
    public string RelationshipId { get; init; } = string.Empty;
    public string ActorSeedId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignRelationshipBranch> BranchKinds { get; init; } = [];
    public IReadOnlyList<string> OrderedQuestSourceIds { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipOverlayDocument
{
    public string SchemaVersion { get; init; } = "generated_campaign_relationship_overlay_v1";
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string OutputPackageSha256 { get; init; } = string.Empty;
    public int RelationshipCount { get; init; }
    public int ArcQuestCount { get; init; }
    public IReadOnlyList<GeneratedCampaignRelationshipBinding> Bindings { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipDefinitionFingerprint> FingerprintsBefore { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipDefinitionFingerprint> FingerprintsAfter { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipInventoryRow> Inventory { get; init; } = [];
    public string InventorySha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedFieldPaths { get; init; } = [];
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsBefore { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsAfter { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public bool ControlledDeltaPassed { get; init; }
    public bool AssignmentUnique { get; init; }
    public bool ArcOrderingDeterministic { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipOverlayResult
{
    public bool Passed { get; init; }
    public GamePackageDefinition RelationshipOverlayPackage { get; init; } = new();
    public string RelationshipOverlayPackageJson { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipOverlayDocument Document { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipOverlayValidationResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipRuntimeFrame
{
    public int ReplayIndex { get; init; }
    public string RelationshipId { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipBranch Branch { get; init; }
    public int ArcStep { get; init; }
    public string QuestId { get; init; } = string.Empty;
    public string EncounterId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public string CommandSha256 { get; init; } = string.Empty;
    public string EventSha256 { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GeneratedCampaignRelationshipBranchQualification
{
    public string RelationshipId { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipBranch Branch { get; init; }
    public bool Available { get; init; }
    public bool Required { get; init; }
    public bool Passed { get; init; }
    public bool ReplayEquivalent { get; init; }
    public int RuntimeStartCount { get; init; }
    public int RuntimeCommandCount { get; init; }
    public int ArcLength { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipHumanFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record GameProjectGeneratedCampaignRelationshipSummary
{
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "ABSENT";
    public int RelationshipCount { get; init; }
    public int QualifiedRelationshipCount { get; init; }
    public int ArcQuestCount { get; init; }
    public int QualifiedArcQuestCount { get; init; }
    public int MaximumObservedArcLength { get; init; }
    public bool AssignmentUnique { get; init; }
    public bool ArcOrderingDeterministic { get; init; }
    public bool OverlayControlledDeltaPassed { get; init; }
    public bool RuntimeQualificationPassed { get; init; }
    public bool ExclusiveBranchingPassed { get; init; }
    public bool ArcProgressionPassed { get; init; }
    public bool ExactCombatCatalogPassed { get; init; }
    public bool SupportPassed { get; init; }
    public bool SupportReplayEquivalent { get; init; }
    public bool ChallengeFleePassed { get; init; }
    public bool ChallengeVictoryPassed { get; init; }
    public bool ChallengeRecoveryPassed { get; init; }
    public bool RefusePassed { get; init; }
    public bool AtomicRollbackPassed { get; init; }
    public bool SaveContinuationFactsPassed { get; init; }
    public string SaveContinuationFactsEvaluationStatus { get; init; } =
        "NOT_EVALUATED_AT_BUILD";
    public int SupportAvailableCount { get; init; }
    public int SupportRequiredCount { get; init; }
    public int SupportQualifiedCount { get; init; }
    public int ChallengeAvailableCount { get; init; }
    public int ChallengeRequiredCount { get; init; }
    public int ChallengeQualifiedCount { get; init; }
    public int RefuseAvailableCount { get; init; }
    public int RefuseRequiredCount { get; init; }
    public int RefuseQualifiedCount { get; init; }
    public int UnavailableBranchRuntimeStartCount { get; init; }
    public string ExactPackageSha256 { get; init; } = string.Empty;
    public string RelationshipOverlaySha256 { get; init; } = string.Empty;
    public string RelationshipInventorySha256 { get; init; } = string.Empty;
    public string RelationshipBranchMatrixSha256 { get; init; } = string.Empty;
    public string QualifiedActionsSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignRelationshipInventoryRow> RelationshipInventory { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipBranchQualification>
        BranchQualifications { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipRuntimeFrame> RuntimeFrames { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipHumanFact> HumanReviewFacts { get; init; } = [];
    public IReadOnlyDictionary<string, string> TechnicalDetails { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public GeneratedCampaignRelationshipOverlayDocument? Overlay { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipMigrationFact
{
    public string RelationshipId { get; init; } = string.Empty;
    public bool DecisionPreserved { get; init; }
    public bool ArcProgressPreserved { get; init; }
    public bool ArcProgressReset { get; init; }
    public string DroppedReason { get; init; } = string.Empty;
}
