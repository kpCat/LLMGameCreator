using LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;
using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilyCommandPlanTests
{
    [Fact]
    public void CommandPlanStagesGoal056MediaAndThreeFamilyLoopMarkers()
    {
        var result = UnityAlphaMultiFamilyTestFactory.BuildFromRepo();

        Assert.True(result.FamilyModeManifest.Passed);
        Assert.Equal(3, result.FamilyModeManifest.FamilyCount);
        Assert.True(result.UnityStagingManifest.Passed);
        Assert.True(result.FamilyCommandPlan.Passed);
        Assert.False(result.FamilyCommandPlan.Accepted);
        Assert.Equal(3, result.FamilyCommandPlan.FamilyModes.Count);
        Assert.True(result.FamilyCommandPlan.Commands.Count >= 15);
        Assert.Contains(result.StagingFiles, item => item.RelativePath == UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath);
        Assert.Contains(result.StagingFiles, item => item.RelativePath == UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyCommandPlanStagingRelativePath);

        foreach (var familyId in UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds)
        {
            Assert.Contains(result.FamilyCommandPlan.ExpectedPlayerMarkers, marker => marker == "family_scenario_loaded=" + familyId);
            Assert.Contains(result.FamilyCommandPlan.ExpectedPlayerMarkers, marker => marker == "family_mode_selected=" + familyId);
            Assert.Contains(result.FamilyCommandPlan.ExpectedPlayerMarkers, marker => marker == "family_loop_completed=" + familyId);
            Assert.True(result.FamilyCommandPlan.Commands.Count(command => command.FamilyId == familyId) >= 5);
        }

        Assert.Contains("media_bound_manifest_loaded=true", result.FamilyCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("media_bound_hash_validation=true", result.FamilyCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("review_package_proof=goal057", result.FamilyCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("unity_alpha_multifamily_playable_loop_verification=required", result.FamilyCommandPlan.ExpectedPlayerMarkers);
    }
}
