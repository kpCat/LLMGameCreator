using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Generation.Procedural;

public static class GeneratedCampaignRegionalEventDefinitionAuthorityService
{
    public static GeneratedCampaignRegionalEventOverlayDocument
        RefreshOverlay(
            GamePackageDefinition package,
            GeneratedCampaignRegionalEventOverlayDocument overlay,
            string? outputPackageSha256 = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(overlay);
        var diagnostics = overlay.Diagnostics.ToList();
        var bindings =
            new List<GeneratedCampaignRegionalEventBinding>();
        foreach (var binding in overlay.Bindings)
        {
            if (!TryEnrich(package, binding, out var enriched,
                    out var bindingDiagnostics))
                diagnostics.AddRange(bindingDiagnostics);
            bindings.Add(enriched);
        }
        var inventory = bindings
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .ThenBy(item => item.MapId, StringComparer.Ordinal)
            .ThenBy(item => item.RelationshipId, StringComparer.Ordinal)
            .ThenBy(item => item.EventKind)
            .Select(GeneratedCampaignRegionalEventInventoryService.Create)
            .ToList();
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return overlay with
        {
            OutputPackageSha256 = outputPackageSha256
                                  ?? PackageSha256(package),
            Bindings = bindings,
            Inventory = inventory,
            InventorySha256 =
                GeneratedCampaignChoiceCanonical.Hash(inventory),
            AddedDefinitionFingerprints = Fingerprints(package, bindings),
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    public static bool TryEnrich(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding,
        out GeneratedCampaignRegionalEventBinding enriched,
        out IReadOnlyList<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(binding);
        var values = new List<string>();
        enriched = binding;

        var maps = package.Game.Maps.Where(item =>
            item.Id == binding.MapId).ToList();
        var prototypes = package.Game.EntityPrototypes.Where(item =>
            item.Id == binding.EntityPrototypeId).ToList();
        var dialogues = package.Game.Dialogues.Where(item =>
            item.Id == binding.DialogueId).ToList();
        var interactions = package.Game.Interactions.Where(item =>
            item.Id == binding.InteractionId).ToList();
        if (maps.Count != 1 || prototypes.Count != 1
            || dialogues.Count != 1 || interactions.Count != 1)
        {
            values.Add(
                "generated_regional_event.actual_package.definition_missing");
            diagnostics = values;
            return false;
        }

        var entities = maps[0].Entities.Where(item =>
            item.Id == binding.MapEntityId).ToList();
        var choiceId = string.IsNullOrWhiteSpace(binding.ResolutionChoiceId)
            ? binding.DialogueId + "/resolve"
            : binding.ResolutionChoiceId;
        var choices = dialogues[0].Nodes.SelectMany(item => item.Choices)
            .Where(item => item.Id == choiceId).ToList();
        if (entities.Count != 1 || choices.Count != 1)
        {
            values.Add(
                "generated_regional_event.actual_package.definition_missing");
            diagnostics = values;
            return false;
        }

        var interactables = entities[0].Components.Where(item =>
            string.Equals(item.Type, "interactable",
                StringComparison.OrdinalIgnoreCase)).ToList();
        if (interactables.Count != 1
            || entities[0].PrototypeId != binding.EntityPrototypeId
            || entities[0].Position.X != binding.Placement.X
            || entities[0].Position.Y != binding.Placement.Y
            || interactables[0].Args.GetValueOrDefault("dialogueId")
            != binding.DialogueId
            || interactables[0].Args.GetValueOrDefault("interactionId")
            != binding.InteractionId)
        {
            values.Add(
                "generated_regional_event.actual_package.reference_mismatch");
            diagnostics = values;
            return false;
        }

        var questHash = OptionalHash(package.Game.Quests,
            binding.SourceQuestId, "quest", values);
        var encounterHash = OptionalHash(package.Game.Encounters,
            binding.ChallengeEncounterId, "encounter", values);
        if (values.Count > 0)
        {
            diagnostics = values.Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal).ToList();
            return false;
        }

        var dialogue = dialogues[0];
        var interaction = interactions[0];
        var prototype = prototypes[0];
        var entity = entities[0];
        var choice = choices[0];
        var interactable = interactables[0];
        enriched = binding with
        {
            ResolutionChoiceId = choice.Id,
            DialogueDefinitionSha256 = Hash(dialogue),
            InteractionDefinitionSha256 = Hash(interaction),
            EntityPrototypeDefinitionSha256 = Hash(prototype),
            MapEntityDefinitionSha256 = Hash(entity),
            SourceQuestDefinitionSha256 = questHash,
            ChallengeEncounterDefinitionSha256 = encounterHash,
            PositionSha256 = Hash(entity.Position),
            InteractableReferencesSha256 = Hash(new
            {
                MapId = maps[0].Id,
                entity.Id,
                entity.PrototypeId,
                entity.Position,
                interactable.Type,
                interactable.Args,
                DialogueId = binding.DialogueId,
                InteractionId = binding.InteractionId
            }),
            ResolutionRequirementsSha256 = Hash(choice.Requirements),
            ResolutionEffectsSha256 = Hash(choice.Effects),
            EventMetadataSha256 = Hash(new
            {
                InteractionMetadata = interaction.Metadata,
                DialogueMetadata = dialogue.Metadata,
                Nodes = dialogue.Nodes.Select(node => new
                {
                    node.Id,
                    node.Metadata,
                    Choices = node.Choices.Select(item => new
                    {
                        item.Id,
                        item.Metadata
                    }).ToList()
                }).ToList()
            })
        };
        diagnostics = [];
        return true;
    }

    public static GeneratedCampaignRegionalEventOverlayValidationResult
        ValidateActualPackage(
            GamePackageDefinition package,
            GeneratedCampaignRegionalEventOverlayDocument overlay)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(overlay);
        var diagnostics = new List<string>();
        var expectedBindings =
            new List<GeneratedCampaignRegionalEventBinding>();
        foreach (var binding in overlay.Bindings)
        {
            if (!TryEnrich(package, binding, out var expected,
                    out var bindingDiagnostics))
            {
                diagnostics.AddRange(bindingDiagnostics);
                continue;
            }
            expectedBindings.Add(expected);
            if (!CanonicalEqual(expected, binding))
                diagnostics.Add(
                    "generated_regional_event.actual_package.binding_definition_hash");
        }

        var expectedInventory = expectedBindings
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .ThenBy(item => item.MapId, StringComparer.Ordinal)
            .ThenBy(item => item.RelationshipId, StringComparer.Ordinal)
            .ThenBy(item => item.EventKind)
            .Select(GeneratedCampaignRegionalEventInventoryService.Create)
            .ToList();
        if (!CanonicalEqual(expectedInventory, overlay.Inventory)
            || overlay.InventorySha256 !=
            GeneratedCampaignChoiceCanonical.Hash(expectedInventory))
            diagnostics.Add(
                "generated_regional_event.actual_package.inventory_definition_hash");

        var fingerprints = Fingerprints(package, expectedBindings);
        if (!CanonicalEqual(fingerprints,
                overlay.AddedDefinitionFingerprints))
            diagnostics.Add(
                "generated_regional_event.actual_package.definition_fingerprint");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignRegionalEventOverlayValidationResult
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    public static IReadOnlyList<
        GeneratedCampaignRegionalEventDefinitionFingerprint> Fingerprints(
        GamePackageDefinition package,
        IReadOnlyList<GeneratedCampaignRegionalEventBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(bindings);
        var result = new List<
            GeneratedCampaignRegionalEventDefinitionFingerprint>();
        foreach (var binding in bindings)
        {
            Add("game.entityPrototypes", binding.EntityPrototypeId,
                package.Game.EntityPrototypes.Single(item =>
                    item.Id == binding.EntityPrototypeId));
            Add("game.dialogues", binding.DialogueId,
                package.Game.Dialogues.Single(item =>
                    item.Id == binding.DialogueId));
            Add("game.interactions", binding.InteractionId,
                package.Game.Interactions.Single(item =>
                    item.Id == binding.InteractionId));
            Add("game.maps[" + binding.MapId + "].entities",
                binding.MapEntityId,
                package.Game.Maps.Single(item =>
                        item.Id == binding.MapId).Entities
                    .Single(item => item.Id == binding.MapEntityId));
            if (!string.IsNullOrWhiteSpace(binding.SourceQuestId))
                Add("game.quests", binding.SourceQuestId,
                    package.Game.Quests.Single(item =>
                        item.Id == binding.SourceQuestId));
            if (!string.IsNullOrWhiteSpace(
                    binding.ChallengeEncounterId))
                Add("game.encounters",
                    binding.ChallengeEncounterId,
                    package.Game.Encounters.Single(item =>
                        item.Id == binding.ChallengeEncounterId));
        }
        return result.OrderBy(item => item.CollectionPath,
                StringComparer.Ordinal)
            .ThenBy(item => item.DefinitionId, StringComparer.Ordinal)
            .ToList();

        void Add<T>(string path, string id, T definition) =>
            result.Add(new
                GeneratedCampaignRegionalEventDefinitionFingerprint
                {
                    CollectionPath = path,
                    DefinitionId = id,
                    CanonicalSha256 = Hash(definition)
                });
    }

    public static IReadOnlyDictionary<string, int>
        GeneratedRecordCounts(GamePackageDefinition package)
    {
        ArgumentNullException.ThrowIfNull(package);
        var dialogueIds = package.Game.Dialogues.Where(item =>
                item.Metadata.ContainsKey("generatedRegionalEventId"))
            .Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var interactionIds = package.Game.Interactions.Where(item =>
                item.Metadata.ContainsKey("generatedRegionalEventId"))
            .Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var entities = package.Game.Maps.SelectMany(item => item.Entities)
            .Where(entity => entity.Components.Any(component =>
                component.Args.TryGetValue("dialogueId",
                    out var dialogueId)
                && dialogueIds.Contains(dialogueId)
                || component.Args.TryGetValue("interactionId",
                    out var interactionId)
                && interactionIds.Contains(interactionId)))
            .ToList();
        var prototypeIds = entities.Select(item => item.PrototypeId)
            .ToHashSet(StringComparer.Ordinal);
        return new SortedDictionary<string, int>(StringComparer.Ordinal)
        {
            ["dialogues"] = dialogueIds.Count,
            ["interactions"] = interactionIds.Count,
            ["mapEntities"] = entities.Count,
            ["entityPrototypes"] = package.Game.EntityPrototypes.Count(
                item => prototypeIds.Contains(item.Id))
        };
    }

    private static string OptionalHash<T>(
        IEnumerable<T> definitions,
        string id,
        string kind,
        ICollection<string> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id))
            return string.Empty;
        var property = typeof(T).GetProperty("Id")
                       ?? throw new InvalidOperationException(
                           "generated_regional_event.definition_id_missing:"
                           + kind);
        var matches = definitions.Where(item =>
            string.Equals(property.GetValue(item) as string, id,
                StringComparison.Ordinal)).ToList();
        if (matches.Count == 1)
            return Hash(matches[0]);
        diagnostics.Add(
            "generated_regional_event.actual_package." + kind
            + "_definition_missing");
        return string.Empty;
    }

    private static bool CanonicalEqual<T>(T left, T right) =>
        GeneratedCampaignChoiceCanonical.Serialize(left)
        == GeneratedCampaignChoiceCanonical.Serialize(right);

    private static string Hash<T>(T value) =>
        GeneratedCampaignChoiceCanonical.Hash(value);

    private static string PackageSha256(
        GamePackageDefinition package) =>
        GeneratedEncounterCombatCanonical.HashText(
            GeneratedEncounterCombatCanonical.Serialize(package)
            + Environment.NewLine);
}
