using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaModuleManifestRegistry;

public sealed class LuaModuleManifestValidatorTests
{
    [Fact]
    public void InvalidMatrixCoversCausalFakeLeakAndBoundaryDiagnostics()
    {
        var matrix = LuaModuleManifestRegistryValidator.BuildInvalidMatrix();
        var codes = matrix.Scenarios
            .SelectMany(item => item.Diagnostics)
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.Equal(matrix.ScenarioCount, matrix.RejectedCount);
        Assert.Contains("lua_manifest.module_id.duplicate", codes);
        Assert.Contains("lua_manifest.module_id.invalid", codes);
        Assert.Contains("lua_manifest.family_id.duplicate_conflict", codes);
        Assert.Contains("lua_manifest.dependency.unknown", codes);
        Assert.Contains("lua_manifest.dependency.cycle", codes);
        Assert.Contains("lua_manifest.host_api.unknown", codes);
        Assert.Contains("lua_manifest.host_api.denied_allowed", codes);
        Assert.Contains("lua_manifest.semantic_scope.required_missing", codes);
        Assert.Contains("lua_manifest.artifact_contract.unknown", codes);
        Assert.Contains("lua_manifest.intent_family.unknown", codes);
        Assert.Contains("lua_manifest.scenario.fake", codes);
        Assert.Contains("lua_manifest.provenance.mismatch", codes);
        Assert.Contains("lua_manifest.candidate.ready_without_review", codes);
        Assert.Contains("lua_manifest.resource_budget.over_limit", codes);
        Assert.Contains("lua_manifest.future_required.treated_ready", codes);
        Assert.Contains("lua_manifest.side_effect.mismatch", codes);
        Assert.Contains("lua_manifest.final_prose.forbidden", codes);
        Assert.Contains("lua_manifest.lua_source_or_execution.forbidden", codes);
        Assert.Contains("lua_manifest.provider_llm_rag.leakage", codes);
        Assert.Contains("lua_manifest.runtime_ui_unity_gamepackage.leakage", codes);
        Assert.Contains("lua_manifest.order.nondeterministic", codes);
    }

    [Fact]
    public void DeniedHostApiGroupAsAllowedReturnsDiagnosticInsteadOfThrowing()
    {
        var families = LuaModuleManifestRegistryCatalog.BuildFamilies();
        var policy = LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy();
        var manifest = LuaModuleManifestRegistryCatalog.BuildDefaultManifests().First(item => item.LifecycleStatus == "ready") with
        {
            AllowedHostApiGroups = ["filesystem"]
        };

        var diagnostics = LuaModuleManifestRegistryValidator.ValidateManifests(families, policy.Groups, [manifest]);

        Assert.Contains(diagnostics, item => item.Code == "lua_manifest.host_api.denied_allowed");
    }
}
