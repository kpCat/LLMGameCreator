using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests
{
    private static readonly HashSet<string> ForbiddenOutputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
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
    public async Task Goal100OfflineGeoworldVisualCacheUnityHandoffProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldVisualCacheUnityHandoffEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(3, result.Report.PackageCount);
        Assert.Equal(10, result.Report.FeatureCount);
        Assert.Equal(18, result.Report.VisualCacheRecordCount);
        Assert.Equal(5, result.Report.SourceChunkCount);
        Assert.Equal(9, result.Report.StreamWindowChunkCount);
        Assert.Equal(5, result.Report.UnityPayloadFileCount);
        Assert.True(result.Report.SimulatedReadProofPassed);
        Assert.True(result.Report.NegativeProofPassed);
        Assert.True(result.Report.AlphaRuntimeBootstrapUnchanged);

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.HandoffManifestFileName)));
        using var packageIndex = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.PackageIndexFileName)));
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.FeatureChunkLedgerFileName)));
        using var stream = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.StreamingAssetsDirectoryPath,
                OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamWindowIndexFileName)));

        Assert.Equal(3, manifest.RootElement.GetProperty("packageCount").GetInt32());
        Assert.Equal(10, manifest.RootElement.GetProperty("featureCount").GetInt32());
        Assert.Equal(10, manifest.RootElement.GetProperty("featureKindCount").GetInt32());
        Assert.Equal(18, manifest.RootElement.GetProperty("visualCacheRecordCount").GetInt32());
        Assert.Equal(5, manifest.RootElement.GetProperty("sourceChunkCount").GetInt32());
        Assert.Equal(9, manifest.RootElement.GetProperty("streamWindowChunkCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("metadataOnly").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noRawGeodata").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noAbsolutePaths").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsRuntimeExecution").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsProviderCalls").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("containsUnityGameplayImplementation").GetBoolean());
        Assert.Equal(3, packageIndex.RootElement.GetProperty("packageCount").GetInt32());
        Assert.Equal(18, packageIndex.RootElement.GetProperty("visualCacheRecordCount").GetInt32());
        Assert.Equal(18, ledger.RootElement.GetProperty("visualCacheRecordCount").GetInt32());
        Assert.Equal(5, ledger.RootElement.GetProperty("sourceChunkCount").GetInt32());
        Assert.Equal(9, stream.RootElement.GetProperty("requiredChunkCount").GetInt32());

        var payloadFiles = Directory.EnumerateFiles(write.StreamingAssetsDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.Equal(5, payloadFiles.Length);
        Assert.DoesNotContain(payloadFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        foreach (var path in payloadFiles)
        {
            var text = await File.ReadAllTextAsync(path);
            Assert.DoesNotContain(repoRoot, text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("rawGeodataIncluded\": true", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("noRawGeodata\": false", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("publicTileScraping", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lfzCopiedCode", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("UnityWebRequest", text, StringComparison.Ordinal);
            Assert.DoesNotContain("HttpClient", text, StringComparison.Ordinal);
        }

        Assert.True(result.ProbeSourceInventory.Passed);
        Assert.True(result.ProbeSourceInventory.UsesApplicationStreamingAssetsPath);
        Assert.True(result.ProbeSourceInventory.UsesExpectedPayloadRoot);
        Assert.True(result.ProbeSourceInventory.ExposesInspectorResultFields);
        Assert.True(result.ProbeSourceInventory.DoesNotReferenceAlphaRuntimeBootstrap);
        Assert.True(result.ProbeSourceInventory.HasNoProviderLlmNetworkMarkers);
        Assert.True(result.QualityGateScan.AlphaRuntimeBootstrapUnchanged);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffGroupPresent);
        Assert.Equal(3, workspace.QualityGateScan.OfflineGeoworldHandoffPackageCount);
        Assert.Equal(18, workspace.QualityGateScan.OfflineGeoworldHandoffVisualCacheRecordCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldHandoffSimulatedReadProofPassed);
        Assert.True(workspace.QualityGateScan.Goal100FilesDiscoveredByRelativePaths);

        var outputFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.DoesNotContain(outputFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("offline_geoworld_visual_cache_unity_handoff_verification required", report);
        Assert.Contains("packageCount: 3", report);
        Assert.Contains("visualCacheRecordCount: 18", report);
        Assert.Contains("unityPayloadFileCount: 5", report);
        Assert.Contains("noNetworkOrProviderImplementation: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoRoot()
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
