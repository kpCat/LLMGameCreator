using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Application.Generation.Procedural;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignDecisionJournalService
{
    public GeneratedCampaignDecisionJournal Project(GamePackageDefinition package, UnifiedRuntimeSession session)
    {
        var rows = new List<GeneratedCampaignDecision>();
        foreach (var dialogue in package.Game.Dialogues.Where(item => item.Tags.Contains("generated_choice_branching", StringComparer.Ordinal)))
        {
            var value = session.GameplayState.Flags.SingleOrDefault(item => item.Id == dialogue.Id)?.Value;
            if (!Enum.TryParse<GeneratedCampaignBranchKind>(value, out var branch)) continue;
            var choice = dialogue.Nodes.SelectMany(item => item.Choices).SingleOrDefault(item =>
                item.Metadata.TryGetValue("generatedChoiceKind", out var kind) && kind == branch.ToString()
                && item.Metadata.TryGetValue("generatedChoicePhase", out var phase) && phase == "initial");
            if (choice is null) continue;
            var followup = dialogue.Nodes.SelectMany(item => item.Choices).Any(item =>
                item.Metadata.TryGetValue("generatedChoiceKind", out var kind) && kind == branch.ToString()
                && item.Metadata.TryGetValue("generatedChoicePhase", out var phase) && phase.StartsWith("followup/", StringComparison.Ordinal));
            rows.Add(new GeneratedCampaignDecision
            {
                ActorTitle = dialogue.Title,
                ChosenBranch = choice.Text,
                Consequence = string.Join("; ", new GeneratedCampaignDialogueChoicePreviewServiceText().Consequences(choice, package)),
                RelatedContent = Related(choice, package),
                Status = followup ? GeneratedCampaignDecisionStatus.FollowUpAvailable : GeneratedCampaignDecisionStatus.Chosen,
                AlternativesLocked = true
            });
        }
        return new GeneratedCampaignDecisionJournal { Decisions = rows.OrderBy(item => item.ActorTitle, StringComparer.Ordinal).ToList() };
    }

    private static string Related(LLMGameCreator.Domain.Definitions.DialogueChoiceDefinition choice, GamePackageDefinition package)
    {
        var faction = choice.Metadata.GetValueOrDefault("generatedChoiceFactionId", string.Empty);
        var quest = choice.Metadata.GetValueOrDefault("generatedChoiceQuestId", string.Empty);
        var encounter = choice.Metadata.GetValueOrDefault("generatedChoiceEncounterId", string.Empty);
        var values = new[]
        {
            package.Game.Factions.SingleOrDefault(item => item.Id == faction)?.Name,
            package.Game.Quests.SingleOrDefault(item => item.Id == quest)?.Title,
            package.Game.Encounters.SingleOrDefault(item => item.Id == encounter)?.Name
        }.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        return string.Join("; ", values);
    }
}

internal sealed class GeneratedCampaignDialogueChoicePreviewServiceText
{
    public IReadOnlyList<string> Consequences(LLMGameCreator.Domain.Definitions.DialogueChoiceDefinition choice, GamePackageDefinition package)
    {
        var values = new List<string>();
        if (choice.Metadata.TryGetValue("generatedChoiceKind", out var kind)) values.Add("Решение: " + kind);
        if (choice.Metadata.TryGetValue("generatedChoiceReputationAmount", out var reputation) && reputation != "0") values.Add("Репутация: " + reputation);
        if (choice.Metadata.TryGetValue("generatedChoiceQuestId", out var quest) && !string.IsNullOrWhiteSpace(quest)) values.Add(package.Game.Quests.SingleOrDefault(item => item.Id == quest)?.Title ?? quest);
        if (choice.Metadata.TryGetValue("generatedChoiceEncounterId", out var encounter) && !string.IsNullOrWhiteSpace(encounter)) values.Add(package.Game.Encounters.SingleOrDefault(item => item.Id == encounter)?.Name ?? encounter);
        return values;
    }
}
