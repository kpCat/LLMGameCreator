using System.Globalization;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;

public sealed class CapabilityDrivenRuntimePlaythroughExpansionService
{
    public const string EndTurnUntilStatusExpired = "end_turn_until_status_expired";
    public const string EndTurnUntilParticipantCurrent = "end_turn_until_participant_current";

    public CapabilityDrivenRuntimePlaythroughExpansionResult Expand(
        IReadOnlyList<FeatureModuleDefinition> selectedModules,
        GamePackageDefinition package)
    {
        var diagnostics = new List<string>();
        var expandedModules = selectedModules.Select(module => module with
        {
            RuntimePlaythroughContracts = ExpandContracts(module, package, diagnostics)
        }).ToList();
        return new CapabilityDrivenRuntimePlaythroughExpansionResult
        {
            Modules = expandedModules,
            Diagnostics = diagnostics
        };
    }

    private static IReadOnlyList<FeatureModuleRuntimePlaythroughContract> ExpandContracts(
        FeatureModuleDefinition module,
        GamePackageDefinition package,
        List<string> diagnostics)
    {
        var result = new List<FeatureModuleRuntimePlaythroughContract>();
        foreach (var contract in module.RuntimePlaythroughContracts)
        {
            if (string.IsNullOrWhiteSpace(contract.ExpansionKind))
            {
                result.Add(contract);
                continue;
            }

            if (contract.ExpansionKind.Equals(EndTurnUntilParticipantCurrent, StringComparison.Ordinal))
            {
                result.AddRange(ExpandParticipantRotation(contract, package, diagnostics));
                continue;
            }
            if (!contract.ExpansionKind.Equals(EndTurnUntilStatusExpired, StringComparison.Ordinal))
            {
                diagnostics.Add("unknown playthrough expansion kind rejected: " + contract.ExpansionKind);
                continue;
            }

            result.AddRange(ExpandStatusLifecycle(contract, package, diagnostics));
        }
        return result;
    }

    private static IReadOnlyList<FeatureModuleRuntimePlaythroughContract> ExpandParticipantRotation(
        FeatureModuleRuntimePlaythroughContract descriptor,
        GamePackageDefinition package,
        List<string> diagnostics)
    {
        var encounterId = Arg(descriptor, "encounterId");
        var sourceParticipantId = Arg(descriptor, "sourceParticipantId");
        var expectedParticipantId = Arg(descriptor, "expectedCurrentParticipantId");
        var encounter = package.Game.Encounters.SingleOrDefault(item => item.Id == encounterId);
        if (encounter is null)
        {
            diagnostics.Add("participant rotation encounter rejected: " + descriptor.ActionId + ":" + encounterId);
            return [];
        }
        var sourceIndex = encounter.Participants.FindIndex(item => item.Id == sourceParticipantId);
        if (sourceIndex < 0 || encounter.Participants.Count(item => item.Id == expectedParticipantId) != 1)
        {
            diagnostics.Add("participant rotation binding rejected: " + descriptor.ActionId + ":"
                            + sourceParticipantId + ":" + expectedParticipantId);
            return [];
        }
        var turns = new List<string>();
        var participantIndex = sourceIndex;
        while (encounter.Participants[(participantIndex + 1) % encounter.Participants.Count].Id != expectedParticipantId)
        {
            participantIndex = (participantIndex + 1) % encounter.Participants.Count;
            turns.Add(encounter.Participants[participantIndex].Id);
            if (turns.Count >= encounter.Participants.Count)
            {
                diagnostics.Add("participant rotation could not reach expected participant: " + descriptor.ActionId);
                return [];
            }
        }
        if (turns.Count == 0)
        {
            diagnostics.Add("participant rotation produced no EndTurn actions: " + descriptor.ActionId);
            return [];
        }
        return ExplicitEndTurns(descriptor, turns, encounterId, string.Empty, string.Empty, 0, 0);
    }

    private static IReadOnlyList<FeatureModuleRuntimePlaythroughContract> ExpandStatusLifecycle(
        FeatureModuleRuntimePlaythroughContract descriptor,
        GamePackageDefinition package,
        List<string> diagnostics)
    {
        var encounterId = Arg(descriptor, "encounterId");
        var targetParticipantId = Arg(descriptor, "targetParticipantId");
        var sourceParticipantId = Arg(descriptor, "sourceParticipantId");
        var resumeParticipantId = Arg(descriptor, "resumeParticipantId");
        var sourceAbilityId = Arg(descriptor, "sourceAbilityId");
        var statusId = Arg(descriptor, "statusId");
        var checkpointAfterTick = ParsePositiveInt(Arg(descriptor, "checkpointAfterTick"), "checkpointAfterTick",
            descriptor.ActionId, diagnostics);
        var encounter = package.Game.Encounters.SingleOrDefault(item => item.Id == encounterId);
        var ability = package.Game.Abilities.SingleOrDefault(item => item.Id == sourceAbilityId);
        if (encounter is null)
            diagnostics.Add("status lifecycle encounter rejected: " + descriptor.ActionId + ":" + encounterId);
        if (ability is null)
            diagnostics.Add("status lifecycle source ability rejected: " + descriptor.ActionId + ":" + sourceAbilityId);
        if (encounter is null || ability is null || checkpointAfterTick <= 0) return [];

        var matchingEffects = ability.Effects.Where(effect =>
                (effect.Type.Equals("add_status", StringComparison.OrdinalIgnoreCase)
                 || effect.Type.Equals("status", StringComparison.OrdinalIgnoreCase))
                && effect.Args.GetValueOrDefault("id") == statusId)
            .ToList();
        if (matchingEffects.Count != 1)
        {
            diagnostics.Add("status lifecycle add-status effect rejected: " + descriptor.ActionId + ":"
                            + sourceAbilityId + ":" + statusId);
            return [];
        }

        var duration = ParsePositiveInt(matchingEffects[0].Args.GetValueOrDefault("amount"), "duration",
            descriptor.ActionId, diagnostics);
        if (duration <= 0) return [];
        if (checkpointAfterTick > duration)
        {
            diagnostics.Add("status lifecycle checkpoint tick exceeds duration: " + descriptor.ActionId + ":"
                            + checkpointAfterTick + ">" + duration);
            return [];
        }

        var sourceIndex = encounter.Participants.FindIndex(item => item.Id == sourceParticipantId);
        var targetCount = encounter.Participants.Count(item => item.Id == targetParticipantId);
        if (sourceIndex < 0 || targetCount != 1 || encounter.Participants.Count == 0)
        {
            diagnostics.Add("status lifecycle participant binding rejected: " + descriptor.ActionId + ":"
                            + sourceParticipantId + ":" + targetParticipantId);
            return [];
        }

        var turns = new List<string>();
        var targetTicks = 0;
        var participantIndex = sourceIndex;
        while (targetTicks < duration)
        {
            participantIndex = (participantIndex + 1) % encounter.Participants.Count;
            var participantId = encounter.Participants[participantIndex].Id;
            turns.Add(participantId);
            if (participantId == targetParticipantId) targetTicks++;
        }
        if (!string.IsNullOrWhiteSpace(resumeParticipantId))
        {
            var resumeCount = encounter.Participants.Count(item => item.Id == resumeParticipantId);
            if (resumeCount != 1)
            {
                diagnostics.Add("status lifecycle resume participant rejected: " + descriptor.ActionId + ":"
                                + resumeParticipantId);
                return [];
            }
            while (encounter.Participants[(participantIndex + 1) % encounter.Participants.Count].Id != resumeParticipantId)
            {
                participantIndex = (participantIndex + 1) % encounter.Participants.Count;
                turns.Add(encounter.Participants[participantIndex].Id);
            }
        }

        return ExplicitEndTurns(descriptor, turns, encounterId, statusId, targetParticipantId, duration,
            checkpointAfterTick);
    }

    private static IReadOnlyList<FeatureModuleRuntimePlaythroughContract> ExplicitEndTurns(
        FeatureModuleRuntimePlaythroughContract descriptor,
        IReadOnlyList<string> turns,
        string encounterId,
        string statusId,
        string targetParticipantId,
        int duration,
        int checkpointAfterTick)
    {
        var actions = new List<FeatureModuleRuntimePlaythroughContract>(turns.Count);
        string? previousActionId = null;
        for (var index = 0; index < turns.Count; index++)
        {
            var participantId = turns[index];
            var targetTick = participantId == targetParticipantId
                ? turns.Take(index + 1).Count(item => item == targetParticipantId)
                : 0;
            var isTerminal = index == turns.Count - 1;
            var actionId = isTerminal ? descriptor.ActionId : descriptor.ActionId + ".turn." + (index + 1).ToString("D4", CultureInfo.InvariantCulture);
            var dependencies = previousActionId is null ? descriptor.DependsOnActionIds : [previousActionId];
            actions.Add(descriptor with
            {
                ContractId = isTerminal ? descriptor.ContractId : descriptor.ContractId + ".turn." + (index + 1).ToString("D4", CultureInfo.InvariantCulture),
                ActionId = actionId,
                RuntimePrimitiveId = CapabilityRuntimePrimitiveIds.EndTurn,
                ExpansionKind = string.Empty,
                TargetSelector = "encounter_participant_id",
                Args = new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["id"] = participantId,
                    ["encounterId"] = encounterId,
                    ["statusId"] = statusId,
                    ["statusTargetParticipantId"] = targetParticipantId,
                    ["targetTick"] = targetTick.ToString(CultureInfo.InvariantCulture),
                    ["configuredDuration"] = duration.ToString(CultureInfo.InvariantCulture)
                },
                DependsOnActionIds = dependencies,
                CheckpointBoundaryAfter = targetTick == checkpointAfterTick,
                PresentationOnly = false
            });
            previousActionId = actionId;
        }
        return actions;
    }

    private static string Arg(FeatureModuleRuntimePlaythroughContract contract, string key) =>
        contract.Args.GetValueOrDefault(key) ?? string.Empty;

    private static int ParsePositiveInt(string? raw, string field, string actionId, List<string> diagnostics)
    {
        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric)
            && numeric > 0 && numeric == decimal.Truncate(numeric) && numeric <= int.MaxValue)
            return decimal.ToInt32(numeric);
        diagnostics.Add("status lifecycle " + field + " rejected: " + actionId + ":" + raw);
        return 0;
    }
}
