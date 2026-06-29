using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaModuleManifestRegistry;

public sealed class LuaModuleManifestRegistryTests
{
    [Fact]
    public void SeedRegistryValidatesCleanlyAndKeepsDeniedHostApisOutOfAllowedSurface()
    {
        var families = LuaModuleManifestRegistryCatalog.BuildFamilies();
        var policy = LuaModuleManifestRegistryCatalog.BuildHostApiSurfacePolicy();
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();

        var diagnostics = LuaModuleManifestRegistryValidator.ValidateFamilies(families)
            .Concat(LuaModuleManifestRegistryValidator.ValidateHostApiSurface(policy.Groups))
            .Concat(LuaModuleManifestRegistryValidator.ValidateManifests(families, policy.Groups, manifests))
            .ToList();

        Assert.DoesNotContain(diagnostics, item => item.Severity == "error");
        Assert.Contains(policy.DeniedGroupIds, item => item == "filesystem");
        Assert.Contains(policy.DeniedGroupIds, item => item == "implicit_lua_execution");
        Assert.DoesNotContain(manifests.SelectMany(item => item.AllowedHostApiGroups), item => policy.DeniedGroupIds.Contains(item, StringComparer.Ordinal));
        Assert.Contains(manifests, item => item.SourceKind == "goal_034_quarantined_candidate" && item.PromotionStatus == "quarantined");
    }

    [Fact]
    public void MetamoduleScenarioUsesGeneratedSlotManifestsWithoutDuplicateModuleIds()
    {
        var manifests = LuaModuleManifestRegistryCatalog.BuildDefaultManifests();
        var slotManifests = manifests
            .Where(item => item.ModuleId.StartsWith("lua-module/metamodule/species-archetype-slot/", StringComparison.Ordinal))
            .ToList();

        Assert.True(slotManifests.Count >= 100);
        Assert.Equal(slotManifests.Count, slotManifests.Select(item => item.ModuleId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(slotManifests, item =>
        {
            Assert.Equal("metamodule_species_archetype_expansion_rules", item.FamilyId);
            Assert.Contains("metamodule.expand", item.AllowedHostApiGroups);
            Assert.False(item.DeclaresLuaSource);
            Assert.False(item.ClaimsLuaExecution);
        });
    }
}
