using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.Application.MediaMaterializationReviewPackage;

public sealed class MediaMaterializationEvidenceTests
{
    [Fact]
    public void EvidenceArtifactsAreDeterministicInspectableAndKeepGoal054GateRequired()
    {
        var first = MediaMaterializationReviewPackageTestFactory.BuildFromRepo();
        var second = MediaMaterializationReviewPackageTestFactory.BuildFromRepo();
        var artifactNames = first.ArtifactJsonByFileName.Keys.OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(
            [
                "family-media-smoke-first-person-grid-dungeon.json",
                "family-media-smoke-map-panel-rpg.json",
                "family-media-smoke-survival-sandbox.json",
                "invalid-media-materialization-matrix.json",
                "materialized-media-inventory.json",
                "media-binding-validation.json",
                "media-materialization-queue.json",
                "media-provenance-license-ledger.json",
                "media-review-package-manifest.json",
                "preview-export-media-payloads.json",
                "source-manifest.json"
            ],
            artifactNames);

        Assert.Equal("GREEN", first.Report.ImplementationStatus);
        Assert.False(first.Report.Accepted);
        Assert.False(first.SourceManifest.Accepted);
        Assert.True(first.Report.Goal053AcceptedByUserHandoff);
        Assert.True(first.Report.Goal053SourceReportGreenRequired);
        Assert.True(first.Report.PhysicalMediaProduced);
        Assert.True(first.Report.PngProofPassed);
        Assert.True(first.Report.WavProofPassed);
        Assert.True(first.Report.ReviewPackageManifestPassed);
        Assert.False(first.Report.ProviderNetworkLlmRagCalled);
        Assert.False(first.Report.GamePackageSchemaChanged);
        Assert.False(first.Report.RuntimeUiUnityChanged);
        Assert.DoesNotContain(first.Report.Diagnostics, item => item.Severity is "error" or "critical");
        Assert.Contains("implementationStatus=GREEN", first.ReportMarkdown);
        Assert.Contains("accepted=false", first.ReportMarkdown);
        Assert.Contains("media_materialization_review_package_verification required", first.ReportMarkdown);
        Assert.Contains("providerNetworkLlmRagCalled=false", first.ReportMarkdown);
        Assert.Contains("gamePackageSchemaChanged=false", first.ReportMarkdown);
        Assert.Contains("runtimeUiUnityChanged=false", first.ReportMarkdown);
    }

    [Fact]
    public void SourceManifestRecordsGoal053AcceptedAndGoal054Required()
    {
        var manifest = MediaMaterializationReviewPackageTestFactory.BuildFromRepo().SourceManifest;

        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "media_asset_campaign_orchestration_verification"
            && item.Status == "passed"
            && item.ProvenanceKind == "user_handoff");
        Assert.Contains(manifest.PreflightGates, item =>
            item.GateId == "media_materialization_review_package_verification"
            && item.Status == "required");
        Assert.True(manifest.Goal053ProducedForReviewReportGreen);
        Assert.True(manifest.Goal053ReportKeptRequired);
        Assert.Equal(36, manifest.Goal053RequestCount);
        Assert.Equal(15, manifest.Goal053BindingCount);
        Assert.Equal(3, manifest.SelectedFamilyIds.Count);
    }
}
