using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.HybridDraftLuaExpansion;
using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;
using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public static class MultiFamilyGeneratedTemplateTestFactory
{
    public static async Task<TempGoal043Project> CreateProjectWithGoal037To040SourceAsync()
    {
        var temp = new TempGoal043Project();
        await new HybridDraftLuaExpansionEvidenceService().BuildAndWriteAsync(temp.Path);
        await new WorldScaleRegionMapEvidenceService().BuildAndWriteAsync(temp.Path);
        await RuntimeChunkDeltaTraversalTestFactory.CreateService().BuildAndWriteAsync(temp.Path);
        await new ChunkedRuntimePreviewExportEvidenceService().BuildAndWriteAsync(temp.Path);
        return temp;
    }

    public static MultiFamilyGeneratedTemplateEvidenceService CreateService() => new();
}

public sealed class TempGoal043Project : IDisposable
{
    public TempGoal043Project()
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
