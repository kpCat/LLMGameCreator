using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed class GeneratedEncounterCombatContractService
{
    public GeneratedEncounterCombatContractResult Resolve(
        GamePackageDefinition laneAPackage,
        GeneratedProjectOverlayDocument generatedOverlay,
        IUnifiedGameRuntimeService runtime)
    {
        ArgumentNullException.ThrowIfNull(laneAPackage);
        ArgumentNullException.ThrowIfNull(generatedOverlay);
        ArgumentNullException.ThrowIfNull(runtime);

        var generatedEncounterIds = generatedOverlay.GeneratedRecords
            .Where(item => string.Equals(item.CollectionPath, "game.encounters", StringComparison.Ordinal))
            .Select(item => item.RecordId)
            .ToHashSet(StringComparer.Ordinal);
        var duplicateIds = laneAPackage.Game.Encounters
            .GroupBy(item => item.Id, StringComparer.Ordinal)
            .Where(group => group.Count() != 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        var encounters = laneAPackage.Game.Encounters
            .Where(item => !generatedEncounterIds.Contains(item.Id) && !duplicateIds.Contains(item.Id))
            .Where(item => item.Participants.Any(participant => IsPlayer(participant.Team))
                           && item.Participants.Any(participant => !IsPlayer(participant.Team)))
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .ToList();

        var diagnostics = new List<string>();
        var rolePairs = new List<RolePair>();
        foreach (var encounter in encounters)
        {
            if (!ReferencesResolveExactly(laneAPackage, encounter))
            {
                diagnostics.Add("generated_combat.definition_reference_invalid");
                continue;
            }

            foreach (var player in encounter.Participants.Where(item => IsPlayer(item.Team))
                         .OrderBy(item => item.Id, StringComparer.Ordinal))
            foreach (var opponent in encounter.Participants.Where(item => !IsPlayer(item.Team))
                         .OrderBy(item => item.Id, StringComparer.Ordinal))
            {
                if (!HasRuntimeHealth(laneAPackage, player) || !HasRuntimeHealth(laneAPackage, opponent))
                {
                    diagnostics.Add("generated_combat.definition_reference_invalid");
                    continue;
                }
                var playerRole = Role(encounter.Id, player);
                var opponentRole = Role(encounter.Id, opponent);
                rolePairs.Add(new RolePair(encounter, player, opponent, playerRole, opponentRole));
            }
        }

        var qualified = new List<QualifiedRolePair>();
        foreach (var pair in rolePairs.OrderBy(item => item.Encounter.Id, StringComparer.Ordinal)
                     .ThenBy(item => item.Player.Id, StringComparer.Ordinal)
                     .ThenBy(item => item.Opponent.Id, StringComparer.Ordinal)
                     .ThenBy(item => item.PlayerRole.RoleFingerprint, StringComparer.Ordinal)
                     .ThenBy(item => item.OpponentRole.RoleFingerprint, StringComparer.Ordinal))
        {
            var qualification = Qualify(laneAPackage, runtime, pair);
            if (!qualification.PlayerRoutePassed)
            {
                diagnostics.Add("generated_combat.player_route_missing");
                continue;
            }
            if (!qualification.OpponentAiPassed)
            {
                diagnostics.Add("generated_combat.opponent_route_missing");
                continue;
            }
            if (!qualification.ExactPackageReferencePassed || !qualification.PackageShaUnchanged)
            {
                diagnostics.Add("generated_combat.definition_reference_invalid");
                continue;
            }
            qualified.Add(new QualifiedRolePair(pair, qualification));
        }

        var selected = qualified.FirstOrDefault();
        if (selected is null)
        {
            if (encounters.Count == 0 || rolePairs.Count == 0)
                diagnostics.Add("generated_combat.contract_missing");
            return new GeneratedEncounterCombatContractResult
            {
                CandidateEncounterCount = encounters.Count,
                CandidateRolePairCount = rolePairs.Count,
                Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList()
            };
        }

        var definitionFingerprints = DefinitionFingerprints(
            laneAPackage, selected.Pair.PlayerRole, selected.Pair.OpponentRole);
        var contract = new GeneratedEncounterCombatContract
        {
            SourcePackageSha256 = GeneratedEncounterCombatCanonical.Hash(laneAPackage),
            SourceEncounterId = selected.Pair.Encounter.Id,
            PlayerRoleFingerprint = selected.Pair.PlayerRole.RoleFingerprint,
            OpponentRoleFingerprint = selected.Pair.OpponentRole.RoleFingerprint,
            PlayerRole = selected.Pair.PlayerRole,
            OpponentRole = selected.Pair.OpponentRole,
            ExactDefinitionFingerprints = definitionFingerprints,
            QualificationSummary = selected.Qualification
        };
        contract = contract with
        {
            ContractId = GeneratedEncounterCombatCanonical.Hash(contract with { ContractId = string.Empty })
        };
        return new GeneratedEncounterCombatContractResult
        {
            Passed = true,
            Contract = contract,
            CandidateEncounterCount = encounters.Count,
            CandidateRolePairCount = rolePairs.Count,
            QualifiedRolePairCount = qualified.Count
        };
    }

    private static GeneratedEncounterCombatContractQualificationSummary Qualify(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        RolePair pair)
    {
        var packageBefore = GeneratedEncounterCombatCanonical.Hash(package);
        var diagnostics = new List<string>();
        var basic = TryPlayerRoute(package, runtime, pair, useAbility: false, out var basicSession);
        var ability = pair.Player.Abilities.Count > 0
                      && TryPlayerRoute(package, runtime, pair, useAbility: true, out _);
        var routeSession = basicSession;
        var playerRoute = basic;
        if (!playerRoute)
            playerRoute = TryPlayerRoute(package, runtime, pair, useAbility: true, out routeSession);
        var ai = playerRoute && routeSession is not null
            ? QualifyOpponentAi(package, runtime, routeSession, out var aiEffect, out var returned)
            : (Passed: false, Effect: false, Returned: false);
        var packageAfter = GeneratedEncounterCombatCanonical.Hash(package);
        if (!playerRoute) diagnostics.Add("generated_combat.player_route_missing");
        if (!ai.Passed) diagnostics.Add("generated_combat.opponent_route_missing");
        return new GeneratedEncounterCombatContractQualificationSummary
        {
            StartEncounterPassed = basicSession is not null || routeSession is not null,
            BasicAttackPassed = basic,
            PackageAbilityPassed = ability,
            PlayerRoutePassed = playerRoute,
            OpponentAiPassed = ai.Passed,
            OpponentEffectObserved = ai.Effect,
            ControlReturnedOrEncounterTerminated = ai.Returned,
            ExactPackageReferencePassed = true,
            PackageShaUnchanged = string.Equals(packageBefore, packageAfter, StringComparison.Ordinal),
            Diagnostics = diagnostics
        };
    }

    private static bool TryPlayerRoute(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        RolePair pair,
        bool useAbility,
        out UnifiedRuntimeSession? resultingSession)
    {
        resultingSession = null;
        var start = runtime.Start(package);
        if (!start.Success) return false;
        var encounter = runtime.ExecuteGameplayCommand(package, start.Session,
            GameRuntimeCommand.StartEncounter(pair.Encounter.Id));
        if (!encounter.Success) return false;
        var prepared = PrepareTurn(package, runtime, encounter.Session, pair.Player.Id);
        if (prepared is null) return false;

        var targets = prepared.GameplayState.ActiveEncounter?.Participants
            .Where(item => item.Alive && !IsPlayer(item.Team))
            .OrderBy(item => item.Id, StringComparer.Ordinal).ToList() ?? [];
        var abilities = useAbility
            ? pair.Player.Abilities.OrderBy(value => value, StringComparer.Ordinal).ToList()
            : [string.Empty];
        foreach (var ability in abilities)
        foreach (var target in targets)
        {
            var attempt = GeneratedEncounterCombatCanonical.Clone(prepared);
            var before = RuntimeHealth(attempt, package, playerTeam: false);
            var command = useAbility
                ? GameRuntimeCommand.UseAbility(ability, pair.Player.Id, target.Id)
                : GameRuntimeCommand.BasicAttack(pair.Player.Id, target.Id);
            var result = runtime.ExecuteGameplayCommand(package, attempt, command);
            if (!result.Success || !HealthChanged(before, RuntimeHealth(result.Session, package, playerTeam: false)))
                continue;
            resultingSession = result.Session;
            return true;
        }
        return false;
    }

    private static UnifiedRuntimeSession? PrepareTurn(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session,
        string participantId)
    {
        var current = session;
        var encounter = current.GameplayState.ActiveEncounter;
        var limit = Math.Max(1, (encounter?.Participants.Count ?? 1) * 4);
        for (var index = 0; index < limit; index++)
        {
            encounter = current.GameplayState.ActiveEncounter;
            if (encounter is not { Active: true } || encounter.Participants.Count == 0) return null;
            var participant = encounter.Participants[Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1)];
            if (string.Equals(participant.Id, participantId, StringComparison.OrdinalIgnoreCase)) return current;
            var result = IsPlayer(participant.Team)
                ? runtime.ExecuteGameplayCommand(package, current,
                    new GameRuntimeCommand { Type = GameRuntimeCommandType.EndTurn, TargetId = participant.Id })
                : runtime.ExecuteGameplayCommand(package, current,
                    new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            if (!result.Success) return null;
            current = result.Session;
        }
        return null;
    }

    private static (bool Passed, bool Effect, bool Returned) QualifyOpponentAi(
        GamePackageDefinition package,
        IUnifiedGameRuntimeService runtime,
        UnifiedRuntimeSession session,
        out bool effect,
        out bool returned)
    {
        effect = false;
        returned = false;
        var current = session;
        var before = PlayerCombatState(current);
        var aiSucceeded = false;
        var encounter = current.GameplayState.ActiveEncounter;
        var limit = Math.Max(1, (encounter?.Participants.Count ?? 1) * 4);
        for (var index = 0; index < limit; index++)
        {
            encounter = current.GameplayState.ActiveEncounter;
            if (encounter is not { Active: true })
            {
                returned = true;
                break;
            }
            if (encounter.Participants.Count == 0) break;
            var participant = encounter.Participants[Math.Clamp(encounter.TurnIndex, 0, encounter.Participants.Count - 1)];
            if (IsPlayer(participant.Team))
            {
                returned = true;
                break;
            }
            var ai = runtime.ExecuteGameplayCommand(package, current,
                new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
            if (!ai.Success) break;
            aiSucceeded = true;
            current = ai.Session;
            effect |= !string.Equals(before, PlayerCombatState(current), StringComparison.Ordinal);
        }
        encounter = current.GameplayState.ActiveEncounter;
        returned |= encounter is not { Active: true } || encounter.Participants.Count > 0
                    && IsPlayer(encounter.Participants[Math.Clamp(encounter.TurnIndex, 0,
                        encounter.Participants.Count - 1)].Team);
        var passed = aiSucceeded && effect && returned;
        return (passed, effect, returned);
    }

    private static GeneratedEncounterCombatRoleContract Role(
        string encounterId,
        EncounterParticipantDefinition participant)
    {
        var role = new GeneratedEncounterCombatRoleContract
        {
            SourceEncounterId = encounterId,
            SourceParticipantId = participant.Id,
            Resources = participant.Resources.OrderBy(item => item.Id, StringComparer.Ordinal)
                .Select(GeneratedEncounterCombatCanonical.Clone).ToList(),
            Stats = participant.Stats.OrderBy(item => item.Id, StringComparer.Ordinal)
                .Select(GeneratedEncounterCombatCanonical.Clone).ToList(),
            Abilities = participant.Abilities.OrderBy(value => value, StringComparer.Ordinal).ToList(),
            InventoryId = participant.InventoryId,
            CombatMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
        return role with
        {
            RoleFingerprint = GeneratedEncounterCombatCanonical.Hash(role with { RoleFingerprint = string.Empty })
        };
    }

    private static IReadOnlyList<GeneratedEncounterCombatDefinitionFingerprint> DefinitionFingerprints(
        GamePackageDefinition package,
        GeneratedEncounterCombatRoleContract player,
        GeneratedEncounterCombatRoleContract opponent)
    {
        var result = new List<GeneratedEncounterCombatDefinitionFingerprint>();
        foreach (var id in player.Resources.Concat(opponent.Resources).Select(item => item.Id)
                     .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal))
            Add("game.resources", id, package.Game.Resources.Single(item => item.Id == id));
        foreach (var id in player.Stats.Concat(opponent.Stats).Select(item => item.Id)
                     .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal))
            Add("game.stats", id, package.Game.Stats.Single(item => item.Id == id));
        foreach (var id in player.Abilities.Concat(opponent.Abilities)
                     .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal))
            Add("game.abilities", id, package.Game.Abilities.Single(item => item.Id == id));
        foreach (var id in new[] { player.InventoryId, opponent.InventoryId }
                     .Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!)
                     .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal))
            Add("game.inventories", id, package.Game.Inventories.Single(item => item.Id == id));
        return result.OrderBy(item => item.CollectionPath, StringComparer.Ordinal)
            .ThenBy(item => item.DefinitionId, StringComparer.Ordinal).ToList();

        void Add<T>(string collection, string id, T definition) => result.Add(
            new GeneratedEncounterCombatDefinitionFingerprint
            {
                CollectionPath = collection,
                DefinitionId = id,
                CanonicalSha256 = GeneratedEncounterCombatCanonical.Hash(definition)
            });
    }

    private static bool ReferencesResolveExactly(GamePackageDefinition package, EncounterDefinition encounter)
    {
        foreach (var participant in encounter.Participants)
        {
            if (participant.Resources.Any(item => package.Game.Resources.Count(definition => definition.Id == item.Id) != 1)
                || participant.Stats.Any(item => package.Game.Stats.Count(definition => definition.Id == item.Id) != 1)
                || participant.Abilities.Any(id => package.Game.Abilities.Count(definition => definition.Id == id) != 1)
                || participant.InventoryId is { Length: > 0 } inventoryId
                && package.Game.Inventories.Count(definition => definition.Id == inventoryId) != 1
                || participant.EntityPrototypeId is { Length: > 0 } prototypeId
                && package.Game.EntityPrototypes.Count(definition => definition.Id == prototypeId) != 1
                || participant.FactionId is { Length: > 0 } factionId
                && package.Game.Factions.Count(definition => definition.Id == factionId) != 1)
                return false;
        }
        return true;
    }

    private static bool HasRuntimeHealth(GamePackageDefinition package, EncounterParticipantDefinition participant) =>
        participant.Resources.Any(resource => package.Game.Resources.Count(definition => definition.Id == resource.Id) == 1
                                              && IsRuntimeHealth(package.Game.Resources.Single(definition => definition.Id == resource.Id)));

    internal static bool IsRuntimeHealth(ResourceDefinition definition) =>
        string.Equals(definition.Id, "resource/health", StringComparison.OrdinalIgnoreCase)
        || string.Equals(definition.Kind, "health", StringComparison.OrdinalIgnoreCase)
        || definition.Tags.Any(tag => string.Equals(tag, "health", StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyDictionary<string, double> RuntimeHealth(
        UnifiedRuntimeSession session,
        GamePackageDefinition package,
        bool playerTeam) => session.GameplayState.ActiveEncounter?.Participants
        .Where(item => IsPlayer(item.Team) == playerTeam)
        .SelectMany(participant => participant.Resources
            .Where(resource => package.Game.Resources.Any(definition => definition.Id == resource.ResourceId
                                                                    && IsRuntimeHealth(definition)))
            .Select(resource => new KeyValuePair<string, double>(participant.Id + "|" + resource.ResourceId,
                resource.Amount)))
        .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
        ?? new Dictionary<string, double>(StringComparer.Ordinal);

    private static bool HealthChanged(
        IReadOnlyDictionary<string, double> before,
        IReadOnlyDictionary<string, double> after) => before.Any(item => after.TryGetValue(item.Key, out var value)
                                                                         && value < item.Value);

    private static string PlayerCombatState(UnifiedRuntimeSession session)
    {
        var players = session.GameplayState.ActiveEncounter?.Participants
            .Where(item => IsPlayer(item.Team)).OrderBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => new
            {
                item.Id,
                Resources = item.Resources.OrderBy(value => value.ResourceId, StringComparer.Ordinal),
                Stats = item.Stats.OrderBy(value => value.StatId, StringComparer.Ordinal),
                Statuses = item.Statuses.OrderBy(value => value.StatusId, StringComparer.Ordinal)
            }).ToList() ?? [];
        return GeneratedEncounterCombatCanonical.Serialize(players);
    }

    private static bool IsPlayer(string? team) =>
        string.Equals(team, "player", StringComparison.OrdinalIgnoreCase);

    private sealed record RolePair(
        EncounterDefinition Encounter,
        EncounterParticipantDefinition Player,
        EncounterParticipantDefinition Opponent,
        GeneratedEncounterCombatRoleContract PlayerRole,
        GeneratedEncounterCombatRoleContract OpponentRole);

    private sealed record QualifiedRolePair(
        RolePair Pair,
        GeneratedEncounterCombatContractQualificationSummary Qualification);
}
