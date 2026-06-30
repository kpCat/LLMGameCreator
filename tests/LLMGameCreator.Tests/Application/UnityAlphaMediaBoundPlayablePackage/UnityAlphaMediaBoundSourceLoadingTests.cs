using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityAlphaMediaBoundSourceLoadingTests
{
    [Fact]
    public void Goal056ConsumesGoal055AndPreservesPriorGoalRefs()
    {
        var result = UnityAlphaMediaBoundTestFactory.BuildFromRepo();
        var manifest = result.SourceManifest;

        Assert.False(manifest.Accepted);
        Assert.True(manifest.Goal055AcceptedByUserHandoff);
        Assert.True(manifest.Goal055ReportWasGreenProducedForReview);
        Assert.True(manifest.BaseAlphaPayloadFound);
        Assert.Equal(15, manifest.Goal055PhysicalMediaFileCount);
        Assert.Equal(9, manifest.Goal055PngFileCount);
        Assert.Equal(3, manifest.Goal055WavFileCount);
        Assert.Equal(3, manifest.Goal055BundleFileCount);
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "media_bound_playable_review_package_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "unity_alpha_media_bound_playable_package_verification"
            && item.Status == "required");
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal047" && item.Exists && item.HashMatches);
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal054" && item.Exists && item.HashMatches);
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal055" && item.Exists && item.HashMatches);
    }
}
