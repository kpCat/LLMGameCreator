using LLMGameCreator.Application.Projects;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public interface IGameProjectPackageActivationStore
{
    Task ReplaceAsync(string qualifiedPackagePath, string projectPackagePath, CancellationToken cancellationToken);
}

public sealed class AtomicGameProjectPackageActivationStore : IGameProjectPackageActivationStore
{
    public Task ReplaceAsync(string qualifiedPackagePath, string projectPackagePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var projectFolder = Path.GetDirectoryName(projectPackagePath)
                            ?? throw new InvalidOperationException("Project package folder is unavailable.");
        Directory.CreateDirectory(projectFolder);
        var temporary = Path.Combine(projectFolder, ".package.json.tmp-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.Copy(qualifiedPackagePath, temporary, overwrite: false);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, projectPackagePath, overwrite: true);
            return Task.CompletedTask;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}

public sealed class GameProjectBuildTransaction
{
    private readonly string _packagePath;
    private readonly string _authoringPath;
    private readonly string _identityPath;
    private readonly string _legacyAuthoringPath;
    private readonly ICurrentGamePackageService _currentPackageService;
    private readonly IGameProjectPackageActivationStore _activationStore;
    private readonly byte[]? _packageBytes;
    private readonly byte[]? _authoringBytes;
    private readonly byte[]? _identityBytes;
    private readonly byte[]? _legacyAuthoringBytes;
    private readonly GamePackageDefinition? _currentPackage;
    private readonly List<SupportFileSnapshot> _supportFileSnapshots = [];
    private readonly HashSet<string> _copiedSupportFileTargets = new(GameProjectSupportFileMaterializer.PathComparer);
    private bool _committed;

    public GameProjectBuildTransaction(
        string projectFolder,
        string authoringPath,
        string identityPath,
        string legacyAuthoringPath,
        ICurrentGamePackageService currentPackageService,
        IGameProjectPackageActivationStore activationStore)
    {
        _packagePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder, "package.json");
        _authoringPath = Path.GetFullPath(authoringPath);
        _identityPath = Path.GetFullPath(identityPath);
        _legacyAuthoringPath = Path.GetFullPath(legacyAuthoringPath);
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _activationStore = activationStore ?? throw new ArgumentNullException(nameof(activationStore));
        _packageBytes = File.Exists(_packagePath) ? File.ReadAllBytes(_packagePath) : null;
        _authoringBytes = File.Exists(_authoringPath) ? File.ReadAllBytes(_authoringPath) : null;
        _identityBytes = File.Exists(_identityPath) ? File.ReadAllBytes(_identityPath) : null;
        _legacyAuthoringBytes = File.Exists(_legacyAuthoringPath) ? File.ReadAllBytes(_legacyAuthoringPath) : null;
        _currentPackage = currentPackageService.CurrentPackage;
    }

    public GameProjectSupportFileActivationResult ActivateSupportFiles(
        GameProjectSupportFilePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!plan.IsValid) throw new InvalidOperationException("invalid support file plan cannot be activated");

        var entries = GameProjectSupportFileMaterializer.UniqueEntries(plan);
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _supportFileSnapshots.Add(new SupportFileSnapshot(
                entry.TargetPath,
                File.Exists(entry.TargetPath),
                File.Exists(entry.TargetPath) ? File.ReadAllBytes(entry.TargetPath) : null));
        }

        var copied = 0;
        var reused = 0;
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.ActivationAction == GameProjectSupportFileActivationAction.Reuse)
            {
                if (!File.Exists(entry.TargetPath)
                    || !string.Equals(GameProjectSupportFileMaterializer.HashFile(entry.TargetPath), entry.SourceSha256, StringComparison.Ordinal))
                    throw new InvalidOperationException("Support file changed after planning: " + entry.RelativePath);
                reused++;
                continue;
            }

            if (entry.ActivationAction != GameProjectSupportFileActivationAction.Copy)
                throw new InvalidOperationException("Rejected support file cannot be activated: " + entry.RelativePath);
            if (File.Exists(entry.TargetPath))
                throw new InvalidOperationException("Support file target appeared after planning: " + entry.RelativePath);
            if (!File.Exists(entry.SourcePath)
                || !string.Equals(GameProjectSupportFileMaterializer.HashFile(entry.SourcePath), entry.SourceSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Support file source changed after planning: " + entry.RelativePath);
            CopyAtomic(entry.SourcePath, entry.TargetPath, entry.SourceSha256, cancellationToken);
            _copiedSupportFileTargets.Add(entry.TargetPath);
            copied++;
        }

        return new GameProjectSupportFileActivationResult
        {
            CopiedFileCount = copied,
            ReusedFileCount = reused
        };
    }

    public Task ActivatePackageAsync(
        string qualifiedPackagePath,
        CancellationToken cancellationToken) =>
        _activationStore.ReplaceAsync(qualifiedPackagePath, _packagePath, cancellationToken);

    public void ReplaceCurrentPackage(GamePackageDefinition qualifiedPackage) =>
        _currentPackageService.ReplaceCurrent(qualifiedPackage);

    public async Task ActivateAsync(
        string qualifiedPackagePath,
        GamePackageDefinition qualifiedPackage,
        CancellationToken cancellationToken)
    {
        await ActivatePackageAsync(qualifiedPackagePath, cancellationToken).ConfigureAwait(false);
        ReplaceCurrentPackage(qualifiedPackage);
    }

    public void Commit() => _committed = true;

    public bool Rollback()
    {
        if (_committed) return false;
        for (var index = _supportFileSnapshots.Count - 1; index >= 0; index--)
        {
            var snapshot = _supportFileSnapshots[index];
            if (!_copiedSupportFileTargets.Contains(snapshot.TargetPath)) continue;
            RestoreFile(snapshot.TargetPath, snapshot.ExistedBefore ? snapshot.OriginalBytes : null);
        }
        RestoreFile(_packagePath, _packageBytes);
        RestoreFile(_authoringPath, _authoringBytes);
        RestoreFile(_identityPath, _identityBytes);
        RestoreFile(_legacyAuthoringPath, _legacyAuthoringBytes);
        if (_currentPackage is not null) _currentPackageService.ReplaceCurrent(_currentPackage);
        return true;
    }

    private static void CopyAtomic(
        string sourcePath,
        string targetPath,
        string expectedSha256,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        var temporary = targetPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.Copy(sourcePath, temporary, overwrite: false);
            if (!string.Equals(GameProjectSupportFileMaterializer.HashFile(temporary), expectedSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("Support file temporary copy hash mismatch: " + targetPath);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, targetPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static void RestoreFile(string path, byte[]? bytes)
    {
        if (bytes is null)
        {
            if (File.Exists(path)) File.Delete(path);
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".rollback-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private sealed record SupportFileSnapshot(string TargetPath, bool ExistedBefore, byte[]? OriginalBytes);
}
