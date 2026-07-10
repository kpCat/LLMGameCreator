using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.ProductLineRuntimeVariantMatrix;
using Xunit;

namespace LLMGameCreator.Tests.Application.ProductLineRuntimeVariantMatrix;

public sealed class ProductLineRuntimeVariantMatrixOperatorRunnerTests
{
    [Fact]
    public async Task FailedGenerationRestoresPreviousArtifactsByteForByte()
    {
        using var temp = new TempRepository();
        var previous = temp.WritePreviousArtifacts();
        var writer = new ThrowingWriter(temp.Path);
        var runner = new ProductLineRuntimeVariantMatrixOperatorRunner(writer);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(temp.Path));

        Assert.True(writer.AttemptedWriteCount >= 1);
        foreach (var item in previous)
        {
            Assert.True(File.Exists(item.Key));
            Assert.Equal(item.Value, File.ReadAllBytes(item.Key));
        }

        Assert.False(File.Exists(Path.Combine(
            temp.ProceduralRoot,
            "partial-green-dashboard.json")));
        Assert.False(File.Exists(Path.Combine(temp.ExportRoot, "partial-handoff.json")));
    }

    [Fact]
    public async Task SuccessfulGenerationRemovesStaleArtifactsAndKeepsNewMatrix()
    {
        using var temp = new TempRepository();
        temp.WritePreviousArtifacts();
        var stalePath = Path.Combine(temp.ProceduralRoot, "stale-artifact.json");
        File.WriteAllText(stalePath, "stale");
        var writer = new SuccessfulWriter(temp.Path);
        var runner = new ProductLineRuntimeVariantMatrixOperatorRunner(writer);

        var result = await runner.RunAsync(temp.Path);

        Assert.Equal("GREEN", result.Dashboard.MatrixStatus);
        Assert.False(File.Exists(stalePath));
        Assert.True(File.Exists(Path.Combine(
            temp.ProceduralRoot,
            ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName)));
        Assert.True(File.Exists(Path.Combine(
            temp.ExportRoot,
            "selected-runtime-variant",
            "selected-runtime-variant-handoff.json")));
    }

    private sealed class ThrowingWriter : IProductLineRuntimeVariantMatrixWriter
    {
        private readonly string _root;

        public ThrowingWriter(string root)
        {
            _root = root;
        }

        public int AttemptedWriteCount { get; private set; }

        public Task<ProductLineRuntimeVariantMatrixWriteResult> BuildAndWriteAsync(
            string repositoryRootPath,
            ProductLineRuntimeVariantMatrixRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            _ = repositoryRootPath;
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            var procedural = ArtifactRoot(
                _root,
                ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory);
            var export = ArtifactRoot(
                _root,
                ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory);
            Directory.CreateDirectory(procedural);
            Directory.CreateDirectory(export);
            File.WriteAllText(Path.Combine(procedural, "partial-green-dashboard.json"), "partial");
            AttemptedWriteCount++;
            File.WriteAllText(Path.Combine(export, "partial-handoff.json"), "partial");
            AttemptedWriteCount++;
            throw new InvalidOperationException("Injected failure after attempted writes.");
        }
    }

    private sealed class SuccessfulWriter : IProductLineRuntimeVariantMatrixWriter
    {
        private readonly string _root;

        public SuccessfulWriter(string root)
        {
            _root = root;
        }

        public Task<ProductLineRuntimeVariantMatrixWriteResult> BuildAndWriteAsync(
            string repositoryRootPath,
            ProductLineRuntimeVariantMatrixRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            _ = repositoryRootPath;
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            var procedural = ArtifactRoot(
                _root,
                ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory);
            var export = ArtifactRoot(
                _root,
                ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory);
            Directory.CreateDirectory(procedural);
            Directory.CreateDirectory(Path.Combine(export, "selected-runtime-variant"));
            File.WriteAllText(
                Path.Combine(procedural, ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName),
                "{\"matrixStatus\":\"GREEN\"}");
            File.WriteAllText(
                Path.Combine(export, "selected-runtime-variant", "selected-runtime-variant-handoff.json"),
                "{\"accepted\":false}");
            return Task.FromResult(new ProductLineRuntimeVariantMatrixWriteResult
            {
                Dashboard = new ProductLineRuntimeVariantMatrixDashboard
                {
                    MatrixStatus = "GREEN",
                    Accepted = false
                }
            });
        }
    }

    private sealed class TempRepository : IDisposable
    {
        public TempRepository()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "LLMGameCreator.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
            File.WriteAllText(System.IO.Path.Combine(Path, "LLMGameCreator.sln"), string.Empty);
        }

        public string Path { get; }
        public string ProceduralRoot => ArtifactRoot(
            Path,
            ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory);
        public string ExportRoot => ArtifactRoot(
            Path,
            ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory);

        public Dictionary<string, byte[]> WritePreviousArtifacts()
        {
            var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
            {
                [System.IO.Path.Combine(
                    ProceduralRoot,
                    ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName)] = [1, 2, 3, 4],
                [System.IO.Path.Combine(
                    ProceduralRoot,
                    ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName)] = [5, 6, 7, 8],
                [System.IO.Path.Combine(
                    ProceduralRoot,
                    "selected-runtime-variant",
                    "selected-runtime-variant-handoff.json")] = [9, 10, 11],
                [System.IO.Path.Combine(
                    ExportRoot,
                    ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName)] = [12, 13, 14],
                [System.IO.Path.Combine(
                    ExportRoot,
                    "selected-runtime-variant",
                    "selected-runtime-variant-handoff.json")] = [15, 16, 17]
            };
            foreach (var item in files)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(item.Key)!);
                File.WriteAllBytes(item.Key, item.Value);
            }

            return files;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private static string ArtifactRoot(string root, string relativePath) =>
        System.IO.Path.Combine(
            root,
            relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
}
