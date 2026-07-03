using System.Text.Json;
using LLMGameCreator.Application.Design.VisualChunkCacheExportContract;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisualChunkCacheExportContractProductSmokeTests
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
    public async Task Goal093VisualChunkCacheExportContractProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new VisualChunkCacheExportEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(repoRoot, projectRoot);

        Assert.Equal(first.ManifestJson, second.ManifestJson);
        Assert.Equal(first.RuntimeHandoffSidecarJson, second.RuntimeHandoffSidecarJson);
        Assert.Equal(first.ReadbackProofJson, second.ReadbackProofJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.RuntimeHandoffSidecarJsonPath));
        Assert.True(File.Exists(write.InvalidationMatrixJsonPath));
        Assert.True(File.Exists(write.ReadbackProofJsonPath));
        Assert.True(File.Exists(write.OverlapReuseProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.ManifestJsonPath));
        using var sidecar = JsonDocument.Parse(await File.ReadAllTextAsync(write.RuntimeHandoffSidecarJsonPath));
        using var readback = JsonDocument.Parse(await File.ReadAllTextAsync(write.ReadbackProofJsonPath));
        using var overlap = JsonDocument.Parse(await File.ReadAllTextAsync(write.OverlapReuseProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var packages = manifest.RootElement.GetProperty("packages").EnumerateArray().ToArray();
        Assert.Contains(packages, item => item.GetProperty("packageId").GetString() == VisualChunkCacheExportContractVocabulary.FinitePackageId);
        Assert.Contains(packages, item => item.GetProperty("packageId").GetString() == VisualChunkCacheExportContractVocabulary.HugeSparsePackageId);
        Assert.Contains(packages, item => item.GetProperty("packageId").GetString() == VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId);
        Assert.Contains(packages, item => item.GetProperty("packageId").GetString() == VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId);

        var huge = packages.Single(item => item.GetProperty("packageId").GetString() == VisualChunkCacheExportContractVocabulary.HugeSparsePackageId);
        Assert.True(huge.GetProperty("estimatedFullWorldChunkCapacity").GetInt64() > huge.GetProperty("exportedRecordCount").GetInt32());
        Assert.True(huge.GetProperty("onlyMaterializedChunksExported").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noRawFullWorldDump").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noPromptDumps").GetBoolean());

        Assert.True(sidecar.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.False(sidecar.RootElement.GetProperty("containsRuntimeExecution").GetBoolean());
        Assert.False(sidecar.RootElement.GetProperty("containsProviderCalls").GetBoolean());
        Assert.False(sidecar.RootElement.GetProperty("containsUnityImplementation").GetBoolean());
        Assert.Equal(VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId, sidecar.RootElement.GetProperty("packageId").GetString());

        Assert.True(readback.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(overlap.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(overlap.RootElement.GetProperty("exportReusedChunkKeyCount").GetInt32() > 0);
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("runtimeHandoffSidecarMetadataOnly").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnityProviderSchemaProjectDependencyChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());

        var expectedPrefixes = quality.RootElement
            .GetProperty("expectedChangedPathPrefixes")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("unity/", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(expectedPrefixes, item => item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(expectedPrefixes, item => item.StartsWith("generator-library/", StringComparison.Ordinal));

        var files = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories).ToList();
        Assert.DoesNotContain(files, path => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(files, path => Path.GetFileName(path).Contains("prompt", StringComparison.OrdinalIgnoreCase));

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("visual_chunk_cache_export_contract_verification required", report);
        Assert.Contains(VisualChunkCacheExportContractVocabulary.FinitePackageId, report);
        Assert.Contains(VisualChunkCacheExportContractVocabulary.HugeSparsePackageId, report);
        Assert.Contains(VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId, report);
        Assert.Contains(VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId, report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
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
