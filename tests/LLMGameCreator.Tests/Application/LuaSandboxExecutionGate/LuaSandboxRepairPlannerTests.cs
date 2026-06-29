using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaSandboxExecutionGate;

public sealed class LuaSandboxRepairPlannerTests
{
    [Fact]
    public void RepairPlansAreDeterministicAndDoNotMutateAcceptedManifests()
    {
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var invalidRequests = LuaSandboxExecutionGateValidator.BuildInvalidRequestCases()
            .Select(item => item.Request)
            .OrderBy(item => item.RequestId, StringComparer.Ordinal)
            .ToList();
        var invalidDecisions = invalidRequests
            .Select(item => LuaSandboxExecutionGateValidator.Decide(item, manifests))
            .OrderBy(item => item.RequestId, StringComparer.Ordinal)
            .ToList();

        var first = new LuaSandboxRepairPlanner().BuildRepairPlanMatrix(invalidRequests, invalidDecisions);
        var second = new LuaSandboxRepairPlanner().BuildRepairPlanMatrix(invalidRequests, invalidDecisions);

        Assert.False(first.MutatesAcceptedManifests);
        Assert.Equal(first.RepairPlans.Select(item => item.RepairPlanId), second.RepairPlans.Select(item => item.RepairPlanId));
        Assert.Contains(first.RepairPlans.SelectMany(item => item.Actions), item => item.ActionKind == "remove-denied-host-api-group");
        Assert.Contains(first.RepairPlans.SelectMany(item => item.Actions), item => item.ActionKind == "reduce-budget");
        Assert.Contains(first.RepairPlans.SelectMany(item => item.Actions), item => item.ActionKind == "add-missing-budget");
        Assert.Contains(first.RepairPlans.SelectMany(item => item.Actions), item => item.ActionKind == "add-goal034-promotion-trace");
        Assert.Contains(first.RepairPlans.SelectMany(item => item.Actions), item => item.ActionKind == "replace-fake-manifest-id");
        Assert.All(first.RepairPlans.SelectMany(item => item.Actions), item => Assert.False(item.MutatesAcceptedManifest));
    }

    [Fact]
    public void BlockedNoExecutorProducesFutureAdapterRepairPlan()
    {
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var request = LuaSandboxExecutionGateCatalog.BuildDefaultRequests().Single(item => item.ScenarioId == "metamodule_kingdoms");
        var decision = LuaSandboxExecutionGateValidator.Decide(request, manifests);

        var plan = new LuaSandboxRepairPlanner().Plan(request, decision);

        Assert.Equal("blocked_no_executor", decision.DecisionStatus);
        Assert.Contains(plan.Actions, item => item.ActionKind == "mark-future-executor-adapter-required");
        Assert.False(plan.MutatesAcceptedManifests);
    }
}
