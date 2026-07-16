using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161R;

public sealed class Goal161ROutputLocationTests
{
    [Fact]
    public void Behavioral_default_output_root_is_local_appdata_lgc_o()
    {
        var service = new ProjectStandaloneOutputLocationService();
        Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LGC", "O"), service.Root);
    }

    [Fact]
    public void Behavioral_token_is_deterministic_for_same_project_and_package()
    {
        using var root = new Goal161RTempRoot();
        var service = root.Locations;
        var first = service.Resolve(Path.Combine(root.Path, "project"), "package.alpha", "a1b2c3d4e5f6");
        var second = service.Resolve(Path.Combine(root.Path, "project"), "package.alpha", "z9y8x7w6v5u4");
        Assert.Equal(first.ProjectToken, second.ProjectToken);
        Assert.Equal(first.CurrentOutputFolder, second.CurrentOutputFolder);
    }

    [Fact]
    public void Behavioral_different_project_paths_produce_different_tokens()
    {
        using var root = new Goal161RTempRoot();
        var first = root.Locations.Resolve(Path.Combine(root.Path, "one"), "package.alpha", "a1b2c3d4e5f6");
        var second = root.Locations.Resolve(Path.Combine(root.Path, "two"), "package.alpha", "a1b2c3d4e5f6");
        Assert.NotEqual(first.ProjectToken, second.ProjectToken);
    }

    [Fact]
    public void Behavioral_token_is_lowercase_hex_and_paths_are_confined()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "пакет.alpha", "a1b2c3d4e5f6");
        Assert.Matches("^[0-9a-f]{16}$", location.ProjectToken);
        Assert.StartsWith(root.OutputRoot + Path.DirectorySeparatorChar, location.CurrentOutputFolder,
            StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(root.OutputRoot + Path.DirectorySeparatorChar, location.StagingOutputFolder,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_staging_and_final_use_fixed_short_operational_names()
    {
        using var root = new Goal161RTempRoot();
        var location = root.Locations.Resolve(Path.Combine(root.Path, "project"), "package.alpha", "a1b2c3d4e5f6");
        Assert.Equal("current", Path.GetFileName(location.CurrentOutputFolder));
        Assert.StartsWith("s-a1b2c3d4e5f6", Path.GetFileName(location.StagingOutputFolder), StringComparison.Ordinal);
        Assert.Equal("g.exe", location.ExecutableName);
        Assert.Equal("g_Data", location.DataDirectoryName);
    }

    [Fact]
    public void Behavioral_root_traversal_is_rejected_with_causal_diagnostic()
    {
        using var root = new Goal161RTempRoot();
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ProjectStandaloneOutputLocationService(Path.Combine(root.Path, "safe", "..", "escape")));
        Assert.Equal("standalone.output.path_escape", exception.Message);
    }
}

internal sealed class Goal161RTempRoot : IDisposable
{
    public Goal161RTempRoot()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "llmgc-goal161r-" + Guid.NewGuid().ToString("N"));
        OutputRoot = System.IO.Path.Combine(Path, "o");
        Directory.CreateDirectory(Path);
        Locations = new ProjectStandaloneOutputLocationService(OutputRoot);
    }

    public string Path { get; }
    public string OutputRoot { get; }
    public ProjectStandaloneOutputLocationService Locations { get; }

    public static void WritePlayerSet(string output, string marker = "new")
    {
        Directory.CreateDirectory(System.IO.Path.Combine(output, "g_Data", "StreamingAssets", "LLMGameCreatorProject"));
        File.WriteAllText(System.IO.Path.Combine(output, "g.exe"), marker, new UTF8Encoding(false));
        File.WriteAllText(System.IO.Path.Combine(output, "UnityPlayer.dll"), marker, new UTF8Encoding(false));
        Directory.CreateDirectory(System.IO.Path.Combine(output, "MonoBleedingEdge"));
        File.WriteAllText(System.IO.Path.Combine(output, "build-manifest.json"), marker, new UTF8Encoding(false));
        foreach (var name in new[] { "project-manifest.json", "player-adapter-model.json", "player-adapter-frames.json", "standalone-launch.json", "game-package.json" })
            File.WriteAllText(System.IO.Path.Combine(output, "g_Data", "StreamingAssets", "LLMGameCreatorProject", name), marker, new UTF8Encoding(false));
    }

    public void Dispose()
    {
        if (Directory.Exists(Path)) Directory.Delete(Path, true);
    }
}
