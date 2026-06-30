using Xunit;

namespace LLMGameCreator.Tests.Application.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundSourceLoadingTests
{
    [Fact]
    public void Goal055SourceManifestLoadsGoal047Goal053AndGoal054Facts()
    {
        var result = MediaBoundPlayableReviewPackageTestFactory.BuildFromRepo();
        var manifest = result.SourceManifest;

        Assert.False(manifest.Accepted);
        Assert.True(manifest.Goal054AcceptedByUserHandoff);
        Assert.True(manifest.Goal054ReportWasGreenProducedForReview);
        Assert.Equal(3, manifest.Goal047FamilyDryRunCount);
        Assert.Equal(15, manifest.Goal053BindingCount);
        Assert.Equal(15, manifest.Goal054PhysicalMediaCount);
        Assert.Equal(9, manifest.Goal054PngCount);
        Assert.Equal(3, manifest.Goal054WavCount);
        Assert.Equal(3, manifest.Goal054BundleJsonCount);
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal047");
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal053");
        Assert.Contains(manifest.SourceArtifactRefs, item => item.SourceGoal == "Goal054");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "media_materialization_review_package_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "media_bound_playable_review_package_verification"
            && item.Status == "required");
    }
}
