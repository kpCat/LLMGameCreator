using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;

public interface IProductLineRuntimeVariantMatrixWriter
{
    Task<ProductLineRuntimeVariantMatrixWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        ProductLineRuntimeVariantMatrixRequest? request = null,
        CancellationToken cancellationToken = default);
}

public sealed class ProductLineRuntimeVariantMatrixOperatorRunner
{
    private readonly IProductLineRuntimeVariantMatrixWriter _writer;
    private readonly SemaphoreSlim _runGate = new(1, 1);

    public ProductLineRuntimeVariantMatrixOperatorRunner(
        IProductLineRuntimeVariantMatrixWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task<ProductLineRuntimeVariantMatrixWriteResult> RunAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        await _runGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await RunTransactionAsync(repositoryRootPath, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _runGate.Release();
        }
    }

    private async Task<ProductLineRuntimeVariantMatrixWriteResult> RunTransactionAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var proceduralRoot = ResolveArtifactRoot(
            root,
            ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory);
        var exportRoot = ResolveArtifactRoot(
            root,
            ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory);
        var backupRoot = Path.Combine(
            Path.GetTempPath(),
            "LLMGameCreator",
            "goal142-operator-" + Guid.NewGuid().ToString("N"));
        var proceduralBackup = Path.Combine(backupRoot, "procedural");
        var exportBackup = Path.Combine(backupRoot, "export");
        var proceduralExisted = Directory.Exists(proceduralRoot);
        var exportExisted = Directory.Exists(exportRoot);

        Directory.CreateDirectory(backupRoot);
        try
        {
            SnapshotDirectory(proceduralRoot, proceduralBackup);
            SnapshotDirectory(exportRoot, exportBackup);
            DeleteDirectory(proceduralRoot);
            DeleteDirectory(exportRoot);

            try
            {
                var result = await _writer.BuildAndWriteAsync(
                        root,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (!string.Equals(result.Dashboard.MatrixStatus, "GREEN", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Goal142 in-process matrix generation did not produce GREEN status.");
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
        if (string.IsNullOrWhiteSpace(repositoryRootPath))
        {
            throw new ArgumentException("Repository root is required.", nameof(repositoryRootPath));
        }

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
        var rootedPrefix = root.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootedPrefix, comparison))
        {
            throw new InvalidOperationException("Goal142 artifact root must stay under repository root.");
        }

        return path;
    }

    private static void SnapshotDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            return;
        }

        CopyDirectory(source, destination);
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
