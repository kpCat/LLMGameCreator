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
              { type = "manifest_update", title = "My Game", description = "Short", start_map_id = "map/start" }
            })
            """
        }, CancellationToken.None);

        var result = new PrototypeLuaDeclarationMapper().MapToPackageOperations(execution.Declarations);

        Assert.True(result.Success);
        Assert.Contains("upsert_tile_prototype", result.OperationsJson);
        Assert.Contains("upsert_map", result.OperationsJson);
        Assert.Contains("upsert_entity_prototype", result.OperationsJson);
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

