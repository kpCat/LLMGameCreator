using System.Text.Json;
using LLMGameCreator.Application.Design.MediaAssetCampaignOrchestration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class MediaAssetCampaignOrchestrationProductSmokeTests
{
    [Fact]
    public async Task MediaAssetCampaignOrchestrationProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new MediaAssetCampaignEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(outputRoot, result);

        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.SlotCatalogJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.RequestQueueJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.StylePolicyJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.LicenseProvenanceLedgerJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.CandidateQuarantineJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.ReviewPromotionLedgerJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.BindingManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.FixtureInventoryJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.PreviewExportMediaPayloadsJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.InvalidMatrixJsonFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var sourceManifest = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.SourceManifestJsonFileName);
        using var requestQueue = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.RequestQueueJsonFileName);
        using var ledger = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.LicenseProvenanceLedgerJsonFileName);
        using var review = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.ReviewPromotionLedgerJsonFileName);
        using var fixtures = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.FixtureInventoryJsonFileName);
        using var bindings = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.BindingManifestJsonFileName);
        using var payloads = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.PreviewExportMediaPayloadsJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, MediaAssetCampaignEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal(3, sourceManifest.RootElement.GetProperty("selectedFamilyIds").GetArrayLength());
        Assert.True(requestQueue.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(requestQueue.RootElement.GetProperty("requestCount").GetInt32() >= 30);
        Assert.True(ledger.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(review.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(fixtures.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(fixtures.RootElement.GetProperty("fixtureFileCount").GetInt32() > 0);
        Assert.True(bindings.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(payloads.RootElement.GetProperty("passed").GetBoolean());
        Assert.False(payloads.RootElement.GetProperty("gamePackageSchemaChanged").GetBoolean());
        Assert.False(payloads.RootElement.GetProperty("unityExportModified").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("implementationStatus=GREEN", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("realProviderCalled=false", report);
        Assert.Contains("realMediaGenerationCalled=false", report);
        Assert.Contains("media_asset_campaign_orchestration_verification required", report);
    }

    private static void AssertFile(string directoryPath, string fileName) =>
        Assert.True(File.Exists(Path.Combine(directoryPath, fileName)), "Missing evidence file: " + fileName);

    private static JsonDocument Parse(string directoryPath, string fileName) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(directoryPath, fileName)));

    private static string ResolveOutputFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var outputFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(outputFolder);
        return outputFolder;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
