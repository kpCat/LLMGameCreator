using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;

public interface ISelectedRuntimeVariantPlayerAdapterWriter
{
    Task<SelectedRuntimeVariantPlayerAdapterWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        SelectedRuntimeVariantPlayerAdapterRequest? request = null,
        CancellationToken cancellationToken = default);
}

public sealed class SelectedRuntimeVariantPlayerAdapterOperatorRunner
{
    private readonly ISelectedRuntimeVariantPlayerAdapterWriter _writer;
    private readonly SemaphoreSlim _runGate = new(1, 1);

    public SelectedRuntimeVariantPlayerAdapterOperatorRunner(
        ISelectedRuntimeVariantPlayerAdapterWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task<SelectedRuntimeVariantPlayerAdapterWriteResult> RunAsync(
        string repositoryRootPath,
        SelectedRuntimeVariantPlayerAdapterRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        await _runGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await RunTransactionAsync(repositoryRootPath, request, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _runGate.Release();
        }
    }

    private async Task<SelectedRuntimeVariantPlayerAdapterWriteResult> RunTransactionAsync(
        string repositoryRootPath,
        SelectedRuntimeVariantPlayerAdapterRequest? request,
        CancellationToken cancellationToken)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var proceduralRoot = ResolveArtifactRoot(
            root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.ProceduralOutputDirectory);
        var exportRoot = ResolveArtifactRoot(
            root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.ExportPackageDirectory);
        var backupRoot = Path.Combine(
            Path.GetTempPath(),
            "LLMGameCreator",
            "goal143-operator-" + Guid.NewGuid().ToString("N"));
        var proceduralBackup = Path.Combine(backupRoot, "procedural");
        var exportBackup = Path.Combine(backupRoot, "export");
        var proceduralExisted = Directory.Exists(proceduralRoot);
        var exportExisted = Directory.Exists(exportRoot);

        Directory.CreateDirectory(backupRoot);
        try
        {
            SnapshotDirectory(proceduralRoot, proceduralBackup);
            SnapshotDirectory(exportRoot, exportBackup);
            try
            {
                var result = await _writer.BuildAndWriteAsync(
                        root,
                        request,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (!result.Result.CorePassed)
                {
                    throw new InvalidOperationException(
                        "Goal143 in-process PlayerAdapter generation did not pass core proof.");
                }

                return result;
            }
            catch
            {
                RestoreDirectory(proceduralRoot, proceduralBackup, proceduralExisted);
                RestoreDirectory(exportRoot, exportBackup, exportExisted);
                throw;
            }
        }
        finally
        {
            DeleteDirectory(backupRoot);
        }
    }

    private static string ResolveRepositoryRoot(string repositoryRootPath)
    {
        var root = Path.GetFullPath(repositoryRootPath);
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found: " + repositoryRootPath);
        }

        return root;
    }

    private static string ResolveArtifactRoot(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(
            root,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefix = root.TrimEnd(
                         Path.DirectorySeparatorChar,
                         Path.AltDirectorySeparatorChar)
                     + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, comparison))
        {
            throw new InvalidOperationException(
                "Goal143 artifact root must stay under repository root.");
        }

        return path;
    }

    private static void SnapshotDirectory(string source, string destination)
    {
        if (Directory.Exists(source))
        {
            CopyDirectory(source, destination);
        }
    }

    private static void RestoreDirectory(
        string destination,
        string backup,
        bool previouslyExisted)
    {
        DeleteDirectory(destination);
        if (previouslyExisted)
        {
            CopyDirectory(backup, destination);
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.EnumerateDirectories(
                     source,
                     "*",
                     SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(
                destination,
                Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
