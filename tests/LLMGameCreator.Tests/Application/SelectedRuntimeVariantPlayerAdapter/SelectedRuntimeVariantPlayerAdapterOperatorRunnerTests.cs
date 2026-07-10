using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantPlayerAdapter;
using Xunit;

namespace LLMGameCreator.Tests.Application.SelectedRuntimeVariantPlayerAdapter;

public sealed class SelectedRuntimeVariantPlayerAdapterOperatorRunnerTests
{
    [Fact]
    public async Task FailureAfterWritesRestoresPreviousArtifactsByteForByte()
    {
        using var temp = new TempRepository();
        var previous = temp.WritePreviousArtifacts();
        var writer = new ThrowingWriter(temp.Root);
        var runner = new SelectedRuntimeVariantPlayerAdapterOperatorRunner(writer);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(temp.Root));

        Assert.True(writer.AttemptedWriteCount >= 2);
        foreach (var item in previous)
        {
            Assert.True(File.Exists(item.Key));
            Assert.Equal(item.Value, File.ReadAllBytes(item.Key));
        }

        Assert.False(File.Exists(Path.Combine(temp.ProceduralRoot, "partial-result.json")));
        Assert.False(File.Exists(Path.Combine(temp.ExportRoot, "partial-handoff.json")));
    }

    private sealed class ThrowingWriter : ISelectedRuntimeVariantPlayerAdapterWriter
    {
        private readonly string _root;

        public ThrowingWriter(string root)
        {
            _root = root;
        }

        public int AttemptedWriteCount { get; private set; }

        public Task<SelectedRuntimeVariantPlayerAdapterWriteResult> BuildAndWriteAsync(
            string repositoryRootPath,
            SelectedRuntimeVariantPlayerAdapterRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            _ = repositoryRootPath;
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            var procedural = ArtifactRoot(
                _root,
                SelectedRuntimeVariantPlayerAdapterVocabulary.ProceduralOutputDirectory);
            var export = ArtifactRoot(
                _root,
                SelectedRuntimeVariantPlayerAdapterVocabulary.ExportPackageDirectory);
            Directory.CreateDirectory(procedural);
            Directory.CreateDirectory(export);
            File.WriteAllText(Path.Combine(procedural, "partial-result.json"), "partial");
            AttemptedWriteCount++;
            File.WriteAllText(Path.Combine(export, "partial-handoff.json"), "partial");
            AttemptedWriteCount++;
            throw new InvalidOperationException("Injected failure after attempted writes.");
        }
    }

    private sealed class TempRepository : IDisposable
    {
        public TempRepository()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "LLMGameCreator.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, "LLMGameCreator.sln"), string.Empty);
        }

        public string Root { get; }
        public string ProceduralRoot => ArtifactRoot(
            Root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.ProceduralOutputDirectory);
        public string ExportRoot => ArtifactRoot(
            Root,
            SelectedRuntimeVariantPlayerAdapterVocabulary.ExportPackageDirectory);

        public Dictionary<string, byte[]> WritePreviousArtifacts()
        {
            var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
            {
                [Path.Combine(
                    ProceduralRoot,
                    SelectedRuntimeVariantPlayerAdapterVocabulary.DashboardFileName)] = [1, 2, 3],
                [Path.Combine(
                    ProceduralRoot,
                    SelectedRuntimeVariantPlayerAdapterVocabulary.HandoffFileName)] = [4, 5, 6],
                [Path.Combine(
                    ExportRoot,
                    SelectedRuntimeVariantPlayerAdapterVocabulary.DashboardFileName)] = [7, 8, 9]
            };
            foreach (var item in files)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item.Key)!);
                File.WriteAllBytes(item.Key, item.Value);
            }

            return files;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }

    private static string ArtifactRoot(string root, string relativePath) =>
        Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
}
