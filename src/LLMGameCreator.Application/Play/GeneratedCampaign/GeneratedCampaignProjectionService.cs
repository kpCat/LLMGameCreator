using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignProjectionService
{
    public GeneratedCampaignSnapshot Project(GeneratedCampaignSessionStatus status, GeneratedCampaignProjectTruth? truth, GamePackageDefinition? package, UnifiedRuntimeSession? session, IReadOnlyList<GeneratedCampaignAction> actions, IReadOnlyList<string> events, string slot, IReadOnlyList<string> diagnostics)
    {
        if (package is null || session is null || truth is null) return new GeneratedCampaignSnapshot { Status = status, StatusTitle = StatusTitle(status), StatusDescription = diagnostics.FirstOrDefault() ?? StatusTitle(status), Diagnostics = diagnostics };
        var map = package.Game.Maps.FirstOrDefault(x => x.Id == session.MapState.CurrentMapId);
        var region = package.GeneratedContent.Regions.FirstOrDefault(r => r.SceneIds.Any(scene => package.GeneratedContent.Scenes.Any(s => s.SourceId == scene && s.PackageMapId == map?.Id)));
        var dialogue = Dialogue(package, session);
        return new GeneratedCampaignSnapshot
        {
            Status = status, StatusTitle = StatusTitle(status), StatusDescription = status == GeneratedCampaignSessionStatus.ACTIVE ? "Кампания активна" : diagnostics.FirstOrDefault() ?? StatusTitle(status),
            ProjectTitle = package.Manifest.Title, WorldTitle = region?.Title ?? package.GeneratedContent.Profile.Title, WorldSeed = truth.WorldId,
            CurrentRegionTitle = region?.Title ?? "Текущий регион", CurrentMapTitle = map?.Name ?? "Карта", SessionSha256 = Hash(session),
            Map = map is null ? null : Map(package, map, session), Actions = actions,
            Resources = session.GameplayState.Resources.Select(r => new GeneratedCampaignTextRow { Title = HumanResource(package, r.ResourceId), Value = r.Amount.ToString("0.##") + (r.Capacity is null ? string.Empty : "/" + r.Capacity.Value.ToString("0.##")) }).ToList(),
            Stats = session.GameplayState.Stats.Select(s => new GeneratedCampaignTextRow { Title = package.Game.Stats.FirstOrDefault(x => x.Id == s.StatId)?.Name ?? "Характеристика", Value = s.Value.ToString("0.##") }).ToList(),
            Progressions = session.GameplayState.Progressions.Select(p => new GeneratedCampaignTextRow { Title = package.Game.Progressions.FirstOrDefault(x => x.Id == p.ProgressionId)?.Name ?? "Развитие", Value = p.Amount.ToString("0.##") }).ToList(),
            Inventory = session.GameplayState.Inventories.SelectMany(i => i.Stacks).Select(s => new GeneratedCampaignTextRow { Title = package.Game.Items.FirstOrDefault(x => x.Id == s.ItemId)?.Name ?? "Предмет", Value = s.Amount.ToString("0.##") }).ToList(),
            Equipment = session.GameplayState.Equipment.SelectMany(e => e.Slots).Where(s => !string.IsNullOrWhiteSpace(s.ItemId)).Select(s => new GeneratedCampaignTextRow { Title = s.SlotId, Value = package.Game.Items.FirstOrDefault(x => x.Id == s.ItemId)?.Name ?? "Предмет" }).ToList(),
            ActiveQuests = session.GameplayState.Quests.Where(q => q.State != "completed").Select(q => new GeneratedCampaignTextRow { Title = package.Game.Quests.FirstOrDefault(x => x.Id == q.QuestId)?.Title ?? "Задание", Value = string.Join(", ", q.Objectives.Select(o => o.CurrentAmount.ToString("0.##") + "/" + o.RequiredAmount.ToString("0.##"))) }).ToList(),
            Dialogue = dialogue,
            Factions = session.GameplayState.Factions.Select(f => new GeneratedCampaignTextRow { Title = package.Game.Factions.FirstOrDefault(x => x.Id == f.FactionId)?.Name ?? "Фракция", Value = f.Reputation.ToString("0.##") }).ToList(),
            RecentEvents = events, SaveState = new GeneratedCampaignSaveState { Slot = slot },
            TechnicalDetails = new Dictionary<string, string> { ["worldId"] = truth.WorldId, ["packageSha256"] = truth.PackageSha256, ["currentMapId"] = session.MapState.CurrentMapId }, Diagnostics = diagnostics
        };
    }

    private static GeneratedCampaignMapProjection Map(GamePackageDefinition package, MapDefinition map, UnifiedRuntimeSession session)
    {
        var cells = new List<GeneratedCampaignMapCell>(); var entities = new List<GeneratedCampaignMapEntity>();
        for (var y=0; y<map.Height; y++) for (var x=0; x<map.Width; x++) { var entity = map.Entities.FirstOrDefault(e => e.Position.X == x && e.Position.Y == y); var walkable = GeneratedCampaignActionPlanner.Walkable(package,map,x,y); var title = entity is null ? (package.Game.TilePrototypes.FirstOrDefault(t => t.Id == (map.Tiles.LastOrDefault(t => t.X == x && t.Y == y)?.TileId ?? map.DefaultTileId))?.Name ?? "Клетка") : GeneratedCampaignActionPlanner.EntityTitle(package,entity); var player = session.MapState.PlayerPosition.X == x && session.MapState.PlayerPosition.Y == y; cells.Add(new GeneratedCampaignMapCell { X=x,Y=y,Walkable=walkable,PlayerPresent=player,PrimarySymbol=player?"●":entity is null?(walkable?"·":"■"):GeneratedCampaignActionPlanner.Interactable(package,entity)?"◆":"■",PrimaryTitle=title,EntityCount=entity is null?0:1,InteractionAvailable=entity is not null && GeneratedCampaignActionPlanner.Interactable(package,entity),Blocked=!walkable }); if(entity is not null) entities.Add(new GeneratedCampaignMapEntity { Title=title,X=x,Y=y,Symbol=GeneratedCampaignActionPlanner.Interactable(package,entity)?"◆":"■",Interactable=GeneratedCampaignActionPlanner.Interactable(package,entity)}); }
        return new GeneratedCampaignMapProjection { Width=map.Width, Height=map.Height, Cells=cells, Entities=entities };
    }
    private static GeneratedCampaignDialogue? Dialogue(GamePackageDefinition package, UnifiedRuntimeSession session) { var active=session.GameplayState.ActiveDialogue; if(active is not { Open:true }) return null; var d=package.Game.Dialogues.FirstOrDefault(x=>x.Id==active.DialogueId); var n=d?.Nodes.FirstOrDefault(x=>x.Id==active.CurrentNodeId); return new GeneratedCampaignDialogue { Open=true, Title=d?.Title??"Разговор", Speaker=n?.SpeakerId??"Собеседник", Text=n?.Text??string.Empty}; }
    private static string HumanResource(GamePackageDefinition p,string id)=>p.Game.Resources.FirstOrDefault(x=>x.Id==id)?.Name??"Ресурс";
    private static string Hash(UnifiedRuntimeSession s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(s)))).ToLowerInvariant();
    private static string StatusTitle(GeneratedCampaignSessionStatus status) => status switch { GeneratedCampaignSessionStatus.NO_PROJECT=>"Проект не открыт", GeneratedCampaignSessionStatus.PROJECT_NOT_GENERATED=>"Кампания недоступна", GeneratedCampaignSessionStatus.PROJECT_NOT_READY=>"Кампания не готова", GeneratedCampaignSessionStatus.READY=>"Готово к игре", GeneratedCampaignSessionStatus.ACTIVE=>"Игра", GeneratedCampaignSessionStatus.STALE_PROJECT=>"Проект изменён", GeneratedCampaignSessionStatus.SAVE_MIGRATION_REQUIRED=>"Требуется перенос сохранения", _=>"Ошибка кампании" };
}
