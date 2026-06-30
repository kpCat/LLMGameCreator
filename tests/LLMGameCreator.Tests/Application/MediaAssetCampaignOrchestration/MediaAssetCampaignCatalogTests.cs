using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignCatalogTests
{
    [Fact]
    public void CatalogDefinesRequiredSlotsWithReviewAndLicensePolicy()
    {
        var result = MediaAssetCampaignTestFactory.BuildFromRepo();
        var catalog = result.SlotCatalog;

        Assert.True(catalog.Passed);
        Assert.Equal(MediaAssetCampaignVocabulary.RequiredSlotIds.Count, catalog.Slots.Count);
        foreach (var slotId in MediaAssetCampaignVocabulary.RequiredSlotIds)
        {
            Assert.Contains(catalog.Slots, slot => slot.SlotId == slotId);
        }

        Assert.Contains(catalog.Slots, slot => slot.MediaKind == "image");
        Assert.Contains(catalog.Slots, slot => slot.MediaKind == "audio");
        Assert.Contains(catalog.Slots, slot => slot.MediaKind == "ui");
        Assert.Contains(catalog.Slots, slot => slot.MediaKind == "bundle");
        Assert.All(catalog.Slots, slot =>
        {
            Assert.NotEmpty(slot.AllowedSourceTypes);
            Assert.NotEmpty(slot.ReviewRequirements);
            Assert.NotEmpty(slot.LicensePolicyRequirement);
            Assert.NotEmpty(slot.BindingTargetKind);
            Assert.NotEmpty(slot.FallbackPlaceholderBehavior);
        });
    }
}
