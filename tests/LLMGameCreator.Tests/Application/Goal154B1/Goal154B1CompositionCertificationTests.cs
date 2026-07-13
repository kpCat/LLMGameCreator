using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleCertification;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Tests.Application.Goal154B;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154B1;

public sealed class Goal154B1CompositionCertificationTests
{
    [Fact]
    public void Behavioral_module_order_keeps_package_and_plan_byte_identical()
    {
        var normal = Goal154BFixture.Create();
        var reverse = Goal154BFixture.CreateSelected(Goal154BFixture.DialogueModuleId,
            Goal154BFixture.QuestModuleId, Goal154BFixture.FactionModuleId);

        Assert.Equal(normal.PackageJson, reverse.PackageJson);
        Assert.Equal(normal.Plan.ActionPlanSignature, reverse.Plan.ActionPlanSignature);
    }

    [Fact]
    public void Behavioral_default_off_package_and_initial_state_hashes_remain_unchanged()
    {
        var root = Goal154BFixture.FindRoot();
        var library = new FeatureModuleLibraryLoader().Load(Path.Combine(root, "catalogs", "feature-modules"));
        var selected = library.Catalog.Modules.Where(module => module.Required).Select(module => module.ModuleId).ToList();
        var binding = new FeatureModuleParameterBindingService().Bind(library.Catalog, selected, []);
        var basePackage = Goal154BFixture.ReadBasePackage();
        var mutated = new FeatureModulePackageMutationService().Apply(basePackage, binding.EffectiveMutationOperations);
        var first = new LLMGameCreator.Runtime.GameRuntimeStateFactory().CreateInitialState(Goal154BFixture.Deserialize(mutated.PackageJson)).State;
        var second = new LLMGameCreator.Runtime.GameRuntimeStateFactory().CreateInitialState(Goal154BFixture.Deserialize(basePackage)).State;

        Assert.True(binding.Passed, string.Join(";", binding.Diagnostics));
        Assert.True(mutated.Passed, string.Join(";", mutated.Diagnostics));
        Assert.Equal(Goal154BFixture.Hash(basePackage), Goal154BFixture.Hash(mutated.PackageJson));
        Assert.Equal(Goal154BFixture.Hash(Goal154BFixture.Stable(first)), Goal154BFixture.Hash(Goal154BFixture.Stable(second)));
    }

    [Fact]
    public void Behavioral_module_version_invalidates_owner_and_dialogue_dependent_only()
    {
        var library = Load();
        var changedQuest = library.Catalog.Modules.Single(module => module.ModuleId == Goal154BFixture.QuestModuleId) with
        {
            ModuleVersion = "1.2.1"
        };
        var changedCatalog = library.Catalog with
        {
            Modules = library.Catalog.Modules.Select(module => module.ModuleId == changedQuest.ModuleId ? changedQuest : module).ToList()
        };
        var changed = Rebuild(library, changedCatalog);
        var planner = new FeatureModuleCertificationPlanner();
        var baselineSha = Goal154BFixture.Hash(Goal154BFixture.ReadBasePackage());
        var before = planner.Plan(library, baselineSha, FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion,
            string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan));
        var after = planner.Plan(changed, baselineSha, FeatureModuleCertificationVocabulary.RuntimeQualifierContractVersion,
            string.Join("|", ProductLineRuntimeQualifier.CanonicalActionPlan));
        var invalidated = before.Modules.Join(after.Modules, item => item.ModuleId, item => item.ModuleId,
                (left, right) => new { left.ModuleId, BeforeFingerprint = left.CacheKey,
                    AfterFingerprint = right.CacheKey })
            .Where(item => item.BeforeFingerprint != item.AfterFingerprint)
            .Select(item => item.ModuleId).OrderBy(item => item, StringComparer.Ordinal).ToList();

        Assert.Equal([Goal154BFixture.DialogueModuleId, Goal154BFixture.QuestModuleId], invalidated);
    }

    [Fact]
    public void Behavioral_reputation_only_module_rejects_an_unbacked_non_reputation_quest_reward_mutation()
    {
        var library = Load();
        var quest = library.Catalog.Modules.Single(module => module.ModuleId == Goal154BFixture.QuestModuleId);
        var goldMutation = quest.MutationOperations.Single(item => item.OperationId == "quest.00_completion_reputation") with
        {
            OperationId = "quest.injected_gold_reward",
            TargetId = "quest/help_healer|rewards|resource|resource/gold",
            JsonPath = "game.quests[id=quest/help_healer].rewards[kind=resource,id=resource/gold].amount",
            RuntimeDimension = "quest_injected_gold_reward"
        };
        var invalid = quest with
        {
            MutationOperations = quest.MutationOperations.Append(goldMutation).ToList(),
            RequiredValidationRules = quest.RequiredValidationRules.Append(
                "activated_package_diff:quest_injected_gold_reward:declared_user_facing_mechanic").ToList()
        };
        var diagnostics = new List<string>();

        Assert.False(FeatureModuleLibraryValidator.ValidateActivatedPackageDiffClaims(invalid, diagnostics));
        Assert.Contains(diagnostics, item => item.Contains("requires a declared user-facing capability", StringComparison.Ordinal));
    }

    private static FeatureModuleLibrarySnapshot Load() => new FeatureModuleLibraryLoader().Load(
        Path.Combine(Goal154BFixture.FindRoot(), "catalogs", "feature-modules"));

    private static FeatureModuleLibrarySnapshot Rebuild(
        FeatureModuleLibrarySnapshot source,
        FeatureModuleCatalogDocument catalog)
    {
        var fingerprints = new FeatureModuleLibraryFingerprintService();
        var moduleFingerprints = catalog.Modules.ToDictionary(module => module.ModuleId,
            fingerprints.ModuleFingerprint, StringComparer.Ordinal);
        return source with
        {
            Catalog = catalog,
            ModuleFingerprints = moduleFingerprints,
            CatalogFingerprint = fingerprints.CatalogFingerprint(moduleFingerprints)
        };
    }
}
