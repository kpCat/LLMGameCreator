using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public enum GeneratedCampaignSessionStatus
{
    NO_PROJECT,
    PROJECT_NOT_GENERATED,
    PROJECT_NOT_READY,
    READY,
    ACTIVE,
    DEFEATED,
    STALE_PROJECT,
    SAVE_MIGRATION_REQUIRED,
    FAILED
}

public enum GeneratedCampaignActionKind
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Interact,
    OpenDialogue,
    ChooseDialogue,
    CloseDialogue,
    StartEncounter,
    BasicAttack,
    UseAbility,
    EndTurn,
    RunEncounterAi,
    ResolveEncounter,
    FleeEncounter,
    CompleteQuest,
    UseItem,
    Save,
    Load,
    MigrateSave,
    RestartSession,
    RetryEncounter,
    RecoveryLoad,
    NewGame
}

public sealed record GeneratedCampaignProjectTruth
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ProjectIdentityFingerprint { get; init; } = string.Empty;
    public string WorldId { get; init; } = string.Empty;
    public string GenerationSeed { get; init; } = string.Empty;
    public string SourceRecordSha256 { get; init; } = string.Empty;
    public string SourceRequestSha256 { get; init; } = string.Empty;
    public string PlanSha256 { get; init; } = string.Empty;
    public string GeneratedBasePackageSha256 { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string SelectedBuildHistorySha256 { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string GeneratedStartMapId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> RegionMapBindings { get; init; }
        = new Dictionary<string, string>();
    public GeneratedCampaignRelationshipOverlayDocument? RelationshipOverlay { get; init; }
}

public sealed record GeneratedCampaignAction
{
    public string ActionId { get; init; } = string.Empty;
    public GeneratedCampaignActionKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public string DisabledReason { get; init; } = string.Empty;
    public bool Primary { get; init; }
    public string TargetTitle { get; init; } = string.Empty;
    public GeneratedCampaignTacticalAction? Tactical { get; init; }
    public string TechnicalChoiceId { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignTacticalAction
{
    public string Title { get; init; } = string.Empty;
    public string TargetTitle { get; init; } = string.Empty;
    public string CostSummary { get; init; } = string.Empty;
    public string EffectSummary { get; init; } = string.Empty;
    public string AvailabilitySummary { get; init; } = string.Empty;
    public bool ProgressesEncounter { get; init; }
    public bool Primary { get; init; }
}

public sealed record GeneratedCampaignMapCell
{
    public int X { get; init; }
    public int Y { get; init; }
    public bool Walkable { get; init; }
    public bool PlayerPresent { get; init; }
    public string PrimarySymbol { get; init; } = "·";
    public string PrimaryTitle { get; init; } = string.Empty;
    public int EntityCount { get; init; }
    public bool InteractionAvailable { get; init; }
    public bool Blocked { get; init; }
}

public sealed record GeneratedCampaignMapEntity
{
    public string Title { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string Symbol { get; init; } = "•";
    public bool Interactable { get; init; }
}

public sealed record GeneratedCampaignMapProjection
{
    public int Width { get; init; }
    public int Height { get; init; }
    public IReadOnlyList<GeneratedCampaignMapCell> Cells { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignMapEntity> Entities { get; init; } = [];
}

public sealed record GeneratedCampaignTextRow
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignPlayerProjection
{
    public string Title { get; init; } = "Игрок";
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed record GeneratedCampaignNearbyProjection
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool InteractionAvailable { get; init; }
}

public sealed record GeneratedCampaignDialogueChoice
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignChoiceOption
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ActorTitle { get; init; } = string.Empty;
    public string FactionTitle { get; init; } = string.Empty;
    public string? QuestTitle { get; init; }
    public string? EncounterTitle { get; init; }
    public IReadOnlyList<string> ConsequencePreview { get; init; } = [];
    public bool Enabled { get; init; }
    public string DisabledReason { get; init; } = string.Empty;
    public GeneratedCampaignBranchKind? BranchKind { get; init; }
    public bool Primary { get; init; }
    public string TechnicalChoiceId { get; init; } = string.Empty;
    public string BeforeStateHash { get; init; } = string.Empty;
    public string AfterStateHash { get; init; } = string.Empty;
    public string ObservedFlagValue { get; init; } = string.Empty;
    public double ObservedReputationDelta { get; init; }
    public string ObservedQuestState { get; init; } = string.Empty;
    public string ObservedEncounterId { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeEventTypes { get; init; } = [];
}

public sealed record GeneratedCampaignDialogueChoicePreview
{
    public string DialogueId { get; init; } = string.Empty;
    public string OriginalSessionSha256 { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> RuntimeAvailableChoiceIds { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignChoiceOption> Options { get; init; } = [];
}

public enum GeneratedCampaignDecisionStatus { Chosen, FollowUpAvailable, Completed }

public sealed record GeneratedCampaignDecision
{
    public string ActorTitle { get; init; } = string.Empty;
    public string ChosenBranch { get; init; } = string.Empty;
    public string Consequence { get; init; } = string.Empty;
    public string RelatedContent { get; init; } = string.Empty;
    public GeneratedCampaignDecisionStatus Status { get; init; }
    public bool AlternativesLocked { get; init; }
}

public sealed record GeneratedCampaignDecisionJournal
{
    public IReadOnlyList<GeneratedCampaignDecision> Decisions { get; init; } = [];
}

public sealed record GeneratedCampaignDialogue
{
    public string Title { get; init; } = string.Empty;
    public string Speaker { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public bool Open { get; init; }
    public IReadOnlyList<GeneratedCampaignDialogueChoice> Choices { get; init; } = [];
}

public sealed record GeneratedCampaignEncounterParticipant
{
    public string Title { get; init; } = string.Empty;
    public string TeamTitle { get; init; } = string.Empty;
    public bool Alive { get; init; }
    public bool CurrentTurn { get; init; }
    public IReadOnlyList<GeneratedCampaignTextRow> Resources { get; init; } = [];
}

public sealed record GeneratedCampaignEncounter
{
    public string Title { get; init; } = string.Empty;
    public bool Active { get; init; }
    public int Round { get; init; }
    public string CurrentTurnTitle { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignEncounterParticipant> Participants { get; init; } = [];
}

public sealed record GeneratedCampaignQuestObjective
{
    public string Title { get; init; } = string.Empty;
    public string Progress { get; init; } = string.Empty;
    public bool Completed { get; init; }
}

public sealed record GeneratedCampaignQuest
{
    public string Title { get; init; } = string.Empty;
    public string StateTitle { get; init; } = string.Empty;
    public bool Completable { get; init; }
    public IReadOnlyList<GeneratedCampaignQuestObjective> Objectives { get; init; } = [];
}

public sealed record GeneratedCampaignSaveState
{
    public string Slot { get; init; } = "campaign";
    public string Status { get; init; } = string.Empty;
    public int RevisionCount { get; init; }
    public bool Deduplicated { get; init; }
    public string LastResult { get; init; } = string.Empty;
}

public sealed record GeneratedCampaignSaveEntryProjection
{
    public GeneratedGameplaySaveEntry Entry { get; init; } = new();
    public string Slot { get; init; } = string.Empty;
    public string StatusTitle { get; init; } = string.Empty;
    public string SavedWorldTitle { get; init; } = string.Empty;
    public string CurrentWorldTitle { get; init; } = string.Empty;
    public int RevisionCount { get; init; }
    public string MigrationSummary { get; init; } = string.Empty;
    public bool CanContinue { get; init; }
    public bool CanMigrate { get; init; }
}

public sealed record GeneratedCampaignRecoveryProjection
{
    public bool Available { get; init; }
    public string EncounterTitle { get; init; } = string.Empty;
    public bool RetryEnabled { get; init; }
    public bool ContinueEnabled { get; init; }
    public bool NewGameEnabled { get; init; }
    public string DisabledReason { get; init; } = string.Empty;
}

public enum GeneratedCampaignConsequenceKind
{
    Dialogue,
    Damage,
    Healing,
    Status,
    EncounterStarted,
    EncounterWon,
    EncounterLost,
    EncounterFled,
    Reward,
    Inventory,
    QuestReady,
    QuestCompleted,
    Reputation,
    MapTravel,
    Save,
    Load,
    Migration,
    Defeat,
    Retry,
    RecoveryLoad,
    NewGame,
    TacticalAction,
    Decision,
    RelationshipStarted,
    RelationshipProgressed,
    RelationshipCompleted,
    RelationshipChallenged,
    RelationshipRefused,
    QuestArcAdvanced,
    BranchLocked,
    BranchFollowUp,
    Failure
}

public enum GeneratedCampaignConsequenceTone
{
    Positive,
    Negative,
    Neutral
}

public sealed record GeneratedCampaignConsequence
{
    public GeneratedCampaignConsequenceKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string BeforeValue { get; init; } = string.Empty;
    public string AfterValue { get; init; } = string.Empty;
    public string Delta { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public GeneratedCampaignConsequenceTone Tone { get; init; }
}

public sealed record GeneratedCampaignActionOutcome
{
    public string ActionTitle { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedCampaignConsequence> Consequences { get; init; } = [];
    public string BeforeSessionSha256 { get; init; } = string.Empty;
    public string AfterSessionSha256 { get; init; } = string.Empty;
    public int RuntimeEventCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record GeneratedCampaignConsequenceTimeline
{
    public const int DefaultMaximumEntries = 64;
    public int MaximumEntries { get; init; } = DefaultMaximumEntries;
    public IReadOnlyList<GeneratedCampaignConsequence> Entries { get; init; } = [];
}

public sealed record GeneratedCampaignSnapshot
{
    public GeneratedCampaignSessionStatus Status { get; init; }
    public string StatusTitle { get; init; } = string.Empty;
    public string StatusDescription { get; init; } = string.Empty;
    public string ProjectTitle { get; init; } = string.Empty;
    public string WorldTitle { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string CurrentRegionTitle { get; init; } = string.Empty;
    public string CurrentMapTitle { get; init; } = string.Empty;
    public string SessionSha256 { get; init; } = string.Empty;
    public GeneratedCampaignMapProjection? Map { get; init; }
    public GeneratedCampaignPlayerProjection? Player { get; init; }
    public IReadOnlyList<GeneratedCampaignNearbyProjection> Nearby { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignAction> Actions { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Resources { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Stats { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Progressions { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Inventory { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Equipment { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> ActiveQuests { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignQuest> Quests { get; init; } = [];
    public GeneratedCampaignDialogue? Dialogue { get; init; }
    public GeneratedCampaignDialogueChoicePreview? ChoicePreview { get; init; }
    public GeneratedCampaignDecisionJournal DecisionJournal { get; init; } = new();
    public GeneratedCampaignEncounter? Encounter { get; init; }
    public IReadOnlyList<GeneratedCampaignTextRow> Factions { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignRelationshipRow> Relationships { get; init; } = [];
    public IReadOnlyList<string> RecentEvents { get; init; } = [];
    public GeneratedCampaignSaveState SaveState { get; init; } = new();
    public GeneratedCampaignRecoveryProjection Recovery { get; init; } = new();
    public GeneratedCampaignActionOutcome? LastActionOutcome { get; init; }
    public IReadOnlyList<GeneratedCampaignConsequence> Consequences { get; init; } = [];
    public IReadOnlyDictionary<string, string> TechnicalDetails { get; init; }
        = new Dictionary<string, string>();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

internal sealed record GeneratedCampaignSession(
    GeneratedCampaignProjectTruth Truth,
    GamePackageDefinition Package,
    UnifiedRuntimeSession RuntimeSession,
    string SlotName,
    IReadOnlyList<GeneratedEncounterCombatQualifiedAction> QualifiedActions);
