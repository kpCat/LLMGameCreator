using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public static class ChunkedRuntimePreviewExportTestFactory
{
    public static async Task<TempGoal040Project> CreateProjectWithGoal039SourceAsync()
    {
        var temp = new TempGoal040Project();
        await RuntimeChunkDeltaTraversalTestFactory.CreateService().BuildAndWriteAsync(temp.Path);
        return temp;
    }

    public static ChunkedRuntimePreviewExportEvidenceService CreateService() => new();
}

public sealed class TempGoal040Project : IDisposable
{
    public TempGoal040Project()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
