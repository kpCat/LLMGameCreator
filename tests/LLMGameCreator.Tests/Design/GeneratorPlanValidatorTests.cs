using LLMGameCreator.Application.Design;
using Xunit;

namespace LLMGameCreator.Tests.Design;

public sealed class GeneratorPlanValidatorTests
{
    private readonly GeneratorPlanValidator _validator = new();

    [Fact]
    public void ValidatorAcceptsMinimalPlanUsingImportedModule()
    {
        var issues = _validator.Validate(
            new GeneratorPlanDraft
            {
                Title = "Plan",
                Goal = "Goal",
                Steps = { new GeneratorPlanDraftStep { Order = 1, ModuleId = "core/base/v1", ConfigJson = "{}" } }
            },
            new[] { Module("core/base/v1") },
            new GeneratorPlanDraftRequest("Plan", "Goal", "Brief"),
            "{\"title\":\"Plan\",\"goal\":\"Goal\",\"steps\":[{\"order\":1,\"module_id\":\"core/base/v1\",\"config\":{},\"depends_on\":[]}]}");

        Assert.DoesNotContain(issues, issue => issue.Severity == "error");
    }

    [Fact]
    public void ValidatorRejectsUnknownModuleId()
    {
        var issues = _validator.Validate(
            new GeneratorPlanDraft { Title = "Plan", Goal = "Goal", Steps = { new GeneratorPlanDraftStep { Order = 1, ModuleId = "missing/v1", ConfigJson = "{}" } } },
            new[] { Module("core/base/v1") },
            new GeneratorPlanDraftRequest("Plan", "Goal", "Brief"));

        Assert.Contains(issues, issue => issue.Code == "plan.module_id.unknown");
    }

    [Fact]
    public void ValidatorRejectsMissingRequiredDependency()
    {
        var issues = _validator.Validate(
            new GeneratorPlanDraft { Title = "Plan", Goal = "Goal", Steps = { new GeneratorPlanDraftStep { Order = 1, ModuleId = "world/map/v1", ConfigJson = "{}" } } },
            new[] { Module("core/base/v1"), Module("world/map/v1", dependenciesJson: "[\"core/base/v1\"]") },
            new GeneratorPlanDraftRequest("Plan", "Goal", "Brief"));

        Assert.Contains(issues, issue => issue.Code == "plan.dependency.missing");
    }

    [Fact]
    public void ValidatorRejectsRawExecutionCodeFields()
    {
        var issues = _validator.Validate(
            new GeneratorPlanDraft { Title = "Plan", Goal = "Goal", Steps = { new GeneratorPlanDraftStep { Order = 1, ModuleId = "core/base/v1", ConfigJson = "{}" } } },
            new[] { Module("core/base/v1") },
            new GeneratorPlanDraftRequest("Plan", "Goal", "Brief"),
            "{\"title\":\"Plan\",\"goal\":\"Goal\",\"steps\":[{\"order\":1,\"module_id\":\"core/base/v1\",\"config\":{\"lua\":\"return {}\"},\"depends_on\":[]}]}");

        Assert.Contains(issues, issue => issue.Code == "plan.execution_field.forbidden");
    }

    [Fact]
    public void ValidatorRejectsInvalidConfigJson()
    {
        var issues = _validator.Validate(
            new GeneratorPlanDraft { Title = "Plan", Goal = "Goal", Steps = { new GeneratorPlanDraftStep { Order = 1, ModuleId = "core/base/v1", ConfigJson = "{ invalid" } } },
            new[] { Module("core/base/v1") },
            new GeneratorPlanDraftRequest("Plan", "Goal", "Brief"));

        Assert.Contains(issues, issue => issue.Code == "plan.config.invalid_json");
    }

    private static GeneratorModuleRecord Module(string id, string dependenciesJson = "[]")
    {
        return new GeneratorModuleRecord(id, "001", "lua/core/base.lua", "core", "[\"core.base\"]", dependenciesJson, "[]", "[]", "[]", "manifests/test.manifest.json", "{}", DateTimeOffset.UtcNow);
    }
}
