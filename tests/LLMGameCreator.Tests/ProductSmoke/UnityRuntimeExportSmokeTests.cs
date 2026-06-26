using System.Text.Json;
using LLMGameCreator.Application.Design.UnityRuntimeExport;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityRuntimeExportSmokeTests
{
    [Fact]
    public async Task UnityRuntimeExportProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityRuntimeExportAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot, content, assets);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, "export-manifest.json")));

        var report = JsonSerializer.Deserialize<UnityRuntimeExportReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(report.Accepted, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal("unity_runtime_export_vertical_slice_artifact_verification", report.ManualGate);
        Assert.Equal("unity-runtime-export", report.ProductSmokeRoute);
        Assert.NotEmpty(report.SelectedInput.PackageId);
        Assert.NotEmpty(report.SelectedInput.SourcePackageHash);
        Assert.NotEmpty(report.SelectedInput.AssetManifestHash);
        Assert.True(report.ExportFileCount >= 6);
        Assert.True(report.ExportByteCount > 0);
        Assert.True(report.ValidMatrixPassed);
        Assert.True(report.InvalidMatrixPassed);
        Assert.True(report.PackageValidationPassed);
        Assert.True(report.AssetManifestValidationPassed);
        Assert.True(report.ExportManifestValidationPassed);
        Assert.True(report.SelectedLoopResolutionPassed);
        Assert.False(report.WindowsExecutableProduced);
        Assert.False(report.UnityEditorExecuted);
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.ExternalExecution.AnyExecuted());
        foreach (var asset in report.SelectedInput.SelectedAssetRefs)
        {
            Assert.True(File.Exists(Path.Combine(write.ExportDirectoryPath, asset.ExportRelativePath.Replace('/', Path.DirectorySeparatorChar))));
        }
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
