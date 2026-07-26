using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignRegionalEventProjectionService
{
    public IReadOnlyList<GeneratedCampaignRegionalEventRow> Project(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GeneratedCampaignRegionalEventOverlayDocument overlay)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(overlay);
        return overlay.Bindings.Select(binding =>
        {
            var dialogue = package.Game.Dialogues.Single(item =>
                item.Id == binding.DialogueId);
            var map = package.Game.Maps.Single(item =>
                item.Id == binding.MapId);
            var status =
                GameProjectGeneratedCampaignRegionalEventQualificationService
                    .Status(binding, session);
            return new GeneratedCampaignRegionalEventRow
            {
                Title = dialogue.Title,
                KindTitle = KindTitle(binding.EventKind),
                RegionTitle = RegionTitle(package, binding.RegionId,
                    map.Name),
                MapTitle = map.Name,
                Status = status,
                StatusTitle = StatusTitle(status),
                NextAction = NextAction(status,
                    session.MapState.CurrentMapId == binding.MapId,
                    map.Name),
                OnCurrentMap =
                    session.MapState.CurrentMapId == binding.MapId,
                X = binding.Placement.X,
                Y = binding.Placement.Y
            };
        }).OrderBy(item => item.Status)
            .ThenBy(item => item.RegionTitle, StringComparer.Ordinal)
            .ThenBy(item => item.Title, StringComparer.Ordinal)
            .ToList();
    }

    private static string KindTitle(
        GeneratedCampaignRegionalEventKind kind) => kind switch
    {
        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE =>
            "Благодарность за поддержку",
        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH =>
            "Последствия победы",
        _ => "Последствия отказа"
    };

    private static string StatusTitle(
        GeneratedCampaignRegionalEventStatus status) => status switch
    {
        GeneratedCampaignRegionalEventStatus.LOCKED =>
            "Ещё не произошло",
        GeneratedCampaignRegionalEventStatus.AVAILABLE =>
            "Можно завершить",
        _ => "Завершено"
    };

    private static string NextAction(
        GeneratedCampaignRegionalEventStatus status,
        bool currentMap,
        string mapTitle) => status switch
    {
        GeneratedCampaignRegionalEventStatus.LOCKED =>
            "Сначала завершите связанное решение.",
        GeneratedCampaignRegionalEventStatus.RESOLVED =>
            "Последствия уже учтены.",
        _ when currentMap =>
            "Найдите отметку события на текущей карте и поговорите.",
        _ => "Отправляйтесь на карту «" + mapTitle + "»."
    };

    private static string RegionTitle(
        GamePackageDefinition package,
        string regionId,
        string fallback)
    {
        var region = package.GeneratedContent.Regions.SingleOrDefault(item =>
            item.SourceId == regionId
            || item.SceneIds.Contains(regionId,
                StringComparer.Ordinal));
        return string.IsNullOrWhiteSpace(region?.Title)
            ? fallback
            : region.Title;
    }
}
