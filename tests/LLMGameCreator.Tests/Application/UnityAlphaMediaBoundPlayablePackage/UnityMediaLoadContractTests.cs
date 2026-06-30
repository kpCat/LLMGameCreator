using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityMediaLoadContractTests
{
    [Fact]
    public void UnityLoadContractRequiresStableMediaBoundMarkersWithoutFakingExecution()
    {
        var result = UnityAlphaMediaBoundTestFactory.BuildFromRepo();

        Assert.True(result.UnityLoadContract.Passed);
        Assert.Equal("Application.streamingAssetsPath", result.UnityLoadContract.ReadSurface);
        Assert.Equal(15, result.UnityLoadContract.ExpectedBindings.Count);
        Assert.Contains("media_bound_manifest_loaded=true", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_family_count=3", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_png_loaded=true", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_wav_loaded=true", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_bundle_loaded=true", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_hash_validation=true", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_playable_review_package_verification=required", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_family_panel_proof=map_panel_rpg", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_family_panel_proof=survival_sandbox", result.UnityLoadContract.RequiredLogMarkers);
        Assert.Contains("media_bound_family_panel_proof=first_person_grid_dungeon", result.UnityLoadContract.RequiredLogMarkers);

        Assert.False(result.UnityLoadProof.Passed);
        Assert.False(result.UnityLoadProof.UnityEditorOrPlayerExecuted);
        Assert.NotEmpty(result.UnityLoadProof.BlockerCode);
    }
}
