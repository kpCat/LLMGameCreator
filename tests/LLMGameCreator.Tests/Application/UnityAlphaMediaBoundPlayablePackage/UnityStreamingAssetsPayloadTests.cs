using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityStreamingAssetsPayloadTests
{
    [Fact]
    public void StagingManifestIsUnityStreamingAssetsCompatibleAndHashStable()
    {
        var first = UnityAlphaMediaBoundTestFactory.BuildFromRepo();
        var second = UnityAlphaMediaBoundTestFactory.BuildFromRepo();

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.StagingManifest.Passed);
        Assert.Equal(UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath, first.StagingManifest.ManifestRelativePath);
        Assert.Equal(15, first.StagingManifest.PhysicalMediaFileCount);
        Assert.Equal(9, first.StagingManifest.PngFileCount);
        Assert.Equal(3, first.StagingManifest.WavFileCount);
        Assert.Equal(3, first.StagingManifest.BundleFileCount);
        Assert.Equal(3, first.StagingManifest.FamilyCount);
        Assert.Contains(first.StagingFiles, item => item.RelativePath == "runtime/unity-runtime-config.json");
        Assert.Contains(first.StagingFiles, item => item.RelativePath == "game-data/game-package.json");
        Assert.Contains(first.StagingFiles, item => item.RelativePath == "assets/asset-manifest.json");
        Assert.Contains(first.StagingFiles, item => item.RelativePath == UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath);

        Assert.All(first.StagingManifest.Bindings, binding =>
        {
            Assert.StartsWith("media-bound/media/", binding.RelativePath, StringComparison.Ordinal);
            Assert.True(binding.SafeRelativePath);
            Assert.True(binding.HashMatchesGoal055);
            Assert.NotEmpty(binding.Sha256);
        });
    }
}
