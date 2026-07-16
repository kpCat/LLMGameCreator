using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedGameplayDefinitionFingerprintService
{
    private static readonly HashSet<string> StructuralIds = new(StringComparer.Ordinal)
    {
        "player",
        "participant/player",
        "global",
        "player_inventory",
        "inventory/player"
    };

    public IReadOnlyList<GeneratedGameplayDefinitionFingerprint> BuildInventory(GamePackageDefinition package)
    {
        ArgumentNullException.ThrowIfNull(package);
        var generated = GeneratedKeys(package);
        var rows = new List<GeneratedGameplayDefinitionFingerprint>();
        Add(rows, generated, "map", package.Game.Maps);
        Add(rows, generated, "item", package.Game.Items);
        Add(rows, generated, "resource", package.Game.Resources);
        Add(rows, generated, "stat", package.Game.Stats);
        Add(rows, generated, "progression", package.Game.Progressions);
        Add(rows, generated, "status", package.Game.Statuses);
        Add(rows, generated, "quest", package.Game.Quests);
        Add(rows, generated, "dialogue", package.Game.Dialogues);
        Add(rows, generated, "faction", package.Game.Factions);
        Add(rows, generated, "encounter", package.Game.Encounters);
        Add(rows, generated, "ability", package.Game.Abilities);
        Add(rows, generated, "interaction", package.Game.Interactions);
        Add(rows, generated, "entity", package.Game.Maps.SelectMany(map => map.Entities));
        Add(rows, generated, "equipment_slot", package.Game.EquipmentSlots);
        return rows.OrderBy(row => row.Kind, StringComparer.Ordinal)
            .ThenBy(row => row.Id, StringComparer.Ordinal).ToList();
    }

    public GeneratedGameplaySessionReferenceInventory CaptureReferences(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        IReadOnlyList<GeneratedGameplayDefinitionFingerprint>? inventory = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(session);
        inventory ??= BuildInventory(package);
        var byKey = inventory.GroupBy(Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var byId = inventory.GroupBy(row => row.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
        var references = new List<GeneratedGameplaySessionReference>();
        var unresolved = new List<GeneratedGameplaySessionReference>();
        var fingerprints = new List<GeneratedGameplayDefinitionFingerprint>();

        void Reference(string kind, string id, string path, bool allowStructural = false)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            var reference = new GeneratedGameplaySessionReference { Kind = kind, Id = id, SourcePath = path };
            references.Add(reference);
            if (allowStructural && IsStructural(id)) return;
            if (!byKey.TryGetValue(kind + "\n" + id, out var matches) || matches.Count != 1)
            {
                unresolved.Add(reference);
                return;
            }
            fingerprints.Add(matches[0]);
        }

        void Scalar(string value, string path)
        {
            if (string.IsNullOrWhiteSpace(value) || !byId.TryGetValue(value, out var matches)) return;
            foreach (var match in matches)
            {
                references.Add(new GeneratedGameplaySessionReference
                {
                    Kind = match.Kind,
                    Id = match.Id,
                    SourcePath = path
                });
                fingerprints.Add(match);
            }
        }

        Reference("map", session.MapState.CurrentMapId, "mapState.currentMapId");
        if (!string.IsNullOrWhiteSpace(session.GameplayState.CurrentMapId))
            Reference("map", session.GameplayState.CurrentMapId, "gameplayState.currentMapId");
        foreach (var pair in session.MapState.Flags)
        {
            Scalar(pair.Key, "mapState.flags.key");
            Scalar(pair.Value, "mapState.flags[" + pair.Key + "]");
        }

        var state = session.GameplayState;
        foreach (var inventoryState in state.Inventories)
        {
            ValidateInventoryId(package, inventoryState.Id, "inventories.id", unresolved);
            ValidateOwner(package, inventoryState.OwnerId, "inventories.ownerId", unresolved);
            foreach (var stack in inventoryState.Stacks)
            {
                Reference("item", stack.ItemId, "inventories.stacks.itemId");
                foreach (var pair in stack.Metadata)
                {
                    Scalar(pair.Key, "inventories.stacks.metadata.key");
                    Scalar(pair.Value, "inventories.stacks.metadata[" + pair.Key + "]");
                }
            }
            foreach (var pair in inventoryState.Metadata)
            {
                Scalar(pair.Key, "inventories.metadata.key");
                Scalar(pair.Value, "inventories.metadata[" + pair.Key + "]");
            }
        }

        foreach (var equipment in state.Equipment)
        {
            ValidateOwner(package, equipment.OwnerId, "equipment.ownerId", unresolved);
            foreach (var slot in equipment.Slots)
            {
                if (package.Game.EquipmentSlots.Any(definition => definition.Id == slot.SlotId))
                    Reference("equipment_slot", slot.SlotId, "equipment.slots.slotId");
                if (!string.IsNullOrWhiteSpace(slot.ItemId))
                    Reference("item", slot.ItemId, "equipment.slots.itemId");
                foreach (var pair in slot.Metadata)
                {
                    Scalar(pair.Key, "equipment.slots.metadata.key");
                    Scalar(pair.Value, "equipment.slots.metadata[" + pair.Key + "]");
                }
            }
            foreach (var pair in equipment.Metadata)
            {
                Scalar(pair.Key, "equipment.metadata.key");
                Scalar(pair.Value, "equipment.metadata[" + pair.Key + "]");
            }
        }

        foreach (var resource in state.Resources)
        {
            Reference("resource", resource.ResourceId, "resources.resourceId");
            ValidateOwner(package, resource.OwnerId, "resources.ownerId", unresolved);
        }
        foreach (var stat in state.Stats) Reference("stat", stat.StatId, "stats.statId");
        foreach (var progression in state.Progressions)
        {
            Reference("progression", progression.ProgressionId, "progressions.progressionId");
            foreach (var pair in progression.Metadata)
            {
                Scalar(pair.Key, "progressions.metadata.key");
                Scalar(pair.Value, "progressions.metadata[" + pair.Key + "]");
            }
        }
        foreach (var flag in state.Flags)
        {
            Scalar(flag.Id, "flags.id");
            Scalar(flag.Value, "flags.value");
        }
        foreach (var status in state.Statuses)
        {
            Reference("status", status.StatusId, "statuses.statusId");
            ValidateTarget(package, state.ActiveEncounter, status.TargetId, "statuses.targetId", unresolved);
            foreach (var pair in status.Metadata)
            {
                Scalar(pair.Key, "statuses.metadata.key");
                Scalar(pair.Value, "statuses.metadata[" + pair.Key + "]");
            }
        }

        if (state.ActiveEncounter is { } encounter)
        {
            Reference("encounter", encounter.EncounterId, "activeEncounter.encounterId");
            var definition = package.Game.Encounters.SingleOrDefault(item => item.Id == encounter.EncounterId);
            foreach (var participant in encounter.Participants)
            {
                if (!IsStructural(participant.Id)
                    && definition?.Participants.All(item => item.Id != participant.Id) != false)
                    unresolved.Add(new GeneratedGameplaySessionReference
                    {
                        Kind = "encounter_participant",
                        Id = participant.Id,
                        SourcePath = "activeEncounter.participants.id"
                    });
                foreach (var stat in participant.Stats)
                    Reference("stat", stat.StatId, "activeEncounter.participants.stats.statId");
                foreach (var resource in participant.Resources)
                    Reference("resource", resource.ResourceId,
                        "activeEncounter.participants.resources.resourceId");
                foreach (var status in participant.Statuses)
                {
                    Reference("status", status.StatusId,
                        "activeEncounter.participants.statuses.statusId");
                    ValidateTarget(package, encounter, status.TargetId,
                        "activeEncounter.participants.statuses.targetId", unresolved);
                }
                foreach (var abilityId in participant.Cooldowns.Keys)
                    Reference("ability", abilityId, "activeEncounter.participants.cooldowns.abilityId");
                ValidateInventoryId(package, participant.InventoryId,
                    "activeEncounter.participants.inventoryId", unresolved);
                foreach (var pair in participant.Metadata)
                {
                    Scalar(pair.Key, "activeEncounter.participants.metadata.key");
                    Scalar(pair.Value, "activeEncounter.participants.metadata[" + pair.Key + "]");
                }
            }
            foreach (var pair in encounter.Metadata)
            {
                Scalar(pair.Key, "activeEncounter.metadata.key");
                Scalar(pair.Value, "activeEncounter.metadata[" + pair.Key + "]");
            }
        }

        foreach (var questId in state.QuestStates.Keys)
            Reference("quest", questId, "questStates.key");
        foreach (var quest in state.Quests)
        {
            Reference("quest", quest.QuestId, "quests.questId");
            var definition = package.Game.Quests.SingleOrDefault(item => item.Id == quest.QuestId);
            foreach (var objective in quest.Objectives)
            {
                var objectiveExists = definition?.Objectives.Any(item => item.Id == objective.ObjectiveId) == true
                                      || definition?.Stages.SelectMany(stage => stage.Objectives)
                                          .Any(item => item.Id == objective.ObjectiveId) == true;
                if (!objectiveExists)
                    unresolved.Add(new GeneratedGameplaySessionReference
                    {
                        Kind = "quest_objective",
                        Id = objective.ObjectiveId,
                        SourcePath = "quests.objectives.objectiveId"
                    });
                if (!string.IsNullOrWhiteSpace(objective.TargetId))
                {
                    if (byId.TryGetValue(objective.TargetId, out var targetMatches))
                        foreach (var target in targetMatches) Reference(target.Kind, target.Id,
                            "quests.objectives.targetId");
                    else if (!IsStructural(objective.TargetId))
                        unresolved.Add(new GeneratedGameplaySessionReference
                        {
                            Kind = "quest_target",
                            Id = objective.TargetId,
                            SourcePath = "quests.objectives.targetId"
                        });
                }
                foreach (var pair in objective.Metadata)
                {
                    Scalar(pair.Key, "quests.objectives.metadata.key");
                    Scalar(pair.Value, "quests.objectives.metadata[" + pair.Key + "]");
                }
            }
            foreach (var pair in quest.Metadata)
            {
                Scalar(pair.Key, "quests.metadata.key");
                Scalar(pair.Value, "quests.metadata[" + pair.Key + "]");
            }
        }

        if (state.ActiveDialogue is { } dialogue)
        {
            Reference("dialogue", dialogue.DialogueId, "activeDialogue.dialogueId");
            var definition = package.Game.Dialogues.SingleOrDefault(item => item.Id == dialogue.DialogueId);
            if (definition?.Nodes.All(node => node.Id != dialogue.CurrentNodeId) != false)
                unresolved.Add(new GeneratedGameplaySessionReference
                {
                    Kind = "dialogue_node", Id = dialogue.CurrentNodeId,
                    SourcePath = "activeDialogue.currentNodeId"
                });
            ValidateTarget(package, state.ActiveEncounter, dialogue.SpeakerId,
                "activeDialogue.speakerId", unresolved);
            foreach (var pair in dialogue.Metadata)
            {
                Scalar(pair.Key, "activeDialogue.metadata.key");
                Scalar(pair.Value, "activeDialogue.metadata[" + pair.Key + "]");
            }
        }

        foreach (var faction in state.Factions)
        {
            Reference("faction", faction.FactionId, "factions.factionId");
            foreach (var pair in faction.Metadata)
            {
                Scalar(pair.Key, "factions.metadata.key");
                Scalar(pair.Value, "factions.metadata[" + pair.Key + "]");
            }
        }
        foreach (var pair in state.Metadata)
        {
            Scalar(pair.Key, "gameplayState.metadata.key");
            Scalar(pair.Value, "gameplayState.metadata[" + pair.Key + "]");
        }
        foreach (var pair in session.Metadata)
        {
            Scalar(pair.Key, "metadata.key");
            Scalar(pair.Value, "metadata[" + pair.Key + "]");
        }

        var portableFlagKeys = session.MapState.Flags.Keys
            .Concat(state.Flags.Select(flag => flag.Id))
            .Concat(state.Metadata.Keys)
            .Where(key => !byId.TryGetValue(key, out var definitions) || definitions.All(item => !item.Generated))
            .Distinct(StringComparer.Ordinal).OrderBy(key => key, StringComparer.Ordinal).ToList();
        var uniqueFingerprints = fingerprints.GroupBy(Key, StringComparer.Ordinal)
            .Select(group => group.First()).OrderBy(item => item.Kind, StringComparer.Ordinal)
            .ThenBy(item => item.Id, StringComparer.Ordinal).ToList();
        var uniqueUnresolved = unresolved.GroupBy(item => item.Kind + "\n" + item.Id + "\n" + item.SourcePath,
                StringComparer.Ordinal)
            .Select(group => group.First()).OrderBy(item => item.SourcePath, StringComparer.Ordinal)
            .ThenBy(item => item.Kind, StringComparer.Ordinal).ThenBy(item => item.Id, StringComparer.Ordinal)
            .ToList();
        return new GeneratedGameplaySessionReferenceInventory
        {
            Passed = uniqueUnresolved.Count == 0,
            Fingerprints = uniqueFingerprints,
            References = references,
            UnresolvedReferences = uniqueUnresolved,
            PortableFlagKeys = portableFlagKeys,
            Diagnostics = uniqueUnresolved.Select(item =>
                "generated_save.reference_unresolved:" + item.Kind + ":" + item.Id).ToList()
        };
    }

    public static bool IsStructural(string? id) =>
        !string.IsNullOrWhiteSpace(id) && StructuralIds.Contains(id);

    private static void Add<T>(
        ICollection<GeneratedGameplayDefinitionFingerprint> rows,
        IReadOnlyDictionary<string, string?> generated,
        string kind,
        IEnumerable<T> definitions)
    {
        var idProperty = typeof(T).GetProperty("Id")
                         ?? throw new InvalidOperationException("generated_save.definition_id_missing:" + kind);
        foreach (var definition in definitions)
        {
            var id = idProperty.GetValue(definition) as string ?? string.Empty;
            var key = kind + "\n" + id;
            generated.TryGetValue(key, out var sourceId);
            rows.Add(new GeneratedGameplayDefinitionFingerprint
            {
                Kind = kind,
                Id = id,
                CanonicalSha256 = GeneratedGameplaySaveJson.HashCanonical(definition),
                Generated = generated.ContainsKey(key),
                SourceId = sourceId
            });
        }
    }

    private static IReadOnlyDictionary<string, string?> GeneratedKeys(GamePackageDefinition package)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);
        void Mark(string kind, string id, string? sourceId)
        {
            if (!string.IsNullOrWhiteSpace(id)) result[kind + "\n" + id] = sourceId;
        }

        foreach (var scene in package.GeneratedContent.Scenes)
            Mark("map", scene.PackageMapId, scene.SourceId);
        var generatedItemSources = package.GeneratedContent.Items.Select(item => item.SourceId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var item in package.Game.Items)
            if (item.Metadata.TryGetValue("sourceItemSeedId", out var source)
                && generatedItemSources.Contains(source)) Mark("item", item.Id, source);
        foreach (var resource in package.Game.Resources)
            if (resource.Metadata.TryGetValue("sourceItemSeedIds", out var sources)
                && sources.Split(',', StringSplitOptions.RemoveEmptyEntries).Any(generatedItemSources.Contains))
                Mark("resource", resource.Id, sources);
        foreach (var faction in package.Game.Factions)
            if (faction.Kind == "generated_faction" && faction.Metadata.TryGetValue("sourceRegionId", out var source))
                Mark("faction", faction.Id, source);
        var encounterSources = package.GeneratedContent.Encounters.Select(item => item.SourceId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var encounter in package.Game.Encounters)
            if (encounter.Metadata.TryGetValue("sourceEncounterSeedId", out var source)
                && encounterSources.Contains(source)) Mark("encounter", encounter.Id, source);
        foreach (var quest in package.GeneratedContent.Quests)
            Mark("quest", quest.PackageQuestId, quest.SourceId);
        foreach (var dialogue in package.Game.Dialogues)
            if (dialogue.Metadata.TryGetValue("sourceActorSeedId", out var source))
                Mark("dialogue", dialogue.Id, source);
        foreach (var mechanic in package.GeneratedContent.Mechanics)
            Mark("ability", mechanic.PackageAbilityId, mechanic.SourceId);
        foreach (var interaction in package.Game.Interactions)
            if (interaction.Metadata.TryGetValue("sourceActorSeedId", out var source))
                Mark("interaction", interaction.Id, source);
        foreach (var map in package.Game.Maps)
        foreach (var entity in map.Entities)
            if (entity.Id.StartsWith(GeneratedWorldTravelOverlayService.TravelEntityIdPrefix,
                    StringComparison.Ordinal)
                || entity.PrototypeId is "entity_prototype/generated_actor" or "entity_prototype/generated_cache")
                Mark("entity", entity.Id, map.Id);
        Mark("entity", GeneratedWorldTravelOverlayService.TravelPrototypeId, "generated_travel");
        return result;
    }

    private static void ValidateOwner(
        GamePackageDefinition package,
        string? id,
        string path,
        ICollection<GeneratedGameplaySessionReference> unresolved)
    {
        if (string.IsNullOrWhiteSpace(id) || IsStructural(id)) return;
        if (package.Game.Maps.SelectMany(map => map.Entities).Any(entity => entity.Id == id)
            || package.Game.EntityPrototypes.Any(entity => entity.Id == id)
            || package.Game.Inventories.Any(inventory => inventory.OwnerId == id)) return;
        unresolved.Add(new GeneratedGameplaySessionReference { Kind = "owner", Id = id, SourcePath = path });
    }

    private static void ValidateInventoryId(
        GamePackageDefinition package,
        string? id,
        string path,
        ICollection<GeneratedGameplaySessionReference> unresolved)
    {
        if (string.IsNullOrWhiteSpace(id) || IsStructural(id)
            || package.Game.Inventories.Any(inventory => inventory.Id == id)) return;
        unresolved.Add(new GeneratedGameplaySessionReference { Kind = "inventory", Id = id, SourcePath = path });
    }

    private static void ValidateTarget(
        GamePackageDefinition package,
        EncounterRuntimeState? encounter,
        string? id,
        string path,
        ICollection<GeneratedGameplaySessionReference> unresolved)
    {
        if (string.IsNullOrWhiteSpace(id) || IsStructural(id)
            || package.Game.Maps.SelectMany(map => map.Entities).Any(entity => entity.Id == id)
            || encounter?.Participants.Any(participant => participant.Id == id) == true) return;
        unresolved.Add(new GeneratedGameplaySessionReference { Kind = "target", Id = id, SourcePath = path });
    }

    private static string Key(GeneratedGameplayDefinitionFingerprint row) => row.Kind + "\n" + row.Id;
}
