using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Runtime;

public sealed class EncounterRuntimeService : IEncounterRuntimeService
{
    private readonly IRequirementEvaluator _requirementEvaluator;
    private readonly IOutputApplier _outputApplier;

    public EncounterRuntimeService(IRequirementEvaluator requirementEvaluator, IOutputApplier outputApplier)
    {
        _requirementEvaluator = requirementEvaluator;
        _outputApplier = outputApplier;
    }

    public GameRuntimeResult StartEncounter(GamePackageDefinition package, GameRuntimeState state, string encounterId, int? seed = null)
    {
        var encounter = package.Game.Encounters.FirstOrDefault(e => RuntimeStateHelpers.IdEquals(e.Id, encounterId));
        if (encounter == null)
        {
            return Failure(state, "encounter.missing", $"Encounter not found: {encounterId}", encounterId);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state };
        var requirements = _requirementEvaluator.Evaluate(package, working, encounter.StartRequirements);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var runtime = new EncounterRuntimeState
        {
            EncounterId = encounter.Id,
            Kind = encounter.Kind,
            Active = true,
            Round = 1,
            TurnIndex = 0,
            Metadata = new Dictionary<string, string>(encounter.Metadata)
        };
        runtime.Metadata["seed"] = (seed ?? encounter.DefaultSeed ?? RuntimeStateHelpers.StableSeed($"{package.Manifest.PackageId}:{encounter.Id}")).ToString();

        foreach (var participant in encounter.Participants)
        {
            runtime.Participants.Add(BuildParticipantState(package, working, participant));
        }

        if (runtime.Participants.Count == 0)
        {
            return Failure(state, "encounter.participants.empty", $"Encounter has no participants: {encounter.Id}", encounter.Id);
        }

        working.ActiveEncounter = runtime;
        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Success = true;
        result.Message = $"Encounter started: {encounter.Id}";
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.EncounterStarted, result.Message, encounter.Id));
        AddTurnStartedEvent(state.ActiveEncounter, result.Events);
        return result;
    }

    public GameRuntimeResult UseAbility(GamePackageDefinition package, GameRuntimeState state, string abilityId, string sourceParticipantId, string? targetParticipantId = null)
    {
        var ability = package.Game.Abilities.FirstOrDefault(a => RuntimeStateHelpers.IdEquals(a.Id, abilityId));
        if (ability == null)
        {
            return Failure(state, "ability.missing", $"Ability not found: {abilityId}", abilityId);
        }

        return ExecuteAbility(package, state, ability, sourceParticipantId, targetParticipantId, isBasicAttack: false);
    }

    public GameRuntimeResult BasicAttack(GamePackageDefinition package, GameRuntimeState state, string sourceParticipantId, string? targetParticipantId = null)
    {
        var encounter = state.ActiveEncounter;
        var definition = encounter == null
            ? null
            : package.Game.Encounters.FirstOrDefault(e => RuntimeStateHelpers.IdEquals(e.Id, encounter.EncounterId));
        var sourceDefinition = definition?.Participants.FirstOrDefault(p => RuntimeStateHelpers.IdEquals(p.Id, sourceParticipantId));
        var abilityId = ResolveBasicAttackAbilityId(package, definition, sourceDefinition);
        if (!string.IsNullOrWhiteSpace(abilityId))
        {
            var ability = package.Game.Abilities.FirstOrDefault(a => RuntimeStateHelpers.IdEquals(a.Id, abilityId));
            if (ability != null)
            {
                return ExecuteAbility(package, state, ability, sourceParticipantId, targetParticipantId, isBasicAttack: true);
            }
        }

        var fallback = new AbilityDefinition
        {
            Id = "ability/basic_attack/fallback",
            Name = "Basic Attack",
            Kind = "attack",
            Power = 1,
            Metadata = new Dictionary<string, string> { ["fallback"] = "true" }
        };
        return ExecuteAbility(package, state, fallback, sourceParticipantId, targetParticipantId, isBasicAttack: true);
    }

    public GameRuntimeResult EndTurn(GamePackageDefinition package, GameRuntimeState state)
    {
        if (state.ActiveEncounter == null || !state.ActiveEncounter.Active)
        {
            return Failure(state, "encounter.not_active", "No active encounter.", null);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state, Success = true, Message = "Turn ended." };
        TickCurrentParticipant(package, working.ActiveEncounter!, result.Events, result.Diagnostics);
        if (result.Diagnostics.Any(d => d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
        {
            result.Success = false;
            result.Message = "Turn end failed.";
            return result;
        }
        AdvanceTurn(working.ActiveEncounter!, result.Events);
        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        return result;
    }

    public GameRuntimeResult FleeEncounter(GamePackageDefinition package, GameRuntimeState state)
    {
        if (state.ActiveEncounter == null || !state.ActiveEncounter.Active)
        {
            return Failure(state, "encounter.not_active", "No active encounter.", null);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        working.ActiveEncounter!.Active = false;
        working.ActiveEncounter.ActionHistory.Add("flee");
        var result = new GameRuntimeResult
        {
            Success = true,
            State = state,
            Message = $"Encounter fled: {working.ActiveEncounter.EncounterId}"
        };
        result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.EncounterEnded, result.Message, working.ActiveEncounter.EncounterId));
        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        return result;
    }

    public GameRuntimeResult ResolveEncounter(GamePackageDefinition package, GameRuntimeState state)
    {
        if (state.ActiveEncounter == null)
        {
            return Failure(state, "encounter.not_active", "No active encounter.", null);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var result = new GameRuntimeResult { State = state, Success = true };
        ResolveEncounterIfComplete(package, working, result.Events, result.Diagnostics, force: true);
        if (result.Diagnostics.Any(d => d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
        {
            result.Success = false;
            result.Message = "Encounter resolve failed.";
            return result;
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Message = "Encounter resolved.";
        return result;
    }

    internal GameRuntimeResult ExecuteAbility(GamePackageDefinition package, GameRuntimeState state, AbilityDefinition ability, string sourceParticipantId, string? targetParticipantId, bool isBasicAttack)
    {
        if (state.ActiveEncounter == null || !state.ActiveEncounter.Active)
        {
            return Failure(state, "encounter.not_active", "No active encounter.", null);
        }

        var working = RuntimeStateHelpers.CloneState(state);
        var encounter = working.ActiveEncounter!;
        var source = encounter.Participants.FirstOrDefault(p => RuntimeStateHelpers.IdEquals(p.Id, sourceParticipantId));
        if (source == null || !source.Alive)
        {
            return Failure(state, "encounter.source_missing", $"Source participant is missing or defeated: {sourceParticipantId}", sourceParticipantId);
        }

        if (!RuntimeStateHelpers.IdEquals(CurrentTurnParticipant(encounter)?.Id, source.Id))
        {
            return Failure(state, "encounter.turn.invalid", $"It is not {source.Id}'s turn.", source.Id);
        }

        var target = ResolveTarget(encounter, source, targetParticipantId);
        if (target == null)
        {
            return Failure(state, "encounter.target_missing", "No valid ability target.", targetParticipantId);
        }

        var result = new GameRuntimeResult { State = state };
        var requirements = _requirementEvaluator.Evaluate(package, working, ability.Requirements, source.InventoryId);
        RecipeRuntimeService.AddRequirementFailures(result, requirements);
        if (!requirements.Success)
        {
            return result;
        }

        var costResult = ConsumeAbilityCosts(source, ability.Costs);
        result.Events.AddRange(costResult.Events);
        result.Diagnostics.AddRange(costResult.Diagnostics);
        if (!costResult.Success)
        {
            result.Success = false;
            result.Message = $"Ability failed: {ability.Id}";
            return result;
        }

        var outputs = BuildAbilityOutputs(ability, isBasicAttack).ToList();
        var equipmentBonus = 0d;
        var equipmentMetadataPresent = false;
        var statBonus = 0d;
        var statId = string.Empty;
        var statValue = 0d;
        var statMetadataPresent = false;
        if (isBasicAttack && RuntimeStateHelpers.IdEquals(source.Id, "player"))
        {
            if (!TryResolveEquipmentDamageBonus(package, working, out equipmentBonus,
                    out equipmentMetadataPresent, out var bonusDiagnostic))
            {
                result.Diagnostics.Add(bonusDiagnostic!);
                result.Success = false;
                result.Message = $"Ability failed: {ability.Id}";
                return result;
            }
            if (!TryResolveStatDamageBonus(package, ability, source, out statBonus, out statId,
                    out statValue, out statMetadataPresent, out var statDiagnostic))
            {
                result.Diagnostics.Add(statDiagnostic!);
                result.Success = false;
                result.Message = $"Ability failed: {ability.Id}";
                return result;
            }
            var totalBonus = equipmentBonus + statBonus;
            if (totalBonus != 0)
            {
                foreach (var output in outputs.Where(IsDamageOutput)) output.Amount += totalBonus;
            }
            if (equipmentBonus > 0)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(
                    "combat.equipment_damage_bonus.applied",
                    "Equipped weapon damage bonus applied: +" + Format(equipmentBonus),
                    source.Id,
                    "info"));
            }
            if (statMetadataPresent)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic(
                    "combat.stat_damage_bonus.applied",
                    "Source stat damage bonus applied: " + Format(statBonus),
                    statId,
                    "info"));
            }
        }
        foreach (var output in outputs)
        {
            ApplyEncounterOutput(package, encounter, source, target, output, result.Events, result.Diagnostics,
                equipmentBonus, equipmentMetadataPresent, statBonus, statId, statValue, statMetadataPresent, ability.Id);
        }

        encounter.ActionHistory.Add($"{source.Id}:{ability.Id}:{target.Id}");
        result.Events.Add(RuntimeStateHelpers.Event(
            GameRuntimeEventType.AbilityUsed,
            $"Ability used: {ability.Id}",
            ability.Id,
            new Dictionary<string, string> { ["source"] = source.Id, ["target"] = target.Id }));

        MarkDefeated(package, encounter, result.Events);
        ResolveEncounterIfComplete(package, working, result.Events, result.Diagnostics, force: false);
        if (working.ActiveEncounter?.Active == true)
        {
            AdvanceTurn(working.ActiveEncounter, result.Events);
        }

        if (result.Diagnostics.Any(d => d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
        {
            result.Success = false;
            result.Message = $"Ability failed: {ability.Id}";
            return result;
        }

        RuntimeStateHelpers.CopyState(working, state);
        result.State = state;
        result.Success = true;
        result.Message = $"Ability used: {ability.Id}";
        return result;
    }

    private static EncounterParticipantState BuildParticipantState(
        GamePackageDefinition package,
        GameRuntimeState runtimeState,
        EncounterParticipantDefinition definition)
    {
        var state = new EncounterParticipantState
        {
            Id = definition.Id,
            Name = definition.Name,
            Team = string.IsNullOrWhiteSpace(definition.Team) ? definition.Kind : definition.Team,
            Alive = true,
            InventoryId = definition.InventoryId,
            Metadata = new Dictionary<string, string>(definition.Metadata)
        };

        foreach (var stat in definition.Stats)
        {
            state.Stats.Add(new StatValueState { StatId = stat.Id, Value = stat.Amount });
        }

        foreach (var statDefinition in package.Game.Stats)
        {
            if (!state.Stats.Any(s => RuntimeStateHelpers.IdEquals(s.StatId, statDefinition.Id)) && statDefinition.DefaultValue.HasValue)
            {
                state.Stats.Add(new StatValueState { StatId = statDefinition.Id, Value = statDefinition.DefaultValue.Value });
            }
        }

        // Current player-owned Runtime stats are authoritative at encounter start.
        // Explicit participant values remain authoritative for non-player participants and
        // are the player fallback when no current Runtime value exists.
        if (RuntimeStateHelpers.KindEquals(state.Team, "player"))
        {
            foreach (var current in runtimeState.Stats)
            {
                var existing = state.Stats.FirstOrDefault(stat => RuntimeStateHelpers.IdEquals(stat.StatId, current.StatId));
                if (existing is null)
                    state.Stats.Add(new StatValueState { StatId = current.StatId, Value = current.Value });
                else
                    existing.Value = current.Value;
            }
        }

        foreach (var resource in definition.Resources)
        {
            var resourceDefinition = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, resource.Id));
            state.Resources.Add(new ResourceState
            {
                ResourceId = resource.Id,
                Amount = resource.Amount,
                Capacity = resourceDefinition?.MaxValue,
                Scope = "participant",
                OwnerId = definition.Id
            });
        }

        return state;
    }

    private static IEnumerable<OutputDefinition> BuildAbilityOutputs(AbilityDefinition ability, bool isBasicAttack)
    {
        foreach (var effect in ability.Effects)
        {
            yield return RuntimeEffectMapper.ToOutput(effect);
        }

        if (ability.Effects.Count == 0 && (isBasicAttack || RuntimeStateHelpers.KindEquals(ability.Kind, "attack")))
        {
            yield return new OutputDefinition
            {
                Kind = "damage_resource",
                Id = ability.ResourceId ?? "resource/health",
                Amount = ability.Power.GetValueOrDefault(1)
            };
        }
    }

    private static CostConsumptionResult ConsumeAbilityCosts(EncounterParticipantState source, IEnumerable<CostDefinition> costs)
    {
        var result = new CostConsumptionResult();
        var materialized = costs.ToList();
        foreach (var cost in materialized)
        {
            if (cost.Amount <= 0)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.cost.amount.invalid", "Ability cost amount must be positive.", cost.Id));
                continue;
            }

            if (!IsResourceCost(cost))
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.cost.kind.unsupported", $"Unsupported encounter ability cost kind: {cost.Kind}", cost.Id));
                continue;
            }

            var resource = source.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.ResourceId, cost.Id));
            if (resource == null || resource.Amount < cost.Amount)
            {
                result.Diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.cost.resource_too_low", $"Participant resource {cost.Id} is too low.", cost.Id));
                continue;
            }

        }

        if (!result.Success) return result;
        foreach (var cost in materialized)
        {
            var resource = source.Resources.First(r => RuntimeStateHelpers.IdEquals(r.ResourceId, cost.Id));
            resource.Amount -= cost.Amount;
            result.Events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.CostConsumed,
                $"Consumed ability resource {cost.Id} x{Format(cost.Amount)}", cost.Id,
                new Dictionary<string, string>
                {
                    ["sourceParticipantId"] = source.Id,
                    ["before"] = Format(resource.Amount + cost.Amount),
                    ["cost"] = Format(cost.Amount),
                    ["after"] = Format(resource.Amount)
                }));
        }

        return result;
    }

    private static void ApplyEncounterOutput(
        GamePackageDefinition package,
        EncounterRuntimeState encounter,
        EncounterParticipantState source,
        EncounterParticipantState target,
        OutputDefinition output,
        List<GameRuntimeEvent> events,
        List<RuntimeDiagnostic> diagnostics,
        double equipmentBonus,
        bool equipmentMetadataPresent,
        double statBonus,
        string statId,
        double statValue,
        bool statMetadataPresent,
        string sourceAbilityId)
    {
        var outputTarget = ResolveOutputTarget(encounter, source, target, output);
        if (IsDamageOutput(output))
        {
            var resource = FindParticipantResource(outputTarget, output.Id);
            if (resource == null)
            {
                diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.effect.resource_missing", $"Damage references missing participant resource: {output.Id}", output.Id));
                return;
            }

            resource.Amount = Math.Max(0, resource.Amount - Math.Abs(output.Amount));
            Dictionary<string, string>? args = null;
            if (statMetadataPresent)
                args = new Dictionary<string, string>
                {
                    ["source"] = source.Id,
                    ["damage"] = Format(Math.Abs(output.Amount)),
                    ["equipmentDamageBonus"] = Format(equipmentBonus),
                    ["statId"] = statId,
                    ["statValue"] = Format(statValue),
                    ["statDamageBonus"] = Format(statBonus),
                    ["totalAdditionalDamage"] = Format(equipmentBonus + statBonus)
                };
            else if (equipmentMetadataPresent)
                args = new Dictionary<string, string>
                {
                    ["source"] = source.Id,
                    ["damage"] = Format(Math.Abs(output.Amount)),
                    ["equipmentDamageBonus"] = Format(equipmentBonus)
                };
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.DamageApplied,
                $"Damage applied: {output.Id} -{Format(Math.Abs(output.Amount))}", outputTarget.Id, args));
            return;
        }

        if (IsHealingOutput(output))
        {
            var resource = FindParticipantResource(outputTarget, output.Id);
            if (resource == null)
            {
                diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.effect.resource_missing", $"Healing references missing participant resource: {output.Id}", output.Id));
                return;
            }

            var definition = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, output.Id));
            var max = resource.Capacity ?? definition?.MaxValue;
            resource.Amount = RuntimeStateHelpers.Clamp(resource.Amount + Math.Abs(output.Amount), definition?.MinValue, max);
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.HealingApplied, $"Healing applied: {output.Id} +{Format(Math.Abs(output.Amount))}", outputTarget.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "add_status") || RuntimeStateHelpers.KindEquals(output.Kind, "status"))
        {
            if (!package.Game.Statuses.Any(status => RuntimeStateHelpers.IdEquals(status.Id, output.Id)))
            {
                diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.effect.status_missing",
                    $"Status definition not found: {output.Id}", output.Id));
                return;
            }

            var duration = output.Amount > 0 ? (long?)Math.Ceiling(output.Amount) : null;
            var existing = outputTarget.Statuses.FirstOrDefault(status => RuntimeStateHelpers.IdEquals(status.StatusId, output.Id));
            if (existing is null)
            {
                existing = new StatusState { StatusId = output.Id, TargetId = outputTarget.Id };
                outputTarget.Statuses.Add(existing);
            }
            existing.RemainingTicks = duration;
            existing.Stacks = 1;
            existing.Metadata["sourceParticipantId"] = source.Id;
            existing.Metadata["sourceAbilityId"] = sourceAbilityId;
            existing.Metadata["appliedRound"] = encounter.Round.ToString();
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.StatusAdded,
                $"Status added: {output.Id}", outputTarget.Id,
                new Dictionary<string, string>
                {
                    ["statusId"] = output.Id,
                    ["duration"] = duration?.ToString() ?? string.Empty,
                    ["sourceParticipantId"] = source.Id,
                    ["sourceAbilityId"] = sourceAbilityId,
                    ["appliedRound"] = encounter.Round.ToString()
                }));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "remove_status"))
        {
            outputTarget.Statuses.RemoveAll(status => RuntimeStateHelpers.IdEquals(status.StatusId, output.Id));
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.StatusRemoved, $"Status removed: {output.Id}", outputTarget.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "change_stat"))
        {
            var stat = outputTarget.Stats.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.StatId, output.Id));
            if (stat == null)
            {
                diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.effect.stat_missing", $"Effect references missing participant stat: {output.Id}", output.Id));
                return;
            }

            var definition = package.Game.Stats.FirstOrDefault(s => RuntimeStateHelpers.IdEquals(s.Id, output.Id));
            stat.Value = RuntimeStateHelpers.Clamp(stat.Value + output.Amount, definition?.MinValue, definition?.MaxValue);
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.OutputApplied, $"Stat changed: {output.Id}", outputTarget.Id));
            return;
        }

        if (RuntimeStateHelpers.KindEquals(output.Kind, "log") || RuntimeStateHelpers.KindEquals(output.Kind, "log_message"))
        {
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.LogMessageAdded, output.Metadata.TryGetValue("message", out var message) ? message : output.Id, output.Id));
            return;
        }

        diagnostics.Add(RuntimeStateHelpers.Diagnostic("ability.effect.kind.unknown", $"Unsupported encounter ability output kind: {output.Kind}", output.Id));
    }

    private void ResolveEncounterIfComplete(GamePackageDefinition package, GameRuntimeState working, List<GameRuntimeEvent> events, List<RuntimeDiagnostic> diagnostics, bool force)
    {
        var encounter = working.ActiveEncounter;
        if (encounter == null || !encounter.Active)
        {
            return;
        }

        var won = IsWon(encounter);
        var lost = IsLost(encounter);
        if (!won && !lost && !force)
        {
            return;
        }

        var definition = package.Game.Encounters.FirstOrDefault(e => RuntimeStateHelpers.IdEquals(e.Id, encounter.EncounterId));
        if (definition == null)
        {
            diagnostics.Add(RuntimeStateHelpers.Diagnostic("encounter.definition_missing", $"Encounter definition missing: {encounter.EncounterId}", encounter.EncounterId));
            return;
        }

        if (won)
        {
            ApplyRewards(package, working, definition, events, diagnostics);
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.EncounterWon, $"Encounter won: {definition.Id}", definition.Id));
        }
        else if (lost)
        {
            var consequences = _outputApplier.Apply(package, working, definition.Consequences);
            events.AddRange(consequences.Events);
            diagnostics.AddRange(consequences.Diagnostics);
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.EncounterLost, $"Encounter lost: {definition.Id}", definition.Id));
        }

        encounter.Active = false;
        events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.EncounterEnded, $"Encounter ended: {definition.Id}", definition.Id));
    }

    private void ApplyRewards(GamePackageDefinition package, GameRuntimeState working, EncounterDefinition definition, List<GameRuntimeEvent> events, List<RuntimeDiagnostic> diagnostics)
    {
        var outputs = definition.Rewards.ToList();
        if (!string.IsNullOrWhiteSpace(definition.LootTableId))
        {
            outputs.Add(new OutputDefinition { Kind = "loot", Id = definition.LootTableId!, Amount = 1 });
        }

        var seed = definition.DefaultSeed ?? RuntimeStateHelpers.StableSeed($"{package.Manifest.PackageId}:{definition.Id}:reward");
        var reward = _outputApplier.Apply(package, working, outputs, seed: seed);
        events.AddRange(reward.Events);
        diagnostics.AddRange(reward.Diagnostics);
        if (reward.Success)
        {
            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.RewardGranted, $"Encounter rewards granted: {definition.Id}", definition.Id));
        }
    }

    private static void MarkDefeated(GamePackageDefinition package, EncounterRuntimeState encounter, List<GameRuntimeEvent> events)
    {
        foreach (var participant in encounter.Participants.Where(p => p.Alive))
        {
            var health = FindHealthResource(package, participant);
            if (health != null && health.Amount <= 0)
            {
                participant.Alive = false;
                events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.ParticipantDefeated, $"Participant defeated: {participant.Id}", participant.Id));
            }
        }
    }

    private static ResourceState? FindHealthResource(GamePackageDefinition package, EncounterParticipantState participant)
    {
        var byId = participant.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.ResourceId, "resource/health"));
        if (byId != null)
        {
            return byId;
        }

        return participant.Resources.FirstOrDefault(resource =>
        {
            var definition = package.Game.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.Id, resource.ResourceId));
            return definition != null
                && (RuntimeStateHelpers.KindEquals(definition.Kind, "health")
                    || definition.Tags.Any(tag => RuntimeStateHelpers.KindEquals(tag, "health")));
        });
    }

    private static ResourceState? FindParticipantResource(EncounterParticipantState participant, string resourceId)
    {
        return participant.Resources.FirstOrDefault(r => RuntimeStateHelpers.IdEquals(r.ResourceId, resourceId));
    }

    private static EncounterParticipantState? ResolveTarget(EncounterRuntimeState encounter, EncounterParticipantState source, string? targetParticipantId)
    {
        if (!string.IsNullOrWhiteSpace(targetParticipantId))
        {
            return encounter.Participants.FirstOrDefault(p => RuntimeStateHelpers.IdEquals(p.Id, targetParticipantId) && p.Alive);
        }

        return encounter.Participants.FirstOrDefault(p => p.Alive && !RuntimeStateHelpers.KindEquals(p.Team, source.Team));
    }

    private static EncounterParticipantState ResolveOutputTarget(EncounterRuntimeState encounter, EncounterParticipantState source, EncounterParticipantState target, OutputDefinition output)
    {
        if (RuntimeStateHelpers.KindEquals(output.Scope, "source"))
        {
            return source;
        }

        if (!string.IsNullOrWhiteSpace(output.Scope))
        {
            return encounter.Participants.FirstOrDefault(p => RuntimeStateHelpers.IdEquals(p.Id, output.Scope)) ?? target;
        }

        return target;
    }

    private static void AdvanceTurn(EncounterRuntimeState encounter, List<GameRuntimeEvent> events)
    {
        if (!encounter.Active || encounter.Participants.Count == 0)
        {
            return;
        }

        for (var attempts = 0; attempts < encounter.Participants.Count; attempts++)
        {
            encounter.TurnIndex++;
            if (encounter.TurnIndex >= encounter.Participants.Count)
            {
                encounter.TurnIndex = 0;
                encounter.Round++;
            }

            if (encounter.Participants[encounter.TurnIndex].Alive)
            {
                AddTurnStartedEvent(encounter, events);
                return;
            }
        }
    }

    private static void AddTurnStartedEvent(EncounterRuntimeState? encounter, List<GameRuntimeEvent> events)
    {
        var participant = CurrentTurnParticipant(encounter);
        if (encounter == null || participant == null)
        {
            return;
        }

        events.Add(RuntimeStateHelpers.Event(
            GameRuntimeEventType.TurnStarted,
            $"Turn started: {participant.Id}",
            participant.Id,
            new Dictionary<string, string> { ["encounterId"] = encounter.EncounterId, ["round"] = encounter.Round.ToString() }));
    }

    private static EncounterParticipantState? CurrentTurnParticipant(EncounterRuntimeState? encounter)
    {
        if (encounter == null || encounter.Participants.Count == 0)
        {
            return null;
        }

        var index = Math.Max(0, Math.Min(encounter.TurnIndex, encounter.Participants.Count - 1));
        return encounter.Participants[index];
    }

    private static void TickCurrentParticipant(
        GamePackageDefinition package,
        EncounterRuntimeState encounter,
        List<GameRuntimeEvent> events,
        List<RuntimeDiagnostic> diagnostics)
    {
        var participant = CurrentTurnParticipant(encounter);
        if (participant == null)
        {
            return;
        }

        foreach (var key in participant.Cooldowns.Keys.ToList())
        {
            participant.Cooldowns[key] = Math.Max(0, participant.Cooldowns[key] - 1);
        }

        foreach (var status in participant.Statuses.ToList())
        {
            var definition = package.Game.Statuses.FirstOrDefault(item => RuntimeStateHelpers.IdEquals(item.Id, status.StatusId));
            if (definition is null)
            {
                diagnostics.Add(RuntimeStateHelpers.Diagnostic("status.tick.definition_missing",
                    $"Status definition not found while ticking: {status.StatusId}", status.StatusId));
                return;
            }

            var eventStart = events.Count;
            foreach (var effect in definition.Effects)
            {
                var output = RuntimeEffectMapper.ToOutput(effect);
                ApplyEncounterOutput(package, encounter, participant, participant, output, events, diagnostics,
                    0, false, 0, string.Empty, 0, false,
                    status.Metadata.TryGetValue("sourceAbilityId", out var abilityId) ? abilityId : string.Empty);
                if (diagnostics.Any(d => d.Severity.Equals("error", StringComparison.OrdinalIgnoreCase)))
                {
                    if (events.Count > eventStart) events.RemoveRange(eventStart, events.Count - eventStart);
                    return;
                }
            }

            events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.StatusTicked,
                $"Status ticked: {status.StatusId}", participant.Id,
                new Dictionary<string, string>
                {
                    ["statusId"] = status.StatusId,
                    ["remainingTicksBefore"] = status.RemainingTicks?.ToString() ?? string.Empty,
                    ["stacks"] = status.Stacks.ToString()
                }));
            if (!status.RemainingTicks.HasValue)
            {
                continue;
            }

            status.RemainingTicks = Math.Max(0, status.RemainingTicks.Value - 1);
            if (status.RemainingTicks.Value == 0)
            {
                participant.Statuses.Remove(status);
                events.Add(RuntimeStateHelpers.Event(GameRuntimeEventType.StatusRemoved, $"Status expired: {status.StatusId}", participant.Id));
            }
        }
    }

    private static bool IsWon(EncounterRuntimeState encounter)
    {
        return encounter.Participants.Any(p => p.Alive && RuntimeStateHelpers.KindEquals(p.Team, "player"))
            && encounter.Participants.Where(p => p.Alive).All(p => RuntimeStateHelpers.KindEquals(p.Team, "player"));
    }

    private static bool IsLost(EncounterRuntimeState encounter)
    {
        return encounter.Participants.Any(p => RuntimeStateHelpers.KindEquals(p.Team, "player"))
            && !encounter.Participants.Any(p => p.Alive && RuntimeStateHelpers.KindEquals(p.Team, "player"));
    }

    private static string? ResolveBasicAttackAbilityId(GamePackageDefinition package, EncounterDefinition? encounter, EncounterParticipantDefinition? source)
    {
        if (encounter?.Metadata.TryGetValue("default_attack_ability_id", out var fromEncounter) == true)
        {
            return fromEncounter;
        }

        var participantAbility = source?.Abilities.FirstOrDefault(abilityId =>
            package.Game.Abilities.Any(ability => RuntimeStateHelpers.IdEquals(ability.Id, abilityId) && ability.Tags.Any(tag => RuntimeStateHelpers.KindEquals(tag, "basic_attack"))));
        if (!string.IsNullOrWhiteSpace(participantAbility))
        {
            return participantAbility;
        }

        return source?.Abilities.FirstOrDefault(abilityId =>
            package.Game.Abilities.Any(ability => RuntimeStateHelpers.IdEquals(ability.Id, abilityId) && RuntimeStateHelpers.KindEquals(ability.Kind, "attack")));
    }

    private static bool IsDamageOutput(OutputDefinition output)
    {
        return RuntimeStateHelpers.KindEquals(output.Kind, "damage_resource")
            || RuntimeStateHelpers.KindEquals(output.Kind, "damage")
            || (RuntimeStateHelpers.KindEquals(output.Kind, "change_resource") && output.Amount < 0);
    }

    private static bool IsHealingOutput(OutputDefinition output)
    {
        return RuntimeStateHelpers.KindEquals(output.Kind, "heal_resource")
            || RuntimeStateHelpers.KindEquals(output.Kind, "heal")
            || (RuntimeStateHelpers.KindEquals(output.Kind, "change_resource") && output.Amount > 0);
    }

    private static bool IsResourceCost(CostDefinition cost)
    {
        return RuntimeStateHelpers.KindEquals(cost.Kind, "resource")
            || RuntimeStateHelpers.KindEquals(cost.Kind, "abstract_resource");
    }

    private static bool TryResolveEquipmentDamageBonus(
        GamePackageDefinition package,
        GameRuntimeState state,
        out double bonus,
        out bool metadataPresent,
        out RuntimeDiagnostic? diagnostic)
    {
        bonus = 0;
        metadataPresent = false;
        diagnostic = null;
        var equipment = state.Equipment.FirstOrDefault(item =>
            RuntimeStateHelpers.KindEquals(item.OwnerKind, "player")
            && (RuntimeStateHelpers.IdEquals(item.OwnerId, state.PlayerEntityId)
                || string.IsNullOrWhiteSpace(item.OwnerId)))
                        ?? state.Equipment.FirstOrDefault(item => RuntimeStateHelpers.KindEquals(item.OwnerKind, "player"));
        if (equipment is null) return true;
        foreach (var slot in equipment.Slots.Where(item => !string.IsNullOrWhiteSpace(item.ItemId)))
        {
            var item = package.Game.Items.FirstOrDefault(definition => RuntimeStateHelpers.IdEquals(definition.Id, slot.ItemId));
            if (item is null)
            {
                diagnostic = RuntimeStateHelpers.Diagnostic("combat.equipment_item_missing",
                    "Equipped item definition is missing: " + slot.ItemId, slot.ItemId);
                return false;
            }
            if (!item.Metadata.TryGetValue("combat_damage_bonus", out var raw)) continue;
            metadataPresent = true;
            if (!double.TryParse(raw, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var value) || value < 0)
            {
                diagnostic = RuntimeStateHelpers.Diagnostic("combat.equipment_damage_bonus.invalid",
                    "Equipped item combat_damage_bonus is invalid: " + item.Id, item.Id);
                return false;
            }
            bonus += value;
        }
        return true;
    }

    private static bool TryResolveStatDamageBonus(
        GamePackageDefinition package,
        AbilityDefinition ability,
        EncounterParticipantState source,
        out double bonus,
        out string statId,
        out double statValue,
        out bool metadataPresent,
        out RuntimeDiagnostic? diagnostic)
    {
        const string statIdKey = "source_stat_damage_stat_id";
        const string baselineKey = "source_stat_damage_baseline";
        const string perPointKey = "source_stat_damage_per_point";
        bonus = 0;
        statId = string.Empty;
        statValue = 0;
        diagnostic = null;
        metadataPresent = ability.Metadata.ContainsKey(statIdKey)
                          || ability.Metadata.ContainsKey(baselineKey)
                          || ability.Metadata.ContainsKey(perPointKey);
        if (!metadataPresent) return true;
        if (!ability.Metadata.TryGetValue(statIdKey, out statId)
            || string.IsNullOrWhiteSpace(statId)
            || !ability.Metadata.TryGetValue(baselineKey, out var rawBaseline)
            || !ability.Metadata.TryGetValue(perPointKey, out var rawPerPoint))
        {
            diagnostic = RuntimeStateHelpers.Diagnostic("combat.stat_damage_metadata.invalid",
                "Source stat damage metadata is incomplete: " + ability.Id, ability.Id);
            return false;
        }
        var resolvedStatId = statId;
        if (!double.TryParse(rawBaseline, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var baseline)
            || !double.IsFinite(baseline)
            || !double.TryParse(rawPerPoint, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var perPoint)
            || !double.IsFinite(perPoint) || perPoint < 0)
        {
            diagnostic = RuntimeStateHelpers.Diagnostic("combat.stat_damage_metadata.invalid",
                "Source stat damage baseline or multiplier is invalid: " + ability.Id, ability.Id);
            return false;
        }
        if (package.Game.Stats.Count(stat => RuntimeStateHelpers.IdEquals(stat.Id, resolvedStatId)) != 1)
        {
            diagnostic = RuntimeStateHelpers.Diagnostic("combat.stat_definition.missing",
                "Source stat definition is missing or ambiguous: " + statId, statId);
            return false;
        }
        var statState = source.Stats.SingleOrDefault(stat => RuntimeStateHelpers.IdEquals(stat.StatId, resolvedStatId));
        if (statState is null)
        {
            diagnostic = RuntimeStateHelpers.Diagnostic("combat.source_stat.missing",
                "Source participant stat is missing: " + statId, statId);
            return false;
        }
        statValue = statState.Value;
        bonus = (statValue - baseline) * perPoint;
        return true;
    }

    private static GameRuntimeResult Failure(GameRuntimeState state, string code, string message, string? targetId)
    {
        return new GameRuntimeResult
        {
            Success = false,
            State = state,
            Message = message,
            Diagnostics = new List<RuntimeDiagnostic> { RuntimeStateHelpers.Diagnostic(code, message, targetId) },
            Events = new List<GameRuntimeEvent> { RuntimeStateHelpers.Event(GameRuntimeEventType.ValidationFailed, message, targetId) }
        };
    }

    private static string Format(double value)
    {
        return value.ToString("0.####");
    }
}
