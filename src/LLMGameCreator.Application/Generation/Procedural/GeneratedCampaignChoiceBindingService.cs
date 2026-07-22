using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedCampaignChoiceBindingService
{
    public GeneratedCampaignChoiceBindingResult Bind(
        SeededGeneratedProjectSourceValidationResult strictSource,
        GamePackageDefinition preChoicePackage)
    {
        ArgumentNullException.ThrowIfNull(strictSource);
        ArgumentNullException.ThrowIfNull(preChoicePackage);
        if (strictSource is not
            {
                Present: true,
                Passed: true,
                RegeneratedPlan: not null,
                Overlay: not null,
                GeneratedMvpPackage: not null
            })
            return Failed("generated_choice.source_invalid");

        var plan = strictSource.RegeneratedPlan;
        var generatedDialogueRecords = strictSource.Overlay.GeneratedRecords
            .Where(item => string.Equals(item.CollectionPath, "game.dialogues", StringComparison.Ordinal))
            .ToList();
        var diagnostics = new List<string>();
        var bindings = new List<GeneratedCampaignChoiceBinding>();
        foreach (var mvpDialogue in strictSource.GeneratedMvpPackage.Game.Dialogues
                     .Where(item => generatedDialogueRecords.Any(record => record.RecordId == item.Id))
                     .OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            if (generatedDialogueRecords.Count(item => item.RecordId == mvpDialogue.Id) != 1
                || !mvpDialogue.Metadata.TryGetValue("sourceActorSeedId", out var mappedActorId))
            {
                diagnostics.Add("generated_choice.dialogue_provenance_invalid");
                continue;
            }
            var actors = plan.ActorSeeds.Where(item => SourceMatches(item.ActorSeedId, mappedActorId)).ToList();
            if (actors.Count != 1)
            {
                diagnostics.Add("generated_choice.actor_provenance_invalid");
                continue;
            }
            var actor = actors[0];
            var dialogues = preChoicePackage.Game.Dialogues.Where(item => item.Id == mvpDialogue.Id
                && item.Metadata.TryGetValue("sourceActorSeedId", out var source)
                && string.Equals(source, mappedActorId, StringComparison.Ordinal)).ToList();
            if (dialogues.Count != 1)
            {
                diagnostics.Add(dialogues.Count == 0
                    ? "generated_choice.dialogue_mapping_missing" : "generated_choice.dialogue_mapping_duplicate");
                continue;
            }
            var dialogue = dialogues[0];
            var mvpEntities = strictSource.GeneratedMvpPackage.Game.Maps.SelectMany(map => map.Entities).Where(entity =>
                entity.Components.Any(component => string.Equals(component.Type, "interactable", StringComparison.OrdinalIgnoreCase)
                    && component.Args.TryGetValue("dialogueId", out var dialogueId)
                    && string.Equals(dialogueId, dialogue.Id, StringComparison.Ordinal))).ToList();
            var entities = mvpEntities.Count == 1
                ? preChoicePackage.Game.Maps.SelectMany(map => map.Entities).Where(entity => entity.Id == mvpEntities[0].Id
                    && entity.Components.Any(component => string.Equals(component.Type, "interactable", StringComparison.OrdinalIgnoreCase)
                        && component.Args.TryGetValue("dialogueId", out var dialogueId)
                        && string.Equals(dialogueId, dialogue.Id, StringComparison.Ordinal))).ToList()
                : [];
            if (entities.Count != 1)
            {
                diagnostics.Add("generated_choice.actor_entity_missing");
                continue;
            }
            var interactable = entities[0].Components.Single(component =>
                string.Equals(component.Type, "interactable", StringComparison.OrdinalIgnoreCase)
                && component.Args.TryGetValue("dialogueId", out var dialogueId)
                && string.Equals(dialogueId, dialogue.Id, StringComparison.Ordinal));
            if (!interactable.Args.TryGetValue("interactionId", out var interactionId)
                || string.IsNullOrWhiteSpace(interactionId))
            {
                diagnostics.Add("generated_choice.interaction_missing");
                continue;
            }
            var mvpInteractions = strictSource.GeneratedMvpPackage.Game.Interactions.Where(item => item.Id == interactionId
                    && item.Metadata.TryGetValue("sourceActorSeedId", out var source)
                    && string.Equals(source, mappedActorId, StringComparison.Ordinal)).ToList();
            if (mvpInteractions.Count != 1 || preChoicePackage.Game.Interactions.Count(item => item.Id == interactionId
                    && item.Metadata.TryGetValue("sourceActorSeedId", out var source)
                    && string.Equals(source, mappedActorId, StringComparison.Ordinal)) != 1)
            {
                diagnostics.Add("generated_choice.interaction_missing");
                continue;
            }
            var factionIds = strictSource.GeneratedMvpPackage.Game.Factions.Select(item => item.Id)
                .Where(item => SourceMatches(actor.FactionId, item)).ToList();
            if (factionIds.Count != 1 || preChoicePackage.Game.Factions.Count(item => item.Id == factionIds[0]) != 1)
            {
                diagnostics.Add("generated_choice.faction_missing");
                continue;
            }
            var factionId = factionIds[0];
            var questCandidates = ResolveQuestCandidates(plan, preChoicePackage, actor.RegionId, actor.FactionId, factionId);
            if (questCandidates.Count > 1) diagnostics.Add("generated_choice.relationship_ambiguous");
            var quest = questCandidates.Count == 0 ? ((QuestDefinition Quest, double ReputationAmount)?)null
                : questCandidates[0];
            var encounter = ResolveEncounter(plan, preChoicePackage, actor.ActorSeedId, actor.RegionId);
            var branches = new List<GeneratedCampaignChoiceBranch>();
            if (quest is { } questBinding)
            {
                var amount = Math.Abs(questBinding.ReputationAmount);
                branches.Add(Branch(GeneratedCampaignBranchKind.SUPPORT, dialogue, factionId, questBinding.Quest.Id,
                    null, amount, actor.ActorSeedId, preChoicePackage));
                branches.Add(Branch(GeneratedCampaignBranchKind.REFUSE, dialogue, factionId, questBinding.Quest.Id,
                    null, -amount, actor.ActorSeedId, preChoicePackage));
            }
            if (encounter is not null)
                branches.Add(Branch(GeneratedCampaignBranchKind.CHALLENGE, dialogue, factionId, null,
                    encounter.Id, 0, actor.ActorSeedId, preChoicePackage));
            bindings.Add(new GeneratedCampaignChoiceBinding
            {
                ActorSeedId = mappedActorId,
                ActorEntityId = entities[0].Id,
                InteractionId = interactionId,
                DialogueId = dialogue.Id,
                FactionId = factionId,
                RegionId = dialogue.Metadata.GetValueOrDefault("sourceRegionId") ?? string.Empty,
                Branches = branches.OrderBy(item => item.Kind).ToList(),
                Status = branches.Count == 0 ? "NO_BRANCH_RELATIONSHIP" : "BRANCHABLE"
            });
        }
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignChoiceBindingResult
        {
            Passed = diagnostics.Count == 0,
            Bindings = bindings.OrderBy(item => item.ActorSeedId, StringComparer.Ordinal).ToList(),
            Diagnostics = diagnostics
        };
    }

    private static List<(QuestDefinition Quest, double ReputationAmount)> ResolveQuestCandidates(
        ProceduralGeneratedGamePlan plan, GamePackageDefinition package, string regionId, string factionSeedId, string factionId)
    {
        var validSeeds = plan.QuestEventSeeds.Where(seed => string.Equals(seed.RegionId, regionId, StringComparison.Ordinal)
            && string.Equals(seed.SourceFactionId, factionSeedId, StringComparison.Ordinal)).ToList();
        var matches = new List<(QuestDefinition, double)>();
        foreach (var seed in validSeeds)
        {
            var matchingSources = package.Game.Quests
                .Select(item => item.Metadata.GetValueOrDefault("sourceQuestEventSeedId"))
                .Where(item => item is not null && SourceMatches(seed.QuestEventSeedId, item))
                .Distinct(StringComparer.Ordinal).ToList();
            if (matchingSources.Count != 1) continue;
            var source = matchingSources[0];
            var quests = package.Game.Quests.Where(item => item.Metadata.TryGetValue("sourceQuestEventSeedId", out var value)
                && string.Equals(value, source, StringComparison.Ordinal)).ToList();
            if (quests.Count != 1) continue;
            var outputs = quests[0].Rewards.Where(item => (string.Equals(item.Kind, "reputation", StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.Kind, "faction_reputation", StringComparison.OrdinalIgnoreCase))
                && string.Equals(item.Id, factionId, StringComparison.Ordinal) && Math.Abs(item.Amount) > 0).ToList();
            if (outputs.Count == 1) matches.Add((quests[0], outputs[0].Amount));
        }
        return matches.OrderBy(item => item.Item1.Id, StringComparer.Ordinal).ToList();
    }

    private static EncounterDefinition? ResolveEncounter(ProceduralGeneratedGamePlan plan, GamePackageDefinition package,
        string actorSeedId, string regionId)
    {
        var candidates = plan.EncounterSeeds.Where(seed => string.Equals(seed.RegionId, regionId, StringComparison.Ordinal)).ToList();
        var actorSpecific = candidates.Where(seed => seed.ActorSeedIds.Contains(actorSeedId, StringComparer.Ordinal)).ToList();
        var selected = (actorSpecific.Count > 0 ? actorSpecific : candidates)
            .OrderBy(seed => seed.EncounterSeedId, StringComparer.Ordinal).FirstOrDefault();
        if (selected is null) return null;
        var matches = package.Game.Encounters.Where(item => item.Metadata.TryGetValue("sourceEncounterSeedId", out var value)
            && SourceMatches(selected.EncounterSeedId, value)).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    private static GeneratedCampaignChoiceBranch Branch(GeneratedCampaignBranchKind kind, DialogueDefinition dialogue,
        string factionId, string? questId, string? encounterId, double reputation, string actorSeedId, GamePackageDefinition package)
    {
        var actor = dialogue.Title;
        var faction = package.Game.Factions.Single(item => item.Id == factionId).Name;
        var quest = questId is null ? null : package.Game.Quests.Single(item => item.Id == questId).Title;
        var encounter = encounterId is null ? null : package.Game.Encounters.Single(item => item.Id == encounterId).Name;
        var title = kind switch
        {
            GeneratedCampaignBranchKind.SUPPORT => "Поддержать «" + faction + "»",
            GeneratedCampaignBranchKind.CHALLENGE => "Бросить вызов: «" + encounter + "»",
            _ => "Отказаться от «" + faction + "»"
        };
        var description = kind switch
        {
            GeneratedCampaignBranchKind.SUPPORT => "Поддержать " + actor + " и продвинуть задание «" + quest + "».",
            GeneratedCampaignBranchKind.CHALLENGE => "Начать встречу «" + encounter + "».",
            _ => "Отказаться от предложения " + actor + " без изменения задания или встречи."
        };
        return new GeneratedCampaignChoiceBranch
        {
            Kind = kind,
            ChoiceId = "generatedChoice/" + kind.ToString().ToLowerInvariant(),
            FlagValue = kind.ToString(),
            FactionId = factionId,
            QuestId = questId,
            EncounterId = encounterId,
            ReputationAmount = reputation,
            Title = title,
            Description = description
        };
    }

    private static bool SourceMatches(string sourceId, string mappedId) =>
        string.Equals(sourceId, mappedId, StringComparison.Ordinal)
        || (!sourceId.StartsWith("generated/", StringComparison.Ordinal)
            && !sourceId.StartsWith("seeded_generated_project/", StringComparison.Ordinal)
            && string.Equals("generated/" + sourceId, mappedId, StringComparison.Ordinal));
    private static GeneratedCampaignChoiceBindingResult Failed(string diagnostic) => new() { Diagnostics = [diagnostic] };
}
