using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleComposition;

public sealed class FeatureModuleCompositionTests
{
    [Fact]
    public void Catalog_has_locked_core_and_imported_goal142_optional_module_lineage()
    {
        var root = FindRoot();
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, FeatureModuleCompositionVocabulary.Goal142Root);

        Assert.Equal(10, catalog.RequiredCoreModuleCount);
        Assert.Equal(3, catalog.OptionalProfileModuleCount);
        Assert.All(catalog.Modules.Where(module => module.Required), module => Assert.False(module.Selectable));
        Assert.All(catalog.Modules.Where(module => module.Selectable), module =>
        {
            Assert.StartsWith("feature.profile.", module.ModuleId, StringComparison.Ordinal);
            Assert.NotEmpty(module.MutationOperations);
            Assert.NotEmpty(module.SourceLineage.RecipeId);
            Assert.Equal(module.MutationOperations.Count, module.SourceLineage.OperationIds.Count);
        });
    }

    [Fact]
    public void Validator_rejects_unknown_duplicate_missing_required_dependency_conflict_and_override()
    {
        var root = FindRoot();
        var catalog = FeatureModuleCatalog.LoadFromGoal142(root, FeatureModuleCompositionVocabulary.Goal142Root);
        var validator = new FeatureModuleCompositionValidator();
        var required = catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        var alchemy = FeatureModuleCompositionVocabulary.OptionalModuleIds[0];
        var combat = FeatureModuleCompositionVocabulary.OptionalModuleIds[1];

        Assert.False(validator.Validate(catalog, required.Append("unknown").ToList()).AllModuleIdsExist);
        Assert.False(validator.Validate(catalog, required.Concat([alchemy, alchemy]).ToList()).ModuleIdsUnique);
        Assert.False(validator.Validate(catalog, required.Skip(1).ToList()).RequiredModulesSelected);
        Assert.False(validator.Validate(catalog,
            required.Where(id => id != "feature.crafting.recipes").Append(alchemy).ToList()).DependenciesSatisfied);
        var conflictCatalog = catalog with
        {
            Modules = catalog.Modules.Select(module => module.ModuleId == alchemy
                ? module with { Conflicts = [combat] }
                : module).ToList()
        };
        Assert.False(validator.Validate(conflictCatalog, required.Concat([alchemy, combat]).ToList()).ConflictsAbsent);
        Assert.False(validator.Validate(catalog, required,
            new Dictionary<string, string> { ["unsupported"] = "1" }).ParameterOverridesSupported);
    }

    [Fact]
    public async Task Service_materializes_and_qualifies_all_eight_novel_compositions()
    {
        var root = FindRoot();
        var service = new FeatureModuleCompositionService(SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
        var result = await service.RunAndWriteAsync(root);

        Assert.Equal(8, result.Matrix.CompositionCount);
        Assert.Equal(8, result.Matrix.PassedCompositionCount);
        Assert.Equal(8, result.Matrix.DistinctPackageSha256Count);
        Assert.Equal(8, result.Matrix.DistinctFinalStateHashCount);
        Assert.Equal(4, result.Matrix.MultiModuleCompositionCount);
        Assert.True(result.Matrix.AllOrderIndependenceProofsPassed);
        Assert.True(result.Matrix.AllCheckpointReloadsPassed);
        Assert.True(result.Matrix.AllFullReplaysEquivalent);
        Assert.True(result.Matrix.AllActionBindingsPassed);
        Assert.True(result.Matrix.SameRuntimeQualifierUsedForGoal145AndGoal146);
        Assert.Equal(3, result.Selection.SelectedOptionalModuleIds.Count);
        Assert.Equal(3, result.Selection.SemanticEffects.Count);
        Assert.False(result.Dashboard.Accepted);
        Assert.True(result.Dashboard.ManualReviewDeferred);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }
}
