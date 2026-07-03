using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class DeterministicVisualMicrotileMaterializerProductSmokeTests
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
    public async Task Goal086DeterministicVisualMicrotileMaterializerProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new DeterministicVisualMicrotileMaterializerEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.Equal(first.PreviewCatalogJson, second.PreviewCatalogJson);
        Assert.Equal(first.MaterializationManifestJson, second.MaterializationManifestJson);
        Assert.Equal(first.FileLedgerJson, second.FileLedgerJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.PreviewCatalogJsonPath));
        Assert.True(File.Exists(write.MaterializationManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.WaterBiomeProofJsonPath));
        Assert.True(File.Exists(write.LayeringProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.PreviewCatalogJsonPath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.MaterializationManifestJsonPath));
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(write.FileLedgerJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterBiomeProofJsonPath));
        using var layering = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayeringProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));

        Assert.Equal(24, catalog.RootElement.GetProperty("previewCount").GetInt32());
        Assert.Equal(24, manifest.RootElement.GetProperty("previewCount").GetInt32());
        Assert.Equal(31, ledger.RootElement.GetProperty("fileCount").GetInt32());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("grassOverworldCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("snowCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("desertDryCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("lavaAshCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("forestOverlayCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("mountainRockCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("waterBaseCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("coastTransitionCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("riverSegmentCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("lakeEdgeCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("marshSwampCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("bridgeDockAnchorMetadataCovered").GetBoolean());
        Assert.True(layering.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("previewCountWithinBounds").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("svgTextOnlyPreviews").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("deterministicRerunStable").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("creatureEquipmentStateCoveragePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("uiEffectWeatherCoveragePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("adultMetadataOnlyFallbackCoveragePassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noProviderCalls").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("goal084ArtifactsGreen").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("goal084AcceptedFalse").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("goal085ArtifactsGreen").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("goal085AcceptedFalse").GetBoolean());

        var previews = catalog.RootElement.GetProperty("previews").EnumerateArray().ToList();
        Assert.Equal(24, previews.Count);
        Assert.Contains(previews, item => item.GetProperty("previewId").GetString() == "terrain_grass_overworld");
        Assert.Contains(previews, item => item.GetProperty("previewId").GetString() == "water_coast_transition");
        Assert.Contains(previews, item => item.GetProperty("previewId").GetString() == "water_river_segment");
        Assert.Contains(previews, item => item.GetProperty("previewId").GetString() == "creature_equipment_clothing_overlay");
        Assert.Contains(previews, item => item.GetProperty("previewId").GetString() == "ui_frame_panel_motif");
        Assert.Contains(previews, item => item.GetProperty("previewId").GetString() == "adult_metadata_only_safe_fallback_slot" && item.GetProperty("adultMetadataOnly").GetBoolean());

        var ledgerEntries = ledger.RootElement.GetProperty("files").EnumerateArray().ToList();
        foreach (var preview in previews)
        {
            var relativePath = preview.GetProperty("previewRelativePath").GetString() ?? string.Empty;
            var previewPath = Path.Combine(write.OutputDirectoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            AssertSafeSvg(previewPath);
            var ledgerPath = DeterministicVisualMicrotileMaterializerVocabulary.RelativeOutputDirectory + "/" + relativePath;
            var ledgerEntry = ledgerEntries.Single(item => item.GetProperty("relativePath").GetString() == ledgerPath);
            var svg = await File.ReadAllTextAsync(previewPath);
            Assert.Equal(DeterministicVisualMicrotileMaterializerHash.Compute(svg), ledgerEntry.GetProperty("sha256").GetString());
            Assert.Equal(Encoding.UTF8.GetByteCount(svg), ledgerEntry.GetProperty("byteLength").GetInt32());
        }

        var negativeIds = negative.RootElement.GetProperty("scenarios").EnumerateArray()
            .ToDictionary(item => item.GetProperty("scenarioId").GetString()!, item => item, StringComparer.Ordinal);
        Assert.False(negativeIds["prompt_text_as_source_of_truth"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["provider_candidate_treated_as_approved_output"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["svg_with_script_external_resource_base64"].GetProperty("actualValid").GetBoolean());

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => MediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);
        Assert.Contains("deterministic_visual_microtile_materializer_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
    }

    private static void AssertSafeSvg(string path)
    {
        Assert.True(File.Exists(path), path);
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=\"0 0 64 64\"", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualMicrotileMaterializerValidator.CountGeneratedShapes(svg) >= 4);
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
