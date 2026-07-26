using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using DomainRequirementDefinition = LLMGameCreator.Domain.Definitions.RequirementDefinition;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedCampaignRelationshipOverlayService
{
    private static readonly IReadOnlyList<string> AllowedFieldPaths =
    [
        "game.dialogues[*].nodes",
        "game.dialogues[*].metadata.generatedRelationship*",
        "game.dialogues[*].tags.generated_relationship",
        "game.quests[*].autoStart",
        "game.quests[*].metadata.generatedRelationship*"
    ];

    public GeneratedCampaignRelationshipOverlayResult Build(
        GamePackageDefinition choiceOverlayPackage,
        GeneratedCampaignRelationshipBindingResult bindingResult)
    {
        ArgumentNullException.ThrowIfNull(choiceOverlayPackage);
        ArgumentNullException.ThrowIfNull(bindingResult);
        if (!bindingResult.Passed)
            return Failed(bindingResult.Diagnostics.Count == 0
                ? ["generated_relationship.binding_invalid"]
                : bindingResult.Diagnostics);

        var relationships = bindingResult.Bindings
            .Where(item => item.QuestArc.Count > 0)
            .OrderBy(item => item.RelationshipId, StringComparer.Ordinal)
            .ToList();
        var before = GeneratedCampaignChoiceCanonical.Clone(choiceOverlayPackage);
        var after = GeneratedCampaignChoiceCanonical.Clone(choiceOverlayPackage);
        var diagnostics = new List<string>();
        foreach (var relationship in relationships)
        {
            var dialogue = after.Game.Dialogues.SingleOrDefault(item =>
                string.Equals(item.Id, relationship.DialogueId,
                    StringComparison.Ordinal));
            if (dialogue is null)
            {
                diagnostics.Add("generated_relationship.dialogue_missing");
                continue;
            }
            ApplyDialogue(after, dialogue, relationship, diagnostics);
            foreach (var step in relationship.QuestArc)
            {
                var quest = after.Game.Quests.SingleOrDefault(item =>
                    string.Equals(item.Id, step.QuestId,
                        StringComparison.Ordinal));
                if (quest is null)
                {
                    diagnostics.Add("generated_relationship.quest_missing");
                    continue;
                }
                quest.AutoStart = false;
                quest.Metadata["generatedRelationshipId"] =
                    relationship.RelationshipId;
                quest.Metadata["generatedRelationshipActorSeedId"] =
                    relationship.ActorSeedId;
                quest.Metadata["generatedRelationshipFactionId"] =
                    relationship.FactionId;
                quest.Metadata["generatedRelationshipArcOrder"] =
                    step.Order.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);
                quest.Metadata["generatedRelationshipQuestSourceId"] =
                    step.QuestSourceId;
            }
        }

        after.Game.Dialogues = after.Game.Dialogues
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        after.Game.Quests = after.Game.Quests
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        var relationshipIds = relationships.Select(item => item.RelationshipId)
            .ToHashSet(StringComparer.Ordinal);
        var assignedQuestIds = relationships.SelectMany(item => item.QuestArc)
            .Select(item => item.QuestId).ToHashSet(StringComparer.Ordinal);
        var assignmentUnique = relationships.SelectMany(item => item.QuestArc)
                                   .Select(item => item.QuestId).Count()
                               == assignedQuestIds.Count;
        if (!assignmentUnique)
            diagnostics.Add("generated_relationship.quest_assigned_multiple");
        var ordering = relationships.All(item => item.QuestArc
            .Select((step, index) => step.Order == index)
            .All(value => value));
        if (!ordering)
            diagnostics.Add("generated_relationship.arc_order_invalid");
        var countsBefore = DefinitionCounts(before);
        var countsAfter = DefinitionCounts(after);
        if (!countsBefore.OrderBy(item => item.Key, StringComparer.Ordinal)
                .SequenceEqual(countsAfter.OrderBy(item => item.Key,
                    StringComparer.Ordinal)))
            diagnostics.Add(
                "generated_relationship.delta_unexpected_collection_change");
        ValidateControlledDelta(before, after, relationshipIds,
            assignedQuestIds, diagnostics);
        var inventory = Inventory(relationships);
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        var json = GeneratedCampaignChoiceCanonical.Serialize(after)
                   + Environment.NewLine;
        var document = new GeneratedCampaignRelationshipOverlayDocument
        {
            SourcePackageSha256 = PackageSha256(before),
            OutputPackageSha256 =
                GeneratedCampaignChoiceCanonical.HashText(json),
            RelationshipCount = relationships.Count,
            ArcQuestCount = relationships.Sum(item => item.QuestArc.Count),
            Bindings = relationships,
            FingerprintsBefore = Fingerprints(before, relationshipIds,
                assignedQuestIds),
            FingerprintsAfter = Fingerprints(after, relationshipIds,
                assignedQuestIds),
            Inventory = inventory,
            InventorySha256 =
                GeneratedCampaignChoiceCanonical.Hash(inventory),
            AllowedFieldPaths = AllowedFieldPaths,
            DefinitionCollectionCountsBefore = countsBefore,
            DefinitionCollectionCountsAfter = countsAfter,
            ControlledDeltaPassed = !diagnostics.Any(item =>
                item.StartsWith("generated_relationship.delta_",
                    StringComparison.Ordinal)),
            AssignmentUnique = assignmentUnique,
            ArcOrderingDeterministic = ordering,
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
        return new GeneratedCampaignRelationshipOverlayResult
        {
            Passed = document.Passed,
            RelationshipOverlayPackage = after,
            RelationshipOverlayPackageJson = json,
            Document = document,
            Diagnostics = diagnostics
        };
    }

    public GeneratedCampaignRelationshipOverlayValidationResult
        ValidateOverlayPackage(
            GamePackageDefinition choiceOverlayPackage,
            GamePackageDefinition relationshipOverlayPackage,
            GeneratedCampaignRelationshipOverlayDocument overlay)
    {
        ArgumentNullException.ThrowIfNull(choiceOverlayPackage);
        ArgumentNullException.ThrowIfNull(relationshipOverlayPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        var diagnostics = new List<string>();
        var relationshipIds = overlay.Bindings
            .Select(item => item.RelationshipId)
            .ToHashSet(StringComparer.Ordinal);
        var questIds = overlay.Bindings.SelectMany(item => item.QuestArc)
            .Select(item => item.QuestId).ToHashSet(StringComparer.Ordinal);
        ValidateControlledDelta(choiceOverlayPackage,
            relationshipOverlayPackage, relationshipIds, questIds, diagnostics);
        if (!string.Equals(overlay.SourcePackageSha256,
                PackageSha256(choiceOverlayPackage), StringComparison.Ordinal)
            || !string.Equals(overlay.OutputPackageSha256,
                PackageSha256(relationshipOverlayPackage),
                StringComparison.Ordinal))
            diagnostics.Add(
                "generated_relationship.overlay_package_hash_mismatch");
        var inventory = Inventory(overlay.Bindings);
        if (!Same(inventory, overlay.Inventory)
            || !string.Equals(
                GeneratedCampaignChoiceCanonical.Hash(inventory),
                overlay.InventorySha256, StringComparison.Ordinal))
            diagnostics.Add("generated_relationship.inventory_mismatch");
        if (!Same(Fingerprints(relationshipOverlayPackage,
                    relationshipIds, questIds),
                overlay.FingerprintsAfter))
            diagnostics.Add("generated_relationship.fingerprint_mismatch");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignRelationshipOverlayValidationResult
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    private static void ApplyDialogue(
        GamePackageDefinition package,
        DialogueDefinition dialogue,
        GeneratedCampaignRelationshipBinding relationship,
        ICollection<string> diagnostics)
    {
        var start = dialogue.Nodes.SingleOrDefault(item =>
            string.Equals(item.Id, dialogue.StartNodeId,
                StringComparison.Ordinal));
        if (start is null)
        {
            diagnostics.Add("generated_relationship.start_node_missing");
            return;
        }

        var support = start.Choices.SingleOrDefault(item =>
            IsChoice(item, GeneratedCampaignRelationshipBranch.SUPPORT,
                "initial"));
        var challenge = start.Choices.SingleOrDefault(item =>
            IsChoice(item, GeneratedCampaignRelationshipBranch.CHALLENGE,
                "initial"));
        var refuse = start.Choices.SingleOrDefault(item =>
            IsChoice(item, GeneratedCampaignRelationshipBranch.REFUSE,
                "initial"));
        if (support is null)
        {
            support = InitialRelationshipChoice(
                GeneratedCampaignRelationshipBranch.SUPPORT, relationship);
            start.Choices.Add(support);
        }
        if (refuse is null)
        {
            refuse = InitialRelationshipChoice(
                GeneratedCampaignRelationshipBranch.REFUSE, relationship);
            start.Choices.Add(refuse);
        }

        support.StartQuestId = relationship.QuestArc[0].QuestId;
        AddRelationshipMetadata(support.Metadata, relationship,
            "initial", 0);
        AddRelationshipMetadata(refuse.Metadata, relationship,
            "initial", -1);
        if (challenge is not null)
            AddRelationshipMetadata(challenge.Metadata, relationship,
                "initial", -1);

        var retained = start.Choices.Where(item =>
                !IsChoice(item, GeneratedCampaignRelationshipBranch.SUPPORT,
                    "followup"))
            .ToList();
        var followUps = new List<DialogueChoiceDefinition>();
        foreach (var step in relationship.QuestArc)
        {
            var quest = package.Game.Quests.Single(item =>
                string.Equals(item.Id, step.QuestId,
                    StringComparison.Ordinal));
            followUps.Add(FollowUp(
                relationship,
                "active/" + step.Order,
                "Задание продолжается: «" + quest.Title + "»",
                [
                    FlagEquals(relationship.DecisionFlagId, "SUPPORT"),
                    QuestState(step.QuestId, "active")
                ],
                null,
                step.Order));
            if (step.Order + 1 < relationship.QuestArc.Count)
            {
                var next = relationship.QuestArc[step.Order + 1];
                var nextQuest = package.Game.Quests.Single(item =>
                    string.Equals(item.Id, next.QuestId,
                        StringComparison.Ordinal));
                followUps.Add(FollowUp(
                    relationship,
                    "next/" + next.Order,
                    "Начать следующее задание: «" + nextQuest.Title + "»",
                    [
                        FlagEquals(relationship.DecisionFlagId, "SUPPORT"),
                        QuestState(step.QuestId, "completed"),
                        QuestState(next.QuestId, "not_started")
                    ],
                    next.QuestId,
                    next.Order));
            }
            else
            {
                followUps.Add(FollowUp(
                    relationship,
                    "completed",
                    "Отношения завершены: все задания выполнены.",
                    [
                        FlagEquals(relationship.DecisionFlagId, "SUPPORT"),
                        QuestState(step.QuestId, "completed")
                    ],
                    null,
                    step.Order));
            }
        }
        start.Choices = retained.Concat(followUps)
            .OrderBy(item => ChoiceOrder(item, support.Id,
                challenge?.Id, refuse.Id))
            .ThenBy(item => item.Id, StringComparer.Ordinal)
            .ToList();
        dialogue.Tags = dialogue.Tags.Append("generated_relationship")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        dialogue.Metadata["generatedRelationshipId"] =
            relationship.RelationshipId;
        dialogue.Metadata["generatedRelationshipActorSeedId"] =
            relationship.ActorSeedId;
        dialogue.Metadata["generatedRelationshipFactionId"] =
            relationship.FactionId;
        dialogue.Metadata["generatedRelationshipArcLength"] =
            relationship.QuestArc.Count.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        dialogue.Metadata["generatedRelationshipOrderedQuestIds"] =
            string.Join(",", relationship.QuestArc.Select(item =>
                item.QuestId));
    }

    private static DialogueChoiceDefinition InitialRelationshipChoice(
        GeneratedCampaignRelationshipBranch branch,
        GeneratedCampaignRelationshipBinding relationship)
    {
        var amount = branch ==
                     GeneratedCampaignRelationshipBranch.SUPPORT
            ? relationship.SupportReputationAmount
            : relationship.RefuseReputationAmount;
        var value = branch.ToString();
        return new DialogueChoiceDefinition
        {
            Id = relationship.RelationshipId + "/generatedChoice/"
                 + value.ToLowerInvariant(),
            Text = branch ==
                   GeneratedCampaignRelationshipBranch.SUPPORT
                ? "Поддержать предложение"
                : "Отказаться от предложения",
            Requirements =
            [
                FlagEquals(relationship.DecisionFlagId, string.Empty)
            ],
            Effects =
            [
                new LLMGameCreator.Domain.Definitions.EffectDefinition
                {
                    Type = "set_flag",
                    Args = new Dictionary<string, string>(
                        StringComparer.Ordinal)
                    {
                        ["id"] = relationship.DecisionFlagId,
                        ["value"] = value
                    }
                },
                new LLMGameCreator.Domain.Definitions.EffectDefinition
                {
                    Type = "change_reputation",
                    Args = new Dictionary<string, string>(
                        StringComparer.Ordinal)
                    {
                        ["id"] = relationship.FactionId,
                        ["amount"] = amount.ToString(
                            System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            ],
            CloseDialogue = true,
            Tags =
            [
                "generated_choice",
                value.ToLowerInvariant(),
                "generated_relationship"
            ],
            Metadata = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["generatedChoiceKind"] = value,
                ["generatedChoicePhase"] = "initial",
                ["generatedChoiceFactionId"] = relationship.FactionId,
                ["generatedChoiceQuestId"] =
                    relationship.QuestArc[0].QuestId,
                ["generatedChoiceEncounterId"] = string.Empty,
                ["generatedChoiceReputationAmount"] = amount.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            }
        };
    }

    private static DialogueChoiceDefinition FollowUp(
        GeneratedCampaignRelationshipBinding relationship,
        string phase,
        string text,
        List<DomainRequirementDefinition> requirements,
        string? startQuestId,
        int arcOrder)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        AddRelationshipMetadata(metadata, relationship,
            "followup/" + phase, arcOrder);
        metadata["generatedChoiceKind"] = "SUPPORT";
        metadata["generatedChoicePhase"] = "followup";
        metadata["generatedChoiceFactionId"] = relationship.FactionId;
        metadata["generatedChoiceQuestId"] = startQuestId ?? string.Empty;
        metadata["generatedChoiceEncounterId"] = string.Empty;
        return new DialogueChoiceDefinition
        {
            Id = "generatedChoice/support/followup/" + phase,
            Text = text,
            Requirements = requirements,
            StartQuestId = startQuestId,
            CloseDialogue = true,
            Tags = ["generated_choice_followup", "support",
                "generated_relationship"],
            Metadata = metadata
        };
    }

    private static void AddRelationshipMetadata(
        IDictionary<string, string> metadata,
        GeneratedCampaignRelationshipBinding relationship,
        string phase,
        int arcOrder)
    {
        metadata["generatedRelationshipId"] =
            relationship.RelationshipId;
        metadata["generatedRelationshipPhase"] = phase;
        metadata["generatedRelationshipActorSeedId"] =
            relationship.ActorSeedId;
        metadata["generatedRelationshipFactionId"] =
            relationship.FactionId;
        metadata["generatedRelationshipArcOrder"] =
            arcOrder.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool IsChoice(
        DialogueChoiceDefinition choice,
        GeneratedCampaignRelationshipBranch branch,
        string phase)
    {
        if (!choice.Metadata.TryGetValue("generatedChoiceKind",
                out var kind)
            || !string.Equals(kind, branch.ToString(),
                StringComparison.Ordinal))
            return false;
        var choicePhase = choice.Metadata.GetValueOrDefault(
            "generatedChoicePhase") ?? string.Empty;
        return phase == "initial"
            ? string.Equals(choicePhase, "initial",
                StringComparison.Ordinal)
            : choicePhase.StartsWith("followup",
                StringComparison.Ordinal);
    }

    private static int ChoiceOrder(
        DialogueChoiceDefinition choice,
        string supportId,
        string? challengeId,
        string refuseId)
    {
        if (choice.Id == supportId) return 0;
        if (choice.Id == challengeId) return 1;
        if (choice.Id == refuseId) return 2;
        return 3;
    }

    private static DomainRequirementDefinition FlagEquals(
        string id,
        string value) => new()
    {
        Kind = "flag_equals",
        Id = id,
        Value = value
    };

    private static DomainRequirementDefinition QuestState(
        string id,
        string value) => new()
    {
        Kind = "quest_state",
        Id = id,
        Value = value
    };

    private static void ValidateControlledDelta(
        GamePackageDefinition before,
        GamePackageDefinition after,
        IReadOnlySet<string> relationshipIds,
        IReadOnlySet<string> assignedQuestIds,
        ICollection<string> diagnostics)
    {
        if (!Same(WithoutDialoguesAndQuests(before),
                WithoutDialoguesAndQuests(after)))
            diagnostics.Add(
                "generated_relationship.delta_non_dialogue_quest_changed");
        var beforeDialogues = before.Game.Dialogues.ToDictionary(
            item => item.Id, StringComparer.Ordinal);
        var afterDialogues = after.Game.Dialogues.ToDictionary(
            item => item.Id, StringComparer.Ordinal);
        var beforeQuests = before.Game.Quests.ToDictionary(
            item => item.Id, StringComparer.Ordinal);
        var afterQuests = after.Game.Quests.ToDictionary(
            item => item.Id, StringComparer.Ordinal);
        if (!SameKeys(beforeDialogues, afterDialogues)
            || !SameKeys(beforeQuests, afterQuests))
        {
            diagnostics.Add(
                "generated_relationship.delta_definition_identity_changed");
            return;
        }
        foreach (var id in beforeDialogues.Keys)
        {
            if (!relationshipIds.Contains(id)
                && !Same(beforeDialogues[id], afterDialogues[id]))
                diagnostics.Add(
                    "generated_relationship.delta_unbound_dialogue_changed");
            if (relationshipIds.Contains(id)
                && !Same(NormalizeDialogue(beforeDialogues[id]),
                    NormalizeDialogue(afterDialogues[id])))
                diagnostics.Add(
                    "generated_relationship.delta_dialogue_forbidden_field");
        }
        foreach (var id in beforeQuests.Keys)
        {
            if (!assignedQuestIds.Contains(id)
                && !Same(beforeQuests[id], afterQuests[id]))
                diagnostics.Add(
                    "generated_relationship.delta_unassigned_quest_changed");
            if (assignedQuestIds.Contains(id)
                && !Same(NormalizeQuest(beforeQuests[id]),
                    NormalizeQuest(afterQuests[id])))
                diagnostics.Add(
                    "generated_relationship.delta_quest_forbidden_field");
            if (assignedQuestIds.Contains(id)
                && (afterQuests[id].AutoStart
                    || string.IsNullOrWhiteSpace(afterQuests[id].Metadata
                        .GetValueOrDefault("generatedRelationshipId"))))
                diagnostics.Add(
                    "generated_relationship.delta_assigned_quest_state_invalid");
        }
    }

    private static object WithoutDialoguesAndQuests(
        GamePackageDefinition value)
    {
        var clone = GeneratedCampaignChoiceCanonical.Clone(value);
        clone.Game.Dialogues = [];
        clone.Game.Quests = [];
        return clone;
    }

    private static DialogueDefinition NormalizeDialogue(
        DialogueDefinition value)
    {
        var clone = GeneratedCampaignChoiceCanonical.Clone(value);
        clone.Nodes = [];
        clone.Tags = clone.Tags.Where(item => item !=
                "generated_relationship")
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        clone.Metadata = clone.Metadata.Where(item =>
                !item.Key.StartsWith("generatedRelationship",
                    StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value,
                StringComparer.Ordinal);
        return clone;
    }

    private static QuestDefinition NormalizeQuest(QuestDefinition value)
    {
        var clone = GeneratedCampaignChoiceCanonical.Clone(value);
        clone.AutoStart = false;
        clone.Metadata = clone.Metadata.Where(item =>
                !item.Key.StartsWith("generatedRelationship",
                    StringComparison.Ordinal))
            .ToDictionary(item => item.Key, item => item.Value,
                StringComparer.Ordinal);
        return clone;
    }

    private static IReadOnlyList<
        GeneratedCampaignRelationshipDefinitionFingerprint> Fingerprints(
        GamePackageDefinition package,
        IReadOnlySet<string> relationshipIds,
        IReadOnlySet<string> questIds)
    {
        var result = package.Game.Dialogues
            .Where(item => relationshipIds.Contains(item.Id))
            .Select(item =>
                new GeneratedCampaignRelationshipDefinitionFingerprint
                {
                    CollectionPath = "game.dialogues",
                    DefinitionId = item.Id,
                    CanonicalSha256 =
                        GeneratedCampaignChoiceCanonical.Hash(item)
                })
            .Concat(package.Game.Quests
                .Where(item => questIds.Contains(item.Id))
                .Select(item =>
                    new GeneratedCampaignRelationshipDefinitionFingerprint
                    {
                        CollectionPath = "game.quests",
                        DefinitionId = item.Id,
                        CanonicalSha256 =
                            GeneratedCampaignChoiceCanonical.Hash(item)
                    }))
            .OrderBy(item => item.CollectionPath, StringComparer.Ordinal)
            .ThenBy(item => item.DefinitionId, StringComparer.Ordinal)
            .ToList();
        return result;
    }

    private static IReadOnlyList<GeneratedCampaignRelationshipInventoryRow>
        Inventory(IEnumerable<GeneratedCampaignRelationshipBinding> bindings) =>
        bindings.OrderBy(item => item.RelationshipId, StringComparer.Ordinal)
            .Select(item => new GeneratedCampaignRelationshipInventoryRow
            {
                RelationshipId = item.RelationshipId,
                ActorSeedId = item.ActorSeedId,
                FactionId = item.FactionId,
                BranchKinds = item.Branches.OrderBy(value => value).ToList(),
                OrderedQuestSourceIds = item.QuestArc
                    .OrderBy(step => step.Order)
                    .Select(step => step.QuestSourceId).ToList()
            }).ToList();

    private static IReadOnlyDictionary<string, int> DefinitionCounts(
        GamePackageDefinition package)
    {
        using var json = JsonDocument.Parse(
            GeneratedCampaignChoiceCanonical.Serialize(package.Game));
        return json.RootElement.EnumerateObject()
            .Where(item => item.Value.ValueKind == JsonValueKind.Array)
            .ToDictionary(item => "game." + item.Name,
                item => item.Value.GetArrayLength(), StringComparer.Ordinal);
    }

    private static bool Same<T>(T left, T right) =>
        string.Equals(GeneratedCampaignChoiceCanonical.Serialize(left),
            GeneratedCampaignChoiceCanonical.Serialize(right),
            StringComparison.Ordinal);

    private static bool SameKeys<T>(
        IReadOnlyDictionary<string, T> left,
        IReadOnlyDictionary<string, T> right) =>
        left.Keys.OrderBy(item => item, StringComparer.Ordinal)
            .SequenceEqual(right.Keys.OrderBy(item => item,
                StringComparer.Ordinal), StringComparer.Ordinal);

    private static string PackageSha256(GamePackageDefinition package) =>
        GeneratedCampaignChoiceCanonical.HashText(
            GeneratedCampaignChoiceCanonical.Serialize(package)
            + Environment.NewLine);

    private static GeneratedCampaignRelationshipOverlayResult Failed(
        IReadOnlyList<string> diagnostics) => new()
    {
        Diagnostics = diagnostics
    };
}
