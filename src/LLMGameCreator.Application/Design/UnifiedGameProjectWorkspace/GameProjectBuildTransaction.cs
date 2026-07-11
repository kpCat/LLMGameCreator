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
    private readonly ICurrentGamePackageService _currentPackageService;
    private readonly IGameProjectPackageActivationStore _activationStore;
    private readonly byte[]? _packageBytes;
    private readonly byte[]? _authoringBytes;
    private readonly GamePackageDefinition? _currentPackage;
    private bool _committed;

    public GameProjectBuildTransaction(
        string projectFolder,
        string authoringPath,
        ICurrentGamePackageService currentPackageService,
        IGameProjectPackageActivationStore activationStore)
    {
        _packagePath = GameProjectFeatureModuleAuthoringService.ConfinedPath(projectFolder, "package.json");
        _authoringPath = Path.GetFullPath(authoringPath);
        _currentPackageService = currentPackageService ?? throw new ArgumentNullException(nameof(currentPackageService));
        _activationStore = activationStore ?? throw new ArgumentNullException(nameof(activationStore));
        _packageBytes = File.Exists(_packagePath) ? File.ReadAllBytes(_packagePath) : null;
        _authoringBytes = File.Exists(_authoringPath) ? File.ReadAllBytes(_authoringPath) : null;
        _currentPackage = currentPackageService.CurrentPackage;
    }

    public async Task ActivateAsync(
        string qualifiedPackagePath,
        GamePackageDefinition qualifiedPackage,
        CancellationToken cancellationToken)
    {
        await _activationStore.ReplaceAsync(qualifiedPackagePath, _packagePath, cancellationToken).ConfigureAwait(false);
        _currentPackageService.ReplaceCurrent(qualifiedPackage);
    }

    public void Commit() => _committed = true;

    public bool Rollback()
    {
        if (_committed) return false;
        RestoreFile(_packagePath, _packageBytes);
        RestoreFile(_authoringPath, _authoringBytes);
        if (_currentPackage is not null) _currentPackageService.ReplaceCurrent(_currentPackage);
        return true;
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
}
