using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Runtime.Abstractions;

public enum PlayerCommandType
{
    Move = 0,
    Interact = 1,
    UseItem = 2,
    UseAbility = 3,
    Wait = 4,
    ChooseDialogueOption = 5
}

public enum Direction2D
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

public sealed class PlayerCommand
{
    public PlayerCommandType Type { get; set; }
    public Direction2D Direction { get; set; } = Direction2D.None;
    public string? TargetId { get; set; }
    public string? Payload { get; set; }

    public static PlayerCommand Move(Direction2D direction) => new PlayerCommand { Type = PlayerCommandType.Move, Direction = direction };
    public static PlayerCommand Interact() => new PlayerCommand { Type = PlayerCommandType.Interact };
}

public sealed class GameState
{
    public string CurrentMapId { get; set; } = string.Empty;
    public Position2D PlayerPosition { get; set; } = new Position2D();
    public string Mode { get; set; } = "map";
    public Dictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();
}

public enum RuntimeEventType
{
    Message = 0,
    PlayerMoved = 1,
    MovementBlocked = 2,
    InteractionTriggered = 3,
    DialogueRequested = 4,
    SoundRequested = 5,
    MusicRequested = 6,
    Error = 7
}

public sealed class RuntimeEvent
{
    public RuntimeEventType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}

public sealed class CommandResult
{
    public GameState State { get; set; } = new GameState();
    public List<RuntimeEvent> Events { get; set; } = new List<RuntimeEvent>();
    public bool Success { get; set; } = true;
}

public interface IGameRuntime
{
    CommandResult Start(GamePackageDefinition package);
    CommandResult Execute(GamePackageDefinition package, GameState state, PlayerCommand command);
}

public interface IChunkGenerator
{
    string Id { get; }
}

public sealed class GameRuntimeState
{
    public string PackageId { get; set; } = string.Empty;
    public string CurrentMapId { get; set; } = string.Empty;
    public string PlayerEntityId { get; set; } = "player";
    public long Tick { get; set; }
    public List<InventoryState> Inventories { get; set; } = new List<InventoryState>();
    public List<EquipmentState> Equipment { get; set; } = new List<EquipmentState>();
    public List<ResourceState> Resources { get; set; } = new List<ResourceState>();
    public List<ProgressionState> Progressions { get; set; } = new List<ProgressionState>();
    public List<RuntimeFlagState> Flags { get; set; } = new List<RuntimeFlagState>();
    public List<StatusState> Statuses { get; set; } = new List<StatusState>();
    public EncounterRuntimeState? ActiveEncounter { get; set; }
    public Dictionary<string, string> QuestStates { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ProgressionState
{
    public string ProgressionId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? StageId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EncounterRuntimeState
{
    public string EncounterId { get; set; } = string.Empty;
    public string Kind { get; set; } = "combat";
    public bool Active { get; set; }
    public int Round { get; set; } = 1;
    public int TurnIndex { get; set; }
    public List<EncounterParticipantState> Participants { get; set; } = new List<EncounterParticipantState>();
    public List<string> ActionHistory { get; set; } = new List<string>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EncounterParticipantState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Team { get; set; } = "neutral";
    public bool Alive { get; set; } = true;
    public List<StatValueState> Stats { get; set; } = new List<StatValueState>();
    public List<ResourceState> Resources { get; set; } = new List<ResourceState>();
    public List<StatusState> Statuses { get; set; } = new List<StatusState>();
    public Dictionary<string, int> Cooldowns { get; set; } = new Dictionary<string, int>();
    public string? InventoryId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class StatValueState
{
    public string StatId { get; set; } = string.Empty;
    public double Value { get; set; }
}

public sealed class EquipmentState
{
    public string OwnerKind { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public List<EquipmentSlotState> Slots { get; set; } = new List<EquipmentSlotState>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class EquipmentSlotState
{
    public string SlotId { get; set; } = string.Empty;
    public string? ItemId { get; set; }
    public string? UniqueInstanceId { get; set; }
    public bool QuestItem { get; set; }
    public double? Durability { get; set; }
    public double? Charge { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class InventoryState
{
    public string Id { get; set; } = string.Empty;
    public string OwnerKind { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public List<ItemStackState> Stacks { get; set; } = new List<ItemStackState>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ItemStackState
{
    public string ItemId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? UniqueInstanceId { get; set; }
    public bool QuestItem { get; set; }
    public double? Durability { get; set; }
    public double? Charge { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class ResourceState
{
    public string ResourceId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public double? Capacity { get; set; }
    public string Scope { get; set; } = "global";
    public string? OwnerId { get; set; }
}

public sealed class StatusState
{
    public string StatusId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public long? RemainingTicks { get; set; }
    public int Stacks { get; set; } = 1;
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class RuntimeFlagState
{
    public string Id { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public enum GameRuntimeCommandType
{
    Initialize = 0,
    AddItem = 1,
    RemoveItem = 2,
    ChangeResource = 3,
    UseItem = 4,
    CraftRecipe = 5,
    RollLootTable = 6,
    ExecuteTransaction = 7,
    TickResourceNodes = 8,
    SetFlag = 9,
    Wait = 10,
    ExecuteInteraction = 11,
    EquipItem = 12,
    UnequipItem = 13,
    OpenContainer = 14,
    TakeFromContainer = 15,
    DepositToContainer = 16,
    HarvestResourceNode = 17,
    StartEncounter = 18,
    UseAbility = 19,
    BasicAttack = 20,
    EndTurn = 21,
    ResolveEncounter = 22,
    FleeEncounter = 23,
    RunCurrentTurnAi = 24
}

public sealed class GameRuntimeCommand
{
    public GameRuntimeCommandType Type { get; set; }
    public string? Id { get; set; }
    public string? InventoryId { get; set; }
    public string? TargetId { get; set; }
    public double Amount { get; set; }
    public int Ticks { get; set; } = 1;
    public int? Seed { get; set; }
    public string? Value { get; set; }
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();

    public static GameRuntimeCommand CraftRecipe(string recipeId, string? inventoryId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.CraftRecipe, Id = recipeId, InventoryId = inventoryId };

    public static GameRuntimeCommand RollLootTable(string lootTableId, string? inventoryId = null, int? seed = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.RollLootTable, Id = lootTableId, InventoryId = inventoryId, Seed = seed };

    public static GameRuntimeCommand ExecuteTransaction(string transactionId, string? inventoryId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.ExecuteTransaction, Id = transactionId, InventoryId = inventoryId };

    public static GameRuntimeCommand TickResourceNodes(int ticks = 1)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.TickResourceNodes, Ticks = ticks };

    public static GameRuntimeCommand UseItem(string itemId, string? inventoryId = null, string? targetId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.UseItem, Id = itemId, InventoryId = inventoryId, TargetId = targetId };

    public static GameRuntimeCommand ExecuteInteraction(string interactionId, string? targetId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.ExecuteInteraction, Id = interactionId, TargetId = targetId };

    public static GameRuntimeCommand EquipItem(string itemId, string slotId, string? inventoryId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.EquipItem, Id = itemId, TargetId = slotId, InventoryId = inventoryId };

    public static GameRuntimeCommand UnequipItem(string slotId, string? inventoryId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.UnequipItem, Id = slotId, InventoryId = inventoryId };

    public static GameRuntimeCommand OpenContainer(string containerInventoryId)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.OpenContainer, Id = containerInventoryId };

    public static GameRuntimeCommand TakeFromContainer(string containerInventoryId, string itemId, double amount = 1, string? inventoryId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.TakeFromContainer, Id = containerInventoryId, TargetId = itemId, Amount = amount, InventoryId = inventoryId };

    public static GameRuntimeCommand DepositToContainer(string containerInventoryId, string itemId, double amount = 1, string? inventoryId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.DepositToContainer, Id = containerInventoryId, TargetId = itemId, Amount = amount, InventoryId = inventoryId };

    public static GameRuntimeCommand HarvestResourceNode(string nodeId, string? inventoryId = null, string? toolItemId = null, int? seed = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.HarvestResourceNode, Id = nodeId, InventoryId = inventoryId, TargetId = toolItemId, Seed = seed };

    public static GameRuntimeCommand StartEncounter(string encounterId, int? seed = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.StartEncounter, Id = encounterId, Seed = seed };

    public static GameRuntimeCommand UseAbility(string abilityId, string sourceParticipantId, string? targetParticipantId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.UseAbility, Id = abilityId, TargetId = targetParticipantId, Args = new Dictionary<string, string> { ["sourceParticipantId"] = sourceParticipantId } };

    public static GameRuntimeCommand BasicAttack(string sourceParticipantId, string? targetParticipantId = null)
        => new GameRuntimeCommand { Type = GameRuntimeCommandType.BasicAttack, TargetId = targetParticipantId, Args = new Dictionary<string, string> { ["sourceParticipantId"] = sourceParticipantId } };
}

public enum GameRuntimeEventType
{
    GameStarted = 0,
    InventoryChanged = 1,
    ResourceChanged = 2,
    StatusAdded = 3,
    StatusRemoved = 4,
    RecipeCrafted = 5,
    LootRolled = 6,
    TransactionExecuted = 7,
    RequirementFailed = 8,
    CostConsumed = 9,
    OutputApplied = 10,
    ResourceNodeTicked = 11,
    LogMessageAdded = 12,
    ValidationFailed = 13,
    InteractionTriggered = 14,
    EquipmentChanged = 15,
    ContainerOpened = 16,
    ItemTransferred = 17,
    ResourceHarvested = 18,
    EncounterStarted = 19,
    TurnStarted = 20,
    AbilityUsed = 21,
    DamageApplied = 22,
    HealingApplied = 23,
    ParticipantDefeated = 24,
    EncounterWon = 25,
    EncounterLost = 26,
    EncounterEnded = 27,
    RewardGranted = 28,
    ProgressionChanged = 29,
    ProgressionStageChanged = 30,
    AiActionChosen = 31
}

public sealed class GameRuntimeEvent
{
    public GameRuntimeEventType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}

public sealed class RuntimeDiagnostic
{
    public string Code { get; set; } = string.Empty;
    public string Severity { get; set; } = "error";
    public string Message { get; set; } = string.Empty;
    public string? TargetId { get; set; }
}

public sealed class GameRuntimeResult
{
    public bool Success { get; set; } = true;
    public GameRuntimeState State { get; set; } = new GameRuntimeState();
    public List<GameRuntimeEvent> Events { get; set; } = new List<GameRuntimeEvent>();
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
    public string Message { get; set; } = string.Empty;
}

public sealed class RequirementEvaluationResult
{
    public bool Success => Failures.Count == 0;
    public List<RequirementFailure> Failures { get; set; } = new List<RequirementFailure>();
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
}

public sealed class RequirementFailure
{
    public string Code { get; set; } = string.Empty;
    public string RequirementKind { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class CostConsumptionResult
{
    public bool Success => Diagnostics.All(d => !d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
    public List<GameRuntimeEvent> Events { get; set; } = new List<GameRuntimeEvent>();
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
}

public sealed class OutputApplicationResult
{
    public bool Success => Diagnostics.All(d => !d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase));
    public List<GameRuntimeEvent> Events { get; set; } = new List<GameRuntimeEvent>();
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
}

public interface IGameRuntimeStateFactory
{
    GameRuntimeResult CreateInitialState(GamePackageDefinition package);
}

public interface IRequirementEvaluator
{
    RequirementEvaluationResult Evaluate(GamePackageDefinition package, GameRuntimeState state, IEnumerable<RequirementDefinition> requirements, string? inventoryId = null);
}

public interface ICostConsumer
{
    CostConsumptionResult Consume(GamePackageDefinition package, GameRuntimeState state, IEnumerable<CostDefinition> costs, string? inventoryId = null);
}

public interface IOutputApplier
{
    OutputApplicationResult Apply(GamePackageDefinition package, GameRuntimeState state, IEnumerable<OutputDefinition> outputs, string? inventoryId = null, int? seed = null);
}

public interface IRecipeRuntimeService
{
    GameRuntimeResult CraftRecipe(GamePackageDefinition package, GameRuntimeState state, string recipeId, string? inventoryId = null);
}

public interface ILootRuntimeService
{
    GameRuntimeResult RollLootTable(GamePackageDefinition package, GameRuntimeState state, string lootTableId, string? targetInventoryId = null, int? seed = null);
}

public interface ITransactionRuntimeService
{
    GameRuntimeResult ExecuteTransaction(GamePackageDefinition package, GameRuntimeState state, string transactionId, string? inventoryId = null);
}

public interface IResourceNetworkRuntimeService
{
    GameRuntimeResult TickResourceNodes(GamePackageDefinition package, GameRuntimeState state, int ticks = 1);
}

public interface IEquipmentRuntimeService
{
    GameRuntimeResult EquipItem(GamePackageDefinition package, GameRuntimeState state, string itemId, string slotId, string? inventoryId = null);
    GameRuntimeResult UnequipItem(GamePackageDefinition package, GameRuntimeState state, string slotId, string? inventoryId = null);
}

public interface IContainerRuntimeService
{
    GameRuntimeResult OpenContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId);
    GameRuntimeResult TakeFromContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId, string itemId, double amount, string? playerInventoryId = null);
    GameRuntimeResult DepositToContainer(GamePackageDefinition package, GameRuntimeState state, string containerInventoryId, string itemId, double amount, string? playerInventoryId = null);
}

public interface IHarvestRuntimeService
{
    GameRuntimeResult HarvestResourceNode(GamePackageDefinition package, GameRuntimeState state, string nodeId, string? inventoryId = null, string? toolItemId = null, int? seed = null);
}

public interface IUseItemRuntimeService
{
    GameRuntimeResult UseItem(GamePackageDefinition package, GameRuntimeState state, string itemId, string? inventoryId = null, string? targetId = null);
}

public interface IInteractionRuntimeService
{
    GameRuntimeResult ExecuteInteraction(GamePackageDefinition package, GameRuntimeState state, string interactionId, string? targetId = null, string? inventoryId = null);
}

public interface IEncounterRuntimeService
{
    GameRuntimeResult StartEncounter(GamePackageDefinition package, GameRuntimeState state, string encounterId, int? seed = null);
    GameRuntimeResult UseAbility(GamePackageDefinition package, GameRuntimeState state, string abilityId, string sourceParticipantId, string? targetParticipantId = null);
    GameRuntimeResult BasicAttack(GamePackageDefinition package, GameRuntimeState state, string sourceParticipantId, string? targetParticipantId = null);
    GameRuntimeResult EndTurn(GamePackageDefinition package, GameRuntimeState state);
    GameRuntimeResult FleeEncounter(GamePackageDefinition package, GameRuntimeState state);
    GameRuntimeResult ResolveEncounter(GamePackageDefinition package, GameRuntimeState state);
}

public interface IEncounterAiService
{
    GameRuntimeResult RunCurrentTurnAi(GamePackageDefinition package, GameRuntimeState state);
}

public interface IGameRuntimeService
{
    GameRuntimeResult CreateInitialState(GamePackageDefinition package);
    GameRuntimeResult Execute(GamePackageDefinition package, GameRuntimeState state, GameRuntimeCommand command);
    GameRuntimeResult ExecuteMany(GamePackageDefinition package, GameRuntimeState state, IEnumerable<GameRuntimeCommand> commands);
}

public sealed class UnifiedRuntimeSession
{
    public GameState MapState { get; set; } = new GameState();
    public GameRuntimeState GameplayState { get; set; } = new GameRuntimeState();
    public List<RuntimeEvent> MapEvents { get; set; } = new List<RuntimeEvent>();
    public List<GameRuntimeEvent> GameplayEvents { get; set; } = new List<GameRuntimeEvent>();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public sealed class UnifiedRuntimeResult
{
    public bool Success { get; set; } = true;
    public UnifiedRuntimeSession Session { get; set; } = new UnifiedRuntimeSession();
    public List<RuntimeEvent> MapEvents { get; set; } = new List<RuntimeEvent>();
    public List<GameRuntimeEvent> GameplayEvents { get; set; } = new List<GameRuntimeEvent>();
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
    public string Message { get; set; } = string.Empty;
}

public interface IUnifiedGameRuntimeService
{
    UnifiedRuntimeResult Start(GamePackageDefinition package);
    UnifiedRuntimeResult ExecutePlayerCommand(GamePackageDefinition package, UnifiedRuntimeSession session, PlayerCommand command);
    UnifiedRuntimeResult ExecuteGameplayCommand(GamePackageDefinition package, UnifiedRuntimeSession session, GameRuntimeCommand command);
    UnifiedRuntimeResult ExecuteMany(GamePackageDefinition package, UnifiedRuntimeSession session, IEnumerable<GameRuntimeCommand> commands);
}

public interface IRuntimeStateSerializer
{
    string Serialize(GameRuntimeState state);
    GameRuntimeState DeserializeGameRuntimeState(string json);
    string Serialize(UnifiedRuntimeSession session);
    UnifiedRuntimeSession DeserializeUnifiedSession(string json);
}

public sealed class RuntimeSnapshotResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SlotName { get; set; }
    public string? Path { get; set; }
    public UnifiedRuntimeSession? Session { get; set; }
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
}

public sealed class RuntimeSnapshotListResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> SlotNames { get; set; } = new List<string>();
    public List<RuntimeDiagnostic> Diagnostics { get; set; } = new List<RuntimeDiagnostic>();
}

public interface IRuntimeSnapshotStore
{
    RuntimeSnapshotResult SaveSnapshot(string projectFolder, string slotName, UnifiedRuntimeSession session);
    RuntimeSnapshotResult LoadSnapshot(string projectFolder, string slotName);
    RuntimeSnapshotListResult ListSnapshots(string projectFolder);
}
