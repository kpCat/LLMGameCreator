using System.Security.Cryptography;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161R;

public sealed class Goal161RPublicationRollbackTests
{
    [Fact]
    public void Behavioral_green_publish_moves_staging_to_current_only_after_validation()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder, "green");
        var validatorSawCurrent = false;
        root.Locations.Publish(location, folder =>
        {
            validatorSawCurrent = Directory.Exists(folder) && File.ReadAllText(Path.Combine(folder, "g.exe")) == "green";
        });
        Assert.True(validatorSawCurrent);
        Assert.True(Directory.Exists(location.CurrentOutputFolder));
        Assert.False(Directory.Exists(location.StagingOutputFolder));
    }

    [Fact]
    public void Behavioral_green_publish_replaces_same_deterministic_current_folder()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.CurrentOutputFolder, "old");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder, "new");
        root.Locations.Publish(location, _ => { });
        Assert.Equal("new", File.ReadAllText(Path.Combine(location.CurrentOutputFolder, "g.exe")));
        Assert.False(Directory.Exists(location.BackupOutputFolder));
    }

    [Fact]
    public void Behavioral_publish_validation_failure_restores_prior_output_byte_identically()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.CurrentOutputFolder, "old");
        var before = HashDirectory(location.CurrentOutputFolder);
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder, "new");
        Assert.Throws<InvalidOperationException>(() => root.Locations.Publish(location, _ => throw new InvalidOperationException("injected publish failure")));
        Assert.Equal(before, HashDirectory(location.CurrentOutputFolder));
        Assert.False(Directory.Exists(location.StagingOutputFolder));
        Assert.False(Directory.Exists(location.BackupOutputFolder));
    }

    [Fact]
    public void Behavioral_first_publish_validation_failure_leaves_no_partial_current_output()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder, "new");
        Assert.Throws<InvalidOperationException>(() => root.Locations.Publish(location, _ => throw new InvalidOperationException("injected publish failure")));
        Assert.False(Directory.Exists(location.CurrentOutputFolder));
        Assert.False(Directory.Exists(location.StagingOutputFolder));
    }

    [Fact]
    public void Behavioral_old_project_local_builds_are_not_part_of_operational_publish()
    {
        using var root = new Goal161RTempRoot();
        var project = Path.Combine(root.Path, "project");
        var old = Path.Combine(project, "Builds", "Windows", "historical", "g.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(old)!);
        File.WriteAllText(old, "historical");
        var location = root.Locations.Resolve(project, "package", "a1b2c3d4e5f6");
        Goal161RTempRoot.WritePlayerSet(location.StagingOutputFolder, "new");
        root.Locations.Publish(location, _ => { });
        Assert.Equal("historical", File.ReadAllText(old));
        Assert.DoesNotContain(Path.Combine("Builds", "Windows"), location.CurrentOutputFolder, StringComparison.OrdinalIgnoreCase);
    }

    private static string HashDirectory(string directory)
    {
        var text = string.Join("\n", Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => Path.GetRelativePath(directory, path) + ":" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))));
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)));
    }
}
