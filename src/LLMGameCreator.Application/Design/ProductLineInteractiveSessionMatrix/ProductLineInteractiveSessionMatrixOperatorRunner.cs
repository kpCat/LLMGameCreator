namespace LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;

public sealed class ProductLineInteractiveSessionMatrixOperatorRunner
{
    private readonly IProductLineInteractiveSessionMatrixWriter _writer;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ProductLineInteractiveSessionMatrixOperatorRunner(IProductLineInteractiveSessionMatrixWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task<ProductLineInteractiveSessionMatrixWriteResult> RunAsync(
        string repositoryRootPath,
        ProductLineInteractiveSessionMatrixRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var root = Path.GetFullPath(repositoryRootPath);
            var procedural = ArtifactRoot(root, ProductLineInteractiveSessionMatrixVocabulary.ProceduralRoot);
            var export = ArtifactRoot(root, ProductLineInteractiveSessionMatrixVocabulary.ExportRoot);
            var backup = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal145-operator-" + Guid.NewGuid().ToString("N"));
            var proceduralBackup = Path.Combine(backup, "procedural");
            var exportBackup = Path.Combine(backup, "export");
            var proceduralExisted = Directory.Exists(procedural);
            var exportExisted = Directory.Exists(export);
            Directory.CreateDirectory(backup);
            try
            {
                SnapshotDirectory(procedural, proceduralBackup);
                SnapshotDirectory(export, exportBackup);
                try
                {
                    return await _writer.RunAndWriteAsync(root, request, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    RestoreDirectory(procedural, proceduralBackup, proceduralExisted);
                    RestoreDirectory(export, exportBackup, exportExisted);
                    throw;
                }
            }
            finally
            {
                DeleteDirectory(backup);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string ArtifactRoot(string root, string relative)
    {
        if (!File.Exists(Path.Combine(root, "LLMGameCreator.sln")))
        {
            throw new InvalidOperationException("Repository root was not found.");
        }

        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        Goal142CandidateDiscovery.GuardUnder(path, root, "Goal145 artifact root");
        return path;
    }

    private static void SnapshotDirectory(string source, string destination)
    {
        if (Directory.Exists(source)) CopyDirectory(source, destination);
    }

    private static void RestoreDirectory(string destination, string backup, bool existed)
    {
        DeleteDirectory(destination);
        if (existed) CopyDirectory(backup, destination);
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
    }
}
