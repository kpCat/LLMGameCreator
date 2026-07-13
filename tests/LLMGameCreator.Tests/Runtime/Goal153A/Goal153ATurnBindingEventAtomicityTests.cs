using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime.Goal153A;

public sealed class Goal153ATurnBindingEventAtomicityTests
{
    private static readonly HashSet<GameRuntimeEventType> MutationSuccessEvents =
    [
        GameRuntimeEventType.CostConsumed,
        GameRuntimeEventType.DamageApplied,
        GameRuntimeEventType.HealingApplied,
        GameRuntimeEventType.StatusAdded,
        GameRuntimeEventType.StatusRemoved,
        GameRuntimeEventType.AbilityUsed,
        GameRuntimeEventType.StatusTicked,
        GameRuntimeEventType.ParticipantDefeated,
        GameRuntimeEventType.EncounterEnded
    ];

    [Fact]
    public void EndTurn_expected_participant_succeeds_and_mismatch_is_atomic()
    {
        var package = Package();
        var service = Service();
        var state = Initial(package);
        Assert.True(service.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        var before = Stable(state);

        var mismatch = service.EndTurn(package, state, "goblin");

        Assert.False(mismatch.Success);
        Assert.Equal(before, Stable(state));
        Assert.Contains(mismatch.Diagnostics, item => item.Code == "encounter.turn.expected_participant_mismatch");
        Assert.DoesNotContain(mismatch.Events, item => MutationSuccessEvents.Contains(item.Type));

        var success = service.EndTurn(package, state, "player");
        Assert.True(success.Success);
        Assert.Equal("goblin", Current(state).Id);
        Assert.Contains(success.Events, item => item.Type == GameRuntimeEventType.TurnStarted && item.TargetId == "goblin");
    }

    [Fact]
    public void Late_ability_failure_discards_cost_damage_and_all_success_events()
    {
        var package = Package();
        package.Game.Abilities.Single(item => item.Id == "ability/arcane_impulse").Effects.Add(
            new EffectDefinition { Type = "unknown_goal153a_effect", Args = new() { ["id"] = "invalid" } });
        var service = Service();
        var state = Initial(package);
        Assert.True(service.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        var before = Stable(state);

        var failed = service.UseAbility(package, state, "ability/arcane_impulse", "player", "goblin");

        Assert.False(failed.Success);
        Assert.Equal(before, Stable(state));
        Assert.Contains(failed.Diagnostics, item => item.Code == "ability.effect.kind.unknown");
        Assert.DoesNotContain(failed.Events, item => MutationSuccessEvents.Contains(item.Type));
    }

    [Fact]
    public void Second_status_failure_discards_first_status_events_and_state()
    {
        var package = Package();
        package.Game.Statuses.Add(new StatusDefinition
        {
            Id = "status/invalid_second",
            Effects = [new EffectDefinition { Type = "unknown_goal153a_tick", Args = new() { ["id"] = "invalid" } }]
        });
        var service = Service();
        var state = Initial(package);
        Assert.True(service.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        var player = Current(state);
        player.Statuses.Add(new StatusState { StatusId = "status/arcane_burn", TargetId = "player", RemainingTicks = 2 });
        player.Statuses.Add(new StatusState { StatusId = "status/invalid_second", TargetId = "player", RemainingTicks = 2 });
        var before = Stable(state);

        var failed = service.EndTurn(package, state, "player");

        Assert.False(failed.Success);
        Assert.Equal(before, Stable(state));
        Assert.Contains(failed.Diagnostics, item => item.Code == "ability.effect.kind.unknown");
        Assert.DoesNotContain(failed.Events, item => MutationSuccessEvents.Contains(item.Type));
    }

    [Fact]
    public void Lethal_enemy_and_player_ticks_resolve_encounter_without_advancing()
    {
        var enemyPackage = Package(statusDamage: 50, statusOnlyAbility: true);
        var service = Service();
        var enemyState = Initial(enemyPackage);
        Assert.True(service.StartEncounter(enemyPackage, enemyState, "encounter/goblin_duel", 136).Success);
        Assert.True(service.UseAbility(enemyPackage, enemyState, "ability/arcane_impulse", "player", "goblin").Success);

        var enemyTick = service.EndTurn(enemyPackage, enemyState, "goblin");

        Assert.True(enemyTick.Success);
        Assert.False(enemyState.ActiveEncounter!.Active);
        Assert.False(enemyState.ActiveEncounter.Participants.Single(item => item.Id == "goblin").Alive);
        Assert.Equal(1, enemyTick.Events.Count(item => item.Type == GameRuntimeEventType.ParticipantDefeated));
        Assert.Equal(1, enemyTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterWon));
        Assert.Equal(1, enemyTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterEnded));
        Assert.DoesNotContain(enemyTick.Events, item => item.Type == GameRuntimeEventType.TurnStarted);

        var playerPackage = Package(statusDamage: 50);
        var playerState = Initial(playerPackage);
        Assert.True(service.StartEncounter(playerPackage, playerState, "encounter/goblin_duel", 136).Success);
        Current(playerState).Statuses.Add(new StatusState
        {
            StatusId = "status/arcane_burn",
            TargetId = "player",
            RemainingTicks = 1
        });

        var playerTick = service.EndTurn(playerPackage, playerState, "player");

        Assert.True(playerTick.Success);
        Assert.False(playerState.ActiveEncounter!.Active);
        Assert.False(playerState.ActiveEncounter.Participants.Single(item => item.Id == "player").Alive);
        Assert.Equal(1, playerTick.Events.Count(item => item.Type == GameRuntimeEventType.ParticipantDefeated));
        Assert.Equal(1, playerTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterLost));
        Assert.Equal(1, playerTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterEnded));
        Assert.DoesNotContain(playerTick.Events, item => item.Type == GameRuntimeEventType.TurnStarted);
    }

    [Fact]
    public void Canonical_failed_snapshot_contains_no_uncommitted_success_events()
    {
        var package = Package();
        package.Game.Abilities.Single(item => item.Id == "ability/arcane_impulse").Effects.Add(
            new EffectDefinition { Type = "unknown_goal153a_effect", Args = new() { ["id"] = "invalid" } });
        var plan = new CapabilityRuntimePlaythroughPlan
        {
            OrderedActions =
            [
                Action("start", CapabilityRuntimePrimitiveIds.Start, package.Manifest.PackageId),
                Action("encounter", CapabilityRuntimePrimitiveIds.StartEncounter, "encounter/goblin_duel"),
                Action("failed_ability", CapabilityRuntimePrimitiveIds.UseAbility, "ability/arcane_impulse",
                    new Dictionary<string, string>
                    {
                        ["sourceParticipantId"] = "player",
                        ["targetParticipantId"] = "goblin"
                    })
            ]
        };
        var loop = CanonicalRuntimePlayerCommandLoopService.CreateDefault();
        var session = loop.BeginSession(package, new CanonicalRuntimePlayerCommandLoopRequest
        {
            CandidateId = "goal153a-atomicity",
            PackagePath = "in-memory/package.json",
            CapabilityPlan = plan
        });

        var execution = loop.ExecuteRange(package, session, new CanonicalRuntimePlayerCommandLoopExecutionRequest
        {
            RequestedOperation = "failed_ability",
            RuntimeCommandStartIndex = 0,
            RuntimeCommandEndIndex = 3
        });

        Assert.False(execution.Success);
        var failed = execution.Snapshots[^1];
        Assert.Equal(failed.StateHashBefore, failed.StateHashAfter);
        Assert.DoesNotContain(failed.RuntimeEvents, item => MutationSuccessEvents.Any(type => item.EventType == type.ToString()));
    }

    [Fact]
    public void Goal153A_runtime_atomicity_and_lethal_evidence_bundle()
    {
        var service = Service();
        var abilityPackage = Package();
        abilityPackage.Game.Abilities.Single(item => item.Id == "ability/arcane_impulse").Effects.Add(
            new EffectDefinition { Type = "unknown_goal153a_effect", Args = new() { ["id"] = "invalid" } });
        var abilityState = Initial(abilityPackage);
        Assert.True(service.StartEncounter(abilityPackage, abilityState, "encounter/goblin_duel", 136).Success);
        var abilityBefore = Stable(abilityState);
        var abilityFailure = service.UseAbility(abilityPackage, abilityState, "ability/arcane_impulse", "player", "goblin");
        Assert.False(abilityFailure.Success);

        var statusPackage = Package();
        statusPackage.Game.Statuses.Add(new StatusDefinition
        {
            Id = "status/invalid_second",
            Effects = [new EffectDefinition { Type = "unknown_goal153a_tick", Args = new() { ["id"] = "invalid" } }]
        });
        var statusState = Initial(statusPackage);
        Assert.True(service.StartEncounter(statusPackage, statusState, "encounter/goblin_duel", 136).Success);
        Current(statusState).Statuses.Add(new StatusState { StatusId = "status/arcane_burn", TargetId = "player", RemainingTicks = 2 });
        Current(statusState).Statuses.Add(new StatusState { StatusId = "status/invalid_second", TargetId = "player", RemainingTicks = 2 });
        var statusBefore = Stable(statusState);
        var statusFailure = service.EndTurn(statusPackage, statusState, "player");
        Assert.False(statusFailure.Success);

        var enemyPackage = Package(statusDamage: 50, statusOnlyAbility: true);
        var enemyState = Initial(enemyPackage);
        Assert.True(service.StartEncounter(enemyPackage, enemyState, "encounter/goblin_duel", 136).Success);
        Assert.True(service.UseAbility(enemyPackage, enemyState, "ability/arcane_impulse", "player", "goblin").Success);
        var enemyTick = service.EndTurn(enemyPackage, enemyState, "goblin");
        Assert.True(enemyTick.Success);

        var playerPackage = Package(statusDamage: 50);
        var playerState = Initial(playerPackage);
        Assert.True(service.StartEncounter(playerPackage, playerState, "encounter/goblin_duel", 136).Success);
        Current(playerState).Statuses.Add(new StatusState { StatusId = "status/arcane_burn", TargetId = "player", RemainingTicks = 1 });
        var playerTick = service.EndTurn(playerPackage, playerState, "player");
        Assert.True(playerTick.Success);

        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL153A_EVIDENCE_ROOT");
        if (string.IsNullOrWhiteSpace(root)) return;
        Directory.CreateDirectory(root);
        Write(root, "event-atomicity-proof.json", new
        {
            schemaVersion = "goal153a_event_atomicity_proof_v1",
            status = "GREEN",
            lateAbilityFailure = new
            {
                stateByteIdentical = abilityBefore == Stable(abilityState),
                successEventCount = abilityFailure.Events.Count(item => MutationSuccessEvents.Contains(item.Type)),
                diagnostics = abilityFailure.Diagnostics.Select(item => item.Code)
            },
            secondStatusFailure = new
            {
                stateByteIdentical = statusBefore == Stable(statusState),
                successEventCount = statusFailure.Events.Count(item => MutationSuccessEvents.Contains(item.Type)),
                diagnostics = statusFailure.Diagnostics.Select(item => item.Code)
            },
            canonicalFailedSnapshotRegression = "Goal153ATurnBindingEventAtomicityTests.Canonical_failed_snapshot_contains_no_uncommitted_success_events"
        });
        Write(root, "lethal-status-resolution-proof.json", new
        {
            schemaVersion = "goal153a_lethal_status_resolution_proof_v1",
            status = "GREEN",
            enemy = new
            {
                defeated = enemyTick.Events.Count(item => item.Type == GameRuntimeEventType.ParticipantDefeated),
                won = enemyTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterWon),
                ended = enemyTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterEnded),
                turnAdvanced = enemyTick.Events.Any(item => item.Type == GameRuntimeEventType.TurnStarted)
            },
            player = new
            {
                defeated = playerTick.Events.Count(item => item.Type == GameRuntimeEventType.ParticipantDefeated),
                lost = playerTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterLost),
                ended = playerTick.Events.Count(item => item.Type == GameRuntimeEventType.EncounterEnded),
                turnAdvanced = playerTick.Events.Any(item => item.Type == GameRuntimeEventType.TurnStarted)
            }
        });
    }

    private static CapabilityRuntimePlaythroughAction Action(
        string actionId,
        string primitiveId,
        string targetId,
        IReadOnlyDictionary<string, string>? args = null) => new()
    {
        ActionId = actionId,
        ContractId = "goal153a." + actionId,
        CapabilityId = "goal153a.atomicity",
        Category = actionId,
        RuntimePrimitiveId = primitiveId,
        ResolvedTargetId = targetId,
        Args = args ?? new Dictionary<string, string>(),
        Required = true
    };

    private static EncounterRuntimeService Service() => new(new RequirementEvaluator(), new OutputApplier());
    private static GameRuntimeState Initial(GamePackageDefinition package) => new GameRuntimeStateFactory().CreateInitialState(package).State;
    private static EncounterParticipantState Current(GameRuntimeState state) => state.ActiveEncounter!.Participants[state.ActiveEncounter.TurnIndex];
    private static string Stable(GameRuntimeState state) => JsonSerializer.Serialize(state, JsonOptions());

    private static GamePackageDefinition Package(int statusDamage = 1, bool statusOnlyAbility = false)
    {
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(
            File.ReadAllText(Path.Combine(FindRoot(), "samples", "minimal-map-game", "package.json")), JsonOptions())!;
        package.Game.Abilities.Add(new AbilityDefinition
        {
            Id = "ability/arcane_impulse",
            Name = "Магический импульс",
            Kind = "attack",
            ResourceId = "resource/health",
            Costs = [new CostDefinition { Kind = "resource", Id = "resource/mana", Amount = 3 }],
            Effects =
            [
                .. statusOnlyAbility
                    ? []
                    : new[] { new EffectDefinition { Type = "damage_resource", Args = new() { ["id"] = "resource/health", ["amount"] = "2" } } },
                new EffectDefinition { Type = "add_status", Args = new() { ["id"] = "status/arcane_burn", ["amount"] = "2" } }
            ]
        });
        package.Game.Statuses.Add(new StatusDefinition
        {
            Id = "status/arcane_burn",
            Name = "Магическое горение",
            Kind = "debuff",
            DurationMode = "turns",
            Effects = [new EffectDefinition { Type = "damage_resource", Args = new() { ["id"] = "resource/health", ["amount"] = statusDamage.ToString() } }]
        });
        var player = package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel").Participants
            .Single(item => item.Id == "player");
        player.Abilities.Add("ability/arcane_impulse");
        player.Resources.Add(new OutputDefinition { Kind = "resource", Id = "resource/mana", Amount = 12 });
        return package;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static void Write(string root, string fileName, object value) =>
        File.WriteAllText(Path.Combine(root, fileName),
            JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);

    private static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln"))) return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
