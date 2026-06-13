using System.Text.Json;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class NarrativeValidationTests
{
    [Fact]
    public void NarrativeContractsRoundtripAndDefaultListsRemainSafe()
    {
        var older = JsonSerializer.Deserialize<GamePackageDefinition>("""
        {
          "manifest": { "packageId": "game/old", "title": "Old", "version": "0.1", "formatVersion": "0.1", "startMapId": "map/start" },
          "game": { "maps": [] },
          "assetCatalog": { "contracts": [], "assets": [], "generationRequests": [] },
          "scriptCatalog": { "scripts": [], "generators": [] }
        }
        """, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(older);
        Assert.Empty(older!.Game.Factions);
        Assert.Empty(older.Game.Quests);
        Assert.Empty(older.Game.Dialogues);

        var package = NarrativePackage();
        var json = JsonSerializer.Serialize(package);
        var roundtrip = JsonSerializer.Deserialize<GamePackageDefinition>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(roundtrip);
        Assert.Single(roundtrip!.Game.Factions);
        Assert.Single(roundtrip.Game.Quests[0].Objectives);
        Assert.Single(roundtrip.Game.Dialogues[0].Nodes[0].Choices);
    }

    [Fact]
    public void NarrativeValidatorChecksKeyReferences()
    {
        var package = NarrativePackage();
        package.Game.Dialogues[0].StartNodeId = "missing";
        package.Game.Quests[0].Objectives[0].TargetId = "item/missing";
        package.Game.Factions[0].Relations.Add(new FactionRelationDefinition { FactionId = "faction/missing" });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "dialogue.start_node_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "quest.objective.item_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "faction.relation.target_missing");
    }

    private static GamePackageDefinition NarrativePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/narrative", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                Items = new List<ItemDefinition> { new ItemDefinition { Id = "item/red_herb", Name = "Red Herb" } },
                Resources = new List<ResourceDefinition> { new ResourceDefinition { Id = "resource/gold", Name = "Gold", MinValue = 0 } },
                Factions = new List<FactionDefinition> { new FactionDefinition { Id = "faction/village", Name = "Village", MinReputation = -100, DefaultReputation = 0, MaxReputation = 100 } },
                Quests = new List<QuestDefinition>
                {
                    new QuestDefinition
                    {
                        Id = "quest/help_healer",
                        Title = "Help Healer",
                        Description = "Gather herbs.",
                        Objectives = new List<QuestObjectiveDefinition>
                        {
                            new QuestObjectiveDefinition { Id = "objective/herbs", Kind = "collect_item", TargetId = "item/red_herb", RequiredAmount = 3 }
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
