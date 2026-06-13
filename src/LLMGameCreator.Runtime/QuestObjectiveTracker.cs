using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class QuestObjectiveTracker : IQuestObjectiveTracker
{
    private readonly IQuestRuntimeService _questRuntimeService;

    public QuestObjectiveTracker(IQuestRuntimeService questRuntimeService)
    {
        _questRuntimeService = questRuntimeService;
    }

    public GameRuntimeResult Track(GamePackageDefinition package, GameRuntimeState state, IEnumerable<GameRuntimeEvent> events)
    {
        var result = new GameRuntimeResult { State = state, Success = true, Message = "Quest objective tracking complete." };
        foreach (var runtimeEvent in events.ToList())
        {
            foreach (var quest in state.Quests.Where(q => q.State == "active").ToList())
            {
                foreach (var objective in quest.Objectives.Where(o => !o.Completed && Matches(runtimeEvent, o)).ToList())
                {
                    var advance = _questRuntimeService.AdvanceQuestObjective(package, state, quest.QuestId, objective.ObjectiveId, ResolveAmount(runtimeEvent));
                    result.Events.AddRange(advance.Events);
                    result.Diagnostics.AddRange(advance.Diagnostics);
                    result.Success = result.Success && advance.Success;
                    if (!advance.Success)
                    {
                        return result;
                    }
                }
            }
        }

        return result;
    }

    private static bool Matches(GameRuntimeEvent runtimeEvent, QuestObjectiveRuntimeState objective)
    {
        var target = objective.TargetId ?? string.Empty;
        if (RuntimeStateHelpers.KindEquals(objective.Kind, "collect_item"))
        {
            return (runtimeEvent.Type == GameRuntimeEventType.InventoryChanged || runtimeEvent.Type == GameRuntimeEventType.OutputApplied)
                && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target || runtimeEvent.Args.Values.Contains(target) || runtimeEvent.Message.Contains(target, StringComparison.Ordinal));
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "gain_resource"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.ResourceChanged && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "complete_encounter"))
        {
            return (runtimeEvent.Type == GameRuntimeEventType.EncounterWon || runtimeEvent.Type == GameRuntimeEventType.EncounterEnded)
                && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "choose_dialogue"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.DialogueChoiceSelected && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "talk_to"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.DialogueOpened && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "harvest_resource"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.ResourceHarvested && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "craft_recipe"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.RecipeCrafted && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "execute_transaction"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.TransactionExecuted && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "open_container"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.ContainerOpened && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "equip_item"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.EquipmentChanged && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "use_item"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.OutputApplied && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        if (RuntimeStateHelpers.KindEquals(objective.Kind, "reach_flag") || RuntimeStateHelpers.KindEquals(objective.Kind, "set_flag"))
        {
            return runtimeEvent.Type == GameRuntimeEventType.OutputApplied && (string.IsNullOrWhiteSpace(target) || runtimeEvent.TargetId == target);
        }

        return false;
    }

    private static double ResolveAmount(GameRuntimeEvent runtimeEvent)
    {
        return runtimeEvent.Args.TryGetValue("amount", out var value) && double.TryParse(value, out var amount) && amount > 0
            ? amount
            : 1;
    }
}
