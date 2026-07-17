using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignProjectionService
{
    public GeneratedCampaignSnapshot Project(
        GeneratedCampaignSessionStatus status,
        GeneratedCampaignProjectTruth? truth,
        GamePackageDefinition? package,
        UnifiedRuntimeSession? session,
        IReadOnlyList<GeneratedCampaignAction> actions,
        IReadOnlyList<string> events,
        string slot,
        IReadOnlyList<string> diagnostics,
        GeneratedCampaignSaveState? saveState = null)
    {
        if (package is null || session is null || truth is null)
        {
            return new GeneratedCampaignSnapshot
            {
                Status = status,
                StatusTitle = StatusTitle(status),
                StatusDescription = HumanDiagnostic(diagnostics.FirstOrDefault(), status),
                SaveState = saveState ?? new GeneratedCampaignSaveState { Slot = slot },
                Diagnostics = diagnostics
            };
        }

        var map = package.Game.Maps.FirstOrDefault(item => IdEquals(item.Id, session.MapState.CurrentMapId));
        var region = CurrentRegion(package, session.MapState.CurrentMapId);
        var quests = Quests(package, session.GameplayState);
        return new GeneratedCampaignSnapshot
        {
            Status = status,
            StatusTitle = StatusTitle(status),
            StatusDescription = status == GeneratedCampaignSessionStatus.ACTIVE
                ? "Кампания активна" : HumanDiagnostic(diagnostics.FirstOrDefault(), status),
            ProjectTitle = Safe(package.Manifest.Title, "Сгенерированная игра"),
            WorldTitle = Safe(package.GeneratedContent.Profile.Title, package.Manifest.Title),
            WorldSeed = truth.GenerationSeed,
            CurrentRegionTitle = Safe(region?.Title, "Текущий регион"),
            CurrentMapTitle = Safe(map?.Name, "Карта"),
            SessionSha256 = Hash(session),
            Map = map is null ? null : Map(package, map, session),
            Player = new GeneratedCampaignPlayerProjection
            {
                X = session.MapState.PlayerPosition.X,
                Y = session.MapState.PlayerPosition.Y
            },
            Nearby = map is null ? [] : Nearby(package, map, session),
            Actions = actions,
            Resources = session.GameplayState.Resources.Select(resource => new GeneratedCampaignTextRow
            {
                Title = ResourceTitle(package, resource.ResourceId),
                Value = Amount(resource.Amount, resource.Capacity)
            }).ToList(),
            Stats = session.GameplayState.Stats.Select(stat => new GeneratedCampaignTextRow
            {
                Title = Safe(package.Game.Stats.FirstOrDefault(item => IdEquals(item.Id, stat.StatId))?.Name,
                    GeneratedCampaignActionPlanner.HumanLabel(stat.StatId, "Характеристика")),
                Value = Number(stat.Value)
            }).ToList(),
            Progressions = session.GameplayState.Progressions.Select(progression => new GeneratedCampaignTextRow
            {
                Title = Safe(package.Game.Progressions.FirstOrDefault(item =>
                        IdEquals(item.Id, progression.ProgressionId))?.Name,
                    GeneratedCampaignActionPlanner.HumanLabel(progression.ProgressionId, "Развитие")),
                Value = Number(progression.Amount)
                        + (string.IsNullOrWhiteSpace(progression.StageId) ? string.Empty
                            : " — " + GeneratedCampaignActionPlanner.HumanLabel(progression.StageId, "этап"))
            }).ToList(),
            Inventory = session.GameplayState.Inventories.SelectMany(inventory => inventory.Stacks)
                .Select(stack => new GeneratedCampaignTextRow
                {
                    Title = ItemTitle(package, stack.ItemId),
                    Value = Number(stack.Amount)
                }).ToList(),
            Equipment = session.GameplayState.Equipment.SelectMany(equipment => equipment.Slots)
                .Where(equipment => !string.IsNullOrWhiteSpace(equipment.ItemId))
                .Select(equipment => new GeneratedCampaignTextRow
                {
                    Title = GeneratedCampaignActionPlanner.HumanLabel(equipment.SlotId, "Снаряжение"),
                    Value = ItemTitle(package, equipment.ItemId!)
                }).ToList(),
            ActiveQuests = quests.Where(quest => quest.StateTitle != "Завершено")
                .Select(quest => new GeneratedCampaignTextRow
                {
                    Title = quest.Title,
                    Value = string.Join(", ", quest.Objectives.Select(objective => objective.Progress))
                }).ToList(),
            Quests = quests,
            Dialogue = Dialogue(package, session),
            Encounter = Encounter(package, session),
            Factions = session.GameplayState.Factions.Select(faction => new GeneratedCampaignTextRow
            {
                Title = Safe(package.Game.Factions.FirstOrDefault(item =>
                        IdEquals(item.Id, faction.FactionId))?.Name,
                    GeneratedCampaignActionPlanner.HumanLabel(faction.FactionId, "Фракция")),
                Value = Number(faction.Reputation) + " — " + RelationTitle(faction.RelationKind)
            }).ToList(),
            RecentEvents = events,
            SaveState = saveState ?? new GeneratedCampaignSaveState { Slot = slot },
            TechnicalDetails = new Dictionary<string, string>
            {
                ["projectFolder"] = truth.ProjectFolder,
                ["worldId"] = truth.WorldId,
                ["packageSha256"] = truth.PackageSha256,
                ["compositionPackageSha256"] = truth.CompositionPackageSha256,
                ["authoringFingerprint"] = truth.QualifiedAuthoringFingerprint,
                ["sessionSha256"] = Hash(session),
                ["currentMapId"] = session.MapState.CurrentMapId
            },
            Diagnostics = diagnostics
        };
    }

    public static IReadOnlyList<GeneratedCampaignSaveEntryProjection> ProjectSaves(
        IEnumerable<GeneratedGameplaySaveEntry> entries) =>
        entries.Select(entry => new GeneratedCampaignSaveEntryProjection
        {
            Entry = entry,
            Slot = entry.SlotName,
            StatusTitle = SaveStatusTitle(entry.Status),
            SavedWorldTitle = Safe(entry.SavedWorldTitle, "Сохранённый мир"),
            CurrentWorldTitle = Safe(entry.CurrentWorldTitle, "Текущий мир"),
            RevisionCount = entry.RevisionCount,
            MigrationSummary = entry.Migration is null ? string.Empty
                : "Сохранено: " + entry.Migration.PreservedCounts.Values.Sum()
                  + ", сброшено: " + entry.Migration.DroppedCounts.Values.Sum(),
            CanContinue = entry.Status == GeneratedGameplaySaveStatus.CURRENT,
            CanMigrate = entry.Status is GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED
                or GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED
        }).ToList();

    private static GeneratedCampaignMapProjection Map(
        GamePackageDefinition package,
        MapDefinition map,
        UnifiedRuntimeSession session)
    {
        var cells = new List<GeneratedCampaignMapCell>(map.Width * map.Height);
        var entities = new List<GeneratedCampaignMapEntity>(map.Entities.Count);
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var atCell = map.Entities.Where(entity => entity.Position.X == x && entity.Position.Y == y)
                    .OrderByDescending(entity => GeneratedCampaignActionPlanner.Interactable(package, entity))
                    .ThenBy(entity => entity.Id, StringComparer.Ordinal)
                    .ToList();
                var primary = atCell.FirstOrDefault();
                var walkable = GeneratedCampaignActionPlanner.Walkable(package, map, x, y);
                var player = session.MapState.PlayerPosition.X == x && session.MapState.PlayerPosition.Y == y;
                var title = primary is null ? TileTitle(package, map, x, y)
                    : GeneratedCampaignActionPlanner.EntityTitle(package, primary);
                var interactable = atCell.Any(entity => GeneratedCampaignActionPlanner.Interactable(package, entity));
                cells.Add(new GeneratedCampaignMapCell
                {
                    X = x,
                    Y = y,
                    Walkable = walkable,
                    PlayerPresent = player,
                    PrimarySymbol = player ? "●" : primary is null ? walkable ? "·" : "■"
                        : Symbol(package, primary),
                    PrimaryTitle = title,
                    EntityCount = atCell.Count,
                    InteractionAvailable = interactable,
                    Blocked = !walkable
                });
                entities.AddRange(atCell.Select(entity => new GeneratedCampaignMapEntity
                {
                    Title = GeneratedCampaignActionPlanner.EntityTitle(package, entity),
                    X = x,
                    Y = y,
                    Symbol = Symbol(package, entity),
                    Interactable = GeneratedCampaignActionPlanner.Interactable(package, entity)
                }));
            }
        }

        return new GeneratedCampaignMapProjection
        {
            Width = map.Width,
            Height = map.Height,
            Cells = cells,
            Entities = entities
        };
    }

    private static IReadOnlyList<GeneratedCampaignNearbyProjection> Nearby(
        GamePackageDefinition package,
        MapDefinition map,
        UnifiedRuntimeSession session) => map.Entities
        .Where(entity => Math.Abs(entity.Position.X - session.MapState.PlayerPosition.X)
                         + Math.Abs(entity.Position.Y - session.MapState.PlayerPosition.Y) <= 1)
        .Select(entity =>
        {
            var title = GeneratedCampaignActionPlanner.EntityTitle(package, entity);
            return new GeneratedCampaignNearbyProjection
            {
                Title = title,
                Description = GeneratedCampaignActionPlanner.InteractionDescription(package, entity, title),
                InteractionAvailable = GeneratedCampaignActionPlanner.Interactable(package, entity)
            };
        }).OrderBy(item => item.Title, StringComparer.CurrentCulture).ToList();

    private static GeneratedCampaignDialogue? Dialogue(
        GamePackageDefinition package,
        UnifiedRuntimeSession session)
    {
        var active = session.GameplayState.ActiveDialogue;
        if (active is not { Open: true }) return null;
        var dialogue = package.Game.Dialogues.FirstOrDefault(item => IdEquals(item.Id, active.DialogueId));
        var node = dialogue?.Nodes.FirstOrDefault(item => IdEquals(item.Id, active.CurrentNodeId));
        return new GeneratedCampaignDialogue
        {
            Open = true,
            Title = Safe(dialogue?.Title, "Разговор"),
            Speaker = Safe(dialogue?.Title, "Собеседник"),
            Text = Safe(node?.Text, "Разговор продолжается."),
            Choices = (node?.Choices ?? []).Select(choice => new GeneratedCampaignDialogueChoice
            {
                Title = Safe(choice.Text, "Продолжить"),
                Description = "Ответить собеседнику"
            }).ToList()
        };
    }

    private static GeneratedCampaignEncounter? Encounter(
        GamePackageDefinition package,
        UnifiedRuntimeSession session)
    {
        var active = session.GameplayState.ActiveEncounter;
        if (active is null) return null;
        var definition = package.Game.Encounters.FirstOrDefault(item => IdEquals(item.Id, active.EncounterId));
        var turnIndex = active.Participants.Count == 0 ? -1
            : Math.Clamp(active.TurnIndex, 0, active.Participants.Count - 1);
        var current = turnIndex < 0 ? null : active.Participants[turnIndex];
        return new GeneratedCampaignEncounter
        {
            Title = Safe(definition?.Name, "Встреча"),
            Active = active.Active,
            Round = active.Round,
            CurrentTurnTitle = current is null ? string.Empty : Safe(current.Name, "Участник"),
            Participants = active.Participants.Select((participant, index) =>
                new GeneratedCampaignEncounterParticipant
                {
                    Title = Safe(participant.Name, "Участник"),
                    TeamTitle = KindEquals(participant.Team, "player") ? "Игрок" : "Противник",
                    Alive = participant.Alive,
                    CurrentTurn = index == turnIndex && active.Active,
                    Resources = participant.Resources.Select(resource => new GeneratedCampaignTextRow
                    {
                        Title = ResourceTitle(package, resource.ResourceId),
                        Value = Amount(resource.Amount, resource.Capacity)
                    }).ToList()
                }).ToList()
        };
    }

    private static IReadOnlyList<GeneratedCampaignQuest> Quests(
        GamePackageDefinition package,
        GameRuntimeState state) => state.Quests.Select(runtime =>
    {
        var definition = package.Game.Quests.FirstOrDefault(item => IdEquals(item.Id, runtime.QuestId));
        var objectives = runtime.Objectives.Select(objective =>
        {
            var source = definition?.Objectives.FirstOrDefault(item => IdEquals(item.Id, objective.ObjectiveId));
            return new GeneratedCampaignQuestObjective
            {
                Title = ObjectiveTitle(package, source, objective),
                Progress = Number(objective.CurrentAmount) + " / " + Number(objective.RequiredAmount),
                Completed = objective.Completed
            };
        }).ToList();
        return new GeneratedCampaignQuest
        {
            Title = Safe(definition?.Title, "Задание"),
            StateTitle = runtime.State == "completed" ? "Завершено" : "Активно",
            Completable = runtime.State != "completed" && objectives.Count > 0
                          && objectives.All(objective => objective.Completed),
            Objectives = objectives
        };
    }).ToList();

    private static string ObjectiveTitle(
        GamePackageDefinition package,
        QuestObjectiveDefinition? definition,
        QuestObjectiveRuntimeState runtime)
    {
        var kind = definition?.Kind ?? runtime.Kind;
        var target = definition?.TargetId ?? runtime.TargetId;
        if (KindEquals(kind, "complete_encounter"))
            return "Завершить встречу «" + Safe(package.Game.Encounters.FirstOrDefault(item =>
                IdEquals(item.Id, target))?.Name, "Встреча") + "»";
        if (KindEquals(kind, "has_item"))
            return "Получить предмет «" + ItemTitle(package, target ?? string.Empty) + "»";
        return GeneratedCampaignActionPlanner.HumanLabel(kind, "Цель задания");
    }

    private static GeneratedRegionDefinition? CurrentRegion(GamePackageDefinition package, string mapId)
    {
        var scene = package.GeneratedContent.Scenes.FirstOrDefault(item => IdEquals(item.PackageMapId, mapId));
        return package.GeneratedContent.Regions.FirstOrDefault(region => region.SceneIds.Any(sceneId =>
            IdEquals(sceneId, mapId) || scene is not null && IdEquals(sceneId, scene.SourceId)));
    }

    private static string TileTitle(GamePackageDefinition package, MapDefinition map, int x, int y)
    {
        var tileId = map.Tiles.LastOrDefault(item => item.X == x && item.Y == y)?.TileId
                     ?? map.DefaultTileId;
        return Safe(package.Game.TilePrototypes.FirstOrDefault(item => IdEquals(item.Id, tileId))?.Name,
            "Клетка карты");
    }

    private static string Symbol(GamePackageDefinition package, EntityInstanceDefinition entity)
    {
        var components = GeneratedCampaignActionPlanner.Components(package, entity).ToList();
        if (components.Any(component => component.Args.ContainsKey("dialogueId"))) return "☺";
        if (components.Any(component => component.Args.ContainsKey(
                MapTransitionInteractionContract.DestinationMapIdKey))) return "⇥";
        return GeneratedCampaignActionPlanner.Interactable(package, entity) ? "◆" : "■";
    }

    private static string ResourceTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Resources.FirstOrDefault(item => IdEquals(item.Id, id))?.Name,
        GeneratedCampaignActionPlanner.HumanLabel(id, "Ресурс"));

    private static string ItemTitle(GamePackageDefinition package, string id) => Safe(
        package.Game.Items.FirstOrDefault(item => IdEquals(item.Id, id))?.Name,
        GeneratedCampaignActionPlanner.HumanLabel(id, "Предмет"));

    private static string RelationTitle(string value) => value.ToLowerInvariant() switch
    {
        "friendly" => "дружественные отношения",
        "hostile" => "враждебные отношения",
        _ => "нейтральные отношения"
    };

    private static string SaveStatusTitle(GeneratedGameplaySaveStatus status) =>
        status switch
        {
            GeneratedGameplaySaveStatus.CURRENT => "Можно продолжить",
            GeneratedGameplaySaveStatus.PACKAGE_REBASE_REQUIRED =>
                "Требуется перенос в текущую сборку",
            GeneratedGameplaySaveStatus.WORLD_MIGRATION_REQUIRED =>
                "Требуется перенос в новый мир",
            GeneratedGameplaySaveStatus.LEGACY_RAW =>
                "Старое техническое сохранение",
            _ => "Сохранение недоступно"
        };

    private static string HumanDiagnostic(string? diagnostic, GeneratedCampaignSessionStatus status)
    {
        if (string.IsNullOrWhiteSpace(diagnostic)) return StatusTitle(status);
        if (diagnostic.Contains("no_project", StringComparison.Ordinal)) return "Откройте проект игры.";
        if (diagnostic.Contains("not_generated", StringComparison.Ordinal))
            return "Этот проект не содержит сгенерированной кампании.";
        if (diagnostic.Contains("truth_changed", StringComparison.Ordinal)
            || diagnostic.Contains("changed", StringComparison.Ordinal))
            return "Проект изменился. Начните новую игру или перенесите сохранение.";
        if (diagnostic.Contains("not_ready", StringComparison.Ordinal)
            || diagnostic.Contains("not_current", StringComparison.Ordinal)
            || diagnostic.Contains("travel", StringComparison.Ordinal))
            return "Соберите и проверьте сгенерированный мир перед игрой.";
        if (diagnostic.Contains("migration", StringComparison.Ordinal))
            return "Сохранение нужно явно перенести в текущий мир.";
        if (diagnostic.Contains("busy", StringComparison.Ordinal))
            return "Проект занят другой операцией. Повторите действие после её завершения.";
        return status == GeneratedCampaignSessionStatus.FAILED
            ? "Действие не выполнено. Технические сведения содержат причину."
            : StatusTitle(status);
    }

    private static string Hash(UnifiedRuntimeSession session) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(session))))
        .ToLowerInvariant();

    private static string Amount(double amount, double? capacity) => Number(amount)
        + (capacity is null ? string.Empty : " / " + Number(capacity.Value));

    private static string Number(double value) => value.ToString("0.##",
        System.Globalization.CultureInfo.InvariantCulture);

    private static string Safe(string? value, string? fallback) => string.IsNullOrWhiteSpace(value)
        ? string.IsNullOrWhiteSpace(fallback) ? "Без названия" : fallback.Trim()
        : value.Trim();

    private static bool IdEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool KindEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static string StatusTitle(GeneratedCampaignSessionStatus status) => status switch
    {
        GeneratedCampaignSessionStatus.NO_PROJECT => "Проект не открыт",
        GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED => "Кампания недоступна",
        GeneratedCampaignSessionStatus.PROJECT_NOT_READY => "Кампания не готова",
        GeneratedCampaignSessionStatus.READY => "Готово к игре",
        GeneratedCampaignSessionStatus.ACTIVE => "Игра",
        GeneratedCampaignSessionStatus.STALE_PROJECT => "Проект изменён",
        GeneratedCampaignSessionStatus.SAVE_MIGRATION_REQUIRED => "Требуется перенос сохранения",
        _ => "Ошибка кампании"
    };
}
