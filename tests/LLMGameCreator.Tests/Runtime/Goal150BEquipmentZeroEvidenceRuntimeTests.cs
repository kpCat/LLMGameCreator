using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class Goal150BEquipmentZeroEvidenceRuntimeTests
{
    [Fact]
    public void Equipped_zero_metadata_is_observed_while_absent_metadata_invents_no_evidence()
    {
        var package = LoadPackage();
        var weapon = package.Game.Items.Single(item => item.Id == "item/rusty_knife");
        weapon.Metadata["combat_damage_bonus"] = "0";

        var withMetadata = AttackWithEquippedWeapon(package);
        var damage = Assert.Single(withMetadata.Events, item => item.Type == GameRuntimeEventType.DamageApplied);
        Assert.Equal("0", damage.Args["equipmentDamageBonus"]);
        Assert.DoesNotContain("statDamageBonus", damage.Args.Keys);
        Assert.DoesNotContain("totalAdditionalDamage", damage.Args.Keys);

        weapon.Metadata.Remove("combat_damage_bonus");
        var withoutMetadata = AttackWithEquippedWeapon(package);
        var baselineDamage = Assert.Single(withoutMetadata.Events, item => item.Type == GameRuntimeEventType.DamageApplied);
        Assert.DoesNotContain("equipmentDamageBonus", baselineDamage.Args.Keys);
    }

    private static GameRuntimeResult AttackWithEquippedWeapon(GamePackageDefinition package)
    {
        var requirement = new RequirementEvaluator();
        var state = new GameRuntimeStateFactory().CreateInitialState(package).State;
        Assert.True(new ContainerRuntimeService().TakeFromContainer(package, state, "inventory/chest_start",
            "item/rusty_knife", 1, "inventory/player_start").Success);
        Assert.True(new EquipmentRuntimeService(requirement).EquipItem(package, state, "item/rusty_knife",
            "slot/weapon", "inventory/player_start").Success);
        var encounter = new EncounterRuntimeService(requirement, new OutputApplier());
        Assert.True(encounter.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        var result = encounter.BasicAttack(package, state, "player", "goblin");
        Assert.True(result.Success);
        return result;
    }

    private static GamePackageDefinition LoadPackage()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Deserialize<GamePackageDefinition>(
            File.ReadAllText(Path.Combine(FindRoot(), "samples", "minimal-map-game", "package.json")), options)!;
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
