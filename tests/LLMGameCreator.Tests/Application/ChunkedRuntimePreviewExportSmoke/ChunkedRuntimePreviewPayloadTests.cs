using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using Xunit;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimePreviewPayloadTests
{
    [Fact]
    public async Task SourceGoal039FactsAreConsumedIntoNonCopyPayloads()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();

        var result = ChunkedRuntimePreviewExportTestFactory.CreateService().Build(temp.Path);

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.Report.Goal039AcceptedByUserHandoff);
        Assert.Equal(4, result.Payloads.Count);
        Assert.All(result.Payloads, payload =>
        {
            Assert.Equal(ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId, payload.CorePayloadSchemaId);
            Assert.True(payload.SourceEvidence.ConsumesGoal039RuntimeDeltaCommands);
            Assert.True(payload.SourceEvidence.ConsumesGoal039SaveLoadProof);
            Assert.True(payload.SourceEvidence.ConsumesGoal039ReplayProof);
            Assert.False(payload.SourceEvidence.PayloadIsSourceJsonCopy);
            Assert.NotEqual(payload.SourceEvidence.SourcePlanHash, payload.PayloadHash);
            Assert.NotEmpty(payload.ChunkIds);
            Assert.NotEmpty(payload.TraversalRoute);
            Assert.NotEmpty(payload.RuntimeDeltaMarkers);
            Assert.NotEmpty(payload.LandmarkDiscoveryIds);
            Assert.NotEmpty(payload.MutationMarkers);
            Assert.Contains(payload.RuntimeDeltaMarkers, item => item.DeltaKind == "deterministic_replay_marker");
        });

        var source = new ChunkedRuntimePreviewExportSourceLoader().Load(temp.Path);
        var frontierPlanJson = source.ArtifactTextByFileName[RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName];
        var frontierPayloadJson = result.ArtifactJsonByFileName[ChunkedRuntimePreviewExportEvidenceService.FrontierPayloadJsonFileName];
        Assert.NotEqual(frontierPlanJson, frontierPayloadJson);
        Assert.Contains("chunked_preview_export_payload_v1", frontierPayloadJson);
    }
}
