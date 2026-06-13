using LLMGameCreator.Scripting;
using Xunit;

namespace LLMGameCreator.Tests.Scripting;

public sealed class PrototypeLuaDeclarationMapperTests
{
    [Fact]
    public async Task MapsSupportedDeclarationsToPackageOperations()
    {
        var execution = await new PrototypeLuaExecutor(new PrototypeLuaStaticAnalyzer()).ExecuteAsync(new PrototypeLuaExecutionRequest
        {
            ScriptId = "script/test",
            Source = """
            data:extend({
              { type = "tile", id = "tile/grass", name = "Grass", walkable = true, movement_cost = 1.0 },
              { type = "map", id = "map/start", name = "Start", width = 4, height = 3, default_tile_id = "tile/grass", start_x = 1, start_y = 1 },
              { type = "entity_prototype", id = "entity/guard", name = "Guard", asset_id = "asset/entity/guard" },
              { type = "item", id = "item/red_herb", name = "Red Herb", kind = "material", max_stack = 20 },
              { type = "resource", id = "resource/mana", name = "Mana", kind = "magic", min_value = 0, max_value = 100 },
              { type = "recipe", id = "recipe/healing_potion", name = "Healing Potion", category = "alchemy", inputs = { { kind = "item", id = "item/red_herb", amount = 2 } }, outputs = { { kind = "resource", id = "resource/mana", amount = 5 } } },
              { type = "loot_table", id = "loot/goblin_common", name = "Goblin Common Loot", entries = { { id = "entry/gold", output = { kind = "resource", id = "resource/mana", amount = 3 } } } },
              { type = "transaction", id = "transaction/mage_training", name = "Mage Training", kind = "training", costs = { { kind = "resource", id = "resource/mana", amount = 10 } }, outputs = { { kind = "progression", id = "progression/fire_magic", amount = 1 } } },
              { type = "resource_network", id = "network/base_power", name = "Base Power Grid", resource_id = "resource/mana", kind = "mana_flow" },
              { type = "resource_node", id = "node/mana_generator", name = "Mana Generator", network_id = "network/base_power", production = { { kind = "resource", id = "resource/mana", amount = 20 } } },
              { type = "equipment_slot", id = "slot/tool", name = "Tool", allowed_tags = { "tool" } },
              { type = "stat", id = "stat/strength", name = "Strength", kind = "attribute", default_value = 5 },
              { type = "progression", id = "progression/level", name = "Level", kind = "xp_level", stages = { { id = "level/1", name = "Level 1", required_amount = 0 } } },
              { type = "ability", id = "ability/basic_attack", name = "Basic Attack", kind = "attack", power = 4, resource_id = "resource/mana" },
              { type = "encounter", id = "encounter/goblin_duel", name = "Goblin Duel", kind = "combat", participants = { { id = "player", name = "Player", team = "player" } } },
              { type = "manifest_update", title = "My Game", description = "Short", start_map_id = "map/start" }
            })
            """
        }, CancellationToken.None);

        var result = new PrototypeLuaDeclarationMapper().MapToPackageOperations(execution.Declarations);

        Assert.True(result.Success);
        Assert.Contains("upsert_tile_prototype", result.OperationsJson);
        Assert.Contains("upsert_map", result.OperationsJson);
        Assert.Contains("upsert_entity_prototype", result.OperationsJson);
        Assert.Contains("upsert_item_prototype", result.OperationsJson);
        Assert.Contains("upsert_resource", result.OperationsJson);
        Assert.Contains("upsert_recipe", result.OperationsJson);
        Assert.Contains("upsert_loot_table", result.OperationsJson);
        Assert.Contains("upsert_transaction", result.OperationsJson);
        Assert.Contains("upsert_resource_network", result.OperationsJson);
        Assert.Contains("upsert_resource_node", result.OperationsJson);
        Assert.Contains("upsert_equipment_slot", result.OperationsJson);
        Assert.Contains("upsert_stat", result.OperationsJson);
        Assert.Contains("upsert_progression", result.OperationsJson);
        Assert.Contains("upsert_ability", result.OperationsJson);
        Assert.Contains("upsert_encounter", result.OperationsJson);
        Assert.Contains("update_manifest", result.OperationsJson);
    }

    [Fact]
    public void RejectsUnknownFieldsBeforePatchValidation()
    {
        var result = new PrototypeLuaDeclarationMapper().MapToPackageOperations(new[]
        {
            new PrototypeLuaDeclaration
            {
                Type = "tile",
                Id = "tile/grass",
                Json = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "tile",
                    ["id"] = "tile/grass",
                    ["name"] = "Grass",
                    ["walkable"] = true,
                    ["movement_cost"] = 1.0,
                    ["script"] = "bad"
                }
            }
        });

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, item => item.Code == "lua.prototype.declaration.field.unknown");
    }
}
