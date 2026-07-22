using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using DomainEffectDefinition = LLMGameCreator.Domain.Definitions.EffectDefinition;
using DomainRequirementDefinition = LLMGameCreator.Domain.Definitions.RequirementDefinition;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedCampaignChoiceOverlayService
{
    private static readonly IReadOnlyList<string> AllowedFieldPaths =
    [
        "game.dialogues[*].nodes",
        "game.dialogues[*].metadata.generatedChoice*",
        "game.dialogues[*].tags.generated_choice_branching"
    ];

    public GeneratedCampaignChoiceOverlayResult Build(
        GamePackageDefinition preChoicePackage,
        GeneratedCampaignChoiceBindingResult bindingResult)
    {
        ArgumentNullException.ThrowIfNull(preChoicePackage);
        ArgumentNullException.ThrowIfNull(bindingResult);
        if (!bindingResult.Passed)
            return Failed(bindingResult.Diagnostics.Count == 0 ? ["generated_choice.binding_invalid"] : bindingResult.Diagnostics);

        var bound = bindingResult.Bindings.Where(item => item.Branches.Count > 0)
            .OrderBy(item => item.DialogueId, StringComparer.Ordinal).ToList();
        var before = GeneratedCampaignChoiceCanonical.Clone(preChoicePackage);
        var after = GeneratedCampaignChoiceCanonical.Clone(preChoicePackage);
        var boundIds = bound.Select(item => item.DialogueId).ToHashSet(StringComparer.Ordinal);
        var diagnostics = new List<string>();
        foreach (var binding in bound)
        {
            var dialogues = after.Game.Dialogues.Where(item => item.Id == binding.DialogueId).ToList();
            if (dialogues.Count != 1)
            {
                diagnostics.Add("generated_choice.dialogue_mapping_missing");
                continue;
            }
            Apply(dialogues[0], binding);
        }
        after.Game.Dialogues = after.Game.Dialogues.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
        var countsBefore = DefinitionCounts(before);
        var countsAfter = DefinitionCounts(after);
        if (!countsBefore.OrderBy(item => item.Key, StringComparer.Ordinal)
                .SequenceEqual(countsAfter.OrderBy(item => item.Key, StringComparer.Ordinal)))
            diagnostics.Add("generated_choice.delta_unexpected_collection_change");
        ValidateControlledDelta(before, after, boundIds, diagnostics);
        ValidateReferences(after, bound, diagnostics);
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
        var json = GeneratedCampaignChoiceCanonical.Serialize(after) + Environment.NewLine;
        var document = new GeneratedCampaignChoiceOverlayDocument
        {
            SourcePackageSha256 = GeneratedCampaignChoiceCanonical.HashText(GeneratedCampaignChoiceCanonical.Serialize(before) + Environment.NewLine),
            OutputPackageSha256 = GeneratedCampaignChoiceCanonical.HashText(json),
            GeneratedDialogueCount = bindingResult.Bindings.Count,
            BranchableDialogueCount = bound.Count,
            QualifiedBranchCount = bound.Sum(item => item.Branches.Count),
            Bindings = bindingResult.Bindings,
            DialogueFingerprintsBefore = Fingerprints(before, boundIds),
            DialogueFingerprintsAfter = Fingerprints(after, boundIds),
            FlagInventory = bound.Select(item => new GeneratedCampaignChoiceFlagInventoryRow
            {
                DialogueId = item.DialogueId,
                SupportedBranchKinds = item.Branches.Select(branch => branch.Kind).Distinct()
                    .OrderBy(kind => kind).ToList()
            }).ToList(),
            AllowedFieldPaths = AllowedFieldPaths,
            DefinitionCollectionCountsBefore = countsBefore,
            DefinitionCollectionCountsAfter = countsAfter,
            Diagnostics = diagnostics,
            Passed = diagnostics.Count == 0
        };
        return new GeneratedCampaignChoiceOverlayResult
        {
            Passed = document.Passed,
            ChoiceOverlayPackage = after,
            ChoiceOverlayPackageJson = json,
            Document = document,
            Diagnostics = diagnostics
        };
    }

    public GeneratedCampaignChoiceOverlayValidationResult ValidateFinalPackage(
        GamePackageDefinition preChoicePackage,
        GamePackageDefinition finalPackage,
        GeneratedCampaignChoiceOverlayDocument overlay)
    {
        ArgumentNullException.ThrowIfNull(preChoicePackage);
        ArgumentNullException.ThrowIfNull(finalPackage);
        ArgumentNullException.ThrowIfNull(overlay);
        var diagnostics = new List<string>();
        var boundIds = overlay.Bindings.Where(item => item.Branches.Count > 0)
            .Select(item => item.DialogueId).ToHashSet(StringComparer.Ordinal);
        ValidateControlledDelta(preChoicePackage, finalPackage, boundIds, diagnostics);
        ValidateReferences(finalPackage, overlay.Bindings.Where(item => item.Branches.Count > 0).ToList(), diagnostics);
        var expectedFlags = overlay.Bindings.Where(item => item.Branches.Count > 0)
            .OrderBy(item => item.DialogueId, StringComparer.Ordinal)
            .Select(item => new GeneratedCampaignChoiceFlagInventoryRow
            {
                DialogueId = item.DialogueId,
                SupportedBranchKinds = item.Branches.Select(branch => branch.Kind).Distinct()
                    .OrderBy(kind => kind).ToList()
            }).ToList();
        if (!Same(expectedFlags, overlay.FlagInventory))
            diagnostics.Add("generated_choice.flag_inventory_mismatch");
        if (!Same(Fingerprints(finalPackage, boundIds), overlay.DialogueFingerprintsAfter))
            diagnostics.Add("generated_choice.dialogue_fingerprint_mismatch");
        diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToList();
        return new GeneratedCampaignChoiceOverlayValidationResult
        {
            Passed = diagnostics.Count == 0,
            Diagnostics = diagnostics
        };
    }

    private static void Apply(DialogueDefinition dialogue, GeneratedCampaignChoiceBinding binding)
    {
        var start = dialogue.Nodes.SingleOrDefault(item => item.Id == dialogue.StartNodeId)
            ?? throw new InvalidOperationException("generated_choice.start_node_missing:" + dialogue.Id);
        var initial = binding.Branches.OrderBy(item => item.Kind).Select(branch => new DialogueChoiceDefinition
        {
            Id = branch.ChoiceId,
            Text = branch.Title,
            Requirements = [FlagEquals(dialogue.Id, string.Empty)],
            Effects = Effects(dialogue.Id, branch),
            StartQuestId = branch.Kind == GeneratedCampaignBranchKind.SUPPORT ? branch.QuestId : null,
            StartEncounterId = branch.EncounterId,
            CloseDialogue = true,
            Tags = ["generated_choice", branch.Kind.ToString().ToLowerInvariant()],
            Metadata = Metadata(branch, "initial")
        });
        var followUps = binding.Branches.OrderBy(item => item.Kind).SelectMany(branch => FollowUps(dialogue, branch));
        start.Choices = initial.Concat(followUps).ToList();
        dialogue.Tags = dialogue.Tags.Append("generated_choice_branching").Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal).ToList();
        dialogue.Metadata["generatedChoiceActorSeedId"] = binding.ActorSeedId;
        dialogue.Metadata["generatedChoiceFactionId"] = binding.FactionId;
        dialogue.Metadata["generatedChoiceBranchKinds"] = string.Join(",", binding.Branches.Select(item => item.Kind).OrderBy(item => item));
    }

    private static IEnumerable<DialogueChoiceDefinition> FollowUps(DialogueDefinition dialogue, GeneratedCampaignChoiceBranch branch)
    {
        if (branch.Kind == GeneratedCampaignBranchKind.SUPPORT && branch.QuestId is { Length: > 0 } questId)
        {
            yield return FollowUp(dialogue, branch, "active", "Задание продолжается: «" + branch.Title + "»",
                [FlagEquals(dialogue.Id, branch.FlagValue), QuestState(questId, "active")]);
            yield return FollowUp(dialogue, branch, "completed", "Поддержка завершена: «" + branch.Title + "»",
                [FlagEquals(dialogue.Id, branch.FlagValue), QuestState(questId, "completed")]);
            yield break;
        }
        yield return FollowUp(dialogue, branch, "chosen", "Вы выбрали: «" + branch.Title + "»",
            [FlagEquals(dialogue.Id, branch.FlagValue)]);
    }

    private static DialogueChoiceDefinition FollowUp(DialogueDefinition dialogue, GeneratedCampaignChoiceBranch branch,
        string state, string text, List<DomainRequirementDefinition> requirements) => new()
    {
        Id = branch.ChoiceId + "/followup/" + state,
        Text = text,
        Requirements = requirements,
        CloseDialogue = true,
        Tags = ["generated_choice_followup", branch.Kind.ToString().ToLowerInvariant()],
        Metadata = Metadata(branch, "followup/" + state)
    };

    private static List<DomainEffectDefinition> Effects(string dialogueId, GeneratedCampaignChoiceBranch branch)
    {
        var values = new List<DomainEffectDefinition>
        {
            new() { Type = "set_flag", Args = new Dictionary<string, string>(StringComparer.Ordinal)
                { ["id"] = dialogueId, ["value"] = branch.FlagValue } }
        };
        if (Math.Abs(branch.ReputationAmount) > 0)
            values.Add(new DomainEffectDefinition { Type = "change_reputation", Args = new Dictionary<string, string>(StringComparer.Ordinal)
                { ["id"] = branch.FactionId, ["amount"] = branch.ReputationAmount.ToString(System.Globalization.CultureInfo.InvariantCulture) } });
        return values;
    }

    private static Dictionary<string, string> Metadata(GeneratedCampaignChoiceBranch branch, string phase) => new(StringComparer.Ordinal)
    {
        ["generatedChoiceKind"] = branch.Kind.ToString(),
        ["generatedChoicePhase"] = phase,
        ["generatedChoiceFactionId"] = branch.FactionId,
        ["generatedChoiceQuestId"] = branch.QuestId ?? string.Empty,
        ["generatedChoiceEncounterId"] = branch.EncounterId ?? string.Empty,
        ["generatedChoiceReputationAmount"] = branch.ReputationAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)
    };

    private static DomainRequirementDefinition FlagEquals(string dialogueId, string value) => new()
        { Kind = "flag_equals", Id = dialogueId, Value = value };
    private static DomainRequirementDefinition QuestState(string questId, string value) => new()
        { Kind = "quest_state", Id = questId, Value = value };

    private static void ValidateReferences(GamePackageDefinition package, IReadOnlyList<GeneratedCampaignChoiceBinding> bindings,
        ICollection<string> diagnostics)
    {
        foreach (var branch in bindings.SelectMany(item => item.Branches))
        {
            if (package.Game.Factions.Count(item => item.Id == branch.FactionId) != 1)
                diagnostics.Add("generated_choice.faction_missing");
            if (branch.QuestId is { Length: > 0 } && package.Game.Quests.Count(item => item.Id == branch.QuestId) != 1)
                diagnostics.Add("generated_choice.quest_missing");
            if (branch.EncounterId is { Length: > 0 } && package.Game.Encounters.Count(item => item.Id == branch.EncounterId) != 1)
                diagnostics.Add("generated_choice.encounter_missing");
        }
    }

    private static void ValidateControlledDelta(GamePackageDefinition before, GamePackageDefinition after,
        IReadOnlySet<string> boundIds, ICollection<string> diagnostics)
    {
        if (!Same(WithoutDialogues(before), WithoutDialogues(after)))
            diagnostics.Add("generated_choice.delta_non_dialogue_changed");
        var left = before.Game.Dialogues.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var right = after.Game.Dialogues.ToDictionary(item => item.Id, StringComparer.Ordinal);
        if (!left.Keys.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(right.Keys.OrderBy(item => item, StringComparer.Ordinal)))
        {
            diagnostics.Add("generated_choice.delta_dialogue_identity_changed");
            return;
        }
        foreach (var id in left.Keys)
        {
            if (!boundIds.Contains(id) && !Same(left[id], right[id]))
                diagnostics.Add("generated_choice.delta_non_generated_dialogue_changed");
            if (boundIds.Contains(id) && (!string.Equals(left[id].Id, right[id].Id, StringComparison.Ordinal)
                || !string.Equals(left[id].Title, right[id].Title, StringComparison.Ordinal)
                || !string.Equals(left[id].StartNodeId, right[id].StartNodeId, StringComparison.Ordinal)
                || !string.Equals(left[id].BackgroundAssetId, right[id].BackgroundAssetId, StringComparison.Ordinal)
                || SourceChanged(left[id], right[id])))
                diagnostics.Add("generated_choice.delta_dialogue_identity_changed");
        }
    }

    private static object WithoutDialogues(GamePackageDefinition value)
    {
        var clone = GeneratedCampaignChoiceCanonical.Clone(value);
        clone.Game.Dialogues = [];
        return clone;
    }
    private static bool SourceChanged(DialogueDefinition left, DialogueDefinition right) =>
        !string.Equals(left.Metadata.GetValueOrDefault("sourceActorSeedId"), right.Metadata.GetValueOrDefault("sourceActorSeedId"), StringComparison.Ordinal)
        || !string.Equals(left.Metadata.GetValueOrDefault("sourceRegionId"), right.Metadata.GetValueOrDefault("sourceRegionId"), StringComparison.Ordinal);
    private static bool Same<T>(T left, T right) => string.Equals(GeneratedCampaignChoiceCanonical.Serialize(left),
        GeneratedCampaignChoiceCanonical.Serialize(right), StringComparison.Ordinal);
    private static IReadOnlyList<GeneratedCampaignChoiceDialogueFingerprint> Fingerprints(GamePackageDefinition package,
        IReadOnlySet<string> ids) => package.Game.Dialogues.Where(item => ids.Contains(item.Id)).OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => new GeneratedCampaignChoiceDialogueFingerprint { DialogueId = item.Id, CanonicalSha256 = GeneratedCampaignChoiceCanonical.Hash(item) }).ToList();
    private static IReadOnlyDictionary<string, int> DefinitionCounts(GamePackageDefinition package)
    {
        using var json = JsonDocument.Parse(GeneratedCampaignChoiceCanonical.Serialize(package.Game));
        return json.RootElement.EnumerateObject().Where(item => item.Value.ValueKind == JsonValueKind.Array)
            .ToDictionary(item => "game." + item.Name, item => item.Value.GetArrayLength(), StringComparer.Ordinal);
    }
    private static GeneratedCampaignChoiceOverlayResult Failed(IReadOnlyList<string> diagnostics) => new() { Diagnostics = diagnostics };
}
