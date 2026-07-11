using System.Security.Cryptography;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public enum GameProjectSupportFileTargetState
{
    Missing,
    MatchingExisting,
    ConflictingExisting
}

public enum GameProjectSupportFileActivationAction
{
    Copy,
    Reuse,
    Reject
}

public sealed record GameProjectSupportFileRequirement
{
    public string ScriptId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string SourceRelativePath { get; init; } = string.Empty;
}

public interface IGameProjectSupportFileSource
{
    string SourceRoot { get; }
    IReadOnlyList<GameProjectSupportFileRequirement> RequiredFiles(GamePackageDefinition package);
}

public sealed class NarrowAlphaTemplateSupportFileSource : IGameProjectSupportFileSource
{
    public NarrowAlphaTemplateSupportFileSource(string sourceRoot)
    {
        if (string.IsNullOrWhiteSpace(sourceRoot))
            throw new ArgumentException("support file source root is required", nameof(sourceRoot));
        SourceRoot = Path.GetFullPath(sourceRoot);
    }

    public string SourceRoot { get; }

    public IReadOnlyList<GameProjectSupportFileRequirement> RequiredFiles(GamePackageDefinition package)
    {
        ArgumentNullException.ThrowIfNull(package);
        return package.ScriptCatalog.Scripts
            .Select(script => new GameProjectSupportFileRequirement
            {
                ScriptId = script.Id,
                RelativePath = script.Path,
                SourceRelativePath = script.Path
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ThenBy(item => item.ScriptId, StringComparer.Ordinal)
            .ToList();
    }
}

public sealed record GameProjectSupportFilePlanEntry
{
    public string ScriptId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string SourceSha256 { get; init; } = string.Empty;
    public string TargetPath { get; init; } = string.Empty;
    public GameProjectSupportFileTargetState TargetState { get; init; }
    public GameProjectSupportFileActivationAction ActivationAction { get; init; }
}

public sealed record GameProjectSupportFilePlan
{
    public string SourceRoot { get; init; } = string.Empty;
    public string ProjectRoot { get; init; } = string.Empty;
    public IReadOnlyList<GameProjectSupportFilePlanEntry> Entries { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool IsValid => Diagnostics.Count == 0
                           && Entries.All(entry => entry.ActivationAction != GameProjectSupportFileActivationAction.Reject);
    public int RequiredFileCount => Entries
        .Select(entry => entry.TargetPath)
        .Where(path => !string.IsNullOrWhiteSpace(path))
        .Distinct(GameProjectSupportFileMaterializer.PathComparer)
        .Count();
}

public sealed record GameProjectSupportFileActivationResult
{
    public int CopiedFileCount { get; init; }
    public int ReusedFileCount { get; init; }
}

public sealed class GameProjectSupportFileMaterializer
{
    internal static StringComparer PathComparer { get; } = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public GameProjectSupportFilePlan CreatePlan(
        GamePackageDefinition package,
        string projectRoot,
        IGameProjectSupportFileSource source)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(projectRoot))
            throw new ArgumentException("project root is required", nameof(projectRoot));

        var fullProjectRoot = Path.GetFullPath(projectRoot);
        var fullSourceRoot = Path.GetFullPath(source.SourceRoot);
        var diagnostics = new List<string>();
        var requirements = source.RequiredFiles(package)
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ThenBy(item => item.ScriptId, StringComparer.Ordinal)
            .ToList();

        foreach (var duplicateId in requirements
                     .Where(item => !string.IsNullOrWhiteSpace(item.ScriptId))
                     .GroupBy(item => item.ScriptId, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key)
                     .OrderBy(value => value, StringComparer.Ordinal))
        {
            diagnostics.Add("support.script_id.duplicate: " + duplicateId);
        }

        var entries = requirements.Select(requirement => BuildEntry(
                requirement,
                fullSourceRoot,
                fullProjectRoot,
                diagnostics))
            .ToList();

        foreach (var group in entries
                     .Where(entry => !string.IsNullOrWhiteSpace(entry.TargetPath))
                     .GroupBy(entry => entry.TargetPath, PathComparer)
                     .Where(group => group.Count() > 1))
        {
            var equivalent = group.All(entry => !string.IsNullOrWhiteSpace(entry.SourceSha256))
                             && group.Select(entry => entry.SourceSha256)
                                 .Distinct(StringComparer.Ordinal).Count() == 1;
            if (equivalent) continue;

            var relativePath = group.Select(entry => entry.RelativePath)
                .OrderBy(value => value, StringComparer.Ordinal).First();
            diagnostics.Add("support.target.duplicate_conflict: " + relativePath);
            for (var index = 0; index < entries.Count; index++)
            {
                if (PathComparer.Equals(entries[index].TargetPath, group.Key))
                    entries[index] = entries[index] with { ActivationAction = GameProjectSupportFileActivationAction.Reject };
            }
        }

        return new GameProjectSupportFilePlan
        {
            SourceRoot = fullSourceRoot,
            ProjectRoot = fullProjectRoot,
            Entries = entries,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList()
        };
    }

    public string StageValidationProject(
        string qualifiedPackagePath,
        GameProjectSupportFilePlan plan,
        string validationProjectRoot)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!plan.IsValid) throw new InvalidOperationException("invalid support file plan cannot be staged");

        var fullValidationRoot = Path.GetFullPath(validationProjectRoot);
        EnsureContained(plan.ProjectRoot, fullValidationRoot, "support validation project path escape rejected");
        if (Directory.Exists(fullValidationRoot))
            throw new InvalidOperationException("support validation project already exists");

        Directory.CreateDirectory(fullValidationRoot);
        File.Copy(qualifiedPackagePath, Path.Combine(fullValidationRoot, "package.json"), overwrite: false);
        foreach (var entry in UniqueEntries(plan))
        {
            var sourcePath = entry.ActivationAction == GameProjectSupportFileActivationAction.Reuse
                ? entry.TargetPath
                : entry.SourcePath;
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Support file changed after planning: " + entry.RelativePath, sourcePath);
            if (!string.Equals(HashFile(sourcePath), entry.SourceSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Support file hash changed after planning: " + entry.RelativePath);
            var targetPath = ResolveContained(fullValidationRoot, entry.RelativePath, "staged support target path escape rejected");
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(sourcePath, targetPath, overwrite: false);
        }

        return fullValidationRoot;
    }

    internal static IReadOnlyList<GameProjectSupportFilePlanEntry> UniqueEntries(GameProjectSupportFilePlan plan) =>
        plan.Entries
            .GroupBy(entry => entry.TargetPath, PathComparer)
            .Select(group => group.OrderBy(entry => entry.ScriptId, StringComparer.Ordinal).First())
            .OrderBy(entry => entry.RelativePath, StringComparer.Ordinal)
            .ToList();

    internal static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static GameProjectSupportFilePlanEntry BuildEntry(
        GameProjectSupportFileRequirement requirement,
        string sourceRoot,
        string projectRoot,
        List<string> diagnostics)
    {
        var relativePath = Normalize(requirement.RelativePath);
        var sourceRelativePath = Normalize(requirement.SourceRelativePath);
        var rejected = false;

        if (string.IsNullOrWhiteSpace(requirement.ScriptId))
        {
            diagnostics.Add("support.script_id.empty: " + relativePath);
            rejected = true;
        }
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            diagnostics.Add("support.path.empty: " + requirement.ScriptId);
            rejected = true;
        }
        else if (IsRooted(relativePath))
        {
            diagnostics.Add("support.path.rooted: " + relativePath);
            rejected = true;
        }
        else if (ContainsTraversal(relativePath))
        {
            diagnostics.Add("support.path.traversal: " + relativePath);
            rejected = true;
        }

        string sourcePath = string.Empty;
        string targetPath = string.Empty;
        if (!rejected)
        {
            try
            {
                sourcePath = ResolveContained(sourceRoot, sourceRelativePath, "support source path escape rejected");
            }
            catch (InvalidOperationException)
            {
                diagnostics.Add("support.source.outside_root: " + sourceRelativePath);
                rejected = true;
            }

            try
            {
                targetPath = ResolveContained(projectRoot, relativePath, "support target path escape rejected");
            }
            catch (InvalidOperationException)
            {
                diagnostics.Add("support.target.outside_project: " + relativePath);
                rejected = true;
            }
        }

        var sourceSha256 = string.Empty;
        if (!rejected && !File.Exists(sourcePath))
        {
            diagnostics.Add("support.source.missing: " + relativePath);
            rejected = true;
        }
        else if (!rejected)
        {
            sourceSha256 = HashFile(sourcePath);
        }

        var targetState = GameProjectSupportFileTargetState.Missing;
        var action = rejected ? GameProjectSupportFileActivationAction.Reject : GameProjectSupportFileActivationAction.Copy;
        if (!rejected && File.Exists(targetPath))
        {
            if (string.Equals(HashFile(targetPath), sourceSha256, StringComparison.Ordinal))
            {
                targetState = GameProjectSupportFileTargetState.MatchingExisting;
                action = GameProjectSupportFileActivationAction.Reuse;
            }
            else
            {
                targetState = GameProjectSupportFileTargetState.ConflictingExisting;
                action = GameProjectSupportFileActivationAction.Reject;
                diagnostics.Add("support.target.conflict: " + relativePath);
            }
        }

        return new GameProjectSupportFilePlanEntry
        {
            ScriptId = requirement.ScriptId,
            RelativePath = relativePath,
            SourcePath = sourcePath,
            SourceSha256 = sourceSha256,
            TargetPath = targetPath,
            TargetState = targetState,
            ActivationAction = action
        };
    }

    private static string ResolveContained(string root, string relativePath, string message)
    {
        var candidate = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(root, candidate, message);
        return candidate;
    }

    private static void EnsureContained(string root, string candidate, string message)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullCandidate = Path.GetFullPath(candidate);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!PathComparer.Equals(fullRoot, fullCandidate)
            && !fullCandidate.StartsWith(fullRoot + Path.DirectorySeparatorChar, comparison))
            throw new InvalidOperationException(message);
    }

    private static bool IsRooted(string path) => Path.IsPathRooted(path)
                                                   || path.Contains(':', StringComparison.Ordinal)
                                                   || path.Contains("://", StringComparison.Ordinal);

    private static bool ContainsTraversal(string path) => path
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Contains("..", StringComparer.Ordinal);

    private static string Normalize(string value) => (value ?? string.Empty).Replace('\\', '/');
}
