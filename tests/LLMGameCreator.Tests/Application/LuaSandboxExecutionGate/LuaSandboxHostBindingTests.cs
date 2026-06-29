using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaSandboxExecutionGate;

public sealed class LuaSandboxHostBindingTests
{
    [Fact]
    public void HostBindingMatrixDeniesDangerousGroupsAndKeepsLuaNonExecutable()
    {
        var matrix = LuaSandboxExecutionGateCatalog.BuildHostBindingMatrix();
        var denied = matrix.DeniedGroupIds.Concat(matrix.BoundaryBlockedGroupIds).ToHashSet(StringComparer.Ordinal);

        Assert.False(matrix.LuaExecutable);
        Assert.Contains("file_system", denied);
        Assert.Contains("network", denied);
        Assert.Contains("process", denied);
        Assert.Contains("reflection", denied);
        Assert.Contains("threading", denied);
        Assert.Contains("time", denied);
        Assert.Contains("random", denied);
        Assert.Contains("ui", denied);
        Assert.Contains("unity", denied);
        Assert.Contains("runtime_mutation", denied);
        Assert.Contains("gamepackage_schema_mutation", denied);
        Assert.Contains("provider_llm", denied);
        Assert.Contains("rag", denied);
        Assert.Contains("media_generation", denied);
        Assert.Contains("native_interop", denied);
        Assert.Contains(matrix.Bindings, item => item.HostApiGroupId == "semantic.read" && item.BindingDecision == "allowed_in_dry_run");
        Assert.Contains(matrix.Bindings, item => item.HostApiGroupId == "metamodule.expand" && item.BindingDecision == "needs_explicit_adapter");
    }
}
