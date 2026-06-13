using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeNarrativeTests
{
    [Fact]
    public void DialogueQuestFactionFlowIsRuntimeOnlyAndSnapshotFriendly()
    {
        var package = CreatePackage();
        var beforePackage = JsonSerializer.Serialize(package);
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var open = runtime.Execute(package, state, GameRuntimeCommand.OpenDialogue("dialogue/healer"));
        var choose = runtime.Execute(package, state, GameRuntimeCommand.ChooseDialogueOption("accept"));
        var advance = runtime.Execute(package, state, GameRuntimeCommand.AdvanceQuestObjective("quest/help_healer", "objective/herbs", 3));

        Assert.True(open.Success);
        Assert.True(choose.Success);
        Assert.True(advance.Success);
        Assert.Contains(choose.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.QuestStarted);
        Assert.Contains(advance.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.QuestCompleted);
        Assert.Contains(state.Quests, quest => quest.QuestId == "quest/help_healer" && quest.State == "completed");
        Assert.Contains(state.Factions, faction => faction.FactionId == "faction/village" && faction.Reputation == 5);
        Assert.False(state.ActiveDialogue!.Open);
        Assert.Equal(beforePackage, JsonSerializer.Serialize(package));

        var stateJson = new RuntimeStateSerializer().Serialize(state);
        Assert.Contains("quest/help_healer", stateJson);
        Assert.DoesNotContain("tilePrototypes", stateJson, StringComparison.OrdinalIgnoreCase);
    }

    private static IGameRuntimeService CreateRuntime()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var encounterRuntime = new EncounterRuntimeService(requirementEvaluator, outputApplier);
        var questRuntime = new QuestRuntimeService(requirementEvaluator, outputApplier);
        var dialogueRuntime = new DialogueRuntimeService(requirementEvaluator, costConsumer, outputApplier, questRuntime, transactionRuntimeService, encounterRuntime);
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new UseItemRuntimeService(requirementEvaluator, outputApplier),
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService, dialogueRuntimeService: dialogueRuntime, questRuntimeService: questRuntime, encounterRuntimeService: encounterRuntime),
            encounterRuntimeService: encounterRuntime,
            questRuntimeService: questRuntime,
            dialogueRuntimeService: dialogueRuntime,
            factionRuntimeService: new FactionRuntimeService(),
            questObjectiveTracker: new QuestObjectiveTracker(questRuntime));
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/narrative-runtime", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                Items = new List<ItemDefinition> { new ItemDefinition { Id = "item/red_herb", Name = "Red Herb" } },
                Factions = new List<FactionDefinition> { new FactionDefinition { Id = "faction/village", Name = "Village", MinReputation = -100, DefaultReputation = 0, MaxReputation = 100 } },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition
                    {
                        Id = "inventory/player",
                        OwnerKind = "player",
                        Stacks = new List<ItemStackDefinition> { new ItemStackDefinition { ItemId = "item/red_herb", Amount = 3 } }
                    }
                },
                Quests = new List<QuestDefinition>
                {
                    new QuestDefinition
                    {
                        Id = "quest/help_healer",
                        Title = "Help Healer",
                        Description = "Gather herbs.",
                        Objectives = new List<QuestObjectiveDefinition>
                        {
                            new QuestObjectiveDefinition { Id = "objective/herbs", Kind = "has_item", TargetId = "item/red_herb", RequiredAmount = 3 }
                        },
                        Rewards = new List<OutputDefinition> { new OutputDefinition { Kind = "reputation", Id = "faction/village", Amount = 5 } }
                    }
                },
                Dialogues = new List<DialogueDefinition>
                {
                    new DialogueDefinition
                    {
                        Id = "dialogue/healer",
                        Title = "Healer",
                        StartNodeId = "start",
                        Nodes = new List<DialogueNodeDefinition>
                        {
                            new DialogueNodeDefinition
                            {
                                Id = "start",
                                SpeakerId = "npc/healer",
                                Text = "Can you help?",
                                Choices = new List<DialogueChoiceDefinition>
                                {
                                    new DialogueChoiceDefinition { Id = "accept", Text = "Yes", StartQuestId = "quest/help_healer", CloseDialogue = true }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
