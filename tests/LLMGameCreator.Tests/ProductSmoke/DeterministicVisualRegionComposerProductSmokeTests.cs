using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class DeterministicVisualRegionComposerProductSmokeTests
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
    public async Task Goal088DeterministicVisualRegionComposerProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new DeterministicVisualRegionComposerEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.Equal(first.DefinitionJson, second.DefinitionJson);
        Assert.Equal(first.PatchPlacementIndexJson, second.PatchPlacementIndexJson);
        Assert.Equal(first.ChunkIndexJson, second.ChunkIndexJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.DefinitionJsonPath));
        Assert.True(File.Exists(write.PatchPlacementIndexJsonPath));
        Assert.True(File.Exists(write.ChunkIndexJsonPath));
        Assert.True(File.Exists(write.BiomeDistributionProofJsonPath));
        Assert.True(File.Exists(write.WaterNetworkProofJsonPath));
        Assert.True(File.Exists(write.RoadReachabilityProofJsonPath));
        Assert.True(File.Exists(write.LayerTransitionProofJsonPath));
        Assert.True(File.Exists(write.ObjectPlacementProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var definition = JsonDocument.Parse(await File.ReadAllTextAsync(write.DefinitionJsonPath));
        using var placements = JsonDocument.Parse(await File.ReadAllTextAsync(write.PatchPlacementIndexJsonPath));
        using var chunks = JsonDocument.Parse(await File.ReadAllTextAsync(write.ChunkIndexJsonPath));
        using var biome = JsonDocument.Parse(await File.ReadAllTextAsync(write.BiomeDistributionProofJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterNetworkProofJsonPath));
        using var roads = JsonDocument.Parse(await File.ReadAllTextAsync(write.RoadReachabilityProofJsonPath));
        using var gates = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayerTransitionProofJsonPath));
        using var objects = JsonDocument.Parse(await File.ReadAllTextAsync(write.ObjectPlacementProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal("heroes_scale_surface_underground_144x144", definition.RootElement.GetProperty("regionId").GetString());
        Assert.Equal(144, definition.RootElement.GetProperty("width").GetInt32());
        Assert.Equal(144, definition.RootElement.GetProperty("height").GetInt32());
        Assert.Equal(2, definition.RootElement.GetProperty("layerCount").GetInt32());
        Assert.Equal(41472, definition.RootElement.GetProperty("derivedLogicalCellCount").GetInt32());
        Assert.False(definition.RootElement.GetProperty("heavyRawCellMode").GetBoolean());
        Assert.Equal(0, definition.RootElement.GetProperty("explicitRawCellRecordCount").GetInt32());

        Assert.True(placements.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(108, placements.RootElement.GetProperty("patchPlacementCount").GetInt32());
        Assert.Equal(54, placements.RootElement.GetProperty("surfacePatchPlacementCount").GetInt32());
        Assert.Equal(54, placements.RootElement.GetProperty("undergroundPatchPlacementCount").GetInt32());
        Assert.True(placements.RootElement.GetProperty("allPatchIdsKnownGoal087").GetBoolean());
        Assert.True(chunks.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(108, chunks.RootElement.GetProperty("chunkCount").GetInt32());
        Assert.True(biome.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("seaCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("coastCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("riverCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("lakeCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("marshCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("bridgeCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("dockCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("undergroundWaterCovered").GetBoolean());
        Assert.True(water.RootElement.GetProperty("lavaBoundaryMetadataCovered").GetBoolean());
        Assert.True(roads.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(gates.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(objects.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("dimensionsPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("compactArtifactsPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("safeSvgOverviewsPassed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnityProviderSchemaProjectDependencyChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());

        AssertSafeSvg(Path.Combine(write.OutputDirectoryPath, DeterministicVisualRegionComposerEvidenceService.SurfaceOverviewSvgFileName), 54);
        AssertSafeSvg(Path.Combine(write.OutputDirectoryPath, DeterministicVisualRegionComposerEvidenceService.UndergroundOverviewSvgFileName), 54);
        AssertSafeSvg(Path.Combine(write.OutputDirectoryPath, DeterministicVisualRegionComposerEvidenceService.CombinedOverviewSvgFileName), 108);

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => MediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("deterministic_visual_region_composer_verification required", report);
        Assert.Contains("patchPlacementCount: 108", report);
        Assert.Contains("derivedLogicalCellCount: 41472", report);
    }

    private static void AssertSafeSvg(string path, int minRectCount)
    {
        Assert.True(File.Exists(path), path);
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualRegionComposerValidator.CountSvgRects(svg) >= minRectCount);
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
