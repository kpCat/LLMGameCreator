using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class DeterministicVisualMapPatchComposerProductSmokeTests
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
    public async Task Goal087DeterministicVisualMapPatchComposerProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new DeterministicVisualMapPatchComposerEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.MaterializationManifestJson, second.MaterializationManifestJson);
        Assert.Equal(first.FileLedgerJson, second.FileLedgerJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.MaterializationManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.WaterFlowProofJsonPath));
        Assert.True(File.Exists(write.ReachabilityProofJsonPath));
        Assert.True(File.Exists(write.LayeringProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.MaterializationManifestJsonPath));
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(write.FileLedgerJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterFlowProofJsonPath));
        using var reachability = JsonDocument.Parse(await File.ReadAllTextAsync(write.ReachabilityProofJsonPath));
        using var layering = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayeringProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(3, catalog.RootElement.GetProperty("patchCount").GetInt32());
        Assert.Equal(3, manifest.RootElement.GetProperty("patchCount").GetInt32());
        Assert.Equal(11, ledger.RootElement.GetProperty("fileCount").GetInt32());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("seaCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("coastCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("riverCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("lakeCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("marshCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("bridgeCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("dockCovered").GetBoolean());
        Assert.True(reachability.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(layering.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("patchCountPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("deterministicRerunStable").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("svgTextOnlyPreviews").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("allReferencesKnownGoal086Microtiles").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noProviderCalls").GetBoolean());

        var patches = catalog.RootElement.GetProperty("patches").EnumerateArray().ToList();
        Assert.Equal(3, patches.Count);
        Assert.Contains(patches, item => item.GetProperty("patchId").GetString() == "heroes_like_overworld_24x16");
        Assert.Contains(patches, item => item.GetProperty("patchId").GetString() == "water_coast_river_lake_marsh_24x16");
        Assert.Contains(patches, item => item.GetProperty("patchId").GetString() == "mixed_biome_settlement_creature_24x16");

        var ledgerEntries = ledger.RootElement.GetProperty("files").EnumerateArray().ToList();
        foreach (var patch in patches)
        {
            var relativePath = patch.GetProperty("patchSvgRelativePath").GetString() ?? string.Empty;
            var previewPath = Path.Combine(write.OutputDirectoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            AssertSafeSvg(previewPath);
            var ledgerPath = DeterministicVisualMapPatchComposerVocabulary.RelativeOutputDirectory + "/" + relativePath;
            var ledgerEntry = ledgerEntries.Single(item => item.GetProperty("relativePath").GetString() == ledgerPath);
            var svg = await File.ReadAllTextAsync(previewPath);
            Assert.Equal(DeterministicVisualMapPatchComposerHash.Compute(svg), ledgerEntry.GetProperty("sha256").GetString());
            Assert.Equal(Encoding.UTF8.GetByteCount(svg), ledgerEntry.GetProperty("byteLength").GetInt32());
        }

        var negativeIds = negative.RootElement.GetProperty("scenarios").EnumerateArray()
            .ToDictionary(item => item.GetProperty("scenarioId").GetString()!, item => item, StringComparer.Ordinal);
        Assert.False(negativeIds["prompt_text_as_source_of_truth"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["provider_candidate_treated_as_approved"].GetProperty("actualValid").GetBoolean());
        Assert.False(negativeIds["svg_with_script_external_resource_base64"].GetProperty("actualValid").GetBoolean());

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => MediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);
        Assert.Equal(3, Directory.EnumerateFiles(write.PatchDirectoryPath, "*.svg").Count());
        Assert.Contains("deterministic_visual_map_patch_composer_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
    }

    private static void AssertSafeSvg(string path)
    {
        Assert.True(File.Exists(path), path);
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=\"0 0 288 192\"", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualMapPatchComposerValidator.CountSvgRects(svg) >= 24 * 16);
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
