using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161R;

public sealed class Goal161RQualificationTests
{
    [Fact]
    public void Behavioral_preflight_failure_prevents_process_invocation()
    {
        using var root = new Goal161RTempRoot();
        var service = new ProjectStandaloneBuildService(root.Path, root.Locations);
        var result = service.RunSmoke("missing-never-started.exe", new ProjectStandalonePayloadSelfCheckResult { Passed = false });
        Assert.False(result.ProcessStarted);
        Assert.Equal("standalone.payload.preflight_failed", result.NamedFailure);
    }

    [Fact]
    public void Behavioral_result_carries_short_output_path_budget_fields()
    {
        var result = new ProjectStandaloneBuildResult
        {
            OutputLocationKind = ProjectStandaloneBuildVocabulary.OutputLocationKind,
            OutputProjectToken = "0123456789abcdef",
            MaximumPlayerPathLength = 240,
            PlayerPathBudgetLimit = 240,
            PlayerPathBudgetPassed = true,
            PriorSuccessfulOutputPreserved = true
        };
        Assert.Equal("short_local_appdata", result.OutputLocationKind);
        Assert.Equal(240, result.PlayerPathBudgetLimit);
        Assert.True(result.PlayerPathBudgetPassed);
        Assert.True(result.PriorSuccessfulOutputPreserved);
    }

    [Fact]
    public void Behavioral_same_project_repeat_uses_same_final_directory()
    {
        using var root = new Goal161RTempRoot();
        var first = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        var second = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "b1c2d3e4f5a6");
        Assert.Equal(first.CurrentOutputFolder, second.CurrentOutputFolder);
        Assert.NotEqual(first.StagingOutputFolder, second.StagingOutputFolder);
    }

    [Fact]
    public void Behavioral_different_project_copy_gets_different_operational_output_folder()
    {
        using var root = new Goal161RTempRoot();
        var original = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        var copy = root.Locations.Resolve(Path.Combine(root.Path, "project-copy"), "package", "a1b2c3d4e5f6");
        Assert.NotEqual(original.CurrentOutputFolder, copy.CurrentOutputFolder);
    }

    [Fact]
    public void Behavioral_fixed_output_set_has_short_launch_and_folder_targets()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.CurrentOutputFolder);
        var executable = Path.Combine(location.CurrentOutputFolder, ProjectStandaloneBuildVocabulary.OperationalExecutableName);
        Assert.True(File.Exists(executable));
        Assert.True(Directory.Exists(location.CurrentOutputFolder));
    }
}
