using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

internal sealed record GeneratedCampaignPlannedAction(GeneratedCampaignAction Action, PlayerCommand? PlayerCommand, GameRuntimeCommand? RuntimeCommand);

public sealed class GeneratedCampaignActionPlanner
{
    internal IReadOnlyList<GeneratedCampaignPlannedAction> Plan(GamePackageDefinition package, UnifiedRuntimeSession session)
    {
        var result = new List<GeneratedCampaignPlannedAction>();
        var state = session.GameplayState;
        if (state.ActiveDialogue is { Open: true } dialogue)
        {
            var definition = package.Game.Dialogues.FirstOrDefault(x => x.Id == dialogue.DialogueId);
            var node = definition?.Nodes.FirstOrDefault(x => x.Id == dialogue.CurrentNodeId);
            foreach (var choice in node?.Choices ?? []) Add(result, GeneratedCampaignActionKind.ChooseDialogue, choice.Text, "Выбрать ответ", true, GameRuntimeCommand.ChooseDialogueOption(choice.Id));
            Add(result, GeneratedCampaignActionKind.CloseDialogue, "Закрыть разговор", "Вернуться к карте", true, new GameRuntimeCommand { Type = GameRuntimeCommandType.CloseDialogue });
            return result;
        }
        if (state.ActiveEncounter is { Active: true } encounter)
        {
            var player = encounter.Participants.FirstOrDefault(x => x.Team.Equals("player", StringComparison.OrdinalIgnoreCase)) ?? encounter.Participants.FirstOrDefault();
            var target = encounter.Participants.FirstOrDefault(x => x.Alive && !x.Team.Equals(player?.Team, StringComparison.OrdinalIgnoreCase));
            if (player is not null && target is not null) Add(result, GeneratedCampaignActionKind.BasicAttack, "Атаковать", "Атаковать " + target.Name, true, GameRuntimeCommand.BasicAttack(player.Id, target.Id));
            foreach (var ability in package.Game.Abilities.Take(4)) if (player is not null) Add(result, GeneratedCampaignActionKind.UseAbility, ability.Name, "Использовать способность", true, GameRuntimeCommand.UseAbility(ability.Id, player.Id, target?.Id));
            Add(result, GeneratedCampaignActionKind.EndTurn, "Завершить ход", "Передать ход", true, new GameRuntimeCommand { Type = GameRuntimeCommandType.EndTurn });
            Add(result, GeneratedCampaignActionKind.FleeEncounter, "Покинуть встречу", "Попытаться отступить", true, new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });
            return result;
        }
        var map = package.Game.Maps.SingleOrDefault(x => x.Id == session.MapState.CurrentMapId);
        if (map is not null)
        {
            foreach (var (kind, direction, title) in new[] { (GeneratedCampaignActionKind.MoveUp, Direction2D.Up, "Вверх"), (GeneratedCampaignActionKind.MoveDown, Direction2D.Down, "Вниз"), (GeneratedCampaignActionKind.MoveLeft, Direction2D.Left, "Влево"), (GeneratedCampaignActionKind.MoveRight, Direction2D.Right, "Вправо") })
            {
                var (x, y) = Offset(session.MapState.PlayerPosition.X, session.MapState.PlayerPosition.Y, direction);
                var valid = Walkable(package, map, x, y);
                Add(result, kind, title, valid ? "Перейти на соседнюю клетку" : "Путь перекрыт", valid, PlayerCommand.Move(direction), valid ? string.Empty : "Клетка недоступна");
            }
            var nearby = map.Entities.FirstOrDefault(entity => Math.Abs(entity.Position.X - session.MapState.PlayerPosition.X) + Math.Abs(entity.Position.Y - session.MapState.PlayerPosition.Y) <= 1 && Interactable(package, entity));
            if (nearby is not null) Add(result, GeneratedCampaignActionKind.Interact, "Взаимодействовать", "Взаимодействовать: " + EntityTitle(package, nearby), true, PlayerCommand.Interact(), targetTitle: EntityTitle(package, nearby));
        }
        var regionId = package.GeneratedContent.Regions.FirstOrDefault(region => region.SceneIds.Any(scene => package.GeneratedContent.Scenes.Any(s => s.SourceId == scene && s.PackageMapId == session.MapState.CurrentMapId)))?.SourceId;
        foreach (var generatedEncounter in package.GeneratedContent.Encounters.Where(x => string.IsNullOrWhiteSpace(regionId) || x.RegionId == regionId))
        {
            var definition = package.Game.Encounters.FirstOrDefault(x => x.Name == generatedEncounter.Title) ?? package.Game.Encounters.FirstOrDefault();
            if (definition is not null) Add(result, GeneratedCampaignActionKind.StartEncounter, generatedEncounter.Title, generatedEncounter.Description, true, GameRuntimeCommand.StartEncounter(definition.Id), targetTitle: generatedEncounter.Title);
        }
        foreach (var quest in state.Quests.Where(q => q.Objectives.All(o => o.Completed) && q.State != "completed"))
            Add(result, GeneratedCampaignActionKind.CompleteQuest, "Завершить задание", "Завершить: " + QuestTitle(package, quest.QuestId), true, new GameRuntimeCommand { Type = GameRuntimeCommandType.CompleteQuest, Id = quest.QuestId });
        return result;
    }

    private static void Add(List<GeneratedCampaignPlannedAction> target, GeneratedCampaignActionKind kind, string title, string description, bool enabled, PlayerCommand command, string disabled = "", string targetTitle = "") => target.Add(new(new GeneratedCampaignAction { ActionId = "campaign:" + kind, Kind = kind, Title = title, Description = description, Enabled = enabled, DisabledReason = disabled, TargetTitle = targetTitle }, command, null));
    private static void Add(List<GeneratedCampaignPlannedAction> target, GeneratedCampaignActionKind kind, string title, string description, bool enabled, GameRuntimeCommand command, string disabled = "", string targetTitle = "") => target.Add(new(new GeneratedCampaignAction { ActionId = "campaign:" + kind + ":" + command.Type + ":" + command.Id + ":" + command.TargetId, Kind = kind, Title = title, Description = description, Enabled = enabled, DisabledReason = disabled, TargetTitle = targetTitle }, null, command));
    private static (int X, int Y) Offset(int x, int y, Direction2D direction) => direction switch { Direction2D.Up => (x, y - 1), Direction2D.Down => (x, y + 1), Direction2D.Left => (x - 1, y), _ => (x + 1, y) };
    internal static bool Walkable(GamePackageDefinition package, MapDefinition map, int x, int y) { if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) return false; var tile = map.Tiles.LastOrDefault(t => t.X == x && t.Y == y)?.TileId ?? map.DefaultTileId; return package.Game.TilePrototypes.FirstOrDefault(t => t.Id == tile)?.Walkable == true && !map.Entities.Any(e => e.Position.X == x && e.Position.Y == y && Collidable(package, e)); }
    internal static string EntityTitle(GamePackageDefinition p, EntityInstanceDefinition e) => p.Game.EntityPrototypes.FirstOrDefault(x => x.Id == e.PrototypeId)?.Name ?? "Объект";
    internal static bool Interactable(GamePackageDefinition p, EntityInstanceDefinition e) => Components(p,e).Any(x => x.Type == "interactable");
    private static bool Collidable(GamePackageDefinition p, EntityInstanceDefinition e) => Components(p,e).Any(x => x.Type == "collidable");
    private static IEnumerable<ComponentDefinition> Components(GamePackageDefinition p, EntityInstanceDefinition e) => e.Components.Concat(p.Game.EntityPrototypes.FirstOrDefault(x => x.Id == e.PrototypeId)?.Components ?? []);
    private static string QuestTitle(GamePackageDefinition p, string id) => p.Game.Quests.FirstOrDefault(q => q.Id == id)?.Title ?? "Задание";
}
