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
        var questAction = capabilityPlan.OrderedActions.SingleOrDefault(action =>
            action.ExpectedRuntimeEffects.Contains(FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful, StringComparer.Ordinal));
        var questEvents = questAction is null ? [] : session.CanonicalSession.Snapshots
            .Where(snapshot => snapshot.StepId == "capability." + questAction.ActionId)
            .SelectMany(snapshot => snapshot.RuntimeEvents)
            .Where(runtimeEvent => runtimeEvent.EventType == "ResourceChanged" && runtimeEvent.TargetId == resourceContract.TargetId)
            .ToList();
        var questEvent = questEvents.SingleOrDefault();
        decimal goldBefore = 0;
        decimal goldAfterQuest = 0;
        if (questEvent is null || !ReadDecimal(questEvent, "before", out goldBefore) || !ReadDecimal(questEvent, "after", out goldAfterQuest))
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
            ChoiceText = string.Empty,
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
    [
        new() { Label = "Репутация", Value = Number(reputationBefore) + " → " + Number(reputationAfter) },
        new() { Label = "Квест", Value = string.Equals(questState, "completed", StringComparison.OrdinalIgnoreCase) ? "завершён" : questState },
        new() { Label = "Доверенная реплика", Value = string.Join(" → ", visibility.Select(Visibility)) },
        new() { Label = "Золото", Value = Number(goldBefore) + " → " + Number(goldAfterQuest) + (claimed ? " → " + Number(goldAfterClaim) : string.Empty) },
        new() { Label = "Награда за доверие", Value = claimed ? "+" + Number(reward) : "не получена" },
        new() { Label = "Повторная награда", Value = "недоступна" },
        new() { Label = "Социальный итог", Value = outcome == "claimed" ? "награда получена" : "порог репутации ещё не достигнут" }
    ];

    private static string Visibility(string value) => value switch
    {
        "available" => "доступна",
        "unavailable" => "недоступна",
        _ => value
    };

    private static string Number(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}
