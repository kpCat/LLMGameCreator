using System.Globalization;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignDialogueChoicePreviewService
{
    private readonly IUnifiedGameRuntimeService _runtime;

    public GeneratedCampaignDialogueChoicePreviewService(IUnifiedGameRuntimeService runtime) => _runtime = runtime;

    public GeneratedCampaignDialogueChoicePreview? Preview(GamePackageDefinition package, UnifiedRuntimeSession session)
    {
        var active = session.GameplayState.ActiveDialogue;
        if (active is not { Open: true }) return null;
        var dialogue = package.Game.Dialogues.SingleOrDefault(item => item.Id == active.DialogueId);
        var node = dialogue?.Nodes.SingleOrDefault(item => item.Id == active.CurrentNodeId);
        if (dialogue is null || node is null) return null;

        var original = GeneratedCampaignChoiceCanonical.Hash(session);
        var packageSha = GeneratedCampaignChoiceCanonical.Hash(package);
        var available = RuntimeAvailableChoiceIds(session, dialogue.Id, node.Id);
        var options = node.Choices.Select((choice, index) =>
        {
            var copy = GeneratedCampaignChoiceCanonical.Clone(session);
            var beforeState = GeneratedCampaignChoiceCanonical.Hash(copy.GameplayState);
            var result = _runtime.ExecuteGameplayCommand(package, copy,
                GameRuntimeCommand.ChooseDialogueOption(choice.Id));
            var enabled = available.Contains(choice.Id) && result.Success;
            var observed = Observe(package, session, result.Session, dialogue.Id, result.GameplayEvents);
            var labelKind = observed.Kind ?? MetadataKind(choice.Metadata);
            return new GeneratedCampaignChoiceOption
            {
                Title = choice.Text,
                Description = enabled ? "Последствия проверены в Runtime." : HumanFailure(result),
                ActorTitle = dialogue.Title,
                FactionTitle = observed.FactionId is { Length: > 0 } factionId
                    ? package.Game.Factions.SingleOrDefault(item => item.Id == factionId)?.Name ?? factionId
                    : Label(package, choice.Metadata, "generatedChoiceFactionId", "faction") ?? string.Empty,
                QuestTitle = observed.QuestId is { Length: > 0 } questId
                    ? package.Game.Quests.SingleOrDefault(item => item.Id == questId)?.Title ?? questId
                    : Label(package, choice.Metadata, "generatedChoiceQuestId", "quest") ?? string.Empty,
                EncounterTitle = observed.EncounterId is { Length: > 0 } encounterId
                    ? package.Game.Encounters.SingleOrDefault(item => item.Id == encounterId)?.Name ?? encounterId
                    : Label(package, choice.Metadata, "generatedChoiceEncounterId", "encounter") ?? string.Empty,
                ConsequencePreview = observed.Rows,
                Enabled = enabled,
                DisabledReason = enabled ? string.Empty : available.Contains(choice.Id)
                    ? HumanFailure(result)
                    : "Этот вариант сейчас недоступен по состоянию Runtime.",
                BranchKind = labelKind,
                Primary = index == 0 && enabled,
                TechnicalChoiceId = choice.Id,
                BeforeStateHash = beforeState,
                AfterStateHash = GeneratedCampaignChoiceCanonical.Hash(result.Session.GameplayState),
                ObservedFlagValue = observed.FlagValue,
                ObservedReputationDelta = observed.ReputationDelta,
                ObservedQuestState = observed.QuestState,
                ObservedEncounterId = observed.EncounterId,
                RuntimeEventTypes = result.GameplayEvents.Select(item => item.Type.ToString()).ToList()
            };
        }).ToList();

        if (original != GeneratedCampaignChoiceCanonical.Hash(session)
            || packageSha != GeneratedCampaignChoiceCanonical.Hash(package))
            throw new InvalidOperationException("generated_choice.preview_mutated_original");
        return new GeneratedCampaignDialogueChoicePreview
        {
            DialogueId = dialogue.Id,
            OriginalSessionSha256 = original,
            PackageSha256 = packageSha,
            RuntimeAvailableChoiceIds = available.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            Options = options
        };
    }

    private static HashSet<string> RuntimeAvailableChoiceIds(
        UnifiedRuntimeSession session,
        string dialogueId,
        string nodeId)
    {
        var runtimeEvent = session.GameplayEvents.LastOrDefault(item =>
            item.Type is GameRuntimeEventType.DialogueOpened or GameRuntimeEventType.DialogueNodeChanged
            && item.Args.GetValueOrDefault("dialogueId") == dialogueId
            && item.Args.GetValueOrDefault("nodeId") == nodeId);
        return runtimeEvent?.Args.TryGetValue("choiceIds", out var raw) == true
            ? raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.Ordinal)
            : [];
    }

    private static ObservedChoiceConsequences Observe(
        GamePackageDefinition package,
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        string dialogueId,
        IReadOnlyList<GameRuntimeEvent> events)
    {
        var rows = new List<string>();
        var beforeFlag = before.GameplayState.Flags.SingleOrDefault(item => item.Id == dialogueId)?.Value
                         ?? string.Empty;
        var flag = after.GameplayState.Flags.SingleOrDefault(item => item.Id == dialogueId)?.Value
                   ?? string.Empty;
        GeneratedCampaignBranchKind? kind = Enum.TryParse<GeneratedCampaignBranchKind>(flag, out var parsed)
            ? parsed
            : null;
        if (flag != beforeFlag && kind is not null) rows.Add("Решение: " + kind.Value.ToString().ToLowerInvariant());

        string factionId = string.Empty;
        double reputationDelta = 0;
        foreach (var faction in after.GameplayState.Factions)
        {
            var previous = before.GameplayState.Factions.SingleOrDefault(item => item.FactionId == faction.FactionId)
                ?.Reputation ?? 0;
            if (Math.Abs(previous - faction.Reputation) < 0.0000001) continue;
            factionId = faction.FactionId;
            reputationDelta = faction.Reputation - previous;
            rows.Add("Репутация: " + reputationDelta.ToString("+0.##;-0.##", CultureInfo.InvariantCulture));
            break;
        }

        string questId = string.Empty;
        string questState = string.Empty;
        foreach (var quest in after.GameplayState.Quests)
        {
            var previous = before.GameplayState.Quests.SingleOrDefault(item => item.QuestId == quest.QuestId)?.State
                           ?? "not_started";
            if (previous == quest.State) continue;
            questId = quest.QuestId;
            questState = quest.State;
            var title = package.Game.Quests.SingleOrDefault(item => item.Id == quest.QuestId)?.Title ?? quest.QuestId;
            rows.Add("Задание: " + title + " — " + quest.State);
            break;
        }

        var beforeEncounter = before.GameplayState.ActiveEncounter;
        var afterEncounter = after.GameplayState.ActiveEncounter;
        var encounterId = events.Any(item => item.Type == GameRuntimeEventType.EncounterStarted)
                          && afterEncounter is { Active: true }
                          && (beforeEncounter is null || beforeEncounter.EncounterId != afterEncounter.EncounterId)
            ? afterEncounter.EncounterId
            : string.Empty;
        if (encounterId.Length > 0)
        {
            var title = package.Game.Encounters.SingleOrDefault(item => item.Id == encounterId)?.Name ?? encounterId;
            rows.Add("Встреча: " + title + " — active");
        }
        return new ObservedChoiceConsequences(kind, flag, factionId, reputationDelta, questId,
            questState, encounterId, rows);
    }

    private static GeneratedCampaignBranchKind? MetadataKind(IReadOnlyDictionary<string, string> metadata) =>
        metadata.TryGetValue("generatedChoiceKind", out var value)
        && Enum.TryParse<GeneratedCampaignBranchKind>(value, out var kind)
            ? kind
            : null;

    private static string? Label(
        GamePackageDefinition package,
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string kind)
    {
        if (!metadata.TryGetValue(key, out var id) || string.IsNullOrWhiteSpace(id)) return null;
        return kind switch
        {
            "faction" => package.Game.Factions.SingleOrDefault(item => item.Id == id)?.Name ?? id,
            "quest" => package.Game.Quests.SingleOrDefault(item => item.Id == id)?.Title ?? id,
            "encounter" => package.Game.Encounters.SingleOrDefault(item => item.Id == id)?.Name ?? id,
            _ => id
        };
    }

    private static string HumanFailure(UnifiedRuntimeResult result) => result.Success
        ? "Этот вариант сейчас недоступен по состоянию Runtime."
        : "Условия этого варианта сейчас не выполнены.";

    private sealed record ObservedChoiceConsequences(
        GeneratedCampaignBranchKind? Kind,
        string FlagValue,
        string FactionId,
        double ReputationDelta,
        string QuestId,
        string QuestState,
        string EncounterId,
        IReadOnlyList<string> Rows);
}
