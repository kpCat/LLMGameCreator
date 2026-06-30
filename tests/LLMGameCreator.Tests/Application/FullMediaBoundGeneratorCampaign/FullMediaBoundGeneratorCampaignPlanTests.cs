using LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignPlanTests
{
    [Fact]
    public void CampaignPlanStagesReviewPackageAndUnityCampaignMarkers()
    {
        var result = FullMediaBoundGeneratorCampaignTestFactory.BuildFromRepo();

        Assert.True(result.CampaignPlan.Passed);
        Assert.False(result.CampaignPlan.Accepted);
        Assert.Equal(3, result.CampaignPlan.FamilyCount);
        Assert.Equal(FullMediaBoundGeneratorCampaignVocabulary.StageIds.Count, result.CampaignPlan.StageCount);
        foreach (var stageId in FullMediaBoundGeneratorCampaignVocabulary.StageIds)
        {
            Assert.Contains(result.CampaignPlan.Stages, item => item.StageId == stageId && item.Passed);
        }

        Assert.True(result.ReviewPackageManifest.Passed);
        Assert.False(result.ReviewPackageManifest.Accepted);
        Assert.Contains("review-package/StreamingAssets/full-media-bound-campaign-manifest.json", result.ReviewPackageManifest.StreamingAssetsFiles);
        Assert.Contains("review-package/StreamingAssets/family-command-plan.json", result.ReviewPackageManifest.StreamingAssetsFiles);
        Assert.Contains("review-package/StreamingAssets/media-bound-manifest.json", result.ReviewPackageManifest.StreamingAssetsFiles);
        Assert.Contains(result.StagingFiles, item => item.RelativePath == FullMediaBoundGeneratorCampaignVocabulary.CampaignManifestStagingRelativePath);
        Assert.Contains(result.StagingFiles, item => item.RelativePath == FullMediaBoundGeneratorCampaignVocabulary.CampaignCommandPlanStagingRelativePath);

        Assert.True(result.UnityCommandPlan.Passed);
        Assert.Contains("campaign_loaded=goal058", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("campaign_media_bound=true", result.UnityCommandPlan.ExpectedPlayerMarkers);
        Assert.Contains("campaign_review_package_proof=goal058", result.UnityCommandPlan.ExpectedPlayerMarkers);
        foreach (var familyId in FullMediaBoundGeneratorCampaignVocabulary.FamilyIds)
        {
            Assert.Contains("campaign_family=" + familyId, result.UnityCommandPlan.ExpectedPlayerMarkers);
            Assert.Contains("campaign_family_completed=" + familyId, result.UnityCommandPlan.ExpectedPlayerMarkers);
            Assert.Contains(result.FamilyRunsByFamilyId.Values, item => item.FamilyId == familyId && item.Passed && item.MediaFileCount >= 5);
        }
    }
}
