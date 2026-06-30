using System.Text.Json;
using LLMGameCreator.Application.Design.MediaBoundPlayableReviewPackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class MediaBoundPlayableReviewPackageProductSmokeTests
{
    [Fact]
    public async Task MediaBoundPlayableReviewPackageProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var outputRoot = ResolveOutputFolder(repoRoot);
        var service = new MediaBoundPlayableReviewPackageEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(outputRoot, result);

        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.SourceManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.ReviewPackageManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.StreamingAssetsManifestJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.PreviewPayloadsJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.UnityLoadContractJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageBuilder.UnityProofFileName("map_panel_rpg"));
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageBuilder.UnityProofFileName("survival_sandbox"));
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageBuilder.UnityProofFileName("first_person_grid_dungeon"));
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.FamilySmokeMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.InvalidMatrixJsonFileName);
        AssertFile(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.ArtifactScopeReportJsonFileName);
        Assert.True(File.Exists(write.ReportMarkdownPath));
        AssertFile(write.OutputDirectoryPath, "review-package/README.md");
        AssertFile(write.OutputDirectoryPath, "review-package/CHECKLIST.md");
        AssertFile(write.OutputDirectoryPath, "review-package/media-bound-playable-manifest.json");
        AssertFile(write.OutputDirectoryPath, "review-package/StreamingAssets/LLMGameCreatorAlpha/media-bound/media-bound-playable-manifest.json");

        using var sourceManifest = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.SourceManifestJsonFileName);
        using var reviewPackage = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.ReviewPackageManifestJsonFileName);
        using var streamingManifest = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.StreamingAssetsManifestJsonFileName);
        using var previewPayloads = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.PreviewPayloadsJsonFileName);
        using var unityContract = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.UnityLoadContractJsonFileName);
        using var familySmoke = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.FamilySmokeMatrixJsonFileName);
        using var invalid = Parse(write.OutputDirectoryPath, MediaBoundPlayableReviewPackageEvidenceService.InvalidMatrixJsonFileName);
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.False(sourceManifest.RootElement.GetProperty("accepted").GetBoolean());
        Assert.True(sourceManifest.RootElement.GetProperty("goal054AcceptedByUserHandoff").GetBoolean());
        Assert.True(reviewPackage.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(15, reviewPackage.RootElement.GetProperty("stagedFileCount").GetInt32());
        Assert.Equal(9, reviewPackage.RootElement.GetProperty("pngFileCount").GetInt32());
        Assert.Equal(3, reviewPackage.RootElement.GetProperty("wavFileCount").GetInt32());
        Assert.Equal(3, reviewPackage.RootElement.GetProperty("bundleJsonFileCount").GetInt32());
        Assert.True(streamingManifest.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(previewPayloads.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(unityContract.RootElement.GetProperty("passed").GetBoolean());
        Assert.False(unityContract.RootElement.GetProperty("unityBuildOrPlayerExecuted").GetBoolean());
        Assert.True(familySmoke.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("implementationStatus=GREEN", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("media_bound_playable_review_package_verification required", report);
        Assert.Contains("providerCalls=false", report);
        Assert.Contains("networkImports=false", report);
        Assert.Contains("llmCalls=false", report);
        Assert.Contains("luaExecuted=false", report);
        Assert.Contains("publicGamePackageSchemaChanged=false", report);

        foreach (var media in result.ReviewPackageManifest.StagedFiles)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, media.StagedRelativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing staged media file: " + media.StagedRelativePath);
        }
    }

    private static void AssertFile(string directoryPath, string relativePath) =>
        Assert.True(File.Exists(Path.Combine(directoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar))), "Missing evidence file: " + relativePath);

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
