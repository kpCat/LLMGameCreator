using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161R;

public sealed class Goal161RPathBudgetTests
{
    [Fact]
    public void Behavioral_reproduced_old_project_local_model_path_reaches_legacy_boundary()
    {
        var longProject = Path.Combine(Path.GetTempPath(), new string('p', 170));
        var old = Path.Combine(longProject, "Builds", "Windows", "all-selectable-migrated-project",
            "all-selectable-migrated-project_Data", "StreamingAssets", "LLMGameCreatorProject", "player-adapter-model.json");
        Assert.True(old.Length >= 260, old.Length.ToString());
    }

    [Fact]
    public void Behavioral_short_staging_player_paths_fit_budget()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder);
        var result = root.Locations.ValidatePlayerPathBudget(location.StagingOutputFolder,
            Path.Combine(root.Path, "m.log"), Path.Combine(root.Path, "p.log"));
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.MaximumAbsolutePathLength <= 240);
    }

    [Fact]
    public void Behavioral_short_final_player_paths_fit_budget()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.CurrentOutputFolder);
        var result = root.Locations.ValidatePlayerPathBudget(location.CurrentOutputFolder,
            Path.Combine(root.Path, "m.log"), Path.Combine(root.Path, "p.log"));
        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.MaximumAbsolutePathLength <= 240);
    }

    [Fact]
    public void Behavioral_budget_failure_reports_relative_path_and_length()
    {
        using var root = new Goal161RTempRoot();
        var output = Path.Combine(root.Path, new string('x', 230));
        Goal161RTempRoot.WritePlayerSet(output);
        var result = root.Locations.ValidatePlayerPathBudget(output,
            Path.Combine(root.Path, "m.log"), Path.Combine(root.Path, "p.log"));
        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.StartsWith("standalone.output.player_path_budget_exceeded:g_Data/StreamingAssets/LLMGameCreatorProject/", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_budget_includes_smoke_marker_and_player_log_paths()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder);
        var tooLong = Path.Combine(root.Path, new string('l', 240) + ".log");
        var result = root.Locations.ValidatePlayerPathBudget(location.StagingOutputFolder, tooLong, tooLong);
        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.Contains("smoke-marker/", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("player-log/", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_budget_result_exposes_maximum_path_and_limit()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder);
        var result = root.Locations.ValidatePlayerPathBudget(location.StagingOutputFolder,
            Path.Combine(root.Path, "m.log"), Path.Combine(root.Path, "p.log"));
        Assert.Equal(240, result.BudgetLimit);
        Assert.NotEmpty(result.LongestRelativePath);
        Assert.True(result.MaximumAbsolutePathLength > 0);
    }
}
