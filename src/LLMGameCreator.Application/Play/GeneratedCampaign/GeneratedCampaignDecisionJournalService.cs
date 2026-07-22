using System.Globalization;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Play.GeneratedCampaign;

public sealed class GeneratedCampaignDecisionJournalService
{
    public GeneratedCampaignDecisionJournal Project(GamePackageDefinition package, UnifiedRuntimeSession session)
    {
        var rows = new List<GeneratedCampaignDecision>();
        foreach (var dialogue in package.Game.Dialogues.Where(item =>
                     item.Tags.Contains("generated_choice_branching", StringComparer.Ordinal)))
        {
            var flag = session.GameplayState.Flags.SingleOrDefault(item => item.Id == dialogue.Id)?.Value;
            if (!Enum.TryParse<GeneratedCampaignBranchKind>(flag, out var branch)) continue;
            var initialChoices = dialogue.Nodes.SelectMany(item => item.Choices).Where(item =>
                item.Metadata.GetValueOrDefault("generatedChoicePhase") == "initial").ToList();
            var choice = initialChoices.SingleOrDefault(item => SetsExactBranchFlag(item, dialogue.Id, branch));
            if (choice is null) continue;

            var questId = choice.StartQuestId ?? string.Empty;
            var questState = session.GameplayState.Quests.SingleOrDefault(item => item.QuestId == questId)?.State
                             ?? string.Empty;
            var encounterId = choice.StartEncounterId ?? string.Empty;
            var encounter = session.GameplayState.ActiveEncounter is { } current
                            && current.EncounterId == encounterId
                ? current
                : null;
            var reputationEffect = choice.Effects.SingleOrDefault(item => item.Type == "change_reputation");
            var factionId = reputationEffect?.Args.GetValueOrDefault("id") ?? string.Empty;
            var reputation = session.GameplayState.Factions.SingleOrDefault(item => item.FactionId == factionId)
                ?.Reputation;
            var consequence = new List<string> { "Флаг ветви: " + branch };
            if (reputation is not null)
                consequence.Add("Репутация: " + reputation.Value.ToString("0.##", CultureInfo.InvariantCulture));
            if (questId.Length > 0) consequence.Add("Задание: " + (questState.Length > 0 ? questState : "not_started"));
            if (encounter is not null) consequence.Add("Встреча: " + (encounter.Active ? "active" : "resolved"));

            var status = branch switch
            {
                GeneratedCampaignBranchKind.SUPPORT when questState == "completed" =>
                    GeneratedCampaignDecisionStatus.Completed,
                GeneratedCampaignBranchKind.SUPPORT when questState == "active" =>
                    GeneratedCampaignDecisionStatus.FollowUpAvailable,
                GeneratedCampaignBranchKind.CHALLENGE when encounter is { Active: false } =>
                    GeneratedCampaignDecisionStatus.FollowUpAvailable,
                GeneratedCampaignBranchKind.REFUSE => GeneratedCampaignDecisionStatus.FollowUpAvailable,
                _ => GeneratedCampaignDecisionStatus.Chosen
            };
            var alternativesLocked = !string.IsNullOrWhiteSpace(flag)
                                     && string.Equals(flag, branch.ToString(), StringComparison.Ordinal)
                                     && initialChoices.Count > 1
                                     && initialChoices.All(item => item.Requirements.Any(requirement =>
                                         requirement.Kind == "flag_equals"
                                         && requirement.Id == dialogue.Id
                                         && requirement.Value == string.Empty));
            rows.Add(new GeneratedCampaignDecision
            {
                ActorTitle = dialogue.Title,
                ChosenBranch = choice.Text,
                Consequence = string.Join("; ", consequence),
                RelatedContent = Related(package, factionId, questId, encounterId),
                Status = status,
                AlternativesLocked = alternativesLocked
            });
        }
        return new GeneratedCampaignDecisionJournal
        {
            Decisions = rows.OrderBy(item => item.ActorTitle, StringComparer.Ordinal).ToList()
        };
    }

    private static bool SetsExactBranchFlag(
        DialogueChoiceDefinition choice,
        string dialogueId,
        GeneratedCampaignBranchKind branch) => choice.Effects.Any(effect =>
        effect.Type == "set_flag"
        && effect.Args.GetValueOrDefault("id") == dialogueId
        && effect.Args.GetValueOrDefault("value") == branch.ToString());

    private static string Related(
        GamePackageDefinition package,
        string factionId,
        string questId,
        string encounterId)
    {
        var values = new[]
        {
            package.Game.Factions.SingleOrDefault(item => item.Id == factionId)?.Name,
            package.Game.Quests.SingleOrDefault(item => item.Id == questId)?.Title,
            package.Game.Encounters.SingleOrDefault(item => item.Id == encounterId)?.Name
        }.Where(item => !string.IsNullOrWhiteSpace(item));
        return string.Join("; ", values!);
    }
}
