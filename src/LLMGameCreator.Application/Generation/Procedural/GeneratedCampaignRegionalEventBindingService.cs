using System.Text;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedCampaignRegionalEventBindingService
{
    public GeneratedCampaignRegionalEventBindingResult Bind(
        GamePackageDefinition relationshipOverlayPackage,
        GeneratedCampaignRelationshipOverlayDocument relationshipOverlay)
    {
        ArgumentNullException.ThrowIfNull(relationshipOverlayPackage);
        ArgumentNullException.ThrowIfNull(relationshipOverlay);
        if (!relationshipOverlay.Passed)
            return Failed(relationshipOverlay.Diagnostics.Count == 0
                ? ["generated_regional_event.relationship_overlay_invalid"]
                : relationshipOverlay.Diagnostics);

        var diagnostics = new List<string>();
        var drafts = new List<EventDraft>();
        foreach (var relationship in relationshipOverlay.Bindings
                     .OrderBy(item => item.RelationshipId,
                         StringComparer.Ordinal))
        {
            if (relationship.Branches.Contains(
                    GeneratedCampaignRelationshipBranch.SUPPORT))
            {
                if (relationship.QuestArc.Count == 0)
                {
                    diagnostics.Add(
                        "generated_regional_event.support_arc_missing");
                }
                else
                {
                    var finalStep = relationship.QuestArc
                        .OrderBy(item => item.Order).Last();
                    var quest = relationshipOverlayPackage.Game.Quests
                        .SingleOrDefault(item =>
                            string.Equals(item.Id, finalStep.QuestId,
                                StringComparison.Ordinal));
                    var rewards = quest?.Rewards.Where(item =>
                            (string.Equals(item.Kind, "reputation",
                                 StringComparison.OrdinalIgnoreCase)
                             || string.Equals(item.Kind,
                                 "faction_reputation",
                                 StringComparison.OrdinalIgnoreCase))
                            && string.Equals(item.Id,
                                relationship.FactionId,
                                StringComparison.Ordinal)
                            && item.Amount > 0)
                        .ToList() ?? [];
                    if (quest is null || rewards.Count != 1)
                    {
                        diagnostics.Add(
                            "generated_regional_event.support_reward_invalid");
                    }
                    else
                    {
                        drafts.Add(Draft(relationship,
                            GeneratedCampaignRegionalEventKind
                                .SUPPORT_GRATITUDE,
                            finalStep.RegionId, finalStep.QuestId,
                            GeneratedCampaignChoiceCanonical.Hash(new
                            {
                                finalStep.QuestId,
                                Reward = rewards[0]
                            }),
                            rewards[0].Amount));
                    }
                }
            }

            if (relationship.Branches.Contains(
                    GeneratedCampaignRelationshipBranch.CHALLENGE))
            {
                if (string.IsNullOrWhiteSpace(
                        relationship.ChallengeEncounterId)
                    || relationshipOverlayPackage.Game.Encounters.Count(
                        item => string.Equals(item.Id,
                            relationship.ChallengeEncounterId,
                            StringComparison.Ordinal)) != 1)
                    diagnostics.Add(
                        "generated_regional_event.challenge_encounter_missing");
                else
                    drafts.Add(Draft(relationship,
                        GeneratedCampaignRegionalEventKind
                            .CHALLENGE_AFTERMATH,
                        relationship.RegionId, string.Empty,
                        string.Empty, 0));
            }

            if (relationship.Branches.Contains(
                    GeneratedCampaignRelationshipBranch.REFUSE))
                drafts.Add(Draft(relationship,
                    GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT,
                    relationship.RegionId, string.Empty, string.Empty, 0));
        }

        var occupied = new Dictionary<string, HashSet<(int X, int Y)>>(
            StringComparer.Ordinal);
        var bindings = new List<GeneratedCampaignRegionalEventBinding>();
        foreach (var draft in drafts.OrderBy(item => item.RegionId,
                     StringComparer.Ordinal)
                 .ThenBy(item => item.Relationship.RelationshipId,
                     StringComparer.Ordinal)
                 .ThenBy(item => item.Kind))
        {
            var map = ResolveMap(relationshipOverlayPackage, draft,
                diagnostics);
            if (map is null)
                continue;
            if (!occupied.TryGetValue(map.Id, out var reserved))
            {
                reserved = [];
                occupied[map.Id] = reserved;
            }
            var placement = Place(relationshipOverlayPackage, map,
                draft.Relationship, draft.RegionId, reserved, diagnostics);
            if (placement is null)
                continue;
            reserved.Add((placement.X, placement.Y));
            var prerequisite = Prerequisite(draft);
            var eventId = EventId(draft.Relationship.RelationshipId,
                draft.Kind);
            bindings.Add(new GeneratedCampaignRegionalEventBinding
            {
                RegionalEventId = eventId,
                EventKind = draft.Kind,
                RelationshipId =
                    draft.Relationship.RelationshipId,
                RelationshipBranch = Branch(draft.Kind),
                ActorSeedId = draft.Relationship.ActorSeedId,
                ActorEntityId = draft.Relationship.ActorEntityId,
                FactionId = draft.Relationship.FactionId,
                RegionId = placement.RegionId,
                MapId = placement.MapId,
                EntityPrototypeId = eventId + "/prototype",
                MapEntityId = eventId + "/entity",
                InteractionId = eventId + "/interaction",
                DialogueId = eventId,
                ResolutionFlagId = eventId,
                Prerequisite = prerequisite,
                Placement = placement,
                SourceQuestId = draft.SourceQuestId,
                SourceQuestRewardFingerprint =
                    draft.RewardFingerprint,
                ResolutionReputationDelta =
                    draft.ResolutionReputationDelta
            });
        }

        if (bindings.Select(item => item.RegionalEventId)
            .Distinct(StringComparer.Ordinal).Count() != bindings.Count)
            diagnostics.Add("generated_regional_event.identity_invalid");
        if (bindings.Any(item =>
                item.RegionalEventId != item.DialogueId
                || item.ResolutionFlagId != item.DialogueId))
            diagnostics.Add("generated_regional_event.identity_invalid");
        if (bindings.GroupBy(item =>
                    (item.MapId, item.Placement.X, item.Placement.Y))
                .Any(group => group.Count() > 1))
            diagnostics.Add(
                "generated_regional_event.placement_collision");

        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignRegionalEventBindingResult
        {
            Passed = diagnostics.Count == 0,
            Bindings = bindings.OrderBy(item => item.RegionId,
                    StringComparer.Ordinal)
                .ThenBy(item => item.MapId, StringComparer.Ordinal)
                .ThenBy(item => item.RelationshipId,
                    StringComparer.Ordinal)
                .ThenBy(item => item.EventKind)
                .ThenBy(item => item.DialogueId,
                    StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
        };
    }

    private static EventDraft Draft(
        GeneratedCampaignRelationshipBinding relationship,
        GeneratedCampaignRegionalEventKind kind,
        string regionId,
        string sourceQuestId,
        string rewardFingerprint,
        double resolutionReputationDelta) => new(
        relationship,
        kind,
        string.IsNullOrWhiteSpace(regionId)
            ? relationship.RegionId
            : regionId,
        sourceQuestId,
        rewardFingerprint,
        resolutionReputationDelta);

    private static GeneratedCampaignRegionalEventPrerequisite Prerequisite(
        EventDraft draft)
    {
        var branch = Branch(draft.Kind);
        var completed = branch ==
                        GeneratedCampaignRelationshipBranch.SUPPORT
            ? draft.Relationship.QuestArc
                .OrderBy(item => item.Order)
                .Select(item => item.QuestId).ToList()
            : [];
        var challengeVictoryFlagId = branch ==
                                     GeneratedCampaignRelationshipBranch
                                         .CHALLENGE
            ? draft.Relationship.RelationshipId + "/challenge-victory"
            : string.Empty;
        var value = new GeneratedCampaignRegionalEventPrerequisite
        {
            DecisionFlagId = draft.Relationship.DecisionFlagId,
            DecisionFlagValue = branch.ToString(),
            CompletedQuestIds = completed,
            ChallengeEncounterId = branch ==
                                   GeneratedCampaignRelationshipBranch
                                       .CHALLENGE
                ? draft.Relationship.ChallengeEncounterId
                : string.Empty,
            ChallengeVictoryFlagId = challengeVictoryFlagId
        };
        return value with
        {
            Fingerprint = GeneratedCampaignChoiceCanonical.Hash(new
            {
                value.DecisionFlagId,
                value.DecisionFlagValue,
                value.CompletedQuestIds,
                value.ChallengeEncounterId,
                value.ChallengeVictoryFlagId
            })
        };
    }

    private static MapDefinition? ResolveMap(
        GamePackageDefinition package,
        EventDraft draft,
        ICollection<string> diagnostics)
    {
        var region = package.GeneratedContent.Regions
            .SingleOrDefault(item =>
                string.Equals(item.SourceId, draft.RegionId,
                    StringComparison.Ordinal)
                || string.Equals(item.SourceId,
                    "generated/" + draft.RegionId,
                    StringComparison.Ordinal));
        var regionMaps = region is null
            ? []
            : package.Game.Maps.Where(item =>
                    region.SceneIds.Contains(item.Id,
                        StringComparer.Ordinal))
                .ToList();
        if (regionMaps.Count == 1)
            return regionMaps[0];

        var expectedId = "map/" + IdSegment(draft.RegionId);
        var exact = package.Game.Maps.Where(item =>
            string.Equals(item.Id, expectedId,
                StringComparison.Ordinal)).ToList();
        if (exact.Count == 1)
            return exact[0];

        var actorMaps = package.Game.Maps.Where(map =>
            map.Entities.Count(entity => string.Equals(entity.Id,
                draft.Relationship.ActorEntityId,
                StringComparison.Ordinal)) == 1).ToList();
        if (actorMaps.Count == 1
            && (draft.Kind !=
                GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE
                || string.Equals(draft.RegionId,
                    draft.Relationship.RegionId,
                    StringComparison.Ordinal)))
            return actorMaps[0];

        diagnostics.Add(
            "generated_regional_event.region_map_missing");
        return null;
    }

    private static GeneratedCampaignRegionalEventPlacement? Place(
        GamePackageDefinition package,
        MapDefinition map,
        GeneratedCampaignRelationshipBinding relationship,
        string regionId,
        IReadOnlySet<(int X, int Y)> reserved,
        ICollection<string> diagnostics)
    {
        if (map.Width <= 0 || map.Height <= 0)
        {
            diagnostics.Add("generated_regional_event.safe_cell_missing");
            return null;
        }
        var blocked = map.Entities.Select(item =>
                (item.Position.X, item.Position.Y))
            .ToHashSet();
        var start = (map.StartPosition.X, map.StartPosition.Y);
        var actor = map.Entities.SingleOrDefault(item =>
            string.Equals(item.Id, relationship.ActorEntityId,
                StringComparison.Ordinal));
        var anchor = actor is null
            ? start
            : (actor.Position.X, actor.Position.Y);
        var anchorKind = actor is null ? "MAP_ENTRY" : "RELATIONSHIP_ACTOR";
        var interactableCells = map.Entities.Where(entity =>
                Components(package, entity).Any(component =>
                    string.Equals(component.Type, "interactable",
                        StringComparison.OrdinalIgnoreCase)))
            .Select(entity =>
                (entity.Position.X, entity.Position.Y))
            .ToHashSet();
        var reachable = Distances(package, map, start, blocked,
            allowBlockedStart: true);
        var anchorDistances = Distances(package, map, anchor, blocked,
            allowBlockedStart: true);
        var candidates = new List<(int X, int Y, int Distance)>();
        for (var y = 0; y < map.Height; y++)
        for (var x = 0; x < map.Width; x++)
        {
            var cell = (x, y);
            if (cell == start || blocked.Contains(cell)
                              || reserved.Contains(cell)
                              || !Walkable(package, map, x, y)
                              || !reachable.ContainsKey(cell))
                continue;
            if (!Neighbours(x, y).Any(next =>
                    reachable.ContainsKey(next)
                    && !blocked.Contains(next)
                    && next != cell
                    && !interactableCells.Any(existing =>
                        Math.Abs(existing.Item1 - next.X)
                        + Math.Abs(existing.Item2 - next.Y) == 1)))
                continue;
            candidates.Add((x, y,
                anchorDistances.GetValueOrDefault(cell,
                    int.MaxValue)));
        }
        var selected = candidates
            .OrderBy(item => item.Distance)
            .ThenBy(item => item.Y)
            .ThenBy(item => item.X)
            .FirstOrDefault();
        if (candidates.Count == 0)
        {
            diagnostics.Add("generated_regional_event.safe_cell_missing");
            return null;
        }
        if (selected.Distance == int.MaxValue)
        {
            diagnostics.Add(
                "generated_regional_event.cell_not_reachable");
            return null;
        }
        return new GeneratedCampaignRegionalEventPlacement
        {
            RegionId = regionId,
            MapId = map.Id,
            X = selected.X,
            Y = selected.Y,
            ReachableDistance = selected.Distance,
            AnchorKind = anchorKind,
            AnchorX = anchor.Item1,
            AnchorY = anchor.Item2,
            Walkable = true,
            Reachable = true,
            Safe = true
        };
    }

    private static Dictionary<(int X, int Y), int> Distances(
        GamePackageDefinition package,
        MapDefinition map,
        (int X, int Y) start,
        IReadOnlySet<(int X, int Y)> blocked,
        bool allowBlockedStart)
    {
        var result = new Dictionary<(int X, int Y), int>();
        if (!InBounds(map, start.Item1, start.Item2)
            || !Walkable(package, map, start.Item1, start.Item2)
            || blocked.Contains(start) && !allowBlockedStart)
            return result;
        var queue = new Queue<(int X, int Y)>();
        result[start] = 0;
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in Neighbours(current.X, current.Y)
                         .OrderBy(item => item.Y)
                         .ThenBy(item => item.X))
            {
                if (!InBounds(map, next.X, next.Y)
                    || !Walkable(package, map, next.X, next.Y)
                    || blocked.Contains(next)
                    || result.ContainsKey(next))
                    continue;
                result[next] = result[current] + 1;
                queue.Enqueue(next);
            }
        }
        return result;
    }

    private static bool Walkable(
        GamePackageDefinition package,
        MapDefinition map,
        int x,
        int y)
    {
        if (!InBounds(map, x, y))
            return false;
        var tileId = map.Tiles.LastOrDefault(item =>
            item.X == x && item.Y == y)?.TileId ?? map.DefaultTileId;
        return package.Game.TilePrototypes.SingleOrDefault(item =>
            string.Equals(item.Id, tileId,
                StringComparison.Ordinal))?.Walkable == true;
    }

    private static bool InBounds(MapDefinition map, int x, int y) =>
        x >= 0 && y >= 0 && x < map.Width && y < map.Height;

    private static IEnumerable<(int X, int Y)> Neighbours(int x, int y)
    {
        yield return (x, y - 1);
        yield return (x - 1, y);
        yield return (x + 1, y);
        yield return (x, y + 1);
    }

    private static IEnumerable<ComponentDefinition> Components(
        GamePackageDefinition package,
        EntityInstanceDefinition entity) =>
        entity.Components.Concat(package.Game.EntityPrototypes
            .SingleOrDefault(item => item.Id == entity.PrototypeId)
            ?.Components ?? []);

    private static GeneratedCampaignRelationshipBranch Branch(
        GeneratedCampaignRegionalEventKind kind) => kind switch
    {
        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE =>
            GeneratedCampaignRelationshipBranch.SUPPORT,
        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH =>
            GeneratedCampaignRelationshipBranch.CHALLENGE,
        _ => GeneratedCampaignRelationshipBranch.REFUSE
    };

    private static string EventId(
        string relationshipId,
        GeneratedCampaignRegionalEventKind kind) =>
        relationshipId + "/regional-event/"
        + kind.ToString().ToLowerInvariant().Replace('_', '-');

    private static string IdSegment(string id)
    {
        var normalized = id.Replace('/', '_').Trim('_').ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
            builder.Append(character is >= 'a' and <= 'z'
                or >= '0' and <= '9' or '_' or '-'
                ? character
                : '_');
        var segment = builder.ToString();
        while (segment.Contains("__", StringComparison.Ordinal))
            segment = segment.Replace("__", "_",
                StringComparison.Ordinal);
        return string.IsNullOrWhiteSpace(segment)
            ? "generated"
            : segment;
    }

    private static GeneratedCampaignRegionalEventBindingResult Failed(
        IReadOnlyList<string> diagnostics) => new()
    {
        Diagnostics = diagnostics
    };

    private sealed record EventDraft(
        GeneratedCampaignRelationshipBinding Relationship,
        GeneratedCampaignRegionalEventKind Kind,
        string RegionId,
        string SourceQuestId,
        string RewardFingerprint,
        double ResolutionReputationDelta);
}
