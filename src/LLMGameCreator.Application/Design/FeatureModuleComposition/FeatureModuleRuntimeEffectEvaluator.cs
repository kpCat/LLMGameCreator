using System.Globalization;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleRuntimeEffectEvaluator
{
    public IReadOnlyList<FeatureModuleRuntimeEffectObservation> Evaluate(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        RuntimeInteractiveSession session,
        RuntimeInteractiveSession baselineSession,
        GamePackageDefinition? package = null)
    {
        ArgumentNullException.ThrowIfNull(selectedModules);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(baselineSession);
        return selectedModules.SelectMany(module => module.RuntimeEffectContracts)
            .OrderBy(contract => contract.EffectId, StringComparer.Ordinal)
            .Select(contract => Observe(contract, session, baselineSession, package))
            .ToList();
    }

    private static FeatureModuleRuntimeEffectObservation Observe(
        FeatureModuleRuntimeEffectContract contract,
        RuntimeInteractiveSession session,
        RuntimeInteractiveSession baselineSession,
        GamePackageDefinition? package)
    {
        var diagnostics = new List<string>();
        if (contract.MetricKind is FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta
                or FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta
            && session.CapabilityPlan?.OrderedActions.All(action => action.ActionId != "basic_attack") == true)
        {
            return new FeatureModuleRuntimeEffectObservation
            {
                EffectId = contract.EffectId,
                ModuleId = contract.ModuleId,
                MetricKind = contract.MetricKind,
                TargetId = contract.TargetId,
                ResourceOrItemId = contract.ResourceOrItemId,
                ComparisonKind = contract.ComparisonKind,
                ExpectedValue = contract.ExpectedValue,
                BaselineValue = "not_applicable",
                ActualValue = "not_applicable",
                RuntimeDimension = contract.RuntimeDimension,
                Passed = true,
                Diagnostics = ["combat capability absent; combat delta is not applicable"]
            };
        }
        var actual = ReadMetric(contract, session, diagnostics, package);
        var baseline = contract.MetricKind switch
        {
            FeatureModuleRuntimeEffectMetricKinds.EquipmentSlotItemEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta => "0",
            FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta => "0",
            FeatureModuleRuntimeEffectMetricKinds.PlayerStatEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.ProgressionAmountEquals => "0",
            FeatureModuleRuntimeEffectMetricKinds.ProgressionStageEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.AbilityDirectDamageEquals => "0",
            FeatureModuleRuntimeEffectMetricKinds.ParticipantResourceEquals => "0",
            FeatureModuleRuntimeEffectMetricKinds.StatusTickDamageEquals => "0",
            FeatureModuleRuntimeEffectMetricKinds.StatusAbsentAfterExpiry => "absent",
            FeatureModuleRuntimeEffectMetricKinds.StatusTerminalOutcome => "not_applicable",
            FeatureModuleRuntimeEffectMetricKinds.FactionReputationInitialized => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.QuestStateEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.DialogueChoiceVisibilitySequence => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.ResourceTransitionTruthful => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.FlagEquals => string.Empty,
            FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome => string.Empty,
            _ => ReadMetric(contract, baselineSession, diagnostics, package)
        };
        var passed = diagnostics.Count == 0 && Compare(contract, actual, baseline);
        if (!passed && diagnostics.Count == 0)
            diagnostics.Add("runtime effect comparison failed");
        return new FeatureModuleRuntimeEffectObservation
        {
            EffectId = contract.EffectId,
            ModuleId = contract.ModuleId,
            MetricKind = contract.MetricKind,
            TargetId = contract.TargetId,
            ResourceOrItemId = contract.ResourceOrItemId,
            ComparisonKind = contract.ComparisonKind,
            ExpectedValue = contract.ExpectedValue,
            BaselineValue = baseline,
            ActualValue = actual,
            RuntimeDimension = contract.RuntimeDimension,
            Passed = passed,
            Diagnostics = diagnostics
        };
    }

    private static string ReadMetric(
        FeatureModuleRuntimeEffectContract contract,
        RuntimeInteractiveSession session,
        List<string> diagnostics,
        GamePackageDefinition? package)
    {
        var value = contract.MetricKind switch
        {
            FeatureModuleRuntimeEffectMetricKinds.InventoryItemQuantity =>
                InventoryQuantity(session.LatestInventorySummary, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.CombatResourceAmount =>
                CombatStartingQuantity(session, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.EquipmentSlotItemEquals =>
                EquipmentItem(session.LatestEquipmentSummary, contract.TargetId),
            FeatureModuleRuntimeEffectMetricKinds.InventoryItemAbsentOrDecreased =>
                InventoryQuantity(session.LatestInventorySummary, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.CombatDamageDelta =>
                EquipmentDamageDelta(session)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.PlayerStatEquals =>
                PlayerStat(session, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.CombatStatDamageDelta =>
                CombatDamageEventValue(session, "statDamageBonus")?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.ProgressionAmountEquals =>
                Progression(session, contract.TargetId)?.Amount.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.ProgressionStageEquals =>
                Progression(session, contract.TargetId)?.StageId,
            FeatureModuleRuntimeEffectMetricKinds.AbilityDirectDamageEquals =>
                AbilityDamage(session, contract.TargetId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.ParticipantResourceEquals =>
                ParticipantResource(session, contract.TargetId, contract.ResourceOrItemId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.StatusTickDamageEquals =>
                StatusTickDamage(session, contract.TargetId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.StatusAbsentAfterExpiry =>
                StatusAbsent(session, contract.TargetId, contract.ResourceOrItemId) ? "absent" : "present",
            FeatureModuleRuntimeEffectMetricKinds.StatusTerminalOutcome =>
                StatusTerminalOutcome(session, contract.TargetId, contract.ResourceOrItemId),
            FeatureModuleRuntimeEffectMetricKinds.FactionReputationInitialized =>
                InitialFactionReputation(session, contract.TargetId)?.ToString(CultureInfo.InvariantCulture),
            FeatureModuleRuntimeEffectMetricKinds.QuestStateEquals =>
                QuestState(session, contract, diagnostics),
            FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful =>
                FactionTransitionTruthful(session, contract, package, diagnostics) ? "true" : "false",
            FeatureModuleRuntimeEffectMetricKinds.DialogueChoiceVisibilitySequence =>
                DialogueChoiceVisibilitySequence(session, contract.TargetId, package),
            FeatureModuleRuntimeEffectMetricKinds.ResourceTransitionTruthful =>
                ResourceTransitionTruthful(session, contract),
            FeatureModuleRuntimeEffectMetricKinds.FlagEquals =>
                FlagValue(session, contract.TargetId),
            FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome =>
                TrustedRewardSocialOutcome(session, contract.TargetId),
            _ => null
        };
        if (value is null)
        {
            diagnostics.Add("unsupported or missing runtime effect metric: " + contract.MetricKind);
            return string.Empty;
        }
        return value;
    }

    private static bool Compare(FeatureModuleRuntimeEffectContract contract, string actualText, string baselineText)
    {
        if (contract.ComparisonKind == FeatureModuleRuntimeEffectComparisonKinds.OneOf)
            return contract.ExpectedValue.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Contains(actualText, StringComparer.Ordinal);
        if (contract.ComparisonKind == FeatureModuleRuntimeEffectComparisonKinds.Equal)
        {
            return decimal.TryParse(contract.ExpectedValue, NumberStyles.Number, CultureInfo.InvariantCulture,
                out var expected)
                ? decimal.TryParse(actualText, NumberStyles.Number, CultureInfo.InvariantCulture, out var actualEqual)
                  && actualEqual == expected
                : string.Equals(actualText, contract.ExpectedValue, StringComparison.Ordinal);
        }
        if (!decimal.TryParse(actualText, NumberStyles.Number, CultureInfo.InvariantCulture, out var actual)
            || !decimal.TryParse(baselineText, NumberStyles.Number, CultureInfo.InvariantCulture, out var baseline)) return false;
        return contract.ComparisonKind switch
        {
            FeatureModuleRuntimeEffectComparisonKinds.GreaterThanBaseline => actual > baseline,
            FeatureModuleRuntimeEffectComparisonKinds.ChangedFromBaseline => actual != baseline,
            FeatureModuleRuntimeEffectComparisonKinds.AtLeast =>
                decimal.TryParse(contract.ExpectedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var minimum)
                && actual >= minimum,
            FeatureModuleRuntimeEffectComparisonKinds.LessThanBaseline => actual < baseline,
            _ => false
        };
    }

    private static int? InventoryQuantity(string summary, string inventoryId, string itemId)
    {
        var inventory = summary.Split(';').FirstOrDefault(part =>
            part.TrimStart().StartsWith(inventoryId + "=", StringComparison.Ordinal));
        if (inventory is null) return null;
        var match = Regex.Match(inventory, Regex.Escape(itemId) + @":(?<value>\d+)");
        return match.Success
            ? int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture)
            : 0;
    }

    private static int? CombatQuantity(string summary, string participantId, string resourceId)
    {
        var match = Regex.Match(summary,
            Regex.Escape(participantId) + @"\[[^\]]*" + Regex.Escape(resourceId) + @"=(?<value>\d+)");
        return match.Success
            ? int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture)
            : null;
    }

    private static decimal? CombatStartingQuantity(
        RuntimeInteractiveSession session,
        string participantId,
        string resourceId)
    {
        var remaining = CombatQuantity(session.LatestCombatSummary, participantId, resourceId);
        if (!remaining.HasValue) return null;
        var damage = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
            .Where(item => item.EventType == "DamageApplied" && item.TargetId == participantId)
            .Select(item => item.Args.TryGetValue("damage", out var raw)
                ? raw
                : Regex.Match(item.Message, @"-(?<value>\d+(?:\.\d+)?)$").Groups["value"].Value)
            .Select(raw => decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0)
            .Sum();
        return remaining.Value + damage;
    }

    private static string? EquipmentItem(string summary, string slotId)
    {
        var entry = summary.Split(';').Select(part => part.Trim())
            .SingleOrDefault(part => part.StartsWith(slotId + ":", StringComparison.Ordinal));
        return entry is null ? null : entry[(slotId.Length + 1)..];
    }

    private static decimal? EquipmentDamageDelta(RuntimeInteractiveSession session)
    {
        var value = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
            .Where(item => item.EventType == "DamageApplied")
            .Select(item => item.Args.TryGetValue("equipmentDamageBonus", out var raw) ? raw : null)
            .LastOrDefault(raw => raw is not null);
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed : null;
    }

    private static double? PlayerStat(RuntimeInteractiveSession session, string statId) =>
        session.CanonicalSession.RuntimeSession.GameplayState.Stats
            .SingleOrDefault(stat => stat.StatId == statId)?.Value;

    private static ProgressionState? Progression(RuntimeInteractiveSession session, string progressionId)
    {
        var action = session.CapabilityPlan?.OrderedActions.LastOrDefault(item =>
            item.RuntimePrimitiveId == "runtime.command.change_progression" && item.ResolvedTargetId == progressionId);
        var summary = action is null ? null : session.CanonicalSession.Snapshots.LastOrDefault(snapshot =>
            snapshot.StepId == "capability." + action.ActionId)?.ProgressionSummary;
        if (!string.IsNullOrWhiteSpace(summary))
        {
            var entry = summary.Split(';').Select(part => part.Trim()).SingleOrDefault(part =>
                part.StartsWith(progressionId + "=", StringComparison.Ordinal));
            if (entry is not null)
            {
                var parts = entry[(progressionId.Length + 1)..].Split(':');
                if (double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                    return new ProgressionState
                    {
                        ProgressionId = progressionId,
                        Amount = amount,
                        StageId = parts.Length > 1 ? parts[1] : string.Empty
                    };
            }
        }
        return session.CanonicalSession.RuntimeSession.GameplayState.Progressions
            .SingleOrDefault(progression => progression.ProgressionId == progressionId);
    }

    private static decimal? CombatDamageEventValue(RuntimeInteractiveSession session, string key)
    {
        var value = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
            .Where(item => item.EventType == "DamageApplied")
            .Select(item => item.Args.GetValueOrDefault(key))
            .LastOrDefault(raw => !string.IsNullOrWhiteSpace(raw));
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed : null;
    }

    private static decimal? AbilityDamage(RuntimeInteractiveSession session, string abilityId)
    {
        var snapshot = session.CanonicalSession.Snapshots.LastOrDefault(item =>
            item.RuntimeEvents.Any(runtimeEvent => runtimeEvent.EventType == "AbilityUsed" && runtimeEvent.TargetId == abilityId));
        return snapshot is null ? null : snapshot.RuntimeEvents.Where(item => item.EventType == "DamageApplied").Sum(EventDamage);
    }

    private static decimal? ParticipantResource(RuntimeInteractiveSession session, string participantId, string resourceId) =>
        session.CanonicalSession.RuntimeSession.GameplayState.ActiveEncounter?.Participants
            .SingleOrDefault(item => item.Id == participantId)?.Resources
            .SingleOrDefault(item => item.ResourceId == resourceId)?.Amount is double value ? (decimal)value : null;

    private static decimal? StatusTickDamage(RuntimeInteractiveSession session, string statusId)
    {
        var snapshots = session.CanonicalSession.Snapshots.Where(item =>
            item.RuntimeEvents.Any(runtimeEvent => runtimeEvent.EventType == "StatusTicked"
                && runtimeEvent.Args.GetValueOrDefault("statusId") == statusId)).ToList();
        return snapshots.Count == 0 ? null : snapshots.SelectMany(item => item.RuntimeEvents)
            .Where(item => item.EventType == "DamageApplied").Select(EventDamage).FirstOrDefault();
    }

    private static bool StatusAbsent(RuntimeInteractiveSession session, string participantId, string statusId)
    {
        var participant = session.CanonicalSession.RuntimeSession.GameplayState.ActiveEncounter?.Participants
            .SingleOrDefault(item => item.Id == participantId);
        return participant is not null && participant.Statuses.All(item => item.StatusId != statusId)
            && session.CanonicalSession.Snapshots.SelectMany(item => item.RuntimeEvents)
                .Any(item => item.EventType == "StatusRemoved" && item.Message.Contains("expired", StringComparison.OrdinalIgnoreCase)
                    && item.TargetId == participantId);
    }

    private static string? StatusTerminalOutcome(
        RuntimeInteractiveSession session,
        string participantSelector,
        string statusId)
    {
        var participantId = participantSelector == "hostile_encounter_participant"
            ? session.CapabilityPlan?.OrderedActions.FirstOrDefault(action =>
                action.TargetSelector == participantSelector)?.ResolvedTargetId
            : participantSelector;
        var events = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
        if (!string.IsNullOrWhiteSpace(participantId)
            && events.Any(item => item.EventType == "ParticipantDefeated" && item.TargetId == participantId))
            return "target_defeated";
        if (events.Any(item => item.EventType == "EncounterWon")) return "encounter_won";
        if (events.Any(item => item.EventType == "EncounterLost")) return "encounter_lost";
        if (!string.IsNullOrWhiteSpace(participantId)
            && events.Any(item => item.EventType == "StatusRemoved"
                                  && item.TargetId == participantId
                                  && item.Message.Contains(statusId, StringComparison.Ordinal)
                                  && item.Message.Contains("expired", StringComparison.OrdinalIgnoreCase)))
            return "expired";
        if (events.Any(item => item.EventType == "EncounterEnded")) return "encounter_ended";
        return null;
    }

    private static decimal EventDamage(CanonicalRuntimePlayerCommandLoopRuntimeEvent runtimeEvent)
    {
        var raw = runtimeEvent.Args.GetValueOrDefault("damage");
        if (string.IsNullOrWhiteSpace(raw)) raw = Regex.Match(runtimeEvent.Message, @"-(?<value>\d+(?:\.\d+)?)$").Groups["value"].Value;
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    private static decimal? InitialFactionReputation(RuntimeInteractiveSession session, string factionId)
    {
        var transitions = session.CanonicalSession.Snapshots.SelectMany(item => item.RuntimeEvents)
            .Where(item => item.EventType == "FactionReputationChanged" && item.TargetId == factionId).ToList();
        if (transitions.Count == 1 && DecimalArg(transitions[0], "before", out var causalBefore))
            return causalBefore;
        var snapshot = session.CanonicalSession.Snapshots.FirstOrDefault(item =>
            item.StepId.StartsWith("presentation.", StringComparison.Ordinal)
            && Regex.IsMatch(item.FactionSummary, @"(?:^|;\s*)" + Regex.Escape(factionId) + @"=(?<value>-?\d+(?:\.\d+)?)"));
        if (snapshot is null) return null;
        var match = Regex.Match(snapshot.FactionSummary,
            @"(?:^|;\s*)" + Regex.Escape(factionId) + @"=(?<value>-?\d+(?:\.\d+)?)");
        return decimal.TryParse(match.Groups["value"].Value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static bool FactionTransitionTruthful(
        RuntimeInteractiveSession session,
        FeatureModuleRuntimeEffectContract contract,
        GamePackageDefinition? package,
        List<string> diagnostics)
    {
        var declaringActions = session.CapabilityPlan?.OrderedActions.Where(action =>
            action.ExpectedRuntimeEffects.Contains(contract.MetricKind, StringComparer.Ordinal)).ToList() ?? [];
        if (declaringActions.Count != 1)
        {
            diagnostics.Add("faction_transition.declaring_action_count=" + declaringActions.Count);
            return false;
        }
        var questId = declaringActions[0].Args.GetValueOrDefault("questId") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(questId))
        {
            diagnostics.Add("faction_transition.quest_id_missing");
            return false;
        }
        var completionSnapshots = session.CanonicalSession.Snapshots.Where(snapshot =>
            snapshot.RuntimeEvents.Any(item => item.EventType == "QuestCompleted" && item.TargetId == questId)).ToList();
        if (completionSnapshots.Count != 1)
        {
            diagnostics.Add("faction_transition.quest_completion_snapshot_count=" + completionSnapshots.Count
                            + ";questId=" + questId);
            return false;
        }
        var completionEvents = completionSnapshots[0].RuntimeEvents.Where(item =>
            item.EventType == "QuestCompleted" && item.TargetId == questId).ToList();
        var transitionEvents = completionSnapshots[0].RuntimeEvents.Where(item =>
            item.EventType == "FactionReputationChanged" && item.TargetId == contract.TargetId).ToList();
        if (completionEvents.Count != 1 || transitionEvents.Count != 1)
        {
            diagnostics.Add("faction_transition.causal_event_counts=completion:" + completionEvents.Count
                            + ",transition:" + transitionEvents.Count + ";questId=" + questId);
            return false;
        }
        var runtimeEvent = transitionEvents[0];
        if (runtimeEvent is null
            || !DecimalArg(runtimeEvent, "before", out var before)
            || !DecimalArg(runtimeEvent, "requested", out var requested)
            || !DecimalArg(runtimeEvent, "after", out var after)
            || !DecimalArg(runtimeEvent, "delta", out var delta)
            || !bool.TryParse(runtimeEvent.Args.GetValueOrDefault("clamped"), out var clamped)) return false;
        var final = session.CanonicalSession.RuntimeSession.GameplayState.Factions
            .SingleOrDefault(item => item.FactionId == contract.TargetId)?.Reputation;
        if (!final.HasValue || (decimal)final.Value != after || delta != after - before
            || clamped != (after != before + requested)) return false;
        if (package is null) return true;
        var definition = package.Game.Factions.SingleOrDefault(item => item.Id == contract.TargetId);
        if (definition is null) return false;
        if (definition.MinReputation.HasValue && after < (decimal)definition.MinReputation.Value) return false;
        if (definition.MaxReputation.HasValue && after > (decimal)definition.MaxReputation.Value) return false;
        if (!clamped) return true;
        return requested >= 0
            ? !definition.MaxReputation.HasValue || after == (decimal)definition.MaxReputation.Value
            : !definition.MinReputation.HasValue || after == (decimal)definition.MinReputation.Value;
    }

    private static string? QuestState(
        RuntimeInteractiveSession session,
        FeatureModuleRuntimeEffectContract contract,
        List<string> diagnostics)
    {
        var quests = session.CanonicalSession.RuntimeSession.GameplayState.Quests
            .Where(item => item.QuestId == contract.TargetId).ToList();
        if (quests.Count != 1)
        {
            diagnostics.Add("quest_state.runtime_quest_count=" + quests.Count + ";questId=" + contract.TargetId);
            return null;
        }
        if (string.Equals(contract.ExpectedValue, "completed", StringComparison.Ordinal))
        {
            var completionSnapshots = session.CanonicalSession.Snapshots.Where(snapshot =>
                snapshot.RuntimeEvents.Any(item =>
                    item.EventType == "QuestCompleted" && item.TargetId == contract.TargetId)).ToList();
            if (completionSnapshots.Count != 1)
            {
                diagnostics.Add("quest_state.completion_snapshot_count=" + completionSnapshots.Count
                                + ";questId=" + contract.TargetId);
                return null;
            }
            var completionEvents = completionSnapshots[0].RuntimeEvents.Count(item =>
                item.EventType == "QuestCompleted" && item.TargetId == contract.TargetId);
            if (completionEvents != 1)
            {
                diagnostics.Add("quest_state.completion_event_count=" + completionEvents
                                + ";questId=" + contract.TargetId);
                return null;
            }
        }
        return quests[0].State;
    }

    private static string? DialogueChoiceVisibilitySequence(
        RuntimeInteractiveSession session,
        string choiceId,
        GamePackageDefinition? package)
    {
        var values = session.CanonicalSession.Snapshots
            .Where(item => item.StepId.StartsWith("presentation.", StringComparison.Ordinal)
                           && Regex.IsMatch(item.DialogueChoicesSummary,
                               @"(?:^|;\s*)" + Regex.Escape(choiceId) + @"=(available|unavailable)"))
            .Select(item => Regex.Match(item.DialogueChoicesSummary,
                    @"(?:^|;\s*)" + Regex.Escape(choiceId) + @"=(?<value>available|unavailable)")
                .Groups["value"].Value).ToList();
        if (values.Count < 3) return null;
        if (values[0] == "available" && values[1] == "available" && values[2] == "unavailable"
            && package is not null)
        {
            var choices = package.Game.Dialogues.SelectMany(dialogue => dialogue.Nodes)
                .SelectMany(node => node.Choices).Where(choice => choice.Id == choiceId).ToList();
            var reputationRequirements = choices.Count == 1
                ? choices[0].Requirements.Where(requirement => requirement.Kind == "reputation_at_least").ToList()
                : [];
            if (reputationRequirements.Count == 1)
            {
                var requirement = reputationRequirements[0];
                var transitions = session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents)
                    .Where(item => item.EventType == "FactionReputationChanged" && item.TargetId == requirement.Id).ToList();
                if (transitions.Count == 1
                    && requirement.Amount.HasValue
                    && DecimalArg(transitions[0], "before", out var before)
                    && DecimalArg(transitions[0], "after", out var after)
                    && before < (decimal)requirement.Amount.Value
                    && after >= (decimal)requirement.Amount.Value)
                    return "unavailable>available>unavailable";
            }
        }
        return string.Join(">", values.Take(3));
    }

    private static string? ResourceTransitionTruthful(
        RuntimeInteractiveSession session,
        FeatureModuleRuntimeEffectContract contract)
    {
        var resourceId = contract.TargetId;
        var events = EventsForMetric(session, contract.MetricKind)
            .Where(item => item.EventType == "ResourceChanged" && item.TargetId == resourceId).ToList();
        if (events.Count == 0) return SocialStillLocked(session) ? "not_applicable" : null;
        if (events.Count != 1) return "false";
        var runtimeEvent = events[0];
        if (runtimeEvent.Args.GetValueOrDefault("resourceId") != resourceId
            || !DecimalArg(runtimeEvent, "before", out var before)
            || !DecimalArg(runtimeEvent, "requestedDelta", out var requested)
            || !DecimalArg(runtimeEvent, "after", out var after)
            || !DecimalArg(runtimeEvent, "actualDelta", out var actual)
            || !bool.TryParse(runtimeEvent.Args.GetValueOrDefault("clamped"), out var clamped)
            || actual != after - before || clamped != (actual != requested)) return "false";
        var scope = runtimeEvent.Args.GetValueOrDefault("scope");
        if (string.IsNullOrWhiteSpace(scope)) scope = "global";
        var final = session.CanonicalSession.RuntimeSession.GameplayState.Resources.SingleOrDefault(item =>
            item.ResourceId == resourceId && item.Scope == scope)?.Amount;
        return final.HasValue && (decimal)final.Value == after ? "true" : "false";
    }

    private static string FlagValue(RuntimeInteractiveSession session, string flagId)
    {
        var value = session.CanonicalSession.RuntimeSession.GameplayState.Flags
            .SingleOrDefault(item => item.Id == flagId)?.Value;
        return value ?? (SocialStillLocked(session) ? "not_applicable" : string.Empty);
    }

    private static string? TrustedRewardSocialOutcome(RuntimeInteractiveSession session, string choiceId)
    {
        if (session.CanonicalSession.Snapshots.SelectMany(item => item.RuntimeEvents)
            .Any(item => item.EventType == "DialogueChoiceSelected" && item.TargetId == choiceId)) return "claimed";
        var action = session.CapabilityPlan?.OrderedActions.SingleOrDefault(item =>
            item.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.ChooseDialogueOption
            && item.Args.GetValueOrDefault("choiceId") == choiceId);
        var journal = action is null ? null : session.ActionJournal.SingleOrDefault(item => item.ActionId == action.ActionId);
        if (journal?.Status == "SKIPPED" && journal.Diagnostics.Any(item =>
                item.Contains("socialOutcome=still_locked", StringComparison.Ordinal))) return "still_locked";
        var flagId = action?.Args.GetValueOrDefault("claimFlagId");
        if (!string.IsNullOrWhiteSpace(flagId) && session.CanonicalSession.RuntimeSession.GameplayState.Flags.Any(item =>
                item.Id == flagId && item.Value.Equals("true", StringComparison.OrdinalIgnoreCase))) return "already_claimed";
        return null;
    }

    private static IReadOnlyList<CanonicalRuntimePlayerCommandLoopRuntimeEvent> EventsForMetric(
        RuntimeInteractiveSession session,
        string metricKind)
    {
        var actionIds = session.CapabilityPlan?.OrderedActions.Where(action =>
                action.ExpectedRuntimeEffects.Contains(metricKind, StringComparer.Ordinal))
            .Select(action => "capability." + action.ActionId).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        var snapshots = actionIds.Count == 0 ? session.CanonicalSession.Snapshots
            : session.CanonicalSession.Snapshots.Where(snapshot => actionIds.Contains(snapshot.StepId));
        return snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
    }

    private static bool SocialStillLocked(RuntimeInteractiveSession session) =>
        session.ActionJournal.Any(entry => entry.Status == "SKIPPED" && entry.Diagnostics.Any(item =>
            item.Contains("socialOutcome=still_locked", StringComparison.Ordinal)));

    private static bool DecimalArg(
        CanonicalRuntimePlayerCommandLoopRuntimeEvent runtimeEvent,
        string key,
        out decimal value) => decimal.TryParse(runtimeEvent.Args.GetValueOrDefault(key), NumberStyles.Number,
        CultureInfo.InvariantCulture, out value);
}
