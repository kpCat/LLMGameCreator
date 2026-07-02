using System.Text.Json;
using LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisualAssetContractRatingMetadataProductSmokeTests
{
    private static readonly HashSet<string> MediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    };

    [Fact]
    public async Task Goal084VisualAssetContractRatingMetadataProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new VisualAssetContractRatingMetadataEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.RatingPolicyMatrixJsonPath));
        Assert.True(File.Exists(write.ValidationMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceDocumentLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var policy = JsonDocument.Parse(await File.ReadAllTextAsync(write.RatingPolicyMatrixJsonPath));
        using var validation = JsonDocument.Parse(await File.ReadAllTextAsync(write.ValidationMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceDocumentLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var fixtureIds = catalog.RootElement.GetProperty("fixtureIds").EnumerateArray().Select(item => item.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("fantasy_overworld_tile_safe", fixtureIds);
        Assert.Contains("water_coast_biome_safe", fixtureIds);
        Assert.Contains("creature_bodyplan_safe", fixtureIds);
        Assert.Contains("humanoid_paperdoll_adult_capable_metadata_only", fixtureIds);
        Assert.Contains("tech_future_ui_panel_safe", fixtureIds);

        Assert.True(policy.RootElement.GetProperty("rows").GetArrayLength() >= 5);
        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noProviderOrLlmOrRagOrMediaExecution").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPublicGamePackageSchemaChanged").GetBoolean());

        var negativeIds = negative.RootElement.GetProperty("scenarios").EnumerateArray()
            .ToDictionary(item => item.GetProperty("scenarioId").GetString()!, item => item, StringComparer.Ordinal);
        Assert.False(negativeIds["adult_public_export_without_fallback"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["provider_candidate_treated_as_approved"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["prompt_text_as_source_of_truth"].GetProperty("actualValid").GetBoolean());

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => MediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);
        Assert.Contains("visual_asset_contract_rating_metadata_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
