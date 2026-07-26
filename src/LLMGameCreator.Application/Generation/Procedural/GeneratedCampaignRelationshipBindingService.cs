using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedCampaignRelationshipBindingService
{
    public GeneratedCampaignRelationshipBindingResult Bind(
        SeededGeneratedProjectSourceValidationResult strictSource,
        GamePackageDefinition choiceOverlayPackage,
        GeneratedCampaignChoiceBindingResult? choiceBindings = null)
    {
        ArgumentNullException.ThrowIfNull(strictSource);
        ArgumentNullException.ThrowIfNull(choiceOverlayPackage);
        if (strictSource is not
            {
                Present: true,
                Passed: true,
                RegeneratedPlan: not null,
                Overlay: not null,
                GeneratedMvpPackage: not null
            })
            return Failed("generated_relationship.source_invalid");

        choiceBindings ??= new GeneratedCampaignChoiceBindingService().Bind(
            strictSource, choiceOverlayPackage);
        if (!choiceBindings.Passed)
            return new GeneratedCampaignRelationshipBindingResult
            {
                Diagnostics = choiceBindings.Diagnostics
            };

        var plan = strictSource.RegeneratedPlan;
        var diagnostics = new List<string>();
        var actors = choiceBindings.Bindings
            .Select(choice => ResolveActor(plan, choice, diagnostics))
            .Where(item => item is not null)
            .Select(item => item!)
            .OrderBy(item => item.Actor.ActorSeedId, StringComparer.Ordinal)
            .ToList();
        if (actors.Select(item => item.Actor.ActorSeedId).Distinct(StringComparer.Ordinal)
                .Count() != actors.Count)
            diagnostics.Add("generated_relationship.actor_binding_ambiguous");

        var questCandidates = ResolveQuestCandidates(
            strictSource, choiceOverlayPackage, diagnostics);
        var assignments = actors.ToDictionary(
            item => item.Actor.ActorSeedId,
            _ => new List<QuestCandidate>(), StringComparer.Ordinal);
        foreach (var candidate in questCandidates
                     .OrderBy(item => item.Seed.QuestEventSeedId,
                         StringComparer.Ordinal))
        {
            var eligible = actors.Where(item => SourceMatches(
                    item.Actor.FactionId, candidate.Seed.SourceFactionId))
                .Select(item => new ActorCandidate(
                    item,
                    ShortestDistance(plan.World, item.Actor.RegionId,
                        candidate.Seed.RegionId)))
                .Where(item => item.Distance is not null)
                .ToList();
            if (eligible.Count == 0) continue;

            var actorSpecific = eligible.Where(item =>
                    candidate.EncounterSeed?.ActorSeedIds.Contains(
                        item.Actor.Actor.ActorSeedId, StringComparer.Ordinal)
                    == true)
                .ToList();
            var sameRegion = eligible.Where(item => string.Equals(
                    item.Actor.Actor.RegionId, candidate.Seed.RegionId,
                    StringComparison.Ordinal))
                .ToList();
            var selectedPool = actorSpecific.Count > 0
                ? actorSpecific
                : sameRegion.Count > 0
                    ? sameRegion
                    : eligible;
            var selected = selectedPool
                .OrderBy(item => item.Actor.Actor.ActorSeedId,
                    StringComparer.Ordinal)
                .First();
            if (selectedPool.Count(item => string.Equals(
                    item.Actor.Actor.ActorSeedId,
                    selected.Actor.Actor.ActorSeedId,
                    StringComparison.Ordinal)) != 1)
            {
                diagnostics.Add(
                    "generated_relationship.quest_assignment_ambiguous");
                continue;
            }
            assignments[selected.Actor.Actor.ActorSeedId].Add(candidate with
            {
                RegionDistance = selected.Distance!.Value
            });
        }

        var bindings = new List<GeneratedCampaignRelationshipBinding>();
        foreach (var actor in actors)
        {
            var choice = actor.Choice;
            var dialogue = choiceOverlayPackage.Game.Dialogues
                .SingleOrDefault(item => string.Equals(item.Id,
                    choice.DialogueId, StringComparison.Ordinal));
            if (dialogue is null)
            {
                diagnostics.Add("generated_relationship.dialogue_missing");
                continue;
            }
            var arcCandidates = assignments[actor.Actor.ActorSeedId]
                .OrderBy(item => item.RegionDistance)
                .ThenBy(item => item.EncounterSeed?.EncounterSeedId
                                ?? string.Empty,
                    StringComparer.Ordinal)
                .ThenBy(item => item.Seed.QuestEventSeedId,
                    StringComparer.Ordinal)
                .ToList();
            var arc = arcCandidates.Select((item, index) =>
                new GeneratedCampaignQuestArcStep
                {
                    Order = index,
                    RegionDistance = item.RegionDistance,
                    QuestId = item.Quest.Id,
                    QuestSourceId = item.Seed.QuestEventSeedId,
                    RegionId = item.Seed.RegionId,
                    TargetEncounterId = item.Encounter?.Id ?? string.Empty,
                    TargetEncounterSourceId =
                        item.EncounterSeed?.EncounterSeedId ?? string.Empty,
                    TargetItemId = item.Item.Id,
                    TargetItemSourceId = item.ItemSeedId,
                    ReputationReward = item.ReputationAmount
                }).ToList();
            var challenge = choice.Branches.SingleOrDefault(item =>
                item.Kind == GeneratedCampaignBranchKind.CHALLENGE);
            var branchKinds = new List<GeneratedCampaignRelationshipBranch>();
            if (arc.Count > 0)
            {
                branchKinds.Add(GeneratedCampaignRelationshipBranch.SUPPORT);
                branchKinds.Add(GeneratedCampaignRelationshipBranch.REFUSE);
            }
            if (challenge is not null)
                branchKinds.Add(GeneratedCampaignRelationshipBranch.CHALLENGE);
            var supportAmount = arc.Count == 0
                ? 0
                : Math.Abs(arc[0].ReputationReward);
            bindings.Add(new GeneratedCampaignRelationshipBinding
            {
                RelationshipId = dialogue.Id,
                DialogueId = dialogue.Id,
                DecisionFlagId = dialogue.Id,
                ActorSeedId = actor.Actor.ActorSeedId,
                ActorEntityId = choice.ActorEntityId,
                InteractionId = choice.InteractionId,
                FactionId = choice.FactionId,
                RegionId = actor.Actor.RegionId,
                ChallengeEncounterId = challenge?.EncounterId ?? string.Empty,
                SupportReputationAmount = supportAmount,
                RefuseReputationAmount = -supportAmount,
                Branches = branchKinds.OrderBy(item => item).ToList(),
                QuestArc = arc
            });
        }

        var assignedQuestIds = bindings.SelectMany(item => item.QuestArc)
            .Select(item => item.QuestId).ToList();
        if (assignedQuestIds.Distinct(StringComparer.Ordinal).Count()
            != assignedQuestIds.Count)
            diagnostics.Add("generated_relationship.quest_assigned_multiple");
        if (bindings.Any(item => item.RelationshipId != item.DialogueId
                                 || item.DecisionFlagId != item.DialogueId))
            diagnostics.Add("generated_relationship.identity_invalid");

        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignRelationshipBindingResult
        {
            Passed = diagnostics.Count == 0,
            Bindings = bindings.OrderBy(item => item.ActorSeedId,
                    StringComparer.Ordinal)
                .ThenBy(item => item.RelationshipId, StringComparer.Ordinal)
                .ToList(),
            Diagnostics = diagnostics
        };
    }

    private static ActorBinding? ResolveActor(
        ProceduralGeneratedGamePlan plan,
        GeneratedCampaignChoiceBinding choice,
        ICollection<string> diagnostics)
    {
        var matches = plan.ActorSeeds.Where(item => SourceMatches(
                item.ActorSeedId, choice.ActorSeedId))
            .ToList();
        if (matches.Count != 1)
        {
            diagnostics.Add("generated_relationship.actor_provenance_invalid");
            return null;
        }
        return new ActorBinding(matches[0], choice);
    }

    private static IReadOnlyList<QuestCandidate> ResolveQuestCandidates(
        SeededGeneratedProjectSourceValidationResult strictSource,
        GamePackageDefinition package,
        ICollection<string> diagnostics)
    {
        var plan = strictSource.RegeneratedPlan!;
        var generatedQuestIds = strictSource.Overlay!.GeneratedRecords
            .Where(item => string.Equals(item.CollectionPath, "game.quests",
                StringComparison.Ordinal))
            .Select(item => item.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var generatedMvpQuestIds = strictSource.GeneratedMvpPackage!.Game.Quests
            .Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var result = new List<QuestCandidate>();
        foreach (var seed in plan.QuestEventSeeds.OrderBy(item =>
                     item.QuestEventSeedId, StringComparer.Ordinal))
        {
            var quests = package.Game.Quests.Where(item =>
                    generatedQuestIds.Contains(item.Id)
                    && generatedMvpQuestIds.Contains(item.Id)
                    && item.Metadata.TryGetValue("sourceQuestEventSeedId",
                        out var source)
                    && SourceMatches(seed.QuestEventSeedId, source))
                .ToList();
            var encounterSeeds = plan.EncounterSeeds.Where(item =>
                SourceMatches(item.EncounterSeedId,
                    seed.TargetEncounterSeedId)).ToList();
            var encounters = encounterSeeds.Count == 1
                ? package.Game.Encounters.Where(item =>
                        item.Metadata.TryGetValue("sourceEncounterSeedId",
                            out var source)
                        && SourceMatches(encounterSeeds[0].EncounterSeedId,
                            source))
                    .ToList()
                : [];
            var items = package.Game.Items.Where(item =>
                    item.Metadata.TryGetValue("sourceItemSeedId", out var source)
                    && SourceMatches(seed.RequiredItemSeedId, source))
                .ToList();
            if (quests.Count != 1 || items.Count != 1)
            {
                diagnostics.Add(
                    "generated_relationship.quest_provenance_invalid:"
                    + seed.QuestEventSeedId);
                continue;
            }
            var quest = quests[0];
            var item = items[0];
            var encounterObjective = quest.Objectives.SingleOrDefault(item =>
                string.Equals(item.Kind, "complete_encounter",
                    StringComparison.OrdinalIgnoreCase));
            var itemObjective = quest.Objectives.SingleOrDefault(item =>
                string.Equals(item.Kind, "has_item",
                    StringComparison.OrdinalIgnoreCase));
            var reputation = quest.Rewards.Where(item =>
                    (string.Equals(item.Kind, "reputation",
                         StringComparison.OrdinalIgnoreCase)
                     || string.Equals(item.Kind, "faction_reputation",
                         StringComparison.OrdinalIgnoreCase))
                    && SourceMatches(seed.SourceFactionId, item.Id)
                    && Math.Abs(item.Amount) > 0)
                .ToList();
            var encounterValid = encounterObjective is null
                ? encounterSeeds.Count == 0 && encounters.Count == 0
                : encounterSeeds.Count == 1 && encounters.Count == 1
                  && encounterObjective.TargetId == encounters[0].Id;
            if (encounterObjective is not null
                && plan.EncounterSeeds.Count == 0
                && encounterSeeds.Count == 0)
                continue;
            if (!encounterValid
                || itemObjective?.TargetId != item.Id
                || reputation.Count != 1)
            {
                diagnostics.Add(
                    "generated_relationship.quest_reference_invalid:"
                    + seed.QuestEventSeedId);
                continue;
            }
            result.Add(new QuestCandidate(seed, quest,
                encounterSeeds.SingleOrDefault(),
                encounters.SingleOrDefault(), seed.RequiredItemSeedId, item,
                reputation[0].Amount, 0));
        }
        return result;
    }

    private static int? ShortestDistance(
        ProceduralWorldPlan world,
        string from,
        string to)
    {
        if (string.Equals(from, to, StringComparison.Ordinal)) return 0;
        var known = world.Regions.Select(item => item.RegionId)
            .ToHashSet(StringComparer.Ordinal);
        if (!known.Contains(from) || !known.Contains(to)) return null;
        var queue = new Queue<(string RegionId, int Distance)>();
        var visited = new HashSet<string>(StringComparer.Ordinal) { from };
        queue.Enqueue((from, 0));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in world.Connections.Where(item =>
                         string.Equals(item.FromRegionId, current.RegionId,
                             StringComparison.Ordinal))
                     .Select(item => item.ToRegionId)
                     .OrderBy(item => item, StringComparer.Ordinal))
            {
                if (!visited.Add(next)) continue;
                if (string.Equals(next, to, StringComparison.Ordinal))
                    return current.Distance + 1;
                queue.Enqueue((next, current.Distance + 1));
            }
        }
        return null;
    }

    private static bool SourceMatches(string sourceId, string mappedId) =>
        string.Equals(sourceId, mappedId, StringComparison.Ordinal)
        || (!sourceId.StartsWith("generated/", StringComparison.Ordinal)
            && !sourceId.StartsWith("seeded_generated_project/",
                StringComparison.Ordinal)
            && string.Equals("generated/" + sourceId, mappedId,
                StringComparison.Ordinal));

    private static GeneratedCampaignRelationshipBindingResult Failed(
        string diagnostic) => new() { Diagnostics = [diagnostic] };

    private sealed record ActorBinding(
        ProceduralActorSeed Actor,
        GeneratedCampaignChoiceBinding Choice);

    private sealed record ActorCandidate(
        ActorBinding Actor,
        int? Distance);

    private sealed record QuestCandidate(
        ProceduralQuestEventSeed Seed,
        QuestDefinition Quest,
        ProceduralEncounterSeed? EncounterSeed,
        EncounterDefinition? Encounter,
        string ItemSeedId,
        ItemDefinition Item,
        double ReputationAmount,
        int RegionDistance);
}
