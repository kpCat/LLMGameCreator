using System.Globalization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

/// <summary>
/// Projects declared social Runtime-effect contracts into concise, typed review facts.
/// It deliberately correlates the declared capability action with its snapshot rather
/// than accepting a final flag or resource state as sufficient evidence.
/// </summary>
public sealed class SocialRuntimeReviewProjectionService
{
    private static readonly string[] RequiredMetricKinds =
    [
        FeatureModuleRuntimeEffectMetricKinds.FactionReputationInitialized,
        FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful,
        FeatureModuleRuntimeEffectMetricKinds.QuestStateEquals,
        FeatureModuleRuntimeEffectMetricKinds.DialogueChoiceVisibilitySequence,
        FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome,
        FeatureModuleRuntimeEffectMetricKinds.ResourceTransitionTruthful,
        FeatureModuleRuntimeEffectMetricKinds.FlagEquals
    ];

    public GameProjectSocialSummary Project(
        IReadOnlyList<FeatureModuleDefinition> selectedEffectiveModules,
        GamePackageDefinition package,
        CapabilityRuntimePlaythroughPlan capabilityPlan,
        RuntimeInteractiveSession session,
        IReadOnlyList<FeatureModuleRuntimeEffectObservation> observations,
        bool checkpointReplayPassed,
        bool fullReplayEquivalent)
    {
        ArgumentNullException.ThrowIfNull(selectedEffectiveModules);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(capabilityPlan);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(observations);

        var contracts = selectedEffectiveModules.SelectMany(module => module.RuntimeEffectContracts)
            .Where(contract => RequiredMetricKinds.Contains(contract.MetricKind, StringComparer.Ordinal))
            .ToList();
        if (contracts.Count == 0)
            return new GameProjectSocialSummary { Present = false, Passed = true };
        // A partial dependency closure (for example, faction alone or faction plus
        // quest) is a valid composition but has no complete human social outcome.
        // Four or more social facts indicate an attempted social outcome and must
        // therefore be causally complete rather than silently projected as partial.
        if (contracts.Count < 4)
            return new GameProjectSocialSummary { Present = false, Passed = true };

        var diagnostics = new List<string>();
        var contractByMetric = new Dictionary<string, FeatureModuleRuntimeEffectContract>(StringComparer.Ordinal);
        foreach (var metric in RequiredMetricKinds)
        {
            var candidates = contracts.Where(contract => contract.MetricKind == metric).ToList();
            if (candidates.Count != 1)
            {
                diagnostics.Add("social.projection.contract_" + (candidates.Count == 0 ? "missing:" : "ambiguous:") + metric);
                continue;
            }
            contractByMetric[metric] = candidates[0];
        }
        if (diagnostics.Count > 0)
            return Failed(diagnostics);

        var observationByMetric = new Dictionary<string, FeatureModuleRuntimeEffectObservation>(StringComparer.Ordinal);
        foreach (var metric in RequiredMetricKinds)
        {
            var candidates = observations.Where(observation => observation.MetricKind == metric).ToList();
            if (candidates.Count != 1)
            {
                diagnostics.Add("social.projection.observation_" + (candidates.Count == 0 ? "missing:" : "ambiguous:") + metric);
                continue;
            }
            if (!candidates[0].Passed)
                diagnostics.Add("social.projection.effect_failed:" + metric + ":" + string.Join("|", candidates[0].Diagnostics));
            observationByMetric[metric] = candidates[0];
        }
        if (diagnostics.Count > 0)
            return Failed(diagnostics);

        var factionContract = contractByMetric[FeatureModuleRuntimeEffectMetricKinds.FactionReputationInitialized];
        var transitionContract = contractByMetric[FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful];
        var questContract = contractByMetric[FeatureModuleRuntimeEffectMetricKinds.QuestStateEquals];
        var choiceContract = contractByMetric[FeatureModuleRuntimeEffectMetricKinds.DialogueChoiceVisibilitySequence];
        var resourceContract = contractByMetric[FeatureModuleRuntimeEffectMetricKinds.ResourceTransitionTruthful];
        var flagContract = contractByMetric[FeatureModuleRuntimeEffectMetricKinds.FlagEquals];
        var outcomeObservation = observationByMetric[FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome];
        var visibility = observationByMetric[FeatureModuleRuntimeEffectMetricKinds.DialogueChoiceVisibilitySequence]
            .ActualValue.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (visibility.Length != 3)
            diagnostics.Add("social.projection.visibility_sequence_invalid");
        if (!checkpointReplayPassed)
            diagnostics.Add("social.projection.checkpoint_replay_failed");
        if (!fullReplayEquivalent)
            diagnostics.Add("social.projection.full_replay_not_equivalent");

        if (!decimal.TryParse(observationByMetric[FeatureModuleRuntimeEffectMetricKinds.FactionReputationInitialized].ActualValue,
                NumberStyles.Number, CultureInfo.InvariantCulture, out var reputationBefore))
            diagnostics.Add("social.projection.reputation_before_invalid");
        var reputationAfter = session.CanonicalSession.RuntimeSession.GameplayState.Factions
            .SingleOrDefault(item => item.FactionId == transitionContract.TargetId)?.Reputation;
        if (!reputationAfter.HasValue)
            diagnostics.Add("social.projection.reputation_after_missing");
        var questState = session.CanonicalSession.RuntimeSession.GameplayState.Quests
            .SingleOrDefault(item => item.QuestId == questContract.TargetId)?.State;
        if (string.IsNullOrWhiteSpace(questState))
            diagnostics.Add("social.projection.quest_state_missing");

        var claimAction = capabilityPlan.OrderedActions.SingleOrDefault(action =>
            action.ExpectedRuntimeEffects.Contains(FeatureModuleRuntimeEffectMetricKinds.ResourceTransitionTruthful, StringComparer.Ordinal)
            && action.ExpectedRuntimeEffects.Contains(FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome, StringComparer.Ordinal)
            && action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.ChooseDialogueOption);
        if (claimAction is null)
            diagnostics.Add("social.projection.claim_action_missing");
        var claimSnapshots = claimAction is null ? [] : session.CanonicalSession.Snapshots
            .Where(snapshot => snapshot.StepId == "capability." + claimAction.ActionId).ToList();
        var claimEvents = claimSnapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
            .Where(runtimeEvent => runtimeEvent.EventType == "ResourceChanged" && runtimeEvent.TargetId == resourceContract.TargetId).ToList();
        var completionSnapshots = session.CanonicalSession.Snapshots.Where(snapshot =>
            snapshot.RuntimeEvents.Any(runtimeEvent =>
                runtimeEvent.EventType == "QuestCompleted" && runtimeEvent.TargetId == questContract.TargetId)
            && snapshot.RuntimeEvents.Any(runtimeEvent =>
                runtimeEvent.EventType == "QuestRewardGranted" && runtimeEvent.TargetId == questContract.TargetId)).ToList();
        if (completionSnapshots.Count != 1)
            diagnostics.Add("social.projection.quest_completion_snapshot_"
                            + (completionSnapshots.Count == 0 ? "missing:" : "ambiguous:")
                            + questContract.TargetId);
        var completionSnapshot = completionSnapshots.Count == 1 ? completionSnapshots[0] : null;
        if (completionSnapshot is not null)
        {
            var completedCount = completionSnapshot.RuntimeEvents.Count(runtimeEvent =>
                runtimeEvent.EventType == "QuestCompleted" && runtimeEvent.TargetId == questContract.TargetId);
            var rewardCount = completionSnapshot.RuntimeEvents.Count(runtimeEvent =>
                runtimeEvent.EventType == "QuestRewardGranted" && runtimeEvent.TargetId == questContract.TargetId);
            if (completedCount != 1 || rewardCount != 1)
                diagnostics.Add("social.projection.quest_completion_event_counts:completed=" + completedCount
                                + ";reward=" + rewardCount + ";questId=" + questContract.TargetId);
        }
        var questEvents = completionSnapshot?.RuntimeEvents
            .Where(runtimeEvent => runtimeEvent.EventType == "ResourceChanged"
                                   && runtimeEvent.TargetId == resourceContract.TargetId).ToList() ?? [];
        decimal goldBefore = 0;
        decimal goldAfterQuest = 0;
        if (questEvents.Count == 0)
            diagnostics.Add("social.projection.quest_resource_transition_missing");
        else if (questEvents.Count > 1)
            diagnostics.Add("social.projection.quest_resource_transition_ambiguous");
        else if (!ReadDecimal(questEvents[0], "before", out goldBefore) || !ReadDecimal(questEvents[0], "after", out goldAfterQuest))
            diagnostics.Add("social.projection.quest_resource_transition_missing");
        var claimed = string.Equals(outcomeObservation.ActualValue, "claimed", StringComparison.Ordinal);
        var locked = string.Equals(outcomeObservation.ActualValue, "still_locked", StringComparison.Ordinal);
        if (!claimed && !locked)
            diagnostics.Add("social.projection.outcome_invalid:" + outcomeObservation.ActualValue);
        decimal goldAfterClaim = 0;
        decimal trustedReward = 0;
        if (claimed)
        {
            if (claimEvents.Count != 1 || !ReadDecimal(claimEvents[0], "after", out goldAfterClaim)
                || !ReadDecimal(claimEvents[0], "actualDelta", out trustedReward))
                diagnostics.Add("social.projection.claim_resource_transition_missing");
        }
        else if (locked)
        {
            if (claimEvents.Count != 0)
                diagnostics.Add("social.projection.locked_claim_resource_event_present");
            goldAfterClaim = goldAfterQuest;
        }

        if (diagnostics.Count > 0)
            return Failed(diagnostics);

        var faction = package.Game.Factions.SingleOrDefault(item => item.Id == factionContract.TargetId);
        var quest = package.Game.Quests.SingleOrDefault(item => item.Id == questContract.TargetId);
        var dialogueId = claimAction!.Args.GetValueOrDefault("dialogueId", string.Empty);
        var nodeId = claimAction.Args.GetValueOrDefault("nodeId", string.Empty);
        var dialogueMatches = package.Game.Dialogues.Where(item => item.Id == dialogueId).ToList();
        var nodeMatches = dialogueMatches.SelectMany(item => item.Nodes).Where(item => item.Id == nodeId).ToList();
        var choiceMatches = nodeMatches.SelectMany(item => item.Choices).Where(item => item.Id == choiceContract.TargetId).ToList();
        if (dialogueMatches.Count != 1 || nodeMatches.Count != 1 || choiceMatches.Count != 1)
            return Failed(["social.projection.choice_text_missing_or_ambiguous"]);
        var reputationAfterValue = reputationAfter ?? 0;
        return new GameProjectSocialSummary
        {
            Present = true,
            Passed = true,
            FactionId = factionContract.TargetId,
            FactionTitle = faction?.Name ?? string.Empty,
            ReputationBefore = reputationBefore,
            ReputationAfter = (decimal)reputationAfterValue,
            QuestId = questContract.TargetId,
            QuestTitle = quest?.Title ?? string.Empty,
            QuestState = questState!,
            ChoiceId = choiceContract.TargetId,
            ChoiceText = choiceMatches[0].Text,
            ChoiceVisibilitySequence = visibility,
            GoldBefore = goldBefore,
            GoldAfterQuest = goldAfterQuest,
            GoldAfterClaim = goldAfterClaim,
            TrustedRewardDelta = trustedReward,
            ClaimFlagId = flagContract.TargetId,
            RewardClaimed = claimed,
            RepeatRewardAvailable = false,
            SocialOutcome = outcomeObservation.ActualValue,
            CheckpointReplayPassed = checkpointReplayPassed,
            FullReplayEquivalent = fullReplayEquivalent,
            HumanFacts = HumanFacts(reputationBefore, (decimal)reputationAfterValue, questState!, visibility,
                goldBefore, goldAfterQuest, goldAfterClaim, trustedReward, claimed, outcomeObservation.ActualValue),
            Diagnostics = []
        };
    }

    public static IReadOnlyList<string> HumanSummaryLines(GameProjectSocialSummary social) => social.Present && social.Passed
        ? social.HumanFacts.Select(fact => fact.Label + ": " + fact.Value).ToList()
        : [];

    private static GameProjectSocialSummary Failed(IReadOnlyList<string> diagnostics) => new()
    {
        Present = true,
        Passed = false,
        Diagnostics = diagnostics
    };

    private static bool ReadDecimal(CanonicalRuntimePlayerCommandLoopRuntimeEvent runtimeEvent, string key, out decimal value) =>
        decimal.TryParse(runtimeEvent.Args.GetValueOrDefault(key), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private static IReadOnlyList<GameProjectSocialHumanFact> HumanFacts(
        decimal reputationBefore, decimal reputationAfter, string questState, IReadOnlyList<string> visibility,
        decimal goldBefore, decimal goldAfterQuest, decimal goldAfterClaim, decimal reward, bool claimed, string outcome) =>
        HumanFactsCore(reputationBefore, reputationAfter, questState, visibility, goldBefore, goldAfterQuest, goldAfterClaim, reward, claimed, outcome);

    /* Historical expression body retained below only for patch-local context; the active formatter is HumanFactsCore. 
    [
        new() { Label = "Репутация", Value = Number(reputationBefore) + " → " + Number(reputationAfter) },
        new() { Label = "Квест", Value = string.Equals(questState, "completed", StringComparison.OrdinalIgnoreCase) ? "завершён" : questState },
        new() { Label = "Доверенная реплика", Value = string.Join(" → ", visibility.Select(Visibility)) },
        new() { Label = "Золото", Value = Number(goldBefore) + " → " + Number(goldAfterQuest) + (claimed ? " → " + Number(goldAfterClaim) : string.Empty) },
        new() { Label = "Награда за доверие", Value = claimed ? "+" + Number(reward) : "не получена" },
        new() { Label = "Повторная награда", Value = "недоступна" },
        new() { Label = "Социальный итог", Value = outcome == "claimed" ? "награда получена" : "порог репутации ещё не достигнут" }
    ];

    */

    private static IReadOnlyList<GameProjectSocialHumanFact> HumanFactsCore(
        decimal reputationBefore, decimal reputationAfter, string questState, IReadOnlyList<string> visibility,
        decimal goldBefore, decimal goldAfterQuest, decimal goldAfterClaim, decimal reward, bool claimed, string outcome)
    {
        var facts = new List<GameProjectSocialHumanFact>
        {
            new() { Label = "Репутация", Value = Number(reputationBefore) + " → " + Number(reputationAfter) },
            new() { Label = "Квест", Value = string.Equals(questState, "completed", StringComparison.OrdinalIgnoreCase) ? "завершён" : questState },
            new() { Label = "Доверенная реплика", Value = string.Join(" → ", visibility.Select(Visibility)) },
            new() { Label = "Золото", Value = Number(goldBefore) + " → " + Number(goldAfterQuest) + (claimed ? " → " + Number(goldAfterClaim) : string.Empty) },
            new() { Label = "Награда за доверие", Value = claimed ? "+" + Number(reward) : "пока недоступна" }
        };
        if (claimed) facts.Add(new GameProjectSocialHumanFact { Label = "Повторная награда", Value = "недоступна" });
        facts.Add(new GameProjectSocialHumanFact
        {
            Label = "Социальный итог",
            Value = outcome == "claimed" ? "награда получена" : "порог репутации ещё не достигнут"
        });
        return facts;
    }

    private static string Visibility(string value) => value switch
    {
        "available" => "доступна",
        "unavailable" => "недоступна",
        _ => value
    };

    private static string Number(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}
