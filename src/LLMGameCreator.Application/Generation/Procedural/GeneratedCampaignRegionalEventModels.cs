using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public enum GeneratedCampaignRegionalEventKind
{
    SUPPORT_GRATITUDE,
    CHALLENGE_AFTERMATH,
    REFUSAL_FALLOUT
}

public enum GeneratedCampaignRegionalEventStatus
{
    LOCKED,
    AVAILABLE,
    RESOLVED
}

public enum GeneratedCampaignRegionalEventReplayRouteKind
{
    LOCKED_PROBE,
    RESOLUTION
}

public enum GeneratedCampaignRegionalEventTargetRegionDerivation
{
    SUPPORT_FINAL_QUEST_REGION,
    EXACT_CHALLENGE_ENCOUNTER_REGION,
    RELATIONSHIP_HOME_FALLBACK,
    RELATIONSHIP_HOME_REGION
}

public sealed record GeneratedCampaignRegionalEventPrerequisite
{
    public string DecisionFlagId { get; init; } = string.Empty;
    public string DecisionFlagValue { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedQuestIds { get; init; } = [];
    public string ChallengeEncounterId { get; init; } = string.Empty;
    public string ChallengeVictoryFlagId { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignRegionalEventPlacement
{
    public string RegionId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public int ReachableDistance { get; init; }
    public string AnchorKind { get; init; } = string.Empty;
    public int AnchorX { get; init; }
    public int AnchorY { get; init; }
    public bool Walkable { get; init; }
    public bool Reachable { get; init; }
    public bool Safe { get; init; }
}

public sealed record GeneratedCampaignRegionalEventBinding
{
    public string RegionalEventId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventKind EventKind { get; init; }
    public string RelationshipId { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipBranch RelationshipBranch { get; init; }
    public string ActorSeedId { get; init; } = string.Empty;
    public string ActorEntityId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string EntityPrototypeId { get; init; } = string.Empty;
    public string MapEntityId { get; init; } = string.Empty;
    public string InteractionId { get; init; } = string.Empty;
    public string DialogueId { get; init; } = string.Empty;
    public string ResolutionFlagId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventPrerequisite Prerequisite { get; init; } = new();
    public GeneratedCampaignRegionalEventPlacement Placement { get; init; } = new();
    public string SourceQuestId { get; init; } = string.Empty;
    public string ChallengeEncounterId { get; init; } = string.Empty;
    public string ChallengeEncounterSourceId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventTargetRegionDerivation
        TargetRegionDerivation { get; init; }
    public string TargetRegionFingerprint { get; init; } = string.Empty;
    public string SourceQuestRewardFingerprint { get; init; } = string.Empty;
    public double ResolutionReputationDelta { get; init; }
    public string ResolutionChoiceId { get; init; } = string.Empty;
    public string DialogueDefinitionSha256 { get; init; } = string.Empty;
    public string InteractionDefinitionSha256 { get; init; } = string.Empty;
    public string EntityPrototypeDefinitionSha256 { get; init; } = string.Empty;
    public string MapEntityDefinitionSha256 { get; init; } = string.Empty;
    public string SourceQuestDefinitionSha256 { get; init; } = string.Empty;
    public string ChallengeEncounterDefinitionSha256 { get; init; } = string.Empty;
    public string PositionSha256 { get; init; } = string.Empty;
    public string InteractableReferencesSha256 { get; init; } = string.Empty;
    public string ResolutionRequirementsSha256 { get; init; } = string.Empty;
    public string ResolutionEffectsSha256 { get; init; } = string.Empty;
    public string EventMetadataSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignRegionalEventBindingResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<GeneratedCampaignRegionalEventBinding> Bindings { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventInventoryRow
{
    public string RegionalEventId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventKind EventKind { get; init; }
    public string RelationshipId { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipBranch RelationshipBranch { get; init; }
    public string ActorSeedId { get; init; } = string.Empty;
    public string ActorEntityId { get; init; } = string.Empty;
    public string FactionId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string EntityPrototypeId { get; init; } = string.Empty;
    public string MapEntityId { get; init; } = string.Empty;
    public string InteractionId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string DialogueId { get; init; } = string.Empty;
    public string ResolutionFlagId { get; init; } = string.Empty;
    public string SourceQuestId { get; init; } = string.Empty;
    public string ChallengeEncounterId { get; init; } = string.Empty;
    public string ChallengeEncounterSourceId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventTargetRegionDerivation
        TargetRegionDerivation { get; init; }
    public string TargetRegionFingerprint { get; init; } = string.Empty;
    public string PrerequisiteFingerprint { get; init; } = string.Empty;
    public string RewardDerivationFingerprint { get; init; } = string.Empty;
    public string ResolutionChoiceId { get; init; } = string.Empty;
    public string DialogueDefinitionSha256 { get; init; } = string.Empty;
    public string InteractionDefinitionSha256 { get; init; } = string.Empty;
    public string EntityPrototypeDefinitionSha256 { get; init; } = string.Empty;
    public string MapEntityDefinitionSha256 { get; init; } = string.Empty;
    public string SourceQuestDefinitionSha256 { get; init; } = string.Empty;
    public string ChallengeEncounterDefinitionSha256 { get; init; } = string.Empty;
    public string PositionSha256 { get; init; } = string.Empty;
    public string InteractableReferencesSha256 { get; init; } = string.Empty;
    public string ResolutionRequirementsSha256 { get; init; } = string.Empty;
    public string ResolutionEffectsSha256 { get; init; } = string.Empty;
    public string EventMetadataSha256 { get; init; } = string.Empty;
    public string EventSemanticFingerprint { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignRegionalEventDefinitionFingerprint
{
    public string CollectionPath { get; init; } = string.Empty;
    public string DefinitionId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignRegionalEventOverlayDocument
{
    public string SchemaVersion { get; init; } =
        "generated_campaign_regional_event_overlay_v1";
    public string SourcePackageSha256 { get; init; } = string.Empty;
    public string OutputPackageSha256 { get; init; } = string.Empty;
    public string EmptyOverlayPolicy { get; init; } =
        "EXACT_EMPTY_EVENT_GRAPH_V1";
    public int EventCount { get; init; }
    public int SupportGratitudeCount { get; init; }
    public int ChallengeAftermathCount { get; init; }
    public int RefusalFalloutCount { get; init; }
    public IReadOnlyList<GeneratedCampaignRegionalEventBinding> Bindings { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRegionalEventInventoryRow> Inventory { get; init; } = [];
    public string InventorySha256 { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignRegionalEventDefinitionFingerprint>
        AddedDefinitionFingerprints { get; init; } = [];
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsBefore { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> DefinitionCollectionCountsAfter { get; init; }
        = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public bool IdentityPassed { get; init; }
    public bool PlacementPassed { get; init; }
    public bool ControlledDeltaPassed { get; init; }
    public bool Deterministic { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventOverlayResult
{
    public bool Passed { get; init; }
    public GamePackageDefinition RegionalEventOverlayPackage { get; init; } = new();
    public string RegionalEventOverlayPackageJson { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventOverlayDocument Document { get; init; } = new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventOverlayValidationResult
{
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventRuntimeFrame
{
    public GeneratedCampaignRegionalEventReplayRouteKind RouteKind { get; init; }
    public int ReplayIndex { get; init; }
    public int SequenceIndex { get; init; }
    public string RegionalEventId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventStatus StatusBefore { get; init; }
    public GeneratedCampaignRegionalEventStatus StatusAfter { get; init; }
    public string CommandType { get; init; } = string.Empty;
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public string CommandSha256 { get; init; } = string.Empty;
    public string EventSha256 { get; init; } = string.Empty;
    public string MapEventSha256 { get; init; } = string.Empty;
    public string GameplayEventSha256 { get; init; } = string.Empty;
    public string AvailableChoiceIdsSha256 { get; init; } = string.Empty;
    public double ObservedReputation { get; init; }
    public double ObservedReputationDelta { get; init; }
    public string ObservedResolutionFlag { get; init; } = string.Empty;
    public string RelationshipFlagsSha256 { get; init; } = string.Empty;
    public string QuestStatesSha256 { get; init; } = string.Empty;
    public string EncounterStateSha256 { get; init; } = string.Empty;
    public bool NestedCombat { get; init; }
    public int NestedCombatSequenceIndex { get; init; } = -1;
    public string NestedCombatCommandIdentity { get; init; } = string.Empty;
    public string QualifiedDescriptorFingerprint { get; init; } = string.Empty;
    public string AbilityDefinitionSha256 { get; init; } = string.Empty;
    public string ObservedEffectClass { get; init; } = string.Empty;
    public string ObservedEffectFingerprint { get; init; } = string.Empty;
    public string NestedCombatMapEventSequenceSha256 { get; init; } = string.Empty;
    public string NestedCombatGameplayEventSequenceSha256 { get; init; } = string.Empty;
    public string EncounterStateBeforeSha256 { get; init; } = string.Empty;
    public string EncounterStateAfterSha256 { get; init; } = string.Empty;
    public int TurnBefore { get; init; } = -1;
    public int TurnAfter { get; init; } = -1;
    public int RoundBefore { get; init; } = -1;
    public int RoundAfter { get; init; } = -1;
    public bool CombatProgressObserved { get; init; }
    public string CombatOutcome { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GeneratedCampaignRegionalEventReplaySignature
{
    public string RegionalEventId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventReplayRouteKind RouteKind { get; init; }
    public int ReplayIndex { get; init; }
    public int FrameCount { get; init; }
    public string CommandSequenceSha256 { get; init; } = string.Empty;
    public string MapEventSequenceSha256 { get; init; } = string.Empty;
    public string GameplayEventSequenceSha256 { get; init; } = string.Empty;
    public string StatusTransitionSequenceSha256 { get; init; } = string.Empty;
    public string StateHashChainSha256 { get; init; } = string.Empty;
    public string AvailableChoiceSequenceSha256 { get; init; } = string.Empty;
    public string ReputationSequenceSha256 { get; init; } = string.Empty;
    public string ResolutionFlagSequenceSha256 { get; init; } = string.Empty;
    public string RelationshipFlagSequenceSha256 { get; init; } = string.Empty;
    public string QuestStateSequenceSha256 { get; init; } = string.Empty;
    public string EncounterStateSequenceSha256 { get; init; } = string.Empty;
    public int NestedCombatFrameCount { get; init; }
    public string NestedCombatCommandSequenceSha256 { get; init; } = string.Empty;
    public string NestedCombatEventSequenceSha256 { get; init; } = string.Empty;
    public string NestedCombatDescriptorSequenceSha256 { get; init; } = string.Empty;
    public string NestedCombatEffectSequenceSha256 { get; init; } = string.Empty;
    public string NestedCombatEncounterStateChainSha256 { get; init; } = string.Empty;
    public string NestedCombatTurnSequenceSha256 { get; init; } = string.Empty;
    public string NestedCombatTraceSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string SignatureSha256 { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GeneratedCampaignRegionalEventReplayComparison
{
    public bool Passed { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventQualification
{
    public string RegionalEventId { get; init; } = string.Empty;
    public GeneratedCampaignRegionalEventKind EventKind { get; init; }
    public string RelationshipId { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipBranch RelationshipBranch { get; init; }
    public bool LockedStatePassed { get; init; }
    public bool AvailableStatePassed { get; init; }
    public bool ResolvedStatePassed { get; init; }
    public bool ExactlyOncePassed { get; init; }
    public bool ReplayPassed { get; init; }
    public int RuntimeStartCount { get; init; }
    public int RuntimeCommandCount { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignRegionalEventReplaySignature>
        ReplaySignatures { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventHumanFact
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record GameProjectGeneratedCampaignRegionalEventSummary
{
    public const string StrictProofSchema =
        "generated_campaign_regional_event_strict_proof_v1";

    public string StrictProofSchemaVersion { get; init; } = string.Empty;
    public bool Present { get; init; }
    public bool Passed { get; init; }
    public string Status { get; init; } = "ABSENT";
    public int EventCount { get; init; }
    public int QualifiedEventCount { get; init; }
    public int SupportGratitudeCount { get; init; }
    public int ChallengeAftermathCount { get; init; }
    public int RefusalFalloutCount { get; init; }
    public bool IdentityPassed { get; init; }
    public bool PlacementPassed { get; init; }
    public bool OverlayControlledDeltaPassed { get; init; }
    public bool RuntimeQualificationPassed { get; init; }
    public bool LockedStatePassed { get; init; }
    public bool AvailableStatePassed { get; init; }
    public bool ResolvedStatePassed { get; init; }
    public bool ExactlyOncePassed { get; init; }
    public bool ReplayPassed { get; init; }
    public string ExactPackageSha256 { get; init; } = string.Empty;
    public string RegionalEventOverlaySha256 { get; init; } = string.Empty;
    public string RegionalEventInventorySha256 { get; init; } = string.Empty;
    public string RelationshipBranchMatrixSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string EmptyOverlayPolicy { get; init; } =
        "EXACT_EMPTY_EVENT_GRAPH_V1";
    public IReadOnlyList<GeneratedCampaignRegionalEventInventoryRow> EventInventory { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRegionalEventQualification> EventQualifications { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRegionalEventRuntimeFrame> RuntimeFrames { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRegionalEventReplaySignature>
        ReplaySignatures { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRegionalEventHumanFact> HumanReviewFacts { get; init; } = [];
    public IReadOnlyDictionary<string, string> TechnicalDetails { get; init; }
        = new SortedDictionary<string, string>(StringComparer.Ordinal);
    public GeneratedCampaignRegionalEventOverlayDocument? Overlay { get; init; }
    public GeneratedCampaignRegionalEventPayloadAuthority PayloadAuthority { get; init; } =
        new();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignRegionalEventPayloadAuthority
{
    public const string CurrentSchema =
        "generated_campaign_regional_event_payload_authority_v1";

    public string SchemaVersion { get; init; } = CurrentSchema;
    public string PackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string InventorySha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> RegionalEventIds { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRegionalEventReplaySignature>
        ReplaySignatures { get; init; } = [];
    public IReadOnlyDictionary<string, string> ComponentSha256 { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> FrameCounts { get; init; } =
        new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> NestedCombatTraceSha256 { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string AuthoritySha256 { get; init; } = string.Empty;
    public bool Passed { get; init; }
}

public sealed record GeneratedCampaignRegionalEventMigrationFact
{
    public string RegionalEventId { get; init; } = string.Empty;
    public bool Compatible { get; init; }
    public bool ResolutionFlagPreserved { get; init; }
    public bool StatusReset { get; init; }
    public string SourceEventFingerprint { get; init; } = string.Empty;
    public string TargetEventFingerprint { get; init; } = string.Empty;
    public bool DefinitionCorrelationPassed { get; init; }
    public bool MarkerDefinitionPreserved { get; init; }
    public bool PrototypeDefinitionPreserved { get; init; }
    public bool DialogueDefinitionPreserved { get; init; }
    public bool InteractionDefinitionPreserved { get; init; }
    public bool PlacementChanged { get; init; }
    public string PlacementPolicy { get; init; } =
        "EXACT_PLACEMENT_REQUIRED";
    public string DroppedReason { get; init; } = string.Empty;
}
