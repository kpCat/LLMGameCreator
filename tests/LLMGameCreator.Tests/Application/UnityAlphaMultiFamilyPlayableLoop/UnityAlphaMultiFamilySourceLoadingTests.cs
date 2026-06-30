using LLMGameCreator.Application.Design.UnityAlphaMultiFamilyPlayableLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMultiFamilyPlayableLoop;

public sealed class UnityAlphaMultiFamilySourceLoadingTests
{
    [Fact]
    public void Goal057ConsumesGoal056HandoffAndAllFamilyEvidence()
    {
        var result = UnityAlphaMultiFamilyTestFactory.BuildFromRepo();
        var manifest = result.SourceManifest;

        Assert.False(manifest.Accepted);
        Assert.True(manifest.Goal056AcceptedByUserHandoff);
        Assert.True(manifest.Goal056ReportWasGreenProducedForReview);
        Assert.True(manifest.Goal056UnityProofPassed);
        Assert.Equal(3, manifest.FamilyCount);
        foreach (var familyId in UnityAlphaMultiFamilyPlayableLoopVocabulary.FamilyIds)
        {
            Assert.Contains(familyId, manifest.SelectedFamilyIds);
        }

        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "unity_alpha_media_bound_playable_package_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "unity_alpha_multifamily_playable_loop_verification"
            && item.Status == "required");
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal043" && item.Exists && item.HashMatches);
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal047" && item.Exists && item.HashMatches);
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal055" && item.Exists && item.HashMatches);
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal056" && item.Exists && item.HashMatches);
    }
}
