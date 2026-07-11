using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.FeatureModuleAuthoring;

public sealed class FeatureModuleParameterizedCompositionTests
{
    [Fact]
    public void Seeded_defaults_preserve_goal146_selected_hashes()
    {
        var root = FeatureModuleLibraryAndParameterTests.FindRoot();
        var library = FeatureModuleLibraryAndParameterTests.Load();
        var workspace = Temp("default-workspace");
        var output = Temp("default-output");
        try
        {
            var persistence = new FeatureModuleCompositionPersistenceService(workspace, new FixedClock());
            var document = persistence.CreateNew(
                "minimal-map-game-composed-alchemy-combat-exploration",
                "Alchemy + Combat + Exploration Resource FeatureModule Composition",
                "Seeded defaults",
                library);
            var result = new FeatureModuleParameterizedCompositionService(
                    SelectedRuntimeVariantInteractiveSessionService.CreateDefault())
                .MaterializeAndQualify(root, library, document, output);

            Assert.True(result.Passed);
            Assert.Equal("9a83d47e8e2ae541e7789b804c32f489acb8e7525c0a9dc32a7cc8be8822d65a", result.PackageSha256);
            Assert.Equal("d5ad29ee7c350918681c2859b80f5d2944834a6414918a16d8b4e1c0746753b9", result.FinalStateHash);
            Assert.Equal(3, result.SatisfiedSelectedModuleCount);
            Assert.Equal(3, result.EffectObservationCount);
        }
        finally { Delete(workspace); Delete(output); }
    }

    [Fact]
    public void Custom_values_span_all_modules_and_materialize_deterministically()
    {
        var root = FeatureModuleLibraryAndParameterTests.FindRoot();
        var library = FeatureModuleLibraryAndParameterTests.Load();
        var workspace = Temp("custom-workspace");
        var firstOutput = Temp("custom-output-a");
        var secondOutput = Temp("custom-output-b");
        try
        {
            var persistence = new FeatureModuleCompositionPersistenceService(workspace, new FixedClock());
            var document = persistence.CreateNew("goal147-custom-all-three", "Goal147 Custom", "Custom", library) with
            {
                ParameterValues =
                [
                    FeatureModuleLibraryAndParameterTests.Value("feature.profile.alchemy_focus", "healingPotionOutput", 3),
                    FeatureModuleLibraryAndParameterTests.Value("feature.profile.combat_focus", "goblinStartingHealth", 18),
                    FeatureModuleLibraryAndParameterTests.Value("feature.profile.combat_focus", "basicAttackDamage", 5),
                    FeatureModuleLibraryAndParameterTests.Value("feature.profile.exploration_resource_focus", "appleYield", 4),
                    FeatureModuleLibraryAndParameterTests.Value("feature.profile.exploration_resource_focus", "logYield", 3),
                    FeatureModuleLibraryAndParameterTests.Value("feature.profile.exploration_resource_focus", "transactionPotionOutput", 3)
                ]
            };
            var service = new FeatureModuleParameterizedCompositionService(
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault());
            var first = service.MaterializeAndQualify(root, library, document, firstOutput);
            var second = service.MaterializeAndQualify(root, library,
                document with { ParameterValues = document.ParameterValues.Reverse().ToList() }, secondOutput);

            Assert.True(first.Passed);
            Assert.NotEqual("9a83d47e8e2ae541e7789b804c32f489acb8e7525c0a9dc32a7cc8be8822d65a", first.PackageSha256);
            Assert.Equal(first.PackageSha256, second.PackageSha256);
            Assert.Equal(first.FinalStateHash, second.FinalStateHash);
            Assert.Equal(first.PackageJson, second.PackageJson);
            Assert.Equal(3, first.SatisfiedSelectedModuleCount);
            Assert.True(first.CheckpointReloadPassed);
            Assert.True(first.FullReplayEquivalent);
            Assert.True(first.ActionBindingPassed);
        }
        finally { Delete(workspace); Delete(firstOutput); Delete(secondOutput); }
    }

    private static string Temp(string name) => Path.Combine(Path.GetTempPath(), "LLMGameCreator", name + "-" + Guid.NewGuid().ToString("N"));
    private static void Delete(string path) { if (Directory.Exists(path)) Directory.Delete(path, true); }

    private sealed class FixedClock : IFeatureModuleAuthoringClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 11, 9, 0, 0, TimeSpan.Zero);
    }
}
