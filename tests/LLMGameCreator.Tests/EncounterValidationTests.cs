using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class EncounterValidationTests
{
    [Fact]
    public void EncounterReferencesAndDuplicatesAreValidated()
    {
        var package = CreatePackage();
        package.Game.Stats.Add(new StatDefinition { Id = "stat/strength", Name = "Strength Copy" });
        package.Game.Encounters.Add(new EncounterDefinition { Id = "encounter/goblin_duel", Name = "Duplicate" });
        package.Game.Encounters[0].Participants[0].Abilities.Add("ability/missing");
        package.Game.Encounters[0].Participants[0].Resources.Add(new OutputDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 });
        package.Game.Encounters[0].LootTableId = "loot/missing";
        package.Game.Abilities[0].Costs.Add(new CostDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "stat.id.duplicate");
        Assert.Contains(report.Issues, issue => issue.Code == "encounter.id.duplicate");
        Assert.Contains(report.Issues, issue => issue.Code == "encounter.ability_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "encounter.participant.resource_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "encounter.loot_table_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "ability.cost.resource_missing");
        Assert.DoesNotContain(report.Issues, issue => issue.Severity == ValidationSeverity.Critical);
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/encounter-validation", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                Resources = new List<ResourceDefinition> { new ResourceDefinition { Id = "resource/health", Name = "Health" } },
                Stats = new List<StatDefinition> { new StatDefinition { Id = "stat/strength", Name = "Strength" } },
                Abilities = new List<AbilityDefinition> { new AbilityDefinition { Id = "ability/basic_attack", Name = "Basic Attack", Kind = "attack" } },
                LootTables = new List<LootTableDefinition> { new LootTableDefinition { Id = "loot/goblin", Name = "Goblin Loot" } },
                Encounters = new List<EncounterDefinition>
                {
                    new EncounterDefinition
                    {
                        Id = "encounter/goblin_duel",
                        Name = "Goblin Duel",
                        LootTableId = "loot/goblin",
                        Participants = new List<EncounterParticipantDefinition>
                        {
                            new EncounterParticipantDefinition
                            {
                                Id = "player",
                                Name = "Player",
                                Team = "player",
                                Resources = new List<OutputDefinition> { new OutputDefinition { Kind = "resource", Id = "resource/health", Amount = 10 } },
                                Stats = new List<OutputDefinition> { new OutputDefinition { Kind = "stat", Id = "stat/strength", Amount = 5 } },
                                Abilities = new List<string> { "ability/basic_attack" }
                            }
                        }
                    }
                }
            }
        };
    }
}
