using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaBoundPlayableReviewPackage;

public sealed class MediaBoundManifestPayloadTests
{
    [Fact]
    public void ReviewStreamingAndPreviewPayloadsAreStableAndFamilyComplete()
    {
        var first = MediaBoundPlayableReviewPackageTestFactory.BuildFromRepo();
        var second = MediaBoundPlayableReviewPackageTestFactory.BuildFromRepo();

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.ReviewPackageManifest.Passed);
        Assert.True(first.StreamingAssetsManifest.Passed);
        Assert.True(first.PreviewPayloads.Passed);
        Assert.Equal(15, first.StreamingAssetsManifest.BindingCount);
        Assert.Equal("review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media-bound-playable-manifest.json", first.StreamingAssetsManifest.ManifestRelativePath);
        Assert.Contains(first.PackageTextFiles, item => item.RelativePath == "review-package/README.md");
        Assert.Contains(first.PackageTextFiles, item => item.RelativePath == "review-package/CHECKLIST.md");
        Assert.Contains(first.PackageTextFiles, item => item.RelativePath == "review-package/media-bound-playable-manifest.json");

        Assert.All(first.PreviewPayloads.Payloads, payload =>
        {
            Assert.Equal("passed", payload.ValidationStatus);
            Assert.NotEmpty(payload.ReferencedDryRunArtifactRef);
            Assert.NotEmpty(payload.Goal054PreviewPayloadId);
            Assert.Equal(5, payload.StagedMediaRefs.Count);
            Assert.EndsWith(MediaBoundPlayableReviewPackageEvidenceService.UnityLoadContractJsonFileName, payload.UnityLoadContractRef, StringComparison.Ordinal);
            Assert.StartsWith("unity-media-load-proof-", payload.UnityLoadProofRef, StringComparison.Ordinal);
        });

        Assert.Equal(
            first.ReviewPackageManifest.StagedFiles.Select(item => item.StagedRelativePath).ToArray(),
            second.ReviewPackageManifest.StagedFiles.Select(item => item.StagedRelativePath).ToArray());
    }
}
