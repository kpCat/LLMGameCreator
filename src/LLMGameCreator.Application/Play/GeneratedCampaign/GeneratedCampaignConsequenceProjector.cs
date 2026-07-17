using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignConsequenceProjector
{
    public GeneratedCampaignActionOutcome ProjectAction(
        GamePackageDefinition package,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        IReadOnlyList<RuntimeEvent> mapEvents,
        IReadOnlyList<GameRuntimeEvent> gameplayEvents,
        GeneratedCampaignAction action,
        IReadOnlyList<GeneratedCampaignQuestReadiness> readinessBefore,
        IReadOnlyList<GeneratedCampaignQuestReadiness> readinessAfter,
        bool success,
        IReadOnlyList<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        ArgumentNullException.ThrowIfNull(action);
        var rows = new List<GeneratedCampaignConsequence>();
        ProjectParticipantResources(package, before, after, rows);
        ProjectParticipantStatuses(package, before, after, rows);
        ProjectInventory(package, before.GameplayState, after.GameplayState, rows);
        ProjectQuests(package, before.GameplayState, after.GameplayState,
            readinessBefore, readinessAfter, rows);
        ProjectReputation(package, before.GameplayState, after.GameplayState, rows);
        ProjectMap(package, before, after, mapEvents, rows);
        ProjectEncounter(package, before, after, gameplayEvents, action, rows);
        ProjectEventConsequences(gameplayEvents, rows);
        if (!success)
        {
            rows.Add(new GeneratedCampaignConsequence
            {
                Kind = GeneratedCampaignConsequenceKind.Failure,
                Title = "Действие не выполнено",
                Description = "Состояние кампании не подтверждает успешное выполнение действия.",
                Tone = GeneratedCampaignConsequenceTone.Negative
            });
        }
        rows = Deduplicate(rows);
        return new GeneratedCampaignActionOutcome
        {
            ActionTitle = action.Title,
            Success = success,
            Summary = Summary(success, rows),
            Consequences = rows,
            BeforeSessionSha256 = HashSession(before),
            AfterSessionSha256 = HashSession(after),
            RuntimeEventCount = mapEvents.Count + gameplayEvents.Count,
            Diagnostics = diagnostics
        };
    }

    public GeneratedCampaignActionOutcome ProjectFailure(
        string actionTitle,
        UnifiedRuntimeSession session,
        IReadOnlyList<string> diagnostics) =>
        new()
        {
            ActionTitle = actionTitle,
            Summary = "Действие не выполнено.",
            Consequences =
            [
                new GeneratedCampaignConsequence
                {
                    Kind = GeneratedCampaignConsequenceKind.Failure,
                    Title = "Действие не выполнено",
                    Description = "Условия действия больше не выполнены.",
                    Tone = GeneratedCampaignConsequenceTone.Negative
                }
            ],
            BeforeSessionSha256 = HashSession(session),
            AfterSessionSha256 = HashSession(session),
            Diagnostics = diagnostics
        };

    public GeneratedCampaignActionOutcome ProjectSave(
        UnifiedRuntimeSession session,
        GeneratedGameplaySaveResult result) =>
        new()
        {
            ActionTitle = "Сохранить игру",
            Success = result.Passed,
            Summary = result.Passed ? "Игра сохранена." : "Сохранение не создано.",
            Consequences =
            [
                new GeneratedCampaignConsequence
                {
                    Kind = result.Passed
                        ? GeneratedCampaignConsequenceKind.Save
                        : GeneratedCampaignConsequenceKind.Failure,
                    Title = result.Passed ? "Игра сохранена" : "Сохранение не создано",
                    AfterValue = result.Passed
                        ? result.Deduplicated ? "Без новой ревизии" : "Новая ревизия"
                        : "Без изменений",
                    Description = result.Passed
                        ? result.Deduplicated
                            ? "Текущее состояние уже было сохранено."
                            : "Текущее состояние записано в сохранение."
                        : "Проверка сохранения не пройдена.",
                    Tone = result.Passed
                        ? GeneratedCampaignConsequenceTone.Positive
                        : GeneratedCampaignConsequenceTone.Negative
                }
            ],
            BeforeSessionSha256 = HashSession(session),
            AfterSessionSha256 = HashSession(session),
            Diagnostics = result.Diagnostics
        };

    public GeneratedCampaignActionOutcome ProjectLoad(
        UnifiedRuntimeSession session,
        GeneratedGameplaySaveResult result) =>
        new()
        {
            ActionTitle = "Продолжить игру",
            Success = result.Passed,
            Summary = result.Passed ? "Сохранённая игра продолжена." : "Сохранение не загружено.",
            Consequences =
            [
                new GeneratedCampaignConsequence
                {
                    Kind = result.Passed
                        ? GeneratedCampaignConsequenceKind.Load
                        : GeneratedCampaignConsequenceKind.Failure,
                    Title = result.Passed ? "Игра продолжена" : "Загрузка не выполнена",
                    AfterValue = result.Passed ? "Текущее сохранение" : "Без изменений",
                    Description = result.Passed
                        ? "Состояние восстановлено из точного сохранения текущего мира."
                        : "Проверка сохранения не пройдена.",
                    Tone = result.Passed
                        ? GeneratedCampaignConsequenceTone.Positive
                        : GeneratedCampaignConsequenceTone.Negative
                }
            ],
            BeforeSessionSha256 = HashSession(session),
            AfterSessionSha256 = HashSession(session),
            Diagnostics = result.Diagnostics
        };

    public GeneratedCampaignActionOutcome ProjectMigration(
        UnifiedRuntimeSession session,
        GeneratedGameplaySaveMigrationResult result)
    {
        var preview = result.Preview;
        var preserved = preview?.PreservedCountsByKind.Values.Sum() ?? 0;
        var dropped = preview?.DroppedCountsByKind.Values.Sum() ?? 0;
        var map = preview?.MapReset == true ? "Позиция сброшена на старт" : "Позиция сохранена";
        return new GeneratedCampaignActionOutcome
        {
            ActionTitle = "Перенести сохранение",
            Success = result.Passed,
            Summary = result.Passed ? "Сохранение перенесено в текущий мир." : "Перенос не выполнен.",
            Consequences =
            [
                new GeneratedCampaignConsequence
                {
                    Kind = result.Passed
                        ? GeneratedCampaignConsequenceKind.Migration
                        : GeneratedCampaignConsequenceKind.Failure,
                    Title = result.Passed ? "Сохранение перенесено" : "Перенос не выполнен",
                    BeforeValue = "Предыдущий мир",
                    AfterValue = result.Passed ? "Текущий мир" : "Без изменений",
                    Delta = result.Passed ? $"Сохранено: {preserved}; сброшено: {dropped}" : string.Empty,
                    Description = result.Passed ? map + "." : "Проверка переноса не пройдена.",
                    Tone = result.Passed
                        ? GeneratedCampaignConsequenceTone.Neutral
                        : GeneratedCampaignConsequenceTone.Negative
                }
            ],
            BeforeSessionSha256 = HashSession(session),
            AfterSessionSha256 = HashSession(session),
            Diagnostics = result.Diagnostics
        };
    }

    public GeneratedCampaignActionOutcome ProjectRecovery(
        GeneratedCampaignConsequenceKind kind,
        string actionTitle,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        bool success,
        string successTitle,
        string successDescription,
        IReadOnlyList<string> diagnostics)
    {
        var consequence = new GeneratedCampaignConsequence
        {
            Kind = success ? kind : GeneratedCampaignConsequenceKind.Failure,
            Title = success ? successTitle : "Действие восстановления не выполнено",
            Description = success ? successDescription : "Состояние кампании не было изменено.",
            Tone = success ? GeneratedCampaignConsequenceTone.Neutral : GeneratedCampaignConsequenceTone.Negative
        };
        return new GeneratedCampaignActionOutcome
        {
            ActionTitle = actionTitle,
            Success = success,
            Summary = success ? successDescription : "Действие восстановления не выполнено.",
            Consequences = [consequence],
            BeforeSessionSha256 = HashSession(before),
            AfterSessionSha256 = HashSession(after),
            Diagnostics = diagnostics
        };
    }

    public IReadOnlyList<GeneratedCampaignConsequence> RebuildFromPersistedEvents(
        GamePackageDefinition package,
        UnifiedRuntimeSession session)
    {
        var rows = new List<GeneratedCampaignConsequence>();
        foreach (var runtimeEvent in session.MapEvents.Where(item => item.Type == RuntimeEventType.MapChanged))
            rows.Add(Simple(GeneratedCampaignConsequenceKind.MapTravel, "Переход между регионами",
                "Переход подтверждён сохранённым событием карты.", GeneratedCampaignConsequenceTone.Neutral));
        foreach (var runtimeEvent in session.GameplayEvents)
        {
            var row = runtimeEvent.Type switch
            {
                GameRuntimeEventType.DamageApplied => Simple(GeneratedCampaignConsequenceKind.Damage,
                    "Получен урон", "Урон подтверждён сохранённым событием боя.",
                    GeneratedCampaignConsequenceTone.Negative),
                GameRuntimeEventType.EncounterWon => Simple(GeneratedCampaignConsequenceKind.EncounterWon,
                    "Победа во встрече", "Победа подтверждена сохранённым событием.",
                    GeneratedCampaignConsequenceTone.Positive),
                GameRuntimeEventType.EncounterLost => Simple(GeneratedCampaignConsequenceKind.EncounterLost,
                    "Поражение во встрече", "Поражение подтверждено сохранённым событием.",
                    GeneratedCampaignConsequenceTone.Negative),
                GameRuntimeEventType.RewardGranted or GameRuntimeEventType.QuestRewardGranted => Simple(
                    GeneratedCampaignConsequenceKind.Reward, "Получена награда",
                    "Награда подтверждена сохранённым событием.", GeneratedCampaignConsequenceTone.Positive),
                GameRuntimeEventType.QuestCompleted => Simple(GeneratedCampaignConsequenceKind.QuestCompleted,
                    "Задание завершено", "Завершение подтверждено сохранённым событием.",
                    GeneratedCampaignConsequenceTone.Positive),
                GameRuntimeEventType.FactionReputationChanged => Simple(
                    GeneratedCampaignConsequenceKind.Reputation, "Репутация изменилась",
                    "Изменение подтверждено сохранённым событием.", GeneratedCampaignConsequenceTone.Neutral),
                _ => null
            };
            if (row is not null) rows.Add(row);
        }
        return Deduplicate(rows).TakeLast(GeneratedCampaignConsequenceTimeline.DefaultMaximumEntries - 1)
            .ToList();
    }

    public static string HashSession(UnifiedRuntimeSession session) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(session)))).ToLowerInvariant();

    private static void ProjectParticipantResources(
        GamePackageDefinition package,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        List<GeneratedCampaignConsequence> rows)
    {
        var left = before.GameplayState.ActiveEncounter;
        var right = after.GameplayState.ActiveEncounter;
        if (left is null || right is null || !IdEquals(left.EncounterId, right.EncounterId)) return;
        foreach (var participant in right.Participants)
        {
            var oldParticipant = left.Participants.SingleOrDefault(item => IdEquals(item.Id, participant.Id));
            if (oldParticipant is null) continue;
            foreach (var resource in participant.Resources)
            {
                var oldResource = oldParticipant.Resources.SingleOrDefault(item =>
                    IdEquals(item.ResourceId, resource.ResourceId));
                if (oldResource is null || oldResource.Amount == resource.Amount) continue;
                var delta = resource.Amount - oldResource.Amount;
                var damage = delta < 0;
                var participantTitle = Safe(participant.Name,
                    KindEquals(participant.Team, "player") ? "Игрок" : "Противник");
                var resourceTitle = ResourceTitle(package, resource.ResourceId);
                rows.Add(new GeneratedCampaignConsequence
                {
                    Kind = damage ? GeneratedCampaignConsequenceKind.Damage
                        : GeneratedCampaignConsequenceKind.Healing,
                    Title = damage ? participantTitle + " получает урон"
                        : participantTitle + " восстанавливает " + resourceTitle.ToLowerInvariant(),
                    BeforeValue = Number(oldResource.Amount),
                    AfterValue = Number(resource.Amount),
                    Delta = Signed(delta),
                    Description = resourceTitle + ": " + Number(oldResource.Amount) + " → " + Number(resource.Amount) + ".",
                    Tone = damage
                        ? KindEquals(participant.Team, "player")
                            ? GeneratedCampaignConsequenceTone.Negative
                            : GeneratedCampaignConsequenceTone.Positive
                        : GeneratedCampaignConsequenceTone.Positive
                });
            }
        }
    }

    private static void ProjectParticipantStatuses(
        GamePackageDefinition package,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        List<GeneratedCampaignConsequence> rows)
    {
        var left = before.GameplayState.ActiveEncounter;
        var right = after.GameplayState.ActiveEncounter;
        if (left is null || right is null || !IdEquals(left.EncounterId, right.EncounterId)) return;
        foreach (var participant in right.Participants)
        {
            var oldParticipant = left.Participants.SingleOrDefault(item => IdEquals(item.Id, participant.Id));
            if (oldParticipant is null) continue;
            foreach (var status in participant.Statuses.Where(status => oldParticipant.Statuses.All(old =>
                         !IdEquals(old.StatusId, status.StatusId))))
            {
                rows.Add(Simple(GeneratedCampaignConsequenceKind.Status,
                    "Новый эффект: " + StatusTitle(package, status.StatusId),
                    "Эффект применён к участнику встречи.", GeneratedCampaignConsequenceTone.Neutral));
            }
            foreach (var status in oldParticipant.Statuses.Where(status => participant.Statuses.All(current =>
                         !IdEquals(current.StatusId, status.StatusId))))
            {
                rows.Add(Simple(GeneratedCampaignConsequenceKind.Status,
                    "Эффект завершён: " + StatusTitle(package, status.StatusId),
                    "Эффект больше не действует.", GeneratedCampaignConsequenceTone.Neutral));
            }
        }
    }

    private static void ProjectInventory(
        GamePackageDefinition package,
        GameRuntimeState before,
        GameRuntimeState after,
        List<GeneratedCampaignConsequence> rows)
    {
        var left = PlayerInventory(before);
        var right = PlayerInventory(after);
        foreach (var id in left.Keys.Concat(right.Keys).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var previous = left.GetValueOrDefault(id);
            var current = right.GetValueOrDefault(id);
            if (previous == current) continue;
            var delta = current - previous;
            rows.Add(new GeneratedCampaignConsequence
            {
                Kind = GeneratedCampaignConsequenceKind.Inventory,
                Title = delta > 0 ? "Получен предмет: " + ItemTitle(package, id)
                    : "Израсходован предмет: " + ItemTitle(package, id),
                BeforeValue = Number(previous),
                AfterValue = Number(current),
                Delta = Signed(delta),
                Description = "Количество предмета изменилось: " + Number(previous) + " → " + Number(current) + ".",
                Tone = delta > 0 ? GeneratedCampaignConsequenceTone.Positive
                    : GeneratedCampaignConsequenceTone.Negative
            });
        }
    }

    private static void ProjectQuests(
        GamePackageDefinition package,
        GameRuntimeState before,
        GameRuntimeState after,
        IReadOnlyList<GeneratedCampaignQuestReadiness> readinessBefore,
        IReadOnlyList<GeneratedCampaignQuestReadiness> readinessAfter,
        List<GeneratedCampaignConsequence> rows)
    {
        foreach (var current in readinessAfter.Where(item => item.Generated && item.Ready))
        {
            var previous = readinessBefore.SingleOrDefault(item => IdEquals(item.QuestId, current.QuestId));
            if (previous?.Ready == true) continue;
            rows.Add(Simple(GeneratedCampaignConsequenceKind.QuestReady,
                "Задание готово к завершению: " + QuestTitle(package, current.QuestId),
                "Все обязательные цели выполнены. Награда будет выдана после завершения задания.",
                GeneratedCampaignConsequenceTone.Positive));
        }
        foreach (var quest in after.Quests.Where(item => KindEquals(item.State, "completed")))
        {
            var previous = before.Quests.SingleOrDefault(item => IdEquals(item.QuestId, quest.QuestId));
            if (previous is null || KindEquals(previous.State, "completed")) continue;
            rows.Add(Simple(GeneratedCampaignConsequenceKind.QuestCompleted,
                "Задание завершено: " + QuestTitle(package, quest.QuestId),
                "Награды задания применены игровым Runtime.", GeneratedCampaignConsequenceTone.Positive));
        }
    }

    private static void ProjectReputation(
        GamePackageDefinition package,
        GameRuntimeState before,
        GameRuntimeState after,
        List<GeneratedCampaignConsequence> rows)
    {
        foreach (var faction in after.Factions)
        {
            var previous = before.Factions.SingleOrDefault(item => IdEquals(item.FactionId, faction.FactionId));
            var oldValue = previous?.Reputation ?? 0;
            if (oldValue == faction.Reputation) continue;
            var delta = faction.Reputation - oldValue;
            rows.Add(new GeneratedCampaignConsequence
            {
                Kind = GeneratedCampaignConsequenceKind.Reputation,
                Title = "Репутация: " + FactionTitle(package, faction.FactionId),
                BeforeValue = Number(oldValue),
                AfterValue = Number(faction.Reputation),
                Delta = Signed(delta),
                Description = "Отношение фракции изменилось: " + Number(oldValue) + " → " + Number(faction.Reputation) + ".",
                Tone = delta > 0 ? GeneratedCampaignConsequenceTone.Positive
                    : GeneratedCampaignConsequenceTone.Negative
            });
        }
    }

    private static void ProjectMap(
        GamePackageDefinition package,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        IReadOnlyList<RuntimeEvent> events,
        List<GeneratedCampaignConsequence> rows)
    {
        if (IdEquals(before.MapState.CurrentMapId, after.MapState.CurrentMapId)) return;
        if (!events.Any(item => item.Type == RuntimeEventType.MapChanged)) return;
        rows.Add(new GeneratedCampaignConsequence
        {
            Kind = GeneratedCampaignConsequenceKind.MapTravel,
            Title = "Переход в новый регион",
            BeforeValue = MapTitle(package, before.MapState.CurrentMapId),
            AfterValue = MapTitle(package, after.MapState.CurrentMapId),
            Description = "Положение игрока изменено подтверждённым переходом карты.",
            Tone = GeneratedCampaignConsequenceTone.Neutral
        });
    }

    private static void ProjectEncounter(
        GamePackageDefinition package,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        IReadOnlyList<GameRuntimeEvent> events,
        GeneratedCampaignAction action,
        List<GeneratedCampaignConsequence> rows)
    {
        var current = after.GameplayState.ActiveEncounter;
        if (events.Any(item => item.Type == GameRuntimeEventType.EncounterStarted)
            && current is not null)
            rows.Add(Simple(GeneratedCampaignConsequenceKind.EncounterStarted,
                "Встреча началась: " + EncounterTitle(package, current.EncounterId),
                "Участники вступили в пошаговую встречу.", GeneratedCampaignConsequenceTone.Neutral));
        if (events.Any(item => item.Type == GameRuntimeEventType.EncounterWon))
            rows.Add(Simple(GeneratedCampaignConsequenceKind.EncounterWon, "Победа во встрече",
                "Все противники побеждены.", GeneratedCampaignConsequenceTone.Positive));
        if (events.Any(item => item.Type == GameRuntimeEventType.EncounterLost))
            rows.Add(Simple(GeneratedCampaignConsequenceKind.EncounterLost, "Поражение во встрече",
                "Все участники команды игрока побеждены.", GeneratedCampaignConsequenceTone.Negative));
        var wasActive = before.GameplayState.ActiveEncounter is { Active: true };
        if (action.Kind == GeneratedCampaignActionKind.FleeEncounter && wasActive
            && current is { Active: false }
            && events.Any(item => item.Type == GameRuntimeEventType.EncounterEnded)
            && !events.Any(item => item.Type is GameRuntimeEventType.EncounterWon
                or GameRuntimeEventType.EncounterLost))
            rows.Add(Simple(GeneratedCampaignConsequenceKind.EncounterFled, "Встреча покинута",
                "Игрок вышел из встречи без победы и награды.", GeneratedCampaignConsequenceTone.Neutral));
    }

    private static void ProjectEventConsequences(
        IReadOnlyList<GameRuntimeEvent> events,
        List<GeneratedCampaignConsequence> rows)
    {
        if (events.Any(item => item.Type is GameRuntimeEventType.RewardGranted
            or GameRuntimeEventType.QuestRewardGranted))
            rows.Add(Simple(GeneratedCampaignConsequenceKind.Reward, "Получена награда",
                "Награда подтверждена событием игрового Runtime.", GeneratedCampaignConsequenceTone.Positive));
        if (events.Any(item => item.Type is GameRuntimeEventType.DialogueOpened
            or GameRuntimeEventType.DialogueChoiceSelected or GameRuntimeEventType.DialogueClosed))
            rows.Add(Simple(GeneratedCampaignConsequenceKind.Dialogue, "Разговор продолжен",
                "Выбор в разговоре применён игровым Runtime.", GeneratedCampaignConsequenceTone.Neutral));
    }

    private static Dictionary<string, double> PlayerInventory(GameRuntimeState state) =>
        state.Inventories.Where(inventory => KindEquals(inventory.OwnerKind, "player")
                                            && (string.IsNullOrWhiteSpace(inventory.OwnerId)
                                                || IdEquals(inventory.OwnerId, state.PlayerEntityId)))
            .SelectMany(inventory => inventory.Stacks)
            .GroupBy(stack => stack.ItemId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Sum(stack => stack.Amount),
                StringComparer.OrdinalIgnoreCase);

    private static GeneratedCampaignConsequence Simple(
        GeneratedCampaignConsequenceKind kind,
        string title,
        string description,
        GeneratedCampaignConsequenceTone tone) => new()
    {
        Kind = kind,
        Title = title,
        Description = description,
        Tone = tone
    };

    private static List<GeneratedCampaignConsequence> Deduplicate(
        IEnumerable<GeneratedCampaignConsequence> rows) => rows
        .GroupBy(item => string.Join("\n", item.Kind, item.Title, item.BeforeValue, item.AfterValue,
            item.Delta, item.Description), StringComparer.Ordinal)
        .Select(group => group.First())
        .ToList();

    private static string Summary(bool success, IReadOnlyList<GeneratedCampaignConsequence> rows)
    {
        if (!success) return "Действие не выполнено.";
        if (rows.Count == 0) return "Действие выполнено без видимых изменений состояния.";
        return string.Join("; ", rows.Take(3).Select(item => item.Title)) + ".";
    }

    private static string ResourceTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Resources.SingleOrDefault(item => IdEquals(item.Id, id))?.Name,
        GeneratedCampaignActionPlanner.HumanLabel(id, "Ресурс"));

    private static string ItemTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Items.SingleOrDefault(item => IdEquals(item.Id, id))?.Name,
        GeneratedCampaignActionPlanner.HumanLabel(id, "Предмет"));

    private static string StatusTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Statuses.SingleOrDefault(item => IdEquals(item.Id, id))?.Name,
        GeneratedCampaignActionPlanner.HumanLabel(id, "Эффект"));

    private static string FactionTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Factions.SingleOrDefault(item => IdEquals(item.Id, id))?.Name,
        GeneratedCampaignActionPlanner.HumanLabel(id, "Фракция"));

    private static string QuestTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Quests.SingleOrDefault(item => IdEquals(item.Id, id))?.Title, "Задание");

    private static string EncounterTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Encounters.SingleOrDefault(item => IdEquals(item.Id, id))?.Name, "Встреча");

    private static string MapTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Maps.SingleOrDefault(item => IdEquals(item.Id, id))?.Name, "Регион");

    private static string Safe(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string Number(double value) => value.ToString("0.##",
        System.Globalization.CultureInfo.InvariantCulture);

    private static string Signed(double value) => (value > 0 ? "+" : string.Empty) + Number(value);

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool KindEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
