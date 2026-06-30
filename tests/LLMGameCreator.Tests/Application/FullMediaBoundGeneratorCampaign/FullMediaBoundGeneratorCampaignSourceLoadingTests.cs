using LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignSourceLoadingTests
{
    [Fact]
    public void Goal058ConsumesGoal057HandoffAndRequiredSourceChain()
    {
        var result = FullMediaBoundGeneratorCampaignTestFactory.BuildFromRepo();
        var manifest = result.SourceManifest;

        Assert.False(manifest.Accepted);
        Assert.True(manifest.Goal057AcceptedByUserHandoff);
        Assert.True(manifest.Goal057ReportWasGreenProducedForReview);
        Assert.True(manifest.Goal057UnityProofPassed);
        Assert.True(manifest.SourceArtifactCount >= 30);
        Assert.Equal(3, manifest.FamilyCount);
        foreach (var familyId in FullMediaBoundGeneratorCampaignVocabulary.FamilyIds)
        {
            Assert.Contains(familyId, manifest.SelectedFamilyIds);
        }

        foreach (var sourceGoal in new[] { "Goal043", "Goal047", "Goal053", "Goal054", "Goal055", "Goal056", "Goal057" })
        {
            Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == sourceGoal && item.Exists && item.HashMatches);
        }

        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "unity_alpha_multifamily_playable_loop_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == FullMediaBoundGeneratorCampaignVocabulary.FinalGate
            && item.Status == "required");
    }
}
