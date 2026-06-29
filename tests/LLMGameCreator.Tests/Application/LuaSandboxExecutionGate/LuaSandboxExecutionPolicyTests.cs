using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaSandboxExecutionGate;

public sealed class LuaSandboxExecutionPolicyTests
{
    [Fact]
    public void PolicySeedValidatesCleanlyAndDeniesExecutionCapabilities()
    {
        var summary = LuaSandboxExecutionGateCatalog.BuildPolicySummary();

        Assert.DoesNotContain(summary.Diagnostics, item => item.Severity == "error");
        Assert.True(summary.NoLuaExecution);
        Assert.True(summary.NoLuaParser);
        Assert.True(summary.NoLuaSourceGeneration);
        Assert.False(summary.Policy.RealLuaExecutionAllowed);
        Assert.False(summary.Policy.LuaParserAllowed);
        Assert.False(summary.Policy.LuaSourceGenerationAllowed);
        Assert.Contains("validate_manifest_selection", summary.Policy.RequiredProbeStepIds);
        Assert.Contains("validate_host_bindings", summary.Policy.RequiredProbeStepIds);
        Assert.Contains("validate_budget", summary.Policy.RequiredProbeStepIds);
        Assert.Contains("validate_dependency_order", summary.Policy.RequiredProbeStepIds);
        Assert.Contains("validate_expected_outputs", summary.Policy.RequiredProbeStepIds);
    }
}
