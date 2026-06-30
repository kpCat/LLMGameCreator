using System.Text.Json;
using LLMGameCreator.Application.Design.FullMediaBoundGeneratorCampaign;
using Xunit;

namespace LLMGameCreator.Tests.Application.FullMediaBoundGeneratorCampaign;

public sealed class FullMediaBoundGeneratorCampaignEvidenceTests
{
    [Fact]
    public void EvidenceIsJsonParseableAndBlockedWhenUnityProofIsNotExecuted()
    {
        var result = FullMediaBoundGeneratorCampaignTestFactory.BuildFromRepo();

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.Report.Goal057AcceptedByUserHandoff);
        Assert.True(result.Report.SourceFactsConsumed);
        Assert.True(result.Report.AllFamiliesIncluded);
        Assert.True(result.Report.CampaignRunnerExecuted);
        Assert.True(result.Report.ReviewPackageManifestPassed);
        Assert.True(result.Report.InvalidMatrixPassed);
        Assert.False(result.Report.UnityEditorOrPlayerExecuted);
        Assert.False(result.Report.AllCampaignMarkersMatched);
        Assert.Contains("implementationStatus=BLOCKED", result.ReportMarkdown);
        Assert.Contains("accepted=false", result.ReportMarkdown);
        Assert.Contains("manualGate=full_media_bound_generator_campaign_verification", result.ReportMarkdown);
        Assert.Contains("full_media_bound_generator_campaign_verification required", result.ReportMarkdown);

        foreach (var fileName in new[]
        {
            FullMediaBoundGeneratorCampaignEvidenceService.SourceManifestJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.CampaignPlanJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.ReviewPackageManifestJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.UnityCommandPlanJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.UnityPlayerProofJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.PreviewExportPayloadJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.PackageCompatibilityProofJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.InvalidMatrixJsonFileName,
            FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("map_panel_rpg"),
            FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("survival_sandbox"),
            FullMediaBoundGeneratorCampaignEvidenceService.FamilyRunFileName("first_person_grid_dungeon")
        })
        {
            Assert.Contains(fileName, result.ArtifactJsonByFileName.Keys);
            using var _ = JsonDocument.Parse(result.ArtifactJsonByFileName[fileName]);
        }
    }
}
