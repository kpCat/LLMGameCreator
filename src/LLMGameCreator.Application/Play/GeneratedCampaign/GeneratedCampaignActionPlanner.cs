using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

internal sealed record GeneratedCampaignPlannedAction(
    GeneratedCampaignAction Action,
    PlayerCommand? PlayerCommand,
    GameRuntimeCommand? RuntimeCommand);

public sealed class GeneratedCampaignActionPlanner
{
    internal IReadOnlyList<GeneratedCampaignPlannedAction> Plan(
        GamePackageDefinition package,
        UnifiedRuntimeSession session)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        var result = new List<GeneratedCampaignPlannedAction>();
        var state = session.GameplayState;

        if (state.ActiveDialogue is { Open: true } dialogue)
        {
            PlanDialogue(package, dialogue, result);
            return result;
        }

        if (state.ActiveEncounter is { Active: true } encounter)
        {
            PlanEncounter(package, encounter, result);
            return result;
        }

        PlanMap(package, session, result);
        PlanRegionActivities(package, session, result);
        PlanCompletableQuests(package, state, result);
        return result;
    }

    private static void PlanDialogue(
        GamePackageDefinition package,
        DialogueRuntimeState dialogue,
        List<GeneratedCampaignPlannedAction> result)
    {
        var definition = package.Game.Dialogues.FirstOrDefault(item => IdEquals(item.Id, dialogue.DialogueId));
        var node = definition?.Nodes.FirstOrDefault(item => IdEquals(item.Id, dialogue.CurrentNodeId));
        foreach (var choice in node?.Choices ?? [])
        {
            Add(result, GeneratedCampaignActionKind.ChooseDialogue, choice.Text,
                "Выбрать ответ в разговоре", true,
                GameRuntimeCommand.ChooseDialogueOption(choice.Id), targetTitle: choice.Text,
                primary: result.Count == 0);
        }

        Add(result, GeneratedCampaignActionKind.CloseDialogue, "Закрыть разговор",
            "Вернуться к исследованию карты", true,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.CloseDialogue });
    }

    private static void PlanEncounter(
        GamePackageDefinition package,
        EncounterRuntimeState encounter,
        List<GeneratedCampaignPlannedAction> result)
    {
        if (encounter.Participants.Count == 0) return;
        var turnIndex = Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1);
        var current = encounter.Participants[turnIndex];
        if (!current.Alive) return;

        if (!KindEquals(current.Team, "player"))
        {
            Add(result, GeneratedCampaignActionKind.RunEncounterAi,
                "Продолжить ход противников", "Дождаться следующего хода игрока", true,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi },
                targetTitle: current.Name, primary: true);
            return;
        }

        var targets = encounter.Participants
            .Where(item => item.Alive && !KindEquals(item.Team, current.Team))
            .ToList();
        foreach (var target in targets)
        {
            Add(result, GeneratedCampaignActionKind.BasicAttack,
                "Атаковать: " + SafeTitle(target.Name, "Противник"),
                "Обычная атака по выбранной цели", true,
                GameRuntimeCommand.BasicAttack(current.Id, target.Id),
                targetTitle: SafeTitle(target.Name, "Противник"), primary: result.Count == 0);
        }

        var definition = package.Game.Encounters.FirstOrDefault(item => IdEquals(item.Id, encounter.EncounterId));
        var participant = definition?.Participants.FirstOrDefault(item => IdEquals(item.Id, current.Id));
        foreach (var abilityId in participant?.Abilities ?? [])
        {
            var ability = package.Game.Abilities.FirstOrDefault(item => IdEquals(item.Id, abilityId));
            if (ability is null) continue;
            foreach (var target in targets)
            {
                var targetTitle = SafeTitle(target.Name, "Противник");
                Add(result, GeneratedCampaignActionKind.UseAbility,
                    SafeTitle(ability.Name, "Способность") + ": " + targetTitle,
                    "Использовать способность по выбранной цели", true,
                    GameRuntimeCommand.UseAbility(ability.Id, current.Id, target.Id),
                    targetTitle: targetTitle);
            }
        }

        Add(result, GeneratedCampaignActionKind.EndTurn, "Завершить ход",
            "Передать ход и выполнить ограниченную последовательность действий противников", true,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.EndTurn, TargetId = current.Id });
        Add(result, GeneratedCampaignActionKind.FleeEncounter, "Покинуть встречу",
            "Завершить встречу без победы", true,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });
    }

    private static void PlanMap(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        List<GeneratedCampaignPlannedAction> result)
    {
        var map = package.Game.Maps.SingleOrDefault(item => IdEquals(item.Id, session.MapState.CurrentMapId));
        if (map is null) return;

        var movements = new[]
        {
            (GeneratedCampaignActionKind.MoveUp, Direction2D.Up, "Вверх"),
            (GeneratedCampaignActionKind.MoveDown, Direction2D.Down, "Вниз"),
            (GeneratedCampaignActionKind.MoveLeft, Direction2D.Left, "Влево"),
            (GeneratedCampaignActionKind.MoveRight, Direction2D.Right, "Вправо")
        };
        foreach (var (kind, direction, title) in movements)
        {
            var (x, y) = Offset(session.MapState.PlayerPosition.X, session.MapState.PlayerPosition.Y, direction);
            var valid = Walkable(package, map, x, y);
            Add(result, kind, title,
                valid ? "Перейти на соседнюю клетку" : "Соседняя клетка недоступна",
                valid, PlayerCommand.Move(direction),
                disabled: valid ? string.Empty : "Путь перекрыт или выходит за границы карты");
        }

        var nearby = map.Entities
            .Where(entity => Math.Abs(entity.Position.X - session.MapState.PlayerPosition.X)
                             + Math.Abs(entity.Position.Y - session.MapState.PlayerPosition.Y) <= 1)
            .Where(entity => Interactable(package, entity))
            .OrderBy(entity => InteractionPriority(package, entity))
            .ThenBy(entity => entity.Position.Y)
            .ThenBy(entity => entity.Position.X)
            .ThenBy(entity => entity.Id, StringComparer.Ordinal)
            .ToList();
        foreach (var entity in nearby)
        {
            var title = EntityTitle(package, entity);
            var command = PlayerCommand.Interact();
            command.TargetId = entity.Id;
            Add(result, GeneratedCampaignActionKind.Interact,
                InteractionTitle(package, entity, title),
                InteractionDescription(package, entity, title), true, command,
                targetTitle: title, primary: !result.Any(item => item.Action.Kind == GeneratedCampaignActionKind.Interact));
        }
    }

    private static void PlanRegionActivities(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        List<GeneratedCampaignPlannedAction> result)
    {
        var scene = package.GeneratedContent.Scenes.FirstOrDefault(item =>
            IdEquals(item.PackageMapId, session.MapState.CurrentMapId));
        var region = package.GeneratedContent.Regions.FirstOrDefault(item =>
            item.SceneIds.Any(sceneId => IdEquals(sceneId, session.MapState.CurrentMapId)
                                         || scene is not null && IdEquals(sceneId, scene.SourceId)));
        if (region is null) return;

        foreach (var generated in package.GeneratedContent.Encounters
                     .Where(item => IdEquals(item.RegionId, region.SourceId))
                     .OrderBy(item => item.Title, StringComparer.CurrentCulture))
        {
            var definitions = package.Game.Encounters
                .Where(item => string.Equals(item.Name, generated.Title, StringComparison.Ordinal))
                .ToList();
            if (definitions.Count != 1) continue;
            var definition = definitions[0];
            if (session.GameplayState.ActiveEncounter is { Active: false } completed
                && IdEquals(completed.EncounterId, definition.Id)) continue;
            Add(result, GeneratedCampaignActionKind.StartEncounter,
                SafeTitle(generated.Title, "Встреча"),
                SafeTitle(generated.Description, "Начать встречу в текущем регионе"), true,
                GameRuntimeCommand.StartEncounter(definition.Id),
                targetTitle: SafeTitle(generated.Title, "Встреча"));
        }
    }

    private static void PlanCompletableQuests(
        GamePackageDefinition package,
        GameRuntimeState state,
        List<GeneratedCampaignPlannedAction> result)
    {
        foreach (var quest in state.Quests.Where(item => item.State != "completed"
                                                          && item.Objectives.Count > 0
                                                          && item.Objectives.All(objective => objective.Completed)))
        {
            var title = QuestTitle(package, quest.QuestId);
            Add(result, GeneratedCampaignActionKind.CompleteQuest,
                "Завершить задание: " + title,
                "Получить награду и применить последствия задания", true,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.CompleteQuest, Id = quest.QuestId },
                targetTitle: title);
        }
    }

    private static void Add(
        List<GeneratedCampaignPlannedAction> target,
        GeneratedCampaignActionKind kind,
        string title,
        string description,
        bool enabled,
        PlayerCommand command,
        string disabled = "",
        string targetTitle = "",
        bool primary = false)
    {
        var token = string.Join("|", kind, command.Type, command.Direction, command.TargetId, command.Payload);
        target.Add(new GeneratedCampaignPlannedAction(new GeneratedCampaignAction
        {
            ActionId = OpaqueId(token),
            Kind = kind,
            Title = title,
            Description = description,
            Enabled = enabled,
            DisabledReason = disabled,
            TargetTitle = targetTitle,
            Primary = primary
        }, command, null));
    }

    private static void Add(
        List<GeneratedCampaignPlannedAction> target,
        GeneratedCampaignActionKind kind,
        string title,
        string description,
        bool enabled,
        GameRuntimeCommand command,
        string disabled = "",
        string targetTitle = "",
        bool primary = false)
    {
        var token = string.Join("|", kind, command.Type, command.Id, command.TargetId,
            command.InventoryId, command.Amount, string.Join(";", command.Args.OrderBy(item => item.Key)
                .Select(item => item.Key + "=" + item.Value)));
        target.Add(new GeneratedCampaignPlannedAction(new GeneratedCampaignAction
        {
            ActionId = OpaqueId(token),
            Kind = kind,
            Title = title,
            Description = description,
            Enabled = enabled,
            DisabledReason = disabled,
            TargetTitle = targetTitle,
            Primary = primary
        }, null, command));
    }

    private static string OpaqueId(string value) => "a-" + Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..20];

    private static (int X, int Y) Offset(int x, int y, Direction2D direction) => direction switch
    {
        Direction2D.Up => (x, y - 1),
        Direction2D.Down => (x, y + 1),
        Direction2D.Left => (x - 1, y),
        _ => (x + 1, y)
    };

    internal static bool Walkable(GamePackageDefinition package, MapDefinition map, int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) return false;
        var tileId = map.Tiles.LastOrDefault(item => item.X == x && item.Y == y)?.TileId
                     ?? map.DefaultTileId;
        return package.Game.TilePrototypes.FirstOrDefault(item => IdEquals(item.Id, tileId))?.Walkable == true
               && !map.Entities.Any(entity => entity.Position.X == x && entity.Position.Y == y
                                             && Collidable(package, entity));
    }

    internal static string EntityTitle(GamePackageDefinition package, EntityInstanceDefinition entity)
    {
        var components = Components(package, entity).ToList();
        var interaction = components.FirstOrDefault(item => KindEquals(item.Type, "interactable"));
        if (interaction is not null
            && interaction.Args.TryGetValue(MapTransitionInteractionContract.TransitionKindKey, out var transition)
            && KindEquals(transition, MapTransitionInteractionContract.TransitionKindMap)
            && interaction.Args.TryGetValue(MapTransitionInteractionContract.DestinationMapIdKey, out var mapId))
        {
            var destination = package.Game.Maps.FirstOrDefault(item => IdEquals(item.Id, mapId));
            return "Переход в " + SafeTitle(destination?.Name, "другой регион");
        }

        if (interaction is not null && interaction.Args.TryGetValue("dialogueId", out var dialogueId))
        {
            var dialogue = package.Game.Dialogues.FirstOrDefault(item => IdEquals(item.Id, dialogueId));
            if (dialogue is not null) return SafeTitle(dialogue.Title, "Собеседник");
        }

        var entitySuffix = entity.Id.Split('/').LastOrDefault() ?? string.Empty;
        var generatedNpc = package.GeneratedContent.Npcs.FirstOrDefault(item =>
            item.SourceId.Replace('/', '_').EndsWith(entitySuffix, StringComparison.OrdinalIgnoreCase));
        if (generatedNpc is not null) return SafeTitle(generatedNpc.Name, "Персонаж");
        var generatedItem = package.GeneratedContent.Items.FirstOrDefault(item =>
            item.SourceId.Replace('/', '_').EndsWith(entitySuffix, StringComparison.OrdinalIgnoreCase));
        if (generatedItem is not null) return SafeTitle(generatedItem.Name, "Тайник");
        var prototype = package.Game.EntityPrototypes.FirstOrDefault(item => IdEquals(item.Id, entity.PrototypeId));
        return SafeTitle(prototype?.Name, "Объект");
    }

    internal static string InteractionDescription(
        GamePackageDefinition package,
        EntityInstanceDefinition entity,
        string title)
    {
        var component = Components(package, entity).FirstOrDefault(item => KindEquals(item.Type, "interactable"));
        if (component?.Args.ContainsKey("dialogueId") == true) return "Поговорить: " + title;
        if (component?.Args.TryGetValue(MapTransitionInteractionContract.TransitionKindKey, out var transition) == true
            && KindEquals(transition, MapTransitionInteractionContract.TransitionKindMap))
            return title;
        return "Взаимодействовать: " + title;
    }

    internal static bool Interactable(GamePackageDefinition package, EntityInstanceDefinition entity) =>
        Components(package, entity).Any(item => KindEquals(item.Type, "interactable"));

    internal static IEnumerable<ComponentDefinition> Components(
        GamePackageDefinition package,
        EntityInstanceDefinition entity) => entity.Components.Concat(
        package.Game.EntityPrototypes.FirstOrDefault(item => IdEquals(item.Id, entity.PrototypeId))?.Components ?? []);

    internal static string HumanLabel(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var last = value.Split('/').LastOrDefault() ?? value;
        var words = last.Replace('_', ' ').Replace('-', ' ').Trim();
        if (string.IsNullOrWhiteSpace(words)) return fallback;
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words.ToLowerInvariant());
    }

    private static int InteractionPriority(GamePackageDefinition package, EntityInstanceDefinition entity)
    {
        var component = Components(package, entity).FirstOrDefault(item => KindEquals(item.Type, "interactable"));
        if (component?.Args.ContainsKey("dialogueId") == true) return 0;
        if (component?.Args.ContainsKey(MapTransitionInteractionContract.DestinationMapIdKey) == true) return 2;
        return 1;
    }

    private static string InteractionTitle(GamePackageDefinition package, EntityInstanceDefinition entity, string title)
    {
        var component = Components(package, entity).FirstOrDefault(item => KindEquals(item.Type, "interactable"));
        if (component?.Args.ContainsKey("dialogueId") == true) return "Поговорить: " + title;
        return component?.Args.ContainsKey(MapTransitionInteractionContract.DestinationMapIdKey) == true
            ? title : "Осмотреть: " + title;
    }

    private static bool Collidable(GamePackageDefinition package, EntityInstanceDefinition entity) =>
        Components(package, entity).Any(item => KindEquals(item.Type, "collidable"));

    private static string QuestTitle(GamePackageDefinition package, string id) => SafeTitle(
        package.Game.Quests.FirstOrDefault(item => IdEquals(item.Id, id))?.Title, "Задание");

    private static string SafeTitle(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool KindEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
