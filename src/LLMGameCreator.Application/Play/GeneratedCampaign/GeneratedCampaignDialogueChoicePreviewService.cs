using LLMGameCreator.GamePackage;
using LLMGameCreator.Application.Generation.Procedural;
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
        var options = node.Choices.Select((choice, index) =>
        {
            var copy = GeneratedCampaignChoiceCanonical.Clone(session);
            var result = _runtime.ExecuteGameplayCommand(package, copy, GameRuntimeCommand.ChooseDialogueOption(choice.Id));
            var metadata = choice.Metadata;
            metadata.TryGetValue("generatedChoiceKind", out var kindText);
            GeneratedCampaignBranchKind? kind = Enum.TryParse<GeneratedCampaignBranchKind>(kindText, out var parsed)
                ? parsed : null;
            var faction = metadata.GetValueOrDefault("generatedChoiceFactionId", string.Empty);
            var quest = metadata.GetValueOrDefault("generatedChoiceQuestId", string.Empty);
            var encounter = metadata.GetValueOrDefault("generatedChoiceEncounterId", string.Empty);
            return new GeneratedCampaignChoiceOption
            {
                Title = choice.Text,
                Description = result.Success ? "Последствия проверены в Runtime." : HumanFailure(result),
                ActorTitle = dialogue.Title,
                FactionTitle = package.Game.Factions.SingleOrDefault(item => item.Id == faction)?.Name ?? string.Empty,
                QuestTitle = package.Game.Quests.SingleOrDefault(item => item.Id == quest)?.Title,
                EncounterTitle = package.Game.Encounters.SingleOrDefault(item => item.Id == encounter)?.Name,
                ConsequencePreview = Consequences(kind, metadata, package),
                Enabled = result.Success,
                DisabledReason = result.Success ? string.Empty : HumanFailure(result),
                BranchKind = kind,
                Primary = index == 0,
                TechnicalChoiceId = choice.Id
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
            Options = options
        };
    }

    private static IReadOnlyList<string> Consequences(GeneratedCampaignBranchKind? kind,
        IReadOnlyDictionary<string, string> metadata, GamePackageDefinition package)
    {
        if (kind is null) return [];
        var rows = new List<string> { "Решение: " + kind.Value.ToString().ToLowerInvariant() };
        if (metadata.TryGetValue("generatedChoiceReputationAmount", out var reputation)
            && double.TryParse(reputation, System.Globalization.CultureInfo.InvariantCulture, out var amount)
            && Math.Abs(amount) > 0) rows.Add("Репутация: " + amount.ToString("+0.##;-0.##", System.Globalization.CultureInfo.InvariantCulture));
        if (metadata.TryGetValue("generatedChoiceQuestId", out var quest) && !string.IsNullOrWhiteSpace(quest))
            rows.Add("Задание: " + (package.Game.Quests.SingleOrDefault(item => item.Id == quest)?.Title ?? "задание"));
        if (metadata.TryGetValue("generatedChoiceEncounterId", out var encounter) && !string.IsNullOrWhiteSpace(encounter))
            rows.Add("Встреча: " + (package.Game.Encounters.SingleOrDefault(item => item.Id == encounter)?.Name ?? "встреча"));
        return rows;
    }
    private static string HumanFailure(UnifiedRuntimeResult result) => result.Diagnostics.FirstOrDefault()?.Message
        ?? "Этот вариант сейчас недоступен.";
}
