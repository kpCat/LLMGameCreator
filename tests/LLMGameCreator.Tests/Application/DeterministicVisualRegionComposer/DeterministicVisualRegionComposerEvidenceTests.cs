using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualRegionComposer;

public sealed class DeterministicVisualRegionComposerEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsGateExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualRegionComposerEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.DefinitionJson, second.DefinitionJson);
        Assert.Equal(first.PatchPlacementIndexJson, second.PatchPlacementIndexJson);
        Assert.Equal(first.ChunkIndexJson, second.ChunkIndexJson);
        Assert.Equal(first.BiomeDistributionProofJson, second.BiomeDistributionProofJson);
        Assert.Equal(first.WaterNetworkProofJson, second.WaterNetworkProofJson);
        Assert.Equal(first.RoadReachabilityProofJson, second.RoadReachabilityProofJson);
        Assert.Equal(first.LayerTransitionProofJson, second.LayerTransitionProofJson);
        Assert.Equal(first.ObjectPlacementProofJson, second.ObjectPlacementProofJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.SourceLineageJson, second.SourceLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.Equal(first.OverviewSvgByFileName, second.OverviewSvgByFileName);

        Assert.Equal(144, first.Definition.Width);
        Assert.Equal(144, first.Definition.Height);
        Assert.Equal(2, first.Definition.LayerCount);
        Assert.Equal(108, first.PatchPlacementIndex.PatchPlacementCount);
        Assert.Equal(54, first.PatchPlacementIndex.SurfacePatchPlacementCount);
        Assert.Equal(54, first.PatchPlacementIndex.UndergroundPatchPlacementCount);
        Assert.Equal(41472, first.PatchPlacementIndex.DerivedLogicalCellCount);
        Assert.False(first.Definition.HeavyRawCellMode);
        Assert.Equal(0, first.Definition.ExplicitRawCellRecordCount);
        Assert.True(first.PatchPlacementIndex.AllPatchIdsKnownGoal087);
        Assert.True(first.ChunkIndex.Passed);
        Assert.Equal(108, first.ChunkIndex.ChunkCount);
        Assert.True(first.BiomeDistributionProof.Passed);
        Assert.True(first.WaterNetworkProof.Passed);
        Assert.True(first.RoadReachabilityProof.Passed);
        Assert.True(first.LayerTransitionProof.Passed);
        Assert.True(first.ObjectPlacementProof.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.SourceLineage.Passed);
        Assert.True(first.QualityGateScan.DimensionsPassed);
        Assert.True(first.QualityGateScan.PatchPlacementCountPassed);
        Assert.True(first.QualityGateScan.CompactArtifactsPassed);
        Assert.True(first.QualityGateScan.SafeSvgOverviewsPassed);
        Assert.True(first.QualityGateScan.NoRuntimeUnityProviderSchemaProjectDependencyChanges);
        Assert.True(first.QualityGateScan.NoBinaryOrRasterMediaAdded);
        Assert.False(first.Report.Accepted);
        Assert.Equal(DeterministicVisualRegionComposerVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Contains("deterministic_visual_region_composer_verification required", first.ReportMarkdown);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualRegionComposerEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

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
        Assert.Equal(3, write.OverviewSvgPaths.Count);
        Assert.All(write.OverviewSvgPaths, path => Assert.True(File.Exists(path), path));

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

        Assert.Equal(144, definition.RootElement.GetProperty("width").GetInt32());
        Assert.Equal(144, definition.RootElement.GetProperty("height").GetInt32());
        Assert.Equal(41472, definition.RootElement.GetProperty("derivedLogicalCellCount").GetInt32());
        Assert.Equal(0, definition.RootElement.GetProperty("explicitRawCellRecordCount").GetInt32());
        Assert.Equal(108, placements.RootElement.GetProperty("patchPlacementCount").GetInt32());
        Assert.Equal(108, chunks.RootElement.GetProperty("chunkCount").GetInt32());
        Assert.True(biome.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(roads.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(gates.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(objects.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("safeSvgOverviewsPassed").GetBoolean());

        AssertSafeSvg(Path.Combine(write.OutputDirectoryPath, DeterministicVisualRegionComposerEvidenceService.SurfaceOverviewSvgFileName), 54);
        AssertSafeSvg(Path.Combine(write.OutputDirectoryPath, DeterministicVisualRegionComposerEvidenceService.UndergroundOverviewSvgFileName), 54);
        AssertSafeSvg(Path.Combine(write.OutputDirectoryPath, DeterministicVisualRegionComposerEvidenceService.CombinedOverviewSvgFileName), 108);
    }

    private static void AssertSafeSvg(string path, int minRectCount)
    {
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualRegionComposerValidator.CountSvgRects(svg) >= minRectCount);
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

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
