using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using Xunit;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimeExportManifestTests
{
    [Fact]
    public async Task ExportManifestIsStableAndReferencesScenarioPayloads()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();
        var service = ChunkedRuntimePreviewExportTestFactory.CreateService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);

        Assert.Equal(first.ExportManifest.ManifestHash, second.ExportManifest.ManifestHash);
        Assert.Equal(4, first.ExportManifest.Payloads.Count);
        Assert.True(first.ExportManifest.UsesGoal039RuntimeDeltas);
        Assert.True(first.ExportManifest.RuntimePreviewCompatible);
        Assert.True(first.ExportManifest.UnityExportCompatible);
        Assert.All(first.ExportManifest.Payloads, entry =>
        {
            var payload = first.Payloads.Single(item => item.ScenarioId == entry.ScenarioId);
            Assert.Equal(payload.PayloadHash, entry.PayloadHash);
            Assert.EndsWith(ChunkedRuntimePreviewExportVocabulary.PayloadFileNamesByScenario[entry.ScenarioId], entry.PayloadPath, StringComparison.Ordinal);
            Assert.True(entry.PreviewReady);
            Assert.True(entry.ExportReady);
        });
        Assert.Contains("runtime_preview_route_integration_future_required", first.ExportManifest.FutureRequiredIntegrationGaps);
    }
}
