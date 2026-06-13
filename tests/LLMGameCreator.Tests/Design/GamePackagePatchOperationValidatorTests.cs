using LLMGameCreator.Application.Design;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GamePackagePatchOperationValidatorTests
{
    private readonly GamePackagePatchOperationValidator _validator = new();

    [Fact]
    public void PackageOperationsValidatorAcceptsValidOperations()
    {
        var result = _validator.ValidatePackageOperationsJson($$"""
        [
          {{TileOperation("tile/stone", "Stone")}},
          {{MapOperation("map/start")}},
          {{ItemOperation("item/red_herb", "Red Herb")}},
          {{ResourceOperation("resource/mana", "Mana")}},
          {{RecipeOperation("recipe/healing_potion")}},
          {{LootTableOperation("loot/goblin_common")}},
          {{TransactionOperation("transaction/buy_healing_potion")}},
          {{ResourceNetworkOperation("network/base_power")}},
          {{ResourceNodeOperation("node/diesel_generator")}},
          {{InventoryOperation("inventory/player_start")}},
          {{StatOperation("stat/strength")}},
          {{ProgressionOperation("progression/level")}},
          {{EncounterOperation("encounter/goblin_duel")}},
          {{AbilityOperation("ability/basic_attack")}},
          { "op": "update_manifest", "title": "Stone Game", "start_map_id": "map/start" }
        ]
        """, "test");

        Assert.DoesNotContain(result.ValidationResults, item => item.Severity == "error");
        Assert.Equal(15, result.Operations.Count);
    }

    [Fact]
    public void PackageOperationsValidatorRejectsUnknownOperation()
    {
        var result = _validator.ValidatePackageOperationsJson("""[{ "op": "merge_anything", "path": "/game" }]""", "test");

        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.op.unknown");
    }

    [Fact]
    public void PackageOperationsValidatorRejectsDeleteOperation()
    {
        var result = _validator.ValidatePackageOperationsJson("""[{ "op": "delete_map", "id": "map/start" }]""", "test");

        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.delete.unsupported");
    }

    [Fact]
    public void PackageOperationsValidatorRejectsDuplicateTarget()
    {
        var result = _validator.ValidatePackageOperationsJson($$"""
        [
          {{TileOperation("tile/stone", "Stone")}},
          {{TileOperation("tile/stone", "Stone 2")}}
        ]
        """, "test");

        Assert.Contains(result.ValidationResults, item => item.Code == "patch.operation.duplicate_target");
    }

    private static string TileOperation(string id, string name)
    {
        return $$"""
        {
          "op": "upsert_tile_prototype",
          "id": "{{id}}",
          "name": "{{name}}",
          "walkable": true,
          "movement_cost": 1.0
        }
        """;
    }

    private static string MapOperation(string id)
    {
        return $$"""
        {
          "op": "upsert_map",
          "id": "{{id}}",
          "name": "Start",
          "width": 8,
          "height": 8,
          "default_tile_id": "tile/stone",
          "start_x": 1,
          "start_y": 1
        }
        """;
    }

    private static string ItemOperation(string id, string name)
    {
        return $$"""{ "op": "upsert_item_prototype", "id": "{{id}}", "name": "{{name}}", "kind": "material", "max_stack": 20 }""";
    }

    private static string ResourceOperation(string id, string name)
    {
        return $$"""{ "op": "upsert_resource", "id": "{{id}}", "name": "{{name}}", "kind": "magic", "min_value": 0, "max_value": 100 }""";
    }

    private static string RecipeOperation(string id)
    {
        return $$"""{ "op": "upsert_recipe", "id": "{{id}}", "name": "Healing Potion", "category": "alchemy", "inputs": [{ "kind": "item", "id": "item/red_herb", "amount": 2 }], "outputs": [{ "kind": "resource", "id": "resource/mana", "amount": 5 }] }""";
    }

    private static string LootTableOperation(string id)
    {
        return $$"""{ "op": "upsert_loot_table", "id": "{{id}}", "name": "Goblin Common Loot", "entries": [{ "id": "entry/gold", "output": { "kind": "resource", "id": "resource/mana", "amount": 3 } }] }""";
    }

    private static string TransactionOperation(string id)
    {
        return $$"""{ "op": "upsert_transaction", "id": "{{id}}", "name": "Buy Healing Potion", "costs": [{ "kind": "resource", "id": "resource/mana", "amount": 5 }], "outputs": [{ "kind": "item", "id": "item/red_herb", "amount": 1 }] }""";
    }

    private static string ResourceNetworkOperation(string id)
    {
        return $$"""{ "op": "upsert_resource_network", "id": "{{id}}", "name": "Base Power Grid", "resource_id": "resource/mana", "kind": "electricity" }""";
    }

    private static string ResourceNodeOperation(string id)
    {
        return $$"""{ "op": "upsert_resource_node", "id": "{{id}}", "name": "Diesel Generator", "kind": "producer", "network_id": "network/base_power", "production": [{ "kind": "resource", "id": "resource/mana", "amount": 20 }] }""";
    }

    private static string InventoryOperation(string id)
    {
        return $$"""{ "op": "upsert_inventory", "id": "{{id}}", "owner_kind": "player", "slots": 16, "stacks": [{ "item_id": "item/red_herb", "amount": 1 }] }""";
    }

    private static string StatOperation(string id)
    {
        return $$"""{ "op": "upsert_stat", "id": "{{id}}", "name": "Strength", "kind": "attribute", "default_value": 5 }""";
    }

    private static string ProgressionOperation(string id)
    {
        return $$"""{ "op": "upsert_progression", "id": "{{id}}", "name": "Level", "kind": "xp_level", "stages": [{ "id": "level/1", "name": "Level 1", "required_amount": 0 }] }""";
    }

    private static string EncounterOperation(string id)
    {
        return $$"""{ "op": "upsert_encounter", "id": "{{id}}", "name": "Goblin Duel", "kind": "combat", "participants": [{ "id": "player", "name": "Player", "team": "player" }] }""";
    }

    private static string AbilityOperation(string id)
    {
        return $$"""{ "op": "upsert_ability", "id": "{{id}}", "name": "Basic Attack", "kind": "attack", "power": 4, "resource_id": "resource/mana" }""";
    }
}
