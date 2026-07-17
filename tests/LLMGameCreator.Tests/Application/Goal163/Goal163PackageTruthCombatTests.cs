using System.Text.Json;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal162;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal163;

[Collection(Goal160Collection.Name)]
public sealed class Goal163PackageTruthCombatTests
{
    [Fact]
    public void Behavioral_basic_attack_dispatch_uses_exact_package_reference()
    {
        var fixture = Goal163TestKit.Dispatch(GameRuntimeCommand.BasicAttack("participant/player", "participant/enemy"));

        Assert.True(fixture.Result.Passed, string.Join(",", fixture.Result.Diagnostics));
        Assert.All(fixture.Runtime.Packages, value => Assert.Same(fixture.Package, value));
        Assert.True(fixture.Result.PackageReferencePreserved);
    }

    [Fact]
    public void Behavioral_basic_attack_command_type_is_not_rewritten()
    {
        var fixture = Goal163TestKit.Dispatch(GameRuntimeCommand.BasicAttack("participant/player", "participant/enemy"));

        var command = Assert.Single(fixture.Runtime.GameplayCommands);
        Assert.Equal(GameRuntimeCommandType.BasicAttack, command.Type);
        Assert.Equal(GameRuntimeCommandType.BasicAttack, fixture.Result.GameplayCommandType);
        Assert.DoesNotContain(fixture.Runtime.GameplayCommands,
            item => item.Type == GameRuntimeCommandType.UseAbility);
    }

    [Fact]
    public void Behavioral_package_sha_is_equal_before_and_after_dispatch()
    {
        var fixture = Goal163TestKit.Dispatch(GameRuntimeCommand.BasicAttack("participant/player", "participant/enemy"));

        Assert.Equal(fixture.Result.PackageSha256Before, fixture.Result.PackageSha256After);
        Assert.Equal(Goal163TestKit.PackageSha(fixture.Package), fixture.Result.PackageSha256After);
    }

    [Fact]
    public void Behavioral_definition_inventories_are_unchanged_by_dispatch()
    {
        var package = Goal163TestKit.CombatPackage();
        var before = Goal163TestKit.DefinitionInventory(package);
        var fixture = Goal163TestKit.Dispatch(
            GameRuntimeCommand.BasicAttack("participant/player", "participant/enemy"), package: package);

        Assert.True(fixture.Result.Passed);
        Assert.Equal(before, Goal163TestKit.DefinitionInventory(package));
    }

    [Fact]
    public void Behavioral_synthetic_campaign_ability_is_absent_from_exact_package()
    {
        var fixture = Goal163TestKit.Dispatch(GameRuntimeCommand.BasicAttack("participant/player", "participant/enemy"));

        Assert.DoesNotContain(fixture.Package.Game.Abilities,
            ability => ability.Id == "campaign/session-compatible-attack");
        Assert.DoesNotContain(fixture.Result.DefinitionIdsUsed,
            id => id == "campaign/session-compatible-attack");
    }

    [Fact]
    public void Contract_campaign_service_source_contains_no_fixed_power_or_package_clone()
    {
        var root = Goal163TestKit.RepositoryRoot;
        var source = File.ReadAllText(Path.Combine(root,
            "src", "LLMGameCreator.Application", "Play", "GeneratedCampaign",
            "GeneratedCampaignSessionService.cs"));

        Assert.DoesNotContain("ClonePackage", source, StringComparison.Ordinal);
        Assert.DoesNotContain("campaign/session-compatible-attack", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Power = 3", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_exact_package_ability_dispatch_succeeds()
    {
        var fixture = Goal163TestKit.Dispatch(
            GameRuntimeCommand.UseAbility("ability/package-strike", "participant/player", "participant/enemy"));

        Assert.True(fixture.Result.Passed, string.Join(",", fixture.Result.Diagnostics));
        Assert.Equal(GameRuntimeCommandType.UseAbility, Assert.Single(fixture.Runtime.GameplayCommands).Type);
        Assert.Equal(1, fixture.Result.UnifiedRuntimeResult.Session.GameplayState.ActiveEncounter!
            .Participants.Single(item => item.Id == "participant/enemy").Resources.Single().Amount);
    }

    [Fact]
    public void Behavioral_unavailable_ability_is_rejected_before_runtime_dispatch()
    {
        var fixture = Goal163TestKit.Dispatch(
            GameRuntimeCommand.UseAbility("ability/missing", "participant/player", "participant/enemy"));

        Assert.False(fixture.Result.Passed);
        Assert.Empty(fixture.Runtime.GameplayCommands);
        Assert.Contains("campaign.ability_not_available", fixture.Result.Diagnostics);
    }

    [Fact]
    public void Behavioral_invalid_ability_target_is_rejected_before_runtime_dispatch()
    {
        var fixture = Goal163TestKit.Dispatch(
            GameRuntimeCommand.UseAbility("ability/package-strike", "participant/player", "participant/player"));

        Assert.False(fixture.Result.Passed);
        Assert.Empty(fixture.Runtime.GameplayCommands);
        Assert.Contains("campaign.ability_target_invalid", fixture.Result.Diagnostics);
    }

    [Fact]
    public void Behavioral_real_qualified_generated_encounter_is_playable_after_combat_upgrade()
    {
        var package = Goal162TestKit.Package;
        var quest = package.Game.Quests.First(item => item.Objectives.Any(objective =>
            objective.Kind == "complete_encounter"));
        var encounterId = quest.Objectives.Single(item => item.Kind == "complete_encounter").TargetId;
        var encounter = package.Game.Encounters.Single(item => item.Id == encounterId);

        var readiness = new GeneratedCampaignCombatReadinessService().Evaluate(package, encounter);

        Assert.True(readiness.Playable);
        Assert.True(readiness.BasicAttackAvailable);
        Assert.NotEmpty(readiness.AbilityIds);
        Assert.DoesNotContain("campaign.encounter_no_executable_player_action", readiness.Diagnostics);
    }

    [Fact]
    public void Behavioral_real_qualified_package_executes_sample_combat_without_package_substitution()
    {
        var package = Goal162TestKit.Package;
        var encounter = package.Game.Encounters.Single(item => item.Id == "encounter/goblin_duel");
        var runtime = new Goal163SpyRuntime(Goal162TestKit.Bundle.Saves.Runtime);
        var dispatch = new GeneratedCampaignRuntimeDispatchService(runtime);
        var started = runtime.Start(package);
        var beforeStart = Goal163TestKit.Copy(started.Session);
        var start = dispatch.DispatchGameplay(package, started.Session,
            GameRuntimeCommand.StartEncounter(encounter.Id));
        var startOutcome = new GeneratedCampaignConsequenceProjector().ProjectAction(
            package, beforeStart, start.UnifiedRuntimeResult.Session,
            start.UnifiedRuntimeResult.MapEvents, start.UnifiedRuntimeResult.GameplayEvents,
            new GeneratedCampaignAction { Kind = GeneratedCampaignActionKind.StartEncounter, Title = "Начать встречу" },
            [], [], true, []);
        var beforeSha = Goal163TestKit.PackageSha(package);
        var beforeInventory = Goal163TestKit.DefinitionInventory(package);
        var beforeHealth = start.UnifiedRuntimeResult.Session.GameplayState.ActiveEncounter!
            .Participants.Single(item => item.Id == "goblin").Resources.Single().Amount;

        var attack = dispatch.DispatchGameplay(package, start.UnifiedRuntimeResult.Session,
            GameRuntimeCommand.BasicAttack("player", "goblin"));

        var afterHealth = attack.UnifiedRuntimeResult.Session.GameplayState.ActiveEncounter!
            .Participants.Single(item => item.Id == "goblin").Resources.Single().Amount;
        Assert.True(attack.Passed);
        Assert.True(attack.PackageReferencePreserved);
        Assert.Equal(GameRuntimeCommandType.BasicAttack, attack.GameplayCommandType);
        Assert.Contains(startOutcome.Consequences,
            item => item.Kind == GeneratedCampaignConsequenceKind.EncounterStarted);
        Assert.True(afterHealth < beforeHealth);
        Assert.Equal(beforeSha, Goal163TestKit.PackageSha(package));
        Assert.Equal(beforeInventory, Goal163TestKit.DefinitionInventory(package));
    }

    [Fact]
    public void Behavioral_exact_flee_runtime_path_grants_no_victory_reward_or_quest_readiness()
    {
        var package = Goal163TestKit.FullPackage();
        var session = Goal163TestKit.CombatSession();
        session.GameplayState.PackageId = package.Manifest.PackageId;
        var before = Goal163TestKit.Copy(session);

        var fixture = Goal163TestKit.Dispatch(
            new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter }, package, session);
        var outcome = new GeneratedCampaignConsequenceProjector().ProjectAction(
            package, before, fixture.Result.UnifiedRuntimeResult.Session,
            fixture.Result.UnifiedRuntimeResult.MapEvents, fixture.Result.UnifiedRuntimeResult.GameplayEvents,
            new GeneratedCampaignAction { Kind = GeneratedCampaignActionKind.FleeEncounter, Title = "Покинуть встречу" },
            [], [], fixture.Result.UnifiedRuntimeResult.Success, fixture.Result.Diagnostics);
        var readiness = new GeneratedCampaignQuestReadinessService()
            .Evaluate(package, Goal163TestKit.ReadyQuestSession(fled: true), "quest/generated");

        Assert.True(fixture.Result.Passed);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.EncounterFled);
        Assert.DoesNotContain(outcome.Consequences, item => item.Kind is GeneratedCampaignConsequenceKind.EncounterWon or GeneratedCampaignConsequenceKind.Reward);
        Assert.False(readiness.Ready);
    }
}

internal static class Goal163TestKit
{
    public static string RepositoryRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
        "..", "..", "..", "..", ".."));

    public static GamePackageDefinition CombatPackage()
    {
        var package = new GamePackageDefinition();
        package.Manifest.PackageId = "goal163-exact-combat";
        package.Game.Resources.Add(new ResourceDefinition
        {
            Id = "resource/health", Name = "Здоровье", Kind = "health", MinValue = 0, MaxValue = 10,
            Tags = ["health"]
        });
        package.Game.Abilities.Add(new AbilityDefinition
        {
            Id = "ability/package-strike", Name = "Точный удар", Kind = "attack",
            ResourceId = "resource/health", Power = 2, Tags = ["basic_attack"]
        });
        package.Game.Items.Add(new ItemDefinition { Id = "item/trophy", Name = "Знак победы" });
        package.Game.Encounters.Add(new EncounterDefinition
        {
            Id = "encounter/exact", Name = "Проверочная встреча", Kind = "combat",
            Participants =
            [
                new EncounterParticipantDefinition
                {
                    Id = "participant/player", Name = "Игрок", Kind = "player", Team = "player",
                    Abilities = ["ability/package-strike"],
                    Resources = [new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 8 }]
                },
                new EncounterParticipantDefinition
                {
                    Id = "participant/enemy", Name = "Противник", Kind = "enemy", Team = "enemy",
                    Resources = [new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 3 }]
                }
            ],
            Rewards = [new OutputDefinition { Kind = "item", Id = "item/trophy", Amount = 1 }]
        });
        return package;
    }

    public static GamePackageDefinition FullPackage()
    {
        var package = CombatPackage();
        package.Manifest.PackageId = "goal163-full-route";
        package.Game.Items.Add(new ItemDefinition { Id = "item/quest-reward", Name = "Награда хранителей" });
        package.Game.Factions.Add(new FactionDefinition
        {
            Id = "faction/keepers", Name = "Хранители", DefaultReputation = 0
        });
        package.Game.Quests.Add(new QuestDefinition
        {
            Id = "quest/generated", Title = "Испытание хранителей", Kind = "generated_quest",
            Tags = ["generated"],
            Objectives =
            [
                new QuestObjectiveDefinition
                {
                    Id = "objective/encounter", Kind = "complete_encounter",
                    TargetId = "encounter/exact", RequiredAmount = 1
                },
                new QuestObjectiveDefinition
                {
                    Id = "objective/item", Kind = "has_item", TargetId = "item/trophy",
                    RequiredAmount = 1
                }
            ],
            Rewards =
            [
                new OutputDefinition { Kind = "item", Id = "item/quest-reward", Amount = 1 },
                new OutputDefinition { Kind = "reputation", Id = "faction/keepers", Amount = 5 }
            ]
        });
        package.GeneratedContent.Quests.Add(new GeneratedQuestSeedDefinition
        {
            SourceId = "generated/quest/keepers", PackageQuestId = "quest/generated",
            Title = "Испытание хранителей"
        });
        return package;
    }

    public static UnifiedRuntimeSession CombatSession(double enemyHealth = 3, bool active = true)
    {
        return new UnifiedRuntimeSession
        {
            GameplayState = new GameRuntimeState
            {
                PackageId = "goal163-exact-combat",
                PlayerEntityId = "player",
                Inventories =
                [
                    new InventoryState { Id = "inventory/player", OwnerKind = "player", OwnerId = "player" }
                ],
                ActiveEncounter = new EncounterRuntimeState
                {
                    EncounterId = "encounter/exact", Active = active, Round = 1, TurnIndex = 0,
                    Participants =
                    [
                        new EncounterParticipantState
                        {
                            Id = "participant/player", Name = "Игрок", Team = "player", Alive = true,
                            Resources =
                            [
                                new ResourceState
                                {
                                    ResourceId = "resource/health", Amount = 8, Capacity = 10,
                                    Scope = "participant", OwnerId = "participant/player"
                                }
                            ]
                        },
                        new EncounterParticipantState
                        {
                            Id = "participant/enemy", Name = "Противник", Team = "enemy",
                            Alive = enemyHealth > 0,
                            Resources =
                            [
                                new ResourceState
                                {
                                    ResourceId = "resource/health", Amount = enemyHealth, Capacity = 10,
                                    Scope = "participant", OwnerId = "participant/enemy"
                                }
                            ]
                        }
                    ]
                }
            }
        };
    }

    public static UnifiedRuntimeSession ReadyQuestSession(bool fled = false, double itemAmount = 1)
    {
        var session = CombatSession(0, active: false);
        session.GameplayState.PackageId = "goal163-full-route";
        session.GameplayState.ActiveEncounter!.ActionHistory.Add(fled ? "flee" : "victory");
        session.GameplayState.Inventories[0].Stacks.Add(new ItemStackState
        {
            ItemId = "item/trophy", Amount = itemAmount
        });
        session.GameplayState.Factions.Add(new FactionRuntimeState
        {
            FactionId = "faction/keepers", Reputation = 0
        });
        session.GameplayState.Quests.Add(new QuestRuntimeState
        {
            QuestId = "quest/generated", State = "active",
            Objectives =
            [
                new QuestObjectiveRuntimeState
                {
                    ObjectiveId = "objective/encounter", Kind = "complete_encounter",
                    TargetId = "encounter/exact", RequiredAmount = 1
                },
                new QuestObjectiveRuntimeState
                {
                    ObjectiveId = "objective/item", Kind = "has_item", TargetId = "item/trophy",
                    RequiredAmount = 1
                }
            ]
        });
        session.GameplayState.QuestStates["quest/generated"] = "active";
        return session;
    }

    public static Goal163DispatchFixture Dispatch(
        GameRuntimeCommand command,
        GamePackageDefinition? package = null,
        UnifiedRuntimeSession? session = null)
    {
        package ??= CombatPackage();
        session ??= CombatSession();
        var initialSession = Copy(session);
        var spy = new Goal163SpyRuntime(Goal162TestKit.Bundle.Saves.Runtime);
        var service = new GeneratedCampaignRuntimeDispatchService(spy);
        var kind = command.Type switch
        {
            GameRuntimeCommandType.BasicAttack => GeneratedCampaignActionKind.BasicAttack,
            GameRuntimeCommandType.UseAbility => GeneratedCampaignActionKind.UseAbility,
            GameRuntimeCommandType.CompleteQuest => GeneratedCampaignActionKind.CompleteQuest,
            GameRuntimeCommandType.FleeEncounter => GeneratedCampaignActionKind.FleeEncounter,
            GameRuntimeCommandType.StartEncounter => GeneratedCampaignActionKind.StartEncounter,
            _ => GeneratedCampaignActionKind.EndTurn
        };
        var planned = new GeneratedCampaignPlannedAction(
            new GeneratedCampaignAction { ActionId = "test", Kind = kind, Title = "Проверка", Enabled = true },
            null,
            command);
        return new Goal163DispatchFixture(package, initialSession, spy,
            service.Dispatch(package, session, planned));
    }

    public static UnifiedRuntimeSession Copy(UnifiedRuntimeSession session) =>
        JsonSerializer.Deserialize<UnifiedRuntimeSession>(JsonSerializer.Serialize(session))!;

    public static string PackageSha(GamePackageDefinition package) =>
        GeneratedCampaignRuntimeDispatchService.PackageSha256(package);

    public static string DefinitionInventory(GamePackageDefinition package) => string.Join("|",
        package.Game.Abilities.Select(item => "ability:" + item.Id)
            .Concat(package.Game.Resources.Select(item => "resource:" + item.Id))
            .Concat(package.Game.Statuses.Select(item => "status:" + item.Id))
            .Concat(package.Game.Stats.Select(item => "stat:" + item.Id))
            .OrderBy(item => item, StringComparer.Ordinal));
}

internal sealed record Goal163DispatchFixture(
    GamePackageDefinition Package,
    UnifiedRuntimeSession InitialSession,
    Goal163SpyRuntime Runtime,
    GeneratedCampaignRuntimeDispatchResult Result);

internal sealed class Goal163SpyRuntime : IUnifiedGameRuntimeService
{
    private readonly IUnifiedGameRuntimeService _inner;
    public Goal163SpyRuntime(IUnifiedGameRuntimeService inner) => _inner = inner;
    public List<GamePackageDefinition> Packages { get; } = [];
    public List<GameRuntimeCommand> GameplayCommands { get; } = [];
    public List<PlayerCommand> PlayerCommands { get; } = [];

    public UnifiedRuntimeResult Start(GamePackageDefinition package)
    {
        Packages.Add(package);
        return _inner.Start(package);
    }

    public UnifiedRuntimeResult ExecutePlayerCommand(
        GamePackageDefinition package, UnifiedRuntimeSession session, PlayerCommand command)
    {
        Packages.Add(package);
        PlayerCommands.Add(command);
        return _inner.ExecutePlayerCommand(package, session, command);
    }

    public UnifiedRuntimeResult ExecuteGameplayCommand(
        GamePackageDefinition package, UnifiedRuntimeSession session, GameRuntimeCommand command)
    {
        Packages.Add(package);
        GameplayCommands.Add(command);
        return _inner.ExecuteGameplayCommand(package, session, command);
    }

    public UnifiedRuntimeResult ExecuteMany(
        GamePackageDefinition package, UnifiedRuntimeSession session,
        IEnumerable<GameRuntimeCommand> commands)
    {
        Packages.Add(package);
        var materialized = commands.ToList();
        GameplayCommands.AddRange(materialized);
        return _inner.ExecuteMany(package, session, materialized);
    }
}
