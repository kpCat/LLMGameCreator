using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal156;

[Collection(Goal156Collection.Name)]
public sealed class Goal156GeneratedWorkspaceTests
{
    private static readonly Lazy<Goal156WorkspaceFixture> Built = new(Goal156WorkspaceFixture.Create);

    [Fact]
    public void Behavioral_unbuilt_generated_project_has_a_typed_source_ready_summary()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.DifferentSeed, "source-ready");
        var snapshot = Goal156TestKit.OpenWorkspace(copy.Path).Snapshot();

        Assert.NotNull(snapshot.GeneratedWorld);
        Assert.Equal("SOURCE_READY", snapshot.GeneratedWorld.Status);
        Assert.True(snapshot.GeneratedWorld.Passed);
        Assert.False(snapshot.GeneratedWorld.PackageContentPreserved);
        Assert.NotEmpty(snapshot.GeneratedWorld.HumanFacts);
    }

    [Fact]
    public void Behavioral_all_selectable_generated_build_is_green_and_preserves_generated_content()
    {
        var build = Built.Value.FirstAllSelectableBuild;

        Assert.True(build.Passed, string.Join(Environment.NewLine, build.Diagnostics));
        Assert.Equal("GREEN", build.Status);
        Assert.True(build.CheckpointReloadPassed);
        Assert.True(build.FullReplayEquivalent);
        Assert.True(build.ActionBindingPassed);
        Assert.True(build.PackageActivated);
        Assert.NotNull(build.GeneratedWorld);
        Assert.Equal("BUILD_CURRENT", build.GeneratedWorld.Status);
        Assert.True(build.GeneratedWorld.PackageContentPreserved);
    }

    [Fact]
    public void Behavioral_all_selectable_generated_build_produces_a_passed_accepted_mechanics_summary()
    {
        var build = Built.Value.FirstAllSelectableBuild;

        Assert.NotNull(build.AcceptedMechanics);
        Assert.True(build.AcceptedMechanics.Passed, string.Join(Environment.NewLine, build.AcceptedMechanics.Diagnostics));
        Assert.Equal(build.SelectedMechanicCount, build.AcceptedMechanics.SelectedMechanicCount);
        Assert.True(build.AcceptedMechanics.CheckpointReloadPassed);
        Assert.True(build.AcceptedMechanics.FullReplayEquivalent);
        Assert.True(build.AcceptedMechanics.ActionBindingPassed);
    }

    [Fact]
    public void Behavioral_repeat_build_is_stable_for_package_runtime_and_generated_summary()
    {
        var fixture = Built.Value;

        Assert.True(fixture.SecondAllSelectableBuild.Passed);
        Assert.Equal(fixture.FirstAllSelectableBuild.PackageSha256, fixture.SecondAllSelectableBuild.PackageSha256);
        Assert.Equal(fixture.FirstAllSelectableBuild.CompositionPackageSha256,
            fixture.SecondAllSelectableBuild.CompositionPackageSha256);
        Assert.Equal(fixture.FirstAllSelectableBuild.FinalStateHash, fixture.SecondAllSelectableBuild.FinalStateHash);
        Assert.Equal(JsonSerializer.Serialize(fixture.FirstAllSelectableBuild.GeneratedWorld),
            JsonSerializer.Serialize(fixture.SecondAllSelectableBuild.GeneratedWorld));
    }

    [Fact]
    public void Behavioral_reopen_restores_generated_and_accepted_summaries_without_execution()
    {
        var fixture = Built.Value;
        var unityBefore = System.Diagnostics.Process.GetProcessesByName("Unity").Length;
        var snapshot = Goal156TestKit.OpenWorkspace(fixture.AllSelectableProject).Snapshot();

        Assert.Equal(unityBefore, System.Diagnostics.Process.GetProcessesByName("Unity").Length);
        Assert.Equal("START_CURRENT", snapshot.GeneratedWorld?.Status);
        Assert.True(snapshot.GeneratedWorld?.PackageContentPreserved);
        Assert.True(snapshot.AcceptedMechanics?.Passed);
        Assert.Equal(fixture.SecondAllSelectableBuild.PackageSha256, snapshot.PackageSha256);
        Assert.Equal(fixture.SecondAllSelectableBuild.FinalStateHash, snapshot.FinalStateHash);
    }

    [Fact]
    public void Behavioral_green_history_persists_the_typed_generated_world_summary()
    {
        var fixture = Built.Value;
        using var history = JsonDocument.Parse(File.ReadAllText(fixture.SecondAllSelectableBuild.BuildHistoryPath));
        var generated = history.RootElement.GetProperty("generatedWorld");

        Assert.Equal("BUILD_CURRENT", generated.GetProperty("status").GetString());
        Assert.True(generated.GetProperty("passed").GetBoolean());
        Assert.True(generated.GetProperty("packageContentPreserved").GetBoolean());
        Assert.Equal(Goal156TestKit.AllSelectable.Request.GenerationSeed, generated.GetProperty("seed").GetString());
    }

    [Fact]
    public void Behavioral_core_only_generated_project_builds_green_without_claiming_rc_ready()
    {
        var fixture = Built.Value;

        Assert.True(fixture.CoreBuild.Passed, string.Join(Environment.NewLine, fixture.CoreBuild.Diagnostics));
        Assert.Equal(0, fixture.CoreBuild.ConfiguredParameterCount);
        Assert.NotNull(fixture.CoreBuild.GeneratedWorld);
        Assert.Equal("BUILD_CURRENT", fixture.CoreBuild.GeneratedWorld.Status);
        Assert.NotEqual("CURRENT", fixture.CoreSnapshot.ReleaseCandidateConfigurationStatus);
        Assert.Null(fixture.CoreSnapshot.ReleaseCandidate);
    }

    [Fact]
    public void Behavioral_corrupt_generated_source_blocks_build_and_preserves_current_package()
    {
        using var copy = Goal156TestKit.Copy(Goal156TestKit.AllSelectable, "workspace-tamper");
        var packagePath = Path.Combine(copy.Path, "package.json");
        var before = File.ReadAllBytes(packagePath);
        var sidecar = Path.Combine(copy.Path, ".llmgc", "generation",
            SeededGeneratedProjectVocabulary.GeneratedBasePackageJsonFileName);
        File.AppendAllText(sidecar, " ");
        var controller = Goal156TestKit.OpenWorkspace(copy.Path);

        var failed = controller.BuildAndQualify();

        Assert.False(failed.Passed);
        Assert.Equal("generated_source.validation", failed.FailureStage);
        Assert.True(failed.RollbackApplied);
        Assert.Equal(before, File.ReadAllBytes(packagePath));
    }
}

internal sealed record Goal156WorkspaceFixture(
    string AllSelectableProject,
    GameProjectBuildResult FirstAllSelectableBuild,
    GameProjectBuildResult SecondAllSelectableBuild,
    GameProjectBuildResult CoreBuild,
    UnifiedGameProjectWorkspaceSnapshot CoreSnapshot)
{
    public static Goal156WorkspaceFixture Create()
    {
        var all = Goal156TestKit.OpenWorkspace(Goal156TestKit.AllSelectable.Path);
        var first = all.BuildAndQualify();
        var second = Goal156TestKit.OpenWorkspace(Goal156TestKit.AllSelectable.Path).BuildAndQualify();
        var core = Goal156TestKit.OpenWorkspace(Goal156TestKit.CoreOnly.Path);
        var coreBuild = core.BuildAndQualify();
        return new Goal156WorkspaceFixture(
            Goal156TestKit.AllSelectable.Path,
            first,
            second,
            coreBuild,
            core.Snapshot());
    }
}
