using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

/// <summary>Prepares the only writable Unity project used for a host build outside the repository.</summary>
public sealed class UnityHostBuildWorkspaceService
{
    private static readonly string[] SourceFolders = ["Assets", "Packages", "ProjectSettings"];
    private readonly string _repositoryProject;
    private readonly string _workspaceRoot;

    public UnityHostBuildWorkspaceService(string repositoryRoot)
    {
        _repositoryProject = Path.Combine(Path.GetFullPath(repositoryRoot), "unity", "LLMGameCreatorAlpha");
        _workspaceRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "UnityHostBuildWorkspaces");
    }

    public UnityHostBuildWorkspace Prepare(CancellationToken cancellationToken = default)
    {
        EnsureSourceExists();
        var before = SourceManifest();
        var key = HashText(string.Join("\n", before.Files.Select(file => file.Path + ":" + file.Sha256)))[..32];
        var root = Path.Combine(_workspaceRoot, key);
        var prepared = Path.Combine(root, "prepared");
        var staging = Path.Combine(root, "staging-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            foreach (var folder in SourceFolders)
                CopySourceFolder(Path.Combine(_repositoryProject, folder), Path.Combine(staging, folder), cancellationToken);
            ValidateWorkspace(staging);
            File.WriteAllText(Path.Combine(staging, "llmgc-workspace-manifest.json"), JsonSerializer.Serialize(new { schemaVersion = "llmgc_unity_host_workspace_v1", workspaceKey = key, sourceManifest = before }, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
            var after = SourceManifest();
            if (!before.Files.SequenceEqual(after.Files)) throw new InvalidOperationException("Repository Unity source changed while preparing the external workspace.");
            var previous = prepared + ".previous-" + Guid.NewGuid().ToString("N");
            if (Directory.Exists(prepared)) Directory.Move(prepared, previous);
            try { Directory.Move(staging, prepared); if (Directory.Exists(previous)) Directory.Delete(previous, true); }
            catch { if (!Directory.Exists(prepared) && Directory.Exists(previous)) Directory.Move(previous, prepared); throw; }
            return new UnityHostBuildWorkspace(key, prepared, before, after);
        }
        catch
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            throw;
        }
    }

    public string CreateUnityArguments(string executableOutput, string logPath, string projectPath)
    {
        var fullProject = Path.GetFullPath(projectPath);
        if (!IsUnder(fullProject, _workspaceRoot) || IsUnder(fullProject, _repositoryProject)) throw new InvalidOperationException("Unity projectPath must be an external LocalAppData workspace.");
        return "-batchmode -nographics -quit -projectPath \"" + fullProject + "\" -executeMethod LLMGameCreator.ProjectStandaloneBuildEntrypoint.BuildWindowsHost -llmgcStandaloneHostOutput \"" + executableOutput + "\" -logFile \"" + logPath + "\"";
    }

    private void EnsureSourceExists()
    {
        foreach (var folder in SourceFolders)
            if (!Directory.Exists(Path.Combine(_repositoryProject, folder))) throw new DirectoryNotFoundException("Unity source folder is missing: " + folder);
    }

    private SourceSnapshotManifest SourceManifest() => new()
    {
        Files = SourceFolders.SelectMany(folder => Directory.GetFiles(Path.Combine(_repositoryProject, folder), "*", SearchOption.AllDirectories)
                .Where(file => !Excluded(Path.GetRelativePath(_repositoryProject, file)))
                .Select(file => new SourceSnapshotFile(Path.GetRelativePath(_repositoryProject, file).Replace('\\', '/'), HashFile(file))))
            .OrderBy(file => file.Path, StringComparer.Ordinal).ToList()
    };

    private void CopySourceFolder(string source, string destination, CancellationToken token)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(_repositoryProject, file);
            if (Excluded(relative)) continue;
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, true);
        }
    }

    private static bool Excluded(string relative)
    {
        var path = relative.Replace('\\', '/');
        return path.StartsWith("Library/", StringComparison.Ordinal) || path.StartsWith("Temp/", StringComparison.Ordinal) || path.StartsWith("Logs/", StringComparison.Ordinal) || path.StartsWith("obj/", StringComparison.Ordinal) || path.StartsWith("UserSettings/", StringComparison.Ordinal) || path.StartsWith("Assets/StreamingAssets/", StringComparison.Ordinal) || path.Equals("Assets/__LLMGC_ProjectStandaloneBuild__.unity", StringComparison.Ordinal) || path.Equals("Assets/__LLMGC_ProjectStandaloneBuild__.unity.meta", StringComparison.Ordinal);
    }

    private static void ValidateWorkspace(string workspace)
    {
        foreach (var folder in SourceFolders)
            if (!Directory.Exists(Path.Combine(workspace, folder))) throw new InvalidOperationException("External workspace is incomplete: " + folder);
    }

    private static bool IsUnder(string path, string root)
    {
        var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(path).StartsWith(prefix, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
    private static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string HashText(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}

public sealed record UnityHostBuildWorkspace(string WorkspaceKey, string ProjectPath, SourceSnapshotManifest SourceBefore, SourceSnapshotManifest SourceAfter);
public sealed record SourceSnapshotManifest { public List<SourceSnapshotFile> Files { get; init; } = []; }
public sealed record SourceSnapshotFile(string Path, string Sha256);
