using System.Security.Cryptography;
using System.Text;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public sealed class ProjectStandaloneOutputLocationService
{
    private readonly string _root;

    public ProjectStandaloneOutputLocationService(string? rootOverride = null)
    {
        var configured = rootOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LGC", "O");
        if (configured.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => string.Equals(segment, "..", StringComparison.Ordinal)))
            throw new InvalidOperationException("standalone.output.path_escape");
        _root = Path.GetFullPath(configured).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        RejectReparsePoint(_root);
    }

    public string Root => _root;

    public ProjectStandaloneOutputLocation Resolve(string projectFolder, string packageId, string attemptToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(attemptToken);
        if (attemptToken.Any(character => !char.IsAsciiLetterOrDigit(character)))
            throw new InvalidOperationException("standalone.output.path_escape");

        var normalizedProject = Path.GetFullPath(projectFolder)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var tokenInput = OperatingSystem.IsWindows()
            ? normalizedProject.ToUpperInvariant() + "\n" + packageId
            : normalizedProject + "\n" + packageId;
        var projectToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenInput))).ToLowerInvariant()[..16];
        var projectRoot = Confined(_root, projectToken);
        return new ProjectStandaloneOutputLocation
        {
            Root = _root,
            ProjectToken = projectToken,
            ProjectRoot = projectRoot,
            CurrentOutputFolder = Confined(projectRoot, ProjectStandaloneBuildVocabulary.CurrentOutputDirectoryName),
            StagingOutputFolder = Confined(projectRoot, "s-" + attemptToken[..Math.Min(12, attemptToken.Length)]),
            BackupOutputFolder = Confined(projectRoot, "b-" + attemptToken[..Math.Min(12, attemptToken.Length)])
        };
    }

    public ProjectStandaloneOutputPathBudgetResult ValidatePlayerPathBudget(
        string outputFolder,
        string markerLogPath,
        string playerLogPath)
    {
        var root = Path.GetFullPath(outputFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var candidates = new List<(string Absolute, string Relative)>
        {
            (Path.Combine(root, ProjectStandaloneBuildVocabulary.OperationalExecutableName), ProjectStandaloneBuildVocabulary.OperationalExecutableName),
            (Path.Combine(root, ProjectStandaloneBuildVocabulary.OperationalDataDirectoryName), ProjectStandaloneBuildVocabulary.OperationalDataDirectoryName),
            (Path.Combine(root, "UnityPlayer.dll"), "UnityPlayer.dll"),
            (Path.Combine(root, "MonoBleedingEdge"), "MonoBleedingEdge"),
            (Path.Combine(root, "build-manifest.json"), "build-manifest.json"),
            (Path.GetFullPath(markerLogPath), "smoke-marker/" + Path.GetFileName(markerLogPath)),
            (Path.GetFullPath(playerLogPath), "player-log/" + Path.GetFileName(playerLogPath))
        };
        var payloadRoot = Path.Combine(root, ProjectStandaloneBuildVocabulary.OperationalDataDirectoryName,
            "StreamingAssets", "LLMGameCreatorProject");
        if (Directory.Exists(payloadRoot))
        {
            candidates.AddRange(Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories)
                .Select(path => (path, Path.GetRelativePath(root, path).Replace('\\', '/'))));
        }

        var longest = candidates.OrderByDescending(candidate => candidate.Absolute.Length)
            .ThenBy(candidate => candidate.Relative, StringComparer.Ordinal).FirstOrDefault();
        var failures = candidates.Where(candidate => candidate.Absolute.Length > ProjectStandaloneBuildVocabulary.PlayerPathBudgetLimit)
            .OrderBy(candidate => candidate.Relative, StringComparer.Ordinal)
            .Select(candidate => "standalone.output.player_path_budget_exceeded:"
                + candidate.Relative + ":" + candidate.Absolute.Length).ToList();
        return new ProjectStandaloneOutputPathBudgetResult
        {
            MaximumAbsolutePathLength = longest.Absolute?.Length ?? 0,
            LongestRelativePath = longest.Relative ?? string.Empty,
            Passed = failures.Count == 0,
            Diagnostics = failures
        };
    }

    public void Publish(ProjectStandaloneOutputLocation location, Action<string> finalOutputValidator)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(finalOutputValidator);
        if (!string.Equals(location.Root, _root, OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("standalone.output.path_escape");

        Directory.CreateDirectory(location.ProjectRoot);
        RejectReparsePoint(location.ProjectRoot);
        var backupCreated = false;
        try
        {
            if (Directory.Exists(location.CurrentOutputFolder))
            {
                if (Directory.Exists(location.BackupOutputFolder)) Directory.Delete(location.BackupOutputFolder, true);
                Directory.Move(location.CurrentOutputFolder, location.BackupOutputFolder);
                backupCreated = true;
            }
            Directory.Move(location.StagingOutputFolder, location.CurrentOutputFolder);
            finalOutputValidator(location.CurrentOutputFolder);
            if (backupCreated && Directory.Exists(location.BackupOutputFolder))
                Directory.Delete(location.BackupOutputFolder, true);
        }
        catch
        {
            if (Directory.Exists(location.CurrentOutputFolder)) Directory.Delete(location.CurrentOutputFolder, true);
            if (backupCreated && Directory.Exists(location.BackupOutputFolder))
                Directory.Move(location.BackupOutputFolder, location.CurrentOutputFolder);
            if (Directory.Exists(location.StagingOutputFolder)) Directory.Delete(location.StagingOutputFolder, true);
            throw;
        }
    }

    private static string Confined(string root, string child)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(fullRoot, child));
        if (!path.StartsWith(fullRoot, OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("standalone.output.path_escape");
        return path;
    }

    private static void RejectReparsePoint(string path)
    {
        if (Directory.Exists(path)
            && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException("standalone.output.path_escape");
    }
}
