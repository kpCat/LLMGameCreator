using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using Xunit;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public sealed class MultiFamilyWorldScaleRegressionTests
{
    [Fact]
    public async Task ThreeFamilyLensesReuseSameCorePayloadSchema()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();

        var result = ChunkedRuntimePreviewExportTestFactory.CreateService().Build(temp.Path);
        var matrix = result.MultiFamilyMatrix;

        Assert.True(matrix.Passed);
        Assert.Equal(3, matrix.FamilyLensCount);
        Assert.Equal(4, matrix.ScenarioCount);
        Assert.Contains(matrix.FamilyLenses, item => item.FamilyLensId == "map_panel_rpg" && item.ExpectedConsumerNeeds.Contains("region_panel_sequence"));
        Assert.Contains(matrix.FamilyLenses, item => item.FamilyLensId == "survival_sandbox" && item.ExpectedConsumerNeeds.Contains("hazard_resource_traversal_hints"));
        Assert.Contains(matrix.FamilyLenses, item => item.FamilyLensId == "first_person_grid_dungeon" && item.ExpectedConsumerNeeds.Contains("corridor_room_route_orientation"));
        Assert.All(matrix.FamilyLenses, lens =>
        {
            Assert.Equal(ChunkedRuntimePreviewExportVocabulary.CorePayloadSchemaId, lens.CorePayloadSchemaId);
            Assert.False(lens.ForksCoreTraversalSchema);
            Assert.NotEmpty(lens.ExpectedConsumerNeeds);
        });
        Assert.All(matrix.ScenarioReuse, scenario =>
        {
            Assert.True(scenario.ReusesSameCoreTraversalPayload);
            Assert.Equal(3, scenario.FamilyLensIds.Count);
        });
    }
}
