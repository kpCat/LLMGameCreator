using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public enum GeneratedCampaignSessionStatus { NO_PROJECT, PROJECT_NOT_GENERATED, PROJECT_NOT_READY, READY, ACTIVE, STALE_PROJECT, SAVE_MIGRATION_REQUIRED, FAILED }
public enum GeneratedCampaignActionKind { MoveUp, MoveDown, MoveLeft, MoveRight, Interact, OpenDialogue, ChooseDialogue, CloseDialogue, StartEncounter, BasicAttack, UseAbility, EndTurn, RunEncounterAi, ResolveEncounter, FleeEncounter, CompleteQuest, UseItem, Save, Load, MigrateSave, RestartSession }

public sealed record GeneratedCampaignProjectTruth
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ProjectIdentityFingerprint { get; init; } = string.Empty;
    public string WorldId { get; init; } = string.Empty;
    public string SourceRecordSha256 { get; init; } = string.Empty;
    public string SourceRequestSha256 { get; init; } = string.Empty;
    public string PlanSha256 { get; init; } = string.Empty;
    public string GeneratedBasePackageSha256 { get; init; } = string.Empty;
    public string PackageSha256 { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string QualifiedAuthoringFingerprint { get; init; } = string.Empty;
    public string SelectedBuildHistoryFileName { get; init; } = string.Empty;
    public string GeneratedStartMapId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> RegionMapBindings { get; init; } = new Dictionary<string, string>();
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
}

public sealed record GeneratedCampaignMapCell { public int X { get; init; } public int Y { get; init; } public bool Walkable { get; init; } public bool PlayerPresent { get; init; } public string PrimarySymbol { get; init; } = "·"; public string PrimaryTitle { get; init; } = string.Empty; public int EntityCount { get; init; } public bool InteractionAvailable { get; init; } public bool Blocked { get; init; } }
public sealed record GeneratedCampaignMapEntity { public string Title { get; init; } = string.Empty; public int X { get; init; } public int Y { get; init; } public string Symbol { get; init; } = "•"; public bool Interactable { get; init; } }
public sealed record GeneratedCampaignMapProjection { public int Width { get; init; } public int Height { get; init; } public IReadOnlyList<GeneratedCampaignMapCell> Cells { get; init; } = []; public IReadOnlyList<GeneratedCampaignMapEntity> Entities { get; init; } = []; }
public sealed record GeneratedCampaignTextRow { public string Title { get; init; } = string.Empty; public string Value { get; init; } = string.Empty; }
public sealed record GeneratedCampaignDialogue { public string Title { get; init; } = string.Empty; public string Speaker { get; init; } = string.Empty; public string Text { get; init; } = string.Empty; public bool Open { get; init; } }
public sealed record GeneratedCampaignSaveState { public string Slot { get; init; } = "campaign"; public string Status { get; init; } = string.Empty; public int RevisionCount { get; init; } public string LastResult { get; init; } = string.Empty; }
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
    public IReadOnlyList<GeneratedCampaignAction> Actions { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Resources { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Stats { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Progressions { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Inventory { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> Equipment { get; init; } = [];
    public IReadOnlyList<GeneratedCampaignTextRow> ActiveQuests { get; init; } = [];
    public GeneratedCampaignDialogue? Dialogue { get; init; }
    public IReadOnlyList<GeneratedCampaignTextRow> Factions { get; init; } = [];
    public IReadOnlyList<string> RecentEvents { get; init; } = [];
    public GeneratedCampaignSaveState SaveState { get; init; } = new();
    public IReadOnlyDictionary<string, string> TechnicalDetails { get; init; } = new Dictionary<string, string>();
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

internal sealed record GeneratedCampaignSession(GeneratedCampaignProjectTruth Truth, UnifiedRuntimeSession RuntimeSession, string SlotName);
