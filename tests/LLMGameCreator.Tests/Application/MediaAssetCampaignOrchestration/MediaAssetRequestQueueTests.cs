using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetRequestQueueTests
{
    [Fact]
    public void RequestQueueCoversThreeFamiliesRequiredKindsAndCompactedMetamoduleStress()
    {
        var queue = MediaAssetCampaignTestFactory.BuildFromRepo().RequestQueue;

        Assert.True(queue.Passed);
        Assert.Equal(3, queue.FamilyCount);
        Assert.True(queue.RequestCount >= 30);
        foreach (var familyId in MediaAssetCampaignVocabulary.FamilyIds)
        {
            Assert.True(queue.Requests.Count(item => item.FamilyId == familyId) >= 8);
        }

        Assert.Contains(queue.Requests, item => item.MediaKind == "image");
        Assert.Contains(queue.Requests, item => item.MediaKind == "audio");
        Assert.Contains(queue.Requests, item => item.MediaKind is "ui" or "bundle");
        Assert.All(queue.Requests, request =>
        {
            Assert.False(request.PromptInputSkeleton.FinalProviderPromptText);
            Assert.NotEmpty(request.TargetGeneratedId);
            Assert.NotEmpty(request.RequiredProvenancePolicy);
            Assert.NotEmpty(request.DeterministicOrderingKey);
        });

        Assert.Equal("metamodule_kingdoms", queue.MetamoduleStressSummary.ScenarioId);
        Assert.False(queue.MetamoduleStressSummary.OneRequestPerSpeciesArchetypeSlotGenerated);
        Assert.True(queue.MetamoduleStressSummary.CompactedSpeciesArchetypeSlotRefCount >= 112);
    }
}
