using LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice;
using Xunit;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyLifecycleBuilderTests
{
    [Fact]
    public async Task LifecyclePlansUseSharedPhasesAndGoal037To040Refs()
    {
        using var temp = await MultiFamilyGeneratedTemplateTestFactory.CreateProjectWithGoal037To040SourceAsync();

        var result = MultiFamilyGeneratedTemplateTestFactory.CreateService().Build(temp.Path);

        Assert.Equal(3, result.Plans.Count);
        Assert.All(result.Plans, plan =>
        {
            Assert.Equal(MultiFamilyGeneratedTemplateVocabulary.SharedLifecycleContractId, plan.SharedLifecycleContractId);
            Assert.Equal(MultiFamilyGeneratedTemplateVocabulary.SharedLifecyclePhases, plan.LifecyclePhases);
            Assert.Empty(plan.UnscopedFamilySpecificFields);
            Assert.False(plan.ArchitectureForkAttempted);
            Assert.False(plan.FinalProsePromotedAsPlayableContent);
            Assert.True(plan.BoundaryClaims.AllFalse);
            Assert.NotEmpty(plan.SelectedFeatureRefs);
            Assert.NotEmpty(plan.SelectedIntentionRefs);
            Assert.NotEmpty(plan.LoopCommands);
            Assert.NotEmpty(plan.ValidationTrace);

            foreach (var sourceGoal in new[] { "Goal034", "Goal035", "Goal036", "Goal037", "Goal038", "Goal039", "Goal040" })
            {
                Assert.Contains(plan.SourceReferences, item => item.SourceGoal == sourceGoal);
            }
        });

        Assert.Contains(result.Plans, item => item.FamilyId == "map_panel_rpg" && item.FamilyExtension.LoopMarkers.Contains("quest_event_progress_marker"));
        Assert.Contains(result.Plans, item => item.FamilyId == "survival_sandbox" && item.FamilyExtension.LoopMarkers.Contains("chunk_context_state_change_marker"));
        Assert.Contains(result.Plans, item => item.FamilyId == "first_person_grid_dungeon" && item.FamilyExtension.LoopMarkers.Contains("party_blob_traversal_marker"));
    }
}
