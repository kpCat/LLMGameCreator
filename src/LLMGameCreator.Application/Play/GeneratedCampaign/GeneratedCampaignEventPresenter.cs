using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignEventPresenter
{
    public IReadOnlyList<string> Present(UnifiedRuntimeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var messages = result.MapEvents.Select(MapMessage)
            .Concat(result.GameplayEvents.Select(GameplayMessage))
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .TakeLast(16)
            .ToList();
        if (messages.Count == 0 && !result.Success) messages.Add("Действие не выполнено.");
        return messages;
    }

    private static string MapMessage(RuntimeEvent runtimeEvent) => runtimeEvent.Type switch
    {
        RuntimeEventType.PlayerMoved => "Игрок переместился.",
        RuntimeEventType.MovementBlocked => "Путь перекрыт.",
        RuntimeEventType.InteractionTriggered => "Взаимодействие выполнено.",
        RuntimeEventType.DialogueRequested => "Доступен разговор.",
        RuntimeEventType.MapChanged => "Выполнен переход в другой регион.",
        RuntimeEventType.Error => "Действие на карте завершилось ошибкой.",
        _ => string.Empty
    };

    private static string GameplayMessage(GameRuntimeEvent runtimeEvent) => runtimeEvent.Type switch
    {
        GameRuntimeEventType.InventoryChanged => "Инвентарь обновлён.",
        GameRuntimeEventType.ResourceChanged => "Ресурсы изменились.",
        GameRuntimeEventType.InteractionTriggered => "Событие взаимодействия выполнено.",
        GameRuntimeEventType.EncounterStarted => "Встреча началась.",
        GameRuntimeEventType.TurnStarted => "Начался следующий ход.",
        GameRuntimeEventType.AbilityUsed => "Способность применена.",
        GameRuntimeEventType.DamageApplied => "Цель получила урон.",
        GameRuntimeEventType.HealingApplied => "Здоровье восстановлено.",
        GameRuntimeEventType.ParticipantDefeated => "Участник встречи побеждён.",
        GameRuntimeEventType.EncounterWon => "Встреча завершена победой.",
        GameRuntimeEventType.EncounterLost => "Встреча завершена поражением.",
        GameRuntimeEventType.EncounterEnded => "Встреча завершена.",
        GameRuntimeEventType.RewardGranted => "Получена награда за встречу.",
        GameRuntimeEventType.AiActionChosen => "Противник выполнил действие.",
        GameRuntimeEventType.QuestStarted => "Новое задание начато.",
        GameRuntimeEventType.QuestObjectiveUpdated => "Цель задания обновлена.",
        GameRuntimeEventType.QuestCompleted => "Задание завершено.",
        GameRuntimeEventType.QuestRewardGranted => "Получена награда за задание.",
        GameRuntimeEventType.DialogueOpened => "Разговор начат.",
        GameRuntimeEventType.DialogueNodeChanged => "Разговор продолжен.",
        GameRuntimeEventType.DialogueChoiceSelected => "Ответ выбран.",
        GameRuntimeEventType.DialogueClosed => "Разговор завершён.",
        GameRuntimeEventType.FactionReputationChanged => "Репутация фракции изменилась.",
        GameRuntimeEventType.ProgressionChanged => "Прогресс персонажа изменился.",
        _ => string.Empty
    };
}
