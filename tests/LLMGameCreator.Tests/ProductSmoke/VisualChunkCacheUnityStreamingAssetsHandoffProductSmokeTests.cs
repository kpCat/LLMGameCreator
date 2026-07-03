using System.Text.Json;
using LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisualChunkCacheUnityStreamingAssetsHandoffProductSmokeTests
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
    public async Task Goal095BuildsReadsAndGuardsUnityStreamingAssetsHandoff()
    {
        var service = new VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService();
        var write = await service.BuildAndWriteAsync(ProjectRoot());
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.FinalGate, result.Report.ManualGate);
        Assert.True(result.StreamingAssetsLedger.Passed);
        Assert.True(result.SimulatedReadProof.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.True(result.QualityGateScan.Passed);

        var manifest = ReadPayload<VisualChunkCacheUnityHandoffManifest>(
            write.StreamingAssetsDirectoryPath,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.HandoffManifestFileName);
        var packages = ReadPayload<VisualChunkCacheUnityPackageIndex>(
            write.StreamingAssetsDirectoryPath,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.PackageIndexFileName);
        var windows = ReadPayload<VisualChunkCacheUnityStreamWindowIndex>(
            write.StreamingAssetsDirectoryPath,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.StreamWindowIndexFileName);
        var ledger = ReadPayload<VisualChunkCacheUnityChunkKeyLedger>(
            write.StreamingAssetsDirectoryPath,
            VisualChunkCacheUnityStreamingAssetsHandoffEvidenceService.ChunkKeyLedgerFileName);

        Assert.Equal(5, manifest.PayloadFileCount);
        Assert.Equal(4, manifest.PackageCount);
        Assert.Equal(93, manifest.ExportRecordCount);
        Assert.Equal(117, manifest.SourceMaterializedChunkCount);
        Assert.Equal(5, manifest.StreamWindowCount);
        Assert.Equal(93, manifest.UniqueChunkKeyCount);
        Assert.True(manifest.RuntimeHandoffSidecarMetadataOnly);
        Assert.True(manifest.NoRawFullWorldDump);
        Assert.True(manifest.NoAbsolutePaths);
        Assert.True(manifest.NoBinaryOrRasterMedia);
        Assert.Equal(manifest.PackageCount, packages.PackageCount);
        Assert.Equal(manifest.ExportRecordCount, packages.ExportRecordCount);
        Assert.Equal(manifest.StreamWindowCount, windows.StreamWindowCount);
        Assert.Equal(manifest.UniqueChunkKeyCount, ledger.UniqueChunkKeyCount);
        Assert.Equal(manifest.ExportRecordCount, ledger.ExportRecordCount);
        Assert.Contains(packages.Packages, item => item.PackageId == "layer_transition_runtime_handoff_sidecar");
        Assert.All(packages.Packages, item => Assert.True(item.MetadataOnly));
        Assert.All(ledger.Entries, item => Assert.Equal(64, item.ChunkKey.Length));

        AssertRejected(result, "missing_manifest");
        AssertRejected(result, "tampered_manifest_hash");
        AssertRejected(result, "missing_package_index");
        AssertRejected(result, "stream_window_count_mismatch");
        AssertRejected(result, "chunk_key_ledger_mismatch");
        AssertRejected(result, "absolute_path_in_payload");
        AssertRejected(result, "raw_full_world_dump_marker");
        AssertRejected(result, "provider_call_marker");
        AssertRejected(result, "fake_success_without_file_read");

        var files = Directory.EnumerateFiles(write.StreamingAssetsDirectoryPath, "*", SearchOption.AllDirectories).ToList();
        Assert.DoesNotContain(files, path => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(files, path => Path.GetFileName(path).Contains("prompt", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item => item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item => item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item => item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item => item.StartsWith("generator-library/", StringComparison.Ordinal));
    }

    private static void AssertRejected(
        VisualChunkCacheUnityBuildResult result,
        string scenarioId)
    {
        Assert.Contains(
            result.NegativeProof.Scenarios,
            scenario => scenario.ScenarioId == scenarioId
                        && scenario.ActualStatus == "rejected"
                        && scenario.Diagnostics.Count > 0);
    }

    private static T ReadPayload<T>(string root, string fileName)
    {
        var value = JsonSerializer.Deserialize<T>(
            File.ReadAllText(Path.Combine(root, fileName)),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        Assert.NotNull(value);
        return value!;
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
