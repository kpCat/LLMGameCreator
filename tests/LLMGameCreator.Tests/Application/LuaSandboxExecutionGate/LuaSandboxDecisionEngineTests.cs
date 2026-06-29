using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaSandboxExecutionGate;

public sealed class LuaSandboxDecisionEngineTests
{
    [Fact]
    public void DefaultRequestsIntegrateGoal035ManifestSelectionsDeterministically()
    {
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var requests = LuaSandboxExecutionGateCatalog.BuildDefaultRequests();
        var decisions = requests
            .Select(item => LuaSandboxExecutionGateValidator.Decide(item, manifests))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(4, decisions.Count);
        Assert.All(decisions, item => Assert.False(item.LuaExecuted));
        Assert.Contains(decisions, item => item.ScenarioId == "frontier_survival" && item.DecisionStatus == "dry_run_only");
        Assert.Contains(decisions, item => item.ScenarioId == "gothic_intrigue" && item.DecisionStatus == "ready_for_future_executor");
        Assert.Contains(decisions, item => item.ScenarioId == "metamodule_kingdoms" && item.DecisionStatus == "blocked_no_executor");
        Assert.True(decisions.Single(item => item.ScenarioId == "metamodule_kingdoms").MetamoduleSpeciesArchetypeSlotManifestCount >= 100);

        var summaries = decisions.Select(item => item.StableSummary).ToArray();
        var repeated = requests
            .Select(item => LuaSandboxExecutionGateValidator.Decide(item, manifests))
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(item => item.StableSummary)
            .ToArray();
        Assert.Equal(summaries, repeated);
    }

    [Fact]
    public void InvalidFakeLeakMatrixReturnsCausalDiagnostics()
    {
        var matrix = LuaSandboxExecutionGateValidator.BuildInvalidMatrix();
        var codes = matrix.Scenarios
            .SelectMany(item => item.Diagnostics)
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.Equal(matrix.ScenarioCount, matrix.RejectedCount + matrix.NeedsRepairCount);
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "fake_manifest_id" && item.ActualStatus == "rejected");
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "missing_budget" && item.ActualStatus == "needs_repair");
        Assert.Contains("lua_sandbox.manifest_id.fake", codes);
        Assert.Contains("lua_sandbox.host_api.unknown", codes);
        Assert.Contains("lua_sandbox.host_api.denied", codes);
        Assert.Contains("lua_sandbox.budget.missing", codes);
        Assert.Contains("lua_sandbox.budget.over_limit", codes);
        Assert.Contains("lua_sandbox.dependency_order.unstable", codes);
        Assert.Contains("lua_sandbox.source_text.forbidden", codes);
        Assert.Contains("lua_sandbox.parser_claim.forbidden", codes);
        Assert.Contains("lua_sandbox.lua_execution_claim.forbidden", codes);
        Assert.Contains("lua_sandbox.final_prose.forbidden", codes);
        Assert.Contains("lua_sandbox.promotion.self_forbidden", codes);
        Assert.Contains("lua_sandbox.promotion_trace.missing", codes);
        Assert.Contains("lua_sandbox.host_api.boundary_blocked", codes);
        Assert.Contains("lua_sandbox.repair.immutable_manifest_mutation", codes);
        Assert.Contains("lua_sandbox.manifest_order.nondeterministic", codes);
    }
}
