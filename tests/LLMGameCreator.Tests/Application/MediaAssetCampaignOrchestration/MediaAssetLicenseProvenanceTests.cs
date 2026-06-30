using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetLicenseProvenanceTests
{
    [Fact]
    public void LicenseLedgerBlocksRiskySourcesAndAutoPromotesOnlyFixtures()
    {
        var ledger = MediaAssetCampaignTestFactory.BuildFromRepo().LicenseLedger;

        Assert.True(ledger.Passed);
        foreach (var sourceKind in MediaAssetCampaignVocabulary.LicenseSourceKinds)
        {
            Assert.Contains(ledger.Policies, item => item.SourceKind == sourceKind);
        }

        Assert.Contains(ledger.Policies, item => item.SourceKind == "fixture-generated-by-repo" && item.CanAutoPromoteInGoal053);
        Assert.All(ledger.Policies.Where(item => item.SourceKind != "fixture-generated-by-repo"), item => Assert.False(item.CanAutoPromoteInGoal053));
        Assert.Contains(ledger.Policies, item => item.SourceKind == "imported-cc-by" && item.PromotionPolicy == "requires_attribution_record");
        Assert.Contains(ledger.Policies, item => item.SourceKind == "imported-share-alike-or-gpl-risk" && item.PromotionPolicy == "quarantine_or_block");
        Assert.Contains(ledger.Policies, item => item.SourceKind == "unknown/no-license" && item.PromotionPolicy == "reject");
        Assert.Contains(ledger.Policies, item => item.SourceKind == "provider-generated-with-model-license" && item.Goal053Decision.Contains("not allowed", StringComparison.OrdinalIgnoreCase));
    }
}
