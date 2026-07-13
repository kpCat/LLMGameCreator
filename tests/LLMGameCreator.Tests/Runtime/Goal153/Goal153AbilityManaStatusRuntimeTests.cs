using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime.Goal153;

public sealed class Goal153AbilityManaStatusRuntimeTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateOptions();

    [Fact]
    public void Configured_ability_consumes_mana_applies_ticks_and_expires_with_provenance()
    {
        var package = Package();
        var service = Service();
        var state = Initial(package);
        Assert.True(service.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);

        var ability = service.UseAbility(package, state, "ability/arcane_impulse", "player", "goblin");

        Assert.True(ability.Success);
        Assert.Equal(10, Amount(state, "goblin", "resource/health"));
        Assert.Equal(9, Amount(state, "player", "resource/mana"));
        var status = Participant(state, "goblin").Statuses.Single(item => item.StatusId == "status/arcane_burn");
        Assert.Equal(2, status.RemainingTicks);
        Assert.Equal("player", status.Metadata["sourceParticipantId"]);
        Assert.Equal("ability/arcane_impulse", status.Metadata["sourceAbilityId"]);

        var firstTick = service.EndTurn(package, state);
        Assert.True(firstTick.Success);
        Assert.Equal(9, Amount(state, "goblin", "resource/health"));
        Assert.Equal(1, Participant(state, "goblin").Statuses.Single().RemainingTicks);
        Assert.Contains(firstTick.Events, item => item.Type == GameRuntimeEventType.StatusTicked);

        Assert.True(service.BasicAttack(package, state, "player", "goblin").Success);
        var secondTick = service.EndTurn(package, state);
        Assert.True(secondTick.Success);
        Assert.Equal(4, Amount(state, "goblin", "resource/health"));
        Assert.Empty(Participant(state, "goblin").Statuses);
        Assert.Contains(secondTick.Events, item => item.Type == GameRuntimeEventType.StatusRemoved && item.Message.Contains("expired"));
    }

    [Fact]
    public void Insufficient_mana_and_invalid_status_tick_are_transactional()
    {
        var package = Package();
        var service = Service();
        var state = Initial(package);
        Assert.True(service.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        Participant(state, "player").Resources.Single(item => item.ResourceId == "resource/mana").Amount = 2;
        var beforeAbility = Stable(state);

        var insufficient = service.UseAbility(package, state, "ability/arcane_impulse", "player", "goblin");

        Assert.False(insufficient.Success);
        Assert.Equal(beforeAbility, Stable(state));

        Participant(state, "player").Resources.Single(item => item.ResourceId == "resource/mana").Amount = 12;
        Assert.True(service.UseAbility(package, state, "ability/arcane_impulse", "player", "goblin").Success);
        package.Game.Statuses.Single(item => item.Id == "status/arcane_burn").Effects[0].Type = "unknown_goal153_effect";
        var beforeTick = Stable(state);

        var invalidTick = service.EndTurn(package, state);

        Assert.False(invalidTick.Success);
        Assert.Equal(beforeTick, Stable(state));
        Assert.Contains(invalidTick.Diagnostics, item => item.Code == "ability.effect.kind.unknown");
    }

    [Fact]
    public void Reapplication_refreshes_single_status_and_checkpoint_continuation_is_equivalent()
    {
        var package = Package();
        var service = Service();
        var state = Initial(package);
        Assert.True(service.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        Assert.True(service.UseAbility(package, state, "ability/arcane_impulse", "player", "goblin").Success);
        Assert.True(service.EndTurn(package, state).Success);
        Assert.True(service.UseAbility(package, state, "ability/arcane_impulse", "player", "goblin").Success);
        var statuses = Participant(state, "goblin").Statuses;
        Assert.Single(statuses);
        Assert.Equal(2, statuses[0].RemainingTicks);

        var checkpoint = JsonSerializer.Deserialize<GameRuntimeState>(Stable(state), JsonOptions)!;
        var uninterruptedEvents = Continue(service, package, state);
        var resumedEvents = Continue(service, package, checkpoint);

        Assert.Equal(uninterruptedEvents, resumedEvents);
        Assert.Equal(Stable(state), Stable(checkpoint));
    }

    [Fact]
    public void Unknown_ability_mana_status_and_tick_resource_references_are_transactional()
    {
        var service = Service();

        var unknownAbilityPackage = Package();
        var unknownAbilityState = Initial(unknownAbilityPackage);
        Assert.True(service.StartEncounter(unknownAbilityPackage, unknownAbilityState, "encounter/goblin_duel", 136).Success);
        var beforeUnknownAbility = Stable(unknownAbilityState);
        Assert.False(service.UseAbility(unknownAbilityPackage, unknownAbilityState, "ability/missing", "player", "goblin").Success);
        Assert.Equal(beforeUnknownAbility, Stable(unknownAbilityState));

        var missingManaPackage = Package();
        var missingManaState = Initial(missingManaPackage);
        Assert.True(service.StartEncounter(missingManaPackage, missingManaState, "encounter/goblin_duel", 136).Success);
        Participant(missingManaState, "player").Resources.RemoveAll(item => item.ResourceId == "resource/mana");
        var beforeMissingMana = Stable(missingManaState);
        Assert.False(service.UseAbility(missingManaPackage, missingManaState, "ability/arcane_impulse", "player", "goblin").Success);
        Assert.Equal(beforeMissingMana, Stable(missingManaState));

        var missingStatusPackage = Package();
        var missingStatusState = Initial(missingStatusPackage);
        Assert.True(service.StartEncounter(missingStatusPackage, missingStatusState, "encounter/goblin_duel", 136).Success);
        missingStatusPackage.Game.Statuses.Clear();
        var beforeMissingStatus = Stable(missingStatusState);
        Assert.False(service.UseAbility(missingStatusPackage, missingStatusState, "ability/arcane_impulse", "player", "goblin").Success);
        Assert.Equal(beforeMissingStatus, Stable(missingStatusState));

        var missingTickResourcePackage = Package();
        var missingTickResourceState = Initial(missingTickResourcePackage);
        Assert.True(service.StartEncounter(missingTickResourcePackage, missingTickResourceState, "encounter/goblin_duel", 136).Success);
        Assert.True(service.UseAbility(missingTickResourcePackage, missingTickResourceState, "ability/arcane_impulse", "player", "goblin").Success);
        missingTickResourcePackage.Game.Statuses.Single(item => item.Id == "status/arcane_burn").Effects[0].Args["id"] = "resource/missing";
        var beforeMissingTickResource = Stable(missingTickResourceState);
        Assert.False(service.EndTurn(missingTickResourcePackage, missingTickResourceState).Success);
        Assert.Equal(beforeMissingTickResource, Stable(missingTickResourceState));
    }

    private static IReadOnlyList<string> Continue(EncounterRuntimeService service, GamePackageDefinition package, GameRuntimeState state)
    {
        var events = new List<string>();
        foreach (var result in new[]
                 {
                     service.EndTurn(package, state),
                     service.BasicAttack(package, state, "player", "goblin"),
                     service.EndTurn(package, state)
                 })
        {
            Assert.True(result.Success);
            events.AddRange(result.Events.Select(item => item.Type + ":" + item.Message + ":" + item.TargetId));
        }
        return events;
    }

    private static EncounterRuntimeService Service() => new(new RequirementEvaluator(), new OutputApplier());
    private static GameRuntimeState Initial(GamePackageDefinition package) => new GameRuntimeStateFactory().CreateInitialState(package).State;
    private static EncounterParticipantState Participant(GameRuntimeState state, string id) => state.ActiveEncounter!.Participants.Single(item => item.Id == id);
    private static double Amount(GameRuntimeState state, string participantId, string resourceId) => Participant(state, participantId).Resources.Single(item => item.ResourceId == resourceId).Amount;
    private static string Stable(GameRuntimeState state) => JsonSerializer.Serialize(state, JsonOptions);

    private static GamePackageDefinition Package()
    {
        var root = FindRoot();
        var package = JsonSerializer.Deserialize<GamePackageDefinition>(File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json")), JsonOptions)!;
        package.Game.Abilities.Add(new AbilityDefinition
        {
            Id = "ability/arcane_impulse", Name = "Магический импульс", Kind = "attack", Power = 2,
            ResourceId = "resource/health", Targeting = "hostile_participant", Tags = ["active", "magic"],
            Costs = [new CostDefinition { Kind = "resource", Id = "resource/mana", Amount = 3 }],
            Effects =
            [
                new EffectDefinition { Type = "damage_resource", Args = new() { ["id"] = "resource/health", ["amount"] = "2" } },
                new EffectDefinition { Type = "add_status", Args = new() { ["id"] = "status/arcane_burn", ["amount"] = "2" } }
            ]
        });
        package.Game.Statuses.Add(new StatusDefinition
        {
            Id = "status/arcane_burn", Name = "Магическое горение", Kind = "debuff", DurationMode = "turns",
            Effects = [new EffectDefinition { Type = "damage_resource", Args = new() { ["id"] = "resource/health", ["amount"] = "1" } }]
        });
        var player = package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel").Participants.Single(item => item.Id == "player");
        player.Abilities.Add("ability/arcane_impulse");
        player.Resources.Add(new OutputDefinition { Kind = "resource", Id = "resource/mana", Amount = 12 });
        return package;
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
