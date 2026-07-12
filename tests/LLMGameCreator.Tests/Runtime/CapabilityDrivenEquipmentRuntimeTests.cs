using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Runtime;

public sealed class CapabilityDrivenEquipmentRuntimeTests
{
    [Fact]
    public void Equipment_bonus_is_applied_only_while_weapon_is_equipped_and_non_player_attack_is_unchanged()
    {
        var package = LoadPackage();
        package.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"] = "2";
        var requirement = new RequirementEvaluator();
        var equipment = new EquipmentRuntimeService(requirement);
        var container = new ContainerRuntimeService();
        var encounter = new EncounterRuntimeService(requirement, new OutputApplier());

        var historical = Initial(package);
        Assert.True(encounter.StartEncounter(package, historical, "encounter/goblin_duel", 136).Success);
        Assert.True(encounter.BasicAttack(package, historical, "player", "goblin").Success);
        Assert.Equal(8, Health(historical, "goblin"));

        var equipped = Initial(package);
        Assert.True(container.TakeFromContainer(package, equipped, "inventory/chest_start", "item/rusty_knife", 1,
            "inventory/player_start").Success);
        Assert.True(equipment.EquipItem(package, equipped, "item/rusty_knife", "slot/weapon", "inventory/player_start").Success);
        Assert.True(encounter.StartEncounter(package, equipped, "encounter/goblin_duel", 136).Success);
        var equippedAttack = encounter.BasicAttack(package, equipped, "player", "goblin");
        Assert.True(equippedAttack.Success);
        Assert.Equal(6, Health(equipped, "goblin"));
        Assert.Contains(equippedAttack.Events, runtimeEvent =>
            runtimeEvent.Args.GetValueOrDefault("equipmentDamageBonus") == "2");

        var unequipped = Initial(package);
        Assert.True(container.TakeFromContainer(package, unequipped, "inventory/chest_start", "item/rusty_knife", 1,
            "inventory/player_start").Success);
        Assert.True(equipment.EquipItem(package, unequipped, "item/rusty_knife", "slot/weapon", "inventory/player_start").Success);
        Assert.True(equipment.UnequipItem(package, unequipped, "slot/weapon", "inventory/player_start").Success);
        Assert.True(encounter.StartEncounter(package, unequipped, "encounter/goblin_duel", 136).Success);
        var unequippedAttack = encounter.BasicAttack(package, unequipped, "player", "goblin");
        Assert.True(unequippedAttack.Success);
        Assert.Equal(8, Health(unequipped, "goblin"));
        Assert.DoesNotContain(unequippedAttack.Events, runtimeEvent => runtimeEvent.Args.ContainsKey("equipmentDamageBonus"));

        var nonPlayer = Initial(package);
        Assert.True(container.TakeFromContainer(package, nonPlayer, "inventory/chest_start", "item/rusty_knife", 1,
            "inventory/player_start").Success);
        Assert.True(equipment.EquipItem(package, nonPlayer, "item/rusty_knife", "slot/weapon", "inventory/player_start").Success);
        Assert.True(encounter.StartEncounter(package, nonPlayer, "encounter/goblin_duel", 136).Success);
        Assert.True(encounter.EndTurn(package, nonPlayer).Success);
        var nonPlayerAttack = encounter.BasicAttack(package, nonPlayer, "goblin", "player");
        Assert.True(nonPlayerAttack.Success);
        Assert.DoesNotContain(nonPlayerAttack.Events, runtimeEvent => runtimeEvent.Args.ContainsKey("equipmentDamageBonus"));
    }

    [Fact]
    public void Invalid_equipped_weapon_metadata_fails_without_combat_state_mutation()
    {
        var package = LoadPackage();
        package.Game.Items.Single(item => item.Id == "item/rusty_knife").Metadata["combat_damage_bonus"] = "broken";
        var requirement = new RequirementEvaluator();
        var equipment = new EquipmentRuntimeService(requirement);
        var container = new ContainerRuntimeService();
        var encounter = new EncounterRuntimeService(requirement, new OutputApplier());
        var state = Initial(package);
        Assert.True(container.TakeFromContainer(package, state, "inventory/chest_start", "item/rusty_knife", 1,
            "inventory/player_start").Success);
        Assert.True(equipment.EquipItem(package, state, "item/rusty_knife", "slot/weapon", "inventory/player_start").Success);
        Assert.True(encounter.StartEncounter(package, state, "encounter/goblin_duel", 136).Success);
        var before = Health(state, "goblin");

        var result = encounter.BasicAttack(package, state, "player", "goblin");

        Assert.False(result.Success);
        Assert.Equal(before, Health(state, "goblin"));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "combat.equipment_damage_bonus.invalid");
    }

    private static GameRuntimeState Initial(GamePackageDefinition package) =>
        new GameRuntimeStateFactory().CreateInitialState(package).State;

    private static double Health(GameRuntimeState state, string participantId) => state.ActiveEncounter!.Participants
        .Single(participant => participant.Id == participantId).Resources
        .Single(resource => resource.ResourceId == "resource/health").Amount;

    private static GamePackageDefinition LoadPackage()
    {
        var root = FindRoot();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Deserialize<GamePackageDefinition>(
            File.ReadAllText(Path.Combine(root, "samples", "minimal-map-game", "package.json")), options)!;
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
