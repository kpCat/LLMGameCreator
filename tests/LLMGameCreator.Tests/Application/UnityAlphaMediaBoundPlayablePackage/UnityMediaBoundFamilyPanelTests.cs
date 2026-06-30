using LLMGameCreator.Application.Design.UnityAlphaMediaBoundPlayablePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.UnityAlphaMediaBoundPlayablePackage;

public sealed class UnityMediaBoundFamilyPanelTests
{
    [Fact]
    public void FamilyPanelModelsCoverAllThreeGoal056Families()
    {
        var result = UnityAlphaMediaBoundTestFactory.BuildFromRepo();

        Assert.True(result.FamilyPanelModels.Passed);
        Assert.Equal(3, result.FamilyPanelModels.FamilyCount);
        foreach (var familyId in UnityAlphaMediaBoundPlayablePackageVocabulary.FamilyIds)
        {
            var panel = Assert.Single(result.FamilyPanelModels.Families, item => item.FamilyId == familyId);
            Assert.Equal("media_bound_family_panel_proof=" + familyId, panel.PanelProofMarker);
            Assert.NotEmpty(panel.ImageBindingId);
            Assert.NotEmpty(panel.WavBindingId);
            Assert.NotEmpty(panel.BundleBindingId);
            Assert.Equal(5, panel.BindingIds.Count);
        }

        Assert.True(result.PreviewExportPayloads.Passed);
        Assert.All(result.PreviewExportPayloads.Payloads, payload =>
        {
            Assert.Equal(UnityAlphaMediaBoundPlayablePackageVocabulary.UnityManifestRelativePath, payload.UnityManifestRef);
            Assert.StartsWith("media_bound_family_panel_proof=", payload.PanelProofMarker, StringComparison.Ordinal);
            Assert.Equal(5, payload.BindingIds.Count);
        });
    }
}
