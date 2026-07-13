using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class DialogueRuntimeService : IDialogueRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly ICostConsumer _costConsumer;
    private readonly IOutputApplier _outputApplier;
    private readonly IQuestRuntimeService _questRuntimeService;
    private readonly ITransactionRuntimeService _transactionRuntimeService;
    private readonly IEncounterRuntimeService _encounterRuntimeService;

    public DialogueRuntimeService(
        IRequirementEvaluator requirementEvaluator,
        ICostConsumer costConsumer,
        IOutputApplier outputApplier,
        IQuestRuntimeService questRuntimeService,
        ITransactionRuntimeService transactionRuntimeService,
        IEncounterRuntimeService encounterRuntimeService)
    {
        _requirementEvaluator = requirementEvaluator;
        _costConsumer = costConsumer;
        _outputApplier = outputApplier;
        _questRuntimeService = questRuntimeService;
        _transactionRuntimeService = transactionRuntimeService;
        _encounterRuntimeService = encounterRuntimeService;
    }

    public GameRuntimeResult OpenDialogue(GamePackageDefinition package, GameRuntimeState state, string dialogueId)
    {
        var dialogue = package.Game.Dialogues.FirstOrDefault(d => RuntimeStateHelpers.IdEquals(d.Id, dialogueId));
        if (dialogue == null)
        {
            return Failure(state, "dialogue.missing", $"Dialogue not found: {dialogueId}", dialogueId);
        }

        var node = dialogue.Nodes.FirstOrDefault(n => RuntimeStateHelpers.IdEquals(n.Id, dialogue.StartNodeId));
        if (node == null)
        {
            return Failure(state, "dialogue.start_node_missing", $"Dialogue start node not found: {dialogue.StartNodeId}", dialogue.Id);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var requirements = _requirementEvaluator.Evaluate(package, working, dialogue.Conditions.Concat(node.Conditions));
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var enter = _outputApplier.Apply(package, working, dialogue.EnterEffects.Concat(node.EnterEffects));
        result.Events.AddRange(enter.Events);
        result.Diagnostics.AddRange(enter.Diagnostics);
        if (!enter.Success)
        {
            result.Success = false;
            result.Message = $"Dialogue open failed: {dialogue.Id}";
            return result;
        }

        working.ActiveDialogue = new DialogueRuntimeState
        {
            DialogueId = dialogue.Id,
            CurrentNodeId = node.Id,
            SpeakerId = node.SpeakerId,
            Open = true,
            History = new List<string> { node.Id },
            Metadata = new Dictionary<string, string>(dialogue.Metadata)
        };
        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Success = true;
        result.Message = $"Dialogue opened: {dialogue.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.DialogueOpened, result.Message, dialogue.Id, ChoiceArgs(package, state, dialogue, node)));
        return result;
    }

    public GameRuntimeResult ChooseDialogueOption(GamePackageDefinition package, GameRuntimeState state, string choiceId, string? inventoryId = null)
    {
        if (state.ActiveDialogue == null || !state.ActiveDialogue.Open)
        {
            return Failure(state, "dialogue.not_open", "No dialogue is open.", choiceId);
        }

        var dialogue = package.Game.Dialogues.FirstOrDefault(d => RuntimeStateHelpers.IdEquals(d.Id, state.ActiveDialogue.DialogueId));
        var node = dialogue?.Nodes.FirstOrDefault(n => RuntimeStateHelpers.IdEquals(n.Id, state.ActiveDialogue.CurrentNodeId));
        var choice = node?.Choices.FirstOrDefault(c => RuntimeStateHelpers.IdEquals(c.Id, choiceId));
        if (dialogue == null || node == null || choice == null)
        {
            return Failure(state, "dialogue.choice_missing", $"Dialogue choice not found: {choiceId}", choiceId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var conditionRequirements = choice.Conditions.Select(RuntimeEffectMapper.ToRequirement).Concat(choice.Requirements).ToList();
        var requirements = _requirementEvaluator.Evaluate(package, working, conditionRequirements, inventoryId);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var costs = _costConsumer.Consume(package, working, choice.Costs, inventoryId);
        result.Events.AddRange(costs.Events);
        result.Diagnostics.AddRange(costs.Diagnostics);
        if (!costs.Success)
        {
            return RolledBackFailure(state, result, $"Dialogue choice failed: {choice.Id}", choice.Id);
        }

        var outputs = choice.Effects.Select(RuntimeEffectMapper.ToOutput).Concat(choice.Rewards).ToList();
        var applied = _outputApplier.Apply(package, working, outputs, inventoryId);
        result.Events.AddRange(applied.Events);
        result.Diagnostics.AddRange(applied.Diagnostics);
        if (!applied.Success)
        {
            return RolledBackFailure(state, result, $"Dialogue choice effects failed: {choice.Id}", choice.Id);
        }

        if (!string.IsNullOrWhiteSpace(choice.StartQuestId))
        {
            var startQuest = _questRuntimeService.StartQuest(package, working, choice.StartQuestId!);
            result.Events.AddRange(startQuest.Events);
            result.Diagnostics.AddRange(startQuest.Diagnostics);
            if (!startQuest.Success)
            {
                return RolledBackFailure(state, result, startQuest.Message, choice.Id);
            }
        }

        if (!string.IsNullOrWhiteSpace(choice.AdvanceQuestId))
        {
            var objectiveId = choice.Metadata.TryGetValue("objective_id", out var value) ? value : choice.Id;
            var advance = _questRuntimeService.AdvanceQuestObjective(package, working, choice.AdvanceQuestId!, objectiveId, 1);
            result.Events.AddRange(advance.Events);
            result.Diagnostics.AddRange(advance.Diagnostics);
        }

        if (!string.IsNullOrWhiteSpace(choice.SetQuestStageId) && !string.IsNullOrWhiteSpace(choice.AdvanceQuestId))
        {
            var setStage = _questRuntimeService.SetQuestStage(package, working, choice.AdvanceQuestId!, choice.SetQuestStageId!);
            result.Events.AddRange(setStage.Events);
            result.Diagnostics.AddRange(setStage.Diagnostics);
        }

        if (!string.IsNullOrWhiteSpace(choice.OpenTransactionId))
        {
            var transaction = _transactionRuntimeService.ExecuteTransaction(package, working, choice.OpenTransactionId!, inventoryId);
            result.Events.AddRange(transaction.Events);
            result.Diagnostics.AddRange(transaction.Diagnostics);
        }

        if (!string.IsNullOrWhiteSpace(choice.StartEncounterId))
        {
            var encounter = _encounterRuntimeService.StartEncounter(package, working, choice.StartEncounterId!);
            result.Events.AddRange(encounter.Events);
            result.Diagnostics.AddRange(encounter.Diagnostics);
        }

        var active = working.ActiveDialogue!;
        active.History.Add(choice.Id);
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.DialogueChoiceSelected, $"Dialogue choice selected: {choice.Id}", choice.Id, new Dictionary<string, string>
        {
            ["dialogueId"] = dialogue.Id,
            ["nodeId"] = node.Id
        }));
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.DialogueEffectApplied, $"Dialogue effects applied: {choice.Id}", choice.Id));

        if (choice.CloseDialogue || string.IsNullOrWhiteSpace(choice.TargetNodeId))
        {
            var exit = _outputApplier.Apply(package, working, node.ExitEffects.Concat(dialogue.ExitEffects), inventoryId);
            result.Events.AddRange(exit.Events);
            result.Diagnostics.AddRange(exit.Diagnostics);
            if (!exit.Success)
                return RolledBackFailure(state, result, $"Dialogue close effects failed: {choice.Id}", choice.Id);
            active.Open = false;
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.DialogueClosed, $"Dialogue closed: {dialogue.Id}", dialogue.Id));
        }
        else
        {
            var target = dialogue.Nodes.FirstOrDefault(n => RuntimeStateHelpers.IdEquals(n.Id, choice.TargetNodeId));
            if (target == null)
            {
                return RolledBackFailure(state, result, $"Dialogue choice target node not found: {choice.TargetNodeId}", choice.Id);
            }

            active.CurrentNodeId = target.Id;
            active.SpeakerId = target.SpeakerId;
            active.History.Add(target.Id);
            var enter = _outputApplier.Apply(package, working, node.ExitEffects.Concat(target.EnterEffects), inventoryId);
            result.Events.AddRange(enter.Events);
            result.Diagnostics.AddRange(enter.Diagnostics);
            if (!enter.Success)
                return RolledBackFailure(state, result, $"Dialogue node effects failed: {choice.Id}", choice.Id);
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.DialogueNodeChanged, $"Dialogue node changed: {target.Id}", target.Id, ChoiceArgs(package, working, dialogue, target)));
        }

        if (result.Diagnostics.Any(d => d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
        {
            return RolledBackFailure(state, result, $"Dialogue choice failed: {choice.Id}", choice.Id);
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Success = true;
        result.Message = $"Dialogue choice selected: {choice.Id}";
        return result;
    }

    public GameRuntimeResult CloseDialogue(GamePackageDefinition package, GameRuntimeState state)
    {
        if (state.ActiveDialogue == null || !state.ActiveDialogue.Open)
        {
            return Failure(state, "dialogue.not_open", "No dialogue is open.", null);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var dialogueId = working.ActiveDialogue!.DialogueId;
        working.ActiveDialogue.Open = false;
        RuntimeStateHelpers.CopyState(working, state);
        return Success(state, $"Dialogue closed: {dialogueId}", GameRuntimeEventType.DialogueClosed, dialogueId);
    }

    private Dictionary<string, string> ChoiceArgs(GamePackageDefinition package, GameRuntimeState state, DialogueDefinition dialogue, DialogueNodeDefinition node)
    {
        var available = node.Choices
            .Where(choice => _requirementEvaluator.Evaluate(package, state, choice.Conditions.Select(RuntimeEffectMapper.ToRequirement).Concat(choice.Requirements)).Success)
            .Select(choice => choice.Id);
        return new Dictionary<string, string>
        {
            ["dialogueId"] = dialogue.Id,
            ["nodeId"] = node.Id,
            ["choiceIds"] = string.Join(",", available)
        };
    }

    private static GameRuntimeResult Success(GameRuntimeState state, string message, GameRuntimeEventType eventType, string targetId)
    {
        return new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = message,
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(eventType, message, targetId) }
        };
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string? targetId)
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

    private static GameRuntimeResult RolledBackFailure(GameRuntimeState state, GameRuntimeResult result, string message, string? targetId)
    {
        result.State = state;
        result.Success = false;
        result.Message = message;
        result.Events.Clear();
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, message, targetId));
        return result;
    }
}
