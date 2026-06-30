using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;
using Xunit;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyTemplateCatalogTests
{
    [Fact]
    public async Task CatalogConsumesGoal037To040AndDefinesThreeFamilies()
    {
        using var temp = await MultiFamilyGeneratedTemplateTestFactory.CreateProjectWithGoal037To040SourceAsync();

        var result = MultiFamilyGeneratedTemplateTestFactory.CreateService().Build(temp.Path);
        var catalog = result.Catalog;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(catalog.Accepted);
        Assert.True(catalog.Goal040AcceptedByUserHandoff);
        Assert.True(catalog.SourceGoal037HybridExpansionConsumed);
        Assert.True(catalog.SourceGoal038WorldMapConsumed);
        Assert.True(catalog.SourceGoal039RuntimeTraversalConsumed);
        Assert.True(catalog.SourceGoal040PreviewExportConsumed);
        Assert.Equal(3, catalog.FamilyCount);
        Assert.Contains(catalog.Families, item => item.FamilyId == "map_panel_rpg" && item.ScenarioId == "gothic_intrigue");
        Assert.Contains(catalog.Families, item => item.FamilyId == "survival_sandbox" && item.ScenarioId == "frontier_survival");
        Assert.Contains(catalog.Families, item => item.FamilyId == "first_person_grid_dungeon" && item.ScenarioId == "metamodule_kingdoms");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal034");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal035");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal036");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal037");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal038");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal039");
        Assert.Contains(catalog.SourceArtifactRefs, item => item.SourceGoal == "Goal040");
    }
}
