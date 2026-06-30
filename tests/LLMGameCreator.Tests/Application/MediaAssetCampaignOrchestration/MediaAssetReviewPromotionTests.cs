using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetReviewPromotionTests
{
    [Fact]
    public void ReviewPromotionDecisionsAreDeterministicAndCausal()
    {
        var first = MediaAssetCampaignTestFactory.BuildFromRepo();
        var second = MediaAssetCampaignTestFactory.BuildFromRepo();
        var ledger = first.ReviewPromotionLedger;

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(ledger.Passed);
        Assert.True(ledger.Deterministic);
        foreach (var decision in MediaAssetCampaignVocabulary.RequiredReviewDecisions)
        {
            Assert.Contains(ledger.Decisions, item => item.Decision == decision);
        }

        var candidates = first.CandidateQuarantine.Candidates.ToDictionary(item => item.CandidateId, StringComparer.Ordinal);
        Assert.All(ledger.Decisions.Where(item => item.Promoted), decision =>
        {
            Assert.Equal("promote_fixture", decision.Decision);
            Assert.Equal("fixture-generated-by-repo", candidates[decision.CandidateId].SourceKind);
            Assert.Contains(decision.Diagnostics, item => item.Code == "goal053.review.fixture_promoted");
        });
        Assert.Contains(ledger.Decisions, item => item.Decision == "blocked_license" && item.CauseCode == "goal053.license.share_alike_or_gpl_risk");
        Assert.Contains(ledger.Decisions, item => item.Decision == "blocked_provider_not_configured" && item.CauseCode == "goal053.provider.metadata_missing");
        Assert.Contains(ledger.Decisions, item => item.Decision == "blocked_leak" && item.CauseCode == "goal053.review.leak_claim");
    }
}
