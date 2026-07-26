using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed record GeneratedCampaignQuestArcProjection
{
    public string CurrentQuest { get; init; } = string.Empty;
    public int CompletedQuestCount { get; init; }
    public int TotalQuestCount { get; init; }
    public IReadOnlyList<string> QuestTitles { get; init; } = [];
}

public sealed record GeneratedCampaignRelationshipRow
{
    public string Actor { get; init; } = string.Empty;
    public string Faction { get; init; } = string.Empty;
    public string Branch { get; init; } = string.Empty;
    public GeneratedCampaignRelationshipStatus Status { get; init; }
    public string StatusTitle { get; init; } = string.Empty;
    public string Reputation { get; init; } = string.Empty;
    public string CurrentQuest { get; init; } = string.Empty;
    public int CompletedQuestCount { get; init; }
    public int TotalQuestCount { get; init; }
    public string NextAction { get; init; } = string.Empty;
    public string Consequences { get; init; } = string.Empty;
    public GeneratedCampaignQuestArcProjection Arc { get; init; } = new();
}

public sealed record GeneratedCampaignRelationshipProjection
{
    public IReadOnlyList<GeneratedCampaignRelationshipRow> Rows { get; init; } = [];
    public int RelationshipCount { get; init; }
    public int CompletedRelationshipCount { get; init; }
}

public sealed class GeneratedCampaignRelationshipProjectionService
{
    public GeneratedCampaignRelationshipProjection Project(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GeneratedCampaignRelationshipOverlayDocument? overlay,
        IReadOnlyList<GeneratedCampaignQuestReadiness>? readiness = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        if (overlay is null || overlay.RelationshipCount == 0)
            return new GeneratedCampaignRelationshipProjection();

        readiness ??= [];
        var rows = overlay.Bindings.OrderBy(item => item.RelationshipId,
                StringComparer.Ordinal)
            .Select(item => Row(package, session, item, readiness))
            .ToList();
        return new GeneratedCampaignRelationshipProjection
        {
            Rows = rows,
            RelationshipCount = rows.Count,
            CompletedRelationshipCount = rows.Count(item =>
                item.Status is GeneratedCampaignRelationshipStatus.COMPLETED
                    or GeneratedCampaignRelationshipStatus.CHALLENGE_RESOLVED
                    or GeneratedCampaignRelationshipStatus.REFUSED)
        };
    }

    private static GeneratedCampaignRelationshipRow Row(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GeneratedCampaignRelationshipBinding relationship,
        IReadOnlyList<GeneratedCampaignQuestReadiness> readiness)
    {
        var dialogue = package.Game.Dialogues.Single(item =>
            item.Id == relationship.DialogueId);
        var faction = package.Game.Factions.Single(item =>
            item.Id == relationship.FactionId);
        var flag = session.GameplayState.Flags.SingleOrDefault(item =>
            item.Id == relationship.DecisionFlagId)?.Value ?? string.Empty;
        var questStates = relationship.QuestArc.Select(step => new
        {
            Step = step,
            State = session.GameplayState.Quests.SingleOrDefault(item =>
                item.QuestId == step.QuestId)?.State ?? "not_started",
            Ready = readiness.SingleOrDefault(item =>
                item.QuestId == step.QuestId)?.Ready ?? false
        }).ToList();
        var completed = questStates.Count(item =>
            item.State == "completed");
        var current = questStates.FirstOrDefault(item =>
            item.State == "active");
        var currentTitle = current is null
            ? string.Empty
            : package.Game.Quests.Single(item =>
                item.Id == current.Step.QuestId).Title;
        var status = ResolveStatus(session, relationship, flag,
            questStates.Select(item => (item.State, item.Ready)).ToList());
        var branch = flag switch
        {
            "SUPPORT" => "Поддержка",
            "CHALLENGE" => "Вызов",
            "REFUSE" => "Отказ",
            _ => "Не выбрано"
        };
        var nextAction = NextAction(package, relationship, status,
            questStates.Select(item => item.State).ToList());
        var reputation = session.GameplayState.Factions.SingleOrDefault(item =>
            item.FactionId == relationship.FactionId)?.Reputation
                         ?? faction.DefaultReputation.GetValueOrDefault();
        return new GeneratedCampaignRelationshipRow
        {
            Actor = dialogue.Title,
            Faction = faction.Name,
            Branch = branch,
            Status = status,
            StatusTitle = StatusTitle(status),
            Reputation = reputation.ToString("0.##",
                System.Globalization.CultureInfo.InvariantCulture),
            CurrentQuest = currentTitle,
            CompletedQuestCount = completed,
            TotalQuestCount = relationship.QuestArc.Count,
            NextAction = nextAction,
            Consequences = Consequences(status, completed,
                relationship.QuestArc.Count, reputation),
            Arc = new GeneratedCampaignQuestArcProjection
            {
                CurrentQuest = currentTitle,
                CompletedQuestCount = completed,
                TotalQuestCount = relationship.QuestArc.Count,
                QuestTitles = relationship.QuestArc.Select(step =>
                        package.Game.Quests.Single(item =>
                            item.Id == step.QuestId).Title)
                    .ToList()
            }
        };
    }

    private static GeneratedCampaignRelationshipStatus ResolveStatus(
        UnifiedRuntimeSession session,
        GeneratedCampaignRelationshipBinding relationship,
        string flag,
        IReadOnlyList<(string State, bool Ready)> quests)
    {
        if (flag == "SUPPORT")
        {
            if (quests.Count > 0 && quests.All(item =>
                    item.State == "completed"))
                return GeneratedCampaignRelationshipStatus.COMPLETED;
            var active = quests.FirstOrDefault(item =>
                item.State == "active");
            if (active.State == "active")
                return active.Ready
                    ? GeneratedCampaignRelationshipStatus.QUEST_READY
                    : GeneratedCampaignRelationshipStatus.QUEST_ACTIVE;
            return GeneratedCampaignRelationshipStatus.SUPPORTED;
        }
        if (flag == "CHALLENGE")
        {
            var encounter = session.GameplayState.ActiveEncounter;
            var resolved = encounter is not null
                           && encounter.EncounterId ==
                           relationship.ChallengeEncounterId
                           && !encounter.Active
                           && encounter.Participants.Any(item =>
                               item.Alive && string.Equals(item.Team,
                                   "player",
                                   StringComparison.OrdinalIgnoreCase))
                           && encounter.Participants.Where(item =>
                                   !string.Equals(item.Team, "player",
                                       StringComparison.OrdinalIgnoreCase))
                               .All(item => !item.Alive)
                           && !encounter.ActionHistory.Any(item =>
                               string.Equals(item, "flee",
                                   StringComparison.OrdinalIgnoreCase));
            return resolved
                ? GeneratedCampaignRelationshipStatus.CHALLENGE_RESOLVED
                : GeneratedCampaignRelationshipStatus.CHALLENGED;
        }
        if (flag == "REFUSE")
            return GeneratedCampaignRelationshipStatus.REFUSED;
        return GeneratedCampaignRelationshipStatus.UNDECIDED;
    }

    private static string NextAction(
        GamePackageDefinition package,
        GeneratedCampaignRelationshipBinding relationship,
        GeneratedCampaignRelationshipStatus status,
        IReadOnlyList<string> questStates)
    {
        if (status == GeneratedCampaignRelationshipStatus.UNDECIDED)
            return "Поговорить и выбрать отношение.";
        if (status == GeneratedCampaignRelationshipStatus.QUEST_ACTIVE)
            return "Продолжить текущее задание.";
        if (status == GeneratedCampaignRelationshipStatus.QUEST_READY)
            return "Завершить текущее задание.";
        if (status == GeneratedCampaignRelationshipStatus.COMPLETED)
            return "Арка завершена.";
        if (status == GeneratedCampaignRelationshipStatus.CHALLENGED)
            return "Разрешить начатую встречу.";
        if (status == GeneratedCampaignRelationshipStatus.CHALLENGE_RESOLVED)
            return "Вызов разрешён.";
        if (status == GeneratedCampaignRelationshipStatus.REFUSED)
            return "Предложение отклонено.";
        var nextIndex = questStates.ToList().FindIndex(item =>
            item == "not_started");
        if (nextIndex >= 0)
        {
            var title = package.Game.Quests.Single(item =>
                item.Id == relationship.QuestArc[nextIndex].QuestId).Title;
            return "Вернуться к персонажу и начать «" + title + "».";
        }
        return "Вернуться к персонажу.";
    }

    private static string StatusTitle(
        GeneratedCampaignRelationshipStatus status) => status switch
    {
        GeneratedCampaignRelationshipStatus.UNDECIDED => "Решение не принято",
        GeneratedCampaignRelationshipStatus.SUPPORTED => "Поддержка выбрана",
        GeneratedCampaignRelationshipStatus.QUEST_ACTIVE => "Задание выполняется",
        GeneratedCampaignRelationshipStatus.QUEST_READY => "Задание готово к сдаче",
        GeneratedCampaignRelationshipStatus.COMPLETED => "Отношения завершены",
        GeneratedCampaignRelationshipStatus.CHALLENGED => "Вызов брошен",
        GeneratedCampaignRelationshipStatus.CHALLENGE_RESOLVED => "Вызов разрешён",
        GeneratedCampaignRelationshipStatus.REFUSED => "Предложение отклонено",
        _ => "Неизвестно"
    };

    private static string Consequences(
        GeneratedCampaignRelationshipStatus status,
        int completed,
        int total,
        double reputation) =>
        StatusTitle(status) + "; задания " + completed + "/" + total
        + "; репутация " + reputation.ToString("0.##",
            System.Globalization.CultureInfo.InvariantCulture);
}
