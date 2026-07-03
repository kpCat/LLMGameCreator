using System.Text.Json;
using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class ParameterizedVisualWorldProfilesProductSmokeTests
{
    private static readonly HashSet<string> BinaryOrRasterMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
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
    public async Task Goal090ParameterizedVisualWorldProfilesProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new ParameterizedVisualWorldProfilesEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.SizeMatrixJson, second.SizeMatrixJson);
        Assert.Equal(first.ValidationMatrixJson, second.ValidationMatrixJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.SizeMatrixJsonPath));
        Assert.True(File.Exists(write.ValidationMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.ChunkAddressProofJsonPath));
        Assert.True(File.Exists(write.SparseWorldProofJsonPath));
        Assert.True(File.Exists(write.LayerModelProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var sizeMatrix = JsonDocument.Parse(await File.ReadAllTextAsync(write.SizeMatrixJsonPath));
        using var validation = JsonDocument.Parse(await File.ReadAllTextAsync(write.ValidationMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var chunkProof = JsonDocument.Parse(await File.ReadAllTextAsync(write.ChunkAddressProofJsonPath));
        using var sparseProof = JsonDocument.Parse(await File.ReadAllTextAsync(write.SparseWorldProofJsonPath));
        using var layerProof = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayerModelProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var profiles = catalog.RootElement.GetProperty("profiles").EnumerateArray().ToArray();
        Assert.Contains(profiles, item => item.GetProperty("profileId").GetString() == "finite_custom_sizes_matrix");
        Assert.Contains(profiles, item =>
            item.GetProperty("profileId").GetString() == "benchmark_heroes_144x144_surface_underground"
            && item.GetProperty("isBenchmarkProfile").GetBoolean());
        Assert.Contains(profiles, item => item.GetProperty("profileId").GetString() == "huge_sparse_100000x100000_multilayer");
        Assert.Contains(profiles, item =>
            item.GetProperty("profileId").GetString() == "infinite_streaming_world_multilayer"
            && item.GetProperty("isInfinite").GetBoolean());

        Assert.True(sizeMatrix.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains(sizeMatrix.RootElement.GetProperty("rows").EnumerateArray(), item =>
            item.GetProperty("width").GetInt32() == 255
            && item.GetProperty("height").GetInt32() == 257
            && item.GetProperty("validatorPassed").GetBoolean());
        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(chunkProof.RootElement.GetProperty("stableAcrossReruns").GetBoolean());
        Assert.True(chunkProof.RootElement.GetProperty("differsBySeedLayerChunkAndVersion").GetBoolean());
        Assert.True(sparseProof.RootElement.GetProperty("hugeSparseProfilePassed").GetBoolean());
        Assert.True(sparseProof.RootElement.GetProperty("infiniteProfilePassed").GetBoolean());
        Assert.True(layerProof.RootElement.GetProperty("notRestrictedToSurfaceUnderground").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRawHeavyCellDump").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnityProviderSchemaProjectDependencyChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());

        Assert.Equal(4, write.OverviewSvgPaths.Count);
        Assert.All(write.OverviewSvgPaths, AssertSafeSvg);

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("parameterized_visual_world_profiles_verification required", report);
        Assert.Contains("finite_custom_sizes_matrix", report);
        Assert.Contains("benchmarkMarkedAsFixtureOnly: true", report);
        Assert.Contains("huge_sparse_100000x100000_multilayer", report);
        Assert.Contains("infinite_streaming_world_multilayer", report);
    }

    private static void AssertSafeSvg(string path)
    {
        Assert.True(File.Exists(path), path);
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(ParameterizedVisualWorldProfilesValidator.CountSvgRects(svg) >= 4);
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
