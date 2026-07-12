using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class GameRuntimeService : IGameRuntimeService
{
    private readonly IGameRuntimeStateFactory _stateFactory;
    private readonly IRecipeRuntimeService _recipeRuntimeService;
    private readonly ILootRuntimeService _lootRuntimeService;
    private readonly ITransactionRuntimeService _transactionRuntimeService;
    private readonly IResourceNetworkRuntimeService _resourceNetworkRuntimeService;
    private readonly IUseItemRuntimeService _useItemRuntimeService;
    private readonly IInteractionRuntimeService _interactionRuntimeService;
    private readonly IEquipmentRuntimeService _equipmentRuntimeService;
    private readonly IContainerRuntimeService _containerRuntimeService;
    private readonly IHarvestRuntimeService _harvestRuntimeService;
    private readonly IEncounterRuntimeService _encounterRuntimeService;
    private readonly IEncounterAiService _encounterAiService;
    private readonly IQuestRuntimeService _questRuntimeService;
    private readonly IDialogueRuntimeService _dialogueRuntimeService;
    private readonly IFactionRuntimeService _factionRuntimeService;
    private readonly IQuestObjectiveTracker _questObjectiveTracker;
    private readonly IOutputApplier _outputApplier;

    public GameRuntimeService(
        IGameRuntimeStateFactory stateFactory,
        IRecipeRuntimeService recipeRuntimeService,
        ILootRuntimeService lootRuntimeService,
        ITransactionRuntimeService transactionRuntimeService,
        IResourceNetworkRuntimeService resourceNetworkRuntimeService,
        IUseItemRuntimeService useItemRuntimeService,
        IInteractionRuntimeService interactionRuntimeService,
        IEquipmentRuntimeService? equipmentRuntimeService = null,
        IContainerRuntimeService? containerRuntimeService = null,
        IHarvestRuntimeService? harvestRuntimeService = null,
        IEncounterRuntimeService? encounterRuntimeService = null,
        IEncounterAiService? encounterAiService = null,
        IQuestRuntimeService? questRuntimeService = null,
        IDialogueRuntimeService? dialogueRuntimeService = null,
        IFactionRuntimeService? factionRuntimeService = null,
        IQuestObjectiveTracker? questObjectiveTracker = null,
        IOutputApplier? outputApplier = null)
    {
        _stateFactory = stateFactory;
        _recipeRuntimeService = recipeRuntimeService;
        _lootRuntimeService = lootRuntimeService;
        _transactionRuntimeService = transactionRuntimeService;
        _resourceNetworkRuntimeService = resourceNetworkRuntimeService;
        _useItemRuntimeService = useItemRuntimeService;
        _interactionRuntimeService = interactionRuntimeService;
        _equipmentRuntimeService = equipmentRuntimeService ?? new EquipmentRuntimeService(new RequirementEvaluator());
        _containerRuntimeService = containerRuntimeService ?? new ContainerRuntimeService();
        _harvestRuntimeService = harvestRuntimeService ?? new HarvestRuntimeService(new RequirementEvaluator(), new CostConsumer(), new OutputApplier());
        _encounterRuntimeService = encounterRuntimeService ?? new EncounterRuntimeService(new RequirementEvaluator(), new OutputApplier());
        _encounterAiService = encounterAiService ?? new EncounterAiService(_encounterRuntimeService);
        _factionRuntimeService = factionRuntimeService ?? new FactionRuntimeService();
        _questRuntimeService = questRuntimeService ?? new QuestRuntimeService(new RequirementEvaluator(), new OutputApplier());
        _dialogueRuntimeService = dialogueRuntimeService ?? new DialogueRuntimeService(new RequirementEvaluator(), new CostConsumer(), new OutputApplier(), _questRuntimeService, _transactionRuntimeService, _encounterRuntimeService);
        _questObjectiveTracker = questObjectiveTracker ?? new QuestObjectiveTracker(_questRuntimeService);
        _outputApplier = outputApplier ?? new OutputApplier();
    }

    public GameRuntimeResult CreateInitialState(GamePackageDefinition package)
    {
        return _stateFactory.CreateInitialState(package);
    }

    public GameRuntimeResult Execute(GamePackageDefinition package, GameRuntimeState state, GameRuntimeCommand command)
    {
        switch (command.Type)
        {
            case GameRuntimeCommandType.Initialize:
                return CreateInitialState(package);
            case GameRuntimeCommandType.AddItem:
                return Track(package, state, AddItem(state, command));
            case GameRuntimeCommandType.RemoveItem:
                return Track(package, state, RemoveItem(state, command));
            case GameRuntimeCommandType.ChangeResource:
                return Track(package, state, ChangeResource(package, state, command));
            case GameRuntimeCommandType.CraftRecipe:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires recipe id.", null)
                    : Track(package, state, _recipeRuntimeService.CraftRecipe(package, state, command.Id.Trim(), command.InventoryId));
            case GameRuntimeCommandType.RollLootTable:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires loot table id.", null)
                    : Track(package, state, _lootRuntimeService.RollLootTable(package, state, command.Id.Trim(), command.InventoryId, command.Seed));
            case GameRuntimeCommandType.ExecuteTransaction:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires transaction id.", null)
                    : Track(package, state, _transactionRuntimeService.ExecuteTransaction(package, state, command.Id.Trim(), command.InventoryId));
            case GameRuntimeCommandType.TickResourceNodes:
            case GameRuntimeCommandType.Wait:
                return Track(package, state, _resourceNetworkRuntimeService.TickResourceNodes(package, state, Math.Max(1, command.Ticks)));
            case GameRuntimeCommandType.SetFlag:
                return Track(package, state, SetFlag(state, command));
            case GameRuntimeCommandType.UseItem:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires item id.", null)
                    : Track(package, state, _useItemRuntimeService.UseItem(package, state, command.Id.Trim(), command.InventoryId, command.TargetId));
            case GameRuntimeCommandType.ExecuteInteraction:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "Runtime command requires interaction id.", null)
                    : Track(package, state, _interactionRuntimeService.ExecuteInteraction(package, state, command.Id.Trim(), command.TargetId, command.InventoryId));
            case GameRuntimeCommandType.EquipItem:
                return string.IsNullOrWhiteSpace(command.Id) || string.IsNullOrWhiteSpace(command.TargetId)
                    ? Fail(state, "runtime.command.id_missing", "EquipItem requires item id and slot id.", command.Id ?? command.TargetId)
                    : Track(package, state, _equipmentRuntimeService.EquipItem(package, state, command.Id.Trim(), command.TargetId.Trim(), command.InventoryId));
            case GameRuntimeCommandType.UnequipItem:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "UnequipItem requires slot id.", null)
                    : Track(package, state, _equipmentRuntimeService.UnequipItem(package, state, command.Id.Trim(), command.InventoryId));
            case GameRuntimeCommandType.OpenContainer:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "OpenContainer requires container inventory id.", null)
                    : Track(package, state, _containerRuntimeService.OpenContainer(package, state, command.Id.Trim()));
            case GameRuntimeCommandType.TakeFromContainer:
                return string.IsNullOrWhiteSpace(command.Id) || string.IsNullOrWhiteSpace(command.TargetId)
                    ? Fail(state, "runtime.command.id_missing", "TakeFromContainer requires container inventory id and item id.", command.Id ?? command.TargetId)
                    : Track(package, state, _containerRuntimeService.TakeFromContainer(package, state, command.Id.Trim(), command.TargetId.Trim(), command.Amount <= 0 ? 1 : command.Amount, command.InventoryId));
            case GameRuntimeCommandType.DepositToContainer:
                return string.IsNullOrWhiteSpace(command.Id) || string.IsNullOrWhiteSpace(command.TargetId)
                    ? Fail(state, "runtime.command.id_missing", "DepositToContainer requires container inventory id and item id.", command.Id ?? command.TargetId)
                    : Track(package, state, _containerRuntimeService.DepositToContainer(package, state, command.Id.Trim(), command.TargetId.Trim(), command.Amount <= 0 ? 1 : command.Amount, command.InventoryId));
            case GameRuntimeCommandType.HarvestResourceNode:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "HarvestResourceNode requires resource node id.", null)
                    : Track(package, state, _harvestRuntimeService.HarvestResourceNode(package, state, command.Id.Trim(), command.InventoryId, command.TargetId, command.Seed));
            case GameRuntimeCommandType.StartEncounter:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "StartEncounter requires encounter id.", null)
                    : Track(package, state, _encounterRuntimeService.StartEncounter(package, state, command.Id.Trim(), command.Seed));
            case GameRuntimeCommandType.UseAbility:
                return string.IsNullOrWhiteSpace(command.Id) || !command.Args.TryGetValue("sourceParticipantId", out var sourceId) || string.IsNullOrWhiteSpace(sourceId)
                    ? Fail(state, "runtime.command.id_missing", "UseAbility requires ability id and sourceParticipantId arg.", command.Id)
                    : Track(package, state, _encounterRuntimeService.UseAbility(package, state, command.Id.Trim(), sourceId.Trim(), command.TargetId));
            case GameRuntimeCommandType.BasicAttack:
                return !command.Args.TryGetValue("sourceParticipantId", out var basicSourceId) || string.IsNullOrWhiteSpace(basicSourceId)
                    ? Fail(state, "runtime.command.id_missing", "BasicAttack requires sourceParticipantId arg.", command.Id)
                    : Track(package, state, _encounterRuntimeService.BasicAttack(package, state, basicSourceId.Trim(), command.TargetId));
            case GameRuntimeCommandType.EndTurn:
                return _encounterRuntimeService.EndTurn(package, state);
            case GameRuntimeCommandType.ResolveEncounter:
                return Track(package, state, _encounterRuntimeService.ResolveEncounter(package, state));
            case GameRuntimeCommandType.FleeEncounter:
                return _encounterRuntimeService.FleeEncounter(package, state);
            case GameRuntimeCommandType.RunCurrentTurnAi:
                return Track(package, state, _encounterAiService.RunCurrentTurnAi(package, state));
            case GameRuntimeCommandType.StartQuest:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "StartQuest requires quest id.", null)
                    : _questRuntimeService.StartQuest(package, state, command.Id.Trim());
            case GameRuntimeCommandType.AdvanceQuestObjective:
                return string.IsNullOrWhiteSpace(command.Id) || string.IsNullOrWhiteSpace(command.TargetId)
                    ? Fail(state, "runtime.command.id_missing", "AdvanceQuestObjective requires quest id and objective id.", command.Id ?? command.TargetId)
                    : _questRuntimeService.AdvanceQuestObjective(package, state, command.Id.Trim(), command.TargetId.Trim(), command.Amount <= 0 ? 1 : command.Amount);
            case GameRuntimeCommandType.SetQuestStage:
                return string.IsNullOrWhiteSpace(command.Id) || string.IsNullOrWhiteSpace(command.TargetId)
                    ? Fail(state, "runtime.command.id_missing", "SetQuestStage requires quest id and stage id.", command.Id ?? command.TargetId)
                    : _questRuntimeService.SetQuestStage(package, state, command.Id.Trim(), command.TargetId.Trim());
            case GameRuntimeCommandType.CompleteQuest:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "CompleteQuest requires quest id.", null)
                    : _questRuntimeService.CompleteQuest(package, state, command.Id.Trim());
            case GameRuntimeCommandType.FailQuest:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "FailQuest requires quest id.", null)
                    : _questRuntimeService.FailQuest(package, state, command.Id.Trim());
            case GameRuntimeCommandType.RefreshQuestObjectives:
                return _questRuntimeService.RefreshQuestObjectives(package, state);
            case GameRuntimeCommandType.OpenDialogue:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "OpenDialogue requires dialogue id.", null)
                    : Track(package, state, _dialogueRuntimeService.OpenDialogue(package, state, command.Id.Trim()));
            case GameRuntimeCommandType.ChooseDialogueOption:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "ChooseDialogueOption requires choice id.", null)
                    : Track(package, state, _dialogueRuntimeService.ChooseDialogueOption(package, state, command.Id.Trim(), command.InventoryId));
            case GameRuntimeCommandType.CloseDialogue:
                return _dialogueRuntimeService.CloseDialogue(package, state);
            case GameRuntimeCommandType.ChangeReputation:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "ChangeReputation requires faction id.", null)
                    : _factionRuntimeService.ChangeReputation(package, state, command.Id.Trim(), command.Amount);
            case GameRuntimeCommandType.SetReputation:
                return string.IsNullOrWhiteSpace(command.Id)
                    ? Fail(state, "runtime.command.id_missing", "SetReputation requires faction id.", null)
                    : _factionRuntimeService.SetReputation(package, state, command.Id.Trim(), command.Amount);
            case GameRuntimeCommandType.ChangeProgression:
                return Track(package, state, ChangeProgression(package, state, command));
            default:
                return Fail(state, "runtime.command.unknown", $"Unknown runtime command: {command.Type}", command.Type.ToString());
        }
    }

    public GameRuntimeResult ExecuteMany(GamePackageDefinition package, GameRuntimeState state, IEnumerable<GameRuntimeCommand> commands)
    {
        var aggregate = new GameRuntimeResult { State = state, Success = true };
        foreach (var command in commands)
        {
            var result = Execute(package, state, command);
            aggregate.Events.AddRange(result.Events);
            aggregate.Diagnostics.AddRange(result.Diagnostics);
            aggregate.State = result.State;
            aggregate.Success = aggregate.Success && result.Success;
            aggregate.Message = result.Message;
            if (!result.Success)
            {
                break;
            }
        }

        return aggregate;
    }

    private static GameRuntimeResult AddItem(GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires item id.", null);
        }

        var itemId = command.Id.Trim();
        var amount = command.Amount <= 0 ? 1 : command.Amount;
        var inventory = RuntimeStateHelpers.FindInventory(state, command.InventoryId) ?? RuntimeStateHelpers.EnsurePlayerInventory(state);
        RuntimeStateHelpers.AddItem(inventory, itemId, amount);
        return Success(state, $"Added item {itemId} x{Format(amount)}", GameRuntimeEventType.InventoryChanged, inventory.Id);
    }

    private static GameRuntimeResult RemoveItem(GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires item id.", null);
        }

        var itemId = command.Id.Trim();
        var amount = command.Amount <= 0 ? 1 : command.Amount;
        var inventory = RuntimeStateHelpers.FindInventory(state, command.InventoryId);
        if (inventory == null || !RuntimeStateHelpers.RemoveItem(inventory, itemId, amount))
        {
            return Fail(state, "runtime.item_missing", $"Missing item {itemId} x{Format(amount)}", itemId);
        }

        return Success(state, $"Removed item {itemId} x{Format(amount)}", GameRuntimeEventType.InventoryChanged, inventory.Id);
    }

    private static GameRuntimeResult ChangeResource(GamePackageDefinition package, GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires resource id.", null);
        }

        var resourceId = command.Id.Trim();
        var resource = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, resourceId));
        if (resource == null)
        {
            return Fail(state, "runtime.resource_missing", $"Resource not found: {resourceId}", resourceId);
        }

        RuntimeStateHelpers.ChangeResource(state, resource, command.Amount);
        return Success(state, $"Changed resource {resourceId} by {Format(command.Amount)}", GameRuntimeEventType.ResourceChanged, resourceId);
    }

    private static GameRuntimeResult SetFlag(GameRuntimeState state, GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
        {
            return Fail(state, "runtime.command.id_missing", "Runtime command requires flag id.", null);
        }

        var flagId = command.Id.Trim();
        var value = command.Value ?? "true";
        RuntimeStateHelpers.SetFlag(state, flagId, value);
        return Success(state, $"Set flag {flagId} = {value}", GameRuntimeEventType.OutputApplied, flagId);
    }

    private GameRuntimeResult ChangeProgression(
        GamePackageDefinition package,
        GameRuntimeState state,
        GameRuntimeCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Id))
            return Fail(state, "runtime.command.id_missing", "ChangeProgression requires progression id.", null);
        if (!double.IsFinite(command.Amount) || command.Amount == 0)
            return Fail(state, "runtime.command.amount_invalid", "ChangeProgression amount must be finite and non-zero.", command.Id);
        var application = _outputApplier.Apply(package, state,
        [
            new OutputDefinition
            {
                Kind = "change_progression",
                Id = command.Id.Trim(),
                Amount = command.Amount
            }
        ]);
        return new GameRuntimeResult
        {
            Success = application.Success,
            State = state,
            Message = application.Success
                ? "Changed progression " + command.Id + " by " + Format(command.Amount)
                : "ChangeProgression failed: " + command.Id,
            Events = application.Events,
            Diagnostics = application.Diagnostics
        };
    }

    private static GameRuntimeResult Success(GameRuntimeState state, string message, GameRuntimeEventType eventType, string? targetId)
    {
        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = message,
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(eventType, message, targetId) }
        };
    }

    private GameRuntimeResult Track(GamePackageDefinition package, GameRuntimeState state, GameRuntimeResult result)
    {
        if (!result.Success || result.Events.Count == 0)
        {
            return result;
        }

        var tracking = _questObjectiveTracker.Track(package, state, result.Events);
        result.Events.AddRange(tracking.Events);
        result.Diagnostics.AddRange(tracking.Diagnostics);
        result.Success = result.Success && tracking.Success;
        return result;
    }

    private static GameRuntimeResult Fail(GameRuntimeState state, string code, string message, string? targetId)
    {
        return new GameRuntimeResult
        {
            Success = false,
            State = state,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) },
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, message, targetId) }
        };
    }

    private static string Format(double value)
    {
        return value.ToString("0.####");
    }
}
