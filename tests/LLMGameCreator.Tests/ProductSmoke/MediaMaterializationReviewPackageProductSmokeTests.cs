using System.Text.Json;
using LLMGameCreator.Application.Design.MediaMaterializationReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class MediaMaterializationReviewPackageProductSmokeTests
{
    [Fact]
    public async Task MediaMaterializationReviewPackageProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new MediaMaterializationReviewPackageEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(outputRoot, result);

        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.QueueJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.InventoryJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.LicenseLedgerJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.BindingValidationJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.ReviewPackageManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.PreviewExportPayloadsJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageBuilder.FamilySmokeFileName("map_panel_rpg"));
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageBuilder.FamilySmokeFileName("survival_sandbox"));
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageBuilder.FamilySmokeFileName("first_person_grid_dungeon"));
        AssertFile(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.InvalidMatrixJsonFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var sourceManifest = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.SourceManifestJsonFileName);
        using var queue = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.QueueJsonFileName);
        using var inventory = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.InventoryJsonFileName);
        using var ledger = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.LicenseLedgerJsonFileName);
        using var validation = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.BindingValidationJsonFileName);
        using var manifest = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.ReviewPackageManifestJsonFileName);
        using var payloads = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.PreviewExportPayloadsJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, MediaMaterializationReviewPackageEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal053AcceptedByUserHandoff").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal053ReportKeptRequired").GetBoolean());
        Assert.Equal(15, queue.RootElement.GetProperty("queueItemCount").GetInt32());
        Assert.True(queue.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(15, inventory.RootElement.GetProperty("fileCount").GetInt32());
        Assert.True(inventory.RootElement.GetProperty("pngFileCount").GetInt32() >= 9);
        Assert.Equal(3, inventory.RootElement.GetProperty("wavFileCount").GetInt32());
        Assert.True(ledger.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(payloads.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("implementationStatus=GREEN", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("media_materialization_review_package_verification required", report);
        Assert.Contains("providerNetworkLlmRagCalled=false", report);

        foreach (var media in result.MaterializedMediaInventory.Files)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, media.RelativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing media file: " + media.RelativePath);
        }
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
