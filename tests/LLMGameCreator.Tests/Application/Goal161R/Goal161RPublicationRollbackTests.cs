using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161R;

public sealed class Goal161RPublicationRollbackTests
{
    [Fact]
    public void Behavioral_goal161r_historical_output_paths_remain_readable_without_operational_publish()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.CurrentOutputFolder, "historical");
        Assert.Equal("historical", File.ReadAllText(Path.Combine(location.CurrentOutputFolder, "g.exe")));
        Assert.False(File.Exists(location.CurrentPointerPath));
    }

    [Fact]
    public void Behavioral_current_pointer_path_is_not_a_current_directory()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Assert.EndsWith("current.json", location.CurrentPointerPath, StringComparison.Ordinal);
        Assert.NotEqual(location.CurrentOutputFolder, location.RunOutputFolder);
    }
}
