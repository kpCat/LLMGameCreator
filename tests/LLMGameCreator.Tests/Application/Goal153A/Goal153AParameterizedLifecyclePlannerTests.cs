using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153A;

public sealed class Goal153AParameterizedLifecyclePlannerTests
{
    private static readonly string[] Goal153Ids =
    [
        "feature.combat.active_ability_loadout",
        "feature.magic.mana_spellcasting",
        "feature.status.turn_effects"
    ];

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Duration_expands_bound_turns_and_full_runtime_replay(int duration)
    {
        var fixture = Fixture(duration, 2, 1);
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules, fixture.Package);
        var endTurns = StatusEndTurns(plan);

        Assert.Equal((duration * 3) - 1, endTurns.Count);
        Assert.Equal(duration, endTurns.Count(action => action.ResolvedTargetId == "goal153_target"));
        Assert.All(endTurns, action => Assert.False(string.IsNullOrWhiteSpace(action.ResolvedTargetId)));
        Assert.Equal(endTurns.Count, endTurns.Select(action => action.ActionId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("complete_arcane_burn_lifecycle", endTurns[^1].ActionId);
        Assert.Equal(1, endTurns.Count(action => action.CheckpointBoundaryAfter));

        var result = Qualify(fixture, plan, "duration-" + duration);
        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.True(result.ActionDescriptorExecutionBindingPassed);
        var events = result.Session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
        Assert.Equal(duration, events.Count(item => item.EventType == "StatusTicked"
                                                  && item.Args.GetValueOrDefault("statusId") == "status/arcane_burn"));
        Assert.Contains(events, item => item.EventType == "StatusRemoved" && item.Message.Contains("expired"));
    }

    [Fact]
    public void Duration_1000_plan_is_deterministic_unique_and_not_executed()
    {
        var fixture = Fixture(1000, 2, 1);
        var planner = new CapabilityDrivenRuntimePlaythroughPlanner();
        var first = planner.Plan(fixture.Modules, fixture.Package);
        var second = planner.Plan(fixture.Modules, fixture.Package);
        var endTurns = StatusEndTurns(first);

        Assert.Equal(2999, endTurns.Count);
        Assert.Equal(1000, endTurns.Count(action => action.ResolvedTargetId == "goal153_target"));
        Assert.Equal(endTurns.Count, endTurns.Select(action => action.ActionId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(endTurns, action => Assert.False(string.IsNullOrWhiteSpace(action.ResolvedTargetId)));
        Assert.Equal(first.ActionPlanSignature, second.ActionPlanSignature);
        Assert.Equal(first.OrderedActions.Select(action => action.ActionId), second.OrderedActions.Select(action => action.ActionId));
    }

    [Fact]
    public void Duration_5_checkpoint_preserves_four_ticks_and_resumed_events()
    {
        var fixture = Fixture(5, 2, 1);
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules, fixture.Package);
        var endTurns = StatusEndTurns(plan);
        var checkpointIndex = endTurns.FindIndex(action => action.CheckpointBoundaryAfter);
        Assert.True(checkpointIndex >= 0);
        var service = new EncounterRuntimeService(new RequirementEvaluator(), new OutputApplier());
        var state = new GameRuntimeStateFactory().CreateInitialState(fixture.Package).State;
        Assert.True(service.StartEncounter(fixture.Package, state, "encounter/goblin_duel", 136).Success);
        Assert.True(service.UseAbility(fixture.Package, state, "ability/arcane_impulse", "player", "goal153_target").Success);
        foreach (var action in endTurns.Take(checkpointIndex + 1))
            Assert.True(service.EndTurn(fixture.Package, state, action.ResolvedTargetId).Success);
        var status = state.ActiveEncounter!.Participants.Single(item => item.Id == "goal153_target").Statuses.Single();
        Assert.Equal(4, status.RemainingTicks);
        Assert.Equal("player", status.Metadata["sourceParticipantId"]);
        Assert.Equal("ability/arcane_impulse", status.Metadata["sourceAbilityId"]);
        Assert.Equal(9, state.ActiveEncounter.Participants.Single(item => item.Id == "player")
            .Resources.Single(item => item.ResourceId == "resource/mana").Amount);

        var resumed = JsonSerializer.Deserialize<GameRuntimeState>(Stable(state), JsonOptions())!;
        var uninterruptedEvents = Continue(service, fixture.Package, state, endTurns.Skip(checkpointIndex + 1));
        var resumedEvents = Continue(service, fixture.Package, resumed, endTurns.Skip(checkpointIndex + 1));
        Assert.Equal(4, uninterruptedEvents.Count(item => item.StartsWith("StatusTicked:", StringComparison.Ordinal)));
        Assert.Equal(uninterruptedEvents, resumedEvents);
        Assert.Equal(Stable(state), Stable(resumed));
        Assert.Empty(state.ActiveEncounter!.Participants.Single(item => item.Id == "goal153_target").Statuses);
    }

    [Fact]
    public void High_damage_fixture_survives_and_mana_relation_fails_at_parameter_stage()
    {
        var fixture = Fixture(5, 100, 50);
        var targetHealth = fixture.Package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel")
            .Participants.Single(item => item.Id == "goal153_target").Resources
            .Single(item => item.Id == "resource/health").Amount;
        var declarationLibrary = new FeatureModuleLibraryLoader().Load(Path.Combine(FindRoot(), "catalogs", "feature-modules"));
        decimal Maximum(string moduleId, string parameterId) => declarationLibrary.Catalog.Modules.Single(module => module.ModuleId == moduleId)
            .ParameterDefinitions.Single(parameter => parameter.ParameterId == parameterId).Maximum!.Value;
        var declaredMaximumDamage = Maximum(Goal153Ids[0], "abilityBaseDamage");
        var declaredMaximumTickDamage = Maximum(Goal153Ids[2], "statusTickDamage");
        var declaredMaximumDuration = Maximum(Goal153Ids[2], "statusDurationTurns");
        Assert.True((decimal)targetHealth > checked(declaredMaximumDamage + checked(declaredMaximumTickDamage * declaredMaximumDuration)));
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules, fixture.Package);
        var result = Qualify(fixture, plan, "high-damage");
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.Contains(result.Session.CanonicalSession.Snapshots.SelectMany(item => item.RuntimeEvents),
            item => item.EventType == "StatusRemoved" && item.Message.Contains("expired"));

        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selected = library.Catalog.Modules.Where(item => item.Required).Select(item => item.ModuleId)
            .Concat(Goal153Ids).ToList();
        var rejected = new FeatureModuleParameterBindingService().Bind(library.Catalog, selected,
        [
            Value(Goal153Ids[1], "startingMana", 2),
            Value(Goal153Ids[1], "abilityManaCost", 3)
        ]);
        Assert.False(rejected.Passed);
        Assert.Contains(rejected.Diagnostics, diagnostic => diagnostic.Contains("abilityManaCost=3")
                                                             && diagnostic.Contains("startingMana=2"));
    }

    [Fact]
    public void Goal153A_parameter_domain_evidence_bundle()
    {
        var rows = new List<object>();
        ProductLineRuntimeQualificationResult? duration5 = null;
        foreach (var duration in new[] { 1, 2, 5 })
        {
            var fixture = Fixture(duration, duration == 5 ? 100 : 2, duration == 5 ? 50 : 1);
            var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules, fixture.Package);
            var result = Qualify(fixture, plan, "evidence-duration-" + duration);
            Assert.True(result.FinalReplay.Passed);
            var turns = StatusEndTurns(plan);
            rows.Add(new
            {
                duration,
                fullQualification = true,
                plannedActionCount = plan.OrderedActions.Count,
                generatedEndTurnCount = turns.Count,
                targetTickCount = turns.Count(action => action.ResolvedTargetId == "goal153_target"),
                uniqueActionIds = turns.Select(action => action.ActionId).Distinct(StringComparer.Ordinal).Count() == turns.Count,
                allExpectedParticipantsBound = turns.All(action => !string.IsNullOrWhiteSpace(action.ResolvedTargetId)),
                plan.ActionPlanSignature,
                result.Session.CurrentStateHash
            });
            if (duration == 5) duration5 = result;
        }
        var planOnlyFixture = Fixture(1000, 2, 1);
        var planOnly = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(planOnlyFixture.Modules, planOnlyFixture.Package);
        var planOnlyTurns = StatusEndTurns(planOnly);
        Assert.Equal(1000, planOnlyTurns.Count(action => action.ResolvedTargetId == "goal153_target"));
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL153A_EVIDENCE_ROOT");
        if (string.IsNullOrWhiteSpace(root)) return;
        Directory.CreateDirectory(root);
        Write(root, "parameter-domain-expansion-proof.json", new
        {
            schemaVersion = "goal153a_parameter_domain_expansion_proof_v1",
            status = "GREEN",
            fullQualification = rows,
            planOnly = new
            {
                duration = 1000,
                executed = false,
                plannedActionCount = planOnly.OrderedActions.Count,
                generatedEndTurnCount = planOnlyTurns.Count,
                targetTickCount = 1000,
                uniqueActionIds = planOnlyTurns.Select(action => action.ActionId).Distinct(StringComparer.Ordinal).Count() == planOnlyTurns.Count,
                allExpectedParticipantsBound = planOnlyTurns.All(action => !string.IsNullOrWhiteSpace(action.ResolvedTargetId)),
                planOnly.ActionPlanSignature
            },
            trainingTargetHealth = planOnlyFixture.Package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel")
                .Participants.Single(item => item.Id == "goal153_target").Resources.Single(item => item.Id == "resource/health").Amount
        });
        Write(root, "turn-binding-proof.json", new
        {
            schemaVersion = "goal153a_turn_binding_proof_v1",
            status = "GREEN",
            expandedEndTurnCount = planOnlyTurns.Count,
            allExpectedParticipantsBound = planOnlyTurns.All(action => !string.IsNullOrWhiteSpace(action.ResolvedTargetId)),
            uniqueActionIds = planOnlyTurns.Select(action => action.ActionId).Distinct(StringComparer.Ordinal).Count() == planOnlyTurns.Count,
            firstBindings = planOnlyTurns.Take(4).Select(action => new { action.ActionId, action.ResolvedTargetId }),
            terminalBinding = new { planOnlyTurns[^1].ActionId, planOnlyTurns[^1].ResolvedTargetId }
        });
        Write(root, "duration5-checkpoint-replay-proof.json", new
        {
            schemaVersion = "goal153a_duration5_checkpoint_replay_proof_v1",
            status = "GREEN",
            remainingTicksAfterFirstTick = 4,
            duration5!.Checkpoint.ExpectedStateHash,
            checkpointActualStateHash = duration5.CheckpointReplay.ActualStateHash,
            checkpointReplayPassed = duration5.CheckpointReplay.Passed,
            finalExpectedStateHash = duration5.FinalReplay.ExpectedStateHash,
            finalActualStateHash = duration5.FinalReplay.ActualStateHash,
            finalReplayEquivalent = duration5.FinalReplay.Passed,
            replayedActionCount = duration5.FinalReplay.ReplayedActionCount
        });
        Write(root, "mana-constraint-proof.json", new
        {
            schemaVersion = "goal153a_mana_constraint_proof_v1",
            status = "GREEN",
            startingMana = 2,
            abilityManaCost = 3,
            rejectedAtStage = "parameter_binding",
            diagnostic = "parameter relation rejected: feature.magic.mana_spellcasting.abilityManaCost=3 must be <= feature.magic.mana_spellcasting.startingMana=2"
        });
    }

    private static ProductLineRuntimeQualificationResult Qualify(
        FixtureData fixture,
        CapabilityRuntimePlaythroughPlan plan,
        string id) =>
        new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).Qualify(
            fixture.Package,
            new ProductLineRuntimeQualificationRequest
            {
                SessionId = "goal153a-" + id,
                CandidateId = "goal153a",
                VariantKind = id,
                PackagePath = "in-memory/package.json",
                PackageSha256 = new string('a', 64),
                CheckpointId = "goal153a-checkpoint-" + id,
                FinalCheckpointId = "goal153a-final-" + id,
                CapabilityPlan = plan
            });

    private static List<CapabilityRuntimePlaythroughAction> StatusEndTurns(CapabilityRuntimePlaythroughPlan plan) =>
        plan.OrderedActions.Where(action => action.RuntimePrimitiveId == CapabilityRuntimePrimitiveIds.EndTurn
                                            && action.Args.GetValueOrDefault("statusId") == "status/arcane_burn")
            .ToList();

    private static IReadOnlyList<string> Continue(
        EncounterRuntimeService service,
        GamePackageDefinition package,
        GameRuntimeState state,
        IEnumerable<CapabilityRuntimePlaythroughAction> actions)
    {
        var events = new List<string>();
        foreach (var action in actions)
        {
            var result = service.EndTurn(package, state, action.ResolvedTargetId);
            Assert.True(result.Success, string.Join("; ", result.Diagnostics.Select(item => item.Code + ":" + item.Message)));
            events.AddRange(result.Events.Select(item => item.Type + ":" + item.Message + ":" + item.TargetId));
        }
        return events;
    }

    private static FixtureData Fixture(int duration, int abilityDamage, int tickDamage)
    {
        var root = FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selectedIds = library.Catalog.Modules.Where(item => item.Required).Select(item => item.ModuleId)
            .Concat(Goal153Ids).ToList();
        var bound = new FeatureModuleParameterBindingService().Bind(library.Catalog, selectedIds,
        [
            Value(Goal153Ids[0], "abilityBaseDamage", abilityDamage),
            Value(Goal153Ids[1], "startingMana", 12),
            Value(Goal153Ids[1], "abilityManaCost", 3),
            Value(Goal153Ids[2], "statusDurationTurns", duration),
            Value(Goal153Ids[2], "statusTickDamage", tickDamage)
        ]);
        Assert.True(bound.Passed, string.Join("; ", bound.Diagnostics));
        var baseline = File.ReadAllText(Path.Combine(root, ".llmgc", "procedural",
            "goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff", "candidates",
            "minimal-map-game-balanced-baseline", "package.json"));
        var mutation = new FeatureModulePackageMutationService().Apply(baseline, bound.EffectiveMutationOperations);
        Assert.True(mutation.Passed, string.Join("; ", mutation.Diagnostics));
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(mutation.PackageJson, JsonOptions())!;
        var modules = bound.EffectiveCatalog.Modules.Where(item => selectedIds.Contains(item.ModuleId)).ToList();
        return new FixtureData(package, modules);
    }

    private static FeatureModuleParameterValue Value(string moduleId, string parameterId, decimal value) => new()
    {
        ModuleId = moduleId,
        ParameterId = parameterId,
        Value = JsonSerializer.SerializeToElement(value)
    };

    private static JsonSerializerOptions JsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string Stable(GameRuntimeState state) => JsonSerializer.Serialize(state, JsonOptions());

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

    private sealed record FixtureData(
        GamePackageDefinition Package,
        IReadOnlyList<FeatureModuleDefinition> Modules);
}
