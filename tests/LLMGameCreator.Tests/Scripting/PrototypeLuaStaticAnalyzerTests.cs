using LLMGameCreator.Scripting;
using Xunit;

namespace LLMGameCreator.Tests.Scripting;

public sealed class PrototypeLuaStaticAnalyzerTests
{
    [Theory]
    [InlineData("io.open('x')", "lua.prototype.forbidden_api")]
    [InlineData("os.execute('x')", "lua.prototype.forbidden_api")]
    [InlineData("debug.traceback()", "lua.prototype.forbidden_api")]
    [InlineData("dofile('x.lua')", "lua.prototype.forbidden_loader")]
    [InlineData("loadfile('x.lua')", "lua.prototype.forbidden_loader")]
    [InlineData("load('return 1')", "lua.prototype.forbidden_loader")]
    [InlineData("require('x')", "lua.prototype.forbidden_loader")]
    [InlineData("package.path = 'x'", "lua.prototype.forbidden_package")]
    [InlineData("math.random()", "lua.prototype.forbidden_random")]
    public void AnalyzerRejectsForbiddenTokens(string source, string code)
    {
        var analyzer = new PrototypeLuaStaticAnalyzer();

        var diagnostics = analyzer.Analyze(source, "test.lua");

        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == code);
    }

    [Fact]
    public void AnalyzerAllowsNormalDataExtendAndIgnoresCommentsAndStrings()
    {
        var analyzer = new PrototypeLuaStaticAnalyzer();

        var diagnostics = analyzer.Analyze("""
        -- io.open('ignored')
        local text = "require('ignored')"
        data:extend({
          { type = "tile", id = "tile/grass", name = "Grass", walkable = true, movement_cost = 1.0 }
        })
        """, "test.lua");

        Assert.Empty(diagnostics);
    }
}

