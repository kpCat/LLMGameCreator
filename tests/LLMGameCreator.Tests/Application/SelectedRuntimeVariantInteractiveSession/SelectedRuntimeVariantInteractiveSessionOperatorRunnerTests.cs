using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.SelectedRuntimeVariantInteractiveSession;
using Xunit;

namespace LLMGameCreator.Tests.Application.SelectedRuntimeVariantInteractiveSession;

public sealed class SelectedRuntimeVariantInteractiveSessionOperatorRunnerTests
{
    [Fact]
    public async Task FailedDrillRestoresPreviousProceduralAndExportArtifacts()
    {
        using var temp = new TempRepository();
        var previous = temp.WritePrevious();
        var runner = new SelectedRuntimeVariantInteractiveSessionOperatorRunner(
            new ThrowingWriter(temp.Root));
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(temp.Root));
        foreach (var item in previous) Assert.Equal(item.Value, File.ReadAllBytes(item.Key));
        Assert.False(File.Exists(Path.Combine(temp.Procedural, "partial.json")));
    }

    private sealed class ThrowingWriter : ISelectedRuntimeVariantInteractiveSessionWriter
    {
        private readonly string _root;
        public ThrowingWriter(string root) => _root = root;
        public Task<SelectedRuntimeVariantLiveSessionWriteResult> RunDrillAndWriteAsync(
            string repositoryRootPath,
            SelectedRuntimeVariantInteractiveSessionRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            _ = repositoryRootPath;
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(_root,
                SelectedRuntimeVariantInteractiveSessionVocabulary.ProceduralOutputDirectory
                    .Replace('/', Path.DirectorySeparatorChar), "partial.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "partial");
            throw new InvalidOperationException("injected Goal144 failure");
        }
    }

    private sealed class TempRepository : IDisposable
    {
        public TempRepository()
        {
            Root = Path.Combine(Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, "LLMGameCreator.sln"), string.Empty);
        }
        public string Root { get; }
        public string Procedural => Path.Combine(Root,
            SelectedRuntimeVariantInteractiveSessionVocabulary.ProceduralOutputDirectory
                .Replace('/', Path.DirectorySeparatorChar));
        public string Export => Path.Combine(Root,
            SelectedRuntimeVariantInteractiveSessionVocabulary.ExportPackageDirectory
                .Replace('/', Path.DirectorySeparatorChar));
        public Dictionary<string, byte[]> WritePrevious()
        {
            var files = new Dictionary<string, byte[]>
            {
                [Path.Combine(Procedural, "previous.json")] = [1, 2, 3],
                [Path.Combine(Export, "previous.json")] = [4, 5, 6]
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
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }
    }
}
