using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using DomainEffectDefinition =
    LLMGameCreator.Domain.Definitions.EffectDefinition;
using DomainRequirementDefinition =
    LLMGameCreator.Domain.Definitions.RequirementDefinition;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedCampaignRegionalEventOverlayService
{
    public GeneratedCampaignRegionalEventOverlayResult Build(
        GamePackageDefinition relationshipOverlayPackage,
        GeneratedCampaignRegionalEventBindingResult bindingResult)
    {
        ArgumentNullException.ThrowIfNull(relationshipOverlayPackage);
        ArgumentNullException.ThrowIfNull(bindingResult);
        if (!bindingResult.Passed)
            return Failed(bindingResult.Diagnostics.Count == 0
                ? ["generated_regional_event.binding_invalid"]
                : bindingResult.Diagnostics);

        var before = GeneratedCampaignChoiceCanonical.Clone(
            relationshipOverlayPackage);
        var after = GeneratedCampaignChoiceCanonical.Clone(
            relationshipOverlayPackage);
        var diagnostics = new List<string>();
        foreach (var binding in bindingResult.Bindings)
            AddEvent(after, binding, diagnostics);
        Canonicalize(after);
        var authoritativeBindings =
            new List<GeneratedCampaignRegionalEventBinding>();
        foreach (var binding in bindingResult.Bindings)
        {
            if (!GeneratedCampaignRegionalEventDefinitionAuthorityService
                    .TryEnrich(after, binding, out var enriched,
                        out var authorityDiagnostics))
                diagnostics.AddRange(authorityDiagnostics);
            authoritativeBindings.Add(enriched);
        }

        ValidateControlledDelta(before, after, authoritativeBindings,
            diagnostics);
        ValidateReferences(after, authoritativeBindings, diagnostics);
        var inventory = Inventory(authoritativeBindings);
        var countsBefore = DefinitionCounts(before);
        var countsAfter = DefinitionCounts(after);
        var identityPassed = authoritativeBindings.All(item =>
            item.RegionalEventId == item.DialogueId
            && item.ResolutionFlagId == item.DialogueId);
        if (!identityPassed)
            diagnostics.Add("generated_regional_event.identity_invalid");
        var placementPassed = authoritativeBindings.All(item =>
            item.Placement is
            {
                Walkable: true,
                Reachable: true,
                Safe: true
            })
            && bindingResult.Bindings.GroupBy(item =>
                    (item.MapId, item.Placement.X, item.Placement.Y))
                .All(group => group.Count() == 1);
        if (!placementPassed)
            diagnostics.Add(
                "generated_regional_event.placement_collision");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var json = GeneratedCampaignChoiceCanonical.Serialize(after)
                   + Environment.NewLine;
        var document = new GeneratedCampaignRegionalEventOverlayDocument
        {
            SourcePackageSha256 = PackageSha256(before),
            OutputPackageSha256 =
                GeneratedCampaignChoiceCanonical.HashText(json),
            EventCount = authoritativeBindings.Count,
            SupportGratitudeCount = authoritativeBindings.Count(item =>
                item.EventKind ==
                GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE),
            ChallengeAftermathCount = authoritativeBindings.Count(item =>
                item.EventKind ==
                GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH),
            RefusalFalloutCount = authoritativeBindings.Count(item =>
                item.EventKind ==
                GeneratedCampaignRegionalEventKind.REFUSAL_FALLOUT),
            Bindings = authoritativeBindings,
            Inventory = inventory,
            InventorySha256 =
                GeneratedCampaignChoiceCanonical.Hash(inventory),
            AddedDefinitionFingerprints =
                GeneratedCampaignRegionalEventDefinitionAuthorityService
                    .Fingerprints(after, authoritativeBindings),
            DefinitionCollectionCountsBefore = countsBefore,
            DefinitionCollectionCountsAfter = countsAfter,
            IdentityPassed = identityPassed,
            PlacementPassed = placementPassed,
            ControlledDeltaPassed = !diagnostics.Any(item =>
                item is
                    "generated_regional_event.overlay_delta_outside_scope"
                    or "generated_regional_event.existing_definition_changed"),
            Deterministic = true,
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
        return new GeneratedCampaignRegionalEventOverlayResult
        {
            Passed = document.Passed,
            RegionalEventOverlayPackage = after,
            RegionalEventOverlayPackageJson = json,
            Document = document,
            Diagnostics = diagnostics
        };
    }

    public GeneratedCampaignRegionalEventOverlayValidationResult
        ValidateOverlayPackage(
            GamePackageDefinition relationshipOverlayPackage,
            GamePackageDefinition regionalEventOverlayPackage,
            GeneratedCampaignRegionalEventOverlayDocument overlay)
    {
        ArgumentNullException.ThrowIfNull(relationshipOverlayPackage);
        ArgumentNullException.ThrowIfNull(regionalEventOverlayPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        var diagnostics = new List<string>();
        ValidateControlledDelta(relationshipOverlayPackage,
            regionalEventOverlayPackage, overlay.Bindings, diagnostics);
        ValidateReferences(regionalEventOverlayPackage,
            overlay.Bindings, diagnostics);
        if (!string.Equals(overlay.SourcePackageSha256,
                PackageSha256(relationshipOverlayPackage),
                StringComparison.Ordinal)
            || !string.Equals(overlay.OutputPackageSha256,
                PackageSha256(regionalEventOverlayPackage),
                StringComparison.Ordinal))
            diagnostics.Add(
                "generated_regional_event.overlay_package_hash_mismatch");
        var inventory = Inventory(overlay.Bindings);
        if (!Same(inventory, overlay.Inventory)
            || !string.Equals(
                GeneratedCampaignChoiceCanonical.Hash(inventory),
                overlay.InventorySha256, StringComparison.Ordinal))
            diagnostics.Add(
                "generated_regional_event.inventory_mismatch");
        if (!Same(
                GeneratedCampaignRegionalEventDefinitionAuthorityService
                    .Fingerprints(regionalEventOverlayPackage,
                        overlay.Bindings),
                overlay.AddedDefinitionFingerprints))
            diagnostics.Add(
                "generated_regional_event.inventory_mismatch");
        var packageAuthority =
            GeneratedCampaignRegionalEventDefinitionAuthorityService
                .ValidateActualPackage(regionalEventOverlayPackage,
                    overlay);
        diagnostics.AddRange(packageAuthority.Diagnostics);
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignRegionalEventOverlayValidationResult
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    private static void AddEvent(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding,
        ICollection<string> diagnostics)
    {
        if (package.Game.EntityPrototypes.Any(item =>
                item.Id == binding.EntityPrototypeId)
            || package.Game.Dialogues.Any(item =>
                item.Id == binding.DialogueId)
            || package.Game.Interactions.Any(item =>
                item.Id == binding.InteractionId)
            || package.Game.Maps.SelectMany(item => item.Entities)
                .Any(item => item.Id == binding.MapEntityId))
        {
            diagnostics.Add(
                "generated_regional_event.identity_invalid");
            return;
        }
        var map = package.Game.Maps.SingleOrDefault(item =>
            item.Id == binding.MapId);
        if (map is null)
        {
            diagnostics.Add(
                "generated_regional_event.region_map_missing");
            return;
        }

        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition
        {
            Id = binding.EntityPrototypeId,
            Name = EventTitle(package, binding),
            Components =
            [
                new ComponentDefinition
                {
                    Type = "collidable"
                }
            ]
        });
        package.Game.Interactions.Add(new InteractionDefinition
        {
            Id = binding.InteractionId,
            Kind = "inspect",
            Metadata = EventMetadata(binding)
        });
        package.Game.Dialogues.Add(Dialogue(package, binding));
        map.Entities.Add(new EntityInstanceDefinition
        {
            Id = binding.MapEntityId,
            PrototypeId = binding.EntityPrototypeId,
            Position = new Position2D(binding.Placement.X,
                binding.Placement.Y),
            Components =
            [
                new ComponentDefinition
                {
                    Type = "interactable",
                    Args = new Dictionary<string, string>(
                        StringComparer.Ordinal)
                    {
                        ["dialogueId"] = binding.DialogueId,
                        ["interactionId"] = binding.InteractionId
                    }
                }
            ]
        });
    }

    private static DialogueDefinition Dialogue(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding)
    {
        var startId = binding.DialogueId + "/start";
        var resolution = new DialogueChoiceDefinition
        {
            Id = binding.DialogueId + "/resolve",
            Text = ResolutionText(binding.EventKind),
            Requirements = ResolutionRequirements(binding),
            Effects = ResolutionEffects(binding),
            CloseDialogue = true,
            Tags = ["generated_regional_event", "resolution"],
            Metadata = EventMetadata(binding)
        };
        var resolved = new DialogueChoiceDefinition
        {
            Id = binding.DialogueId + "/resolved",
            Text = "Событие уже завершено.",
            Requirements =
            [
                FlagEquals(binding.ResolutionFlagId, "RESOLVED")
            ],
            CloseDialogue = true,
            Tags = ["generated_regional_event", "resolved"],
            Metadata = EventMetadata(binding)
        };
        return new DialogueDefinition
        {
            Id = binding.DialogueId,
            Title = EventTitle(package, binding),
            StartNodeId = startId,
            Tags =
            [
                "generated_regional_event",
                binding.EventKind.ToString().ToLowerInvariant()
            ],
            Metadata = EventMetadata(binding),
            Nodes =
            [
                new DialogueNodeDefinition
                {
                    Id = startId,
                    SpeakerId = binding.ActorEntityId,
                    Text = EventDescription(binding.EventKind),
                    Metadata = EventMetadata(binding),
                    Choices = [resolution, resolved]
                }
            ]
        };
    }

    private static List<DomainRequirementDefinition> ResolutionRequirements(
        GeneratedCampaignRegionalEventBinding binding)
    {
        var result = new List<DomainRequirementDefinition>
        {
            FlagEquals(binding.Prerequisite.DecisionFlagId,
                binding.Prerequisite.DecisionFlagValue),
            FlagEquals(binding.ResolutionFlagId, string.Empty)
        };
        result.AddRange(binding.Prerequisite.CompletedQuestIds.Select(id =>
            new DomainRequirementDefinition
            {
                Kind = "quest_state",
                Id = id,
                Value = "completed"
            }));
        if (!string.IsNullOrWhiteSpace(
                binding.Prerequisite.ChallengeVictoryFlagId))
            result.Add(FlagEquals(
                binding.Prerequisite.ChallengeVictoryFlagId,
                "VICTORY"));
        return result;
    }

    private static List<DomainEffectDefinition> ResolutionEffects(
        GeneratedCampaignRegionalEventBinding binding)
    {
        var result = new List<DomainEffectDefinition>
        {
            new()
            {
                Type = "set_flag",
                Args = new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["id"] = binding.ResolutionFlagId,
                    ["value"] = "RESOLVED"
                }
            }
        };
        if (binding.EventKind ==
            GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE)
            result.Add(new DomainEffectDefinition
            {
                Type = "change_reputation",
                Args = new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["id"] = binding.FactionId,
                    ["amount"] = binding.ResolutionReputationDelta
                        .ToString(
                            System.Globalization.CultureInfo
                                .InvariantCulture)
                }
            });
        return result;
    }

    private static DomainRequirementDefinition FlagEquals(
        string id,
        string value) => new()
    {
        Kind = "flag_equals",
        Id = id,
        Value = value
    };

    private static Dictionary<string, string> EventMetadata(
        GeneratedCampaignRegionalEventBinding binding) =>
        new(StringComparer.Ordinal)
        {
            ["generatedRegionalEventId"] =
                binding.RegionalEventId,
            ["generatedRegionalEventKind"] =
                binding.EventKind.ToString(),
            ["generatedRegionalEventRelationshipId"] =
                binding.RelationshipId,
            ["generatedRegionalEventBranch"] =
                binding.RelationshipBranch.ToString(),
            ["generatedRegionalEventRegionId"] = binding.RegionId,
            ["generatedRegionalEventMapId"] = binding.MapId,
            ["generatedRegionalEventActorSeedId"] =
                binding.ActorSeedId,
            ["generatedRegionalEventActorEntityId"] =
                binding.ActorEntityId,
            ["generatedRegionalEventFactionId"] =
                binding.FactionId,
            ["generatedRegionalEventPrototypeId"] =
                binding.EntityPrototypeId,
            ["generatedRegionalEventMapEntityId"] =
                binding.MapEntityId,
            ["generatedRegionalEventInteractionId"] =
                binding.InteractionId,
            ["generatedRegionalEventResolutionFlagId"] =
                binding.ResolutionFlagId,
            ["generatedRegionalEventSourceQuestId"] =
                binding.SourceQuestId,
            ["generatedRegionalEventChallengeEncounterId"] =
                binding.ChallengeEncounterId,
            ["generatedRegionalEventChallengeEncounterSourceId"] =
                binding.ChallengeEncounterSourceId,
            ["generatedRegionalEventTargetRegionDerivation"] =
                binding.TargetRegionDerivation.ToString(),
            ["generatedRegionalEventTargetRegionFingerprint"] =
                binding.TargetRegionFingerprint,
            ["generatedRegionalEventPrerequisiteFingerprint"] =
                binding.Prerequisite.Fingerprint,
            ["generatedRegionalEventRewardFingerprint"] =
                binding.SourceQuestRewardFingerprint
        };

    private static string EventTitle(
        GamePackageDefinition package,
        GeneratedCampaignRegionalEventBinding binding)
    {
        var actor = package.Game.Dialogues.SingleOrDefault(item =>
            item.Id == binding.RelationshipId)?.Title
            ?? "Жители региона";
        return binding.EventKind switch
        {
            GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE =>
                "Благодарность: " + actor,
            GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH =>
                "После поединка: " + actor,
            _ => "Последствия отказа: " + actor
        };
    }

    private static string EventDescription(
        GeneratedCampaignRegionalEventKind kind) => kind switch
    {
        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE =>
            "Поддержка изменила жизнь региона. Здесь вас хотят поблагодарить.",
        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH =>
            "Победа в поединке получила продолжение в этом регионе.",
        _ => "Отказ вызвал последствия, о которых стало известно в регионе."
    };

    private static string ResolutionText(
        GeneratedCampaignRegionalEventKind kind) => kind switch
    {
        GeneratedCampaignRegionalEventKind.SUPPORT_GRATITUDE =>
            "Принять благодарность",
        GeneratedCampaignRegionalEventKind.CHALLENGE_AFTERMATH =>
            "Обсудить последствия победы",
        _ => "Разобраться с последствиями отказа"
    };

    private static void Canonicalize(GamePackageDefinition package)
    {
        package.Game.EntityPrototypes = package.Game.EntityPrototypes
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Dialogues = package.Game.Dialogues
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        package.Game.Interactions = package.Game.Interactions
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        foreach (var map in package.Game.Maps)
            map.Entities = map.Entities.OrderBy(item => item.Id,
                StringComparer.Ordinal).ToList();
        package.Game.Maps = package.Game.Maps.OrderBy(item => item.Id,
            StringComparer.Ordinal).ToList();
    }

    private static void ValidateControlledDelta(
        GamePackageDefinition before,
        GamePackageDefinition after,
        IReadOnlyList<GeneratedCampaignRegionalEventBinding> bindings,
        ICollection<string> diagnostics)
    {
        var stripped = GeneratedCampaignChoiceCanonical.Clone(after);
        var prototypeIds = bindings.Select(item => item.EntityPrototypeId)
            .ToHashSet(StringComparer.Ordinal);
        var dialogueIds = bindings.Select(item => item.DialogueId)
            .ToHashSet(StringComparer.Ordinal);
        var interactionIds = bindings.Select(item => item.InteractionId)
            .ToHashSet(StringComparer.Ordinal);
        var entityIds = bindings.Select(item => item.MapEntityId)
            .ToHashSet(StringComparer.Ordinal);
        stripped.Game.EntityPrototypes = stripped.Game.EntityPrototypes
            .Where(item => !prototypeIds.Contains(item.Id)).ToList();
        stripped.Game.Dialogues = stripped.Game.Dialogues
            .Where(item => !dialogueIds.Contains(item.Id)).ToList();
        stripped.Game.Interactions = stripped.Game.Interactions
            .Where(item => !interactionIds.Contains(item.Id)).ToList();
        foreach (var map in stripped.Game.Maps)
            map.Entities = map.Entities.Where(item =>
                !entityIds.Contains(item.Id)).ToList();
        Canonicalize(stripped);
        var canonicalBefore =
            GeneratedCampaignChoiceCanonical.Clone(before);
        Canonicalize(canonicalBefore);
        if (!Same(canonicalBefore, stripped))
            diagnostics.Add(
                "generated_regional_event.existing_definition_changed");

        var beforeCounts = DefinitionCounts(before);
        var afterCounts = DefinitionCounts(after);
        foreach (var pair in beforeCounts)
        {
            var expected = pair.Value;
            if (pair.Key is "game.entityPrototypes"
                or "game.dialogues" or "game.interactions")
                expected += bindings.Count;
            if (afterCounts.GetValueOrDefault(pair.Key) != expected)
                diagnostics.Add(
                    "generated_regional_event.overlay_delta_outside_scope");
        }
        if (before.Game.Maps.Count != after.Game.Maps.Count
            || before.Game.Maps.Sum(item => item.Entities.Count)
            + bindings.Count !=
            after.Game.Maps.Sum(item => item.Entities.Count))
            diagnostics.Add(
                "generated_regional_event.overlay_delta_outside_scope");
    }

    private static void ValidateReferences(
        GamePackageDefinition package,
        IReadOnlyList<GeneratedCampaignRegionalEventBinding> bindings,
        ICollection<string> diagnostics)
    {
        foreach (var binding in bindings)
        {
            var entity = package.Game.Maps.Single(item =>
                    item.Id == binding.MapId).Entities
                .SingleOrDefault(item => item.Id ==
                    binding.MapEntityId);
            var component = entity?.Components.SingleOrDefault(item =>
                string.Equals(item.Type, "interactable",
                    StringComparison.OrdinalIgnoreCase));
            if (package.Game.EntityPrototypes.Count(item =>
                    item.Id == binding.EntityPrototypeId) != 1
                || package.Game.Dialogues.Count(item =>
                    item.Id == binding.DialogueId) != 1
                || package.Game.Interactions.Count(item =>
                    item.Id == binding.InteractionId) != 1
                || entity?.PrototypeId != binding.EntityPrototypeId
                || component?.Args.GetValueOrDefault("dialogueId")
                != binding.DialogueId
                || component.Args.GetValueOrDefault("interactionId")
                != binding.InteractionId)
                diagnostics.Add(
                    "generated_regional_event.inventory_mismatch");
        }
    }

    private static IReadOnlyList<GeneratedCampaignRegionalEventInventoryRow>
        Inventory(
            IEnumerable<GeneratedCampaignRegionalEventBinding> bindings) =>
        bindings.OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .ThenBy(item => item.MapId, StringComparer.Ordinal)
            .ThenBy(item => item.RelationshipId,
                StringComparer.Ordinal)
            .ThenBy(item => item.EventKind)
            .Select(GeneratedCampaignRegionalEventInventoryService.Create)
            .ToList();

    private static IReadOnlyDictionary<string, int> DefinitionCounts(
        GamePackageDefinition package)
    {
        using var json = JsonDocument.Parse(
            GeneratedCampaignChoiceCanonical.Serialize(package.Game));
        return json.RootElement.EnumerateObject()
            .Where(item => item.Value.ValueKind == JsonValueKind.Array)
            .ToDictionary(item => "game." + item.Name,
                item => item.Value.GetArrayLength(),
                StringComparer.Ordinal);
    }

    private static bool Same<T>(T left, T right) =>
        string.Equals(GeneratedCampaignChoiceCanonical.Serialize(left),
            GeneratedCampaignChoiceCanonical.Serialize(right),
            StringComparison.Ordinal);

    private static string PackageSha256(GamePackageDefinition package) =>
        GeneratedCampaignChoiceCanonical.HashText(
            GeneratedCampaignChoiceCanonical.Serialize(package)
            + Environment.NewLine);

    private static GeneratedCampaignRegionalEventOverlayResult Failed(
        IReadOnlyList<string> diagnostics) => new()
    {
        Diagnostics = diagnostics
    };
}
