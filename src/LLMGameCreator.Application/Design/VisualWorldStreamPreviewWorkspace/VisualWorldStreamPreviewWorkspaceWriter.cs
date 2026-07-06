namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> WriteAsync(
        string projectRootPath,
        VisualWorldStreamPreviewWorkspaceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(
            projectRoot,
            VisualWorldStreamPreviewWorkspaceVocabulary.RelativeOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar)));
        EnsureContained(projectRoot, outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var write = new VisualWorldStreamPreviewWorkspaceWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            ReportMarkdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName),
            CatalogJsonPath = Path.Combine(outputDirectory, CatalogJsonFileName),
            ProofStatusJsonPath = Path.Combine(outputDirectory, ProofStatusJsonFileName),
            WinFormsBindingInventoryJsonPath =
                Path.Combine(outputDirectory, WinFormsBindingInventoryJsonFileName),
            QualityGateScanJsonPath = Path.Combine(outputDirectory, QualityGateScanJsonFileName),
            SourceHealthScanJsonPath = Path.Combine(outputDirectory, SourceHealthScanJsonFileName),
            Result = result
        };

        await File.WriteAllTextAsync(
            write.ReportMarkdownPath,
            result.ReportMarkdown,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.CatalogJsonPath,
            result.CatalogJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.ProofStatusJsonPath,
            result.ProofStatusJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.WinFormsBindingInventoryJsonPath,
            result.WinFormsBindingInventoryJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.QualityGateScanJsonPath,
            result.QualityGateScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            write.SourceHealthScanJsonPath,
            result.SourceHealthScanJson,
            Utf8WithoutBom,
            cancellationToken).ConfigureAwait(false);

        return write;
    }
}
