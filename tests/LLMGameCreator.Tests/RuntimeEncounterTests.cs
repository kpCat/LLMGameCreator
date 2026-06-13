using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeEncounterTests
{
    [Fact]
    public void StartEncounterCreatesRuntimeOnlyStateAndEvents()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var result = runtime.Execute(package, state, GameRuntimeCommand.StartEncounter("encounter/goblin_duel", seed: 7));

        Assert.True(result.Success);
        Assert.NotNull(state.ActiveEncounter);
        Assert.Contains(state.ActiveEncounter!.Participants, participant => participant.Id == "player" && participant.Resources.Any(resource => resource.ResourceId == "resource/health" && resource.Amount == 20));
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.EncounterStarted);
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.TurnStarted);
    }

    [Fact]
    public void AbilityDamageHealingStatusWinRewardsProgressionAndAiAreDeterministic()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        runtime.Execute(package, state, GameRuntimeCommand.StartEncounter("encounter/goblin_duel", seed: 7));

        var attack1 = runtime.Execute(package, state, GameRuntimeCommand.BasicAttack("player", "goblin"));
        var ai = runtime.Execute(package, state, new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
        var attack2 = runtime.Execute(package, state, GameRuntimeCommand.BasicAttack("player", "goblin"));
        var ai2 = runtime.Execute(package, state, new GameRuntimeCommand { Type = GameRuntimeCommandType.RunCurrentTurnAi });
        var attack3 = runtime.Execute(package, state, GameRuntimeCommand.BasicAttack("player", "goblin"));

        Assert.True(attack1.Success);
        Assert.True(ai.Success);
        Assert.True(attack2.Success);
        Assert.True(ai2.Success);
        Assert.True(attack3.Success);
        Assert.Contains(ai.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.AiActionChosen);
        Assert.False(state.ActiveEncounter!.Active);
        Assert.Contains(attack3.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.EncounterWon);
        Assert.Contains(attack3.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.RewardGranted);
        Assert.Contains(state.Progressions, progression => progression.ProgressionId == "progression/level" && progression.Amount == 10 && progression.StageId == "level/2");
        Assert.Contains(state.Resources, resource => resource.ResourceId == "resource/gold" && resource.Amount >= 5);
    }

    [Fact]
    public void AbilityFailureDoesNotMutateStateOrPackageDefinitions()
    {
        var package = CreatePackage();
        var packageBefore = JsonSerializer.Serialize(package);
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        runtime.Execute(package, state, GameRuntimeCommand.StartEncounter("encounter/goblin_duel", seed: 7));
        var before = JsonSerializer.Serialize(state);

        var result = runtime.Execute(package, state, GameRuntimeCommand.UseAbility("ability/heal_minor", "player", "player"));

        Assert.False(result.Success);
        Assert.Equal(before, JsonSerializer.Serialize(state));
        Assert.Equal(packageBefore, JsonSerializer.Serialize(package));
    }

    [Fact]
    public void FleeEncounterEndsWithoutRewards()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        runtime.Execute(package, state, GameRuntimeCommand.StartEncounter("encounter/goblin_duel", seed: 7));

        var result = runtime.Execute(package, state, new GameRuntimeCommand { Type = GameRuntimeCommandType.FleeEncounter });

        Assert.True(result.Success);
        Assert.False(state.ActiveEncounter!.Active);
        Assert.DoesNotContain(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.RewardGranted);
    }

    private static IGameRuntimeService CreateRuntime()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var encounterRuntime = new EncounterRuntimeService(requirementEvaluator, outputApplier);
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new UseItemRuntimeService(requirementEvaluator, outputApplier),
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService),
            encounterRuntimeService: encounterRuntime,
            encounterAiService: new EncounterAiService(encounterRuntime));
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/encounter-runtime", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                Resources = new List<ResourceDefinition>
                {
                    new ResourceDefinition { Id = "resource/health", Name = "Health", Kind = "health", MinValue = 0, MaxValue = 20, Tags = new List<string> { "health" } },
                    new ResourceDefinition { Id = "resource/stamina", Name = "Stamina", MinValue = 0, MaxValue = 10 },
                    new ResourceDefinition { Id = "resource/gold", Name = "Gold", MinValue = 0 }
                },
                Statuses = new List<StatusDefinition> { new StatusDefinition { Id = "status/poisoned", Name = "Poisoned" } },
                Progressions = new List<ProgressionDefinition>
                {
                    new ProgressionDefinition
                    {
                        Id = "progression/level",
                        Name = "Level",
                        Stages = new List<ProgressionStageDefinition>
                        {
                            new ProgressionStageDefinition { Id = "level/1", Name = "Level 1", RequiredAmount = 0 },
                            new ProgressionStageDefinition { Id = "level/2", Name = "Level 2", RequiredAmount = 10 }
                        }
                    }
                },
                Abilities = new List<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Id = "ability/basic_attack",
                        Name = "Basic Attack",
                        Kind = "attack",
                        Power = 4,
                        ResourceId = "resource/health",
                        Tags = new List<string> { "basic_attack" },
                        Effects = new List<EffectDefinition> { new EffectDefinition { Type = "damage_resource", Args = new Dictionary<string, string> { ["id"] = "resource/health", ["amount"] = "4" } } }
                    },
                    new AbilityDefinition
                    {
                        Id = "ability/goblin_slash",
                        Name = "Goblin Slash",
                        Kind = "attack",
                        Effects = new List<EffectDefinition> { new EffectDefinition { Type = "damage_resource", Args = new Dictionary<string, string> { ["id"] = "resource/health", ["amount"] = "3" } } }
                    },
                    new AbilityDefinition
                    {
                        Id = "ability/heal_minor",
                        Name = "Minor Heal",
                        Kind = "heal",
                        Costs = new List<CostDefinition> { new CostDefinition { Kind = "resource", Id = "resource/stamina", Amount = 99 } },
                        Effects = new List<EffectDefinition> { new EffectDefinition { Type = "heal_resource", Args = new Dictionary<string, string> { ["id"] = "resource/health", ["amount"] = "5", ["scope"] = "source" } } }
                    }
                },
                LootTables = new List<LootTableDefinition>
                {
                    new LootTableDefinition
                    {
                        Id = "loot/goblin",
                        Name = "Goblin Loot",
                        Entries = new List<LootEntryDefinition> { new LootEntryDefinition { Id = "entry/gold", Weight = 1, Output = new OutputDefinition { Kind = "resource", Id = "resource/gold", Amount = 1 } } }
                    }
                },
                Encounters = new List<EncounterDefinition>
                {
                    new EncounterDefinition
                    {
                        Id = "encounter/goblin_duel",
                        Name = "Goblin Duel",
                        Kind = "combat",
                        LootTableId = "loot/goblin",
                        Rewards = new List<OutputDefinition>
                        {
                            new OutputDefinition { Kind = "resource", Id = "resource/gold", Amount = 5 },
                            new OutputDefinition { Kind = "progression", Id = "progression/level", Amount = 10 }
                        },
                        Participants = new List<EncounterParticipantDefinition>
                        {
                            new EncounterParticipantDefinition
                            {
                                Id = "player",
                                Name = "Player",
                                Kind = "player",
                                Team = "player",
                                Resources = new List<OutputDefinition>
                                {
                                    new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 20 },
                                    new OutputDefinition { Kind = "resource", Id = "resource/stamina", Amount = 2 }
                                },
                                Abilities = new List<string> { "ability/basic_attack", "ability/heal_minor" }
                            },
                            new EncounterParticipantDefinition
                            {
                                Id = "goblin",
                                Name = "Goblin",
                                Kind = "enemy",
                                Team = "enemy",
                                Resources = new List<OutputDefinition> { new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 12 } },
                                Abilities = new List<string> { "ability/goblin_slash" }
                            }
                        }
                    }
                }
            }
        };
    }
}
