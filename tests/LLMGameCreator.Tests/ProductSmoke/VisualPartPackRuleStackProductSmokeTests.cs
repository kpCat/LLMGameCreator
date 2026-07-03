using System.Text.Json;
using LLMGameCreator.Application.Design.VisualPartPackRuleStack;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisualPartPackRuleStackProductSmokeTests
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
    public async Task Goal085VisualPartPackRuleStackProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new VisualPartPackRuleStackEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.ValidationMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.DeepsearchLineageJsonPath));
        Assert.True(File.Exists(write.Goal084BindingMatrixJsonPath));
        Assert.True(File.Exists(write.WaterBiomeCoverageMatrixJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var validation = JsonDocument.Parse(await File.ReadAllTextAsync(write.ValidationMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var deepsearch = JsonDocument.Parse(await File.ReadAllTextAsync(write.DeepsearchLineageJsonPath));
        using var goal084 = JsonDocument.Parse(await File.ReadAllTextAsync(write.Goal084BindingMatrixJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterBiomeCoverageMatrixJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var fixtureIds = catalog.RootElement.GetProperty("fixturePackIds").EnumerateArray().Select(item => item.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("fantasy_overworld_tile_part_pack", fixtureIds);
        Assert.Contains("water_coast_river_marsh_part_pack", fixtureIds);
        Assert.Contains("settlement_building_facade_part_pack", fixtureIds);
        Assert.Contains("creature_bodyplan_equipment_part_pack", fixtureIds);
        Assert.Contains("ui_theme_icon_effect_part_pack", fixtureIds);
        Assert.Contains("adult_rating_gated_extension_metadata_only", fixtureIds);

        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(deepsearch.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(8, deepsearch.RootElement.GetProperty("documentCount").GetInt32());
        Assert.True(goal084.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("coastCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("riverCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("lakeCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("marshCovered").GetBoolean());

        var creaturePack = catalog.RootElement.GetProperty("manifest").GetProperty("partPacks").EnumerateArray()
            .Single(item => item.GetProperty("packId").GetString() == "creature_bodyplan_equipment_part_pack");
        Assert.True(creaturePack.GetProperty("bodyPlanGrammarCapacity").GetInt32() >= 100);
        Assert.Equal(0, creaturePack.GetProperty("handAuthoredSpeciesAssetCount").GetInt32());
        Assert.True(creaturePack.GetProperty("equipmentOverlayProfiles").GetArrayLength() >= 4);

        var negativeIds = negative.RootElement.GetProperty("scenarios").EnumerateArray()
            .ToDictionary(item => item.GetProperty("scenarioId").GetString()!, item => item, StringComparer.Ordinal);
        Assert.False(negativeIds["water_without_coast_river_lake"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["provider_candidate_treated_as_approved"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["prompt_text_as_source_of_truth"].GetProperty("actualValid").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noImagesMediaBinaryAssetsAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("adultMetadataOnlyFallbackBound").GetBoolean());

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => MediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);
        Assert.Contains("visual_part_pack_rule_stack_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
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
