namespace LLMGameCreator.Application.Design.FeatureModuleComposition;

public sealed class FeatureModuleCompositionOperatorRunner
{
    private readonly FeatureModuleCompositionService _service;

    public FeatureModuleCompositionOperatorRunner(FeatureModuleCompositionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<FeatureModuleCompositionWriteResult> RunAsync(
        string repositoryRoot,
        IReadOnlyList<string>? selectedModuleIds = null,
        string compositionId = "",
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(repositoryRoot);
        var procedural = Path.Combine(root, FeatureModuleCompositionVocabulary.ProceduralRoot.Replace('/', Path.DirectorySeparatorChar));
        var export = Path.Combine(root, FeatureModuleCompositionVocabulary.ExportRoot.Replace('/', Path.DirectorySeparatorChar));
        var backup = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal146-operator-" + Guid.NewGuid().ToString("N"));
        var proceduralExisted = Directory.Exists(procedural);
        var exportExisted = Directory.Exists(export);
        try
        {
            SnapshotDirectory(procedural, Path.Combine(backup, "procedural"));
            SnapshotDirectory(export, Path.Combine(backup, "export"));
            return await _service.RunAndWriteAsync(root, new FeatureModuleCompositionRunRequest
            {
                SelectedModuleIds = selectedModuleIds,
                CompositionId = compositionId
            }, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            RestoreDirectory(procedural, Path.Combine(backup, "procedural"), proceduralExisted);
            RestoreDirectory(export, Path.Combine(backup, "export"), exportExisted);
            throw;
        }
        finally
        {
            if (Directory.Exists(backup)) Directory.Delete(backup, true);
        }
    }

    internal static void SnapshotDirectory(string source, string destination)
    {
        if (!Directory.Exists(source)) return;
        CopyDirectory(source, destination);
    }

    internal static void RestoreDirectory(string destination, string backup, bool existed)
    {
        if (Directory.Exists(destination)) Directory.Delete(destination, true);
        if (existed) CopyDirectory(backup, destination);
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, true);
        }
    }
}
