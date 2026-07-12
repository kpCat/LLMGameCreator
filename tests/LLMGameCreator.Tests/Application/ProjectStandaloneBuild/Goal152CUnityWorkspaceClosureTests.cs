using LLMGameCreator.Application.Design.ProjectStandaloneBuild;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProjectStandaloneBuild;

public sealed class Goal152CUnityWorkspaceClosureTests
{
    [Fact]
    public void External_workspace_copies_required_source_without_generated_paths_or_repository_mutation()
    {
        var repository = CreateRepository();
        try
        {
            var service = new UnityHostBuildWorkspaceService(repository);
            var workspace = service.Prepare();
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Assert.StartsWith(Path.GetFullPath(local), Path.GetFullPath(workspace.ProjectPath), StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Path.GetFullPath(repository), Path.GetFullPath(workspace.ProjectPath), StringComparison.OrdinalIgnoreCase);
            Assert.True(Directory.Exists(Path.Combine(workspace.ProjectPath, "Assets")));
            Assert.True(Directory.Exists(Path.Combine(workspace.ProjectPath, "Packages")));
            Assert.True(Directory.Exists(Path.Combine(workspace.ProjectPath, "ProjectSettings")));
            Assert.False(File.Exists(Path.Combine(workspace.ProjectPath, "Assets", "StreamingAssets", "ignored.txt")));
            Assert.Equal(workspace.SourceBefore.Files, workspace.SourceAfter.Files);
            var command = service.CreateUnityArguments("C:\\temp\\host.exe", "C:\\temp\\unity.log", workspace.ProjectPath);
            Assert.Contains("-batchmode -nographics -quit", command, StringComparison.Ordinal);
            Assert.Contains("-projectPath \"" + workspace.ProjectPath + "\"", command, StringComparison.Ordinal);
            Assert.DoesNotContain(Path.Combine(repository, "unity", "LLMGameCreatorAlpha"), command, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (Directory.Exists(repository)) Directory.Delete(repository, true); }
    }

    [Fact]
    public void Workspace_argument_rejects_repository_project_path()
    {
        var repository = CreateRepository();
        try
        {
            var service = new UnityHostBuildWorkspaceService(repository);
            Assert.Throws<InvalidOperationException>(() => service.CreateUnityArguments("out.exe", "unity.log", Path.Combine(repository, "unity", "LLMGameCreatorAlpha")));
        }
        finally { if (Directory.Exists(repository)) Directory.Delete(repository, true); }
    }

    [Fact]
    public void Failed_preparation_preserves_the_prior_external_workspace()
    {
        var repository = CreateRepository();
        try
        {
            var service = new UnityHostBuildWorkspaceService(repository);
            var first = service.Prepare();
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            Assert.Throws<OperationCanceledException>(() => service.Prepare(cancelled.Token));
            Assert.True(File.Exists(Path.Combine(first.ProjectPath, "Packages", "manifest.json")));
        }
        finally { if (Directory.Exists(repository)) Directory.Delete(repository, true); }
    }

    [Fact]
    public void Cache_hit_contract_skips_workspace_preparation_and_unity_process_start()
    {
        var service = File.ReadAllText(Path.Combine(FindRoot(), "src", "LLMGameCreator.Application", "Design", "ProjectStandaloneBuild", "ProjectStandaloneBuildService.cs"));
        Assert.True(service.IndexOf("if (!HostIsComplete(hostRoot, cacheKey))", StringComparison.Ordinal) < service.IndexOf("BuildHost(unity, hostRoot, cacheKey, token)", StringComparison.Ordinal));
        Assert.Contains("var workspace = workspaceService.Prepare(token);", service, StringComparison.Ordinal);
        Assert.Contains("Process.Start(new ProcessStartInfo(unityPath, arguments)", service, StringComparison.Ordinal);
    }

    private static string CreateRepository()
    {
        var root = Path.Combine(Path.GetTempPath(), "llmgc-goal152c-" + Guid.NewGuid().ToString("N"));
        var unity = Path.Combine(root, "unity", "LLMGameCreatorAlpha");
        Directory.CreateDirectory(Path.Combine(unity, "Assets", "StreamingAssets"));
        Directory.CreateDirectory(Path.Combine(unity, "Packages"));
        Directory.CreateDirectory(Path.Combine(unity, "ProjectSettings"));
        File.WriteAllText(Path.Combine(unity, "Assets", "kept.txt"), "assets");
        File.WriteAllText(Path.Combine(unity, "Assets", "StreamingAssets", "ignored.txt"), "ignored");
        File.WriteAllText(Path.Combine(unity, "Packages", "manifest.json"), "{}");
        File.WriteAllText(Path.Combine(unity, "ProjectSettings", "ProjectVersion.txt"), "m_EditorVersion: test");
        return root;
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
