using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class QuestRuntimeService : IQuestRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IOutputApplier _outputApplier;

    public QuestRuntimeService(IRequirementEvaluator requirementEvaluator, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult StartQuest(GamePackageDefinition package, GameRuntimeState state, string questId)
    {
        var quest = FindQuest(package, questId);
        if (quest == null)
        {
            return Failure(state, "quest.missing", $"Quest not found: {questId}", questId);
        }

        var existing = state.Quests.FirstOrDefault(q => RuntimeStateHelpers.IdEquals(q.QuestId, quest.Id));
        if (existing != null && existing.State == "active")
        {
            return Success(state, $"Quest already active: {quest.Id}", GameRuntimeEventType.JournalUpdated, quest.Id);
        }

        if (existing != null && existing.State == "completed" && !quest.Repeatable)
        {
            return Failure(state, "quest.repeat_not_allowed", $"Quest is already completed and not repeatable: {quest.Id}", quest.Id);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var requirements = _requirementEvaluator.Evaluate(package, working, quest.StartConditions);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var output = _outputApplier.Apply(package, working, quest.StartEffects);
        result.Events.AddRange(output.Events);
        result.Diagnostics.AddRange(output.Diagnostics);
        if (!output.Success)
        {
            result.Success = false;
            result.Message = $"Quest start failed: {quest.Id}";
            return result;
        }

        working.Quests.RemoveAll(q => RuntimeStateHelpers.IdEquals(q.QuestId, quest.Id));
        var runtimeQuest = BuildQuestState(quest, working.Tick);
        working.Quests.Add(runtimeQuest);
        working.QuestStates[quest.Id] = runtimeQuest.State;
        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Success = true;
        result.Message = $"Quest started: {quest.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.QuestStarted, result.Message, quest.Id));
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.JournalUpdated, $"Journal updated: {quest.Id}", quest.Id));
        return result;
    }

    public GameRuntimeResult AdvanceQuestObjective(GamePackageDefinition package, GameRuntimeState state, string questId, string objectiveId, double amount = 1)
    {
        var quest = FindQuest(package, questId);
        if (quest == null)
        {
            return Failure(state, "quest.missing", $"Quest not found: {questId}", questId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtimeQuest = working.Quests.FirstOrDefault(q => RuntimeStateHelpers.IdEquals(q.QuestId, quest.Id) && q.State == "active");
        if (runtimeQuest == null)
        {
            return Failure(state, "quest.not_active", $"Quest is not active: {quest.Id}", quest.Id);
        }

        var objective = runtimeQuest.Objectives.FirstOrDefault(o => RuntimeStateHelpers.IdEquals(o.ObjectiveId, objectiveId));
        if (objective == null)
        {
            return Failure(state, "quest.objective.missing", $"Quest objective not found: {objectiveId}", objectiveId);
        }

        var result = new GameRuntimeResult { State = state, Success = true };
        objective.CurrentAmount = Math.Min(objective.RequiredAmount, objective.CurrentAmount + Math.Max(1, amount));
        var completedNow = !objective.Completed && objective.CurrentAmount >= objective.RequiredAmount;
        objective.Completed = objective.Completed || completedNow;
        if (completedNow)
        {
            var definition = AllObjectives(quest).FirstOrDefault(o => RuntimeStateHelpers.IdEquals(o.Id, objective.ObjectiveId));
            if (definition != null)
            {
                var completion = _outputApplier.Apply(package, working, definition.CompletionEffects);
                result.Events.AddRange(completion.Events);
                result.Diagnostics.AddRange(completion.Diagnostics);
            }
        }

        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.QuestObjectiveUpdated, $"Quest objective updated: {objective.ObjectiveId}", objective.ObjectiveId));
        CompleteStageOrQuestIfReady(package, working, quest, runtimeQuest, result);
        if (!result.Success)
        {
            return result;
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Message = $"Quest objective advanced: {objectiveId}";
        return result;
    }

    public GameRuntimeResult SetQuestStage(GamePackageDefinition package, GameRuntimeState state, string questId, string stageId)
    {
        var quest = FindQuest(package, questId);
        if (quest == null || !quest.Stages.Any(s => RuntimeStateHelpers.IdEquals(s.Id, stageId)))
        {
            return Failure(state, "quest.stage.missing", $"Quest stage not found: {questId}/{stageId}", questId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtimeQuest = working.Quests.FirstOrDefault(q => RuntimeStateHelpers.IdEquals(q.QuestId, quest.Id) && q.State == "active");
        if (runtimeQuest == null)
        {
            return Failure(state, "quest.not_active", $"Quest is not active: {quest.Id}", quest.Id);
        }

        runtimeQuest.CurrentStageId = stageId;
        ReplaceStageObjectives(runtimeQuest, quest.Stages.First(s => RuntimeStateHelpers.IdEquals(s.Id, stageId)));
        RuntimeStateHelpers.CopyState(working, state);
        return Success(state, $"Quest stage changed: {questId} -> {stageId}", GameRuntimeEventType.QuestStageChanged, questId);
    }

    public GameRuntimeResult CompleteQuest(GamePackageDefinition package, GameRuntimeState state, string questId)
    {
        var quest = FindQuest(package, questId);
        if (quest == null)
        {
            return Failure(state, "quest.missing", $"Quest not found: {questId}", questId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtimeQuest = working.Quests.FirstOrDefault(q => RuntimeStateHelpers.IdEquals(q.QuestId, quest.Id) && q.State == "active");
        if (runtimeQuest == null)
        {
            return Failure(state, "quest.not_active", $"Quest is not active: {quest.Id}", quest.Id);
        }

        var result = new GameRuntimeResult { State = state, Success = true };
        ApplyQuestCompletion(package, working, quest, runtimeQuest, result);
        if (!result.Success)
        {
            return result;
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Message = $"Quest completed: {quest.Id}";
        return result;
    }

    public GameRuntimeResult FailQuest(GamePackageDefinition package, GameRuntimeState state, string questId)
    {
        var quest = FindQuest(package, questId);
        if (quest == null)
        {
            return Failure(state, "quest.missing", $"Quest not found: {questId}", questId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var runtimeQuest = working.Quests.FirstOrDefault(q => RuntimeStateHelpers.IdEquals(q.QuestId, quest.Id) && q.State == "active");
        if (runtimeQuest == null)
        {
            return Failure(state, "quest.not_active", $"Quest is not active: {quest.Id}", quest.Id);
        }

        var result = new GameRuntimeResult { State = state, Success = true };
        var effects = _outputApplier.Apply(package, working, quest.FailureEffects);
        result.Events.AddRange(effects.Events);
        result.Diagnostics.AddRange(effects.Diagnostics);
        if (!effects.Success)
        {
            result.Success = false;
            result.Message = $"Quest fail effects failed: {quest.Id}";
            return result;
        }

        runtimeQuest.State = "failed";
        working.QuestStates[quest.Id] = "failed";
        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Message = $"Quest failed: {quest.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.QuestFailed, result.Message, quest.Id));
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.JournalUpdated, $"Journal updated: {quest.Id}", quest.Id));
        return result;
    }

    public GameRuntimeResult RefreshQuestObjectives(GamePackageDefinition package, GameRuntimeState state)
    {
        var result = new GameRuntimeResult { State = state, Success = true, Message = "Quest objectives refreshed." };
        foreach (var questState in state.Quests.Where(q => q.State == "active").ToList())
        {
            var quest = FindQuest(package, questState.QuestId);
            if (quest == null)
            {
                continue;
            }

            foreach (var objective in questState.Objectives.Where(o => !o.Completed).ToList())
            {
                var amount = ResolveCurrentAmount(state, objective);
                if (amount <= objective.CurrentAmount)
                {
                    continue;
                }

                var advance = AdvanceQuestObjective(package, state, questState.QuestId, objective.ObjectiveId, amount - objective.CurrentAmount);
                result.Events.AddRange(advance.Events);
                result.Diagnostics.AddRange(advance.Diagnostics);
                result.Success = result.Success && advance.Success;
                if (!advance.Success)
                {
                    return result;
                }
            }
        }

        return result;
    }

    private void CompleteStageOrQuestIfReady(GamePackageDefinition package, GameRuntimeState working, QuestDefinition quest, QuestRuntimeState runtimeQuest, GameRuntimeResult result)
    {
        var required = runtimeQuest.Objectives.Where(o => !IsOptional(quest, o.ObjectiveId)).ToList();
        if (required.Any(o => !o.Completed))
        {
            result.Success = true;
            return;
        }

        var stage = quest.Stages.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.Id, runtimeQuest.CurrentStageId));
        if (stage != null && !string.IsNullOrWhiteSpace(stage.NextStageId))
        {
            var next = quest.Stages.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.Id, stage.NextStageId));
            if (next != null)
            {
                var stageOutputs = _outputApplier.Apply(package, working, stage.CompleteEffects.Concat(stage.Rewards));
                result.Events.AddRange(stageOutputs.Events);
                result.Diagnostics.AddRange(stageOutputs.Diagnostics);
                if (!stageOutputs.Success)
                {
                    result.Success = false;
                    result.Message = $"Quest stage completion failed: {stage.Id}";
                    return;
                }

                runtimeQuest.CurrentStageId = next.Id;
                ReplaceStageObjectives(runtimeQuest, next);
                var enter = _outputApplier.Apply(package, working, next.EnterEffects);
                result.Events.AddRange(enter.Events);
                result.Diagnostics.AddRange(enter.Diagnostics);
                result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.QuestStageChanged, $"Quest stage changed: {quest.Id} -> {next.Id}", quest.Id));
                result.Success = enter.Success;
                return;
            }
        }

        ApplyQuestCompletion(package, working, quest, runtimeQuest, result);
    }

    private void ApplyQuestCompletion(GamePackageDefinition package, GameRuntimeState working, QuestDefinition quest, QuestRuntimeState runtimeQuest, GameRuntimeResult result)
    {
        var stage = quest.Stages.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.Id, runtimeQuest.CurrentStageId));
        var outputs = new List<OutputDefinition>();
        if (stage != null)
        {
            outputs.AddRange(stage.CompleteEffects);
            outputs.AddRange(stage.Rewards);
        }

        outputs.AddRange(quest.Rewards);
        var reward = _outputApplier.Apply(package, working, outputs);
        result.Events.AddRange(reward.Events);
        result.Diagnostics.AddRange(reward.Diagnostics);
        if (!reward.Success)
        {
            result.Success = false;
            result.Message = $"Quest rewards failed: {quest.Id}";
            return;
        }

        runtimeQuest.State = "completed";
        runtimeQuest.CompletedTick = working.Tick;
        working.QuestStates[quest.Id] = "completed";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.QuestCompleted, $"Quest completed: {quest.Id}", quest.Id));
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.QuestRewardGranted, $"Quest rewards granted: {quest.Id}", quest.Id));
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.JournalUpdated, $"Journal updated: {quest.Id}", quest.Id));
        result.Success = true;
    }

    private static QuestRuntimeState BuildQuestState(QuestDefinition quest, long tick)
    {
        var stage = quest.Stages.FirstOrDefault();
        var objectives = stage?.Objectives.Count > 0 ? stage.Objectives : quest.Objectives;
        return new QuestRuntimeState
        {
            QuestId = quest.Id,
            State = "active",
            CurrentStageId = stage?.Id,
            StartedTick = tick,
            Metadata = new Dictionary<string, string>(quest.Metadata),
            Objectives = objectives.Select(ToRuntimeObjective).ToList()
        };
    }

    private static QuestObjectiveRuntimeState ToRuntimeObjective(QuestObjectiveDefinition objective)
    {
        return new QuestObjectiveRuntimeState
        {
            ObjectiveId = objective.Id,
            Kind = objective.Kind,
            TargetId = objective.TargetId,
            CurrentAmount = objective.CurrentAmountDefault,
            RequiredAmount = objective.RequiredAmount <= 0 ? 1 : objective.RequiredAmount,
            Completed = objective.CurrentAmountDefault >= (objective.RequiredAmount <= 0 ? 1 : objective.RequiredAmount),
            Metadata = new Dictionary<string, string>(objective.Metadata)
        };
    }

    private static void ReplaceStageObjectives(QuestRuntimeState runtimeQuest, QuestStageDefinition stage)
    {
        if (stage.Objectives.Count > 0)
        {
            runtimeQuest.Objectives = stage.Objectives.Select(ToRuntimeObjective).ToList();
        }
    }

    private static IEnumerable<QuestObjectiveDefinition> AllObjectives(QuestDefinition quest)
    {
        return quest.Objectives.Concat(quest.Stages.SelectMany(stage => stage.Objectives));
    }

    private static bool IsOptional(QuestDefinition quest, string objectiveId)
    {
        return AllObjectives(quest).FirstOrDefault(o => RuntimeStateHelpers.IdEquals(o.Id, objectiveId))?.Optional == true;
    }

    private static double ResolveCurrentAmount(GameRuntimeState state, QuestObjectiveRuntimeState objective)
    {
        if (RuntimeStateHelpers.KindEquals(objective.Kind, "has_item") || RuntimeStateHelpers.KindEquals(objective.Kind, "collect_item"))
        {
            return RuntimeStateHelpers.GetItemAmount(RuntimeStateHelpers.FindInventory(state, null), objective.TargetId ?? string.Empty);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "gain_resource") || RuntimeStateHelpers.KindEquals(objective.Kind, "resource_at_least"))
        {
            return RuntimeStateHelpers.GetResourceAmount(state, objective.TargetId ?? string.Empty);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "reach_flag") || RuntimeStateHelpers.KindEquals(objective.Kind, "set_flag"))
        {
            return string.IsNullOrWhiteSpace(objective.TargetId) || string.IsNullOrWhiteSpace(RuntimeStateHelpers.GetFlagValue(state, objective.TargetId)) ? 0 : objective.RequiredAmount;
        }

        return objective.CurrentAmount;
    }

    private static QuestDefinition? FindQuest(GamePackageDefinition package, string questId)
    {
        return package.Game.Quests.FirstOrDefault(q => RuntimeStateHelpers.IdEquals(q.Id, questId));
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

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string targetId)
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
}
