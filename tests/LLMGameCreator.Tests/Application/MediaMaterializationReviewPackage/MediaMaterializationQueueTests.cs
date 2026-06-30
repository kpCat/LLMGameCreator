using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationQueueTests
{
    [Fact]
    public void MaterializationQueueIsDeterministicAndCoversPromotedBindings()
    {
        var first = MediaMaterializationReviewPackageTestFactory.BuildFromRepo();
        var second = MediaMaterializationReviewPackageTestFactory.BuildFromRepo();

        Assert.True(first.MaterializationQueue.Passed);
        Assert.Equal(first.MaterializationQueue.QueueItemCount, first.SourceManifest.Goal053BindingCount);
        Assert.Equal(15, first.MaterializationQueue.QueueItemCount);
        Assert.Equal(
            first.MaterializationQueue.Items.Select(item => item.MaterializationId).ToArray(),
            second.MaterializationQueue.Items.Select(item => item.MaterializationId).ToArray());
        Assert.Equal(
            first.MaterializationQueue.Items.Select(item => item.ExpectedSha256).ToArray(),
            second.MaterializationQueue.Items.Select(item => item.ExpectedSha256).ToArray());

        Assert.Equal(
            first.MaterializationQueue.Items.OrderBy(item => item.DeterministicOrderingKey, StringComparer.Ordinal).Select(item => item.MaterializationId),
            first.MaterializationQueue.Items.Select(item => item.MaterializationId));

        foreach (var familyId in MediaMaterializationReviewPackageVocabulary.FamilyIds)
        {
            Assert.Contains(first.MaterializationQueue.Items, item => item.FamilyId == familyId && item.MediaKind == "image" && item.MaterializedMediaFormat == "png");
            Assert.Contains(first.MaterializationQueue.Items, item => item.FamilyId == familyId && item.MediaKind == "audio" && item.MaterializedMediaFormat == "wav_pcm_s16_mono");
            Assert.Contains(first.MaterializationQueue.Items, item => item.FamilyId == familyId && item.MediaKind == "ui" && item.MaterializedMediaFormat == "png");
            Assert.Contains(first.MaterializationQueue.Items, item => item.FamilyId == familyId && item.MediaKind == "bundle" && item.MaterializedMediaFormat == "bundle_manifest_json");
        }

        Assert.All(first.MaterializationQueue.Items, item =>
        {
            Assert.StartsWith("review-package/media/", item.OutputRelativePath, StringComparison.Ordinal);
            Assert.DoesNotContain(":", item.OutputRelativePath);
            Assert.DoesNotContain("..", item.OutputRelativePath);
            Assert.False(Path.IsPathRooted(item.OutputRelativePath));
            Assert.False(string.IsNullOrWhiteSpace(item.ExpectedSha256));
            Assert.True(item.ExpectedByteLength > 0);
        });
    }
}
