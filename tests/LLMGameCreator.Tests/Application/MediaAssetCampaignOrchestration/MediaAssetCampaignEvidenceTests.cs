using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaAssetCampaignOrchestration;

public sealed class MediaAssetCampaignEvidenceTests
{
    [Fact]
    public void EvidenceArtifactsAreDeterministicInspectableAndKeepManualGateRequired()
    {
        var first = MediaAssetCampaignTestFactory.BuildFromRepo();
        var second = MediaAssetCampaignTestFactory.BuildFromRepo();
        var artifactNames = first.ArtifactJsonByFileName.Keys.OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(
            [
                "invalid-media-diagnostics-matrix.json",
                "media-binding-manifest.json",
                "media-campaign-source-manifest.json",
                "media-candidate-quarantine.json",
                "media-fixture-file-inventory.json",
                "media-license-provenance-ledger.json",
                "media-request-queue.json",
                "media-review-promotion-ledger.json",
                "media-slot-catalog.json",
                "media-style-policy.json",
                "preview-export-media-payloads.json"
            ],
            artifactNames);

        Assert.Equal("GREEN", first.Report.ImplementationStatus);
        Assert.False(first.Report.Accepted);
        Assert.False(first.SourceManifest.Accepted);
        Assert.True(first.Report.Goal047AcceptedByUserHandoff);
        Assert.True(first.Report.FixtureMediaProduced);
        Assert.False(first.Report.RealProviderCalled);
        Assert.False(first.Report.RealMediaGenerationCalled);
        Assert.False(first.Report.NetworkOrImportCalled);
        Assert.False(first.Report.GamePackageSchemaChanged);
        Assert.False(first.Report.RuntimeUiUnityChanged);
        Assert.DoesNotContain(first.Report.Diagnostics, item => item.Severity is "error" or "critical");
        Assert.Contains("implementationStatus=GREEN", first.ReportMarkdown);
        Assert.Contains("accepted=false", first.ReportMarkdown);
        Assert.Contains("media_asset_campaign_orchestration_verification required", first.ReportMarkdown);
        Assert.Contains("realProviderCalled=false", first.ReportMarkdown);
        Assert.Contains("realMediaGenerationCalled=false", first.ReportMarkdown);
    }

    [Fact]
    public void SourceManifestRecordsGoal047AcceptedAndPreservesGoal031AndGoal032()
    {
        var manifest = MediaAssetCampaignTestFactory.BuildFromRepo().SourceManifest;

        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "full_generator_without_media_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "semantic_pack_composition_blueprint_verification"
            && item.Status == "produced_for_review_not_passed");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "dynamic_semantic_feature_system_verification"
            && item.Status == "produced_for_review_not_passed");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "media_asset_campaign_orchestration_verification"
            && item.Status == "required");
    }
}
