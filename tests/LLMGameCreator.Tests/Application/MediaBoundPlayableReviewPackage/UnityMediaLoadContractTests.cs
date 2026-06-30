using Xunit;

namespace LLMGameCreator.Tests.Application.MediaBoundPlayableReviewPackage;

public sealed class UnityMediaLoadContractTests
{
    [Fact]
    public void UnityCompatibleContractAndProofRecordsExposeRequiredLinesWithoutClaimingBuildExecution()
    {
        var result = MediaBoundPlayableReviewPackageTestFactory.BuildFromRepo();

        Assert.True(result.UnityLoadContract.Passed);
        Assert.False(result.UnityLoadContract.UnitySourceChanged);
        Assert.False(result.UnityLoadContract.UnityBuildOrPlayerExecuted);
        Assert.Equal("Application.streamingAssetsPath", result.UnityLoadContract.ReadSurface);
        Assert.Contains("UnityEngine.ImageConversion.LoadImage", result.UnityLoadContract.ImageLoadApi);
        Assert.Contains("no_playback_claim", result.UnityLoadContract.WavValidationMode);

        foreach (var proof in result.UnityLoadProofs)
        {
            Assert.True(proof.Passed);
            Assert.True(proof.ManifestLoaded);
            Assert.True(proof.ImageLoaded);
            Assert.True(proof.WavValidated);
            Assert.True(proof.FamilyPanelReady);
            Assert.Contains(proof.ProofLines, line => line == "MEDIA_BOUND_MANIFEST_LOADED family=" + proof.FamilyId);
            Assert.Contains(proof.ProofLines, line => line.StartsWith("MEDIA_BOUND_IMAGE_LOADED family=" + proof.FamilyId, StringComparison.Ordinal));
            Assert.Contains(proof.ProofLines, line => line.StartsWith("MEDIA_BOUND_WAV_VALIDATED family=" + proof.FamilyId, StringComparison.Ordinal));
            Assert.Contains(proof.ProofLines, line => line == "MEDIA_BOUND_FAMILY_PANEL_READY family=" + proof.FamilyId);
        }
    }
}
