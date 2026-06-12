using LLMGameCreator.Scripting;
using Xunit;

namespace LLMGameCreator.Tests.Scripting;

public sealed class PrototypeLuaExecutorTests
{
    [Fact]
    public async Task ExecutesValidTileDeclaration()
    {
        var result = await CreateExecutor().ExecuteAsync(Request("""
        data:extend({
          { type = "tile", id = "tile/grass", name = "Grass", walkable = true, movement_cost = 1.0 }
        })
        """), CancellationToken.None);

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(item => item.Message)));
        Assert.Single(result.Declarations);
        Assert.Equal("tile", result.Declarations[0].Type);
        Assert.Equal("tile/grass", result.Declarations[0].Id);
    }

    [Fact]
    public async Task ExecutesMultipleDeclarationsInDeterministicOrder()
    {
        var result = await CreateExecutor().ExecuteAsync(Request("""
        data:extend({
          { type = "tile", id = "tile/grass", name = "Grass", walkable = true, movement_cost = 1.0 },
          { type = "map", id = "map/start", name = "Start", width = 4, height = 3, default_tile_id = "tile/grass", start_x = 1, start_y = 1 },
          { type = "entity_prototype", id = "entity/guard", name = "Guard" },
          { type = "item", id = "item/red_herb", name = "Red Herb" },
          { type = "resource", id = "resource/mana", name = "Mana" },
          { type = "recipe", id = "recipe/healing_potion", name = "Healing Potion" },
          { type = "loot_table", id = "loot/goblin_common", name = "Goblin Common Loot" },
          { type = "transaction", id = "transaction/mage_training", name = "Mage Training" },
          { type = "resource_network", id = "network/base_power", name = "Base Power Grid", resource_id = "resource/mana" },
          { type = "resource_node", id = "node/mana_generator", name = "Mana Generator" },
          { type = "inventory", id = "inventory/player_start", owner_kind = "player" }
        })
        """), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(new[] { "tile", "map", "entity_prototype", "item", "resource", "recipe", "loot_table", "transaction", "resource_network", "resource_node", "inventory" }, result.Declarations.Select(item => item.Type));
    }

    [Fact]
    public async Task RejectsUnknownDeclarationType()
    {
        var result = await CreateExecutor().ExecuteAsync(Request("""
        data:extend({ { type = "spell", id = "spell/fire" } })
        """), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, item => item.Code == "lua.prototype.runtime_error");
    }

    [Fact]
    public async Task EnforcesMaxDeclarations()
    {
        var result = await CreateExecutor().ExecuteAsync(new PrototypeLuaExecutionRequest
        {
            ScriptId = "script/test",
            MaxDeclarations = 1,
            Source = """
            data:extend({
              { type = "tile", id = "tile/grass", name = "Grass", walkable = true, movement_cost = 1.0 },
              { type = "tile", id = "tile/water", name = "Water", walkable = false, movement_cost = 2.0 }
            })
            """
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, item => item.Message.Contains("declaration limit", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("return io")]
    [InlineData("return os")]
    [InlineData("return debug")]
    [InlineData("require('x')")]
    public async Task ForbiddenGlobalsAreNotAvailable(string source)
    {
        var result = await CreateExecutor().ExecuteAsync(Request(source), CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DoesNotExposeDirectHostObjectAccess()
    {
        var result = await CreateExecutor().ExecuteAsync(Request("""
        if clr ~= nil then
          data:extend({
            { type = "tile", id = "tile/host", name = "Host", walkable = true, movement_cost = 1.0 }
          })
        end
        """), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Declarations);
    }

    [Fact]
    public async Task ReturnsDiagnosticsOnSyntaxError()
    {
        var result = await CreateExecutor().ExecuteAsync(Request("data:extend({"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, item => item.Code == "lua.prototype.syntax_error");
    }

    private static PrototypeLuaExecutor CreateExecutor()
    {
        return new PrototypeLuaExecutor(new PrototypeLuaStaticAnalyzer());
    }

    private static PrototypeLuaExecutionRequest Request(string source)
    {
        return new PrototypeLuaExecutionRequest
        {
            ScriptId = "script/test",
            Source = source
        };
    }
}
