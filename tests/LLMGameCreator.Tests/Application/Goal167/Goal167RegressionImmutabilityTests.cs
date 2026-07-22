using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167RegressionImmutabilityTests
{
    [Fact]
    public void Behavioral_all_selectable_build_keeps_generation_source_and_sidecars_byte_identical()
    {
        AssertSidecars(Goal164TestKit.AllSelectable);
    }

    [Fact]
    public void Behavioral_core_only_build_keeps_generation_source_and_sidecars_byte_identical()
    {
        AssertSidecars(Goal164TestKit.CoreOnly);
    }

    [Fact]
    public void Behavioral_world_rollback_restores_original_generation_sidecars_byte_identically()
    {
        var state = Goal164RegenerationState.Value;
        var generationRoot = Path.Combine(state.Build.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));

        Assert.All(state.Build.GenerationSidecarHashesBefore, pair =>
            Assert.Equal(pair.Value, Goal164TestKit.FileSha(Path.Combine(generationRoot, pair.Key))));
    }

    [Fact]
    public void Behavioral_choice_overlay_changes_final_package_without_rewriting_strict_source_authority()
    {
        var build = Goal164TestKit.AllSelectable;
        var current = build.SourceService.Validate(build.Project.Path);

        Assert.True(current.Passed, string.Join(",", current.Diagnostics));
        Assert.Equal(build.Source.Source?.PlanSha256, current.Source?.PlanSha256);
        Assert.Equal(build.Source.Source?.GeneratedOverlaySha256, current.Source?.GeneratedOverlaySha256);
        Assert.Equal(build.Source.Source?.GeneratedBasePackageSha256,
            current.Source?.GeneratedBasePackageSha256);
        Assert.NotEqual(current.Source?.GeneratedBasePackageSha256, build.Build.PackageSha256);
    }

    private static void AssertSidecars(Goal164BuildFixture build)
    {
        var generationRoot = Path.Combine(build.Project.Path,
            SeededGeneratedProjectVocabulary.GenerationRelativeRoot
                .Replace('/', Path.DirectorySeparatorChar));
        Assert.All(build.GenerationSidecarHashesBefore, pair =>
            Assert.Equal(pair.Value, Goal164TestKit.FileSha(Path.Combine(generationRoot, pair.Key))));
    }
}
