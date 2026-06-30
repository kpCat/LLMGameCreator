using Xunit;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class FamilySimulatableLoopRunnerTests
{
    [Fact]
    public async Task ThreeFamilyLoopProofsHaveStateTransitionsBlockedInvalidActionsAndReplayHashes()
    {
        using var temp = await MultiFamilyGeneratedTemplateTestFactory.CreateProjectWithGoal037To040SourceAsync();

        var result = MultiFamilyGeneratedTemplateTestFactory.CreateService().Build(temp.Path);

        Assert.Equal(3, result.LoopProofs.Count);
        Assert.All(result.LoopProofs, proof =>
        {
            Assert.True(proof.StateChanged);
            Assert.True(proof.FamilySpecificMinimumsPassed);
            Assert.True(proof.BlockedInvalidAction.Blocked);
            Assert.NotEmpty(proof.Events);
            Assert.NotEmpty(proof.ChangedMarkers);
            Assert.False(string.IsNullOrWhiteSpace(proof.ReplayDeterminismHash));
            Assert.Equal(proof.ReplayDeterminismHash, proof.ReplayedDeterminismHash);
        });

        Assert.Contains(result.LoopProofs, item => item.FamilyId == "map_panel_rpg"
            && item.ChangedMarkers.Contains("movement_traversal_marker")
            && item.ChangedMarkers.Contains("focused_target_marker")
            && item.ChangedMarkers.Contains("quest_event_progress_marker"));
        Assert.Contains(result.LoopProofs, item => item.FamilyId == "survival_sandbox"
            && item.ChangedMarkers.Contains("hazard_resource_observation_marker")
            && item.ChangedMarkers.Contains("collect_consume_craft_survival_marker")
            && item.ChangedMarkers.Contains("chunk_context_state_change_marker"));
        Assert.Contains(result.LoopProofs, item => item.FamilyId == "first_person_grid_dungeon"
            && item.ChangedMarkers.Contains("orientation_corridor_room_marker")
            && item.ChangedMarkers.Contains("encounter_locked_route_pressure_marker")
            && item.ChangedMarkers.Contains("party_blob_traversal_marker"));
    }
}
